using ClinicalIntelligence.Api.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ClinicalIntelligence.Api.Services.Processing;

/// <summary>
/// Implementation of patient identity matching using MRN or name+DOB.
/// MRN matching has priority over name+DOB.
/// </summary>
public sealed class PatientIdentityMatcher : IPatientIdentityMatcher
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<PatientIdentityMatcher> _logger;

    public PatientIdentityMatcher(
        ApplicationDbContext dbContext,
        ILogger<PatientIdentityMatcher> logger)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public async Task<PatientMatchResult> MatchByMrnAsync(
        string mrn,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(mrn))
        {
            return PatientMatchResult.NotFound("MRN is required for matching.");
        }

        var normalizedMrn = NormalizeMrn(mrn);

        var patient = await _dbContext.ErdPatients
            .Where(p => p.Mrn.ToUpper() == normalizedMrn)
            .Select(p => new { p.Id })
            .FirstOrDefaultAsync(cancellationToken);

        if (patient == null)
        {
            _logger.LogDebug("No patient found for MRN match");
            return PatientMatchResult.NotFound();
        }

        _logger.LogDebug("Patient matched by MRN");
        return PatientMatchResult.Found(patient.Id, PatientMatchMethod.Mrn);
    }

    /// <inheritdoc />
    public async Task<PatientMatchResult> MatchByNameAndDobAsync(
        string name,
        DateOnly dateOfBirth,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return PatientMatchResult.NotFound("Name is required for matching.");
        }

        var normalizedName = NormalizeName(name);

        var candidates = await _dbContext.ErdPatients
            .Where(p => p.Dob == dateOfBirth)
            .Select(p => new { p.Id, p.Name })
            .ToListAsync(cancellationToken);

        var matches = candidates
            .Where(c => NormalizeName(c.Name) == normalizedName)
            .ToList();

        if (matches.Count == 0)
        {
            _logger.LogDebug("No patient found for name+DOB match");
            return PatientMatchResult.NotFound();
        }

        if (matches.Count > 1)
        {
            _logger.LogWarning("Ambiguous patient match: multiple patients with same name+DOB");
            return PatientMatchResult.Ambiguous(
                $"Multiple patients ({matches.Count}) match the provided name and date of birth.");
        }

        _logger.LogDebug("Patient matched by name+DOB");
        return PatientMatchResult.Found(matches[0].Id, PatientMatchMethod.NameDob);
    }

    /// <inheritdoc />
    public async Task<PatientMatchResult> MatchAsync(
        PatientIdentifiers identifiers,
        CancellationToken cancellationToken = default)
    {
        if (identifiers == null)
        {
            return PatientMatchResult.NotFound("No identifiers provided.");
        }

        if (!string.IsNullOrWhiteSpace(identifiers.Mrn))
        {
            var mrnResult = await MatchByMrnAsync(identifiers.Mrn, cancellationToken);
            if (mrnResult.IsMatch)
            {
                return mrnResult;
            }
        }

        if (!string.IsNullOrWhiteSpace(identifiers.Name) && identifiers.DateOfBirth.HasValue)
        {
            return await MatchByNameAndDobAsync(
                identifiers.Name,
                identifiers.DateOfBirth.Value,
                cancellationToken);
        }

        return PatientMatchResult.NotFound("Insufficient identifiers for matching.");
    }

    private static string NormalizeMrn(string mrn)
    {
        return mrn.Trim().ToUpperInvariant();
    }

    private static string NormalizeName(string name)
    {
        var normalized = string.Join(" ", name.Trim().Split(
            new[] { ' ', '\t' },
            StringSplitOptions.RemoveEmptyEntries));
        return normalized.ToUpperInvariant();
    }
}
