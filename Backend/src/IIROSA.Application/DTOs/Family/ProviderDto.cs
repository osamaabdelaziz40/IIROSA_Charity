using System.ComponentModel.DataAnnotations;

namespace IIROSA.Application.DTOs.Family;

/// <summary>
/// Provider DTO - Non-parent provider information
/// </summary>
public class ProviderDto
{
    public Guid Id { get; set; }
    public Guid? FamilyId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string RelationshipToFamily { get; set; } = string.Empty;
    public string NationalId { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string? Address { get; set; }
    public string? Job { get; set; }
    public decimal? MonthlyIncome { get; set; }
    public string? Notes { get; set; }

    // Refugee register extensions (epic 7, UC-REF-03 §12.S.2 اضافة معيل)
    public DateTime? DateOfBirth { get; set; }
    public int? NationalityCountryId { get; set; }
    public bool? IsAlive { get; set; }
    public DateTime? DeathDate { get; set; }
    /// <summary>Cause of death — closed set: طبيعية / مرض / حادث (static list on the form)</summary>
    public string? DeathReason { get; set; }
    public int? ReasonOfRelationId { get; set; }
    public string? ReasonOfRelationName { get; set; }

    // Housing register extensions (epic 6, UC-HOU-03 §11.S.2 اضافة معيل)
    public int? RelationId { get; set; }
    public string? RelationName { get; set; }
    /// <summary>Main relation — closed set: الاب / الام</summary>
    public string? MainRelation { get; set; }
    public int? SocialStatusId { get; set; }
    public string? SocialStatusName { get; set; }
    public int? HealthStatusId { get; set; }
    public string? HealthStatusName { get; set; }
    public int? EducationLevelId { get; set; }
    public string? EducationLevelName { get; set; }
    public bool? WidowSponsorship { get; set; }
    public bool? AnotherSponsor { get; set; }
    public bool? MotherIsMar { get; set; }
    public bool? IsCaring { get; set; }
}

/// <summary>
/// Create Provider DTO - Used for adding non-parent provider (UC-4.6: Add Non-Parent Provider)
/// </summary>
public class CreateProviderDto
{
    [Required(ErrorMessage = "Full name is required")]
    [StringLength(200, ErrorMessage = "Full name cannot exceed 200 characters")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Relationship to family is required")]
    [StringLength(100, ErrorMessage = "Relationship cannot exceed 100 characters")]
    public string RelationshipToFamily { get; set; } = string.Empty;

    [Required(ErrorMessage = "National ID is required")]
    [StringLength(50, ErrorMessage = "National ID cannot exceed 50 characters")]
    public string NationalId { get; set; } = string.Empty;

    [Required(ErrorMessage = "Phone is required")]
    [StringLength(20, ErrorMessage = "Phone cannot exceed 20 characters")]
    public string Phone { get; set; } = string.Empty;

    [StringLength(500, ErrorMessage = "Address cannot exceed 500 characters")]
    public string? Address { get; set; }

    [StringLength(100, ErrorMessage = "Job cannot exceed 100 characters")]
    public string? Job { get; set; }

    public decimal? MonthlyIncome { get; set; }

    [StringLength(2000, ErrorMessage = "Notes cannot exceed 2000 characters")]
    public string? Notes { get; set; }

    // Refugee register extensions (epic 7, UC-REF-03 §12.S.2 اضافة معيل)
    public DateTime? DateOfBirth { get; set; }
    public int? NationalityCountryId { get; set; }
    public bool? IsAlive { get; set; }
    public DateTime? DeathDate { get; set; }
    /// <summary>Cause of death — closed set: طبيعية / مرض / حادث (static list on the form)</summary>
    [StringLength(100, ErrorMessage = "Death reason cannot exceed 100 characters")]
    public string? DeathReason { get; set; }
    public int? ReasonOfRelationId { get; set; }

    // Housing register extensions (epic 6, UC-HOU-03 §11.S.2 اضافة معيل)
    /// <summary>Relation kind (نوعها) — lookup Relation</summary>
    public int? RelationId { get; set; }
    /// <summary>Main relation — closed set: الاب / الام</summary>
    [StringLength(20, ErrorMessage = "Main relation cannot exceed 20 characters")]
    public string? MainRelation { get; set; }
    public int? SocialStatusId { get; set; }
    public int? HealthStatusId { get; set; }
    public int? EducationLevelId { get; set; }
    public bool? WidowSponsorship { get; set; }
    public bool? AnotherSponsor { get; set; }
    public bool? MotherIsMar { get; set; }
    public bool? IsCaring { get; set; }
}

/// <summary>
/// Update Provider DTO
/// </summary>
public class UpdateProviderDto
{
    [Required]
    public Guid Id { get; set; }

    [StringLength(200, ErrorMessage = "Full name cannot exceed 200 characters")]
    public string? FullName { get; set; }

    [StringLength(100, ErrorMessage = "Relationship cannot exceed 100 characters")]
    public string? RelationshipToFamily { get; set; }

    [StringLength(50, ErrorMessage = "National ID cannot exceed 50 characters")]
    public string? NationalId { get; set; }

    [StringLength(20, ErrorMessage = "Phone cannot exceed 20 characters")]
    public string? Phone { get; set; }

    [StringLength(500, ErrorMessage = "Address cannot exceed 500 characters")]
    public string? Address { get; set; }

    [StringLength(100, ErrorMessage = "Job cannot exceed 100 characters")]
    public string? Job { get; set; }

    public decimal? MonthlyIncome { get; set; }

    [StringLength(2000, ErrorMessage = "Notes cannot exceed 2000 characters")]
    public string? Notes { get; set; }

    // Refugee register extensions (epic 7, UC-REF-03 §12.S.2 اضافة معيل)
    public DateTime? DateOfBirth { get; set; }
    public int? NationalityCountryId { get; set; }
    public bool? IsAlive { get; set; }
    public DateTime? DeathDate { get; set; }
    [StringLength(100, ErrorMessage = "Death reason cannot exceed 100 characters")]
    public string? DeathReason { get; set; }
    public int? ReasonOfRelationId { get; set; }

    // Housing register extensions (epic 6, UC-HOU-03 §11.S.2 اضافة معيل)
    public int? RelationId { get; set; }
    [StringLength(20, ErrorMessage = "Main relation cannot exceed 20 characters")]
    public string? MainRelation { get; set; }
    public int? SocialStatusId { get; set; }
    public int? HealthStatusId { get; set; }
    public int? EducationLevelId { get; set; }
    public bool? WidowSponsorship { get; set; }
    public bool? AnotherSponsor { get; set; }
    public bool? MotherIsMar { get; set; }
    public bool? IsCaring { get; set; }
}
