using IIROSA.Domain.Entities.Base;

namespace IIROSA.Domain.Entities.TechnicalSupport;

/// <summary>
/// Ticket Response entity - Inherits from FullAuditedEntityBase<Guid>
/// Implements use case UC-13.7: Add Ticket Response
/// All audit fields (CreatedOn, UpdatedOn, CreatedBy, UpdatedBy, DeletedOn, DeletedBy, IsDeleted) are inherited
/// </summary>
public class TicketResponse : FullAuditedEntity
{
    // Ticket Reference (UC-13.7)
    public Guid TicketId { get; set; }

    // Response Content (UC-13.7)
    public string ResponseText { get; set; } = string.Empty;

    // Internal Note (UC-13.7)
    public bool IsInternalNote { get; set; } = false;

    // Responder Information (UC-13.7)
    public string RespondedByUserId { get; set; } = string.Empty;
    public string ResponderName { get; set; } = string.Empty;
    public string ResponderEmail { get; set; } = string.Empty;

    // Attachment (UC-13.7)
    public string? AttachmentFilePath { get; set; }
    public string? AttachmentFileName { get; set; }
    public long? AttachmentFileSize { get; set; }

    // Navigation Properties
    public virtual SupportTicket Ticket { get; set; } = null!;
}
