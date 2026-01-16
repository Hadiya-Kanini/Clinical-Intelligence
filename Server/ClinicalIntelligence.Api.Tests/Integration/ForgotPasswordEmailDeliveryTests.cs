using ClinicalIntelligence.Api.Configuration;
using ClinicalIntelligence.Api.Data;
using ClinicalIntelligence.Api.Domain.Models;
using ClinicalIntelligence.Api.Services;
using ClinicalIntelligence.Api.Services.Auth;
using ClinicalIntelligence.Api.Services.Email;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace ClinicalIntelligence.Api.Tests.Integration;

/// <summary>
/// Integration tests for forgot-password email delivery (US_026).
/// Tests that reset emails are sent for existing users, contain correct reset links,
/// and handle failures gracefully while preserving non-enumeration behavior.
/// </summary>
public class ForgotPasswordEmailDeliveryTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private const string TestEmail = "email-delivery-test@example.com";
    private const string TestPassword = "TestPassword123!";

    public ForgotPasswordEmailDeliveryTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    private bool IsPostgreSqlAvailable()
    {
        try
        {
            using var scope = _factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            return dbContext.Database.CanConnect();
        }
        catch
        {
            return false;
        }
    }

    private WebApplicationFactory<Program> CreateFactoryWithMockEmailSender(MockSmtpEmailSender mockSender, MockEmailService? mockEmailService = null)
    {
        return _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // Remove existing ISmtpEmailSender registration
                var smtpDescriptor = services.FirstOrDefault(d => d.ServiceType == typeof(ISmtpEmailSender));
                if (smtpDescriptor != null)
                {
                    services.Remove(smtpDescriptor);
                }

                // Remove existing IEmailService registration if mock provided
                if (mockEmailService != null)
                {
                    var emailDescriptor = services.FirstOrDefault(d => d.ServiceType == typeof(IEmailService));
                    if (emailDescriptor != null)
                    {
                        services.Remove(emailDescriptor);
                    }
                    services.AddSingleton<IEmailService>(mockEmailService);
                }

                // Add mock SMTP sender
                services.AddSingleton<ISmtpEmailSender>(mockSender);
            });
        });
    }

    private async Task<User> EnsureTestUserExistsAsync(ApplicationDbContext dbContext)
    {
        var user = await dbContext.Users
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.Email == TestEmail);

        if (user == null)
        {
            user = new User
            {
                Id = Guid.NewGuid(),
                Email = TestEmail,
                Name = "Email Delivery Test User",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(TestPassword),
                Role = "Standard",
                Status = "Active",
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            dbContext.Users.Add(user);
            await dbContext.SaveChangesAsync();
        }
        return user;
    }

    private async Task CleanupUserTokensAsync(ApplicationDbContext dbContext, Guid userId)
    {
        var existingTokens = await dbContext.PasswordResetTokens
            .Where(t => t.UserId == userId)
            .ToListAsync();
        dbContext.PasswordResetTokens.RemoveRange(existingTokens);
        await dbContext.SaveChangesAsync();
    }

    [Fact]
    public async Task ForgotPassword_ExistingUser_SendsEmailWithResetLink()
    {
        if (!IsPostgreSqlAvailable()) return;

        // Arrange
        var mockEmailService = new MockEmailService();
        var mockSmtpSender = new MockSmtpEmailSender();

        using var customFactory = CreateFactoryWithMockEmailSender(mockSmtpSender, mockEmailService);
        using var scope = customFactory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var user = await EnsureTestUserExistsAsync(dbContext);
        await CleanupUserTokensAsync(dbContext, user.Id);

        var client = customFactory.CreateClient();

        // Act
        var response = await client.PostAsJsonAsync("/api/v1/auth/forgot-password", new
        {
            email = TestEmail
        });

        // Assert - Response should be 200 OK
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        // Wait briefly for async email sending
        await Task.Delay(500);

        // Verify email service was called
        Assert.True(mockEmailService.SendPasswordResetEmailCalled,
            "Email service should be called for existing user");

        // Verify recipient email
        Assert.Equal(TestEmail, mockEmailService.LastRecipientEmail);

        // Verify reset URL contains expected path and token parameter
        Assert.NotNull(mockEmailService.LastResetUrl);
        Assert.Contains("/reset-password", mockEmailService.LastResetUrl);
        Assert.Contains("token=", mockEmailService.LastResetUrl);
    }

    [Fact]
    public async Task ForgotPassword_ExistingUser_ResetLinkPointsToFrontendResetPage()
    {
        if (!IsPostgreSqlAvailable()) return;

        // Arrange
        var mockEmailService = new MockEmailService();
        var mockSmtpSender = new MockSmtpEmailSender();

        using var customFactory = CreateFactoryWithMockEmailSender(mockSmtpSender, mockEmailService);
        using var scope = customFactory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var user = await EnsureTestUserExistsAsync(dbContext);
        await CleanupUserTokensAsync(dbContext, user.Id);

        var client = customFactory.CreateClient();

        // Act
        var response = await client.PostAsJsonAsync("/api/v1/auth/forgot-password", new
        {
            email = TestEmail
        });

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        await Task.Delay(500);

        // Verify reset URL format: should be {baseUrl}/reset-password?token={token}
        Assert.NotNull(mockEmailService.LastResetUrl);
        
        var uri = new Uri(mockEmailService.LastResetUrl);
        Assert.Equal("/reset-password", uri.AbsolutePath);
        
        var query = System.Web.HttpUtility.ParseQueryString(uri.Query);
        var token = query["token"];
        Assert.NotNull(token);
        Assert.NotEmpty(token);
    }

    [Fact]
    public async Task ForgotPassword_NonExistingEmail_ReturnsOk_DoesNotSendEmail()
    {
        if (!IsPostgreSqlAvailable()) return;

        // Arrange
        var mockEmailService = new MockEmailService();
        var mockSmtpSender = new MockSmtpEmailSender();
        var nonExistingEmail = $"nonexistent-{Guid.NewGuid()}@example.com";

        using var customFactory = CreateFactoryWithMockEmailSender(mockSmtpSender, mockEmailService);
        var client = customFactory.CreateClient();

        // Act
        var response = await client.PostAsJsonAsync("/api/v1/auth/forgot-password", new
        {
            email = nonExistingEmail
        });

        // Assert - Same response as existing email (no enumeration)
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("If the email exists", content);

        // Wait briefly to ensure async operations complete
        await Task.Delay(500);

        // Email service should NOT be called for non-existing user
        Assert.False(mockEmailService.SendPasswordResetEmailCalled,
            "Email service should NOT be called for non-existing user");
    }

    [Fact]
    public async Task ForgotPassword_EmailSenderThrows_ReturnsGenericOk()
    {
        if (!IsPostgreSqlAvailable()) return;

        // Arrange - Configure mock to throw exception
        var mockEmailService = new MockEmailService { ShouldThrow = true };
        var mockSmtpSender = new MockSmtpEmailSender { ShouldThrow = true };

        using var customFactory = CreateFactoryWithMockEmailSender(mockSmtpSender, mockEmailService);
        using var scope = customFactory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var user = await EnsureTestUserExistsAsync(dbContext);
        await CleanupUserTokensAsync(dbContext, user.Id);

        var client = customFactory.CreateClient();

        // Act
        var response = await client.PostAsJsonAsync("/api/v1/auth/forgot-password", new
        {
            email = TestEmail
        });

        // Assert - Should still return 200 OK (non-enumeration behavior preserved)
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("If the email exists", content);
    }

    [Fact]
    public async Task ForgotPassword_ExistingVsNonExisting_ResponsesIndistinguishable()
    {
        if (!IsPostgreSqlAvailable()) return;

        // Arrange
        var mockEmailService = new MockEmailService();
        var mockSmtpSender = new MockSmtpEmailSender();

        using var customFactory = CreateFactoryWithMockEmailSender(mockSmtpSender, mockEmailService);
        using var scope = customFactory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var user = await EnsureTestUserExistsAsync(dbContext);
        await CleanupUserTokensAsync(dbContext, user.Id);

        var client = customFactory.CreateClient();
        var nonExistingEmail = $"nonexistent-{Guid.NewGuid()}@example.com";

        // Act
        var responseExisting = await client.PostAsJsonAsync("/api/v1/auth/forgot-password", new
        {
            email = TestEmail
        });

        // Reset mock state
        mockEmailService.Reset();

        var responseNonExisting = await client.PostAsJsonAsync("/api/v1/auth/forgot-password", new
        {
            email = nonExistingEmail
        });

        // Assert - Both should return 200 with same message structure
        Assert.Equal(HttpStatusCode.OK, responseExisting.StatusCode);
        Assert.Equal(HttpStatusCode.OK, responseNonExisting.StatusCode);

        var contentExisting = await responseExisting.Content.ReadAsStringAsync();
        var contentNonExisting = await responseNonExisting.Content.ReadAsStringAsync();

        // Both should contain the same generic message
        Assert.Contains("If the email exists", contentExisting);
        Assert.Contains("If the email exists", contentNonExisting);

        // Response structure should be identical
        Assert.Equal(contentExisting, contentNonExisting);
    }

    [Fact]
    public async Task ForgotPassword_ResetLinkToken_IsNotLoggedInPlainText()
    {
        if (!IsPostgreSqlAvailable()) return;

        // Arrange
        var mockEmailService = new MockEmailService();
        var mockSmtpSender = new MockSmtpEmailSender();

        using var customFactory = CreateFactoryWithMockEmailSender(mockSmtpSender, mockEmailService);
        using var scope = customFactory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var user = await EnsureTestUserExistsAsync(dbContext);
        await CleanupUserTokensAsync(dbContext, user.Id);

        var client = customFactory.CreateClient();

        // Act
        var response = await client.PostAsJsonAsync("/api/v1/auth/forgot-password", new
        {
            email = TestEmail
        });

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        await Task.Delay(500);

        // Get the token from the reset URL
        Assert.NotNull(mockEmailService.LastResetUrl);
        var uri = new Uri(mockEmailService.LastResetUrl);
        var query = System.Web.HttpUtility.ParseQueryString(uri.Query);
        var plainToken = query["token"];

        // Verify token is stored as hash in database, not plain text
        var storedToken = await dbContext.PasswordResetTokens
            .FirstOrDefaultAsync(t => t.UserId == user.Id && t.UsedAt == null);

        Assert.NotNull(storedToken);
        Assert.NotNull(storedToken.TokenHash);

        // Plain token should NOT equal stored hash
        Assert.NotEqual(plainToken, storedToken.TokenHash);

        // Hash should be 64 characters (SHA-256 hex)
        Assert.Equal(64, storedToken.TokenHash.Length);
    }

    [Fact]
    public void PasswordResetLinkBuilder_BuildResetUrl_ProducesCorrectFormat()
    {
        // Arrange
        var options = new FrontendUrlsOptions
        {
            BaseUrl = "https://app.example.com",
            PasswordResetPath = "/reset-password"
        };
        var linkBuilder = new PasswordResetLinkBuilder(options);
        var testToken = "test-token-123";

        // Act
        var url = linkBuilder.BuildResetUrl(testToken);

        // Assert
        Assert.Equal("https://app.example.com/reset-password?token=test-token-123", url);
    }

    [Fact]
    public void PasswordResetLinkBuilder_BuildResetUrl_EncodesSpecialCharacters()
    {
        // Arrange
        var options = new FrontendUrlsOptions
        {
            BaseUrl = "https://app.example.com",
            PasswordResetPath = "/reset-password"
        };
        var linkBuilder = new PasswordResetLinkBuilder(options);
        var tokenWithSpecialChars = "abc+def/ghi=";

        // Act
        var url = linkBuilder.BuildResetUrl(tokenWithSpecialChars);

        // Assert - Special characters should be URL-encoded
        Assert.Contains("token=abc%2Bdef%2Fghi%3D", url);
    }

    [Fact]
    public void PasswordResetLinkBuilder_BuildResetUrl_ThrowsOnEmptyToken()
    {
        // Arrange
        var options = new FrontendUrlsOptions
        {
            BaseUrl = "https://app.example.com",
            PasswordResetPath = "/reset-password"
        };
        var linkBuilder = new PasswordResetLinkBuilder(options);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => linkBuilder.BuildResetUrl(""));
        Assert.Throws<ArgumentException>(() => linkBuilder.BuildResetUrl(null!));
    }

    [Fact]
    public void SmtpOptions_FromConfiguration_LoadsCorrectValues()
    {
        // Arrange
        var configValues = new Dictionary<string, string?>
        {
            ["SMTP_HOST"] = "smtp.example.com",
            ["SMTP_PORT"] = "465",
            ["SMTP_USERNAME"] = "user@example.com",
            ["SMTP_PASSWORD"] = "secret",
            ["SMTP_FROM_EMAIL"] = "noreply@example.com",
            ["SMTP_FROM_NAME"] = "Test App",
            ["SMTP_ENABLE_SSL"] = "true",
            ["SMTP_TIMEOUT_SECONDS"] = "60"
        };

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configValues)
            .Build();

        // Act
        var options = SmtpOptions.FromConfiguration(configuration);

        // Assert
        Assert.Equal("smtp.example.com", options.Host);
        Assert.Equal(465, options.Port);
        Assert.Equal("user@example.com", options.Username);
        Assert.Equal("secret", options.Password);
        Assert.Equal("noreply@example.com", options.FromEmail);
        Assert.Equal("Test App", options.FromName);
        Assert.True(options.EnableSsl);
        Assert.Equal(60, options.TimeoutSeconds);
        Assert.True(options.IsConfigured);
    }

    [Fact]
    public void SmtpOptions_Validate_ReturnsFalseWhenNotConfigured()
    {
        // Arrange
        var options = new SmtpOptions();

        // Act
        var result = options.Validate();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void SmtpOptions_Validate_ThrowsWhenPartiallyConfigured()
    {
        // Arrange - Only host set
        var optionsHostOnly = new SmtpOptions { Host = "smtp.example.com" };

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => optionsHostOnly.Validate());

        // Arrange - Only from email set
        var optionsFromOnly = new SmtpOptions { FromEmail = "test@example.com" };

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => optionsFromOnly.Validate());
    }
}

/// <summary>
/// Mock implementation of ISmtpEmailSender for testing.
/// </summary>
public class MockSmtpEmailSender : ISmtpEmailSender
{
    public bool IsConfigured => true;
    public bool ShouldThrow { get; set; }
    public ConcurrentBag<(string To, string Subject, string HtmlBody)> SentEmails { get; } = new();

    public Task<bool> SendAsync(string to, string subject, string htmlBody, string? textBody = null, CancellationToken cancellationToken = default)
    {
        if (ShouldThrow)
        {
            throw new InvalidOperationException("Mock SMTP sender configured to throw");
        }

        SentEmails.Add((to, subject, htmlBody));
        return Task.FromResult(true);
    }
}

/// <summary>
/// Mock implementation of IEmailService for testing.
/// </summary>
public class MockEmailService : IEmailService
{
    public bool IsConfigured => true;
    public bool ShouldThrow { get; set; }
    public bool SendPasswordResetEmailCalled { get; private set; }
    public string? LastRecipientEmail { get; private set; }
    public string? LastResetUrl { get; private set; }
    public string? LastUserName { get; private set; }

    public void Reset()
    {
        SendPasswordResetEmailCalled = false;
        LastRecipientEmail = null;
        LastResetUrl = null;
        LastUserName = null;
    }

    public Task<bool> SendEmailAsync(string to, string subject, string body, bool isHtml = true)
    {
        if (ShouldThrow)
        {
            throw new InvalidOperationException("Mock email service configured to throw");
        }
        return Task.FromResult(true);
    }

    public Task<bool> SendPasswordResetEmailAsync(string to, string resetToken, string userName, string resetUrl)
    {
        if (ShouldThrow)
        {
            throw new InvalidOperationException("Mock email service configured to throw");
        }

        SendPasswordResetEmailCalled = true;
        LastRecipientEmail = to;
        LastResetUrl = resetUrl;
        LastUserName = userName;
        return Task.FromResult(true);
    }

    public Task<bool> SendPasswordResetConfirmationAsync(string to, string userName)
    {
        if (ShouldThrow)
        {
            throw new InvalidOperationException("Mock email service configured to throw");
        }
        return Task.FromResult(true);
    }

    public Task<bool> SendAccountLockedEmailAsync(string to, string userName, DateTime lockedUntil)
    {
        if (ShouldThrow)
        {
            throw new InvalidOperationException("Mock email service configured to throw");
        }
        return Task.FromResult(true);
    }

    public Task<bool> SendNewUserCredentialsEmailAsync(string to, string userName, string temporaryPassword)
    {
        if (ShouldThrow)
        {
            throw new InvalidOperationException("Mock email service configured to throw");
        }
        return Task.FromResult(true);
    }
}
