using IIROSA.Domain.Entities.Base;
using IIROSA.Domain.Entities.TechnicalSupport.Lookups;

namespace IIROSA.Domain.Entities.TechnicalSupport;

/// <summary>
/// Support Ticket entity - Inherits from FullAuditedEntityBase<Guid>
/// Implements all use cases UC-13.1 through UC-13.10
/// All audit fields (CreatedOn, UpdatedOn, CreatedBy, UpdatedBy, DeletedOn, DeletedBy, IsDeleted) are inherited
/// </summary>
public class SupportTicket : FullAuditedEntity
{
    // Basic Information (UC-13.1)
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;

    // Categorization (UC-13.1, UC-13.4)
    public int CategoryId { get; set; }
    public int PriorityId { get; set; }
    public int StatusId { get; set; }

    // Resolution (UC-13.6)
    public bool IsSolved { get; set; } = false;
    public string? ResolutionDescription { get; set; }
    public DateTime? ResolvedOn { get; set; }
    public string? ResolvedBy { get; set; }

    // System Information (UC-13.1)
    public string? BrowserInfo { get; set; }
    public string? PageUrl { get; set; }
    public string? UserAction { get; set; }

    // Assignment (UC-13.4, UC-13.8)
    public string? AssignedTo { get; set; }

    // User who created the ticket (UC-13.1, UC-13.3)
    public string CreatedByUserId { get; set; } = string.Empty;

    // Attachment (UC-13.1, UC-13.2)
    public string? AttachmentFilePath { get; set; }
    public string? AttachmentFileName { get; set; }
    public long? AttachmentFileSize { get; set; }

    // Navigation Properties
    public virtual SupportTicketCategory Category { get; set; } = null!;
    public virtual SupportTicketPriority Priority { get; set; } = null!;
    public virtual SupportTicketStatus Status { get; set; } = null!;

    public virtual ICollection<TicketResponse> Responses { get; set; } = new List<TicketResponse>();
}
