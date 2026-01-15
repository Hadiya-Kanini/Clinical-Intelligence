namespace ClinicalIntelligence.Api.Services.Auth;

/// <summary>
/// Service interface for invalidating user sessions.
/// Used during password reset to revoke all existing sessions for security.
/// </summary>
public interface ISessionInvalidationService
{
    /// <summary>
    /// Invalidates all active sessions for a user by setting IsRevoked = true.
    /// </summary>
    /// <param name="userId">The user ID whose sessions should be invalidated.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Result containing the count of revoked sessions.</returns>
    Task<SessionInvalidationResult> InvalidateAllSessionsAsync(Guid userId, CancellationToken cancellationToken = default);
}

/// <summary>
/// Result of session invalidation operation.
/// </summary>
public sealed record SessionInvalidationResult
{
    /// <summary>
    /// Number of sessions that were revoked.
    /// </summary>
    public int RevokedCount { get; init; }

    /// <summary>
    /// IDs of the revoked sessions (for audit logging).
    /// </summary>
    public IReadOnlyList<Guid> RevokedSessionIds { get; init; } = Array.Empty<Guid>();

    /// <summary>
    /// Creates a result with the specified revoked count and session IDs.
    /// </summary>
    public static SessionInvalidationResult Create(int revokedCount, IReadOnlyList<Guid> revokedSessionIds) => new()
    {
        RevokedCount = revokedCount,
        RevokedSessionIds = revokedSessionIds
    };

    /// <summary>
    /// Creates a result indicating no sessions were revoked.
    /// </summary>
    public static SessionInvalidationResult None => new()
    {
        RevokedCount = 0,
        RevokedSessionIds = Array.Empty<Guid>()
    };
}
