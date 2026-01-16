# Task - [TASK_001]

## Requirement Reference
- User Story: [us_051]
- Story Location: [.propel/context/tasks/us_051/us_051.md]
- Acceptance Criteria: 
    - Given a document is uploaded, When processing begins, Then status transitions: Pending → Processing → Completed/Failed (FR-020).
    - Given status changes, When they occur, Then the database is updated with current status.
    - Given the document list, When displayed, Then each document shows its current status.
    - Given a failed document, When status is Failed, Then the status includes Validation_Failed for validation errors (TR-004).

## Task Overview
Implement a document processing status service that manages status transitions for uploaded documents. The service tracks document lifecycle states (Pending, Processing, Completed, Failed, Validation_Failed) and provides APIs for status updates and retrieval. This builds on the existing `Document` and `ProcessingJob` models.

## Dependent Tasks
- [US_050/task_001] - Backend document storage service (provides document persistence)

## Impacted Components
- [CREATE | Server/ClinicalIntelligence.Api/Services/IDocumentStatusService.cs | Interface for document status operations]
- [CREATE | Server/ClinicalIntelligence.Api/Services/DocumentStatusService.cs | Status management implementation]
- [CREATE | Server/ClinicalIntelligence.Api/Domain/Enums/DocumentStatus.cs | Enum for document status values]
- [MODIFY | Server/ClinicalIntelligence.Api/Domain/Models/Document.cs | Add status transition timestamp fields]
- [MODIFY | Server/ClinicalIntelligence.Api/Program.cs | Register status service and add status endpoints]

## Implementation Plan

### 1. Create Document Status Enum
```csharp
namespace ClinicalIntelligence.Api.Domain.Enums;

/// <summary>
/// Document processing status values per FR-020 and TR-004.
/// </summary>
public enum DocumentStatus
{
    /// <summary>
    /// Document uploaded, awaiting processing.
    /// </summary>
    Pending,
    
    /// <summary>
    /// Document is being processed by AI worker.
    /// </summary>
    Processing,
    
    /// <summary>
    /// Processing completed successfully.
    /// </summary>
    Completed,
    
    /// <summary>
    /// Processing failed due to system error.
    /// </summary>
    Failed,
    
    /// <summary>
    /// Processing failed due to validation errors (TR-004).
    /// </summary>
    ValidationFailed
}
```

### 2. Create Document Status Service Interface
```csharp
namespace ClinicalIntelligence.Api.Services;

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

public record DocumentStatusResult
{
    public Guid DocumentId { get; init; }
    public string Status { get; init; } = string.Empty;
    public DateTime? StatusChangedAt { get; init; }
    public string? ErrorMessage { get; init; }
    public int? ProcessingTimeMs { get; init; }
}
```

### 3. Implement Document Status Service
```csharp
namespace ClinicalIntelligence.Api.Services;

public class DocumentStatusService : IDocumentStatusService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<DocumentStatusService> _logger;
    
    // Valid status transitions per FR-020
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
                StatusChangedAt = d.UploadedAt, // TODO: Add StatusChangedAt field
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
```

### 4. Add Status Endpoint
```csharp
// GET /api/v1/documents/{documentId}/status
v1.MapGet("/documents/{documentId}/status", async (
    Guid documentId,
    HttpContext context,
    IDocumentStatusService statusService) =>
{
    var result = await statusService.GetStatusAsync(documentId, context.RequestAborted);
    
    if (result == null)
    {
        return ApiErrorResults.NotFound("document_not_found", "Document not found.");
    }
    
    return Results.Ok(result);
})
    .RequireAuthorization()
    .WithName("GetDocumentStatus")
    .WithOpenApi(operation => new(operation)
    {
        Summary = "Get document processing status",
        Description = "Returns the current processing status of a document."
    });
```

## Current Project State
```
Server/ClinicalIntelligence.Api/
├── Domain/Models/
│   ├── Document.cs           # Has Status property (string)
│   └── ProcessingJob.cs      # Has Status, RetryCount, ErrorMessage
├── Services/
│   └── DocumentService.cs    # Sets initial status to "Pending"
```

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| CREATE | Server/ClinicalIntelligence.Api/Domain/Enums/DocumentStatus.cs | Enum for document status values |
| CREATE | Server/ClinicalIntelligence.Api/Services/IDocumentStatusService.cs | Interface for status operations |
| CREATE | Server/ClinicalIntelligence.Api/Services/DocumentStatusService.cs | Status management implementation |
| MODIFY | Server/ClinicalIntelligence.Api/Program.cs | Register service, add status endpoint |

## External References
- https://learn.microsoft.com/en-us/dotnet/architecture/microservices/microservice-ddd-cqrs-patterns/

## Build Commands
- dotnet build Server/ClinicalIntelligence.Api
- dotnet test Server/ClinicalIntelligence.Api.Tests

## Implementation Validation Strategy
- [Automated] Unit tests verify valid status transitions
- [Automated] Unit tests verify invalid transitions are rejected
- [Automated] Integration tests verify status endpoint returns correct data
- [Manual] Verify status updates are persisted to database

## Implementation Checklist
- [x] Create DocumentStatus enum with all status values
- [x] Create IDocumentStatusService interface
- [x] Implement DocumentStatusService with transition validation
- [x] Add GET /documents/{id}/status endpoint
- [x] Register IDocumentStatusService in DI container
- [x] Add unit tests for status transitions
- [x] Add integration tests for status endpoint
