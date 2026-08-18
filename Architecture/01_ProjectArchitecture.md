# Project Architecture - Repository & Service Pattern

## Solution Structure

```
IIROSA.System.sln
│
├── src/
│   ├── IIROSA.Domain/                    # Core Domain Layer
│   │   ├── Entities/                    # Entity Models
│   │   │   ├── User.cs
│   │   │   ├── Role.cs
│   │   │   ├── Charity.cs
│   │   │   ├── Family.cs
│   │   │   ├── Orphan.cs
│   │   │   ├── PeriodicReport.cs
│   │   │   ├── OfficeProject.cs
│   │   │   ├── Mission.cs
│   │   │   ├── SeasonalAidCampaign.cs
│   │   │   ├── HousingProject.cs
│   │   │   ├── Check.cs
│   │   │   ├── Employee.cs
│   │   │   ├── ImportExportLog.cs
│   │   │   └── Notification.cs
│   │   ├── Interfaces/                  # Domain Interfaces
│   │   │   ├── IRepository.cs
│   │   │   ├── IUnitOfWork.cs
│   │   │   └── IDomainService.cs
│   │   ├── ValueObjects/                # Value Objects
│   │   ├── Specifications/              # Specifications Pattern
│   │   └── IIROSA.Domain.csproj
│   │
│   ├── IIROSA.Application/              # Application Layer
│   │   ├── Interfaces/                  # Service Interfaces
│   │   │   ├── IUserService.cs
│   │   │   ├── IRoleService.cs
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
│   │   │   ├── INotificationService.cs
│   │   │   ├── IDashboardService.cs
│   │   │   └── IAuditLogService.cs
│   │   ├── DTOs/                        # Data Transfer Objects
│   │   │   ├── User/
│   │   │   │   ├── UserDto.cs
│   │   │   │   ├── CreateUserDto.cs
│   │   │   │   �   └── UpdateUserDto.cs
│   │   │   ├── Charity/
│   │   │   ├── Family/
│   │   │   ├── Orphan/
│   │   │   ├── PeriodicReport/
│   │   │   └── [All Modules]/
│   │   ├── Mappers/                     # AutoMapper Profiles
│   │   │   └── MappingProfile.cs
│   │   ├── Validators/                  # FluentValidation Validators
│   │   │   ├── UserValidator.cs
│   │   │   ├── CharityValidator.cs
│   │   │   └── [All Modules]/
│   │   ├── Services/                    # Service Implementations
│   │   │   ├── UserService.cs
│   │   │   ├── RoleService.cs
│   │   │   ├── CharityService.cs
│   │   │   ├── FamilyService.cs
│   │   │   ├── OrphanService.cs
│   │   │   ├── PeriodicReportService.cs
│   │   │   ├── OfficeProjectService.cs
│   │   │   ├── MissionService.cs
│   │   │   ├── SeasonalAidService.cs
│   │   │   ├── HousingProjectService.cs
│   │   │   ├── CheckService.cs
│   │   │   ├── EmployeeService.cs
│   │   │   ├── ImportExportService.cs
│   │   │   ├── NotificationService.cs
│   │   │   ├── DashboardService.cs
│   │   │   └── AuditLogService.cs
│   │   ├── Exceptions/                  # Custom Exceptions
│   │   │   ├── NotFoundException.cs
│   │   │   ├── ValidationException.cs
│   │   │   └── BusinessException.cs
│   │   └── IIROSA.Application.csproj
│   │
│   ├── IIROSA.Infrastructure/           # Infrastructure Layer
│   │   ├── Data/                        # Data Access
│   │   │   ├── ApplicationDbContext.cs
│   │   │   ├── Repository/              # Repository Implementations
│   │   │   │   ├── Repository.cs
│   │   │   │   ├── UserRepository.cs
│   │   │   │   ├── RoleRepository.cs
│   │   │   │   ├── CharityRepository.cs
│   │   │   │   ├── FamilyRepository.cs
│   │   │   │   ├── OrphanRepository.cs
│   │   │   │   ├── PeriodicReportRepository.cs
│   │   │   │   ├── OfficeProjectRepository.cs
│   │   │   │   ├── MissionRepository.cs
│   │   │   │   ├── SeasonalAidRepository.cs
│   │   │   │   ├── HousingProjectRepository.cs
│   │   │   │   ├── CheckRepository.cs
│   │   │   │   ├── EmployeeRepository.cs
│   │   │   │   ├── ImportExportRepository.cs
│   │   │   │   ├── NotificationRepository.cs
│   │   │   │   └── AuditLogRepository.cs
│   │   │   ├── UnitOfWork/              # Unit of Work Pattern
│   │   │   │   └── UnitOfWork.cs
│   │   │   └── Seeds/                   # Data Seeding
│   │   │       └── DataSeeder.cs
│   │   ├── Hubs/                        # SignalR Hubs
│   │   │   ├── NotificationHub.cs
│   │   │   └── DashboardHub.cs
│   │   ├── BackgroundServices/          # Hosted Services
│   │   │   ├── NotificationCleanupService.cs
│   │   │   └── ReportGenerationService.cs
│   │   ├── Email/                       # Email Services
│   │   │   └── EmailService.cs
│   │   ├── SMS/                         # SMS Services
│   │   │   └── SMSService.cs
│   │   ├── Storage/                     # File Storage
│   │   │   └── FileStorageService.cs
│   │   ├── Caching/                     # Caching
│   │   │   └── CacheService.cs
│   │   ├── Logging/                     # Logging
│   │   │   └── LoggingService.cs
│   │   └── IIROSA.Infrastructure.csproj
│   │
│   ├── IIROSA.Web/                      # Presentation Layer (Web API)
│   │   ├── Controllers/
│   │   │   ├── Api/
│   │   │   │   ├── V1/
│   │   │   │   │   ├── UsersController.cs
│   │   │   │   │   ├── RolesController.cs
│   │   │   │   │   ├── CharitiesController.cs
│   │   │   │   │   ├── FamiliesController.cs
│   │   │   │   │   ├── OrphansController.cs
│   │   │   │   │   ├── PeriodicReportsController.cs
│   │   │   │   │   ├── OfficeProjectsController.cs
│   │   │   │   │   ├── MissionsController.cs
│   │   │   │   │   ├── SeasonalAidController.cs
│   │   │   │   │   ├── HousingProjectsController.cs
│   │   │   │   │   ├── ChecksController.cs
│   │   │   │   │   ├── EmployeesController.cs
│   │   │   │   │   ├── ImportsExportsController.cs
│   │   │   │   │   ├── NotificationsController.cs
│   │   │   │   │   ├── DashboardController.cs
│   │   │   │   │   └── AuditLogsController.cs
│   │   │   │   └── V2/                 # Future API versions
│   │   │   └── BaseController.cs
│   │   ├── Filters/                     # Action Filters
│   │   │   ├── ValidateModelAttribute.cs
│   │   │   ├── AuthorizeRoleAttribute.cs
│   │   │   └── AuditLogAttribute.cs
│   │   ├── Middleware/                  # Custom Middleware
│   │   │   ├── ExceptionHandlerMiddleware.cs
│   │   │   └── AuditLogMiddleware.cs
│   │   ├── Models/                      # View Models
│   │   │   └── ApiResponse.cs
│   │   ├── Extensions/                  # Extensions
│   │   │   ├── ServiceCollectionExtensions.cs
│   │   │   └── HttpContextExtensions.cs
│   │   ├── wwwroot/
│   │   │   ├── js/
│   │   │   ├── css/
│   │   │   └── lib/                     # Frontend libraries
│   │   ├── appsettings.json
│   │   ├── appsettings.Development.json
│   │   ├── appsettings.Production.json
│   │   └── IIROSA.Web.csproj
│   │
│   └── IIROSA.Tests/                    # Test Project
│       ├── Unit/
│       │   ├── Services/
│       │   ├── Repositories/
│       │   └── Controllers/
│       ├── Integration/
│       └── IIROSA.Tests.csproj
│
├── docs/                                # Documentation
│   ├── Architecture/
│   │   ├── RepositoryPattern.md
│   │   ├── ServiceLayer.md
│   │   └── DependencyInjection.md
│   ├── API/
│   │   └── SwaggerDocumentation.md
│   └── Database/
│       └── Schema.sql
│
├── .gitignore
├── README.md
└── IIROSA.System.sln
```

---

## Layer Responsibilities

### **1. Domain Layer (IIROSA.Domain)**
- **Purpose:** Core business entities and domain logic
- **Contains:** Entity classes, domain interfaces, value objects, specifications
- **Dependencies:** None (pure .NET library)
- **Rules:** No external dependencies, no EF Core, pure POCOs

### **2. Application Layer (IIROSA.Application)**
- **Purpose:** Business logic and orchestration
- **Contains:** Service interfaces & implementations, DTOs, validators, mappers
- **Dependencies:** Domain layer
- **Rules:** No data access code, no infrastructure concerns

### **3. Infrastructure Layer (IIROSA.Infrastructure)**
- **Purpose:** Data access and external services
- **Contains:** EF Core, repositories, SignalR, email, SMS, caching
- **Dependencies:** Domain and Application layers
- **Rules:** Implements interfaces from Application layer

### **4. Presentation Layer (IIROSA.Web)**
- **Purpose:** API endpoints and UI
- **Contains:** Controllers, filters, middleware, frontend
- **Dependencies:** All layers
- **Rules:** Thin controllers, delegate to services

---

## Repository Pattern Implementation

### **Generic Repository Interface (Domain Layer)**

```csharp
// IIROSA.Domain/Interfaces/IRepository.cs
namespace IIROSA.Domain.Interfaces;

public interface IRepository<T> where T : class
{
    // Query Operations
    Task<T?> GetByIdAsync(Guid id);
    Task<T?> GetByIdAsync(int id);
    Task<IEnumerable<T>> GetAllAsync();
    Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);
    Task<(IEnumerable<T> Items, int TotalCount)> GetPagedAsync(
        Expression<Func<T, bool>>? filter = null,
        Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
        int pageNumber = 1,
        int pageSize = 10);

    // Include Operations (for eager loading)
    IIncludableQueryable<T, TProperty> Include<TProperty>(
        Expression<Func<T, TProperty>> navigationPropertyPath);

    // CRUD Operations
    Task AddAsync(T entity);
    Task AddRangeAsync(IEnumerable<T> entities);
    void Update(T entity);
    void UpdateRange(IEnumerable<T> entities);
    void Delete(T entity);
    void DeleteRange(IEnumerable<T> entities);

    // Raw SQL
    Task<IEnumerable<T>> ExecuteSqlRawAsync(string sql, params object[] parameters);

    // Count Operations
    Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null);
    Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate);
}
```

### **Generic Repository Implementation (Infrastructure Layer)**

```csharp
// IIROSA.Infrastructure/Data/Repository/Repository.cs
using IIROSA.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;

namespace IIROSA.Infrastructure.Data.Repository;

public class Repository<T> : IRepository<T> where T : class
{
    protected readonly ApplicationDbContext _context;
    protected readonly DbSet<T> _dbSet;

    public Repository(ApplicationDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    public virtual async Task<T?> GetByIdAsync(Guid id)
    {
        return await _dbSet.FindAsync(id);
    }

    public virtual async Task<T?> GetByIdAsync(int id)
    {
        return await _dbSet.FindAsync(id);
    }

    public virtual async Task<IEnumerable<T>> GetAllAsync()
    {
        return await _dbSet.ToListAsync();
    }

    public virtual async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
    {
        return await _dbSet.Where(predicate).ToListAsync();
    }

    public virtual async Task<(IEnumerable<T> Items, int TotalCount)> GetPagedAsync(
        Expression<Func<T, bool>>? filter = null,
        Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
        int pageNumber = 1,
        int pageSize = 10)
    {
        IQueryable<T> query = _dbSet;

        // Apply filter
        if (filter != null)
        {
            query = query.Where(filter);
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

    public IIncludableQueryable<T, TProperty> Include<TProperty>(
        Expression<Func<T, TProperty>> navigationPropertyPath)
    {
        return _dbSet.Include(navigationPropertyPath);
    }

    public virtual async Task AddAsync(T entity)
    {
        await _dbSet.AddAsync(entity);
    }

    public virtual async Task AddRangeAsync(IEnumerable<T> entities)
    {
        await _dbSet.AddRangeAsync(entities);
    }

    public virtual void Update(T entity)
    {
        _dbSet.Update(entity);
    }

    public virtual void UpdateRange(IEnumerable<T> entities)
    {
        _dbSet.UpdateRange(entities);
    }

    public virtual void Delete(T entity)
    {
        if (_context.Entry(entity).State == EntityState.Detached)
        {
            _dbSet.Attach(entity);
        }
        _dbSet.Remove(entity);
    }

    public virtual void DeleteRange(IEnumerable<T> entities)
    {
        _dbSet.RemoveRange(entities);
    }

    public virtual async Task<IEnumerable<T>> ExecuteSqlRawAsync(string sql, params object[] parameters)
    {
        return await _dbSet.FromSqlRaw(sql, parameters).ToListAsync();
    }

    public virtual async Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null)
    {
        return predicate == null
            ? await _dbSet.CountAsync()
            : await _dbSet.CountAsync(predicate);
    }

    public virtual async Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate)
    {
        return await _dbSet.AnyAsync(predicate);
    }
}
```

### **Specific Repository Example (Infrastructure Layer)**

```csharp
// IIROSA.Infrastructure/Data/Repository/CharityRepository.cs
using IIROSA.Domain.Entities;
using IIROSA.Domain.Interfaces;

namespace IIROSA.Infrastructure.Data.Repository;

public class CharityRepository : Repository<Charity>, ICharityRepository
{
    public CharityRepository(ApplicationDbContext context) : base(context)
    {
    }

    // Domain-specific query methods
    public async Task<IEnumerable<Charity>> GetActiveCharitiesAsync()
    {
        return await FindAsync(c => c.IsActive && !c.IsLocked);
    }

    public async Task<IEnumerable<Charity>> GetCharitiesByRegionAsync(Guid regionId)
    {
        return await FindAsync(c => c.FK_RegionId == regionId);
    }

    public async Task<Charity?> GetCharityWithUsersAsync(Guid charityId)
    {
        return await _context.Charities
            .Include(c => c.User)
            .Include(c => c.Families)
            .Include(c => c.Orphans)
            .FirstOrDefaultAsync(c => c.Id == charityId);
    }

    public async Task<bool> IsCharityNameUniqueAsync(string name, Guid? excludeId = null)
    {
        return excludeId == null
            ? !await ExistsAsync(c => c.Name == name)
            : !await ExistsAsync(c => c.Name == name && c.Id != excludeId.Value);
    }

    public async Task<int> GetTotalOrphansCountAsync(Guid charityId)
    {
        return await _context.Orphans
            .CountAsync(o => o.FK_CharityId == charityId && !o.IsDeleted);
    }

    public async Task<int> GetTotalFamiliesCountAsync(Guid charityId)
    {
        return await _context.Families
            .CountAsync(f => f.FK_CharityId == charityId && !f.IsDeleted);
    }
}

// Interface in Domain Layer
public interface ICharityRepository : IRepository<Charity>
{
    Task<IEnumerable<Charity>> GetActiveCharitiesAsync();
    Task<IEnumerable<Charity>> GetCharitiesByRegionAsync(Guid regionId);
    Task<Charity?> GetCharityWithUsersAsync(Guid charityId);
    Task<bool> IsCharityNameUniqueAsync(string name, Guid? excludeId = null);
    Task<int> GetTotalOrphansCountAsync(Guid charityId);
    Task<int> GetTotalFamiliesCountAsync(Guid charityId);
}
```

---

## Unit of Work Pattern

### **Unit of Work Interface (Domain Layer)**

```csharp
// IIROSA.Domain/Interfaces/IUnitOfWork.cs
namespace IIROSA.Domain.Interfaces;

public interface IUnitOfWork : IDisposable
{
    // Repositories
    ICharityRepository Charities { get; }
    IFamilyRepository Families { get; }
    IOrphanRepository Orphans { get; }
    IPeriodicReportRepository PeriodicReports { get; }
    IOfficeProjectRepository OfficeProjects { get; }
    IMissionRepository Missions { get; }
    ISeasonalAidRepository SeasonalAidCampaigns { get; }
    IHousingProjectRepository HousingProjects { get; }
    ICheckRepository Checks { get; }
    IEmployeeRepository Employees { get; }
    IImportExportRepository ImportExports { get; }
    INotificationRepository Notifications { get; }
    IAuditLogRepository AuditLogs { get; }

    // Transaction Management
    Task<int> SaveChangesAsync();
    Task BeginTransactionAsync();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();
}
```

### **Unit of Work Implementation (Infrastructure Layer)**

```csharp
// IIROSA.Infrastructure/Data/UnitOfWork/UnitOfWork.cs
using IIROSA.Domain.Interfaces;
using Microsoft.EntityFrameworkCore.Storage;

namespace IIROSA.Infrastructure.Data.UnitOfWork;

public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;
    private IDbContextTransaction? _transaction;
    private readonly Dictionary<Type, object> _repositories = new();

    public UnitOfWork(ApplicationDbContext context)
    {
        _context = context;
    }

    // Lazy-loaded repositories
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

    public INotificationRepository Notifications =>
        GetRepository<INotificationRepository, NotificationRepository>();

    public IAuditLogRepository AuditLogs =>
        GetRepository<IAuditLogRepository, AuditLogRepository>();

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
        // Set audit fields automatically
        SetAuditFields();

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

    private void SetAuditFields()
    {
        var currentUserId = GetCurrentUserId();
        var currentDate = DateTime.UtcNow;

        foreach (var entry in _context.ChangeTracker.Entries<IAuditable>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedDate = currentDate;
                    entry.Entity.CreatedBy = currentUserId;
                    entry.Entity.ModifiedDate = currentDate;
                    entry.Entity.ModifiedBy = currentUserId;
                    break;

                case EntityState.Modified:
                    entry.Entity.ModifiedDate = currentDate;
                    entry.Entity.ModifiedBy = currentUserId;
                    // Don't modify CreatedDate/CreatedBy
                    break;

                case EntityState.Deleted:
                    entry.Entity.DeletedDate = currentDate;
                    entry.Entity.DeletedBy = currentUserId;
                    entry.Entity.IsDeleted = true;
                    // Soft delete
                    entry.State = EntityState.Modified;
                    break;
            }
        }
    }

    private Guid GetCurrentUserId()
    {
        // Get from HttpContext or claims
        // Implementation depends on your authentication setup
        return Guid.Empty; // Placeholder
    }

    public void Dispose()
    {
        _transaction?.Dispose();
        _context.Dispose();
    }
}

// Audit interface
public interface IAuditable
{
    DateTime CreatedDate { get; set; }
    Guid? CreatedBy { get; set; }
    DateTime ModifiedDate { get; set; }
    Guid? ModifiedBy { get; set; }
    DateTime? DeletedDate { get; set; }
    Guid? DeletedBy { get; set; }
    bool IsDeleted { get; set; }
}
```

---

## Service Layer Implementation

### **Base Service Interface (Application Layer)**

```csharp
// IIROSA.Application/Interfaces/IService.cs
namespace IIROSA.Application.Interfaces;

public interface IService<TDto, TEntity> where TDto : class where TEntity : class
{
    // CRUD Operations
    Task<TDto?> GetByIdAsync(Guid id);
    Task<IEnumerable<TDto>> GetAllAsync();
    Task<(IEnumerable<TDto> Items, int TotalCount)> GetPagedAsync(
        int pageNumber = 1,
        int pageSize = 10,
        string? filter = null,
        string? sortBy = null);

    Task<TDto> CreateAsync(TDto dto);
    Task<TDto> UpdateAsync(Guid id, TDto dto);
    Task DeleteAsync(Guid id);

    // Validation
    Task<ValidationResult> ValidateAsync(TDto dto);
}
```

### **Base Service Implementation (Application Layer)**

```csharp
// IIROSA.Application/Services/Service.cs
using IIROSA.Domain.Interfaces;
using IIROSA.Application.Interfaces;
using IIROSA.Application.Exceptions;
using AutoMapper;

namespace IIROSA.Application.Services;

public abstract class Service<TDto, TEntity, TRepository> : IService<TDto, TEntity>
    where TDto : class
    where TEntity : class, IAuditable
    where TRepository : IRepository<TEntity>
{
    protected readonly TRepository _repository;
    protected readonly IUnitOfWork _unitOfWork;
    protected readonly IMapper _mapper;
    protected readonly ILogger _logger;

    protected Service(
        TRepository repository,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger logger)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public virtual async Task<TDto?> GetByIdAsync(Guid id)
    {
        var entity = await _repository.GetByIdAsync(id);
        if (entity == null)
        {
            _logger.LogWarning("{EntityType} with ID {Id} not found", typeof(TEntity).Name, id);
            return null;
        }

        return _mapper.Map<TDto>(entity);
    }

    public virtual async Task<IEnumerable<TDto>> GetAllAsync()
    {
        var entities = await _repository.GetAllAsync();
        return _mapper.Map<IEnumerable<TDto>>(entities);
    }

    public virtual async Task<(IEnumerable<TDto> Items, int TotalCount)> GetPagedAsync(
        int pageNumber = 1,
        int pageSize = 10,
        string? filter = null,
        string? sortBy = null)
    {
        // Build filter expression dynamically
        Expression<Func<TEntity, bool>>? filterExpression = null;

        if (!string.IsNullOrEmpty(filter))
        {
            // Parse filter and build expression
            // This is simplified - you'd want a more sophisticated filter parser
            filterExpression = BuildFilterExpression(filter);
        }

        // Build sort expression dynamically
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderByExpression = null;

        if (!string.IsNullOrEmpty(sortBy))
        {
            orderByExpression = BuildOrderByExpression(sortBy);
        }

        var (entities, totalCount) = await _repository.GetPagedAsync(
            filterExpression,
            orderByExpression,
            pageNumber,
            pageSize);

        var dtos = _mapper.Map<IEnumerable<TDto>>(entities);
        return (dtos, totalCount);
    }

    public virtual async Task<TDto> CreateAsync(TDto dto)
    {
        // Validate
        var validationResult = await ValidateAsync(dto);
        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }

        // Map to entity
        var entity = _mapper.Map<TEntity>(dto);

        // Additional setup before save (override in derived classes)
        await BeforeCreateAsync(entity, dto);

        // Add to repository
        await _repository.AddAsync(entity);

        // Save changes
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("{EntityType} created with ID {Id}", typeof(TEntity).Name,
            ((IAuditable)entity).Id);

        // Return created DTO
        return _mapper.Map<TDto>(entity);
    }

    public virtual async Task<TDto> UpdateAsync(Guid id, TDto dto)
    {
        var entity = await _repository.GetByIdAsync(id);
        if (entity == null)
        {
            throw new NotFoundException(typeof(TEntity).Name, id);
        }

        // Validate
        var validationResult = await ValidateAsync(dto);
        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }

        // Map changes to entity
        _mapper.Map(dto, entity);

        // Additional processing before update (override in derived classes)
        await BeforeUpdateAsync(entity, dto);

        // Update in repository
        _repository.Update(entity);

        // Save changes
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("{EntityType} with ID {Id} updated", typeof(TEntity).Name, id);

        return _mapper.Map<TDto>(entity);
    }

    public virtual async Task DeleteAsync(Guid id)
    {
        var entity = await _repository.GetByIdAsync(id);
        if (entity == null)
        {
            throw new NotFoundException(typeof(TEntity).Name, id);
        }

        // Check if can be deleted (override in derived classes for business rules)
        await BeforeDeleteAsync(entity);

        // Soft delete
        _repository.Delete(entity);

        // Save changes
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("{EntityType} with ID {Id} deleted", typeof(TEntity).Name, id);
    }

    public virtual async Task<ValidationResult> ValidateAsync(TDto dto)
    {
        // Use FluentValidation or custom validation
        return new ValidationResult();
    }

    // Override these in derived classes for custom logic
    protected virtual Task BeforeCreateAsync(TEntity entity, TDto dto) => Task.CompletedTask;
    protected virtual Task BeforeUpdateAsync(TEntity entity, TDto dto) => Task.CompletedTask;
    protected virtual Task BeforeDeleteAsync(TEntity entity) => Task.CompletedTask;

    // Helper methods
    protected virtual Expression<Func<TEntity, bool>>? BuildFilterExpression(string filter)
    {
        // Implement dynamic filter building
        return null;
    }

    protected virtual Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? BuildOrderByExpression(string sortBy)
    {
        // Implement dynamic sorting
        return null;
    }
}
```

### **CharityService Example (Application Layer)**

```csharp
// IIROSA.Application/Interfaces/ICharityService.cs
namespace IIROSA.Application.Interfaces;

public interface ICharityService : IService<CharityDto, Charity>
{
    Task<IEnumerable<CharityDto>> GetActiveCharitiesAsync();
    Task<CharityDto?> GetCharityWithUsersAsync(Guid id);
    Task<bool> IsNameUniqueAsync(string name, Guid? excludeId = null);
    Task<CharityStatisticsDto> GetStatisticsAsync(Guid charityId);
    Task ActivateCharityAsync(Guid id);
    Task DeactivateCharityAsync(Guid id);
    Task LockCharityAsync(Guid id);
    Task UnlockCharityAsync(Guid id);
    Task UpdateRightsAsync(Guid id, bool canAdd, bool canUpdate);
}

// IIROSA.Application/Services/CharityService.cs
using IIROSA.Application.Interfaces;
using IIROSA.Application.DTOs.Charity;
using IIROSA.Domain.Entities;
using IIROSA.Domain.Interfaces;
using IIROSA.Application.Exceptions;

namespace IIROSA.Application.Services;

public class CharityService : Service<CharityDto, Charity, ICharityRepository>, ICharityService
{
    private readonly ICharityRepository _charityRepository;
    private readonly INotificationService _notificationService;

    public CharityService(
        ICharityRepository repository,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<CharityService> logger,
        INotificationService notificationService)
        : base(repository, unitOfWork, mapper, logger)
    {
        _charityRepository = repository;
        _notificationService = notificationService;
    }

    public async Task<IEnumerable<CharityDto>> GetActiveCharitiesAsync()
    {
        var charities = await _charityRepository.GetActiveCharitiesAsync();
        return _mapper.Map<IEnumerable<CharityDto>>(charities);
    }

    public async Task<CharityDto?> GetCharityWithUsersAsync(Guid id)
    {
        var charity = await _charityRepository.GetCharityWithUsersAsync(id);
        return charity == null ? null : _mapper.Map<CharityDto>(charity);
    }

    public async Task<bool> IsNameUniqueAsync(string name, Guid? excludeId = null)
    {
        return await _charityRepository.IsCharityNameUniqueAsync(name, excludeId);
    }

    public async Task<CharityStatisticsDto> GetStatisticsAsync(Guid charityId)
    {
        var orphanCount = await _charityRepository.GetTotalOrphansCountAsync(charityId);
        var familyCount = await _charityRepository.GetTotalFamiliesCountAsync(charityId);

        return new CharityStatisticsDto
        {
            CharityId = charityId,
            TotalOrphans = orphanCount,
            TotalFamilies = familyCount
        };
    }

    public async Task ActivateCharityAsync(Guid id)
    {
        var charity = await _charityRepository.GetByIdAsync(id);
        if (charity == null)
            throw new NotFoundException(nameof(Charity), id);

        charity.IsActive = true;
        charity.IsLocked = false;

        _charityRepository.Update(charity);
        await _unitOfWork.SaveChangesAsync();

        // Notify charity
        await _notificationService.SendNotification(
            userId: charity.UserId,
            type: NotificationType.Success,
            title: "Account Activated",
            message: "Your charity account has been activated. You can now access the system.",
            category: "System"
        );

        _logger.LogInformation("Charity {CharityId} activated", id);
    }

    public async Task DeactivateCharityAsync(Guid id)
    {
        var charity = await _charityRepository.GetByIdAsync(id);
        if (charity == null)
            throw new NotFoundException(nameof(Charity), id);

        charity.IsActive = false;

        _charityRepository.Update(charity);
        await _unitOfWork.SaveChangesAsync();

        // Notify charity
        await _notificationService.SendNotification(
            userId: charity.UserId,
            type: NotificationType.Warning,
            title: "Account Deactivated",
            message: "Your charity account has been deactivated. Please contact administrator.",
            category: "Alert"
        );

        _logger.LogInformation("Charity {CharityId} deactivated", id);
    }

    public async Task LockCharityAsync(Guid id)
    {
        var charity = await _charityRepository.GetByIdAsync(id);
        if (charity == null)
            throw new NotFoundException(nameof(Charity), id);

        charity.IsLocked = true;

        _charityRepository.Update(charity);
        await _unitOfWork.SaveChangesAsync();

        // Notify charity
        await _notificationService.SendNotification(
            userId: charity.UserId,
            type: NotificationType.Error,
            title: "Account Locked",
            message: "Your charity account has been locked. All operations are suspended.",
            category: "Alert",
            priority: NotificationPriority.Urgent
        );

        _logger.LogInformation("Charity {CharityId} locked", id);
    }

    public async Task UnlockCharityAsync(Guid id)
    {
        var charity = await _charityRepository.GetByIdAsync(id);
        if (charity == null)
            throw new NotFoundException(nameof(Charity), id);

        charity.IsLocked = false;

        _charityRepository.Update(charity);
        await _unitOfWork.SaveChangesAsync();

        // Notify charity
        await _notificationService.SendNotification(
            userId: charity.UserId,
            type: NotificationType.Success,
            title: "Account Unlocked",
            message: "Your charity account has been unlocked. You can resume operations.",
            category: "System"
        );

        _logger.LogInformation("Charity {CharityId} unlocked", id);
    }

    public async Task UpdateRightsAsync(Guid id, bool canAdd, bool canUpdate)
    {
        var charity = await _charityRepository.GetByIdAsync(id);
        if (charity == null)
            throw new NotFoundException(nameof(Charity), id);

        charity.IsAddEnabled = canAdd;
        charity.IsUpdateEnabled = canUpdate;

        _charityRepository.Update(charity);
        await _unitOfWork.SaveChangesAsync();

        // Notify charity
        await _notificationService.SendNotification(
            userId: charity.UserId,
            type: NotificationType.Info,
            title: "Permissions Updated",
            message: $"Your permissions have been updated. Add: {canAdd}, Update: {canUpdate}",
            category: "System"
        );

        _logger.LogInformation("Charity {CharityId} rights updated: Add={Add}, Update={Update}",
            id, canAdd, canUpdate);
    }

    protected override async Task BeforeCreateAsync(Charity entity, CharityDto dto)
    {
        // Check name uniqueness
        if (!await IsNameUniqueAsync(entity.Name))
        {
            throw new BusinessException("Charity name already exists");
        }

        // Generate code if not provided
        if (string.IsNullOrEmpty(entity.Code))
        {
            entity.Code = await GenerateCharityCodeAsync();
        }

        await Task.CompletedTask;
    }

    private async Task<string> GenerateCharityCodeAsync()
    {
        // Generate unique charity code
        return $"CHR{DateTime.UtcNow:yyyyMMddHHmmss}";
    }
}
```

### **DTO Example (Application Layer)**

```csharp
// IIROSA.Application/DTOs/Charity/CharityDto.cs
namespace IIROSA.Application.DTOs.Charity;

public class CharityDto
{
    public Guid Id { get; set; }
    public string Code { get; set; }
    public string Name { get; set; }
    public string Address { get; set; }
    public string City { get; set; }
    public string Phone { get; set; }
    public string Email { get; set; }
    public Guid? RegionId { get; set; }
    public string RegionName { get; set; }
    public Guid? CenterId { get; set; }
    public string CenterName { get; set; }
    public bool IsActive { get; set; }
    public bool IsLocked { get; set; }
    public bool IsAddEnabled { get; set; }
    public bool IsUpdateEnabled { get; set; }
    public DateTime CreatedDate { get; set; }
    public int TotalOrphans { get; set; }
    public int TotalFamilies { get; set; }
}

public class CreateCharityDto
{
    public string Name { get; set; }
    public string Code { get; set; }
    public string Address { get; set; }
    public string City { get; set; }
    public string Phone { get; set; }
    public string Email { get; set; }
    public Guid? RegionId { get; set; }
    public Guid? CenterId { get; set; }
    public string BossName { get; set; }
    public string ResponsibleJobName { get; set; }
    public bool CreateUserAccount { get; set; }
}

public class UpdateCharityDto
{
    public string Name { get; set; }
    public string Address { get; set; }
    public string Phone { get; set; }
    public string Email { get; set; }
    public Guid? RegionId { get; set; }
    public Guid? CenterId { get; set; }
}

public class CharityStatisticsDto
{
    public Guid CharityId { get; set; }
    public int TotalOrphans { get; set; }
    public int TotalFamilies { get; set; }
}
```

---

## Controller Implementation (Presentation Layer)

### **Base Controller**

```csharp
// IIROSA.Web/Controllers/BaseController.cs
using IIROSA.Application.Interfaces;
using IIROSA.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace IIROSA.Web.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize]
public abstract class BaseController<TDto, TEntity> : ControllerBase
    where TDto : class
    where TEntity : class
{
    protected readonly IService<TDto, TEntity> _service;
    protected readonly ILogger _logger;

    protected BaseController(IService<TDto, TEntity> service, ILogger logger)
    {
        _service = service;
        _logger = logger;
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponse<TDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public virtual async Task<ActionResult<ApiResponse<TDto>>> GetById(Guid id)
    {
        try
        {
            var result = await _service.GetByIdAsync(id);
            if (result == null)
            {
                return NotFound(ApiResponse<TDto>.NotFound($"{typeof(TEntity).Name} not found"));
            }

            return Ok(ApiResponse<TDto>.Success(result));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving {EntityType} with ID {Id}", typeof(TEntity).Name, id);
            return StatusCode(StatusCodes.Status500InternalServerError,
                ApiResponse<TDto>.Error("An error occurred while retrieving data"));
        }
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<TDto>>), StatusCodes.Status200OK)]
    public virtual async Task<ActionResult<ApiResponse<IEnumerable<TDto>>>> GetAll(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? filter = null,
        [FromQuery] string? sortBy = null)
    {
        try
        {
            var (items, totalCount) = await _service.GetPagedAsync(pageNumber, pageSize, filter, sortBy);

            return Ok(ApiResponse<IEnumerable<TDto>>.Success(items, totalCount, pageNumber, pageSize));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving {EntityType} list", typeof(TEntity).Name);
            return StatusCode(StatusCodes.Status500InternalServerError,
                ApiResponse<IEnumerable<TDto>>.Error("An error occurred while retrieving data"));
        }
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<TDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public virtual async Task<ActionResult<ApiResponse<TDto>>> Create([FromBody] TDto dto)
    {
        try
        {
            var result = await _service.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = ((dynamic)result).Id },
                ApiResponse<TDto>.Success(result, "Created successfully"));
        }
        catch (ValidationException ex)
        {
            return BadRequest(ApiResponse<TDto>.ValidationError(ex.Errors));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating {EntityType}", typeof(TEntity).Name);
            return StatusCode(StatusCodes.Status500InternalServerError,
                ApiResponse<TDto>.Error("An error occurred while creating data"));
        }
    }

    [HttpPut("{id}")]
    [ProducesResponseType(typeof(ApiResponse<TDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public virtual async Task<ActionResult<ApiResponse<TDto>>> Update(Guid id, [FromBody] TDto dto)
    {
        try
        {
            var result = await _service.UpdateAsync(id, dto);
            return Ok(ApiResponse<TDto>.Success(result, "Updated successfully"));
        }
        catch (NotFoundException ex)
        {
            return NotFound(ApiResponse<TDto>.NotFound(ex.Message));
        }
        catch (ValidationException ex)
        {
            return BadRequest(ApiResponse<TDto>.ValidationError(ex.Errors));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating {EntityType} with ID {Id}", typeof(TEntity).Name, id);
            return StatusCode(StatusCodes.Status500InternalServerError,
                ApiResponse<TDto>.Error("An error occurred while updating data"));
        }
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public virtual async Task<ActionResult<ApiResponse>> Delete(Guid id)
    {
        try
        {
            await _service.DeleteAsync(id);
            return Ok(ApiResponse.Success("Deleted successfully"));
        }
        catch (NotFoundException ex)
        {
            return NotFound(ApiResponse.NotFound(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting {EntityType} with ID {Id}", typeof(TEntity).Name, id);
            return StatusCode(StatusCodes.Status500InternalServerError,
                ApiResponse.Error("An error occurred while deleting data"));
        }
    }
}
```

### **CharityController Example**

```csharp
// IIROSA.Web/Controllers/Api/V1/CharitiesController.cs
using IIROSA.Application.Interfaces;
using IIROSA.Application.DTOs.Charity;
using IIROSA.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace IIROSA.Web.Controllers.Api.V1;

public class CharitiesController : BaseController<CharityDto, Charity>
{
    private readonly ICharityService _charityService;

    public CharitiesController(ICharityService charityService, ILogger<CharitiesController> logger)
        : base(charityService, logger)
    {
        _charityService = charityService;
    }

    // Additional endpoints specific to Charities

    [HttpGet("active")]
    public async Task<ActionResult<ApiResponse<IEnumerable<CharityDto>>>> GetActiveCharities()
    {
        try
        {
            var charities = await _charityService.GetActiveCharitiesAsync();
            return Ok(ApiResponse<IEnumerable<CharityDto>>.Success(charities));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving active charities");
            return StatusCode(StatusCodes.Status500InternalServerError,
                ApiResponse<IEnumerable<CharityDto>>.Error("An error occurred"));
        }
    }

    [HttpGet("{id}/with-users")]
    public async Task<ActionResult<ApiResponse<CharityDto>>> GetCharityWithUsers(Guid id)
    {
        try
        {
            var charity = await _charityService.GetCharityWithUsersAsync(id);
            if (charity == null)
            {
                return NotFound(ApiResponse<CharityDto>.NotFound("Charity not found"));
            }

            return Ok(ApiResponse<CharityDto>.Success(charity));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving charity with users");
            return StatusCode(StatusCodes.Status500InternalServerError,
                ApiResponse<CharityDto>.Error("An error occurred"));
        }
    }

    [HttpGet("{id}/statistics")]
    public async Task<ActionResult<ApiResponse<CharityStatisticsDto>>> GetStatistics(Guid id)
    {
        try
        {
            var stats = await _charityService.GetStatisticsAsync(id);
            return Ok(ApiResponse<CharityStatisticsDto>.Success(stats));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving charity statistics");
            return StatusCode(StatusCodes.Status500InternalServerError,
                ApiResponse<CharityStatisticsDto>.Error("An error occurred"));
        }
    }

    [HttpPost("{id}/activate")]
    public async Task<ActionResult<ApiResponse>> Activate(Guid id)
    {
        try
        {
            await _charityService.ActivateCharityAsync(id);
            return Ok(ApiResponse.Success("Charity activated successfully"));
        }
        catch (NotFoundException ex)
        {
            return NotFound(ApiResponse.NotFound(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error activating charity");
            return StatusCode(StatusCodes.Status500InternalServerError,
                ApiResponse.Error("An error occurred"));
        }
    }

    [HttpPost("{id}/deactivate")]
    public async Task<ActionResult<ApiResponse>> Deactivate(Guid id)
    {
        try
        {
            await _charityService.DeactivateCharityAsync(id);
            return Ok(ApiResponse.Success("Charity deactivated successfully"));
        }
        catch (NotFoundException ex)
        {
            return NotFound(ApiResponse.NotFound(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deactivating charity");
            return StatusCode(StatusCodes.Status500InternalServerError,
                ApiResponse.Error("An error occurred"));
        }
    }

    [HttpPost("{id}/lock")]
    public async Task<ActionResult<ApiResponse>> Lock(Guid id)
    {
        try
        {
            await _charityService.LockCharityAsync(id);
            return Ok(ApiResponse.Success("Charity locked successfully"));
        }
        catch (NotFoundException ex)
        {
            return NotFound(ApiResponse.NotFound(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error locking charity");
            return StatusCode(StatusCodes.Status500InternalServerError,
                ApiResponse.Error("An error occurred"));
        }
    }

    [HttpPost("{id}/unlock")]
    public async Task<ActionResult<ApiResponse>> Unlock(Guid id)
    {
        try
        {
            await _charityService.UnlockCharityAsync(id);
            return Ok(ApiResponse.Success("Charity unlocked successfully"));
        }
        catch (NotFoundException ex)
        {
            return NotFound(ApiResponse.NotFound(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error unlocking charity");
            return StatusCode(StatusCodes.Status500InternalServerError,
                ApiResponse.Error("An error occurred"));
        }
    }

    [HttpPut("{id}/rights")]
    public async Task<ActionResult<ApiResponse>> UpdateRights(Guid id, [FromBody] UpdateRightsDto dto)
    {
        try
        {
            await _charityService.UpdateRightsAsync(id, dto.CanAdd, dto.CanUpdate);
            return Ok(ApiResponse.Success("Charity rights updated successfully"));
        }
        catch (NotFoundException ex)
        {
            return NotFound(ApiResponse.NotFound(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating charity rights");
            return StatusCode(StatusCodes.Status500InternalServerError,
                ApiResponse.Error("An error occurred"));
        }
    }
}
```

---

## Dynamic Dependency Injection Configuration

### **Service Collection Extension (Infrastructure Layer)**

```csharp
// IIROSA.Infrastructure/DependencyInjection/ServiceCollectionExtensions.cs
using IIROSA.Domain.Interfaces;
using IIROSA.Infrastructure.Data.Repository;
using IIROSA.Infrastructure.Data.UnitOfWork;
using Microsoft.Extensions.DependencyInjection;

namespace IIROSA.Infrastructure.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Database
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                sqlOptions => sqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 3,
                    maxRetryDelay: TimeSpan.FromSeconds(30),
                    errorNumbersToAdd: null)
            ));

        // Register Unit of Work
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Register Repositories (Dynamic Registration)
        services.RegisterRepositories();

        // Register SignalR Hubs
        services.AddSignalR();

        // Register Infrastructure Services
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<ISMSService, SMSService>();
        services.AddScoped<IFileStorageService, FileStorageService>();
        services.AddScoped<ICacheService, CacheService>();
        services.AddScoped<ILoggingService, LoggingService>();

        // Register Background Services
        services.AddHostedService<NotificationCleanupService>();
        services.AddHostedService<ReportGenerationService>();

        return services;
    }

    private static IServiceCollection RegisterRepositories(this IServiceCollection services)
    {
        // Use reflection to dynamically register all repositories
        var assembly = typeof(Repository<>).Assembly;

        // Find all repository implementations
        var repositoryTypes = assembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && t.Name.EndsWith("Repository"))
            .ToList();

        foreach (var repoType in repositoryTypes)
        {
            // Get the interface it implements (e.g., ICharityRepository)
            var interfaceType = repoType.GetInterfaces()
                .FirstOrDefault(i => i.Name == $"I{repoType.Name}");

            if (interfaceType != null)
            {
                // Register as scoped
                services.AddScoped(interfaceType, repoType);
            }
        }

        return services;
    }
}
```

### **Service Registration Extension (Application Layer)**

```csharp
// IIROSA.Application/DependencyInjection/ServiceCollectionExtensions.cs
using IIROSA.Application.Interfaces;
using IIROSA.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace IIROSA.Application.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // Register Application Services (Dynamic Registration)
        services.RegisterApplicationServices();

        // Register AutoMapper
        services.AddAutoMapper(typeof(MappingProfile));

        // Register FluentValidation validators
        services.RegisterValidators();

        // Register HttpClient for external API calls
        services.AddHttpClient();

        return services;
    }

    private static IServiceCollection RegisterApplicationServices(this IServiceCollection services)
    {
        // Use reflection to dynamically register all services
        var assembly = typeof(Service<,,>).Assembly;

        // Find all service interfaces
        var serviceInterfaces = assembly.GetTypes()
            .Where(t => t.IsInterface && t.GetInterfaces().Any(i => i.IsGenericType &&
                i.GetGenericTypeDefinition() == typeof(IService<,>)))
            .ToList();

        foreach (var serviceInterface in serviceInterfaces)
        {
            // Find the implementation
            var serviceImplementation = assembly.GetTypes()
                .FirstOrDefault(t => serviceInterface.IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract);

            if (serviceImplementation != null)
            {
                // Register as scoped
                services.AddScoped(serviceInterface, serviceImplementation);
            }
        }

        // Register services that don't inherit from base Service class
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<IDashboardService, DashboardService>();
        services.AddScoped<IAuditLogService, AuditLogService>();
        services.AddScoped<IReportGenerationService, ReportGenerationService>();

        return services;
    }

    private static IServiceCollection RegisterValidators(this IServiceCollection services)
    {
        // Find all validators using reflection
        var assembly = typeof(MappingProfile).Assembly;

        var validatorTypes = assembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && t.Name.EndsWith("Validator"))
            .ToList();

        foreach (var validatorType in validatorTypes)
        {
            // Get the generic IValidator interface
            var validatorInterface = validatorType.GetInterfaces()
                .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IValidator<>));

            if (validatorInterface != null)
            {
                services.AddScoped(validatorInterface, validatorType);
            }
        }

        return services;
    }
}
```

### **Program.cs Configuration**

```csharp
// IIROSA.Web/Program.cs
using IIROSA.Application.DependencyInjection;
using IIROSA.Infrastructure.DependencyInjection;
using IIROSA.Web.Extensions;
using IIROSA.Web.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddInfrastructureServices(builder.Configuration);
builder.Services.AddApplicationServices();
builder.Services.AddWebServices(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "IIROSA API V1");
    });
}
else
{
    app.UseExceptionHandler("/error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

// Custom Middleware
app.UseMiddleware<ExceptionHandlerMiddleware>();
app.UseMiddleware<AuditLogMiddleware>();

// SignalR Hubs
app.MapHub<NotificationHub>("/notificationHub");
app.MapHub<DashboardHub>("/dashboardHub");

app.MapControllers();

app.Run();
```

---

## AutoMapper Configuration

```csharp
// IIROSA.Application/Mappers/MappingProfile.cs
using AutoMapper;
using IIROSA.Application.DTOs.Charity;
using IIROSA.Application.DTOs.Family;
using IIROSA.Application.DTOs.Orphan;
using IIROSA.Domain.Entities;

namespace IIROSA.Application.Mappers;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // Charity Mappings
        CreateMap<Charity, CharityDto>()
            .ForMember(dest => dest.RegionName, opt => opt.MapFrom(src => src.Region != null ? src.Region.Name : null))
            .ForMember(dest => dest.CenterName, opt => opt.MapFrom(src => src.Center != null ? src.Center.Name : null))
            .ForMember(dest => dest.TotalOrphans, opt => opt.MapFrom(src => src.Orphans != null ? src.Orphans.Count(o => !o.IsDeleted) : 0))
            .ForMember(dest => dest.TotalFamilies, opt => opt.MapFrom(src => src.Families != null ? src.Families.Count(f => !f.IsDeleted) : 0));

        CreateMap<CreateCharityDto, Charity>();
        CreateMap<UpdateCharityDto, Charity>();

        // Family Mappings
        CreateMap<Family, FamilyDto>();
        CreateMap<CreateFamilyDto, Family>();
        CreateMap<UpdateFamilyDto, Family>();

        // Orphan Mappings
        CreateMap<Orphan, OrphanDto>();
        CreateMap<CreateOrphanDto, Orphan>();
        CreateMap<UpdateOrphanDto, Orphan>();

        // Add mappings for all other entities...
    }
}
```

---

## Complete Module Example: Periodic Reports

### **1. Domain Entity**

```csharp
// IIROSA.Domain/Entities/PeriodicReport.cs
public class PeriodicReport : IAuditable
{
    public Guid Id { get; set; }
    public Guid? FK_ChildId { get; set; }
    public DateTime? TimeStamp { get; set; }
    public Guid? FK_User { get; set; }
    public Guid? FK_Reviewer { get; set; }
    public bool? Reviewed { get; set; }
    public DateTime? ReviewedDate { get; set; }
    public bool? Locked { get; set; }
    public DateTime? LockedDate { get; set; }
    public bool? Active { get; set; }
    public DateTime? ActiveDate { get; set; }
    public bool? Deleted { get; set; }
    public DateTime? DeletedDate { get; set; }
    public DateTime? ReportDate { get; set; }
    public DateTime? UpdateDate { get; set; }

    // Progress Tracking Fields (60+ fields from use case)
    public string? PrayerStatus { get; set; }
    public string? MannersStatus { get; set; }
    public string? QuranParts { get; set; }
    public string? QuranVerses { get; set; }
    public string? HadeethStatus { get; set; }
    public string? MedicalStatus { get; set; }
    public string? Disease { get; set; }
    public string? Disability { get; set; }
    // ... (all other fields from use case)

    // Navigation Properties
    public virtual Child Child { get; set; }
    public virtual aspnet_Users User { get; set; }
    public virtual aspnet_Users ReviewerUser { get; set; }
    public virtual EducationalStage EducationalStage { get; set; }
    public virtual EducationalLevel EducationalLevel { get; set; }

    // IAuditable implementation
    public DateTime CreatedDate { get; set; }
    public Guid? CreatedBy { get; set; }
    public DateTime ModifiedDate { get; set; }
    public Guid? ModifiedBy { get; set; }
    public DateTime? DeletedDate { get; set; }
    public Guid? DeletedBy { get; set; }
    public bool IsDeleted { get; set; }
}
```

### **2. Repository Interface & Implementation**

```csharp
// IIROSA.Domain/Interfaces/IPeriodicReportRepository.cs
public interface IPeriodicReportRepository : IRepository<PeriodicReport>
{
    Task<IEnumerable<PeriodicReport>> GetPendingReportsAsync();
    Task<IEnumerable<PeriodicReport>> GetApprovedReportsAsync();
    Task<IEnumerable<PeriodicReport>> GetRejectedReportsAsync();
    Task<IEnumerable<PeriodicReport>> GetReportsByOrphanAsync(Guid orphanId);
    Task<IEnumerable<PeriodicReport>> GetReportsByCharityAsync(Guid charityId, DateTime? startDate = null, DateTime? endDate = null);
    Task<PeriodicReport?> GetReportWithDetailsAsync(Guid id);
}

// IIROSA.Infrastructure/Data/Repository/PeriodicReportRepository.cs
public class PeriodicReportRepository : Repository<PeriodicReport>, IPeriodicReportRepository
{
    public PeriodicReportRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<PeriodicReport>> GetPendingReportsAsync()
    {
        return await _context.PeriodicReports
            .Include(pr => pr.Child)
            .Include(pr => pr.Child.Charity)
            .Where(pr => !pr.Reviewed.HasValue && pr.Active == true && !pr.IsDeleted)
            .OrderByDescending(pr => pr.ReportDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<PeriodicReport>> GetApprovedReportsAsync()
    {
        return await _context.PeriodicReports
            .Include(pr => pr.Child)
            .Where(pr => pr.Reviewed == true && pr.IsAccepted == true && !pr.IsDeleted)
            .OrderByDescending(pr => pr.ReportDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<PeriodicReport>> GetRejectedReportsAsync()
    {
        return await _context.PeriodicReports
            .Include(pr => pr.Child)
            .Where(pr => pr.Reviewed == true && pr.IsRefused == true && !pr.IsDeleted)
            .OrderByDescending(pr => pr.ReportDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<PeriodicReport>> GetReportsByOrphanAsync(Guid orphanId)
    {
        return await _context.PeriodicReports
            .Where(pr => pr.FK_ChildId == orphanId && !pr.IsDeleted)
            .OrderByDescending(pr => pr.TimeStamp)
            .ToListAsync();
    }

    public async Task<IEnumerable<PeriodicReport>> GetReportsByCharityAsync(Guid charityId, DateTime? startDate = null, DateTime? endDate = null)
    {
        var query = _context.PeriodicReports
            .Include(pr => pr.Child)
            .Where(pr => pr.Child.FK_CharityId == charityId && !pr.IsDeleted);

        if (startDate.HasValue)
            query = query.Where(pr => pr.ReportDate >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(pr => pr.ReportDate <= endDate.Value);

        return await query
            .OrderByDescending(pr => pr.ReportDate)
            .ToListAsync();
    }

    public async Task<PeriodicReport?> GetReportWithDetailsAsync(Guid id)
    {
        return await _context.PeriodicReports
            .Include(pr => pr.Child)
            .Include(pr => pr.Child.Charity)
            .Include(pr => pr.Child.Family)
            .Include(pr => pr.User)
            .Include(pr => pr.ReviewerUser)
            .Include(pr => pr.EducationalStage)
            .Include(pr => pr.EducationalLevel)
            .FirstOrDefaultAsync(pr => pr.Id == id);
    }
}
```

### **3. Service Interface & Implementation**

```csharp
// IIROSA.Application/Interfaces/IPeriodicReportService.cs
public interface IPeriodicReportService : IService<PeriodicReportDto, PeriodicReport>
{
    Task<IEnumerable<PeriodicReportDto>> GetPendingReportsAsync();
    Task<IEnumerable<PeriodicReportDto>> GetApprovedReportsAsync();
    Task<IEnumerable<PeriodicReportDto>> GetRejectedReportsAsync();
    Task<IEnumerable<PeriodicReportDto>> GetReportsByOrphanAsync(Guid orphanId);
    Task<PeriodicReportDto> CreateReportAsync(CreatePeriodicReportDto dto);
    Task<PeriodicReportDto> ApproveReportAsync(Guid id);
    Task<PeriodicReportDto> RejectReportAsync(Guid id, RejectReportDto dto);
    Task<byte[]> ExportToExcelAsync(ReportExportFilterDto filter);
}

// IIROSA.Application/Services/PeriodicReportService.cs
public class PeriodicReportService : Service<PeriodicReportDto, PeriodicReport, IPeriodicReportRepository>,
    IPeriodicReportService
{
    private readonly IPeriodicReportRepository _reportRepository;
    private readonly INotificationService _notificationService;
    private readonly IReportGenerationService _reportService;

    public PeriodicReportService(
        IPeriodicReportRepository repository,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<PeriodicReportService> logger,
        INotificationService notificationService,
        IReportGenerationService reportService)
        : base(repository, unitOfWork, mapper, logger)
    {
        _reportRepository = repository;
        _notificationService = notificationService;
        _reportService = reportService;
    }

    public async Task<IEnumerable<PeriodicReportDto>> GetPendingReportsAsync()
    {
        var reports = await _reportRepository.GetPendingReportsAsync();
        return _mapper.Map<IEnumerable<PeriodicReportDto>>(reports);
    }

    public async Task<IEnumerable<PeriodicReportDto>> GetApprovedReportsAsync()
    {
        var reports = await _reportRepository.GetApprovedReportsAsync();
        return _mapper.Map<IEnumerable<PeriodicReportDto>>(reports);
    }

    public async Task<IEnumerable<PeriodicReportDto>> GetRejectedReportsAsync()
    {
        var reports = await _reportRepository.GetRejectedReportsAsync();
        return _mapper.Map<IEnumerable<PeriodicReportDto>>(reports);
    }

    public async Task<IEnumerable<PeriodicReportDto>> GetReportsByOrphanAsync(Guid orphanId)
    {
        var reports = await _reportRepository.GetReportsByOrphanAsync(orphanId);
        return _mapper.Map<IEnumerable<PeriodicReportDto>>(reports);
    }

    public async Task<PeriodicReportDto> CreateReportAsync(CreatePeriodicReportDto dto)
    {
        // Check if Periodic Reports feature is enabled for charity
        var isEnabled = await _reportRepository.IsPeriodicReportsEnabledAsync(dto.CharityId);
        if (!isEnabled)
        {
            throw new BusinessException("Periodic Reports feature is not enabled for your charity");
        }

        return await CreateAsync(dto);
    }

    public async Task<PeriodicReportDto> ApproveReportAsync(Guid id)
    {
        var report = await _reportRepository.GetReportWithDetailsAsync(id);
        if (report == null)
            throw new NotFoundException(nameof(PeriodicReport), id);

        // Update report
        report.Reviewed = true;
        report.ReviewedDate = DateTime.UtcNow;
        report.FK_Reviewer = GetCurrentUserId();
        report.IsAccepted = true;
        report.IsRefused = false;

        _reportRepository.Update(report);
        await _unitOfWork.SaveChangesAsync();

        // Notify charity
        await _notificationService.SendNotification(
            userId: report.Child.Charity.UserId,
            type: NotificationType.Success,
            title: "Periodic Report Approved",
            message: $"The periodic report for {report.Child.FullName} has been approved",
            category: "Approval",
            actionUrl: $"/periodic-reports/{report.Id}",
            entityType: "PeriodicReport",
            entityId: report.Id
        );

        _logger.LogInformation("Periodic report {ReportId} approved", id);

        return _mapper.Map<PeriodicReportDto>(report);
    }

    public async Task<PeriodicReportDto> RejectReportAsync(Guid id, RejectReportDto dto)
    {
        var report = await _reportRepository.GetReportWithDetailsAsync(id);
        if (report == null)
            throw new NotFoundException(nameof(PeriodicReport), id);

        // Update report
        report.Reviewed = true;
        report.ReviewedDate = DateTime.UtcNow;
        report.FK_Reviewer = GetCurrentUserId();
        report.IsRefused = true;
        report.IsAccepted = false;
        report.RefuseReason = dto.Reason;
        report.RefuseReasonId = dto.RefuseReasonId;

        _reportRepository.Update(report);
        await _unitOfWork.SaveChangesAsync();

        // Notify charity
        await _notificationService.SendNotification(
            userId: report.Child.Charity.UserId,
            type: NotificationType.Error,
            title: "Periodic Report Rejected",
            message: $"The periodic report for {report.Child.FullName} requires changes. Reason: {dto.Reason}",
            category: "Approval",
            actionUrl: $"/periodic-reports/{report.Id}/edit",
            entityType: "PeriodicReport",
            entityId: report.Id,
            priority: NotificationPriority.High
        );

        _logger.LogInformation("Periodic report {ReportId} rejected", id);

        return _mapper.Map<PeriodicReportDto>(report);
    }

    public async Task<byte[]> ExportToExcelAsync(ReportExportFilterDto filter)
    {
        return await _reportService.GeneratePeriodicReportsExcelAsync(filter);
    }

    protected override async Task BeforeCreateAsync(PeriodicReport entity, PeriodicReportDto dto)
    {
        // Set user who created the report
        entity.FK_User = GetCurrentUserId();
        entity.TimeStamp = DateTime.UtcNow;
        entity.ReportDate = DateTime.UtcNow;
        entity.Active = true;
        entity.Reviewed = false;

        await Task.CompletedTask;
    }
}
```

### **4. Controller**

```csharp
// IIROSA.Web/Controllers/Api/V1/PeriodicReportsController.cs
[ApiVersion("1.0")]
public class PeriodicReportsController : BaseController<PeriodicReportDto, PeriodicReport>
{
    private readonly IPeriodicReportService _reportService;

    public PeriodicReportsController(IPeriodicReportService reportService, ILogger<PeriodicReportsController> logger)
        : base(reportService, logger)
    {
        _reportService = reportService;
    }

    [HttpGet("pending")]
    public async Task<ActionResult<ApiResponse<IEnumerable<PeriodicReportDto>>>> GetPendingReports()
    {
        try
        {
            var reports = await _reportService.GetPendingReportsAsync();
            return Ok(ApiResponse<IEnumerable<PeriodicReportDto>>.Success(reports));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving pending reports");
            return StatusCode(StatusCodes.Status500InternalServerError,
                ApiResponse<IEnumerable<PeriodicReportDto>>.Error("An error occurred"));
        }
    }

    [HttpGet("approved")]
    public async Task<ActionResult<ApiResponse<IEnumerable<PeriodicReportDto>>>> GetApprovedReports()
    {
        try
        {
            var reports = await _reportService.GetApprovedReportsAsync();
            return Ok(ApiResponse<IEnumerable<PeriodicReportDto>>.Success(reports));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving approved reports");
            return StatusCode(StatusCodes.Status500InternalServerError,
                ApiResponse<IEnumerable<PeriodicReportDto>>.Error("An error occurred"));
        }
    }

    [HttpGet("rejected")]
    public async Task<ActionResult<ApiResponse<IEnumerable<PeriodicReportDto>>>> GetRejectedReports()
    {
        try
        {
            var reports = await _reportService.GetRejectedReportsAsync();
            return Ok(ApiResponse<IEnumerable<PeriodicReportDto>>.Success(reports));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving rejected reports");
            return StatusCode(StatusCodes.Status500InternalServerError,
                ApiResponse<IEnumerable<PeriodicReportDto>>.Error("An error occurred"));
        }
    }

    [HttpGet("orphan/{orphanId}")]
    public async Task<ActionResult<ApiResponse<IEnumerable<PeriodicReportDto>>>> GetReportsByOrphan(Guid orphanId)
    {
        try
        {
            var reports = await _reportService.GetReportsByOrphanAsync(orphanId);
            return Ok(ApiResponse<IEnumerable<PeriodicReportDto>>.Success(reports));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving reports for orphan {OrphanId}", orphanId);
            return StatusCode(StatusCodes.Status500InternalServerError,
                ApiResponse<IEnumerable<PeriodicReportDto>>.Error("An error occurred"));
        }
    }

    [HttpPost("{id}/approve")]
    public async Task<ActionResult<ApiResponse<PeriodicReportDto>>> ApproveReport(Guid id)
    {
        try
        {
            var result = await _reportService.ApproveReportAsync(id);
            return Ok(ApiResponse<PeriodicReportDto>.Success(result, "Report approved successfully"));
        }
        catch (NotFoundException ex)
        {
            return NotFound(ApiResponse<PeriodicReportDto>.NotFound(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error approving report {ReportId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError,
                ApiResponse<PeriodicReportDto>.Error("An error occurred"));
        }
    }

    [HttpPost("{id}/reject")]
    public async Task<ActionResult<ApiResponse<PeriodicReportDto>>> RejectReport(Guid id, [FromBody] RejectReportDto dto)
    {
        try
        {
            var result = await _reportService.RejectReportAsync(id, dto);
            return Ok(ApiResponse<PeriodicReportDto>.Success(result, "Report rejected successfully"));
        }
        catch (NotFoundException ex)
        {
            return NotFound(ApiResponse<PeriodicReportDto>.NotFound(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rejecting report {ReportId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError,
                ApiResponse<PeriodicReportDto>.Error("An error occurred"));
        }
    }

    [HttpPost("export")]
    public async Task<IActionResult> ExportToExcel([FromBody] ReportExportFilterDto filter)
    {
        try
        {
            var excelBytes = await _reportService.ExportToExcelAsync(filter);
            return File(excelBytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"PeriodicReports_{DateTime.UtcNow:yyyyMMddHHmmss}.xlsx");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting reports to Excel");
            return StatusCode(StatusCodes.Status500InternalServerError,
                "An error occurred while exporting data");
        }
    }
}
```

---

## API Response Model

```csharp
// IIROSA.Web/Models/ApiResponse.cs
namespace IIROSA.Web.Models;

public class ApiResponse
{
    public bool Success { get; set; }
    public string Message { get; set; }
    public List<string> Errors { get; set; } = new();
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    public static ApiResponse Ok(string message = "Operation successful")
    {
        return new ApiResponse { Success = true, Message = message };
    }

    public static ApiResponse Error(string message, List<string>? errors = null)
    {
        return new ApiResponse
        {
            Success = false,
            Message = message,
            Errors = errors ?? new List<string>()
        };
    }

    public static ApiResponse NotFound(string message = "Resource not found")
    {
        return new ApiResponse { Success = false, Message = message };
    }
}

public class ApiResponse<T> : ApiResponse
{
    public T? Data { get; set; }
    public int? TotalCount { get; set; }
    public int? PageNumber { get; set; }
    public int? PageSize { get; set; }

    public static ApiResponse<T> Success(T data, string message = "Operation successful")
    {
        return new ApiResponse<T>
        {
            Success = true,
            Message = message,
            Data = data
        };
    }

    public static ApiResponse<T> Success(T data, int totalCount, int pageNumber, int pageSize)
    {
        return new ApiResponse<T>
        {
            Success = true,
            Message = "Operation successful",
            Data = data,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }

    public static new ApiResponse<T> NotFound(string message = "Resource not found")
    {
        return new ApiResponse<T>
        {
            Success = false,
            Message = message
        };
    }

    public static new ApiResponse<T> Error(string message, List<string>? errors = null)
    {
        return new ApiResponse<T>
        {
            Success = false,
            Message = message,
            Errors = errors ?? new List<string>()
        };
    }

    public static ApiResponse<T> ValidationError(Dictionary<string, string[]> errors)
    {
        var errorList = errors.SelectMany(kvp =>
            kvp.Value.Select(v => $"{kvp.Key}: {v}")).ToList();

        return new ApiResponse<T>
        {
            Success = false,
            Message = "Validation failed",
            Errors = errorList
        };
    }
}
```

---

## Custom Exceptions

```csharp
// IIROSA.Application/Exceptions/NotFoundException.cs
namespace IIROSA.Application.Exceptions;

public class NotFoundException : Exception
{
    public string EntityType { get; }
    public object EntityId { get; }

    public NotFoundException(string entityType, object entityId)
        : base($"{entityType} with ID {EntityId} was not found")
    {
        EntityType = entityType;
        EntityId = entityId;
    }
}

// IIROSA.Application/Exceptions/ValidationException.cs
public class ValidationException : Exception
{
    public List<string> Errors { get; }

    public ValidationException(List<string> errors)
        : base("Validation failed")
    {
        Errors = errors;
    }
}

// IIROSA.Application/Exceptions/BusinessException.cs
public class BusinessException : Exception
{
    public BusinessException(string message) : base(message)
    {
    }
}
```

---

## Configuration Files

### **appsettings.json**

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=.;Database=IIROSA.System;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
  },
  "Serilog": {
    "MinimumLevel": "Information",
    "WriteTo": [
      {
        "Name": "Console"
      },
      {
        "Name": "File",
        "Args": {
          "path": "Logs/log-.txt",
          "rollingInterval": "Day"
        }
      }
    ]
  },
  "SignalR": {
    "EnableDetailedErrors": true,
    "KeepAliveInterval": 15
  },
  "Cache": {
    "DefaultExpirationMinutes": 30
  },
  "Email": {
    "From": "noreply@iirosa.org",
    "SmtpServer": "smtp.example.com",
    "Port": 587,
    "UseSsl": true
  },
  "Storage": {
    "Provider": "Local", // or "Azure", "AWS"
    "Path": "wwwroot/uploads"
  },
  "AllowedHosts": "*"
}
```

---

## Benefits of This Architecture

### **1. Separation of Concerns**
- Each layer has a specific responsibility
- Easy to maintain and test
- Changes in one layer don't affect others

### **2. Testability**
- Unit tests can mock repositories
- Service tests can use in-memory repositories
- Controller tests can mock services

### **3. Scalability**
- Easy to add new features/modules
- Services can be reused across different controllers
- Dynamic DI reduces boilerplate code

### **4. Flexibility**
- Easy to swap implementations (e.g., different caching providers)
- Can add cross-cutting concerns without modifying core logic
- Supports multiple data sources

### **5. Maintainability**
- Clear structure makes it easy to find code
- Consistent patterns across all modules
- Easy to onboard new developers

This architecture follows SOLID principles and Clean Architecture best practices, providing a robust foundation for your Orphan Management System.
