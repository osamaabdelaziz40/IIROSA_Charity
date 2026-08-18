### 4.15 Module: Dynamic Page Management

**Module Owner:** Super Admin  
**Purpose:** Control which pages each role can see  
**Dependencies:** Pages, Roles  

**Important Note:** Only Super Admin can access this module. This controls page visibility for all other roles.

#### Use Case UC-15.1: Create Page Definition

| Field | Value |
|-------|-------|
| **ID** | UC-15.1 |
| **Name** | Create Page Definition |
| **Actor** | Super Admin |
| **Priority** | High |
| **Description** | Define new page with name, route, and default visibility |

**Preconditions:**
- Super Admin is logged in

**Main Flow:**
1. Super Admin navigates to Dynamic Page Management page
2. System displays list of pages with "Add New Page" button
3. Super Admin clicks "Add New Page"
4. System displays page creation form:
   - **Basic Information:**
     - Page Name (required, e.g., "Families", "Orphan Payments")
     - Page Name Arabic (required)
     - Route/Path (required, e.g., "/families", "/orphan-payments")
     - Icon (dropdown or icon class)
     - Description
   - **Page Type:**
     - Page Type (dropdown: List, Details, Dashboard, Report, Settings)
     - Parent Page (dropdown, if this is a sub-page)
   - **Default Settings:**
     - Is Active (checkbox, default: true)
     - Display Order (number)
     - Requires Authentication (checkbox, default: true)
     - Show in Navigation (checkbox, default: true)
   - **Module:**
     - Module Name (dropdown: Families, Projects, Missions, etc.)
   - **Permissions:**
     - Required Permissions (multi-select from available permissions)
5. Super Admin fills in required fields
6. System validates route uniqueness
7. System creates page definition record
8. System logs creation in audit log
9. System displays success message
10. Page appears in page list
11. Page available for role assignment

**Postconditions:**
- Page definition created
- Ready for role visibility assignment

---

#### Use Case UC-15.2: Assign Page to Role

| Field | Value |
|-------|-------|
| **ID** | UC-15.2 |
| **Name** | Assign Page to Role |
| **Actor** | Super Admin |
| **Priority** | High |
| **Description** | Configure which roles can access specific page |

**Preconditions:**
- Super Admin is logged in
- Page exists
- Roles exist

**Main Flow:**
1. Super Admin navigates to Dynamic Page Management page
2. System displays list of pages
3. Super Admin selects page
4. System displays page details with "Role Assignments" section
5. System displays list of roles with checkboxes:
   - Super Admin
   - Admin
   - Charity
   - Accountant
   - Financial Officer
   - Employee
6. Currently assigned roles are checked
7. Super Admin checks/unchecks roles to grant/revoke access
8. At least one role must be assigned
9. Super Admin clicks "Save Assignments"
10. System creates or updates RolePageAssignment records
11. System logs assignments in audit log
12. System displays success message
13. Page visibility updated for affected roles
14. Users with assigned roles see page in navigation
15. Users without assigned roles no longer see page

**Postconditions:**
- Page assigned to roles
- Navigation menu updated
- Role-based access enforced

---

#### Use Case UC-15.3: Remove Page from Role

| Field | Value |
|-------|-------|
| **ID** | UC-15.3 |
| **Name** | Remove Page from Role |
| **Actor** | Super Admin |
| **Priority** | Medium |
| **Description** | Remove page access from specific role |

**Preconditions:**
- Super Admin is logged in
- Page assigned to role

**Main Flow:**
1. Super Admin navigates to Dynamic Page Management page
2. System displays list of pages
3. Super Admin selects page
4. System displays page details with "Role Assignments" section
5. System displays roles with checkboxes
6. Super Admin unchecks role to remove access
7. Super Admin clicks "Save Assignments"
8. System deletes RolePageAssignment record for role-page combination
9. System logs removal in audit log
10. System displays success message
11. Role no longer has access to page
12. Page removed from navigation for users of that role

**Postconditions:**
- Page removed from role
- Access revoked
- Navigation updated

---

#### Use Case UC-15.4: Set Page Display Order

| Field | Value |
|-------|-------|
| **ID** | UC-15.4 |
| **Name** | Set Page Display Order |
| **Actor** | Super Admin |
| **Priority** | Medium |
| **Description** | Define order of pages in navigation menu per role |

**Preconditions:**
- Super Admin is logged in
- Pages exist

**Main Flow:**
1. Super Admin navigates to Dynamic Page Management page
2. System provides "Order Pages" button
3. Super Admin clicks "Order Pages"
4. System displays page ordering interface:
   - Select Role (dropdown)
   - Drag-and-drop list of pages for that role
   - OR numbered input fields for order
5. Super Admin selects role
6. System displays pages assigned to that role
7. Super Admin adjusts page order:
   - **Drag-and-drop:** Drag pages to desired position
   - **Manual:** Enter order numbers
8. Super Admin clicks "Save Order"
9. System updates DisplayOrder for RolePageAssignment records
10. System logs order changes in audit log
11. System displays success message
12. Navigation menu reflects new order for selected role

**Postconditions:**
- Page order set per role
- Navigation menu updated

---

#### Use Case UC-15.5: Enable/Disable Page for Role

| Field | Value |
|-------|-------|
| **ID** | UC-15.5 |
| **Name** | Enable/Disable Page for Role |
| **Actor** | Super Admin |
| **Priority** | Medium |
| **Description** | Toggle page visibility for specific role without deleting assignment |

**Preconditions:**
- Super Admin is logged in
- Page assigned to role

**Main Flow:**
1. Super Admin navigates to Dynamic Page Management page
2. System displays list of pages
3. Super Admin selects page
4. System displays page details with "Role Assignments" section
5. For each assigned role, system shows:
   - Role Name
   - Is Enabled toggle (On/Off)
6. Super Admin toggles Is Enabled for desired role(s)
7. Super Admin clicks "Save"
8. System updates RolePageAssignment.IsEnabled flag
9. System log changes in audit log
10. System displays success message
11. **If disabled:** Page hidden from navigation for that role
12. **If enabled:** Page visible in navigation for that role

**Postconditions:**
- Page visibility toggled
- Assignment preserved
- Easy to re-enable

---

#### Use Case UC-15.6: View All Page Definitions

| Field | Value |
|-------|-------|
| **ID** | UC-15.6 |
| **Name** | View All Page Definitions |
| **Actor** | Super Admin |
| **Priority** | Low |
| **Description** | View all pages with role assignments |

**Preconditions:**
- Super Admin is logged in

**Main Flow:**
1. Super Admin navigates to Dynamic Page Management page
2. System displays list of pages in grid with columns:
   - Page Name
   - Route
   - Module
   - Page Type
   - Is Active
   - Display Order
   - Assigned Roles (comma-separated list)
   - Actions (Edit, Assign Roles, Delete)
3. System provides search by page name or route
4. System provides filter by:
   - Module
   - Page Type
   - Is Active
5. System provides sorting by any column
6. Super Admin can click page to view details
7. System shows total page count

**Postconditions:**
- All pages visible
- Role assignments visible
- Easy management

---

#### Use Case UC-15.7: View Pages by Role

| Field | Value |
|-------|-------|
| **ID** | UC-15.7 |
| **Name** | View Pages by Role |
| **Actor** | Super Admin |
| **Priority** | Low |
| **Description** | View all pages accessible to specific role |

**Preconditions:**
- Super Admin is logged in

**Main Flow:**
1. Super Admin navigates to Dynamic Page Management page
2. System provides "View by Role" button
3. Super Admin clicks "View by Role"
4. System displays role selection dropdown
5. Super Admin selects role
6. System displays pages assigned to that role:
   - Page Name
   - Route
   - Display Order
   - Is Enabled
   - Actions
7. System shows pages in navigation order
8. Super Admin can see what each role can access
9. Super Admin can adjust assignments from this view

**Postconditions:**
- Pages visible by role
- Easy to verify permissions

---

#### Use Case UC-15.8: Update Page Details

| Field | Value |
|-------|-------|
| **ID** | UC-15.8 |
| **Name** | Update Page Details |
| **Actor** | Super Admin |
| **Priority** | Medium |
| **Description** | Modify page name, route, or description |

**Preconditions:**
- Super Admin is logged in
- Page exists

**Main Flow:**
1. Super Admin navigates to Dynamic Page Management page
2. System displays list of pages
3. Super Admin selects page to edit
4. System displays page edit form with current data
5. Super Admin modifies desired fields:
   - Page Name
   - Page Name Arabic
   - Route
   - Icon
   - Description
   - Display Order
   - Is Active
   - Show in Navigation
6. Super Admin clicks "Save"
7. System validates route uniqueness (if route changed)
8. System updates page record
9. System log changes in audit log
10. System displays success message
11. Page details updated
12. Navigation menu reflects changes

**Postconditions:**
- Page details updated
- System reflects changes

---

#### Use Case UC-15.9: Set Page as Dashboard

| Field | Value |
|-------|-------|
| **ID** | UC-15.9 |
| **Name** | Set Page as Dashboard |
| **Actor** | Super Admin |
| **Priority** | Medium |
| **Description** | Configure default landing page for each role |

**Preconditions:**
- Super Admin is logged in
- Pages exist

**Main Flow:**
1. Super Admin navigates to Dynamic Page Management page
2. System provides "Set Dashboards" button
3. Super Admin clicks "Set Dashboards"
4. System displays dashboard configuration interface:
   - For each role:
     - Role Name
     - Dashboard Page (dropdown of available pages)
5. Super Admin selects dashboard page for each role:
   - Super Admin → "Admin Dashboard"
   - Admin → "Admin Dashboard"
   - Charity → "Charity Dashboard"
   - Accountant → "Financial Dashboard"
   - Financial Officer → "Reports Dashboard"
   - Employee → "Employee Dashboard"
6. Super Admin clicks "Save Dashboards"
7. System updates user preferences or role settings
8. System logs dashboard assignments in audit log
9. System displays success message
10. Users land on assigned dashboard when logging in

**Postconditions:**
- Dashboards set per role
- Default landing page configured

---

#### Use Case UC-15.10: Preview Page Navigation

| Field | Value |
|-------|-------|
| **ID** | UC-15.10 |
| **Name** | Preview Page Navigation |
| **Actor** | Super Admin |
| **Priority** | Low |
| **Description** | Preview navigation menu as seen by specific role |

**Preconditions:**
- Super Admin is logged in

**Main Flow:**
1. Super Admin navigates to Dynamic Page Management page
2. System provides "Preview Navigation" button
3. Super Admin clicks "Preview Navigation"
4. System displays role selection dropdown
5. Super Admin selects role to preview
6. System renders preview of navigation menu:
   - Shows menu items as they appear to that role
   - Shows page order
   - Shows icons
   - Shows enabled/disabled pages
   - Interactive preview (can click items)
7. Super Admin can verify:
   - Correct pages visible
   - Correct order
   - Correct icons and labels
   - No unauthorized pages
8. Super Admin can switch between roles to compare
9. Super Admin can adjust assignments based on preview

**Postconditions:**
- Navigation previewed
- Easy to verify setup
- Adjustments made as needed

---


