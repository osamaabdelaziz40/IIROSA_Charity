using Framework.Core.Caching;
using Framework.Core.Data.Repositories;
using Framework.Core.Notifications;
using Framework.Core.SharedServices.Services;
using Framework.Identity.Data.Entities;
using Framework.Identity.Data.Services;
using IIROSA.Domain.Contracts;
using IIROSA.Domain.Contracts.Persistence;
using IIROSA.Domain.Interfaces;
using IIROSA.Infrastructure.Data;
using IIROSA.Infrastructure.Data.Repository;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace IIROSA.Infrastructure.Extensions;

/// <summary>
/// Service collection extensions for Dependency Injection
/// </summary>
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // HttpContextAccessor
        services.AddHttpContextAccessor();

        // Memory Cache
        services.AddMemoryCache();
        services.AddScoped<Microsoft.Extensions.Caching.Memory.IMemoryCache, Microsoft.Extensions.Caching.Memory.MemoryCache>();

        // Identity Token Manager (required by Framework DbContexts)
        services.AddScoped<Framework.Core.Helper.IIdentityTokenManager, Framework.Core.Helper.JwtIdentityTokenManager>();

        // Register all three DbContexts with Audit Logging Interceptor
        services.AddDbContext<ApplicationDbContext>((sp, options) =>
        {
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"))
                   .EnableSensitiveDataLogging()
                   .AddInterceptors(sp.GetRequiredService<IIROSA.Infrastructure.Data.Interceptors.AuditLogSaveChangesInterceptor>());
        });

        // CommonsDbContext for Framework entities (Attachment, SystemSetting, NotificationTemplate, AuditLog)
        services.AddDbContext<Framework.Core.SharedServices.CommonsDbContext>((sp, options) =>
        {
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"))
                   .EnableSensitiveDataLogging();
        });
        services.AddScoped<Framework.Core.SharedServices.ICommonsDbContext, Framework.Core.SharedServices.CommonsDbContext>();

        // AppIdentityDbContext for User/Role management
        services.AddDbContext<Framework.Identity.Data.AppIdentityDbContext>((sp, options) =>
        {
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"))
                   .EnableSensitiveDataLogging();
        });

        // Register Audit Logging Interceptor (UC-17: Audit Logging Module)
        services.AddScoped<IIROSA.Infrastructure.Data.Interceptors.AuditLogSaveChangesInterceptor>();

        //services.AddScoped<ITermsAndConditionRepository, TermsAndConditionRepository>();
        //services.AddScoped<ICompanyProfileRequestRepository, CompanyProfileRequestRepository>();

        //Genaric registeration of services
        Assembly.GetExecutingAssembly()
                .GetTypes()
                .Where(a => a.Name.EndsWith("Repository") && !a.IsAbstract && !a.IsInterface)
                .Select(a => new { assignedType = a, serviceTypes = a.GetInterfaces().ToList() })
                .ToList()
                .ForEach(typesToRegister =>
                {
                    typesToRegister.serviceTypes.ForEach(typeToRegister => services.AddScoped(typeToRegister, typesToRegister.assignedType));
                });
        services.AddScoped<IAppDbContext, ApplicationDbContext>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));

        //// 1. CommonsDbContext - for Framework entities (Attachment, SystemSetting, NotificationTemplate, etc.)
        //services.AddDbContext<Framework.Core.SharedServices.CommonsDbContext>(options =>
        //    options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));
        //services.AddScoped<Framework.Core.SharedServices.ICommonsDbContext, Framework.Core.SharedServices.CommonsDbContext>();

        //// 2. AppIdentityDbContext - for User/Role management
        //services.AddDbContext<Framework.Identity.Data.AppIdentityDbContext>(options =>
        //    options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        //// 3. ApplicationDbContext - for business entities only (Charity, Family, Employee, etc.)
        //services.AddDbContext<ApplicationDbContext>(options =>
        //    options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        // Unit of Work
        //services.AddScoped<IIROSA.Domain.Interfaces.IUnitOfWork, IIROSA.Infrastructure.Data.UnitOfWork>();

        // Identity - using AppIdentityDbContext
        //services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
        //{
        //    // Password settings
        //    options.Password.RequireDigit = true;
        //    options.Password.RequireLowercase = true;
        //    options.Password.RequireUppercase = false;
        //    options.Password.RequireNonAlphanumeric = false;
        //    options.Password.RequiredLength = 6;

        //    // User settings
        //    options.User.RequireUniqueEmail = true;
        //    options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";

        //    // Lockout settings
        //    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
        //    options.Lockout.MaxFailedAccessAttempts = 5;
        //    options.Lockout.AllowedForNewUsers = true;
        //})
        //.AddEntityFrameworkStores<Framework.Identity.Data.AppIdentityDbContext>()
        //.AddDefaultTokenProviders();

        //// Framework Core Services (Dynamic registration - will be auto-registered in Program.cs)
        //services.AddScoped(typeof(IRepositoryBase<,>), typeof(RepositoryBase<,>));
        //services.AddScoped<INotificationsManager, NotificationsManager>();
        //services.AddScoped<ICacheManager, MemoryCacheManager>();

        //// Framework Identity Services - Explicit registration to ensure DI resolution
        //try
        //{
        //    services.AddScoped<Framework.Identity.Data.Services.UserAppService>();
        //    services.AddScoped<Framework.Identity.Data.Services.Interfaces.IUserAppService, Framework.Identity.Data.Services.UserAppService>();
        //}
        //catch (Exception ex)
        //{
        //    // Log error but don't prevent application startup
        //    Console.WriteLine($"⚠️  Warning: Could not register UserAppService: {ex.Message}");
        //}

        // Seed Data Initializer
        services.AddScoped<IIROSA.Infrastructure.Data.SeedData.IIROSASeedDataInitializer>();

        // Framework Identity Repositories (Dynamic registration - will be auto-registered in Program.cs)
        // Note: Manual registration kept here for backward compatibility, but dynamic registration in Program.cs will override/add to these

        // Framework Identity Services (Dynamic registration - will be auto-registered in Program.cs)
        // Note: Manual registration kept here for critical services, but dynamic registration in Program.cs will handle most

        // Audit Logging Services (UC-17: Audit Logging Module)
        // Registered here to avoid circular dependency with Application layer
        services.AddScoped<IIROSA.Application.Interfaces.IAuditLogRepository, IIROSA.Infrastructure.Data.Repository.AuditLogRepository>();

        return services;
    }

    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // Custom Token Service for JWT generation
        services.AddScoped<IIROSA.Application.Services.ITokenService, IIROSA.Application.Services.TokenService>();

        // Application Services (Business Logic Layer)
        // Employee Management Service (following approved architecture)
        services.AddScoped<IIROSA.Application.Interfaces.IEmployeeService, IIROSA.Application.Services.EmployeeService>();

        // Mission Management Service (UC-8.1 to UC-8.13)
        services.AddScoped<IIROSA.Application.Interfaces.IMissionService, IIROSA.Application.Services.MissionService>();

        // Office Project Management Service (UC-7.1 to UC-7.14)
        services.AddScoped<IIROSA.Application.Interfaces.IOfficeProjectService, IIROSA.Application.Services.OfficeProjectService>();

        // Lookup Management Services (UC-14)
        services.AddScoped<IIROSA.Application.Interfaces.ICountryService, IIROSA.Application.Services.CountryService>();
        services.AddScoped<IIROSA.Application.Interfaces.IRegionService, IIROSA.Application.Services.RegionService>();
        services.AddScoped<IIROSA.Application.Interfaces.ICenterService, IIROSA.Application.Services.CenterService>();
        services.AddScoped<IIROSA.Application.Interfaces.IDepartmentService, IIROSA.Application.Services.DepartmentService>();
        services.AddScoped<IIROSA.Application.Interfaces.IMissionTypeService, IIROSA.Application.Services.MissionTypeService>();
        services.AddScoped<IIROSA.Application.Interfaces.IProjectTypeService, IIROSA.Application.Services.ProjectTypeService>();
        services.AddScoped<IIROSA.Application.Interfaces.IOfficeProjectTypeService, IIROSA.Application.Services.OfficeProjectTypeService>();
        services.AddScoped<IIROSA.Application.Interfaces.IBankService, IIROSA.Application.Services.BankService>();
        services.AddScoped<IIROSA.Application.Interfaces.INGOTypeService, IIROSA.Application.Services.NGOTypeService>();
        services.AddScoped<IIROSA.Application.Interfaces.ILookupManagementService, IIROSA.Application.Services.LookupManagementService>();

        // Technical Support Services (UC-13.1 through UC-13.10)
        services.AddScoped<IIROSA.Application.Interfaces.ISupportTicketService, IIROSA.Application.Services.SupportTicketService>();

        // Incoming & Outgoing Correspondence Services (UC-12.1 through UC-12.14)
        services.AddScoped<IIROSA.Application.Interfaces.IIncomingService, IIROSA.Application.Services.IncomingService>();
        services.AddScoped<IIROSA.Application.Interfaces.IOutgoingService, IIROSA.Application.Services.OutgoingService>();

        // Register repositories
        services.AddScoped<IIROSA.Domain.Interfaces.IEmployeeRepository, IIROSA.Infrastructure.Data.Repository.EmployeeRepository>();

        // Mission Management Repository
        services.AddScoped<IIROSA.Domain.Interfaces.IMissionRepository, IIROSA.Infrastructure.Data.Repository.MissionRepository>();

        // Office Project Management Repository (UC-7.1 to UC-7.14)
        services.AddScoped<IIROSA.Domain.Interfaces.IOfficeProjectRepository, IIROSA.Infrastructure.Data.Repository.OfficeProjectRepository>();

        // Register Lookup repositories
        services.AddScoped<IIROSA.Domain.Interfaces.ICountryRepository, IIROSA.Infrastructure.Data.Repository.CountryRepository>();
        services.AddScoped<IIROSA.Domain.Interfaces.IRegionRepository, IIROSA.Infrastructure.Data.Repository.RegionRepository>();
        services.AddScoped<IIROSA.Domain.Interfaces.ICenterRepository, IIROSA.Infrastructure.Data.Repository.CenterRepository>();
        services.AddScoped<IIROSA.Domain.Interfaces.IDepartmentRepository, IIROSA.Infrastructure.Data.Repository.DepartmentRepository>();
        services.AddScoped<IIROSA.Domain.Interfaces.IMissionTypeRepository, IIROSA.Infrastructure.Data.Repository.MissionTypeRepository>();
        services.AddScoped<IIROSA.Domain.Interfaces.IProjectTypeRepository, IIROSA.Infrastructure.Data.Repository.ProjectTypeRepository>();
        services.AddScoped<IIROSA.Domain.Interfaces.IOfficeProjectTypeRepository, IIROSA.Infrastructure.Data.Repository.OfficeProjectTypeRepository>();
        services.AddScoped<IIROSA.Domain.Interfaces.IBankRepository, IIROSA.Infrastructure.Data.Repository.BankRepository>();
        services.AddScoped<IIROSA.Domain.Interfaces.INGOTypeRepository, IIROSA.Infrastructure.Data.Repository.NGOTypeRepository>();

        // Technical Support repositories (UC-13.1 through UC-13.10)
        services.AddScoped<IIROSA.Domain.Interfaces.ISupportTicketRepository, IIROSA.Infrastructure.Data.Repository.SupportTicketRepository>();
        services.AddScoped<IIROSA.Domain.Interfaces.ITicketResponseRepository, IIROSA.Infrastructure.Data.Repository.TicketResponseRepository>();

        // Incoming & Outgoing Correspondence Repositories (UC-12.1 through UC-12.14)
        services.AddScoped<IIROSA.Domain.Interfaces.IIncomingRepository, IIROSA.Infrastructure.Data.Repository.IncomingRepository>();
        services.AddScoped<IIROSA.Domain.Interfaces.IOutgoingRepository, IIROSA.Infrastructure.Data.Repository.OutgoingRepository>();

        // Periodic Orphan Reports Services (UC-6.11 through UC-6.17)
        // NOTE: Service temporarily excluded due to Infrastructure layer dependency
        // TODO: Move service implementation to Infrastructure layer or create interface-based architecture
        // services.AddScoped<IIROSA.Application.Interfaces.IPeriodicOrphanReportService, IIROSA.Application.Services.PeriodicOrphanReportService>();

        // Orphan Summary Reports Services (UC-6.1 through UC-6.10)
        // NOTE: Service temporarily excluded due to Infrastructure layer dependency
        // TODO: Move service implementation to Infrastructure layer or create interface-based architecture
        // services.AddScoped<IIROSA.Application.Interfaces.IOrphanReportService, IIROSA.Application.Services.OrphanReportService>();

        // TODO: Add UserManagement service once implemented
        // services.AddScoped<IIROSA.Application.Services.UserManagement.IUserManagementService, IIROSA.Application.Services.UserManagement.UserManagementService>();


        return services;
    }
}
