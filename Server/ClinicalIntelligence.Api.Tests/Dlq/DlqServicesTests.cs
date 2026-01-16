using ClinicalIntelligence.Api.Contracts.Dlq;
using ClinicalIntelligence.Api.Domain.Models;
using Xunit;

namespace ClinicalIntelligence.Api.Tests.Dlq;

/// <summary>
/// Unit tests for DLQ domain models and contracts (US_055 TASK_005).
/// Note: Full integration tests require PostgreSQL due to pgvector dependency.
/// </summary>
public class DlqServicesTests
{
    #region DeadLetterJob Model Tests

    [Fact]
    public void DeadLetterJob_DefaultStatus_IsPending()
    {
        // Arrange & Act
        var dlqJob = new DeadLetterJob();

        // Assert
        Assert.Equal(DeadLetterJobStatus.Pending, dlqJob.Status);
    }

    [Fact]
    public void DeadLetterJob_CanSetAllRequiredFields()
    {
        // Arrange
        var id = Guid.NewGuid();
        var processingJobId = Guid.NewGuid();
        var documentId = Guid.NewGuid();
        var deadLetteredAt = DateTime.UtcNow;

        // Act
        var dlqJob = new DeadLetterJob
        {
            Id = id,
            ProcessingJobId = processingJobId,
            DocumentId = documentId,
            OriginalMessage = "{\"test\": \"message\"}",
            MessageSchemaVersion = "1.0",
            ErrorMessage = "Test error",
            ErrorDetails = "{\"stackTrace\": \"...\"}",
            RetryHistory = "[{\"attempt\": 1}]",
            RetryCount = 3,
            DeadLetterReason = "Max retries exhausted",
            DeadLetteredAt = deadLetteredAt,
            Status = DeadLetterJobStatus.Pending
        };

        // Assert
        Assert.Equal(id, dlqJob.Id);
        Assert.Equal(processingJobId, dlqJob.ProcessingJobId);
        Assert.Equal(documentId, dlqJob.DocumentId);
        Assert.Equal("{\"test\": \"message\"}", dlqJob.OriginalMessage);
        Assert.Equal("1.0", dlqJob.MessageSchemaVersion);
        Assert.Equal("Test error", dlqJob.ErrorMessage);
        Assert.Equal(3, dlqJob.RetryCount);
        Assert.Equal("Max retries exhausted", dlqJob.DeadLetterReason);
        Assert.Equal(deadLetteredAt, dlqJob.DeadLetteredAt);
    }

    [Fact]
    public void DeadLetterJob_ActionTrackingFields_AreNullByDefault()
    {
        // Arrange & Act
        var dlqJob = new DeadLetterJob();

        // Assert
        Assert.Null(dlqJob.LastActionAt);
        Assert.Null(dlqJob.LastActionByUserId);
        Assert.Null(dlqJob.LastReplayError);
        Assert.Null(dlqJob.ReplayedJobId);
        Assert.Equal(0, dlqJob.ReplayAttempts);
    }

    #endregion

    #region DeadLetterJobStatus Constants Tests

    [Fact]
    public void DeadLetterJobStatus_HasCorrectValues()
    {
        Assert.Equal("Pending", DeadLetterJobStatus.Pending);
        Assert.Equal("Replayed", DeadLetterJobStatus.Replayed);
        Assert.Equal("Discarded", DeadLetterJobStatus.Discarded);
    }

    #endregion

    #region DlqListQuery Tests

    [Fact]
    public void DlqListQuery_DefaultValues_AreCorrect()
    {
        // Arrange & Act
        var query = new DlqListQuery();

        // Assert
        Assert.Equal(1, query.Page);
        Assert.Equal(20, query.PageSize);
        Assert.Null(query.DocumentId);
        Assert.Null(query.ProcessingJobId);
        Assert.Null(query.Status);
        Assert.Null(query.FromDate);
        Assert.Null(query.ToDate);
    }

    [Fact]
    public void DlqListQuery_CanSetAllFilters()
    {
        // Arrange
        var documentId = Guid.NewGuid();
        var processingJobId = Guid.NewGuid();
        var fromDate = DateTime.UtcNow.AddDays(-7);
        var toDate = DateTime.UtcNow;

        // Act
        var query = new DlqListQuery
        {
            Page = 2,
            PageSize = 50,
            DocumentId = documentId,
            ProcessingJobId = processingJobId,
            Status = "Pending",
            FromDate = fromDate,
            ToDate = toDate
        };

        // Assert
        Assert.Equal(2, query.Page);
        Assert.Equal(50, query.PageSize);
        Assert.Equal(documentId, query.DocumentId);
        Assert.Equal(processingJobId, query.ProcessingJobId);
        Assert.Equal("Pending", query.Status);
        Assert.Equal(fromDate, query.FromDate);
        Assert.Equal(toDate, query.ToDate);
    }

    #endregion

    #region DlqItemResponse Tests

    [Fact]
    public void DlqItemResponse_CanBeCreatedWithAllFields()
    {
        // Arrange
        var id = Guid.NewGuid();
        var processingJobId = Guid.NewGuid();
        var documentId = Guid.NewGuid();
        var deadLetteredAt = DateTime.UtcNow;

        // Act
        var response = new DlqItemResponse
        {
            Id = id,
            ProcessingJobId = processingJobId,
            DocumentId = documentId,
            OriginalMessage = "{\"test\": true}",
            MessageSchemaVersion = "1.0",
            ErrorMessage = "Error occurred",
            ErrorDetails = "{\"details\": \"...\"}",
            RetryHistory = "[]",
            RetryCount = 3,
            DeadLetterReason = "Max retries",
            DeadLetteredAt = deadLetteredAt,
            Status = "Pending",
            ReplayAttempts = 0
        };

        // Assert
        Assert.Equal(id, response.Id);
        Assert.Equal(processingJobId, response.ProcessingJobId);
        Assert.Equal(documentId, response.DocumentId);
        Assert.Equal("{\"test\": true}", response.OriginalMessage);
        Assert.Equal("Error occurred", response.ErrorMessage);
        Assert.Equal(3, response.RetryCount);
    }

    #endregion

    #region DlqReplayResponse Tests

    [Fact]
    public void DlqReplayResponse_SuccessfulReplay_HasCorrectFields()
    {
        // Arrange
        var dlqId = Guid.NewGuid();
        var newJobId = Guid.NewGuid();

        // Act
        var response = new DlqReplayResponse
        {
            DeadLetterJobId = dlqId,
            NewJobId = newJobId,
            Status = "Replayed",
            ReplayAttempts = 1,
            Success = true,
            Message = "Job replayed successfully."
        };

        // Assert
        Assert.Equal(dlqId, response.DeadLetterJobId);
        Assert.Equal(newJobId, response.NewJobId);
        Assert.Equal("Replayed", response.Status);
        Assert.Equal(1, response.ReplayAttempts);
        Assert.True(response.Success);
        Assert.Equal("Job replayed successfully.", response.Message);
    }

    [Fact]
    public void DlqReplayResponse_FailedReplay_HasNoNewJobId()
    {
        // Arrange & Act
        var response = new DlqReplayResponse
        {
            DeadLetterJobId = Guid.NewGuid(),
            NewJobId = null,
            Status = "Pending",
            ReplayAttempts = 1,
            Success = false,
            Message = "Failed to publish job to queue."
        };

        // Assert
        Assert.Null(response.NewJobId);
        Assert.False(response.Success);
    }

    #endregion

    #region DlqDiscardResponse Tests

    [Fact]
    public void DlqDiscardResponse_SuccessfulDiscard_HasCorrectFields()
    {
        // Arrange
        var dlqId = Guid.NewGuid();

        // Act
        var response = new DlqDiscardResponse
        {
            DeadLetterJobId = dlqId,
            Status = "Discarded",
            Success = true,
            Message = "Entry discarded successfully."
        };

        // Assert
        Assert.Equal(dlqId, response.DeadLetterJobId);
        Assert.Equal("Discarded", response.Status);
        Assert.True(response.Success);
    }

    #endregion

    #region DlqMetricsResponse Tests

    [Fact]
    public void DlqMetricsResponse_EmptyDlq_HasZeroCounts()
    {
        // Arrange & Act
        var response = new DlqMetricsResponse
        {
            TotalCount = 0,
            PendingCount = 0,
            ReplayedCount = 0,
            DiscardedCount = 0,
            OldestPendingAgeSeconds = null,
            OldestPendingAt = null,
            HealthStatus = "Healthy"
        };

        // Assert
        Assert.Equal(0, response.TotalCount);
        Assert.Equal(0, response.PendingCount);
        Assert.Null(response.OldestPendingAgeSeconds);
        Assert.Equal("Healthy", response.HealthStatus);
    }

    [Fact]
    public void DlqMetricsResponse_WithPendingEntries_HasCorrectCounts()
    {
        // Arrange
        var oldestAt = DateTime.UtcNow.AddHours(-2);

        // Act
        var response = new DlqMetricsResponse
        {
            TotalCount = 10,
            PendingCount = 5,
            ReplayedCount = 3,
            DiscardedCount = 2,
            OldestPendingAgeSeconds = 7200,
            OldestPendingAt = oldestAt,
            HealthStatus = "Degraded"
        };

        // Assert
        Assert.Equal(10, response.TotalCount);
        Assert.Equal(5, response.PendingCount);
        Assert.Equal(3, response.ReplayedCount);
        Assert.Equal(2, response.DiscardedCount);
        Assert.Equal(7200, response.OldestPendingAgeSeconds);
        Assert.Equal(oldestAt, response.OldestPendingAt);
        Assert.Equal("Degraded", response.HealthStatus);
    }

    #endregion

    #region PaginationMetadata Tests

    [Fact]
    public void PaginationMetadata_FirstPage_HasCorrectFlags()
    {
        // Arrange & Act
        var pagination = new PaginationMetadata
        {
            Page = 1,
            PageSize = 20,
            TotalItems = 50,
            TotalPages = 3,
            HasNextPage = true,
            HasPreviousPage = false
        };

        // Assert
        Assert.Equal(1, pagination.Page);
        Assert.True(pagination.HasNextPage);
        Assert.False(pagination.HasPreviousPage);
    }

    [Fact]
    public void PaginationMetadata_LastPage_HasCorrectFlags()
    {
        // Arrange & Act
        var pagination = new PaginationMetadata
        {
            Page = 3,
            PageSize = 20,
            TotalItems = 50,
            TotalPages = 3,
            HasNextPage = false,
            HasPreviousPage = true
        };

        // Assert
        Assert.Equal(3, pagination.Page);
        Assert.False(pagination.HasNextPage);
        Assert.True(pagination.HasPreviousPage);
    }

    #endregion
}
