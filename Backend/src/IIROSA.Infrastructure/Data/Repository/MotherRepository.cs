using Microsoft.EntityFrameworkCore;
using IIROSA.Domain.Entities;
using IIROSA.Domain.Interfaces;
using IIROSA.Infrastructure.Data;

namespace IIROSA.Infrastructure.Data.Repository;

/// <summary>
/// Mother Repository Implementation
/// Provides data access operations for Mother entity
/// Implements UC-4.3 (Add Family Mother) and UC-4.10 (Update Mother Details)
/// </summary>
public class MotherRepository : Repository<Mother>, IMotherRepository
{
    private readonly DbSet<Mother> _dbSet;

    public MotherRepository(ApplicationDbContext context) : base(context)
    {
        _dbSet = context.Set<Mother>();
    }

    /// <summary>
    /// Get mother by family ID
    /// </summary>
    public async Task<Mother?> GetByFamilyIdAsync(Guid familyId)
    {
        return await IncludeNavigationProperties()
            .FirstOrDefaultAsync(m => m.FamilyId == familyId && !m.IsDeleted);
    }

    /// <summary>
    /// Check if national ID exists
    /// </summary>
    public async Task<bool> IsNationalIdExistsAsync(string nationalId, Guid? excludeId = null)
    {
        var query = _dbSet.Where(m => m.NationalId == nationalId && !m.IsDeleted);

        if (excludeId.HasValue)
        {
            query = query.Where(m => m.Id != excludeId.Value);
        }

        return await query.AnyAsync();
    }

    /// <summary>
    /// Include navigation properties
    /// </summary>
    public System.Linq.IQueryable<Mother> IncludeNavigationProperties()
    {
        return _dbSet
            .Include(m => m.Family)
            .Include(m => m.EducationLevel)
            .Include(m => m.HealthStatus);
    }
}
