using ClinicalIntelligence.Api.Configuration;
using ClinicalIntelligence.Api.Data;
using ClinicalIntelligence.Api.Domain.Models;
using ClinicalIntelligence.Api.Services;
using ClinicalIntelligence.Api.Services.Auth;
using ClinicalIntelligence.Api.Services.Email;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;

namespace ClinicalIntelligence.Api.Tests.Integration;

/// <summary>
/// Integration tests for forgot-password non-enumeration and timing normalization (US_027).
/// Validates that:
/// - Existing and non-existing emails return identical responses (status code + payload)
/// - Response timing meets the configured minimum floor for syntactically valid requests
/// - Invalid input (400) is not subject to timing normalization
/// </summary>
public class ForgotPasswordNonEnumerationAndTimingTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private const string TestEmail = "timing-test-user@example.com";
    private const string TestPassword = "TestPassword123!";
    private const int TestMinDelayMs = 200;
    private const int TimingToleranceMs = 50;

    public ForgotPasswordNonEnumerationAndTimingTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
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

    private WebApplicationFactory<Program> CreateFactoryWithTimingConfig(int minDelayMs)
    {
        return _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureAppConfiguration((context, config) =>
            {
                config.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    [$"{ForgotPasswordResponseTimingOptions.SectionName}:MinDelayMs"] = minDelayMs.ToString(),
                    [$"{ForgotPasswordResponseTimingOptions.SectionName}:JitterMs"] = "0"
                });
            });

            builder.ConfigureServices(services =>
            {
                // Remove existing timing options and normalizer
                var timingOptionsDescriptor = services.FirstOrDefault(d => d.ServiceType == typeof(ForgotPasswordResponseTimingOptions));
                if (timingOptionsDescriptor != null)
                {
                    services.Remove(timingOptionsDescriptor);
                }

                var normalizerDescriptor = services.FirstOrDefault(d => d.ServiceType == typeof(IResponseTimingNormalizer));
                if (normalizerDescriptor != null)
                {
                    services.Remove(normalizerDescriptor);
                }

                // Add timing options with test configuration
                var testOptions = new ForgotPasswordResponseTimingOptions
                {
                    MinDelayMs = minDelayMs,
                    JitterMs = 0
                };
                services.AddSingleton(testOptions);
                services.AddSingleton<IResponseTimingNormalizer, ResponseTimingNormalizer>();

                // Replace email services with mocks to avoid actual email sending
                var emailDescriptor = services.FirstOrDefault(d => d.ServiceType == typeof(IEmailService));
                if (emailDescriptor != null)
                {
                    services.Remove(emailDescriptor);
                }
                services.AddSingleton<IEmailService, NoOpEmailService>();

                var smtpDescriptor = services.FirstOrDefault(d => d.ServiceType == typeof(ISmtpEmailSender));
                if (smtpDescriptor != null)
                {
                    services.Remove(smtpDescriptor);
                }
                services.AddSingleton<ISmtpEmailSender, NoOpSmtpEmailSender>();
            });
        });
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
                Name = "Timing Test User",
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

    private async Task CleanupUserTokensAsync(ApplicationDbContext dbContext, Guid userId)
    {
        var existingTokens = await dbContext.PasswordResetTokens
            .Where(t => t.UserId == userId)
            .ToListAsync();
        dbContext.PasswordResetTokens.RemoveRange(existingTokens);
        await dbContext.SaveChangesAsync();
    }

    [Fact]
    public async Task ForgotPassword_ExistingVsNonExisting_ReturnsIdentical200Response()
    {
        if (!IsPostgreSqlAvailable()) return;

        // Arrange
        using var customFactory = CreateFactoryWithTimingConfig(TestMinDelayMs);
        using var scope = customFactory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var user = await EnsureTestUserExistsAsync(dbContext);
        await CleanupUserTokensAsync(dbContext, user.Id);

        var client = customFactory.CreateClient();
        var nonExistingEmail = $"nonexistent-{Guid.NewGuid()}@example.com";

        // Act - Request for existing email
        var responseExisting = await client.PostAsJsonAsync("/api/v1/auth/forgot-password", new
        {
            email = TestEmail
        });

        // Act - Request for non-existing email
        var responseNonExisting = await client.PostAsJsonAsync("/api/v1/auth/forgot-password", new
        {
            email = nonExistingEmail
        });

        // Assert - Both return 200 OK
        Assert.Equal(HttpStatusCode.OK, responseExisting.StatusCode);
        Assert.Equal(HttpStatusCode.OK, responseNonExisting.StatusCode);

        // Assert - Response bodies are identical
        var contentExisting = await responseExisting.Content.ReadAsStringAsync();
        var contentNonExisting = await responseNonExisting.Content.ReadAsStringAsync();

        Assert.Equal(contentExisting, contentNonExisting);
        Assert.Contains("If the email exists", contentExisting);
    }

    [Fact]
    public async Task ForgotPassword_ExistingVsNonExisting_ResponseStructureIdentical()
    {
        if (!IsPostgreSqlAvailable()) return;

        // Arrange
        using var customFactory = CreateFactoryWithTimingConfig(TestMinDelayMs);
        using var scope = customFactory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var user = await EnsureTestUserExistsAsync(dbContext);
        await CleanupUserTokensAsync(dbContext, user.Id);

        var client = customFactory.CreateClient();
        var nonExistingEmail = $"nonexistent-{Guid.NewGuid()}@example.com";

        // Act
        var responseExisting = await client.PostAsJsonAsync("/api/v1/auth/forgot-password", new
        {
            email = TestEmail
        });

        var responseNonExisting = await client.PostAsJsonAsync("/api/v1/auth/forgot-password", new
        {
            email = nonExistingEmail
        });

        // Assert - Parse JSON and compare structure
        var jsonExisting = JsonDocument.Parse(await responseExisting.Content.ReadAsStringAsync());
        var jsonNonExisting = JsonDocument.Parse(await responseNonExisting.Content.ReadAsStringAsync());

        // Both should have "message" property
        Assert.True(jsonExisting.RootElement.TryGetProperty("message", out var msgExisting));
        Assert.True(jsonNonExisting.RootElement.TryGetProperty("message", out var msgNonExisting));

        // Messages should be identical
        Assert.Equal(msgExisting.GetString(), msgNonExisting.GetString());
    }

    [Fact]
    public async Task ForgotPassword_SyntacticallyValidEmail_MeetsMinimumResponseTime()
    {
        if (!IsPostgreSqlAvailable()) return;

        // Arrange - Use a longer delay for more reliable timing measurement
        const int testDelayMs = 200;
        using var customFactory = CreateFactoryWithTimingConfig(testDelayMs);
        var client = customFactory.CreateClient();
        var testEmailForTiming = $"timing-test-{Guid.NewGuid()}@example.com";

        // Act - Measure response time
        var stopwatch = Stopwatch.StartNew();
        var response = await client.PostAsJsonAsync("/api/v1/auth/forgot-password", new
        {
            email = testEmailForTiming
        });
        stopwatch.Stop();

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        // Response time should be at least the configured minimum (with tolerance for test environment variance)
        var elapsedMs = stopwatch.ElapsedMilliseconds;
        Assert.True(
            elapsedMs >= testDelayMs - TimingToleranceMs,
            $"Response time {elapsedMs}ms should be >= {testDelayMs - TimingToleranceMs}ms (configured: {testDelayMs}ms, tolerance: {TimingToleranceMs}ms)");
    }

    [Fact]
    public async Task ForgotPassword_ExistingUser_MeetsMinimumResponseTime()
    {
        if (!IsPostgreSqlAvailable()) return;

        // Arrange
        const int testDelayMs = 200;
        using var customFactory = CreateFactoryWithTimingConfig(testDelayMs);
        using var scope = customFactory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var user = await EnsureTestUserExistsAsync(dbContext);
        await CleanupUserTokensAsync(dbContext, user.Id);

        var client = customFactory.CreateClient();

        // Act - Measure response time for existing user
        var stopwatch = Stopwatch.StartNew();
        var response = await client.PostAsJsonAsync("/api/v1/auth/forgot-password", new
        {
            email = TestEmail
        });
        stopwatch.Stop();

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var elapsedMs = stopwatch.ElapsedMilliseconds;
        Assert.True(
            elapsedMs >= testDelayMs - TimingToleranceMs,
            $"Response time for existing user {elapsedMs}ms should be >= {testDelayMs - TimingToleranceMs}ms");
    }

    [Fact]
    public async Task ForgotPassword_InvalidEmail_Returns400_NoTimingNormalization()
    {
        if (!IsPostgreSqlAvailable()) return;

        // Arrange - Use a long delay to make timing difference obvious
        const int testDelayMs = 500;
        using var customFactory = CreateFactoryWithTimingConfig(testDelayMs);
        var client = customFactory.CreateClient();

        // Act - Send invalid email format
        var stopwatch = Stopwatch.StartNew();
        var response = await client.PostAsJsonAsync("/api/v1/auth/forgot-password", new
        {
            email = "not-an-email"
        });
        stopwatch.Stop();

        // Assert - Should return 400 Bad Request
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        // Response should be fast (no timing normalization applied)
        // Allow generous tolerance but should be significantly faster than the configured delay
        var elapsedMs = stopwatch.ElapsedMilliseconds;
        Assert.True(
            elapsedMs < testDelayMs - 100,
            $"Invalid email response time {elapsedMs}ms should be < {testDelayMs - 100}ms (timing normalization should NOT apply)");
    }

    [Fact]
    public async Task ForgotPassword_MissingEmail_Returns400_NoTimingNormalization()
    {
        if (!IsPostgreSqlAvailable()) return;

        // Arrange
        const int testDelayMs = 500;
        using var customFactory = CreateFactoryWithTimingConfig(testDelayMs);
        var client = customFactory.CreateClient();

        // Act - Send empty email
        var stopwatch = Stopwatch.StartNew();
        var response = await client.PostAsJsonAsync("/api/v1/auth/forgot-password", new
        {
            email = ""
        });
        stopwatch.Stop();

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var elapsedMs = stopwatch.ElapsedMilliseconds;
        Assert.True(
            elapsedMs < testDelayMs - 100,
            $"Missing email response time {elapsedMs}ms should be < {testDelayMs - 100}ms");
    }

    [Fact]
    public async Task ForgotPassword_MultipleRequests_ConsistentTiming()
    {
        if (!IsPostgreSqlAvailable()) return;

        // Arrange
        const int testDelayMs = 200;
        const int requestCount = 3;
        using var customFactory = CreateFactoryWithTimingConfig(testDelayMs);
        var client = customFactory.CreateClient();

        var timings = new List<long>();

        // Act - Make multiple requests with different non-existing emails
        for (int i = 0; i < requestCount; i++)
        {
            var email = $"timing-consistency-{Guid.NewGuid()}@example.com";
            var stopwatch = Stopwatch.StartNew();
            var response = await client.PostAsJsonAsync("/api/v1/auth/forgot-password", new
            {
                email = email
            });
            stopwatch.Stop();

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            timings.Add(stopwatch.ElapsedMilliseconds);
        }

        // Assert - All timings should be >= minimum delay
        foreach (var timing in timings)
        {
            Assert.True(
                timing >= testDelayMs - TimingToleranceMs,
                $"Request timing {timing}ms should be >= {testDelayMs - TimingToleranceMs}ms");
        }

        // Assert - Timing variance should be reasonable (within 100ms of each other)
        var minTiming = timings.Min();
        var maxTiming = timings.Max();
        var variance = maxTiming - minTiming;
        Assert.True(
            variance < 150,
            $"Timing variance {variance}ms should be < 150ms for consistent response times");
    }

    [Fact]
    public async Task ForgotPassword_ResponseHeaders_NoEnumerationLeakage()
    {
        if (!IsPostgreSqlAvailable()) return;

        // Arrange
        using var customFactory = CreateFactoryWithTimingConfig(TestMinDelayMs);
        using var scope = customFactory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var user = await EnsureTestUserExistsAsync(dbContext);
        await CleanupUserTokensAsync(dbContext, user.Id);

        var client = customFactory.CreateClient();
        var nonExistingEmail = $"nonexistent-{Guid.NewGuid()}@example.com";

        // Act
        var responseExisting = await client.PostAsJsonAsync("/api/v1/auth/forgot-password", new
        {
            email = TestEmail
        });

        var responseNonExisting = await client.PostAsJsonAsync("/api/v1/auth/forgot-password", new
        {
            email = nonExistingEmail
        });

        // Assert - Content-Type headers should be identical
        Assert.Equal(
            responseExisting.Content.Headers.ContentType?.ToString(),
            responseNonExisting.Content.Headers.ContentType?.ToString());

        // Assert - No headers should leak user existence
        // (e.g., no X-User-Found or similar custom headers)
        Assert.False(responseExisting.Headers.Contains("X-User-Found"));
        Assert.False(responseNonExisting.Headers.Contains("X-User-Found"));
        Assert.False(responseExisting.Headers.Contains("X-User-Exists"));
        Assert.False(responseNonExisting.Headers.Contains("X-User-Exists"));
    }
}

/// <summary>
/// No-op implementation of IEmailService for timing tests.
/// </summary>
internal class NoOpEmailService : IEmailService
{
    public bool IsConfigured => true;

    public Task<bool> SendEmailAsync(string to, string subject, string body, bool isHtml = true)
        => Task.FromResult(true);

    public Task<bool> SendPasswordResetEmailAsync(string to, string resetToken, string userName, string resetUrl)
        => Task.FromResult(true);

    public Task<bool> SendPasswordResetConfirmationAsync(string to, string userName)
        => Task.FromResult(true);

    public Task<bool> SendAccountLockedEmailAsync(string to, string userName, DateTime lockedUntil)
        => Task.FromResult(true);

    public Task<bool> SendNewUserCredentialsEmailAsync(string to, string userName, string temporaryPassword)
        => Task.FromResult(true);
}

/// <summary>
/// No-op implementation of ISmtpEmailSender for timing tests.
/// </summary>
internal class NoOpSmtpEmailSender : ISmtpEmailSender
{
    public bool IsConfigured => true;

    public Task<bool> SendAsync(string to, string subject, string htmlBody, string? textBody = null, CancellationToken cancellationToken = default)
        => Task.FromResult(true);
}
