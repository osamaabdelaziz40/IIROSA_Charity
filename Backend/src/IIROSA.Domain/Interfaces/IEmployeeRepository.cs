using IIROSA.Domain.Contracts.Persistence;
using IIROSA.Domain.Entities;

namespace IIROSA.Domain.Interfaces;

/// <summary>
/// Employee Repository Interface
/// Defines data access operations for Employee entity
/// </summary>
public interface IEmployeeRepository : IRepository<Employee>
{
    // Employee-specific queries
    Task<Employee?> GetByEmailAsync(string email);
    Task<Employee?> GetByCodeAsync(string code);
    Task<IEnumerable<Employee>> GetByDepartmentAsync(int departmentId);
    Task<IEnumerable<Employee>> GetActiveEmployeesAsync();
    Task<bool> IsEmailUniqueAsync(string email, Guid? excludeId = null);
    Task<bool> IsCodeUniqueAsync(string code, Guid? excludeId = null);
}
