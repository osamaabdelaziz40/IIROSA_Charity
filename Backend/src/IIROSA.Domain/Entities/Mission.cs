using IIROSA.Domain.Entities.Base;
using IIROSA.Domain.Entities.Lookups;
using Framework.Identity.Data.Entities;

namespace IIROSA.Domain.Entities;

/// <summary>
/// Mission entity - Inherits from FullAuditedEntityBase<Guid>
/// Implements all use cases UC-8.1 through UC-8.13
/// All audit fields (CreatedOn, UpdatedOn, CreatedBy, UpdatedBy, DeletedOn, DeletedBy, IsDeleted) are inherited
/// IMPORTANT: Charity users CANNOT access this module. Only Admin and Super Admin can manage missions.
/// </summary>
public class Mission : FullAuditedEntity
{
    // ========== Basic Information (UC-8.1) ==========

    /// <summary>
    /// Mission target/title (required)
    /// Examples: "Site Visit", "Training Session", "Field Assessment"
    /// </summary>
    public string MissionTarget { get; set; } = string.Empty;

    /// <summary>
    /// Detailed description of the mission
    /// </summary>
    public string? MissionDetails { get; set; }

    /// <summary>
    /// Additional notes or details about the mission
    /// </summary>
    public string? Details { get; set; }

    // ========== Mission Classification (UC-8.3, UC-8.4) ==========

    /// <summary>
    /// Foreign key to MissionType lookup (required)
    /// Examples: Fieldwork, Conference, Training, Meeting, Inspection
    /// </summary>
    public int? FK_MissionTypeId { get; set; }

    /// <summary>
    /// Foreign key to MissionTimeType lookup (required)
    /// Examples: One-time, Daily, Weekly, Monthly, Quarterly, Annually
    /// </summary>
    public int? FK_MissionTimeTypeId { get; set; }

    // ========== Scheduling (UC-8.2) ==========

    /// <summary>
    /// Scheduled date for the mission (required)
    /// </summary>
    public DateTime MissionDate { get; set; }

    /// <summary>
    /// Date when the mission was completed
    /// </summary>
    public DateTime? MissionCompletedDate { get; set; }

    /// <summary>
    /// Indicates if the mission is completed (UC-8.8)
    /// Default: false
    /// </summary>
    public bool IsMissionCompleted { get; set; } = false;

    /// <summary>
    /// Completion notes or text (UC-8.8)
    /// </summary>
    public string? MissionCompletedTxt { get; set; }

    // ========== Location (UC-8.5) ==========

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
    /// Mission location address (text field)
    /// </summary>
    public string? MissionLocation { get; set; }

    /// <summary>
    /// Village name (if applicable)
    /// </summary>
    public string? Village { get; set; }

    // ========== Assignment (UC-8.6) ==========

    /// <summary>
    /// Foreign key to ApplicationUser (assigned user)
    /// The user responsible for executing this mission
    /// </summary>
    public Guid? FK_UserId { get; set; }

    // ========== Event Information (UC-8.9) ==========

    /// <summary>
    /// Entity name (if applicable to the mission)
    /// </summary>
    public string? EntityName { get; set; }

    /// <summary>
    /// Conference name (if applicable to the mission)
    /// </summary>
    public string? ConferenceName { get; set; }

    // ========== Navigation Properties ==========

    /// <summary>
    /// Navigation to MissionType lookup
    /// </summary>
    public virtual MissionType? MissionType { get; set; }

    /// <summary>
    /// Navigation to MissionTimeType lookup
    /// </summary>
    public virtual MissionTimeType? MissionTimeType { get; set; }

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
    /// Navigation to assigned ApplicationUser
    /// </summary>
    public virtual ApplicationUser? AssignedUser { get; set; }
}
