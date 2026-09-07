using IIROSA.Domain.Entities.Base;

namespace IIROSA.Domain.Entities.Lookups;

/// <summary>
/// Relation lookup entity (epic 7, UC-REF-03 §12.S.2 — نوع العلاقة of a provider, نوعها)
/// The chosen value is stored on Provider.RelationshipToFamily (string) — this lookup is the source list.
/// Inherits from LookupEntity which provides: Id (int), Name, NameAr, Description, IsActive, SortOrder, and audit fields
/// </summary>
public class Relation : LookupEntity
{
    // Id, NameAr, NameEn, Name, IsActive, SortOrder are inherited from LookupEntityBase
}
