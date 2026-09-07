using Microsoft.EntityFrameworkCore;
using IIROSA.Domain.Entities;
using IIROSA.Domain.Interfaces;
using IIROSA.Infrastructure.Data;

namespace IIROSA.Infrastructure.Data.Repository;

/// <summary>
/// Provider Repository Implementation
/// Provides data access operations for Provider entity (non-parent guardian)
/// Implements UC-4.6 (Add Non-Parent Provider)
/// </summary>
public class ProviderRepository : Repository<Provider>, IProviderRepository
{
    private readonly DbSet<Provider> _dbSet;

    public ProviderRepository(ApplicationDbContext context) : base(context)
    {
        _dbSet = context.Set<Provider>();
    }

    /// <summary>
    /// Get provider by family ID — the PRIMARY guardian (first live row by CreatedOn/Id).
    /// Deterministic order matters: §11.S.2 housing families can carry several live
    /// guardians, and every legacy seat-semantics caller must see the same primary row.
    /// </summary>
    public async Task<Provider?> GetByFamilyIdAsync(Guid familyId)
    {
        return await IncludeNavigationProperties()
            .Where(p => p.FamilyId == familyId && !p.IsDeleted)
            .OrderBy(p => p.CreatedOn).ThenBy(p => p.Id)
            .FirstOrDefaultAsync();
    }

    /// <summary>
    /// All live providers of a family (§11.S.2 اضافة الاباء — multi-guardian) in
    /// primary-first order. Callers wanting the legacy single seat read the first row.
    /// </summary>
    public async Task<List<Provider>> GetAllByFamilyIdAsync(Guid familyId)
    {
        return await IncludeNavigationProperties()
            .Where(p => p.FamilyId == familyId && !p.IsDeleted)
            .OrderBy(p => p.CreatedOn).ThenBy(p => p.Id)
            .ToListAsync();
    }

    /// <summary>
    /// Check if national ID exists
    /// </summary>
    public async Task<bool> IsNationalIdExistsAsync(string nationalId, Guid? excludeId = null)
    {
        var query = _dbSet.Where(p => p.NationalId == nationalId && !p.IsDeleted);

        if (excludeId.HasValue)
        {
            query = query.Where(p => p.Id != excludeId.Value);
        }

        return await query.AnyAsync();
    }

    /// <summary>
    /// Include navigation properties
    /// </summary>
    public System.Linq.IQueryable<Provider> IncludeNavigationProperties()
    {
        return _dbSet
            .Include(p => p.Family)
            // Refugee register (§12.S.2 اضافة معيل) — nationality + reason-of-relation names
            .Include(p => p.Country)
            .Include(p => p.ReasonOfRelation);
    }
}
