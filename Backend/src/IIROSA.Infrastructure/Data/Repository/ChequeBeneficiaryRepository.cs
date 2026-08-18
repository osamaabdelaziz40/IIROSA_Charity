using IIROSA.Domain.Contracts.Persistence;
using IIROSA.Domain.Entities.Lookups;
using IIROSA.Domain.Interfaces;
using IIROSA.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace IIROSA.Infrastructure.Data.Repository;

/// <summary>
/// ChequeBeneficiary Repository Implementation
/// Provides data access operations for ChequeBeneficiary lookup entity
/// </summary>
public class ChequeBeneficiaryRepository : Repository<ChequeBeneficiary>, IChequeBeneficiaryRepository
{
    private readonly DbSet<ChequeBeneficiary> _dbSet;

    public ChequeBeneficiaryRepository(ApplicationDbContext context) : base(context)
    {
        _dbSet = context.Set<ChequeBeneficiary>();
    }

    public async Task<IEnumerable<ChequeBeneficiary>> GetActiveAsync()
    {
        return await _dbSet
            .Include(b => b.Bank)
            .Where(b => b.IsActive)
            .OrderBy(b => b.Name)
            .ToListAsync();
    }

    public async Task<IEnumerable<ChequeBeneficiary>> SearchByNameAsync(string name)
    {
        return await _dbSet
            .Include(b => b.Bank)
            .Where(b => b.Name.Contains(name) || (b.NameAr != null && b.NameAr.Contains(name)))
            .OrderBy(b => b.Name)
            .ToListAsync();
    }

    public async Task<IEnumerable<ChequeBeneficiary>> GetByTypeAsync(string beneficiaryType)
    {
        return await _dbSet
            .Include(b => b.Bank)
            .Where(b => b.BeneficiaryType == beneficiaryType && b.IsActive)
            .OrderBy(b => b.Name)
            .ToListAsync();
    }
}
