using AutoMapper;
using FluentValidation;
using IIROSA.Application.DTOs.CheckManagement;
using IIROSA.Application.Interfaces;
using IIROSA.Domain.Contracts.Persistence;
using IIROSA.Domain.Entities;
using IIROSA.Domain.Entities.Lookups;
using IIROSA.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace IIROSA.Application.Services;

/// <summary>
/// General cheques service (chapter 16, UC-CHQ-01..10).
///
/// Tenancy: every read and write is scoped to the caller's charity from the JWT.
/// A charity user is pinned to their own charity whatever they send; a head-office
/// user may pass an explicit charity id, is pinned to their country when the token
/// carries one, and otherwise sees all charities. Only the UnitOfWork persists.
/// </summary>
public class CheckService : ICheckService
{
    private readonly ICheckRepository _checkRepository;
    private readonly IRepository<Bank> _bankRepository;
    private readonly ICharityRepository _charityRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CheckService> _logger;
    private readonly ICurrentUserService _currentUser;
    private readonly IMapper _mapper;

    private readonly IValidator<CreateCheckDto> _createValidator;
    private readonly IValidator<UpdateCheckDto> _updateValidator;
    private readonly IValidator<CheckFilterDto> _filterValidator;

    public CheckService(
        ICheckRepository checkRepository,
        IRepository<Bank> bankRepository,
        ICharityRepository charityRepository,
        IUnitOfWork unitOfWork,
        ILogger<CheckService> logger,
        ICurrentUserService currentUser,
        IMapper mapper,
        IValidator<CreateCheckDto> createValidator,
        IValidator<UpdateCheckDto> updateValidator,
        IValidator<CheckFilterDto> filterValidator)
    {
        _checkRepository = checkRepository;
        _bankRepository = bankRepository;
        _charityRepository = charityRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
        _currentUser = currentUser;
        _mapper = mapper;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
        _filterValidator = filterValidator;
    }

    /// <inheritdoc />
    public async Task<CheckPagedResult<CheckListDto>> GetChecksAsync(CheckFilterDto filter)
    {
        filter ??= new CheckFilterDto();
        _filterValidator.ValidateAndThrow(filter);

        var query = ApplyFilters(ScopedQuery(), filter);

        var totalCount = await query.CountAsync();
        var page = Math.Max(filter.Page, 1);
        var pageSize = Math.Clamp(filter.PageSize, 1, 200);

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new CheckPagedResult<CheckListDto>
        {
            Items = _mapper.Map<List<CheckListDto>>(items),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    /// <inheritdoc />
    /// <remarks>
    /// Deliberately not filter-reactive: the band describes the caller's whole register
    /// scope, not the current search. The scope rides <see cref="ScopedQuery"/> plus
    /// <see cref="ApplyFilters"/> with a blank filter — with no user input every optional
    /// predicate falls away and only the scope rules remain (charity pin, or the head-office
    /// country pin). "This year" counts by <c>CheckDate</c> (تاريخ الشيك, mandatory on every
    /// cheque) rather than CreatedOn: the cheque's own date is what the register is ordered
    /// and filtered by, and a back-dated cheque registered today belongs to its cheque year.
    /// The blank filter needs no validation because nothing in it came from a caller.
    /// </remarks>
    public async Task<CheckStatisticsDto> GetStatisticsAsync()
    {
        _logger.LogInformation("Getting cheque register statistics");

        var now = DateTime.UtcNow;
        var monthStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var yearStart = new DateTime(now.Year, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        // One grouped round-trip for every scalar card; null when the scope matches no rows.
        // ApplyFilters' register ordering is harmless here — grouping discards it in translation.
        var totals = await ApplyFilters(ScopedQuery(), new CheckFilterDto())
            .GroupBy(_ => 1)
            .Select(g => new
            {
                Total = g.Count(),
                ThisYear = g.Count(c => c.CheckDate >= yearStart && c.CheckDate < yearStart.AddYears(1)),
                AddedThisMonth = g.Count(c => c.CreatedOn >= monthStart)
            })
            .FirstOrDefaultAsync();

        return new CheckStatisticsDto
        {
            Total = totals?.Total ?? 0,
            ThisYear = totals?.ThisYear ?? 0,
            AddedThisMonth = totals?.AddedThisMonth ?? 0
        };
    }

    /// <inheritdoc />
    public async Task<CheckDetailDto?> GetCheckByIdAsync(Guid id)
    {
        var check = await ScopedQuery().FirstOrDefaultAsync(c => c.Id == id);
        return check is null ? null : _mapper.Map<CheckDetailDto>(check);
    }

    /// <inheritdoc />
    public async Task<CheckDetailDto> CreateCheckAsync(CreateCheckDto dto)
    {
        _createValidator.ValidateAndThrow(dto);

        var bank = await _bankRepository.AsQueryable().FirstOrDefaultAsync(b => b.Id == dto.BankId);
        if (bank is null)
        {
            throw new InvalidOperationException($"Bank {dto.BankId} does not exist");
        }

        // Tenancy: a charity user owns the cheque whatever they send; an HQ user may
        // attribute it to an explicit charity (validated) or leave it unattributed.
        Guid? charityId = dto.CharityId;
        if (_currentUser.CharityId.HasValue)
        {
            if (dto.CharityId.HasValue && dto.CharityId != _currentUser.CharityId)
            {
                _logger.LogWarning(
                    "User {UserId} of charity {CallerCharityId} tried to issue a cheque for charity {RequestedCharityId}; owner forced to their own charity",
                    _currentUser.UserId, _currentUser.CharityId, dto.CharityId);
            }
            charityId = _currentUser.CharityId;
        }
        else if (charityId.HasValue)
        {
            var charityExists = await _charityRepository.AsQueryable().AnyAsync(ch => ch.Id == charityId.Value);
            if (!charityExists)
            {
                throw new InvalidOperationException($"Charity {charityId} does not exist");
            }
        }

        dto.Currency = dto.Currency.Trim().ToUpperInvariant();
        await EnsureUniqueAsync(dto.CheckNumber, dto.BankId!.Value, charityId, excludeId: null);

        var check = _mapper.Map<Check>(dto);
        check.FK_BankId = bank.Id;
        check.FK_CharityId = charityId;
        if (string.IsNullOrWhiteSpace(check.AmountInWords))
        {
            check.AmountInWords = ArabicAmountInWords.Convert(check.Amount, check.Currency);
        }

        await _checkRepository.InsertAsync(check);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation(
            "Cheque {CheckNumber} issued for charity {CharityId} by user {UserId}",
            check.CheckNumber, check.FK_CharityId, _currentUser.UserId);

        var created = await ScopedQuery().FirstAsync(c => c.Id == check.Id);
        return _mapper.Map<CheckDetailDto>(created);
    }

    /// <inheritdoc />
    public async Task<CheckDetailDto> UpdateCheckAsync(UpdateCheckDto dto)
    {
        _updateValidator.ValidateAndThrow(dto);

        var check = await ScopedQuery().FirstOrDefaultAsync(c => c.Id == dto.Id);
        if (check is null)
        {
            throw new KeyNotFoundException($"Check {dto.Id} not found");
        }

        var bank = await _bankRepository.AsQueryable().FirstOrDefaultAsync(b => b.Id == dto.BankId);
        if (bank is null)
        {
            throw new InvalidOperationException($"Bank {dto.BankId} does not exist");
        }

        dto.Currency = dto.Currency.Trim().ToUpperInvariant();
        await EnsureUniqueAsync(dto.CheckNumber, dto.BankId!.Value, check.FK_CharityId, excludeId: check.Id);

        // The charity assignment never moves on edit — the issuer stays the owner.
        var amountOrCurrencyChanged = check.Amount != dto.Amount
            || !string.Equals(check.Currency, dto.Currency, StringComparison.OrdinalIgnoreCase);
        var wordsUntouched = string.Equals(dto.AmountInWords?.Trim(), check.AmountInWords?.Trim(), StringComparison.Ordinal);

        check.CheckNumber = dto.CheckNumber;
        check.CheckDate = dto.CheckDate;
        check.Currency = dto.Currency;
        check.ChequeType = string.IsNullOrWhiteSpace(dto.ChequeType) ? "Individuals" : dto.ChequeType;
        check.FK_BankId = bank.Id;
        check.FK_ChequeBeneficiaryId = dto.ChequeBeneficiaryId;
        check.BeneficiaryType = dto.BeneficiaryType;
        check.BeneficiaryName = dto.BeneficiaryName;
        check.BeneficiaryAddress = dto.BeneficiaryAddress;
        check.BeneficiaryPhone = dto.BeneficiaryPhone;
        check.BeneficiaryEmail = dto.BeneficiaryEmail;
        check.BeneficiaryIdNumber = dto.BeneficiaryIdNumber;
        check.BankBranch = dto.BankBranch;
        check.AccountNumber = dto.AccountNumber;
        check.Amount = dto.Amount;
        check.IsDamaged = dto.IsDamaged;
        check.IsReturned = dto.IsReturned;
        check.IsDispensed = dto.IsDispensed;
        check.IsDone = dto.IsDone;
        check.Notes = dto.Comment;

        // تفقيط: regenerate when the field arrives blank, or when the amount/currency
        // changed and the client echoed the previous words back untouched.
        if (string.IsNullOrWhiteSpace(dto.AmountInWords) || (amountOrCurrencyChanged && wordsUntouched))
        {
            check.AmountInWords = ArabicAmountInWords.Convert(check.Amount, check.Currency);
        }
        else
        {
            check.AmountInWords = dto.AmountInWords;
        }

        _checkRepository.Update(check);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Cheque {CheckId} updated by user {UserId}", check.Id, _currentUser.UserId);

        var updated = await ScopedQuery().FirstAsync(c => c.Id == check.Id);
        return _mapper.Map<CheckDetailDto>(updated);
    }

    /// <inheritdoc />
    public async Task<CheckStatementDto> GetStatementAsync(CheckFilterDto filter)
    {
        filter ??= new CheckFilterDto();
        _filterValidator.ValidateAndThrow(filter);

        var query = ApplyFilters(ScopedQuery(), filter);

        var totalCount = await query.CountAsync();
        var page = Math.Max(filter.Page, 1);
        var pageSize = Math.Clamp(filter.PageSize, 1, 200);

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        // Totals over the whole filtered set, not just the page.
        var totals = await query
            .GroupBy(c => c.Currency)
            .Select(g => new { Currency = g.Key, Total = g.Sum(c => c.Amount) })
            .ToListAsync();

        return new CheckStatementDto
        {
            Items = _mapper.Map<List<CheckListDto>>(items),
            TotalCount = totalCount,
            TotalPages = pageSize > 0 ? (int)Math.Ceiling((decimal)totalCount / pageSize) : 0,
            TotalByCurrency = totals.ToDictionary(t => t.Currency, t => t.Total),
            GeneratedOn = DateTime.UtcNow
        };
    }

    /// <inheritdoc />
    public async Task<byte[]> ExportChecksToExcelAsync(CheckFilterDto filter)
    {
        filter ??= new CheckFilterDto();
        _filterValidator.ValidateAndThrow(filter);

        // Every filtered row — the register's §16.S.1 grid columns, no paging
        // (OrphanPaymentService.ExportPaymentGroupsToExcelAsync pattern).
        var checks = _mapper.Map<List<CheckListDto>>(
            await ApplyFilters(ScopedQuery(), filter).ToListAsync());

        using (var package = new OfficeOpenXml.ExcelPackage())
        {
            var worksheet = package.Workbook.Worksheets.Add("الشيكات العامة");

            worksheet.Cells[1, 1].Value = "الرقم";
            worksheet.Cells[1, 2].Value = "رقم الشيك";
            worksheet.Cells[1, 3].Value = "تاريخ الشيك";
            worksheet.Cells[1, 4].Value = "اسم المستفيد";
            worksheet.Cells[1, 5].Value = "المبلغ";
            worksheet.Cells[1, 6].Value = "العملة";
            worksheet.Cells[1, 7].Value = "البنك";

            using (var range = worksheet.Cells[1, 1, 1, 7])
            {
                range.Style.Font.Bold = true;
                range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
            }

            var row = 2;
            var serial = 1;
            foreach (var check in checks)
            {
                worksheet.Cells[row, 1].Value = serial++;
                worksheet.Cells[row, 2].Value = check.CheckNumber;
                worksheet.Cells[row, 3].Value = check.CheckDate.ToString("yyyy-MM-dd");
                worksheet.Cells[row, 4].Value = check.BeneficiaryName;
                worksheet.Cells[row, 5].Value = check.Amount;
                worksheet.Cells[row, 6].Value = check.Currency;
                worksheet.Cells[row, 7].Value = check.BankName ?? "";
                row++;
            }

            worksheet.Cells[1, 1, row - 1, 7].AutoFitColumns();

            return package.GetAsByteArray();
        }
    }

    /// <inheritdoc />
    public Task<AmountInWordsDto> GetAmountInWordsAsync(decimal amount, string currency)
    {
        if (amount < 0)
        {
            throw new ValidationException("Amount cannot be negative");
        }

        currency = (currency ?? string.Empty).Trim().ToUpperInvariant();
        if (currency.Length != 3)
        {
            throw new ValidationException("Currency must be a 3-letter ISO code");
        }

        return Task.FromResult(new AmountInWordsDto
        {
            Amount = amount,
            Currency = currency,
            Words = ArabicAmountInWords.Convert(amount, currency)
        });
    }

    #region Helpers

    /// <summary>
    /// The register query narrowed to what the caller may see: their own charity when
    /// bound to one; otherwise head-office scope — an explicit charity, their country
    /// when the token carries one, or every charity.
    /// </summary>
    private IQueryable<Check> ScopedQuery()
    {
        var query = _checkRepository.Query();

        if (_currentUser.CharityId.HasValue)
        {
            return query.Where(c => c.FK_CharityId == _currentUser.CharityId);
        }

        if (!_currentUser.IsHeadOffice)
        {
            _logger.LogWarning(
                "Unscopeable caller {UserId} reached the cheque register; scope narrowed to nothing",
                _currentUser.UserId);
            return query.Where(c => false);
        }

        return query;
    }

    /// <summary>
    /// Applies the shared §16.S.1/§16.S.3 filters (charity is resolved by the scope;
    /// a country-scoped HQ caller stays inside their country) and the register order.
    /// </summary>
    private IQueryable<Check> ApplyFilters(IQueryable<Check> query, CheckFilterDto filter)
    {
        if (_currentUser.CharityId.HasValue)
        {
            query = query.Where(c => c.FK_CharityId == _currentUser.CharityId);
        }
        else if (_currentUser.IsHeadOffice)
        {
            if (filter.CharityId.HasValue)
            {
                query = query.Where(c => c.FK_CharityId == filter.CharityId.Value);
            }
            else if (_currentUser.CountryId.HasValue)
            {
                query = query.Where(c => c.Charity != null && c.Charity.CountryId == _currentUser.CountryId);
            }
        }

        if (filter.BankId.HasValue)
        {
            query = query.Where(c => c.FK_BankId == filter.BankId.Value);
        }

        if (filter.DateFrom.HasValue)
        {
            query = query.Where(c => c.CheckDate >= filter.DateFrom.Value);
        }

        if (filter.DateTo.HasValue)
        {
            // Inclusive end date: through the last moment of the day.
            var endExclusive = filter.DateTo.Value.Date.AddDays(1);
            query = query.Where(c => c.CheckDate < endExclusive);
        }

        if (!string.IsNullOrWhiteSpace(filter.ChequeType))
        {
            query = query.Where(c => c.ChequeType == filter.ChequeType);
        }

        if (!string.IsNullOrWhiteSpace(filter.SearchText))
        {
            var term = filter.SearchText.Trim();
            query = query.Where(c => c.CheckNumber.Contains(term) || c.BeneficiaryName.Contains(term));
        }

        return query.OrderByDescending(c => c.CheckDate).ThenByDescending(c => c.CreatedOn);
    }

    /// <summary>
    /// A cheque number is unique per bank per charity among live rows (§16.S.2);
    /// the filtered unique index is the last line of defence.
    /// </summary>
    private async Task EnsureUniqueAsync(string checkNumber, int bankId, Guid? charityId, Guid? excludeId)
    {
        var number = checkNumber.Trim();
        var clash = await _checkRepository.Query().AnyAsync(c =>
            c.CheckNumber == number &&
            c.FK_BankId == bankId &&
            c.FK_CharityId == charityId &&
            (excludeId == null || c.Id != excludeId));

        if (clash)
        {
            throw new InvalidOperationException($"Cheque number {number} already exists for this bank");
        }
    }

    #endregion
}
