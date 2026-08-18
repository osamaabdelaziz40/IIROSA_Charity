using IIROSA.Application.DTOs.SeasonalAid;

namespace IIROSA.Application.Interfaces;

/// <summary>
/// Seasonal Aid Service Interface
/// Implements all use cases UC-9.1 through UC-9.11
/// </summary>
public interface ISeasonalAidService
{
    // UC-9.1: Create Seasonal Aid Campaign
    Task<SeasonalAidCampaignDto> CreateCampaignAsync(CreateSeasonalAidCampaignDto dto);

    // UC-9.2: Set Campaign Period
    Task SetCampaignPeriodAsync(Guid campaignId, DateTime startDate, DateTime endDate);

    // UC-9.3: Allocate Campaign Budget
    Task SetCampaignBudgetAsync(Guid campaignId, decimal totalBudget, string currency, decimal perFamilyAllocation);

    // UC-9.4: Register Beneficiary for Aid
    Task<(int RegisteredCount, decimal TotalAllocation, decimal BudgetImpact)> RegisterBeneficiariesAsync(CreateSeasonalAidBeneficiaryDto dto);
    Task<(IEnumerable<SeasonalAidBeneficiaryDto> Items, int TotalCount)> GetEligibleFamiliesAsync(EligibleFamiliesFilterDto filter);
    Task RemoveBeneficiaryAsync(Guid beneficiaryId);

    // UC-9.5: Record Aid Distribution
    Task<SeasonalAidDistributionDto> RecordDistributionAsync(CreateSeasonalAidDistributionDto dto);
    Task RecordDistributionsAsync(List<CreateSeasonalAidDistributionDto> distributions);

    // UC-9.6: View Campaign List
    Task<(IEnumerable<SeasonalAidCampaignListDto> Items, int TotalCount)> GetCampaignsAsync(SeasonalAidCampaignFilterDto filter);
    Task<IEnumerable<SeasonalAidCampaignListDto>> GetActiveCampaignsAsync();

    // UC-9.7: View Campaign Beneficiaries
    Task<(IEnumerable<SeasonalAidBeneficiaryDto> Items, int TotalCount)> GetBeneficiariesAsync(
        Guid campaignId, SeasonalAidBeneficiaryFilterDto filter);
    Task<SeasonalAidBeneficiaryDto?> GetBeneficiaryAsync(Guid beneficiaryId);

    // UC-9.8: Update Campaign Details
    Task<SeasonalAidCampaignDto> UpdateCampaignAsync(UpdateSeasonalAidCampaignDto dto);

    // UC-9.9: Close Campaign
    Task CloseCampaignAsync(CloseCampaignDto dto);
    Task ReopenCampaignAsync(Guid campaignId);

    // UC-9.10: Generate Campaign Report
    Task<SeasonalAidCampaignReportDto> GenerateCampaignReportAsync(Guid campaignId);
    Task<byte[]> ExportCampaignReportToPdfAsync(Guid campaignId);
    Task<byte[]> ExportCampaignReportToExcelAsync(Guid campaignId);

    // UC-9.11: Assign Campaign to Charity
    Task AssignCampaignToCharityAsync(Guid campaignId, Guid? charityId);

    // Additional helper methods
    Task<SeasonalAidCampaignDto?> GetCampaignByIdAsync(Guid id);
    Task<SeasonalAidCampaignDto?> GetCampaignByNameAsync(string name);
    Task<bool> IsCampaignNameUniqueAsync(string name, Guid? excludeId = null);
    Task<bool> IsCampaignActiveAsync(Guid campaignId);
    Task<decimal> GetCampaignRemainingBudgetAsync(Guid campaignId);
    Task<int> GetCampaignAvailableSlotsAsync(Guid campaignId);
    Task DeleteCampaignAsync(Guid id);
}
