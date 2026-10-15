using IIROSA.Domain.Enums;

namespace IIROSA.Application.DTOs.OrphanReportCycles;

/// <summary>
/// One row of the orphan-context chooser (UC-ORC-06): an In Progress cycle paired with
/// this orphan's submission state inside it. Drives both the «رفع تقرير» / «تعديل
/// التقرير» row action and the button's very existence on the orphan screens.
/// </summary>
public class OrphanCycleReportStatusDto
{
    public Guid CycleId { get; set; }

    public string TitleAr { get; set; } = string.Empty;

    public string? TitleEn { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }

    /// <summary>True when this orphan already has a live report inside the cycle.</summary>
    public bool Submitted { get; set; }

    /// <summary>The report's id when Submitted — the edit route's target.</summary>
    public Guid? ReportId { get; set; }

    public DateTime? ReportDate { get; set; }

    /// <summary>"Accepted" / "Refused" / "Pending" when Submitted; null otherwise.</summary>
    public string? ReviewState { get; set; }
}

/// <summary>
/// Minimal cycle read for the charity-facing pickers (UC-ORC-06/09): the report-entry
/// buttons' "any cycle running?" check and the coverage screen's cycle dropdown. Status
/// is derived on read (the lazy half of UC-ORC-05) — the selector never writes. Open to
/// every authenticated caller: cycles are platform-wide reference data, and the rows a
/// charity may act on stay scoped by the endpoints that serve them.
/// </summary>
public class OrphanReportCycleSelectorDto
{
    public Guid Id { get; set; }

    public string TitleAr { get; set; } = string.Empty;

    public string? TitleEn { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }

    /// <summary>0 = NotStarted, 1 = InProgress, 2 = Completed — numeric on the wire (Newtonsoft default).</summary>
    public OrphanReportCycleStatus Status { get; set; }
}

/// <summary>
/// Coverage filter (UC-ORC-09). The cycle is mandatory; the charity filter is honored
/// for head-office callers only — a charity user's scope is pinned server-side from the
/// token and never widened by a payload value.
/// </summary>
public class CycleReportCoverageFilterDto
{
    public Guid CycleId { get; set; }

    /// <summary>Head-office only — ignored (narrowing never widens) for charity callers.</summary>
    public Guid? CharityId { get; set; }

    /// <summary>Optional free text over the orphan code / name.</summary>
    public string? Search { get; set; }

    /// <summary>
    /// Which half of the split to page through: null/All for both interleaved,
    /// "Submitted" or "NotSubmitted" for one tab.
    /// </summary>
    public string? Category { get; set; }

    public int Page { get; set; } = 1;

    public int PageSize { get; set; } = 20;
}

/// <summary>One orphan row of the coverage split (UC-ORC-09).</summary>
public class CycleReportCoverageRowDto
{
    public Guid OrphanId { get; set; }

    public string OrphanCode { get; set; } = string.Empty;

    public string OrphanName { get; set; } = string.Empty;

    public string? CharityName { get; set; }

    public bool HasReport { get; set; }

    public DateTime? ReportDate { get; set; }

    /// <summary>"Accepted" / "Refused" / "Pending" when HasReport; null otherwise.</summary>
    public string? ReviewState { get; set; }
}

/// <summary>Coverage result (UC-ORC-09) — the paged rows plus the two tab counts.</summary>
public class CycleReportCoverageResultDto
{
    public Guid CycleId { get; set; }

    public string CycleTitle { get; set; } = string.Empty;

    public List<CycleReportCoverageRowDto> Items { get; set; } = new();

    public int TotalCount { get; set; }

    /// <summary>Live orphans of the scope with a report inside the cycle (the submitted tab count).</summary>
    public int SubmittedCount { get; set; }

    /// <summary>Live orphans of the scope without one (the not-submitted tab count).</summary>
    public int NotSubmittedCount { get; set; }

    public int Page { get; set; }

    public int PageSize { get; set; }
}
