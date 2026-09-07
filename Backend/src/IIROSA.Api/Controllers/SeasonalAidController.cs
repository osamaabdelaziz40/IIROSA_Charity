using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using IIROSA.Application.Interfaces;
using IIROSA.Application.DTOs.SeasonalAid;

namespace IIROSA.Api.Controllers;

/// <summary>
/// Seasonal Aid Controller
/// Implements all seasonal aid management endpoints (UC-9.1 through UC-9.11, UC-PRJ-01 through UC-PRJ-12).
/// Authorization is declared per action: campaign management is head-office only
/// (SuperAdmin/Admin), while the family-selection and distribution surface is shared with
/// charity users, whose data is scoped server-side by <see cref="ISeasonalAidService"/>.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SeasonalAidController : ControllerBase
{
    private readonly ISeasonalAidService _seasonalAidService;
    private readonly ILogger<SeasonalAidController> _logger;

    public SeasonalAidController(
        ISeasonalAidService seasonalAidService,
        ILogger<SeasonalAidController> logger)
    {
        _seasonalAidService = seasonalAidService;
        _logger = logger;
    }

    #region CRUD Operations

    /// <summary>
    /// Get all campaigns with filtering and pagination (UC-9.6 / UC-PRJ-01)
    /// </summary>
    [HttpGet("campaigns")]
    [Authorize(Roles = "SuperAdmin,Admin,Charity")]
    [ProducesResponseType(typeof(IEnumerable<SeasonalAidCampaignListDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<(IEnumerable<SeasonalAidCampaignListDto> Items, int TotalCount)>> GetCampaigns(
        [FromQuery] SeasonalAidCampaignFilterDto filter)
    {
        try
        {
            var result = await _seasonalAidService.GetCampaignsAsync(filter);
            return Ok(new { result.Items, result.TotalCount });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving campaigns");
            return StatusCode(500, new { message = "Error retrieving campaigns", error = ex.Message });
        }
    }

    /// <summary>
    /// Get active campaigns
    /// </summary>
    [HttpGet("campaigns/active")]
    [Authorize(Roles = "SuperAdmin,Admin,Charity")]
    [ProducesResponseType(typeof(IEnumerable<SeasonalAidCampaignListDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<SeasonalAidCampaignListDto>>> GetActiveCampaigns()
    {
        try
        {
            var campaigns = await _seasonalAidService.GetActiveCampaignsAsync();
            return Ok(campaigns);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving active campaigns");
            return StatusCode(500, new { message = "Error retrieving active campaigns", error = ex.Message });
        }
    }

    /// <summary>
    /// Get campaign by ID (UC-PRJ-03)
    /// </summary>
    [HttpGet("campaigns/{id}")]
    [Authorize(Roles = "SuperAdmin,Admin,Charity")]
    [ProducesResponseType(typeof(SeasonalAidCampaignDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SeasonalAidCampaignDto>> GetCampaign(Guid id)
    {
        try
        {
            var campaign = await _seasonalAidService.GetCampaignByIdAsync(id);
            if (campaign == null)
            {
                return NotFound(new { message = $"Campaign with ID '{id}' not found" });
            }

            return Ok(campaign);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving campaign: {Id}", id);
            return StatusCode(500, new { message = "Error retrieving campaign", error = ex.Message });
        }
    }

    /// <summary>
    /// Create new campaign (UC-9.1 / UC-PRJ-02)
    /// </summary>
    [HttpPost("campaigns")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    [ProducesResponseType(typeof(SeasonalAidCampaignDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<SeasonalAidCampaignDto>> CreateCampaign([FromBody] CreateSeasonalAidCampaignDto dto)
    {
        try
        {
            var campaign = await _seasonalAidService.CreateCampaignAsync(dto);
            return CreatedAtAction(nameof(GetCampaign), new { id = campaign.Id }, campaign);
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
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating campaign");
            return StatusCode(500, new { message = "Error creating campaign", error = ex.Message });
        }
    }

    /// <summary>
    /// Update campaign (UC-9.8 / UC-PRJ-04)
    /// </summary>
    [HttpPut("campaigns/{id}")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    [ProducesResponseType(typeof(SeasonalAidCampaignDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SeasonalAidCampaignDto>> UpdateCampaign(Guid id, [FromBody] UpdateSeasonalAidCampaignDto dto)
    {
        try
        {
            if (id != dto.Id)
            {
                return BadRequest(new { message = "ID mismatch" });
            }

            var campaign = await _seasonalAidService.UpdateCampaignAsync(dto);
            return Ok(campaign);
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
            _logger.LogError(ex, "Error updating campaign: {Id}", id);
            return StatusCode(500, new { message = "Error updating campaign", error = ex.Message });
        }
    }

    /// <summary>
    /// Delete campaign (UC-PRJ-05)
    /// </summary>
    [HttpDelete("campaigns/{id}")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> DeleteCampaign(Guid id)
    {
        try
        {
            await _seasonalAidService.DeleteCampaignAsync(id);
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
            _logger.LogError(ex, "Error deleting campaign: {Id}", id);
            return StatusCode(500, new { message = "Error deleting campaign", error = ex.Message });
        }
    }

    #endregion

    #region Campaign Period Management (UC-9.2)

    /// <summary>
    /// Set campaign period (UC-9.2)
    /// </summary>
    [HttpPut("campaigns/{id}/period")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> SetCampaignPeriod(Guid id, [FromBody] SetPeriodDto dto)
    {
        try
        {
            await _seasonalAidService.SetCampaignPeriodAsync(id, dto.StartDate, dto.EndDate);
            return Ok(new { message = "Campaign period updated successfully" });
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
            _logger.LogError(ex, "Error setting campaign period: {Id}", id);
            return StatusCode(500, new { message = "Error setting campaign period", error = ex.Message });
        }
    }

    #endregion

    #region Budget Management (UC-9.3)

    /// <summary>
    /// Set campaign budget (UC-9.3)
    /// </summary>
    [HttpPut("campaigns/{id}/budget")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> SetCampaignBudget(Guid id, [FromBody] SetBudgetDto dto)
    {
        try
        {
            await _seasonalAidService.SetCampaignBudgetAsync(id, dto.TotalBudget, dto.Currency, dto.PerFamilyAllocation);
            return Ok(new { message = "Campaign budget updated successfully" });
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
            _logger.LogError(ex, "Error setting campaign budget: {Id}", id);
            return StatusCode(500, new { message = "Error setting campaign budget", error = ex.Message });
        }
    }

    #endregion

    #region Beneficiary Management (UC-9.4, UC-9.7, UC-PRJ-06 through UC-PRJ-10)

    /// <summary>
    /// Get eligible families for a campaign (UC-PRJ-10 — كافة الأسر للمشروع)
    /// </summary>
    [HttpGet("campaigns/{campaignId}/eligible-families")]
    [Authorize(Roles = "SuperAdmin,Admin,Charity")]
    [ProducesResponseType(typeof(IEnumerable<SeasonalAidBeneficiaryDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<(IEnumerable<SeasonalAidBeneficiaryDto> Items, int TotalCount)>> GetEligibleFamilies(
        Guid campaignId, [FromQuery] EligibleFamiliesFilterDto filter)
    {
        try
        {
            filter.CampaignId = campaignId;
            var result = await _seasonalAidService.GetEligibleFamiliesAsync(filter);
            return Ok(new { result.Items, result.TotalCount });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving eligible families for campaign: {CampaignId}", campaignId);
            return StatusCode(500, new { message = "Error retrieving eligible families", error = ex.Message });
        }
    }

    /// <summary>
    /// Register beneficiaries for a campaign — quick add (UC-9.4 / UC-PRJ-07)
    /// </summary>
    [HttpPost("campaigns/{campaignId}/beneficiaries")]
    [Authorize(Roles = "SuperAdmin,Admin,Charity")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> RegisterBeneficiaries(Guid campaignId, [FromBody] CreateSeasonalAidBeneficiaryDto dto)
    {
        try
        {
            dto.CampaignId = campaignId;

            var result = await _seasonalAidService.RegisterBeneficiariesAsync(dto);

            return Ok(new
            {
                message = $"Successfully registered {result.RegisteredCount} beneficiaries",
                registeredCount = result.RegisteredCount,
                totalAllocation = result.TotalAllocation,
                budgetImpact = result.BudgetImpact
            });
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
            _logger.LogError(ex, "Error registering beneficiaries for campaign: {CampaignId}", campaignId);
            return StatusCode(500, new { message = "Error registering beneficiaries", error = ex.Message });
        }
    }

    /// <summary>
    /// Full sync of a campaign's family registrations (UC-PRJ-07 — اختيار الأسر للمشروع).
    /// The payload carries the desired final set of families; adds and removals are applied
    /// as one diff, all-or-nothing.
    /// </summary>
    [HttpPut("campaigns/{campaignId}/beneficiaries")]
    [Authorize(Roles = "SuperAdmin,Admin,Charity")]
    [ProducesResponseType(typeof(UpdateBeneficiariesResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UpdateBeneficiariesResultDto>> UpdateBeneficiaries(
        Guid campaignId, [FromBody] UpdateSeasonalAidBeneficiariesDto dto)
    {
        try
        {
            var result = await _seasonalAidService.UpdateCampaignBeneficiariesAsync(campaignId, dto);
            return Ok(result);
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
            _logger.LogError(ex, "Error updating beneficiaries for campaign: {CampaignId}", campaignId);
            return StatusCode(500, new { message = "Error updating beneficiaries", error = ex.Message });
        }
    }

    /// <summary>
    /// Get campaign beneficiaries (UC-9.7 / UC-PRJ-06, UC-PRJ-09)
    /// </summary>
    [HttpGet("campaigns/{campaignId}/beneficiaries")]
    [Authorize(Roles = "SuperAdmin,Admin,Charity")]
    [ProducesResponseType(typeof(IEnumerable<SeasonalAidBeneficiaryDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<(IEnumerable<SeasonalAidBeneficiaryDto> Items, int TotalCount)>> GetBeneficiaries(
        Guid campaignId, [FromQuery] SeasonalAidBeneficiaryFilterDto filter)
    {
        try
        {
            var result = await _seasonalAidService.GetBeneficiariesAsync(campaignId, filter);
            return Ok(new { result.Items, result.TotalCount });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving beneficiaries for campaign: {CampaignId}", campaignId);
            return StatusCode(500, new { message = "Error retrieving beneficiaries", error = ex.Message });
        }
    }

    /// <summary>
    /// Remove beneficiary from campaign
    /// </summary>
    [HttpDelete("beneficiaries/{beneficiaryId}")]
    [Authorize(Roles = "SuperAdmin,Admin,Charity")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> RemoveBeneficiary(Guid beneficiaryId)
    {
        try
        {
            await _seasonalAidService.RemoveBeneficiaryAsync(beneficiaryId);
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
            _logger.LogError(ex, "Error removing beneficiary: {BeneficiaryId}", beneficiaryId);
            return StatusCode(500, new { message = "Error removing beneficiary", error = ex.Message });
        }
    }

    #endregion

    #region Distribution Management (UC-9.5, UC-PRJ-08)

    /// <summary>
    /// Record aid distribution (UC-9.5)
    /// </summary>
    [HttpPost("beneficiaries/{beneficiaryId}/distributions")]
    [Authorize(Roles = "SuperAdmin,Admin,Charity")]
    [ProducesResponseType(typeof(SeasonalAidDistributionDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<SeasonalAidDistributionDto>> RecordDistribution(
        Guid beneficiaryId, [FromBody] CreateSeasonalAidDistributionDto dto)
    {
        try
        {
            dto.BeneficiaryId = beneficiaryId;

            var distribution = await _seasonalAidService.RecordDistributionAsync(dto);
            return CreatedAtAction(nameof(GetBeneficiary), new { id = beneficiaryId }, distribution);
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
            _logger.LogError(ex, "Error recording distribution for beneficiary: {BeneficiaryId}", beneficiaryId);
            return StatusCode(500, new { message = "Error recording distribution", error = ex.Message });
        }
    }

    /// <summary>
    /// Record multiple distributions at once
    /// </summary>
    [HttpPost("distributions/batch")]
    [Authorize(Roles = "SuperAdmin,Admin,Charity")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> RecordDistributionsBatch([FromBody] List<CreateSeasonalAidDistributionDto> distributions)
    {
        try
        {
            await _seasonalAidService.RecordDistributionsAsync(distributions);
            return Ok(new { message = $"Successfully recorded {distributions.Count} distributions" });
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
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error recording batch distributions");
            return StatusCode(500, new { message = "Error recording distributions", error = ex.Message });
        }
    }

    #endregion

    #region Campaign Closure (UC-9.9)

    /// <summary>
    /// Close campaign (UC-9.9)
    /// </summary>
    [HttpPost("campaigns/{id}/close")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> CloseCampaign(Guid id, [FromBody] CloseCampaignDto dto)
    {
        try
        {
            dto.CampaignId = id;
            await _seasonalAidService.CloseCampaignAsync(dto);
            return Ok(new { message = "Campaign closed successfully" });
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
            _logger.LogError(ex, "Error closing campaign: {Id}", id);
            return StatusCode(500, new { message = "Error closing campaign", error = ex.Message });
        }
    }

    /// <summary>
    /// Reopen campaign
    /// </summary>
    [HttpPost("campaigns/{id}/reopen")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> ReopenCampaign(Guid id)
    {
        try
        {
            await _seasonalAidService.ReopenCampaignAsync(id);
            return Ok(new { message = "Campaign reopened successfully" });
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
            _logger.LogError(ex, "Error reopening campaign: {Id}", id);
            return StatusCode(500, new { message = "Error reopening campaign", error = ex.Message });
        }
    }

    #endregion

    #region Reporting (UC-9.10, UC-PRJ-11, UC-PRJ-12)

    /// <summary>
    /// Generate campaign report (UC-9.10 / UC-PRJ-11, UC-PRJ-12)
    /// </summary>
    [HttpGet("campaigns/{id}/report")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    [ProducesResponseType(typeof(SeasonalAidCampaignReportDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SeasonalAidCampaignReportDto>> GenerateCampaignReport(Guid id)
    {
        try
        {
            var report = await _seasonalAidService.GenerateCampaignReportAsync(id);
            return Ok(report);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating report for campaign: {Id}", id);
            return StatusCode(500, new { message = "Error generating report", error = ex.Message });
        }
    }

    /// <summary>
    /// Export campaign report to PDF (UC-9.10)
    /// </summary>
    [HttpGet("campaigns/{id}/report/pdf")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> ExportCampaignReportToPdf(Guid id)
    {
        try
        {
            var pdfBytes = await _seasonalAidService.ExportCampaignReportToPdfAsync(id);
            return File(pdfBytes, "application/pdf", $"campaign-report-{id}.pdf");
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (NotImplementedException)
        {
            return NotFound(new { message = "PDF export not yet implemented" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting report to PDF for campaign: {Id}", id);
            return StatusCode(500, new { message = "Error exporting report", error = ex.Message });
        }
    }

    /// <summary>
    /// Export campaign report to Excel (UC-9.10)
    /// </summary>
    [HttpGet("campaigns/{id}/report/excel")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> ExportCampaignReportToExcel(Guid id)
    {
        try
        {
            var excelBytes = await _seasonalAidService.ExportCampaignReportToExcelAsync(id);
            return File(excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"campaign-report-{id}.xlsx");
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (NotImplementedException)
        {
            return NotFound(new { message = "Excel export not yet implemented" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting report to Excel for campaign: {Id}", id);
            return StatusCode(500, new { message = "Error exporting report", error = ex.Message });
        }
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Get beneficiary by ID
    /// </summary>
    [HttpGet("beneficiaries/{id}")]
    [Authorize(Roles = "SuperAdmin,Admin,Charity")]
    [ProducesResponseType(typeof(SeasonalAidBeneficiaryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SeasonalAidBeneficiaryDto>> GetBeneficiary(Guid id)
    {
        try
        {
            var beneficiary = await _seasonalAidService.GetBeneficiaryAsync(id);
            if (beneficiary == null)
            {
                return NotFound(new { message = $"Beneficiary with ID '{id}' not found" });
            }

            return Ok(beneficiary);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving beneficiary: {Id}", id);
            return StatusCode(500, new { message = "Error retrieving beneficiary", error = ex.Message });
        }
    }

    #endregion
}

#region Helper DTOs for Controller

public class SetPeriodDto
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}

public class SetBudgetDto
{
    public decimal TotalBudget { get; set; }
    public string Currency { get; set; } = "EGP";
    public decimal PerFamilyAllocation { get; set; }

    public string? DonorName { get; set; }
}

#endregion
