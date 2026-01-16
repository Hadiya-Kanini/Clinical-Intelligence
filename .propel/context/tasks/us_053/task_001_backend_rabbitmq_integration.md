# Task - [TASK_001]

## Requirement Reference
- User Story: [us_053]
- Story Location: [.propel/context/tasks/us_053/us_053.md]
- Acceptance Criteria: 
    - Given a document is uploaded and validated, When stored, Then a processing job is enqueued in RabbitMQ (FR-023).
    - Given the job message, When created, Then it includes document_id, patient_id, and required metadata.
    - Given asynchronous processing, When jobs are queued, Then the user interface is not blocked (FR-024).
    - Given the queue, When jobs are enqueued, Then they are persisted for durability.

## Task Overview
Implement RabbitMQ integration for asynchronous document processing. This includes connection management, message publishing, and queue configuration with durable queues for reliability. The service publishes processing jobs after successful document upload.

## Dependent Tasks
- [US_050/task_001] - Backend document storage service
- [US_051/task_001] - Backend document status service

## Impacted Components
- [CREATE | Server/ClinicalIntelligence.Api/Configuration/RabbitMqOptions.cs | RabbitMQ configuration options]
- [CREATE | Server/ClinicalIntelligence.Api/Services/Queue/IMessagePublisher.cs | Message publishing interface]
- [CREATE | Server/ClinicalIntelligence.Api/Services/Queue/RabbitMqPublisher.cs | RabbitMQ publisher implementation]
- [CREATE | Server/ClinicalIntelligence.Api/Contracts/DocumentProcessingJob.cs | Job message contract]
- [MODIFY | Server/ClinicalIntelligence.Api/Services/DocumentService.cs | Enqueue job after upload]
- [MODIFY | Server/ClinicalIntelligence.Api/Program.cs | Register RabbitMQ services]

## Implementation Plan

### 1. Create RabbitMQ Configuration Options
```csharp
namespace ClinicalIntelligence.Api.Configuration;

/// <summary>
/// RabbitMQ connection and queue configuration.
/// </summary>
public class RabbitMqOptions
{
    public const string SectionName = "RabbitMq";
    
    /// <summary>
    /// RabbitMQ host. Default: localhost.
    /// </summary>
    public string Host { get; set; } = "localhost";
    
    /// <summary>
    /// RabbitMQ port. Default: 5672.
    /// </summary>
    public int Port { get; set; } = 5672;
    
    /// <summary>
    /// RabbitMQ username. Default: guest.
    /// </summary>
    public string Username { get; set; } = "guest";
    
    /// <summary>
    /// RabbitMQ password. Default: guest.
    /// </summary>
    public string Password { get; set; } = "guest";
    
    /// <summary>
    /// Virtual host. Default: /.
    /// </summary>
    public string VirtualHost { get; set; } = "/";
    
    /// <summary>
    /// Document processing queue name.
    /// </summary>
    public string DocumentProcessingQueue { get; set; } = "document-processing";
    
    /// <summary>
    /// Dead letter queue name for failed jobs.
    /// </summary>
    public string DeadLetterQueue { get; set; } = "document-processing-dlq";
    
    /// <summary>
    /// Exchange name for document processing.
    /// </summary>
    public string ExchangeName { get; set; } = "clinical-intelligence";
    
    /// <summary>
    /// Enable publisher confirms for reliability.
    /// </summary>
    public bool EnablePublisherConfirms { get; set; } = true;
    
    /// <summary>
    /// Connection retry count.
    /// </summary>
    public int RetryCount { get; set; } = 3;
    
    /// <summary>
    /// Connection retry delay in milliseconds.
    /// </summary>
    public int RetryDelayMs { get; set; } = 1000;
}
```

### 2. Create Job Message Contract
```csharp
namespace ClinicalIntelligence.Api.Contracts;

/// <summary>
/// Document processing job message for RabbitMQ (FR-023).
/// </summary>
public record DocumentProcessingJob
{
    /// <summary>
    /// Unique job identifier.
    /// </summary>
    public Guid JobId { get; init; }
    
    /// <summary>
    /// Document to process.
    /// </summary>
    public Guid DocumentId { get; init; }
    
    /// <summary>
    /// Patient associated with document.
    /// </summary>
    public Guid PatientId { get; init; }
    
    /// <summary>
    /// User who uploaded the document.
    /// </summary>
    public Guid UploadedByUserId { get; init; }
    
    /// <summary>
    /// Original file name.
    /// </summary>
    public string OriginalName { get; init; } = string.Empty;
    
    /// <summary>
    /// MIME type of document.
    /// </summary>
    public string MimeType { get; init; } = string.Empty;
    
    /// <summary>
    /// Storage path for document retrieval.
    /// </summary>
    public string StoragePath { get; init; } = string.Empty;
    
    /// <summary>
    /// File size in bytes.
    /// </summary>
    public long SizeBytes { get; init; }
    
    /// <summary>
    /// Job creation timestamp.
    /// </summary>
    public DateTime CreatedAt { get; init; }
    
    /// <summary>
    /// Current retry attempt (0 = first attempt).
    /// </summary>
    public int RetryCount { get; init; }
    
    /// <summary>
    /// Correlation ID for tracing.
    /// </summary>
    public string? CorrelationId { get; init; }
}
```

### 3. Create Message Publisher Interface
```csharp
namespace ClinicalIntelligence.Api.Services.Queue;

/// <summary>
/// Interface for publishing messages to queue.
/// </summary>
public interface IMessagePublisher
{
    /// <summary>
    /// Publishes a document processing job to the queue.
    /// </summary>
    Task<bool> PublishDocumentJobAsync(
        DocumentProcessingJob job, 
        CancellationToken ct = default);
    
    /// <summary>
    /// Checks if the queue connection is healthy.
    /// </summary>
    bool IsConnected { get; }
}
```

### 4. Implement RabbitMQ Publisher
```csharp
namespace ClinicalIntelligence.Api.Services.Queue;

using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

/// <summary>
/// RabbitMQ message publisher with durable queues (FR-023, FR-024).
/// </summary>
public class RabbitMqPublisher : IMessagePublisher, IDisposable
{
    private readonly RabbitMqOptions _options;
    private readonly ILogger<RabbitMqPublisher> _logger;
    private IConnection? _connection;
    private IModel? _channel;
    private readonly object _lock = new();
    
    public RabbitMqPublisher(
        IOptions<RabbitMqOptions> options,
        ILogger<RabbitMqPublisher> logger)
    {
        _options = options.Value;
        _logger = logger;
        InitializeConnection();
    }
    
    public bool IsConnected => _connection?.IsOpen == true && _channel?.IsOpen == true;
    
    private void InitializeConnection()
    {
        try
        {
            var factory = new ConnectionFactory
            {
                HostName = _options.Host,
                Port = _options.Port,
                UserName = _options.Username,
                Password = _options.Password,
                VirtualHost = _options.VirtualHost,
                AutomaticRecoveryEnabled = true,
                NetworkRecoveryInterval = TimeSpan.FromSeconds(10)
            };
            
            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();
            
            // Enable publisher confirms for reliability
            if (_options.EnablePublisherConfirms)
            {
                _channel.ConfirmSelect();
            }
            
            // Declare exchange
            _channel.ExchangeDeclare(
                exchange: _options.ExchangeName,
                type: ExchangeType.Direct,
                durable: true,
                autoDelete: false);
            
            // Declare dead letter exchange
            _channel.ExchangeDeclare(
                exchange: $"{_options.ExchangeName}-dlx",
                type: ExchangeType.Direct,
                durable: true,
                autoDelete: false);
            
            // Declare dead letter queue
            _channel.QueueDeclare(
                queue: _options.DeadLetterQueue,
                durable: true,
                exclusive: false,
                autoDelete: false);
            
            _channel.QueueBind(
                queue: _options.DeadLetterQueue,
                exchange: $"{_options.ExchangeName}-dlx",
                routingKey: _options.DocumentProcessingQueue);
            
            // Declare main processing queue with DLX
            var queueArgs = new Dictionary<string, object>
            {
                { "x-dead-letter-exchange", $"{_options.ExchangeName}-dlx" },
                { "x-dead-letter-routing-key", _options.DocumentProcessingQueue }
            };
            
            _channel.QueueDeclare(
                queue: _options.DocumentProcessingQueue,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: queueArgs);
            
            _channel.QueueBind(
                queue: _options.DocumentProcessingQueue,
                exchange: _options.ExchangeName,
                routingKey: _options.DocumentProcessingQueue);
            
            _logger.LogInformation(
                "RabbitMQ connection established: Host={Host}, Queue={Queue}",
                _options.Host, _options.DocumentProcessingQueue);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to connect to RabbitMQ: Host={Host}", _options.Host);
            throw;
        }
    }
    
    public async Task<bool> PublishDocumentJobAsync(
        DocumentProcessingJob job, 
        CancellationToken ct = default)
    {
        if (!IsConnected)
        {
            _logger.LogWarning("RabbitMQ not connected, attempting reconnection");
            lock (_lock)
            {
                if (!IsConnected)
                {
                    InitializeConnection();
                }
            }
        }
        
        try
        {
            var messageBody = JsonSerializer.Serialize(job, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });
            
            var body = Encoding.UTF8.GetBytes(messageBody);
            
            var properties = _channel!.CreateBasicProperties();
            properties.Persistent = true; // Durable message
            properties.ContentType = "application/json";
            properties.MessageId = job.JobId.ToString();
            properties.CorrelationId = job.CorrelationId ?? Guid.NewGuid().ToString();
            properties.Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds());
            properties.Headers = new Dictionary<string, object>
            {
                { "document-id", job.DocumentId.ToString() },
                { "retry-count", job.RetryCount }
            };
            
            _channel.BasicPublish(
                exchange: _options.ExchangeName,
                routingKey: _options.DocumentProcessingQueue,
                basicProperties: properties,
                body: body);
            
            // Wait for publisher confirm
            if (_options.EnablePublisherConfirms)
            {
                _channel.WaitForConfirmsOrDie(TimeSpan.FromSeconds(5));
            }
            
            _logger.LogInformation(
                "Document job published: JobId={JobId}, DocumentId={DocumentId}",
                job.JobId, job.DocumentId);
            
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, 
                "Failed to publish document job: JobId={JobId}, DocumentId={DocumentId}",
                job.JobId, job.DocumentId);
            return false;
        }
    }
    
    public void Dispose()
    {
        _channel?.Close();
        _channel?.Dispose();
        _connection?.Close();
        _connection?.Dispose();
    }
}
```

### 5. Update DocumentService to Enqueue Jobs
```csharp
// Add to DocumentService constructor
private readonly IMessagePublisher? _messagePublisher;

// Add after successful document storage in ValidateAndAcknowledgeAsync
if (isValid && _messagePublisher != null)
{
    var job = new DocumentProcessingJob
    {
        JobId = Guid.NewGuid(),
        DocumentId = documentId,
        PatientId = patientId,
        UploadedByUserId = uploadedByUserId,
        OriginalName = fileName,
        MimeType = contentType,
        StoragePath = storagePath,
        SizeBytes = fileSize,
        CreatedAt = DateTime.UtcNow,
        RetryCount = 0
    };
    
    var published = await _messagePublisher.PublishDocumentJobAsync(job, cancellationToken);
    
    if (!published)
    {
        _logger.LogWarning(
            "Failed to enqueue processing job, document will need manual processing: DocumentId={DocumentId}",
            documentId);
    }
}
```

### 6. Register Services in Program.cs
```csharp
// Add RabbitMQ configuration
builder.Services.Configure<RabbitMqOptions>(
    builder.Configuration.GetSection(RabbitMqOptions.SectionName));

// Register RabbitMQ publisher
builder.Services.AddSingleton<IMessagePublisher, RabbitMqPublisher>();

// Add health check for RabbitMQ
builder.Services.AddHealthChecks()
    .AddRabbitMQ(
        rabbitConnectionString: $"amqp://{rabbitMqOptions.Username}:{rabbitMqOptions.Password}@{rabbitMqOptions.Host}:{rabbitMqOptions.Port}/{rabbitMqOptions.VirtualHost}",
        name: "rabbitmq");
```

## Current Project State
```
Server/ClinicalIntelligence.Api/
├── Configuration/
│   └── (existing options)
├── Services/
│   └── DocumentService.cs    # Needs IMessagePublisher integration
├── Contracts/
│   └── (existing contracts)
```

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| CREATE | Server/ClinicalIntelligence.Api/Configuration/RabbitMqOptions.cs | RabbitMQ configuration |
| CREATE | Server/ClinicalIntelligence.Api/Contracts/DocumentProcessingJob.cs | Job message contract |
| CREATE | Server/ClinicalIntelligence.Api/Services/Queue/IMessagePublisher.cs | Publisher interface |
| CREATE | Server/ClinicalIntelligence.Api/Services/Queue/RabbitMqPublisher.cs | RabbitMQ implementation |
| MODIFY | Server/ClinicalIntelligence.Api/Services/DocumentService.cs | Enqueue job after upload |
| MODIFY | Server/ClinicalIntelligence.Api/Program.cs | Register services |
| MODIFY | Server/ClinicalIntelligence.Api/ClinicalIntelligence.Api.csproj | Add RabbitMQ.Client package |

## External References
- https://www.rabbitmq.com/docs/dotnet-api-guide
- https://www.rabbitmq.com/docs/dlx
- https://www.rabbitmq.com/docs/confirms

## Build Commands
- dotnet add Server/ClinicalIntelligence.Api package RabbitMQ.Client
- dotnet add Server/ClinicalIntelligence.Api package AspNetCore.HealthChecks.Rabbitmq
- dotnet build Server/ClinicalIntelligence.Api

## Implementation Validation Strategy
- [Automated] Unit tests verify message serialization
- [Automated] Integration tests verify queue connection
- [Manual] Verify messages appear in RabbitMQ management UI
- [Manual] Verify dead letter queue receives failed messages

## Implementation Checklist
- [x] Add RabbitMQ.Client NuGet package
- [x] Create RabbitMqOptions configuration class
- [x] Create DocumentProcessingJob contract
- [x] Create IMessagePublisher interface
- [x] Implement RabbitMqPublisher with durable queues
- [x] Configure dead letter exchange and queue
- [ ] Update DocumentService to publish jobs
- [x] Register services in Program.cs
- [ ] Add RabbitMQ health check
- [x] Add appsettings.json configuration section
- [x] Add unit tests for publisher
