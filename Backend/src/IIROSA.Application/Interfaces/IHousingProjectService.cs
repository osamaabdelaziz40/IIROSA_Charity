using IIROSA.Application.DTOs.HousingProject;

namespace IIROSA.Application.Interfaces;

/// <summary>
/// Housing Project Service Interface
/// Implements all use cases UC-10.1 through UC-10.10
/// </summary>
public interface IHousingProjectService
{
    // UC-10.1: Register Housing Project
    Task<HousingProjectDto> CreateProjectAsync(CreateHousingProjectDto dto);

    // UC-10.2: Set Project Budget
    Task SetProjectBudgetAsync(Guid projectId, decimal totalBudget, string currency, string? donorName);

    // UC-10.3: Assign Beneficiary Family
    Task AssignBeneficiaryFamilyAsync(Guid projectId, Guid? charityId, Guid? familyId);

    // UC-10.4: Track Construction Progress
    Task UpdateProjectProgressAsync(UpdateProjectProgressDto dto);

    // UC-10.5: Record Project Completion
    Task CompleteProjectAsync(CompleteProjectDto dto);
    Task ReopenProjectAsync(Guid projectId);

    // UC-10.6: View Housing Projects
    Task<(IEnumerable<HousingProjectListDto> Items, int TotalCount)> GetProjectsAsync(HousingProjectFilterDto filter);
    Task<IEnumerable<HousingProjectListDto>> GetActiveProjectsAsync();
    Task<IEnumerable<HousingProjectListDto>> GetCompletedProjectsAsync();
    Task<IEnumerable<HousingProjectListDto>> GetDelayedProjectsAsync();

    // UC-10.7: Update Project Status
    Task<HousingProjectDto> UpdateProjectAsync(UpdateHousingProjectDto dto);

    // UC-10.9: Generate Housing Report
    Task<HousingProjectReportDto> GenerateHousingReportAsync(HousingProjectReportFilterDto filter);
    Task<byte[]> ExportHousingReportToPdfAsync(HousingProjectReportFilterDto filter);
    Task<byte[]> ExportHousingReportToExcelAsync(HousingProjectReportFilterDto filter);

    // UC-10.10: Assign Project to Charity
    Task AssignProjectToCharityAsync(Guid projectId, Guid? charityId);

    // Additional helper methods
    Task<HousingProjectDto?> GetProjectByIdAsync(Guid id);
    Task<HousingProjectDto?> GetProjectByNameAsync(string name);
    Task<bool> IsProjectNameUniqueAsync(string name, Guid? excludeId = null);
    Task<bool> IsProjectActiveAsync(Guid projectId);
    Task<bool> IsProjectCompletedAsync(Guid projectId);
    Task<decimal> GetProjectRemainingBudgetAsync(Guid projectId);
    Task DeleteProjectAsync(Guid id);

    // Statistics
    Task<int> GetTotalProjectsCountAsync();
    Task<int> GetProjectsByStatusCountAsync(string status);
    Task<double> GetAverageCompletionPercentageAsync();
    Task<int> GetDelayedProjectsCountAsync();
    Task<int> GetOnTrackProjectsCountAsync();
}

/// <summary>
/// Housing Project Report Filter DTO
/// </summary>
public class HousingProjectReportFilterDto
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string? ProjectStatus { get; set; }
    public bool IncludeCompletedProjects { get; set; } = true;
    public bool IncludeActiveProjects { get; set; } = true;
    public string? GroupBy { get; set; } // Charity, Region, ProjectType
}
