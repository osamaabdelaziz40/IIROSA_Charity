using System.Linq.Expressions;
using IIROSA.Domain.Contracts.Persistence;
using IIROSA.Domain.Entities;

namespace IIROSA.Domain.Interfaces;

/// <summary>
/// Repository contract for NotificationsLog (UC-NTF notifications epic).
/// </summary>
public interface INotificationsLogRepository : IRepository<NotificationsLog>
{
    /// <summary>
    /// Paged read with an optional filter and ordering — both list reads (the
    /// admin register and the recipient's my-notifications) share it.
    /// </summary>
    Task<(IEnumerable<NotificationsLog> Items, int TotalCount)> GetPagedAsync(
        Expression<Func<NotificationsLog, bool>>? filter = null,
        Func<IQueryable<NotificationsLog>, IOrderedQueryable<NotificationsLog>>? orderBy = null,
        int pageNumber = 1,
        int pageSize = 20);
}
