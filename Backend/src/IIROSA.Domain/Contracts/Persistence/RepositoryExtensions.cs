using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace IIROSA.Domain.Contracts.Persistence;

/// <summary>
/// Extension methods for IRepository to provide commonly used method names
/// </summary>
public static class RepositoryExtensions
{
    /// <summary>
    /// Add an entity (alias for InsertAsync)
    /// </summary>
    public static async Task<TEntity> AddAsync<TEntity>(this IRepository<TEntity> repository, TEntity entity)
        where TEntity : class
    {
        return await repository.InsertAsync(entity);
    }

    /// <summary>
    /// Add multiple entities (alias for InsertRangeAsync)
    /// </summary>
    public static Task AddRangeAsync<TEntity>(this IRepository<TEntity> repository, IEnumerable<TEntity> entities)
        where TEntity : class
    {
        return repository.InsertRangeAsync(entities);
    }

    /// <summary>
    /// Get entity by Guid ID (converts Guid to object for base method)
    /// </summary>
    public static async Task<TEntity?> GetByIdAsync<TEntity>(this IRepository<TEntity> repository, Guid id)
        where TEntity : class
    {
        return await repository.GetByIdAsync((object)id);
    }

    /// <summary>
    /// Get entity by int ID (converts int to object for base method)
    /// </summary>
    public static async Task<TEntity?> GetByIdAsync<TEntity>(this IRepository<TEntity> repository, int id)
        where TEntity : class
    {
        return await repository.GetByIdAsync((object)id);
    }

    /// <summary>
    /// Save changes to database
    /// This uses the Commit method from the base interface
    /// </summary>
    public static async Task<int> SaveChangesAsync<TEntity>(this IRepository<TEntity> repository)
        where TEntity : class
    {
        var committed = await repository.Commit();
        return committed ? 1 : 0;
    }

    /// <summary>
    /// FirstOrDefault with Guid ID
    /// </summary>
    public static async Task<TEntity?> FirstOrDefaultAsync<TEntity>(this IRepository<TEntity> repository, Guid id)
        where TEntity : class
    {
        return await repository.GetByIdAsync((object)id);
    }
}
