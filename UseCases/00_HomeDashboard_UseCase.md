# Home Dashboard Use Cases

### Module: Home Dashboard (Analytics & Statistics)

**Module Owner:** All Users (Role-based views)  
**Purpose:** Provide at-a-glance statistics, KPIs, and quick actions for all users  
**Dependencies:** All modules (aggregated data), SignalR (real-time updates), User & Role Management (UC-1.1: Seed Roles and Users must be executed first)  
**Technical Stack:** SQL Views, Stored Procedures, Chart.js/D3.js, SignalR  
**Initialization Requirement:** Execute UC-1.1 (Seed Roles and Users) before dashboard can be accessed  

---

## Dashboard Architecture

### **Role-Based Dashboard Views:**

1. **Super Admin Dashboard** - System-wide statistics
2. **Admin Dashboard** - Organization-wide statistics
3. **Charity Dashboard** - Charity-specific statistics
4. **Accountant Dashboard** - Financial statistics
5. **Employee Dashboard** - Task-based statistics

---

## Data Model (Dashboard Views & Stored Procedures)

```sql
-- Dashboard Statistics View (System-wide)
CREATE VIEW vw_Dashboard_Statistics AS
SELECT 
    -- Orphans Statistics
    (SELECT COUNT(*) FROM Orphans WHERE IsDeleted = 0) AS TotalOrphans,
    (SELECT COUNT(*) FROM Orphans WHERE IsDeleted = 0 AND SponsorshipStatus = 'Sponsored') AS SponsoredOrphans,
    (SELECT COUNT(*) FROM Orphans WHERE IsDeleted = 0 AND SponsorshipStatus = 'Unsponsored') AS UnsponsoredOrphans,
    
    -- Families Statistics
    (SELECT COUNT(*) FROM Families WHERE IsDeleted = 0) AS TotalFamilies,
    (SELECT COUNT(DISTINCT FK_CharityId) FROM Families WHERE IsDeleted = 0) AS ActiveCharities,
    
    -- Periodic Reports Statistics
    (SELECT COUNT(*) FROM PeriodicReports WHERE IsDeleted = 0) AS TotalPeriodicReports,
    (SELECT COUNT(*) FROM PeriodicReports WHERE IsDeleted = 0 AND Reviewed = 1 AND IsAccepted = 1) AS ApprovedReports,
    (SELECT COUNT(*) FROM PeriodicReports WHERE IsDeleted = 0 AND Reviewed = 1 AND IsRefused = 1) AS RejectedReports,
    (SELECT COUNT(*) FROM PeriodicReports WHERE IsDeleted = 0 AND Reviewed = 0) AS PendingReports,
    
    -- Charities Statistics
    (SELECT COUNT(*) FROM Charities WHERE IsDeleted = 0) AS TotalCharities,
    (SELECT COUNT(*) FROM Charities WHERE IsDeleted = 0 AND IsActive = 1) AS ActiveCharitiesCount,
    (SELECT COUNT(*) FROM Charities WHERE IsDeleted = 0 AND IsActive = 0) AS InactiveCharities,
    (SELECT COUNT(*) FROM Charities WHERE IsDeleted = 0 AND IsLocked = 1) AS LockedCharities,
    
    -- Projects Statistics
    (SELECT COUNT(*) FROM OfficeProjects WHERE IsDeleted = 0) AS TotalOfficeProjects,
    (SELECT COUNT(*) FROM OfficeProjects WHERE IsDeleted = 0 AND IsFinished = 1) AS CompletedOfficeProjects,
    (SELECT COUNT(*) FROM HousingProjects WHERE IsDeleted = 0) AS TotalHousingProjects,
    (SELECT COUNT(*) FROM HousingProjects WHERE IsDeleted = 0 AND ProjectStatus = 'Completed') AS CompletedHousingProjects,
    
    -- Missions Statistics
    (SELECT COUNT(*) FROM Missions WHERE IsDeleted = 0) AS TotalMissions,
    (SELECT COUNT(*) FROM Missions WHERE IsDeleted = 0 AND IsMissionCompleted = 1) AS CompletedMissions,
    (SELECT COUNT(*) FROM Missions WHERE IsDeleted = 0 AND IsMissionCompleted = 0 AND MissionDate < CAST(GETDATE() AS DATE)) AS OverdueMissions,
    
    -- Financial Statistics
    (SELECT COUNT(*) FROM Checks WHERE IsDeleted = 0 AND CheckStatus = 'Pending') AS PendingChecks,
    (SELECT COUNT(*) FROM Checks WHERE IsDeleted = 0 AND CheckStatus = 'Cleared') AS ClearedChecks,
    (SELECT COUNT(*) FROM Checks WHERE IsDeleted = 0 AND CheckStatus = 'Void') AS VoidChecks,
    (SELECT ISNULL(SUM(Amount), 0) FROM Checks WHERE IsDeleted = 0 AND CheckStatus = 'Cleared') AS TotalClearedAmount,
    
    -- Seasonal Aid Statistics
    (SELECT COUNT(*) FROM SeasonalAidCampaigns WHERE IsDeleted = 0) AS TotalCampaigns,
    (SELECT COUNT(*) FROM SeasonalAidCampaigns WHERE IsDeleted = 0 AND IsClosed = 0) AS ActiveCampaigns,
    
    -- Employees Statistics
    (SELECT COUNT(*) FROM aspnet_Users WHERE IsDeleted = 0 AND RoleId = 'Employee') AS TotalEmployees,
    (SELECT COUNT(*) FROM aspnet_Users WHERE IsDeleted = 0 AND IsActive = 1 AND RoleId = 'Employee') AS ActiveEmployees
GO

-- Charity-Specific Dashboard Statistics
CREATE PROCEDURE sp_Dashboard_CharityStatistics
    @CharityId UNIQUEIDENTIFIER
AS
BEGIN
    SELECT 
        -- Charity Orphans
        (SELECT COUNT(*) FROM Orphans WHERE FK_CharityId = @CharityId AND IsDeleted = 0) AS TotalOrphans,
        (SELECT COUNT(*) FROM Orphans WHERE FK_CharityId = @CharityId AND IsDeleted = 0 AND SponsorshipStatus = 'Sponsored') AS SponsoredOrphans,
        (SELECT COUNT(*) FROM Orphans WHERE FK_CharityId = @CharityId AND IsDeleted = 0 AND SponsorshipStatus = 'Unsponsored') AS UnsponsoredOrphans,
        
        -- Charity Families
        (SELECT COUNT(*) FROM Families WHERE FK_CharityId = @CharityId AND IsDeleted = 0) AS TotalFamilies,
        
        -- Charity Periodic Reports
        (SELECT COUNT(*) FROM PeriodicReports pr
         INNER JOIN Orphans o ON pr.FK_ChildId = o.Id
         WHERE o.FK_CharityId = @CharityId AND pr.IsDeleted = 0) AS TotalPeriodicReports,
        
        (SELECT COUNT(*) FROM PeriodicReports pr
         INNER JOIN Orphans o ON pr.FK_ChildId = o.Id
         WHERE o.FK_CharityId = @CharityId AND pr.IsDeleted = 0 AND pr.Reviewed = 0) AS PendingReports,
         
        (SELECT COUNT(*) FROM PeriodicReports pr
         INNER JOIN Orphans o ON pr.FK_ChildId = o.Id
         WHERE o.FK_CharityId = @CharityId AND pr.IsDeleted = 0 AND pr.Reviewed = 1 AND pr.IsAccepted = 1) AS ApprovedReports,
         
        (SELECT COUNT(*) FROM PeriodicReports pr
         INNER JOIN Orphans o ON pr.FK_ChildId = o.Id
         WHERE o.FK_CharityId = @CharityId AND pr.IsDeleted = 0 AND pr.Reviewed = 1 AND pr.IsRefused = 1) AS RejectedReports,
        
        -- Upcoming Tasks
        (SELECT COUNT(*) FROM PeriodicReports pr
         INNER JOIN Orphans o ON pr.FK_ChildId = o.Id
         WHERE o.FK_CharityId = @CharityId 
         AND pr.IsDeleted = 0 
         AND pr.ReportDate >= CAST(GETDATE() AS DATE)
         AND pr.ReportDate <= DATEADD(DAY, 7, CAST(GETDATE() AS DATE))
         AND NOT EXISTS (
             SELECT 1 FROM PeriodicReports pr2 
             WHERE pr2.FK_ChildId = pr.FK_ChildId 
             AND pr2.ReportDate > pr.ReportDate
             AND pr2.IsDeleted = 0
         )) AS ReportsDueThisWeek,
        
        -- Rights Status
        (SELECT IsAddEnabled FROM Charities WHERE Id = @CharityId) AS CanAddRecords,
        (SELECT IsUpdateEnabled FROM Charities WHERE Id = @CharityId) AS CanUpdateRecords,
        (SELECT IsLocked FROM Charities WHERE Id = @CharityId) AS IsLocked,
        
        -- Recent Activity
        (SELECT COUNT(*) FROM AuditLog 
         WHERE CharityId = @CharityId 
         AND ActionDate >= DATEADD(DAY, -7, GETDATE())) AS RecentActivityCount
END
GO

-- Time-Series Data for Charts
CREATE PROCEDURE sp_Dashboard_OrphansTrend
    @StartDate DATE,
    @EndDate DATE,
    @CharityId UNIQUEIDENTIFIER = NULL
AS
BEGIN
    SELECT 
        DATEADD(MONTH, DATEDIFF(MONTH, 0, CreationDate), 0) AS Month,
        COUNT(*) AS NewOrphans,
        SUM(CASE WHEN SponsorshipStatus = 'Sponsored' THEN 1 ELSE 0 END) AS SponsoredOrphans
    FROM Orphans
    WHERE IsDeleted = 0
    AND CAST(CreationDate AS DATE) BETWEEN @StartDate AND @EndDate
    AND (@CharityId IS NULL OR FK_CharityId = @CharityId)
    GROUP BY DATEADD(MONTH, DATEDIFF(MONTH, 0, CreationDate), 0)
    ORDER BY Month
END
GO

-- Pending Tasks for User
CREATE PROCEDURE sp_Dashboard_PendingTasks
    @UserId UNIQUEIDENTIFIER,
    @UserRole NVARCHAR(50)
AS
BEGIN
    -- Periodic Reports Pending Review (for Admin, SuperAdmin, Accountant, Employee)
    IF @UserRole IN ('Admin', 'SuperAdmin', 'Accountant', 'Employee')
    BEGIN
        SELECT 
            'PeriodicReportReview' AS TaskType,
            'Review Periodic Report' AS Title,
            pr.Id AS EntityId,
            c.Name AS CharityName,
            o.FullName AS OrphanName,
            pr.ReportDate AS DueDate,
            pr.CreatedDate AS SubmittedDate,
            'High' AS Priority
        FROM PeriodicReports pr
        INNER JOIN Orphans o ON pr.FK_ChildId = o.Id
        INNER JOIN Charities c ON o.FK_CharityId = c.Id
        WHERE pr.Reviewed = 0
        AND pr.IsDeleted = 0
        AND pr.IsActive = 1
        ORDER BY pr.CreatedDate ASC
    END
    
    -- Overdue Missions (for assigned users)
    SELECT 
        'MissionOverdue' AS TaskType,
        'Overdue Mission' AS Title,
        m.Id AS EntityId,
        m.MissionTarget,
        m.MissionDate AS DueDate,
        'High' AS Priority
    FROM Missions m
    WHERE m.FK_UserId = @UserId
    AND m.IsMissionCompleted = 0
    AND m.MissionDate < CAST(GETDATE() AS DATE)
    AND m.IsDeleted = 0
    
    -- Checks Due Today (for Accountant)
    IF @UserRole = 'Accountant'
    BEGIN
        SELECT 
            'CheckDueToday' AS TaskType,
            'Check Payment Due' AS Title,
            chk.Id AS EntityId,
            chk.CheckNumber,
            chk.DueDate,
            chk.Amount,
            'Medium' AS Priority
        FROM Checks chk
        WHERE chk.DueDate = CAST(GETDATE() AS DATE)
        AND chk.CheckStatus = 'Pending'
        AND chk.IsDeleted = 0
    END
END
GO
```

---

## Dashboard Use Cases

#### Use Case UC-DASH-1: View Home Dashboard

| Field | Value |
|-------|-------|
| **ID** | UC-DASH-1 |
| **Name** | View Home Dashboard |
| **Actor** | All Users |
| **Priority** | High |
| **Description** | View personalized dashboard with statistics, KPIs, charts, and pending tasks |

**Preconditions:**
- System initialization complete (UC-1.1: Seed Roles and Users executed)
- User is logged in
- User has appropriate role permissions
- At least one user account exists with valid credentials

**Main Flow:**
1. User logs into system
2. System redirects to Home Dashboard (default page after login)
3. System displays dashboard based on user role:
   
   **For Super Admin / Admin:**
   - **Top Statistics Cards (4 columns):**
     - Total Orphans (with trend indicator ↑↓)
     - Total Families
     - Active Charities
     - Total Employees
     - Pending Periodic Reports
     - Active Projects
     - Pending Checks
     - Total Budget (this month)
   
   - **Charts Section (2x2 grid):**
     - **Chart 1:** Orphans Growth Trend (Line chart - 12 months)
       - X-axis: Months
       - Y-axis: Count
       - Series: New Orphans, Sponsored Orphans
     - **Chart 2:** Orphan Sponsorship Status (Pie chart)
       - Sponsored vs Unsponsored breakdown
       - Drill-down by charity
     - **Chart 3:** Periodic Reports Status (Bar chart)
       - X-axis: Charities
       - Y-axis: Report count
       - Series: Approved, Rejected, Pending
     - **Chart 4:** Financial Overview (Column chart)
       - X-axis: Months (last 6 months)
       - Y-axis: Amount
       - Series: Budget Allocated, Amount Spent
   
   - **Pending Tasks Section:**
     - List of pending reviews/approvals
     - Click to navigate to detail page
     - Show priority indicators (High/Medium/Low)
   
   - **Recent Activity Section:**
     - Recent system events (last 10)
     - Who did what, when
     - Link to audit log
   
   - **Quick Actions Panel:**
     - "Add New Orphan" button
     - "Add New Family" button
     - "Create Report" button
     - "Add Employee" button
   
   **For Charity Users:**
   - **Top Statistics Cards:**
     - My Orphans (total for charity)
     - Sponsored Orphans (%)
     - My Families
     - Pending Periodic Reports
     - Reports Due This Week
     - Approved Reports (this month)
     - Rejected Reports (this month)
     - Can Add Records: Yes/No
     - Can Update Records: Yes/No
     - Account Locked: Yes/No
   
   - **Charts:**
     - My Orphans Growth (Line chart)
     - Orphan Sponsorship Status (Pie chart)
     - Periodic Reports Trend (Bar chart - Approved/Rejected/Pending)
   
   - **My Pending Tasks:**
     - Periodic reports to submit
     - Overdue reports
     - Reports requiring resubmission
   
   - **Recent Activity:**
     - Recent actions by charity users
   
   - **Quick Actions:**
     - "Add Orphan" (if CanAdd = true)
     - "Add Family" (if CanAdd = true)
     - "Submit Periodic Report"
     - "View My Reports"

   **For Accountant:**
   - **Top Statistics Cards:**
     - Pending Checks
     - Cleared Checks (today)
     - Void Checks (this month)
     - Total Amount Cleared (this month)
     - Checks Due Today
     - Checks Due This Week
   
   - **Charts:**
     - Checks Status Distribution (Pie chart)
     - Monthly Check Volume (Bar chart)
     - Payment Amounts by Month (Line chart)
   
   - **Pending Tasks:**
     - Checks to process
     - Checks to clear
     - Checks to reconcile
   
   - **Quick Actions:**
     - "Issue New Check"
     - "View All Checks"
     - "Reconcile Checks"

4. **Real-Time Updates (SignalR):**
   - Dashboard auto-refreshes statistics every 30 seconds
   - Or user clicks "Refresh" button
   - SignalR pushes updates when:
     - New orphan/family added
     - Periodic report submitted/approved/rejected
     - Check issued/cleared
     - Project status changed
     - Mission completed

5. **Interactive Features:**
   - Click chart data point → drill down to detail page
   - Click statistic card → view filtered list
   - Filter charts by date range
   - Export chart as image
   - Export data as Excel

**Postconditions:**
- User sees personalized dashboard
- Real-time statistics displayed
- Quick access to key metrics and tasks
- User can navigate to detailed views

---

#### Use Case UC-DASH-2: Customize Dashboard Layout

| Field | Value |
|-------|-------|
| **ID** | UC-DASH-2 |
| **Name** | Customize Dashboard Layout |
| **Actor** | All Users |
| **Priority** | Low |
| **Description** | Personalize dashboard widgets, layout, and preferences |

**Preconditions:**
- User is logged in

**Main Flow:**
1. User on Home Dashboard
2. User clicks "Customize Dashboard" button
3. System displays customization panel with:
   - **Available Widgets** (drag-and-drop):
     - Statistics cards (select which to show)
     - Charts (select which to show)
     - Pending Tasks
     - Recent Activity
     - Quick Actions
     - Calendar (upcoming deadlines)
     - Notifications Summary
   - **Layout Options:**
     - 1-column, 2-column, 3-column layout
     - Widget size (small, medium, large)
     - Position (drag to reorder)
   - **Date Range Defaults:**
     - Charts default period (7 days, 30 days, 90 days, this year)
   - **Refresh Interval:**
     - Auto-refresh frequency (30 sec, 1 min, 5 min, manual)
4. User customizes dashboard:
   - Adds/removes widgets
   - Reorders widgets
   - Selects layout
   - Sets preferences
5. User clicks "Save Layout"
6. System saves user's dashboard preferences:
   - Widget configuration (JSON)
   - Layout selection
   - Preferences
7. System applies saved layout immediately
8. System logs customization in audit log

**Postconditions:**
- Dashboard layout personalized
- Preferences saved for future sessions
- User sees customized dashboard on next login

---

#### Use Case UC-DASH-3: Drill Down from Dashboard

| Field | Value |
|-------|-------|
| **ID** | UC-DASH-3 |
| **Name** | Drill Down from Dashboard |
| **Actor** | All Users |
| **Priority** | Medium |
| **Description** | Navigate from dashboard statistics to detailed data views |

**Preconditions:**
- User is on Home Dashboard

**Main Flow:**
1. User views dashboard
2. User clicks on statistic card, chart, or data point
3. **Drill-Down Scenarios:**
   
   **Scenario A: Click "Total Orphans" Card**
   - System navigates to Orphans List page
   - Applies filter: Show all orphans
   - Shows summary statistics at top of page
   
   **Scenario B: Click "Pending Periodic Reports" Card**
   - System navigates to Periodic Reports page
   - Applies filter: Reviewed = false
   - Shows pending reports list
   
   **Scenario C: Click Chart Data Point (e.g., March on Orphans Trend)**
   - System displays drill-down modal/page
   - Shows orphans created in March
   - Lists orphan details in table
   - Provides "Export to Excel" button
   
   **Scenario D: Click Chart Segment (e.g., Sponsored Orphans in Pie Chart)**
   - System filters to sponsored orphans
   - Shows list with details
   - Provides charity breakdown
   
   **Scenario E: Click Pending Task**
   - System navigates to detail page
   - Opens specific entity (orphan, report, check, etc.)
   - Pre-filters to relevant records
   
4. User can navigate back to dashboard via breadcrumb

**Postconditions:**
- User navigated from dashboard to detailed view
- Context filters applied automatically
- Easy navigation back to dashboard

---

#### Use Case UC-DASH-4: Export Dashboard Data

| Field | Value |
|-------|-------|
| **ID** | UC-DASH-4 |
| **Name** | Export Dashboard Data |
| **Actor** | All Users |
| **Priority** | Low |
| **Description** | Export dashboard statistics and charts for reporting |

**Preconditions:**
- User is on Home Dashboard

**Main Flow:**
1. User on Home Dashboard
2. User clicks "Export" button
3. System displays export options:
   - **Export Format:**
     - Excel (.xlsx) - with multiple sheets
     - PDF - with charts and tables
     - Image (.png) - individual charts
   - **Include:**
     - Statistics Cards (checkbox)
     - Charts (checkbox, select which)
     - Pending Tasks (checkbox)
     - Recent Activity (checkbox)
   - **Date Range:**
     - Custom date range for chart data
4. User selects export options
5. User clicks "Generate Export"
6. System generates export file:
   - **For Excel:** 
     - Sheet 1: Summary Statistics
     - Sheet 2-N: Chart data tables
   - **For PDF:**
     - Formatted report with logo
     - All selected widgets
     - Charts as images
     - Tables with data
   - **For Image:**
     - High-resolution chart image
7. System downloads file to user's device
8. System logs export in audit log

**Postconditions:**
- Dashboard data exported
- File available for sharing/presentation
- Audit log contains export record

---

#### Use Case UC-DASH-5: View Dashboard on Mobile

| Field | Value |
|-------|-------|
| **ID** | UC-DASH-5 |
| **Name** | View Dashboard on Mobile |
| **Actor** | All Users |
| **Priority** | Medium |
| **Description** | Access responsive dashboard on mobile devices |

**Preconditions:**
- User has mobile device or tablet
- User is logged in

**Main Flow:**
1. User accesses system via mobile browser
2. System detects mobile device
3. System displays mobile-optimized dashboard:
   - **Single Column Layout** (vertical scroll)
   - **Simplified Statistics Cards** (2 columns)
   - **Touch-Optimized Charts** (larger touch targets)
   - **Collapsible Sections** (tap to expand/collapse)
   - **Hamburger Menu** for navigation
   - **Pull-to-Refresh** gesture
   - **Swipe Navigation** between dashboard sections
4. User can:
   - Tap cards to drill down
   - Tap chart to see details
   - Use gestures for navigation
   - Access all dashboard features

**Postconditions:**
- Mobile-friendly dashboard displayed
- Touch-optimized interface
- Full functionality available on mobile

---

## Dashboard Widget Library

### **Statistics Cards Widgets:**
```javascript
// Statistics Card Component
{
  "type": "stat-card",
  "title": "Total Orphans",
  "value": 1234,
  "trend": {
    "direction": "up", // or "down", "neutral"
    "percentage": 12.5,
    "period": "vs last month"
  },
  "icon": "users",
  "color": "blue",
  "actionUrl": "/Orphans",
  "refreshInterval": 30000 // 30 seconds
}
```

### **Chart Widgets:**
```javascript
// Line Chart Widget
{
  "type": "chart-line",
  "title": "Orphans Growth Trend",
  "dataSource": "/api/dashboard/orphans-trend",
  "xAxis": "Month",
  "yAxis": "Count",
  "series": [
    { "name": "New Orphans", "color": "#3b82f6" },
    { "name": "Sponsored Orphans", "color": "#10b981" }
  ],
  "height": 300,
  "responsive": true,
  "exportable": true
}

// Pie Chart Widget
{
  "type": "chart-pie",
  "title": "Sponsorship Status",
  "dataSource": "/api/dashboard/sponsorship-status",
  "data": [
    { "label": "Sponsored", "value": 850, "color": "#10b981" },
    { "label": "Unsponsored", "value": 384, "color": "#ef4444" }
  ],
  "height": 300,
  "showLegend": true,
  "exportable": true
}
```

### **Pending Tasks Widget:**
```javascript
// Pending Tasks Widget
{
  "type": "pending-tasks",
  "title": "Pending Tasks",
  "dataSource": "/api/dashboard/pending-tasks",
  "maxItems": 5,
  "showPriority": true,
  "groupBy": "taskType",
  "actionUrl": "/Tasks/{taskId}"
}
```

### **Recent Activity Widget:**
```javascript
// Recent Activity Widget
{
  "type": "recent-activity",
  "title": "Recent Activity",
  "dataSource": "/api/dashboard/recent-activity",
  "maxItems": 10,
  "showUser": true,
  "showTimestamp": true,
  "autoRefresh": true
}
```

---

## API Endpoints for Dashboard

```csharp
// GET: /api/dashboard/statistics
// Get dashboard statistics for current user
[HttpGet("statistics")]
[Authorize]
public async Task<ActionResult<DashboardStatisticsDto>> GetStatistics()
{
    var userId = GetCurrentUserId();
    var userRole = GetUserRole();
    var charityId = GetUserCharityId();
    
    DashboardStatisticsDto statistics;
    
    if (userRole == "SuperAdmin" || userRole == "Admin")
    {
        // System-wide statistics
        statistics = await _dashboardService.GetSystemStatistics();
    }
    else if (userRole == "Charity")
    {
        // Charity-specific statistics
        statistics = await _dashboardService.GetCharityStatistics(charityId);
    }
    else if (userRole == "Accountant")
    {
        // Financial statistics
        statistics = await _dashboardService.GetFinancialStatistics();
    }
    else
    {
        // Employee statistics
        statistics = await _dashboardService.GetEmployeeStatistics(userId);
    }
    
    return Ok(statistics);
}

// GET: /api/dashboard/chart-data/{chartType}
// Get chart data
[HttpGet("chart-data/{chartType}")]
[Authorize]
public async Task<ActionResult<ChartDataDto>> GetChartData(
    string chartType,
    DateTime? startDate = null,
    DateTime? endDate = null)
{
    var userId = GetCurrentUserId();
    var charityId = GetUserCharityId();
    
    var data = await _dashboardService.GetChartData(
        chartType, 
        userId, 
        charityId, 
        startDate,
        endDate);
    
    return Ok(data);
}

// GET: /api/dashboard/pending-tasks
// Get pending tasks for current user
[HttpGet("pending-tasks")]
[Authorize]
public async Task<ActionResult<List<PendingTaskDto>>> GetPendingTasks()
{
    var userId = GetCurrentUserId();
    var userRole = GetUserRole();
    
    var tasks = await _dashboardService.GetPendingTasks(userId, userRole);
    
    return Ok(tasks);
}

// GET: /api/dashboard/recent-activity
// Get recent activity
[HttpGet("recent-activity")]
[Authorize]
public async Task<ActionResult<List<ActivityDto>>> GetRecentActivity(int count = 10)
{
    var userId = GetCurrentUserId();
    var userRole = GetUserRole();
    var charityId = GetUserCharityId();
    
    var activities = await _dashboardService.GetRecentActivity(
        userId, 
        userRole, 
        charityId, 
        count);
    
    return Ok(activities);
}

// GET: /api/dashboard/orphans-trend
// Get orphans trend data
[HttpGet("orphans-trend")]
[Authorize]
public async Task<ActionResult<List<TrendDataDto>>> GetOrphansTrend(
    DateTime startDate,
    DateTime endDate)
{
    var charityId = GetUserCharityId();
    
    // Use stored procedure
    var trendData = await _context.Database
        .SqlQueryRaw<TrendDataDto>(
            "EXEC sp_Dashboard_OrphansTrend @StartDate, @EndDate, @CharityId",
            new SqlParameter("@StartDate", startDate),
            new SqlParameter("@EndDate", endDate),
            new SqlParameter("@CharityId", (object?)charityId ?? DBNull.Value)
        )
        .ToListAsync();
    
    return Ok(trendData);
}
```

---

## Real-Time Dashboard Updates (SignalR)

```csharp
// Dashboard Hub for Real-Time Updates
[Authorize]
public class DashboardHub : Hub
{
    private readonly ICacheService _cache;

    public DashboardHub(ICacheService cache)
    {
        _cache = cache;
    }

    // Join dashboard group for role-based updates
    public async Task JoinDashboardGroup()
    {
        var userRole = GetUserRole();
        await Groups.AddToGroupAsync(Context.ConnectionId, $"Dashboard_{userRole}");
    }

    // Broadcast statistic update
    public async Task BroadcastStatisticUpdate(string statisticName, object newValue)
    {
        await Clients.Group("Dashboard_All").SendAsync("StatisticUpdated", new {
            name = statisticName,
            value = newValue,
            timestamp = DateTime.UtcNow
        });
    }

    // Broadcast chart data update
    public async Task BroadcastChartDataUpdate(string chartType, object newData)
    {
        await Clients.Group("Dashboard_All").SendAsync("ChartDataUpdated", new {
            chartType = chartType,
            data = newData,
            timestamp = DateTime.UtcNow
        });
    }
}

// Service to trigger dashboard updates
public class DashboardUpdateService
{
    private readonly IHubContext<DashboardHub> _hubContext;

    public DashboardUpdateService(IHubContext<DashboardHub> hubContext)
    {
        _hubContext = hubContext;
    }

    // Call this when an orphan is added
    public async Task NotifyOrphanAdded(Guid charityId)
    {
        await _hubContext.Clients.Group("Dashboard_SuperAdmin")
            .SendAsync("StatisticUpdated", new { name = "TotalOrphans", value = GetTotalOrphans() });
        
        await _hubContext.Clients.Group($"Dashboard_Charity_{charityId}")
            .SendAsync("StatisticUpdated", new { name = "MyOrphans", value = GetCharityOrphans(charityId) });
    }

    // Call this when a periodic report is submitted
    public async Task NotifyReportSubmitted()
    {
        await _hubContext.Clients.Group("Dashboard_SuperAdmin")
            .SendAsync("StatisticUpdated", new { name = "PendingReports", value = GetPendingReports() });
        
        await _hubContext.Clients.Group("Dashboard_Admin")
            .SendAsync("PendingTaskAdded", new { 
                type = "PeriodicReportReview",
                message = "New periodic report requires review"
            });
    }

    // Call this when a check is issued
    public async Task NotifyCheckIssued()
    {
        await _hubContext.Clients.Group("Dashboard_Accountant")
            .SendAsync("StatisticUpdated", new { name = "PendingChecks", value = GetPendingChecks() });
    }
}
```

---

## Client-Side Dashboard (JavaScript/TypeScript)

```javascript
// Dashboard Service
class DashboardService {
    private hubConnection: signalR.HubConnection;
    
    constructor() {
        this.hubConnection = new signalR.HubConnectionBuilder()
            .withUrl('/dashboardHub')
            .withAutomaticReconnect()
            .build();
        
        this.setupEventListeners();
        this.startConnection();
    }
    
    private async startConnection() {
        try {
            await this.hubConnection.start();
            console.log('Dashboard Hub Connected');
            
            // Join dashboard group
            const userRole = getUserRole();
            await this.hubConnection.invoke('JoinDashboardGroup');
        } catch (err) {
            console.error('Dashboard Hub Error:', err);
        }
    }
    
    private setupEventListeners() {
        // Listen for statistic updates
        this.hubConnection.on('StatisticUpdated', (data) => {
            this.updateStatisticCard(data.name, data.value);
        });
        
        // Listen for chart updates
        this.hubConnection.on('ChartDataUpdated', (data) => {
            this.updateChart(data.chartType, data.data);
        });
        
        // Listen for pending tasks
        this.hubConnection.on('PendingTaskAdded', (task) => {
            this.addPendingTask(task);
        });
    }
    
    private updateStatisticCard(statisticName: string, newValue: any) {
        const card = document.querySelector(`[data-stat="${statisticName}"]`);
        if (card) {
            const valueElement = card.querySelector('.stat-value');
            valueElement.textContent = this.formatValue(newValue);
            
            // Flash animation
            card.classList.add('stat-updated');
            setTimeout(() => card.classList.remove('stat-updated'), 1000);
        }
    }
    
    private updateChart(chartType: string, newData: any) {
        // Update chart using Chart.js or similar library
        const chart = Chart.getChart(`chart-${chartType}`);
        if (chart) {
            chart.data = newData;
            chart.update('active'); // Smooth animation
        }
    }
    
    private addPendingTask(task: any) {
        const tasksList = document.getElementById('pending-tasks-list');
        const taskElement = this.createTaskElement(task);
        tasksList.insertBefore(taskElement, tasksList.firstChild);
        
        // Show notification
        showNotification('New Task', task.message, 'info');
    }
    
    private formatValue(value: any): string {
        // Format numbers, dates, etc.
        if (typeof value === 'number') {
            return value.toLocaleString();
        }
        return value;
    }
}

// Initialize dashboard
const dashboardService = new DashboardService();

// Load initial dashboard data
async function loadDashboard() {
    const response = await fetch('/api/dashboard/statistics');
    const statistics = await response.json();
    
    // Render statistics cards
    statistics.cards.forEach(card => {
        renderStatisticCard(card);
    });
    
    // Render charts
    statistics.charts.forEach(chart => {
        renderChart(chart);
    });
    
    // Load pending tasks
    loadPendingTasks();
    
    // Load recent activity
    loadRecentActivity();
}

// Auto-refresh dashboard
setInterval(() => {
    loadDashboard();
}, 30000); // Every 30 seconds

// Manual refresh
document.getElementById('refresh-dashboard').addEventListener('click', () => {
    loadDashboard();
});
```

---

## Dashboard Configuration & Personalization

```javascript
// Dashboard Layout Schema
interface DashboardLayout {
    userId: string;
    layout: '1-column' | '2-column' | '3-column';
    widgets: Widget[];
    preferences: DashboardPreferences;
    lastModified: Date;
}

interface Widget {
    id: string;
    type: 'stat-card' | 'chart-line' | 'chart-pie' | 'chart-bar' | 'pending-tasks' | 'recent-activity';
    position: { row: number; column: number };
    size: 'small' | 'medium' | 'large';
    config: any; // Widget-specific configuration
    visible: boolean;
}

interface DashboardPreferences {
    autoRefresh: boolean;
    refreshInterval: number; // seconds
    defaultDateRange: string;
    theme: 'light' | 'dark';
    compactMode: boolean;
}

// Save dashboard layout
async function saveDashboardLayout(layout: DashboardLayout) {
    await fetch('/api/dashboard/layout', {
        method: 'PUT',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(layout)
    });
}

// Load dashboard layout
async function loadDashboardLayout(): Promise<DashboardLayout> {
    const response = await fetch('/api/dashboard/layout');
    return await response.json();
}
```

---

## Performance Optimization

1. **Caching Strategy:**
   - Cache statistics for 30 seconds
   - Cache chart data for 5 minutes
   - Invalidate cache on data changes
   - Use Redis for distributed caching

2. **Database Optimization:**
   - Use indexed views for statistics
   - Pre-aggregate data in nightly jobs
   - Use stored procedures for complex queries
   - Implement query result caching

3. **Lazy Loading:**
   - Load charts on-demand (when scrolled into view)
   - Load pending tasks after initial page load
   - Use pagination for large lists

4. **CDN for Static Assets:**
   - Serve charting libraries from CDN
   - Cache dashboard JavaScript/CSS

---

## Accessibility & Internationalization

1. **Accessibility:**
   - ARIA labels for all widgets
   - Keyboard navigation support
   - Screen reader compatible
   - High contrast mode support
   - Font size adjustment

2. **Internationalization:**
   - Multi-language support (Arabic, English)
   - RTL layout support for Arabic
   - Date/time localization
   - Number formatting by locale
   - Currency formatting by locale
