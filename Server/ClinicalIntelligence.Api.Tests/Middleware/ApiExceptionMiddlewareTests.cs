using ClinicalIntelligence.Api.Contracts;
using ClinicalIntelligence.Api.Middleware;
using ClinicalIntelligence.Api.Tests.Mocks;
using ClinicalIntelligence.Api.Tests.TestData;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using System.Text.Json;
using Xunit;

namespace ClinicalIntelligence.Api.Tests.Middleware;

public class ApiExceptionMiddlewareTests
{
    private readonly Mock<ILogger<ApiExceptionMiddleware>> _mockLogger;

    public ApiExceptionMiddlewareTests()
    {
        _mockLogger = new Mock<ILogger<ApiExceptionMiddleware>>();
    }

    private static DefaultHttpContext CreateHttpContext()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        var serviceProvider = services.BuildServiceProvider();

        var context = new DefaultHttpContext
        {
            RequestServices = serviceProvider
        };
        context.Response.Body = new MemoryStream();
        context.Request.Path = "/api/v1/test";
        context.Request.Method = "GET";
        return context;
    }

    private static async Task<(int statusCode, ApiErrorResponse? body)> GetResponseFromContext(HttpContext context)
    {
        context.Response.Body.Seek(0, SeekOrigin.Begin);
        var reader = new StreamReader(context.Response.Body);
        var responseBody = await reader.ReadToEndAsync();

        ApiErrorResponse? body = null;
        if (!string.IsNullOrEmpty(responseBody))
        {
            body = JsonSerializer.Deserialize<ApiErrorResponse>(responseBody);
        }

        return (context.Response.StatusCode, body);
    }

    [Fact]
    public async Task InvokeAsync_UnhandledException_Returns500WithStandardizedError()
    {
        var context = CreateHttpContext();
        RequestDelegate next = _ => throw new InvalidOperationException(ErrorResponseTestData.ExceptionMessages.DatabaseConnectionFailed);
        var middleware = new ApiExceptionMiddleware(next, _mockLogger.Object);

        await middleware.InvokeAsync(context);

        var (statusCode, body) = await GetResponseFromContext(context);

        Assert.Equal(StatusCodes.Status500InternalServerError, statusCode);
        Assert.NotNull(body);
        Assert.Equal("internal_server_error", body.Error.Code);
        Assert.Equal("An unexpected error occurred.", body.Error.Message);
        Assert.NotNull(body.Error.Details);
        Assert.Empty(body.Error.Details);
    }

    [Fact]
    public async Task InvokeAsync_UnhandledException_DoesNotLeakStackTrace()
    {
        var context = CreateHttpContext();
        RequestDelegate next = _ => Task.Run(() => MockExceptionEndpoint.ThrowInvalidOperationException());
        var middleware = new ApiExceptionMiddleware(next, _mockLogger.Object);

        await middleware.InvokeAsync(context);

        context.Response.Body.Seek(0, SeekOrigin.Begin);
        var reader = new StreamReader(context.Response.Body);
        var responseBody = await reader.ReadToEndAsync();

        foreach (var pattern in ErrorResponseTestData.SensitivePatterns.StackTracePatterns)
        {
            Assert.DoesNotContain(pattern, responseBody, StringComparison.OrdinalIgnoreCase);
        }
    }

    [Fact]
    public async Task InvokeAsync_ExceptionWithSensitiveData_SanitizesResponse()
    {
        var context = CreateHttpContext();
        RequestDelegate next = _ => Task.Run(() => MockExceptionEndpoint.ThrowExceptionWithSensitiveData());
        var middleware = new ApiExceptionMiddleware(next, _mockLogger.Object);

        await middleware.InvokeAsync(context);

        context.Response.Body.Seek(0, SeekOrigin.Begin);
        var reader = new StreamReader(context.Response.Body);
        var responseBody = await reader.ReadToEndAsync();

        foreach (var pattern in ErrorResponseTestData.SensitivePatterns.ConnectionStringPatterns)
        {
            Assert.DoesNotContain(pattern, responseBody, StringComparison.OrdinalIgnoreCase);
        }

        var (statusCode, body) = await GetResponseFromContext(context);
        Assert.Equal(StatusCodes.Status500InternalServerError, statusCode);
        Assert.NotNull(body);
        Assert.Equal("An unexpected error occurred.", body.Error.Message);
    }

    [Fact]
    public async Task InvokeAsync_ExceptionWithEnvironmentVariable_DoesNotLeakEnvVars()
    {
        var context = CreateHttpContext();
        RequestDelegate next = _ => Task.Run(() => MockExceptionEndpoint.ThrowExceptionWithEnvironmentVariable());
        var middleware = new ApiExceptionMiddleware(next, _mockLogger.Object);

        await middleware.InvokeAsync(context);

        context.Response.Body.Seek(0, SeekOrigin.Begin);
        var reader = new StreamReader(context.Response.Body);
        var responseBody = await reader.ReadToEndAsync();

        foreach (var pattern in ErrorResponseTestData.SensitivePatterns.EnvironmentVariablePatterns)
        {
            Assert.DoesNotContain(pattern, responseBody, StringComparison.OrdinalIgnoreCase);
        }
    }

    [Fact]
    public async Task InvokeAsync_SuccessfulRequest_PassesThrough()
    {
        var context = CreateHttpContext();
        var nextCalled = false;
        RequestDelegate next = ctx =>
        {
            nextCalled = true;
            ctx.Response.StatusCode = StatusCodes.Status200OK;
            return Task.CompletedTask;
        };
        var middleware = new ApiExceptionMiddleware(next, _mockLogger.Object);

        await middleware.InvokeAsync(context);

        Assert.True(nextCalled);
        Assert.Equal(StatusCodes.Status200OK, context.Response.StatusCode);
    }

    [Fact]
    public async Task InvokeAsync_Exception_LogsExceptionDetails()
    {
        var context = CreateHttpContext();
        context.Items["CorrelationId"] = "test-correlation-id";
        var exception = new InvalidOperationException(ErrorResponseTestData.ExceptionMessages.DatabaseConnectionFailed);
        RequestDelegate next = _ => throw exception;
        var middleware = new ApiExceptionMiddleware(next, _mockLogger.Object);

        await middleware.InvokeAsync(context);

        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("test-correlation-id")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task InvokeAsync_ExceptionWithoutCorrelationId_LogsWithUnknown()
    {
        var context = CreateHttpContext();
        var exception = new InvalidOperationException(ErrorResponseTestData.ExceptionMessages.DatabaseConnectionFailed);
        RequestDelegate next = _ => throw exception;
        var middleware = new ApiExceptionMiddleware(next, _mockLogger.Object);

        await middleware.InvokeAsync(context);

        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("unknown")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task InvokeAsync_ResponseAlreadyStarted_LogsException()
    {
        var context = CreateHttpContext();
        var exception = new InvalidOperationException(ErrorResponseTestData.ExceptionMessages.DatabaseConnectionFailed);
        RequestDelegate next = async ctx =>
        {
            await ctx.Response.StartAsync();
            await ctx.Response.WriteAsync("Started");
            throw exception;
        };
        var middleware = new ApiExceptionMiddleware(next, _mockLogger.Object);

        await middleware.InvokeAsync(context);

        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task InvokeAsync_ArgumentNullException_Returns500()
    {
        var context = CreateHttpContext();
        RequestDelegate next = _ => Task.Run(() => MockExceptionEndpoint.ThrowArgumentNullException());
        var middleware = new ApiExceptionMiddleware(next, _mockLogger.Object);

        await middleware.InvokeAsync(context);

        var (statusCode, body) = await GetResponseFromContext(context);

        Assert.Equal(StatusCodes.Status500InternalServerError, statusCode);
        Assert.NotNull(body);
        Assert.Equal("internal_server_error", body.Error.Code);
    }

    [Fact]
    public async Task InvokeAsync_UnauthorizedAccessException_Returns500()
    {
        var context = CreateHttpContext();
        RequestDelegate next = _ => Task.Run(() => MockExceptionEndpoint.ThrowUnauthorizedAccessException());
        var middleware = new ApiExceptionMiddleware(next, _mockLogger.Object);

        await middleware.InvokeAsync(context);

        var (statusCode, body) = await GetResponseFromContext(context);

        Assert.Equal(StatusCodes.Status500InternalServerError, statusCode);
        Assert.NotNull(body);
        Assert.Equal("internal_server_error", body.Error.Code);
    }

    [Fact]
    public async Task InvokeAsync_AsyncException_HandlesCorrectly()
    {
        var context = CreateHttpContext();
        RequestDelegate next = _ => MockExceptionEndpoint.ThrowAsyncException();
        var middleware = new ApiExceptionMiddleware(next, _mockLogger.Object);

        await middleware.InvokeAsync(context);

        var (statusCode, body) = await GetResponseFromContext(context);

        Assert.Equal(StatusCodes.Status500InternalServerError, statusCode);
        Assert.NotNull(body);
        Assert.Equal("internal_server_error", body.Error.Code);
    }

    [Fact]
    public async Task InvokeAsync_MultipleExceptions_HandlesFirstException()
    {
        var context = CreateHttpContext();
        var callCount = 0;
        RequestDelegate next = _ =>
        {
            callCount++;
            throw new InvalidOperationException($"Exception {callCount}");
        };
        var middleware = new ApiExceptionMiddleware(next, _mockLogger.Object);

        await middleware.InvokeAsync(context);

        Assert.Equal(1, callCount);
        var (statusCode, body) = await GetResponseFromContext(context);
        Assert.Equal(StatusCodes.Status500InternalServerError, statusCode);
    }

    [Fact]
    public async Task InvokeAsync_ExceptionLogging_IncludesPathAndMethod()
    {
        var context = CreateHttpContext();
        context.Request.Path = "/api/v1/test-endpoint";
        context.Request.Method = "POST";
        var exception = new InvalidOperationException("Test exception");
        RequestDelegate next = _ => throw exception;
        var middleware = new ApiExceptionMiddleware(next, _mockLogger.Object);

        await middleware.InvokeAsync(context);

        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => 
                    v.ToString()!.Contains("/api/v1/test-endpoint") && 
                    v.ToString()!.Contains("POST")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task InvokeAsync_ClearsResponseBeforeWritingError()
    {
        var context = CreateHttpContext();
        context.Response.Headers.Append("X-Custom-Header", "test-value");
        RequestDelegate next = _ => Task.FromException(new InvalidOperationException("Test exception"));
        var middleware = new ApiExceptionMiddleware(next, _mockLogger.Object);

        await middleware.InvokeAsync(context);

        var (statusCode, _) = await GetResponseFromContext(context);
        Assert.Equal(StatusCodes.Status500InternalServerError, statusCode);
    }

    [Fact]
    public async Task InvokeAsync_ResponseContentType_IsApplicationJson()
    {
        var context = CreateHttpContext();
        RequestDelegate next = _ => throw new InvalidOperationException("Test exception");
        var middleware = new ApiExceptionMiddleware(next, _mockLogger.Object);

        await middleware.InvokeAsync(context);

        Assert.Equal("application/json", context.Response.ContentType);
    }
}
