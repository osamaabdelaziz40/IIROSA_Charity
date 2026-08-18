using Framework.Core.SharedServices.Dto;

namespace IIROSA.Application.DTOs.OfficeProjectManagement;

/// <summary>
/// OfficeProject list item for grid display (UC-7.10)
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
/// OfficeProject detail with all information (UC-7.11)
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
    public int? FK_OfficeProjectTypeId { get; set; }
    public string? OfficeProjectTypeName { get; set; }

    // Location
    public int? FK_CountryId { get; set; }
    public string? CountryName { get; set; }
    public int? FK_RegionId { get; set; }
    public string? RegionName { get; set; }
    public int? FK_CenterId { get; set; }
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
    public Guid? FK_CharityId { get; set; }
    public string? CharityName { get; set; }

    // Documents
    public Guid? FK_AttachedFileId { get; set; }
    public string? AttachedFileName { get; set; }
    public Guid? FK_ProjectReportFileId { get; set; }
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
/// Create office project DTO (UC-7.1)
/// </summary>
public class CreateOfficeProjectDto
{
    // Required fields
    public string ProjectName { get; set; } = string.Empty;
    public DateTime ProjectDate { get; set; }
    public int FK_OfficeProjectTypeId { get; set; }

    // Optional fields
    public string? ProjectHint { get; set; }
    public DateTime? ProjectEndDate { get; set; }
    public int? FK_CountryId { get; set; }
    public int? FK_RegionId { get; set; }
    public int? FK_CenterId { get; set; }
    public string? VillageName { get; set; }
    public decimal? ProjectCostEGP { get; set; }
    public decimal? ProjectCostSAR { get; set; }
    public string? DonorName { get; set; }
    public int? BeneficiariesCount { get; set; }
    public string? BeneficiariesType { get; set; }
    public Guid? FK_CharityId { get; set; }

    // Attachments - Use List<AttachmentDto> for proper attachment handling
    public List<AttachmentDto>? Document_Attach { get; set; }
    public List<AttachmentDto>? Report_Attach { get; set; }

    public string? Notes { get; set; }
    public bool IsFinished { get; set; } = false;
}

/// <summary>
/// Update office project DTO (UC-7.8)
/// </summary>
public class UpdateOfficeProjectDto
{
    public string? ProjectName { get; set; }
    public string? ProjectHint { get; set; }
    public DateTime? ProjectDate { get; set; }
    public DateTime? ProjectEndDate { get; set; }
    public int? FK_OfficeProjectTypeId { get; set; }
    public int? FK_CountryId { get; set; }
    public int? FK_RegionId { get; set; }
    public int? FK_CenterId { get; set; }
    public string? VillageName { get; set; }
    public decimal? ProjectCostEGP { get; set; }
    public decimal? ProjectCostSAR { get; set; }
    public string? DonorName { get; set; }
    public int? BeneficiariesCount { get; set; }
    public string? BeneficiariesType { get; set; }
    public Guid? FK_CharityId { get; set; }

    // Attachments - Use List<AttachmentDto> for proper attachment handling
    public List<AttachmentDto>? Document_Attach { get; set; }
    public List<AttachmentDto>? Report_Attach { get; set; }

    public string? Notes { get; set; }
    public bool? IsFinished { get; set; }
}

/// <summary>
/// Office project filter DTO for queries (UC-7.10)
/// </summary>
public class OfficeProjectFilterDto
{
    public string? SearchText { get; set; }               // Search by project name
    public int? FK_OfficeProjectTypeId { get; set; }      // Filter by project type
    public int? FK_CountryId { get; set; }
    public int? FK_RegionId { get; set; }
    public int? FK_CenterId { get; set; }
    public Guid? FK_CharityId { get; set; }               // Filter by assigned charity
    public bool? IsFinished { get; set; }                 // Filter by completion status
    public string? DonorName { get; set; }                // Filter by donor
    public DateTime? StartDate { get; set; }              // Date range filter
    public DateTime? EndDate { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

/// <summary>
/// Office project status summary (UC-7.12)
/// </summary>
public class OfficeProjectStatusSummaryDto
{
    public int OngoingCount { get; set; }
    public int CompletedCount { get; set; }
    public int TotalCount { get; set; }
    public decimal? TotalCostEGP { get; set; }
    public decimal? TotalCostSAR { get; set; }
    public int? TotalBeneficiaries { get; set; }
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

// ========== DTOs for Specific Use Cases ==========

/// <summary>
/// Set project budget DTO (UC-7.2)
/// </summary>
public class SetProjectBudgetDto
{
    public decimal? ProjectCostEGP { get; set; }
    public decimal? ProjectCostSAR { get; set; }
}

/// <summary>
/// Specify project donor DTO (UC-7.3)
/// </summary>
public class SpecifyProjectDonorDto
{
    public string DonorName { get; set; } = string.Empty;
}

/// <summary>
/// Set beneficiaries count DTO (UC-7.4)
/// </summary>
public class SetBeneficiariesCountDto
{
    public int BeneficiariesCount { get; set; }
    public string BeneficiariesType { get; set; } = "Families"; // Families, Individuals, Both
}

/// <summary>
/// Assign project location DTO (UC-7.5)
/// </summary>
public class AssignProjectLocationDto
{
    public int? FK_CountryId { get; set; }
    public int? FK_RegionId { get; set; }
    public int? FK_CenterId { get; set; }
    public string? VillageName { get; set; }
}

/// <summary>
/// Attach project document DTO (UC-7.6)
/// </summary>
public class AttachProjectDocumentDto
{
    public Guid FK_AttachedFileId { get; set; }
    public string DocumentType { get; set; } = string.Empty; // Proposal, Contract, Progress Report, Other
    public string? Description { get; set; }
    public DateTime DocumentDate { get; set; } = DateTime.Today;
}

/// <summary>
/// Upload project report DTO (UC-7.7)
/// </summary>
public class UploadProjectReportDto
{
    public Guid FK_ProjectReportFileId { get; set; }
    public string ReportType { get; set; } = string.Empty; // Completion, Progress, Final
    public DateTime ReportDate { get; set; } = DateTime.Today;
    public string? Summary { get; set; }
}

/// <summary>
/// Mark project as completed DTO (UC-7.9)
/// </summary>
public class MarkProjectCompletedDto
{
    public bool IsFinished { get; set; } = true;
    public DateTime? ProjectEndDate { get; set; } = DateTime.Today;
}

/// <summary>
/// Set project dates DTO (UC-7.13)
/// </summary>
public class SetProjectDatesDto
{
    public DateTime ProjectDate { get; set; }
    public DateTime? ProjectEndDate { get; set; }
}

/// <summary>
/// Assign project to charity DTO (UC-7.14)
/// </summary>
public class AssignProjectToCharityDto
{
    public Guid? FK_CharityId { get; set; }
}
