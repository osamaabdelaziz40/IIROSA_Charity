using Framework.Core.SharedServices.Entities;
using IIROSA.Domain.Entities.Base;
using IIROSA.Domain.Entities.Lookups;

namespace IIROSA.Domain.Entities;

/// <summary>
/// HqTransfer entity — a financial transfer issued by HQ (الإدارة المالية) to a destination
/// country (UC-TRF-01…08). Audit fields (CreatedOn, CreatedBy, UpdatedOn, UpdatedBy, DeletedOn,
/// DeletedBy, IsDeleted) are inherited from FullAuditedEntity.
/// Head-office module: only Admin and SuperAdmin reach it; a caller whose token carries a country
/// claim is pinned to that country on every read and write.
/// </summary>
public class HqTransfer : FullAuditedEntity
{
    // ========== Destination (الدولة) ==========

    /// <summary>
    /// Foreign key to Country lookup (required) — the transfer's destination
    /// </summary>
    public int FK_CountryId { get; set; }

    // ========== Requesting department (اسم الادارة الطالبة) ==========

    /// <summary>
    /// Foreign key to Department lookup (required) — the issuing head-office department
    /// </summary>
    public int FK_DepartmentId { get; set; }

    // ========== Operation identification ==========

    /// <summary>
    /// رقم العملية (required)
    /// </summary>
    public string OperationNumber { get; set; } = string.Empty;

    /// <summary>
    /// السنة المالية (required) — legacy free-text box, kept as text
    /// </summary>
    public string FinYear { get; set; } = string.Empty;

    /// <summary>
    /// رقم الدفعة (required) — domain 1..4 (validated in the service layer)
    /// </summary>
    public int PaymentNumber { get; set; }

    // ========== Period ==========

    /// <summary>
    /// من تاريخ (required)
    /// </summary>
    public DateTime DateFrom { get; set; }

    /// <summary>
    /// الى تاريخ (required)
    /// </summary>
    public DateTime DateTo { get; set; }

    // ========== Financials ==========

    /// <summary>
    /// مبلغ الدفعة (required)
    /// </summary>
    public decimal AmountOfPayment { get; set; }

    /// <summary>
    /// البيان (optional)
    /// </summary>
    public string? Statement { get; set; }

    /// <summary>
    /// عدد المستفيدين (required)
    /// </summary>
    public int BeneficiariesNumber { get; set; }

    // ========== Transaction ==========

    /// <summary>
    /// رقم المعاملة (required)
    /// </summary>
    public string TransactionNumber { get; set; } = string.Empty;

    /// <summary>
    /// تاريخ المعاملة (required)
    /// </summary>
    public DateTime TransactionDate { get; set; }

    // ========== Navigation Properties ==========

    /// <summary>
    /// Navigation to Country lookup
    /// </summary>
    public virtual Country? Country { get; set; }

    /// <summary>
    /// Navigation to Department lookup
    /// </summary>
    public virtual Department? Department { get; set; }

    /// <summary>
    /// Navigation to the transfer's allocation lines (تفاصيل الحوالة, UC-TRF-08)
    /// </summary>
    public virtual ICollection<HqTransferDetail> Details { get; set; } = new List<HqTransferDetail>();
}
