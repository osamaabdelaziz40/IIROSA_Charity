using System.ComponentModel.DataAnnotations;

namespace IIROSA.Application.DTOs.TechnicalSupport;

/// <summary>
/// Create Support Ticket DTO
/// Used for UC-13.1: Create Support Ticket
/// </summary>
public class CreateSupportTicketDto
{
    [Required(ErrorMessage = "Title is required")]
    [StringLength(200, ErrorMessage = "Title cannot exceed 200 characters")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Message is required")]
    [StringLength(4000, ErrorMessage = "Message cannot exceed 4000 characters")]
    public string Message { get; set; } = string.Empty;

    [Required(ErrorMessage = "Category is required")]
    public int CategoryId { get; set; }

    [Required(ErrorMessage = "Priority is required")]
    public int PriorityId { get; set; }

    // Optional Attachment (UC-13.2)
    public string? AttachmentFileName { get; set; }
    public string? AttachmentFilePath { get; set; }
    public long? AttachmentFileSize { get; set; }

    // System Information (auto-detected)
    public string? BrowserInfo { get; set; }
    public string? PageUrl { get; set; }
    public string? UserAction { get; set; }
}
