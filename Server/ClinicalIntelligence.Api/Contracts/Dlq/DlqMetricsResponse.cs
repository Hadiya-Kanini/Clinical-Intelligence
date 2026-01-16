namespace ClinicalIntelligence.Api.Contracts.Dlq;

/// <summary>
/// Response DTO for DLQ depth metrics (NFR-011).
/// </summary>
public record DlqMetricsResponse
{
    /// <summary>
    /// Total count of DLQ entries.
    /// </summary>
    public int TotalCount { get; init; }

    /// <summary>
    /// Count of pending (unprocessed) DLQ entries.
    /// </summary>
    public int PendingCount { get; init; }

    /// <summary>
    /// Count of replayed DLQ entries.
    /// </summary>
    public int ReplayedCount { get; init; }

    /// <summary>
    /// Count of discarded DLQ entries.
    /// </summary>
    public int DiscardedCount { get; init; }

    /// <summary>
    /// Age of the oldest pending DLQ entry in seconds.
    /// Null if no pending entries exist.
    /// </summary>
    public long? OldestPendingAgeSeconds { get; init; }

    /// <summary>
    /// UTC timestamp of the oldest pending DLQ entry.
    /// Null if no pending entries exist.
    /// </summary>
    public DateTime? OldestPendingAt { get; init; }

    /// <summary>
    /// Health status based on configured thresholds.
    /// </summary>
    public string HealthStatus { get; init; } = "Healthy";

    /// <summary>
    /// UTC timestamp when metrics were captured.
    /// </summary>
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
}
