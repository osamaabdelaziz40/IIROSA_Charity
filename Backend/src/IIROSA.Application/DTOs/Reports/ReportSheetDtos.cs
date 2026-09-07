using IIROSA.Application.DTOs.Family;

namespace IIROSA.Application.DTOs.Reports;

/// <summary>
/// UC-FAM-14 (§10.U.14 طباعة كشوف المتابعة) — one request shape for the three report keys
/// served by <c>POST /api/Reports/{reportKey}/export/pdf</c>. The route wins over any body
/// value for <see cref="ReportKey"/> (forced in the controller bind step).
/// </summary>
public class ReportSheetRequestDto
{
    /// <summary>Route-forced report key: family-update-tracking · guardian-identification-sheets · widow-identification-sheets.</summary>
    public string ReportKey { get; set; } = string.Empty;

    /// <summary>The activity day — required for family-update-tracking (ignored by the identification sheets).</summary>
    public DateTime? Date { get; set; }

    /// <summary>Optional charity scope; a Charity-role caller is forced to its own charity server-side.</summary>
    public Guid? CharityId { get; set; }

    /// <summary>
    /// Optional family code — restricts an identification sheet to that one family
    /// (the user-facing key; resolved to the family inside the service).
    /// </summary>
    public string? FamilyCode { get; set; }

    /// <summary>
    /// UC-ORR-17: the periodic report to print — required by the
    /// <c>orphan-report-form</c> key (ignored by the family sheets).
    /// </summary>
    public Guid? ReportId { get; set; }
}

/// <summary>
/// The printable sheet payload. The endpoint returns JSON (the platform's recorded
/// client-side-print ruling): the SPA renders the document — the browser's print-to-PDF
/// produces the file. Recorded deviation from the story's server-bytes plan: no server PDF
/// pipeline exists on this stack (both export "pipelines" are TODO stubs) and 18-21 owns the
/// jsPDF/Arabic-font decision.
/// </summary>
public class ReportSheetPayloadDto<TRow>
{
    /// <summary>The report key the sheet was built for (drives the client's variant rendering).</summary>
    public string VariantKey { get; set; } = string.Empty;

    /// <summary>Localized-agnostic sheet title (the client translates per variant).</summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>Charity the sheet is scoped to, when known — null means all charities.</summary>
    public string? CharityName { get; set; }

    /// <summary>Server generation timestamp (UTC).</summary>
    public DateTime GeneratedOn { get; set; }

    /// <summary>The whole selection — sheets are never paged (but see <see cref="Truncated"/>).</summary>
    public List<TRow> Rows { get; set; } = new();

    /// <summary>
    /// True when the selection exceeded the row ceiling and the sheet carries only the first
    /// <see cref="Rows"/> up to it — the client must warn the operator the document is partial
    /// (an audit-facing sheet must never print silently incomplete).
    /// </summary>
    public bool Truncated { get; set; }
}

/// <summary>
/// One row of an identification sheet (guardian or widow): who they are, the family they
/// belong to, and the identity columns the paper sheet carries.
/// </summary>
public class IdentificationSheetRowDto
{
    public string FamilyCode { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? NationalId { get; set; }
    public string? Relationship { get; set; }
    public string? Phone { get; set; }
    public string? CharityName { get; set; }
}
