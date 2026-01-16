using ClinicalIntelligence.Api.Configuration;
using ClinicalIntelligence.Api.Domain.Enums;
using Microsoft.Extensions.Options;

namespace ClinicalIntelligence.Api.Services.Queue;

/// <summary>
/// Exponential backoff retry policy per NFR-008.
/// Delays: 1s, 2s, 4s (with optional jitter).
/// </summary>
public class ExponentialBackoffRetryPolicy : IRetryPolicy
{
    private readonly RetryPolicyOptions _options;
    private readonly Random _random;
    
    private static readonly HashSet<ProcessingErrorType> NonRetryableErrors = new()
    {
        ProcessingErrorType.Permanent,
        ProcessingErrorType.Unauthorized
    };
    
    public ExponentialBackoffRetryPolicy(IOptions<RetryPolicyOptions> options)
    {
        _options = options.Value;
        _random = new Random();
    }
    
    public int MaxRetries => _options.MaxRetries;
    
    public bool IsRetryable(ProcessingErrorType errorType)
    {
        return !NonRetryableErrors.Contains(errorType);
    }
    
    public bool ShouldRetry(int currentRetryCount)
    {
        return currentRetryCount < _options.MaxRetries;
    }
    
    public TimeSpan GetNextDelay(int retryCount)
    {
        var baseDelayMs = _options.InitialDelayMs * Math.Pow(_options.BackoffMultiplier, retryCount);
        
        var delayMs = Math.Min(baseDelayMs, _options.MaxDelayMs);
        
        if (_options.EnableJitter)
        {
            var jitterRange = delayMs * _options.JitterFactor;
            var jitter = (_random.NextDouble() * 2 - 1) * jitterRange;
            delayMs += jitter;
        }
        
        return TimeSpan.FromMilliseconds(Math.Max(0, delayMs));
    }
}
