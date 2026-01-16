using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using ClinicalIntelligence.Api.Contracts.Admin;
using ClinicalIntelligence.Api.Data;
using ClinicalIntelligence.Api.Domain.Models;
using ClinicalIntelligence.Api.Middleware;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace ClinicalIntelligence.Api.Tests.Integration;

/// <summary>
/// Integration tests for the admin create user endpoint (US_037).
/// Requires PostgreSQL database - tests are skipped when database is unavailable.
/// </summary>
public class AdminCreateUserEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public AdminCreateUserEndpointTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureAppConfiguration((context, config) =>
            {
                config.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["RateLimiting:LoginPermitLimit"] = "100",
                    ["RateLimiting:LoginWindowSeconds"] = "60"
                });
            });
        });
    }
    private const string AdminEmail = "admin-create-test@example.com";
    private const string AdminPassword = "AdminPassword123!";
    private const string StandardEmail = "standard-create-test@example.com";
    private const string StandardPassword = "StandardPassword123!";

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
                Name = "Admin Create Test User",
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

    private async Task<User> EnsureStandardUserExistsAsync(ApplicationDbContext dbContext)
    {
        var user = await dbContext.Users
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.Email == StandardEmail);

        if (user == null)
        {
            user = new User
            {
                Id = Guid.NewGuid(),
                Email = StandardEmail,
                Name = "Standard Create Test User",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(StandardPassword),
                Role = "Standard",
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

    private HttpClient CreateClientWithCookies()
    {
        return _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            HandleCookies = true
        });
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

    private async Task<(HttpClient client, string? csrfToken)> LoginAsStandardUserAndGetCsrfAsync(ApplicationDbContext dbContext)
    {
        await EnsureStandardUserExistsAsync(dbContext);
        
        var client = CreateClientWithCookies();
        var loginRequest = new { email = StandardEmail, password = StandardPassword };
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
    public async Task CreateUser_AsAdmin_WithValidRequest_Returns201AndCreatesUser()
    {
        Skip.If(!IsPostgreSqlAvailable(), "PostgreSQL database not available");

        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var (client, csrfToken) = await LoginAsAdminAndGetCsrfAsync(dbContext);
        Skip.If(csrfToken == null, "Could not obtain CSRF token");

        var uniqueEmail = $"newuser_{Guid.NewGuid():N}@example.com";
        var request = new CreateUserRequest
        {
            Name = "New Test User",
            Email = uniqueEmail,
            Password = "SecurePassword123!"
        };

        var response = await PostWithCsrfAsync(client, "/api/v1/admin/users", request, csrfToken);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();
        var jsonDoc = JsonDocument.Parse(content);
        var root = jsonDoc.RootElement;

        Assert.True(root.TryGetProperty("id", out var idElement));
        Assert.False(string.IsNullOrEmpty(idElement.GetString()));
        Assert.True(root.TryGetProperty("email", out var emailElement));
        Assert.Equal(uniqueEmail.ToLowerInvariant(), emailElement.GetString());
        Assert.True(root.TryGetProperty("role", out var roleElement));
        Assert.Equal("standard", roleElement.GetString());

        var createdUserId = Guid.Parse(idElement.GetString()!);
        var createdUser = await dbContext.Users.FirstOrDefaultAsync(u => u.Id == createdUserId);
        
        Assert.NotNull(createdUser);
        Assert.Equal(uniqueEmail.ToLowerInvariant(), createdUser.Email);
        Assert.Equal("New Test User", createdUser.Name);
        Assert.Equal("Standard", createdUser.Role);
        Assert.Equal("Active", createdUser.Status);
        Assert.False(createdUser.IsStaticAdmin);
        Assert.False(createdUser.IsDeleted);
    }

    [SkippableFact]
    public async Task CreateUser_AsAdmin_CreatesUserCreatedAuditEvent()
    {
        Skip.If(!IsPostgreSqlAvailable(), "PostgreSQL database not available");

        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var (client, csrfToken) = await LoginAsAdminAndGetCsrfAsync(dbContext);
        Skip.If(csrfToken == null, "Could not obtain CSRF token");

        var uniqueEmail = $"audituser_{Guid.NewGuid():N}@example.com";
        var request = new CreateUserRequest
        {
            Name = "Audit Test User",
            Email = uniqueEmail,
            Password = "SecurePassword123!"
        };

        var response = await PostWithCsrfAsync(client, "/api/v1/admin/users", request, csrfToken);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();
        var jsonDoc = JsonDocument.Parse(content);
        var createdUserId = Guid.Parse(jsonDoc.RootElement.GetProperty("id").GetString()!);

        await Task.Delay(100);

        var auditEvent = await dbContext.AuditLogEvents
            .FirstOrDefaultAsync(a => a.ActionType == "USER_CREATED" && a.ResourceId == createdUserId);

        Assert.NotNull(auditEvent);
        Assert.Equal("User", auditEvent.ResourceType);
        Assert.NotNull(auditEvent.Metadata);
        Assert.Contains(uniqueEmail.ToLowerInvariant(), auditEvent.Metadata);
        Assert.DoesNotContain("SecurePassword123!", auditEvent.Metadata);
    }

    [SkippableFact]
    public async Task CreateUser_AsStandardUser_Returns403Forbidden()
    {
        Skip.If(!IsPostgreSqlAvailable(), "PostgreSQL database not available");

        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var (client, csrfToken) = await LoginAsStandardUserAndGetCsrfAsync(dbContext);
        Skip.If(csrfToken == null, "Could not obtain CSRF token");

        var request = new CreateUserRequest
        {
            Name = "Should Not Create",
            Email = "shouldnotcreate@example.com",
            Password = "SecurePassword123!"
        };

        var response = await PostWithCsrfAsync(client, "/api/v1/admin/users", request, csrfToken);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();
        var jsonDoc = JsonDocument.Parse(content);
        
        Assert.True(jsonDoc.RootElement.TryGetProperty("error", out var errorElement));
        Assert.Equal("forbidden", errorElement.GetProperty("code").GetString());
    }

    [Fact]
    public async Task CreateUser_WithoutAuthentication_Returns401Unauthorized()
    {
        using var freshClient = _factory.CreateClient();

        var request = new CreateUserRequest
        {
            Name = "Unauthenticated User",
            Email = "unauthenticated@example.com",
            Password = "SecurePassword123!"
        };

        var response = await freshClient.PostAsJsonAsync("/api/v1/admin/users", request);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [SkippableFact]
    public async Task CreateUser_WithMissingName_Returns400BadRequest()
    {
        Skip.If(!IsPostgreSqlAvailable(), "PostgreSQL database not available");

        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var (client, csrfToken) = await LoginAsAdminAndGetCsrfAsync(dbContext);
        Skip.If(csrfToken == null, "Could not obtain CSRF token");

        var request = new CreateUserRequest
        {
            Name = null,
            Email = "validname@example.com",
            Password = "SecurePassword123!"
        };

        var response = await PostWithCsrfAsync(client, "/api/v1/admin/users", request, csrfToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();
        var jsonDoc = JsonDocument.Parse(content);
        
        Assert.True(jsonDoc.RootElement.TryGetProperty("error", out var errorElement));
        Assert.Equal("invalid_input", errorElement.GetProperty("code").GetString());
        
        var details = errorElement.GetProperty("details").EnumerateArray().Select(d => d.GetString()).ToList();
        Assert.Contains("name:required", details);
    }

    [SkippableFact]
    public async Task CreateUser_WithInvalidEmailFormat_Returns400BadRequest()
    {
        Skip.If(!IsPostgreSqlAvailable(), "PostgreSQL database not available");

        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var (client, csrfToken) = await LoginAsAdminAndGetCsrfAsync(dbContext);
        Skip.If(csrfToken == null, "Could not obtain CSRF token");

        var request = new CreateUserRequest
        {
            Name = "Valid Name",
            Email = "not-an-email",
            Password = "SecurePassword123!"
        };

        var response = await PostWithCsrfAsync(client, "/api/v1/admin/users", request, csrfToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();
        var jsonDoc = JsonDocument.Parse(content);
        
        var details = jsonDoc.RootElement.GetProperty("error").GetProperty("details")
            .EnumerateArray().Select(d => d.GetString()).ToList();
        Assert.Contains("email:invalid_format", details);
    }

    [SkippableFact]
    public async Task CreateUser_WithWeakPassword_Returns400BadRequest()
    {
        Skip.If(!IsPostgreSqlAvailable(), "PostgreSQL database not available");

        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var (client, csrfToken) = await LoginAsAdminAndGetCsrfAsync(dbContext);
        Skip.If(csrfToken == null, "Could not obtain CSRF token");

        var request = new CreateUserRequest
        {
            Name = "Valid Name",
            Email = "weakpassword@example.com",
            Password = "weak"
        };

        var response = await PostWithCsrfAsync(client, "/api/v1/admin/users", request, csrfToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();
        var jsonDoc = JsonDocument.Parse(content);
        
        Assert.Equal("invalid_input", jsonDoc.RootElement.GetProperty("error").GetProperty("code").GetString());
        
        var details = jsonDoc.RootElement.GetProperty("error").GetProperty("details")
            .EnumerateArray().Select(d => d.GetString()).ToList();
        Assert.Contains(details, d => d!.StartsWith("password:"));
    }

    [SkippableFact]
    public async Task CreateUser_WithExistingEmail_Returns409Conflict()
    {
        Skip.If(!IsPostgreSqlAvailable(), "PostgreSQL database not available");

        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var (client, csrfToken) = await LoginAsAdminAndGetCsrfAsync(dbContext);
        Skip.If(csrfToken == null, "Could not obtain CSRF token");

        await EnsureStandardUserExistsAsync(dbContext);
        
        var request = new CreateUserRequest
        {
            Name = "Duplicate User",
            Email = StandardEmail,
            Password = "SecurePassword123!"
        };

        var response = await PostWithCsrfAsync(client, "/api/v1/admin/users", request, csrfToken);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();
        var jsonDoc = JsonDocument.Parse(content);
        
        Assert.True(jsonDoc.RootElement.TryGetProperty("error", out var errorElement));
        Assert.Equal("duplicate_email", errorElement.GetProperty("code").GetString());
        Assert.Equal("A user with this email already exists.", errorElement.GetProperty("message").GetString());
    }

    [SkippableFact]
    public async Task CreateUser_WithUppercaseEmail_NormalizesToLowercase()
    {
        Skip.If(!IsPostgreSqlAvailable(), "PostgreSQL database not available");

        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var (client, csrfToken) = await LoginAsAdminAndGetCsrfAsync(dbContext);
        Skip.If(csrfToken == null, "Could not obtain CSRF token");

        var uniqueEmail = $"UPPERCASE_{Guid.NewGuid():N}@EXAMPLE.COM";
        var request = new CreateUserRequest
        {
            Name = "Uppercase Email User",
            Email = uniqueEmail,
            Password = "SecurePassword123!"
        };

        var response = await PostWithCsrfAsync(client, "/api/v1/admin/users", request, csrfToken);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();
        var jsonDoc = JsonDocument.Parse(content);
        
        var returnedEmail = jsonDoc.RootElement.GetProperty("email").GetString();
        Assert.Equal(uniqueEmail.ToLowerInvariant(), returnedEmail);
    }

    #region US_038 Case-Insensitive Duplicate Email Tests

    [SkippableFact]
    public async Task CreateUser_WithDifferentCaseEmail_Returns409Conflict()
    {
        Skip.If(!IsPostgreSqlAvailable(), "PostgreSQL database not available");

        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var (client, csrfToken) = await LoginAsAdminAndGetCsrfAsync(dbContext);
        Skip.If(csrfToken == null, "Could not obtain CSRF token");

        var baseEmail = $"casetest_{Guid.NewGuid():N}@example.com";
        
        var firstRequest = new CreateUserRequest
        {
            Name = "First User",
            Email = baseEmail.ToLowerInvariant(),
            Password = "SecurePassword123!"
        };

        var firstResponse = await PostWithCsrfAsync(client, "/api/v1/admin/users", firstRequest, csrfToken);
        Assert.Equal(HttpStatusCode.Created, firstResponse.StatusCode);

        var differentCaseEmail = baseEmail.ToUpperInvariant();
        var secondRequest = new CreateUserRequest
        {
            Name = "Second User",
            Email = differentCaseEmail,
            Password = "SecurePassword123!"
        };

        var secondResponse = await PostWithCsrfAsync(client, "/api/v1/admin/users", secondRequest, csrfToken);

        Assert.Equal(HttpStatusCode.Conflict, secondResponse.StatusCode);

        var content = await secondResponse.Content.ReadAsStringAsync();
        var jsonDoc = JsonDocument.Parse(content);
        
        Assert.True(jsonDoc.RootElement.TryGetProperty("error", out var errorElement));
        Assert.Equal("duplicate_email", errorElement.GetProperty("code").GetString());
        Assert.Equal("A user with this email already exists.", errorElement.GetProperty("message").GetString());
    }

    [SkippableFact]
    public async Task CreateUser_WithMixedCaseEmail_Returns409Conflict()
    {
        Skip.If(!IsPostgreSqlAvailable(), "PostgreSQL database not available");

        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var (client, csrfToken) = await LoginAsAdminAndGetCsrfAsync(dbContext);
        Skip.If(csrfToken == null, "Could not obtain CSRF token");

        var uniqueId = Guid.NewGuid().ToString("N");
        var lowercaseEmail = $"mixedcase_{uniqueId}@example.com";
        
        var firstRequest = new CreateUserRequest
        {
            Name = "First Mixed Case User",
            Email = lowercaseEmail,
            Password = "SecurePassword123!"
        };

        var firstResponse = await PostWithCsrfAsync(client, "/api/v1/admin/users", firstRequest, csrfToken);
        Assert.Equal(HttpStatusCode.Created, firstResponse.StatusCode);

        var mixedCaseEmail = $"MixedCase_{uniqueId}@Example.COM";
        var secondRequest = new CreateUserRequest
        {
            Name = "Second Mixed Case User",
            Email = mixedCaseEmail,
            Password = "SecurePassword123!"
        };

        var secondResponse = await PostWithCsrfAsync(client, "/api/v1/admin/users", secondRequest, csrfToken);

        Assert.Equal(HttpStatusCode.Conflict, secondResponse.StatusCode);

        var content = await secondResponse.Content.ReadAsStringAsync();
        var jsonDoc = JsonDocument.Parse(content);
        
        Assert.Equal("duplicate_email", jsonDoc.RootElement.GetProperty("error").GetProperty("code").GetString());
    }

    [SkippableFact]
    public async Task CreateUser_DuplicateEmailConflict_DoesNotRevealExistingUserDetails()
    {
        Skip.If(!IsPostgreSqlAvailable(), "PostgreSQL database not available");

        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var (client, csrfToken) = await LoginAsAdminAndGetCsrfAsync(dbContext);
        Skip.If(csrfToken == null, "Could not obtain CSRF token");

        await EnsureStandardUserExistsAsync(dbContext);
        
        var request = new CreateUserRequest
        {
            Name = "Duplicate Attempt User",
            Email = StandardEmail.ToUpperInvariant(),
            Password = "SecurePassword123!"
        };

        var response = await PostWithCsrfAsync(client, "/api/v1/admin/users", request, csrfToken);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();
        var jsonDoc = JsonDocument.Parse(content);
        
        var errorElement = jsonDoc.RootElement.GetProperty("error");
        
        Assert.DoesNotContain("id", content.ToLowerInvariant().Replace("\"id\"", ""));
        Assert.DoesNotContain("role", errorElement.GetProperty("message").GetString()!.ToLowerInvariant());
        Assert.DoesNotContain("status", errorElement.GetProperty("message").GetString()!.ToLowerInvariant());
        Assert.DoesNotContain("standard", errorElement.GetProperty("message").GetString()!.ToLowerInvariant());
        
        var details = errorElement.GetProperty("details").EnumerateArray().Select(d => d.GetString()).ToList();
        Assert.Empty(details);
    }

    #endregion
}
