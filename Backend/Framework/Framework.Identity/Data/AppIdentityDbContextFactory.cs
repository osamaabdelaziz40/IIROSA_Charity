using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Framework.Identity.Data
{
    public class AppIdentityDbContextFactory : IDesignTimeDbContextFactory<AppIdentityDbContext>
    {
        public AppIdentityDbContext CreateDbContext(string[] args)
        {
            // Load configuration from appsettings.json
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true)
                .Build();

            var optionsBuilder = new DbContextOptionsBuilder<AppIdentityDbContext>();

            var connectionString = config.GetConnectionString("DefaultConnection");

            optionsBuilder.UseSqlServer(connectionString);

            // Return context with nulls for services not needed at design-time
            return new AppIdentityDbContext(optionsBuilder.Options, null, null);
        }
    }
}
