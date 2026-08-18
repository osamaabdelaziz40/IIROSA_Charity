using IIROSA.Application.DTOs.PeriodicOrphanReport;

namespace IIROSA.Application.Interfaces;

/// <summary>
/// Periodic Orphan Report Service Interface
/// Implements use cases UC-6.11 through UC-6.17 for Periodic Orphan Reports
/// </summary>
public interface IPeriodicOrphanReportService
{
    #region CRUD Operations (UC-6.11)

    /// <summary>
    /// Create new periodic orphan report - UC-6.11
    /// </summary>
    Task<PeriodicOrphanReportDto> CreateReportAsync(CreatePeriodicOrphanReportDto dto);

    /// <summary>
    /// Update existing periodic orphan report - UC-6.11
    /// Only allowed if report is not locked or reviewed
    /// </summary>
    Task<PeriodicOrphanReportDto> UpdateReportAsync(UpdatePeriodicOrphanReportDto dto);

    /// <summary>
    /// Get report by ID - UC-6.11, UC-6.13, UC-6.14, UC-6.15, UC-6.16
    /// </summary>
    Task<PeriodicOrphanReportDto?> GetByIdAsync(Guid id);

    /// <summary>
    /// Delete report (soft delete) - UC-6.11
    /// Only allowed if report is not locked or reviewed
    /// </summary>
    Task DeleteReportAsync(Guid id);

    #endregion

    #region Review Operations (UC-6.13)

    /// <summary>
    /// Review periodic report (approve or reject) - UC-6.13
    /// Only Super Admin, Admin, Accountant, and Employee can review
    /// </summary>
    Task<PeriodicOrphanReportDto> ReviewReportAsync(ReviewPeriodicReportDto dto);

    #endregion

    #region List and Filter Operations (UC-6.14, UC-6.15, UC-6.16)

    /// <summary>
    /// Get periodic reports with filtering and pagination - UC-6.14 (Approved), UC-6.15 (Rejected), UC-6.16 (Search)
    /// Charity users see only their charity's reports
    /// Admin/Super Admin see all reports with optional charity filter
    /// </summary>
    Task<(IEnumerable<PeriodicOrphanReportListDto> Items, int TotalCount)> GetReportsAsync(PeriodicOrphanReportFilterDto filter);

    /// <summary>
    /// Get approved reports - UC-6.14
    /// </summary>
    Task<(IEnumerable<PeriodicOrphanReportListDto> Items, int TotalCount)> GetApprovedReportsAsync(PeriodicOrphanReportFilterDto filter);

    /// <summary>
    /// Get rejected reports - UC-6.15
    /// </summary>
    Task<(IEnumerable<PeriodicOrphanReportListDto> Items, int TotalCount)> GetRejectedReportsAsync(PeriodicOrphanReportFilterDto filter);

    /// <summary>
    /// Get reports by orphan - UC-6.16
    /// Returns orphan's complete periodic report history ordered by creation date (newest first)
    /// </summary>
    Task<(IEnumerable<PeriodicOrphanReportListDto> Items, int TotalCount)> GetReportsByOrphanAsync(Guid orphanId, int pageNumber = 1, int pageSize = 20);

    /// <summary>
    /// Get orphan's report summary - UC-6.16
    /// </summary>
    Task<PeriodicOrphanReportSummaryDto> GetOrphanReportSummaryAsync(Guid orphanId);

    #endregion

    #region Export Operations (UC-6.17)

    /// <summary>
    /// Export periodic reports to Excel - UC-6.17
    /// </summary>
    Task<byte[]> ExportToExcelAsync(PeriodicOrphanReportFilterDto filter, bool includeAllFields = false);

    /// <summary>
    /// Export orphan's report history to Excel - UC-6.16, UC-6.17
    /// </summary>
    Task<byte[]> ExportOrphanHistoryToExcelAsync(Guid orphanId);

    #endregion

    #region Status Management (UC-6.11)

    /// <summary>
    /// Lock report (prevent modifications) - UC-6.11
    /// </summary>
    Task LockReportAsync(Guid id);

    /// <summary>
    /// Unlock report (allow modifications) - UC-6.11
    /// Only Admin/Super Admin can unlock
    /// </summary>
    Task UnlockReportAsync(Guid id);

    /// <summary>
    /// Activate report - UC-6.11
    /// </summary>
    Task ActivateReportAsync(Guid id);

    /// <summary>
    /// Deactivate report - UC-6.11
    /// </summary>
    Task DeactivateReportAsync(Guid id);

    #endregion

    #region Helper Methods

    /// <summary>
    /// Check if report can be edited (not locked and not reviewed)
    /// </summary>
    Task<bool> CanEditReportAsync(Guid id);

    /// <summary>
    /// Check if user can review reports (Admin, Super Admin, Accountant, Employee)
    /// </summary>
    Task<bool> CanUserReviewReportsAsync(Guid userId);

    /// <summary>
    /// Check if Periodic Reports feature is enabled for charity - UC-6.12
    /// </summary>
    Task<bool> IsPeriodicReportsEnabledForCharityAsync(Guid charityId);

    #endregion
}
