# Task - [TASK_002]

## Requirement Reference
- User Story: [us_047]
- Story Location: [.propel/context/tasks/us_047/us_047.md]
- Acceptance Criteria: 
    - Given a password-protected PDF or DOCX, When detected, Then it is rejected with "Password-protected files are not supported" error (FR-015c).
    - Given a corrupted or malformed file, When detected, Then it is rejected with "File appears to be corrupted" error (FR-015d).
    - Given document structure validation, When performed, Then file integrity is verified before acceptance (FR-015e).
    - Given an empty file (0 bytes), When detected, Then it is rejected with "Empty files cannot be processed" error (FR-015f).

## Task Overview
Create comprehensive unit and integration tests for password-protected file detection, corrupted file detection, and document structure validation implemented in TASK_001. Tests must cover PDF and DOCX formats with various encryption methods, corruption scenarios, and edge cases.

## Dependent Tasks
- [US_047/task_001] - Backend password-protected and corrupted file detection implementation

## Impacted Components
- [CREATE | Server/ClinicalIntelligence.Api.Tests/Services/DocumentIntegrityValidatorTests.cs | Unit tests for document integrity validation]
- [MODIFY | Server/ClinicalIntelligence.Api.Tests/Endpoints/UploadAcknowledgmentTests.cs | Add integration tests for password-protected and corrupted file scenarios]
- [CREATE | Server/ClinicalIntelligence.Api.Tests/Fixtures/TestDocuments/ | Test document fixtures (password-protected, corrupted samples)]

## Implementation Plan

### 1. Unit Tests for PDF Password Protection Detection (FR-015c)
```csharp
[Fact]
public async Task ValidatePdf_PasswordProtected_ReturnsPasswordProtectedError()

[Fact]
public async Task ValidatePdf_PasswordProtectedAES256_ReturnsPasswordProtectedError()

[Fact]
public async Task ValidatePdf_NoPassword_ReturnsValid()

[Fact]
public async Task ValidatePdf_OwnerPasswordOnly_ReturnsPasswordProtectedError()
// Owner password restricts editing but may allow reading
```

### 2. Unit Tests for DOCX Password Protection Detection (FR-015c)
```csharp
[Fact]
public async Task ValidateDocx_PasswordProtected_ReturnsPasswordProtectedError()

[Fact]
public async Task ValidateDocx_EncryptedPackage_ReturnsPasswordProtectedError()

[Fact]
public async Task ValidateDocx_NoPassword_ReturnsValid()

[Fact]
public async Task ValidateDocx_ReadOnlyPassword_ReturnsPasswordProtectedError()
```

### 3. Unit Tests for PDF Corruption Detection (FR-015d)
```csharp
[Fact]
public async Task ValidatePdf_MissingHeader_ReturnsCorruptedError()

[Fact]
public async Task ValidatePdf_MissingEOF_ReturnsCorruptedError()

[Fact]
public async Task ValidatePdf_InvalidXref_ReturnsCorruptedError()

[Fact]
public async Task ValidatePdf_TruncatedFile_ReturnsCorruptedError()

[Fact]
public async Task ValidatePdf_RandomBytes_ReturnsCorruptedError()

[Fact]
public async Task ValidatePdf_ValidHeaderInvalidContent_ReturnsCorruptedError()
```

### 4. Unit Tests for DOCX Corruption Detection (FR-015d)
```csharp
[Fact]
public async Task ValidateDocx_InvalidZipSignature_ReturnsCorruptedError()

[Fact]
public async Task ValidateDocx_MissingContentTypes_ReturnsCorruptedError()

[Fact]
public async Task ValidateDocx_MissingDocumentXml_ReturnsCorruptedError()

[Fact]
public async Task ValidateDocx_CorruptedZipArchive_ReturnsCorruptedError()

[Fact]
public async Task ValidateDocx_TruncatedFile_ReturnsCorruptedError()
```

### 5. Unit Tests for Document Structure Validation (FR-015e)
```csharp
[Fact]
public async Task ValidatePdf_ValidStructure_ReturnsValid()

[Fact]
public async Task ValidatePdf_InvalidTrailer_ReturnsInvalidStructureError()

[Fact]
public async Task ValidateDocx_ValidStructure_ReturnsValid()

[Fact]
public async Task ValidateDocx_MissingRequiredParts_ReturnsInvalidStructureError()

[Fact]
public async Task ValidateDocx_InvalidXmlInDocumentPart_ReturnsInvalidStructureError()
```

### 6. Unit Tests for Empty File Detection (FR-015f)
```csharp
[Fact]
public async Task ValidateFile_ZeroBytes_ReturnsEmptyFileError()

[Fact]
public async Task ValidateFile_OneByte_ReturnsCorruptedError()
// 1 byte is not empty but definitely corrupted

[Fact]
public async Task ValidateFile_MinimalValidPdf_ReturnsValid()

[Fact]
public async Task ValidateFile_MinimalValidDocx_ReturnsValid()
```

### 7. Integration Tests for API Error Responses
```csharp
[Fact]
public async Task UploadDocument_PasswordProtectedPdf_Returns422WithPasswordError()

[Fact]
public async Task UploadDocument_PasswordProtectedDocx_Returns422WithPasswordError()

[Fact]
public async Task UploadDocument_CorruptedPdf_Returns422WithCorruptedError()

[Fact]
public async Task UploadDocument_CorruptedDocx_Returns422WithCorruptedError()

[Fact]
public async Task UploadDocument_EmptyFile_Returns422WithEmptyError()

[Fact]
public async Task UploadDocument_ValidPdf_Returns200Accepted()

[Fact]
public async Task UploadDocument_ValidDocx_Returns200Accepted()
```

### 8. Edge Case Tests
```csharp
[Fact]
public async Task ValidatePdf_PartiallyCorrupted_ReturnsCorruptedError()
// File with valid header but corrupted content

[Fact]
public async Task ValidatePdf_DifferentPdfVersions_HandlesCorrectly()
// PDF 1.4, 1.5, 1.6, 1.7, 2.0

[Fact]
public async Task ValidateDocx_OldWordFormat_ReturnsInvalidExtensionError()
// .doc files should be rejected at extension level

[Fact]
public async Task ValidateFile_LargeValidFile_CompletesWithinTimeout()
// Performance test for 50MB file validation
```

### 9. Test Fixtures Setup
Create test document fixtures:
- `valid.pdf` - Valid PDF document
- `valid.docx` - Valid DOCX document
- `password_protected.pdf` - PDF with user password
- `password_protected.docx` - DOCX with password
- `corrupted_header.pdf` - PDF with invalid header
- `corrupted_zip.docx` - DOCX with corrupted ZIP structure
- `truncated.pdf` - Truncated PDF file
- `empty.pdf` - 0-byte file with .pdf extension

## Current Project State
```
Server/ClinicalIntelligence.Api.Tests/
├── Services/
│   └── DocumentServiceValidationTests.cs  # From US_046
├── Endpoints/
│   └── UploadAcknowledgmentTests.cs
├── Fixtures/
│   └── (test fixtures)
```

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| CREATE | Server/ClinicalIntelligence.Api.Tests/Services/DocumentIntegrityValidatorTests.cs | Unit tests for password protection and corruption detection |
| MODIFY | Server/ClinicalIntelligence.Api.Tests/Endpoints/UploadAcknowledgmentTests.cs | Add integration tests for password-protected and corrupted files |
| CREATE | Server/ClinicalIntelligence.Api.Tests/Fixtures/TestDocuments/ | Directory for test document fixtures |
| CREATE | Server/ClinicalIntelligence.Api.Tests/Helpers/TestDocumentGenerator.cs | Helper to generate test documents programmatically |

## External References
- https://xunit.net/docs/getting-started/netcore/cmdline
- https://learn.microsoft.com/en-us/dotnet/core/testing/unit-testing-best-practices

## Build Commands
- dotnet test Server/ClinicalIntelligence.Api.Tests

## Implementation Validation Strategy
- [Automated] All unit tests pass for PDF password protection detection
- [Automated] All unit tests pass for DOCX password protection detection
- [Automated] All unit tests pass for PDF corruption detection
- [Automated] All unit tests pass for DOCX corruption detection
- [Automated] All unit tests pass for empty file detection
- [Automated] All integration tests pass for API error responses
- [Automated] Code coverage report shows >90% coverage for integrity validation

## Implementation Checklist
- [x] Create TestDocuments fixtures directory
- [x] Create TestDocumentGenerator helper for programmatic test document creation
- [x] Create DocumentIntegrityValidatorTests.cs test file
- [x] Implement PDF password protection detection tests
- [x] Implement DOCX password protection detection tests
- [x] Implement PDF corruption detection tests
- [x] Implement DOCX corruption detection tests
- [x] Implement empty file detection tests
- [x] Add integration tests for API error responses
- [x] Add edge case tests (partial corruption, PDF versions)
- [x] Verify all tests pass
- [x] Generate code coverage report
