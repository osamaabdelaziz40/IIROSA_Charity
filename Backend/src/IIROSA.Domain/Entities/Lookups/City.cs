using IIROSA.Domain.Entities.Base;

namespace IIROSA.Domain.Entities.Lookups;

/// <summary>
/// City lookup entity - Inherits from LookupEntityBase (which has int Id)
/// </summary>
public class City : LookupEntity
{
    public int? CountryId { get; set; }

    // Navigation Properties
    public virtual Country? Country { get; set; }
}
