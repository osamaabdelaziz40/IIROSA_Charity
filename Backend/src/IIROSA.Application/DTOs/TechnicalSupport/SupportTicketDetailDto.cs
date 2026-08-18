namespace IIROSA.Application.DTOs.TechnicalSupport;

/// <summary>
/// Support Ticket Detail DTO
/// Used for UC-13.8: View Ticket Details
/// Includes complete ticket information with all responses
/// </summary>
public class SupportTicketDetailDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;

    // Lookup Properties
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public int PriorityId { get; set; }
    public string PriorityName { get; set; } = string.Empty;
    public int StatusId { get; set; }
    public string StatusName { get; set; } = string.Empty;

    // Resolution
    public bool IsSolved { get; set; }
    public string? ResolutionDescription { get; set; }
    public DateTime? ResolvedOn { get; set; }
    public string? ResolvedBy { get; set; }

    // System Information
    public string? BrowserInfo { get; set; }
    public string? PageUrl { get; set; }
    public string? UserAction { get; set; }

    // Assignment
    public string? AssignedTo { get; set; }
    public string? AssignedToName { get; set; }

    // User Information
    public string CreatedByUserId { get; set; } = string.Empty;
    public string CreatedByUserName { get; set; } = string.Empty;
    public string CreatedByEmail { get; set; } = string.Empty;

    // Attachment
    public string? AttachmentFileName { get; set; }
    public string? AttachmentFilePath { get; set; }
    public long? AttachmentFileSize { get; set; }

    // Audit Fields
    public DateTime CreatedOn { get; set; }
    public DateTime UpdatedOn { get; set; }

    // Response History (UC-13.8)
    public IEnumerable<TicketResponseDto> Responses { get; set; } = new List<TicketResponseDto>();

    // Public Responses Only (for regular users)
    public IEnumerable<TicketResponseDto> PublicResponses { get; set; } = new List<TicketResponseDto>();

    // Internal Notes Only (for Admin/Super Admin)
    public IEnumerable<TicketResponseDto> InternalNotes { get; set; } = new List<TicketResponseDto>();
}
