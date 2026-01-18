using ClinicalIntelligence.Api.Data;
using ClinicalIntelligence.Api.Domain.Models;
using ClinicalIntelligence.Api.Services.Processing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace ClinicalIntelligence.Api.Tests.Processing;

/// <summary>
/// Unit tests for PatientIdentityMatcher.
/// Tests MRN-first and name+DOB patient identity matching behavior.
/// </summary>
public class PatientIdentityMatcherTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly PatientIdentityMatcher _matcher;
    private readonly Guid _testPatientId;

    public PatientIdentityMatcherTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _dbContext = new ApplicationDbContext(options);
        var logger = new Mock<ILogger<PatientIdentityMatcher>>();
        _matcher = new PatientIdentityMatcher(_dbContext, logger.Object);

        _testPatientId = Guid.NewGuid();
        SeedTestData();
    }

    private void SeedTestData()
    {
        var patient = new ErdPatient
        {
            Id = _testPatientId,
            Mrn = "MRN-12345",
            Name = "John Doe",
            Dob = new DateOnly(1980, 5, 15)
        };
        _dbContext.ErdPatients.Add(patient);

        var patient2 = new ErdPatient
        {
            Id = Guid.NewGuid(),
            Mrn = "MRN-67890",
            Name = "Jane Smith",
            Dob = new DateOnly(1975, 3, 20)
        };
        _dbContext.ErdPatients.Add(patient2);

        _dbContext.SaveChanges();
    }

    public void Dispose()
    {
        _dbContext.Dispose();
    }

    [Fact]
    public async Task MatchByMrnAsync_ExactMatch_ReturnsPatient()
    {
        var result = await _matcher.MatchByMrnAsync("MRN-12345");

        Assert.True(result.IsMatch);
        Assert.Equal(_testPatientId, result.PatientId);
        Assert.Equal(PatientMatchMethod.Mrn, result.MatchMethod);
    }

    [Fact]
    public async Task MatchByMrnAsync_CaseInsensitive_ReturnsPatient()
    {
        var result = await _matcher.MatchByMrnAsync("mrn-12345");

        Assert.True(result.IsMatch);
        Assert.Equal(_testPatientId, result.PatientId);
    }

    [Fact]
    public async Task MatchByMrnAsync_WithWhitespace_ReturnsPatient()
    {
        var result = await _matcher.MatchByMrnAsync("  MRN-12345  ");

        Assert.True(result.IsMatch);
        Assert.Equal(_testPatientId, result.PatientId);
    }

    [Fact]
    public async Task MatchByMrnAsync_NotFound_ReturnsNotFound()
    {
        var result = await _matcher.MatchByMrnAsync("MRN-NONEXISTENT");

        Assert.False(result.IsMatch);
        Assert.Null(result.PatientId);
    }

    [Fact]
    public async Task MatchByMrnAsync_EmptyMrn_ReturnsNotFound()
    {
        var result = await _matcher.MatchByMrnAsync("");

        Assert.False(result.IsMatch);
        Assert.Contains("required", result.Message);
    }

    [Fact]
    public async Task MatchByNameAndDobAsync_ExactMatch_ReturnsPatient()
    {
        var result = await _matcher.MatchByNameAndDobAsync(
            "John Doe",
            new DateOnly(1980, 5, 15));

        Assert.True(result.IsMatch);
        Assert.Equal(_testPatientId, result.PatientId);
        Assert.Equal(PatientMatchMethod.NameDob, result.MatchMethod);
    }

    [Fact]
    public async Task MatchByNameAndDobAsync_CaseInsensitive_ReturnsPatient()
    {
        var result = await _matcher.MatchByNameAndDobAsync(
            "JOHN DOE",
            new DateOnly(1980, 5, 15));

        Assert.True(result.IsMatch);
        Assert.Equal(_testPatientId, result.PatientId);
    }

    [Fact]
    public async Task MatchByNameAndDobAsync_NormalizedWhitespace_ReturnsPatient()
    {
        var result = await _matcher.MatchByNameAndDobAsync(
            "  John   Doe  ",
            new DateOnly(1980, 5, 15));

        Assert.True(result.IsMatch);
        Assert.Equal(_testPatientId, result.PatientId);
    }

    [Fact]
    public async Task MatchByNameAndDobAsync_WrongDob_ReturnsNotFound()
    {
        var result = await _matcher.MatchByNameAndDobAsync(
            "John Doe",
            new DateOnly(1980, 5, 16));

        Assert.False(result.IsMatch);
    }

    [Fact]
    public async Task MatchByNameAndDobAsync_WrongName_ReturnsNotFound()
    {
        var result = await _matcher.MatchByNameAndDobAsync(
            "John Smith",
            new DateOnly(1980, 5, 15));

        Assert.False(result.IsMatch);
    }

    [Fact]
    public async Task MatchByNameAndDobAsync_EmptyName_ReturnsNotFound()
    {
        var result = await _matcher.MatchByNameAndDobAsync(
            "",
            new DateOnly(1980, 5, 15));

        Assert.False(result.IsMatch);
        Assert.Contains("required", result.Message);
    }

    [Fact]
    public async Task MatchAsync_MrnTakesPriority_ReturnsMrnMatch()
    {
        var identifiers = new PatientIdentifiers
        {
            Mrn = "MRN-12345",
            Name = "Wrong Name",
            DateOfBirth = new DateOnly(1999, 1, 1)
        };

        var result = await _matcher.MatchAsync(identifiers);

        Assert.True(result.IsMatch);
        Assert.Equal(PatientMatchMethod.Mrn, result.MatchMethod);
    }

    [Fact]
    public async Task MatchAsync_MrnNotFound_FallsBackToNameDob()
    {
        var identifiers = new PatientIdentifiers
        {
            Mrn = "MRN-NONEXISTENT",
            Name = "John Doe",
            DateOfBirth = new DateOnly(1980, 5, 15)
        };

        var result = await _matcher.MatchAsync(identifiers);

        Assert.True(result.IsMatch);
        Assert.Equal(PatientMatchMethod.NameDob, result.MatchMethod);
    }

    [Fact]
    public async Task MatchAsync_OnlyNameDob_ReturnsMatch()
    {
        var identifiers = new PatientIdentifiers
        {
            Name = "John Doe",
            DateOfBirth = new DateOnly(1980, 5, 15)
        };

        var result = await _matcher.MatchAsync(identifiers);

        Assert.True(result.IsMatch);
        Assert.Equal(PatientMatchMethod.NameDob, result.MatchMethod);
    }

    [Fact]
    public async Task MatchAsync_InsufficientIdentifiers_ReturnsNotFound()
    {
        var identifiers = new PatientIdentifiers
        {
            Name = "John Doe"
        };

        var result = await _matcher.MatchAsync(identifiers);

        Assert.False(result.IsMatch);
        Assert.Contains("Insufficient", result.Message);
    }

    [Fact]
    public async Task MatchAsync_NullIdentifiers_ReturnsNotFound()
    {
        var result = await _matcher.MatchAsync(null!);

        Assert.False(result.IsMatch);
    }

    [Fact]
    public async Task MatchByNameAndDobAsync_AmbiguousMatch_ReturnsAmbiguous()
    {
        var duplicatePatient = new ErdPatient
        {
            Id = Guid.NewGuid(),
            Mrn = "MRN-DUPLICATE",
            Name = "John Doe",
            Dob = new DateOnly(1980, 5, 15)
        };
        _dbContext.ErdPatients.Add(duplicatePatient);
        await _dbContext.SaveChangesAsync();

        var result = await _matcher.MatchByNameAndDobAsync(
            "John Doe",
            new DateOnly(1980, 5, 15));

        Assert.False(result.IsMatch);
        Assert.True(result.IsAmbiguous);
        Assert.Contains("Multiple patients", result.Message);
    }
}
