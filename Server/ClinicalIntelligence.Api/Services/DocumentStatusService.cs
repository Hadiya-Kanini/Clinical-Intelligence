using ClinicalIntelligence.Api.Data;
using ClinicalIntelligence.Api.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace ClinicalIntelligence.Api.Services;

/// <summary>
/// Service for managing document processing status transitions per FR-020.
/// </summary>
public class DocumentStatusService : IDocumentStatusService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<DocumentStatusService> _logger;
    
    /// <summary>
    /// Valid status transitions per FR-020.
    /// </summary>
    private static readonly Dictionary<DocumentStatus, HashSet<DocumentStatus>> ValidTransitions = new()
    {
        { DocumentStatus.Pending, new() { DocumentStatus.Processing, DocumentStatus.Failed, DocumentStatus.ValidationFailed } },
        { DocumentStatus.Processing, new() { DocumentStatus.Completed, DocumentStatus.Failed, DocumentStatus.ValidationFailed } },
        { DocumentStatus.Completed, new() { } }, // Terminal state
        { DocumentStatus.Failed, new() { DocumentStatus.Pending } }, // Allow retry
        { DocumentStatus.ValidationFailed, new() { } } // Terminal state
    };
    
    public DocumentStatusService(ApplicationDbContext dbContext, ILogger<DocumentStatusService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }
    
    public async Task<DocumentStatusResult?> GetStatusAsync(Guid documentId, CancellationToken ct = default)
    {
        var document = await _dbContext.Documents
            .AsNoTracking()
            .Where(d => d.Id == documentId && !d.IsDeleted)
            .Select(d => new DocumentStatusResult
            {
                DocumentId = d.Id,
                Status = d.Status,
                StatusChangedAt = d.UploadedAt,
                ErrorMessage = null
            })
            .FirstOrDefaultAsync(ct);
        
        return document;
    }
    
    public async Task<DocumentStatusResult> UpdateStatusAsync(
        Guid documentId, 
        DocumentStatus newStatus, 
        string? errorMessage = null,
        CancellationToken ct = default)
    {
        var document = await _dbContext.Documents
            .FirstOrDefaultAsync(d => d.Id == documentId && !d.IsDeleted, ct);
        
        if (document == null)
        {
            throw new InvalidOperationException($"Document {documentId} not found.");
        }
        
        var currentStatus = Enum.Parse<DocumentStatus>(document.Status, ignoreCase: true);
        
        if (!IsValidTransition(currentStatus, newStatus))
        {
            _logger.LogWarning(
                "Invalid status transition attempted: DocumentId={DocumentId}, Current={Current}, Target={Target}",
                documentId, currentStatus, newStatus);
            throw new InvalidOperationException(
                $"Invalid status transition from {currentStatus} to {newStatus}.");
        }
        
        document.Status = newStatus.ToString();
        
        await _dbContext.SaveChangesAsync(ct);
        
        _logger.LogInformation(
            "Document status updated: DocumentId={DocumentId}, OldStatus={OldStatus}, NewStatus={NewStatus}",
            documentId, currentStatus, newStatus);
        
        return new DocumentStatusResult
        {
            DocumentId = documentId,
            Status = document.Status,
            StatusChangedAt = DateTime.UtcNow,
            ErrorMessage = errorMessage
        };
    }
    
    public async Task<IReadOnlyList<DocumentStatusResult>> GetStatusBatchAsync(
        IEnumerable<Guid> documentIds, 
        CancellationToken ct = default)
    {
        var ids = documentIds.ToList();
        
        return await _dbContext.Documents
            .AsNoTracking()
            .Where(d => ids.Contains(d.Id) && !d.IsDeleted)
            .Select(d => new DocumentStatusResult
            {
                DocumentId = d.Id,
                Status = d.Status,
                StatusChangedAt = d.UploadedAt
            })
            .ToListAsync(ct);
    }
    
    public bool IsValidTransition(DocumentStatus current, DocumentStatus target)
    {
        return ValidTransitions.TryGetValue(current, out var allowed) && allowed.Contains(target);
    }
}
