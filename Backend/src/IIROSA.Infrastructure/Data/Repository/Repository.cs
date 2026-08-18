using Framework.Core.Data.Repositories;
using IIROSA.Domain.Contracts;
using IIROSA.Domain.Contracts.Persistence;

namespace IIROSA.Infrastructure.Data.Repository;

/// <summary>
/// Generic Repository Implementation for Application Entities
/// Extends the Framework's RepositoryBase with IAppDbContext
/// </summary>
/// <typeparam name="TEntity">The entity type</typeparam>
public class Repository<TEntity> : RepositoryBase<IAppDbContext, TEntity>, IRepository<TEntity>
    where TEntity : class
{
    public Repository(IAppDbContext dbContext) : base(dbContext)
    {
    }
}
