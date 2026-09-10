using Microsoft.EntityFrameworkCore;
using IIROSA.Domain.Entities;
using IIROSA.Domain.Interfaces;
using IIROSA.Infrastructure.Data;

namespace IIROSA.Infrastructure.Data.Repository;

/// <summary>
/// Family Phone Repository Implementation
/// Provides data access operations for the FamilyPhone entity (family contact numbers)
/// </summary>
public class FamilyPhoneRepository : Repository<FamilyPhone>, IFamilyPhoneRepository
{
    private readonly DbSet<FamilyPhone> _dbSet;

    public FamilyPhoneRepository(ApplicationDbContext context) : base(context)
    {
        _dbSet = context.Set<FamilyPhone>();
    }

    /// <summary>
    /// All live phone rows of a family, default-first then by creation — the order the
    /// contact card and the edit form present them in.
    /// </summary>
    public async Task<List<FamilyPhone>> GetAllByFamilyIdAsync(Guid familyId)
    {
        return await _dbSet
            .Where(p => p.FamilyId == familyId && !p.IsDeleted)
            .OrderByDescending(p => p.IsDefault)
            .ThenBy(p => p.CreatedOn).ThenBy(p => p.Id)
            .ToListAsync();
    }
}
