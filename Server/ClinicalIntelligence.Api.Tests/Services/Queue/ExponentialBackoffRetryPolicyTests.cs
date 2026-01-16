using ClinicalIntelligence.Api.Configuration;
using ClinicalIntelligence.Api.Domain.Enums;
using ClinicalIntelligence.Api.Services.Queue;
using Microsoft.Extensions.Options;
using Xunit;

namespace ClinicalIntelligence.Api.Tests.Services.Queue;

public class ExponentialBackoffRetryPolicyTests
{
    [Theory]
    [InlineData(0, 1000)]
    [InlineData(1, 2000)]
    [InlineData(2, 4000)]
    public void GetNextDelay_WithoutJitter_ReturnsExpectedDelay(int retryCount, int expectedMs)
    {
        // Arrange
        var options = Options.Create(new RetryPolicyOptions
        {
            InitialDelayMs = 1000,
            BackoffMultiplier = 2.0,
            EnableJitter = false
        });
        var policy = new ExponentialBackoffRetryPolicy(options);
        
        // Act
        var delay = policy.GetNextDelay(retryCount);
        
        // Assert
        Assert.Equal(expectedMs, delay.TotalMilliseconds);
    }
    
    [Fact]
    public void GetNextDelay_WithJitter_ReturnsDelayWithinRange()
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
        
        // Act - run multiple times to verify jitter
        var delays = Enumerable.Range(0, 100)
            .Select(_ => policy.GetNextDelay(0).TotalMilliseconds)
            .ToList();
        
        // Assert - all delays should be within 10% of base (900-1100ms)
        Assert.All(delays, d => Assert.InRange(d, 900, 1100));
        
        // Verify there's some variance (jitter is working)
        var distinctDelays = delays.Distinct().Count();
        Assert.True(distinctDelays > 1, "Jitter should produce varying delays");
    }
    
    [Fact]
    public void GetNextDelay_ExceedsMaxDelay_IsCapped()
    {
        // Arrange
        var options = Options.Create(new RetryPolicyOptions
        {
            InitialDelayMs = 1000,
            BackoffMultiplier = 10.0,
            MaxDelayMs = 5000,
            EnableJitter = false
        });
        var policy = new ExponentialBackoffRetryPolicy(options);
        
        // Act - retry 3 would be 1000 * 10^3 = 1,000,000ms without cap
        var delay = policy.GetNextDelay(3);
        
        // Assert
        Assert.Equal(5000, delay.TotalMilliseconds);
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
    public void IsRetryable_ReturnsExpectedResult(ProcessingErrorType errorType, bool expected)
    {
        // Arrange
        var options = Options.Create(new RetryPolicyOptions());
        var policy = new ExponentialBackoffRetryPolicy(options);
        
        // Act
        var result = policy.IsRetryable(errorType);
        
        // Assert
        Assert.Equal(expected, result);
    }
    
    [Theory]
    [InlineData(0, 3, true)]
    [InlineData(1, 3, true)]
    [InlineData(2, 3, true)]
    [InlineData(3, 3, false)]
    [InlineData(4, 3, false)]
    public void ShouldRetry_ReturnsExpectedResult(int currentRetryCount, int maxRetries, bool expected)
    {
        // Arrange
        var options = Options.Create(new RetryPolicyOptions { MaxRetries = maxRetries });
        var policy = new ExponentialBackoffRetryPolicy(options);
        
        // Act
        var result = policy.ShouldRetry(currentRetryCount);
        
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
    
    [Fact]
    public void GetNextDelay_ZeroRetryCount_ReturnsInitialDelay()
    {
        // Arrange
        var options = Options.Create(new RetryPolicyOptions
        {
            InitialDelayMs = 500,
            EnableJitter = false
        });
        var policy = new ExponentialBackoffRetryPolicy(options);
        
        // Act
        var delay = policy.GetNextDelay(0);
        
        // Assert
        Assert.Equal(500, delay.TotalMilliseconds);
    }
    
    [Fact]
    public void GetNextDelay_NeverReturnsNegative()
    {
        // Arrange
        var options = Options.Create(new RetryPolicyOptions
        {
            InitialDelayMs = 100,
            EnableJitter = true,
            JitterFactor = 0.5
        });
        var policy = new ExponentialBackoffRetryPolicy(options);
        
        // Act - run many times
        var delays = Enumerable.Range(0, 1000)
            .Select(_ => policy.GetNextDelay(0).TotalMilliseconds)
            .ToList();
        
        // Assert
        Assert.All(delays, d => Assert.True(d >= 0, "Delay should never be negative"));
    }
}
