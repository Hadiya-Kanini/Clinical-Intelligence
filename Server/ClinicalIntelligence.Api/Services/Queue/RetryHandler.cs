using ClinicalIntelligence.Api.Contracts;
using ClinicalIntelligence.Api.Domain.Enums;

namespace ClinicalIntelligence.Api.Services.Queue;

/// <summary>
/// Handles retry decisions for failed processing jobs per NFR-008.
/// </summary>
public class RetryHandler : IRetryHandler
{
    private readonly IRetryPolicy _retryPolicy;
    private readonly IMessagePublisher _publisher;
    private readonly ILogger<RetryHandler> _logger;
    
    public RetryHandler(
        IRetryPolicy retryPolicy,
        IMessagePublisher publisher,
        ILogger<RetryHandler> logger)
    {
        _retryPolicy = retryPolicy;
        _publisher = publisher;
        _logger = logger;
    }
    
    public RetryDecision EvaluateRetry(
        DocumentProcessingJob job, 
        ProcessingErrorType errorType, 
        string? errorMessage)
    {
        if (!_retryPolicy.IsRetryable(errorType))
        {
            _logger.LogInformation(
                "Non-retryable error for job {JobId}: {ErrorType}",
                job.JobId, errorType);
            
            return new RetryDecision
            {
                ShouldRetry = false,
                MoveToDlq = true,
                Reason = $"Non-retryable error: {errorType}"
            };
        }
        
        if (!_retryPolicy.ShouldRetry(job.RetryCount))
        {
            _logger.LogWarning(
                "Max retries exhausted for job {JobId}: {RetryCount}/{MaxRetries}",
                job.JobId, job.RetryCount, _retryPolicy.MaxRetries);
            
            return new RetryDecision
            {
                ShouldRetry = false,
                MoveToDlq = true,
                Reason = $"Max retries ({_retryPolicy.MaxRetries}) exhausted"
            };
        }
        
        var delay = _retryPolicy.GetNextDelay(job.RetryCount);
        
        _logger.LogInformation(
            "Scheduling retry for job {JobId}: Attempt {RetryCount}/{MaxRetries}, Delay={DelayMs}ms",
            job.JobId, job.RetryCount + 1, _retryPolicy.MaxRetries, delay.TotalMilliseconds);
        
        return new RetryDecision
        {
            ShouldRetry = true,
            Delay = delay,
            MoveToDlq = false,
            Reason = $"Retry {job.RetryCount + 1} of {_retryPolicy.MaxRetries}",
            NextRetryCount = job.RetryCount + 1
        };
    }
    
    public async Task<bool> ScheduleRetryAsync(
        DocumentProcessingJob job, 
        TimeSpan delay, 
        CancellationToken ct = default)
    {
        var retryJob = job with
        {
            RetryCount = job.RetryCount + 1,
            CreatedAt = DateTime.UtcNow
        };
        
        return await _publisher.PublishDocumentJobAsync(retryJob, ct);
    }
    
    public Task<bool> MoveToDeadLetterQueueAsync(
        DocumentProcessingJob job, 
        string reason, 
        CancellationToken ct = default)
    {
        _logger.LogError(
            "Moving job to DLQ: JobId={JobId}, DocumentId={DocumentId}, Reason={Reason}",
            job.JobId, job.DocumentId, reason);
        
        return Task.FromResult(true);
    }
}
