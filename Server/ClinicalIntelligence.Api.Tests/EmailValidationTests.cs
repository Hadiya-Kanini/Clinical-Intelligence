using ClinicalIntelligence.Api.Validation;
using Xunit;

namespace ClinicalIntelligence.Api.Tests;

public class EmailValidationTests
{
    #region Normalize Tests

    [Theory]
    [InlineData("  User@Domain.COM  ", "user@domain.com")]
    [InlineData("USER@DOMAIN.COM", "user@domain.com")]
    [InlineData("user@domain.com", "user@domain.com")]
    public void Normalize_ShouldTrimAndLowercase(string input, string expected)
    {
        var result = EmailValidation.Normalize(input);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Normalize_ShouldReturnEmptyForNullOrWhitespace(string? input)
    {
        var result = EmailValidation.Normalize(input);
        Assert.Equal(string.Empty, result);
    }

    #endregion

    #region IsValid - Valid Emails

    [Theory]
    [InlineData("user@domain.com")]
    [InlineData("user+tag@domain.com")]
    [InlineData("first.last@domain.com")]
    [InlineData("user@sub.domain.com")]
    [InlineData("user@my-domain.com")]
    [InlineData("123@domain.com")]
    [InlineData("user@domain.healthcare")]
    [InlineData("user+tag+another@domain.com")]
    [InlineData("user_name@domain.com")]
    [InlineData("User@Domain.COM")]
    public void IsValid_ShouldAcceptValidEmails(string email)
    {
        Assert.True(EmailValidation.IsValid(email));
    }

    #endregion

    #region IsValid - Invalid Emails

    [Theory]
    [InlineData("userdomain.com")]
    [InlineData("user@")]
    [InlineData("@domain.com")]
    [InlineData("user @domain.com")]
    [InlineData("user@domain.com.")]
    [InlineData(".user@domain.com")]
    [InlineData("user.@domain.com")]
    [InlineData("user..name@domain.com")]
    [InlineData("user@domain")]
    [InlineData("user@domain.c")]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void IsValid_ShouldRejectInvalidEmails(string? email)
    {
        Assert.False(EmailValidation.IsValid(email));
    }

    [Fact]
    public void IsValid_ShouldRejectEmailExceeding254Characters()
    {
        var longEmail = new string('a', 250) + "@b.com";
        Assert.False(EmailValidation.IsValid(longEmail));
    }

    [Fact]
    public void IsValid_ShouldRejectLocalPartExceeding64Characters()
    {
        var longLocalPart = new string('a', 65) + "@domain.com";
        Assert.False(EmailValidation.IsValid(longLocalPart));
    }

    [Fact]
    public void IsValid_ShouldRejectDotBeforeAtSymbol()
    {
        Assert.False(EmailValidation.IsValid("user.@domain.com"));
    }

    [Fact]
    public void IsValid_ShouldRejectDotAfterAtSymbol()
    {
        Assert.False(EmailValidation.IsValid("user@.domain.com"));
    }

    #endregion

    #region ValidateWithDetails Tests

    [Fact]
    public void ValidateWithDetails_ShouldReturnErrorForEmptyEmail()
    {
        var result = EmailValidation.ValidateWithDetails("");
        
        Assert.False(result.IsValid);
        Assert.Equal("email:required", result.ErrorDetail);
        Assert.Equal(string.Empty, result.NormalizedEmail);
    }

    [Fact]
    public void ValidateWithDetails_ShouldReturnErrorForNullEmail()
    {
        var result = EmailValidation.ValidateWithDetails(null);
        
        Assert.False(result.IsValid);
        Assert.Equal("email:required", result.ErrorDetail);
    }

    [Fact]
    public void ValidateWithDetails_ShouldReturnErrorForInvalidFormat()
    {
        var result = EmailValidation.ValidateWithDetails("invalid-email");
        
        Assert.False(result.IsValid);
        Assert.Equal("email:invalid_format", result.ErrorDetail);
        Assert.Equal("invalid-email", result.NormalizedEmail);
    }

    [Fact]
    public void ValidateWithDetails_ShouldReturnSuccessForValidEmail()
    {
        var result = EmailValidation.ValidateWithDetails("user+tag@domain.com");
        
        Assert.True(result.IsValid);
        Assert.Null(result.ErrorDetail);
        Assert.Equal("user+tag@domain.com", result.NormalizedEmail);
    }

    [Fact]
    public void ValidateWithDetails_ShouldNormalizeEmailInResult()
    {
        var result = EmailValidation.ValidateWithDetails("  USER@DOMAIN.COM  ");
        
        Assert.True(result.IsValid);
        Assert.Equal("user@domain.com", result.NormalizedEmail);
    }

    #endregion

    #region Edge Cases - Plus Addressing

    [Theory]
    [InlineData("user+tag@domain.com")]
    [InlineData("user+@domain.com")]
    [InlineData("user+tag+subtag@domain.com")]
    [InlineData("user+123@domain.com")]
    public void IsValid_ShouldAcceptPlusAddressing(string email)
    {
        Assert.True(EmailValidation.IsValid(email));
    }

    #endregion

    #region Edge Cases - International Domains

    [Fact]
    public void IsValid_ShouldAcceptPunycodeDomains()
    {
        Assert.True(EmailValidation.IsValid("user@xn--nxasmq5b.com"));
    }

    #endregion
}
