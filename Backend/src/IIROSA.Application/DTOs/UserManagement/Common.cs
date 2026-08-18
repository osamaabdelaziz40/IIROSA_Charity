using Framework.Core;

namespace IIROSA.Application.DTOs.UserManagement;

/// <summary>
/// Paged result wrapper
/// </summary>
public class PagedResult<T>
{
    public IEnumerable<T> Items { get; set; } = Enumerable.Empty<T>();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    public bool HasPrevious => PageNumber > 1;
    public bool HasNext => PageNumber < TotalPages;
}

/// <summary>
/// User activity log entry
/// </summary>
public class UserActivityDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string? UserName { get; set; }
    public string ActionType { get; set; } = string.Empty; // Create, Update, Delete, Login, Logout
    public string? EntityAffected { get; set; }
    public string? Details { get; set; }
    public string? IpAddress { get; set; }
    public DateTime Timestamp { get; set; }
}

/// <summary>
/// Activity search criteria
/// </summary>
public class ActivitySearchDto
{
    public Guid? UserId { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? ActionType { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
