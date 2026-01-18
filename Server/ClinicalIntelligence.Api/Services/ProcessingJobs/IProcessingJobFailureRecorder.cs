using System;
using System.Threading;
using System.Threading.Tasks;

namespace ClinicalIntelligence.Api.Services.ProcessingJobs;

/// <summary>
/// Interface for recording processing job failures with validation errors.
/// Supports dependency inversion for testability (DIP).
/// </summary>
public interface IProcessingJobFailureRecorder
{
    /// <summary>
    /// Records a validation failure for a processing job.
    /// Updates the job status to validation-failed and persists error details.
    /// </summary>
    /// <param name="processingJobId">The ID of the processing job.</param>
    /// <param name="errorMessage">Short human-readable error message.</param>
    /// <param name="errorDetailsJson">Structured error details as JSON string.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if the job was found and updated; false if the job was not found.</returns>
    Task<bool> RecordValidationFailureAsync(
        Guid processingJobId,
        string errorMessage,
        string? errorDetailsJson,
        CancellationToken cancellationToken = default);
}
