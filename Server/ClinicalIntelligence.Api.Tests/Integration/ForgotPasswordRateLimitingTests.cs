using ClinicalIntelligence.Api.Configuration;
using ClinicalIntelligence.Api.Data;
using ClinicalIntelligence.Api.Domain.Models;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;

namespace ClinicalIntelligence.Api.Tests.Integration;

/// <summary>
/// Integration tests for forgot-password rate limiting behavior (US_028).
/// Tests 429 response, Retry-After header, window reset, and audit logging.
/// </summary>
public class ForgotPasswordRateLimitingTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private const string TestEmail = "forgotpassword-ratelimit-test@example.com";
    private const string TestPassword = "TestPassword123!";
    private const int TestPermitLimit = 3;
    private const int TestWindowSeconds = 2;

    public ForgotPasswordRateLimitingTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureAppConfiguration((context, config) =>
            {
                config.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["RateLimiting:ForgotPasswordPermitLimit"] = TestPermitLimit.ToString(),
                    ["RateLimiting:ForgotPasswordWindowSeconds"] = TestWindowSeconds.ToString()
                });
            });
        });
    }

    private bool IsPostgreSqlAvailable()
    {
        try
        {
            using var scope = _factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            return dbContext.Database.CanConnect();
        }
        catch
        {
            return false;
        }
    }

    private async Task<User> EnsureTestUserExistsAsync(ApplicationDbContext dbContext)
    {
        var user = await dbContext.Users
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.Email == TestEmail);

        if (user == null)
        {
            user = new User
            {
                Id = Guid.NewGuid(),
                Email = TestEmail,
                Name = "Forgot Password Rate Limit Test User",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(TestPassword),
                Role = "Standard",
                Status = "Active",
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            dbContext.Users.Add(user);
            await dbContext.SaveChangesAsync();
        }
        return user;
    }

    private HttpClient CreateTestClient()
    {
        return _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            HandleCookies = true
        });
    }

    [Fact]
    public async Task ForgotPassword_WithinRateLimit_ReturnsSuccess()
    {
        if (!IsPostgreSqlAvailable()) return;

        // Arrange
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await EnsureTestUserExistsAsync(dbContext);

        var client = CreateTestClient();

        // Act - single forgot-password request should succeed
        var response = await client.PostAsJsonAsync("/api/v1/auth/forgot-password", new
        {
            email = TestEmail
        });

        // Assert - should return 200 OK (generic success response)
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task ForgotPassword_ExceedsRateLimit_Returns429()
    {
        if (!IsPostgreSqlAvailable()) return;

        // Arrange
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await EnsureTestUserExistsAsync(dbContext);

        var client = CreateTestClient();

        // Act - make requests up to and exceeding the limit
        var responses = new List<HttpResponseMessage>();
        for (int i = 0; i < TestPermitLimit + 1; i++)
        {
            var response = await client.PostAsJsonAsync("/api/v1/auth/forgot-password", new
            {
                email = TestEmail
            });
            responses.Add(response);
        }

        // Assert - first TestPermitLimit requests should not be rate limited
        for (int i = 0; i < TestPermitLimit; i++)
        {
            Assert.NotEqual(HttpStatusCode.TooManyRequests, responses[i].StatusCode);
        }

        // The request exceeding the limit should return 429
        Assert.Equal(HttpStatusCode.TooManyRequests, responses[TestPermitLimit].StatusCode);
    }

    [Fact]
    public async Task ForgotPassword_RateLimitExceeded_IncludesRetryAfterHeader()
    {
        if (!IsPostgreSqlAvailable()) return;

        // Arrange
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await EnsureTestUserExistsAsync(dbContext);

        var client = CreateTestClient();

        // Act - exceed the rate limit
        HttpResponseMessage? rateLimitedResponse = null;
        for (int i = 0; i <= TestPermitLimit; i++)
        {
            var response = await client.PostAsJsonAsync("/api/v1/auth/forgot-password", new
            {
                email = TestEmail
            });

            if (response.StatusCode == HttpStatusCode.TooManyRequests)
            {
                rateLimitedResponse = response;
                break;
            }
        }

        // Assert
        Assert.NotNull(rateLimitedResponse);
        Assert.True(rateLimitedResponse.Headers.Contains("Retry-After"));

        var retryAfterValue = rateLimitedResponse.Headers.GetValues("Retry-After").FirstOrDefault();
        Assert.NotNull(retryAfterValue);
        Assert.True(int.TryParse(retryAfterValue, out var retryAfterSeconds));
        Assert.True(retryAfterSeconds > 0);
    }

    [Fact]
    public async Task ForgotPassword_RateLimitExceeded_ReturnsJsonErrorBody()
    {
        if (!IsPostgreSqlAvailable()) return;

        // Arrange
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await EnsureTestUserExistsAsync(dbContext);

        var client = CreateTestClient();

        // Act - exceed the rate limit
        HttpResponseMessage? rateLimitedResponse = null;
        for (int i = 0; i <= TestPermitLimit; i++)
        {
            var response = await client.PostAsJsonAsync("/api/v1/auth/forgot-password", new
            {
                email = TestEmail
            });

            if (response.StatusCode == HttpStatusCode.TooManyRequests)
            {
                rateLimitedResponse = response;
                break;
            }
        }

        // Assert
        Assert.NotNull(rateLimitedResponse);

        var content = await rateLimitedResponse.Content.ReadAsStringAsync();
        Assert.False(string.IsNullOrEmpty(content));

        using var doc = JsonDocument.Parse(content);
        var root = doc.RootElement;

        Assert.True(root.TryGetProperty("error", out var errorElement));
        Assert.True(errorElement.TryGetProperty("code", out var codeElement));
        Assert.Equal("rate_limited", codeElement.GetString());

        Assert.True(errorElement.TryGetProperty("message", out var messageElement));
        Assert.False(string.IsNullOrEmpty(messageElement.GetString()));
    }

    [Fact]
    public async Task ForgotPassword_RateLimitExceeded_ReturnsCorrectMessage()
    {
        if (!IsPostgreSqlAvailable()) return;

        // Arrange
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await EnsureTestUserExistsAsync(dbContext);

        var client = CreateTestClient();

        // Act - exceed the rate limit
        HttpResponseMessage? rateLimitedResponse = null;
        for (int i = 0; i <= TestPermitLimit; i++)
        {
            var response = await client.PostAsJsonAsync("/api/v1/auth/forgot-password", new
            {
                email = TestEmail
            });

            if (response.StatusCode == HttpStatusCode.TooManyRequests)
            {
                rateLimitedResponse = response;
                break;
            }
        }

        // Assert
        Assert.NotNull(rateLimitedResponse);

        var content = await rateLimitedResponse.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(content);
        var root = doc.RootElement;

        Assert.True(root.TryGetProperty("error", out var errorElement));
        Assert.True(errorElement.TryGetProperty("message", out var messageElement));
        
        var message = messageElement.GetString();
        Assert.NotNull(message);
        // Should contain "password reset" not "login"
        Assert.Contains("password reset", message, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("login", message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ForgotPassword_RateLimitExceeded_DoesNotLeakSensitiveInfo()
    {
        if (!IsPostgreSqlAvailable()) return;

        // Arrange
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await EnsureTestUserExistsAsync(dbContext);

        var client = CreateTestClient();

        // Act - exceed the rate limit
        HttpResponseMessage? rateLimitedResponse = null;
        for (int i = 0; i <= TestPermitLimit; i++)
        {
            var response = await client.PostAsJsonAsync("/api/v1/auth/forgot-password", new
            {
                email = TestEmail
            });

            if (response.StatusCode == HttpStatusCode.TooManyRequests)
            {
                rateLimitedResponse = response;
                break;
            }
        }

        // Assert
        Assert.NotNull(rateLimitedResponse);

        var content = await rateLimitedResponse.Content.ReadAsStringAsync();

        // Should not contain sensitive information
        Assert.DoesNotContain(TestEmail, content);
        Assert.DoesNotContain("password", content.ToLower());
        Assert.DoesNotContain("ip", content.ToLower());
    }

    [Fact]
    public async Task ForgotPassword_AfterWindowExpires_AllowsNewAttempts()
    {
        if (!IsPostgreSqlAvailable()) return;

        // Arrange
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await EnsureTestUserExistsAsync(dbContext);

        var client = CreateTestClient();

        // Act - exceed the rate limit
        for (int i = 0; i <= TestPermitLimit; i++)
        {
            await client.PostAsJsonAsync("/api/v1/auth/forgot-password", new
            {
                email = TestEmail
            });
        }

        // Wait for the window to expire (add buffer for timing)
        await Task.Delay(TimeSpan.FromSeconds(TestWindowSeconds + 1));

        // Try again after window expires
        var responseAfterWindow = await client.PostAsJsonAsync("/api/v1/auth/forgot-password", new
        {
            email = TestEmail
        });

        // Assert - should not be rate limited after window expires
        Assert.NotEqual(HttpStatusCode.TooManyRequests, responseAfterWindow.StatusCode);
    }

    [Fact]
    public async Task ForgotPassword_RateLimitExceeded_CreatesAuditLogEvent()
    {
        if (!IsPostgreSqlAvailable()) return;

        // Arrange
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await EnsureTestUserExistsAsync(dbContext);

        // Clear existing rate limit audit events for forgot-password
        var existingEvents = await dbContext.AuditLogEvents
            .Where(e => e.ActionType == "RATE_LIMIT_EXCEEDED")
            .ToListAsync();
        dbContext.AuditLogEvents.RemoveRange(existingEvents);
        await dbContext.SaveChangesAsync();

        var client = CreateTestClient();

        // Act - exceed the rate limit
        for (int i = 0; i <= TestPermitLimit; i++)
        {
            await client.PostAsJsonAsync("/api/v1/auth/forgot-password", new
            {
                email = TestEmail
            });
        }

        // Assert - verify audit log event was created
        var auditEvent = await dbContext.AuditLogEvents
            .OrderByDescending(e => e.Timestamp)
            .FirstOrDefaultAsync(e => e.ActionType == "RATE_LIMIT_EXCEEDED");

        Assert.NotNull(auditEvent);
        Assert.Equal("RATE_LIMIT_EXCEEDED", auditEvent.ActionType);
        Assert.Equal("Auth", auditEvent.ResourceType);
        Assert.Null(auditEvent.UserId);
        Assert.Null(auditEvent.SessionId);
        Assert.NotNull(auditEvent.Metadata);

        // Verify metadata contains expected fields including endpoint path
        using var metadataDoc = JsonDocument.Parse(auditEvent.Metadata);
        var metadata = metadataDoc.RootElement;
        Assert.True(metadata.TryGetProperty("endpoint", out var endpointElement));
        Assert.Contains("forgot-password", endpointElement.GetString(), StringComparison.OrdinalIgnoreCase);
        Assert.True(metadata.TryGetProperty("permitLimit", out _));
        Assert.True(metadata.TryGetProperty("windowSeconds", out _));
    }

    [Fact]
    public async Task ForgotPassword_OtherEndpoints_NotAffectedByForgotPasswordRateLimit()
    {
        if (!IsPostgreSqlAvailable()) return;

        // Arrange
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await EnsureTestUserExistsAsync(dbContext);

        var client = CreateTestClient();

        // Exhaust the forgot-password rate limit
        for (int i = 0; i <= TestPermitLimit; i++)
        {
            await client.PostAsJsonAsync("/api/v1/auth/forgot-password", new
            {
                email = TestEmail
            });
        }

        // Act - try to access health endpoint (should not be rate limited)
        var healthResponse = await client.GetAsync("/health");

        // Assert
        Assert.Equal(HttpStatusCode.OK, healthResponse.StatusCode);
    }

    [Fact]
    public async Task ForgotPassword_NonExistentEmail_StillCountsTowardRateLimit()
    {
        if (!IsPostgreSqlAvailable()) return;

        // Arrange
        var client = CreateTestClient();

        // Act - make requests with non-existent email up to and exceeding the limit
        var responses = new List<HttpResponseMessage>();
        for (int i = 0; i < TestPermitLimit + 1; i++)
        {
            var response = await client.PostAsJsonAsync("/api/v1/auth/forgot-password", new
            {
                email = $"nonexistent-{i}@example.com"
            });
            responses.Add(response);
        }

        // Assert - first TestPermitLimit requests should return 200 (generic success)
        for (int i = 0; i < TestPermitLimit; i++)
        {
            Assert.Equal(HttpStatusCode.OK, responses[i].StatusCode);
        }

        // The request exceeding the limit should return 429
        Assert.Equal(HttpStatusCode.TooManyRequests, responses[TestPermitLimit].StatusCode);
    }

    [Fact]
    public async Task ForgotPassword_LoginRateLimit_IndependentFromForgotPasswordRateLimit()
    {
        if (!IsPostgreSqlAvailable()) return;

        // Arrange
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await EnsureTestUserExistsAsync(dbContext);

        var client = CreateTestClient();

        // Exhaust the forgot-password rate limit
        for (int i = 0; i <= TestPermitLimit; i++)
        {
            await client.PostAsJsonAsync("/api/v1/auth/forgot-password", new
            {
                email = TestEmail
            });
        }

        // Act - try to login (should not be affected by forgot-password rate limit)
        var loginResponse = await client.PostAsJsonAsync("/api/v1/auth/login", new
        {
            email = TestEmail,
            password = TestPassword
        });

        // Assert - login should not return 429 (it has its own rate limit)
        // It should return either 200 (success) or 401 (invalid credentials) but not 429
        Assert.NotEqual(HttpStatusCode.TooManyRequests, loginResponse.StatusCode);
    }
}
