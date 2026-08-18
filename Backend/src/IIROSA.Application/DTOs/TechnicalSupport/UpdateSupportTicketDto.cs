using System.ComponentModel.DataAnnotations;

namespace IIROSA.Application.DTOs.TechnicalSupport;

/// <summary>
/// Update Support Ticket DTO
/// Used for updating ticket details
/// </summary>
public class UpdateSupportTicketDto
{
    [Required]
    public Guid Id { get; set; }

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

    [Required(ErrorMessage = "Status is required")]
    public int StatusId { get; set; }
}
