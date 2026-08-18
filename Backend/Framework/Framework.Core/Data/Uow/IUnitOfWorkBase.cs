using System.Threading.Tasks;

namespace Framework.Core.Data.Uow
{
    public interface IUnitOfWorkBase<TContext> where TContext : IBaseDbContext
    {
        int SaveChanges();

        Task<int> SaveChangesAsync();
    }
}