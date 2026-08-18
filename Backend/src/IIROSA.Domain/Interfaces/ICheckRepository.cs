using IIROSA.Domain.Contracts.Persistence;
using IIROSA.Domain.Entities;

namespace IIROSA.Domain.Interfaces;

/// <summary>
/// Check Repository Interface
/// Provides data access operations for Check entity
/// Implements all use cases UC-11.1 through UC-11.10
/// </summary>
public interface ICheckRepository : IRepository<Check>
{
    // ========== Check-Specific Queries ==========

    Task<IEnumerable<Check>> GetByStatusAsync(string status);
    Task<IEnumerable<Check>> GetByBeneficiaryAsync(string beneficiaryName);
    Task<IEnumerable<Check>> GetByBankAsync(int bankId);
    Task<IEnumerable<Check>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
    Task<IEnumerable<Check>> GetByCurrencyAsync(string currency);
    Task<IEnumerable<Check>> GetPendingChecksAsync();
    Task<IEnumerable<Check>> GetIssuedChecksAsync();
    Task<IEnumerable<Check>> GetClearedChecksAsync();
    Task<IEnumerable<Check>> GetVoidedChecksAsync();
    Task<IEnumerable<Check>> GetUnreconciledChecksAsync();

    // ========== Filtering and Pagination ==========

    Task<(IEnumerable<Check> Items, int TotalCount)> GetChecksPagedAsync(
        System.Linq.Expressions.Expression<Func<Check, bool>>? filter = null,
        Func<IQueryable<Check>, IOrderedQueryable<Check>>? orderBy = null,
        int pageNumber = 1,
        int pageSize = 10);

    // ========== Navigation Properties ==========

    IQueryable<Check> IncludeNavigationProperties();
}
