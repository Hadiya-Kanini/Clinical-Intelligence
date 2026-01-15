using Npgsql;

namespace ClinicalIntelligence.Api.Diagnostics;

/// <summary>
/// Represents a snapshot of PostgreSQL connection pool metrics.
/// </summary>
public sealed record DbPoolMetricsSnapshot
{
    /// <summary>
    /// Gets the number of active (in-use) connections.
    /// </summary>
    public int ActiveConnections { get; init; }

    /// <summary>
    /// Gets the number of idle connections in the pool.
    /// </summary>
    public int IdleConnections { get; init; }

    /// <summary>
    /// Gets the total number of connections (active + idle).
    /// </summary>
    public int TotalConnections { get; init; }

    /// <summary>
    /// Gets the configured minimum pool size.
    /// </summary>
    public int MinPoolSize { get; init; }

    /// <summary>
    /// Gets the configured maximum pool size.
    /// </summary>
    public int MaxPoolSize { get; init; }

    /// <summary>
    /// Gets the timestamp when this snapshot was taken.
    /// </summary>
    public DateTimeOffset Timestamp { get; init; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Indicates whether pool metrics are available.
    /// </summary>
    public bool IsAvailable { get; init; }

    /// <summary>
    /// Gets an error message if metrics collection failed.
    /// </summary>
    public string? ErrorMessage { get; init; }
}

/// <summary>
/// Service for collecting PostgreSQL connection pool metrics.
/// Uses Npgsql's pool statistics via pg_stat_activity query.
/// </summary>
public sealed class DbPoolMetricsCollector
{
    private readonly string _connectionString;
    private readonly int _minPoolSize;
    private readonly int _maxPoolSize;

    /// <summary>
    /// Initializes a new instance of the <see cref="DbPoolMetricsCollector"/> class.
    /// </summary>
    /// <param name="connectionString">The PostgreSQL connection string.</param>
    public DbPoolMetricsCollector(string connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new ArgumentNullException(nameof(connectionString));
        }

        _connectionString = connectionString;

        var builder = new NpgsqlConnectionStringBuilder(connectionString);
        _minPoolSize = builder.MinPoolSize;
        _maxPoolSize = builder.MaxPoolSize;
    }

    /// <summary>
    /// Captures a snapshot of the current connection pool statistics.
    /// Queries PostgreSQL's pg_stat_activity to get connection counts.
    /// </summary>
    /// <returns>A snapshot of pool metrics.</returns>
    public async Task<DbPoolMetricsSnapshot> CaptureSnapshotAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);

            await using var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT 
                    COUNT(*) FILTER (WHERE state = 'active') as active_connections,
                    COUNT(*) FILTER (WHERE state = 'idle') as idle_connections,
                    COUNT(*) as total_connections
                FROM pg_stat_activity 
                WHERE datname = current_database() 
                  AND pid != pg_backend_pid()";
            command.CommandTimeout = 5;

            await using var reader = await command.ExecuteReaderAsync(cancellationToken);

            if (await reader.ReadAsync(cancellationToken))
            {
                var active = reader.GetInt64(0);
                var idle = reader.GetInt64(1);
                var total = reader.GetInt64(2);

                return new DbPoolMetricsSnapshot
                {
                    ActiveConnections = (int)active,
                    IdleConnections = (int)idle,
                    TotalConnections = (int)total,
                    MinPoolSize = _minPoolSize,
                    MaxPoolSize = _maxPoolSize,
                    IsAvailable = true,
                    Timestamp = DateTimeOffset.UtcNow
                };
            }

            return new DbPoolMetricsSnapshot
            {
                IsAvailable = false,
                ErrorMessage = "No statistics returned from database.",
                MinPoolSize = _minPoolSize,
                MaxPoolSize = _maxPoolSize
            };
        }
        catch (Exception ex)
        {
            return new DbPoolMetricsSnapshot
            {
                IsAvailable = false,
                ErrorMessage = $"Failed to collect pool metrics: {ex.GetType().Name}",
                MinPoolSize = _minPoolSize,
                MaxPoolSize = _maxPoolSize
            };
        }
    }

    /// <summary>
    /// Captures a snapshot of the current connection pool statistics synchronously.
    /// </summary>
    /// <returns>A snapshot of pool metrics.</returns>
    public DbPoolMetricsSnapshot CaptureSnapshot()
    {
        return CaptureSnapshotAsync().GetAwaiter().GetResult();
    }
}
