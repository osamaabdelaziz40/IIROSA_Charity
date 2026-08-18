using IIROSA.Application.DTOs.MissionManagement;
using IIROSA.Application.DTOs.LookupManagement;
using IIROSA.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace IIROSA.Api.Controllers;

/// <summary>
/// Mission Management API Controller
/// Implements UC-8.1 to UC-8.13: Mission CRUD operations and specialized actions
/// Follows approved Framework.Core architecture
/// IMPORTANT: Only Admin and Super Admin roles can access this controller.
/// Charity users are explicitly blocked from this module.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin,SuperAdmin")]
public class MissionManagementController : ControllerBase
{
    private readonly IMissionService _missionService;
    private readonly IMissionTypeService _missionTypeService;
    private readonly ILogger<MissionManagementController> _logger;

    public MissionManagementController(
        IMissionService missionService,
        IMissionTypeService missionTypeService,
        ILogger<MissionManagementController> logger)
    {
        _missionService = missionService;
        _missionTypeService = missionTypeService;
        _logger = logger;
    }

    // ========== CRUD Operations ==========

    /// <summary>
    /// Get all missions with filtering and pagination (UC-8.10: View Mission List)
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<MissionPagedResult<MissionListDto>>> GetMissions(
        [FromQuery] MissionFilterDto filter)
    {
        try
        {
            var result = await _missionService.GetMissionsFilteredAsync(filter);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving missions");
            return StatusCode(500, new { message = "An error occurred while retrieving missions" });
        }
    }

    /// <summary>
    /// Get mission by ID (UC-8.11: View Mission Details)
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<MissionDetailDto>> GetMission(Guid id)
    {
        try
        {
            var mission = await _missionService.GetMissionByIdAsync(id);
            if (mission == null)
            {
                return NotFound(new { message = "Mission not found" });
            }

            return Ok(mission);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving mission {MissionId}", id);
            return StatusCode(500, new { message = "An error occurred while retrieving mission" });
        }
    }

    /// <summary>
    /// Create new mission (UC-8.1: Create Mission)
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<MissionDetailDto>> CreateMission([FromBody] CreateMissionDto model)
    {
        try
        {
            var mission = await _missionService.CreateMissionAsync(model);
            return CreatedAtAction(nameof(GetMission), new { id = mission.Id }, mission);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while creating mission");
            return StatusCode(500, new { message = "An error occurred while creating mission" });
        }
    }

    /// <summary>
    /// Update mission (UC-8.7: Update Mission Details)
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<MissionDetailDto>> UpdateMission(Guid id, [FromBody] UpdateMissionDto model)
    {
        try
        {
            var mission = await _missionService.UpdateMissionAsync(id, model);
            return Ok(mission);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating mission {MissionId}", id);
            return StatusCode(500, new { message = "An error occurred while updating mission" });
        }
    }

    /// <summary>
    /// Delete mission
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteMission(Guid id)
    {
        try
        {
            await _missionService.DeleteMissionAsync(id);
            _logger.LogInformation("Mission {MissionId} deleted by {DeletedBy}", id, User.Identity?.Name);
            return Ok(new { message = "Mission deleted successfully" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while deleting mission {MissionId}", id);
            return StatusCode(500, new { message = "An error occurred while deleting mission" });
        }
    }

    // ========== Mission-Specific Operations ==========

    /// <summary>
    /// Set mission date (UC-8.2: Set Mission Date)
    /// </summary>
    [HttpPut("{id}/date")]
    public async Task<ActionResult> SetMissionDate(Guid id, [FromBody] SetMissionDateDto model)
    {
        try
        {
            await _missionService.SetMissionDateAsync(id, model.MissionDate);
            _logger.LogInformation("Mission date updated for mission {MissionId} by {UpdatedBy}", id, User.Identity?.Name);
            return Ok(new { message = "Mission date updated successfully" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while setting mission date for mission {MissionId}", id);
            return StatusCode(500, new { message = "An error occurred while setting mission date" });
        }
    }

    /// <summary>
    /// Assign mission type (UC-8.3: Assign Mission Type)
    /// </summary>
    [HttpPut("{id}/type")]
    public async Task<ActionResult> AssignMissionType(Guid id, [FromBody] AssignMissionTypeDto model)
    {
        try
        {
            await _missionService.AssignMissionTypeAsync(id, model.FK_MissionTypeId);
            _logger.LogInformation("Mission type assigned for mission {MissionId} by {UpdatedBy}", id, User.Identity?.Name);
            return Ok(new { message = "Mission type assigned successfully" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while assigning mission type for mission {MissionId}", id);
            return StatusCode(500, new { message = "An error occurred while assigning mission type" });
        }
    }

    /// <summary>
    /// Assign mission time type (UC-8.4: Assign Mission Time Type)
    /// </summary>
    [HttpPut("{id}/timetype")]
    public async Task<ActionResult> AssignMissionTimeType(Guid id, [FromBody] AssignMissionTimeTypeDto model)
    {
        try
        {
            await _missionService.AssignMissionTimeTypeAsync(id, model.FK_MissionTimeTypeId);
            _logger.LogInformation("Mission time type assigned for mission {MissionId} by {UpdatedBy}", id, User.Identity?.Name);
            return Ok(new { message = "Mission time type assigned successfully" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while assigning mission time type for mission {MissionId}", id);
            return StatusCode(500, new { message = "An error occurred while assigning mission time type" });
        }
    }

    /// <summary>
    /// Set mission location (UC-8.5: Set Mission Location)
    /// </summary>
    [HttpPut("{id}/location")]
    public async Task<ActionResult> SetMissionLocation(Guid id, [FromBody] MissionLocationDto model)
    {
        try
        {
            await _missionService.SetMissionLocationAsync(id, model);
            _logger.LogInformation("Mission location updated for mission {MissionId} by {UpdatedBy}", id, User.Identity?.Name);
            return Ok(new { message = "Mission location updated successfully" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while setting mission location for mission {MissionId}", id);
            return StatusCode(500, new { message = "An error occurred while setting mission location" });
        }
    }

    /// <summary>
    /// Assign mission owner (UC-8.6: Assign Mission Owner)
    /// </summary>
    [HttpPut("{id}/assign")]
    public async Task<ActionResult> AssignMissionOwner(Guid id, [FromBody] AssignMissionOwnerDto model)
    {
        try
        {
            await _missionService.AssignMissionOwnerAsync(id, model.FK_UserId);
            _logger.LogInformation("Mission owner assigned for mission {MissionId} by {UpdatedBy}", id, User.Identity?.Name);
            return Ok(new { message = "Mission owner assigned successfully" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while assigning mission owner for mission {MissionId}", id);
            return StatusCode(500, new { message = "An error occurred while assigning mission owner" });
        }
    }

    /// <summary>
    /// Complete mission (UC-8.8: Mark Mission as Completed)
    /// </summary>
    [HttpPut("{id}/complete")]
    public async Task<ActionResult> CompleteMission(Guid id, [FromBody] CompleteMissionDto model)
    {
        try
        {
            await _missionService.CompleteMissionAsync(id, model);
            _logger.LogInformation("Mission completed: {MissionId} by {CompletedBy}", id, User.Identity?.Name);
            return Ok(new { message = "Mission completed successfully" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while completing mission {MissionId}", id);
            return StatusCode(500, new { message = "An error occurred while completing mission" });
        }
    }

    /// <summary>
    /// Record conference/entity information (UC-8.9: Record Conference/Entity)
    /// </summary>
    [HttpPut("{id}/event")]
    public async Task<ActionResult> RecordConferenceEntity(Guid id, [FromBody] RecordEventDto model)
    {
        try
        {
            await _missionService.RecordConferenceEntityAsync(id, model.ConferenceName, model.EntityName);
            _logger.LogInformation("Conference/Entity information recorded for mission {MissionId} by {UpdatedBy}", id, User.Identity?.Name);
            return Ok(new { message = "Conference/Entity information recorded successfully" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while recording conference/entity for mission {MissionId}", id);
            return StatusCode(500, new { message = "An error occurred while recording conference/entity information" });
        }
    }

    // ========== View Operations ==========

    /// <summary>
    /// Get missions assigned to current user (UC-8.12: View My Missions)
    /// </summary>
    [HttpGet("my-missions")]
    public async Task<ActionResult<MissionPagedResult<MissionListDto>>> GetMyMissions(
        [FromQuery] MissionFilterDto filter)
    {
        try
        {
            // Get current user ID from claims
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
            {
                return Unauthorized(new { message = "User ID not found in token" });
            }

            var result = await _missionService.GetMyMissionsAsync(userId, filter);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving my missions");
            return StatusCode(500, new { message = "An error occurred while retrieving my missions" });
        }
    }

    /// <summary>
    /// Get mission status summary (UC-8.13: Track Mission Status)
    /// </summary>
    [HttpGet("status-summary")]
    public async Task<ActionResult<MissionStatusSummaryDto>> GetStatusSummary()
    {
        try
        {
            var summary = await _missionService.GetMissionStatusSummaryAsync();
            return Ok(summary);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving mission status summary");
            return StatusCode(500, new { message = "An error occurred while retrieving mission status summary" });
        }
    }

    /// <summary>
    /// Get overdue missions
    /// </summary>
    [HttpGet("overdue")]
    public async Task<ActionResult<List<MissionListDto>>> GetOverdueMissions()
    {
        try
        {
            var overdueMissions = await _missionService.GetOverdueMissionsAsync();
            return Ok(overdueMissions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving overdue missions");
            return StatusCode(500, new { message = "An error occurred while retrieving overdue missions" });
        }
    }

    // ========== Export ==========

    /// <summary>
    /// Export missions to Excel (UC-8.10: Export to Excel)
    /// </summary>
    [HttpPost("export")]
    public async Task<IActionResult> ExportMissions([FromBody] MissionFilterDto filter)
    {
        try
        {
            var excelBytes = await _missionService.ExportMissionsToExcelAsync(filter);

            _logger.LogInformation("Missions exported to Excel by {ExportedBy}", User.Identity?.Name);

            return File(
                excelBytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"missions_{DateTime.UtcNow:yyyyMMddHHmmss}.xlsx"
            );
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while exporting missions to Excel");
            return StatusCode(500, new { message = "An error occurred while exporting missions to Excel" });
        }
    }

    // ========== Lookup Data ==========

    /// <summary>
    /// Get all active mission types for dropdown
    /// </summary>
    [HttpGet("mission-types")]
    public async Task<ActionResult<List<MissionTypeDto>>> GetMissionTypes()
    {
        try
        {
            var filter = new LookupFilterDto
            {
                IsActive = true,
                Page = 1,
                PageSize = 1000 // Get all active types
            };
            var result = await _missionTypeService.GetLookupItemsAsync(filter);
            return Ok(result.Items);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving mission types");
            return StatusCode(500, new { message = "An error occurred while retrieving mission types" });
        }
    }

    /// <summary>
    /// Get all active mission time types for dropdown
    /// TODO: Create IMissionTimeTypeService and MissionTimeTypeService similar to MissionTypeService
    /// </summary>
    [HttpGet("mission-time-types")]
    public ActionResult<List<MissionTimeTypeDto>> GetMissionTimeTypes()
    {
        try
        {
            // TODO: Implement this endpoint once MissionTimeTypeService is created
            // For now, return a hardcoded list based on common mission time types
            var timeTypes = new List<MissionTimeTypeDto>
            {
                new() { Id = 1, Name = "One-time", NameAr = "مرة واحدة", TimeTypeCode = "ONETIME", IsActive = true },
                new() { Id = 2, Name = "Daily", NameAr = "يومي", TimeTypeCode = "DAILY", IsActive = true },
                new() { Id = 3, Name = "Weekly", NameAr = "أسبوعي", TimeTypeCode = "WEEKLY", IsActive = true },
                new() { Id = 4, Name = "Monthly", NameAr = "شهري", TimeTypeCode = "MONTHLY", IsActive = true }
            };
            return Ok(timeTypes);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving mission time types");
            return StatusCode(500, new { message = "An error occurred while retrieving mission time types" });
        }
    }
}
