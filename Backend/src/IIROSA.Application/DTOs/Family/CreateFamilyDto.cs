using System.ComponentModel.DataAnnotations;

namespace IIROSA.Application.DTOs.Family;

/// <summary>
/// Create Family DTO - Used for creating new family (UC-4.1: Register Family)
/// </summary>
public class CreateFamilyDto
{
    // Family Information
    /// <summary>
    /// Family code (optional, auto-generated if not provided)
    /// </summary>
    [StringLength(50, ErrorMessage = "Code cannot exceed 50 characters")]
    public string? Code { get; set; }

    /// <summary>
    /// Registration date (default: today)
    /// </summary>
    public DateTime? RegistrationDate { get; set; }

    /// <summary>
    /// Head of family name
    /// </summary>
    [StringLength(200, ErrorMessage = "Head of family name cannot exceed 200 characters")]
    public string HeadOfFamily { get; set; } = string.Empty;

    /// <summary>
    /// Address (required)
    /// </summary>
    [Required(ErrorMessage = "Address is required")]
    [StringLength(500, ErrorMessage = "Address cannot exceed 500 characters")]
    public string Address { get; set; } = string.Empty;

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
    /// Charity ID for data isolation
    /// </summary>
    public Guid? CharityId { get; set; }

    /// <summary>
    /// Living Condition ID (dropdown)
    /// </summary>
    public int? LivingConditionId { get; set; }

    /// <summary>
    /// Housing Type ID (dropdown)
    /// </summary>
    public int? HousingTypeId { get; set; }

    /// <summary>
    /// Provider Type: Father, Mother, Other
    /// </summary>
    [StringLength(50, ErrorMessage = "Provider type cannot exceed 50 characters")]
    public string? ProviderType { get; set; }

    /// <summary>
    /// Notes
    /// </summary>
    [StringLength(2000, ErrorMessage = "Notes cannot exceed 2000 characters")]
    public string? Notes { get; set; }

    // Provider Information
    /// <summary>
    /// Father information (mandatory - at minimum Full Name, National ID, Date of Birth required)
    /// </summary>
    [Required(ErrorMessage = "Father information is required")]
    public CreateFatherDto Father { get; set; } = new();

    /// <summary>
    /// Mother information (mandatory - at minimum Full Name, National ID, Date of Birth required)
    /// </summary>
    [Required(ErrorMessage = "Mother information is required")]
    public CreateMotherDto Mother { get; set; } = new();

    /// <summary>
    /// Other provider information (optional, required only if ProviderType is "Other")
    /// </summary>
    public CreateProviderDto? Provider { get; set; }

    /// <summary>
    /// Other family relatives (optional - brothers, sisters, grandparents, uncles, aunts, etc.)
    /// </summary>
    public List<CreateRelativeDto>? Relatives { get; set; }

    // Initial Orphans (optional, can add later)
    /// <summary>
    /// List of orphans to add to family
    /// </summary>
    public List<CreateOrphanDto>? Orphans { get; set; }
}
