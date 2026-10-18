using Framework.Core.SharedServices.Services;
using IIROSA.Application.DTOs.Reports;
using IIROSA.Application.Interfaces;
using IIROSA.Domain.Contracts.Persistence;
using IIROSA.Domain.Entities;
using IIROSA.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Framework.Core.SharedServices;
using Framework.Identity.Data.Entities;
using Microsoft.AspNetCore.Identity;

namespace IIROSA.Application.Services;

/// <summary>
/// UC-RPT-32 (§23.U.32 صفحة ملخص الصرف) — the payment batch summary figures for the batch
/// view's totals band. Read-only: composes over <c>TableNoTracking</c> and never writes.
/// Registered by the assembly convention (<c>RegisterApplicationServices</c> scans classes
/// ending "Service") — no manual DI line.
/// </summary>
public class DashboardService : IDashboardService
{
    private readonly IOrphanRepository _orphanRepository;
    private readonly IRepository<OrphanPaymentItem> _paymentItemRepository;
    private readonly IRepository<Charity> _charityRepository;
    private readonly IRepository<OrphanPayment> _orphanPaymentRepository;
    private readonly ICurrentUserService _currentUser;
    private readonly ILogger<DashboardService> _logger;
    private readonly IRepository<Family> _familyRepository;
    private readonly IRepository<Mission> _missionRepository;
    private readonly ICommonsDbContext _commonsContext;
    private readonly UserManager<ApplicationUser> _userManager;

    public DashboardService(
        IOrphanRepository orphanRepository,
        IRepository<OrphanPaymentItem> paymentItemRepository,
        IRepository<Charity> charityRepository,
        IRepository<OrphanPayment> orphanPaymentRepository,
        IRepository<Family> familyRepository,
        IRepository<Mission> missionRepository,
        ICommonsDbContext commonsContext,
        UserManager<ApplicationUser> userManager,
        ICurrentUserService currentUser,
        ILogger<DashboardService> logger)
    {
        _orphanRepository = orphanRepository;
        _paymentItemRepository = paymentItemRepository;
        _charityRepository = charityRepository;
        _orphanPaymentRepository = orphanPaymentRepository;
        _familyRepository = familyRepository;
        _missionRepository = missionRepository;
        _commonsContext = commonsContext;
        _userManager = userManager;
        _currentUser = currentUser;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<DashboardStatisticsDto> GetStatisticsAsync(
        Guid? charityId = null,
        CancellationToken cancellationToken = default)
    {
        var charities = _charityRepository.TableNoTracking.Where(c => !c.IsDeleted);
        if (_currentUser.CharityId.HasValue)
            charities = charities.Where(c => c.Id == _currentUser.CharityId.Value);
        else if (!_currentUser.IsHeadOffice)
            throw new UnauthorizedAccessException("Caller has no charity or head-office scope.");
        else if (charityId.HasValue)
            charities = charities.Where(c => c.Id == charityId.Value);

        var charityRows = await charities.Select(c => new { c.Id, c.Name })
            .ToListAsync(cancellationToken);
        var charityIds = charityRows.Select(c => c.Id).ToList();
        var families = _familyRepository.TableNoTracking.Where(f => !f.IsDeleted
            && ((f.CharityId.HasValue && charityIds.Contains(f.CharityId.Value))
                || (f.FK_CharityId.HasValue && charityIds.Contains(f.FK_CharityId.Value))));
        var scopedFamilyIds = families.Select(f => f.Id);
        var orphans = _orphanRepository.TableNoTracking.Where(o => !o.IsDeleted
            && ((o.FK_CharityId.HasValue && charityIds.Contains(o.FK_CharityId.Value))
                || (o.FamilyId.HasValue && scopedFamilyIds.Contains(o.FamilyId.Value))));
        var missions = _missionRepository.TableNoTracking.Where(m => !m.IsDeleted
            && m.FK_CharityId.HasValue && charityIds.Contains(m.FK_CharityId.Value));

        var today = DateTime.UtcNow.Date;
        var firstMonth = new DateTime(today.Year, today.Month, 1).AddMonths(-11);
        var months = Enumerable.Range(0, 12).Select(i => firstMonth.AddMonths(i)).ToList();
        var monthEnd = months[^1].AddMonths(1);
        var charityDates = await charities.Where(c => c.CreatedOn >= firstMonth && c.CreatedOn < monthEnd)
            .Select(c => c.CreatedOn).ToListAsync(cancellationToken);
        var familyDates = await families.Where(f => f.CreatedOn >= firstMonth && f.CreatedOn < monthEnd)
            .Select(f => f.CreatedOn).ToListAsync(cancellationToken);
        var orphanDates = await orphans.Where(o => o.CreatedOn >= firstMonth && o.CreatedOn < monthEnd)
            .Select(o => o.CreatedOn).ToListAsync(cancellationToken);

        var result = new DashboardStatisticsDto
        {
            TotalCharities = charityRows.Count,
            TotalFamilies = await families.CountAsync(cancellationToken),
            TotalOrphans = await orphans.CountAsync(cancellationToken),
            ActiveMissions = await missions.CountAsync(m => !m.IsMissionCompleted, cancellationToken),
            Months = months.Select(m => m.ToString("MMM yyyy")).ToList(),
            CharitiesCreatedByMonth = months.Select(m => charityDates.Count(d => d.Year == m.Year && d.Month == m.Month)).ToList(),
            FamiliesCreatedByMonth = months.Select(m => familyDates.Count(d => d.Year == m.Year && d.Month == m.Month)).ToList(),
            OrphansCreatedByMonth = months.Select(m => orphanDates.Count(d => d.Year == m.Year && d.Month == m.Month)).ToList()
        };

        var familyCounts = await families.GroupBy(f =>
                f.CharityId.HasValue && charityIds.Contains(f.CharityId.Value) ? f.CharityId : f.FK_CharityId)
            .Select(g => new { CharityId = g.Key, Count = g.Count() }).ToListAsync(cancellationToken);
        var orphanCounts = await orphans.Select(o => new
            {
                CharityId = o.FK_CharityId.HasValue && charityIds.Contains(o.FK_CharityId.Value)
                    ? o.FK_CharityId
                    : o.Family != null && o.Family.CharityId.HasValue && charityIds.Contains(o.Family.CharityId.Value)
                        ? o.Family.CharityId
                        : o.Family != null ? o.Family.FK_CharityId : null
            })
            .GroupBy(o => o.CharityId)
            .Select(g => new { CharityId = g.Key, Count = g.Count() }).ToListAsync(cancellationToken);
        var familyById = familyCounts.Where(g => g.CharityId.HasValue).ToDictionary(g => g.CharityId!.Value, g => g.Count);
        var orphanById = orphanCounts.Where(g => g.CharityId.HasValue).ToDictionary(g => g.CharityId!.Value, g => g.Count);
        var charityTotals = charityRows.Select(c => new
        {
            c.Id, c.Name,
            Families = familyById.GetValueOrDefault(c.Id),
            Orphans = orphanById.GetValueOrDefault(c.Id)
        }).ToList();
        var topFamilyCharities = charityTotals.OrderByDescending(c => c.Families).Take(10).ToList();
        result.FamilyCharityNames = topFamilyCharities.Select(c => c.Name).ToList();
        result.FamiliesByCharity = topFamilyCharities.Select(c => c.Families).ToList();
        var topOrphanCharities = charityTotals.OrderByDescending(c => c.Orphans).Take(10).ToList();
        result.OrphanCharityNames = topOrphanCharities.Select(c => c.Name).ToList();
        result.OrphansByCharity = topOrphanCharities.Select(c => c.Orphans).ToList();

        var scopedCharityIds = charityIds;
        var activityQuery = _commonsContext.AuditLogs.AsNoTracking()
            .Where(a => a.CharityId.HasValue && scopedCharityIds.Contains(a.CharityId.Value));
        var charityActivity = await activityQuery.Where(a => a.UserId != null)
            .GroupBy(a => a.CharityId).Select(g => new { CharityId = g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken);
        var activityByCharity = charityActivity.Where(a => a.CharityId.HasValue)
            .ToDictionary(a => a.CharityId!.Value, a => a.Count);
        var activeOrganizations = charityRows.Select(c => new { c.Name, Count = activityByCharity.GetValueOrDefault(c.Id) })
            .OrderByDescending(c => c.Count).Take(10).ToList();
        result.ActiveCharityNames = activeOrganizations.Select(c => c.Name).ToList();
        result.ActiveCharityInteractions = activeOrganizations.Select(c => c.Count).ToList();

        var associationUsers = await _userManager.GetUsersInRoleAsync("Charity");
        var associationUserIds = associationUsers.Select(user => user.Id).ToList();
        var userActivity = await _commonsContext.AuditLogs.AsNoTracking()
            .Where(a => a.UserId.HasValue && !associationUserIds.Contains(a.UserId.Value))
            .GroupBy(a => a.UserName).Select(g => new { UserName = g.Key, Count = g.Count() })
            .OrderByDescending(a => a.Count).Take(5).ToListAsync(cancellationToken);
        result.ActiveUserNames = userActivity.Select(u => u.UserName).ToList();
        result.UserInteractions = userActivity.Select(u => u.Count).ToList();

        var genders = await orphans.GroupBy(o => o.Gender)
            .Select(g => new { Gender = g.Key, Count = g.Count() }).ToListAsync(cancellationToken);
        result.MaleOrphans = genders.Where(g => g.Gender == "Male" || g.Gender == "ذكر").Sum(g => g.Count);
        result.FemaleOrphans = genders.Where(g => g.Gender == "Female" || g.Gender == "أنثى").Sum(g => g.Count);

        var paymentRows = await _paymentItemRepository.TableNoTracking
            .Where(i => !i.IsDeleted && i.OrphanPayment != null && !i.OrphanPayment.IsDeleted
                && orphans.Select(o => o.Id).Contains(i.OrphanId))
            .Select(i => new
            {
                AllocationDate = i.OrphanPayment!.PaymentDate ?? i.OrphanPayment.GroupDate,
                ReceivedDate = i.ReceivedOn,
                i.Amount,
                i.IsGotIt,
                i.IsStopped
            })
            .ToListAsync(cancellationToken);
        result.AllocatedPaymentsByMonth = months.Select(m => paymentRows
            .Where(p => p.AllocationDate.Year == m.Year && p.AllocationDate.Month == m.Month)
            .Sum(p => p.Amount ?? 0m)).ToList();
        result.ReceivedPaymentsByMonth = months.Select(m => paymentRows
            .Where(p => p.IsGotIt && (p.ReceivedDate ?? p.AllocationDate).Year == m.Year
                && (p.ReceivedDate ?? p.AllocationDate).Month == m.Month)
            .Sum(p => p.Amount ?? 0m)).ToList();
        result.PaymentStatusLabels = new List<string> { "Received", "Pending", "Stopped" };
        result.PaymentStatusValues = new List<int>
        {
            paymentRows.Count(p => p.IsGotIt),
            paymentRows.Count(p => !p.IsGotIt && !p.IsStopped),
            paymentRows.Count(p => p.IsStopped)
        };

        var yearStart = new DateTime(today.Year, 1, 1);
        var missionRows = await missions.Where(m => m.MissionDate >= yearStart && m.MissionDate < yearStart.AddYears(1))
            .Select(m => new { m.MissionDate, m.IsMissionCompleted }).ToListAsync(cancellationToken);
        result.MissionQuarterLabels = new List<string> { "Q1", "Q2", "Q3", "Q4" };
        result.CompletedMissionsByQuarter = Enumerable.Range(1, 4).Select(q => missionRows
            .Count(m => ((m.MissionDate.Month - 1) / 3) + 1 == q && m.IsMissionCompleted)).ToList();
        result.PendingMissionsByQuarter = Enumerable.Range(1, 4).Select(q => missionRows
            .Count(m => ((m.MissionDate.Month - 1) / 3) + 1 == q && !m.IsMissionCompleted)).ToList();
        return result;
    }

    /// <inheritdoc />
    public async Task<PaymentSummaryDto> GetPaymentSummaryAsync(
        Guid paymentId,
        Guid? charityId = null,
        CancellationToken cancellationToken = default)
    {
        // Seeded with the requested id — every figure defaults to 0 (AC 4: absent data
        // renders zeros, never a 500, never a null figure).
        var summary = new PaymentSummaryDto { PaymentId = paymentId };

        // Batch resolution mirrors 18-29/30/31: the seed group names the BatchNo, then all
        // live groups sharing it aggregate together (GroupDate order).
        var seedGroup = await _orphanPaymentRepository.TableNoTracking
            .Where(g => !g.IsDeleted && g.Id == paymentId)
            .Select(g => new { g.BatchNo, g.PaymentPeriodFrom, g.PaymentPeriodTo, g.Currency })
            .FirstOrDefaultAsync(cancellationToken);
        if (seedGroup is null || string.IsNullOrWhiteSpace(seedGroup.BatchNo))
        {
            summary.Message = "Payment batch not found";
            _logger.LogInformation(
                "Payment summary (UC-RPT-32): payment {PaymentId} has no live batch; zeros returned; caller {Caller}",
                paymentId, _currentUser.UserId?.ToString() ?? "anonymous");
            return summary;
        }

        var batchNo = seedGroup.BatchNo;
        summary.BatchNo = batchNo;
        var batchGroups = await _orphanPaymentRepository.TableNoTracking
            .Where(g => !g.IsDeleted && g.BatchNo == batchNo)
            .OrderBy(g => g.GroupDate)
            .Select(g => new { g.Id, g.GroupDate, g.PaymentPeriodFrom, g.PaymentPeriodTo, g.Currency })
            .ToListAsync(cancellationToken);
        // Review P12 2026-08-26: the group set is read twice no-tracking — a soft-delete between
        // the reads can empty the list; guard the [0] indexer instead of throwing.
        if (batchGroups.Count == 0)
        {
            summary.Message = "Payment batch not found";
            return summary;
        }
        var first = batchGroups[0];
        var groupIds = batchGroups.Select(g => g.Id).ToList();
        summary.BatchDate = first.GroupDate;
        summary.PeriodFrom = first.PaymentPeriodFrom;
        summary.PeriodTo = first.PaymentPeriodTo;
        summary.Currency = first.Currency;

        // The 18-22/24 charity-rooted ladder, verbatim: claim pin → HQ narrow → country pin.
        var orphanQuery = _orphanRepository.TableNoTracking
            .Where(o => !o.IsDeleted && o.FK_CharityId != null);
        Guid? effectiveCharityId = null;
        if (_currentUser.CharityId.HasValue)
        {
            var pinned = _currentUser.CharityId.Value;
            effectiveCharityId = pinned;
            orphanQuery = orphanQuery.Where(o => o.FK_CharityId == pinned);
        }
        else if (_currentUser.IsHeadOffice && charityId.HasValue)
        {
            var narrowed = charityId.Value;
            effectiveCharityId = narrowed;
            orphanQuery = orphanQuery.Where(o => o.FK_CharityId == narrowed);
        }
        if (_currentUser.CountryId.HasValue)
        {
            var countryId = _currentUser.CountryId.Value;
            var countryCharityIds = _charityRepository.TableNoTracking
                .Where(c => !c.IsDeleted && c.CountryId == countryId)
                .Select(c => c.Id);
            orphanQuery = orphanQuery.Where(o => countryCharityIds.Contains(o.FK_CharityId!.Value));
        }
        else
        {
            // Review P1 2026-08-26: fail closed — a claim-less non-HQ token previously fell
            // through this ladder UNSCOPED and read every charity's totals.
            FailClosedWhenUnscoped();
        }
        var scopedOrphanIds = orphanQuery.Select(o => o.Id);

        if (effectiveCharityId.HasValue)
        {
            summary.CharityName = await _charityRepository.TableNoTracking
                .Where(c => !c.IsDeleted && c.Id == effectiveCharityId.Value)
                .Select(c => c.Name)
                .FirstOrDefaultAsync(cancellationToken);
        }

        // Review P10 2026-08-26: a nested Distinct().Count() inside the GroupBy projection does
        // not translate on SQL Server (the endpoint 500s at runtime) — the distinct-orphan figure
        // moves to its own query; the grouped read keeps only translatable sum/count forms.
        var orphanCount = await _paymentItemRepository.TableNoTracking
            .Where(i => !i.IsDeleted
                && groupIds.Contains(i.OrphanPaymentId)
                && scopedOrphanIds.Contains(i.OrphanId))
            .Select(i => i.OrphanId)
            .Distinct()
            .CountAsync(cancellationToken);

        // One grouped read carries every figure — the §15.U.18–20 dataset definition with no
        // divergent predicates: received = IsGotIt, not-received = !IsGotIt (the stopped slice
        // rides it too), stopped = IsStopped, cheques = a ChiqueNum recorded (10-12).
        var stats = await _paymentItemRepository.TableNoTracking
            .Where(i => !i.IsDeleted
                && groupIds.Contains(i.OrphanPaymentId)
                && scopedOrphanIds.Contains(i.OrphanId))
            .GroupBy(_ => 1)
            .Select(g => new
            {
                TotalAmount = g.Sum(i => i.Amount ?? 0m),
                ReceivedCount = g.Count(i => i.IsGotIt),
                ReceivedAmount = g.Sum(i => i.IsGotIt ? (i.Amount ?? 0m) : 0m),
                NotReceivedCount = g.Count(i => !i.IsGotIt),
                NotReceivedAmount = g.Sum(i => !i.IsGotIt ? (i.Amount ?? 0m) : 0m),
                StoppedCount = g.Count(i => i.IsStopped),
                StoppedAmount = g.Sum(i => i.IsStopped ? (i.Amount ?? 0m) : 0m),
                ChequeCount = g.Count(i => !string.IsNullOrEmpty(i.ChiqueNum)),
                ChequeTotal = g.Sum(i => !string.IsNullOrEmpty(i.ChiqueNum)
                    ? (i.Amount ?? 0m)
                    : 0m)
            })
            .FirstOrDefaultAsync(cancellationToken);

        // AC 4 — an empty batch keeps the zeroed figures and says so; it is not an error.
        if (stats is null)
        {
            summary.OrphanCount = orphanCount;
            summary.Message = "No payment rows for this batch in the requested scope";
            return summary;
        }

        summary.OrphanCount = orphanCount;
        summary.TotalAmount = stats.TotalAmount;
        summary.ReceivedCount = stats.ReceivedCount;
        summary.ReceivedAmount = stats.ReceivedAmount;
        summary.NotReceivedCount = stats.NotReceivedCount;
        summary.NotReceivedAmount = stats.NotReceivedAmount;
        summary.StoppedCount = stats.StoppedCount;
        summary.StoppedAmount = stats.StoppedAmount;
        summary.ChequeCount = stats.ChequeCount;
        summary.ChequeTotal = stats.ChequeTotal;

        return summary;
    }

    /// <summary>
    /// Review P1 2026-08-26: fail-closed tail of the inlined scope ladder above (mirrors
    /// ReportService.FailClosedWhenUnscoped). A caller with no charity claim, no country claim
    /// and no head-office marker must not run unscoped; HQ with no claims is the one legitimate
    /// unscoped case. Throws <see cref="UnauthorizedAccessException"/> so the controller maps
    /// it to 403, not 500.
    /// </summary>
    private void FailClosedWhenUnscoped()
    {
        if (_currentUser.IsHeadOffice || _currentUser.CharityId.HasValue || _currentUser.CountryId.HasValue)
        {
            return;
        }

        throw new UnauthorizedAccessException(
            "Caller has no charity, country, or head-office scope; refusing unscoped query.");
    }
}
