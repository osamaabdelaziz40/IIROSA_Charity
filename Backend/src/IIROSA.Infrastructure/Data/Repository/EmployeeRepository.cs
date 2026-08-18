using IIROSA.Domain.Contracts.Persistence;
using IIROSA.Domain.Entities;
using IIROSA.Domain.Interfaces;
using IIROSA.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace IIROSA.Infrastructure.Data.Repository;

/// <summary>
/// Employee Repository Implementation
/// Provides data access operations for Employee entity using ApplicationDbContext
/// </summary>
public class EmployeeRepository : Repository<Employee>, IEmployeeRepository
{
    private readonly DbSet<Employee> _dbSet;

    public EmployeeRepository(ApplicationDbContext context) : base(context)
    {
        _dbSet = context.Set<Employee>();
    }

    public async Task<Employee?> GetByEmailAsync(string email)
    {
        return await _dbSet
            .Include(e => e.Department)
            .FirstOrDefaultAsync(e => e.Email == email);
    }

    public async Task<Employee?> GetByCodeAsync(string code)
    {
        return await _dbSet
            .Include(e => e.Department)
            .FirstOrDefaultAsync(e => e.Code == code);
    }

    public async Task<IEnumerable<Employee>> GetByDepartmentAsync(int departmentId)
    {
        return await _dbSet
            .Include(e => e.Department)
            .Where(e => e.DepartmentId == departmentId)
            .ToListAsync();
    }

    public async Task<IEnumerable<Employee>> GetActiveEmployeesAsync()
    {
        return await _dbSet
            .Include(e => e.Department)
            .Where(e => e.IsActive)
            .OrderBy(e => e.FullName)
            .ToListAsync();
    }

    public async Task<bool> IsEmailUniqueAsync(string email, Guid? excludeId = null)
    {
        var query = _dbSet.Where(e => e.Email == email);

        if (excludeId.HasValue)
        {
            query = query.Where(e => e.Id != excludeId.Value);
        }

        return !await query.AnyAsync();
    }

    public async Task<bool> IsCodeUniqueAsync(string code, Guid? excludeId = null)
    {
        var query = _dbSet.Where(e => e.Code == code);

        if (excludeId.HasValue)
        {
            query = query.Where(e => e.Id != excludeId.Value);
        }

        return !await query.AnyAsync();
    }
}
