using ClinicalIntelligence.Api.Contracts;
using ClinicalIntelligence.Api.Domain.Enums;
using ClinicalIntelligence.Api.Services.Queue;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace ClinicalIntelligence.Api.Tests.Services.Queue;

public class RetryHandlerTests
{
    [Fact]
    public void EvaluateRetry_NonRetryableError_ReturnsDlqDecision()
    {
        // Arrange
        var handler = CreateHandler();
        var job = CreateTestJob(retryCount: 0);
        
        // Act
        var decision = handler.EvaluateRetry(job, ProcessingErrorType.Permanent, "Invalid format");
        
        // Assert
        Assert.False(decision.ShouldRetry);
        Assert.True(decision.MoveToDlq);
        Assert.Contains("Non-retryable", decision.Reason);
    }
    
    [Fact]
    public void EvaluateRetry_RetryableError_ReturnsRetryDecision()
    {
        // Arrange
        var handler = CreateHandler();
        var job = CreateTestJob(retryCount: 0);
        
        // Act
        var decision = handler.EvaluateRetry(job, ProcessingErrorType.Transient, "Network timeout");
        
        // Assert
        Assert.True(decision.ShouldRetry);
        Assert.False(decision.MoveToDlq);
        Assert.Equal(1, decision.NextRetryCount);
        Assert.True(decision.Delay.TotalMilliseconds > 0);
    }
    
    [Fact]
    public void EvaluateRetry_MaxRetriesExhausted_ReturnsDlqDecision()
    {
        // Arrange
        var handler = CreateHandler(maxRetries: 3);
        var job = CreateTestJob(retryCount: 3);
        
        // Act
        var decision = handler.EvaluateRetry(job, ProcessingErrorType.Transient, "Network timeout");
        
        // Assert
        Assert.False(decision.ShouldRetry);
        Assert.True(decision.MoveToDlq);
        Assert.Contains("exhausted", decision.Reason);
    }
    
    [Theory]
    [InlineData(ProcessingErrorType.Transient)]
    [InlineData(ProcessingErrorType.ExternalService)]
    [InlineData(ProcessingErrorType.Database)]
    [InlineData(ProcessingErrorType.AiService)]
    public void EvaluateRetry_RetryableErrors_AllowRetry(ProcessingErrorType errorType)
    {
        // Arrange
        var handler = CreateHandler();
        var job = CreateTestJob(retryCount: 0);
        
        // Act
        var decision = handler.EvaluateRetry(job, errorType, "Error message");
        
        // Assert
        Assert.True(decision.ShouldRetry);
    }
    
    [Theory]
    [InlineData(ProcessingErrorType.Permanent)]
    [InlineData(ProcessingErrorType.Unauthorized)]
    public void EvaluateRetry_NonRetryableErrors_DenyRetry(ProcessingErrorType errorType)
    {
        // Arrange
        var handler = CreateHandler();
        var job = CreateTestJob(retryCount: 0);
        
        // Act
        var decision = handler.EvaluateRetry(job, errorType, "Error message");
        
        // Assert
        Assert.False(decision.ShouldRetry);
        Assert.True(decision.MoveToDlq);
    }
    
    [Fact]
    public async Task ScheduleRetryAsync_IncrementsRetryCount()
    {
        // Arrange
        var mockPublisher = new Mock<IMessagePublisher>();
        DocumentProcessingJob? publishedJob = null;
        mockPublisher
            .Setup(p => p.PublishDocumentJobAsync(It.IsAny<DocumentProcessingJob>(), It.IsAny<CancellationToken>()))
            .Callback<DocumentProcessingJob, CancellationToken>((job, _) => publishedJob = job)
            .ReturnsAsync(true);
        
        var handler = CreateHandler(publisher: mockPublisher.Object);
        var job = CreateTestJob(retryCount: 1);
        
        // Act
        await handler.ScheduleRetryAsync(job, TimeSpan.FromSeconds(1));
        
        // Assert
        Assert.NotNull(publishedJob);
        Assert.Equal(2, publishedJob.RetryCount);
        Assert.Equal(job.DocumentId, publishedJob.DocumentId);
    }
    
    [Fact]
    public async Task MoveToDeadLetterQueueAsync_ReturnsTrue()
    {
        // Arrange
        var handler = CreateHandler();
        var job = CreateTestJob();
        
        // Act
        var result = await handler.MoveToDeadLetterQueueAsync(job, "Max retries exhausted");
        
        // Assert
        Assert.True(result);
    }
    
    [Fact]
    public void EvaluateRetry_CalculatesCorrectDelay()
    {
        // Arrange
        var handler = CreateHandler();
        var job = CreateTestJob(retryCount: 0);
        
        // Act
        var decision = handler.EvaluateRetry(job, ProcessingErrorType.Transient, "Error");
        
        // Assert - first retry should have delay around 1000ms (with jitter)
        Assert.True(decision.Delay.TotalMilliseconds >= 900);
        Assert.True(decision.Delay.TotalMilliseconds <= 1100);
    }
    
    private static RetryHandler CreateHandler(int maxRetries = 3, IMessagePublisher? publisher = null)
    {
        var retryPolicy = new Mock<IRetryPolicy>();
        retryPolicy.Setup(p => p.MaxRetries).Returns(maxRetries);
        retryPolicy.Setup(p => p.IsRetryable(It.IsAny<ProcessingErrorType>()))
            .Returns<ProcessingErrorType>(e => 
                e != ProcessingErrorType.Permanent && e != ProcessingErrorType.Unauthorized);
        retryPolicy.Setup(p => p.ShouldRetry(It.IsAny<int>()))
            .Returns<int>(count => count < maxRetries);
        retryPolicy.Setup(p => p.GetNextDelay(It.IsAny<int>()))
            .Returns<int>(count => TimeSpan.FromMilliseconds(1000 * Math.Pow(2, count)));
        
        publisher ??= new Mock<IMessagePublisher>().Object;
        var logger = new Mock<ILogger<RetryHandler>>();
        
        return new RetryHandler(retryPolicy.Object, publisher, logger.Object);
    }
    
    private static DocumentProcessingJob CreateTestJob(int retryCount = 0)
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
            SizeBytes = 1024,
            CreatedAt = DateTime.UtcNow,
            RetryCount = retryCount
        };
    }
}
