using IIROSA.Domain.Contracts.Persistence;
using IIROSA.Domain.Entities;
using IIROSA.Domain.Interfaces;
using IIROSA.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace IIROSA.Infrastructure.Data.Repository;

/// <summary>
/// Outgoing Letter Repository Implementation
/// Implements data access for UC-12.6 through UC-12.10
/// </summary>
public class OutgoingRepository : Repository<Outgoing>, IOutgoingRepository
{
    private readonly DbSet<Outgoing> _dbSet;

    public OutgoingRepository(ApplicationDbContext context) : base(context)
    {
        _dbSet = context.Set<Outgoing>();
    }

    // Common CRUD wrapper with full filtering (UC-12.6)
    public async Task<(IEnumerable<Outgoing> Items, int TotalCount)> GetPagedAsync(
        int pageNumber,
        int pageSize,
        string? searchTerm = null,
        int? departmentId = null,
        int? categoryId = null,
        int? year = null,
        DateTime? startDate = null,
        DateTime? endDate = null,
        Guid? createdByUserId = null,
        bool? hasReply = null,
        string? sortBy = null,
        string? sortOrder = null)
    {
        var query = _dbSet.AsQueryable();

        // Apply search filter
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            query = query.Where(o =>
                o.Subject.Contains(searchTerm) ||
                o.OutGoingId.Contains(searchTerm) ||
                (o.OutGoingNumber != null && o.OutGoingNumber.Contains(searchTerm)));
        }

        // Apply department filter
        if (departmentId.HasValue)
        {
            query = query.Where(o => o.Fk_DepartmentId == departmentId.Value);
        }

        // Apply category filter
        if (categoryId.HasValue)
        {
            query = query.Where(o => o.OutgoingCategoryId == categoryId.Value);
        }

        // Apply year filter
        if (year.HasValue)
        {
            query = query.Where(o => o.Year == year.Value);
        }

        // Apply date range filter
        if (startDate.HasValue)
        {
            query = query.Where(o => o.Date >= startDate.Value);
        }

        if (endDate.HasValue)
        {
            query = query.Where(o => o.Date <= endDate.Value);
        }

        // Apply created by user filter
        if (createdByUserId.HasValue)
        {
            query = query.Where(o => o.CreatedBy == createdByUserId.Value.ToString());
        }

        // Apply has reply filter (check if there are incoming letters linked to this outgoing)
        if (hasReply.HasValue)
        {
            if (hasReply.Value)
            {
                query = query.Where(o => o.IncomingId != null);
            }
            else
            {
                query = query.Where(o => o.IncomingId == null);
            }
        }

        var totalCount = await query.CountAsync();

        // Apply sorting
        query = ApplySorting(query, sortBy, sortOrder);

        var items = await query
            .Include(o => o.Department)
            .Include(o => o.Category)
            .Include(o => o.UploadedFile)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    // Business Logic Queries
    public async Task<bool> IsOutgoingIdUniqueAsync(string outgoingId, Guid? excludeId = null)
    {
        var query = _dbSet.Where(o => o.OutGoingId == outgoingId);

        if (excludeId.HasValue)
        {
            query = query.Where(o => o.Id != excludeId.Value);
        }

        return await query.CountAsync() == 0;
    }

    public async Task<IEnumerable<Outgoing>> GetByDepartmentAsync(int departmentId)
    {
        return await _dbSet
            .Include(o => o.Department)
            .Include(o => o.Category)
            .Where(o => o.Fk_DepartmentId == departmentId)
            .OrderByDescending(o => o.CreatedOn)
            .ToListAsync();
    }

    public async Task<IEnumerable<Outgoing>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        return await _dbSet
            .Include(o => o.Department)
            .Include(o => o.Category)
            .Where(o => o.Date >= startDate && o.Date <= endDate)
            .OrderByDescending(o => o.Date)
            .ToListAsync();
    }

    public async Task<IEnumerable<Outgoing>> GetByYearAsync(int year)
    {
        return await _dbSet
            .Include(o => o.Department)
            .Include(o => o.Category)
            .Where(o => o.Year == year)
            .OrderByDescending(o => o.CreatedOn)
            .ToListAsync();
    }

    public async Task<IEnumerable<Outgoing>> GetByCategoryAsync(int categoryId)
    {
        return await _dbSet
            .Include(o => o.Department)
            .Include(o => o.Category)
            .Where(o => o.OutgoingCategoryId == categoryId)
            .OrderByDescending(o => o.CreatedOn)
            .ToListAsync();
    }

    public async Task<IEnumerable<Outgoing>> GetRepliesToIncomingAsync(Guid incomingId)
    {
        return await _dbSet
            .Include(o => o.Department)
            .Include(o => o.Category)
            .Where(o => o.IncomingId == incomingId)
            .OrderByDescending(o => o.CreatedOn)
            .ToListAsync();
    }

    public async Task<IEnumerable<Outgoing>> GetByUserAsync(Guid userId)
    {
        return await _dbSet
            .Include(o => o.Department)
            .Include(o => o.Category)
            .Where(o => o.CreatedBy == userId.ToString())
            .OrderByDescending(o => o.CreatedOn)
            .ToListAsync();
    }

    // Import/Export Support
    public async Task<int> GetNextSerialNumberAsync(int? departmentId = null, int? year = null)
    {
        var query = _dbSet.AsQueryable();

        if (departmentId.HasValue)
        {
            query = query.Where(o => o.Fk_DepartmentId == departmentId.Value);
        }

        if (year.HasValue)
        {
            query = query.Where(o => o.Year == year.Value);
        }

        var maxSerial = await query.MaxAsync(o => (int?)o.Serial) ?? 0;
        return maxSerial + 1;
    }

    #region Helper Methods

    private IQueryable<Outgoing> ApplySorting(IQueryable<Outgoing> query, string? sortBy, string? sortOrder)
    {
        sortBy = sortBy?.ToLower();
        sortOrder = sortOrder?.ToLower();

        // Default sort: Date descending
        if (string.IsNullOrWhiteSpace(sortBy))
        {
            sortBy = "date";
        }

        var isDescending = sortOrder == "descending";

        return sortBy switch
        {
            "date" => isDescending
                ? query.OrderByDescending(o => o.Date ?? o.CreatedOn)
                : query.OrderBy(o => o.Date ?? o.CreatedOn),
            "serial" => isDescending
                ? query.OrderByDescending(o => o.Serial)
                : query.OrderBy(o => o.Serial),
            "subject" => isDescending
                ? query.OrderByDescending(o => o.Subject)
                : query.OrderBy(o => o.Subject),
            "outgoingid" => isDescending
                ? query.OrderByDescending(o => o.OutGoingId)
                : query.OrderBy(o => o.OutGoingId),
            "createdon" => isDescending
                ? query.OrderByDescending(o => o.CreatedOn)
                : query.OrderBy(o => o.CreatedOn),
            _ => query.OrderByDescending(o => o.Date ?? o.CreatedOn)
        };
    }

    #endregion
}
