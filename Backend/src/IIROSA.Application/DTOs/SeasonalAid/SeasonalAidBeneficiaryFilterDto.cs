namespace IIROSA.Application.DTOs.SeasonalAid;

/// <summary>
/// Seasonal Aid Beneficiary Filter DTO - Used for filtering beneficiaries (UC-9.7)
/// </summary>
public class SeasonalAidBeneficiaryFilterDto
{
    // Filters
    public bool? IsDistributed { get; set; }
    public Guid? CharityId { get; set; }
    public int? RegionId { get; set; }
    public int? CenterId { get; set; }
    public string? FamilyType { get; set; } // Orphan Families, Needy Families, All

    // Date Range
    public DateTime? RegistrationDateFrom { get; set; }
    public DateTime? RegistrationDateTo { get; set; }
    public DateTime? DistributionDateFrom { get; set; }
    public DateTime? DistributionDateTo { get; set; }

    // Search
    public string? SearchTerm { get; set; } // Search by family code or address

    // Pagination
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;

    // Sorting
    public string? SortBy { get; set; } = "RegistrationDate";
    public bool SortDescending { get; set; } = false;
}
