namespace ClinicalIntelligence.Api.Contracts.Auth;

/// <summary>
/// Abstraction for checking token/session revocation status.
/// Enables validation during JWT authentication to enforce server-side logout.
/// </summary>
public interface ITokenRevocationStore
{
    /// <summary>
    /// Checks if a session has been revoked or is invalid.
    /// </summary>
    /// <param name="sessionId">The session ID from the JWT 'sid' claim.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if the session is revoked or invalid; false if valid.</returns>
    Task<bool> IsSessionRevokedAsync(Guid sessionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Revokes a session by marking it as revoked in the database.
    /// </summary>
    /// <param name="sessionId">The session ID to revoke.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if revocation succeeded; false if session not found.</returns>
    Task<bool> RevokeSessionAsync(Guid sessionId, CancellationToken cancellationToken = default);
}
