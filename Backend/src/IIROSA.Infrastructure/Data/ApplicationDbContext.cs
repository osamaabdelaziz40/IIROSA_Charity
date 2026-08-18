using Framework.Core.Data;
using Framework.Core.Data.Mapping;
using Framework.Core.Helper;
using Framework.Core.SharedServices.Seed;
using IIROSA.Domain.Contracts;
using IIROSA.Domain.Entities.Base;
using IIROSA.Domain.Entities.TechnicalSupport.Lookups;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Reflection;

namespace IIROSA.Infrastructure.Data;

/// <summary>
/// Application Database Context
/// Manages BUSINESS ENTITIES ONLY from IIROSA.Domain
/// Entities are auto-discovered from IIROSA.Domain assemblies - NO explicit DbSets needed
///
/// NOTE: This context does NOT manage:
/// - User/Role management (managed by AppIdentityDbContext in Framework.Identity)
/// - Framework entities like Attachment, SystemSetting, NotificationTemplate (managed by CommonsDbContext in Framework.Core)
/// </summary>
public class ApplicationDbContext : BaseDbContext<ApplicationDbContext>, IAppDbContext
{

    // Design-time constructor for migrations
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options, null, null)
    {
    }

    // Runtime constructor with full dependencies
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, IHttpContextAccessor httpContextAccessor, IIdentityTokenManager identityTokenManager) : base(options, httpContextAccessor, identityTokenManager)
    {
        //CurrentUserName = httpContextAccessor?.HttpContext?.User?.Identity?.Name;
        //        CurrentUserName = ((System.Security.Claims.ClaimsIdentity)httpContextAccessor?.HttpContext?.User?.Identity)?.
        //FindFirst("fullName")?.Value;

        CurrentUserName = httpContextAccessor?.HttpContext?.User?.Identity?.Name;
        var token = httpContextAccessor?.HttpContext?.Request.Headers.Authorization;
        if (token.HasValue)
        {
            CurrentUserName = string.IsNullOrEmpty(token.Value) ? "" : identityTokenManager.GetCurrentUserName(token.Value.ToString());

        }
    }
    //protected override void OnModelCreating(ModelBuilder modelBuilder)
    //{
    //    //dynamically load all entity and query type configurations
    //    var typeConfigurations = Assembly.GetExecutingAssembly().GetTypes().Where(type =>
    //        (type.BaseType?.IsGenericType ?? false)
    //        && (type.BaseType.GetGenericTypeDefinition() == typeof(EntityTypeConfiguration<>)));

    //    foreach (var typeConfiguration in typeConfigurations)
    //    {
    //        var configuration = (IMappingConfiguration)Activator.CreateInstance(typeConfiguration);
    //        configuration.ApplyConfiguration(modelBuilder);
    //    }


    //    modelBuilder.AddSeadData();
    //    //modelBuilder.ApplyConfiguration(new VW_RequestAttachmentConfiguration());
    //    base.OnModelCreating(modelBuilder);
    //}
    // Explicit DbSets for Technical Support lookup entities (needed for SeedData)
    public DbSet<SupportTicketCategory> SupportTicketCategories { get; set; }
    public DbSet<SupportTicketPriority> SupportTicketPriorities { get; set; }
    public DbSet<SupportTicketStatus> SupportTicketStatuses { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Dynamically discover and register all entities from IIROSA.Domain assembly
        // This finds all entity types that inherit from EntityBase and registers them automatically
        var entityTypes = typeof(FullAuditedEntity).Assembly.GetTypes()
            .Where(type => type.IsClass && !type.IsAbstract && type.IsSubclassOf(typeof(object)))
            .Where(type => type.GetProperty("Id") != null) // Has Id property (entity)
            .Where(type => !type.IsGenericType) // Exclude generic types
            .Where(type => type.Namespace != null &&
                         type.Namespace.StartsWith("IIROSA.Domain.Entities") &&
                         !type.Namespace.StartsWith("IIROSA.Domain.Entities.Framework")) // Exclude Framework entities
            .ToList();

        foreach (var entityType in entityTypes)
        {
            builder.Entity(entityType);
        }

        // Auto-discover and apply all entity configurations from IIROSA.Domain assembly
        builder.ApplyConfigurationsFromAssembly(typeof(FullAuditedEntity).Assembly);

        // Note: Framework entities are configured in their respective DbContexts:
        // - User/Role management: AppIdentityDbContext (Framework.Identity)
        // - Attachment, SystemSetting, NotificationTemplate: CommonsDbContext (Framework.Core)
    }
}
