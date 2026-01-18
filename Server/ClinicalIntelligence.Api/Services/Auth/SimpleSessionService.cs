using ClinicalIntelligence.Api.Data;
using ClinicalIntelligence.Api.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace ClinicalIntelligence.Api.Services.Auth;

/// <summary>
/// Simplified session management service
/// </summary>
public interface ISimpleSessionService
{
    Task<Guid> CreateSessionAsync(Guid userId, string ipAddress, string userAgent);
    Task<bool> IsSessionValidAsync(Guid sessionId);
    Task RevokeSessionAsync(Guid sessionId);
}

public class SimpleSessionService : ISimpleSessionService
{
    private readonly ApplicationDbContext _dbContext;

    public SimpleSessionService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Guid> CreateSessionAsync(Guid userId, string ipAddress, string userAgent)
    {
        var session = new Session
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            IpAddress = ipAddress ?? "unknown",
            UserAgent = userAgent ?? "unknown",
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddHours(24),
            LastActivityAt = DateTime.UtcNow,
            IsRevoked = false
        };

        _dbContext.Sessions.Add(session);
        await _dbContext.SaveChangesAsync();

        return session.Id;
    }

    public async Task<bool> IsSessionValidAsync(Guid sessionId)
    {
        var session = await _dbContext.Sessions
            .Where(s => s.Id == sessionId)
            .FirstOrDefaultAsync();

        if (session == null)
            return false;

        if (session.IsRevoked)
            return false;

        if (session.ExpiresAt < DateTime.UtcNow)
            return false;

        return true;
    }

    public async Task RevokeSessionAsync(Guid sessionId)
    {
        var session = await _dbContext.Sessions
            .Where(s => s.Id == sessionId)
            .FirstOrDefaultAsync();

        if (session != null)
        {
            session.IsRevoked = true;
            await _dbContext.SaveChangesAsync();
        }
    }
}
