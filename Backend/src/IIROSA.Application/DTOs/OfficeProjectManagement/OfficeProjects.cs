using Framework.Core.SharedServices.Dto;

namespace IIROSA.Application.DTOs.OfficeProjectManagement;

// Naming note: DTO properties use clean names (CountryId, RegionId, …) so the camelCase wire
// contract (verified: `countryId`, not `fK_CountryId`) matches what every client naturally sends.
// The entity keeps its legacy `FK_`-prefixed columns; OfficeProjectProfile bridges the two with
// explicit ForMember maps, so no database migration is involved.

/// <summary>
/// OfficeProject list item for grid display (UC-OFP-01)
/// </summary>
public class OfficeProjectListDto
{
    public Guid Id { get; set; }
    public string ProjectName { get; set; } = string.Empty;
    public string? ProjectType { get; set; }
    public DateTime ProjectDate { get; set; }
    public string? Region { get; set; }
    public string? Center { get; set; }
    public string? Village { get; set; }
    public decimal? ProjectCostEGP { get; set; }
    public decimal? ProjectCostSAR { get; set; }
    public int? BeneficiariesCount { get; set; }
    public string? DonorName { get; set; }
    public bool IsFinished { get; set; }
    public DateTime? ProjectEndDate { get; set; }
    public string? AssignedCharity { get; set; }
    public string? CountryName { get; set; }
}

/// <summary>
/// OfficeProject detail with all information (UC-OFP-04)
/// </summary>
public class OfficeProjectDetailDto
{
    public Guid Id { get; set; }

    // Basic Information
    public string ProjectName { get; set; } = string.Empty;
    public string? ProjectHint { get; set; }
    public DateTime ProjectDate { get; set; }
    public DateTime? ProjectEndDate { get; set; }

    // Classification
    public int? OfficeProjectTypeId { get; set; }
    public string? OfficeProjectTypeName { get; set; }

    // Location
    public int? CountryId { get; set; }
    public string? CountryName { get; set; }
    public int? RegionId { get; set; }
    public string? RegionName { get; set; }
    public int? CenterId { get; set; }
    public string? CenterName { get; set; }
    public string? VillageName { get; set; }

    // Financial Information
    public decimal? ProjectCostEGP { get; set; }
    public decimal? ProjectCostSAR { get; set; }
    public string? DonorName { get; set; }

    // Beneficiaries
    public int? BeneficiariesCount { get; set; }
    public string? BeneficiariesType { get; set; }

    // Charity Assignment
    public Guid? CharityId { get; set; }
    public string? CharityName { get; set; }

    // Documents
    public Guid? AttachedFileId { get; set; }
    public string? AttachedFileName { get; set; }
    public Guid? ProjectReportFileId { get; set; }
    public string? ProjectReportFileName { get; set; }

    // Attachments (for display)
    public List<AttachmentDto>? Document_Attach { get; set; }
    public List<AttachmentDto>? Report_Attach { get; set; }

    // Status
    public bool IsFinished { get; set; }

    // Additional Notes
    public string? Notes { get; set; }

    // Audit
    public DateTime CreatedOn { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedOn { get; set; }
    public string? UpdatedBy { get; set; }
}

/// <summary>
/// Create office project DTO (UC-OFP-03)
/// </summary>
public class CreateOfficeProjectDto
{
    // Required fields
    public string ProjectName { get; set; } = string.Empty;
    public DateTime ProjectDate { get; set; }
    public int OfficeProjectTypeId { get; set; }

    // Optional fields
    public string? ProjectHint { get; set; }
    public DateTime? ProjectEndDate { get; set; }
    public int? CountryId { get; set; }
    public int? RegionId { get; set; }
    public int? CenterId { get; set; }
    public string? VillageName { get; set; }
    public decimal? ProjectCostEGP { get; set; }
    public decimal? ProjectCostSAR { get; set; }
    public string? DonorName { get; set; }
    public int? BeneficiariesCount { get; set; }
    public string? BeneficiariesType { get; set; }
    public Guid? CharityId { get; set; }

    // Attachments - Use List<AttachmentDto> for proper attachment handling
    public List<AttachmentDto>? Document_Attach { get; set; }
    public List<AttachmentDto>? Report_Attach { get; set; }

    public string? Notes { get; set; }
    public bool IsFinished { get; set; } = false;
}

/// <summary>
/// Update office project DTO (UC-OFP-04). Null means "leave unchanged" — the service patches
/// non-null values onto the loaded entity.
/// </summary>
public class UpdateOfficeProjectDto
{
    public string? ProjectName { get; set; }
    public string? ProjectHint { get; set; }
    public DateTime? ProjectDate { get; set; }
    public DateTime? ProjectEndDate { get; set; }
    public int? OfficeProjectTypeId { get; set; }
    public int? CountryId { get; set; }
    public int? RegionId { get; set; }
    public int? CenterId { get; set; }
    public string? VillageName { get; set; }
    public decimal? ProjectCostEGP { get; set; }
    public decimal? ProjectCostSAR { get; set; }
    public string? DonorName { get; set; }
    public int? BeneficiariesCount { get; set; }
    public string? BeneficiariesType { get; set; }
    public Guid? CharityId { get; set; }

    // Attachments - Use List<AttachmentDto> for proper attachment handling
    public List<AttachmentDto>? Document_Attach { get; set; }
    public List<AttachmentDto>? Report_Attach { get; set; }

    public string? Notes { get; set; }
    public bool? IsFinished { get; set; }
}

/// <summary>
/// Office project filter DTO for queries (UC-OFP-01, UC-OFP-06)
/// </summary>
public class OfficeProjectFilterDto
{
    public string? SearchText { get; set; }               // Search by project name
    public int? OfficeProjectTypeId { get; set; }         // Filter by project type
    public int? CountryId { get; set; }
    public int? RegionId { get; set; }
    public int? CenterId { get; set; }
    public Guid? CharityId { get; set; }                  // Filter by assigned charity
    public bool? IsFinished { get; set; }                 // Filter by completion status
    public string? DonorName { get; set; }                // Filter by donor
    public DateTime? StartDate { get; set; }              // Date range filter
    public DateTime? EndDate { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

/// <summary>
/// Register statistics for the band above the projects grid (UC-OFP-01).
/// Counts follow the caller's country scope — the same pin the list read applies —
/// so the band describes the caller's whole register, not the current search.
/// </summary>
public class ProjectStatisticsDto
{
    public int Total { get; set; }

    /// <summary>Projects marked finished (IsFinished, UC-7.9).</summary>
    public int Completed { get; set; }

    /// <summary>Projects whose start date (ProjectDate) falls in the current year.</summary>
    public int ThisYear { get; set; }

    /// <summary>Projects created since the first day of the current (UTC) month.</summary>
    public int AddedThisMonth { get; set; }
}

/// <summary>
/// Mark project as completed DTO (module completion tracking, feeds `#/office-development-projects/progress`)
/// </summary>
public class MarkProjectCompletedDto
{
    public bool IsFinished { get; set; } = true;
    public DateTime? ProjectEndDate { get; set; } = DateTime.Today;
}

/// <summary>
/// Paged result wrapper for office projects
/// </summary>
public class OfficeProjectPagedResult<T>
{
    public List<T> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling((decimal)TotalCount / PageSize);
}
