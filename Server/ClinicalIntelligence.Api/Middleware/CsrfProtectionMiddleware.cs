using ClinicalIntelligence.Api.Data;
using ClinicalIntelligence.Api.Results;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace ClinicalIntelligence.Api.Middleware;

/// <summary>
/// Middleware that enforces CSRF token validation for state-changing requests (POST, PUT, PATCH, DELETE)
/// when cookie-based authentication is in use. Validates X-CSRF-TOKEN header against per-session token.
/// </summary>
public class CsrfProtectionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<CsrfProtectionMiddleware> _logger;

    /// <summary>
    /// Header name for CSRF token.
    /// </summary>
    public const string CsrfHeaderName = "X-CSRF-TOKEN";

    /// <summary>
    /// HTTP methods that require CSRF validation (state-changing methods).
    /// </summary>
    private static readonly HashSet<string> StateChangingMethods = new(StringComparer.OrdinalIgnoreCase)
    {
        "POST", "PUT", "PATCH", "DELETE"
    };

    /// <summary>
    /// Endpoints exempt from CSRF validation (e.g., login which establishes the session).
    /// </summary>
    private static readonly HashSet<string> ExemptEndpoints = new(StringComparer.OrdinalIgnoreCase)
    {
        "/api/v1/auth/login",
        "/api/v1/auth/forgot-password",
        "/api/v1/auth/reset-password"
    };

    public CsrfProtectionMiddleware(RequestDelegate next, ILogger<CsrfProtectionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, ApplicationDbContext dbContext)
    {
        var path = context.Request.Path.Value ?? string.Empty;
        var method = context.Request.Method;

        // Skip CSRF validation for non-state-changing methods
        if (!StateChangingMethods.Contains(method))
        {
            await _next(context);
            return;
        }

        // Skip CSRF validation for exempt endpoints
        if (ExemptEndpoints.Contains(path))
        {
            await _next(context);
            return;
        }

        // Skip CSRF validation for unauthenticated requests (auth middleware handles this)
        if (context.User.Identity?.IsAuthenticated != true)
        {
            await _next(context);
            return;
        }

        // Extract session ID from JWT claims
        var sessionIdClaim = context.User.FindFirst("sid")?.Value;
        
        if (string.IsNullOrEmpty(sessionIdClaim) || !Guid.TryParse(sessionIdClaim, out var sessionId))
        {
            _logger.LogWarning("CSRF validation failed: missing session ID claim for {Path}", path);
            await ApiErrorResults.Forbidden(
                code: "csrf_validation_failed",
                message: "CSRF validation failed. Session is invalid."
            ).ExecuteAsync(context);
            return;
        }

        // Get CSRF token from header
        var csrfToken = context.Request.Headers[CsrfHeaderName].FirstOrDefault();

        if (string.IsNullOrEmpty(csrfToken))
        {
            _logger.LogWarning("CSRF validation failed: missing X-CSRF-TOKEN header for {Path}", path);
            await ApiErrorResults.Forbidden(
                code: "csrf_token_missing",
                message: "CSRF token is required for this request."
            ).ExecuteAsync(context);
            return;
        }

        // Load session and validate CSRF token
        var session = await dbContext.Sessions
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == sessionId);

        if (session == null)
        {
            _logger.LogWarning("CSRF validation failed: session not found {SessionId}", sessionId);
            await ApiErrorResults.Forbidden(
                code: "csrf_validation_failed",
                message: "CSRF validation failed. Session not found."
            ).ExecuteAsync(context);
            return;
        }

        if (string.IsNullOrEmpty(session.CsrfTokenHash))
        {
            _logger.LogWarning("CSRF validation failed: no CSRF token hash stored for session {SessionId}", sessionId);
            await ApiErrorResults.Forbidden(
                code: "csrf_validation_failed",
                message: "CSRF validation failed. Please refresh your session."
            ).ExecuteAsync(context);
            return;
        }

        // Hash the provided token and compare with stored hash
        var providedTokenHash = ComputeTokenHash(csrfToken);

        if (!CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(providedTokenHash),
            Encoding.UTF8.GetBytes(session.CsrfTokenHash)))
        {
            _logger.LogWarning("CSRF validation failed: token mismatch for session {SessionId}", sessionId);
            await ApiErrorResults.Forbidden(
                code: "csrf_token_invalid",
                message: "CSRF token is invalid or expired."
            ).ExecuteAsync(context);
            return;
        }

        // CSRF validation passed
        await _next(context);
    }

    /// <summary>
    /// Computes SHA-256 hash of a CSRF token.
    /// </summary>
    public static string ComputeTokenHash(string token)
    {
        var hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(token));
        return Convert.ToHexString(hashBytes).ToLowerInvariant();
    }

    /// <summary>
    /// Generates a cryptographically secure CSRF token.
    /// </summary>
    public static string GenerateToken()
    {
        var tokenBytes = RandomNumberGenerator.GetBytes(32);
        return Convert.ToBase64String(tokenBytes);
    }
}
