using System.Text.RegularExpressions;

namespace ClinicalIntelligence.Api.Validation;

/// <summary>
/// Centralized RFC 5322-compliant email validation utility.
/// Provides consistent email format validation across all API endpoints and seeding operations.
/// </summary>
public static class EmailValidation
{
    /// <summary>
    /// RFC 5322-compliant email regex pattern.
    /// Supports:
    /// - Standard emails (user@domain.com)
    /// - Plus addressing (user+tag@domain.com)
    /// - Subdomains (user@sub.domain.com)
    /// - Dots in local part (first.last@domain.com)
    /// - Hyphens in domain (user@my-domain.com)
    /// - Numeric domains (user@123.com)
    /// 
    /// IDN Handling: Accepts ASCII and punycode domains only.
    /// Raw unicode domains are rejected for security and consistency.
    /// </summary>
    private static readonly Regex Rfc5322EmailRegex = new(
        @"^(?!.*\.\.)(?!.*\.$)(?!^\.)(?!.*@\.)(?!.*\.@)[a-zA-Z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-zA-Z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-zA-Z0-9](?:[a-zA-Z0-9-]*[a-zA-Z0-9])?\.)+[a-zA-Z]{2,}$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant,
        TimeSpan.FromMilliseconds(250));

    /// <summary>
    /// Normalizes an email address by trimming whitespace and converting to lowercase.
    /// </summary>
    /// <param name="email">The email address to normalize.</param>
    /// <returns>Normalized email address, or empty string if input is null/whitespace.</returns>
    public static string Normalize(string? email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return string.Empty;

        return email.Trim().ToLowerInvariant();
    }

    /// <summary>
    /// Validates whether the provided email address conforms to RFC 5322 format.
    /// </summary>
    /// <param name="email">The email address to validate (will be normalized internally).</param>
    /// <returns>True if the email format is valid; otherwise, false.</returns>
    public static bool IsValid(string? email)
    {
        var normalized = Normalize(email);

        if (string.IsNullOrEmpty(normalized))
            return false;

        if (normalized.Length > 254)
            return false;

        var atIndex = normalized.IndexOf('@');
        if (atIndex < 0)
            return false;

        var localPart = normalized[..atIndex];
        if (localPart.Length > 64)
            return false;

        try
        {
            return Rfc5322EmailRegex.IsMatch(normalized);
        }
        catch (RegexMatchTimeoutException)
        {
            return false;
        }
    }

    /// <summary>
    /// Validates email format and returns a validation result with details.
    /// </summary>
    /// <param name="email">The email address to validate.</param>
    /// <returns>A tuple containing (isValid, normalizedEmail, errorDetail).</returns>
    public static (bool IsValid, string NormalizedEmail, string? ErrorDetail) ValidateWithDetails(string? email)
    {
        var normalized = Normalize(email);

        if (string.IsNullOrEmpty(normalized))
            return (false, string.Empty, "email:required");

        if (!IsValid(normalized))
            return (false, normalized, "email:invalid_format");

        return (true, normalized, null);
    }
}
