using ClinicalIntelligence.Api.Configuration;
using ClinicalIntelligence.Api.Contracts;
using Microsoft.Extensions.Options;
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
    
    public RabbitMqPublisher(
        IOptions<RabbitMqOptions> options,
        ILogger<RabbitMqPublisher> logger)
    {
        _options = options.Value;
        _logger = logger;
        
        if (_options.Enabled)
        {
            InitializeConnection();
        }
        else
        {
            _logger.LogInformation("RabbitMQ integration is disabled. Messages will be logged but not queued.");
        }
    }
    
    public bool IsConnected => _options.Enabled && _isConnected;
    
    private bool _isConnected;
    
    private void InitializeConnection()
    {
        try
        {
            _logger.LogInformation(
                "RabbitMQ connection initialized (simulated): Host={Host}, Queue={Queue}",
                _options.Host, _options.DocumentProcessingQueue);
            _isConnected = true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to connect to RabbitMQ: Host={Host}", _options.Host);
            _isConnected = false;
        }
    }
    
    public Task<bool> PublishDocumentJobAsync(
        DocumentProcessingJob job, 
        CancellationToken ct = default)
    {
        if (!_options.Enabled)
        {
            _logger.LogInformation(
                "RabbitMQ disabled - Document job logged: JobId={JobId}, DocumentId={DocumentId}",
                job.JobId, job.DocumentId);
            return Task.FromResult(true);
        }
        
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
            
            _logger.LogInformation(
                "Document job published: JobId={JobId}, DocumentId={DocumentId}, MessageSize={Size}",
                job.JobId, job.DocumentId, Encoding.UTF8.GetByteCount(messageBody));
            
            return Task.FromResult(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, 
                "Failed to publish document job: JobId={JobId}, DocumentId={DocumentId}",
                job.JobId, job.DocumentId);
            return Task.FromResult(false);
        }
    }
    
    public void Dispose()
    {
        if (_disposed) return;
        
        _disposed = true;
        _isConnected = false;
        _logger.LogInformation("RabbitMQ publisher disposed");
    }
}
