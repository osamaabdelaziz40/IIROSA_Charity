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
8. **SIMULTANEOUSLY** System creates audit log entry (per UC-17.1):
    - AuditLogId (GUID)
    - EntityType = "OfficeProject"
    - EntityId = ProjectId
    - Operation = Create
    - UserId = current user ID
    - UserName
    - Timestamp (UTC)
    - IpAddress
    - FieldChanges (JSON with all project fields):
      ```json
      {
        "ProjectName": {"old": null, "new": "Cairo Office Renovation"},
        "ProjectType": {"old": null, "new": "Renovation"},
        "ProjectDate": {"old": null, "new": "2026-06-01"},
        "Region": {"old": null, "new": "Cairo"},
        "Center": {"old": null, "new": "Downtown"},
        "ProjectCostEGP": {"old": null, "new": 500000},
        "ProjectCostSAR": {"old": null, "new": 100000},
        "DonorName": {"old": null, "new": "Islamic Relief"},
        "BeneficiariesCount": {"old": null, "new": 200}
      }
      ```
9. System saves audit log entry
10. System displays success message
11. Project appears in project list

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
9. System retrieves current budget values
10. System updates project financial details
11. **SIMULTANEOUSLY** System creates audit log entry (per UC-17.2):
    - AuditLogId (GUID)
    - EntityType = "OfficeProject"
    - EntityId = ProjectId
    - Operation = Update
    - UserId = current user ID
    - UserName
    - Timestamp (UTC)
    - IpAddress
    - FieldChanges (JSON):
      ```json
      {
        "ProjectCostEGP": {"old": 400000, "new": 500000},
        "ProjectCostSAR": {"old": 80000, "new": 100000},
        "DonorName": {"old": "Previous Donor", "new": "New Donor"}
      }
      ```
12. System saves audit log entry
13. System displays success message

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
8. System retrieves current beneficiary details
9. System updates project beneficiary details
10. **SIMULTANEOUSLY** System creates audit log entry (per UC-17.2):
    - AuditLogId (GUID)
    - EntityType = "OfficeProject"
    - EntityId = ProjectId
    - Operation = Update
    - UserId = current user ID
    - UserName
    - Timestamp (UTC)
    - IpAddress
    - FieldChanges (JSON):
      ```json
      {
        "BeneficiariesCount": {"old": 150, "new": 200},
        "BeneficiariesType": {"old": "Families", "new": "Both"}
      }
      ```
11. System saves audit log entry
12. System displays success message

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
7. System retrieves current project location
8. System updates project location
9. **SIMULTANEOUSLY** System creates audit log entry (per UC-17.2):
    - AuditLogId (GUID)
    - EntityType = "OfficeProject"
    - EntityId = ProjectId
    - Operation = Update
    - UserId = current user ID
    - UserName
    - Timestamp (UTC)
    - IpAddress
    - FieldChanges (JSON):
      ```json
      {
        "Country": {"old": "Egypt", "new": "Egypt"},
        "Region": {"old": "Cairo", "new": "Giza"},
        "Center": {"old": "Downtown", "new": "Pyramids"},
        "VillageName": {"old": null, "new": "Al-Haram"}
      }
      ```
10. System saves audit log entry
11. System displays success message

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
14. **SIMULTANEOUSLY** System creates audit log entry (per UC-17.1):
    - AuditLogId (GUID)
    - EntityType = "ProjectAttachment"
    - EntityId = AttachmentId
    - Operation = Create
    - UserId = current user ID
    - UserName
    - Timestamp (UTC)
    - IpAddress
    - FieldChanges (JSON):
      ```json
      {
        "FileName": {"old": null, "new": "project_proposal.pdf"},
        "DocumentType": {"old": null, "new": "Proposal"},
        "ProjectId": {"old": null, "new": "[Project-GUID]"}
      }
      ```
15. System saves audit log entry
16. System displays success message
17. Attachment visible in project documents list

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
8. System retrieves current project values (before update)
9. System updates project record
10. **SIMULTANEOUSLY** System creates audit log entry (per UC-17.2):
    - AuditLogId (GUID)
    - EntityType = "OfficeProject"
    - EntityId = ProjectId
    - Operation = Update
    - UserId = current user ID
    - UserName
    - Timestamp (UTC)
    - IpAddress
    - FieldChanges (JSON with changed fields):
      ```json
      {
        "ProjectName": {"old": "Cairo Office", "new": "Cairo Main Office"},
        "Description": {"old": null, "new": "Renovation of main office building"},
        "ProjectCostEGP": {"old": 400000, "new": 450000}
      }
      ```
11. System saves audit log entry
12. System displays success message

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
10. **SIMULTANEOUSLY** System creates audit log entry (per UC-17.2):
    - AuditLogId (GUID)
    - EntityType = "OfficeProject"
    - EntityId = ProjectId
    - Operation = Update (completion)
    - UserId = current user ID
    - UserName
    - Timestamp (UTC)
    - IpAddress
    - FieldChanges (JSON):
      ```json
      {
        "IsFinished": {"old": false, "new": true},
        "ProjectEndDate": {"old": null, "new": "2026-06-02"},
        "CompletionStatus": {"old": "In Progress", "new": "Completed"}
      }
      ```
11. System saves audit log entry
12. System displays success message
13. Project status shows as "Completed"
14. Project becomes read-only (or requires special permission to edit)

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
8. System provides "Export to Excel" button to export current filtered/sorted project list
9. User can export list to Excel format with all displayed columns

**Postconditions:**
- Project list displayed
- Filtering and search available
- Project data can be exported to Excel

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
8. System retrieves current project dates
9. System updates project dates
10. **SIMULTANEOUSLY** System creates audit log entry (per UC-17.2):
    - AuditLogId (GUID)
    - EntityType = "OfficeProject"
    - EntityId = ProjectId
    - Operation = Update
    - UserId = current user ID
    - UserName
    - Timestamp (UTC)
    - IpAddress
    - FieldChanges (JSON):
      ```json
      {
        "ProjectDate": {"old": "2026-06-01", "new": "2026-07-01"},
        "ProjectEndDate": {"old": "2026-09-30", "new": "2026-12-31"}
      }
      ```
11. System saves audit log entry
12. System displays success message

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
7. System retrieves current charity assignment
8. System updates project with charity assignment
9. **SIMULTANEOUSLY** System creates audit log entry (per UC-17.2):
    - AuditLogId (GUID)
    - EntityType = "OfficeProject"
    - EntityId = ProjectId
    - Operation = Update (charity assignment)
    - UserId = current user ID
    - UserName
    - Timestamp (UTC)
    - IpAddress
    - FieldChanges (JSON):
      ```json
      {
        "AssignedCharity": {"old": null, "new": "[Charity-GUID]"},
        "CharityName": {"old": null, "new": "Cairo Charity Org"}
      }
      ```
10. System saves audit log entry
11. System displays success message
12. Project linked to charity
13. Charity can see project in their view (if applicable)

**Postconditions:**
- Project assigned to charity
- Used for filtering and reporting

---


