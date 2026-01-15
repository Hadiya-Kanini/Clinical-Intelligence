namespace ClinicalIntelligence.Api.Contracts;

/// <summary>
/// Response contract for CSRF token retrieval endpoint.
/// </summary>
public sealed record CsrfTokenResponse
{
    /// <summary>
    /// The CSRF token to include in X-CSRF-TOKEN header for state-changing requests.
    /// </summary>
    public required string Token { get; init; }

    /// <summary>
    /// Token expiration timestamp (aligned with session expiry).
    /// </summary>
    public required DateTime ExpiresAt { get; init; }
}
