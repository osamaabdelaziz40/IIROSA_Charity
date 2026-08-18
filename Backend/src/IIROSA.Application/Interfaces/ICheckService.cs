using IIROSA.Application.DTOs.CheckManagement;

namespace IIROSA.Application.Interfaces;

/// <summary>
/// Check Service Interface
/// Implements business logic for Check management following UC-11.1 to UC-11.10
/// IMPORTANT: Only Admin, Super Admin, and Accountant roles can access this service.
/// Charity users are explicitly blocked from this module.
/// </summary>
public interface ICheckService
{
    // ========== CRUD Operations ==========

    Task<CheckPagedResult<CheckListDto>> GetChecksFilteredAsync(CheckFilterDto filter);
    Task<CheckDetailDto?> GetCheckByIdAsync(Guid id);
    Task<CheckDetailDto> CreateCheckAsync(CreateCheckDto dto);
    Task<CheckDetailDto> UpdateCheckAsync(Guid id, UpdateCheckDto dto);
    Task DeleteCheckAsync(Guid id);

    // ========== Check-Specific Operations ==========

    /// <summary>
    /// Set check amount (UC-11.3)
    /// </summary>
    Task SetCheckAmountAsync(Guid id, SetCheckAmountDto dto);

    /// <summary>
    /// Set check date (UC-11.4)
    /// </summary>
    Task SetCheckDateAsync(Guid id, SetCheckDateDto dto);

    /// <summary>
    /// Mark check as cleared (UC-11.5)
    /// </summary>
    Task MarkCheckAsClearedAsync(Guid id, MarkCheckClearedDto dto);

    /// <summary>
    /// Void check (UC-11.6)
    /// </summary>
    Task VoidCheckAsync(Guid id, VoidCheckDto dto);

    /// <summary>
    /// Reconcile checks (UC-11.9)
    /// </summary>
    Task<CheckReconciliationDto> ReconcileChecksAsync(CheckReconciliationDto reconciliation);

    /// <summary>
    /// Generate check report (UC-11.10)
    /// </summary>
    Task<CheckReportDto> GenerateCheckReportAsync(CheckReportFilterDto filter);

    // ========== View Operations ==========

    Task<List<CheckListDto>> GetPendingChecksAsync();
    Task<List<CheckListDto>> GetIssuedChecksAsync();
    Task<List<CheckListDto>> GetClearedChecksAsync();
    Task<List<CheckListDto>> GetVoidedChecksAsync();
    Task<List<CheckListDto>> GetUnreconciledChecksAsync();
    Task<CheckStatusSummaryDto> GetCheckStatusSummaryAsync();

    // ========== Export ==========

    Task<byte[]> ExportChecksToExcelAsync(CheckFilterDto filter);
}
