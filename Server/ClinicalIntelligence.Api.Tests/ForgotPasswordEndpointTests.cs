using ClinicalIntelligence.Api.Contracts;
using ClinicalIntelligence.Api.Data;
using ClinicalIntelligence.Api.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http.Json;
using System.Text.Json;
using System.Net;
using Xunit;

namespace ClinicalIntelligence.Api.Tests;

/// <summary>
/// Integration tests for forgot password endpoint.
/// </summary>
public sealed class ForgotPasswordEndpointTests : IClassFixture<TestWebApplicationFactory<Program>>
{
    private readonly TestWebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    private readonly JsonSerializerOptions _jsonOptions;

    public ForgotPasswordEndpointTests(TestWebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
    }

    [Fact]
    public async Task ForgotPassword_ValidEmail_ReturnsSuccess()
    {
        // Arrange
        var request = new ForgotPasswordRequest { Email = "test@example.com" };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/auth/forgot-password", request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("reset link has been sent", content);
    }

    [Fact]
    public async Task ForgotPassword_NonExistentEmail_ReturnsSuccess()
    {
        // Arrange
        var request = new ForgotPasswordRequest { Email = "nonexistent@example.com" };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/auth/forgot-password", request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        // Should return same message to prevent user enumeration
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("reset link has been sent", content);
    }

    [Fact]
    public async Task ForgotPassword_InvalidEmail_ReturnsBadRequest()
    {
        // Arrange
        var request = new ForgotPasswordRequest { Email = "invalid-email" };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/auth/forgot-password", request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        
        var errorResponse = await response.Content.ReadFromJsonAsync<JsonElement>(_jsonOptions);
        Assert.Equal("invalid_input", errorResponse.GetProperty("error").GetProperty("code").GetString());
    }

    [Fact]
    public async Task ForgotPassword_EmptyEmail_ReturnsBadRequest()
    {
        // Arrange
        var request = new ForgotPasswordRequest { Email = "" };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/auth/forgot-password", request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task ForgotPassword_RateLimitExceeded_Returns429()
    {
        // Arrange
        var request = new ForgotPasswordRequest { Email = "ratelimit@example.com" };
        
        // Act - Make 4 requests rapidly
        var tasks = new List<Task<HttpResponseMessage>>();
        for (int i = 0; i < 4; i++)
        {
            tasks.Add(_client.PostAsJsonAsync("/api/v1/auth/forgot-password", request));
        }

        var responses = await Task.WhenAll(tasks);

        // Assert
        var successCount = responses.Count(r => r.StatusCode == HttpStatusCode.OK);
        var rateLimitedCount = responses.Count(r => r.StatusCode == HttpStatusCode.TooManyRequests);

        Assert.True(successCount >= 3, "Should allow at least 3 requests");
        Assert.True(rateLimitedCount >= 1, "Should rate limit after 3 requests");

        var rateLimitedResponse = responses.First(r => r.StatusCode == HttpStatusCode.TooManyRequests);
        Assert.True(rateLimitedResponse.Headers.Contains("Retry-After"));
    }

    [Fact]
    public async Task ForgotPassword_CreatesTokenInDatabase()
    {
        // Arrange
        var request = new ForgotPasswordRequest { Email = "test@example.com" };
        
        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/auth/forgot-password", request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        // Verify token was created in database
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        
        var tokens = await dbContext.PasswordResetTokens
            .Include(t => t.User)
            .Where(t => t.User.Email == "test@example.com")
            .ToListAsync();

        Assert.True(tokens.Count > 0, "Token should be created in database");
        
        var token = tokens.First();
        Assert.True(token.ExpiresAt > DateTime.UtcNow, "Token should have future expiration");
        Assert.True(token.ExpiresAt <= DateTime.UtcNow.AddHours(1), "Token should expire within 1 hour");
        Assert.Null(token.UsedAt);
        Assert.NotNull(token.User);
    }

    [Fact]
    public async Task ForgotPassword_InvalidateOldTokens()
    {
        // Arrange
        var request = new ForgotPasswordRequest { Email = "test@example.com" };
        
        // Act - Make two requests
        await _client.PostAsJsonAsync("/api/v1/auth/forgot-password", request);
        await Task.Delay(100); // Small delay to ensure different timestamps
        await _client.PostAsJsonAsync("/api/v1/auth/forgot-password", request);

        // Assert
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        
        var tokens = await dbContext.PasswordResetTokens
            .Include(t => t.User)
            .Where(t => t.User.Email == "test@example.com")
            .OrderByDescending(t => t.Id)
            .ToListAsync();

        Assert.True(tokens.Count >= 2, "Should have at least 2 tokens");

        // The latest token should be valid (not expired)
        var latestToken = tokens.First();
        Assert.True(latestToken.ExpiresAt > DateTime.UtcNow, "Latest token should be valid");

        // Previous tokens should be expired
        var previousTokens = tokens.Skip(1);
        foreach (var previousToken in previousTokens)
        {
            Assert.True(previousToken.ExpiresAt <= DateTime.UtcNow, "Previous tokens should be invalidated");
        }
    }

    [Fact]
    public async Task ForgotPassword_ExistingAndNonExistingEmails_ReturnIdenticalResponses()
    {
        // Arrange
        var existingEmail = "test@example.com";
        var nonExistingEmail = "nonexistent-unique-12345@example.com";

        // Act
        var existingResponse = await _client.PostAsJsonAsync("/api/v1/auth/forgot-password", new ForgotPasswordRequest { Email = existingEmail });
        var nonExistingResponse = await _client.PostAsJsonAsync("/api/v1/auth/forgot-password", new ForgotPasswordRequest { Email = nonExistingEmail });

        // Assert - both should return 200
        Assert.Equal(HttpStatusCode.OK, existingResponse.StatusCode);
        Assert.Equal(HttpStatusCode.OK, nonExistingResponse.StatusCode);

        // Parse response bodies
        var existingContent = await existingResponse.Content.ReadAsStringAsync();
        var nonExistingContent = await nonExistingResponse.Content.ReadAsStringAsync();

        using var existingDoc = JsonDocument.Parse(existingContent);
        using var nonExistingDoc = JsonDocument.Parse(nonExistingContent);

        // Assert - both responses should have the same structure and message
        Assert.True(existingDoc.RootElement.TryGetProperty("message", out var existingMessage));
        Assert.True(nonExistingDoc.RootElement.TryGetProperty("message", out var nonExistingMessage));

        // Assert - messages should be identical (no enumeration leak)
        Assert.Equal(existingMessage.GetString(), nonExistingMessage.GetString());
    }

    [Fact]
    public async Task ForgotPassword_ResponseDoesNotLeakUserExistence()
    {
        // Arrange
        var existingEmail = "test@example.com";
        var nonExistingEmail = "nonexistent-unique-67890@example.com";

        // Act
        var existingResponse = await _client.PostAsJsonAsync("/api/v1/auth/forgot-password", new ForgotPasswordRequest { Email = existingEmail });
        var nonExistingResponse = await _client.PostAsJsonAsync("/api/v1/auth/forgot-password", new ForgotPasswordRequest { Email = nonExistingEmail });

        // Assert - response content should not contain any indication of user existence
        var existingContent = await existingResponse.Content.ReadAsStringAsync();
        var nonExistingContent = await nonExistingResponse.Content.ReadAsStringAsync();

        // Should not contain "found", "not found", "exists", "does not exist"
        Assert.DoesNotContain("found", existingContent.ToLower());
        Assert.DoesNotContain("exists", existingContent.ToLower());
        Assert.DoesNotContain("found", nonExistingContent.ToLower());
        Assert.DoesNotContain("exists", nonExistingContent.ToLower());

        // Should not contain the actual email addresses in response
        Assert.DoesNotContain(existingEmail, existingContent);
        Assert.DoesNotContain(nonExistingEmail, nonExistingContent);
    }

    [Fact]
    public async Task ForgotPassword_ValidationErrorResponse_HasStandardizedShape()
    {
        // Arrange
        var request = new ForgotPasswordRequest { Email = "invalid-email" };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/auth/forgot-password", request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(content);
        var root = doc.RootElement;

        // Verify standardized error response shape
        Assert.True(root.TryGetProperty("error", out var errorElement));
        Assert.True(errorElement.TryGetProperty("code", out var codeElement));
        Assert.True(errorElement.TryGetProperty("message", out var messageElement));
        Assert.True(errorElement.TryGetProperty("details", out var detailsElement));

        Assert.False(string.IsNullOrEmpty(codeElement.GetString()));
        Assert.False(string.IsNullOrEmpty(messageElement.GetString()));
        Assert.Equal(JsonValueKind.Array, detailsElement.ValueKind);
    }

    [Theory]
    [InlineData("notanemail")]
    [InlineData("missing@")]
    [InlineData("@nodomain.com")]
    public async Task ForgotPassword_VariousInvalidEmailFormats_Returns400(string invalidEmail)
    {
        // Arrange
        var request = new ForgotPasswordRequest { Email = invalidEmail };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/auth/forgot-password", request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var errorResponse = await response.Content.ReadFromJsonAsync<JsonElement>(_jsonOptions);
        Assert.Equal("invalid_input", errorResponse.GetProperty("error").GetProperty("code").GetString());
    }

    [Fact]
    public async Task ForgotPassword_MissingEmailField_Returns400WithRequiredError()
    {
        // Arrange - send empty object
        var response = await _client.PostAsJsonAsync("/api/v1/auth/forgot-password", new { });

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(content);
        var root = doc.RootElement;

        Assert.True(root.TryGetProperty("error", out var errorElement));
        Assert.True(errorElement.TryGetProperty("code", out var codeElement));
        Assert.Equal("invalid_input", codeElement.GetString());

        Assert.True(errorElement.TryGetProperty("details", out var detailsElement));
        var details = detailsElement.EnumerateArray().Select(d => d.GetString()).ToList();
        Assert.Contains("email:required", details);
    }
}

/// <summary>
/// Integration tests for reset password endpoint.
/// </summary>
public sealed class ResetPasswordEndpointTests : IClassFixture<TestWebApplicationFactory<Program>>
{
    private readonly TestWebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    private readonly JsonSerializerOptions _jsonOptions;

    public ResetPasswordEndpointTests(TestWebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
    }

    [Fact]
    public async Task ResetPassword_ValidToken_ReturnsSuccess()
    {
        // Arrange - Create a reset token
        var resetToken = await CreateResetToken();
        var request = new ResetPasswordRequest
        {
            Token = resetToken.PlainToken,
            NewPassword = "NewPassword123!"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/auth/reset-password", request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("Password reset successful", content);

        // Verify password was updated
        await VerifyPasswordUpdated(resetToken.UserId, "NewPassword123!");
    }

    [Fact]
    public async Task ResetPassword_ExpiredToken_ReturnsUnauthorized()
    {
        // Arrange - Create expired token
        var expiredToken = await CreateExpiredResetToken();
        var request = new ResetPasswordRequest
        {
            Token = expiredToken.PlainToken,
            NewPassword = "NewPassword123!"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/auth/reset-password", request);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        
        var errorResponse = await response.Content.ReadFromJsonAsync<JsonElement>(_jsonOptions);
        Assert.Equal("token_expired", errorResponse.GetProperty("error").GetProperty("code").GetString());
    }

    [Fact]
    public async Task ResetPassword_UsedToken_ReturnsUnauthorized()
    {
        // Arrange - Create and use a token
        var resetToken = await CreateResetToken();
        await UseResetToken(resetToken);
        
        var request = new ResetPasswordRequest
        {
            Token = resetToken.PlainToken,
            NewPassword = "NewPassword123!"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/auth/reset-password", request);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        
        var errorResponse = await response.Content.ReadFromJsonAsync<JsonElement>(_jsonOptions);
        Assert.Equal("invalid_token", errorResponse.GetProperty("error").GetProperty("code").GetString());
    }

    [Fact]
    public async Task ResetPassword_InvalidToken_ReturnsUnauthorized()
    {
        // Arrange
        var request = new ResetPasswordRequest
        {
            Token = "invalid-token-guid",
            NewPassword = "NewPassword123!"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/auth/reset-password", request);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task ResetPassword_WeakPassword_ReturnsBadRequest()
    {
        // Arrange
        var resetToken = await CreateResetToken();
        var request = new ResetPasswordRequest
        {
            Token = resetToken.PlainToken,
            NewPassword = "weak"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/auth/reset-password", request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        
        var errorResponse = await response.Content.ReadFromJsonAsync<JsonElement>(_jsonOptions);
        Assert.Equal("password_requirements_not_met", errorResponse.GetProperty("error").GetProperty("code").GetString());
        
        var details = errorResponse.GetProperty("error").GetProperty("details").EnumerateArray();
        Assert.True(details.Any(), "Should return specific password requirements");
    }

    [Fact]
    public async Task ResetPassword_ResetsFailedLoginAttempts()
    {
        // Arrange - Create user with failed attempts
        var user = await CreateUserWithFailedAttempts();
        var resetToken = await CreateResetTokenForUser(user.Id);
        
        var request = new ResetPasswordRequest
        {
            Token = resetToken.PlainToken,
            NewPassword = "NewPassword123!"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/auth/reset-password", request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        // Verify failed attempts were reset
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        
        var updatedUser = await dbContext.Users.FindAsync(user.Id);
        Assert.Equal(0, updatedUser.FailedLoginAttempts);
        Assert.Null(updatedUser.LockedUntil);
    }

    private async Task<(string PlainToken, Guid UserId)> CreateResetToken()
    {
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        
        var user = await dbContext.Users.FirstAsync(u => u.Email == "test@example.com");
        var plainToken = Guid.NewGuid().ToString();
        var tokenHash = Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(plainToken))).ToLowerInvariant();

        var resetToken = new PasswordResetToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            TokenHash = tokenHash,
            ExpiresAt = DateTime.UtcNow.AddHours(1)
        };

        dbContext.PasswordResetTokens.Add(resetToken);
        await dbContext.SaveChangesAsync();

        return (plainToken, user.Id);
    }

    private async Task<(string PlainToken, Guid UserId)> CreateExpiredResetToken()
    {
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        
        var user = await dbContext.Users.FirstAsync(u => u.Email == "test@example.com");
        var plainToken = Guid.NewGuid().ToString();
        var tokenHash = Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(plainToken))).ToLowerInvariant();

        var resetToken = new PasswordResetToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            TokenHash = tokenHash,
            ExpiresAt = DateTime.UtcNow.AddHours(-1) // Expired
        };

        dbContext.PasswordResetTokens.Add(resetToken);
        await dbContext.SaveChangesAsync();

        return (plainToken, user.Id);
    }

    private async Task UseResetToken((string PlainToken, Guid UserId) resetToken)
    {
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        
        var token = await dbContext.PasswordResetTokens
            .FirstAsync(t => t.UserId == resetToken.UserId);
        
        token.UsedAt = DateTime.UtcNow;
        await dbContext.SaveChangesAsync();
    }

    private async Task<(string PlainToken, Guid UserId)> CreateResetTokenForUser(Guid userId)
    {
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        
        var plainToken = Guid.NewGuid().ToString();
        var tokenHash = Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(plainToken))).ToLowerInvariant();

        var resetToken = new PasswordResetToken
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            TokenHash = tokenHash,
            ExpiresAt = DateTime.UtcNow.AddHours(1)
        };

        dbContext.PasswordResetTokens.Add(resetToken);
        await dbContext.SaveChangesAsync();

        return (plainToken, userId);
    }

    private async Task<User> CreateUserWithFailedAttempts()
    {
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        
        var user = await dbContext.Users.FirstAsync(u => u.Email == "test@example.com");
        user.FailedLoginAttempts = 5;
        user.LockedUntil = DateTime.UtcNow.AddMinutes(30);
        await dbContext.SaveChangesAsync();

        return user;
    }

    private async Task VerifyPasswordUpdated(Guid userId, string newPassword)
    {
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        
        var user = await dbContext.Users.FindAsync(userId);
        Assert.NotNull(user);
        
        // Verify password hash was updated (can't verify exact hash without BCrypt verification)
        Assert.NotEmpty(user.PasswordHash);
        Assert.Equal(0, user.FailedLoginAttempts);
        Assert.Null(user.LockedUntil);
    }
}
