namespace IIROSA.Application.DTOs.MissionManagement;

/// <summary>
/// Mission list item for grid display (UC-MSN-01, §20.S.1 — 13 grid columns)
/// </summary>
public class MissionListDto
{
    public Guid Id { get; set; }
    public string MissionTarget { get; set; } = string.Empty;
    public string? MissionType { get; set; }
    public string? MissionTimeType { get; set; }
    public string? EntityName { get; set; }
    public string? Details { get; set; }
    public DateTime MissionDate { get; set; }
    public string? Region { get; set; }
    public string? Village { get; set; }
    public string? MissionLocation { get; set; }
    public string? Center { get; set; }
    public string? AssignedTo { get; set; }
    public bool IsMissionCompleted { get; set; }
    public string? MissionCompletedTxt { get; set; }
    public string? CountryName { get; set; }
}

/// <summary>
/// Mission detail with all information (UC-MSN-07).
/// Id keys use the clean wire names — under Newtonsoft camelCase a `FK_…` property serializes
/// as `fK_…`, which matches nothing the SPA reads (13-3 rename precedent).
/// </summary>
public class MissionDetailDto
{
    public Guid Id { get; set; }

    // Basic Information
    public string MissionTarget { get; set; } = string.Empty;
    public string? MissionDetails { get; set; }
    public string? Details { get; set; }

    // Classification
    public int? MissionTypeId { get; set; }
    public string? MissionTypeName { get; set; }
    public int? MissionTimeTypeId { get; set; }
    public string? MissionTimeTypeName { get; set; }
    public int? MissionInterviewTypeId { get; set; }
    public string? MissionInterviewTypeName { get; set; }

    // Scheduling
    public DateTime MissionDate { get; set; }
    public DateTime? MissionCompletedDate { get; set; }
    public bool IsMissionCompleted { get; set; }
    public string? MissionCompletedTxt { get; set; }

    // Location
    public int? CountryId { get; set; }
    public string? CountryName { get; set; }
    public int? RegionId { get; set; }
    public string? RegionName { get; set; }
    public int? CenterId { get; set; }
    public string? CenterName { get; set; }
    public string? MissionLocation { get; set; }
    public string? Village { get; set; }

    // Assignment
    public Guid? AssignedToUserId { get; set; }
    public string? AssignedUserName { get; set; }
    public string? AssignedUserEmail { get; set; }

    // Ownership
    public Guid? CharityId { get; set; }
    public string? CharityName { get; set; }

    // Event Information
    public string? EntityName { get; set; }
    public string? ConferenceName { get; set; }

    // Audit
    public DateTime CreatedOn { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedOn { get; set; }
    public string? UpdatedBy { get; set; }
}

/// <summary>
/// Create mission DTO (UC-MSN-06)
/// Wire keys are the clean names the SPA sends (13-3 precedent for the FK_* → clean rename).
/// </summary>
public class CreateMissionDto
{
    // Required fields
    public string MissionTarget { get; set; } = string.Empty;
    public DateTime MissionDate { get; set; }
    public int MissionTypeId { get; set; }
    public int MissionTimeTypeId { get; set; }
    public int MissionInterviewTypeId { get; set; }
    public Guid AssignedToUserId { get; set; }

    // Optional fields
    public string? MissionDetails { get; set; }
    public string? Details { get; set; }
    public int? CountryId { get; set; }
    public int? RegionId { get; set; }
    public int? CenterId { get; set; }
    public string? MissionLocation { get; set; }
    public string? Village { get; set; }
    public string? EntityName { get; set; }
    public string? ConferenceName { get; set; }
}

/// <summary>
/// Update mission DTO (UC-MSN-07) — same wire names as create
/// </summary>
public class UpdateMissionDto
{
    public string? MissionTarget { get; set; }
    public string? MissionDetails { get; set; }
    public string? Details { get; set; }
    public DateTime? MissionDate { get; set; }
    public int? MissionTypeId { get; set; }
    public int? MissionTimeTypeId { get; set; }
    public int? MissionInterviewTypeId { get; set; }
    public int? CountryId { get; set; }
    public int? RegionId { get; set; }
    public int? CenterId { get; set; }
    public string? MissionLocation { get; set; }
    public string? Village { get; set; }
    public string? EntityName { get; set; }
    public string? ConferenceName { get; set; }
    public Guid? AssignedToUserId { get; set; }
}

/// <summary>
/// Register mission result DTO (UC-MSN-09) — the §20.S.3 editable fields plus the outcome.
/// The screen's two completion checkboxes are one tri-state on the wire: the actor checks
/// either "completed" or "not completed".
/// </summary>
public class RegisterMissionResultDto
{
    public string? EntityName { get; set; }
    public string? ConferenceName { get; set; }
    public string? Details { get; set; }
    public string? MissionTarget { get; set; }
    public string? MissionDetails { get; set; }
    public string? MissionLocation { get; set; }
    public Guid? AssignedToUserId { get; set; }
    public string? Village { get; set; }

    /// <summary>
    /// The completion outcome — must be explicit (either true or false), never left null.
    /// </summary>
    public bool? IsCompleted { get; set; }

    /// <summary>
    /// السبب — the reason/result text (MissionCompletedTxt), mandatory.
    /// </summary>
    public string? Reason { get; set; }
}

/// <summary>
/// Mission filter DTO for queries (UC-MSN-01, UC-MSN-02)
/// Wire keys are the clean names the SPA sends; the server adds the caller scope on top.
/// </summary>
public class MissionFilterDto
{
    public string? Search { get; set; }                 // Search by mission target
    public int? MissionTypeId { get; set; }             // Filter by type
    public int? MissionTimeTypeId { get; set; }         // Filter by time type
    public Guid? CharityId { get; set; }                // Filter by owning charity
    public int? CountryId { get; set; }
    public int? RegionId { get; set; }
    public int? CenterId { get; set; }
    public Guid? AssignedToUserId { get; set; }         // Filter by assigned user
    public bool? IsCompleted { get; set; }              // Filter by completion status
    public DateTime? DateFrom { get; set; }             // Date range filter
    public DateTime? DateTo { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

/// <summary>
/// Mission status summary (legacy status dashboard support)
/// </summary>
public class MissionStatusSummaryDto
{
    public int PendingCount { get; set; }
    public int InProgressCount { get; set; }
    public int CompletedCount { get; set; }
    public int OverdueCount { get; set; }
    public int TotalCount { get; set; }
}

/// <summary>
/// Paged result wrapper for missions
/// </summary>
public class MissionPagedResult<T>
{
    public List<T> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling((decimal)TotalCount / PageSize);
}

// ========== DTOs for the legacy fine-grained endpoints (UC-8.x capability variants) ==========

/// <summary>
/// Set mission date DTO (UC-8.2)
/// </summary>
public class SetMissionDateDto
{
    public DateTime MissionDate { get; set; }
}

/// <summary>
/// Assign mission type DTO (UC-8.3)
/// </summary>
public class AssignMissionTypeDto
{
    public int FK_MissionTypeId { get; set; }
}

/// <summary>
/// Assign mission time type DTO (UC-8.4)
/// </summary>
public class AssignMissionTimeTypeDto
{
    public int FK_MissionTimeTypeId { get; set; }
}

/// <summary>
/// Mission location DTO (UC-8.5)
/// </summary>
public class MissionLocationDto
{
    public int? FK_CountryId { get; set; }
    public int? FK_RegionId { get; set; }
    public int? FK_CenterId { get; set; }
    public string? MissionLocation { get; set; }
    public string? Village { get; set; }
}

/// <summary>
/// Assign mission owner DTO (UC-8.6)
/// </summary>
public class AssignMissionOwnerDto
{
    public Guid FK_UserId { get; set; }
}
