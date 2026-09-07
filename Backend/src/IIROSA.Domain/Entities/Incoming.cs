using IIROSA.Domain.Entities.Base;
using IIROSA.Domain.Entities.Lookups;
using Framework.Identity.Data.Entities;

namespace IIROSA.Domain.Entities;

/// <summary>
/// Incoming Letters/Correspondence Entity (epic 16, UC-COR-01…09)
/// Inherits audit fields from FullAuditedEntity
/// </summary>
public class Incoming : FullAuditedEntity
{
    // Serial Information (Auto-generated, per charity + year — UC-COR-03)
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
    public Guid? FK_UserId { get; set; }  // The employee the letter is routed to (الموظف المناط به)
    public Guid? UploadedFileId { get; set; }
    public Guid? FK_CharityId { get; set; }  // Owning charity — tenancy (stamped server-side)

    // Navigation Properties
    public virtual Department? Department { get; set; }
    public virtual UploadedFile? UploadedFile { get; set; }
    public virtual ApplicationUser? AssignedUser { get; set; }
    public virtual Charity? Charity { get; set; }

    // Navigation Collections
    public virtual ICollection<Outgoing> Replies { get; set; } = new List<Outgoing>();
    public virtual ICollection<IncomingEmployee> Employees { get; set; } = new List<IncomingEmployee>();
}
