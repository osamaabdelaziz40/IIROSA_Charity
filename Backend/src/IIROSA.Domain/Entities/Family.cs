using IIROSA.Domain.Entities.Base;
using IIROSA.Domain.Entities.Lookups;

namespace IIROSA.Domain.Entities;

/// <summary>
/// Family entity - Inherits from FullAuditedEntityBase<Guid>
/// Implements all use cases UC-4.1 through UC-4.15
/// All audit fields (CreatedOn, UpdatedOn, CreatedBy, UpdatedBy, DeletedOn, DeletedBy, IsDeleted) are inherited
/// </summary>
public class Family : FullAuditedEntity
{
    /// <summary>
    /// Family code (auto-generated or manual)
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Registration date (default: today)
    /// </summary>
    public DateTime RegistrationDate { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Head of family name
    /// </summary>
    public string HeadOfFamily { get; set; } = string.Empty;

    /// <summary>
    /// Phone number
    /// </summary>
    public string? PhoneNumber { get; set; }

    /// <summary>
    /// Address (required)
    /// </summary>
    public string? Address { get; set; }

    /// <summary>
    /// City or Village name
    /// </summary>
    public string? CityVillage { get; set; }

    /// <summary>
    /// District or Area
    /// </summary>
    public string? DistrictArea { get; set; }

    /// <summary>
    /// Foreign key to Country lookup
    /// </summary>
    public int? CountryId { get; set; }

    /// <summary>
    /// Foreign key to City/Region lookup
    /// </summary>
    public int? CityId { get; set; }

    /// <summary>
    /// Foreign key to Charity (data isolation)
    /// </summary>
    public Guid? CharityId { get; set; }

    /// <summary>
    /// Alternative foreign key to Charity
    /// </summary>
    public Guid? FK_CharityId { get; set; }

    /// <summary>
    /// Family active status
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Family status
    /// </summary>
    public string? FamilyStatus { get; set; }

    /// <summary>
    /// Financial status
    /// </summary>
    public string? FinancialStatus { get; set; }

    /// <summary>
    /// Foreign key to Living Condition lookup
    /// </summary>
    public int? LivingConditionId { get; set; }

    /// <summary>
    /// Foreign key to Housing Type lookup
    /// </summary>
    public int? HousingTypeId { get; set; }

    /// <summary>
    /// Provider type: Father, Mother, Other
    /// </summary>
    public string? ProviderType { get; set; }

    /// <summary>
    /// Total family members count
    /// </summary>
    public int FamilyMembersCount { get; set; }

    /// <summary>
    /// Orphans count in family
    /// </summary>
    public int OrphansCount { get; set; }

    /// <summary>
    /// Relatives count in family
    /// </summary>
    public int RelativesCount { get; set; }

    /// <summary>
    /// Monthly income
    /// </summary>
    public decimal? MonthlyIncome { get; set; }

    /// <summary>
    /// Monthly assistance amount
    /// </summary>
    public decimal? MonthlyAssistance { get; set; }

    /// <summary>
    /// Additional notes
    /// </summary>
    public string? Notes { get; set; }

    // Navigation Properties
    public virtual Country? Country { get; set; }
    public virtual City? City { get; set; }
    public virtual Charity? Charity { get; set; }
    public virtual LivingCondition? LivingCondition { get; set; }
    public virtual HousingType? HousingType { get; set; }
    public virtual Father? Father { get; set; }
    public virtual Mother? Mother { get; set; }
    public virtual Provider? Provider { get; set; }
    public virtual ICollection<Relative> Relatives { get; set; } = new List<Relative>();
    public virtual ICollection<Orphan> Orphans { get; set; } = new List<Orphan>();
}
