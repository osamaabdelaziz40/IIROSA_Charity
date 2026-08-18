using IIROSA.Domain.Contracts.Persistence;
using IIROSA.Domain.Entities;
using IIROSA.Domain.Interfaces;
using IIROSA.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace IIROSA.Infrastructure.Data.Repository;

/// <summary>
/// Check Repository Implementation
/// Provides data access operations for Check entity using ApplicationDbContext
/// Implements all use cases UC-11.1 through UC-11.10
/// </summary>
public class CheckRepository : Repository<Check>, ICheckRepository
{
    private readonly DbSet<Check> _dbSet;

    public CheckRepository(ApplicationDbContext context) : base(context)
    {
        _dbSet = context.Set<Check>();
    }

    // ========== Check-Specific Queries ==========

    public async Task<IEnumerable<Check>> GetByStatusAsync(string status)
    {
        return await IncludeNavigationProperties()
            .Where(c => c.CheckStatus == status)
            .OrderByDescending(c => c.CheckDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<Check>> GetByBeneficiaryAsync(string beneficiaryName)
    {
        return await IncludeNavigationProperties()
            .Where(c => c.BeneficiaryName.Contains(beneficiaryName))
            .OrderByDescending(c => c.CheckDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<Check>> GetByBankAsync(int bankId)
    {
        return await IncludeNavigationProperties()
            .Where(c => c.FK_BankId == bankId)
            .OrderByDescending(c => c.CheckDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<Check>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        return await IncludeNavigationProperties()
            .Where(c => c.CheckDate >= startDate && c.CheckDate <= endDate)
            .OrderByDescending(c => c.CheckDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<Check>> GetByCurrencyAsync(string currency)
    {
        return await IncludeNavigationProperties()
            .Where(c => c.Currency == currency)
            .OrderByDescending(c => c.CheckDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<Check>> GetPendingChecksAsync()
    {
        return await IncludeNavigationProperties()
            .Where(c => c.CheckStatus == "Pending")
            .OrderByDescending(c => c.CheckDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<Check>> GetIssuedChecksAsync()
    {
        return await IncludeNavigationProperties()
            .Where(c => c.CheckStatus == "Issued")
            .OrderByDescending(c => c.CheckDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<Check>> GetClearedChecksAsync()
    {
        return await IncludeNavigationProperties()
            .Where(c => c.CheckStatus == "Cleared")
            .OrderByDescending(c => c.ClearanceDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<Check>> GetVoidedChecksAsync()
    {
        return await IncludeNavigationProperties()
            .Where(c => c.CheckStatus == "Void")
            .OrderByDescending(c => c.VoidDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<Check>> GetUnreconciledChecksAsync()
    {
        return await IncludeNavigationProperties()
            .Where(c => c.CheckStatus == "Issued")
            .OrderBy(c => c.CheckDate)
            .ToListAsync();
    }

    // ========== Filtering and Pagination ==========

    public async Task<(IEnumerable<Check> Items, int TotalCount)> GetChecksPagedAsync(
        Expression<Func<Check, bool>>? filter = null,
        Func<IQueryable<Check>, IOrderedQueryable<Check>>? orderBy = null,
        int pageNumber = 1,
        int pageSize = 10)
    {
        var query = IncludeNavigationProperties();

        // Apply filter if provided
        if (filter != null)
        {
            query = query.Where(filter);
        }

        // Get total count before pagination
        var totalCount = await query.CountAsync();

        // Apply ordering
        if (orderBy != null)
        {
            query = orderBy(query);
        }
        else
        {
            // Default ordering by check date descending
            query = query.OrderByDescending(c => c.CheckDate);
        }

        // Apply pagination
        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    // ========== Navigation Properties ==========

    public IQueryable<Check> IncludeNavigationProperties()
    {
        return _dbSet
            .Include(c => c.ChequeBeneficiary)
            .Include(c => c.Bank);
    }
}
