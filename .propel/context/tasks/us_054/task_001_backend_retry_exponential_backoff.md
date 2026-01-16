# Task - [TASK_001]

## Requirement Reference
- User Story: [us_054]
- Story Location: [.propel/context/tasks/us_054/us_054.md]
- Acceptance Criteria: 
    - Given a processing job fails, When retried, Then exponential backoff is applied (e.g., 1s, 2s, 4s) (NFR-008).
    - Given retry attempts, When tracked, Then the retry count is recorded in job metadata (FR-027).
    - Given maximum retries (3), When exhausted, Then the job is moved to dead letter queue.
    - Given retry logic, When implemented, Then it distinguishes between retryable and non-retryable errors.

## Task Overview
Implement retry logic with exponential backoff for failed document processing jobs. The implementation includes retry policy configuration, error classification (retryable vs non-retryable), and dead letter queue routing for jobs that exhaust retries.

## Dependent Tasks
- [US_053/task_001] - Backend RabbitMQ integration (provides queue infrastructure)

## Impacted Components
- [CREATE | Server/ClinicalIntelligence.Api/Configuration/RetryPolicyOptions.cs | Retry policy configuration]
- [CREATE | Server/ClinicalIntelligence.Api/Services/Queue/IRetryPolicy.cs | Retry policy interface]
- [CREATE | Server/ClinicalIntelligence.Api/Services/Queue/ExponentialBackoffRetryPolicy.cs | Retry policy implementation]
- [CREATE | Server/ClinicalIntelligence.Api/Domain/Enums/ProcessingErrorType.cs | Error classification enum]
- [MODIFY | Server/ClinicalIntelligence.Api/Services/Queue/RabbitMqPublisher.cs | Add retry support]
- [MODIFY | Server/ClinicalIntelligence.Api/Contracts/DocumentProcessingJob.cs | Add retry metadata fields]

## Implementation Plan

### 1. Create Retry Policy Configuration
```csharp
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
```

### 2. Create Processing Error Type Enum
```csharp
namespace ClinicalIntelligence.Api.Domain.Enums;

/// <summary>
/// Classification of processing errors for retry decisions.
/// </summary>
public enum ProcessingErrorType
{
    /// <summary>
    /// Unknown error type - treat as retryable.
    /// </summary>
    Unknown,
    
    /// <summary>
    /// Transient errors that may succeed on retry.
    /// Examples: network timeout, service unavailable, rate limit.
    /// </summary>
    Transient,
    
    /// <summary>
    /// Permanent errors that will not succeed on retry.
    /// Examples: invalid document format, validation failure, malformed data.
    /// </summary>
    Permanent,
    
    /// <summary>
    /// Resource not found - may be retryable if timing issue.
    /// </summary>
    NotFound,
    
    /// <summary>
    /// Authentication/authorization failure - not retryable.
    /// </summary>
    Unauthorized,
    
    /// <summary>
    /// External service error - retryable.
    /// </summary>
    ExternalService,
    
    /// <summary>
    /// Database error - may be retryable.
    /// </summary>
    Database,
    
    /// <summary>
    /// AI/LLM service error - retryable.
    /// </summary>
    AiService
}
```

### 3. Create Retry Policy Interface
```csharp
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
```

### 4. Implement Exponential Backoff Retry Policy
```csharp
namespace ClinicalIntelligence.Api.Services.Queue;

/// <summary>
/// Exponential backoff retry policy per NFR-008.
/// Delays: 1s, 2s, 4s (with optional jitter).
/// </summary>
public class ExponentialBackoffRetryPolicy : IRetryPolicy
{
    private readonly RetryPolicyOptions _options;
    private readonly Random _random;
    
    // Error types that should NOT be retried
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
        // Calculate base delay with exponential backoff
        // Retry 0: 1s, Retry 1: 2s, Retry 2: 4s
        var baseDelayMs = _options.InitialDelayMs * Math.Pow(_options.BackoffMultiplier, retryCount);
        
        // Apply maximum cap
        var delayMs = Math.Min(baseDelayMs, _options.MaxDelayMs);
        
        // Apply jitter if enabled
        if (_options.EnableJitter)
        {
            var jitterRange = delayMs * _options.JitterFactor;
            var jitter = (_random.NextDouble() * 2 - 1) * jitterRange; // -jitter to +jitter
            delayMs += jitter;
        }
        
        return TimeSpan.FromMilliseconds(Math.Max(0, delayMs));
    }
}
```

### 5. Create Retry Result Record
```csharp
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
```

### 6. Create Retry Handler Service
```csharp
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
        // Check if error type is retryable
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
        
        // Check if retries exhausted
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
        
        // Calculate delay for next retry
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
        // Create retry job with incremented count
        var retryJob = job with
        {
            RetryCount = job.RetryCount + 1,
            CreatedAt = DateTime.UtcNow
        };
        
        // For RabbitMQ, we use delayed message exchange or TTL
        // Simple approach: publish with x-delay header
        return await _publisher.PublishDocumentJobAsync(retryJob, ct);
    }
    
    public async Task<bool> MoveToDeadLetterQueueAsync(
        DocumentProcessingJob job, 
        string reason, 
        CancellationToken ct = default)
    {
        _logger.LogError(
            "Moving job to DLQ: JobId={JobId}, DocumentId={DocumentId}, Reason={Reason}",
            job.JobId, job.DocumentId, reason);
        
        // The message will be automatically routed to DLQ by RabbitMQ
        // when rejected without requeue
        return await Task.FromResult(true);
    }
}
```

### 7. Update DocumentProcessingJob Contract
```csharp
// Add to DocumentProcessingJob record
/// <summary>
/// Error type from last failure (for retry decisions).
/// </summary>
public ProcessingErrorType? LastErrorType { get; init; }

/// <summary>
/// Error message from last failure.
/// </summary>
public string? LastErrorMessage { get; init; }

/// <summary>
/// Timestamp of last failure.
/// </summary>
public DateTime? LastFailedAt { get; init; }

/// <summary>
/// History of retry attempts with timestamps.
/// </summary>
public IReadOnlyList<RetryAttempt>? RetryHistory { get; init; }

public record RetryAttempt
{
    public int AttemptNumber { get; init; }
    public DateTime AttemptedAt { get; init; }
    public ProcessingErrorType ErrorType { get; init; }
    public string? ErrorMessage { get; init; }
    public TimeSpan? ProcessingDuration { get; init; }
}
```

### 8. Register Services in Program.cs
```csharp
// Add retry policy configuration
builder.Services.Configure<RetryPolicyOptions>(
    builder.Configuration.GetSection(RetryPolicyOptions.SectionName));

// Register retry services
builder.Services.AddSingleton<IRetryPolicy, ExponentialBackoffRetryPolicy>();
builder.Services.AddScoped<IRetryHandler, RetryHandler>();
```

## Current Project State
```
Server/ClinicalIntelligence.Api/
├── Configuration/
│   └── RabbitMqOptions.cs    # From US_053
├── Services/Queue/
│   ├── IMessagePublisher.cs  # From US_053
│   └── RabbitMqPublisher.cs  # From US_053
├── Contracts/
│   └── DocumentProcessingJob.cs  # From US_053
```

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| CREATE | Server/ClinicalIntelligence.Api/Configuration/RetryPolicyOptions.cs | Retry configuration |
| CREATE | Server/ClinicalIntelligence.Api/Domain/Enums/ProcessingErrorType.cs | Error classification |
| CREATE | Server/ClinicalIntelligence.Api/Services/Queue/IRetryPolicy.cs | Retry policy interface |
| CREATE | Server/ClinicalIntelligence.Api/Services/Queue/ExponentialBackoffRetryPolicy.cs | Backoff implementation |
| CREATE | Server/ClinicalIntelligence.Api/Services/Queue/IRetryHandler.cs | Retry handler interface |
| CREATE | Server/ClinicalIntelligence.Api/Services/Queue/RetryHandler.cs | Retry handler implementation |
| MODIFY | Server/ClinicalIntelligence.Api/Contracts/DocumentProcessingJob.cs | Add retry metadata |
| MODIFY | Server/ClinicalIntelligence.Api/Program.cs | Register retry services |

## External References
- https://learn.microsoft.com/en-us/dotnet/architecture/microservices/implement-resilient-applications/implement-retries-exponential-backoff
- https://www.rabbitmq.com/docs/dlx

## Build Commands
- dotnet build Server/ClinicalIntelligence.Api
- dotnet test Server/ClinicalIntelligence.Api.Tests

## Implementation Validation Strategy
- [Automated] Unit tests verify exponential backoff calculation
- [Automated] Unit tests verify error type classification
- [Automated] Unit tests verify retry count limits
- [Manual] Verify delays: 1s, 2s, 4s for retries 0, 1, 2
- [Manual] Verify DLQ routing after 3 failures

## Implementation Checklist
- [x] Create RetryPolicyOptions configuration class
- [x] Create ProcessingErrorType enum
- [x] Create IRetryPolicy interface
- [x] Implement ExponentialBackoffRetryPolicy
- [x] Create IRetryHandler interface
- [x] Implement RetryHandler with DLQ support
- [x] Update DocumentProcessingJob with retry metadata
- [x] Register services in Program.cs
- [x] Add appsettings.json configuration section
- [x] Add unit tests for retry policy
