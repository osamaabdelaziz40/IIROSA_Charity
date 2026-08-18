namespace IIROSA.Application.DTOs.OrphanPayment;

/// <summary>
/// OrphanPayment Filter DTO - Used for filtering payment groups (UC-5.8, UC-5.12, UC-5.13)
/// </summary>
public class OrphanPaymentFilterDto
{
    /// <summary>
    /// Search term (searches in GroupName and BatchNo)
    /// </summary>
    public string? SearchTerm { get; set; }

    // Payment Period Filters (UC-5.13)
    /// <summary>
    /// Filter by payment period start date from
    /// </summary>
    public DateTime? PaymentPeriodFrom { get; set; }

    /// <summary>
    /// Filter by payment period end date to
    /// </summary>
    public DateTime? PaymentPeriodTo { get; set; }

    // Group Date Filters (UC-5.13)
    /// <summary>
    /// Filter by group date from
    /// </summary>
    public DateTime? GroupDateFrom { get; set; }

    /// <summary>
    /// Filter by group date to
    /// </summary>
    public DateTime? GroupDateTo { get; set; }

    /// <summary>
    /// Filter by uploaded status (UC-5.8)
    /// </summary>
    public bool? IsBatchUploaded { get; set; }

    /// <summary>
    /// Filter by charity (UC-5.12)
    /// </summary>
    public int? CharityId { get; set; }

    // Pagination
    /// <summary>
    /// Page number (1-based)
    /// </summary>
    public int PageNumber { get; set; } = 1;

    /// <summary>
    /// Page size
    /// </summary>
    public int PageSize { get; set; } = 10;

    // Sorting
    /// <summary>
    /// Sort by field (GroupName, BatchNo, PaymentPeriodFrom, PaymentPeriodTo, GroupDate, OrphanCount)
    /// </summary>
    public string? SortBy { get; set; } = "GroupDate";

    /// <summary>
    /// Sort descending
    /// </summary>
    public bool SortDescending { get; set; } = true;
}
