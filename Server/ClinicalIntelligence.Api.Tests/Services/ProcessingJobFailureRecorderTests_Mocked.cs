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
/// Unit tests for ProcessingJobFailureRecorder using mocked DbContext.
/// No database required - fast and reliable.
/// </summary>
public class ProcessingJobFailureRecorderTests_Mocked : IDisposable
{
    private readonly Mock<IProcessingJobFailureDbContext> _dbContextMock;
    private readonly Mock<DbSet<ProcessingJob>> _dbSetMock;
    private readonly ProcessingJobFailureRecorder _recorder;
    private readonly Mock<ILogger<ProcessingJobFailureRecorder>> _loggerMock;

    public ProcessingJobFailureRecorderTests_Mocked()
    {
        _dbContextMock = new Mock<IProcessingJobFailureDbContext>();
        _dbSetMock = new Mock<DbSet<ProcessingJob>>();
        _loggerMock = new Mock<ILogger<ProcessingJobFailureRecorder>>();

        _dbContextMock.Setup(x => x.ProcessingJobs).Returns(_dbSetMock.Object);
        _dbContextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        _recorder = new ProcessingJobFailureRecorder(_dbContextMock.Object, _loggerMock.Object);
    }

    public void Dispose()
    {
        // No cleanup needed for mocks
    }

    [Fact]
    public async Task RecordValidationFailureAsync_UpdatesJobStatus()
    {
        // Arrange
        var jobId = Guid.NewGuid();
        var job = new ProcessingJob
        {
            Id = jobId,
            DocumentId = Guid.NewGuid(),
            Status = "Running"
        };

        var jobs = new List<ProcessingJob> { job };
        _dbSetMock.Setup(x => x.FindAsync(jobId)).ReturnsAsync(job);

        // Act
        var result = await _recorder.RecordValidationFailureAsync(
            jobId,
            "Validation failed",
            null);

        // Assert
        Assert.True(result);
        Assert.Equal(ProcessingJobFailureRecorder.ValidationFailedStatus, job.Status);
        Assert.NotNull(job.CompletedAt);
        _dbContextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RecordValidationFailureAsync_ReturnsFalseForMissingJob()
    {
        // Arrange
        var nonExistentJobId = Guid.NewGuid();
        _dbSetMock.Setup(x => x.FindAsync(nonExistentJobId)).ReturnsAsync((ProcessingJob)null);

        // Act
        var result = await _recorder.RecordValidationFailureAsync(
            nonExistentJobId,
            "Validation failed",
            null);

        // Assert
        Assert.False(result);
        _dbContextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public void ValidationFailedStatus_IsCorrectValue()
    {
        // Assert
        Assert.Equal("Validation_Failed", ProcessingJobFailureRecorder.ValidationFailedStatus);
    }
}
