using Framework.Core.SharedServices.Entities;
using IIROSA.Domain.Entities.Base;
using IIROSA.Domain.Entities.Lookups;

namespace IIROSA.Domain.Entities;

/// <summary>
/// OfficeProject entity - Inherits from FullAuditedEntityBase<Guid>
/// Implements all use cases UC-7.1 through UC-7.14
/// All audit fields (CreatedOn, UpdatedOn, CreatedBy, UpdatedBy, DeletedOn, DeletedBy, IsDeleted) are inherited
/// IMPORTANT: Charity users CANNOT access this module. Only Admin and Super Admin can manage office development projects.
/// </summary>
public class OfficeProject : FullAuditedEntity
{
    // ========== Basic Information (UC-7.1) ==========

    /// <summary>
    /// Project name (required)
    /// </summary>
    public string ProjectName { get; set; } = string.Empty;

    /// <summary>
    /// Project hint/description
    /// </summary>
    public string? ProjectHint { get; set; }

    /// <summary>
    /// Project start date (required)
    /// </summary>
    public DateTime ProjectDate { get; set; } = DateTime.Today;

    /// <summary>
    /// Project end date (set when project is completed)
    /// </summary>
    public DateTime? ProjectEndDate { get; set; }

    // ========== Project Classification (UC-7.1) ==========

    /// <summary>
    /// Foreign key to OfficeProjectType lookup (required)
    /// </summary>
    public int? FK_OfficeProjectTypeId { get; set; }

    // ========== Location (UC-7.5) ==========

    /// <summary>
    /// Foreign key to Country lookup
    /// </summary>
    public int? FK_CountryId { get; set; }

    /// <summary>
    /// Foreign key to Region lookup (filtered by Country)
    /// </summary>
    public int? FK_RegionId { get; set; }

    /// <summary>
    /// Foreign key to Center lookup (filtered by Region)
    /// </summary>
    public int? FK_CenterId { get; set; }

    /// <summary>
    /// Village name (text field)
    /// </summary>
    public string? VillageName { get; set; }

    // ========== Financial Information (UC-7.2) ==========

    /// <summary>
    /// Project cost in Egyptian Pounds
    /// </summary>
    public decimal? ProjectCostEGP { get; set; }

    /// <summary>
    /// Project cost in Saudi Riyals
    /// </summary>
    public decimal? ProjectCostSAR { get; set; }

    /// <summary>
    /// Donor name (UC-7.3)
    /// </summary>
    public string? DonorName { get; set; }

    // ========== Beneficiaries (UC-7.4) ==========

    /// <summary>
    /// Number of beneficiaries
    /// </summary>
    public int? BeneficiariesCount { get; set; }

    /// <summary>
    /// Beneficiaries type: Families, Individuals, Both
    /// </summary>
    public string? BeneficiariesType { get; set; }

    // ========== Charity Assignment (UC-7.14) ==========

    /// <summary>
    /// Foreign key to Charity (optional)
    /// </summary>
    public Guid? FK_CharityId { get; set; }

    // ========== Documents (UC-7.6, UC-7.7) ==========

    /// <summary>
    /// Foreign key to attached file (project proposal/document)
    /// </summary>
    public Guid? FK_AttachedFileId { get; set; }

    /// <summary>
    /// Foreign key to project report file (completion report)
    /// </summary>
    public Guid? FK_ProjectReportFileId { get; set; }

    // ========== Status (UC-7.9) ==========

    /// <summary>
    /// Indicates if the project is finished/completed
    /// Default: false
    /// </summary>
    public bool IsFinished { get; set; } = false;

    // ========== Additional Notes ==========

    /// <summary>
    /// Additional notes or details about the project
    /// </summary>
    public string? Notes { get; set; }

    // ========== Navigation Properties ==========

    /// <summary>
    /// Navigation to OfficeProjectType lookup
    /// </summary>
    public virtual OfficeProjectType? OfficeProjectType { get; set; }

    /// <summary>
    /// Navigation to Country lookup
    /// </summary>
    public virtual Country? Country { get; set; }

    /// <summary>
    /// Navigation to Region lookup
    /// </summary>
    public virtual Region? Region { get; set; }

    /// <summary>
    /// Navigation to Center lookup
    /// </summary>
    public virtual Center? Center { get; set; }

    /// <summary>
    /// Navigation to assigned Charity
    /// </summary>
    public virtual Charity? Charity { get; set; }

    /// <summary>
    /// Navigation to attached file
    /// </summary>
    //public virtual Attachment? AttachedFile { get; set; }

    ///// <summary>
    ///// Navigation to project report file
    ///// </summary>
    //public virtual Attachment? ProjectReportFile { get; set; }
}
