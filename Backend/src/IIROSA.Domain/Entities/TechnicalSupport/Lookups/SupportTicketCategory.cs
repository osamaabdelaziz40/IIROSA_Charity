using IIROSA.Domain.Entities.Base;

namespace IIROSA.Domain.Entities.TechnicalSupport.Lookups;

/// <summary>
/// Support Ticket Category lookup entity
/// Inherits from LookupEntity (int Id, Name, IsActive, etc.)
/// Used for categorizing support tickets (UC-13.1, UC-13.4)
/// </summary>
public class SupportTicketCategory : LookupEntity
{
    // Id: int (inherited)
    // NameAr, NameEn, Name, IsActive (inherited from LookupEntity)
    // CreatedBy, CreatedOn, UpdatedBy, UpdatedOn (inherited from LookupEntity)

    // Additional properties
    public string? Description { get; set; }
    public string? Icon { get; set; }
    public int SortOrder { get; set; } = 0;

    // Navigation
    public virtual ICollection<SupportTicket> Tickets { get; set; } = new List<SupportTicket>();
}
