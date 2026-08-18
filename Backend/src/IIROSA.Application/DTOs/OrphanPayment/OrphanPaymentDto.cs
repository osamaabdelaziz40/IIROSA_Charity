namespace IIROSA.Application.DTOs.OrphanPayment;

/// <summary>
/// OrphanPayment DTO - Full payment group details
/// Used for displaying complete payment group information
/// </summary>
public class OrphanPaymentDto
{
    public Guid Id { get; set; }
    public string GroupName { get; set; } = string.Empty;
    public string? Description { get; set; }

    // Payment Period
    public DateTime PaymentPeriodFrom { get; set; }
    public DateTime PaymentPeriodTo { get; set; }
    public DateTime GroupDate { get; set; }

    // Financial Information
    public decimal? ExchangeRate { get; set; }
    public string? Currency { get; set; }
    public bool DontRemoveRate { get; set; }

    // Batch Information
    public string? BatchNo { get; set; }
    public int ShowOrder { get; set; }

    // Status
    public bool IsBatchUploaded { get; set; }
    public DateTime? UploadDate { get; set; }

    // Notes
    public string? Notes { get; set; }

    // Audit
    public DateTime CreatedOn { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime UpdatedOn { get; set; }
    public string? UpdatedBy { get; set; }

    // Statistics
    public int OrphanCount { get; set; }
    public Dictionary<Guid, int> OrphanCountByCharity { get; set; } = new();
    public Dictionary<int, int> OrphanCountByRegion { get; set; } = new();

    // Orphans in this group
    public List<OrphanPaymentItemDto> Orphans { get; set; } = new();
}
