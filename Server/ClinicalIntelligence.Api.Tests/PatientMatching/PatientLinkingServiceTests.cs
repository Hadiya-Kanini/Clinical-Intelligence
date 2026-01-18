using ClinicalIntelligence.Api.Data;
using ClinicalIntelligence.Api.Domain.Models;
using ClinicalIntelligence.Api.Services.PatientMatching;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace ClinicalIntelligence.Api.Tests.PatientMatching;

/// <summary>
/// Tests for PatientLinkingService validating document/batch reassignment behavior
/// and related-record consistency.
/// </summary>
public class PatientLinkingServiceTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly PatientMatcher _patientMatcher;
    private readonly PatientLinkingService _linkingService;
    private readonly User _testUser;

    public PatientLinkingServiceTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _dbContext = new ApplicationDbContext(options);
        
        var matcherLogger = Mock.Of<ILogger<PatientMatcher>>();
        _patientMatcher = new PatientMatcher(_dbContext, matcherLogger);
        
        var linkingLogger = Mock.Of<ILogger<PatientLinkingService>>();
        _linkingService = new PatientLinkingService(_dbContext, _patientMatcher, linkingLogger);

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
        _dbContext.SaveChanges();
    }

    public void Dispose()
    {
        _dbContext.Dispose();
    }

    private Document CreateTestDocument(Guid? patientId = null, Guid? batchId = null)
    {
        var document = new Document
        {
            Id = Guid.NewGuid(),
            PatientId = patientId,
            DocumentBatchId = batchId,
            UploadedByUserId = _testUser.Id,
            OriginalName = "test.pdf",
            MimeType = "application/pdf",
            SizeBytes = 1024,
            StoragePath = "/test/path",
            Status = "Completed",
            UploadedAt = DateTime.UtcNow
        };
        _dbContext.Documents.Add(document);
        return document;
    }

    private ErdPatient CreateTestPatient(string mrn, string name, DateOnly? dob = null)
    {
        var patient = new ErdPatient
        {
            Id = Guid.NewGuid(),
            Mrn = mrn,
            Name = name,
            Dob = dob ?? new DateOnly(1980, 1, 1),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _dbContext.ErdPatients.Add(patient);
        return patient;
    }

    #region Basic Linkage Tests

    [Fact]
    public async Task LinkDocumentToPatientAsync_NewPatient_CreatesAndLinks()
    {
        // Arrange
        var document = CreateTestDocument();
        await _dbContext.SaveChangesAsync();

        var input = new PatientLinkageInput
        {
            DocumentId = document.Id,
            Mrn = "MRN-NEW-001",
            Name = "New Patient",
            DobString = "1990-05-15"
        };

        // Act
        var result = await _linkingService.LinkDocumentToPatientAsync(input);

        // Assert
        Assert.True(result.PatientWasCreated);
        Assert.False(result.WasReassigned);
        Assert.NotEqual(Guid.Empty, result.PatientId);

        // Verify document is linked
        var updatedDoc = await _dbContext.Documents.FindAsync(document.Id);
        Assert.Equal(result.PatientId, updatedDoc!.PatientId);
    }

    [Fact]
    public async Task LinkDocumentToPatientAsync_ExistingPatientByMrn_LinksToExisting()
    {
        // Arrange
        var existingPatient = CreateTestPatient("MRN-EXISTING", "Existing Patient");
        var document = CreateTestDocument();
        await _dbContext.SaveChangesAsync();

        var input = new PatientLinkageInput
        {
            DocumentId = document.Id,
            Mrn = "mrn existing", // Different format
            Name = "Different Name",
            DobString = "2000-01-01"
        };

        // Act
        var result = await _linkingService.LinkDocumentToPatientAsync(input);

        // Assert
        Assert.False(result.PatientWasCreated);
        Assert.Equal(existingPatient.Id, result.PatientId);
    }

    [Fact]
    public async Task LinkDocumentToPatientAsync_AlreadyLinkedToSamePatient_NoOp()
    {
        // Arrange
        var existingPatient = CreateTestPatient("MRN-SAME", "Same Patient");
        var document = CreateTestDocument(patientId: existingPatient.Id);
        await _dbContext.SaveChangesAsync();

        var input = new PatientLinkageInput
        {
            DocumentId = document.Id,
            Mrn = "MRN-SAME",
            Name = "Same Patient"
        };

        // Act
        var result = await _linkingService.LinkDocumentToPatientAsync(input);

        // Assert
        Assert.False(result.PatientWasCreated);
        Assert.False(result.WasReassigned);
        Assert.Equal(existingPatient.Id, result.PatientId);
    }

    #endregion

    #region Reassignment Tests

    [Fact]
    public async Task LinkDocumentToPatientAsync_Reassignment_UpdatesPatientId()
    {
        // Arrange
        var oldPatient = CreateTestPatient("MRN-OLD", "Old Patient");
        var newPatient = CreateTestPatient("MRN-NEW", "New Patient");
        var document = CreateTestDocument(patientId: oldPatient.Id);
        await _dbContext.SaveChangesAsync();

        var input = new PatientLinkageInput
        {
            DocumentId = document.Id,
            Mrn = "MRN-NEW", // Matches new patient
            Name = "New Patient"
        };

        // Act
        var result = await _linkingService.LinkDocumentToPatientAsync(input);

        // Assert
        Assert.False(result.PatientWasCreated);
        Assert.True(result.WasReassigned);
        Assert.Equal(newPatient.Id, result.PatientId);
        Assert.Equal(oldPatient.Id, result.PreviousPatientId);
    }

    #endregion

    #region Batch Consistency Tests

    [Fact]
    public async Task LinkDocumentToPatientAsync_WithBatch_UpdatesBatchAndAllDocuments()
    {
        // Arrange
        var batch = new DocumentBatch
        {
            Id = Guid.NewGuid(),
            UploadedByUserId = _testUser.Id,
            UploadedAt = DateTime.UtcNow
        };
        _dbContext.DocumentBatches.Add(batch);

        var doc1 = CreateTestDocument(batchId: batch.Id);
        var doc2 = CreateTestDocument(batchId: batch.Id);
        var doc3 = CreateTestDocument(batchId: batch.Id);
        await _dbContext.SaveChangesAsync();

        var input = new PatientLinkageInput
        {
            DocumentId = doc1.Id,
            Mrn = "MRN-BATCH-001",
            Name = "Batch Patient"
        };

        // Act
        var result = await _linkingService.LinkDocumentToPatientAsync(input);

        // Assert
        Assert.True(result.PatientWasCreated);

        // Verify batch is linked
        var updatedBatch = await _dbContext.DocumentBatches.FindAsync(batch.Id);
        Assert.Equal(result.PatientId, updatedBatch!.PatientId);

        // Verify all documents in batch are linked
        var allDocs = await _dbContext.Documents
            .Where(d => d.DocumentBatchId == batch.Id)
            .ToListAsync();
        Assert.All(allDocs, d => Assert.Equal(result.PatientId, d.PatientId));
    }

    #endregion

    #region Related Records Tests

    [Fact]
    public async Task LinkDocumentToPatientAsync_UpdatesExtractedEntities()
    {
        // Arrange
        var oldPatient = CreateTestPatient("MRN-OLD-ENT", "Old Patient");
        var document = CreateTestDocument(patientId: oldPatient.Id);
        
        var entity1 = new ExtractedEntity
        {
            Id = Guid.NewGuid(),
            PatientId = oldPatient.Id,
            DocumentId = document.Id,
            Category = "Diagnosis",
            Name = "Test Diagnosis",
            Value = "Test Value"
        };
        var entity2 = new ExtractedEntity
        {
            Id = Guid.NewGuid(),
            PatientId = oldPatient.Id,
            DocumentId = document.Id,
            Category = "Medication",
            Name = "Test Medication",
            Value = "Test Dosage"
        };
        _dbContext.ExtractedEntities.AddRange(entity1, entity2);
        await _dbContext.SaveChangesAsync();

        var input = new PatientLinkageInput
        {
            DocumentId = document.Id,
            Mrn = "MRN-NEW-ENT",
            Name = "New Patient"
        };

        // Act
        var result = await _linkingService.LinkDocumentToPatientAsync(input);

        // Assert
        Assert.True(result.PatientWasCreated);
        Assert.True(result.WasReassigned);
        Assert.Equal(2, result.RelatedRecordsUpdated);

        // Verify entities are updated
        var updatedEntities = await _dbContext.ExtractedEntities
            .Where(e => e.DocumentId == document.Id)
            .ToListAsync();
        Assert.All(updatedEntities, e => Assert.Equal(result.PatientId, e.PatientId));
    }

    [Fact]
    public async Task LinkDocumentToPatientAsync_UpdatesCodeSuggestions()
    {
        // Arrange
        var oldPatient = CreateTestPatient("MRN-OLD-CODE", "Old Patient");
        var document = CreateTestDocument(patientId: oldPatient.Id);

        // Add billing code catalog item
        var billingCode = new BillingCodeCatalogItem
        {
            Code = "A00.0",
            CodeType = "ICD10",
            Description = "Cholera due to Vibrio cholerae 01, biovar cholerae"
        };
        _dbContext.BillingCodeCatalogItems.Add(billingCode);
        
        var entity = new ExtractedEntity
        {
            Id = Guid.NewGuid(),
            PatientId = oldPatient.Id,
            DocumentId = document.Id,
            Category = "Diagnosis",
            Name = "Test Diagnosis",
            Value = "Test Value"
        };
        _dbContext.ExtractedEntities.Add(entity);

        var codeSuggestion = new CodeSuggestion
        {
            Id = Guid.NewGuid(),
            PatientId = oldPatient.Id,
            ExtractedEntityId = entity.Id,
            Code = "A00.0",
            CodeType = "ICD10",
            Status = "Pending",
            SuggestedAt = DateTime.UtcNow
        };
        _dbContext.CodeSuggestions.Add(codeSuggestion);
        await _dbContext.SaveChangesAsync();

        var input = new PatientLinkageInput
        {
            DocumentId = document.Id,
            Mrn = "MRN-NEW-CODE",
            Name = "New Patient"
        };

        // Act
        var result = await _linkingService.LinkDocumentToPatientAsync(input);

        // Assert
        Assert.True(result.PatientWasCreated);
        Assert.Equal(2, result.RelatedRecordsUpdated); // 1 entity + 1 code suggestion

        // Verify code suggestion is updated
        var updatedSuggestion = await _dbContext.CodeSuggestions.FindAsync(codeSuggestion.Id);
        Assert.Equal(result.PatientId, updatedSuggestion!.PatientId);
    }

    #endregion

    #region MRN Conflict Tests

    [Fact]
    public async Task LinkDocumentToPatientAsync_MrnConflict_RecordsConflict()
    {
        // Arrange - Patient matched by name+DOB but has different MRN
        var existingPatient = CreateTestPatient("MRN-EXISTING-CONFLICT", "Jane Doe", new DateOnly(1985, 6, 15));
        var document = CreateTestDocument();
        await _dbContext.SaveChangesAsync();

        var input = new PatientLinkageInput
        {
            DocumentId = document.Id,
            Mrn = "MRN-DIFFERENT", // Different MRN
            Name = "Jane Doe", // Same name
            DobString = "1985-06-15" // Same DOB - will match by name+DOB
        };

        // Act
        var result = await _linkingService.LinkDocumentToPatientAsync(input);

        // Assert
        Assert.False(result.PatientWasCreated);
        Assert.True(result.MrnConflictDetected);
        Assert.Equal(existingPatient.Id, result.PatientId);

        // Verify conflict was recorded
        var conflicts = await _dbContext.ErdConflicts
            .Where(c => c.PatientId == existingPatient.Id && c.Field == "mrn")
            .ToListAsync();
        Assert.Single(conflicts);
        Assert.Equal("High", conflicts[0].Severity);
        Assert.Equal("Pending", conflicts[0].Status);
    }

    [Fact]
    public async Task LinkDocumentToPatientAsync_SyntheticMrnNoConflict_NoConflictRecorded()
    {
        // Arrange - Patient with synthetic MRN matched by name+DOB
        var existingPatient = CreateTestPatient("AUTO-12345678901234567890123456789012", "John Smith", new DateOnly(1990, 3, 20));
        var document = CreateTestDocument();
        await _dbContext.SaveChangesAsync();

        var input = new PatientLinkageInput
        {
            DocumentId = document.Id,
            Mrn = "MRN-REAL", // Real MRN provided
            Name = "John Smith",
            DobString = "1990-03-20"
        };

        // Act
        var result = await _linkingService.LinkDocumentToPatientAsync(input);

        // Assert - No conflict because existing MRN is synthetic
        Assert.False(result.PatientWasCreated);
        Assert.False(result.MrnConflictDetected);

        // Verify no conflict was recorded
        var conflicts = await _dbContext.ErdConflicts
            .Where(c => c.PatientId == existingPatient.Id && c.Field == "mrn")
            .ToListAsync();
        Assert.Empty(conflicts);
    }

    #endregion

    #region Error Handling Tests

    [Fact]
    public async Task LinkDocumentToPatientAsync_DocumentNotFound_ThrowsException()
    {
        // Arrange
        var input = new PatientLinkageInput
        {
            DocumentId = Guid.NewGuid(), // Non-existent
            Mrn = "MRN-001"
        };

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _linkingService.LinkDocumentToPatientAsync(input));
    }

    #endregion

    #region Multiple Documents Same Patient Tests

    [Fact]
    public async Task LinkDocumentToPatientAsync_TwoDocumentsSameMrn_LinkToSamePatient()
    {
        // Arrange
        var doc1 = CreateTestDocument();
        var doc2 = CreateTestDocument();
        await _dbContext.SaveChangesAsync();

        var input1 = new PatientLinkageInput
        {
            DocumentId = doc1.Id,
            Mrn = "MRN-SHARED",
            Name = "Shared Patient"
        };

        var input2 = new PatientLinkageInput
        {
            DocumentId = doc2.Id,
            Mrn = "mrn shared", // Different format
            Name = "PATIENT, SHARED" // Different name format
        };

        // Act
        var result1 = await _linkingService.LinkDocumentToPatientAsync(input1);
        var result2 = await _linkingService.LinkDocumentToPatientAsync(input2);

        // Assert
        Assert.True(result1.PatientWasCreated);
        Assert.False(result2.PatientWasCreated);
        Assert.Equal(result1.PatientId, result2.PatientId);
    }

    [Fact]
    public async Task LinkDocumentToPatientAsync_FallbackMatch_LinksToSamePatient()
    {
        // Arrange
        var doc1 = CreateTestDocument();
        var doc2 = CreateTestDocument();
        await _dbContext.SaveChangesAsync();

        var input1 = new PatientLinkageInput
        {
            DocumentId = doc1.Id,
            Mrn = "MRN-FIRST",
            Name = "Mary Johnson",
            DobString = "1975-08-22"
        };

        var input2 = new PatientLinkageInput
        {
            DocumentId = doc2.Id,
            Mrn = "MRN-SECOND", // Different MRN - no MRN match
            Name = "JOHNSON, MARY", // Same name, different format
            DobString = "08/22/1975" // Same DOB, different format
        };

        // Act
        var result1 = await _linkingService.LinkDocumentToPatientAsync(input1);
        var result2 = await _linkingService.LinkDocumentToPatientAsync(input2);

        // Assert - Second doc should match by name+DOB
        Assert.True(result1.PatientWasCreated);
        Assert.False(result2.PatientWasCreated);
        Assert.Equal(result1.PatientId, result2.PatientId);
        Assert.True(result2.MrnConflictDetected); // MRN conflict should be recorded
    }

    #endregion
}
