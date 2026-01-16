using System.Text.Json.Serialization;

namespace ClinicalIntelligence.Api.Contracts.Admin;

/// <summary>
/// Query parameters for admin users list endpoint.
/// Supports search, sorting, and pagination.
/// </summary>
public sealed class AdminUsersListQuery
{
    /// <summary>
    /// Search query for partial match on name or email.
    /// </summary>
    [JsonPropertyName("q")]
    public string? Q { get; set; }

    /// <summary>
    /// Column to sort by. Allowed values: name, email, role, status.
    /// Default: name.
    /// </summary>
    [JsonPropertyName("sortBy")]
    public string SortBy { get; set; } = "name";

    /// <summary>
    /// Sort direction. Allowed values: asc, desc.
    /// Default: asc.
    /// </summary>
    [JsonPropertyName("sortDir")]
    public string SortDir { get; set; } = "asc";

    /// <summary>
    /// Page number (1-based).
    /// Default: 1.
    /// </summary>
    [JsonPropertyName("page")]
    public int Page { get; set; } = 1;

    /// <summary>
    /// Number of items per page.
    /// Default: 20, Max: 100.
    /// </summary>
    [JsonPropertyName("pageSize")]
    public int PageSize { get; set; } = 20;

    /// <summary>
    /// Allowed sort columns (whitelist for security).
    /// </summary>
    public static readonly string[] AllowedSortColumns = { "name", "email", "role", "status" };

    /// <summary>
    /// Allowed sort directions.
    /// </summary>
    public static readonly string[] AllowedSortDirections = { "asc", "desc" };

    /// <summary>
    /// Maximum allowed page size.
    /// </summary>
    public const int MaxPageSize = 100;

    /// <summary>
    /// Validates the query parameters and returns validation errors.
    /// </summary>
    public List<string> Validate()
    {
        var errors = new List<string>();

        if (Page < 1)
        {
            errors.Add("page:must_be_positive");
        }

        if (PageSize < 1)
        {
            errors.Add("pageSize:must_be_positive");
        }
        else if (PageSize > MaxPageSize)
        {
            errors.Add($"pageSize:max_{MaxPageSize}");
        }

        if (!string.IsNullOrEmpty(SortBy) && 
            !AllowedSortColumns.Contains(SortBy, StringComparer.OrdinalIgnoreCase))
        {
            errors.Add($"sortBy:invalid_value");
        }

        if (!string.IsNullOrEmpty(SortDir) && 
            !AllowedSortDirections.Contains(SortDir, StringComparer.OrdinalIgnoreCase))
        {
            errors.Add($"sortDir:invalid_value");
        }

        return errors;
    }

    /// <summary>
    /// Normalizes the query parameters to safe defaults.
    /// </summary>
    public void Normalize()
    {
        if (Page < 1) Page = 1;
        if (PageSize < 1) PageSize = 20;
        if (PageSize > MaxPageSize) PageSize = MaxPageSize;

        SortBy = AllowedSortColumns.Contains(SortBy, StringComparer.OrdinalIgnoreCase) 
            ? SortBy.ToLowerInvariant() 
            : "name";

        SortDir = AllowedSortDirections.Contains(SortDir, StringComparer.OrdinalIgnoreCase) 
            ? SortDir.ToLowerInvariant() 
            : "asc";
    }
}
