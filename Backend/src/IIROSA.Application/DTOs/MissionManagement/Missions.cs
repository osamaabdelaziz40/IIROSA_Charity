namespace IIROSA.Application.DTOs.MissionManagement;

/// <summary>
/// Mission list item for grid display (UC-8.10)
/// </summary>
public class MissionListDto
{
    public Guid Id { get; set; }
    public string MissionTarget { get; set; } = string.Empty;
    public string? MissionType { get; set; }
    public string? MissionTimeType { get; set; }
    public DateTime MissionDate { get; set; }
    public string? Region { get; set; }
    public string? Center { get; set; }
    public string? AssignedTo { get; set; }
    public bool IsMissionCompleted { get; set; }
    public DateTime? MissionCompletedDate { get; set; }
    public string? CountryName { get; set; }
}

/// <summary>
/// Mission detail with all information (UC-8.11)
/// </summary>
public class MissionDetailDto
{
    public Guid Id { get; set; }

    // Basic Information
    public string MissionTarget { get; set; } = string.Empty;
    public string? MissionDetails { get; set; }
    public string? Details { get; set; }

    // Classification
    public int? FK_MissionTypeId { get; set; }
    public string? MissionTypeName { get; set; }
    public int? FK_MissionTimeTypeId { get; set; }
    public string? MissionTimeTypeName { get; set; }

    // Scheduling
    public DateTime MissionDate { get; set; }
    public DateTime? MissionCompletedDate { get; set; }
    public bool IsMissionCompleted { get; set; }
    public string? MissionCompletedTxt { get; set; }

    // Location
    public int? FK_CountryId { get; set; }
    public string? CountryName { get; set; }
    public int? FK_RegionId { get; set; }
    public string? RegionName { get; set; }
    public int? FK_CenterId { get; set; }
    public string? CenterName { get; set; }
    public string? MissionLocation { get; set; }
    public string? Village { get; set; }

    // Assignment
    public Guid? FK_UserId { get; set; }
    public string? AssignedUserName { get; set; }
    public string? AssignedUserEmail { get; set; }

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
/// Create mission DTO (UC-8.1)
/// </summary>
public class CreateMissionDto
{
    // Required fields
    public string MissionTarget { get; set; } = string.Empty;
    public DateTime MissionDate { get; set; }
    public int FK_MissionTypeId { get; set; }
    public int FK_MissionTimeTypeId { get; set; }
    public Guid FK_UserId { get; set; }

    // Optional fields
    public string? MissionDetails { get; set; }
    public string? Details { get; set; }
    public int? FK_CountryId { get; set; }
    public int? FK_RegionId { get; set; }
    public int? FK_CenterId { get; set; }
    public string? MissionLocation { get; set; }
    public string? Village { get; set; }
    public string? EntityName { get; set; }
    public string? ConferenceName { get; set; }
}

/// <summary>
/// Update mission DTO (UC-8.7)
/// </summary>
public class UpdateMissionDto
{
    public string? MissionTarget { get; set; }
    public string? MissionDetails { get; set; }
    public string? Details { get; set; }
    public DateTime? MissionDate { get; set; }
    public int? FK_MissionTypeId { get; set; }
    public int? FK_MissionTimeTypeId { get; set; }
    public int? FK_CountryId { get; set; }
    public int? FK_RegionId { get; set; }
    public int? FK_CenterId { get; set; }
    public string? MissionLocation { get; set; }
    public string? Village { get; set; }
    public string? EntityName { get; set; }
    public string? ConferenceName { get; set; }
}

/// <summary>
/// Mission completion DTO (UC-8.8)
/// </summary>
public class CompleteMissionDto
{
    public bool IsMissionCompleted { get; set; } = true;
    public string? MissionCompletedTxt { get; set; }  // Completion notes
}

/// <summary>
/// Mission filter DTO for queries (UC-8.10, UC-8.13)
/// </summary>
public class MissionFilterDto
{
    public string? SearchText { get; set; }            // Search by mission target
    public int? FK_MissionTypeId { get; set; }         // Filter by type
    public int? FK_MissionTimeTypeId { get; set; }     // Filter by time type
    public int? FK_CountryId { get; set; }
    public int? FK_RegionId { get; set; }
    public int? FK_CenterId { get; set; }
    public Guid? FK_UserId { get; set; }               // Filter by assigned user
    public bool? IsMissionCompleted { get; set; }      // Filter by completion status
    public DateTime? StartDate { get; set; }           // Date range filter
    public DateTime? EndDate { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

/// <summary>
/// Mission status summary (UC-8.13)
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

// ========== DTOs for Specific Use Cases ==========

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

/// <summary>
/// Record conference/entity DTO (UC-8.9)
/// </summary>
public class RecordEventDto
{
    public string? ConferenceName { get; set; }
    public string? EntityName { get; set; }
}
