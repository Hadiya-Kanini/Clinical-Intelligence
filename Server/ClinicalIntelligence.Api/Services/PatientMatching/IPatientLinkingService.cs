namespace ClinicalIntelligence.Api.Services.PatientMatching;

/// <summary>
/// Input for patient linkage operation.
/// </summary>
public sealed record PatientLinkageInput
{
    /// <summary>
    /// The document ID to link to a patient.
    /// </summary>
    public required Guid DocumentId { get; init; }

    /// <summary>
    /// Extracted MRN from the document (optional).
    /// </summary>
    public string? Mrn { get; init; }

    /// <summary>
    /// Extracted patient name from the document (optional).
    /// </summary>
    public string? Name { get; init; }

    /// <summary>
    /// Extracted DOB string from the document (optional).
    /// </summary>
    public string? DobString { get; init; }

    /// <summary>
    /// Extracted address from the document (optional).
    /// </summary>
    public string? Address { get; init; }

    /// <summary>
    /// Extracted contact from the document (optional).
    /// </summary>
    public string? Contact { get; init; }
}

/// <summary>
/// Result of patient linkage operation.
/// </summary>
public sealed record PatientLinkageResult
{
    /// <summary>
    /// The patient ID the document was linked to.
    /// </summary>
    public required Guid PatientId { get; init; }

    /// <summary>
    /// Whether the patient was newly created.
    /// </summary>
    public bool PatientWasCreated { get; init; }

    /// <summary>
    /// Whether the document was reassigned from a different patient.
    /// </summary>
    public bool WasReassigned { get; init; }

    /// <summary>
    /// The previous patient ID if reassigned.
    /// </summary>
    public Guid? PreviousPatientId { get; init; }

    /// <summary>
    /// Number of related records updated (entities, code suggestions, conflicts).
    /// </summary>
    public int RelatedRecordsUpdated { get; init; }

    /// <summary>
    /// Whether an MRN conflict was detected and recorded.
    /// </summary>
    public bool MrnConflictDetected { get; init; }
}

/// <summary>
/// Abstraction to apply patient linkage to documents/batches and keep related records consistent.
/// Implements FR-050: Link multiple documents to the same patient.
/// </summary>
public interface IPatientLinkingService
{
    /// <summary>
    /// Links a document to a patient based on extracted identity values.
    /// 
    /// In a transaction:
    /// 1. Loads the document and optional batch
    /// 2. Determines target patient via IPatientMatcher
    /// 3. Updates Document.PatientId and batch/documents if needed
    /// 4. Updates patient-scoped records (ExtractedEntity, CodeSuggestion, ErdConflict)
    /// </summary>
    /// <param name="input">The linkage input containing document ID and extracted identity values.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Result of the linkage operation.</returns>
    Task<PatientLinkageResult> LinkDocumentToPatientAsync(
        PatientLinkageInput input,
        CancellationToken cancellationToken = default);
}
