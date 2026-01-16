using ClinicalIntelligence.Api.Services;

namespace ClinicalIntelligence.Api.Tests.Fakes;

/// <summary>
/// Fake email service for integration tests.
/// Captures send invocations for verification without sending real emails.
/// </summary>
public sealed class FakeEmailService : IEmailService
{
    private readonly List<NewUserCredentialsEmailInvocation> _newUserCredentialsInvocations = new();
    private bool _shouldSucceed = true;

    /// <summary>
    /// Gets the list of new user credentials email invocations.
    /// </summary>
    public IReadOnlyList<NewUserCredentialsEmailInvocation> NewUserCredentialsInvocations => _newUserCredentialsInvocations;

    /// <summary>
    /// Configures whether email sends should succeed or fail.
    /// </summary>
    public void SetShouldSucceed(bool shouldSucceed) => _shouldSucceed = shouldSucceed;

    /// <summary>
    /// Clears all recorded invocations.
    /// </summary>
    public void ClearInvocations() => _newUserCredentialsInvocations.Clear();

    /// <inheritdoc />
    public bool IsConfigured => true;

    /// <inheritdoc />
    public Task<bool> SendEmailAsync(string to, string subject, string body, bool isHtml = true)
    {
        return Task.FromResult(_shouldSucceed);
    }

    /// <inheritdoc />
    public Task<bool> SendPasswordResetEmailAsync(string to, string resetToken, string userName, string resetUrl)
    {
        return Task.FromResult(_shouldSucceed);
    }

    /// <inheritdoc />
    public Task<bool> SendPasswordResetConfirmationAsync(string to, string userName)
    {
        return Task.FromResult(_shouldSucceed);
    }

    /// <inheritdoc />
    public Task<bool> SendAccountLockedEmailAsync(string to, string userName, DateTime lockedUntil)
    {
        return Task.FromResult(_shouldSucceed);
    }

    /// <inheritdoc />
    public Task<bool> SendNewUserCredentialsEmailAsync(string to, string userName, string temporaryPassword)
    {
        _newUserCredentialsInvocations.Add(new NewUserCredentialsEmailInvocation(to, userName, temporaryPassword));
        return Task.FromResult(_shouldSucceed);
    }

    /// <summary>
    /// Record of a new user credentials email invocation.
    /// </summary>
    public sealed record NewUserCredentialsEmailInvocation(string To, string UserName, string TemporaryPassword);
}
