using ClinicalIntelligence.Api.Domain.Models;

namespace ClinicalIntelligence.Api.Services.PatientMatching;

/// <summary>
/// Input model for patient matching containing extracted identity values.
/// </summary>
public sealed record PatientMatchInput
{
    /// <summary>
    /// Medical Record Number from extraction (optional).
    /// </summary>
    public string? Mrn { get; init; }

    /// <summary>
    /// Patient name from extraction (optional).
    /// </summary>
    public string? Name { get; init; }

    /// <summary>
    /// Date of birth string from extraction (optional).
    /// Will be parsed to DateOnly during matching.
    /// </summary>
    public string? DobString { get; init; }

    /// <summary>
    /// Address from extraction (optional).
    /// </summary>
    public string? Address { get; init; }

    /// <summary>
    /// Contact information from extraction (optional).
    /// </summary>
    public string? Contact { get; init; }
}

/// <summary>
/// Result of patient matching operation.
/// </summary>
public sealed record PatientMatchResult
{
    /// <summary>
    /// The matched or created patient.
    /// </summary>
    public required ErdPatient Patient { get; init; }

    /// <summary>
    /// Indicates whether the patient was newly created.
    /// </summary>
    public bool WasCreated { get; init; }

    /// <summary>
    /// The match type used to find the patient.
    /// </summary>
    public PatientMatchType MatchType { get; init; }
}

/// <summary>
/// Type of match used to identify the patient.
/// </summary>
public enum PatientMatchType
{
    /// <summary>
    /// Patient was matched by MRN (primary match).
    /// </summary>
    MrnMatch,

    /// <summary>
    /// Patient was matched by normalized name + DOB (fallback match).
    /// </summary>
    NameDobMatch,

    /// <summary>
    /// No match found; patient was created.
    /// </summary>
    Created
}

/// <summary>
/// Abstraction for deterministic patient matching based on MRN and fallback name+DOB.
/// Implements FR-050: Link multiple documents to the same patient via MRN or name+DOB matching.
/// </summary>
public interface IPatientMatcher
{
    /// <summary>
    /// Finds an existing patient or creates a new one based on extracted identity values.
    /// 
    /// Matching priority:
    /// 1. MRN match (primary) - if normalized MRN matches an existing patient
    /// 2. Name + DOB match (fallback) - if both normalized name and parsed DOB match
    /// 3. Create new patient - if no match found
    /// </summary>
    /// <param name="input">The extracted patient identity values.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The matched or created patient with match metadata.</returns>
    Task<PatientMatchResult> FindOrCreatePatientAsync(
        PatientMatchInput input,
        CancellationToken cancellationToken = default);
}
