using IIROSA.Application.DTOs.LookupManagement;

namespace IIROSA.Application.DTOs.TechnicalSupport;

/// <summary>
/// Ticket Lookups DTO
/// Category / priority / status options for the ticket form selects.
/// </summary>
public class TicketLookupsDto
{
    public IEnumerable<LookupDto> Categories { get; set; } = Enumerable.Empty<LookupDto>();
    public IEnumerable<LookupDto> Priorities { get; set; } = Enumerable.Empty<LookupDto>();
    public IEnumerable<LookupDto> Statuses { get; set; } = Enumerable.Empty<LookupDto>();
}
