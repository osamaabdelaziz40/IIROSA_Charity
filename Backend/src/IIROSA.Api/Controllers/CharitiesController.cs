using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.ComponentModel.DataAnnotations;
using IIROSA.Application.Interfaces;
using IIROSA.Application.DTOs.Charity;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace IIROSA.Api.Controllers;

/// <summary>
/// Charities Controller
/// Implements all charity management endpoints (UC-3.1 through UC-3.14)
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class CharitiesController : ControllerBase
{
    /// <summary>
    /// Mirrors <c>CharityConfiguration</c>'s <c>HasMaxLength(200)</c> on <c>Charity.Name</c>.
    /// </summary>
    private const int CharityNameMaxLength = 200;

    private readonly ICharityService _charityService;
    private readonly ICurrentUserService _currentUser;
    private readonly ILogger<CharitiesController> _logger;

    public CharitiesController(
        ICharityService charityService,
        ICurrentUserService currentUser,
        ILogger<CharitiesController> logger)
    {
        _charityService = charityService;
        _currentUser = currentUser;
        _logger = logger;
    }

    #region CRUD Operations

    /// <summary>
    /// Get all charities with filtering and pagination (UC-3.10)
    /// </summary>
    [HttpGet]
    // A charity user may call this; CharityService scopes the result to their own record, so the
    // list is the same screen for both audiences and the scoping decision stays server-side.
    [Authorize(Roles = "SuperAdmin,Admin,Charity")]
    [ProducesResponseType(typeof(IEnumerable<CharityListDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<(IEnumerable<CharityListDto> Items, int TotalCount)>> GetCharities(
        [FromQuery] CharityFilterDto filter)
    {
        try
        {
            var result = await _charityService.GetCharitiesAsync(filter);
            return Ok(new { result.Items, result.TotalCount });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving charities");
            return StatusCode(500, new { message = "Error retrieving charities", error = ex.Message });
        }
    }

    /// <summary>
    /// Register statistics for the band above the all-charities grid (UC-3.10) — same
    /// caller scope as the list: charity callers get their own record's counts,
    /// country-pinned head-office callers their country's.
    /// </summary>
    // Literal segment beats the {id} route, but it stays above it by convention so the
    // pairing is visible where GetCharity is read.
    [HttpGet("statistics")]
    [Authorize(Roles = "SuperAdmin,Admin,Charity")]
    [ProducesResponseType(typeof(CharityStatisticsDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<CharityStatisticsDto>> GetStatistics()
    {
        try
        {
            var statistics = await _charityService.GetStatisticsAsync();
            return Ok(statistics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving charity statistics");
            return StatusCode(500, new { message = "Error retrieving charity statistics", error = ex.Message });
        }
    }

    /// <summary>
    /// Get charity by ID
    /// </summary>
    [HttpGet("{id}")]
    [Authorize(Roles = "SuperAdmin,Admin,Charity")]
    [ProducesResponseType(typeof(CharityDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CharityDto>> GetCharity(Guid id)
    {
        try
        {
            var charity = await _charityService.GetByIdAsync(id);
            if (charity == null)
            {
                return NotFound(new { message = $"Charity with ID '{id}' not found" });
            }

            // Charity users can only view their own profile
            // Gated on the tenancy claim, not the role name: a charity-bound user holding any
            // other role (Accountant, FinancialOfficer) would otherwise skip the check entirely.
            if (!HasAccessToCharity(id))
            {
                return Forbid();
            }

            return Ok(charity);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving charity: {Id}", id);
            return StatusCode(500, new { message = "Error retrieving charity", error = ex.Message });
        }
    }

    /// <summary>
    /// Get charity profile (UC-3.11)
    /// </summary>
    [HttpGet("{id}/profile")]
    [Authorize(Roles = "SuperAdmin,Admin,Charity")]
    [ProducesResponseType(typeof(CharityProfileDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CharityProfileDto>> GetCharityProfile(Guid id)
    {
        try
        {
            var profile = await _charityService.GetCharityProfileAsync(id);

            // Charity users can only view their own profile
            // Gated on the tenancy claim, not the role name: a charity-bound user holding any
            // other role (Accountant, FinancialOfficer) would otherwise skip the check entirely.
            if (!HasAccessToCharity(id))
            {
                return Forbid();
            }

            return Ok(profile);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving charity profile: {Id}", id);
            return StatusCode(500, new { message = "Error retrieving charity profile", error = ex.Message });
        }
    }

    /// <summary>
    /// Get my profile (for Charity users)
    /// </summary>
    [HttpGet("my-profile")]
    [Authorize(Roles = "Charity")]
    [ProducesResponseType(typeof(CharityDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<CharityDto>> GetMyProfile()
    {
        try
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { message = "User not found" });
            }

            var charityDto = await _charityService.GetByUserIdAsync(userId);
            if (charityDto == null)
            {
                return NotFound(new { message = "Charity not found" });
            }

            return Ok(charityDto);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving my profile");
            return StatusCode(500, new { message = "Error retrieving profile", error = ex.Message });
        }
    }

    /// <summary>
    /// Check whether a charity name is still available (UC-CHR-02).
    /// </summary>
    /// <param name="name">The name to check.</param>
    /// <param name="excludeId">
    /// The charity being edited, if any. Without it, editing a charity without renaming it would
    /// report its own name as taken.
    /// </param>
    /// <remarks>
    /// Uniqueness is register-wide, not per charity, because that is the rule the save enforces.
    /// Reporting a name as free that <c>CreateCharityAsync</c> would then reject would be worse
    /// than not offering the check at all.
    ///
    /// Restricted to the roles that may create or update a charity so the endpoint cannot be used
    /// to enumerate the register by probing names.
    /// </remarks>
    [HttpGet("check-name")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    [ProducesResponseType(typeof(CharityNameAvailabilityDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CharityNameAvailabilityDto>> CheckNameAvailability(
        [FromQuery] string name,
        [FromQuery] Guid? excludeId = null)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return BadRequest(new { message = "A name is required to check availability" });
        }

        var candidate = name.Trim();

        // Charity.Name is nvarchar(200). Without this an over-long name finds no match, is
        // reported available, and then fails the save with a SQL truncation error.
        if (candidate.Length > CharityNameMaxLength)
        {
            return BadRequest(new
            {
                message = $"A charity name may be at most {CharityNameMaxLength} characters"
            });
        }

        try
        {
            var isAvailable = await _charityService.IsNameUniqueAsync(candidate, excludeId);

            return Ok(new CharityNameAvailabilityDto
            {
                Name = candidate,
                IsAvailable = isAvailable
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking charity name availability for {Name}", name);
            // Deliberately no exception detail: on an EF/SqlException ex.Message carries table,
            // column and constraint names. The full exception is in the log above.
            return StatusCode(500, new { message = "Error checking name availability" });
        }
    }

    /// <summary>
    /// Create new charity (UC-3.1)
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "SuperAdmin,Admin")]
    [ProducesResponseType(typeof(CharityDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CharityDto>> CreateCharity([FromBody] CreateCharityDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var charity = await _charityService.CreateCharityAsync(dto);
            return CreatedAtAction(nameof(GetCharity), new { id = charity.Id }, charity);
        }
        // Must precede the catch-all: ValidationException derives from Exception, so without this
        // it was swallowed into a 500 and never reached ExceptionMiddleware, leaving the client
        // with no `errors` map and no way to flag the offending field.
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
            _logger.LogError(ex, "Error creating charity");
            return StatusCode(500, new { message = "Error creating charity", error = ex.Message });
        }
    }

    /// <summary>
    /// Update charity (UC-3.2)
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    [ProducesResponseType(typeof(CharityDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CharityDto>> UpdateCharity(Guid id, [FromBody] UpdateCharityDto dto)
    {
        try
        {
            dto.Id = id;
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            //if (id != dto.Id)
            //{
            //    return BadRequest(new { message = "ID mismatch" });
            //}

            var charity = await _charityService.UpdateCharityAsync(dto);
            return Ok(charity);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
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
            _logger.LogError(ex, "Error updating charity: {Id}", id);
            return StatusCode(500, new { message = "Error updating charity", error = ex.Message });
        }
    }

    /// <summary>
    /// Delete charity
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "SuperAdmin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> DeleteCharity(Guid id)
    {
        try
        {
            await _charityService.DeleteCharityAsync(id);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting charity: {Id}", id);
            return StatusCode(500, new { message = "Error deleting charity", error = ex.Message });
        }
    }

    #endregion

    #region Status Management (UC-3.3, UC-3.4)

    /// <summary>
    /// Activate charity (UC-3.3)
    /// </summary>
    [HttpPost("{id}/activate")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> ActivateCharity(Guid id)
    {
        try
        {
            await _charityService.ActivateCharityAsync(id);
            return Ok(new { message = "Charity activated successfully" });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error activating charity: {Id}", id);
            return StatusCode(500, new { message = "Error activating charity", error = ex.Message });
        }
    }

    /// <summary>
    /// Deactivate charity (UC-3.4)
    /// </summary>
    [HttpPost("{id}/deactivate")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> DeactivateCharity(Guid id)
    {
        try
        {
            await _charityService.DeactivateCharityAsync(id);
            return Ok(new { message = "Charity deactivated successfully" });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deactivating charity: {Id}", id);
            return StatusCode(500, new { message = "Error deactivating charity", error = ex.Message });
        }
    }

    #endregion

    #region Password Management (UC-3.5)

    /// <summary>
    /// Reset charity password (UC-3.5)
    /// </summary>
    [HttpPost("{id}/reset-password")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<string>> ResetPassword(Guid id)
    {
        try
        {
            var newPassword = await _charityService.ResetPasswordAsync(id);
            return Ok(new { message = "Password reset successfully", newPassword });
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
            _logger.LogError(ex, "Error resetting password for charity: {Id}", id);
            return StatusCode(500, new { message = "Error resetting password", error = ex.Message });
        }
    }

    #endregion

    #region Rights Management (UC-3.6, UC-3.7)

    /// <summary>
    /// Enable/Disable add rights (UC-3.6)
    /// </summary>
    [HttpPut("{id}/add-rights")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> SetAddRights(Guid id, [FromBody] bool isEnabled)
    {
        try
        {
            await _charityService.SetAddRightsAsync(id, isEnabled);
            return Ok(new { message = $"Add rights {(isEnabled ? "enabled" : "disabled")} successfully" });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting add rights for charity: {Id}", id);
            return StatusCode(500, new { message = "Error setting add rights", error = ex.Message });
        }
    }

    /// <summary>
    /// Enable/Disable update rights (UC-3.7)
    /// </summary>
    [HttpPut("{id}/update-rights")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> SetUpdateRights(Guid id, [FromBody] bool isEnabled)
    {
        try
        {
            await _charityService.SetUpdateRightsAsync(id, isEnabled);
            return Ok(new { message = $"Update rights {(isEnabled ? "enabled" : "disabled")} successfully" });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting update rights for charity: {Id}", id);
            return StatusCode(500, new { message = "Error setting update rights", error = ex.Message });
        }
    }

    #endregion

    #region Lock Management (UC-3.8)

    /// <summary>
    /// Lock charity (UC-3.8)
    /// </summary>
    [HttpPost("{id}/lock")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> LockCharity(Guid id)
    {
        try
        {
            await _charityService.LockCharityAsync(id);
            return Ok(new { message = "Charity locked successfully" });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error locking charity: {Id}", id);
            return StatusCode(500, new { message = "Error locking charity", error = ex.Message });
        }
    }

    /// <summary>
    /// Unlock charity
    /// </summary>
    [HttpPost("{id}/unlock")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> UnlockCharity(Guid id)
    {
        try
        {
            await _charityService.UnlockCharityAsync(id);
            return Ok(new { message = "Charity unlocked successfully" });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error unlocking charity: {Id}", id);
            return StatusCode(500, new { message = "Error unlocking charity", error = ex.Message });
        }
    }

    #endregion

    #region Bank Details (UC-3.9)

    /// <summary>
    /// Set bank account details (UC-3.9)
    /// </summary>
    [HttpPut("{id}/bank-details")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> SetBankDetails(Guid id, [FromBody] CharityBankDetailsDto dto)
    {
        try
        {
            if (id != dto.CharityId)
            {
                return BadRequest(new { message = "ID mismatch" });
            }

            await _charityService.SetBankDetailsAsync(dto);
            return Ok(new { message = "Bank details updated successfully" });
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
            _logger.LogError(ex, "Error setting bank details for charity: {Id}", id);
            return StatusCode(500, new { message = "Error setting bank details", error = ex.Message });
        }
    }

    #endregion

    #region Location Management (UC-3.12)

    /// <summary>
    /// Assign charity to center (UC-3.12)
    /// </summary>
    [HttpPut("{id}/location")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> SetLocation(Guid id, [FromBody] CharityLocationDto dto)
    {
        try
        {
            if (id != dto.CharityId)
            {
                return BadRequest(new { message = "ID mismatch" });
            }

            await _charityService.SetLocationAsync(dto);
            return Ok(new { message = "Location updated successfully" });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting location for charity: {Id}", id);
            return StatusCode(500, new { message = "Error setting location", error = ex.Message });
        }
    }

    #endregion

    #region Management Contacts (UC-3.13)

    /// <summary>
    /// Update management contacts (UC-3.13)
    /// </summary>
    [HttpPut("{id}/management-contacts")]
    [Authorize(Roles = "SuperAdmin,Admin,Charity")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> UpdateManagementContacts(Guid id, [FromBody] CharityManagementContactsDto dto)
    {
        try
        {
            if (id != dto.CharityId)
            {
                return BadRequest(new { message = "ID mismatch" });
            }

            // Charity users can only update their own contacts
            // Gated on the tenancy claim, not the role name: a charity-bound user holding any
            // other role (Accountant, FinancialOfficer) would otherwise skip the check entirely.
            if (!HasAccessToCharity(id))
            {
                return Forbid();
            }

            await _charityService.UpdateManagementContactsAsync(dto);
            return Ok(new { message = "Management contacts updated successfully" });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating management contacts for charity: {Id}", id);
            return StatusCode(500, new { message = "Error updating management contacts", error = ex.Message });
        }
    }

    #endregion

    #region Map Location (UC-3.14)

    /// <summary>
    /// Set map location (UC-3.14)
    /// </summary>
    [HttpPut("{id}/map-location")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> SetMapLocation(Guid id, [FromBody] string mapLocation)
    {
        try
        {
            await _charityService.SetMapLocationAsync(id, mapLocation);
            return Ok(new { message = "Map location updated successfully" });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting map location for charity: {Id}", id);
            return StatusCode(500, new { message = "Error setting map location", error = ex.Message });
        }
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Whether the caller may read the given charity.
    /// </summary>
    /// <remarks>
    /// Decided from the signed tenancy claim, never from a request parameter. A caller bound to a
    /// charity may read only that charity; a head-office caller may read any. A caller with no
    /// charity claim and no head-office role is refused, because an unscopeable caller must not
    /// default to full access.
    /// </remarks>
    private bool HasAccessToCharity(Guid charityId)
    {
        if (!_currentUser.IsAuthenticated)
        {
            return false;
        }

        var callerCharityId = _currentUser.CharityId;
        if (callerCharityId.HasValue)
        {
            return callerCharityId.Value == charityId;
        }

        return _currentUser.IsHeadOffice;
    }

    #endregion
}
