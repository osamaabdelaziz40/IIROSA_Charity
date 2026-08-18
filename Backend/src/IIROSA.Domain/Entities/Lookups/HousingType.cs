using IIROSA.Domain.Entities.Base;

namespace IIROSA.Domain.Entities.Lookups;

/// <summary>
/// Housing Type lookup entity (UC-4.1)
/// Values: Owned, Rented, Shared, Temporary, Other
/// Inherits from LookupEntity which provides: Id (int), Name, NameAr, Description, IsActive, SortOrder, and audit fields
/// </summary>
public class HousingType : LookupEntity
{
    // Additional properties can be added here if needed
    // Id, Name, NameAr, Description, IsActive, SortOrder are inherited from LookupEntity
}
