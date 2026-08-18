using IIROSA.Domain.Entities.Base;
using IIROSA.Domain.Entities.Lookups;

namespace IIROSA.Domain.Entities;

/// <summary>
/// Sponsor entity - Inherits from FullAuditedEntityBase<Guid>
/// </summary>
public class Sponsor : FullAuditedEntity
{
    public string Code { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public int? CountryId { get; set; }
    public int? CityId { get; set; }
    public string? SponsorshipType { get; set; }
    public decimal? MonthlyAmount { get; set; }
    public string? PaymentMethod { get; set; }
    public Guid? FK_CharityId { get; set; }
    public bool IsActive { get; set; } = true;
    public string? Notes { get; set; }

    // Navigation Properties
    public virtual Country? Country { get; set; }
    public virtual City? City { get; set; }
    public virtual ICollection<Orphan> Orphans { get; set; } = new List<Orphan>();
}
