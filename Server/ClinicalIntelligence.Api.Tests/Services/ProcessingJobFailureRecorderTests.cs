using System;
using System.Text.Json;
using System.Threading.Tasks;
using ClinicalIntelligence.Api.Domain.Models;
using ClinicalIntelligence.Api.Services.ProcessingJobs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace ClinicalIntelligence.Api.Tests.Services;

/// <summary>
/// Unit tests for ProcessingJobFailureRecorder.
/// Validates status and error field persistence for validation failures.
/// NOTE: Currently skipped due to in-memory database not supporting pgvector.
/// Services are validated with integration tests using real PostgreSQL.
/// </summary>
[Trait("Category", "RequiresPostgreSQL")]
public class ProcessingJobFailureRecorderTests : IDisposable
{
    private readonly TestApplicationDbContext _dbContext;
    private readonly ProcessingJobFailureRecorder _recorder;
    private readonly Mock<ILogger<ProcessingJobFailureRecorder>> _loggerMock;

    public ProcessingJobFailureRecorderTests()
    {
        var options = new DbContextOptionsBuilder<TestApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _dbContext = new TestApplicationDbContext(options);
        _loggerMock = new Mock<ILogger<ProcessingJobFailureRecorder>>();
        _recorder = new ProcessingJobFailureRecorder(_dbContext, _loggerMock.Object);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
    }

    [Fact]
    public async Task RecordValidationFailureAsync_SetsStatusToValidationFailed()
    {
        // Arrange
        var jobId = Guid.NewGuid();
        var documentId = Guid.NewGuid();
        
        var job = new ProcessingJob
        {
            Id = jobId,
            DocumentId = documentId,
            Status = "Running"
        };
        _dbContext.ProcessingJobs.Add(job);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _recorder.RecordValidationFailureAsync(
            jobId,
            "Validation failed",
            null);

        // Assert
        Assert.True(result);
        
        var updatedJob = await _dbContext.ProcessingJobs.FindAsync(jobId);
        Assert.NotNull(updatedJob);
        Assert.Equal(ProcessingJobFailureRecorder.ValidationFailedStatus, updatedJob.Status);
    }

    [Fact]
    public async Task RecordValidationFailureAsync_PersistsErrorMessage()
    {
        // Arrange
        var jobId = Guid.NewGuid();
        var documentId = Guid.NewGuid();
        var errorMessage = "Entity payload validation failed with 3 error(s)";
        
        var job = new ProcessingJob
        {
            Id = jobId,
            DocumentId = documentId,
            Status = "Running"
        };
        _dbContext.ProcessingJobs.Add(job);
        await _dbContext.SaveChangesAsync();

        // Act
        await _recorder.RecordValidationFailureAsync(jobId, errorMessage, null);

        // Assert
        var updatedJob = await _dbContext.ProcessingJobs.FindAsync(jobId);
        Assert.NotNull(updatedJob);
        Assert.Equal(errorMessage, updatedJob.ErrorMessage);
    }

    [Fact]
    public async Task RecordValidationFailureAsync_PersistsErrorDetailsAsJson()
    {
        // Arrange
        var jobId = Guid.NewGuid();
        var documentId = Guid.NewGuid();
        var errorDetails = new[]
        {
            new { field_path = "document_id", error_type = "missing", message = "document_id is required" },
            new { field_path = "extracted_entities.0.entity_value", error_type = "missing", message = "entity_value is required" }
        };
        var errorDetailsJson = JsonSerializer.Serialize(errorDetails);
        
        var job = new ProcessingJob
        {
            Id = jobId,
            DocumentId = documentId,
            Status = "Running"
        };
        _dbContext.ProcessingJobs.Add(job);
        await _dbContext.SaveChangesAsync();

        // Act
        await _recorder.RecordValidationFailureAsync(jobId, "Validation failed", errorDetailsJson);

        // Assert
        var updatedJob = await _dbContext.ProcessingJobs.FindAsync(jobId);
        Assert.NotNull(updatedJob);
        Assert.Equal(errorDetailsJson, updatedJob.ErrorDetails);
        
        // Verify JSON round-trips correctly
        var deserializedDetails = JsonSerializer.Deserialize<JsonElement[]>(updatedJob.ErrorDetails!);
        Assert.NotNull(deserializedDetails);
        Assert.Equal(2, deserializedDetails.Length);
    }

    [Fact]
    public async Task RecordValidationFailureAsync_SetsCompletedAt()
    {
        // Arrange
        var jobId = Guid.NewGuid();
        var documentId = Guid.NewGuid();
        var beforeTime = DateTime.UtcNow;
        
        var job = new ProcessingJob
        {
            Id = jobId,
            DocumentId = documentId,
            Status = "Running"
        };
        _dbContext.ProcessingJobs.Add(job);
        await _dbContext.SaveChangesAsync();

        // Act
        await _recorder.RecordValidationFailureAsync(jobId, "Validation failed", null);

        // Assert
        var updatedJob = await _dbContext.ProcessingJobs.FindAsync(jobId);
        Assert.NotNull(updatedJob);
        Assert.NotNull(updatedJob.CompletedAt);
        Assert.True(updatedJob.CompletedAt >= beforeTime);
    }

    [Fact]
    public async Task RecordValidationFailureAsync_ReturnsFalseForMissingJob()
    {
        // Arrange
        var nonExistentJobId = Guid.NewGuid();

        // Act
        var result = await _recorder.RecordValidationFailureAsync(
            nonExistentJobId,
            "Validation failed",
            null);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task RecordValidationFailureAsync_ThrowsForEmptyJobId()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _recorder.RecordValidationFailureAsync(Guid.Empty, "Error", null));
    }

    [Fact]
    public async Task RecordValidationFailureAsync_ThrowsForEmptyErrorMessage()
    {
        // Arrange
        var jobId = Guid.NewGuid();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _recorder.RecordValidationFailureAsync(jobId, "", null));
        
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _recorder.RecordValidationFailureAsync(jobId, "   ", null));
    }

    [Fact]
    public void ValidationFailedStatus_IsCorrectValue()
    {
        // Assert
        Assert.Equal("Validation_Failed", ProcessingJobFailureRecorder.ValidationFailedStatus);
    }
}
