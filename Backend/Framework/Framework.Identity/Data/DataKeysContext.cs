using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Framework.Identity.Data
{
    public class DataKeysContext : DbContext, IDataProtectionKeyContext
    {
        // A recommended constructor overload when using EF Core
        // with dependency injection.
        public DataKeysContext(DbContextOptions<DataKeysContext> options)
            : base(options) { }

        public DbSet<DataProtectionKey> DataProtectionKeys { get; set; }
    }
}