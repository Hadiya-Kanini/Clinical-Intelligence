namespace ClinicalIntelligence.Api.Services.Processing;

/// <summary>
/// Abstraction for selecting and ordering documents for patient-level merge processing.
/// Follows DIP: consumers depend on this interface, not the concrete implementation.
/// </summary>
public interface IPatientDocumentMergePlanner
{
    /// <summary>
    /// Plans a merge operation for all documents belonging to a patient.
    /// </summary>
    /// <param name="patientId">The patient identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Merge plan with ordered document IDs.</returns>
    Task<PatientMergePlan> PlanMergeAsync(
        Guid patientId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Plans a merge operation for specific documents belonging to a patient.
    /// </summary>
    /// <param name="patientId">The patient identifier.</param>
    /// <param name="documentIds">Specific document IDs to include.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Merge plan with ordered document IDs.</returns>
    Task<PatientMergePlan> PlanMergeAsync(
        Guid patientId,
        IEnumerable<Guid> documentIds,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Result of merge planning containing ordered document IDs and job payload.
/// </summary>
public sealed record PatientMergePlan
{
    /// <summary>
    /// Whether the plan is valid and can be executed.
    /// </summary>
    public bool IsValid { get; init; }

    /// <summary>
    /// The patient identifier.
    /// </summary>
    public Guid PatientId { get; init; }

    /// <summary>
    /// Ordered list of document IDs for merging.
    /// </summary>
    public IReadOnlyList<Guid> DocumentIds { get; init; } = Array.Empty<Guid>();

    /// <summary>
    /// Error message if the plan is invalid.
    /// </summary>
    public string? ErrorMessage { get; init; }

    /// <summary>
    /// Creates an invalid plan with an error message.
    /// </summary>
    public static PatientMergePlan Invalid(string errorMessage) =>
        new() { IsValid = false, ErrorMessage = errorMessage };

    /// <summary>
    /// Creates a valid plan with ordered document IDs.
    /// </summary>
    public static PatientMergePlan Valid(Guid patientId, IReadOnlyList<Guid> documentIds) =>
        new() { IsValid = true, PatientId = patientId, DocumentIds = documentIds };

    /// <summary>
    /// Converts the plan to a job payload object for the job contract.
    /// </summary>
    public object ToJobPayload()
    {
        return new
        {
            patient_id = PatientId.ToString(),
            document_ids = DocumentIds.Select(id => id.ToString()).ToArray()
        };
    }
}
