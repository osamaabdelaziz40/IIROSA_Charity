using IIROSA.Domain.Entities.Base;

namespace IIROSA.Domain.Entities.Lookups;

/// <summary>
/// Death reason lookup entity (سبب الوفاة)
/// Seeds: طبيعية / مرض / حادث — feeds the father/mother death-reason drop-downs
/// Inherits from LookupEntity which provides: Id (int), Name, NameAr, NameEn, Description, IsActive, SortOrder, and audit fields
/// </summary>
public class DeathReason : LookupEntity
{
    // Id, Name, NameAr, NameEn, Description, IsActive, SortOrder are inherited from LookupEntity
}
