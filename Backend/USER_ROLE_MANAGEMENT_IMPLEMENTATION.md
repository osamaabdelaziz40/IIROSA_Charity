# UserRoleManagement Use Case Implementation

## ✅ Architecture - Proper Layer Separation

### Layer 1: Controllers (Presentation Layer) - THIN
**File:** `src/IIROSA.Api/Controllers/UserManagementController.cs`

- Controllers are **THIN** - they only call Application services
- No business logic in controllers
- Handle HTTP concerns only (requests, responses, status codes)
- Return Framework's `ApiResponse<T>` for consistency

**Example:**
```csharp
[HttpPost("users")]
public async Task<IActionResult> CreateUser([FromBody] CreateUserDto dto)
{
    if (!ModelState.IsValid)
        return BadRequest(ModelState);

    var result = await _userManagementService.CreateUserAsync(dto);

    if (!result.Success)
        return BadRequest(result);

    return Ok(result);
}
```

### Layer 2: Application Services (Business Logic Layer)
**Files:**
- `src/IIROSA.Application/Services/UserManagement/IUserManagementService.cs`
- `src/IIROSA.Application/Services/UserManagement/UserManagementService.cs`

- Contains **ALL** business logic
- Handles DTO mapping between Framework and Application layers
- Calls Framework services (UserAppService, RoleAppService)
- Implements use case logic (validation, workflows, etc.)
- Uses Framework's `ApiResponse<T>` for responses

**Example:**
```csharp
public async Task<ApiResponse<UserDetailDto>> CreateUserAsync(CreateUserDto dto)
{
    // Business logic
    var userRegister = new UserRegister
    {
        Email = dto.Email,
        Password = dto.Password ?? "P@ssw0rd@2022", // Default password logic
        RoleNames = dto.Roles?.ToArray() ?? Array.Empty<string>()
    };

    // Call Framework service
    var result = await _userAppService.Register(userRegister);

    // Map response
    if (!result.Success)
        return new ApiResponse<UserDetailDto> { Success = false, Message = result.Message };

    var createdUser = await GetUserByIdAsync(result.Value.Id.Value);

    return new ApiResponse<UserDetailDto> { Success = true, Value = createdUser };
}
```

### Layer 3: Framework Services (Data Access Layer)
- Framework.Identity provides: `UserAppService`, `RoleAppService`, `UserRoleAppService`
- Framework.Core provides: `AppSettingsService`, `NotificationsManager`, etc.
- We **USE** these services directly from Application layer
- No wrappers needed!

## ✅ Implemented Use Cases

| UC # | Use Case | Status | Endpoint |
|------|----------|--------|----------|
| UC-1.1 | Create User | ✅ Complete | `POST /api/usermanagement/users` |
| UC-1.2 | Update User | ✅ Complete | `PUT /api/usermanagement/users/{id}` |
| UC-1.3 | Deactivate User | ✅ Complete | `POST /api/usermanagement/users/{id}/deactivate` |
| UC-1.4 | Reset Password | ✅ Complete | `POST /api/usermanagement/users/{id}/reset-password` |
| UC-1.5 | Assign Roles | ✅ Complete | `POST /api/usermanagement/users/{id}/roles` |
| UC-1.6 | Create Role | ⏳ TODO | `POST /api/usermanagement/roles` |
| UC-1.7 | Update Role Permissions | ⏳ TODO | `PUT /api/usermanagement/roles/{id}` |
| UC-1.8 | View All Users | ✅ Complete | `GET /api/usermanagement/users` |
| UC-1.9 | View User Activity | ⏳ TODO | `GET /api/usermanagement/users/{id}/activity` |
| UC-1.10 | Manage User Claims | ⏳ TODO | `GET/POST/DELETE /api/usermanagement/users/{id}/claims` |

## ✅ API Endpoints

### User CRUD
```
GET    /api/usermanagement/users              - List users with pagination (UC-1.8)
GET    /api/usermanagement/users/{id}         - Get user by ID
POST   /api/usermanagement/users              - Create user (UC-1.1)
PUT    /api/usermanagement/users/{id}         - Update user (UC-1.2)
POST   /api/usermanagement/users/{id}/deactivate - Deactivate user (UC-1.3)
POST   /api/usermanagement/users/{id}/activate   - Activate user
DELETE /api/usermanagement/users/{id}         - Delete user
POST   /api/usermanagement/users/{id}/reset-password - Reset password (UC-1.4)
POST   /api/usermanagement/users/export       - Export to Excel (UC-1.8)
```

### Role Assignment
```
GET    /api/usermanagement/users/{id}/roles  - Get user roles (UC-1.5)
POST   /api/usermanagement/users/{id}/roles  - Assign roles to user (UC-1.5)
```

### Claims Management
```
GET    /api/usermanagement/users/{id}/claims    - Get user claims (UC-1.10)
POST   /api/usermanagement/users/{id}/claims    - Add claim (UC-1.10)
DELETE /api/usermanagement/users/{id}/claims/{claimType} - Remove claim (UC-1.10)
```

### Role Management
```
GET    /api/usermanagement/roles             - List roles
GET    /api/usermanagement/roles/{id}        - Get role by ID
GET    /api/usermanagement/roles/all         - Get all roles (dropdown)
POST   /api/usermanagement/roles             - Create role (UC-1.6)
PUT    /api/usermanagement/roles/{id}        - Update role (UC-1.7)
DELETE /api/usermanagement/roles/{id}        - Delete role
```

### Activity Log
```
GET    /api/usermanagement/users/{id}/activity - Get user activity (UC-1.9)
```

## ✅ DTOs Structure

### Input DTOs (Controller → Application)
- `CreateUserDto` - Create user request
- `UpdateUserDto` - Update user request
- `UserSearchDto` - User search criteria
- `ResetPasswordDto` - Password reset request
- `CreateRoleDto` - Create role request
- `UpdateRoleDto` - Update role request
- `RoleSearchDto` - Role search criteria
- `ActivitySearchDto` - Activity log search
- `UserClaimDto` - User claim

### Output DTOs (Application → Controller)
- `UserListDto` - User list item
- `UserDetailDto` - User detail with all info
- `RoleListDto` - Role list item
- `RoleDetailDto` - Role detail with permissions
- `UserRoleDto` - User role assignment
- `UserActivityDto` - Activity log entry
- `PagedResult<T>` - Paginated result wrapper

### Framework DTOs (Used Internally)
- Framework.Identity: `UserDto`, `UserRegister`, `LoginDto`, `ChangePasswordDto`, `UserGridSearchDto`
- Framework.Core: `ApiResponse`, `ReturnResult`, `SettingsDto`

## ✅ Key Features Implemented

1. **Proper Layering:**
   - Controllers are thin (just HTTP handling)
   - Application services contain business logic
   - Framework services used for data access

2. **DTO Mapping:**
   - Framework DTOs mapped to Application DTOs
   - Clean separation between layers
   - No Framework DTOs exposed to API

3. **Pagination:**
   - `PagedResult<T>` wrapper
   - Uses Framework's pagination settings
   - Supports filtering, searching, sorting

4. **Excel Export:**
   - Export to CSV (can be upgraded to Excel with EPPlus)
   - Respects user's current filter/sort
   - Uses Framework's ExportNoOfItems setting

5. **Default Password:**
   - Auto-generates password: `P@ssw0rd@{Year}`
   - Can be overridden during creation

6. **Role Assignment:**
   - Multi-select role support
   - Updates user roles on user update

## ⏳ TODO Items

1. **Claims Management** - Need access to `UserManager<ApplicationUser>.AddClaimAsync()`
   - Add `UserManager<ApplicationUser>` injection to Application service
   - Implement AddClaimAsync, RemoveClaimAsync, GetUserClaimsAsync

2. **Role CRUD** - Need to explore `RoleAppService` methods
   - Implement CreateRoleAsync, UpdateRoleAsync, DeleteRoleAsync
   - Add permission management

3. **Activity Logging** - Need audit log implementation
   - Create ActivityLog entity
   - Log all user actions (Create, Update, Delete, Login, Logout)
   - Implement activity retrieval

4. **Email Notifications** - Integrate with Framework.Notifications
   - Send welcome email on user creation
   - Send password reset email
   - Use Framework's NotificationsManager

5. **Excel Export Enhancement** - Use EPPlus for real Excel files
   - Install EPPlus package
   - Generate .xlsx files with formatting
   - Add Arabic/English headers based on culture

## ✅ Next Steps

To complete the implementation:

1. **Add to DI Container** (already done in `ServiceCollectionExtensions.cs`):
```csharp
services.AddScoped<IUserManagementService, UserManagementService>();
```

2. **Test the API** using Swagger:
   - Navigate to `/swagger`
   - Authorize with JWT token
   - Test each endpoint

3. **Implement TODO items** listed above

4. **Create Angular frontend** components for:
   - User list with search/filter/pagination
   - User create/edit forms
   - Role assignment interface
   - Role management interface

---

**This implementation follows proper clean architecture principles with clear separation of concerns!** 🎉
