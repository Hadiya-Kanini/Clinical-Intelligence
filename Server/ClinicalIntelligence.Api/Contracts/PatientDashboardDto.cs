namespace ClinicalIntelligence.Api.Contracts;

/// <summary>
/// Patient dashboard response DTO
/// </summary>
public class PatientDashboardDto
{
    public Guid Id { get; set; }
    public string? Mrn { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string FullName => $"{FirstName} {LastName}".Trim();
    public DateTime? DateOfBirth { get; set; }
    public string? Gender { get; set; }
    public string? Phone { get; set; }
    public int DocumentCount { get; set; }
    public DateTime? LastDocumentUploadedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

/// <summary>
/// Patient dashboard response with pagination
/// </summary>
public class PatientDashboardResponse
{
    public List<PatientDashboardDto> Patients { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
}
