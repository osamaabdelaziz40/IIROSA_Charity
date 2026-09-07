using IIROSA.Domain.Entities.Base;
using IIROSA.Domain.Entities.Lookups;

namespace IIROSA.Domain.Entities;

/// <summary>
/// Seasonal Aid Campaign entity - Implements all use cases UC-9.1 through UC-9.11
/// Inherits from FullAuditedEntityBase<Guid> with all audit fields
/// </summary>
public class SeasonalAidCampaign : FullAuditedEntity
{
    // Basic Information (UC-9.1)
    public string Name { get; set; } = string.Empty;
    public string CampaignType { get; set; } = string.Empty; // Ramadan, Eid Al-Fitr, Eid Al-Adha, Winter, School Supplies, Other
    public string? Description { get; set; }

    // Campaign Period (UC-9.2)
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }

    // Financial Information (UC-9.3)
    public decimal TotalBudget { get; set; }
    public string BudgetCurrency { get; set; } = "EGP"; // EGP, SAR, USD
    public decimal PerFamilyAllocation { get; set; }

    // Geographic Scope
    public int? CountryId { get; set; }
    public int? RegionId { get; set; }
    public int? CenterId { get; set; }

    // Beneficiary Criteria (UC-9.1)
    public int? MaximumFamilies { get; set; }
    public string? FamilyType { get; set; } // All, Orphan Families, Needy Families
    public int? MinChildrenAge { get; set; }
    public int? MaxChildrenAge { get; set; }

    // Status
    public bool IsActive { get; set; } = true;
    public bool IsClosed { get; set; } = false;
    public DateTime? ClosedDate { get; set; }
    public string? ClosureNotes { get; set; }

    // Calculated fields (not stored in database)
    [System.Text.Json.Serialization.JsonIgnore]
    public decimal AllocatedBudget => Beneficiaries.Sum(b => b.AllocationAmount);

    [System.Text.Json.Serialization.JsonIgnore]
    public decimal DistributedBudget => Beneficiaries
        .SelectMany(b => b.Distributions)
        .Sum(d => d.AmountDistributed);

    [System.Text.Json.Serialization.JsonIgnore]
    public int RegisteredBeneficiariesCount => Beneficiaries.Count;

    [System.Text.Json.Serialization.JsonIgnore]
    public int DistributedBeneficiariesCount => Beneficiaries
        .Count(b => b.Distributions.Any(d => d.IsDistributed));

    // Navigation Properties
    public virtual Country? Country { get; set; }
    public virtual Region? Region { get; set; }
    public virtual Center? Center { get; set; }

    public virtual ICollection<SeasonalAidBeneficiary> Beneficiaries { get; set; } = new List<SeasonalAidBeneficiary>();
}
