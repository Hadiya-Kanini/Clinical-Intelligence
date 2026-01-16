# Task - [TASK_001]

## Requirement Reference
- User Story: [us_049]
- Story Location: [.propel/context/tasks/us_049/us_049.md]
- Acceptance Criteria: 
    - Given a batch upload, When more than 10 files are selected, Then only the first 10 are accepted (FR-014).
    - Given batch limit is exceeded, When detected, Then a warning is displayed about the remaining files.
    - Given the API, When more than 10 files are submitted, Then excess files are rejected with clear error.

## Task Overview
Implement backend batch upload endpoint that enforces the 10-file limit per batch. The endpoint must accept multiple files, validate the batch size, process the first 10 files if limit is exceeded, and return appropriate warnings about rejected files. This creates the `DocumentBatch` entity and links uploaded documents to it.

This task builds on the existing single-file upload endpoint and adds:
1. Multi-file batch upload endpoint
2. Batch size validation and enforcement
3. DocumentBatch entity creation and management
4. Per-file validation results with batch-level summary

## Dependent Tasks
- [US_046/task_001] - Backend file format, MIME type, and size validation
- [US_047/task_001] - Backend password-protected and corrupted file detection
- [US_048/task_001] - Backend malware scanning service

## Impacted Components
- [CREATE | Server/ClinicalIntelligence.Api/Contracts/BatchUploadRequest.cs | Request contract for batch upload]
- [CREATE | Server/ClinicalIntelligence.Api/Contracts/BatchUploadResponse.cs | Response contract with per-file results]
- [CREATE | Server/ClinicalIntelligence.Api/Services/BatchUploadService.cs | Service for batch upload orchestration]
- [MODIFY | Server/ClinicalIntelligence.Api/Services/DocumentService.cs | Add batch-aware validation method]
- [MODIFY | Server/ClinicalIntelligence.Api/Program.cs | Add batch upload endpoint]

## Implementation Plan

### 1. Create Batch Upload Request Contract
```csharp
public record BatchUploadRequest
{
    [JsonPropertyName("patientId")]
    public Guid PatientId { get; init; }
}
```

### 2. Create Batch Upload Response Contract
```csharp
public record BatchUploadResponse
{
    [JsonPropertyName("batchId")]
    public Guid BatchId { get; init; }
    
    [JsonPropertyName("patientId")]
    public Guid PatientId { get; init; }
    
    [JsonPropertyName("totalFilesReceived")]
    public int TotalFilesReceived { get; init; }
    
    [JsonPropertyName("filesAccepted")]
    public int FilesAccepted { get; init; }
    
    [JsonPropertyName("filesRejected")]
    public int FilesRejected { get; init; }
    
    [JsonPropertyName("batchLimitExceeded")]
    public bool BatchLimitExceeded { get; init; }
    
    [JsonPropertyName("batchLimitWarning")]
    public string? BatchLimitWarning { get; init; }
    
    [JsonPropertyName("fileResults")]
    public List<FileUploadResult> FileResults { get; init; } = new();
    
    [JsonPropertyName("acknowledgedAt")]
    public DateTime AcknowledgedAt { get; init; }
}

public record FileUploadResult
{
    [JsonPropertyName("fileName")]
    public string FileName { get; init; } = string.Empty;
    
    [JsonPropertyName("documentId")]
    public Guid? DocumentId { get; init; }
    
    [JsonPropertyName("isAccepted")]
    public bool IsAccepted { get; init; }
    
    [JsonPropertyName("status")]
    public string Status { get; init; } = string.Empty;
    
    [JsonPropertyName("validationErrors")]
    public List<string> ValidationErrors { get; init; } = new();
    
    [JsonPropertyName("rejectionReason")]
    public string? RejectionReason { get; init; }
}
```

### 3. Create BatchUploadService
```csharp
public interface IBatchUploadService
{
    Task<BatchUploadResponse> ProcessBatchAsync(
        IFormFileCollection files,
        Guid patientId,
        Guid uploadedByUserId,
        CancellationToken ct);
}

public class BatchUploadService : IBatchUploadService
{
    private const int MaxFilesPerBatch = 10;
    
    public async Task<BatchUploadResponse> ProcessBatchAsync(...)
    {
        var batchId = Guid.NewGuid();
        var fileResults = new List<FileUploadResult>();
        var batchLimitExceeded = files.Count > MaxFilesPerBatch;
        
        // Create DocumentBatch entity
        var batch = new DocumentBatch
        {
            Id = batchId,
            PatientId = patientId,
            UploadedByUserId = uploadedByUserId,
            UploadedAt = DateTime.UtcNow
        };
        
        // Process first 10 files
        var filesToProcess = files.Take(MaxFilesPerBatch);
        foreach (var file in filesToProcess)
        {
            var result = await _documentService.ValidateAndAcknowledgeAsync(
                file, patientId, uploadedByUserId, batchId, ct);
            fileResults.Add(MapToFileResult(result));
        }
        
        // Mark remaining files as rejected due to batch limit
        var excessFiles = files.Skip(MaxFilesPerBatch);
        foreach (var file in excessFiles)
        {
            fileResults.Add(new FileUploadResult
            {
                FileName = file.FileName,
                IsAccepted = false,
                Status = "BatchLimitExceeded",
                RejectionReason = "File exceeds batch limit of 10 files"
            });
        }
        
        return BuildResponse(batchId, patientId, fileResults, batchLimitExceeded);
    }
}
```

### 4. Update DocumentService for Batch Support
```csharp
public async Task<UploadAcknowledgmentResponse> ValidateAndAcknowledgeAsync(
    IFormFile file,
    Guid patientId,
    Guid uploadedByUserId,
    Guid? batchId = null,  // NEW: Optional batch ID
    CancellationToken cancellationToken = default)
{
    // ... existing validation logic
    
    if (isValid)
    {
        var document = new Document
        {
            // ... existing fields
            DocumentBatchId = batchId  // Link to batch
        };
    }
}
```

### 5. Add Batch Upload Endpoint
```csharp
v1.MapPost("/documents/batch", async (HttpContext context, IBatchUploadService batchService, ILogger<Program> logger) =>
{
    var userIdClaim = context.User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
    if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
    {
        return ApiErrorResults.Unauthorized("unauthorized", "User authentication required.");
    }

    if (!context.Request.HasFormContentType)
    {
        return ApiErrorResults.BadRequest("invalid_content_type", "Request must be multipart/form-data.");
    }

    var form = await context.Request.ReadFormAsync(context.RequestAborted);
    var files = form.Files;

    if (files.Count == 0)
    {
        return ApiErrorResults.BadRequest("missing_files", "No files provided in the request.");
    }

    var patientIdStr = form["patientId"].ToString();
    if (string.IsNullOrEmpty(patientIdStr) || !Guid.TryParse(patientIdStr, out var patientId))
    {
        return ApiErrorResults.BadRequest("invalid_patient_id", "Valid patientId is required.");
    }

    var response = await batchService.ProcessBatchAsync(files, patientId, userId, context.RequestAborted);
    
    if (response.BatchLimitExceeded)
    {
        logger.LogWarning("Batch upload limit exceeded: Received={Received}, Accepted={Accepted}",
            response.TotalFilesReceived, response.FilesAccepted);
    }
    
    return Results.Ok(response);
})
    .RequireAuthorization()
    .DisableAntiforgery()
    .WithName("BatchUploadDocuments")
    .WithOpenApi(operation => new(operation)
    {
        Summary = "Upload multiple documents in a batch",
        Description = "Uploads up to 10 documents (PDF or DOCX, max 50MB each) per batch. " +
                      "Files beyond the 10-file limit are rejected with a warning."
    });
```

### 6. Batch Limit Warning Message
```csharp
private string GenerateBatchLimitWarning(int totalReceived, int accepted)
{
    var rejected = totalReceived - accepted;
    return $"Batch limit of 10 files exceeded. {accepted} files were accepted, " +
           $"{rejected} files were not processed. Please upload remaining files in a separate batch.";
}
```

## Current Project State
```
Server/ClinicalIntelligence.Api/
├── Services/
│   └── DocumentService.cs          # Single file upload
├── Domain/Models/
│   ├── Document.cs                 # Has DocumentBatchId FK
│   └── DocumentBatch.cs            # Batch entity exists
├── Contracts/
│   └── UploadAcknowledgmentResponse.cs
```

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| CREATE | Server/ClinicalIntelligence.Api/Contracts/BatchUploadRequest.cs | Request contract for batch upload |
| CREATE | Server/ClinicalIntelligence.Api/Contracts/BatchUploadResponse.cs | Response contract with per-file results and batch warnings |
| CREATE | Server/ClinicalIntelligence.Api/Services/BatchUploadService.cs | Service orchestrating batch upload with limit enforcement |
| MODIFY | Server/ClinicalIntelligence.Api/Services/DocumentService.cs | Add optional batchId parameter to validation method |
| MODIFY | Server/ClinicalIntelligence.Api/Program.cs | Add POST /documents/batch endpoint and register BatchUploadService |

## External References
- https://learn.microsoft.com/en-us/aspnet/core/mvc/models/file-uploads#upload-large-files-with-streaming
- https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.http.iformfilecollection

## Build Commands
- dotnet build Server/ClinicalIntelligence.Api
- dotnet test Server/ClinicalIntelligence.Api.Tests

## Implementation Validation Strategy
- [Automated] Unit tests verify batch limit enforcement (10 files max)
- [Automated] Unit tests verify first 10 files are processed when limit exceeded
- [Automated] Unit tests verify excess files are marked as rejected
- [Automated] Unit tests verify batch warning message generation
- [Automated] Integration tests verify API returns correct response structure
- [Automated] Integration tests verify DocumentBatch entity is created

## Implementation Checklist
- [x] Create BatchUploadRequest contract
- [x] Create BatchUploadResponse and FileUploadResult contracts
- [x] Create IBatchUploadService interface
- [x] Implement BatchUploadService with 10-file limit enforcement
- [x] Update DocumentService.ValidateAndAcknowledgeAsync to accept optional batchId
- [x] Add POST /documents/batch endpoint to Program.cs
- [x] Register IBatchUploadService in DI container
- [x] Implement batch limit warning message generation
- [x] Log batch limit exceeded events
