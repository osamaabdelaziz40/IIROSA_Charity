using IIROSA.Domain.Entities.Base;

namespace IIROSA.Domain.Entities.Lookups;

/// <summary>
/// Reason of Relation lookup entity (epic 7, UC-REF-03 §12.S.2 — السبب of a provider's link to the family)
/// Inherits from LookupEntity which provides: Id (int), Name, NameAr, Description, IsActive, SortOrder, and audit fields
/// </summary>
public class ReasonOfRel : LookupEntity
{
    // Id, NameAr, NameEn, Name, IsActive, SortOrder are inherited from LookupEntityBase
}
