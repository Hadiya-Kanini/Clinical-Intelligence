using System.ComponentModel.DataAnnotations;

namespace ClinicalIntelligence.Api.Domain.Models;

/// <summary>
/// Billing code catalog item for ICD/CPT code reference.
/// Maps to the billing_code_catalog_items table per ERD specification.
/// </summary>
public sealed class BillingCodeCatalogItem
{
    /// <summary>
    /// The billing code (ICD-10, CPT, etc.) - primary key.
    /// </summary>
    [Key]
    [MaxLength(20)]
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Type of code: ICD-10, CPT, HCPCS, etc.
    /// </summary>
    [Required]
    [MaxLength(20)]
    public string CodeType { get; set; } = string.Empty;

    /// <summary>
    /// Human-readable description of the code.
    /// </summary>
    [Required]
    [MaxLength(500)]
    public string Description { get; set; } = string.Empty;

    // Navigation property
    public ICollection<CodeSuggestion> CodeSuggestions { get; set; } = new List<CodeSuggestion>();
}
