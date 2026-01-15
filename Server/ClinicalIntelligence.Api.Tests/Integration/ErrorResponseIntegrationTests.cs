using ClinicalIntelligence.Api.Contracts;
using ClinicalIntelligence.Api.Tests.TestData;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Text.Json;
using Xunit;

namespace ClinicalIntelligence.Api.Tests.Integration;

public class ErrorResponseIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public ErrorResponseIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    private static async Task<(HttpStatusCode statusCode, ApiErrorResponse? body)> GetErrorResponse(HttpResponseMessage response)
    {
        var content = await response.Content.ReadAsStringAsync();
        ApiErrorResponse? body = null;
        
        if (!string.IsNullOrEmpty(content))
        {
            body = JsonSerializer.Deserialize<ApiErrorResponse>(content);
        }

        return (response.StatusCode, body);
    }

    private static void AssertStandardizedErrorStructure(ApiErrorResponse? body, string expectedCode)
    {
        Assert.NotNull(body);
        Assert.NotNull(body.Error);
        Assert.NotNull(body.Error.Code);
        Assert.NotNull(body.Error.Message);
        Assert.NotNull(body.Error.Details);
        Assert.IsType<string[]>(body.Error.Details);
        Assert.Equal(expectedCode, body.Error.Code);
    }

    [Fact]
    public async Task UnsupportedApiVersion_ReturnsStandardizedError()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/v2/ping");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        
        var (statusCode, body) = await GetErrorResponse(response);
        AssertStandardizedErrorStructure(body, "unsupported_api_version");
        Assert.Equal("The requested API version is not supported.", body!.Error.Message);
        Assert.NotEmpty(body.Error.Details);
        Assert.Contains(body.Error.Details, d => d.Contains("v2"));
    }

    [Fact]
    public async Task UnsupportedApiVersion_V3_ReturnsStandardizedError()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/v3/test");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        
        var (statusCode, body) = await GetErrorResponse(response);
        AssertStandardizedErrorStructure(body, "unsupported_api_version");
        Assert.Contains(body!.Error.Details, d => d.Contains("v3"));
    }

    [Fact]
    public async Task UnsupportedApiVersion_V99_ReturnsStandardizedError()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/v99/endpoint");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        
        var (statusCode, body) = await GetErrorResponse(response);
        AssertStandardizedErrorStructure(body, "unsupported_api_version");
    }

    [Fact]
    public async Task UnauthorizedEndpoint_ReturnsStandardizedError()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/v1/ping");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        
        var content = await response.Content.ReadAsStringAsync();
        
        if (!string.IsNullOrEmpty(content))
        {
            var body = JsonSerializer.Deserialize<ApiErrorResponse>(content);
            AssertStandardizedErrorStructure(body, "unauthorized");
        }
    }

    [Fact]
    public async Task ErrorResponse_HasCorrectContentType()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/v2/ping");

        Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    public async Task ErrorResponse_DetailsIsArray()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/v2/ping");

        var content = await response.Content.ReadAsStringAsync();
        var jsonDoc = JsonDocument.Parse(content);
        var root = jsonDoc.RootElement;

        Assert.True(root.TryGetProperty("error", out var error));
        Assert.True(error.TryGetProperty("details", out var details));
        Assert.Equal(JsonValueKind.Array, details.ValueKind);
    }

    [Fact]
    public async Task MultipleUnsupportedVersionRequests_AllReturnStandardizedError()
    {
        var client = _factory.CreateClient();
        var versions = new[] { "v0", "v2", "v5", "v10", "v99" };

        foreach (var version in versions)
        {
            var response = await client.GetAsync($"/api/{version}/test");

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            
            var (statusCode, body) = await GetErrorResponse(response);
            AssertStandardizedErrorStructure(body, "unsupported_api_version");
            Assert.Contains(body!.Error.Details, d => d.Contains(version));
        }
    }

    [Fact]
    public async Task ErrorResponse_DoesNotContainStackTrace()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/v2/ping");

        var content = await response.Content.ReadAsStringAsync();

        foreach (var pattern in ErrorResponseTestData.SensitivePatterns.StackTracePatterns)
        {
            Assert.DoesNotContain(pattern, content, StringComparison.OrdinalIgnoreCase);
        }
    }

    [Fact]
    public async Task ErrorResponse_DoesNotContainSensitiveData()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/v2/ping");

        var content = await response.Content.ReadAsStringAsync();

        foreach (var pattern in ErrorResponseTestData.SensitivePatterns.ConnectionStringPatterns)
        {
            Assert.DoesNotContain(pattern, content, StringComparison.OrdinalIgnoreCase);
        }

        foreach (var pattern in ErrorResponseTestData.SensitivePatterns.EnvironmentVariablePatterns)
        {
            Assert.DoesNotContain(pattern, content, StringComparison.OrdinalIgnoreCase);
        }
    }

    [Fact]
    public async Task ErrorResponse_CodeIsStableIdentifier()
    {
        var client = _factory.CreateClient();

        var response1 = await client.GetAsync("/api/v2/ping");
        var response2 = await client.GetAsync("/api/v3/test");

        var (_, body1) = await GetErrorResponse(response1);
        var (_, body2) = await GetErrorResponse(response2);

        Assert.Equal(body1!.Error.Code, body2!.Error.Code);
        Assert.Equal("unsupported_api_version", body1.Error.Code);
    }

    [Fact]
    public async Task ErrorResponse_MessageIsHumanReadable()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/v2/ping");

        var (_, body) = await GetErrorResponse(response);

        Assert.NotNull(body!.Error.Message);
        Assert.NotEmpty(body.Error.Message);
        Assert.True(body.Error.Message.Length > 10);
        Assert.Contains("not supported", body.Error.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task HealthEndpoint_SuccessDoesNotReturnErrorStructure()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var content = await response.Content.ReadAsStringAsync();
        var jsonDoc = JsonDocument.Parse(content);
        var root = jsonDoc.RootElement;

        Assert.False(root.TryGetProperty("error", out _));
        Assert.True(root.TryGetProperty("status", out _));
    }

    [Fact]
    public async Task NotFoundEndpoint_ReturnsNotFound()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/v1/nonexistent-endpoint");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task MalformedVersionPattern_ReturnsStandardizedError()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/vX/endpoint");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        
        var (_, body) = await GetErrorResponse(response);
        AssertStandardizedErrorStructure(body, "unsupported_api_version");
    }

    [Fact]
    public async Task ErrorResponse_DetailsContainsRelevantInformation()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/v2/ping");

        var (_, body) = await GetErrorResponse(response);

        Assert.NotEmpty(body!.Error.Details);
        Assert.All(body.Error.Details, detail =>
        {
            Assert.NotNull(detail);
            Assert.NotEmpty(detail);
        });
    }

    [Fact]
    public async Task CaseInsensitiveVersioning_ReturnsStandardizedError()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/API/V2/ping");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        
        var (_, body) = await GetErrorResponse(response);
        AssertStandardizedErrorStructure(body, "unsupported_api_version");
    }

    [Fact]
    public async Task MixedCaseVersioning_ReturnsStandardizedError()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/Api/v2/Ping");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        
        var (_, body) = await GetErrorResponse(response);
        AssertStandardizedErrorStructure(body, "unsupported_api_version");
    }

    [Fact]
    public async Task ErrorResponse_JsonStructureIsValid()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/v2/ping");

        var content = await response.Content.ReadAsStringAsync();
        
        var exception = Record.Exception(() => JsonDocument.Parse(content));
        Assert.Null(exception);
    }

    [Fact]
    public async Task ErrorResponse_HasAllRequiredFields()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/v2/ping");

        var content = await response.Content.ReadAsStringAsync();
        var jsonDoc = JsonDocument.Parse(content);
        var root = jsonDoc.RootElement;

        Assert.True(root.TryGetProperty("error", out var error));
        Assert.True(error.TryGetProperty("code", out _));
        Assert.True(error.TryGetProperty("message", out _));
        Assert.True(error.TryGetProperty("details", out _));
    }
}
