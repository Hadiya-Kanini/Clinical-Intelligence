using ClinicalIntelligence.Api.Contracts;
using ClinicalIntelligence.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace ClinicalIntelligence.Api.Services;

/// <summary>
/// Loads the patient aggregate from EF Core using patient-centric organization.
/// Efficiently retrieves all related clinical data in a single query pattern.
/// </summary>
public sealed class PatientAggregateService : IPatientAggregateService
{
    private readonly ApplicationDbContext _dbContext;

    public PatientAggregateService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <inheritdoc />
    public async Task<PatientAggregateResponse?> GetPatientAggregateAsync(Guid patientId, CancellationToken cancellationToken = default)
    {
        var patient = await _dbContext.Patients
            .AsNoTracking()
            .Include(p => p.Encounters)
            .Include(p => p.Observations)
            .Include(p => p.MedicationStatements)
            .Include(p => p.Conditions)
            .Include(p => p.Procedures)
            .FirstOrDefaultAsync(p => p.Id == patientId, cancellationToken);

        if (patient is null)
        {
            return null;
        }

        return new PatientAggregateResponse
        {
            PatientId = patient.Id,
            Demographics = MapToDemographics(patient),
            Encounters = MapToEncounters(patient.Encounters),
            Observations = MapToObservations(patient.Observations),
            Medications = MapToMedications(patient.MedicationStatements),
            Diagnoses = MapToDiagnoses(patient.Conditions),
            Procedures = MapToProcedures(patient.Procedures),
            RetrievedAt = DateTime.UtcNow
        };
    }

    private static PatientDemographicsDto MapToDemographics(Domain.Models.Patient patient)
    {
        return new PatientDemographicsDto
        {
            Id = patient.Id,
            Mrn = patient.Mrn,
            GivenName = patient.GivenName,
            FamilyName = patient.FamilyName,
            DateOfBirth = patient.DateOfBirth,
            Gender = patient.Gender,
            Address = patient.Address,
            Phone = patient.Phone,
            Email = patient.Email,
            IsActive = patient.IsActive
        };
    }

    private static IReadOnlyList<EncounterDto> MapToEncounters(ICollection<Domain.Models.Encounter> encounters)
    {
        return encounters
            .OrderByDescending(e => e.StartDate)
            .Select(e => new EncounterDto
            {
                Id = e.Id,
                Status = e.Status,
                Class = e.Class,
                Type = e.Type,
                StartDate = e.StartDate,
                EndDate = e.EndDate,
                ReasonCode = e.ReasonCode,
                Location = e.Location
            })
            .ToList();
    }

    private static IReadOnlyList<ObservationDto> MapToObservations(ICollection<Domain.Models.Observation> observations)
    {
        return observations
            .OrderByDescending(o => o.EffectiveDate)
            .Select(o => new ObservationDto
            {
                Id = o.Id,
                EncounterId = o.EncounterId,
                Status = o.Status,
                Category = o.Category,
                Code = o.Code,
                CodeDisplay = o.CodeDisplay,
                Value = o.Value,
                Unit = o.Unit,
                EffectiveDate = o.EffectiveDate,
                Interpretation = o.Interpretation,
                ReferenceRangeLow = o.ReferenceRangeLow,
                ReferenceRangeHigh = o.ReferenceRangeHigh
            })
            .ToList();
    }

    private static IReadOnlyList<MedicationDto> MapToMedications(ICollection<Domain.Models.MedicationStatement> medications)
    {
        return medications
            .OrderByDescending(m => m.EffectiveDate)
            .Select(m => new MedicationDto
            {
                Id = m.Id,
                Status = m.Status,
                MedicationCode = m.MedicationCode,
                MedicationName = m.MedicationName,
                Dosage = m.Dosage,
                Route = m.Route,
                Frequency = m.Frequency,
                EffectiveDate = m.EffectiveDate,
                EndDate = m.EndDate,
                ReasonCode = m.ReasonCode
            })
            .ToList();
    }

    private static IReadOnlyList<DiagnosisDto> MapToDiagnoses(ICollection<Domain.Models.Condition> conditions)
    {
        return conditions
            .OrderByDescending(c => c.OnsetDate)
            .Select(c => new DiagnosisDto
            {
                Id = c.Id,
                EncounterId = c.EncounterId,
                ClinicalStatus = c.ClinicalStatus,
                VerificationStatus = c.VerificationStatus,
                Category = c.Category,
                Code = c.Code,
                CodeDisplay = c.CodeDisplay,
                OnsetDate = c.OnsetDate,
                AbatementDate = c.AbatementDate,
                Severity = c.Severity
            })
            .ToList();
    }

    private static IReadOnlyList<ProcedureDto> MapToProcedures(ICollection<Domain.Models.Procedure> procedures)
    {
        return procedures
            .OrderByDescending(p => p.PerformedDate)
            .Select(p => new ProcedureDto
            {
                Id = p.Id,
                EncounterId = p.EncounterId,
                Status = p.Status,
                Code = p.Code,
                CodeDisplay = p.CodeDisplay,
                Category = p.Category,
                PerformedDate = p.PerformedDate,
                PerformedEndDate = p.PerformedEndDate,
                ReasonCode = p.ReasonCode,
                BodySite = p.BodySite,
                Outcome = p.Outcome
            })
            .ToList();
    }
}
