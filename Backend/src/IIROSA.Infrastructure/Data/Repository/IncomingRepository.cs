using IIROSA.Domain.Contracts.Persistence;
using IIROSA.Domain.Entities;
using IIROSA.Domain.Interfaces;
using IIROSA.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace IIROSA.Infrastructure.Data.Repository;

/// <summary>
/// Incoming Letter Repository Implementation (epic 16, UC-COR-01…09)
/// </summary>
public class IncomingRepository : Repository<Incoming>, IIncomingRepository
{
    private readonly DbSet<Incoming> _dbSet;

    public IncomingRepository(ApplicationDbContext context) : base(context)
    {
        _dbSet = context.Set<Incoming>();
    }

    public async Task<(IEnumerable<Incoming> Items, int TotalCount)> GetPagedAsync(
        IncomingFilterCriteria criteria,
        int pageNumber,
        int pageSize)
    {
        var query = BuildFilteredQuery(criteria);

        var totalCount = await query.CountAsync();

        query = ApplySorting(query, criteria.SortBy, criteria.SortOrder);

        var items = await query
            .Include(i => i.Department)
            .Include(i => i.AssignedUser)
            .Include(i => i.UploadedFile)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    public Task<Incoming?> GetWithDetailsAsync(Guid id)
    {
        // Soft delete is NOT a global query filter in this platform — every read
        // carries an explicit !IsDeleted predicate (review P1)
        return _dbSet
            .Include(i => i.Department)
            .Include(i => i.UploadedFile)
            .Include(i => i.AssignedUser)
            .Include(i => i.OutgoingLetter)
            .Include(i => i.Charity)
            .FirstOrDefaultAsync(i => i.Id == id && !i.IsDeleted);
    }

    public async Task<bool> IsLetterNumberUniqueAsync(string letterNumber, Guid? charityId, int year, Guid? excludeId = null)
    {
        var query = _dbSet.Where(i => !i.IsDeleted && i.LetterNumber == letterNumber && i.Year == year);
        if (charityId.HasValue)
        {
            query = query.Where(i => i.FK_CharityId == charityId.Value);
        }
        else
        {
            // HQ-owned letter (D1 decision, review P14): uniqueness is scoped to the
            // letter's effective charity — NULL-charity letters are checked only
            // against other NULL-charity letters, never against every charity's numbers
            query = query.Where(i => i.FK_CharityId == null);
        }

        if (excludeId.HasValue)
        {
            query = query.Where(i => i.Id != excludeId.Value);
        }

        return !await query.AnyAsync();
    }

    public Task<int> CountOutgoingRepliesAsync(Guid incomingId)
    {
        // Outgoing.IncomingId — outgoing letters replying to this incoming letter;
        // soft-deleted replies must not block the delete (review P1)
        return DbContext.Set<Outgoing>().CountAsync(o => o.IncomingId == incomingId && !o.IsDeleted);
    }

    public async Task<int> GetNextSerialAsync(Guid? charityId, int year)
    {
        // Serials derive from live rows only — a soft-deleted letter's serial is reusable
        var query = _dbSet.Where(i => !i.IsDeleted && i.Year == year);
        if (charityId.HasValue)
        {
            query = query.Where(i => i.FK_CharityId == charityId.Value);
        }

        // Cast to int? so an empty sequence yields 0 instead of throwing
        var max = await query.MaxAsync(i => (int?)i.Serial);
        return (max ?? 0) + 1;
    }

    private IQueryable<Incoming> BuildFilteredQuery(IncomingFilterCriteria criteria)
    {
        // Soft delete is NOT a global query filter in this platform — the register
        // grids never list deleted letters (review P1)
        var query = _dbSet.Where(i => !i.IsDeleted);

        if (!string.IsNullOrWhiteSpace(criteria.SearchTerm))
        {
            // §21.S.1 الموضوع — the subject search; serial and letter number are their
            // own criteria, never folded into this one.
            var term = criteria.SearchTerm.Trim();
            query = query.Where(i => i.Subject.Contains(term));
        }

        if (criteria.Serial.HasValue)
        {
            query = query.Where(i => i.Serial == criteria.Serial.Value);
        }

        if (!string.IsNullOrWhiteSpace(criteria.LetterNumber))
        {
            query = query.Where(i => i.LetterNumber == criteria.LetterNumber);
        }

        if (criteria.DepartmentId.HasValue)
        {
            query = query.Where(i => i.FK_DepartmentId == criteria.DepartmentId.Value);
        }

        if (!string.IsNullOrWhiteSpace(criteria.Status))
        {
            query = query.Where(i => i.Status == criteria.Status);
        }

        if (criteria.Year.HasValue)
        {
            query = query.Where(i => i.Year == criteria.Year.Value);
        }

        if (criteria.StartDate.HasValue)
        {
            query = query.Where(i => i.Date >= criteria.StartDate.Value);
        }

        if (criteria.EndDate.HasValue)
        {
            query = query.Where(i => i.Date <= criteria.EndDate.Value);
        }

        if (criteria.AssignedUserId.HasValue)
        {
            // §21.S.1 الموظف — the responsible employee (FK_UserId), not the audit creator
            query = query.Where(i => i.FK_UserId == criteria.AssignedUserId.Value);
        }

        if (criteria.CharityId.HasValue)
        {
            query = query.Where(i => i.FK_CharityId == criteria.CharityId.Value);
        }

        if (criteria.CountryId.HasValue)
        {
            // Country pin (review P7) — letters carry no country column, the scope rides
            // through the owning charity; HQ-owned (NULL-charity) letters match no country
            var countryCharityIds = DbContext.Set<Charity>()
                .Where(c => c.CountryId == criteria.CountryId.Value)
                .Select(c => c.Id);
            query = query.Where(i => i.FK_CharityId != null && countryCharityIds.Contains(i.FK_CharityId.Value));
        }

        return query;
    }

    private static IQueryable<Incoming> ApplySorting(IQueryable<Incoming> query, string? sortBy, string? sortOrder)
    {
        var isDescending = !"asc".Equals(sortOrder, StringComparison.OrdinalIgnoreCase);

        return (sortBy?.ToLowerInvariant()) switch
        {
            "serial" => isDescending ? query.OrderByDescending(i => i.Serial) : query.OrderBy(i => i.Serial),
            "subject" => isDescending ? query.OrderByDescending(i => i.Subject) : query.OrderBy(i => i.Subject),
            "letterdate" => isDescending ? query.OrderByDescending(i => i.LetterDate) : query.OrderBy(i => i.LetterDate),
            "createdon" => isDescending ? query.OrderByDescending(i => i.CreatedOn) : query.OrderBy(i => i.CreatedOn),
            _ => isDescending
                ? query.OrderByDescending(i => i.Date ?? i.CreatedOn)
                : query.OrderBy(i => i.Date ?? i.CreatedOn)
        };
    }
}
