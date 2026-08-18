using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.ComponentModel.DataAnnotations;
using IIROSA.Application.Interfaces;
using IIROSA.Application.DTOs.HousingProject;

namespace IIROSA.Api.Controllers;

/// <summary>
/// Housing Projects Controller
/// Implements all housing project management endpoints (UC-10.1 through UC-10.10)
/// IMPORTANT: Charity users CANNOT access this module - only Admin and Super Admin
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "SuperAdmin,Admin")]
public class HousingProjectsController : ControllerBase
{
    private readonly IHousingProjectService _housingProjectService;
    private readonly ILogger<HousingProjectsController> _logger;

    public HousingProjectsController(
        IHousingProjectService housingProjectService,
        ILogger<HousingProjectsController> logger)
    {
        _housingProjectService = housingProjectService;
        _logger = logger;
    }

    #region CRUD Operations

    /// <summary>
    /// Get all projects with filtering and pagination (UC-10.6)
    /// </summary>
    [HttpGet("projects")]
    [ProducesResponseType(typeof(IEnumerable<HousingProjectListDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<(IEnumerable<HousingProjectListDto> Items, int TotalCount)>> GetProjects(
        [FromQuery] HousingProjectFilterDto filter)
    {
        try
        {
            var result = await _housingProjectService.GetProjectsAsync(filter);
            return Ok(new { result.Items, result.TotalCount });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving projects");
            return StatusCode(500, new { message = "Error retrieving projects", error = ex.Message });
        }
    }

    /// <summary>
    /// Get active projects
    /// </summary>
    [HttpGet("projects/active")]
    [ProducesResponseType(typeof(IEnumerable<HousingProjectListDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<HousingProjectListDto>>> GetActiveProjects()
    {
        try
        {
            var projects = await _housingProjectService.GetActiveProjectsAsync();
            return Ok(projects);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving active projects");
            return StatusCode(500, new { message = "Error retrieving active projects", error = ex.Message });
        }
    }

    /// <summary>
    /// Get completed projects
    /// </summary>
    [HttpGet("projects/completed")]
    [ProducesResponseType(typeof(IEnumerable<HousingProjectListDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<HousingProjectListDto>>> GetCompletedProjects()
    {
        try
        {
            var projects = await _housingProjectService.GetCompletedProjectsAsync();
            return Ok(projects);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving completed projects");
            return StatusCode(500, new { message = "Error retrieving completed projects", error = ex.Message });
        }
    }

    /// <summary>
    /// Get delayed projects
    /// </summary>
    [HttpGet("projects/delayed")]
    [ProducesResponseType(typeof(IEnumerable<HousingProjectListDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<HousingProjectListDto>>> GetDelayedProjects()
    {
        try
        {
            var projects = await _housingProjectService.GetDelayedProjectsAsync();
            return Ok(projects);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving delayed projects");
            return StatusCode(500, new { message = "Error retrieving delayed projects", error = ex.Message });
        }
    }

    /// <summary>
    /// Get project by ID
    /// </summary>
    [HttpGet("projects/{id}")]
    [ProducesResponseType(typeof(HousingProjectDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<HousingProjectDto>> GetProject(Guid id)
    {
        try
        {
            var project = await _housingProjectService.GetProjectByIdAsync(id);
            if (project == null)
            {
                return NotFound(new { message = $"Project with ID '{id}' not found" });
            }

            return Ok(project);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving project: {Id}", id);
            return StatusCode(500, new { message = "Error retrieving project", error = ex.Message });
        }
    }

    /// <summary>
    /// Create new project (UC-10.1)
    /// </summary>
    [HttpPost("projects")]
    [ProducesResponseType(typeof(HousingProjectDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<HousingProjectDto>> CreateProject([FromBody] CreateHousingProjectDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var project = await _housingProjectService.CreateProjectAsync(dto);
            return CreatedAtAction(nameof(GetProject), new { id = project.Id }, project);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating project");
            return StatusCode(500, new { message = "Error creating project", error = ex.Message });
        }
    }

    /// <summary>
    /// Update project (UC-10.7)
    /// </summary>
    [HttpPut("projects/{id}")]
    [ProducesResponseType(typeof(HousingProjectDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<HousingProjectDto>> UpdateProject(Guid id, [FromBody] UpdateHousingProjectDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != dto.Id)
            {
                return BadRequest(new { message = "ID mismatch" });
            }

            var project = await _housingProjectService.UpdateProjectAsync(dto);
            return Ok(project);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating project: {Id}", id);
            return StatusCode(500, new { message = "Error updating project", error = ex.Message });
        }
    }

    /// <summary>
    /// Delete project
    /// </summary>
    [HttpDelete("projects/{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> DeleteProject(Guid id)
    {
        try
        {
            await _housingProjectService.DeleteProjectAsync(id);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting project: {Id}", id);
            return StatusCode(500, new { message = "Error deleting project", error = ex.Message });
        }
    }

    #endregion

    #region Budget Management (UC-10.2)

    /// <summary>
    /// Set project budget (UC-10.2)
    /// </summary>
    [HttpPut("projects/{id}/budget")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> SetProjectBudget(Guid id, [FromBody] SetBudgetDto dto)
    {
        try
        {
            await _housingProjectService.SetProjectBudgetAsync(id, dto.TotalBudget, dto.Currency, dto.DonorName);
            return Ok(new { message = "Project budget updated successfully" });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting project budget: {Id}", id);
            return StatusCode(500, new { message = "Error setting project budget", error = ex.Message });
        }
    }

    #endregion

    #region Beneficiary Management (UC-10.3, UC-10.10)

    /// <summary>
    /// Assign beneficiary family to project (UC-10.3)
    /// </summary>
    [HttpPut("projects/{id}/beneficiary")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> AssignBeneficiaryFamily(Guid id, [FromBody] AssignBeneficiaryDto dto)
    {
        try
        {
            await _housingProjectService.AssignBeneficiaryFamilyAsync(id, dto.CharityId, dto.FamilyId);
            return Ok(new { message = "Beneficiary family assigned successfully" });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error assigning beneficiary family to project: {Id}", id);
            return StatusCode(500, new { message = "Error assigning beneficiary family", error = ex.Message });
        }
    }

    /// <summary>
    /// Assign project to charity (UC-10.10)
    /// </summary>
    [HttpPut("projects/{id}/charity")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> AssignProjectToCharity(Guid id, [FromBody] AssignCharityDto dto)
    {
        try
        {
            await _housingProjectService.AssignProjectToCharityAsync(id, dto.CharityId);
            return Ok(new { message = "Project assigned to charity successfully" });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error assigning project to charity: {Id}", id);
            return StatusCode(500, new { message = "Error assigning project to charity", error = ex.Message });
        }
    }

    #endregion

    #region Progress Tracking (UC-10.4)

    /// <summary>
    /// Update project progress (UC-10.4)
    /// </summary>
    [HttpPut("projects/{id}/progress")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> UpdateProjectProgress(Guid id, [FromBody] UpdateProjectProgressDto dto)
    {
        try
        {
            dto.ProjectId = id;
            await _housingProjectService.UpdateProjectProgressAsync(dto);
            return Ok(new { message = "Project progress updated successfully" });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating project progress: {Id}", id);
            return StatusCode(500, new { message = "Error updating project progress", error = ex.Message });
        }
    }

    #endregion

    #region Project Completion (UC-10.5)

    /// <summary>
    /// Complete project (UC-10.5)
    /// </summary>
    [HttpPost("projects/{id}/complete")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> CompleteProject(Guid id, [FromBody] CompleteProjectDto dto)
    {
        try
        {
            dto.ProjectId = id;
            await _housingProjectService.CompleteProjectAsync(dto);
            return Ok(new { message = "Project completed successfully" });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error completing project: {Id}", id);
            return StatusCode(500, new { message = "Error completing project", error = ex.Message });
        }
    }

    /// <summary>
    /// Reopen project
    /// </summary>
    [HttpPost("projects/{id}/reopen")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> ReopenProject(Guid id)
    {
        try
        {
            await _housingProjectService.ReopenProjectAsync(id);
            return Ok(new { message = "Project reopened successfully" });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reopening project: {Id}", id);
            return StatusCode(500, new { message = "Error reopening project", error = ex.Message });
        }
    }

    #endregion

    #region Reporting (UC-10.9)

    /// <summary>
    /// Generate housing report (UC-10.9)
    /// </summary>
    [HttpPost("reports/generate")]
    [ProducesResponseType(typeof(HousingProjectReportDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<HousingProjectReportDto>> GenerateHousingReport(
        [FromBody] HousingProjectReportFilterDto filter)
    {
        try
        {
            var report = await _housingProjectService.GenerateHousingReportAsync(filter);
            return Ok(report);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating housing report");
            return StatusCode(500, new { message = "Error generating housing report", error = ex.Message });
        }
    }

    /// <summary>
    /// Export housing report to PDF (UC-10.9)
    /// </summary>
    [HttpPost("reports/export/pdf")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult> ExportHousingReportToPdf([FromBody] HousingProjectReportFilterDto filter)
    {
        try
        {
            var pdfBytes = await _housingProjectService.ExportHousingReportToPdfAsync(filter);
            return File(pdfBytes, "application/pdf", $"housing-report-{DateTime.UtcNow:yyyyMMdd}.pdf");
        }
        catch (NotImplementedException)
        {
            return NotFound(new { message = "PDF export not yet implemented" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting report to PDF");
            return StatusCode(500, new { message = "Error exporting report", error = ex.Message });
        }
    }

    /// <summary>
    /// Export housing report to Excel (UC-10.9)
    /// </summary>
    [HttpPost("reports/export/excel")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult> ExportHousingReportToExcel([FromBody] HousingProjectReportFilterDto filter)
    {
        try
        {
            var excelBytes = await _housingProjectService.ExportHousingReportToExcelAsync(filter);
            return File(excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"housing-report-{DateTime.UtcNow:yyyyMMdd}.xlsx");
        }
        catch (NotImplementedException)
        {
            return NotFound(new { message = "Excel export not yet implemented" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting report to Excel");
            return StatusCode(500, new { message = "Error exporting report", error = ex.Message });
        }
    }

    #endregion

    #region Statistics

    /// <summary>
    /// Get project statistics
    /// </summary>
    [HttpGet("statistics")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public async Task<ActionResult> GetStatistics()
    {
        try
        {
            var stats = new
            {
                TotalProjects = await _housingProjectService.GetTotalProjectsCountAsync(),
                PlanningProjects = await _housingProjectService.GetProjectsByStatusCountAsync("Planning"),
                InProgressProjects = await _housingProjectService.GetProjectsByStatusCountAsync("In Progress"),
                OnHoldProjects = await _housingProjectService.GetProjectsByStatusCountAsync("On Hold"),
                CompletedProjects = await _housingProjectService.GetProjectsByStatusCountAsync("Completed"),
                AverageCompletionPercentage = await _housingProjectService.GetAverageCompletionPercentageAsync(),
                DelayedProjects = await _housingProjectService.GetDelayedProjectsCountAsync(),
                OnTrackProjects = await _housingProjectService.GetOnTrackProjectsCountAsync()
            };

            return Ok(stats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving statistics");
            return StatusCode(500, new { message = "Error retrieving statistics", error = ex.Message });
        }
    }

    #endregion
}

#region Helper DTOs for Controller

//public class SetBudgetDto
//{
//    public decimal TotalBudget { get; set; }
//    public string Currency { get; set; } = "EGP";
//    public string? DonorName { get; set; }
//}

public class AssignBeneficiaryDto
{
    public Guid? CharityId { get; set; }
    public Guid? FamilyId { get; set; }
}

public class AssignCharityDto
{
    public Guid? CharityId { get; set; }
}

#endregion
