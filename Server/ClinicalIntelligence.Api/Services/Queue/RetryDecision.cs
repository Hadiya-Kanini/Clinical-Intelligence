namespace ClinicalIntelligence.Api.Services.Queue;

/// <summary>
/// Result of a retry decision.
/// </summary>
public record RetryDecision
{
    /// <summary>
    /// Whether the job should be retried.
    /// </summary>
    public bool ShouldRetry { get; init; }
    
    /// <summary>
    /// Delay before retry (if retrying).
    /// </summary>
    public TimeSpan Delay { get; init; }
    
    /// <summary>
    /// Whether to move to dead letter queue.
    /// </summary>
    public bool MoveToDlq { get; init; }
    
    /// <summary>
    /// Reason for the decision.
    /// </summary>
    public string Reason { get; init; } = string.Empty;
    
    /// <summary>
    /// Next retry count (if retrying).
    /// </summary>
    public int NextRetryCount { get; init; }
}
