using IIROSA.Domain.Entities.Base;
using IIROSA.Domain.Entities.Lookups;

namespace IIROSA.Domain.Entities;

/// <summary>
/// Father entity - Represents father of a family
/// Inherits from FullAuditedEntity which provides all audit fields
/// Implements use cases UC-4.2 (Add Family Father) and UC-4.9 (Update Father Details)
/// </summary>
public class Father : FullAuditedEntity
{
    /// <summary>
    /// Foreign key to Family
    /// </summary>
    public Guid? FamilyId { get; set; }

    /// <summary>
    /// Full name of the father (Required)
    /// </summary>
    public string FullName { get; set; } = string.Empty;

    /// <summary>
    /// National ID or Passport number (Required)
    /// </summary>
    public string NationalId { get; set; } = string.Empty;

    /// <summary>
    /// Date of birth
    /// </summary>
    public DateTime? DateOfBirth { get; set; }

    /// <summary>
    /// Place of birth
    /// </summary>
    public string? PlaceOfBirth { get; set; }

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
    /// Indicates if father is alive (default: true)
    /// </summary>
    public bool IsAlive { get; set; } = true;

    /// <summary>
    /// Indicates if father is the family provider (default: false)
    /// </summary>
    public bool IsProvider { get; set; } = false;

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
