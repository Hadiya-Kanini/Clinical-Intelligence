using ClinicalIntelligence.Api.Data;
using ClinicalIntelligence.Api.Domain.Models;
using ClinicalIntelligence.Api.Services.PatientMatching;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace ClinicalIntelligence.Api.Tests.PatientMatching;

/// <summary>
/// Unit tests for PatientMatcher service validating MRN-first logic, 
/// fallback matching, normalization, and DOB parsing.
/// </summary>
public class PatientMatcherTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly PatientMatcher _matcher;

    public PatientMatcherTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _dbContext = new ApplicationDbContext(options);
        var logger = Mock.Of<ILogger<PatientMatcher>>();
        _matcher = new PatientMatcher(_dbContext, logger);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
    }

    #region PatientIdentityNormalizer Tests

    [Theory]
    [InlineData("MRN-123", "MRN123")]
    [InlineData("mrn 456", "MRN456")]
    [InlineData("MRN_789", "MRN789")]
    [InlineData("  MRN-001  ", "MRN001")]
    [InlineData("abc-def-ghi", "ABCDEFGHI")]
    public void NormalizeMrn_VariousFormats_ReturnsNormalized(string input, string expected)
    {
        var result = PatientIdentityNormalizer.NormalizeMrn(input);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void NormalizeMrn_NullOrEmpty_ReturnsNull(string? input)
    {
        var result = PatientIdentityNormalizer.NormalizeMrn(input);
        Assert.Null(result);
    }

    [Theory]
    [InlineData("DOE, JANE", "DOE JANE")]
    [InlineData("Jane   Doe", "DOE JANE")]
    [InlineData("jane doe", "DOE JANE")]
    [InlineData("JANE DOE", "DOE JANE")]
    [InlineData("O'Brien, Mary", "MARY OBRIEN")]
    [InlineData("Smith-Jones, Anna", "ANNA SMITHJONES")]
    public void NormalizeName_VariousFormats_ReturnsNormalized(string input, string expected)
    {
        var result = PatientIdentityNormalizer.NormalizeName(input);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void NormalizeName_NullOrEmpty_ReturnsNull(string? input)
    {
        var result = PatientIdentityNormalizer.NormalizeName(input);
        Assert.Null(result);
    }

    [Theory]
    [InlineData("1990-05-15", 1990, 5, 15)]
    [InlineData("05/15/1990", 1990, 5, 15)]
    [InlineData("15-05-1990", 1990, 5, 15)]
    [InlineData("5/15/1990", 1990, 5, 15)]
    [InlineData("1990/05/15", 1990, 5, 15)]
    public void ParseDob_VariousFormats_ReturnsParsedDate(string input, int year, int month, int day)
    {
        var result = PatientIdentityNormalizer.ParseDob(input);
        Assert.NotNull(result);
        Assert.Equal(new DateOnly(year, month, day), result.Value);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("invalid-date")]
    [InlineData("not a date")]
    public void ParseDob_InvalidInput_ReturnsNull(string? input)
    {
        var result = PatientIdentityNormalizer.ParseDob(input);
        Assert.Null(result);
    }

    [Fact]
    public void GenerateSyntheticMrn_ReturnsAutoPrefix()
    {
        var result = PatientIdentityNormalizer.GenerateSyntheticMrn();
        Assert.StartsWith("AUTO-", result);
        Assert.Equal(37, result.Length); // "AUTO-" (5) + GUID without hyphens (32)
    }

    #endregion

    #region PatientMatcher MRN Match Tests

    [Fact]
    public async Task FindOrCreatePatientAsync_MrnMatch_ReturnsExistingPatient()
    {
        // Arrange
        var existingPatient = new ErdPatient
        {
            Id = Guid.NewGuid(),
            Mrn = "MRN-001",
            Name = "John Doe",
            Dob = new DateOnly(1980, 1, 1),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _dbContext.ErdPatients.Add(existingPatient);
        await _dbContext.SaveChangesAsync();

        var input = new PatientMatchInput
        {
            Mrn = "mrn 001", // Different format, should normalize to same
            Name = "Different Name",
            DobString = "2000-01-01"
        };

        // Act
        var result = await _matcher.FindOrCreatePatientAsync(input);

        // Assert
        Assert.False(result.WasCreated);
        Assert.Equal(PatientMatchType.MrnMatch, result.MatchType);
        Assert.Equal(existingPatient.Id, result.Patient.Id);
    }

    [Fact]
    public async Task FindOrCreatePatientAsync_MrnMatchWinsOverNameDob_ReturnsMrnMatchedPatient()
    {
        // Arrange - Create two patients: one with matching MRN, one with matching name+DOB
        var mrnPatient = new ErdPatient
        {
            Id = Guid.NewGuid(),
            Mrn = "MRN-100",
            Name = "Alice Smith",
            Dob = new DateOnly(1985, 6, 15),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        var nameDobPatient = new ErdPatient
        {
            Id = Guid.NewGuid(),
            Mrn = "MRN-200",
            Name = "Jane Doe",
            Dob = new DateOnly(1990, 3, 20),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _dbContext.ErdPatients.AddRange(mrnPatient, nameDobPatient);
        await _dbContext.SaveChangesAsync();

        var input = new PatientMatchInput
        {
            Mrn = "MRN-100", // Matches mrnPatient
            Name = "Jane Doe", // Matches nameDobPatient
            DobString = "1990-03-20" // Matches nameDobPatient
        };

        // Act
        var result = await _matcher.FindOrCreatePatientAsync(input);

        // Assert - MRN match should win
        Assert.False(result.WasCreated);
        Assert.Equal(PatientMatchType.MrnMatch, result.MatchType);
        Assert.Equal(mrnPatient.Id, result.Patient.Id);
    }

    #endregion

    #region PatientMatcher Name+DOB Fallback Tests

    [Fact]
    public async Task FindOrCreatePatientAsync_NameDobFallback_ReturnsExistingPatient()
    {
        // Arrange
        var existingPatient = new ErdPatient
        {
            Id = Guid.NewGuid(),
            Mrn = "EXISTING-MRN",
            Name = "Jane Doe",
            Dob = new DateOnly(1990, 5, 15),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _dbContext.ErdPatients.Add(existingPatient);
        await _dbContext.SaveChangesAsync();

        var input = new PatientMatchInput
        {
            Mrn = "DIFFERENT-MRN", // No MRN match
            Name = "DOE, JANE", // Should normalize to match
            DobString = "05/15/1990" // Different format, same date
        };

        // Act
        var result = await _matcher.FindOrCreatePatientAsync(input);

        // Assert
        Assert.False(result.WasCreated);
        Assert.Equal(PatientMatchType.NameDobMatch, result.MatchType);
        Assert.Equal(existingPatient.Id, result.Patient.Id);
    }

    [Fact]
    public async Task FindOrCreatePatientAsync_NameVariations_MatchesSamePatient()
    {
        // Arrange
        var existingPatient = new ErdPatient
        {
            Id = Guid.NewGuid(),
            Mrn = "MRN-EXISTING",
            Name = "Mary O'Brien",
            Dob = new DateOnly(1975, 12, 25),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _dbContext.ErdPatients.Add(existingPatient);
        await _dbContext.SaveChangesAsync();

        var input = new PatientMatchInput
        {
            Mrn = "DIFFERENT-MRN",
            Name = "OBRIEN, MARY", // Different format
            DobString = "1975-12-25"
        };

        // Act
        var result = await _matcher.FindOrCreatePatientAsync(input);

        // Assert
        Assert.False(result.WasCreated);
        Assert.Equal(PatientMatchType.NameDobMatch, result.MatchType);
        Assert.Equal(existingPatient.Id, result.Patient.Id);
    }

    [Fact]
    public async Task FindOrCreatePatientAsync_NameMatchButDobMismatch_CreatesNewPatient()
    {
        // Arrange
        var existingPatient = new ErdPatient
        {
            Id = Guid.NewGuid(),
            Mrn = "MRN-EXISTING",
            Name = "John Smith",
            Dob = new DateOnly(1980, 1, 1),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _dbContext.ErdPatients.Add(existingPatient);
        await _dbContext.SaveChangesAsync();

        var input = new PatientMatchInput
        {
            Mrn = "DIFFERENT-MRN",
            Name = "John Smith", // Same name
            DobString = "1985-06-15" // Different DOB
        };

        // Act
        var result = await _matcher.FindOrCreatePatientAsync(input);

        // Assert - Should create new patient because DOB doesn't match
        Assert.True(result.WasCreated);
        Assert.Equal(PatientMatchType.Created, result.MatchType);
        Assert.NotEqual(existingPatient.Id, result.Patient.Id);
    }

    #endregion

    #region PatientMatcher Creation Tests

    [Fact]
    public async Task FindOrCreatePatientAsync_NoMatch_CreatesNewPatient()
    {
        // Arrange
        var input = new PatientMatchInput
        {
            Mrn = "NEW-MRN-001",
            Name = "New Patient",
            DobString = "2000-01-01",
            Address = "123 Main St",
            Contact = "555-1234"
        };

        // Act
        var result = await _matcher.FindOrCreatePatientAsync(input);

        // Assert
        Assert.True(result.WasCreated);
        Assert.Equal(PatientMatchType.Created, result.MatchType);
        Assert.Equal("NEW-MRN-001", result.Patient.Mrn);
        Assert.Equal("New Patient", result.Patient.Name);
        Assert.Equal(new DateOnly(2000, 1, 1), result.Patient.Dob);
        Assert.Equal("123 Main St", result.Patient.Address);
        Assert.Equal("555-1234", result.Patient.Contact);
    }

    [Fact]
    public async Task FindOrCreatePatientAsync_MissingMrn_CreatesSyntheticMrn()
    {
        // Arrange
        var input = new PatientMatchInput
        {
            Mrn = null, // No MRN
            Name = "Patient Without MRN",
            DobString = "1995-07-20"
        };

        // Act
        var result = await _matcher.FindOrCreatePatientAsync(input);

        // Assert
        Assert.True(result.WasCreated);
        Assert.Equal(PatientMatchType.Created, result.MatchType);
        Assert.StartsWith("AUTO-", result.Patient.Mrn);
    }

    [Fact]
    public async Task FindOrCreatePatientAsync_MissingName_UsesPlaceholder()
    {
        // Arrange
        var input = new PatientMatchInput
        {
            Mrn = "MRN-NO-NAME",
            Name = null,
            DobString = "1990-01-01"
        };

        // Act
        var result = await _matcher.FindOrCreatePatientAsync(input);

        // Assert
        Assert.True(result.WasCreated);
        Assert.Equal("Unknown", result.Patient.Name);
    }

    [Fact]
    public async Task FindOrCreatePatientAsync_InvalidDob_CreatesPatientWithNullDob()
    {
        // Arrange
        var input = new PatientMatchInput
        {
            Mrn = "MRN-INVALID-DOB",
            Name = "Patient With Invalid DOB",
            DobString = "not-a-date"
        };

        // Act
        var result = await _matcher.FindOrCreatePatientAsync(input);

        // Assert
        Assert.True(result.WasCreated);
        Assert.Null(result.Patient.Dob);
    }

    #endregion

    #region PatientMatcher Soft Delete Tests

    [Fact]
    public async Task FindOrCreatePatientAsync_DeletedPatient_DoesNotMatch()
    {
        // Arrange
        var deletedPatient = new ErdPatient
        {
            Id = Guid.NewGuid(),
            Mrn = "MRN-DELETED",
            Name = "Deleted Patient",
            Dob = new DateOnly(1980, 1, 1),
            IsDeleted = true,
            DeletedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _dbContext.ErdPatients.Add(deletedPatient);
        await _dbContext.SaveChangesAsync();

        var input = new PatientMatchInput
        {
            Mrn = "MRN-DELETED",
            Name = "Deleted Patient",
            DobString = "1980-01-01"
        };

        // Act
        var result = await _matcher.FindOrCreatePatientAsync(input);

        // Assert - Should create new patient, not match deleted one
        Assert.True(result.WasCreated);
        Assert.Equal(PatientMatchType.Created, result.MatchType);
        Assert.NotEqual(deletedPatient.Id, result.Patient.Id);
    }

    #endregion

    #region Multiple Documents Same Patient Tests

    [Fact]
    public async Task FindOrCreatePatientAsync_MultipleDocumentsSameMrn_LinkToSamePatient()
    {
        // Arrange - First document creates patient
        var input1 = new PatientMatchInput
        {
            Mrn = "MRN-MULTI-001",
            Name = "Multi Doc Patient",
            DobString = "1985-03-10"
        };

        var result1 = await _matcher.FindOrCreatePatientAsync(input1);
        Assert.True(result1.WasCreated);

        // Act - Second document with same MRN
        var input2 = new PatientMatchInput
        {
            Mrn = "mrn multi 001", // Different format
            Name = "PATIENT, MULTI DOC", // Different name format
            DobString = "03/10/1985" // Different date format
        };

        var result2 = await _matcher.FindOrCreatePatientAsync(input2);

        // Assert - Should match same patient
        Assert.False(result2.WasCreated);
        Assert.Equal(PatientMatchType.MrnMatch, result2.MatchType);
        Assert.Equal(result1.Patient.Id, result2.Patient.Id);
    }

    #endregion
}
