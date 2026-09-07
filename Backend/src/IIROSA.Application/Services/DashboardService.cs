using Framework.Core.SharedServices.Services;
using IIROSA.Application.DTOs.Reports;
using IIROSA.Application.Interfaces;
using IIROSA.Domain.Contracts.Persistence;
using IIROSA.Domain.Entities;
using IIROSA.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

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

    public DashboardService(
        IOrphanRepository orphanRepository,
        IRepository<OrphanPaymentItem> paymentItemRepository,
        IRepository<Charity> charityRepository,
        IRepository<OrphanPayment> orphanPaymentRepository,
        ICurrentUserService currentUser,
        ILogger<DashboardService> logger)
    {
        _orphanRepository = orphanRepository;
        _paymentItemRepository = paymentItemRepository;
        _charityRepository = charityRepository;
        _orphanPaymentRepository = orphanPaymentRepository;
        _currentUser = currentUser;
        _logger = logger;
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
