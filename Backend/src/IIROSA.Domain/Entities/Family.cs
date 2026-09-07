using IIROSA.Domain.Entities.Base;
using IIROSA.Domain.Entities.Lookups;
using IIROSA.Domain.Enums;

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
    /// Register discriminator: Regular (orphan sponsorship), Housing (UC-HOU chapter 11), Refugee (epic 7).
    /// Distinct from the HousingTypeId living-condition lookup above.
    /// </summary>
    public FamilyType FamilyType { get; set; } = FamilyType.Regular;

    /// <summary>
    /// Allocation: the organisation-owned building the housing family lives in (UC-HOU-05)
    /// </summary>
    public int? FK_HousingBuildingId { get; set; }

    /// <summary>
    /// Allocation: the flat inside that building (UC-HOU-05)
    /// </summary>
    public int? FK_HousingFlatId { get; set; }

    /// <summary>
    /// Holding-family marker (5-6/5-7/5-8 ruling 2026-08-24): member-control detach moves
    /// (action 0) auto-create a synthetic family to hold the detached member. This flag makes
    /// them distinguishable from real register families in lists and reports. Deliberately NOT
    /// a FamilyType value — that discriminator belongs to the register contracts (epics 6/7).
    /// </summary>
    public bool IsHoldingFamily { get; set; } = false;

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

    // Refugee register household fields (epic 7, UC-REF-03 §12.S.2) — all nullable:
    // a Regular/Housing family row is untouched; one table, no join.

    /// <summary>
    /// Governorate / region of residence (المنطقة /المحافظة)
    /// </summary>
    public int? RegionId { get; set; }

    /// <summary>
    /// Center / city of residence (المركز/ المدينة) — cascade-loads from the region
    /// </summary>
    public int? CenterId { get; set; }

    /// <summary>
    /// Nearby landmark (بجوار)
    /// </summary>
    public string? NearBy { get; set; }

    /// <summary>
    /// Street (الشارع)
    /// </summary>
    public string? Street { get; set; }

    /// <summary>
    /// Monthly rent amount (قيمة الإيجار) — relevant when the ownership is إيجار
    /// </summary>
    public decimal? RentAmount { get; set; }

    /// <summary>
    /// House ownership (ملكية السكن)
    /// </summary>
    public int? HouseOwnershipId { get; set; }

    /// <summary>
    /// House contents status (حالة محتويات السكن)
    /// </summary>
    public int? HouseStatusId { get; set; }

    /// <summary>
    /// Income type (نوع الدخل)
    /// </summary>
    public int? IncomeTypeId { get; set; }

    // Navigation Properties
    public virtual Country? Country { get; set; }
    public virtual City? City { get; set; }
    public virtual Charity? Charity { get; set; }
    public virtual LivingCondition? LivingCondition { get; set; }
    public virtual HousingType? HousingType { get; set; }
    public virtual Lookups.HousingBuilding? HousingBuilding { get; set; }
    public virtual Lookups.HousingFlat? HousingFlat { get; set; }
    public virtual Father? Father { get; set; }
    public virtual Mother? Mother { get; set; }
    /// <summary>
    /// §11.S.2 multi-guardian set (اضافة الاباء · AddNewParent() always): a housing family
    /// may carry several live guardians. Primary = first by CreatedOn/Id; the legacy
    /// single-seat rule (BR-06) stays enforced in code for the standalone member endpoint.
    /// </summary>
    public virtual ICollection<Provider> Providers { get; set; } = new List<Provider>();
    public virtual ICollection<Relative> Relatives { get; set; } = new List<Relative>();
    public virtual ICollection<Orphan> Orphans { get; set; } = new List<Orphan>();

    // Refugee register navigations (epic 7)
    public virtual Region? Region { get; set; }
    public virtual Center? Center { get; set; }
    public virtual HouseOwnership? HouseOwnership { get; set; }
    public virtual HouseStatus? HouseStatus { get; set; }
    public virtual IncomeType? IncomeType { get; set; }
}
