using IIROSA.Domain.Entities.Base;

namespace IIROSA.Domain.Entities.Lookups;

/// <summary>
/// Family project status lookup entity (حالة المشروع)
/// Seeded values: يوجد مشروع قائم / مشروع جديد
/// Inherits from LookupEntity which provides: Id (int), Name, NameAr, NameEn, Description, IsActive, SortOrder, and audit fields
/// </summary>
public class FamilyProjectStatus : LookupEntity
{
    // Id, NameAr, NameEn, Name, IsActive, SortOrder are inherited from LookupEntityBase
}
