using ClinicalIntelligence.Api.Contracts;
using ClinicalIntelligence.Api.Results;
using ClinicalIntelligence.Api.Tests.TestData;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;
using Xunit;

namespace ClinicalIntelligence.Api.Tests.Results;

public class ApiErrorResultsTests
{
    private static async Task<(int statusCode, ApiErrorResponse? body, string? contentType)> ExecuteResult(IResult result)
    {
        var services = new Microsoft.Extensions.DependencyInjection.ServiceCollection();
        services.AddLogging();
        var serviceProvider = services.BuildServiceProvider();

        var httpContext = new DefaultHttpContext
        {
            RequestServices = serviceProvider
        };
        httpContext.Response.Body = new MemoryStream();

        await result.ExecuteAsync(httpContext);

        httpContext.Response.Body.Seek(0, SeekOrigin.Begin);
        var reader = new StreamReader(httpContext.Response.Body);
        var responseBody = await reader.ReadToEndAsync();

        ApiErrorResponse? body = null;
        if (!string.IsNullOrEmpty(responseBody))
        {
            body = JsonSerializer.Deserialize<ApiErrorResponse>(responseBody);
        }

        return (httpContext.Response.StatusCode, body, httpContext.Response.ContentType);
    }

    [Fact]
    public async Task BadRequest_ReturnsStatus400WithStandardizedError()
    {
        var result = ApiErrorResults.BadRequest(
            ErrorResponseTestData.ErrorCodes.ValidationError,
            ErrorResponseTestData.ErrorMessages.InvalidInput
        );

        var (statusCode, body, contentType) = await ExecuteResult(result);

        Assert.Equal(StatusCodes.Status400BadRequest, statusCode);
        Assert.NotNull(body);
        Assert.Equal(ErrorResponseTestData.ErrorCodes.ValidationError, body.Error.Code);
        Assert.Equal(ErrorResponseTestData.ErrorMessages.InvalidInput, body.Error.Message);
        Assert.NotNull(body.Error.Details);
        Assert.Empty(body.Error.Details);
        Assert.Equal("application/json", contentType);
    }

    [Fact]
    public async Task Unauthorized_ReturnsStatus401WithStandardizedError()
    {
        var result = ApiErrorResults.Unauthorized();

        var (statusCode, body, contentType) = await ExecuteResult(result);

        Assert.Equal(StatusCodes.Status401Unauthorized, statusCode);
        Assert.NotNull(body);
        Assert.Equal("unauthorized", body.Error.Code);
        Assert.Equal("Unauthorized.", body.Error.Message);
        Assert.NotNull(body.Error.Details);
        Assert.Empty(body.Error.Details);
        Assert.Equal("application/json", contentType);
    }

    [Fact]
    public async Task Unauthorized_CustomCodeAndMessage_ReturnsCustomValues()
    {
        var result = ApiErrorResults.Unauthorized("custom_unauthorized", "Custom unauthorized message");

        var (statusCode, body, contentType) = await ExecuteResult(result);

        Assert.Equal(StatusCodes.Status401Unauthorized, statusCode);
        Assert.NotNull(body);
        Assert.Equal("custom_unauthorized", body.Error.Code);
        Assert.Equal("Custom unauthorized message", body.Error.Message);
    }

    [Fact]
    public async Task Forbidden_ReturnsStatus403WithStandardizedError()
    {
        var result = ApiErrorResults.Forbidden();

        var (statusCode, body, contentType) = await ExecuteResult(result);

        Assert.Equal(StatusCodes.Status403Forbidden, statusCode);
        Assert.NotNull(body);
        Assert.Equal("forbidden", body.Error.Code);
        Assert.Equal("Forbidden.", body.Error.Message);
        Assert.NotNull(body.Error.Details);
        Assert.Empty(body.Error.Details);
        Assert.Equal("application/json", contentType);
    }

    [Fact]
    public async Task NotFound_ReturnsStatus404WithStandardizedError()
    {
        var result = ApiErrorResults.NotFound();

        var (statusCode, body, contentType) = await ExecuteResult(result);

        Assert.Equal(StatusCodes.Status404NotFound, statusCode);
        Assert.NotNull(body);
        Assert.Equal("not_found", body.Error.Code);
        Assert.Equal("Not found.", body.Error.Message);
        Assert.NotNull(body.Error.Details);
        Assert.Empty(body.Error.Details);
        Assert.Equal("application/json", contentType);
    }

    [Fact]
    public async Task Conflict_ReturnsStatus409WithStandardizedError()
    {
        var result = ApiErrorResults.Conflict();

        var (statusCode, body, contentType) = await ExecuteResult(result);

        Assert.Equal(StatusCodes.Status409Conflict, statusCode);
        Assert.NotNull(body);
        Assert.Equal("conflict", body.Error.Code);
        Assert.Equal("Conflict.", body.Error.Message);
        Assert.NotNull(body.Error.Details);
        Assert.Empty(body.Error.Details);
        Assert.Equal("application/json", contentType);
    }

    [Fact]
    public async Task TooManyRequests_ReturnsStatus429WithStandardizedError()
    {
        var result = ApiErrorResults.TooManyRequests();

        var (statusCode, body, contentType) = await ExecuteResult(result);

        Assert.Equal(StatusCodes.Status429TooManyRequests, statusCode);
        Assert.NotNull(body);
        Assert.Equal("rate_limited", body.Error.Code);
        Assert.Equal("Too many requests.", body.Error.Message);
        Assert.NotNull(body.Error.Details);
        Assert.Empty(body.Error.Details);
        Assert.Equal("application/json", contentType);
    }

    [Fact]
    public async Task InternalServerError_ReturnsStatus500WithStandardizedError()
    {
        var result = ApiErrorResults.InternalServerError();

        var (statusCode, body, contentType) = await ExecuteResult(result);

        Assert.Equal(StatusCodes.Status500InternalServerError, statusCode);
        Assert.NotNull(body);
        Assert.Equal("internal_server_error", body.Error.Code);
        Assert.Equal("An unexpected error occurred.", body.Error.Message);
        Assert.NotNull(body.Error.Details);
        Assert.Empty(body.Error.Details);
        Assert.Equal("application/json", contentType);
    }

    [Fact]
    public async Task BadRequest_WithDetails_PopulatesDetailsArray()
    {
        var details = ErrorResponseTestData.ValidationDetails.MultipleErrors;
        var result = ApiErrorResults.BadRequest(
            ErrorResponseTestData.ErrorCodes.ValidationError,
            ErrorResponseTestData.ErrorMessages.InvalidInput,
            details
        );

        var (statusCode, body, contentType) = await ExecuteResult(result);

        Assert.Equal(StatusCodes.Status400BadRequest, statusCode);
        Assert.NotNull(body);
        Assert.NotNull(body.Error.Details);
        Assert.Equal(3, body.Error.Details.Length);
        Assert.Equal(details, body.Error.Details);
    }

    [Fact]
    public async Task BadRequest_WithEmptyCode_HandlesGracefully()
    {
        var result = ApiErrorResults.BadRequest(
            string.Empty,
            ErrorResponseTestData.ErrorMessages.InvalidInput
        );

        var (statusCode, body, contentType) = await ExecuteResult(result);

        Assert.Equal(StatusCodes.Status400BadRequest, statusCode);
        Assert.NotNull(body);
        Assert.Equal(string.Empty, body.Error.Code);
    }

    [Fact]
    public async Task BadRequest_WithNullDetails_DefaultsToEmptyArray()
    {
        var result = ApiErrorResults.BadRequest(
            ErrorResponseTestData.ErrorCodes.ValidationError,
            ErrorResponseTestData.ErrorMessages.InvalidInput,
            null
        );

        var (statusCode, body, contentType) = await ExecuteResult(result);

        Assert.Equal(StatusCodes.Status400BadRequest, statusCode);
        Assert.NotNull(body);
        Assert.NotNull(body.Error.Details);
        Assert.Empty(body.Error.Details);
    }

    [Fact]
    public async Task UnsupportedApiVersion_ReturnsCorrectErrorStructure()
    {
        var result = ApiErrorResults.UnsupportedApiVersion("v99");

        var (statusCode, body, contentType) = await ExecuteResult(result);

        Assert.Equal(StatusCodes.Status400BadRequest, statusCode);
        Assert.NotNull(body);
        Assert.Equal("unsupported_api_version", body.Error.Code);
        Assert.Equal("The requested API version is not supported.", body.Error.Message);
        Assert.NotNull(body.Error.Details);
        Assert.Single(body.Error.Details);
        Assert.Contains("requested_version:v99", body.Error.Details);
    }

    [Fact]
    public async Task Forbidden_WithCustomCodeAndMessage_ReturnsCustomValues()
    {
        var result = ApiErrorResults.Forbidden("custom_forbidden", "Custom forbidden message");

        var (statusCode, body, contentType) = await ExecuteResult(result);

        Assert.Equal(StatusCodes.Status403Forbidden, statusCode);
        Assert.NotNull(body);
        Assert.Equal("custom_forbidden", body.Error.Code);
        Assert.Equal("Custom forbidden message", body.Error.Message);
    }

    [Fact]
    public async Task NotFound_WithDetails_IncludesDetailsInResponse()
    {
        var details = new[] { "Resource ID: 12345 not found" };
        var result = ApiErrorResults.NotFound("resource_not_found", "Resource not found", details);

        var (statusCode, body, contentType) = await ExecuteResult(result);

        Assert.Equal(StatusCodes.Status404NotFound, statusCode);
        Assert.NotNull(body);
        Assert.Equal("resource_not_found", body.Error.Code);
        Assert.Single(body.Error.Details);
        Assert.Equal(details[0], body.Error.Details[0]);
    }

    [Fact]
    public async Task AllErrorResults_SetContentTypeToApplicationJson()
    {
        var results = new[]
        {
            ApiErrorResults.BadRequest("code", "message"),
            ApiErrorResults.Unauthorized(),
            ApiErrorResults.Forbidden(),
            ApiErrorResults.NotFound(),
            ApiErrorResults.Conflict(),
            ApiErrorResults.TooManyRequests(),
            ApiErrorResults.InternalServerError()
        };

        foreach (var result in results)
        {
            var (_, _, contentType) = await ExecuteResult(result);
            Assert.Equal("application/json", contentType);
        }
    }

    [Fact]
    public async Task Conflict_WithDetails_PopulatesDetailsArray()
    {
        var details = new[] { "Resource with ID 123 already exists" };
        var result = ApiErrorResults.Conflict("duplicate_resource", "Resource already exists", details);

        var (statusCode, body, contentType) = await ExecuteResult(result);

        Assert.Equal(StatusCodes.Status409Conflict, statusCode);
        Assert.NotNull(body);
        Assert.Equal("duplicate_resource", body.Error.Code);
        Assert.Single(body.Error.Details);
        Assert.Equal(details[0], body.Error.Details[0]);
    }

    [Fact]
    public async Task TooManyRequests_WithCustomMessage_ReturnsCustomMessage()
    {
        var result = ApiErrorResults.TooManyRequests("rate_limit_exceeded", "You have exceeded the rate limit. Try again in 60 seconds.");

        var (statusCode, body, contentType) = await ExecuteResult(result);

        Assert.Equal(StatusCodes.Status429TooManyRequests, statusCode);
        Assert.NotNull(body);
        Assert.Equal("rate_limit_exceeded", body.Error.Code);
        Assert.Equal("You have exceeded the rate limit. Try again in 60 seconds.", body.Error.Message);
    }
}
