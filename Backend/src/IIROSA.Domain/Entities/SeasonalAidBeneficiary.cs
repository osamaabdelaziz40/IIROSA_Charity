using IIROSA.Domain.Entities.Base;

namespace IIROSA.Domain.Entities;

/// <summary>
/// Seasonal Aid Beneficiary entity - Represents a family registered for a seasonal aid campaign
/// Junction table between SeasonalAidCampaign and Family with additional distribution information
/// </summary>
public class SeasonalAidBeneficiary : FullAuditedEntity
{
    // Foreign Keys
    public Guid CampaignId { get; set; }
    public Guid FamilyId { get; set; }

    // Allocation Information
    public decimal AllocationAmount { get; set; }
    public string? Currency { get; set; } = "EGP";

    // Status
    public bool IsRegistered { get; set; } = true;
    public DateTime RegistrationDate { get; set; } = DateTime.UtcNow;
    public string? RegistrationNotes { get; set; }

    // Distribution Status
    public bool IsDistributed { get; set; } = false;
    public DateTime? DistributionDate { get; set; }

    // Navigation Properties
    public virtual SeasonalAidCampaign Campaign { get; set; } = null!;
    public virtual Family Family { get; set; } = null!;

    public virtual ICollection<SeasonalAidDistribution> Distributions { get; set; } = new List<SeasonalAidDistribution>();
}
