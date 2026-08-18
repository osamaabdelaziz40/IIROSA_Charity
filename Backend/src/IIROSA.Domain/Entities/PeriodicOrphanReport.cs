using IIROSA.Domain.Entities.Base;

namespace IIROSA.Domain.Entities;

/// <summary>
/// Periodic Orphan Report entity - Implements UC-6.11 (Create Periodic Orphan Report)
/// Inherits from FullAuditedEntity which provides: Id, CreatedOn, UpdatedOn, CreatedBy, UpdatedBy, DeletedOn, DeletedBy, IsDeleted
/// </summary>
public class PeriodicOrphanReport : FullAuditedEntity
{
    #region Basic Information

    /// <summary>
    /// Foreign key to Orphan (Required) - UC-6.11
    /// </summary>
    public Guid OrphanId { get; set; }

    /// <summary>
    /// Optional link to OrphanPayment (UC-6.11) - Report can be created with or without payment selection
    /// </summary>
    public Guid? OrphanPaymentId { get; set; }

    /// <summary>
    /// Report Date (Required) - UC-6.11
    /// </summary>
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
    /// Report Number (auto-generated or manual) - UC-6.11
    /// </summary>
    public string? ReportNo { get; set; }

    /// <summary>
    /// Foreign key to Charity - Optional, derived from orphan
    /// </summary>
    public Guid? CharityId { get; set; }

    #endregion

    #region Religious & Behavioral Tracking (UC-6.11)

    /// <summary>
    /// Prayer Status - UC-6.11
    /// </summary>
    public string? PrayerStatus { get; set; }

    /// <summary>
    /// Manners Status - UC-6.11
    /// </summary>
    public string? MannersStatus { get; set; }

    /// <summary>
    /// Hadeeth Status - UC-6.11
    /// </summary>
    public string? HadeethStatus { get; set; }

    #endregion

    #region Quran Education (UC-6.11)

    /// <summary>
    /// Quran Parts memorized/being studied - UC-6.11
    /// </summary>
    public string? QuranParts { get; set; }

    /// <summary>
    /// Quran Verses memorized/being studied - UC-6.11
    /// </summary>
    public string? QuranVerses { get; set; }

    #endregion

    #region Health & Medical (UC-6.11)

    /// <summary>
    /// Medical Status - UC-6.11
    /// </summary>
    public string? MedicalStatus { get; set; }

    /// <summary>
    /// Disease information - UC-6.11
    /// </summary>
    public string? Disease { get; set; }

    /// <summary>
    /// Disability information - UC-6.11
    /// </summary>
    public string? Disability { get; set; }

    /// <summary>
    /// Disability Description - UC-6.11
    /// </summary>
    public string? DisabilityDescription { get; set; }

    /// <summary>
    /// Disease Description - UC-6.11
    /// </summary>
    public string? DiseaseDescription { get; set; }

    /// <summary>
    /// Medical Report Image (Attachment ID) - FK_MedicalReportImg - UC-6.11
    /// </summary>
    public Guid? MedicalReportImageId { get; set; }

    #endregion

    #region Personal Development (UC-6.11)

    /// <summary>
    /// Hobby - UC-6.11
    /// </summary>
    public string? Hobby { get; set; }

    /// <summary>
    /// Course - UC-6.11
    /// </summary>
    public string? Course { get; set; }

    /// <summary>
    /// Course Name - UC-6.11
    /// </summary>
    public string? CourseName { get; set; }

    /// <summary>
    /// Sport Name - UC-6.11
    /// </summary>
    public string? SportName { get; set; }

    /// <summary>
    /// Profession Name - UC-6.11
    /// </summary>
    public string? ProfessionName { get; set; }

    /// <summary>
    /// Achievement - UC-6.11
    /// </summary>
    public string? Achievement { get; set; }

    /// <summary>
    /// Achievement Array (stored as JSON) - UC-6.11
    /// </summary>
    public string? AchievementArr { get; set; }

    /// <summary>
    /// Wish - UC-6.11
    /// </summary>
    public string? Wish { get; set; }

    /// <summary>
    /// Wish Array (stored as JSON) - UC-6.11
    /// </summary>
    public string? WishArr { get; set; }

    /// <summary>
    /// Orphan Message - UC-6.11
    /// </summary>
    public string? OrphanMessage { get; set; }

    #endregion

    #region Education Details (UC-6.11)

    /// <summary>
    /// Educational Stage (Lookup FK) - FK_EducationalStage - UC-6.11
    /// </summary>
    public int? EducationalStageId { get; set; }

    /// <summary>
    /// Educational Level (Lookup FK) - FK_EducationalLevel - UC-6.11
    /// </summary>
    public int? EducationalLevelId { get; set; }

    /// <summary>
    /// Grade - UC-6.11
    /// </summary>
    public string? Grade { get; set; }

    /// <summary>
    /// School Name - UC-6.11
    /// </summary>
    public string? School { get; set; }

    /// <summary>
    /// School Type - UC-6.11
    /// </summary>
    public string? SchoolType { get; set; }

    /// <summary>
    /// Education Degree - UC-6.11
    /// </summary>
    public string? EducationDegree { get; set; }

    /// <summary>
    /// Highest Educational Level - UC-6.11
    /// </summary>
    public string? HighestEducationalLevel { get; set; }

    /// <summary>
    /// Highest Educational Level Year - UC-6.11
    /// </summary>
    public int? HighestEducationalLevelYear { get; set; }

    /// <summary>
    /// Is Orphan Student - UC-6.11
    /// </summary>
    public bool? IsOrphanStudent { get; set; }

    /// <summary>
    /// Educational Year - UC-6.11
    /// </summary>
    public int? EducationalYear { get; set; }

    /// <summary>
    /// Annual Fee for Study - UC-6.11
    /// </summary>
    public decimal? AnnualFeeForStudy { get; set; }

    /// <summary>
    /// Studying Years - UC-6.11
    /// </summary>
    public int? StudyingYears { get; set; }

    /// <summary>
    /// Rest Studying Years - UC-6.11
    /// </summary>
    public int? RestStudyingYears { get; set; }

    /// <summary>
    /// Graduation Year - UC-6.11
    /// </summary>
    public int? GraduationYear { get; set; }

    /// <summary>
    /// Drop Out - UC-6.11
    /// </summary>
    public bool? DropOut { get; set; }

    /// <summary>
    /// Drop Out Year - UC-6.11
    /// </summary>
    public int? DropOutYear { get; set; }

    /// <summary>
    /// Drop Out Stage (Lookup FK) - FK_DropOutStage - UC-6.11
    /// </summary>
    public int? DropOutStageId { get; set; }

    /// <summary>
    /// Faculty - UC-6.11
    /// </summary>
    public string? Faculty { get; set; }

    /// <summary>
    /// Department - UC-6.11
    /// </summary>
    public string? Department { get; set; }

    /// <summary>
    /// Specialization - UC-6.11
    /// </summary>
    public string? Specialization { get; set; }

    #endregion

    #region Life Events (UC-6.11)

    /// <summary>
    /// Married - UC-6.11
    /// </summary>
    public bool? Married { get; set; }

    /// <summary>
    /// Marriage Date - UC-6.11
    /// </summary>
    public DateTime? MarriageDate { get; set; }

    /// <summary>
    /// Orphan Marriage Image (Attachment ID) - FK_OrphanMarriegeImage - UC-6.11
    /// </summary>
    public Guid? OrphanMarriageImageId { get; set; }

    /// <summary>
    /// Dead - UC-6.11
    /// </summary>
    public bool? Dead { get; set; }

    /// <summary>
    /// Death Date - UC-6.11
    /// </summary>
    public DateTime? DeathDate { get; set; }

    /// <summary>
    /// Orphan Dead Image (Attachment ID) - FK_OrphanDeadImage - UC-6.11
    /// </summary>
    public Guid? OrphanDeadImageId { get; set; }

    #endregion

    #region Attachments (UC-6.11)

    /// <summary>
    /// Orphan Certificate Image (Attachment ID) - FK_OrphanCertificateImg - UC-6.11
    /// </summary>
    public Guid? OrphanCertificateImageId { get; set; }

    /// <summary>
    /// Orphan Image (Attachment ID) - FK_OrphanImage - UC-6.11
    /// </summary>
    public Guid? OrphanImageId { get; set; }

    /// <summary>
    /// Missing Documents - UC-6.11
    /// </summary>
    public bool? MissingDocuments { get; set; }

    /// <summary>
    /// Missing Documents Name - UC-6.11
    /// </summary>
    public string? MissingDocumentsName { get; set; }

    #endregion

    #region Status Tracking (UC-6.11, UC-6.13)

    /// <summary>
    /// Reviewed - UC-6.11, UC-6.13
    /// </summary>
    public bool Reviewed { get; set; } = false;

    /// <summary>
    /// Reviewed Date - UC-6.11, UC-6.13
    /// </summary>
    public DateTime? ReviewedDate { get; set; }

    /// <summary>
    /// Reviewer (User FK) - UC-6.11, UC-6.13
    /// </summary>
    public Guid? ReviewerId { get; set; }

    /// <summary>
    /// Locked - UC-6.11
    /// </summary>
    public bool Locked { get; set; } = false;

    /// <summary>
    /// Locked Date - UC-6.11
    /// </summary>
    public DateTime? LockedDate { get; set; }

    /// <summary>
    /// Active - UC-6.11
    /// </summary>
    public bool Active { get; set; } = true;

    /// <summary>
    /// Active Date - UC-6.11
    /// </summary>
    public DateTime? ActiveDate { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Deleted (soft delete flag) - UC-6.11
    /// Note: Also inherited from base class as IsDeleted
    /// </summary>
    public bool Deleted { get; set; } = false;

    /// <summary>
    /// Deleted Date - UC-6.11
    /// </summary>
    public DateTime? DeletedDate { get; set; }

    /// <summary>
    /// Is Accepted - UC-6.11, UC-6.13
    /// </summary>
    public bool IsAccepted { get; set; } = false;

    /// <summary>
    /// Is Refused - UC-6.11, UC-6.13
    /// </summary>
    public bool IsRefused { get; set; } = false;

    /// <summary>
    /// Report Year (for reporting and filtering) - UC-6.11
    /// </summary>
    public int? ReportYear { get; set; }

    /// <summary>
    /// Report Month (for reporting and filtering) - UC-6.11
    /// </summary>
    public int? ReportMonth { get; set; }

    /// <summary>
    /// Review Status (Pending/Approved/Rejected) - UC-6.13
    /// </summary>
    public string? ReviewStatus { get; set; }

    /// <summary>
    /// Review Comments - UC-6.13
    /// </summary>
    public string? ReviewComments { get; set; }

    /// <summary>
    /// Quran Memorization Level - UC-6.11
    /// </summary>
    public string? QuranMemorization { get; set; }

    /// <summary>
    /// Education Level - UC-6.11
    /// </summary>
    public string? EducationLevel { get; set; }

    /// <summary>
    /// Education Stage - UC-6.11
    /// </summary>
    public string? EducationStage { get; set; }

    /// <summary>
    /// Academic Performance - UC-6.11
    /// </summary>
    public string? AcademicPerformance { get; set; }

    /// <summary>
    /// School Name - UC-6.11
    /// </summary>
    public string? SchoolName { get; set; }

    /// <summary>
    /// Health Status - UC-6.11
    /// </summary>
    public string? HealthStatus { get; set; }

    /// <summary>
    /// Chronic Diseases - UC-6.11
    /// </summary>
    public string? ChronicDiseases { get; set; }

    /// <summary>
    /// Medical Notes - UC-6.11
    /// </summary>
    public string? MedicalNotes { get; set; }

    /// <summary>
    /// Skills - UC-6.11
    /// </summary>
    public string? Skills { get; set; }

    /// <summary>
    /// Hobbies - UC-6.11
    /// </summary>
    public string? Hobbies { get; set; }

    /// <summary>
    /// Personal Notes - UC-6.11
    /// </summary>
    public string? PersonalNotes { get; set; }

    /// <summary>
    /// Major Events - UC-6.11
    /// </summary>
    public string? MajorEvents { get; set; }

    /// <summary>
    /// Achievements - UC-6.11
    /// </summary>
    public string? Achievements { get; set; }

    /// <summary>
    /// Challenges - UC-6.11
    /// </summary>
    public string? Challenges { get; set; }

    /// <summary>
    /// Refuse Reason - UC-6.11, UC-6.13
    /// </summary>
    public string? RefuseReason { get; set; }

    /// <summary>
    /// Refuse Reason Id (Lookup FK) - UC-6.11
    /// </summary>
    public int? RefuseReasonId { get; set; }

    /// <summary>
    /// Message Id - UC-6.11
    /// </summary>
    public int? MessageId { get; set; }

    #endregion

    #region Navigation Properties

    /// <summary>
    /// Navigation to Orphan - UC-6.11
    /// </summary>
    public virtual Orphan Orphan { get; set; } = null!;

    /// <summary>
    /// Navigation to Charity - UC-6.11
    /// </summary>
    public virtual Charity? Charity { get; set; }

    /// <summary>
    /// Navigation to OrphanPayment (optional) - UC-6.11
    /// </summary>
    public virtual OrphanPayment? OrphanPayment { get; set; }

    /// <summary>
    /// Navigation to Reviewer - UC-6.13
    /// TODO: Fix namespace reference - Framework.Identity.Data.Entities.ApplicationUser
    /// </summary>
    //public virtual Framework.Identity.Data.Entities.ApplicationUser? Reviewer { get; set; }

    #endregion
}
