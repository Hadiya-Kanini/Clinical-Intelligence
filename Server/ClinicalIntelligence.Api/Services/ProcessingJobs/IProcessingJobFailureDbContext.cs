using System.Threading.Tasks;
using ClinicalIntelligence.Api.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace ClinicalIntelligence.Api.Services.ProcessingJobs;

/// <summary>
/// Interface for database context needed by ProcessingJobFailureRecorder.
/// Allows dependency injection of test-specific contexts.
/// </summary>
public interface IProcessingJobFailureDbContext
{
    DbSet<ProcessingJob> ProcessingJobs { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
