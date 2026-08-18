# 01 - User Role Management Use Case

### 4.1 Module: User & Role Management

**Module Owner:** Super Admin  
**Purpose:** Manage system users, roles, and access control  
**Dependencies:** Framework.Identity  

---

#### Use Case UC-1.1: Seed Roles and Users (System Initialization)

| Field | Value |
|-------|-------|
| **ID** | UC-1.1 |
| **Name** | Seed Roles and Users |
| **Actor** | System Administrator / Database Administrator |
| **Priority** | Critical |
| **Description** | Initialize system with default roles and seed users for initial system setup and deployment |

**Preconditions:**
- System is freshly installed or database is empty
- Database schema is deployed
- Application is configured and running
- Database connection is established

**Main Flow:**
1. System Administrator executes seed data script or runs initialization command
2. System checks if roles already exist to prevent duplicate seeding
3. System creates seed roles in the following order:

   **Seed Roles:**
   ```
   Role ID              | Role Name          | Description
   ---------------------|--------------------|--------------------------------------------------
   {GUID-1}            | Super Admin        | Full system access, can manage all users and roles
   {GUID-2}            | Admin              | Organization-wide management, cannot manage Super Admins
   {GUID-3}            | Charity            | Charity users managing orphans and families
   {GUID-4}            | Accountant         | Financial management, checks, and payments
   {GUID-5}            | FinancialOfficer   | Financial oversight and approval
   ```

4. For each role, system assigns default permissions:
   
   **Super Admin Permissions:**
   - Create, Read, Update, Delete users
   - Create, Read, Update, Delete roles
   - Manage system settings
   - Full access to all modules
   - View all audit logs
   
   **Admin Permissions:**
   - Create, Read, Update users (except Super Admins)
   - Read roles
   - Manage organizations and charities
   - Full access to most modules
   - View relevant audit logs
   
   **Charity Permissions:**
   - Create, Read, Update orphans (if charity rights enabled)
   - Create, Read, Update families (if charity rights enabled)
   - Submit periodic reports
   - View charity-specific data
   
   **Accountant Permissions:**
   - Create, Read, Update checks
   - Manage payments
   - View financial reports
   - Reconcile accounts
   
   **FinancialOfficer Permissions:**
   - Approve financial transactions
   - View all financial data
   - Generate financial reports
   - Oversee budget management

5. System creates seed users with the following specifications:

   **Seed Users (Password: P@ssw0rd@2022 for all):**
   ```
   Email                      | Name              | Assigned Role      | Additional Info
   --------------------------|-------------------|--------------------|------------------
   OsamaSuper@IIROSA.com     | Osama Abdelaziz   | Super Admin        | System owner
   Admin@IIROSA.com          | System Admin      | Admin              | Administrative user
   Charity@IIROSA.com        | Charity User      | Charity            | Charity operations
   Accountant@IIROSA.com     | Accountant User   | Accountant         | Financial operations
   FinancialOfficer@IIROSA.com| Financial Officer| FinancialOfficer   | Financial oversight
   ```

6. For each seed user, system:
   - Validates email uniqueness
   - Creates user account with:
     * Email (as username)
     * EmailConfirmed = true
     * IsActive = true
     * CreationDate = current timestamp
     * PasswordHash = hash of "P@ssw0rd@2022"
   - Assigns user to specific role (one-to-one mapping)
   - Adds default claims for user
   - Logs user creation in audit log with "SeedData" tag

7. System displays seeding results:
   - Total roles created
   - Total users created
   - List of created roles with IDs
   - List of created users with IDs
   - Any warnings or errors encountered

8. System generates seed data report for documentation

9. System stores seed data metadata in database:
   - SeedDate (timestamp)
   - SeededBy (system/admin user)
   - SeedVersion (version identifier)

**Alternative Flows:**
- **2a. Roles already exist:** System displays warning "Seed roles already exist. Skipping role creation." and continues with user creation
- **6a. User email already exists:** System displays warning "User [Email] already exists. Skipping user creation." and continues with remaining users
- **6b. Role does not exist when creating user:** System displays error "Required role [RoleName] not found. Cannot create user." and stops seeding process
- **8a. Critical error during seeding:** System displays error message, rolls back all changes made during current seeding operation, and logs error details

**Postconditions:**
- All seed roles exist in database with correct permissions
- All seed users exist in database with correct role assignments
- All seed users can authenticate with default password
- Audit log contains all seed data creation records
- System is ready for initial login
- Seed data metadata is stored for tracking

**Technical Implementation Notes:**

**SQL Script for Seeding:**
```sql
-- Seed Roles
IF NOT EXISTS (SELECT 1 FROM aspnet_Roles WHERE Name = 'Super Admin')
BEGIN
    INSERT INTO aspnet_Roles (Id, Name, NormalizedName, ConcurrencyStamp)
    VALUES (NEWID(), 'Super Admin', 'SUPER ADMIN', NEWID())
END

IF NOT EXISTS (SELECT 1 FROM aspnet_Roles WHERE Name = 'Admin')
BEGIN
    INSERT INTO aspnet_Roles (Id, Name, NormalizedName, ConcurrencyStamp)
    VALUES (NEWID(), 'Admin', 'ADMIN', NEWID())
END

IF NOT EXISTS (SELECT 1 FROM aspnet_Roles WHERE Name = 'Charity')
BEGIN
    INSERT INTO aspnet_Roles (Id, Name, NormalizedName, ConcurrencyStamp)
    VALUES (NEWID(), 'Charity', 'CHARITY', NEWID())
END

IF NOT EXISTS (SELECT 1 FROM aspnet_Roles WHERE Name = 'Accountant')
BEGIN
    INSERT INTO aspnet_Roles (Id, Name, NormalizedName, ConcurrencyStamp)
    VALUES (NEWID(), 'Accountant', 'ACCOUNTANT', NEWID())
END

IF NOT EXISTS (SELECT 1 FROM aspnet_Roles WHERE Name = 'FinancialOfficer')
BEGIN
    INSERT INTO aspnet_Roles (Id, Name, NormalizedName, ConcurrencyStamp)
    VALUES (NEWID(), 'FinancialOfficer', 'FINANCIALOFFICER', NEWID())
END

-- Seed Users
DECLARE @SuperAdminId UNIQUEIDENTIFIER = NEWID()
DECLARE @AdminId UNIQUEIDENTIFIER = NEWID()
DECLARE @CharityId UNIQUEIDENTIFIER = NEWID()
DECLARE @AccountantId UNIQUEIDENTIFIER = NEWID()
DECLARE @FinancialOfficerId UNIQUEIDENTIFIER = NEWID()

-- OsamaSuper@IIROSA.com (Super Admin)
IF NOT EXISTS (SELECT 1 FROM aspnet_Users WHERE Email = 'OsamaSuper@IIROSA.com')
BEGIN
    INSERT INTO aspnet_Users (Id, Email, EmailConfirmed, PhoneNumberConfirmed, TwoFactorEnabled, LockoutEnabled, AccessFailedCount, UserName, NormalizedUserName, IsActive, CreationDate)
    VALUES (@SuperAdminId, 'OsamaSuper@IIROSA.com', 1, 0, 0, 1, 0, 'OsamaSuper@IIROSA.com', 'OSAMASUPER@IIROSA.COM', 1, GETDATE())
    
    -- Assign to Super Admin role
    INSERT INTO aspnet_UserRoles (UserId, RoleId)
    SELECT @SuperAdminId, Id FROM aspnet_Roles WHERE Name = 'Super Admin'
END

-- Admin@IIROSA.com (Admin)
IF NOT EXISTS (SELECT 1 FROM aspnet_Users WHERE Email = 'Admin@IIROSA.com')
BEGIN
    INSERT INTO aspnet_Users (Id, Email, EmailConfirmed, PhoneNumberConfirmed, TwoFactorEnabled, LockoutEnabled, AccessFailedCount, UserName, NormalizedUserName, IsActive, CreationDate)
    VALUES (@AdminId, 'Admin@IIROSA.com', 1, 0, 0, 1, 0, 'Admin@IIROSA.com', 'ADMIN@IIROSA.COM', 1, GETDATE())
    
    -- Assign to Admin role
    INSERT INTO aspnet_UserRoles (UserId, RoleId)
    SELECT @AdminId, Id FROM aspnet_Roles WHERE Name = 'Admin'
END

-- Charity@IIROSA.com (Charity)
IF NOT EXISTS (SELECT 1 FROM aspnet_Users WHERE Email = 'Charity@IIROSA.com')
BEGIN
    INSERT INTO aspnet_Users (Id, Email, EmailConfirmed, PhoneNumberConfirmed, TwoFactorEnabled, LockoutEnabled, AccessFailedCount, UserName, NormalizedUserName, IsActive, CreationDate)
    VALUES (@CharityId, 'Charity@IIROSA.com', 1, 0, 0, 1, 0, 'Charity@IIROSA.com', 'CHARITY@IIROSA.COM', 1, GETDATE())
    
    -- Assign to Charity role
    INSERT INTO aspnet_UserRoles (UserId, RoleId)
    SELECT @CharityId, Id FROM aspnet_Roles WHERE Name = 'Charity'
END

-- Accountant@IIROSA.com (Accountant)
IF NOT EXISTS (SELECT 1 FROM aspnet_Users WHERE Email = 'Accountant@IIROSA.com')
BEGIN
    INSERT INTO aspnet_Users (Id, Email, EmailConfirmed, PhoneNumberConfirmed, TwoFactorEnabled, LockoutEnabled, AccessFailedCount, UserName, NormalizedUserName, IsActive, CreationDate)
    VALUES (@AccountantId, 'Accountant@IIROSA.com', 1, 0, 0, 1, 0, 'Accountant@IIROSA.com', 'ACCOUNTANT@IIROSA.COM', 1, GETDATE())
    
    -- Assign to Accountant role
    INSERT INTO aspnet_UserRoles (UserId, RoleId)
    SELECT @AccountantId, Id FROM aspnet_Roles WHERE Name = 'Accountant'
END

-- FinancialOfficer@IIROSA.com (FinancialOfficer)
IF NOT EXISTS (SELECT 1 FROM aspnet_Users WHERE Email = 'FinancialOfficer@IIROSA.com')
BEGIN
    INSERT INTO aspnet_Users (Id, Email, EmailConfirmed, PhoneNumberConfirmed, TwoFactorEnabled, LockoutEnabled, AccessFailedCount, UserName, NormalizedUserName, IsActive, CreationDate)
    VALUES (@FinancialOfficerId, 'FinancialOfficer@IIROSA.com', 1, 0, 0, 1, 0, 'FinancialOfficer@IIROSA.com', 'FINANCIALOFFICER@IIROSA.COM', 1, GETDATE())
    
    -- Assign to FinancialOfficer role
    INSERT INTO aspnet_UserRoles (UserId, RoleId)
    SELECT @FinancialOfficerId, Id FROM aspnet_Roles WHERE Name = 'FinancialOfficer'
END

-- Log seed data creation
INSERT INTO SeedDataLog (SeedDate, SeededBy, SeedVersion, Description)
VALUES (GETDATE(), 'SYSTEM', '1.0', 'Initial seed data: 5 roles and 5 users')
```

**C# Implementation for Seeding:**
```csharp
public static class SeedData
{
    public static async Task InitializeAsync(IServiceProvider serviceProvider)
    {
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var logger = serviceProvider.GetRequiredService<ILogger<Program>>();

        // Seed Roles
        var roles = new[]
        {
            new { Name = "Super Admin", Description = "Full system access" },
            new { Name = "Admin", Description = "Organization management" },
            new { Name = "Charity", Description = "Charity operations" },
            new { Name = "Accountant", Description = "Financial management" },
            new { Name = "FinancialOfficer", Description = "Financial oversight" }
        };

        foreach (var roleInfo in roles)
        {
            if (!await roleManager.RoleExistsAsync(roleInfo.Name))
            {
                var role = new IdentityRole(roleInfo.Name)
                {
                    NormalizedName = roleInfo.Name.ToUpper()
                };
                await roleManager.CreateAsync(role);
                logger.LogInformation($"Role '{roleInfo.Name}' created successfully.");
            }
        }

        // Seed Users
        var seedUsers = new[]
        {
            new 
            { 
                Email = "OsamaSuper@IIROSA.com", 
                Name = "Osama Abdelaziz",
                Role = "Super Admin" 
            },
            new 
            { 
                Email = "Admin@IIROSA.com", 
                Name = "System Admin",
                Role = "Admin" 
            },
            new 
            { 
                Email = "Charity@IIROSA.com", 
                Name = "Charity User",
                Role = "Charity" 
            },
            new 
            { 
                Email = "Accountant@IIROSA.com", 
                Name = "Accountant User",
                Role = "Accountant" 
            },
            new 
            { 
                Email = "FinancialOfficer@IIROSA.com", 
                Name = "Financial Officer",
                Role = "FinancialOfficer" 
            }
        };

        foreach (var userInfo in seedUsers)
        {
            var existingUser = await userManager.FindByEmailAsync(userInfo.Email);
            if (existingUser == null)
            {
                var user = new ApplicationUser
                {
                    UserName = userInfo.Email,
                    Email = userInfo.Email,
                    EmailConfirmed = true,
                    IsActive = true,
                    CreationDate = DateTime.UtcNow
                };

                var result = await userManager.CreateAsync(user, "P@ssw0rd@2022");
                
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, userInfo.Role);
                    logger.LogInformation($"User '{userInfo.Email}' created and assigned to role '{userInfo.Role}'.");
                }
                else
                {
                    logger.LogError($"Failed to create user '{userInfo.Email}': {string.Join(", ", result.Errors.Select(e => e.Description))}");
                }
            }
            else
            {
                logger.LogWarning($"User '{userInfo.Email}' already exists. Skipping creation.");
            }
        }
    }
}
```

**Program.cs Integration:**
```csharp
var app = builder.Build();

// Seed data on application startup (only in Development or on first run)
if (app.Environment.IsDevelopment() || IsFirstRun())
{
    using (var scope = app.Services.CreateScope())
    {
        await SeedData.InitializeAsync(scope.ServiceProvider);
    }
}
```

**Security Notes:**
- Default password "P@ssw0rd@2022" should be changed on first login for all seed users
- Implement force password change on first login for seed users
- Consider adding password expiration for seed accounts
- Log all seed data creation activities for audit trail
- Seed data script should be executed only by authorized personnel
- Remove or disable seed data script in production after initial deployment

**Maintenance:**
- Update seed data version when schema changes
- Document seed data changes in version control
- Test seed data creation in staging environment before production
- Keep seed data script in source control for reproducibility

---

#### Use Case UC-1.2: Create User

| Field | Value |
|-------|-------|
| **ID** | UC-1.2 |
| **Name** | Create User |
| **Actor** | Super Admin, Admin |
| **Priority** | High |
| **Description** | Create a new user account with role assignment, email, personal details, and default password |

**Preconditions:**
- UC-1.1 (Seed Roles and Users) has been executed
- Super Admin or Admin is logged in
- Super Admin or Admin has user creation permission

**Main Flow:**
1. Super Admin navigates to User Management page
2. System displays list of existing users with "Add New" button
3. Super Admin clicks "Add New User"
4. System displays user creation form with fields:
   - Email (required, unique)
   - First Name (required)
   - Last Name (required)
   - Phone Number (optional)
   - Role(s) (required, multi-select)
   - Default Password (auto-generated: P@ssw0rd@2022)
   - isActive (default: true)
5. Super Admin fills in required fields
6. System validates email uniqueness
7. System validates all required fields
8. System creates user account
9. System assigns selected role(s) to user
10. System sends welcome email with credentials
11. **SIMULTANEOUSLY** System creates audit log entry (per UC-17.1):
    - AuditLogId (GUID)
    - EntityType = "User"
    - EntityId = UserId of created user
    - Operation = Create
    - UserId = current user ID
    - UserName
    - Timestamp (UTC)
    - IpAddress
    - FieldChanges (JSON with all user fields after creation):
      ```json
      {
        "Email": {"old": null, "new": "user@example.com"},
        "FirstName": {"old": null, "new": "John"},
        "LastName": {"old": null, "new": "Doe"},
        "PhoneNumber": {"old": null, "new": "+1234567890"},
        "IsActive": {"old": null, "new": true},
        "AssignedRoles": {"old": null, "new": ["Admin", "Charity"]}
      }
      ```
12. System saves audit log entry to database
13. System displays success message
14. System redirects to user list with new user visible

**Alternative Flows:**
- **6a. Email already exists:** System displays error "Email already registered" and highlights email field
- **7a. Required field missing:** System highlights missing fields and displays validation message
- **9a. Role assignment fails:** System displays error and rolls back user creation

**Postconditions:**
- New user account exists in database
- User has assigned role(s)
- Audit log contains user creation record

---

#### Use Case UC-1.3: Update User

| Field | Value |
|-------|-------|
| **ID** | UC-1.3 |
| **Name** | Update User |
| **Actor** | Super Admin, Admin |
| **Priority** | High |
| **Description** | Modify existing user information including personal details and role reassignment |

**Preconditions:**
- UC-1.1 (Seed Roles and Users) has been executed
- Super Admin or Admin is logged in
- User account exists

**Main Flow:**
1. Super Admin navigates to User Management page
2. System displays list of users
3. Super Admin selects user to edit
4. System displays user edit form with current data
5. Super Admin modifies desired fields:
   - Email (if changed, must be unique)
   - First Name
   - Last Name
   - Phone Number
   - Role(s)
   - Active status
6. Super Admin clicks "Save"
7. System validates email uniqueness (if changed)
8. System retrieves current user values (before update)
9. System updates user record
10. System updates role assignments
11. **SIMULTANEOUSLY** System creates audit log entry (per UC-17.2):
    - AuditLogId (GUID)
    - EntityType = "User"
    - EntityId = UserId
    - Operation = Update
    - UserId = current user ID
    - UserName
    - Timestamp (UTC)
    - IpAddress
    - FieldChanges (JSON with changed fields only):
      ```json
      {
        "FirstName": {"old": "John", "new": "Jane"},
        "PhoneNumber": {"old": "+1234567890", "new": "+9876543210"},
        "AssignedRoles": {"old": ["Admin"], "new": ["Admin", "Accountant"]},
        "IsActive": {"old": true, "new": false}
      }
      ```
12. System saves audit log entry
13. System displays success message
14. System redirects to user list with updated data

**Alternative Flows:**
- **7a. Email conflict:** System displays error and prevents update
- **9a. Role update fails:** System displays error and rolls back changes

**Postconditions:**
- User information is updated in database
- Role assignments reflect changes
- Audit log contains update record with before/after values

---

#### Use Case UC-1.4: Deactivate User

| Field | Value |
|-------|-------|
| **ID** | UC-1.4 |
| **Name** | Deactivate User |
| **Actor** | Super Admin, Admin |
| **Priority** | High |
| **Description** | Disable user account to prevent system access while preserving data |

**Preconditions:**
- UC-1.1 (Seed Roles and Users) has been executed
- Super Admin or Admin is logged in
- User account is active

**Main Flow:**
1. Super Admin navigates to User Management page
2. System displays list of users
3. Super Admin selects user to deactivate
4. System displays user details with "Deactivate" button
5. Super Admin clicks "Deactivate"
6. System displays confirmation dialog: "Are you sure you want to deactivate this user?"
7. Super Admin confirms deactivation
8. System sets user.IsActive = false
9. **SIMULTANEOUSLY** System creates audit log entry (per UC-17.3):
    - AuditLogId (GUID)
    - EntityType = "User"
    - EntityId = UserId
    - Operation = Delete (soft delete/deactivation)
    - UserId = current user ID
    - UserName
    - Timestamp (UTC)
    - IpAddress
    - FieldChanges (JSON with status change):
      ```json
      {
        "IsActive": {"old": true, "new": false},
        "DeactivationReason": "Admin deactivation"
      }
      ```
10. System saves audit log entry
11. System displays success message
12. System updates user list (user shows as Inactive)
13. User can no longer log into system

**Alternative Flows:**
- **7a. User cancels:** System returns to user details without changes

**Postconditions:**
- User account is deactivated
- User cannot authenticate
- Audit log contains deactivation record
- User data preserved in database

---

#### Use Case UC-1.5: Reset User Password

| Field | Value |
|-------|-------|
| **ID** | UC-1.5 |
| **Name** | Reset User Password |
| **Actor** | Super Admin, Admin |
| **Priority** | High |
| **Description** | Reset password for any user account in the system |

**Preconditions:**
- UC-1.1 (Seed Roles and Users) has been executed
- Super Admin or Admin is logged in
- User account exists

**Main Flow:**
1. Super Admin navigates to User Management page
2. System displays list of users
3. Super Admin selects user for password reset
4. System displays user details with "Reset Password" button
5. Super Admin clicks "Reset Password"
6. System displays confirmation dialog
7. Super Admin confirms
8. System generates new password or prompts for manual entry
9. System hashes password and updates user record
10. **SIMULTANEOUSLY** System creates audit log entry (per UC-17.2):
    - AuditLogId (GUID)
    - EntityType = "User"
    - EntityId = UserId
    - Operation = Update (password reset)
    - UserId = current user ID
    - UserName
    - Timestamp (UTC)
    - IpAddress
    - FieldChanges (JSON):
      ```json
      {
        "PasswordReset": {"old": "[Hashed Old Password]", "new": "[Hashed New Password]"},
        "PasswordResetDate": {"old": null, "new": "2026-06-02T10:30:00Z"},
        "ResetBy": {"old": null, "new": "Admin@example.com"}
      }
      ```
    - Note: Password values are hashed before logging
11. System saves audit log entry
12. System displays success message with new password
13. System sends email to user with new password

**Alternative Flows:**
- **8a. Manual password entry:** Super Admin enters custom password instead of auto-generated
- **7a. User cancels:** System returns to user details without changes

**Postconditions:**
- User password is updated
- Audit log contains password reset record
- User receives email notification
- Old password no longer valid

---

#### Use Case UC-1.6: Assign User to Role

| Field | Value |
|-------|-------|
| **ID** | UC-1.6 |
| **Name** | Assign User to Role |
| **Actor** | Super Admin, Admin |
| **Priority** | High |
| **Description** | Assign one or more roles to a user granting specific permissions |

**Preconditions:**
- UC-1.1 (Seed Roles and Users) has been executed
- Super Admin or Admin is logged in
- User account exists
- Role(s) exist

**Main Flow:**
1. Super Admin navigates to User Management page
2. System displays list of users
3. Super Admin selects user
4. System displays user details with "Manage Roles" button
5. Super Admin clicks "Manage Roles"
6. System displays list of available roles with checkboxes
7. System highlights currently assigned roles
8. Super Admin selects/deselects roles
9. System retrieves current role assignments
10. Super Admin clicks "Save"
11. System updates role assignments
12. **SIMULTANEOUSLY** System creates audit log entry (per UC-17.2):
    - AuditLogId (GUID)
    - EntityType = "UserRole"
    - EntityId = UserId
    - Operation = Update (role assignment change)
    - UserId = current user ID
    - UserName
    - Timestamp (UTC)
    - IpAddress
    - FieldChanges (JSON):
      ```json
      {
        "AssignedRoles": {
          "old": ["Admin"],
          "new": ["Admin", "Accountant"]
        },
        "AddedRoles": ["Accountant"],
        "RemovedRoles": []
      }
      ```
13. System saves audit log entry
14. System displays success message
15. User permissions reflect new role assignments

**Alternative Flows:**
- **9a. No role selected:** System displays error "At least one role must be assigned"

**Postconditions:**
- User has assigned role(s)
- User permissions match role permissions
- Audit log contains role assignment record

---

#### Use Case UC-1.7: Create Role

| Field | Value |
|-------|-------|
| **ID** | UC-1.7 |
| **Name** | Create Role |
| **Actor** | Super Admin |
| **Priority** | Medium |
| **Description** | Define new role with specific permissions and claims |

**Preconditions:**
- UC-1.1 (Seed Roles and Users) has been executed
- Super Admin is logged in
- Super Admin has role creation permission

**Main Flow:**
1. Super Admin navigates to Role Management page
2. System displays list of existing roles
3. Super Admin clicks "Add New Role"
4. System displays role creation form with fields:
   - Role Name (required, unique)
   - Description (optional)
   - Permissions (checkbox list by module)
   - Claims (key-value pairs for granular permissions)
5. Super Admin fills in role details
6. Super Admin selects permissions for each module
7. Super Admin adds claims if needed
8. Super Admin clicks "Save"
9. System validates role name uniqueness
10. System creates role record
11. System creates role-permission mappings
12. System creates role-claim mappings
13. **SIMULTANEOUSLY** System creates audit log entry (per UC-17.1):
    - AuditLogId (GUID)
    - EntityType = "Role"
    - EntityId = RoleId
    - Operation = Create
    - UserId = current user ID
    - UserName
    - Timestamp (UTC)
    - IpAddress
    - FieldChanges (JSON with all role fields):
      ```json
      {
        "RoleName": {"old": null, "new": "ProjectManager"},
        "Description": {"old": null, "new": "Manages housing projects"},
        "AssignedPermissions": {"old": null, "new": ["ViewProjects", "EditProjects", "DeleteProjects"]},
        "AssignedClaims": {"old": null, "new": [{"type": "ProjectScope", "value": "All"}]}
      }
      ```
14. System saves audit log entry
15. System displays success message
16. Role appears in role list and user assignment dropdowns

**Alternative Flows:**
- **9a. Role name exists:** System displays error "Role name already exists"

**Postconditions:**
- New role exists in database
- Role has assigned permissions and claims
- Role available for user assignment
- Audit log contains role creation record

---

#### Use Case UC-1.8: Update Role Permissions

| Field | Value |
|-------|-------|
| **ID** | UC-1.8 |
| **Name** | Update Role Permissions |
| **Actor** | Super Admin |
| **Priority** | Medium |
| **Description** | Modify permissions and claims associated with existing role |

**Preconditions:**
- UC-1.1 (Seed Roles and Users) has been executed
- Super Admin is logged in
- Role exists

**Main Flow:**
1. Super Admin navigates to Role Management page
2. System displays list of roles
3. Super Admin selects role to edit
4. System displays role edit form with current permissions
5. Super Admin modifies permissions (checkboxes)
6. Super Admin adds/removes claims
7. Super Admin clicks "Save"
8. System retrieves current role permissions and claims
9. System updates role-permission mappings
10. System updates role-claim mappings
11. **SIMULTANEOUSLY** System creates audit log entry (per UC-17.2):
    - AuditLogId (GUID)
    - EntityType = "Role"
    - EntityId = RoleId
    - Operation = Update
    - UserId = current user ID
    - UserName
    - Timestamp (UTC)
    - IpAddress
    - FieldChanges (JSON with permission changes):
      ```json
      {
        "AssignedPermissions": {
          "old": ["ViewProjects", "EditProjects"],
          "new": ["ViewProjects", "EditProjects", "DeleteProjects"]
        },
        "AddedPermissions": ["DeleteProjects"],
        "RemovedPermissions": [],
        "AssignedClaims": {
          "old": [{"type": "ProjectScope", "value": "Own"}],
          "new": [{"type": "ProjectScope", "value": "All"}]
        }
      }
      ```
12. System saves audit log entry
13. System displays success message
14. All users with this role receive updated permissions

**Alternative Flows:**
- **8a. Update fails:** System displays error and rolls back changes

**Postconditions:**
- Role permissions reflect changes
- All users with role have updated access
- Audit log contains permission change record

---

#### Use Case UC-1.9: View All Users

| Field | Value |
|-------|-------|
| **ID** | UC-1.9 |
| **Name** | View All Users |
| **Actor** | Super Admin, Admin |
| **Priority** | Low |
| **Description** | View list of all system users with their roles, status, and contact information |

**Preconditions:**
- UC-1.1 (Seed Roles and Users) has been executed
- Super Admin or Admin is logged in

**Main Flow:**
1. User navigates to User Management page
2. System displays list of users in grid/table with columns:
   - Email
   - Full Name
   - Role(s)
   - Status (Active/Inactive)
   - Phone Number
   - Creation Date
   - Last Login
3. System provides search functionality
4. System provides filter by role
5. System provides filter by status
6. System provides pagination
7. System provides sort by any column
8. User can click any user to view details
9. System provides "Export to Excel" button to export current filtered/sorted user list
10. User can export list to Excel format with all displayed columns

**Postconditions:**
- User list is displayed
- User can search, filter, and sort
- User can export data to Excel

---

#### Use Case UC-1.10: View User Activity

| Field | Value |
|-------|-------|
| **ID** | UC-1.10 |
| **Name** | View User Activity |
| **Actor** | Super Admin, Admin |
| **Priority** | Medium |
| **Description** | View audit log of user actions, login history, and system interactions |

**Preconditions:**
- UC-1.1 (Seed Roles and Users) has been executed
- Super Admin or Admin is logged in
- User account exists

**Main Flow:**
1. Super Admin navigates to User Management page
2. System displays list of users
3. Super Admin selects user
4. System displays user details with "View Activity" button
5. Super Admin clicks "View Activity"
6. System retrieves audit log entries for user
7. System displays activity timeline with:
   - Timestamp
   - Action type (Create, Update, Delete, Login, Logout)
   - Entity affected
   - Details/Changes
   - IP Address
8. System provides filter by date range
9. System provides filter by action type
10. System provides pagination

**Postconditions:**
- User activity history is displayed
- Super Admin can audit user actions

---

#### Use Case UC-1.11: Manage User Claims

| Field | Value |
|-------|-------|
| **ID** | UC-1.11 |
| **Name** | Manage User Claims |
| **Actor** | Super Admin |
| **Priority** | Low |
| **Description** | Assign or remove specific claims for granular permission control beyond role-based permissions |

**Preconditions:**
- UC-1.1 (Seed Roles and Users) has been executed
- Super Admin is logged in
- User account exists

**Main Flow:**
1. Super Admin navigates to User Management page
2. System displays list of users
3. Super Admin selects user
4. System displays user details with "Manage Claims" button
5. Super Admin clicks "Manage Claims"
6. System displays current claims with key-value pairs
7. System provides "Add Claim" button
8. Super Admin clicks "Add Claim"
9. System displays claim form:
   - Claim Type (dropdown or text)
   - Claim Value (text)
10. Super Admin enters claim details
11. Super Admin clicks "Save"
12. System adds claim to user
13. **SIMULTANEOUSLY** System creates audit log entry (per UC-17.2):
    - AuditLogId (GUID)
    - EntityType = "UserClaim"
    - EntityId = UserId
    - Operation = Update (claim addition)
    - UserId = current user ID
    - UserName
    - Timestamp (UTC)
    - IpAddress
    - FieldChanges (JSON):
      ```json
      {
        "ClaimType": {"old": null, "new": "ProjectScope"},
        "ClaimValue": {"old": null, "new": "AllProjects"},
        "Action": "AddClaim"
      }
      ```
14. System saves audit log entry
15. System displays updated claims list
16. Super Admin can remove claims via "Delete" button (also logged)

**Postconditions:**
- User has specific claims beyond role permissions
- Audit log contains claim changes

---
