using Microsoft.EntityFrameworkCore;
using IIROSA.Domain.Entities;
using IIROSA.Domain.Interfaces;
using IIROSA.Infrastructure.Data;

namespace IIROSA.Infrastructure.Data.Repository;

/// <summary>
/// Father Repository Implementation
/// Provides data access operations for Father entity
/// Implements UC-4.2 (Add Family Father) and UC-4.9 (Update Father Details)
/// </summary>
public class FatherRepository : Repository<Father>, IFatherRepository
{
    private readonly DbSet<Father> _dbSet;

    public FatherRepository(ApplicationDbContext context) : base(context)
    {
        _dbSet = context.Set<Father>();
    }

    /// <summary>
    /// Get father by family ID
    /// </summary>
    public async Task<Father?> GetByFamilyIdAsync(Guid familyId)
    {
        return await IncludeNavigationProperties()
            .FirstOrDefaultAsync(f => f.FamilyId == familyId && !f.IsDeleted);
    }

    /// <summary>
    /// Check if national ID exists
    /// </summary>
    public async Task<bool> IsNationalIdExistsAsync(string nationalId, Guid? excludeId = null)
    {
        var query = _dbSet.Where(f => f.NationalId == nationalId && !f.IsDeleted);

        if (excludeId.HasValue)
        {
            query = query.Where(f => f.Id != excludeId.Value);
        }

        return await query.AnyAsync();
    }

    /// <summary>
    /// Include navigation properties
    /// </summary>
    public System.Linq.IQueryable<Father> IncludeNavigationProperties()
    {
        return _dbSet
            .Include(f => f.Family)
            .Include(f => f.EducationLevel)
            .Include(f => f.HealthStatus);
    }
}
