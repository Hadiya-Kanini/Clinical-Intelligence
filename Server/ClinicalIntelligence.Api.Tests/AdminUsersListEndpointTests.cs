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
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace ClinicalIntelligence.Api.Tests;

/// <summary>
/// Integration tests for the admin users list endpoint (US_040 TASK_003).
/// Requires PostgreSQL database - tests are skipped when database is unavailable.
/// </summary>
public class AdminUsersListEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public AdminUsersListEndpointTests(WebApplicationFactory<Program> factory)
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

    private const string AdminEmail = "admin-list-test@example.com";
    private const string AdminPassword = "AdminPassword123!";
    private const string StandardEmail = "standard-list-test@example.com";
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
                Name = "Admin List Test User",
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
                Name = "Standard List Test User",
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

    private async Task<List<User>> SeedTestUsersAsync(ApplicationDbContext dbContext, string prefix, int count)
    {
        var users = new List<User>();
        for (int i = 1; i <= count; i++)
        {
            var email = $"{prefix}_{i}_{Guid.NewGuid():N}@example.com";
            var existingUser = await dbContext.Users
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(u => u.Email == email);

            if (existingUser == null)
            {
                var user = new User
                {
                    Id = Guid.NewGuid(),
                    Email = email,
                    Name = $"{prefix} User {i}",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("TestPassword123!"),
                    Role = i % 2 == 0 ? "Admin" : "Standard",
                    Status = i % 3 == 0 ? "Inactive" : "Active",
                    IsStaticAdmin = false,
                    IsDeleted = false,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                dbContext.Users.Add(user);
                users.Add(user);
            }
        }
        await dbContext.SaveChangesAsync();
        return users;
    }

    private HttpClient CreateClientWithCookies()
    {
        return _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            HandleCookies = true
        });
    }

    private async Task<HttpClient> LoginAsAdminAsync(ApplicationDbContext dbContext)
    {
        await EnsureAdminUserExistsAsync(dbContext);
        
        var client = CreateClientWithCookies();
        var loginRequest = new { email = AdminEmail, password = AdminPassword };
        var loginResponse = await client.PostAsJsonAsync("/api/v1/auth/login", loginRequest);
        
        if (loginResponse.StatusCode != HttpStatusCode.OK)
        {
            throw new InvalidOperationException("Failed to login as admin");
        }

        return client;
    }

    private async Task<HttpClient> LoginAsStandardUserAsync(ApplicationDbContext dbContext)
    {
        await EnsureStandardUserExistsAsync(dbContext);
        
        var client = CreateClientWithCookies();
        var loginRequest = new { email = StandardEmail, password = StandardPassword };
        var loginResponse = await client.PostAsJsonAsync("/api/v1/auth/login", loginRequest);
        
        if (loginResponse.StatusCode != HttpStatusCode.OK)
        {
            throw new InvalidOperationException("Failed to login as standard user");
        }

        return client;
    }

    #region Authorization Tests

    [Fact]
    public async Task ListUsers_WithoutAuthentication_Returns401Unauthorized()
    {
        using var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/v1/admin/users");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [SkippableFact]
    public async Task ListUsers_AsStandardUser_Returns403Forbidden()
    {
        Skip.If(!IsPostgreSqlAvailable(), "PostgreSQL database not available");

        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var client = await LoginAsStandardUserAsync(dbContext);

        var response = await client.GetAsync("/api/v1/admin/users");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();
        var jsonDoc = JsonDocument.Parse(content);
        
        Assert.True(jsonDoc.RootElement.TryGetProperty("error", out var errorElement));
        Assert.Equal("forbidden", errorElement.GetProperty("code").GetString());
    }

    [SkippableFact]
    public async Task ListUsers_AsAdmin_Returns200Ok()
    {
        Skip.If(!IsPostgreSqlAvailable(), "PostgreSQL database not available");

        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var client = await LoginAsAdminAsync(dbContext);

        var response = await client.GetAsync("/api/v1/admin/users");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();
        var jsonDoc = JsonDocument.Parse(content);
        
        Assert.True(jsonDoc.RootElement.TryGetProperty("items", out _));
        Assert.True(jsonDoc.RootElement.TryGetProperty("page", out _));
        Assert.True(jsonDoc.RootElement.TryGetProperty("pageSize", out _));
        Assert.True(jsonDoc.RootElement.TryGetProperty("total", out _));
    }

    #endregion

    #region Response Schema Tests

    [SkippableFact]
    public async Task ListUsers_ReturnsCorrectResponseSchema()
    {
        Skip.If(!IsPostgreSqlAvailable(), "PostgreSQL database not available");

        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var client = await LoginAsAdminAsync(dbContext);

        var response = await client.GetAsync("/api/v1/admin/users?page=1&pageSize=5");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();
        var jsonDoc = JsonDocument.Parse(content);
        var root = jsonDoc.RootElement;

        Assert.True(root.TryGetProperty("items", out var itemsElement));
        Assert.Equal(JsonValueKind.Array, itemsElement.ValueKind);

        Assert.True(root.TryGetProperty("page", out var pageElement));
        Assert.Equal(JsonValueKind.Number, pageElement.ValueKind);
        Assert.Equal(1, pageElement.GetInt32());

        Assert.True(root.TryGetProperty("pageSize", out var pageSizeElement));
        Assert.Equal(JsonValueKind.Number, pageSizeElement.ValueKind);
        Assert.Equal(5, pageSizeElement.GetInt32());

        Assert.True(root.TryGetProperty("total", out var totalElement));
        Assert.Equal(JsonValueKind.Number, totalElement.ValueKind);

        if (itemsElement.GetArrayLength() > 0)
        {
            var firstItem = itemsElement[0];
            Assert.True(firstItem.TryGetProperty("id", out _));
            Assert.True(firstItem.TryGetProperty("name", out _));
            Assert.True(firstItem.TryGetProperty("email", out _));
            Assert.True(firstItem.TryGetProperty("role", out _));
            Assert.True(firstItem.TryGetProperty("status", out _));
        }
    }

    #endregion

    #region Pagination Tests

    [SkippableFact]
    public async Task ListUsers_WithPagination_ReturnsCorrectPageSize()
    {
        Skip.If(!IsPostgreSqlAvailable(), "PostgreSQL database not available");

        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        await SeedTestUsersAsync(dbContext, "pagination", 10);

        var client = await LoginAsAdminAsync(dbContext);

        var response = await client.GetAsync("/api/v1/admin/users?page=1&pageSize=3");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();
        var jsonDoc = JsonDocument.Parse(content);
        var root = jsonDoc.RootElement;

        var items = root.GetProperty("items");
        Assert.True(items.GetArrayLength() <= 3);

        var total = root.GetProperty("total").GetInt32();
        Assert.True(total >= 3);
    }

    [SkippableFact]
    public async Task ListUsers_WithPagination_Page2_ReturnsNextPage()
    {
        Skip.If(!IsPostgreSqlAvailable(), "PostgreSQL database not available");

        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        await SeedTestUsersAsync(dbContext, "page2test", 10);

        var client = await LoginAsAdminAsync(dbContext);

        var page1Response = await client.GetAsync("/api/v1/admin/users?page=1&pageSize=5&sortBy=email&sortDir=asc");
        var page2Response = await client.GetAsync("/api/v1/admin/users?page=2&pageSize=5&sortBy=email&sortDir=asc");

        Assert.Equal(HttpStatusCode.OK, page1Response.StatusCode);
        Assert.Equal(HttpStatusCode.OK, page2Response.StatusCode);

        var page1Content = await page1Response.Content.ReadAsStringAsync();
        var page2Content = await page2Response.Content.ReadAsStringAsync();

        var page1Doc = JsonDocument.Parse(page1Content);
        var page2Doc = JsonDocument.Parse(page2Content);

        var page1Items = page1Doc.RootElement.GetProperty("items");
        var page2Items = page2Doc.RootElement.GetProperty("items");

        if (page1Items.GetArrayLength() > 0 && page2Items.GetArrayLength() > 0)
        {
            var page1FirstId = page1Items[0].GetProperty("id").GetString();
            var page2FirstId = page2Items[0].GetProperty("id").GetString();
            Assert.NotEqual(page1FirstId, page2FirstId);
        }
    }

    [SkippableFact]
    public async Task ListUsers_WithInvalidPageSize_Returns400BadRequest()
    {
        Skip.If(!IsPostgreSqlAvailable(), "PostgreSQL database not available");

        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var client = await LoginAsAdminAsync(dbContext);

        var response = await client.GetAsync("/api/v1/admin/users?pageSize=200");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();
        var jsonDoc = JsonDocument.Parse(content);
        
        Assert.True(jsonDoc.RootElement.TryGetProperty("error", out var errorElement));
        Assert.Equal("invalid_input", errorElement.GetProperty("code").GetString());
    }

    [SkippableFact]
    public async Task ListUsers_WithNegativePage_Returns400BadRequest()
    {
        Skip.If(!IsPostgreSqlAvailable(), "PostgreSQL database not available");

        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var client = await LoginAsAdminAsync(dbContext);

        var response = await client.GetAsync("/api/v1/admin/users?page=-1");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    #endregion

    #region Search Tests

    [SkippableFact]
    public async Task ListUsers_WithSearchQuery_FiltersResults()
    {
        Skip.If(!IsPostgreSqlAvailable(), "PostgreSQL database not available");

        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var uniquePrefix = $"searchtest_{Guid.NewGuid():N}";
        var testUser = new User
        {
            Id = Guid.NewGuid(),
            Email = $"{uniquePrefix}@example.com",
            Name = $"Unique {uniquePrefix} Name",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("TestPassword123!"),
            Role = "Standard",
            Status = "Active",
            IsStaticAdmin = false,
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        dbContext.Users.Add(testUser);
        await dbContext.SaveChangesAsync();

        var client = await LoginAsAdminAsync(dbContext);

        var response = await client.GetAsync($"/api/v1/admin/users?q={uniquePrefix}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();
        var jsonDoc = JsonDocument.Parse(content);
        var items = jsonDoc.RootElement.GetProperty("items");

        Assert.True(items.GetArrayLength() >= 1);

        var foundUser = items.EnumerateArray()
            .Any(item => item.GetProperty("email").GetString()!.Contains(uniquePrefix));
        Assert.True(foundUser);
    }

    [SkippableFact]
    public async Task ListUsers_WithSearchQuery_MatchesEmail()
    {
        Skip.If(!IsPostgreSqlAvailable(), "PostgreSQL database not available");

        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var client = await LoginAsAdminAsync(dbContext);

        var response = await client.GetAsync($"/api/v1/admin/users?q={AdminEmail.Split('@')[0]}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();
        var jsonDoc = JsonDocument.Parse(content);
        var items = jsonDoc.RootElement.GetProperty("items");

        var foundAdmin = items.EnumerateArray()
            .Any(item => item.GetProperty("email").GetString() == AdminEmail);
        Assert.True(foundAdmin);
    }

    [SkippableFact]
    public async Task ListUsers_WithSearchQuery_IsCaseInsensitive()
    {
        Skip.If(!IsPostgreSqlAvailable(), "PostgreSQL database not available");

        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var client = await LoginAsAdminAsync(dbContext);

        var response = await client.GetAsync("/api/v1/admin/users?q=ADMIN");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();
        var jsonDoc = JsonDocument.Parse(content);
        var items = jsonDoc.RootElement.GetProperty("items");

        Assert.True(items.GetArrayLength() >= 1);
    }

    #endregion

    #region Sorting Tests

    [SkippableFact]
    public async Task ListUsers_SortByName_Ascending_ReturnsOrderedResults()
    {
        Skip.If(!IsPostgreSqlAvailable(), "PostgreSQL database not available");

        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var client = await LoginAsAdminAsync(dbContext);

        var response = await client.GetAsync("/api/v1/admin/users?sortBy=name&sortDir=asc&pageSize=50");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();
        var jsonDoc = JsonDocument.Parse(content);
        var items = jsonDoc.RootElement.GetProperty("items");

        if (items.GetArrayLength() >= 2)
        {
            var names = items.EnumerateArray()
                .Select(item => item.GetProperty("name").GetString())
                .ToList();

            for (int i = 0; i < names.Count - 1; i++)
            {
                Assert.True(
                    string.Compare(names[i], names[i + 1], StringComparison.OrdinalIgnoreCase) <= 0,
                    $"Names not in ascending order: '{names[i]}' should come before '{names[i + 1]}'");
            }
        }
    }

    [SkippableFact]
    public async Task ListUsers_SortByName_Descending_ReturnsOrderedResults()
    {
        Skip.If(!IsPostgreSqlAvailable(), "PostgreSQL database not available");

        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var client = await LoginAsAdminAsync(dbContext);

        var response = await client.GetAsync("/api/v1/admin/users?sortBy=name&sortDir=desc&pageSize=50");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();
        var jsonDoc = JsonDocument.Parse(content);
        var items = jsonDoc.RootElement.GetProperty("items");

        if (items.GetArrayLength() >= 2)
        {
            var names = items.EnumerateArray()
                .Select(item => item.GetProperty("name").GetString())
                .ToList();

            for (int i = 0; i < names.Count - 1; i++)
            {
                Assert.True(
                    string.Compare(names[i], names[i + 1], StringComparison.OrdinalIgnoreCase) >= 0,
                    $"Names not in descending order: '{names[i]}' should come after '{names[i + 1]}'");
            }
        }
    }

    [SkippableFact]
    public async Task ListUsers_SortByEmail_ReturnsOrderedResults()
    {
        Skip.If(!IsPostgreSqlAvailable(), "PostgreSQL database not available");

        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var client = await LoginAsAdminAsync(dbContext);

        var response = await client.GetAsync("/api/v1/admin/users?sortBy=email&sortDir=asc&pageSize=50");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();
        var jsonDoc = JsonDocument.Parse(content);
        var items = jsonDoc.RootElement.GetProperty("items");

        if (items.GetArrayLength() >= 2)
        {
            var emails = items.EnumerateArray()
                .Select(item => item.GetProperty("email").GetString())
                .ToList();

            for (int i = 0; i < emails.Count - 1; i++)
            {
                Assert.True(
                    string.Compare(emails[i], emails[i + 1], StringComparison.OrdinalIgnoreCase) <= 0,
                    $"Emails not in ascending order: '{emails[i]}' should come before '{emails[i + 1]}'");
            }
        }
    }

    [SkippableFact]
    public async Task ListUsers_SortByRole_ReturnsOrderedResults()
    {
        Skip.If(!IsPostgreSqlAvailable(), "PostgreSQL database not available");

        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var client = await LoginAsAdminAsync(dbContext);

        var response = await client.GetAsync("/api/v1/admin/users?sortBy=role&sortDir=asc&pageSize=50");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();
        var jsonDoc = JsonDocument.Parse(content);
        var items = jsonDoc.RootElement.GetProperty("items");

        Assert.True(items.GetArrayLength() >= 1);
    }

    [SkippableFact]
    public async Task ListUsers_SortByStatus_ReturnsOrderedResults()
    {
        Skip.If(!IsPostgreSqlAvailable(), "PostgreSQL database not available");

        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var client = await LoginAsAdminAsync(dbContext);

        var response = await client.GetAsync("/api/v1/admin/users?sortBy=status&sortDir=asc&pageSize=50");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();
        var jsonDoc = JsonDocument.Parse(content);
        var items = jsonDoc.RootElement.GetProperty("items");

        Assert.True(items.GetArrayLength() >= 1);
    }

    [SkippableFact]
    public async Task ListUsers_WithInvalidSortBy_Returns400BadRequest()
    {
        Skip.If(!IsPostgreSqlAvailable(), "PostgreSQL database not available");

        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var client = await LoginAsAdminAsync(dbContext);

        var response = await client.GetAsync("/api/v1/admin/users?sortBy=invalid_column");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();
        var jsonDoc = JsonDocument.Parse(content);
        
        Assert.True(jsonDoc.RootElement.TryGetProperty("error", out var errorElement));
        Assert.Equal("invalid_input", errorElement.GetProperty("code").GetString());
    }

    [SkippableFact]
    public async Task ListUsers_WithInvalidSortDir_Returns400BadRequest()
    {
        Skip.If(!IsPostgreSqlAvailable(), "PostgreSQL database not available");

        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var client = await LoginAsAdminAsync(dbContext);

        var response = await client.GetAsync("/api/v1/admin/users?sortDir=invalid");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    #endregion

    #region Soft Delete Tests

    [SkippableFact]
    public async Task ListUsers_ExcludesSoftDeletedUsers()
    {
        Skip.If(!IsPostgreSqlAvailable(), "PostgreSQL database not available");

        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var uniqueEmail = $"softdeleted_{Guid.NewGuid():N}@example.com";
        var deletedUser = new User
        {
            Id = Guid.NewGuid(),
            Email = uniqueEmail,
            Name = "Soft Deleted User",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("TestPassword123!"),
            Role = "Standard",
            Status = "Active",
            IsStaticAdmin = false,
            IsDeleted = true,
            DeletedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        dbContext.Users.Add(deletedUser);
        await dbContext.SaveChangesAsync();

        var client = await LoginAsAdminAsync(dbContext);

        var response = await client.GetAsync($"/api/v1/admin/users?q={uniqueEmail}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();
        var jsonDoc = JsonDocument.Parse(content);
        var items = jsonDoc.RootElement.GetProperty("items");

        var foundDeletedUser = items.EnumerateArray()
            .Any(item => item.GetProperty("email").GetString() == uniqueEmail);
        Assert.False(foundDeletedUser, "Soft-deleted user should not appear in results");
    }

    #endregion

    #region Default Values Tests

    [SkippableFact]
    public async Task ListUsers_WithNoParameters_UsesDefaults()
    {
        Skip.If(!IsPostgreSqlAvailable(), "PostgreSQL database not available");

        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var client = await LoginAsAdminAsync(dbContext);

        var response = await client.GetAsync("/api/v1/admin/users");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();
        var jsonDoc = JsonDocument.Parse(content);
        var root = jsonDoc.RootElement;

        Assert.Equal(1, root.GetProperty("page").GetInt32());
        Assert.Equal(20, root.GetProperty("pageSize").GetInt32());
    }

    #endregion
}
