using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace ClinicalIntelligence.Api.Tests;

public class LoginEmailValidationIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public LoginEmailValidationIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Login_WithInvalidEmailFormat_Returns400WithStableDetails()
    {
        var request = new { email = "not-an-email", password = "TestPassword123!" };

        var response = await _client.PostAsJsonAsync("/api/v1/auth/login", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();
        var json = JsonDocument.Parse(content);
        var error = json.RootElement.GetProperty("error");

        Assert.Equal("invalid_input", error.GetProperty("code").GetString());
        Assert.Equal("Email format is invalid.", error.GetProperty("message").GetString());

        var details = error.GetProperty("details");
        Assert.Contains("email:invalid_format", details.EnumerateArray().Select(d => d.GetString()));
    }

    [Fact]
    public async Task Login_WithMissingAtSymbol_Returns400()
    {
        var request = new { email = "userdomain.com", password = "TestPassword123!" };

        var response = await _client.PostAsJsonAsync("/api/v1/auth/login", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("email:invalid_format", content);
    }

    [Fact]
    public async Task Login_WithTrailingDot_Returns400()
    {
        var request = new { email = "user@domain.com.", password = "TestPassword123!" };

        var response = await _client.PostAsJsonAsync("/api/v1/auth/login", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("email:invalid_format", content);
    }

    [Fact]
    public async Task Login_WithConsecutiveDots_Returns400()
    {
        var request = new { email = "user..name@domain.com", password = "TestPassword123!" };

        var response = await _client.PostAsJsonAsync("/api/v1/auth/login", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("email:invalid_format", content);
    }

    [Fact]
    public async Task Login_WithValidPlusAddressingFormat_DoesNotReturn400ForFormat()
    {
        var request = new { email = "user+tag@domain.com", password = "TestPassword123!" };

        var response = await _client.PostAsJsonAsync("/api/v1/auth/login", request);

        var content = await response.Content.ReadAsStringAsync();
        Assert.DoesNotContain("email:invalid_format", content);
    }

    [Fact]
    public async Task Login_WithEmptyEmail_Returns400WithRequiredDetail()
    {
        var request = new { email = "", password = "TestPassword123!" };

        var response = await _client.PostAsJsonAsync("/api/v1/auth/login", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("email:required", content);
    }

    [Fact]
    public async Task Login_WithWhitespaceOnlyEmail_Returns400WithRequiredDetail()
    {
        var request = new { email = "   ", password = "TestPassword123!" };

        var response = await _client.PostAsJsonAsync("/api/v1/auth/login", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("email:required", content);
    }

    [Fact]
    public async Task Login_WithMissingDomain_Returns400()
    {
        var request = new { email = "user@", password = "TestPassword123!" };

        var response = await _client.PostAsJsonAsync("/api/v1/auth/login", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("email:invalid_format", content);
    }

    [Fact]
    public async Task Login_WithMissingLocalPart_Returns400()
    {
        var request = new { email = "@domain.com", password = "TestPassword123!" };

        var response = await _client.PostAsJsonAsync("/api/v1/auth/login", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("email:invalid_format", content);
    }
}
