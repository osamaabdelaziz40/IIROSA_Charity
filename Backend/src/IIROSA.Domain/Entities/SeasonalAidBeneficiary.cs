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

    /// <summary>
    /// Main/pending split of the project register (UC-PRJ-06 selection screen):
    /// true = main family (الأسرة الأساسية) added to the project; false = pending
    /// list entry waiting to be confirmed as main. Distinct from IsRegistered,
    /// which the removal flow drives (false + soft delete).
    /// </summary>
    public bool IsMain { get; set; } = true;

    // Distribution Status
    public bool IsDistributed { get; set; } = false;
    public DateTime? DistributionDate { get; set; }

    // Navigation Properties
    public virtual SeasonalAidCampaign Campaign { get; set; } = null!;
    public virtual Family Family { get; set; } = null!;

    public virtual ICollection<SeasonalAidDistribution> Distributions { get; set; } = new List<SeasonalAidDistribution>();
}
