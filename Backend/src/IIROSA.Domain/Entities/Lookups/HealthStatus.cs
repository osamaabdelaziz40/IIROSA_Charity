using IIROSA.Domain.Entities.Base;

namespace IIROSA.Domain.Entities.Lookups;

/// <summary>
/// Health Status lookup entity (UC-4.2, UC-4.3, UC-4.4)
/// Values: Excellent, Good, Fair, Poor, Critical
/// Inherits from LookupEntity which provides: Id (int), Name, NameAr, Description, IsActive, SortOrder, and audit fields
/// </summary>
public class HealthStatus : LookupEntity
{
    // Additional properties can be added here if needed
    // Id, Name, NameAr, Description, IsActive, SortOrder are inherited from LookupEntity
}
