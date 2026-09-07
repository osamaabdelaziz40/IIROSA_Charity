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

    // §15.1 row ledger (EP-10) — snapshot taken at enrolment
    public decimal? Amount { get; set; }
    public bool IsStopped { get; set; }
    public DateTime? StoppedOn { get; set; }
    public bool IsPrinted { get; set; }
    public DateTime? PrintedOn { get; set; }
    public bool IsGotIt { get; set; }
    public DateTime? ReceivedOn { get; set; }
    public string? ChiqueNum { get; set; }
    public DateTime? Printdate { get; set; }
    public string? BenificiaryName { get; set; }
    public string? TransferNo { get; set; }
    public int? ExchangeStatus { get; set; }

    // Orphan Details
    public string OrphanCode { get; set; } = string.Empty;
    public string OrphanFullName { get; set; } = string.Empty;
    public string? OrphanFamilyName { get; set; }
    public int? OrphanAge { get; set; }
    public string? OrphanGender { get; set; }
    public string? OrphanEducationLevel { get; set; }
    public decimal? OrphanMonthlyAmount { get; set; }

    // Charity Details
    public Guid? CharityId { get; set; }
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
