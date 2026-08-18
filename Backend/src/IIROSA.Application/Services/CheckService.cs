using IIROSA.Application.DTOs.CheckManagement;
using IIROSA.Application.Interfaces;
using IIROSA.Domain.Entities;
using IIROSA.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using AutoMapper;

namespace IIROSA.Application.Services;

/// <summary>
/// Check Service Implementation
/// Implements business logic for Check management following UC-11.1 to UC-11.10
/// IMPORTANT: Only Admin, Super Admin, and Accountant roles can access this service.
/// Charity users are explicitly blocked from this module.
/// </summary>
public class CheckService : ICheckService
{
    private readonly ICheckRepository _checkRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<CheckService> _logger;

    public CheckService(
        ICheckRepository checkRepository,
        IMapper mapper,
        ILogger<CheckService> logger)
    {
        _checkRepository = checkRepository;
        _mapper = mapper;
        _logger = logger;
    }

    // ========== CRUD Operations ==========

    public async Task<CheckPagedResult<CheckListDto>> GetChecksFilteredAsync(CheckFilterDto filter)
    {
        try
        {
            _logger.LogInformation("Retrieving checks with filter: {@Filter}", filter);

            System.Linq.Expressions.Expression<Func<Check, bool>>? filterExpression = null;

            if (filter != null)
            {
                filterExpression = c =>
                    (string.IsNullOrWhiteSpace(filter.CheckStatus) || c.CheckStatus == filter.CheckStatus) &&
                    (!filter.FK_BankId.HasValue || c.FK_BankId == filter.FK_BankId.Value) &&
                    (string.IsNullOrWhiteSpace(filter.Currency) || c.Currency == filter.Currency) &&
                    (!filter.StartDate.HasValue || c.CheckDate >= filter.StartDate.Value) &&
                    (!filter.EndDate.HasValue || c.CheckDate <= filter.EndDate.Value) &&
                    (!filter.MinAmount.HasValue || c.Amount >= filter.MinAmount.Value) &&
                    (!filter.MaxAmount.HasValue || c.Amount <= filter.MaxAmount.Value) &&
                    (string.IsNullOrWhiteSpace(filter.SearchText) ||
                     c.CheckNumber.Contains(filter.SearchText) ||
                     c.BeneficiaryName.Contains(filter.SearchText));
            }

            var (items, totalCount) = await _checkRepository.GetChecksPagedAsync(
                filterExpression,
                q => q.OrderByDescending(c => c.CheckDate),
                filter?.Page ?? 1,
                filter?.PageSize ?? 20);

            var checkDtos = _mapper.Map<List<CheckListDto>>(items);

            return new CheckPagedResult<CheckListDto>
            {
                Items = checkDtos,
                TotalCount = totalCount,
                Page = filter?.Page ?? 1,
                PageSize = filter?.PageSize ?? 20
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving checks with filter: {@Filter}", filter);
            throw;
        }
    }

    public async Task<CheckDetailDto?> GetCheckByIdAsync(Guid id)
    {
        try
        {
            var check = await _checkRepository.GetByIdAsync(id);
            if (check == null)
            {
                _logger.LogWarning("Check with ID {CheckId} not found", id);
                return null;
            }

            return _mapper.Map<CheckDetailDto>(check);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving check detail for ID: {CheckId}", id);
            throw;
        }
    }

    public async Task<CheckDetailDto> CreateCheckAsync(CreateCheckDto dto)
    {
        try
        {
            _logger.LogInformation("Creating new check: {@Check}", dto);

            var check = _mapper.Map<Check>(dto);
            check.CheckStatus = "Pending";

            // Auto-generate amount in words if not provided
            if (string.IsNullOrWhiteSpace(check.AmountInWords))
            {
                check.AmountInWords = ConvertAmountToWords(check.Amount, check.Currency);
            }

            await _checkRepository.AddAsync(check);
            await _checkRepository.SaveChangesAsync();

            _logger.LogInformation("Check created successfully with ID: {CheckId}", check.Id);

            return _mapper.Map<CheckDetailDto>(check);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while creating check: {@Check}", dto);
            throw;
        }
    }

    public async Task<CheckDetailDto> UpdateCheckAsync(Guid id, UpdateCheckDto dto)
    {
        try
        {
            var check = await _checkRepository.GetByIdAsync(id);
            if (check == null)
            {
                throw new InvalidOperationException($"Check with ID {id} not found");
            }

            // Validate that check can be modified
            if (!check.CanModify)
            {
                throw new InvalidOperationException("Cannot modify a check that is not in Pending status");
            }

            // Update only non-null properties
            if (!string.IsNullOrWhiteSpace(dto.CheckNumber))
                check.CheckNumber = dto.CheckNumber;

            if (dto.CheckDate.HasValue)
                check.CheckDate = dto.CheckDate.Value;

            if (dto.DueDate.HasValue)
                check.DueDate = dto.DueDate.Value;

            if (!string.IsNullOrWhiteSpace(dto.Currency))
                check.Currency = dto.Currency;

            if (dto.BeneficiaryType != null)
                check.BeneficiaryType = dto.BeneficiaryType;

            if (!string.IsNullOrWhiteSpace(dto.BeneficiaryName))
                check.BeneficiaryName = dto.BeneficiaryName;

            if (dto.FK_ChequeBeneficiaryId.HasValue)
                check.FK_ChequeBeneficiaryId = dto.FK_ChequeBeneficiaryId.Value;

            if (dto.BeneficiaryAddress != null)
                check.BeneficiaryAddress = dto.BeneficiaryAddress;

            if (dto.BeneficiaryPhone != null)
                check.BeneficiaryPhone = dto.BeneficiaryPhone;

            if (dto.BeneficiaryEmail != null)
                check.BeneficiaryEmail = dto.BeneficiaryEmail;

            if (dto.BeneficiaryIdNumber != null)
                check.BeneficiaryIdNumber = dto.BeneficiaryIdNumber;

            if (dto.Amount.HasValue)
            {
                check.Amount = dto.Amount.Value;
                // Auto-generate amount in words
                check.AmountInWords = ConvertAmountToWords(check.Amount, check.Currency);
            }

            if (!string.IsNullOrWhiteSpace(dto.PaymentReason))
                check.PaymentReason = dto.PaymentReason;

            if (dto.PaymentDescription != null)
                check.PaymentDescription = dto.PaymentDescription;

            if (dto.FK_BankId.HasValue)
                check.FK_BankId = dto.FK_BankId.Value;

            if (dto.BankBranch != null)
                check.BankBranch = dto.BankBranch;

            if (dto.AccountNumber != null)
                check.AccountNumber = dto.AccountNumber;

            if (dto.Notes != null)
                check.Notes = dto.Notes;

            _checkRepository.Update(check);
            await _checkRepository.SaveChangesAsync();

            _logger.LogInformation("Check updated successfully: {CheckId}", id);

            return _mapper.Map<CheckDetailDto>(check);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating check {CheckId}: {@Check}", id, dto);
            throw;
        }
    }

    public async Task DeleteCheckAsync(Guid id)
    {
        try
        {
            var check = await _checkRepository.GetByIdAsync(id);
            if (check == null)
            {
                throw new InvalidOperationException($"Check with ID {id} not found");
            }

            // Validate that check can be deleted
            if (!check.CanModify)
            {
                throw new InvalidOperationException("Cannot delete a check that is not in Pending status");
            }

            _checkRepository.Delete(check);
            await _checkRepository.SaveChangesAsync();

            _logger.LogInformation("Check deleted successfully: {CheckId}", id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while deleting check {CheckId}", id);
            throw;
        }
    }

    // ========== Check-Specific Operations ==========

    public async Task SetCheckAmountAsync(Guid id, SetCheckAmountDto dto)
    {
        try
        {
            var check = await _checkRepository.GetByIdAsync(id);
            if (check == null)
            {
                throw new InvalidOperationException($"Check with ID {id} not found");
            }

            if (!check.CanModify)
            {
                throw new InvalidOperationException("Cannot modify a check that is not in Pending status");
            }

            check.Amount = dto.Amount;
            check.Currency = dto.Currency;
            check.AmountInWords = string.IsNullOrWhiteSpace(dto.AmountInWords)
                ? ConvertAmountToWords(dto.Amount, dto.Currency)
                : dto.AmountInWords;

            _checkRepository.Update(check);
            await _checkRepository.SaveChangesAsync();

            _logger.LogInformation("Check amount updated for check {CheckId}: Amount={Amount}, Currency={Currency}", id, dto.Amount, dto.Currency);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while setting check amount for check {CheckId}", id);
            throw;
        }
    }

    public async Task SetCheckDateAsync(Guid id, SetCheckDateDto dto)
    {
        try
        {
            var check = await _checkRepository.GetByIdAsync(id);
            if (check == null)
            {
                throw new InvalidOperationException($"Check with ID {id} not found");
            }

            if (!check.CanModify)
            {
                throw new InvalidOperationException("Cannot modify a check that is not in Pending status");
            }

            // Validate dates
            if (dto.DueDate.HasValue && dto.DueDate.Value < dto.CheckDate)
            {
                throw new InvalidOperationException("Due date cannot be before check date");
            }

            check.CheckDate = dto.CheckDate;
            check.DueDate = dto.DueDate;

            _checkRepository.Update(check);
            await _checkRepository.SaveChangesAsync();

            _logger.LogInformation("Check dates updated for check {CheckId}: CheckDate={CheckDate}, DueDate={DueDate}", id, dto.CheckDate, dto.DueDate);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while setting check date for check {CheckId}", id);
            throw;
        }
    }

    public async Task MarkCheckAsClearedAsync(Guid id, MarkCheckClearedDto dto)
    {
        try
        {
            var check = await _checkRepository.GetByIdAsync(id);
            if (check == null)
            {
                throw new InvalidOperationException($"Check with ID {id} not found");
            }

            if (!check.CanBeCleared)
            {
                throw new InvalidOperationException("Only issued checks can be marked as cleared");
            }

            check.CheckStatus = "Cleared";
            check.ClearanceDate = dto.ClearanceDate;
            check.BankReference = dto.BankReference;
            check.ClearanceNotes = dto.ClearanceNotes;

            _checkRepository.Update(check);
            await _checkRepository.SaveChangesAsync();

            _logger.LogInformation("Check marked as cleared: {CheckId}", id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while marking check as cleared {CheckId}", id);
            throw;
        }
    }

    public async Task VoidCheckAsync(Guid id, VoidCheckDto dto)
    {
        try
        {
            var check = await _checkRepository.GetByIdAsync(id);
            if (check == null)
            {
                throw new InvalidOperationException($"Check with ID {id} not found");
            }

            if (!check.CanBeVoided)
            {
                throw new InvalidOperationException("Check cannot be voided");
            }

            if (string.IsNullOrWhiteSpace(dto.VoidNotes))
            {
                throw new InvalidOperationException("Void notes are required");
            }

            check.CheckStatus = "Void";
            check.VoidDate = dto.VoidDate;
            check.VoidReason = dto.VoidReason;
            check.VoidNotes = dto.VoidNotes;

            _checkRepository.Update(check);
            await _checkRepository.SaveChangesAsync();

            _logger.LogInformation("Check voided: {CheckId}, Reason: {Reason}", id, dto.VoidReason);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while voiding check {CheckId}", id);
            throw;
        }
    }

    public async Task<CheckReconciliationDto> ReconcileChecksAsync(CheckReconciliationDto reconciliation)
    {
        try
        {
            _logger.LogInformation("Reconciling checks: {CheckCount} checks", reconciliation.ReconciledCheckIds.Count);

            foreach (var checkId in reconciliation.ReconciledCheckIds)
            {
                var check = await _checkRepository.GetByIdAsync(checkId);
                if (check != null && check.CheckStatus == "Issued")
                {
                    check.CheckStatus = "Cleared";
                    check.ClearanceDate = reconciliation.ReconciliationDate;
                    check.ClearanceNotes = $"Reconciled: {reconciliation.ReconciliationNotes}";

                    _checkRepository.Update(check);
                }
            }

            await _checkRepository.SaveChangesAsync();

            reconciliation.TotalChecksReconciled = reconciliation.ReconciledCheckIds.Count;

            var unreconciledChecks = await _checkRepository.GetUnreconciledChecksAsync();
            reconciliation.TotalAmountReconciled = unreconciledChecks
                .Where(c => reconciliation.ReconciledCheckIds.Contains(c.Id))
                .Sum(c => c.Amount);

            reconciliation.UnreconciledChecks = unreconciledChecks.Count();

            _logger.LogInformation("Checks reconciled successfully: {Count} checks", reconciliation.TotalChecksReconciled);

            return reconciliation;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while reconciling checks");
            throw;
        }
    }

    public async Task<CheckReportDto> GenerateCheckReportAsync(CheckReportFilterDto filter)
    {
        try
        {
            _logger.LogInformation("Generating check report: {@Filter}", filter);

            var allChecks = await _checkRepository.GetByDateRangeAsync(filter.StartDate, filter.EndDate);
            var filteredChecks = allChecks.AsQueryable();

            // Apply filters
            if (!string.IsNullOrWhiteSpace(filter.CheckStatus))
            {
                filteredChecks = filteredChecks.Where(c => c.CheckStatus == filter.CheckStatus);
            }

            if (filter.FK_BankId.HasValue)
            {
                filteredChecks = filteredChecks.Where(c => c.FK_BankId == filter.FK_BankId.Value);
            }

            if (!string.IsNullOrWhiteSpace(filter.Currency))
            {
                filteredChecks = filteredChecks.Where(c => c.Currency == filter.Currency);
            }

            var checksList = filteredChecks.ToList();

            var report = new CheckReportDto
            {
                Summary = new CheckStatusSummaryDto
                {
                    TotalChecks = checksList.Count,
                    PendingChecks = checksList.Count(c => c.CheckStatus == "Pending"),
                    IssuedChecks = checksList.Count(c => c.CheckStatus == "Issued"),
                    ClearedChecks = checksList.Count(c => c.CheckStatus == "Cleared"),
                    VoidedChecks = checksList.Count(c => c.CheckStatus == "Void"),
                    TotalAmount = checksList.Sum(c => c.Amount),
                    PendingAmount = checksList.Where(c => c.CheckStatus == "Pending").Sum(c => c.Amount),
                    ClearedAmount = checksList.Where(c => c.CheckStatus == "Cleared").Sum(c => c.Amount),
                    AmountByCurrency = checksList.GroupBy(c => c.Currency)
                        .ToDictionary(g => g.Key, g => g.Sum(c => c.Amount)),
                    CountByBank = checksList.Where(c => c.FK_BankId.HasValue)
                        .GroupBy(c => c.FK_BankId!.Value)
                        .ToDictionary(g => g.Key.ToString(), g => g.Count())
                },
                DetailedChecks = _mapper.Map<List<CheckListDto>>(checksList),
                ClearedChecks = _mapper.Map<List<CheckListDto>>(checksList.Where(c => c.CheckStatus == "Cleared")),
                PendingChecks = _mapper.Map<List<CheckListDto>>(checksList.Where(c => c.CheckStatus == "Pending")),
                VoidChecks = _mapper.Map<List<CheckListDto>>(checksList.Where(c => c.CheckStatus == "Void")),
                ReportGeneratedOn = DateTime.UtcNow
            };

            _logger.LogInformation("Check report generated successfully");

            return report;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while generating check report");
            throw;
        }
    }

    // ========== View Operations ==========

    public async Task<List<CheckListDto>> GetPendingChecksAsync()
    {
        try
        {
            var pendingChecks = await _checkRepository.GetPendingChecksAsync();
            return _mapper.Map<List<CheckListDto>>(pendingChecks);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving pending checks");
            throw;
        }
    }

    public async Task<List<CheckListDto>> GetIssuedChecksAsync()
    {
        try
        {
            var issuedChecks = await _checkRepository.GetIssuedChecksAsync();
            return _mapper.Map<List<CheckListDto>>(issuedChecks);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving issued checks");
            throw;
        }
    }

    public async Task<List<CheckListDto>> GetClearedChecksAsync()
    {
        try
        {
            var clearedChecks = await _checkRepository.GetClearedChecksAsync();
            return _mapper.Map<List<CheckListDto>>(clearedChecks);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving cleared checks");
            throw;
        }
    }

    public async Task<List<CheckListDto>> GetVoidedChecksAsync()
    {
        try
        {
            var voidedChecks = await _checkRepository.GetVoidedChecksAsync();
            return _mapper.Map<List<CheckListDto>>(voidedChecks);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving voided checks");
            throw;
        }
    }

    public async Task<List<CheckListDto>> GetUnreconciledChecksAsync()
    {
        try
        {
            var unreconciledChecks = await _checkRepository.GetUnreconciledChecksAsync();
            return _mapper.Map<List<CheckListDto>>(unreconciledChecks);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving unreconciled checks");
            throw;
        }
    }

    public async Task<CheckStatusSummaryDto> GetCheckStatusSummaryAsync()
    {
        try
        {
            var allChecks = await _checkRepository.GetAllAsync();

            var summary = new CheckStatusSummaryDto
            {
                TotalChecks = allChecks.Count(),
                PendingChecks = allChecks.Count(c => c.CheckStatus == "Pending"),
                IssuedChecks = allChecks.Count(c => c.CheckStatus == "Issued"),
                ClearedChecks = allChecks.Count(c => c.CheckStatus == "Cleared"),
                VoidedChecks = allChecks.Count(c => c.CheckStatus == "Void"),
                TotalAmount = allChecks.Sum(c => c.Amount),
                PendingAmount = allChecks.Where(c => c.CheckStatus == "Pending").Sum(c => c.Amount),
                ClearedAmount = allChecks.Where(c => c.CheckStatus == "Cleared").Sum(c => c.Amount),
                AmountByCurrency = allChecks.GroupBy(c => c.Currency)
                    .ToDictionary(g => g.Key, g => g.Sum(c => c.Amount)),
                CountByBank = allChecks.Where(c => c.FK_BankId.HasValue)
                    .GroupBy(c => c.FK_BankId!.Value)
                    .ToDictionary(g => g.Key.ToString(), g => g.Count())
            };

            return summary;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving check status summary");
            throw;
        }
    }

    // ========== Export ==========

    public async Task<byte[]> ExportChecksToExcelAsync(CheckFilterDto filter)
    {
        try
        {
            _logger.LogInformation("Exporting checks to Excel with filter: {@Filter}", filter);

            var result = await GetChecksFilteredAsync(filter);

            // TODO: Implement Excel export using EPPlus or ClosedXML
            throw new NotImplementedException("Excel export functionality not yet implemented");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while exporting checks to Excel");
            throw;
        }
    }

    // ========== Helper Methods ==========

    private string ConvertAmountToWords(decimal amount, string currency)
    {
        // TODO: Implement proper number-to-words conversion
        // For now, return a placeholder
        return $"{amount} {currency}";
    }
}
