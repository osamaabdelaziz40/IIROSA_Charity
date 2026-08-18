namespace IIROSA.Application.DTOs.UserManagement;

/// <summary>
/// Role list item
/// </summary>
public class RoleListDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsSystemRole { get; set; }
    public int UserCount { get; set; }
}

/// <summary>
/// Role detail with permissions
/// </summary>
public class RoleDetailDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsSystemRole { get; set; }
    public List<string> Permissions { get; set; } = new();
    public List<RoleClaimDto> Claims { get; set; } = new();
}

/// <summary>
/// Create role DTO
/// </summary>
public class CreateRoleDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public List<string> Permissions { get; set; } = new();
    public List<RoleClaimDto> Claims { get; set; } = new();
}

/// <summary>
/// Update role DTO
/// </summary>
public class UpdateRoleDto
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public List<string>? Permissions { get; set; }
    public List<RoleClaimDto>? Claims { get; set; }
}

/// <summary>
/// Role search criteria
/// </summary>
public class RoleSearchDto
{
    public string? SearchText { get; set; }
    public bool? IsSystemRole { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

/// <summary>
/// Role claim for granular permissions
/// </summary>
public class RoleClaimDto
{
    public string ClaimType { get; set; } = string.Empty;
    public string ClaimValue { get; set; } = string.Empty;
}

/// <summary>
/// Simple role DTO for dropdowns
/// </summary>
public class RoleDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsSystemRole { get; set; }
}
