# Task - [TASK_002]

## Requirement Reference
- User Story: [us_046]
- Story Location: [.propel/context/tasks/us_046/us_046.md]
- Acceptance Criteria: 
    - Given a file is uploaded, When validated, Then only .pdf and .docx extensions are accepted (FR-015a).
    - Given a file is uploaded, When validated, Then MIME type must match declared format (FR-015b).
    - Given a file is uploaded, When validated, Then size must not exceed 50MB (FR-016).
    - Given validation failure, When detected, Then the file is rejected with specific error message.

## Task Overview
Create comprehensive unit and integration tests for the file format, MIME type, and size validation functionality implemented in TASK_001. Tests must cover all validation scenarios including edge cases for MIME-extension mismatches, double extensions, and size boundary conditions.

## Dependent Tasks
- [US_046/task_001] - Backend file format, MIME type, and size validation implementation

## Impacted Components
- [CREATE | Server/ClinicalIntelligence.Api.Tests/Services/DocumentServiceValidationTests.cs | Unit tests for validation logic]
- [MODIFY | Server/ClinicalIntelligence.Api.Tests/Endpoints/UploadAcknowledgmentTests.cs | Add integration tests for new validation scenarios]

## Implementation Plan

### 1. Unit Tests for Extension Validation (FR-015a)
```csharp
[Theory]
[InlineData(".pdf", true)]
[InlineData(".docx", true)]
[InlineData(".PDF", true)]   // Case insensitive
[InlineData(".DOCX", true)]  // Case insensitive
[InlineData(".txt", false)]
[InlineData(".exe", false)]
[InlineData(".doc", false)]  // Old Word format not allowed
[InlineData("", false)]      // No extension
public async Task ValidateFile_Extension_ReturnsExpectedResult(string extension, bool expectedValid)
```

### 2. Unit Tests for MIME Type Validation (FR-015b)
```csharp
[Theory]
[InlineData("application/pdf", ".pdf", true)]
[InlineData("application/vnd.openxmlformats-officedocument.wordprocessingml.document", ".docx", true)]
[InlineData("text/plain", ".pdf", false)]           // MIME mismatch
[InlineData("application/pdf", ".docx", false)]     // Extension mismatch
[InlineData("application/octet-stream", ".pdf", false)] // Generic MIME
public async Task ValidateFile_MimeType_ReturnsExpectedResult(string mimeType, string extension, bool expectedValid)
```

### 3. Unit Tests for Size Validation (FR-016)
```csharp
[Theory]
[InlineData(0, false)]                    // Empty file
[InlineData(1024, true)]                  // 1KB - valid
[InlineData(52428800, true)]              // Exactly 50MB - valid
[InlineData(52428801, false)]             // 50MB + 1 byte - invalid
[InlineData(104857600, false)]            // 100MB - invalid
public async Task ValidateFile_Size_ReturnsExpectedResult(long fileSize, bool expectedValid)
```

### 4. Unit Tests for Double Extension Detection
```csharp
[Theory]
[InlineData("document.pdf", true)]
[InlineData("document.docx", true)]
[InlineData("document.pdf.exe", false)]   // Double extension attack
[InlineData("document.docx.bat", false)]  // Double extension attack
[InlineData("my.report.pdf", true)]       // Valid: period in name
[InlineData("file.pdf.pdf", false)]       // Suspicious double extension
public async Task ValidateFile_DoubleExtension_ReturnsExpectedResult(string fileName, bool expectedValid)
```

### 5. Unit Tests for Error Code Assignment
```csharp
[Fact]
public async Task ValidateFile_InvalidExtension_ReturnsInvalidExtensionErrorCode()

[Fact]
public async Task ValidateFile_MimeMismatch_ReturnsMimeExtensionMismatchErrorCode()

[Fact]
public async Task ValidateFile_FileTooLarge_ReturnsFileTooLargeErrorCode()

[Fact]
public async Task ValidateFile_EmptyFile_ReturnsFileEmptyErrorCode()

[Fact]
public async Task ValidateFile_DoubleExtension_ReturnsDoubleExtensionErrorCode()
```

### 6. Integration Tests for API Response
```csharp
[Fact]
public async Task UploadDocument_MimeMismatch_Returns422WithMismatchError()

[Fact]
public async Task UploadDocument_DoubleExtension_Returns422WithSecurityError()

[Fact]
public async Task UploadDocument_Exactly50MB_Returns200Accepted()

[Fact]
public async Task UploadDocument_Over50MB_Returns422WithSizeError()
```

### 7. Edge Case Tests
```csharp
[Fact]
public async Task ValidateFile_CaseInsensitiveExtension_AcceptsPDFAndDocx()

[Fact]
public async Task ValidateFile_WhitespaceInFilename_HandlesCorrectly()

[Fact]
public async Task ValidateFile_UnicodeFilename_HandlesCorrectly()

[Fact]
public async Task ValidateFile_VeryLongFilename_HandlesCorrectly()
```

## Current Project State
```
Server/ClinicalIntelligence.Api.Tests/
├── Services/
│   └── (service test files)
├── Endpoints/
│   └── UploadAcknowledgmentTests.cs  # Existing upload tests
```

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| CREATE | Server/ClinicalIntelligence.Api.Tests/Services/DocumentServiceValidationTests.cs | Unit tests for file validation logic |
| MODIFY | Server/ClinicalIntelligence.Api.Tests/Endpoints/UploadAcknowledgmentTests.cs | Add integration tests for MIME mismatch, double extension, size boundary |

## External References
- https://xunit.net/docs/getting-started/netcore/cmdline
- https://learn.microsoft.com/en-us/dotnet/core/testing/unit-testing-best-practices

## Build Commands
- dotnet test Server/ClinicalIntelligence.Api.Tests

## Implementation Validation Strategy
- [Automated] All unit tests pass for extension validation
- [Automated] All unit tests pass for MIME type validation
- [Automated] All unit tests pass for size validation
- [Automated] All unit tests pass for double extension detection
- [Automated] All integration tests pass for API error responses
- [Automated] Code coverage report shows >90% coverage for validation logic

## Implementation Checklist
- [x] Create DocumentServiceValidationTests.cs test file
- [x] Implement extension validation tests (valid and invalid cases)
- [x] Implement MIME type validation tests (matching and mismatching)
- [x] Implement size validation tests (boundary conditions)
- [x] Implement double extension detection tests
- [x] Implement error code assignment tests
- [x] Add integration tests for API error responses
- [x] Add edge case tests (case sensitivity, unicode, long names)
- [x] Verify all tests pass
- [x] Generate code coverage report
