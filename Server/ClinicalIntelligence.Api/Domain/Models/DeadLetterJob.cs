using System.ComponentModel.DataAnnotations;

namespace ClinicalIntelligence.Api.Domain.Models;

/// <summary>
/// Dead-lettered processing job entity for tracking failed jobs after max retries.
/// Maps to the dead_letter_jobs table per US_055 specification.
/// </summary>
public sealed class DeadLetterJob
{
    [Key]
    public Guid Id { get; set; }

    /// <summary>
    /// Reference to the original processing job that failed.
    /// </summary>
    public Guid ProcessingJobId { get; set; }

    /// <summary>
    /// Reference to the document for operator context.
    /// </summary>
    public Guid DocumentId { get; set; }

    /// <summary>
    /// Original message payload as JSON for replay capability.
    /// </summary>
    public string OriginalMessage { get; set; } = string.Empty;

    /// <summary>
    /// Schema version of the original message for compatibility.
    /// </summary>
    [MaxLength(20)]
    public string MessageSchemaVersion { get; set; } = "1.0";

    /// <summary>
    /// Final error message that caused the job to be dead-lettered.
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Detailed error information as JSON (stack trace, inner exceptions, etc.).
    /// </summary>
    public string? ErrorDetails { get; set; }

    /// <summary>
    /// Retry history as JSON array containing each retry attempt details.
    /// </summary>
    public string? RetryHistory { get; set; }

    /// <summary>
    /// Number of retry attempts before dead-lettering.
    /// </summary>
    public int RetryCount { get; set; }

    /// <summary>
    /// Reason for dead-lettering (e.g., "Max retries exhausted", "Non-retryable error").
    /// </summary>
    [Required]
    [MaxLength(200)]
    public string DeadLetterReason { get; set; } = string.Empty;

    /// <summary>
    /// UTC timestamp when the job was moved to DLQ.
    /// </summary>
    public DateTime DeadLetteredAt { get; set; }

    /// <summary>
    /// DLQ entry status: Pending, Replayed, Discarded.
    /// </summary>
    [Required]
    [MaxLength(20)]
    public string Status { get; set; } = DeadLetterJobStatus.Pending;

    /// <summary>
    /// UTC timestamp of the last operator action (replay/discard).
    /// </summary>
    public DateTime? LastActionAt { get; set; }

    /// <summary>
    /// User ID of the operator who performed the last action.
    /// </summary>
    public Guid? LastActionByUserId { get; set; }

    /// <summary>
    /// Number of replay attempts made on this DLQ entry.
    /// </summary>
    public int ReplayAttempts { get; set; }

    /// <summary>
    /// Error message from the last failed replay attempt.
    /// </summary>
    public string? LastReplayError { get; set; }

    /// <summary>
    /// New job ID created during replay (if successful).
    /// </summary>
    public Guid? ReplayedJobId { get; set; }

    /// <summary>
    /// Concurrency token for optimistic concurrency control.
    /// </summary>
    [Timestamp]
    public byte[]? RowVersion { get; set; }

    // Navigation properties
    public ProcessingJob? ProcessingJob { get; set; }
    public Document? Document { get; set; }
    public User? LastActionByUser { get; set; }
}

/// <summary>
/// Status constants for dead-letter jobs.
/// </summary>
public static class DeadLetterJobStatus
{
    public const string Pending = "Pending";
    public const string Replayed = "Replayed";
    public const string Discarded = "Discarded";
}
