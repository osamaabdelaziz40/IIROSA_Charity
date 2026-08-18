using System.ComponentModel.DataAnnotations;

namespace IIROSA.Application.DTOs.TechnicalSupport;

/// <summary>
/// Update Ticket Status DTO
/// Used for UC-13.5: Update Ticket Status
/// </summary>
public class UpdateTicketStatusDto
{
    [Required]
    public Guid TicketId { get; set; }

    [Required(ErrorMessage = "Status is required")]
    public int StatusId { get; set; }

    // Optional status note/comment
    public string? StatusNote { get; set; }
}
