using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Data.Sqlite;
using Npgsql;

namespace ClinicalIntelligence.Api.Configuration;

/// <summary>
/// Configuration options for application secrets and settings.
/// </summary>
public sealed class SecretsOptions
{
    /// <summary>
    /// Default minimum pool size for PostgreSQL connections.
    /// </summary>
    public const int DefaultMinPoolSize = 10;

    /// <summary>
    /// Default maximum pool size for PostgreSQL connections.
    /// </summary>
    public const int DefaultMaxPoolSize = 100;

    /// <summary>
    /// Default connection idle lifetime in seconds (5 minutes).
    /// </summary>
    public const int DefaultConnectionIdleLifetimeSeconds = 300;

    /// <summary>
    /// Default connection pruning interval in seconds.
    /// </summary>
    public const int DefaultConnectionPruningIntervalSeconds = 10;

    /// <summary>
    /// Default pool wait timeout in seconds for pool exhaustion.
    /// </summary>
    public const int DefaultPoolWaitTimeoutSeconds = 30;

    /// <summary>
    /// Gets the database connection string.
    /// </summary>
    public string? DatabaseConnectionString { get; init; }
    
    /// <summary>
    /// Gets the JWT signing key for token generation and validation.
    /// </summary>
    public string? JwtKey { get; init; }
    
    /// <summary>
    /// Gets the JWT token issuer.
    /// </summary>
    public string JwtIssuer { get; init; } = "ClinicalIntelligence";
    
    /// <summary>
    /// Gets the JWT token audience.
    /// </summary>
    public string JwtAudience { get; init; } = "ClinicalIntelligence.Users";
    
    /// <summary>
    /// Gets the JWT token expiration time in minutes.
    /// Default is 15 minutes per US_011 security requirements.
    /// </summary>
    public int JwtExpirationMinutes { get; init; } = 15;
    
    /// <summary>
    /// Gets the development database file name.
    /// </summary>
    public string DevelopmentDatabaseName { get; init; } = "clinicalintelligence.db";

    /// <summary>
    /// Creates a SecretsOptions instance from configuration.
    /// </summary>
    /// <param name="configuration">The configuration instance.</param>
    /// <returns>A configured SecretsOptions instance.</returns>
    public static SecretsOptions FromConfiguration(IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? configuration["DATABASE_CONNECTION_STRING"];

        return new SecretsOptions
        {
            DatabaseConnectionString = string.IsNullOrWhiteSpace(connectionString) ? null : connectionString,
            JwtKey = configuration["JWT_KEY"],
            DevelopmentDatabaseName = configuration["DEV_DATABASE_NAME"] ?? "clinicalintelligence.db"
        };
    }

    /// <summary>
    /// Resolves the database connection string based on environment and configuration.
    /// </summary>
    /// <param name="environment">The hosting environment.</param>
    /// <returns>A valid database connection string.</returns>
    /// <exception cref="InvalidOperationException">Thrown when configuration is invalid.</exception>
    public string ResolveDatabaseConnectionString(IHostEnvironment environment)
    {
        if (!string.IsNullOrWhiteSpace(DatabaseConnectionString))
        {
            try
            {
                // Try to validate as PostgreSQL connection string first
                _ = new NpgsqlConnectionStringBuilder(DatabaseConnectionString);
            }
            catch (Exception)
            {
                // If PostgreSQL fails, try SQLite
                try
                {
                    _ = new SqliteConnectionStringBuilder(DatabaseConnectionString);
                }
                catch (Exception)
                {
                    throw new InvalidOperationException(
                        "Invalid database connection string format. Provide a valid PostgreSQL or SQLite connection string via 'ConnectionStrings:DefaultConnection' or 'DATABASE_CONNECTION_STRING'."
                    );
                }
            }

            return DatabaseConnectionString;
        }

        if (environment.IsDevelopment())
        {
            return $"Data Source={DevelopmentDatabaseName}";
        }

        throw new InvalidOperationException(
            "Missing required configuration value for database connection string. Provide 'ConnectionStrings:DefaultConnection' or 'DATABASE_CONNECTION_STRING'."
        );
    }

    /// <summary>
    /// Resolves and normalizes the database connection string with pooling parameters for PostgreSQL.
    /// SQLite connection strings are returned unchanged.
    /// </summary>
    /// <param name="environment">The hosting environment.</param>
    /// <param name="poolWaitTimeoutSeconds">Optional pool wait timeout override (default: 30 seconds).</param>
    /// <returns>A normalized database connection string with pooling parameters for PostgreSQL, or unchanged for SQLite.</returns>
    public string ResolveNormalizedConnectionString(
        IHostEnvironment environment,
        int? poolWaitTimeoutSeconds = null)
    {
        var connectionString = ResolveDatabaseConnectionString(environment);
        return NormalizeConnectionString(connectionString, poolWaitTimeoutSeconds);
    }

    /// <summary>
    /// Normalizes a connection string by applying Npgsql pooling parameters for PostgreSQL.
    /// SQLite connection strings are returned unchanged.
    /// </summary>
    /// <param name="connectionString">The connection string to normalize.</param>
    /// <param name="poolWaitTimeoutSeconds">Optional pool wait timeout override (default: 30 seconds).</param>
    /// <returns>A normalized connection string with pooling parameters for PostgreSQL, or unchanged for SQLite.</returns>
    public static string NormalizeConnectionString(
        string connectionString,
        int? poolWaitTimeoutSeconds = null)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return connectionString;
        }

        // Check if it's a PostgreSQL connection string
        if (IsPostgreSqlConnectionString(connectionString))
        {
            return ApplyNpgsqlPoolingDefaults(connectionString, poolWaitTimeoutSeconds ?? DefaultPoolWaitTimeoutSeconds);
        }

        // SQLite or other connection strings are returned unchanged
        return connectionString;
    }

    /// <summary>
    /// Determines if a connection string is for PostgreSQL.
    /// </summary>
    public static bool IsPostgreSqlConnectionString(string connectionString)
    {
        try
        {
            _ = new NpgsqlConnectionStringBuilder(connectionString);
            // If it parses successfully and has a Host, it's PostgreSQL
            var builder = new NpgsqlConnectionStringBuilder(connectionString);
            return !string.IsNullOrEmpty(builder.Host);
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Applies Npgsql pooling defaults to a PostgreSQL connection string.
    /// </summary>
    private static string ApplyNpgsqlPoolingDefaults(string connectionString, int poolWaitTimeoutSeconds)
    {
        var builder = new NpgsqlConnectionStringBuilder(connectionString)
        {
            MinPoolSize = DefaultMinPoolSize,
            MaxPoolSize = DefaultMaxPoolSize,
            ConnectionIdleLifetime = DefaultConnectionIdleLifetimeSeconds,
            ConnectionPruningInterval = DefaultConnectionPruningIntervalSeconds,
            Timeout = poolWaitTimeoutSeconds
        };

        return builder.ConnectionString;
    }

    /// <summary>
    /// Validates the JWT configuration requirements.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when JWT configuration is invalid.</exception>
    public void ValidateJwtConfiguration()
    {
        if (string.IsNullOrWhiteSpace(JwtKey))
        {
            throw new InvalidOperationException(
                "Missing required configuration value 'JWT_KEY' for JWT token generation and validation."
            );
        }

        if (JwtKey.Length < 32)
        {
            throw new InvalidOperationException(
                "JWT_KEY must be at least 32 characters long for secure token signing."
            );
        }
    }
}
