using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using IIROSA.Application.DTOs.Reports;
using IIROSA.Application.Interfaces;
using IIROSA.Application.Exceptions;
using Framework.Core;

namespace IIROSA.Api.Controllers;

/// <summary>
/// Print-sheet payloads for the report keys the legacy contract served as
/// <c>POST /api/Reports/&lt;report-key&gt;/export/pdf</c> (UC-FAM-14 طباعة كشوف المتابعة;
/// epic 18 extends this controller with more keys).
///
/// Recorded deviation (client-side print ruling, epics 9/10/18): the route keeps its legacy
/// <c>/export/pdf</c> shape verbatim, but the response is the sheet's JSON payload — the SPA
/// renders the document and the browser's print-to-PDF produces the file. No server PDF
/// pipeline exists on this stack; story 18-21 owns the jsPDF/Arabic-font decision.
/// </summary>
public class ReportsController : ApiController
{
    private readonly IReportSheetService _reportSheetService;
    private readonly IReportService _reportService;
    private readonly IPeriodicOrphanReportService _periodicReportService;

    // Review P18 2026-08-24: the catch-alls swallowed exceptions with no trace — the
    // base class keeps its own logger private, so this controller carries a local one.
    private readonly ILogger<ReportsController> _logger;

    public ReportsController(
        IReportSheetService reportSheetService,
        IReportService reportService,
        IPeriodicOrphanReportService periodicReportService,
        ILogger<ReportsController> logger) : base(logger)
    {
        _reportSheetService = reportSheetService;
        _reportService = reportService;
        _periodicReportService = periodicReportService;
        _logger = logger;
    }

    /// <summary>
    /// UC-RPT-01 (§23.S.3 بيانات الأيتام) — read-only, caller-scoped orphan master listing.
    /// Raw envelope per the 15-1 ruling (NOT ApiResponse — each endpoint keeps its own story's
    /// contract); 400 carries <c>{ message, errors }</c> with PascalCase DTO keys → message list.
    /// The charity scope is resolved inside the service from the token, never the payload.
    /// </summary>
    [HttpPost("orphans")]
    [Authorize(Roles = "SuperAdmin,Admin,Charity")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetOrphanData([FromBody] OrphanDataFilterDto filter)
    {
        try
        {
            var result = await _reportService.GetOrphanDataAsync(filter);
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
        catch (UnauthorizedAccessException)
        {
            // Review P2 2026-08-26: the scope ladders' fail-closed refusal is an authorisation
            // outcome — 403, not the blanket 500.
            return Forbid();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error running the orphan data report");
            return StatusCode(500, new { message = "Error running the orphan data report" });
        }
    }

    /// <summary>
    /// UC-RPT-03 (§23.S.4 الايتام المستبعدين) — the excluded set with the 6-column grid
    /// contract. Read-only; one narrow (charity, HQ only — resolved inside the service from the
    /// token). The exclusion WRITE path belongs to EP-08; nothing is deleted or un-excluded here.
    /// </summary>
    [HttpPost("excluded-orphans")]
    [Authorize(Roles = "SuperAdmin,Admin,Charity")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetExcludedOrphans([FromBody] ExcludedOrphansFilterDto filter)
    {
        try
        {
            var result = await _reportService.GetExcludedOrphansAsync(filter);
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
        catch (UnauthorizedAccessException)
        {
            // Review P2 2026-08-26: the scope ladders' fail-closed refusal is an authorisation
            // outcome — 403, not the blanket 500.
            return Forbid();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error running the excluded-orphans report");
            return StatusCode(500, new { message = "Error running the excluded-orphans report" });
        }
    }

    /// <summary>
    /// UC-RPT-04 (§23.S.5 أيتام انتهت كفالتهم) — HQ-only (Gen. Director). The domain carries no
    /// ended state today; per the recorded product ruling the set is empty with a logged gap —
    /// the screen/endpoint/export contract ships now, the predicate lands with the state.
    /// </summary>
    [HttpPost("finished-sponsorship-orphans")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetFinishedSponsorshipOrphans([FromBody] OrphanStatusReportFilterDto filter)
    {
        try
        {
            var result = await _reportService.GetFinishedSponsorshipOrphansAsync(filter);
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
        catch (UnauthorizedAccessException)
        {
            // Review P2 2026-08-26: the scope ladders' fail-closed refusal is an authorisation
            // outcome — 403, not the blanket 500.
            return Forbid();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error running the finished-sponsorship report");
            return StatusCode(500, new { message = "Error running the finished-sponsorship report" });
        }
    }

    /// <summary>
    /// UC-RPT-05 (§23.S.6 أيتام غير مكفولين) — HQ-only (Gen. Director). Coded orphans whose
    /// status is Unsponsored (an empty Code is UNCODED, not unsponsored — EP-08 ruling).
    /// Read-only; charity scope resolves inside the service from the token.
    /// </summary>
    [HttpPost("unsponsored-orphans")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetUnsponsoredOrphans([FromBody] OrphanStatusReportFilterDto filter)
    {
        try
        {
            var result = await _reportService.GetUnsponsoredOrphansAsync(filter);
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
        catch (UnauthorizedAccessException)
        {
            // Review P2 2026-08-26: the scope ladders' fail-closed refusal is an authorisation
            // outcome — 403, not the blanket 500.
            return Forbid();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error running the unsponsored-orphans report");
            return StatusCode(500, new { message = "Error running the unsponsored-orphans report" });
        }
    }

    /// <summary>
    /// UC-RPT-06 (§23.S.9 ارامل مطلوب لهم كفاله) — the 22-column widow grid over the family's
    /// mother joined to the family's residence/income columns. Read-only, query-only (the spec
    /// lists no استخراج command). No widow-sponsorship flag exists — the predicate is the data's
    /// floor: mother alive, registered family, husband death date present, orphans present.
    /// </summary>
    [HttpPost("widows-allowing-sponsorship")]
    [Authorize(Roles = "SuperAdmin,Admin,Charity")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetWidowsAllowingSponsorship([FromBody] WidowSponsorshipFilterDto filter)
    {
        try
        {
            var result = await _reportService.GetWidowsAllowingSponsorshipAsync(filter);
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
        catch (UnauthorizedAccessException)
        {
            // Review P2 2026-08-26: the scope ladders' fail-closed refusal is an authorisation
            // outcome — 403, not the blanket 500.
            return Forbid();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error running the widows report");
            return StatusCode(500, new { message = "Error running the widows report" });
        }
    }

    /// <summary>
    /// UC-RPT-07 (§23.S.8 تقرير الكروت المسجله) + UC-RPT-08 (§23.U.8 استخراج كروت العائل) —
    /// HQ-only. ONE action, ONE widened DTO serving both variants: the paged read (18-7) when
    /// ReportNo is absent; the full-selection extract (18-8) when ReportNo is present. The
    /// domain has no card column yet — both variants are empty with the gap logged until the
    /// registration vertical lands the fields.
    /// </summary>
    [HttpPost("meza-cards")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetMezaCards([FromBody] MezaCardsFilterDto filter)
    {
        try
        {
            // ReportNo's presence is the §23.U.8 extract discriminator (single endpoint ruling).
            // Review P17 2026-08-24: ReportNo = 0 is the client's "unset" default — treating
            // 0 as present routed the paged read into the extract validator and surfaced a
            // 400 instead of the grid. Only a positive number selects the extract variant.
            var result = filter.ReportNo is > 0
                ? await _reportService.GetMezaCardsForExportAsync(filter)
                : await _reportService.GetMezaCardsAsync(filter);
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
        catch (UnauthorizedAccessException)
        {
            // Review P2 2026-08-26: the scope ladders' fail-closed refusal is an authorisation
            // outcome — 403, not the blanket 500.
            return Forbid();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error running the meza-cards report");
            return StatusCode(500, new { message = "Error running the meza-cards report" });
        }
    }

    /// <summary>
    /// UC-RPT-09 (§23.S.13 بيانات أسر المساعدات) — the distinct families benefiting from
    /// seasonal-aid assistance, one row per family with the campaigns aggregated. The legacy
    /// screen was a bare extract with no grid; the platform adaptation renders a grid so
    /// استخراج has rows to extract (recorded). Read-only; scope resolves inside the service.
    /// </summary>
    [HttpPost("beneficiary-family-details")]
    [Authorize(Roles = "SuperAdmin,Admin,Charity")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetBeneficiaryFamilyDetails([FromBody] BeneficiaryFamilyFilterDto filter)
    {
        try
        {
            var result = await _reportService.GetBeneficiaryFamiliesAsync(filter);
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
        catch (UnauthorizedAccessException)
        {
            // Review P2 2026-08-26: the scope ladders' fail-closed refusal is an authorisation
            // outcome — 403, not the blanket 500.
            return Forbid();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error running the beneficiary-families report");
            return StatusCode(500, new { message = "Error running the beneficiary-families report" });
        }
    }

    /// <summary>
    /// UC-RPT-11 (§23.S.7 مشاريع الأسر) — HQ-only (Gen. Director). US-RPT-11 is typed "Create a
    /// record" — a template artefact; this is a read. The row source (a project registered FOR a
    /// family) does not exist in the domain yet — no HousingProject class, no FamilyId on
    /// OfficeProject — so per the standing ruling the set is empty with a logged gap until a
    /// family-project vertical lands the linkage. The screen/endpoint/export contract ships now.
    /// </summary>
    [HttpPost("registered-family-projects")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetRegisteredFamilyProjects([FromBody] FamilyProjectsFilterDto filter)
    {
        try
        {
            var result = await _reportService.GetRegisteredFamilyProjectsAsync(filter);
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
        catch (UnauthorizedAccessException)
        {
            // Review P2 2026-08-26: the scope ladders' fail-closed refusal is an authorisation
            // outcome — 403, not the blanket 500.
            return Forbid();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error running the family-projects report");
            return StatusCode(500, new { message = "Error running the family-projects report" });
        }
    }

    /// <summary>
    /// UC-RPT-12 (§23.S.19 تقارير تعديل المعيل) — guardian change history: one row per
    /// provider assignment with the current guardian's fields, audit stamps as تاريخ التعديل
    /// and the family/charity/orphan context. Read-only; charity scope resolves server-side
    /// from the token, never the payload.
    /// </summary>
    [HttpPost("provider-sponsor-changes")]
    [Authorize(Roles = "SuperAdmin,Admin,Charity")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetProviderSponsorChanges([FromBody] ProviderChangeFilterDto filter)
    {
        try
        {
            var result = await _reportService.GetProviderSponsorChangesAsync(filter);
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
        catch (UnauthorizedAccessException)
        {
            // Review P2 2026-08-26: the scope ladders' fail-closed refusal is an authorisation
            // outcome — 403, not the blanket 500.
            return Forbid();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error running the guardian-changes report");
            return StatusCode(500, new { message = "Error running the guardian-changes report" });
        }
    }

    /// <summary>
    /// UC-RPT-15 (§23.S.14 أيتام مكودون مطلوب لهم تقرير) — the summary grid: per-charity
    /// count of coded orphans with no accepted report covering the trailing 12 months
    /// (BR-11 semantics, aligned with the UC-ORR-14 chase list). Read-only; charity scope
    /// resolves server-side from the token, never the payload.
    /// </summary>
    [HttpPost("orphans-missing-reports")]
    [Authorize(Roles = "SuperAdmin,Admin,Charity")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetOrphansMissingReports([FromBody] OrphansMissingReportsFilterDto filter)
    {
        try
        {
            var result = await _reportService.GetOrphansMissingReportsAsync(filter);
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
        catch (UnauthorizedAccessException)
        {
            // Review P2 2026-08-26: the scope ladders' fail-closed refusal is an authorisation
            // outcome — 403, not the blanket 500.
            return Forbid();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error running the orphans-missing-reports report");
            return StatusCode(500, new { message = "Error running the orphans-missing-reports report" });
        }
    }

    /// <summary>
    /// UC-RPT-15 drill-down (ExtractDetails) — one charity's chase list. A charity caller
    /// naming another charity is clamped to its own rows (pin-never-widen). Read-only.
    /// </summary>
    [HttpPost("orphans-missing-reports/details")]
    [Authorize(Roles = "SuperAdmin,Admin,Charity")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetOrphansMissingReportsDetail([FromBody] OrphansMissingReportsDetailRequestDto request)
    {
        try
        {
            // Review P17 2026-08-26: page bounds and the empty-charityId refusal are
            // validator-owned in the service now — no controller-side Math.Max clamp.
            var result = await _reportService.GetOrphansMissingReportsDetailAsync(request);
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
        catch (UnauthorizedAccessException)
        {
            // Review P2 2026-08-26: the scope ladders' fail-closed refusal is an authorisation
            // outcome — 403, not the blanket 500.
            return Forbid();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error running the orphans-missing-reports detail report");
            return StatusCode(500, new { message = "Error running the orphans-missing-reports detail report" });
        }
    }

    /// <summary>
    /// UC-RPT-16 (§23.S.15 أيتام مطلوب لهم ملفات) — a charity's missing-files worklist: coded
    /// orphans whose latest periodic report is flagged MissingDocuments. Read-only; charity
    /// scope resolves server-side from the token, never the payload.
    /// </summary>
    [HttpPost("orphans-missing-files")]
    [Authorize(Roles = "SuperAdmin,Admin,Charity")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetOrphansMissingFiles([FromBody] OrphansMissingFilesFilterDto filter)
    {
        try
        {
            var result = await _reportService.GetOrphansMissingFilesAsync(filter);
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
        catch (UnauthorizedAccessException)
        {
            // Review P2 2026-08-26: the scope ladders' fail-closed refusal is an authorisation
            // outcome — 403, not the blanket 500.
            return Forbid();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error running the orphans-missing-files report");
            return StatusCode(500, new { message = "Error running the orphans-missing-files report" });
        }
    }

    /// <summary>
    /// UC-RPT-17 (§23.S.16 تقارير في انتظار الموافقة) — the review queue: submitted periodic
    /// reports that are neither accepted nor refused, oldest submission first. Read-only; the
    /// EditOrpReport jump happens client-side to /periodic-orphan-reports/{id}/review.
    /// </summary>
    [HttpPost("reports-awaiting-approval")]
    [Authorize(Roles = "SuperAdmin,Admin,Charity")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetReportsAwaitingApproval([FromBody] ReportsAwaitingApprovalFilterDto filter)
    {
        try
        {
            var result = await _reportService.GetReportsAwaitingApprovalAsync(filter);
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
        catch (UnauthorizedAccessException)
        {
            // Review P2 2026-08-26: the scope ladders' fail-closed refusal is an authorisation
            // outcome — 403, not the blanket 500.
            return Forbid();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error running the reports-awaiting-approval report");
            return StatusCode(500, new { message = "Error running the reports-awaiting-approval report" });
        }
    }

    /// <summary>
    /// UC-RPT-18 (§23.S.17 تقارير تم رفضها) — the refused worklist: refused periodic reports
    /// with the reason resolved (catalogue label preferred, free-text fallback), newest refusal
    /// first. Read-only; the decision write path stays in the periodic-reports module — the
    /// EditOrpReport jump opens /periodic-orphan-reports/{id}/review, which authorises and
    /// records the decision itself.
    /// </summary>
    [HttpPost("refused-reports")]
    [Authorize(Roles = "SuperAdmin,Admin,Charity")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetRefusedReports([FromBody] RefusedReportsFilterDto filter)
    {
        try
        {
            var result = await _reportService.GetRefusedReportsAsync(filter);
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
        catch (UnauthorizedAccessException)
        {
            // Review P2 2026-08-26: the scope ladders' fail-closed refusal is an authorisation
            // outcome — 403, not the blanket 500.
            return Forbid();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error running the refused-reports report");
            return StatusCode(500, new { message = "Error running the refused-reports report" });
        }
    }

    /// <summary>
    /// UC-ORR-14 / UC-RPT-19 (الأيتام بدون تقرير مجدد) — the chase list before a payment
    /// run, in two modes: a <c>batchId</c> (or no window at all — the current batch) chases
    /// that batch's orphans without a report renewed for the trailing-12-months cycle
    /// (18-19); a window with no batch chases the (scoped) charity's coded orphans with no
    /// accepted report covering it (§14.U.14). <paramref name="request"/>.CountOnly
    /// collapses the legacy _Number variant. Read-only; charity scope resolves server-side
    /// from the token, never the payload.
    /// </summary>
    [HttpPost("non-renewed-reports")]
    [Authorize(Roles = "SuperAdmin,Admin,Accountant,Employee,Charity")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetNonRenewedReports([FromBody] NonRenewedReportsRequestDto request)
    {
        try
        {
            var result = await _reportService.GetNonRenewedReportAsync(request);
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
        catch (UnauthorizedAccessException)
        {
            // Review P2 2026-08-26: the scope ladders' fail-closed refusal is an authorisation
            // outcome — 403, not the blanket 500.
            return Forbid();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error running the non-renewed report");
            return StatusCode(500, new { message = "Error running the non-renewed report" });
        }
    }

    /// <summary>
    /// UC-RPT-20 (§23.S.12 متابعة الجمعيات) — the cross-charity tracking grid over one
    /// payment batch: one row per charity present in the batch (members, entered reports,
    /// upload state). HQ-only — the endpoint authorises; the menu gate is convenience.
    /// Read-only; the pins (charity claim, country claim) hold defensively server-side.
    /// </summary>
    [HttpPost("charity-payment-tracking")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetCharityPaymentTracking([FromBody] CharityPaymentTrackingFilterDto filter)
    {
        try
        {
            var result = await _reportService.GetCharityPaymentTrackingAsync(filter);
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
        catch (UnauthorizedAccessException)
        {
            // Review P2 2026-08-26: the scope ladders' fail-closed refusal is an authorisation
            // outcome — 403, not the blanket 500.
            return Forbid();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error running the charity-payment-tracking report");
            return StatusCode(500, new { message = "Error running the charity-payment-tracking report" });
        }
    }

    /// <summary>
    /// UC-RPT-21 (§23.S.12 متابعة تحديث بيانات الأسر) — the family-update monitoring sheet's
    /// data: families of the anchor payment's charities refreshed in
    /// [Date, the payment's period end]. The PDF itself is produced client-side (browser
    /// print — the recorded epic-wide path), so there is deliberately NO /export/pdf twin.
    /// HQ-only: the sheet is cross-charity by design; the pins still hold in the service.
    /// </summary>
    [HttpPost("family-update-tracking")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetFamilyUpdateTracking([FromBody] FamilyUpdateTrackingFilterDto filter)
    {
        try
        {
            var result = await _reportService.GetFamilyUpdateTrackingAsync(filter);
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
        catch (UnauthorizedAccessException)
        {
            // Review P2 2026-08-26: the scope ladders' fail-closed refusal is an authorisation
            // outcome — 403, not the blanket 500.
            return Forbid();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error running the family-update-tracking report");
            return StatusCode(500, new { message = "Error running the family-update-tracking report" });
        }
    }

    /// <summary>
    /// UC-RPT-22 (§23.S.11 أيتام مستحقون دفعات سابقة) — the arrears grid: scoped orphans with
    /// at least one entitled-but-unreceived batch, with the per-batch receipt map the grid's
    /// dynamic batch columns render. Read-only; charity callers are pinned server-side (the
    /// payload's charityId is an HQ narrow only). سبب طلب الاستعداد renders null — no persisted
    /// arrears reason exists (real-data-only ruling).
    /// </summary>
    [HttpPost("missed-payments")]
    [Authorize(Roles = "SuperAdmin,Admin,Charity")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetMissedPayments([FromBody] MissedPaymentsReportFilterDto filter)
    {
        try
        {
            var result = await _reportService.GetMissedPaymentsAsync(filter);
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
        catch (UnauthorizedAccessException)
        {
            // UC-RPT-23: the all-orphans scope is HQ-only — a non-HQ caller sending the flag
            // (toggle UI or forged payload) is refused server-side (OrphanPaymentsController
            // refusal-ladder shape). Review P2 2026-08-26: the scope ladders' fail-closed
            // refusal shares this arm — 403, not the blanket 500.
            return Forbid();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error running the missed-payments report");
            return StatusCode(500, new { message = "Error running the missed-payments report" });
        }
    }

    /// <summary>
    /// UC-RPT-24 (§23.S.18 صور الأيتام) — the paged photograph manifest: photos attached to
    /// accepted periodic reports dated in [من تاريخ, الى تاريخ]. The legacy /export EPPlus
    /// file stream is superseded (recorded deviation): the JSON manifest is the response and
    /// the SPA builds the ExcelJS workbook client-side. Per-row downloads stream through the
    /// live attachment endpoint the manifest's downloadUrl points at (19-2 — not rebuilt).
    /// </summary>
    [HttpPost("orphan-files/export")]
    [Authorize(Roles = "SuperAdmin,Admin,Charity")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ExportOrphanFiles([FromBody] OrphanFilesExportFilterDto filter)
    {
        try
        {
            var result = await _reportService.ExportOrphanFilesAsync(filter);
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
        catch (UnauthorizedAccessException)
        {
            // Review P2 2026-08-26: the scope ladders' fail-closed refusal is an authorisation
            // outcome — 403, not the blanket 500.
            return Forbid();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error running the orphan-photographs report");
            return StatusCode(500, new { message = "Error running the orphan-photographs report" });
        }
    }

    /// <summary>
    /// UC-RPT-25 (§23.S.18 صور الشهادات) — the certificates manifest: 18-24's read with the
    /// kind selector flipped to the report's certificate image column (one screen, two grids,
    /// one filter shape). Same JSON-manifest → client-ExcelJS export path; per-row downloads
    /// stream through the live attachment endpoint (19-2).
    /// </summary>
    [HttpPost("certificate-files/export")]
    [Authorize(Roles = "SuperAdmin,Admin,Charity")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ExportCertificateFiles([FromBody] OrphanFilesExportFilterDto filter)
    {
        try
        {
            var result = await _reportService.ExportCertificateFilesAsync(filter);
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
        catch (UnauthorizedAccessException)
        {
            // Review P2 2026-08-26: the scope ladders' fail-closed refusal is an authorisation
            // outcome — 403, not the blanket 500.
            return Forbid();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error running the certificate-images report");
            return StatusCode(500, new { message = "Error running the certificate-images report" });
        }
    }

    /// <summary>
    /// §23.U.28 أيتام لم يصرف لهم (UC-RPT-28) — the zero-disbursement gaps of one payment
    /// batch. HQ report (the §23.S.3 command family); the service's charity pin remains
    /// defence-in-depth under the HQ role gate.
    /// </summary>
    [HttpPost("orphans-without-payment")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetOrphansWithoutPayment([FromBody] OrphansWithoutPaymentFilterDto filter)
    {
        try
        {
            var result = await _reportService.GetOrphansWithoutPaymentAsync(filter);
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
        catch (UnauthorizedAccessException)
        {
            // Review P2 2026-08-26: the scope ladders' fail-closed refusal is an authorisation
            // outcome — 403, not the blanket 500.
            return Forbid();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error running the orphans-without-payment report");
            return StatusCode(500, new { message = "Error running the orphans-without-payment report" });
        }
    }

    /// <summary>
    /// §23.U.29 المستلمون / غير المستلمين / الموقوفون (UC-RPT-29) — the three outcome
    /// lists of one cheque batch. One endpoint; the variant discriminator in the filter
    /// selects the list. Charity roles allowed (§23.U.29 runs from the payment screen);
    /// a charity caller is pinned server-side, HQ may narrow by charityId.
    /// </summary>
    [HttpPost("payments-received")]
    [Authorize(Roles = "SuperAdmin,Admin,Charity")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetPaymentsOutcome([FromBody] PaymentsOutcomeFilterDto filter)
    {
        try
        {
            var result = await _reportService.GetPaymentsOutcomeAsync(filter);
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
        catch (UnauthorizedAccessException)
        {
            // Review P2 2026-08-26: the scope ladders' fail-closed refusal is an authorisation
            // outcome — 403, not the blanket 500.
            return Forbid();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error running the payments outcome report");
            return StatusCode(500, new { message = "Error running the payments outcome report" });
        }
    }

    /// <summary>
    /// §23.U.30 أرقام الشيكات (UC-RPT-30) — the batch's recorded cheque numbers for
    /// handover/reconciliation. Charity roles allowed; a charity caller is pinned
    /// server-side, HQ may narrow by charityId.
    /// </summary>
    [HttpPost("cheque-numbers")]
    [Authorize(Roles = "SuperAdmin,Admin,Charity")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetChequeNumbers([FromBody] ChequeNumbersFilterDto filter)
    {
        try
        {
            var result = await _reportService.GetChequeNumbersAsync(filter);
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
        catch (UnauthorizedAccessException)
        {
            // Review P2 2026-08-26: the scope ladders' fail-closed refusal is an authorisation
            // outcome — 403, not the blanket 500.
            return Forbid();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error running the cheque-numbers report");
            return StatusCode(500, new { message = "Error running the cheque-numbers report" });
        }
    }

    /// <summary>
    /// §23.U.31 كروت الاستلام (UC-RPT-31) — the batch's collection cards: one card per
    /// (guardian, orphan) payment item, signed at collection. Read-only — the printed
    /// flags stay with the payments vertical's own flow. Charity roles allowed; a charity
    /// caller is pinned server-side, HQ may narrow by charityId.
    /// </summary>
    [HttpPost("receipt-cards")]
    [Authorize(Roles = "SuperAdmin,Admin,Charity")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetReceiptCards([FromBody] ReceiptCardsFilterDto filter)
    {
        try
        {
            var result = await _reportService.GetReceiptCardsAsync(filter);
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
        catch (UnauthorizedAccessException)
        {
            // Review P2 2026-08-26: the scope ladders' fail-closed refusal is an authorisation
            // outcome — 403, not the blanket 500.
            return Forbid();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error running the receipt-cards report");
            return StatusCode(500, new { message = "Error running the receipt-cards report" });
        }
    }

    /// <summary>
    /// UC-RPT-35 (§23.U.35 الأيتام والأرامل الجدد) — the sponsorship-offer lists: the four
    /// legacy .rpt reports collapsed to one variant-keyed read. HQ-only surface; the service
    /// still runs the scope ladder (charity claim pins — defense in depth). Raw paged envelope
    /// + the canonical variant echo, never PDF bytes.
    /// </summary>
    [HttpPost("new-beneficiaries")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetNewBeneficiaries([FromBody] NewBeneficiariesFilterDto filter)
    {
        try
        {
            var result = await _reportService.GetNewBeneficiariesAsync(filter);
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
        catch (UnauthorizedAccessException)
        {
            // Review P2 2026-08-26: the scope ladders' fail-closed refusal is an authorisation
            // outcome — 403, not the blanket 500.
            return Forbid();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error running the new-beneficiaries report");
            return StatusCode(500, new { message = "Error running the new-beneficiaries report" });
        }
    }

    /// <summary>
    /// UC-RPT-36 (§23.U.36 كشوف المتابعة والتسليم) — the legacy Crystal trio (متابعة / متابعة
    /// الأسر / تسليم) as one variant-keyed GET. Read-only; the grid renders the paged envelope,
    /// the sheet prints client-side (18-21). Charity callers are pinned server-side from the
    /// token; HQ may narrow via charityId; a country claim intersects.
    /// </summary>
    [HttpGet("follow-up-sheets")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetFollowUpSheets([FromQuery] FollowUpSheetFilterDto filter)
    {
        try
        {
            var result = await _reportService.GetFollowUpSheetsAsync(filter);
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
        catch (UnauthorizedAccessException)
        {
            // Review P2 2026-08-26: the scope ladders' fail-closed refusal is an authorisation
            // outcome — 403, not the blanket 500.
            return Forbid();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error running the follow-up-sheets report");
            return StatusCode(500, new { message = "Error running the follow-up-sheets report" });
        }
    }

    /// <summary>
    /// UC-RPT-37 (§23.U.37 كشوف تعريف العائل والأرامل) — the identification sheets: every
    /// guardian / the widows / one family's union, as ONE variant-keyed POST returning the whole
    /// selection (a print document is never page 1). Read-only; the sheet is composed client-side
    /// (18-21). The §23.S.10 host screen (#/reports/family-orphans) carries the command. The
    /// producing user resolves from the token; a charity caller is pinned server-side; HQ may
    /// narrow via charityId; a country claim intersects.
    /// </summary>
    [HttpPost("guardian-identification-sheets")]
    [Authorize(Roles = "SuperAdmin,Admin,Charity")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetGuardianIdentificationSheets(
        [FromBody] GuardianIdentificationSheetFilterDto filter)
    {
        try
        {
            var result = await _reportService.GetGuardianIdentificationSheetsAsync(filter);
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
        catch (UnauthorizedAccessException)
        {
            // Review P2 2026-08-26: the scope ladders' fail-closed refusal is an authorisation
            // outcome — 403, not the blanket 500.
            return Forbid();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error producing the guardian-identification sheets");
            return StatusCode(500, new { message = "Error producing the guardian-identification sheets" });
        }
    }

    /// <summary>
    /// §23.U.38 مرفقات الصادر الناقصة — paged audit list of outgoing letters that carry zero
    /// orphan-report attachment rows (the v1 rule). HQ + SuperAdmin/Admin only: an audit of the
    /// correspondence register is a back-office concern, so Charity is not in the role set.
    /// GET with a query-bound filter — the screen is query/grid-only (no print payload).
    /// </summary>
    [HttpGet("missing-outgoing-attachments")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetMissingOutgoingAttachments(
        [FromQuery] MissingOutgoingAttachmentsFilterDto filter)
    {
        try
        {
            var result = await _reportService.GetMissingOutgoingAttachmentsAsync(filter);
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
        catch (UnauthorizedAccessException)
        {
            // Review P2 2026-08-26: the scope ladders' fail-closed refusal is an authorisation
            // outcome — 403, not the blanket 500.
            return Forbid();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error listing the missing outgoing attachments");
            return StatusCode(500, new { message = "Error listing the missing outgoing attachments" });
        }
    }

    /// <summary>
    /// §23.U.39 أيتام الأسر بتاريخ (UC-RPT-39) — one charity's families as at a date, each with
    /// its orphans grouped under it (a page = families). POST: the as-at date is part of the
    /// request body, not a query string. HQ + SuperAdmin/Admin only — the same back-office
    /// boundary as the audit sheets; the document is composed client-side (18-21's PDF path,
    /// the legacy /export/pdf streaming superseded). No route collision with 18-13's
    /// GET /api/Families/{id}/follow-up (different controller family).
    /// </summary>
    [HttpPost("family-orphans")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetFamilyOrphansByDate(
        [FromBody] FamilyOrphansByDateFilterDto filter)
    {
        try
        {
            var result = await _reportService.GetFamilyOrphansByDateAsync(filter);
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
        catch (UnauthorizedAccessException)
        {
            // Review P2 2026-08-26: the scope ladders' fail-closed refusal is an authorisation
            // outcome — 403, not the blanket 500.
            return Forbid();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error producing the family-orphan list by date");
            return StatusCode(500, new { message = "Error producing the family-orphan list by date" });
        }
    }

    /// <summary>
    /// UC-ORR-17 (§14.U.17 طباعة التقرير الدوري) — the periodic report form's print payload:
    /// caller-scoped detail data + the resolved variant key (the collapsed 24-template matrix)
    /// + the attachment ids present. A dedicated literal route so its role set stays this
    /// story's (the family sheets' action keeps its own). Foreign/unknown id → 404 — there is
    /// nothing to produce; the charity pin resolves inside the service from the token.
    /// </summary>
    [HttpPost("orphan-report-form/export/pdf")]
    [Authorize(Roles = "SuperAdmin,Admin,Accountant,Employee,Charity")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ExportOrphanReportForm([FromBody] ReportSheetRequestDto dto)
    {
        try
        {
            // Review P17 2026-08-26: the empty-id refusal moved into the service (was a
            // controller null-check) — the id-required rule lives with the query now.
            var form = await _periodicReportService.GetPrintFormAsync(dto.ReportId ?? Guid.Empty);
            if (form == null)
            {
                return NotFound(new ApiResponse { Success = false, Message = "Report not found" });
            }

            return Ok(form);
        }
        catch (FluentValidation.ValidationException)
        {
            return BadRequest(new ApiResponse { Success = false, Message = "Report id is required" });
        }
        catch (UnauthorizedAccessException)
        {
            // Review P2 2026-08-26: the scope ladders' fail-closed refusal is an authorisation
            // outcome — 403, not the blanket 500.
            return Forbid();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error producing the report form");
            return StatusCode(500, new ApiResponse { Success = false, Message = "Error producing the report form" });
        }
    }

    /// <summary>
    /// One template serves every key — the key selects the data, not a controller per sheet.
    /// An empty selection refuses with 400 (nothing to produce) instead of an empty document.
    /// </summary>
    [HttpPost("{reportKey}/export/pdf")]
    [Authorize(Roles = "SuperAdmin,Admin,Charity")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ExportSheetPdf(string reportKey, [FromBody] ReportSheetRequestDto dto)
    {
        try
        {
            // The route wins over any body value for the key (thin bind step, nothing more).
            dto.ReportKey = reportKey;

            var userCharityId = GetUserCharityId();
            var userRole = GetUserRole();

            // Review P17 2026-08-26: the key → builder dispatch lives in the service now —
            // the controller only binds the route key onto the DTO (thin bind, nothing more).
            return Ok(await _reportSheetService.BuildSheetAsync(dto, userCharityId, userRole));
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
        catch (UnauthorizedAccessException)
        {
            // Review P2 2026-08-26: the scope ladders' fail-closed refusal is an authorisation
            // outcome — 403, not the blanket 500.
            return Forbid();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error producing the report sheet");
            return StatusCode(500, new ApiResponse { Success = false, Message = "Error producing the report sheet" });
        }
    }

    #region Claims Helpers

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
        // Tenancy keys on the Charity role wherever it appears — not on which role claim
        // happens to be listed first. A multi-role user holding Charity is charity-scoped.
        // Review P3 2026-08-26: scan the Role claims directly — User.IsInRole rides the
        // platform's broken role-membership store (surrogate-key ApplicationUserRoles), and
        // the previous first-claim fallback made a multi-role token's scope depend on claim
        // ordering. (The sheet builders no longer consume this for scoping — claims do —
        // it remains only as a display/legacy hint.)
        var roles = User.Claims
            .Where(c => c.Type == System.Security.Claims.ClaimTypes.Role)
            .Select(c => c.Value)
            .ToList();
        if (roles.Any(r => string.Equals(r, "Charity", StringComparison.OrdinalIgnoreCase)))
        {
            return "Charity";
        }
        return roles.FirstOrDefault();
    }

    #endregion
}
