using IIROSA.Domain.Contracts.Persistence;
using IIROSA.Domain.Entities;
using IIROSA.Domain.Enums;
using IIROSA.Domain.Interfaces;
using IIROSA.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace IIROSA.Infrastructure.Data.Repository;

/// <summary>
/// OrphanReportCycle repository (UC-ORC-01…09, docs/Modules/26-UC-ORC).
/// OrphanReportCycle has no navigation properties to include — the paged read is a flat
/// scan shared by the HQ register; the chooser and the background worker reads are the
/// two shaped queries below.
/// </summary>
public class OrphanReportCycleRepository : Repository<OrphanReportCycle>, IOrphanReportCycleRepository
{
    private readonly DbSet<OrphanReportCycle> _dbSet;

    public OrphanReportCycleRepository(ApplicationDbContext context) : base(context)
    {
        _dbSet = context.Set<OrphanReportCycle>();
    }

    /// <summary>Paged register read with an optional filter and ordering; newest first by default.</summary>
    public async Task<(IEnumerable<OrphanReportCycle> Items, int TotalCount)> GetPagedAsync(
        Expression<Func<OrphanReportCycle, bool>>? filter = null,
        Func<IQueryable<OrphanReportCycle>, IOrderedQueryable<OrphanReportCycle>>? orderBy = null,
        int pageNumber = 1,
        int pageSize = 20)
    {
        var query = _dbSet.AsQueryable();

        if (filter != null)
        {
            query = query.Where(filter);
        }

        var totalCount = await query.CountAsync();

        query = orderBy != null
            ? orderBy(query)
            : query.OrderByDescending(c => c.CreatedOn);

        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    /// <summary>
    /// Every live In Progress cycle ordered by start date (UC-ORC-06 chooser). The stored
    /// status is trusted here — the service layer refreshes due transitions before reading,
    /// so a delayed background tick cannot serve a stale active set.
    /// </summary>
    public async Task<List<OrphanReportCycle>> GetActiveAsync()
    {
        return await _dbSet
            .AsNoTracking()
            .Where(c => !c.IsDeleted && c.Status == OrphanReportCycleStatus.InProgress)
            .OrderBy(c => c.StartDate)
            .ToListAsync();
    }

    /// <summary>
    /// The background worker's scan set (UC-ORC-05): tracked rows whose stored status no
    /// longer matches their dates — NotStarted rows whose start has arrived, and
    /// not-yet-completed rows whose end has passed. Tracked so the caller mutates and
    /// saves in one pass.
    /// </summary>
    public async Task<List<OrphanReportCycle>> GetDueForStatusChangeAsync(DateTime utcNow)
    {
        return await _dbSet
            .Where(c => !c.IsDeleted)
            .Where(c =>
                (c.Status == OrphanReportCycleStatus.NotStarted && c.StartDate <= utcNow)
                || (c.Status != OrphanReportCycleStatus.Completed && c.EndDate < utcNow))
            .ToListAsync();
    }
}
