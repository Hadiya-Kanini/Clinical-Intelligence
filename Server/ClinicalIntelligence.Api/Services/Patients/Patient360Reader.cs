using ClinicalIntelligence.Api.Contracts.Entities;
using ClinicalIntelligence.Api.Contracts.Patients;
using ClinicalIntelligence.Api.Data;
using ClinicalIntelligence.Api.Services.Entities;
using Microsoft.EntityFrameworkCore;

namespace ClinicalIntelligence.Api.Services.Patients;

/// <summary>
/// Query aggregator for Patient 360 with grounding filter.
/// Enforces the "trust-first" invariant by only returning grounded entities.
/// </summary>
public interface IPatient360Reader
{
    /// <summary>
    /// Gets the Patient 360 view for a patient, including only grounded entities.
    /// Enforces FR-051, FR-056: Only entities with valid source citations are returned.
    /// </summary>
    /// <param name="patientId">The patient ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Patient 360 response with grounded entities and citations.</returns>
    Task<Patient360Response?> GetPatient360Async(
        Guid patientId,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// EF Core implementation of Patient 360 reader with grounding enforcement.
/// </summary>
public sealed class Patient360Reader : IPatient360Reader
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IEntityCitationReader _citationReader;
    private readonly ILogger<Patient360Reader> _logger;

    public Patient360Reader(
        ApplicationDbContext dbContext,
        IEntityCitationReader citationReader,
        ILogger<Patient360Reader> logger)
    {
        _dbContext = dbContext;
        _citationReader = citationReader;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<Patient360Response?> GetPatient360Async(
        Guid patientId,
        CancellationToken cancellationToken = default)
    {
        // Load patient
        var patient = await _dbContext.ErdPatients
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == patientId && !p.IsDeleted, cancellationToken);

        if (patient == null)
        {
            _logger.LogDebug("Patient not found: {PatientId}", patientId);
            return null;
        }

        // Get only grounded entities (those with at least one citation)
        var groundedEntities = await _citationReader.GetGroundedEntitiesForPatientAsync(
            patientId, 
            cancellationToken);

        _logger.LogDebug(
            "Patient 360 for {PatientId}: {GroundedCount} grounded entities",
            patientId, groundedEntities.Count);

        // Load documents for this patient
        var documents = await _dbContext.Documents
            .AsNoTracking()
            .Where(d => d.PatientId == patientId && !d.IsDeleted)
            .OrderByDescending(d => d.UploadedAt)
            .ToListAsync(cancellationToken);

        // Count grounded entities per document
        var documentDtos = documents.Select(d => new Patient360DocumentDto
        {
            Id = d.Id,
            OriginalName = d.OriginalName,
            Status = d.Status,
            UploadedAt = d.UploadedAt,
            GroundedEntityCount = groundedEntities.Count(e => 
                e.Citations.Any(c => c.DocumentId == d.Id))
        }).ToList();

        return new Patient360Response
        {
            PatientId = patient.Id,
            Mrn = patient.Mrn,
            Name = patient.Name,
            Dob = patient.Dob,
            Address = patient.Address,
            Contact = patient.Contact,
            Entities = groundedEntities,
            Documents = documentDtos,
            GeneratedAt = DateTime.UtcNow
        };
    }
}
