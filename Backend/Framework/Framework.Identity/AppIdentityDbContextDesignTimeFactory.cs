using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Framework.Identity.Data;
using System.IO;

namespace Framework.Identity
{
    /// <summary>
    /// Design-time factory for creating AppIdentityDbContext during migrations
    /// </summary>
    public class AppIdentityDbContextDesignTimeFactory : IDesignTimeDbContextFactory<AppIdentityDbContext>
    {
        public AppIdentityDbContext CreateDbContext(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false)
                .AddJsonFile("appsettings.Development.json", optional: true)
                .Build();

            var optionsBuilder = new DbContextOptionsBuilder<AppIdentityDbContext>();
            optionsBuilder.UseSqlServer(configuration.GetConnectionString("DefaultConnection"));

            // Create a minimal IHttpContextAccessor and IIdentityTokenManager for design time
            return new AppIdentityDbContext(optionsBuilder.Options, null, null);
        }
    }
}
