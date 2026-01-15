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
    /// Gets the bcrypt work factor for password hashing.
    /// Default is 12 per OWASP Password Storage Cheat Sheet recommendations.
    /// </summary>
    public int BcryptWorkFactor { get; init; } = 12;

    #region SMTP Configuration

    /// <summary>
    /// SMTP server hostname.
    /// </summary>
    public string? SmtpHost { get; init; }

    /// <summary>
    /// SMTP server port (default: 587 for TLS).
    /// </summary>
    public int SmtpPort { get; init; } = 587;

    /// <summary>
    /// SMTP authentication username.
    /// </summary>
    public string? SmtpUsername { get; init; }

    /// <summary>
    /// SMTP authentication password.
    /// </summary>
    public string? SmtpPassword { get; init; }

    /// <summary>
    /// Email address to send from.
    /// </summary>
    public string? SmtpFromEmail { get; init; }

    /// <summary>
    /// Display name for the sender.
    /// </summary>
    public string SmtpFromName { get; init; } = "Clinical Intelligence";

    /// <summary>
    /// Enable SSL/TLS for SMTP connection.
    /// </summary>
    public bool SmtpEnableSsl { get; init; } = true;

    /// <summary>
    /// Frontend URL for constructing password reset links.
    /// </summary>
    public string FrontendUrl { get; init; } = "http://localhost:5173";

    #endregion

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
            DevelopmentDatabaseName = configuration["DEV_DATABASE_NAME"] ?? "clinicalintelligence.db",
            // SMTP Configuration
            SmtpHost = configuration["SMTP_HOST"],
            SmtpPort = int.TryParse(configuration["SMTP_PORT"], out var port) ? port : 587,
            SmtpUsername = configuration["SMTP_USERNAME"],
            SmtpPassword = configuration["SMTP_PASSWORD"],
            SmtpFromEmail = configuration["SMTP_FROM_EMAIL"],
            SmtpFromName = configuration["SMTP_FROM_NAME"] ?? "Clinical Intelligence",
            SmtpEnableSsl = !string.Equals(configuration["SMTP_ENABLE_SSL"], "false", StringComparison.OrdinalIgnoreCase),
            FrontendUrl = configuration["FRONTEND_URL"] ?? "http://localhost:5173",
            BcryptWorkFactor = int.TryParse(configuration["BCRYPT_WORK_FACTOR"], out var bcryptWorkFactor) ? bcryptWorkFactor : 12
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

    /// <summary>
    /// Validates the bcrypt work factor configuration.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when bcrypt work factor is below minimum (12) or above maximum (31).</exception>
    public void ValidateBcryptConfiguration()
    {
        const int MinimumWorkFactor = 12;
        const int MaximumWorkFactor = 31;

        if (BcryptWorkFactor < MinimumWorkFactor)
        {
            throw new InvalidOperationException(
                $"BCRYPT_WORK_FACTOR must be at least {MinimumWorkFactor} for secure password hashing. Current value: {BcryptWorkFactor}");
        }

        if (BcryptWorkFactor > MaximumWorkFactor)
        {
            throw new InvalidOperationException(
                $"BCRYPT_WORK_FACTOR must not exceed {MaximumWorkFactor}. Current value: {BcryptWorkFactor}");
        }
    }

    /// <summary>
    /// Validates SMTP configuration for email service.
    /// </summary>
    /// <returns>True if SMTP is configured, false if not configured (email disabled).</returns>
    /// <exception cref="InvalidOperationException">Thrown when SMTP is partially configured.</exception>
    public bool ValidateSmtpConfiguration()
    {
        var hasHost = !string.IsNullOrWhiteSpace(SmtpHost);
        var hasFromEmail = !string.IsNullOrWhiteSpace(SmtpFromEmail);

        // If neither is set, SMTP is disabled (valid state)
        if (!hasHost && !hasFromEmail)
        {
            return false;
        }

        // If partially configured, throw error
        if (!hasHost)
        {
            throw new InvalidOperationException(
                "Missing required SMTP configuration: SMTP_HOST must be provided when SMTP_FROM_EMAIL is set."
            );
        }

        if (!hasFromEmail)
        {
            throw new InvalidOperationException(
                "Missing required SMTP configuration: SMTP_FROM_EMAIL must be provided when SMTP_HOST is set."
            );
        }

        return true;
    }
}
