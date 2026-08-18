### 4.10 Module: Housing Projects

**Module Owner:** Admin, Super Admin  
**Purpose:** Track housing construction/renovation projects (Charity CANNOT access)  
**Dependencies:** Charities, Centers, Regions, Families  

**Important Note:** Charity users CANNOT access this module. Only Admin and Super Admin can manage housing projects.

#### Use Case UC-10.1: Register Housing Project

| Field | Value |
|-------|-------|
| **ID** | UC-10.1 |
| **Name** | Register Housing Project |
| **Actor** | Admin, Super Admin |
| **Priority** | High |
| **Description** | Create new housing project with location and specifications (Charity CANNOT access) |

**Preconditions:**
- Admin or Super Admin is logged in
- Geographic data exists

**Main Flow:**
1. User navigates to Housing Projects page
2. System displays list of projects with "Add New Project" button
3. User clicks "Add New Project"
4. System displays housing project creation form with sections:
   - **Basic Information:**
     - Project Name (required)
     - Project Description
     - Project Type (dropdown: New Construction, Renovation, Repair, Expansion)
     - Start Date (required)
     - Expected End Date
   - **Location:**
     - Country (dropdown)
     - Region (dropdown, filtered by Country)
     - Center (dropdown, filtered by Region)
     - Address/Village
     - GPS Coordinates (optional)
   - **Specifications:**
     - Housing Type (dropdown: Apartment, Villa, House, Room)
     - Number of Units
     - Area per Unit (sq meters)
     - Total Area (sq meters)
   - **Financial Information:**
     - Total Budget (decimal)
     - Budget Currency (dropdown: EGP, SAR)
     - Donor Name
   - **Beneficiary Assignment:**
     - Assigned Charity (dropdown: None or specific)
     - Assigned Family (dropdown: None or specific)
   - **Status:**
     - Project Status (dropdown: Planning, In Progress, Completed, On Hold)
     - Completion Percentage (0-100)
   - **Documents:**
     - Attachments (plans, permits, photos)
5. User fills in required fields
6. System validates data
7. System creates housing project record
8. System logs creation in audit log
9. System displays success message
10. Project appears in project list

**Postconditions:**
- Housing project record created
- Audit log contains creation record

---

#### Use Case UC-10.2: Set Project Budget

| Field | Value |
|-------|-------|
| **ID** | UC-10.2 |
| **Name** | Set Project Budget |
| **Actor** | Admin, Super Admin |
| **Priority** | High |
| **Description** | Specify construction/renovation budget |

**Preconditions:**
- Admin or Super Admin is logged in
- Housing project exists

**Main Flow:**
1. User navigates to Housing Projects page
2. System displays list of projects
3. User selects project
4. System displays project details with "Financial Information" section
5. User enters:
   - Total Budget (decimal)
   - Budget Currency (dropdown)
   - Donor Name (optional)
6. User clicks "Save"
7. System validates budget (positive number)
8. System updates project budget
9. System logs change in audit log
10. System displays success message

**Postconditions:**
- Project budget set
- Used for cost tracking

---

#### Use Case UC-10.3: Assign Beneficiary Family

| Field | Value |
|-------|-------|
| **ID** | UC-10.3 |
| **Name** | Assign Beneficiary Family |
| **Actor** | Admin, Super Admin |
| **Priority** | High |
| **Description** | Link eligible family to housing project |

**Preconditions:**
- Admin or Super Admin is logged in
- Housing project exists
- Families exist

**Main Flow:**
1. User navigates to Housing Projects page
2. System displays list of projects
3. User selects project
4. System displays project details with "Beneficiary" section
5. User selects:
   - Assigned Charity (dropdown, filters families)
   - Assigned Family (dropdown filtered by charity)
6. User clicks "Save"
7. System validates family selection
8. System updates project with beneficiary
9. System logs assignment in audit log
10. System displays success message
11. Family linked to project
12. Project visible in family's profile (if applicable)

**Postconditions:**
- Family assigned to project
- Used for tracking and reporting

---

#### Use Case UC-10.4: Track Construction Progress

| Field | Value |
|-------|-------|
| **ID** | UC-10.4 |
| **Name** | Track Construction Progress |
| **Actor** | Admin, Super Admin |
| **Priority** | Medium |
| **Description** | Monitor housing project milestones |

**Preconditions:**
- Admin or Super Admin is logged in
- Housing project exists

**Main Flow:**
1. User navigates to Housing Projects page
2. System displays list of projects
3. User selects project
4. System displays project details with "Progress Tracking" section
5. User updates:
   - Project Status (dropdown: Planning, In Progress, Completed, On Hold)
   - Completion Percentage (slider or number input: 0-100)
   - Current Stage (dropdown: Foundation, Structure, Finishing, Completed)
   - Notes on progress
6. User uploads progress photos (optional)
7. User clicks "Save Progress"
8. System updates project progress
9. System creates progress history entry
10. System logs update in audit log
11. System displays success message
12. Progress visible in project timeline

**Postconditions:**
- Project progress tracked
- Progress history maintained

---

#### Use Case UC-10.5: Record Project Completion

| Field | Value |
|-------|-------|
| **ID** | UC-10.5 |
| **Name** | Record Project Completion |
| **Actor** | Admin, Super Admin |
| **Priority** | High |
| **Description** | Mark housing project as completed with handover date |

**Preconditions:**
- Admin or Super Admin is logged in
- Housing project exists
- Project not already completed

**Main Flow:**
1. User navigates to Housing Projects page
2. System displays list of projects
3. User selects project to complete
4. System displays project details with "Mark as Completed" button
5. User clicks "Mark as Completed"
6. System displays completion form:
   - Actual End Date (default: today)
   - Final Cost (decimal)
   - Completion Notes
   - Handover Document (upload)
7. User fills in completion details
8. User optionally uploads handover document/photos
9. User confirms completion
10. System sets project.ProjectStatus = "Completed"
11. System sets project.CompletionPercentage = 100
12. System sets project.ActualEndDate
13. System creates completion record
14. System logs completion in audit log
15. System displays success message
16. Project marked as completed
17. Project becomes read-only (or restricted edit)

**Postconditions:**
- Project completed
- Completion date and final cost recorded
- Audit log contains completion record

---

#### Use Case UC-10.6: View Housing Projects

| Field | Value |
|-------|-------|
| **ID** | UC-10.6 |
| **Name** | View Housing Projects |
| **Actor** | Admin, Super Admin |
| **Priority** | Low |
| **Description** | View all housing projects with status |

**Preconditions:**
- Admin or Super Admin is logged in

**Main Flow:**
1. User navigates to Housing Projects page
2. System displays list of projects in grid with columns:
   - Project Name
   - Project Type
   - Region
   - Center
   - Address
   - Assigned Charity
   - Assigned Family
   - Project Status
   - Completion Percentage
   - Start Date
   - Expected End Date
   - Budget
3. System provides search by project name
4. System provides filter by:
   - Project Type
   - Project Status
   - Region
   - Center
   - Charity
   - Date Range
5. System provides pagination
6. System provides sorting by any column
7. User can click project to view details
8. System provides "Export to Excel" button to export current filtered/sorted project list
9. User can export list to Excel format with all displayed columns

**Postconditions:**
- Housing project list displayed
- Filtering available
- Project data can be exported to Excel

---

#### Use Case UC-10.7: Update Project Status

| Field | Value |
|-------|-------|
| **ID** | UC-10.7 |
| **Name** | Update Project Status |
| **Actor** | Admin, Super Admin |
| **Priority** | Medium |
| **Description** | Modify project status and progress |

**Preconditions:**
- Admin or Super Admin is logged in
- Housing project exists

**Main Flow:**
1. User navigates to Housing Projects page
2. System displays list of projects
3. User selects project
4. System displays project details
5. User updates:
   - Project Status (dropdown)
   - Completion Percentage
   - Current Stage
   - Notes
6. User clicks "Save"
7. System validates data
8. System updates project status
9. System logs change in audit log
10. System displays success message

**Postconditions:**
- Project status updated
- Progress tracked

---

#### Use Case UC-10.8: Attach Project Documents

| Field | Value |
|-------|-------|
| **ID** | UC-10.8 |
| **Name** | Attach Project Documents |
| **Actor** | Admin, Super Admin |
| **Priority** | Medium |
| **Description** | Upload plans, permits, and photos |

**Preconditions:**
- Admin or Super Admin is logged in
- Housing project exists

**Main Flow:**
1. User navigates to Housing Projects page
2. System displays list of projects
3. User selects project
4. System displays project details with "Documents" section
5. User clicks "Attach Document"
6. System displays file upload dialog:
   - File selection
   - Document Type (dropdown: Plan, Permit, Photo, Progress Report, Other)
   - Description
   - Date
7. User selects file
8. User selects document type
9. User enters description
10. User clicks "Upload"
11. System validates file
12. System uploads file
13. System creates attachment record
14. System logs attachment in audit log
15. System displays success message
16. Document visible in project documents list

**Postconditions:**
- Document attached to project
- Attachment record created

---

#### Use Case UC-10.9: Generate Housing Report

| Field | Value |
|-------|-------|
| **ID** | UC-10.9 |
| **Name** | Generate Housing Report |
| **Actor** | Admin, Super Admin |
| **Priority** | Medium |
| **Description** | Report on housing projects and beneficiaries |

**Preconditions:**
- Admin or Super Admin is logged in
- Housing projects exist

**Main Flow:**
1. User navigates to Housing Projects page
2. System provides "Generate Report" button
3. User clicks "Generate Report"
4. System displays report options:
   - Report Period (date range)
   - Project Status filter
   - Include Completed Projects
   - Include Active Projects
   - Group By (Charity, Region, Project Type)
5. User selects report options
6. User clicks "Generate"
7. System generates housing report with:
   - **Project Summary:**
     - Total Projects
     - By Status
     - By Type
     - By Region
     - By Charity
   - **Financial Summary:**
     - Total Budget
     - Budget by Status
     - Average Cost per Project
   - **Progress Summary:**
     - Average Completion Percentage
     - Projects On Track
     - Delayed Projects
   - **Beneficiary Summary:**
     - Families Housed
     - Individuals Benefited (estimated)
   - **Detailed Project List:**
     - All projects matching filters with details
8. System displays report preview
9. System provides export options (Excel, PDF)
10. User can export or print report
11. System logs report generation in audit log

**Postconditions:**
- Housing report generated
- Available for export/print

---

#### Use Case UC-10.10: Assign Project to Charity

| Field | Value |
|-------|-------|
| **ID** | UC-10.10 |
| **Name** | Assign Project to Charity |
| **Actor** | Admin, Super Admin |
| **Priority** | Medium |
| **Description** | Link housing project to specific charity for beneficiary tracking |

**Preconditions:**
- Admin or Super Admin is logged in
- Housing project exists
- Charities exist

**Main Flow:**
1. User navigates to Housing Projects page
2. System displays list of projects
3. User selects project
4. System displays project details with "Charity Assignment" section
5. User selects Assigned Charity from dropdown
6. User clicks "Save"
7. System updates project with charity
8. System logs assignment in audit log
9. System displays success message
10. Project linked to charity
11. Family selection filtered by assigned charity

**Postconditions:**
- Project assigned to charity
- Used for filtering and reporting

---

**[Document continues with remaining modules in next section...]**
