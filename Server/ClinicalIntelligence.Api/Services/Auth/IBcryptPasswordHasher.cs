namespace ClinicalIntelligence.Api.Services.Auth;

/// <summary>
/// Interface for bcrypt password hashing operations.
/// Provides abstraction for password hashing and verification with configurable work factor.
/// </summary>
public interface IBcryptPasswordHasher
{
    /// <summary>
    /// Hashes a password using bcrypt with the configured work factor.
    /// </summary>
    /// <param name="password">The plaintext password to hash. Must not be null or empty.</param>
    /// <returns>The bcrypt hash of the password.</returns>
    /// <exception cref="ArgumentException">Thrown when password is null or empty.</exception>
    string HashPassword(string password);

    /// <summary>
    /// Verifies a password against a bcrypt hash using timing-safe comparison.
    /// </summary>
    /// <param name="password">The plaintext password to verify.</param>
    /// <param name="passwordHash">The bcrypt hash to verify against.</param>
    /// <returns>True if the password matches the hash; otherwise false.</returns>
    bool Verify(string password, string passwordHash);

    /// <summary>
    /// Gets the configured bcrypt work factor.
    /// </summary>
    int WorkFactor { get; }
}
