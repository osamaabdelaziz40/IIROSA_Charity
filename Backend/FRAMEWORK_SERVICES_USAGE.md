# Framework Services Integration Guide

## ✅ Correct Approach - Using Framework Services Directly

### 1. User Management (Framework.Identity)

**Service:** `UserAppService` from `Framework.Identity.Data.Services`

```csharp
public class AuthController : BaseController
{
    private readonly UserAppService _userAppService;
    private readonly ITokenService _tokenService; // Custom service for JWT generation

    public AuthController(UserAppService userAppService, ITokenService tokenService)
    {
        _userAppService = userAppService;
        _tokenService = tokenService;
    }

    [HttpPost("login")]
    public async Task<ActionResult<ApiResponse>> Login(LoginDto loginDto)
    {
        var result = await _userAppService.Login(loginDto);
        if (result.Success)
        {
            var token = await _tokenService.GenerateTokenAsync(result.Value);
            var objRes = new LoginResponseDto { Token = token.Token };
            return Ok(new ApiResponse<LoginResponseDto> { Value = objRes, Success = true });
        }
        return Ok(result);
    }

    [HttpPost("register")]
    public async Task<ActionResult<ApiResponse>> Register(UserRegister model)
    {
        var result = await _userAppService.Register(model);
        return Ok(result);
    }
}
```

### 2. Attachments (Framework.Core)

**Service:** `AttachmentService` from `Framework.Core.SharedServices.Services`

```csharp
public class AttachmentController : ControllerBase
{
    private readonly AttachmentService _attachment;

    public AttachmentController(AttachmentService attachment)
    {
        _attachment = attachment;
    }

    [HttpPost("CreateAttachment")]
    public IActionResult CreateAttachment(IFormFile file)
    {
        var result = _attachment.AddAttachment(file, file.FileName, file.ContentType);
        return Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAttachment(Guid id)
    {
        await _attachment.DeleteAttachmentFromDbAndFileSystem(id);
        return Ok(true);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetAttachment(Guid id)
    {
        var attach = await _attachment.GetAttachment(id, true);
        return Ok(attach);
    }

    [HttpPost("Download")]
    public async Task<IActionResult> Download(Guid id)
    {
        var result = await _attachment.GetAttachmentForDownload(id);
        return Ok(result);
    }
}
```

### 3. Notifications (Framework.Core)

**Service:** `NotificationsManager` from `Framework.Core.Notifications`

```csharp
public class NotificationsController : ControllerBase
{
    private readonly NotificationsManager _notificationsManager;
    private readonly AppSettingsService _appSettingsService;

    public NotificationsController(
        NotificationsManager notificationsManager,
        AppSettingsService appSettingsService)
    {
        _notificationsManager = notificationsManager;
        _appSettingsService = appSettingsService;
    }

    [HttpPost("send-email")]
    public async Task<IActionResult> SendEmail([FromBody] EmailNotificationRequest request)
    {
        var message = new EmailMessage
        {
            To = request.To,
            TemplateName = request.TemplateName,
            TemplateData = request.TemplateData ?? new Dictionary<string, string>()
        };

        await _notificationsManager.EnqueueEmailAsync(message);
        return Ok(new { message = "Email queued successfully" });
    }

    [HttpGet("settings")]
    public IActionResult GetNotificationSettings()
    {
        var settings = _notificationsManager.LoadNotificationsSettings();
        return Ok(settings);
    }
}
```

### 4. Settings (Framework.Core)

**Service:** `AppSettingsService` from `Framework.Core.SharedServices.Services`

```csharp
public class SettingsController : ControllerBase
{
    private readonly AppSettingsService _appSettingsService;

    public SettingsController(AppSettingsService appSettingsService)
    {
        _appSettingsService = appSettingsService;
    }

    [HttpGet]
    public IActionResult GetAll([FromQuery] string? groupName)
    {
        var settings = string.IsNullOrEmpty(groupName)
            ? _appSettingsService.GetAllSettingsCached().SelectMany(x => x.Value).ToList()
            : _appSettingsService.GetSettingsByGroup(groupName);

        return Ok(settings);
    }

    [HttpGet("{key}")]
    public IActionResult GetByKey(string key)
    {
        var setting = _appSettingsService.GetSetting(key);
        return Ok(setting);
    }

    [HttpPost]
    public IActionResult Set([FromBody] SettingsDto setting)
    {
        _appSettingsService.UpdateSetting(setting);
        return Ok(new { message = "Setting updated successfully" });
    }
}
```

## ✅ Dependency Injection Configuration

### Program.cs or Startup.cs

```csharp
// Infrastructure (DbContext, Identity, Framework services)
builder.Services.AddInfrastructure(builder.Configuration);

// Application services (Token generation, etc.)
builder.Services.AddApplicationServices();
```

### Infrastructure Extensions

```csharp
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHttpContextAccessor();

        // Database
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        // Identity
        services.AddIdentity<ApplicationUser, ApplicationRole>(options => { ... })
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

        // Framework Core Services
        services.AddScoped(typeof(IRepositoryBase<,>), typeof(RepositoryBase<,>));
        services.AddScoped<INotificationsManager, NotificationsManager>();
        services.AddScoped<AppSettingsService>();
        services.AddScoped<AttachmentService>();
        services.AddScoped<NotificationTemplateService>();
        services.AddScoped<NotificationLogAppService>();
        services.AddScoped<ICacheManager, MemoryCacheManager>();

        // Framework Identity Services
        services.AddScoped<UserAppService>();
        services.AddScoped<RoleAppService>();
        services.AddScoped<UserRoleAppService>();

        return services;
    }

    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // Custom services (JWT token generation, etc.)
        services.AddScoped<ITokenService, TokenService>();
        return services;
    }
}
```

## ✅ DbContext Implementation

### ApplicationDbContext implements ICommonsDbContext

```csharp
public class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, string>, ICommonsDbContext
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options,
        IHttpContextAccessor httpContextAccessor)
        : base(options)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    // IBaseDbContext implementation
    public string CurrentUserName
    {
        get => _httpContextAccessor.HttpContext?.User?.Identity?.Name ?? "System";
        set { }
    }

    public Task<int> SaveChangesWithAuditAsync(CancellationToken cancellationToken = default)
    {
        return SaveChangesAsync(cancellationToken);
    }

    // Framework DbSets
    public DbSet<NotificationTemplate> NotificationTemplates { get; set; }
    public DbSet<SystemSetting> SystemSettings { get; set; }
    public DbSet<NotificationType> NotificationTypes { get; set; }
}
```

## ✅ Framework Services Available

### Framework.Identity.Data.Services
- `UserAppService` - User management, login, register
- `RoleAppService` - Role management
- `UserRoleAppService` - User-role assignments

### Framework.Core.SharedServices.Services
- `AttachmentService` - File upload, download, delete
- `AppSettingsService` - System settings management
- `NotificationTemplateService` - Notification templates
- `NotificationLogAppService` - Notification logging

### Framework.Core.Notifications
- `NotificationsManager` - Send email, SMS, mobile notifications

### Framework.Core.Data.Repositories
- `IRepositoryBase<ICommonsDbContext, TEntity>` - Generic repository
- `RepositoryBase<ICommonsDbContext, TEntity>` - Implementation

## ✅ Key Points

1. **Use Framework services directly** - Don't create wrapper services
2. **Implement ICommonsDbContext** - ApplicationDbContext must implement this interface
3. **Register IHttpContextAccessor** - Required by Framework services for CurrentUserName
4. **Use Framework DTOs** - LoginDto, UserRegister, ApiResponse, etc. from Framework
5. **Custom services only when needed** - Only create services for business logic not in Framework

## ✅ What NOT to Do

❌ Don't create custom `ISettingsService`, `INotificationService`, `IAttachmentService` wrappers
❌ Don't use `UserManager`, `SignInManager` directly - use `UserAppService` instead
❌ Don't create custom repositories - use Framework's `IRepositoryBase<,>`
❌ Don't create duplicate entity classes - use Framework entities directly
