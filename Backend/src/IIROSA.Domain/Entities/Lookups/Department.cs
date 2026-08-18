using IIROSA.Domain.Entities.Base;

namespace IIROSA.Domain.Entities.Lookups;

/// <summary>
/// Department lookup entity - Inherits from LookupEntityBase (which has int Id)
/// </summary>
public class Department : LookupEntity
{
    public string? Description { get; set; }
}
