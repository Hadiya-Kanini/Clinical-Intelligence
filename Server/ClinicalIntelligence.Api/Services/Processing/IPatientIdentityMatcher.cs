namespace ClinicalIntelligence.Api.Services.Processing;

/// <summary>
/// Abstraction for patient identity matching using MRN or name+DOB.
/// Follows DIP: consumers depend on this interface, not the concrete implementation.
/// </summary>
public interface IPatientIdentityMatcher
{
    /// <summary>
    /// Attempts to match a patient by MRN.
    /// </summary>
    /// <param name="mrn">Medical Record Number to match.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Match result with patient ID if found.</returns>
    Task<PatientMatchResult> MatchByMrnAsync(string mrn, CancellationToken cancellationToken = default);

    /// <summary>
    /// Attempts to match a patient by normalized name and date of birth.
    /// </summary>
    /// <param name="name">Patient name.</param>
    /// <param name="dateOfBirth">Patient date of birth.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Match result with patient ID if found.</returns>
    Task<PatientMatchResult> MatchByNameAndDobAsync(
        string name,
        DateOnly dateOfBirth,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Attempts to match a patient using available identifiers.
    /// MRN takes priority over name+DOB.
    /// </summary>
    /// <param name="identifiers">Patient identifiers to match.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Match result with patient ID if found.</returns>
    Task<PatientMatchResult> MatchAsync(
        PatientIdentifiers identifiers,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Input model for patient identifiers used in matching.
/// </summary>
public sealed record PatientIdentifiers
{
    /// <summary>
    /// Medical Record Number (highest priority for matching).
    /// </summary>
    public string? Mrn { get; init; }

    /// <summary>
    /// Patient name for matching (used with DateOfBirth).
    /// </summary>
    public string? Name { get; init; }

    /// <summary>
    /// Patient date of birth for matching (used with Name).
    /// </summary>
    public DateOnly? DateOfBirth { get; init; }
}

/// <summary>
/// Result of a patient identity match attempt.
/// </summary>
public sealed record PatientMatchResult
{
    /// <summary>
    /// Whether a match was found.
    /// </summary>
    public bool IsMatch { get; init; }

    /// <summary>
    /// The matched patient ID (null if no match).
    /// </summary>
    public Guid? PatientId { get; init; }

    /// <summary>
    /// The method used for matching (MRN or NameDob).
    /// </summary>
    public PatientMatchMethod? MatchMethod { get; init; }

    /// <summary>
    /// Error or warning message (e.g., ambiguous match).
    /// </summary>
    public string? Message { get; init; }

    /// <summary>
    /// Whether the match is ambiguous (multiple candidates).
    /// </summary>
    public bool IsAmbiguous { get; init; }

    public static PatientMatchResult NotFound(string? message = null) =>
        new() { IsMatch = false, Message = message };

    public static PatientMatchResult Found(Guid patientId, PatientMatchMethod method) =>
        new() { IsMatch = true, PatientId = patientId, MatchMethod = method };

    public static PatientMatchResult Ambiguous(string message) =>
        new() { IsMatch = false, IsAmbiguous = true, Message = message };
}

/// <summary>
/// Method used for patient identity matching.
/// </summary>
public enum PatientMatchMethod
{
    Mrn,
    NameDob
}
