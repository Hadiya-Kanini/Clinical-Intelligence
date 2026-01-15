using System.ComponentModel.DataAnnotations;

namespace ClinicalIntelligence.Api.Domain.Models;

/// <summary>
/// Processing job entity for tracking document processing status.
/// Maps to the processing_jobs table per ERD specification.
/// </summary>
public sealed class ProcessingJob
{
    [Key]
    public Guid Id { get; set; }

    /// <summary>
    /// Reference to the document being processed.
    /// </summary>
    public Guid DocumentId { get; set; }

    /// <summary>
    /// Job status: Pending, Running, Completed, Failed.
    /// </summary>
    [Required]
    [MaxLength(30)]
    public string Status { get; set; } = "Pending";

    /// <summary>
    /// Number of retry attempts.
    /// </summary>
    public int RetryCount { get; set; }

    /// <summary>
    /// Error message if job failed.
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Detailed error information as JSON.
    /// </summary>
    public string? ErrorDetails { get; set; }

    /// <summary>
    /// Processing time in milliseconds.
    /// </summary>
    public int? ProcessingTimeMs { get; set; }

    /// <summary>
    /// Timestamp when processing started.
    /// </summary>
    public DateTime? StartedAt { get; set; }

    /// <summary>
    /// Timestamp when processing completed.
    /// </summary>
    public DateTime? CompletedAt { get; set; }

    // Navigation property
    public Document Document { get; set; } = null!;
}
