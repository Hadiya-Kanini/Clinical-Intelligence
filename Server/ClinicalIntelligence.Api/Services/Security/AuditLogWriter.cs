using System.Text.Json;
using ClinicalIntelligence.Api.Data;
using ClinicalIntelligence.Api.Domain.Models;
using Microsoft.Extensions.Logging;

namespace ClinicalIntelligence.Api.Services.Security;

/// <summary>
/// Implementation of audit log writer with best-effort persistence.
/// Swallows persistence failures and logs warnings to prevent audit logging
/// from affecting the externally observable behavior of endpoints.
/// </summary>
public sealed class AuditLogWriter : IAuditLogWriter
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<AuditLogWriter>? _logger;

    public AuditLogWriter(ApplicationDbContext dbContext, ILogger<AuditLogWriter>? logger = null)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<bool> WriteAsync(
        string actionType,
        Guid? userId,
        Guid? sessionId,
        string? resourceType,
        Guid? resourceId,
        string? ipAddress,
        string? userAgent,
        object? metadata,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var auditEvent = new AuditLogEvent
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                SessionId = sessionId,
                ActionType = actionType,
                IpAddress = ipAddress,
                UserAgent = userAgent,
                ResourceType = resourceType,
                ResourceId = resourceId,
                Timestamp = DateTime.UtcNow,
                Metadata = metadata != null ? JsonSerializer.Serialize(metadata) : null
            };

            _dbContext.AuditLogEvents.Add(auditEvent);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return true;
        }
        catch (OperationCanceledException)
        {
            // Request was cancelled - rethrow to let framework handle it
            throw;
        }
        catch (Exception ex)
        {
            // Best-effort: log warning but do not propagate exception
            _logger?.LogWarning(
                ex,
                "Failed to persist {ActionType} audit event for user {UserId}",
                actionType,
                userId);

            return false;
        }
    }
}
