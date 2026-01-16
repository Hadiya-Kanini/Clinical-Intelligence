using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using ClinicalIntelligence.Api.Data;
using ClinicalIntelligence.Api.Domain.Models;
using ClinicalIntelligence.Api.Middleware;
using ClinicalIntelligence.Api.Services;
using ClinicalIntelligence.Api.Tests.Fakes;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace ClinicalIntelligence.Api.Tests.Integration;

/// <summary>
/// Integration tests for admin create user credential email functionality (US_039).
/// Requires PostgreSQL database - tests are skipped when database is unavailable.
/// </summary>
public class AdminCreateUserCredentialEmailTests : IClassFixture<AdminCreateUserCredentialEmailTests.CredentialEmailTestFactory>
{
    private readonly CredentialEmailTestFactory _factory;
    private const string AdminEmail = "admin-credential-email-test@example.com";
    private const string AdminPassword = "AdminPassword123!";

    public AdminCreateUserCredentialEmailTests(CredentialEmailTestFactory factory)
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

    private HttpClient CreateClientWithCookies()
    {
        return _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            HandleCookies = true
        });
    }

    private async Task<User> EnsureAdminUserExistsAsync(ApplicationDbContext dbContext)
    {
        var user = await dbContext.Users
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.Email == AdminEmail);

        if (user == null)
        {
            user = new User
            {
                Id = Guid.NewGuid(),
                Email = AdminEmail,
                Name = "Admin Credential Email Test User",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(AdminPassword),
                Role = "Admin",
                Status = "Active",
                IsStaticAdmin = false,
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            dbContext.Users.Add(user);
            await dbContext.SaveChangesAsync();
        }
        return user;
    }

    private async Task<(HttpClient client, string? csrfToken)> LoginAsAdminAndGetCsrfAsync(ApplicationDbContext dbContext)
    {
        await EnsureAdminUserExistsAsync(dbContext);
        
        var client = CreateClientWithCookies();
        var loginRequest = new { email = AdminEmail, password = AdminPassword };
        var loginResponse = await client.PostAsJsonAsync("/api/v1/auth/login", loginRequest);
        
        if (loginResponse.StatusCode != HttpStatusCode.OK)
        {
            return (client, null);
        }

        var csrfResponse = await client.GetAsync("/api/v1/auth/csrf");
        string? csrfToken = null;
        if (csrfResponse.IsSuccessStatusCode)
        {
            var content = await csrfResponse.Content.ReadAsStringAsync();
            var jsonDoc = JsonDocument.Parse(content);
            if (jsonDoc.RootElement.TryGetProperty("token", out var tokenElement))
            {
                csrfToken = tokenElement.GetString();
            }
        }

        return (client, csrfToken);
    }

    private async Task<HttpResponseMessage> PostWithCsrfAsync(HttpClient client, string url, object content, string? csrfToken)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, url);
        request.Content = JsonContent.Create(content);
        if (!string.IsNullOrEmpty(csrfToken))
        {
            request.Headers.Add(CsrfProtectionMiddleware.CsrfHeaderName, csrfToken);
        }
        return await client.SendAsync(request);
    }

    [SkippableFact]
    public async Task CreateUser_WhenEmailSendSucceeds_ReturnsCredentialsEmailSentTrue()
    {
        Skip.IfNot(IsPostgreSqlAvailable(), "PostgreSQL database is not available");

        // Arrange
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        
        _factory.FakeEmailService.SetShouldSucceed(true);
        _factory.FakeEmailService.ClearInvocations();

        var (client, csrfToken) = await LoginAsAdminAndGetCsrfAsync(dbContext);
        Skip.If(csrfToken == null, "Could not obtain CSRF token");

        var uniqueEmail = $"emailsuccess_{Guid.NewGuid():N}@example.com";
        var request = new { name = "Email Success User", email = uniqueEmail };

        // Act
        var response = await PostWithCsrfAsync(client, "/api/v1/admin/users", request, csrfToken);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();
        var jsonDoc = JsonDocument.Parse(content);
        var root = jsonDoc.RootElement;

        Assert.True(root.TryGetProperty("credentials_email_sent", out var emailSentElement));
        Assert.True(emailSentElement.GetBoolean());

        Assert.False(root.TryGetProperty("credentials_email_error_code", out _));

        // Verify fake email service was invoked
        Assert.Single(_factory.FakeEmailService.NewUserCredentialsInvocations);
        var invocation = _factory.FakeEmailService.NewUserCredentialsInvocations[0];
        Assert.Equal(uniqueEmail.ToLowerInvariant(), invocation.To);
        Assert.Equal("Email Success User", invocation.UserName);
        Assert.False(string.IsNullOrEmpty(invocation.TemporaryPassword));
    }

    [SkippableFact]
    public async Task CreateUser_WhenEmailSendFails_ReturnsCredentialsEmailSentFalseAndUserStillCreated()
    {
        Skip.IfNot(IsPostgreSqlAvailable(), "PostgreSQL database is not available");

        // Arrange
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        
        _factory.FakeEmailService.SetShouldSucceed(false);
        _factory.FakeEmailService.ClearInvocations();

        var (client, csrfToken) = await LoginAsAdminAndGetCsrfAsync(dbContext);
        Skip.If(csrfToken == null, "Could not obtain CSRF token");

        var uniqueEmail = $"emailfail_{Guid.NewGuid():N}@example.com";
        var request = new { name = "Email Fail User", email = uniqueEmail };

        // Act
        var response = await PostWithCsrfAsync(client, "/api/v1/admin/users", request, csrfToken);

        // Assert - User creation still succeeds
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();
        var jsonDoc = JsonDocument.Parse(content);
        var root = jsonDoc.RootElement;

        // Verify credentials_email_sent is false
        Assert.True(root.TryGetProperty("credentials_email_sent", out var emailSentElement));
        Assert.False(emailSentElement.GetBoolean());

        // Verify error code is present
        Assert.True(root.TryGetProperty("credentials_email_error_code", out var errorCodeElement));
        Assert.Equal("email_send_failed", errorCodeElement.GetString());

        // Verify user was still created in database
        var createdUserId = Guid.Parse(root.GetProperty("id").GetString()!);
        var createdUser = await dbContext.Users.FirstOrDefaultAsync(u => u.Id == createdUserId);
        Assert.NotNull(createdUser);
        Assert.Equal(uniqueEmail.ToLowerInvariant(), createdUser.Email);
    }

    [SkippableFact]
    public async Task CreateUser_EmailInvocation_ContainsNonEmptyTemporaryPassword()
    {
        Skip.IfNot(IsPostgreSqlAvailable(), "PostgreSQL database is not available");

        // Arrange
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        
        _factory.FakeEmailService.SetShouldSucceed(true);
        _factory.FakeEmailService.ClearInvocations();

        var (client, csrfToken) = await LoginAsAdminAndGetCsrfAsync(dbContext);
        Skip.If(csrfToken == null, "Could not obtain CSRF token");

        var uniqueEmail = $"passwordcheck_{Guid.NewGuid():N}@example.com";
        var request = new { name = "Password Check User", email = uniqueEmail };

        // Act
        var response = await PostWithCsrfAsync(client, "/api/v1/admin/users", request, csrfToken);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.Single(_factory.FakeEmailService.NewUserCredentialsInvocations);
        
        var invocation = _factory.FakeEmailService.NewUserCredentialsInvocations[0];
        
        // Verify password is non-empty and meets minimum length
        Assert.False(string.IsNullOrEmpty(invocation.TemporaryPassword));
        Assert.True(invocation.TemporaryPassword.Length >= 12, "Temporary password should be at least 12 characters");
    }

    [SkippableFact]
    public async Task CreateUser_ResponseDoesNotContainPlaintextPassword()
    {
        Skip.IfNot(IsPostgreSqlAvailable(), "PostgreSQL database is not available");

        // Arrange
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        
        _factory.FakeEmailService.SetShouldSucceed(true);
        _factory.FakeEmailService.ClearInvocations();

        var (client, csrfToken) = await LoginAsAdminAndGetCsrfAsync(dbContext);
        Skip.If(csrfToken == null, "Could not obtain CSRF token");

        var uniqueEmail = $"nopassword_{Guid.NewGuid():N}@example.com";
        var request = new { name = "No Password In Response User", email = uniqueEmail };

        // Act
        var response = await PostWithCsrfAsync(client, "/api/v1/admin/users", request, csrfToken);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        
        var content = await response.Content.ReadAsStringAsync();
        var jsonDoc = JsonDocument.Parse(content);
        var root = jsonDoc.RootElement;

        // Verify response does not contain password field
        Assert.False(root.TryGetProperty("password", out _));
        Assert.False(root.TryGetProperty("temporary_password", out _));
        Assert.False(root.TryGetProperty("temporaryPassword", out _));

        // Get the actual password that was sent via email
        var invocation = _factory.FakeEmailService.NewUserCredentialsInvocations[0];
        
        // Verify the password is not in the response content
        Assert.DoesNotContain(invocation.TemporaryPassword, content);
    }

    [SkippableFact]
    public async Task CreateUser_GeneratedPasswordMeetsPasswordPolicy()
    {
        Skip.IfNot(IsPostgreSqlAvailable(), "PostgreSQL database is not available");

        // Arrange
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        
        _factory.FakeEmailService.SetShouldSucceed(true);
        _factory.FakeEmailService.ClearInvocations();

        var (client, csrfToken) = await LoginAsAdminAndGetCsrfAsync(dbContext);
        Skip.If(csrfToken == null, "Could not obtain CSRF token");

        var uniqueEmail = $"policycheck_{Guid.NewGuid():N}@example.com";
        var request = new { name = "Policy Check User", email = uniqueEmail };

        // Act
        var response = await PostWithCsrfAsync(client, "/api/v1/admin/users", request, csrfToken);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.Single(_factory.FakeEmailService.NewUserCredentialsInvocations);
        
        var password = _factory.FakeEmailService.NewUserCredentialsInvocations[0].TemporaryPassword;
        
        // Verify password meets policy requirements
        Assert.True(password.Length >= 8, "Password must be at least 8 characters");
        Assert.Contains(password, c => char.IsLower(c));
        Assert.Contains(password, c => char.IsUpper(c));
        Assert.Contains(password, c => char.IsDigit(c));
        Assert.Contains(password, c => !char.IsLetterOrDigit(c));
    }

    /// <summary>
    /// Custom test factory for credential email tests with accessible FakeEmailService.
    /// Uses PostgreSQL like other integration tests but overrides IEmailService with fake.
    /// </summary>
    public sealed class CredentialEmailTestFactory : WebApplicationFactory<Program>
    {
        public FakeEmailService FakeEmailService { get; } = new();

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureAppConfiguration((context, config) =>
            {
                config.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["RateLimiting:LoginPermitLimit"] = "100",
                    ["RateLimiting:LoginWindowSeconds"] = "60",
                    ["RateLimiting:ForgotPasswordPermitLimit"] = "1000",
                    ["RateLimiting:ForgotPasswordWindowSeconds"] = "1"
                });
            });

            builder.ConfigureServices(services =>
            {
                // Remove existing IEmailService and register fake for tests
                var emailServiceDescriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(IEmailService));
                if (emailServiceDescriptor != null)
                {
                    services.Remove(emailServiceDescriptor);
                }
                services.AddSingleton<IEmailService>(FakeEmailService);
            });
        }
    }
}
