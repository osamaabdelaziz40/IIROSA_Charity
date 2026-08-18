# Remaining Use Cases - Modules 4.6 through 4.18

This document contains the detailed use cases for modules 4.6 through 4.18 to be appended to the main Use Case Specification document.

---

### 4.6 Module: Periodic Orphan Reports

**Module Owner:** Charity, Admin, Super Admin  
**Purpose:** Generate reports on orphans for specified periods  
**Dependencies:** Orphans, Families, Charities  

#### Use Case UC-6.1: Generate Orphan Report

| Field | Value |
|-------|-------|
| **ID** | UC-6.1 |
| **Name** | Generate Orphan Report |
| **Actor** | Charity, Admin, Super Admin |
| **Priority** | High |
| **Description** | Create periodic report for registered orphans. Charity sees ONLY their orphans |

**Preconditions:**
- User is logged in
- Orphans exist in system

**Main Flow:**
1. User navigates to Orphan Reports page
2. System displays report generation form with filters:
   - **Period:**
     - From Date (required, date picker)
     - To Date (required, date picker)
   - **Filters:**
     - Charity (dropdown: All or specific) - Admin/Super Admin only
     - Region (dropdown: All or specific)
     - Center (dropdown: All or specific)
     - Sponsorship Status (dropdown: Sponsored, Unsponsored, All)
     - Age Range (from/to)
     - Gender (Male/Female/All)
   - **Report Options:**
     - Include Family Details (checkbox)
     - Include Contact Information (checkbox)
     - Include Education Details (checkbox)
     - Include Health Details (checkbox)
     - Group By Charity (checkbox for Admin/Super Admin)
3. User sets report period (From/To dates)
4. User applies desired filters
5. User selects report options
6. User clicks "Generate Report"
7. System validates date range (From <= To)
8. **For Charity:**
   - System queries orphans WHERE CharityId = CurrentUser.CharityId
9. **For Admin/Super Admin:**
   - System queries all orphans (with optional Charity filter)
10. System applies all selected filters
11. System generates report with:
    - Report metadata (period, filters, generation date)
    - Summary statistics (total orphans, by status, by age group, by gender)
    - Detailed orphan list (according to options)
    - Charity breakdown (for Admin/Super Admin)
12. System displays report in preview
13. System provides export options (Excel, PDF)
14. System logs report generation in audit log
15. User can view, export, or print report

**Alternative Flows:**
- **7a. Invalid date range:** System displays error "From date must be before To date"
- **8a. No orphans found:** System displays message "No orphans found matching criteria"

**Business Rules:**
- Charity: Report shows only orphans where CharityId = CurrentUser.CharityId
- Admin/SuperAdmin: Report shows all orphans with optional Charity filter
- Report data filtered by selected period and filters

**Postconditions:**
- Report generated and displayed
- Audit log contains report generation record
- Report available for export/print

---

#### Use Case UC-6.2: Set Report Period

| Field | Value |
|-------|-------|
| **ID** | UC-6.2 |
| **Name** | Set Report Period |
| **Actor** | Charity, Admin, Super Admin |
| **Priority** | Medium |
| **Description** | Specify start and end date for orphan report |

**Preconditions:**
- User is logged in
- On Orphan Reports page

**Main Flow:**
1. User navigates to Orphan Reports page
2. System displays report generation form
3. User selects From Date (date picker)
4. User selects To Date (date picker)
5. System validates date range
6. System displays selected period in preview

**Postconditions:**
- Report period set
- Used for filtering orphan data

---

#### Use Case UC-6.3: Filter Orphans by Status

| Field | Value |
|-------|-------|
| **ID** | UC-6.3 |
| **Name** | Filter Orphans by Status |
| **Actor** | Charity, Admin, Super Admin |
| **Priority** | Medium |
| **Description** | Generate report for active/sponsored orphans only |

**Preconditions:**
- User is logged in
- On Orphan Reports page

**Main Flow:**
1. User navigates to Orphan Reports page
2. System displays Sponsorship Status dropdown
3. User selects status:
   - Sponsored
   - Unsponsored
   - Pending
   - All
4. System applies filter to orphan query
5. Report includes only matching orphans

**Postconditions:**
- Report filtered by sponsorship status

---

#### Use Case UC-6.4: Filter Orphans by Charity

| Field | Value |
|-------|-------|
| **ID** | UC-6.4 |
| **Name** | Filter Orphans by Charity |
| **Actor** | Admin, Super Admin |
| **Priority** | Medium |
| **Description** | Generate report for specific charity (dropdown filter). Charity sees only their own |

**Preconditions:**
- Admin or Super Admin is logged in
- On Orphan Reports page

**Main Flow:**
1. Admin/Super Admin navigates to Orphan Reports page
2. System displays Charity dropdown with options:
   - All Charities (default)
   - [Charity 1]
   - [Charity 2]
   - ... (all active charities)
3. User selects specific charity
4. System filters orphans WHERE CharityId = SelectedCharityId
5. Report includes only orphans from selected charity

**Business Rules:**
- Admin/SuperAdmin only (Charity users don't see this filter)
- Charity users automatically filtered to own charity

**Postconditions:**
- Report filtered by charity
- Summary statistics reflect selected charity

---

#### Use Case UC-6.5: Filter Orphans by Region

| Field | Value |
|-------|-------|
| **ID** | UC-6.5 |
| **Name** | Filter Orphans by Region |
| **Actor** | Charity, Admin, Super Admin |
| **Priority** | Medium |
| **Description** | Generate report for specific geographic region |

**Preconditions:**
- User is logged in
- On Orphan Reports page

**Main Flow:**
1. User navigates to Orphan Reports page
2. System displays Region dropdown with options:
   - All Regions (default)
   - [Region 1]
   - [Region 2]
   - ... (all active regions)
3. User selects specific region
4. System filters orphans by region (through charity's center assignment)
5. Report includes only orphans from selected region

**Postconditions:**
- Report filtered by region

---

#### Use Case UC-6.6: Include Family Details

| Field | Value |
|-------|-------|
| **ID** | UC-6.6 |
| **Name** | Include Family Details |
| **Actor** | Charity, Admin, Super Admin |
| **Priority** | Low |
| **Description** | Generate report with orphan family information |

**Preconditions:**
- User is logged in
- On Orphan Reports page

**Main Flow:**
1. User navigates to Orphan Reports page
2. System displays "Include Family Details" checkbox
3. User checks checkbox
4. System includes in report:
   - Family Address
   - Father Name (if exists)
   - Mother Name (if exists)
   - Provider Information
   - Family Phone
5. Report enhanced with family context

**Postconditions:**
- Report includes family information

---

#### Use Case UC-6.7: Export Orphan Report

| Field | Value |
|-------|-------|
| **ID** | UC-6.7 |
| **Name** | Export Orphan Report |
| **Actor** | Charity, Admin, Super Admin |
| **Priority** | High |
| **Description** | Export report to Excel/PDF format |

**Preconditions:**
- User is logged in
- Report has been generated

**Main Flow:**
1. User generates orphan report
2. System displays report preview
3. System provides "Export" button with format options:
   - Excel (.xlsx)
   - PDF (.pdf)
4. User selects format
5. User clicks "Export"
6. System generates file in selected format
7. System downloads file to user's device
8. System logs export in audit log (user, format, record count)
9. System displays success message

**Postconditions:**
- Report file exported
- Audit log contains export record

---

#### Use Case UC-6.8: Schedule Recurring Report

| Field | Value |
|-------|-------|
| **ID** | UC-6.8 |
| **Name** | Schedule Recurring Report |
| **Actor** | Charity, Admin, Super Admin |
| **Priority** | Low |
| **Description** | Configure automatic report generation on periodic basis |

**Preconditions:**
- User is logged in
- Report configuration exists

**Main Flow:**
1. User navigates to Orphan Reports page
2. System provides "Schedule Report" button
3. User clicks "Schedule Report"
4. System displays scheduling form:
   - Report Name (required)
   - Frequency (dropdown: Monthly, Quarterly, Annually)
   - Day of Month (if monthly)
   - Email Report To (comma-separated emails)
   - Filters (same as ad-hoc report)
5. User fills in schedule details
6. User clicks "Save Schedule"
7. System creates scheduled report configuration
8. System adds to background job scheduler
9. System logs schedule creation in audit log
10. System displays success message
11. Report will be generated automatically according to schedule

**Postconditions:**
- Scheduled report created
- Report generated automatically
- Emailed to recipients

---

#### Use Case UC-6.9: View Report History

| Field | Value |
|-------|-------|
| **ID** | UC-6.9 |
| **Name** | View Report History |
| **Actor** | Charity, Admin, Super Admin |
| **Priority** | Low |
| **Description** | View previously generated orphan reports. Charity sees only their reports |

**Preconditions:**
- User is logged in
- Reports have been generated

**Main Flow:**
1. User navigates to Orphan Reports page
2. System displays "Report History" tab/section
3. System displays list of previously generated reports with columns:
   - Report Name/Period
   - Generation Date
   - Generated By
   - Filters Applied
   - Orphan Count
   - Actions (View, Export, Delete)
4. **For Charity:**
   - System shows only reports generated by charity users
5. **For Admin/Super Admin:**
   - System shows all reports
6. User can click report to view details
7. User can re-export report
8. User can delete report (with confirmation)

**Postconditions:**
- Report history displayed
- Past reports accessible

---

#### Use Case UC-6.10: Compare Period Reports

| Field | Value |
|-------|-------|
| **ID** | UC-6.10 |
| **Name** | Compare Period Reports |
| **Actor** | Admin, Super Admin |
| **Priority** | Low |
| **Description** | Compare orphan statistics between different periods across all charities |

**Preconditions:**
- Admin or Super Admin is logged in
- At least two reports exist

**Main Flow:**
1. Admin/Super Admin navigates to Orphan Reports page
2. System provides "Compare Reports" button
3. User clicks "Compare Reports"
4. System displays comparison form:
   - Select Report 1 (dropdown from history)
   - Select Report 2 (dropdown from history)
   - Comparison Metrics (checkboxes):
     - Total Orphan Count
     - Sponsored vs Unsponsored
     - Age Distribution
     - Gender Distribution
     - Charity Distribution
5. User selects reports to compare
6. User selects metrics to compare
7. User clicks "Compare"
8. System generates comparison view showing:
   - Side-by-side statistics
   - Percentage changes
   - Visual charts/graphs
   - Charity-by-charity breakdown
9. System displays comparison results

**Postconditions:**
- Period comparison displayed
- Trends and changes visible

---

### 4.7 Module: Office Development Projects

**Module Owner:** Admin, Super Admin  
**Purpose:** Track office development projects (Charity CANNOT access)  
**Dependencies:** Centers, Regions, OfficeProjectTypes, Charities  

**Important Note:** Charity users CANNOT access this module. Only Admin and Super Admin can manage office development projects.

#### Use Case UC-7.1: Create Office Project

| Field | Value |
|-------|-------|
| **ID** | UC-7.1 |
| **Name** | Create Office Project |
| **Actor** | Admin, Super Admin |
| **Priority** | High |
| **Description** | Register new development project with name, type, and location (Charity CANNOT access) |

**Preconditions:**
- Admin or Super Admin is logged in
- Project types, centers, regions exist

**Main Flow:**
1. User navigates to Office Development Projects page
2. System displays list of projects with "Add New Project" button
3. User clicks "Add New Project"
4. System displays project creation form with sections:
   - **Basic Information:**
     - Project Name (required)
     - Project Hint/Description
     - Project Date (default: today)
   - **Project Classification:**
     - Office Project Type (dropdown from lookup)
   - **Location:**
     - Country (dropdown from lookup)
     - Region (dropdown, filtered by Country)
     - Center (dropdown, filtered by Region)
     - Village Name
   - **Financial Information:**
     - Project Cost in Egyptian Pounds (decimal)
     - Project Cost in Saudi Riyals (decimal)
     - Donor Name
   - **Beneficiaries:**
     - Beneficiaries Count (number)
     - Beneficiaries Type (dropdown: Families, Individuals, Both)
   - **Charity Assignment:**
     - Assigned Charity (dropdown: None or specific charity) - optional
   - **Documents:**
     - Attached File (upload project proposal/document)
     - Project Report File (upload completion report)
   - **Status:**
     - Is Finished (checkbox, default: false)
     - Project End Date (if finished)
5. User fills in required fields
6. System validates data
7. System creates project record
8. System logs creation in audit log
9. System displays success message
10. Project appears in project list

**Alternative Flows:**
- **6a. Validation fails:** System highlights missing required fields

**Postconditions:**
- Project record created
- Audit log contains creation record

---

#### Use Case UC-7.2: Set Project Budget

| Field | Value |
|-------|-------|
| **ID** | UC-7.2 |
| **Name** | Set Project Budget |
| **Actor** | Admin, Super Admin |
| **Priority** | High |
| **Description** | Specify project cost in Egyptian Pounds and Saudi Riyals |

**Preconditions:**
- Admin or Super Admin is logged in
- Project exists

**Main Flow:**
1. User navigates to Office Development Projects page
2. System displays list of projects
3. User selects project
4. System displays project details with "Financial Information" section
5. User enters budget amounts:
   - Project Cost in EGP (decimal)
   - Project Cost in SAR (decimal)
6. User optionally enters Donor Name
7. User clicks "Save"
8. System validates amounts (non-negative)
9. System updates project financial details
10. System logs changes in audit log
11. System displays success message

**Postconditions:**
- Project budget set
- Used for financial reporting

---

#### Use Case UC-7.3: Specify Project Donor

| Field | Value |
|-------|-------|
| **ID** | UC-7.3 |
| **Name** | Specify Project Donor |
| **Actor** | Admin, Super Admin |
| **Priority** | Medium |
| **Description** | Record donor name and details for project |

**Preconditions:**
- Admin or Super Admin is logged in
- Project exists

**Main Flow:**
1. User navigates to Office Development Projects page
2. System displays list of projects
3. User selects project
4. System displays project details
5. User enters Donor Name
6. User optionally adds donor details in notes
7. User clicks "Save"
8. System updates project with donor information
9. System logs change in audit log
10. System displays success message

**Postconditions:**
- Donor information recorded

---

#### Use Case UC-7.4: Set Beneficiaries Count

| Field | Value |
|-------|-------|
| **ID** | UC-7.4 |
| **Name** | Set Beneficiaries Count |
| **Actor** | Admin, Super Admin |
| **Priority** | Medium |
| **Description** | Specify number and type of beneficiaries for project |

**Preconditions:**
- Admin or Super Admin is logged in
- Project exists

**Main Flow:**
1. User navigates to Office Development Projects page
2. System displays list of projects
3. User selects project
4. System displays project details with "Beneficiaries" section
5. User enters:
   - Beneficiaries Count (number)
   - Beneficiaries Type (dropdown: Families, Individuals, Both)
6. User clicks "Save"
7. System validates count (positive integer)
8. System updates project beneficiary details
9. System logs change in audit log
10. System displays success message

**Postconditions:**
- Beneficiary details recorded

---

#### Use Case UC-7.5: Assign Project Location

| Field | Value |
|-------|-------|
| **ID** | UC-7.5 |
| **Name** | Assign Project Location |
| **Actor** | Admin, Super Admin |
| **Priority** | High |
| **Description** | Set village name, region, and center for project |

**Preconditions:**
- Admin or Super Admin is logged in
- Project exists
- Geographic lookup data exists

**Main Flow:**
1. User navigates to Office Development Projects page
2. System displays list of projects
3. User selects project
4. System displays project details with "Location" section
5. User selects:
   - Country (dropdown)
   - Region (dropdown, filtered by Country)
   - Center (dropdown, filtered by Region)
   - Village Name (text)
6. User clicks "Save"
7. System updates project location
8. System logs change in audit log
9. System displays success message

**Postconditions:**
- Project location assigned
- Used for geographic reporting

---

#### Use Case UC-7.6: Attach Project Documents

| Field | Value |
|-------|-------|
| **ID** | UC-7.6 |
| **Name** | Attach Project Documents |
| **Actor** | Admin, Super Admin |
| **Priority** | Medium |
| **Description** | Upload project proposal and related files (FK_AttachedFile) |

**Preconditions:**
- Admin or Super Admin is logged in
- Project exists

**Main Flow:**
1. User navigates to Office Development Projects page
2. System displays list of projects
3. User selects project
4. System displays project details with "Documents" section
5. User clicks "Attach Document"
6. System displays file upload dialog:
   - File selection
   - Document Type (dropdown: Proposal, Contract, Progress Report, Other)
   - Description
   - Date (default: today)
7. User selects file
8. User selects document type
9. User enters description
10. User clicks "Upload"
11. System validates file (type, size)
12. System uploads file
13. System creates attachment record linked to project
14. System logs attachment in audit log
15. System displays success message
16. Attachment visible in project documents list

**Postconditions:**
- Document attached to project
- Attachment record created

---

#### Use Case UC-7.7: Upload Project Report

| Field | Value |
|-------|-------|
| **ID** | UC-7.7 |
| **Name** | Upload Project Report |
| **Actor** | Admin, Super Admin |
| **Priority** | Medium |
| **Description** | Upload completion report for project (FK_ProjectReportFile) |

**Preconditions:**
- Admin or Super Admin is logged in
- Project exists

**Main Flow:**
1. User navigates to Office Development Projects page
2. System displays list of projects
3. User selects project
4. System displays project details
5. User clicks "Upload Project Report"
6. System displays file upload dialog:
   - Report File (required)
   - Report Type (dropdown: Completion, Progress, Final)
   - Report Date
   - Summary
7. User selects report file
8. User fills in report details
9. User clicks "Upload"
10. System validates file
11. System uploads file
12. System links file to project (FK_ProjectReportFile)
13. System updates project with report information
14. System logs upload in audit log
15. System displays success message

**Postconditions:**
- Project report uploaded
- Linked to project record

---

#### Use Case UC-7.8: Update Project Details

| Field | Value |
|-------|-------|
| **ID** | UC-7.8 |
| **Name** | Update Project Details |
| **Actor** | Admin, Super Admin |
| **Priority** | Medium |
| **Description** | Modify project information before completion |

**Preconditions:**
- Admin or Super Admin is logged in
- Project exists
- Project not marked as finished

**Main Flow:**
1. User navigates to Office Development Projects page
2. System displays list of projects
3. User selects project to edit
4. System displays project edit form with current data
5. User modifies desired fields:
   - Project Name
   - Description
   - Budget
   - Beneficiaries
   - Location
   - Notes
6. User clicks "Save"
7. System validates data
8. System updates project record
9. System logs all changes in audit log
10. System displays success message

**Business Rules:**
- Can update only if IsFinished = false
- Finished projects require special permission to modify

**Postconditions:**
- Project information updated
- Audit log contains changes

---

#### Use Case UC-7.9: Mark Project as Completed

| Field | Value |
|-------|-------|
| **ID** | UC-7.9 |
| **Name** | Mark Project as Completed |
| **Actor** | Admin, Super Admin |
| **Priority** | High |
| **Description** | Set project as finished with completion date (IsFinished) |

**Preconditions:**
- Admin or Super Admin is logged in
- Project exists
- Project not already completed

**Main Flow:**
1. User navigates to Office Development Projects page
2. System displays list of projects
3. User selects project to complete
4. System displays project details with "Mark as Completed" button
5. User clicks "Mark as Completed"
6. System displays confirmation dialog: "Mark project as completed? This will prevent further modifications."
7. User confirms
8. System sets project.IsFinished = true
9. System sets project.ProjectEndDate = current date
10. System logs completion in audit log
11. System displays success message
12. Project status shows as "Completed"
13. Project becomes read-only (or requires special permission to edit)

**Alternative Flows:**
- **7a. User cancels:** System returns without changes

**Postconditions:**
- Project marked as completed
- Completion date recorded
- Audit log contains completion record

---

#### Use Case UC-7.10: View Project List

| Field | Value |
|-------|-------|
| **ID** | UC-7.10 |
| **Name** | View Project List |
| **Actor** | Admin, Super Admin |
| **Priority** | Low |
| **Description** | View all projects with filtering by status, type, region |

**Preconditions:**
- Admin or Super Admin is logged in

**Main Flow:**
1. User navigates to Office Development Projects page
2. System displays list of projects in grid with columns:
   - Project Name
   - Project Type
   - Region
   - Center
   - Village
   - Project Cost (EGP)
   - Project Cost (SAR)
   - Beneficiaries Count
   - Donor Name
   - Is Finished (Yes/No)
   - Project Date
   - Assigned Charity (if any)
3. System provides search by project name
4. System provides filter by:
   - Project Type
   - Region
   - Center
   - Is Finished
   - Assigned Charity
   - Date Range
5. System provides pagination
6. System provides sorting by any column
7. User can click project to view details

**Postconditions:**
- Project list displayed
- Filtering and search available

---

#### Use Case UC-7.11: View Project Details

| Field | Value |
|-------|-------|
| **ID** | UC-7.11 |
| **Name** | View Project Details |
| **Actor** | Admin, Super Admin |
| **Priority** | Low |
| **Description** | View complete project profile with all details and attachments |

**Preconditions:**
- Admin or Super Admin is logged in
- Project exists

**Main Flow:**
1. User navigates to Office Development Projects page
2. System displays list of projects
3. User selects project
4. System displays comprehensive project details:
   - **Basic Information:**
     - Project Name
     - Description
     - Project Date
     - Project End Date (if completed)
   - **Classification:**
     - Office Project Type
   - **Location:**
     - Country, Region, Center, Village
   - **Financial:**
     - Project Cost (EGP & SAR)
     - Donor Name
   - **Beneficiaries:**
     - Count and Type
   - **Charity Assignment:**
     - Assigned Charity (if any)
   - **Status:**
     - Is Finished
     - Completion Date
   - **Documents:**
     - List of attached files
     - Project Report (if uploaded)
   - **Actions:**
     - "Edit" button (if not finished)
     - "Mark as Completed" button (if not finished)
     - "Attach Document" button
     - "Upload Report" button
5. System displays audit trail (created, modified info)

**Postconditions:**
- Complete project details visible

---

#### Use Case UC-7.12: Track Project Progress

| Field | Value |
|-------|-------|
| **ID** | UC-7.12 |
| **Name** | Track Project Progress |
| **Actor** | Admin, Super Admin |
| **Priority** | Medium |
| **Description** | Monitor ongoing projects and completion status |

**Preconditions:**
- Admin or Super Admin is logged in
- Projects exist

**Main Flow:**
1. User navigates to Office Development Projects page
2. System provides "Project Progress" dashboard/view
3. System displays ongoing projects with:
   - Project Name
   - Start Date
   - Expected End Date
   - Progress Status (Not Started, In Progress, Completed)
   - Budget vs Actual (if tracked)
   - Beneficiaries served vs planned
   - Assigned Charity
4. System provides visual indicators (progress bars, status colors)
5. System provides filter by status
6. User can drill down to project details
7. System can generate progress report

**Postconditions:**
- Project progress monitored
- Status visible for all projects

---

#### Use Case UC-7.13: Set Project Dates

| Field | Value |
|-------|-------|
| **ID** | UC-7.13 |
| **Name** | Set Project Dates |
| **Actor** | Admin, Super Admin |
| **Priority** | Medium |
| **Description** | Specify project start date and expected/actual end date |

**Preconditions:**
- Admin or Super Admin is logged in
- Project exists

**Main Flow:**
1. User navigates to Office Development Projects page
2. System displays list of projects
3. User selects project
4. System displays project details with date fields
5. User sets/updates:
   - Project Date (start date)
   - Project End Date (actual or expected)
6. User clicks "Save"
7. System validates dates (end date >= start date)
8. System updates project dates
9. System logs changes in audit log
10. System displays success message

**Postconditions:**
- Project dates set
- Used for progress tracking

---

#### Use Case UC-7.14: Assign Project to Charity

| Field | Value |
|-------|-------|
| **ID** | UC-7.14 |
| **Name** | Assign Project to Charity |
| **Actor** | Admin, Super Admin |
| **Priority** | Medium |
| **Description** | Link project to specific charity for tracking and beneficiary assignment |

**Preconditions:**
- Admin or Super Admin is logged in
- Project exists
- Charities exist

**Main Flow:**
1. User navigates to Office Development Projects page
2. System displays list of projects
3. User selects project
4. System displays project details with "Charity Assignment" section
5. User selects Assigned Charity from dropdown:
   - None (default)
   - [Charity 1]
   - [Charity 2]
   - ... (all active charities)
6. User clicks "Save"
7. System updates project with charity assignment
8. System logs assignment in audit log
9. System displays success message
10. Project linked to charity
11. Charity can see project in their view (if applicable)

**Postconditions:**
- Project assigned to charity
- Used for filtering and reporting

---

### 4.8 Module: Missions

**Module Owner:** Admin, Super Admin  
**Purpose:** Plan and track missions and fieldwork (Charity CANNOT access)  
**Dependencies:** Centers, Regions, MissionTypes, MissionTimeTypes, Users  

**Important Note:** Charity users CANNOT access this module. Only Admin and Super Admin can manage missions.

#### Use Case UC-8.1: Create Mission

| Field | Value |
|-------|-------|
| **ID** | UC-8.1 |
| **Name** | Create Mission |
| **Actor** | Admin, Super Admin |
| **Priority** | High |
| **Description** | Plan new mission with target, details, and location (Charity CANNOT access) |

**Preconditions:**
- Admin or Super Admin is logged in
- Mission types, time types, centers, regions exist

**Main Flow:**
1. User navigates to Missions page
2. System displays list of missions with "Add New Mission" button
3. User clicks "Add New Mission"
4. System displays mission creation form with sections:
   - **Basic Information:**
     - Mission Target (required, e.g., "Site Visit", "Training Session")
     - Mission Details (description)
     - Details (additional notes)
   - **Mission Classification:**
     - Mission Type (dropdown from lookup)
     - Mission Time Type (dropdown from lookup: One-time, Recurring)
   - **Scheduling:**
     - Mission Date (required, date picker)
     - Mission Completed Date (if applicable)
   - **Location:**
     - Country (dropdown from lookup)
     - Region (dropdown, filtered by Country)
     - Center (dropdown, filtered by Region)
     - Mission Location (text address)
     - Village
   - **Assignment:**
     - Assigned To (dropdown of users)
   - **Event Information:**
     - Entity Name (if applicable)
     - Conference Name (if applicable)
   - **Status:**
     - Is Mission Completed (checkbox, default: false)
5. User fills in required fields:
   - Mission Target
   - Mission Date
   - Mission Type
   - Mission Time Type
   - Assigned To
6. System validates data
7. System creates mission record
8. System logs creation in audit log
9. System displays success message
10. Mission appears in mission list

**Postconditions:**
- Mission record created
- Audit log contains creation record

---

#### Use Case UC-8.2: Set Mission Date

| Field | Value |
|-------|-------|
| **ID** | UC-8.2 |
| **Name** | Set Mission Date |
| **Actor** | Admin, Super Admin |
| **Priority** | High |
| **Description** | Schedule mission date and time |

**Preconditions:**
- Admin or Super Admin is logged in
- Mission exists

**Main Flow:**
1. User navigates to Missions page
2. System displays list of missions
3. User selects mission
4. System displays mission details
5. User selects Mission Date (date picker)
6. User optionally sets time
7. User clicks "Save"
8. System validates date (not in past for new missions)
9. System updates mission date
10. System logs change in audit log
11. System displays success message

**Postconditions:**
- Mission date scheduled

---

#### Use Case UC-8.3: Assign Mission Type

| Field | Value |
|-------|-------|
| **ID** | UC-8.3 |
| **Name** | Assign Mission Type |
| **Actor** | Admin, Super Admin |
| **Priority** | Medium |
| **Description** | Categorize mission by type (fieldwork, conference, etc.) |

**Preconditions:**
- Admin or Super Admin is logged in
- Mission exists
- Mission types exist

**Main Flow:**
1. User navigates to Missions page
2. System displays list of missions
3. User selects mission
4. System displays mission details
5. User selects Mission Type from dropdown:
   - Fieldwork
   - Conference
   - Training
   - Meeting
   - Inspection
   - Other (from lookup)
6. User clicks "Save"
7. System updates mission type
8. System logs change in audit log
9. System displays success message

**Postconditions:**
- Mission type assigned
- Used for categorization and reporting

---

#### Use Case UC-8.4: Assign Mission Time Type

| Field | Value |
|-------|-------|
| **ID** | UC-8.4 |
| **Name** | Assign Mission Time Type |
| **Actor** | Admin, Super Admin |
| **Priority** | Medium |
| **Description** | Specify if mission is one-time or recurring |

**Preconditions:**
- Admin or Super Admin is logged in
- Mission exists
- Mission time types exist

**Main Flow:**
1. User navigates to Missions page
2. System displays list of missions
3. User selects mission
4. System displays mission details
5. User selects Mission Time Type from dropdown:
   - One-time
   - Daily
   - Weekly
   - Monthly
   - Quarterly
   - Annually
6. User clicks "Save"
7. System updates mission time type
8. System logs change in audit log
9. System displays success message

**Postconditions:**
- Mission time type set
- Used for scheduling recurring missions

---

#### Use Case UC-8.5: Set Mission Location

| Field | Value |
|-------|-------|
| **ID** | UC-8.5 |
| **Name** | Set Mission Location |
| **Actor** | Admin, Super Admin |
| **Priority** | High |
| **Description** | Specify region, center, village for mission |

**Preconditions:**
- Admin or Super Admin is logged in
- Mission exists
- Geographic data exists

**Main Flow:**
1. User navigates to Missions page
2. System displays list of missions
3. User selects mission
4. System displays mission details with "Location" section
5. User selects/fills:
   - Country (dropdown)
   - Region (dropdown, filtered by Country)
   - Center (dropdown, filtered by Region)
   - Mission Location (address text)
   - Village
6. User clicks "Save"
7. System updates mission location
8. System logs change in audit log
9. System displays success message

**Postconditions:**
- Mission location assigned
- Used for geographic tracking

---

#### Use Case UC-8.6: Assign Mission Owner

| Field | Value |
|-------|-------|
| **ID** | UC-8.6 |
| **Name** | Assign Mission Owner |
| **Actor** | Admin, Super Admin |
| **Priority** | High |
| **Description** | Assign user responsible for mission execution |

**Preconditions:**
- Admin or Super Admin is logged in
- Mission exists
- Users exist

**Main Flow:**
1. User navigates to Missions page
2. System displays list of missions
3. User selects mission
4. System displays mission details
5. User selects "Assigned To" from dropdown:
   - List of active users
6. User clicks "Save"
7. System updates mission with assigned user (FK_UserId)
8. System logs assignment in audit log
9. System displays success message
10. Assigned user receives notification
11. Mission appears in user's "My Missions" list

**Postconditions:**
- Mission assigned to user
- User notified of assignment

---

#### Use Case UC-8.7: Update Mission Details

| Field | Value |
|-------|-------|
| **ID** | UC-8.7 |
| **Name** | Update Mission Details |
| **Actor** | Admin, Super Admin |
| **Priority** | Medium |
| **Description** | Modify mission information before completion |

**Preconditions:**
- Admin or Super Admin is logged in
- Mission exists
- Mission not completed

**Main Flow:**
1. User navigates to Missions page
2. System displays list of missions
3. User selects mission to edit
4. System displays mission edit form with current data
5. User modifies desired fields:
   - Mission Target
   - Mission Details
   - Details/Notes
   - Date
   - Location
   - Type
6. User clicks "Save"
7. System validates data
8. System updates mission record
9. System logs all changes in audit log
10. System displays success message

**Postconditions:**
- Mission details updated
- Audit log contains changes

---

#### Use Case UC-8.8: Mark Mission as Completed

| Field | Value |
|-------|-------|
| **ID** | UC-8.8 |
| **Name** | Mark Mission as Completed |
| **Actor** | Admin, Super Admin |
| **Priority** | High |
| **Description** | Set mission status to completed with completion date |

**Preconditions:**
- Admin or Super Admin is logged in
- Mission exists
- Mission not already completed

**Main Flow:**
1. User navigates to Missions page
2. System displays list of missions
3. User selects mission to complete
4. System displays mission details with "Mark as Completed" button
5. User clicks "Mark as Completed"
6. System optionally prompts for completion notes
7. User enters notes (optional)
8. User confirms completion
9. System sets mission.IsMissionCompleted = true
10. System sets mission.MissionCompletedDate = current date/time
11. System sets mission.MissionCompletedTxt = completion note/text
12. System logs completion in audit log
13. System displays success message
14. Mission status shows as "Completed"
15. Mission becomes read-only

**Postconditions:**
- Mission marked as completed
- Completion date recorded
- Audit log contains completion record

---

#### Use Case UC-8.9: Record Conference/Entity

| Field | Value |
|-------|-------|
| **ID** | UC-8.9 |
| **Name** | Record Conference/Entity |
| **Actor** | Admin, Super Admin |
| **Priority** | Low |
| **Description** | Document conference name or entity name for mission |

**Preconditions:**
- Admin or Super Admin is logged in
- Mission exists

**Main Flow:**
1. User navigates to Missions page
2. System displays list of missions
3. User selects mission
4. System displays mission details with "Event Information" section
5. User enters:
   - Conference Name (if applicable)
   - Entity Name (if applicable)
6. User clicks "Save"
7. System updates mission with event information
8. System logs change in audit log
9. System displays success message

**Postconditions:**
- Event information recorded

---

#### Use Case UC-8.10: View Mission List

| Field | Value |
|-------|-------|
| **ID** | UC-8.10 |
| **Name** | View Mission List |
| **Actor** | Admin, Super Admin |
| **Priority** | Low |
| **Description** | View all missions with filtering by status, type, date |

**Preconditions:**
- Admin or Super Admin is logged in

**Main Flow:**
1. User navigates to Missions page
2. System displays list of missions in grid with columns:
   - Mission Target
   - Mission Type
   - Mission Date
   - Region
   - Center
   - Assigned To
   - Is Mission Completed (Yes/No)
   - Mission Completed Date
3. System provides search by mission target
4. System provides filter by:
   - Mission Type
   - Mission Time Type
   - Region
   - Center
   - Is Completed
   - Assigned To
   - Date Range
5. System provides pagination
6. System provides sorting by any column
7. User can click mission to view details

**Postconditions:**
- Mission list displayed
- Filtering and search available

---

#### Use Case UC-8.11: View Mission Details

| Field | Value |
|-------|-------|
| **ID** | UC-8.11 |
| **Name** | View Mission Details |
| **Actor** | Admin, Super Admin |
| **Priority** | Low |
| **Description** | View complete mission profile with all details |

**Preconditions:**
- Admin or Super Admin is logged in
- Mission exists

**Main Flow:**
1. User navigates to Missions page
2. System displays list of missions
3. User selects mission
4. System displays comprehensive mission details:
   - **Basic Information:**
     - Mission Target
     - Mission Details
     - Details/Notes
   - **Classification:**
     - Mission Type
     - Mission Time Type
   - **Scheduling:**
     - Mission Date
     - Mission Completed Date
     - Is Mission Completed
     - Mission Completed Txt
   - **Location:**
     - Country, Region, Center
     - Mission Location
     - Village
   - **Assignment:**
     - Assigned To
   - **Event:**
     - Conference Name
     - Entity Name
   - **Actions:**
     - "Edit" button (if not completed)
     - "Mark as Completed" button (if not completed)
5. System displays audit trail

**Postconditions:**
- Complete mission details visible

---

#### Use Case UC-8.12: View My Missions

| Field | Value |
|-------|-------|
| **ID** | UC-8.12 |
| **Name** | View My Missions |
| **Actor** | Admin, Super Admin |
| **Priority** | Low |
| **Description** | View missions assigned to current user |

**Preconditions:**
- User is logged in
- Missions assigned to user exist

**Main Flow:**
1. User navigates to Missions page
2. System provides "My Missions" tab/section
3. System displays missions WHERE AssignedTo = CurrentUser
4. System displays same grid columns as mission list
5. System provides same filtering options
6. User can view mission details
7. User can update mission status (if assigned)

**Postconditions:**
- User sees only assigned missions
- Easier to manage own tasks

---

#### Use Case UC-8.13: Track Mission Status

| Field | Value |
|-------|-------|
| **ID** | UC-8.13 |
| **Name** | Track Mission Status |
| **Actor** | Admin, Super Admin |
| **Priority** | Medium |
| **Description** | Monitor ongoing and pending missions |

**Preconditions:**
- Admin or Super Admin is logged in
- Missions exist

**Main Flow:**
1. User navigates to Missions page
2. System provides "Mission Status" dashboard
3. System displays missions grouped by status:
   - Pending (not started)
   - In Progress (started, not completed)
   - Completed
   - Overdue (past due date, not completed)
4. System provides counts for each status
5. System provides visual indicators (color coding)
6. User can filter by status
7. User can drill down to mission details
8. System can generate status report

**Postconditions:**
- Mission status visible
- Easy monitoring of all missions

---

### 4.9 Module: Seasonal Aid

**Module Owner:** Admin, Super Admin  
**Purpose:** Manage seasonal aid campaigns (Charity CANNOT access)  
**Dependencies:** Charities, Centers, Regions, Families  

**Important Note:** Charity users CANNOT access this module. Only Admin and Super Admin can manage seasonal aid campaigns.

#### Use Case UC-9.1: Create Seasonal Aid Campaign

| Field | Value |
|-------|-------|
| **ID** | UC-9.1 |
| **Name** | Create Seasonal Aid Campaign |
| **Actor** | Admin, Super Admin |
| **Priority** | High |
| **Description** | Launch new seasonal aid program (Ramadan, Eid, Winter, etc.) (Charity CANNOT access) |

**Preconditions:**
- Admin or Super Admin is logged in
- Charities exist

**Main Flow:**
1. User navigates to Seasonal Aid page
2. System displays list of campaigns with "Add New Campaign" button
3. User clicks "Add New Campaign"
4. System displays campaign creation form with sections:
   - **Basic Information:**
     - Campaign Name (required, e.g., "Ramadan 2026 Aid")
     - Campaign Type (dropdown: Ramadan, Eid Al-Fitr, Eid Al-Adha, Winter, School Supplies, Other)
     - Description
     - Start Date (required)
     - End Date (required)
   - **Financial Information:**
     - Total Budget (decimal)
     - Budget Currency (dropdown: EGP, SAR, USD)
     - Per-Family Allocation (decimal)
   - **Geographic Scope:**
     - Country (dropdown)
     - Region (dropdown, multi-select)
     - Center (dropdown, multi-select)
   - **Charity Assignment:**
     - Assigned Charity (dropdown: All or specific charity)
   - **Beneficiary Criteria:**
     - Maximum Families (number)
     - Family Type (dropdown: All, Orphan Families, Needy Families)
     - Age Range (children)
   - **Status:**
     - Is Active (checkbox, default: true)
     - Is Closed (checkbox, default: false)
5. User fills in required fields
6. System validates date range (End Date >= Start Date)
7. System creates campaign record
8. System logs creation in audit log
9. System displays success message
10. Campaign appears in campaign list

**Postconditions:**
- Campaign record created
- Ready for beneficiary registration

---

#### Use Case UC-9.2: Set Campaign Period

| Field | Value |
|-------|-------|
| **ID** | UC-9.2 |
| **Name** | Set Campaign Period |
| **Actor** | Admin, Super Admin |
| **Priority** | High |
| **Description** | Specify start and end dates for seasonal aid campaign |

**Preconditions:**
- Admin or Super Admin is logged in
- Campaign exists

**Main Flow:**
1. User navigates to Seasonal Aid page
2. System displays list of campaigns
3. User selects campaign
4. System displays campaign details
5. User sets/updates:
   - Start Date (date picker)
   - End Date (date picker)
6. User clicks "Save"
7. System validates date range
8. System updates campaign dates
9. System logs change in audit log
10. System displays success message

**Postconditions:**
- Campaign period set
- Used for beneficiary eligibility

---

#### Use Case UC-9.3: Allocate Campaign Budget

| Field | Value |
|-------|-------|
| **ID** | UC-9.3 |
| **Name** | Allocate Campaign Budget |
| **Actor** | Admin, Super Admin |
| **Priority** | High |
| **Description** | Set budget and resources for seasonal aid |

**Preconditions:**
- Admin or Super Admin is logged in
- Campaign exists

**Main Flow:**
1. User navigates to Seasonal Aid page
2. System displays list of campaigns
3. User selects campaign
4. System displays campaign details with "Financial Information" section
5. User enters:
   - Total Budget (decimal)
   - Budget Currency (dropdown)
   - Per-Family Allocation (decimal)
6. User clicks "Save"
7. System validates budget (positive numbers)
8. System updates campaign budget
9. System logs change in audit log
10. System displays success message
11. Budget visible in campaign details

**Postconditions:**
- Campaign budget allocated
- Used for tracking expenses

---

#### Use Case UC-9.4: Register Beneficiary for Aid

| Field | Value |
|-------|-------|
| **ID** | UC-9.4 |
| **Name** | Register Beneficiary for Aid |
| **Actor** | Admin, Super Admin |
| **Priority** | High |
| **Description** | Add families/individuals to receive seasonal aid |

**Preconditions:**
- Admin or Super Admin is logged in
- Campaign exists and is active
- Families exist

**Main Flow:**
1. User navigates to Seasonal Aid page
2. System displays list of campaigns
3. User selects campaign
4. System displays campaign details with "Beneficiaries" section
5. User clicks "Add Beneficiaries"
6. System displays beneficiary selection page with:
   - **Filters:**
     - Charity (dropdown)
     - Region (dropdown)
     - Center (dropdown)
     - Family Type (dropdown)
   - **Family List:**
     - Checkbox for each eligible family
     - Family Code
     - Family Address
     - Orphan Count
     - Charity
   - **Search:** By family code or address
7. User applies filters
8. User selects families via checkboxes or "Select All"
9. User clicks "Add Selected Families"
10. System creates campaign-beneficiary mappings
11. System calculates required budget (families × allocation)
12. System logs additions in audit log
13. System displays success message with count and budget impact
14. Beneficiaries visible in campaign details

**Postconditions:**
- Families registered as beneficiaries
- Campaign budget impact calculated

---

#### Use Case UC-9.5: Record Aid Distribution

| Field | Value |
|-------|-------|
| **ID** | UC-9.5 |
| **Name** | Record Aid Distribution |
| **Actor** | Admin, Super Admin |
| **Priority** | High |
| **Description** | Document distribution of aid to beneficiaries |

**Preconditions:**
- Admin or Super Admin is logged in
- Campaign exists
- Beneficiaries registered

**Main Flow:**
1. User navigates to Seasonal Aid page
2. System displays list of campaigns
3. User selects campaign
4. System displays campaign details with beneficiaries list
5. User clicks "Record Distribution"
6. System displays distribution recording page:
   - List of registered beneficiaries
   - For each beneficiary:
     - Distributed checkbox (default: false)
     - Distribution Date (date picker, default: today)
     - Amount Distributed (decimal, default: allocation)
     - Notes
     - Received By (person name)
     - Signature (optional upload)
7. User records distribution for each beneficiary:
   - Checks "Distributed"
   - Enters date, amount, notes, recipient
   - Uploads proof/document (optional)
8. User clicks "Save Distributions"
9. System validates data
10. System creates distribution records
11. System updates beneficiary status
12. System calculates total distributed amount
13. System logs distributions in audit log
14. System displays success message with summary
15. Distribution visible in campaign details

**Postconditions:**
- Distribution recorded
- Audit trail created
- Budget tracking updated

---

#### Use Case UC-9.6: View Campaign List

| Field | Value |
|-------|-------|
| **ID** | UC-9.6 |
| **Name** | View Campaign List |
| **Actor** | Admin, Super Admin |
| **Priority** | Low |
| **Description** | View all seasonal campaigns with status |

**Preconditions:**
- Admin or Super Admin is logged in

**Main Flow:**
1. User navigates to Seasonal Aid page
2. System displays list of campaigns in grid with columns:
   - Campaign Name
   - Campaign Type
   - Start Date
   - End Date
   - Total Budget
   - Allocated Budget
   - Distributed Budget
   - Beneficiary Count
   - Status (Active/Closed)
   - Assigned Charity
3. System provides search by campaign name
4. System provides filter by:
   - Campaign Type
   - Status
   - Date Range
   - Charity
5. System provides pagination
6. System provides sorting by any column
7. User can click campaign to view details

**Postconditions:**
- Campaign list displayed
- Filtering available

---

#### Use Case UC-9.7: View Campaign Beneficiaries

| Field | Value |
|-------|-------|
| **ID** | UC-9.7 |
| **Name** | View Campaign Beneficiaries |
| **Actor** | Admin, Super Admin |
| **Priority** | Low |
| **Description** | View list of beneficiaries for specific campaign |

**Preconditions:**
- Admin or Super Admin is logged in
- Campaign exists

**Main Flow:**
1. User navigates to Seasonal Aid page
2. System displays list of campaigns
3. User selects campaign
4. System displays campaign details with "Beneficiaries" section
5. System displays list of beneficiaries with:
   - Family Code
   - Family Address
   - Charity
   - Orphan Count
   - Allocation Amount
   - Distribution Status (Pending/Distributed)
   - Distribution Date
   - Amount Distributed
6. System provides filter by:
   - Distribution Status
   - Charity
   - Region
7. System provides search by family code or address
8. User can click beneficiary to view distribution details

**Postconditions:**
- Beneficiary list displayed
- Distribution status visible

---

#### Use Case UC-9.8: Update Campaign Details

| Field | Value |
|-------|-------|
| **ID** | UC-9.8 |
| **Name** | Update Campaign Details |
| **Actor** | Admin, Super Admin |
| **Priority** | Medium |
| **Description** | Modify campaign information while active |

**Preconditions:**
- Admin or Super Admin is logged in
- Campaign exists
- Campaign not closed

**Main Flow:**
1. User navigates to Seasonal Aid page
2. System displays list of campaigns
3. User selects campaign to edit
4. System displays campaign edit form with current data
5. User modifies desired fields:
   - Campaign Name
   - Description
   - Budget
   - Allocation
   - Dates
   - Notes
6. User clicks "Save"
7. System validates data
8. System updates campaign record
9. System logs all changes in audit log
10. System displays success message

**Postconditions:**
- Campaign details updated
- Audit log contains changes

---

#### Use Case UC-9.9: Close Campaign

| Field | Value |
|-------|-------|
| **ID** | UC-9.9 |
| **Name** | Close Campaign |
| **Actor** | Admin, Super Admin |
| **Priority** | High |
| **Description** | Mark campaign as completed with summary |

**Preconditions:**
- Admin or Super Admin is logged in
- Campaign exists
- Campaign not already closed

**Main Flow:**
1. User navigates to Seasonal Aid page
2. System displays list of campaigns
3. User selects campaign to close
4. System displays campaign details with "Close Campaign" button
5. User clicks "Close Campaign"
6. System displays campaign summary:
   - Total Beneficiaries
   - Total Budget
   - Allocated Amount
   - Distributed Amount
   - Remaining Budget
   - Undistributed Beneficiaries
7. System prompts for closure notes
8. User enters notes (optional)
9. User confirms closure
10. System sets campaign.IsClosed = true
11. System sets closure date
12. System logs closure in audit log
13. System displays success message
14. Campaign status shows as "Closed"
15. Campaign becomes read-only

**Postconditions:**
- Campaign closed
- Final summary generated
- Audit log contains closure record

---

#### Use Case UC-9.10: Generate Campaign Report

| Field | Value |
|-------|-------|
| **ID** | UC-9.10 |
| **Name** | Generate Campaign Report |
| **Actor** | Admin, Super Admin |
| **Priority** | Medium |
| **Description** | Produce report on aid distribution and impact |

**Preconditions:**
- Admin or Super Admin is logged in
- Campaign exists

**Main Flow:**
1. User navigates to Seasonal Aid page
2. System displays list of campaigns
3. User selects campaign
4. System displays campaign details with "Generate Report" button
5. User clicks "Generate Report"
6. System generates campaign report with:
   - **Campaign Information:**
     - Name, Type, Dates, Budget
   - **Beneficiary Summary:**
     - Total Beneficiaries
     - By Region
     - By Charity
     - By Family Type
   - **Financial Summary:**
     - Total Budget
     - Allocated
     - Distributed
     - Remaining
   - **Distribution Details:**
     - List of beneficiaries with distribution status
     - Distribution dates
     - Amounts distributed
   - **Impact Metrics:**
     - Families served
     - Individuals served (estimated)
     - Geographic coverage
7. System displays report preview
8. System provides export options (Excel, PDF)
9. User can export or print report
10. System logs report generation in audit log

**Postconditions:**
- Campaign report generated
- Available for export/print

---

#### Use Case UC-9.11: Assign Campaign to Charity

| Field | Value |
|-------|-------|
| **ID** | UC-9.11 |
| **Name** | Assign Campaign to Charity |
| **Actor** | Admin, Super Admin |
| **Priority** | Medium |
| **Description** | Link seasonal aid campaign to specific charity for beneficiary tracking |

**Preconditions:**
- Admin or Super Admin is logged in
- Campaign exists
- Charities exist

**Main Flow:**
1. User navigates to Seasonal Aid page
2. System displays list of campaigns
3. User selects campaign
4. System displays campaign details with "Charity Assignment" section
5. User selects Assigned Charity from dropdown:
   - All Charities (default, no filter)
   - [Charity 1]
   - [Charity 2]
   - ... (all active charities)
6. User clicks "Save"
7. System updates campaign with charity assignment
8. System logs assignment in audit log
9. System displays success message
10. Campaign linked to charity
11. Beneficiary selection filtered by charity (if specified)

**Postconditions:**
- Campaign assigned to charity
- Used for filtering and beneficiary selection

---

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

**Postconditions:**
- Housing project list displayed
- Filtering available

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
