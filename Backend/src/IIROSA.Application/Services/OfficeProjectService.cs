using IIROSA.Application.DTOs.OfficeProjectManagement;
using IIROSA.Application.Interfaces;
using IIROSA.Domain.Entities;
using IIROSA.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using AutoMapper;
using Framework.Core.SharedServices.Services;
using Framework.Core.SharedServices.Dto;

namespace IIROSA.Application.Services;

/// <summary>
/// OfficeProject Service Implementation
/// Implements business logic for OfficeProject management following UC-7.1 to UC-7.14
/// IMPORTANT: Only Admin and Super Admin roles can access this service.
/// Charity users are explicitly blocked from this module.
/// </summary>
public class OfficeProjectService : IOfficeProjectService
{
    private readonly IOfficeProjectRepository _projectRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<OfficeProjectService> _logger;
    private readonly AttachmentService _attachmentService;
    private readonly IAttachmentHelperService _attachmentHelperService;

    public OfficeProjectService(
        IOfficeProjectRepository projectRepository,
        IMapper mapper,
        ILogger<OfficeProjectService> logger,
        AttachmentService attachmentService,
        IAttachmentHelperService attachmentHelperService)
    {
        _projectRepository = projectRepository;
        _mapper = mapper;
        _logger = logger;
        _attachmentService = attachmentService;
        _attachmentHelperService = attachmentHelperService;
    }

    // ========== CRUD Operations ==========

    /// <summary>
    /// Get projects with filtering and pagination (UC-7.10: View Project List)
    /// </summary>
    public async Task<OfficeProjectPagedResult<OfficeProjectListDto>> GetProjectsFilteredAsync(OfficeProjectFilterDto filter)
    {
        try
        {
            _logger.LogInformation("Retrieving office projects with filter: {@Filter}", filter);

            // Build filter expression
            System.Linq.Expressions.Expression<Func<OfficeProject, bool>>? filterExpression = null;

            if (filter != null)
            {
                filterExpression = p =>
                    (!filter.FK_OfficeProjectTypeId.HasValue || p.FK_OfficeProjectTypeId == filter.FK_OfficeProjectTypeId.Value) &&
                    (!filter.FK_CountryId.HasValue || p.FK_CountryId == filter.FK_CountryId.Value) &&
                    (!filter.FK_RegionId.HasValue || p.FK_RegionId == filter.FK_RegionId.Value) &&
                    (!filter.FK_CenterId.HasValue || p.FK_CenterId == filter.FK_CenterId.Value) &&
                    (!filter.FK_CharityId.HasValue || p.FK_CharityId == filter.FK_CharityId.Value) &&
                    (!filter.IsFinished.HasValue || p.IsFinished == filter.IsFinished.Value) &&
                    (!filter.StartDate.HasValue || p.ProjectDate >= filter.StartDate.Value) &&
                    (!filter.EndDate.HasValue || p.ProjectDate <= filter.EndDate.Value) &&
                    (string.IsNullOrWhiteSpace(filter.DonorName) || p.DonorName != null && p.DonorName.Contains(filter.DonorName)) &&
                    (string.IsNullOrWhiteSpace(filter.SearchText) || p.ProjectName.Contains(filter.SearchText));
            }

            var (items, totalCount) = await _projectRepository.GetProjectsPagedAsync(
                filterExpression,
                q => q.OrderByDescending(p => p.ProjectDate),
                filter?.Page ?? 1,
                filter?.PageSize ?? 20);

            var projectDtos = _mapper.Map<List<OfficeProjectListDto>>(items);

            return new OfficeProjectPagedResult<OfficeProjectListDto>
            {
                Items = projectDtos,
                TotalCount = totalCount,
                Page = filter?.Page ?? 1,
                PageSize = filter?.PageSize ?? 20
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving office projects with filter: {@Filter}", filter);
            throw;
        }
    }

    /// <summary>
    /// Get project by ID (UC-7.11: View Project Details)
    /// </summary>
    public async Task<OfficeProjectDetailDto?> GetProjectByIdAsync(Guid id)
    {
        try
        {
            return await GetByIdAsync(id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving office project detail for ID: {ProjectId}", id);
            throw;
        }
    }

    /// <summary>
    /// Create new project (UC-7.1: Create Office Project)
    /// </summary>
    public async Task<OfficeProjectDetailDto> CreateProjectAsync(CreateOfficeProjectDto dto)
    {
        try
        {
            _logger.LogInformation("Creating new office project: {@Project}", dto);

            // Validate location cascade (Country → Region → Center)
            if (dto.FK_CenterId.HasValue && !dto.FK_RegionId.HasValue)
            {
                throw new InvalidOperationException("Region must be specified when Center is selected");
            }

            if (dto.FK_RegionId.HasValue && !dto.FK_CountryId.HasValue)
            {
                throw new InvalidOperationException("Country must be specified when Region is selected");
            }

            // Process document attachment
            Guid? documentFileId = null;
            if (dto.Document_Attach != null && dto.Document_Attach.Any())
            {
                var attachment = dto.Document_Attach.FirstOrDefault(a => !string.IsNullOrEmpty(a.FileName) && a.FileData != null);
                if (attachment != null)
                {
                    documentFileId = await _attachmentHelperService.SaveAttachmentAsync(attachment);
                }
            }

            // Process report attachment
            Guid? reportFileId = null;
            if (dto.Report_Attach != null && dto.Report_Attach.Any())
            {
                var attachment = dto.Report_Attach.FirstOrDefault(a => !string.IsNullOrEmpty(a.FileName) && a.FileData != null);
                if (attachment != null)
                {
                    reportFileId = await _attachmentHelperService.SaveAttachmentAsync(attachment);
                }
            }

            // Create project entity
            var project = _mapper.Map<OfficeProject>(dto);
            project.FK_AttachedFileId = documentFileId;
            project.FK_ProjectReportFileId = reportFileId;

            // Save to database
            await _projectRepository.AddAsync(project);
            await _projectRepository.SaveChangesAsync();

            _logger.LogInformation("Office project created successfully with ID: {ProjectId}", project.Id);

            return await GetByIdAsync(project.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while creating office project: {@Project}", dto);
            throw;
        }
    }

    /// <summary>
    /// Update project (UC-7.8: Update Project Details)
    /// </summary>
    public async Task<OfficeProjectDetailDto> UpdateProjectAsync(Guid id, UpdateOfficeProjectDto dto)
    {
        try
        {
            var project = await _projectRepository.GetByIdAsync(id);
            if (project == null)
            {
                throw new InvalidOperationException($"Office project with ID {id} not found");
            }

            // Validate that project is not finished
            if (project.IsFinished)
            {
                throw new InvalidOperationException("Cannot update a completed project");
            }

            // Validate location cascade
            if (dto.FK_CenterId.HasValue && !dto.FK_RegionId.HasValue)
            {
                throw new InvalidOperationException("Region must be specified when Center is selected");
            }

            if (dto.FK_RegionId.HasValue && !dto.FK_CountryId.HasValue)
            {
                throw new InvalidOperationException("Country must be specified when Region is selected");
            }

            // Update only non-null properties
            if (!string.IsNullOrWhiteSpace(dto.ProjectName))
                project.ProjectName = dto.ProjectName;

            if (dto.ProjectHint != null)
                project.ProjectHint = dto.ProjectHint;

            if (dto.ProjectDate.HasValue)
                project.ProjectDate = dto.ProjectDate.Value;

            if (dto.ProjectEndDate.HasValue)
                project.ProjectEndDate = dto.ProjectEndDate.Value;

            if (dto.IsFinished.HasValue)
                project.IsFinished = dto.IsFinished.Value;

            if (dto.FK_OfficeProjectTypeId.HasValue)
                project.FK_OfficeProjectTypeId = dto.FK_OfficeProjectTypeId.Value;

            if (dto.FK_CountryId.HasValue)
                project.FK_CountryId = dto.FK_CountryId.Value;

            if (dto.FK_RegionId.HasValue)
                project.FK_RegionId = dto.FK_RegionId.Value;

            if (dto.FK_CenterId.HasValue)
                project.FK_CenterId = dto.FK_CenterId.Value;

            if (dto.VillageName != null)
                project.VillageName = dto.VillageName;

            if (dto.ProjectCostEGP.HasValue)
                project.ProjectCostEGP = dto.ProjectCostEGP.Value;

            if (dto.ProjectCostSAR.HasValue)
                project.ProjectCostSAR = dto.ProjectCostSAR.Value;

            if (dto.DonorName != null)
                project.DonorName = dto.DonorName;

            if (dto.BeneficiariesCount.HasValue)
                project.BeneficiariesCount = dto.BeneficiariesCount.Value;

            if (dto.BeneficiariesType != null)
                project.BeneficiariesType = dto.BeneficiariesType;

            if (dto.FK_CharityId.HasValue)
                project.FK_CharityId = dto.FK_CharityId.Value;

            // Process document attachment
            if (dto.Document_Attach != null && dto.Document_Attach.Any())
            {
                var newAttachment = dto.Document_Attach.FirstOrDefault(a => !string.IsNullOrEmpty(a.FileName) && a.FileData != null && !a.IsDeleted && a.IsNew);
                var deletedAttachment = dto.Document_Attach.FirstOrDefault(a => a.IsDeleted);

                if (deletedAttachment != null && project.FK_AttachedFileId.HasValue)
                {
                    // Delete old attachment
                    await _attachmentService.RemoveAsync(project.FK_AttachedFileId.Value);
                    project.FK_AttachedFileId = null;
                }

                if (newAttachment != null)
                {
                    // Save new attachment
                    project.FK_AttachedFileId = await _attachmentHelperService.SaveAttachmentAsync(newAttachment, project.FK_AttachedFileId);
                }
            }

            // Process report attachment
            if (dto.Report_Attach != null && dto.Report_Attach.Any())
            {
                var newAttachment = dto.Report_Attach.FirstOrDefault(a => !string.IsNullOrEmpty(a.FileName) && a.FileData != null && !a.IsDeleted && a.IsNew);
                var deletedAttachment = dto.Report_Attach.FirstOrDefault(a => a.IsDeleted);

                if (deletedAttachment != null && project.FK_ProjectReportFileId.HasValue)
                {
                    // Delete old attachment
                    await _attachmentService.RemoveAsync(project.FK_ProjectReportFileId.Value);
                    project.FK_ProjectReportFileId = null;
                }

                if (newAttachment != null)
                {
                    // Save new attachment
                    project.FK_ProjectReportFileId = await _attachmentHelperService.SaveAttachmentAsync(newAttachment, project.FK_ProjectReportFileId);
                }
            }

            if (dto.Notes != null)
                project.Notes = dto.Notes;

            _projectRepository.Update(project);
            await _projectRepository.SaveChangesAsync();

            _logger.LogInformation("Office project updated successfully: {ProjectId}", id);

            return await GetByIdAsync(project.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating office project {ProjectId}: {@Project}", id, dto);
            throw;
        }
    }

    /// <summary>
    /// Delete project
    /// </summary>
    public async Task DeleteProjectAsync(Guid id)
    {
        try
        {
            var project = await _projectRepository.GetByIdAsync(id);
            if (project == null)
            {
                throw new InvalidOperationException($"Office project with ID {id} not found");
            }

            // Validate that project is not finished
            if (project.IsFinished)
            {
                throw new InvalidOperationException("Cannot delete a completed project");
            }

            _projectRepository.Delete(project);
            await _projectRepository.SaveChangesAsync();

            _logger.LogInformation("Office project deleted successfully: {ProjectId}", id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while deleting office project {ProjectId}", id);
            throw;
        }
    }

    // ========== Project-Specific Operations ==========

    /// <summary>
    /// Set project budget (UC-7.2: Set Project Budget)
    /// </summary>
    public async Task SetProjectBudgetAsync(Guid id, SetProjectBudgetDto dto)
    {
        try
        {
            var project = await _projectRepository.GetByIdAsync(id);
            if (project == null)
            {
                throw new InvalidOperationException($"Office project with ID {id} not found");
            }

            if (project.IsFinished)
            {
                throw new InvalidOperationException("Cannot modify a completed project");
            }

            project.ProjectCostEGP = dto.ProjectCostEGP;
            project.ProjectCostSAR = dto.ProjectCostSAR;

            _projectRepository.Update(project);
            await _projectRepository.SaveChangesAsync();

            _logger.LogInformation("Project budget updated for project {ProjectId}: EGP={EGP}, SAR={SAR}", id, dto.ProjectCostEGP, dto.ProjectCostSAR);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while setting project budget for project {ProjectId}", id);
            throw;
        }
    }

    /// <summary>
    /// Specify project donor (UC-7.3: Specify Project Donor)
    /// </summary>
    public async Task SpecifyProjectDonorAsync(Guid id, SpecifyProjectDonorDto dto)
    {
        try
        {
            var project = await _projectRepository.GetByIdAsync(id);
            if (project == null)
            {
                throw new InvalidOperationException($"Office project with ID {id} not found");
            }

            if (project.IsFinished)
            {
                throw new InvalidOperationException("Cannot modify a completed project");
            }

            project.DonorName = dto.DonorName;

            _projectRepository.Update(project);
            await _projectRepository.SaveChangesAsync();

            _logger.LogInformation("Project donor specified for project {ProjectId}: {DonorName}", id, dto.DonorName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while specifying project donor for project {ProjectId}", id);
            throw;
        }
    }

    /// <summary>
    /// Set beneficiaries count (UC-7.4: Set Beneficiaries Count)
    /// </summary>
    public async Task SetBeneficiariesCountAsync(Guid id, SetBeneficiariesCountDto dto)
    {
        try
        {
            var project = await _projectRepository.GetByIdAsync(id);
            if (project == null)
            {
                throw new InvalidOperationException($"Office project with ID {id} not found");
            }

            if (project.IsFinished)
            {
                throw new InvalidOperationException("Cannot modify a completed project");
            }

            project.BeneficiariesCount = dto.BeneficiariesCount;
            project.BeneficiariesType = dto.BeneficiariesType;

            _projectRepository.Update(project);
            await _projectRepository.SaveChangesAsync();

            _logger.LogInformation("Beneficiaries count set for project {ProjectId}: {Count} of {Type}", id, dto.BeneficiariesCount, dto.BeneficiariesType);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while setting beneficiaries count for project {ProjectId}", id);
            throw;
        }
    }

    /// <summary>
    /// Assign project location (UC-7.5: Assign Project Location)
    /// </summary>
    public async Task AssignProjectLocationAsync(Guid id, AssignProjectLocationDto dto)
    {
        try
        {
            var project = await _projectRepository.GetByIdAsync(id);
            if (project == null)
            {
                throw new InvalidOperationException($"Office project with ID {id} not found");
            }

            if (project.IsFinished)
            {
                throw new InvalidOperationException("Cannot modify a completed project");
            }

            // Validate location cascade
            if (dto.FK_CenterId.HasValue && !dto.FK_RegionId.HasValue)
            {
                throw new InvalidOperationException("Region must be specified when Center is selected");
            }

            if (dto.FK_RegionId.HasValue && !dto.FK_CountryId.HasValue)
            {
                throw new InvalidOperationException("Country must be specified when Region is selected");
            }

            project.FK_CountryId = dto.FK_CountryId;
            project.FK_RegionId = dto.FK_RegionId;
            project.FK_CenterId = dto.FK_CenterId;
            project.VillageName = dto.VillageName;

            _projectRepository.Update(project);
            await _projectRepository.SaveChangesAsync();

            _logger.LogInformation("Project location updated for project {ProjectId}", id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while assigning project location for project {ProjectId}", id);
            throw;
        }
    }

    /// <summary>
    /// Attach project document (UC-7.6: Attach Project Documents)
    /// </summary>
    public async Task AttachProjectDocumentAsync(Guid id, AttachProjectDocumentDto dto)
    {
        try
        {
            var project = await _projectRepository.GetByIdAsync(id);
            if (project == null)
            {
                throw new InvalidOperationException($"Office project with ID {id} not found");
            }

            // Allow document attachment even for finished projects

            project.FK_AttachedFileId = dto.FK_AttachedFileId;

            _projectRepository.Update(project);
            await _projectRepository.SaveChangesAsync();

            _logger.LogInformation("Project document attached for project {ProjectId}: DocumentType={DocumentType}", id, dto.DocumentType);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while attaching project document for project {ProjectId}", id);
            throw;
        }
    }

    /// <summary>
    /// Upload project report (UC-7.7: Upload Project Report)
    /// </summary>
    public async Task UploadProjectReportAsync(Guid id, UploadProjectReportDto dto)
    {
        try
        {
            var project = await _projectRepository.GetByIdAsync(id);
            if (project == null)
            {
                throw new InvalidOperationException($"Office project with ID {id} not found");
            }

            // Allow report upload even for finished projects

            project.FK_ProjectReportFileId = dto.FK_ProjectReportFileId;

            _projectRepository.Update(project);
            await _projectRepository.SaveChangesAsync();

            _logger.LogInformation("Project report uploaded for project {ProjectId}: ReportType={ReportType}", id, dto.ReportType);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while uploading project report for project {ProjectId}", id);
            throw;
        }
    }

    /// <summary>
    /// Mark project as completed (UC-7.9: Mark Project as Completed)
    /// </summary>
    public async Task MarkProjectAsCompletedAsync(Guid id, MarkProjectCompletedDto dto)
    {
        try
        {
            var project = await _projectRepository.GetByIdAsync(id);
            if (project == null)
            {
                throw new InvalidOperationException($"Office project with ID {id} not found");
            }

            if (project.IsFinished)
            {
                throw new InvalidOperationException("Project is already completed");
            }

            project.IsFinished = dto.IsFinished;
            project.ProjectEndDate = dto.ProjectEndDate;

            _projectRepository.Update(project);
            await _projectRepository.SaveChangesAsync();

            _logger.LogInformation("Project marked as completed: {ProjectId}", id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while marking project as completed {ProjectId}", id);
            throw;
        }
    }

    /// <summary>
    /// Set project dates (UC-7.13: Set Project Dates)
    /// </summary>
    public async Task SetProjectDatesAsync(Guid id, SetProjectDatesDto dto)
    {
        try
        {
            var project = await _projectRepository.GetByIdAsync(id);
            if (project == null)
            {
                throw new InvalidOperationException($"Office project with ID {id} not found");
            }

            if (project.IsFinished)
            {
                throw new InvalidOperationException("Cannot modify a completed project");
            }

            project.ProjectDate = dto.ProjectDate;
            project.ProjectEndDate = dto.ProjectEndDate;

            _projectRepository.Update(project);
            await _projectRepository.SaveChangesAsync();

            _logger.LogInformation("Project dates updated for project {ProjectId}: Start={StartDate}, End={EndDate}", id, dto.ProjectDate, dto.ProjectEndDate);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while setting project dates for project {ProjectId}", id);
            throw;
        }
    }

    /// <summary>
    /// Assign project to charity (UC-7.14: Assign Project to Charity)
    /// </summary>
    public async Task AssignProjectToCharityAsync(Guid id, AssignProjectToCharityDto dto)
    {
        try
        {
            var project = await _projectRepository.GetByIdAsync(id);
            if (project == null)
            {
                throw new InvalidOperationException($"Office project with ID {id} not found");
            }

            var previousCharityId = project.FK_CharityId;
            project.FK_CharityId = dto.FK_CharityId;

            _projectRepository.Update(project);
            await _projectRepository.SaveChangesAsync();

            _logger.LogInformation("Project assigned to charity for project {ProjectId}: CharityId={CharityId}", id, dto.FK_CharityId);

            // TODO: Send notification to charity
            _logger.LogInformation("TODO: Send notification to charity {CharityId} for project assignment", dto.FK_CharityId);

            // Notify previous charity if changed
            if (previousCharityId.HasValue && previousCharityId.Value != dto.FK_CharityId)
            {
                _logger.LogInformation("TODO: Send notification to previous charity {CharityId} for project reassignment", previousCharityId.Value);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while assigning project to charity for project {ProjectId}", id);
            throw;
        }
    }

    // ========== View Operations ==========

    /// <summary>
    /// Get projects assigned to a specific charity (UC-7.14)
    /// </summary>
    public async Task<OfficeProjectPagedResult<OfficeProjectListDto>> GetProjectsByCharityAsync(Guid charityId, OfficeProjectFilterDto filter)
    {
        try
        {
            _logger.LogInformation("Retrieving office projects for charity {CharityId}", charityId);

            filter.FK_CharityId = charityId;
            return await GetProjectsFilteredAsync(filter);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving office projects for charity {CharityId}", charityId);
            throw;
        }
    }

    /// <summary>
    /// Get project status summary (UC-7.12: Track Project Progress)
    /// </summary>
    public async Task<OfficeProjectStatusSummaryDto> GetProjectStatusSummaryAsync()
    {
        try
        {
            var allProjects = await _projectRepository.GetAllAsync();

            var summary = new OfficeProjectStatusSummaryDto
            {
                OngoingCount = allProjects.Count(p => !p.IsFinished),
                CompletedCount = allProjects.Count(p => p.IsFinished),
                TotalCount = allProjects.Count(),
                TotalCostEGP = allProjects.Where(p => p.ProjectCostEGP.HasValue).Sum(p => p.ProjectCostEGP),
                TotalCostSAR = allProjects.Where(p => p.ProjectCostSAR.HasValue).Sum(p => p.ProjectCostSAR),
                TotalBeneficiaries = allProjects.Where(p => p.BeneficiariesCount.HasValue).Sum(p => p.BeneficiariesCount)
            };

            _logger.LogInformation("Office project status summary retrieved: {@Summary}", summary);

            return summary;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving office project status summary");
            throw;
        }
    }

    /// <summary>
    /// Get ongoing projects
    /// </summary>
    public async Task<List<OfficeProjectListDto>> GetOngoingProjectsAsync()
    {
        try
        {
            var ongoingProjects = await _projectRepository.GetOngoingProjectsAsync();
            return _mapper.Map<List<OfficeProjectListDto>>(ongoingProjects);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving ongoing office projects");
            throw;
        }
    }

    /// <summary>
    /// Get completed projects
    /// </summary>
    public async Task<List<OfficeProjectListDto>> GetCompletedProjectsAsync()
    {
        try
        {
            var completedProjects = await _projectRepository.GetCompletedProjectsAsync();
            return _mapper.Map<List<OfficeProjectListDto>>(completedProjects);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving completed office projects");
            throw;
        }
    }

    // ========== Export ==========

    /// <summary>
    /// Export projects to Excel (UC-7.10: Export to Excel)
    /// </summary>
    public async Task<byte[]> ExportProjectsToExcelAsync(OfficeProjectFilterDto filter)
    {
        try
        {
            _logger.LogInformation("Exporting office projects to Excel with filter: {@Filter}", filter);

            // Get all projects (without pagination for export)
            var exportFilter = filter ?? new OfficeProjectFilterDto();
            exportFilter.Page = 1;
            exportFilter.PageSize = int.MaxValue;

            var result = await GetProjectsFilteredAsync(exportFilter);

            using (var package = new OfficeOpenXml.ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Office Projects");

                // Add headers
                worksheet.Cells[1, 1].Value = "Project Name";
                worksheet.Cells[1, 2].Value = "Project Date";
                worksheet.Cells[1, 3].Value = "Project Type";
                worksheet.Cells[1, 4].Value = "Country";
                worksheet.Cells[1, 5].Value = "Region";
                worksheet.Cells[1, 6].Value = "Center";
                worksheet.Cells[1, 7].Value = "Village";
                worksheet.Cells[1, 8].Value = "Cost (EGP)";
                worksheet.Cells[1, 9].Value = "Cost (SAR)";
                worksheet.Cells[1, 10].Value = "Donor";
                worksheet.Cells[1, 11].Value = "Beneficiaries";
                worksheet.Cells[1, 12].Value = "Charity";
                worksheet.Cells[1, 13].Value = "Status";

                // Style header row
                using (var range = worksheet.Cells[1, 1, 1, 13])
                {
                    range.Style.Font.Bold = true;
                    range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                }

                // Add data rows
                int row = 2;
                foreach (var project in result.Items)
                {
                    worksheet.Cells[row, 1].Value = project.ProjectName;
                    worksheet.Cells[row, 2].Value = project.ProjectDate.ToString("yyyy-MM-dd");
                    worksheet.Cells[row, 3].Value = project.ProjectType ?? "";
                    worksheet.Cells[row, 4].Value = project.CountryName ?? "";
                    worksheet.Cells[row, 5].Value = project.Region ?? "";
                    worksheet.Cells[row, 6].Value = project.Center ?? "";
                    worksheet.Cells[row, 7].Value = project.Village ?? "";
                    worksheet.Cells[row, 8].Value = project.ProjectCostEGP?.ToString("F2") ?? "";
                    worksheet.Cells[row, 9].Value = project.ProjectCostSAR?.ToString("F2") ?? "";
                    worksheet.Cells[row, 10].Value = project.DonorName ?? "";
                    worksheet.Cells[row, 11].Value = project.BeneficiariesCount?.ToString() ?? "";
                    worksheet.Cells[row, 12].Value = project.AssignedCharity ?? "";
                    worksheet.Cells[row, 13].Value = project.IsFinished ? "Completed" : "Ongoing";
                    row++;
                }

                // Auto-fit columns
                worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

                return package.GetAsByteArray();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while exporting office projects to Excel");
            throw;
        }
    }

    #region Private Helper Methods

    /// <summary>
    /// Private helper method to get project by ID with attachments loaded
    /// </summary>
    private async Task<OfficeProjectDetailDto?> GetByIdAsync(Guid id)
    {
        var project = await _projectRepository.GetByIdAsync(id);
        if (project == null) return null;

        var dto = _mapper.Map<OfficeProjectDetailDto>(project);

        // Load document attachment if FK_AttachedFileId exists
        if (project.FK_AttachedFileId.HasValue)
        {
            var attachments = await _attachmentService.GetAttachmentAsync(new List<Guid> { project.FK_AttachedFileId.Value });
            if (attachments != null && attachments.Any())
            {
                dto.Document_Attach = attachments.Select(a => new Framework.Core.SharedServices.Dto.AttachmentDto
                {
                    Id = a.Id,
                    FileName = a.FileName,
                    ContentType = a.ContentType,
                    FilePath = a.FilePath,
                    Extension = a.Extension.TrimStart('.'),
                    FileData = a.AttachmentContent.FileContent
                }).ToList();
            }
        }

        // Load report attachment if FK_ProjectReportFileId exists
        if (project.FK_ProjectReportFileId.HasValue)
        {
            var attachments = await _attachmentService.GetAttachmentAsync(new List<Guid> { project.FK_ProjectReportFileId.Value });
            if (attachments != null && attachments.Any())
            {
                dto.Report_Attach = attachments.Select(a => new Framework.Core.SharedServices.Dto.AttachmentDto
                {
                    Id = a.Id,
                    FileName = a.FileName,
                    ContentType = a.ContentType,
                    FilePath = a.FilePath,
                    Extension = a.Extension.TrimStart('.'),
                    FileData = a.AttachmentContent.FileContent
                }).ToList();
            }
        }

        return dto;
    }

    #endregion
}
