using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ClinicalIntelligence.Api.Data;
using ClinicalIntelligence.Api.Domain.Models;
using ClinicalIntelligence.Api.Services;
using ClinicalIntelligence.Api.Tests.Fakes;

namespace ClinicalIntelligence.Api.Tests;

/// <summary>
/// Test web application factory for integration tests.
/// Uses PostgreSQL with test database for full feature compatibility.
/// </summary>
public sealed class TestWebApplicationFactory<TProgram> : WebApplicationFactory<TProgram>
    where TProgram : class
{
    private readonly string _testDatabaseName = $"test_db_{Guid.NewGuid():N}";

    public TestWebApplicationFactory()
    {
        // Set environment variables for test configuration
        Environment.SetEnvironmentVariable("CORS_ALLOWED_ORIGINS", "http://localhost:3000");
        Environment.SetEnvironmentVariable("JWT_KEY", "TestSecretKeyForJwtTokenGeneration12345678901234567890");
        Environment.SetEnvironmentVariable("JWT_ISSUER", "TestIssuer");
        Environment.SetEnvironmentVariable("JWT_AUDIENCE", "TestAudience");
        Environment.SetEnvironmentVariable("JWT_EXPIRATION_MINUTES", "60");
        Environment.SetEnvironmentVariable("BCRYPT_WORK_FACTOR", "4");
        
        // Set test database connection string
        Environment.SetEnvironmentVariable("CONNECTION_STRING", 
            $"Host=localhost;Database={_testDatabaseName};Username=postgres;Password=test;Port=5432;");
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
                ["Bcrypt:WorkFactor"] = "4",
                ["RateLimiting:LoginPermitLimit"] = "100",
                ["RateLimiting:LoginWindowSeconds"] = "60",
                ["RateLimiting:ForgotPasswordPermitLimit"] = "1000",
                ["RateLimiting:ForgotPasswordWindowSeconds"] = "1",
                ["ConnectionStrings:DefaultConnection"] = $"Host=localhost;Database={_testDatabaseName};Username=postgres;Password=test;Port=5432;"
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
            services.AddSingleton<FakeEmailService>();
            services.AddSingleton<IEmailService>(sp => sp.GetRequiredService<FakeEmailService>());

            // Create the service provider
            var sp = services.BuildServiceProvider();

            // Create a scope to obtain a reference to the database context
            using var scope = sp.CreateScope();
            var scopedServices = scope.ServiceProvider;
            var db = scopedServices.GetRequiredService<ApplicationDbContext>();

            try
            {
                // Ensure the database is created and migrated
                db.Database.Migrate();
                
                // Seed test data
                SeedTestData(db);
            }
            catch (Exception ex)
            {
                // Log the error but don't fail the test setup
                // Tests will be skipped if database is not available
                Console.WriteLine($"Warning: Could not set up test database: {ex.Message}");
            }
        });

        // Use Testing environment
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
}
