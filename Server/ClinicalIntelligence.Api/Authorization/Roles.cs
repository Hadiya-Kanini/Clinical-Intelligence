namespace ClinicalIntelligence.Api.Authorization;

/// <summary>
/// Centralized role constants for RBAC.
/// Matches database seed values and User.Role defaults.
/// Use these constants to avoid string drift across JWT claims, DB, and authorization policies.
/// </summary>
public static class Roles
{
    /// <summary>
    /// Administrator role with full system access including User Management and System Health.
    /// </summary>
    public const string Admin = "Admin";

    /// <summary>
    /// Standard user role with access to Patient Dashboard, Document Upload, and Profile functions.
    /// </summary>
    public const string Standard = "Standard";

    /// <summary>
    /// All valid roles in the system.
    /// </summary>
    public static readonly string[] All = { Admin, Standard };

    /// <summary>
    /// Checks if the provided role is valid (case-insensitive comparison).
    /// </summary>
    /// <param name="role">Role to validate.</param>
    /// <returns>True if the role is valid; otherwise false.</returns>
    public static bool IsValid(string? role)
    {
        if (string.IsNullOrWhiteSpace(role))
            return false;

        return string.Equals(role, Admin, StringComparison.OrdinalIgnoreCase) ||
               string.Equals(role, Standard, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Normalizes the role string to match the canonical casing.
    /// </summary>
    /// <param name="role">Role to normalize.</param>
    /// <returns>Normalized role string or null if invalid.</returns>
    public static string? Normalize(string? role)
    {
        if (string.IsNullOrWhiteSpace(role))
            return null;

        if (string.Equals(role, Admin, StringComparison.OrdinalIgnoreCase))
            return Admin;

        if (string.Equals(role, Standard, StringComparison.OrdinalIgnoreCase))
            return Standard;

        return null;
    }
}
