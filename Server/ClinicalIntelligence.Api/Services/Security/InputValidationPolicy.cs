using System.Text.RegularExpressions;

namespace ClinicalIntelligence.Api.Services.Security;

/// <summary>
/// Reusable input validation helpers for detecting suspicious patterns and normalizing inputs.
/// Designed to balance security with avoiding false positives for legitimate clinical content.
/// </summary>
public static class InputValidationPolicy
{
    /// <summary>
    /// SQL injection patterns that are suspicious in query parameters.
    /// These patterns are unlikely to appear in legitimate search/filter values.
    /// </summary>
    private static readonly string[] SqlInjectionPatterns =
    {
        "union select", "union all select", "insert into", "delete from", "drop table",
        "drop database", "truncate table", "update set", "exec(", "execute(",
        "xp_cmdshell", "xp_", "sp_executesql", "sp_execute", "waitfor delay",
        "benchmark(", "sleep(", "pg_sleep", "0x", "char(", "nchar(",
        "cast(", "convert(", "declare @", "set @"
    };

    /// <summary>
    /// XSS/script injection patterns that are suspicious in query parameters.
    /// </summary>
    private static readonly string[] ScriptInjectionPatterns =
    {
        "<script", "</script", "javascript:", "vbscript:", "onload=", "onerror=",
        "onclick=", "onmouseover=", "onfocus=", "onblur=", "expression(",
        "eval(", "document.cookie", "document.write", "window.location"
    };

    /// <summary>
    /// Comment patterns that may indicate injection attempts.
    /// </summary>
    private static readonly string[] CommentPatterns =
    {
        "--", "/*", "*/", "#--"
    };

    /// <summary>
    /// Validates a query parameter value for suspicious injection patterns.
    /// Returns true if the value is safe, false if suspicious patterns are detected.
    /// </summary>
    /// <param name="value">The query parameter value to validate.</param>
    /// <param name="detectedPattern">Output parameter containing the detected pattern if found.</param>
    /// <returns>True if safe; false if suspicious.</returns>
    public static bool IsQueryParameterSafe(string? value, out string? detectedPattern)
    {
        detectedPattern = null;

        if (string.IsNullOrEmpty(value))
            return true;

        var lowerValue = value.ToLowerInvariant();

        // Check SQL injection patterns
        foreach (var pattern in SqlInjectionPatterns)
        {
            if (lowerValue.Contains(pattern))
            {
                detectedPattern = "sql_injection_pattern";
                return false;
            }
        }

        // Check script injection patterns
        foreach (var pattern in ScriptInjectionPatterns)
        {
            if (lowerValue.Contains(pattern))
            {
                detectedPattern = "script_injection_pattern";
                return false;
            }
        }

        // Check comment patterns (common in SQL injection)
        foreach (var pattern in CommentPatterns)
        {
            if (lowerValue.Contains(pattern))
            {
                detectedPattern = "comment_injection_pattern";
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Validates a header value for suspicious patterns.
    /// More permissive than query parameters to avoid blocking legitimate User-Agent strings.
    /// </summary>
    /// <param name="headerName">The header name being validated.</param>
    /// <param name="headerValue">The header value to validate.</param>
    /// <param name="detectedPattern">Output parameter containing the detected pattern if found.</param>
    /// <returns>True if safe; false if suspicious.</returns>
    public static bool IsHeaderValueSafe(string headerName, string? headerValue, out string? detectedPattern)
    {
        detectedPattern = null;

        if (string.IsNullOrEmpty(headerValue))
            return true;

        // Skip validation for common headers that may contain special characters
        var skipHeaders = new[] { "user-agent", "accept", "accept-language", "accept-encoding", "cookie", "authorization" };
        if (skipHeaders.Contains(headerName.ToLowerInvariant()))
            return true;

        var lowerValue = headerValue.ToLowerInvariant();

        // Only check for script injection in custom headers
        foreach (var pattern in ScriptInjectionPatterns)
        {
            if (lowerValue.Contains(pattern))
            {
                detectedPattern = "header_script_injection";
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Normalizes an email address for consistent storage and comparison.
    /// </summary>
    /// <param name="email">The email address to normalize.</param>
    /// <returns>Normalized email address (trimmed and lowercased).</returns>
    public static string NormalizeEmail(string email)
    {
        return email?.Trim().ToLowerInvariant() ?? string.Empty;
    }

    /// <summary>
    /// Normalizes a general text input by trimming whitespace.
    /// Does not alter case to preserve user intent.
    /// </summary>
    /// <param name="input">The input to normalize.</param>
    /// <returns>Trimmed input.</returns>
    public static string NormalizeText(string? input)
    {
        return input?.Trim() ?? string.Empty;
    }

    /// <summary>
    /// Validates that a string does not exceed a maximum length.
    /// </summary>
    /// <param name="value">The value to check.</param>
    /// <param name="maxLength">Maximum allowed length.</param>
    /// <returns>True if within limits; false otherwise.</returns>
    public static bool IsWithinLengthLimit(string? value, int maxLength)
    {
        return string.IsNullOrEmpty(value) || value.Length <= maxLength;
    }

    /// <summary>
    /// Sanitizes a value for safe logging by truncating and removing sensitive patterns.
    /// Never logs the actual suspicious content to prevent log injection.
    /// </summary>
    /// <param name="parameterName">The parameter name (safe to log).</param>
    /// <param name="valueLength">The length of the original value.</param>
    /// <returns>A safe log representation.</returns>
    public static string GetSafeLogRepresentation(string parameterName, int valueLength)
    {
        return $"[{parameterName}:length={valueLength}]";
    }
}
