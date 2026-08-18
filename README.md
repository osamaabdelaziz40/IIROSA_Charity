# IIROSA - Orphan Management System

A comprehensive full-stack web application for managing orphan sponsorship, charities, families, and periodic reports.

## Architecture

### Backend (.NET 8.0)
- **Clean Architecture** with Domain, Application, Infrastructure, and Presentation layers
- **Framework.Core** integration for shared entities (FullAuditedEntityBase, LookupEntityBase)
- **Framework.Identity** integration for authentication and authorization
- **Entity Framework Core** with auto-discovery and soft delete patterns
- **Repository & Unit of Work** patterns
- **SignalR** for real-time notifications
- **JWT Authentication** with role-based authorization

### Frontend (Angular 18+)
- **Angular 18+** with TypeScript
- **TinyDash Dark RTL** template integration
- **Shared Components**: Attachment, Input Fields, Data List
- **SignalR Client** for real-time updates
- **Localization** (English, Arabic)
- **Reactive Forms** with custom validators
- **RTL Support** for Arabic language

## Solution Structure

```
IIROSA Claude/
├── Backend/
│   ├── IIROSA.Domain/              # Entities and interfaces
│   ├── IIROSA.Application/         # DTOs, Services, Profiles
│   ├── IIROSA.Infrastructure/      # DbContext, Repositories
│   └── IIROSA.Presentation/        # API Controllers, Hubs
│
├── Frontend/
│   └── src/
│       ├── app/
│       │   ├── core/              # Services, Guards, Interceptors
│       │   ├── shared/            # Shared Components
│       │   ├── layouts/           # Main & Auth Layouts
│       │   └── modules/           # Feature Modules
│       └── assets/
│           ├── i18n/              # Localization Files
│           └── css/               # TinyDash Styles
│
└── Architecture/                   # Design Documents
```

## Features Implemented

### Backend
- ✅ Domain entities (Charity, Employee, Orphan, Family, Sponsor, PeriodicOrphanReport)
- ✅ Framework.Core & Framework.Identity integration
- ✅ DbContext with auto-discovery and soft delete
- ✅ Repository & Unit of Work patterns
- ✅ Service layer with AutoMapper
- ✅ API Controllers with JWT auth
- ✅ SignalR Hub for notifications
- ✅ Dynamic Dependency Injection for services

### Frontend
- ✅ Angular 18+ project structure
- ✅ Main layout with TinyDash Dark RTL
- ✅ Authentication module (Login)
- ✅ Core services (Auth, HTTP, SignalR)
- ✅ Guards and Interceptors
- ✅ Localization (en.json, ar.json)
- ✅ Shared Attachment Component
- ✅ Employees List with pagination
- ✅ Dashboard module

## Getting Started

### Backend Setup

1. **Restore NuGet packages:**
   ```bash
   cd Backend
   dotnet restore
   ```

2. **Update connection string in `appsettings.json`:**
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=YOUR_SERVER;Database=IIROSADb;..."
     }
   }
   ```

3. **Run migrations (when available):**
   ```bash
   dotnet ef database update
   ```

4. **Build and run:**
   ```bash
   cd IIROSA.Presentation
   dotnet run
   ```

   API will be available at: `https://localhost:5001`

### Frontend Setup

1. **Install dependencies:**
   ```bash
   cd Frontend
   npm install
   ```

2. **Start development server:**
   ```bash
   ng serve
   ```

   Application will be available at: `http://localhost:4200`

3. **Build for production:**
   ```bash
   ng build --configuration production
   ```

## Framework Integration

### Framework.Core
- `FullAuditedEntityBase<TKey>` - Base entity with audit fields
- `LookupEntityBase<TKey>` - Base lookup entity
- `INotificationService` - Notification management
- `ISettingService` - Application settings
- `IAttachmentService` - File attachments

### Framework.Identity
- `ApplicationUser` - User entity
- `ApplicationRole` - Role entity
- `IUserManagementService` - User management
- `IRoleService` - Role management
- `IPermissionService` - Permission management

## API Endpoints

### Authentication
- `POST /api/auth/login` - User login
- `POST /api/auth/logout` - User logout
- `POST /api/auth/refresh` - Refresh token

### Charities
- `GET /api/charities` - Get all charities
- `GET /api/charities/{id}` - Get charity by ID
- `POST /api/charities` - Create charity
- `PUT /api/charities/{id}` - Update charity
- `DELETE /api/charities/{id}` - Delete charity
- `POST /api/charities/paged` - Get paged charities

### Employees
- `GET /api/employees` - Get all employees
- `GET /api/employees/{id}` - Get employee by ID
- `POST /api/employees` - Create employee
- `PUT /api/employees/{id}` - Update employee
- `DELETE /api/employees/{id}` - Delete employee
- `POST /api/employees/paged` - Get paged employees

### SignalR
- `/hubs/notifications` - Real-time notifications

## Default Credentials

After setting up the database, create a default user:
- Username: `admin`
- Password: `Admin@123`
- Role: `SuperAdmin`

## Development Roadmap

### Phase 1: Foundation ✅
- Backend structure
- Frontend structure
- Authentication
- Basic modules

### Phase 2: Core Features (Next)
- Complete CRUD operations
- File upload/download
- Advanced filtering & search
- Excel export

### Phase 3: Advanced Features
- Periodic Orphan Reports
- Dashboard analytics
- Real-time notifications
- Permission management

## Technologies

### Backend
- .NET 8.0
- Entity Framework Core 8.0
- SQL Server
- SignalR
- JWT Authentication
- AutoMapper
- FluentValidation

### Frontend
- Angular 18+
- TypeScript 5.x
- Bootstrap 5.3
- SignalR Client
- ngx-translate
- ExcelJS
- TinyDash Template

## Contributing

1. Follow the existing code structure
2. Use proper naming conventions
3. Add XML comments for public APIs
4. Write unit tests for business logic
5. Follow SOLID principles

## License

Copyright © 2026 IIROSA. All rights reserved.
