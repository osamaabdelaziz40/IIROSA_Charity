namespace IIROSA.Application.DTOs.Family;

/// <summary>
/// Family Filter DTO - Used for filtering and searching families (UC-4.12: View Family List)
/// </summary>
public class FamilyFilterDto
{
    /// <summary>
    /// Search term - searches in Code, Address, Father Name, Mother Name
    /// </summary>
    public string? SearchTerm { get; set; }

    /// <summary>
    /// Filter by Charity ID (for Admin/SuperAdmin)
    /// </summary>
    public Guid? CharityId { get; set; }

    /// <summary>
    /// Filter by Country ID
    /// </summary>
    public int? CountryId { get; set; }

    /// <summary>
    /// Filter by Region/City ID
    /// </summary>
    public int? CityId { get; set; }

    /// <summary>
    /// Filter by Provider Type
    /// </summary>
    public string? ProviderType { get; set; }

    /// <summary>
    /// Filter by Orphan Count range (minimum)
    /// </summary>
    public int? OrphanCountFrom { get; set; }

    /// <summary>
    /// Filter by Orphan Count range (maximum)
    /// </summary>
    public int? OrphanCountTo { get; set; }

    /// <summary>
    /// Filter by Registration Date (from)
    /// </summary>
    public DateTime? RegistrationDateFrom { get; set; }

    /// <summary>
    /// Filter by Registration Date (to)
    /// </summary>
    public DateTime? RegistrationDateTo { get; set; }

    /// <summary>
    /// Filter by Active status
    /// </summary>
    public bool? IsActive { get; set; }

    /// <summary>
    /// Page number (default: 1)
    /// </summary>
    public int PageNumber { get; set; } = 1;

    /// <summary>
    /// Page size (default: 10)
    /// </summary>
    public int PageSize { get; set; } = 10;

    /// <summary>
    /// Sort by field (default: Code)
    /// </summary>
    public string? SortBy { get; set; } = "Code";

    /// <summary>
    /// Sort descending (default: false)
    /// </summary>
    public bool SortDescending { get; set; } = false;
}
