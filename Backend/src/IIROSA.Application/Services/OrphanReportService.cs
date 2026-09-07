using IIROSA.Application.DTOs.OrphanReport;
using IIROSA.Application.Interfaces;
using IIROSA.Domain.Contracts.Persistence;
using IIROSA.Domain.Entities;
using IIROSA.Domain.Entities.Lookups;
using IIROSA.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace IIROSA.Application.Services;

/// <summary>
/// Service for managing Orphan Summary Reports
/// Implements UC-6.1 through UC-6.10
/// </summary>
public class OrphanReportService : IOrphanReportService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IRepository<Orphan> _orphanRepository;
    private readonly IRepository<Charity> _charityRepository;
    private readonly IRepository<Family> _familyRepository;
    private readonly IRepository<PeriodicOrphanReport> _periodicReportRepository;
    private readonly IRepository<EducationLevel> _educationLevelRepository;
    private readonly ICurrentUserService _currentUser;
    private readonly ILogger<OrphanReportService> _logger;

    public OrphanReportService(
        IUnitOfWork unitOfWork,
        IRepository<Orphan> orphanRepository,
        IRepository<Charity> charityRepository,
        IRepository<Family> familyRepository,
        IRepository<PeriodicOrphanReport> periodicReportRepository,
        IRepository<EducationLevel> educationLevelRepository,
        ICurrentUserService currentUser,
        ILogger<OrphanReportService> logger)
    {
        _unitOfWork = unitOfWork;
        _orphanRepository = orphanRepository;
        _charityRepository = charityRepository;
        _familyRepository = familyRepository;
        _periodicReportRepository = periodicReportRepository;
        _educationLevelRepository = educationLevelRepository;
        _currentUser = currentUser;
        _logger = logger;
    }

    #region Report Generation (UC-6.1)

    public async Task<OrphanReportResultDto> GenerateReportAsync(OrphanReportFilterDto filter)
    {
        var query = _orphanRepository.AsQueryable()
            .Include(o => o.Family)
                .ThenInclude(f => f!.Father)
            .Include(o => o.Family)
                .ThenInclude(f => f!.Mother)
            .Include(o => o.Family)
                .ThenInclude(f => f!.Providers)
            .Include(o => o.Family)
                .ThenInclude(f => f!.Region)
            .Include(o => o.Family)
                .ThenInclude(f => f!.Center)
            .Include(o => o.Sponsor)
            .Include(o => o.EducationLevel)
            .Include(o => o.HealthStatus)
            .Where(o => !o.IsDeleted);

        // Apply charity filter (UC-6.4) — caller scope: a charity claim pins and a
        // forged request charityId is ignored (§14.U.11 AC 3).
        // Review P2/P3 2026-08-24: was `_currentUser.CharityId ?? filter.CharityId` — no HQ
        // gate, no country pin, fail-open with no claims. Now the ApplyCharityScopeAsync ladder.
        var scope = await ResolveCallerScopeAsync(filter.CharityId);
        query = ApplyScope(query, scope);

        // Apply region filter (UC-6.5)
        if (filter.RegionId.HasValue)
        {
            query = query.Where(o => o.Family != null && o.Family.RegionId == filter.RegionId.Value);
        }

        // Apply center filter (UC-6.1)
        if (filter.CenterId.HasValue)
        {
            query = query.Where(o => o.Family != null && o.Family.CenterId == filter.CenterId.Value);
        }

        // Apply sponsorship status filter (UC-6.3)
        if (!string.IsNullOrWhiteSpace(filter.SponsorshipStatus) && filter.SponsorshipStatus != "All")
        {
            query = query.Where(o => o.SponsorshipStatus == filter.SponsorshipStatus);
        }

        // Apply age range filter (UC-6.1)
        if (filter.AgeFrom.HasValue)
        {
            var ageFromDate = DateTime.UtcNow.AddYears(-filter.AgeFrom.Value);
            query = query.Where(o => o.DateOfBirth <= ageFromDate);
        }
        if (filter.AgeTo.HasValue)
        {
            var ageToDate = DateTime.UtcNow.AddYears(-filter.AgeTo.Value - 1);
            query = query.Where(o => o.DateOfBirth >= ageToDate);
        }

        // Apply gender filter (UC-6.1)
        if (!string.IsNullOrWhiteSpace(filter.Gender) && filter.Gender != "All")
        {
            query = query.Where(o => o.Gender == filter.Gender);
        }

        var orphans = await query.ToListAsync();

        // Orphan has no Charity navigation — resolve charity names in one batch.
        var charityIds = orphans.Where(o => o.FK_CharityId.HasValue)
            .Select(o => o.FK_CharityId!.Value)
            .Distinct()
            .ToList();
        var charityNames = charityIds.Count == 0
            ? new Dictionary<Guid, string>()
            : await _charityRepository.AsQueryable()
                .Where(c => charityIds.Contains(c.Id))
                .ToDictionaryAsync(c => c.Id, c => c.Name);

        // Map to DTOs
        var orphanDtos = new List<OrphanReportDto>();
        foreach (var orphan in orphans)
        {
            var dto = new OrphanReportDto
            {
                OrphanId = orphan.Id,
                OrphanCode = orphan.Code,
                OrphanName = orphan.FullName,
                DateOfBirth = orphan.DateOfBirth,
                Age = CalculateAge(orphan.DateOfBirth),
                Gender = orphan.Gender,
                OrphanType = orphan.OrphanType,
                SponsorshipStatus = orphan.SponsorshipStatus,
                CharityId = orphan.FK_CharityId,
                CharityName = orphan.FK_CharityId.HasValue && charityNames.TryGetValue(orphan.FK_CharityId.Value, out var charityName)
                    ? charityName
                    : null,
                RegionId = orphan.Family?.RegionId,
                RegionName = orphan.Family?.Region?.Name,
                CenterId = orphan.Family?.CenterId,
                CenterName = orphan.Family?.Center?.Name,
                SponsorId = orphan.SponsorId,
                SponsorName = orphan.Sponsor?.FullName,
                MonthlyAmount = orphan.MonthlyAmount,
                PhotoAttachmentId = orphan.PhotoAttachmentId
            };

            // Include Family Details if requested (UC-6.6)
            if (filter.IncludeFamilyDetails)
            {
                dto.FamilyAddress = orphan.Family?.Address;
                dto.FatherName = orphan.Family?.Father?.FullName;
                dto.MotherName = orphan.Family?.Mother?.FullName;
                // §11.S.2 multi-guardian: PRIMARY guardian = first live row by CreatedOn/Id.
                dto.ProviderName = orphan.Family?.Providers
                    ?.Where(p => !p.IsDeleted)
                    .OrderBy(p => p.CreatedOn).ThenBy(p => p.Id)
                    .Select(p => p.FullName)
                    .FirstOrDefault();
                dto.FamilyPhone = orphan.Family?.PhoneNumber;
            }

            // Include Contact Information if requested (UC-6.1)
            if (filter.IncludeContactInformation)
            {
                dto.OrphanPhone = orphan.Phone;
                dto.OrphanEmail = orphan.Email;
            }

            // Include Education Details if requested (UC-6.1)
            if (filter.IncludeEducationDetails)
            {
                dto.EducationLevelId = orphan.EducationLevelId;
                dto.EducationLevelName = orphan.EducationLevel?.Name;
                dto.SchoolName = orphan.SchoolName;
                dto.GradeClass = orphan.GradeClass;
                dto.AcademicPerformance = orphan.AcademicPerformance;
            }

            // Include Health Details if requested (UC-6.1)
            if (filter.IncludeHealthDetails)
            {
                dto.HealthStatusId = orphan.HealthStatusId;
                dto.HealthStatusName = orphan.HealthStatus?.Name;
                dto.Disabilities = orphan.Disabilities;
                dto.ChronicDiseases = orphan.ChronicDiseases;
            }

            orphanDtos.Add(dto);
        }

        // Generate summary statistics (UC-6.1)
        var summary = new OrphanReportSummaryDto
        {
            FromDate = filter.FromDate,
            ToDate = filter.ToDate,
            TotalOrphans = orphanDtos.Count,
            SponsoredCount = orphanDtos.Count(o => o.SponsorshipStatus == "Sponsored"),
            UnsponsoredCount = orphanDtos.Count(o => o.SponsorshipStatus == "Unsponsored"),
            PendingCount = orphanDtos.Count(o => o.SponsorshipStatus == "Pending"),
            MaleCount = orphanDtos.Count(o => o.Gender == "Male"),
            FemaleCount = orphanDtos.Count(o => o.Gender == "Female"),
            Age0To5Count = orphanDtos.Count(o => o.Age.HasValue && o.Age >= 0 && o.Age <= 5),
            Age6To12Count = orphanDtos.Count(o => o.Age.HasValue && o.Age >= 6 && o.Age <= 12),
            Age13To18Count = orphanDtos.Count(o => o.Age.HasValue && o.Age >= 13 && o.Age <= 18),
            Age19PlusCount = orphanDtos.Count(o => o.Age.HasValue && o.Age > 18)
        };

        // Group by charity if requested (UC-6.1)
        if (filter.GroupByCharity)
        {
            var charityGroups = orphanDtos
                .Where(o => o.CharityId.HasValue)
                .GroupBy(o => o.CharityId!.Value)
                .Select(g => new CharityBreakdownDto
                {
                    CharityId = g.Key,
                    CharityName = g.First().CharityName ?? "Unknown",
                    OrphanCount = g.Count(),
                    SponsoredCount = g.Count(o => o.SponsorshipStatus == "Sponsored"),
                    UnsponsoredCount = g.Count(o => o.SponsorshipStatus == "Unsponsored")
                })
                .ToList();

            summary.CharityBreakdown = charityGroups;
        }

        // Create metadata
        var metadata = new OrphanReportMetadataDto
        {
            FromDate = filter.FromDate,
            ToDate = filter.ToDate,
            CharityId = filter.CharityId,
            RegionId = filter.RegionId,
            CenterId = filter.CenterId,
            SponsorshipStatus = filter.SponsorshipStatus,
            AgeFrom = filter.AgeFrom,
            AgeTo = filter.AgeTo,
            Gender = filter.Gender,
            IncludeFamilyDetails = filter.IncludeFamilyDetails,
            IncludeContactInformation = filter.IncludeContactInformation,
            IncludeEducationDetails = filter.IncludeEducationDetails,
            IncludeHealthDetails = filter.IncludeHealthDetails,
            GroupByCharity = filter.GroupByCharity
        };

        // ---- §14.U.11 detailed periodic-report extract (UC-ORR-11) ----
        // The general-purpose pull 9-12/9-13 specialise with a review-state predicate.
        // Rows are capped at 1000 per extract batch; ReportsTotalCount carries the uncapped count.
        // TODO EP-10: batch filter (no Batch entity in the Domain today — 17-2 deferred-rule precedent)
        // Review P1 2026-08-24: soft-deleted reports leaked into the extract — no global
        // filter exists, so every read filters explicitly (PeriodicOrphanReportService pattern).
        var reports = ApplyScope(
            _periodicReportRepository.AsQueryable().Where(r => !r.IsDeleted), scope);
        if (!string.IsNullOrWhiteSpace(filter.ReportNo))
        {
            var reportNo = filter.ReportNo.Trim();
            reports = reports.Where(r => r.ReportNo != null && r.ReportNo.Contains(reportNo));
        }
        if (filter.FromDate != default)
            reports = reports.Where(r => r.ReportDate >= filter.FromDate.Date);
        if (filter.ToDate != default)
            // Inclusive end date — the whole of "to" (9-1 Task 3 rule)
            reports = reports.Where(r => r.ReportDate < filter.ToDate.Date.AddDays(1));

        var reportsTotal = await reports.CountAsync();

        var detailRows = await reports
            // Review P20 2026-08-24: ties broke the cap deterministically — id is the tiebreak.
            .OrderByDescending(r => r.ReportDate)
            .ThenBy(r => r.Id)
            .Take(1000)
            .Select(r => new OrphanReportDetailRow
            {
                ReportId = r.Id,
                ReportNo = r.ReportNo,
                ReportDate = r.ReportDate,
                ReportPeriodFrom = r.ReportPeriodFrom,
                ReportPeriodTo = r.ReportPeriodTo,
                OrphanCode = r.Orphan.Code,
                OrphanName = r.Orphan.FullName,
                CharityName = r.Charity != null ? r.Charity.Name : null,
                ReviewStatus = r.ReviewStatus ?? "Pending",
                Reviewed = r.Reviewed,
                IsAccepted = r.IsAccepted,
                IsRefused = r.IsRefused,
                RefuseReason = r.RefuseReason,
                SchoolType = r.SchoolType,
                School = r.School,
                Faculty = r.Faculty,
                Specialization = r.Specialization,
                Grade = r.Grade,
                EducationDegree = r.EducationDegree,
                EducationalLevelId = r.EducationalLevelId,
                MedicalStatus = r.MedicalStatus,
                Disease = r.Disease,
                Disability = r.Disability,
                Married = r.Married,
                MarriageDate = r.MarriageDate,
                Dead = r.Dead,
                DeathDate = r.DeathDate
            })
            .ToListAsync();

        // Resolve the level names the projection cannot join inline (no nav on the report).
        var levelIds = detailRows.Where(x => x.EducationalLevelId.HasValue)
            .Select(x => x.EducationalLevelId!.Value)
            .Distinct()
            .ToList();
        if (levelIds.Count > 0)
        {
            var levelNames = await _educationLevelRepository.AsQueryable()
                .Where(l => levelIds.Contains(l.Id))
                .ToDictionaryAsync(l => l.Id, l => l.NameAr ?? l.NameEn);
            foreach (var row in detailRows.Where(x => x.EducationalLevelId.HasValue))
                if (levelNames.TryGetValue(row.EducationalLevelId!.Value, out var name))
                    row.EducationalLevelName = name;
        }

        return new OrphanReportResultDto
        {
            Metadata = metadata,
            Summary = summary,
            Orphans = orphanDtos,
            Reports = detailRows,
            ReportsTotalCount = reportsTotal,
            GeneratedOn = DateTime.UtcNow
        };
    }

    #endregion

    #region Report Period (UC-6.2)

    public async Task<bool> ValidateReportPeriodAsync(DateTime fromDate, DateTime toDate)
    {
        await Task.CompletedTask;
        return fromDate <= toDate;
    }

    #endregion

    #region Export Operations (UC-6.7)

    public async Task<byte[]> ExportReportAsync(OrphanReportFilterDto filter, OrphanReportExportDto exportOptions)
    {
        // Generate the report first
        var report = await GenerateReportAsync(filter);

        // TODO: Implement Excel/PDF export using ClosedXML or similar library
        // For now, return empty byte array
        await Task.CompletedTask;
        return Array.Empty<byte>();
    }

    #endregion

    #region Report History (UC-6.9)

    public async Task<(IEnumerable<OrphanReportHistoryDto> Items, int TotalCount)> GetReportHistoryAsync(int pageNumber = 1, int pageSize = 20)
    {
        // TODO: Implement report history tracking
        // For now, return empty list
        await Task.CompletedTask;
        return (Enumerable.Empty<OrphanReportHistoryDto>(), 0);
    }

    public async Task<OrphanReportHistoryDto?> GetReportHistoryEntryAsync(Guid reportId)
    {
        // TODO: Implement report history retrieval
        await Task.CompletedTask;
        return null;
    }

    public async Task DeleteReportHistoryEntryAsync(Guid reportId)
    {
        // TODO: Implement report history deletion
        await Task.CompletedTask;
    }

    #endregion

    #region Period Comparison (UC-6.10)

    public async Task<OrphanReportComparisonDto> ComparePeriodsAsync(Guid report1Id, Guid report2Id)
    {
        // TODO: Implement period comparison
        await Task.CompletedTask;
        return new OrphanReportComparisonDto();
    }

    #endregion

    #region Scheduling (UC-6.8)

    public async Task<Guid> ScheduleRecurringReportAsync(ScheduleRecurringReportDto dto)
    {
        // TODO: Implement recurring report scheduling using Hangfire or similar
        var scheduleId = Guid.NewGuid();
        await Task.CompletedTask;
        return scheduleId;
    }

    public async Task<(IEnumerable<ScheduledReportDto> Items, int TotalCount)> GetScheduledReportsAsync(int pageNumber = 1, int pageSize = 20)
    {
        // TODO: Implement scheduled reports retrieval
        await Task.CompletedTask;
        return (Enumerable.Empty<ScheduledReportDto>(), 0);
    }

    public async Task UpdateScheduledReportAsync(Guid scheduleId, ScheduleRecurringReportDto dto)
    {
        // TODO: Implement scheduled report update
        await Task.CompletedTask;
    }

    public async Task DeleteScheduledReportAsync(Guid scheduleId)
    {
        // TODO: Implement scheduled report deletion
        await Task.CompletedTask;
    }

    #endregion

    #region Statistics

    public async Task<OrphanStatisticsDto> GetOrphanStatisticsAsync(OrphanReportFilterDto filter)
    {
        // Caller scope (UC-ORR-10 AC 3): a charity claim pins the aggregation to that
        // charity and a forged request charityId is ignored; HQ may pass any explicit one.
        // Review P2/P3 2026-08-24: full ApplyCharityScopeAsync ladder — HQ gate, country pin,
        // fail-closed when the caller carries no scoping claim at all.
        var scope = await ResolveCallerScopeAsync(filter.CharityId);

        // ---- Scalar section (dashboard counts, additive — other screens read it) ----
        var query = _orphanRepository.AsQueryable().Where(o => !o.IsDeleted);

        query = ApplyScope(query, scope);
        if (filter.RegionId.HasValue)
            query = query.Where(o => o.Family != null && o.Family.RegionId == filter.RegionId.Value);
        if (filter.CenterId.HasValue)
            query = query.Where(o => o.Family != null && o.Family.CenterId == filter.CenterId.Value);

        var orphans = await query.ToListAsync();

        var statistics = new OrphanStatisticsDto
        {
            TotalOrphans = orphans.Count,
            SponsoredCount = orphans.Count(o => o.SponsorshipStatus == "Sponsored"),
            UnsponsoredCount = orphans.Count(o => o.SponsorshipStatus == "Unsponsored"),
            PendingCount = orphans.Count(o => o.SponsorshipStatus == "Pending"),
            MaleCount = orphans.Count(o => o.Gender == "Male"),
            FemaleCount = orphans.Count(o => o.Gender == "Female")
        };

        // ---- §14.S.3 grouped rows (UC-ORR-10): الحاله التعليميه × المرحله الدراسه ----
        // Review P1/P15 2026-08-24: soft-deleted reports leaked in, and rows counted REPORTS —
        // §14.S.3 counts orphans, so each orphan is classified once, by their LATEST report
        // (classification mirrors the 9-9 filter mapping: dropout > graduated > studying).
        var reports = ApplyScope(
            _periodicReportRepository.AsQueryable().Where(r => !r.IsDeleted), scope);

        var reportRows = await reports
            .Select(r => new
            {
                r.OrphanId,
                r.ReportDate,
                r.DropOut,
                r.HighestEducationalLevel,
                r.IsOrphanStudent,
                r.EducationalLevelId,
                Gender = r.Orphan.Gender
            })
            .ToListAsync();

        var grouped = reportRows
            .GroupBy(r => r.OrphanId)
            .Select(g => g.OrderByDescending(r => r.ReportDate).First())
            .GroupBy(r => new
            {
                EducationalStatus =
                    r.DropOut == true ? "dropout"
                    : !string.IsNullOrEmpty(r.HighestEducationalLevel) ? "graduated"
                    : r.IsOrphanStudent == true ? "studying"
                    : "unspecified",
                EducationalLevelId = r.EducationalLevelId ?? 0
            })
            .Select(g => new
            {
                g.Key.EducationalStatus,
                g.Key.EducationalLevelId,
                Total = g.Count(),
                // Gender is stored as the Arabic token (ذكر/أنثى); legacy rows may carry English
                Female = g.Count(x => x.Gender == "أنثى" || x.Gender == "Female"),
                Male = g.Count(x => x.Gender == "ذكر" || x.Gender == "Male")
            })
            .ToList();

        var levelIds = grouped.Where(g => g.EducationalLevelId != 0)
            .Select(g => g.EducationalLevelId)
            .Distinct()
            .ToList();
        var levelNames = levelIds.Count > 0
            ? await _educationLevelRepository.AsQueryable()
                .Where(l => levelIds.Contains(l.Id))
                .ToDictionaryAsync(l => l.Id, l => l.NameAr ?? l.NameEn)
            : new Dictionary<int, string>();

        statistics.Groups = grouped
            .Select(g => new OrphanReportGroupCountRow
            {
                EducationalStatus = g.EducationalStatus,
                EducationalLevelName = g.EducationalLevelId != 0
                    && levelNames.TryGetValue(g.EducationalLevelId, out var name)
                        ? name
                        : null,
                FemaleCount = g.Female,
                MaleCount = g.Male,
                TotalCount = g.Total
            })
            .OrderByDescending(g => g.TotalCount)
            .ToList();

        // ---- §14.U.15 numbers-in-period branch (UC-ORR-15) ----
        // The ReportNo values CREATED in the window (CreatedOn bounds, inclusive end date),
        // additive to the grouped branch above — the shared statistics contract.
        // TODO EP-10: payment-batch filter (no batch model yet; the window carries the semantic).
        // Review P16 2026-08-24: a numbers request without a full window was a silent no-op —
        // now an explicit contract error instead of an empty-looking answer.
        if (filter.IncludeReportNumbers && (filter.FromDate == default || filter.ToDate == default))
        {
            throw new ArgumentException(
                "A full date window (fromDate, toDate) is required when includeReportNumbers is set.");
        }

        if (filter.IncludeReportNumbers)
        {
            // Review P1 2026-08-24: soft-deleted reports leaked here too.
            var numbers = ApplyScope(
                _periodicReportRepository.AsQueryable().Where(r => !r.IsDeleted), scope);
            numbers = numbers.Where(r => r.CreatedOn >= filter.FromDate.Date
                && r.CreatedOn < filter.ToDate.Date.AddDays(1));

            statistics.ReportNumbersCount = await numbers.CountAsync();
            statistics.UnnumberedReportsCount = await numbers.CountAsync(r => r.ReportNo == null);

            statistics.ReportNumbers = await numbers
                // Review P20: id tiebreak keeps the 1000-cap deterministic.
                .OrderByDescending(r => r.CreatedOn)
                .ThenBy(r => r.Id)
                .Take(1000)
                .Select(r => new OrphanReportNumberRow
                {
                    ReportId = r.Id,
                    ReportNo = r.ReportNo,
                    OrphanCode = r.Orphan.Code,
                    OrphanName = r.Orphan.FullName,
                    ReportDate = r.ReportDate,
                    CreatedOn = r.CreatedOn,
                    ReviewStatus = r.ReviewStatus ?? "Pending"
                })
                .ToListAsync();
        }

        return statistics;
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Resolved caller scope (Review P2/P3 2026-08-24 — ReportService.ApplyCharityScopeAsync
    /// ladder): a charity caller is pinned to its own charity; an HQ caller may narrow to one
    /// explicit charity; a CountryId claim is pinned to that country's (non-deleted) charities;
    /// unconstrained HQ sees everything; anyone else fails closed.
    /// </summary>
    private sealed record CallerScope(Guid? PinnedCharityId, List<Guid>? CountryCharityIds);

    private async Task<CallerScope> ResolveCallerScopeAsync(Guid? requestedCharityId)
    {
        // Charity caller — pinned; the payload's CharityId is never trusted (AC 3).
        if (_currentUser.CharityId.HasValue)
        {
            return new CallerScope(_currentUser.CharityId.Value, null);
        }

        // HQ caller may narrow to one explicit charity.
        if (_currentUser.IsHeadOffice && requestedCharityId.HasValue)
        {
            return new CallerScope(requestedCharityId.Value, null);
        }

        // Country claim — additionally pinned to that country's charities.
        if (_currentUser.CountryId.HasValue)
        {
            var countryId = _currentUser.CountryId.Value;
            var countryCharityIds = await _charityRepository.AsQueryable()
                .Where(c => !c.IsDeleted && c.CountryId == countryId)
                .Select(c => c.Id)
                .ToListAsync();
            return new CallerScope(null, countryCharityIds);
        }

        if (_currentUser.IsHeadOffice)
        {
            return new CallerScope(null, null);
        }

        // Review P3: previously any claim-less caller fell through unscoped (fail-open).
        throw new InvalidOperationException(
            "Caller has no charity, country, or head-office scope; refusing unscoped query.");
    }

    private static IQueryable<Orphan> ApplyScope(IQueryable<Orphan> query, CallerScope scope)
    {
        if (scope.PinnedCharityId.HasValue)
        {
            var pinned = scope.PinnedCharityId.Value;
            return query.Where(o => o.FK_CharityId == pinned);
        }

        if (scope.CountryCharityIds != null)
        {
            var ids = scope.CountryCharityIds;
            return query.Where(o => o.FK_CharityId != null && ids.Contains(o.FK_CharityId.Value));
        }

        return query;
    }

    private static IQueryable<PeriodicOrphanReport> ApplyScope(
        IQueryable<PeriodicOrphanReport> query, CallerScope scope)
    {
        if (scope.PinnedCharityId.HasValue)
        {
            var pinned = scope.PinnedCharityId.Value;
            return query.Where(r => r.CharityId == pinned);
        }

        if (scope.CountryCharityIds != null)
        {
            var ids = scope.CountryCharityIds;
            return query.Where(r => r.CharityId != null && ids.Contains(r.CharityId.Value));
        }

        return query;
    }

    private int? CalculateAge(DateTime? birthDate)
    {
        if (!birthDate.HasValue) return null;

        var today = DateTime.UtcNow;
        var age = today.Year - birthDate.Value.Year;

        if (today < birthDate.Value.AddYears(age))
            age--;

        return age;
    }

    #endregion
}
