using IIROSA.Application.DTOs.Family;
using IIROSA.Application.DTOs.Reports;

namespace IIROSA.Application.Interfaces;

/// <summary>
/// UC-FAM-14 (طباعة كشوف المتابعة) — builds the printable sheet payloads behind
/// <c>POST /api/Reports/{reportKey}/export/pdf</c>. Returns JSON payloads, not PDF bytes:
/// the platform's recorded client-side-print ruling (no server PDF pipeline exists; 18-21
/// owns the jsPDF/Arabic-font decision). Sheets carry the WHOLE selection, never a page.
/// </summary>
public interface IReportSheetService
{
    /// <summary>
    /// The family update-tracking sheet for one activity day — reuses 5-11's audit projection
    /// (<see cref="IFamilyService.GetFollowUpActivityAsync"/>) verbatim.
    /// </summary>
    /// <exception cref="Exceptions.BusinessException">No activity on the chosen day (nothing to print).</exception>
    Task<ReportSheetPayloadDto<FamilyFollowUpListDto>> BuildUpdateTrackingSheetAsync(
        ReportSheetRequestDto dto, Guid? userCharityId, string? userRole);

    /// <summary>
    /// Identification sheets for guardians — the families' provider rows, scoped by charity or
    /// a single family code.
    /// </summary>
    /// <exception cref="Exceptions.BusinessException">Empty selection (nothing to print).</exception>
    Task<ReportSheetPayloadDto<IdentificationSheetRowDto>> BuildGuardianIdentificationSheetAsync(
        ReportSheetRequestDto dto, Guid? userCharityId, string? userRole);

    /// <summary>
    /// Identification sheets for widows — the mothers of record (the UC-4.7 guardian
    /// designation <c>Mother.IsProvider</c>; the recorded widow mapping — see the story's
    /// Dev Notes), scoped by charity or a single family code.
    /// </summary>
    /// <exception cref="Exceptions.BusinessException">Empty selection (nothing to print).</exception>
    Task<ReportSheetPayloadDto<IdentificationSheetRowDto>> BuildWidowIdentificationSheetAsync(
        ReportSheetRequestDto dto, Guid? userCharityId, string? userRole);

    /// <summary>
    /// Review P17 2026-08-26: the report-key → builder dispatch, moved out of the controller
    /// (no business logic in controllers) — one place owns the route's key vocabulary.
    /// </summary>
    /// <exception cref="Exceptions.BusinessException">Unknown report key.</exception>
    Task<object> BuildSheetAsync(ReportSheetRequestDto dto, Guid? userCharityId, string? userRole);
}
