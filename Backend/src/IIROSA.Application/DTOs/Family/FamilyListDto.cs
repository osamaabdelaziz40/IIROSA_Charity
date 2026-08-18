namespace IIROSA.Application.DTOs.Family;

/// <summary>
/// Family List DTO - Summary view for family list (UC-4.12: View Family List)
/// </summary>
public class FamilyListDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string? CityVillage { get; set; }
    public string? FatherName { get; set; }
    public string? MotherName { get; set; }
    public int OrphansCount { get; set; }
    public int RelativesCount { get; set; }
    public string? ProviderType { get; set; }
    public DateTime RegistrationDate { get; set; }
    public bool IsActive { get; set; }
    public string? CharityName { get; set; }
}
