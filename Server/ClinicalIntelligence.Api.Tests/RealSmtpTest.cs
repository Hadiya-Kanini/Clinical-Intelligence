using ClinicalIntelligence.Api.Configuration;
using ClinicalIntelligence.Api.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace ClinicalIntelligence.Api.Tests;

/// <summary>
/// Manual test for real SMTP functionality.
/// Run this test manually with actual SMTP credentials to verify email sending.
/// </summary>
public sealed class RealSmtpTest
{
    [Fact(Skip = "Manual test - requires real SMTP credentials")]
    public async Task TestRealSmtpConnection()
    {
        // Arrange - Load real configuration from environment
        var secrets = new SecretsOptions
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

        var logger = new Mock<ILogger<SmtpEmailService>>();
        var emailService = new SmtpEmailService(logger.Object, secrets);

        // Act & Assert - Verify service is configured
        Assert.True(emailService.IsConfigured, "Email service should be configured with SMTP credentials");

        // Test sending a test email
        var testEmail = Environment.GetEnvironmentVariable("TEST_EMAIL") ?? secrets.SmtpFromEmail;
        var result = await emailService.SendEmailAsync(
            testEmail,
            "Test Email from Clinical Intelligence",
            "<h1>Test Successful</h1><p>This is a test email to verify SMTP configuration is working.</p>");

        Assert.True(result, "Email should be sent successfully");

        // Test password reset email
        var resetResult = await emailService.SendPasswordResetEmailAsync(
            testEmail,
            "test-token-123",
            "Test User",
            $"{secrets.FrontendUrl}/reset-password?token=test-token-123");

        Assert.True(resetResult, "Password reset email should be sent successfully");

        // Test confirmation email
        var confirmResult = await emailService.SendPasswordResetConfirmationAsync(
            testEmail,
            "Test User");

        Assert.True(confirmResult, "Confirmation email should be sent successfully");

        logger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Sending password reset email")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once, "Should log password reset email sending");
    }
}
