using System;
using System.Collections.Generic;
using ClinicalIntelligence.Api.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Xunit;

namespace ClinicalIntelligence.Api.Tests;

public sealed class SecretsOptionsTests
{
    [Fact]
    public void FromConfiguration_ConnectionStringPresent_BindsValue()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = "Data Source=test.db"
            })
            .Build();

        var options = SecretsOptions.FromConfiguration(configuration);

        Assert.Equal("Data Source=test.db", options.DatabaseConnectionString);
    }

    [Fact]
    public void FromConfiguration_DatabaseConnectionStringEnvVarPresent_BindsValue()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["DATABASE_CONNECTION_STRING"] = "Data Source=test.db"
            })
            .Build();

        var options = SecretsOptions.FromConfiguration(configuration);

        Assert.Equal("Data Source=test.db", options.DatabaseConnectionString);
    }

    [Fact]
    public void FromConfiguration_WhitespaceValue_TreatedAsNull()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["DATABASE_CONNECTION_STRING"] = "   "
            })
            .Build();

        var options = SecretsOptions.FromConfiguration(configuration);

        Assert.Null(options.DatabaseConnectionString);
    }

    [Fact]
    public void ResolveDatabaseConnectionString_ValuePresent_ReturnsValue()
    {
        var options = new SecretsOptions
        {
            DatabaseConnectionString = "Data Source=test.db"
        };

        var env = new TestHostEnvironment { EnvironmentName = Environments.Production };

        var resolved = options.ResolveDatabaseConnectionString(env);

        Assert.Equal("Data Source=test.db", resolved);
    }

    [Fact]
    public void ResolveDatabaseConnectionString_DevelopmentWithoutValue_DefaultsToLocalSqlite()
    {
        var options = new SecretsOptions
        {
            DatabaseConnectionString = null
        };

        var env = new TestHostEnvironment { EnvironmentName = Environments.Development };

        var resolved = options.ResolveDatabaseConnectionString(env);

        Assert.Equal("Data Source=clinicalintelligence.db", resolved);
    }

    [Fact]
    public void ResolveDatabaseConnectionString_ProductionWithoutValue_ThrowsWithNonSensitiveMessage()
    {
        var options = new SecretsOptions
        {
            DatabaseConnectionString = null
        };

        var env = new TestHostEnvironment { EnvironmentName = Environments.Production };

        var ex = Assert.Throws<InvalidOperationException>(() => options.ResolveDatabaseConnectionString(env));

        Assert.Contains("Missing required configuration value for database connection string", ex.Message);
        Assert.DoesNotContain("Data Source=", ex.Message);
    }

    [Fact]
    public void ResolveDatabaseConnectionString_ProductionWithMalformedValue_ThrowsWithNonSensitiveMessage()
    {
        var options = new SecretsOptions
        {
            DatabaseConnectionString = "Mode=invalid"
        };

        var env = new TestHostEnvironment { EnvironmentName = Environments.Production };

        var ex = Assert.Throws<InvalidOperationException>(() => options.ResolveDatabaseConnectionString(env));

        Assert.Contains("Invalid database connection string format", ex.Message);
        Assert.DoesNotContain("Mode=invalid", ex.Message);
    }

    [Fact]
    public void FromConfiguration_JwtKeyPresent_BindsValue()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["JWT_KEY"] = "test_jwt_key_at_least_32_characters_long"
            })
            .Build();

        var options = SecretsOptions.FromConfiguration(configuration);

        Assert.Equal("test_jwt_key_at_least_32_characters_long", options.JwtKey);
    }

    [Fact]
    public void ValidateJwtConfiguration_MissingJwtKey_ThrowsInvalidOperationException()
    {
        var options = new SecretsOptions
        {
            JwtKey = null
        };

        var ex = Assert.Throws<InvalidOperationException>(() => options.ValidateJwtConfiguration());

        Assert.Contains("Missing required configuration value 'JWT_KEY'", ex.Message);
    }

    [Fact]
    public void ValidateJwtConfiguration_ShortJwtKey_ThrowsInvalidOperationException()
    {
        var options = new SecretsOptions
        {
            JwtKey = "short"
        };

        var ex = Assert.Throws<InvalidOperationException>(() => options.ValidateJwtConfiguration());

        Assert.Contains("JWT_KEY must be at least 32 characters long", ex.Message);
    }

    [Fact]
    public void ValidateJwtConfiguration_ValidJwtKey_DoesNotThrow()
    {
        var options = new SecretsOptions
        {
            JwtKey = "this_is_a_valid_jwt_key_that_is_long_enough"
        };

        var exception = Record.Exception(() => options.ValidateJwtConfiguration());

        Assert.Null(exception);
    }

    [Fact]
    public void FromConfiguration_DefaultValues_AppliedCorrectly()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["JWT_KEY"] = "test_jwt_key_at_least_32_characters_long"
            })
            .Build();

        var options = SecretsOptions.FromConfiguration(configuration);

        Assert.Equal("ClinicalIntelligence", options.JwtIssuer);
        Assert.Equal("ClinicalIntelligence.Users", options.JwtAudience);
        Assert.Equal(60, options.JwtExpirationMinutes);
    }

    private sealed class TestHostEnvironment : IHostEnvironment
    {
        public string EnvironmentName { get; set; } = string.Empty;

        public string ApplicationName { get; set; } = string.Empty;

        public string ContentRootPath { get; set; } = string.Empty;

        public IFileProvider ContentRootFileProvider { get; set; } = null!;
    }
}
