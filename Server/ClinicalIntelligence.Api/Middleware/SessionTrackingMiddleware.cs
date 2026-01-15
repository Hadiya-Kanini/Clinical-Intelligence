using ClinicalIntelligence.Api.Data;
using ClinicalIntelligence.Api.Results;
using ClinicalIntelligence.Api.Authorization;
using ClinicalIntelligence.Api.Services.Security;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace ClinicalIntelligence.Api.Middleware;

/// <summary>
/// Middleware that enforces server-side session validity and inactivity timeout.
/// Runs after authentication to validate session state and update LastActivityAt.
/// </summary>
public class SessionTrackingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<SessionTrackingMiddleware> _logger;

    /// <summary>
    /// Session inactivity timeout in minutes. Default is 15 minutes per US_012 requirements.
    /// </summary>
    private const int SessionInactivityTimeoutMinutes = 15;

    public SessionTrackingMiddleware(RequestDelegate next, ILogger<SessionTrackingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, ApplicationDbContext dbContext, IAuditLogWriter auditLogWriter)
    {
        // Skip session tracking for unauthenticated requests
        if (context.User.Identity?.IsAuthenticated != true)
        {
            await _next(context);
            return;
        }

        // Extract session ID from JWT claims
        var sessionIdClaim = context.User.FindFirst("sid")?.Value;
        
        if (string.IsNullOrEmpty(sessionIdClaim) || !Guid.TryParse(sessionIdClaim, out var sessionId))
        {
            _logger.LogWarning("Authenticated request missing valid session ID claim");
            await ApiErrorResults.Unauthorized(
                code: "session_expired",
                message: "Session is invalid or expired. Please log in again."
            ).ExecuteAsync(context);
            return;
        }

        // Load session from database
        var session = await dbContext.Sessions
            .FirstOrDefaultAsync(s => s.Id == sessionId);

        if (session == null)
        {
            _logger.LogWarning("Session not found: {SessionId}", sessionId);
            await ApiErrorResults.Unauthorized(
                code: "session_expired",
                message: "Session not found. Please log in again."
            ).ExecuteAsync(context);
            return;
        }

        // Check if session is revoked (e.g., due to login from another device)
        if (session.IsRevoked)
        {
            _logger.LogInformation("Revoked session access attempt: {SessionId}", sessionId);
            await ApiErrorResults.Unauthorized(
                code: "session_invalidated",
                message: "Your session was invalidated because you signed in on another device."
            ).ExecuteAsync(context);
            return;
        }

        // Check if session has expired (absolute expiration)
        if (session.ExpiresAt <= DateTime.UtcNow)
        {
            _logger.LogInformation("Expired session access attempt: {SessionId}", sessionId);
            await ApiErrorResults.Unauthorized(
                code: "session_expired",
                message: "Session has expired. Please log in again."
            ).ExecuteAsync(context);
            return;
        }

        // Check inactivity timeout (15 minutes since last activity)
        var lastActivity = session.LastActivityAt ?? session.CreatedAt;
        var inactivityThreshold = DateTime.UtcNow.AddMinutes(-SessionInactivityTimeoutMinutes);

        if (lastActivity < inactivityThreshold)
        {
            _logger.LogInformation(
                "Session inactivity timeout: {SessionId}, LastActivity: {LastActivity}, Threshold: {Threshold}",
                sessionId, lastActivity, inactivityThreshold);
            await ApiErrorResults.Unauthorized(
                code: "session_expired",
                message: "Session expired due to inactivity. Please log in again."
            ).ExecuteAsync(context);
            return;
        }

        // US_033 TASK_002: Detect role mismatch between JWT and database
        // If user's role was changed mid-session, invalidate the session
        var jwtRoleClaim = context.User.FindFirst(ClaimTypes.Role)?.Value 
                           ?? context.User.FindFirst("role")?.Value;

        if (!string.IsNullOrEmpty(jwtRoleClaim))
        {
            // Load user to get current DB role
            var user = await dbContext.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == session.UserId);

            if (user != null)
            {
                // Compare roles (case-insensitive)
                var dbRole = Roles.Normalize(user.Role);
                var tokenRole = Roles.Normalize(jwtRoleClaim);

                if (dbRole != null && tokenRole != null && !string.Equals(dbRole, tokenRole, StringComparison.Ordinal))
                {
                    _logger.LogInformation(
                        "Role mismatch detected for session {SessionId}: JWT role '{JwtRole}' differs from DB role '{DbRole}'. Invalidating session.",
                        sessionId, tokenRole, dbRole);

                    // Revoke the session
                    session.IsRevoked = true;
                    await dbContext.SaveChangesAsync();

                    // Audit log: ROLE_CHANGE_SESSION_INVALIDATED (best-effort)
                    _ = auditLogWriter.WriteAsync(
                        actionType: "ROLE_CHANGE_SESSION_INVALIDATED",
                        userId: session.UserId,
                        sessionId: sessionId,
                        resourceType: "Session",
                        resourceId: sessionId,
                        ipAddress: context.Connection.RemoteIpAddress?.ToString(),
                        userAgent: context.Request.Headers.UserAgent.ToString(),
                        metadata: new { previousRole = tokenRole, newRole = dbRole },
                        cancellationToken: context.RequestAborted);

                    await ApiErrorResults.Unauthorized(
                        code: "session_invalidated",
                        message: "Your session was invalidated due to a role change. Please log in again."
                    ).ExecuteAsync(context);
                    return;
                }
            }
        }

        // Update LastActivityAt for valid session (sliding window)
        session.LastActivityAt = DateTime.UtcNow;
        
        // Optionally slide ExpiresAt forward to maintain inactivity timeout window
        session.ExpiresAt = DateTime.UtcNow.AddMinutes(SessionInactivityTimeoutMinutes);
        
        await dbContext.SaveChangesAsync();

        // Store session ID in HttpContext for downstream use (e.g., logout)
        context.Items["SessionId"] = sessionId;

        await _next(context);
    }
}
