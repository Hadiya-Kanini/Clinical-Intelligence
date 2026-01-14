using ClinicalIntelligence.Api.Results;

namespace ClinicalIntelligence.Api.Middleware;

/// <summary>
/// Middleware that handles unhandled exceptions and returns consistent error responses.
/// </summary>
public class ApiExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ApiExceptionMiddleware> _logger;

    /// <summary>
    /// Initializes a new instance of the ApiExceptionMiddleware.
    /// </summary>
    /// <param name="next">The next middleware in the pipeline.</param>
    /// <param name="logger">The logger instance.</param>
    public ApiExceptionMiddleware(RequestDelegate next, ILogger<ApiExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    /// <summary>
    /// Processes the HTTP request and handles any unhandled exceptions.
    /// </summary>
    /// <param name="context">The HTTP context.</param>
    /// <returns>A task representing the middleware execution.</returns>
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            var correlationId = context.Items["CorrelationId"]?.ToString() ?? "unknown";
            
            _logger.LogError(ex, 
                "An unhandled exception occurred. CorrelationId: {CorrelationId}, Path: {Path}, Method: {Method}", 
                correlationId, context.Request.Path, context.Request.Method);

            if (context.Response.HasStarted)
            {
                throw;
            }

            context.Response.Clear();
            var errorResult = ApiErrorResults.InternalServerError();
            await errorResult.ExecuteAsync(context);
        }
    }
}
