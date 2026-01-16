using ClinicalIntelligence.Api.Contracts;
using ClinicalIntelligence.Api.Data;
using ClinicalIntelligence.Api.Services;
using ClinicalIntelligence.Api.Tests.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace ClinicalIntelligence.Api.Tests.Services;

/// <summary>
/// Unit tests for BatchUploadService.
/// Tests batch limit enforcement (FR-014) and per-file validation.
/// </summary>
public class BatchUploadServiceTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly Mock<IDocumentService> _mockDocumentService;
    private readonly Mock<ILogger<BatchUploadService>> _mockLogger;
    private readonly BatchUploadService _service;
    private readonly Guid _testPatientId = Guid.NewGuid();
    private readonly Guid _testUserId = Guid.NewGuid();

    public BatchUploadServiceTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: $"BatchUploadTest_{Guid.NewGuid()}")
            .Options;

        _dbContext = new ApplicationDbContext(options);
        _mockDocumentService = new Mock<IDocumentService>();
        _mockLogger = new Mock<ILogger<BatchUploadService>>();

        // Setup default successful response
        _mockDocumentService
            .Setup(x => x.ValidateAndAcknowledgeAsync(
                It.IsAny<Microsoft.AspNetCore.Http.IFormFile>(),
                It.IsAny<Guid>(),
                It.IsAny<Guid>(),
                It.IsAny<Guid?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Microsoft.AspNetCore.Http.IFormFile file, Guid patientId, Guid userId, Guid? batchId, CancellationToken ct) =>
                new UploadAcknowledgmentResponse
                {
                    DocumentId = Guid.NewGuid(),
                    FileName = file.FileName,
                    FileSize = file.Length,
                    Status = "Accepted",
                    IsValid = true,
                    ValidationErrors = new List<string>(),
                    AcknowledgedAt = DateTime.UtcNow
                });

        _service = new BatchUploadService(_dbContext, _mockDocumentService.Object, _mockLogger.Object);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
    }

    [Fact]
    public async Task ProcessBatch_ExactlyTenFiles_AcceptsAllFiles()
    {
        // Arrange
        var files = BatchUploadTestHelpers.CreateMockFiles(10);

        // Act
        var result = await _service.ProcessBatchAsync(files, _testPatientId, _testUserId, CancellationToken.None);

        // Assert
        Assert.Equal(10, result.TotalFilesReceived);
        Assert.Equal(10, result.FilesAccepted);
        Assert.Equal(0, result.FilesRejected);
        Assert.False(result.BatchLimitExceeded);
        Assert.Null(result.BatchLimitWarning);
    }

    [Fact]
    public async Task ProcessBatch_LessThanTenFiles_AcceptsAllFiles()
    {
        // Arrange
        var files = BatchUploadTestHelpers.CreateMockFiles(5);

        // Act
        var result = await _service.ProcessBatchAsync(files, _testPatientId, _testUserId, CancellationToken.None);

        // Assert
        Assert.Equal(5, result.TotalFilesReceived);
        Assert.Equal(5, result.FilesAccepted);
        Assert.Equal(0, result.FilesRejected);
        Assert.False(result.BatchLimitExceeded);
    }

    [Fact]
    public async Task ProcessBatch_MoreThanTenFiles_AcceptsFirstTenOnly()
    {
        // Arrange
        var files = BatchUploadTestHelpers.CreateMockFiles(15);

        // Act
        var result = await _service.ProcessBatchAsync(files, _testPatientId, _testUserId, CancellationToken.None);

        // Assert
        Assert.Equal(15, result.TotalFilesReceived);
        Assert.Equal(10, result.FilesAccepted);
        Assert.Equal(5, result.FilesRejected);
        Assert.True(result.BatchLimitExceeded);
    }

    [Fact]
    public async Task ProcessBatch_FifteenFiles_RejectsFiveFiles()
    {
        // Arrange
        var files = BatchUploadTestHelpers.CreateMockFiles(15);

        // Act
        var result = await _service.ProcessBatchAsync(files, _testPatientId, _testUserId, CancellationToken.None);

        // Assert
        var rejectedFiles = result.FileResults.Where(r => !r.IsAccepted).ToList();
        Assert.Equal(5, rejectedFiles.Count);
        Assert.All(rejectedFiles, r => Assert.Equal("BatchLimitExceeded", r.Status));
    }

    [Fact]
    public async Task ProcessBatch_ExcessFiles_MarkedAsBatchLimitExceeded()
    {
        // Arrange
        var files = BatchUploadTestHelpers.CreateMockFiles(12);

        // Act
        var result = await _service.ProcessBatchAsync(files, _testPatientId, _testUserId, CancellationToken.None);

        // Assert
        var excessFiles = result.FileResults.Skip(10).ToList();
        Assert.Equal(2, excessFiles.Count);
        Assert.All(excessFiles, r =>
        {
            Assert.Equal("BatchLimitExceeded", r.Status);
            Assert.False(r.IsAccepted);
        });
    }

    [Fact]
    public async Task ProcessBatch_ExcessFiles_HaveCorrectRejectionReason()
    {
        // Arrange
        var files = BatchUploadTestHelpers.CreateMockFiles(11);

        // Act
        var result = await _service.ProcessBatchAsync(files, _testPatientId, _testUserId, CancellationToken.None);

        // Assert
        var excessFile = result.FileResults.Last();
        Assert.Contains("batch limit", excessFile.RejectionReason, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ProcessBatch_CreatesDocumentBatchEntity()
    {
        // Arrange
        var files = BatchUploadTestHelpers.CreateMockFiles(3);

        // Act
        var result = await _service.ProcessBatchAsync(files, _testPatientId, _testUserId, CancellationToken.None);

        // Assert
        var batch = await _dbContext.DocumentBatches.FindAsync(result.BatchId);
        Assert.NotNull(batch);
        Assert.Equal(_testPatientId, batch.PatientId);
        Assert.Equal(_testUserId, batch.UploadedByUserId);
    }

    [Fact]
    public async Task ProcessBatch_ReturnsBatchLimitExceededTrue_WhenOverLimit()
    {
        // Arrange
        var files = BatchUploadTestHelpers.CreateMockFiles(11);

        // Act
        var result = await _service.ProcessBatchAsync(files, _testPatientId, _testUserId, CancellationToken.None);

        // Assert
        Assert.True(result.BatchLimitExceeded);
    }

    [Fact]
    public async Task ProcessBatch_ReturnsBatchLimitExceededFalse_WhenWithinLimit()
    {
        // Arrange
        var files = BatchUploadTestHelpers.CreateMockFiles(10);

        // Act
        var result = await _service.ProcessBatchAsync(files, _testPatientId, _testUserId, CancellationToken.None);

        // Assert
        Assert.False(result.BatchLimitExceeded);
    }

    [Fact]
    public async Task ProcessBatch_GeneratesWarningMessage_WhenOverLimit()
    {
        // Arrange
        var files = BatchUploadTestHelpers.CreateMockFiles(15);

        // Act
        var result = await _service.ProcessBatchAsync(files, _testPatientId, _testUserId, CancellationToken.None);

        // Assert
        Assert.NotNull(result.BatchLimitWarning);
        Assert.Contains("10", result.BatchLimitWarning);
        Assert.Contains("5", result.BatchLimitWarning);
    }

    [Fact]
    public async Task ProcessBatch_NoWarningMessage_WhenWithinLimit()
    {
        // Arrange
        var files = BatchUploadTestHelpers.CreateMockFiles(8);

        // Act
        var result = await _service.ProcessBatchAsync(files, _testPatientId, _testUserId, CancellationToken.None);

        // Assert
        Assert.Null(result.BatchLimitWarning);
    }

    [Fact]
    public async Task ProcessBatch_ExactlyElevenFiles_RejectsOneFile()
    {
        // Arrange
        var files = BatchUploadTestHelpers.CreateMockFiles(11);

        // Act
        var result = await _service.ProcessBatchAsync(files, _testPatientId, _testUserId, CancellationToken.None);

        // Assert
        Assert.Equal(11, result.TotalFilesReceived);
        Assert.Equal(10, result.FilesAccepted);
        Assert.Equal(1, result.FilesRejected);
    }

    [Fact]
    public async Task ProcessBatch_TwentyFiles_RejectsTenFiles()
    {
        // Arrange
        var files = BatchUploadTestHelpers.CreateMockFiles(20);

        // Act
        var result = await _service.ProcessBatchAsync(files, _testPatientId, _testUserId, CancellationToken.None);

        // Assert
        Assert.Equal(20, result.TotalFilesReceived);
        Assert.Equal(10, result.FilesAccepted);
        Assert.Equal(10, result.FilesRejected);
    }

    [Fact]
    public async Task ProcessBatch_SingleFile_ReturnsOk()
    {
        // Arrange
        var files = BatchUploadTestHelpers.CreateMockFiles(1);

        // Act
        var result = await _service.ProcessBatchAsync(files, _testPatientId, _testUserId, CancellationToken.None);

        // Assert
        Assert.Equal(1, result.TotalFilesReceived);
        Assert.Equal(1, result.FilesAccepted);
        Assert.False(result.BatchLimitExceeded);
    }

    [Fact]
    public async Task ProcessBatch_AcceptedFilesHaveDocumentId()
    {
        // Arrange
        var files = BatchUploadTestHelpers.CreateMockFiles(3);

        // Act
        var result = await _service.ProcessBatchAsync(files, _testPatientId, _testUserId, CancellationToken.None);

        // Assert
        var acceptedFiles = result.FileResults.Where(r => r.IsAccepted).ToList();
        Assert.All(acceptedFiles, r => Assert.NotNull(r.DocumentId));
    }

    [Fact]
    public async Task ProcessBatch_RejectedFilesHaveNoDocumentId()
    {
        // Arrange
        var files = BatchUploadTestHelpers.CreateMockFiles(12);

        // Act
        var result = await _service.ProcessBatchAsync(files, _testPatientId, _testUserId, CancellationToken.None);

        // Assert
        var rejectedFiles = result.FileResults.Where(r => !r.IsAccepted).ToList();
        Assert.All(rejectedFiles, r => Assert.Null(r.DocumentId));
    }

    [Fact]
    public async Task ProcessBatch_FileResultsMatchTotalReceived()
    {
        // Arrange
        var files = BatchUploadTestHelpers.CreateMockFiles(15);

        // Act
        var result = await _service.ProcessBatchAsync(files, _testPatientId, _testUserId, CancellationToken.None);

        // Assert
        Assert.Equal(result.TotalFilesReceived, result.FileResults.Count);
    }

    [Fact]
    public async Task ProcessBatch_ValidationFailure_CountsAsRejected()
    {
        // Arrange
        var files = BatchUploadTestHelpers.CreateMockFiles(3);

        // Setup one file to fail validation
        _mockDocumentService
            .Setup(x => x.ValidateAndAcknowledgeAsync(
                It.Is<Microsoft.AspNetCore.Http.IFormFile>(f => f.FileName == "file2.pdf"),
                It.IsAny<Guid>(),
                It.IsAny<Guid>(),
                It.IsAny<Guid?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new UploadAcknowledgmentResponse
            {
                DocumentId = Guid.NewGuid(),
                FileName = "file2.pdf",
                FileSize = 1024,
                Status = "ValidationFailed",
                IsValid = false,
                ValidationErrors = new List<string> { "Invalid file format" },
                AcknowledgedAt = DateTime.UtcNow
            });

        // Act
        var result = await _service.ProcessBatchAsync(files, _testPatientId, _testUserId, CancellationToken.None);

        // Assert
        Assert.Equal(3, result.TotalFilesReceived);
        Assert.Equal(2, result.FilesAccepted);
        Assert.Equal(1, result.FilesRejected);
    }
}

/// <summary>
/// Tests for batch limit warning message generation.
/// </summary>
public class BatchLimitWarningTests
{
    [Theory]
    [InlineData(11, 10, 1)]
    [InlineData(15, 10, 5)]
    [InlineData(20, 10, 10)]
    public async Task GenerateWarning_CorrectCounts(int total, int expectedAccepted, int expectedRejected)
    {
        // Arrange
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: $"WarningTest_{Guid.NewGuid()}")
            .Options;

        using var dbContext = new ApplicationDbContext(options);
        var mockDocumentService = new Mock<IDocumentService>();
        var mockLogger = new Mock<ILogger<BatchUploadService>>();

        mockDocumentService
            .Setup(x => x.ValidateAndAcknowledgeAsync(
                It.IsAny<Microsoft.AspNetCore.Http.IFormFile>(),
                It.IsAny<Guid>(),
                It.IsAny<Guid>(),
                It.IsAny<Guid?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Microsoft.AspNetCore.Http.IFormFile file, Guid patientId, Guid userId, Guid? batchId, CancellationToken ct) =>
                new UploadAcknowledgmentResponse
                {
                    DocumentId = Guid.NewGuid(),
                    FileName = file.FileName,
                    FileSize = file.Length,
                    Status = "Accepted",
                    IsValid = true,
                    ValidationErrors = new List<string>(),
                    AcknowledgedAt = DateTime.UtcNow
                });

        var service = new BatchUploadService(dbContext, mockDocumentService.Object, mockLogger.Object);
        var files = BatchUploadTestHelpers.CreateMockFiles(total);

        // Act
        var result = await service.ProcessBatchAsync(files, Guid.NewGuid(), Guid.NewGuid(), CancellationToken.None);

        // Assert
        Assert.Equal(expectedAccepted, result.FilesAccepted);
        Assert.Equal(expectedRejected, result.FilesRejected);
        Assert.NotNull(result.BatchLimitWarning);
    }

    [Fact]
    public async Task GenerateWarning_ContainsAcceptedCount()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: $"WarningTest_{Guid.NewGuid()}")
            .Options;

        using var dbContext = new ApplicationDbContext(options);
        var mockDocumentService = new Mock<IDocumentService>();
        var mockLogger = new Mock<ILogger<BatchUploadService>>();

        mockDocumentService
            .Setup(x => x.ValidateAndAcknowledgeAsync(
                It.IsAny<Microsoft.AspNetCore.Http.IFormFile>(),
                It.IsAny<Guid>(),
                It.IsAny<Guid>(),
                It.IsAny<Guid?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Microsoft.AspNetCore.Http.IFormFile file, Guid patientId, Guid userId, Guid? batchId, CancellationToken ct) =>
                new UploadAcknowledgmentResponse
                {
                    DocumentId = Guid.NewGuid(),
                    FileName = file.FileName,
                    FileSize = file.Length,
                    Status = "Accepted",
                    IsValid = true,
                    ValidationErrors = new List<string>(),
                    AcknowledgedAt = DateTime.UtcNow
                });

        var service = new BatchUploadService(dbContext, mockDocumentService.Object, mockLogger.Object);
        var files = BatchUploadTestHelpers.CreateMockFiles(15);

        // Act
        var result = await service.ProcessBatchAsync(files, Guid.NewGuid(), Guid.NewGuid(), CancellationToken.None);

        // Assert
        Assert.Contains("10", result.BatchLimitWarning);
    }

    [Fact]
    public async Task GenerateWarning_SuggestsSeparateBatch()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: $"WarningTest_{Guid.NewGuid()}")
            .Options;

        using var dbContext = new ApplicationDbContext(options);
        var mockDocumentService = new Mock<IDocumentService>();
        var mockLogger = new Mock<ILogger<BatchUploadService>>();

        mockDocumentService
            .Setup(x => x.ValidateAndAcknowledgeAsync(
                It.IsAny<Microsoft.AspNetCore.Http.IFormFile>(),
                It.IsAny<Guid>(),
                It.IsAny<Guid>(),
                It.IsAny<Guid?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Microsoft.AspNetCore.Http.IFormFile file, Guid patientId, Guid userId, Guid? batchId, CancellationToken ct) =>
                new UploadAcknowledgmentResponse
                {
                    DocumentId = Guid.NewGuid(),
                    FileName = file.FileName,
                    FileSize = file.Length,
                    Status = "Accepted",
                    IsValid = true,
                    ValidationErrors = new List<string>(),
                    AcknowledgedAt = DateTime.UtcNow
                });

        var service = new BatchUploadService(dbContext, mockDocumentService.Object, mockLogger.Object);
        var files = BatchUploadTestHelpers.CreateMockFiles(12);

        // Act
        var result = await service.ProcessBatchAsync(files, Guid.NewGuid(), Guid.NewGuid(), CancellationToken.None);

        // Assert
        Assert.Contains("separate batch", result.BatchLimitWarning, StringComparison.OrdinalIgnoreCase);
    }
}
