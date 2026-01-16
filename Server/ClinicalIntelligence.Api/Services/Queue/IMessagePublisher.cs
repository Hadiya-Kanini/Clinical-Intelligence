using ClinicalIntelligence.Api.Contracts;

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
