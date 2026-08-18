using AutoMapper;
using FluentValidation;
using Framework.Core.AutoMapper;
using Framework.Core.BackgroundJobs;
using Framework.Core.DependencyManagement;
using Framework.Core.Helper;
using Framework.Core.Notifications;
using Framework.Core.SharedServices.Services;
using Framework.Identity.Data.Services;
using Framework.Identity.Data.Services.Interfaces;
using Hangfire;
using Hangfire.Logging.LogProviders;
using Hangfire.SqlServer;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using IIROSA.Application.Interfaces;

namespace IIROSA.Application
{
    public static class ServiceCollectionExtensions
    {
        public static void ConfigureApplicationServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Register Application Services dynamically (Interface -> Implementation mapping)
            RegisterApplicationServices(services);

            // Register Audit Services (UC-17: Audit Logging Module)
            services.AddScoped<IAuditService, Services.AuditService>();
            // IAuditLogRepository is registered in Infrastructure layer to avoid circular dependency

            // Register Assembly Public Non-Generic Classes ending with "AppService"
            services.RegisterAssemblyPublicNonGenericClasses(Assembly.GetAssembly(typeof(ServiceCollectionExtensions)))
           .Where(c => c.Name.EndsWith("AppService"))
           .AsConcreteTypesScoped();

            // Framework Identity Services
            services.AddScoped<Framework.Identity.Data.Services.Interfaces.IUserAppService, Framework.Identity.Data.Services.UserAppService>();
            services.AddScoped<Framework.Identity.Data.Services.Interfaces.IRefreshTokenService, Framework.Identity.Data.Services.RefreshTokenService>();
            services.AddScoped<Framework.Identity.Data.Services.UserAppService>();
            services.AddScoped<Framework.Identity.Data.Services.RefreshTokenService>();
            services.AddScoped<Framework.Identity.Data.Services.UserRoleAppService>();
            services.AddScoped<Framework.Identity.Data.Services.RoleAppService>();

            #region Identity Layer
            services.AddScoped<IIdentityTokenManager, JwtIdentityTokenManager>();
            #endregion

            // Framework Core Services
            services.AddScoped<NotificationSettings>();
            services.AddScoped<AppSettingsService>();

            // Configuration Settings
            services.Configure<JwtIdentitySettingDto>(configuration.GetSection("JWT"));

            // AutoMapper Configuration
            var config = new MapperConfiguration(cfg =>
            {
                // Get assemblies to scan for profiles
                var assemblies = new[]
                {
                    Assembly.GetExecutingAssembly(), // IIROSA.Application
                    typeof(Framework.Identity.Data.IdentityAutoMapperProfile).Assembly // Framework.Identity
                };

                // Scan all assemblies for profiles
                foreach (var assembly in assemblies)
                {
                    var profiles = assembly.GetTypes()
                        .Where(type => type.IsSubclassOf(typeof(Profile)) &&
                                       type.Name.EndsWith("Profile"));

                    foreach (var profile in profiles)
                    {
                        cfg.AddProfile(profile);
                    }
                }
            });

            AutoMapperConfiguration.Init(config);

            // Register IMapper in DI container
            services.AddScoped<IMapper>(sp => new Mapper(config, sp.GetService));

            // FluentValidation
            services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

            // MediatR
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));
        }

        public static void AddAuth(this IServiceCollection services, IConfiguration configuration)
        {
            var ss = configuration["Jwt:Key"];
            // IMPORTANT: Configure authentication BEFORE Identity to ensure JWT is the default
            services.AddAuthentication(options =>
            {
                // Set JWT Bearer as the DEFAULT scheme for ALL requests
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultForbidScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultSignInScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultSignOutScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                // Try to get JWT settings from multiple possible locations
                var jwtKey = configuration["Jwt:Key"]
                             ?? configuration["Jwt:Key"]
                             ?? configuration["JWT:Key"]
                             ?? "YourSuperSecretKeyHere123456789012";

                var jwtIssuer = configuration["Jwt:Issuer"]
                                ?? configuration["Jwt:Issuer"]
                                ?? configuration["JWT:Issuer"]
                                ?? "IIROSAApi";

                var jwtAudience = configuration["Jwt:Audience"]
                                 ?? configuration["Jwt:Audience"]
                                 ?? configuration["JWT:Audience"]
                                 ?? "IIROSAClient";

                Console.WriteLine($"🔧 JWT Bearer Configuration:");
                Console.WriteLine($"   Key source: {(configuration["Jwt:Key"] != null ? "Jwt" : configuration["Jwt:Key"] != null ? "Jwt" : "Fallback")}");
                Console.WriteLine($"   Key Length: {jwtKey.Length}");
                Console.WriteLine($"   Issuer: {jwtIssuer}");
                Console.WriteLine($"   Audience: {jwtAudience}");

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtIssuer,
                    ValidAudience = jwtAudience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
                    // Important: Map role claims correctly for authorization
                    RoleClaimType = System.Security.Claims.ClaimTypes.Role,
                    NameClaimType = System.Security.Claims.ClaimTypes.Name
                };

                // Add JWT Bearer events for debugging
                options.Events = new Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerEvents
                {
                    OnAuthenticationFailed = context =>
                    {
                        Console.WriteLine($"❌ JWT Authentication Failed: {context.Exception.Message}");
                        return Task.CompletedTask;
                    },
                    OnTokenValidated = context =>
                    {
                        Console.WriteLine($"✅ JWT Token Validated for user: {context.Principal.Identity.Name}");
                        var roles = context.Principal.FindAll(System.Security.Claims.ClaimTypes.Role);
                        Console.WriteLine($"   User Roles: {string.Join(", ", roles.Select(r => r.Value))}");
                        return Task.CompletedTask;
                    },
                    OnChallenge = context =>
                    {
                        Console.WriteLine($"⚠️  JWT Challenge: {context.Request.Path}");
                        return Task.CompletedTask;
                    },
                    OnMessageReceived = context =>
                    {
                        var token = context.Request.Headers["Authorization"].FirstOrDefault();
                        Console.WriteLine($"📨 JWT Message Received: {(!string.IsNullOrEmpty(token) ? "Token present" : "No token")}");
                        return Task.CompletedTask;
                    }
                };
            });

            // Note: Cookie authentication is already configured in IdentityConfigureServices
            // No need to duplicate ConfigureApplicationCookie here

            // Add Authorization with IIROSA role policies
            services.AddAuthorization(options =>
            {
                // IIROSA Role Policies as per UC-1.1
                options.AddPolicy("SuperAdminOnly", policy => policy.RequireRole("SuperAdmin"));
                options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
                options.AddPolicy("CharityOnly", policy => policy.RequireRole("Charity"));
                options.AddPolicy("AccountantOnly", policy => policy.RequireRole("Accountant"));
                options.AddPolicy("FinancialOfficerOnly", policy => policy.RequireRole("FinancialOfficer"));

                // Combined policies for broader access
                options.AddPolicy("ManagementOnly", policy => policy.RequireRole("SuperAdmin", "Admin"));
                options.AddPolicy("FinancialOnly", policy => policy.RequireRole("SuperAdmin", "Admin", "Accountant", "FinancialOfficer"));
                options.AddPolicy("AllRoles", policy => policy.RequireRole("SuperAdmin", "Admin", "Charity", "Accountant", "FinancialOfficer"));

                // Operational policies
                options.AddPolicy("CanManageUsers", policy => policy.RequireRole("SuperAdmin"));
                options.AddPolicy("CanManageRoles", policy => policy.RequireRole("SuperAdmin"));
                options.AddPolicy("CanViewReports", policy => policy.RequireRole("SuperAdmin", "Admin", "Charity", "Accountant", "FinancialOfficer"));
                options.AddPolicy("CanManageFinance", policy => policy.RequireRole("SuperAdmin", "Accountant", "FinancialOfficer"));
            });
        }

        public static void InitHangfire(this IServiceCollection services, string connectionString)
        {
            var sqlStorage = new SqlServerStorage(connectionString);

            JobStorage.Current = sqlStorage;

            services.AddHangfire(config =>
            {
                config.UseLogProvider(new ColouredConsoleLogProvider());

                GlobalConfiguration.Configuration.UseSqlServerStorage(connectionString,
                    new SqlServerStorageOptions
                    {
                        CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
                        SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
                        QueuePollInterval = TimeSpan.Zero,
                        UseRecommendedIsolationLevel = true,
                        DisableGlobalLocks = true
                    });
            });

            services.AddHangfireServer();

            var serviceProvider = services.BuildServiceProvider();
            GlobalConfiguration.Configuration.UseActivator(new HangfireActivator(serviceProvider));

            var backGroundTasks = serviceProvider.GetService<IBackgroundTasks>();
            backGroundTasks?.Init();
        }

        /// <summary>
        /// Dynamically registers all application services with their interfaces
        /// Maps I[ServiceName] to [ServiceName] implementation
        /// </summary>
        private static void RegisterApplicationServices(IServiceCollection services)
        {
            var assembly = Assembly.GetExecutingAssembly();

            // Get all service classes (non-abstract, ending with "Service" but not "AppService")
            var serviceTypes = assembly.GetTypes()
                .Where(t => t.IsClass && !t.IsAbstract && t.IsPublic &&
                           t.Name.EndsWith("Service") && !t.Name.EndsWith("AppService"))
                .ToList();

            foreach (var serviceType in serviceTypes)
            {
                // Find the corresponding interface (I + ClassName)
                var interfaceType = serviceType.GetInterfaces()
                    .FirstOrDefault(i => i.Name == $"I{serviceType.Name}");

                if (interfaceType != null)
                {
                    // Register the interface to implementation mapping
                    services.AddScoped(interfaceType, serviceType);
                }
                else
                {
                    // If no interface found, register as concrete type
                    services.AddScoped(serviceType);
                }
            }
        }
    }
}
