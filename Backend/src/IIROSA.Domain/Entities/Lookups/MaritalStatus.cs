using IIROSA.Domain.Entities.Base;

namespace IIROSA.Domain.Entities.Lookups;

/// <summary>
/// Marital Status lookup entity (UC-SYS-05 — the guardian/provider sections of the family forms).
/// Values: Single أعزب, Married متزوج, Divorced مطلق, Widowed أرمل
/// Inherits from LookupEntity which provides: Id (int), Name, NameAr, NameEn, Description, IsActive, SortOrder, and audit fields
/// </summary>
public class MaritalStatus : LookupEntity
{
    // Additional properties can be added here if needed
    // Id, Name, NameAr, Description, IsActive, SortOrder are inherited from LookupEntity
}
