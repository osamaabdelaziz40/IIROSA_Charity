using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.ComponentModel.DataAnnotations;
using IIROSA.Application.Interfaces;
using IIROSA.Application.DTOs.Family;
using IIROSA.Application.DTOs.Reports;
using IIROSA.Application.DTOs.SeasonalAid;
using IIROSA.Application.Exceptions;
using Framework.Core;
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
    private readonly ISeasonalAidService _seasonalAidService;
    private readonly IGuardianChangeRequestService _guardianChangeRequestService;
    private readonly ILogger<FamiliesController> _logger;

    public FamiliesController(
        IFamilyService familyService,
        ISeasonalAidService seasonalAidService,
        IGuardianChangeRequestService guardianChangeRequestService,
        ILogger<FamiliesController> logger)
    {
        _familyService = familyService;
        _seasonalAidService = seasonalAidService;
        _guardianChangeRequestService = guardianChangeRequestService;
        _logger = logger;
    }

    #region Guardian Change Requests (UC-FAM-09/10)

    /// <summary>
    /// The guardian-change review queue (UC-FAM-09 طلبات تعديل العائل) — pending by default,
    /// newest first, paged. HQ roles see every charity's requests and may filter by charity;
    /// a Charity-role caller is scoped server-side to its own requests only.
    /// </summary>
    /// <remarks>
    /// Literal route segment — registered above the <c>{id}</c> templates so the queue path can
    /// never be swallowed by a parameterized route.
    /// </remarks>
    [HttpGet("provider-requests")]
    [Authorize(Roles = "SuperAdmin,Admin,Charity")]
    [ProducesResponseType(typeof(IEnumerable<GuardianChangeRequestListDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<object>> GetGuardianChangeRequests(
        [FromQuery] GuardianChangeRequestFilterDto filter)
    {
        try
        {
            var userCharityId = GetUserCharityId();
            var userRole = GetUserRole();

            var result = await _guardianChangeRequestService.GetRequestsAsync(filter, userCharityId, userRole);
            return Ok(new { result.Items, result.TotalCount });
        }
        catch (FluentValidation.ValidationException ex)
        {
            var response = new ApiResponse
            {
                Success = false,
                Message = "One or more fields are invalid"
            };
            foreach (var group in ex.Errors.GroupBy(error => error.PropertyName ?? string.Empty))
            {
                response.ModelStateErrors.Add(new Item(group.Key, string.Join(" ", group.Select(error => error.ErrorMessage))));
            }
            return BadRequest(response);
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving guardian-change requests");
            return StatusCode(500, new { message = "Error retrieving guardian-change requests" });
        }
    }

    /// <summary>
    /// Raise a guardian-change request for a family (UC-FAM-09 producer) — the charity proposes
    /// a new guardian; the head office reviews and decides (5-10). Refuses a second pending
    /// request on the same family with the legacy message.
    /// </summary>
    [HttpPost("{familyId}/provider-requests")]
    [Authorize(Roles = "SuperAdmin,Admin,Charity")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse>> RaiseGuardianChangeRequest(
        Guid familyId,
        [FromBody] CreateGuardianChangeRequestDto dto)
    {
        try
        {
            var userCharityId = GetUserCharityId();
            var userRole = GetUserRole();

            await _guardianChangeRequestService.CreateRequestAsync(familyId, dto, User.Identity?.Name, userCharityId, userRole);

            return Ok(new ApiResponse { Success = true, Message = "Guardian-change request raised and awaiting head-office review" });
        }
        catch (FluentValidation.ValidationException ex)
        {
            var response = new ApiResponse
            {
                Success = false,
                Message = "One or more fields are invalid"
            };
            foreach (var group in ex.Errors.GroupBy(error => error.PropertyName ?? string.Empty))
            {
                response.ModelStateErrors.Add(new Item(group.Key, string.Join(" ", group.Select(error => error.ErrorMessage))));
            }
            return BadRequest(response);
        }
        catch (NotFoundException ex)
        {
            return NotFound(new ApiResponse { Success = false, Message = ex.Message });
        }
        catch (BusinessException ex)
        {
            return BadRequest(new ApiResponse { Success = false, Message = ex.Message });
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error raising guardian-change request for family {FamilyId}", familyId);
            return StatusCode(500, new ApiResponse { Success = false, Message = "Error raising guardian-change request" });
        }
    }

    /// <summary>
    /// Record the head-office decision on a guardian-change request (UC-FAM-10 اعتماد تعديل
    /// العائل) — General Director only. Approving applies the proposed guardian to the family's
    /// provider row and stamps the request Approved in one transaction; refusing stores the
    /// mandatory reason. Re-deciding an already-decided request is refused with a 400.
    /// </summary>
    [HttpPost("provider-requests/{requestId}/approve")]
    [Authorize(Roles = "SuperAdmin")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse>> DecideGuardianChangeRequest(
        Guid requestId,
        [FromBody] ApproveGuardianChangeRequestDto dto)
    {
        try
        {
            // DecidedBy must resolve back to a user record — the id, not the display name
            // (name collisions and renames would break the audit trail).
            var decidedBy = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                ?? User.Identity?.Name;

            var decided = await _guardianChangeRequestService.DecideRequestAsync(
                requestId, dto, decidedBy, GetUserRole());

            return Ok(new ApiResponse
            {
                Success = true,
                Message = dto.IsApproved
                    ? "Guardian-change request approved and applied to the family file"
                    : "Guardian-change request refused",
                Value = decided
            });
        }
        catch (FluentValidation.ValidationException ex)
        {
            var response = new ApiResponse
            {
                Success = false,
                Message = "One or more fields are invalid"
            };
            foreach (var group in ex.Errors.GroupBy(error => error.PropertyName ?? string.Empty))
            {
                response.ModelStateErrors.Add(new Item(group.Key, string.Join(" ", group.Select(error => error.ErrorMessage))));
            }
            return BadRequest(response);
        }
        catch (NotFoundException ex)
        {
            return NotFound(new ApiResponse { Success = false, Message = ex.Message });
        }
        catch (BusinessException ex)
        {
            return BadRequest(new ApiResponse { Success = false, Message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deciding guardian-change request {RequestId}", requestId);
            return StatusCode(500, new ApiResponse { Success = false, Message = "Error deciding guardian-change request" });
        }
    }

    #endregion

    #region Follow-Up Activity (UC-FAM-11)

    /// <summary>
    /// The family follow-up report (UC-FAM-11 متابعة إدخالات الأسر) — what was created or
    /// updated on the family files for one calendar day. Read-only projection over the audit
    /// columns; HQ roles see every charity (narrowable) and a Charity-role caller is scoped
    /// server-side to its own register.
    /// </summary>
    /// <remarks>
    /// Literal route segment — sits above the <c>{id}</c> templates like the queue paths.
    /// </remarks>
    [HttpGet("follow-up")]
    [Authorize(Roles = "SuperAdmin,Admin,Charity")]
    [ProducesResponseType(typeof(IEnumerable<FamilyFollowUpListDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<object>> GetFollowUpActivity(
        [FromQuery] FamilyFollowUpFilterDto filter)
    {
        try
        {
            var userCharityId = GetUserCharityId();
            var userRole = GetUserRole();

            var result = await _familyService.GetFollowUpActivityAsync(filter, userCharityId, userRole);
            return Ok(new { result.Items, result.TotalCount });
        }
        catch (FluentValidation.ValidationException ex)
        {
            var response = new ApiResponse
            {
                Success = false,
                Message = "One or more fields are invalid"
            };
            foreach (var group in ex.Errors.GroupBy(error => error.PropertyName ?? string.Empty))
            {
                response.ModelStateErrors.Add(new Item(group.Key, string.Join(" ", group.Select(error => error.ErrorMessage))));
            }
            return BadRequest(response);
        }
        catch (BusinessException ex)
        {
            return BadRequest(new ApiResponse { Success = false, Message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving family follow-up activity");
            return StatusCode(500, new { message = "Error retrieving family follow-up activity" });
        }
    }

    /// <summary>
    /// The family & orphan entry-tracking report (UC-RPT-13 متابعة إدخلات الأسر والأيتام) —
    /// registrations on/after a date, one endpoint for the §23.S.10 screen's two data
    /// commands: <c>mode=totals</c> (إجماليات) and <c>mode=details</c> (تفاصيل). Distinct
    /// from UC-FAM-11's literal <c>follow-up</c> route (one-day activity). Read-only; a
    /// charity caller is clamped server-side to its own register whatever the route id says.
    /// </summary>
    /// <remarks>Route id is the charity; Guid.Empty (<c>000…000</c>) means كل الجهات (HQ only).</remarks>
    [HttpGet("{id}/follow-up")]
    [Authorize(Roles = "SuperAdmin,Admin,Charity")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetFamilyFollowUp(
        Guid id,
        [FromQuery] DateTime? date,
        [FromQuery] string? mode,
        [FromQuery] int? page,
        [FromQuery] int? pageSize)
    {
        try
        {
            var filter = new FamilyEntryTrackingFilterDto
            {
                CharityId = id == Guid.Empty ? null : id,
                Date = date,
                Mode = string.IsNullOrWhiteSpace(mode) ? "details" : mode,
                Page = page ?? 1,
                PageSize = pageSize ?? 20
            };

            var result = await _familyService.GetFamilyFollowUpAsync(
                filter, GetUserCharityId(), GetUserRole());
            return Ok(result);
        }
        catch (FluentValidation.ValidationException ex)
        {
            var errors = ex.Errors
                .GroupBy(error => error.PropertyName ?? string.Empty)
                .ToDictionary(
                    group => group.Key,
                    group => string.Join(" ", group.Select(error => error.ErrorMessage)));

            return BadRequest(new { message = "One or more fields are invalid", errors });
        }
        catch (BusinessException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving the family entry-tracking report");
            return StatusCode(500, new { message = "Error retrieving the family entry-tracking report" });
        }
    }

    #endregion

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

            // D4 fail-closed (epic-7 review): a Charity token whose charity claim fails to parse
            // must be refused here — otherwise the caller-supplied dto.CharityId would be minted
            // verbatim as the family's owner.
            if (User.IsInRole("Charity") && GetUserCharityId() == null)
            {
                return Forbid();
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
        catch (FluentValidation.ValidationException ex)
        {
            // Refugee-register §12.S.2 mandatory flags (CreateRefugeeFamilyValidator) surface here
            var response = new ApiResponse
            {
                Success = false,
                Message = "One or more fields are invalid"
            };
            foreach (var group in ex.Errors.GroupBy(error => error.PropertyName ?? string.Empty))
            {
                response.ModelStateErrors.Add(new Item(group.Key, string.Join(" ", group.Select(error => error.ErrorMessage))));
            }
            return BadRequest(response);
        }
        catch (BusinessException ex)
        {
            // e.g. duplicate provider national id in the refugee register
            return BadRequest(new ApiResponse { Success = false, Message = ex.Message });
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
        catch (FluentValidation.ValidationException ex)
        {
            var response = new ApiResponse
            {
                Success = false,
                Message = "One or more fields are invalid"
            };
            foreach (var group in ex.Errors.GroupBy(error => error.PropertyName ?? string.Empty))
            {
                response.ModelStateErrors.Add(new Item(group.Key, string.Join(" ", group.Select(error => error.ErrorMessage))));
            }
            return BadRequest(response);
        }
        catch (BusinessException ex)
        {
            return BadRequest(new ApiResponse { Success = false, Message = ex.Message });
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

    #region Orphan Register & Coding (UC-ORP-01 through UC-ORP-10)

    /// <summary>
    /// Search orphans by name or code, resolve a code back to its orphan, or open the coding
    /// worklist (UC-ORP-02, UC-ORP-03, UC-ORP-07).
    /// One endpoint, two query modes: a plain search serves every role; supplying
    /// <c>codingStatus=Pending</c> makes it the HQ-only coding worklist (the role gate is enforced
    /// in the service, not the menu).
    /// </summary>
    [HttpGet("orphans")]
    [Authorize(Roles = "SuperAdmin,Admin,Charity")]
    [ProducesResponseType(typeof(OrphanLookupDto), StatusCodes.Status200OK)]
    public async Task<ActionResult> SearchOrphans([FromQuery] OrphanSearchFilterDto filter)
    {
        try
        {
            // D4 fail-closed: a Charity-role token without a parseable charity claim must never
            // fall through to the unscoped (HQ) branch.
            if (User.IsInRole("Charity") && GetUserCharityId() == null)
            {
                return Forbid();
            }

            var result = await _familyService.SearchOrphansAsync(filter, GetUserCharityId(), GetUserRole());
            return Ok(new { result.Items, result.TotalCount, page = filter.PageNumber });
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (FluentValidation.ValidationException vex)
        {
            return BadRequest(new { message = "Validation failed", errors = vex.Errors.Select(e => e.ErrorMessage) });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching orphans");
            return StatusCode(500, new { message = "Error searching orphans" });
        }
    }

    /// <summary>
    /// Check whether an orphan may be added (UC-ORP-01): the charity's write state and the
    /// national ID's prior registration, as a verdict the orphan form flags before saving.
    /// </summary>
    [HttpGet("orphans/check-national-id")]
    [Authorize(Roles = "SuperAdmin,Admin,Charity")]
    [ProducesResponseType(typeof(OrphanEligibilityDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<OrphanEligibilityDto>> CheckOrphanCanBeAdded([FromQuery] OrphanEligibilityCheckDto check)
    {
        try
        {
            // D4 fail-closed: a Charity-role token without a parseable charity claim must never
            // fall through to the unscoped (HQ) branch.
            if (User.IsInRole("Charity") && GetUserCharityId() == null)
            {
                return Forbid();
            }

            var verdict = await _familyService.CheckOrphanCanBeAddedAsync(check, GetUserCharityId(), GetUserRole());
            return Ok(verdict);
        }
        catch (FluentValidation.ValidationException vex)
        {
            return BadRequest(new { message = "Validation failed", errors = vex.Errors.Select(e => e.ErrorMessage) });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking whether an orphan may be added");
            return StatusCode(500, new { message = "Error checking orphan eligibility" });
        }
    }

    /// <summary>
    /// Check a national id against every person holder in the register (UC-SYS-12): a family-level
    /// uniqueness verdict that names any clash — holder name, family code and holder kind — so the
    /// user can decide rather than guess. Soft-deleted rows never block; the family being edited
    /// (familyId) never self-reports.
    /// </summary>
    [HttpGet("check-national-id")]
    [Authorize(Roles = "SuperAdmin,Admin,Charity")]
    [ProducesResponseType(typeof(FamilyNationalIdCheckResultDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<FamilyNationalIdCheckResultDto>> CheckFamilyNationalId([FromQuery] CheckFamilyNationalIdDto check)
    {
        try
        {
            // D4 fail-closed: a Charity-role token without a parseable charity claim must never
            // fall through to the unscoped (HQ) branch.
            if (User.IsInRole("Charity") && GetUserCharityId() == null)
            {
                return Forbid();
            }

            var verdict = await _familyService.CheckFamilyNationalIdAsync(check, GetUserCharityId(), GetUserRole());
            return Ok(verdict);
        }
        catch (FluentValidation.ValidationException vex)
        {
            return BadRequest(new { message = "Validation failed", errors = vex.Errors.Select(e => e.ErrorMessage) });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking national id uniqueness");
            return StatusCode(500, new { message = "Error checking national id" });
        }
    }

    /// <summary>
    /// Verify a sponsorship code is not already used (UC-ORP-05, BR-07: unique within the charity) —
    /// the inline check behind the worklist's تعديل الكود command.
    /// </summary>
    [HttpGet("orphans/check-code")]
    [Authorize(Roles = "SuperAdmin,Admin,Charity")]
    [ProducesResponseType(typeof(OrphanCodeCheckDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<OrphanCodeCheckDto>> CheckOrphanCode([FromQuery] OrphanCodeCheckFilterDto filter)
    {
        try
        {
            // D4 fail-closed: a Charity-role token without a parseable charity claim must never
            // fall through to the unscoped (HQ) branch.
            if (User.IsInRole("Charity") && GetUserCharityId() == null)
            {
                return Forbid();
            }

            var verdict = await _familyService.CheckOrphanCodeUniqueAsync(filter, GetUserCharityId(), GetUserRole());
            return Ok(verdict);
        }
        catch (FluentValidation.ValidationException vex)
        {
            return BadRequest(new { message = "Validation failed", errors = vex.Errors.Select(e => e.ErrorMessage) });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying orphan code");
            return StatusCode(500, new { message = "Error verifying orphan code" });
        }
    }

    /// <summary>
    /// Assign a sponsorship code to an orphan (UC-ORP-06) — the worklist's SaveCode command.
    /// HQ-only (SuperAdmin/Admin, D3): the worklist is a head-office function, so the write is
    /// too. Uniqueness is re-checked inside the save; the first code assignment flips the
    /// orphan's SponsorshipStatus to "Unsponsored".
    /// </summary>
    [HttpPost("orphans/{orphanId}/code")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    [ProducesResponseType(typeof(OrphanDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OrphanDto>> AssignOrphanCode(Guid orphanId, [FromBody] AssignOrphanCodeDto dto)
    {
        try
        {
            // D4 fail-closed: a Charity-role token without a parseable charity claim must never
            // fall through to the unscoped (HQ) branch.
            if (User.IsInRole("Charity") && GetUserCharityId() == null)
            {
                return Forbid();
            }

            dto.OrphanId = orphanId;
            var orphan = await _familyService.AssignOrphanCodeAsync(dto, GetUserCharityId(), GetUserRole());
            return Ok(orphan);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (FluentValidation.ValidationException vex)
        {
            return BadRequest(new { message = "Validation failed", errors = vex.Errors.Select(e => e.ErrorMessage) });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Microsoft.EntityFrameworkCore.DbUpdateException)
        {
            // D1: the BR-07 gate raced another coder and the database's unique index lost —
            // the clash is a 400, not a 500.
            return BadRequest(new { message = $"Code '{dto.Code.Trim()}' was assigned to another orphan while this one was being saved. Re-check the code." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error assigning code to orphan: {OrphanId}", orphanId);
            return StatusCode(500, new { message = "Error assigning orphan code" });
        }
    }

    /// <summary>
    /// Check a phone number is not duplicated (UC-ORP-10) — the check behind every phone input of
    /// the family form. Spans the five phone holders of every other family in scope.
    /// </summary>
    [HttpGet("{familyId}/provider/check-phone")]
    [Authorize(Roles = "SuperAdmin,Admin,Charity")]
    [ProducesResponseType(typeof(PhoneCheckDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PhoneCheckDto>> CheckPhoneDuplicate(Guid familyId, [FromQuery] PhoneCheckFilterDto filter)
    {
        try
        {
            // D4 fail-closed: a Charity-role token without a parseable charity claim must never
            // fall through to the unscoped (HQ) branch.
            if (User.IsInRole("Charity") && GetUserCharityId() == null)
            {
                return Forbid();
            }

            var verdict = await _familyService.CheckPhoneNumberDuplicateAsync(familyId, filter, GetUserCharityId(), GetUserRole());
            return Ok(verdict);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (FluentValidation.ValidationException vex)
        {
            return BadRequest(new { message = "Validation failed", errors = vex.Errors.Select(e => e.ErrorMessage) });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking phone duplication for family: {FamilyId}", familyId);
            return StatusCode(500, new { message = "Error checking phone duplication" });
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
            // Row-level tenancy lives in the service now (UpdateProviderAsync pattern, against
            // the live FK_CharityId column) — the old inline guard compared FamilyDto.CharityId,
            // the mirror column not stamped on all rows, so it Forbade rightful owners and fell
            // open on a null-vs-null compare. D4 fail-closed: a Charity token without a parseable
            // charity claim is refused before the service call.
            if (User.IsInRole("Charity") && GetUserCharityId() == null)
            {
                return Forbid();
            }

            var provider = await _familyService.AddProviderToFamilyAsync(familyId, dto, GetUserCharityId(), GetUserRole());
            return Ok(provider);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (IIROSA.Application.Interfaces.CharityWriteForbiddenException ex)
        {
            // Locked / add-disabled charity — a permission refusal, not a server error. Without
            // this catch the generic Exception handler below turns it into a 500 and the
            // middleware's dedicated 403 mapping is never reached.
            return StatusCode(StatusCodes.Status403Forbidden, new { message = ex.Message });
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding provider to family: {FamilyId}", familyId);
            return StatusCode(500, new { message = "Error adding provider" });
        }
    }

    /// <summary>
    /// Get the family's guardian (UC-REF-04 — §12.S.2 view/edit screens, per-member GET)
    /// </summary>
    [HttpGet("{familyId}/provider")]
    // Review D4 2026-08-26: role-gated like its sibling reads — the bare [Authorize] let a
    // no-role token through as userRole=null, which every FamilyService scope gate treats as
    // head office (fail-open).
    [Authorize(Roles = "SuperAdmin,Admin,Charity")]
    [ProducesResponseType(typeof(ProviderDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProviderDto>> GetProvider(Guid familyId)
    {
        try
        {
            // D4 fail-closed: a Charity token without a parseable charity claim never falls
            // through to the unscoped branch (the service gate repeats the same rule).
            if (User.IsInRole("Charity") && GetUserCharityId() == null)
            {
                return Forbid();
            }

            var userCharityId = GetUserCharityId();
            var userRole = GetUserRole();

            var provider = await _familyService.GetProviderByFamilyAsync(familyId, userCharityId, userRole);
            return Ok(provider);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting provider for family: {FamilyId}", familyId);
            return StatusCode(500, new { message = "Error getting provider", error = ex.Message });
        }
    }

    /// <summary>
    /// Update the family's guardian (UC-REF-04 — §12.S.2 refugee edit screen, per-member PUT)
    /// </summary>
    [HttpPut("{familyId}/provider")]
    [Authorize(Roles = "SuperAdmin,Admin,Charity")]
    [ProducesResponseType(typeof(ProviderDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProviderDto>> UpdateProvider(Guid familyId, [FromBody] UpdateProviderDto dto)
    {
        try
        {
            // D4 fail-closed: a Charity token without a parseable charity claim must be refused.
            // Row-level tenancy moved INTO the service (against the live FK_CharityId column) —
            // the old guard compared FamilyDto.CharityId, the mirror column not stamped on all
            // rows, which Forbade rightful owners and failed open on a null-vs-null compare.
            if (User.IsInRole("Charity") && GetUserCharityId() == null)
            {
                return Forbid();
            }

            var provider = await _familyService.UpdateProviderAsync(familyId, dto, GetUserCharityId(), GetUserRole());
            return Ok(provider);
        }
        catch (BusinessException ex)
        {
            return BadRequest(new ApiResponse { Success = false, Message = ex.Message });
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating provider for family: {FamilyId}", familyId);
            return StatusCode(500, new { message = "Error updating provider", error = ex.Message });
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
        catch (IIROSA.Application.Interfaces.CharityWriteForbiddenException ex)
        {
            // Locked / add-disabled charity — a permission refusal, not a server error. Without
            // this catch the generic Exception handler below turns it into a 500 and the
            // middleware's dedicated 403 mapping is never reached.
            return StatusCode(StatusCodes.Status403Forbidden, new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying provider for family: {FamilyId}", familyId);
            return StatusCode(500, new { message = "Error verifying provider" });
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

    #region Seasonal Aid Received Flag (UC-PRJ-08)

    /// <summary>
    /// Confirm or withdraw a family's receipt of seasonal assistance (UC-PRJ-08 — تحديث حالة استلام المساعدة).
    /// Routed on Families because the module spec posts to /api/Families/{id}/received-flag; the acting
    /// user is always taken from the token, never from the request body.
    /// </summary>
    [HttpPut("{id}/received-flag")]
    [Authorize(Roles = "SuperAdmin,Admin,Charity")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> SetFamilyReceivedFlag(Guid id, [FromBody] SetFamilyReceivedFlagDto dto)
    {
        try
        {
            await _seasonalAidService.SetFamilyReceivedFlagAsync(id, dto);
            return Ok(new { message = "Family received flag updated successfully" });
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
            _logger.LogError(ex, "Error setting received flag for family: {FamilyId}", id);
            return StatusCode(500, new { message = "Error updating received flag", error = ex.Message });
        }
    }

    #endregion

    #region Transfer Family to Another Charity (UC-FAM-06)

    /// <summary>
    /// Transfer a family and its dependents to another charity (UC-FAM-06 نقل الأسرة لجمعية أخرى).
    /// HQ-only: the receiving charity is validated server-side and the movement is recorded —
    /// family, orphans and the transfer record commit together or not at all.
    /// </summary>
    [HttpPut("{id}/charity")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse>> TransferFamilyToCharity(Guid id, [FromBody] TransferFamilyDto dto)
    {
        try
        {
            var userCharityId = GetUserCharityId();
            var userRole = GetUserRole();

            await _familyService.TransferFamilyToCharityAsync(id, dto, userCharityId, userRole);

            return Ok(new ApiResponse { Success = true, Message = "Family transferred successfully" });
        }
        catch (FluentValidation.ValidationException ex)
        {
            var response = new ApiResponse
            {
                Success = false,
                Message = "One or more fields are invalid"
            };
            foreach (var group in ex.Errors.GroupBy(error => error.PropertyName ?? string.Empty))
            {
                response.ModelStateErrors.Add(new Item(group.Key, string.Join(" ", group.Select(error => error.ErrorMessage))));
            }
            return BadRequest(response);
        }
        catch (NotFoundException ex)
        {
            return NotFound(new ApiResponse { Success = false, Message = ex.Message });
        }
        catch (BusinessException ex)
        {
            return BadRequest(new ApiResponse { Success = false, Message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error transferring family {FamilyId} to charity {NewCharityId}", id, dto.NewCharityId);
            return StatusCode(500, new ApiResponse { Success = false, Message = "Error transferring family" });
        }
    }

    #endregion

    #region Member Control — move an orphan between families (UC-FAM-07)

    /// <summary>
    /// Move an orphan between families (UC-FAM-07 نقل يتيم بين الأسر). HQ-only. Action 0 detaches
    /// the orphan to a new holding family under the same charity (justification recorded on the
    /// orphan); action 1 attaches it to the family whose register code is given. Atomic: counters
    /// on both families and the orphan move commit together or not at all.
    /// </summary>
    [HttpPost("{familyId}/members/{memberId}/control")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse>> ControlFamilyMember(Guid familyId, Guid memberId, [FromBody] MemberControlDto dto)
    {
        try
        {
            var userCharityId = GetUserCharityId();
            var userRole = GetUserRole();

            await _familyService.ControlFamilyMemberAsync(familyId, memberId, dto, userCharityId, userRole, User.Identity?.Name);

            return Ok(new ApiResponse { Success = true, Message = "Member moved successfully" });
        }
        catch (FluentValidation.ValidationException ex)
        {
            var response = new ApiResponse
            {
                Success = false,
                Message = "One or more fields are invalid"
            };
            foreach (var group in ex.Errors.GroupBy(error => error.PropertyName ?? string.Empty))
            {
                response.ModelStateErrors.Add(new Item(group.Key, string.Join(" ", group.Select(error => error.ErrorMessage))));
            }
            return BadRequest(response);
        }
        catch (NotFoundException ex)
        {
            return NotFound(new ApiResponse { Success = false, Message = ex.Message });
        }
        catch (BusinessException ex)
        {
            return BadRequest(new ApiResponse { Success = false, Message = ex.Message });
        }
        catch (UnauthorizedAccessException)
        {
            // Service-side tenancy guard (UpdateProviderAsync pattern) — never a 500.
            return Forbid();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error moving member {MemberId} of family {FamilyId} (action {Action})", memberId, familyId, dto.Action);
            return StatusCode(500, new ApiResponse { Success = false, Message = "Error moving family member" });
        }
    }

    /// <summary>
    /// UC-FAM-13 حذف كفالة العائل — removes the family's guardian sponsorship link
    /// (soft delete of the provider row). Refused while any orphan of the family still
    /// has an active sponsorship. The التعليق travels as a query parameter (DELETE bodies
    /// are unreliable across clients) and is recorded on the provider's notes. HQ roles only.
    /// </summary>
    [HttpDelete("{familyId}/provider/sponsor")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse>> RemoveProviderSponsorLink(Guid familyId, [FromQuery] string? comment)
    {
        try
        {
            var userCharityId = GetUserCharityId();
            var userRole = GetUserRole();

            await _familyService.RemoveProviderSponsorLinkAsync(
                familyId,
                new RemoveProviderSponsorLinkDto { Comment = comment },
                userCharityId,
                userRole,
                User.Identity?.Name);

            return Ok(new ApiResponse { Success = true, Message = "Guardian sponsorship link removed" });
        }
        catch (FluentValidation.ValidationException ex)
        {
            var response = new ApiResponse
            {
                Success = false,
                Message = "One or more fields are invalid"
            };
            foreach (var group in ex.Errors.GroupBy(error => error.PropertyName ?? string.Empty))
            {
                response.ModelStateErrors.Add(new Item(group.Key, string.Join(" ", group.Select(error => error.ErrorMessage))));
            }
            return BadRequest(response);
        }
        catch (NotFoundException ex)
        {
            return NotFound(new ApiResponse { Success = false, Message = ex.Message });
        }
        catch (BusinessException ex)
        {
            return BadRequest(new ApiResponse { Success = false, Message = ex.Message });
        }
        catch (UnauthorizedAccessException)
        {
            // Service-side tenancy guard (UpdateProviderAsync pattern) — never a 500.
            return Forbid();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing guardian sponsorship link of family {FamilyId}", familyId);
            return StatusCode(500, new ApiResponse { Success = false, Message = "Error removing guardian sponsorship link" });
        }
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// The charity the caller belongs to, from the access token.
    /// </summary>
    /// <remarks>
    /// This read the literal string "CharityId" for a claim that no token carried, so every
    /// tenancy branch in this controller was dormant and the register was effectively unscoped.
    /// Story 3-1 added a `charityId` claim, and because .NET compares claim types
    /// case-insensitively, this method began returning a value — activating ~17 call sites at once
    /// as a side effect rather than a decision.
    ///
    /// Now bound to <see cref="IiroSaClaimTypes.CharityId"/> so the coupling is explicit and a
    /// rename cannot silently switch this controller's scoping on or off again.
    ///
    /// NOTE: this scopes families by <c>ApplicationUser.CharityId</c>, while family rows carry
    /// <c>FK_CharityId</c>. The two are populated by separate mechanisms; if they disagree for an
    /// existing tenant, that tenant's family register will read as empty. Verify they agree before
    /// relying on this in production.
    /// </remarks>
    private Guid? GetUserCharityId()
    {
        var charityIdClaim = User.FindFirst(IiroSaClaimTypes.CharityId)?.Value;
        if (Guid.TryParse(charityIdClaim, out var charityId))
        {
            return charityId;
        }
        return null;
    }

    private string? GetUserRole()
    {
        // D4: tenancy keys on the Charity role wherever it appears — not on which role claim
        // happens to be listed first. A multi-role user holding Charity is charity-scoped.
        if (User.IsInRole("Charity"))
        {
            return "Charity";
        }
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
