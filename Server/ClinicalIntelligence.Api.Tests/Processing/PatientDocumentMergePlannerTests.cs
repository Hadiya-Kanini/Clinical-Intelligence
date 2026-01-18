using ClinicalIntelligence.Api.Data;
using ClinicalIntelligence.Api.Domain.Models;
using ClinicalIntelligence.Api.Services.Processing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace ClinicalIntelligence.Api.Tests.Processing;

/// <summary>
/// Unit tests for PatientDocumentMergePlanner.
/// Tests stable document ordering and patient scoping in merge planning.
/// </summary>
public class PatientDocumentMergePlannerTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly PatientDocumentMergePlanner _planner;
    private readonly Guid _testUserId;
    private readonly Guid _testPatientId;
    private readonly Guid _testPatient2Id;
    private readonly List<Guid> _testDocumentIds;

    public PatientDocumentMergePlannerTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _dbContext = new ApplicationDbContext(options);
        var logger = new Mock<ILogger<PatientDocumentMergePlanner>>();
        _planner = new PatientDocumentMergePlanner(_dbContext, logger.Object);

        _testUserId = Guid.NewGuid();
        _testPatientId = Guid.NewGuid();
        _testPatient2Id = Guid.NewGuid();
        _testDocumentIds = new List<Guid>();

        SeedTestData();
    }

    private void SeedTestData()
    {
        var user = new User
        {
            Id = _testUserId,
            Email = "test@example.com",
            PasswordHash = "hash",
            Name = "Test User",
            Role = "Admin",
            Status = "Active"
        };
        _dbContext.Users.Add(user);

        var patient1 = new ErdPatient
        {
            Id = _testPatientId,
            Mrn = "MRN-001",
            Name = "Patient One"
        };
        _dbContext.ErdPatients.Add(patient1);

        var patient2 = new ErdPatient
        {
            Id = _testPatient2Id,
            Mrn = "MRN-002",
            Name = "Patient Two"
        };
        _dbContext.ErdPatients.Add(patient2);

        var baseTime = new DateTime(2026, 1, 1, 10, 0, 0, DateTimeKind.Utc);

        for (int i = 0; i < 3; i++)
        {
            var docId = Guid.NewGuid();
            _testDocumentIds.Add(docId);

            var doc = new Document
            {
                Id = docId,
                PatientId = _testPatientId,
                UploadedByUserId = _testUserId,
                OriginalName = $"doc{i + 1}.pdf",
                MimeType = "application/pdf",
                StoragePath = $"/test/doc{i + 1}.pdf",
                Status = "Pending",
                UploadedAt = baseTime.AddHours(i)
            };
            _dbContext.Documents.Add(doc);
        }

        var otherPatientDoc = new Document
        {
            Id = Guid.NewGuid(),
            PatientId = _testPatient2Id,
            UploadedByUserId = _testUserId,
            OriginalName = "other.pdf",
            MimeType = "application/pdf",
            StoragePath = "/test/other.pdf",
            Status = "Pending",
            UploadedAt = baseTime
        };
        _dbContext.Documents.Add(otherPatientDoc);

        _dbContext.SaveChanges();
    }

    public void Dispose()
    {
        _dbContext.Dispose();
    }

    [Fact]
    public async Task PlanMergeAsync_ValidPatient_ReturnsAllDocuments()
    {
        var result = await _planner.PlanMergeAsync(_testPatientId);

        Assert.True(result.IsValid);
        Assert.Equal(_testPatientId, result.PatientId);
        Assert.Equal(3, result.DocumentIds.Count);
    }

    [Fact]
    public async Task PlanMergeAsync_DocumentsOrderedByUploadedAt()
    {
        var result = await _planner.PlanMergeAsync(_testPatientId);

        Assert.True(result.IsValid);
        Assert.Equal(_testDocumentIds[0], result.DocumentIds[0]);
        Assert.Equal(_testDocumentIds[1], result.DocumentIds[1]);
        Assert.Equal(_testDocumentIds[2], result.DocumentIds[2]);
    }

    [Fact]
    public async Task PlanMergeAsync_PatientNotFound_ReturnsInvalid()
    {
        var nonExistentPatientId = Guid.NewGuid();

        var result = await _planner.PlanMergeAsync(nonExistentPatientId);

        Assert.False(result.IsValid);
        Assert.Contains("not found", result.ErrorMessage);
    }

    [Fact]
    public async Task PlanMergeAsync_PatientWithNoDocuments_ReturnsInvalid()
    {
        var emptyPatient = new ErdPatient
        {
            Id = Guid.NewGuid(),
            Mrn = "MRN-EMPTY",
            Name = "Empty Patient"
        };
        _dbContext.ErdPatients.Add(emptyPatient);
        await _dbContext.SaveChangesAsync();

        var result = await _planner.PlanMergeAsync(emptyPatient.Id);

        Assert.False(result.IsValid);
        Assert.Contains("No documents", result.ErrorMessage);
    }

    [Fact]
    public async Task PlanMergeAsync_OnlyReturnsPatientDocuments()
    {
        var result = await _planner.PlanMergeAsync(_testPatientId);

        Assert.True(result.IsValid);
        foreach (var docId in result.DocumentIds)
        {
            Assert.Contains(docId, _testDocumentIds);
        }
    }

    [Fact]
    public async Task PlanMergeAsync_WithSpecificDocuments_ReturnsOnlyRequested()
    {
        var requestedIds = new[] { _testDocumentIds[0], _testDocumentIds[2] };

        var result = await _planner.PlanMergeAsync(_testPatientId, requestedIds);

        Assert.True(result.IsValid);
        Assert.Equal(2, result.DocumentIds.Count);
        Assert.Contains(_testDocumentIds[0], result.DocumentIds);
        Assert.Contains(_testDocumentIds[2], result.DocumentIds);
    }

    [Fact]
    public async Task PlanMergeAsync_WithSpecificDocuments_MaintainsOrder()
    {
        var requestedIds = new[] { _testDocumentIds[2], _testDocumentIds[0] };

        var result = await _planner.PlanMergeAsync(_testPatientId, requestedIds);

        Assert.True(result.IsValid);
        Assert.Equal(_testDocumentIds[0], result.DocumentIds[0]);
        Assert.Equal(_testDocumentIds[2], result.DocumentIds[1]);
    }

    [Fact]
    public async Task PlanMergeAsync_WithInvalidDocumentIds_IgnoresInvalid()
    {
        var invalidId = Guid.NewGuid();
        var requestedIds = new[] { _testDocumentIds[0], invalidId };

        var result = await _planner.PlanMergeAsync(_testPatientId, requestedIds);

        Assert.True(result.IsValid);
        Assert.Single(result.DocumentIds);
        Assert.Equal(_testDocumentIds[0], result.DocumentIds[0]);
    }

    [Fact]
    public async Task PlanMergeAsync_WithOtherPatientDocument_IgnoresIt()
    {
        var otherPatientDocId = await _dbContext.Documents
            .Where(d => d.PatientId == _testPatient2Id)
            .Select(d => d.Id)
            .FirstAsync();

        var requestedIds = new[] { _testDocumentIds[0], otherPatientDocId };

        var result = await _planner.PlanMergeAsync(_testPatientId, requestedIds);

        Assert.True(result.IsValid);
        Assert.Single(result.DocumentIds);
        Assert.DoesNotContain(otherPatientDocId, result.DocumentIds);
    }

    [Fact]
    public async Task PlanMergeAsync_EmptyDocumentIds_ReturnsInvalid()
    {
        var result = await _planner.PlanMergeAsync(_testPatientId, Array.Empty<Guid>());

        Assert.False(result.IsValid);
        Assert.Contains("No document IDs", result.ErrorMessage);
    }

    [Fact]
    public async Task PlanMergeAsync_AllInvalidDocumentIds_ReturnsInvalid()
    {
        var invalidIds = new[] { Guid.NewGuid(), Guid.NewGuid() };

        var result = await _planner.PlanMergeAsync(_testPatientId, invalidIds);

        Assert.False(result.IsValid);
        Assert.Contains("No valid documents", result.ErrorMessage);
    }

    [Fact]
    public async Task ToJobPayload_ProducesCorrectStructure()
    {
        var result = await _planner.PlanMergeAsync(_testPatientId);

        var payload = result.ToJobPayload();
        var payloadType = payload.GetType();

        var patientIdProp = payloadType.GetProperty("patient_id");
        var documentIdsProp = payloadType.GetProperty("document_ids");

        Assert.NotNull(patientIdProp);
        Assert.NotNull(documentIdsProp);
        Assert.Equal(_testPatientId.ToString(), patientIdProp.GetValue(payload));
    }

    [Fact]
    public async Task PlanMergeAsync_DeterministicOrdering_SameResultOnMultipleCalls()
    {
        var result1 = await _planner.PlanMergeAsync(_testPatientId);
        var result2 = await _planner.PlanMergeAsync(_testPatientId);

        Assert.Equal(result1.DocumentIds.Count, result2.DocumentIds.Count);
        for (int i = 0; i < result1.DocumentIds.Count; i++)
        {
            Assert.Equal(result1.DocumentIds[i], result2.DocumentIds[i]);
        }
    }
}
