using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace ClinicalIntelligence.Api.Data;

/// <summary>
/// Hosted service that pre-establishes and validates the minimum pool size at startup.
/// Opens and validates connections to warm up the connection pool before the application starts accepting requests.
/// </summary>
public sealed class DatabaseWarmupHostedService : IHostedService
{
    private readonly string _connectionString;
    private readonly int _minPoolSize;
    private readonly ILogger<DatabaseWarmupHostedService> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="DatabaseWarmupHostedService"/> class.
    /// </summary>
    /// <param name="connectionString">The normalized PostgreSQL connection string.</param>
    /// <param name="minPoolSize">The minimum pool size to warm up.</param>
    /// <param name="logger">The logger instance.</param>
    public DatabaseWarmupHostedService(
        string connectionString,
        int minPoolSize,
        ILogger<DatabaseWarmupHostedService> logger)
    {
        _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
        _minPoolSize = minPoolSize;
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Warms up the connection pool by pre-establishing and validating the minimum number of connections.
    /// </summary>
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting database connection pool warm-up with {MinPoolSize} connections", _minPoolSize);

        var connections = new List<NpgsqlConnection>(_minPoolSize);

        try
        {
            for (int i = 0; i < _minPoolSize; i++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync(cancellationToken);

                await using var command = connection.CreateCommand();
                command.CommandText = "SELECT 1;";
                command.CommandTimeout = 5;
                await command.ExecuteScalarAsync(cancellationToken);

                connections.Add(connection);
            }

            _logger.LogInformation(
                "Database connection pool warm-up completed successfully. Established and validated {Count} connections",
                connections.Count);
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Database connection pool warm-up was cancelled");
            throw;
        }
        catch (NpgsqlException ex)
        {
            _logger.LogError(
                "Database connection pool warm-up failed: {ErrorType}. Application startup aborted",
                ex.GetType().Name);
            throw new InvalidOperationException(
                "Database connection pool warm-up failed. Ensure the database is accessible and properly configured.",
                ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                "Unexpected error during database connection pool warm-up: {ErrorType}. Application startup aborted",
                ex.GetType().Name);
            throw new InvalidOperationException(
                "Database connection pool warm-up failed due to an unexpected error.",
                ex);
        }
        finally
        {
            foreach (var connection in connections)
            {
                await connection.DisposeAsync();
            }
        }
    }

    /// <summary>
    /// No-op for shutdown as connections are returned to the pool.
    /// </summary>
    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Database connection pool warm-up service stopping");
        return Task.CompletedTask;
    }
}
