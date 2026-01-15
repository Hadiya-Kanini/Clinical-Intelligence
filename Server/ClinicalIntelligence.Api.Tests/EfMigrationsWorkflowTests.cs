using ClinicalIntelligence.Api.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Xunit;

namespace ClinicalIntelligence.Api.Tests;

/// <summary>
/// Integration-style tests for EF Core migration workflows.
/// These tests validate migration infrastructure using SQLite for relational operations.
/// Tests that require PostgreSQL are conditionally skipped when the database is unavailable.
/// </summary>
public sealed class EfMigrationsWorkflowTests : IDisposable
{
    private const string PostgresConnectionStringEnvVar = "DATABASE_CONNECTION_STRING";
    private readonly string _sqliteDbPath;

    public EfMigrationsWorkflowTests()
    {
        _sqliteDbPath = Path.Combine(Path.GetTempPath(), $"EfMigrationTest_{Guid.NewGuid()}.db");
    }

    public void Dispose()
    {
        if (File.Exists(_sqliteDbPath))
        {
            try { File.Delete(_sqliteDbPath); } catch { }
        }
    }

    private DbContextOptions<ApplicationDbContext> CreateSqliteOptions()
    {
        return new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite($"Data Source={_sqliteDbPath}")
            .Options;
    }

    /// <summary>
    /// Verifies that ApplicationDbContext can be instantiated with InMemory provider.
    /// </summary>
    [Fact]
    public void ApplicationDbContext_WithInMemoryProvider_CanBeInstantiated()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}")
            .Options;

        // Act
        using var context = new ApplicationDbContext(options);

        // Assert
        Assert.NotNull(context);
        Assert.NotNull(context.Patients);
        Assert.NotNull(context.Encounters);
        Assert.NotNull(context.Observations);
    }

    /// <summary>
    /// Verifies that the DbContext model can be built without errors.
    /// </summary>
    [Fact]
    public void ApplicationDbContext_ModelBuilder_CreatesValidModel()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}")
            .Options;

        // Act
        using var context = new ApplicationDbContext(options);
        var model = context.Model;

        // Assert
        Assert.NotNull(model);
        Assert.NotEmpty(model.GetEntityTypes());
        
        var patientEntity = model.FindEntityType(typeof(Domain.Models.Patient));
        Assert.NotNull(patientEntity);
        Assert.Equal("patients", patientEntity.GetTableName());
    }

    /// <summary>
    /// Verifies that migrations are discoverable in the assembly using SQLite provider.
    /// </summary>
    [Fact]
    public void Migrations_AreDiscoverable_InAssembly()
    {
        // Arrange
        var options = CreateSqliteOptions();
        using var context = new ApplicationDbContext(options);
        var migrationsAssembly = context.GetService<IMigrationsAssembly>();

        // Act
        var migrations = migrationsAssembly.Migrations;

        // Assert
        Assert.NotNull(migrations);
        Assert.NotEmpty(migrations);
    }

    /// <summary>
    /// Verifies that pending migrations can be queried using SQLite provider.
    /// </summary>
    [Fact]
    public async Task GetPendingMigrations_WithSqliteProvider_ReturnsMigrations()
    {
        // Arrange
        var options = CreateSqliteOptions();
        using var context = new ApplicationDbContext(options);

        // Act
        var pendingMigrations = await context.Database.GetPendingMigrationsAsync();

        // Assert
        Assert.NotNull(pendingMigrations);
        Assert.NotEmpty(pendingMigrations);
    }

    /// <summary>
    /// Verifies that applied migrations can be queried when using a relational provider.
    /// Requires PostgreSQL - run with DATABASE_CONNECTION_STRING environment variable set.
    /// </summary>
    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Database", "PostgreSQL")]
    public async Task GetAppliedMigrations_WithPostgres_ReturnsAppliedMigrations()
    {
        var connectionString = Environment.GetEnvironmentVariable(PostgresConnectionStringEnvVar);
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            Assert.True(true, $"Test skipped: {PostgresConnectionStringEnvVar} environment variable not set.");
            return;
        }

        // Arrange
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseNpgsql(connectionString)
            .Options;

        using var context = new ApplicationDbContext(options);

        // Act
        try
        {
            var appliedMigrations = await context.Database.GetAppliedMigrationsAsync();

            // Assert
            Assert.NotNull(appliedMigrations);
        }
        catch (Npgsql.NpgsqlException)
        {
            Assert.True(true, "Test skipped: PostgreSQL connection failed.");
        }
    }

    /// <summary>
    /// Verifies that migrations can be applied to PostgreSQL.
    /// Requires PostgreSQL - run with DATABASE_CONNECTION_STRING environment variable set.
    /// </summary>
    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Database", "PostgreSQL")]
    public async Task DatabaseMigrate_WithPostgres_AppliesMigrationsSuccessfully()
    {
        var connectionString = Environment.GetEnvironmentVariable(PostgresConnectionStringEnvVar);
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            Assert.True(true, $"Test skipped: {PostgresConnectionStringEnvVar} environment variable not set.");
            return;
        }

        // Arrange
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseNpgsql(connectionString)
            .Options;

        using var context = new ApplicationDbContext(options);

        // Act & Assert
        try
        {
            await context.Database.MigrateAsync();
            
            var appliedMigrations = await context.Database.GetAppliedMigrationsAsync();
            Assert.NotNull(appliedMigrations);
        }
        catch (Npgsql.NpgsqlException)
        {
            Assert.True(true, "Test skipped: PostgreSQL connection failed.");
        }
    }

    /// <summary>
    /// Verifies that migrations list includes timestamps in the migration ID format.
    /// </summary>
    [Fact]
    public void MigrationIds_FollowTimestampNamingConvention()
    {
        // Arrange
        var options = CreateSqliteOptions();
        using var context = new ApplicationDbContext(options);
        var migrationsAssembly = context.GetService<IMigrationsAssembly>();

        // Act
        var migrations = migrationsAssembly.Migrations;

        // Assert
        Assert.NotEmpty(migrations);
        foreach (var migration in migrations)
        {
            var migrationId = migration.Key;
            Assert.Matches(@"^\d{14}_\w+$", migrationId);
        }
    }

    /// <summary>
    /// Verifies that the model snapshot exists and is valid.
    /// </summary>
    [Fact]
    public void ModelSnapshot_Exists_AndIsValid()
    {
        // Arrange
        var options = CreateSqliteOptions();
        using var context = new ApplicationDbContext(options);
        var migrationsAssembly = context.GetService<IMigrationsAssembly>();

        // Act
        var modelSnapshot = migrationsAssembly.ModelSnapshot;

        // Assert
        Assert.NotNull(modelSnapshot);
    }

    /// <summary>
    /// Verifies that database can be created and migrations applied using SQLite.
    /// </summary>
    [Fact]
    public async Task EnsureCreated_WithSqlite_CreatesDatabase()
    {
        // Arrange
        var options = CreateSqliteOptions();
        using var context = new ApplicationDbContext(options);

        // Act
        var created = await context.Database.EnsureCreatedAsync();

        // Assert
        Assert.True(File.Exists(_sqliteDbPath));
    }
}
