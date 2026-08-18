using IIROSA.Application.DTOs.OrphanReport;
using IIROSA.Application.Interfaces;
using IIROSA.Domain.Entities;
using IIROSA.Infrastructure.Data.Repository;
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
    private readonly ILogger<OrphanReportService> _logger;

    public OrphanReportService(
        IUnitOfWork unitOfWork,
        IRepository<Orphan> orphanRepository,
        IRepository<Charity> charityRepository,
        IRepository<Family> familyRepository,
        ILogger<OrphanReportService> logger)
    {
        _unitOfWork = unitOfWork;
        _orphanRepository = orphanRepository;
        _charityRepository = charityRepository;
        _familyRepository = familyRepository;
        _logger = logger;
    }

    #region Report Generation (UC-6.1)

    public async Task<OrphanReportResultDto> GenerateReportAsync(OrphanReportFilterDto filter)
    {
        var query = _orphanRepository.GetQueryable()
            .Include(o => o.Family)
                .ThenInclude(f => f!.Father)
            .Include(o => o.Family)
                .ThenInclude(f => f!.Mother)
            .Include(o => o.Family)
                .ThenInclude(f => f!.Provider)
            .Include(o => o.Family)
                .ThenInclude(f => f!.Region)
            .Include(o => o.Family)
                .ThenInclude(f => f!.Center)
            .Include(o => o.Sponsor)
            .Include(o => o.EducationLevel)
            .Include(o => o.HealthStatus)
            .Include(o => o.FK_CharityId.HasValue ? o.Charity : null)
            .Where(o => !o.IsDeleted);

        // Apply charity filter (UC-6.4)
        if (filter.CharityId.HasValue)
        {
            query = query.Where(o => o.FK_CharityId == filter.CharityId.Value);
        }

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
                CharityName = orphan.Charity?.Name,
                RegionId = orphan.Family?.RegionId,
                RegionName = orphan.Family?.Region?.Name,
                CenterId = orphan.Family?.CenterId,
                CenterName = orphan.Family?.Center?.Name,
                SponsorId = orphan.SponsorId,
                SponsorName = orphan.Sponsor?.Name,
                MonthlyAmount = orphan.MonthlyAmount,
                PhotoAttachmentId = orphan.PhotoAttachmentId
            };

            // Include Family Details if requested (UC-6.6)
            if (filter.IncludeFamilyDetails)
            {
                dto.FamilyAddress = orphan.Family?.Address;
                dto.FatherName = orphan.Family?.Father?.FullName;
                dto.MotherName = orphan.Family?.Mother?.FullName;
                dto.ProviderName = orphan.Family?.Provider?.Name;
                dto.FamilyPhone = orphan.Family?.Phone;
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
            GroupByCharity = filter.GroupByCharity,
            GeneratedOn = DateTime.UtcNow
        };

        return new OrphanReportResultDto
        {
            Metadata = metadata,
            Summary = summary,
            Orphans = orphanDtos,
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
        var query = _orphanRepository.GetQueryable().Where(o => !o.IsDeleted);

        // Apply filters
        if (filter.CharityId.HasValue)
            query = query.Where(o => o.FK_CharityId == filter.CharityId.Value);
        if (filter.RegionId.HasValue)
            query = query.Where(o => o.Family != null && o.Family.RegionId == filter.RegionId.Value);
        if (filter.CenterId.HasValue)
            query = query.Where(o => o.Family != null && o.Family.CenterId == filter.CenterId.Value);

        var orphans = await query.ToListAsync();

        return new OrphanStatisticsDto
        {
            TotalOrphans = orphans.Count,
            SponsoredCount = orphans.Count(o => o.SponsorshipStatus == "Sponsored"),
            UnsponsoredCount = orphans.Count(o => o.SponsorshipStatus == "Unsponsored"),
            PendingCount = orphans.Count(o => o.SponsorshipStatus == "Pending"),
            MaleCount = orphans.Count(o => o.Gender == "Male"),
            FemaleCount = orphans.Count(o => o.Gender == "Female")
        };
    }

    #endregion

    #region Helper Methods

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
