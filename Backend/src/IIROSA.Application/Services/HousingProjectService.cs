using Microsoft.Extensions.Logging;
using IIROSA.Domain.Interfaces;
using IIROSA.Domain.Entities;
using IIROSA.Application.Interfaces;
using IIROSA.Application.DTOs.HousingProject;
using Microsoft.EntityFrameworkCore;

namespace IIROSA.Application.Services;

/// <summary>
/// Housing Project Service Implementation
/// Implements all use cases UC-10.1 through UC-10.10
/// </summary>
public class HousingProjectService : IHousingProjectService 
{
    private readonly IHousingProjectRepository _projectRepository;
    private readonly IFamilyRepository _familyRepository;
    private readonly ICharityRepository _charityRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<HousingProjectService> _logger;

    public HousingProjectService(
        IHousingProjectRepository projectRepository,
        IFamilyRepository familyRepository,
        ICharityRepository charityRepository,
        IUnitOfWork unitOfWork,
        ILogger<HousingProjectService> logger)
    {
        _projectRepository = projectRepository;
        _familyRepository = familyRepository;
        _charityRepository = charityRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    #region UC-10.1: Register Housing Project

    public async Task<HousingProjectDto> CreateProjectAsync(CreateHousingProjectDto dto)
    {
        _logger.LogInformation("Creating new housing project: {Name}", dto.Name);

        // Validate uniqueness
        if (!await IsProjectNameUniqueAsync(dto.Name))
        {
            throw new InvalidOperationException($"Project with name '{dto.Name}' already exists");
        }

        // Validate date range
        if (dto.ExpectedEndDate < dto.StartDate)
        {
            throw new ArgumentException("Expected end date must be greater than or equal to start date");
        }

        // Validate budget
        if (dto.TotalBudget <= 0)
        {
            throw new ArgumentException("Total budget must be greater than zero");
        }

        // Validate charity and family
        if (!await _charityRepository.ExistsAsync(dto.CharityId))
        {
            throw new ArgumentException($"Charity with ID '{dto.CharityId}' not found");
        }

        var family = await _familyRepository.GetByIdAsync(dto.FamilyId);
        if (family == null)
        {
            throw new ArgumentException($"Family with ID '{dto.FamilyId}' not found");
        }

        // Validate charity matches family's charity
        if (dto.CharityId != family.CharityId)
        {
            throw new ArgumentException("Family must belong to the assigned charity");
        }

        var project = new HousingProject
        {
            Id = Guid.NewGuid(),
            Name = dto.Name,
            Description = dto.Description,
            ProjectType = dto.ProjectType,
            StartDate = dto.StartDate,
            ExpectedEndDate = dto.ExpectedEndDate,
            CountryId = dto.CountryId,
            RegionId = dto.RegionId,
            CenterId = dto.CenterId,
            Address = dto.Address,
            Village = dto.Village,
            GPSCoordinates = dto.GPSCoordinates,
            HousingType = dto.HousingType,
            NumberOfUnits = dto.NumberOfUnits,
            AreaPerUnit = dto.AreaPerUnit,
            TotalArea = dto.TotalArea,
            TotalBudget = dto.TotalBudget,
            BudgetCurrency = dto.BudgetCurrency,
            DonorName = dto.DonorName,
            CharityId = dto.CharityId,
            FamilyId = dto.FamilyId,
            ProjectStatus = dto.ProjectStatus,
            CompletionPercentage = dto.CompletionPercentage
        };

        await _projectRepository.AddAsync(project);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Housing project created successfully with ID: {Id}", project.Id);

        return await GetProjectByIdAsync(project.Id);
    }

    #endregion

    #region UC-10.2: Set Project Budget

    public async Task SetProjectBudgetAsync(Guid projectId, decimal totalBudget, string currency, string? donorName)
    {
        _logger.LogInformation("Setting budget for project: {ProjectId}", projectId);

        var project = await _projectRepository.GetByIdAsync(projectId);
        if (project == null)
        {
            throw new KeyNotFoundException($"Project with ID '{projectId}' not found");
        }

        if (project.ProjectStatus == "Completed")
        {
            throw new InvalidOperationException("Cannot modify budget of a completed project");
        }

        if (totalBudget <= 0)
        {
            throw new ArgumentException("Total budget must be greater than zero");
        }

        project.TotalBudget = totalBudget;
        project.BudgetCurrency = currency;
        project.DonorName = donorName;

        _projectRepository.Update(project);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Project budget updated successfully for project: {ProjectId}", projectId);
    }

    #endregion

    #region UC-10.3: Assign Beneficiary Family

    public async Task AssignBeneficiaryFamilyAsync(Guid projectId, Guid? charityId, Guid? familyId)
    {
        _logger.LogInformation("Assigning beneficiary family to project: {ProjectId}", projectId);

        var project = await _projectRepository.GetByIdAsync(projectId);
        if (project == null)
        {
            throw new KeyNotFoundException($"Project with ID '{projectId}' not found");
        }

        if (project.ProjectStatus == "Completed")
        {
            throw new InvalidOperationException("Cannot modify beneficiary assignment for a completed project");
        }

        // Validate charity if provided
        if (charityId.HasValue && !await _charityRepository.ExistsAsync(charityId.Value))
        {
            throw new ArgumentException($"Charity with ID '{charityId}' not found");
        }

        // Validate family if provided
        if (familyId.HasValue)
        {
            var family = await _familyRepository.GetByIdAsync(familyId.Value);
            if (family == null)
            {
                throw new ArgumentException($"Family with ID '{familyId}' not found");
            }

            // If both charity and family are provided, validate they match
            if (charityId.HasValue && charityId != family.CharityId)
            {
                throw new ArgumentException("Family must belong to the assigned charity");
            }

            // If only family is provided, use family's charity
            project.CharityId = family.CharityId;
        }
        else
        {
            project.CharityId = charityId;
        }

        project.FamilyId = familyId;

        _projectRepository.Update(project);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Beneficiary family assigned successfully to project: {ProjectId}", projectId);
    }

    #endregion

    #region UC-10.4: Track Construction Progress

    public async Task UpdateProjectProgressAsync(UpdateProjectProgressDto dto)
    {
        _logger.LogInformation("Updating progress for project: {ProjectId}", dto.ProjectId);

        var project = await _projectRepository.GetByIdAsync(dto.ProjectId);
        if (project == null)
        {
            throw new KeyNotFoundException($"Project with ID '{dto.ProjectId}' not found");
        }

        if (project.ProjectStatus == "Completed")
        {
            throw new InvalidOperationException("Cannot update progress of a completed project");
        }

        // Validate status
        var validStatuses = new[] { "Planning", "In Progress", "On Hold" };
        if (!validStatuses.Contains(dto.ProjectStatus))
        {
            throw new ArgumentException($"Invalid project status. Must be one of: {string.Join(", ", validStatuses)}");
        }

        project.ProjectStatus = dto.ProjectStatus;
        project.CompletionPercentage = dto.CompletionPercentage;
        project.CurrentStage = dto.CurrentStage;
        project.ProgressNotes = dto.ProgressNotes;

        _projectRepository.Update(project);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Project progress updated successfully for project: {ProjectId}", dto.ProjectId);
    }

    #endregion

    #region UC-10.5: Record Project Completion

    public async Task CompleteProjectAsync(CompleteProjectDto dto)
    {
        _logger.LogInformation("Completing project: {ProjectId}", dto.ProjectId);

        var project = await _projectRepository.GetByIdAsync(dto.ProjectId);
        if (project == null)
        {
            throw new KeyNotFoundException($"Project with ID '{dto.ProjectId}' not found");
        }

        if (project.ProjectStatus == "Completed")
        {
            throw new InvalidOperationException("Project is already completed");
        }

        if (dto.FinalCost <= 0)
        {
            throw new ArgumentException("Final cost must be greater than zero");
        }

        project.ProjectStatus = "Completed";
        project.CompletionPercentage = 100;
        project.ActualEndDate = dto.ActualEndDate;
        project.FinalCost = dto.FinalCost;
        project.CompletionNotes = dto.CompletionNotes;
        project.HandoverDocumentId = dto.HandoverDocumentId;

        _projectRepository.Update(project);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Project completed successfully: {ProjectId}", dto.ProjectId);
    }

    public async Task ReopenProjectAsync(Guid projectId)
    {
        _logger.LogInformation("Reopening project: {ProjectId}", projectId);

        var project = await _projectRepository.GetByIdAsync(projectId);
        if (project == null)
        {
            throw new KeyNotFoundException($"Project with ID '{projectId}' not found");
        }

        if (project.ProjectStatus != "Completed")
        {
            throw new InvalidOperationException("Only completed projects can be reopened");
        }

        project.ProjectStatus = "In Progress";
        project.CompletionPercentage = 99; // Set to 99% to indicate near completion
        project.ActualEndDate = null;
        project.FinalCost = null;
        project.CompletionNotes = null;

        _projectRepository.Update(project);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Project reopened successfully: {ProjectId}", projectId);
    }

    #endregion

    #region UC-10.6: View Housing Projects

    public async Task<(IEnumerable<HousingProjectListDto> Items, int TotalCount)> GetProjectsAsync(HousingProjectFilterDto filter)
    {
        _logger.LogInformation("Getting projects with filter: {@Filter}", filter);

        var (projects, totalCount) = await _projectRepository.GetFilteredAsync(
            name: filter.SearchTerm,
            projectType: filter.ProjectType,
            projectStatus: filter.ProjectStatus,
            countryId: filter.CountryId,
            regionId: filter.RegionId,
            centerId: filter.CenterId,
            charityId: filter.CharityId,
            housingType: filter.HousingType);

        var projectDtos = projects.Select(p => new HousingProjectListDto
        {
            Id = p.Id,
            Name = p.Name,
            ProjectType = p.ProjectType,
            RegionName = p.Region?.Name,
            CenterName = p.Center?.Name,
            Address = p.Address,
            CharityName = p.Charity?.Name,
            FamilyCode = p.Family?.Code,
            ProjectStatus = p.ProjectStatus,
            CompletionPercentage = p.CompletionPercentage,
            StartDate = p.StartDate,
            ExpectedEndDate = p.ExpectedEndDate,
            TotalBudget = p.TotalBudget,
            BudgetCurrency = p.BudgetCurrency,
            IsCompleted = p.IsCompleted,
            IsDelayed = p.IsDelayed
        });

        return (projectDtos, totalCount);
    }

    public async Task<IEnumerable<HousingProjectListDto>> GetActiveProjectsAsync()
    {
        _logger.LogInformation("Getting active projects");

        var projects = await _projectRepository.GetActiveProjectsAsync();

        return projects.Select(p => new HousingProjectListDto
        {
            Id = p.Id,
            Name = p.Name,
            ProjectType = p.ProjectType,
            RegionName = p.Region?.Name,
            CenterName = p.Center?.Name,
            Address = p.Address,
            CharityName = p.Charity?.Name,
            FamilyCode = p.Family?.Code,
            ProjectStatus = p.ProjectStatus,
            CompletionPercentage = p.CompletionPercentage,
            StartDate = p.StartDate,
            ExpectedEndDate = p.ExpectedEndDate,
            TotalBudget = p.TotalBudget,
            BudgetCurrency = p.BudgetCurrency,
            IsCompleted = p.IsCompleted,
            IsDelayed = p.IsDelayed
        });
    }

    public async Task<IEnumerable<HousingProjectListDto>> GetCompletedProjectsAsync()
    {
        _logger.LogInformation("Getting completed projects");

        var projects = await _projectRepository.GetCompletedProjectsAsync();

        return projects.Select(p => new HousingProjectListDto
        {
            Id = p.Id,
            Name = p.Name,
            ProjectType = p.ProjectType,
            RegionName = p.Region?.Name,
            CenterName = p.Center?.Name,
            Address = p.Address,
            CharityName = p.Charity?.Name,
            FamilyCode = p.Family?.Code,
            ProjectStatus = p.ProjectStatus,
            CompletionPercentage = p.CompletionPercentage,
            StartDate = p.StartDate,
            ExpectedEndDate = p.ExpectedEndDate,
            TotalBudget = p.TotalBudget,
            BudgetCurrency = p.BudgetCurrency,
            IsCompleted = p.IsCompleted,
            IsDelayed = p.IsDelayed
        });
    }

    public async Task<IEnumerable<HousingProjectListDto>> GetDelayedProjectsAsync()
    {
        _logger.LogInformation("Getting delayed projects");

        var projects = await _projectRepository.GetDelayedProjectsAsync();

        return projects.Select(p => new HousingProjectListDto
        {
            Id = p.Id,
            Name = p.Name,
            ProjectType = p.ProjectType,
            RegionName = p.Region?.Name,
            CenterName = p.Center?.Name,
            Address = p.Address,
            CharityName = p.Charity?.Name,
            FamilyCode = p.Family?.Code,
            ProjectStatus = p.ProjectStatus,
            CompletionPercentage = p.CompletionPercentage,
            StartDate = p.StartDate,
            ExpectedEndDate = p.ExpectedEndDate,
            TotalBudget = p.TotalBudget,
            BudgetCurrency = p.BudgetCurrency,
            IsCompleted = p.IsCompleted,
            IsDelayed = p.IsDelayed
        });
    }

    #endregion

    #region UC-10.7: Update Project Status

    public async Task<HousingProjectDto> UpdateProjectAsync(UpdateHousingProjectDto dto)
    {
        _logger.LogInformation("Updating project: {Id}", dto.Id);

        var project = await _projectRepository.GetByIdAsync(dto.Id);
        if (project == null)
        {
            throw new KeyNotFoundException($"Project with ID '{dto.Id}' not found");
        }

        // Validate uniqueness
        if (!await IsProjectNameUniqueAsync(dto.Name, dto.Id))
        {
            throw new InvalidOperationException($"Project with name '{dto.Name}' already exists");
        }

        // Validate date range
        if (dto.ExpectedEndDate.HasValue && dto.ExpectedEndDate < dto.StartDate)
        {
            throw new ArgumentException("Expected end date must be greater than or equal to start date");
        }

        // Validate charity and family
        if (!await _charityRepository.ExistsAsync(dto.CharityId))
        {
            throw new ArgumentException($"Charity with ID '{dto.CharityId}' not found");
        }

        var family = await _familyRepository.GetByIdAsync(dto.FamilyId);
        if (family == null)
        {
            throw new ArgumentException($"Family with ID '{dto.FamilyId}' not found");
        }

        if (dto.CharityId != family.CharityId)
        {
            throw new ArgumentException("Family must belong to the assigned charity");
        }

        // Update fields
        project.Name = dto.Name;
        project.Description = dto.Description;
        project.ProjectType = dto.ProjectType;
        project.StartDate = dto.StartDate;
        project.ExpectedEndDate = dto.ExpectedEndDate;
        project.ActualEndDate = dto.ActualEndDate;
        project.CountryId = dto.CountryId;
        project.RegionId = dto.RegionId;
        project.CenterId = dto.CenterId;
        project.Address = dto.Address;
        project.Village = dto.Village;
        project.GPSCoordinates = dto.GPSCoordinates;
        project.HousingType = dto.HousingType;
        project.NumberOfUnits = dto.NumberOfUnits;
        project.AreaPerUnit = dto.AreaPerUnit;
        project.TotalArea = dto.TotalArea;
        project.TotalBudget = dto.TotalBudget;
        project.BudgetCurrency = dto.BudgetCurrency;
        project.DonorName = dto.DonorName;
        project.FinalCost = dto.FinalCost;
        project.CharityId = dto.CharityId;
        project.FamilyId = dto.FamilyId;
        project.ProjectStatus = dto.ProjectStatus;
        project.CompletionPercentage = dto.CompletionPercentage;
        project.CurrentStage = dto.CurrentStage;
        project.ProgressNotes = dto.ProgressNotes;
        project.CompletionNotes = dto.CompletionNotes;
        project.HandoverDocumentId = dto.HandoverDocumentId;

        _projectRepository.Update(project);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Project updated successfully: {Id}", dto.Id);

        return await GetProjectByIdAsync(project.Id);
    }

    #endregion

    #region UC-10.9: Generate Housing Report

    public async Task<HousingProjectReportDto> GenerateHousingReportAsync(HousingProjectReportFilterDto filter)
    {
        _logger.LogInformation("Generating housing report with filter: {@Filter}", filter);

        var allProjects = await _projectRepository.GetAllAsync();

        // Apply date filter
        var projects = allProjects
            .Where(p => p.StartDate >= filter.StartDate && p.StartDate <= filter.EndDate)
            .ToList();

        // Apply status filter
        if (!string.IsNullOrEmpty(filter.ProjectStatus))
        {
            projects = projects.Where(p => p.ProjectStatus == filter.ProjectStatus).ToList();
        }

        // Apply completion filter
        if (!filter.IncludeCompletedProjects)
        {
            projects = projects.Where(p => p.ProjectStatus != "Completed").ToList();
        }

        if (!filter.IncludeActiveProjects)
        {
            projects = projects.Where(p => p.ProjectStatus == "Completed").ToList();
        }

        var report = new HousingProjectReportDto
        {
            ReportStartDate = filter.StartDate,
            ReportEndDate = filter.EndDate,
            GroupBy = filter.GroupBy,
            GeneratedOn = DateTime.UtcNow,
            TotalProjects = projects.Count,
            ProjectsByStatus = projects.GroupBy(p => p.ProjectStatus)
                .ToDictionary(g => g.Key, g => g.Count()),
            ProjectsByType = projects.GroupBy(p => p.ProjectType)
                .ToDictionary(g => g.Key, g => g.Count()),
            ProjectsByRegion = await _projectRepository.GetProjectsByRegionAsync(),
            ProjectsByCharity = await _projectRepository.GetProjectsByCharityAsync(),
            TotalBudget = projects.Sum(p => p.TotalBudget),
            TotalActualCost = projects.Where(p => p.FinalCost.HasValue).Sum(p => p.FinalCost.Value),
            AverageCostPerProject = projects.Any() ? projects.Average(p => p.FinalCost ?? p.TotalBudget) : 0,
            BudgetByStatus = await _projectRepository.GetBudgetByStatusAsync(),
            AverageCompletionPercentage = projects.Any() ? projects.Average(p => p.CompletionPercentage) : 0,
            ProjectsOnTrack = projects.Count(p => p.IsOnTrack),
            DelayedProjects = projects.Count(p => p.IsDelayed),
            CompletedProjects = projects.Count(p => p.IsCompleted),
            ActiveProjects = projects.Count(p => !p.IsCompleted),
            FamiliesHoused = projects.Count(p => p.FamilyId.HasValue),
            IndividualsBenefited = projects.Where(p => p.Family != null).Sum(p => p.Family.FamilyMembersCount),
            TotalUnitsBuilt = projects.Sum(p => p.NumberOfUnits ?? 0),
            TotalAreaBuilt = projects.Sum(p => p.TotalArea ?? 0)
        };

        // Build detailed project list
        report.ProjectDetails = projects.Select(p => new HousingProjectReportDetailDto
        {
            ProjectId = p.Id,
            ProjectName = p.Name,
            ProjectType = p.ProjectType,
            RegionName = p.Region?.Name,
            CharityName = p.Charity?.Name,
            FamilyCode = p.Family?.Code,
            ProjectStatus = p.ProjectStatus,
            CompletionPercentage = p.CompletionPercentage,
            StartDate = p.StartDate,
            ExpectedEndDate = p.ExpectedEndDate,
            ActualEndDate = p.ActualEndDate,
            TotalBudget = p.TotalBudget,
            FinalCost = p.FinalCost,
            BudgetCurrency = p.BudgetCurrency,
            NumberOfUnits = p.NumberOfUnits,
            TotalArea = p.TotalArea,
            IsDelayed = p.IsDelayed
        }).ToList();

        _logger.LogInformation("Housing report generated successfully");

        return report;
    }

    public async Task<byte[]> ExportHousingReportToPdfAsync(HousingProjectReportFilterDto filter)
    {
        _logger.LogInformation("Exporting housing report to PDF");

        var report = await GenerateHousingReportAsync(filter);

        // TODO: Implement PDF generation using a library like iTextSharp or QuestPDF
        throw new NotImplementedException("PDF export not yet implemented");
    }

    public async Task<byte[]> ExportHousingReportToExcelAsync(HousingProjectReportFilterDto filter)
    {
        _logger.LogInformation("Exporting housing report to Excel");

        var report = await GenerateHousingReportAsync(filter);

        // TODO: Implement Excel generation using a library like EPPlus or ClosedXML
        throw new NotImplementedException("Excel export not yet implemented");
    }

    #endregion

    #region UC-10.10: Assign Project to Charity

    public async Task AssignProjectToCharityAsync(Guid projectId, Guid? charityId)
    {
        _logger.LogInformation("Assigning project {ProjectId} to charity {CharityId}", projectId, charityId);

        var project = await _projectRepository.GetByIdAsync(projectId);
        if (project == null)
        {
            throw new KeyNotFoundException($"Project with ID '{projectId}' not found");
        }

        if (project.ProjectStatus == "Completed")
        {
            throw new InvalidOperationException("Cannot modify charity assignment for a completed project");
        }

        // Validate charity if provided
        if (charityId.HasValue && !await _charityRepository.ExistsAsync(charityId.Value))
        {
            throw new ArgumentException($"Charity with ID '{charityId}' not found");
        }

        // If project has a family assigned, validate charity matches
        if (project.FamilyId.HasValue)
        {
            var family = await _familyRepository.GetByIdAsync(project.FamilyId.Value);
            if (family != null && charityId.HasValue && charityId != family.CharityId)
            {
                throw new ArgumentException("Cannot assign project to a different charity than the assigned family's charity");
            }
        }

        project.CharityId = charityId;

        _projectRepository.Update(project);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Project assigned to charity successfully: {ProjectId}", projectId);
    }

    #endregion

    #region Additional Helper Methods

    public async Task<HousingProjectDto?> GetProjectByIdAsync(Guid id)
    {
        var project = await _projectRepository.GetByIdAsync(id);

        if (project == null)
        {
            return null;
        }

        return MapToProjectDto(project);
    }

    public async Task<HousingProjectDto?> GetProjectByNameAsync(string name)
    {
        var projects = await _projectRepository.SearchAsync(name);
        var project = projects.FirstOrDefault();

        return project == null ? null : MapToProjectDto(project);
    }

    public async Task<bool> IsProjectNameUniqueAsync(string name, Guid? excludeId = null)
    {
        return await _projectRepository.IsNameUniqueAsync(name, excludeId);
    }

    public async Task<bool> IsProjectActiveAsync(Guid projectId)
    {
        return await _projectRepository.IsActiveAsync(projectId);
    }

    public async Task<bool> IsProjectCompletedAsync(Guid projectId)
    {
        return await _projectRepository.IsCompletedAsync(projectId);
    }

    public async Task<decimal> GetProjectRemainingBudgetAsync(Guid projectId)
    {
        var project = await _projectRepository.GetByIdAsync(projectId);
        if (project == null)
        {
            throw new KeyNotFoundException($"Project with ID '{projectId}' not found");
        }

        return project.BudgetRemaining;
    }

    public async Task DeleteProjectAsync(Guid id)
    {
        _logger.LogInformation("Deleting project: {Id}", id);

        var project = await _projectRepository.GetByIdAsync(id);
        if (project == null)
        {
            throw new KeyNotFoundException($"Project with ID '{id}' not found");
        }

        if (project.ProjectStatus == "Completed")
        {
            throw new InvalidOperationException("Cannot delete a completed project");
        }

        _projectRepository.Delete(project);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Project deleted successfully: {Id}", id);
    }

    #endregion

    #region Statistics

    public async Task<int> GetTotalProjectsCountAsync()
    {
        return await _projectRepository.GetTotalProjectsAsync();
    }

    public async Task<int> GetProjectsByStatusCountAsync(string status)
    {
        return await _projectRepository.GetProjectsByStatusCountAsync(status);
    }

    public async Task<double> GetAverageCompletionPercentageAsync()
    {
        return await _projectRepository.GetAverageCompletionPercentageAsync();
    }

    public async Task<int> GetDelayedProjectsCountAsync()
    {
        return await _projectRepository.GetDelayedProjectsCountAsync();
    }

    public async Task<int> GetOnTrackProjectsCountAsync()
    {
        return await _projectRepository.GetOnTrackProjectsCountAsync();
    }

    #endregion

    #region Private Helper Methods

    private HousingProjectDto MapToProjectDto(HousingProject project)
    {
        return new HousingProjectDto
        {
            Id = project.Id,
            Name = project.Name,
            Description = project.Description,
            ProjectType = project.ProjectType,
            StartDate = project.StartDate,
            ExpectedEndDate = project.ExpectedEndDate,
            ActualEndDate = project.ActualEndDate,
            CountryId = project.CountryId,
            CountryName = project.Country?.Name,
            RegionId = project.RegionId,
            RegionName = project.Region?.Name,
            CenterId = project.CenterId,
            CenterName = project.Center?.Name,
            Address = project.Address,
            Village = project.Village,
            GPSCoordinates = project.GPSCoordinates,
            HousingType = project.HousingType,
            NumberOfUnits = project.NumberOfUnits,
            AreaPerUnit = project.AreaPerUnit,
            TotalArea = project.TotalArea,
            TotalBudget = project.TotalBudget,
            BudgetCurrency = project.BudgetCurrency,
            DonorName = project.DonorName,
            FinalCost = project.FinalCost,
            BudgetRemaining = project.BudgetRemaining,
            CharityId = project.CharityId,
            CharityName = project.Charity?.Name,
            FamilyId = project.FamilyId,
            FamilyCode = project.Family?.Code,
            FamilyAddress = project.Family?.Address,
            ProjectStatus = project.ProjectStatus,
            CompletionPercentage = project.CompletionPercentage,
            CurrentStage = project.CurrentStage,
            ProgressNotes = project.ProgressNotes,
            CompletionNotes = project.CompletionNotes,
            HandoverDocumentId = project.HandoverDocumentId,
            IsCompleted = project.IsCompleted,
            IsDelayed = project.IsDelayed,
            IsOnTrack = project.IsOnTrack,
            CreatedOn = project.CreatedOn,
            CreatedBy = Guid.TryParse(project.CreatedBy, out var createdById) ? createdById.ToString() : null,
            UpdatedOn = project.UpdatedOn,
            UpdatedBy = Guid.TryParse(project.UpdatedBy, out var updatedById) ? updatedById.ToString() : null
        };
    }

    #endregion
}
