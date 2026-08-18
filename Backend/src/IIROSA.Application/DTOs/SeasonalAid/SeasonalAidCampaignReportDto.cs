namespace IIROSA.Application.DTOs.SeasonalAid;

/// <summary>
/// Seasonal Aid Campaign Report DTO - Campaign report data (UC-9.10)
/// </summary>
public class SeasonalAidCampaignReportDto
{
    // Campaign Information
    public Guid CampaignId { get; set; }
    public string CampaignName { get; set; } = string.Empty;
    public string CampaignType { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string? Description { get; set; }

    // Financial Summary
    public decimal TotalBudget { get; set; }
    public string BudgetCurrency { get; set; } = "EGP";
    public decimal AllocatedBudget { get; set; }
    public decimal DistributedBudget { get; set; }
    public decimal RemainingBudget { get; set; }
    public double BudgetUtilizationPercentage => TotalBudget > 0 ? (double)(DistributedBudget * 100m / TotalBudget) : 0;

    // Beneficiary Summary
    public int TotalBeneficiaries { get; set; }
    public int DistributedBeneficiaries { get; set; }
    public int PendingBeneficiaries { get; set; }
    public double BeneficiaryDistributionPercentage => TotalBeneficiaries > 0
        ? (DistributedBeneficiaries * 100.0 / TotalBeneficiaries)
        : 0;

    // Beneficiary Breakdown by Region
    public Dictionary<string, int> BeneficiariesByRegion { get; set; } = new();

    // Beneficiary Breakdown by Charity
    public Dictionary<string, int> BeneficiariesByCharity { get; set; } = new();

    // Beneficiary Breakdown by Family Type
    public Dictionary<string, int> BeneficiariesByFamilyType { get; set; } = new();

    // Geographic Coverage
    public List<string> CoveredCountries { get; set; } = new();
    public List<string> CoveredRegions { get; set; } = new();
    public List<string> CoveredCenters { get; set; } = new();

    // Impact Metrics
    public int EstimatedIndividualsServed { get; set; }
    public int TotalOrphansServed { get; set; }
    public int TotalFamiliesServed { get; set; }

    // Distribution Details
    public List<BeneficiaryDistributionDetail> DistributionDetails { get; set; } = new();

    // Campaign Status
    public bool IsActive { get; set; }
    public bool IsClosed { get; set; }
    public DateTime? ClosedDate { get; set; }
    public string? ClosureNotes { get; set; }

    // Report Metadata
    public DateTime ReportGeneratedOn { get; set; } = DateTime.UtcNow;
    public string GeneratedBy { get; set; } = string.Empty;
}

/// <summary>
/// Beneficiary Distribution Detail - Individual beneficiary distribution info
/// </summary>
public class BeneficiaryDistributionDetail
{
    public Guid BeneficiaryId { get; set; }
    public string FamilyCode { get; set; } = string.Empty;
    public string? FamilyAddress { get; set; }
    public string? CharityName { get; set; }
    public string? RegionName { get; set; }
    public decimal AllocationAmount { get; set; }
    public decimal DistributedAmount { get; set; }
    public bool IsDistributed { get; set; }
    public DateTime? DistributionDate { get; set; }
    public string? ReceivedBy { get; set; }
    public string? Notes { get; set; }
}
