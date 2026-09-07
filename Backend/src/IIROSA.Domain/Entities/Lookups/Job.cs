using IIROSA.Domain.Entities.Base;

namespace IIROSA.Domain.Entities.Lookups;

/// <summary>
/// Job / profession lookup entity (UC-SYS-09 — the guardian/provider job fields on the family forms).
/// Values: government employee, private-sector employee, labourer, merchant, farmer, housewife, retired, unemployed …
/// Inherits from LookupEntity which provides: Id (int), Name, NameAr, NameEn, Description, IsActive, SortOrder, and audit fields
/// </summary>
public class Job : LookupEntity
{
    // Additional properties can be added here if needed
    // Id, Name, NameAr, Description, IsActive, SortOrder are inherited from LookupEntity
}
