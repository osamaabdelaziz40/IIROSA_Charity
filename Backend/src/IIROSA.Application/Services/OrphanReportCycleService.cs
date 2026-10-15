using AutoMapper;
using FluentValidation;
using IIROSA.Application.DTOs.OrphanReportCycles;
using IIROSA.Application.Exceptions;
using IIROSA.Application.Interfaces;
using IIROSA.Domain.Contracts.Persistence;
using IIROSA.Domain.Entities;
using IIROSA.Domain.Enums;
using IIROSA.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace IIROSA.Application.Services;

/// <summary>
/// Orphan Report Cycle service (UC-ORC-01…09, docs/Modules/26-UC-ORC).
///
/// Cycles are HQ-defined and platform-wide; submissions and the coverage report are
/// scoped server-side to the caller's charity (a head-office caller may filter by an
/// explicit charity id — a charity caller never widens). Status is derived from the
/// dates everywhere (BR-ORC-02): create/update recompute it, RefreshDueStatusesAsync is
/// the only writer of stored transitions, and every consumer that gates on a window
/// (submission, charity edit) re-derives at read time rather than trusting the stored
/// badge — BR-ORC-11's server-side write-time check lives in PeriodicOrphanReportService.
/// Only IUnitOfWork saves.
/// </summary>
public class OrphanReportCycleService : IOrphanReportCycleService
{
    /// <summary>Upper bound for the page size a client can request in one call.</summary>
    private const int MaxPageSize = 200;

    private readonly IOrphanReportCycleRepository _cycleRepository;
    private readonly IRepository<PeriodicOrphanReport> _reportRepository;
    private readonly IRepository<Orphan> _orphanRepository;
    private readonly IRepository<Charity> _charityRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;
    private readonly IMapper _mapper;
    private readonly IValidator<CreateOrphanReportCycleDto> _createValidator;
    private readonly IValidator<UpdateOrphanReportCycleDto> _updateValidator;
    private readonly ILogger<OrphanReportCycleService> _logger;

    public OrphanReportCycleService(
        IOrphanReportCycleRepository cycleRepository,
        IRepository<PeriodicOrphanReport> reportRepository,
        IRepository<Orphan> orphanRepository,
        IRepository<Charity> charityRepository,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUser,
        IMapper mapper,
        IValidator<CreateOrphanReportCycleDto> createValidator,
        IValidator<UpdateOrphanReportCycleDto> updateValidator,
        ILogger<OrphanReportCycleService> logger)
    {
        _cycleRepository = cycleRepository;
        _reportRepository = reportRepository;
        _orphanRepository = orphanRepository;
        _charityRepository = charityRepository;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _mapper = mapper;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
        _logger = logger;
    }

    /// <summary>
    /// The single status derivation (BR-ORC-02) — shared by create/update, the background
    /// worker, the lazy re-check and the submission/edit window guards, so every consumer
    /// computes "where is this cycle now" from the same rule: end passed ⇒ Completed,
    /// start arrived ⇒ InProgress, otherwise NotStarted.
    /// </summary>
    public static OrphanReportCycleStatus DeriveStatus(DateTime startDate, DateTime endDate, DateTime utcNow)
        => endDate < utcNow
            ? OrphanReportCycleStatus.Completed
            : startDate <= utcNow ? OrphanReportCycleStatus.InProgress : OrphanReportCycleStatus.NotStarted;

    // ========== Reads (UC-ORC-01, UC-ORC-03) ==========

    /// <summary>The HQ register — paged, newest first, with report counts and progress.</summary>
    public async Task<OrphanReportCyclePagedResult> GetFilteredAsync(OrphanReportCycleFilterDto filter)
    {
        filter ??= new OrphanReportCycleFilterDto();
        ClampPaging(filter);

        var search = filter.Search?.Trim();
        var expression = BuildSearchExpression(search, filter.Status);

        var (items, totalCount) = await _cycleRepository.GetPagedAsync(
            expression,
            q => q.OrderByDescending(c => c.CreatedOn),
            filter.Page,
            filter.PageSize);

        var dtoItems = await DecorateWithCountsAsync(items);

        return new OrphanReportCyclePagedResult
        {
            Items = dtoItems,
            TotalCount = totalCount,
            Page = filter.Page,
            PageSize = filter.PageSize
        };
    }

    /// <summary>Detail read for the edit screen.</summary>
    public async Task<OrphanReportCycleDto?> GetByIdAsync(Guid id)
    {
        var cycle = await _cycleRepository.TableNoTracking
            .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);

        return cycle == null ? null : (await DecorateWithCountsAsync(new[] { cycle })).First();
    }

    // ========== Writes (UC-ORC-02…04) ==========

    /// <summary>
    /// Create (UC-ORC-02): status computed from the dates — a period that already runs is
    /// born InProgress, not an error. Overlapping cycles are legal (BR-ORC-03).
    /// </summary>
    public async Task<OrphanReportCycleDto> CreateAsync(CreateOrphanReportCycleDto dto)
    {
        await _createValidator.ValidateAndThrowAsync(dto);

        var cycle = _mapper.Map<OrphanReportCycle>(dto);
        cycle.Id = Guid.NewGuid();

        ApplyDerivedStatus(cycle, DateTime.UtcNow);

        await _cycleRepository.InsertAsync(cycle);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Orphan report cycle {CycleId} («{Title}») created by {UserId}",
            cycle.Id, cycle.TitleAr, _currentUser.UserId);

        return (await GetByIdAsync(cycle.Id))!;
    }

    /// <summary>
    /// Update (UC-ORC-03): master data and dates while not completed. BR-ORC-08 guards:
    /// completed is read-only; a running cycle's EndDate may be extended but never pulled
    /// before today (it would strand in-window submissions). Status recomputes on save.
    /// </summary>
    public async Task<OrphanReportCycleDto> UpdateAsync(UpdateOrphanReportCycleDto dto)
    {
        await _updateValidator.ValidateAndThrowAsync(dto);

        var cycle = await _cycleRepository.Table
            .FirstOrDefaultAsync(c => c.Id == dto.Id && !c.IsDeleted)
            ?? throw new NotFoundException(nameof(OrphanReportCycle), dto.Id);

        var utcNow = DateTime.UtcNow;

        if (cycle.Status == OrphanReportCycleStatus.Completed)
            throw new BusinessException(
                "This cycle has completed and is read-only; completed cycles are the audit record of a reporting round.");

        // BR-ORC-08: shortening a running cycle's end into the past is refused — a report
        // submitted inside the old window would suddenly sit outside the cycle.
        if (dto.EndDate < utcNow && dto.EndDate != cycle.EndDate)
            throw new BusinessException(
                "The end date cannot be moved into the past while the cycle has not completed; extend it instead.");

        cycle.TitleAr = dto.TitleAr.Trim();
        cycle.TitleEn = string.IsNullOrWhiteSpace(dto.TitleEn) ? null : dto.TitleEn.Trim();
        cycle.DescriptionAr = string.IsNullOrWhiteSpace(dto.DescriptionAr) ? null : dto.DescriptionAr.Trim();
        cycle.DescriptionEn = string.IsNullOrWhiteSpace(dto.DescriptionEn) ? null : dto.DescriptionEn.Trim();
        cycle.StartDate = dto.StartDate;
        cycle.EndDate = dto.EndDate;

        ApplyDerivedStatus(cycle, utcNow);

        await _cycleRepository.UpdateAsync(cycle);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Orphan report cycle {CycleId} updated by {UserId}", cycle.Id, _currentUser.UserId);

        return (await GetByIdAsync(cycle.Id))!;
    }

    /// <summary>
    /// Soft-delete (UC-ORC-04): only a NotStarted cycle with zero bound reports (BR-ORC-07).
    /// </summary>
    public async Task DeleteAsync(Guid id)
    {
        var cycle = await _cycleRepository.Table
            .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted)
            ?? throw new NotFoundException(nameof(OrphanReportCycle), id);

        if (cycle.Status != OrphanReportCycleStatus.NotStarted)
            throw new BusinessException(
                "Only a cycle that has not started can be deleted; in-progress and completed cycles are kept as the record of the round.");

        var boundReports = await _reportRepository.TableNoTracking
            .AnyAsync(r => r.CycleId == id && !r.IsDeleted);

        if (boundReports)
            throw new BusinessException(
                "This cycle already has periodic reports bound to it and cannot be deleted.");

        await _cycleRepository.DeleteAsync(cycle);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Orphan report cycle {CycleId} deleted by {UserId}", cycle.Id, _currentUser.UserId);
    }

    // ========== Lifecycle (UC-ORC-05) ==========

    /// <summary>
    /// Advance every cycle whose stored status no longer matches its dates. The background
    /// worker calls this on its interval; the chooser and coverage reads call it lazily so
    /// a delayed tick cannot serve a stale window. Cheap when in sync — the due scan
    /// returns nothing and no save happens.
    /// </summary>
    public async Task<int> RefreshDueStatusesAsync(CancellationToken cancellationToken = default)
    {
        var utcNow = DateTime.UtcNow;
        var due = await _cycleRepository.GetDueForStatusChangeAsync(utcNow);
        if (due.Count == 0)
            return 0;

        var transitions = 0;
        foreach (var cycle in due)
        {
            var derived = DeriveStatus(cycle.StartDate, cycle.EndDate, utcNow);
            if (derived == cycle.Status)
                continue;

            cycle.Status = derived;
            cycle.StatusChangedOn = utcNow;
            transitions++;
            _logger.LogInformation(
                "Orphan report cycle {CycleId} («{Title}») advanced to {Status} by the status service",
                cycle.Id, cycle.TitleAr, derived);
        }

        if (transitions > 0)
            await _unitOfWork.SaveChangesAsync();

        return transitions;
    }

    // ========== Selector (UC-ORC-06/09 picker read) ==========

    /// <summary>
    /// Every non-deleted cycle, newest first, status derived on read. Serves the
    /// report-entry button gating and the coverage screen's cycle dropdown — callers
    /// on charity screens, so this read is deliberately open and write-free (the
    /// lazy RefreshDueStatusesAsync belongs to the per-orphan chooser, a smaller set).
    /// </summary>
    public async Task<List<OrphanReportCycleSelectorDto>> GetSelectorAsync()
    {
        var utcNow = DateTime.UtcNow;

        return await _cycleRepository.TableNoTracking
            .OrderByDescending(c => c.StartDate)
            .Select(c => new OrphanReportCycleSelectorDto
            {
                Id = c.Id,
                TitleAr = c.TitleAr,
                TitleEn = c.TitleEn,
                StartDate = c.StartDate,
                EndDate = c.EndDate,
                Status = c.EndDate < utcNow
                    ? OrphanReportCycleStatus.Completed
                    : c.StartDate <= utcNow
                        ? OrphanReportCycleStatus.InProgress
                        : OrphanReportCycleStatus.NotStarted
            })
            .ToListAsync();
    }

    // ========== Chooser (UC-ORC-06) ==========

    /// <summary>
    /// The chooser payload: every live In Progress cycle with this orphan's submission
    /// state inside it. Refreshes due transitions first (the lazy half of UC-ORC-05).
    /// </summary>
    public async Task<List<OrphanCycleReportStatusDto>> GetOrphanCycleReportStatusAsync(Guid orphanId)
    {
        await RefreshDueStatusesAsync();

        var reports = await _reportRepository.TableNoTracking
            .Where(r => r.OrphanId == orphanId && !r.IsDeleted && r.CycleId != null)
            .Select(r => new { r.CycleId, r.Id, r.ReportDate, r.IsAccepted, r.IsRefused })
            .ToListAsync();

        var activeCycles = await _cycleRepository.GetActiveAsync();

        return activeCycles.Select(c =>
        {
            var report = reports.FirstOrDefault(r => r.CycleId == c.Id);
            return new OrphanCycleReportStatusDto
            {
                CycleId = c.Id,
                TitleAr = c.TitleAr,
                TitleEn = c.TitleEn,
                StartDate = c.StartDate,
                EndDate = c.EndDate,
                Submitted = report != null,
                ReportId = report?.Id,
                ReportDate = report?.ReportDate,
                ReviewState = report == null ? null : ReviewStateOf(report.IsAccepted, report.IsRefused)
            };
        }).ToList();
    }

    // ========== Coverage (UC-ORC-09) ==========

    /// <summary>
    /// The coverage split: the coded-orphan population of the caller's scope, each row
    /// flagged submitted (with report date and review state) or not. Charity scope comes
    /// from the token — an explicit filter charity is honored for head office only.
    /// </summary>
    public async Task<CycleReportCoverageResultDto> GetCoverageAsync(CycleReportCoverageFilterDto filter)
    {
        filter ??= new CycleReportCoverageFilterDto();
        ClampPaging(filter);

        var cycle = await _cycleRepository.TableNoTracking
            .FirstOrDefaultAsync(c => c.Id == filter.CycleId && !c.IsDeleted)
            ?? throw new NotFoundException(nameof(OrphanReportCycle), filter.CycleId);

        // Tenancy pin: the caller's charity unless head office narrows explicitly.
        Guid? charityScope = _currentUser.IsHeadOffice ? filter.CharityId : _currentUser.CharityId;

        var orphans = _orphanRepository.TableNoTracking
            .Where(o => !o.IsDeleted && o.Code != string.Empty);
        if (charityScope.HasValue)
            orphans = orphans.Where(o => o.FK_CharityId == charityScope.Value);

        var search = filter.Search?.Trim();
        if (!string.IsNullOrWhiteSpace(search))
            orphans = orphans.Where(o => o.Code.Contains(search) || o.FullName.Contains(search));

        var cycleReports = _reportRepository.TableNoTracking
            .Where(r => r.CycleId == filter.CycleId && !r.IsDeleted);

        // Left join: every coded orphan of the scope once, with its cycle report when
        // one exists. GroupJoin keeps the not-submitted half — the chase list.
        var query = from o in orphans
                    join r in cycleReports on o.Id equals r.OrphanId into reports
                    from r in reports.DefaultIfEmpty()
                    select new CycleReportCoverageRowDto
                    {
                        OrphanId = o.Id,
                        OrphanCode = o.Code,
                        OrphanName = o.FullName,
                        CharityName = o.FK_CharityId != null
                            ? _charityRepository.TableNoTracking
                                .Where(c => c.Id == o.FK_CharityId)
                                .Select(c => c.Name)
                                .FirstOrDefault()
                            : null,
                        HasReport = r != null,
                        ReportDate = r != null ? r.ReportDate : null,
                        ReviewState = r != null ? ReviewStateOf(r.IsAccepted, r.IsRefused) : null
                    };

        // The tab filter — one category pages alone, null/anything else interleaves both.
        var category = filter.Category?.Trim();
        if (string.Equals(category, "Submitted", StringComparison.OrdinalIgnoreCase))
            query = query.Where(row => row.HasReport);
        else if (string.Equals(category, "NotSubmitted", StringComparison.OrdinalIgnoreCase))
            query = query.Where(row => !row.HasReport);

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(row => row.HasReport)
            .ThenBy(row => row.OrphanCode)
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync();

        // The two tab counts cover the unfiltered scope, whatever tab is paged.
        var scopeOrphans = orphans.Select(o => o.Id);
        var submittedCount = await cycleReports.CountAsync(r => scopeOrphans.Contains(r.OrphanId));
        var scopeCount = await orphans.CountAsync();

        return new CycleReportCoverageResultDto
        {
            CycleId = cycle.Id,
            CycleTitle = cycle.TitleAr,
            Items = items,
            TotalCount = totalCount,
            SubmittedCount = submittedCount,
            NotSubmittedCount = scopeCount - submittedCount,
            Page = filter.Page,
            PageSize = filter.PageSize
        };
    }

    // ========== Helpers ==========

    /// <summary>Stamp the derived status, touching StatusChangedOn only on an actual change.</summary>
    private static void ApplyDerivedStatus(OrphanReportCycle cycle, DateTime utcNow)
    {
        var derived = DeriveStatus(cycle.StartDate, cycle.EndDate, utcNow);
        if (derived == cycle.Status)
            return;

        cycle.Status = derived;
        cycle.StatusChangedOn = utcNow;
    }

    /// <summary>The chapter-14 review state as one token the SPA maps to its i18n keys.</summary>
    private static string ReviewStateOf(bool isAccepted, bool isRefused)
        => isAccepted ? "Accepted" : isRefused ? "Refused" : "Pending";

    /// <summary>Register search over the bilingual title/description plus an optional status pin.</summary>
    private static System.Linq.Expressions.Expression<Func<OrphanReportCycle, bool>> BuildSearchExpression(
        string? search, OrphanReportCycleStatus? status)
    {
        if (string.IsNullOrWhiteSpace(search) && status == null)
            return c => !c.IsDeleted;

        var term = search?.Trim();
        return c => !c.IsDeleted
                    && (status == null || c.Status == status)
                    && (string.IsNullOrWhiteSpace(term)
                        || c.TitleAr.Contains(term!)
                        || (c.TitleEn != null && c.TitleEn.Contains(term))
                        || (c.DescriptionAr != null && c.DescriptionAr.Contains(term))
                        || (c.DescriptionEn != null && c.DescriptionEn.Contains(term)));
    }

    /// <summary>Map rows and attach per-cycle report counts plus the shared population denominator.</summary>
    private async Task<List<OrphanReportCycleDto>> DecorateWithCountsAsync(IEnumerable<OrphanReportCycle> cycles)
    {
        var cycleList = cycles.ToList();
        var cycleIds = cycleList.Select(c => c.Id).ToList();

        var reportCounts = await _reportRepository.TableNoTracking
            .Where(r => r.CycleId != null && !r.IsDeleted && cycleIds.Contains(r.CycleId.Value))
            .GroupBy(r => r.CycleId!.Value)
            .Select(g => new { CycleId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.CycleId, x => x.Count);

        // The register's progress denominator: every live coded orphan, platform-wide —
        // cycles are HQ-defined rounds over the whole population (§26.1).
        var codedOrphans = await _orphanRepository.TableNoTracking
            .CountAsync(o => !o.IsDeleted && o.Code != string.Empty);

        return cycleList.Select(c =>
        {
            var reports = reportCounts.GetValueOrDefault(c.Id);
            return new OrphanReportCycleDto
            {
                Id = c.Id,
                TitleAr = c.TitleAr,
                TitleEn = c.TitleEn,
                DescriptionAr = c.DescriptionAr,
                DescriptionEn = c.DescriptionEn,
                StartDate = c.StartDate,
                EndDate = c.EndDate,
                Status = c.Status,
                StatusChangedOn = c.StatusChangedOn,
                ReportsCount = reports,
                CodedOrphansCount = codedOrphans,
                ProgressPercent = codedOrphans == 0 ? 0 : (int)Math.Round(reports * 100.0 / codedOrphans),
                CreatedOn = c.CreatedOn
            };
        }).ToList();
    }

    private static void ClampPaging<TFilter>(TFilter filter) where TFilter : class
    {
        // Both filter DTOs carry Page/PageSize; reflection-free clamp via the known shapes.
        if (filter is OrphanReportCycleFilterDto f)
        {
            f.Page = Math.Max(1, f.Page);
            f.PageSize = Math.Clamp(f.PageSize, 1, MaxPageSize);
        }
        else if (filter is CycleReportCoverageFilterDto c)
        {
            c.Page = Math.Max(1, c.Page);
            c.PageSize = Math.Clamp(c.PageSize, 1, MaxPageSize);
        }
    }
}
