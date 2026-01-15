using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ClinicalIntelligence.Api.Data;
using ClinicalIntelligence.Api.Domain.Models;

namespace ClinicalIntelligence.Api.Tests;

/// <summary>
/// Test web application factory for integration tests.
/// </summary>
public sealed class TestWebApplicationFactory<TProgram> : WebApplicationFactory<TProgram>
    where TProgram : class
{
    private readonly string _testDatabaseName = $"test_db_{Guid.NewGuid():N}.db";
    private readonly string _testDatabasePath;

    public TestWebApplicationFactory()
    {
        _testDatabasePath = Path.Combine(Path.GetTempPath(), _testDatabaseName);
        
        // Set environment variables for test configuration
        Environment.SetEnvironmentVariable("CORS_ALLOWED_ORIGINS", "http://localhost:3000");
        Environment.SetEnvironmentVariable("JWT_KEY", "TestSecretKeyForJwtTokenGeneration12345678901234567890");
        Environment.SetEnvironmentVariable("JWT_ISSUER", "TestIssuer");
        Environment.SetEnvironmentVariable("JWT_AUDIENCE", "TestAudience");
        Environment.SetEnvironmentVariable("JWT_EXPIRATION_MINUTES", "60");
        Environment.SetEnvironmentVariable("BCRYPT_WORK_FACTOR", "4");
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((context, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Cors:AllowedOrigins"] = "http://localhost:3000",
                ["Jwt:Key"] = "TestSecretKeyForJwtTokenGeneration12345678901234567890",
                ["Jwt:Issuer"] = "TestIssuer",
                ["Jwt:Audience"] = "TestAudience",
                ["Jwt:ExpirationMinutes"] = "60",
                ["Bcrypt:WorkFactor"] = "4"
            });
        });

        builder.ConfigureServices(services =>
        {
            // Remove the existing DbContext
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));
            if (descriptor != null)
            {
                services.Remove(descriptor);
            }

            // Add test database
            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseSqlite($"Data Source={_testDatabasePath}");
            });

            // Create the service provider
            var sp = services.BuildServiceProvider();

            // Create a scope to obtain a reference to the database context
            using var scope = sp.CreateScope();
            var scopedServices = scope.ServiceProvider;
            var db = scopedServices.GetRequiredService<ApplicationDbContext>();

            // Ensure the database is created
            db.Database.EnsureCreated();

            // Seed test data
            SeedTestData(db);
        });

        // Use Testing environment to skip automatic migrations in Program.cs
        // (Development environment triggers db.Database.Migrate() which fails with SQLite)
        builder.UseEnvironment("Testing");
    }

    private static void SeedTestData(ApplicationDbContext dbContext)
    {
        // Check if test user already exists
        var existingUser = dbContext.Users.FirstOrDefault(u => u.Email == "test@example.com");
        if (existingUser == null)
        {
            // Create test user
            var testUser = new User
            {
                Id = Guid.NewGuid(),
                Email = "test@example.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("TestPassword123!"),
                Name = "Test User",
                Role = "Standard",
                Status = "Active",
                IsStaticAdmin = false,
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            dbContext.Users.Add(testUser);
        }

        // Create admin user if not exists
        var existingAdmin = dbContext.Users.FirstOrDefault(u => u.Email == "admin@example.com");
        if (existingAdmin == null)
        {
            var adminUser = new User
            {
                Id = Guid.NewGuid(),
                Email = "admin@example.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("AdminPassword123!"),
                Name = "Admin User",
                Role = "Admin",
                Status = "Active",
                IsStaticAdmin = false,
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            dbContext.Users.Add(adminUser);
        }

        dbContext.SaveChanges();
    }

    protected override void Dispose(bool disposing)
    {
        // Clean up test database
        if (File.Exists(_testDatabasePath))
        {
            try
            {
                File.Delete(_testDatabasePath);
            }
            catch
            {
                // Ignore cleanup errors
            }
        }

        base.Dispose(disposing);
    }
}
