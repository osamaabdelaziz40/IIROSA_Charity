### 4.5 Module: Orphan Payments (Groups/Batches)

**Module Owner:** Admin, Super Admin  
**Purpose:** Create and manage orphan payment groups for manual payment processing  
**Dependencies:** Orphans, Charities  

**Important Notes:**
- These are **GROUPS/BATCHES of orphans**, NOT actual payment processing
- **No online payment gateway** integration
- Used for organizing orphans for **offline/manual payment processing**
- Managed by Admin/Super Admin (not Accountant-only)

#### Use Case UC-5.1: Create Orphan Payment Group

| Field | Value |
|-------|-------|
| **ID** | UC-5.1 |
| **Name** | Create Orphan Payment Group |
| **Actor** | Admin, Super Admin |
| **Priority** | High |
| **Description** | Create new orphan payment group/batch with name and date range. This is a GROUP of orphans, NOT actual payment processing |

**Preconditions:**
- Admin or Super Admin is logged in
- Orphans exist in system

**Main Flow:**
1. User navigates to Orphan Payment Groups page
2. System displays list of payment groups with "Add New Group" button
3. User clicks "Add New Group"
4. System displays payment group creation form:
   - **Basic Information:**
     - Group Name (required, e.g., "January 2026 Payments - Region A")
     - Description
     - Payment Period From (date, required)
     - Payment Period To (date, required)
     - Group Date (default: today)
   - **Financial Information:**
     - Exchange Rate (decimal, for reporting purposes only)
     - Currency (dropdown: EGP, SAR)
     - Don't Remove Rate (checkbox - locks exchange rate)
   - **Filtering Options:**
     - Charity (dropdown: All or specific charity)
     - Region (dropdown: All or specific region)
     - Center (dropdown: All or specific center)
     - Sponsorship Status (dropdown: Sponsored, All)
     - Age Range (from/to)
   - **Settings:**
     - Batch Number (auto-generated or manual)
     - Show Order (for sorting)
     - Notes
5. User fills in required fields:
   - Group Name
   - Payment Period From/To
6. User optionally sets filters for orphan selection
7. User clicks "Create Group"
8. System validates date range (From <= To)
9. System auto-generates batch number if not provided
10. System creates OrphanPayment record
11. System logs creation in audit log
12. System displays success message
13. System redirects to "Add Orphans to Group" page
14. Payment group appears in list

**Alternative Flows:**
- **8a. Invalid date range:** System displays error "From date must be before To date"

**Postconditions:**
- Payment group created
- Ready for orphan assignment
- Audit log contains creation record

---

#### Use Case UC-5.2: Set Exchange Rate

| Field | Value |
|-------|-------|
| **ID** | UC-5.2 |
| **Name** | Set Exchange Rate |
| **Actor** | Admin, Super Admin |
| **Priority** | Medium |
| **Description** | Configure currency exchange rate for payment group reporting |

**Preconditions:**
- Admin or Super Admin is logged in
- Payment group exists

**Main Flow:**
1. User navigates to Orphan Payment Groups page
2. System displays list of payment groups
3. User selects payment group
4. System displays payment group details with "Financial Information" section
5. User enters Exchange Rate (decimal, e.g., 0.21 for SAR to EGP)
6. User selects Currency (dropdown: SAR, EGP, USD)
7. User checks "Don't Remove Rate" to lock rate
8. User clicks "Save"
9. System validates exchange rate format
10. System retrieves current exchange rate
11. System updates payment group with exchange rate
12. **SIMULTANEOUSLY** System creates audit log entry (per UC-17.2):
    - AuditLogId (GUID)
    - EntityType = "OrphanPaymentGroup"
    - EntityId = OrphanPaymentId
    - Operation = Update
    - UserId = current user ID
    - UserName
    - Timestamp (UTC)
    - IpAddress
    - FieldChanges (JSON):
      ```json
      {
        "ExchangeRate": {"old": 0.20, "new": 0.21},
        "Currency": {"old": "SAR", "new": "SAR"},
        "DontRemoveRate": {"old": false, "new": true}
      }
      ```
13. System saves audit log entry
14. System displays success message
15. Exchange rate visible in group details and reports

**Postconditions:**
- Exchange rate configured
- Used for reporting calculations
- Audit log contains change

---

#### Use Case UC-5.3: Add Orphans to Payment Group

| Field | Value |
|-------|-------|
| **ID** | UC-5.3 |
| **Name** | Add Orphans to Payment Group |
| **Actor** | Admin, Super Admin |
| **Priority** | High |
| **Description** | Select orphans to include in payment group batch |

**Preconditions:**
- Admin or Super Admin is logged in
- Payment group exists
- Orphans exist

**Main Flow:**
1. User navigates to Orphan Payment Groups page
2. System displays list of payment groups
3. User selects payment group
4. System displays payment group details with "Add Orphans" button
5. User clicks "Add Orphans"
6. System displays orphan selection page with:
   - **Filters:**
     - Charity (dropdown, defaults to group's charity filter)
     - Region (dropdown)
     - Center (dropdown)
     - Sponsorship Status (dropdown: Sponsored, Unsponsored, All)
     - Age Range (from/to)
     - Gender (Male/Female/All)
   - **Orphan List:**
     - Checkbox for each orphan
     - Orphan Name
     - Family Name
     - Charity
     - Age
     - Sponsorship Status
   - **Search:** By orphan name or family name
7. User applies filters as needed
8. System displays filtered orphan list
9. User selects orphans via checkboxes:
   - "Select All" button
   - Individual checkboxes
10. User clicks "Add Selected Orphans"
11. System validates selection (at least one orphan)
12. System creates orphan-payment mappings
13. System displays selected orphans in group details
14. System shows total orphan count
15. **SIMULTANEOUSLY** System creates audit log entry (per UC-17.1):
    - AuditLogId (GUID)
    - EntityType = "OrphanPaymentMapping"
    - EntityId = MappingId
    - Operation = Create
    - UserId = current user ID
    - UserName
    - Timestamp (UTC)
    - IpAddress
    - FieldChanges (JSON):
      ```json
      {
        "OrphanPaymentGroupId": {"old": null, "new": "[PaymentGroup-GUID]"},
        "OrphanIds": {"old": null, "new": ["[Orphan1-GUID]", "[Orphan2-GUID]"]},
        "OrphanCount": {"old": null, "new": 25}
      }
      ```
16. System saves audit log entry
17. System displays success message: "Added X orphans to group"

**Alternative Flows:**
- **11a. No orphans selected:** System displays error "Select at least one orphan"
- **12a. Orphan already in group:** System skips duplicate orphans

**Postconditions:**
- Orphans added to payment group
- Mappings created
- Audit log contains additions

---

#### Use Case UC-5.4: Remove Orphan from Group

| Field | Value |
|-------|-------|
| **ID** | UC-5.4 |
| **Name** | Remove Orphan from Group |
| **Actor** | Admin, Super Admin |
| **Priority** | Medium |
| **Description** | Remove specific orphan from payment group |

**Preconditions:**
- Admin or Super Admin is logged in
- Payment group exists with orphans

**Main Flow:**
1. User navigates to Orphan Payment Groups page
2. System displays list of payment groups
3. User selects payment group
4. System displays payment group details with list of orphans
5. User clicks "Remove" button next to orphan
6. System displays confirmation: "Remove orphan from payment group?"
7. User confirms
8. System removes orphan-payment mapping
9. **SIMULTANEOUSLY** System creates audit log entry (per UC-17.3):
    - AuditLogId (GUID)
    - EntityType = "OrphanPaymentMapping"
    - EntityId = MappingId
    - Operation = Delete
    - UserId = current user ID
    - UserName
    - Timestamp (UTC)
    - IpAddress
    - FieldChanges (JSON):
      ```json
      {
        "OrphanPaymentGroupId": {"old": "[PaymentGroup-GUID]", "new": null},
        "OrphanId": {"old": "[Orphan-GUID]", "new": null},
        "RemovalReason": "Removed from payment group"
      }
      ```
10. System saves audit log entry
11. System displays updated orphan list
12. System updates orphan count
13. System displays success message

**Alternative Flows:**
- **7a. User cancels:** System returns to group details without changes

**Postconditions:**
- Orphan removed from group
- Audit log contains removal

---

#### Use Case UC-5.5: Update Payment Group

| Field | Value |
|-------|-------|
| **ID** | UC-5.5 |
| **Name** | Update Payment Group |
| **Actor** | Admin, Super Admin |
| **Priority** | Medium |
| **Description** | Modify group details including name, date range, and exchange rate |

**Preconditions:**
- Admin or Super Admin is logged in
- Payment group exists

**Main Flow:**
1. User navigates to Orphan Payment Groups page
2. System displays list of payment groups
3. User selects payment group to edit
4. System displays payment group edit form with current data
5. User modifies desired fields:
    - Group Name
    - Description
    - Payment Period From/To
    - Exchange Rate
    - Currency
    - Notes
6. User clicks "Save"
7. System validates data (especially date range)
8. System retrieves current payment group values (before update)
9. System updates payment group record
10. **SIMULTANEOUSLY** System creates audit log entry (per UC-17.2):
    - AuditLogId (GUID)
    - EntityType = "OrphanPaymentGroup"
    - EntityId = OrphanPaymentId
    - Operation = Update
    - UserId = current user ID
    - UserName
    - Timestamp (UTC)
    - IpAddress
    - FieldChanges (JSON with changed fields):
      ```json
      {
        "GroupName": {"old": "January 2026", "new": "January 2026 - Updated"},
        "ExchangeRate": {"old": 0.20, "new": 0.22},
        "Notes": {"old": null, "new": "Rate updated per central bank"}
      }
      ```
11. System saves audit log entry
12. System displays success message
13. Payment group list reflects changes

**Postconditions:**
- Payment group updated
- Audit log contains changes

---

#### Use Case UC-5.6: Lock Exchange Rate

| Field | Value |
|-------|-------|
| **ID** | UC-5.6 |
| **Name** | Lock Exchange Rate |
| **Actor** | Admin, Super Admin |
| **Priority** | Medium |
| **Description** | Lock exchange rate to prevent changes (DontRemoveRate flag) |

**Preconditions:**
- Admin or Super Admin is logged in
- Payment group exists with exchange rate

**Main Flow:**
1. User navigates to Orphan Payment Groups page
2. System displays list of payment groups
3. User selects payment group
4. System displays payment group details
5. User checks "Lock Exchange Rate" checkbox
6. User clicks "Save"
7. System retrieves current DontRemoveRate value
8. System sets payment group.DontRemoveRate = true
9. **SIMULTANEOUSLY** System creates audit log entry (per UC-17.2):
    - AuditLogId (GUID)
    - EntityType = "OrphanPaymentGroup"
    - EntityId = OrphanPaymentId
    - Operation = Update (lock action)
    - UserId = current user ID
    - UserName
    - Timestamp (UTC)
    - IpAddress
    - FieldChanges (JSON):
      ```json
      {
        "DontRemoveRate": {"old": false, "new": true},
        "LockDate": {"old": null, "new": "2026-06-02T17:00:00Z"},
        "LockedBy": "Admin User"
      }
      ```
10. System saves audit log entry
11. System displays success message
12. Exchange rate field becomes read-only
13. Exchange rate cannot be modified without unchecking

**Postconditions:**
- Exchange rate locked
- Prevents accidental changes
- Audit log contains lock record

---

#### Use Case UC-5.7: Mark Group as Uploaded

| Field | Value |
|-------|-------|
| **ID** | UC-5.7 |
| **Name** | Mark Group as Uploaded |
| **Actor** | Admin, Super Admin |
| **Priority** | Medium |
| **Description** | Mark group as uploaded/confirmed (IsBatchUploaded flag). Indicates group ready for manual processing |

**Preconditions:**
- Admin or Super Admin is logged in
- Payment group exists with orphans

**Main Flow:**
1. User navigates to Orphan Payment Groups page
2. System displays list of payment groups
3. User selects payment group
4. System displays payment group details with orphan list
5. User verifies orphan list is complete
6. User clicks "Mark as Uploaded" button
7. System displays confirmation: "Mark group as uploaded and ready for processing?"
8. User confirms
9. System sets payment group.IsBatchUploaded = true
10. System sets upload timestamp
11. **SIMULTANEOUSLY** System creates audit log entry (per UC-17.2):
    - AuditLogId (GUID)
    - EntityType = "OrphanPaymentGroup"
    - EntityId = OrphanPaymentId
    - Operation = Update (status change)
    - UserId = current user ID
    - UserName
    - Timestamp (UTC)
    - IpAddress
    - FieldChanges (JSON):
      ```json
      {
        "IsBatchUploaded": {"old": false, "new": true},
        "UploadDate": {"old": null, "new": "2026-06-02T18:00:00Z"},
        "UploadedBy": "Admin User"
      }
      ```
12. System saves audit log entry
13. System displays success message
14. Group status shows as "Uploaded"
15. Group cannot be modified (unless unmarked)

**Alternative Flows:**
- **8a. User cancels:** System returns without changes
- **User can click "Unmark as Uploaded" to revert status**

**Postconditions:**
- Group marked as uploaded
- Indicates ready for manual payment processing
- Audit log contains action

---

#### Use Case UC-5.8: View Payment Groups

| Field | Value |
|-------|-------|
| **ID** | UC-5.8 |
| **Name** | View Payment Groups |
| **Actor** | Admin, Super Admin |
| **Priority** | Low |
| **Description** | View all orphan payment groups with status and orphan counts |

**Preconditions:**
- Admin or Super Admin is logged in

**Main Flow:**
1. User navigates to Orphan Payment Groups page
2. System displays list of payment groups in grid with columns:
   - Batch Number
   - Group Name
   - Payment Period From
   - Payment Period To
   - Orphan Count
   - Exchange Rate
   - Currency
   - Is Batch Uploaded (Yes/No)
   - Group Date
   - Created By
3. System provides search by:
   - Group Name
   - Batch Number
4. System provides filter by:
   - Payment Period (date range)
   - Is Batch Uploaded
   - Charity
5. System provides sorting by any column
6. System provides pagination
7. User can click group to view details
8. User can export group list to Excel

**Postconditions:**
- Payment groups displayed
- Filtering and search available

---

#### Use Case UC-5.9: View Payment Group Details

| Field | Value |
|-------|-------|
| **ID** | UC-5.9 |
| **Name** | View Payment Group Details |
| **Actor** | Admin, Super Admin |
| **Priority** | Low |
| **Description** | View complete group profile with list of included orphans |

**Preconditions:**
- Admin or Super Admin is logged in
- Payment group exists

**Main Flow:**
1. User navigates to Orphan Payment Groups page
2. System displays list of payment groups
3. User selects payment group
4. System displays comprehensive payment group details:
   - **Group Information:**
     - Batch Number
     - Group Name
     - Description
     - Payment Period From/To
     - Group Date
   - **Financial Information:**
     - Exchange Rate
     - Currency
     - Don't Remove Rate (Yes/No)
   - **Status:**
     - Is Batch Uploaded (Yes/No)
     - Upload Date (if applicable)
   - **Statistics:**
     - Total Orphan Count
     - By Charity breakdown
     - By Region breakdown
   - **Orphans List:**
     - For each orphan:
       - Orphan Name
       - Family Name
       - Charity
       - Age
       - Sponsorship Status
       - "Remove" button
   - **Actions:**
     - "Add Orphans" button
     - "Mark as Uploaded" button
     - "Export Group" button
     - "Print Group List" button
   - **Audit Trail:**
     - Created Date, Created By
     - Last Modified Date, Modified By

**Postconditions:**
- Complete group details displayed
- Orphan list visible
- Actions available

---

#### Use Case UC-5.10: Export Payment Group Report

| Field | Value |
|-------|-------|
| **ID** | UC-5.10 |
| **Name** | Export Payment Group Report |
| **Actor** | Admin, Super Admin |
| **Priority** | Medium |
| **Description** | Generate exportable report for payment group (offline processing reference) |

**Preconditions:**
- Admin or Super Admin is logged in
- Payment group exists

**Main Flow:**
1. User navigates to Orphan Payment Groups page
2. System displays list of payment groups
3. User selects payment group
4. System displays payment group details
5. User clicks "Export Group" button
6. System displays export options:
   - Format (Excel, PDF)
   - Include Photos (Yes/No)
   - Group Orphans By (Charity, Region, None)
7. User selects export options
8. User clicks "Generate Export"
9. System generates export file with:
    - Group information header
    - Orphan list with details
    - Statistics summary
    - Exchange rate information
10. System downloads file to user's device
11. **SIMULTANEOUSLY** System creates audit log entry (per UC-17.2):
    - AuditLogId (GUID)
    - EntityType = "OrphanPaymentExport"
    - EntityId = ExportId
    - Operation = Update (export action)
    - UserId = current user ID
    - UserName
    - Timestamp (UTC)
    - IpAddress
    - FieldChanges (JSON):
      ```json
      {
        "ExportType": {"old": null, "new": "PaymentGroupReport"},
        "PaymentGroupId": {"old": null, "new": "[PaymentGroup-GUID]"},
        "Format": {"old": null, "new": "Excel"},
        "IncludePhotos": {"old": null, "new": false},
        "OrphanCount": {"old": null, "new": 150}
      }
      ```
12. System saves audit log entry
13. System displays success message

**Postconditions:**
- Export file generated and downloaded
- Used for manual/offline payment processing
- Audit log contains export record

---

#### Use Case UC-5.11: Assign Group Number

| Field | Value |
|-------|-------|
| **ID** | UC-5.11 |
| **Name** | Assign Group Number |
| **Actor** | Admin, Super Admin |
| **Priority** | Low |
| **Description** | Assign sequential batch number to payment group |

**Preconditions:**
- Admin or Super Admin is logged in
- Payment group exists

**Main Flow:**
1. User navigates to Orphan Payment Groups page
2. System displays list of payment groups
3. User selects payment group
4. System displays payment group details
5. System shows current Batch Number (auto-generated or empty)
6. User clicks "Edit Batch Number"
7. User enters batch number manually
    - OR clicks "Auto-Generate" for next sequential number
8. User clicks "Save"
9. System validates batch number uniqueness
10. System retrieves current batch number
11. System updates payment group.BatchNo
12. **SIMULTANEOUSLY** System creates audit log entry (per UC-17.2):
    - AuditLogId (GUID)
    - EntityType = "OrphanPaymentGroup"
    - EntityId = OrphanPaymentId
    - Operation = Update
    - UserId = current user ID
    - UserName
    - Timestamp (UTC)
    - IpAddress
    - FieldChanges (JSON):
      ```json
      {
        "BatchNo": {"old": null, "new": "BATCH-2026-001"},
        "AssignedBy": "Admin User",
        "AssignmentDate": "2026-06-02T19:00:00Z"
      }
      ```
13. System saves audit log entry
14. System displays success message

**Alternative Flows:**
- **9a. Batch number exists:** System displays error "Batch number already in use"

**Postconditions:**
- Batch number assigned
- Used for tracking and reference
- Audit log contains change

---

#### Use Case UC-5.12: Filter Groups by Charity

| Field | Value |
|-------|-------|
| **ID** | UC-5.12 |
| **Name** | Filter Groups by Charity |
| **Actor** | Admin, Super Admin |
| **Priority** | Medium |
| **Description** | View payment groups filtered by specific charity (dropdown filter) |

**Preconditions:**
- Admin or Super Admin is logged in
- Payment groups exist

**Main Flow:**
1. User navigates to Orphan Payment Groups page
2. System displays list of all payment groups
3. System provides "Charity" dropdown filter with options:
   - All Charities (default)
   - [Charity 1]
   - [Charity 2]
   - ... (all active charities)
4. User selects specific charity from dropdown
5. System filters payment groups to show only:
   - Groups created for this charity OR
   - Groups containing orphans from this charity
6. System updates grid with filtered results
7. System displays orphan count for filtered groups
8. User can reset filter by selecting "All Charities"

**Postconditions:**
- Groups filtered by charity
- Easier to find charity-specific groups

---

#### Use Case UC-5.13: Filter Groups by Date Range

| Field | Value |
|-------|-------|
| **ID** | UC-5.13 |
| **Name** | Filter Groups by Date Range |
| **Actor** | Admin, Super Admin |
| **Priority** | Medium |
| **Description** | View payment groups within specified date period |

**Preconditions:**
- Admin or Super Admin is logged in
- Payment groups exist

**Main Flow:**
1. User navigates to Orphan Payment Groups page
2. System displays list of all payment groups
3. System provides date range filters:
   - Payment Period From (date picker)
   - Payment Period To (date picker)
   - Group Date From (date picker)
   - Group Date To (date picker)
4. User enters desired date range
5. User clicks "Apply Filter"
6. System filters groups to show only those within date range
7. System updates grid with filtered results
8. User can clear filter by clicking "Reset"

**Postconditions:**
- Groups filtered by date range
- Easier to find period-specific groups

---
