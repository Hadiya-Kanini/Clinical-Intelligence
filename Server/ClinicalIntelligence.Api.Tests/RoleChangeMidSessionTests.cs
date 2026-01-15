using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using ClinicalIntelligence.Api.Contracts;
using ClinicalIntelligence.Api.Data;
using ClinicalIntelligence.Api.Domain.Models;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace ClinicalIntelligence.Api.Tests;

/// <summary>
/// Integration tests for role change mid-session handling (US_033 TASK_002, TASK_004).
/// Validates that session is invalidated when user's role is changed in the database.
/// </summary>
public class RoleChangeMidSessionTests : IClassFixture<TestWebApplicationFactory<Program>>
{
    private readonly TestWebApplicationFactory<Program> _factory;

    public RoleChangeMidSessionTests(TestWebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Request_AfterRoleDowngrade_Returns401WithSessionInvalidated()
    {
        // Arrange - Create a unique user for this test to avoid interference
        var testUserId = Guid.NewGuid();
        var testEmail = $"roletest-{testUserId:N}@example.com";
        var testPassword = "TestPassword123!";

        using (var scope = _factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var user = new User
            {
                Id = testUserId,
                Email = testEmail,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(testPassword),
                Name = "Role Test User",
                Role = "Admin", // Start as Admin
                Status = "Active",
                IsStaticAdmin = false,
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            dbContext.Users.Add(user);
            await dbContext.SaveChangesAsync();
        }

        var client = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        // Login as Admin
        var loginResponse = await client.PostAsJsonAsync("/api/v1/auth/login", 
            new LoginRequest(testEmail, testPassword));
        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);

        // Extract cookies
        var cookies = loginResponse.Headers.GetValues("Set-Cookie");
        var cookieHeader = string.Join("; ", cookies.Select(c => c.Split(';')[0]));

        // Verify initial access works (admin can access /health/db)
        var initialRequest = new HttpRequestMessage(HttpMethod.Get, "/health/db");
        initialRequest.Headers.Add("Cookie", cookieHeader);
        var initialResponse = await client.SendAsync(initialRequest);
        Assert.Equal(HttpStatusCode.OK, initialResponse.StatusCode);

        // Change user's role in DB from Admin to Standard (simulate admin action)
        using (var scope = _factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var user = await dbContext.Users.FirstOrDefaultAsync(u => u.Id == testUserId);
            Assert.NotNull(user);
            user!.Role = "Standard"; // Downgrade to Standard
            user.UpdatedAt = DateTime.UtcNow;
            await dbContext.SaveChangesAsync();
        }

        // Act - Make another request with the same session (JWT still has Admin role)
        var subsequentRequest = new HttpRequestMessage(HttpMethod.Get, "/api/v1/auth/me");
        subsequentRequest.Headers.Add("Cookie", cookieHeader);
        var subsequentResponse = await client.SendAsync(subsequentRequest);

        // Assert - Should return 401 with session_invalidated code
        Assert.Equal(HttpStatusCode.Unauthorized, subsequentResponse.StatusCode);

        var content = await subsequentResponse.Content.ReadAsStringAsync();
        var jsonDoc = JsonDocument.Parse(content);
        
        // Response structure uses {"error":{"code":"..."}} format
        var errorElement = jsonDoc.RootElement.GetProperty("error");
        Assert.Equal("session_invalidated", errorElement.GetProperty("code").GetString());
    }

    [Fact]
    public async Task Request_AfterRoleUpgrade_Returns401WithSessionInvalidated()
    {
        // Arrange - Create a unique user for this test
        var testUserId = Guid.NewGuid();
        var testEmail = $"roletest-upgrade-{testUserId:N}@example.com";
        var testPassword = "TestPassword123!";

        using (var scope = _factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var user = new User
            {
                Id = testUserId,
                Email = testEmail,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(testPassword),
                Name = "Role Upgrade Test User",
                Role = "Standard", // Start as Standard
                Status = "Active",
                IsStaticAdmin = false,
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            dbContext.Users.Add(user);
            await dbContext.SaveChangesAsync();
        }

        var client = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        // Login as Standard
        var loginResponse = await client.PostAsJsonAsync("/api/v1/auth/login", 
            new LoginRequest(testEmail, testPassword));
        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);

        // Extract cookies
        var cookies = loginResponse.Headers.GetValues("Set-Cookie");
        var cookieHeader = string.Join("; ", cookies.Select(c => c.Split(';')[0]));

        // Change user's role in DB from Standard to Admin (simulate promotion)
        // Do this BEFORE making any authenticated request so we can test the mismatch detection
        using (var scope = _factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var user = await dbContext.Users.FirstOrDefaultAsync(u => u.Id == testUserId);
            Assert.NotNull(user);
            user!.Role = "Admin"; // Upgrade to Admin
            user.UpdatedAt = DateTime.UtcNow;
            await dbContext.SaveChangesAsync();
        }

        // Act - Make request with the session (JWT has Standard role, DB has Admin)
        var subsequentRequest = new HttpRequestMessage(HttpMethod.Get, "/api/v1/auth/me");
        subsequentRequest.Headers.Add("Cookie", cookieHeader);
        var subsequentResponse = await client.SendAsync(subsequentRequest);

        // Assert - Should return 401 with session_invalidated code
        // User must re-login to get new JWT with Admin role
        Assert.Equal(HttpStatusCode.Unauthorized, subsequentResponse.StatusCode);

        var content = await subsequentResponse.Content.ReadAsStringAsync();
        var jsonDoc = JsonDocument.Parse(content);
        
        // Response structure uses {"error":{"code":"..."}} format
        var errorElement = jsonDoc.RootElement.GetProperty("error");
        Assert.Equal("session_invalidated", errorElement.GetProperty("code").GetString());
    }

    [Fact]
    public async Task RoleChangeMidSession_CreatesAuditLogEntry()
    {
        // Arrange - Create a unique user for this test
        var testUserId = Guid.NewGuid();
        var testEmail = $"roletest-audit-{testUserId:N}@example.com";
        var testPassword = "TestPassword123!";

        using (var scope = _factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var user = new User
            {
                Id = testUserId,
                Email = testEmail,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(testPassword),
                Name = "Role Audit Test User",
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

        var client = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        // Login
        var loginResponse = await client.PostAsJsonAsync("/api/v1/auth/login", 
            new LoginRequest(testEmail, testPassword));
        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);

        var cookies = loginResponse.Headers.GetValues("Set-Cookie");
        var cookieHeader = string.Join("; ", cookies.Select(c => c.Split(';')[0]));

        // Change role in DB
        using (var scope = _factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var user = await dbContext.Users.FirstOrDefaultAsync(u => u.Id == testUserId);
            user!.Role = "Standard";
            await dbContext.SaveChangesAsync();
        }

        // Trigger role mismatch detection
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/v1/auth/me");
        request.Headers.Add("Cookie", cookieHeader);
        await client.SendAsync(request);

        // Assert - Check audit log entry was created
        using (var scope = _factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            
            // Wait briefly for async audit log write
            await Task.Delay(100);
            
            var auditEntry = await dbContext.AuditLogEvents
                .FirstOrDefaultAsync(a => a.UserId == testUserId && 
                                          a.ActionType == "ROLE_CHANGE_SESSION_INVALIDATED");
            
            Assert.NotNull(auditEntry);
            Assert.Equal("Session", auditEntry!.ResourceType);
            Assert.NotNull(auditEntry.Metadata);
            Assert.Contains("previousRole", auditEntry.Metadata);
            Assert.Contains("newRole", auditEntry.Metadata);
        }
    }
}
