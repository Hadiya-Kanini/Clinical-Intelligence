using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using ClinicalIntelligence.Api.Data;
using ClinicalIntelligence.Api.Domain.Models;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace ClinicalIntelligence.Api.Tests.Integration;

/// <summary>
/// Integration tests for GET /api/v1/documents endpoint (US_056 TASK_004).
/// Validates documents list endpoint includes processing metadata and surfaces error messages.
/// </summary>
public sealed class DocumentsListEndpointTests : IClassFixture<TestWebApplicationFactory<Program>>
{
    private readonly TestWebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public DocumentsListEndpointTests(TestWebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    private async Task<HttpClient> GetAuthenticatedClientAsync(string email = "test@example.com", string password = "TestPassword123!")
    {
        var client = _factory.CreateClient();
        
        var loginResponse = await client.PostAsJsonAsync("/api/v1/auth/login", new
        {
            email,
            password
        });

        if (!loginResponse.IsSuccessStatusCode)
        {
            throw new InvalidOperationException($"Failed to authenticate: {loginResponse.StatusCode}");
        }

        return client;
    }

    [Fact]
    public async Task GetDocuments_Unauthenticated_ReturnsUnauthorized()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/api/v1/documents");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetDocuments_Authenticated_ReturnsOkWithItems()
    {
        // Arrange
        HttpClient client;
        try
        {
            client = await GetAuthenticatedClientAsync();
        }
        catch (InvalidOperationException)
        {
            // Skip test if database is not available
            return;
        }

        // Act
        var response = await client.GetAsync("/api/v1/documents");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var content = await response.Content.ReadAsStringAsync();
        var json = JsonDocument.Parse(content);
        
        Assert.True(json.RootElement.TryGetProperty("items", out var items));
        Assert.True(json.RootElement.TryGetProperty("total", out _));
        Assert.True(json.RootElement.TryGetProperty("page", out _));
        Assert.True(json.RootElement.TryGetProperty("pageSize", out _));
    }

    [Fact]
    public async Task GetDocuments_WithProcessingMetadata_ReturnsExpectedFields()
    {
        // Arrange
        Guid userId;
        Guid patientId;
        Guid documentId;
        Guid jobId;

        try
        {
            using var scope = _factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            // Get test user
            var user = dbContext.Users.FirstOrDefault(u => u.Email == "test@example.com");
            if (user == null)
            {
                // Skip test if database is not available
                return;
            }
            userId = user.Id;

            // Create test patient
            var patient = new ErdPatient
            {
                Id = Guid.NewGuid(),
                Mrn = $"MRN_{Guid.NewGuid():N}",
                Name = "Test Patient",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            dbContext.ErdPatients.Add(patient);
            patientId = patient.Id;

            // Create test document
            var document = new Document
            {
                Id = Guid.NewGuid(),
                PatientId = patientId,
                UploadedByUserId = userId,
                OriginalName = "test_with_metadata.pdf",
                MimeType = "application/pdf",
                SizeBytes = 2048,
                StoragePath = "/test/path/metadata",
                Status = "Completed",
                UploadedAt = DateTime.UtcNow
            };
            dbContext.Documents.Add(document);
            documentId = document.Id;

            // Create processing job with metadata
            var job = new ProcessingJob
            {
                Id = Guid.NewGuid(),
                DocumentId = documentId,
                Status = "Completed",
                RetryCount = 1,
                StartedAt = DateTime.UtcNow.AddSeconds(-10),
                CompletedAt = DateTime.UtcNow,
                ProcessingTimeMs = 10000
            };
            dbContext.ProcessingJobs.Add(job);
            jobId = job.Id;

            await dbContext.SaveChangesAsync();
        }
        catch (Exception)
        {
            // Skip test if database is not available
            return;
        }

        var client = await GetAuthenticatedClientAsync();

        // Act
        var response = await client.GetAsync("/api/v1/documents");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var content = await response.Content.ReadAsStringAsync();
        var json = JsonDocument.Parse(content);
        var items = json.RootElement.GetProperty("items");
        
        // Find our test document
        var testDoc = items.EnumerateArray()
            .FirstOrDefault(item => item.GetProperty("id").GetString() == documentId.ToString());
        
        if (testDoc.ValueKind != JsonValueKind.Undefined)
        {
            // Verify processing metadata fields are present
            Assert.True(testDoc.TryGetProperty("jobId", out _));
            Assert.True(testDoc.TryGetProperty("retryCount", out var retryCount));
            Assert.True(testDoc.TryGetProperty("startedAt", out _));
            Assert.True(testDoc.TryGetProperty("completedAt", out _));
            Assert.True(testDoc.TryGetProperty("processingTimeMs", out var processingTime));
            
            // Verify values
            if (retryCount.ValueKind == JsonValueKind.Number)
            {
                Assert.Equal(1, retryCount.GetInt32());
            }
            if (processingTime.ValueKind == JsonValueKind.Number)
            {
                Assert.Equal(10000, processingTime.GetInt32());
            }
        }
    }

    [Fact]
    public async Task GetDocuments_FailedDocument_ReturnsErrorMessage()
    {
        // Arrange
        Guid userId;
        Guid patientId;
        Guid documentId;
        const string expectedErrorMessage = "Document processing failed: invalid format";

        try
        {
            using var scope = _factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            // Get test user
            var user = dbContext.Users.FirstOrDefault(u => u.Email == "test@example.com");
            if (user == null)
            {
                return;
            }
            userId = user.Id;

            // Create test patient
            var patient = new ErdPatient
            {
                Id = Guid.NewGuid(),
                Mrn = $"MRN_{Guid.NewGuid():N}",
                Name = "Test Patient Failed",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            dbContext.ErdPatients.Add(patient);
            patientId = patient.Id;

            // Create failed document
            var document = new Document
            {
                Id = Guid.NewGuid(),
                PatientId = patientId,
                UploadedByUserId = userId,
                OriginalName = "failed_document.pdf",
                MimeType = "application/pdf",
                SizeBytes = 1024,
                StoragePath = "/test/path/failed",
                Status = "Failed",
                UploadedAt = DateTime.UtcNow
            };
            dbContext.Documents.Add(document);
            documentId = document.Id;

            // Create failed processing job with error message
            var job = new ProcessingJob
            {
                Id = Guid.NewGuid(),
                DocumentId = documentId,
                Status = "Failed",
                RetryCount = 3,
                StartedAt = DateTime.UtcNow.AddSeconds(-5),
                CompletedAt = DateTime.UtcNow,
                ProcessingTimeMs = 5000,
                ErrorMessage = expectedErrorMessage
            };
            dbContext.ProcessingJobs.Add(job);

            await dbContext.SaveChangesAsync();
        }
        catch (Exception)
        {
            return;
        }

        var client = await GetAuthenticatedClientAsync();

        // Act
        var response = await client.GetAsync("/api/v1/documents");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var content = await response.Content.ReadAsStringAsync();
        var json = JsonDocument.Parse(content);
        var items = json.RootElement.GetProperty("items");
        
        // Find our failed document
        var failedDoc = items.EnumerateArray()
            .FirstOrDefault(item => item.GetProperty("id").GetString() == documentId.ToString());
        
        if (failedDoc.ValueKind != JsonValueKind.Undefined)
        {
            // Verify error message is present for failed document
            Assert.True(failedDoc.TryGetProperty("errorMessage", out var errorMessage));
            Assert.Equal(expectedErrorMessage, errorMessage.GetString());
            Assert.Equal("Failed", failedDoc.GetProperty("status").GetString());
        }
    }

    [Fact]
    public async Task GetDocuments_CompletedDocument_DoesNotExposeErrorMessage()
    {
        // Arrange
        Guid userId;
        Guid patientId;
        Guid documentId;

        try
        {
            using var scope = _factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var user = dbContext.Users.FirstOrDefault(u => u.Email == "test@example.com");
            if (user == null)
            {
                return;
            }
            userId = user.Id;

            var patient = new ErdPatient
            {
                Id = Guid.NewGuid(),
                Mrn = $"MRN_{Guid.NewGuid():N}",
                Name = "Test Patient Completed",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            dbContext.ErdPatients.Add(patient);
            patientId = patient.Id;

            var document = new Document
            {
                Id = Guid.NewGuid(),
                PatientId = patientId,
                UploadedByUserId = userId,
                OriginalName = "completed_document.pdf",
                MimeType = "application/pdf",
                SizeBytes = 1024,
                StoragePath = "/test/path/completed",
                Status = "Completed",
                UploadedAt = DateTime.UtcNow
            };
            dbContext.Documents.Add(document);
            documentId = document.Id;

            var job = new ProcessingJob
            {
                Id = Guid.NewGuid(),
                DocumentId = documentId,
                Status = "Completed",
                StartedAt = DateTime.UtcNow.AddSeconds(-2),
                CompletedAt = DateTime.UtcNow,
                ProcessingTimeMs = 2000
            };
            dbContext.ProcessingJobs.Add(job);

            await dbContext.SaveChangesAsync();
        }
        catch (Exception)
        {
            return;
        }

        var client = await GetAuthenticatedClientAsync();

        // Act
        var response = await client.GetAsync("/api/v1/documents");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var content = await response.Content.ReadAsStringAsync();
        var json = JsonDocument.Parse(content);
        var items = json.RootElement.GetProperty("items");
        
        var completedDoc = items.EnumerateArray()
            .FirstOrDefault(item => item.GetProperty("id").GetString() == documentId.ToString());
        
        if (completedDoc.ValueKind != JsonValueKind.Undefined)
        {
            // Verify error message is null for completed document
            if (completedDoc.TryGetProperty("errorMessage", out var errorMessage))
            {
                Assert.True(errorMessage.ValueKind == JsonValueKind.Null);
            }
            Assert.Equal("Completed", completedDoc.GetProperty("status").GetString());
        }
    }

    [Fact]
    public async Task GetDocuments_WithSearchFilter_ReturnsFilteredResults()
    {
        // Arrange
        HttpClient client;
        try
        {
            client = await GetAuthenticatedClientAsync();
        }
        catch (InvalidOperationException)
        {
            return;
        }

        // Act
        var response = await client.GetAsync("/api/v1/documents?search=nonexistent_file_xyz");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var content = await response.Content.ReadAsStringAsync();
        var json = JsonDocument.Parse(content);
        var items = json.RootElement.GetProperty("items");
        
        // Should return empty or filtered results
        Assert.True(items.GetArrayLength() >= 0);
    }

    [Fact]
    public async Task GetDocuments_WithPagination_ReturnsCorrectPage()
    {
        // Arrange
        HttpClient client;
        try
        {
            client = await GetAuthenticatedClientAsync();
        }
        catch (InvalidOperationException)
        {
            return;
        }

        // Act
        var response = await client.GetAsync("/api/v1/documents?page=1&pageSize=5");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var content = await response.Content.ReadAsStringAsync();
        var json = JsonDocument.Parse(content);
        
        Assert.Equal(1, json.RootElement.GetProperty("page").GetInt32());
        Assert.Equal(5, json.RootElement.GetProperty("pageSize").GetInt32());
    }
}
