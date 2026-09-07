using IIROSA.Domain.Entities.Base;

namespace IIROSA.Domain.Entities;

/// <summary>
/// Outgoing ↔ Orphan report attachment link (epic 16, UC-COR-18 — إضافة تقارير الأيتام
/// إلى خطاب صادر). BR-26: an orphan's report attaches to at most ONE outgoing letter —
/// enforced by a filtered unique index on OrphanId; the service refuses with
/// «Operation Faild» before the index ever fires.
/// </summary>
public class OutgoingOrphanReport : FullAuditedEntity
{
    public Guid OutgoingId { get; set; }

    public Guid OrphanId { get; set; }

    public virtual Outgoing Outgoing { get; set; } = null!;

    public virtual Orphan Orphan { get; set; } = null!;
}
