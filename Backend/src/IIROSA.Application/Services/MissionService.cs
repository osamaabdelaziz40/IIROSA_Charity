using IIROSA.Application.DTOs.MissionManagement;
using IIROSA.Application.Interfaces;
using IIROSA.Domain.Entities;
using IIROSA.Domain.Interfaces;
using Framework.Identity.Data.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using AutoMapper;
using FluentValidation;
using FluentValidation.Results;

namespace IIROSA.Application.Services;

/// <summary>
/// Mission Service Implementation
/// Business logic for Mission management (epic 15, UC-MSN-01…09)
///
/// Head-office module: only Admin and Super Admin reach it. The caller's country claim, when
/// present, scopes every read and write — a pinned caller cannot enumerate, edit or delete
/// another country's missions by omitting the filter, and the single-record paths treat an
/// out-of-scope row exactly like a missing one. Only the UnitOfWork saves; validators run in
/// this layer per the platform rule.
/// </summary>
public class MissionService : IMissionService
{
    /// <summary>Upper bound for the page size a client can request in one call.</summary>
    private const int MaxPageSize = 200;

    private readonly IMissionRepository _missionRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<MissionService> _logger;
    private readonly ICurrentUserService _currentUser;
    private readonly IValidator<CreateMissionDto> _createValidator;
    private readonly IValidator<UpdateMissionDto> _updateValidator;
    private readonly IValidator<RegisterMissionResultDto> _registerResultValidator;
    private readonly IMissionTypeRepository _missionTypeRepository;
    private readonly IMissionTimeTypeRepository _missionTimeTypeRepository;
    private readonly IMissionInterviewTypeRepository _missionInterviewTypeRepository;
    private readonly ICountryRepository _countryRepository;
    private readonly IRegionRepository _regionRepository;
    private readonly ICenterRepository _centerRepository;
    private readonly IUserAppServiceExtended _userAppService;

    public MissionService(
        IMissionRepository missionRepository,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<MissionService> logger,
        ICurrentUserService currentUser,
        IValidator<CreateMissionDto> createValidator,
        IValidator<UpdateMissionDto> updateValidator,
        IValidator<RegisterMissionResultDto> registerResultValidator,
        IMissionTypeRepository missionTypeRepository,
        IMissionTimeTypeRepository missionTimeTypeRepository,
        IMissionInterviewTypeRepository missionInterviewTypeRepository,
        ICountryRepository countryRepository,
        IRegionRepository regionRepository,
        ICenterRepository centerRepository,
        IUserAppServiceExtended userAppService)
    {
        _missionRepository = missionRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
        _currentUser = currentUser;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
        _registerResultValidator = registerResultValidator;
        _missionTypeRepository = missionTypeRepository;
        _missionTimeTypeRepository = missionTimeTypeRepository;
        _missionInterviewTypeRepository = missionInterviewTypeRepository;
        _countryRepository = countryRepository;
        _regionRepository = regionRepository;
        _centerRepository = centerRepository;
        _userAppService = userAppService;
    }

    // ========== Reads ==========

    /// <summary>
    /// Get missions with filtering and pagination (UC-MSN-01 / UC-MSN-02)
    /// </summary>
    public async Task<MissionPagedResult<MissionListDto>> GetMissionsFilteredAsync(MissionFilterDto filter)
    {
        try
        {
            filter ??= new MissionFilterDto();

            ApplyCallerScope(filter);

            // Paging bounds arrive from the wire — clamp before Skip/Take so a zero or
            // negative page cannot push Skip below zero, a zero page size cannot reach the
            // total-pages division, and no caller can ask for the whole table in one call.
            filter.Page = Math.Max(1, filter.Page);
            filter.PageSize = Math.Clamp(filter.PageSize, 1, MaxPageSize);

            _logger.LogInformation("Retrieving missions with filter: {@Filter}", filter);

            var filterExpression = BuildFilterExpression(filter);

            var (items, totalCount) = await _missionRepository.GetMissionsPagedAsync(
                filterExpression,
                q => q.OrderByDescending(m => m.MissionDate),
                filter.Page,
                filter.PageSize);

            var missionDtos = _mapper.Map<List<MissionListDto>>(items);

            return new MissionPagedResult<MissionListDto>
            {
                Items = missionDtos,
                TotalCount = totalCount,
                Page = filter.Page,
                PageSize = filter.PageSize
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving missions with filter: {@Filter}", filter);
            throw;
        }
    }

    /// <summary>
    /// Get mission by ID (UC-MSN-07 detail read) — with navigations and caller scope.
    /// An out-of-scope id returns null, exactly like a missing one.
    /// </summary>
    public async Task<MissionDetailDto?> GetMissionByIdAsync(Guid id)
    {
        try
        {
            var mission = await _missionRepository.IncludeNavigationProperties()
                .FirstOrDefaultAsync(m => m.Id == id);
            if (mission == null)
            {
                _logger.LogWarning("Mission with ID {MissionId} not found", id);
                return null;
            }

            if (!IsWithinCallerScope(mission))
            {
                _logger.LogWarning(
                    "Caller pinned to country {CallerCountry} requested mission {MissionId} of country {MissionCountry}; treating as not found",
                    _currentUser.CountryId, id, mission.FK_CountryId);
                return null;
            }

            return _mapper.Map<MissionDetailDto>(mission);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving mission detail for ID: {MissionId}", id);
            throw;
        }
    }

    /// <summary>
    /// The register read of §20.U.1: scoped to the caller's charity and country — NOT
    /// assigned-to-me. The assigned user stays an optional filter.
    /// </summary>
    public async Task<MissionPagedResult<MissionListDto>> GetMyMissionsAsync(MissionFilterDto filter)
    {
        return await GetMissionsFilteredAsync(filter);
    }

    /// <summary>
    /// Register statistics for the band above the missions grid (UC-MSN-01).
    /// </summary>
    /// <remarks>
    /// Deliberately not filter-reactive: the band describes the caller's whole register,
    /// not the current search. The scope rides <see cref="ApplyCallerScope"/> with a blank
    /// filter — the same country/charity pin the list read applies, so the numbers are
    /// exactly what the grid under it would show on page one.
    /// </remarks>
    public async Task<MissionStatisticsDto> GetStatisticsAsync()
    {
        try
        {
            _logger.LogInformation("Getting mission register statistics");

            var scoped = new MissionFilterDto();
            ApplyCallerScope(scoped);

            var monthStart = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1, 0, 0, 0, DateTimeKind.Utc);

            var query = _missionRepository.TableNoTracking.Where(m => !m.IsDeleted);
            if (scoped.CountryId.HasValue)
            {
                query = query.Where(m => m.FK_CountryId == scoped.CountryId.Value);
            }
            if (scoped.CharityId.HasValue)
            {
                query = query.Where(m => m.FK_CharityId == scoped.CharityId.Value);
            }

            // One grouped round-trip for every scalar card; null when the scope matches no rows.
            var totals = await query
                .GroupBy(_ => 1)
                .Select(g => new
                {
                    Total = g.Count(),
                    Completed = g.Count(m => m.IsMissionCompleted),
                    AddedThisMonth = g.Count(m => m.CreatedOn >= monthStart)
                })
                .FirstOrDefaultAsync();

            return new MissionStatisticsDto
            {
                Total = totals?.Total ?? 0,
                Completed = totals?.Completed ?? 0,
                InProgress = (totals?.Total ?? 0) - (totals?.Completed ?? 0),
                AddedThisMonth = totals?.AddedThisMonth ?? 0
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving mission register statistics");
            throw;
        }
    }

    // ========== Writes ==========

    /// <summary>
    /// Create new mission (UC-MSN-06)
    /// </summary>
    public async Task<MissionDetailDto> CreateMissionAsync(CreateMissionDto dto)
    {
        await _createValidator.ValidateAndThrowAsync(dto);
        await ValidateForeignKeysAsync(dto);

        try
        {
            _logger.LogInformation("Creating new mission: {@Mission}", dto);

            var mission = _mapper.Map<Mission>(dto);
            mission.IsMissionCompleted = false;

            // Ownership is stamped server-side from the caller's charity claim — never from
            // the payload. HQ callers without a claim save null.
            mission.FK_CharityId = _currentUser.CharityId;

            // The record's country belongs to the caller when their token pins one: pin it.
            if (_currentUser.CountryId.HasValue)
            {
                if (dto.CountryId.HasValue && dto.CountryId.Value != _currentUser.CountryId.Value)
                {
                    _logger.LogWarning(
                        "Caller pinned to country {CallerCountry} tried to file a mission under country {RequestedCountry}; pinning to {CallerCountry}",
                        _currentUser.CountryId.Value, dto.CountryId.Value, _currentUser.CountryId.Value);
                }
                mission.FK_CountryId = _currentUser.CountryId.Value;
            }

            await _missionRepository.AddAsync(mission);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Mission created successfully with ID: {MissionId}", mission.Id);

            // TODO: Send notification to assigned user
            // Notification integration to be implemented using INotificationsManager
            if (mission.FK_UserId.HasValue && mission.FK_UserId.Value != Guid.Empty)
            {
                _logger.LogInformation("TODO: Send notification to user {UserId} for mission {MissionId}", mission.FK_UserId, mission.Id);
            }

            return (await GetMissionByIdAsync(mission.Id))!;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while creating mission: {@Mission}", dto);
            throw;
        }
    }

    /// <summary>
    /// Update mission (UC-MSN-07) — a completed mission is immutable (the register entry
    /// is final; the sanctioned outcome path is UC-MSN-09).
    /// </summary>
    public async Task<MissionDetailDto> UpdateMissionAsync(Guid id, UpdateMissionDto dto)
    {
        await _updateValidator.ValidateAndThrowAsync(dto);

        try
        {
            var mission = await _missionRepository.GetByIdAsync(id);
            if (mission == null)
            {
                throw new InvalidOperationException($"Mission with ID {id} not found");
            }

            if (!IsWithinCallerScope(mission))
            {
                throw new InvalidOperationException($"Mission with ID {id} not found");
            }

            if (mission.IsMissionCompleted)
            {
                throw new InvalidOperationException("Cannot update a completed mission");
            }

            await ValidateForeignKeysAsync(dto);

            // Apply only non-null properties
            if (!string.IsNullOrWhiteSpace(dto.MissionTarget))
                mission.MissionTarget = dto.MissionTarget;

            if (dto.MissionDetails != null)
                mission.MissionDetails = dto.MissionDetails;

            if (dto.Details != null)
                mission.Details = dto.Details;

            // The date may not MOVE into the past — resaving an unchanged past date
            // (the form always submits it) stays allowed.
            if (dto.MissionDate.HasValue && dto.MissionDate.Value.Date != mission.MissionDate.Date)
            {
                if (dto.MissionDate.Value < DateTime.Today)
                {
                    throw new InvalidOperationException("Mission date cannot be in the past");
                }

                mission.MissionDate = dto.MissionDate.Value;
            }

            if (dto.MissionTypeId.HasValue)
                mission.FK_MissionTypeId = dto.MissionTypeId.Value;

            if (dto.MissionTimeTypeId.HasValue)
                mission.FK_MissionTimeTypeId = dto.MissionTimeTypeId.Value;

            if (dto.MissionInterviewTypeId.HasValue)
                mission.FK_MissionInterviewTypeId = dto.MissionInterviewTypeId.Value;

            // A pinned caller cannot move the record out of their country — the payload
            // value applies only to unscoped (HQ) callers.
            if (dto.CountryId.HasValue)
                mission.FK_CountryId = _currentUser.CountryId ?? dto.CountryId.Value;

            if (dto.RegionId.HasValue)
                mission.FK_RegionId = dto.RegionId.Value;

            if (dto.CenterId.HasValue)
                mission.FK_CenterId = dto.CenterId.Value;

            if (dto.MissionLocation != null)
                mission.MissionLocation = dto.MissionLocation;

            if (dto.Village != null)
                mission.Village = dto.Village;

            if (dto.EntityName != null)
                mission.EntityName = dto.EntityName;

            if (dto.ConferenceName != null)
                mission.ConferenceName = dto.ConferenceName;

            if (dto.AssignedToUserId.HasValue && dto.AssignedToUserId.Value != Guid.Empty)
                mission.FK_UserId = dto.AssignedToUserId.Value;

            _missionRepository.Update(mission);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Mission updated successfully: {MissionId}", id);

            return (await GetMissionByIdAsync(id))!;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating mission {MissionId}: {@Mission}", id, dto);
            throw;
        }
    }

    /// <summary>
    /// Delete mission (UC-MSN-08) — soft delete via the entity's IsDeleted machinery,
    /// saved through the UnitOfWork only.
    /// </summary>
    public async Task DeleteMissionAsync(Guid id)
    {
        try
        {
            var mission = await _missionRepository.GetByIdAsync(id);
            if (mission == null)
            {
                throw new InvalidOperationException($"Mission with ID {id} not found");
            }

            if (!IsWithinCallerScope(mission))
            {
                throw new InvalidOperationException($"Mission with ID {id} not found");
            }

            _missionRepository.Delete(mission);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Mission deleted successfully: {MissionId}", id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while deleting mission {MissionId}", id);
            throw;
        }
    }

    /// <summary>
    /// Register the mission result (UC-MSN-09, §20.S.3): the findings + the completion
    /// outcome + السبب. A mission whose result is already registered is immutable.
    /// Replaces the copied CompleteMission/RecordConferenceEntity pair.
    /// </summary>
    public async Task<MissionDetailDto> RegisterMissionResultAsync(Guid id, RegisterMissionResultDto dto)
    {
        await _registerResultValidator.ValidateAndThrowAsync(dto);

        try
        {
            var mission = await _missionRepository.GetByIdAsync(id);
            if (mission == null)
            {
                throw new InvalidOperationException($"Mission with ID {id} not found");
            }

            if (!IsWithinCallerScope(mission))
            {
                throw new InvalidOperationException($"Mission with ID {id} not found");
            }

            // The register entry is final once ANY outcome is recorded, completed or not —
            // IsMissionCompleted alone would let a not-completed result be re-registered
            // indefinitely.
            if (mission.IsMissionCompleted || !string.IsNullOrEmpty(mission.MissionCompletedTxt))
            {
                throw new InvalidOperationException("Mission result is already registered");
            }

            if (!string.IsNullOrWhiteSpace(dto.EntityName))
                mission.EntityName = dto.EntityName;

            if (!string.IsNullOrWhiteSpace(dto.ConferenceName))
                mission.ConferenceName = dto.ConferenceName;

            if (!string.IsNullOrWhiteSpace(dto.Details))
                mission.Details = dto.Details;

            if (!string.IsNullOrWhiteSpace(dto.MissionTarget))
                mission.MissionTarget = dto.MissionTarget;

            if (!string.IsNullOrWhiteSpace(dto.MissionDetails))
                mission.MissionDetails = dto.MissionDetails;

            if (!string.IsNullOrWhiteSpace(dto.MissionLocation))
                mission.MissionLocation = dto.MissionLocation;

            if (!string.IsNullOrWhiteSpace(dto.Village))
                mission.Village = dto.Village;

            if (dto.AssignedToUserId.HasValue && dto.AssignedToUserId.Value != Guid.Empty)
            {
                if (!await AssigneeExistsAsync(dto.AssignedToUserId.Value))
                {
                    throw new ValidationException(
                        new[] { new ValidationFailure(nameof(dto.AssignedToUserId), "Assigned user does not exist") });
                }

                mission.FK_UserId = dto.AssignedToUserId.Value;
            }

            mission.IsMissionCompleted = dto.IsCompleted!.Value;
            mission.MissionCompletedTxt = dto.Reason;
            // §20.S.3: the completion date belongs to the completed outcome — a
            // not-completed registration records the reason, never a completion date.
            if (dto.IsCompleted.Value)
            {
                mission.MissionCompletedDate = DateTime.UtcNow;
            }

            _missionRepository.Update(mission);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Mission result registered: {MissionId} (completed: {IsCompleted})", id, mission.IsMissionCompleted);

            return (await GetMissionByIdAsync(id))!;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while registering result for mission {MissionId}", id);
            throw;
        }
    }

    // ========== Legacy fine-grained operations (UC-8.x capability variants) ==========

    /// <summary>
    /// Set mission date (UC-8.2)
    /// </summary>
    public async Task SetMissionDateAsync(Guid id, DateTime missionDate)
    {
        try
        {
            var mission = await _missionRepository.GetByIdAsync(id);
            if (mission == null)
            {
                throw new InvalidOperationException($"Mission with ID {id} not found");
            }

            if (!IsWithinCallerScope(mission))
            {
                throw new InvalidOperationException($"Mission with ID {id} not found");
            }

            if (mission.IsMissionCompleted)
            {
                throw new InvalidOperationException("Cannot modify a completed mission");
            }

            if (missionDate < DateTime.Today)
            {
                throw new InvalidOperationException("Mission date cannot be in the past");
            }

            mission.MissionDate = missionDate;
            _missionRepository.Update(mission);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Mission date updated for mission {MissionId}: {MissionDate}", id, missionDate);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while setting mission date for mission {MissionId}", id);
            throw;
        }
    }

    /// <summary>
    /// Assign mission type (UC-8.3)
    /// </summary>
    public async Task AssignMissionTypeAsync(Guid id, int missionTypeId)
    {
        try
        {
            var mission = await _missionRepository.GetByIdAsync(id);
            if (mission == null)
            {
                throw new InvalidOperationException($"Mission with ID {id} not found");
            }

            if (!IsWithinCallerScope(mission))
            {
                throw new InvalidOperationException($"Mission with ID {id} not found");
            }

            if (mission.IsMissionCompleted)
            {
                throw new InvalidOperationException("Cannot modify a completed mission");
            }

            mission.FK_MissionTypeId = missionTypeId;
            _missionRepository.Update(mission);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Mission type assigned for mission {MissionId}: {MissionTypeId}", id, missionTypeId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while assigning mission type for mission {MissionId}", id);
            throw;
        }
    }

    /// <summary>
    /// Assign mission time type (UC-8.4)
    /// </summary>
    public async Task AssignMissionTimeTypeAsync(Guid id, int missionTimeTypeId)
    {
        try
        {
            var mission = await _missionRepository.GetByIdAsync(id);
            if (mission == null)
            {
                throw new InvalidOperationException($"Mission with ID {id} not found");
            }

            if (!IsWithinCallerScope(mission))
            {
                throw new InvalidOperationException($"Mission with ID {id} not found");
            }

            if (mission.IsMissionCompleted)
            {
                throw new InvalidOperationException("Cannot modify a completed mission");
            }

            mission.FK_MissionTimeTypeId = missionTimeTypeId;
            _missionRepository.Update(mission);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Mission time type assigned for mission {MissionId}: {MissionTimeTypeId}", id, missionTimeTypeId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while assigning mission time type for mission {MissionId}", id);
            throw;
        }
    }

    /// <summary>
    /// Set mission location (UC-8.5)
    /// </summary>
    public async Task SetMissionLocationAsync(Guid id, MissionLocationDto location)
    {
        try
        {
            var mission = await _missionRepository.GetByIdAsync(id);
            if (mission == null)
            {
                throw new InvalidOperationException($"Mission with ID {id} not found");
            }

            if (!IsWithinCallerScope(mission))
            {
                throw new InvalidOperationException($"Mission with ID {id} not found");
            }

            if (mission.IsMissionCompleted)
            {
                throw new InvalidOperationException("Cannot modify a completed mission");
            }

            // Validate location cascade
            if (location.FK_CenterId.HasValue && !location.FK_RegionId.HasValue)
            {
                throw new InvalidOperationException("Region must be specified when Center is selected");
            }

            if (location.FK_RegionId.HasValue && !location.FK_CountryId.HasValue)
            {
                throw new InvalidOperationException("Country must be specified when Region is selected");
            }

            // A pinned caller cannot move the record to another country — the payload
            // value applies only to unscoped (HQ) callers.
            mission.FK_CountryId = _currentUser.CountryId ?? location.FK_CountryId;
            mission.FK_RegionId = location.FK_RegionId;
            mission.FK_CenterId = location.FK_CenterId;
            mission.MissionLocation = location.MissionLocation;
            mission.Village = location.Village;

            _missionRepository.Update(mission);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Mission location updated for mission {MissionId}", id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while setting mission location for mission {MissionId}", id);
            throw;
        }
    }

    /// <summary>
    /// Assign mission owner (UC-8.6)
    /// </summary>
    public async Task AssignMissionOwnerAsync(Guid id, Guid userId)
    {
        try
        {
            var mission = await _missionRepository.GetByIdAsync(id);
            if (mission == null)
            {
                throw new InvalidOperationException($"Mission with ID {id} not found");
            }

            if (!IsWithinCallerScope(mission))
            {
                throw new InvalidOperationException($"Mission with ID {id} not found");
            }

            if (mission.IsMissionCompleted)
            {
                throw new InvalidOperationException("Cannot modify a completed mission");
            }

            var previousUserId = mission.FK_UserId;
            mission.FK_UserId = userId;

            _missionRepository.Update(mission);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Mission owner assigned for mission {MissionId}: {UserId}", id, userId);

            // TODO: Send notification to new owner
            // Notification integration to be implemented using INotificationsManager
            _logger.LogInformation("TODO: Send notification to user {UserId} for mission assignment", userId);

            if (previousUserId.HasValue && previousUserId.Value != userId)
            {
                _logger.LogInformation("TODO: Send notification to previous owner {UserId} for mission reassignment", previousUserId.Value);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while assigning mission owner for mission {MissionId}", id);
            throw;
        }
    }

    // ========== View Operations ==========

    /// <summary>
    /// Get mission status summary (legacy UC-8.13 support)
    /// </summary>
    public async Task<MissionStatusSummaryDto> GetMissionStatusSummaryAsync()
    {
        try
        {
            var allMissions = await _missionRepository.GetAllAsync();
            var today = DateTime.Today;

            var summary = new MissionStatusSummaryDto
            {
                TotalCount = allMissions.Count(),
                PendingCount = allMissions.Count(m => m.MissionDate >= today && !m.IsMissionCompleted),
                InProgressCount = allMissions.Count(m => m.MissionDate <= today && !m.IsMissionCompleted),
                CompletedCount = allMissions.Count(m => m.IsMissionCompleted),
                OverdueCount = allMissions.Count(m => m.MissionDate < today && !m.IsMissionCompleted)
            };

            _logger.LogInformation("Mission status summary retrieved: {@Summary}", summary);

            return summary;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving mission status summary");
            throw;
        }
    }

    /// <summary>
    /// Get overdue missions
    /// </summary>
    public async Task<List<MissionListDto>> GetOverdueMissionsAsync()
    {
        try
        {
            var overdueMissions = await _missionRepository.GetOverdueMissionsAsync();
            return _mapper.Map<List<MissionListDto>>(overdueMissions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving overdue missions");
            throw;
        }
    }

    // ========== Export ==========

    /// <summary>
    /// Export missions to Excel — §20.S.1's 13 grid columns, all filtered rows
    /// (OfficeProjectService.ExportProjectsToExcelAsync pattern).
    /// </summary>
    public async Task<byte[]> ExportMissionsToExcelAsync(MissionFilterDto filter)
    {
        try
        {
            _logger.LogInformation("Exporting missions to Excel with filter: {@Filter}", filter);

            var exportFilter = filter ?? new MissionFilterDto();
            exportFilter.Page = 1;
            exportFilter.PageSize = int.MaxValue;

            var result = await GetMissionsFilteredAsync(exportFilter);

            using (var package = new OfficeOpenXml.ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("المهام");

                worksheet.Cells[1, 1].Value = "الرقم";
                worksheet.Cells[1, 2].Value = "جهة المهمة";
                worksheet.Cells[1, 3].Value = "نوع المهمة";
                worksheet.Cells[1, 4].Value = "نوع التوقيت";
                worksheet.Cells[1, 5].Value = "اسم الجهة";
                worksheet.Cells[1, 6].Value = "التفاصيل";
                worksheet.Cells[1, 7].Value = "تاريخ المهمة";
                worksheet.Cells[1, 8].Value = "الدولة";
                worksheet.Cells[1, 9].Value = "المنطقة";
                worksheet.Cells[1, 10].Value = "المركز";
                worksheet.Cells[1, 11].Value = "القرية";
                worksheet.Cells[1, 12].Value = "مكان المهمة";
                worksheet.Cells[1, 13].Value = "المكلف بها";
                worksheet.Cells[1, 14].Value = "حالة الانتهاء";

                using (var range = worksheet.Cells[1, 1, 1, 14])
                {
                    range.Style.Font.Bold = true;
                    range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                }

                var row = 2;
                var serial = 1;
                foreach (var mission in result.Items)
                {
                    worksheet.Cells[row, 1].Value = serial++;
                    worksheet.Cells[row, 2].Value = mission.MissionTarget;
                    worksheet.Cells[row, 3].Value = mission.MissionType ?? "";
                    worksheet.Cells[row, 4].Value = mission.MissionTimeType ?? "";
                    worksheet.Cells[row, 5].Value = mission.EntityName ?? "";
                    worksheet.Cells[row, 6].Value = mission.Details ?? "";
                    worksheet.Cells[row, 7].Value = mission.MissionDate.ToString("yyyy-MM-dd");
                    worksheet.Cells[row, 8].Value = mission.CountryName ?? "";
                    worksheet.Cells[row, 9].Value = mission.Region ?? "";
                    worksheet.Cells[row, 10].Value = mission.Center ?? "";
                    worksheet.Cells[row, 11].Value = mission.Village ?? "";
                    worksheet.Cells[row, 12].Value = mission.MissionLocation ?? "";
                    worksheet.Cells[row, 13].Value = mission.AssignedTo ?? "";
                    worksheet.Cells[row, 14].Value = mission.MissionCompletedTxt ?? "";
                    row++;
                }

                worksheet.Cells[1, 1, row - 1, 14].AutoFitColumns();

                return package.GetAsByteArray();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while exporting missions to Excel");
            throw;
        }
    }

    // ========== Scope + filter helpers ==========

    /// <summary>
    /// Pin the caller's country and charity claims onto the filter — pin, never widen.
    /// A caller without the claims sees all (HQ module).
    /// </summary>
    private void ApplyCallerScope(MissionFilterDto filter)
    {
        var callerCountry = _currentUser.CountryId;
        if (callerCountry.HasValue)
        {
            if (filter.CountryId.HasValue && filter.CountryId.Value != callerCountry.Value)
            {
                _logger.LogWarning(
                    "Caller pinned to country {CallerCountry} requested country {RequestedCountry}; pinning to {CallerCountry}",
                    callerCountry.Value, filter.CountryId.Value, callerCountry.Value);
            }

            filter.CountryId = callerCountry;
        }

        var callerCharity = _currentUser.CharityId;
        if (callerCharity.HasValue)
        {
            if (filter.CharityId.HasValue && filter.CharityId.Value != callerCharity.Value)
            {
                _logger.LogWarning(
                    "Caller pinned to charity {CallerCharity} requested charity {RequestedCharity}; pinning to {CallerCharity}",
                    callerCharity.Value, filter.CharityId.Value, callerCharity.Value);
            }

            filter.CharityId = callerCharity;
        }
    }

    /// <summary>
    /// Whether the record is visible to the caller: a caller without claims sees
    /// everything; a pinned caller sees only their own country's and charity's rows.
    /// </summary>
    private bool IsWithinCallerScope(Mission mission)
    {
        var callerCountry = _currentUser.CountryId;
        if (callerCountry.HasValue && mission.FK_CountryId != callerCountry.Value)
        {
            return false;
        }

        var callerCharity = _currentUser.CharityId;
        if (callerCharity.HasValue && mission.FK_CharityId != callerCharity)
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// Existence-check every FK the create payload names, so a bogus id fails as a 400
    /// field error instead of a SQL foreign-key 500. The validator has already run, so
    /// the required keys are non-null here.
    /// </summary>
    private async Task ValidateForeignKeysAsync(CreateMissionDto dto)
    {
        var failures = new List<ValidationFailure>();

        if (!await _missionTypeRepository.ExistsAsync(dto.MissionTypeId))
            failures.Add(new ValidationFailure(nameof(dto.MissionTypeId), "Mission type does not exist"));
        if (!await _missionTimeTypeRepository.ExistsAsync(dto.MissionTimeTypeId))
            failures.Add(new ValidationFailure(nameof(dto.MissionTimeTypeId), "Mission time type does not exist"));
        if (!await _missionInterviewTypeRepository.ExistsAsync(dto.MissionInterviewTypeId))
            failures.Add(new ValidationFailure(nameof(dto.MissionInterviewTypeId), "Mission interview type does not exist"));
        if (!await _regionRepository.ExistsAsync(dto.RegionId!.Value))
            failures.Add(new ValidationFailure(nameof(dto.RegionId), "Region does not exist"));
        if (!await _centerRepository.ExistsAsync(dto.CenterId!.Value))
            failures.Add(new ValidationFailure(nameof(dto.CenterId), "Center does not exist"));
        if (dto.CountryId.HasValue && !await _countryRepository.ExistsAsync(dto.CountryId.Value))
            failures.Add(new ValidationFailure(nameof(dto.CountryId), "Country does not exist"));
        if (!await AssigneeExistsAsync(dto.AssignedToUserId))
            failures.Add(new ValidationFailure(nameof(dto.AssignedToUserId), "Assigned user does not exist"));

        if (failures.Count > 0)
        {
            throw new ValidationException(failures);
        }
    }

    /// <summary>
    /// Same existence checks for the patch-style update payload — only the values the
    /// DTO actually carries are checked.
    /// </summary>
    private async Task ValidateForeignKeysAsync(UpdateMissionDto dto)
    {
        var failures = new List<ValidationFailure>();

        if (dto.MissionTypeId.HasValue && !await _missionTypeRepository.ExistsAsync(dto.MissionTypeId.Value))
            failures.Add(new ValidationFailure(nameof(dto.MissionTypeId), "Mission type does not exist"));
        if (dto.MissionTimeTypeId.HasValue && !await _missionTimeTypeRepository.ExistsAsync(dto.MissionTimeTypeId.Value))
            failures.Add(new ValidationFailure(nameof(dto.MissionTimeTypeId), "Mission time type does not exist"));
        if (dto.MissionInterviewTypeId.HasValue && !await _missionInterviewTypeRepository.ExistsAsync(dto.MissionInterviewTypeId.Value))
            failures.Add(new ValidationFailure(nameof(dto.MissionInterviewTypeId), "Mission interview type does not exist"));
        if (dto.RegionId.HasValue && !await _regionRepository.ExistsAsync(dto.RegionId.Value))
            failures.Add(new ValidationFailure(nameof(dto.RegionId), "Region does not exist"));
        if (dto.CenterId.HasValue && !await _centerRepository.ExistsAsync(dto.CenterId.Value))
            failures.Add(new ValidationFailure(nameof(dto.CenterId), "Center does not exist"));
        if (dto.CountryId.HasValue && !await _countryRepository.ExistsAsync(dto.CountryId.Value))
            failures.Add(new ValidationFailure(nameof(dto.CountryId), "Country does not exist"));
        if (dto.AssignedToUserId.HasValue && dto.AssignedToUserId.Value != Guid.Empty
            && !await AssigneeExistsAsync(dto.AssignedToUserId.Value))
            failures.Add(new ValidationFailure(nameof(dto.AssignedToUserId), "Assigned user does not exist"));

        if (failures.Count > 0)
        {
            throw new ValidationException(failures);
        }
    }

    /// <summary>
    /// Whether the named user exists — the user-management endpoint (/api/usermanagement,
    /// the identity user store) is the assignee pick-list source, so an assignee must
    /// resolve there. Employees and users are distinct populations: a user without an
    /// employee record is a valid assignee (Mission.FK_UserId is an identity user id).
    /// </summary>
    private async Task<bool> AssigneeExistsAsync(Guid userId)
    {
        var user = await _userAppService.GetUserDetailAsync(userId);
        return user != null;
    }

    /// <summary>
    /// Build the combined filter expression — every predicate ANDs together; the dates
    /// combine with the rest instead of replacing them.
    /// </summary>
    private static System.Linq.Expressions.Expression<Func<Mission, bool>>? BuildFilterExpression(MissionFilterDto filter)
    {
        return m =>
            (!filter.MissionTypeId.HasValue || m.FK_MissionTypeId == filter.MissionTypeId.Value) &&
            (!filter.MissionTimeTypeId.HasValue || m.FK_MissionTimeTypeId == filter.MissionTimeTypeId.Value) &&
            (!filter.CharityId.HasValue || m.FK_CharityId == filter.CharityId.Value) &&
            (!filter.CountryId.HasValue || m.FK_CountryId == filter.CountryId.Value) &&
            (!filter.RegionId.HasValue || m.FK_RegionId == filter.RegionId.Value) &&
            (!filter.CenterId.HasValue || m.FK_CenterId == filter.CenterId.Value) &&
            (!filter.AssignedToUserId.HasValue || m.FK_UserId == filter.AssignedToUserId.Value) &&
            (!filter.IsCompleted.HasValue || m.IsMissionCompleted == filter.IsCompleted.Value) &&
            (!filter.DateFrom.HasValue || m.MissionDate >= filter.DateFrom.Value) &&
            // DateTo is inclusive of its whole day: a date-only value binds midnight, so the
            // bound is the start of the NEXT day, not the end of the named one.
            (!filter.DateTo.HasValue || m.MissionDate < filter.DateTo.Value.Date.AddDays(1)) &&
            (string.IsNullOrWhiteSpace(filter.Search) ||
             (m.MissionTarget != null && m.MissionTarget.Contains(filter.Search)));
    }
}
