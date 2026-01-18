using ClinicalIntelligence.Api.Data;
using ClinicalIntelligence.Api.Domain.Models;
using ClinicalIntelligence.Api.Services.ProcessingJobs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace ClinicalIntelligence.Api.Tests.Services.ProcessingJobs;

/// <summary>
/// Tests for DbProcessingJobMetadataWriter (US_056 TASK_004).
/// Validates processing_jobs updates for timing, retry, and error capture.
/// </summary>
public sealed class DbProcessingJobMetadataWriterTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly DbProcessingJobMetadataWriter _writer;
    private readonly Guid _testUserId;
    private readonly Guid _testPatientId;

    public DbProcessingJobMetadataWriterTests()
    {
        // Use in-memory database for unit tests
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}")
            .Options;

        _dbContext = new ApplicationDbContext(options);
        var logger = Mock.Of<ILogger<DbProcessingJobMetadataWriter>>();
        _writer = new DbProcessingJobMetadataWriter(_dbContext, logger);

        // Seed test data
        _testUserId = Guid.NewGuid();
        _testPatientId = Guid.NewGuid();
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
            Role = "Standard",
            Status = "Active",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _dbContext.Users.Add(user);

        var patient = new ErdPatient
        {
            Id = _testPatientId,
            Mrn = "MRN001",
            Name = "Test Patient",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _dbContext.ErdPatients.Add(patient);

        _dbContext.SaveChanges();
    }

    private (Document document, ProcessingJob job) CreateDocumentWithJob(string status = "Pending")
    {
        var document = new Document
        {
            Id = Guid.NewGuid(),
            PatientId = _testPatientId,
            UploadedByUserId = _testUserId,
            OriginalName = "test.pdf",
            MimeType = "application/pdf",
            SizeBytes = 1024,
            StoragePath = "/test/path",
            Status = status,
            UploadedAt = DateTime.UtcNow
        };
        _dbContext.Documents.Add(document);

        var job = new ProcessingJob
        {
            Id = Guid.NewGuid(),
            DocumentId = document.Id,
            Status = status,
            RetryCount = 0
        };
        _dbContext.ProcessingJobs.Add(job);

        _dbContext.SaveChanges();
        return (document, job);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
    }

    [Fact]
    public async Task MarkStartedAsync_ValidJob_SetsStatusAndStartedAt()
    {
        // Arrange
        var (_, job) = CreateDocumentWithJob("Pending");

        // Act
        var result = await _writer.MarkStartedAsync(job.Id);

        // Assert
        Assert.True(result);
        var updatedJob = await _dbContext.ProcessingJobs.FindAsync(job.Id);
        Assert.NotNull(updatedJob);
        Assert.Equal("Processing", updatedJob.Status);
        Assert.NotNull(updatedJob.StartedAt);
        Assert.True(updatedJob.StartedAt <= DateTime.UtcNow);
    }

    [Fact]
    public async Task MarkStartedAsync_NonExistentJob_ReturnsFalse()
    {
        // Arrange
        var nonExistentJobId = Guid.NewGuid();

        // Act
        var result = await _writer.MarkStartedAsync(nonExistentJobId);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task MarkCompletedAsync_ValidJob_SetsStatusCompletedAtAndProcessingTime()
    {
        // Arrange
        var (_, job) = CreateDocumentWithJob("Processing");
        var startedAt = DateTime.UtcNow.AddSeconds(-5);
        job.StartedAt = startedAt;
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _writer.MarkCompletedAsync(job.Id);

        // Assert
        Assert.True(result);
        var updatedJob = await _dbContext.ProcessingJobs.FindAsync(job.Id);
        Assert.NotNull(updatedJob);
        Assert.Equal("Completed", updatedJob.Status);
        Assert.NotNull(updatedJob.CompletedAt);
        Assert.NotNull(updatedJob.ProcessingTimeMs);
        Assert.True(updatedJob.ProcessingTimeMs >= 0);
    }

    [Fact]
    public async Task MarkCompletedAsync_JobWithoutStartedAt_SetsNullProcessingTime()
    {
        // Arrange
        var (_, job) = CreateDocumentWithJob("Processing");
        job.StartedAt = null;
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _writer.MarkCompletedAsync(job.Id);

        // Assert
        Assert.True(result);
        var updatedJob = await _dbContext.ProcessingJobs.FindAsync(job.Id);
        Assert.NotNull(updatedJob);
        Assert.Equal("Completed", updatedJob.Status);
        Assert.NotNull(updatedJob.CompletedAt);
        Assert.Null(updatedJob.ProcessingTimeMs);
    }

    [Fact]
    public async Task MarkCompletedAsync_NonExistentJob_ReturnsFalse()
    {
        // Arrange
        var nonExistentJobId = Guid.NewGuid();

        // Act
        var result = await _writer.MarkCompletedAsync(nonExistentJobId);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task MarkFailedAsync_ValidJob_SetsStatusAndErrorMessage()
    {
        // Arrange
        var (_, job) = CreateDocumentWithJob("Processing");
        job.StartedAt = DateTime.UtcNow.AddSeconds(-3);
        await _dbContext.SaveChangesAsync();
        var errorMessage = "Document processing failed due to invalid format";

        // Act
        var result = await _writer.MarkFailedAsync(job.Id, errorMessage);

        // Assert
        Assert.True(result);
        var updatedJob = await _dbContext.ProcessingJobs.FindAsync(job.Id);
        Assert.NotNull(updatedJob);
        Assert.Equal("Failed", updatedJob.Status);
        Assert.NotNull(updatedJob.CompletedAt);
        Assert.NotNull(updatedJob.ProcessingTimeMs);
        Assert.Equal(errorMessage, updatedJob.ErrorMessage);
    }

    [Fact]
    public async Task MarkFailedAsync_LongErrorMessage_TruncatesTo500Chars()
    {
        // Arrange
        var (_, job) = CreateDocumentWithJob("Processing");
        var longErrorMessage = new string('x', 600);

        // Act
        var result = await _writer.MarkFailedAsync(job.Id, longErrorMessage);

        // Assert
        Assert.True(result);
        var updatedJob = await _dbContext.ProcessingJobs.FindAsync(job.Id);
        Assert.NotNull(updatedJob);
        Assert.NotNull(updatedJob.ErrorMessage);
        Assert.True(updatedJob.ErrorMessage.Length <= 500);
        Assert.EndsWith("...", updatedJob.ErrorMessage);
    }

    [Fact]
    public async Task MarkFailedAsync_NullErrorMessage_SetsDefaultMessage()
    {
        // Arrange
        var (_, job) = CreateDocumentWithJob("Processing");

        // Act
        var result = await _writer.MarkFailedAsync(job.Id, null!);

        // Assert
        Assert.True(result);
        var updatedJob = await _dbContext.ProcessingJobs.FindAsync(job.Id);
        Assert.NotNull(updatedJob);
        Assert.Equal("An error occurred during processing.", updatedJob.ErrorMessage);
    }

    [Fact]
    public async Task MarkFailedAsync_WithErrorDetails_StoresDetails()
    {
        // Arrange
        var (_, job) = CreateDocumentWithJob("Processing");
        var errorMessage = "Processing failed";
        var errorDetails = "{\"exception\":\"NullReferenceException\",\"stackTrace\":\"...\"}";

        // Act
        var result = await _writer.MarkFailedAsync(job.Id, errorMessage, errorDetails);

        // Assert
        Assert.True(result);
        var updatedJob = await _dbContext.ProcessingJobs.FindAsync(job.Id);
        Assert.NotNull(updatedJob);
        Assert.Equal(errorDetails, updatedJob.ErrorDetails);
    }

    [Fact]
    public async Task MarkFailedAsync_NonExistentJob_ReturnsFalse()
    {
        // Arrange
        var nonExistentJobId = Guid.NewGuid();

        // Act
        var result = await _writer.MarkFailedAsync(nonExistentJobId, "Error");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task IncrementRetryCountAsync_ValidJob_IncrementsCount()
    {
        // Arrange
        var (_, job) = CreateDocumentWithJob("Processing");
        Assert.Equal(0, job.RetryCount);

        // Act
        var newCount = await _writer.IncrementRetryCountAsync(job.Id);

        // Assert
        Assert.Equal(1, newCount);
        var updatedJob = await _dbContext.ProcessingJobs.FindAsync(job.Id);
        Assert.NotNull(updatedJob);
        Assert.Equal(1, updatedJob.RetryCount);
    }

    [Fact]
    public async Task IncrementRetryCountAsync_MultipleIncrements_AccumulatesCorrectly()
    {
        // Arrange
        var (_, job) = CreateDocumentWithJob("Processing");

        // Act
        await _writer.IncrementRetryCountAsync(job.Id);
        await _writer.IncrementRetryCountAsync(job.Id);
        var finalCount = await _writer.IncrementRetryCountAsync(job.Id);

        // Assert
        Assert.Equal(3, finalCount);
        var updatedJob = await _dbContext.ProcessingJobs.FindAsync(job.Id);
        Assert.NotNull(updatedJob);
        Assert.Equal(3, updatedJob.RetryCount);
    }

    [Fact]
    public async Task IncrementRetryCountAsync_NonExistentJob_ReturnsNegativeOne()
    {
        // Arrange
        var nonExistentJobId = Guid.NewGuid();

        // Act
        var result = await _writer.IncrementRetryCountAsync(nonExistentJobId);

        // Assert
        Assert.Equal(-1, result);
    }

    [Fact]
    public async Task FullWorkflow_StartCompleteSuccess_AllFieldsPopulated()
    {
        // Arrange
        var (_, job) = CreateDocumentWithJob("Pending");

        // Act - simulate full workflow
        await _writer.MarkStartedAsync(job.Id);
        await Task.Delay(10); // Small delay to ensure measurable processing time
        await _writer.MarkCompletedAsync(job.Id);

        // Assert
        var updatedJob = await _dbContext.ProcessingJobs.FindAsync(job.Id);
        Assert.NotNull(updatedJob);
        Assert.Equal("Completed", updatedJob.Status);
        Assert.NotNull(updatedJob.StartedAt);
        Assert.NotNull(updatedJob.CompletedAt);
        Assert.NotNull(updatedJob.ProcessingTimeMs);
        Assert.True(updatedJob.ProcessingTimeMs >= 0);
        Assert.Null(updatedJob.ErrorMessage);
    }

    [Fact]
    public async Task FullWorkflow_StartFailWithRetry_AllFieldsPopulated()
    {
        // Arrange
        var (_, job) = CreateDocumentWithJob("Pending");

        // Act - simulate workflow with retry and failure
        await _writer.MarkStartedAsync(job.Id);
        await _writer.IncrementRetryCountAsync(job.Id);
        await _writer.IncrementRetryCountAsync(job.Id);
        await _writer.MarkFailedAsync(job.Id, "Max retries exceeded");

        // Assert
        var updatedJob = await _dbContext.ProcessingJobs.FindAsync(job.Id);
        Assert.NotNull(updatedJob);
        Assert.Equal("Failed", updatedJob.Status);
        Assert.NotNull(updatedJob.StartedAt);
        Assert.NotNull(updatedJob.CompletedAt);
        Assert.Equal(2, updatedJob.RetryCount);
        Assert.Equal("Max retries exceeded", updatedJob.ErrorMessage);
    }
}
