namespace IIROSA.Application.DTOs.OrphanPayment;

/// <summary>
/// Orphan Filter for Payment DTO - Used for filtering orphans to add to payment groups (UC-5.3)
/// </summary>
public class OrphanFilterForPaymentDto
{
    /// <summary>
    /// Payment group ID (to exclude orphans already in the group)
    /// </summary>
    public Guid? OrphanPaymentId { get; set; }

    /// <summary>
    /// Search term (searches in orphan name and family name)
    /// </summary>
    public string? SearchTerm { get; set; }

    /// <summary>
    /// Charity filter
    /// </summary>
    public int? CharityId { get; set; }

    /// <summary>
    /// Region filter
    /// </summary>
    public int? RegionId { get; set; }

    /// <summary>
    /// Center filter
    /// </summary>
    public int? CenterId { get; set; }

    /// <summary>
    /// Sponsorship status filter (Sponsored, Unsponsored, All)
    /// </summary>
    public string? SponsorshipStatus { get; set; }

    /// <summary>
    /// Age range filter - from
    /// </summary>
    public int? AgeFrom { get; set; }

    /// <summary>
    /// Age range filter - to
    /// </summary>
    public int? AgeTo { get; set; }

    /// <summary>
    /// Gender filter (Male, Female, All)
    /// </summary>
    public string? Gender { get; set; }

    // Pagination
    /// <summary>
    /// Page number (1-based)
    /// </summary>
    public int PageNumber { get; set; } = 1;

    /// <summary>
    /// Page size
    /// </summary>
    public int PageSize { get; set; } = 50;
}
