using ClinicalIntelligence.Api.Contracts;

namespace ClinicalIntelligence.Api.Services.Queue;

/// <summary>
/// Abstraction for recording dead-letter queue entries from retry exhaustion path.
/// Follows DIP (Dependency Inversion Principle) to decouple DLQ persistence from retry handling.
/// </summary>
public interface IDeadLetterQueueWriter
{
    /// <summary>
    /// Writes a job to the dead-letter queue and updates the associated ProcessingJob status atomically.
    /// </summary>
    /// <param name="job">The document processing job that failed.</param>
    /// <param name="reason">Reason for dead-lettering (e.g., "Max retries exhausted").</param>
    /// <param name="errorMessage">Final error message.</param>
    /// <param name="errorDetails">Detailed error information as JSON.</param>
    /// <param name="retryHistory">JSON array of retry attempt details.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The ID of the created DeadLetterJob entry, or null if failed.</returns>
    Task<Guid?> WriteAsync(
        DocumentProcessingJob job,
        string reason,
        string? errorMessage,
        string? errorDetails,
        string? retryHistory,
        CancellationToken ct = default);
}
