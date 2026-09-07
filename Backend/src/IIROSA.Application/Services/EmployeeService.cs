using IIROSA.Application.DTOs.EmployeeManagement;
using IIROSA.Application.Interfaces;
using IIROSA.Domain.Entities;
using IIROSA.Domain.Interfaces;
using Framework.Identity.Data.Services.Interfaces;
using Microsoft.Extensions.Logging;
using AutoMapper;

namespace IIROSA.Application.Services;

/// <summary>
/// Employee Service Implementation
/// Implements business logic for Employee management following UC-2.1 to UC-2.7
/// Integrates with Framework.Identity for user account management
/// </summary>
public class EmployeeService : IEmployeeService
{
    private readonly IEmployeeRepository _employeeRepository;
    private readonly IUserAppServiceExtended _userAppService;
    private readonly IRoleAppService _roleAppService;
    private readonly ILogger<EmployeeService> _logger;
    private readonly IMapper _mapper;
    private readonly IDepartmentService _departmentService;

    public EmployeeService(
        IEmployeeRepository employeeRepository,
        IUserAppServiceExtended userAppService,
        IRoleAppService roleAppService,
        ILogger<EmployeeService> logger,
        IMapper mapper,
        IDepartmentService departmentService)
    {
        _employeeRepository = employeeRepository;
        _userAppService = userAppService;
        _roleAppService = roleAppService;
        _logger = logger;
        _mapper = mapper;
        _departmentService = departmentService;
    }

    /// <summary>
    /// Get employees with filtering and pagination (UC-2.5: View Employees List)
    /// </summary>
    public async Task<EmployeePagedResult<EmployeeListDto>> GetEmployeesFilteredAsync(EmployeeFilterDto filter)
    {
        try
        {
            // Department is a navigation the DTOs expose (DepartmentName) — Include it here;
            // the base GetAllAsync materializes bare rows and the name would map as null
            var query = await _employeeRepository.GetAllWithDepartmentAsync();

            // Soft-deleted rows never appear in reads
            var filteredQuery = query.AsQueryable().Where(e => !e.IsDeleted);

            if (!string.IsNullOrWhiteSpace(filter.SearchText))
            {
                var searchText = filter.SearchText.ToLower();
                filteredQuery = filteredQuery
                    .Where(e =>
                        (e.FullName != null && e.FullName.ToLower().Contains(searchText)) ||
                        (e.Code != null && e.Code.ToLower().Contains(searchText)) ||
                        (e.Email != null && e.Email.ToLower().Contains(searchText)) ||
                        (e.PhoneNumber != null && e.PhoneNumber.Contains(searchText)) ||
                        (e.NationalId != null && e.NationalId.Contains(searchText)));
            }

            if (filter.DepartmentId.HasValue)
            {
                filteredQuery = filteredQuery.Where(e => e.DepartmentId == filter.DepartmentId.Value);
            }

            if (!string.IsNullOrWhiteSpace(filter.Position))
            {
                filteredQuery = filteredQuery.Where(e => e.Position == filter.Position);
            }

            if (filter.IsActive.HasValue)
            {
                filteredQuery = filteredQuery.Where(e => e.IsActive == filter.IsActive.Value);
            }

            // Role filter lives in the identity store, not on the Employee row: resolve the
            // users holding the role first, then keep only employees linked to one of them.
            if (!string.IsNullOrWhiteSpace(filter.Role))
            {
                var usersInRole = await _userAppService.GetUsersInRoles(new List<string> { filter.Role });
                var userIds = usersInRole
                    .Where(u => u.Id.HasValue)
                    .Select(u => u.Id!.Value)
                    .ToHashSet();

                filteredQuery = filteredQuery.Where(e => e.FK_UserId.HasValue && userIds.Contains(e.FK_UserId.Value));
            }

            // Get total count
            var totalCount = filteredQuery.Count();

            // Clamp paging inputs — page 0 would produce a negative Skip and throw
            var page = Math.Max(1, filter.Page);
            var pageSize = Math.Max(1, filter.PageSize);

            // Apply pagination
            var pageRows = filteredQuery
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var employeeDtos = _mapper.Map<List<EmployeeListDto>>(pageRows);

            // Load roles for the page only, straight from the rows already in hand
            // (no per-row re-fetch of the employee)
            for (var i = 0; i < pageRows.Count; i++)
            {
                // Roles live on the identity user, so the guard is the account link — an
                // account-linked row with no email still has roles to show
                if (pageRows[i].FK_UserId.HasValue)
                {
                    employeeDtos[i].Roles = await _userAppService.GetUserRolesAsync(pageRows[i].FK_UserId.Value);
                }
            }

            return new EmployeePagedResult<EmployeeListDto>
            {
                Items = employeeDtos,
                TotalCount = totalCount,
                PageNumber = page,
                PageSize = pageSize
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving employees with filter: {@Filter}", filter);
            throw;
        }
    }

    /// <summary>
    /// UC-EMP-02: verify a proposed login name is free before the record is submitted.
    /// In this stack the identity UserName IS the email, so the probe consults both stores
    /// that enforce uniqueness at save time: the Employee table (IsEmailUniqueAsync, the rule
    /// CreateEmployeeAsync applies) and the identity user store (FindByEmailAsync, the rule
    /// CreateUserAsync applies). excludeEmployeeId spares an employee's own account when editing.
    /// </summary>
    public async Task<bool> IsUserNameAvailableAsync(string userName, Guid? excludeEmployeeId = null)
    {
        var name = (userName ?? string.Empty).Trim();

        // Nothing to check, or longer than the column allows — both mean "cannot use this name"
        if (name.Length == 0 || name.Length > 100)
        {
            return false;
        }

        if (!await _employeeRepository.IsEmailUniqueAsync(name, excludeEmployeeId))
        {
            return false;
        }

        var identityMatch = await _userAppService.FindByEmailAsync(name);
        if (identityMatch == null)
        {
            return true;
        }

        // A located account with no readable id is store corruption, not a free name
        if (!identityMatch.Id.HasValue)
        {
            return false;
        }

        // A hit is fine when it is the excluded employee's own account
        if (excludeEmployeeId.HasValue)
        {
            var employee = await _employeeRepository.GetByIdAsync(excludeEmployeeId.Value);
            if (employee != null && employee.FK_UserId == identityMatch.Id.Value)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Get employee by ID (UC-2.7: View Employee Profile)
    /// </summary>
    public async Task<EmployeeDetailDto?> GetEmployeeByIdAsync(Guid id)
    {
        try
        {
            // Include Department — DepartmentName maps from the navigation
            var employee = await _employeeRepository.GetByIdWithDepartmentAsync(id);
            if (employee == null || employee.IsDeleted)
                return null;

            var employeeDto = _mapper.Map<EmployeeDetailDto>(employee);

            // Get employee roles — keyed on the account link, not the email
            if (employee.FK_UserId.HasValue)
            {
                employeeDto.Roles = await _userAppService.GetUserRolesAsync(employee.FK_UserId.Value);
            }

            return employeeDto;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving employee detail for ID: {EmployeeId}", id);
            throw;
        }
    }

    /// <summary>
    /// Create new employee (UC-2.1: Add Employee)
    /// </summary>
    public async Task<EmployeeDetailDto> CreateEmployeeAsync(CreateEmployeeDto dto)
    {
        try
        {
            dto.FullName = (dto.FullName ?? string.Empty).Trim();
            if (dto.FullName.Length == 0)
            {
                throw new InvalidOperationException("Full name is required");
            }

            dto.Email = dto.Email?.Trim();
            dto.Code = (dto.Code ?? string.Empty).Trim();
            dto.Position = dto.Position?.Trim();

            // §9.S.2 mandatory fields — the screen enforces them and so does the endpoint;
            // the account is the point of UC-EMP-03, it is never optional here
            if (string.IsNullOrWhiteSpace(dto.Email))
                throw new InvalidOperationException("Email (login name) is required");
            if (string.IsNullOrWhiteSpace(dto.Password))
                throw new InvalidOperationException("Password is required");
            if (dto.Roles == null || dto.Roles.Count == 0)
                throw new InvalidOperationException("At least one role is required");
            if (string.IsNullOrWhiteSpace(dto.Position))
                throw new InvalidOperationException("Position is required");

            if (dto.DepartmentId.HasValue &&
                await _departmentService.GetLookupByIdAsync(dto.DepartmentId.Value) == null)
            {
                throw new InvalidOperationException($"Department {dto.DepartmentId} does not exist");
            }

            // Login-name availability across BOTH stores (Employee table + identity users),
            // the same rule the UC-EMP-02 check exposes to the screen
            if (!await IsUserNameAvailableAsync(dto.Email))
            {
                throw new InvalidOperationException($"Email '{dto.Email}' is already in use");
            }

            // Code is uniquely indexed; generate one when the screen leaves it blank,
            // re-drawing until it is provably unique
            if (dto.Code.Length == 0)
            {
                do
                {
                    dto.Code = $"EMP-{Guid.NewGuid().ToString("N")[..8].ToUpper()}";
                }
                while (!await _employeeRepository.IsCodeUniqueAsync(dto.Code));
            }
            else if (!await _employeeRepository.IsCodeUniqueAsync(dto.Code))
            {
                throw new InvalidOperationException($"Employee code '{dto.Code}' already exists");
            }

            // The login account is created FIRST and fails loudly. If the Employee insert then
            // fails, the just-created login is deleted again — otherwise it would occupy the
            // email forever and every retry would fail the availability check above.
            Guid? userId = null;
            Employee? employee = null;
            try
            {
                var createUserDto = new Framework.Identity.Data.Services.Interfaces.CreateUserDto
                {
                    Email = dto.Email,
                    FullName = dto.FullName,
                    PhoneNumber = dto.PhoneNumber,
                    Roles = dto.Roles,
                    Password = dto.Password
                };

                var userResult = await _userAppService.CreateUserAsync(createUserDto);
                if (!userResult.Success)
                {
                    throw new InvalidOperationException(
                        $"Could not create the login account for '{dto.Email}': {userResult.VerificationMSG}");
                }
                userId = userResult.InsertedId;

                // Create employee entity, already linked to the account
                employee = _mapper.Map<Employee>(dto);
                employee.IsActive = true;
                employee.FK_UserId = userId;

                await _employeeRepository.AddAsync(employee);
                await _employeeRepository.SaveChangesAsync();

                _logger.LogInformation("Employee created successfully: {EmployeeId} - {Code} - {FullName}",
                    employee.Id, employee.Code, employee.FullName);

                var result = await GetEmployeeByIdAsync(employee.Id);
                if (result == null)
                {
                    throw new InvalidOperationException($"Failed to retrieve newly created employee with ID {employee.Id}");
                }

                return result;
            }
            catch
            {
                if (userId.HasValue)
                {
                    try
                    {
                        await _userAppService.DeleteAsync(userId.Value);
                    }
                    catch (Exception cleanupEx)
                    {
                        _logger.LogError(cleanupEx,
                            "Failed to roll back the login account {UserId} created for '{Email}' — it must be removed manually",
                            userId.Value, dto.Email);
                    }
                }
                throw;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while creating employee: {@Model}", dto);
            throw;
        }
    }

    /// <summary>
    /// Update employee (UC-2.2: Update Employee Information)
    /// </summary>
    public async Task<EmployeeDetailDto> UpdateEmployeeAsync(Guid id, UpdateEmployeeDto dto)
    {
        try
        {
            var employee = await _employeeRepository.GetByIdWithDepartmentAsync(id);
            if (employee == null || employee.IsDeleted)
                throw new KeyNotFoundException($"Employee with ID {id} not found");

            dto.Email = dto.Email?.Trim();
            dto.Code = dto.Code?.Trim();
            dto.FullName = dto.FullName?.Trim();

            // The email is the login credential and FullName a required column — neither may
            // be blanked by a PUT
            if (string.IsNullOrWhiteSpace(dto.Email))
                throw new InvalidOperationException("Email (login name) is required");
            if (string.IsNullOrWhiteSpace(dto.FullName))
                throw new InvalidOperationException("Full name is required");

            if (dto.DepartmentId.HasValue &&
                await _departmentService.GetLookupByIdAsync(dto.DepartmentId.Value) == null)
            {
                throw new InvalidOperationException($"Department {dto.DepartmentId} does not exist");
            }

            // Login-name availability across both stores, sparing this employee's own account
            if (!await IsUserNameAvailableAsync(dto.Email, id))
            {
                throw new InvalidOperationException($"Email '{dto.Email}' is already in use");
            }

            var originalCode = employee.Code;
            if (!string.IsNullOrWhiteSpace(dto.Code) &&
                dto.Code != originalCode &&
                !await _employeeRepository.IsCodeUniqueAsync(dto.Code, id))
            {
                throw new InvalidOperationException($"Employee code '{dto.Code}' already exists");
            }

            // Update employee fields. PUT semantics (review decision 2026-08-24): the edit
            // screen owns the whole record, so an explicit null/empty value is written —
            // clearing a field on screen clears it in the database.
            _mapper.Map(dto, employee);

            // A blank code means "no opinion", not "erase the code" (Code is uniquely indexed)
            if (string.IsNullOrWhiteSpace(employee.Code))
            {
                employee.Code = originalCode;
            }

            _employeeRepository.Update(employee);
            await _employeeRepository.SaveChangesAsync();

            // Mirror profile changes onto the linked login account — the identity UserName IS
            // the email, so an unsynced change would leave the old credential working.
            // A failed mirror is a failed save: report it, never claim success over drift.
            if (employee.FK_UserId.HasValue)
            {
                var mirrorResult = await _userAppService.UpdateUserDetailAsync(employee.FK_UserId.Value,
                    new Framework.Identity.Data.Services.Interfaces.UpdateUserDto
                    {
                        Email = employee.Email,
                        FullName = employee.FullName,
                        PhoneNumber = employee.PhoneNumber
                    });

                if (mirrorResult == Guid.Empty)
                {
                    throw new InvalidOperationException(
                        $"Could not update the login account for '{employee.Email}' — the saved employee and the login may have diverged");
                }

                // Role changes ride along when the request carries a role set (UC-EMP-05)
                if (dto.Roles != null)
                {
                    var currentRoles = await _userAppService.GetUserRolesAsync(employee.FK_UserId.Value);

                    foreach (var removed in currentRoles.Where(r => !dto.Roles.Contains(r)))
                    {
                        if (!await _userAppService.RemoveUserFromRoleAsync(employee.FK_UserId.Value, removed))
                        {
                            throw new InvalidOperationException($"Could not remove role '{removed}' from the login account");
                        }
                    }

                    foreach (var added in dto.Roles.Where(r => !currentRoles.Contains(r)))
                    {
                        if (!await _userAppService.AssignUserToRoleAsync(employee.FK_UserId.Value, added))
                        {
                            throw new InvalidOperationException($"Could not assign role '{added}' to the login account");
                        }
                    }
                }
            }

            _logger.LogInformation("Employee updated successfully: {EmployeeId}", id);

            var result = await GetEmployeeByIdAsync(id);
            if (result == null)
            {
                throw new InvalidOperationException($"Failed to retrieve updated employee with ID {id}");
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating employee {EmployeeId}: {@Model}", id, dto);
            throw;
        }
    }

    /// <summary>
    /// Deactivate employee (UC-2.3: Deactivate Employee)
    /// </summary>
    public async Task DeactivateEmployeeAsync(Guid id)
    {
        try
        {
            var employee = await _employeeRepository.GetByIdAsync(id);
            if (employee == null || employee.IsDeleted)
                throw new InvalidOperationException($"Employee with ID {id} not found");

            employee.IsActive = false;
            _employeeRepository.Update(employee);
            await _employeeRepository.SaveChangesAsync();

            _logger.LogInformation("Employee {EmployeeId} deactivated", id);

            // Also deactivate associated user account
            if (employee.FK_UserId.HasValue)
            {
                try
                {
                    await _userAppService.SetUserActiveStatusAsync(employee.FK_UserId.Value, false);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Could not deactivate user account for employee {EmployeeId}", id);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while deactivating employee {EmployeeId}", id);
            throw;
        }
    }

    /// <summary>
    /// Activate employee
    /// </summary>
    public async Task ActivateEmployeeAsync(Guid id)
    {
        try
        {
            var employee = await _employeeRepository.GetByIdAsync(id);
            if (employee == null || employee.IsDeleted)
                throw new InvalidOperationException($"Employee with ID {id} not found");

            employee.IsActive = true;
            _employeeRepository.Update(employee);
            await _employeeRepository.SaveChangesAsync();

            _logger.LogInformation("Employee {EmployeeId} activated", id);

            // Also activate associated user account
            if (employee.FK_UserId.HasValue)
            {
                try
                {
                    await _userAppService.SetUserActiveStatusAsync(employee.FK_UserId.Value, true);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Could not activate user account for employee {EmployeeId}", id);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while activating employee {EmployeeId}", id);
            throw;
        }
    }

    /// <summary>
    /// Reset employee password (UC-2.4: Reset Employee Password)
    /// </summary>
    public async Task ResetEmployeePasswordAsync(Guid id, string? newPassword = null)
    {
        try
        {
            var employee = await _employeeRepository.GetByIdAsync(id);
            if (employee == null || employee.IsDeleted)
                throw new InvalidOperationException($"Employee with ID {id} not found");

            if (!employee.FK_UserId.HasValue)
            {
                throw new InvalidOperationException("Employee does not have an associated user account");
            }

            var password = newPassword ?? GenerateDefaultPassword();
            var success = await _userAppService.ResetUserPasswordAsync(employee.FK_UserId.Value, password);

            if (!success)
            {
                throw new InvalidOperationException("Failed to reset user password");
            }

            _logger.LogInformation("Password reset for employee {EmployeeId}", id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while resetting password for employee {EmployeeId}", id);
            throw;
        }
    }

    /// <summary>
    /// Assign employee to role (UC-2.6: Assign Employee Role)
    /// </summary>
    public async Task AssignEmployeeRoleAsync(Guid id, string roleName)
    {
        try
        {
            var employee = await _employeeRepository.GetByIdAsync(id);
            if (employee == null || employee.IsDeleted)
                throw new InvalidOperationException($"Employee with ID {id} not found");

            if (!employee.FK_UserId.HasValue)
            {
                throw new InvalidOperationException("Employee does not have an associated user account");
            }

            var success = await _userAppService.AssignUserToRoleAsync(employee.FK_UserId.Value, roleName);

            if (!success)
            {
                throw new InvalidOperationException($"Failed to assign role '{roleName}' to employee");
            }

            _logger.LogInformation("Employee {EmployeeId} assigned to role {RoleName}", id, roleName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while assigning role {RoleName} to employee {EmployeeId}",
                roleName, id);
            throw;
        }
    }

    /// <summary>
    /// Remove employee from role
    /// </summary>
    public async Task RemoveEmployeeRoleAsync(Guid id, string roleName)
    {
        try
        {
            var employee = await _employeeRepository.GetByIdAsync(id);
            if (employee == null || employee.IsDeleted)
                throw new InvalidOperationException($"Employee with ID {id} not found");

            if (!employee.FK_UserId.HasValue)
            {
                throw new InvalidOperationException("Employee does not have an associated user account");
            }

            var success = await _userAppService.RemoveUserFromRoleAsync(employee.FK_UserId.Value, roleName);

            if (!success)
            {
                throw new InvalidOperationException($"Failed to remove role '{roleName}' from employee");
            }

            _logger.LogInformation("Employee {EmployeeId} removed from role {RoleName}", id, roleName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while removing role {RoleName} from employee {EmployeeId}",
                roleName, id);
            throw;
        }
    }

    /// <summary>
    /// Delete employee
    /// </summary>
    public async Task DeleteEmployeeAsync(Guid id)
    {
        try
        {
            var employee = await _employeeRepository.GetByIdAsync(id);
            if (employee == null || employee.IsDeleted)
                throw new InvalidOperationException($"Employee with ID {id} not found");

            _employeeRepository.Delete(employee);
            await _employeeRepository.SaveChangesAsync();

            _logger.LogInformation("Employee {EmployeeId} deleted", id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while deleting employee {EmployeeId}", id);
            throw;
        }
    }

    /// <summary>
    /// Get employee roles
    /// </summary>
    public async Task<List<string>> GetEmployeeRolesAsync(Guid id)
    {
        try
        {
            var employee = await _employeeRepository.GetByIdAsync(id);
            if (employee == null || !employee.FK_UserId.HasValue)
                return new List<string>();

            return await _userAppService.GetUserRolesAsync(employee.FK_UserId.Value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving roles for employee {EmployeeId}", id);
            return new List<string>();
        }
    }

    /// <summary>
    /// Export employees to Excel/CSV (UC-2.5: View Employees List - Export functionality)
    /// </summary>
    public async Task<EmployeePagedResult<EmployeeExportDto>> GetEmployeesForExportAsync(EmployeeFilterDto filter)
    {
        try
        {
            // Reuse the filtering logic
            var result = await GetEmployeesFilteredAsync(new EmployeeFilterDto
            {
                SearchText = filter.SearchText,
                DepartmentId = filter.DepartmentId,
                Position = filter.Position,
                Role = filter.Role,
                IsActive = filter.IsActive,
                Page = 1,
                PageSize = 10000 // Export all matching records
            });

            var exportDtos = _mapper.Map<List<EmployeeExportDto>>(result.Items);

            return new EmployeePagedResult<EmployeeExportDto>
            {
                Items = exportDtos,
                TotalCount = result.TotalCount,
                PageNumber = 1,
                PageSize = result.TotalCount
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while exporting employees with filter: {@Filter}", filter);
            throw;
        }
    }

    /// <summary>
    /// Generate default password
    /// </summary>
    private string GenerateDefaultPassword()
    {
        return "P@ssw0rd@2022";
    }
}
