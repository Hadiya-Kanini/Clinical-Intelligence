using ClinicalIntelligence.Api.Contracts;

namespace ClinicalIntelligence.Api.Services;

/// <summary>
/// Abstraction for patient aggregate retrieval operations.
/// Supports dependency injection and testing.
/// </summary>
public interface IPatientAggregateService
{
    /// <summary>
    /// Retrieves the patient aggregate by patient ID.
    /// Returns null if patient not found.
    /// </summary>
    /// <param name="patientId">The patient identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Patient aggregate response or null if not found.</returns>
    Task<PatientAggregateResponse?> GetPatientAggregateAsync(Guid patientId, CancellationToken cancellationToken = default);
}
