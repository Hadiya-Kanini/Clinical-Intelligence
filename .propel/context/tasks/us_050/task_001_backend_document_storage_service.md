# Task - [TASK_001]

## Requirement Reference
- User Story: [us_050]
- Story Location: [.propel/context/tasks/us_050/us_050.md]
- Acceptance Criteria: 
    - Given a document is uploaded, When stored, Then it is saved to the configured file system path (FR-021).
    - Given document storage, When saved, Then the storage path is recorded in the database with document metadata.
    - Given the storage structure, When organized, Then it follows the pattern: {tenant_id}/{patient_id}/{document_id}/original.{ext} (TR-017).
    - Given document retrieval, When requested, Then the file can be loaded using the stored path.

## Task Overview
Implement a document storage service that persists uploaded files to the local file system following a structured path pattern and records the storage path in the database. The service must handle file operations safely, support retrieval, and integrate with the existing document upload pipeline.

This task builds on the existing `DocumentService.cs` which currently uses a placeholder storage path and adds:
1. Configurable base storage path
2. Structured directory creation: `{tenant_id}/{patient_id}/{document_id}/`
3. File persistence with original extension preservation
4. Database path recording
5. File retrieval capability

## Dependent Tasks
- [US_046/task_001] - Backend file format, MIME type, and size validation
- [US_047/task_001] - Backend password-protected and corrupted file detection
- [US_048/task_001] - Backend malware scanning service

## Impacted Components
- [CREATE | Server/ClinicalIntelligence.Api/Services/IDocumentStorageService.cs | Interface for document storage operations]
- [CREATE | Server/ClinicalIntelligence.Api/Services/LocalFileStorageService.cs | Local file system storage implementation]
- [CREATE | Server/ClinicalIntelligence.Api/Configuration/DocumentStorageOptions.cs | Storage configuration options]
- [MODIFY | Server/ClinicalIntelligence.Api/Services/DocumentService.cs | Integrate storage service for file persistence]
- [MODIFY | Server/ClinicalIntelligence.Api/Program.cs | Register storage service and configuration]

## Implementation Plan

### 1. Create Storage Configuration Options
```csharp
public class DocumentStorageOptions
{
    public const string SectionName = "DocumentStorage";
    
    /// <summary>
    /// Base path for document storage. Default: ./storage/documents
    /// </summary>
    public string BasePath { get; set; } = "./storage/documents";
    
    /// <summary>
    /// Temporary upload path for processing. Default: ./storage/temp
    /// </summary>
    public string TempPath { get; set; } = "./storage/temp";
    
    /// <summary>
    /// Default tenant ID for Phase 1 single-tenant deployment.
    /// </summary>
    public string DefaultTenantId { get; set; } = "default";
    
    /// <summary>
    /// Maximum storage size in bytes (0 = unlimited).
    /// </summary>
    public long MaxStorageBytes { get; set; } = 0;
}
```

### 2. Create Document Storage Interface
```csharp
public interface IDocumentStorageService
{
    /// <summary>
    /// Stores a document file and returns the storage path.
    /// </summary>
    Task<DocumentStorageResult> StoreAsync(
        Stream fileStream,
        string fileName,
        Guid patientId,
        Guid documentId,
        CancellationToken ct);
    
    /// <summary>
    /// Retrieves a document file stream by storage path.
    /// </summary>
    Task<Stream?> RetrieveAsync(string storagePath, CancellationToken ct);
    
    /// <summary>
    /// Deletes a document file by storage path.
    /// </summary>
    Task<bool> DeleteAsync(string storagePath, CancellationToken ct);
    
    /// <summary>
    /// Checks if a document exists at the storage path.
    /// </summary>
    Task<bool> ExistsAsync(string storagePath, CancellationToken ct);
}

public record DocumentStorageResult
{
    public bool IsSuccess { get; init; }
    public string StoragePath { get; init; } = string.Empty;
    public string AbsolutePath { get; init; } = string.Empty;
    public long BytesWritten { get; init; }
    public string? ErrorMessage { get; init; }
}
```

### 3. Implement Local File Storage Service
```csharp
public class LocalFileStorageService : IDocumentStorageService
{
    private readonly DocumentStorageOptions _options;
    private readonly ILogger<LocalFileStorageService> _logger;
    
    public LocalFileStorageService(
        IOptions<DocumentStorageOptions> options,
        ILogger<LocalFileStorageService> logger)
    {
        _options = options.Value;
        _logger = logger;
        
        // Ensure base directories exist
        EnsureDirectoryExists(_options.BasePath);
        EnsureDirectoryExists(_options.TempPath);
    }
    
    public async Task<DocumentStorageResult> StoreAsync(
        Stream fileStream,
        string fileName,
        Guid patientId,
        Guid documentId,
        CancellationToken ct)
    {
        try
        {
            // Build storage path: {tenant_id}/{patient_id}/{document_id}/original.{ext}
            var extension = Path.GetExtension(fileName);
            var relativePath = BuildStoragePath(patientId, documentId, extension);
            var absolutePath = Path.Combine(_options.BasePath, relativePath);
            
            // Ensure directory exists
            var directory = Path.GetDirectoryName(absolutePath);
            EnsureDirectoryExists(directory!);
            
            // Write file
            await using var fileStreamOut = new FileStream(
                absolutePath, 
                FileMode.Create, 
                FileAccess.Write,
                FileShare.None,
                bufferSize: 81920,
                useAsync: true);
            
            await fileStream.CopyToAsync(fileStreamOut, ct);
            
            _logger.LogInformation(
                "Document stored: DocumentId={DocumentId}, Path={Path}, Bytes={Bytes}",
                documentId, relativePath, fileStreamOut.Length);
            
            return new DocumentStorageResult
            {
                IsSuccess = true,
                StoragePath = relativePath,
                AbsolutePath = absolutePath,
                BytesWritten = fileStreamOut.Length
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to store document: DocumentId={DocumentId}", documentId);
            return new DocumentStorageResult
            {
                IsSuccess = false,
                ErrorMessage = ex.Message
            };
        }
    }
    
    private string BuildStoragePath(Guid patientId, Guid documentId, string extension)
    {
        // Pattern: {tenant_id}/{patient_id}/{document_id}/original.{ext}
        return Path.Combine(
            _options.DefaultTenantId,
            patientId.ToString(),
            documentId.ToString(),
            $"original{extension}");
    }
    
    public async Task<Stream?> RetrieveAsync(string storagePath, CancellationToken ct)
    {
        var absolutePath = Path.Combine(_options.BasePath, storagePath);
        
        if (!File.Exists(absolutePath))
        {
            _logger.LogWarning("Document not found: Path={Path}", storagePath);
            return null;
        }
        
        return new FileStream(
            absolutePath,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read,
            bufferSize: 81920,
            useAsync: true);
    }
    
    public Task<bool> DeleteAsync(string storagePath, CancellationToken ct)
    {
        var absolutePath = Path.Combine(_options.BasePath, storagePath);
        
        if (!File.Exists(absolutePath))
        {
            return Task.FromResult(false);
        }
        
        File.Delete(absolutePath);
        
        // Clean up empty parent directories
        CleanupEmptyDirectories(Path.GetDirectoryName(absolutePath)!);
        
        _logger.LogInformation("Document deleted: Path={Path}", storagePath);
        return Task.FromResult(true);
    }
    
    public Task<bool> ExistsAsync(string storagePath, CancellationToken ct)
    {
        var absolutePath = Path.Combine(_options.BasePath, storagePath);
        return Task.FromResult(File.Exists(absolutePath));
    }
    
    private static void EnsureDirectoryExists(string path)
    {
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
    }
    
    private void CleanupEmptyDirectories(string directory)
    {
        // Don't delete beyond base path
        if (!directory.StartsWith(_options.BasePath))
            return;
            
        if (Directory.Exists(directory) && !Directory.EnumerateFileSystemEntries(directory).Any())
        {
            Directory.Delete(directory);
            CleanupEmptyDirectories(Path.GetDirectoryName(directory)!);
        }
    }
}
```

### 4. Update DocumentService Integration
```csharp
public class DocumentService : IDocumentService
{
    private readonly IDocumentStorageService _storageService;
    
    public async Task<UploadAcknowledgmentResponse> ValidateAndAcknowledgeAsync(...)
    {
        // ... existing validation logic
        
        if (isValid)
        {
            // Store file to file system
            using var stream = file.OpenReadStream();
            var storageResult = await _storageService.StoreAsync(
                stream, 
                fileName, 
                patientId, 
                documentId, 
                cancellationToken);
            
            if (!storageResult.IsSuccess)
            {
                validationErrors.Add($"Failed to store file: {storageResult.ErrorMessage}");
                isValid = false;
            }
            else
            {
                var document = new Document
                {
                    Id = documentId,
                    PatientId = patientId,
                    UploadedByUserId = uploadedByUserId,
                    OriginalName = fileName,
                    MimeType = contentType,
                    SizeBytes = (int)fileSize,
                    StoragePath = storageResult.StoragePath,  // Use actual storage path
                    Status = "Pending",
                    UploadedAt = DateTime.UtcNow
                };
                
                _dbContext.Documents.Add(document);
                await _dbContext.SaveChangesAsync(cancellationToken);
            }
        }
    }
}
```

### 5. Add Document Retrieval Endpoint
```csharp
v1.MapGet("/documents/{documentId}/content", async (
    Guid documentId,
    HttpContext context,
    ApplicationDbContext dbContext,
    IDocumentStorageService storageService,
    ILogger<Program> logger) =>
{
    var userIdClaim = context.User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
    if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
    {
        return ApiErrorResults.Unauthorized("unauthorized", "User authentication required.");
    }
    
    var document = await dbContext.Documents
        .FirstOrDefaultAsync(d => d.Id == documentId && !d.IsDeleted);
    
    if (document == null)
    {
        return ApiErrorResults.NotFound("document_not_found", "Document not found.");
    }
    
    var stream = await storageService.RetrieveAsync(document.StoragePath, context.RequestAborted);
    if (stream == null)
    {
        return ApiErrorResults.NotFound("file_not_found", "Document file not found in storage.");
    }
    
    return Results.File(stream, document.MimeType, document.OriginalName);
})
    .RequireAuthorization()
    .WithName("GetDocumentContent")
    .WithOpenApi(operation => new(operation)
    {
        Summary = "Retrieve document content",
        Description = "Downloads the original document file by document ID."
    });
```

## Current Project State
```
Server/ClinicalIntelligence.Api/
├── Services/
│   └── DocumentService.cs          # Uses placeholder storage path
├── Configuration/
│   └── (existing options classes)
├── Domain/Models/
│   └── Document.cs                 # Has StoragePath property
```

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| CREATE | Server/ClinicalIntelligence.Api/Services/IDocumentStorageService.cs | Interface for document storage operations |
| CREATE | Server/ClinicalIntelligence.Api/Services/LocalFileStorageService.cs | Local file system storage implementation |
| CREATE | Server/ClinicalIntelligence.Api/Configuration/DocumentStorageOptions.cs | Storage configuration options |
| MODIFY | Server/ClinicalIntelligence.Api/Services/DocumentService.cs | Integrate IDocumentStorageService for file persistence |
| MODIFY | Server/ClinicalIntelligence.Api/Program.cs | Register storage service, add retrieval endpoint |

## External References
- https://learn.microsoft.com/en-us/dotnet/api/system.io.filestream
- https://learn.microsoft.com/en-us/aspnet/core/fundamentals/configuration/options

## Build Commands
- dotnet build Server/ClinicalIntelligence.Api
- dotnet test Server/ClinicalIntelligence.Api.Tests

## Implementation Validation Strategy
- [Automated] Unit tests verify storage path pattern generation
- [Automated] Unit tests verify file write and read operations
- [Automated] Unit tests verify directory creation
- [Automated] Integration tests verify document upload stores file
- [Automated] Integration tests verify document retrieval returns file
- [Manual] Verify storage directory structure matches pattern

## Implementation Checklist
- [x] Create DocumentStorageOptions configuration class
- [x] Create IDocumentStorageService interface with DocumentStorageResult
- [x] Implement LocalFileStorageService with path pattern logic
- [x] Implement StoreAsync with directory creation and file write
- [x] Implement RetrieveAsync with file stream return
- [x] Implement DeleteAsync with empty directory cleanup
- [x] Update DocumentService to use IDocumentStorageService
- [x] Add GET /documents/{id}/content retrieval endpoint
- [x] Register services and configuration in Program.cs
- [ ] Add appsettings.json configuration section for DocumentStorage
