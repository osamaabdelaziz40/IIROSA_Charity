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
    /// Register discriminator: Regular | Housing | Refugee (string on the wire, parsed with
    /// Enum.TryParse in the service — 6-1 convention). Absent ⇒ Regular.
    /// </summary>
    [StringLength(20, ErrorMessage = "Family type cannot exceed 20 characters")]
    public string? FamilyType { get; set; }

    // Refugee register household fields (epic 7, UC-REF-03 §12.S.2) — all optional here;
    // the CreateRefugeeFamilyValidator enforces the §12.S.2 mandatory flags when FamilyType=Refugee

    /// <summary>
    /// Governorate / region (المنطقة /المحافظة)
    /// </summary>
    public int? RegionId { get; set; }

    /// <summary>
    /// Center / city (المركز/ المدينة)
    /// </summary>
    public int? CenterId { get; set; }

    /// <summary>
    /// Nearby landmark (بجوار)
    /// </summary>
    [StringLength(100, ErrorMessage = "Nearby cannot exceed 100 characters")]
    public string? NearBy { get; set; }

    /// <summary>
    /// Street (الشارع)
    /// </summary>
    [StringLength(100, ErrorMessage = "Street cannot exceed 100 characters")]
    public string? Street { get; set; }

    /// <summary>
    /// Monthly rent (قيمة الإيجار)
    /// </summary>
    public decimal? RentAmount { get; set; }

    /// <summary>
    /// House ownership (ملكية السكن)
    /// </summary>
    public int? HouseOwnershipId { get; set; }

    /// <summary>
    /// House contents status (حالة محتويات السكن)
    /// </summary>
    public int? HouseStatusId { get; set; }

    /// <summary>
    /// Income type (نوع الدخل)
    /// </summary>
    public int? IncomeTypeId { get; set; }

    // Housing register allocation (epic 6, UC-HOU-03 §11.S.2 رقم العماره / رقم الشقه) —
    // mandatory when FamilyType=Housing (CreateHousingFamilyValidator); flat must belong to
    // the chosen building (enforced in AddNewHousingFamilyAsync)
    /// <summary>
    /// Housing building (رقم العماره) — lookup HousingBuilding
    /// </summary>
    public int? HousingBuildingId { get; set; }

    /// <summary>
    /// Housing flat (رقم الشقه) — lookup HousingFlat
    /// </summary>
    public int? HousingFlatId { get; set; }

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
    /// Father information — mandatory for a Regular family (enforced in the service);
    /// a Refugee family (§12.S.2) has no father/mother sections, so both are nullable.
    /// </summary>
    public CreateFatherDto? Father { get; set; }

    /// <summary>
    /// Mother information — mandatory for a Regular family (enforced in the service);
    /// a Refugee family (§12.S.2) has no father/mother sections, so both are nullable.
    /// </summary>
    public CreateMotherDto? Mother { get; set; }

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
