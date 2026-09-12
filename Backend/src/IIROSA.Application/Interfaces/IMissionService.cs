using IIROSA.Application.DTOs.MissionManagement;

namespace IIROSA.Application.Interfaces;

/// <summary>
/// Mission Service Interface
/// Business operations for Mission management (epic 15, UC-MSN-01…09)
/// IMPORTANT: Only Admin and Super Admin roles can access these operations.
/// </summary>
public interface IMissionService
{
    // ========== Reads ==========

    /// <summary>
    /// Get missions with filtering and pagination (UC-MSN-01 / UC-MSN-02)
    /// </summary>
    Task<MissionPagedResult<MissionListDto>> GetMissionsFilteredAsync(MissionFilterDto filter);

    /// <summary>
    /// Get mission by ID (UC-MSN-07 detail read) — includes navigations; an
    /// out-of-scope id returns null exactly like a missing one.
    /// </summary>
    Task<MissionDetailDto?> GetMissionByIdAsync(Guid id);

    /// <summary>
    /// The §20.U.1 register read: scoped to the caller's charity and country (NOT
    /// assigned-to-me; the assigned user stays an optional filter).
    /// </summary>
    Task<MissionPagedResult<MissionListDto>> GetMyMissionsAsync(MissionFilterDto filter);

    /// <summary>
    /// Register statistics for the band above the missions grid (UC-MSN-01) — same
    /// caller scope as the list read; describes the whole register, not the current search.
    /// </summary>
    Task<MissionStatisticsDto> GetStatisticsAsync();

    // ========== Writes ==========

    /// <summary>
    /// Create new mission (UC-MSN-06). Ownership (charity, country) is stamped
    /// server-side from the caller's claims.
    /// </summary>
    Task<MissionDetailDto> CreateMissionAsync(CreateMissionDto dto);

    /// <summary>
    /// Update mission (UC-MSN-07) — a completed mission is immutable.
    /// </summary>
    Task<MissionDetailDto> UpdateMissionAsync(Guid id, UpdateMissionDto dto);

    /// <summary>
    /// Delete mission (UC-MSN-08) — soft delete.
    /// </summary>
    Task DeleteMissionAsync(Guid id);

    /// <summary>
    /// Register the mission result (UC-MSN-09): findings + completion outcome + السبب.
    /// </summary>
    Task<MissionDetailDto> RegisterMissionResultAsync(Guid id, RegisterMissionResultDto dto);

    // ========== Legacy fine-grained operations (UC-8.x capability variants) ==========

    /// <summary>
    /// Set mission date (UC-8.2)
    /// </summary>
    Task SetMissionDateAsync(Guid id, DateTime missionDate);

    /// <summary>
    /// Assign mission type (UC-8.3)
    /// </summary>
    Task AssignMissionTypeAsync(Guid id, int missionTypeId);

    /// <summary>
    /// Assign mission time type (UC-8.4)
    /// </summary>
    Task AssignMissionTimeTypeAsync(Guid id, int missionTimeTypeId);

    /// <summary>
    /// Set mission location (UC-8.5)
    /// </summary>
    Task SetMissionLocationAsync(Guid id, MissionLocationDto location);

    /// <summary>
    /// Assign mission owner (UC-8.6)
    /// </summary>
    Task AssignMissionOwnerAsync(Guid id, Guid userId);

    // ========== View Operations ==========

    /// <summary>
    /// Get mission status summary (legacy UC-8.13 support)
    /// </summary>
    Task<MissionStatusSummaryDto> GetMissionStatusSummaryAsync();

    /// <summary>
    /// Get overdue missions
    /// </summary>
    Task<List<MissionListDto>> GetOverdueMissionsAsync();

    // ========== Export ==========

    /// <summary>
    /// Export missions to Excel (not in epic 15's scope)
    /// </summary>
    Task<byte[]> ExportMissionsToExcelAsync(MissionFilterDto filter);
}
