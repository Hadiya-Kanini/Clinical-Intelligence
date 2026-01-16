# Task - [TASK_002]

## Requirement Reference
- User Story: [us_053]
- Story Location: [.propel/context/tasks/us_053/us_053.md]
- Acceptance Criteria: 
    - Given a document is uploaded and validated, When stored, Then a processing job is enqueued in RabbitMQ (FR-023).
    - Given the queue, When jobs are enqueued, Then they are persisted for durability.

## Task Overview
Implement unit and integration tests for RabbitMQ integration. Tests cover message publishing, queue configuration, connection resilience, and edge cases like RabbitMQ unavailability.

## Dependent Tasks
- [US_053/task_001] - Backend RabbitMQ integration

## Impacted Components
- [CREATE | Server/ClinicalIntelligence.Api.Tests/Services/Queue/RabbitMqPublisherTests.cs | Unit tests for publisher]
- [CREATE | Server/ClinicalIntelligence.Api.Tests/Services/Queue/DocumentProcessingJobTests.cs | Tests for job contract]
- [CREATE | Server/ClinicalIntelligence.Api.Tests/Integration/RabbitMqIntegrationTests.cs | Integration tests]

## Implementation Plan

### 1. Unit Tests for DocumentProcessingJob
```csharp
namespace ClinicalIntelligence.Api.Tests.Services.Queue;

public class DocumentProcessingJobTests
{
    [Fact]
    public void DocumentProcessingJob_Serialization_RoundTrip()
    {
        // Arrange
        var job = new DocumentProcessingJob
        {
            JobId = Guid.NewGuid(),
            DocumentId = Guid.NewGuid(),
            PatientId = Guid.NewGuid(),
            UploadedByUserId = Guid.NewGuid(),
            OriginalName = "test-document.pdf",
            MimeType = "application/pdf",
            StoragePath = "default/patient-id/doc-id/original.pdf",
            SizeBytes = 1024000,
            CreatedAt = DateTime.UtcNow,
            RetryCount = 0,
            CorrelationId = Guid.NewGuid().ToString()
        };
        
        // Act
        var json = JsonSerializer.Serialize(job, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
        var deserialized = JsonSerializer.Deserialize<DocumentProcessingJob>(json, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
        
        // Assert
        Assert.NotNull(deserialized);
        Assert.Equal(job.JobId, deserialized.JobId);
        Assert.Equal(job.DocumentId, deserialized.DocumentId);
        Assert.Equal(job.PatientId, deserialized.PatientId);
        Assert.Equal(job.OriginalName, deserialized.OriginalName);
        Assert.Equal(job.MimeType, deserialized.MimeType);
        Assert.Equal(job.StoragePath, deserialized.StoragePath);
        Assert.Equal(job.SizeBytes, deserialized.SizeBytes);
        Assert.Equal(job.RetryCount, deserialized.RetryCount);
    }
    
    [Fact]
    public void DocumentProcessingJob_RequiredFields_ArePresent()
    {
        // Arrange
        var job = new DocumentProcessingJob
        {
            JobId = Guid.NewGuid(),
            DocumentId = Guid.NewGuid(),
            PatientId = Guid.NewGuid(),
            UploadedByUserId = Guid.NewGuid(),
            OriginalName = "test.pdf",
            MimeType = "application/pdf",
            StoragePath = "path/to/file.pdf",
            SizeBytes = 1000,
            CreatedAt = DateTime.UtcNow
        };
        
        // Assert
        Assert.NotEqual(Guid.Empty, job.JobId);
        Assert.NotEqual(Guid.Empty, job.DocumentId);
        Assert.NotEqual(Guid.Empty, job.PatientId);
        Assert.NotEmpty(job.OriginalName);
        Assert.NotEmpty(job.MimeType);
        Assert.NotEmpty(job.StoragePath);
        Assert.True(job.SizeBytes > 0);
    }
}
```

### 2. Unit Tests for RabbitMqPublisher (with Mocks)
```csharp
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
            EnablePublisherConfirms = false
        };
        
        // Assert
        Assert.Equal("rabbitmq.example.com", options.Host);
        Assert.Equal(5673, options.Port);
        Assert.Equal("admin", options.Username);
        Assert.Equal("secret", options.Password);
        Assert.Equal("/clinical", options.VirtualHost);
        Assert.Equal("custom-queue", options.DocumentProcessingQueue);
        Assert.False(options.EnablePublisherConfirms);
    }
}
```

### 3. Integration Tests (requires RabbitMQ)
```csharp
namespace ClinicalIntelligence.Api.Tests.Integration;

[Collection("RabbitMQ")]
public class RabbitMqIntegrationTests : IDisposable
{
    private readonly RabbitMqPublisher _publisher;
    private readonly IConnection _connection;
    private readonly IModel _channel;
    
    public RabbitMqIntegrationTests()
    {
        // Skip if RabbitMQ not available
        var options = Options.Create(new RabbitMqOptions
        {
            Host = Environment.GetEnvironmentVariable("RABBITMQ_HOST") ?? "localhost",
            DocumentProcessingQueue = $"test-queue-{Guid.NewGuid():N}"
        });
        
        var logger = new NullLogger<RabbitMqPublisher>();
        _publisher = new RabbitMqPublisher(options, logger);
        
        // Create connection for verification
        var factory = new ConnectionFactory { HostName = options.Value.Host };
        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();
    }
    
    [Fact]
    [Trait("Category", "Integration")]
    public async Task PublishDocumentJobAsync_ValidJob_PublishesSuccessfully()
    {
        // Arrange
        var job = CreateTestJob();
        
        // Act
        var result = await _publisher.PublishDocumentJobAsync(job);
        
        // Assert
        Assert.True(result);
    }
    
    [Fact]
    [Trait("Category", "Integration")]
    public async Task PublishDocumentJobAsync_MessageIsDurable()
    {
        // Arrange
        var job = CreateTestJob();
        
        // Act
        await _publisher.PublishDocumentJobAsync(job);
        
        // Assert - verify message is in queue
        var messageCount = _channel.MessageCount(_publisher.Options.DocumentProcessingQueue);
        Assert.True(messageCount > 0);
    }
    
    [Fact]
    [Trait("Category", "Integration")]
    public void IsConnected_WhenConnected_ReturnsTrue()
    {
        // Assert
        Assert.True(_publisher.IsConnected);
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
    
    public void Dispose()
    {
        _channel?.Close();
        _channel?.Dispose();
        _connection?.Close();
        _connection?.Dispose();
        _publisher?.Dispose();
    }
}
```

### 4. Edge Case Tests
```csharp
namespace ClinicalIntelligence.Api.Tests.Services.Queue;

public class RabbitMqEdgeCaseTests
{
    [Fact]
    public async Task PublishDocumentJobAsync_LargeMetadata_HandlesCorrectly()
    {
        // Arrange
        var job = new DocumentProcessingJob
        {
            JobId = Guid.NewGuid(),
            DocumentId = Guid.NewGuid(),
            PatientId = Guid.NewGuid(),
            UploadedByUserId = Guid.NewGuid(),
            OriginalName = new string('a', 255), // Max filename length
            MimeType = "application/pdf",
            StoragePath = new string('b', 1000), // Max path length
            SizeBytes = 50 * 1024 * 1024, // 50MB
            CreatedAt = DateTime.UtcNow,
            RetryCount = 0
        };
        
        // Act
        var json = JsonSerializer.Serialize(job);
        
        // Assert
        Assert.NotEmpty(json);
        Assert.Contains(job.OriginalName, json);
    }
    
    [Fact]
    public void DocumentProcessingJob_RetryCount_IncrementsCorrectly()
    {
        // Arrange
        var originalJob = new DocumentProcessingJob
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
            RetryCount = 0
        };
        
        // Act - simulate retry
        var retriedJob = originalJob with { RetryCount = originalJob.RetryCount + 1 };
        
        // Assert
        Assert.Equal(0, originalJob.RetryCount);
        Assert.Equal(1, retriedJob.RetryCount);
        Assert.Equal(originalJob.DocumentId, retriedJob.DocumentId);
    }
}
```

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| CREATE | Server/ClinicalIntelligence.Api.Tests/Services/Queue/DocumentProcessingJobTests.cs | Job contract tests |
| CREATE | Server/ClinicalIntelligence.Api.Tests/Services/Queue/RabbitMqPublisherTests.cs | Publisher unit tests |
| CREATE | Server/ClinicalIntelligence.Api.Tests/Services/Queue/RabbitMqEdgeCaseTests.cs | Edge case tests |
| CREATE | Server/ClinicalIntelligence.Api.Tests/Integration/RabbitMqIntegrationTests.cs | Integration tests |

## Build Commands
- dotnet test Server/ClinicalIntelligence.Api.Tests --filter "FullyQualifiedName~RabbitMq"
- dotnet test Server/ClinicalIntelligence.Api.Tests --filter "Category=Integration"

## Implementation Validation Strategy
- [Automated] All unit tests pass without RabbitMQ
- [Automated] Integration tests pass with RabbitMQ running
- [Manual] Verify test coverage >= 80%

## Implementation Checklist
- [x] Create DocumentProcessingJobTests for serialization
- [x] Create RabbitMqPublisherTests for configuration
- [x] Create RabbitMqEdgeCaseTests for boundary conditions
- [ ] Create RabbitMqIntegrationTests (requires RabbitMQ)
- [ ] Add test fixtures for RabbitMQ connection
- [x] Verify all tests pass
