namespace ClinicalIntelligence.Api.Configuration;

/// <summary>
/// Configuration options for document storage.
/// </summary>
public class DocumentStorageOptions
{
    public const string SectionName = "DocumentStorage";

    /// <summary>
    /// Base path for document storage. Default: ./storage/documents
    /// </summary>
    public string BasePath { get; set; } = "./storage/documents";

    /// <summary>
    /// Temporary upload path for processing. Default: ./storage/temp
    /// </summary>
    public string TempPath { get; set; } = "./storage/temp";

    /// <summary>
    /// Default tenant ID for Phase 1 single-tenant deployment.
    /// </summary>
    public string DefaultTenantId { get; set; } = "default";

    /// <summary>
    /// Maximum storage size in bytes (0 = unlimited).
    /// </summary>
    public long MaxStorageBytes { get; set; } = 0;

    /// <summary>
    /// Creates options from configuration.
    /// </summary>
    public static DocumentStorageOptions FromConfiguration(IConfiguration configuration)
    {
        var options = new DocumentStorageOptions();
        configuration.GetSection(SectionName).Bind(options);

        // Check environment variables for overrides
        var basePathEnv = Environment.GetEnvironmentVariable("DOCUMENT_STORAGE_BASE_PATH");
        if (!string.IsNullOrWhiteSpace(basePathEnv))
        {
            options.BasePath = basePathEnv;
        }

        var tempPathEnv = Environment.GetEnvironmentVariable("DOCUMENT_STORAGE_TEMP_PATH");
        if (!string.IsNullOrWhiteSpace(tempPathEnv))
        {
            options.TempPath = tempPathEnv;
        }

        var tenantIdEnv = Environment.GetEnvironmentVariable("DOCUMENT_STORAGE_TENANT_ID");
        if (!string.IsNullOrWhiteSpace(tenantIdEnv))
        {
            options.DefaultTenantId = tenantIdEnv;
        }

        return options;
    }
}
