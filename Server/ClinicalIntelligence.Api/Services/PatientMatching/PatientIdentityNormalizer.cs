using System.Globalization;
using System.Text.RegularExpressions;

namespace ClinicalIntelligence.Api.Services.PatientMatching;

/// <summary>
/// Centralized normalization utilities for patient identity matching.
/// Provides deterministic normalization for MRN, name, and DOB parsing.
/// </summary>
public static partial class PatientIdentityNormalizer
{
    private static readonly string[] SupportedDobFormats = new[]
    {
        "yyyy-MM-dd",
        "MM/dd/yyyy",
        "dd-MM-yyyy",
        "M/d/yyyy",
        "d-M-yyyy",
        "yyyy/MM/dd"
    };

    /// <summary>
    /// Normalizes an MRN for comparison by trimming, removing separators, and converting to uppercase.
    /// </summary>
    /// <param name="mrn">The MRN to normalize.</param>
    /// <returns>Normalized MRN or null if input is null/empty.</returns>
    public static string? NormalizeMrn(string? mrn)
    {
        if (string.IsNullOrWhiteSpace(mrn))
            return null;

        // Trim whitespace
        var normalized = mrn.Trim();

        // Remove common separators (spaces, dashes, underscores)
        normalized = MrnSeparatorRegex().Replace(normalized, string.Empty);

        // Convert to uppercase for case-insensitive comparison
        normalized = normalized.ToUpperInvariant();

        return string.IsNullOrEmpty(normalized) ? null : normalized;
    }

    /// <summary>
    /// Normalizes a patient name for comparison by trimming, collapsing whitespace,
    /// removing punctuation, and converting to uppercase.
    /// Handles variations like "DOE, JANE" vs "Jane Doe".
    /// </summary>
    /// <param name="name">The name to normalize.</param>
    /// <returns>Normalized name or null if input is null/empty.</returns>
    public static string? NormalizeName(string? name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return null;

        // Trim whitespace
        var normalized = name.Trim();

        // Remove punctuation (commas, periods, apostrophes, etc.)
        normalized = NamePunctuationRegex().Replace(normalized, " ");

        // Collapse consecutive whitespace to single space
        normalized = WhitespaceRegex().Replace(normalized, " ").Trim();

        // Convert to uppercase for case-insensitive comparison
        normalized = normalized.ToUpperInvariant();

        // Sort words alphabetically to handle "DOE JANE" vs "JANE DOE"
        var words = normalized.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        Array.Sort(words, StringComparer.Ordinal);
        normalized = string.Join(" ", words);

        return string.IsNullOrEmpty(normalized) ? null : normalized;
    }

    /// <summary>
    /// Parses a DOB string into DateOnly using multiple supported formats.
    /// </summary>
    /// <param name="dobString">The DOB string to parse.</param>
    /// <returns>Parsed DateOnly or null if parsing fails.</returns>
    public static DateOnly? ParseDob(string? dobString)
    {
        if (string.IsNullOrWhiteSpace(dobString))
            return null;

        var trimmed = dobString.Trim();

        foreach (var format in SupportedDobFormats)
        {
            if (DateOnly.TryParseExact(
                trimmed,
                format,
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out var result))
            {
                return result;
            }
        }

        // Try general parsing as fallback
        if (DateOnly.TryParse(trimmed, CultureInfo.InvariantCulture, DateTimeStyles.None, out var fallbackResult))
        {
            return fallbackResult;
        }

        return null;
    }

    /// <summary>
    /// Generates a synthetic MRN for patients without an extracted MRN.
    /// Format: AUTO-{GUID} to satisfy DB constraints.
    /// </summary>
    /// <returns>A synthetic MRN string.</returns>
    public static string GenerateSyntheticMrn()
    {
        return $"AUTO-{Guid.NewGuid():N}";
    }

    [GeneratedRegex(@"[\s\-_]")]
    private static partial Regex MrnSeparatorRegex();

    [GeneratedRegex(@"[,.'`\-]")]
    private static partial Regex NamePunctuationRegex();

    [GeneratedRegex(@"\s+")]
    private static partial Regex WhitespaceRegex();
}
