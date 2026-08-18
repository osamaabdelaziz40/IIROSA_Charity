# IIROSA Backend

.NET 8.0 Clean Architecture backend for IIROSA Orphan Management System.

## Structure

```
IIROSA.Domain/
├── Entities/              # Domain entities
│   ├── Base/             # Base classes (FullAuditedEntity)
│   ├── Lookups/          # Lookup entities (Country, City, etc.)
│   └── [Entities].cs     # Main entities (Charity, Employee, etc.)
└── Interfaces/           # Domain interfaces (IRepository, IService)

IIROSA.Application/
├── DTOs/                 # Data Transfer Objects
├── Services/             # Business logic services
├── Profiles/             # AutoMapper profiles
└── Validators/           # FluentValidation validators

IIROSA.Infrastructure/
├── Data/                 # DbContext & configuration
├── Repositories/         # Repository implementations
└── Configurations/       # EF Core entity configurations

IIROSA.Presentation/
├── Controllers/          # API controllers
├── Hubs/                 # SignalR hubs
└── Middleware/           # Custom middleware

```

## Key Features

- **Clean Architecture** - Separation of concerns
- **Repository Pattern** - Generic repository with Unit of Work
- **Service Layer** - Business logic separation
- **Auto-Discovery** - Entities auto-discovered by DbContext
- **Soft Delete** - Global query filters for IsDeleted
- **JWT Authentication** - Token-based auth with refresh tokens
- **SignalR** - Real-time notifications
- **Dynamic DI** - Services auto-registered via reflection

## Framework Integration

### Framework.Core
```csharp
// All main entities inherit from FullAuditedEntityBase<Guid>
public class Charity : FullAuditedEntityBase<Guid>
{
    public string Code { get; set; }
    public string Name { get; set; }
    // No audit fields needed - inherited!
}

// Lookup entities inherit from LookupEntityBase<int>
public class Country : LookupEntityBase<int>
{
    public string? IsoCode { get; set; }
    // Id, Name, IsActive inherited
}
```

### Framework.Identity
```csharp
// User management from Framework.Identity
private readonly IUserManagementService _userService;

// Authentication handled by Framework.Identity
builder.Services.AddFrameworkIdentity(builder.Configuration);
```

## Running the Application

1. **Restore packages:**
   ```bash
   dotnet restore
   ```

2. **Set up database:**
   ```bash
   dotnet ef database update
   ```

3. **Run:**
   ```bash
   cd IIROSA.Presentation
   dotnet run
   ```

API: `https://localhost:5001`
Swagger: `https://localhost:5001/swagger`

## Adding New Modules

1. Create entity in `IIROSA.Domain/Entities/`
2. Create DTOs in `IIROSA.Application/DTOs/`
3. Create service in `IIROSA.Application/Services/`
4. Create controller in `IIROSA.Presentation/Controllers/`
5. Service will be auto-registered via reflection

## Configuration

### appsettings.json
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=..."
  },
  "Jwt": {
    "Issuer": "https://localhost:5001",
    "Audience": "https://localhost:5001",
    "Secret": "YourSecretKeyHere",
    "ExpiryInMinutes": 60
  }
}
```
