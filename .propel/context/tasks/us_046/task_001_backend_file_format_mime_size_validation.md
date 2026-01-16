# Task - [TASK_001]

## Requirement Reference
- User Story: [us_046]
- Story Location: [.propel/context/tasks/us_046/us_046.md]
- Acceptance Criteria: 
    - Given a file is uploaded, When validated, Then only .pdf and .docx extensions are accepted (FR-015a).
    - Given a file is uploaded, When validated, Then MIME type must match declared format (FR-015b).
    - Given a file is uploaded, When validated, Then size must not exceed 50MB (FR-016).
    - Given validation failure, When detected, Then the file is rejected with specific error message.

## Task Overview
Enhance the backend `DocumentService` to implement comprehensive file validation for format, MIME type, and size. The service must validate that uploaded files have correct extensions (.pdf, .docx), matching MIME types, and do not exceed 50MB. When validation fails, specific error messages must be returned identifying the exact issue.

This task builds on the existing `DocumentService.cs` which has basic validation. The enhancement focuses on:
1. Stricter MIME type to extension matching (detect mismatches like .pdf with text/plain)
2. Double extension detection (.pdf.exe)
3. Boundary condition handling for 50MB limit
4. Specific error codes for each validation failure type

## Dependent Tasks
- [US_044/task_001] - Backend upload acknowledgment endpoint (completed)

## Impacted Components
- [MODIFY | Server/ClinicalIntelligence.Api/Services/DocumentService.cs | Enhance validation with MIME-extension matching and specific error codes]
- [CREATE | Server/ClinicalIntelligence.Api/Contracts/FileValidationErrorCode.cs | Enum for specific validation error codes]
- [MODIFY | Server/ClinicalIntelligence.Api/Contracts/UploadAcknowledgmentResponse.cs | Add error code field for programmatic error handling]

## Implementation Plan

### 1. Define Validation Error Codes
```csharp
public enum FileValidationErrorCode
{
    None = 0,
    InvalidExtension = 1,        // FR-015a: Wrong file extension
    InvalidMimeType = 2,         // FR-015b: MIME type doesn't match extension
    MimeExtensionMismatch = 3,   // FR-015b: MIME type doesn't match declared format
    FileTooLarge = 4,            // FR-016: Exceeds 50MB
    FileEmpty = 5,               // FR-015f: 0 bytes
    DoubleExtension = 6,         // Security: .pdf.exe pattern
    SuspiciousContent = 7        // FR-015g: Executable content detected
}
```

### 2. Enhance MIME Type Validation
- Create MIME-to-extension mapping dictionary
- Validate that declared MIME type matches file extension
- Detect MIME type spoofing (e.g., .pdf file with text/plain MIME)

```csharp
private static readonly Dictionary<string, string[]> MimeToExtensionMap = new()
{
    { "application/pdf", new[] { ".pdf" } },
    { "application/vnd.openxmlformats-officedocument.wordprocessingml.document", new[] { ".docx" } }
};
```

### 3. Implement Double Extension Detection
- Parse filename for multiple extensions
- Reject files with patterns like `.pdf.exe`, `.docx.bat`
- Return specific error for double extension attempts

### 4. Enhance Size Validation
- Validate file size against 50MB limit (52,428,800 bytes)
- Handle boundary condition: files exactly at 50MB should be accepted
- Files at 50MB + 1 byte should be rejected

### 5. Update Response Contract
```csharp
public record UploadAcknowledgmentResponse
{
    // ... existing fields
    public FileValidationErrorCode? ErrorCode { get; init; }
    public string ErrorType { get; init; } // "invalid_extension", "mime_mismatch", etc.
}
```

### 6. Implement Validation Pipeline
```csharp
private ValidationResult ValidateFile(IFormFile file)
{
    // 1. Check for empty file
    // 2. Check file size
    // 3. Validate extension
    // 4. Check for double extensions
    // 5. Validate MIME type
    // 6. Validate MIME-extension match
    // Return first failure with specific error code
}
```

## Current Project State
```
Server/ClinicalIntelligence.Api/
├── Services/
│   └── DocumentService.cs          # Has basic validation, needs enhancement
├── Contracts/
│   └── UploadAcknowledgmentResponse.cs  # Needs error code field
├── Endpoints/
│   └── (endpoint files)
```

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| CREATE | Server/ClinicalIntelligence.Api/Contracts/FileValidationErrorCode.cs | Enum for specific validation error codes |
| MODIFY | Server/ClinicalIntelligence.Api/Contracts/UploadAcknowledgmentResponse.cs | Add ErrorCode and ErrorType fields |
| MODIFY | Server/ClinicalIntelligence.Api/Services/DocumentService.cs | Implement MIME-extension matching, double extension detection, boundary size handling |

## External References
- https://developer.mozilla.org/en-US/docs/Web/HTTP/Basics_of_HTTP/MIME_types
- https://learn.microsoft.com/en-us/aspnet/core/mvc/models/file-uploads

## Build Commands
- dotnet build Server/ClinicalIntelligence.Api
- dotnet test Server/ClinicalIntelligence.Api.Tests

## Implementation Validation Strategy
- [Automated] Unit tests verify .pdf and .docx extensions are accepted
- [Automated] Unit tests verify other extensions (.txt, .exe) are rejected with InvalidExtension error
- [Automated] Unit tests verify MIME type mismatch detection (e.g., .pdf with text/plain)
- [Automated] Unit tests verify double extension rejection (.pdf.exe)
- [Automated] Unit tests verify 50MB boundary (50MB accepted, 50MB+1 rejected)
- [Automated] Unit tests verify empty file rejection
- [Automated] Integration tests verify error codes returned in response

## Implementation Checklist
- [x] Create FileValidationErrorCode enum with all error types
- [x] Update UploadAcknowledgmentResponse with ErrorCode and ErrorType fields
- [x] Implement MIME-to-extension mapping validation
- [x] Add double extension detection logic
- [x] Enhance size validation with exact boundary handling
- [x] Update ValidateAndAcknowledgeAsync to use new validation pipeline
- [x] Add specific error messages for each validation failure type
- [x] Write unit tests for all validation scenarios
- [x] Test edge cases (exactly 50MB, MIME spoofing, double extensions)
