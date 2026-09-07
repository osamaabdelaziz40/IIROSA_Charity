using AutoMapper;
using FluentValidation;
using IIROSA.Application.DTOs.HqTransfers;
using IIROSA.Application.Exceptions;
using IIROSA.Application.Interfaces;
using IIROSA.Domain.Entities;
using IIROSA.Domain.Entities.Lookups;
using IIROSA.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace IIROSA.Application.Services;

/// <summary>
/// HqTransfer Service Implementation (UC-TRF-01…08)
///
/// Head-office module: only Admin and SuperAdmin reach it. The caller's country claim, when
/// present, scopes every read and write — a pinned caller cannot enumerate or file another
/// country's transfers. Only IUnitOfWork saves.
/// </summary>
public class HqTransferService : IHqTransferService
{
    private readonly IHqTransferRepository _transferRepository;
    private readonly IHqTransferDetailRepository _transferDetailRepository;
    private readonly ICountryRepository _countryRepository;
    private readonly IDepartmentRepository _departmentRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<HqTransferService> _logger;
    private readonly ICurrentUserService _currentUser;
    private readonly IValidator<CreateHqTransferDto> _createValidator;
    private readonly IValidator<UpdateHqTransferDto> _updateValidator;
    private readonly IValidator<UpdateCountryMaxTransferDto> _maxTransferValidator;
    private readonly IValidator<SaveHqTransferDetailLineDto> _detailLineValidator;

    public HqTransferService(
        IHqTransferRepository transferRepository,
        IHqTransferDetailRepository transferDetailRepository,
        ICountryRepository countryRepository,
        IDepartmentRepository departmentRepository,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<HqTransferService> logger,
        ICurrentUserService currentUser,
        IValidator<CreateHqTransferDto> createValidator,
        IValidator<UpdateHqTransferDto> updateValidator,
        IValidator<UpdateCountryMaxTransferDto> maxTransferValidator,
        IValidator<SaveHqTransferDetailLineDto> detailLineValidator)
    {
        _transferRepository = transferRepository;
        _transferDetailRepository = transferDetailRepository;
        _countryRepository = countryRepository;
        _departmentRepository = departmentRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
        _currentUser = currentUser;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
        _maxTransferValidator = maxTransferValidator;
        _detailLineValidator = detailLineValidator;
    }

    // ========== Reads (UC-TRF-01) ==========

    /// <summary>
    /// Get transfers with filtering and pagination (UC-TRF-01: list), newest transaction first
    /// </summary>
    public async Task<HqTransferPagedResult<HqTransferListDto>> GetHqTransfersAsync(HqTransferFilterDto filter)
    {
        try
        {
            filter ??= new HqTransferFilterDto();

            // Page/PageSize come straight off the query string — clamp before they reach
            // Skip/Take and TotalPages (page 0 → negative Skip SQL error; pageSize 0 →
            // DivideByZeroException; unbounded pageSize pages the whole table)
            filter.Page = Math.Max(1, filter.Page);
            filter.PageSize = Math.Clamp(filter.PageSize, 1, 200);

            ApplyCallerScope(filter);

            _logger.LogInformation("Retrieving HQ transfers with filter: {@Filter}", filter);

            System.Linq.Expressions.Expression<Func<HqTransfer, bool>>? filterExpression =
                filter.CountryId.HasValue
                    ? t => t.FK_CountryId == filter.CountryId.Value
                    : null;

            var (items, totalCount) = await _transferRepository.GetTransfersPagedAsync(
                filterExpression,
                q => q.OrderByDescending(t => t.TransactionDate).ThenByDescending(t => t.CreatedOn),
                filter.Page,
                filter.PageSize);

            var transferDtos = _mapper.Map<List<HqTransferListDto>>(items);

            return new HqTransferPagedResult<HqTransferListDto>
            {
                Items = transferDtos,
                TotalCount = totalCount,
                Page = filter.Page,
                PageSize = filter.PageSize
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving HQ transfers with filter: {@Filter}", filter);
            throw;
        }
    }

    // ========== Updates (UC-TRF-04) ==========

    /// <summary>
    /// Update a transfer (UC-TRF-04). Validation first, then the same 404-shaped guards as the
    /// read (absent or out-of-scope → NotFoundException, nothing written), FK re-validation, and
    /// a map onto the tracked entity — the audit interceptor owns UpdatedOn/UpdatedBy.
    /// </summary>
    public async Task<HqTransferDetailDto> UpdateHqTransferAsync(UpdateHqTransferDto dto)
    {
        await _updateValidator.ValidateAndThrowAsync(dto);

        try
        {
            _logger.LogInformation("Updating HQ transfer {TransferId}: {@Transfer}", dto.Id, dto);

            var transfer = await _transferRepository.GetByIdWithLinesAsync(dto.Id)
                ?? throw new NotFoundException(typeof(HqTransfer), dto.Id);

            // Same scope rule as the read: a pinned caller editing another country's record
            // reads as 404 — existence is never confirmed across the boundary
            if (!IsWithinCallerScope(transfer))
            {
                _logger.LogWarning(
                    "Caller pinned to country {CallerCountry} tried to update transfer {TransferId} belonging to country {RecordCountry}; refusing",
                    _currentUser.CountryId, dto.Id, transfer.FK_CountryId);
                throw new NotFoundException(typeof(HqTransfer), dto.Id);
            }

            // A pinned caller cannot move their record into another country (scope escape).
            // Ruled 2026-08-24: refuse the mismatch (same as the create path) instead of
            // silently re-pinning — the record must keep the country the actor chose, or
            // the save is refused. Runs BEFORE the FK/ceiling rules.
            if (_currentUser.CountryId.HasValue && dto.CountryId != _currentUser.CountryId.Value)
            {
                _logger.LogWarning(
                    "Caller pinned to country {CallerCountry} tried to move transfer {TransferId} to country {RequestedCountry}; refusing",
                    _currentUser.CountryId.Value, dto.Id, dto.CountryId);
                throw new UnauthorizedAccessException(
                    $"Transfers can only be filed under the caller's own country (country {dto.CountryId} is outside the caller's scope)");
            }

            // Σ-lines invariant (UC-TRF-08's rule, header side): the payment amount may not
            // be lowered below the total already allocated to detail lines — otherwise the
            // record goes over-allocated and every existing line becomes unfixable
            var linesTotal = transfer.Details.Sum(d => d.Amount);
            if (linesTotal > dto.AmountOfPayment)
            {
                throw new InvalidOperationException(
                    $"The transfer's payment amount ({dto.AmountOfPayment}) is below the total of its detail lines ({linesTotal})");
            }

            // DB-dependent rules — create-parity for whatever CHANGED; unchanged values are
            // grandfathered (ruled 2026-08-24: a lowered ceiling or a deactivated lookup
            // must not lock an existing record out of editing)
            var countryUnchanged = dto.CountryId == transfer.FK_CountryId;
            var departmentUnchanged = dto.DepartmentId == transfer.FK_DepartmentId;

            var country = transfer.Country;
            if (!countryUnchanged)
            {
                country = await _countryRepository.GetByIdAsync(dto.CountryId);
                if (country == null || !country.IsActive)
                {
                    throw new InvalidOperationException($"Country with ID {dto.CountryId} does not exist or is not active");
                }
            }

            if (!departmentUnchanged)
            {
                var department = await _departmentRepository.GetByIdAsync(dto.DepartmentId);
                if (department == null || !department.IsActive)
                {
                    throw new InvalidOperationException($"Department with ID {dto.DepartmentId} does not exist or is not active");
                }
            }

            // UC-TRF-06 ceiling (17-6): refused with a message naming the ceiling; nothing
            // written — re-checked only when the amount changed (grandfathered otherwise)
            if (dto.AmountOfPayment != transfer.AmountOfPayment)
            {
                EnforceTransferCeiling(country!, dto.AmountOfPayment);
            }

            _mapper.Map(dto, transfer);

            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("HQ transfer {TransferId} updated successfully", dto.Id);

            return (await GetTransferByIdInternalAsync(transfer.Id))!;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating HQ transfer {TransferId}", dto.Id);
            throw;
        }
    }

    // ========== Single reads (UC-TRF-03) ==========

    /// <summary>
    /// Get one transfer with its lookup names resolved (UC-TRF-03: عرض الحوالة). A record that
    /// is absent, soft-deleted, or outside the caller's country scope reads identically as 404 —
    /// existence is never confirmed across the country boundary.
    /// </summary>
    public async Task<HqTransferDetailDto> GetHqTransferByIdAsync(Guid id)
    {
        try
        {
            _logger.LogInformation("Retrieving HQ transfer {TransferId}", id);

            var transfer = await _transferRepository.GetByIdWithLookupsAsync(id)
                ?? throw new NotFoundException(typeof(HqTransfer), id);

            if (!IsWithinCallerScope(transfer))
            {
                _logger.LogWarning(
                    "Caller pinned to country {CallerCountry} requested transfer {TransferId} belonging to country {RecordCountry}; answering 404",
                    _currentUser.CountryId, id, transfer.FK_CountryId);
                throw new NotFoundException(typeof(HqTransfer), id);
            }

            return _mapper.Map<HqTransferDetailDto>(transfer);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving HQ transfer {TransferId}", id);
            throw;
        }
    }

    // ========== Writes (UC-TRF-02) ==========

    /// <summary>
    /// Create a new transfer (UC-TRF-02). FluentValidation first, then the DB-dependent lookup
    /// checks; the ceiling rule against Country.MaxTransferAmount is 17-6's to retrofit.
    /// </summary>
    public async Task<HqTransferDetailDto> AddNewTransferAsync(CreateHqTransferDto dto)
    {
        await _createValidator.ValidateAndThrowAsync(dto);

        try
        {
            _logger.LogInformation("Creating new HQ transfer: {@Transfer}", dto);

            // The record's country belongs to the caller when their token pins one (pin-never-
            // widen, same as the read paths). Ruled 2026-08-24: refuse the mismatch instead of
            // silently re-pinning — the record must be stored under the country the actor
            // chose, or not at all. Runs BEFORE the FK/ceiling rules.
            if (_currentUser.CountryId.HasValue && dto.CountryId != _currentUser.CountryId.Value)
            {
                _logger.LogWarning(
                    "Caller pinned to country {CallerCountry} tried to file a transfer under country {RequestedCountry}; refusing",
                    _currentUser.CountryId.Value, dto.CountryId);
                throw new UnauthorizedAccessException(
                    $"Transfers can only be filed under the caller's own country (country {dto.CountryId} is outside the caller's scope)");
            }

            // DB-dependent rules (epic-14 precedent): the lookups must exist and be active
            var country = await _countryRepository.GetByIdAsync(dto.CountryId);
            if (country == null || !country.IsActive)
            {
                throw new InvalidOperationException($"Country with ID {dto.CountryId} does not exist or is not active");
            }

            var department = await _departmentRepository.GetByIdAsync(dto.DepartmentId);
            if (department == null || !department.IsActive)
            {
                throw new InvalidOperationException($"Department with ID {dto.DepartmentId} does not exist or is not active");
            }

            // UC-TRF-06 ceiling (17-6): refused with a message naming the ceiling; nothing written
            EnforceTransferCeiling(country, dto.AmountOfPayment);

            var transfer = _mapper.Map<HqTransfer>(dto);

            await _transferRepository.AddAsync(transfer);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("HQ transfer created successfully with ID: {TransferId}", transfer.Id);

            return (await GetTransferByIdInternalAsync(transfer.Id))!;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while creating HQ transfer: {@Transfer}", dto);
            throw;
        }
    }

    // ========== Ceiling (UC-TRF-06) ==========

    /// <summary>
    /// The per-country transfer ceiling (UC-TRF-06). NULL means no limit configured —
    /// unlimited is a legal state, not an error. Unknown country reads as 404.
    /// </summary>
    public async Task<CountryMaxTransferAmountDto> GetMaxTransferAmountAsync(int countryId)
    {
        try
        {
            _logger.LogInformation("Retrieving transfer ceiling for country {CountryId}", countryId);

            var country = await _countryRepository.GetByIdAsync(countryId)
                ?? throw new NotFoundException(typeof(Country), countryId);

            // Same pin-never-widen rule as every transfer read: a pinned caller cannot read
            // another country's ceiling — existence is never confirmed across the boundary
            if (_currentUser.CountryId.HasValue && country.Id != _currentUser.CountryId.Value)
            {
                _logger.LogWarning(
                    "Caller pinned to country {CallerCountry} requested the transfer ceiling of country {RequestedCountry}; answering 404",
                    _currentUser.CountryId.Value, countryId);
                throw new NotFoundException(typeof(Country), countryId);
            }

            return new CountryMaxTransferAmountDto
            {
                CountryId = country.Id,
                CountryName = country.NameAr ?? country.NameEn ?? string.Empty,
                MaxTransferAmount = country.MaxTransferAmount
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving the transfer ceiling for country {CountryId}", countryId);
            throw;
        }
    }

    /// <summary>
    /// Set a country's transfer ceiling (UC-TRF-07). Touches ONLY the MaxTransferAmount column —
    /// NameAr/NameEn/IsActive and the audit history stay untouched (the interceptor owns
    /// UpdatedOn/UpdatedBy). NULL clears the ceiling (unlimited).
    /// </summary>
    public async Task<CountryMaxTransferAmountDto> UpdateCountryMaxTransferAmountAsync(UpdateCountryMaxTransferDto dto)
    {
        await _maxTransferValidator.ValidateAndThrowAsync(dto);

        try
        {
            _logger.LogInformation(
                "Setting transfer ceiling for country {CountryId} to {MaxTransferAmount}",
                dto.CountryId, dto.MaxTransferAmount);

            var country = await _countryRepository.GetByIdAsync(dto.CountryId)
                ?? throw new NotFoundException(typeof(Country), dto.CountryId);

            // Same pin-never-widen rule as the read half: a pinned caller cannot overwrite
            // another country's ceiling — 404-shaped refusal, nothing written
            if (_currentUser.CountryId.HasValue && country.Id != _currentUser.CountryId.Value)
            {
                _logger.LogWarning(
                    "Caller pinned to country {CallerCountry} tried to set the transfer ceiling of country {RequestedCountry}; refusing",
                    _currentUser.CountryId.Value, dto.CountryId);
                throw new NotFoundException(typeof(Country), dto.CountryId);
            }

            country.MaxTransferAmount = dto.MaxTransferAmount;

            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Transfer ceiling for country {CountryId} saved", dto.CountryId);

            return new CountryMaxTransferAmountDto
            {
                CountryId = country.Id,
                CountryName = country.NameAr ?? country.NameEn ?? string.Empty,
                MaxTransferAmount = country.MaxTransferAmount
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while setting the transfer ceiling for country {CountryId}", dto.CountryId);
            throw;
        }
    }

    // ========== Detail lines (UC-TRF-08) ==========

    /// <summary>
    /// The §22.S.3 screen read (UC-TRF-08): header summary + allocation lines ordered by
    /// رقم الحوالة. Same 404-shaped scope guard as every other transfer read.
    /// </summary>
    public async Task<HqTransferDetailsResultDto> GetTransferDetailsAsync(Guid transferId)
    {
        try
        {
            _logger.LogInformation("Retrieving detail lines for HQ transfer {TransferId}", transferId);

            var transfer = await _transferRepository.GetByIdWithLinesAsync(transferId)
                ?? throw new NotFoundException(typeof(HqTransfer), transferId);

            if (!IsWithinCallerScope(transfer))
            {
                _logger.LogWarning(
                    "Caller pinned to country {CallerCountry} requested the details of transfer {TransferId} belonging to country {RecordCountry}; answering 404",
                    _currentUser.CountryId, transferId, transfer.FK_CountryId);
                throw new NotFoundException(typeof(HqTransfer), transferId);
            }

            return new HqTransferDetailsResultDto
            {
                TransferId = transfer.Id,
                OperationNumber = transfer.OperationNumber,
                AmountOfPayment = transfer.AmountOfPayment,
                CountryName = transfer.Country != null
                    ? transfer.Country.NameAr ?? transfer.Country.NameEn
                    : null,
                Lines = _mapper.Map<List<HqTransferDetailLineDto>>(
                    transfer.Details.OrderBy(d => d.CreatedOn).ThenBy(d => d.TransferNumber).ToList())
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving detail lines for HQ transfer {TransferId}", transferId);
            throw;
        }
    }

    /// <summary>
    /// Save one allocation line (UC-TRF-08): per-row «حفظ». Upsert semantics — Id present
    /// updates that line (404 when it belongs to another transfer), absent adds it. The sum
    /// rule refuses a save that would push Σ line Amount past the header's AmountOfPayment;
    /// the validator owns the «Failed Operation» state gating. UoW-only save.
    /// </summary>
    public async Task<HqTransferDetailLineDto> SaveTransferDetailLineAsync(Guid transferId, SaveHqTransferDetailLineDto dto)
    {
        await _detailLineValidator.ValidateAndThrowAsync(dto);

        try
        {
            _logger.LogInformation(
                "Saving detail line {LineId} of HQ transfer {TransferId}: {@Line}",
                dto.Id, transferId, dto);

            var transfer = await _transferRepository.GetByIdWithLinesAsync(transferId)
                ?? throw new NotFoundException(typeof(HqTransfer), transferId);

            if (!IsWithinCallerScope(transfer))
            {
                _logger.LogWarning(
                    "Caller pinned to country {CallerCountry} tried to save a detail line on transfer {TransferId} belonging to country {RecordCountry}; refusing",
                    _currentUser.CountryId, transferId, transfer.FK_CountryId);
                throw new NotFoundException(typeof(HqTransfer), transferId);
            }

            // Ownership first (404 before any other answer): Id present ⇒ update that line,
            // absent ⇒ add. A line id that belongs to another transfer reads as 404 here,
            // never as a sum-rule 400 (17-8 review finding)
            HqTransferDetail? existingLine = null;
            if (dto.Id.HasValue)
            {
                existingLine = transfer.Details.FirstOrDefault(d => d.Id == dto.Id.Value)
                    ?? throw new NotFoundException(typeof(HqTransferDetail), dto.Id.Value);

                // Ruled 2026-08-24: execution/arrival data is a financial record — a line
                // that carries any of it cannot be un-executed (the data would otherwise be
                // erased in one save by the full-row map)
                if (dto.IsExecuted != true
                    && (existingLine.ExecutionDate.HasValue
                        || existingLine.ArrivalDate.HasValue
                        || existingLine.ArrivalAmount.HasValue))
                {
                    throw new InvalidOperationException(
                        "A line with recorded execution or arrival data cannot be marked as not executed");
                }
            }

            // Sum rule — Σ line Amount (others unchanged + the incoming value) may not exceed
            // the header's payment amount (the recorded interpretation of "allocation")
            var othersTotal = transfer.Details
                .Where(d => !dto.Id.HasValue || d.Id != dto.Id.Value)
                .Sum(d => d.Amount);
            if (othersTotal + dto.Amount > transfer.AmountOfPayment)
            {
                throw new InvalidOperationException(
                    $"The sum of detail line amounts ({othersTotal + dto.Amount}) exceeds the transfer's payment amount ({transfer.AmountOfPayment})");
            }

            HqTransferDetail line;
            if (existingLine != null)
            {
                line = existingLine;
                _mapper.Map(dto, line);
            }
            else
            {
                line = _mapper.Map<HqTransferDetail>(dto);
                line.FK_HqTransferId = transfer.Id;
                await _transferDetailRepository.AddAsync(line);
            }

            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation(
                "Detail line {LineId} of HQ transfer {TransferId} saved", line.Id, transferId);

            return _mapper.Map<HqTransferDetailLineDto>(line);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while saving a detail line of HQ transfer {TransferId}", transferId);
            throw;
        }
    }

    #region Private Helper Methods

    /// <summary>
    /// UC-TRF-02/06 business rule: when the destination country has a configured ceiling, the
    /// payment amount may not exceed it. NULL ceiling = unlimited (never treated as zero).
    /// InvalidOperationException → 400 { message } in the controller; nothing is written.
    /// </summary>
    private static void EnforceTransferCeiling(Country country, decimal amountOfPayment)
    {
        if (country.MaxTransferAmount.HasValue && amountOfPayment > country.MaxTransferAmount.Value)
        {
            throw new InvalidOperationException(
                $"Amount of payment {amountOfPayment} exceeds the maximum transfer amount ({country.MaxTransferAmount.Value}) configured for this country");
        }
    }

    /// <summary>
    /// Detail read with navigations; returns null for absent rows (the caller decides 404
    /// semantics — used by the create path's echo and, from 17-3, the detail endpoint)
    /// </summary>
    private async Task<HqTransferDetailDto?> GetTransferByIdInternalAsync(Guid id)
    {
        var transfer = await _transferRepository.GetByIdWithLookupsAsync(id);
        return transfer == null ? null : _mapper.Map<HqTransferDetailDto>(transfer);
    }


    /// <summary>
    /// Pins the filter's country to the caller's country claim. The module is head-office only;
    /// a caller whose token carries no country claim reads every country.
    /// </summary>
    private void ApplyCallerScope(HqTransferFilterDto filter)
    {
        var callerCountry = _currentUser.CountryId;
        if (!callerCountry.HasValue)
        {
            return;
        }

        if (filter.CountryId.HasValue && filter.CountryId.Value != callerCountry.Value)
        {
            _logger.LogWarning(
                "Caller pinned to country {CallerCountry} requested country {RequestedCountry}; pinning to {CallerCountry}",
                callerCountry.Value, filter.CountryId.Value, callerCountry.Value);
        }

        filter.CountryId = callerCountry;
    }

    /// <summary>
    /// Whether the record is visible to the caller: a caller without a country claim sees
    /// everything; a pinned caller sees only their own country's rows.
    /// </summary>
    private bool IsWithinCallerScope(HqTransfer transfer)
    {
        var callerCountry = _currentUser.CountryId;
        return !callerCountry.HasValue || transfer.FK_CountryId == callerCountry.Value;
    }

    #endregion
}
