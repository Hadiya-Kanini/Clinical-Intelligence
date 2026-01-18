namespace ClinicalIntelligence.Api.Services.ProcessingJobs;

/// <summary>
/// Abstraction for recording processing job metadata transitions (timing, retries, errors).
/// Follows DIP: queue/worker components depend on this interface, not the concrete DB implementation.
/// </summary>
public interface IProcessingJobMetadataWriter
{
    /// <summary>
    /// Marks a processing job as started. Sets Status to "Processing" and records StartedAt.
    /// </summary>
    /// <param name="jobId">The processing job ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if the job was found and updated; false otherwise.</returns>
    Task<bool> MarkStartedAsync(Guid jobId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Marks a processing job as completed successfully. Sets Status to "Completed",
    /// records CompletedAt, and computes ProcessingTimeMs from StartedAt.
    /// </summary>
    /// <param name="jobId">The processing job ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if the job was found and updated; false otherwise.</returns>
    Task<bool> MarkCompletedAsync(Guid jobId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Marks a processing job as failed. Sets Status to "Failed", records CompletedAt,
    /// computes ProcessingTimeMs, and stores the error message.
    /// </summary>
    /// <param name="jobId">The processing job ID.</param>
    /// <param name="errorMessage">User-safe error message (max 500 chars, truncated if longer).</param>
    /// <param name="errorDetails">Optional structured error details as JSON (internal use only).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if the job was found and updated; false otherwise.</returns>
    Task<bool> MarkFailedAsync(
        Guid jobId,
        string errorMessage,
        string? errorDetails = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Increments the retry count for a processing job.
    /// </summary>
    /// <param name="jobId">The processing job ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The new retry count, or -1 if the job was not found.</returns>
    Task<int> IncrementRetryCountAsync(Guid jobId, CancellationToken cancellationToken = default);
}
