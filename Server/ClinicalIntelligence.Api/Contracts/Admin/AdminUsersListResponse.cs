using System.Text.Json.Serialization;

namespace ClinicalIntelligence.Api.Contracts.Admin;

/// <summary>
/// Response contract for admin users list endpoint.
/// Contains paginated user items and metadata.
/// </summary>
public sealed class AdminUsersListResponse
{
    /// <summary>
    /// List of user items for the current page.
    /// </summary>
    [JsonPropertyName("items")]
    public List<AdminUserItem> Items { get; set; } = new();

    /// <summary>
    /// Current page number (1-based).
    /// </summary>
    [JsonPropertyName("page")]
    public int Page { get; set; }

    /// <summary>
    /// Number of items per page.
    /// </summary>
    [JsonPropertyName("pageSize")]
    public int PageSize { get; set; }

    /// <summary>
    /// Total number of users matching the query (before pagination).
    /// </summary>
    [JsonPropertyName("total")]
    public int Total { get; set; }
}

/// <summary>
/// Individual user item in the admin users list.
/// </summary>
public sealed class AdminUserItem
{
    /// <summary>
    /// User ID.
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// User display name.
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// User email address.
    /// </summary>
    [JsonPropertyName("email")]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// User role (admin or standard).
    /// </summary>
    [JsonPropertyName("role")]
    public string Role { get; set; } = string.Empty;

    /// <summary>
    /// User status (active, inactive, locked).
    /// </summary>
    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;
}
