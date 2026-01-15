using ClinicalIntelligence.Api.Configuration;
using Npgsql;
using Xunit;

namespace ClinicalIntelligence.Api.Tests;

public sealed class DatabaseConnectionPoolingConfigurationTests
{
    [Fact]
    public void NormalizeConnectionString_PostgresConnectionString_AppliesPoolingDefaults()
    {
        var originalConnectionString = "Host=localhost;Database=testdb;Username=user;Password=pass";

        var normalized = SecretsOptions.NormalizeConnectionString(originalConnectionString);

        var builder = new NpgsqlConnectionStringBuilder(normalized);
        Assert.Equal(SecretsOptions.DefaultMinPoolSize, builder.MinPoolSize);
        Assert.Equal(SecretsOptions.DefaultMaxPoolSize, builder.MaxPoolSize);
        Assert.Equal(SecretsOptions.DefaultConnectionIdleLifetimeSeconds, builder.ConnectionIdleLifetime);
        Assert.Equal(SecretsOptions.DefaultConnectionPruningIntervalSeconds, builder.ConnectionPruningInterval);
        Assert.Equal(SecretsOptions.DefaultPoolWaitTimeoutSeconds, builder.Timeout);
    }

    [Fact]
    public void NormalizeConnectionString_PostgresConnectionString_PreservesOriginalValues()
    {
        var originalConnectionString = "Host=myhost;Database=mydb;Username=myuser;Password=mypass;Port=5433";

        var normalized = SecretsOptions.NormalizeConnectionString(originalConnectionString);

        var builder = new NpgsqlConnectionStringBuilder(normalized);
        Assert.Equal("myhost", builder.Host);
        Assert.Equal("mydb", builder.Database);
        Assert.Equal("myuser", builder.Username);
        Assert.Equal("mypass", builder.Password);
        Assert.Equal(5433, builder.Port);
    }

    [Fact]
    public void NormalizeConnectionString_PostgresConnectionString_CustomPoolWaitTimeout()
    {
        var originalConnectionString = "Host=localhost;Database=testdb;Username=user;Password=pass";
        var customTimeout = 60;

        var normalized = SecretsOptions.NormalizeConnectionString(originalConnectionString, customTimeout);

        var builder = new NpgsqlConnectionStringBuilder(normalized);
        Assert.Equal(customTimeout, builder.Timeout);
    }

    [Fact]
    public void NormalizeConnectionString_SqliteConnectionString_ReturnsUnchanged()
    {
        var sqliteConnectionString = "Data Source=test.db";

        var normalized = SecretsOptions.NormalizeConnectionString(sqliteConnectionString);

        Assert.Equal(sqliteConnectionString, normalized);
    }

    [Fact]
    public void NormalizeConnectionString_SqliteInMemoryConnectionString_ReturnsUnchanged()
    {
        var sqliteConnectionString = "Data Source=:memory:";

        var normalized = SecretsOptions.NormalizeConnectionString(sqliteConnectionString);

        Assert.Equal(sqliteConnectionString, normalized);
    }

    [Fact]
    public void NormalizeConnectionString_NullConnectionString_ReturnsNull()
    {
        var normalized = SecretsOptions.NormalizeConnectionString(null!);

        Assert.Null(normalized);
    }

    [Fact]
    public void NormalizeConnectionString_EmptyConnectionString_ReturnsEmpty()
    {
        var normalized = SecretsOptions.NormalizeConnectionString(string.Empty);

        Assert.Equal(string.Empty, normalized);
    }

    [Fact]
    public void NormalizeConnectionString_WhitespaceConnectionString_ReturnsWhitespace()
    {
        var normalized = SecretsOptions.NormalizeConnectionString("   ");

        Assert.Equal("   ", normalized);
    }

    [Fact]
    public void IsPostgreSqlConnectionString_ValidPostgresConnectionString_ReturnsTrue()
    {
        var connectionString = "Host=localhost;Database=testdb;Username=user;Password=pass";

        var result = SecretsOptions.IsPostgreSqlConnectionString(connectionString);

        Assert.True(result);
    }

    [Fact]
    public void IsPostgreSqlConnectionString_SqliteConnectionString_ReturnsFalse()
    {
        var connectionString = "Data Source=test.db";

        var result = SecretsOptions.IsPostgreSqlConnectionString(connectionString);

        Assert.False(result);
    }

    [Fact]
    public void IsPostgreSqlConnectionString_EmptyConnectionString_ReturnsFalse()
    {
        var result = SecretsOptions.IsPostgreSqlConnectionString(string.Empty);

        Assert.False(result);
    }

    [Theory]
    [InlineData(10, "Minimum Pool Size")]
    [InlineData(100, "Maximum Pool Size")]
    public void PoolingDefaults_MatchAcceptanceCriteria(int expectedValue, string parameterName)
    {
        var actualValue = parameterName switch
        {
            "Minimum Pool Size" => SecretsOptions.DefaultMinPoolSize,
            "Maximum Pool Size" => SecretsOptions.DefaultMaxPoolSize,
            _ => throw new ArgumentException($"Unknown parameter: {parameterName}")
        };

        Assert.Equal(expectedValue, actualValue);
    }

    [Fact]
    public void PoolingDefaults_ConnectionIdleLifetime_Is300Seconds()
    {
        Assert.Equal(300, SecretsOptions.DefaultConnectionIdleLifetimeSeconds);
    }

    [Fact]
    public void PoolingDefaults_ConnectionPruningInterval_Is10Seconds()
    {
        Assert.Equal(10, SecretsOptions.DefaultConnectionPruningIntervalSeconds);
    }

    [Fact]
    public void PoolingDefaults_PoolWaitTimeout_Is30Seconds()
    {
        Assert.Equal(30, SecretsOptions.DefaultPoolWaitTimeoutSeconds);
    }

    [Fact]
    public void NormalizeConnectionString_PostgresWithExistingPoolSettings_OverridesWithDefaults()
    {
        var originalConnectionString = "Host=localhost;Database=testdb;Username=user;Password=pass;Minimum Pool Size=5;Maximum Pool Size=50";

        var normalized = SecretsOptions.NormalizeConnectionString(originalConnectionString);

        var builder = new NpgsqlConnectionStringBuilder(normalized);
        Assert.Equal(SecretsOptions.DefaultMinPoolSize, builder.MinPoolSize);
        Assert.Equal(SecretsOptions.DefaultMaxPoolSize, builder.MaxPoolSize);
    }
}
