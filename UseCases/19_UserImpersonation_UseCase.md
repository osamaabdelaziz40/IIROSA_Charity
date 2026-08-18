# 19 - User Impersonation Use Case

### Module: User Impersonation

**Module Owner:** Super Admin, Admin  
**Purpose:** Allow authorized administrators to login as another user for troubleshooting, support, and verification purposes  
**Dependencies:** Framework.Identity, Audit Logging (UC-17)  
**Security Level:** Critical

---

## Module Overview

User Impersonation allows Super Admin and Admin users to temporarily assume the identity of another user in the system. This feature is essential for:

- **Troubleshooting:** Reproduce user-reported issues
- **Support:** Assist users with complex operations
- **Verification:** Validate user permissions and data access
- **Training:** Demonstrate system functionality
- **Testing:** Verify role-based access controls

### Security Considerations

- **Restricted Access:** Only Super Admin and Admin roles can impersonate
- **Audit Trail:** All impersonation activities are logged with full details
- **Time-Limited:** Impersonation sessions can be configured with timeout
- **Explicit Consent:** System clearly indicates impersonation mode
- **Non-Critical Operations:** Certain operations are blocked during impersonation

### Actors

| Actor | Description | Impersonation Permission |
|-------|-------------|---------------------------|
| **Super Admin** | System owner with full access | Can impersonate ANY user including Admin |
| **Admin** | Organization administrator | Can impersonate ANY user EXCEPT Super Admin |
| **Impersonated User** | The user whose identity is being assumed | N/A (passive actor) |

---

## Use Cases

#### Use Case UC-19.1: Start User Impersonation

| Field | Value |
|-------|-------|
| **ID** | UC-19.1 |
| **Name** | Start User Impersonation |
| **Actor** | Super Admin, Admin |
| **Priority** | High |
| **Description** | Initiate impersonation session by entering target user's username |

**Preconditions:**
- Super Admin or Admin is logged in
- User has impersonation permission
- Target user account exists and is active
- Target user is NOT a Super Admin (for Admin actor)

**Main Flow:**
1. Super Admin navigates to User Management or User List page
2. System displays list of users with "Impersonate" button for each eligible user
3. Super Admin locates target user in list
4. Super Admin clicks "Impersonate" button next to target user
   - OR Super Admin uses global search to find user by username
   - OR Super Admin enters username directly in impersonation dialog
5. System validates impersonation request:
   - Verify current user has impersonation permission
   - Verify target user is active
   - Verify target user is not Super Admin (if current user is Admin)
6. System displays confirmation dialog:
   ```
   Impersonate User: [Target User Name]
   Email: [target@example.com]
   Role: [Charity]
   
   WARNING: You will be logged in as this user.
   All your actions will be audited.
   The impersonation indicator will be visible at all times.
   
   [Cancel]  [Start Impersonation]
   ```
7. Super Admin confirms impersonation
8. System generates impersonation session:
   - Creates ImpersonationSession record:
     * ImpersonationSessionId (GUID)
     * ImpersonatorUserId = current user ID
     * ImpersonatedUserId = target user ID
     * StartTime = current timestamp (UTC)
     * EndTime = null
     * IsActive = true
     * OriginalUserIpAddress = current IP
     * OriginalUserName = current user username
   - Stores original user context in secure session
   - Sets current authentication context to target user
9. **SIMULTANEOUSLY** System creates audit log entry (per UC-17.1):
   - AuditLogId (GUID)
   - EntityType = "ImpersonationSession"
   - EntityId = ImpersonationSessionId
   - Operation = Create
   - UserId = impersonator user ID
   - UserName = impersonator username
   - Timestamp (UTC)
   - IpAddress = original IP
   - FieldChanges (JSON):
     ```json
     {
       "Action": "StartImpersonation",
       "ImpersonatorUserId": "[impersonator-guid]",
       "ImpersonatorUserName": "admin@iirosa.com",
       "ImpersonatorRole": "Admin",
       "ImpersonatedUserId": "[target-guid]",
       "ImpersonatedUserName": "charity@iirosa.com",
       "ImpersonatedRole": "Charity",
       "StartTime": "2026-06-03T12:00:00Z",
       "SourceIpAddress": "192.168.1.100"
     }
     ```
10. System saves audit log entry
11. System redirects to home page/dashboard
12. System displays prominent impersonation indicator:
    - Fixed banner at top of screen: **"⚠️ IMPERSONATION MODE: You are logged in as [User Name] ([Role])"**
    - Banner color: Distinctive (e.g., orange/yellow) to differentiate from normal mode
    - Display "Return to Admin Account" button
13. System sets current user context to target user:
    - User.Name = target user name
    - User.Email = target user email
    - User.Roles = target user roles
    - User.Permissions = target user permissions
    - User.Claims = target user claims + ImpersonatorClaim

**Alternative Flows:**
- **5a. Target user not found:** System displays error "User not found" and returns to user list
- **5b. Target user is inactive:** System displays error "Cannot impersonate inactive user account"
- **5c. Target user is Super Admin (Admin actor):** System displays error "You do not have permission to impersonate Super Admin users"
- **5d. Current user lacks permission:** System displays error "You do not have impersonation permission"
- **7a. User cancels:** System returns to previous page without starting impersonation

**Postconditions:**
- Impersonation session is active
- Current user context is set to target user
- Impersonation indicator is visible on all pages
- Original user context is securely stored
- Audit log contains impersonation start record
- All user actions will be performed as target user

**Business Rules:**
- BR-19.1.1: Only Super Admin and Admin roles can initiate impersonation
- BR-19.1.2: Admin cannot impersonate Super Admin users
- BR-19.1.3: Target user must have active account (IsActive = true)
- BR-19.1.4: Only one impersonation session can be active at a time per user
- BR-19.1.5: Impersonation sessions automatically expire after 8 hours (configurable)

---

#### Use Case UC-19.2: End User Impersonation

| Field | Value |
|-------|-------|
| **ID** | UC-19.2 |
| **Name** | End User Impersonation |
| **Actor** | Super Admin, Admin (currently in impersonation mode) |
| **Priority** | High |
| **Description** | Terminate impersonation session and return to original user account |

**Preconditions:**
- Active impersonation session exists
- Current user is in impersonation mode

**Main Flow:**
1. Super Admin (in impersonation mode) clicks "Return to Admin Account" button in impersonation banner
   - OR Super Admin navigates to profile menu and selects "End Impersonation"
   - OR Super Admin clicks on impersonation indicator banner
2. System displays confirmation dialog:
   ```
   End Impersonation Session
   
   You are currently impersonating: [User Name]
   Started: [timestamp]
   Duration: [X minutes]
   
   [Cancel]  [End Impersonation]
   ```
3. Super Admin confirms ending impersonation
4. System retrieves impersonation session details:
   - ImpersonatorUserId
   - ImpersonatorUserName
   - SessionStartTime
   - ActionsPerformed (count of operations)
5. System updates ImpersonationSession record:
   - EndTime = current timestamp (UTC)
   - IsActive = false
   - SessionDuration = EndTime - StartTime
6. **SIMULTANEOUSLY** System creates audit log entry (per UC-17.3):
   - AuditLogId (GUID)
   - EntityType = "ImpersonationSession"
   - EntityId = ImpersonationSessionId
   - Operation = Delete (end session)
   - UserId = impersonated user ID (context during impersonation)
   - UserName = impersonated username
   - Timestamp (UTC)
   - IpAddress = IP during impersonation
   - FieldChanges (JSON):
     ```json
     {
       "Action": "EndImpersonation",
       "ImpersonatorUserId": "[impersonator-guid]",
       "ImpersonatorUserName": "admin@iirosa.com",
       "ImpersonatedUserId": "[target-guid]",
       "ImpersonatedUserName": "charity@iirosa.com",
       "StartTime": "2026-06-03T12:00:00Z",
       "EndTime": "2026-06-03T14:30:00Z",
       "SessionDuration": "02:30:00",
       "ActionsPerformed": 15
     }
     ```
7. System saves audit log entry
8. System restores original user context:
   - User.Name = impersonator name
   - User.Email = impersonator email
   - User.Roles = impersonator roles
   - User.Permissions = impersonator permissions
9. System clears impersonation session data
10. System redirects to User Management or last page before impersonation
11. System displays success message:
    - "Impersonation session ended successfully"
    - Display session duration summary
12. Impersonation banner is removed

**Alternative Flows:**
- **3a. User cancels:** System returns to current page in impersonation mode
- **2a. System timeout (auto-end):** If impersonation session exceeds maximum duration (8 hours), system automatically ends session and redirects to login page with message "Your impersonation session has expired. Please login again."

**Postconditions:**
- Impersonation session is terminated
- Original user context is restored
- User returns to their normal administrative account
- Audit log contains impersonation end record with session duration
- Impersonation indicator is no longer visible

**Business Rules:**
- BR-19.2.1: Impersonation sessions automatically expire after 8 hours (configurable in app settings)
- BR-19.2.2: System logs all actions performed during impersonation
- BR-19.2.3: Impersonation session duration is calculated and stored for audit purposes

---

#### Use Case UC-19.3: View Active Impersonation Sessions

| Field | Value |
|-------|-------|
| **ID** | UC-19.3 |
| **Name** | View Active Impersonation Sessions |
| **Actor** | Super Admin |
| **Priority** | Medium |
| **Description** | View all currently active impersonation sessions in the system |

**Preconditions:**
- Super Admin is logged in
- User has impersonation management permission

**Main Flow:**
1. Super Admin navigates to Administration / Security / Impersonation Sessions
2. System retrieves all active impersonation sessions:
   - WHERE IsActive = true
   - ORDER BY StartTime DESC
3. System displays active sessions in grid/table with columns:
   - Impersonator Name (with email)
   - Impersonator Role
   - Impersonated User Name (with email)
   - Impersonated Role
   - Session Start Time
   - Session Duration (calculated)
   - Source IP Address
   - Actions (Terminate Session button)
4. System provides filter by impersonator
5. System provides filter by impersonated user
6. System provides auto-refresh every 30 seconds
7. Super Admin can terminate any active session by clicking "Terminate"
8. System displays session count summary

**Postconditions:**
- All active impersonation sessions are visible
- Super Admin can monitor impersonation activity
- Super Admin can terminate inappropriate sessions

---

#### Use Case UC-19.4: Terminate Impersonation Session (Admin Override)

| Field | Value |
|-------|-------|
| **ID** | UC-19.4 |
| **Name** | Terminate Impersonation Session (Admin Override) |
| **Actor** | Super Admin |
| **Priority** | High |
| **Description** | Forcefully terminate an active impersonation session initiated by another admin |

**Preconditions:**
- Super Admin is logged in
- Active impersonation session exists

**Main Flow:**
1. Super Admin navigates to Active Impersonation Sessions page (per UC-19.3)
2. System displays list of active sessions
3. Super Admin identifies session to terminate
4. Super Admin clicks "Terminate" button
5. System displays confirmation dialog:
   ```
   Terminate Impersonation Session
   
   Impersonator: [Admin Name]
   Impersonated: [User Name]
   Started: [timestamp]
   
   This will immediately end the impersonation session.
   The impersonator will be returned to their account.
   
   [Cancel]  [Terminate]
   ```
6. Super Admin confirms termination
7. System updates impersonation session:
   - EndTime = current timestamp (UTC)
   - IsActive = false
   - TerminatedBy = current Super Admin ID
   - TerminationReason = "Admin Override"
8. **SIMULTANEOUSLY** System creates audit log entry (per UC-17.3):
   - AuditLogId (GUID)
   - EntityType = "ImpersonationSession"
   - EntityId = ImpersonationSessionId
   - Operation = Delete (forced termination)
   - UserId = current Super Admin ID
   - UserName = current Super Admin username
   - Timestamp (UTC)
   - IpAddress
   - FieldChanges (JSON):
     ```json
     {
       "Action": "ForcedTermination",
       "TerminatedBy": "[superadmin-guid]",
       "TerminatedByUserName": "superadmin@iirosa.com",
       "OriginalImpersonator": "[admin-guid]",
       "OriginalImpersonatedUser": "[user-guid]",
       "TerminationReason": "Admin Override",
       "OriginalStartTime": "2026-06-03T12:00:00Z"
     }
     ```
9. System saves audit log entry
10. System sends notification to impersonator:
    - "Your impersonation session has been terminated by [Super Admin Name]"
    - "Reason: Admin Override"
11. System refreshes active sessions list

**Alternative Flows:**
- **6a. User cancels:** System returns to sessions list without changes

**Postconditions:**
- Impersonation session is terminated
- Audit log contains forced termination record
- Original impersonator is notified
- Impersonator cannot continue impersonating

**Business Rules:**
- BR-19.4.1: Only Super Admin can terminate another admin's impersonation session
- BR-19.4.2: Admin override is logged with full audit trail
- BR-19.4.3: Original impersonator is notified of session termination

---

#### Use Case UC-19.5: Impersonate by Username (Quick Access)

| Field | Value |
|-------|-------|
| **ID** | UC-19.5 |
| **Name** | Impersonate by Username (Quick Access) |
| **Actor** | Super Admin, Admin |
| **Priority** | Medium |
| **Description** | Quick impersonation access via global search by username |

**Preconditions:**
- Super Admin or Admin is logged in
- User has impersonation permission

**Main Flow:**
1. Super Admin uses global keyboard shortcut (e.g., Ctrl+Shift+I)
   - OR clicks "Impersonate" button in global navigation header
2. System displays impersonation dialog:
   ```
   Quick Impersonation
   
   Enter Username: [_______________]
   
   [Search]  [Cancel]
   ```
3. Super Admin enters target username (partial or full)
4. System searches for matching users:
   - WHERE UserName LIKE '%input%' OR Email LIKE '%input%'
   - WHERE IsActive = true
   - Excludes Super Admin users (for Admin actor)
5. System displays search results dropdown with:
   - User Name
   - Email
   - Role
   - [Impersonate] button for each result
6. Super Admin selects target user from results
7. System proceeds with impersonation flow (per UC-19.1, steps 6-13)

**Alternative Flows:**
- **4a. No users found:** System displays "No users found matching your search"
- **4b. Multiple results:** System displays all matches for selection

**Postconditions:**
- Impersonation is initiated via quick access method
- User does not need to navigate to User Management page

**Business Rules:**
- BR-19.5.1: Search returns maximum of 10 results to prevent abuse
- BR-19.5.2: Search requires minimum 3 characters

---

#### Use Case UC-19.6: View Impersonation History

| Field | Value |
|-------|-------|
| **ID** | UC-19.6 |
| **Name** | View Impersonation History |
| **Actor** | Super Admin |
| **Priority** | Low |
| **Description** | View historical impersonation sessions for audit and analysis |

**Preconditions:**
- Super Admin is logged in
- User has impersonation management permission

**Main Flow:**
1. Super Admin navigates to Administration / Security / Impersonation History
2. System retrieves historical impersonation sessions:
   - All sessions (active and ended)
   - ORDER BY StartTime DESC
3. System displays history in grid/table with columns:
   - Impersonator Name
   - Impersonated User Name
   - Start Time
   - End Time
   - Session Duration
   - Status (Active/Ended)
   - Terminated By (if applicable)
   - Actions Performed Count
   - Source IP
4. System provides filters:
   - Date range picker
   - Impersonator dropdown
   - Impersonated user dropdown
   - Status (Active/Ended)
5. System provides export to Excel
6. System provides pagination
7. Super Admin can click session to view details:
   - Full session information
   - List of actions performed (from audit log)
   - Session timeline

**Postconditions:**
- Super Admin can review all impersonation activity
- Historical data is available for compliance and analysis

---

#### Use Case UC-19.7: Perform Actions During Impersonation

| Field | Value |
|-------|-------|
| **ID** | UC-19.7 |
| **Name** | Perform Actions During Impersonation |
| **Actor** | Super Admin, Admin (impersonating) |
| **Priority** | High |
| **Description** | Execute system actions on behalf of impersonated user with proper audit trail |

**Preconditions:**
- Active impersonation session exists
- Current user is in impersonation mode

**Main Flow:**
1. Super Admin (impersonating Charity user) navigates to any authorized page
2. System checks permissions based on impersonated user's role
3. System displays content accessible to impersonated user
4. Super Admin performs action (e.g., creates Orphan record):
   - System validates impersonated user has permission for action
   - System proceeds with normal action flow
5. **SIMULTANEOUSLY** System creates audit log entry with enhanced context:
   - AuditLogId (GUID)
   - EntityType = "Orphan" (or relevant entity)
   - EntityId = created entity ID
   - Operation = Create
   - UserId = impersonated user ID (technical context)
   - UserName = impersonated username
   - **ImpersonatorUserId = impersonator user ID** (additional field)
   - **ImpersonatorUserName = impersonator username** (additional field)
   - **IsImpersonatedAction = true** (additional field)
   - Timestamp (UTC)
   - IpAddress
   - FieldChanges (JSON with entity changes):
     ```json
     {
       "Action": "Create",
       "Entity": "Orphan",
       "EntityId": "[orphan-guid]",
       "PerformedBy": {
         "ActualUserId": "[charity-guid]",
         "ActualUserName": "charity@iirosa.com",
         "ImpersonatorUserId": "[admin-guid]",
         "ImpersonatorUserName": "admin@iirosa.com",
         "ImpersonationSessionId": "[session-guid]"
       },
       "FieldChanges": {
         "FullName": {"old": null, "new": "Ahmed Mohamed"},
         "DateOfBirth": {"old": null, "new": "2015-05-15"}
       }
     }
     ```
6. System saves audit log entry with impersonation context
7. System updates ImpersonationSession.ActionsPerformedCount += 1

**Restricted Operations During Impersonation:**
The following operations are **BLOCKED** during impersonation:
- Cannot change own password (security risk)
- Cannot modify user roles/permissions (security risk)
- Cannot delete or deactivate the impersonated user account
- Cannot initiate nested impersonation (cannot impersonate while impersonating)
- Cannot perform financial transactions requiring secondary approval
- Cannot export sensitive data to external systems

**Alternative Flows:**
- **4a. Insufficient permissions:** System displays error "The impersonated user does not have permission for this action"
- **4b. Restricted operation:** System displays error "This operation cannot be performed during impersonation for security reasons"

**Postconditions:**
- Action is performed on behalf of impersonated user
- Audit log contains complete context including impersonator information
- Impersonation session action counter is incremented
- Security restrictions are enforced

**Business Rules:**
- BR-19.7.1: All actions during impersonation are logged with both impersonator and impersonated user context
- BR-19.7.2: Nested impersonation is not allowed
- BR-19.7.3: Certain security-sensitive operations are blocked during impersonation
- BR-19.7.4: Impersonator inherits all permissions of impersonated user (no elevated access)

---

#### Use Case UC-19.8: Configure Impersonation Settings

| Field | Value |
|-------|-------|
| **ID** | UC-19.8 |
| **Name** | Configure Impersonation Settings |
| **Actor** | Super Admin |
| **Priority** | Low |
| **Description** | Configure global impersonation settings and policies |

**Preconditions:**
- Super Admin is logged in
- Super Admin has system configuration permission

**Main Flow:**
1. Super Admin navigates to Administration / Security / Impersonation Settings
2. System displays configuration form with settings:
   
   **Session Settings:**
   - Maximum Session Duration (hours) [Default: 8]
   - Session Warning Threshold (minutes before expiry) [Default: 15]
   - Allow Session Extension [Yes/No] [Default: Yes]
   - Maximum Extensions per Session [Default: 2]
   
   **Security Settings:**
   - Require Confirmation Before Impersonation [Yes/No] [Default: Yes]
   - Log All Actions During Impersonation [Yes/No] [Default: Yes, cannot be disabled]
   - Notify Impersonated User [Yes/No] [Default: No]
   - IP Restriction (require same IP) [Yes/No] [Default: No]
   
   **Audit Settings:**
   - Retention Period for Impersonation Logs (days) [Default: 365]
   - Include in Compliance Reports [Yes/No] [Default: Yes]
   
3. Super Admin modifies settings as needed
4. Super Admin clicks "Save Settings"
5. **SIMULTANEOUSLY** System creates audit log entry (per UC-17.2):
   - AuditLogId (GUID)
   - EntityType = "SystemSetting"
   - EntityId = "ImpersonationSettings"
   - Operation = Update
   - UserId = current Super Admin ID
   - UserName
   - Timestamp (UTC)
   - IpAddress
   - FieldChanges (JSON):
     ```json
     {
       "MaximumSessionDuration": {"old": 8, "new": 12},
       "SessionWarningThreshold": {"old": 15, "new": 10},
       "RequireConfirmation": {"old": true, "new": true}
     }
     ```
6. System saves audit log entry
7. System updates settings in database
8. System displays success message
9. Settings are applied immediately for new sessions

**Postconditions:**
- Impersonation settings reflect changes
- New sessions use updated settings
- Audit log contains settings change record

---

## Data Model

### ImpersonationSession Entity

```csharp
public class ImpersonationSession : FullAuditedEntity
{
    /// <summary>
    /// The user who is initiating the impersonation (admin/super admin)
    /// </summary>
    public Guid ImpersonatorUserId { get; set; }
    
    /// <summary>
    /// The user whose identity is being assumed
    /// </summary>
    public Guid ImpersonatedUserId { get; set; }
    
    /// <summary>
    /// Session start timestamp (UTC)
    /// </summary>
    public DateTime StartTime { get; set; }
    
    /// <summary>
    /// Session end timestamp (UTC) - null for active sessions
    /// </summary>
    public DateTime? EndTime { get; set; }
    
    /// <summary>
    /// Indicates if session is currently active
    /// </summary>
    public bool IsActive { get; set; } = true;
    
    /// <summary>
    /// IP address from which impersonation was initiated
    /// </summary>
    public string? OriginalUserIpAddress { get; set; }
    
    /// <summary>
    /// Username of impersonator
    /// </summary>
    public string? OriginalUserName { get; set; }
    
    /// <summary>
    /// Number of actions performed during this session
    /// </summary>
    public int ActionsPerformedCount { get; set; } = 0;
    
    /// <summary>
    /// If session was terminated by another admin, their user ID
    /// </summary>
    public Guid? TerminatedBy { get; set; }
    
    /// <summary>
    /// Reason for termination (if applicable)
    /// </summary>
    public string? TerminationReason { get; set; }
    
    // Navigation Properties
    public virtual ApplicationUser? ImpersonatorUser { get; set; }
    public virtual ApplicationUser? ImpersonatedUser { get; set; }
}
```

### Enhanced AuditLog Entity Fields

```csharp
// Additional fields for impersonation tracking
public Guid? ImpersonatorUserId { get; set; }
public string? ImpersonatorUserName { get; set; }
public bool IsImpersonatedAction { get; set; } = false;
public Guid? ImpersonationSessionId { get; set; }
```

---

## API Endpoints

### Impersonation Controller

```csharp
[ApiController]
[Route("api/[controller]")]
public class ImpersonationController : ControllerBase
{
    // POST: api/impersonation/start
    [HttpPost("start")]
    [Authorize(Roles = "SuperAdmin, Admin")]
    public async Task<IActionResult> StartImpersonation([FromBody] StartImpersonationRequest request)
    {
        // Validate request
        // Check permissions
        // Create impersonation session
        // Return session token
    }
    
    // POST: api/impersonation/end
    [HttpPost("end")]
    public async Task<IActionResult> EndImpersonation()
    {
        // Validate active session
        // End session
        // Restore original context
    }
    
    // GET: api/impersonation/active
    [HttpGet("active")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> GetActiveSessions()
    {
        // Return all active sessions
    }
    
    // POST: api/impersonation/terminate/{sessionId}
    [HttpPost("terminate/{sessionId}")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> TerminateSession(Guid sessionId)
    {
        // Force terminate session
    }
    
    // GET: api/impersonation/history
    [HttpGet("history")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> GetHistory([FromQuery] HistoryFilter filter)
    {
        // Return historical sessions
    }
}
```

---

## Security Considerations

### Authentication & Authorization

1. **Permission Claims:**
   - `CanImpersonate` - Required to initiate impersonation
   - `CanImpersonateSuperAdmin` - Required to impersonate Super Admin users (Super Admin only)
   - `IsImpersonating` - Set during impersonation session

2. **Role Restrictions:**
   - Only Super Admin and Admin roles have `CanImpersonate` claim
   - Admin cannot impersonate users with Super Admin role
   - Charity, Accountant, FinancialOfficer cannot impersonate

3. **Session Security:**
   - Impersonation context stored in secure cookie with HttpOnly, Secure flags
   - Original user ID encrypted in session token
   - Sessions cannot be transferred between IP addresses (if configured)

### Audit & Compliance

1. **Comprehensive Logging:**
   - All impersonation start/end events logged
   - All actions during impersonation logged with enhanced context
   - Logs include both impersonator and impersonated user information

2. **Retention:**
   - Impersonation session logs retained for 365 days (configurable)
   - Logs included in compliance reports
   - Historical data available for forensic analysis

3. **Monitoring:**
   - Active sessions visible to Super Admin
   - Abnormal session duration alerts
   - Termination of inappropriate sessions

### User Experience

1. **Visual Indicators:**
   - Persistent banner during impersonation
   - Distinct color scheme for impersonation mode
   - Clear "Return to Admin Account" action

2. **Behavioral Differences:**
   - Some operations disabled during impersonation
   - Clear error messages for restricted actions
   - User always aware of impersonation status

---

## Implementation Notes

### Database Schema

```sql
CREATE TABLE [IIROSA].[ImpersonationSessions] (
    [Id] UNIQUEIDENTIFIER PRIMARY KEY,
    [ImpersonatorUserId] UNIQUEIDENTIFIER NOT NULL,
    [ImpersonatedUserId] UNIQUEIDENTIFIER NOT NULL,
    [StartTime] DATETIME2 NOT NULL,
    [EndTime] DATETIME2 NULL,
    [IsActive] BIT NOT NULL DEFAULT 1,
    [OriginalUserIpAddress] NVARCHAR(50) NULL,
    [OriginalUserName] NVARCHAR(256) NULL,
    [ActionsPerformedCount] INT NOT NULL DEFAULT 0,
    [TerminatedBy] UNIQUEIDENTIFIER NULL,
    [TerminationReason] NVARCHAR(200) NULL,
    [CreatedDate] DATETIME2 NOT NULL,
    [CreatedBy] UNIQUEIDENTIFIER NULL,
    [ModifiedDate] DATETIME2 NOT NULL,
    [ModifiedBy] UNIQUEIDENTIFIER NULL,
    [IsDeleted] BIT NOT NULL DEFAULT 0,
    [DeletedDate] DATETIME2 NULL,
    [DeletedBy] UNIQUEIDENTIFIER NULL
);

-- Add impersonation tracking to AuditLog
ALTER TABLE [Framework].[AuditLogs]
ADD [ImpersonatorUserId] UNIQUEIDENTIFIER NULL,
    [ImpersonatorUserName] NVARCHAR(256) NULL,
    [IsImpersonatedAction] BIT NULL DEFAULT 0,
    [ImpersonationSessionId] UNIQUEIDENTIFIER NULL;
```

### Configuration (appsettings.json)

```json
{
  "ImpersonationSettings": {
    "MaxSessionDurationHours": 8,
    "WarningThresholdMinutes": 15,
    "AllowSessionExtension": true,
    "MaxExtensionsPerSession": 2,
    "RequireConfirmation": true,
    "LogAllActions": true,
    "NotifyImpersonatedUser": false,
    "IpRestrictionEnabled": false,
    "LogRetentionDays": 365
  }
}
```

---

## Cross-Module References

| Module | Reference |
|--------|-----------|
| UC-1: User Role Management | Uses user/role data for impersonation validation |
| UC-17: Audit Logging | All impersonation actions logged here |
| UC-18: Cross-Cutting Concerns | Uses notifications for session termination alerts |

---

**Version:** 1.0  
**Date:** 2026-06-03  
**Author:** System Analyst  
**Status:** Complete  
**Module Number:** 19

---

## Change History

| Version | Date | Changes | Author |
|---------|------|---------|--------|
| 1.0 | 2026-06-03 | Initial creation of User Impersonation use cases | System Analyst |
