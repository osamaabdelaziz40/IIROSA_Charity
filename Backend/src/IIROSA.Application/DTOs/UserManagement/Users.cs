namespace IIROSA.Application.DTOs.UserManagement;

/// <summary>
/// User list item for grid display
/// </summary>
public class UserListDto
{
    public Guid Id { get; set; }
    public string? Email { get; set; }
    public string? FullName { get; set; }
    public string? UserName { get; set; }
    public string? PhoneNumber { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedOn { get; set; }
    public List<string> Roles { get; set; } = new();
}

/// <summary>
/// User detail with all information
/// </summary>
public class UserDetailDto
{
    public Guid Id { get; set; }
    public string? Email { get; set; }
    public string? FullName { get; set; }
    public string? UserName { get; set; }
    public string? PhoneNumber { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedOn { get; set; }
    public DateTime? UpdatedOn { get; set; }
    public List<string> Roles { get; set; } = new();
}

/// <summary>
/// Create user DTO
/// </summary>
public class CreateUserDto
{
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public List<string> Roles { get; set; } = new();
    public string? Password { get; set; } // Default: P@ssw0rd@2022
}

/// <summary>
/// Update user DTO
/// </summary>
public class UpdateUserDto
{
    public string? Email { get; set; }
    public string? FullName { get; set; }
    public string? PhoneNumber { get; set; }
    public bool? IsActive { get; set; }
    public List<string>? Roles { get; set; }
}

/// <summary>
/// User search criteria
/// </summary>
public class UserSearchDto
{
    public string? SearchText { get; set; }
    public bool? IsActive { get; set; }
    public List<string>? Roles { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

/// <summary>
/// User role assignment
/// </summary>
public class UserRoleDto
{
    public string RoleId { get; set; } = string.Empty;
    public string RoleName { get; set; } = string.Empty;
    public bool IsAssigned { get; set; }
}

/// <summary>
/// User claim
/// </summary>
public class UserClaimDto
{
    public string ClaimType { get; set; } = string.Empty;
    public string ClaimValue { get; set; } = string.Empty;
}

/// <summary>
/// Password reset DTO
/// </summary>
public class ResetPasswordDto
{
    public string? NewPassword { get; set; } // If null, auto-generate
    public bool SendEmail { get; set; } = true;
}
