namespace IIROSA.Application.DTOs.Charity;

/// <summary>
/// Charity List DTO - Used for displaying charity in grid/list (UC-3.10)
/// </summary>
public class CharityListDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? RegionName { get; set; }
    public string? CenterName { get; set; }
    public string Phone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;

    // Status
    public bool IsActive { get; set; }
    public string StatusText => IsLocked ? "Locked" : (IsActive ? "Active" : "Inactive");

    public bool IsAddEnabled { get; set; }
    public string AddRightsText => IsAddEnabled ? "Enabled" : "Disabled";

    public bool IsUpdateEnabled { get; set; }
    public string UpdateRightsText => IsUpdateEnabled ? "Enabled" : "Disabled";

    public bool IsLocked { get; set; }
    public string LockedText => IsLocked ? "Yes" : "No";
}
