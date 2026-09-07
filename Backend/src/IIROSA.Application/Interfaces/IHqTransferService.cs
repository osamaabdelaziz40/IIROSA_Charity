using IIROSA.Application.DTOs.HqTransfers;

namespace IIROSA.Application.Interfaces;

/// <summary>
/// HqTransfer Service Interface (UC-TRF-01…08)
/// </summary>
public interface IHqTransferService
{
    /// <summary>
    /// Get transfers with paging, scoped to the caller's country claim (UC-TRF-01: list)
    /// </summary>
    Task<HqTransferPagedResult<HqTransferListDto>> GetHqTransfersAsync(HqTransferFilterDto filter);

    /// <summary>
    /// Export the §22.S.1 register to Excel — the grid's serial + 11 data columns, every
    /// row in the caller's country scope (the list read's rules; paging ignored)
    /// </summary>
    Task<byte[]> ExportHqTransfersToExcelAsync(HqTransferFilterDto filter);

    /// <summary>
    /// Get one transfer with resolved lookup names (UC-TRF-03: view) — NotFoundException
    /// covers absent, soft-deleted, and out-of-scope records alike
    /// </summary>
    Task<HqTransferDetailDto> GetHqTransferByIdAsync(Guid id);

    /// <summary>
    /// Create a new transfer (UC-TRF-02) — validation, lookup checks, UoW-only save
    /// </summary>
    Task<HqTransferDetailDto> AddNewTransferAsync(CreateHqTransferDto dto);

    /// <summary>
    /// Update a transfer (UC-TRF-04) — id in the body; 404-shaped guards for absent or
    /// out-of-scope records; UoW-only save; audit interceptor owns UpdatedOn/UpdatedBy
    /// </summary>
    Task<HqTransferDetailDto> UpdateHqTransferAsync(UpdateHqTransferDto dto);

    /// <summary>
    /// The per-country transfer ceiling (UC-TRF-06) — NotFoundException for an unknown
    /// country; NULL MaxTransferAmount means unlimited
    /// </summary>
    Task<CountryMaxTransferAmountDto> GetMaxTransferAmountAsync(int countryId);

    /// <summary>
    /// Set a country's transfer ceiling (UC-TRF-07) — NULL clears it (unlimited); touches
    /// only the MaxTransferAmount column; UoW-only save
    /// </summary>
    Task<CountryMaxTransferAmountDto> UpdateCountryMaxTransferAmountAsync(UpdateCountryMaxTransferDto dto);

    /// <summary>
    /// The §22.S.3 screen read (UC-TRF-08) — header summary + allocation lines; the same
    /// 404-shaped scope guard as every transfer read
    /// </summary>
    Task<HqTransferDetailsResultDto> GetTransferDetailsAsync(Guid transferId);

    /// <summary>
    /// Save one allocation line (UC-TRF-08) — upsert (Id ⇒ update, null ⇒ add); the
    /// «Failed Operation» state gating in the validator; the sum rule (Σ line Amount ≤
    /// header AmountOfPayment) in the service; UoW-only save
    /// </summary>
    Task<HqTransferDetailLineDto> SaveTransferDetailLineAsync(Guid transferId, SaveHqTransferDetailLineDto dto);
}
