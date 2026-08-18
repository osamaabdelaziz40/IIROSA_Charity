### 4.3 Module: Charities

**Module Owner:** Super Admin, Admin  
**Purpose:** Manage charitable organization accounts  
**Dependencies:** Centers, Regions, Countries, Banks  

#### Use Case UC-3.1: Register Charity

| Field | Value |
|-------|-------|
| **ID** | UC-3.1 |
| **Name** | Register Charity |
| **Actor** | Super Admin, Admin |
| **Priority** | High |
| **Description** | Create new charity with name, address, contact, and location details |

**Preconditions:**
- Super Admin or Admin is logged in
- Country, Region, Center lookup data exists

**Main Flow:**
1. User navigates to Charity Management page
2. System displays list of charities with "Add New" button
3. User clicks "Add New Charity"
4. System displays charity creation form with fields:
   - **Basic Information:**
     - Name (required, unique)
     - Code (optional, auto-generated if empty)
     - Charity Type (dropdown from lookup)
   - **Contact Information:**
     - Address (required)
     - Street Name
     - Village
     - City
     - Postal Code
     - Mail Box
     - Country (dropdown from lookup)
   - **Phone/Contact:**
     - Phone (required)
     - Phone2
     - Home Phone
     - Fax
     - Email (required)
   - **Location:**
     - Region (dropdown from lookup)
     - Center (dropdown from lookup, filtered by Region)
     - Map Location (GPS coordinates or link)
   - **Banking:**
     - Bank (dropdown from lookup)
     - Bank Account
     - IBAN
   - **Management:**
     - Boss Name
     - Boss Job Name
     - Boss Phone1
     - Boss Phone2
     - Responsible Job Name
     - Responsible Phone1
     - Responsible Phone2
   - **Settings:**
     - Office Icon (URL or file upload)
     - Receiving Donations (checkbox, default: true)
     - Notes (multiline text)
   - **User Account:**
     - Create User Account (checkbox, default: true)
     - Username/Email (required if creating account)
     - Default Password (auto-generated)
5. User fills in required fields
6. System validates charity name uniqueness
7. System validates required fields
8. System creates charity record
9. System creates associated user account (if checked)
10. System assigns Charity role to user
11. **SIMULTANEOUSLY** System creates audit log entry (per UC-17.1):
    - AuditLogId (GUID)
    - EntityType = "Charity"
    - EntityId = CharityId
    - Operation = Create
    - UserId = current user ID
    - UserName
    - Timestamp (UTC)
    - IpAddress
    - FieldChanges (JSON with all charity fields):
      ```json
      {
        "Name": {"old": null, "new": "Cairo Charity Org"},
        "Code": {"old": null, "new": "CAI-001"},
        "Address": {"old": null, "new": "123 Main St, Cairo"},
        "Phone": {"old": null, "new": "+202000000000"},
        "Email": {"old": null, "new": "cairo@charity.org"},
        "Region": {"old": null, "new": "Cairo"},
        "Center": {"old": null, "new": "Downtown"},
        "Bank": {"old": null, "new": "National Bank of Egypt"},
        "BankAccount": {"old": null, "new": "1234567890"},
        "IsActive": {"old": null, "new": true},
        "IsAddEnabled": {"old": null, "new": true},
        "IsUpdateEnabled": {"old": null, "new": true}
      }
      ```
12. System saves audit log entry
13. System displays success message with credentials
14. Charity appears in charity list

**Alternative Flows:**
- **6a. Charity name exists:** System displays error and highlights name field
- **9a. User creation fails:** System displays error, charity created but user account not created

**Postconditions:**
- Charity record exists in database
- Charity has user account (if created)
- Charity can log into system
- Audit log contains creation record

---

#### Use Case UC-3.2: Update Charity Details

| Field | Value |
|-------|-------|
| **ID** | UC-3.2 |
| **Name** | Update Charity Details |
| **Actor** | Super Admin, Admin |
| **Priority** | High |
| **Description** | Modify charity information including address, phone, email, and bank details |

**Preconditions:**
- Super Admin or Admin is logged in
- Charity exists

**Main Flow:**
1. User navigates to Charity Management page
2. System displays list of charities
3. User selects charity to edit
4. System displays charity edit form with current data
5. User modifies desired fields
6. User clicks "Save"
7. System validates data
8. System retrieves current charity values (before update)
9. System updates charity record
10. **SIMULTANEOUSLY** System creates audit log entry (per UC-17.2):
    - AuditLogId (GUID)
    - EntityType = "Charity"
    - EntityId = CharityId
    - Operation = Update
    - UserId = current user ID
    - UserName
    - Timestamp (UTC)
    - IpAddress
    - FieldChanges (JSON with changed fields):
      ```json
      {
        "Phone": {"old": "+202000000000", "new": "+202111111111"},
        "Email": {"old": "cairo@charity.org", "new": "info@cairocharity.org"},
        "Address": {"old": "123 Main St", "new": "456 New Address"},
        "BankAccount": {"old": "1234567890", "new": "0987654321"}
      }
      ```
11. System saves audit log entry
12. System displays success message
13. Charity list reflects changes

**Alternative Flows:**
- **7a. Validation fails:** System displays error with field highlights

**Postconditions:**
- Charity information updated
- Audit log contains all field changes

---

#### Use Case UC-3.3: Activate Charity

| Field | Value |
|-------|-------|
| **ID** | UC-3.3 |
| **Name** | Activate Charity |
| **Actor** | Super Admin, Admin |
| **Priority** | High |
| **Description** | Enable charity account to allow system access and operations |

**Preconditions:**
- Super Admin or Admin is logged in
- Charity account is inactive or locked

**Main Flow:**
1. User navigates to Charity Management page
2. System displays list of charities
3. User selects inactive charity
4. System displays charity details with "Activate" button
5. User clicks "Activate"
6. System displays confirmation dialog
7. User confirms
8. System sets charity.IsActive = true
9. System sets charity.IsLocked = false
10. **SIMULTANEOUSLY** System creates audit log entry (per UC-17.2):
    - AuditLogId (GUID)
    - EntityType = "Charity"
    - EntityId = CharityId
    - Operation = Update (activation)
    - UserId = current user ID
    - UserName
    - Timestamp (UTC)
    - IpAddress
    - FieldChanges (JSON):
      ```json
      {
        "IsActive": {"old": false, "new": true},
        "IsLocked": {"old": true, "new": false},
        "ActivationDate": {"old": null, "new": "2026-06-02T12:00:00Z"}
      }
      ```
11. System saves audit log entry
12. System displays success message
13. Charity can log into system
14. Charity list shows status as "Active"

**Postconditions:**
- Charity account is active
- Charity can authenticate
- Audit log contains activation record

---

#### Use Case UC-3.4: Deactivate Charity

| Field | Value |
|-------|-------|
| **ID** | UC-3.4 |
| **Name** | Deactivate Charity |
| **Actor** | Super Admin, Admin |
| **Priority** | High |
| **Description** | Disable charity account temporarily suspending operations |

**Preconditions:**
- Super Admin or Admin is logged in
- Charity account is active

**Main Flow:**
1. User navigates to Charity Management page
2. System displays list of charities
3. User selects charity to deactivate
4. System displays charity details with "Deactivate" button
5. User clicks "Deactivate"
6. System displays confirmation dialog: "Deactivating will prevent charity from accessing the system. Continue?"
7. User confirms
8. System sets charity.IsActive = false
9. **SIMULTANEOUSLY** System creates audit log entry (per UC-17.3):
    - AuditLogId (GUID)
    - EntityType = "Charity"
    - EntityId = CharityId
    - Operation = Delete (soft delete/deactivation)
    - UserId = current user ID
    - UserName
    - Timestamp (UTC)
    - IpAddress
    - FieldChanges (JSON):
      ```json
      {
        "IsActive": {"old": true, "new": false},
        "DeactivationDate": {"old": null, "new": "2026-06-02T12:30:00Z"},
        "DeactivationReason": "Admin initiated deactivation"
      }
      ```
10. System saves audit log entry
11. System displays success message
12. Charity cannot log into system
13. Charity data preserved
14. Charity list shows status as "Inactive"

**Alternative Flows:**
- **7a. User cancels:** System returns to charity details without changes

**Postconditions:**
- Charity account is inactive
- Charity cannot authenticate
- All charity data preserved
- Audit log contains deactivation record

---

#### Use Case UC-3.5: Change Charity Password

| Field | Value |
|-------|-------|
| **ID** | UC-3.5 |
| **Name** | Change Charity Password |
| **Actor** | Super Admin, Admin |
| **Priority** | High |
| **Description** | Reset password for charity user account |

**Preconditions:**
- Super Admin or Admin is logged in
- Charity exists with user account

**Main Flow:**
1. User navigates to Charity Management page
2. System displays list of charities
3. User selects charity
4. System displays charity details with "Reset Password" button
5. User clicks "Reset Password"
6. System displays confirmation dialog
7. User confirms
8. System generates new password (or prompts for entry)
9. System updates charity user password
10. **SIMULTANEOUSLY** System creates audit log entry (per UC-17.2):
    - AuditLogId (GUID)
    - EntityType = "Charity"
    - EntityId = CharityId
    - Operation = Update (password reset)
    - UserId = current user ID
    - UserName
    - Timestamp (UTC)
    - IpAddress
    - FieldChanges (JSON):
      ```json
      {
        "PasswordReset": {"old": "[Hashed Old Password]", "new": "[Hashed New Password]"},
        "PasswordResetDate": {"old": null, "new": "2026-06-02T13:00:00Z"},
        "ResetBy": {"old": null, "new": "Super Admin"}
      }
      ```
11. System saves audit log entry
12. System displays success message with new password
13. System sends email to charity contact with new password

**Alternative Flows:**
- **8a. Manual entry:** User enters custom password

**Postconditions:**
- Charity password is updated
- Audit log contains reset record
- Charity notified via email

---

#### Use Case UC-3.6: Enable/Disable Add Rights

| Field | Value |
|-------|-------|
| **ID** | UC-3.6 |
| **Name** | Enable/Disable Add Rights |
| **Actor** | Super Admin, Admin |
| **Priority** | Medium |
| **Description** | Control whether charity can add new records (IsAddEnabled flag) |

**Preconditions:**
- Super Admin or Admin is logged in
- Charity exists

**Main Flow:**
1. User navigates to Charity Management page
2. System displays list of charities
3. User selects charity
4. System displays charity details with "Add Rights" toggle
5. System shows current status (Enabled/Disabled)
6. User changes toggle to desired status
7. User clicks "Save"
8. System retrieves current IsAddEnabled value
9. System updates charity.IsAddEnabled flag
10. **SIMULTANEOUSLY** System creates audit log entry (per UC-17.2):
    - AuditLogId (GUID)
    - EntityType = "Charity"
    - EntityId = CharityId
    - Operation = Update (permission change)
    - UserId = current user ID
    - UserName
    - Timestamp (UTC)
    - IpAddress
    - FieldChanges (JSON):
      ```json
      {
        "IsAddEnabled": {"old": false, "new": true},
        "PermissionType": "AddRights",
        "ChangedBy": "Admin User"
      }
      ```
11. System saves audit log entry
12. System displays success message
13. Charity can/cannot add records based on setting

**Postconditions:**
- IsAddEnabled flag reflects selection
- Charity add operations controlled by flag
- Audit log contains permission change

---

#### Use Case UC-3.7: Enable/Disable Update Rights

| Field | Value |
|-------|-------|
| **ID** | UC-3.7 |
| **Name** | Enable/Disable Update Rights |
| **Actor** | Super Admin, Admin |
| **Priority** | Medium |
| **Description** | Control whether charity can modify existing records (IsUpdateEnabled flag) |

**Preconditions:**
- Super Admin or Admin is logged in
- Charity exists

**Main Flow:**
1. User navigates to Charity Management page
2. System displays list of charities
3. User selects charity
4. System displays charity details with "Update Rights" toggle
5. System shows current status (Enabled/Disabled)
6. User changes toggle to desired status
7. User clicks "Save"
8. System retrieves current IsUpdateEnabled value
9. System updates charity.IsUpdateEnabled flag
10. **SIMULTANEOUSLY** System creates audit log entry (per UC-17.2):
    - AuditLogId (GUID)
    - EntityType = "Charity"
    - EntityId = CharityId
    - Operation = Update (permission change)
    - UserId = current user ID
    - UserName
    - Timestamp (UTC)
    - IpAddress
    - FieldChanges (JSON):
      ```json
      {
        "IsUpdateEnabled": {"old": true, "new": false},
        "PermissionType": "UpdateRights",
        "ChangedBy": "Admin User"
      }
      ```
11. System saves audit log entry
12. System displays success message
13. Charity can/cannot update records based on setting

**Postconditions:**
- IsUpdateEnabled flag reflects selection
- Charity update operations controlled by flag
- Audit log contains permission change

---

#### Use Case UC-3.8: Lock Charity

| Field | Value |
|-------|-------|
| **ID** | UC-3.8 |
| **Name** | Lock Charity |
| **Actor** | Super Admin, Admin |
| **Priority** | High |
| **Description** | Lock charity account preventing all operations (IsLocked flag) |

**Preconditions:**
- Super Admin or Admin is logged in
- Charity exists

**Main Flow:**
1. User navigates to Charity Management page
2. System displays list of charities
3. User selects charity
4. System displays charity details with "Lock Account" button
5. User clicks "Lock Account"
6. System displays confirmation dialog: "Locking will prevent all charity operations. Continue?"
7. User confirms
8. System sets charity.IsLocked = true
9. **SIMULTANEOUSLY** System creates audit log entry (per UC-17.2):
    - AuditLogId (GUID)
    - EntityType = "Charity"
    - EntityId = CharityId
    - Operation = Update (lock action)
    - UserId = current user ID
    - UserName
    - Timestamp (UTC)
    - IpAddress
    - FieldChanges (JSON):
      ```json
      {
        "IsLocked": {"old": false, "new": true},
        "LockDate": {"old": null, "new": "2026-06-02T14:00:00Z"},
        "LockedBy": "Admin User"
      }
      ```
10. System saves audit log entry
11. System displays success message
12. Charity cannot perform any operations
13. Charity list shows status as "Locked"

**Alternative Flows:**
- **Unlock:** User clicks "Unlock Account" to remove lock

**Postconditions:**
- IsLocked flag = true
- Charity cannot add/update/delete records
- Audit log contains lock record

---

#### Use Case UC-3.9: Set Bank Account Details

| Field | Value |
|-------|-------|
| **ID** | UC-3.9 |
| **Name** | Set Bank Account Details |
| **Actor** | Super Admin, Admin |
| **Priority** | Medium |
| **Description** | Configure bank account, IBAN, and bank information for payments |

**Preconditions:**
- Super Admin or Admin is logged in
- Charity exists
- Bank lookup data exists

**Main Flow:**
1. User navigates to Charity Management page
2. System displays list of charities
3. User selects charity
4. System displays charity details with "Banking" section
5. User updates banking information:
   - Bank (dropdown from lookup)
   - Bank Account Number
   - IBAN
6. User clicks "Save"
7. System validates IBAN format (if provided)
8. System retrieves current banking details
9. System updates charity banking details
10. **SIMULTANEOUSLY** System creates audit log entry (per UC-17.2):
    - AuditLogId (GUID)
    - EntityType = "Charity"
    - EntityId = CharityId
    - Operation = Update (banking details)
    - UserId = current user ID
    - UserName
    - Timestamp (UTC)
    - IpAddress
    - FieldChanges (JSON):
      ```json
      {
        "Bank": {"old": "National Bank of Egypt", "new": "Banque Misr"},
        "BankAccount": {"old": "1234567890", "new": "9876543210"},
        "IBAN": {"old": "EG1234567890", "new": "EG9876543210"}
      }
      ```
11. System saves audit log entry
12. System displays success message
13. Banking information available for payment processing

**Alternative Flows:**
- **7a. Invalid IBAN:** System displays error "Invalid IBAN format"

**Postconditions:**
- Banking information updated
- Audit log contains banking changes

---

#### Use Case UC-3.10: View All Charities

| Field | Value |
|-------|-------|
| **ID** | UC-3.10 |
| **Name** | View All Charities |
| **Actor** | Super Admin, Admin |
| **Priority** | Low |
| **Description** | View list of all charities with status and contact information. Admin/Super Admin can filter by charity |

**Preconditions:**
- Super Admin or Admin is logged in

**Main Flow:**
1. User navigates to Charity Management page
2. System displays list of charities in grid with columns:
   - Code
   - Name
   - Region
   - Center
   - Phone
   - Email
   - Status (Active/Inactive/Locked)
   - Add Rights (Enabled/Disabled)
   - Update Rights (Enabled/Disabled)
   - Is Locked (Yes/No)
3. System provides search by name or code
4. System provides filter by status
5. System provides filter by region
6. System provides filter by center
7. System provides pagination
8. User can click charity to view details
9. System provides "Export to Excel" button to export current filtered/sorted charity list
10. User can export charity list to Excel format with all displayed columns

**Postconditions:**
- Charity list displayed with filters
- All charities visible to Admin/Super Admin
- Charity data can be exported to Excel

---

#### Use Case UC-3.11: View Charity Profile

| Field | Value |
|-------|-------|
| **ID** | UC-3.11 |
| **Name** | View Charity Profile |
| **Actor** | Super Admin, Admin, Charity |
| **Priority** | Low |
| **Description** | View detailed charity profile including all configured details and rights. Charity sees only their own profile |

**Preconditions:**
- User is logged in
- Charity exists (or viewing own profile if Charity role)

**Main Flow:**
1. User navigates to Charity Management (Admin/Super Admin) or My Profile (Charity)
2. **For Admin/Super Admin:**
   - System displays list of charities
   - User selects charity
3. **For Charity:**
   - System automatically displays own profile
4. System displays charity profile with sections:
   - **Basic Information:** Name, Code, Type
   - **Contact Details:** Address, Phones, Email
   - **Location:** Country, Region, Center, Map
   - **Banking:** Bank, Account, IBAN
   - **Management:** Boss and Responsible contacts
   - **Settings:** Rights, Lock status, Donation acceptance
   - **User Account:** Email, Last Login
   - **Statistics:** Family count, Orphan count (if applicable)
5. System provides "Edit" button (Admin/Super Admin only)
6. System provides "Change Password" button (Admin/Super Admin only)

**Postconditions:**
- Charity profile displayed
- Charity sees only own data
- Admin/Super Admin see all details

---

#### Use Case UC-3.12: Assign Charity to Center

| Field | Value |
|-------|-------|
| **ID** | UC-3.12 |
| **Name** | Assign Charity to Center |
| **Actor** | Super Admin, Admin |
| **Priority** | Medium |
| **Description** | Link charity to specific center/region |

**Preconditions:**
- Super Admin or Admin is logged in
- Charity exists
- Center lookup data exists

**Main Flow:**
1. User navigates to Charity Management page
2. System displays list of charities
3. User selects charity
4. System displays charity details with "Location" section
5. User updates geographic assignment:
   - Country (dropdown)
   - Region (dropdown, filtered by Country)
   - Center (dropdown, filtered by Region)
6. User clicks "Save"
7. System retrieves current charity location
8. System updates charity location
9. **SIMULTANEOUSLY** System creates audit log entry (per UC-17.2):
    - AuditLogId (GUID)
    - EntityType = "Charity"
    - EntityId = CharityId
    - Operation = Update (location change)
    - UserId = current user ID
    - UserName
    - Timestamp (UTC)
    - IpAddress
    - FieldChanges (JSON):
      ```json
      {
        "Country": {"old": "Egypt", "new": "Egypt"},
        "Region": {"old": "Cairo", "new": "Alexandria"},
        "Center": {"old": "Downtown", "new": "Montaza"}
      }
      ```
10. System saves audit log entry
11. System displays success message
12. Charity assigned to new center/region

**Postconditions:**
- Charity linked to center/region
- Audit log contains location change

---

#### Use Case UC-3.13: Manage Charity Contacts

| Field | Value |
|-------|-------|
| **ID** | UC-3.13 |
| **Name** | Manage Charity Contacts |
| **Actor** | Super Admin, Admin, Charity |
| **Priority** | Medium |
| **Description** | Update boss name, responsible person, and their contact information. Charity can update only their own |

**Preconditions:**
- User is logged in
- Charity exists

**Main Flow:**
1. User navigates to Charity Profile
2. System displays charity details with "Management Contacts" section
3. User updates management contacts:
   - Boss Name
   - Boss Job Name
   - Boss Phone1
   - Boss Phone2
   - Responsible Job Name
   - Responsible Phone1
   - Responsible Phone2
4. User clicks "Save"
5. System validates phone numbers (if provided)
6. System retrieves current contact information
7. System updates contact information
8. **SIMULTANEOUSLY** System creates audit log entry (per UC-17.2):
    - AuditLogId (GUID)
    - EntityType = "Charity"
    - EntityId = CharityId
    - Operation = Update (contact change)
    - UserId = current user ID
    - UserName
    - Timestamp (UTC)
    - IpAddress
    - FieldChanges (JSON):
      ```json
      {
        "BossName": {"old": "Mohamed Ali", "new": "Ahmed Hassan"},
        "BossPhone1": {"old": "+201000000001", "new": "+201000000002"},
        "ResponsibleJobName": {"old": "Director", "new": "Executive Director"},
        "ResponsiblePhone1": {"old": "+201000000003", "new": "+201000000004"}
      }
      ```
9. System saves audit log entry
10. System displays success message

**Business Rules:**
- Charity role can only update own contacts
- Admin/Super Admin can update any charity contacts

**Postconditions:**
- Management contacts updated
- Audit log contains changes

---

#### Use Case UC-3.14: Set Map Location

| Field | Value |
|-------|-------|
| **ID** | UC-3.14 |
| **Name** | Set Map Location |
| **Actor** | Super Admin, Admin |
| **Priority** | Low |
| **Description** | Configure GPS/map location for charity office |

**Preconditions:**
- Super Admin or Admin is logged in
- Charity exists

**Main Flow:**
1. User navigates to Charity Management page
2. System displays list of charities
3. User selects charity
4. System displays charity details with "Map Location" field
5. User enters map location:
   - GPS coordinates (latitude, longitude)
   - OR Google Maps link
   - OR other map service link
6. User clicks "Save"
7. System retrieves current map location
8. System updates charity.NgoMapLocation
9. **SIMULTANEOUSLY** System creates audit log entry (per UC-17.2):
    - AuditLogId (GUID)
    - EntityType = "Charity"
    - EntityId = CharityId
    - Operation = Update (map location)
    - UserId = current user ID
    - UserName
    - Timestamp (UTC)
    - IpAddress
    - FieldChanges (JSON):
      ```json
      {
        "NgoMapLocation": {"old": "https://maps.google.com/?old", "new": "https://maps.google.com/?new"}
      }
      ```
10. System saves audit log entry
11. System displays success message
12. Map location available for viewing

**Postconditions:**
- Map location saved
- Audit log contains change

---

