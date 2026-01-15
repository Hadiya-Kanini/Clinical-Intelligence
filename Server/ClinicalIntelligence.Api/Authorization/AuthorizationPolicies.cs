namespace ClinicalIntelligence.Api.Authorization;

/// <summary>
/// Centralized authorization policy names and configuration.
/// Defines policy names and their role requirements for consistent RBAC enforcement.
/// </summary>
public static class AuthorizationPolicies
{
    /// <summary>
    /// Policy requiring the Admin role.
    /// Use for endpoints that should only be accessible to administrators.
    /// </summary>
    public const string AdminOnly = "AdminOnly";

    /// <summary>
    /// Policy requiring any authenticated user (Admin or Standard).
    /// Use for endpoints that require authentication but no specific role.
    /// </summary>
    public const string Authenticated = "Authenticated";
}
