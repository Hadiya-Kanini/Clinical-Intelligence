using ClinicalIntelligence.Api.Contracts;
using System.Text.Json;
using Xunit;

namespace ClinicalIntelligence.Api.Tests.Services.Queue;

public class DocumentProcessingJobTests
{
    [Fact]
    public void DocumentProcessingJob_Serialization_RoundTrip()
    {
        // Arrange
        var job = new DocumentProcessingJob
        {
            JobId = Guid.NewGuid(),
            DocumentId = Guid.NewGuid(),
            PatientId = Guid.NewGuid(),
            UploadedByUserId = Guid.NewGuid(),
            OriginalName = "test-document.pdf",
            MimeType = "application/pdf",
            StoragePath = "default/patient-id/doc-id/original.pdf",
            SizeBytes = 1024000,
            CreatedAt = DateTime.UtcNow,
            RetryCount = 0,
            CorrelationId = Guid.NewGuid().ToString()
        };
        
        // Act
        var json = JsonSerializer.Serialize(job, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
        var deserialized = JsonSerializer.Deserialize<DocumentProcessingJob>(json, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
        
        // Assert
        Assert.NotNull(deserialized);
        Assert.Equal(job.JobId, deserialized.JobId);
        Assert.Equal(job.DocumentId, deserialized.DocumentId);
        Assert.Equal(job.PatientId, deserialized.PatientId);
        Assert.Equal(job.OriginalName, deserialized.OriginalName);
        Assert.Equal(job.MimeType, deserialized.MimeType);
        Assert.Equal(job.StoragePath, deserialized.StoragePath);
        Assert.Equal(job.SizeBytes, deserialized.SizeBytes);
        Assert.Equal(job.RetryCount, deserialized.RetryCount);
    }
    
    [Fact]
    public void DocumentProcessingJob_RequiredFields_ArePresent()
    {
        // Arrange
        var job = new DocumentProcessingJob
        {
            JobId = Guid.NewGuid(),
            DocumentId = Guid.NewGuid(),
            PatientId = Guid.NewGuid(),
            UploadedByUserId = Guid.NewGuid(),
            OriginalName = "test.pdf",
            MimeType = "application/pdf",
            StoragePath = "path/to/file.pdf",
            SizeBytes = 1000,
            CreatedAt = DateTime.UtcNow
        };
        
        // Assert
        Assert.NotEqual(Guid.Empty, job.JobId);
        Assert.NotEqual(Guid.Empty, job.DocumentId);
        Assert.NotEqual(Guid.Empty, job.PatientId);
        Assert.NotEmpty(job.OriginalName);
        Assert.NotEmpty(job.MimeType);
        Assert.NotEmpty(job.StoragePath);
        Assert.True(job.SizeBytes > 0);
    }
    
    [Fact]
    public void DocumentProcessingJob_DefaultValues_AreCorrect()
    {
        // Arrange & Act
        var job = new DocumentProcessingJob();
        
        // Assert
        Assert.Equal(Guid.Empty, job.JobId);
        Assert.Equal(Guid.Empty, job.DocumentId);
        Assert.Equal(string.Empty, job.OriginalName);
        Assert.Equal(string.Empty, job.MimeType);
        Assert.Equal(string.Empty, job.StoragePath);
        Assert.Equal(0, job.SizeBytes);
        Assert.Equal(0, job.RetryCount);
        Assert.Null(job.CorrelationId);
    }
    
    [Fact]
    public void DocumentProcessingJob_WithRetry_CanBeCloned()
    {
        // Arrange
        var originalJob = new DocumentProcessingJob
        {
            JobId = Guid.NewGuid(),
            DocumentId = Guid.NewGuid(),
            PatientId = Guid.NewGuid(),
            UploadedByUserId = Guid.NewGuid(),
            OriginalName = "test.pdf",
            MimeType = "application/pdf",
            StoragePath = "path/to/file.pdf",
            SizeBytes = 1000,
            CreatedAt = DateTime.UtcNow,
            RetryCount = 0
        };
        
        // Act - simulate retry using with expression
        var retriedJob = originalJob with { RetryCount = originalJob.RetryCount + 1 };
        
        // Assert
        Assert.Equal(0, originalJob.RetryCount);
        Assert.Equal(1, retriedJob.RetryCount);
        Assert.Equal(originalJob.DocumentId, retriedJob.DocumentId);
        Assert.Equal(originalJob.JobId, retriedJob.JobId);
    }
}
