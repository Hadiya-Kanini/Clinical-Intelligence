using ClinicalIntelligence.Api.Services.Queue;
using Xunit;

namespace ClinicalIntelligence.Api.Tests.Services.Queue;

public class RetryDecisionTests
{
    [Fact]
    public void RetryDecision_DefaultValues_AreCorrect()
    {
        // Arrange & Act
        var decision = new RetryDecision();
        
        // Assert
        Assert.False(decision.ShouldRetry);
        Assert.Equal(TimeSpan.Zero, decision.Delay);
        Assert.False(decision.MoveToDlq);
        Assert.Equal(string.Empty, decision.Reason);
        Assert.Equal(0, decision.NextRetryCount);
    }
    
    [Fact]
    public void RetryDecision_WithRetry_HasCorrectProperties()
    {
        // Arrange & Act
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
        Assert.Equal(TimeSpan.FromSeconds(2), decision.Delay);
        Assert.False(decision.MoveToDlq);
        Assert.Equal("Retry 1 of 3", decision.Reason);
        Assert.Equal(1, decision.NextRetryCount);
    }
    
    [Fact]
    public void RetryDecision_WithDlq_HasCorrectProperties()
    {
        // Arrange & Act
        var decision = new RetryDecision
        {
            ShouldRetry = false,
            MoveToDlq = true,
            Reason = "Max retries exhausted"
        };
        
        // Assert
        Assert.False(decision.ShouldRetry);
        Assert.True(decision.MoveToDlq);
        Assert.Equal("Max retries exhausted", decision.Reason);
    }
    
    [Fact]
    public void RetryDecision_IsImmutable_WithExpression()
    {
        // Arrange
        var original = new RetryDecision
        {
            ShouldRetry = true,
            Delay = TimeSpan.FromSeconds(1),
            NextRetryCount = 1
        };
        
        // Act
        var modified = original with { NextRetryCount = 2 };
        
        // Assert
        Assert.Equal(1, original.NextRetryCount);
        Assert.Equal(2, modified.NextRetryCount);
        Assert.Equal(original.ShouldRetry, modified.ShouldRetry);
        Assert.Equal(original.Delay, modified.Delay);
    }
    
    [Fact]
    public void RetryDecision_Equality_WorksCorrectly()
    {
        // Arrange
        var decision1 = new RetryDecision
        {
            ShouldRetry = true,
            Delay = TimeSpan.FromSeconds(1),
            Reason = "Test"
        };
        var decision2 = new RetryDecision
        {
            ShouldRetry = true,
            Delay = TimeSpan.FromSeconds(1),
            Reason = "Test"
        };
        var decision3 = new RetryDecision
        {
            ShouldRetry = false,
            Delay = TimeSpan.FromSeconds(1),
            Reason = "Test"
        };
        
        // Assert
        Assert.Equal(decision1, decision2);
        Assert.NotEqual(decision1, decision3);
    }
}
