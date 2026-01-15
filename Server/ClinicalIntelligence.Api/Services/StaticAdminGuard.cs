using ClinicalIntelligence.Api.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ClinicalIntelligence.Api.Services;

/// <summary>
/// Implementation of IStaticAdminGuard that protects the static admin account from deletion or deactivation.
/// Implements FR-010c: Static admin cannot be deleted or deactivated.
/// </summary>
public sealed class StaticAdminGuard : IStaticAdminGuard
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<StaticAdminGuard> _logger;

    public StaticAdminGuard(ApplicationDbContext dbContext, ILogger<StaticAdminGuard> logger)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task ValidateCanDeleteAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        if (await IsStaticAdminAsync(userId, cancellationToken))
        {
            _logger.LogWarning("Attempted to delete static admin account {UserId}", userId);
            throw StaticAdminProtectionException.CannotDelete();
        }
    }

    public async Task ValidateCanChangeStatusAsync(Guid userId, string newStatus, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(newStatus))
        {
            throw new ArgumentException("Status cannot be null or empty.", nameof(newStatus));
        }

        // Only block status changes away from Active for static admin
        if (!string.Equals(newStatus, "Active", StringComparison.OrdinalIgnoreCase))
        {
            if (await IsStaticAdminAsync(userId, cancellationToken))
            {
                _logger.LogWarning("Attempted to change static admin account {UserId} status to {NewStatus}", userId, newStatus);
                throw StaticAdminProtectionException.CannotChangeStatus(newStatus);
            }
        }
    }

    public async Task<bool> IsStaticAdminAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Users
            .IgnoreQueryFilters()
            .AnyAsync(u => u.Id == userId && u.IsStaticAdmin, cancellationToken);
    }
}
