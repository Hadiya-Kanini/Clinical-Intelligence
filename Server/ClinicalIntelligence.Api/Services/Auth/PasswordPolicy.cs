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

    private const string LowercaseChars = "abcdefghijklmnopqrstuvwxyz";
    private const string UppercaseChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
    private const string DigitChars = "0123456789";
    private const string SpecialChars = "!@#$%^&*()-_=+[]{}|;:,.<>?";

    /// <summary>
    /// Generates a cryptographically secure random password that satisfies all policy requirements.
    /// </summary>
    /// <param name="length">Desired password length (minimum 12, default 16).</param>
    /// <returns>A random password meeting all complexity requirements.</returns>
    public static string GenerateSecurePassword(int length = 16)
    {
        if (length < 12)
            length = 12;

        using var rng = System.Security.Cryptography.RandomNumberGenerator.Create();
        var password = new char[length];
        var allChars = LowercaseChars + UppercaseChars + DigitChars + SpecialChars;

        password[0] = GetRandomChar(rng, LowercaseChars);
        password[1] = GetRandomChar(rng, UppercaseChars);
        password[2] = GetRandomChar(rng, DigitChars);
        password[3] = GetRandomChar(rng, SpecialChars);

        for (var i = 4; i < length; i++)
        {
            password[i] = GetRandomChar(rng, allChars);
        }

        Shuffle(rng, password);

        return new string(password);
    }

    private static char GetRandomChar(System.Security.Cryptography.RandomNumberGenerator rng, string chars)
    {
        var bytes = new byte[4];
        rng.GetBytes(bytes);
        var index = (int)(BitConverter.ToUInt32(bytes, 0) % (uint)chars.Length);
        return chars[index];
    }

    private static void Shuffle(System.Security.Cryptography.RandomNumberGenerator rng, char[] array)
    {
        var bytes = new byte[4];
        for (var i = array.Length - 1; i > 0; i--)
        {
            rng.GetBytes(bytes);
            var j = (int)(BitConverter.ToUInt32(bytes, 0) % (uint)(i + 1));
            (array[i], array[j]) = (array[j], array[i]);
        }
    }
}
