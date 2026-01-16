# Task - [TASK_003]

## Requirement Reference
- User Story: [us_049]
- Story Location: [.propel/context/tasks/us_049/us_049.md]
- Acceptance Criteria: 
    - Given a batch upload, When more than 10 files are selected, Then only the first 10 are accepted.
    - Given batch limit is exceeded, When detected, Then a warning is displayed about the remaining files.
    - Given the API, When more than 10 files are submitted, Then excess files are rejected with clear error.

## Task Overview
Create comprehensive unit and integration tests for the batch upload limit functionality. Tests must verify the 10-file limit enforcement, warning message generation, per-file result tracking, and API response structure.

## Dependent Tasks
- [US_049/task_001] - Backend batch upload limit enforcement
- [US_049/task_002] - Frontend batch upload limit UI

## Impacted Components
- [CREATE | Server/ClinicalIntelligence.Api.Tests/Services/BatchUploadServiceTests.cs | Unit tests for batch upload service]
- [CREATE | Server/ClinicalIntelligence.Api.Tests/Endpoints/BatchUploadEndpointTests.cs | Integration tests for batch upload API]

## Implementation Plan

### 1. BatchUploadService Unit Tests
```csharp
public class BatchUploadServiceTests
{
    [Fact]
    public async Task ProcessBatch_ExactlyTenFiles_AcceptsAllFiles()
    
    [Fact]
    public async Task ProcessBatch_LessThanTenFiles_AcceptsAllFiles()
    
    [Fact]
    public async Task ProcessBatch_MoreThanTenFiles_AcceptsFirstTenOnly()
    
    [Fact]
    public async Task ProcessBatch_FifteenFiles_RejectsFiveFiles()
    
    [Fact]
    public async Task ProcessBatch_ExcessFiles_MarkedAsBatchLimitExceeded()
    
    [Fact]
    public async Task ProcessBatch_ExcessFiles_HaveCorrectRejectionReason()
    
    [Fact]
    public async Task ProcessBatch_CreatesDocumentBatchEntity()
    
    [Fact]
    public async Task ProcessBatch_LinksDocumentsToBatch()
    
    [Fact]
    public async Task ProcessBatch_ReturnsBatchLimitExceededTrue_WhenOverLimit()
    
    [Fact]
    public async Task ProcessBatch_ReturnsBatchLimitExceededFalse_WhenWithinLimit()
    
    [Fact]
    public async Task ProcessBatch_GeneratesWarningMessage_WhenOverLimit()
    
    [Fact]
    public async Task ProcessBatch_NoWarningMessage_WhenWithinLimit()
}
```

### 2. Batch Upload Response Tests
```csharp
public class BatchUploadResponseTests
{
    [Fact]
    public void Response_ContainsTotalFilesReceived()
    
    [Fact]
    public void Response_ContainsFilesAcceptedCount()
    
    [Fact]
    public void Response_ContainsFilesRejectedCount()
    
    [Fact]
    public void Response_FileResultsMatchTotalReceived()
    
    [Fact]
    public void Response_AcceptedFilesHaveDocumentId()
    
    [Fact]
    public void Response_RejectedFilesHaveNoDocumentId()
}
```

### 3. Integration Tests
```csharp
public class BatchUploadEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    [Fact]
    public async Task BatchUpload_TenValidFiles_ReturnsOkWithAllAccepted()
    
    [Fact]
    public async Task BatchUpload_FifteenFiles_ReturnsOkWithWarning()
    
    [Fact]
    public async Task BatchUpload_FifteenFiles_AcceptsFirstTen()
    
    [Fact]
    public async Task BatchUpload_FifteenFiles_RejectsLastFive()
    
    [Fact]
    public async Task BatchUpload_NoFiles_ReturnsBadRequest()
    
    [Fact]
    public async Task BatchUpload_SingleFile_ReturnsOk()
    
    [Fact]
    public async Task BatchUpload_MixedValidInvalid_ProcessesCorrectly()
    
    [Fact]
    public async Task BatchUpload_Unauthorized_Returns401()
    
    [Fact]
    public async Task BatchUpload_InvalidPatientId_ReturnsBadRequest()
    
    [Fact]
    public async Task BatchUpload_CreatesBatchInDatabase()
    
    [Fact]
    public async Task BatchUpload_LinksDocumentsToBatchInDatabase()
}
```

### 4. Edge Case Tests
```csharp
public class BatchUploadEdgeCaseTests
{
    [Fact]
    public async Task ProcessBatch_ExactlyElevenFiles_RejectsOneFile()
    
    [Fact]
    public async Task ProcessBatch_ZeroFiles_ReturnsEmptyResults()
    
    [Fact]
    public async Task ProcessBatch_AllFilesInvalid_ReturnsAllRejected()
    
    [Fact]
    public async Task ProcessBatch_SomeFilesInvalid_ProcessesValidOnes()
    
    [Fact]
    public async Task ProcessBatch_TwentyFiles_RejectsTenFiles()
}
```

### 5. Warning Message Tests
```csharp
public class BatchLimitWarningTests
{
    [Theory]
    [InlineData(11, 10, 1)]
    [InlineData(15, 10, 5)]
    [InlineData(20, 10, 10)]
    public void GenerateWarning_CorrectCounts(int total, int accepted, int rejected)
    
    [Fact]
    public void GenerateWarning_ContainsAcceptedCount()
    
    [Fact]
    public void GenerateWarning_ContainsRejectedCount()
    
    [Fact]
    public void GenerateWarning_SuggestsSeparateBatch()
}
```

### 6. Test Helpers
```csharp
public static class BatchUploadTestHelpers
{
    public static IFormFileCollection CreateMockFiles(int count, string extension = ".pdf")
    {
        var files = new FormFileCollection();
        for (int i = 0; i < count; i++)
        {
            files.Add(CreateMockFile($"file{i + 1}{extension}"));
        }
        return files;
    }
    
    public static IFormFile CreateMockFile(string fileName, byte[]? content = null)
    {
        content ??= CreateValidPdfContent();
        var stream = new MemoryStream(content);
        return new FormFile(stream, 0, content.Length, "file", fileName)
        {
            Headers = new HeaderDictionary(),
            ContentType = GetContentType(fileName)
        };
    }
    
    public static MultipartFormDataContent CreateBatchUploadContent(
        Guid patientId, 
        int fileCount)
    {
        var content = new MultipartFormDataContent();
        content.Add(new StringContent(patientId.ToString()), "patientId");
        
        for (int i = 0; i < fileCount; i++)
        {
            var fileContent = new ByteArrayContent(CreateValidPdfContent());
            fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");
            content.Add(fileContent, "files", $"file{i + 1}.pdf");
        }
        
        return content;
    }
}
```

## Current Project State
```
Server/ClinicalIntelligence.Api.Tests/
├── Services/
│   └── (existing service tests)
├── Endpoints/
│   └── UploadAcknowledgmentTests.cs
├── Helpers/
│   └── (existing test helpers)
```

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| CREATE | Server/ClinicalIntelligence.Api.Tests/Services/BatchUploadServiceTests.cs | Unit tests for batch upload service |
| CREATE | Server/ClinicalIntelligence.Api.Tests/Endpoints/BatchUploadEndpointTests.cs | Integration tests for batch upload API |
| CREATE | Server/ClinicalIntelligence.Api.Tests/Helpers/BatchUploadTestHelpers.cs | Test helper methods for batch uploads |

## External References
- https://xunit.net/docs/getting-started/netcore/cmdline
- https://learn.microsoft.com/en-us/aspnet/core/test/integration-tests

## Build Commands
- dotnet build Server/ClinicalIntelligence.Api.Tests
- dotnet test Server/ClinicalIntelligence.Api.Tests --filter "FullyQualifiedName~BatchUpload"

## Implementation Validation Strategy
- [Automated] All unit tests pass with mocked dependencies
- [Automated] Integration tests verify end-to-end batch upload flow
- [Automated] Code coverage >= 80% for batch upload components

## Implementation Checklist
- [x] Create BatchUploadTestHelpers with mock file factories
- [x] Implement BatchUploadServiceTests for limit enforcement
- [x] Implement BatchUploadResponseTests for response structure
- [x] Implement BatchUploadEndpointTests for API integration
- [x] Implement BatchUploadEdgeCaseTests for boundary conditions
- [x] Implement BatchLimitWarningTests for message generation
- [x] Verify all tests pass with `dotnet test`
