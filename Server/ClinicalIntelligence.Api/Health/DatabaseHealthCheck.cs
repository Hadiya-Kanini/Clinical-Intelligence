using System.Diagnostics;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Npgsql;

namespace ClinicalIntelligence.Api.Health;

public sealed class DatabaseHealthCheck : IHealthCheck
{
    private readonly string _connectionString;
    private readonly TimeSpan _latencyThreshold;

    private const int DefaultLatencyThresholdMs = 100;

    public DatabaseHealthCheck(string connectionString, int latencyThresholdMs = DefaultLatencyThresholdMs)
    {
        _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
        _latencyThreshold = TimeSpan.FromMilliseconds(latencyThresholdMs);
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            await using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);

            await using var command = connection.CreateCommand();
            command.CommandText = "SELECT 1;";
            command.CommandTimeout = 5;

            await command.ExecuteScalarAsync(cancellationToken);

            stopwatch.Stop();
            var latencyMs = stopwatch.Elapsed.TotalMilliseconds;

            var data = new Dictionary<string, object>
            {
                { "latency_ms", Math.Round(latencyMs, 2) },
                { "threshold_ms", _latencyThreshold.TotalMilliseconds }
            };

            if (stopwatch.Elapsed > _latencyThreshold)
            {
                return HealthCheckResult.Degraded(
                    description: $"Database responded but latency ({latencyMs:F2}ms) exceeded threshold ({_latencyThreshold.TotalMilliseconds}ms)",
                    data: data);
            }

            return HealthCheckResult.Healthy(
                description: $"Database is responsive (latency: {latencyMs:F2}ms)",
                data: data);
        }
        catch (NpgsqlException ex)
        {
            stopwatch.Stop();

            return HealthCheckResult.Unhealthy(
                description: "Database connection failed",
                exception: null,
                data: new Dictionary<string, object>
                {
                    { "error_type", ex.GetType().Name },
                    { "latency_ms", Math.Round(stopwatch.Elapsed.TotalMilliseconds, 2) }
                });
        }
        catch (OperationCanceledException)
        {
            return HealthCheckResult.Unhealthy(
                description: "Database health check was cancelled or timed out",
                data: new Dictionary<string, object>
                {
                    { "latency_ms", Math.Round(stopwatch.Elapsed.TotalMilliseconds, 2) }
                });
        }
        catch (Exception ex)
        {
            stopwatch.Stop();

            return HealthCheckResult.Unhealthy(
                description: "Unexpected error during database health check",
                exception: null,
                data: new Dictionary<string, object>
                {
                    { "error_type", ex.GetType().Name },
                    { "latency_ms", Math.Round(stopwatch.Elapsed.TotalMilliseconds, 2) }
                });
        }
    }
}

public static class DatabaseHealthCheckExtensions
{
    public static IHealthChecksBuilder AddDatabaseHealthCheck(
        this IHealthChecksBuilder builder,
        string connectionString,
        int latencyThresholdMs = 100,
        string name = "database",
        HealthStatus? failureStatus = null,
        IEnumerable<string>? tags = null)
    {
        return builder.Add(new HealthCheckRegistration(
            name,
            _ => new DatabaseHealthCheck(connectionString, latencyThresholdMs),
            failureStatus,
            tags));
    }
}
