using IIROSA.Domain.Contracts.Persistence;
using IIROSA.Domain.Entities;

namespace IIROSA.Domain.Interfaces;

/// <summary>
/// OrphanPayment Repository Interface
/// Provides data access methods for OrphanPayment entity
/// Implements UC-5.1 through UC-5.13
/// </summary>
public interface IOrphanPaymentRepository : IRepository<OrphanPayment>
{
    // Extended Query Methods

    /// <summary>
    /// Get payment group by batch number
    /// </summary>
    Task<OrphanPayment?> GetByBatchNoAsync(string batchNo);

    /// <summary>
    /// Check if batch number is unique
    /// </summary>
    Task<bool> IsBatchNoUniqueAsync(string batchNo, Guid? excludeId = null);

    /// <summary>
    /// Get next available batch number
    /// </summary>
    Task<string> GetNextBatchNumberAsync();

    // Search and Filter Methods (UC-5.8, UC-5.12, UC-5.13)

    /// <summary>
    /// Get payment groups with filtering and pagination (UC-5.8)
    /// </summary>
    Task<(IEnumerable<OrphanPayment> Items, int TotalCount)> GetFilteredPaginatedAsync(
        string? searchTerm = null,
        DateTime? paymentPeriodFrom = null,
        DateTime? paymentPeriodTo = null,
        DateTime? groupDateFrom = null,
        DateTime? groupDateTo = null,
        bool? isBatchUploaded = null,
        Guid? charityId = null,
        int pageNumber = 1,
        int pageSize = 10,
        string? sortBy = null,
        bool sortDescending = false);

    /// <summary>
    /// Get payment groups by charity (UC-5.12)
    /// Returns groups created for this charity OR containing orphans from this charity
    /// </summary>
    Task<IEnumerable<OrphanPayment>> GetByCharityIdAsync(int charityId);

    /// <summary>
    /// Get payment groups by date range (UC-5.13)
    /// </summary>
    Task<IEnumerable<OrphanPayment>> GetByDateRangeAsync(
        DateTime? paymentPeriodFrom = null,
        DateTime? paymentPeriodTo = null,
        DateTime? groupDateFrom = null,
        DateTime? groupDateTo = null);

    /// <summary>
    /// Search payment groups by group name or batch number
    /// </summary>
    Task<IEnumerable<OrphanPayment>> SearchAsync(string searchTerm);

    // Orphan Management Methods (UC-5.3, UC-5.4)

    /// <summary>
    /// Get orphans in a payment group with details
    /// </summary>
    Task<IEnumerable<OrphanPaymentItem>> GetOrphansInGroupAsync(Guid orphanPaymentId);

    /// <summary>
    /// Check if an orphan is already in a payment group
    /// </summary>
    Task<bool> IsOrphanInGroupAsync(Guid orphanPaymentId, Guid orphanId);

    /// <summary>
    /// Get orphan payment item by ID
    /// </summary>
    Task<OrphanPaymentItem?> GetOrphanPaymentItemAsync(Guid id);

    // Statistics Methods (UC-5.9)

    /// <summary>
    /// Get total orphan count in a payment group
    /// </summary>
    Task<int> GetOrphanCountAsync(Guid orphanPaymentId);

    /// <summary>
    /// Get orphan count breakdown by charity for a payment group
    /// Note: Returns Dictionary<Guid, int> because FK_CharityId is Guid? in the entity
    /// </summary>
    Task<Dictionary<Guid, int>> GetOrphanCountByCharityAsync(Guid orphanPaymentId);

    /// <summary>
    /// Get orphan count breakdown by region for a payment group
    /// </summary>
    Task<Dictionary<int, int>> GetOrphanCountByRegionAsync(Guid orphanPaymentId);

    // Include Operations

    /// <summary>
    /// Include navigation properties for detailed queries
    /// </summary>
    System.Linq.IQueryable<OrphanPayment> IncludeNavigationProperties();

    /// <summary>
    /// Include orphans for detailed queries
    /// </summary>
    System.Linq.IQueryable<OrphanPayment> IncludeOrphans();
}
