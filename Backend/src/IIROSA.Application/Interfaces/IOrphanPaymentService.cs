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

    // UC-PAY-09..13 (§15.1): row actions on ONE endpoint — action 0 = stop/resume (10-9),
    // actions 1..4 land with 10-10..10-13. Caller context drives tenancy + the HQ-stop lock.
    Task<DTOs.OrphanPayment.OrphanPaymentItemDto> UpdateOrphanPaymentItemAsync(
        UpdateOrphanPaymentItemDto dto,
        Guid? userCharityId = null,
        string? userRole = null,
        Guid? userId = null);

    // UC-5.5: Update Payment Group
    Task<OrphanPaymentDto> UpdatePaymentGroupAsync(UpdateOrphanPaymentDto dto);

    // UC-5.6: Lock/Unlock Exchange Rate (included in SetExchangeRate)
    Task LockExchangeRateAsync(Guid orphanPaymentId, bool lockRate);

    // UC-5.7: Mark Group as Uploaded
    Task MarkAsUploadedAsync(MarkAsUploadedDto dto);

    // UC-5.8: View Payment Groups (with filtering and pagination).
    // UC-ORP-08: with filter.OrphanId set this becomes one orphan's payment history — charity-role
    // callers must use that mode (the scoping params verify the orphan is theirs).
    Task<(IEnumerable<OrphanPaymentListDto> Items, int TotalCount)> GetPaymentGroupsAsync(OrphanPaymentFilterDto filter, Guid? userCharityId = null, string? userRole = null);

    // List export to Excel — same scope rules as GetPaymentGroupsAsync
    Task<byte[]> ExportPaymentGroupsToExcelAsync(OrphanPaymentFilterDto filter, Guid? userCharityId = null, string? userRole = null);

    // UC-5.9: View Payment Group Details.
    // UC-ORP-09: with orphanId set, the batch header stays complete but Orphans is filtered to
    // that orphan's rows; out-of-scope orphan → KeyNotFoundException (no existence leak).
    Task<OrphanPaymentDto> GetPaymentGroupDetailsAsync(Guid id, Guid? orphanId = null, Guid? userCharityId = null, string? userRole = null, Guid? charityId = null);

    // UC-ORP-11: distinct batch numbers (رقم الحصة reference list), most-recent batch first.
    // Charity role sees only batches containing its orphans; HQ may narrow by charityId.
    Task<List<BatchNumberDto>> GetBatchNumbersAsync(Guid? userCharityId, string? userRole, Guid? charityId = null);

    // UC-5.10: Export Payment Group Report (placeholder - can be implemented later)
    Task<byte[]> ExportPaymentGroupAsync(Guid id, string format = "Excel", bool includePhotos = false, string groupBy = "None");

    // UC-5.11: Assign Batch Number
    Task AssignBatchNumberAsync(AssignBatchNumberDto dto);

    // UC-5.12 & UC-5.13: Filtering is included in GetPaymentGroupsAsync

    // Orphan Selection for Adding to Group (UC-5.3)
    Task<(IEnumerable<OrphanForPaymentListDto> Items, int TotalCount)> GetAvailableOrphansAsync(OrphanFilterForPaymentDto filter);

    // Helper Methods
    Task<OrphanPaymentDto?> GetByIdAsync(Guid id);
    Task<OrphanPaymentDto?> GetByBatchNoAsync(string batchNo, Guid? userCharityId = null, string? userRole = null);
    Task<bool> IsBatchNoUniqueAsync(string batchNo, Guid? excludeId = null);
    Task DeletePaymentGroupAsync(Guid id);

    // Statistics
    Task<int> GetTotalOrphanCountAsync(Guid orphanPaymentId);
    Task<Dictionary<Guid, int>> GetOrphanCountByCharityAsync(Guid orphanPaymentId);
    Task<Dictionary<int, int>> GetOrphanCountByRegionAsync(Guid orphanPaymentId);
}
