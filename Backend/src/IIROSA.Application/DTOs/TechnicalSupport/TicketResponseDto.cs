namespace IIROSA.Application.DTOs.TechnicalSupport;

/// <summary>
/// Ticket Response DTO
/// Used for displaying ticket responses
/// </summary>
public class TicketResponseDto
{
    public Guid Id { get; set; }
    public Guid TicketId { get; set; }
    public string ResponseText { get; set; } = string.Empty;

    // Internal Note (UC-13.7)
    public bool IsInternalNote { get; set; }

    // Responder Information
    public string RespondedByUserId { get; set; } = string.Empty;
    public string ResponderName { get; set; } = string.Empty;
    public string ResponderEmail { get; set; } = string.Empty;

    // Attachment
    public string? AttachmentFileName { get; set; }
    public string? AttachmentFilePath { get; set; }
    public long? AttachmentFileSize { get; set; }

    // Audit Fields (inherited from FullAuditedEntity)
    public DateTime CreatedOn { get; set; }
}
