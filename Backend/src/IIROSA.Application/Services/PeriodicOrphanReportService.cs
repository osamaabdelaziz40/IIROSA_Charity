using IIROSA.Application.DTOs.PeriodicOrphanReport;
using IIROSA.Application.Interfaces;
using IIROSA.Domain.Entities;
using IIROSA.Infrastructure.Data.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace IIROSA.Application.Services;

/// <summary>
/// Service for managing Periodic Orphan Reports
/// Implements UC-6.11 through UC-6.17
/// </summary>
public class PeriodicOrphanReportService : IPeriodicOrphanReportService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IRepository<PeriodicOrphanReport> _reportRepository;
    private readonly IRepository<Orphan> _orphanRepository;
    private readonly IRepository<Charity> _charityRepository;
    private readonly ILogger<PeriodicOrphanReportService> _logger;

    public PeriodicOrphanReportService(
        IUnitOfWork unitOfWork,
        IRepository<PeriodicOrphanReport> reportRepository,
        IRepository<Orphan> orphanRepository,
        IRepository<Charity> charityRepository,
        ILogger<PeriodicOrphanReportService> logger)
    {
        _unitOfWork = unitOfWork;
        _reportRepository = reportRepository;
        _orphanRepository = orphanRepository;
        _charityRepository = charityRepository;
        _logger = logger;
    }

    #region CRUD Operations (UC-6.11)

    public async Task<PeriodicOrphanReportDto> CreateReportAsync(CreatePeriodicOrphanReportDto dto)
    {
        // Validate orphan exists
        var orphan = await _orphanRepository.GetByIdAsync(dto.OrphanId);
        if (orphan == null)
            throw new KeyNotFoundException($"Orphan with ID '{dto.OrphanId}' not found.");

        // Check if feature is enabled for charity (UC-6.12)
        if (orphan.FK_CharityId.HasValue && !await IsPeriodicReportsEnabledForCharityAsync(orphan.FK_CharityId.Value))
            throw new InvalidOperationException("Periodic Reports feature is not enabled for your charity. Please contact administrator.");

        // Generate report number if not provided
        if (string.IsNullOrWhiteSpace(dto.ReportNo))
        {
            dto.ReportNo = await GenerateReportNumberAsync();
        }

        var report = new PeriodicOrphanReport
        {
            OrphanId = dto.OrphanId,
            OrphanPaymentId = dto.OrphanPaymentId,
            ReportDate = dto.ReportDate,
            ReportPeriodFrom = dto.ReportPeriodFrom,
            ReportPeriodTo = dto.ReportPeriodTo,
            ReportNo = dto.ReportNo,
            CharityId = orphan.FK_CharityId,

            // Religious & Behavioral
            PrayerStatus = dto.PrayerStatus,
            MannersStatus = dto.MannersStatus,
            HadeethStatus = dto.HadeethStatus,

            // Quran Education
            QuranParts = dto.QuranParts,
            QuranVerses = dto.QuranVerses,

            // Health & Medical
            MedicalStatus = dto.MedicalStatus,
            Disease = dto.Disease,
            Disability = dto.Disability,
            DisabilityDescription = dto.DisabilityDescription,
            DiseaseDescription = dto.DiseaseDescription,
            MedicalReportImageId = dto.MedicalReportImageId,

            // Personal Development
            Hobby = dto.Hobby,
            Course = dto.Course,
            CourseName = dto.CourseName,
            SportName = dto.SportName,
            ProfessionName = dto.ProfessionName,
            Achievement = dto.Achievement,
            AchievementArr = dto.AchievementArr,
            Wish = dto.Wish,
            WishArr = dto.WishArr,
            OrphanMessage = dto.OrphanMessage,

            // Education Details
            EducationalStageId = dto.EducationalStageId,
            EducationalLevelId = dto.EducationalLevelId,
            Grade = dto.Grade,
            School = dto.School,
            SchoolType = dto.SchoolType,
            EducationDegree = dto.EducationDegree,
            HighestEducationalLevel = dto.HighestEducationalLevel,
            HighestEducationalLevelYear = dto.HighestEducationalLevelYear,
            IsOrphanStudent = dto.IsOrphanStudent,
            EducationalYear = dto.EducationalYear,
            AnnualFeeForStudy = dto.AnnualFeeForStudy,
            StudyingYears = dto.StudyingYears,
            RestStudyingYears = dto.RestStudyingYears,
            GraduationYear = dto.GraduationYear,
            DropOut = dto.DropOut,
            DropOutYear = dto.DropOutYear,
            DropOutStageId = dto.DropOutStageId,
            Faculty = dto.Faculty,
            Department = dto.Department,
            Specialization = dto.Specialization,

            // Life Events
            Married = dto.Married,
            MarriageDate = dto.MarriageDate,
            OrphanMarriageImageId = dto.OrphanMarriageImageId,
            Dead = dto.Dead,
            DeathDate = dto.DeathDate,
            OrphanDeadImageId = dto.OrphanDeadImageId,

            // Attachments
            OrphanCertificateImageId = dto.OrphanCertificateImageId,
            OrphanImageId = dto.OrphanImageId,
            MissingDocuments = dto.MissingDocuments,
            MissingDocumentsName = dto.MissingDocumentsName,

            // Status Tracking - UC-6.11
            Reviewed = false,
            IsAccepted = false,
            IsRefused = false,
            Locked = false,
            Active = true,
            ActiveDate = DateTime.UtcNow,
            Deleted = false
        };

        await _reportRepository.AddAsync(report);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Created Periodic Orphan Report {ReportId} for Orphan {OrphanId}", report.Id, report.OrphanId);

        return await MapToDtoAsync(report);
    }

    public async Task<PeriodicOrphanReportDto> UpdateReportAsync(UpdatePeriodicOrphanReportDto dto)
    {
        var report = await _reportRepository.GetByIdAsync(dto.Id);
        if (report == null)
            throw new KeyNotFoundException($"Report with ID '{dto.Id}' not found.");

        // Check if report can be edited (UC-6.11)
        if (report.Locked || report.Reviewed)
            throw new InvalidOperationException("Cannot edit a locked or reviewed report.");

        // Update fields
        report.OrphanPaymentId = dto.OrphanPaymentId ?? report.OrphanPaymentId;
        report.ReportDate = dto.ReportDate ?? report.ReportDate;
        report.ReportPeriodFrom = dto.ReportPeriodFrom ?? report.ReportPeriodFrom;
        report.ReportPeriodTo = dto.ReportPeriodTo ?? report.ReportPeriodTo;
        report.ReportNo = dto.ReportNo ?? report.ReportNo;

        // Religious & Behavioral
        report.PrayerStatus = dto.PrayerStatus ?? report.PrayerStatus;
        report.MannersStatus = dto.MannersStatus ?? report.MannersStatus;
        report.HadeethStatus = dto.HadeethStatus ?? report.HadeethStatus;

        // Quran Education
        report.QuranParts = dto.QuranParts ?? report.QuranParts;
        report.QuranVerses = dto.QuranVerses ?? report.QuranVerses;

        // Health & Medical
        report.MedicalStatus = dto.MedicalStatus ?? report.MedicalStatus;
        report.Disease = dto.Disease ?? report.Disease;
        report.Disability = dto.Disability ?? report.Disability;
        report.DisabilityDescription = dto.DisabilityDescription ?? report.DisabilityDescription;
        report.DiseaseDescription = dto.DiseaseDescription ?? report.DiseaseDescription;
        report.MedicalReportImageId = dto.MedicalReportImageId ?? report.MedicalReportImageId;

        // Personal Development
        report.Hobby = dto.Hobby ?? report.Hobby;
        report.Course = dto.Course ?? report.Course;
        report.CourseName = dto.CourseName ?? report.CourseName;
        report.SportName = dto.SportName ?? report.SportName;
        report.ProfessionName = dto.ProfessionName ?? report.ProfessionName;
        report.Achievement = dto.Achievement ?? report.Achievement;
        report.AchievementArr = dto.AchievementArr ?? report.AchievementArr;
        report.Wish = dto.Wish ?? report.Wish;
        report.WishArr = dto.WishArr ?? report.WishArr;
        report.OrphanMessage = dto.OrphanMessage ?? report.OrphanMessage;

        // Education Details
        report.EducationalStageId = dto.EducationalStageId ?? report.EducationalStageId;
        report.EducationalLevelId = dto.EducationalLevelId ?? report.EducationalLevelId;
        report.Grade = dto.Grade ?? report.Grade;
        report.School = dto.School ?? report.School;
        report.SchoolType = dto.SchoolType ?? report.SchoolType;
        report.EducationDegree = dto.EducationDegree ?? report.EducationDegree;
        report.HighestEducationalLevel = dto.HighestEducationalLevel ?? report.HighestEducationalLevel;
        report.HighestEducationalLevelYear = dto.HighestEducationalLevelYear ?? report.HighestEducationalLevelYear;
        report.IsOrphanStudent = dto.IsOrphanStudent ?? report.IsOrphanStudent;
        report.EducationalYear = dto.EducationalYear ?? report.EducationalYear;
        report.AnnualFeeForStudy = dto.AnnualFeeForStudy ?? report.AnnualFeeForStudy;
        report.StudyingYears = dto.StudyingYears ?? report.StudyingYears;
        report.RestStudyingYears = dto.RestStudyingYears ?? report.RestStudyingYears;
        report.GraduationYear = dto.GraduationYear ?? report.GraduationYear;
        report.DropOut = dto.DropOut ?? report.DropOut;
        report.DropOutYear = dto.DropOutYear ?? report.DropOutYear;
        report.DropOutStageId = dto.DropOutStageId ?? report.DropOutStageId;
        report.Faculty = dto.Faculty ?? report.Faculty;
        report.Department = dto.Department ?? report.Department;
        report.Specialization = dto.Specialization ?? report.Specialization;

        // Life Events
        report.Married = dto.Married ?? report.Married;
        report.MarriageDate = dto.MarriageDate ?? report.MarriageDate;
        report.OrphanMarriageImageId = dto.OrphanMarriageImageId ?? report.OrphanMarriageImageId;
        report.Dead = dto.Dead ?? report.Dead;
        report.DeathDate = dto.DeathDate ?? report.DeathDate;
        report.OrphanDeadImageId = dto.OrphanDeadImageId ?? report.OrphanDeadImageId;

        // Attachments
        report.OrphanCertificateImageId = dto.OrphanCertificateImageId ?? report.OrphanCertificateImageId;
        report.OrphanImageId = dto.OrphanImageId ?? report.OrphanImageId;
        report.MissingDocuments = dto.MissingDocuments ?? report.MissingDocuments;
        report.MissingDocumentsName = dto.MissingDocumentsName ?? report.MissingDocumentsName;

        await _reportRepository.UpdateAsync(report);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Updated Periodic Orphan Report {ReportId}", report.Id);

        return await MapToDtoAsync(report);
    }

    public async Task<PeriodicOrphanReportDto?> GetByIdAsync(Guid id)
    {
        var report = await _reportRepository.GetByIdAsync(id);
        return report == null ? null : await MapToDtoAsync(report);
    }

    public async Task DeleteReportAsync(Guid id)
    {
        var report = await _reportRepository.GetByIdAsync(id);
        if (report == null)
            throw new KeyNotFoundException($"Report with ID '{id}' not found.");

        // Check if report can be deleted (not locked and not reviewed) - UC-6.11
        if (report.Locked || report.Reviewed)
            throw new InvalidOperationException("Cannot delete a locked or reviewed report.");

        // Soft delete
        report.Deleted = true;
        report.DeletedDate = DateTime.UtcNow;
        report.Active = false;

        await _reportRepository.UpdateAsync(report);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Deleted Periodic Orphan Report {ReportId}", id);
    }

    #endregion

    #region Review Operations (UC-6.13)

    public async Task<PeriodicOrphanReportDto> ReviewReportAsync(ReviewPeriodicReportDto dto)
    {
        var report = await _reportRepository.GetByIdAsync(dto.ReportId);
        if (report == null)
            throw new KeyNotFoundException($"Report with ID '{dto.ReportId}' not found.");

        // Check if already reviewed
        if (report.Reviewed)
            throw new InvalidOperationException("This report has already been reviewed.");

        // Update review status - UC-6.13
        report.Reviewed = true;
        report.ReviewedDate = DateTime.UtcNow;
        report.ReviewerId = Guid.Empty; // Will be set by the calling controller with current user ID
        report.IsAccepted = dto.IsApproved;
        report.IsRefused = !dto.IsApproved;
        report.RefuseReason = dto.IsApproved ? null : dto.RefuseReason;
        report.RefuseReasonId = dto.RefuseReasonId;

        await _reportRepository.UpdateAsync(report);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Reviewed Periodic Orphan Report {ReportId}: {Result}", dto.ReportId, dto.IsApproved ? "Approved" : "Rejected");

        return await MapToDtoAsync(report);
    }

    #endregion

    #region List and Filter Operations (UC-6.14, UC-6.15, UC-6.16)

    public async Task<(IEnumerable<PeriodicOrphanReportListDto> Items, int TotalCount)> GetReportsAsync(PeriodicOrphanReportFilterDto filter)
    {
        var query = _reportRepository.GetQueryable()
            .Include(r => r.Orphan)
            .Include(r => r.Charity)
            .Where(r => !r.Deleted && r.Active);

        // Apply filters
        query = ApplyFilters(query, filter);

        // Get total count
        var totalCount = await query.CountAsync();

        // Apply sorting (UC-6.16 - newest first by default)
        query = ApplySorting(query, filter);

        // Apply pagination
        var items = await query
            .Skip((filter.PageNumber - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync();

        var dtos = items.Select(MapToListDto).ToList();

        return (dtos, totalCount);
    }

    public async Task<(IEnumerable<PeriodicOrphanReportListDto> Items, int TotalCount)> GetApprovedReportsAsync(PeriodicOrphanReportFilterDto filter)
    {
        filter.Reviewed = true;
        filter.IsAccepted = true;
        return await GetReportsAsync(filter);
    }

    public async Task<(IEnumerable<PeriodicOrphanReportListDto> Items, int TotalCount)> GetRejectedReportsAsync(PeriodicOrphanReportFilterDto filter)
    {
        filter.Reviewed = true;
        filter.IsRefused = true;
        return await GetReportsAsync(filter);
    }

    public async Task<(IEnumerable<PeriodicOrphanReportListDto> Items, int TotalCount)> GetReportsByOrphanAsync(Guid orphanId, int pageNumber = 1, int pageSize = 20)
    {
        var query = _reportRepository.GetQueryable()
            .Include(r => r.Orphan)
            .Include(r => r.Charity)
            .Where(r => !r.Deleted && r.Active && r.OrphanId == orphanId);

        var totalCount = await query.CountAsync();

        // Order by CreatedOn DESC (newest first) - UC-6.16
        var items = await query
            .OrderByDescending(r => r.CreatedOn)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var dtos = items.Select(MapToListDto).ToList();

        return (dtos, totalCount);
    }

    public async Task<PeriodicOrphanReportSummaryDto> GetOrphanReportSummaryAsync(Guid orphanId)
    {
        var reports = await _reportRepository.GetQueryable()
            .Where(r => !r.Deleted && r.Active && r.OrphanId == orphanId)
            .ToListAsync();

        return new PeriodicOrphanReportSummaryDto
        {
            OrphanId = orphanId,
            TotalReports = reports.Count,
            PendingReports = reports.Count(r => !r.Reviewed),
            ApprovedReports = reports.Count(r => r.IsAccepted),
            RejectedReports = reports.Count(r => r.IsRefused),
            LockedReports = reports.Count(r => r.Locked)
        };
    }

    #endregion

    #region Export Operations (UC-6.17)

    public async Task<byte[]> ExportToExcelAsync(PeriodicOrphanReportFilterDto filter, bool includeAllFields = false)
    {
        // TODO: Implement Excel export using ClosedXML or similar library
        // For now, return empty byte array
        await Task.CompletedTask;
        return Array.Empty<byte>();
    }

    public async Task<byte[]> ExportOrphanHistoryToExcelAsync(Guid orphanId)
    {
        // TODO: Implement Excel export for orphan history
        await Task.CompletedTask;
        return Array.Empty<byte>();
    }

    #endregion

    #region Status Management (UC-6.11)

    public async Task LockReportAsync(Guid id)
    {
        var report = await _reportRepository.GetByIdAsync(id);
        if (report == null)
            throw new KeyNotFoundException($"Report with ID '{id}' not found.");

        report.Locked = true;
        report.LockedDate = DateTime.UtcNow;

        await _reportRepository.UpdateAsync(report);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Locked Periodic Orphan Report {ReportId}", id);
    }

    public async Task UnlockReportAsync(Guid id)
    {
        var report = await _reportRepository.GetByIdAsync(id);
        if (report == null)
            throw new KeyNotFoundException($"Report with ID '{id}' not found.");

        report.Locked = false;
        report.LockedDate = null;

        await _reportRepository.UpdateAsync(report);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Unlocked Periodic Orphan Report {ReportId}", id);
    }

    public async Task ActivateReportAsync(Guid id)
    {
        var report = await _reportRepository.GetByIdAsync(id);
        if (report == null)
            throw new KeyNotFoundException($"Report with ID '{id}' not found.");

        report.Active = true;
        report.ActiveDate = DateTime.UtcNow;

        await _reportRepository.UpdateAsync(report);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Activated Periodic Orphan Report {ReportId}", id);
    }

    public async Task DeactivateReportAsync(Guid id)
    {
        var report = await _reportRepository.GetByIdAsync(id);
        if (report == null)
            throw new KeyNotFoundException($"Report with ID '{id}' not found.");

        report.Active = false;

        await _reportRepository.UpdateAsync(report);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Deactivated Periodic Orphan Report {ReportId}", id);
    }

    #endregion

    #region Helper Methods

    public async Task<bool> CanEditReportAsync(Guid id)
    {
        var report = await _reportRepository.GetByIdAsync(id);
        return report != null && !report.Locked && !report.Reviewed;
    }

    public async Task<bool> CanUserReviewReportsAsync(Guid userId)
    {
        // TODO: Implement role checking logic
        // Only Super Admin, Admin, Accountant, and Employee can review (UC-6.13)
        await Task.CompletedTask;
        return true;
    }

    public async Task<bool> IsPeriodicReportsEnabledForCharityAsync(Guid charityId)
    {
        // UC-6.12: Check Settings table for "PeriodicReportsEnabled_CharityId_[CharityId]"
        // For now, return true (feature enabled by default)
        await Task.CompletedTask;
        return true;
    }

    private async Task<string> GenerateReportNumberAsync()
    {
        // Generate report number like: POR-2026-0001
        var year = DateTime.UtcNow.Year;
        var count = await _reportRepository.GetQueryable()
            .Where(r => r.ReportNo != null && r.ReportNo.StartsWith($"POR-{year}"))
            .CountAsync() + 1;
        return $"POR-{year}-{count:D4}";
    }

    private IQueryable<PeriodicOrphanReport> ApplyFilters(IQueryable<PeriodicOrphanReport> query, PeriodicOrphanReportFilterDto filter)
    {
        // Search term (orphan name or report number) - UC-6.14, UC-6.15, UC-6.16
        if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
        {
            query = query.Where(r =>
                (r.Orphan != null && r.Orphan.FullName.Contains(filter.SearchTerm)) ||
                (r.ReportNo != null && r.ReportNo.Contains(filter.SearchTerm)));
        }

        // Orphan filter - UC-6.16
        if (filter.OrphanId.HasValue)
        {
            query = query.Where(r => r.OrphanId == filter.OrphanId.Value);
        }

        // Charity filter - UC-6.14, UC-6.15
        if (filter.CharityId.HasValue)
        {
            query = query.Where(r => r.CharityId == filter.CharityId.Value);
        }

        // Review status filter
        if (!string.IsNullOrWhiteSpace(filter.ReviewStatus))
        {
            switch (filter.ReviewStatus.ToLower())
            {
                case "pending":
                    query = query.Where(r => !r.Reviewed);
                    break;
                case "approved":
                    query = query.Where(r => r.IsAccepted);
                    break;
                case "rejected":
                    query = query.Where(r => r.IsRefused);
                    break;
            }
        }

        // Reviewed filter
        if (filter.Reviewed.HasValue)
        {
            query = query.Where(r => r.Reviewed == filter.Reviewed.Value);
        }

        // Accepted filter - UC-6.14
        if (filter.IsAccepted.HasValue)
        {
            query = query.Where(r => r.IsAccepted == filter.IsAccepted.Value);
        }

        // Refused filter - UC-6.15
        if (filter.IsRefused.HasValue)
        {
            query = query.Where(r => r.IsRefused == filter.IsRefused.Value);
        }

        // Reviewer filter - UC-6.14, UC-6.15
        if (filter.ReviewerId.HasValue)
        {
            query = query.Where(r => r.ReviewerId == filter.ReviewerId.Value);
        }

        // Report date range filter - UC-6.14, UC-6.15
        if (filter.ReportDateFrom.HasValue)
        {
            query = query.Where(r => r.ReportDate >= filter.ReportDateFrom.Value);
        }
        if (filter.ReportDateTo.HasValue)
        {
            query = query.Where(r => r.ReportDate <= filter.ReportDateTo.Value);
        }

        // Educational stage filter
        if (filter.EducationalStageId.HasValue)
        {
            query = query.Where(r => r.EducationalStageId == filter.EducationalStageId.Value);
        }

        // Educational level filter
        if (filter.EducationalLevelId.HasValue)
        {
            query = query.Where(r => r.EducationalLevelId == filter.EducationalLevelId.Value);
        }

        // Medical status filter - UC-6.14, UC-6.15
        if (!string.IsNullOrWhiteSpace(filter.MedicalStatus))
        {
            query = query.Where(r => r.MedicalStatus == filter.MedicalStatus);
        }

        // Active filter
        if (filter.Active.HasValue)
        {
            query = query.Where(r => r.Active == filter.Active.Value);
        }

        // Locked filter
        if (filter.Locked.HasValue)
        {
            query = query.Where(r => r.Locked == filter.Locked.Value);
        }

        return query;
    }

    private IQueryable<PeriodicOrphanReport> ApplySorting(IQueryable<PeriodicOrphanReport> query, PeriodicOrphanReportFilterDto filter)
    {
        var sortBy = filter.SortBy ?? "CreatedOn";
        var sortDirection = filter.SortDirection ?? "DESC";

        query = sortBy.ToLower() switch
        {
            "reportdate" => sortDirection == "ASC"
                ? query.OrderBy(r => r.ReportDate)
                : query.OrderByDescending(r => r.ReportDate),
            "reportno" => sortDirection == "ASC"
                ? query.OrderBy(r => r.ReportNo ?? "")
                : query.OrderByDescending(r => r.ReportNo ?? ""),
            "orphanname" => sortDirection == "ASC"
                ? query.OrderBy(r => r.Orphan != null ? r.Orphan.FullName : "")
                : query.OrderByDescending(r => r.Orphan != null ? r.Orphan.FullName : ""),
            "reviewstatus" => sortDirection == "ASC"
                ? query.OrderBy(r => r.Reviewed).ThenBy(r => r.IsAccepted)
                : query.OrderByDescending(r => r.Reviewed).ThenByDescending(r => r.IsAccepted),
            _ => sortDirection == "ASC"
                ? query.OrderBy(r => r.CreatedOn)
                : query.OrderByDescending(r => r.CreatedOn)
        };

        return query;
    }

    private async Task<PeriodicOrphanReportDto> MapToDtoAsync(PeriodicOrphanReport report)
    {
        // Load related entities if not already loaded
        if (report.Orphan == null)
        {
            await _reportRepository.GetEntry(report).Reference(r => r.Orphan).LoadAsync();
        }
        if (report.Charity == null && report.CharityId.HasValue)
        {
            await _reportRepository.GetEntry(report).Reference(r => r.Charity).LoadAsync();
        }
        if (report.ReviewerId.HasValue)
        {
            // Load reviewer navigation if needed
        }

        return new PeriodicOrphanReportDto
        {
            Id = report.Id,
            OrphanId = report.OrphanId,
            OrphanCode = report.Orphan?.Code,
            OrphanName = report.Orphan?.FullName,
            OrphanPaymentId = report.OrphanPaymentId,
            ReportDate = report.ReportDate,
            ReportPeriodFrom = report.ReportPeriodFrom,
            ReportPeriodTo = report.ReportPeriodTo,
            ReportNo = report.ReportNo,
            CharityId = report.CharityId,
            CharityName = report.Charity?.Name,
            PrayerStatus = report.PrayerStatus,
            MannersStatus = report.MannersStatus,
            HadeethStatus = report.HadeethStatus,
            QuranParts = report.QuranParts,
            QuranVerses = report.QuranVerses,
            MedicalStatus = report.MedicalStatus,
            Disease = report.Disease,
            Disability = report.Disability,
            DisabilityDescription = report.DisabilityDescription,
            DiseaseDescription = report.DiseaseDescription,
            MedicalReportImageId = report.MedicalReportImageId,
            Hobby = report.Hobby,
            Course = report.Course,
            CourseName = report.CourseName,
            SportName = report.SportName,
            ProfessionName = report.ProfessionName,
            Achievement = report.Achievement,
            AchievementArr = report.AchievementArr,
            Wish = report.Wish,
            WishArr = report.WishArr,
            OrphanMessage = report.OrphanMessage,
            EducationalStageId = report.EducationalStageId,
            EducationalLevelId = report.EducationalLevelId,
            Grade = report.Grade,
            School = report.School,
            SchoolType = report.SchoolType,
            EducationDegree = report.EducationDegree,
            HighestEducationalLevel = report.HighestEducationalLevel,
            HighestEducationalLevelYear = report.HighestEducationalLevelYear,
            IsOrphanStudent = report.IsOrphanStudent,
            EducationalYear = report.EducationalYear,
            AnnualFeeForStudy = report.AnnualFeeForStudy,
            StudyingYears = report.StudyingYears,
            RestStudyingYears = report.RestStudyingYears,
            GraduationYear = report.GraduationYear,
            DropOut = report.DropOut,
            DropOutYear = report.DropOutYear,
            DropOutStageId = report.DropOutStageId,
            Faculty = report.Faculty,
            Department = report.Department,
            Specialization = report.Specialization,
            Married = report.Married,
            MarriageDate = report.MarriageDate,
            OrphanMarriageImageId = report.OrphanMarriageImageId,
            Dead = report.Dead,
            DeathDate = report.DeathDate,
            OrphanDeadImageId = report.OrphanDeadImageId,
            OrphanCertificateImageId = report.OrphanCertificateImageId,
            OrphanImageId = report.OrphanImageId,
            MissingDocuments = report.MissingDocuments,
            MissingDocumentsName = report.MissingDocumentsName,
            Reviewed = report.Reviewed,
            ReviewedDate = report.ReviewedDate,
            ReviewerId = report.ReviewerId,
            Locked = report.Locked,
            LockedDate = report.LockedDate,
            Active = report.Active,
            ActiveDate = report.ActiveDate,
            Deleted = report.Deleted,
            DeletedDate = report.DeletedDate,
            IsAccepted = report.IsAccepted,
            IsRefused = report.IsRefused,
            RefuseReason = report.RefuseReason,
            RefuseReasonId = report.RefuseReasonId,
            MessageId = report.MessageId,
            CreatedOn = report.CreatedOn,
            CreatedBy = report.CreatedBy,
            UpdatedOn = report.UpdatedOn,
            UpdatedBy = report.UpdatedBy
        };
    }

    private PeriodicOrphanReportListDto MapToListDto(PeriodicOrphanReport report)
    {
        return new PeriodicOrphanReportListDto
        {
            Id = report.Id,
            ReportNo = report.ReportNo,
            ReportDate = report.ReportDate,
            ReportPeriodFrom = report.ReportPeriodFrom,
            ReportPeriodTo = report.ReportPeriodTo,
            OrphanId = report.OrphanId,
            OrphanCode = report.Orphan?.Code,
            OrphanName = report.Orphan?.FullName,
            CharityId = report.CharityId,
            CharityName = report.Charity?.Name,
            PrayerStatus = report.PrayerStatus,
            EducationalLevelName = null, // Will be loaded via lookup if needed
            MedicalStatus = report.MedicalStatus,
            Reviewed = report.Reviewed,
            IsAccepted = report.IsAccepted,
            IsRefused = report.IsRefused,
            ReviewerName = report.ReviewerId.ToString(), // Will be loaded from user table
            ReviewedDate = report.ReviewedDate,
            ReviewStatus = !report.Reviewed ? "Pending" : report.IsAccepted ? "Approved" : "Rejected",
            Married = report.Married,
            Dead = report.Dead,
            CreatedOn = report.CreatedOn
        };
    }

    #endregion
}
