using System;
using ClinicalIntelligence.Api.Data;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace ClinicalIntelligence.Api.Tests;

/// <summary>
/// Fixture for creating test databases with pgvector support.
/// Uses a real PostgreSQL instance instead of in-memory database.
/// </summary>
public class TestDatabaseFixture : IDisposable
{
    private const string TestConnectionString = "Host=localhost;Database=clinical_intelligence_test;Username=postgres;Password=postgres";
    private readonly string _databaseName;
    
    public ApplicationDbContext DbContext { get; private set; }

    public TestDatabaseFixture()
    {
        _databaseName = $"test_db_{Guid.NewGuid():N}";
        
        // Create test database
        CreateTestDatabase();
        
        // Create DbContext
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseNpgsql(GetConnectionString())
            .Options;
        
        DbContext = new ApplicationDbContext(options);
        
        // Ensure database is created and migrations applied
        DbContext.Database.EnsureCreated();
    }

    private void CreateTestDatabase()
    {
        using var connection = new NpgsqlConnection(GetMasterConnectionString());
        connection.Open();
        
        using var command = connection.CreateCommand();
        command.CommandText = $"CREATE DATABASE {_databaseName}";
        command.ExecuteNonQuery();
    }

    private string GetMasterConnectionString()
    {
        return "Host=localhost;Database=postgres;Username=postgres;Password=postgres";
    }

    private string GetConnectionString()
    {
        return $"Host=localhost;Database={_databaseName};Username=postgres;Password=postgres";
    }

    public void Dispose()
    {
        DbContext?.Dispose();
        
        // Drop test database
        try
        {
            using var connection = new NpgsqlConnection(GetMasterConnectionString());
            connection.Open();
            
            // Terminate connections to the test database
            using var terminateCmd = connection.CreateCommand();
            terminateCmd.CommandText = $@"
                SELECT pg_terminate_backend(pg_stat_activity.pid)
                FROM pg_stat_activity
                WHERE pg_stat_activity.datname = '{_databaseName}'
                AND pid <> pg_backend_pid()";
            terminateCmd.ExecuteNonQuery();
            
            // Drop database
            using var dropCmd = connection.CreateCommand();
            dropCmd.CommandText = $"DROP DATABASE IF EXISTS {_databaseName}";
            dropCmd.ExecuteNonQuery();
        }
        catch
        {
            // Ignore cleanup errors
        }
    }
}
