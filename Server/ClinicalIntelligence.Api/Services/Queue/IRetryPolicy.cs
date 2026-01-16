using ClinicalIntelligence.Api.Domain.Enums;

namespace ClinicalIntelligence.Api.Services.Queue;

/// <summary>
/// Interface for retry policy decisions.
/// </summary>
public interface IRetryPolicy
{
    /// <summary>
    /// Determines if an error is retryable.
    /// </summary>
    bool IsRetryable(ProcessingErrorType errorType);
    
    /// <summary>
    /// Determines if more retries are allowed.
    /// </summary>
    bool ShouldRetry(int currentRetryCount);
    
    /// <summary>
    /// Calculates the delay before next retry.
    /// </summary>
    TimeSpan GetNextDelay(int retryCount);
    
    /// <summary>
    /// Gets the maximum number of retries.
    /// </summary>
    int MaxRetries { get; }
}
