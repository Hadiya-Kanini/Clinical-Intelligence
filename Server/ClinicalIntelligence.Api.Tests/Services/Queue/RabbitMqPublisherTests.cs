using ClinicalIntelligence.Api.Configuration;
using ClinicalIntelligence.Api.Contracts;
using ClinicalIntelligence.Api.Services.Queue;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace ClinicalIntelligence.Api.Tests.Services.Queue;

public class RabbitMqPublisherTests
{
    [Fact]
    public void RabbitMqOptions_DefaultValues_AreCorrect()
    {
        // Arrange
        var options = new RabbitMqOptions();
        
        // Assert
        Assert.Equal("localhost", options.Host);
        Assert.Equal(5672, options.Port);
        Assert.Equal("guest", options.Username);
        Assert.Equal("guest", options.Password);
        Assert.Equal("/", options.VirtualHost);
        Assert.Equal("document-processing", options.DocumentProcessingQueue);
        Assert.Equal("document-processing-dlq", options.DeadLetterQueue);
        Assert.True(options.EnablePublisherConfirms);
        Assert.False(options.Enabled);
    }
    
    [Fact]
    public void RabbitMqOptions_CustomValues_AreApplied()
    {
        // Arrange
        var options = new RabbitMqOptions
        {
            Host = "rabbitmq.example.com",
            Port = 5673,
            Username = "admin",
            Password = "secret",
            VirtualHost = "/clinical",
            DocumentProcessingQueue = "custom-queue",
            EnablePublisherConfirms = false,
            Enabled = true
        };
        
        // Assert
        Assert.Equal("rabbitmq.example.com", options.Host);
        Assert.Equal(5673, options.Port);
        Assert.Equal("admin", options.Username);
        Assert.Equal("secret", options.Password);
        Assert.Equal("/clinical", options.VirtualHost);
        Assert.Equal("custom-queue", options.DocumentProcessingQueue);
        Assert.False(options.EnablePublisherConfirms);
        Assert.True(options.Enabled);
    }
    
    [Fact]
    public async Task PublishDocumentJobAsync_WhenDisabled_ReturnsTrue()
    {
        // Arrange
        var options = Options.Create(new RabbitMqOptions { Enabled = false });
        var logger = new Mock<ILogger<RabbitMqPublisher>>();
        var publisher = new RabbitMqPublisher(options, logger.Object);
        var job = CreateTestJob();
        
        // Act
        var result = await publisher.PublishDocumentJobAsync(job);
        
        // Assert
        Assert.True(result);
    }
    
    [Fact]
    public void IsConnected_WhenDisabled_ReturnsFalse()
    {
        // Arrange
        var options = Options.Create(new RabbitMqOptions { Enabled = false });
        var logger = new Mock<ILogger<RabbitMqPublisher>>();
        var publisher = new RabbitMqPublisher(options, logger.Object);
        
        // Assert
        Assert.False(publisher.IsConnected);
    }
    
    [Fact]
    public async Task PublishDocumentJobAsync_ValidJob_LogsMessage()
    {
        // Arrange
        var options = Options.Create(new RabbitMqOptions { Enabled = false });
        var logger = new Mock<ILogger<RabbitMqPublisher>>();
        var publisher = new RabbitMqPublisher(options, logger.Object);
        var job = CreateTestJob();
        
        // Act
        await publisher.PublishDocumentJobAsync(job);
        
        // Assert - verify logging occurred
        logger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains(job.JobId.ToString())),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }
    
    [Fact]
    public void Dispose_CanBeCalledMultipleTimes()
    {
        // Arrange
        var options = Options.Create(new RabbitMqOptions { Enabled = false });
        var logger = new Mock<ILogger<RabbitMqPublisher>>();
        var publisher = new RabbitMqPublisher(options, logger.Object);
        
        // Act & Assert - should not throw
        publisher.Dispose();
        publisher.Dispose();
    }
    
    private static DocumentProcessingJob CreateTestJob()
    {
        return new DocumentProcessingJob
        {
            JobId = Guid.NewGuid(),
            DocumentId = Guid.NewGuid(),
            PatientId = Guid.NewGuid(),
            UploadedByUserId = Guid.NewGuid(),
            OriginalName = "test-document.pdf",
            MimeType = "application/pdf",
            StoragePath = "test/path/document.pdf",
            SizeBytes = 1024,
            CreatedAt = DateTime.UtcNow,
            RetryCount = 0
        };
    }
}
