using IIROSA.Domain.Contracts.Persistence;
using IIROSA.Domain.Entities;
using IIROSA.Domain.Interfaces;
using IIROSA.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace IIROSA.Infrastructure.Data.Repository;

/// <summary>
/// Check repository implementation (chapter 16, UC-CHQ).
/// Reads never call SaveChanges — only the UnitOfWork persists.
/// </summary>
public class CheckRepository : Repository<Check>, ICheckRepository
{
    private readonly DbSet<Check> _dbSet;

    public CheckRepository(ApplicationDbContext context) : base(context)
    {
        _dbSet = context.Set<Check>();
    }

    /// <inheritdoc />
    public IQueryable<Check> Query()
    {
        return _dbSet
            .Include(c => c.Bank)
            .Include(c => c.ChequeBeneficiary)
            .Include(c => c.Charity);
    }
}
