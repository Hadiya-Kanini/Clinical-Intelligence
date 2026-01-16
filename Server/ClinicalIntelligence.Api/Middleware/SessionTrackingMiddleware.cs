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
        _logger.LogInformation("SessionTrackingMiddleware constructor called");
    }

    public async Task InvokeAsync(HttpContext context, ApplicationDbContext dbContext, IAuditLogWriter auditLogWriter)
    {
        _logger.LogInformation("SessionTracking: Middleware invoked for path: {Path}", context.Request.Path);
        
        // Skip session tracking for Swagger endpoints, health checks, and auth endpoints
        var path = context.Request.Path.Value?.ToLowerInvariant();
        if (path != null && (path.StartsWith("/swagger") || path.StartsWith("/health") || path == "/" || path.StartsWith("/api/v1/auth")))
        {
            _logger.LogDebug("SessionTracking: Skipping - path excluded: {Path}", context.Request.Path);
            await _next(context);
            return;
        }
        
        try
        {
            // Skip session tracking for unauthenticated requests
            if (context.User.Identity?.IsAuthenticated != true)
            {
                _logger.LogDebug("SessionTracking: Skipping - user not authenticated");
                await _next(context);
                return;
            }

            _logger.LogInformation("SessionTracking: User is authenticated, checking session...");

            // Extract session ID from JWT claims
            var sessionIdClaim = context.User.FindFirst("sid")?.Value;
            _logger.LogInformation("SessionTracking: Session ID claim = {SessionId}", sessionIdClaim);
            
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
            if (session.ExpiresAt.ToUniversalTime() <= DateTime.UtcNow)
            {
                _logger.LogInformation("Expired session access attempt: {SessionId}, ExpiresAt={ExpiresAt}, UtcNow={UtcNow}", 
                    sessionId, session.ExpiresAt.ToUniversalTime(), DateTime.UtcNow);
                await ApiErrorResults.Unauthorized(
                    code: "session_expired",
                    message: "Session has expired. Please log in again."
                ).ExecuteAsync(context);
                return;
            }

            // Check inactivity timeout (15 minutes since last activity)
            var lastActivity = (session.LastActivityAt ?? session.CreatedAt).ToUniversalTime();
            var inactivityThreshold = DateTime.UtcNow.AddMinutes(-SessionInactivityTimeoutMinutes);

            _logger.LogDebug("Inactivity check: LastActivity={LastActivity}, Threshold={Threshold}, IsExpired={IsExpired}",
                lastActivity, inactivityThreshold, lastActivity < inactivityThreshold);

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
            
            // Store user ID in HttpContext for downstream use (e.g., dashboard)
            context.Items["UserId"] = session.UserId;

            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "SessionTracking middleware error for path: {Path}", context.Request.Path);
            throw;
        }
    }
}
