using IIROSA.Domain.Entities.Base;

namespace IIROSA.Domain.Entities;

/// <summary>
/// Provider entity - Represents non-parent guardian/provider for a family
/// Inherits from FullAuditedEntity which provides all audit fields
/// Implements use case UC-4.6 (Add Non-Parent Provider)
/// </summary>
public class Provider : FullAuditedEntity
{
    /// <summary>
    /// Foreign key to Family
    /// </summary>
    public Guid? FamilyId { get; set; }

    /// <summary>
    /// Full name of the provider (Required)
    /// </summary>
    public string FullName { get; set; } = string.Empty;

    /// <summary>
    /// Relationship to family: Grandfather, Uncle, Brother, Guardian, Other, etc.
    /// </summary>
    public string RelationshipToFamily { get; set; } = string.Empty;

    /// <summary>
    /// National ID or Passport number (Required)
    /// </summary>
    public string NationalId { get; set; } = string.Empty;

    /// <summary>
    /// Phone number (Required)
    /// </summary>
    public string Phone { get; set; } = string.Empty;

    /// <summary>
    /// Address of the provider
    /// </summary>
    public string? Address { get; set; }

    /// <summary>
    /// Current job/occupation
    /// </summary>
    public string? Job { get; set; }

    /// <summary>
    /// Monthly income amount
    /// </summary>
    public decimal? MonthlyIncome { get; set; }

    /// <summary>
    /// Additional notes
    /// </summary>
    public string? Notes { get; set; }

    // Navigation Properties
    public virtual Family? Family { get; set; }
}
