namespace IIROSA.Application.DTOs.SeasonalAid;

/// <summary>
/// Seasonal Aid Distribution DTO - Distribution record details
/// </summary>
public class SeasonalAidDistributionDto
{
    public Guid Id { get; set; }
    public Guid BeneficiaryId { get; set; }

    // Family and Campaign Information
    public string FamilyCode { get; set; } = string.Empty;
    public string CampaignName { get; set; } = string.Empty;

    // Distribution Information
    public bool IsDistributed { get; set; }
    public DateTime DistributionDate { get; set; }
    public decimal AmountDistributed { get; set; }
    public string Currency { get; set; } = "EGP";

    // Recipient Information
    public string? ReceivedBy { get; set; }
    public string? RecipientRelationship { get; set; }
    public string? Notes { get; set; }

    // Proof/Documentation
    public string? SignatureImageUrl { get; set; }
    public string? AttachmentId { get; set; }

    // Additional Metadata
    public string? DistributionMethod { get; set; }
    public string? DistributorName { get; set; }
    public string? DistributorRole { get; set; }

    // Audit
    public DateTime CreatedOn { get; set; }
    public string? CreatedBy { get; set; }
}
