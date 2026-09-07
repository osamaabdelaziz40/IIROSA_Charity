using IIROSA.Application.DTOs.PeriodicOrphanReport;

namespace IIROSA.Application.Interfaces;

/// <summary>
/// Periodic Orphan Report Service Interface
/// Implements epic 9 use cases (UC-ORR-01 … UC-ORR-17, from WAR.IIROSA UC-6.11–6.17)
/// </summary>
public interface IPeriodicOrphanReportService
{
    #region CRUD Operations (UC-ORR-03, UC-ORR-04, UC-ORR-05, UC-ORR-06)

    /// <summary>
    /// Create new periodic orphan report - UC-ORR-03 (create a periodic report)
    /// Duplicate reports for the same orphan/month/year are rejected.
    /// </summary>
    Task<PeriodicOrphanReportDto> CreateReportAsync(CreatePeriodicOrphanReportDto dto);

    /// <summary>
    /// Update existing periodic orphan report - UC-ORR-05
    /// Allowed while unlocked and not accepted; a refused report resubmits as pending.
    /// </summary>
    Task<PeriodicOrphanReportDto> UpdateReportAsync(UpdatePeriodicOrphanReportDto dto);

    /// <summary>
    /// Get report by ID - UC-ORR-04 (view a periodic report) and the review screens
    /// </summary>
    Task<PeriodicOrphanReportDto?> GetByIdAsync(Guid id);

    /// <summary>
    /// UC-ORR-17 (§14.U.17 طباعة التقرير الدوري) — compose the print payload: the caller-scoped
    /// 9-4 detail data, the resolved layout variant (the collapsed 24-template matrix) and the
    /// attachment ids present. Read-only; null when unknown, deleted, or outside the caller's
    /// scope (there is nothing to produce — AC 4/5).
    /// </summary>
    Task<OrphanReportFormPrintDto?> GetPrintFormAsync(Guid id);

    /// <summary>
    /// Delete report (soft delete) - UC-ORR-06
    /// Only allowed while the report is not locked and not accepted.
    /// </summary>
    Task DeleteReportAsync(Guid id);

    #endregion

    #region Orphan Lookup (UC-ORR-02)

    /// <summary>
    /// Look up an orphan by sponsorship code - UC-ORR-02.
    /// Returns null when no live orphan matches, or the orphan is outside the caller's scope.
    /// </summary>
    Task<OrphanLookupDto?> GetOrphanByCodeAsync(string code);

    #endregion

    #region Review Operations (UC-ORR-07, UC-ORR-08)

    /// <summary>
    /// Review periodic report (accept or refuse) - UC-ORR-07 / UC-ORR-08
    /// Reviewer is always resolved from the current user, never from the payload.
    /// </summary>
    Task<PeriodicOrphanReportDto> ReviewReportAsync(ReviewPeriodicReportDto dto);

    #endregion

    #region List and Filter Operations (UC-ORR-01, UC-ORR-09, UC-ORR-12, UC-ORR-13)

    /// <summary>
    /// Get periodic reports with filtering and pagination - UC-ORR-01 / UC-ORR-09
    /// Every query is scoped server-side to the caller's charity/country.
    /// </summary>
    Task<PeriodicOrphanReportPagedResult<PeriodicOrphanReportListDto>> GetReportsAsync(PeriodicOrphanReportFilterDto filter);

    /// <summary>
    /// Get accepted (approved) reports - UC-ORR-12
    /// </summary>
    Task<PeriodicOrphanReportPagedResult<PeriodicOrphanReportListDto>> GetApprovedReportsAsync(PeriodicOrphanReportFilterDto filter);

    /// <summary>
    /// Get refused (rejected) reports - UC-ORR-13
    /// </summary>
    Task<PeriodicOrphanReportPagedResult<PeriodicOrphanReportListDto>> GetRejectedReportsAsync(PeriodicOrphanReportFilterDto filter);

    /// <summary>
    /// Get reports by orphan - UC-ORR-01
    /// Returns the orphan's complete periodic report history, newest first.
    /// </summary>
    Task<PeriodicOrphanReportPagedResult<PeriodicOrphanReportListDto>> GetReportsByOrphanAsync(Guid orphanId, int pageNumber = 1, int pageSize = 20);

    /// <summary>
    /// UC-HOU-06 (§11.S.3): one housing beneficiary's report history, selected by the
    /// ChildOrParent discriminator. Child ⇒ beneficiaryId is an orphan id; Parent ⇒ it is the
    /// housing family's guardian (Provider) id, resolved to the family and read through
    /// FK_HousingFamilyId. Unknown / non-housing / out-of-scope ⇒ NotFoundException.
    /// Served by GET /api/PeriodicOrphanReports/by-orphan/{beneficiaryId}?childOrParent=.
    /// </summary>
    Task<PeriodicOrphanReportPagedResult<PeriodicOrphanReportListDto>> GetHousingBeneficiaryReportsAsync(
        Guid beneficiaryId, Domain.Enums.ReportBeneficiaryType beneficiaryType, int pageNumber = 1, int pageSize = 20);

    /// <summary>
    /// Get orphan's report counters - UC-ORR-01
    /// </summary>
    Task<PeriodicOrphanReportSummaryDto> GetOrphanReportSummaryAsync(Guid orphanId);

    #endregion

    #region Export Operations (UC-ORR-11)

    /// <summary>
    /// Export periodic reports to Excel - UC-ORR-11
    /// </summary>
    Task<byte[]> ExportToExcelAsync(PeriodicOrphanReportFilterDto filter, bool includeAllFields = false);

    /// <summary>
    /// Export orphan's report history to Excel - UC-ORR-01 / UC-ORR-11
    /// </summary>
    Task<byte[]> ExportOrphanHistoryToExcelAsync(Guid orphanId);

    #endregion

    #region Status Management (UC-ORR-03 lifecycle)

    /// <summary>
    /// Lock report (prevent modifications)
    /// </summary>
    Task LockReportAsync(Guid id);

    /// <summary>
    /// Unlock report (allow modifications) - Admin/Super Admin only
    /// </summary>
    Task UnlockReportAsync(Guid id);

    /// <summary>
    /// Activate report
    /// </summary>
    Task ActivateReportAsync(Guid id);

    /// <summary>
    /// Deactivate report
    /// </summary>
    Task DeactivateReportAsync(Guid id);

    #endregion

    #region Helper Methods

    /// <summary>
    /// Check if report can be edited (not locked and not accepted)
    /// </summary>
    Task<bool> CanEditReportAsync(Guid id);

    /// <summary>
    /// Check if the current user can review reports (Admin, Super Admin, Accountant, Employee)
    /// </summary>
    Task<bool> CanUserReviewReportsAsync(Guid userId);

    /// <summary>
    /// Check if Periodic Reports feature is enabled for charity - UC-6.12 / epic 19 (configuration)
    /// </summary>
    Task<bool> IsPeriodicReportsEnabledForCharityAsync(Guid charityId);

    #endregion
}
