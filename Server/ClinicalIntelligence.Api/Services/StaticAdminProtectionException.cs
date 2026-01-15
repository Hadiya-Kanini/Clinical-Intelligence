namespace ClinicalIntelligence.Api.Services;

/// <summary>
/// Exception thrown when an operation attempts to delete or deactivate the static admin account.
/// </summary>
public sealed class StaticAdminProtectionException : InvalidOperationException
{
    public string ErrorCode { get; }

    public StaticAdminProtectionException(string errorCode, string message)
        : base(message)
    {
        ErrorCode = errorCode;
    }

    public static StaticAdminProtectionException CannotDelete() =>
        new("static_admin_protected", "The static admin account cannot be deleted.");

    public static StaticAdminProtectionException CannotDeactivate() =>
        new("static_admin_protected", "The static admin account cannot be deactivated.");

    public static StaticAdminProtectionException CannotChangeStatus(string newStatus) =>
        new("static_admin_protected", $"The static admin account status cannot be changed to '{newStatus}'.");
}
