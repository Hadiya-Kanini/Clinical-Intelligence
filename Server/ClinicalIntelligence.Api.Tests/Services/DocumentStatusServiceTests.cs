using ClinicalIntelligence.Api.Data;
using ClinicalIntelligence.Api.Domain.Enums;
using ClinicalIntelligence.Api.Domain.Models;
using ClinicalIntelligence.Api.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace ClinicalIntelligence.Api.Tests.Services;

public class DocumentStatusServiceTests
{
    [Theory]
    [InlineData(DocumentStatus.Pending, DocumentStatus.Processing, true)]
    [InlineData(DocumentStatus.Pending, DocumentStatus.Failed, true)]
    [InlineData(DocumentStatus.Pending, DocumentStatus.ValidationFailed, true)]
    [InlineData(DocumentStatus.Processing, DocumentStatus.Completed, true)]
    [InlineData(DocumentStatus.Processing, DocumentStatus.Failed, true)]
    [InlineData(DocumentStatus.Processing, DocumentStatus.ValidationFailed, true)]
    [InlineData(DocumentStatus.Completed, DocumentStatus.Processing, false)]
    [InlineData(DocumentStatus.Completed, DocumentStatus.Pending, false)]
    [InlineData(DocumentStatus.Failed, DocumentStatus.Pending, true)]
    [InlineData(DocumentStatus.Failed, DocumentStatus.Processing, false)]
    [InlineData(DocumentStatus.ValidationFailed, DocumentStatus.Pending, false)]
    [InlineData(DocumentStatus.ValidationFailed, DocumentStatus.Processing, false)]
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
    public async Task GetStatusAsync_NonExistentDocument_ReturnsNull()
    {
        // Arrange
        var documentId = Guid.NewGuid();
        var dbContext = CreateInMemoryDbContext();
        var service = CreateService(dbContext);
        
        // Act
        var result = await service.GetStatusAsync(documentId);
        
        // Assert
        Assert.Null(result);
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
        
        var document = await dbContext.Documents
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(d => d.Id == documentId);
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
    public async Task UpdateStatusAsync_NonExistentDocument_ThrowsException()
    {
        // Arrange
        var documentId = Guid.NewGuid();
        var dbContext = CreateInMemoryDbContext();
        var service = CreateService(dbContext);
        
        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.UpdateStatusAsync(documentId, DocumentStatus.Processing));
        Assert.Contains(documentId.ToString(), exception.Message);
    }
    
    [Fact]
    public async Task UpdateStatusAsync_WithErrorMessage_IncludesErrorInResult()
    {
        // Arrange
        var documentId = Guid.NewGuid();
        var dbContext = CreateInMemoryDbContext();
        await SeedDocument(dbContext, documentId, "Processing");
        var service = CreateService(dbContext);
        var errorMessage = "Processing failed due to invalid format";
        
        // Act
        var result = await service.UpdateStatusAsync(documentId, DocumentStatus.Failed, errorMessage);
        
        // Assert
        Assert.Equal("Failed", result.Status);
        Assert.Equal(errorMessage, result.ErrorMessage);
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
        Assert.Contains(results, r => r.Status == "Pending");
        Assert.Contains(results, r => r.Status == "Processing");
        Assert.Contains(results, r => r.Status == "Completed");
    }
    
    [Fact]
    public async Task GetStatusBatchAsync_EmptyList_ReturnsEmpty()
    {
        // Arrange
        var dbContext = CreateInMemoryDbContext();
        var service = CreateService(dbContext);
        
        // Act
        var results = await service.GetStatusBatchAsync(Array.Empty<Guid>());
        
        // Assert
        Assert.Empty(results);
    }
    
    [Fact]
    public async Task GetStatusBatchAsync_ExcludesDeletedDocuments()
    {
        // Arrange
        var dbContext = CreateInMemoryDbContext();
        var ids = new[] { Guid.NewGuid(), Guid.NewGuid() };
        await SeedDocument(dbContext, ids[0], "Pending");
        await SeedDocument(dbContext, ids[1], "Processing", isDeleted: true);
        var service = CreateService(dbContext);
        
        // Act
        var results = await service.GetStatusBatchAsync(ids);
        
        // Assert
        Assert.Single(results);
        Assert.Equal(ids[0], results[0].DocumentId);
    }
    
    [Fact]
    public async Task UpdateStatusAsync_FailedToPending_AllowsRetry()
    {
        // Arrange
        var documentId = Guid.NewGuid();
        var dbContext = CreateInMemoryDbContext();
        await SeedDocument(dbContext, documentId, "Failed");
        var service = CreateService(dbContext);
        
        // Act
        var result = await service.UpdateStatusAsync(documentId, DocumentStatus.Pending);
        
        // Assert
        Assert.Equal("Pending", result.Status);
    }
    
    [Fact]
    public void IsValidTransition_CompletedIsTerminal_NoTransitionsAllowed()
    {
        // Arrange
        var service = CreateService();
        
        // Act & Assert
        Assert.False(service.IsValidTransition(DocumentStatus.Completed, DocumentStatus.Pending));
        Assert.False(service.IsValidTransition(DocumentStatus.Completed, DocumentStatus.Processing));
        Assert.False(service.IsValidTransition(DocumentStatus.Completed, DocumentStatus.Failed));
        Assert.False(service.IsValidTransition(DocumentStatus.Completed, DocumentStatus.ValidationFailed));
    }
    
    [Fact]
    public void IsValidTransition_ValidationFailedIsTerminal_NoTransitionsAllowed()
    {
        // Arrange
        var service = CreateService();
        
        // Act & Assert
        Assert.False(service.IsValidTransition(DocumentStatus.ValidationFailed, DocumentStatus.Pending));
        Assert.False(service.IsValidTransition(DocumentStatus.ValidationFailed, DocumentStatus.Processing));
        Assert.False(service.IsValidTransition(DocumentStatus.ValidationFailed, DocumentStatus.Completed));
        Assert.False(service.IsValidTransition(DocumentStatus.ValidationFailed, DocumentStatus.Failed));
    }
    
    private static DocumentStatusService CreateService(ApplicationDbContext? dbContext = null)
    {
        dbContext ??= CreateInMemoryDbContext();
        var logger = new Mock<ILogger<DocumentStatusService>>();
        return new DocumentStatusService(dbContext, logger.Object);
    }
    
    private static ApplicationDbContext CreateInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new ApplicationDbContext(options);
    }
    
    private static async Task SeedDocument(
        ApplicationDbContext dbContext, 
        Guid documentId, 
        string status, 
        bool isDeleted = false)
    {
        var patientId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        
        // Seed required patient
        dbContext.ErdPatients.Add(new ErdPatient
        {
            Id = patientId,
            Mrn = $"MRN-{patientId:N}".Substring(0, 20),
            Name = "Test Patient",
            Dob = new DateOnly(1990, 1, 1),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });
        
        // Seed required user
        dbContext.Users.Add(new User
        {
            Id = userId,
            Email = $"user-{userId}@test.com",
            Name = "Test User",
            PasswordHash = "hash",
            Role = "Standard",
            Status = "Active",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });
        
        dbContext.Documents.Add(new Document
        {
            Id = documentId,
            PatientId = patientId,
            UploadedByUserId = userId,
            OriginalName = "test-document.pdf",
            MimeType = "application/pdf",
            SizeBytes = 1024,
            StoragePath = $"test/{documentId}/original.pdf",
            Status = status,
            IsDeleted = isDeleted,
            DeletedAt = isDeleted ? DateTime.UtcNow : null,
            UploadedAt = DateTime.UtcNow
        });
        
        await dbContext.SaveChangesAsync();
    }
}
