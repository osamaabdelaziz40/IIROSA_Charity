# 02 - Employees Use Case

### 4.2 Module: Employees

**Module Owner:** Super Admin, Admin  
**Purpose:** Manage employee accounts and assignments  
**Dependencies:** User & Role Management  

---

#### Use Case UC-2.1: Add Employee

| Field | Value |
|-------|-------|
| **ID** | UC-2.1 |
| **Name** | Add Employee |
| **Actor** | Super Admin, Admin |
| **Priority** | High |
| **Description** | Register new employee with personal details, position, and role assignment |

**Preconditions:**
- Super Admin or Admin is logged in
- Employee role exists

**Main Flow:**
1. User navigates to Employee Management page
2. System displays list of employees with "Add New" button
3. User clicks "Add New Employee"
4. System displays employee creation form with fields:
   - Email (required, unique)
   - First Name (required)
   - Last Name (required)
   - Employee Code (optional)
   - Position/Job Title (required)
   - Department (dropdown)
   - Phone Number (optional)
   - Assigned Role (dropdown: Employee, or other applicable roles)
   - Date of Employment (default: today)
5. User fills in required fields
6. System validates email uniqueness
7. System validates required fields
8. System creates employee user account
9. System assigns selected role
10. System links employee to user account
11. **SIMULTANEOUSLY** System creates audit log entry (per UC-17.1):
    - AuditLogId (GUID)
    - EntityType = "Employee"
    - EntityId = EmployeeId
    - Operation = Create
    - UserId = current user ID
    - UserName
    - Timestamp (UTC)
    - IpAddress
    - FieldChanges (JSON with all employee fields):
      ```json
      {
        "Email": {"old": null, "new": "employee@example.com"},
        "FirstName": {"old": null, "new": "Ahmed"},
        "LastName": {"old": null, "new": "Mohamed"},
        "EmployeeCode": {"old": null, "new": "EMP-2026-001"},
        "Position": {"old": null, "new": "Case Worker"},
        "Department": {"old": null, "new": "Field Operations"},
        "AssignedRole": {"old": null, "new": "Employee"}
      }
      ```
12. System saves audit log entry
13. System displays success message
14. Employee appears in employee list

**Postconditions:**
- Employee account exists
- Employee has user credentials
- Employee has assigned role
- Audit log contains creation record

---

#### Use Case UC-2.2: Update Employee Information

| Field | Value |
|-------|-------|
| **ID** | UC-2.2 |
| **Name** | Update Employee Information |
| **Actor** | Super Admin, Admin |
| **Priority** | Medium |
| **Description** | Modify employee details including contact information and job title |

**Preconditions:**
- Super Admin or Admin is logged in
- Employee exists

**Main Flow:**
1. User navigates to Employee Management page
2. System displays list of employees
3. User selects employee to edit
4. System displays employee edit form with current data
5. User modifies desired fields:
   - First Name
   - Last Name
   - Position/Job Title
   - Department
   - Phone Number
   - Assigned Role
6. User clicks "Save"
7. System validates data
8. System retrieves current employee values (before update)
9. System updates employee record
10. **SIMULTANEOUSLY** System creates audit log entry (per UC-17.2):
    - AuditLogId (GUID)
    - EntityType = "Employee"
    - EntityId = EmployeeId
    - Operation = Update
    - UserId = current user ID
    - UserName
    - Timestamp (UTC)
    - IpAddress
    - FieldChanges (JSON with changed fields):
      ```json
      {
        "Position": {"old": "Case Worker", "new": "Senior Case Worker"},
        "Department": {"old": "Field Operations", "new": "Management"},
        "Phone": {"old": "+201000000000", "new": "+201111111111"}
      }
      ```
11. System saves audit log entry
12. System displays success message
13. Employee list reflects changes

**Postconditions:**
- Employee information is updated
- Audit log contains changes

---

#### Use Case UC-2.3: Deactivate Employee

| Field | Value |
|-------|-------|
| **ID** | UC-2.3 |
| **Name** | Deactivate Employee |
| **Actor** | Super Admin, Admin |
| **Priority** | Medium |
| **Description** | Deactivate employee account while preserving records |

**Preconditions:**
- Super Admin or Admin is logged in
- Employee is active

**Main Flow:**
1. User navigates to Employee Management page
2. System displays list of employees
3. User selects employee to deactivate
4. System displays employee details with "Deactivate" button
5. User clicks "Deactivate"
6. System displays confirmation dialog
7. User confirms
8. System sets employee user.IsActive = false
9. **SIMULTANEOUSLY** System creates audit log entry (per UC-17.3):
    - AuditLogId (GUID)
    - EntityType = "Employee"
    - EntityId = EmployeeId
    - Operation = Delete (soft delete/deactivation)
    - UserId = current user ID
    - UserName
    - Timestamp (UTC)
    - IpAddress
    - FieldChanges (JSON):
      ```json
      {
        "IsActive": {"old": true, "new": false},
        "DeactivationDate": {"old": null, "new": "2026-06-02T11:00:00Z"},
        "DeactivatedBy": "Admin User"
      }
      ```
10. System saves audit log entry
11. System displays success message
12. Employee cannot log into system
13. Employee data preserved

**Postconditions:**
- Employee account is deactivated
- Employee cannot authenticate
- Data preserved for reporting

---

#### Use Case UC-2.4: Reset Employee Password

| Field | Value |
|-------|-------|
| **ID** | UC-2.4 |
| **Name** | Reset Employee Password |
| **Actor** | Super Admin, Admin |
| **Priority** | High |
| **Description** | Change password for employee accounts |

**Preconditions:**
- Super Admin or Admin is logged in
- Employee exists

**Main Flow:**
1. User navigates to Employee Management page
2. System displays list of employees
3. User selects employee
4. System displays employee details with "Reset Password" button
5. User clicks "Reset Password"
6. System displays confirmation dialog
7. User confirms
8. System generates or prompts for new password
9. System updates employee password
10. **SIMULTANEOUSLY** System creates audit log entry (per UC-17.2):
    - AuditLogId (GUID)
    - EntityType = "Employee"
    - EntityId = EmployeeId
    - Operation = Update (password reset)
    - UserId = current user ID
    - UserName
    - Timestamp (UTC)
    - IpAddress
    - FieldChanges (JSON):
      ```json
      {
        "PasswordReset": {"old": "[Hashed Old Password]", "new": "[Hashed New Password]"},
        "PasswordResetDate": {"old": null, "new": "2026-06-02T11:15:00Z"},
        "ResetBy": {"old": null, "new": "Admin User"}
      }
      ```
11. System saves audit log entry
12. System displays success message
13. Employee receives email with new password

**Postconditions:**
- Employee password is changed
- Audit log contains reset record
- Employee notified via email

---

#### Use Case UC-2.5: View Employees List

| Field | Value |
|-------|-------|
| **ID** | UC-2.5 |
| **Name** | View Employees List |
| **Actor** | Super Admin, Admin |
| **Priority** | Low |
| **Description** | View all employees with filtering and search capabilities |

**Preconditions:**
- Super Admin or Admin is logged in

**Main Flow:**
1. User navigates to Employee Management page
2. System displays list of employees in grid with columns:
   - Employee Code
   - Full Name
   - Email
   - Position
   - Department
   - Role
   - Status (Active/Inactive)
   - Date of Employment
3. System provides search by name or email
4. System provides filter by department
5. System provides filter by role
6. System provides filter by status
7. System provides pagination
8. User can click employee to view details
9. System provides "Export to Excel" button to export current filtered/sorted employee list
10. User can export list to Excel format with all displayed columns

**Postconditions:**
- Employee list displayed with filters
- Employee data can be exported to Excel

---

#### Use Case UC-2.6: Assign Employee Role

| Field | Value |
|-------|-------|
| **ID** | UC-2.6 |
| **Name** | Assign Employee Role |
| **Actor** | Super Admin, Admin |
| **Priority** | High |
| **Description** | Assign specific roles to employee determining module access |

**Preconditions:**
- Super Admin or Admin is logged in
- Employee exists
- Roles exist

**Main Flow:**
1. User navigates to Employee Management page
2. System displays list of employees
3. User selects employee
4. System displays employee details with current role
5. User clicks "Change Role"
6. System displays role dropdown
7. User selects new role
8. System retrieves current employee role
9. User clicks "Save"
10. System updates employee role
11. **SIMULTANEOUSLY** System creates audit log entry (per UC-17.2):
    - AuditLogId (GUID)
    - EntityType = "Employee"
    - EntityId = EmployeeId
    - Operation = Update (role change)
    - UserId = current user ID
    - UserName
    - Timestamp (UTC)
    - IpAddress
    - FieldChanges (JSON):
      ```json
      {
        "AssignedRole": {"old": "Employee", "new": "Accountant"},
        "PreviousRole": "Employee",
        "NewRole": "Accountant"
      }
      ```
12. System saves audit log entry
13. System displays success message
14. Employee permissions updated immediately

**Postconditions:**
- Employee has new role
- Employee access reflects new permissions
- Audit log contains role change

---

#### Use Case UC-2.7: View Employee Profile

| Field | Value |
|-------|-------|
| **ID** | UC-2.7 |
| **Name** | View Employee Profile |
| **Actor** | Super Admin, Admin, Employee |
| **Priority** | Low |
| **Description** | View detailed employee profile with assigned roles and permissions |

**Preconditions:**
- User is logged in
- Employee exists (or viewing own profile)

**Main Flow:**
1. User navigates to Employee Management page
2. System displays list of employees (if Admin/Super Admin)
3. User selects employee (or views own profile)
4. System displays employee profile with:
   - Personal Information
   - Contact Details
   - Position and Department
   - Assigned Role(s)
   - Permissions (derived from role)
   - Employment Date
   - Activity History (if Admin/Super Admin)
5. System provides "Edit" button (if Admin/Super Admin)

**Postconditions:**
- Employee profile displayed
- Permissions visible

---
