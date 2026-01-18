using ClinicalIntelligence.Api.Data;
using ClinicalIntelligence.Api.Domain.Models;
using ClinicalIntelligence.Api.Services.PatientMatching;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace ClinicalIntelligence.Api.Tests.PatientMatching;

/// <summary>
/// Edge case tests for patient identity matching covering:
/// - Conflicting identifiers across documents
/// - Typos / name formatting differences
/// - DOB format differences
/// </summary>
public class PatientIdentityEdgeCaseTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly PatientMatcher _matcher;
    private readonly PatientLinkingService _linkingService;
    private readonly User _testUser;

    public PatientIdentityEdgeCaseTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _dbContext = new ApplicationDbContext(options);

        var matcherLogger = Mock.Of<ILogger<PatientMatcher>>();
        _matcher = new PatientMatcher(_dbContext, matcherLogger);

        var linkingLogger = Mock.Of<ILogger<PatientLinkingService>>();
        _linkingService = new PatientLinkingService(_dbContext, _matcher, linkingLogger);

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

    private Document CreateTestDocument(Guid? patientId = null)
    {
        var document = new Document
        {
            Id = Guid.NewGuid(),
            PatientId = patientId,
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

    #region MRN Conflict Scenarios

    [Fact]
    public async Task ConflictingMrn_SameNameDob_RecordsConflictAndLinksToExisting()
    {
        // Scenario: Two documents with same name+DOB but different MRNs
        // Expected: Link via name+DOB fallback and record MRN conflict
        
        // Arrange
        var existingPatient = CreateTestPatient("MRN-AAA-111", "Sarah Connor", new DateOnly(1965, 2, 28));
        var doc1 = CreateTestDocument(patientId: existingPatient.Id);
        var doc2 = CreateTestDocument();
        await _dbContext.SaveChangesAsync();

        var input = new PatientLinkageInput
        {
            DocumentId = doc2.Id,
            Mrn = "MRN-BBB-222", // Different MRN
            Name = "Sarah Connor", // Same name
            DobString = "1965-02-28" // Same DOB
        };

        // Act
        var result = await _linkingService.LinkDocumentToPatientAsync(input);

        // Assert
        Assert.False(result.PatientWasCreated);
        Assert.True(result.MrnConflictDetected);
        Assert.Equal(existingPatient.Id, result.PatientId);

        // Verify conflict was persisted
        var conflicts = await _dbContext.ErdConflicts
            .Where(c => c.PatientId == existingPatient.Id && c.Field == "mrn")
            .ToListAsync();
        Assert.Single(conflicts);
        Assert.Contains("MRN-AAA-111", conflicts[0].ConflictingValues);
        Assert.Contains("MRN-BBB-222", conflicts[0].ConflictingValues);
    }

    [Fact]
    public async Task ConflictingMrn_DoesNotOverwriteExistingMrn()
    {
        // Scenario: Extracted MRN conflicts with existing patient's MRN
        // Expected: Do not overwrite stored MRN
        
        // Arrange
        var existingPatient = CreateTestPatient("MRN-ORIGINAL", "John Doe", new DateOnly(1970, 5, 15));
        var document = CreateTestDocument();
        await _dbContext.SaveChangesAsync();

        var input = new PatientLinkageInput
        {
            DocumentId = document.Id,
            Mrn = "MRN-NEW-EXTRACTED", // Different MRN
            Name = "John Doe",
            DobString = "1970-05-15"
        };

        // Act
        var result = await _linkingService.LinkDocumentToPatientAsync(input);

        // Assert
        Assert.Equal(existingPatient.Id, result.PatientId);

        // Verify original MRN was NOT overwritten
        var patient = await _dbContext.ErdPatients.FindAsync(existingPatient.Id);
        Assert.Equal("MRN-ORIGINAL", patient!.Mrn);
    }

    [Fact]
    public async Task MrnMatch_WinsOverNameDobMatch_EvenWithDifferentName()
    {
        // Scenario: MRN matches one patient, name+DOB matches another
        // Expected: MRN match takes priority
        
        // Arrange
        var mrnPatient = CreateTestPatient("MRN-PRIORITY", "Alice Smith", new DateOnly(1985, 3, 10));
        var nameDobPatient = CreateTestPatient("MRN-OTHER", "Bob Jones", new DateOnly(1990, 7, 20));
        var document = CreateTestDocument();
        await _dbContext.SaveChangesAsync();

        var input = new PatientLinkageInput
        {
            DocumentId = document.Id,
            Mrn = "MRN-PRIORITY", // Matches mrnPatient
            Name = "Bob Jones", // Matches nameDobPatient
            DobString = "1990-07-20" // Matches nameDobPatient
        };

        // Act
        var result = await _linkingService.LinkDocumentToPatientAsync(input);

        // Assert - MRN match wins
        Assert.Equal(mrnPatient.Id, result.PatientId);
        Assert.False(result.MrnConflictDetected); // No conflict because MRN matched
    }

    #endregion

    #region Name Formatting Variation Scenarios

    [Theory]
    [InlineData("DOE, JANE", "Jane Doe")]
    [InlineData("jane doe", "JANE DOE")]
    [InlineData("Jane   Doe", "Jane Doe")]
    [InlineData("DOE,JANE", "Jane Doe")]
    [InlineData("Doe Jane", "Jane Doe")]
    public async Task NameVariations_MatchSamePatient(string extractedName, string existingName)
    {
        // Scenario: Name formatting differences should normalize to same match key
        
        // Arrange
        var existingPatient = CreateTestPatient("MRN-EXISTING", existingName, new DateOnly(1980, 1, 1));
        var document = CreateTestDocument();
        await _dbContext.SaveChangesAsync();

        var input = new PatientLinkageInput
        {
            DocumentId = document.Id,
            Mrn = "MRN-DIFFERENT", // No MRN match
            Name = extractedName,
            DobString = "1980-01-01"
        };

        // Act
        var result = await _linkingService.LinkDocumentToPatientAsync(input);

        // Assert
        Assert.False(result.PatientWasCreated);
        Assert.Equal(existingPatient.Id, result.PatientId);
    }

    [Theory]
    [InlineData("O'Brien, Mary", "Mary OBrien")]
    [InlineData("Mary O'Brien", "OBRIEN, MARY")]
    [InlineData("O`Brien Mary", "Mary O'Brien")]
    public async Task NameWithApostrophe_MatchesVariations(string extractedName, string existingName)
    {
        // Scenario: Names with apostrophes/special characters
        
        // Arrange
        var existingPatient = CreateTestPatient("MRN-APOSTROPHE", existingName, new DateOnly(1975, 6, 15));
        var document = CreateTestDocument();
        await _dbContext.SaveChangesAsync();

        var input = new PatientLinkageInput
        {
            DocumentId = document.Id,
            Mrn = "MRN-DIFFERENT",
            Name = extractedName,
            DobString = "1975-06-15"
        };

        // Act
        var result = await _linkingService.LinkDocumentToPatientAsync(input);

        // Assert
        Assert.False(result.PatientWasCreated);
        Assert.Equal(existingPatient.Id, result.PatientId);
    }

    [Theory]
    [InlineData("Smith-Jones, Anna", "Anna Smith-Jones")]
    [InlineData("SMITHJONES ANNA", "Anna Smith-Jones")]
    [InlineData("Anna SmithJones", "Smith-Jones, Anna")]
    public async Task HyphenatedName_MatchesVariations(string extractedName, string existingName)
    {
        // Scenario: Hyphenated names
        
        // Arrange
        var existingPatient = CreateTestPatient("MRN-HYPHEN", existingName, new DateOnly(1988, 12, 25));
        var document = CreateTestDocument();
        await _dbContext.SaveChangesAsync();

        var input = new PatientLinkageInput
        {
            DocumentId = document.Id,
            Mrn = "MRN-DIFFERENT",
            Name = extractedName,
            DobString = "1988-12-25"
        };

        // Act
        var result = await _linkingService.LinkDocumentToPatientAsync(input);

        // Assert
        Assert.False(result.PatientWasCreated);
        Assert.Equal(existingPatient.Id, result.PatientId);
    }

    [Fact]
    public async Task ExtraWhitespace_NormalizesCorrectly()
    {
        // Scenario: Extra whitespace in names
        
        // Arrange
        var existingPatient = CreateTestPatient("MRN-WHITESPACE", "John Smith", new DateOnly(1990, 1, 1));
        var document = CreateTestDocument();
        await _dbContext.SaveChangesAsync();

        var input = new PatientLinkageInput
        {
            DocumentId = document.Id,
            Mrn = "MRN-DIFFERENT",
            Name = "  John    Smith  ", // Extra whitespace
            DobString = "1990-01-01"
        };

        // Act
        var result = await _linkingService.LinkDocumentToPatientAsync(input);

        // Assert
        Assert.False(result.PatientWasCreated);
        Assert.Equal(existingPatient.Id, result.PatientId);
    }

    #endregion

    #region DOB Format Variation Scenarios

    [Theory]
    [InlineData("1990-05-15", "1990-05-15")]
    [InlineData("05/15/1990", "1990-05-15")]
    [InlineData("15-05-1990", "1990-05-15")]
    [InlineData("5/15/1990", "1990-05-15")]
    [InlineData("1990/05/15", "1990-05-15")]
    public async Task DobFormats_ParseToSameDate(string extractedDob, string existingDobIso)
    {
        // Scenario: Multiple DOB formats should parse to same DateOnly
        
        // Arrange
        var existingDob = DateOnly.Parse(existingDobIso);
        var existingPatient = CreateTestPatient("MRN-EXISTING-DOB", "Test Patient", existingDob);
        var document = CreateTestDocument();
        await _dbContext.SaveChangesAsync();

        var input = new PatientLinkageInput
        {
            DocumentId = document.Id,
            Mrn = "MRN-DIFFERENT",
            Name = "Test Patient",
            DobString = extractedDob
        };

        // Act
        var result = await _linkingService.LinkDocumentToPatientAsync(input);

        // Assert
        Assert.False(result.PatientWasCreated);
        Assert.Equal(existingPatient.Id, result.PatientId);
    }

    [Theory]
    [InlineData("invalid-date")]
    [InlineData("not a date")]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("32/13/2000")]
    public async Task InvalidDob_DoesNotThrow_CreatesPatientWithNullDob(string invalidDob)
    {
        // Scenario: Invalid DOB strings should not throw and should not cause incorrect matches
        
        // Arrange
        var document = CreateTestDocument();
        await _dbContext.SaveChangesAsync();

        var input = new PatientLinkageInput
        {
            DocumentId = document.Id,
            Mrn = "MRN-INVALID-DOB-TEST",
            Name = "Patient With Invalid DOB",
            DobString = invalidDob
        };

        // Act - Should not throw
        var result = await _linkingService.LinkDocumentToPatientAsync(input);

        // Assert
        Assert.True(result.PatientWasCreated);
        
        var patient = await _dbContext.ErdPatients.FindAsync(result.PatientId);
        Assert.Null(patient!.Dob);
    }

    [Fact]
    public async Task InvalidDob_DoesNotMatchPatientWithValidDob()
    {
        // Scenario: Invalid DOB should not accidentally match a patient
        
        // Arrange
        var existingPatient = CreateTestPatient("MRN-EXISTING-VALID", "Same Name", new DateOnly(1985, 6, 15));
        var document = CreateTestDocument();
        await _dbContext.SaveChangesAsync();

        var input = new PatientLinkageInput
        {
            DocumentId = document.Id,
            Mrn = "MRN-DIFFERENT",
            Name = "Same Name", // Same name
            DobString = "invalid" // Invalid DOB - should not match
        };

        // Act
        var result = await _linkingService.LinkDocumentToPatientAsync(input);

        // Assert - Should create new patient because DOB couldn't be parsed for fallback match
        Assert.True(result.PatientWasCreated);
        Assert.NotEqual(existingPatient.Id, result.PatientId);
    }

    [Fact]
    public async Task MissingDob_DoesNotUseFallbackMatch()
    {
        // Scenario: Missing DOB should prevent fallback name+DOB matching
        
        // Arrange
        var existingPatient = CreateTestPatient("MRN-EXISTING-NODOB", "Test Patient", new DateOnly(1990, 1, 1));
        var document = CreateTestDocument();
        await _dbContext.SaveChangesAsync();

        var input = new PatientLinkageInput
        {
            DocumentId = document.Id,
            Mrn = "MRN-DIFFERENT",
            Name = "Test Patient", // Same name
            DobString = null // Missing DOB
        };

        // Act
        var result = await _linkingService.LinkDocumentToPatientAsync(input);

        // Assert - Should create new patient because DOB is required for fallback
        Assert.True(result.PatientWasCreated);
        Assert.NotEqual(existingPatient.Id, result.PatientId);
    }

    #endregion

    #region Deterministic Behavior Tests

    [Fact]
    public async Task SameInput_ProducesSameResult_Deterministic()
    {
        // Scenario: Same input should always produce same result
        
        // Arrange
        var doc1 = CreateTestDocument();
        var doc2 = CreateTestDocument();
        await _dbContext.SaveChangesAsync();

        var input1 = new PatientLinkageInput
        {
            DocumentId = doc1.Id,
            Mrn = "MRN-DETERMINISTIC",
            Name = "Deterministic Patient",
            DobString = "1985-03-20"
        };

        var input2 = new PatientLinkageInput
        {
            DocumentId = doc2.Id,
            Mrn = "MRN-DETERMINISTIC",
            Name = "Deterministic Patient",
            DobString = "1985-03-20"
        };

        // Act
        var result1 = await _linkingService.LinkDocumentToPatientAsync(input1);
        var result2 = await _linkingService.LinkDocumentToPatientAsync(input2);

        // Assert - Both should link to same patient
        Assert.True(result1.PatientWasCreated);
        Assert.False(result2.PatientWasCreated);
        Assert.Equal(result1.PatientId, result2.PatientId);
    }

    [Fact]
    public async Task NormalizationIsDeterministic_SameNormalizedKey()
    {
        // Scenario: Different input formats should produce same normalized key
        
        // Arrange
        var formats = new[]
        {
            ("DOE, JANE", "1990-05-15"),
            ("Jane Doe", "05/15/1990"),
            ("jane doe", "15-05-1990"),
            ("JANE   DOE", "1990/05/15")
        };

        var documents = new List<Document>();
        foreach (var _ in formats)
        {
            documents.Add(CreateTestDocument());
        }
        await _dbContext.SaveChangesAsync();

        // Act
        var results = new List<PatientLinkageResult>();
        for (int i = 0; i < formats.Length; i++)
        {
            var input = new PatientLinkageInput
            {
                DocumentId = documents[i].Id,
                Mrn = "MRN-DIFFERENT-" + i, // Different MRNs to force fallback
                Name = formats[i].Item1,
                DobString = formats[i].Item2
            };
            results.Add(await _linkingService.LinkDocumentToPatientAsync(input));
        }

        // Assert - All should link to same patient via name+DOB fallback
        var firstPatientId = results[0].PatientId;
        Assert.True(results[0].PatientWasCreated);
        for (int i = 1; i < results.Count; i++)
        {
            Assert.False(results[i].PatientWasCreated);
            Assert.Equal(firstPatientId, results[i].PatientId);
        }
    }

    #endregion

    #region No External Dependencies Tests

    [Fact]
    public async Task TestsRunWithoutExternalServices()
    {
        // Scenario: Tests should run reliably without external dependencies
        
        // Arrange
        var document = CreateTestDocument();
        await _dbContext.SaveChangesAsync();

        var input = new PatientLinkageInput
        {
            DocumentId = document.Id,
            Mrn = "MRN-ISOLATED-TEST",
            Name = "Isolated Test Patient",
            DobString = "2000-01-01"
        };

        // Act - Should complete without any external calls
        var result = await _linkingService.LinkDocumentToPatientAsync(input);

        // Assert
        Assert.True(result.PatientWasCreated);
        Assert.NotEqual(Guid.Empty, result.PatientId);
    }

    #endregion
}
