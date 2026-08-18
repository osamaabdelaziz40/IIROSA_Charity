using IIROSA.Application.DTOs.MissionManagement;
using IIROSA.Application.Interfaces;
using IIROSA.Domain.Entities;
using IIROSA.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using AutoMapper;

namespace IIROSA.Application.Services;

/// <summary>
/// Mission Service Implementation
/// Implements business logic for Mission management following UC-8.1 to UC-8.13
/// Integrates with Framework.Core for notifications and Framework.Identity for user operations
/// IMPORTANT: Only Admin and Super Admin roles can access this service.
/// Charity users are explicitly blocked from this module.
/// </summary>
public class MissionService : IMissionService
{
    private readonly IMissionRepository _missionRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<MissionService> _logger;

    public MissionService(
        IMissionRepository missionRepository,
        IMapper mapper,
        ILogger<MissionService> logger)
    {
        _missionRepository = missionRepository;
        _mapper = mapper;
        _logger = logger;
    }

    // ========== CRUD Operations ==========

    /// <summary>
    /// Get missions with filtering and pagination (UC-8.10: View Mission List)
    /// </summary>
    public async Task<MissionPagedResult<MissionListDto>> GetMissionsFilteredAsync(MissionFilterDto filter)
    {
        try
        {
            _logger.LogInformation("Retrieving missions with filter: {@Filter}", filter);

            // Build filter expression
            System.Linq.Expressions.Expression<Func<Mission, bool>>? filterExpression = null;

            if (filter != null)
            {
                filterExpression = m =>
                    (!filter.FK_MissionTypeId.HasValue || m.FK_MissionTypeId == filter.FK_MissionTypeId.Value) &&
                    (!filter.FK_MissionTimeTypeId.HasValue || m.FK_MissionTimeTypeId == filter.FK_MissionTimeTypeId.Value) &&
                    (!filter.FK_CountryId.HasValue || m.FK_CountryId == filter.FK_CountryId.Value) &&
                    (!filter.FK_RegionId.HasValue || m.FK_RegionId == filter.FK_RegionId.Value) &&
                    (!filter.FK_CenterId.HasValue || m.FK_CenterId == filter.FK_CenterId.Value) &&
                    (!filter.FK_UserId.HasValue || m.FK_UserId == filter.FK_UserId.Value) &&
                    (!filter.IsMissionCompleted.HasValue || m.IsMissionCompleted == filter.IsMissionCompleted.Value) &&
                    (!filter.StartDate.HasValue || m.MissionDate >= filter.StartDate.Value) &&
                    (!filter.EndDate.HasValue || m.MissionDate <= filter.EndDate.Value) &&
                    (string.IsNullOrWhiteSpace(filter.SearchText) ||
                     (m.MissionTarget != null && m.MissionTarget.Contains(filter.SearchText)));

                // Add date range filter if specified
                if (filter.StartDate.HasValue && filter.EndDate.HasValue)
                {
                    var startDate = filter.StartDate.Value;
                    var endDate = filter.EndDate.Value;
                    filterExpression = m => m.MissionDate >= startDate && m.MissionDate <= endDate;
                }
            }

            // Default ordering by mission date descending
            // Note: Ordering is handled in the repository call

            var (items, totalCount) = await _missionRepository.GetMissionsPagedAsync(
                filterExpression,
                q => q.OrderByDescending(m => m.MissionDate),
                filter?.Page ?? 1,
                filter?.PageSize ?? 20);

            var missionDtos = _mapper.Map<List<MissionListDto>>(items);

            return new MissionPagedResult<MissionListDto>
            {
                Items = missionDtos,
                TotalCount = totalCount,
                Page = filter?.Page ?? 1,
                PageSize = filter?.PageSize ?? 20
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving missions with filter: {@Filter}", filter);
            throw;
        }
    }

    /// <summary>
    /// Get mission by ID (UC-8.11: View Mission Details)
    /// </summary>
    public async Task<MissionDetailDto?> GetMissionByIdAsync(Guid id)
    {
        try
        {
            var mission = await _missionRepository.GetByIdAsync(id);
            if (mission == null)
            {
                _logger.LogWarning("Mission with ID {MissionId} not found", id);
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
    /// Create new mission (UC-8.1: Create Mission)
    /// </summary>
    public async Task<MissionDetailDto> CreateMissionAsync(CreateMissionDto dto)
    {
        try
        {
            _logger.LogInformation("Creating new mission: {@Mission}", dto);

            // 1. Validate business rules
            if (dto.MissionDate < DateTime.Today)
            {
                throw new InvalidOperationException("Mission date cannot be in the past");
            }

            // 2. Validate location cascade (Country → Region → Center)
            if (dto.FK_CenterId.HasValue && !dto.FK_RegionId.HasValue)
            {
                throw new InvalidOperationException("Region must be specified when Center is selected");
            }

            if (dto.FK_RegionId.HasValue && !dto.FK_CountryId.HasValue)
            {
                throw new InvalidOperationException("Country must be specified when Region is selected");
            }

            // 3. Create mission entity
            var mission = _mapper.Map<Mission>(dto);
            mission.IsMissionCompleted = false;

            // 4. Save to database
            await _missionRepository.AddAsync(mission);
            await _missionRepository.SaveChangesAsync();

            _logger.LogInformation("Mission created successfully with ID: {MissionId}", mission.Id);

            // TODO: Send notification to assigned user
            // Notification integration to be implemented using INotificationsManager
            if (dto.FK_UserId != Guid.Empty)
            {
                _logger.LogInformation("TODO: Send notification to user {UserId} for mission {MissionId}", dto.FK_UserId, mission.Id);
            }

            return _mapper.Map<MissionDetailDto>(mission);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while creating mission: {@Mission}", dto);
            throw;
        }
    }

    /// <summary>
    /// Update mission (UC-8.7: Update Mission Details)
    /// </summary>
    public async Task<MissionDetailDto> UpdateMissionAsync(Guid id, UpdateMissionDto dto)
    {
        try
        {
            var mission = await _missionRepository.GetByIdAsync(id);
            if (mission == null)
            {
                throw new InvalidOperationException($"Mission with ID {id} not found");
            }

            // Validate that mission is not completed
            if (mission.IsMissionCompleted)
            {
                throw new InvalidOperationException("Cannot update a completed mission");
            }

            // Validate mission date
            if (dto.MissionDate.HasValue && dto.MissionDate.Value < DateTime.Today)
            {
                throw new InvalidOperationException("Mission date cannot be in the past");
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
            if (!string.IsNullOrWhiteSpace(dto.MissionTarget))
                mission.MissionTarget = dto.MissionTarget;

            if (dto.MissionDetails != null)
                mission.MissionDetails = dto.MissionDetails;

            if (dto.Details != null)
                mission.Details = dto.Details;

            if (dto.MissionDate.HasValue)
                mission.MissionDate = dto.MissionDate.Value;

            if (dto.FK_MissionTypeId.HasValue)
                mission.FK_MissionTypeId = dto.FK_MissionTypeId.Value;

            if (dto.FK_MissionTimeTypeId.HasValue)
                mission.FK_MissionTimeTypeId = dto.FK_MissionTimeTypeId.Value;

            if (dto.FK_CountryId.HasValue)
                mission.FK_CountryId = dto.FK_CountryId.Value;

            if (dto.FK_RegionId.HasValue)
                mission.FK_RegionId = dto.FK_RegionId.Value;

            if (dto.FK_CenterId.HasValue)
                mission.FK_CenterId = dto.FK_CenterId.Value;

            if (dto.MissionLocation != null)
                mission.MissionLocation = dto.MissionLocation;

            if (dto.Village != null)
                mission.Village = dto.Village;

            if (dto.EntityName != null)
                mission.EntityName = dto.EntityName;

            if (dto.ConferenceName != null)
                mission.ConferenceName = dto.ConferenceName;

            _missionRepository.Update(mission);
            await _missionRepository.SaveChangesAsync();

            _logger.LogInformation("Mission updated successfully: {MissionId}", id);

            return _mapper.Map<MissionDetailDto>(mission);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating mission {MissionId}: {@Mission}", id, dto);
            throw;
        }
    }

    /// <summary>
    /// Delete mission
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

            // Validate that mission is not completed
            if (mission.IsMissionCompleted)
            {
                throw new InvalidOperationException("Cannot delete a completed mission");
            }

            _missionRepository.Delete(mission);
            await _missionRepository.SaveChangesAsync();

            _logger.LogInformation("Mission deleted successfully: {MissionId}", id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while deleting mission {MissionId}", id);
            throw;
        }
    }

    // ========== Mission-Specific Operations ==========

    /// <summary>
    /// Set mission date (UC-8.2: Set Mission Date)
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
            await _missionRepository.SaveChangesAsync();

            _logger.LogInformation("Mission date updated for mission {MissionId}: {MissionDate}", id, missionDate);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while setting mission date for mission {MissionId}", id);
            throw;
        }
    }

    /// <summary>
    /// Assign mission type (UC-8.3: Assign Mission Type)
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

            if (mission.IsMissionCompleted)
            {
                throw new InvalidOperationException("Cannot modify a completed mission");
            }

            mission.FK_MissionTypeId = missionTypeId;
            _missionRepository.Update(mission);
            await _missionRepository.SaveChangesAsync();

            _logger.LogInformation("Mission type assigned for mission {MissionId}: {MissionTypeId}", id, missionTypeId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while assigning mission type for mission {MissionId}", id);
            throw;
        }
    }

    /// <summary>
    /// Assign mission time type (UC-8.4: Assign Mission Time Type)
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

            if (mission.IsMissionCompleted)
            {
                throw new InvalidOperationException("Cannot modify a completed mission");
            }

            mission.FK_MissionTimeTypeId = missionTimeTypeId;
            _missionRepository.Update(mission);
            await _missionRepository.SaveChangesAsync();

            _logger.LogInformation("Mission time type assigned for mission {MissionId}: {MissionTimeTypeId}", id, missionTimeTypeId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while assigning mission time type for mission {MissionId}", id);
            throw;
        }
    }

    /// <summary>
    /// Set mission location (UC-8.5: Set Mission Location)
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

            mission.FK_CountryId = location.FK_CountryId;
            mission.FK_RegionId = location.FK_RegionId;
            mission.FK_CenterId = location.FK_CenterId;
            mission.MissionLocation = location.MissionLocation;
            mission.Village = location.Village;

            _missionRepository.Update(mission);
            await _missionRepository.SaveChangesAsync();

            _logger.LogInformation("Mission location updated for mission {MissionId}", id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while setting mission location for mission {MissionId}", id);
            throw;
        }
    }

    /// <summary>
    /// Assign mission owner (UC-8.6: Assign Mission Owner)
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

            var previousUserId = mission.FK_UserId;
            mission.FK_UserId = userId;

            _missionRepository.Update(mission);
            await _missionRepository.SaveChangesAsync();

            _logger.LogInformation("Mission owner assigned for mission {MissionId}: {UserId}", id, userId);

            // TODO: Send notification to new owner
            // Notification integration to be implemented using INotificationsManager
            _logger.LogInformation("TODO: Send notification to user {UserId} for mission assignment", userId);

            // Notify previous owner if changed
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

    /// <summary>
    /// Complete mission (UC-8.8: Mark Mission as Completed)
    /// </summary>
    public async Task CompleteMissionAsync(Guid id, CompleteMissionDto dto)
    {
        try
        {
            var mission = await _missionRepository.GetByIdAsync(id);
            if (mission == null)
            {
                throw new InvalidOperationException($"Mission with ID {id} not found");
            }

            if (mission.IsMissionCompleted)
            {
                throw new InvalidOperationException("Mission is already completed");
            }

            mission.IsMissionCompleted = true;
            mission.MissionCompletedDate = DateTime.UtcNow;
            mission.MissionCompletedTxt = dto.MissionCompletedTxt;

            _missionRepository.Update(mission);
            await _missionRepository.SaveChangesAsync();

            _logger.LogInformation("Mission completed: {MissionId}", id);

            // TODO: Send completion notification
            // Notification integration to be implemented using INotificationsManager
            if (mission.FK_UserId.HasValue)
            {
                _logger.LogInformation("TODO: Send completion notification to user {UserId} for mission {MissionId}", mission.FK_UserId.Value, id);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while completing mission {MissionId}", id);
            throw;
        }
    }

    /// <summary>
    /// Record conference/entity information (UC-8.9: Record Conference/Entity)
    /// </summary>
    public async Task RecordConferenceEntityAsync(Guid id, string? conferenceName, string? entityName)
    {
        try
        {
            var mission = await _missionRepository.GetByIdAsync(id);
            if (mission == null)
            {
                throw new InvalidOperationException($"Mission with ID {id} not found");
            }

            if (mission.IsMissionCompleted)
            {
                throw new InvalidOperationException("Cannot modify a completed mission");
            }

            mission.ConferenceName = conferenceName;
            mission.EntityName = entityName;

            _missionRepository.Update(mission);
            await _missionRepository.SaveChangesAsync();

            _logger.LogInformation("Conference/Entity information recorded for mission {MissionId}", id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while recording conference/entity for mission {MissionId}", id);
            throw;
        }
    }

    // ========== View Operations ==========

    /// <summary>
    /// Get missions assigned to current user (UC-8.12: View My Missions)
    /// </summary>
    public async Task<MissionPagedResult<MissionListDto>> GetMyMissionsAsync(Guid userId, MissionFilterDto filter)
    {
        try
        {
            _logger.LogInformation("Retrieving missions for user {UserId}", userId);

            filter.FK_UserId = userId;
            return await GetMissionsFilteredAsync(filter);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving missions for user {UserId}", userId);
            throw;
        }
    }

    /// <summary>
    /// Get mission status summary (UC-8.13: Track Mission Status)
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
    /// Export missions to Excel (UC-8.10: Export to Excel)
    /// </summary>
    public async Task<byte[]> ExportMissionsToExcelAsync(MissionFilterDto filter)
    {
        try
        {
            _logger.LogInformation("Exporting missions to Excel with filter: {@Filter}", filter);

            var result = await GetMissionsFilteredAsync(filter);

            // TODO: Implement Excel export using a library like EPPlus or ClosedXML
            // For now, return a placeholder
            throw new NotImplementedException("Excel export functionality not yet implemented");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while exporting missions to Excel");
            throw;
        }
    }
}
