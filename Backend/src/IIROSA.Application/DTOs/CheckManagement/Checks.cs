namespace IIROSA.Application.DTOs.CheckManagement;

/// <summary>
/// Cheque list row — the columns of the register grid (§16.S.1) and the statement grid (§16.S.3).
/// </summary>
public class CheckListDto
{
    public Guid Id { get; set; }
    public string CheckNumber { get; set; } = string.Empty;
    public DateTime CheckDate { get; set; }
    public string BeneficiaryName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string ChequeType { get; set; } = "Individuals";
    public string? BankName { get; set; }
    public Guid? CharityId { get; set; }
    public bool IsDamaged { get; set; }
    public bool IsReturned { get; set; }
    public bool IsDispensed { get; set; }
    public bool IsDone { get; set; }
    public string? Comment { get; set; }
}

/// <summary>
/// Full cheque record for the view/edit screens (§16.S.2 fields plus the stored snapshot).
/// </summary>
public class CheckDetailDto
{
    public Guid Id { get; set; }

    // Check information
    public string CheckNumber { get; set; } = string.Empty;
    public DateTime CheckDate { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string ChequeType { get; set; } = "Individuals";

    // Beneficiary snapshot
    public int? ChequeBeneficiaryId { get; set; }
    public string? BeneficiaryType { get; set; }
    public string BeneficiaryName { get; set; } = string.Empty;
    public string? BeneficiaryAddress { get; set; }
    public string? BeneficiaryPhone { get; set; }
    public string? BeneficiaryEmail { get; set; }
    public string? BeneficiaryIdNumber { get; set; }

    // Financial
    public decimal Amount { get; set; }
    public string? AmountInWords { get; set; }

    // Bank
    public int? BankId { get; set; }
    public string? BankName { get; set; }
    public string? BankBranch { get; set; }
    public string? AccountNumber { get; set; }

    // Tenancy & flags
    public Guid? CharityId { get; set; }
    public string? CharityName { get; set; }
    public bool IsDamaged { get; set; }
    public bool IsReturned { get; set; }
    public bool IsDispensed { get; set; }
    public bool IsDone { get; set; }

    /// <summary>تعليقات.</summary>
    public string? Comment { get; set; }

    // Audit
    public DateTime CreatedOn { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedOn { get; set; }
    public string? UpdatedBy { get; set; }
}

/// <summary>
/// UC-CHQ-02 — issue a cheque. Mandatory fields per §16.S.2:
/// bank, beneficiary name, cheque date, cheque number, currency, amount.
/// </summary>
public class CreateCheckDto
{
    public int? BankId { get; set; }
    public string BeneficiaryName { get; set; } = string.Empty;
    public DateTime CheckDate { get; set; } = DateTime.Today;
    public string CheckNumber { get; set; } = string.Empty;
    public string Currency { get; set; } = "EGP";
    public decimal Amount { get; set; }

    // Optional
    public Guid? CharityId { get; set; }
    public int? ChequeBeneficiaryId { get; set; }
    public string? BeneficiaryType { get; set; }
    public string? BeneficiaryAddress { get; set; }
    public string? BeneficiaryPhone { get; set; }
    public string? BeneficiaryEmail { get; set; }
    public string? BeneficiaryIdNumber { get; set; }
    public string? BankBranch { get; set; }
    public string? AccountNumber { get; set; }
    public string? AmountInWords { get; set; }
    public string ChequeType { get; set; } = "Individuals";
    public bool IsDamaged { get; set; }
    public bool IsReturned { get; set; }
    public bool IsDispensed { get; set; }
    public bool IsDone { get; set; }
    public string? Comment { get; set; }
}

/// <summary>
/// UC-CHQ-04 — update a cheque. Same fields as create; the record id travels in the body
/// because the legacy contract (and the spec) is PUT /api/CheckManagement.
/// </summary>
public class UpdateCheckDto : CreateCheckDto
{
    public Guid Id { get; set; }
}

/// <summary>
/// Register filter (§16.S.1 / §16.S.3): charity, bank, date range, cheque type, free text.
/// </summary>
public class CheckFilterDto
{
    /// <summary>Explicit charity scope — honoured for HQ roles only; charity users are pinned to their own.</summary>
    public Guid? CharityId { get; set; }
    public int? BankId { get; set; }
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
    /// <summary>"Orphans" (شيكات إيتام) or "Individuals" (شيكات أفراد); null = both.</summary>
    public string? ChequeType { get; set; }
    /// <summary>Matches cheque number or beneficiary name.</summary>
    public string? SearchText { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

/// <summary>
/// Paged result wrapper for the register.
/// </summary>
public class CheckPagedResult<T>
{
    public List<T> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => PageSize > 0 ? (int)Math.Ceiling((decimal)TotalCount / PageSize) : 0;
}

/// <summary>
/// UC-CHQ-07 — GET /api/CheckManagement/amount-in-words response.
/// </summary>
public class AmountInWordsDto
{
    public decimal Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string Words { get; set; } = string.Empty;
}

/// <summary>
/// UC-CHQ-09 — GET /api/CheckManagement/report (cheque statement بيان الشيكات).
/// </summary>
public class CheckStatementDto
{
    public List<CheckListDto> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int TotalPages { get; set; }
    public Dictionary<string, decimal> TotalByCurrency { get; set; } = new();
    public DateTime GeneratedOn { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// UC-CHQ-05 — GET /api/LookupManagement/cheque-beneficiaries type-ahead row.
/// </summary>
public class ChequeBeneficiaryOptionDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? NameAr { get; set; }
    public string? NameEn { get; set; }
    public string? BeneficiaryType { get; set; }
    public string? Address { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? IdNumber { get; set; }
    public int? BankId { get; set; }
    public string? AccountNumber { get; set; }
}

/// <summary>
/// UC-CHQ-06 — GET /api/LookupManagement/currencies row.
/// </summary>
public class CurrencyOptionDto
{
    public string Code { get; set; } = string.Empty;
    public string? NameAr { get; set; }
    public string? NameEn { get; set; }
}

/// <summary>
/// UC-CHQ-08 — GET /api/LookupManagement/banks/{id}/cheque-positions response.
/// Configured=false means the bank has no stationery offsets stored and the caller
/// should fall back to the default layout (or report nothing to align to).
/// </summary>
public class BankChequePositionsDto
{
    public int BankId { get; set; }
    public string BankName { get; set; } = string.Empty;
    public bool Configured { get; set; }
    public decimal? DateX { get; set; }
    public decimal? DateY { get; set; }
    public decimal? PayeeX { get; set; }
    public decimal? PayeeY { get; set; }
    public decimal? AmountX { get; set; }
    public decimal? AmountY { get; set; }
    public decimal? AmountWordsX { get; set; }
    public decimal? AmountWordsY { get; set; }
}

/// <summary>
/// Register statistics band shown above the cheque register (§16.S.1, UC-CHQ-01).
/// Counts follow the caller's scope exactly like the register read: charity-pinned
/// callers see their own charity, country-pinned head-office callers their country.
/// </summary>
public class CheckStatisticsDto
{
    public int Total { get; set; }
    /// <summary>Cheques dated in the current calendar year (CheckDate, تاريخ الشيك).</summary>
    public int ThisYear { get; set; }
    /// <summary>Cheques registered since the first day of the current (UTC) month.</summary>
    public int AddedThisMonth { get; set; }
}
