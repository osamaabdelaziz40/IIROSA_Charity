namespace IIROSA.Domain.Enums;

/// <summary>
/// Lifecycle status of an Orphan Report Cycle (UC-ORC-05, §26.1 of the module spec).
///
/// The status is derived from the cycle's dates against the current moment and is
/// advanced only by the background status service (or the lazy re-check on read) —
/// never an input on any screen or endpoint. Stored on the row so list filters,
/// the chooser and the coverage report can query it directly.
/// </summary>
public enum OrphanReportCycleStatus
{
    /// <summary>
    /// لم تبدأ — created, the start date is still in the future. No reports may be submitted.
    /// </summary>
    NotStarted = 0,

    /// <summary>
    /// جارية — now falls within [StartDate, EndDate]. Charities may submit and edit reports.
    /// </summary>
    InProgress = 1,

    /// <summary>
    /// مكتملة — the end date has passed. Submissions and charity-side edits are locked;
    /// HQ review (UC-ORR-07/08) is unaffected.
    /// </summary>
    Completed = 2
}
