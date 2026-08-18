using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using IIROSA.Infrastructure.Data;
using System.IO;

namespace IIROSA.Infrastructure;

/// <summary>
/// Design-time factory for creating ApplicationDbContext during migrations
/// This avoids the need to resolve all application services at design time
/// </summary>
public class ApplicationDbContextDesignTimeFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        // Get the solution directory and look for appsettings in the API project
        var currentDirectory = Directory.GetCurrentDirectory();
        var apiProjectPath = Path.Combine(Directory.GetCurrentDirectory(), "..", "IIROSA.Api");

        // If we're running from the Infrastructure project, adjust the path
        if (currentDirectory.Contains("IIROSA.Infrastructure"))
        {
            apiProjectPath = Path.Combine(currentDirectory, "..", "IIROSA.Api");
        }

        var configuration = new ConfigurationBuilder()
            .SetBasePath(apiProjectPath)
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .Build();

        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
        optionsBuilder.UseSqlServer(configuration.GetConnectionString("DefaultConnection"));

        return new ApplicationDbContext(optionsBuilder.Options);
    }
}
