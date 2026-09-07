using FluentValidation;
using Microsoft.EntityFrameworkCore;

using IIROSA.Application.DTOs.Family;
using IIROSA.Application.DTOs.Reports;
using IIROSA.Application.Exceptions;
using IIROSA.Application.Interfaces;
using IIROSA.Domain.Entities;
using IIROSA.Domain.Interfaces;

namespace IIROSA.Application.Services;

/// <summary>
/// UC-FAM-14 (طباعة كشوف المتابعة) — sheet payloads for the three report keys. Read-only:
/// no UoW save, tracking-no-track queries, one business rule per sheet (an empty selection
/// refuses rather than producing an empty document).
/// </summary>
public class ReportSheetService : IReportSheetService
{
    /// <summary>
    /// Row ceiling for every sheet. An identification sheet over a whole HQ register is a PII
    /// dump that can freeze the print tab — bounded, and the payload's Truncated flag tells the
    /// client to warn rather than silently print an incomplete document.
    /// </summary>
    private const int SheetRowCap = 5000;

    private readonly IFamilyService _familyService;
    private readonly IFamilyRepository _familyRepository;
    private readonly IProviderRepository _providerRepository;
    private readonly IMotherRepository _motherRepository;
    private readonly ICharityRepository _charityRepository;
    private readonly IValidator<ReportSheetRequestDto> _requestValidator;
    private readonly IIROSA.Application.Interfaces.ICurrentUserService _currentUser;

    public ReportSheetService(
        IFamilyService familyService,
        IFamilyRepository familyRepository,
        IProviderRepository providerRepository,
        IMotherRepository motherRepository,
        ICharityRepository charityRepository,
        IValidator<ReportSheetRequestDto> requestValidator,
        IIROSA.Application.Interfaces.ICurrentUserService currentUser)
    {
        _familyService = familyService;
        _familyRepository = familyRepository;
        _providerRepository = providerRepository;
        _motherRepository = motherRepository;
        _charityRepository = charityRepository;
        _requestValidator = requestValidator;
        _currentUser = currentUser;
    }

    public async Task<ReportSheetPayloadDto<FamilyFollowUpListDto>> BuildUpdateTrackingSheetAsync(
        ReportSheetRequestDto dto, Guid? userCharityId, string? userRole)
    {
        await _requestValidator.ValidateAndThrowAsync(dto);

        // The tracking sheet IS the 5-11 follow-up report printed — reuse the projection
        // verbatim (scope + day filter + kind precedence live in ONE place). Whole selection:
        // the internal print ceiling (the public report clamps at 100).
        var (rows, totalCount) = await _familyService.GetFollowUpActivityAsync(
            new FamilyFollowUpFilterDto
            {
                Date = dto.Date!.Value,
                CharityId = dto.CharityId,
                PageNumber = 1,
                PageSize = SheetRowCap
            },
            userCharityId,
            userRole,
            maxPageSize: SheetRowCap);

        var rowList = rows.ToList();
        if (rowList.Count == 0)
        {
            throw new BusinessException("There is no follow-up activity to print for the selected date");
        }

        return new ReportSheetPayloadDto<FamilyFollowUpListDto>
        {
            VariantKey = dto.ReportKey,
            Title = "Family update tracking sheet",
            CharityName = await ResolveCharityNameAsync(dto, userCharityId, userRole),
            GeneratedOn = DateTime.UtcNow,
            Rows = rowList,
            Truncated = totalCount > rowList.Count
        };
    }

    public async Task<ReportSheetPayloadDto<IdentificationSheetRowDto>> BuildGuardianIdentificationSheetAsync(
        ReportSheetRequestDto dto, Guid? userCharityId, string? userRole)
    {
        await _requestValidator.ValidateAndThrowAsync(dto);

        var families = ScopeFamilies(dto, userCharityId, userRole);
        var rows = await (
            from f in families
            join p in _providerRepository.TableNoTracking.Where(p => !p.IsDeleted && p.FamilyId != null)
                on f.Id equals p.FamilyId!.Value
            select new IdentificationSheetRowDto
            {
                FamilyCode = f.Code,
                FullName = p.FullName,
                NationalId = p.NationalId,
                Relationship = p.RelationshipToFamily,
                Phone = p.Phone,
                CharityName = f.Charity != null ? f.Charity.Name : null
            })
            .OrderBy(r => r.FamilyCode)
            .ThenBy(r => r.FullName)
            // Cap + 1: one over the ceiling tells us the sheet is truncated without a second
            // count query — an audit-facing document must never print silently incomplete.
            .Take(SheetRowCap + 1)
            .ToListAsync();

        if (rows.Count == 0)
        {
            throw new BusinessException("There are no guardians to print for the selected scope");
        }

        var truncated = rows.Count > SheetRowCap;
        if (truncated)
        {
            rows = rows.Take(SheetRowCap).ToList();
        }

        return new ReportSheetPayloadDto<IdentificationSheetRowDto>
        {
            VariantKey = dto.ReportKey,
            Title = "Guardian identification sheets",
            CharityName = await ResolveCharityNameAsync(dto, userCharityId, userRole),
            GeneratedOn = DateTime.UtcNow,
            Rows = rows,
            Truncated = truncated
        };
    }

    public async Task<ReportSheetPayloadDto<IdentificationSheetRowDto>> BuildWidowIdentificationSheetAsync(
        ReportSheetRequestDto dto, Guid? userCharityId, string? userRole)
    {
        await _requestValidator.ValidateAndThrowAsync(dto);

        // Widow mapping (recorded interpretation, story Dev Notes): the platform has no
        // IsWidow flag — the legacy widow sheet listed mothers heading/guarding their family.
        // That is exactly the UC-4.7 guardian designation on the mother (Mother.IsProvider),
        // set by the verify-parent flow. Keep the SQL simple; a richer widow definition
        // belongs to epic 10/18 if ever needed.
        var families = ScopeFamilies(dto, userCharityId, userRole);
        var rows = await (
            from f in families
            join m in _motherRepository.TableNoTracking.Where(m => !m.IsDeleted && m.IsProvider == true && m.FamilyId != null)
                on f.Id equals m.FamilyId!.Value
            select new IdentificationSheetRowDto
            {
                FamilyCode = f.Code,
                FullName = m.FullName,
                NationalId = m.NationalId,
                Relationship = "Mother",
                Phone = m.Phone,
                CharityName = f.Charity != null ? f.Charity.Name : null
            })
            .OrderBy(r => r.FamilyCode)
            .ThenBy(r => r.FullName)
            // Cap + 1 — same truncation probe as the guardian sheet.
            .Take(SheetRowCap + 1)
            .ToListAsync();

        if (rows.Count == 0)
        {
            throw new BusinessException("There are no widows to print for the selected scope");
        }

        var truncated = rows.Count > SheetRowCap;
        if (truncated)
        {
            rows = rows.Take(SheetRowCap).ToList();
        }

        return new ReportSheetPayloadDto<IdentificationSheetRowDto>
        {
            VariantKey = dto.ReportKey,
            Title = "Widow identification sheets",
            CharityName = await ResolveCharityNameAsync(dto, userCharityId, userRole),
            GeneratedOn = DateTime.UtcNow,
            Rows = rows,
            Truncated = truncated
        };
    }

    /// <summary>
    /// Review P17 2026-08-26: the report-key → builder dispatch (was a switch in the
    /// controller) — the route's key vocabulary lives here, beside the builders it selects.
    /// </summary>
    /// <exception cref="BusinessException">Unknown report key.</exception>
    public async Task<object> BuildSheetAsync(ReportSheetRequestDto dto, Guid? userCharityId, string? userRole) =>
        dto.ReportKey switch
        {
            "family-update-tracking" => await BuildUpdateTrackingSheetAsync(dto, userCharityId, userRole),
            "guardian-identification-sheets" => await BuildGuardianIdentificationSheetAsync(dto, userCharityId, userRole),
            "widow-identification-sheets" => await BuildWidowIdentificationSheetAsync(dto, userCharityId, userRole),
            _ => throw new BusinessException("Unknown report key")
        };

    /// <summary>
    /// The shared family scope: not deleted; a CharityId-claim caller is pinned to its own
    /// register; HQ may narrow through the filter; an optional family code restricts the sheet
    /// to that one family. Scoped on FK_CharityId — the LIVE tenancy column; OR-ing the
    /// CharityId mirror would leak rows the charity no longer owns after a transfer.
    /// Review P3 2026-08-26: scope is derived from the caller's CLAIMS (the platform's
    /// pin-never-widen ladder), not from the controller-supplied role/charity strings — the
    /// previous role-string compare treated any non-Charity role as head office and adopted
    /// the payload's charity (or ran unscoped) on its word. The legacy parameters stay on the
    /// public signature (interface stability) but no longer scope.
    /// </summary>
    private IQueryable<Family> ScopeFamilies(ReportSheetRequestDto dto, Guid? userCharityId, string? userRole)
    {
        var query = _familyRepository.TableNoTracking.Where(f => !f.IsDeleted);

        if (_currentUser.CharityId.HasValue)
        {
            var pinned = _currentUser.CharityId.Value;
            query = query.Where(f => f.FK_CharityId == pinned);
        }
        else if (dto.CharityId.HasValue)
        {
            var narrowed = dto.CharityId.Value;
            query = query.Where(f => f.FK_CharityId == narrowed);
        }
        else if (!_currentUser.IsHeadOffice)
        {
            throw new UnauthorizedAccessException(
                "Caller has no charity or head-office scope; refusing unscoped query.");
        }

        if (!string.IsNullOrWhiteSpace(dto.FamilyCode))
        {
            var code = dto.FamilyCode.Trim();
            query = query.Where(f => f.Code.ToLower() == code.ToLower());
        }

        return query;
    }

    private async Task<string?> ResolveCharityNameAsync(ReportSheetRequestDto dto, Guid? userCharityId, string? userRole)
    {
        Guid? charityId = _currentUser.CharityId ?? dto.CharityId;

        if (!charityId.HasValue)
        {
            return null;
        }

        // Review P15 2026-08-26: no-tracking + !IsDeleted like every sibling charity lookup —
        // a soft-deleted charity must not name itself on an identification sheet.
        var name = await _charityRepository.TableNoTracking
            .Where(c => !c.IsDeleted && c.Id == charityId.Value)
            .Select(c => c.Name)
            .FirstOrDefaultAsync();
        return name;
    }
}
