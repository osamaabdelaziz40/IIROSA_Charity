using System.ComponentModel.DataAnnotations;

namespace IIROSA.Application.DTOs.TechnicalSupport;

/// <summary>
/// Create Ticket Response DTO
/// Used for UC-13.7: Add Ticket Response
/// </summary>
public class CreateTicketResponseDto
{
    [Required]
    public Guid TicketId { get; set; }

    [Required(ErrorMessage = "Response text is required")]
    [StringLength(4000, ErrorMessage = "Response cannot exceed 4000 characters")]
    public string ResponseText { get; set; } = string.Empty;

    // Internal Note (UC-13.7)
    public bool IsInternalNote { get; set; } = false;

    // Optional Attachment
    public string? AttachmentFileName { get; set; }
    public string? AttachmentFilePath { get; set; }
    public long? AttachmentFileSize { get; set; }
}
