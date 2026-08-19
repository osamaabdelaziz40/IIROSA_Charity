using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Framework.Identity.Data;
using System;
using System.IO;
using System.Linq;

namespace Framework.Identity
{
    /// <summary>
    /// Design-time factory for creating AppIdentityDbContext during migrations
    /// </summary>
    public class AppIdentityDbContextDesignTimeFactory : IDesignTimeDbContextFactory<AppIdentityDbContext>
    {
        /// <summary>
        /// Where the real configuration lives, relative to the repository root. This project has no
        /// appsettings.json of its own; the API project owns the connection string.
        /// </summary>
        private static readonly string[] SettingsProbePaths =
        {
            Path.Combine("Backend", "src", "IIROSA.Api"),
            Path.Combine("src", "IIROSA.Api"),
            "."
        };

        public AppIdentityDbContext CreateDbContext(string[] args)
        {
            var settingsDirectory = LocateSettingsDirectory()
                ?? throw new InvalidOperationException(
                    "Could not find appsettings.json. It is expected under Backend/src/IIROSA.Api. " +
                    "Run the EF tooling from inside the repository, or pass an explicit connection " +
                    "string with --connection.");

            // ASPNETCORE_ENVIRONMENT is honoured so `database update` targets the same database the
            // API would use when run with the same environment set.
            // Defaults to Production, matching ASP.NET Core. Defaulting to Development here would make
            // `database update` migrate IIROSA_Db_Dev while an API started without the variable set
            // runs against IIROSA_Db — both exist, so neither side reports an error.
            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production";

            var configuration = new ConfigurationBuilder()
                .SetBasePath(settingsDirectory)
                .AddJsonFile("appsettings.json", optional: false)
                .AddJsonFile($"appsettings.{environment}.json", optional: true)
                .AddEnvironmentVariables()
                .Build();

            var connectionString = configuration.GetConnectionString("DefaultConnection");

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException(
                    $"No 'DefaultConnection' connection string found in {settingsDirectory}. " +
                    "Migrations cannot be scaffolded or applied without it.");
            }

            var optionsBuilder = new DbContextOptionsBuilder<AppIdentityDbContext>();
            optionsBuilder.UseSqlServer(connectionString);

            // Create a minimal IHttpContextAccessor and IIdentityTokenManager for design time
            return new AppIdentityDbContext(optionsBuilder.Options, null, null);
        }

        /// <summary>
        /// Finds the directory holding appsettings.json by walking up from the working directory.
        /// </summary>
        /// <remarks>
        /// The EF tooling sets the working directory to whichever project it was invoked against,
        /// which for this class library has no appsettings.json. Walking up to the repository root
        /// and back down to the API project makes the command work from anywhere in the tree
        /// instead of only from the API folder.
        /// </remarks>
        private static string? LocateSettingsDirectory()
        {
            var directory = new DirectoryInfo(Directory.GetCurrentDirectory());

            while (directory != null)
            {
                var match = SettingsProbePaths
                    .Select(relative => Path.GetFullPath(Path.Combine(directory.FullName, relative)))
                    .FirstOrDefault(candidate => File.Exists(Path.Combine(candidate, "appsettings.json")));

                if (match != null)
                {
                    return match;
                }

                directory = directory.Parent;
            }

            return null;
        }
    }
}
