using ClinicalIntelligence.Api.Data;
using ClinicalIntelligence.Api.Domain.Models;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace ClinicalIntelligence.Api.Services.PatientMatching;

/// <summary>
/// Transactional implementation that uses IPatientMatcher and updates Document, DocumentBatch,
/// and patient-scoped records (ExtractedEntity, CodeSuggestion, ErdConflict).
/// </summary>
public sealed class PatientLinkingService : IPatientLinkingService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IPatientMatcher _patientMatcher;
    private readonly ILogger<PatientLinkingService> _logger;

    public PatientLinkingService(
        ApplicationDbContext dbContext,
        IPatientMatcher patientMatcher,
        ILogger<PatientLinkingService> logger)
    {
        _dbContext = dbContext;
        _patientMatcher = patientMatcher;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<PatientLinkageResult> LinkDocumentToPatientAsync(
        PatientLinkageInput input,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(input);

        _logger.LogDebug(
            "Starting patient linkage for DocumentId: {DocumentId}",
            input.DocumentId);

        // Use a transaction to ensure atomicity
        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            // 1. Load the document
            var document = await _dbContext.Documents
                .Include(d => d.DocumentBatch)
                .FirstOrDefaultAsync(d => d.Id == input.DocumentId, cancellationToken);

            if (document == null)
            {
                throw new InvalidOperationException($"Document not found: {input.DocumentId}");
            }

            var previousPatientId = document.PatientId;

            // 2. Determine target patient via IPatientMatcher
            var matchInput = new PatientMatchInput
            {
                Mrn = input.Mrn,
                Name = input.Name,
                DobString = input.DobString,
                Address = input.Address,
                Contact = input.Contact
            };

            var matchResult = await _patientMatcher.FindOrCreatePatientAsync(matchInput, cancellationToken);
            var targetPatientId = matchResult.Patient.Id;

            // 3. Check if already linked to target patient (no-op)
            if (document.PatientId == targetPatientId)
            {
                _logger.LogDebug(
                    "Document {DocumentId} already linked to patient {PatientId}",
                    input.DocumentId, targetPatientId);

                await transaction.CommitAsync(cancellationToken);

                return new PatientLinkageResult
                {
                    PatientId = targetPatientId,
                    PatientWasCreated = matchResult.WasCreated,
                    WasReassigned = false,
                    RelatedRecordsUpdated = 0,
                    MrnConflictDetected = false
                };
            }

            // 4. Check for MRN conflict (name+DOB match but different MRN)
            bool mrnConflictDetected = false;
            if (matchResult.MatchType == PatientMatchType.NameDobMatch && !string.IsNullOrWhiteSpace(input.Mrn))
            {
                var normalizedInputMrn = PatientIdentityNormalizer.NormalizeMrn(input.Mrn);
                var normalizedExistingMrn = PatientIdentityNormalizer.NormalizeMrn(matchResult.Patient.Mrn);

                if (normalizedInputMrn != normalizedExistingMrn && 
                    !matchResult.Patient.Mrn.StartsWith("AUTO-", StringComparison.OrdinalIgnoreCase))
                {
                    mrnConflictDetected = true;
                    await RecordMrnConflictAsync(
                        targetPatientId,
                        matchResult.Patient.Mrn,
                        input.Mrn,
                        cancellationToken);
                }
            }

            // 5. Update document patient linkage
            document.PatientId = targetPatientId;

            // 6. Update batch if present
            if (document.DocumentBatchId.HasValue)
            {
                var batch = document.DocumentBatch;
                if (batch != null)
                {
                    batch.PatientId = targetPatientId;

                    // Update all documents in the batch to point to the same patient
                    var batchDocuments = await _dbContext.Documents
                        .Where(d => d.DocumentBatchId == document.DocumentBatchId && d.Id != document.Id)
                        .ToListAsync(cancellationToken);

                    foreach (var batchDoc in batchDocuments)
                    {
                        batchDoc.PatientId = targetPatientId;
                    }
                }
            }

            // 7. Update patient-scoped records for this document
            int relatedRecordsUpdated = 0;

            // Update ExtractedEntity.PatientId
            var entities = await _dbContext.ExtractedEntities
                .Where(e => e.DocumentId == input.DocumentId)
                .ToListAsync(cancellationToken);

            foreach (var entity in entities)
            {
                entity.PatientId = targetPatientId;
                relatedRecordsUpdated++;
            }

            // Update CodeSuggestion.PatientId for entities linked to this document
            var entityIds = entities.Select(e => e.Id).ToList();
            if (entityIds.Count > 0)
            {
                var codeSuggestions = await _dbContext.CodeSuggestions
                    .Where(cs => cs.ExtractedEntityId.HasValue && entityIds.Contains(cs.ExtractedEntityId.Value))
                    .ToListAsync(cancellationToken);

                foreach (var suggestion in codeSuggestions)
                {
                    suggestion.PatientId = targetPatientId;
                    relatedRecordsUpdated++;
                }
            }

            // Note: ErdConflict records are patient-scoped, not document-scoped
            // New conflicts are created at patient level, existing conflicts remain with original patient

            await _dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            _logger.LogInformation(
                "Linked document {DocumentId} to patient {PatientId}. WasCreated: {WasCreated}, WasReassigned: {WasReassigned}, RelatedRecords: {RelatedRecords}",
                input.DocumentId,
                targetPatientId,
                matchResult.WasCreated,
                previousPatientId.HasValue && previousPatientId != targetPatientId,
                relatedRecordsUpdated);

            return new PatientLinkageResult
            {
                PatientId = targetPatientId,
                PatientWasCreated = matchResult.WasCreated,
                WasReassigned = previousPatientId.HasValue && previousPatientId != targetPatientId,
                PreviousPatientId = previousPatientId != targetPatientId ? previousPatientId : null,
                RelatedRecordsUpdated = relatedRecordsUpdated,
                MrnConflictDetected = mrnConflictDetected
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to link document {DocumentId} to patient", input.DocumentId);
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    private async Task RecordMrnConflictAsync(
        Guid patientId,
        string existingMrn,
        string extractedMrn,
        CancellationToken cancellationToken)
    {
        var conflictingValues = JsonSerializer.Serialize(new[]
        {
            new { value = existingMrn, source = "existing_patient" },
            new { value = extractedMrn, source = "extracted_document" }
        });

        var conflict = new ErdConflict
        {
            Id = Guid.NewGuid(),
            PatientId = patientId,
            Field = "mrn",
            EntityCategory = "Demographics",
            ConflictingValues = conflictingValues,
            Severity = "High",
            Status = "Pending",
            DetectedAt = DateTime.UtcNow
        };

        _dbContext.ErdConflicts.Add(conflict);

        _logger.LogWarning(
            "MRN conflict detected for patient {PatientId}. Existing: {ExistingMrn}, Extracted: {ExtractedMrn}",
            patientId, existingMrn, extractedMrn);
    }
}
