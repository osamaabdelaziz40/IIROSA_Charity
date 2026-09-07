namespace IIROSA.Application.DTOs.SeasonalAid;

/// <summary>
/// Seasonal Aid Campaign DTO - Full campaign details
/// </summary>
public class SeasonalAidCampaignDto
{
    public Guid Id { get; set; }

    // Basic Information
    public string Name { get; set; } = string.Empty;
    public string CampaignType { get; set; } = string.Empty;
    public string? Description { get; set; }

    // Campaign Period
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }

    // Financial Information
    public decimal TotalBudget { get; set; }
    public string BudgetCurrency { get; set; } = "EGP";
    public decimal PerFamilyAllocation { get; set; }

    // Calculated financial fields
    public decimal AllocatedBudget { get; set; }
    public decimal DistributedBudget { get; set; }
    public decimal RemainingBudget => TotalBudget - AllocatedBudget;

    // Geographic Scope
    public int? CountryId { get; set; }
    public string? CountryName { get; set; }
    public int? RegionId { get; set; }
    public string? RegionName { get; set; }
    public int? CenterId { get; set; }
    public string? CenterName { get; set; }

    // Beneficiary Criteria
    public int? MaximumFamilies { get; set; }
    public string? FamilyType { get; set; }
    public int? MinChildrenAge { get; set; }
    public int? MaxChildrenAge { get; set; }

    // Beneficiary counts
    public int RegisteredBeneficiariesCount { get; set; }
    public int DistributedBeneficiariesCount { get; set; }
    public int PendingBeneficiariesCount => RegisteredBeneficiariesCount - DistributedBeneficiariesCount;

    // Status
    public bool IsActive { get; set; }
    public bool IsClosed { get; set; }
    public DateTime? ClosedDate { get; set; }
    public string? ClosureNotes { get; set; }

    // Audit fields (inherited from FullAuditedEntity)
    public DateTime CreatedOn { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime UpdatedOn { get; set; }
    public string? UpdatedBy { get; set; }
}
