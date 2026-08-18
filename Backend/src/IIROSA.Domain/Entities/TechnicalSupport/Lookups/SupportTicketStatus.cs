using IIROSA.Domain.Entities.Base;

namespace IIROSA.Domain.Entities.TechnicalSupport.Lookups;

/// <summary>
/// Support Ticket Status lookup entity
/// Inherits from LookupEntity (int Id, Name, IsActive, etc.)
/// Used for tracking ticket status (UC-13.4, UC-13.5)
/// </summary>
public class SupportTicketStatus : LookupEntity
{
    // Id: int (inherited)
    // NameAr, NameEn, Name, IsActive (inherited from LookupEntity)
    // CreatedBy, CreatedOn, UpdatedBy, UpdatedOn (inherited from LookupEntity)

    // Additional properties
    public string? Description { get; set; }
    public string? ColorCode { get; set; }  // For UI display
    public int SortOrder { get; set; } = 0;
    public bool IsTerminalStatus { get; set; } = false;  // e.g., Closed, Resolved

    // Navigation
    public virtual ICollection<SupportTicket> Tickets { get; set; } = new List<SupportTicket>();
}
