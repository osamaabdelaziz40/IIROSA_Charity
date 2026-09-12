using IIROSA.Application.DTOs.OfficeProjectManagement;
using IIROSA.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IIROSA.Api.Controllers;

/// <summary>
/// Office Project Management API Controller (UC-OFP-01…06): list, view, create, update, delete,
/// completion tracking and the Excel report.
///
/// Roles: the spec's actors (Gen. Director ≈ SuperAdmin, Staff ≈ Admin) map to the permission
/// matrix row "Office development projects" F/F/–/–/– — everything is Admin,SuperAdmin except
/// delete, which is the General Director's alone (UC-OFP-05).
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin,SuperAdmin")]
public class OfficeProjectManagementController : ControllerBase
{
    private readonly IOfficeProjectService _projectService;
    private readonly ILogger<OfficeProjectManagementController> _logger;

    public OfficeProjectManagementController(
        IOfficeProjectService projectService,
        ILogger<OfficeProjectManagementController> logger)
    {
        _projectService = projectService;
        _logger = logger;
    }

    // ========== Reads ==========

    /// <summary>
    /// Get all office projects with filtering and pagination (UC-OFP-01: list)
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<OfficeProjectPagedResult<OfficeProjectListDto>>> GetProjects(
        [FromQuery] OfficeProjectFilterDto filter)
    {
        try
        {
            var result = await _projectService.GetProjectsFilteredAsync(filter);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving office projects");
            return StatusCode(500, new { message = "An error occurred while retrieving office projects" });
        }
    }

    /// <summary>
    /// Register statistics for the band above the projects grid (UC-OFP-01: list) — same
    /// caller scope as the list: the band describes the caller's whole register,
    /// not the current search.
    /// </summary>
    // Literal segment beats the {id} route, but it stays above it by convention so the
    // pairing with the list is visible where GetProjects is read.
    [HttpGet("statistics")]
    public async Task<ActionResult<ProjectStatisticsDto>> GetStatistics()
    {
        try
        {
            var statistics = await _projectService.GetStatisticsAsync();
            return Ok(statistics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving office project statistics");
            return StatusCode(500, new { message = "An error occurred while retrieving office project statistics" });
        }
    }

    /// <summary>
    /// Get office project by ID (UC-OFP-04: view)
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<OfficeProjectDetailDto>> GetProject(Guid id)
    {
        try
        {
            var project = await _projectService.GetProjectByIdAsync(id);
            if (project == null)
            {
                return NotFound(new { message = "Office project not found" });
            }

            return Ok(project);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving office project {ProjectId}", id);
            return StatusCode(500, new { message = "An error occurred while retrieving office project" });
        }
    }

    // ========== Writes ==========

    /// <summary>
    /// Create new office project (UC-OFP-03)
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<OfficeProjectDetailDto>> CreateProject([FromBody] CreateOfficeProjectDto model)
    {
        try
        {
            var project = await _projectService.CreateProjectAsync(model);
            return CreatedAtAction(nameof(GetProject), new { id = project.Id }, project);
        }
        // Must precede the catch-all: ValidationException derives from Exception, so without this
        // it is swallowed into a 500, leaving the client no `errors` map to flag fields against.
        catch (FluentValidation.ValidationException ex)
        {
            return BadRequest(new
            {
                message = "One or more fields are invalid",
                errors = ex.Errors
                    .GroupBy(error => error.PropertyName ?? string.Empty)
                    .ToDictionary(
                        group => group.Key,
                        group => group.Select(error => error.ErrorMessage).ToArray())
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while creating office project");
            return StatusCode(500, new { message = "An error occurred while creating office project" });
        }
    }

    /// <summary>
    /// Update office project (UC-OFP-04)
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<OfficeProjectDetailDto>> UpdateProject(Guid id, [FromBody] UpdateOfficeProjectDto model)
    {
        try
        {
            var project = await _projectService.UpdateProjectAsync(id, model);
            return Ok(project);
        }
        catch (FluentValidation.ValidationException ex)
        {
            return BadRequest(new
            {
                message = "One or more fields are invalid",
                errors = ex.Errors
                    .GroupBy(error => error.PropertyName ?? string.Empty)
                    .ToDictionary(
                        group => group.Key,
                        group => group.Select(error => error.ErrorMessage).ToArray())
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating office project {ProjectId}", id);
            return StatusCode(500, new { message = "An error occurred while updating office project" });
        }
    }

    /// <summary>
    /// Delete office project — soft delete (UC-OFP-05). General Director only: the spec names no
    /// other actor for deletion and the permission matrix gives Staff no delete here.
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<ActionResult> DeleteProject(Guid id)
    {
        try
        {
            await _projectService.DeleteProjectAsync(id);
            _logger.LogInformation("Office project {ProjectId} deleted by {DeletedBy}", id, User.Identity?.Name);
            return Ok(new { message = "Office project deleted successfully" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while deleting office project {ProjectId}", id);
            return StatusCode(500, new { message = "An error occurred while deleting office project" });
        }
    }

    /// <summary>
    /// Mark project as completed (module completion tracking; feeds the progress view)
    /// </summary>
    [HttpPut("{id}/complete")]
    public async Task<ActionResult> MarkProjectAsCompleted(Guid id, [FromBody] MarkProjectCompletedDto model)
    {
        try
        {
            await _projectService.MarkProjectAsCompletedAsync(id, model);
            _logger.LogInformation("Project marked as completed: {ProjectId} by {CompletedBy}", id, User.Identity?.Name);
            return Ok(new { message = "Project marked as completed successfully" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while marking project as completed {ProjectId}", id);
            return StatusCode(500, new { message = "An error occurred while marking project as completed" });
        }
    }

    // ========== Report (UC-OFP-06) ==========

    /// <summary>
    /// Export projects to Excel (UC-OFP-06: report). A read like the list — GET with the filter
    /// bound from the query string, scoped to the caller's country by the service.
    /// </summary>
    [HttpGet("export")]
    public async Task<IActionResult> ExportProjects([FromQuery] OfficeProjectFilterDto filter)
    {
        try
        {
            var excelBytes = await _projectService.ExportProjectsToExcelAsync(filter);

            _logger.LogInformation("Office projects exported to Excel by {ExportedBy}", User.Identity?.Name);

            return File(
                excelBytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"office-projects_{DateTime.UtcNow:yyyyMMddHHmmss}.xlsx"
            );
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while exporting office projects to Excel");
            return StatusCode(500, new { message = "An error occurred while exporting office projects to Excel" });
        }
    }
}
