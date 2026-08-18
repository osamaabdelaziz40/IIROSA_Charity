using IIROSA.Application.DTOs.MissionManagement;

namespace IIROSA.Application.Interfaces;

/// <summary>
/// Mission Service Interface
/// Defines business operations for Mission management following UC-8.1 to UC-8.13
/// IMPORTANT: Only Admin and Super Admin roles can access these operations.
/// Charity users are explicitly blocked from this module.
/// </summary>
public interface IMissionService
{
    // ========== CRUD Operations ==========

    /// <summary>
    /// Get missions with filtering and pagination (UC-8.10: View Mission List)
    /// </summary>
    Task<MissionPagedResult<MissionListDto>> GetMissionsFilteredAsync(MissionFilterDto filter);

    /// <summary>
    /// Get mission by ID (UC-8.11: View Mission Details)
    /// </summary>
    Task<MissionDetailDto?> GetMissionByIdAsync(Guid id);

    /// <summary>
    /// Create new mission (UC-8.1: Create Mission)
    /// </summary>
    Task<MissionDetailDto> CreateMissionAsync(CreateMissionDto dto);

    /// <summary>
    /// Update mission (UC-8.7: Update Mission Details)
    /// </summary>
    Task<MissionDetailDto> UpdateMissionAsync(Guid id, UpdateMissionDto dto);

    /// <summary>
    /// Delete mission
    /// </summary>
    Task DeleteMissionAsync(Guid id);

    // ========== Mission-Specific Operations ==========

    /// <summary>
    /// Set mission date (UC-8.2: Set Mission Date)
    /// </summary>
    Task SetMissionDateAsync(Guid id, DateTime missionDate);

    /// <summary>
    /// Assign mission type (UC-8.3: Assign Mission Type)
    /// </summary>
    Task AssignMissionTypeAsync(Guid id, int missionTypeId);

    /// <summary>
    /// Assign mission time type (UC-8.4: Assign Mission Time Type)
    /// </summary>
    Task AssignMissionTimeTypeAsync(Guid id, int missionTimeTypeId);

    /// <summary>
    /// Set mission location (UC-8.5: Set Mission Location)
    /// </summary>
    Task SetMissionLocationAsync(Guid id, MissionLocationDto location);

    /// <summary>
    /// Assign mission owner (UC-8.6: Assign Mission Owner)
    /// </summary>
    Task AssignMissionOwnerAsync(Guid id, Guid userId);

    /// <summary>
    /// Complete mission (UC-8.8: Mark Mission as Completed)
    /// </summary>
    Task CompleteMissionAsync(Guid id, CompleteMissionDto dto);

    /// <summary>
    /// Record conference/entity information (UC-8.9: Record Conference/Entity)
    /// </summary>
    Task RecordConferenceEntityAsync(Guid id, string? conferenceName, string? entityName);

    // ========== View Operations ==========

    /// <summary>
    /// Get missions assigned to current user (UC-8.12: View My Missions)
    /// </summary>
    Task<MissionPagedResult<MissionListDto>> GetMyMissionsAsync(Guid userId, MissionFilterDto filter);

    /// <summary>
    /// Get mission status summary (UC-8.13: Track Mission Status)
    /// </summary>
    Task<MissionStatusSummaryDto> GetMissionStatusSummaryAsync();

    /// <summary>
    /// Get overdue missions
    /// </summary>
    Task<List<MissionListDto>> GetOverdueMissionsAsync();

    // ========== Export ==========

    /// <summary>
    /// Export missions to Excel (UC-8.10: Export to Excel)
    /// </summary>
    Task<byte[]> ExportMissionsToExcelAsync(MissionFilterDto filter);
}
