using IIROSA.Domain.Entities.Base;

namespace IIROSA.Domain.Entities.Lookups;

/// <summary>
/// Living Condition lookup entity (UC-4.1)
/// Values: Good, Fair, Poor, Critical
/// Inherits from LookupEntity which provides: Id (int), Name, NameAr, Description, IsActive, SortOrder, and audit fields
/// </summary>
public class LivingCondition : LookupEntity
{
    // Additional properties can be added here if needed
    // Id, Name, NameAr, Description, IsActive, SortOrder are inherited from LookupEntity
}
