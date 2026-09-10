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

    // Refugee register household fields (epic 7, UC-REF-03 §12.S.2; applied when present)

    /// <summary>
    /// Register discriminator: Regular | Housing | Refugee (string on the wire, parsed with
    /// Enum.TryParse in the service — 6-1 convention). Absent ⇒ unchanged.
    /// </summary>
    [StringLength(20, ErrorMessage = "Family type cannot exceed 20 characters")]
    public string? FamilyType { get; set; }

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

    // Family data extension (§4 معلومات الأسرة) — shared across registers, patch-style like the rest

    /// <summary>
    /// Income value (قيمة الدخل)
    /// </summary>
    public decimal? IncomeValue { get; set; }

    /// <summary>
    /// Total income (الدخل الكلى)
    /// </summary>
    public decimal? TotalIncome { get; set; }

    /// <summary>
    /// Children count (عدد الأبناء)
    /// </summary>
    public int? ChildrenCount { get; set; }

    /// <summary>
    /// Does the family own a project (هل الأسرة تمتلك مشروع) — null ⇒ unchanged
    /// </summary>
    public bool? HasProject { get; set; }

    /// <summary>
    /// Family project status (حالة المشروع) — lookup FamilyProjectStatus
    /// </summary>
    public int? FamilyProjectStatusId { get; set; }

    /// <summary>
    /// Contact numbers (multi phone) — full replace sync keyed by nothing (client sends the
    /// complete live set); absent ⇒ phones untouched. The service mirrors the default into
    /// <see cref="PhoneNumber"/> for legacy consumers.
    /// </summary>
    public List<CreateFamilyPhoneDto>? Phones { get; set; }
}
