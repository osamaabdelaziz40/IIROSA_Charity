using Framework.Identity.Data.Entities;
using IIROSA.Domain.Entities.Base;

namespace IIROSA.Domain.Entities;

/// <summary>
/// Incoming ↔ Employee attachment link (epic 16, UC-COR-09 — إضافة موظفين إلى خطاب وارد).
/// The register records which employees a dispatch/routing carried. A (letter, user) pair
/// links at most once — enforced by a filtered unique index (BR behind «Operation Faild»).
/// </summary>
public class IncomingEmployee : FullAuditedEntity
{
    public Guid IncomingId { get; set; }

    public Guid UserId { get; set; }

    public virtual Incoming Incoming { get; set; } = null!;

    public virtual ApplicationUser User { get; set; } = null!;
}
