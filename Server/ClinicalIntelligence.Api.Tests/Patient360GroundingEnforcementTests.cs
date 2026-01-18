using ClinicalIntelligence.Api.Data;
using ClinicalIntelligence.Api.Domain.Models;
using ClinicalIntelligence.Api.Services.Entities;
using ClinicalIntelligence.Api.Services.Patients;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace ClinicalIntelligence.Api.Tests;

/// <summary>
/// Tests validating Patient 360 API only returns grounded entities.
/// Enforces FR-051, FR-056: 100% grounding requirement.
/// </summary>
public class Patient360GroundingEnforcementTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly EntityCitationReader _citationReader;
    private readonly Patient360Reader _patient360Reader;
    private readonly User _testUser;
    private readonly ErdPatient _testPatient;
    private readonly Document _testDocument;
    private readonly DocumentChunk _testChunk;

    public Patient360GroundingEnforcementTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _dbContext = new ApplicationDbContext(options);
        _citationReader = new EntityCitationReader(_dbContext);
        
        var logger = Mock.Of<ILogger<Patient360Reader>>();
        _patient360Reader = new Patient360Reader(_dbContext, _citationReader, logger);

        // Create test user
        _testUser = new User
        {
            Id = Guid.NewGuid(),
            Email = "test@example.com",
            PasswordHash = "hash",
            Name = "Test User",
            Role = "Standard",
            Status = "Active",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _dbContext.Users.Add(_testUser);

        // Create test patient
        _testPatient = new ErdPatient
        {
            Id = Guid.NewGuid(),
            Mrn = "MRN-360-TEST",
            Name = "Patient 360 Test",
            Dob = new DateOnly(1985, 6, 15),
            Address = "123 Test St",
            Contact = "555-1234",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _dbContext.ErdPatients.Add(_testPatient);

        // Create test document
        _testDocument = new Document
        {
            Id = Guid.NewGuid(),
            PatientId = _testPatient.Id,
            UploadedByUserId = _testUser.Id,
            OriginalName = "patient-record.pdf",
            MimeType = "application/pdf",
            SizeBytes = 2048,
            StoragePath = "/test/path/patient-record.pdf",
            Status = "Completed",
            UploadedAt = DateTime.UtcNow
        };
        _dbContext.Documents.Add(_testDocument);

        // Create test document chunk
        _testChunk = new DocumentChunk
        {
            Id = Guid.NewGuid(),
            DocumentId = _testDocument.Id,
            Page = 1,
            Section = "Demographics",
            Coordinates = "{\"x\":10,\"y\":20,\"width\":100,\"height\":15}",
            TextContent = "Patient: Patient 360 Test, DOB: 06/15/1985",
            ChunkHash = "chunk123"
        };
        _dbContext.DocumentChunks.Add(_testChunk);

        _dbContext.SaveChanges();
    }

    public void Dispose()
    {
        _dbContext.Dispose();
    }

    #region Grounding Enforcement Tests

    [Fact]
    public async Task GetPatient360Async_OnlyReturnsGroundedEntities()
    {
        // Arrange - Create one grounded and one ungrounded entity
        var groundedEntity = new ExtractedEntity
        {
            Id = Guid.NewGuid(),
            PatientId = _testPatient.Id,
            DocumentId = _testDocument.Id,
            Category = "Demographics",
            Name = "patient_name",
            Value = "Patient 360 Test"
        };
        var ungroundedEntity = new ExtractedEntity
        {
            Id = Guid.NewGuid(),
            PatientId = _testPatient.Id,
            DocumentId = _testDocument.Id,
            Category = "Diagnosis",
            Name = "condition",
            Value = "Ungrounded Condition"
        };
        _dbContext.ExtractedEntities.AddRange(groundedEntity, ungroundedEntity);

        // Add citation only for grounded entity
        var citation = new EntityCitation
        {
            Id = Guid.NewGuid(),
            ExtractedEntityId = groundedEntity.Id,
            DocumentChunkId = _testChunk.Id,
            Page = 1,
            Section = "Demographics",
            CitedText = "Patient: Patient 360 Test"
        };
        _dbContext.EntityCitations.Add(citation);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _patient360Reader.GetPatient360Async(_testPatient.Id);

        // Assert - Only grounded entity should be returned
        Assert.NotNull(result);
        Assert.Single(result.Entities);
        Assert.Equal(groundedEntity.Id, result.Entities[0].Id);
        Assert.True(result.Entities[0].IsGrounded);
        Assert.Single(result.Entities[0].Citations);
    }

    [Fact]
    public async Task GetPatient360Async_ExcludesAllUngroundedEntities()
    {
        // Arrange - Create multiple ungrounded entities
        var entities = new[]
        {
            new ExtractedEntity
            {
                Id = Guid.NewGuid(),
                PatientId = _testPatient.Id,
                DocumentId = _testDocument.Id,
                Category = "Diagnosis",
                Name = "condition1",
                Value = "Ungrounded 1"
            },
            new ExtractedEntity
            {
                Id = Guid.NewGuid(),
                PatientId = _testPatient.Id,
                DocumentId = _testDocument.Id,
                Category = "Medication",
                Name = "drug1",
                Value = "Ungrounded 2"
            },
            new ExtractedEntity
            {
                Id = Guid.NewGuid(),
                PatientId = _testPatient.Id,
                DocumentId = _testDocument.Id,
                Category = "Allergy",
                Name = "allergen1",
                Value = "Ungrounded 3"
            }
        };
        _dbContext.ExtractedEntities.AddRange(entities);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _patient360Reader.GetPatient360Async(_testPatient.Id);

        // Assert - No entities should be returned (all ungrounded)
        Assert.NotNull(result);
        Assert.Empty(result.Entities);
        Assert.Equal(0, result.EntityCount);
    }

    [Fact]
    public async Task GetPatient360Async_IncludesAllCitationsForGroundedEntity()
    {
        // Arrange - Create entity with multiple citations
        var entity = new ExtractedEntity
        {
            Id = Guid.NewGuid(),
            PatientId = _testPatient.Id,
            DocumentId = _testDocument.Id,
            Category = "Medication",
            Name = "drug",
            Value = "Aspirin"
        };
        _dbContext.ExtractedEntities.Add(entity);

        // Create second chunk
        var chunk2 = new DocumentChunk
        {
            Id = Guid.NewGuid(),
            DocumentId = _testDocument.Id,
            Page = 2,
            Section = "Medications",
            TextContent = "Current medications: Aspirin 81mg daily",
            ChunkHash = "chunk456"
        };
        _dbContext.DocumentChunks.Add(chunk2);

        // Add multiple citations
        var citation1 = new EntityCitation
        {
            Id = Guid.NewGuid(),
            ExtractedEntityId = entity.Id,
            DocumentChunkId = _testChunk.Id,
            Page = 1,
            Section = "Summary",
            CitedText = "Medications: Aspirin"
        };
        var citation2 = new EntityCitation
        {
            Id = Guid.NewGuid(),
            ExtractedEntityId = entity.Id,
            DocumentChunkId = chunk2.Id,
            Page = 2,
            Section = "Medications",
            CitedText = "Aspirin 81mg daily"
        };
        _dbContext.EntityCitations.AddRange(citation1, citation2);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _patient360Reader.GetPatient360Async(_testPatient.Id);

        // Assert - Entity should have both citations
        Assert.NotNull(result);
        Assert.Single(result.Entities);
        Assert.Equal(2, result.Entities[0].Citations.Count);
    }

    [Fact]
    public async Task GetPatient360Async_CitationsMatchPersistedValues()
    {
        // Arrange
        var entity = new ExtractedEntity
        {
            Id = Guid.NewGuid(),
            PatientId = _testPatient.Id,
            DocumentId = _testDocument.Id,
            Category = "VitalSigns",
            Name = "blood_pressure",
            Value = "120/80"
        };
        _dbContext.ExtractedEntities.Add(entity);

        var citation = new EntityCitation
        {
            Id = Guid.NewGuid(),
            ExtractedEntityId = entity.Id,
            DocumentChunkId = _testChunk.Id,
            Page = 3,
            Section = "Vitals",
            Coordinates = "{\"x\":100,\"y\":200,\"width\":150,\"height\":25}",
            CitedText = "BP: 120/80 mmHg"
        };
        _dbContext.EntityCitations.Add(citation);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _patient360Reader.GetPatient360Async(_testPatient.Id);

        // Assert - Citation values match persisted values
        Assert.NotNull(result);
        Assert.Single(result.Entities);
        
        var returnedCitation = result.Entities[0].Citations[0];
        Assert.Equal(citation.Id, returnedCitation.Id);
        Assert.Equal(_testDocument.Id, returnedCitation.DocumentId);
        Assert.Equal("patient-record.pdf", returnedCitation.DocumentName);
        Assert.Equal(3, returnedCitation.Page);
        Assert.Equal("Vitals", returnedCitation.Section);
        Assert.Equal("{\"x\":100,\"y\":200,\"width\":150,\"height\":25}", returnedCitation.Coordinates);
        Assert.Equal("BP: 120/80 mmHg", returnedCitation.CitedText);
    }

    #endregion

    #region Patient Data Tests

    [Fact]
    public async Task GetPatient360Async_ReturnsPatientDemographics()
    {
        // Arrange - Add a grounded entity so response is not empty
        var entity = new ExtractedEntity
        {
            Id = Guid.NewGuid(),
            PatientId = _testPatient.Id,
            DocumentId = _testDocument.Id,
            Category = "Demographics",
            Name = "name",
            Value = "Test"
        };
        _dbContext.ExtractedEntities.Add(entity);

        var citation = new EntityCitation
        {
            Id = Guid.NewGuid(),
            ExtractedEntityId = entity.Id,
            DocumentChunkId = _testChunk.Id,
            Page = 1,
            CitedText = "Test"
        };
        _dbContext.EntityCitations.Add(citation);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _patient360Reader.GetPatient360Async(_testPatient.Id);

        // Assert - Patient demographics are included
        Assert.NotNull(result);
        Assert.Equal(_testPatient.Id, result.PatientId);
        Assert.Equal("MRN-360-TEST", result.Mrn);
        Assert.Equal("Patient 360 Test", result.Name);
        Assert.Equal(new DateOnly(1985, 6, 15), result.Dob);
        Assert.Equal("123 Test St", result.Address);
        Assert.Equal("555-1234", result.Contact);
    }

    [Fact]
    public async Task GetPatient360Async_PatientNotFound_ReturnsNull()
    {
        // Act
        var result = await _patient360Reader.GetPatient360Async(Guid.NewGuid());

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetPatient360Async_DeletedPatient_ReturnsNull()
    {
        // Arrange - Mark patient as deleted
        _testPatient.IsDeleted = true;
        _testPatient.DeletedAt = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _patient360Reader.GetPatient360Async(_testPatient.Id);

        // Assert
        Assert.Null(result);
    }

    #endregion

    #region Document Summary Tests

    [Fact]
    public async Task GetPatient360Async_IncludesDocumentSummary()
    {
        // Arrange - Add grounded entity
        var entity = new ExtractedEntity
        {
            Id = Guid.NewGuid(),
            PatientId = _testPatient.Id,
            DocumentId = _testDocument.Id,
            Category = "Demographics",
            Name = "name",
            Value = "Test"
        };
        _dbContext.ExtractedEntities.Add(entity);

        var citation = new EntityCitation
        {
            Id = Guid.NewGuid(),
            ExtractedEntityId = entity.Id,
            DocumentChunkId = _testChunk.Id,
            Page = 1,
            CitedText = "Test"
        };
        _dbContext.EntityCitations.Add(citation);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _patient360Reader.GetPatient360Async(_testPatient.Id);

        // Assert - Document summary is included
        Assert.NotNull(result);
        Assert.Single(result.Documents);
        Assert.Equal(_testDocument.Id, result.Documents[0].Id);
        Assert.Equal("patient-record.pdf", result.Documents[0].OriginalName);
        Assert.Equal("Completed", result.Documents[0].Status);
    }

    [Fact]
    public async Task GetPatient360Async_DocumentGroundedEntityCount_IsAccurate()
    {
        // Arrange - Create multiple grounded entities from same document
        var entity1 = new ExtractedEntity
        {
            Id = Guid.NewGuid(),
            PatientId = _testPatient.Id,
            DocumentId = _testDocument.Id,
            Category = "Demographics",
            Name = "name",
            Value = "Test"
        };
        var entity2 = new ExtractedEntity
        {
            Id = Guid.NewGuid(),
            PatientId = _testPatient.Id,
            DocumentId = _testDocument.Id,
            Category = "Diagnosis",
            Name = "condition",
            Value = "Hypertension"
        };
        _dbContext.ExtractedEntities.AddRange(entity1, entity2);

        var citation1 = new EntityCitation
        {
            Id = Guid.NewGuid(),
            ExtractedEntityId = entity1.Id,
            DocumentChunkId = _testChunk.Id,
            Page = 1,
            CitedText = "Test"
        };
        var citation2 = new EntityCitation
        {
            Id = Guid.NewGuid(),
            ExtractedEntityId = entity2.Id,
            DocumentChunkId = _testChunk.Id,
            Page = 2,
            CitedText = "Hypertension"
        };
        _dbContext.EntityCitations.AddRange(citation1, citation2);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _patient360Reader.GetPatient360Async(_testPatient.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.EntityCount);
        Assert.Single(result.Documents);
        Assert.Equal(2, result.Documents[0].GroundedEntityCount);
    }

    #endregion

    #region Response Metadata Tests

    [Fact]
    public async Task GetPatient360Async_IncludesGeneratedAtTimestamp()
    {
        // Arrange
        var entity = new ExtractedEntity
        {
            Id = Guid.NewGuid(),
            PatientId = _testPatient.Id,
            DocumentId = _testDocument.Id,
            Category = "Demographics",
            Name = "name",
            Value = "Test"
        };
        _dbContext.ExtractedEntities.Add(entity);

        var citation = new EntityCitation
        {
            Id = Guid.NewGuid(),
            ExtractedEntityId = entity.Id,
            DocumentChunkId = _testChunk.Id,
            Page = 1,
            CitedText = "Test"
        };
        _dbContext.EntityCitations.Add(citation);
        await _dbContext.SaveChangesAsync();

        var beforeCall = DateTime.UtcNow;

        // Act
        var result = await _patient360Reader.GetPatient360Async(_testPatient.Id);

        var afterCall = DateTime.UtcNow;

        // Assert
        Assert.NotNull(result);
        Assert.True(result.GeneratedAt >= beforeCall);
        Assert.True(result.GeneratedAt <= afterCall);
    }

    #endregion
}
