using ClinicalIntelligence.Api.Services.Security;
using Xunit;

namespace ClinicalIntelligence.Api.Tests.Services.Security;

/// <summary>
/// Unit tests for InputValidationPolicy to verify injection pattern detection
/// and safe handling of legitimate inputs.
/// </summary>
public class InputValidationPolicyTests
{
    #region Query Parameter Validation Tests

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("hello")]
    [InlineData("search term")]
    [InlineData("john.doe@example.com")]
    [InlineData("123-456-7890")]
    [InlineData("Patient Name")]
    [InlineData("2026-01-15")]
    public void IsQueryParameterSafe_LegitimateValues_ReturnsTrue(string? value)
    {
        // Act
        var result = InputValidationPolicy.IsQueryParameterSafe(value, out var detectedPattern);

        // Assert
        Assert.True(result);
        Assert.Null(detectedPattern);
    }

    [Theory]
    [InlineData("union select * from users")]
    [InlineData("UNION SELECT password FROM users")]
    [InlineData("1; DROP TABLE users")]
    [InlineData("'; DELETE FROM sessions;")]
    [InlineData("admin'--")]
    [InlineData("1 OR 1=1 /*")]
    [InlineData("test */ OR 1=1")]
    [InlineData("'; EXEC(xp_cmdshell 'dir')")]
    [InlineData("1; WAITFOR DELAY '0:0:5'")]
    [InlineData("benchmark(1000000,SHA1('test'))")]
    public void IsQueryParameterSafe_SqlInjectionPatterns_ReturnsFalse(string value)
    {
        // Act
        var result = InputValidationPolicy.IsQueryParameterSafe(value, out var detectedPattern);

        // Assert
        Assert.False(result);
        Assert.NotNull(detectedPattern);
    }

    [Theory]
    [InlineData("<script>alert('xss')</script>")]
    [InlineData("<img src=x onerror=alert(1)>")]
    [InlineData("javascript:alert(document.cookie)")]
    [InlineData("' onmouseover='alert(1)'")]
    [InlineData("eval('malicious code')")]
    public void IsQueryParameterSafe_XssPatterns_ReturnsFalse(string value)
    {
        // Act
        var result = InputValidationPolicy.IsQueryParameterSafe(value, out var detectedPattern);

        // Assert
        Assert.False(result);
        Assert.Equal("script_injection_pattern", detectedPattern);
    }

    [Fact]
    public void IsQueryParameterSafe_PasswordWithSpecialChars_ReturnsTrue()
    {
        // Passwords should NOT be validated through query parameters,
        // but if they were, common password characters should not trigger false positives
        var password = "MyP@ssw0rd!#$%";
        
        var result = InputValidationPolicy.IsQueryParameterSafe(password, out var detectedPattern);

        Assert.True(result);
        Assert.Null(detectedPattern);
    }

    [Fact]
    public void IsQueryParameterSafe_ClinicalText_ReturnsTrue()
    {
        // Clinical notes may contain various characters but should not trigger injection detection
        var clinicalNote = "Patient presents with chest pain. BP: 120/80. Prescribed aspirin 81mg daily.";
        
        var result = InputValidationPolicy.IsQueryParameterSafe(clinicalNote, out var detectedPattern);

        Assert.True(result);
        Assert.Null(detectedPattern);
    }

    #endregion

    #region Header Validation Tests

    [Theory]
    [InlineData("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36")]
    [InlineData("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8")]
    [InlineData("Accept-Language", "en-US,en;q=0.5")]
    [InlineData("Cookie", "session=abc123; preferences=dark")]
    [InlineData("Authorization", "Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...")]
    public void IsHeaderValueSafe_CommonBrowserHeaders_ReturnsTrue(string headerName, string headerValue)
    {
        // Common browser headers should be skipped from validation
        var result = InputValidationPolicy.IsHeaderValueSafe(headerName, headerValue, out var detectedPattern);

        Assert.True(result);
        Assert.Null(detectedPattern);
    }

    [Theory]
    [InlineData("X-Custom-Header", "normal value")]
    [InlineData("X-Request-Id", "abc-123-def")]
    [InlineData("X-Correlation-Id", "550e8400-e29b-41d4-a716-446655440000")]
    public void IsHeaderValueSafe_CustomHeadersWithSafeValues_ReturnsTrue(string headerName, string headerValue)
    {
        var result = InputValidationPolicy.IsHeaderValueSafe(headerName, headerValue, out var detectedPattern);

        Assert.True(result);
        Assert.Null(detectedPattern);
    }

    [Theory]
    [InlineData("X-Custom-Header", "<script>alert(1)</script>")]
    [InlineData("X-Data", "javascript:void(0)")]
    public void IsHeaderValueSafe_CustomHeadersWithXss_ReturnsFalse(string headerName, string headerValue)
    {
        var result = InputValidationPolicy.IsHeaderValueSafe(headerName, headerValue, out var detectedPattern);

        Assert.False(result);
        Assert.Equal("header_script_injection", detectedPattern);
    }

    [Fact]
    public void IsHeaderValueSafe_NullOrEmpty_ReturnsTrue()
    {
        Assert.True(InputValidationPolicy.IsHeaderValueSafe("X-Test", null, out _));
        Assert.True(InputValidationPolicy.IsHeaderValueSafe("X-Test", "", out _));
    }

    #endregion

    #region Normalization Tests

    [Theory]
    [InlineData("Test@Example.COM", "test@example.com")]
    [InlineData("  user@domain.org  ", "user@domain.org")]
    [InlineData("ADMIN@HOSPITAL.ORG", "admin@hospital.org")]
    public void NormalizeEmail_VariousFormats_ReturnsNormalized(string input, string expected)
    {
        var result = InputValidationPolicy.NormalizeEmail(input);
        Assert.Equal(expected, result);
    }

    [Fact]
    public void NormalizeEmail_Null_ReturnsEmptyString()
    {
        var result = InputValidationPolicy.NormalizeEmail(null!);
        Assert.Equal(string.Empty, result);
    }

    [Theory]
    [InlineData("  Hello World  ", "Hello World")]
    [InlineData("NoTrim", "NoTrim")]
    [InlineData("\t\nSpaces\t\n", "Spaces")]
    public void NormalizeText_VariousInputs_ReturnsTrimmed(string input, string expected)
    {
        var result = InputValidationPolicy.NormalizeText(input);
        Assert.Equal(expected, result);
    }

    [Fact]
    public void NormalizeText_Null_ReturnsEmptyString()
    {
        var result = InputValidationPolicy.NormalizeText(null);
        Assert.Equal(string.Empty, result);
    }

    #endregion

    #region Length Validation Tests

    [Theory]
    [InlineData(null, 100, true)]
    [InlineData("", 100, true)]
    [InlineData("short", 100, true)]
    [InlineData("exact", 5, true)]
    [InlineData("toolong", 5, false)]
    public void IsWithinLengthLimit_VariousInputs_ReturnsExpected(string? value, int maxLength, bool expected)
    {
        var result = InputValidationPolicy.IsWithinLengthLimit(value, maxLength);
        Assert.Equal(expected, result);
    }

    #endregion

    #region Safe Logging Tests

    [Fact]
    public void GetSafeLogRepresentation_ReturnsFormattedString()
    {
        var result = InputValidationPolicy.GetSafeLogRepresentation("password", 25);
        
        Assert.Equal("[password:length=25]", result);
        Assert.DoesNotContain("actual", result.ToLower());
    }

    #endregion
}
