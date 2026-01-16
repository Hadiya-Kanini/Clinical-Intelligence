using ClinicalIntelligence.Api.Data;
using ClinicalIntelligence.Api.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;

namespace ClinicalIntelligence.Api.Health;

/// <summary>
/// Health check for DLQ depth monitoring (NFR-011).
/// Reports Healthy/Degraded/Unhealthy based on configurable thresholds.
/// </summary>
public class DeadLetterQueueHealthCheck : IHealthCheck
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly DlqHealthCheckOptions _options;
    private readonly ILogger<DeadLetterQueueHealthCheck> _logger;

    public DeadLetterQueueHealthCheck(
        IServiceScopeFactory scopeFactory,
        IOptions<DlqHealthCheckOptions> options,
        ILogger<DeadLetterQueueHealthCheck> logger)
    {
        _scopeFactory = scopeFactory;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            // Use efficient indexed count queries
            var pendingCount = await dbContext.DeadLetterJobs
                .Where(d => d.Status == DeadLetterJobStatus.Pending)
                .CountAsync(cancellationToken);

            // Get oldest pending entry timestamp
            var oldestPendingAt = await dbContext.DeadLetterJobs
                .Where(d => d.Status == DeadLetterJobStatus.Pending)
                .OrderBy(d => d.DeadLetteredAt)
                .Select(d => (DateTime?)d.DeadLetteredAt)
                .FirstOrDefaultAsync(cancellationToken);

            long? oldestAgeSeconds = null;
            if (oldestPendingAt.HasValue)
            {
                oldestAgeSeconds = (long)(DateTime.UtcNow - oldestPendingAt.Value).TotalSeconds;
            }

            var data = new Dictionary<string, object>
            {
                ["pending_count"] = pendingCount,
                ["warning_threshold"] = _options.WarningThresholdCount,
                ["critical_threshold"] = _options.CriticalThresholdCount,
                ["oldest_pending_age_seconds"] = oldestAgeSeconds ?? 0,
                ["age_warning_seconds"] = _options.AgeWarningSeconds,
                ["age_critical_seconds"] = _options.AgeCriticalSeconds
            };

            if (oldestPendingAt.HasValue)
            {
                data["oldest_pending_at"] = oldestPendingAt.Value.ToString("O");
            }

            // Determine health status
            if (pendingCount >= _options.CriticalThresholdCount ||
                (oldestAgeSeconds.HasValue && oldestAgeSeconds.Value >= _options.AgeCriticalSeconds))
            {
                return HealthCheckResult.Unhealthy(
                    $"DLQ critical: {pendingCount} pending entries" +
                    (oldestAgeSeconds.HasValue ? $", oldest is {oldestAgeSeconds.Value}s old" : ""),
                    data: data);
            }

            if (pendingCount >= _options.WarningThresholdCount ||
                (oldestAgeSeconds.HasValue && oldestAgeSeconds.Value >= _options.AgeWarningSeconds))
            {
                return HealthCheckResult.Degraded(
                    $"DLQ warning: {pendingCount} pending entries" +
                    (oldestAgeSeconds.HasValue ? $", oldest is {oldestAgeSeconds.Value}s old" : ""),
                    data: data);
            }

            return HealthCheckResult.Healthy(
                $"DLQ healthy: {pendingCount} pending entries",
                data: data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "DLQ health check failed");
            return HealthCheckResult.Unhealthy(
                "DLQ health check failed",
                exception: ex);
        }
    }
}

/// <summary>
/// Configuration options for DLQ health check thresholds.
/// </summary>
public class DlqHealthCheckOptions
{
    public const string SectionName = "Dlq";

    /// <summary>
    /// Pending count threshold for Degraded status. Default: 100.
    /// </summary>
    public int WarningThresholdCount { get; set; } = 100;

    /// <summary>
    /// Pending count threshold for Unhealthy status. Default: 500.
    /// </summary>
    public int CriticalThresholdCount { get; set; } = 500;

    /// <summary>
    /// Age threshold (seconds) for Degraded status. Default: 3600 (1 hour).
    /// </summary>
    public long AgeWarningSeconds { get; set; } = 3600;

    /// <summary>
    /// Age threshold (seconds) for Unhealthy status. Default: 86400 (24 hours).
    /// </summary>
    public long AgeCriticalSeconds { get; set; } = 86400;
}
