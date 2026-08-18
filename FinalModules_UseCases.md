# Final Use Cases - Modules 4.14 through 4.18

Final modules of the complete use case specification.

---

### 4.14 Module: Lookup Management

**Module Owner:** Super Admin  
**Purpose:** Manage all lookup tables and reference data  
**Dependencies:** All modules that use lookups  

**Important Note:** Only Super Admin can access this module. All other roles cannot manage lookups.

#### Use Case UC-14.1: Create Lookup Item

| Field | Value |
|-------|-------|
| **ID** | UC-14.1 |
| **Name** | Create Lookup Item |
| **Actor** | Super Admin |
| **Priority** | High |
| **Description** | Add new item to any lookup table (Centers, Regions, Countries, Departments, etc.) |

**Preconditions:**
- Super Admin is logged in
- Lookup table exists

**Main Flow:**
1. Super Admin navigates to Lookup Management page
2. System displays list of lookup tables:
   - Centers
   - Regions
   - Countries
   - Departments
   - Mission Types
   - Project Types
   - Banks
   - NGO Types
   - Cheque Beneficiaries
   - Etc.
3. Super Admin selects lookup table
4. System displays current items with "Add New Item" button
5. Super Admin clicks "Add New Item"
6. System displays lookup item creation form (fields vary by lookup type):
   - **Common Fields:**
     - Name (required, in Arabic and English)
     - Order (number, for display sorting)
     - IsActive (checkbox, default: true)
   - **For Countries:**
     - Country Code (ISO code)
     - Dialing Code
     - Currency
   - **For Regions:**
     - FK_CountryId (dropdown)
     - Region Code
   - **For Centers:**
     - FK_RegionId (dropdown)
     - FK_CountryId (auto-populated)
     - Center Code
   - **For Departments:**
     - Department Code
     - Description
   - **For Banks:**
     - Bank Code
     - Swift Code
     - Address
   - **For NGOs/Charities Types:**
     - Type Code
     - Description
7. Super Admin fills in required fields
8. System validates data
9. System creates lookup item record
10. System logs creation in audit log
11. System displays success message
12. New item visible in lookup list

**Postconditions:**
- Lookup item created
- Available in dropdowns throughout system
- Audit log contains creation record

---

#### Use Case UC-14.2: Update Lookup Item

| Field | Value |
|-------|-------|
| **ID** | UC-14.2 |
| **Name** | Update Lookup Item |
| **Actor** | Super Admin |
| **Priority** | Medium |
| **Description** | Modify existing lookup item details |

**Preconditions:**
- Super Admin is logged in
- Lookup item exists

**Main Flow:**
1. Super Admin navigates to Lookup Management page
2. System displays list of lookup tables
3. Super Admin selects lookup table
4. System displays list of items
5. Super Admin selects item to edit
6. System displays edit form with current data
7. Super Admin modifies desired fields
8. Super Admin clicks "Save"
9. System validates data
10. System updates lookup item
11. System logs changes in audit log
12. System displays success message
13. Updated item visible in list and system dropdowns

**Business Rules:**
- Cannot change ID field
- Changes affect all records using this lookup

**Postconditions:**
- Lookup item updated
- Changes reflected system-wide

---

#### Use Case UC-14.3: Deactivate Lookup Item

| Field | Value |
|-------|-------|
| **ID** | UC-14.3 |
| **Name** | Deactivate Lookup Item |
| **Actor** | Super Admin |
| **Priority** | Medium |
| **Description** | Deactivate lookup item preventing future use |

**Preconditions:**
- Super Admin is logged in
- Lookup item exists
- Item is active

**Main Flow:**
1. Super Admin navigates to Lookup Management page
2. System displays list of lookup tables
3. Super Admin selects lookup table
4. System displays list of items
5. Super Admin selects item to deactivate
6. System displays item details with "Deactivate" button
7. Super Admin clicks "Deactivate"
8. System displays confirmation: "Deactivating will prevent this item from being selected in new records. Existing records will keep this value. Continue?"
9. Super Admin confirms
10. System sets item.IsActive = false
11. System logs deactivation in audit log
12. System displays success message
13. Item shows as "Inactive" in list
14. Item no longer appears in dropdowns for new records

**Alternative Flows:**
- **9a. User cancels:** System returns without changes

**Postconditions:**
- Lookup item deactivated
- Not available for new selections
- Existing records preserved

---

#### Use Case UC-14.4: Set Lookup Item Order

| Field | Value |
|-------|-------|
| **ID** | UC-14.4 |
| **Name** | Set Lookup Item Order |
| **Actor** | Super Admin |
| **Priority** | Low |
| **Description** | Define display order for lookup items (Order field) |

**Preconditions:**
- Super Admin is logged in
- Lookup items exist

**Main Flow:**
1. Super Admin navigates to Lookup Management page
2. System displays list of lookup tables
3. Super Admin selects lookup table
4. System displays list of items with current Order values
5. System provides drag-and-drop interface for reordering
6. **OR** System provides numbered input fields for Order
7. Super Admin adjusts order:
   - **Drag-and-drop:** Drag items to desired position
   - **Manual:** Enter order numbers
8. Super Admin clicks "Save Order"
9. System updates Order field for all items
10. System logs reorder in audit log
11. System displays success message
12. Items appear in new order in system dropdowns

**Postconditions:**
- Lookup item order updated
- Dropdowns reflect new order

---

#### Use Case UC-14.5: View All Lookup Tables

| Field | Value |
|-------|-------|
| **ID** | UC-14.5 |
| **Name** | View All Lookup Tables |
| **Actor** | Super Admin |
| **Priority** | Low |
| **Description** | View all lookup tables with their items |

**Preconditions:**
- Super Admin is logged in

**Main Flow:**
1. Super Admin navigates to Lookup Management page
2. System displays all lookup tables in grid or card view:
   - Table Name
   - Item Count
   - Active Items
   - Inactive Items
   - Last Modified
   - Actions (View Items, Export)
3. System provides search by table name
4. System provides filter by:
   - Item Count range
   - Modified Date range
5. Super Admin can click table to view items
6. System provides "Refresh" button to reload data

**Postconditions:**
- All lookup tables visible
- Easy navigation to manage items

---

#### Use Case UC-14.6: Manage Center Lookups

| Field | Value |
|-------|-------|
| **ID** | UC-14.6 |
| **Name** | Manage Center Lookups |
| **Actor** | Super Admin |
| **Priority** | High |
| **Description** | Create, update, deactivate centers linked to regions |

**Preconditions:**
- Super Admin is logged in
- Regions exist

**Main Flow:**
1. Super Admin navigates to Lookup Management page
2. System displays lookup tables
3. Super Admin selects "Centers"
4. System displays centers list with filters:
   - By Region (dropdown)
   - By Country (dropdown)
   - By Status (Active/Inactive)
5. Super Admin can:
   - Add new center
   - Edit center
   - Deactivate center
   - Set order
6. **When creating/editing center:**
   - Center Name (Arabic and English)
   - Center Code
   - FK_RegionId (required dropdown)
   - FK_CountryId (auto-filled from region)
   - Order (number)
   - IsActive (checkbox)
7. System validates region is selected
8. System saves center with region linkage
9. System logs changes in audit log

**Postconditions:**
- Centers managed with region linkage
- Used in geographic hierarchy

---

#### Use Case UC-14.7: Manage Region Lookups

| Field | Value |
|-------|-------|
| **ID** | UC-14.7 |
| **Name** | Manage Region Lookups |
| **Actor** | Super Admin |
| **Priority** | High |
| **Description** | Create, update, deactivate regions linked to countries |

**Preconditions:**
- Super Admin is logged in
- Countries exist

**Main Flow:**
1. Super Admin navigates to Lookup Management page
2. System displays lookup tables
3. Super Admin selects "Regions"
4. System displays regions list with filters:
   - By Country (dropdown)
   - By Status (Active/Inactive)
5. Super Admin can:
   - Add new region
   - Edit region
   - Deactivate region
   - Set order
6. **When creating/editing region:**
   - Region Name (Arabic and English)
   - Region Code
   - FK_CountryId (required dropdown)
   - Order (number)
   - IsActive (checkbox)
7. System validates country is selected
8. System saves region with country linkage
9. System logs changes in audit log

**Postconditions:**
- Regions managed with country linkage
- Used in geographic hierarchy

---

#### Use Case UC-14.8: Manage Country Lookups

| Field | Value |
|-------|-------|
| **ID** | UC-14.8 |
| **Name** | Manage Country Lookups |
| **Actor** | Super Admin |
| **Priority** | High |
| **Description** | Create, update, deactivate countries |

**Preconditions:**
- Super Admin is logged in

**Main Flow:**
1. Super Admin navigates to Lookup Management page
2. System displays lookup tables
3. Super Admin selects "Countries"
4. System displays countries list
5. Super Admin can:
   - Add new country
   - Edit country
   - Deactivate country
   - Set order
6. **When creating/editing country:**
   - Country Name (Arabic and English)
   - Country Code (ISO code)
   - Dialing Code
   - Currency (dropdown)
   - Order (number)
   - IsActive (checkbox)
7. System validates country code uniqueness
8. System saves country
9. System logs changes in audit log

**Postconditions:**
- Countries managed
- Used in geographic hierarchy

---

#### Use Case UC-14.9: Manage Department Lookups

| Field | Value |
|-------|-------|
| **ID** | UC-14.9 |
| **Name** | Manage Department Lookups |
| **Actor** | Super Admin |
| **Priority** | Medium |
| **Description** | Create, update, deactivate departments for correspondence routing |

**Preconditions:**
- Super Admin is logged in

**Main Flow:**
1. Super Admin navigates to Lookup Management page
2. System displays lookup tables
3. Super Admin selects "Departments"
4. System displays departments list
5. Super Admin can:
   - Add new department
   - Edit department
   - Deactivate department
   - Set order
6. **When creating/editing department:**
   - Department Name (Arabic and English)
   - Department Code
   - Description
   - Order (number)
   - IsActive (checkbox)
7. System saves department
8. System logs changes in audit log

**Postconditions:**
- Departments managed
- Used in correspondence routing

---

#### Use Case UC-14.10: Manage Mission Type Lookups

| Field | Value |
|-------|-------|
| **ID** | UC-14.10 |
| **Name** | Manage Mission Type Lookups |
| **Actor** | Super Admin |
| **Priority** | Medium |
| **Description** | Create, update, deactivate mission type categories |

**Preconditions:**
- Super Admin is logged in

**Main Flow:**
1. Super Admin navigates to Lookup Management page
2. System displays lookup tables
3. Super Admin selects "Mission Types"
4. System displays mission types list
5. Super Admin can:
   - Add new mission type
   - Edit mission type
   - Deactivate mission type
   - Set order
6. **When creating/editing mission type:**
   - Type Name (Arabic and English)
   - Type Code
   - Description
   - Order (number)
   - IsActive (checkbox)
7. System saves mission type
8. System logs changes in audit log

**Postconditions:**
- Mission types managed
- Used in mission classification

---

#### Use Case UC-14.11: Manage Project Type Lookups

| Field | Value |
|-------|-------|
| **ID** | UC-14.11 |
| **Name** | Manage Project Type Lookups |
| **Actor** | Super Admin |
| **Priority** | Medium |
| **Description** | Create, update, deactivate project type categories |

**Preconditions:**
- Super Admin is logged in

**Main Flow:**
1. Super Admin navigates to Lookup Management page
2. System displays lookup tables
3. Super Admin selects "Project Types"
4. System displays project types list
5. Super Admin can:
   - Add new project type
   - Edit project type
   - Deactivate project type
   - Set order
6. **When creating/editing project type:**
   - Type Name (Arabic and English)
   - Type Code
   - Description
   - Order (number)
   - IsActive (checkbox)
7. System saves project type
8. System logs changes in audit log

**Postconditions:**
- Project types managed
- Used in project classification

---

#### Use Case UC-14.12: Manage Bank Lookups

| Field | Value |
|-------|-------|
| **ID** | UC-14.12 |
| **Name** | Manage Bank Lookups |
| **Actor** | Super Admin |
| **Priority** | Medium |
| **Description** | Create, update, deactivate bank information |

**Preconditions:**
- Super Admin is logged in

**Main Flow:**
1. Super Admin navigates to Lookup Management page
2. System displays lookup tables
3. Super Admin selects "Banks"
4. System displays banks list
5. Super Admin can:
   - Add new bank
   - Edit bank
   - Deactivate bank
   - Set order
6. **When creating/editing bank:**
   - Bank Name (Arabic and English)
   - Bank Code
   - Swift Code
   - Address
   - Phone
   - Order (number)
   - IsActive (checkbox)
7. System saves bank
8. System logs changes in audit log

**Postconditions:**
- Banks managed
- Used in financial operations

---

#### Use Case UC-14.13: Manage NGO Type Lookups

| Field | Value |
|-------|-------|
| **ID** | UC-14.13 |
| **Name** | Manage NGO Type Lookups |
| **Actor** | Super Admin |
| **Priority** | Medium |
| **Description** | Create, update, deactivate charity type categories |

**Preconditions:**
- Super Admin is logged in

**Main Flow:**
1. Super Admin navigates to Lookup Management page
2. System displays lookup tables
3. Super Admin selects "NGO Types" or "Charity Types"
4. System displays NGO types list
5. Super Admin can:
   - Add new NGO type
   - Edit NGO type
   - Deactivate NGO type
   - Set order
6. **When creating/editing NGO type:**
   - Type Name (Arabic and English)
   - Type Code
   - Description
   - Order (number)
   - IsActive (checkbox)
7. System saves NGO type
8. System logs changes in audit log

**Postconditions:**
- NGO types managed
- Used in charity classification

---

#### Use Case UC-14.14: Export Lookup Table

| Field | Value |
|-------|-------|
| **ID** | UC-14.14 |
| **Name** | Export Lookup Table |
| **Actor** | Super Admin |
| **Priority** | Low |
| **Description** | Export complete lookup table to file |

**Preconditions:**
- Super Admin is logged in
- Lookup table exists

**Main Flow:**
1. Super Admin navigates to Lookup Management page
2. System displays list of lookup tables
3. Super Admin selects lookup table
4. System displays items list
5. Super Admin clicks "Export" button
6. System displays export options:
   - Format (Excel, CSV, JSON)
   - Include Inactive Items (checkbox)
   - Language (Arabic, English, Both)
7. Super Admin selects export options
8. Super Admin clicks "Generate Export"
9. System generates export file with:
   - All lookup items
   - All fields
   - Active and inactive (if selected)
10. System downloads file to user's device
11. System logs export in audit log
12. System displays success message

**Postconditions:**
- Lookup table exported
- File available for backup or migration

---

#### Use Case UC-14.15: Import Lookup Table

| Field | Value |
|-------|-------|
| **ID** | UC-14.15 |
| **Name** | Import Lookup Table |
| **Actor** | Super Admin |
| **Priority** | Low |
| **Description** | Bulk import lookup items from file |

**Preconditions:**
- Super Admin is logged in
- Import file exists (Excel/CSV)

**Main Flow:**
1. Super Admin navigates to Lookup Management page
2. System displays list of lookup tables
3. Super Admin selects lookup table
4. System displays items list with "Import" button
5. Super Admin clicks "Import"
6. System displays import interface:
   - Select File button
   - Download Template button
   - Column mapping
   - Import options (Skip Duplicates, Update Existing)
7. Super Admin downloads template (optional)
8. Super Admin prepares file
9. Super Admin selects file to import
10. System uploads and parses file
11. Super Admin maps columns to lookup fields
12. Super Admin validates data
13. System displays validation results
14. Super Admin confirms import
15. System processes import:
   - Creates new items
   - Updates existing items (if option selected)
   - Skips duplicates (if option selected)
16. System logs import in audit log
17. System displays success message with summary
18. Imported items visible in list

**Postconditions:**
- Lookup items imported
- List updated
- Audit log contains import record

---

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

### 4.16 Module: Localization Management

**Module Owner:** Super Admin  
**Purpose:** Create and manage translation files (Arabic and English)  
**Dependencies:** All UI components  

#### Use Case UC-16.1: Create Arabic Translation

| Field | Value |
|-------|-------|
| **ID** | UC-16.1 |
| **Name** | Create Arabic Translation |
| **Actor** | Super Admin |
| **Priority** | High |
| **Description** | Add Arabic translation for UI text in ar.json |

**Preconditions:**
- Super Admin is logged in

**Main Flow:**
1. Super Admin navigates to Localization Management page
2. System displays language files:
   - ar.json (Arabic)
   - en.json (English)
3. Super Admin selects ar.json
4. System displays current Arabic translations in editable grid:
   - Key (e.g., "families.add.title")
   - Value (Arabic text)
   - Module/Section
   - Last Modified
5. Super Admin clicks "Add New Translation"
6. System displays translation creation form:
   - Translation Key (required, e.g., "buttons.submit")
   - Arabic Text (required)
   - Module (dropdown for organization)
   - Notes (optional)
7. Super Admin enters translation key
8. Super Admin enters Arabic text
9. System validates key format (dot notation)
10. System checks if key already exists
11. System adds translation to ar.json
12. System saves file
13. System logs addition in audit log
14. System displays success message
15. Translation available in UI

**Postconditions:**
- Arabic translation created
- Available in Arabic UI

---

#### Use Case UC-16.2: Create English Translation

| Field | Value |
|-------|-------|
| **ID** | UC-16.2 |
| **Name** | Create English Translation |
| **Actor** | Super Admin |
| **Priority** | High |
| **Description** | Add English translation for UI text in en.json |

**Preconditions:**
- Super Admin is logged in

**Main Flow:**
1. Super Admin navigates to Localization Management page
2. System displays language files
3. Super Admin selects en.json
4. System displays current English translations in editable grid
5. Super Admin clicks "Add New Translation"
6. System displays translation creation form:
   - Translation Key (required)
   - English Text (required)
   - Module (dropdown)
   - Notes (optional)
7. Super Admin enters translation key
8. Super Admin enters English text
9. System validates key format
10. System checks if key already exists
11. System adds translation to en.json
12. System saves file
13. System logs addition in audit log
14. System displays success message
15. Translation available in UI

**Postconditions:**
- English translation created
- Available in English UI

---

#### Use Case UC-16.3: Update Translation

| Field | Value |
|-------|-------|
| **ID** | UC-16.3 |
| **Name** | Update Translation |
| **Actor** | Super Admin |
| **Priority** | Medium |
| **Description** | Modify existing translation in either language file |

**Preconditions:**
- Super Admin is logged in
- Translation exists

**Main Flow:**
1. Super Admin navigates to Localization Management page
2. System displays language files
3. Super Admin selects language file (ar.json or en.json)
4. System displays translations in editable grid
5. Super Admin locates translation to update
6. Super Admin edits value text directly in grid
   - OR clicks "Edit" button for inline editing
7. Super Admin modifies translation text
8. Super Admin clicks "Save"
9. System updates translation in JSON file
10. System logs change in audit log (key, old value, new value)
11. System displays success message
12. Updated translation reflected in UI

**Postconditions:**
- Translation updated
- UI reflects change

---

#### Use Case UC-16.4: Sync Translation Keys

| Field | Value |
|-------|-------|
| **ID** | UC-16.4 |
| **Name** | Sync Translation Keys |
| **Actor** | Super Admin |
| **Priority** | Medium |
| **Description** | Ensure translation keys exist in both language files |

**Preconditions:**
- Super Admin is logged in

**Main Flow:**
1. Super Admin navigates to Localization Management page
2. System provides "Sync Keys" button
3. Super Admin clicks "Sync Keys"
4. System analyzes both language files:
   - ar.json keys
   - en.json keys
5. System identifies:
   - Keys only in ar.json (missing in en.json)
   - Keys only in en.json (missing in ar.json)
   - Keys in both files
6. System displays sync report:
   - Missing keys list
   - Recommendations
7. Super Admin can:
   - Add missing keys to en.json
   - Add missing keys to ar.json
   - Create placeholders for missing translations
8. Super Admin clicks "Sync"
9. System adds missing keys to both files:
   - Creates placeholder text: "[TRANSLATION NEEDED]"
10. System saves both files
11. System logs sync in audit log
12. System displays success message
13. Both files have same keys

**Postconditions:**
- Translation keys synchronized
- Both files have matching keys
- Missing translations identified

---

#### Use Case UC-16.5: Export Translation File

| Field | Value |
|-------|-------|
| **ID** | UC-16.5 |
| **Name** | Export Translation File |
| **Actor** | Super Admin |
| **Priority** | Low |
| **Description** | Export language JSON file for external translation |

**Preconditions:**
- Super Admin is logged in
- Translation file exists

**Main Flow:**
1. Super Admin navigates to Localization Management page
2. System displays language files
3. Super Admin selects file to export (ar.json or en.json)
4. System displays "Export" button
5. Super Admin clicks "Export"
6. System displays export options:
   - Export Format (JSON, Excel, CSV)
   - Include Metadata (keys, modules, last modified)
   - Filter by Module (optional)
7. Super Admin selects export options
8. Super Admin clicks "Generate Export"
9. System generates export file
10. System downloads file to user's device
11. System logs export in audit log
12. File can be sent to external translators
13. External translators work on file without system access

**Postconditions:**
- Translation file exported
- Ready for external translation

---

#### Use Case UC-16.6: Import Translation File

| Field | Value |
|-------|-------|
| **ID** | UC-16.6 |
| **Name** | Import Translation File |
| **Actor** | Super Admin |
| **Priority** | Medium |
| **Description** | Import translated language JSON file |

**Preconditions:**
- Super Admin is logged in
- Translation file exists (from external translators)

**Main Flow:**
1. Super Admin navigates to Localization Management page
2. System displays "Import" button
3. Super Admin clicks "Import"
4. System displays import interface:
   - Select File button
   - Target Language (dropdown: ar.json, en.json)
   - Import Options:
     - Overwrite Existing (checkbox)
     - Add New Keys Only (checkbox)
     - Create Backup (checkbox, default: true)
5. Super Admin selects file
6. Super Admin selects target language
7. Super Admin selects import options
8. System validates file format (valid JSON)
9. System validates file structure (matches translation format)
10. System displays preview:
    - Keys to import
    - Keys to overwrite
    - New keys to add
    - Validation errors (if any)
11. Super Admin reviews preview
12. Super Admin confirms import
13. System creates backup of current file (if option selected)
14. System processes import:
    - Adds new keys
    - Updates existing keys (if overwrite enabled)
15. System saves updated JSON file
16. System logs import in audit log
17. System displays success message
18. New translations available in UI

**Postconditions:**
- Translation file imported
- UI updated with new translations
- Backup created (if enabled)

---

#### Use Case UC-16.7: View Missing Translations

| Field | Value |
|-------|-------|
| **ID** | UC-16.7 |
| **Name** | View Missing Translations |
| **Actor** | Super Admin |
| **Priority** | Medium |
| **Description** | Identify translation keys missing in either language |

**Preconditions:**
- Super Admin is logged in

**Main Flow:**
1. Super Admin navigates to Localization Management page
2. System provides "View Missing" button
3. Super Admin clicks "View Missing"
4. System analyzes both language files
5. System displays missing translations report:
   - **Keys missing in ar.json:**
     - List of keys in en.json but not ar.json
     - English text for reference
   - **Keys missing in en.json:**
     - List of keys in ar.json but not en.json
     - Arabic text for reference
   - **Placeholder translations:**
     - Keys with placeholder text
6. System provides filter by module
7. System provides export of missing list
8. Super Admin can add missing translations directly from report
9. System shows translation completion percentage

**Postconditions:**
- Missing translations identified
- Can be addressed systematically
- Completion metric visible

---

#### Use Case UC-16.8: Set Default Language

| Field | Value |
|-------|-------|
| **ID** | UC-16.8 |
| **Name** | Set Default Language |
| **Actor** | Super Admin |
| **Priority** | High |
| **Description** | Configure system default language (Arabic) |

**Preconditions:**
- Super Admin is logged in

**Main Flow:**
1. Super Admin navigates to Localization Management page
2. System displays "Language Settings" section
3. System shows current default language: Arabic (ar.json)
4. Super Admin can change default language dropdown:
   - Arabic (ar.json)
   - English (en.json)
5. System warns: "Changing default language affects all new users"
6. Super Admin selects default language
7. Super Admin clicks "Save Settings"
8. System updates system configuration
9. System logs change in audit log
10. System displays success message
11. New users will have selected language as default
12. Existing users keep their language preference

**Business Rules:**
- System default: Arabic (ar.json)
- Can be changed but Arabic recommended
- User preferences override system default

**Postconditions:**
- Default language configured
- Affects new users

---

#### Use Case UC-16.9: Add New Language

| Field | Value |
|-------|-------|
| **ID** | UC-16.9 |
| **Name** | Add New Language |
| **Actor** | Super Admin |
| **Priority** | Low |
| **Description** | Add support for additional language beyond Arabic/English |

**Preconditions:**
- Super Admin is logged in

**Main Flow:**
1. Super Admin navigates to Localization Management page
2. System displays "Add Language" button
3. Super Admin clicks "Add Language"
4. System displays language creation form:
   - Language Name (e.g., "French")
   - Language Code (ISO code, e.g., "fr")
   - Text Direction (dropdown: LTR, RTL)
   - Copy from Existing (dropdown: ar.json, en.json)
5. Super Admin fills in language details
6. Super Admin selects "Copy from Existing" to use as template
7. System validates language code uniqueness
8. System creates new language JSON file (e.g., fr.json)
9. System copies keys from selected template
10. System sets placeholder translations
11. System logs language creation in audit log
12. System displays success message
13. Language available for translation
14. Language switcher includes new language

**Postconditions:**
- New language added
- Ready for translation
- Available in UI

---

#### UseCase UC-16.10: Validate Translation Syntax

| Field | Value |
|-------|-------|
| **ID** | UC-16.10 |
| **Name** | Validate Translation Syntax |
| **Actor** | Super Admin |
| **Priority** | Medium |
| **Description** | Validate JSON syntax and structure of translation files |

**Preconditions:**
- Super Admin is logged in
- Translation files exist

**Main Flow:**
1. Super Admin navigates to Localization Management page
2. System provides "Validate Files" button
3. Super Admin clicks "Validate Files"
4. System validates both language files:
   - ar.json syntax
   - en.json syntax
5. System checks for:
   - Valid JSON format
   - Duplicate keys
   - Invalid characters
   - Malformed keys
   - Empty values (warnings)
   - Key format compliance (dot notation)
6. System displays validation report:
   - Validation Status (Passed/Failed)
   - Errors found (if any)
   - Warnings found (if any)
   - File size
   - Key count
7. If errors found:
   - System shows error details
   - System shows line numbers
   - System suggests fixes
8. If validation passed:
   - System displays "All files valid"
9. System logs validation in audit log
10. Super Admin can fix errors and re-validate

**Postconditions:**
- Translation files validated
- Errors identified
- Syntax confirmed correct

---

### 4.17 Module: Audit Logging

**Module Owner:** System (Automatic)  
**Purpose:** Track every database change with comprehensive details  
**Dependencies:** All entities  

**Important:** All audit logging is automatic. Users cannot directly create audit logs, but can view them.

#### Use Case UC-17.1: Log Entity Creation

| Field | Value |
|-------|-------|
| **ID** | UC-17.1 |
| **Name** | Log Entity Creation |
| **Actor** | System (Automatic) |
| **Priority** | Critical |
| **Description** | Automatically log record creation with user ID, timestamp, and field values |

**Preconditions:**
- Entity being created
- User authenticated

**Main Flow:**
1. User creates new entity record (e.g., new family)
2. System receives create request
3. System validates user has permission
4. System creates entity record
5. **SIMULTANEOUSLY** System creates audit log entry:
   - AuditLogId (GUID)
   - EntityType (e.g., "Family")
   - EntityId (GUID of created record)
   - Operation (enum: Create)
   - UserId (current user ID)
   - UserName
   - Timestamp (UTC)
   - IpAddress
   - FieldChanges (JSON with all field values after creation)
   - Changes (JSON: {"field": {"old": null, "new": "value"}})
6. System saves audit log entry to database
7. Audit log entry linked to entity via EntityId
8. Creation logged permanently

**Postconditions:**
- Entity creation logged
- All field values captured
- User and timestamp recorded
- Cannot be modified by users

---

#### Use Case UC-17.2: Log Entity Update

| Field | Value |
|-------|-------|
| **ID** | UC-17.2 |
| **Name** | Log Entity Update |
| **Actor** | System (Automatic) |
| **Priority** | Critical |
| **Description** | Automatically log record modification with user ID, timestamp, and field changes (before/after) |

**Preconditions:**
- Entity being updated
- User authenticated
- Entity exists

**Main Flow:**
1. User requests to update entity record
2. System receives update request
3. System retrieves current entity values (before update)
4. System validates user has permission
5. System validates changes
6. System applies updates to entity
7. **SIMULTANEOUSLY** System creates audit log entry:
   - AuditLogId (GUID)
   - EntityType (e.g., "Family")
   - EntityId (GUID of record)
   - Operation (enum: Update)
   - UserId (current user ID)
   - UserName
   - Timestamp (UTC)
   - IpAddress
   - FieldChanges (JSON with changed fields only):
     - For each changed field:
       - FieldName
       - OldValue
       - NewValue
8. System saves audit log entry
9. Update logged with field-level detail

**Example FieldChanges JSON:**
```json
{
  "Address": {
    "old": "123 Old Street",
    "new": "456 New Street"
  },
  "Phone": {
    "old": "555-1234",
    "new": "555-5678"
  }
}
```

**Postconditions:**
- Entity update logged
- Before/after values captured for changed fields
- User and timestamp recorded
- Cannot be modified by users

---

#### Use Case UC-17.3: Log Entity Deletion

| Field | Value |
|-------|-------|
| **ID** | UC-17.3 |
| **Name** | Log Entity Deletion |
| **Actor** | System (Automatic) |
| **Priority** | Critical |
| **Description** | Automatically log record deletion with user ID, timestamp, and deleted data |

**Preconditions:**
- Entity being deleted
- User authenticated
- Entity exists

**Main Flow:**
1. User requests to delete entity record
2. System receives delete request
3. System retrieves current entity values (before deletion)
4. System validates user has permission
5. System performs soft delete:
   - Sets entity.IsDeleted = true
   - Sets entity.DeletionTime = current UTC
   - Sets entity.DeleterId = current user ID
6. **SIMULTANEOUSLY** System creates audit log entry:
   - AuditLogId (GUID)
   - EntityType (e.g., "Family")
   - EntityId (GUID of record)
   - Operation (enum: Delete)
   - UserId (current user ID)
   - UserName
   - Timestamp (UTC)
   - IpAddress
   - FieldChanges (JSON with all field values before deletion)
7. System saves audit log entry
8. Deletion logged with complete record snapshot

**Postconditions:**
- Entity deletion logged
- All field values preserved before deletion
- User and timestamp recorded
- Data recoverable via audit log

---

#### Use Case UC-17.4: Log User Login

| Field | Value |
|-------|-------|
| **ID** | UC-17.4 |
| **Name** | Log User Login |
| **Actor** | System (Automatic) |
| **Priority** | High |
| **Description** | Track user login events with timestamp and IP address |

**Preconditions:**
- User attempting login

**Main Flow:**
1. User submits login credentials
2. System validates credentials
3. If credentials valid:
   - System generates JWT token
   - System logs login event:
     - AuditLogId (GUID)
     - EntityType = "UserLogin"
     - EntityId = UserId
     - Operation = "Login"
     - UserId
     - UserName
     - Timestamp (UTC)
     - IpAddress
     - FieldChanges (JSON with login details)
4. System saves login audit log
5. User login successful

**Postconditions:**
- Login event logged
- Timestamp and IP recorded
- Used for security monitoring

---

#### Use Case UC-17.5: Log User Logout

| Field | Value |
|-------|-------|
| **ID** | UC-17.5 |
| **Name** | Log User Logout |
| **Actor** | System (Automatic) |
| **Priority** | Medium |
| **Description** | Track user logout events with timestamp |

**Preconditions:**
- User logging out

**Main Flow:**
1. User clicks logout
2. System invalidates JWT token
3. System logs logout event:
   - AuditLogId (GUID)
   - EntityType = "UserLogout"
   - EntityId = UserId
   - Operation = "Logout"
   - UserId
   - UserName
   - Timestamp (UTC)
   - IpAddress
4. System saves logout audit log
5. User logged out

**Postconditions:**
- Logout event logged
- Session tracking complete

---

#### Use Case UC-17.6: Log Failed Login Attempts

| Field | Value |
|-------|-------|
| **ID** | UC-17.6 |
| **Name** | Log Failed Login Attempts |
| **Actor** | System (Automatic) |
| **Priority** | High |
| **Description** | Record failed login attempts for security monitoring |

**Preconditions:**
- Failed login attempt

**Main Flow:**
1. User submits login credentials
2. System validates credentials
3. If credentials INVALID:
   - System logs failed login attempt:
     - AuditLogId (GUID)
     - EntityType = "FailedLogin"
     - EntityId = null (no user context)
     - Operation = "FailedLogin"
     - UserId = null
     - UserName = attempted username
     - Timestamp (UTC)
     - IpAddress
     - FieldChanges (JSON with reason for failure)
4. System increments failed login counter for IP/username
5. If exceeded threshold:
   - System locks account or IP
   - System logs lock event
6. Failed login recorded

**Postconditions:**
- Failed login logged
- Security monitoring data
- Account lockout triggered if threshold exceeded

---

#### Use Case UC-17.7: View Audit Logs

| Field | Value |
|-------|-------|
| **ID** | UC-17.7 |
| **Name** | View Audit Logs |
| **Actor** | Super Admin, Admin |
| **Priority** | Medium |
| **Description** | View comprehensive audit logs with filtering by entity, user, date range |

**Preconditions:**
- Super Admin or Admin is logged in
- Audit logs exist

**Main Flow:**
1. Admin/Super Admin navigates to Audit Logs page
2. System displays audit log search interface with filters:
   - **Filters:**
     - Entity Type (dropdown: All, Family, Orphan, Charity, etc.)
     - Operation (dropdown: All, Create, Update, Delete, Login, Logout)
     - User (dropdown of users)
     - Date Range From (date picker)
     - Date Range To (date picker)
     - Entity Id (text input)
   - **Search:** By username, entity type, etc.
3. User applies filters
4. System queries audit log based on filters
5. System displays audit logs in grid with columns:
   - Timestamp
   - Operation
   - Entity Type
   - Entity Id
   - User Name
   - IP Address
   - Details (expandable)
6. System provides pagination
7. System provides sorting by timestamp
8. User can click log entry to view details
9. System can export filtered logs

**Postconditions:**
- Audit logs displayed
- Filtering enables specific searches
- Security monitoring possible

---

#### Use Case UC-17.8: View Entity Change History

| Field | Value |
|-------|-------|
| **ID** | UC-17.8 |
| **Name** | View Entity Change History |
| **Actor** | Super Admin, Admin |
| **Priority** | Medium |
| **Description** | View complete modification history for specific record |

**Preconditions:**
- Super Admin or Admin is logged in
- Entity exists

**Main Flow:**
1. Admin/Super Admin views entity details (e.g., family details)
2. System provides "View History" button
3. User clicks "View History"
4. System queries audit logs WHERE EntityId = current entity
5. System retrieves all audit log entries for this entity
6. System displays change history in chronological order:
   - **For each log entry:**
     - Timestamp
     - Operation (Create, Update, Delete)
     - User Name
     - Field Changes (before/after values)
     - IP Address
7. System highlights changes visually:
   - Added values in green
   - Deleted values in red
   - Modified values highlighted
8. System provides timeline view
9. User can see complete history of record
10. User can export history

**Postconditions:**
- Complete entity history visible
- All changes tracked
- Accountability maintained

---

#### Use Case UC-17.9: View User Activity Log

| Field | Value |
|-------|-------|
| **ID** | UC-17.9 |
| **Name** | View User Activity Log |
| **Actor** | Super Admin |
| **Priority** | Medium |
| **Description** | View all actions performed by specific user |

**Preconditions:**
- Super Admin is logged in
- User exists

**Main Flow:**
1. Super Admin navigates to User Management page
2. System displays list of users
3. Super Admin selects user
4. System displays user details with "View Activity" button
5. Super Admin clicks "View Activity"
6. System queries audit logs WHERE UserId = selected user
7. System retrieves all audit log entries for user
8. System displays user activity in chronological order:
   - Timestamp
   - Operation
   - Entity Type
   - Entity Details
   - IP Address
   - Field Changes
9. System provides summary statistics:
   - Total actions
   - Actions by type
   - Actions by entity
   - First activity date
   - Last activity date
10. System provides filter by date range
11. System provides filter by operation type
12. System can export user activity

**Postconditions:**
- User activity visible
- Complete audit trail for user
- Security monitoring enabled

---

#### Use Case UC-17.10: Export Audit Logs

| Field | Value |
|-------|-------|
| **ID** | UC-17.10 |
| **Name** | Export Audit Logs |
| **Actor** | Super Admin |
| **Priority** | Low |
| **Description** | Export audit logs for compliance and reporting |

**Preconditions:**
- Super Admin is logged in
- Audit logs exist

**Main Flow:**
1. Super Admin navigates to Audit Logs page
2. System provides "Export Logs" button
3. Super Admin clicks "Export Logs"
4. System displays export options:
   - Date Range (required)
   - Filters (entity type, operation, user)
   - Export Format (Excel, CSV, JSON)
   - Include Field Changes (checkbox, for detailed export)
5. Super Admin selects export options
6. Super Admin clicks "Generate Export"
7. System queries audit logs based on filters
8. System generates export file with:
   - All audit log fields
   - Field changes (if included)
   - Formatted for readability
9. System downloads file to user's device
10. System logs export in audit log (meta-logging!)
11. System displays success message

**Postconditions:**
- Audit logs exported
- Available for compliance
- Long-term archival

---

#### Use Case UC-17.11: Search Audit Logs

| Field | Value |
|-------|-------|
| **ID** | UC-17.11 |
| **Name** | Search Audit Logs |
| **Actor** | Super Admin, Admin |
| **Priority** | Medium |
| **Description** | Search audit logs by keyword, entity type, or action |

**Preconditions:**
- Super Admin or Admin is logged in
- Audit logs exist

**Main Flow:**
1. Admin/Super Admin navigates to Audit Logs page
2. System displays search box
3. User enters search term:
   - Keyword (searches field values, usernames)
   - Entity Type
   - Operation
4. User applies filters
5. System searches audit logs:
   - Searches FieldChanges JSON for keyword
   - Searches UserName
   - Searches EntityType
6. System displays matching logs
7. System highlights search term in results
8. System shows result count
9. User can refine search

**Postconditions:**
- Audit logs searched
- Specific events found
- Investigation enabled

---

#### Use Case UC-17.12: Compare Record Versions

| Field | Value |
|-------|-------|
| **ID** | UC-17.12 |
| **Name** | Compare Record Versions |
| **Actor** | Super Admin, Admin |
| **Priority** | Low |
| **Description** | Compare different versions of record to see changes |

**Preconditions:**
- Super Admin or Admin is logged in
- Entity has multiple audit log entries

**Main Flow:**
1. Admin/Super Admin views entity history
2. System displays list of audit log entries for entity
3. User selects two log entries to compare:
   - Select "Before" version
   - Select "After" version
4. System retrieves field changes for both versions
5. System displays side-by-side comparison:
   - Field Name | Before Value | After Value
   - Highlights differences
   - Shows unchanged fields
   - Shows added/removed fields
6. System provides visual diff
7. User can see exactly what changed
8. User can understand evolution of record

**Postconditions:**
- Record versions compared
- Changes clearly visible
- Understanding of modifications

---

#### Use Case UC-17.13: Restore Record Version

| Field | Value |
|-------|-------|
| **ID** | UC-17.13 |
| **Name** | Restore Record Version |
| **Actor** | Super Admin |
| **Priority** | Low |
| **Description** | Restore record to previous version from audit log |

**Preconditions:**
- Super Admin is logged in
- Entity exists
- Audit log contains previous version

**Main Flow:**
1. Super Admin views entity history
2. System displays audit log entries
3. Super Admin selects previous version to restore
4. System displays "Restore This Version" button
5. Super Admin clicks "Restore"
6. System displays confirmation dialog:
   - "This will revert the record to the state it was in on [timestamp]. Continue?"
   - System shows what will be changed
7. Super Admin confirms
8. System retrieves field values from selected audit log
9. System updates current entity with historical values
10. System creates NEW audit log entry for this restore:
    - Operation = Update
    - FieldChanges = current values → restored values
11. System logs restore in audit log (meta-logging)
12. System displays success message
13. Record restored to previous state
14. Current values preserved in new audit log

**Alternative Flows:**
- **7a. User cancels:** System returns without changes

**Business Rules:**
- Only Super Admin can restore
- Restore creates new audit log (chain of custody maintained)
- Original audit logs remain untouched

**Postconditions:**
- Record restored to previous version
- New audit log entry created
- Full history preserved

---

### 4.18 Module: Cross-Cutting Concerns

**Module Owner:** All Users (varies by concern)  
**Purpose:** System-wide functionalities that apply across multiple modules  

#### Use Case UC-18.1: Attach Document to Entity

| Field | Value |
|-------|-------|
| **ID** | UC-18.1 |
| **Name** | Attach Document to Entity |
| **Actor** | All Users |
| **Priority** | Medium |
| **Description** | Upload and attach files to any entity (families, projects, etc.) |

**Preconditions:**
- User is logged in
- User has permission to modify entity
- Entity exists

**Main Flow:**
1. User views entity details (e.g., family, project, mission)
2. System displays "Attachments" section with "Add Attachment" button
3. User clicks "Add Attachment"
4. System displays file upload dialog:
   - File Selection (browse or drag-and-drop)
   - Document Type (dropdown: ID, Birth Certificate, Contract, Photo, Report, Other)
   - Description (optional)
   - Date (default: today)
5. User selects file
6. User selects document type
7. User enters description (optional)
8. User clicks "Upload"
9. System validates file:
   - File type (allowed types)
   - File size (max 10MB)
   - Virus scan (if available)
10. System uploads file to storage
11. System generates unique file ID (GUID)
12. System creates attachment record:
    - AttachmentId (GUID)
    - FileName
    - FilePath/URL
    - FileSize
    - ContentType
    - DocumentType
    - Description
    - UploadDate
    - UploadedBy (FK_UserId)
    - EntityType (e.g., "Family")
    - EntityId (GUID of entity)
13. System logs attachment creation in audit log
14. System displays success message
15. Attachment visible in entity's attachments list
16. User can download attachment by clicking filename

**Postconditions:**
- Document attached to entity
- Attachment record created
- File stored securely
- Audit log contains attachment record

---

#### Use Case UC-18.2: View Entity Attachments

| Field | Value |
|-------|-------|
| **ID** | UC-18.2 |
| **Name** | View Entity Attachments |
| **Actor** | All Users |
| **Priority** | Low |
| **Description** | View all files attached to specific entity record |

**Preconditions:**
- User is logged in
- User has permission to view entity
- Entity exists

**Main Flow:**
1. User views entity details
2. System displays "Attachments" section
3. System retrieves attachments WHERE EntityId = current entity
4. System displays attachments in list/grid:
   - FileName (clickable to download)
   - Document Type
   - Description
   - File Size
   - Upload Date
   - Uploaded By (user name)
   - Actions (Download, Delete - if user has permission)
5. System provides filter by document type
6. System provides search by filename
7. User can click attachment to download
8. User can delete attachments (if has permission)

**Postconditions:**
- All attachments visible
- Downloadable
- Manageable

---

#### Use Case UC-18.3: Delete Attachment

| Field | Value |
|-------|-------|
| **ID** | UC-18.3 |
| **Name** | Delete Attachment |
| **Actor** | All Users |
| **Priority** | Medium |
| **Description** | Remove attached file from entity |

**Preconditions:**
- User is logged in
- User has permission to modify entity
- Attachment exists

**Main Flow:**
1. User views entity attachments
2. System displays attachments list
3. User clicks "Delete" button next to attachment
4. System displays confirmation: "Delete this attachment? This cannot be undone."
5. User confirms deletion
6. System deletes attachment record (soft delete preferred)
7. System deletes file from storage OR marks for deletion
8. System logs deletion in audit log:
    - AttachmentId
    - FileName
    - EntityId
    - DeletedBy
9. System displays success message
10. Attachment no longer visible in list
11. File removed from storage

**Alternative Flows:**
- **5a. User cancels:** System returns without changes

**Postconditions:**
- Attachment deleted
- File removed from storage
- Audit log contains deletion record

---

#### Use Case UC-18.4: Download Attachment

| Field | Value |
|-------|-------|
| **ID** | UC-18.4 |
| **Name** | Download Attachment |
| **Actor** | All Users |
| **Priority** | Low |
| **Description** | Download attached file to local system |

**Preconditions:**
- User is logged in
- User has permission to view entity
- Attachment exists

**Main Flow:**
1. User views entity attachments
2. System displays attachments list
3. User clicks attachment filename
4. System validates user has access to entity
5. System retrieves file from storage
6. System streams file to user's browser
7. Browser downloads file to user's device
8. System logs download in audit log:
    - AttachmentId
    - FileName
    - DownloadedBy
    - DownloadTimestamp
9. File saved on user's device

**Postconditions:**
- File downloaded
- Access logged
- Audit trail maintained

---

#### Use Case UC-18.5: Send Notification

| Field | Value |
|-------|-------|
| **ID** | UC-18.5 |
| **Name** | Send Notification |
| **Actor** | System (Automatic) |
| **Priority** | High |
| **Description** | Send notification to users based on events |

**Preconditions:**
- System event occurs
- User notification preferences configured

**Main Flow:**
1. System event occurs (e.g., record created, status changed, ticket assigned)
2. System identifies users who should be notified
3. System checks user notification preferences:
   - Email notifications (enabled/disabled)
   - In-app notifications (enabled/disabled)
   - SMS notifications (if configured)
4. System creates notification record:
   - NotificationId (GUID)
   - UserId (recipient)
   - Title
   - Message
   - Type (dropdown: Info, Warning, Success, Error)
   - Priority (Low, Medium, High, Urgent)
   - ActionUrl (link to related entity)
   - CreatedDate
   - IsRead (false, initially)
5. System saves notification to database
6. System sends notifications via enabled channels:
   - **In-App:** Notification appears in user's notification center
   - **Email:** Email sent to user's email address
   - **SMS:** SMS sent to user's phone (if configured)
7. System logs notification delivery
8. User receives notification

**Postconditions:**
- Notification created and delivered
- User informed of event
- Notification logged

---

#### Use Case UC-18.6: View Notifications

| Field | Value |
|-------|-------|
| **ID** | UC-18.6 |
| **Name** | View Notifications |
| **Actor** | All Users |
| **Priority** | Medium |
| **Description** | View user notifications and alerts |

**Preconditions:**
- User is logged in
- Notifications exist

**Main Flow:**
1. User logged into system
2. System displays notification bell/icon in header
3. System shows badge with unread notification count
4. User clicks notification bell/icon
5. System displays notification dropdown/list:
   - **For each notification:**
     - Title
     - Message
     - Type (with icon/color coding)
     - Created Date (relative time: "2 hours ago")
     - IsRead indicator (unread highlighted)
     - Action Link (clickable)
6. System shows most recent notifications first
7. System provides "Mark All as Read" button
8. System provides "View All Notifications" link
9. User can click notification to view details
10. User can click action link to navigate to related entity

**Postconditions:**
- Notifications visible
- Unread count shown
- User can stay informed

---

#### Use Case UC-18.7: Mark Notification as Read

| Field | Value |
|-------|-------|
| **ID** | UC-18.7 |
| **Name** | Mark Notification as Read |
| **Actor** | All Users |
| **Priority** | Low |
| **Description** | Mark specific notification as read |

**Preconditions:**
- User is logged in
- Unread notification exists

**Main Flow:**
1. User views notification list
2. System displays notifications with unread indicators
3. User clicks on notification
4. System marks notification as read:
   - Sets notification.IsRead = true
   - Sets notification.ReadDate = current timestamp
5. System updates notification record
6. System logs read action in audit log
7. Notification no longer highlighted as unread
8. Unread count badge decreases
9. If user clicks "Mark All as Read":
   - System marks all user's notifications as read
   - System clears unread badge

**Postconditions:**
- Notification marked as read
- Unread count updated
- User acknowledged notification

---

#### Use Case UC-18.8: Configure Dynamic Lists

| Field | Value |
|-------|-------|
| **ID** | UC-18.8 |
| **Name** | Configure Dynamic Lists |
| **Actor** | Super Admin |
| **Priority** | Medium |
| **Description** | Set up configurable lists and lookups for dropdowns and selections |

**Preconditions:**
- Super Admin is logged in

**Main Flow:**
1. Super Admin navigates to Dynamic Lists configuration
2. System displays list of configurable lists
3. Super Admin selects list to configure
4. System displays list items with:
   - Value (stored in database)
   - Display Text (Arabic)
   - Display Text (English)
   - Order
   - IsActive
   - Color/Icon (optional)
5. Super Admin can:
   - Add new list items
   - Edit existing items
   - Reorder items
   - Activate/deactivate items
   - Set colors/icons
6. Super Admin clicks "Save Configuration"
7. System updates list configuration
8. System log changes in audit log
9. System displays success message
10. Updated list visible in dropdowns throughout system

**Postconditions:**
- Dynamic list configured
- Dropdowns reflect configuration
- No code changes needed

---

#### Use Case UC-18.9: View Dynamic List Items

| Field | Value |
|-------|-------|
| **ID** | UC-18.9 |
| **Name** | View Dynamic List Items |
| **Actor** | All Users |
| **Priority** | Low |
| **Description** | View items from configured lookup lists |

**Preconditions:**
- User is logged in
- Dynamic lists configured

**Main Flow:**
1. User interacts with system (forms, filters, dropdowns)
2. System displays dropdown/select controls
3. System populates dropdowns from dynamic lists:
   - Retrieves active items
   - Orders by Order field
   - Displays in user's language (Arabic or English)
4. User sees current list values
5. User selects value
6. System uses selected value

**Postconditions:**
- Dynamic list items visible
- User can select from list
- Lists maintained by Super Admin

---

#### Use Case UC-18.10: Search and Filter Records

| Field | Value |
|-------|-------|
| **ID** | UC-18.10 |
| **Name** | Search and Filter Records |
| **Actor** | All Users |
| **Priority** | Medium |
| **Description** | Apply search and filters across all modules with Charity filtering for Admin roles |

**Preconditions:**
- User is logged in
- Records exist in module

**Main Flow:**
1. User navigates to any module (e.g., Families, Projects, Missions)
2. System displays grid/list of records
3. System provides search box:
   - Free-text search across relevant fields
4. System provides filter panel:
   - Filters vary by module
   - Common filters: Date Range, Status, Category, etc.
5. **For Charity role:**
   - System automatically filters by CharityId = CurrentUser.CharityId
   - Charity filter not visible (data isolation)
6. **For Admin/Super Admin:**
   - System provides Charity dropdown filter:
     - All Charities (shows all records)
     - [Charity 1]
     - [Charity 2]
     - ... (all active charities)
   - User can select specific charity to filter
7. User enters search term or applies filters
8. System queries database with filters
9. System updates grid with filtered results
10. System displays result count
11. User can clear filters to see all records

**Business Rules:**
- Charity: Automatic filtering by CharityId
- Admin/SuperAdmin: Optional Charity filtering
- Database-level filtering for security

**Postconditions:**
- Records filtered
- Search results displayed
- Data isolation enforced

---

#### Use Case UC-18.11: Export Any Grid

| Field | Value |
|-------|-------|
| **ID** | UC-18.11 |
| **Name** | Export Any Grid |
| **Actor** | All Users |
| **Priority** | Medium |
| **Description** | Export data grid views to Excel/PDF |

**Preconditions:**
- User is logged in
- User has permission to view data
- Grid has records

**Main Flow:**
1. User views any data grid in system
2. System provides "Export" button above grid
3. User clicks "Export"
4. System displays export options:
   - **Format:** Excel, PDF, CSV
   - **Fields:** Select All checkbox, individual field checkboxes
   - **Filters:** Include Current Filters (checkbox)
   - **Language:** Arabic, English, Both
5. User selects export format
6. User selects fields to export (or Select All)
7. User chooses to include/exclude current filters
8. User clicks "Generate Export"
9. System retrieves data from grid:
   - Applies current filters (if selected)
   - Respects data isolation (Charity filtering)
   - Includes selected fields
10. System generates export file
11. System downloads file to user's device
12. System logs export in audit log:
    - Exported By
    - Module/Entity
    - Record Count
    - Format
13. System displays success message

**Postconditions:**
- Grid data exported
- User gets file with current view
- Audit log contains export record

---

#### Use Case UC-18.12: Apply Charity Data Filter

| Field | Value |
|-------|-------|
| **ID** | UC-18.12 |
| **Name** | Apply Charity Data Filter |
| **Actor** | System (Automatic) |
| **Priority** | Critical |
| **Description** | Automatically filter data by Charity ID for Charity role users |

**Preconditions:**
- Charity user logged in
- Data query executed

**Main Flow:**
1. Charity user accesses any data grid/list
2. System identifies user role = Charity
3. System retrieves user's CharityId
4. User's CharityId stored in JWT token
5. **For EVERY database query:**
   - System automatically adds WHERE clause:
     - `WHERE CharityId = @CurrentUserCharityId`
   - For entities without direct CharityId:
     - System filters through related entities
     - Example: Family → linked to Charity directly
     - Example: Orphan → linked to Family → linked to Charity
6. System executes filtered query
7. System returns only records belonging to user's charity
8. Charity user sees only their data
9. Charity filter NOT visible in UI (automatic, not optional)

**Business Rules:**
- AUTOMATIC and MANDATORY for Charity role
- Cannot be bypassed
- Enforced at database level, not just UI
- Admin/SuperAdmin NOT filtered (see UC-18.13)

**Postconditions:**
- Charity data isolated
- Charity users see only their data
- Security enforced at database level

---

#### Use Case UC-18.13: Validate Charity Access

| Field | Value |
|-------|-------|
| **ID** | UC-18.13 |
| **Name** | Validate Charity Access |
| **Actor** | System (Automatic) |
| **Priority** | Critical |
| **Description** | Verify Charity user can only access their own data on every request |

**Preconditions:**
- Charity user logged in
- API request received

**Main Flow:**
1. Charity user makes API request (read, update, delete)
2. System receives request with entity ID
3. System extracts user's CharityId from JWT token
4. System validates CharityId matches entity's CharityId:
   - **For READ requests:**
     - Query automatically filtered (UC-18.12)
     - User cannot request other charities' data
   - **For UPDATE/DELETE requests:**
     - System validates: entity.CharityId == user.CharityId
     - If mismatch: System returns 403 Forbidden
     - Logs unauthorized access attempt
5. System blocks cross-charity access
6. System logs validation failures in security log
7. Charity users can only access their own data

**Security Rules:**
- Validated on EVERY request
- Cannot bypass validation
- Unauthorized attempts logged
- Admin/SuperAdmin exempt from validation

**Postconditions:**
- Charity access validated
- Cross-charity access blocked
- Security maintained

---

## Final Summary

This completes the comprehensive Use Case Specification for the Charity.IIROSA system with all 18 modules and 230+ use cases.

### Module Breakdown:
1. **User & Role Management** (10 use cases)
2. **Employees** (7 use cases)
3. **Charities** (14 use cases)
4. **Families** (15 use cases)
5. **Orphan Payments** (13 use cases)
6. **Periodic Orphan Reports** (10 use cases)
7. **Office Development Projects** (14 use cases)
8. **Missions** (13 use cases)
9. **Seasonal Aid** (11 use cases)
10. **Housing Projects** (10 use cases)
11. **General Checks** (10 use cases)
12. **Imports & Exports** (14 use cases)
13. **Technical Support** (10 use cases)
14. **Lookup Management** (15 use cases)
15. **Dynamic Page Management** (10 use cases)
16. **Localization Management** (10 use cases)
17. **Audit Logging** (13 use cases)
18. **Cross-Cutting Concerns** (13 use cases)

### Total Use Cases: **230+**

### Key Features:
- Comprehensive data isolation (Charity users see only their data)
- Full audit logging (every database change tracked)
- Arabic-first UI with English support
- Dynamic page management per role
- Lookup management by Super Admin
- Offline payment processing (orphan groups, not online payments)
- Role-based module access control

---

**End of Complete Use Case Specification**

Document Status: **COMPLETE**  
Version: 1.0  
Date: 2026-04-20  
Total Use Cases: 230+  
Total Modules: 18  
Total Actors: 7
