using IIROSA.Application.DTOs.EmployeeManagement;

namespace IIROSA.Application.Interfaces;

/// <summary>
/// Employee Service Interface
/// Defines business operations for Employee management following UC-2.1 to UC-2.7
/// </summary>
public interface IEmployeeService
{
    /// <summary>
    /// Get employees with filtering and pagination (UC-2.5: View Employees List)
    /// </summary>
    Task<EmployeePagedResult<EmployeeListDto>> GetEmployeesFilteredAsync(EmployeeFilterDto filter);

    /// <summary>
    /// Check whether a proposed login name (the identity user's email/username) is free
    /// (UC-EMP-02: Verify employee username availability). Consults both the identity user
    /// store and the Employee table, excluding the given employee's own account when editing.
    /// </summary>
    Task<bool> IsUserNameAvailableAsync(string userName, Guid? excludeEmployeeId = null);

    /// <summary>
    /// Get employee by ID (UC-2.7: View Employee Profile)
    /// </summary>
    Task<EmployeeDetailDto?> GetEmployeeByIdAsync(Guid id);

    /// <summary>
    /// Create new employee (UC-2.1: Add Employee)
    /// </summary>
    Task<EmployeeDetailDto> CreateEmployeeAsync(CreateEmployeeDto dto);

    /// <summary>
    /// Update employee (UC-2.2: Update Employee Information)
    /// </summary>
    Task<EmployeeDetailDto> UpdateEmployeeAsync(Guid id, UpdateEmployeeDto dto);

    /// <summary>
    /// Deactivate employee (UC-2.3: Deactivate Employee)
    /// </summary>
    Task DeactivateEmployeeAsync(Guid id);

    /// <summary>
    /// Activate employee
    /// </summary>
    Task ActivateEmployeeAsync(Guid id);

    /// <summary>
    /// Reset employee password (UC-2.4: Reset Employee Password)
    /// </summary>
    Task ResetEmployeePasswordAsync(Guid id, string? newPassword = null);

    /// <summary>
    /// Assign employee to role (UC-2.6: Assign Employee Role)
    /// </summary>
    Task AssignEmployeeRoleAsync(Guid id, string roleName);

    /// <summary>
    /// Remove employee from role
    /// </summary>
    Task RemoveEmployeeRoleAsync(Guid id, string roleName);

    /// <summary>
    /// Delete employee
    /// </summary>
    Task DeleteEmployeeAsync(Guid id);

    /// <summary>
    /// Get employee roles
    /// </summary>
    Task<List<string>> GetEmployeeRolesAsync(Guid id);

    /// <summary>
    /// Export employees to Excel/CSV (UC-2.5: View Employees List - Export functionality)
    /// </summary>
    Task<EmployeePagedResult<EmployeeExportDto>> GetEmployeesForExportAsync(EmployeeFilterDto filter);
}
