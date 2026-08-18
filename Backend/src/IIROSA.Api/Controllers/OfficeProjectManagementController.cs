using IIROSA.Application.DTOs.OfficeProjectManagement;
using IIROSA.Application.DTOs.LookupManagement;
using IIROSA.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IIROSA.Api.Controllers;

/// <summary>
/// Office Project Management API Controller
/// Implements UC-7.1 to UC-7.14: Office Project CRUD operations and specialized actions
/// Follows approved Framework.Core architecture
/// IMPORTANT: Only Admin and Super Admin roles can access this controller.
/// Charity users are explicitly blocked from this module.
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

    // ========== CRUD Operations ==========

    /// <summary>
    /// Get all office projects with filtering and pagination (UC-7.10: View Project List)
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
    /// Get office project by ID (UC-7.11: View Project Details)
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

    /// <summary>
    /// Create new office project (UC-7.1: Create Office Project)
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<OfficeProjectDetailDto>> CreateProject([FromBody] CreateOfficeProjectDto model)
    {
        try
        {
            var project = await _projectService.CreateProjectAsync(model);
            return CreatedAtAction(nameof(GetProject), new { id = project.Id }, project);
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
    /// Update office project (UC-7.8: Update Project Details)
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<OfficeProjectDetailDto>> UpdateProject(Guid id, [FromBody] UpdateOfficeProjectDto model)
    {
        try
        {
            var project = await _projectService.UpdateProjectAsync(id, model);
            return Ok(project);
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
    /// Delete office project
    /// </summary>
    [HttpDelete("{id}")]
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

    // ========== Project-Specific Operations ==========

    /// <summary>
    /// Set project budget (UC-7.2: Set Project Budget)
    /// </summary>
    [HttpPut("{id}/budget")]
    public async Task<ActionResult> SetProjectBudget(Guid id, [FromBody] SetProjectBudgetDto model)
    {
        try
        {
            await _projectService.SetProjectBudgetAsync(id, model);
            _logger.LogInformation("Project budget updated for project {ProjectId} by {UpdatedBy}", id, User.Identity?.Name);
            return Ok(new { message = "Project budget updated successfully" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while setting project budget for project {ProjectId}", id);
            return StatusCode(500, new { message = "An error occurred while setting project budget" });
        }
    }

    /// <summary>
    /// Specify project donor (UC-7.3: Specify Project Donor)
    /// </summary>
    [HttpPut("{id}/donor")]
    public async Task<ActionResult> SpecifyProjectDonor(Guid id, [FromBody] SpecifyProjectDonorDto model)
    {
        try
        {
            await _projectService.SpecifyProjectDonorAsync(id, model);
            _logger.LogInformation("Project donor specified for project {ProjectId} by {UpdatedBy}", id, User.Identity?.Name);
            return Ok(new { message = "Project donor specified successfully" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while specifying project donor for project {ProjectId}", id);
            return StatusCode(500, new { message = "An error occurred while specifying project donor" });
        }
    }

    /// <summary>
    /// Set beneficiaries count (UC-7.4: Set Beneficiaries Count)
    /// </summary>
    [HttpPut("{id}/beneficiaries")]
    public async Task<ActionResult> SetBeneficiariesCount(Guid id, [FromBody] SetBeneficiariesCountDto model)
    {
        try
        {
            await _projectService.SetBeneficiariesCountAsync(id, model);
            _logger.LogInformation("Beneficiaries count set for project {ProjectId} by {UpdatedBy}", id, User.Identity?.Name);
            return Ok(new { message = "Beneficiaries count set successfully" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while setting beneficiaries count for project {ProjectId}", id);
            return StatusCode(500, new { message = "An error occurred while setting beneficiaries count" });
        }
    }

    /// <summary>
    /// Assign project location (UC-7.5: Assign Project Location)
    /// </summary>
    [HttpPut("{id}/location")]
    public async Task<ActionResult> AssignProjectLocation(Guid id, [FromBody] AssignProjectLocationDto model)
    {
        try
        {
            await _projectService.AssignProjectLocationAsync(id, model);
            _logger.LogInformation("Project location assigned for project {ProjectId} by {UpdatedBy}", id, User.Identity?.Name);
            return Ok(new { message = "Project location assigned successfully" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while assigning project location for project {ProjectId}", id);
            return StatusCode(500, new { message = "An error occurred while assigning project location" });
        }
    }

    /// <summary>
    /// Attach project document (UC-7.6: Attach Project Documents)
    /// </summary>
    [HttpPut("{id}/documents")]
    public async Task<ActionResult> AttachProjectDocument(Guid id, [FromBody] AttachProjectDocumentDto model)
    {
        try
        {
            await _projectService.AttachProjectDocumentAsync(id, model);
            _logger.LogInformation("Project document attached for project {ProjectId} by {UpdatedBy}", id, User.Identity?.Name);
            return Ok(new { message = "Project document attached successfully" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while attaching project document for project {ProjectId}", id);
            return StatusCode(500, new { message = "An error occurred while attaching project document" });
        }
    }

    /// <summary>
    /// Upload project report (UC-7.7: Upload Project Report)
    /// </summary>
    [HttpPut("{id}/report")]
    public async Task<ActionResult> UploadProjectReport(Guid id, [FromBody] UploadProjectReportDto model)
    {
        try
        {
            await _projectService.UploadProjectReportAsync(id, model);
            _logger.LogInformation("Project report uploaded for project {ProjectId} by {UpdatedBy}", id, User.Identity?.Name);
            return Ok(new { message = "Project report uploaded successfully" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while uploading project report for project {ProjectId}", id);
            return StatusCode(500, new { message = "An error occurred while uploading project report" });
        }
    }

    /// <summary>
    /// Mark project as completed (UC-7.9: Mark Project as Completed)
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

    /// <summary>
    /// Set project dates (UC-7.13: Set Project Dates)
    /// </summary>
    [HttpPut("{id}/dates")]
    public async Task<ActionResult> SetProjectDates(Guid id, [FromBody] SetProjectDatesDto model)
    {
        try
        {
            await _projectService.SetProjectDatesAsync(id, model);
            _logger.LogInformation("Project dates updated for project {ProjectId} by {UpdatedBy}", id, User.Identity?.Name);
            return Ok(new { message = "Project dates updated successfully" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while setting project dates for project {ProjectId}", id);
            return StatusCode(500, new { message = "An error occurred while setting project dates" });
        }
    }

    /// <summary>
    /// Assign project to charity (UC-7.14: Assign Project to Charity)
    /// </summary>
    [HttpPut("{id}/assign-charity")]
    public async Task<ActionResult> AssignProjectToCharity(Guid id, [FromBody] AssignProjectToCharityDto model)
    {
        try
        {
            await _projectService.AssignProjectToCharityAsync(id, model);
            _logger.LogInformation("Project assigned to charity for project {ProjectId} by {UpdatedBy}", id, User.Identity?.Name);
            return Ok(new { message = "Project assigned to charity successfully" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while assigning project to charity for project {ProjectId}", id);
            return StatusCode(500, new { message = "An error occurred while assigning project to charity" });
        }
    }

    // ========== View Operations ==========

    /// <summary>
    /// Get projects assigned to a specific charity (UC-7.14)
    /// </summary>
    [HttpGet("charity/{charityId}")]
    public async Task<ActionResult<OfficeProjectPagedResult<OfficeProjectListDto>>> GetProjectsByCharity(
        Guid charityId,
        [FromQuery] OfficeProjectFilterDto filter)
    {
        try
        {
            var result = await _projectService.GetProjectsByCharityAsync(charityId, filter);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving office projects for charity {CharityId}", charityId);
            return StatusCode(500, new { message = "An error occurred while retrieving office projects for charity" });
        }
    }

    /// <summary>
    /// Get project status summary (UC-7.12: Track Project Progress)
    /// </summary>
    [HttpGet("status-summary")]
    public async Task<ActionResult<OfficeProjectStatusSummaryDto>> GetStatusSummary()
    {
        try
        {
            var summary = await _projectService.GetProjectStatusSummaryAsync();
            return Ok(summary);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving office project status summary");
            return StatusCode(500, new { message = "An error occurred while retrieving office project status summary" });
        }
    }

    /// <summary>
    /// Get ongoing projects
    /// </summary>
    [HttpGet("ongoing")]
    public async Task<ActionResult<List<OfficeProjectListDto>>> GetOngoingProjects()
    {
        try
        {
            var ongoingProjects = await _projectService.GetOngoingProjectsAsync();
            return Ok(ongoingProjects);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving ongoing office projects");
            return StatusCode(500, new { message = "An error occurred while retrieving ongoing office projects" });
        }
    }

    /// <summary>
    /// Get completed projects
    /// </summary>
    [HttpGet("completed")]
    public async Task<ActionResult<List<OfficeProjectListDto>>> GetCompletedProjects()
    {
        try
        {
            var completedProjects = await _projectService.GetCompletedProjectsAsync();
            return Ok(completedProjects);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving completed office projects");
            return StatusCode(500, new { message = "An error occurred while retrieving completed office projects" });
        }
    }

    // ========== Export ==========

    /// <summary>
    /// Export projects to Excel (UC-7.10: Export to Excel)
    /// </summary>
    [HttpPost("export")]
    public async Task<IActionResult> ExportProjects([FromBody] OfficeProjectFilterDto filter)
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

    // ========== Lookup Data ==========
}
