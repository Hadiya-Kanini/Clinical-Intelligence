using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using ClinicalIntelligence.Api.Contracts.Dlq;
using ClinicalIntelligence.Api.Data;
using ClinicalIntelligence.Api.Domain.Models;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace ClinicalIntelligence.Api.Tests.Dlq;

/// <summary>
/// Integration tests for DLQ endpoints (US_055 TASK_005).
/// Requires PostgreSQL with pgvector extension to run.
/// Tests are skipped if database is not available.
/// </summary>
public class DlqEndpointsTests : IClassFixture<TestWebApplicationFactory<Program>>
{
    private readonly TestWebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public DlqEndpointsTests(TestWebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    #region Test Data Helpers

    private async Task<HttpClient> GetAuthenticatedAdminClientAsync()
    {
        var client = _factory.CreateClient();
        
        var loginResponse = await client.PostAsJsonAsync("/api/v1/auth/login", new
        {
            email = "admin@example.com",
            password = "AdminPassword123!"
        });

        loginResponse.EnsureSuccessStatusCode();
        return client;
    }

    private async Task<HttpClient> GetAuthenticatedStandardUserClientAsync()
    {
        var client = _factory.CreateClient();
        
        var loginResponse = await client.PostAsJsonAsync("/api/v1/auth/login", new
        {
            email = "test@example.com",
            password = "TestPassword123!"
        });

        loginResponse.EnsureSuccessStatusCode();
        return client;
    }

    private async Task<Guid> SeedDeadLetterJobAsync(
        string status = "Pending",
        string? errorMessage = "Test error message")
    {
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        // Create a patient first
        var patient = new ErdPatient
        {
            Id = Guid.NewGuid(),
            Mrn = $"MRN-{Guid.NewGuid():N}".Substring(0, 20),
            Name = "Test Patient",
            Dob = new DateOnly(1990, 1, 1),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        dbContext.ErdPatients.Add(patient);

        // Create a document
        var document = new Document
        {
            Id = Guid.NewGuid(),
            PatientId = patient.Id,
            OriginalName = "test-document.pdf",
            MimeType = "application/pdf",
            StoragePath = "/test/path/document.pdf",
            SizeBytes = 1024,
            Status = "Failed",
            UploadedAt = DateTime.UtcNow
        };
        dbContext.Documents.Add(document);

        // Create a processing job
        var processingJob = new ProcessingJob
        {
            Id = Guid.NewGuid(),
            DocumentId = document.Id,
            Status = "DeadLettered",
            RetryCount = 3,
            ErrorMessage = errorMessage
        };
        dbContext.ProcessingJobs.Add(processingJob);

        // Create the dead letter job
        var deadLetterJob = new DeadLetterJob
        {
            Id = Guid.NewGuid(),
            ProcessingJobId = processingJob.Id,
            DocumentId = document.Id,
            OriginalMessage = JsonSerializer.Serialize(new
            {
                jobId = processingJob.Id,
                documentId = document.Id,
                patientId = patient.Id,
                originalName = "test-document.pdf",
                mimeType = "application/pdf",
                storagePath = "[REDACTED]",
                sizeBytes = 1024,
                createdAt = DateTime.UtcNow,
                retryCount = 3
            }),
            MessageSchemaVersion = "1.0",
            ErrorMessage = errorMessage,
            ErrorDetails = JsonSerializer.Serialize(new { stackTrace = "Test stack trace" }),
            RetryHistory = JsonSerializer.Serialize(new[]
            {
                new { attempt = 1, error = "First attempt failed", timestamp = DateTime.UtcNow.AddMinutes(-10) },
                new { attempt = 2, error = "Second attempt failed", timestamp = DateTime.UtcNow.AddMinutes(-5) },
                new { attempt = 3, error = "Third attempt failed", timestamp = DateTime.UtcNow.AddMinutes(-1) }
            }),
            RetryCount = 3,
            DeadLetterReason = "Max retries exhausted",
            DeadLetteredAt = DateTime.UtcNow,
            Status = status
        };
        dbContext.DeadLetterJobs.Add(deadLetterJob);

        await dbContext.SaveChangesAsync();
        return deadLetterJob.Id;
    }

    private async Task CleanupDeadLetterJobsAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        
        dbContext.DeadLetterJobs.RemoveRange(dbContext.DeadLetterJobs);
        await dbContext.SaveChangesAsync();
    }

    #endregion

    #region List Endpoint Tests

    [Fact]
    public async Task ListDlq_WithAdminAuth_ReturnsOkWithPagination()
    {
        // Skip if PostgreSQL is not available
        await SkipIfPostgreSqlNotAvailable();

        // Arrange
        await CleanupDeadLetterJobsAsync();
        await SeedDeadLetterJobAsync();
        var client = await GetAuthenticatedAdminClientAsync();

        // Act
        var response = await client.GetAsync("/api/v1/admin/dlq");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var content = await response.Content.ReadFromJsonAsync<DlqListResponse>();
        Assert.NotNull(content);
        Assert.NotNull(content.Pagination);
        Assert.True(content.Pagination.TotalItems >= 1);
    }

    [Fact]
    public async Task ListDlq_WithPaginationParams_RespectsPageSize()
    {
        // Skip if PostgreSQL is not available
        await SkipIfPostgreSqlNotAvailable();

        // Arrange
        await CleanupDeadLetterJobsAsync();
        await SeedDeadLetterJobAsync();
        await SeedDeadLetterJobAsync();
        await SeedDeadLetterJobAsync();
        var client = await GetAuthenticatedAdminClientAsync();

        // Act
        var response = await client.GetAsync("/api/v1/admin/dlq?page=1&pageSize=2");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var content = await response.Content.ReadFromJsonAsync<DlqListResponse>();
        Assert.NotNull(content);
        Assert.Equal(2, content.Items.Count);
        Assert.Equal(2, content.Pagination.PageSize);
    }

    [Fact]
    public async Task ListDlq_WithoutAuth_ReturnsUnauthorized()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/api/v1/admin/dlq");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task ListDlq_WithStandardUserAuth_ReturnsForbidden()
    {
        // Arrange
        var client = await GetAuthenticatedStandardUserClientAsync();

        // Act
        var response = await client.GetAsync("/api/v1/admin/dlq");

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    #endregion

    #region Get By ID Endpoint Tests

    [Fact]
    public async Task GetDlqById_WithValidId_ReturnsFullDetails()
    {
        // Skip if PostgreSQL is not available
        await SkipIfPostgreSqlNotAvailable();

        // Arrange
        await CleanupDeadLetterJobsAsync();
        var dlqId = await SeedDeadLetterJobAsync();
        var client = await GetAuthenticatedAdminClientAsync();

        // Act
        var response = await client.GetAsync($"/api/v1/admin/dlq/{dlqId}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var content = await response.Content.ReadFromJsonAsync<DlqItemResponse>();
        Assert.NotNull(content);
        Assert.Equal(dlqId, content.Id);
        Assert.NotEmpty(content.OriginalMessage);
        Assert.NotNull(content.RetryHistory);
    }

    [Fact]
    public async Task GetDlqById_WithUnknownId_ReturnsNotFound()
    {
        // Skip if PostgreSQL is not available
        await SkipIfPostgreSqlNotAvailable();

        // Arrange
        var client = await GetAuthenticatedAdminClientAsync();
        var unknownId = Guid.NewGuid();

        // Act
        var response = await client.GetAsync($"/api/v1/admin/dlq/{unknownId}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    #endregion

    #region Replay Endpoint Tests

    [Fact]
    public async Task ReplayDlq_WithPendingEntry_ReturnsSuccess()
    {
        // Skip if PostgreSQL is not available
        await SkipIfPostgreSqlNotAvailable();

        // Arrange
        await CleanupDeadLetterJobsAsync();
        var dlqId = await SeedDeadLetterJobAsync(status: "Pending");
        var client = await GetAuthenticatedAdminClientAsync();

        // Act
        var response = await client.PostAsync($"/api/v1/admin/dlq/{dlqId}/replay", null);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var content = await response.Content.ReadFromJsonAsync<DlqReplayResponse>();
        Assert.NotNull(content);
        Assert.True(content.Success);
        Assert.Equal("Replayed", content.Status);
    }

    [Fact]
    public async Task ReplayDlq_WithoutAuth_ReturnsUnauthorized()
    {
        // Arrange
        var client = _factory.CreateClient();
        var dlqId = Guid.NewGuid();

        // Act
        var response = await client.PostAsync($"/api/v1/admin/dlq/{dlqId}/replay", null);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    #endregion

    #region Discard Endpoint Tests

    [Fact]
    public async Task DiscardDlq_WithPendingEntry_ReturnsSuccess()
    {
        // Skip if PostgreSQL is not available
        await SkipIfPostgreSqlNotAvailable();

        // Arrange
        await CleanupDeadLetterJobsAsync();
        var dlqId = await SeedDeadLetterJobAsync(status: "Pending");
        var client = await GetAuthenticatedAdminClientAsync();

        // Act
        var response = await client.DeleteAsync($"/api/v1/admin/dlq/{dlqId}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var content = await response.Content.ReadFromJsonAsync<DlqDiscardResponse>();
        Assert.NotNull(content);
        Assert.True(content.Success);
        Assert.Equal("Discarded", content.Status);
    }

    [Fact]
    public async Task DiscardDlq_WithoutAuth_ReturnsUnauthorized()
    {
        // Arrange
        var client = _factory.CreateClient();
        var dlqId = Guid.NewGuid();

        // Act
        var response = await client.DeleteAsync($"/api/v1/admin/dlq/{dlqId}");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    #endregion

    #region Metrics Endpoint Tests

    [Fact]
    public async Task GetDlqMetrics_WithAdminAuth_ReturnsMetrics()
    {
        // Skip if PostgreSQL is not available
        await SkipIfPostgreSqlNotAvailable();

        // Arrange
        await CleanupDeadLetterJobsAsync();
        await SeedDeadLetterJobAsync(status: "Pending");
        await SeedDeadLetterJobAsync(status: "Pending");
        await SeedDeadLetterJobAsync(status: "Discarded");
        var client = await GetAuthenticatedAdminClientAsync();

        // Act
        var response = await client.GetAsync("/api/v1/admin/dlq/metrics");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var content = await response.Content.ReadFromJsonAsync<DlqMetricsResponse>();
        Assert.NotNull(content);
        Assert.Equal(3, content.TotalCount);
        Assert.Equal(2, content.PendingCount);
        Assert.Equal(1, content.DiscardedCount);
    }

    [Fact]
    public async Task GetDlqMetrics_WithoutAuth_ReturnsUnauthorized()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/api/v1/admin/dlq/metrics");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    #endregion

    #region Health Endpoint Tests

    [Fact]
    public async Task GetDlqHealth_DoesNotRequireAuth()
    {
        // Skip if PostgreSQL is not available
        await SkipIfPostgreSqlNotAvailable();

        // Arrange
        var client = _factory.CreateClient(); // Unauthenticated client

        // Act
        var response = await client.GetAsync("/health/dlq");

        // Assert - Should not be 401 or 403
        Assert.NotEqual(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.NotEqual(HttpStatusCode.Forbidden, response.StatusCode);
    }

    #endregion

    #region Helper Methods

    private async Task SkipIfPostgreSqlNotAvailable()
    {
        try
        {
            using var scope = _factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            
            // Try to connect to database
            await dbContext.Database.CanConnectAsync();
        }
        catch (Exception ex)
        {
            // Skip test if PostgreSQL is not available
            Xunit.Skip.If(true, $"PostgreSQL not available for integration tests: {ex.Message}");
        }
    }

    #endregion
}
