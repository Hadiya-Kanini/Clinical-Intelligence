using System.Diagnostics;

namespace ClinicalIntelligence.Api.Middleware;

/// <summary>
/// Middleware that adds correlation IDs to HTTP requests for distributed tracing.
/// </summary>
public class CorrelationIdMiddleware
{
    private readonly RequestDelegate _next;
    private const string CorrelationIdHeaderName = "X-Correlation-ID";
    private const string CorrelationIdLogPropertyName = "CorrelationId";

    /// <summary>
    /// Initializes a new instance of the CorrelationIdMiddleware.
    /// </summary>
    /// <param name="next">The next middleware in the pipeline.</param>
    public CorrelationIdMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    /// <summary>
    /// Processes the HTTP request and adds correlation ID tracking.
    /// </summary>
    /// <param name="context">The HTTP context.</param>
    /// <returns>A task representing the middleware execution.</returns>
    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = GetOrCreateCorrelationId(context);
        
        // Add correlation ID to response header
        context.Response.OnStarting(() =>
        {
            if (!context.Response.Headers.ContainsKey(CorrelationIdHeaderName))
            {
                context.Response.Headers[CorrelationIdHeaderName] = correlationId;
            }
            return Task.CompletedTask;
        });

        // Add correlation ID to activity for distributed tracing
        Activity.Current?.SetTag(CorrelationIdLogPropertyName, correlationId);

        await _next(context);
    }

    /// <summary>
    /// Gets an existing correlation ID from the request or creates a new one.
    /// </summary>
    /// <param name="context">The HTTP context.</param>
    /// <returns>The correlation ID for the request.</returns>
    private static string GetOrCreateCorrelationId(HttpContext context)
    {
        // Check if correlation ID is already present in request header
        if (context.Request.Headers.TryGetValue(CorrelationIdHeaderName, out var correlationIdValue))
        {
            var correlationId = correlationIdValue.FirstOrDefault();
            if (!string.IsNullOrEmpty(correlationId))
            {
                return correlationId;
            }
        }

        // Generate new correlation ID
        var newCorrelationId = Guid.NewGuid().ToString("N")[..8]; // Short format for readability
        context.Items[CorrelationIdLogPropertyName] = newCorrelationId;
        return newCorrelationId;
    }
}
