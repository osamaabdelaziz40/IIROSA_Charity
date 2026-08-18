namespace IIROSA.Application.DTOs.EmployeeManagement;

/// <summary>
/// Paged result wrapper for Employee management
/// </summary>
public class EmployeePagedResult<T>
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
/// Employee activity log entry
/// </summary>
public class EmployeeActivityDto
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public string? EmployeeName { get; set; }
    public string ActionType { get; set; } = string.Empty; // Create, Update, Delete, Activate, Deactivate
    public string? EntityAffected { get; set; }
    public string? Details { get; set; }
    public string? IpAddress { get; set; }
    public DateTime Timestamp { get; set; }
}

/// <summary>
/// Employee export response
/// </summary>
public class EmployeeExportDto
{
    public string Code { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Position { get; set; }
    public string? DepartmentName { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime? HireDate { get; set; }
    public DateTime CreatedOn { get; set; }
}
