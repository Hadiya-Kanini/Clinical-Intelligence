using ClinicalIntelligence.Api.Data;
using ClinicalIntelligence.Api.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace ClinicalIntelligence.Api.Services.PatientMatching;

/// <summary>
/// EF Core implementation of patient matching with MRN-first, name+DOB fallback, and create-new behavior.
/// Implements FR-050: Link multiple documents to the same patient via MRN or name+DOB matching.
/// </summary>
public sealed class PatientMatcher : IPatientMatcher
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<PatientMatcher> _logger;

    public PatientMatcher(ApplicationDbContext dbContext, ILogger<PatientMatcher> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<PatientMatchResult> FindOrCreatePatientAsync(
        PatientMatchInput input,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(input);

        // Normalize input values
        var normalizedMrn = PatientIdentityNormalizer.NormalizeMrn(input.Mrn);
        var normalizedName = PatientIdentityNormalizer.NormalizeName(input.Name);
        var parsedDob = PatientIdentityNormalizer.ParseDob(input.DobString);

        _logger.LogDebug(
            "Attempting patient match. NormalizedMRN: {Mrn}, NormalizedName: {Name}, ParsedDOB: {Dob}",
            normalizedMrn ?? "(null)",
            normalizedName ?? "(null)",
            parsedDob?.ToString() ?? "(null)");

        // 1. Primary match: MRN
        if (!string.IsNullOrEmpty(normalizedMrn))
        {
            var mrnMatch = await FindByMrnAsync(normalizedMrn, cancellationToken);
            if (mrnMatch != null)
            {
                _logger.LogInformation(
                    "Patient matched by MRN. PatientId: {PatientId}, MRN: {Mrn}",
                    mrnMatch.Id, mrnMatch.Mrn);

                return new PatientMatchResult
                {
                    Patient = mrnMatch,
                    WasCreated = false,
                    MatchType = PatientMatchType.MrnMatch
                };
            }
        }

        // 2. Fallback match: Name + DOB (both required for fallback)
        if (!string.IsNullOrEmpty(normalizedName) && parsedDob.HasValue)
        {
            var nameDobMatch = await FindByNameAndDobAsync(normalizedName, parsedDob.Value, cancellationToken);
            if (nameDobMatch != null)
            {
                _logger.LogInformation(
                    "Patient matched by Name+DOB. PatientId: {PatientId}, Name: {Name}, DOB: {Dob}",
                    nameDobMatch.Id, nameDobMatch.Name, nameDobMatch.Dob);

                return new PatientMatchResult
                {
                    Patient = nameDobMatch,
                    WasCreated = false,
                    MatchType = PatientMatchType.NameDobMatch
                };
            }
        }

        // 3. No match found - create new patient
        var newPatient = await CreatePatientAsync(input, normalizedMrn, normalizedName, parsedDob, cancellationToken);

        _logger.LogInformation(
            "Created new patient. PatientId: {PatientId}, MRN: {Mrn}",
            newPatient.Id, newPatient.Mrn);

        return new PatientMatchResult
        {
            Patient = newPatient,
            WasCreated = true,
            MatchType = PatientMatchType.Created
        };
    }

    private async Task<ErdPatient?> FindByMrnAsync(string normalizedMrn, CancellationToken cancellationToken)
    {
        // Query patients and normalize MRN for comparison
        // We need to compare normalized values, so we fetch candidates and filter in memory
        // For better performance with large datasets, consider storing normalized MRN in DB
        var patients = await _dbContext.ErdPatients
            .Where(p => !p.IsDeleted)
            .ToListAsync(cancellationToken);

        return patients.FirstOrDefault(p =>
            PatientIdentityNormalizer.NormalizeMrn(p.Mrn) == normalizedMrn);
    }

    private async Task<ErdPatient?> FindByNameAndDobAsync(
        string normalizedName,
        DateOnly dob,
        CancellationToken cancellationToken)
    {
        // First filter by DOB in the database, then normalize name in memory
        var candidates = await _dbContext.ErdPatients
            .Where(p => !p.IsDeleted && p.Dob == dob)
            .ToListAsync(cancellationToken);

        return candidates.FirstOrDefault(p =>
            PatientIdentityNormalizer.NormalizeName(p.Name) == normalizedName);
    }

    private async Task<ErdPatient> CreatePatientAsync(
        PatientMatchInput input,
        string? normalizedMrn,
        string? normalizedName,
        DateOnly? parsedDob,
        CancellationToken cancellationToken)
    {
        // Use extracted MRN if available, otherwise generate synthetic MRN
        var mrn = !string.IsNullOrEmpty(input.Mrn)
            ? input.Mrn.Trim()
            : PatientIdentityNormalizer.GenerateSyntheticMrn();

        // Use original name (not normalized) for storage, or placeholder if missing
        var name = !string.IsNullOrWhiteSpace(input.Name)
            ? input.Name.Trim()
            : "Unknown";

        var patient = new ErdPatient
        {
            Id = Guid.NewGuid(),
            Mrn = mrn,
            Name = name,
            Dob = parsedDob,
            Address = input.Address?.Trim(),
            Contact = input.Contact?.Trim(),
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _dbContext.ErdPatients.Add(patient);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return patient;
    }
}
