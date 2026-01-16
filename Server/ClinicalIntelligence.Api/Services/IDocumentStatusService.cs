using ClinicalIntelligence.Api.Domain.Enums;

namespace ClinicalIntelligence.Api.Services;

/// <summary>
/// Service interface for document status operations per FR-020.
/// </summary>
public interface IDocumentStatusService
{
    /// <summary>
    /// Gets the current status of a document.
    /// </summary>
    Task<DocumentStatusResult?> GetStatusAsync(Guid documentId, CancellationToken ct = default);
    
    /// <summary>
    /// Updates document status with transition validation.
    /// </summary>
    Task<DocumentStatusResult> UpdateStatusAsync(
        Guid documentId, 
        DocumentStatus newStatus, 
        string? errorMessage = null,
        CancellationToken ct = default);
    
    /// <summary>
    /// Gets status for multiple documents (batch query).
    /// </summary>
    Task<IReadOnlyList<DocumentStatusResult>> GetStatusBatchAsync(
        IEnumerable<Guid> documentIds, 
        CancellationToken ct = default);
    
    /// <summary>
    /// Checks if a status transition is valid.
    /// </summary>
    bool IsValidTransition(DocumentStatus current, DocumentStatus target);
}

/// <summary>
/// Result of a document status query.
/// </summary>
public record DocumentStatusResult
{
    public Guid DocumentId { get; init; }
    public string Status { get; init; } = string.Empty;
    public DateTime? StatusChangedAt { get; init; }
    public string? ErrorMessage { get; init; }
    public int? ProcessingTimeMs { get; init; }
}
