# Task - [TASK_002]

## Requirement Reference
- User Story: [us_054]
- Story Location: [.propel/context/tasks/us_054/us_054.md]
- Acceptance Criteria: 
    - Given a processing job fails, When retried, Then exponential backoff is applied (e.g., 1s, 2s, 4s) (NFR-008).
    - Given retry attempts, When tracked, Then the retry count is recorded in job metadata (FR-027).
    - Given maximum retries (3), When exhausted, Then the job is moved to dead letter queue.

## Task Overview
Implement comprehensive unit tests for the retry policy and exponential backoff logic. Tests verify delay calculations, error classification, retry limits, and DLQ routing.

## Dependent Tasks
- [US_054/task_001] - Backend retry with exponential backoff implementation

## Impacted Components
- [CREATE | Server/ClinicalIntelligence.Api.Tests/Services/Queue/ExponentialBackoffRetryPolicyTests.cs | Retry policy tests]
- [CREATE | Server/ClinicalIntelligence.Api.Tests/Services/Queue/RetryHandlerTests.cs | Retry handler tests]
- [CREATE | Server/ClinicalIntelligence.Api.Tests/Domain/ProcessingErrorTypeTests.cs | Error type tests]

## Implementation Plan

### 1. Exponential Backoff Retry Policy Tests
```csharp
namespace ClinicalIntelligence.Api.Tests.Services.Queue;

public class ExponentialBackoffRetryPolicyTests
{
    [Theory]
    [InlineData(0, 1000)]   // First retry: 1s
    [InlineData(1, 2000)]   // Second retry: 2s
    [InlineData(2, 4000)]   // Third retry: 4s
    [InlineData(3, 8000)]   // Fourth retry: 8s (if allowed)
    public void GetNextDelay_ReturnsExponentialBackoff(int retryCount, int expectedBaseMs)
    {
        // Arrange
        var options = Options.Create(new RetryPolicyOptions
        {
            InitialDelayMs = 1000,
            BackoffMultiplier = 2.0,
            EnableJitter = false // Disable jitter for predictable testing
        });
        var policy = new ExponentialBackoffRetryPolicy(options);
        
        // Act
        var delay = policy.GetNextDelay(retryCount);
        
        // Assert
        Assert.Equal(expectedBaseMs, delay.TotalMilliseconds);
    }
    
    [Fact]
    public void GetNextDelay_RespectsMaxDelayCap()
    {
        // Arrange
        var options = Options.Create(new RetryPolicyOptions
        {
            InitialDelayMs = 1000,
            BackoffMultiplier = 2.0,
            MaxDelayMs = 5000,
            EnableJitter = false
        });
        var policy = new ExponentialBackoffRetryPolicy(options);
        
        // Act - retry 10 would be 1024 seconds without cap
        var delay = policy.GetNextDelay(10);
        
        // Assert
        Assert.Equal(5000, delay.TotalMilliseconds);
    }
    
    [Fact]
    public void GetNextDelay_WithJitter_VariesDelay()
    {
        // Arrange
        var options = Options.Create(new RetryPolicyOptions
        {
            InitialDelayMs = 1000,
            BackoffMultiplier = 2.0,
            EnableJitter = true,
            JitterFactor = 0.1
        });
        var policy = new ExponentialBackoffRetryPolicy(options);
        
        // Act - get multiple delays
        var delays = Enumerable.Range(0, 100)
            .Select(_ => policy.GetNextDelay(0).TotalMilliseconds)
            .ToList();
        
        // Assert - delays should vary within jitter range (900-1100ms)
        Assert.True(delays.Min() >= 900);
        Assert.True(delays.Max() <= 1100);
        Assert.True(delays.Distinct().Count() > 1); // Should have variance
    }
    
    [Theory]
    [InlineData(0, true)]   // 0 retries, should retry
    [InlineData(1, true)]   // 1 retry, should retry
    [InlineData(2, true)]   // 2 retries, should retry
    [InlineData(3, false)]  // 3 retries (max), should not retry
    [InlineData(4, false)]  // 4 retries, should not retry
    public void ShouldRetry_RespectsMaxRetries(int currentRetryCount, bool expected)
    {
        // Arrange
        var options = Options.Create(new RetryPolicyOptions { MaxRetries = 3 });
        var policy = new ExponentialBackoffRetryPolicy(options);
        
        // Act
        var result = policy.ShouldRetry(currentRetryCount);
        
        // Assert
        Assert.Equal(expected, result);
    }
    
    [Theory]
    [InlineData(ProcessingErrorType.Transient, true)]
    [InlineData(ProcessingErrorType.ExternalService, true)]
    [InlineData(ProcessingErrorType.Database, true)]
    [InlineData(ProcessingErrorType.AiService, true)]
    [InlineData(ProcessingErrorType.NotFound, true)]
    [InlineData(ProcessingErrorType.Unknown, true)]
    [InlineData(ProcessingErrorType.Permanent, false)]
    [InlineData(ProcessingErrorType.Unauthorized, false)]
    public void IsRetryable_ClassifiesErrorsCorrectly(ProcessingErrorType errorType, bool expected)
    {
        // Arrange
        var options = Options.Create(new RetryPolicyOptions());
        var policy = new ExponentialBackoffRetryPolicy(options);
        
        // Act
        var result = policy.IsRetryable(errorType);
        
        // Assert
        Assert.Equal(expected, result);
    }
    
    [Fact]
    public void MaxRetries_ReturnsConfiguredValue()
    {
        // Arrange
        var options = Options.Create(new RetryPolicyOptions { MaxRetries = 5 });
        var policy = new ExponentialBackoffRetryPolicy(options);
        
        // Assert
        Assert.Equal(5, policy.MaxRetries);
    }
}
```

### 2. Retry Handler Tests
```csharp
namespace ClinicalIntelligence.Api.Tests.Services.Queue;

public class RetryHandlerTests
{
    [Fact]
    public void EvaluateRetry_RetryableError_FirstAttempt_ReturnsRetry()
    {
        // Arrange
        var handler = CreateHandler();
        var job = CreateJob(retryCount: 0);
        
        // Act
        var decision = handler.EvaluateRetry(job, ProcessingErrorType.Transient, "Timeout");
        
        // Assert
        Assert.True(decision.ShouldRetry);
        Assert.False(decision.MoveToDlq);
        Assert.Equal(1, decision.NextRetryCount);
        Assert.True(decision.Delay.TotalMilliseconds > 0);
    }
    
    [Fact]
    public void EvaluateRetry_RetryableError_MaxRetriesExhausted_ReturnsDlq()
    {
        // Arrange
        var handler = CreateHandler(maxRetries: 3);
        var job = CreateJob(retryCount: 3); // Already at max
        
        // Act
        var decision = handler.EvaluateRetry(job, ProcessingErrorType.Transient, "Timeout");
        
        // Assert
        Assert.False(decision.ShouldRetry);
        Assert.True(decision.MoveToDlq);
        Assert.Contains("exhausted", decision.Reason.ToLower());
    }
    
    [Fact]
    public void EvaluateRetry_NonRetryableError_ReturnsDlqImmediately()
    {
        // Arrange
        var handler = CreateHandler();
        var job = CreateJob(retryCount: 0);
        
        // Act
        var decision = handler.EvaluateRetry(job, ProcessingErrorType.Permanent, "Invalid format");
        
        // Assert
        Assert.False(decision.ShouldRetry);
        Assert.True(decision.MoveToDlq);
        Assert.Contains("non-retryable", decision.Reason.ToLower());
    }
    
    [Fact]
    public void EvaluateRetry_UnauthorizedError_ReturnsDlqImmediately()
    {
        // Arrange
        var handler = CreateHandler();
        var job = CreateJob(retryCount: 0);
        
        // Act
        var decision = handler.EvaluateRetry(job, ProcessingErrorType.Unauthorized, "Access denied");
        
        // Assert
        Assert.False(decision.ShouldRetry);
        Assert.True(decision.MoveToDlq);
    }
    
    [Theory]
    [InlineData(0, 1000)]
    [InlineData(1, 2000)]
    [InlineData(2, 4000)]
    public void EvaluateRetry_CalculatesCorrectDelay(int retryCount, int expectedDelayMs)
    {
        // Arrange
        var handler = CreateHandler(enableJitter: false);
        var job = CreateJob(retryCount: retryCount);
        
        // Act
        var decision = handler.EvaluateRetry(job, ProcessingErrorType.Transient, "Error");
        
        // Assert
        Assert.Equal(expectedDelayMs, decision.Delay.TotalMilliseconds);
    }
    
    [Fact]
    public void EvaluateRetry_IncrementsRetryCount()
    {
        // Arrange
        var handler = CreateHandler();
        var job = CreateJob(retryCount: 1);
        
        // Act
        var decision = handler.EvaluateRetry(job, ProcessingErrorType.Transient, "Error");
        
        // Assert
        Assert.Equal(2, decision.NextRetryCount);
    }
    
    private static RetryHandler CreateHandler(int maxRetries = 3, bool enableJitter = false)
    {
        var policyOptions = Options.Create(new RetryPolicyOptions
        {
            MaxRetries = maxRetries,
            InitialDelayMs = 1000,
            BackoffMultiplier = 2.0,
            EnableJitter = enableJitter
        });
        var policy = new ExponentialBackoffRetryPolicy(policyOptions);
        var publisher = new Mock<IMessagePublisher>();
        var logger = new NullLogger<RetryHandler>();
        
        return new RetryHandler(policy, publisher.Object, logger);
    }
    
    private static DocumentProcessingJob CreateJob(int retryCount = 0)
    {
        return new DocumentProcessingJob
        {
            JobId = Guid.NewGuid(),
            DocumentId = Guid.NewGuid(),
            PatientId = Guid.NewGuid(),
            UploadedByUserId = Guid.NewGuid(),
            OriginalName = "test.pdf",
            MimeType = "application/pdf",
            StoragePath = "path/to/file.pdf",
            SizeBytes = 1000,
            CreatedAt = DateTime.UtcNow,
            RetryCount = retryCount
        };
    }
}
```

### 3. Processing Error Type Tests
```csharp
namespace ClinicalIntelligence.Api.Tests.Domain;

public class ProcessingErrorTypeTests
{
    [Fact]
    public void ProcessingErrorType_HasExpectedValues()
    {
        // Assert all expected error types exist
        Assert.True(Enum.IsDefined(typeof(ProcessingErrorType), ProcessingErrorType.Unknown));
        Assert.True(Enum.IsDefined(typeof(ProcessingErrorType), ProcessingErrorType.Transient));
        Assert.True(Enum.IsDefined(typeof(ProcessingErrorType), ProcessingErrorType.Permanent));
        Assert.True(Enum.IsDefined(typeof(ProcessingErrorType), ProcessingErrorType.NotFound));
        Assert.True(Enum.IsDefined(typeof(ProcessingErrorType), ProcessingErrorType.Unauthorized));
        Assert.True(Enum.IsDefined(typeof(ProcessingErrorType), ProcessingErrorType.ExternalService));
        Assert.True(Enum.IsDefined(typeof(ProcessingErrorType), ProcessingErrorType.Database));
        Assert.True(Enum.IsDefined(typeof(ProcessingErrorType), ProcessingErrorType.AiService));
    }
    
    [Fact]
    public void ProcessingErrorType_CanBeParsedFromString()
    {
        // Arrange
        var errorTypeString = "Transient";
        
        // Act
        var parsed = Enum.Parse<ProcessingErrorType>(errorTypeString);
        
        // Assert
        Assert.Equal(ProcessingErrorType.Transient, parsed);
    }
    
    [Fact]
    public void ProcessingErrorType_CanBeConvertedToString()
    {
        // Arrange
        var errorType = ProcessingErrorType.ExternalService;
        
        // Act
        var asString = errorType.ToString();
        
        // Assert
        Assert.Equal("ExternalService", asString);
    }
}
```

### 4. Retry Decision Tests
```csharp
namespace ClinicalIntelligence.Api.Tests.Services.Queue;

public class RetryDecisionTests
{
    [Fact]
    public void RetryDecision_RetryScenario_HasCorrectProperties()
    {
        // Arrange
        var decision = new RetryDecision
        {
            ShouldRetry = true,
            Delay = TimeSpan.FromSeconds(2),
            MoveToDlq = false,
            Reason = "Retry 1 of 3",
            NextRetryCount = 1
        };
        
        // Assert
        Assert.True(decision.ShouldRetry);
        Assert.Equal(2000, decision.Delay.TotalMilliseconds);
        Assert.False(decision.MoveToDlq);
        Assert.Equal(1, decision.NextRetryCount);
    }
    
    [Fact]
    public void RetryDecision_DlqScenario_HasCorrectProperties()
    {
        // Arrange
        var decision = new RetryDecision
        {
            ShouldRetry = false,
            Delay = TimeSpan.Zero,
            MoveToDlq = true,
            Reason = "Max retries exhausted",
            NextRetryCount = 0
        };
        
        // Assert
        Assert.False(decision.ShouldRetry);
        Assert.True(decision.MoveToDlq);
        Assert.Equal(TimeSpan.Zero, decision.Delay);
    }
}
```

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| CREATE | Server/ClinicalIntelligence.Api.Tests/Services/Queue/ExponentialBackoffRetryPolicyTests.cs | Policy tests |
| CREATE | Server/ClinicalIntelligence.Api.Tests/Services/Queue/RetryHandlerTests.cs | Handler tests |
| CREATE | Server/ClinicalIntelligence.Api.Tests/Services/Queue/RetryDecisionTests.cs | Decision tests |
| CREATE | Server/ClinicalIntelligence.Api.Tests/Domain/ProcessingErrorTypeTests.cs | Error type tests |

## Build Commands
- dotnet test Server/ClinicalIntelligence.Api.Tests --filter "FullyQualifiedName~Retry"
- dotnet test Server/ClinicalIntelligence.Api.Tests --filter "FullyQualifiedName~ExponentialBackoff"

## Implementation Validation Strategy
- [Automated] All unit tests pass
- [Automated] Code coverage >= 90% for retry logic
- [Manual] Verify delay calculations match NFR-008 (1s, 2s, 4s)

## Implementation Checklist
- [x] Create ExponentialBackoffRetryPolicyTests
- [x] Add tests for delay calculation with/without jitter
- [x] Add tests for max delay cap
- [x] Add tests for retry count limits
- [x] Create RetryHandlerTests
- [x] Add tests for error classification
- [x] Add tests for DLQ routing
- [x] Create ProcessingErrorTypeTests
- [x] Create RetryDecisionTests
- [x] Verify all tests pass
