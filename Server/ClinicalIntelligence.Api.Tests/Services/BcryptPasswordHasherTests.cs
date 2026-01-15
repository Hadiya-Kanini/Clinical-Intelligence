using System;
using ClinicalIntelligence.Api.Services.Auth;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace ClinicalIntelligence.Api.Tests.Services;

/// <summary>
/// Unit tests for BcryptPasswordHasher service.
/// Validates hashing/verification behavior and minimum work factor enforcement.
/// </summary>
public sealed class BcryptPasswordHasherTests
{
    #region Constructor Tests

    [Fact]
    public void Constructor_WorkFactorBelowMinimum_ThrowsArgumentOutOfRangeException()
    {
        // Arrange & Act & Assert
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            new BcryptPasswordHasher(workFactor: 11));

        Assert.Equal("workFactor", exception.ParamName);
        Assert.Contains("at least 12", exception.Message);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(5)]
    [InlineData(10)]
    [InlineData(11)]
    public void Constructor_WorkFactorBelowMinimum_ThrowsForAllInvalidValues(int workFactor)
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            new BcryptPasswordHasher(workFactor));

        Assert.Contains("at least 12", exception.Message);
    }

    [Fact]
    public void Constructor_WorkFactorAboveMaximum_ThrowsArgumentOutOfRangeException()
    {
        // Arrange & Act & Assert
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            new BcryptPasswordHasher(workFactor: 32));

        Assert.Equal("workFactor", exception.ParamName);
        Assert.Contains("must not exceed 31", exception.Message);
    }

    [Theory]
    [InlineData(12)]
    [InlineData(13)]
    [InlineData(14)]
    [InlineData(15)]
    public void Constructor_ValidWorkFactor_DoesNotThrow(int workFactor)
    {
        // Act
        var hasher = new BcryptPasswordHasher(workFactor);

        // Assert
        Assert.Equal(workFactor, hasher.WorkFactor);
    }

    [Fact]
    public void Constructor_MinimumWorkFactor_SetsCorrectValue()
    {
        // Act
        var hasher = new BcryptPasswordHasher(BcryptPasswordHasher.MinimumWorkFactor);

        // Assert
        Assert.Equal(12, hasher.WorkFactor);
    }

    #endregion

    #region HashPassword Tests

    [Fact]
    public void HashPassword_ValidPassword_ReturnsNonEmptyHash()
    {
        // Arrange
        var hasher = new BcryptPasswordHasher(12);
        var password = "ValidP@ss123";

        // Act
        var hash = hasher.HashPassword(password);

        // Assert
        Assert.NotNull(hash);
        Assert.NotEmpty(hash);
        Assert.StartsWith("$2", hash); // BCrypt hash prefix
    }

    [Fact]
    public void HashPassword_NullPassword_ThrowsArgumentException()
    {
        // Arrange
        var hasher = new BcryptPasswordHasher(12);

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            hasher.HashPassword(null!));

        Assert.Equal("password", exception.ParamName);
        Assert.Contains("cannot be null or empty", exception.Message);
    }

    [Fact]
    public void HashPassword_EmptyPassword_ThrowsArgumentException()
    {
        // Arrange
        var hasher = new BcryptPasswordHasher(12);

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            hasher.HashPassword(string.Empty));

        Assert.Equal("password", exception.ParamName);
    }

    [Fact]
    public void HashPassword_SamePasswordTwice_ProducesDifferentHashes()
    {
        // Arrange
        var hasher = new BcryptPasswordHasher(12);
        var password = "ValidP@ss123";

        // Act
        var hash1 = hasher.HashPassword(password);
        var hash2 = hasher.HashPassword(password);

        // Assert - BCrypt uses random salt, so hashes should differ
        Assert.NotEqual(hash1, hash2);
    }

    [Fact]
    public void HashPassword_DoesNotLogPlaintextPassword()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<BcryptPasswordHasher>>();
        var hasher = new BcryptPasswordHasher(12, mockLogger.Object);
        var password = "SecretP@ss123";

        // Act
        hasher.HashPassword(password);

        // Assert - Verify no log contains the plaintext password
        mockLogger.Verify(
            x => x.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains(password)),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Never);
    }

    #endregion

    #region Verify Tests

    [Fact]
    public void Verify_CorrectPassword_ReturnsTrue()
    {
        // Arrange
        var hasher = new BcryptPasswordHasher(12);
        var password = "ValidP@ss123";
        var hash = hasher.HashPassword(password);

        // Act
        var result = hasher.Verify(password, hash);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void Verify_IncorrectPassword_ReturnsFalse()
    {
        // Arrange
        var hasher = new BcryptPasswordHasher(12);
        var password = "ValidP@ss123";
        var hash = hasher.HashPassword(password);

        // Act
        var result = hasher.Verify("WrongPassword!", hash);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void Verify_NullPassword_ReturnsFalse()
    {
        // Arrange
        var hasher = new BcryptPasswordHasher(12);
        var hash = hasher.HashPassword("ValidP@ss123");

        // Act
        var result = hasher.Verify(null!, hash);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void Verify_EmptyPassword_ReturnsFalse()
    {
        // Arrange
        var hasher = new BcryptPasswordHasher(12);
        var hash = hasher.HashPassword("ValidP@ss123");

        // Act
        var result = hasher.Verify(string.Empty, hash);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void Verify_NullHash_ReturnsFalse()
    {
        // Arrange
        var hasher = new BcryptPasswordHasher(12);

        // Act
        var result = hasher.Verify("ValidP@ss123", null!);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void Verify_EmptyHash_ReturnsFalse()
    {
        // Arrange
        var hasher = new BcryptPasswordHasher(12);

        // Act
        var result = hasher.Verify("ValidP@ss123", string.Empty);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void Verify_InvalidHashFormat_ReturnsFalse()
    {
        // Arrange
        var hasher = new BcryptPasswordHasher(12);

        // Act
        var result = hasher.Verify("ValidP@ss123", "not-a-valid-bcrypt-hash");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void Verify_MalformedBcryptHash_ReturnsFalse()
    {
        // Arrange
        var hasher = new BcryptPasswordHasher(12);

        // Act - Hash with invalid structure but BCrypt-like prefix
        var result = hasher.Verify("ValidP@ss123", "$2a$12$invalid");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void Verify_HashFromDifferentWorkFactor_StillVerifies()
    {
        // Arrange - Hash with work factor 12
        var hasher12 = new BcryptPasswordHasher(12);
        var password = "ValidP@ss123";
        var hash = hasher12.HashPassword(password);

        // Act - Verify with hasher configured for work factor 13
        // (BCrypt verification doesn't depend on configured work factor)
        var hasher13 = new BcryptPasswordHasher(13);
        var result = hasher13.Verify(password, hash);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void Verify_DoesNotLogPlaintextPassword()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<BcryptPasswordHasher>>();
        var hasher = new BcryptPasswordHasher(12, mockLogger.Object);
        var password = "SecretP@ss123";
        var hash = hasher.HashPassword(password);

        // Act
        hasher.Verify(password, hash);

        // Assert - Verify no log contains the plaintext password
        mockLogger.Verify(
            x => x.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains(password)),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Never);
    }

    #endregion

    #region Round-Trip Tests

    [Theory]
    [InlineData("SimplePass1!")]
    [InlineData("C0mpl3x@Password")]
    [InlineData("Unicode™Pass1!")]
    [InlineData("VeryLongPasswordWithManyCharacters123!@#$%^&*()")]
    public void HashAndVerify_RoundTrip_Succeeds(string password)
    {
        // Arrange
        var hasher = new BcryptPasswordHasher(12);

        // Act
        var hash = hasher.HashPassword(password);
        var result = hasher.Verify(password, hash);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void HashAndVerify_CaseSensitive_FailsOnCaseMismatch()
    {
        // Arrange
        var hasher = new BcryptPasswordHasher(12);
        var password = "ValidP@ss123";
        var hash = hasher.HashPassword(password);

        // Act
        var result = hasher.Verify("validp@ss123", hash);

        // Assert
        Assert.False(result);
    }

    #endregion

    #region Constants Tests

    [Fact]
    public void MinimumWorkFactor_IsAtLeast12()
    {
        Assert.Equal(12, BcryptPasswordHasher.MinimumWorkFactor);
    }

    [Fact]
    public void MaximumWorkFactor_Is31()
    {
        Assert.Equal(31, BcryptPasswordHasher.MaximumWorkFactor);
    }

    #endregion
}
