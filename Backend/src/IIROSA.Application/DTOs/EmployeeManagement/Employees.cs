namespace IIROSA.Application.DTOs.EmployeeManagement;

/// <summary>
/// Employee list item for grid display
/// </summary>
public class EmployeeListDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Position { get; set; }
    public string? DepartmentName { get; set; }
    public bool IsActive { get; set; }
    public DateTime HireDate { get; set; }
    public List<string> Roles { get; set; } = new();
}

/// <summary>
/// Employee detail with all information
/// </summary>
public class EmployeeDetailDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? NationalId { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? Gender { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public int? DepartmentId { get; set; }
    public string? DepartmentName { get; set; }
    public string? Position { get; set; }
    public DateTime? HireDate { get; set; }
    public decimal? Salary { get; set; }
    public bool IsActive { get; set; }
    public string? Notes { get; set; }
    public Guid? FK_UserId { get; set; }
    public bool HasAccount => FK_UserId.HasValue;
    public DateTime CreatedOn { get; set; }
    public DateTime? UpdatedOn { get; set; }
    public List<string> Roles { get; set; } = new();
}

/// <summary>
/// Create employee DTO
/// </summary>
public class CreateEmployeeDto
{
    public string Code { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? NationalId { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? Gender { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public int? DepartmentId { get; set; }
    public string? Position { get; set; }
    public DateTime? HireDate { get; set; }
    public decimal? Salary { get; set; }
    public bool IsActive { get; set; } = true;
    public string? Notes { get; set; }
    public List<string> Roles { get; set; } = new();
    public string? Password { get; set; } // For creating associated user account
}

/// <summary>
/// Update employee DTO
/// </summary>
public class UpdateEmployeeDto
{
    public string? Code { get; set; }
    public string? FullName { get; set; }
    public string? NationalId { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? Gender { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public int? DepartmentId { get; set; }
    public string? Position { get; set; }
    public DateTime? HireDate { get; set; }
    public decimal? Salary { get; set; }
    public bool? IsActive { get; set; }
    public string? Notes { get; set; }
    public List<string>? Roles { get; set; }
}

/// <summary>
/// Employee search criteria
/// </summary>
public class EmployeeSearchDto
{
    public string? SearchText { get; set; }
    public int? DepartmentId { get; set; }
    public string? Position { get; set; }
    public string? Role { get; set; }
    public bool? IsActive { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

/// <summary>
/// Employee filter for API queries
/// </summary>
public class EmployeeFilterDto
{
    public string? SearchText { get; set; }
    public int? DepartmentId { get; set; }
    public string? Position { get; set; }
    public string? Role { get; set; }
    public bool? IsActive { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

/// <summary>
/// Result of the UC-EMP-02 username availability check. The server echoes the
/// (trimmed) name it actually checked so the client can discard stale replies.
/// </summary>
public class EmployeeUserNameAvailabilityDto
{
    public string UserName { get; set; } = string.Empty;
    public bool IsAvailable { get; set; }
}

/// <summary>
/// Employee role assignment
/// </summary>
public class EmployeeRoleDto
{
    public string RoleName { get; set; } = string.Empty;
    public bool IsAssigned { get; set; }
}

/// <summary>
/// Employee password reset DTO
/// </summary>
public class EmployeeResetPasswordDto
{
    public string? NewPassword { get; set; } // If null, auto-generate
    public bool SendEmail { get; set; } = true;
}

/// <summary>
/// Assign role to employee DTO
/// </summary>
public class AssignRoleDto
{
    public string RoleName { get; set; } = string.Empty;
}
