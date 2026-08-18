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

    // Audit fields (inherited from FullAuditedEntity)
    public DateTime CreatedOn { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedOn { get; set; }
    public string? UpdatedBy { get; set; }

    // Related entities
    public FatherDto? Father { get; set; }
    public MotherDto? Mother { get; set; }
    public ProviderDto? Provider { get; set; }
    public List<RelativeListDto> Relatives { get; set; } = new();
    public List<OrphanListDto> Orphans { get; set; } = new();
    public int RelativesCount { get; set; }
}
