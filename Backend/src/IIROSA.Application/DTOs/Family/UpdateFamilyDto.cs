using System.ComponentModel.DataAnnotations;

namespace IIROSA.Application.DTOs.Family;

/// <summary>
/// Update Family DTO - Used for updating family information (UC-4.8: Update Family Information)
/// </summary>
public class UpdateFamilyDto
{
    [Required]
    public Guid Id { get; set; }

    // Family Information
    /// <summary>
    /// Family code
    /// </summary>
    [StringLength(50, ErrorMessage = "Code cannot exceed 50 characters")]
    public string? Code { get; set; }

    /// <summary>
    /// Registration date
    /// </summary>
    public DateTime? RegistrationDate { get; set; }

    /// <summary>
    /// Head of family name
    /// </summary>
    [StringLength(200, ErrorMessage = "Head of family name cannot exceed 200 characters")]
    public string? HeadOfFamily { get; set; }

    /// <summary>
    /// Address
    /// </summary>
    [StringLength(500, ErrorMessage = "Address cannot exceed 500 characters")]
    public string? Address { get; set; }

    /// <summary>
    /// City or Village
    /// </summary>
    [StringLength(200, ErrorMessage = "City/Village cannot exceed 200 characters")]
    public string? CityVillage { get; set; }

    /// <summary>
    /// District or Area
    /// </summary>
    [StringLength(200, ErrorMessage = "District/Area cannot exceed 200 characters")]
    public string? DistrictArea { get; set; }

    /// <summary>
    /// Phone number
    /// </summary>
    [StringLength(20, ErrorMessage = "Phone cannot exceed 20 characters")]
    public string? PhoneNumber { get; set; }

    /// <summary>
    /// Country ID
    /// </summary>
    public int? CountryId { get; set; }

    /// <summary>
    /// City ID
    /// </summary>
    public int? CityId { get; set; }

    /// <summary>
    /// Living Condition ID
    /// </summary>
    public int? LivingConditionId { get; set; }

    /// <summary>
    /// Housing Type ID
    /// </summary>
    public int? HousingTypeId { get; set; }

    /// <summary>
    /// Provider Type
    /// </summary>
    [StringLength(50, ErrorMessage = "Provider type cannot exceed 50 characters")]
    public string? ProviderType { get; set; }

    /// <summary>
    /// Notes
    /// </summary>
    [StringLength(2000, ErrorMessage = "Notes cannot exceed 2000 characters")]
    public string? Notes { get; set; }

    /// <summary>
    /// Family status
    /// </summary>
    [StringLength(100, ErrorMessage = "Family status cannot exceed 100 characters")]
    public string? FamilyStatus { get; set; }

    /// <summary>
    /// Financial status
    /// </summary>
    [StringLength(100, ErrorMessage = "Financial status cannot exceed 100 characters")]
    public string? FinancialStatus { get; set; }

    /// <summary>
    /// Monthly income
    /// </summary>
    public decimal? MonthlyIncome { get; set; }

    /// <summary>
    /// Monthly assistance
    /// </summary>
    public decimal? MonthlyAssistance { get; set; }
}
