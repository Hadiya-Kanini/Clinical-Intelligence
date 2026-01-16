# Task - [TASK_002]

## Requirement Reference
- User Story: [us_051]
- Story Location: [.propel/context/tasks/us_051/us_051.md]
- Acceptance Criteria: 
    - Given a document is uploaded, When processing begins, Then status transitions: Pending → Processing → Completed/Failed (FR-020).
    - Given status changes, When they occur, Then the database is updated with current status.

## Task Overview
Implement comprehensive unit and integration tests for the DocumentStatusService. Tests cover status transition validation, edge cases for stuck processing, and API endpoint behavior.

## Dependent Tasks
- [US_051/task_001] - Backend document status service implementation

## Impacted Components
- [CREATE | Server/ClinicalIntelligence.Api.Tests/Services/DocumentStatusServiceTests.cs | Unit tests for status service]
- [CREATE | Server/ClinicalIntelligence.Api.Tests/Endpoints/DocumentStatusEndpointTests.cs | Integration tests for status endpoint]

## Implementation Plan

### 1. Unit Tests for DocumentStatusService
```csharp
namespace ClinicalIntelligence.Api.Tests.Services;

public class DocumentStatusServiceTests
{
    [Theory]
    [InlineData(DocumentStatus.Pending, DocumentStatus.Processing, true)]
    [InlineData(DocumentStatus.Pending, DocumentStatus.Failed, true)]
    [InlineData(DocumentStatus.Pending, DocumentStatus.ValidationFailed, true)]
    [InlineData(DocumentStatus.Processing, DocumentStatus.Completed, true)]
    [InlineData(DocumentStatus.Processing, DocumentStatus.Failed, true)]
    [InlineData(DocumentStatus.Completed, DocumentStatus.Processing, false)]
    [InlineData(DocumentStatus.Failed, DocumentStatus.Pending, true)] // Retry allowed
    [InlineData(DocumentStatus.ValidationFailed, DocumentStatus.Pending, false)]
    public void IsValidTransition_ReturnsExpectedResult(
        DocumentStatus current, 
        DocumentStatus target, 
        bool expected)
    {
        // Arrange
        var service = CreateService();
        
        // Act
        var result = service.IsValidTransition(current, target);
        
        // Assert
        Assert.Equal(expected, result);
    }
    
    [Fact]
    public async Task GetStatusAsync_ExistingDocument_ReturnsStatus()
    {
        // Arrange
        var documentId = Guid.NewGuid();
        var dbContext = CreateInMemoryDbContext();
        await SeedDocument(dbContext, documentId, "Pending");
        var service = CreateService(dbContext);
        
        // Act
        var result = await service.GetStatusAsync(documentId);
        
        // Assert
        Assert.NotNull(result);
        Assert.Equal(documentId, result.DocumentId);
        Assert.Equal("Pending", result.Status);
    }
    
    [Fact]
    public async Task GetStatusAsync_DeletedDocument_ReturnsNull()
    {
        // Arrange
        var documentId = Guid.NewGuid();
        var dbContext = CreateInMemoryDbContext();
        await SeedDocument(dbContext, documentId, "Pending", isDeleted: true);
        var service = CreateService(dbContext);
        
        // Act
        var result = await service.GetStatusAsync(documentId);
        
        // Assert
        Assert.Null(result);
    }
    
    [Fact]
    public async Task UpdateStatusAsync_ValidTransition_UpdatesDatabase()
    {
        // Arrange
        var documentId = Guid.NewGuid();
        var dbContext = CreateInMemoryDbContext();
        await SeedDocument(dbContext, documentId, "Pending");
        var service = CreateService(dbContext);
        
        // Act
        var result = await service.UpdateStatusAsync(documentId, DocumentStatus.Processing);
        
        // Assert
        Assert.Equal("Processing", result.Status);
        
        var document = await dbContext.Documents.FindAsync(documentId);
        Assert.Equal("Processing", document!.Status);
    }
    
    [Fact]
    public async Task UpdateStatusAsync_InvalidTransition_ThrowsException()
    {
        // Arrange
        var documentId = Guid.NewGuid();
        var dbContext = CreateInMemoryDbContext();
        await SeedDocument(dbContext, documentId, "Completed");
        var service = CreateService(dbContext);
        
        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.UpdateStatusAsync(documentId, DocumentStatus.Processing));
    }
    
    [Fact]
    public async Task GetStatusBatchAsync_MultipleDocuments_ReturnsAll()
    {
        // Arrange
        var dbContext = CreateInMemoryDbContext();
        var ids = new[] { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() };
        await SeedDocument(dbContext, ids[0], "Pending");
        await SeedDocument(dbContext, ids[1], "Processing");
        await SeedDocument(dbContext, ids[2], "Completed");
        var service = CreateService(dbContext);
        
        // Act
        var results = await service.GetStatusBatchAsync(ids);
        
        // Assert
        Assert.Equal(3, results.Count);
    }
}
```

### 2. Integration Tests for Status Endpoint
```csharp
namespace ClinicalIntelligence.Api.Tests.Endpoints;

public class DocumentStatusEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    [Fact]
    public async Task GetDocumentStatus_ValidDocument_ReturnsOk()
    {
        // Arrange
        var client = CreateAuthenticatedClient();
        var documentId = await CreateTestDocument();
        
        // Act
        var response = await client.GetAsync($"/api/v1/documents/{documentId}/status");
        
        // Assert
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<DocumentStatusResult>();
        Assert.Equal("Pending", result!.Status);
    }
    
    [Fact]
    public async Task GetDocumentStatus_NonExistentDocument_ReturnsNotFound()
    {
        // Arrange
        var client = CreateAuthenticatedClient();
        var documentId = Guid.NewGuid();
        
        // Act
        var response = await client.GetAsync($"/api/v1/documents/{documentId}/status");
        
        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
    
    [Fact]
    public async Task GetDocumentStatus_Unauthorized_Returns401()
    {
        // Arrange
        var client = CreateUnauthenticatedClient();
        var documentId = Guid.NewGuid();
        
        // Act
        var response = await client.GetAsync($"/api/v1/documents/{documentId}/status");
        
        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
```

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| CREATE | Server/ClinicalIntelligence.Api.Tests/Services/DocumentStatusServiceTests.cs | Unit tests |
| CREATE | Server/ClinicalIntelligence.Api.Tests/Endpoints/DocumentStatusEndpointTests.cs | Integration tests |

## Build Commands
- dotnet test Server/ClinicalIntelligence.Api.Tests --filter "FullyQualifiedName~DocumentStatus"

## Implementation Validation Strategy
- [Automated] All unit tests pass
- [Automated] All integration tests pass
- [Automated] Code coverage >= 80% for DocumentStatusService

## Implementation Checklist
- [x] Create DocumentStatusServiceTests with transition validation tests
- [x] Add tests for GetStatusAsync with various scenarios
- [x] Add tests for UpdateStatusAsync with valid/invalid transitions
- [x] Add tests for GetStatusBatchAsync
- [x] Create DocumentStatusEndpointTests for API integration
- [x] Verify all tests pass
