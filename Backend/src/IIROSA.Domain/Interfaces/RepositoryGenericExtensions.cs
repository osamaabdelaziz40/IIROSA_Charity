using IIROSA.Domain.Contracts.Persistence;

namespace IIROSA.Domain.Interfaces;

/// <summary>
/// Generic extension methods for all IRepository implementations
/// </summary>
public static class RepositoryGenericExtensions
{
    /// <summary>
    /// Get entity by Guid ID - generic extension for any repository
    /// </summary>
    public static async Task<TEntity?> GetByIdAsync<TEntity>(this IRepository<TEntity> repository, Guid id)
        where TEntity : class
    {
        return await repository.GetByIdAsync((object)id);
    }

    /// <summary>
    /// Add entity - generic extension (alias for InsertAsync)
    /// </summary>
    public static async Task<TEntity> AddAsync<TEntity>(this IRepository<TEntity> repository, TEntity entity)
        where TEntity : class
    {
        return await repository.InsertAsync(entity);
    }

    /// <summary>
    /// Add range of entities - generic extension (alias for InsertRangeAsync)
    /// </summary>
    public static Task AddRangeAsync<TEntity>(this IRepository<TEntity> repository, System.Collections.Generic.IEnumerable<TEntity> entities)
        where TEntity : class
    {
        return repository.InsertRangeAsync(entities);
    }

    /// <summary>
    /// Save changes - generic extension (uses Commit from base interface)
    /// </summary>
    public static async Task<int> SaveChangesAsync<TEntity>(this IRepository<TEntity> repository)
        where TEntity : class
    {
        var committed = await repository.Commit();
        return committed ? 1 : 0;
    }
}
