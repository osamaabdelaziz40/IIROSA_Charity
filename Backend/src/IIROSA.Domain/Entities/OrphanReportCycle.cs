using IIROSA.Domain.Entities.Base;
using IIROSA.Domain.Enums;

namespace IIROSA.Domain.Entities;

/// <summary>
/// Orphan Report Cycle entity (UC-ORC-01…09, docs/Modules/26-UC-ORC-Orphan-Report-Cycles.md).
///
/// An HQ-defined reporting round: a named period during which every charity submits one
/// periodic report per coded orphan (دورة التقارير الدورية للأيتام). Status is derived
/// from the dates and advanced only by the background status service — it is never set
/// by hand (BR-ORC-02). Overlapping cycles are permitted, including several In Progress
/// at once (BR-ORC-03): when two cycles run together the orphan screens offer a chooser
/// (UC-ORC-06). One live report per (orphan, cycle) is enforced by a filtered unique
/// index on PeriodicOrphanReport (BR-ORC-05).
/// </summary>
public class OrphanReportCycle : FullAuditedEntity
{
    #region Identity (UC-ORC-02)

    /// <summary>
    /// Cycle title, Arabic — the primary display name (required, BR-ORC-12)
    /// </summary>
    public string TitleAr { get; set; } = string.Empty;

    /// <summary>
    /// Cycle title, English (optional)
    /// </summary>
    public string? TitleEn { get; set; }

    /// <summary>
    /// Free-text description, Arabic (optional)
    /// </summary>
    public string? DescriptionAr { get; set; }

    /// <summary>
    /// Free-text description, English (optional)
    /// </summary>
    public string? DescriptionEn { get; set; }

    #endregion

    #region Duration (UC-ORC-02, BR-ORC-03)

    /// <summary>
    /// First day reports may be submitted. Reaching it moves the cycle In Progress (UC-ORC-05).
    /// </summary>
    public DateTime StartDate { get; set; }

    /// <summary>
    /// Last day reports may be submitted — strictly after <see cref="StartDate"/>. Passing it
    /// completes the cycle and locks charity-side editing (BR-ORC-06). A running cycle's end
    /// date may be extended by HQ but never pulled before today (BR-ORC-08).
    /// </summary>
    public DateTime EndDate { get; set; }

    #endregion

    #region Lifecycle (UC-ORC-05, BR-ORC-02)

    /// <summary>
    /// Derived status — written only by the background status service / the lazy re-check on
    /// read. Never an input: creating or editing a cycle recomputes it from the dates.
    /// </summary>
    public OrphanReportCycleStatus Status { get; set; } = OrphanReportCycleStatus.NotStarted;

    /// <summary>
    /// When the status last changed — set with each background transition for the register's
    /// audit trail. Null while the cycle has never left NotStarted.
    /// </summary>
    public DateTime? StatusChangedOn { get; set; }

    #endregion

    #region Navigation Properties

    /// <summary>
    /// The periodic reports submitted inside this cycle — at most one live row per orphan
    /// (BR-ORC-05, filtered unique index). Restrict on delete: a cycle with reports cannot
    /// be removed (BR-ORC-07).
    /// </summary>
    public virtual ICollection<PeriodicOrphanReport> Reports { get; set; } = new List<PeriodicOrphanReport>();

    #endregion
}
