namespace IIROSA.Application.DTOs.PeriodicOrphanReport;

/// <summary>
/// Register statistics band shown above the periodic reports grid (§14.S.1, UC-ORR-01).
/// Counts follow the caller's scope exactly like the register read: a charity caller sees
/// their own reports, a country-pinned head-office caller their country's.
/// </summary>
public class PeriodicOrphanReportStatisticsDto
{
    public int Total { get; set; }
    /// <summary>Reviewed and accepted rows — the register's "approved" predicate (Reviewed &amp;&amp; IsAccepted).</summary>
    public int Accepted { get; set; }
    /// <summary>Rows still awaiting review (!Reviewed) — the reviewer's pending queue.</summary>
    public int Pending { get; set; }
    /// <summary>Reports registered since the first day of the current (UTC) month.</summary>
    public int AddedThisMonth { get; set; }
}
