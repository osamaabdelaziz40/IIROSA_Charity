using IIROSA.Domain.Contracts.Persistence;
using IIROSA.Domain.Entities;
using System.Linq.Expressions;

namespace IIROSA.Domain.Interfaces;

/// <summary>
/// HqTransfer Repository Interface (UC-TRF-01…08)
/// </summary>
public interface IHqTransferRepository : IRepository<HqTransfer>
{
    /// <summary>
    /// Get a single transfer with its lookup navigations (Country/Department) loaded, so the
    /// detail DTO carries the lookup names and FK ids the edit form patches from (UC-TRF-03,
    /// UC-TRF-04). The detail LINES are not included — use <see cref="GetByIdWithLinesAsync"/>.
    /// </summary>
    Task<HqTransfer?> GetByIdWithLookupsAsync(Guid id);

    /// <summary>
    /// Get a single transfer with its detail lines loaded (UC-TRF-08) — the details screen's
    /// read; keeps the shared IncludeNavigationProperties lean for list/detail reads
    /// </summary>
    Task<HqTransfer?> GetByIdWithLinesAsync(Guid id);

    /// <summary>
    /// Get paginated transfers with optional filtering and sorting; navigations included so list
    /// rows carry their country/department names (UC-TRF-01)
    /// </summary>
    Task<(IEnumerable<HqTransfer> Items, int TotalCount)> GetTransfersPagedAsync(
        Expression<Func<HqTransfer, bool>>? filter = null,
        Func<IQueryable<HqTransfer>, IOrderedQueryable<HqTransfer>>? orderBy = null,
        int pageNumber = 1,
        int pageSize = 10);

    /// <summary>
    /// Register statistics for the band above the §22.S.1 grid — one grouped round-trip over
    /// the scoped rows (!IsDeleted applied here; TotalAmount sums the header's
    /// AmountOfPayment, not the detail lines). Scope pinning happens in the service, before
    /// this call.
    /// </summary>
    Task<(int Total, decimal TotalAmount, int AddedThisMonth)> GetRegisterStatisticsAsync(
        Expression<Func<HqTransfer, bool>>? filter = null);

    /// <summary>
    /// Eager-load the lookup navigations shared by every read path
    /// </summary>
    IQueryable<HqTransfer> IncludeNavigationProperties();
}
