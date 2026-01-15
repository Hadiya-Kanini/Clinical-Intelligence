using ClinicalIntelligence.Api.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ClinicalIntelligence.Api.Services.Auth;

/// <summary>
/// EF Core implementation of session invalidation service.
/// Revokes all active sessions for a user by setting IsRevoked = true.
/// </summary>
public sealed class SessionInvalidationService : ISessionInvalidationService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<SessionInvalidationService>? _logger;

    public SessionInvalidationService(
        ApplicationDbContext dbContext,
        ILogger<SessionInvalidationService>? logger = null)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<SessionInvalidationResult> InvalidateAllSessionsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var activeSessions = await _dbContext.Sessions
            .Where(s => s.UserId == userId && !s.IsRevoked && s.ExpiresAt > DateTime.UtcNow)
            .ToListAsync(cancellationToken);

        if (activeSessions.Count == 0)
        {
            _logger?.LogDebug("No active sessions to invalidate for user {UserId}", userId);
            return SessionInvalidationResult.None;
        }

        var revokedSessionIds = new List<Guid>(activeSessions.Count);

        foreach (var session in activeSessions)
        {
            session.IsRevoked = true;
            revokedSessionIds.Add(session.Id);
        }

        _logger?.LogInformation(
            "Invalidated {SessionCount} active sessions for user {UserId}",
            revokedSessionIds.Count,
            userId);

        return SessionInvalidationResult.Create(revokedSessionIds.Count, revokedSessionIds);
    }
}
