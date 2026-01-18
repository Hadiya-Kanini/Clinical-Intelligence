using ClinicalIntelligence.Api.Data;
using ClinicalIntelligence.Api.Domain.Models;
using ClinicalIntelligence.Api.Services.Entities;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace ClinicalIntelligence.Api.Tests;

/// <summary>
/// Tests validating entity citation persistence and referential integrity.
/// </summary>
public class EntityCitationPersistenceTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly EntityCitationReader _citationReader;
    private readonly User _testUser;
    private readonly ErdPatient _testPatient;
    private readonly Document _testDocument;
    private readonly DocumentChunk _testChunk;

    public EntityCitationPersistenceTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _dbContext = new ApplicationDbContext(options);
        _citationReader = new EntityCitationReader(_dbContext);

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
            Mrn = "MRN-TEST-001",
            Name = "Test Patient",
            Dob = new DateOnly(1980, 1, 1),
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
            OriginalName = "test-document.pdf",
            MimeType = "application/pdf",
            SizeBytes = 1024,
            StoragePath = "/test/path/test-document.pdf",
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
            Section = "Header",
            Coordinates = "{\"x\":10,\"y\":20,\"width\":100,\"height\":15}",
            TextContent = "Patient: Test Patient, DOB: 01/01/1980",
            ChunkHash = "abc123"
        };
        _dbContext.DocumentChunks.Add(_testChunk);

        _dbContext.SaveChanges();
    }

    public void Dispose()
    {
        _dbContext.Dispose();
    }

    #region Persistence Tests

    [Fact]
    public async Task PersistCitation_ValidData_RowsExistInBothTables()
    {
        // Arrange
        var entity = new ExtractedEntity
        {
            Id = Guid.NewGuid(),
            PatientId = _testPatient.Id,
            DocumentId = _testDocument.Id,
            Category = "Demographics",
            Name = "patient_name",
            Value = "Test Patient"
        };
        _dbContext.ExtractedEntities.Add(entity);

        var citation = new EntityCitation
        {
            Id = Guid.NewGuid(),
            ExtractedEntityId = entity.Id,
            DocumentChunkId = _testChunk.Id,
            Page = 1,
            Section = "Header",
            Coordinates = "{\"x\":10,\"y\":20,\"width\":100,\"height\":15}",
            CitedText = "Patient: Test Patient"
        };
        _dbContext.EntityCitations.Add(citation);

        // Act
        await _dbContext.SaveChangesAsync();

        // Assert - Verify rows exist
        var savedEntity = await _dbContext.ExtractedEntities.FindAsync(entity.Id);
        var savedCitation = await _dbContext.EntityCitations.FindAsync(citation.Id);

        Assert.NotNull(savedEntity);
        Assert.NotNull(savedCitation);
        Assert.Equal(entity.Id, savedCitation.ExtractedEntityId);
        Assert.Equal(_testChunk.Id, savedCitation.DocumentChunkId);
    }

    [Fact]
    public async Task PersistCitation_ForeignKeyRelationships_AreValid()
    {
        // Arrange
        var entity = new ExtractedEntity
        {
            Id = Guid.NewGuid(),
            PatientId = _testPatient.Id,
            DocumentId = _testDocument.Id,
            Category = "Diagnosis",
            Name = "condition",
            Value = "Hypertension"
        };
        _dbContext.ExtractedEntities.Add(entity);

        var citation = new EntityCitation
        {
            Id = Guid.NewGuid(),
            ExtractedEntityId = entity.Id,
            DocumentChunkId = _testChunk.Id,
            Page = 2,
            Section = "Assessment",
            CitedText = "Diagnosis: Hypertension"
        };
        _dbContext.EntityCitations.Add(citation);
        await _dbContext.SaveChangesAsync();

        // Act - Load with navigation properties
        var loadedCitation = await _dbContext.EntityCitations
            .Include(c => c.ExtractedEntity)
            .Include(c => c.DocumentChunk)
            .FirstOrDefaultAsync(c => c.Id == citation.Id);

        // Assert - FK relationships are valid
        Assert.NotNull(loadedCitation);
        Assert.NotNull(loadedCitation.ExtractedEntity);
        Assert.NotNull(loadedCitation.DocumentChunk);
        Assert.Equal(entity.Id, loadedCitation.ExtractedEntity.Id);
        Assert.Equal(_testChunk.Id, loadedCitation.DocumentChunk.Id);
    }

    [Fact]
    public async Task PersistCitation_CitationFieldsRoundTrip_Correctly()
    {
        // Arrange
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

        var citation = new EntityCitation
        {
            Id = Guid.NewGuid(),
            ExtractedEntityId = entity.Id,
            DocumentChunkId = _testChunk.Id,
            Page = 3,
            Section = "Medications",
            Coordinates = "{\"x\":50,\"y\":100,\"width\":200,\"height\":20}",
            CitedText = "Current Medications: Aspirin 81mg daily"
        };
        _dbContext.EntityCitations.Add(citation);
        await _dbContext.SaveChangesAsync();

        // Act - Clear tracking and reload
        _dbContext.ChangeTracker.Clear();
        var reloaded = await _dbContext.EntityCitations.FindAsync(citation.Id);

        // Assert - All fields round-trip correctly
        Assert.NotNull(reloaded);
        Assert.Equal(3, reloaded.Page);
        Assert.Equal("Medications", reloaded.Section);
        Assert.Equal("{\"x\":50,\"y\":100,\"width\":200,\"height\":20}", reloaded.Coordinates);
        Assert.Equal("Current Medications: Aspirin 81mg daily", reloaded.CitedText);
    }

    [Fact]
    public async Task PersistMultipleCitations_ForSameEntity_AllPersisted()
    {
        // Arrange
        var entity = new ExtractedEntity
        {
            Id = Guid.NewGuid(),
            PatientId = _testPatient.Id,
            DocumentId = _testDocument.Id,
            Category = "Allergy",
            Name = "allergen",
            Value = "Penicillin"
        };
        _dbContext.ExtractedEntities.Add(entity);

        // Create second chunk
        var chunk2 = new DocumentChunk
        {
            Id = Guid.NewGuid(),
            DocumentId = _testDocument.Id,
            Page = 2,
            Section = "Allergies",
            TextContent = "Allergies: Penicillin - severe reaction",
            ChunkHash = "def456"
        };
        _dbContext.DocumentChunks.Add(chunk2);

        var citation1 = new EntityCitation
        {
            Id = Guid.NewGuid(),
            ExtractedEntityId = entity.Id,
            DocumentChunkId = _testChunk.Id,
            Page = 1,
            Section = "Header",
            CitedText = "Allergies: Penicillin"
        };

        var citation2 = new EntityCitation
        {
            Id = Guid.NewGuid(),
            ExtractedEntityId = entity.Id,
            DocumentChunkId = chunk2.Id,
            Page = 2,
            Section = "Allergies",
            CitedText = "Known allergy to Penicillin - anaphylaxis"
        };

        _dbContext.EntityCitations.AddRange(citation1, citation2);
        await _dbContext.SaveChangesAsync();

        // Act
        var citations = await _dbContext.EntityCitations
            .Where(c => c.ExtractedEntityId == entity.Id)
            .ToListAsync();

        // Assert
        Assert.Equal(2, citations.Count);
    }

    #endregion

    #region EntityCitationReader Tests

    [Fact]
    public async Task GetCitationsForEntityAsync_ReturnsCitations()
    {
        // Arrange
        var entity = new ExtractedEntity
        {
            Id = Guid.NewGuid(),
            PatientId = _testPatient.Id,
            DocumentId = _testDocument.Id,
            Category = "Demographics",
            Name = "dob",
            Value = "1980-01-01"
        };
        _dbContext.ExtractedEntities.Add(entity);

        var citation = new EntityCitation
        {
            Id = Guid.NewGuid(),
            ExtractedEntityId = entity.Id,
            DocumentChunkId = _testChunk.Id,
            Page = 1,
            Section = "Header",
            CitedText = "DOB: 01/01/1980"
        };
        _dbContext.EntityCitations.Add(citation);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _citationReader.GetCitationsForEntityAsync(entity.Id);

        // Assert
        Assert.Single(result);
        Assert.Equal(citation.Id, result[0].Id);
        Assert.Equal(_testDocument.Id, result[0].DocumentId);
        Assert.Equal("test-document.pdf", result[0].DocumentName);
        Assert.Equal(1, result[0].Page);
        Assert.Equal("Header", result[0].Section);
        Assert.Equal("DOB: 01/01/1980", result[0].CitedText);
    }

    [Fact]
    public async Task GetEntitiesWithCitationsForPatientAsync_ReturnsAllEntities()
    {
        // Arrange
        var entity1 = new ExtractedEntity
        {
            Id = Guid.NewGuid(),
            PatientId = _testPatient.Id,
            DocumentId = _testDocument.Id,
            Category = "Demographics",
            Name = "name",
            Value = "Test Patient"
        };
        var entity2 = new ExtractedEntity
        {
            Id = Guid.NewGuid(),
            PatientId = _testPatient.Id,
            DocumentId = _testDocument.Id,
            Category = "Diagnosis",
            Name = "condition",
            Value = "Diabetes"
        };
        _dbContext.ExtractedEntities.AddRange(entity1, entity2);

        var citation1 = new EntityCitation
        {
            Id = Guid.NewGuid(),
            ExtractedEntityId = entity1.Id,
            DocumentChunkId = _testChunk.Id,
            Page = 1,
            CitedText = "Patient: Test Patient"
        };
        _dbContext.EntityCitations.Add(citation1);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _citationReader.GetEntitiesWithCitationsForPatientAsync(_testPatient.Id);

        // Assert
        Assert.Equal(2, result.Count);
        
        var demoEntity = result.First(e => e.Category == "Demographics");
        Assert.Single(demoEntity.Citations);
        Assert.True(demoEntity.IsGrounded);

        var diagEntity = result.First(e => e.Category == "Diagnosis");
        Assert.Empty(diagEntity.Citations);
        Assert.False(diagEntity.IsGrounded);
    }

    [Fact]
    public async Task GetGroundedEntitiesForPatientAsync_ReturnsOnlyGroundedEntities()
    {
        // Arrange
        var groundedEntity = new ExtractedEntity
        {
            Id = Guid.NewGuid(),
            PatientId = _testPatient.Id,
            DocumentId = _testDocument.Id,
            Category = "Demographics",
            Name = "name",
            Value = "Test Patient"
        };
        var ungroundedEntity = new ExtractedEntity
        {
            Id = Guid.NewGuid(),
            PatientId = _testPatient.Id,
            DocumentId = _testDocument.Id,
            Category = "Diagnosis",
            Name = "condition",
            Value = "Ungrounded Diagnosis"
        };
        _dbContext.ExtractedEntities.AddRange(groundedEntity, ungroundedEntity);

        var citation = new EntityCitation
        {
            Id = Guid.NewGuid(),
            ExtractedEntityId = groundedEntity.Id,
            DocumentChunkId = _testChunk.Id,
            Page = 1,
            CitedText = "Patient: Test Patient"
        };
        _dbContext.EntityCitations.Add(citation);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _citationReader.GetGroundedEntitiesForPatientAsync(_testPatient.Id);

        // Assert - Only grounded entity returned
        Assert.Single(result);
        Assert.Equal(groundedEntity.Id, result[0].Id);
        Assert.True(result[0].IsGrounded);
    }

    [Fact]
    public async Task GetEntitiesWithCitationsForDocumentAsync_ReturnsDocumentEntities()
    {
        // Arrange
        var entity = new ExtractedEntity
        {
            Id = Guid.NewGuid(),
            PatientId = _testPatient.Id,
            DocumentId = _testDocument.Id,
            Category = "Medication",
            Name = "drug",
            Value = "Metformin"
        };
        _dbContext.ExtractedEntities.Add(entity);

        var citation = new EntityCitation
        {
            Id = Guid.NewGuid(),
            ExtractedEntityId = entity.Id,
            DocumentChunkId = _testChunk.Id,
            Page = 2,
            Section = "Medications",
            CitedText = "Metformin 500mg twice daily"
        };
        _dbContext.EntityCitations.Add(citation);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _citationReader.GetEntitiesWithCitationsForDocumentAsync(_testDocument.Id);

        // Assert
        Assert.Single(result);
        Assert.Equal("Medication", result[0].Category);
        Assert.Single(result[0].Citations);
    }

    #endregion

    #region Cascade Delete Tests

    [Fact]
    public async Task DeleteEntity_CascadesDeleteToCitations()
    {
        // Arrange
        var entity = new ExtractedEntity
        {
            Id = Guid.NewGuid(),
            PatientId = _testPatient.Id,
            DocumentId = _testDocument.Id,
            Category = "Test",
            Name = "test",
            Value = "value"
        };
        _dbContext.ExtractedEntities.Add(entity);

        var citation = new EntityCitation
        {
            Id = Guid.NewGuid(),
            ExtractedEntityId = entity.Id,
            DocumentChunkId = _testChunk.Id,
            Page = 1,
            CitedText = "Test citation"
        };
        _dbContext.EntityCitations.Add(citation);
        await _dbContext.SaveChangesAsync();

        // Act - Delete entity
        _dbContext.ExtractedEntities.Remove(entity);
        await _dbContext.SaveChangesAsync();

        // Assert - Citation should be deleted via cascade
        var remainingCitation = await _dbContext.EntityCitations.FindAsync(citation.Id);
        Assert.Null(remainingCitation);
    }

    #endregion
}
