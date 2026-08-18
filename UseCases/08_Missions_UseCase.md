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
8. **SIMULTANEOUSLY** System creates audit log entry (per UC-17.1):
    - AuditLogId (GUID)
    - EntityType = "Mission"
    - EntityId = MissionId
    - Operation = Create
    - UserId = current user ID
    - UserName
    - Timestamp (UTC)
    - IpAddress
    - FieldChanges (JSON with all mission fields):
      ```json
      {
        "MissionTarget": {"old": null, "new": "Site Visit - Cairo Branch"},
        "MissionDetails": {"old": null, "new": "Quarterly inspection visit"},
        "MissionType": {"old": null, "new": "Fieldwork"},
        "MissionTimeType": {"old": null, "new": "One-time"},
        "MissionDate": {"old": null, "new": "2026-06-15"},
        "Region": {"old": null, "new": "Cairo"},
        "Center": {"old": null, "new": "Downtown"},
        "AssignedTo": {"old": null, "new": "[User-GUID]"}
      }
      ```
9. System saves audit log entry
10. System displays success message
11. Mission appears in mission list

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
9. System retrieves current mission date
10. System updates mission date
11. **SIMULTANEOUSLY** System creates audit log entry (per UC-17.2):
    - AuditLogId (GUID)
    - EntityType = "Mission"
    - EntityId = MissionId
    - Operation = Update
    - UserId = current user ID
    - UserName
    - Timestamp (UTC)
    - IpAddress
    - FieldChanges (JSON):
      ```json
      {
        "MissionDate": {"old": "2026-06-15", "new": "2026-06-20"},
        "Reason": "Rescheduled due to weather"
      }
      ```
12. System saves audit log entry
13. System displays success message

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
7. System retrieves current mission type
8. System updates mission type
9. **SIMULTANEOUSLY** System creates audit log entry (per UC-17.2):
    - AuditLogId (GUID)
    - EntityType = "Mission"
    - EntityId = MissionId
    - Operation = Update
    - UserId = current user ID
    - UserName
    - Timestamp (UTC)
    - IpAddress
    - FieldChanges (JSON):
      ```json
      {
        "MissionType": {"old": "Fieldwork", "new": "Training"},
        "PreviousType": "Fieldwork",
        "NewType": "Training"
      }
      ```
10. System saves audit log entry
11. System displays success message

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
7. System retrieves current mission time type
8. System updates mission time type
9. **SIMULTANEOUSLY** System creates audit log entry (per UC-17.2):
    - AuditLogId (GUID)
    - EntityType = "Mission"
    - EntityId = MissionId
    - Operation = Update
    - UserId = current user ID
    - UserName
    - Timestamp (UTC)
    - IpAddress
    - FieldChanges (JSON):
      ```json
      {
        "MissionTimeType": {"old": "One-time", "new": "Monthly"},
        "FrequencyChanged": true
      }
      ```
10. System saves audit log entry
11. System displays success message

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
7. System retrieves current mission location
8. System updates mission location
9. **SIMULTANEOUSLY** System creates audit log entry (per UC-17.2):
    - AuditLogId (GUID)
    - EntityType = "Mission"
    - EntityId = MissionId
    - Operation = Update
    - UserId = current user ID
    - UserName
    - Timestamp (UTC)
    - IpAddress
    - FieldChanges (JSON):
      ```json
      {
        "Country": {"old": "Egypt", "new": "Egypt"},
        "Region": {"old": "Cairo", "new": "Alexandria"},
        "Center": {"old": "Downtown", "new": "Montaza"},
        "MissionLocation": {"old": "123 Main St", "new": "456 New St"},
        "Village": {"old": null, "new": "Smouha"}
      }
      ```
10. System saves audit log entry
11. System displays success message

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
7. System retrieves current assigned user
8. System updates mission with assigned user (FK_UserId)
9. **SIMULTANEOUSLY** System creates audit log entry (per UC-17.2):
    - AuditLogId (GUID)
    - EntityType = "Mission"
    - EntityId = MissionId
    - Operation = Update (assignment change)
    - UserId = current user ID
    - UserName
    - Timestamp (UTC)
    - IpAddress
    - FieldChanges (JSON):
      ```json
      {
        "AssignedTo": {"old": "[OldUser-GUID]", "new": "[NewUser-GUID]"},
        "PreviousAssignee": "John Doe",
        "NewAssignee": "Jane Smith",
        "AssignmentDate": "2026-06-02T21:00:00Z"
      }
      ```
10. System saves audit log entry
11. System displays success message
12. Assigned user receives notification
13. Mission appears in user's "My Missions" list

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
8. System retrieves current mission values (before update)
9. System updates mission record
10. **SIMULTANEOUSLY** System creates audit log entry (per UC-17.2):
    - AuditLogId (GUID)
    - EntityType = "Mission"
    - EntityId = MissionId
    - Operation = Update
    - UserId = current user ID
    - UserName
    - Timestamp (UTC)
    - IpAddress
    - FieldChanges (JSON with changed fields):
      ```json
      {
        "MissionTarget": {"old": "Site Visit", "new": "Site Visit & Training"},
        "MissionDetails": {"old": "Quarterly visit", "new": "Quarterly visit with staff training"},
        "Notes": {"old": null, "new": "Bring training materials"}
      }
      ```
11. System saves audit log entry
12. System displays success message

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
12. **SIMULTANEOUSLY** System creates audit log entry (per UC-17.2):
    - AuditLogId (GUID)
    - EntityType = "Mission"
    - EntityId = MissionId
    - Operation = Update (completion)
    - UserId = current user ID
    - UserName
    - Timestamp (UTC)
    - IpAddress
    - FieldChanges (JSON):
      ```json
      {
        "IsMissionCompleted": {"old": false, "new": true},
        "MissionCompletedDate": {"old": null, "new": "2026-06-02T22:00:00Z"},
        "MissionCompletedTxt": {"old": null, "new": "Successfully completed site visit and training"}
      }
      ```
13. System saves audit log entry
14. System displays success message
15. Mission status shows as "Completed"
16. Mission becomes read-only

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
7. System retrieves current event information
8. System updates mission with event information
9. **SIMULTANEOUSLY** System creates audit log entry (per UC-17.2):
    - AuditLogId (GUID)
    - EntityType = "Mission"
    - EntityId = MissionId
    - Operation = Update
    - UserId = current user ID
    - UserName
    - Timestamp (UTC)
    - IpAddress
    - FieldChanges (JSON):
      ```json
      {
        "ConferenceName": {"old": null, "new": "International Charity Conference 2026"},
        "EntityName": {"old": null, "new": "UNHCR"},
        "EventLocation": {"old": null, "new": "Cairo Convention Center"}
      }
      ```
10. System saves audit log entry
11. System displays success message

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
8. System provides "Export to Excel" button to export current filtered/sorted mission list
9. User can export list to Excel format with all displayed columns

**Postconditions:**
- Mission list displayed
- Filtering and search available
- Mission data can be exported to Excel

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


