namespace IIROSA.Application.DTOs.OrphanPayment;

/// <summary>
/// OrphanPaymentItem DTO - Represents an orphan within a payment group
/// </summary>
public class OrphanPaymentItemDto
{
    public Guid Id { get; set; }
    public Guid OrphanPaymentId { get; set; }
    public Guid OrphanId { get; set; }
    public int DisplayOrder { get; set; }
    public string? Notes { get; set; }

    // Orphan Details
    public string OrphanCode { get; set; } = string.Empty;
    public string OrphanFullName { get; set; } = string.Empty;
    public string? OrphanFamilyName { get; set; }
    public int? OrphanAge { get; set; }
    public string? OrphanGender { get; set; }
    public string? OrphanEducationLevel { get; set; }
    public decimal? OrphanMonthlyAmount { get; set; }

    // Charity Details
    public int? CharityId { get; set; }
    public string? CharityName { get; set; }

    // Location Details
    public int? RegionId { get; set; }
    public string? RegionName { get; set; }
    public int? CenterId { get; set; }
    public string? CenterName { get; set; }

    // Sponsorship Details
    public Guid? SponsorId { get; set; }
    public string? SponsorName { get; set; }
    public DateTime? SponsorshipStartDate { get; set; }
    public string? SponsorshipStatus { get; set; }
}
