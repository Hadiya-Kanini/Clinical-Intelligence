using System;
using Microsoft.Extensions.Logging;

namespace ClinicalIntelligence.Api.Services.Auth;

/// <summary>
/// Centralized bcrypt password hashing service with configurable work factor.
/// Enforces minimum work factor of 12 per OWASP recommendations.
/// Never logs plaintext passwords or includes them in exception messages.
/// </summary>
public sealed class BcryptPasswordHasher : IBcryptPasswordHasher
{
    /// <summary>
    /// Minimum allowed bcrypt work factor per OWASP Password Storage Cheat Sheet.
    /// </summary>
    public const int MinimumWorkFactor = 12;

    /// <summary>
    /// Maximum allowed bcrypt work factor to prevent DoS via excessive computation.
    /// </summary>
    public const int MaximumWorkFactor = 31;

    private readonly int _workFactor;
    private readonly ILogger<BcryptPasswordHasher>? _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="BcryptPasswordHasher"/> class.
    /// </summary>
    /// <param name="workFactor">The bcrypt work factor (cost). Must be >= 12.</param>
    /// <param name="logger">Optional logger for diagnostic messages (never logs passwords).</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when work factor is below minimum (12) or above maximum (31).</exception>
    public BcryptPasswordHasher(int workFactor, ILogger<BcryptPasswordHasher>? logger = null)
    {
        if (workFactor < MinimumWorkFactor)
        {
            throw new ArgumentOutOfRangeException(
                nameof(workFactor),
                workFactor,
                $"Bcrypt work factor must be at least {MinimumWorkFactor} for secure password hashing.");
        }

        if (workFactor > MaximumWorkFactor)
        {
            throw new ArgumentOutOfRangeException(
                nameof(workFactor),
                workFactor,
                $"Bcrypt work factor must not exceed {MaximumWorkFactor}.");
        }

        _workFactor = workFactor;
        _logger = logger;
    }

    /// <inheritdoc />
    public int WorkFactor => _workFactor;

    /// <inheritdoc />
    public string HashPassword(string password)
    {
        if (string.IsNullOrEmpty(password))
        {
            throw new ArgumentException("Password cannot be null or empty.", nameof(password));
        }

        var hash = BCrypt.Net.BCrypt.HashPassword(password, _workFactor);

        _logger?.LogDebug("Password hashed successfully with work factor {WorkFactor}", _workFactor);

        return hash;
    }

    /// <inheritdoc />
    public bool Verify(string password, string passwordHash)
    {
        if (string.IsNullOrEmpty(password))
        {
            _logger?.LogDebug("Password verification failed: empty password provided");
            return false;
        }

        if (string.IsNullOrEmpty(passwordHash))
        {
            _logger?.LogDebug("Password verification failed: empty hash provided");
            return false;
        }

        try
        {
            var result = BCrypt.Net.BCrypt.Verify(password, passwordHash);

            if (result)
            {
                _logger?.LogDebug("Password verification succeeded");
            }
            else
            {
                _logger?.LogDebug("Password verification failed: password mismatch");
            }

            return result;
        }
        catch (BCrypt.Net.SaltParseException)
        {
            _logger?.LogWarning("Password verification failed: invalid hash format");
            return false;
        }
        catch (Exception ex) when (ex is not OutOfMemoryException)
        {
            _logger?.LogWarning(ex, "Password verification failed due to unexpected error");
            return false;
        }
    }
}
