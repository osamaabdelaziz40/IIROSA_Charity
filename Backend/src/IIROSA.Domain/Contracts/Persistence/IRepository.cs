using Framework.Core.Data.Repositories;
using IIROSA.Domain.Contracts;

namespace IIROSA.Domain.Contracts.Persistence;

/// <summary>
/// Generic Repository Interface for Application Entities
/// Extends the Framework's RepositoryBase with IAppDbContext
/// </summary>
/// <typeparam name="TEntity">The entity type</typeparam>
public interface IRepository<TEntity> : IRepositoryBase<IAppDbContext, TEntity>
    where TEntity : class
{
}
