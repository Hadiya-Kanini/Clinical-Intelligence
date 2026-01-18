using System.Threading.Tasks;
using ClinicalIntelligence.Api.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace ClinicalIntelligence.Api.Services.ExtractedEntities;

/// <summary>
/// Interface for database context needed by DbExtractedEntityWriter.
/// Allows dependency injection of test-specific contexts.
/// </summary>
public interface IExtractedEntityDbContext
{
    DbSet<ExtractedEntity> ExtractedEntities { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
