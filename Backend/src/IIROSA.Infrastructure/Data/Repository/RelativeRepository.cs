using Microsoft.EntityFrameworkCore;
using IIROSA.Domain.Entities;
using IIROSA.Domain.Interfaces;
using IIROSA.Infrastructure.Data;

namespace IIROSA.Infrastructure.Data.Repository;

/// <summary>
/// Relative Repository Implementation
/// Provides data access operations for Relative entity
/// Implements UC-4.7 (Add Family Relative), UC-4.8 (Update Relative Details), UC-4.9 (Remove Relative from Family)
/// </summary>
public class RelativeRepository : Repository<Relative>, IRelativeRepository
{
    private readonly DbSet<Relative> _dbSet;

    public RelativeRepository(ApplicationDbContext context) : base(context)
    {
        _dbSet = context.Set<Relative>();
    }

    /// <summary>
    /// Get all relatives for a specific family
    /// </summary>
    public async Task<IEnumerable<Relative>> GetByFamilyIdAsync(Guid familyId)
    {
        return await IncludeNavigationProperties()
            .Where(r => r.FamilyId == familyId && !r.IsDeleted)
            .OrderBy(r => r.RelationshipType)
            .ThenBy(r => r.FullName)
            .ToListAsync();
    }

    /// <summary>
    /// Get a specific relative by ID
    /// </summary>
    public async Task<Relative?> GetByIdAsync(Guid id)
    {
        return await IncludeNavigationProperties()
            .FirstOrDefaultAsync(r => r.Id == id && !r.IsDeleted);
    }

    /// <summary>
    /// Check if relative belongs to a family
    /// </summary>
    public async Task<bool> BelongsToFamilyAsync(Guid relativeId, Guid familyId)
    {
        return await _dbSet
            .AnyAsync(r => r.Id == relativeId && r.FamilyId == familyId && !r.IsDeleted);
    }

    /// <summary>
    /// Include navigation properties
    /// </summary>
    public System.Linq.IQueryable<Relative> IncludeNavigationProperties()
    {
        return _dbSet
            .Include(r => r.Family)
            .Include(r => r.EducationLevel)
            .Include(r => r.HealthStatus);
    }
}
