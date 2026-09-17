using IIROSA.Domain.Entities.Base;
using IIROSA.Domain.Entities.Lookups;

namespace IIROSA.Domain.Entities;

/// <summary>
/// Outgoing Letters/Correspondence Entity (epic 16, UC-COR-10…19)
/// Inherits audit fields from FullAuditedEntity
/// </summary>
public class Outgoing : FullAuditedEntity
{
    // Serial Information (Auto-generated, per charity + year — UC-COR-12)
    public int? Serial { get; set; }

    // Basic Letter Information
    public string Subject { get; set; } = string.Empty;
    public DateTime? Date { get; set; }
    public string? OutGoingNumber { get; set; }
    public string OutGoingId { get; set; } = string.Empty;
    public string? Body { get; set; }

    // Letter Details
    public int? Year { get; set; }

    // Foreign Keys
    public int? Fk_DepartmentId { get; set; }
    // The dispatch's file (الملف, §21.S.5) — a framework Attachment id minted by
    // POST /api/attachments/upload. Loose Guid reference with no EF navigation, the
    // platform's established pattern for framework attachments (e.g. orphan photo ids).
    public Guid? UploadedFileId { get; set; }
    public int? OutgoingCategoryId { get; set; }
    public Guid? IncomingId { get; set; }
    public Guid? FK_CharityId { get; set; }  // Owning charity — tenancy (stamped server-side)

    // Navigation Properties
    public virtual Department? Department { get; set; }
    public virtual OutgoingCategory? Category { get; set; }
    public virtual Incoming? IncomingLetter { get; set; }
    public virtual Charity? Charity { get; set; }

    // Navigation Collections
    public virtual ICollection<OutgoingOrphanReport> OrphanReports { get; set; } = new List<OutgoingOrphanReport>();
}
