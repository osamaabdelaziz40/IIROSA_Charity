using IIROSA.Domain.Entities.TechnicalSupport.Lookups;

namespace IIROSA.Domain.Interfaces;

/// <summary>
/// Support Ticket Lookup Repository Interface
/// Reads the seeded category / priority / status lookup tables of the Technical Support module.
/// </summary>
public interface ISupportTicketLookupRepository
{
    Task<IEnumerable<SupportTicketCategory>> GetCategoriesAsync();
    Task<IEnumerable<SupportTicketPriority>> GetPrioritiesAsync();
    Task<IEnumerable<SupportTicketStatus>> GetStatusesAsync();

    // Existence checks — FK guards so invalid ids fail as 400 field errors, not 500s
    Task<bool> CategoryExistsAsync(int id);
    Task<bool> PriorityExistsAsync(int id);
    Task<bool> StatusExistsAsync(int id);
}
