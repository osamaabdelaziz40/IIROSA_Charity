using IIROSA.Domain.Entities.Base;
using IIROSA.Domain.Entities.Lookups;

namespace IIROSA.Domain.Entities;

/// <summary>
/// Outgoing Letters/Correspondence Entity
/// Implements use cases UC-12.2, UC-12.3, UC-12.4, UC-12.5, UC-12.6, UC-12.11, UC-12.12, UC-12.13
/// Inherits audit fields from FullAuditedEntity
/// </summary>
public class Outgoing : FullAuditedEntity
{
    // Serial Information (Auto-generated)
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
    public Guid? UploadedFileId { get; set; }
    public int? OutgoingCategoryId { get; set; }
    public Guid? IncomingId { get; set; }

    // Navigation Properties
    public virtual Department? Department { get; set; }
    public virtual UploadedFile? UploadedFile { get; set; }
    public virtual OutgoingCategory? Category { get; set; }
    public virtual Incoming? IncomingLetter { get; set; }

    // Navigation Collections
    public virtual ICollection<ChildOutGoing> ChildOutGoings { get; set; } = new List<ChildOutGoing>();
}
