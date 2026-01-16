namespace ClinicalIntelligence.Api.Contracts.Dlq;

/// <summary>
/// Response DTO for DLQ replay operation.
/// </summary>
public record DlqReplayResponse
{
    /// <summary>
    /// ID of the DLQ entry that was replayed.
    /// </summary>
    public Guid DeadLetterJobId { get; init; }

    /// <summary>
    /// New job ID created for the replay.
    /// </summary>
    public Guid? NewJobId { get; init; }

    /// <summary>
    /// Updated status of the DLQ entry.
    /// </summary>
    public string Status { get; init; } = string.Empty;

    /// <summary>
    /// Number of replay attempts including this one.
    /// </summary>
    public int ReplayAttempts { get; init; }

    /// <summary>
    /// Whether the replay was successful.
    /// </summary>
    public bool Success { get; init; }

    /// <summary>
    /// Message describing the result.
    /// </summary>
    public string Message { get; init; } = string.Empty;
}

/// <summary>
/// Response DTO for DLQ discard operation.
/// </summary>
public record DlqDiscardResponse
{
    /// <summary>
    /// ID of the DLQ entry that was discarded.
    /// </summary>
    public Guid DeadLetterJobId { get; init; }

    /// <summary>
    /// Updated status of the DLQ entry.
    /// </summary>
    public string Status { get; init; } = string.Empty;

    /// <summary>
    /// Whether the discard was successful.
    /// </summary>
    public bool Success { get; init; }

    /// <summary>
    /// Message describing the result.
    /// </summary>
    public string Message { get; init; } = string.Empty;
}
