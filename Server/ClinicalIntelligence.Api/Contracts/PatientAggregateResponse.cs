namespace ClinicalIntelligence.Api.Contracts;

/// <summary>
/// Read contract organizing patient data by category for downstream usage (Patient 360, exports).
/// Decoupled from FHIR import/export representations for Phase 1 stability.
/// </summary>
public sealed record PatientAggregateResponse
{
    /// <summary>
    /// Patient identifier.
    /// </summary>
    public required Guid PatientId { get; init; }

    /// <summary>
    /// Patient demographics information.
    /// </summary>
    public required PatientDemographicsDto Demographics { get; init; }

    /// <summary>
    /// Patient encounters/visits. Empty collection if none exist.
    /// </summary>
    public required IReadOnlyList<EncounterDto> Encounters { get; init; }

    /// <summary>
    /// Patient observations (labs, vitals). Empty collection if none exist.
    /// </summary>
    public required IReadOnlyList<ObservationDto> Observations { get; init; }

    /// <summary>
    /// Patient medications. Empty collection if none exist.
    /// </summary>
    public required IReadOnlyList<MedicationDto> Medications { get; init; }

    /// <summary>
    /// Patient diagnoses/conditions. Empty collection if none exist.
    /// </summary>
    public required IReadOnlyList<DiagnosisDto> Diagnoses { get; init; }

    /// <summary>
    /// Patient procedures. Empty collection if none exist.
    /// </summary>
    public required IReadOnlyList<ProcedureDto> Procedures { get; init; }

    /// <summary>
    /// Timestamp when the aggregate was retrieved.
    /// </summary>
    public required DateTime RetrievedAt { get; init; }
}

/// <summary>
/// Patient demographics DTO.
/// </summary>
public sealed record PatientDemographicsDto
{
    public required Guid Id { get; init; }
    public string? Mrn { get; init; }
    public string? GivenName { get; init; }
    public string? FamilyName { get; init; }
    public DateOnly? DateOfBirth { get; init; }
    public string? Gender { get; init; }
    public string? Address { get; init; }
    public string? Phone { get; init; }
    public string? Email { get; init; }
    public bool IsActive { get; init; }
}

/// <summary>
/// Encounter/visit DTO.
/// </summary>
public sealed record EncounterDto
{
    public required Guid Id { get; init; }
    public required string Status { get; init; }
    public string? Class { get; init; }
    public string? Type { get; init; }
    public DateTime? StartDate { get; init; }
    public DateTime? EndDate { get; init; }
    public string? ReasonCode { get; init; }
    public string? Location { get; init; }
}

/// <summary>
/// Observation (labs/vitals) DTO.
/// </summary>
public sealed record ObservationDto
{
    public required Guid Id { get; init; }
    public Guid? EncounterId { get; init; }
    public required string Status { get; init; }
    public string? Category { get; init; }
    public required string Code { get; init; }
    public string? CodeDisplay { get; init; }
    public string? Value { get; init; }
    public string? Unit { get; init; }
    public DateTime? EffectiveDate { get; init; }
    public string? Interpretation { get; init; }
    public string? ReferenceRangeLow { get; init; }
    public string? ReferenceRangeHigh { get; init; }
}

/// <summary>
/// Medication DTO.
/// </summary>
public sealed record MedicationDto
{
    public required Guid Id { get; init; }
    public required string Status { get; init; }
    public string? MedicationCode { get; init; }
    public required string MedicationName { get; init; }
    public string? Dosage { get; init; }
    public string? Route { get; init; }
    public string? Frequency { get; init; }
    public DateTime? EffectiveDate { get; init; }
    public DateTime? EndDate { get; init; }
    public string? ReasonCode { get; init; }
}

/// <summary>
/// Diagnosis/condition DTO.
/// </summary>
public sealed record DiagnosisDto
{
    public required Guid Id { get; init; }
    public Guid? EncounterId { get; init; }
    public string? ClinicalStatus { get; init; }
    public string? VerificationStatus { get; init; }
    public string? Category { get; init; }
    public string? Code { get; init; }
    public required string CodeDisplay { get; init; }
    public DateTime? OnsetDate { get; init; }
    public DateTime? AbatementDate { get; init; }
    public string? Severity { get; init; }
}

/// <summary>
/// Procedure DTO.
/// </summary>
public sealed record ProcedureDto
{
    public required Guid Id { get; init; }
    public Guid? EncounterId { get; init; }
    public required string Status { get; init; }
    public string? Code { get; init; }
    public required string CodeDisplay { get; init; }
    public string? Category { get; init; }
    public DateTime? PerformedDate { get; init; }
    public DateTime? PerformedEndDate { get; init; }
    public string? ReasonCode { get; init; }
    public string? BodySite { get; init; }
    public string? Outcome { get; init; }
}
