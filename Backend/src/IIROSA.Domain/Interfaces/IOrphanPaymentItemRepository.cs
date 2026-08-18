using IIROSA.Domain.Contracts.Persistence;
using IIROSA.Domain.Entities;

namespace IIROSA.Domain.Interfaces;

/// <summary>
/// OrphanPaymentItem Repository Interface
/// Provides data access methods for OrphanPaymentItem entity (orphans in payment groups)
/// </summary>
public interface IOrphanPaymentItemRepository : IRepository<OrphanPaymentItem>
{
    /// <summary>
    /// Get all items for a specific payment group
    /// </summary>
    Task<IEnumerable<OrphanPaymentItem>> GetByPaymentGroupIdAsync(Guid orphanPaymentId);

    /// <summary>
    /// Get all items for a specific orphan across all payment groups
    /// </summary>
    Task<IEnumerable<OrphanPaymentItem>> GetByOrphanIdAsync(Guid orphanId);

    /// <summary>
    /// Check if orphan is in payment group
    /// </summary>
    Task<bool> ExistsAsync(Guid orphanPaymentId, Guid orphanId);

    /// <summary>
    /// Remove all orphans from a payment group
    /// </summary>
    Task RemoveAllFromGroupAsync(Guid orphanPaymentId);

    /// <summary>
    /// Get item with orphan details
    /// </summary>
    Task<OrphanPaymentItem?> GetWithOrphanAsync(Guid id);

    /// <summary>
    /// Get items with orphan details for a payment group
    /// </summary>
    Task<IEnumerable<OrphanPaymentItem>> GetWithOrphansByGroupAsync(Guid orphanPaymentId);
}
