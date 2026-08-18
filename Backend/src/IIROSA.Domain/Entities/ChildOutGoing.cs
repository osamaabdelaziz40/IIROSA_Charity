using IIROSA.Domain.Entities.Base;

namespace IIROSA.Domain.Entities;

/// <summary>
/// Child Outgoing Letters - Follow-up letters
/// Linked to parent Outgoing letter
/// </summary>
public class ChildOutGoing : FullAuditedEntity
{
    // Foreign Key to parent Outgoing letter
    public Guid OutgoingId { get; set; }

    // Basic Letter Information
    public string Subject { get; set; } = string.Empty;
    public DateTime? Date { get; set; }
    public string? Body { get; set; }
    public int? Year { get; set; }

    // Foreign Keys
    public int? Fk_DepartmentId { get; set; }
    public Guid? UploadedFileId { get; set; }

    // Navigation Properties
    public virtual Outgoing Outgoing { get; set; } = null!;
    public virtual UploadedFile? UploadedFile { get; set; }
}
