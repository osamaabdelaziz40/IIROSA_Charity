using IIROSA.Application.DTOs.OfficeProjectManagement;
using IIROSA.Application.Interfaces;
using IIROSA.Domain.Entities;
using IIROSA.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using AutoMapper;
using FluentValidation;
using Framework.Core.SharedServices.Services;

namespace IIROSA.Application.Services;

/// <summary>
/// OfficeProject Service Implementation (UC-OFP-01…06)
///
/// Head-office module: only Admin and Super Admin reach it. The caller's country claim, when
/// present, scopes every read and write — a pinned caller cannot enumerate, edit or delete
/// another country's projects by omitting the filter, and the single-record paths treat an
/// out-of-scope row exactly like a missing one.
/// </summary>
public class OfficeProjectService : IOfficeProjectService
{
    private readonly ICharityWriteGuard _charityWriteGuard;
    private readonly IOfficeProjectRepository _projectRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<OfficeProjectService> _logger;
    private readonly AttachmentService _attachmentService;
    private readonly IAttachmentHelperService _attachmentHelperService;
    private readonly ICurrentUserService _currentUser;
    private readonly IValidator<CreateOfficeProjectDto> _createValidator;
    private readonly IValidator<UpdateOfficeProjectDto> _updateValidator;

    public OfficeProjectService(
        ICharityWriteGuard charityWriteGuard,
        IOfficeProjectRepository projectRepository,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<OfficeProjectService> logger,
        AttachmentService attachmentService,
        IAttachmentHelperService attachmentHelperService,
        ICurrentUserService currentUser,
        IValidator<CreateOfficeProjectDto> createValidator,
        IValidator<UpdateOfficeProjectDto> updateValidator)
    {
        _charityWriteGuard = charityWriteGuard;
        _projectRepository = projectRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
        _attachmentService = attachmentService;
        _attachmentHelperService = attachmentHelperService;
        _currentUser = currentUser;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    // ========== Reads (UC-OFP-01, UC-OFP-04) ==========

    /// <summary>
    /// Get projects with filtering and pagination (UC-OFP-01: list)
    /// </summary>
    public async Task<OfficeProjectPagedResult<OfficeProjectListDto>> GetProjectsFilteredAsync(OfficeProjectFilterDto filter)
    {
        try
        {
            filter ??= new OfficeProjectFilterDto();
            ApplyCallerScope(filter);

            _logger.LogInformation("Retrieving office projects with filter: {@Filter}", filter);

            var filterExpression = BuildFilterExpression(filter);

            var (items, totalCount) = await _projectRepository.GetProjectsPagedAsync(
                filterExpression,
                q => q.OrderByDescending(p => p.ProjectDate),
                filter.Page,
                filter.PageSize);

            var projectDtos = _mapper.Map<List<OfficeProjectListDto>>(items);

            return new OfficeProjectPagedResult<OfficeProjectListDto>
            {
                Items = projectDtos,
                TotalCount = totalCount,
                Page = filter.Page,
                PageSize = filter.PageSize
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving office projects with filter: {@Filter}", filter);
            throw;
        }
    }

    /// <summary>
    /// Register statistics for the band above the projects grid (UC-OFP-01).
    /// </summary>
    /// <remarks>
    /// Deliberately not filter-reactive: the band describes the caller's whole register,
    /// not the current search. The scope rides <see cref="ApplyCallerScope"/> with a blank
    /// filter — the same country pin the list read applies, so the numbers are exactly
    /// what the grid under it would show on page one.
    /// </remarks>
    public async Task<ProjectStatisticsDto> GetStatisticsAsync()
    {
        try
        {
            _logger.LogInformation("Getting office project register statistics");

            var scoped = new OfficeProjectFilterDto();
            ApplyCallerScope(scoped);

            var now = DateTime.UtcNow;
            var yearStart = new DateTime(now.Year, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var monthStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);

            var query = _projectRepository.TableNoTracking.Where(p => !p.IsDeleted);
            if (scoped.CountryId.HasValue)
            {
                query = query.Where(p => p.FK_CountryId == scoped.CountryId.Value);
            }

            // One grouped round-trip for every scalar card; null when the scope matches no rows.
            var totals = await query
                .GroupBy(_ => 1)
                .Select(g => new
                {
                    Total = g.Count(),
                    Completed = g.Count(p => p.IsFinished),
                    ThisYear = g.Count(p => p.ProjectDate >= yearStart),
                    AddedThisMonth = g.Count(p => p.CreatedOn >= monthStart)
                })
                .FirstOrDefaultAsync();

            return new ProjectStatisticsDto
            {
                Total = totals?.Total ?? 0,
                Completed = totals?.Completed ?? 0,
                ThisYear = totals?.ThisYear ?? 0,
                AddedThisMonth = totals?.AddedThisMonth ?? 0
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving office project register statistics");
            throw;
        }
    }

    /// <summary>
    /// Get project by ID with navigations and attachments (UC-OFP-04: view)
    /// </summary>
    public async Task<OfficeProjectDetailDto?> GetProjectByIdAsync(Guid id)
    {
        try
        {
            var project = await _projectRepository.GetByIdWithDetailsAsync(id);
            if (project == null || !IsWithinCallerScope(project))
            {
                return null;
            }

            return await MapDetailWithAttachmentsAsync(project);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving office project detail for ID: {ProjectId}", id);
            throw;
        }
    }

    // ========== Writes (UC-OFP-03, UC-OFP-04, UC-OFP-05) ==========

    /// <summary>
    /// Create new project (UC-OFP-03)
    /// </summary>
    public async Task<OfficeProjectDetailDto> CreateProjectAsync(CreateOfficeProjectDto dto)
    {
        // UC-CHR-07/08/09: head office can lock a charity or withdraw its add/edit rights.
        await _charityWriteGuard.EnsureCanAddAsync();

        await _createValidator.ValidateAndThrowAsync(dto);

        try
        {
            _logger.LogInformation("Creating new office project: {@Project}", dto);

            var project = _mapper.Map<OfficeProject>(dto);

            // The record's country belongs to the caller when their token pins one: default it
            // when omitted, pin it when another country was asked for.
            if (_currentUser.CountryId.HasValue)
            {
                if (dto.CountryId.HasValue && dto.CountryId.Value != _currentUser.CountryId.Value)
                {
                    _logger.LogWarning(
                        "Caller pinned to country {CallerCountry} tried to file a project under country {RequestedCountry}; pinning to {CallerCountry}",
                        _currentUser.CountryId.Value, dto.CountryId.Value, _currentUser.CountryId.Value);
                }
                project.FK_CountryId = _currentUser.CountryId.Value;
            }

            project.FK_AttachedFileId = await SaveFirstAttachmentAsync(dto.Document_Attach);
            project.FK_ProjectReportFileId = await SaveFirstAttachmentAsync(dto.Report_Attach);

            await _projectRepository.AddAsync(project);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Office project created successfully with ID: {ProjectId}", project.Id);

            return (await GetProjectByIdAsync(project.Id))!;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while creating office project: {@Project}", dto);
            throw;
        }
    }

    /// <summary>
    /// Update project (UC-OFP-04). Null fields are left unchanged; a finished project can still
    /// be corrected — §18.U.04 makes completion a data field, not a lock.
    /// </summary>
    public async Task<OfficeProjectDetailDto> UpdateProjectAsync(Guid id, UpdateOfficeProjectDto dto)
    {
        // UC-CHR-07/08/09: head office can lock a charity or withdraw its add/edit rights.
        await _charityWriteGuard.EnsureCanUpdateAsync();

        await _updateValidator.ValidateAndThrowAsync(dto);

        try
        {
            var project = await _projectRepository.GetByIdAsync(id);
            if (project == null || !IsWithinCallerScope(project))
            {
                throw new InvalidOperationException($"Office project with ID {id} not found");
            }

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

            if (dto.OfficeProjectTypeId.HasValue)
                project.FK_OfficeProjectTypeId = dto.OfficeProjectTypeId.Value;

            // Same country rule as create: a pinned caller cannot move a record out of their
            // country, and the cascade (region needs country) is enforced by the validator.
            if (dto.CountryId.HasValue)
                project.FK_CountryId = _currentUser.CountryId ?? dto.CountryId.Value;

            if (dto.RegionId.HasValue)
                project.FK_RegionId = dto.RegionId.Value;

            if (dto.CenterId.HasValue)
                project.FK_CenterId = dto.CenterId.Value;

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

            if (dto.CharityId.HasValue)
                project.FK_CharityId = dto.CharityId.Value;

            if (dto.Notes != null)
                project.Notes = dto.Notes;

            project.FK_AttachedFileId = await ApplyAttachmentChangeAsync(
                dto.Document_Attach, project.FK_AttachedFileId);
            project.FK_ProjectReportFileId = await ApplyAttachmentChangeAsync(
                dto.Report_Attach, project.FK_ProjectReportFileId);

            _projectRepository.Update(project);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Office project updated successfully: {ProjectId}", id);

            return (await GetProjectByIdAsync(project.Id))!;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating office project {ProjectId}: {@Project}", id, dto);
            throw;
        }
    }

    /// <summary>
    /// Delete project — soft delete (UC-OFP-05). A finished project entered in error is
    /// deletable like any other; §18.U.05 has no completion guard.
    /// </summary>
    public async Task DeleteProjectAsync(Guid id)
    {
        try
        {
            var project = await _projectRepository.GetByIdAsync(id);
            if (project == null || !IsWithinCallerScope(project))
            {
                throw new InvalidOperationException($"Office project with ID {id} not found");
            }

            _projectRepository.Delete(project);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Office project deleted successfully: {ProjectId}", id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while deleting office project {ProjectId}", id);
            throw;
        }
    }

    /// <summary>
    /// Mark project as completed (module completion tracking, feeds the progress view)
    /// </summary>
    public async Task MarkProjectAsCompletedAsync(Guid id, MarkProjectCompletedDto dto)
    {
        try
        {
            var project = await _projectRepository.GetByIdAsync(id);
            if (project == null || !IsWithinCallerScope(project))
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
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Project marked as completed: {ProjectId}", id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while marking project as completed {ProjectId}", id);
            throw;
        }
    }

    // ========== Report (UC-OFP-06) ==========

    /// <summary>
    /// Export projects to Excel (UC-OFP-06). Columns follow §18.S.1: الرقم، اسم المشروع، اسم
    /// المتبرع، التكلفه بالجنيه، التكلفه بالريال، الجمعيه. The caller's country scope applies —
    /// the report cannot be used to read around it.
    /// </summary>
    public async Task<byte[]> ExportProjectsToExcelAsync(OfficeProjectFilterDto filter)
    {
        try
        {
            _logger.LogInformation("Exporting office projects to Excel with filter: {@Filter}", filter);

            var exportFilter = filter ?? new OfficeProjectFilterDto();
            exportFilter.Page = 1;
            exportFilter.PageSize = int.MaxValue;

            var result = await GetProjectsFilteredAsync(exportFilter);

            using (var package = new OfficeOpenXml.ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("المشاريع التنموية");

                worksheet.Cells[1, 1].Value = "الرقم";
                worksheet.Cells[1, 2].Value = "اسم المشروع";
                worksheet.Cells[1, 3].Value = "اسم المتبرع";
                worksheet.Cells[1, 4].Value = "التكلفه بالجنيه";
                worksheet.Cells[1, 5].Value = "التكلفه بالريال";
                worksheet.Cells[1, 6].Value = "الجمعيه";

                using (var range = worksheet.Cells[1, 1, 1, 6])
                {
                    range.Style.Font.Bold = true;
                    range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                }

                var row = 2;
                var serial = 1;
                foreach (var project in result.Items)
                {
                    worksheet.Cells[row, 1].Value = serial++;
                    worksheet.Cells[row, 2].Value = project.ProjectName;
                    worksheet.Cells[row, 3].Value = project.DonorName ?? "";
                    worksheet.Cells[row, 4].Value = project.ProjectCostEGP;
                    worksheet.Cells[row, 5].Value = project.ProjectCostSAR;
                    worksheet.Cells[row, 6].Value = project.AssignedCharity ?? "";
                    row++;
                }

                worksheet.Cells[1, 1, row - 1, 6].AutoFitColumns();

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
    /// Pins the filter's country to the caller's country claim. The module is head-office only;
    /// a caller whose token carries no country claim reads every country.
    /// </summary>
    private void ApplyCallerScope(OfficeProjectFilterDto filter)
    {
        var callerCountry = _currentUser.CountryId;
        if (!callerCountry.HasValue)
        {
            return;
        }

        if (filter.CountryId.HasValue && filter.CountryId.Value != callerCountry.Value)
        {
            _logger.LogWarning(
                "Caller pinned to country {CallerCountry} requested country {RequestedCountry}; pinning to {CallerCountry}",
                callerCountry.Value, filter.CountryId.Value, callerCountry.Value);
        }

        filter.CountryId = callerCountry;
    }

    /// <summary>
    /// Whether the record is visible to the caller: a caller without a country claim sees
    /// everything; a pinned caller sees only their own country's rows.
    /// </summary>
    private bool IsWithinCallerScope(OfficeProject project)
    {
        var callerCountry = _currentUser.CountryId;
        return !callerCountry.HasValue || project.FK_CountryId == callerCountry.Value;
    }

    private static System.Linq.Expressions.Expression<Func<OfficeProject, bool>>? BuildFilterExpression(OfficeProjectFilterDto filter)
    {
        return p =>
            (!filter.OfficeProjectTypeId.HasValue || p.FK_OfficeProjectTypeId == filter.OfficeProjectTypeId.Value) &&
            (!filter.CountryId.HasValue || p.FK_CountryId == filter.CountryId.Value) &&
            (!filter.RegionId.HasValue || p.FK_RegionId == filter.RegionId.Value) &&
            (!filter.CenterId.HasValue || p.FK_CenterId == filter.CenterId.Value) &&
            (!filter.CharityId.HasValue || p.FK_CharityId == filter.CharityId.Value) &&
            (!filter.IsFinished.HasValue || p.IsFinished == filter.IsFinished.Value) &&
            (!filter.StartDate.HasValue || p.ProjectDate >= filter.StartDate.Value) &&
            (!filter.EndDate.HasValue || p.ProjectDate <= filter.EndDate.Value) &&
            (string.IsNullOrWhiteSpace(filter.DonorName) || p.DonorName != null && p.DonorName.Contains(filter.DonorName)) &&
            (string.IsNullOrWhiteSpace(filter.SearchText) || p.ProjectName.Contains(filter.SearchText));
    }

    /// <summary>
    /// Create path: persist the first real upload in the list, if any.
    /// </summary>
    private async Task<Guid?> SaveFirstAttachmentAsync(List<Framework.Core.SharedServices.Dto.AttachmentDto>? attachments)
    {
        var attachment = attachments?.FirstOrDefault(a => !string.IsNullOrEmpty(a.FileName) && a.FileData != null);
        return attachment == null
            ? null
            : await _attachmentHelperService.SaveAttachmentAsync(attachment);
    }

    /// <summary>
    /// Update path: honour an explicit delete of the current attachment, then a replacement
    /// upload, mirroring the shared attachment component's flags.
    /// </summary>
    private async Task<Guid?> ApplyAttachmentChangeAsync(
        List<Framework.Core.SharedServices.Dto.AttachmentDto>? attachments,
        Guid? currentFileId)
    {
        if (attachments == null || !attachments.Any())
        {
            return currentFileId;
        }

        if (attachments.Any(a => a.IsDeleted) && currentFileId.HasValue)
        {
            await _attachmentService.RemoveAsync(currentFileId.Value);
            currentFileId = null;
        }

        var newAttachment = attachments.FirstOrDefault(a =>
            !string.IsNullOrEmpty(a.FileName) && a.FileData != null && !a.IsDeleted && a.IsNew);

        if (newAttachment != null)
        {
            currentFileId = await _attachmentHelperService.SaveAttachmentAsync(newAttachment, currentFileId);
        }

        return currentFileId;
    }

    /// <summary>
    /// Map the detail DTO and load its two attachment lists from storage.
    /// </summary>
    private async Task<OfficeProjectDetailDto> MapDetailWithAttachmentsAsync(OfficeProject project)
    {
        var dto = _mapper.Map<OfficeProjectDetailDto>(project);

        if (project.FK_AttachedFileId.HasValue)
        {
            dto.Document_Attach = await LoadAttachmentListAsync(project.FK_AttachedFileId.Value);
        }

        if (project.FK_ProjectReportFileId.HasValue)
        {
            dto.Report_Attach = await LoadAttachmentListAsync(project.FK_ProjectReportFileId.Value);
        }

        return dto;
    }

    private async Task<List<Framework.Core.SharedServices.Dto.AttachmentDto>?> LoadAttachmentListAsync(Guid fileId)
    {
        var attachments = await _attachmentService.GetAttachmentAsync(new List<Guid> { fileId });
        if (attachments == null || !attachments.Any())
        {
            return null;
        }

        return attachments.Select(a => new Framework.Core.SharedServices.Dto.AttachmentDto
        {
            Id = a.Id,
            FileName = a.FileName,
            ContentType = a.ContentType,
            FilePath = a.FilePath,
            Extension = a.Extension.TrimStart('.'),
            FileData = a.AttachmentContent.FileContent
        }).ToList();
    }

    #endregion
}
