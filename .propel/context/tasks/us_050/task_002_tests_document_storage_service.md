# Task - [TASK_002]

## Requirement Reference
- User Story: [us_050]
- Story Location: [.propel/context/tasks/us_050/us_050.md]
- Acceptance Criteria: 
    - Given a document is uploaded, When stored, Then it is saved to the configured file system path (FR-021).
    - Given document storage, When saved, Then the storage path is recorded in the database with document metadata.
    - Given the storage structure, When organized, Then it follows the pattern: {tenant_id}/{patient_id}/{document_id}/original.{ext}.
    - Given document retrieval, When requested, Then the file can be loaded using the stored path.

## Task Overview
Create comprehensive unit and integration tests for the document storage service. Tests must verify storage path pattern generation, file operations (store, retrieve, delete), directory management, error handling, and integration with the document upload pipeline.

## Dependent Tasks
- [US_050/task_001] - Backend document storage service implementation

## Impacted Components
- [CREATE | Server/ClinicalIntelligence.Api.Tests/Services/LocalFileStorageServiceTests.cs | Unit tests for storage service]
- [CREATE | Server/ClinicalIntelligence.Api.Tests/Services/DocumentStorageIntegrationTests.cs | Integration tests for storage pipeline]
- [CREATE | Server/ClinicalIntelligence.Api.Tests/Endpoints/DocumentRetrievalEndpointTests.cs | Tests for document retrieval API]

## Implementation Plan

### 1. Storage Path Pattern Tests
```csharp
public class StoragePathPatternTests
{
    [Fact]
    public void BuildStoragePath_FollowsPattern_TenantPatientDocumentOriginal()
    
    [Fact]
    public void BuildStoragePath_PreservesFileExtension()
    
    [Theory]
    [InlineData(".pdf")]
    [InlineData(".docx")]
    public void BuildStoragePath_SupportsAllowedExtensions(string extension)
    
    [Fact]
    public void BuildStoragePath_UsesDefaultTenantId()
    
    [Fact]
    public void BuildStoragePath_IncludesPatientId()
    
    [Fact]
    public void BuildStoragePath_IncludesDocumentId()
}
```

### 2. File Store Operation Tests
```csharp
public class LocalFileStorageServiceStoreTests : IDisposable
{
    private readonly string _testBasePath;
    
    [Fact]
    public async Task StoreAsync_CreatesFile_AtCorrectPath()
    
    [Fact]
    public async Task StoreAsync_CreatesDirectoryStructure()
    
    [Fact]
    public async Task StoreAsync_WritesCorrectContent()
    
    [Fact]
    public async Task StoreAsync_ReturnsCorrectBytesWritten()
    
    [Fact]
    public async Task StoreAsync_ReturnsRelativeStoragePath()
    
    [Fact]
    public async Task StoreAsync_ReturnsAbsolutePath()
    
    [Fact]
    public async Task StoreAsync_HandlesLargeFiles()
    
    [Fact]
    public async Task StoreAsync_FailsGracefully_OnDiskFull()
    
    [Fact]
    public async Task StoreAsync_FailsGracefully_OnPermissionDenied()
}
```

### 3. File Retrieve Operation Tests
```csharp
public class LocalFileStorageServiceRetrieveTests : IDisposable
{
    [Fact]
    public async Task RetrieveAsync_ReturnsFileStream_WhenExists()
    
    [Fact]
    public async Task RetrieveAsync_ReturnsNull_WhenNotExists()
    
    [Fact]
    public async Task RetrieveAsync_StreamContainsCorrectContent()
    
    [Fact]
    public async Task RetrieveAsync_StreamIsReadable()
    
    [Fact]
    public async Task RetrieveAsync_HandlesLargeFiles()
}
```

### 4. File Delete Operation Tests
```csharp
public class LocalFileStorageServiceDeleteTests : IDisposable
{
    [Fact]
    public async Task DeleteAsync_RemovesFile_WhenExists()
    
    [Fact]
    public async Task DeleteAsync_ReturnsFalse_WhenNotExists()
    
    [Fact]
    public async Task DeleteAsync_CleansUpEmptyDirectories()
    
    [Fact]
    public async Task DeleteAsync_DoesNotDeleteNonEmptyDirectories()
    
    [Fact]
    public async Task DeleteAsync_DoesNotDeleteBeyondBasePath()
}
```

### 5. Directory Management Tests
```csharp
public class DirectoryManagementTests : IDisposable
{
    [Fact]
    public void Constructor_CreatesBaseDirectory_IfNotExists()
    
    [Fact]
    public void Constructor_CreatesTempDirectory_IfNotExists()
    
    [Fact]
    public async Task StoreAsync_CreatesNestedDirectories()
    
    [Fact]
    public async Task CleanupEmptyDirectories_RemovesEmptyParents()
    
    [Fact]
    public async Task CleanupEmptyDirectories_StopsAtBasePath()
}
```

### 6. Integration Tests
```csharp
public class DocumentStorageIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    [Fact]
    public async Task UploadDocument_StoresFileToFileSystem()
    
    [Fact]
    public async Task UploadDocument_RecordsStoragePathInDatabase()
    
    [Fact]
    public async Task UploadDocument_StoragePathFollowsPattern()
    
    [Fact]
    public async Task RetrieveDocument_ReturnsStoredFile()
    
    [Fact]
    public async Task RetrieveDocument_ReturnsCorrectMimeType()
    
    [Fact]
    public async Task RetrieveDocument_ReturnsCorrectFileName()
    
    [Fact]
    public async Task RetrieveDocument_NotFound_Returns404()
    
    [Fact]
    public async Task RetrieveDocument_Unauthorized_Returns401()
}
```

### 7. Document Retrieval Endpoint Tests
```csharp
public class DocumentRetrievalEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    [Fact]
    public async Task GetDocumentContent_ValidId_ReturnsFile()
    
    [Fact]
    public async Task GetDocumentContent_InvalidId_Returns404()
    
    [Fact]
    public async Task GetDocumentContent_DeletedDocument_Returns404()
    
    [Fact]
    public async Task GetDocumentContent_MissingFile_Returns404()
    
    [Fact]
    public async Task GetDocumentContent_SetsCorrectContentType()
    
    [Fact]
    public async Task GetDocumentContent_SetsContentDisposition()
}
```

### 8. Edge Case Tests
```csharp
public class DocumentStorageEdgeCaseTests : IDisposable
{
    [Fact]
    public async Task StoreAsync_HandlesSpecialCharactersInFileName()
    
    [Fact]
    public async Task StoreAsync_HandlesUnicodeFileName()
    
    [Fact]
    public async Task StoreAsync_HandlesVeryLongFileName()
    
    [Fact]
    public async Task ExistsAsync_ReturnsTrue_WhenFileExists()
    
    [Fact]
    public async Task ExistsAsync_ReturnsFalse_WhenFileNotExists()
    
    [Fact]
    public async Task StoreAsync_OverwritesExistingFile()
}
```

### 9. Test Helpers
```csharp
public static class DocumentStorageTestHelpers
{
    public static string CreateTempTestDirectory()
    {
        var path = Path.Combine(Path.GetTempPath(), $"ci_test_{Guid.NewGuid()}");
        Directory.CreateDirectory(path);
        return path;
    }
    
    public static void CleanupTestDirectory(string path)
    {
        if (Directory.Exists(path))
        {
            Directory.Delete(path, recursive: true);
        }
    }
    
    public static Stream CreateTestFileStream(int sizeBytes = 1024)
    {
        var content = new byte[sizeBytes];
        new Random().NextBytes(content);
        return new MemoryStream(content);
    }
    
    public static DocumentStorageOptions CreateTestOptions(string basePath)
    {
        return new DocumentStorageOptions
        {
            BasePath = basePath,
            TempPath = Path.Combine(basePath, "temp"),
            DefaultTenantId = "test-tenant"
        };
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
| CREATE | Server/ClinicalIntelligence.Api.Tests/Services/LocalFileStorageServiceTests.cs | Unit tests for storage service operations |
| CREATE | Server/ClinicalIntelligence.Api.Tests/Services/DocumentStorageIntegrationTests.cs | Integration tests for storage pipeline |
| CREATE | Server/ClinicalIntelligence.Api.Tests/Endpoints/DocumentRetrievalEndpointTests.cs | Tests for document retrieval API |
| CREATE | Server/ClinicalIntelligence.Api.Tests/Helpers/DocumentStorageTestHelpers.cs | Test helper methods for storage tests |

## External References
- https://xunit.net/docs/getting-started/netcore/cmdline
- https://learn.microsoft.com/en-us/dotnet/api/system.io.path.gettemppath

## Build Commands
- dotnet build Server/ClinicalIntelligence.Api.Tests
- dotnet test Server/ClinicalIntelligence.Api.Tests --filter "FullyQualifiedName~Storage"

## Implementation Validation Strategy
- [Automated] All unit tests pass with isolated test directories
- [Automated] Integration tests verify end-to-end storage flow
- [Automated] Code coverage >= 80% for storage components
- [Manual] Verify storage directory structure on file system

## Implementation Checklist
- [x] Create DocumentStorageTestHelpers with temp directory management
- [x] Implement StoragePathPatternTests for path generation
- [x] Implement LocalFileStorageServiceStoreTests for write operations
- [x] Implement LocalFileStorageServiceRetrieveTests for read operations
- [x] Implement LocalFileStorageServiceDeleteTests for delete operations
- [x] Implement DirectoryManagementTests for directory operations
- [x] Implement DocumentStorageIntegrationTests for pipeline
- [x] Implement DocumentRetrievalEndpointTests for API
- [x] Verify all tests pass with `dotnet test`
