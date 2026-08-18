namespace IIROSA.Application.DTOs.OrphanPayment;

/// <summary>
/// Orphan For Payment List DTO - Used in orphan selection list (UC-5.3)
/// </summary>
public class OrphanForPaymentListDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? FamilyName { get; set; }
    public int? Age { get; set; }
    public string? Gender { get; set; }
    public string? SponsorshipStatus { get; set; }

    // Charity Information
    public int? CharityId { get; set; }
    public string? CharityName { get; set; }

    // Location Information
    public int? RegionId { get; set; }
    public string? RegionName { get; set; }
    public int? CenterId { get; set; }
    public string? CenterName { get; set; }

    /// <summary>
    /// Indicates if orphan is already selected in the group
    /// </summary>
    public bool IsInGroup { get; set; }

    /// <summary>
    /// The orphan payment item ID if already in group
    /// </summary>
    public Guid? OrphanPaymentItemId { get; set; }
}
