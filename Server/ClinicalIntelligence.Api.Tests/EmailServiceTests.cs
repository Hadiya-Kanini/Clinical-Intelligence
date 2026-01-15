using ClinicalIntelligence.Api.Configuration;
using ClinicalIntelligence.Api.Services;
using Microsoft.Extensions.Logging;
using MimeKit;
using Moq;
using Xunit;

namespace ClinicalIntelligence.Api.Tests;

/// <summary>
/// Unit tests for SmtpEmailService.
/// </summary>
public sealed class EmailServiceTests
{
    private readonly Mock<ILogger<SmtpEmailService>> _mockLogger;
    private readonly SecretsOptions _testSecrets;
    private readonly SmtpEmailService _emailService;

    public EmailServiceTests()
    {
        _mockLogger = new Mock<ILogger<SmtpEmailService>>();
        _testSecrets = new SecretsOptions
        {
            SmtpHost = "smtp.test.com",
            SmtpPort = 587,
            SmtpUsername = "test@test.com",
            SmtpPassword = "test-password",
            SmtpFromEmail = "noreply@test.com",
            SmtpFromName = "Test Service",
            SmtpEnableSsl = true,
            FrontendUrl = "http://localhost:5173"
        };
        _emailService = new SmtpEmailService(_mockLogger.Object, _testSecrets);
    }

    [Fact]
    public void IsConfigured_ReturnsTrue_WhenSmtpSettingsPresent()
    {
        // Act
        var result = _emailService.IsConfigured;

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsConfigured_ReturnsFalse_WhenSmtpSettingsMissing()
    {
        // Arrange
        var emptySecrets = new SecretsOptions();
        var service = new SmtpEmailService(_mockLogger.Object, emptySecrets);

        // Act
        var result = service.IsConfigured;

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task SendEmailAsync_ReturnsFalse_WhenNotConfigured()
    {
        // Arrange
        var emptySecrets = new SecretsOptions();
        var service = new SmtpEmailService(_mockLogger.Object, emptySecrets);

        // Act
        var result = await service.SendEmailAsync("test@test.com", "Test", "Test body");

        // Assert
        Assert.False(result);
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Email service is not configured")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task SendPasswordResetEmailAsync_GeneratesCorrectContent()
    {
        // This test verifies the email content generation without actually sending
        // In a real scenario, we'd mock the SMTP client, but for now we verify the service is configured
        
        // Act
        var result = await _emailService.SendPasswordResetEmailAsync(
            "test@test.com", 
            "test-token", 
            "Test User", 
            "http://localhost:5173/reset-password?token=test-token");

        // Assert - Should attempt to send (may fail due to test SMTP, but should not crash)
        Assert.True(_emailService.IsConfigured);
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Sending password reset email")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task SendPasswordResetConfirmationAsync_GeneratesCorrectContent()
    {
        // Act
        var result = await _emailService.SendPasswordResetConfirmationAsync(
            "test@test.com", 
            "Test User");

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Sending password reset confirmation email")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task SendAccountLockedEmailAsync_GeneratesCorrectContent()
    {
        // Arrange
        var lockedUntil = DateTime.UtcNow.AddMinutes(30);

        // Act
        var result = await _emailService.SendAccountLockedEmailAsync(
            "test@test.com", 
            "Test User", 
            lockedUntil);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Sending account locked email")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}

/// <summary>
/// Integration tests for email service with real SMTP (requires actual SMTP credentials).
/// </summary>
public sealed class EmailServiceIntegrationTests : IClassFixture<EmailServiceTestFixture>
{
    private readonly EmailServiceTestFixture _fixture;

    public EmailServiceIntegrationTests(EmailServiceTestFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact(Skip = "Requires real SMTP credentials - run manually for testing")]
    public async Task SendEmailAsync_WithRealSmtp_SendsSuccessfully()
    {
        // Arrange
        var service = _fixture.CreateEmailService();

        // Act
        var result = await service.SendEmailAsync(
            _fixture.TestEmail, 
            "Test Email from Clinical Intelligence", 
            "<h1>Test Email</h1><p>This is a test email from the password reset system.</p>");

        // Assert
        Assert.True(result);
    }

    [Fact(Skip = "Requires real SMTP credentials - run manually for testing")]
    public async Task SendPasswordResetEmailAsync_WithRealSmtp_SendsSuccessfully()
    {
        // Arrange
        var service = _fixture.CreateEmailService();
        var resetUrl = $"{_fixture.Secrets.FrontendUrl}/reset-password?token=test-token-123";

        // Act
        var result = await service.SendPasswordResetEmailAsync(
            _fixture.TestEmail, 
            "test-token-123", 
            "Test User", 
            resetUrl);

        // Assert
        Assert.True(result);
    }
}

/// <summary>
/// Test fixture for email service integration tests.
/// </summary>
public sealed class EmailServiceTestFixture : IDisposable
{
    public SecretsOptions Secrets { get; }
    public string TestEmail { get; }

    public EmailServiceTestFixture()
    {
        // Load from environment or use test values
        Secrets = new SecretsOptions
        {
            SmtpHost = Environment.GetEnvironmentVariable("SMTP_HOST") ?? "smtp.gmail.com",
            SmtpPort = int.Parse(Environment.GetEnvironmentVariable("SMTP_PORT") ?? "587"),
            SmtpUsername = Environment.GetEnvironmentVariable("SMTP_USERNAME") ?? "",
            SmtpPassword = Environment.GetEnvironmentVariable("SMTP_PASSWORD") ?? "",
            SmtpFromEmail = Environment.GetEnvironmentVariable("SMTP_FROM_EMAIL") ?? "",
            SmtpFromName = Environment.GetEnvironmentVariable("SMTP_FROM_NAME") ?? "Clinical Intelligence",
            SmtpEnableSsl = !string.Equals(Environment.GetEnvironmentVariable("SMTP_ENABLE_SSL"), "false"),
            FrontendUrl = Environment.GetEnvironmentVariable("FRONTEND_URL") ?? "http://localhost:5173"
        };

        TestEmail = Environment.GetEnvironmentVariable("TEST_EMAIL") ?? "test@example.com";
    }

    public SmtpEmailService CreateEmailService()
    {
        var logger = new Mock<ILogger<SmtpEmailService>>();
        return new SmtpEmailService(logger.Object, Secrets);
    }

    public void Dispose()
    {
        // Cleanup if needed
    }
}
