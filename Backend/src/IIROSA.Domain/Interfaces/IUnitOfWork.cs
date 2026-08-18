namespace IIROSA.Domain.Interfaces;

/// <summary>
/// Unit of Work Interface
/// Manages transactions and coordinates repository operations
/// </summary>
public interface IUnitOfWork : IDisposable
{
    // Begin Transaction
    Task BeginTransactionAsync();

    // Commit Transaction
    Task CommitTransactionAsync();

    // Rollback Transaction
    Task RollbackTransactionAsync();

    // Save Changes
    Task<int> SaveChangesAsync();
}
