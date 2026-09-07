using IIROSA.Domain.Entities.Base;

namespace IIROSA.Domain.Entities;

/// <summary>
/// FamilyCharityTransfer entity — the audit record of a family being moved from one charity to
/// another (UC-FAM-06 نقل الأسرة لجمعية أخرى). One row per transfer; who moved the family and
/// when come from the inherited audit fields (CreatedBy / CreatedOn). Transfers are HQ-only
/// (SuperAdmin, Admin) and are refused unless family + orphans re-scope in the same transaction.
/// </summary>
public class FamilyCharityTransfer : FullAuditedEntity
{
    // ========== Family (الأسرة) ==========

    /// <summary>
    /// Foreign key to the transferred Family (required)
    /// </summary>
    public Guid FamilyId { get; set; }

    // ========== Movement (من / إلى) ==========

    /// <summary>
    /// The charity the family belonged to before the transfer (required)
    /// </summary>
    public Guid FromCharityId { get; set; }

    /// <summary>
    /// The receiving charity (required)
    /// </summary>
    public Guid ToCharityId { get; set; }

    // ========== Reason (السبب) ==========

    /// <summary>
    /// سبب النقل (optional, free text)
    /// </summary>
    public string? Reason { get; set; }

    // ========== Navigation Properties ==========

    /// <summary>
    /// Navigation to the transferred family
    /// </summary>
    public virtual Family? Family { get; set; }

    /// <summary>
    /// Navigation to the sending charity
    /// </summary>
    public virtual Charity? FromCharity { get; set; }

    /// <summary>
    /// Navigation to the receiving charity
    /// </summary>
    public virtual Charity? ToCharity { get; set; }
}
