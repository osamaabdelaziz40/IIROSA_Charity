using IIROSA.Application.DTOs.OrphanPayment;

namespace IIROSA.Application.Interfaces;

/// <summary>
/// OrphanPayment Service Interface
/// Implements all use cases UC-5.1 through UC-5.13 for Orphan Payment Groups
/// </summary>
public interface IOrphanPaymentService
{
    // UC-5.1: Create Orphan Payment Group
    Task<OrphanPaymentDto> CreatePaymentGroupAsync(CreateOrphanPaymentDto dto);

    // UC-5.2 & UC-5.6: Set Exchange Rate (includes locking)
    Task SetExchangeRateAsync(SetExchangeRateDto dto);

    // UC-5.3: Add Orphans to Payment Group
    Task<(int AddedCount, int SkippedCount)> AddOrphansToGroupAsync(AddOrphansToGroupDto dto);

    // UC-5.4: Remove Orphan from Group
    Task RemoveOrphanFromGroupAsync(Guid orphanPaymentItemId);

    // UC-5.5: Update Payment Group
    Task<OrphanPaymentDto> UpdatePaymentGroupAsync(UpdateOrphanPaymentDto dto);

    // UC-5.6: Lock/Unlock Exchange Rate (included in SetExchangeRate)
    Task LockExchangeRateAsync(Guid orphanPaymentId, bool lockRate);

    // UC-5.7: Mark Group as Uploaded
    Task MarkAsUploadedAsync(MarkAsUploadedDto dto);

    // UC-5.8: View Payment Groups (with filtering and pagination)
    Task<(IEnumerable<OrphanPaymentListDto> Items, int TotalCount)> GetPaymentGroupsAsync(OrphanPaymentFilterDto filter);

    // UC-5.9: View Payment Group Details
    Task<OrphanPaymentDto> GetPaymentGroupDetailsAsync(Guid id);

    // UC-5.10: Export Payment Group Report (placeholder - can be implemented later)
    Task<byte[]> ExportPaymentGroupAsync(Guid id, string format = "Excel", bool includePhotos = false, string groupBy = "None");

    // UC-5.11: Assign Batch Number
    Task AssignBatchNumberAsync(AssignBatchNumberDto dto);

    // UC-5.12 & UC-5.13: Filtering is included in GetPaymentGroupsAsync

    // Orphan Selection for Adding to Group (UC-5.3)
    Task<(IEnumerable<OrphanForPaymentListDto> Items, int TotalCount)> GetAvailableOrphansAsync(OrphanFilterForPaymentDto filter);

    // Helper Methods
    Task<OrphanPaymentDto?> GetByIdAsync(Guid id);
    Task<OrphanPaymentDto?> GetByBatchNoAsync(string batchNo);
    Task<bool> IsBatchNoUniqueAsync(string batchNo, Guid? excludeId = null);
    Task DeletePaymentGroupAsync(Guid id);

    // Statistics
    Task<int> GetTotalOrphanCountAsync(Guid orphanPaymentId);
    Task<Dictionary<Guid, int>> GetOrphanCountByCharityAsync(Guid orphanPaymentId);
    Task<Dictionary<int, int>> GetOrphanCountByRegionAsync(Guid orphanPaymentId);
}
