using System.Text.Json;
using System.Text.RegularExpressions;
using ClinicalIntelligence.Api.Data;
using ClinicalIntelligence.Api.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ClinicalIntelligence.Api.Services.Auth;

/// <summary>
/// Implementation of password reset service.
/// Validates token, updates password hash (bcrypt), marks token as used,
/// invalidates all user sessions, and writes audit log atomically.
/// </summary>
public sealed class PasswordResetService : IPasswordResetService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IPasswordResetTokenValidator _tokenValidator;
    private readonly IBcryptPasswordHasher _passwordHasher;
    private readonly ISessionInvalidationService _sessionInvalidationService;
    private readonly ILogger<PasswordResetService>? _logger;

    public PasswordResetService(
        ApplicationDbContext dbContext,
        IPasswordResetTokenValidator tokenValidator,
        IBcryptPasswordHasher passwordHasher,
        ISessionInvalidationService sessionInvalidationService,
        ILogger<PasswordResetService>? logger = null)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _tokenValidator = tokenValidator ?? throw new ArgumentNullException(nameof(tokenValidator));
        _passwordHasher = passwordHasher ?? throw new ArgumentNullException(nameof(passwordHasher));
        _sessionInvalidationService = sessionInvalidationService ?? throw new ArgumentNullException(nameof(sessionInvalidationService));
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<PasswordResetResult> ResetPasswordAsync(string plainToken, string newPassword, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(plainToken))
        {
            return PasswordResetResult.Failed("invalid_input", "Token is required.", new[] { "token:required" });
        }

        if (string.IsNullOrEmpty(newPassword))
        {
            return PasswordResetResult.Failed("invalid_input", "New password is required.", new[] { "newPassword:required" });
        }

        var passwordValidation = ValidatePasswordComplexity(newPassword);
        if (passwordValidation.Length > 0)
        {
            return PasswordResetResult.Failed("password_requirements_not_met", "Password does not meet complexity requirements.", passwordValidation);
        }

        var tokenValidation = await _tokenValidator.ValidateTokenAsync(plainToken, cancellationToken);

        if (!tokenValidation.IsValid)
        {
            return tokenValidation.InvalidReason switch
            {
                TokenInvalidReason.Missing => PasswordResetResult.Failed("invalid_input", "Token is required.", new[] { "token:required" }),
                TokenInvalidReason.Malformed => PasswordResetResult.Failed("invalid_input", "Invalid token format.", new[] { "token:invalid_format" }),
                TokenInvalidReason.NotFound => PasswordResetResult.Failed("invalid_token", "Invalid or expired reset link."),
                TokenInvalidReason.Expired => PasswordResetResult.Failed("token_expired", "Reset link has expired."),
                TokenInvalidReason.AlreadyUsed => PasswordResetResult.Failed("token_used", "This reset link has already been used."),
                TokenInvalidReason.UserInvalid => PasswordResetResult.Failed("invalid_token", "Invalid or expired reset link."),
                _ => PasswordResetResult.Failed("invalid_token", "Invalid or expired reset link.")
            };
        }

        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            // Atomic token consumption: conditionally update UsedAt only if still null
            // This prevents race conditions where concurrent requests could both succeed
            var now = DateTime.UtcNow;
            var rowsAffected = await _dbContext.PasswordResetTokens
                .Where(t => t.Id == tokenValidation.TokenId && t.UsedAt == null)
                .ExecuteUpdateAsync(s => s.SetProperty(t => t.UsedAt, now), cancellationToken);

            if (rowsAffected == 0)
            {
                // Token was already consumed by another request (race condition lost)
                await transaction.RollbackAsync(cancellationToken);
                return PasswordResetResult.Failed("invalid_token", "Invalid or expired reset link.");
            }

            // Reload token with user to get user data for password update
            var resetToken = await _dbContext.PasswordResetTokens
                .Include(t => t.User)
                .FirstOrDefaultAsync(t => t.Id == tokenValidation.TokenId, cancellationToken);

            if (resetToken == null)
            {
                await transaction.RollbackAsync(cancellationToken);
                return PasswordResetResult.Failed("invalid_token", "Invalid or expired reset link.");
            }

            var user = resetToken.User;
            if (user == null || user.IsDeleted)
            {
                await transaction.RollbackAsync(cancellationToken);
                return PasswordResetResult.Failed("invalid_token", "Invalid or expired reset link.");
            }

            var passwordHash = _passwordHasher.HashPassword(newPassword);

            user.PasswordHash = passwordHash;
            user.FailedLoginAttempts = 0;
            user.LockedUntil = null;
            user.UpdatedAt = now;

            // Invalidate all existing sessions for the user (US_031)
            var sessionInvalidationResult = await _sessionInvalidationService.InvalidateAllSessionsAsync(user.Id, cancellationToken);

            // Write audit log event for session invalidation (safe metadata only - no tokens/passwords)
            var auditEvent = new AuditLogEvent
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                SessionId = null,
                ActionType = "PASSWORD_RESET_SESSIONS_INVALIDATED",
                IpAddress = null,
                UserAgent = null,
                ResourceType = "Session",
                Timestamp = now,
                Metadata = JsonSerializer.Serialize(new
                {
                    revokedSessionCount = sessionInvalidationResult.RevokedCount
                })
            };
            _dbContext.AuditLogEvents.Add(auditEvent);

            await _dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            _logger?.LogInformation(
                "Password reset successful for user {UserId}. Invalidated {SessionCount} sessions.",
                user.Id,
                sessionInvalidationResult.RevokedCount);

            return PasswordResetResult.Succeeded(user.Id, user.Email, user.Name);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            _logger?.LogError(ex, "Error during password reset transaction");
            throw;
        }
    }

    /// <summary>
    /// Validates password complexity requirements (FR-009c).
    /// </summary>
    private static string[] ValidatePasswordComplexity(string password)
    {
        var errors = new List<string>();

        if (password.Length < 8)
            errors.Add("Password must be at least 8 characters.");
        if (!Regex.IsMatch(password, @"[A-Z]"))
            errors.Add("Password must contain at least one uppercase letter.");
        if (!Regex.IsMatch(password, @"[a-z]"))
            errors.Add("Password must contain at least one lowercase letter.");
        if (!Regex.IsMatch(password, @"\d"))
            errors.Add("Password must contain at least one digit.");
        if (!Regex.IsMatch(password, @"[^A-Za-z0-9]"))
            errors.Add("Password must contain at least one special character.");

        return errors.ToArray();
    }
}
