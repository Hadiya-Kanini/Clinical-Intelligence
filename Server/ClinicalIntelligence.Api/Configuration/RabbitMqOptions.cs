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
    
    /// <summary>
    /// Enable RabbitMQ integration. Default: false (disabled until configured).
    /// </summary>
    public bool Enabled { get; set; } = false;
}
