using IIROSA.Domain.Entities.Base;

namespace IIROSA.Domain.Entities.Lookups;

/// <summary>
/// Social Status lookup entity (epic 7, UC-REF-03 §12.S.2 — الحالة الاجتماعية of an اضافة ابن)
/// Inherits from LookupEntity which provides: Id (int), Name, NameAr, Description, IsActive, SortOrder, and audit fields
/// </summary>
public class SocialStatus : LookupEntity
{
    // Id, NameAr, NameEn, Name, IsActive, SortOrder are inherited from LookupEntityBase
}
