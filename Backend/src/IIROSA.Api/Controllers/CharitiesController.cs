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
    private readonly ICharityService _charityService;
    private readonly ILogger<CharitiesController> _logger;

    public CharitiesController(
        ICharityService charityService,
        ILogger<CharitiesController> logger)
    {
        _charityService = charityService;
        _logger = logger;
    }

    #region CRUD Operations

    /// <summary>
    /// Get all charities with filtering and pagination (UC-3.10)
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "SuperAdmin,Admin")]
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
            if (User.IsInRole("Charity") && !HasAccessToCharity(id))
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
            if (User.IsInRole("Charity") && !HasAccessToCharity(id))
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
            if (User.IsInRole("Charity") && !HasAccessToCharity(id))
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

    private bool HasAccessToCharity(Guid charityId)
    {
        // Check if the current user is associated with this charity
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return false;
        }

        // TODO: Implement actual check against charity.UserId
        // For now, this is a placeholder
        return true;
    }

    #endregion
}
