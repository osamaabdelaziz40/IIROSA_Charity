using IIROSA.Application.DTOs.OfficeProjectManagement;

namespace IIROSA.Application.Interfaces;

/// <summary>
/// OfficeProject Service Interface
/// Defines business operations for OfficeProject management following UC-7.1 to UC-7.14
/// IMPORTANT: Only Admin and Super Admin roles can access these operations.
/// Charity users are explicitly blocked from this module.
/// </summary>
public interface IOfficeProjectService
{
    // ========== CRUD Operations ==========

    /// <summary>
    /// Get office projects with filtering and pagination (UC-7.10: View Project List)
    /// </summary>
    Task<OfficeProjectPagedResult<OfficeProjectListDto>> GetProjectsFilteredAsync(OfficeProjectFilterDto filter);

    /// <summary>
    /// Get office project by ID (UC-7.11: View Project Details)
    /// </summary>
    Task<OfficeProjectDetailDto?> GetProjectByIdAsync(Guid id);

    /// <summary>
    /// Create new office project (UC-7.1: Create Office Project)
    /// </summary>
    Task<OfficeProjectDetailDto> CreateProjectAsync(CreateOfficeProjectDto dto);

    /// <summary>
    /// Update office project (UC-7.8: Update Project Details)
    /// </summary>
    Task<OfficeProjectDetailDto> UpdateProjectAsync(Guid id, UpdateOfficeProjectDto dto);

    /// <summary>
    /// Delete office project
    /// </summary>
    Task DeleteProjectAsync(Guid id);

    // ========== Project-Specific Operations ==========

    /// <summary>
    /// Set project budget (UC-7.2: Set Project Budget)
    /// </summary>
    Task SetProjectBudgetAsync(Guid id, SetProjectBudgetDto dto);

    /// <summary>
    /// Specify project donor (UC-7.3: Specify Project Donor)
    /// </summary>
    Task SpecifyProjectDonorAsync(Guid id, SpecifyProjectDonorDto dto);

    /// <summary>
    /// Set beneficiaries count (UC-7.4: Set Beneficiaries Count)
    /// </summary>
    Task SetBeneficiariesCountAsync(Guid id, SetBeneficiariesCountDto dto);

    /// <summary>
    /// Assign project location (UC-7.5: Assign Project Location)
    /// </summary>
    Task AssignProjectLocationAsync(Guid id, AssignProjectLocationDto dto);

    /// <summary>
    /// Attach project document (UC-7.6: Attach Project Documents)
    /// </summary>
    Task AttachProjectDocumentAsync(Guid id, AttachProjectDocumentDto dto);

    /// <summary>
    /// Upload project report (UC-7.7: Upload Project Report)
    /// </summary>
    Task UploadProjectReportAsync(Guid id, UploadProjectReportDto dto);

    /// <summary>
    /// Mark project as completed (UC-7.9: Mark Project as Completed)
    /// </summary>
    Task MarkProjectAsCompletedAsync(Guid id, MarkProjectCompletedDto dto);

    /// <summary>
    /// Set project dates (UC-7.13: Set Project Dates)
    /// </summary>
    Task SetProjectDatesAsync(Guid id, SetProjectDatesDto dto);

    /// <summary>
    /// Assign project to charity (UC-7.14: Assign Project to Charity)
    /// </summary>
    Task AssignProjectToCharityAsync(Guid id, AssignProjectToCharityDto dto);

    // ========== View Operations ==========

    /// <summary>
    /// Get projects assigned to a specific charity (UC-7.14)
    /// </summary>
    Task<OfficeProjectPagedResult<OfficeProjectListDto>> GetProjectsByCharityAsync(Guid charityId, OfficeProjectFilterDto filter);

    /// <summary>
    /// Get project status summary (UC-7.12: Track Project Progress)
    /// </summary>
    Task<OfficeProjectStatusSummaryDto> GetProjectStatusSummaryAsync();

    /// <summary>
    /// Get ongoing projects
    /// </summary>
    Task<List<OfficeProjectListDto>> GetOngoingProjectsAsync();

    /// <summary>
    /// Get completed projects
    /// </summary>
    Task<List<OfficeProjectListDto>> GetCompletedProjectsAsync();

    // ========== Export ==========

    /// <summary>
    /// Export projects to Excel (UC-7.10: Export to Excel)
    /// </summary>
    Task<byte[]> ExportProjectsToExcelAsync(OfficeProjectFilterDto filter);
}
