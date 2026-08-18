using IIROSA.Domain.Entities.Base;
using IIROSA.Domain.Entities.Lookups;

namespace IIROSA.Domain.Entities;

/// <summary>
/// Incoming Letters/Correspondence Entity
/// Implements use cases UC-12.1, UC-12.3, UC-12.4, UC-12.5, UC-12.6, UC-12.10, UC-12.12, UC-12.13
/// Inherits audit fields from FullAuditedEntity
/// </summary>
public class Incoming : FullAuditedEntity
{
    // Serial Information (Auto-generated)
    public int? Serial { get; set; }
    public string? Serial_Txt { get; set; }

    // Basic Letter Information
    public string Subject { get; set; } = string.Empty;
    public DateTime? Date { get; set; }
    public string? IncomingNumber { get; set; }
    public string IncomingId { get; set; } = string.Empty;
    public string? Body { get; set; }

    // Letter Details
    public string? LetterNumber { get; set; }
    public DateTime? LetterDate { get; set; }
    public int? Year { get; set; }
    public string? Status { get; set; }
    public string? LetterDescription { get; set; }

    // Foreign Keys
    public int? FK_DepartmentId { get; set; }
    public Guid? FK_UserId { get; set; }  // Created by user (separate from audit trail)
    public Guid? OutgoingId { get; set; }  // Linked outgoing letter (if this is a reply)
    public Guid? UploadedFileId { get; set; }

    // Navigation Properties
    public virtual Department? Department { get; set; }
    public virtual Outgoing? OutgoingLetter { get; set; }
    public virtual UploadedFile? UploadedFile { get; set; }

    // Navigation Collections
    public virtual ICollection<Outgoing> Replies { get; set; } = new List<Outgoing>();
}
