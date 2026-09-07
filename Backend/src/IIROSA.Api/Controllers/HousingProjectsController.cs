using IIROSA.Application.DTOs.Family;
using IIROSA.Application.Exceptions;
using IIROSA.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IIROSA.Api.Controllers;

/// <summary>
/// Housing Project module API (chapter 11, UC-HOU-01 … UC-HOU-08).
///
/// RE-CUT (6-1/6-3, epic-11 precedent): this controller is the housing-FAMILY register API on
/// the shared families store (Family with FamilyType=Housing) — NOT the construction-project
/// tracker it was copied over as. The 16 construction actions and the HousingProject entity
/// were deleted; the register list itself lives on GET /api/Families?familyType=Housing (6-1).
///
/// Surface across the epic:
///   POST   projects                    — UC-HOU-03 register a housing family (this story)
///   GET    projects/{id}               — UC-HOU-04 view (6-4)
///   PUT    projects/{id}               — UC-HOU-04 update (6-4)
///   GET    projects/{id}/beneficiaries — UC-HOU-07 (6-7)
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "SuperAdmin,Admin,Charity")]
public class HousingProjectsController : ControllerBase
{
    private readonly IFamilyService _familyService;
    private readonly ILogger<HousingProjectsController> _logger;

    public HousingProjectsController(
        IFamilyService familyService,
        ILogger<HousingProjectsController> logger)
    {
        _familyService = familyService;
        _logger = logger;
    }

    /// <summary>
    /// UC-HOU-03 (§11.U.3): register a housing family. The Housing discriminator and the
    /// §11.S.2 mandatory flags are enforced server-side in AddNewHousingFamilyAsync; a Charity
    /// caller is pinned to its own charity (pin-never-widen, same rule as POST /api/Families).
    /// </summary>
    [HttpPost("projects")]
    [ProducesResponseType(typeof(FamilyDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<FamilyDto>> CreateHousingFamily([FromBody] CreateFamilyDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Pin charity callers to their own charity — a Charity user can never mint a
            // housing family owned by another organisation. Review 2026-08-24: a Charity
            // token with a missing/unparseable charity claim is REFUSED (fail-closed,
            // FamiliesController pattern) — previously it fell through and the client's
            // charityId was honored verbatim.
            if (User.IsInRole("Charity"))
            {
                var userCharityId = GetUserCharityId();
                if (userCharityId.HasValue)
                {
                    dto.CharityId = userCharityId.Value;
                }
                else
                {
                    return Forbid();
                }
            }

            var family = await _familyService.AddNewHousingFamilyAsync(dto);
            return StatusCode(StatusCodes.Status201Created, family);
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
        catch (BusinessException ex)
        {
            // Business refusals — duplicate guardian «أحد المعيلين مكرر من قبل أكثر من مرة»,
            // flat outside the chosen building — carry their literal §11.U.3 messages.
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating housing family");
            return StatusCode(500, new { message = "Error creating housing family", error = ex.Message });
        }
    }

    /// <summary>
    /// UC-HOU-04 (§11.U.4): the housing-family aggregate for the view/edit screen — every
    /// §11.S.2 field incl. the guardian block and full child detail. Unknown, non-housing and
    /// foreign rows all answer 404: a foreign id must not prove the record exists.
    /// </summary>
    [HttpGet("projects/{id:guid}")]
    [ProducesResponseType(typeof(HousingFamilyDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<HousingFamilyDetailDto>> GetHousingFamily(Guid id)
    {
        try
        {
            var detail = await _familyService.GetHousingFamilyAsync(id, GetUserCharityId(), CallerRole);
            return Ok(detail);
        }
        catch (NotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading housing family {FamilyId}", id);
            return StatusCode(500, new { message = "Error loading housing family", error = ex.Message });
        }
    }

    /// <summary>
    /// UC-HOU-07 (§11.U.7 البحث بالكود): the housing family's beneficiaries — children + the
    /// guardian — each carrying the BeneficiaryId + ChildOrParent that the reports read (6-6)
    /// consumes. ?code= resolves a child's sponsorship code exactly; an unknown or
    /// other-charity code answers an EMPTY list (the caller surfaces explicit not-found).
    /// Guardian rows carry no code — they are picked from the list, never resolved by code.
    /// </summary>
    [HttpGet("projects/{id:guid}/beneficiaries")]
    [ProducesResponseType(typeof(List<HousingBeneficiaryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<List<HousingBeneficiaryDto>>> GetHousingBeneficiaries(
        Guid id, [FromQuery] string? code)
    {
        try
        {
            var beneficiaries = await _familyService.GetHousingBeneficiariesAsync(
                id, code, GetUserCharityId(), CallerRole);
            return Ok(beneficiaries);
        }
        catch (NotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resolving housing beneficiaries for family {FamilyId}", id);
            return StatusCode(500, new { message = "Error resolving housing beneficiaries", error = ex.Message });
        }
    }

    /// <summary>
    /// UC-HOU-04 (§11.U.4): update a housing family under the same §11.S.2 contract as the
    /// create. Any client-sent charity field is ignored — ownership never moves — and the
    /// register discriminator is immutable. Charity callers stay pinned server-side.
    /// </summary>
    [HttpPut("projects/{id:guid}")]
    [ProducesResponseType(typeof(HousingFamilyDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<HousingFamilyDetailDto>> UpdateHousingFamily(Guid id, [FromBody] CreateFamilyDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Ownership never moves: the dto's charity field is not consulted — the service
            // scopes the row to the caller and stamps only the §11.S.2 fields. The caller's
            // name rides along for the child-removal deletion stamps (review 2026-08-24).
            var detail = await _familyService.UpdateHousingFamilyAsync(
                id, dto, GetUserCharityId(), CallerRole, User.Identity?.Name);
            return Ok(detail);
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
        catch (BusinessException ex)
        {
            // Business refusals — allocation flat ⊂ building, a payload child not belonging to
            // the family, duplicate guardian — carry their literal §11 messages.
            return BadRequest(new { message = ex.Message });
        }
        catch (NotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating housing family {FamilyId}", id);
            return StatusCode(500, new { message = "Error updating housing family", error = ex.Message });
        }
    }

    /// <summary>
    /// Charity claim of the caller (null for HQ roles) — same helper as FamiliesController.
    /// </summary>
    private Guid? GetUserCharityId()
    {
        var charityIdClaim = User.FindFirst(IiroSaClaimTypes.CharityId)?.Value;
        if (Guid.TryParse(charityIdClaim, out var charityId))
        {
            return charityId;
        }
        return null;
    }

    /// <summary>
    /// "Charity" for charity-role callers, null for HQ — the scoping role the service pairs
    /// with the charity claim (HQ roles have no pin).
    /// </summary>
    private string? CallerRole => User.IsInRole("Charity") ? "Charity" : null;
}
