using ClinicalIntelligence.Api.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ClinicalIntelligence.Api.Services.Processing;

/// <summary>
/// EF-backed implementation that selects and orders documents for patient-level merge.
/// Documents are ordered by UploadedAt ASC, then by Id ASC for deterministic ordering.
/// </summary>
public sealed class PatientDocumentMergePlanner : IPatientDocumentMergePlanner
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<PatientDocumentMergePlanner> _logger;

    public PatientDocumentMergePlanner(
        ApplicationDbContext dbContext,
        ILogger<PatientDocumentMergePlanner> logger)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public async Task<PatientMergePlan> PlanMergeAsync(
        Guid patientId,
        CancellationToken cancellationToken = default)
    {
        var patientExists = await _dbContext.ErdPatients
            .AnyAsync(p => p.Id == patientId, cancellationToken);

        if (!patientExists)
        {
            _logger.LogWarning("Patient not found for merge planning");
            return PatientMergePlan.Invalid("Patient not found.");
        }

        var documentIds = await _dbContext.Documents
            .Where(d => d.PatientId == patientId)
            .OrderBy(d => d.UploadedAt)
            .ThenBy(d => d.Id)
            .Select(d => d.Id)
            .ToListAsync(cancellationToken);

        if (documentIds.Count == 0)
        {
            _logger.LogDebug("No documents found for patient merge");
            return PatientMergePlan.Invalid("No documents found for patient.");
        }

        _logger.LogDebug("Merge plan created with {DocumentCount} documents", documentIds.Count);
        return PatientMergePlan.Valid(patientId, documentIds);
    }

    /// <inheritdoc />
    public async Task<PatientMergePlan> PlanMergeAsync(
        Guid patientId,
        IEnumerable<Guid> documentIds,
        CancellationToken cancellationToken = default)
    {
        if (documentIds == null || !documentIds.Any())
        {
            return PatientMergePlan.Invalid("No document IDs provided.");
        }

        var requestedIds = documentIds.ToHashSet();

        var patientExists = await _dbContext.ErdPatients
            .AnyAsync(p => p.Id == patientId, cancellationToken);

        if (!patientExists)
        {
            _logger.LogWarning("Patient not found for merge planning");
            return PatientMergePlan.Invalid("Patient not found.");
        }

        var validDocuments = await _dbContext.Documents
            .Where(d => d.PatientId == patientId && requestedIds.Contains(d.Id))
            .OrderBy(d => d.UploadedAt)
            .ThenBy(d => d.Id)
            .Select(d => d.Id)
            .ToListAsync(cancellationToken);

        if (validDocuments.Count == 0)
        {
            _logger.LogWarning("No valid documents found for patient merge");
            return PatientMergePlan.Invalid("No valid documents found for patient.");
        }

        var invalidIds = requestedIds.Except(validDocuments).ToList();
        if (invalidIds.Count > 0)
        {
            _logger.LogWarning(
                "Some requested documents do not belong to patient or do not exist: {Count}",
                invalidIds.Count);
        }

        _logger.LogDebug("Merge plan created with {DocumentCount} documents", validDocuments.Count);
        return PatientMergePlan.Valid(patientId, validDocuments);
    }
}
