using IIROSA.Domain.Entities.Base;
using IIROSA.Domain.Entities.Lookups;

namespace IIROSA.Domain.Entities;

/// <summary>
/// Relative entity - Represents additional family relatives (siblings, grandparents, uncles, aunts, etc.)
/// Inherits from FullAuditedEntity which provides all audit fields
/// Implements use cases UC-4.7 (Add Family Relative), UC-4.8 (Update Relative Details), UC-4.9 (Remove Relative from Family)
/// </summary>
public class Relative : FullAuditedEntity
{
    /// <summary>
    /// Foreign key to Family
    /// </summary>
    public Guid? FamilyId { get; set; }

    /// <summary>
    /// Full name of the relative (Required)
    /// </summary>
    public string FullName { get; set; } = string.Empty;

    /// <summary>
    /// Relationship type to family: Brother, Sister, Grandfather, Grandmother, Uncle, Aunt, Cousin, Guardian, Other
    /// </summary>
    public string RelationshipType { get; set; } = string.Empty;

    /// <summary>
    /// Gender: Male, Female
    /// </summary>
    public string Gender { get; set; } = string.Empty;

    /// <summary>
    /// Date of birth (Required)
    /// </summary>
    public DateTime DateOfBirth { get; set; }

    /// <summary>
    /// Place of birth
    /// </summary>
    public string? PlaceOfBirth { get; set; }

    /// <summary>
    /// National ID or Passport number
    /// </summary>
    public string? NationalId { get; set; }

    /// <summary>
    /// Foreign key to Education Level lookup
    /// </summary>
    public int? EducationLevelId { get; set; }

    /// <summary>
    /// Current job/occupation
    /// </summary>
    public string? Job { get; set; }

    /// <summary>
    /// Monthly income amount
    /// </summary>
    public decimal? MonthlyIncome { get; set; }

    /// <summary>
    /// Foreign key to Health Status lookup
    /// </summary>
    public int? HealthStatusId { get; set; }

    /// <summary>
    /// Phone number
    /// </summary>
    public string? Phone { get; set; }

    /// <summary>
    /// Address (if different from family)
    /// </summary>
    public string? Address { get; set; }

    /// <summary>
    /// Indicates if relative is alive (default: true)
    /// </summary>
    public bool IsAlive { get; set; } = true;

    /// <summary>
    /// Indicates if relative is living with the family (default: false)
    /// </summary>
    public bool IsLivingWithFamily { get; set; } = false;

    /// <summary>
    /// Date of death (if not alive)
    /// </summary>
    public DateTime? DeathDate { get; set; }

    /// <summary>
    /// Additional notes
    /// </summary>
    public string? Notes { get; set; }

    // Navigation Properties
    public virtual Family? Family { get; set; }
    public virtual EducationLevel? EducationLevel { get; set; }
    public virtual HealthStatus? HealthStatus { get; set; }
}
