namespace IIROSA.Application.DTOs.SeasonalAid;

/// <summary>
/// Seasonal Aid Campaign List DTO - Summary for list views (UC-9.6)
/// </summary>
public class SeasonalAidCampaignListDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string CampaignType { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }

    // Financial Summary
    public decimal TotalBudget { get; set; }
    public string BudgetCurrency { get; set; } = "EGP";
    public decimal AllocatedBudget { get; set; }
    public decimal DistributedBudget { get; set; }

    // Beneficiary Summary
    public int RegisteredBeneficiariesCount { get; set; }
    public int DistributedBeneficiariesCount { get; set; }

    // Status
    public bool IsActive { get; set; }
    public bool IsClosed { get; set; }
    public string? CountryName { get; set; }

    // Progress
    public double CompletionPercentage => RegisteredBeneficiariesCount > 0
        ? (DistributedBeneficiariesCount * 100.0 / RegisteredBeneficiariesCount)
        : 0;
}
