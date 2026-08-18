namespace IIROSA.Application.DTOs.PeriodicOrphanReport;

/// <summary>
/// Full Periodic Orphan Report DTO - Implements UC-6.11, UC-6.13, UC-6.14, UC-6.15, UC-6.16
/// </summary>
public class PeriodicOrphanReportDto
{
    public Guid Id { get; set; }

    #region Basic Information

    public Guid OrphanId { get; set; }
    public string? OrphanCode { get; set; }
    public string? OrphanName { get; set; }
    public Guid? OrphanPaymentId { get; set; }
    public DateTime ReportDate { get; set; }
    public DateTime? ReportPeriodFrom { get; set; }
    public DateTime? ReportPeriodTo { get; set; }
    public string? ReportNo { get; set; }
    public Guid? CharityId { get; set; }
    public string? CharityName { get; set; }

    #endregion

    #region Religious & Behavioral Tracking

    public string? PrayerStatus { get; set; }
    public string? MannersStatus { get; set; }
    public string? HadeethStatus { get; set; }

    #endregion

    #region Quran Education

    public string? QuranParts { get; set; }
    public string? QuranVerses { get; set; }

    #endregion

    #region Health & Medical

    public string? MedicalStatus { get; set; }
    public string? Disease { get; set; }
    public string? Disability { get; set; }
    public string? DisabilityDescription { get; set; }
    public string? DiseaseDescription { get; set; }
    public Guid? MedicalReportImageId { get; set; }
    public string? MedicalReportImageUrl { get; set; }

    #endregion

    #region Personal Development

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

    #region Education Details

    public int? EducationalStageId { get; set; }
    public string? EducationalStageName { get; set; }
    public int? EducationalLevelId { get; set; }
    public string? EducationalLevelName { get; set; }
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
    public string? DropOutStageName { get; set; }
    public string? Faculty { get; set; }
    public string? Department { get; set; }
    public string? Specialization { get; set; }

    #endregion

    #region Life Events

    public bool? Married { get; set; }
    public DateTime? MarriageDate { get; set; }
    public Guid? OrphanMarriageImageId { get; set; }
    public string? OrphanMarriageImageUrl { get; set; }
    public bool? Dead { get; set; }
    public DateTime? DeathDate { get; set; }
    public Guid? OrphanDeadImageId { get; set; }
    public string? OrphanDeadImageUrl { get; set; }

    #endregion

    #region Attachments

    public Guid? OrphanCertificateImageId { get; set; }
    public string? OrphanCertificateImageUrl { get; set; }
    public Guid? OrphanImageId { get; set; }
    public string? OrphanImageUrl { get; set; }
    public bool? MissingDocuments { get; set; }
    public string? MissingDocumentsName { get; set; }

    #endregion

    #region Status Tracking

    public bool Reviewed { get; set; }
    public DateTime? ReviewedDate { get; set; }
    public Guid? ReviewerId { get; set; }
    public string? ReviewerName { get; set; }
    public bool Locked { get; set; }
    public DateTime? LockedDate { get; set; }
    public bool Active { get; set; }
    public DateTime? ActiveDate { get; set; }
    public bool Deleted { get; set; }
    public DateTime? DeletedDate { get; set; }
    public bool IsAccepted { get; set; }
    public bool IsRefused { get; set; }
    public string? RefuseReason { get; set; }
    public int? RefuseReasonId { get; set; }
    public string? RefuseReasonName { get; set; }
    public int? MessageId { get; set; }

    /// <summary>
    /// Computed status for UI display - UC-6.14, UC-6.15
    /// </summary>
    public string ReviewStatus => !Reviewed ? "Pending" : IsAccepted ? "Approved" : "Rejected";

    #endregion

    #region Audit Fields (Inherited from FullAuditedEntity)

    public DateTime CreatedOn { get; set; }
    public string? CreatedBy { get; set; }
    public string? CreatedByName { get; set; }
    public DateTime UpdatedOn { get; set; }
    public string? UpdatedBy { get; set; }
    public string? UpdatedByName { get; set; }

    #endregion
}
