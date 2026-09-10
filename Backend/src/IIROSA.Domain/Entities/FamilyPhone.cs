using IIROSA.Domain.Entities.Base;

namespace IIROSA.Domain.Entities;

/// <summary>
/// Family contact phone (UC-4.x family data extension) — a family carries several numbers,
/// exactly one of which is the default. Family.PhoneNumber mirrors the default row so legacy
/// list/report consumers keep working without a join.
/// </summary>
public class FamilyPhone : FullAuditedEntity
{
    /// <summary>
    /// Owning family
    /// </summary>
    public Guid FamilyId { get; set; }

    /// <summary>
    /// The number as dialled
    /// </summary>
    public string Number { get; set; } = string.Empty;

    /// <summary>
    /// Default-number flag — the service keeps at most one row per family true
    /// </summary>
    public bool IsDefault { get; set; } = false;

    // Navigation Properties
    public virtual Family? Family { get; set; }
}
