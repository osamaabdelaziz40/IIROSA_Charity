using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.ComponentModel.DataAnnotations;
using IIROSA.Application.Interfaces;
using IIROSA.Application.DTOs.Family;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace IIROSA.Api.Controllers;

/// <summary>
/// Families Controller
/// Implements all family management endpoints (UC-4.1 through UC-4.15)
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class FamiliesController : ControllerBase
{
    private readonly IFamilyService _familyService;
    private readonly ILogger<FamiliesController> _logger;

    public FamiliesController(
        IFamilyService familyService,
        ILogger<FamiliesController> logger)
    {
        _familyService = familyService;
        _logger = logger;
    }

    #region CRUD Operations

    /// <summary>
    /// Get all families with filtering and pagination (UC-4.12: View Family List)
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "SuperAdmin,Admin,Charity")]
    [ProducesResponseType(typeof(IEnumerable<FamilyListDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<(IEnumerable<FamilyListDto> Items, int TotalCount)>> GetFamilies(
        [FromQuery] FamilyFilterDto filter)
    {
        try
        {
            var userCharityId = GetUserCharityId();
            var userRole = GetUserRole();

            var result = await _familyService.GetFamiliesAsync(filter, userCharityId, userRole);
            return Ok(new { result.Items, result.TotalCount });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving families");
            return StatusCode(500, new { message = "Error retrieving families", error = ex.Message });
        }
    }

    /// <summary>
    /// Get family by ID (UC-4.13: View Family Details)
    /// </summary>
    [HttpGet("{id}")]
    [Authorize(Roles = "SuperAdmin,Admin,Charity")]
    [ProducesResponseType(typeof(FamilyDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<FamilyDto>> GetFamily(Guid id)
    {
        try
        {
            var userCharityId = GetUserCharityId();
            var userRole = GetUserRole();

            var family = await _familyService.GetFamilyByIdAsync(id, userCharityId, userRole);
            return Ok(family);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving family: {Id}", id);
            return StatusCode(500, new { message = "Error retrieving family", error = ex.Message });
        }
    }

    /// <summary>
    /// Create new family (UC-4.1: Register Family)
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "SuperAdmin,Admin,Charity")]
    [ProducesResponseType(typeof(FamilyDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<FamilyDto>> CreateFamily([FromBody] CreateFamilyDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Set charity ID from current user for Charity role
            if (User.IsInRole("Charity"))
            {
                var userCharityId = GetUserCharityId();
                if (userCharityId.HasValue)
                {
                    dto.CharityId = userCharityId.Value;
                }
            }

            var family = await _familyService.CreateFamilyAsync(dto);
            return CreatedAtAction(nameof(GetFamily), new { id = family.Id }, family);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating family");
            return StatusCode(500, new { message = "Error creating family", error = ex.Message });
        }
    }

    /// <summary>
    /// Update family (UC-4.8: Update Family Information)
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Roles = "SuperAdmin,Admin,Charity")]
    [ProducesResponseType(typeof(FamilyDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<FamilyDto>> UpdateFamily(Guid id, [FromBody] UpdateFamilyDto dto)
    {
        try
        {
            dto.Id = id;
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Verify access for Charity role
            if (User.IsInRole("Charity"))
            {
                var userCharityId = GetUserCharityId();
                var existingFamily = await _familyService.GetByIdAsync(id);
                if (existingFamily != null && existingFamily.CharityId != userCharityId)
                {
                    return Forbid();
                }
            }

            var family = await _familyService.UpdateFamilyAsync(dto);
            return Ok(family);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating family: {Id}", id);
            return StatusCode(500, new { message = "Error updating family", error = ex.Message });
        }
    }

    /// <summary>
    /// Delete family
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> DeleteFamily(Guid id)
    {
        try
        {
            await _familyService.DeactivateFamilyAsync(id);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting family: {Id}", id);
            return StatusCode(500, new { message = "Error deleting family", error = ex.Message });
        }
    }

    #endregion

    #region Father Management (UC-4.2, UC-4.9)

    /// <summary>
    /// Add father to family (UC-4.2: Add Family Father)
    /// </summary>
    [HttpPost("{familyId}/father")]
    [Authorize(Roles = "SuperAdmin,Admin,Charity")]
    [ProducesResponseType(typeof(FatherDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<FatherDto>> AddFather(Guid familyId, [FromBody] CreateFatherDto dto)
    {
        try
        {
            // Verify access for Charity role
            if (User.IsInRole("Charity"))
            {
                var userCharityId = GetUserCharityId();
                var family = await _familyService.GetByIdAsync(familyId);
                if (family != null && family.CharityId != userCharityId)
                {
                    return Forbid();
                }
            }

            var father = await _familyService.AddFatherToFamilyAsync(familyId, dto);
            return Ok(father);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding father to family: {FamilyId}", familyId);
            return StatusCode(500, new { message = "Error adding father", error = ex.Message });
        }
    }

    /// <summary>
    /// Update father details (UC-4.9: Update Father Details)
    /// </summary>
    [HttpPut("father/{fatherId}")]
    [Authorize(Roles = "SuperAdmin,Admin,Charity")]
    [ProducesResponseType(typeof(FatherDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<FatherDto>> UpdateFather(Guid fatherId, [FromBody] UpdateFatherDto dto)
    {
        try
        {
            dto.Id = fatherId;
            var father = await _familyService.UpdateFatherAsync(dto);
            return Ok(father);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating father: {FatherId}", fatherId);
            return StatusCode(500, new { message = "Error updating father", error = ex.Message });
        }
    }

    #endregion

    #region Mother Management (UC-4.3, UC-4.10)

    /// <summary>
    /// Add mother to family (UC-4.3: Add Family Mother)
    /// </summary>
    [HttpPost("{familyId}/mother")]
    [Authorize(Roles = "SuperAdmin,Admin,Charity")]
    [ProducesResponseType(typeof(MotherDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<MotherDto>> AddMother(Guid familyId, [FromBody] CreateMotherDto dto)
    {
        try
        {
            // Verify access for Charity role
            if (User.IsInRole("Charity"))
            {
                var userCharityId = GetUserCharityId();
                var family = await _familyService.GetByIdAsync(familyId);
                if (family != null && family.CharityId != userCharityId)
                {
                    return Forbid();
                }
            }

            var mother = await _familyService.AddMotherToFamilyAsync(familyId, dto);
            return Ok(mother);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding mother to family: {FamilyId}", familyId);
            return StatusCode(500, new { message = "Error adding mother", error = ex.Message });
        }
    }

    /// <summary>
    /// Update mother details (UC-4.10: Update Mother Details)
    /// </summary>
    [HttpPut("mother/{motherId}")]
    [Authorize(Roles = "SuperAdmin,Admin,Charity")]
    [ProducesResponseType(typeof(MotherDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<MotherDto>> UpdateMother(Guid motherId, [FromBody] UpdateMotherDto dto)
    {
        try
        {
            dto.Id = motherId;
            var mother = await _familyService.UpdateMotherAsync(dto);
            return Ok(mother);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating mother: {MotherId}", motherId);
            return StatusCode(500, new { message = "Error updating mother", error = ex.Message });
        }
    }

    #endregion

    #region Relative Management (UC-4.7, UC-4.8, UC-4.9)

    /// <summary>
    /// Add relative to family (UC-4.7: Add Family Relative)
    /// </summary>
    [HttpPost("{familyId}/relatives")]
    [Authorize(Roles = "SuperAdmin,Admin,Charity")]
    [ProducesResponseType(typeof(RelativeDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RelativeDto>> AddRelative(Guid familyId, [FromBody] CreateRelativeDto dto)
    {
        try
        {
            // Verify access for Charity role
            if (User.IsInRole("Charity"))
            {
                var userCharityId = GetUserCharityId();
                var family = await _familyService.GetByIdAsync(familyId);
                if (family != null && family.CharityId != userCharityId)
                {
                    return Forbid();
                }
            }

            var relative = await _familyService.AddRelativeToFamilyAsync(familyId, dto);
            return Ok(relative);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding relative to family: {FamilyId}", familyId);
            return StatusCode(500, new { message = "Error adding relative", error = ex.Message });
        }
    }

    /// <summary>
    /// Update relative details (UC-4.8: Update Relative Details)
    /// </summary>
    [HttpPut("relatives/{relativeId}")]
    [Authorize(Roles = "SuperAdmin,Admin,Charity")]
    [ProducesResponseType(typeof(RelativeDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RelativeDto>> UpdateRelative(Guid relativeId, [FromBody] UpdateRelativeDto dto)
    {
        try
        {
            dto.Id = relativeId;
            var relative = await _familyService.UpdateRelativeAsync(dto);
            return Ok(relative);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating relative: {RelativeId}", relativeId);
            return StatusCode(500, new { message = "Error updating relative", error = ex.Message });
        }
    }

    /// <summary>
    /// Remove relative from family (UC-4.9: Remove Relative from Family)
    /// </summary>
    [HttpDelete("{familyId}/relatives/{relativeId}")]
    [Authorize(Roles = "SuperAdmin,Admin,Charity")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> RemoveRelative(Guid familyId, Guid relativeId)
    {
        try
        {
            // Verify access for Charity role
            if (User.IsInRole("Charity"))
            {
                var userCharityId = GetUserCharityId();
                var family = await _familyService.GetByIdAsync(familyId);
                if (family != null && family.CharityId != userCharityId)
                {
                    return Forbid();
                }
            }

            await _familyService.RemoveRelativeAsync(familyId, relativeId);
            return Ok(new { message = "Relative removed successfully" });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing relative: {RelativeId}", relativeId);
            return StatusCode(500, new { message = "Error removing relative", error = ex.Message });
        }
    }

    /// <summary>
    /// Get family relatives
    /// </summary>
    [HttpGet("{familyId}/relatives")]
    [Authorize(Roles = "SuperAdmin,Admin,Charity")]
    [ProducesResponseType(typeof(IEnumerable<RelativeListDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<RelativeListDto>>> GetFamilyRelatives(Guid familyId)
    {
        try
        {
            // Verify access for Charity role
            if (User.IsInRole("Charity"))
            {
                var userCharityId = GetUserCharityId();
                var family = await _familyService.GetByIdAsync(familyId);
                if (family != null && family.CharityId != userCharityId)
                {
                    return Forbid();
                }
            }

            var relatives = await _familyService.GetFamilyRelativesAsync(familyId);
            return Ok(relatives);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving relatives for family: {FamilyId}", familyId);
            return StatusCode(500, new { message = "Error retrieving relatives", error = ex.Message });
        }
    }

    #endregion

    #region Orphan Management (UC-4.4, UC-4.11)

    /// <summary>
    /// Add orphan to family (UC-4.4: Add Orphan to Family)
    /// </summary>
    [HttpPost("{familyId}/orphans")]
    [Authorize(Roles = "SuperAdmin,Admin,Charity")]
    [ProducesResponseType(typeof(OrphanDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OrphanDto>> AddOrphan(Guid familyId, [FromBody] CreateOrphanDto dto)
    {
        try
        {
            // Verify access for Charity role
            if (User.IsInRole("Charity"))
            {
                var userCharityId = GetUserCharityId();
                var family = await _familyService.GetByIdAsync(familyId);
                if (family != null && family.CharityId != userCharityId)
                {
                    return Forbid();
                }
            }

            var orphan = await _familyService.AddOrphanToFamilyAsync(familyId, dto);
            return Ok(orphan);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding orphan to family: {FamilyId}", familyId);
            return StatusCode(500, new { message = "Error adding orphan", error = ex.Message });
        }
    }

    /// <summary>
    /// Update orphan details (UC-4.11: Update Orphan Details)
    /// </summary>
    [HttpPut("orphans/{orphanId}")]
    [Authorize(Roles = "SuperAdmin,Admin,Charity")]
    [ProducesResponseType(typeof(OrphanDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OrphanDto>> UpdateOrphan(Guid orphanId, [FromBody] UpdateOrphanDto dto)
    {
        try
        {
            dto.Id = orphanId;
            var orphan = await _familyService.UpdateOrphanAsync(dto);
            return Ok(orphan);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating orphan: {OrphanId}", orphanId);
            return StatusCode(500, new { message = "Error updating orphan", error = ex.Message });
        }
    }

    /// <summary>
    /// Get family orphans
    /// </summary>
    [HttpGet("{familyId}/orphans")]
    [Authorize(Roles = "SuperAdmin,Admin,Charity")]
    [ProducesResponseType(typeof(IEnumerable<OrphanListDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<OrphanListDto>>> GetFamilyOrphans(Guid familyId)
    {
        try
        {
            // Verify access for Charity role
            if (User.IsInRole("Charity"))
            {
                var userCharityId = GetUserCharityId();
                var family = await _familyService.GetByIdAsync(familyId);
                if (family != null && family.CharityId != userCharityId)
                {
                    return Forbid();
                }
            }

            var orphans = await _familyService.GetFamilyOrphansAsync(familyId);
            return Ok(orphans);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving orphans for family: {FamilyId}", familyId);
            return StatusCode(500, new { message = "Error retrieving orphans", error = ex.Message });
        }
    }

    #endregion

    #region Provider Management (UC-4.5, UC-4.6, UC-4.7)

    /// <summary>
    /// Set provider type (UC-4.5: Specify Provider Type)
    /// </summary>
    [HttpPut("{familyId}/provider-type")]
    [Authorize(Roles = "SuperAdmin,Admin,Charity")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> SetProviderType(Guid familyId, [FromBody] string providerType)
    {
        try
        {
            // Verify access for Charity role
            if (User.IsInRole("Charity"))
            {
                var userCharityId = GetUserCharityId();
                var family = await _familyService.GetByIdAsync(familyId);
                if (family != null && family.CharityId != userCharityId)
                {
                    return Forbid();
                }
            }

            await _familyService.SetProviderTypeAsync(familyId, providerType);
            return Ok(new { message = $"Provider type set to {providerType}" });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting provider type for family: {FamilyId}", familyId);
            return StatusCode(500, new { message = "Error setting provider type", error = ex.Message });
        }
    }

    /// <summary>
    /// Add non-parent provider (UC-4.6: Add Non-Parent Provider)
    /// </summary>
    [HttpPost("{familyId}/provider")]
    [Authorize(Roles = "SuperAdmin,Admin,Charity")]
    [ProducesResponseType(typeof(ProviderDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProviderDto>> AddProvider(Guid familyId, [FromBody] CreateProviderDto dto)
    {
        try
        {
            // Verify access for Charity role
            if (User.IsInRole("Charity"))
            {
                var userCharityId = GetUserCharityId();
                var family = await _familyService.GetByIdAsync(familyId);
                if (family != null && family.CharityId != userCharityId)
                {
                    return Forbid();
                }
            }

            var provider = await _familyService.AddProviderToFamilyAsync(familyId, dto);
            return Ok(provider);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding provider to family: {FamilyId}", familyId);
            return StatusCode(500, new { message = "Error adding provider", error = ex.Message });
        }
    }

    /// <summary>
    /// Verify parent as provider (UC-4.7: Verify Parent as Provider)
    /// </summary>
    [HttpPost("{familyId}/verify-provider")]
    [Authorize(Roles = "SuperAdmin,Admin,Charity")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> VerifyParentProvider(Guid familyId, [FromBody] VerifyProviderDto dto)
    {
        try
        {
            // Verify access for Charity role
            if (User.IsInRole("Charity"))
            {
                var userCharityId = GetUserCharityId();
                var family = await _familyService.GetByIdAsync(familyId);
                if (family != null && family.CharityId != userCharityId)
                {
                    return Forbid();
                }
            }

            await _familyService.VerifyParentProviderAsync(familyId, dto.FatherIsProvider, dto.MotherIsProvider, dto.Notes);
            return Ok(new { message = "Provider verification completed" });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying provider for family: {FamilyId}", familyId);
            return StatusCode(500, new { message = "Error verifying provider", error = ex.Message });
        }
    }

    #endregion

    #region Deactivate Family (UC-4.14)

    /// <summary>
    /// Deactivate family (UC-4.14: Deactivate Family)
    /// </summary>
    [HttpPost("{id}/deactivate")]
    [Authorize(Roles = "SuperAdmin,Admin,Charity")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> DeactivateFamily(Guid id)
    {
        try
        {
            // Verify access for Charity role
            if (User.IsInRole("Charity"))
            {
                var userCharityId = GetUserCharityId();
                var family = await _familyService.GetByIdAsync(id);
                if (family != null && family.CharityId != userCharityId)
                {
                    return Forbid();
                }
            }

            await _familyService.DeactivateFamilyAsync(id);
            return Ok(new { message = "Family deactivated successfully" });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deactivating family: {Id}", id);
            return StatusCode(500, new { message = "Error deactivating family", error = ex.Message });
        }
    }

    #endregion

    #region Document Attachments (UC-4.15)

    /// <summary>
    /// Attach document to family (UC-4.15: Attach Family Documents)
    /// </summary>
    [HttpPost("{familyId}/attachments")]
    [Authorize(Roles = "SuperAdmin,Admin,Charity")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Guid>> AttachDocument(Guid familyId, [FromBody] AttachDocumentDto dto)
    {
        try
        {
            // Verify access for Charity role
            if (User.IsInRole("Charity"))
            {
                var userCharityId = GetUserCharityId();
                var family = await _familyService.GetByIdAsync(familyId);
                if (family != null && family.CharityId != userCharityId)
                {
                    return Forbid();
                }
            }

            var attachmentId = await _familyService.AttachDocumentAsync(
                familyId,
                dto.FileName,
                dto.ContentType,
                dto.FileData,
                dto.DocumentType,
                dto.Description);

            return Ok(new { attachmentId, message = "Document attached successfully" });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error attaching document to family: {FamilyId}", familyId);
            return StatusCode(500, new { message = "Error attaching document", error = ex.Message });
        }
    }

    #endregion

    #region Helper Methods

    private Guid? GetUserCharityId()
    {
        var charityIdClaim = User.FindFirst("CharityId")?.Value;
        if (Guid.TryParse(charityIdClaim, out var charityId))
        {
            return charityId;
        }
        return null;
    }

    private string? GetUserRole()
    {
        return User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
    }

    #endregion
}

/// <summary>
/// DTO for verifying parent as provider
/// </summary>
public class VerifyProviderDto
{
    public bool FatherIsProvider { get; set; }
    public bool MotherIsProvider { get; set; }
    public string? Notes { get; set; }
}

/// <summary>
/// DTO for attaching documents
/// </summary>
public class AttachDocumentDto
{
    [Required]
    public string FileName { get; set; } = string.Empty;

    [Required]
    public string ContentType { get; set; } = string.Empty;

    [Required]
    public byte[] FileData { get; set; } = Array.Empty<byte>();

    [Required]
    public string DocumentType { get; set; } = string.Empty;

    public string? Description { get; set; }
}
