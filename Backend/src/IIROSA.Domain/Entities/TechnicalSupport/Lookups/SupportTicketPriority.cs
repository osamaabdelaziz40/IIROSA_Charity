using IIROSA.Domain.Entities.Base;

namespace IIROSA.Domain.Entities.TechnicalSupport.Lookups;

/// <summary>
/// Support Ticket Priority lookup entity
/// Inherits from LookupEntity (int Id, Name, IsActive, etc.)
/// Used for setting ticket priority levels (UC-13.1, UC-13.4)
/// </summary>
public class SupportTicketPriority : LookupEntity
{
    // Id: int (inherited)
    // NameAr, NameEn, Name, IsActive (inherited from LookupEntity)
    // CreatedBy, CreatedOn, UpdatedBy, UpdatedOn (inherited from LookupEntity)

    // Additional properties
    public string? Description { get; set; }
    public string? ColorCode { get; set; }  // For UI display (e.g., "#FF0000" for Urgent)
    public int SeverityLevel { get; set; } = 0;  // 0=Low, 1=Medium, 2=High, 3=Urgent
    public int ResponseTimeHours { get; set; } = 24;  // Expected response time in hours

    // Navigation
    public virtual ICollection<SupportTicket> Tickets { get; set; } = new List<SupportTicket>();
}
