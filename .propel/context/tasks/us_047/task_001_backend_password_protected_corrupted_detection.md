# Task - [TASK_001]

## Requirement Reference
- User Story: [us_047]
- Story Location: [.propel/context/tasks/us_047/us_047.md]
- Acceptance Criteria: 
    - Given a password-protected PDF or DOCX, When detected, Then it is rejected with "Password-protected files are not supported" error (FR-015c).
    - Given a corrupted or malformed file, When detected, Then it is rejected with "File appears to be corrupted" error (FR-015d).
    - Given document structure validation, When performed, Then file integrity is verified before acceptance (FR-015e).
    - Given an empty file (0 bytes), When detected, Then it is rejected with "Empty files cannot be processed" error (FR-015f).

## Task Overview
Enhance the backend `DocumentService` to detect and reject password-protected PDF/DOCX files, corrupted/malformed documents, and empty files. The service must perform document structure validation to verify file integrity before accepting documents for processing.

This task builds on US_046/task_001 validation and adds deeper document inspection:
1. PDF password protection detection using PDF header/trailer analysis
2. DOCX password protection detection using ZIP structure analysis
3. Document structure integrity validation
4. Specific error messages for each rejection reason

## Dependent Tasks
- [US_046/task_001] - Backend file format, MIME type, and size validation

## Impacted Components
- [MODIFY | Server/ClinicalIntelligence.Api/Services/DocumentService.cs | Add password protection and corruption detection]
- [MODIFY | Server/ClinicalIntelligence.Api/Contracts/FileValidationErrorCode.cs | Add error codes for password-protected and corrupted files]
- [CREATE | Server/ClinicalIntelligence.Api/Services/DocumentIntegrityValidator.cs | Document structure validation service]

## Implementation Plan

### 1. Add New Validation Error Codes
```csharp
public enum FileValidationErrorCode
{
    // ... existing codes from US_046
    PasswordProtected = 10,      // FR-015c: Password-protected file
    FileCorrupted = 11,          // FR-015d: Corrupted or malformed
    InvalidStructure = 12,       // FR-015e: Failed structure validation
    FileEmpty = 5                // FR-015f: Empty file (already exists)
}
```

### 2. PDF Password Protection Detection
```csharp
private async Task<bool> IsPdfPasswordProtectedAsync(Stream stream, CancellationToken ct)
{
    // Read PDF content and check for encryption dictionary
    // Look for /Encrypt entry in PDF trailer
    // Check for /Standard or /AESV2 encryption markers
    // Return true if password protection detected
}
```

**PDF Encryption Markers:**
- `/Encrypt` dictionary in trailer
- `/Standard` security handler
- `/AESV2` or `/AESV3` encryption
- `/P` permissions entry with restricted value

### 3. DOCX Password Protection Detection
```csharp
private async Task<bool> IsDocxPasswordProtectedAsync(Stream stream, CancellationToken ct)
{
    // DOCX is a ZIP archive
    // Check for EncryptedPackage in ZIP entries
    // Look for [Content_Types].xml with encrypted content type
    // Check for encryption.xml in docProps folder
}
```

**DOCX Encryption Markers:**
- `EncryptedPackage` stream in OLE compound document
- Missing or encrypted `[Content_Types].xml`
- `encryption.xml` in package

### 4. PDF Structure Validation
```csharp
private async Task<bool> ValidatePdfStructureAsync(Stream stream, CancellationToken ct)
{
    // Verify PDF header (%PDF-1.x)
    // Check for %%EOF marker at end
    // Validate xref table presence
    // Verify trailer dictionary
    // Return false if structure is invalid
}
```

### 5. DOCX Structure Validation
```csharp
private async Task<bool> ValidateDocxStructureAsync(Stream stream, CancellationToken ct)
{
    // Verify ZIP signature (PK\x03\x04)
    // Check for required entries:
    //   - [Content_Types].xml
    //   - word/document.xml
    //   - _rels/.rels
    // Validate XML structure of core files
}
```

### 6. Create DocumentIntegrityValidator Service
```csharp
public interface IDocumentIntegrityValidator
{
    Task<DocumentValidationResult> ValidateAsync(IFormFile file, CancellationToken ct);
}

public record DocumentValidationResult
{
    public bool IsValid { get; init; }
    public FileValidationErrorCode? ErrorCode { get; init; }
    public string ErrorMessage { get; init; }
    public bool IsPasswordProtected { get; init; }
    public bool IsCorrupted { get; init; }
    public bool IsEmpty { get; init; }
}
```

### 7. Update DocumentService Validation Pipeline
```csharp
public async Task<UploadAcknowledgmentResponse> ValidateAndAcknowledgeAsync(...)
{
    // 1. Basic validation (size, extension, MIME) - from US_046
    // 2. Empty file check
    // 3. Document integrity validation
    // 4. Password protection check
    // 5. Return first failure with specific error
}
```

### 8. Error Messages
| Error Code | Message |
|------------|---------|
| PasswordProtected | "Password-protected files are not supported. Please remove password protection and try again." |
| FileCorrupted | "File appears to be corrupted. Please try re-downloading or re-exporting the file." |
| InvalidStructure | "File structure is invalid. The file may be damaged or not a valid PDF/DOCX." |
| FileEmpty | "Empty files cannot be processed. Please upload a file with content." |

## Current Project State
```
Server/ClinicalIntelligence.Api/
├── Services/
│   └── DocumentService.cs          # Has basic validation from US_046
├── Contracts/
│   ├── FileValidationErrorCode.cs  # Error codes from US_046
│   └── UploadAcknowledgmentResponse.cs
```

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| CREATE | Server/ClinicalIntelligence.Api/Services/DocumentIntegrityValidator.cs | Document structure and integrity validation service |
| MODIFY | Server/ClinicalIntelligence.Api/Contracts/FileValidationErrorCode.cs | Add PasswordProtected, FileCorrupted, InvalidStructure error codes |
| MODIFY | Server/ClinicalIntelligence.Api/Services/DocumentService.cs | Integrate DocumentIntegrityValidator, add password/corruption detection |
| MODIFY | Server/ClinicalIntelligence.Api/Program.cs | Register IDocumentIntegrityValidator in DI container |

## External References
- https://www.adobe.com/content/dam/acom/en/devnet/pdf/pdfs/PDF32000_2008.pdf (PDF Reference)
- https://docs.microsoft.com/en-us/openspecs/office_standards/ms-docx (DOCX Specification)
- https://learn.microsoft.com/en-us/dotnet/api/system.io.compression.ziparchive

## Build Commands
- dotnet build Server/ClinicalIntelligence.Api
- dotnet test Server/ClinicalIntelligence.Api.Tests

## Implementation Validation Strategy
- [Automated] Unit tests verify password-protected PDF detection
- [Automated] Unit tests verify password-protected DOCX detection
- [Automated] Unit tests verify corrupted PDF rejection
- [Automated] Unit tests verify corrupted DOCX rejection
- [Automated] Unit tests verify empty file rejection with correct error
- [Automated] Integration tests verify API returns correct error codes
- [Manual] Test with real password-protected PDF files
- [Manual] Test with real password-protected DOCX files

## Implementation Checklist
- [x] Add PasswordProtected, FileCorrupted, InvalidStructure to FileValidationErrorCode enum
- [x] Create IDocumentIntegrityValidator interface
- [x] Implement DocumentIntegrityValidator class
- [x] Implement PDF password protection detection (check for /Encrypt dictionary)
- [x] Implement DOCX password protection detection (check for EncryptedPackage)
- [x] Implement PDF structure validation (header, EOF, xref, trailer)
- [x] Implement DOCX structure validation (ZIP entries, required files)
- [x] Update DocumentService to use DocumentIntegrityValidator
- [x] Register DocumentIntegrityValidator in DI container
- [x] Add specific error messages for each failure type
- [x] Test with password-protected sample files
- [x] Test with corrupted sample files
