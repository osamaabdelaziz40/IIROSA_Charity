using System.ComponentModel.DataAnnotations;

namespace IIROSA.Application.DTOs.SeasonalAid;

/// <summary>
/// Create Seasonal Aid Distribution DTO - Used for recording distributions (UC-9.5)
/// </summary>
public class CreateSeasonalAidDistributionDto
{
    [Required(ErrorMessage = "Beneficiary ID is required")]
    public Guid BeneficiaryId { get; set; }

    // Distribution Information (UC-9.5)
    [Required(ErrorMessage = "Distribution date is required")]
    public DateTime DistributionDate { get; set; } = DateTime.UtcNow;

    [Required(ErrorMessage = "Amount distributed is required")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Amount distributed must be greater than zero")]
    public decimal AmountDistributed { get; set; }

    [Required(ErrorMessage = "Currency is required")]
    [StringLength(3, ErrorMessage = "Currency code cannot exceed 3 characters")]
    public string Currency { get; set; } = "EGP";

    // Recipient Information
    [Required(ErrorMessage = "Received by is required")]
    [StringLength(100, ErrorMessage = "Received by name cannot exceed 100 characters")]
    public string ReceivedBy { get; set; } = string.Empty;

    [StringLength(50, ErrorMessage = "Recipient relationship cannot exceed 50 characters")]
    public string? RecipientRelationship { get; set; } // Father, Mother, Guardian, etc.

    [StringLength(1000, ErrorMessage = "Notes cannot exceed 1000 characters")]
    public string? Notes { get; set; }

    // Proof/Documentation
    [StringLength(500, ErrorMessage = "Signature image URL cannot exceed 500 characters")]
    public string? SignatureImageUrl { get; set; }

    public string? AttachmentId { get; set; }

    // Additional Metadata
    [StringLength(50, ErrorMessage = "Distribution method cannot exceed 50 characters")]
    public string? DistributionMethod { get; set; } // Cash, Bank Transfer, In-Kind, etc.

    [StringLength(100, ErrorMessage = "Distributor name cannot exceed 100 characters")]
    public string? DistributorName { get; set; }

    [StringLength(50, ErrorMessage = "Distributor role cannot exceed 50 characters")]
    public string? DistributorRole { get; set; }
}
