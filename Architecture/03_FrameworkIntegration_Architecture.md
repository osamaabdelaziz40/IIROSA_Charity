# Project Architecture - Framework Integration (Corrected)

## Solution Structure with Framework Projects

```
IIROSA.System.sln
│
├── Framework/
│   ├── Framework.Core/                      # Core Framework Project
│   │   ├── Settings/                        # Settings Management
│   │   │   ├── Setting.cs                    # Setting Entity
│   │   │   ├── ISettingService.cs
│   │   │   └── SettingService.cs
│   │   ├── Notifications/                   # Notifications System
│   │   │   ├── Notification.cs
│   │   │   ├── NotificationSettings.cs
│   │   │   ├── INotificationService.cs
│   │   │   └── NotificationService.cs
│   │   ├── Attachments/                     # File Attachments
│   │   │   ├── Attachment.cs
│   │   │   ├── IAttachmentService.cs
│   │   │   └── AttachmentService.cs
│   │   ├── Audit/                           # Audit Trail
│   │   │   ├── AuditLog.cs
│   │   │   └── IAuditLogService.cs
│   │   ├── BaseEntities/                    # Base Entity Classes
│   │   │   ├── FullAuditedEntityBase.cs      # Base for main entities (Guid)
│   │   │   ├── FullAuditedEntityBaseInt.cs    # Base for lookup entities (int)
│   │   │   ├── LookupEntityBase.cs          # Base for lookups
│   │   │   └── EntityBase.cs                 # Base for all entities
│   │   ├── Interfaces/
│   │   │   ├── IRepository.cs
│   │   │   ├── IUnitOfWork.cs
│   │   │   └── ICacheService.cs
│   │   └── Framework.Core.csproj
│   │
│   └── Framework.Identity/                 # Identity Framework Project
│       ├── Identity/                         # ASP.NET Identity
│       │   ├── Models/
│       │   │   ├── ApplicationUser.cs        # Extends IdentityUser
│       │   │   ├── ApplicationRole.cs         # Extends IdentityRole
│       │   │   ├── ApplicationRoleClaim.cs
│       │   │   └── ApplicationUserClaim.cs
│       │   ├── Services/
│       │   │   ├── IUserService.cs
│       │   │   ├── IRoleService.cs
│       │   │   ├── IUserClaimService.cs
│       │   │   ├── IUserManagementService.cs
│       │   │   └── UserManagerService.cs
│       │   ├── Helpers/
│       │   │   ├── UserHelper.cs
│       │   │   ├── RoleHelper.cs
│       │   │   └── ClaimHelper.cs
│       │   ├── Extensions/
│       │   │   ├── UserExtensions.cs
│       │   │   ├── RoleExtensions.cs
│       │   │   ├── ClaimsPrincipalExtensions.cs
│       │   │   └── IdentityResultExtensions.cs
│       │   └── Framework.Identity.csproj
│
├── src/
│   ├── IIROSA.Domain/                      # Application Domain Layer
│   │   ├── Entities/                        # Domain Entities (inherit from Framework.Core)
│   │   │   ├── Charity.cs                   # : FullAuditedEntityBase<Guid>
│   │   │   ├── Family.cs                    # : FullAuditedEntityBase<Guid>
│   │   │   ├── Orphan.cs                    # : FullAuditedEntityBase<Guid>
│   │   │   ├── Provider.cs                  # : FullAuditedEntityBase<Guid>
│   │   │   ├── Father.cs                     # : FullAuditedEntityBase<Guid>
│   │   │   ├── Mother.cs                     # : FullAuditedEntityBase<Guid>
│   │   │   ├── PeriodicReport.cs             # : FullAuditedEntityBase<Guid>
│   │   │   ├── OfficeProject.cs               # : FullAuditedEntityBase<Guid>
│   │   │   ├── Mission.cs                    # : FullAuditedEntityBase<Guid>
│   │   │   ├── SeasonalAidCampaign.cs         # : FullAuditedEntityBase<Guid>
│   │   │   ├── HousingProject.cs              # : FullAuditedEntityBase<Guid>
│   │   │   ├── Check.cs                      # : FullAuditedEntityBase<Guid>
│   │   │   ├── Employee.cs                   # : FullAuditedEntityBase<Guid>
│   │   │   ├── ImportExportLog.cs            # : FullAuditedEntityBase<Guid>
│   │   │   └── Lookups/                       # Lookup Entities
│   │   │       ├── Country.cs                 # : FullAuditedEntityBase<int>
│   │   │       ├── Region.cs                  # : FullAuditedEntityBase<int>
│   │   │       ├── Center.cs                  # : FullAuditedEntityBase<int>
│   │   │       ├── OfficeProjectType.cs       # : FullAuditedEntityBase<int>
│   │   │       ├── MissionType.cs             # : FullAuditedEntityBase<int>
│   │   │       ├── MissionTimeType.cs         # : FullAuditedEntityBase<int>
│   │   │       ├── EducationalStage.cs        # : FullAuditedEntityBase<int>
│   │   │       ├── EducationalLevel.cs        # : FullAuditedEntityBase<int>
│   │   │       ├── Bank.cs                    # : FullAuditedEntityBase<int>
│   │   │       ├── ChequeBeneficiary.cs       # : FullAuditedEntityBase<int>
│   │   │       ├── DocumentType.cs            # : FullAuditedEntityBase<int>
│   │   │       └── ... (more lookups)
│   │   ├── Interfaces/                      # Domain Interfaces
│   │   │   ├── ICharityRepository.cs
│   │   │   ├── IFamilyRepository.cs
│   │   │   ├── IOrphanRepository.cs
│   │   │   └── ... (one per entity)
│   │   └── IIROSA.Domain.csproj
│   │
│   ├── IIROSA.Application/                  # Application Layer
│   │   ├── Interfaces/                      # Service Interfaces
│   │   │   ├── ICharityService.cs
│   │   │   ├── IFamilyService.cs
│   │   │   ├── IOrphanService.cs
│   │   │   ├── IPeriodicReportService.cs
│   │   │   ├── IOfficeProjectService.cs
│   │   │   ├── IMissionService.cs
│   │   │   ├── ISeasonalAidService.cs
│   │   │   ├── IHousingProjectService.cs
│   │   │   ├── ICheckService.cs
│   │   │   ├── IEmployeeService.cs
│   │   │   ├── IImportExportService.cs
│   │   │   └── ... (services from Framework.Core reused)
│   │   ├── DTOs/                            # Data Transfer Objects
│   │   │   ├── Charity/
│   │   │   ├── Family/
│   │   │   ├── Orphan/
│   │   │   └── ... (one per entity)
│   │   ├── Mappers/                         # AutoMapper Profiles
│   │   │   └── MappingProfile.cs
│   │   ├── Validators/                      # FluentValidation Validators
│   │   │   └── ... (one per entity)
│   │   ├── Services/                        # Service Implementations
│   │   │   ├── CharityService.cs
│   │   │   ├── FamilyService.cs
│   │   │   ├── OrphanService.cs
│   │   │   └── ... (one per entity)
│   │   ├── Exceptions/                      # Custom Exceptions
│   │   │   └── ...
│   │   └── IIROSA.Application.csproj
│   │
│   ├── IIROSA.Infrastructure/               # Infrastructure Layer
│   │   ├── Data/                            # Data Access
│   │   │   ├── ApplicationDbContext.cs      # Auto-discovers entities
│   │   │   ├── Repository/                    # Repository Implementations
│   │   │   │   ├── CharityRepository.cs
│   │   │   │   ├── FamilyRepository.cs
│   │   │   │   ├── OrphanRepository.cs
│   │   │   │   └── ... (one per entity)
│   │   │   ├── UnitOfWork/
│   │   │   │   └── UnitOfWork.cs
│   │   │   └── Conventions/
│   │   │       ├── EntityConfigurationConvention.cs
│   │   │       └── ForeignKeyConvention.cs
│   │   ├── Hubs/                            # SignalR Hubs
│   │   │   ├── NotificationHub.cs
│   │   │   └── DashboardHub.cs
│   │   ├── BackgroundServices/
│   │   │   └── ...
│   │   └── IIROSA.Infrastructure.csproj
│   │
│   └── IIROSA.Web/                         # Presentation Layer
│       ├── Controllers/
│       │   └── Api/
│       │       └── V1/
│       │           ├── CharitiesController.cs
│       │           ├── FamiliesController.cs
│       │           ├── OrphansController.cs
│       │           ├── PeriodicReportsController.cs
│       │           ├── OfficeProjectsController.cs
│       │           ├── MissionsController.cs
│       │           ├── SeasonalAidController.cs
│       │           ├── HousingProjectsController.cs
│       │           ├── ChecksController.cs
│       │           ├── EmployeesController.cs
│       │           ├── ImportsExportsController.cs
│       │           ├── NotificationsController.cs   # From Framework.Core
│       │           ├── DashboardController.cs
│       │           └── AuditLogsController.cs            # From Framework.Core
│       ├── Filters/
│       ├── Middleware/
│       ├── Models/
│       ├── Extensions/
│       │   └── ServiceCollectionExtensions.cs  # DI Configuration
│       ├── wwwroot/
│       ├── appsettings.json
│       └── IIROSA.Web.csproj
│
├── tests/
│   └── IIROSA.Tests/
│       ├── Unit/
│       ├── Integration/
│       └── IIROSA.Tests.csproj
│
├── docs/
│   └── ...
│
└── README.md
```

---

## Framework.Core Base Classes

### **FullAuditedEntityBase<Guid> (Main Entities)**

```csharp
// Framework.Core/BaseEntities/FullAuditedEntityBase.cs
using System;

namespace Framework.Core.BaseEntities;

public abstract class FullAuditedEntityBase<TKey> : EntityBase
{
    // Audit Fields (automatically populated by Framework)
    public DateTime CreatedDate { get; set; }
    public Guid? CreatedBy { get; set; }
    public DateTime ModifiedDate { get; set; }
    public Guid? ModifiedBy { get; set; }
    public DateTime? DeletedDate { get; set; }
    public Guid? DeletedBy { get; set; }
    public bool IsDeleted { get; set; }
}

public abstract class FullAuditedEntityBase : FullAuditedEntityBase<Guid>
{
    // Convenience class for Guid entities
}

public abstract class FullAuditedEntityBaseInt : FullAuditedEntityBase<int>
{
    // Convenience class for int entities (lookups)
}
```

### **EntityBase (Base for All Entities)**

```csharp
// Framework.Core/BaseEntities/EntityBase.cs
namespace Framework.Core.BaseEntities;

public abstract class EntityBase
{
    // Common properties for all entities
    public virtual TKey Id { get; set; }

    protected EntityBase()
    {
        Id = default!;
    }
}

public abstract class EntityBase : EntityBase<Guid>
{
    // Convenience class
}
```

### **LookupEntityBase (For Lookup Tables)**

```csharp
// Framework.Core/BaseEntities/LookupEntityBase.cs
namespace Framework.Core.BaseEntities;

public abstract class LookupEntityBase : FullAuditedEntityBaseInt
{
    // Additional properties for lookup entities
    public string Name { get; set; } = string.Empty;
    public string? NameAr { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
    public int SortOrder { get; set; }

    // Override equality for lookups
    public override bool Equals(object? obj)
    {
        if (obj is LookupEntityBase other)
        {
            return Id == other.Id;
        }
        return false;
    }

    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }
}
```

---

## Database Schema Conventions

### **Schema Organization**
All database tables are organized into two main schemas:

| Schema | Purpose | Examples |
|--------|---------|----------|
| **`Lookup`** | Lookup/reference tables (all entities inheriting from `LookupEntity`) | Country, Region, Center, Bank, Department, etc. |
| **`IIROSA`** | Main business entities (all entities inheriting from `FullAuditedEntity`) | Charity, Family, Orphan, Sponsor, Mission, etc. |

### **Configuration Pattern**
All entity configurations use the `MappingDefaults` constants:

```csharp
// IIROSA.Domain/Configurations/MappingDefaults.cs
public static class MappingDefaults
{
    public const string LOOKUP_SCHEMA = "Lookup";   // For lookup entities
    public const string IIROSA_SCHEMA = "IIROSA";    // For main business entities
}
```

### **Configuration File Examples**

**Lookup Entity Configuration:**
```csharp
// IIROSA.Domain/Configurations/CountryConfiguration.cs
public class CountryConfiguration : IEntityTypeConfiguration<Country>
{
    public void Configure(EntityTypeBuilder<Country> builder)
    {
        // Uses nameof() for table name + Lookup schema
        builder.ToTable(nameof(Country), MappingDefaults.LOOKUP_SCHEMA);

        builder.HasKey(x => x.Id);
        // ... rest of configuration
    }
}
```

**Main Entity Configuration:**
```csharp
// IIROSA.Domain/Configurations/CharityConfiguration.cs
public class CharityConfiguration : IEntityTypeConfiguration<Charity>
{
    public void Configure(EntityTypeBuilder<Charity> builder)
    {
        // Uses nameof() for table name + IIROSA schema
        builder.ToTable(nameof(Charity), MappingDefaults.IIROSA_SCHEMA);

        builder.HasKey(x => x.Id);
        // ... rest of configuration
    }
}
```

---

## Domain Entities (Updated)

### **Main Entity Example**

```csharp
// IIROSA.Domain/Entities/Charity.cs
using Framework.Core.BaseEntities;

namespace IIROSA.Domain.Entities;

public class Charity : FullAuditedEntityBase  // Inherits Framework base class
{
    // No need to declare audit fields - they're inherited!
    // CreatedDate, CreatedBy, ModifiedDate, ModifiedBy, DeletedDate, DeletedBy, IsDeleted
    // are all available from FullAuditedEntityBase<Guid>

    // Entity Properties
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string NGOType { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Fax { get; set; }
    public string MapLocation { get; set; }

    // Foreign Keys to Lookup Entities
    public Guid? FK_CountryId { get; set; }
    public Guid? FK_RegionId { get; set; }
    public Guid? FK_CenterId { get; set; }
    public Guid? FK_BankId { get; set; }

    // Navigation Properties
    public virtual Country? Country { get; set; }
    public virtual Region? Region { get; set; }
    public virtual Center? Center { get; set; }
    public virtual Bank? Bank { get; set; }

    // Rights Management
    public bool IsAddEnabled { get; set; } = true;
    public bool IsUpdateEnabled { get; set; } = true;
    public bool IsLocked { get; set; } = false;

    public string ReceivingDonations { get; set; } = "Yes";
    public string OfficeIcon { get; set; }

    // Navigation Collections
    public virtual ICollection<Family> Families { get; set; } = new List<Family>();
    public virtual ICollection<Orphan> Orphans { get; set; } = new List<Orphan>();
    public virtual ICollection<OfficeProject> OfficeProjects { get; set; } = new List<OfficeProject>();
    public virtual ICollection<HousingProject> HousingProjects { get; set; } = new List<HousingProject>();
}
```

### **Lookup Entity Example**

```csharp
// IIROSA.Domain/Entities/Lookups/Country.cs
using Framework.Core.BaseEntities;

namespace IIROSA.Domain.Entities.Lookups;

public class Country : LookupEntityBase  // Inherits int-based audit base
{
    // Id: int (inherited)
    // Name, IsActive, etc. (inherited)

    // Additional properties
    public string? IsoCode { get; set; }
    public string? DialingCode { get; set; }
    public string? FlagIcon { get; set; }

    // Navigation
    public virtual ICollection<Charity> Charities { get; set; } = new List<Charity>();
    public virtual ICollection<Family> Families { get; set; } = new List<Family>();
}

// IIROSA.Domain/Entities/Lookups/Region.cs
public class Region : LookupEntityBase
{
    public Guid? FK_CountryId { get; set; }

    public virtual Country? Country { get; set; }
    public virtual ICollection<Center> Centers { get; set; } = new List<Center>();
}
```

### **Entity-to-Schema Mapping Reference**

| Entity | Base Class | Schema | Table Name |
|--------|-----------|--------|------------|
| **Lookup Entities** | | | |
| Country | `LookupEntity` | `Lookup` | `Lookup.Country` |
| Region | `LookupEntity` | `Lookup` | `Lookup.Region` |
| Center | `LookupEntity` | `Lookup` | `Lookup.Center` |
| City | `LookupEntity` | `Lookup` | `Lookup.City` |
| Bank | `LookupEntity` | `Lookup` | `Lookup.Bank` |
| Department | `LookupEntity` | `Lookup` | `Lookup.Department` |
| OfficeProjectType | `LookupEntity` | `Lookup` | `Lookup.OfficeProjectType` |
| MissionType | `LookupEntity` | `Lookup` | `Lookup.MissionType` |
| MissionTimeType | `LookupEntity` | `Lookup` | `Lookup.MissionTimeType` |
| NGOType | `LookupEntity` | `Lookup` | `Lookup.NGOType` |
| ChequeBeneficiary | `LookupEntity` | `Lookup` | `Lookup.ChequeBeneficiary` |
| OutgoingCategory | `LookupEntity` | `Lookup` | `Lookup.OutgoingCategory` |
| LivingCondition | `LookupEntity` | `Lookup` | `Lookup.LivingCondition` |
| HousingType | `LookupEntity` | `Lookup` | `Lookup.HousingType` |
| EducationLevel | `LookupEntity` | `Lookup` | `Lookup.EducationLevel` |
| HealthStatus | `LookupEntity` | `Lookup` | `Lookup.HealthStatus` |
| SupportTicketCategory | `LookupEntity` | `Lookup` | `Lookup.SupportTicketCategory` |
| SupportTicketPriority | `LookupEntity` | `Lookup` | `Lookup.SupportTicketPriority` |
| SupportTicketStatus | `LookupEntity` | `Lookup` | `Lookup.SupportTicketStatus` |
| **Main Entities** | | | |
| Charity | `FullAuditedEntity` | `IIROSA` | `IIROSA.Charity` |
| Family | `FullAuditedEntity` | `IIROSA` | `IIROSA.Family` |
| Orphan | `FullAuditedEntity` | `IIROSA` | `IIROSA.Orphan` |
| Sponsor | `FullAuditedEntity` | `IIROSA` | `IIROSA.Sponsor` |
| Father | `FullAuditedEntity` | `IIROSA` | `IIROSA.Father` |
| Mother | `FullAuditedEntity` | `IIROSA` | `IIROSA.Mother` |
| PeriodicReport | `FullAuditedEntity` | `IIROSA` | `IIROSA.PeriodicReport` |
| OfficeProject | `FullAuditedEntity` | `IIROSA` | `IIROSA.OfficeProject` |
| Mission | `FullAuditedEntity` | `IIROSA` | `IIROSA.Mission` |
| SeasonalAidCampaign | `FullAuditedEntity` | `IIROSA` | `IIROSA.SeasonalAidCampaign` |
| HousingProject | `FullAuditedEntity` | `IIROSA` | `IIROSA.HousingProject` |
| Check | `FullAuditedEntity` | `IIROSA` | `IIROSA.Check` |
| Employee | `FullAuditedEntity` | `IIROSA` | `IIROSA.Employee` |
| OrphanPayment | `FullAuditedEntity` | `IIROSA` | `IIROSA.OrphanPayment` |
| OrphanPaymentItem | `FullAuditedEntity` | `IIROSA` | `IIROSA.OrphanPaymentItem` |
| SupportTicket | `FullAuditedEntity` | `IIROSA` | `IIROSA.SupportTicket` |
| TicketResponse | `FullAuditedEntity` | `IIROSA` | `IIROSA.TicketResponse` |
| Incoming | `FullAuditedEntity` | `IIROSA` | `IIROSA.Incoming` |
| Outgoing | `FullAuditedEntity` | `IIROSA` | `IIROSA.Outgoing` |

---

## Repository Pattern (Updated for Framework)

### **Generic Repository Interface**

```csharp
// Framework.Core/Interfaces/IRepository.cs
using System.Linq.Expressions;

namespace Framework.Core.Interfaces;

public interface IRepository<TEntity, TKey> where TEntity : class
{
    // Query Operations
    Task<TEntity?> GetByIdAsync(TKey id);
    Task<IEnumerable<TEntity>> GetAllAsync();
    Task<IEnumerable<TEntity>> FindAsync(Expression<Func<TEntity, bool>> predicate);
    Task<(IEnumerable<TEntity> Items, int TotalCount)> GetPagedAsync(
        Expression<Func<TEntity, bool>>? filter = null,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
        int pageNumber = 1,
        int pageSize = 10);

    // Include Operations
    IQueryable<TEntity> Include<TProperty>(Expression<Func<TEntity, TProperty>> navigationPropertyPath);

    // CRUD Operations
    Task AddAsync(TEntity entity);
    Task AddRangeAsync(IEnumerable<TEntity> entities);
    void Update(TEntity entity);
    void UpdateRange(IEnumerable<TEntity> entities);
    void Delete(TEntity entity);
    void DeleteRange(IEnumerable<TEntity> entities);

    // Raw SQL
    Task<IEnumerable<TEntity>> ExecuteSqlRawAsync(string sql, params object[] parameters);

    // Count Operations
    Task<int> CountAsync(Expression<Func<TEntity, bool>>? filter = null);
    Task<bool> ExistsAsync(Expression<Func<TEntity, bool>> predicate);
}

// Convenience wrapper for Guid entities
public interface IRepository<TEntity> : IRepository<TEntity, Guid>, IDisposable
    where TEntity : FullAuditedEntityBase
{
}

// Convenience wrapper for int entities
public interface ILookupRepository<TEntity> : IRepository<TEntity, int>, IDisposable
    where TEntity : LookupEntityBase
{
}
```

### **Generic Repository Implementation**

```csharp
// IIROSA.Infrastructure/Data/Repository/Repository.cs
using Framework.Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;

namespace IIROSA.Infrastructure.Data.Repository;

public class Repository<TEntity, TKey> : IRepository<TEntity, TKey>
    where TEntity : class
{
    protected readonly ApplicationDbContext _context;
    protected readonly DbSet<TEntity> _dbSet;

    public Repository(ApplicationDbContext context)
    {
        _context = context;
        _dbSet = context.Set<TEntity>();
    }

    public virtual async Task<TEntity?> GetByIdAsync(TKey id)
    {
        return await _dbSet.FindAsync(id);
    }

    public virtual async Task<IEnumerable<TEntity>> GetAllAsync()
    {
        return await _dbSet.ToListAsync();
    }

    public virtual async Task<IEnumerable<TEntity>> FindAsync(Expression<Func<TEntity, bool>> predicate)
    {
        return await _dbSet.Where(predicate).ToListAsync();
    }

    public virtual async Task<(IEnumerable<TEntity> Items, int TotalCount)> GetPagedAsync(
        Expression<Func<TEntity, bool>>? filter = null,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
        int pageNumber = 1,
        int pageSize = 10)
    {
        IQueryable<TEntity> query = _dbSet;

        // Apply filter (include soft delete check if entity has IsDeleted)
        if (filter != null)
        {
            query = query.Where(filter);
        }

        // Auto-filter soft deletes if entity implements it
        if (typeof(ISoftDeletable).IsAssignableFrom(typeof(TEntity)))
        {
            var param = Expression.Parameter(typeof(TEntity), "e");
            var isDeletedProperty = Expression.Property(param, "IsDeleted");
            var notDeleted = Expression.Lambda(
                Expression.Equal(isDeletedProperty, Expression.Constant(false)),
                param
            );
            query = query.Where((Expression<Func<TEntity, bool>>)notDeleted);
        }

        // Get total count before pagination
        var totalCount = await query.CountAsync();

        // Apply ordering
        if (orderBy != null)
        {
            query = orderBy(query);
        }

        // Apply pagination
        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    public virtual IQueryable<TEntity> Include<TProperty>(
        Expression<Func<TEntity, TProperty>> navigationPropertyPath)
    {
        return _dbSet.Include(navigationPropertyPath);
    }

    public virtual async Task AddAsync(TEntity entity)
    {
        // Set audit fields if entity implements audit interface
        if (entity is IAuditable auditable)
        {
            auditable.CreatedDate = DateTime.UtcNow;
            auditable.CreatedBy = GetCurrentUserId();
            auditable.ModifiedDate = DateTime.UtcNow;
            auditable.ModifiedBy = GetCurrentUserId();
        }

        await _dbSet.AddAsync(entity);
    }

    public virtual async Task AddRangeAsync(IEnumerable<TEntity> entities)
    {
        foreach (var entity in entities)
        {
            if (entity is IAuditable auditable)
            {
                auditable.CreatedDate = DateTime.UtcNow;
                auditable.CreatedBy = GetCurrentUserId();
                auditable.ModifiedDate = DateTime.UtcNow;
                auditable.ModifiedBy = GetCurrentUserId();
            }
        }

        await _dbSet.AddRangeAsync(entities);
    }

    public virtual void Update(TEntity entity)
    {
        // Update audit fields
        if (entity is IAuditable auditable)
        {
            auditable.ModifiedDate = DateTime.UtcNow;
            auditable.ModifiedBy = GetCurrentUserId();
        }

        _dbSet.Update(entity);
    }

    public virtual void UpdateRange(IEnumerable<TEntity> entities)
    {
        foreach (var entity in entities)
        {
            if (entity is IAuditable auditable)
            {
                auditable.ModifiedDate = DateTime.UtcNow;
                auditable.ModifiableBy = GetCurrentUserId();
            }
        }

        _dbSet.UpdateRange(entities);
    }

    public virtual void Delete(TEntity entity)
    {
        // Soft delete if entity supports it
        if (entity is ISoftDeletable softDeletable)
        {
            softDeletable.IsDeleted = true;
            softDeletable.DeletedDate = DateTime.UtcNow;
            softDeletable.DeletedBy = GetCurrentUserId();
            _dbSet.Update(entity);
        }
        else
        {
            // Hard delete
            _dbSet.Remove(entity);
        }
    }

    public virtual void DeleteRange(IEnumerable<TEntity> entities)
    {
        foreach (var entity in entities)
        {
            if (entity is ISoftDeletable softDeletable)
            {
                softDeletable.IsDeleted = true;
                softDeletable.DeletedDate = DateTime.UtcNow;
                softDeletable.DeletedBy = GetCurrentUserId();
                _dbSet.Update(entity);
            }
            else
            {
                _dbSet.Remove(entity);
            }
        }
    }

    public virtual async Task<IEnumerable<TEntity>> ExecuteSqlRawAsync(string sql, params object[] parameters)
    {
        return await _dbSet.FromSqlRaw(sql, parameters).ToListAsync();
    }

    public virtual async Task<int> CountAsync(Expression<Func<TEntity, bool>>? filter = null)
    {
        return filter == null
            ? await _dbSet.CountAsync()
            : await _dbSet.CountAsync(filter);
    }

    public virtual async Task<bool> ExistsAsync(Expression<Func<TEntity, bool>> predicate)
    {
        return await _dbSet.AnyAsync(predicate);
    }

    protected Guid GetCurrentUserId()
    {
        // Get from HttpContext via ICurrentUserService
        return Guid.Empty; // Placeholder
    }

    public virtual void Dispose()
    {
        _context.Dispose();
    }
}

// Convenience implementations
public class Repository<TEntity> : Repository<TEntity, Guid>, IRepository<TEntity>
    where TEntity : FullAuditedEntityBase
{
    public Repository(ApplicationDbContext context) : base(context)
    {
    }
}

public class LookupRepository<TEntity> : Repository<TEntity, int>, ILookupRepository<TEntity>
    where TEntity : LookupEntityBase
{
    public LookupRepository(ApplicationDbContext context) : base(context)
    {
    }
}
```

---

## Application DbContext (Auto-Discovery)

```csharp
// IIROSA.Infrastructure/Data/ApplicationDbContext.cs
using Framework.Core.BaseEntities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Framework.Identity.Models;
using Microsoft.EntityFrameworkCore;

namespace IIROSA.Infrastructure.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, string>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
        // Enable auto-discovery of entities from assemblies
    }

    // NO NEED to manually add DbSets!
    // Entities are auto-discovered from:
    // 1. Framework.Core (Notification, Setting, Attachment, AuditLog)
    // 2. Framework.Identity (ApplicationUser, ApplicationRole, etc.)
    // 3. IIROSA.Domain (all business entities)

    // Database schemas are configured in individual entity configuration files:
    // - Lookup schema: for all LookupEntity-derived entities
    // - IIROSA schema: for all FullAuditedEntity-derived entities

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply all configurations from assemblies
        modelBuilder.ApplyConfigurationsFromAssemblies(
            typeof(FullAuditedEntityBase).Assembly,
            typeof(ApplicationUser).Assembly,
            typeof(Charity).Assembly
        );

        // Configure soft delete global query filters
        ConfigureSoftDeleteGlobalFilters(modelBuilder);

        // Configure entity relationships
        ConfigureRelationships(modelBuilder);
    }

    private void ConfigureSoftDeleteGlobalFilters(ModelBuilder modelBuilder)
    {
        // Automatically filter soft-deleted entities for all entities implementing ISoftDeletable
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(ISoftDeletable).IsAssignableFrom(entityType.ClrType))
            {
                var parameter = Expression.Parameter(entityType.ClrType, "e");
                var property = Expression.Property(parameter, "IsDeleted");
                var filter = Expression.Lambda(
                    Expression.Equal(property, Expression.Constant(false)),
                    parameter
                );

                modelBuilder.Entity(entityType.ClrType)
                    .HasQueryFilter(filter);
            }
        }
    }

    private void ConfigureRelationships(ModelBuilder modelBuilder)
    {
        // Charity Relationships
        modelBuilder.Entity<Charity>(entity =>
        {
            entity.ToTable("Charities");

            entity.HasIndex(e => e.Code).IsUnique();
            entity.HasIndex(e => e.Name);
            entity.HasIndex(e => e.Email);
            entity.HasIndex(e => e.IsActive);
            entity.HasIndex(e => e.FK_CountryId);
            entity.HasIndex(e => e.FK_RegionId);
            entity.HasIndex(e => e.FK_CenterId);

            // Foreign Keys
            entity.HasOne(d => d.Country)
                .WithMany()
                .HasForeignKey(d => d.FK_CountryId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(d => d.Region)
                .WithMany(p => p.Charities)
                .HasForeignKey(d => d.FK_RegionId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(d => d.Center)
                .WithMany(p => p.Charities)
                .HasForeignKey(d => d.FK_CenterId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(d => d.Bank)
                .WithMany()
                .HasForeignKey(d => d.FK_BankId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Family Relationships
        modelBuilder.Entity<Family>(entity =>
        {
            entity.ToTable("Families");

            entity.HasIndex(e => e.FK_CharityId);
            entity.HasIndex(e => e.IsActive);
            entity.HasIndex(e => e.IsDeleted);

            entity.HasOne(d => d.Charity)
                .WithMany(c => c.Families)
                .HasForeignKey(d => d.FK_ChartyId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Orphan Relationships
        modelBuilder.Entity<Orphan>(entity =>
        {
            entity.ToTable("Orphans");

            entity.HasIndex(e => e.FK_CharityId);
            entity.HasIndex(e => e.FK_FamilyId);
            entity.HasIndex(e => e.IsActive);

            entity.HasOne(d => d.Charity)
                .WithMany(c => c.Orphans)
                .HasForeignKey(d => d.FK_CharityId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(d => d.Family)
                .WithMany(f => f.Orphans)
                .HasForeignKey(d => d.FK_FamilyId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Periodic Report Relationships
        modelBuilder.Entity<PeriodicReport>(entity =>
        {
            entity.ToTable("PeriodicReports");

            entity.HasIndex(e => e.FK_ChildId);
            entity.HasIndex(e => e.Reviewed);
            entity.HasIndex(e => e.IsAccepted);
            entity.HasIndex(e => e.IsRefused);
            entity.HasIndex(e => e.ReportDate);

            entity.HasOne(d => d.Child)
                .WithMany(c => c.PeriodicReports)
                .HasForeignKey(d => d.FK_ChildId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ... Configure all other entities ...
    }
}
```

---

## Unit of Work (Updated)

```csharp
// IIROSA.Infrastructure/Data/UnitOfWork/UnitOfWork.cs
using Framework.Core.Interfaces;

namespace IIROSA.Infrastructure.Data.UnitOfWork;

public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;
    private readonly Dictionary<Type, object> _repositories = new();
    private IDbContextTransaction? _transaction;

    public UnitOfWork(ApplicationDbContext context)
    {
        _context = context;
    }

    // Main Entities (Guid-based)
    public ICharityRepository Charities =>
        GetRepository<ICharityRepository, CharityRepository>();

    public IFamilyRepository Families =>
        GetRepository<IFamilyRepository, FamilyRepository>();

    public IOrphanRepository Orphans =>
        GetRepository<IOrphanRepository, OrphanRepository>();

    public IPeriodicReportRepository PeriodicReports =>
        GetRepository<IPeriodicReportRepository, PeriodicReportRepository>();

    public IOfficeProjectRepository OfficeProjects =>
        GetRepository<IOfficeProjectRepository, OfficeProjectRepository>();

    public IMissionRepository Missions =>
        GetRepository<IMissionRepository, MissionRepository>();

    public ISeasonalAidRepository SeasonalAidCampaigns =>
        GetRepository<ISeasonalAidRepository, SeasonalAidRepository>();

    public IHousingProjectRepository HousingProjects =>
        GetRepository<IHousingProjectRepository, HousingProjectRepository>();

    public ICheckRepository Checks =>
        GetRepository<ICheckRepository, CheckRepository>();

    public IEmployeeRepository Employees =>
        GetRepository<IEmployeeRepository, EmployeeRepository>();

    public IImportExportRepository ImportExports =>
        GetRepository<IImportExportRepository, ImportExportRepository>();

    // Framework.Core Entities
    public INotificationRepository Notifications =>
        GetRepository<INotificationRepository, NotificationRepository>();

    public ISettingRepository Settings =>
        GetRepository<ISettingRepository, SettingRepository>();

    public IAttachmentRepository Attachments =>
        GetRepository<IAttachmentRepository, AttachmentRepository>();

    public IAuditLogRepository AuditLogs =>
        GetRepository<IAuditLogRepository, AuditLogRepository>();

    // Lookup Entities (int-based)
    public ICountryRepository Countries =>
        GetRepository<ICountryRepository, CountryRepository>();

    public IRegionRepository Regions =>
        GetRepository<IRegionRepository, RegionRepository>();

    public ICenterRepository Centers =>
        GetRepository<ICenterRepository, CenterRepository>();

    public IBankRepository Banks =>
        GetRepository<IBankRepository, BankRepository>();

    // ... more lookup repositories ...

    private TInterface GetRepository<TInterface, TImplementation>()
        where TInterface : class
        where TImplementation : class, TInterface
    {
        var type = typeof(TImplementation);

        if (!_repositories.ContainsKey(type))
        {
            var repository = Activator.CreateInstance(type, _context);
            _repositories[type] = repository!;
        }

        return _repositories[type] as TInterface;
    }

    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }

    public async Task BeginTransactionAsync()
    {
        _transaction = await _context.Database.BeginTransactionAsync();
    }

    public async Task CommitTransactionAsync()
    {
        try
        {
            await _context.SaveChangesAsync();
            await _transaction!.CommitAsync();
        }
        catch
        {
            await RollbackTransactionAsync();
            throw;
        }
        finally
        {
            if (_transaction != null)
            {
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }
    }

    public async Task RollbackTransactionAsync()
    {
        try
        {
            await _transaction!.RollbackAsync();
        }
        finally
        {
            if (_transaction != null)
            {
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }
    }

    public void Dispose()
    {
        _transaction?.Dispose();
        _context.Dispose();
    }
}
```

---

## Service Layer (Using Framework Services)

```csharp
// IIROSA.Application/Services/CharityService.cs
using Framework.Core.Interfaces;
using Framework.Identity.Services;
using Framework.Core.Services;
using IIROSA.Domain.Entities;

namespace IIROSA.Application.Services;

public class CharityService : Service<CharityDto, Charity, ICharityRepository>, ICharityService
{
    private readonly ICharityRepository _charityRepository;
    private readonly IUserManagementService _userService;  // From Framework.Identity
    private readonly INotificationService _notificationService;  // From Framework.Core

    public CharityService(
        ICharityRepository repository,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<CharityService> logger,
        IUserManagementService userService,
        INotificationService notificationService)
        : base(repository, unitOfWork, mapper, logger)
    {
        _charityRepository = repository;
        _userService = userService;
        _notificationService = notificationService;
    }

    public async Task<CharityDto> CreateCharityAsync(CreateCharityDto dto)
    {
        // Check name uniqueness
        if (!await IsNameUniqueAsync(dto.Name))
        {
            throw new BusinessException("Charity name already exists");
        }

        // Create charity
        var charity = new Charity
        {
            Code = await GenerateCharityCodeAsync(),
            Name = dto.Name,
            Address = dto.Address,
            Email = dto.Email,
            Phone = dto.Phone,
            IsActive = true,
            IsAddEnabled = true,
            IsUpdateEnabled = true
        };

        // Create user account via Framework.Identity service
        if (dto.CreateUserAccount)
        {
            var user = await _userService.CreateUserAsync(
                email: dto.Email,
                password: GenerateRandomPassword(),
                firstName: dto.ContactPersonName,
                lastName: "Charity User",
                role: "Charity"
            );

            charity.FK_UserId = user.Id;
        }

        await _charityRepository.AddAsync(charity);
        await _unitOfWork.SaveChangesAsync();

        // Send notification via Framework.Core service
        await _notificationService.SendNotification(
            userId: charity.FK_UserId,
            type: NotificationType.Info,
            title: "Welcome to IIROSA System",
            message: $"Your charity account has been created successfully",
            category: "System"
        );

        return _mapper.Map<CharityDto>(charity);
    }

    public async Task LockCharityAsync(Guid id)
    {
        var charity = await _charityRepository.GetByIdAsync(id);
        if (charity == null)
            throw new NotFoundException(nameof(Charity), id);

        charity.IsLocked = true;
        _charityRepository.Update(charity);
        await _unitOfWork.SaveChangesAsync();

        // Framework.Core notification
        await _notificationService.SendNotification(
            userId: charity.FK_UserId,
            type: NotificationType.Error,
            title: "Account Locked",
            message: "Your charity account has been locked",
            category: "Alert"
        );
    }

    // ... other methods
}
```

---

## Controllers (Updated)

```csharp
// IIROSA.Web/Controllers/Api/V1/CharitiesController.cs
using Framework.Identity.Services;
using Framework.Core.Services;

public class CharitiesController : BaseController<CharityDto, Charity>
{
    private readonly ICharityService _charityService;
    private readonly IUserManagementService _userService;

    public CharitiesController(
        ICharityService charityService,
        IUserManagementService userService,
        ILogger<CharitiesController> logger)
        : base(charityService, logger)
    {
        _charityService = charityService;
        _userService = userService;
    }

    // Standard CRUD endpoints inherited from BaseController

    [HttpPost("{id}/activate")]
    public async Task<ActionResult<ApiResponse>> Activate(Guid id)
    {
        await _charityService.ActivateCharityAsync(id);
        return Ok(ApiResponse.Success("Charity activated"));
    }

    [HttpPost("{id}/lock")]
    public async Task<ActionResult<ApiResponse>> Lock(Guid id)
    {
        await _charityService.LockCharityAsync(id);
        return Ok(ApiResponse.Success("Charity locked"));
    }

    // ... other endpoints
}
```

---

## Dependency Injection (Updated)

```csharp
// IIROSA.Web/Extensions/ServiceCollectionExtensions.cs
using Microsoft.Extensions.DependencyInjection;
using Framework.Core.Interfaces;
using Framework.Identity.Services;
using Framework.Core.Services;
using IIROSA.Application.Interfaces;
using IIROSA.Application.Services;
using IIROSA.Infrastructure.Data;
using IIROSA.Infrastructure.Data.Repository;
using IIROSA.Infrastructure.Data.UnitOfWork;

namespace IIROSA.Web.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddIIROSAServices(this IServiceCollection services, IConfiguration configuration)
    {
        // === Framework.Identity Services ===
        services.AddIdentityServices(configuration);

        // === Framework.Core Services ===
        services.AddFrameworkCoreServices(configuration);

        // === Application Services ===
        services.AddApplicationServices();

        // === Infrastructure Services ===
        services.AddInfrastructureServices(configuration);

        return services;
    }

    public static IServiceCollection AddIdentityServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Register Framework.Identity services
        services.AddScoped<IUserManagementService, UserManagementService>();
        services.AddScoped<IRoleService, RoleService>();
        services.AddScoped<IUserClaimService, UserClaimService>();
        services.AddScoped<IUserService, UserService>();

        return services;
    }

    public static IServiceCollection AddFrameworkCoreServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Register Framework.Core services
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<ISettingService, SettingService>();
        services.AddScoped<IAttachmentService, AttachmentService>();
        services.AddScoped<IAuditLogService, AuditLogService>();
        services.AddScoped<ICacheService, CacheService>();
        services.AddScoped<IFileStorageService, FileStorageService>();
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<ISMSService, SMSService>();

        return services;
    }

    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // Auto-register all application services using reflection
        var assembly = typeof(Service<,,>).Assembly;

        var serviceInterfaces = assembly.GetTypes()
            .Where(t => t.IsInterface && t.Namespace == "IIROSA.Application.Interfaces" && t.Name.StartsWith("I") && t.Name.EndsWith("Service"))
            .ToList();

        foreach (var serviceInterface in serviceInterfaces)
        {
            var serviceImplementation = assembly.GetTypes()
                .FirstOrDefault(t => serviceInterface.IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract);

            if (serviceImplementation != null)
            {
                services.AddScoped(serviceInterface, serviceImplementation);
            }
        }

        return services;
    }

    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Database
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                sqlOptions => sqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 3,
                    maxRetryDelay: TimeSpan.FromSeconds(30)
                )
            ));

        // Unit of Work
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Auto-register all repositories using reflection
        var assembly = typeof(Repository<,>).Assembly;

        var repositoryInterfaces = assembly.GetTypes()
            .Where(t => t.IsInterface && t.Namespace.StartsWith("IIROSA.Domain.Interfaces"))
            .ToList();

        foreach (var repoInterface in repositoryInterfaces)
        {
            var repoImplementation = assembly.GetTypes()
                .FirstOrDefault(t => repoInterface.IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract);

            if (repoImplementation != null)
            {
                services.AddScoped(repoInterface, repoImplementation);
            }
        }

        // SignalR
        services.AddSignalR();

        // Background Services
        services.AddHostedService<NotificationCleanupService>();
        services.AddHostedService<ReportGenerationService>();

        return services;
    }
}
```

---

## Module List (Updated - Removed User & Role)

### **Core Modules (from Framework.Identity):**
- ✅ User Management (Framework.Identity)
- ✅ Role Management (Framework.Identity)
- ✅ Claims Management (Framework.Identity)

### **Framework.Core Modules:**
- ✅ Notifications
- ✅ Settings
- ✅ Attachments
- ✅ Audit Logs

### **Application Modules:**
1. Charities
2. Families
3. Orphans
4. Periodic Reports
5. Office Projects
6. Missions
7. Seasonal Aid
8. Housing Projects
9. General Checks
10. Employees
11. Imports & Exports
12. Dashboard
13. Analytics & Reporting

---

## Key Benefits of Framework Integration

### **1. Reusable Framework Components**
```csharp
// Don't write this - use from Framework.Identity
public class ApplicationUser : IdentityUser { }

// Don't write this - use from Framework.Core
public class Notification : FullAuditedEntityBase<Guid>
{
    public Guid UserId { get; set; }
    public string Title { get; set; }
    // Framework.Core provides all audit fields automatically
}
```

### **2. Auto-Discovery of Entities**
```csharp
// ApplicationDbContext doesn't need manual DbSets!
public class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, string>
{
    // No DbSet<Charity> needed!
    // No DbSet<Notification> needed!
    // All entities auto-discovered from assemblies
}
```

### **3. Consistent Audit Trail**
All entities inherit audit fields from `FullAuditedEntityBase`:
- CreatedDate, CreatedBy
- ModifiedDate, ModifiedBy
- DeletedDate, DeletedBy
- IsDeleted

### **4. Built-in Services**
- User Management (from Framework.Identity)
- Notifications (from Framework.Core)
- Settings Management (from Framework.Core)
- File Attachments (from Framework.Core)
- Audit Logging (from Framework.Core)

---

## Program.cs (Final Configuration)

```csharp
var builder = WebApplication.CreateBuilder(args);

// Add all services
builder.Services.AddIIROSAServices(builder.Configuration);

// Add authentication & authorization
builder.Services.AddAuthentication()
        .AddCookieAuthentication()
        .AddIdentity<ApplicationUser, ApplicationRole>(options =>
        {
            // User configuration
        })
        .AddEntityFrameworkStores<ApplicationDbContext>()
        .AddDefaultTokenProviders();

var app = builder.Build();

// Configure middleware
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

// SignalR Hubs
app.MapHub<NotificationHub>("/notificationHub");
app.MapHub<DashboardHub>("/dashboardHub");

// Controllers
app.MapControllers();

app.Run();
```

---

## Summary of Changes

### **Removed:**
- ❌ IAuditable interface (replaced by Framework.Core base class)
- ❌ Manual DbSet declarations in DbContext
- ❌ User & Role Management modules (use Framework.Identity)
- ❌ Custom audit field declarations (inherited from Framework)

### **Added:**
- ✅ Framework.Core project with base entities
- ✅ Framework.Identity project with user/role management
- ✅ All entities inherit from `FullAuditedEntityBase<Guid>` or `FullAuditedEntityBase<int>`
- ✅ Auto-discovery of entities in DbContext
- ✅ Integration with Framework.Identity services
- ✅ Integration with Framework.Core services

This corrected architecture properly leverages your existing framework projects and follows the established patterns in your organization!
