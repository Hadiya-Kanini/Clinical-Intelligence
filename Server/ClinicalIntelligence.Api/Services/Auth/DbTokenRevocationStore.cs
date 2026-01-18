using ClinicalIntelligence.Api.Contracts.Auth;
using ClinicalIntelligence.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace ClinicalIntelligence.Api.Services.Auth;

/// <summary>
/// Database-backed implementation of token revocation store.
/// Uses the sessions table to track revocation status.
/// </summary>
public sealed class DbTokenRevocationStore : ITokenRevocationStore
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<DbTokenRevocationStore> _logger;

    public DbTokenRevocationStore(ApplicationDbContext dbContext, ILogger<DbTokenRevocationStore> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<bool> IsSessionRevokedAsync(Guid sessionId, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("IsSessionRevokedAsync: Checking session {SessionId}", sessionId);
        
        var session = await _dbContext.Sessions
            .AsNoTracking()
            .Where(s => s.Id == sessionId)
            .Select(s => new { s.IsRevoked, s.ExpiresAt })
            .FirstOrDefaultAsync(cancellationToken);

        if (session == null)
        {
            _logger.LogDebug("IsSessionRevokedAsync: Session not found: {SessionId}", sessionId);
            return true; // Treat missing session as revoked
        }

        if (session.IsRevoked)
        {
            _logger.LogDebug("IsSessionRevokedAsync: Session is revoked: {SessionId}", sessionId);
            return true;
        }

        if (session.ExpiresAt.ToUniversalTime() <= DateTime.UtcNow)
        {
            _logger.LogDebug("IsSessionRevokedAsync: Session has expired: {SessionId}", sessionId);
            return true;
        }

        _logger.LogDebug("IsSessionRevokedAsync: Session is valid: {SessionId}", sessionId);
        return false;
    }

    /// <inheritdoc />
    public async Task<bool> RevokeSessionAsync(Guid sessionId, CancellationToken cancellationToken = default)
    {
        var session = await _dbContext.Sessions.FindAsync(new object[] { sessionId }, cancellationToken);

        if (session == null)
        {
            _logger.LogWarning("Attempted to revoke non-existent session: {SessionId}", sessionId);
            return false;
        }

        if (session.IsRevoked)
        {
            _logger.LogDebug("Session already revoked: {SessionId}", sessionId);
            return true; // Idempotent - already revoked is success
        }

        session.IsRevoked = true;
        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Session revoked successfully: {SessionId}", sessionId);
        return true;
    }
}
