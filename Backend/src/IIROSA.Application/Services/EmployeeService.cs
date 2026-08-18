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

    public EmployeeService(
        IEmployeeRepository employeeRepository,
        IUserAppServiceExtended userAppService,
        IRoleAppService roleAppService,
        ILogger<EmployeeService> logger,
        IMapper mapper)
    {
        _employeeRepository = employeeRepository;
        _userAppService = userAppService;
        _roleAppService = roleAppService;
        _logger = logger;
        _mapper = mapper;
    }

    /// <summary>
    /// Get employees with filtering and pagination (UC-2.5: View Employees List)
    /// </summary>
    public async Task<EmployeePagedResult<EmployeeListDto>> GetEmployeesFilteredAsync(EmployeeFilterDto filter)
    {
        try
        {
            var query = await _employeeRepository.GetAllAsync();

            // Apply filters
            var filteredQuery = query.AsQueryable();

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

            if (filter.IsActive.HasValue)
            {
                filteredQuery = filteredQuery.Where(e => e.IsActive == filter.IsActive.Value);
            }

            // Get total count
            var totalCount = filteredQuery.Count();

            // Apply pagination
            var paginatedQuery = filteredQuery
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize);

            var employeeDtos = _mapper.Map<List<EmployeeListDto>>(paginatedQuery);

            // Load roles for each employee
            foreach (var employeeDto in employeeDtos)
            {
                var employee = await _employeeRepository.GetByIdAsync(employeeDto.Id);
                if (employee != null && !string.IsNullOrWhiteSpace(employee.Email))
                {
                    employeeDto.Roles = await GetEmployeeRolesAsync(employeeDto.Id);
                }
            }

            return new EmployeePagedResult<EmployeeListDto>
            {
                Items = employeeDtos,
                TotalCount = totalCount,
                PageNumber = filter.Page,
                PageSize = filter.PageSize
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving employees with filter: {@Filter}", filter);
            throw;
        }
    }

    /// <summary>
    /// Get employee by ID (UC-2.7: View Employee Profile)
    /// </summary>
    public async Task<EmployeeDetailDto?> GetEmployeeByIdAsync(Guid id)
    {
        try
        {
            var employee = await _employeeRepository.GetByIdAsync(id);
            if (employee == null)
                return null;

            var employeeDto = _mapper.Map<EmployeeDetailDto>(employee);

            // Get employee roles
            if (!string.IsNullOrWhiteSpace(employee.Email))
            {
                employeeDto.Roles = await GetEmployeeRolesAsync(id);
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
            // Validate email uniqueness
            if (!string.IsNullOrWhiteSpace(dto.Email) &&
                !await _employeeRepository.IsEmailUniqueAsync(dto.Email))
            {
                throw new InvalidOperationException($"Email '{dto.Email}' is already in use");
            }

            // Validate code uniqueness
            if (!string.IsNullOrWhiteSpace(dto.Code) &&
                !await _employeeRepository.IsCodeUniqueAsync(dto.Code))
            {
                throw new InvalidOperationException($"Employee code '{dto.Code}' already exists");
            }

            // Create employee entity
            var employee = _mapper.Map<Employee>(dto);
            employee.IsActive = true;

            await _employeeRepository.AddAsync(employee);
            await _employeeRepository.SaveChangesAsync();

            // Create associated user account if email and password are provided
            if (!string.IsNullOrWhiteSpace(dto.Email) && !string.IsNullOrWhiteSpace(dto.Password))
            {
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
                    if (userResult.Success)
                    {
                        // Link employee to user
                        employee.FK_UserId = userResult.InsertedId;
                        _employeeRepository.Update(employee);
                        await _employeeRepository.SaveChangesAsync();
                    }
                    else
                    {
                        _logger.LogWarning("User account creation failed for employee {EmployeeId}: {Message}",
                            employee.Id, userResult.VerificationMSG);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error creating user account for employee {EmployeeId}", employee.Id);
                }
            }

            _logger.LogInformation("Employee created successfully: {EmployeeId} - {Code} - {FullName}",
                employee.Id, employee.Code, employee.FullName);

            var result = await GetEmployeeByIdAsync(employee.Id);
            if (result == null)
            {
                throw new InvalidOperationException($"Failed to retrieve newly created employee with ID {employee.Id}");
            }

            return result;
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
            var employee = await _employeeRepository.GetByIdAsync(id);
            if (employee == null)
                throw new InvalidOperationException($"Employee with ID {id} not found");

            // Validate email uniqueness (excluding current employee)
            if (!string.IsNullOrWhiteSpace(dto.Email) &&
                !await _employeeRepository.IsEmailUniqueAsync(dto.Email, id))
            {
                throw new InvalidOperationException($"Email '{dto.Email}' is already in use");
            }

            // Update employee fields
            _mapper.Map(dto, employee);
            _employeeRepository.Update(employee);
            await _employeeRepository.SaveChangesAsync();

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
            if (employee == null)
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
            if (employee == null)
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
            if (employee == null)
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
            if (employee == null)
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
            if (employee == null)
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
            if (employee == null)
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

            var allUsers = await _userAppService.GetUsersFilteredAsync(
                new Framework.Identity.Data.Services.Interfaces.UserFilterDto());
            var associatedUser = allUsers.Items.FirstOrDefault(u => u.Id == employee.FK_UserId.Value);

            if (associatedUser == null)
                return new List<string>();

            var userDetail = await _userAppService.GetUserDetailAsync(associatedUser.Id);
            return userDetail?.Roles?.ToList() ?? new List<string>();
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
                Department = filter.Department,
                Position = filter.Position,
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
