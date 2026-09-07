using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using IIROSA.Application.Interfaces;
using IIROSA.Application.DTOs.OrphanPayment;
using IIROSA.Domain.Entities;
using IIROSA.Domain.Interfaces;
using AutoMapper;
using FluentValidation;

namespace IIROSA.Application.Services;

/// <summary>
/// OrphanPayment Service Implementation
/// Implements all use cases UC-5.1 through UC-5.13
/// </summary>
public class OrphanPaymentService : IOrphanPaymentService
{
    private readonly IIROSA.Application.Interfaces.ICharityWriteGuard _charityWriteGuard;
    private readonly IOrphanPaymentRepository _orphanPaymentRepository;
    private readonly IOrphanPaymentItemRepository _orphanPaymentItemRepository;
    private readonly IOrphanRepository _orphanRepository;
    private readonly ICharityRepository _charityRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<OrphanPaymentService> _logger;
    private readonly IValidator<CreateOrphanPaymentDto> _createValidator;
    private readonly IValidator<UpdateOrphanPaymentDto> _updateValidator;
    private readonly IValidator<UpdateOrphanPaymentItemDto> _updateItemValidator;
    private readonly Framework.Identity.Data.Services.Interfaces.IUserAppService _userAppService;
    // Fully qualified: importing Contracts.Persistence wholesale would make the AddAsync
    // extension ambiguous against Domain.Interfaces' set.
    private readonly Domain.Contracts.Persistence.IRepository<PeriodicOrphanReport> _periodicReportRepository;

    public OrphanPaymentService(
        IIROSA.Application.Interfaces.ICharityWriteGuard charityWriteGuard,
        IOrphanPaymentRepository orphanPaymentRepository,
        IOrphanPaymentItemRepository orphanPaymentItemRepository,
        IOrphanRepository orphanRepository,
        ICharityRepository charityRepository,
        Domain.Contracts.Persistence.IRepository<PeriodicOrphanReport> periodicReportRepository,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<OrphanPaymentService> logger,
        IValidator<CreateOrphanPaymentDto> createValidator,
        IValidator<UpdateOrphanPaymentDto> updateValidator,
        IValidator<UpdateOrphanPaymentItemDto> updateItemValidator,
        Framework.Identity.Data.Services.Interfaces.IUserAppService userAppService)
    {
        _charityWriteGuard = charityWriteGuard;
        _orphanPaymentRepository = orphanPaymentRepository;
        _orphanPaymentItemRepository = orphanPaymentItemRepository;
        _orphanRepository = orphanRepository;
        _charityRepository = charityRepository;
        _periodicReportRepository = periodicReportRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
        _updateItemValidator = updateItemValidator;
        _userAppService = userAppService;
    }

    #region UC-5.1: Create Orphan Payment Group

    public async Task<OrphanPaymentDto> CreatePaymentGroupAsync(CreateOrphanPaymentDto dto)
    {
        // UC-CHR-07/08/09: head office can lock a charity or withdraw its add/edit rights.
        await _charityWriteGuard.EnsureCanAddAsync();

        // §15.S.2 mandatory fields + period ordering (10-2; was DataAnnotations only)
        await _createValidator.ValidateAndThrowAsync(dto);

        _logger.LogInformation("Creating new orphan payment group: {GroupName}", dto.GroupName);

        // Generate batch number if not provided
        string? batchNo = dto.BatchNo;
        if (string.IsNullOrWhiteSpace(batchNo))
        {
            batchNo = await GenerateNextBatchNumberAsync();
        }
        else if (!await IsBatchNoUniqueAsync(batchNo))
        {
            throw new InvalidOperationException($"Batch number '{batchNo}' already exists");
        }

        // Create payment group
        var orphanPayment = new OrphanPayment
        {
            Id = Guid.NewGuid(),
            GroupName = dto.GroupName,
            Description = dto.Description,
            PaymentPeriodFrom = dto.PaymentPeriodFrom,
            PaymentPeriodTo = dto.PaymentPeriodTo,
            GroupDate = dto.GroupDate ?? DateTime.UtcNow,
            PaymentDate = dto.PaymentDate,
            ExchangeRate = dto.ExchangeRate,
            Currency = dto.Currency,
            DontRemoveRate = dto.DontRemoveRate,
            BatchNo = batchNo,
            ShowOrder = dto.ShowOrder,
            Notes = dto.Notes,
            IsBatchUploaded = false
        };

        await _orphanPaymentRepository.AddAsync(orphanPayment);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Orphan payment group created successfully with ID: {Id}", orphanPayment.Id);

        return await GetByIdAsync(orphanPayment.Id) ?? throw new InvalidOperationException("Failed to retrieve created payment group");
    }

    #endregion

    #region UC-5.2 & UC-5.6: Set Exchange Rate (including locking)

    public async Task SetExchangeRateAsync(SetExchangeRateDto dto)
    {
        _logger.LogInformation("Setting exchange rate for payment group: {OrphanPaymentId}", dto.OrphanPaymentId);

        // Review P4: write-path fetch honours soft delete — a deleted batch reads as missing,
        // instead of being resurrected and mutated (FindAsync ignores IsDeleted).
        var paymentGroup = await GetLivePaymentGroupAsync(dto.OrphanPaymentId);
        if (paymentGroup == null)
        {
            throw new KeyNotFoundException($"Payment group with ID '{dto.OrphanPaymentId}' not found");
        }

        // Check if exchange rate is locked (UC-5.6)
        if (paymentGroup.DontRemoveRate && !dto.DontRemoveRate)
        {
            throw new InvalidOperationException("Exchange rate is locked and cannot be modified");
        }

        paymentGroup.ExchangeRate = dto.ExchangeRate;
        paymentGroup.Currency = dto.Currency;
        paymentGroup.DontRemoveRate = dto.DontRemoveRate;

        _orphanPaymentRepository.Update(paymentGroup);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Exchange rate set successfully for payment group: {OrphanPaymentId}", dto.OrphanPaymentId);
    }

    public async Task LockExchangeRateAsync(Guid orphanPaymentId, bool lockRate)
    {
        _logger.LogInformation("Setting exchange rate lock for payment group {OrphanPaymentId} to {LockRate}", orphanPaymentId, lockRate);

        // Review P4: write-path fetch honours soft delete (FindAsync ignores IsDeleted).
        var paymentGroup = await GetLivePaymentGroupAsync(orphanPaymentId);
        if (paymentGroup == null)
        {
            throw new KeyNotFoundException($"Payment group with ID '{orphanPaymentId}' not found");
        }

        paymentGroup.DontRemoveRate = lockRate;

        _orphanPaymentRepository.Update(paymentGroup);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Exchange rate lock set successfully for payment group: {OrphanPaymentId}", orphanPaymentId);
    }

    #endregion

    #region UC-5.3: Add Orphans to Payment Group

    public async Task<(int AddedCount, int SkippedCount)> AddOrphansToGroupAsync(AddOrphansToGroupDto dto)
    {
        _logger.LogInformation("Adding {Count} orphans to payment group: {OrphanPaymentId}", dto.OrphanIds.Count, dto.OrphanPaymentId);

        if (dto.OrphanIds == null || dto.OrphanIds.Count == 0)
        {
            throw new InvalidOperationException("At least one orphan must be selected (UC-5.3 Alternative Flow 11a)");
        }

        // Review P4: write-path fetch honours soft delete — a deleted batch reads as missing,
        // instead of being resurrected and mutated (FindAsync ignores IsDeleted).
        var paymentGroup = await GetLivePaymentGroupAsync(dto.OrphanPaymentId);
        if (paymentGroup == null)
        {
            throw new KeyNotFoundException($"Payment group with ID '{dto.OrphanPaymentId}' not found");
        }

        // Review P2: removal is a soft delete, but IX_OrphanPaymentItem(OrphanPaymentId,
        // OrphanId) is unique with no IsDeleted filter — re-enrolling a previously removed
        // orphan must RESURRECT its soft-deleted row instead of inserting a second one (was:
        // duplicate check saw live rows only → INSERT → SqlException 2601 → raw 500).
        var allGroupItems = await _orphanPaymentItemRepository.AsQueryable()
            .Where(opi => opi.OrphanPaymentId == dto.OrphanPaymentId)
            .ToListAsync();
        var existingOrphanIds = allGroupItems
            .Where(opi => !opi.IsDeleted)
            .Select(opi => opi.OrphanId)
            .ToHashSet();
        var removedItemByOrphanId = allGroupItems
            .Where(opi => opi.IsDeleted)
            .GroupBy(opi => opi.OrphanId)
            .ToDictionary(g => g.Key, g => g.First());

        // 10-2 defect 7: batch-load the selected orphans (was GetByIdAsync per orphan)
        var orphanIdsToAdd = dto.OrphanIds
            .Where(id => !existingOrphanIds.Contains(id))
            .Distinct()
            .ToList();
        var orphansById = orphanIdsToAdd.Count == 0
            ? new Dictionary<Guid, Orphan>()
            : (await _orphanRepository.GetPagedAsync(o => orphanIdsToAdd.Contains(o.Id), 1, orphanIdsToAdd.Count))
                .Items.ToDictionary(o => o.Id);

        int addedCount = 0;
        int skippedCount = 0;
        int displayOrder = existingOrphanIds.Count;

        foreach (var orphanId in dto.OrphanIds)
        {
            // Skip if orphan already in group (UC-5.3 Alternative Flow 12a)
            if (existingOrphanIds.Contains(orphanId))
            {
                skippedCount++;
                continue;
            }

            // Verify orphan exists
            if (!orphansById.TryGetValue(orphanId, out var orphan))
            {
                _logger.LogWarning("Orphan with ID {OrphanId} not found, skipping", orphanId);
                skippedCount++;
                continue;
            }

            // Review P2: a soft-deleted row for this (batch, orphan) pair resurrects with a
            // fresh snapshot (BR-17) rather than colliding with the unfiltered unique index.
            if (removedItemByOrphanId.TryGetValue(orphanId, out var removedItem))
            {
                removedItem.IsDeleted = false;
                removedItem.DeletedOn = null;
                removedItem.DeletedBy = null;
                removedItem.DisplayOrder = displayOrder++;
                removedItem.Amount = orphan.MonthlyAmount;
                _orphanPaymentItemRepository.Update(removedItem);
                addedCount++;
                existingOrphanIds.Add(orphanId);
                continue;
            }

            // Create orphan payment item — Amount snapshotted from the orphan's current
            // monthly amount at enrolment (BR-17: later orphan changes never mutate past batches)
            var orphanPaymentItem = new OrphanPaymentItem
            {
                Id = Guid.NewGuid(),
                OrphanPaymentId = dto.OrphanPaymentId,
                OrphanId = orphanId,
                DisplayOrder = displayOrder++,
                Amount = orphan.MonthlyAmount
            };

            await _orphanPaymentItemRepository.AddAsync(orphanPaymentItem);
            addedCount++;
            existingOrphanIds.Add(orphanId);
        }

        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Added {AddedCount} orphans to payment group, skipped {SkippedCount}", addedCount, skippedCount);

        return (addedCount, skippedCount);
    }

    #endregion

    #region UC-5.4: Remove Orphan from Group

    public async Task RemoveOrphanFromGroupAsync(Guid orphanPaymentItemId)
    {
        _logger.LogInformation("Removing orphan payment item: {OrphanPaymentItemId}", orphanPaymentItemId);

        var orphanPaymentItem = await _orphanPaymentItemRepository.GetByIdAsync(orphanPaymentItemId);
        if (orphanPaymentItem == null || orphanPaymentItem.IsDeleted)
        {
            throw new KeyNotFoundException($"Orphan payment item with ID '{orphanPaymentItemId}' not found");
        }

        // Review P4: a row in a soft-deleted batch reads as missing, like every other surface.
        // Review P12: the §25.6 A1 disbursement guard the group delete enforces applies per
        // row too — money that moved (received, transferred or cheque-issued) may not be
        // quietly dropped from a batch one row at a time.
        if (await GetLivePaymentGroupAsync(orphanPaymentItem.OrphanPaymentId) == null)
        {
            throw new KeyNotFoundException($"Orphan payment item with ID '{orphanPaymentItemId}' not found");
        }
        if (orphanPaymentItem.IsGotIt || !string.IsNullOrEmpty(orphanPaymentItem.TransferNo)
            || !string.IsNullOrEmpty(orphanPaymentItem.ChiqueNum))
        {
            throw new InvalidOperationException(
                "Cannot remove a disbursed row (received, transferred or cheque-issued)");
        }

        // Soft delete (platform convention) — was a hard DbSet.Remove (10-5 defect 3)
        orphanPaymentItem.IsDeleted = true;
        _orphanPaymentItemRepository.Update(orphanPaymentItem);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Orphan removed from payment group successfully: {OrphanPaymentItemId}", orphanPaymentItemId);
    }

    #endregion

    #region UC-PAY-09..13: Row Actions (§15.1 action model — ONE endpoint)

    /// <summary>
    /// §15.1 row-action switch. 10-9 ships action 0 (stop/resume); actions 1..4 are the
    /// per-story behaviours of 10-10..10-13 on this same method — the DTO is frozen.
    /// Tenancy: same item→orphan→charity derivation as every other row surface.
    /// </summary>
    public async Task<OrphanPaymentItemDto> UpdateOrphanPaymentItemAsync(
        UpdateOrphanPaymentItemDto dto,
        Guid? userCharityId = null,
        string? userRole = null,
        Guid? userId = null)
    {
        await _updateItemValidator.ValidateAndThrowAsync(dto);

        _logger.LogInformation("Applying row action {Action} to orphan payment item: {Id}",
            dto.Action, dto.OrphanPaymentItemId);

        var item = await _orphanPaymentItemRepository.GetWithOrphanAsync(dto.OrphanPaymentItemId);
        if (item == null || item.IsDeleted)
        {
            throw new KeyNotFoundException($"Orphan payment item with ID '{dto.OrphanPaymentItemId}' not found");
        }

        // Review P13: rows of a soft-deleted batch read as missing everywhere else — the write
        // path must not resurrect them; and an uploaded batch is frozen for row actions (the
        // FE canModify() gate mirrored server-side — hiding a button is not a control).
        var parentBatch = await GetLivePaymentGroupAsync(item.OrphanPaymentId);
        if (parentBatch == null)
        {
            throw new KeyNotFoundException($"Orphan payment item with ID '{dto.OrphanPaymentItemId}' not found");
        }
        if (parentBatch.IsBatchUploaded)
        {
            throw new InvalidOperationException("This batch has been uploaded; its rows can no longer be modified");
        }

        // Charity scope (AC 2): same guard family as the reads — out-of-scope rows refuse, and
        // a Charity token without a parseable charity claim fails CLOSED (review P1/D4): the
        // scope check may never be skipped just because the claim is absent.
        var orphanCharityId = item.Orphan?.FK_CharityId ?? item.Orphan?.Family?.FK_CharityId;
        if (userRole == "Charity")
        {
            if (!userCharityId.HasValue)
            {
                throw new UnauthorizedAccessException("Charity caller has no charity scope");
            }
            if (orphanCharityId != userCharityId.Value)
            {
                throw new UnauthorizedAccessException("Orphan payment item is outside the caller's charity scope");
            }
        }

        switch (dto.Action)
        {
            case 0: // stop / resume (Flag carries the direction)
                if (dto.Flag == true)
                {
                    item.IsStopped = true;
                    item.StoppedOn = DateTime.UtcNow;
                    item.StoppedByUserId = userId;
                }
                else
                {
                    // BR-16/BR-21 realisation — the HQ-stop lock: a stop stamped by an HQ-role
                    // user cannot be resumed by a Charity caller (HQ always may). Review P5:
                    // a stop with no recorded stopper (legacy rows / unresolvable identity at
                    // stop time) fails CLOSED — treated as an HQ stop, not an unlocked one.
                    var stoppedByHeadOffice = !item.StoppedByUserId.HasValue
                        || await IsHeadOfficeUserAsync(item.StoppedByUserId.Value);
                    if (userRole == "Charity" && stoppedByHeadOffice)
                    {
                        throw new InvalidOperationException(
                            "This payment was stopped by head office; a charity user cannot resume it");
                    }
                    item.IsStopped = false;
                    item.StoppedOn = null;
                    item.StoppedByUserId = null;
                }
                break;

            case 1: // mark printed (§15.U.10) — idempotent: re-marking neither errors nor re-stamps
                if (!item.IsPrinted)
                {
                    item.IsPrinted = true;
                    item.PrintedOn = DateTime.UtcNow;
                }
                break;

            default:
                // Actions 2..4 belong to 10-11..10-13 — refuse loudly, never silently no-op.
                throw new InvalidOperationException($"Row action {dto.Action} is not available yet");
        }

        _orphanPaymentItemRepository.Update(item);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Row action {Action} applied to orphan payment item {Id}",
            dto.Action, dto.OrphanPaymentItemId);

        return MapOrphanPaymentItemDto(item);
    }

    /// <summary>The HQ role set for the stop-lock check — the vertical's standing HQ-Fin definition.</summary>
    private static readonly string[] HeadOfficeRoles = { "SuperAdmin", "Admin", "Accountant", "FinancialOfficer" };

    /// <summary>
    /// Membership goes through IUserAppService.GetUsersInRoles (the platform's own join), NOT
    /// UserManager.IsInRoleAsync — ApplicationUserRoles carries a surrogate Id PK on the legacy
    /// schema, so Identity's FindUserRoleAsync(userId, roleId) throws ("2 values passed to Find").
    /// Review P5: a stopper holding an HQ role locks; a stopper holding ONLY the Charity role
    /// does not; an unknown user (deleted, de-role'd — both sets miss) fails CLOSED — read as
    /// HQ, so a charity caller can never resume a stop whose initiator can't be vouched for.
    /// </summary>
    private async Task<bool> IsHeadOfficeUserAsync(Guid userId)
    {
        var hqUsers = await _userAppService.GetUsersInRoles(HeadOfficeRoles.ToList());
        if (hqUsers.Any(u => u.Id == userId)) return true;

        var charityUsers = await _userAppService.GetUsersInRoles(new List<string> { "Charity" });
        return !charityUsers.Any(u => u.Id == userId);
    }

    #endregion

    #region UC-5.5: Update Payment Group

    public async Task<OrphanPaymentDto> UpdatePaymentGroupAsync(UpdateOrphanPaymentDto dto)
    {
        // UC-CHR-07/08/09: head office can lock a charity or withdraw its add/edit rights.
        await _charityWriteGuard.EnsureCanUpdateAsync();

        _logger.LogInformation("Updating payment group: {Id}", dto.Id);

        // Review P4: write-path fetch honours soft delete — a deleted batch reads as missing
        // here too, instead of being resurrected and mutated (FindAsync ignores IsDeleted).
        var paymentGroup = await GetLivePaymentGroupAsync(dto.Id);
        if (paymentGroup == null)
        {
            throw new KeyNotFoundException($"Payment group with ID '{dto.Id}' not found");
        }

        // §15.S.2 field set incl. PaymentDate + period order (10-4) — replaces the hand
        // date-range check; throws ValidationException → controller 400 field map
        await _updateValidator.ValidateAndThrowAsync(dto);

        // Review P7: the edit dialog's picker offers every group's number — a manual BatchNo
        // must clear the same uniqueness gate the create and assign paths enforce (was a raw
        // 2601 → 500). Review P15: numbers are stored trimmed so the picker's Trim() grouping
        // and the uniqueness/resolve lookups always agree on the same value.
        string? normalizedBatchNo = null;
        if (!string.IsNullOrWhiteSpace(dto.BatchNo))
        {
            normalizedBatchNo = dto.BatchNo.Trim();
            if (!await IsBatchNoUniqueAsync(normalizedBatchNo, dto.Id))
            {
                throw new InvalidOperationException($"Batch number '{normalizedBatchNo}' already in use");
            }
        }

        // Check if exchange rate is locked and being modified
        if (paymentGroup.DontRemoveRate && (dto.ExchangeRate != paymentGroup.ExchangeRate || dto.Currency != paymentGroup.Currency))
        {
            throw new InvalidOperationException("Exchange rate is locked and cannot be modified");
        }

        // Update fields
        paymentGroup.GroupName = dto.GroupName;
        paymentGroup.Description = dto.Description;
        paymentGroup.PaymentPeriodFrom = dto.PaymentPeriodFrom;
        paymentGroup.PaymentPeriodTo = dto.PaymentPeriodTo;
        paymentGroup.GroupDate = dto.GroupDate ?? paymentGroup.GroupDate;
        paymentGroup.PaymentDate = dto.PaymentDate ?? paymentGroup.PaymentDate;
        paymentGroup.ExchangeRate = dto.ExchangeRate;
        paymentGroup.Currency = dto.Currency;
        paymentGroup.DontRemoveRate = dto.DontRemoveRate;
        paymentGroup.BatchNo = normalizedBatchNo;
        paymentGroup.ShowOrder = dto.ShowOrder;
        paymentGroup.Notes = dto.Notes;
        paymentGroup.IsBatchUploaded = dto.IsBatchUploaded;
        paymentGroup.UploadDate = dto.IsBatchUploaded ? (dto.UploadDate ?? DateTime.UtcNow) : null;

        _orphanPaymentRepository.Update(paymentGroup);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Payment group updated successfully: {Id}", dto.Id);

        return await GetByIdAsync(dto.Id) ?? throw new InvalidOperationException("Failed to retrieve updated payment group");
    }

    #endregion

    #region UC-5.7: Mark Group as Uploaded

    public async Task MarkAsUploadedAsync(MarkAsUploadedDto dto)
    {
        _logger.LogInformation("Marking payment group {OrphanPaymentId} as uploaded: {IsUploaded}", dto.OrphanPaymentId, dto.IsUploaded);

        // Review P4: write-path fetch honours soft delete — a deleted batch reads as missing,
        // instead of being resurrected and mutated (FindAsync ignores IsDeleted).
        var paymentGroup = await GetLivePaymentGroupAsync(dto.OrphanPaymentId);
        if (paymentGroup == null)
        {
            throw new KeyNotFoundException($"Payment group with ID '{dto.OrphanPaymentId}' not found");
        }

        paymentGroup.IsBatchUploaded = dto.IsUploaded;
        paymentGroup.UploadDate = dto.IsUploaded ? DateTime.UtcNow : null;

        _orphanPaymentRepository.Update(paymentGroup);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Payment group upload status updated successfully: {OrphanPaymentId}", dto.OrphanPaymentId);
    }

    #endregion

    #region UC-5.8: View Payment Groups (with filtering and pagination)

    public async Task<(IEnumerable<OrphanPaymentListDto> Items, int TotalCount)> GetPaymentGroupsAsync(OrphanPaymentFilterDto filter, Guid? userCharityId = null, string? userRole = null)
    {
        _logger.LogInformation("Getting payment groups with filter: {@Filter}", filter);

        // UC-ORP-08: an orphan's payment history — the batches containing that orphan. Bypasses
        // the paged repository read because membership lives on OrphanPaymentItem.
        if (filter.OrphanId.HasValue)
        {
            await EnsureOrphanInCallerScopeAsync(filter.OrphanId.Value, userCharityId, userRole, filter.CharityId);
            return await GetOrphanPaymentHistoryAsync(filter);
        }

        var (paymentGroups, totalCount) = await _orphanPaymentRepository.GetFilteredPaginatedAsync(
            filter.SearchTerm,
            filter.PaymentPeriodFrom,
            filter.PaymentPeriodTo,
            filter.GroupDateFrom,
            filter.GroupDateTo,
            filter.IsBatchUploaded,
            filter.CharityId,
            filter.PageNumber,
            filter.PageSize,
            filter.SortBy,
            filter.SortDescending);

        // Map to DTOs with orphan counts
        var paymentGroupDtos = new List<OrphanPaymentListDto>();
        foreach (var group in paymentGroups)
        {
            var dto = _mapper.Map<OrphanPaymentListDto>(group);
            dto.OrphanCount = await _orphanPaymentRepository.GetOrphanCountAsync(group.Id);
            paymentGroupDtos.Add(dto);
        }

        return (paymentGroupDtos, totalCount);
    }

    /// <summary>
    /// Export payment groups to Excel — §15.S.1's grid columns, every filtered row
    /// (OfficeProjectService.ExportProjectsToExcelAsync pattern).
    /// </summary>
    public async Task<byte[]> ExportPaymentGroupsToExcelAsync(OrphanPaymentFilterDto filter, Guid? userCharityId = null, string? userRole = null)
    {
        _logger.LogInformation("Exporting payment groups to Excel with filter: {@Filter}", filter);

        var exportFilter = filter ?? new OrphanPaymentFilterDto();
        exportFilter.PageNumber = 1;
        exportFilter.PageSize = int.MaxValue;

        var (groups, _) = await GetPaymentGroupsAsync(exportFilter, userCharityId, userRole);

        using (var package = new OfficeOpenXml.ExcelPackage())
        {
            var worksheet = package.Workbook.Worksheets.Add("دفعات الأيتام");

            worksheet.Cells[1, 1].Value = "الرقم";
            worksheet.Cells[1, 2].Value = "رقم الحصة";
            worksheet.Cells[1, 3].Value = "اسم المجموعة";
            worksheet.Cells[1, 4].Value = "فترة الدفع من";
            worksheet.Cells[1, 5].Value = "فترة الدفع إلى";
            worksheet.Cells[1, 6].Value = "عدد الأيتام";
            worksheet.Cells[1, 7].Value = "سعر الصرف";
            worksheet.Cells[1, 8].Value = "العملة";
            worksheet.Cells[1, 9].Value = "حالة الرفع";
            worksheet.Cells[1, 10].Value = "تاريخ المجموعة";
            worksheet.Cells[1, 11].Value = "تاريخ بدء التوزيع";
            worksheet.Cells[1, 12].Value = "أنشئ بواسطة";

            using (var range = worksheet.Cells[1, 1, 1, 12])
            {
                range.Style.Font.Bold = true;
                range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
            }

            var row = 2;
            var serial = 1;
            foreach (var group in groups)
            {
                worksheet.Cells[row, 1].Value = serial++;
                worksheet.Cells[row, 2].Value = group.BatchNo ?? "";
                worksheet.Cells[row, 3].Value = group.GroupName;
                worksheet.Cells[row, 4].Value = group.PaymentPeriodFrom.ToString("yyyy-MM-dd");
                worksheet.Cells[row, 5].Value = group.PaymentPeriodTo.ToString("yyyy-MM-dd");
                worksheet.Cells[row, 6].Value = group.OrphanCount;
                worksheet.Cells[row, 7].Value = group.ExchangeRate;
                worksheet.Cells[row, 8].Value = group.Currency ?? "";
                worksheet.Cells[row, 9].Value = group.IsBatchUploaded ? "مرفوع" : "معلق";
                worksheet.Cells[row, 10].Value = group.GroupDate.ToString("yyyy-MM-dd");
                worksheet.Cells[row, 11].Value = group.PaymentDate?.ToString("yyyy-MM-dd") ?? "";
                worksheet.Cells[row, 12].Value = group.CreatedByName ?? "";
                row++;
            }

            worksheet.Cells[1, 1, row - 1, 12].AutoFitColumns();

            return package.GetAsByteArray();
        }
    }

    /// <summary>
    /// UC-ORP-08 — the payment history of one orphan: the payment groups containing it, filtered
    /// by the caller's other constraints, newest batch first.
    /// </summary>
    private async Task<(IEnumerable<OrphanPaymentListDto> Items, int TotalCount)> GetOrphanPaymentHistoryAsync(OrphanPaymentFilterDto filter)
    {
        var items = await _orphanPaymentItemRepository.GetByOrphanIdAsync(filter.OrphanId!.Value);
        IEnumerable<OrphanPayment> groups = items
            .Where(i => i.OrphanPayment != null && !i.OrphanPayment.IsDeleted)
            .Select(i => i.OrphanPayment!)
            .DistinctBy(g => g.Id);

        if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
        {
            var term = filter.SearchTerm.Trim();
            groups = groups.Where(g =>
                (g.GroupName != null && g.GroupName.Contains(term)) ||
                (g.BatchNo != null && g.BatchNo.Contains(term)));
        }
        // P14 — the batch picker's رقم الدفعة filter is an exact match on the trimmed number,
        // distinct from the free-text SearchTerm above.
        if (!string.IsNullOrWhiteSpace(filter.BatchNo))
        {
            var batchNo = filter.BatchNo.Trim();
            groups = groups.Where(g => string.Equals(g.BatchNo?.Trim(), batchNo, StringComparison.Ordinal));
        }
        if (filter.PaymentPeriodFrom.HasValue)
        {
            groups = groups.Where(g => g.PaymentPeriodFrom >= filter.PaymentPeriodFrom.Value);
        }
        if (filter.PaymentPeriodTo.HasValue)
        {
            groups = groups.Where(g => g.PaymentPeriodTo <= filter.PaymentPeriodTo.Value);
        }
        if (filter.GroupDateFrom.HasValue)
        {
            groups = groups.Where(g => g.GroupDate >= filter.GroupDateFrom.Value);
        }
        if (filter.GroupDateTo.HasValue)
        {
            groups = groups.Where(g => g.GroupDate <= filter.GroupDateTo.Value);
        }
        if (filter.IsBatchUploaded.HasValue)
        {
            groups = groups.Where(g => g.IsBatchUploaded == filter.IsBatchUploaded.Value);
        }

        var ordered = filter.SortDescending
            ? groups.OrderByDescending(g => g.GroupDate)
            : groups.OrderBy(g => g.GroupDate);
        var totalCount = ordered.Count();
        var page = ordered
            .Skip((filter.PageNumber - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToList();

        var dtos = new List<OrphanPaymentListDto>();
        foreach (var group in page)
        {
            var dto = _mapper.Map<OrphanPaymentListDto>(group);
            dto.OrphanCount = await _orphanPaymentRepository.GetOrphanCountAsync(group.Id);
            dtos.Add(dto);
        }

        return (dtos, totalCount);
    }

    /// <summary>
    /// Tenancy for the orphan-scoped reads (UC-ORP-08/09): loads the orphan soft-delete-aware
    /// and returns its resolved charity. A charity caller out of scope, and an HQ caller
    /// narrowing to a charity the orphan is not in (P13), are plain 404s — no existence leak
    /// (P12: out-of-scope used to surface as a 403).
    /// </summary>
    private async Task<Guid?> EnsureOrphanInCallerScopeAsync(Guid orphanId, Guid? userCharityId, string? userRole, Guid? filterCharityId = null)
    {
        var orphan = await _orphanRepository.IncludeNavigationProperties()
            .FirstOrDefaultAsync(o => o.Id == orphanId && !o.IsDeleted);
        if (orphan == null)
        {
            throw new KeyNotFoundException($"Orphan with ID '{orphanId}' not found");
        }

        var orphanCharityId = orphan.FK_CharityId ?? orphan.Family?.FK_CharityId;
        if (userRole == "Charity" && userCharityId.HasValue && orphanCharityId != userCharityId.Value)
        {
            throw new KeyNotFoundException($"Orphan with ID '{orphanId}' not found");
        }
        if (userRole != "Charity" && filterCharityId.HasValue && orphanCharityId != filterCharityId.Value)
        {
            throw new KeyNotFoundException($"Orphan with ID '{orphanId}' not found");
        }

        return orphanCharityId;
    }

    #endregion

    #region UC-5.9: View Payment Group Details

    public async Task<OrphanPaymentDto> GetPaymentGroupDetailsAsync(Guid id, Guid? orphanId = null, Guid? userCharityId = null, string? userRole = null, Guid? charityId = null)
    {
        _logger.LogInformation("Getting payment group details: {Id}", id);

        // Soft-deleted batches read as missing (platform rule: deleted rows never surface in reads).
        var paymentGroup = await _orphanPaymentRepository.IncludeOrphans().FirstOrDefaultAsync(pg => pg.Id == id && !pg.IsDeleted);
        if (paymentGroup == null)
        {
            throw new KeyNotFoundException($"Payment group with ID '{id}' not found");
        }

        var dto = _mapper.Map<OrphanPaymentDto>(paymentGroup);

        // Load orphans with details
        var orphanItems = await _orphanPaymentItemRepository.GetWithOrphansByGroupAsync(id);
        dto.Orphans = orphanItems.Select(opi => MapOrphanPaymentItemDto(opi)).ToList();
        dto.OrphanCount = dto.Orphans.Count;

        // UC-ORP-09: one orphan's rows within the batch. The header stays complete — the batch
        // facts are the batch's — but the rows narrow to the caller's orphan. Without orphanId the
        // shape is exactly what UC-5.9 has always served (the 10-7/10-8 screens rely on that).
        if (orphanId.HasValue)
        {
            await EnsureOrphanInCallerScopeAsync(orphanId.Value, userCharityId, userRole);
            dto.Orphans = dto.Orphans.Where(o => o.OrphanId == orphanId.Value).ToList();
            dto.OrphanCount = dto.Orphans.Count;
            if (dto.Orphans.Count == 0)
            {
                throw new KeyNotFoundException($"Orphan '{orphanId}' has no rows in payment group '{id}'");
            }
        }
        else
        {
            // 10-7 (UC-PAY-07): the charity dimension on the batch read. A Charity caller is
            // pinned to its own rows (claim, never a payload value — an explicit charityId is
            // ignored for them); HQ may narrow with one. Zero matching rows still returns the
            // shared header with an empty item list — the batch exists, the charity just has no
            // orphans in it. Tenancy derivation: Orphan.FK_CharityId ?? Family.FK_CharityId.
            var scopeCharityId = userRole == "Charity" ? userCharityId : charityId;
            if (scopeCharityId.HasValue)
            {
                dto.Orphans = dto.Orphans.Where(o => o.CharityId == scopeCharityId.Value).ToList();
                dto.OrphanCount = dto.Orphans.Count;
            }
        }

        // Name joins batch-loaded (§15.S.3) — one charity query for the whole grid, no N+1.
        // Review P19: runs for BOTH branches — the single-orphan (UC-ORP-09) mode used to
        // return before the join, leaving the charity column blank on that surface.
        var charityIds = dto.Orphans
            .Where(o => o.CharityId.HasValue)
            .Select(o => o.CharityId!.Value)
            .Distinct()
            .ToList();
        if (charityIds.Count > 0)
        {
            var charityNames = await _charityRepository.AsQueryable()
                .Where(c => charityIds.Contains(c.Id) && !c.IsDeleted)
                .Select(c => new { c.Id, c.Name })
                .ToListAsync();
            var nameById = charityNames.ToDictionary(c => c.Id, c => c.Name);
            foreach (var row in dto.Orphans)
            {
                if (row.CharityId.HasValue && nameById.TryGetValue(row.CharityId.Value, out var name))
                {
                    row.CharityName = name;
                }
            }
        }

        // Load statistics
        dto.OrphanCountByCharity = await _orphanPaymentRepository.GetOrphanCountByCharityAsync(id);
        dto.OrphanCountByRegion = await _orphanPaymentRepository.GetOrphanCountByRegionAsync(id);

        return dto;
    }

    #endregion

    #region UC-ORP-11: Batch Number Reference List

    public async Task<List<BatchNumberDto>> GetBatchNumbersAsync(Guid? userCharityId, string? userRole, Guid? charityId = null)
    {
        _logger.LogInformation("Getting distinct batch numbers (UC-ORP-11), role: {Role}", userRole);

        // P10/P1: the distinct-and-latest projection runs in SQL — the whole payment table no
        // longer crosses the wire, and deleted batches drop out of the picker.
        var query = _orphanPaymentRepository.IncludeNavigationProperties()
            .Where(p => !p.IsDeleted && !string.IsNullOrEmpty(p.BatchNo));

        // Charity scoping: batches containing the charity's orphans. HQ may narrow the same
        // way. Review P6: a Charity token without a parseable charity claim fails CLOSED —
        // the claim pins the scope, it can never fall through to the all-batches branch.
        if (userRole == "Charity" && !userCharityId.HasValue)
        {
            throw new UnauthorizedAccessException("Charity caller has no charity scope");
        }
        var scopeCharityId = userRole == "Charity" ? userCharityId : charityId;
        if (scopeCharityId.HasValue)
        {
            var groupIds = (await _orphanPaymentItemRepository.GetGroupIdsByCharityAsync(scopeCharityId.Value)).ToList();
            query = query.Where(p => groupIds.Contains(p.Id));
        }

        var rows = await query
            .GroupBy(p => p.BatchNo!.Trim())
            .Select(g => new BatchNumberDto { BatchNo = g.Key, LatestGroupDate = g.Max(p => p.GroupDate) })
            .ToListAsync();

        return rows
            .OrderByDescending(b => b.LatestGroupDate)
            .ToList();
    }

    #endregion

    #region UC-5.10: Export Payment Group Report

    public async Task<byte[]> ExportPaymentGroupAsync(Guid id, string format = "Excel", bool includePhotos = false, string groupBy = "None")
    {
        _logger.LogInformation("Exporting payment group {Id} as {Format}", id, format);

        var paymentGroup = await GetPaymentGroupDetailsAsync(id);

        // TODO: Implement actual export logic
        // For now, return a placeholder
        _logger.LogInformation("Export functionality not yet implemented for format: {Format}", format);

        return Array.Empty<byte>();
    }

    #endregion

    #region UC-5.11: Assign Batch Number

    public async Task AssignBatchNumberAsync(AssignBatchNumberDto dto)
    {
        _logger.LogInformation("Assigning batch number to payment group: {OrphanPaymentId}", dto.OrphanPaymentId);

        // Review P4: write-path fetch honours soft delete — a deleted batch reads as missing,
        // instead of being resurrected and mutated (FindAsync ignores IsDeleted).
        var paymentGroup = await GetLivePaymentGroupAsync(dto.OrphanPaymentId);
        if (paymentGroup == null)
        {
            throw new KeyNotFoundException($"Payment group with ID '{dto.OrphanPaymentId}' not found");
        }

        string? batchNo = dto.BatchNo;

        // Auto-generate if requested
        if (dto.AutoGenerate || string.IsNullOrWhiteSpace(batchNo))
        {
            batchNo = await GenerateNextBatchNumberAsync();
        }
        else
        {
            // Check uniqueness if manually provided
            if (!await IsBatchNoUniqueAsync(batchNo, dto.OrphanPaymentId))
            {
                throw new InvalidOperationException($"Batch number '{batchNo}' already in use");
            }
        }

        paymentGroup.BatchNo = batchNo;

        _orphanPaymentRepository.Update(paymentGroup);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Batch number assigned successfully to payment group: {OrphanPaymentId}", dto.OrphanPaymentId);
    }

    #endregion

    #region Orphan Selection for Adding to Group (UC-5.3)

    public async Task<(IEnumerable<OrphanForPaymentListDto> Items, int TotalCount)> GetAvailableOrphansAsync(OrphanFilterForPaymentDto filter)
    {
        _logger.LogInformation("Getting available orphans for payment group with filter: {@Filter}", filter);

        // Get orphans based on filter
        var (orphans, totalCount) = await _orphanRepository.SearchFilteredAsync(
            filter.SearchTerm,
            filter.CharityId,
            filter.RegionId,
            filter.CenterId,
            filter.SponsorshipStatus,
            filter.AgeFrom,
            filter.AgeTo,
            filter.Gender,
            filter.PageNumber,
            filter.PageSize);

        // Get orphans already in group if group ID provided
        // 10-2 defect 7: loaded ONCE as an id→itemId map (was re-queried per row)
        Dictionary<Guid, Guid>? itemsByOrphanId = null;
        if (filter.OrphanPaymentId.HasValue)
        {
            itemsByOrphanId = (await _orphanPaymentItemRepository.GetByPaymentGroupIdAsync(filter.OrphanPaymentId.Value))
                .ToDictionary(opi => opi.OrphanId, opi => opi.Id);
        }

        // Map to DTOs
        var orphanDtos = new List<OrphanForPaymentListDto>();
        foreach (var orphan in orphans)
        {
            bool isInGroup = itemsByOrphanId?.ContainsKey(orphan.Id) ?? false;
            Guid? orphanPaymentItemId = isInGroup ? itemsByOrphanId![orphan.Id] : null;

            var dto = _mapper.Map<OrphanForPaymentListDto>(orphan);
            dto.IsInGroup = isInGroup;
            dto.OrphanPaymentItemId = orphanPaymentItemId;

            // Calculate age if date of birth exists
            if (orphan.DateOfBirth.HasValue)
            {
                dto.Age = DateTime.UtcNow.Year - orphan.DateOfBirth.Value.Year -
                    (DateTime.UtcNow.DayOfYear < orphan.DateOfBirth.Value.DayOfYear ? 1 : 0);
            }

            // Determine sponsorship status
            dto.SponsorshipStatus = orphan.SponsorId.HasValue ? "Sponsored" : "Unsponsored";

            orphanDtos.Add(dto);
        }

        return (orphanDtos, totalCount);
    }

    #endregion

    #region Helper Methods

    public async Task<OrphanPaymentDto?> GetByIdAsync(Guid id)
    {
        // Soft-deleted batches read as missing (FindAsync ignores IsDeleted).
        var paymentGroup = await _orphanPaymentRepository.AsQueryable()
            .FirstOrDefaultAsync(pg => pg.Id == id && !pg.IsDeleted);
        if (paymentGroup == null) return null;

        var dto = _mapper.Map<OrphanPaymentDto>(paymentGroup);
        dto.OrphanCount = await _orphanPaymentRepository.GetOrphanCountAsync(id);
        dto.OrphanCountByCharity = await _orphanPaymentRepository.GetOrphanCountByCharityAsync(id);
        dto.OrphanCountByRegion = await _orphanPaymentRepository.GetOrphanCountByRegionAsync(id);

        return dto;
    }

    public async Task<OrphanPaymentDto?> GetByBatchNoAsync(string batchNo, Guid? userCharityId = null, string? userRole = null)
    {
        var paymentGroup = await _orphanPaymentRepository.GetByBatchNoAsync(batchNo);
        if (paymentGroup == null) return null;

        // 10-6 (AC 3): a Charity caller resolves only batches it participates in — a batch
        // outside the charity's enrolment reads as missing (404-not-leak), the same
        // participation set the batch-numbers picker scopes by. Review P6: a Charity token
        // without a parseable charity claim fails CLOSED — never the unscoped branch.
        if (userRole == "Charity")
        {
            if (!userCharityId.HasValue)
            {
                throw new UnauthorizedAccessException("Charity caller has no charity scope");
            }
            var groupIds = (await _orphanPaymentItemRepository.GetGroupIdsByCharityAsync(userCharityId.Value)).ToList();
            if (!groupIds.Contains(paymentGroup.Id)) return null;

            // Review P14: participation gates the header; the rows scope like {id}/details —
            // a participating charity sees its own rows only, never the full batch ledger.
            return await GetPaymentGroupDetailsAsync(paymentGroup.Id, null, userCharityId, userRole);
        }

        return await GetByIdAsync(paymentGroup.Id);
    }

    public async Task<bool> IsBatchNoUniqueAsync(string batchNo, Guid? excludeId = null)
    {
        return await _orphanPaymentRepository.IsBatchNoUniqueAsync(batchNo, excludeId);
    }

    public async Task DeletePaymentGroupAsync(Guid id)
    {
        _logger.LogInformation("Deleting payment group: {Id}", id);

        // Soft-deleted batches read as missing — a repeat delete answers 404, not a second pass.
        var paymentGroup = await _orphanPaymentRepository.AsQueryable()
            .FirstOrDefaultAsync(pg => pg.Id == id && !pg.IsDeleted);
        if (paymentGroup == null)
        {
            throw new KeyNotFoundException($"Payment group with ID '{id}' not found");
        }

        // §25.6 A1 (UC-PAY-05 AC 3): delete is limited to pre-disbursement batches — a row
        // that was received, transferred or cheque-issued means money moved under this batch.
        var items = await _orphanPaymentItemRepository.GetByPaymentGroupIdAsync(id);
        if (items.Any(i => i.IsGotIt || !string.IsNullOrEmpty(i.TransferNo) || !string.IsNullOrEmpty(i.ChiqueNum)))
        {
            throw new InvalidOperationException(
                "Cannot delete a payment batch with disbursed rows (received, transferred or cheque-issued)");
        }

        // PeriodicOrphanReport.OrphanPaymentId is a nullable snapshot FK pinning report
        // history to the batch — deleting the batch would orphan those snapshots, so a
        // referencing report blocks the delete instead of the FK being cleared.
        if (await _periodicReportRepository.CheckExistAsync(r => r.OrphanPaymentId == id))
        {
            throw new InvalidOperationException(
                "Cannot delete a payment batch referenced by periodic orphan reports");
        }

        // Soft delete (platform convention): flag rows + header via the change tracker —
        // the previous path hard-deleted through DbSet.Remove (10-5 defect 3).
        foreach (var item in items)
        {
            item.IsDeleted = true;
            _orphanPaymentItemRepository.Update(item);
        }
        paymentGroup.IsDeleted = true;
        _orphanPaymentRepository.Update(paymentGroup);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Payment group soft-deleted successfully: {Id}", id);
    }

    public async Task<int> GetTotalOrphanCountAsync(Guid orphanPaymentId)
    {
        return await _orphanPaymentRepository.GetOrphanCountAsync(orphanPaymentId);
    }

    public async Task<Dictionary<Guid, int>> GetOrphanCountByCharityAsync(Guid orphanPaymentId)
    {
        return await _orphanPaymentRepository.GetOrphanCountByCharityAsync(orphanPaymentId);
    }

    public async Task<Dictionary<int, int>> GetOrphanCountByRegionAsync(Guid orphanPaymentId)
    {
        return await _orphanPaymentRepository.GetOrphanCountByRegionAsync(orphanPaymentId);
    }

    #endregion

    #region Private Helper Methods

    /// <summary>
    /// Review P4: write-path entity fetch that honours soft delete — RepositoryBase's
    /// FindAsync-based GetByIdAsync resurrects deleted batches, so every mutating path
    /// resolves its aggregate through this filtered read instead.
    /// </summary>
    private async Task<OrphanPayment?> GetLivePaymentGroupAsync(Guid id)
    {
        return await _orphanPaymentRepository.AsQueryable()
            .FirstOrDefaultAsync(pg => pg.Id == id && !pg.IsDeleted);
    }

    private async Task<string> GenerateNextBatchNumberAsync()
    {
        return await _orphanPaymentRepository.GetNextBatchNumberAsync();
    }

    private OrphanPaymentItemDto MapOrphanPaymentItemDto(OrphanPaymentItem item)
    {
        var dto = _mapper.Map<OrphanPaymentItemDto>(item);

        if (item.Orphan != null)
        {
            dto.OrphanCode = item.Orphan.Code;
            dto.OrphanFullName = item.Orphan.FullName;
            dto.OrphanMonthlyAmount = item.Orphan.MonthlyAmount;
            dto.OrphanEducationLevel = item.Orphan.EducationLevel?.Name;
            dto.SponsorshipStartDate = item.Orphan.SponsorshipStartDate;

            // Calculate age
            if (item.Orphan.DateOfBirth.HasValue)
            {
                dto.OrphanAge = DateTime.UtcNow.Year - item.Orphan.DateOfBirth.Value.Year -
                    (DateTime.UtcNow.DayOfYear < item.Orphan.DateOfBirth.Value.DayOfYear ? 1 : 0);
            }

            // Load family name if available
            if (item.Orphan.Family != null)
            {
                dto.OrphanFamilyName = item.Orphan.Family.HeadOfFamily;
            }

            // 10-7: the row's charity — derived tenancy (Orphan's own, else the family's). The
            // name join happens batch-loaded in GetPaymentGroupDetailsAsync (no N+1 here).
            dto.CharityId = item.Orphan.FK_CharityId ?? item.Orphan.Family?.FK_CharityId;

            dto.SponsorshipStatus = item.Orphan.SponsorId.HasValue ? "Sponsored" : "Unsponsored";
        }

        return dto;
    }

    #endregion
}
