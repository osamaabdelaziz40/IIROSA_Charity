using IIROSA.Application.DTOs.OrphanReportCycles;

namespace IIROSA.Application.Interfaces;

/// <summary>
/// Orphan Report Cycle service contract (UC-ORC-01…09, docs/Modules/26-UC-ORC).
///
/// Owns the cycle register CRUD, the derived status lifecycle (shared by the background
/// worker and the lazy re-check), the orphan-context chooser payload and the coverage
/// report. Status is never a write input anywhere: create/update recompute it from the
/// dates, and RefreshDueStatusesAsync is the only writer of stored transitions.
/// Tenancy: cycles are HQ-defined and platform-wide; coverage is scoped to the caller —
/// a charity user's CharityId pins the population, a head-office caller may filter by
/// an explicit charity id.
/// </summary>
public interface IOrphanReportCycleService
{
    /// <summary>The HQ register (UC-ORC-01) — paged, newest first, with report counts and progress.</summary>
    Task<OrphanReportCyclePagedResult> GetFilteredAsync(OrphanReportCycleFilterDto filter);

    /// <summary>Detail read for the edit screen (UC-ORC-03).</summary>
    Task<OrphanReportCycleDto?> GetByIdAsync(Guid id);

    /// <summary>
    /// Create (UC-ORC-02): validates the period and stores the cycle with its status
    /// computed from the dates — NotStarted for a future period, InProgress when it
    /// already runs. Overlapping cycles are legal (BR-ORC-03).
    /// </summary>
    Task<OrphanReportCycleDto> CreateAsync(CreateOrphanReportCycleDto dto);

    /// <summary>
    /// Update (UC-ORC-03): master data and dates while not completed. BR-ORC-08 guards —
    /// a completed cycle is read-only; a running cycle's EndDate may be extended but
    /// never pulled before today. Status recomputes on save.
    /// </summary>
    Task<OrphanReportCycleDto> UpdateAsync(UpdateOrphanReportCycleDto dto);

    /// <summary>
    /// Soft-delete (UC-ORC-04): only a NotStarted cycle with zero bound reports
    /// (BR-ORC-07). Completed cycles are the audit record of a reporting round and are
    /// never deleted.
    /// </summary>
    Task DeleteAsync(Guid id);

    /// <summary>
    /// Every non-deleted cycle, newest first, with status derived on read — the
    /// charity-facing picker read behind the report-entry button gating and the coverage
    /// screen's cycle dropdown. Distinct from the HQ register: no counts, no paging,
    /// open to all authenticated callers (cycles are platform-wide reference data).
    /// </summary>
    Task<List<OrphanReportCycleSelectorDto>> GetSelectorAsync();

    /// <summary>
    /// The live In Progress set (UC-ORC-06), each with this orphan's submission state.
    /// Refreshes due transitions first, so a delayed background tick cannot serve a
    /// stale chooser. Empty list when no cycle runs — the caller hides the action.
    /// </summary>
    Task<List<OrphanCycleReportStatusDto>> GetOrphanCycleReportStatusAsync(Guid orphanId);

    /// <summary>
    /// Advance every cycle whose stored status no longer matches its dates (UC-ORC-05):
    /// NotStarted → InProgress when the start arrives, → Completed when the end passes.
    /// Called by the background worker on its interval and lazily before chooser/coverage
    /// reads. Returns the number of transitions applied.
    /// </summary>
    Task<int> RefreshDueStatusesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// The coverage split (UC-ORC-09): the coded-orphan population of the caller's scope,
    /// each row flagged submitted (with report date and review state) or not. Category
    /// pages one tab; counts cover both.
    /// </summary>
    Task<CycleReportCoverageResultDto> GetCoverageAsync(CycleReportCoverageFilterDto filter);
}
