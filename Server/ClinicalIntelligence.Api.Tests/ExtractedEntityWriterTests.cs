using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ClinicalIntelligence.Api.Services.ExtractedEntities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace ClinicalIntelligence.Api.Tests;

/// <summary>
/// Unit tests for DbExtractedEntityWriter.
/// Validates persistence behavior and mapping correctness.
/// NOTE: Currently skipped due to in-memory database not supporting pgvector.
/// Services are validated with integration tests using real PostgreSQL.
/// </summary>
[Trait("Category", "RequiresPostgreSQL")]
public class ExtractedEntityWriterTests : IDisposable
{
    private readonly TestApplicationDbContext _dbContext;
    private readonly DbExtractedEntityWriter _writer;
    private readonly Mock<ILogger<DbExtractedEntityWriter>> _loggerMock;

    public ExtractedEntityWriterTests()
    {
        var options = new DbContextOptionsBuilder<TestApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _dbContext = new TestApplicationDbContext(options);
        _loggerMock = new Mock<ILogger<DbExtractedEntityWriter>>();
        _writer = new DbExtractedEntityWriter(_dbContext, _loggerMock.Object);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
    }

    [Fact]
    public async Task WriteEntitiesAsync_PersistsCorrectMappings()
    {
        // Arrange
        var patientId = Guid.NewGuid();
        var documentId = Guid.NewGuid();
        var entities = new List<ExtractedEntityDto>
        {
            new() { EntityGroupName = "patient_demographics", EntityName = "name", EntityValue = "Jane Doe" },
            new() { EntityGroupName = "medications", EntityName = "medication_name", EntityValue = "Aspirin 81mg" },
        };

        // Act
        var count = await _writer.WriteEntitiesAsync(patientId, documentId, entities);

        // Assert
        Assert.Equal(2, count);

        var persisted = await _dbContext.ExtractedEntities
            .Where(e => e.DocumentId == documentId)
            .ToListAsync();

        Assert.Equal(2, persisted.Count);

        var demographics = persisted.First(e => e.Category == "patient_demographics");
        Assert.Equal("name", demographics.Name);
        Assert.Equal("Jane Doe", demographics.Value);
        Assert.Equal(patientId, demographics.PatientId);

        var medication = persisted.First(e => e.Category == "medications");
        Assert.Equal("medication_name", medication.Name);
        Assert.Equal("Aspirin 81mg", medication.Value);
    }

    [Fact]
    public async Task WriteEntitiesAsync_EmptyListWritesZeroRows()
    {
        // Arrange
        var patientId = Guid.NewGuid();
        var documentId = Guid.NewGuid();
        var entities = new List<ExtractedEntityDto>();

        // Act
        var count = await _writer.WriteEntitiesAsync(patientId, documentId, entities);

        // Assert
        Assert.Equal(0, count);

        var persisted = await _dbContext.ExtractedEntities
            .Where(e => e.DocumentId == documentId)
            .ToListAsync();

        Assert.Empty(persisted);
    }

    [Fact]
    public async Task WriteEntitiesAsync_NullListWritesZeroRows()
    {
        // Arrange
        var patientId = Guid.NewGuid();
        var documentId = Guid.NewGuid();

        // Act
        var count = await _writer.WriteEntitiesAsync(patientId, documentId, null!);

        // Assert
        Assert.Equal(0, count);
    }

    [Fact]
    public async Task WriteEntitiesAsync_TruncatesLongCategory()
    {
        // Arrange
        var patientId = Guid.NewGuid();
        var documentId = Guid.NewGuid();
        var longCategory = new string('a', 100);
        var entities = new List<ExtractedEntityDto>
        {
            new() { EntityGroupName = longCategory, EntityName = "test", EntityValue = "value" },
        };

        // Act
        var count = await _writer.WriteEntitiesAsync(patientId, documentId, entities);

        // Assert
        Assert.Equal(1, count);

        var persisted = await _dbContext.ExtractedEntities.FirstAsync(e => e.DocumentId == documentId);
        Assert.Equal(50, persisted.Category.Length);
    }

    [Fact]
    public async Task WriteEntitiesAsync_TruncatesLongName()
    {
        // Arrange
        var patientId = Guid.NewGuid();
        var documentId = Guid.NewGuid();
        var longName = new string('b', 300);
        var entities = new List<ExtractedEntityDto>
        {
            new() { EntityGroupName = "test", EntityName = longName, EntityValue = "value" },
        };

        // Act
        var count = await _writer.WriteEntitiesAsync(patientId, documentId, entities);

        // Assert
        Assert.Equal(1, count);

        var persisted = await _dbContext.ExtractedEntities.FirstAsync(e => e.DocumentId == documentId);
        Assert.Equal(200, persisted.Name.Length);
    }

    [Fact]
    public async Task WriteEntitiesAsync_TruncatesLongValue()
    {
        // Arrange
        var patientId = Guid.NewGuid();
        var documentId = Guid.NewGuid();
        var longValue = new string('c', 600);
        var entities = new List<ExtractedEntityDto>
        {
            new() { EntityGroupName = "test", EntityName = "name", EntityValue = longValue },
        };

        // Act
        var count = await _writer.WriteEntitiesAsync(patientId, documentId, entities);

        // Assert
        Assert.Equal(1, count);

        var persisted = await _dbContext.ExtractedEntities.FirstAsync(e => e.DocumentId == documentId);
        Assert.Equal(500, persisted.Value!.Length);
    }

    [Fact]
    public async Task WriteEntitiesAsync_ThrowsForEmptyPatientId()
    {
        // Arrange
        var documentId = Guid.NewGuid();
        var entities = new List<ExtractedEntityDto>
        {
            new() { EntityGroupName = "test", EntityName = "name", EntityValue = "value" },
        };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _writer.WriteEntitiesAsync(Guid.Empty, documentId, entities));
    }

    [Fact]
    public async Task WriteEntitiesAsync_ThrowsForEmptyDocumentId()
    {
        // Arrange
        var patientId = Guid.NewGuid();
        var entities = new List<ExtractedEntityDto>
        {
            new() { EntityGroupName = "test", EntityName = "name", EntityValue = "value" },
        };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _writer.WriteEntitiesAsync(patientId, Guid.Empty, entities));
    }

    [Fact]
    public async Task WriteEntitiesAsync_SetsIsVerifiedToFalse()
    {
        // Arrange
        var patientId = Guid.NewGuid();
        var documentId = Guid.NewGuid();
        var entities = new List<ExtractedEntityDto>
        {
            new() { EntityGroupName = "test", EntityName = "name", EntityValue = "value" },
        };

        // Act
        await _writer.WriteEntitiesAsync(patientId, documentId, entities);

        // Assert
        var persisted = await _dbContext.ExtractedEntities.FirstAsync(e => e.DocumentId == documentId);
        Assert.False(persisted.IsVerified);
    }
}
