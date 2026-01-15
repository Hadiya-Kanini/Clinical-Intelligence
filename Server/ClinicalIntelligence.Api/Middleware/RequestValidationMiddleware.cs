using System.Text.Json;
using ClinicalIntelligence.Api.Results;
using ClinicalIntelligence.Api.Services.Security;

namespace ClinicalIntelligence.Api.Middleware;

/// <summary>
/// Middleware that validates incoming HTTP requests for security and compliance.
/// Validates query parameters and selected headers for injection patterns.
/// JSON body validation is intentionally scoped to avoid false positives on
/// legitimate clinical content and password fields.
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

        // Validate query parameters using InputValidationPolicy
        foreach (var queryParam in context.Request.Query)
        {
            foreach (var value in queryParam.Value)
            {
                if (!InputValidationPolicy.IsQueryParameterSafe(value, out var detectedPattern))
                {
                    // Log safely without exposing raw input values (FR-009g)
                    _logger.LogWarning(
                        "Suspicious input detected in query parameter. Path: {Path}, Parameter: {Param}, Pattern: {Pattern}",
                        context.Request.Path,
                        queryParam.Key,
                        detectedPattern);
                    
                    await ApiErrorResults.BadRequest(
                        "invalid_input",
                        "Invalid characters detected in request parameters"
                    ).ExecuteAsync(context);
                    return;
                }
            }
        }

        // Validate selected custom headers (skip common browser headers)
        foreach (var header in context.Request.Headers)
        {
            if (!InputValidationPolicy.IsHeaderValueSafe(header.Key, header.Value, out var detectedPattern))
            {
                _logger.LogWarning(
                    "Suspicious input detected in header. Path: {Path}, Header: {Header}, Pattern: {Pattern}",
                    context.Request.Path,
                    header.Key,
                    detectedPattern);
                
                await ApiErrorResults.BadRequest(
                    "invalid_input",
                    "Invalid characters detected in request headers"
                ).ExecuteAsync(context);
                return;
            }
        }

        await _next(context);
    }
}
