using System.Text.Json;
using ClinicalIntelligence.Api.Results;

namespace ClinicalIntelligence.Api.Middleware;

/// <summary>
/// Middleware that validates incoming HTTP requests for security and compliance.
/// </summary>
public class RequestValidationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestValidationMiddleware> _logger;

    /// <summary>
    /// Initializes a new instance of the RequestValidationMiddleware.
    /// </summary>
    /// <param name="next">The next middleware in the pipeline.</param>
    /// <param name="logger">The logger instance.</param>
    public RequestValidationMiddleware(RequestDelegate next, ILogger<RequestValidationMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    /// <summary>
    /// Processes the HTTP request and performs security validation.
    /// </summary>
    /// <param name="context">The HTTP context.</param>
    /// <returns>A task representing the middleware execution.</returns>
    public async Task InvokeAsync(HttpContext context)
    {
        // Skip validation for health checks and Swagger
        if (context.Request.Path.StartsWithSegments("/health") || 
            context.Request.Path.StartsWithSegments("/swagger"))
        {
            await _next(context);
            return;
        }

        // Validate content type for POST/PUT requests
        if (context.Request.Method.Equals("POST", StringComparison.OrdinalIgnoreCase) ||
            context.Request.Method.Equals("PUT", StringComparison.OrdinalIgnoreCase))
        {
            if (!context.Request.HasJsonContentType())
            {
                _logger.LogWarning("Invalid content type for {Method} request to {Path}", 
                    context.Request.Method, context.Request.Path);
                
                await ApiErrorResults.BadRequest(
                    "invalid_content_type",
                    "Request must have Content-Type: application/json"
                ).ExecuteAsync(context);
                return;
            }

            // Validate request body size
            if (context.Request.ContentLength > 10 * 1024 * 1024) // 10MB limit
            {
                _logger.LogWarning("Request body too large: {Size} bytes", context.Request.ContentLength);
                
                await ApiErrorResults.BadRequest(
                    "request_too_large",
                    "Request body size exceeds maximum allowed limit of 10MB"
                ).ExecuteAsync(context);
                return;
            }
        }

        // Validate query parameters for SQL injection attempts
        foreach (var queryParam in context.Request.Query)
        {
            if (ContainsSqlInjectionPatterns(queryParam.Value))
            {
                _logger.LogWarning("Potential SQL injection detected in query parameter {Param}: {Value}", 
                    queryParam.Key, queryParam.Value);
                
                await ApiErrorResults.BadRequest(
                    "invalid_input",
                    "Invalid characters detected in request parameters"
                ).ExecuteAsync(context);
                return;
            }
        }

        await _next(context);
    }

    /// <summary>
    /// Checks if a value contains potential SQL injection patterns.
    /// </summary>
    /// <param name="value">The value to check.</param>
    /// <returns>True if suspicious patterns are found, false otherwise.</returns>
    private static bool ContainsSqlInjectionPatterns(string? value)
    {
        if (string.IsNullOrEmpty(value))
            return false;

        var suspiciousPatterns = new[]
        {
            "drop table", "delete from", "insert into", "update set", 
            "union select", "exec(", "script>", "<script", "--", "/*", "*/",
            "xp_", "sp_", "0x", "waitfor delay", "benchmark("
        };

        var lowerValue = value.ToLowerInvariant();
        return suspiciousPatterns.Any(pattern => lowerValue.Contains(pattern));
    }
}
