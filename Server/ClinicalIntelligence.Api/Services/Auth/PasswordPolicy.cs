using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace ClinicalIntelligence.Api.Services.Auth;

/// <summary>
/// Centralized password complexity validation per FR-009c.
/// Provides reusable validation logic with detailed requirement feedback.
/// </summary>
public static class PasswordPolicy
{
    public const int MinLength = 8;
    public const int MaxLength = 128;

    private static readonly Regex LowercaseRegex = new(@"[a-z]", RegexOptions.Compiled);
    private static readonly Regex UppercaseRegex = new(@"[A-Z]", RegexOptions.Compiled);
    private static readonly Regex DigitRegex = new(@"\d", RegexOptions.Compiled);
    private static readonly Regex SpecialCharRegex = new(@"[^A-Za-z0-9]", RegexOptions.Compiled);

    /// <summary>
    /// Validates password against all complexity requirements.
    /// </summary>
    /// <param name="password">The password to validate.</param>
    /// <returns>True if password meets all requirements; otherwise false.</returns>
    public static bool IsValid(string? password)
    {
        return GetMissingRequirements(password).Count == 0;
    }

    /// <summary>
    /// Returns a list of unmet password requirements for error reporting.
    /// </summary>
    /// <param name="password">The password to validate.</param>
    /// <returns>List of requirement descriptions that are not met.</returns>
    public static List<string> GetMissingRequirements(string? password)
    {
        var missing = new List<string>();

        if (string.IsNullOrEmpty(password))
        {
            missing.Add($"Password must be at least {MinLength} characters");
            missing.Add("Password must contain a lowercase letter");
            missing.Add("Password must contain an uppercase letter");
            missing.Add("Password must contain a number");
            missing.Add("Password must contain a special character");
            return missing;
        }

        if (password.Length < MinLength)
        {
            missing.Add($"Password must be at least {MinLength} characters");
        }

        if (password.Length > MaxLength)
        {
            missing.Add($"Password must not exceed {MaxLength} characters");
        }

        if (!LowercaseRegex.IsMatch(password))
        {
            missing.Add("Password must contain a lowercase letter");
        }

        if (!UppercaseRegex.IsMatch(password))
        {
            missing.Add("Password must contain an uppercase letter");
        }

        if (!DigitRegex.IsMatch(password))
        {
            missing.Add("Password must contain a number");
        }

        if (!SpecialCharRegex.IsMatch(password))
        {
            missing.Add("Password must contain a special character");
        }

        return missing;
    }

    /// <summary>
    /// Validates password and throws InvalidOperationException if invalid.
    /// Error message does not include the password value for security.
    /// </summary>
    /// <param name="password">The password to validate.</param>
    /// <param name="contextMessage">Optional context for the error message (e.g., "ADMIN_PASSWORD").</param>
    /// <exception cref="InvalidOperationException">Thrown when password does not meet requirements.</exception>
    public static void ValidateOrThrow(string? password, string contextMessage = "Password")
    {
        var missing = GetMissingRequirements(password);
        if (missing.Count > 0)
        {
            throw new InvalidOperationException(
                $"{contextMessage} must be at least {MinLength} characters with mixed case, number, and special character");
        }
    }
}
