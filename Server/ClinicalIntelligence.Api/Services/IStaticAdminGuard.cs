namespace ClinicalIntelligence.Api.Services;

/// <summary>
/// Service interface for protecting the static admin account from deletion or deactivation.
/// Implements FR-010c: Static admin cannot be deleted or deactivated.
/// </summary>
public interface IStaticAdminGuard
{
    /// <summary>
    /// Validates that the specified user can be deleted.
    /// Throws StaticAdminProtectionException if the user is the static admin.
    /// </summary>
    /// <param name="userId">The user ID to validate.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Task that completes if deletion is allowed.</returns>
    Task ValidateCanDeleteAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates that the specified user's status can be changed.
    /// Throws StaticAdminProtectionException if the user is the static admin and the new status is not Active.
    /// </summary>
    /// <param name="userId">The user ID to validate.</param>
    /// <param name="newStatus">The proposed new status.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Task that completes if status change is allowed.</returns>
    Task ValidateCanChangeStatusAsync(Guid userId, string newStatus, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if the specified user is the static admin account.
    /// </summary>
    /// <param name="userId">The user ID to check.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if the user is the static admin, false otherwise.</returns>
    Task<bool> IsStaticAdminAsync(Guid userId, CancellationToken cancellationToken = default);
}
