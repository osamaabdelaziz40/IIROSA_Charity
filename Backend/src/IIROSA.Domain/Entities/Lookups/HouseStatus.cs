using IIROSA.Domain.Entities.Base;

namespace IIROSA.Domain.Entities.Lookups;

/// <summary>
/// House Status lookup entity (epic 7, UC-REF-03 §12.S.2 حالة محتويات السكن)
/// Inherits from LookupEntity which provides: Id (int), Name, NameAr, Description, IsActive, SortOrder, and audit fields
/// </summary>
public class HouseStatus : LookupEntity
{
    // Id, NameAr, NameEn, Name, IsActive, SortOrder are inherited from LookupEntityBase
}
