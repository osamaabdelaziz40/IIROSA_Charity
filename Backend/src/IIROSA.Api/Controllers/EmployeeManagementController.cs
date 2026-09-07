using IIROSA.Application.DTOs.EmployeeManagement;
using IIROSA.Application.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IIROSA.Api.Controllers
{
    /// <summary>
    /// Employee Management API Controller
    /// Implements UC-2.1 to UC-2.7: Employee CRUD operations
    /// Follows approved Framework.Core architecture
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class EmployeeManagementController : ControllerBase
    {
        private readonly IEmployeeService _employeeService;
        private readonly ILogger<EmployeeManagementController> _logger;

        public EmployeeManagementController(
            IEmployeeService employeeService,
            ILogger<EmployeeManagementController> logger)
        {
            _employeeService = employeeService;
            _logger = logger;
        }

        /// <summary>
        /// Get all employees with filtering and pagination (UC-2.5: View Employees List)
        /// </summary>
        [HttpGet]
        [Authorize(Policy = "ManagementOnly")]
        public async Task<ActionResult<EmployeePagedResult<EmployeeListDto>>> GetEmployees(
            [FromQuery] string? search = null,
            [FromQuery] int? departmentId = null,
            [FromQuery] string? position = null,
            [FromQuery] string? role = null,
            [FromQuery] bool? isActive = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            try
            {
                var filter = new EmployeeFilterDto
                {
                    SearchText = search,
                    DepartmentId = departmentId,
                    Position = position,
                    Role = role,
                    IsActive = isActive,
                    Page = page,
                    PageSize = pageSize
                };

                var result = await _employeeService.GetEmployeesFilteredAsync(filter);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving employees");
                return StatusCode(500, new { message = "An error occurred while retrieving employees" });
            }
        }

        /// <summary>
        /// Check login-name availability (UC-EMP-02: Verify employee username availability).
        /// In this stack the identity UserName IS the email, so this probes the same two
        /// stores the create/update uniqueness rules enforce against. excludeEmployeeId
        /// spares an employee's own account when editing.
        /// </summary>
        [HttpGet("check-username")]
        [Authorize(Policy = "ManagementOnly")]
        public async Task<ActionResult<EmployeeUserNameAvailabilityDto>> CheckUserName(
            [FromQuery] string? userName,
            [FromQuery] Guid? excludeEmployeeId = null)
        {
            try
            {
                var name = (userName ?? string.Empty).Trim();
                var isAvailable = await _employeeService.IsUserNameAvailableAsync(name, excludeEmployeeId);
                return Ok(new EmployeeUserNameAvailabilityDto { UserName = name, IsAvailable = isAvailable });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while checking username availability for {UserName}", userName);
                return StatusCode(500, new { message = "An error occurred while checking username availability" });
            }
        }

        /// <summary>
        /// Get employee by ID (UC-2.7: View Employee Profile)
        /// </summary>
        [HttpGet("{id}")]
        [Authorize(Policy = "ManagementOnly")]
        public async Task<ActionResult<EmployeeDetailDto>> GetEmployee(Guid id)
        {
            try
            {
                var employee = await _employeeService.GetEmployeeByIdAsync(id);
                if (employee == null)
                {
                    return NotFound(new { message = "Employee not found" });
                }

                return Ok(employee);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving employee {EmployeeId}", id);
                return StatusCode(500, new { message = "An error occurred while retrieving employee" });
            }
        }

        /// <summary>
        /// Create new employee (UC-2.1: Add Employee)
        /// </summary>
        [HttpPost]
        [Authorize(Policy = "CanManageUsers")]
        public async Task<ActionResult<EmployeeDetailDto>> CreateEmployee([FromBody] CreateEmployeeDto model)
        {
            try
            {
                var employee = await _employeeService.CreateEmployeeAsync(model);
                return CreatedAtAction(nameof(GetEmployee), new { id = employee.Id }, employee);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating employee");
                return StatusCode(500, new { message = "An error occurred while creating employee" });
            }
        }

        /// <summary>
        /// Update employee (UC-2.2: Update Employee Information)
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Policy = "CanManageUsers")]
        public async Task<ActionResult<EmployeeDetailDto>> UpdateEmployee(Guid id, [FromBody] UpdateEmployeeDto model)
        {
            try
            {
                var employee = await _employeeService.UpdateEmployeeAsync(id, model);
                return Ok(employee);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                // Validation conflicts (duplicate email/code, rejected values) are 400s —
                // the same exception family the create action maps to BadRequest
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating employee {EmployeeId}", id);
                return StatusCode(500, new { message = "An error occurred while updating employee" });
            }
        }

        /// <summary>
        /// Deactivate employee (UC-2.3: Deactivate Employee)
        /// </summary>
        [HttpPatch("{id}/deactivate")]
        [Authorize(Policy = "CanManageUsers")]
        public async Task<ActionResult> DeactivateEmployee(Guid id)
        {
            try
            {
                await _employeeService.DeactivateEmployeeAsync(id);
                _logger.LogInformation("Employee {EmployeeId} deactivated by {DeactivatedBy}", id, User.Identity?.Name);
                return Ok(new { message = "Employee deactivated successfully" });
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deactivating employee {EmployeeId}", id);
                return StatusCode(500, new { message = "An error occurred while deactivating employee" });
            }
        }

        /// <summary>
        /// Activate employee
        /// </summary>
        [HttpPatch("{id}/activate")]
        [Authorize(Policy = "CanManageUsers")]
        public async Task<ActionResult> ActivateEmployee(Guid id)
        {
            try
            {
                await _employeeService.ActivateEmployeeAsync(id);
                _logger.LogInformation("Employee {EmployeeId} activated by {ActivatedBy}", id, User.Identity?.Name);
                return Ok(new { message = "Employee activated successfully" });
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while activating employee {EmployeeId}", id);
                return StatusCode(500, new { message = "An error occurred while activating employee" });
            }
        }

        /// <summary>
        /// Reset employee password (UC-2.4: Reset Employee Password)
        /// </summary>
        [HttpPost("{id}/reset-password")]
        [Authorize(Policy = "CanManageUsers")]
        public async Task<ActionResult> ResetEmployeePassword(Guid id, [FromBody] EmployeeResetPasswordDto model)
        {
            try
            {
                await _employeeService.ResetEmployeePasswordAsync(id, model.NewPassword);
                _logger.LogInformation("Password reset for employee {EmployeeId} by {ResetBy}", id, User.Identity?.Name);
                return Ok(new
                {
                    message = "Password reset successfully",
                    newPassword = model.SendEmail ? null : model.NewPassword ?? "P@ssw0rd@2022"
                });
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while resetting password for employee {EmployeeId}", id);
                return StatusCode(500, new { message = "An error occurred while resetting password" });
            }
        }

        /// <summary>
        /// Assign employee to role (UC-2.6: Assign Employee Role)
        /// </summary>
        [HttpPost("{id}/roles")]
        [Authorize(Policy = "CanManageUsers")]
        public async Task<ActionResult> AssignEmployeeToRole(Guid id, [FromBody] AssignRoleDto model)
        {
            try
            {
                await _employeeService.AssignEmployeeRoleAsync(id, model.RoleName);
                _logger.LogInformation("Employee {EmployeeId} assigned to role {RoleName} by {AssignedBy}", id, model.RoleName, User.Identity?.Name);
                return Ok(new { message = $"Employee successfully assigned to role '{model.RoleName}'" });
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while assigning role to employee {EmployeeId}", id);
                return StatusCode(500, new { message = "An error occurred while assigning role" });
            }
        }

        /// <summary>
        /// Remove employee from role
        /// </summary>
        [HttpDelete("{id}/roles/{roleName}")]
        [Authorize(Policy = "CanManageUsers")]
        public async Task<ActionResult> RemoveEmployeeFromRole(Guid id, string roleName)
        {
            try
            {
                await _employeeService.RemoveEmployeeRoleAsync(id, roleName);
                _logger.LogInformation("Employee {EmployeeId} removed from role {RoleName} by {RemovedBy}", id, roleName, User.Identity?.Name);

                return Ok(new { message = $"Employee successfully removed from role '{roleName}'" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while removing role from employee {EmployeeId}", id);
                return StatusCode(500, new { message = "An error occurred while removing role" });
            }
        }

        /// <summary>
        /// Delete employee
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Policy = "CanManageUsers")]
        public async Task<ActionResult> DeleteEmployee(Guid id)
        {
            try
            {
                await _employeeService.DeleteEmployeeAsync(id);
                _logger.LogInformation("Employee {EmployeeId} deleted by {DeletedBy}", id, User.Identity?.Name);

                return Ok(new { message = "Employee deleted successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting employee {EmployeeId}", id);
                return StatusCode(500, new { message = "An error occurred while deleting employee" });
            }
        }

        /// <summary>
        /// Export employees to Excel (UC-2.1: View All Employees - Export functionality)
        /// </summary>
        [HttpGet("export")]
        [Authorize(Policy = "ManagementOnly")]
        public async Task<IActionResult> ExportEmployees(
            [FromQuery] string? search = null,
            [FromQuery] int? departmentId = null,
            [FromQuery] string? position = null,
            [FromQuery] string? role = null,
            [FromQuery] bool? isActive = null)
        {
            try
            {
                var filter = new EmployeeFilterDto
                {
                    SearchText = search,
                    DepartmentId = departmentId,
                    Position = position,
                    Role = role,
                    IsActive = isActive,
                    Page = 1,
                    PageSize = 10000 // Export all matching records
                };

                var result = await _employeeService.GetEmployeesForExportAsync(filter);

                // Generate CSV content
                var csv = new System.Text.StringBuilder();

                // Header row
                csv.AppendLine("Code,Full Name,Email,Phone Number,Position,Department,Status,Hire Date,Created Date");

                // Data rows
                foreach (var employee in result.Items)
                {
                    var hireDate = employee.HireDate.HasValue ? employee.HireDate.Value.ToString("yyyy-MM-dd") : "";
                    var createdDate = employee.CreatedOn.ToString("yyyy-MM-dd HH:mm:ss");

                    csv.AppendLine($"{employee.Code},{employee.FullName},{employee.Email ?? ""},{employee.PhoneNumber ?? ""},{employee.Position ?? ""},{employee.DepartmentName ?? ""},{employee.Status},{hireDate},{createdDate}");
                }

                var fileName = $"employees_export_{DateTime.UtcNow:yyyyMMdd_HHmmss}.csv";
                var contentType = "text/csv";

                // Export completed successfully

                return File(System.Text.Encoding.UTF8.GetBytes(csv.ToString()), contentType, fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while exporting employees");
                return StatusCode(500, new { message = "An error occurred while exporting employees" });
            }
        }

        /// <summary>
        /// Get employee roles
        /// </summary>
        [HttpGet("{id}/roles")]
        [Authorize(Policy = "ManagementOnly")]
        public async Task<ActionResult> GetEmployeeRoles(Guid id)
        {
            try
            {
                var roles = await _employeeService.GetEmployeeRolesAsync(id);
                return Ok(new { employeeId = id, roles });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving roles for employee {EmployeeId}", id);
                return StatusCode(500, new { message = "An error occurred while retrieving employee roles" });
            }
        }
    }

    // DTOs
  
}
