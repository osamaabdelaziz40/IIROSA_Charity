using IIROSA.Domain.Contracts.Persistence;
using IIROSA.Domain.Entities;
using IIROSA.Domain.Interfaces;
using IIROSA.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace IIROSA.Infrastructure.Data.Repository;

/// <summary>
/// Incoming Letter Repository Implementation
/// Implements data access for UC-12.1 through UC-12.5
/// </summary>
public class IncomingRepository : Repository<Incoming>, IIncomingRepository
{
    private readonly DbSet<Incoming> _dbSet;

    public IncomingRepository(ApplicationDbContext context) : base(context)
    {
        _dbSet = context.Set<Incoming>();
    }

    // Common CRUD wrapper with full filtering (UC-12.1)
    public async Task<(IEnumerable<Incoming> Items, int TotalCount)> GetPagedAsync(
        int pageNumber,
        int pageSize,
        string? searchTerm = null,
        int? departmentId = null,
        string? status = null,
        int? year = null,
        DateTime? startDate = null,
        DateTime? endDate = null,
        Guid? createdByUserId = null,
        string? sortBy = null,
        string? sortOrder = null)
    {
        var query = _dbSet.AsQueryable();

        // Apply search filter
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            query = query.Where(i =>
                i.Subject.Contains(searchTerm) ||
                i.IncomingId.Contains(searchTerm) ||
                i.LetterNumber.Contains(searchTerm) ||
                (i.IncomingNumber != null && i.IncomingNumber.Contains(searchTerm)));
        }

        // Apply department filter
        if (departmentId.HasValue)
        {
            query = query.Where(i => i.FK_DepartmentId == departmentId.Value);
        }

        // Apply status filter
        if (!string.IsNullOrWhiteSpace(status))
        {
            query = query.Where(i => i.Status == status);
        }

        // Apply year filter
        if (year.HasValue)
        {
            query = query.Where(i => i.Year == year.Value);
        }

        // Apply date range filter
        if (startDate.HasValue)
        {
            query = query.Where(i => i.Date >= startDate.Value);
        }

        if (endDate.HasValue)
        {
            query = query.Where(i => i.Date <= endDate.Value);
        }

        // Apply created by user filter
        if (createdByUserId.HasValue)
        {
            query = query.Where(i => i.CreatedBy == createdByUserId.Value.ToString());
        }

        var totalCount = await query.CountAsync();

        // Apply sorting
        query = ApplySorting(query, sortBy, sortOrder);

        var items = await query
            .Include(i => i.Department)
            .Include(i => i.UploadedFile)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    // Business Logic Queries
    public async Task<bool> IsIncomingIdUniqueAsync(string incomingId, Guid? excludeId = null)
    {
        var query = _dbSet.Where(i => i.IncomingId == incomingId);

        if (excludeId.HasValue)
        {
            query = query.Where(i => i.Id != excludeId.Value);
        }

        return await query.CountAsync() == 0;
    }

    public async Task<bool> IsLetterNumberUniqueAsync(string letterNumber, int? departmentId, int? year, Guid? excludeId = null)
    {
        var query = _dbSet.Where(i => i.LetterNumber == letterNumber);

        if (departmentId.HasValue)
        {
            query = query.Where(i => i.FK_DepartmentId == departmentId.Value);
        }

        if (year.HasValue)
        {
            query = query.Where(i => i.Year == year.Value);
        }

        if (excludeId.HasValue)
        {
            query = query.Where(i => i.Id != excludeId.Value);
        }

        return await query.CountAsync() == 0;
    }

    public async Task<IEnumerable<Incoming>> GetByDepartmentAsync(int departmentId)
    {
        return await _dbSet
            .Include(i => i.Department)
            .Where(i => i.FK_DepartmentId == departmentId)
            .OrderByDescending(i => i.CreatedOn)
            .ToListAsync();
    }

    public async Task<IEnumerable<Incoming>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        return await _dbSet
            .Include(i => i.Department)
            .Where(i => i.Date >= startDate && i.Date <= endDate)
            .OrderByDescending(i => i.Date)
            .ToListAsync();
    }

    public async Task<IEnumerable<Incoming>> GetByStatusAsync(string status)
    {
        return await _dbSet
            .Include(i => i.Department)
            .Where(i => i.Status == status)
            .OrderByDescending(i => i.CreatedOn)
            .ToListAsync();
    }

    public async Task<IEnumerable<Incoming>> GetByYearAsync(int year)
    {
        return await _dbSet
            .Include(i => i.Department)
            .Where(i => i.Year == year)
            .OrderByDescending(i => i.CreatedOn)
            .ToListAsync();
    }

    public async Task<IEnumerable<Incoming>> GetByUserAsync(Guid userId)
    {
        return await _dbSet
            .Include(i => i.Department)
            .Where(i => i.FK_UserId == userId)
            .OrderByDescending(i => i.CreatedOn)
            .ToListAsync();
    }

    // Import/Export Support
    public async Task<int> GetNextSerialNumberAsync(int? departmentId = null, int? year = null)
    {
        var query = _dbSet.AsQueryable();

        if (departmentId.HasValue)
        {
            query = query.Where(i => i.FK_DepartmentId == departmentId.Value);
        }

        if (year.HasValue)
        {
            query = query.Where(i => i.Year == year.Value);
        }

        var maxSerial = await query.MaxAsync(i => (int?)i.Serial) ?? 0;
        return maxSerial + 1;
    }

    public async Task<string> GenerateSerialTextAsync(int serial)
    {
        // Format: INC-YYYY-XXXX (e.g., INC-2024-0001)
        var year = DateTime.Now.Year;
        return $"INC-{year}-{serial:D4}";
    }

    #region Helper Methods

    private IQueryable<Incoming> ApplySorting(IQueryable<Incoming> query, string? sortBy, string? sortOrder)
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
                ? query.OrderByDescending(i => i.Date ?? i.CreatedOn)
                : query.OrderBy(i => i.Date ?? i.CreatedOn),
            "serial" => isDescending
                ? query.OrderByDescending(i => i.Serial)
                : query.OrderBy(i => i.Serial),
            "subject" => isDescending
                ? query.OrderByDescending(i => i.Subject)
                : query.OrderBy(i => i.Subject),
            "letterdate" => isDescending
                ? query.OrderByDescending(i => i.LetterDate)
                : query.OrderBy(i => i.LetterDate),
            "createdon" => isDescending
                ? query.OrderByDescending(i => i.CreatedOn)
                : query.OrderBy(i => i.CreatedOn),
            _ => query.OrderByDescending(i => i.Date ?? i.CreatedOn)
        };
    }

    #endregion
}
