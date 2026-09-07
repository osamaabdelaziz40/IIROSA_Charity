using IIROSA.Domain.Entities.Base;

namespace IIROSA.Domain.Entities.Lookups;

/// <summary>
/// House Ownership lookup entity (epic 7, UC-REF-03 §12.S.2 ملكية السكن)
/// Values: ملك / إيجار / ...
/// Inherits from LookupEntity which provides: Id (int), Name, NameAr, Description, IsActive, SortOrder, and audit fields
/// </summary>
public class HouseOwnership : LookupEntity
{
    // Id, NameAr, NameEn, Name, IsActive, SortOrder are inherited from LookupEntityBase
}
