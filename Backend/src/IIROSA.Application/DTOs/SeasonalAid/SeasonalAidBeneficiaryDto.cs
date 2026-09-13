namespace IIROSA.Application.DTOs.SeasonalAid;

/// <summary>
/// Seasonal Aid Beneficiary DTO - Beneficiary details (UC-9.7)
/// </summary>
public class SeasonalAidBeneficiaryDto
{
    public Guid Id { get; set; }
    public Guid CampaignId { get; set; }
    public Guid FamilyId { get; set; }

    // Family Information
    public string FamilyCode { get; set; } = string.Empty;
    public string? FamilyAddress { get; set; }
    public int OrphansCount { get; set; }
    public int FamilyMembersCount { get; set; }
    public string? CharityName { get; set; }
    public string? RegionName { get; set; }
    public string? CenterName { get; set; }

    // Allocation Information
    public decimal AllocationAmount { get; set; }
    public string Currency { get; set; } = "EGP";

    // Registration Information
    public bool IsRegistered { get; set; }
    public DateTime RegistrationDate { get; set; }
    public string? RegistrationNotes { get; set; }

    /// <summary>Main family (true) or pending-list entry (false) — UC-PRJ-06 selection screen split.</summary>
    public bool IsMain { get; set; }

    // Distribution Status (UC-9.7)
    public bool IsDistributed { get; set; }
    public DateTime? DistributionDate { get; set; }
    public decimal DistributedAmount { get; set; }

    // Latest Distribution Details
    public string? ReceivedBy { get; set; }
    public string? Notes { get; set; }

    // Audit
    public DateTime CreatedOn { get; set; }
}
