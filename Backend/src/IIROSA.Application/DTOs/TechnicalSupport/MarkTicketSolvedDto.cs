using System.ComponentModel.DataAnnotations;

namespace IIROSA.Application.DTOs.TechnicalSupport;

/// <summary>
/// Mark Ticket as Solved DTO
/// Used for UC-13.6: Mark Ticket as Solved
/// </summary>
public class MarkTicketSolvedDto
{
    [Required]
    public Guid TicketId { get; set; }

    [Required(ErrorMessage = "Resolution description is required")]
    [StringLength(4000, ErrorMessage = "Resolution description cannot exceed 4000 characters")]
    public string ResolutionDescription { get; set; } = string.Empty;

    // Solution Steps (multiline)
    public string? SolutionSteps { get; set; }

    // Optional Attachments
    public string? AttachmentFileName { get; set; }
    public string? AttachmentFilePath { get; set; }
    public long? AttachmentFileSize { get; set; }
}
