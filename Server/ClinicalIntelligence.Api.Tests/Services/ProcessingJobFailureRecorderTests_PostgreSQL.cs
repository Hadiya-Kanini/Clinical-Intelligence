using System;
using System.Text.Json;
using System.Threading.Tasks;
using ClinicalIntelligence.Api.Data;
using ClinicalIntelligence.Api.Domain.Models;
using ClinicalIntelligence.Api.Services.ProcessingJobs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace ClinicalIntelligence.Api.Tests.Services;

/// <summary>
/// Integration tests for ProcessingJobFailureRecorder using real PostgreSQL.
/// Requires PostgreSQL with pgvector extension.
/// </summary>
public class ProcessingJobFailureRecorderTests_PostgreSQL : IClassFixture<TestDatabaseFixture>, IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ProcessingJobFailureRecorder _recorder;
    private readonly Mock<ILogger<ProcessingJobFailureRecorder>> _loggerMock;

    public ProcessingJobFailureRecorderTests_PostgreSQL(TestDatabaseFixture fixture)
    {
        _dbContext = fixture.DbContext;
        _loggerMock = new Mock<ILogger<ProcessingJobFailureRecorder>>();
        _recorder = new ProcessingJobFailureRecorder(_dbContext, _loggerMock.Object);
    }

    public void Dispose()
    {
        // Cleanup handled by TestDatabaseFixture
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
}
