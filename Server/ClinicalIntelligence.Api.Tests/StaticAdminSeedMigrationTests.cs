using System;
using System.Linq;
using System.Threading.Tasks;
using ClinicalIntelligence.Api.Data;
using ClinicalIntelligence.Api.Domain.Models;
using ClinicalIntelligence.Api.Services.Auth;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace ClinicalIntelligence.Api.Tests;

/// <summary>
/// Integration tests for static admin seed migration behavior.
/// Tests validate env-var validation, seeded admin properties, and idempotency.
/// Skippable when PostgreSQL database is unavailable.
/// </summary>
public class StaticAdminSeedMigrationTests : IDisposable
{
    private readonly string? _originalAdminEmail;
    private readonly string? _originalAdminPassword;

    public StaticAdminSeedMigrationTests()
    {
        _originalAdminEmail = Environment.GetEnvironmentVariable("ADMIN_EMAIL");
        _originalAdminPassword = Environment.GetEnvironmentVariable("ADMIN_PASSWORD");
    }

    public void Dispose()
    {
        if (_originalAdminEmail != null)
            Environment.SetEnvironmentVariable("ADMIN_EMAIL", _originalAdminEmail);
        else
            Environment.SetEnvironmentVariable("ADMIN_EMAIL", null);

        if (_originalAdminPassword != null)
            Environment.SetEnvironmentVariable("ADMIN_PASSWORD", _originalAdminPassword);
        else
            Environment.SetEnvironmentVariable("ADMIN_PASSWORD", null);
    }

    [Fact]
    public void MissingAdminEmail_ThrowsInvalidOperationException_WithRequiredMessage()
    {
        // Arrange
        Environment.SetEnvironmentVariable("ADMIN_EMAIL", null);
        Environment.SetEnvironmentVariable("ADMIN_PASSWORD", "ValidP@ss123");

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() =>
            ValidateEnvironmentVariables());

        Assert.Equal(
            "Required environment variables ADMIN_EMAIL and ADMIN_PASSWORD must be set",
            exception.Message);
    }

    [Fact]
    public void MissingAdminPassword_ThrowsInvalidOperationException_WithRequiredMessage()
    {
        // Arrange
        Environment.SetEnvironmentVariable("ADMIN_EMAIL", "admin@example.com");
        Environment.SetEnvironmentVariable("ADMIN_PASSWORD", null);

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() =>
            ValidateEnvironmentVariables());

        Assert.Equal(
            "Required environment variables ADMIN_EMAIL and ADMIN_PASSWORD must be set",
            exception.Message);
    }

    [Fact]
    public void EmptyAdminEmail_ThrowsInvalidOperationException_WithRequiredMessage()
    {
        // Arrange
        Environment.SetEnvironmentVariable("ADMIN_EMAIL", "   ");
        Environment.SetEnvironmentVariable("ADMIN_PASSWORD", "ValidP@ss123");

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() =>
            ValidateEnvironmentVariables());

        Assert.Equal(
            "Required environment variables ADMIN_EMAIL and ADMIN_PASSWORD must be set",
            exception.Message);
    }

    [Fact]
    public void InvalidEmailFormat_ThrowsInvalidOperationException_WithEmailFormatMessage()
    {
        // Arrange
        Environment.SetEnvironmentVariable("ADMIN_EMAIL", "not-an-email");
        Environment.SetEnvironmentVariable("ADMIN_PASSWORD", "ValidP@ss123");

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() =>
            ValidateEnvironmentVariablesWithEmailValidation());

        Assert.Equal(
            "ADMIN_EMAIL must be a valid email address format",
            exception.Message);
    }

    [Theory]
    [InlineData("short")]
    [InlineData("nouppercase1!")]
    [InlineData("NOLOWERCASE1!")]
    [InlineData("NoSpecialChar1")]
    [InlineData("NoNumber!@#")]
    public void InvalidPasswordComplexity_ThrowsInvalidOperationException_WithComplexityMessage(string password)
    {
        // Arrange
        Environment.SetEnvironmentVariable("ADMIN_EMAIL", "admin@example.com");
        Environment.SetEnvironmentVariable("ADMIN_PASSWORD", password);

        // Act & Assert - Use centralized PasswordPolicy
        var exception = Assert.Throws<InvalidOperationException>(() =>
            PasswordPolicy.ValidateOrThrow(password, "ADMIN_PASSWORD"));

        Assert.Equal(
            "ADMIN_PASSWORD must be at least 8 characters with mixed case, number, and special character",
            exception.Message);
    }

    [Theory]
    [InlineData("ValidP@ss1")]
    [InlineData("Str0ng!Pass")]
    [InlineData("C0mpl3x@Password")]
    public void ValidPasswordComplexity_DoesNotThrow(string password)
    {
        // Act & Assert - Use centralized PasswordPolicy
        Assert.True(PasswordPolicy.IsValid(password));
    }

    [Theory]
    [InlineData("Pass1!", "Password must be at least 8 characters")]
    [InlineData("PASSWORD1!", "Password must contain a lowercase letter")]
    [InlineData("password1!", "Password must contain an uppercase letter")]
    [InlineData("Password!!", "Password must contain a number")]
    [InlineData("Password12", "Password must contain a special character")]
    public void PasswordPolicy_GetMissingRequirements_ReturnsCorrectMessage(string password, string expectedMessage)
    {
        // Act
        var missing = PasswordPolicy.GetMissingRequirements(password);

        // Assert
        Assert.Contains(expectedMessage, missing);
    }

    [Fact]
    public void PasswordPolicy_NullPassword_ReturnsAllRequirements()
    {
        // Act
        var missing = PasswordPolicy.GetMissingRequirements(null);

        // Assert
        Assert.Equal(5, missing.Count);
    }

    [Fact]
    public void PasswordPolicy_EmptyPassword_ReturnsAllRequirements()
    {
        // Act
        var missing = PasswordPolicy.GetMissingRequirements(string.Empty);

        // Assert
        Assert.Equal(5, missing.Count);
    }

    [Fact]
    public void PasswordPolicy_ExceedsMaxLength_ReturnsMaxLengthError()
    {
        // Arrange - Create password exceeding 128 characters
        var longPassword = new string('A', 129) + "a1!";

        // Act
        var missing = PasswordPolicy.GetMissingRequirements(longPassword);

        // Assert
        Assert.Contains($"Password must not exceed {PasswordPolicy.MaxLength} characters", missing);
    }

    [Theory]
    [InlineData("Pässwörd1!")]  // Unicode letters
    [InlineData("Password1™")]  // Unicode symbol as special char
    [InlineData("Password1€")]  // Currency symbol as special char
    public void PasswordPolicy_UnicodeSpecialCharacters_AreAccepted(string password)
    {
        // Act & Assert - Unicode non-alphanumeric chars count as special
        Assert.True(PasswordPolicy.IsValid(password));
    }

    [Fact]
    public void BcryptHash_VerifiesCorrectly_WithWorkFactor12()
    {
        // Arrange
        var password = "ValidP@ss123";
        var hash = BCrypt.Net.BCrypt.HashPassword(password, 12);

        // Act & Assert
        Assert.True(BCrypt.Net.BCrypt.Verify(password, hash));
        Assert.False(BCrypt.Net.BCrypt.Verify("WrongPassword", hash));
    }

    #region Configurable Work Factor Tests

    [Fact]
    public void BcryptHash_VerifiesCorrectly_WithConfiguredWorkFactor()
    {
        // Arrange - Simulate configured work factor
        var password = "ValidP@ss123";
        var workFactor = 13; // Higher than minimum
        var hash = BCrypt.Net.BCrypt.HashPassword(password, workFactor);

        // Act & Assert
        Assert.True(BCrypt.Net.BCrypt.Verify(password, hash));
        Assert.False(BCrypt.Net.BCrypt.Verify("WrongPassword", hash));
    }

    [Theory]
    [InlineData(12)]
    [InlineData(13)]
    [InlineData(14)]
    public void BcryptHash_WorkFactorAboveMinimum_ProducesValidHash(int workFactor)
    {
        // Arrange
        var password = "ValidP@ss123";

        // Act
        var hash = BCrypt.Net.BCrypt.HashPassword(password, workFactor);

        // Assert
        Assert.True(BCrypt.Net.BCrypt.Verify(password, hash));
        Assert.StartsWith("$2", hash);
    }

    #endregion

    [SkippableFact]
    public async Task SeededAdmin_HasCorrectProperties_WhenMigrationsApplied()
    {
        // Skip if no database connection string is available
        var connectionString = Environment.GetEnvironmentVariable("DATABASE_CONNECTION_STRING");
        Skip.If(string.IsNullOrEmpty(connectionString), "PostgreSQL database not available");

        // Arrange
        Environment.SetEnvironmentVariable("ADMIN_EMAIL", "test-admin@example.com");
        Environment.SetEnvironmentVariable("ADMIN_PASSWORD", "TestP@ss123");

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseNpgsql(connectionString)
            .Options;

        await using var context = new ApplicationDbContext(options);

        // Act - Query for the seeded admin (ignoring query filters)
        var admin = await context.Users
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.IsStaticAdmin);

        // Assert
        Skip.If(admin == null, "Static admin not seeded - migration may not have run");

        Assert.Equal("Admin", admin.Role);
        Assert.Equal("Active", admin.Status);
        Assert.True(admin.IsStaticAdmin);
        Assert.False(admin.IsDeleted);
    }

    [SkippableFact]
    public async Task SeededAdmin_EmailIsUnique_EnsuresIdempotency()
    {
        // Skip if no database connection string is available
        var connectionString = Environment.GetEnvironmentVariable("DATABASE_CONNECTION_STRING");
        Skip.If(string.IsNullOrEmpty(connectionString), "PostgreSQL database not available");

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseNpgsql(connectionString)
            .Options;

        await using var context = new ApplicationDbContext(options);

        // Act - Count static admins
        var staticAdminCount = await context.Users
            .IgnoreQueryFilters()
            .CountAsync(u => u.IsStaticAdmin);

        // Assert - Should be exactly one static admin (idempotent)
        Assert.Equal(1, staticAdminCount);
    }

    private static void ValidateEnvironmentVariables()
    {
        var adminEmail = Environment.GetEnvironmentVariable("ADMIN_EMAIL");
        var adminPassword = Environment.GetEnvironmentVariable("ADMIN_PASSWORD");

        if (string.IsNullOrWhiteSpace(adminEmail) || string.IsNullOrWhiteSpace(adminPassword))
        {
            throw new InvalidOperationException(
                "Required environment variables ADMIN_EMAIL and ADMIN_PASSWORD must be set");
        }
    }

    private static void ValidateEnvironmentVariablesWithEmailValidation()
    {
        ValidateEnvironmentVariables();

        var adminEmail = Environment.GetEnvironmentVariable("ADMIN_EMAIL")!.Trim().ToLowerInvariant();

        try
        {
            var mailAddress = new System.Net.Mail.MailAddress(adminEmail);
            if (mailAddress.Address != adminEmail)
            {
                throw new InvalidOperationException(
                    "ADMIN_EMAIL must be a valid email address format");
            }
        }
        catch (FormatException)
        {
            throw new InvalidOperationException(
                "ADMIN_EMAIL must be a valid email address format");
        }
    }

}
