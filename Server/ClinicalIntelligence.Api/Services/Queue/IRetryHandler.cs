using ClinicalIntelligence.Api.Contracts;
using ClinicalIntelligence.Api.Domain.Enums;

namespace ClinicalIntelligence.Api.Services.Queue;

/// <summary>
/// Handles retry decisions for failed processing jobs.
/// </summary>
public interface IRetryHandler
{
    /// <summary>
    /// Evaluates a failed job and determines retry action.
    /// </summary>
    RetryDecision EvaluateRetry(
        DocumentProcessingJob job, 
        ProcessingErrorType errorType, 
        string? errorMessage);
    
    /// <summary>
    /// Schedules a job for retry with delay.
    /// </summary>
    Task<bool> ScheduleRetryAsync(
        DocumentProcessingJob job, 
        TimeSpan delay, 
        CancellationToken ct = default);
    
    /// <summary>
    /// Moves a job to the dead letter queue.
    /// </summary>
    Task<bool> MoveToDeadLetterQueueAsync(
        DocumentProcessingJob job, 
        string reason, 
        CancellationToken ct = default);
}
