# Notification System Use Cases

### Module: Real-Time Notifications (SignalR)

**Module Owner:** All Users (System-wide)  
**Purpose:** Deliver real-time notifications to users based on system events  
**Dependencies:** Framework.Core (Notifications tables), SignalR Hub  
**Technical Stack:** SignalR, Framework.Core.Notifications, Entity Framework  

---

## Data Model (Framework.Core Notifications Tables)

The system uses the following existing tables from Framework.Core:

```sql
-- Notifications Table (Framework.Core)
CREATE TABLE Notifications (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    UserId UNIQUEIDENTIFIER NOT NULL,  -- FK to aspnet_Users
    Title NVARCHAR(200) NOT NULL,
    Message NVARCHAR(MAX) NOT NULL,
    NotificationType NVARCHAR(50) NOT NULL,  -- Info, Warning, Error, Success
    Category NVARCHAR(100),  -- Approval, Reminder, Alert, System
    ActionUrl NVARCHAR(500),  -- Link to related entity
    EntityType NVARCHAR(100),  -- e.g., "Orphan", "Family", "PeriodicReport"
    EntityId UNIQUEIDENTIFIER,  -- ID of related entity
    IsRead BIT DEFAULT 0,
    ReadDate DATETIME NULL,
    CreatedDate DATETIME DEFAULT GETDATE(),
    CreatedBy UNIQUEIDENTIFIER,
    Priority INT DEFAULT 0,  -- 0=Normal, 1=High, 2=Urgent
    ExpiryDate DATETIME NULL,  -- Auto-dismiss after this date
    SentViaEmail BIT DEFAULT 0,
    SentViaSMS BIT DEFAULT 0,
    SentViaPush BIT DEFAULT 0,
    EmailSentDate DATETIME NULL,
    SMSSentDate DATETIME NULL,
    PushSentDate DATETIME NULL
)

-- Notification Settings (User Preferences)
CREATE TABLE NotificationSettings (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    UserId UNIQUEIDENTIFIER NOT NULL,
    EmailNotifications BIT DEFAULT 1,
    SMSNotifications BIT DEFAULT 0,
    PushNotifications BIT DEFAULT 1,
    InAppNotifications BIT DEFAULT 1,
    QuietHoursStart TIME NULL,
    QuietHoursEnd TIME NULL,
    MuteAll BIT DEFAULT 0,
    Categories NVARCHAR(MAX)  -- JSON array of subscribed categories
)
```

---

#### Use Case UC-NOTIF-1: Send Real-Time Notification (System)

| Field | Value |
|-------|-------|
| **ID** | UC-NOTIF-1 |
| **Name** | Send Real-Time Notification (System) |
| **Actor** | System (Automated) |
| **Priority** | High |
| **Description** | Automatically create and send notification when system events occur |

**Preconditions:**
- User exists in system
- Event triggers notification requirement
- SignalR connection established (for real-time delivery)

**Main Flow:**
1. **Event Triggered:** System event occurs (e.g., record created, approved, rejected)
2. **Notification Creation:**
   - System creates notification record in Notifications table
   - Populates fields: UserId, Title, Message, Type, Category, ActionUrl
   - Sets EntityType and EntityId for context
   - Sets CreatedDate = current timestamp
3. **SignalR Broadcast:**
   - System invokes SignalR Hub method: `SendNotification(userId, notification)`
   - Hub sends real-time push to connected clients for userId
   - Client receives notification immediately (if online)
4. **Notification Display:**
   - User's browser/app shows toast/popup notification
   - Notification bell icon shows unread count badge
   - Notification added to notification center dropdown
5. **Fallback Delivery (if user offline):**
   - System checks NotificationSettings for user preferences
   - If EmailNotifications = true: sends email
   - If SMSNotifications = true: sends SMS (for urgent notifications)
   - Sets appropriate Sent flags (SentViaEmail, SentViaSMS)
6. **Quiet Hours Check:**
   - System checks if current time within QuietHoursStart-QuietHoursEnd
   - If within quiet hours and not urgent: delays delivery until after quiet period
7. **Audit Logging:**
   - System logs notification in audit log
   - Tracks delivery status and timestamps

**Postconditions:**
- Notification created in database
- Real-time push sent via SignalR
- User receives notification (if online)
- Fallback delivery queued (if offline)
- Delivery status tracked

**Technical Implementation:**
```csharp
// Service Layer
public async Task SendNotification(Guid userId, NotificationType type, string title, string message, 
    string category = null, string actionUrl = null, string entityType = null, Guid? entityId = null)
{
    // Create notification record
    var notification = new Notification
    {
        Id = Guid.NewGuid(),
        UserId = userId,
        Title = title,
        Message = message,
        NotificationType = type.ToString(),
        Category = category,
        ActionUrl = actionUrl,
        EntityType = entityType,
        EntityId = entityId,
        CreatedDate = DateTime.UtcNow,
        Priority = type == NotificationType.Urgent ? 2 : 0
    };
    
    await _context.Notifications.AddAsync(notification);
    await _context.SaveChangesAsync();
    
    // Send via SignalR
    await _notificationHub.Clients.User(userId.ToString())
        .SendAsync("ReceiveNotification", notification);
    
    // Check quiet hours and send email/SMS if needed
    await SendFallbackNotifications(notification);
}
```

---

#### Use Case UC-NOTIF-2: View Notification Center

| Field | Value |
|-------|-------|
| **ID** | UC-NOTIF-2 |
| **Name** | View Notification Center |
| **Actor** | All Users |
| **Priority** | High |
| **Description** | View all notifications with filtering and search capabilities |

**Preconditions:**
- User is logged in
- User has received notifications

**Main Flow:**
1. User clicks notification bell icon in header
2. System displays notification center dropdown/panel with:
   - **Tabs/Sections:**
     - "All" (all notifications)
     - "Unread" (IsRead = false)
     - "Important" (Priority > 0)
     - "Approvals" (Category = 'Approval')
   - **Notification List:** Grouped by date (Today, Yesterday, This Week, Older)
   - **Each Notification Shows:**
     - Icon/Badge based on Type (Info = Blue, Warning = Yellow, Error = Red, Success = Green)
     - Title (bold if unread)
     - Message (truncated if long)
     - Time ago (e.g., "5 minutes ago")
     - Unread indicator (dot/badge)
     - Action button/link (if ActionUrl exists)
3. **Filtering & Search:**
   - Search by title/message
   - Filter by type (Info, Warning, Error, Success)
   - Filter by category (Approval, Reminder, Alert, System)
   - Filter by date range
4. **Pagination:** 20 notifications per section (load more on scroll)
5. **Mark as Read:**
   - Viewing notification center auto-marks visible as read
   - Or manual "Mark all as read" button
6. **Real-time Updates:**
   - SignalR connection listens for new notifications
   - Auto-refreshes notification center when new notification arrives
   - Plays sound (optional, based on settings)

**Postconditions:**
- User sees all notifications organized by date
- Unread notifications highlighted
- User can click to navigate to related entities
- Notifications marked as read when viewed

---

#### Use Case UC-NOTIF-3: Mark Notification as Read/Unread

| Field | Value |
|-------|-------|
| **ID** | UC-NOTIF-3 |
| **Name** | Mark Notification as Read/Unread |
| **Actor** | All Users |
| **Priority** | Medium |
| **Description** | Manually mark notification as read or unread |

**Preconditions:**
- User is logged in
- Notification exists

**Main Flow:**
1. User views notification center
2. User performs one of:
   - **Click notification:** System marks as read (IsRead = true, ReadDate = now)
   - **Click "Mark as unread":** System marks as unread (IsRead = false, ReadDate = null)
   - **Click "Mark all as read":** System marks all user's notifications as read
3. System updates notification record
4. System updates unread count badge
5. System updates UI via SignalR (sync across multiple devices)
6. System logs action in audit log

**Postconditions:**
- Notification read status updated
- Unread count badge updated
- Change synced across user's devices

---

#### Use Case UC-NOTIF-4: Delete Notification

| Field | Value |
|-------|-------|
| **ID** | UC-NOTIF-4 |
| **Name** | Delete Notification |
| **Actor** | All Users |
| **Priority** | Low |
| **Description** | Remove notification from list |

**Preconditions:**
- User is logged in
- Notification exists

**Main Flow:**
1. User views notification center
2. User clicks "X" or "Delete" on notification
3. System displays confirmation: "Delete this notification?"
4. User confirms
5. System soft-deletes notification (sets IsDeleted = true, DeletedDate = now)
6. System removes notification from UI
7. System updates unread count
8. System logs deletion in audit log

**Postconditions:**
- Notification removed from view
- Unread count updated
- Audit trail maintained

---

#### Use Case UC-NOTIF-5: Configure Notification Preferences

| Field | Value |
|-------|-------|
| **ID** | UC-NOTIF-5 |
| **Name** | Configure Notification Preferences |
| **Actor** | All Users |
| **Priority** | Medium |
| **Description** | Set how and when to receive notifications |

**Preconditions:**
- User is logged in

**Main Flow:**
1. User navigates to Settings → Notifications
2. System displays notification preferences form with sections:
   - **Delivery Channels:**
     - Email Notifications (checkbox)
     - SMS Notifications (checkbox)
     - Push/Browser Notifications (checkbox)
     - In-App Notifications (checkbox, default: true)
   - **Quiet Hours:**
     - Enable Quiet Hours (checkbox)
     - Start Time (time picker, e.g., 22:00)
     - End Time (time picker, e.g., 08:00)
     - "Allow urgent notifications during quiet hours" (checkbox)
   - **Category Subscriptions:**
     - Approvals (checkbox) - When approval required/reviewed
     - Reminders (checkbox) - Deadlines, follow-ups
     - Alerts (checkbox) - System alerts, errors
     - Updates (checkbox) - Record updates, changes
     - Reports (checkbox) - Report generation complete
   - **Per-Module Settings:**
     - Families: Enable/Disable
     - Orphans: Enable/Disable
     - Charities: Enable/Disable
     - Periodic Reports: Enable/Disable
     - Projects: Enable/Disable
     - Missions: Enable/Disable
     - etc.
   - **Sound:**
     - Play Sound (checkbox)
     - Sound Selection (dropdown)
3. User modifies preferences
4. User clicks "Save"
5. System updates NotificationSettings record
6. System logs changes in audit log
7. System displays success message

**Postconditions:**
- Notification preferences updated
- Future notifications respect preferences
- Settings persist across sessions

---

#### Use Case UC-NOTIF-6: Batch Delete Old Notifications

| Field | Value |
|-------|-------|
| **ID** | UC-NOTIF-6 |
| **Name** | Batch Delete Old Notifications |
| **Actor** | System (Scheduled Job) |
| **Priority** | Low |
| **Description** | Automatically clean up old read notifications |

**Preconditions:**
- Scheduled job configured
- Notification retention policy defined

**Main Flow:**
1. **Scheduled Job runs** (e.g., daily at 2:00 AM)
2. System queries notifications WHERE:
   - CreatedDate < [Retention Period] (e.g., 90 days ago)
   - IsRead = true
   - Priority = 0 (Normal, not important)
3. System soft-deletes matched notifications (IsDeleted = true)
4. System logs cleanup in audit log:
   - Count of notifications deleted
   - Date range cleaned
5. System sends summary to admin (optional)

**Postconditions:**
- Old read notifications removed
- Database size controlled
- Important/Unread notifications preserved

---

## Notification Triggers by Module

### **Families Module:**
- **Family Created** → Notify Charity users (if created by Admin)
- **Family Updated** → Notify assigned Charity (if data changed)
- **Family Deactivated** → Notify Charity
- **Orphan Added** → Notify Charity
- **Orphan Updated** → Notify Charity
- **Missing Documents Alert** → Notify Charity (if MissingDocuments = true)

### **Charities Module:**
- **Charity Registered** → Notify Super Admin, Admin
- **Charity Activated/Deactivated** → Notify Charity users
- **Rights Changed** → Notify Charity (Add/Update rights)
- **Account Locked** → Notify Charity
- **Password Reset** → Notify Charity via email

### **Periodic Orphan Reports Module:**
- **Report Submitted** → Notify Reviewers (Admin, Super Admin, Accountant, Employee)
- **Report Approved** → Notify submitting Charity
- **Report Rejected** → Notify submitting Charity (with reason)
- **Report Due Reminder** → Notify Charity (3 days before deadline)
- **Overdue Report Alert** → Notify Charity + Admin (when past due)

### **Projects Module:**
- **Project Created** → Notify assigned Charity (if applicable)
- **Project Completed** → Notify Admin, Super Admin
- **Budget Exceeded** → Notify Financial Officer, Admin (warning)
- **Project Deadline Approaching** → Notify Project Manager (reminder)

### **Missions Module:**
- **Mission Assigned** → Notify assigned user
- **Mission Due Soon** → Notify assigned user (reminder)
- **Mission Overdue** → Notify assigned user + Admin (alert)
- **Mission Completed** → Notify Admin

### **Seasonal Aid Module:**
- **Campaign Created** → Notify all Charity users (if applicable)
- **Campaign Closing Soon** → Notify Charities (reminder to distribute)
- **Distribution Required** → Notify assigned Charities

### **General Checks Module:**
- **Check Issued** → Notify Accountant
- **Check Due Today** → Notify Accountant (reminder)
- **Check Cleared** → Notify issuer
- **Check Bounced** → Notify Accountant + Admin (alert)

### **Employees Module:**
- **Employee Added** → Notify new employee (welcome email with credentials)
- **Employee Deactivated** → Notify employee
- **Password Reset** → Notify employee
- **Role Changed** → Notify employee
- **Performance Review Due** → Notify Manager + Employee

### **Imports & Exports Module:**
- **Import Completed** → Notify user who initiated
- **Import Failed** → Notify user (with error details)
- **Export Ready** → Notify user (with download link)

---

## SignalR Hub Implementation

### **NotificationHub.cs**
```csharp
[Authorize]
public class NotificationHub : Hub
{
    private readonly ApplicationDbContext _context;
    private readonly IUserService _userService;

    public NotificationHub(ApplicationDbContext context, IUserService userService)
    {
        _context = context;
        _userService = userService;
    }

    public async Task JoinNotificationGroup()
    {
        var userId = _userService.GetCurrentUserId();
        await Groups.AddToGroupAsync(Context.ConnectionId, $"User_{userId}");
    }

    public async Task LeaveNotificationGroup()
    {
        var userId = _userService.GetCurrentUserId();
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"User_{userId}");
    }

    public override async Task OnConnectedAsync()
    {
        await JoinNotificationGroup();
        
        // Send pending unread notifications on connect
        var userId = _userService.GetCurrentUserId();
        var unreadNotifications = await _context.Notifications
            .Where(n => n.UserId == userId && !n.IsRead && !n.IsDeleted)
            .OrderByDescending(n => n.CreatedDate)
            .Take(10)
            .ToListAsync();
        
        if (unreadNotifications.Any())
        {
            await Clients.Caller.SendAsync("ReceivePendingNotifications", unreadNotifications);
        }
        
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception exception)
    {
        await LeaveNotificationGroup();
        await base.OnDisconnectedAsync(exception);
    }
}
```

---

## Client-Side Implementation (JavaScript/TypeScript)

### **notification.service.js**
```javascript
// SignalR Connection Setup
const connection = new signalR.HubConnectionBuilder()
    .withUrl("/notificationHub")
    .withAutomaticReconnect()
    .configureLogging(signalR.LogLevel.Information)
    .build();

// Start connection
connection.start().catch(err => console.error(err));

// Listen for real-time notifications
connection.on("ReceiveNotification", (notification) => {
    // Update UI
    showNotificationToast(notification);
    updateUnreadCount();
    addToNotificationCenter(notification);
    playNotificationSound();
});

// Listen for pending notifications on connect
connection.on("ReceivePendingNotifications", (notifications) => {
    notifications.forEach(notification => {
        addToNotificationCenter(notification);
    });
    updateUnreadCount();
});

// Show toast notification
function showNotificationToast(notification) {
    const toast = document.createElement('div');
    toast.className = `notification-toast notification-${notification.notificationType.toLowerCase()}`;
    toast.innerHTML = `
        <div class="toast-icon">${getNotificationIcon(notification.notificationType)}</div>
        <div class="toast-content">
            <div class="toast-title">${notification.title}</div>
            <div class="toast-message">${notification.message}</div>
        </div>
        <div class="toast-actions">
            ${notification.actionUrl ? `<a href="${notification.actionUrl}" class="toast-action">View</a>` : ''}
            <button onclick="dismissToast(this)">×</button>
        </div>
    `;
    
    document.getElementById('toast-container').appendChild(toast);
    
    // Auto-dismiss after 5 seconds
    setTimeout(() => {
        toast.classList.add('toast-dismissing');
        setTimeout(() => toast.remove(), 300);
    }, 5000);
}

// Update unread count badge
async function updateUnreadCount() {
    const response = await fetch('/api/notifications/unread-count');
    const count = await response.json();
    const badge = document.getElementById('notification-badge');
    badge.textContent = count > 0 : count : '';
    badge.style.display = count > 0 ? 'block' : 'none';
}
```

---

## Notification Templates by Type

### **Approval Notifications:**
```csharp
// Periodic Report Submitted
Title: "New Periodic Report Requires Review"
Message: "Charity {CharityName} has submitted periodic report for orphan {OrphanName}. Please review."
ActionUrl: "/PeriodicReports/Review/{ReportId}"
Category: "Approval"
Priority: High (1)

// Report Approved
Title: "Periodic Report Approved"
Message: "Good news! The periodic report for {OrphanName} has been approved."
ActionUrl: "/PeriodicReports/Details/{ReportId}"
Category: "Approval"

// Report Rejected
Title: "Periodic Report Rejected"
Message: "The periodic report for {OrphanName} requires changes. Reason: {RefuseReason}"
ActionUrl: "/PeriodicReports/Edit/{ReportId}"
Category: "Approval"
Priority: High (1)
```

### **Reminder Notifications:**
```csharp
// Report Due Soon
Title: "Periodic Report Due Soon"
Message: "Reminder: Periodic report for {OrphanName} is due in 3 days."
ActionUrl: "/PeriodicReports/Create?orphanId={OrphanId}"
Category: "Reminder"

// Check Payment Due
Title: "Check Payment Due Today"
Message: "Check #{CheckNumber} for {Amount} {Currency} is due today."
ActionUrl: "/Checks/Details/{CheckId}"
Category: "Reminder"
```

### **Alert Notifications:**
```csharp
// Account Locked
Title: "Account Locked"
Message: "Your charity account has been locked. Please contact administrator."
ActionUrl: "/Contact"
Category: "Alert"
Priority: Urgent (2)

// Budget Exceeded
Title: "Project Budget Exceeded"
Message: "Project {ProjectName} has exceeded its budget by {ExcessAmount}."
ActionUrl: "/Projects/Details/{ProjectId}"
Category: "Alert"
Priority: High (1)
```

### **System Notifications:**
```csharp
// Import Completed
Title: "Import Completed Successfully"
Message: "Successfully imported {RecordCount} incoming letters."
ActionUrl: "/Imports/History/{ImportId}"
Category: "System"

// Employee Added
Title: "Welcome to the System!"
Message: "Your employee account has been created. Please log in with your temporary password."
ActionUrl: "/Login"
Category: "System"
```

---

## API Endpoints

```csharp
// GET: /api/notifications
// Get user's notifications with filtering
[HttpGet]
public async Task<ActionResult<PaginatedList<NotificationDto>>> GetNotifications(
    bool? isRead = null,
    string category = null,
    string type = null,
    DateTime? startDate = null,
    DateTime? endDate = null,
    int page = 1,
    int pageSize = 20)
{
    var userId = GetCurrentUserId();
    var query = _context.Notifications
        .Where(n => n.UserId == userId && !n.IsDeleted);
    
    if (isRead.HasValue) query = query.Where(n => n.IsRead == isRead.Value);
    if (!string.IsNullOrEmpty(category)) query = query.Where(n => n.Category == category);
    if (!string.IsNullOrEmpty(type)) query = query.Where(n => n.NotificationType == type);
    if (startDate.HasValue) query = query.Where(n => n.CreatedDate >= startDate.Value);
    if (endDate.HasValue) query = query.Where(n => n.CreatedDate <= endDate.Value);
    
    var notifications = await query
        .OrderByDescending(n => n.CreatedDate)
        .Skip((page - 1) * pageSize)
        .Take(pageSize)
        .ToListAsync();
    
    return Ok(notifications);
}

// GET: /api/notifications/unread-count
// Get count of unread notifications
[HttpGet("unread-count")]
public async Task<ActionResult<int>> GetUnreadCount()
{
    var userId = GetCurrentUserId();
    var count = await _context.Notifications
        .CountAsync(n => n.UserId == userId && !n.IsRead && !n.IsDeleted);
    return Ok(count);
}

// PUT: /api/notifications/{id}/mark-read
// Mark notification as read
[HttpPut("{id}/mark-read")]
public async Task<ActionResult> MarkAsRead(Guid id)
{
    var userId = GetCurrentUserId();
    var notification = await _context.Notifications
        .FirstOrDefaultAsync(n => n.Id == id && n.UserId == userId);
    
    if (notification == null) return NotFound();
    
    notification.IsRead = true;
    notification.ReadDate = DateTime.UtcNow;
    await _context.SaveChangesAsync();
    
    // Broadcast via SignalR to sync across devices
    await _notificationHub.Clients.User(userId.ToString())
        .SendAsync("NotificationMarkedRead", id);
    
    return Ok();
}

// PUT: /api/notifications/mark-all-read
// Mark all notifications as read
[HttpPut("mark-all-read")]
public async Task<ActionResult> MarkAllAsRead()
{
    var userId = GetCurrentUserId();
    var notifications = await _context.Notifications
        .Where(n => n.UserId == userId && !n.IsRead && !n.IsDeleted)
        .ToListAsync();
    
    foreach (var notification in notifications)
    {
        notification.IsRead = true;
        notification.ReadDate = DateTime.UtcNow;
    }
    
    await _context.SaveChangesAsync();
    
    // Broadcast via SignalR
    await _notificationHub.Clients.User(userId.ToString())
        .SendAsync("AllNotificationsMarkedRead");
    
    return Ok();
}

// DELETE: /api/notifications/{id}
// Delete notification
[HttpDelete("{id}")]
public async Task<ActionResult> DeleteNotification(Guid id)
{
    var userId = GetCurrentUserId();
    var notification = await _context.Notifications
        .FirstOrDefaultAsync(n => n.Id == id && n.UserId == userId);
    
    if (notification == null) return NotFound();
    
    notification.IsDeleted = true;
    notification.DeletedDate = DateTime.UtcNow;
    await _context.SaveChangesAsync();
    
    // Broadcast via SignalR
    await _notificationHub.Clients.User(userId.ToString())
        .SendAsync("NotificationDeleted", id);
    
    return Ok();
}

// GET: /api/notifications/settings
// Get user's notification preferences
[HttpGet("settings")]
public async Task<ActionResult<NotificationSettingsDto>> GetSettings()
{
    var userId = GetCurrentUserId();
    var settings = await _context.NotificationSettings
        .FirstOrDefaultAsync(s => s.UserId == userId);
    
    if (settings == null)
    {
        // Create default settings
        settings = new NotificationSettings { UserId = userId };
        _context.NotificationSettings.Add(settings);
        await _context.SaveChangesAsync();
    }
    
    return Ok(settings);
}

// PUT: /api/notifications/settings
// Update notification preferences
[HttpPut("settings")]
public async Task<ActionResult> UpdateSettings(UpdateNotificationSettingsDto dto)
{
    var userId = GetCurrentUserId();
    var settings = await _context.NotificationSettings
        .FirstOrDefaultAsync(s => s.UserId == userId);
    
    if (settings == null)
    {
        settings = new NotificationSettings { UserId = userId };
        _context.NotificationSettings.Add(settings);
    }
    
    settings.EmailNotifications = dto.EmailNotifications;
    settings.SMSNotifications = dto.SMSNotifications;
    settings.PushNotifications = dto.PushNotifications;
    settings.InAppNotifications = dto.InAppNotifications;
    settings.QuietHoursStart = dto.QuietHoursStart;
    settings.QuietHoursEnd = dto.QuietHoursEnd;
    settings.MuteAll = dto.MuteAll;
    settings.Categories = JsonConvert.SerializeObject(dto.Categories);
    
    await _context.SaveChangesAsync();
    
    return Ok(settings);
}
```

---

## Database Indexes for Performance

```sql
-- Improve query performance for notifications
CREATE INDEX IX_Notifications_UserId_CreatedDate 
    ON Notifications(UserId, CreatedDate DESC);

CREATE INDEX IX_Notifications_UserId_IsRead 
    ON Notifications(UserId, IsRead, CreatedDate DESC);

CREATE INDEX IX_Notifications_UserId_Category 
    ON Notifications(UserId, Category, CreatedDate DESC);

CREATE INDEX IX_Notifications_UserId_Priority 
    ON Notifications(UserId, Priority DESC, CreatedDate DESC);

CREATE INDEX IX_Notifications_EntityType_EntityId 
    ON Notifications(EntityType, EntityId);

-- Include columns for covering queries
CREATE INDEX IX_Notifications_UserId_IsRead_IsDeleted 
    ON Notifications(UserId, IsRead, IsDeleted)
    INCLUDE (Title, Message, CreatedDate, NotificationType, Category, ActionUrl);
```

---

## Monitoring & Analytics

### **Notification Analytics Dashboard (Admin/Super Admin):**
- Total notifications sent (by day/week/month)
- Delivery rate (SignalR success, email success rate)
- Open rate (read notifications)
- Click-through rate (ActionUrl clicks)
- Notifications by type/category
- Average delivery time
- Failed notifications (with error logs)
- User engagement metrics
- Most active users
- Notification volume trends

---

## Security Considerations

1. **Authorization:** Users can only access their own notifications
2. **Rate Limiting:** Prevent notification spam
3. **Sensitive Data:** Mask sensitive information in notifications
4. **Privacy:** Respect notification preferences and quiet hours
5. **Audit Trail:** Log all notification sends, deliveries, reads
6. **GDPR Compliance:** Allow users to delete notification history
7. **Data Retention:** Auto-delete old notifications per policy
