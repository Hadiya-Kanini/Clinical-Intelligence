using ClinicalIntelligence.Api.Configuration;
using ClinicalIntelligence.Api.Contracts;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace ClinicalIntelligence.Api.Services.Queue;

/// <summary>
/// RabbitMQ message publisher with durable queues (FR-023, FR-024).
/// Uses abstraction to allow graceful degradation when RabbitMQ is not available.
/// </summary>
public class RabbitMqPublisher : IMessagePublisher, IDisposable
{
    private readonly RabbitMqOptions _options;
    private readonly ILogger<RabbitMqPublisher> _logger;
    private readonly object _lock = new();
    private bool _disposed;
    private IConnection? _connection;
    private IChannel? _channel;
    
    public RabbitMqPublisher(
        IOptions<RabbitMqOptions> options,
        ILogger<RabbitMqPublisher> logger)
    {
        _options = options.Value;
        _logger = logger;
        
        _logger.LogWarning("[RABBITMQ] ========== PUBLISHER CREATED ==========");
        _logger.LogWarning("[RABBITMQ] Enabled: {Enabled}, Host: {Host}:{Port}, Queue: {Queue}",
            _options.Enabled, _options.Host, _options.Port, _options.DocumentProcessingQueue);
        
        if (!_options.Enabled)
        {
            _logger.LogInformation("RabbitMQ integration is disabled. Messages will be logged but not queued.");
        }
        else
        {
            _logger.LogInformation("RabbitMQ publisher initialized. Establishing connection at startup...");
            // Establish connection at startup instead of lazily
            Task.Run(async () =>
            {
                try
                {
                    await InitializeConnectionAsync();
                    _logger.LogWarning("[RABBITMQ] ✅ Startup connection established successfully");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "[RABBITMQ] ❌ Failed to establish startup connection");
                }
            }).Wait(TimeSpan.FromSeconds(10)); // Wait up to 10 seconds for connection
        }
    }
    
    public bool IsConnected => _options.Enabled && _isConnected;
    
    private bool _isConnected;
    
    private async Task InitializeConnectionAsync()
    {
        _logger.LogWarning("[RABBITMQ] Attempting connection: Host={Host}:{Port}, VHost={VHost}, Queue={Queue}",
            _options.Host, _options.Port, _options.VirtualHost, _options.DocumentProcessingQueue);
        
        try
        {
            var factory = new ConnectionFactory
            {
                HostName = _options.Host,
                Port = _options.Port,
                UserName = _options.Username,
                Password = _options.Password,
                VirtualHost = _options.VirtualHost
            };
            
            _logger.LogWarning("[RABBITMQ] Creating connection...");
            _connection = await factory.CreateConnectionAsync();
            _logger.LogWarning("[RABBITMQ] ✅ Connection created");
            
            _logger.LogWarning("[RABBITMQ] Creating channel...");
            _channel = await _connection.CreateChannelAsync();
            _logger.LogWarning("[RABBITMQ] ✅ Channel created");
            
            _logger.LogWarning("[RABBITMQ] Declaring queue: {Queue}", _options.DocumentProcessingQueue);
            var queueArguments = new Dictionary<string, object>
            {
                { "x-message-ttl", 3600000 }
            };
            
            await _channel.QueueDeclareAsync(
                queue: _options.DocumentProcessingQueue,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: queueArguments);
            _logger.LogWarning("[RABBITMQ] ✅ Queue declared");
            
            _logger.LogWarning("[RABBITMQ] ✅ CONNECTION ESTABLISHED: {Host}:{Port}", _options.Host, _options.Port);
            _isConnected = true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[RABBITMQ] ❌ CONNECTION FAILED: {Message}", ex.Message);
            _isConnected = false;
        }
    }
    
    public async Task<bool> PublishDocumentJobAsync(
        DocumentProcessingJob job, 
        CancellationToken ct = default)
    {
        _logger.LogWarning("[RABBITMQ] PublishDocumentJobAsync called: JobId={JobId}, DocumentId={DocumentId}", job.JobId, job.DocumentId);
        
        if (!_options.Enabled)
        {
            _logger.LogWarning("[RABBITMQ] ❌ RabbitMQ disabled - NOT publishing");
            return false;
        }
        
        _logger.LogWarning("[RABBITMQ] Status: Enabled={Enabled}, IsConnected={IsConnected}", _options.Enabled, IsConnected);
        
        if (!IsConnected)
        {
            _logger.LogWarning("[RABBITMQ] 🔌 Not connected, attempting connection...");
            try
            {
                await InitializeConnectionAsync();
                _logger.LogWarning("[RABBITMQ] Connection attempt completed. IsConnected={IsConnected}", IsConnected);
            }
            catch (Exception connEx)
            {
                _logger.LogError(connEx, "[RABBITMQ] ❌ Exception during connection initialization");
            }
            
            if (!IsConnected)
            {
                _logger.LogWarning("[RABBITMQ] ❌ Connection failed. Cannot publish job.");
                return false;
            }
        }
        else
        {
            _logger.LogWarning("[RABBITMQ] ✅ Already connected, proceeding to publish");
        }
        
        try
        {
            if (_channel == null)
            {
                _logger.LogWarning("[RABBITMQ] ❌ Channel is null!");
                return false;
            }
            
            _logger.LogWarning("[RABBITMQ] Building job payload...");
            
            // Create job in worker-expected format (matches job.schema.json)
            var payload = new Dictionary<string, object?>
            {
                ["storage_path"] = job.StoragePath,
                ["mime_type"] = job.MimeType,
                ["document_id"] = job.DocumentId.ToString()
            };
            
            if (job.PatientId.HasValue)
            {
                payload["patient_id"] = job.PatientId.Value.ToString();
            }
            
            var workerJob = new
            {
                schema_version = "1.0",
                job_id = job.JobId.ToString(),
                document_id = job.DocumentId.ToString(),
                status = "pending",
                payload
            };
            
            var messageBody = JsonSerializer.Serialize(workerJob);
            var body = Encoding.UTF8.GetBytes(messageBody);
            
            _logger.LogWarning("[RABBITMQ] Message size: {Size} bytes, Queue: {Queue}", body.Length, _options.DocumentProcessingQueue);
            
            var properties = new BasicProperties
            {
                Persistent = true,
                ContentType = "application/json"
            };
            
            try
            {
                await _channel.BasicPublishAsync(
                    exchange: "",
                    routingKey: _options.DocumentProcessingQueue,
                    mandatory: false,
                    basicProperties: properties,
                    body: body);
                
                _logger.LogWarning("[RABBITMQ] ✅ JOB PUBLISHED SUCCESSFULLY! JobId={JobId}", job.JobId);
                return true;
            }
            catch (Exception publishEx)
            {
                _logger.LogError(publishEx, "[RABBITMQ] ❌ BasicPublishAsync FAILED");
                throw;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[RABBITMQ] ❌ PUBLISH ERROR");
            return false;
        }
    }
    
    public async Task<bool> TestConnectionAsync()
    {
        if (!_options.Enabled)
        {
            _logger.LogWarning("RabbitMQ is disabled");
            return false;
        }
        
        if (!IsConnected)
        {
            await InitializeConnectionAsync();
        }
        
        return IsConnected;
    }
    
    public void Dispose()
    {
        if (_disposed) return;
        
        _disposed = true;
        _isConnected = false;
        
        try
        {
            _channel?.CloseAsync().GetAwaiter().GetResult();
            _channel?.Dispose();
            _connection?.CloseAsync().GetAwaiter().GetResult();
            _connection?.Dispose();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error disposing RabbitMQ connection");
        }
        
        _logger.LogInformation("RabbitMQ publisher disposed");
    }
}
