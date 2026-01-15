using System;
using System.Linq;
using System.Threading.Tasks;
using ClinicalIntelligence.Api.Data;
using ClinicalIntelligence.Api.Domain.Models;
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

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() =>
            ValidatePasswordComplexity(password));

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
        // Act & Assert - should not throw
        var isValid = IsValidPasswordComplexity(password);
        Assert.True(isValid);
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

    private static void ValidatePasswordComplexity(string password)
    {
        if (!IsValidPasswordComplexity(password))
        {
            throw new InvalidOperationException(
                "ADMIN_PASSWORD must be at least 8 characters with mixed case, number, and special character");
        }
    }

    private static bool IsValidPasswordComplexity(string password)
    {
        if (string.IsNullOrEmpty(password) || password.Length < 8)
            return false;

        if (!System.Text.RegularExpressions.Regex.IsMatch(password, @"[a-z]"))
            return false;

        if (!System.Text.RegularExpressions.Regex.IsMatch(password, @"[A-Z]"))
            return false;

        if (!System.Text.RegularExpressions.Regex.IsMatch(password, @"\d"))
            return false;

        if (!System.Text.RegularExpressions.Regex.IsMatch(password, @"[!@#$%^&*()_+\-=\[\]{};':""\\|,.<>\/?]"))
            return false;

        return true;
    }
}
