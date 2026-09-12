namespace IIROSA.Application.DTOs.HqTransfers;

// Naming note: DTO properties use clean names (CountryId, DepartmentId, …) so the camelCase wire
// contract (`countryId`, not `fK_CountryId`) matches what every client naturally sends — the
// Newtonsoft default emitter mangles `FK_`-prefixed properties. The entity keeps its legacy
// `FK_`-prefixed columns; HqTransferProfile bridges the two with explicit ForMember maps.

/// <summary>
/// HqTransfer list item for grid display (UC-TRF-01 — the §22.S.1 grid's 11 data columns)
/// </summary>
public class HqTransferListDto
{
    public Guid Id { get; set; }

    // Destination / requesting department names (السنة… columns resolve from navigations)
    public string? CountryName { get; set; }
    public string? DepartmentName { get; set; }

    // Operation identification
    public string OperationNumber { get; set; } = string.Empty;
    public string FinYear { get; set; } = string.Empty;
    public int PaymentNumber { get; set; }

    // Period
    public DateTime DateFrom { get; set; }
    public DateTime DateTo { get; set; }

    // Financials
    public decimal AmountOfPayment { get; set; }
    public string? Statement { get; set; }
    public int BeneficiariesNumber { get; set; }

    // Transaction
    public string TransactionNumber { get; set; } = string.Empty;
    public DateTime TransactionDate { get; set; }
}

/// <summary>
/// HqTransfer filter DTO (UC-TRF-01). §22.S.1 defines no search box — the only server-side
/// narrowing beyond paging is the caller's country claim, pinned in the service.
/// </summary>
public class HqTransferFilterDto
{
    public int? CountryId { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

/// <summary>
/// Paged result wrapper for HQ transfers (OfficeProject shape)
/// </summary>
public class HqTransferPagedResult<T>
{
    public List<T> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    // PageSize is clamped to ≥1 in the service, but the guard keeps the property safe for
    // any other construction path (Checks.cs:142 pattern — divide-by-zero is a 500, not a 0)
    public int TotalPages => PageSize > 0 ? (int)Math.Ceiling((decimal)TotalCount / PageSize) : 0;
}

/// <summary>
/// HqTransfer detail — the full §22.S.2 record with resolved lookup names (UC-TRF-02/03/04)
/// </summary>
public class HqTransferDetailDto
{
    public Guid Id { get; set; }

    // Destination / requesting department
    public int CountryId { get; set; }
    public string? CountryName { get; set; }
    public int DepartmentId { get; set; }
    public string? DepartmentName { get; set; }

    // Operation identification
    public string OperationNumber { get; set; } = string.Empty;
    public string FinYear { get; set; } = string.Empty;
    public int PaymentNumber { get; set; }

    // Period
    public DateTime DateFrom { get; set; }
    public DateTime DateTo { get; set; }

    // Financials
    public decimal AmountOfPayment { get; set; }
    public string? Statement { get; set; }
    public int BeneficiariesNumber { get; set; }

    // Transaction
    public string TransactionNumber { get; set; } = string.Empty;
    public DateTime TransactionDate { get; set; }

    // Audit (read-only echo)
    public DateTime CreatedOn { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedOn { get; set; }
    public string? UpdatedBy { get; set; }
}

/// <summary>
/// Create HqTransfer DTO (UC-TRF-02) — §22.S.2's 12 fields, 11 mandatory. Clean wire names
/// only (the entity's FK_* columns are bridged in HqTransferProfile).
/// </summary>
public class CreateHqTransferDto
{
    // Mandatory lookups
    public int CountryId { get; set; }
    public int DepartmentId { get; set; }

    // Operation identification
    public string OperationNumber { get; set; } = string.Empty;
    public string FinYear { get; set; } = string.Empty;
    public int PaymentNumber { get; set; }

    // Period
    public DateTime DateFrom { get; set; }
    public DateTime DateTo { get; set; }

    // Financials
    public decimal AmountOfPayment { get; set; }
    public string? Statement { get; set; }
    public int BeneficiariesNumber { get; set; }

    // Transaction
    public string TransactionNumber { get; set; } = string.Empty;
    public DateTime TransactionDate { get; set; }
}

/// <summary>
/// Per-country transfer ceiling (UC-TRF-06) — MaxTransferAmount NULL means no limit
/// configured (unlimited); that is a legal state, not an error.
/// </summary>
public class CountryMaxTransferAmountDto
{
    public int CountryId { get; set; }
    public string CountryName { get; set; } = string.Empty;
    public decimal? MaxTransferAmount { get; set; }
}

/// <summary>
/// Set a country's transfer ceiling (UC-TRF-07). MaxTransferAmount NULL is legal — it means
/// unlimited; when present it must be positive.
/// </summary>
public class UpdateCountryMaxTransferDto
{
    public int CountryId { get; set; }
    public decimal? MaxTransferAmount { get; set; }
}

/// <summary>
/// One allocation line of a transfer (UC-TRF-08, §22.S.3) — the grid row read shape
/// </summary>
public class HqTransferDetailLineDto
{
    public Guid Id { get; set; }

    // رقم الحوالة — the line's own transfer reference
    public string TransferNumber { get; set; } = string.Empty;

    // مبلغ الحوالة — this line's share of the header sum
    public decimal Amount { get; set; }

    public DateTime? EstimatedTransferDate { get; set; }

    // مصير الحوالة — false = لم ينفذ, true = تم التنفيذ, null = unset
    public bool? IsExecuted { get; set; }

    public DateTime? ExecutionDate { get; set; }
    public DateTime? ArrivalDate { get; set; }
    public decimal? ArrivalAmount { get; set; }
}

/// <summary>
/// The §22.S.3 screen read (UC-TRF-08) — header summary + the grid's lines
/// </summary>
public class HqTransferDetailsResultDto
{
    public Guid TransferId { get; set; }
    public string OperationNumber { get; set; } = string.Empty;
    public decimal AmountOfPayment { get; set; }
    public string? CountryName { get; set; }
    public List<HqTransferDetailLineDto> Lines { get; set; } = new();
}

/// <summary>
/// Save one allocation line (UC-TRF-08) — Id present ⇒ update that line, absent ⇒ add.
/// Execution/arrival fields are legal only when IsExecuted is true (validator).
/// </summary>
public class SaveHqTransferDetailLineDto
{
    public Guid? Id { get; set; }
    public string TransferNumber { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime? EstimatedTransferDate { get; set; }
    public bool? IsExecuted { get; set; }
    public DateTime? ExecutionDate { get; set; }
    public DateTime? ArrivalDate { get; set; }
    public decimal? ArrivalAmount { get; set; }
}

/// <summary>
/// Update HqTransfer DTO (UC-TRF-04) — id in the body (board contract: PUT /api/HqTransfers)
/// plus the same 12 §22.S.2 fields with identical rules to the create DTO.
/// </summary>
public class UpdateHqTransferDto
{
    public Guid Id { get; set; }

    // Mandatory lookups
    public int CountryId { get; set; }
    public int DepartmentId { get; set; }

    // Operation identification
    public string OperationNumber { get; set; } = string.Empty;
    public string FinYear { get; set; } = string.Empty;
    public int PaymentNumber { get; set; }

    // Period
    public DateTime DateFrom { get; set; }
    public DateTime DateTo { get; set; }

    // Financials
    public decimal AmountOfPayment { get; set; }
    public string? Statement { get; set; }
    public int BeneficiariesNumber { get; set; }

    // Transaction
    public string TransactionNumber { get; set; } = string.Empty;
    public DateTime TransactionDate { get; set; }
}

/// <summary>
/// Register statistics band shown above the §22.S.1 grid (UC-TRF-01). Counts follow the
/// caller's country claim — the list read's scope — so the band and the grid beneath it
/// can never disagree about what is counted.
/// </summary>
public class HqTransferStatisticsDto
{
    public int Total { get; set; }

    /// <summary>
    /// Σ <c>AmountOfPayment</c> over the scoped transfers — the header's payment amount,
    /// not the detail lines' partial allocations.
    /// </summary>
    public decimal TotalAmount { get; set; }

    /// <summary>Transfers filed since the first day of the current (UTC) month.</summary>
    public int AddedThisMonth { get; set; }
}
