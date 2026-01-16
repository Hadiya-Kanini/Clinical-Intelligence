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
/// Integration tests for the admin update user endpoint (US_041 TASK_001).
/// Requires PostgreSQL database - tests are skipped when database is unavailable.
/// </summary>
public class AdminUpdateUserEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public AdminUpdateUserEndpointTests(WebApplicationFactory<Program> factory)
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

    private const string AdminEmail = "admin-update-test@example.com";
    private const string AdminPassword = "AdminPassword123!";
    private const string StandardEmail = "standard-update-test@example.com";
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
                Name = "Admin Update Test User",
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
                Name = "Standard Update Test User",
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

    private async Task<User> CreateTestUserAsync(ApplicationDbContext dbContext, string emailPrefix)
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = $"{emailPrefix}_{Guid.NewGuid():N}@example.com",
            Name = $"Test User {emailPrefix}",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("TestPassword123!"),
            Role = "Standard",
            Status = "Active",
            IsStaticAdmin = false,
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();
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

    private async Task<HttpResponseMessage> PutWithCsrfAsync(HttpClient client, string url, object content, string? csrfToken)
    {
        var request = new HttpRequestMessage(HttpMethod.Put, url);
        request.Content = JsonContent.Create(content);
        if (!string.IsNullOrEmpty(csrfToken))
        {
            request.Headers.Add(CsrfProtectionMiddleware.CsrfHeaderName, csrfToken);
        }
        return await client.SendAsync(request);
    }

    [SkippableFact]
    public async Task UpdateUser_AsAdmin_WithValidRequest_Returns200AndUpdatesUser()
    {
        Skip.If(!IsPostgreSqlAvailable(), "PostgreSQL database not available");

        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var (client, csrfToken) = await LoginAsAdminAndGetCsrfAsync(dbContext);
        Skip.If(csrfToken == null, "Could not obtain CSRF token");

        var targetUser = await CreateTestUserAsync(dbContext, "update_target");
        var newEmail = $"updated_{Guid.NewGuid():N}@example.com";

        var request = new UpdateUserRequest
        {
            Name = "Updated Name",
            Email = newEmail,
            Role = "admin"
        };

        var response = await PutWithCsrfAsync(client, $"/api/v1/admin/users/{targetUser.Id}", request, csrfToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();
        var jsonDoc = JsonDocument.Parse(content);
        var root = jsonDoc.RootElement;

        Assert.Equal(targetUser.Id.ToString(), root.GetProperty("id").GetString());
        Assert.Equal("Updated Name", root.GetProperty("name").GetString());
        Assert.Equal(newEmail.ToLowerInvariant(), root.GetProperty("email").GetString());
        Assert.Equal("admin", root.GetProperty("role").GetString());

        // Verify persistence
        await dbContext.Entry(targetUser).ReloadAsync();
        Assert.Equal("Updated Name", targetUser.Name);
        Assert.Equal(newEmail.ToLowerInvariant(), targetUser.Email);
        Assert.Equal("Admin", targetUser.Role);
    }

    [SkippableFact]
    public async Task UpdateUser_AsAdmin_CreatesUserUpdatedAuditEvent()
    {
        Skip.If(!IsPostgreSqlAvailable(), "PostgreSQL database not available");

        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var (client, csrfToken) = await LoginAsAdminAndGetCsrfAsync(dbContext);
        Skip.If(csrfToken == null, "Could not obtain CSRF token");

        var targetUser = await CreateTestUserAsync(dbContext, "audit_update");

        var request = new UpdateUserRequest
        {
            Name = "Audit Updated Name",
            Email = targetUser.Email,
            Role = "standard"
        };

        var response = await PutWithCsrfAsync(client, $"/api/v1/admin/users/{targetUser.Id}", request, csrfToken);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        await Task.Delay(100);

        var auditEvent = await dbContext.AuditLogEvents
            .FirstOrDefaultAsync(a => a.ActionType == "USER_UPDATED" && a.ResourceId == targetUser.Id);

        Assert.NotNull(auditEvent);
        Assert.Equal("User", auditEvent.ResourceType);
        Assert.NotNull(auditEvent.Metadata);
    }

    [SkippableFact]
    public async Task UpdateUser_AsStandardUser_Returns403Forbidden()
    {
        Skip.If(!IsPostgreSqlAvailable(), "PostgreSQL database not available");

        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var (client, csrfToken) = await LoginAsStandardUserAndGetCsrfAsync(dbContext);
        Skip.If(csrfToken == null, "Could not obtain CSRF token");

        var targetUser = await CreateTestUserAsync(dbContext, "forbidden_update");

        var request = new UpdateUserRequest
        {
            Name = "Should Not Update",
            Email = targetUser.Email,
            Role = "standard"
        };

        var response = await PutWithCsrfAsync(client, $"/api/v1/admin/users/{targetUser.Id}", request, csrfToken);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();
        var jsonDoc = JsonDocument.Parse(content);
        
        Assert.True(jsonDoc.RootElement.TryGetProperty("error", out var errorElement));
        Assert.Equal("forbidden", errorElement.GetProperty("code").GetString());
    }

    [SkippableFact]
    public async Task UpdateUser_WithDuplicateEmail_Returns409Conflict()
    {
        Skip.If(!IsPostgreSqlAvailable(), "PostgreSQL database not available");

        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var (client, csrfToken) = await LoginAsAdminAndGetCsrfAsync(dbContext);
        Skip.If(csrfToken == null, "Could not obtain CSRF token");

        var existingUser = await CreateTestUserAsync(dbContext, "existing_email");
        var targetUser = await CreateTestUserAsync(dbContext, "target_dup");

        var request = new UpdateUserRequest
        {
            Name = "Duplicate Email Test",
            Email = existingUser.Email,
            Role = "standard"
        };

        var response = await PutWithCsrfAsync(client, $"/api/v1/admin/users/{targetUser.Id}", request, csrfToken);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();
        var jsonDoc = JsonDocument.Parse(content);
        
        Assert.Equal("duplicate_email", jsonDoc.RootElement.GetProperty("error").GetProperty("code").GetString());
    }

    [SkippableFact]
    public async Task UpdateUser_WithSameEmail_Returns200()
    {
        Skip.If(!IsPostgreSqlAvailable(), "PostgreSQL database not available");

        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var (client, csrfToken) = await LoginAsAdminAndGetCsrfAsync(dbContext);
        Skip.If(csrfToken == null, "Could not obtain CSRF token");

        var targetUser = await CreateTestUserAsync(dbContext, "same_email");

        var request = new UpdateUserRequest
        {
            Name = "Updated Name Same Email",
            Email = targetUser.Email,
            Role = "admin"
        };

        var response = await PutWithCsrfAsync(client, $"/api/v1/admin/users/{targetUser.Id}", request, csrfToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [SkippableFact]
    public async Task UpdateUser_WithInvalidUserId_Returns400BadRequest()
    {
        Skip.If(!IsPostgreSqlAvailable(), "PostgreSQL database not available");

        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var (client, csrfToken) = await LoginAsAdminAndGetCsrfAsync(dbContext);
        Skip.If(csrfToken == null, "Could not obtain CSRF token");

        var request = new UpdateUserRequest
        {
            Name = "Test",
            Email = "test@example.com",
            Role = "standard"
        };

        var response = await PutWithCsrfAsync(client, "/api/v1/admin/users/not-a-guid", request, csrfToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [SkippableFact]
    public async Task UpdateUser_WithNonExistentUser_Returns404NotFound()
    {
        Skip.If(!IsPostgreSqlAvailable(), "PostgreSQL database not available");

        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var (client, csrfToken) = await LoginAsAdminAndGetCsrfAsync(dbContext);
        Skip.If(csrfToken == null, "Could not obtain CSRF token");

        var request = new UpdateUserRequest
        {
            Name = "Test",
            Email = "test@example.com",
            Role = "standard"
        };

        var response = await PutWithCsrfAsync(client, $"/api/v1/admin/users/{Guid.NewGuid()}", request, csrfToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [SkippableFact]
    public async Task UpdateUser_WithInvalidRole_Returns400BadRequest()
    {
        Skip.If(!IsPostgreSqlAvailable(), "PostgreSQL database not available");

        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var (client, csrfToken) = await LoginAsAdminAndGetCsrfAsync(dbContext);
        Skip.If(csrfToken == null, "Could not obtain CSRF token");

        var targetUser = await CreateTestUserAsync(dbContext, "invalid_role");

        var request = new UpdateUserRequest
        {
            Name = "Test",
            Email = targetUser.Email,
            Role = "superadmin"
        };

        var response = await PutWithCsrfAsync(client, $"/api/v1/admin/users/{targetUser.Id}", request, csrfToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();
        var jsonDoc = JsonDocument.Parse(content);
        
        var details = jsonDoc.RootElement.GetProperty("error").GetProperty("details")
            .EnumerateArray().Select(d => d.GetString()).ToList();
        Assert.Contains("role:invalid_value", details);
    }
}
