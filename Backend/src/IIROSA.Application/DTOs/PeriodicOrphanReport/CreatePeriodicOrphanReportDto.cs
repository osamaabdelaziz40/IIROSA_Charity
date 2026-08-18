using System.ComponentModel.DataAnnotations;

namespace IIROSA.Application.DTOs.PeriodicOrphanReport;

/// <summary>
/// DTO for creating a new Periodic Orphan Report - Implements UC-6.11
/// </summary>
public class CreatePeriodicOrphanReportDto
{
    #region Basic Information

    /// <summary>
    /// Orphan ID (Required) - UC-6.11
    /// </summary>
    [Required(ErrorMessage = "Orphan is required")]
    public Guid OrphanId { get; set; }

    /// <summary>
    /// Optional link to OrphanPayment - UC-6.11
    /// </summary>
    public Guid? OrphanPaymentId { get; set; }

    /// <summary>
    /// Report Date (Required) - UC-6.11
    /// </summary>
    [Required(ErrorMessage = "Report date is required")]
    public DateTime ReportDate { get; set; }

    /// <summary>
    /// Report Period Start Date - UC-6.11
    /// </summary>
    public DateTime? ReportPeriodFrom { get; set; }

    /// <summary>
    /// Report Period End Date - UC-6.11
    /// </summary>
    public DateTime? ReportPeriodTo { get; set; }

    /// <summary>
    /// Report Number (optional, auto-generated if not provided) - UC-6.11
    /// </summary>
    public string? ReportNo { get; set; }

    #endregion

    #region Religious & Behavioral Tracking (UC-6.11)

    public string? PrayerStatus { get; set; }
    public string? MannersStatus { get; set; }
    public string? HadeethStatus { get; set; }

    #endregion

    #region Quran Education (UC-6.11)

    public string? QuranParts { get; set; }
    public string? QuranVerses { get; set; }

    #endregion

    #region Health & Medical (UC-6.11)

    public string? MedicalStatus { get; set; }
    public string? Disease { get; set; }
    public string? Disability { get; set; }
    public string? DisabilityDescription { get; set; }
    public string? DiseaseDescription { get; set; }
    public Guid? MedicalReportImageId { get; set; }

    #endregion

    #region Personal Development (UC-6.11)

    public string? Hobby { get; set; }
    public string? Course { get; set; }
    public string? CourseName { get; set; }
    public string? SportName { get; set; }
    public string? ProfessionName { get; set; }
    public string? Achievement { get; set; }
    public string? AchievementArr { get; set; }
    public string? Wish { get; set; }
    public string? WishArr { get; set; }
    public string? OrphanMessage { get; set; }

    #endregion

    #region Education Details (UC-6.11)

    public int? EducationalStageId { get; set; }
    public int? EducationalLevelId { get; set; }
    public string? Grade { get; set; }
    public string? School { get; set; }
    public string? SchoolType { get; set; }
    public string? EducationDegree { get; set; }
    public string? HighestEducationalLevel { get; set; }
    public int? HighestEducationalLevelYear { get; set; }
    public bool? IsOrphanStudent { get; set; }
    public int? EducationalYear { get; set; }
    public decimal? AnnualFeeForStudy { get; set; }
    public int? StudyingYears { get; set; }
    public int? RestStudyingYears { get; set; }
    public int? GraduationYear { get; set; }
    public bool? DropOut { get; set; }
    public int? DropOutYear { get; set; }
    public int? DropOutStageId { get; set; }
    public string? Faculty { get; set; }
    public string? Department { get; set; }
    public string? Specialization { get; set; }

    #endregion

    #region Life Events (UC-6.11)

    public bool? Married { get; set; }
    public DateTime? MarriageDate { get; set; }
    public Guid? OrphanMarriageImageId { get; set; }
    public bool? Dead { get; set; }
    public DateTime? DeathDate { get; set; }
    public Guid? OrphanDeadImageId { get; set; }

    #endregion

    #region Attachments (UC-6.11)

    public Guid? OrphanCertificateImageId { get; set; }
    public Guid? OrphanImageId { get; set; }
    public bool? MissingDocuments { get; set; }
    public string? MissingDocumentsName { get; set; }

    #endregion
}
