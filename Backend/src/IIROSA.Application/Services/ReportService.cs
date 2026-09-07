using AutoMapper;
using FluentValidation;
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
/// UC-RPT-01 — read-only report projections (EP-18 epic skeleton). Composes over
/// <c>TableNoTracking</c> with its own include chain (the orphan repository's
/// <c>IncludeNavigationProperties</c> loads only Family + Sponsor + SocialStatus — not enough
/// for the §23.S.3 column contract). Repositories never save; this service never writes.
/// </summary>
public class ReportService : IReportService
{
    private readonly IOrphanRepository _orphanRepository;
    private readonly IRepository<Mother> _motherRepository;
    private readonly IRepository<OrphanPaymentItem> _paymentItemRepository;
    private readonly IRepository<Charity> _charityRepository;
    private readonly IRepository<PeriodicOrphanReport> _periodicReportRepository;
    private readonly IRepository<SeasonalAidBeneficiary> _beneficiaryRepository;
    private readonly IRepository<Provider> _providerRepository;
    private readonly IRepository<Family> _familyRepository;
    private readonly IRepository<OrphanPayment> _orphanPaymentRepository;
    private readonly IRepository<Outgoing> _outgoingRepository;
    private readonly IValidator<OrphanDataFilterDto> _validator;
    private readonly IValidator<NonRenewedReportsRequestDto> _nonRenewedValidator;
    private readonly IValidator<CharityPaymentTrackingFilterDto> _charityPaymentTrackingValidator;
    private readonly IValidator<FamilyUpdateTrackingFilterDto> _familyUpdateTrackingValidator;
    private readonly IValidator<MissedPaymentsReportFilterDto> _missedPaymentsValidator;
    private readonly IValidator<OrphanFilesExportFilterDto> _orphanFilesValidator;
    private readonly IValidator<OrphansWithoutPaymentFilterDto> _orphansWithoutPaymentValidator;
    private readonly IValidator<PaymentsOutcomeFilterDto> _paymentsOutcomeValidator;
    private readonly IValidator<ChequeNumbersFilterDto> _chequeNumbersValidator;
    private readonly IValidator<ReceiptCardsFilterDto> _receiptCardsValidator;
    private readonly IValidator<NewBeneficiariesFilterDto> _newBeneficiariesValidator;
    private readonly IValidator<FollowUpSheetFilterDto> _followUpSheetsValidator;
    private readonly IValidator<GuardianIdentificationSheetFilterDto> _guardianIdentificationValidator;
    private readonly IValidator<MissingOutgoingAttachmentsFilterDto> _missingOutgoingAttachmentsValidator;
    private readonly IValidator<FamilyOrphansByDateFilterDto> _familyOrphansByDateValidator;
    private readonly IValidator<ExcludedOrphansFilterDto> _excludedOrphansValidator;
    private readonly IValidator<OrphanStatusReportFilterDto> _orphanStatusReportValidator;
    private readonly IValidator<WidowSponsorshipFilterDto> _widowSponsorshipValidator;
    private readonly IValidator<MezaCardsFilterDto> _mezaCardsValidator;
    private readonly IValidator<BeneficiaryFamilyFilterDto> _beneficiaryFamilyValidator;
    private readonly IValidator<FamilyProjectsFilterDto> _familyProjectsValidator;
    private readonly IValidator<ProviderChangeFilterDto> _providerChangeValidator;
    private readonly IValidator<OrphansMissingReportsFilterDto> _orphansMissingReportsValidator;
    private readonly IValidator<OrphansMissingFilesFilterDto> _orphansMissingFilesValidator;
    private readonly IValidator<ReportsAwaitingApprovalFilterDto> _reportsAwaitingApprovalValidator;
    private readonly IValidator<RefusedReportsFilterDto> _refusedReportsValidator;
    private readonly IValidator<OrphansMissingReportsDetailRequestDto> _orphansMissingReportsDetailValidator;
    private readonly IRepository<Domain.Entities.Lookups.RefuseReason> _refuseReasonRepository;
    // Framework's attachment store — the manifest's FileName/ContentType source (18-24).
    private readonly AttachmentService _attachmentService;
    private readonly ICurrentUserService _currentUser;
    private readonly IMapper _mapper;
    private readonly ILogger<ReportService> _logger;

    public ReportService(
        IOrphanRepository orphanRepository,
        IRepository<Mother> motherRepository,
        IRepository<OrphanPaymentItem> paymentItemRepository,
        IRepository<Charity> charityRepository,
        IRepository<PeriodicOrphanReport> periodicReportRepository,
        IRepository<SeasonalAidBeneficiary> beneficiaryRepository,
        IRepository<Provider> providerRepository,
        IRepository<Family> familyRepository,
        IRepository<OrphanPayment> orphanPaymentRepository,
        IRepository<Outgoing> outgoingRepository,
        IValidator<OrphanDataFilterDto> validator,
        IValidator<NonRenewedReportsRequestDto> nonRenewedValidator,
        IValidator<CharityPaymentTrackingFilterDto> charityPaymentTrackingValidator,
        IValidator<FamilyUpdateTrackingFilterDto> familyUpdateTrackingValidator,
        IValidator<MissedPaymentsReportFilterDto> missedPaymentsValidator,
        IValidator<OrphanFilesExportFilterDto> orphanFilesValidator,
        IValidator<OrphansWithoutPaymentFilterDto> orphansWithoutPaymentValidator,
        IValidator<PaymentsOutcomeFilterDto> paymentsOutcomeValidator,
        IValidator<ChequeNumbersFilterDto> chequeNumbersValidator,
        IValidator<ReceiptCardsFilterDto> receiptCardsValidator,
        IValidator<NewBeneficiariesFilterDto> newBeneficiariesValidator,
        IValidator<FollowUpSheetFilterDto> followUpSheetsValidator,
        IValidator<GuardianIdentificationSheetFilterDto> guardianIdentificationValidator,
        IValidator<MissingOutgoingAttachmentsFilterDto> missingOutgoingAttachmentsValidator,
        IValidator<FamilyOrphansByDateFilterDto> familyOrphansByDateValidator,
        IValidator<ExcludedOrphansFilterDto> excludedOrphansValidator,
        IValidator<OrphanStatusReportFilterDto> orphanStatusReportValidator,
        IValidator<WidowSponsorshipFilterDto> widowSponsorshipValidator,
        IValidator<MezaCardsFilterDto> mezaCardsValidator,
        IValidator<BeneficiaryFamilyFilterDto> beneficiaryFamilyValidator,
        IValidator<FamilyProjectsFilterDto> familyProjectsValidator,
        IValidator<ProviderChangeFilterDto> providerChangeValidator,
        IValidator<OrphansMissingReportsFilterDto> orphansMissingReportsValidator,
        IValidator<OrphansMissingFilesFilterDto> orphansMissingFilesValidator,
        IValidator<ReportsAwaitingApprovalFilterDto> reportsAwaitingApprovalValidator,
        IValidator<RefusedReportsFilterDto> refusedReportsValidator,
        IValidator<OrphansMissingReportsDetailRequestDto> orphansMissingReportsDetailValidator,
        IRepository<Domain.Entities.Lookups.RefuseReason> refuseReasonRepository,
        AttachmentService attachmentService,
        ICurrentUserService currentUser,
        IMapper mapper,
        ILogger<ReportService> logger)
    {
        _orphanRepository = orphanRepository;
        _motherRepository = motherRepository;
        _paymentItemRepository = paymentItemRepository;
        _charityRepository = charityRepository;
        _periodicReportRepository = periodicReportRepository;
        _beneficiaryRepository = beneficiaryRepository;
        _providerRepository = providerRepository;
        _familyRepository = familyRepository;
        _orphanPaymentRepository = orphanPaymentRepository;
        _outgoingRepository = outgoingRepository;
        _validator = validator;
        _nonRenewedValidator = nonRenewedValidator;
        _charityPaymentTrackingValidator = charityPaymentTrackingValidator;
        _familyUpdateTrackingValidator = familyUpdateTrackingValidator;
        _missedPaymentsValidator = missedPaymentsValidator;
        _orphanFilesValidator = orphanFilesValidator;
        _orphansWithoutPaymentValidator = orphansWithoutPaymentValidator;
        _paymentsOutcomeValidator = paymentsOutcomeValidator;
        _chequeNumbersValidator = chequeNumbersValidator;
        _receiptCardsValidator = receiptCardsValidator;
        _newBeneficiariesValidator = newBeneficiariesValidator;
        _followUpSheetsValidator = followUpSheetsValidator;
        _guardianIdentificationValidator = guardianIdentificationValidator;
        _missingOutgoingAttachmentsValidator = missingOutgoingAttachmentsValidator;
        _familyOrphansByDateValidator = familyOrphansByDateValidator;
        _excludedOrphansValidator = excludedOrphansValidator;
        _orphanStatusReportValidator = orphanStatusReportValidator;
        _widowSponsorshipValidator = widowSponsorshipValidator;
        _mezaCardsValidator = mezaCardsValidator;
        _beneficiaryFamilyValidator = beneficiaryFamilyValidator;
        _familyProjectsValidator = familyProjectsValidator;
        _providerChangeValidator = providerChangeValidator;
        _orphansMissingReportsValidator = orphansMissingReportsValidator;
        _orphansMissingFilesValidator = orphansMissingFilesValidator;
        _reportsAwaitingApprovalValidator = reportsAwaitingApprovalValidator;
        _refusedReportsValidator = refusedReportsValidator;
        _orphansMissingReportsDetailValidator = orphansMissingReportsDetailValidator;
        _refuseReasonRepository = refuseReasonRepository;
        _attachmentService = attachmentService;
        _currentUser = currentUser;
        _mapper = mapper;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<ReportPagedResult<OrphanDataListDto>> GetOrphanDataAsync(
        OrphanDataFilterDto filter,
        CancellationToken cancellationToken = default)
    {
        // FluentValidation in the service layer (platform rule)
        await _validator.ValidateAndThrowAsync(filter, cancellationToken);

        // The platform has NO global soft-delete query filter — every read filters explicitly.
        var query = _orphanRepository.TableNoTracking
            .Where(o => !o.IsDeleted)
            .Include(o => o.Family!).ThenInclude(f => f.Father)
            .Include(o => o.Family!).ThenInclude(f => f.Mother)
            .Include(o => o.Family!).ThenInclude(f => f.Providers)
            .Include(o => o.Family!).ThenInclude(f => f.Region)
            .Include(o => o.Family!).ThenInclude(f => f.Center)
            .Include(o => o.Family!).ThenInclude(f => f.HouseOwnership)
            .Include(o => o.Family!).ThenInclude(f => f.HouseStatus)
            .Include(o => o.Family!).ThenInclude(f => f.HousingType)
            .Include(o => o.EducationLevel)
            .Include(o => o.HealthStatus)
            .Include(o => o.SocialStatus)
            .AsQueryable();

        // Tenancy first — a charity caller is pinned to its own charity whatever the payload says.
        query = await ApplyCharityScopeAsync(query, filter.CharityId, cancellationToken);

        // الدفعة المالية المنصرقة للايتام — orphans enrolled in this payment batch
        if (!string.IsNullOrWhiteSpace(filter.BatchNumber))
        {
            var batchNumber = filter.BatchNumber.Trim();
            var orphansInBatch = _paymentItemRepository.TableNoTracking
                .Where(item => !item.IsDeleted
                    && item.OrphanPayment != null
                    && !item.OrphanPayment.IsDeleted
                    && item.OrphanPayment.BatchNo == batchNumber)
                .Select(item => item.OrphanId);
            query = query.Where(o => orphansInBatch.Contains(o.Id));
        }

        // المحافظة / المركز
        if (filter.GovernorateId.HasValue)
        {
            query = query.Where(o => o.Family != null && o.Family.RegionId == filter.GovernorateId.Value);
        }
        if (filter.CenterId.HasValue)
        {
            query = query.Where(o => o.Family != null && o.Family.CenterId == filter.CenterId.Value);
        }

        // من / الى — age computed on the server from DateOfBirth. Review P5 2026-08-26: the
        // range converts to a DOB window so "age" means completed birthday-aware years EXACTLY
        // as displayed (DateDiffYear counted calendar-year boundaries and read one year high
        // before the birthday). A future DOB matches no age band.
        var today = DateTime.Today;
        if (filter.AgeFrom.HasValue)
        {
            var oldest = today.AddYears(-filter.AgeFrom.Value);
            query = query.Where(o => o.DateOfBirth != null && o.DateOfBirth.Value <= oldest);
        }
        if (filter.AgeTo.HasValue)
        {
            var youngest = today.AddYears(-filter.AgeTo.Value - 1);
            query = query.Where(o => o.DateOfBirth != null
                && o.DateOfBirth.Value > youngest
                && o.DateOfBirth.Value <= today);
        }

        // Exclusion flag (§23.S.3): Excluded → excluded set only; default → the whole scoped
        // register. No exclusion columns exist yet (epic-wide no-migration ruling), so the
        // excluded set is empty — recorded, not patched with a migration. (Review P4 2026-08-26:
        // the dead AllOrphans/NotExcluded wire flags were removed from the DTO.)
        if (filter.Excluded)
        {
            _logger.LogInformation("Orphan data report: Excluded set requested — empty until exclusion columns exist (recorded gap)");
            return new ReportPagedResult<OrphanDataListDto>
            {
                Items = new List<OrphanDataListDto>(),
                TotalCount = 0,
                Page = filter.Page,
                PageSize = filter.PageSize
            };
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var page = await query
            .OrderBy(o => o.Code)
            .Skip(PageSkip(filter.Page, filter.PageSize))
            .Take(filter.PageSize)
            .ToListAsync(cancellationToken);

        var items = _mapper.Map<List<OrphanDataListDto>>(page);

        // Charity names resolve through one code path — a dictionary over the page's charity ids.
        await ResolveCharityNamesAsync(items, cancellationToken);

        return new ReportPagedResult<OrphanDataListDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = filter.Page,
            PageSize = filter.PageSize
        };
    }

    /// <inheritdoc />
    public async Task<NonRenewedReportsResultDto> GetNonRenewedReportAsync(
        NonRenewedReportsRequestDto request,
        CancellationToken cancellationToken = default)
    {
        await _nonRenewedValidator.ValidateAndThrowAsync(request, cancellationToken);

        var page = Math.Max(request.Page, 1);
        var pageSize = request.PageSize <= 0 ? 20 : Math.Min(request.PageSize, 100);

        // Batch mode (18-19): a batchId sent, or no window at all — an absent batch means
        // the current one. Window mode (§14.U.14): a window with no batch — the dates are
        // the chase-list semantic.
        var batchMode = !string.IsNullOrWhiteSpace(request.BatchId)
            || (request.DateFrom == default && request.DateTo == default);

        string? batchNo = null;
        IQueryable<Orphan> query;
        IQueryable<Guid> acceptedIds;

        if (batchMode)
        {
            // Resolve the batch: given → match on OrphanPayment.BatchNo; absent → the latest
            // payment by GroupDate that carries items (a batch without members chases nobody).
            batchNo = request.BatchId;
            if (string.IsNullOrWhiteSpace(batchNo))
            {
                batchNo = await _paymentItemRepository.TableNoTracking
                    .Where(i => !i.IsDeleted
                        && i.OrphanPayment != null
                        && !i.OrphanPayment.IsDeleted
                        && i.OrphanPayment.BatchNo != null
                        && i.OrphanPayment.BatchNo != string.Empty)
                    .OrderByDescending(i => i.OrphanPayment!.GroupDate)
                    .ThenByDescending(i => i.OrphanPayment!.Id)
                    .Select(i => i.OrphanPayment!.BatchNo!)
                    .FirstOrDefaultAsync(cancellationToken);
            }

            if (string.IsNullOrWhiteSpace(batchNo))
            {
                // No batches exist — nothing to chase (the zero-pages case).
                return new NonRenewedReportsResultDto
                {
                    Items = new List<NonRenewedOrphanRowDto>(),
                    Page = page,
                    PageSize = pageSize
                };
            }

            // Membership is the batch's payment items; the trailing-12-months cycle rule
            // (18-15) decides renewal. Uncoded filter skipped here on purpose — the batch,
            // not the coding, is the membership semantic.
            var batchOrphanIds = _paymentItemRepository.TableNoTracking
                .Where(i => !i.IsDeleted
                    && i.OrphanPayment != null
                    && !i.OrphanPayment.IsDeleted
                    && i.OrphanPayment.BatchNo == batchNo)
                .Select(i => i.OrphanId);

            query = _orphanRepository.TableNoTracking
                .Where(o => !o.IsDeleted && batchOrphanIds.Contains(o.Id));

            acceptedIds = BuildAcceptedReportIdsQuery(
                DateTime.UtcNow.AddMonths(-12).Date,
                DateTime.UtcNow.Date.AddDays(1));
        }
        else
        {
            // Coded, non-deleted orphans under the caller scope (pin-never-widen, AC 5) —
            // uncoded orphans never join a payment run, so they are not chased.
            query = _orphanRepository.TableNoTracking
                .Where(o => !o.IsDeleted && !string.IsNullOrEmpty(o.Code));

            acceptedIds = BuildAcceptedReportIdsQuery(
                request.DateFrom.Date,
                ExclusiveUpperBound(request.DateTo.Date));
        }

        query = await ApplyCharityScopeAsync(query, request.CharityId, cancellationToken);
        query = query.Where(o => !acceptedIds.Contains(o.Id));

        var count = await query.CountAsync(cancellationToken);
        if (request.CountOnly)
        {
            // The legacy _Number screens — just the number.
            return new NonRenewedReportsResultDto
            {
                Count = count,
                TotalCount = count,
                Items = new List<NonRenewedOrphanRowDto>(),
                Page = 1,
                PageSize = 0
            };
        }

        var anyReports = _periodicReportRepository.TableNoTracking
            .Where(r => !r.IsDeleted);

        var rows = await query
            .OrderBy(o => o.Code)
            .Skip(PageSkip(page, pageSize))
            .Take(pageSize)
            .Select(o => new NonRenewedOrphanRowDto
            {
                OrphanId = o.Id,
                Code = o.Code,
                FullName = o.FullName,
                FamilyCode = o.Family != null ? o.Family.Code : null,
                // Any-state latest report — an accepted one would have cleared the orphan.
                LastReportDate = anyReports
                    .Where(r => r.OrphanId == o.Id)
                    .OrderByDescending(r => r.ReportDate)
                    .Select(r => (DateTime?)r.ReportDate)
                    .FirstOrDefault(),
                CharityId = o.FK_CharityId,
                BatchNo = batchNo
            })
            .ToListAsync(cancellationToken);

        await ResolveNonRenewedCharityNamesAsync(rows, cancellationToken);

        return new NonRenewedReportsResultDto
        {
            Count = count,
            TotalCount = count,
            Items = rows,
            Page = page,
            PageSize = pageSize
        };
    }

    /// <inheritdoc />
    public async Task<ReportPagedResult<CharityPaymentTrackingRowDto>> GetCharityPaymentTrackingAsync(
        CharityPaymentTrackingFilterDto filter,
        CancellationToken cancellationToken = default)
    {
        await _charityPaymentTrackingValidator.ValidateAndThrowAsync(filter, cancellationToken);

        var page = Math.Max(filter.Page, 1);
        var pageSize = filter.PageSize <= 0 ? 20 : Math.Min(filter.PageSize, 100);

        // Resolve the batch (18-19's ruling): given BatchNo, else the latest payment by
        // GroupDate that carries items — a batch without members tracks nobody.
        var batchNo = filter.BatchId;
        if (string.IsNullOrWhiteSpace(batchNo))
        {
            batchNo = await _paymentItemRepository.TableNoTracking
                .Where(i => !i.IsDeleted
                    && i.OrphanPayment != null
                    && !i.OrphanPayment.IsDeleted
                    && i.OrphanPayment.BatchNo != null
                    && i.OrphanPayment.BatchNo != string.Empty)
                .OrderByDescending(i => i.OrphanPayment!.GroupDate)
                .ThenByDescending(i => i.OrphanPayment!.Id)
                .Select(i => i.OrphanPayment!.BatchNo!)
                .FirstOrDefaultAsync(cancellationToken);
        }

        if (string.IsNullOrWhiteSpace(batchNo))
        {
            // No batches exist — the zero-pages case (AC 5).
            return new ReportPagedResult<CharityPaymentTrackingRowDto>
            {
                Items = new List<CharityPaymentTrackingRowDto>(),
                TotalCount = 0,
                Page = page,
                PageSize = pageSize
            };
        }

        // The batch membership, flattened to (orphan, charity, payment upload state) —
        // grouped in memory: one row per charity, and the report counts resolve per
        // charity's member set without a grouped-subquery translation.
        var memberships = await _paymentItemRepository.TableNoTracking
            .Where(i => !i.IsDeleted
                && i.OrphanPayment != null
                && !i.OrphanPayment.IsDeleted
                && i.OrphanPayment.BatchNo == batchNo)
            .Select(i => new
            {
                i.OrphanId,
                CharityId = i.Orphan != null ? i.Orphan.FK_CharityId : (Guid?)null,
                Uploaded = i.OrphanPayment!.IsBatchUploaded,
                UploadDate = i.OrphanPayment.UploadDate
            })
            .ToListAsync(cancellationToken);

        var groups = memberships
            .Where(m => m.CharityId.HasValue)
            .GroupBy(m => m.CharityId!.Value)
            .Select(g => new CharityTrackingGroup(
                g.Key,
                g.Select(x => x.OrphanId).Distinct().ToList(),
                // Batch-header state per charity via its own items' payments — honest when
                // one BatchNo spans several payment groups (any uploaded / latest upload).
                g.Any(x => x.Uploaded),
                g.Select(x => x.UploadDate).Max()))
            .ToList();

        // Charity-rooted scope ladder (the orphan-typed ApplyCharityScopeAsync mirrored on
        // the charity set): claim pin → HQ narrow → country pin. Pin-never-widen.
        IEnumerable<CharityTrackingGroup> scoped = groups;
        if (_currentUser.CharityId.HasValue)
        {
            var pinned = _currentUser.CharityId.Value;
            scoped = groups.Where(g => g.CharityId == pinned);
        }
        else if (_currentUser.IsHeadOffice && filter.CharityId.HasValue)
        {
            var narrowed = filter.CharityId.Value;
            scoped = groups.Where(g => g.CharityId == narrowed);
        }

        FailClosedWhenUnscoped();

        if (_currentUser.CountryId.HasValue)
        {
            var countryId = _currentUser.CountryId.Value;
            var countryCharityIds = await _charityRepository.TableNoTracking
                .Where(c => !c.IsDeleted && c.CountryId == countryId)
                .Select(c => c.Id)
                .ToListAsync(cancellationToken);
            scoped = scoped.Where(g => countryCharityIds.Contains(g.CharityId));
        }

        return await BuildCharityTrackingPageAsync(scoped.ToList(), page, pageSize, cancellationToken);
    }

    /// <summary>Counts the entered reports per member orphan and pages the per-charity rows.</summary>
    private async Task<ReportPagedResult<CharityPaymentTrackingRowDto>> BuildCharityTrackingPageAsync(
        List<CharityTrackingGroup> groups,
        int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        // Entered reports (any state, non-deleted) for the batch members — chunked IN over
        // the member ids (a batch can exceed the 2100-parameter SQL Server limit).
        var reportCounts = new Dictionary<Guid, int>();
        var memberIds = groups.SelectMany(g => g.OrphanIds).Distinct().ToList();
        foreach (var chunk in memberIds.Chunk(1000))
        {
            var chunkCounts = await _periodicReportRepository.TableNoTracking
                .Where(r => !r.IsDeleted && chunk.Contains(r.OrphanId))
                .GroupBy(r => r.OrphanId)
                .Select(g => new { g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Key, x => x.Count, cancellationToken);
            foreach (var pair in chunkCounts)
            {
                reportCounts[pair.Key] = pair.Value;
            }
        }

        var charityIds = groups.Select(g => g.CharityId).ToList();
        var names = await _charityRepository.TableNoTracking
            .Where(c => !c.IsDeleted && charityIds.Contains(c.Id))
            .ToDictionaryAsync(c => c.Id, c => c.Name, cancellationToken);

        var all = groups
            .Select(g => new CharityPaymentTrackingRowDto
            {
                CharityId = g.CharityId,
                CharityName = names.TryGetValue(g.CharityId, out var name) ? name : null,
                OrphansInBatch = g.OrphanIds.Count,
                ReportsEntered = g.OrphanIds.Sum(id => reportCounts.TryGetValue(id, out var count) ? count : 0),
                BatchUploaded = g.Uploaded,
                UploadDate = g.UploadDate
            })
            .OrderBy(r => r.CharityName, StringComparer.OrdinalIgnoreCase)
            .ThenBy(r => r.CharityId)
            .ToList();

        return new ReportPagedResult<CharityPaymentTrackingRowDto>
        {
            Items = all.Skip(PageSkip(page, pageSize)).Take(pageSize).ToList(),
            TotalCount = all.Count,
            Page = page,
            PageSize = pageSize
        };
    }

    /// <summary>The in-memory per-charity grouping of one batch's membership (UC-RPT-20).</summary>
    private sealed record CharityTrackingGroup(
        Guid CharityId,
        List<Guid> OrphanIds,
        bool Uploaded,
        DateTime? UploadDate);

    /// <inheritdoc />
    public async Task<ReportPagedResult<FamilyUpdateTrackingRowDto>> GetFamilyUpdateTrackingAsync(
        FamilyUpdateTrackingFilterDto filter,
        CancellationToken cancellationToken = default)
    {
        // FluentValidation in the service layer (platform rule) — Date is the one required
        // field (من فضلك ادخل تاريخ بدا التحديث).
        await _familyUpdateTrackingValidator.ValidateAndThrowAsync(filter, cancellationToken);

        var page = filter.Page < 1 ? 1 : filter.Page;
        var pageSize = Math.Clamp(filter.PageSize, 1, 500);

        var empty = new ReportPagedResult<FamilyUpdateTrackingRowDto>
        {
            Items = new List<FamilyUpdateTrackingRowDto>(),
            TotalCount = 0,
            Page = page,
            PageSize = pageSize
        };

        // Resolve the payment anchor: PaymentId (precise) → the batch's latest payment → the
        // latest payment. Unlike 18-19/18-20 the anchor is a period reference, so it need not
        // carry items — the items are only used to derive the payment's charity set.
        OrphanPayment? payment;
        if (filter.PaymentId.HasValue)
        {
            payment = await _orphanPaymentRepository.TableNoTracking
                .Where(p => !p.IsDeleted && p.Id == filter.PaymentId.Value)
                .FirstOrDefaultAsync(cancellationToken);
        }
        else if (!string.IsNullOrWhiteSpace(filter.BatchNo))
        {
            payment = await _orphanPaymentRepository.TableNoTracking
                .Where(p => !p.IsDeleted && p.BatchNo == filter.BatchNo)
                .OrderByDescending(p => p.GroupDate)
                .ThenByDescending(p => p.Id)
                .FirstOrDefaultAsync(cancellationToken);
        }
        else
        {
            payment = await _orphanPaymentRepository.TableNoTracking
                .Where(p => !p.IsDeleted)
                .OrderByDescending(p => p.GroupDate)
                .ThenByDescending(p => p.Id)
                .FirstOrDefaultAsync(cancellationToken);
        }

        if (payment == null)
        {
            // No payments exist — nothing to track (the zero-pages case).
            return empty;
        }

        // The monitoring window: [requested date 00:00, period end + 1 day) — families of the
        // payment's charities refreshed in that window. A payment without a usable period end
        // closes the window at now (story fallback).
        var windowStart = filter.Date.Date;
        var periodEnd = payment.PaymentPeriodTo != default(DateTime)
            ? payment.PaymentPeriodTo.Date
            : DateTime.UtcNow.Date;
        var windowEndExclusive = ExclusiveUpperBound(periodEnd);
        if (windowEndExclusive <= windowStart)
        {
            // The requested date is at/after the payment period end — nothing can be in window.
            return empty;
        }

        // Charities present in the payment (via its orphans) — the sheet's charity set.
        var paymentCharityIds = _paymentItemRepository.TableNoTracking
            .Where(i => !i.IsDeleted
                && i.OrphanPaymentId == payment.Id
                && i.Orphan != null
                && i.Orphan.FK_CharityId != null)
            .Select(i => i.Orphan!.FK_CharityId!.Value);

        // The platform has NO global soft-delete query filter — every read filters explicitly.
        var query = _familyRepository.TableNoTracking
            .Where(f => !f.IsDeleted
                && f.FK_CharityId != null
                && paymentCharityIds.Contains(f.FK_CharityId!.Value)
                && f.UpdatedOn >= windowStart
                && f.UpdatedOn < windowEndExclusive);

        // Charity-rooted scope ladder (HQ-only endpoint — the pins are defensive):
        // claim pin → HQ narrow → country pin. Pin-never-widen.
        if (_currentUser.CharityId.HasValue)
        {
            var pinned = _currentUser.CharityId.Value;
            query = query.Where(f => f.FK_CharityId == pinned);
        }
        else if (_currentUser.IsHeadOffice && filter.CharityId.HasValue)
        {
            var narrowed = filter.CharityId.Value;
            query = query.Where(f => f.FK_CharityId == narrowed);
        }

        FailClosedWhenUnscoped();

        if (_currentUser.CountryId.HasValue)
        {
            var countryId = _currentUser.CountryId.Value;
            var countryCharityIds = _charityRepository.TableNoTracking
                .Where(c => !c.IsDeleted && c.CountryId == countryId)
                .Select(c => c.Id);
            query = query.Where(f => countryCharityIds.Contains(f.FK_CharityId!.Value));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var rows = await query
            .OrderBy(f => f.Code)
            .ThenBy(f => f.Id)
            .Skip(PageSkip(page, pageSize))
            .Take(pageSize)
            .Select(f => new { f.Id, f.Code, f.HeadOfFamily, f.FK_CharityId, f.UpdatedOn })
            .ToListAsync(cancellationToken);

        // Charity labels resolve for the page's rows only (c.Name — Charity has plain Name).
        var pageCharityIds = rows
            .Where(r => r.FK_CharityId.HasValue)
            .Select(r => r.FK_CharityId!.Value)
            .Distinct()
            .ToList();
        var names = pageCharityIds.Count == 0
            ? new Dictionary<Guid, string>()
            : await _charityRepository.TableNoTracking
                .Where(c => !c.IsDeleted && pageCharityIds.Contains(c.Id))
                .ToDictionaryAsync(c => c.Id, c => c.Name, cancellationToken);

        var items = rows
            .Select(r => new FamilyUpdateTrackingRowDto
            {
                FamilyId = r.Id,
                FamilyCode = r.Code,
                HeadOfFamily = r.HeadOfFamily,
                CharityName = r.FK_CharityId.HasValue
                    && names.TryGetValue(r.FK_CharityId.Value, out var name)
                    ? name
                    : null,
                UpdatedOn = r.UpdatedOn
            })
            .ToList();

        return new ReportPagedResult<FamilyUpdateTrackingRowDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    /// <inheritdoc />
    public async Task<ReportPagedResult<MissedPaymentReportRowDto>> GetMissedPaymentsAsync(
        MissedPaymentsReportFilterDto filter,
        CancellationToken cancellationToken = default)
    {
        await _missedPaymentsValidator.ValidateAndThrowAsync(filter, cancellationToken);

        var page = filter.Page < 1 ? 1 : filter.Page;
        var pageSize = Math.Clamp(filter.PageSize, 1, 100);

        // UC-RPT-23's widened scope (جميع الدفعات الفائتة) is HQ-only — a non-HQ caller
        // sending allOrphans (toggle UI or forged payload) is refused; the pinned claim never
        // widens. Review P4 2026-08-26: for HQ the flag is an ACCEPTED NO-OP, and this comment
        // previously claimed otherwise — an unconstrained HQ caller's default scope is already
        // the full register (the ladder only narrows on an explicit CharityId), so there is no
        // narrower default for the flag to widen. The refusal above is the flag's entire effect.
        if (filter.AllOrphans && !_currentUser.IsHeadOffice)
        {
            throw new UnauthorizedAccessException("The all-orphans scope is not permitted for this caller");
        }

        // Scope the orphan set (charity-rooted ladder): claim pin → HQ narrow → country pin.
        // Pin-never-widen; the payload's CharityId is an HQ narrow, never a charity caller's.
        var orphanQuery = _orphanRepository.TableNoTracking
            .Where(o => !o.IsDeleted && o.FK_CharityId != null);
        if (_currentUser.CharityId.HasValue)
        {
            var pinned = _currentUser.CharityId.Value;
            orphanQuery = orphanQuery.Where(o => o.FK_CharityId == pinned);
        }
        else if (_currentUser.IsHeadOffice && filter.CharityId.HasValue)
        {
            var narrowed = filter.CharityId.Value;
            orphanQuery = orphanQuery.Where(o => o.FK_CharityId == narrowed);
        }
        FailClosedWhenUnscoped();

        if (_currentUser.CountryId.HasValue)
        {
            var countryId = _currentUser.CountryId.Value;
            var countryCharityIds = _charityRepository.TableNoTracking
                .Where(c => !c.IsDeleted && c.CountryId == countryId)
                .Select(c => c.Id);
            orphanQuery = orphanQuery.Where(o => countryCharityIds.Contains(o.FK_CharityId!.Value));
        }

        var scopedOrphanIds = orphanQuery.Select(o => o.Id);

        // Entitlement evidence: non-deleted, non-stopped items in a numbered batch. A stopped
        // item is a deliberate stop (18-29's stopped list), not an arrears. The platform has NO
        // global soft-delete filter — explicit !IsDeleted on every leg.
        var items = await _paymentItemRepository.TableNoTracking
            .Where(i => !i.IsDeleted
                && !i.IsStopped
                && i.OrphanPayment != null
                && !i.OrphanPayment.IsDeleted
                && i.OrphanPayment.BatchNo != null
                && i.OrphanPayment.BatchNo != string.Empty
                && scopedOrphanIds.Contains(i.OrphanId))
            .Select(i => new
            {
                i.OrphanId,
                i.IsGotIt,
                BatchNo = i.OrphanPayment!.BatchNo!,
                OrphanCode = i.Orphan!.Code,
                OrphanName = i.Orphan!.FullName,
                CharityId = i.Orphan!.FK_CharityId
            })
            .ToListAsync(cancellationToken);

        // Group per orphan, then per batch (one BatchNo can span several payment groups —
        // any received item of that batch clears the batch for the orphan).
        var allRows = items
            .GroupBy(i => i.OrphanId)
            .Select(g => new
            {
                OrphanId = g.Key,
                OrphanCode = g.First().OrphanCode,
                OrphanName = g.First().OrphanName,
                CharityId = g.First().CharityId,
                States = g.GroupBy(x => x.BatchNo, StringComparer.Ordinal)
                    .Select(b => new { BatchNo = b.Key, GotIt = b.Any(x => x.IsGotIt) })
                    .ToDictionary(b => b.BatchNo, b => b.GotIt)
            })
            // The arrears set: at least one entitled-but-unreceived batch.
            .Where(g => g.States.Any(s => !s.Value))
            .OrderBy(g => g.OrphanCode, StringComparer.OrdinalIgnoreCase)
            .ThenBy(g => g.OrphanId)
            .ToList();

        var paged = allRows
            .Skip(PageSkip(page, pageSize))
            .Take(pageSize)
            .ToList();

        // Charity labels resolve for the page's rows only (c.Name — Charity has plain Name).
        var pageCharityIds = paged
            .Where(r => r.CharityId.HasValue)
            .Select(r => r.CharityId!.Value)
            .Distinct()
            .ToList();
        var names = pageCharityIds.Count == 0
            ? new Dictionary<Guid, string>()
            : await _charityRepository.TableNoTracking
                .Where(c => !c.IsDeleted && pageCharityIds.Contains(c.Id))
                .ToDictionaryAsync(c => c.Id, c => c.Name, cancellationToken);

        return new ReportPagedResult<MissedPaymentReportRowDto>
        {
            Items = paged.Select(r => new MissedPaymentReportRowDto
            {
                OrphanId = r.OrphanId,
                OrphanCode = r.OrphanCode,
                OrphanName = r.OrphanName,
                CharityName = r.CharityId.HasValue && names.TryGetValue(r.CharityId.Value, out var name)
                    ? name
                    : null,
                Reason = null,
                BatchStates = r.States,
                MissedBatches = r.States.Count(s => !s.Value)
            }).ToList(),
            TotalCount = allRows.Count,
            Page = page,
            PageSize = pageSize
        };
    }

    /// <inheritdoc />
    public Task<ReportPagedResult<OrphanFileManifestRowDto>> ExportOrphanFilesAsync(
        OrphanFilesExportFilterDto filter,
        CancellationToken cancellationToken = default)
    {
        // §23.U.24: the photograph kind — the report's OrphanImageId column.
        return GetReportImageManifestAsync(filter, certificateKind: false, cancellationToken);
    }

    /// <inheritdoc />
    public Task<ReportPagedResult<OrphanFileManifestRowDto>> ExportCertificateFilesAsync(
        OrphanFilesExportFilterDto filter,
        CancellationToken cancellationToken = default)
    {
        // §23.U.25: the certificate kind — the report's OrphanCertificateImageId column. The
        // kind discriminator is the REPORT's own column pair (verified: photo and certificate
        // are distinct FKs on PeriodicOrphanReport, not AttachmentType values).
        return GetReportImageManifestAsync(filter, certificateKind: true, cancellationToken);
    }

    /// <summary>
    /// The shared §23.S.18 image-manifest read behind UC-RPT-24 (photographs) and UC-RPT-25
    /// (certificates): accepted periodic reports dated in [من تاريخ, الى تاريخ], one row per
    /// report carrying an image of the requested kind, ordered by orphan code. Same filter,
    /// same validator, same envelope — only the report's image column differs.
    /// </summary>
    private async Task<ReportPagedResult<OrphanFileManifestRowDto>> GetReportImageManifestAsync(
        OrphanFilesExportFilterDto filter,
        bool certificateKind,
        CancellationToken cancellationToken)
    {
        await _orphanFilesValidator.ValidateAndThrowAsync(filter, cancellationToken);

        var page = filter.Page < 1 ? 1 : filter.Page;
        var pageSize = Math.Clamp(filter.PageSize, 1, 100);

        // Day-inclusive window [من تاريخ, الى تاريخ]; an absent الى تاريخ leaves the upper
        // bound open. ReportDate is the §23.U.24/§23.U.25 recency axis.
        var from = filter.DateFrom!.Value.Date;
        var toExclusive = filter.DateTo.HasValue
            ? ExclusiveUpperBound(filter.DateTo.Value.Date)
            : DateTime.MaxValue.Date;

        // Scope the orphan set (the 18-22 charity-rooted ladder): claim pin → HQ narrow →
        // country pin. Pin-never-widen; the payload's CharityId is an HQ narrow, never a
        // charity caller's.
        var orphanQuery = _orphanRepository.TableNoTracking
            .Where(o => !o.IsDeleted && o.FK_CharityId != null);
        if (_currentUser.CharityId.HasValue)
        {
            var pinned = _currentUser.CharityId.Value;
            orphanQuery = orphanQuery.Where(o => o.FK_CharityId == pinned);
        }
        else if (_currentUser.IsHeadOffice && filter.CharityId.HasValue)
        {
            var narrowed = filter.CharityId.Value;
            orphanQuery = orphanQuery.Where(o => o.FK_CharityId == narrowed);
        }
        FailClosedWhenUnscoped();

        if (_currentUser.CountryId.HasValue)
        {
            var countryId = _currentUser.CountryId.Value;
            var countryCharityIds = _charityRepository.TableNoTracking
                .Where(c => !c.IsDeleted && c.CountryId == countryId)
                .Select(c => c.Id);
            orphanQuery = orphanQuery.Where(o => countryCharityIds.Contains(o.FK_CharityId!.Value));
        }
        var scopedOrphanIds = orphanQuery.Select(o => o.Id);

        // §23.U.24/§23.U.25: images of ACCEPTED periodic reports (IsAccepted) dated inside
        // the window — one manifest row per accepted report carrying an image of the kind.
        // The platform has NO global soft-delete filter — explicit !IsDeleted on both legs.
        var reportsQuery = _periodicReportRepository.TableNoTracking
            .Where(r => !r.IsDeleted
                && r.IsAccepted
                && (certificateKind ? r.OrphanCertificateImageId != null : r.OrphanImageId != null)
                && r.ReportDate >= from
                && r.ReportDate < toExclusive
                && scopedOrphanIds.Contains(r.OrphanId));

        var totalCount = await reportsQuery.CountAsync(cancellationToken);

        var pageRows = await reportsQuery
            .OrderBy(r => r.Orphan!.Code)
            .ThenBy(r => r.ReportDate)
            .ThenBy(r => r.Id)
            .Skip(PageSkip(page, pageSize))
            .Take(pageSize)
            .Select(r => new
            {
                OrphanId = r.OrphanId,
                OrphanCode = r.Orphan!.Code,
                OrphanName = r.Orphan!.FullName,
                CharityId = r.Orphan!.FK_CharityId,
                AttachmentId = certificateKind ? r.OrphanCertificateImageId!.Value : r.OrphanImageId!.Value
            })
            .ToListAsync(cancellationToken);

        // Attachment metadata (FileName/ContentType) lives in Framework's store, not this
        // context — batch-resolved for the page's rows only (mirror of the page-rows-only
        // charity-name rule). A row never disappears because its metadata fetch failed.
        var attachmentIds = pageRows.Select(r => r.AttachmentId).Distinct().ToList();
        var attachments = attachmentIds.Count == 0
            ? new List<Framework.Core.SharedServices.Entities.Attachment>()
            : await _attachmentService.GetAttachmentAsync(attachmentIds);
        var metaById = attachments
            .GroupBy(a => a.Id)
            .ToDictionary(g => g.Key, g => g.First());

        // Charity labels resolve for the page's rows only (c.Name — Charity has plain Name).
        var pageCharityIds = pageRows
            .Where(r => r.CharityId.HasValue)
            .Select(r => r.CharityId!.Value)
            .Distinct()
            .ToList();
        var names = pageCharityIds.Count == 0
            ? new Dictionary<Guid, string>()
            : await _charityRepository.TableNoTracking
                .Where(c => !c.IsDeleted && pageCharityIds.Contains(c.Id))
                .ToDictionaryAsync(c => c.Id, c => c.Name, cancellationToken);

        return new ReportPagedResult<OrphanFileManifestRowDto>
        {
            Items = pageRows.Select(r =>
            {
                metaById.TryGetValue(r.AttachmentId, out var meta);
                return new OrphanFileManifestRowDto
                {
                    OrphanId = r.OrphanId,
                    OrphanCode = r.OrphanCode,
                    OrphanName = r.OrphanName,
                    CharityName = r.CharityId.HasValue && names.TryGetValue(r.CharityId.Value, out var name)
                        ? name
                        : null,
                    AttachmentId = r.AttachmentId,
                    FileName = meta?.FileName,
                    ContentType = meta?.ContentType,
                    DownloadUrl = $"api/Attachments/{r.AttachmentId}/download"
                };
            }).ToList(),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    /// <inheritdoc />
    public async Task<ReportPagedResult<OrphansWithoutPaymentListDto>> GetOrphansWithoutPaymentAsync(
        OrphansWithoutPaymentFilterDto filter,
        CancellationToken cancellationToken = default)
    {
        await _orphansWithoutPaymentValidator.ValidateAndThrowAsync(filter, cancellationToken);

        var page = filter.Page < 1 ? 1 : filter.Page;
        var pageSize = Math.Clamp(filter.PageSize, 1, 100);

        // Scope the orphan set (the 18-22 charity-rooted ladder): claim pin → HQ narrow →
        // country pin. Pin-never-widen; the payload's CharityId is an HQ narrow, never a
        // charity caller's. The endpoint's role gate is HQ-only — the pin stays as
        // defence-in-depth.
        var orphanQuery = _orphanRepository.TableNoTracking
            .Where(o => !o.IsDeleted && o.FK_CharityId != null);
        if (_currentUser.CharityId.HasValue)
        {
            var pinned = _currentUser.CharityId.Value;
            orphanQuery = orphanQuery.Where(o => o.FK_CharityId == pinned);
        }
        else if (_currentUser.IsHeadOffice && filter.CharityId.HasValue)
        {
            var narrowed = filter.CharityId.Value;
            orphanQuery = orphanQuery.Where(o => o.FK_CharityId == narrowed);
        }
        FailClosedWhenUnscoped();

        if (_currentUser.CountryId.HasValue)
        {
            var countryId = _currentUser.CountryId.Value;
            var countryCharityIds = _charityRepository.TableNoTracking
                .Where(c => !c.IsDeleted && c.CountryId == countryId)
                .Select(c => c.Id);
            orphanQuery = orphanQuery.Where(o => countryCharityIds.Contains(o.FK_CharityId!.Value));
        }
        var scopedOrphanIds = orphanQuery.Select(o => o.Id);

        // §23.U.28: "nothing disbursed" = NO disbursement instrument and nothing received —
        // no cheque (ChiqueNum), no transfer (TransferNo), !IsGotIt. An orphan whose cheque
        // was issued but not yet collected is the غير مستلم variant (out of scope here);
        // a stopped orphan IS a zero-disbursement gap and stays in the list.
        // Review P8 2026-08-26: empty-string instrument numbers count as "no instrument" here
        // too — the dashboard's cheque figures use the same IsNullOrEmpty reading, so a row
        // can no longer be "cheque-less" in one report and "cheque-issued" in the other.
        var gapsQuery = _paymentItemRepository.TableNoTracking
            .Where(i => !i.IsDeleted
                && i.OrphanPaymentId == filter.PaymentId
                && !i.IsGotIt
                && string.IsNullOrEmpty(i.ChiqueNum)
                && string.IsNullOrEmpty(i.TransferNo)
                && scopedOrphanIds.Contains(i.OrphanId));

        var totalCount = await gapsQuery.CountAsync(cancellationToken);

        var pageRows = await gapsQuery
            .OrderBy(i => i.Orphan!.Code)
            .ThenBy(i => i.Id)
            .Skip(PageSkip(page, pageSize))
            .Take(pageSize)
            .Select(i => new
            {
                i.OrphanId,
                OrphanCode = i.Orphan!.Code,
                OrphanName = i.Orphan!.FullName,
                CharityId = i.Orphan!.FK_CharityId,
                Amount = i.Amount ?? 0m,
                i.IsStopped
            })
            .ToListAsync(cancellationToken);

        // Charity labels resolve for the page's rows only (c.Name — Charity has plain Name).
        var pageCharityIds = pageRows
            .Where(r => r.CharityId.HasValue)
            .Select(r => r.CharityId!.Value)
            .Distinct()
            .ToList();
        var names = pageCharityIds.Count == 0
            ? new Dictionary<Guid, string>()
            : await _charityRepository.TableNoTracking
                .Where(c => !c.IsDeleted && pageCharityIds.Contains(c.Id))
                .ToDictionaryAsync(c => c.Id, c => c.Name, cancellationToken);

        return new ReportPagedResult<OrphansWithoutPaymentListDto>
        {
            Items = pageRows.Select(r => new OrphansWithoutPaymentListDto
            {
                OrphanId = r.OrphanId,
                OrphanCode = r.OrphanCode,
                OrphanName = r.OrphanName,
                CharityName = r.CharityId.HasValue && names.TryGetValue(r.CharityId.Value, out var name)
                    ? name
                    : null,
                Amount = r.Amount,
                IsStopped = r.IsStopped
            }).ToList(),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    /// <inheritdoc />
    public async Task<PaymentsOutcomeReportDto> GetPaymentsOutcomeAsync(
        PaymentsOutcomeFilterDto filter,
        CancellationToken cancellationToken = default)
    {
        await _paymentsOutcomeValidator.ValidateAndThrowAsync(filter, cancellationToken);

        // Canonical variant (the validator only checked membership, not casing).
        var variant = filter.Variant.ToLowerInvariant();

        var empty = new PaymentsOutcomeReportDto
        {
            Variant = variant,
            BatchNo = filter.OrpCheckBatchNo.Trim(),
            Rows = new List<PaymentsOutcomeRowDto>(),
            TotalCount = 0,
            TotalAmount = 0m
        };

        // The batch's groups — BatchNo is the discriminator; several groups may share one.
        var batchNo = filter.OrpCheckBatchNo.Trim();
        var groups = await _orphanPaymentRepository.TableNoTracking
            .Where(g => !g.IsDeleted && g.BatchNo == batchNo)
            .OrderBy(g => g.GroupDate)
            .ToListAsync(cancellationToken);
        if (groups.Count == 0)
        {
            empty.Message = "Batch not found";
            return empty;
        }
        var groupIds = groups.Select(g => g.Id).ToList();

        // Scope the orphan set (the 18-22 charity-rooted ladder, verbatim): claim pin → HQ
        // narrow → country pin. Pin-never-widen; this endpoint serves charity callers too,
        // so the pin is the primary scope for them, not defence-in-depth.
        var orphanQuery = _orphanRepository.TableNoTracking
            .Where(o => !o.IsDeleted && o.FK_CharityId != null);
        Guid? effectiveCharityId = null;
        if (_currentUser.CharityId.HasValue)
        {
            var pinned = _currentUser.CharityId.Value;
            effectiveCharityId = pinned;
            orphanQuery = orphanQuery.Where(o => o.FK_CharityId == pinned);
        }
        else if (_currentUser.IsHeadOffice && filter.CharityId.HasValue)
        {
            var narrowed = filter.CharityId.Value;
            effectiveCharityId = narrowed;
            orphanQuery = orphanQuery.Where(o => o.FK_CharityId == narrowed);
        }
        FailClosedWhenUnscoped();

        if (_currentUser.CountryId.HasValue)
        {
            var countryId = _currentUser.CountryId.Value;
            var countryCharityIds = _charityRepository.TableNoTracking
                .Where(c => !c.IsDeleted && c.CountryId == countryId)
                .Select(c => c.Id);
            orphanQuery = orphanQuery.Where(o => countryCharityIds.Contains(o.FK_CharityId!.Value));
        }
        var scopedOrphanIds = orphanQuery.Select(o => o.Id);

        // The three variants over ONE dataset definition (§15.U.18–20): received = IsGotIt,
        // not-received = !IsGotIt, stopped = IsStopped. No divergent predicates — the numbers
        // cannot disagree between the lists.
        var itemsQuery = _paymentItemRepository.TableNoTracking
            .Where(i => !i.IsDeleted
                && groupIds.Contains(i.OrphanPaymentId)
                && scopedOrphanIds.Contains(i.OrphanId));
        itemsQuery = variant switch
        {
            "received" => itemsQuery.Where(i => i.IsGotIt),
            "stopped" => itemsQuery.Where(i => i.IsStopped),
            // canonical lowercase of the wire value "notReceived" — the frontend type is
            // 'received' | 'notReceived' | 'stopped' (report.model.ts).
            "notreceived" => itemsQuery.Where(i => !i.IsGotIt),
            // Review P9 2026-08-26: the default arm used to swallow ANY unknown string as
            // not-received — a typo silently produced the wrong list instead of a 400.
            _ => throw new FluentValidation.ValidationException(
                $"Unknown payments-outcome variant '{filter.Variant}'; expected received, notReceived, or stopped.")
        };

        var rows = await itemsQuery
            .OrderBy(i => i.Orphan!.Code)
            .ThenBy(i => i.Id)
            .Select(i => new PaymentsOutcomeRowDto
            {
                OrphanId = i.OrphanId,
                OrphanCode = i.Orphan!.Code,
                OrphanName = i.Orphan!.FullName,
                // §11.S.2 multi-guardian: the PRIMARY guardian (first live row) names the row
                GuardianName = i.Orphan!.Family!.Providers
                    .Where(p => !p.IsDeleted)
                    .OrderBy(p => p.CreatedOn).ThenBy(p => p.Id)
                    .Select(p => p.FullName)
                    .FirstOrDefault(),
                Amount = i.Amount ?? 0m,
                ChiqueNo = i.ChiqueNum,
                PrintDate = i.Printdate,
                CollectorName = i.BenificiaryName
            })
            .ToListAsync(cancellationToken);

        // Header — the batch's first group carries the period/currency; mixing-currency
        // batches report the first (recorded limitation).
        var first = groups[0];
        string? charityName = null;
        if (effectiveCharityId.HasValue)
        {
            charityName = await _charityRepository.TableNoTracking
                .Where(c => !c.IsDeleted && c.Id == effectiveCharityId.Value)
                .Select(c => c.Name)
                .FirstOrDefaultAsync(cancellationToken);
        }

        var report = new PaymentsOutcomeReportDto
        {
            Variant = variant,
            BatchNo = batchNo,
            CharityName = charityName,
            PeriodFrom = first.PaymentPeriodFrom,
            PeriodTo = first.PaymentPeriodTo,
            Currency = first.Currency,
            Rows = rows,
            TotalCount = rows.Count,
            TotalAmount = rows.Sum(r => r.Amount)
        };

        // AC 4 — nothing to produce surfaces as rows:[] + message, never an empty document.
        if (rows.Count == 0)
        {
            report.Message = "No rows in this variant for the requested scope";
        }

        return report;
    }

    /// <inheritdoc />
    public async Task<ChequeNumbersReportDto> GetChequeNumbersAsync(
        ChequeNumbersFilterDto filter,
        CancellationToken cancellationToken = default)
    {
        await _chequeNumbersValidator.ValidateAndThrowAsync(filter, cancellationToken);

        var empty = new ChequeNumbersReportDto
        {
            BatchNo = filter.OrpCheckBatchNo.Trim(),
            Rows = new List<ChequeNumbersRowDto>(),
            TotalCount = 0,
            TotalAmount = 0m
        };

        // Batch resolution mirrors 18-29: BatchNo string, live groups, GroupDate order.
        var batchNo = filter.OrpCheckBatchNo.Trim();
        var groups = await _orphanPaymentRepository.TableNoTracking
            .Where(g => !g.IsDeleted && g.BatchNo == batchNo)
            .OrderBy(g => g.GroupDate)
            .ToListAsync(cancellationToken);
        if (groups.Count == 0)
        {
            empty.Message = "Batch not found";
            return empty;
        }
        var groupIds = groups.Select(g => g.Id).ToList();

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
        else if (_currentUser.IsHeadOffice && filter.CharityId.HasValue)
        {
            var narrowed = filter.CharityId.Value;
            effectiveCharityId = narrowed;
            orphanQuery = orphanQuery.Where(o => o.FK_CharityId == narrowed);
        }
        FailClosedWhenUnscoped();

        if (_currentUser.CountryId.HasValue)
        {
            var countryId = _currentUser.CountryId.Value;
            var countryCharityIds = _charityRepository.TableNoTracking
                .Where(c => !c.IsDeleted && c.CountryId == countryId)
                .Select(c => c.Id);
            orphanQuery = orphanQuery.Where(o => countryCharityIds.Contains(o.FK_CharityId!.Value));
        }
        var scopedOrphanIds = orphanQuery.Select(o => o.Id);

        // §23.U.30 — only rows with a cheque number recorded (10-12's settlement); the
        // data-source ruling: payment-row ChiqueNum, NOT a join against the checks register
        // (Check has no batch link — 18-33's dimension, not this sheet's).
        var rows = await _paymentItemRepository.TableNoTracking
            .Where(i => !i.IsDeleted
                && groupIds.Contains(i.OrphanPaymentId)
                && scopedOrphanIds.Contains(i.OrphanId)
                && i.ChiqueNum != null && i.ChiqueNum != string.Empty)
            .OrderBy(i => i.Orphan!.Code)
            .ThenBy(i => i.Id)
            .Select(i => new ChequeNumbersRowDto
            {
                OrphanId = i.OrphanId,
                OrphanCode = i.Orphan!.Code,
                OrphanName = i.Orphan!.FullName,
                // §11.S.2 multi-guardian: the PRIMARY guardian (first live row) names the row
                GuardianName = i.Orphan!.Family!.Providers
                    .Where(p => !p.IsDeleted)
                    .OrderBy(p => p.CreatedOn).ThenBy(p => p.Id)
                    .Select(p => p.FullName)
                    .FirstOrDefault(),
                Amount = i.Amount ?? 0m,
                ChiqueNo = i.ChiqueNum,
                PrintDate = i.Printdate,
                CollectorName = i.BenificiaryName
            })
            .ToListAsync(cancellationToken);

        var first = groups[0];
        string? charityName = null;
        if (effectiveCharityId.HasValue)
        {
            charityName = await _charityRepository.TableNoTracking
                .Where(c => !c.IsDeleted && c.Id == effectiveCharityId.Value)
                .Select(c => c.Name)
                .FirstOrDefaultAsync(cancellationToken);
        }

        var report = new ChequeNumbersReportDto
        {
            BatchNo = batchNo,
            CharityName = charityName,
            PeriodFrom = first.PaymentPeriodFrom,
            PeriodTo = first.PaymentPeriodTo,
            Currency = first.Currency,
            Rows = rows,
            TotalCount = rows.Count,
            TotalAmount = rows.Sum(r => r.Amount)
        };

        // AC 4 — no cheque recorded surfaces as rows:[] + message, never an empty sheet.
        if (rows.Count == 0)
        {
            report.Message = "No cheques recorded for this batch in the requested scope";
        }

        return report;
    }

    /// <inheritdoc />
    public async Task<ReceiptCardsReportDto> GetReceiptCardsAsync(
        ReceiptCardsFilterDto filter,
        CancellationToken cancellationToken = default)
    {
        await _receiptCardsValidator.ValidateAndThrowAsync(filter, cancellationToken);

        var empty = new ReceiptCardsReportDto
        {
            BatchNo = filter.OrpCheckBatchNo.Trim(),
            Rows = new List<ReceiptCardDto>(),
            TotalCount = 0,
            TotalAmount = 0m
        };

        // Batch resolution mirrors 18-30: BatchNo string, live groups, GroupDate order.
        var batchNo = filter.OrpCheckBatchNo.Trim();
        var groups = await _orphanPaymentRepository.TableNoTracking
            .Where(g => !g.IsDeleted && g.BatchNo == batchNo)
            .OrderBy(g => g.GroupDate)
            .ToListAsync(cancellationToken);
        if (groups.Count == 0)
        {
            empty.Message = "Batch not found";
            return empty;
        }
        var groupIds = groups.Select(g => g.Id).ToList();

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
        else if (_currentUser.IsHeadOffice && filter.CharityId.HasValue)
        {
            var narrowed = filter.CharityId.Value;
            effectiveCharityId = narrowed;
            orphanQuery = orphanQuery.Where(o => o.FK_CharityId == narrowed);
        }
        FailClosedWhenUnscoped();

        if (_currentUser.CountryId.HasValue)
        {
            var countryId = _currentUser.CountryId.Value;
            var countryCharityIds = _charityRepository.TableNoTracking
                .Where(c => !c.IsDeleted && c.CountryId == countryId)
                .Select(c => c.Id);
            orphanQuery = orphanQuery.Where(o => countryCharityIds.Contains(o.FK_CharityId!.Value));
        }
        var scopedOrphanIds = orphanQuery.Select(o => o.Id);

        // §23.U.31 — every payment row of the batch in scope is a card (a guardian with several
        // orphans signs one card per orphan); unlike 18-30 there is no cheque-recorded filter.
        // Read-only on the printed flags: IsPrinted/PrintedOn are the payments vertical's stamps.
        var rows = await _paymentItemRepository.TableNoTracking
            .Where(i => !i.IsDeleted
                && groupIds.Contains(i.OrphanPaymentId)
                && scopedOrphanIds.Contains(i.OrphanId))
            .OrderBy(i => i.Orphan!.Code)
            .ThenBy(i => i.Id)
            .Select(i => new ReceiptCardDto
            {
                OrphanId = i.OrphanId,
                OrphanCode = i.Orphan!.Code,
                OrphanName = i.Orphan!.FullName,
                // §11.S.2 multi-guardian: the PRIMARY guardian (first live row) names the row
                GuardianName = i.Orphan!.Family!.Providers
                    .Where(p => !p.IsDeleted)
                    .OrderBy(p => p.CreatedOn).ThenBy(p => p.Id)
                    .Select(p => p.FullName)
                    .FirstOrDefault(),
                Amount = i.Amount ?? 0m,
                ChiqueNo = i.ChiqueNum
            })
            .ToListAsync(cancellationToken);

        var first = groups[0];
        string? charityName = null;
        if (effectiveCharityId.HasValue)
        {
            charityName = await _charityRepository.TableNoTracking
                .Where(c => !c.IsDeleted && c.Id == effectiveCharityId.Value)
                .Select(c => c.Name)
                .FirstOrDefaultAsync(cancellationToken);
        }

        var report = new ReceiptCardsReportDto
        {
            BatchNo = batchNo,
            CharityName = charityName,
            PeriodFrom = first.PaymentPeriodFrom,
            PeriodTo = first.PaymentPeriodTo,
            Currency = first.Currency,
            Rows = rows,
            TotalCount = rows.Count,
            TotalAmount = rows.Sum(r => r.Amount)
        };

        // AC 3 — nothing to produce surfaces as rows:[] + message, never an empty document.
        if (rows.Count == 0)
        {
            report.Message = "No payment rows for this batch in the requested scope";
        }

        return report;
    }

    /// <inheritdoc />
    public async Task<NewBeneficiariesReportDto> GetNewBeneficiariesAsync(
        NewBeneficiariesFilterDto filter,
        CancellationToken cancellationToken = default)
    {
        await _newBeneficiariesValidator.ValidateAndThrowAsync(filter, cancellationToken);

        // Canonical variant (the validator only checked membership, not casing).
        var variant = filter.Variant.Trim().ToLowerInvariant();

        // The registration window — dateFrom inclusive, dateTo inclusive (day granularity).
        var from = filter.DateFrom.Date;
        var toExclusive = filter.DateTo.HasValue
            ? ExclusiveUpperBound(filter.DateTo.Value.Date)
            : DateTime.MaxValue.Date;

        if (variant is "orphans" or "orphansv2")
        {
            // Data-source ruling (recorded): no dedicated registration column exists on Orphan —
            // CreatedOn (audit) is the proxy for "registered in the window".
            var query = _orphanRepository.TableNoTracking
                .Where(o => !o.IsDeleted && o.FK_CharityId != null
                    && o.CreatedOn >= from && o.CreatedOn < toExclusive);

            // The 18-22/24 charity-rooted ladder, verbatim: claim pin → HQ narrow → country pin.
            if (_currentUser.CharityId.HasValue)
            {
                var pinned = _currentUser.CharityId.Value;
                query = query.Where(o => o.FK_CharityId == pinned);
            }
            else if (_currentUser.IsHeadOffice && filter.CharityId.HasValue)
            {
                var narrowed = filter.CharityId.Value;
                query = query.Where(o => o.FK_CharityId == narrowed);
            }
            FailClosedWhenUnscoped();

            if (_currentUser.CountryId.HasValue)
            {
                var countryId = _currentUser.CountryId.Value;
                var countryCharityIds = _charityRepository.TableNoTracking
                    .Where(c => !c.IsDeleted && c.CountryId == countryId)
                    .Select(c => c.Id);
                query = query.Where(o => countryCharityIds.Contains(o.FK_CharityId!.Value));
            }

            var totalCount = await query.CountAsync(cancellationToken);
            var rows = await query
                .OrderBy(o => o.CreatedOn)
                .ThenBy(o => o.Code)
                .Skip(PageSkip(filter.Page, filter.PageSize))
                .Take(filter.PageSize)
                .Select(o => new NewBeneficiaryListDto
                {
                    OrphanCode = o.Code,
                    OrphanName = o.FullName,
                    BirthDate = o.DateOfBirth,
                    FamilyCode = o.Family != null ? o.Family.Code : null,
                    GuardianName = o.Family != null
                        ? o.Family.Providers
                            .Where(p => !p.IsDeleted)
                            .OrderBy(p => p.CreatedOn).ThenBy(p => p.Id)
                            .Select(p => p.FullName)
                            .FirstOrDefault()
                        : null,
                    CharityId = o.FK_CharityId,
                    RegistrationDate = o.CreatedOn
                })
                .ToListAsync(cancellationToken);

            await ResolveCharityNamesAsync(rows, r => r.CharityId, (r, name) => r.CharityName = name, cancellationToken);

            return new NewBeneficiariesReportDto
            {
                Variant = variant,
                Items = rows,
                TotalCount = totalCount,
                Page = filter.Page,
                PageSize = filter.PageSize
            };
        }
        else
        {
            // Widow variants (§18-6's widow definition): the widow IS the family's mother —
            // mother alive + husband death date present; "new" = the FAMILY registered in the
            // window (Family.CreatedOn proxy, same recorded ruling). The has-orphans floor is
            // dropped: the family's registration is the frame, not its member count.
            var query = _motherRepository.TableNoTracking
                .Where(m => !m.IsDeleted
                    && m.IsAlive
                    && m.Family != null
                    && !m.Family.IsDeleted
                    && m.Family.FK_CharityId != null
                    && m.Family.Father != null
                    && !m.Family.Father.IsDeleted
                    && m.Family.Father.DeathDate != null
                    && m.Family.CreatedOn >= from
                    && m.Family.CreatedOn < toExclusive);

            query = await ApplyMotherCharityScopeAsync(query, filter.CharityId, cancellationToken);

            var totalCount = await query.CountAsync(cancellationToken);
            var rows = await query
                .OrderBy(m => m.Family!.CreatedOn)
                .ThenBy(m => m.Family!.Code)
                .Skip(PageSkip(filter.Page, filter.PageSize))
                .Take(filter.PageSize)
                .Select(m => new NewBeneficiaryListDto
                {
                    WidowName = m.FullName,
                    NationalId = m.NationalId,
                    HusbandDeathDate = m.Family!.Father!.DeathDate,
                    FamilyCode = m.Family.Code,
                    ChildrenCount = m.Family.Orphans.Count(o => !o.IsDeleted),
                    CharityId = m.Family.FK_CharityId,
                    RegistrationDate = m.Family.CreatedOn
                })
                .ToListAsync(cancellationToken);

            await ResolveCharityNamesAsync(rows, r => r.CharityId, (r, name) => r.CharityName = name, cancellationToken);

            return new NewBeneficiariesReportDto
            {
                Variant = variant,
                Items = rows,
                TotalCount = totalCount,
                Page = filter.Page,
                PageSize = filter.PageSize
            };
        }
    }

    /// <inheritdoc />
    public async Task<ReportPagedResult<FollowUpSheetRowDto>> GetFollowUpSheetsAsync(
        FollowUpSheetFilterDto filter,
        CancellationToken cancellationToken = default)
    {
        await _followUpSheetsValidator.ValidateAndThrowAsync(filter, cancellationToken);

        // The optional registration window (inclusive both ends, day granularity) — the row
        // ANCHOR's CreatedOn (the 18-35 proxy ruling; no dedicated registration column exists).
        var from = filter.DateFrom?.Date;
        var toExclusive = filter.DateTo.HasValue
            ? ExclusiveUpperBound(filter.DateTo.Value.Date)
            : (DateTime?)null;

        if (filter.Variant == FollowUpSheetVariant.FollowUpFamily)
        {
            // متابعة الأسر (rptFollowUpFamily) — one row per family.
            var query = _familyRepository.TableNoTracking
                .Where(f => !f.IsDeleted && f.FK_CharityId != null);

            if (from.HasValue)
            {
                query = query.Where(f => f.CreatedOn >= from.Value);
            }
            if (toExclusive.HasValue)
            {
                query = query.Where(f => f.CreatedOn < toExclusive.Value);
            }

            // The 18-22/24 charity-rooted ladder, verbatim (family-rooted form).
            if (_currentUser.CharityId.HasValue)
            {
                var pinned = _currentUser.CharityId.Value;
                query = query.Where(f => f.FK_CharityId == pinned);
            }
            else if (_currentUser.IsHeadOffice && filter.CharityId.HasValue)
            {
                var narrowed = filter.CharityId.Value;
                query = query.Where(f => f.FK_CharityId == narrowed);
            }
            FailClosedWhenUnscoped();

            if (_currentUser.CountryId.HasValue)
            {
                var countryId = _currentUser.CountryId.Value;
                var countryCharityIds = _charityRepository.TableNoTracking
                    .Where(c => !c.IsDeleted && c.CountryId == countryId)
                    .Select(c => c.Id);
                query = query.Where(f => countryCharityIds.Contains(f.FK_CharityId!.Value));
            }

            var familyTotal = await query.CountAsync(cancellationToken);
            var familyRows = await query
                .OrderBy(f => f.Code)
                .Skip(PageSkip(filter.Page, filter.PageSize))
                .Take(filter.PageSize)
                .Select(f => new FollowUpSheetRowDto
                {
                    FamilyCode = f.Code,
                    HeadOfFamily = f.HeadOfFamily,
                    OrphansCount = f.Orphans.Count(o => !o.IsDeleted),
                    FamilyStatus = f.FamilyStatus,
                    RegistrationDate = f.CreatedOn,
                    LastUpdate = f.UpdatedOn,
                    CharityId = f.FK_CharityId
                })
                .ToListAsync(cancellationToken);

            await ResolveCharityNamesAsync(familyRows, r => r.CharityId, (r, name) => r.CharityName = name, cancellationToken);

            return new ReportPagedResult<FollowUpSheetRowDto>
            {
                Items = familyRows,
                TotalCount = familyTotal,
                Page = filter.Page,
                PageSize = filter.PageSize
            };
        }
        else
        {
            // متابعة / تسليم (rptFollowUp / rptFollowUpTasleem) — one row per orphan; the two
            // variants share this row source, only the screen's column set differs (the 18-35
            // variant-collapse convention).
            var query = _orphanRepository.TableNoTracking
                .Where(o => !o.IsDeleted && o.FK_CharityId != null);

            if (from.HasValue)
            {
                query = query.Where(o => o.CreatedOn >= from.Value);
            }
            if (toExclusive.HasValue)
            {
                query = query.Where(o => o.CreatedOn < toExclusive.Value);
            }

            // The 18-22/24 charity-rooted ladder, verbatim (orphan-rooted form).
            if (_currentUser.CharityId.HasValue)
            {
                var pinned = _currentUser.CharityId.Value;
                query = query.Where(o => o.FK_CharityId == pinned);
            }
            else if (_currentUser.IsHeadOffice && filter.CharityId.HasValue)
            {
                var narrowed = filter.CharityId.Value;
                query = query.Where(o => o.FK_CharityId == narrowed);
            }
            FailClosedWhenUnscoped();

            if (_currentUser.CountryId.HasValue)
            {
                var countryId = _currentUser.CountryId.Value;
                var countryCharityIds = _charityRepository.TableNoTracking
                    .Where(c => !c.IsDeleted && c.CountryId == countryId)
                    .Select(c => c.Id);
                query = query.Where(o => countryCharityIds.Contains(o.FK_CharityId!.Value));
            }

            var totalCount = await query.CountAsync(cancellationToken);

            // OrphanId rides the page projection (never the wire DTO) for the LastReportDate stitch.
            var page = await query
                .OrderBy(o => o.Code)
                .Skip(PageSkip(filter.Page, filter.PageSize))
                .Take(filter.PageSize)
                .Select(o => new
                {
                    Row = new FollowUpSheetRowDto
                    {
                        OrphanCode = o.Code,
                        OrphanName = o.FullName,
                        GuardianName = o.Family != null
                            ? (o.Family.Providers.Where(p => !p.IsDeleted)
                                    .OrderBy(p => p.CreatedOn).ThenBy(p => p.Id)
                                    .Select(p => p.FullName).FirstOrDefault()
                                ?? o.Family.HeadOfFamily)
                            : null,
                        FamilyCode = o.Family != null ? o.Family.Code : null,
                        CharityId = o.FK_CharityId,
                        SponsorshipStatus = o.SponsorshipStatus,
                        MonthlyAmount = o.MonthlyAmount
                    },
                    OrphanId = o.Id
                })
                .ToListAsync(cancellationToken);

            var rows = page.Select(p => p.Row).ToList();

            // متابعة only: تاريخ آخر تقرير — the latest ACCEPTED periodic report per orphan
            // (BR-11: an accepted report is the only clearing state; pending/refused never count).
            if (filter.Variant == FollowUpSheetVariant.FollowUp && page.Count > 0)
            {
                var orphanIds = page.Select(p => p.OrphanId).ToList();
                var lastByOrphan = (await _periodicReportRepository.TableNoTracking
                        .Where(r => !r.IsDeleted && r.IsAccepted && orphanIds.Contains(r.OrphanId))
                        .GroupBy(r => r.OrphanId)
                        .Select(g => new { OrphanId = g.Key, Last = g.Max(r => r.ReportDate) })
                        .ToListAsync(cancellationToken))
                    .ToDictionary(x => x.OrphanId, x => x.Last);

                for (var i = 0; i < rows.Count; i++)
                {
                    if (lastByOrphan.TryGetValue(orphanIds[i], out var last))
                    {
                        rows[i].LastReportDate = last;
                    }
                }
            }

            await ResolveCharityNamesAsync(rows, r => r.CharityId, (r, name) => r.CharityName = name, cancellationToken);

            return new ReportPagedResult<FollowUpSheetRowDto>
            {
                Items = rows,
                TotalCount = totalCount,
                Page = filter.Page,
                PageSize = filter.PageSize
            };
        }
    }

    /// <inheritdoc />
    public async Task<GuardianIdentificationSheetDto> GetGuardianIdentificationSheetsAsync(
        GuardianIdentificationSheetFilterDto filter,
        CancellationToken cancellationToken = default)
    {
        await _guardianIdentificationValidator.ValidateAndThrowAsync(filter, cancellationToken);

        var rows = new List<GuardianIdentificationRowDto>();

        if (filter.Variant is GuardianIdentificationSheetVariant.WidowsOnly
            or GuardianIdentificationSheetVariant.SingleFamily)
        {
            // الأرملة — the 18-6/18-35 widow definition: mother alive, family registered,
            // husband (Father) death date present.
            var mothers = _motherRepository.TableNoTracking
                .Where(m => !m.IsDeleted && m.IsAlive
                    && m.Family != null && !m.Family.IsDeleted
                    && m.Family.FK_CharityId != null
                    && m.Family.Father != null && !m.Family.Father.IsDeleted
                    && m.Family.Father.DeathDate != null);

            if (filter.Variant == GuardianIdentificationSheetVariant.SingleFamily)
            {
                // The explicit family choice is the frame — the start date does not cut its members.
                var familyId = filter.FamilyId!.Value;
                mothers = mothers.Where(m => m.Family!.Id == familyId);
            }
            else if (filter.Date.HasValue)
            {
                // تاريخ بدء التقرير — register entries created on/after the date.
                var from = filter.Date.Value.Date;
                mothers = mothers.Where(m => m.CreatedOn >= from);
            }

            mothers = await ApplyMotherCharityScopeAsync(mothers, filter.CharityId, cancellationToken);

            var widowRows = await mothers
                .OrderBy(m => m.Family!.Code)
                .Select(m => new GuardianIdentificationRowDto
                {
                    WidowName = m.FullName,
                    NationalId = m.NationalId,
                    Phone = m.Phone,
                    DateOfBirth = m.DateOfBirth,
                    FamilyCode = m.Family!.Code,
                    CharityId = m.Family.FK_CharityId
                })
                .ToListAsync(cancellationToken);

            rows.AddRange(widowRows);
        }

        if (filter.Variant is GuardianIdentificationSheetVariant.AllGuardians
            or GuardianIdentificationSheetVariant.SingleFamily)
        {
            var providers = _providerRepository.TableNoTracking
                .Where(p => !p.IsDeleted
                    && p.Family != null && !p.Family.IsDeleted
                    && p.Family.FK_CharityId != null);

            if (filter.Variant == GuardianIdentificationSheetVariant.SingleFamily)
            {
                var familyId = filter.FamilyId!.Value;
                providers = providers.Where(p => p.Family!.Id == familyId);
            }
            else if (filter.Date.HasValue)
            {
                var from = filter.Date.Value.Date;
                providers = providers.Where(p => p.CreatedOn >= from);
            }

            // Provider-rooted charity ladder (the 18-22/24 form, mirror of the mother helper).
            if (_currentUser.CharityId.HasValue)
            {
                var pinned = _currentUser.CharityId.Value;
                providers = providers.Where(p => p.Family!.FK_CharityId == pinned);
            }
            else if (_currentUser.IsHeadOffice && filter.CharityId.HasValue)
            {
                var narrowed = filter.CharityId.Value;
                providers = providers.Where(p => p.Family!.FK_CharityId == narrowed);
            }
            FailClosedWhenUnscoped();

            if (_currentUser.CountryId.HasValue)
            {
                var countryId = _currentUser.CountryId.Value;
                var countryCharityIds = _charityRepository.TableNoTracking
                    .Where(c => !c.IsDeleted && c.CountryId == countryId)
                    .Select(c => c.Id);
                providers = providers.Where(p => countryCharityIds.Contains(p.Family!.FK_CharityId!.Value));
            }

            var guardianRows = await providers
                .OrderBy(p => p.Family!.Code)
                .Select(p => new GuardianIdentificationRowDto
                {
                    GuardianName = p.FullName,
                    NationalId = p.NationalId,
                    Phone = p.Phone,
                    RelationshipToFamily = p.RelationshipToFamily,
                    Job = p.Job,
                    FamilyCode = p.Family!.Code,
                    CharityId = p.Family.FK_CharityId
                })
                .ToListAsync(cancellationToken);

            rows.AddRange(guardianRows);
        }

        // أسرة محددة's union reads family-grouped — guardians and widows under the family code.
        rows = rows
            .OrderBy(r => r.FamilyCode)
            .ThenBy(r => r.GuardianName ?? r.WidowName ?? string.Empty)
            .ToList();

        // Review P14 2026-08-26: bound the sheet — same ceiling + Truncated probe as
        // ReportSheetService, applied once over the ordered union.
        var truncated = rows.Count > GuardianSheetRowCap;
        if (truncated)
        {
            rows = rows.Take(GuardianSheetRowCap).ToList();
        }

        await ResolveCharityNamesAsync(rows, r => r.CharityId, (r, name) => r.CharityName = name, cancellationToken);

        return new GuardianIdentificationSheetDto
        {
            Variant = filter.Variant.ToString(),
            // Stamped from the token — the legacy userId payload key is ruled out.
            ProducedBy = _currentUser.UserId?.ToString(),
            ProducedOn = DateTime.UtcNow,
            TotalCount = rows.Count,
            Rows = rows,
            Truncated = truncated,
            Message = rows.Count == 0 ? "No rows in the requested scope" : null
        };
    }

    /// <inheritdoc />
    public async Task<ReportPagedResult<MissingOutgoingAttachmentsRowDto>> GetMissingOutgoingAttachmentsAsync(
        MissingOutgoingAttachmentsFilterDto filter,
        CancellationToken cancellationToken = default)
    {
        await _missingOutgoingAttachmentsValidator.ValidateAndThrowAsync(filter, cancellationToken);

        // The letter's own date window (inclusive both ends, day granularity).
        var from = filter.DateFrom?.Date;
        var toExclusive = filter.DateTo.HasValue
            ? ExclusiveUpperBound(filter.DateTo.Value.Date)
            : (DateTime?)null;

        // v1 rule: an outgoing with ZERO live OrphanReports rows is "missing its attachments" —
        // the legacy expected-list table (ChildOutGoing) is still absent from the domain, so
        // AttachmentCount rides as a constant zero and the grid states the finding.
        var query = _outgoingRepository.TableNoTracking
            .Where(o => !o.IsDeleted && o.FK_CharityId != null
                && !o.OrphanReports.Any(r => !r.IsDeleted));

        if (from.HasValue)
        {
            query = query.Where(o => o.Date >= from.Value);
        }
        if (toExclusive.HasValue)
        {
            query = query.Where(o => o.Date < toExclusive.Value);
        }
        if (filter.OutgoingCategoryId.HasValue)
        {
            var categoryId = filter.OutgoingCategoryId.Value;
            query = query.Where(o => o.OutgoingCategoryId == categoryId);
        }

        // The 18-22/24 charity-rooted ladder, verbatim (outgoing-rooted form).
        if (_currentUser.CharityId.HasValue)
        {
            var pinned = _currentUser.CharityId.Value;
            query = query.Where(o => o.FK_CharityId == pinned);
        }
        else if (_currentUser.IsHeadOffice && filter.CharityId.HasValue)
        {
            var narrowed = filter.CharityId.Value;
            query = query.Where(o => o.FK_CharityId == narrowed);
        }
        FailClosedWhenUnscoped();

        if (_currentUser.CountryId.HasValue)
        {
            var countryId = _currentUser.CountryId.Value;
            var countryCharityIds = _charityRepository.TableNoTracking
                .Where(c => !c.IsDeleted && c.CountryId == countryId)
                .Select(c => c.Id);
            query = query.Where(o => countryCharityIds.Contains(o.FK_CharityId!.Value));
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var rows = await query
            .OrderByDescending(o => o.Date)
            .ThenByDescending(o => o.Serial)
            .Skip(PageSkip(filter.Page, filter.PageSize))
            .Take(filter.PageSize)
            .Select(o => new MissingOutgoingAttachmentsRowDto
            {
                Serial = o.Serial,
                OutGoingNumber = o.OutGoingNumber,
                Subject = o.Subject,
                Date = o.Date,
                Year = o.Year,
                CategoryName = o.Category != null ? (o.Category.NameAr ?? o.Category.NameEn) : null,
                CharityId = o.FK_CharityId,
                AttachmentCount = 0
            })
            .ToListAsync(cancellationToken);

        await ResolveCharityNamesAsync(rows, r => r.CharityId, (r, name) => r.CharityName = name, cancellationToken);

        return new ReportPagedResult<MissingOutgoingAttachmentsRowDto>
        {
            Items = rows,
            TotalCount = totalCount,
            Page = filter.Page,
            PageSize = filter.PageSize
        };
    }

    /// <inheritdoc />
    public async Task<ReportPagedResult<FamilyWithOrphansDto>> GetFamilyOrphansByDateAsync(
        FamilyOrphansByDateFilterDto filter,
        CancellationToken cancellationToken = default)
    {
        await _familyOrphansByDateValidator.ValidateAndThrowAsync(filter, cancellationToken);

        // As-at semantics: the family's DEDICATED RegistrationDate column — Family carries one
        // (verified at dev time), so no CreatedOn proxy is needed here, unlike 18-35. Inclusive
        // on the day: registered on or before the chosen date.
        var asAt = filter.Date.Date;

        var query = _familyRepository.TableNoTracking
            .Where(f => !f.IsDeleted && f.FK_CharityId != null && f.RegistrationDate <= asAt);

        // The 18-22/24 charity-rooted ladder, verbatim (family-rooted form).
        if (_currentUser.CharityId.HasValue)
        {
            var pinned = _currentUser.CharityId.Value;
            query = query.Where(f => f.FK_CharityId == pinned);
        }
        else if (_currentUser.IsHeadOffice && filter.CharityId.HasValue)
        {
            var narrowed = filter.CharityId.Value;
            query = query.Where(f => f.FK_CharityId == narrowed);
        }
        FailClosedWhenUnscoped();

        if (_currentUser.CountryId.HasValue)
        {
            var countryId = _currentUser.CountryId.Value;
            var countryCharityIds = _charityRepository.TableNoTracking
                .Where(c => !c.IsDeleted && c.CountryId == countryId)
                .Select(c => c.Id);
            query = query.Where(f => countryCharityIds.Contains(f.FK_CharityId!.Value));
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var families = await query
            .OrderBy(f => f.Code)
            .Skip(PageSkip(filter.Page, filter.PageSize))
            .Take(filter.PageSize)
            .Select(f => new FamilyWithOrphansDto
            {
                FamilyCode = f.Code,
                HeadOfFamily = f.HeadOfFamily,
                RegionName = f.Region != null ? f.Region.NameAr ?? f.Region.NameEn : null,
                CenterName = f.Center != null ? f.Center.NameAr ?? f.Center.NameEn : null,
                RegistrationDate = f.RegistrationDate,
                Orphans = f.Orphans
                    .Where(o => !o.IsDeleted)
                    .OrderBy(o => o.Code)
                    .Select(o => new FamilyOrphanRowDto
                    {
                        Code = o.Code,
                        FullName = o.FullName,
                        DateOfBirth = o.DateOfBirth
                    })
                    .ToList()
            })
            .ToListAsync(cancellationToken);

        // Age at the as-at date + the per-family count, computed post-fetch — TimeSpan math is
        // not portably translatable, and the count rides the already-materialised list.
        foreach (var family in families)
        {
            family.OrphansCount = family.Orphans.Count;
            foreach (var orphan in family.Orphans)
            {
                // Review P5 2026-08-26: unified birthday-aware age (was TotalDays/365.2425 —
                // a third formula that disagreed with the filter and every other report).
                orphan.Age = CompletedAge(orphan.DateOfBirth, asAt);
            }
        }

        return new ReportPagedResult<FamilyWithOrphansDto>
        {
            Items = families,
            TotalCount = totalCount,
            Page = filter.Page,
            PageSize = filter.PageSize
        };
    }

    /// <summary>
    /// Charity-name resolution shared by the §23.U.35–38 report rows: post-fetch dictionary from
    /// the charity table (HQ runs span charities; Charity.Name resolves NameAr ?? NameEn).
    /// </summary>
    private async Task ResolveCharityNamesAsync<TRow>(
        List<TRow> rows,
        Func<TRow, Guid?> charityIdOf,
        Action<TRow, string?> setName,
        CancellationToken cancellationToken)
    {
        var charityIds = rows
            .Select(charityIdOf)
            .Where(id => id.HasValue)
            .Select(id => id!.Value)
            .Distinct()
            .ToList();
        if (charityIds.Count == 0)
        {
            return;
        }

        var names = await _charityRepository.TableNoTracking
            .Where(c => !c.IsDeleted && charityIds.Contains(c.Id))
            .ToDictionaryAsync(c => c.Id, c => c.Name, cancellationToken);

        foreach (var row in rows)
        {
            var charityId = charityIdOf(row);
            if (charityId.HasValue && names.TryGetValue(charityId.Value, out var name))
            {
                setName(row, name);
            }
        }
    }

    /// <inheritdoc />
    public async Task<ReportPagedResult<ExcludedOrphanListDto>> GetExcludedOrphansAsync(
        ExcludedOrphansFilterDto filter,
        CancellationToken cancellationToken = default)
    {
        await _excludedOrphansValidator.ValidateAndThrowAsync(filter, cancellationToken);

        // Degenerate set (18-1's recorded gap, re-proven for this story): NO exclusion column
        // exists anywhere in the domain (verified — no Exclud* member on any entity or lookup).
        // The excluded set is therefore empty by construction; the write path belongs to EP-08
        // and the columns to a later migration — never this epic (no-migration ruling).
        _logger.LogInformation(
            "Excluded-orphans report (UC-RPT-03): empty — no exclusion columns exist yet (recorded gap); caller {Caller}",
            _currentUser.UserId?.ToString() ?? "anonymous");

        return new ReportPagedResult<ExcludedOrphanListDto>
        {
            Items = new List<ExcludedOrphanListDto>(),
            TotalCount = 0,
            Page = filter.Page,
            PageSize = filter.PageSize
        };
    }

    /// <inheritdoc />
    public async Task<ReportPagedResult<OrphanStatusReportListDto>> GetFinishedSponsorshipOrphansAsync(
        OrphanStatusReportFilterDto filter,
        CancellationToken cancellationToken = default)
    {
        await _orphanStatusReportValidator.ValidateAndThrowAsync(filter, cancellationToken);

        // Product ruling (2026-08-24, recorded): the domain has NO ended-sponsorship state —
        // SponsorshipStatus is a free string (Sponsored/Unsponsored/Pending), there is no
        // sponsorship end date and no sponsor-history entity; the dev DB holds no rows that
        // could represent "ended" under any derivation. The report ships with an EMPTY set +
        // logged gap; when EP-08/EP-10 add the state, the predicate lands in this method only.
        _logger.LogInformation(
            "Finished-sponsorship report (UC-RPT-04): empty — the domain has no ended state yet (recorded gap); caller {Caller}",
            _currentUser.UserId?.ToString() ?? "anonymous");

        return new ReportPagedResult<OrphanStatusReportListDto>
        {
            Items = new List<OrphanStatusReportListDto>(),
            TotalCount = 0,
            Page = filter.Page,
            PageSize = filter.PageSize
        };
    }

    /// <inheritdoc />
    public async Task<ReportPagedResult<OrphanStatusReportListDto>> GetUnsponsoredOrphansAsync(
        OrphanStatusReportFilterDto filter,
        CancellationToken cancellationToken = default)
    {
        await _orphanStatusReportValidator.ValidateAndThrowAsync(filter, cancellationToken);

        // EP-08 ruling: "unsponsored" = CODED orphan with status Unsponsored (the first code
        // assignment flips null/Pending → Unsponsored). An empty Code is UNCODED — never here.
        var query = _orphanRepository.TableNoTracking
            .Where(o => !o.IsDeleted
                && !string.IsNullOrEmpty(o.Code)
                && o.SponsorshipStatus == "Unsponsored");

        query = await ApplyCharityScopeAsync(query, filter.CharityId, cancellationToken);

        var totalCount = await query.CountAsync(cancellationToken);

        var today = DateTime.Today;
        var rows = await query
            .OrderBy(o => o.Code)
            .Skip(PageSkip(filter.Page, filter.PageSize))
            .Take(filter.PageSize)
            .Select(o => new
            {
                o.Code,
                o.FullName,
                CharityId = o.FK_CharityId,
                FamilyCode = o.Family != null ? o.Family.Code : null,
                o.DateOfBirth
            })
            .ToListAsync(cancellationToken);

        // Review P5 2026-08-26: age computed after materialization — DateDiffYear counted
        // calendar-year boundaries and read one year high before the birthday.
        var page = rows
            .Select(r => new OrphanStatusReportListDto
            {
                Code = r.Code,
                FullName = r.FullName,
                CharityId = r.CharityId,
                FamilyCode = r.FamilyCode,
                Age = CompletedAge(r.DateOfBirth, today)
            })
            .ToList();

        await ResolveStatusReportCharityNamesAsync(page, cancellationToken);

        return new ReportPagedResult<OrphanStatusReportListDto>
        {
            Items = page,
            TotalCount = totalCount,
            Page = filter.Page,
            PageSize = filter.PageSize
        };
    }

    /// <inheritdoc />
    public async Task<ReportPagedResult<WidowSponsorshipListDto>> GetWidowsAllowingSponsorshipAsync(
        WidowSponsorshipFilterDto filter,
        CancellationToken cancellationToken = default)
    {
        await _widowSponsorshipValidator.ValidateAndThrowAsync(filter, cancellationToken);

        // Data-source decision (story Dev Notes): no standalone Widow entity exists — the widow
        // IS the family's mother; her residence/income columns live on Family (the epic-7
        // refugee contract's column set). Predicate = the data's floor, recorded: no dedicated
        // widow-sponsorship flag exists, so "requiring sponsorship" resolves to mother alive +
        // registered family + husband death date present + at least one non-deleted orphan.
        var query = _motherRepository.TableNoTracking
            .Where(m => !m.IsDeleted
                && m.IsAlive
                && m.Family != null
                && !m.Family.IsDeleted
                && m.Family.Father != null
                && !m.Family.Father.IsDeleted
                && m.Family.Father.DeathDate != null
                && m.Family.Orphans.Any(o => !o.IsDeleted));

        query = await ApplyMotherCharityScopeAsync(query, filter.CharityId, cancellationToken);

        var totalCount = await query.CountAsync(cancellationToken);

        var rows = await query
            .OrderBy(m => m.FullName)
            .ThenBy(m => m.Family!.Code)
            .Skip(PageSkip(filter.Page, filter.PageSize))
            .Take(filter.PageSize)
            .Select(m => new WidowSponsorshipListDto
            {
                WidowName = m.FullName,
                GovernorateName = m.Family!.Region != null
                    ? (m.Family.Region.NameAr ?? m.Family.Region.NameEn) : null,
                CenterName = m.Family.Center != null
                    ? (m.Family.Center.NameAr ?? m.Family.Center.NameEn) : null,
                CityVillage = m.Family.CityVillage,
                DetailedAddress = m.Family.Address,
                MobileNumber = m.Phone,
                // No backing column — recorded gap, renders blank.
                MobileNumber2 = null,
                HouseOwnershipName = m.Family.HouseOwnership != null
                    ? (m.Family.HouseOwnership.NameAr ?? m.Family.HouseOwnership.NameEn) : null,
                RentAmount = m.Family.RentAmount,
                HousingTypeName = m.Family.HousingType != null
                    ? (m.Family.HousingType.NameAr ?? m.Family.HousingType.NameEn) : null,
                HouseStatusName = m.Family.HouseStatus != null
                    ? (m.Family.HouseStatus.NameAr ?? m.Family.HouseStatus.NameEn) : null,
                MonthlyIncome = m.Family.MonthlyIncome,
                HusbandDeathDate = m.Family.Father!.DeathDate,
                // No backing column — recorded gap (مشروع تنموي), renders blank.
                DevelopmentProject = null,
                EducationLevelName = m.EducationLevel != null
                    ? (m.EducationLevel.NameAr ?? m.EducationLevel.NameEn) : null,
                Profession = m.Job,
                HealthStatusName = m.HealthStatus != null
                    ? (m.HealthStatus.NameAr ?? m.HealthStatus.NameEn) : null,
                NationalId = m.NationalId,
                Notes = m.Notes,
                CharityId = m.Family.FK_CharityId,
                LastUpdatedDate = m.UpdatedOn
            })
            .ToListAsync(cancellationToken);

        await ResolveWidowCharityNamesAsync(rows, cancellationToken);

        return new ReportPagedResult<WidowSponsorshipListDto>
        {
            Items = rows,
            TotalCount = totalCount,
            Page = filter.Page,
            PageSize = filter.PageSize
        };
    }

    /// <inheritdoc />
    public async Task<ReportPagedResult<MezaCardsListDto>> GetMezaCardsAsync(
        MezaCardsFilterDto filter,
        CancellationToken cancellationToken = default)
    {
        await _mezaCardsValidator.ValidateAndThrowAsync(filter, cancellationToken);

        // Recorded product ruling (same as UC-RPT-03/04): the row source is FAMILIES CARRYING
        // registered guardian Meza cards (AC 4), and the domain carries no Meza/card column
        // anywhere (grep-proven 2026-08-24 — no Meza|Card member on any entity or lookup).
        // No family can therefore be a card carrier today: the set is empty by construction and
        // the gap is logged. When the guardian registration gains the card fields, the
        // family+guardian+children projection and card predicate land HERE ALONE — the DTO
        // contract already carries the keys, so no screen/export change will be needed.
        _logger.LogInformation(
            "Meza-cards report (UC-RPT-07): empty — no Meza/card column exists in the domain yet (recorded gap); caller {Caller}",
            _currentUser.UserId?.ToString() ?? "anonymous");

        return new ReportPagedResult<MezaCardsListDto>
        {
            Items = new List<MezaCardsListDto>(),
            TotalCount = 0,
            Page = filter.Page,
            PageSize = filter.PageSize
        };
    }

    /// <inheritdoc />
    public async Task<ReportPagedResult<MezaCardsListDto>> GetMezaCardsForExportAsync(
        MezaCardsFilterDto filter,
        CancellationToken cancellationToken = default)
    {
        await _mezaCardsValidator.ValidateAndThrowAsync(filter, cancellationToken);

        // 18-7's recorded gap carries over verbatim (Task 2 ruling): the row source is families
        // carrying registered guardian Meza cards and NO Meza/card column exists domain-wide
        // (grep-proven) — the extract is empty by construction with the gap logged. When the
        // registration vertical adds card storage, the family+guardian+children projection AND
        // the extract predicates (ReportNo/BatchId/date-range/MezaCardExist) land HERE ALONE,
        // returning the FULL selection (the bank file is never page 1).
        _logger.LogInformation(
            "Meza-cards extract (UC-RPT-08, report {ReportNo}): empty — no Meza/card column exists in the domain yet (recorded gap); caller {Caller}",
            filter.ReportNo, _currentUser.UserId?.ToString() ?? "anonymous");

        return new ReportPagedResult<MezaCardsListDto>
        {
            Items = new List<MezaCardsListDto>(),
            TotalCount = 0,
            Page = 1,
            // Never 0 — TotalPages divides by it (divide-by-zero on serialise caught in smoke).
            PageSize = filter.PageSize
        };
    }

    /// <inheritdoc />
    public async Task<ReportPagedResult<BeneficiaryFamilyListDto>> GetBeneficiaryFamiliesAsync(
        BeneficiaryFamilyFilterDto filter,
        CancellationToken cancellationToken = default)
    {
        await _beneficiaryFamilyValidator.ValidateAndThrowAsync(filter, cancellationToken);

        // No global soft-delete filter on this platform — filter explicitly at every level.
        var query = _beneficiaryRepository.TableNoTracking
            .Where(b => !b.IsDeleted
                && b.Family != null
                && !b.Family.IsDeleted
                && b.Campaign != null
                && !b.Campaign.IsDeleted);

        query = await ApplyBeneficiaryCharityScopeAsync(query, filter.CharityId, cancellationToken);

        // One row per DISTINCT benefiting family with the campaigns aggregated (story contract).
        // GroupBy + string.Join has no SQL translation, so the grouping runs in memory over a
        // narrow projection — the projection stays translatable, only the join is client-side.
        var projected = await query
            .OrderBy(b => b.Family!.Code)
            .Select(b => new
            {
                b.FamilyId,
                FamilyCode = b.Family!.Code,
                HeadName = b.Family.Providers.Where(p => !p.IsDeleted)
                               .OrderBy(p => p.CreatedOn).ThenBy(p => p.Id)
                               .Select(p => p.FullName).FirstOrDefault()
                           ?? b.Family.Father!.FullName
                           ?? b.Family.Mother!.FullName,
                Phone = b.Family.PhoneNumber,
                Address = b.Family.Address,
                CharityId = b.Family.FK_CharityId,
                CampaignName = b.Campaign!.Name
            })
            .ToListAsync(cancellationToken);

        var grouped = projected
            .GroupBy(b => b.FamilyId)
            .Select(g => new BeneficiaryFamilyListDto
            {
                FamilyCode = g.First().FamilyCode,
                HeadOfFamilyName = g.Select(x => x.HeadName).FirstOrDefault(x => x != null),
                Phone = g.First().Phone,
                Address = g.First().Address,
                CharityId = g.First().CharityId,
                CampaignNames = string.Join("، ", g
                    .Where(x => !string.IsNullOrEmpty(x.CampaignName))
                    .Select(x => x.CampaignName)
                    .Distinct())
            })
            .ToList();

        var totalCount = grouped.Count;
        var items = grouped
            .Skip(PageSkip(filter.Page, filter.PageSize))
            .Take(filter.PageSize)
            .ToList();

        await ResolveBeneficiaryCharityNamesAsync(items, cancellationToken);

        return new ReportPagedResult<BeneficiaryFamilyListDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = filter.Page,
            PageSize = filter.PageSize
        };
    }

    /// <inheritdoc />
    public async Task<ReportPagedResult<FamilyProjectReportRowDto>> GetRegisteredFamilyProjectsAsync(
        FamilyProjectsFilterDto filter,
        CancellationToken cancellationToken = default)
    {
        await _familyProjectsValidator.ValidateAndThrowAsync(filter, cancellationToken);

        // Row-source gap (grep-proven, standing 18-4 ruling): NO project entity carries a family
        // link — no HousingProject class exists in the Domain and OfficeProject (donors,
        // beneficiaries-count) has no FamilyId. §23.S.7's family+project row is unrepresentable
        // today, so the set is empty with the gap logged. When a family-project vertical adds the
        // entity/link, the whole projection (project columns + Family.Code/PhoneNumber/Provider +
        // joined orphan name/code/national-id strings, ordered StartDate DESC) lands HERE ALONE.
        _logger.LogInformation(
            "Family-projects report (UC-RPT-11): empty — no project entity carries a family link yet (recorded gap); caller {Caller}",
            _currentUser.UserId?.ToString() ?? "anonymous");

        return new ReportPagedResult<FamilyProjectReportRowDto>
        {
            Items = new List<FamilyProjectReportRowDto>(),
            TotalCount = 0,
            Page = filter.Page,
            // Never 0 — TotalPages divides by it (divide-by-zero on serialise caught in 18-8's smoke).
            PageSize = filter.PageSize
        };
    }

    /// <inheritdoc />
    public async Task<ReportPagedResult<ProviderChangeReportRowDto>> GetProviderSponsorChangesAsync(
        ProviderChangeFilterDto filter,
        CancellationToken cancellationToken = default)
    {
        await _providerChangeValidator.ValidateAndThrowAsync(filter, cancellationToken);

        // The platform keeps no guardian-change audit trail (recorded story finding): Provider
        // holds the CURRENT assignment only. This report projects what exists — one row per
        // provider assignment, تاريخ التعديل from the audit stamps — and the three
        // prior-guardian columns (اسم المعيل السابق / صله القرابه السابقة / سبب التغيير)
        // ship null. No storage is invented; a ProviderChangeHistory entity is a scope change.
        // No global soft-delete filter on this platform — filter explicitly at every level.
        var query = _providerRepository.TableNoTracking
            .Where(p => !p.IsDeleted
                && p.FamilyId != null
                && p.Family != null
                && !p.Family.IsDeleted);

        query = await ApplyProviderCharityScopeAsync(query, filter.CharityId, cancellationToken);

        var totalCount = await query.CountAsync(cancellationToken);

        // تاريخ التعديل — UpdatedOn marks an edited record when it is later than CreatedOn.
        var items = await query
            .OrderByDescending(p => p.CreatedOn)
            .ThenByDescending(p => p.UpdatedOn)
            .Select(p => new ProviderChangeReportRowDto
            {
                ProviderId = p.Id,
                FamilyId = p.FamilyId,
                FamilyCode = p.Family!.Code,
                NewGuardianName = p.FullName,
                NewRelationship = p.RelationshipToFamily,
                ChangeDate = p.UpdatedOn.HasValue && p.UpdatedOn > p.CreatedOn
                    ? p.UpdatedOn
                    : p.CreatedOn,
                CharityId = p.Family.FK_CharityId
            })
            .Skip(PageSkip(filter.Page, filter.PageSize))
            .Take(filter.PageSize)
            .ToListAsync(cancellationToken);

        await AttachProviderChangeOrphanCodesAsync(items, cancellationToken);
        await ResolveProviderChangeCharityNamesAsync(items, cancellationToken);

        return new ReportPagedResult<ProviderChangeReportRowDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = filter.Page,
            PageSize = filter.PageSize
        };
    }

    /// <summary>
    /// كود اليتيم — the family's orphan codes joined per row. GroupBy + string.Join has no SQL
    /// translation, so the join runs in memory over a narrow per-orphan projection fetched for
    /// the page's families only (the 18-9 pattern).
    /// </summary>
    private async Task AttachProviderChangeOrphanCodesAsync(
        List<ProviderChangeReportRowDto> items,
        CancellationToken cancellationToken)
    {
        var familyIds = items
            .Where(r => r.FamilyId.HasValue)
            .Select(r => r.FamilyId!.Value)
            .Distinct()
            .ToList();
        if (familyIds.Count == 0)
        {
            return;
        }

        var codes = await _orphanRepository.TableNoTracking
            .Where(o => !o.IsDeleted && o.FamilyId != null && familyIds.Contains(o.FamilyId!.Value))
            .Select(o => new { o.FamilyId, o.Code })
            .ToListAsync(cancellationToken);

        var joined = codes
            .GroupBy(o => o.FamilyId!.Value)
            .ToDictionary(g => g.Key, g => string.Join(", ", g.Select(o => o.Code).OrderBy(c => c)));

        foreach (var row in items)
        {
            if (row.FamilyId.HasValue && joined.TryGetValue(row.FamilyId.Value, out var codes2))
            {
                row.OrphanCodes = codes2;
            }
        }
    }

    /// <summary>Charity-name resolution for the §23.S.19 guardian-change rows.</summary>
    private async Task ResolveProviderChangeCharityNamesAsync(
        List<ProviderChangeReportRowDto> rows,
        CancellationToken cancellationToken)
    {
        var charityIds = rows
            .Where(r => r.CharityId.HasValue)
            .Select(r => r.CharityId!.Value)
            .Distinct()
            .ToList();
        if (charityIds.Count == 0)
        {
            return;
        }

        var names = await _charityRepository.TableNoTracking
            .Where(c => !c.IsDeleted && charityIds.Contains(c.Id))
            .ToDictionaryAsync(c => c.Id, c => c.Name, cancellationToken);

        foreach (var row in rows)
        {
            if (row.CharityId.HasValue && names.TryGetValue(row.CharityId.Value, out var name))
            {
                row.CharityName = name;
            }
        }
    }

    /// <inheritdoc />
    public async Task<ReportPagedResult<OrphansMissingReportsSummaryDto>> GetOrphansMissingReportsAsync(
        OrphansMissingReportsFilterDto filter,
        CancellationToken cancellationToken = default)
    {
        await _orphansMissingReportsValidator.ValidateAndThrowAsync(filter, cancellationToken);

        var query = await BuildOrphansMissingReportsQueryAsync(filter.CharityId, cancellationToken);

        // §23.S.14 summary — one row per charity with its chase count. The page control pages
        // the charity rows, so TotalCount is the number of groups (the 18-9 grouped-paging
        // precedent). No global soft-delete filter on this platform — every level inside the
        // builder filters !IsDeleted explicitly.
        var grouped = query.GroupBy(o => o.FK_CharityId);
        var totalCount = await grouped.CountAsync(cancellationToken);

        var items = await grouped
            .OrderByDescending(g => g.Count())
            .ThenBy(g => g.Key)
            .Select(g => new OrphansMissingReportsSummaryDto
            {
                CharityId = g.Key,
                MissingCount = g.Count()
            })
            .Skip(PageSkip(filter.Page, filter.PageSize))
            .Take(filter.PageSize)
            .ToListAsync(cancellationToken);

        await ResolveMissingReportsCharityNamesAsync(items, cancellationToken);

        return new ReportPagedResult<OrphansMissingReportsSummaryDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = filter.Page,
            PageSize = filter.PageSize
        };
    }

    /// <inheritdoc />
    public async Task<ReportPagedResult<OrphansMissingReportsDetailDto>> GetOrphansMissingReportsDetailAsync(
        OrphansMissingReportsDetailRequestDto request,
        CancellationToken cancellationToken = default)
    {
        // Review P17 2026-08-26: the endpoint joined the FluentValidation fold — the
        // validator owns the empty-charityId refusal (was the P11 inline throw) and the page
        // bounds (was a controller-side Math.Max clamp).
        await _orphansMissingReportsDetailValidator.ValidateAndThrowAsync(request, cancellationToken);

        var charityId = request.CharityId;

        // Pin-never-widen (AC 3/4): the shared scope helper inside the builder pins a charity
        // caller to its own charity whatever the drilled row said; the exact-charity filter
        // then intersects — a charity caller drilling another charity's row gets the
        // intersection (empty), never another charity's rows.
        var query = await BuildOrphansMissingReportsQueryAsync(charityId, cancellationToken);
        query = query.Where(o => o.FK_CharityId == charityId);

        var totalCount = await query.CountAsync(cancellationToken);

        // تاريخ آخر تقرير — latest report of ANY state for each orphan (an accepted one would
        // have cleared it out of the chase set) — the UC-ORR-14 projection shape.
        var anyReports = _periodicReportRepository.TableNoTracking
            .Where(r => !r.IsDeleted);

        var items = await query
            .OrderBy(o => o.Code)
            .Skip(PageSkip(request.Page, request.PageSize))
            .Take(request.PageSize)
            .Select(o => new OrphansMissingReportsDetailDto
            {
                OrphanId = o.Id,
                OrphanCode = o.Code,
                OrphanName = o.FullName,
                LastReportDate = anyReports
                    .Where(r => r.OrphanId == o.Id)
                    .OrderByDescending(r => r.ReportDate)
                    .Select(r => (DateTime?)r.ReportDate)
                    .FirstOrDefault()
            })
            .ToListAsync(cancellationToken);

        return new ReportPagedResult<OrphansMissingReportsDetailDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize
        };
    }

    /// <summary>
    /// The UC-RPT-15 chase set: coded (Code set), non-deleted orphans under the caller scope
    /// with NO accepted report covering the trailing 12 months. "Covers" reuses BR-11 (the
    /// UC-ORR-14 ruling): an accepted+reviewed report whose period intersects the window,
    /// falling back to ReportDate inside the window when the period is incomplete — so this
    /// chase list and the §14.U.14 one agree on who needs a report for the same window.
    /// </summary>
    private async Task<IQueryable<Orphan>> BuildOrphansMissingReportsQueryAsync(
        Guid? requestedCharityId,
        CancellationToken cancellationToken)
    {
        var windowStart = DateTime.UtcNow.AddMonths(-12).Date;
        var windowEndExclusive = DateTime.UtcNow.Date.AddDays(1);

        var query = _orphanRepository.TableNoTracking
            .Where(o => !o.IsDeleted && !string.IsNullOrEmpty(o.Code));

        query = await ApplyCharityScopeAsync(query, requestedCharityId, cancellationToken);

        var acceptedIds = BuildAcceptedReportIdsQuery(windowStart, windowEndExclusive);

        return query.Where(o => !acceptedIds.Contains(o.Id));
    }

    /// <summary>
    /// BR-11 "report covers the window" — one rule across the epic's chase reports (18-15,
    /// 18-19, and the §14.U.14 window list): only an ACCEPTED+reviewed report clears an
    /// orphan; coverage is the period intersecting [windowStart, windowEndExclusive), with
    /// the ReportDate standing in for the period when it is incomplete.
    /// </summary>
    private IQueryable<Guid> BuildAcceptedReportIdsQuery(DateTime windowStart, DateTime windowEndExclusive)
    {
        return _periodicReportRepository.TableNoTracking
            .Where(r => !r.IsDeleted && r.Reviewed && r.IsAccepted)
            .Where(r =>
                (r.ReportPeriodFrom != null && r.ReportPeriodTo != null
                    && r.ReportPeriodFrom < windowEndExclusive
                    && r.ReportPeriodTo >= windowStart)
                || ((r.ReportPeriodFrom == null || r.ReportPeriodTo == null)
                    && r.ReportDate >= windowStart
                    && r.ReportDate < windowEndExclusive))
            .Select(r => r.OrphanId);
    }

    /// <summary>Charity-name resolution for the §23.S.14 summary rows (live-column dictionary).</summary>
    private async Task ResolveMissingReportsCharityNamesAsync(
        List<OrphansMissingReportsSummaryDto> rows,
        CancellationToken cancellationToken)
    {
        var charityIds = rows
            .Where(r => r.CharityId.HasValue)
            .Select(r => r.CharityId!.Value)
            .Distinct()
            .ToList();
        if (charityIds.Count == 0)
        {
            return;
        }

        var names = await _charityRepository.TableNoTracking
            .Where(c => !c.IsDeleted && charityIds.Contains(c.Id))
            .ToDictionaryAsync(c => c.Id, c => c.Name, cancellationToken);

        foreach (var row in rows)
        {
            if (row.CharityId.HasValue && names.TryGetValue(row.CharityId.Value, out var name))
            {
                row.CharityName = name;
            }
        }
    }

    /// <inheritdoc />
    public async Task<ReportPagedResult<OrphansMissingFilesListDto>> GetOrphansMissingFilesAsync(
        OrphansMissingFilesFilterDto filter,
        CancellationToken cancellationToken = default)
    {
        await _orphansMissingFilesValidator.ValidateAndThrowAsync(filter, cancellationToken);

        // The missing-files rule (story decision, one rule): an orphan is IN the worklist when
        // its LATEST report (highest ReportDate) is flagged MissingDocuments == true — any
        // review state; the flag is the signal. Expressed as the anti-semi-join EF translates
        // cleanly: a flagged report with no later report for the same orphan. The hard-null
        // attachment signals are deliberately NOT unioned (recorded in the story Dev Notes).
        // No global soft-delete filter on this platform — explicit !IsDeleted at every level.
        var missingOrphanIds = _periodicReportRepository.TableNoTracking
            .Where(r => !r.IsDeleted && r.MissingDocuments == true
                && !_periodicReportRepository.TableNoTracking.Any(later =>
                    !later.IsDeleted
                    && later.OrphanId == r.OrphanId
                    && later.ReportDate > r.ReportDate))
            .Select(r => r.OrphanId);

        var query = _orphanRepository.TableNoTracking
            .Where(o => !o.IsDeleted && missingOrphanIds.Contains(o.Id));

        query = await ApplyCharityScopeAsync(query, filter.CharityId, cancellationToken);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderBy(o => o.Code)
            .Skip(PageSkip(filter.Page, filter.PageSize))
            .Take(filter.PageSize)
            .Select(o => new OrphansMissingFilesListDto
            {
                OrphanId = o.Id,
                FamilyId = o.FamilyId,
                OrphanCode = o.Code,
                OrphanName = o.FullName,
                // العنوان / القريه — null-safe: an orphan without a live family renders empty
                // columns, never a thrown NRE (decided).
                Address = o.Family != null ? o.Family.Address : null,
                Village = o.Family != null ? o.Family.CityVillage : null,
                CharityId = o.FK_CharityId
            })
            .ToListAsync(cancellationToken);

        await AttachMissingDocumentsNamesAsync(items, cancellationToken);
        await ResolveMissingFilesCharityNamesAsync(items, cancellationToken);

        return new ReportPagedResult<OrphansMissingFilesListDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = filter.Page,
            PageSize = filter.PageSize
        };
    }

    /// <inheritdoc />
    public async Task<ReportPagedResult<ReportsAwaitingApprovalListDto>> GetReportsAwaitingApprovalAsync(
        ReportsAwaitingApprovalFilterDto filter,
        CancellationToken cancellationToken = default)
    {
        await _reportsAwaitingApprovalValidator.ValidateAndThrowAsync(filter, cancellationToken);

        // Pending = submitted, neither accepted nor refused, never reviewed — the {id}/review
        // endpoint is what sets these bits (§23.U.17's queue). The three flags are non-nullable
        // bools defaulting false, so the direct comparisons need no null-lift.
        // No global soft-delete filter on this platform — explicit !IsDeleted.
        var query = _periodicReportRepository.TableNoTracking
            .Where(r => !r.IsDeleted && !r.Reviewed && !r.IsAccepted && !r.IsRefused);

        // Caller scope — the report-query ladder mirrors PeriodicOrphanReportService's
        // ApplyCallerScope: a CharityId claim pins the report's own CharityId (copied from
        // orphan.FK_CharityId at create time), a CountryId claim pins the charity's country.
        // The requested charity narrows by plain intersection AFTER the pin — pin-never-widen.
        if (_currentUser.CharityId.HasValue)
        {
            query = query.Where(r => r.CharityId == _currentUser.CharityId.Value);
        }
        else if (_currentUser.CountryId.HasValue)
        {
            query = query.Where(r => r.Charity != null && r.Charity.CountryId == _currentUser.CountryId.Value);
        }

        // Review P1 2026-08-26: fail closed — the filter intersect below is NOT a scope; a
        // claim-less non-HQ caller must not see the cross-charity queue.
        FailClosedWhenUnscoped();

        if (filter.CharityId.HasValue)
        {
            query = query.Where(r => r.CharityId == filter.CharityId.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        // Oldest submission first — it is a queue; Id breaks ties deterministically.
        var items = await query
            .OrderBy(r => r.ReportDate)
            .ThenBy(r => r.Id)
            .Skip(PageSkip(filter.Page, filter.PageSize))
            .Take(filter.PageSize)
            .Select(r => new ReportsAwaitingApprovalListDto
            {
                ReportId = r.Id,
                OrphanId = r.OrphanId,
                CharityName = r.Charity != null ? r.Charity.Name : null,
                OrphanName = r.Orphan.FullName,
                ReportDate = r.ReportDate,
                OrphanCode = r.Orphan.Code,
                RefuseReason = r.RefuseReason
            })
            .ToListAsync(cancellationToken);

        return new ReportPagedResult<ReportsAwaitingApprovalListDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = filter.Page,
            PageSize = filter.PageSize
        };
    }

    /// <inheritdoc />
    public async Task<ReportPagedResult<RefusedReportsListDto>> GetRefusedReportsAsync(
        RefusedReportsFilterDto filter,
        CancellationToken cancellationToken = default)
    {
        await _refusedReportsValidator.ValidateAndThrowAsync(filter, cancellationToken);

        // Refused = IsRefused == true — set exclusively by the {id}/review write path
        // (the periodic-reports module owns the decision; this report never writes).
        // No global soft-delete filter on this platform — explicit !IsDeleted.
        var query = _periodicReportRepository.TableNoTracking
            .Where(r => !r.IsDeleted && r.IsRefused);

        // Caller scope — the same report-rooted ladder as the pending queue (18-17),
        // mirroring PeriodicOrphanReportService.ApplyCallerScope. Pin-never-widen.
        if (_currentUser.CharityId.HasValue)
        {
            query = query.Where(r => r.CharityId == _currentUser.CharityId.Value);
        }
        else if (_currentUser.CountryId.HasValue)
        {
            query = query.Where(r => r.Charity != null && r.Charity.CountryId == _currentUser.CountryId.Value);
        }

        // Review P1 2026-08-26: fail closed — the filter intersect below is NOT a scope; a
        // claim-less non-HQ caller must not see the cross-charity queue.
        FailClosedWhenUnscoped();

        if (filter.CharityId.HasValue)
        {
            query = query.Where(r => r.CharityId == filter.CharityId.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        // Newest refusal first; Id breaks ties deterministically.
        var items = await query
            .OrderByDescending(r => r.ReviewedDate)
            .ThenByDescending(r => r.Id)
            .Skip(PageSkip(filter.Page, filter.PageSize))
            .Take(filter.PageSize)
            .Select(r => new RefusedReportsListDto
            {
                ReportId = r.Id,
                OrphanId = r.OrphanId,
                CharityId = r.CharityId,
                CharityName = r.Charity != null ? r.Charity.Name : null,
                OrphanName = r.Orphan.FullName,
                ReportDate = r.ReportDate,
                OrphanCode = r.Orphan.Code,
                RefuseReason = r.RefuseReason,
                RefuseReasonId = r.RefuseReasonId,
                ReviewedDate = r.ReviewedDate,
                ReviewerId = r.ReviewerId
            })
            .ToListAsync(cancellationToken);

        await AttachRefusedReasonLabelsAsync(items, cancellationToken);

        return new ReportPagedResult<RefusedReportsListDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = filter.Page,
            PageSize = filter.PageSize
        };
    }

    /// <summary>
    /// سبب الرفض resolution — the catalogue label for <see cref="RefusedReportsListDto.RefuseReasonId"/>
    /// overrides the reviewer's free text; free text stays when the report carries no catalogue
    /// id or the lookup row is gone. Resolved in memory for the page (the 18-12 attach pattern);
    /// the lookup's culture-dependent <c>Name</c> is NotMapped, so the raw columns are read.
    /// </summary>
    private async Task AttachRefusedReasonLabelsAsync(
        List<RefusedReportsListDto> items,
        CancellationToken cancellationToken)
    {
        var lookupIds = items
            .Where(i => i.RefuseReasonId.HasValue)
            .Select(i => i.RefuseReasonId!.Value)
            .Distinct()
            .ToList();

        if (lookupIds.Count == 0)
        {
            return;
        }

        var labels = await _refuseReasonRepository.TableNoTracking
            .Where(l => lookupIds.Contains(l.Id))
            .ToDictionaryAsync(l => l.Id, l => l.NameAr ?? l.NameEn ?? string.Empty, cancellationToken);

        foreach (var item in items)
        {
            if (item.RefuseReasonId.HasValue
                && labels.TryGetValue(item.RefuseReasonId.Value, out var label)
                && !string.IsNullOrWhiteSpace(label))
            {
                item.RefuseReason = label;
            }
        }
    }

    /// <summary>
    /// ما ينقص — the latest report's MissingDocumentsName for the page's orphans. The name
    /// rides the report row, so a flat per-orphan projection is fetched for the page and the
    /// latest picked in memory (the 18-12 attach pattern).
    /// </summary>
    private async Task AttachMissingDocumentsNamesAsync(
        List<OrphansMissingFilesListDto> items,
        CancellationToken cancellationToken)
    {
        var orphanIds = items.Select(i => i.OrphanId).ToList();
        if (orphanIds.Count == 0)
        {
            return;
        }

        var reports = await _periodicReportRepository.TableNoTracking
            .Where(r => !r.IsDeleted && orphanIds.Contains(r.OrphanId))
            .Select(r => new { r.OrphanId, r.ReportDate, r.MissingDocumentsName })
            .ToListAsync(cancellationToken);

        var latest = reports
            .GroupBy(r => r.OrphanId)
            .ToDictionary(g => g.Key, g => g.OrderByDescending(r => r.ReportDate).First());

        foreach (var item in items)
        {
            if (latest.TryGetValue(item.OrphanId, out var row))
            {
                item.MissingDocumentsName = row.MissingDocumentsName;
            }
        }
    }

    /// <summary>Charity-name resolution for the §23.S.15 worklist rows (live-column dictionary).</summary>
    private async Task ResolveMissingFilesCharityNamesAsync(
        List<OrphansMissingFilesListDto> rows,
        CancellationToken cancellationToken)
    {
        var charityIds = rows
            .Where(r => r.CharityId.HasValue)
            .Select(r => r.CharityId!.Value)
            .Distinct()
            .ToList();
        if (charityIds.Count == 0)
        {
            return;
        }

        var names = await _charityRepository.TableNoTracking
            .Where(c => !c.IsDeleted && charityIds.Contains(c.Id))
            .ToDictionaryAsync(c => c.Id, c => c.Name, cancellationToken);

        foreach (var row in rows)
        {
            if (row.CharityId.HasValue && names.TryGetValue(row.CharityId.Value, out var name))
            {
                row.CharityName = name;
            }
        }
    }

    /// <summary>
    /// Pin-never-widen over the PROVIDER set (family-scoped mirror of ApplyCharityScopeAsync):
    /// a charity caller is pinned to its own charity whatever the payload says; HQ may narrow
    /// to one explicit charity; a CountryId claim is additionally pinned to that country's
    /// charities; unconstrained HQ sees everything.
    /// </summary>
    private async Task<IQueryable<Provider>> ApplyProviderCharityScopeAsync(
        IQueryable<Provider> query,
        Guid? requestedCharityId,
        CancellationToken cancellationToken)
    {
        if (_currentUser.CharityId.HasValue)
        {
            var pinned = _currentUser.CharityId.Value;
            return query.Where(p => p.Family!.FK_CharityId == pinned);
        }

        if (_currentUser.IsHeadOffice && requestedCharityId.HasValue)
        {
            var narrowed = requestedCharityId.Value;
            return query.Where(p => p.Family!.FK_CharityId == narrowed);
        }

        FailClosedWhenUnscoped();

        if (_currentUser.CountryId.HasValue)
        {
            var countryId = _currentUser.CountryId.Value;
            var countryCharityIds = await _charityRepository.TableNoTracking
                .Where(c => !c.IsDeleted && c.CountryId == countryId)
                .Select(c => c.Id)
                .ToListAsync(cancellationToken);
            return query.Where(p => p.Family!.FK_CharityId != null
                && countryCharityIds.Contains(p.Family.FK_CharityId.Value));
        }

        // Review P1 2026-08-26: fail closed like ApplyCharityScopeAsync — a claim-less non-HQ
        // caller must not fall through unscoped.
        if (_currentUser.IsHeadOffice)
        {
            return query;
        }

        throw new UnauthorizedAccessException(
            "Caller has no charity, country, or head-office scope; refusing unscoped query.");
    }

    /// <summary>
    /// Pin-never-widen over the SEASONAL-AID BENEFICIARY set (family-scoped mirror of
    /// ApplyCharityScopeAsync): a charity caller is pinned to its own charity whatever the
    /// payload says; HQ may narrow to one explicit charity; a CountryId claim is additionally
    /// pinned to that country's charities; unconstrained HQ sees everything.
    /// </summary>
    private async Task<IQueryable<SeasonalAidBeneficiary>> ApplyBeneficiaryCharityScopeAsync(
        IQueryable<SeasonalAidBeneficiary> query,
        Guid? requestedCharityId,
        CancellationToken cancellationToken)
    {
        if (_currentUser.CharityId.HasValue)
        {
            var pinned = _currentUser.CharityId.Value;
            return query.Where(b => b.Family!.FK_CharityId == pinned);
        }

        if (_currentUser.IsHeadOffice && requestedCharityId.HasValue)
        {
            var narrowed = requestedCharityId.Value;
            return query.Where(b => b.Family!.FK_CharityId == narrowed);
        }

        FailClosedWhenUnscoped();

        if (_currentUser.CountryId.HasValue)
        {
            var countryId = _currentUser.CountryId.Value;
            var countryCharityIds = await _charityRepository.TableNoTracking
                .Where(c => !c.IsDeleted && c.CountryId == countryId)
                .Select(c => c.Id)
                .ToListAsync(cancellationToken);
            return query.Where(b => b.Family!.FK_CharityId != null
                && countryCharityIds.Contains(b.Family!.FK_CharityId.Value));
        }

        // Review P1 2026-08-26: fail closed like ApplyCharityScopeAsync — a claim-less non-HQ
        // caller must not fall through unscoped.
        if (_currentUser.IsHeadOffice)
        {
            return query;
        }

        throw new UnauthorizedAccessException(
            "Caller has no charity, country, or head-office scope; refusing unscoped query.");
    }

    /// <summary>Charity-name resolution for the §23.S.13 rows (same shape as the others).</summary>
    private async Task ResolveBeneficiaryCharityNamesAsync(
        List<BeneficiaryFamilyListDto> rows,
        CancellationToken cancellationToken)
    {
        var charityIds = rows
            .Where(r => r.CharityId.HasValue)
            .Select(r => r.CharityId!.Value)
            .Distinct()
            .ToList();
        if (charityIds.Count == 0)
        {
            return;
        }

        var names = await _charityRepository.TableNoTracking
            .Where(c => !c.IsDeleted && charityIds.Contains(c.Id))
            .ToDictionaryAsync(c => c.Id, c => c.Name, cancellationToken);

        foreach (var row in rows)
        {
            if (row.CharityId.HasValue && names.TryGetValue(row.CharityId.Value, out var name))
            {
                row.CharityName = name;
            }
        }
    }

    /// <summary>
    /// Pin-never-widen over the MOTHER set (family-scoped mirror of ApplyCharityScopeAsync):
    /// the mother's tenancy rides its family's <c>FK_CharityId</c>.
    /// </summary>
    private async Task<IQueryable<Mother>> ApplyMotherCharityScopeAsync(
        IQueryable<Mother> query,
        Guid? requestedCharityId,
        CancellationToken cancellationToken)
    {
        if (_currentUser.CharityId.HasValue)
        {
            var pinned = _currentUser.CharityId.Value;
            return query.Where(m => m.Family!.FK_CharityId == pinned);
        }

        if (_currentUser.IsHeadOffice && requestedCharityId.HasValue)
        {
            var narrowed = requestedCharityId.Value;
            return query.Where(m => m.Family!.FK_CharityId == narrowed);
        }

        FailClosedWhenUnscoped();

        if (_currentUser.CountryId.HasValue)
        {
            var countryId = _currentUser.CountryId.Value;
            var countryCharityIds = await _charityRepository.TableNoTracking
                .Where(c => !c.IsDeleted && c.CountryId == countryId)
                .Select(c => c.Id)
                .ToListAsync(cancellationToken);
            return query.Where(m => m.Family!.FK_CharityId != null
                && countryCharityIds.Contains(m.Family.FK_CharityId.Value));
        }

        // Review P1 2026-08-26: fail closed like ApplyCharityScopeAsync — a claim-less non-HQ
        // caller must not fall through unscoped.
        if (_currentUser.IsHeadOffice)
        {
            return query;
        }

        throw new UnauthorizedAccessException(
            "Caller has no charity, country, or head-office scope; refusing unscoped query.");
    }

    /// <summary>Charity-name resolution for the §23.S.9 widow rows.</summary>
    private async Task ResolveWidowCharityNamesAsync(
        List<WidowSponsorshipListDto> rows,
        CancellationToken cancellationToken)
    {
        var charityIds = rows
            .Where(r => r.CharityId.HasValue)
            .Select(r => r.CharityId!.Value)
            .Distinct()
            .ToList();
        if (charityIds.Count == 0)
        {
            return;
        }

        var names = await _charityRepository.TableNoTracking
            .Where(c => !c.IsDeleted && charityIds.Contains(c.Id))
            .ToDictionaryAsync(c => c.Id, c => c.Name, cancellationToken);

        foreach (var row in rows)
        {
            if (row.CharityId.HasValue && names.TryGetValue(row.CharityId.Value, out var name))
            {
                row.CharityName = name;
            }
        }
    }

    /// <summary>Charity-name resolution for the §23.S.5/§23.S.6 status-report rows.</summary>
    private async Task ResolveStatusReportCharityNamesAsync(
        List<OrphanStatusReportListDto> items,
        CancellationToken cancellationToken)
    {
        var charityIds = items
            .Where(i => i.CharityId.HasValue)
            .Select(i => i.CharityId!.Value)
            .Distinct()
            .ToList();
        if (charityIds.Count == 0)
        {
            return;
        }

        var names = await _charityRepository.TableNoTracking
            .Where(c => !c.IsDeleted && charityIds.Contains(c.Id))
            .ToDictionaryAsync(c => c.Id, c => c.Name, cancellationToken);

        foreach (var item in items)
        {
            if (item.CharityId.HasValue && names.TryGetValue(item.CharityId.Value, out var name))
            {
                item.CharityName = name;
            }
        }
    }

    /// <summary>
    /// Pin-never-widen (OfficeProjectService.ApplyCallerScope shape): a charity caller is pinned
    /// to <c>ICurrentUserService.CharityId</c>; an HQ caller may narrow to one explicit charity;
    /// a CountryId claim is additionally pinned to that country's charities; unconstrained HQ
    /// sees everything.
    /// </summary>
    private async Task<IQueryable<Orphan>> ApplyCharityScopeAsync(
        IQueryable<Orphan> query,
        Guid? requestedCharityId,
        CancellationToken cancellationToken)
    {
        // Charity caller — pinned; the payload's CharityId is never trusted (AC 3).
        if (_currentUser.CharityId.HasValue)
        {
            var pinned = _currentUser.CharityId.Value;
            return query.Where(o => o.FK_CharityId == pinned);
        }

        // HQ caller may narrow to one explicit charity.
        if (_currentUser.IsHeadOffice && requestedCharityId.HasValue)
        {
            var narrowed = requestedCharityId.Value;
            return query.Where(o => o.FK_CharityId == narrowed);
        }

        // Country claim — additionally pinned to the (non-deleted) charities of that country.
        FailClosedWhenUnscoped();

        if (_currentUser.CountryId.HasValue)
        {
            var countryId = _currentUser.CountryId.Value;
            var countryCharityIds = await _charityRepository.TableNoTracking
                .Where(c => !c.IsDeleted && c.CountryId == countryId)
                .Select(c => c.Id)
                .ToListAsync(cancellationToken);
            return query.Where(o => o.FK_CharityId != null && countryCharityIds.Contains(o.FK_CharityId.Value));
        }

        // Review D2/P3 2026-08-24: unconstrained HQ sees everything; anyone else reaching here
        // has no charity and no country claim and previously fell through UNSCOPED (fail-open).
        // Fail closed instead — a caller must carry at least one scoping claim or be HQ.
        if (_currentUser.IsHeadOffice)
        {
            return query;
        }

        throw new UnauthorizedAccessException(
            "Caller has no charity, country, or head-office scope; refusing unscoped query.");
    }

    /// <summary>
    /// Review P7 2026-08-26: date windows are half-open [from, toExclusive) at day granularity —
    /// AddDays(1) on a user-supplied 9999-12-31 previously threw ArgumentOutOfRangeException
    /// (a 500 from every windowed report); the MaxValue boundary saturates instead.
    /// </summary>
    private static DateTime ExclusiveUpperBound(DateTime inclusiveToDate)
        => inclusiveToDate >= DateTime.MaxValue.Date ? DateTime.MaxValue : inclusiveToDate.AddDays(1);

    /// <summary>
    /// Review P7 2026-08-26: (Page - 1) * PageSize in unchecked int wraps NEGATIVE on large page
    /// numbers (EF Skip and LINQ Skip both throw on negative) — computed in long and clamped so
    /// an absurd page request returns an empty page, not a 500.
    /// </summary>
    private static int PageSkip(int page, int pageSize)
        => (int)Math.Min((long)(page - 1) * pageSize, int.MaxValue);

    /// <summary>
    /// Review P14 2026-08-26: row ceiling for the identification sheet — mirrors
    /// ReportSheetService.SheetRowCap (an unbounded sheet over an HQ-wide register is a PII
    /// dump that can freeze the print tab).
    /// </summary>
    private const int GuardianSheetRowCap = 5000;

    /// <summary>
    /// Review P5 2026-08-26: the ONE age rule for reports — completed years, birthday-aware
    /// (AddYears comparison; the DateDiffYear / DayOfYear / 365.2425 variants each drift by a
    /// day or a year across leap years and pre-birthday windows). A future DOB renders blank,
    /// never a negative age.
    /// </summary>
    private static int? CompletedAge(DateTime? dateOfBirth, DateTime asAt)
    {
        if (dateOfBirth is not { } dob || dob > asAt)
        {
            return null;
        }

        var age = asAt.Year - dob.Year;
        return dob.AddYears(age) > asAt ? age - 1 : age;
    }

    /// <summary>
    /// Review P1 2026-08-26: the fail-closed tail of every INLINED scope ladder. The hand-inlined
    /// ladder copies (orphan-, family-, provider-, mother-, beneficiary- and payment-item-rooted)
    /// previously fell through UNSCOPED for a caller with no claims (fail-open) while this class's
    /// shared helpers already failed closed — this guard closes the copies. HQ with no claims is
    /// the one legitimate unscoped case (unconstrained HQ sees everything). Throws
    /// <see cref="UnauthorizedAccessException"/> so the controllers map it to 403, not 500.
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

    /// <summary>Resolves CharityName for the page's distinct charity ids (single code path).</summary>
    private async Task ResolveCharityNamesAsync(List<OrphanDataListDto> items, CancellationToken cancellationToken)
    {
        var charityIds = items
            .Where(i => i.CharityId.HasValue)
            .Select(i => i.CharityId!.Value)
            .Distinct()
            .ToList();
        if (charityIds.Count == 0)
        {
            return;
        }

        var names = await _charityRepository.TableNoTracking
            .Where(c => !c.IsDeleted && charityIds.Contains(c.Id))
            .ToDictionaryAsync(c => c.Id, c => c.Name, cancellationToken);

        foreach (var item in items)
        {
            if (item.CharityId.HasValue && names.TryGetValue(item.CharityId.Value, out var name))
            {
                item.CharityName = name;
            }
        }
    }

    /// <summary>Charity-name resolution for the §14.U.14 chase-list rows (same shape as above).</summary>
    private async Task ResolveNonRenewedCharityNamesAsync(
        List<NonRenewedOrphanRowDto> rows,
        CancellationToken cancellationToken)
    {
        var charityIds = rows
            .Where(r => r.CharityId.HasValue)
            .Select(r => r.CharityId!.Value)
            .Distinct()
            .ToList();
        if (charityIds.Count == 0)
        {
            return;
        }

        var names = await _charityRepository.TableNoTracking
            .Where(c => !c.IsDeleted && charityIds.Contains(c.Id))
            .ToDictionaryAsync(c => c.Id, c => c.Name, cancellationToken);

        foreach (var row in rows)
        {
            if (row.CharityId.HasValue && names.TryGetValue(row.CharityId.Value, out var name))
            {
                row.CharityName = name;
            }
        }
    }
}
