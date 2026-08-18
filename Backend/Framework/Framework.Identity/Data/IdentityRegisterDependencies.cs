using Framework.Core.DependencyManagement;
using Framework.Identity.Data.Entities;
using Framework.Identity.Data.Repositories;
using Framework.Identity.Data.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.Negotiate;
using Microsoft.AspNetCore.Http;
using Framework.Identity.Data.Services;
using Framework.Identity.Data.Extensions;

namespace Framework.Identity.Data
{
    public static class IdentityRegisterDependencies
    {
        public static void IdentityConfigureServices(this IServiceCollection services, string connectionString)
        {
            services.AddDbContext<AppIdentityDbContext>(options =>
                 options.UseSqlServer(connectionString));


            //services.AddIdentity<ApplicationUser, ApplicationRole>()
            //    .AddEntityFrameworkStores<AppIdentityDbContext>()
            //    .AddDefaultTokenProviders();


            services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
            {
                options.Lockout.AllowedForNewUsers = true;  // Lockout enabled for new users
                options.Lockout.MaxFailedAccessAttempts = 5; // Maximum failed attempts before lockout
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(10); // Lockout duration (60 minutes)
            })
            .AddEntityFrameworkStores<AppIdentityDbContext>()
            .AddDefaultTokenProviders();

            // Configure cookie authentication to return 401 for API requests instead of redirecting
            services.ConfigureApplicationCookie(options =>
            {
                // Disable automatic redirect to /Account/Login for API requests
                options.Events.OnRedirectToLogin = context =>
                {
                    // For API requests, return 401 instead of redirecting
                    if (context.Request.Path.StartsWithSegments("/api"))
                    {
                        context.Response.StatusCode = 401;
                        return Task.CompletedTask;
                    }
                    // For non-API requests, use the default redirect behavior
                    context.Response.Redirect(context.RedirectUri);
                    return Task.CompletedTask;
                };

                // Also handle redirect to access denied
                options.Events.OnRedirectToAccessDenied = context =>
                {
                    if (context.Request.Path.StartsWithSegments("/api"))
                    {
                        context.Response.StatusCode = 403;
                        return Task.CompletedTask;
                    }
                    context.Response.Redirect(context.RedirectUri);
                    return Task.CompletedTask;
                };
            });

            //services.AddScoped<UserManager<ApplicationUser>, CustomUserManager<ApplicationUser>>();

            services.AddScoped<UserRepository>();
            services.AddScoped<RoleRepository>();
            services.AddScoped<UserRolesRepository>();
            services.AddScoped<UserTokensRepository>();
            services.AddScoped<IImpersonationSessionRepository, ImpersonationSessionRepository>();
            //services.AddScoped<UserAppService>();
            //services.AddScoped<RoleAppService>();
            services.ConfigureIdentityServices();

            //services.AddScoped<NotificationService>();

            services.Configure<IdentityOptions>(options =>
            {
                // Default Password settings.
                //options.Password.RequireDigit = true;
                //options.Password.RequireLowercase = true;
                //options.Password.RequireNonAlphanumeric = false;
                //options.Password.RequireUppercase = true;
                //options.Password.RequiredLength = 8;
                //options.Password.RequiredUniqueChars = 0;

                // Default SignIn settings.
                //options.SignIn.RequireConfirmedEmail = true;
                //options.SignIn.RequireConfirmedPhoneNumber = false;
                // Lockout settings.


                options.Password.RequireDigit = false;            // No numeric characters required
                options.Password.RequireLowercase = false;        // No lowercase characters required
                options.Password.RequireUppercase = false;        // No uppercase characters required
                options.Password.RequireNonAlphanumeric = false;  // No special characters required
                options.Password.RequiredLength = 1;              // Minimum password length
                options.Password.RequiredUniqueChars = 0;         // No unique characters required



                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.AllowedForNewUsers = true;

                // User settings.
                options.User.RequireUniqueEmail = true;
            });
        }

        public static void AddDataProtection(this IServiceCollection services, string connectionString)
        {
            services.AddDbContext<DataKeysContext>(options =>
                options.UseSqlServer(connectionString));

            services.AddDataProtection()
                .SetApplicationName("DGA-WebApp")
                .PersistKeysToDbContext<DataKeysContext>();

            services.Configure<DataProtectionTokenProviderOptions>(
                options =>
                    options.TokenLifespan = TimeSpan.FromHours(24)
            );
        }

        public static void UseIdentityDBMigration(this IApplicationBuilder app)
        {
            using (var serviceScope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>()
            .CreateScope())
            {
                serviceScope.ServiceProvider.GetService<AppIdentityDbContext>().Database.Migrate();
            }
        }

        public static void UseDataKeysMigration(this IApplicationBuilder app)
        {
            using (var serviceScope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>()
            .CreateScope())
            {
                serviceScope.ServiceProvider.GetService<DataKeysContext>().Database.Migrate();
            }
        }

        public static void ConfigureIdentityServices(this IServiceCollection services)
        {
            //Auto Register App services As Scoped
            //services.RegisterAssemblyPublicNonGenericClasses(Assembly.GetAssembly(typeof(UserAppService)))
            //    .Where(c => c.Name.EndsWith("AppService"))
            //    .AsConcreteTypesScoped();

            services.RegisterAssemblyPublicNonGenericClasses(Assembly.GetAssembly(typeof(IUserAppService)))
                .Where(c => c.Name.EndsWith("AppService"))
                .AsPublicImplementedInterfaces();

            // Manual registration for ImpersonationService (doesn't follow "AppService" naming convention)
            services.AddScoped<Framework.Identity.Data.Services.Interfaces.IImpersonationService, Framework.Identity.Data.Services.ImpersonationService>();
        }
    }
}