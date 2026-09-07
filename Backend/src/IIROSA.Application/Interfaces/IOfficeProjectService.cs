using IIROSA.Application.DTOs.OfficeProjectManagement;

namespace IIROSA.Application.Interfaces;

/// <summary>
/// OfficeProject Service Interface (UC-OFP-01…06)
/// Head-office module: only Admin and Super Admin reach it; the controller enforces the roles and
/// the service enforces the caller's country scope on every read and write.
/// </summary>
public interface IOfficeProjectService
{
    /// <summary>
    /// Get office projects with filtering and pagination (UC-OFP-01: list)
    /// </summary>
    Task<OfficeProjectPagedResult<OfficeProjectListDto>> GetProjectsFilteredAsync(OfficeProjectFilterDto filter);

    /// <summary>
    /// Get office project by ID with navigations and attachments (UC-OFP-04: view)
    /// </summary>
    Task<OfficeProjectDetailDto?> GetProjectByIdAsync(Guid id);

    /// <summary>
    /// Create new office project (UC-OFP-03)
    /// </summary>
    Task<OfficeProjectDetailDto> CreateProjectAsync(CreateOfficeProjectDto dto);

    /// <summary>
    /// Update office project (UC-OFP-04)
    /// </summary>
    Task<OfficeProjectDetailDto> UpdateProjectAsync(Guid id, UpdateOfficeProjectDto dto);

    /// <summary>
    /// Delete office project — soft delete (UC-OFP-05)
    /// </summary>
    Task DeleteProjectAsync(Guid id);

    /// <summary>
    /// Mark project as completed (module completion tracking, feeds the progress view)
    /// </summary>
    Task MarkProjectAsCompletedAsync(Guid id, MarkProjectCompletedDto dto);

    /// <summary>
    /// Export projects to Excel with the §18.S.1 report columns (UC-OFP-06: report)
    /// </summary>
    Task<byte[]> ExportProjectsToExcelAsync(OfficeProjectFilterDto filter);
}
