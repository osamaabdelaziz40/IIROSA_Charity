using Framework.Core.Helper;
using Framework.Core.SharedServices;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Framework.Core.Data
{
    public class CommonsDbContextFactory : IDesignTimeDbContextFactory<CommonsDbContext>
    {
        public CommonsDbContext CreateDbContext(string[] args)
        {
            // Get config - look for appsettings in the API project
            var currentDir = Directory.GetCurrentDirectory();
            var apiProjectPath = Path.Combine(currentDir, "..", "..", "src", "IIROSA.Api");

            // If running from Framework.Core directory, adjust path
            if (currentDir.Contains("Framework.Core"))
            {
                apiProjectPath = Path.Combine(currentDir, "..", "..", "src", "IIROSA.Api");
            }

            IConfigurationRoot configuration = new ConfigurationBuilder()
                .SetBasePath(apiProjectPath)
                .AddJsonFile("appsettings.json", optional: false)
                .AddJsonFile("appsettings.Development.json", optional: true)
                .Build();

            var connectionString = configuration.GetConnectionString("DefaultConnection");

            var optionsBuilder = new DbContextOptionsBuilder<CommonsDbContext>();
            optionsBuilder.UseSqlServer(connectionString);

            // Provide dummy IHttpContextAccessor and IIdentityTokenManager
            var httpContextAccessor = new HttpContextAccessor();
            var identityTokenManager = new DummyTokenManager(); // you will define this below

            return new CommonsDbContext(optionsBuilder.Options, httpContextAccessor, identityTokenManager);
        }

        // Dummy implementation for design-time only
        private class DummyTokenManager : IIdentityTokenManager
        {
            public string UserId => "design-time-user-id";
            public string UserName => "design-time-user";
            public string AccessToken => "dummy-token";
            public string Email => "user@example.com";
            public string Language => "en";
            public string Country => "EG";
            public bool IsAuthenticated => false;

           
            public string GetCurrentUserName(string token)
            {
                return "design-time-user";
            }
        }
    }
}
