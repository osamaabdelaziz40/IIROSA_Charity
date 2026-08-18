using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Framework.Core.Data
{
    public interface IBaseDbContext : IDisposable
    {
        string CurrentUserName { get; set; }

        int SaveChanges();

        ChangeTracker ChangeTracker { get; }

        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

        Task<int> SaveChangesWithAuditAsync(CancellationToken cancellationToken = default);

        DbSet<T> Set<T>() where T : class;

        EntityEntry<TEntity> Attach<TEntity>([NotNull] TEntity entity) where TEntity : class;

        EntityEntry<TEntity> Update<TEntity>([NotNull] TEntity entity) where TEntity : class;
    }
}