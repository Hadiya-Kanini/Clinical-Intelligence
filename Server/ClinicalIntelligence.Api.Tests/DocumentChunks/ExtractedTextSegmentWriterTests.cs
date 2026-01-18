using ClinicalIntelligence.Api.Data;
using ClinicalIntelligence.Api.Domain.Models;
using ClinicalIntelligence.Api.Services.DocumentChunks;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace ClinicalIntelligence.Api.Tests.DocumentChunks;

/// <summary>
/// Unit tests for DbExtractedTextSegmentWriter.
/// Tests metadata preservation and safe handling of missing coordinates.
/// </summary>
public class ExtractedTextSegmentWriterTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly DbExtractedTextSegmentWriter _writer;
    private readonly Guid _testUserId;
    private readonly Guid _testPatientId;
    private readonly Guid _testDocumentId;

    public ExtractedTextSegmentWriterTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _dbContext = new ApplicationDbContext(options);
        _writer = new DbExtractedTextSegmentWriter(_dbContext);

        _testUserId = Guid.NewGuid();
        _testPatientId = Guid.NewGuid();
        _testDocumentId = Guid.NewGuid();

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

        var patient = new ErdPatient
        {
            Id = _testPatientId,
            Mrn = "MRN-TEST-001",
            Name = "Test Patient"
        };
        _dbContext.ErdPatients.Add(patient);

        var document = new Document
        {
            Id = _testDocumentId,
            PatientId = _testPatientId,
            UploadedByUserId = _testUserId,
            OriginalName = "test.pdf",
            MimeType = "application/pdf",
            StoragePath = "/test/path/test.pdf",
            Status = "Pending"
        };
        _dbContext.Documents.Add(document);

        _dbContext.SaveChanges();
    }

    public void Dispose()
    {
        _dbContext.Dispose();
    }

    [Fact]
    public async Task WriteSegmentsAsync_WithValidSegments_PersistsDocumentChunks()
    {
        var segments = new List<ExtractedSegmentInput>
        {
            new() { Text = "First segment text", Page = 1, Section = "Introduction" },
            new() { Text = "Second segment text", Page = 2, Section = "Methods" }
        };

        var result = await _writer.WriteSegmentsAsync(_testDocumentId, segments);

        Assert.Equal(2, result.Count);
        var chunks = await _dbContext.DocumentChunks.Where(c => c.DocumentId == _testDocumentId).ToListAsync();
        Assert.Equal(2, chunks.Count);
    }

    [Fact]
    public async Task WriteSegmentsAsync_PreservesPageMetadata()
    {
        var segments = new List<ExtractedSegmentInput>
        {
            new() { Text = "Page 5 content", Page = 5 }
        };

        await _writer.WriteSegmentsAsync(_testDocumentId, segments);

        var chunk = await _dbContext.DocumentChunks.FirstAsync(c => c.DocumentId == _testDocumentId);
        Assert.Equal(5, chunk.Page);
    }

    [Fact]
    public async Task WriteSegmentsAsync_PreservesSectionMetadata()
    {
        var segments = new List<ExtractedSegmentInput>
        {
            new() { Text = "Section content", Section = "Diagnosis" }
        };

        await _writer.WriteSegmentsAsync(_testDocumentId, segments);

        var chunk = await _dbContext.DocumentChunks.FirstAsync(c => c.DocumentId == _testDocumentId);
        Assert.Equal("Diagnosis", chunk.Section);
    }

    [Fact]
    public async Task WriteSegmentsAsync_PreservesCoordinatesAsJson()
    {
        var coordinatesJson = "{\"x0\":10,\"y0\":20,\"x1\":100,\"y1\":50}";
        var segments = new List<ExtractedSegmentInput>
        {
            new() { Text = "Coordinates content", Coordinates = coordinatesJson }
        };

        await _writer.WriteSegmentsAsync(_testDocumentId, segments);

        var chunk = await _dbContext.DocumentChunks.FirstAsync(c => c.DocumentId == _testDocumentId);
        Assert.Equal(coordinatesJson, chunk.Coordinates);
    }

    [Fact]
    public async Task WriteSegmentsAsync_HandlesNullMetadataSafely()
    {
        var segments = new List<ExtractedSegmentInput>
        {
            new() { Text = "Text only, no metadata", Page = null, Section = null, Coordinates = null }
        };

        var result = await _writer.WriteSegmentsAsync(_testDocumentId, segments);

        Assert.Single(result);
        var chunk = await _dbContext.DocumentChunks.FirstAsync(c => c.DocumentId == _testDocumentId);
        Assert.Equal("Text only, no metadata", chunk.TextContent);
        Assert.Null(chunk.Page);
        Assert.Null(chunk.Section);
        Assert.Null(chunk.Coordinates);
    }

    [Fact]
    public async Task WriteSegmentsAsync_SkipsEmptyTextSegments()
    {
        var segments = new List<ExtractedSegmentInput>
        {
            new() { Text = "Valid text" },
            new() { Text = "" },
            new() { Text = "   " },
            new() { Text = "Another valid text" }
        };

        var result = await _writer.WriteSegmentsAsync(_testDocumentId, segments);

        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task WriteSegmentsAsync_GeneratesChunkHash()
    {
        var segments = new List<ExtractedSegmentInput>
        {
            new() { Text = "Hash test content" }
        };

        await _writer.WriteSegmentsAsync(_testDocumentId, segments);

        var chunk = await _dbContext.DocumentChunks.FirstAsync(c => c.DocumentId == _testDocumentId);
        Assert.NotNull(chunk.ChunkHash);
        Assert.Equal(64, chunk.ChunkHash.Length);
    }

    [Fact]
    public async Task WriteSegmentsAsync_LeavesEmbeddingNull()
    {
        var segments = new List<ExtractedSegmentInput>
        {
            new() { Text = "Embedding should be null" }
        };

        await _writer.WriteSegmentsAsync(_testDocumentId, segments);

        var chunk = await _dbContext.DocumentChunks.FirstAsync(c => c.DocumentId == _testDocumentId);
        Assert.Null(chunk.Embedding);
    }

    [Fact]
    public async Task WriteSegmentsAsync_TruncatesLongSection()
    {
        var longSection = new string('A', 150);
        var segments = new List<ExtractedSegmentInput>
        {
            new() { Text = "Content", Section = longSection }
        };

        await _writer.WriteSegmentsAsync(_testDocumentId, segments);

        var chunk = await _dbContext.DocumentChunks.FirstAsync(c => c.DocumentId == _testDocumentId);
        Assert.Equal(100, chunk.Section!.Length);
    }

    [Fact]
    public async Task WriteSegmentsAsync_ThrowsForNonExistentDocument()
    {
        var nonExistentDocId = Guid.NewGuid();
        var segments = new List<ExtractedSegmentInput>
        {
            new() { Text = "Content" }
        };

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _writer.WriteSegmentsAsync(nonExistentDocId, segments));
    }

    [Fact]
    public async Task WriteSegmentsAsync_ThrowsForNullSegments()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(
            () => _writer.WriteSegmentsAsync(_testDocumentId, null!));
    }

    [Fact]
    public async Task WriteSegmentsAsync_ReturnsEmptyListForEmptyInput()
    {
        var segments = new List<ExtractedSegmentInput>();

        var result = await _writer.WriteSegmentsAsync(_testDocumentId, segments);

        Assert.Empty(result);
    }
}
