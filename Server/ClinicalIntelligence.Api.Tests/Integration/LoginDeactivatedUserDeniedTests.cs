using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using ClinicalIntelligence.Api.Data;
using ClinicalIntelligence.Api.Domain.Models;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace ClinicalIntelligence.Api.Tests.Integration;

/// <summary>
/// Integration tests for login denial of deactivated users (US_041 TASK_003).
/// Requires PostgreSQL database - tests are skipped when database is unavailable.
/// </summary>
public class LoginDeactivatedUserDeniedTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public LoginDeactivatedUserDeniedTests(WebApplicationFactory<Program> factory)
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

    private const string TestPassword = "TestPassword123!";

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

    private async Task<User> CreateTestUserAsync(ApplicationDbContext dbContext, string emailPrefix, string status)
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = $"{emailPrefix}_{Guid.NewGuid():N}@example.com",
            Name = $"Test User {emailPrefix}",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(TestPassword),
            Role = "Standard",
            Status = status,
            IsStaticAdmin = false,
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();
        return user;
    }

    private HttpClient CreateClient()
    {
        return _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            HandleCookies = true
        });
    }

    [SkippableFact]
    public async Task Login_WithInactiveUser_ValidCredentials_Returns403WithAccountInactive()
    {
        Skip.If(!IsPostgreSqlAvailable(), "PostgreSQL database not available");

        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var inactiveUser = await CreateTestUserAsync(dbContext, "inactive_login", "Inactive");

        var client = CreateClient();
        var loginRequest = new { email = inactiveUser.Email, password = TestPassword };
        
        var response = await client.PostAsJsonAsync("/api/v1/auth/login", loginRequest);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();
        var jsonDoc = JsonDocument.Parse(content);
        
        Assert.True(jsonDoc.RootElement.TryGetProperty("error", out var errorElement));
        Assert.Equal("account_inactive", errorElement.GetProperty("code").GetString());
        Assert.Contains("deactivated", errorElement.GetProperty("message").GetString()!.ToLowerInvariant());
    }

    [SkippableFact]
    public async Task Login_WithInactiveUser_WrongPassword_Returns401InvalidCredentials()
    {
        Skip.If(!IsPostgreSqlAvailable(), "PostgreSQL database not available");

        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var inactiveUser = await CreateTestUserAsync(dbContext, "inactive_wrong_pw", "Inactive");

        var client = CreateClient();
        var loginRequest = new { email = inactiveUser.Email, password = "WrongPassword123!" };
        
        var response = await client.PostAsJsonAsync("/api/v1/auth/login", loginRequest);

        // Should return 401 for wrong password, not 403 for inactive
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();
        var jsonDoc = JsonDocument.Parse(content);
        
        Assert.Equal("invalid_credentials", jsonDoc.RootElement.GetProperty("error").GetProperty("code").GetString());
    }

    [SkippableFact]
    public async Task Login_WithActiveUser_ValidCredentials_Returns200()
    {
        Skip.If(!IsPostgreSqlAvailable(), "PostgreSQL database not available");

        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var activeUser = await CreateTestUserAsync(dbContext, "active_login", "Active");

        var client = CreateClient();
        var loginRequest = new { email = activeUser.Email, password = TestPassword };
        
        var response = await client.PostAsJsonAsync("/api/v1/auth/login", loginRequest);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [SkippableFact]
    public async Task Login_WithNonExistentUser_Returns401InvalidCredentials()
    {
        Skip.If(!IsPostgreSqlAvailable(), "PostgreSQL database not available");

        var client = CreateClient();
        var loginRequest = new { email = $"nonexistent_{Guid.NewGuid():N}@example.com", password = TestPassword };
        
        var response = await client.PostAsJsonAsync("/api/v1/auth/login", loginRequest);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();
        var jsonDoc = JsonDocument.Parse(content);
        
        Assert.Equal("invalid_credentials", jsonDoc.RootElement.GetProperty("error").GetProperty("code").GetString());
    }

    [SkippableFact]
    public async Task Login_WithLockedUser_Returns403AccountLocked()
    {
        Skip.If(!IsPostgreSqlAvailable(), "PostgreSQL database not available");

        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var lockedUser = new User
        {
            Id = Guid.NewGuid(),
            Email = $"locked_{Guid.NewGuid():N}@example.com",
            Name = "Locked User",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(TestPassword),
            Role = "Standard",
            Status = "Active",
            IsStaticAdmin = false,
            IsDeleted = false,
            LockedUntil = DateTime.UtcNow.AddMinutes(30),
            FailedLoginAttempts = 5,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        dbContext.Users.Add(lockedUser);
        await dbContext.SaveChangesAsync();

        var client = CreateClient();
        var loginRequest = new { email = lockedUser.Email, password = TestPassword };
        
        var response = await client.PostAsJsonAsync("/api/v1/auth/login", loginRequest);

        // Lockout should take precedence
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();
        var jsonDoc = JsonDocument.Parse(content);
        
        Assert.Equal("account_locked", jsonDoc.RootElement.GetProperty("error").GetProperty("code").GetString());
    }

    [SkippableFact]
    public async Task Login_InactiveUserMessage_ContainsContactAdministrator()
    {
        Skip.If(!IsPostgreSqlAvailable(), "PostgreSQL database not available");

        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var inactiveUser = await CreateTestUserAsync(dbContext, "inactive_msg", "Inactive");

        var client = CreateClient();
        var loginRequest = new { email = inactiveUser.Email, password = TestPassword };
        
        var response = await client.PostAsJsonAsync("/api/v1/auth/login", loginRequest);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();
        var jsonDoc = JsonDocument.Parse(content);
        
        var message = jsonDoc.RootElement.GetProperty("error").GetProperty("message").GetString()!;
        Assert.Contains("administrator", message.ToLowerInvariant());
    }
}
