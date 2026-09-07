using IIROSA.Domain.Contracts.Persistence;
using IIROSA.Domain.Entities;
using IIROSA.Domain.Interfaces;
using IIROSA.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace IIROSA.Infrastructure.Data.Repository;

/// <summary>
/// Outgoing Letter Repository Implementation (epic 16, UC-COR-10…19)
/// </summary>
public class OutgoingRepository : Repository<Outgoing>, IOutgoingRepository
{
    private readonly DbSet<Outgoing> _dbSet;

    public OutgoingRepository(ApplicationDbContext context) : base(context)
    {
        _dbSet = context.Set<Outgoing>();
    }

    public async Task<(IEnumerable<Outgoing> Items, int TotalCount)> GetPagedAsync(
        OutgoingFilterCriteria criteria,
        int pageNumber,
        int pageSize)
    {
        var query = BuildFilteredQuery(criteria);

        var totalCount = await query.CountAsync();

        query = ApplySorting(query, criteria.SortBy, criteria.SortOrder);

        var items = await query
            .Include(o => o.Department)
            .Include(o => o.Category)
            .Include(o => o.UploadedFile)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    public Task<Outgoing?> GetWithDetailsAsync(Guid id)
    {
        // Soft delete is NOT a global query filter in this platform — every read
        // carries an explicit !IsDeleted predicate (review P1); detached (soft-deleted)
        // orphan links are filtered out of the included collection as well
        return _dbSet
            .Include(o => o.Department)
            .Include(o => o.Category)
            .Include(o => o.UploadedFile)
            .Include(o => o.IncomingLetter)
            .Include(o => o.Charity)
            .Include(o => o.OrphanReports.Where(r => !r.IsDeleted)).ThenInclude(r => r.Orphan).ThenInclude(or => or!.Family)
            .FirstOrDefaultAsync(o => o.Id == id && !o.IsDeleted);
    }

    public async Task<int> GetNextSerialAsync(Guid? charityId, int year)
    {
        // Serials derive from live rows only — a soft-deleted letter's serial is reusable
        var query = _dbSet.Where(o => !o.IsDeleted && o.Year == year);
        if (charityId.HasValue)
        {
            query = query.Where(o => o.FK_CharityId == charityId.Value);
        }

        // Cast to int? so an empty sequence yields 0 instead of throwing
        var max = await query.MaxAsync(o => (int?)o.Serial);
        return (max ?? 0) + 1;
    }

    private IQueryable<Outgoing> BuildFilteredQuery(OutgoingFilterCriteria criteria)
    {
        // Soft delete is NOT a global query filter in this platform — the register
        // grids never list deleted letters (review P1)
        var query = _dbSet.Where(o => !o.IsDeleted);

        if (!string.IsNullOrWhiteSpace(criteria.SearchTerm))
        {
            // §21.S.4 الموضوع — the subject search; the serial is its own criterion,
            // never folded into this one.
            var term = criteria.SearchTerm.Trim();
            query = query.Where(o => o.Subject.Contains(term));
        }

        if (criteria.Serial.HasValue)
        {
            query = query.Where(o => o.Serial == criteria.Serial.Value);
        }

        if (criteria.DepartmentId.HasValue)
        {
            query = query.Where(o => o.Fk_DepartmentId == criteria.DepartmentId.Value);
        }

        if (criteria.CategoryId.HasValue)
        {
            query = query.Where(o => o.OutgoingCategoryId == criteria.CategoryId.Value);
        }

        if (criteria.Year.HasValue)
        {
            query = query.Where(o => o.Year == criteria.Year.Value);
        }

        if (criteria.StartDate.HasValue)
        {
            query = query.Where(o => o.Date >= criteria.StartDate.Value);
        }

        if (criteria.EndDate.HasValue)
        {
            query = query.Where(o => o.Date <= criteria.EndDate.Value);
        }

        if (criteria.HasReply.HasValue)
        {
            query = criteria.HasReply.Value
                ? query.Where(o => o.IncomingId != null)
                : query.Where(o => o.IncomingId == null);
        }

        if (criteria.CharityId.HasValue)
        {
            query = query.Where(o => o.FK_CharityId == criteria.CharityId.Value);
        }

        if (criteria.CountryId.HasValue)
        {
            // Country pin (review P7) — letters carry no country column, the scope rides
            // through the owning charity; HQ-owned (NULL-charity) letters match no country
            var countryCharityIds = DbContext.Set<Charity>()
                .Where(c => c.CountryId == criteria.CountryId.Value)
                .Select(c => c.Id);
            query = query.Where(o => o.FK_CharityId != null && countryCharityIds.Contains(o.FK_CharityId.Value));
        }

        return query;
    }

    private static IQueryable<Outgoing> ApplySorting(IQueryable<Outgoing> query, string? sortBy, string? sortOrder)
    {
        var isDescending = !"asc".Equals(sortOrder, StringComparison.OrdinalIgnoreCase);

        return (sortBy?.ToLowerInvariant()) switch
        {
            "serial" => isDescending ? query.OrderByDescending(o => o.Serial) : query.OrderBy(o => o.Serial),
            "subject" => isDescending ? query.OrderByDescending(o => o.Subject) : query.OrderBy(o => o.Subject),
            "createdon" => isDescending ? query.OrderByDescending(o => o.CreatedOn) : query.OrderBy(o => o.CreatedOn),
            _ => isDescending
                ? query.OrderByDescending(o => o.Date ?? o.CreatedOn)
                : query.OrderBy(o => o.Date ?? o.CreatedOn)
        };
    }
}
