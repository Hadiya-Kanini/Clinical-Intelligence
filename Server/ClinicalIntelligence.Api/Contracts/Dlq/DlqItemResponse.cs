namespace ClinicalIntelligence.Api.Contracts.Dlq;

/// <summary>
/// Response DTO for a single DLQ entry with full details.
/// </summary>
public record DlqItemResponse
{
    /// <summary>
    /// Unique identifier of the DLQ entry.
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    /// Reference to the original processing job.
    /// </summary>
    public Guid ProcessingJobId { get; init; }

    /// <summary>
    /// Reference to the document for operator context.
    /// </summary>
    public Guid DocumentId { get; init; }

    /// <summary>
    /// Original message payload (may be redacted for security).
    /// </summary>
    public string OriginalMessage { get; init; } = string.Empty;

    /// <summary>
    /// Schema version of the original message.
    /// </summary>
    public string MessageSchemaVersion { get; init; } = "1.0";

    /// <summary>
    /// Final error message that caused the job to be dead-lettered.
    /// </summary>
    public string? ErrorMessage { get; init; }

    /// <summary>
    /// Detailed error information.
    /// </summary>
    public string? ErrorDetails { get; init; }

    /// <summary>
    /// Retry history containing each retry attempt details.
    /// </summary>
    public string? RetryHistory { get; init; }

    /// <summary>
    /// Number of retry attempts before dead-lettering.
    /// </summary>
    public int RetryCount { get; init; }

    /// <summary>
    /// Reason for dead-lettering.
    /// </summary>
    public string DeadLetterReason { get; init; } = string.Empty;

    /// <summary>
    /// UTC timestamp when the job was moved to DLQ.
    /// </summary>
    public DateTime DeadLetteredAt { get; init; }

    /// <summary>
    /// DLQ entry status: Pending, Replayed, Discarded.
    /// </summary>
    public string Status { get; init; } = "Pending";

    /// <summary>
    /// UTC timestamp of the last operator action.
    /// </summary>
    public DateTime? LastActionAt { get; init; }

    /// <summary>
    /// User ID of the operator who performed the last action.
    /// </summary>
    public Guid? LastActionByUserId { get; init; }

    /// <summary>
    /// Number of replay attempts made on this DLQ entry.
    /// </summary>
    public int ReplayAttempts { get; init; }

    /// <summary>
    /// Error message from the last failed replay attempt.
    /// </summary>
    public string? LastReplayError { get; init; }

    /// <summary>
    /// New job ID created during replay (if successful).
    /// </summary>
    public Guid? ReplayedJobId { get; init; }
}

/// <summary>
/// Summary DTO for DLQ list view (excludes large payload fields).
/// </summary>
public record DlqItemSummary
{
    public Guid Id { get; init; }
    public Guid ProcessingJobId { get; init; }
    public Guid DocumentId { get; init; }
    public string? ErrorMessage { get; init; }
    public int RetryCount { get; init; }
    public string DeadLetterReason { get; init; } = string.Empty;
    public DateTime DeadLetteredAt { get; init; }
    public string Status { get; init; } = "Pending";
    public DateTime? LastActionAt { get; init; }
    public int ReplayAttempts { get; init; }
}
