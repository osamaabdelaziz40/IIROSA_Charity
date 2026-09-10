using IIROSA.Domain.Contracts.Persistence;
using IIROSA.Domain.Entities;
using IIROSA.Domain.Interfaces;
using IIROSA.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace IIROSA.Infrastructure.Data.Repository;

/// <summary>
/// NotificationsLog Repository Implementation (UC-NTF notifications epic).
/// NotificationsLog has no navigation properties to include — the paged read is
/// a flat table scan with filter/order, shared by the admin register and the
/// recipient's my-notifications list.
/// </summary>
public class NotificationsLogRepository : Repository<NotificationsLog>, INotificationsLogRepository
{
    private readonly DbSet<NotificationsLog> _dbSet;

    public NotificationsLogRepository(ApplicationDbContext context) : base(context)
    {
        _dbSet = context.Set<NotificationsLog>();
    }

    /// <summary>Paged read with an optional filter and ordering; newest first by default.</summary>
    public async Task<(IEnumerable<NotificationsLog> Items, int TotalCount)> GetPagedAsync(
        Expression<Func<NotificationsLog, bool>>? filter = null,
        Func<IQueryable<NotificationsLog>, IOrderedQueryable<NotificationsLog>>? orderBy = null,
        int pageNumber = 1,
        int pageSize = 20)
    {
        var query = _dbSet.AsQueryable();

        if (filter != null)
        {
            query = query.Where(filter);
        }

        var totalCount = await query.CountAsync();

        query = orderBy != null
            ? orderBy(query)
            : query.OrderByDescending(n => n.CreatedOn);

        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }
}
