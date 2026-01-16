namespace ClinicalIntelligence.Api.Configuration;

/// <summary>
/// Retry policy configuration per NFR-008.
/// </summary>
public class RetryPolicyOptions
{
    public const string SectionName = "RetryPolicy";
    
    /// <summary>
    /// Maximum number of retry attempts. Default: 3 (NFR-008).
    /// </summary>
    public int MaxRetries { get; set; } = 3;
    
    /// <summary>
    /// Initial delay in milliseconds. Default: 1000 (1 second).
    /// </summary>
    public int InitialDelayMs { get; set; } = 1000;
    
    /// <summary>
    /// Multiplier for exponential backoff. Default: 2.
    /// Delays: 1s, 2s, 4s for multiplier=2.
    /// </summary>
    public double BackoffMultiplier { get; set; } = 2.0;
    
    /// <summary>
    /// Maximum delay cap in milliseconds. Default: 30000 (30 seconds).
    /// </summary>
    public int MaxDelayMs { get; set; } = 30000;
    
    /// <summary>
    /// Add jitter to prevent thundering herd. Default: true.
    /// </summary>
    public bool EnableJitter { get; set; } = true;
    
    /// <summary>
    /// Jitter factor (0.0 to 1.0). Default: 0.1 (10% variance).
    /// </summary>
    public double JitterFactor { get; set; } = 0.1;
}
