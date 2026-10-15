using IIROSA.Domain.Contracts.Persistence;
using IIROSA.Domain.Entities;
using IIROSA.Domain.Enums;

namespace IIROSA.Domain.Interfaces;

/// <summary>
/// OrphanReportCycle repository contract (UC-ORC-01…09).
/// </summary>
public interface IOrphanReportCycleRepository : IRepository<OrphanReportCycle>
{
    /// <summary>
    /// Paged register read (UC-ORC-01) with an optional filter and ordering; newest first
    /// by default. Soft-deleted rows stay out — the caller's filter always adds !IsDeleted.
    /// </summary>
    Task<(IEnumerable<OrphanReportCycle> Items, int TotalCount)> GetPagedAsync(
        System.Linq.Expressions.Expression<Func<OrphanReportCycle, bool>>? filter = null,
        Func<IQueryable<OrphanReportCycle>, IOrderedQueryable<OrphanReportCycle>>? orderBy = null,
        int pageNumber = 1,
        int pageSize = 20);

    /// <summary>
    /// Every live In Progress cycle ordered by start date (UC-ORC-06 chooser). The service
    /// re-checks the window lazily before returning, so a delayed background tick cannot
    /// serve a stale active set.
    /// </summary>
    Task<List<OrphanReportCycle>> GetActiveAsync();

    /// <summary>
    /// The background worker's scan set (UC-ORC-05): tracked rows whose stored status no
    /// longer matches their dates — candidates for the NotStarted→InProgress and →Completed
    /// transitions. Returns tracked entities so the worker mutates and saves in one pass.
    /// </summary>
    Task<List<OrphanReportCycle>> GetDueForStatusChangeAsync(DateTime utcNow);
}
