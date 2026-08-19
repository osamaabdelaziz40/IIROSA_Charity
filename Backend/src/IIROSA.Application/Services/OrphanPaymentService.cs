using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using IIROSA.Application.Interfaces;
using IIROSA.Application.DTOs.OrphanPayment;
using IIROSA.Domain.Entities;
using IIROSA.Domain.Interfaces;
using AutoMapper;

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

    public OrphanPaymentService(
        IIROSA.Application.Interfaces.ICharityWriteGuard charityWriteGuard,
        IOrphanPaymentRepository orphanPaymentRepository,
        IOrphanPaymentItemRepository orphanPaymentItemRepository,
        IOrphanRepository orphanRepository,
        ICharityRepository charityRepository,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<OrphanPaymentService> logger)
    {
        _charityWriteGuard = charityWriteGuard;
        _orphanPaymentRepository = orphanPaymentRepository;
        _orphanPaymentItemRepository = orphanPaymentItemRepository;
        _orphanRepository = orphanRepository;
        _charityRepository = charityRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    #region UC-5.1: Create Orphan Payment Group

    public async Task<OrphanPaymentDto> CreatePaymentGroupAsync(CreateOrphanPaymentDto dto)
    {
        // UC-CHR-07/08/09: head office can lock a charity or withdraw its add/edit rights.
        await _charityWriteGuard.EnsureCanAddAsync();

        _logger.LogInformation("Creating new orphan payment group: {GroupName}", dto.GroupName);

        // Validate date range (UC-5.1 Alternative Flow 8a)
        if (dto.PaymentPeriodFrom > dto.PaymentPeriodTo)
        {
            throw new InvalidOperationException("Payment period start date must be before end date");
        }

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

        var paymentGroup = await _orphanPaymentRepository.GetByIdAsync(dto.OrphanPaymentId);
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

        var paymentGroup = await _orphanPaymentRepository.GetByIdAsync(orphanPaymentId);
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

        var paymentGroup = await _orphanPaymentRepository.GetByIdAsync(dto.OrphanPaymentId);
        if (paymentGroup == null)
        {
            throw new KeyNotFoundException($"Payment group with ID '{dto.OrphanPaymentId}' not found");
        }

        // Get current orphans in group
        var existingOrphanIds = (await _orphanPaymentItemRepository.GetByPaymentGroupIdAsync(dto.OrphanPaymentId))
            .Select(opi => opi.OrphanId)
            .ToHashSet();

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
            var orphan = await _orphanRepository.GetByIdAsync(orphanId);
            if (orphan == null)
            {
                _logger.LogWarning("Orphan with ID {OrphanId} not found, skipping", orphanId);
                skippedCount++;
                continue;
            }

            // Create orphan payment item
            var orphanPaymentItem = new OrphanPaymentItem
            {
                Id = Guid.NewGuid(),
                OrphanPaymentId = dto.OrphanPaymentId,
                OrphanId = orphanId,
                DisplayOrder = displayOrder++
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
        if (orphanPaymentItem == null)
        {
            throw new KeyNotFoundException($"Orphan payment item with ID '{orphanPaymentItemId}' not found");
        }

        _orphanPaymentItemRepository.Delete(orphanPaymentItem);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Orphan removed from payment group successfully: {OrphanPaymentItemId}", orphanPaymentItemId);
    }

    #endregion

    #region UC-5.5: Update Payment Group

    public async Task<OrphanPaymentDto> UpdatePaymentGroupAsync(UpdateOrphanPaymentDto dto)
    {
        // UC-CHR-07/08/09: head office can lock a charity or withdraw its add/edit rights.
        await _charityWriteGuard.EnsureCanUpdateAsync();

        _logger.LogInformation("Updating payment group: {Id}", dto.Id);

        var paymentGroup = await _orphanPaymentRepository.GetByIdAsync(dto.Id);
        if (paymentGroup == null)
        {
            throw new KeyNotFoundException($"Payment group with ID '{dto.Id}' not found");
        }

        // Validate date range
        if (dto.PaymentPeriodFrom > dto.PaymentPeriodTo)
        {
            throw new InvalidOperationException("Payment period start date must be before end date");
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
        paymentGroup.ExchangeRate = dto.ExchangeRate;
        paymentGroup.Currency = dto.Currency;
        paymentGroup.DontRemoveRate = dto.DontRemoveRate;
        paymentGroup.BatchNo = dto.BatchNo;
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

        var paymentGroup = await _orphanPaymentRepository.GetByIdAsync(dto.OrphanPaymentId);
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

    public async Task<(IEnumerable<OrphanPaymentListDto> Items, int TotalCount)> GetPaymentGroupsAsync(OrphanPaymentFilterDto filter)
    {
        _logger.LogInformation("Getting payment groups with filter: {@Filter}", filter);

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

    #endregion

    #region UC-5.9: View Payment Group Details

    public async Task<OrphanPaymentDto> GetPaymentGroupDetailsAsync(Guid id)
    {
        _logger.LogInformation("Getting payment group details: {Id}", id);

        var paymentGroup = await _orphanPaymentRepository.IncludeOrphans().FirstOrDefaultAsync(pg => pg.Id == id);
        if (paymentGroup == null)
        {
            throw new KeyNotFoundException($"Payment group with ID '{id}' not found");
        }

        var dto = _mapper.Map<OrphanPaymentDto>(paymentGroup);

        // Load orphans with details
        var orphanItems = await _orphanPaymentItemRepository.GetWithOrphansByGroupAsync(id);
        dto.Orphans = orphanItems.Select(opi => MapOrphanPaymentItemDto(opi)).ToList();
        dto.OrphanCount = dto.Orphans.Count;

        // Load statistics
        dto.OrphanCountByCharity = await _orphanPaymentRepository.GetOrphanCountByCharityAsync(id);
        dto.OrphanCountByRegion = await _orphanPaymentRepository.GetOrphanCountByRegionAsync(id);

        return dto;
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

        var paymentGroup = await _orphanPaymentRepository.GetByIdAsync(dto.OrphanPaymentId);
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
        HashSet<Guid>? existingOrphanIds = null;
        if (filter.OrphanPaymentId.HasValue)
        {
            existingOrphanIds = (await _orphanPaymentItemRepository.GetByPaymentGroupIdAsync(filter.OrphanPaymentId.Value))
                .Select(opi => opi.OrphanId)
                .ToHashSet();
        }

        // Map to DTOs
        var orphanDtos = new List<OrphanForPaymentListDto>();
        foreach (var orphan in orphans)
        {
            bool isInGroup = existingOrphanIds?.Contains(orphan.Id) ?? false;
            Guid? orphanPaymentItemId = null;

            if (isInGroup && filter.OrphanPaymentId.HasValue)
            {
                var existingItem = (await _orphanPaymentItemRepository.GetByPaymentGroupIdAsync(filter.OrphanPaymentId.Value))
                    .FirstOrDefault(opi => opi.OrphanId == orphan.Id);
                orphanPaymentItemId = existingItem?.Id;
            }

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
        var paymentGroup = await _orphanPaymentRepository.GetByIdAsync(id);
        if (paymentGroup == null) return null;

        var dto = _mapper.Map<OrphanPaymentDto>(paymentGroup);
        dto.OrphanCount = await _orphanPaymentRepository.GetOrphanCountAsync(id);
        dto.OrphanCountByCharity = await _orphanPaymentRepository.GetOrphanCountByCharityAsync(id);
        dto.OrphanCountByRegion = await _orphanPaymentRepository.GetOrphanCountByRegionAsync(id);

        return dto;
    }

    public async Task<OrphanPaymentDto?> GetByBatchNoAsync(string batchNo)
    {
        var paymentGroup = await _orphanPaymentRepository.GetByBatchNoAsync(batchNo);
        if (paymentGroup == null) return null;

        return await GetByIdAsync(paymentGroup.Id);
    }

    public async Task<bool> IsBatchNoUniqueAsync(string batchNo, Guid? excludeId = null)
    {
        return await _orphanPaymentRepository.IsBatchNoUniqueAsync(batchNo, excludeId);
    }

    public async Task DeletePaymentGroupAsync(Guid id)
    {
        _logger.LogInformation("Deleting payment group: {Id}", id);

        var paymentGroup = await _orphanPaymentRepository.GetByIdAsync(id);
        if (paymentGroup == null)
        {
            throw new KeyNotFoundException($"Payment group with ID '{id}' not found");
        }

        // Delete all orphan items in the group first
        await _orphanPaymentItemRepository.RemoveAllFromGroupAsync(id);

        // Delete the payment group
        _orphanPaymentRepository.Delete(paymentGroup);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Payment group deleted successfully: {Id}", id);
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

            // Load charity information
            if (item.Orphan.FK_CharityId.HasValue)
            {
                // Note: CharityId is int in Charity entity but Orphan uses Guid FK_CharityId
                // This might need adjustment based on actual schema
            }

            dto.SponsorshipStatus = item.Orphan.SponsorId.HasValue ? "Sponsored" : "Unsponsored";
        }

        return dto;
    }

    #endregion
}
