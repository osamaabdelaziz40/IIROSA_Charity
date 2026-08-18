namespace IIROSA.Application.DTOs.TechnicalSupport;

/// <summary>
/// Support Ticket DTO
/// Used for displaying ticket details
/// </summary>
public class SupportTicketDto
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

    // Audit Fields (inherited from FullAuditedEntity)
    public DateTime CreatedOn { get; set; }
    public DateTime UpdatedOn { get; set; }

    // Response Count
    public int ResponseCount { get; set; }
}
