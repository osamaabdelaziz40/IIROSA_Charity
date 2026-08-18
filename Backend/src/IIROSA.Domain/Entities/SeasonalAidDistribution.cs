using IIROSA.Domain.Entities.Base;

namespace IIROSA.Domain.Entities;

/// <summary>
/// Seasonal Aid Distribution entity - Records actual distribution of aid to beneficiaries
/// Implements use case UC-9.5: Record Aid Distribution
/// </summary>
public class SeasonalAidDistribution : FullAuditedEntity
{
    // Foreign Keys
    public Guid BeneficiaryId { get; set; }

    // Distribution Information (UC-9.5)
    public bool IsDistributed { get; set; } = false;
    public DateTime DistributionDate { get; set; }
    public decimal AmountDistributed { get; set; }
    public string Currency { get; set; } = "EGP";

    // Recipient Information
    public string? ReceivedBy { get; set; } // Person name who received the aid
    public string? RecipientRelationship { get; set; } // Relationship to family head
    public string? Notes { get; set; }

    // Proof/Documentation
    public string? SignatureImageUrl { get; set; }
    public string? AttachmentId { get; set; } // Reference to Framework.Core Attachment entity

    // Additional Metadata
    public string? DistributionMethod { get; set; } // Cash, Bank Transfer, In-Kind, etc.
    public string? DistributorName { get; set; } // Name of person who distributed the aid
    public string? DistributorRole { get; set; } // Role of the distributor

    // Navigation Properties
    public virtual SeasonalAidBeneficiary Beneficiary { get; set; } = null!;
}
