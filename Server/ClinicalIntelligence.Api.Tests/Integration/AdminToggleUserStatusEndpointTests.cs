using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
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
/// Integration tests for the admin toggle user status endpoint (US_041 TASK_002).
/// Requires PostgreSQL database - tests are skipped when database is unavailable.
/// </summary>
public class AdminToggleUserStatusEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public AdminToggleUserStatusEndpointTests(WebApplicationFactory<Program> factory)
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

    private const string AdminEmail = "admin-toggle-test@example.com";
    private const string AdminPassword = "AdminPassword123!";
    private const string StandardEmail = "standard-toggle-test@example.com";
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
                Name = "Admin Toggle Test User",
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
                Name = "Standard Toggle Test User",
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

    private async Task<User> CreateTestUserAsync(ApplicationDbContext dbContext, string emailPrefix, string status = "Active", bool isStaticAdmin = false)
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = $"{emailPrefix}_{Guid.NewGuid():N}@example.com",
            Name = $"Test User {emailPrefix}",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("TestPassword123!"),
            Role = "Standard",
            Status = status,
            IsStaticAdmin = isStaticAdmin,
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

    private async Task<(HttpClient client, string? csrfToken, User adminUser)> LoginAsAdminAndGetCsrfAsync(ApplicationDbContext dbContext)
    {
        var adminUser = await EnsureAdminUserExistsAsync(dbContext);
        
        var client = CreateClientWithCookies();
        var loginRequest = new { email = AdminEmail, password = AdminPassword };
        var loginResponse = await client.PostAsJsonAsync("/api/v1/auth/login", loginRequest);
        
        if (loginResponse.StatusCode != HttpStatusCode.OK)
        {
            return (client, null, adminUser);
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

        return (client, csrfToken, adminUser);
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

    private async Task<HttpResponseMessage> PatchWithCsrfAsync(HttpClient client, string url, string? csrfToken)
    {
        var request = new HttpRequestMessage(HttpMethod.Patch, url);
        if (!string.IsNullOrEmpty(csrfToken))
        {
            request.Headers.Add(CsrfProtectionMiddleware.CsrfHeaderName, csrfToken);
        }
        return await client.SendAsync(request);
    }

    [SkippableFact]
    public async Task ToggleStatus_AsAdmin_TogglesActiveToInactive()
    {
        Skip.If(!IsPostgreSqlAvailable(), "PostgreSQL database not available");

        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var (client, csrfToken, _) = await LoginAsAdminAndGetCsrfAsync(dbContext);
        Skip.If(csrfToken == null, "Could not obtain CSRF token");

        var targetUser = await CreateTestUserAsync(dbContext, "toggle_active", "Active");

        var response = await PatchWithCsrfAsync(client, $"/api/v1/admin/users/{targetUser.Id}/toggle-status", csrfToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();
        var jsonDoc = JsonDocument.Parse(content);
        
        Assert.Equal("inactive", jsonDoc.RootElement.GetProperty("status").GetString());

        // Verify persistence
        await dbContext.Entry(targetUser).ReloadAsync();
        Assert.Equal("Inactive", targetUser.Status);
    }

    [SkippableFact]
    public async Task ToggleStatus_AsAdmin_TogglesInactiveToActive()
    {
        Skip.If(!IsPostgreSqlAvailable(), "PostgreSQL database not available");

        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var (client, csrfToken, _) = await LoginAsAdminAndGetCsrfAsync(dbContext);
        Skip.If(csrfToken == null, "Could not obtain CSRF token");

        var targetUser = await CreateTestUserAsync(dbContext, "toggle_inactive", "Inactive");

        var response = await PatchWithCsrfAsync(client, $"/api/v1/admin/users/{targetUser.Id}/toggle-status", csrfToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();
        var jsonDoc = JsonDocument.Parse(content);
        
        Assert.Equal("active", jsonDoc.RootElement.GetProperty("status").GetString());

        // Verify persistence
        await dbContext.Entry(targetUser).ReloadAsync();
        Assert.Equal("Active", targetUser.Status);
    }

    [SkippableFact]
    public async Task ToggleStatus_AsAdmin_CreatesUserDeactivatedAuditEvent()
    {
        Skip.If(!IsPostgreSqlAvailable(), "PostgreSQL database not available");

        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var (client, csrfToken, _) = await LoginAsAdminAndGetCsrfAsync(dbContext);
        Skip.If(csrfToken == null, "Could not obtain CSRF token");

        var targetUser = await CreateTestUserAsync(dbContext, "audit_deactivate", "Active");

        var response = await PatchWithCsrfAsync(client, $"/api/v1/admin/users/{targetUser.Id}/toggle-status", csrfToken);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        await Task.Delay(100);

        var auditEvent = await dbContext.AuditLogEvents
            .FirstOrDefaultAsync(a => a.ActionType == "USER_DEACTIVATED" && a.ResourceId == targetUser.Id);

        Assert.NotNull(auditEvent);
        Assert.Equal("User", auditEvent.ResourceType);
        Assert.NotNull(auditEvent.Metadata);
        Assert.Contains("Inactive", auditEvent.Metadata);
    }

    [SkippableFact]
    public async Task ToggleStatus_AsStandardUser_Returns403Forbidden()
    {
        Skip.If(!IsPostgreSqlAvailable(), "PostgreSQL database not available");

        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var (client, csrfToken) = await LoginAsStandardUserAndGetCsrfAsync(dbContext);
        Skip.If(csrfToken == null, "Could not obtain CSRF token");

        var targetUser = await CreateTestUserAsync(dbContext, "forbidden_toggle");

        var response = await PatchWithCsrfAsync(client, $"/api/v1/admin/users/{targetUser.Id}/toggle-status", csrfToken);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();
        var jsonDoc = JsonDocument.Parse(content);
        
        Assert.Equal("forbidden", jsonDoc.RootElement.GetProperty("error").GetProperty("code").GetString());
    }

    [SkippableFact]
    public async Task ToggleStatus_SelfDeactivation_Returns400BadRequest()
    {
        Skip.If(!IsPostgreSqlAvailable(), "PostgreSQL database not available");

        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var (client, csrfToken, adminUser) = await LoginAsAdminAndGetCsrfAsync(dbContext);
        Skip.If(csrfToken == null, "Could not obtain CSRF token");

        // Try to deactivate self
        var response = await PatchWithCsrfAsync(client, $"/api/v1/admin/users/{adminUser.Id}/toggle-status", csrfToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();
        var jsonDoc = JsonDocument.Parse(content);
        
        var details = jsonDoc.RootElement.GetProperty("error").GetProperty("details")
            .EnumerateArray().Select(d => d.GetString()).ToList();
        Assert.Contains("userId:self_status_change", details);
    }

    [SkippableFact]
    public async Task ToggleStatus_StaticAdmin_Returns403Forbidden()
    {
        Skip.If(!IsPostgreSqlAvailable(), "PostgreSQL database not available");

        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var (client, csrfToken, _) = await LoginAsAdminAndGetCsrfAsync(dbContext);
        Skip.If(csrfToken == null, "Could not obtain CSRF token");

        // Find or create static admin
        var staticAdmin = await dbContext.Users
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.IsStaticAdmin);

        if (staticAdmin == null)
        {
            staticAdmin = await CreateTestUserAsync(dbContext, "static_admin", "Active", isStaticAdmin: true);
        }

        var response = await PatchWithCsrfAsync(client, $"/api/v1/admin/users/{staticAdmin.Id}/toggle-status", csrfToken);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();
        var jsonDoc = JsonDocument.Parse(content);
        
        Assert.Equal("static_admin_protected", jsonDoc.RootElement.GetProperty("error").GetProperty("code").GetString());
    }

    [SkippableFact]
    public async Task ToggleStatus_WithInvalidUserId_Returns400BadRequest()
    {
        Skip.If(!IsPostgreSqlAvailable(), "PostgreSQL database not available");

        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var (client, csrfToken, _) = await LoginAsAdminAndGetCsrfAsync(dbContext);
        Skip.If(csrfToken == null, "Could not obtain CSRF token");

        var response = await PatchWithCsrfAsync(client, "/api/v1/admin/users/not-a-guid/toggle-status", csrfToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [SkippableFact]
    public async Task ToggleStatus_WithNonExistentUser_Returns404NotFound()
    {
        Skip.If(!IsPostgreSqlAvailable(), "PostgreSQL database not available");

        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var (client, csrfToken, _) = await LoginAsAdminAndGetCsrfAsync(dbContext);
        Skip.If(csrfToken == null, "Could not obtain CSRF token");

        var response = await PatchWithCsrfAsync(client, $"/api/v1/admin/users/{Guid.NewGuid()}/toggle-status", csrfToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
