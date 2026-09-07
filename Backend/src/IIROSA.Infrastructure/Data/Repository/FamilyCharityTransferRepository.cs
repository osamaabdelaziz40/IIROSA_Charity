using IIROSA.Domain.Contracts.Persistence;
using IIROSA.Domain.Entities;
using IIROSA.Domain.Interfaces;
using IIROSA.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace IIROSA.Infrastructure.Data.Repository;

/// <summary>
/// FamilyCharityTransfer Repository Implementation (UC-FAM-06)
/// Registered by the Infrastructure assembly scan (any class ending in "Repository"),
/// same as the rest of the repositories — no manual DI line needed.
/// </summary>
public class FamilyCharityTransferRepository : Repository<FamilyCharityTransfer>, IFamilyCharityTransferRepository
{
    private readonly DbSet<FamilyCharityTransfer> _dbSet;

    public FamilyCharityTransferRepository(ApplicationDbContext context) : base(context)
    {
        _dbSet = context.Set<FamilyCharityTransfer>();
    }

    /// <inheritdoc />
    public async Task<IEnumerable<FamilyCharityTransfer>> GetByFamilyIdAsync(Guid familyId)
    {
        return await _dbSet
            .Where(t => t.FamilyId == familyId && !t.IsDeleted)
            .OrderByDescending(t => t.CreatedOn)
            .ToListAsync();
    }
}
