using System.Net;
using System.Net.Http.Headers;
using ClinicalIntelligence.Api.Data;
using ClinicalIntelligence.Api.Domain.Models;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace ClinicalIntelligence.Api.Tests;

/// <summary>
/// Integration tests for GET /api/v1/documents/{documentId}/content endpoint.
/// Covers authentication, authorization, 404 behavior, and content headers (US_070 TASK_005).
/// </summary>
public sealed class DocumentContentEndpointTests : IClassFixture<WebApplicationFactory<Program>>, IAsyncLifetime
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    private Guid _testDocumentId;
    private Guid _deletedDocumentId;
    private Guid _testUserId;
    private string _testFilePath = string.Empty;

    public DocumentContentEndpointTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));
                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }

                services.AddDbContext<ApplicationDbContext>(options =>
                {
                    options.UseInMemoryDatabase("DocumentContentTests");
                });
            });
        });

        _client = _factory.CreateClient();
    }

    public async Task InitializeAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await dbContext.Database.EnsureCreatedAsync();

        // Create test user
        _testUserId = Guid.NewGuid();
        var testUser = new User
        {
            Id = _testUserId,
            Email = "test@example.com",
            Name = "Test User",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("TestPassword123!"),
            Role = "Standard",
            Status = "Active",
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        dbContext.Users.Add(testUser);

        // Create test file on disk
        _testFilePath = Path.Combine(Path.GetTempPath(), $"test_document_{Guid.NewGuid()}.pdf");
        await File.WriteAllTextAsync(_testFilePath, "Test PDF content");

        // Create test document (active)
        _testDocumentId = Guid.NewGuid();
        var testDocument = new Document
        {
            Id = _testDocumentId,
            UploadedByUserId = _testUserId,
            OriginalName = "test-document.pdf",
            MimeType = "application/pdf",
            SizeBytes = 1024,
            StoragePath = _testFilePath,
            Status = "Completed",
            IsDeleted = false,
            UploadedAt = DateTime.UtcNow
        };
        dbContext.Documents.Add(testDocument);

        // Create deleted document
        _deletedDocumentId = Guid.NewGuid();
        var deletedDocument = new Document
        {
            Id = _deletedDocumentId,
            UploadedByUserId = _testUserId,
            OriginalName = "deleted-document.pdf",
            MimeType = "application/pdf",
            SizeBytes = 512,
            StoragePath = _testFilePath,
            Status = "Completed",
            IsDeleted = true,
            DeletedAt = DateTime.UtcNow,
            UploadedAt = DateTime.UtcNow.AddDays(-1)
        };
        dbContext.Documents.Add(deletedDocument);

        await dbContext.SaveChangesAsync();
    }

    public async Task DisposeAsync()
    {
        // Cleanup test file
        if (File.Exists(_testFilePath))
        {
            File.Delete(_testFilePath);
        }

        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await dbContext.Database.EnsureDeletedAsync();
    }

    [Fact]
    public async Task GetDocumentContent_Unauthenticated_Returns401()
    {
        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Get, $"/api/v1/documents/{_testDocumentId}/content");

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetDocumentContent_NonExistentDocument_Returns404()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();
        var request = new HttpRequestMessage(HttpMethod.Get, $"/api/v1/documents/{nonExistentId}/content");
        await AuthenticateAsync();

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("document_not_found", content);
    }

    [Fact]
    public async Task GetDocumentContent_DeletedDocument_Returns404()
    {
        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Get, $"/api/v1/documents/{_deletedDocumentId}/content");
        await AuthenticateAsync();

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("document_not_found", content);
    }

    [Fact]
    public async Task GetDocumentContent_ValidDocument_Returns200WithCorrectHeaders()
    {
        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Get, $"/api/v1/documents/{_testDocumentId}/content");
        await AuthenticateAsync();

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("application/pdf", response.Content.Headers.ContentType?.MediaType);
        
        // Verify content-disposition header contains filename
        var contentDisposition = response.Content.Headers.ContentDisposition;
        Assert.NotNull(contentDisposition);
        Assert.Equal("test-document.pdf", contentDisposition.FileName?.Trim('"'));
    }

    [Fact]
    public async Task GetDocumentContent_InvalidGuidFormat_Returns404()
    {
        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/v1/documents/not-a-guid/content");
        await AuthenticateAsync();

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        // Invalid GUID format should result in 404 (route not matched)
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    private async Task AuthenticateAsync()
    {
        // Login to get JWT token
        var loginResponse = await _client.PostAsJsonAsync("/api/v1/auth/login", new
        {
            email = "test@example.com",
            password = "TestPassword123!"
        });

        if (loginResponse.IsSuccessStatusCode)
        {
            var loginResult = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>();
            if (loginResult?.Token != null)
            {
                _client.DefaultRequestHeaders.Authorization = 
                    new AuthenticationHeaderValue("Bearer", loginResult.Token);
            }
        }
    }

    private sealed record LoginResponse
    {
        public string? Token { get; init; }
    }
}
