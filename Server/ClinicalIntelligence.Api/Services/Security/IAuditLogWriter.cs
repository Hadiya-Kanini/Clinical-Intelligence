namespace ClinicalIntelligence.Api.Services.Security;

/// <summary>
/// Abstraction for best-effort persistence of audit log events.
/// Follows DIP (Dependency Inversion Principle) to decouple audit logging from endpoints.
/// </summary>
public interface IAuditLogWriter
{
    /// <summary>
    /// Writes an audit log event with best-effort persistence.
    /// Failures are logged but do not propagate exceptions to callers.
    /// </summary>
    /// <param name="actionType">Type of action (e.g., PASSWORD_RESET_REQUESTED).</param>
    /// <param name="userId">Optional user ID associated with the event.</param>
    /// <param name="sessionId">Optional session ID associated with the event.</param>
    /// <param name="resourceType">Type of resource affected (e.g., Auth, Session).</param>
    /// <param name="resourceId">Optional ID of the affected resource.</param>
    /// <param name="ipAddress">Client IP address.</param>
    /// <param name="userAgent">Client user agent string.</param>
    /// <param name="metadata">Additional metadata object to serialize as JSON.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if the event was persisted successfully; false otherwise.</returns>
    Task<bool> WriteAsync(
        string actionType,
        Guid? userId,
        Guid? sessionId,
        string? resourceType,
        Guid? resourceId,
        string? ipAddress,
        string? userAgent,
        object? metadata,
        CancellationToken cancellationToken = default);
}
