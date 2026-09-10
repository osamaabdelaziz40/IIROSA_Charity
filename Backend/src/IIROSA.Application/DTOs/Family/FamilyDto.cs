namespace IIROSA.Application.DTOs.Family;

/// <summary>
/// Family DTO - Detailed family information (UC-4.13: View Family Details)
/// </summary>
public class FamilyDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public DateTime RegistrationDate { get; set; }
    public string HeadOfFamily { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string? Address { get; set; }
    public string? CityVillage { get; set; }
    public string? DistrictArea { get; set; }
    public int? CountryId { get; set; }
    public string? CountryName { get; set; }
    public int? CityId { get; set; }
    public string? CityName { get; set; }
    public Guid? CharityId { get; set; }
    public string? CharityName { get; set; }
    public bool IsActive { get; set; }
    public string? FamilyStatus { get; set; }
    public string? FinancialStatus { get; set; }
    public int? LivingConditionId { get; set; }
    public string? LivingConditionName { get; set; }
    public int? HousingTypeId { get; set; }
    public string? HousingTypeName { get; set; }
    public string? ProviderType { get; set; }
    public int FamilyMembersCount { get; set; }
    public int OrphansCount { get; set; }
    public decimal? MonthlyIncome { get; set; }
    public decimal? MonthlyAssistance { get; set; }
    public string? Notes { get; set; }

    // Refugee register household fields (epic 7, UC-REF-03 §12.S.2) — null on Regular/Housing rows

    /// <summary>
    /// Register discriminator: Regular | Housing | Refugee
    /// </summary>
    public string FamilyType { get; set; } = "Regular";

    public int? RegionId { get; set; }
    public string? RegionName { get; set; }
    public int? CenterId { get; set; }
    public string? CenterName { get; set; }
    public string? NearBy { get; set; }
    public string? Street { get; set; }
    public decimal? RentAmount { get; set; }
    public int? HouseOwnershipId { get; set; }
    public string? HouseOwnershipName { get; set; }
    public int? HouseStatusId { get; set; }
    public string? HouseStatusName { get; set; }
    public int? IncomeTypeId { get; set; }
    public string? IncomeTypeName { get; set; }

    // Family data extension (§4 معلومات الأسرة) — shared across registers
    public decimal? IncomeValue { get; set; }
    public decimal? TotalIncome { get; set; }
    public int? ChildrenCount { get; set; }
    public bool HasProject { get; set; }
    public int? FamilyProjectStatusId { get; set; }
    public string? FamilyProjectStatusName { get; set; }
    /// <summary>Contact numbers (multi phone), default-first.</summary>
    public List<FamilyPhoneDto> Phones { get; set; } = new();

    // Housing register allocation (epic 6, §11.S.2) — null on non-housing rows
    public int? HousingBuildingId { get; set; }
    public string? HousingBuildingName { get; set; }
    public int? HousingFlatId { get; set; }
    public string? HousingFlatName { get; set; }

    /// <summary>
    /// نصيب الفرد — computed read-only: (TotalIncome ?? MonthlyIncome) / (ChildrenCount ??
    /// FamilyMembersCount); not a column. Falls back to the §12.S.2 refugee behaviour when
    /// the family-data extension fields are not set.
    /// </summary>
    public decimal? PerMemberShare
    {
        get
        {
            var income = TotalIncome ?? MonthlyIncome;
            var divisor = ChildrenCount ?? FamilyMembersCount;
            return divisor > 0 && income.HasValue
                ? Math.Round(income.Value / divisor, 2)
                : null;
        }
    }

    // Audit fields (inherited from FullAuditedEntity)
    public DateTime CreatedOn { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedOn { get; set; }
    public string? UpdatedBy { get; set; }

    // Related entities
    public FatherDto? Father { get; set; }
    public MotherDto? Mother { get; set; }
    public ProviderDto? Provider { get; set; }
    /// <summary>§11.S.2 multi-guardian set (اضافة الاباء), primary-first; Provider above
    /// mirrors the first row for the legacy single-seat consumers.</summary>
    public List<ProviderDto> Providers { get; set; } = new();
    public List<RelativeListDto> Relatives { get; set; } = new();
    public List<OrphanListDto> Orphans { get; set; } = new();
    public int RelativesCount { get; set; }
}
