using ClinicalIntelligence.Api.Data;
using ClinicalIntelligence.Api.Domain.Models;
using ClinicalIntelligence.Api.Services;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace ClinicalIntelligence.Api.Tests.Endpoints;

public class DocumentStatusEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public DocumentStatusEndpointTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetDocumentStatus_Unauthorized_Returns401()
    {
        // Arrange
        var client = _factory.CreateClient();
        var documentId = Guid.NewGuid();
        
        // Act
        var response = await client.GetAsync($"/api/v1/documents/{documentId}/status");
        
        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetDocumentStatus_NonExistentDocument_ReturnsNotFound()
    {
        // Arrange
        var client = await CreateAuthenticatedClientAsync();
        var documentId = Guid.NewGuid();
        
        // Act
        var response = await client.GetAsync($"/api/v1/documents/{documentId}/status");
        
        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetDocumentStatus_ValidDocument_ReturnsOk()
    {
        // Arrange
        var client = await CreateAuthenticatedClientAsync();
        var documentId = await CreateTestDocumentAsync();
        
        // Act
        var response = await client.GetAsync($"/api/v1/documents/{documentId}/status");
        
        // Assert
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<DocumentStatusResult>();
        Assert.NotNull(result);
        Assert.Equal(documentId, result.DocumentId);
        Assert.Equal("Pending", result.Status);
    }

    private async Task<HttpClient> CreateAuthenticatedClientAsync()
    {
        var client = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // Use in-memory database for testing
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));
                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }
                services.AddDbContext<ApplicationDbContext>(options =>
                {
                    options.UseInMemoryDatabase($"TestDb_{Guid.NewGuid()}");
                });
            });
        }).CreateClient();

        // Login to get authentication cookie
        var loginResponse = await client.PostAsJsonAsync("/api/v1/auth/login", new
        {
            email = "admin@clinical-intelligence.local",
            password = "Admin123!"
        });

        return client;
    }

    private async Task<Guid> CreateTestDocumentAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        
        var patientId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var documentId = Guid.NewGuid();

        dbContext.ErdPatients.Add(new ErdPatient
        {
            Id = patientId,
            Mrn = $"MRN-{patientId:N}".Substring(0, 20),
            Name = "Test Patient",
            Dob = new DateOnly(1990, 1, 1),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });

        dbContext.Users.Add(new User
        {
            Id = userId,
            Email = $"user-{userId}@test.com",
            Name = "Test User",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Test123!"),
            Role = "Standard",
            Status = "Active",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });

        dbContext.Documents.Add(new Document
        {
            Id = documentId,
            PatientId = patientId,
            UploadedByUserId = userId,
            OriginalName = "test-document.pdf",
            MimeType = "application/pdf",
            SizeBytes = 1024,
            StoragePath = $"test/{documentId}/original.pdf",
            Status = "Pending",
            UploadedAt = DateTime.UtcNow
        });

        await dbContext.SaveChangesAsync();
        return documentId;
    }
}
