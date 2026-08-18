# Use Case Specification - Charity.IIROSA System

## Document Control

| Field | Value |
|-------|-------|
| **Document Title** | Use Case Specification - Charity.IIROSA System |
| **Version** | 1.0 |
| **Date** | 2026-04-20 |
| **Author** | Business Analyst |
| **Status** | Final |
| **Project** | Charity.IIROSA Management System |

---

## Table of Contents

1. [Introduction](#1-introduction)
2. [System Overview](#2-system-overview)
3. [Actors and Stakeholders](#3-actors-and-stakeholders)
4. [Use Cases by Module](#4-use-cases-by-module)
   - 4.1 User & Role Management
   - 4.2 Employees
   - 4.3 Charities
   - 4.4 Families
   - 4.5 Orphan Payments
   - 4.6 Periodic Orphan Reports
   - 4.7 Office Development Projects
   - 4.8 Missions
   - 4.9 Seasonal Aid
   - 4.10 Housing Projects
   - 4.11 General Checks
   - 4.12 Imports & Exports (Correspondence)
   - 4.13 Technical Support
   - 4.14 Lookup Management
   - 4.15 Dynamic Page Management
   - 4.16 Localization Management
   - 4.17 Audit Logging
   - 4.18 Cross-Cutting Concerns
5. [Technical Rules and Constraints](#5-technical-rules-and-constraints)
6. [Business Rules](#6-business-rules)
7. [Non-Functional Requirements](#7-non-functional-requirements)

---

## 1. Introduction

### 1.1 Purpose

This document specifies the use cases for the Charity.IIROSA system, a comprehensive charitable organization management platform designed to manage charities (NGOs), beneficiaries (orphans and families), financial operations, development projects, missions, and organizational workflows.

### 1.2 Scope

The system supports:
- Multiple user roles with strict data isolation
- Arabic (primary) and English languages
- Comprehensive audit logging
- Dynamic page management
- Offline payment processing (no online payments)
- Geographic and organizational hierarchies

### 1.3 Definitions

| Term | Definition |
|------|------------|
| **Charity** | Charitable organization/NGO that manages families and orphans |
| **Orphan Payment Group** | A batch/group of orphans organized for manual payment processing (not online payment) |
| **Data Isolation** | Security rule where Charity users can only access their own data |
| **Audit Logging** | Comprehensive tracking of all database changes with field-level details |
| **Lookup** | Reference data table managed by Super Admin (e.g., Countries, Regions, Centers) |
| **Dynamic Page** | Page visibility controlled by Super Admin per role |

---

## 2. System Overview

### 2.1 System Architecture

**Backend:** ASP.NET Core Web API  
**Frontend:** Angular (Dark-RTL Template)  
**Database:** SQL Server with Entity Framework Core  
**Authentication:** JWT Tokens with Role-Based Access Control (RBAC)

### 2.2 Key Business Functions

| Function | Description |
|----------|-------------|
| **Charity Management** | Register, activate/deactivate, and manage charitable organizations |
| **Family & Orphan Management** | Register families with detailed structure (father, mother, orphans, provider) |
| **Data Isolation** | Charity users see only their data; Admins see all with filtering |
| **Orphan Payment Groups** | Create and manage batches of orphans for manual payment processing |
| **Development Projects** | Track office development projects (Admin/Super Admin only) |
| **Mission Management** | Plan and track missions and fieldwork (Admin/Super Admin only) |
| **Seasonal Aid** | Manage seasonal aid campaigns (Admin/Super Admin only) |
| **Housing Projects** | Track housing construction/renovation projects (Admin/Super Admin only) |
| **Financial Operations** | Manage checks and payment groups (offline, no online payments) |
| **Correspondence** | Import/export incoming and outgoing letters (excluding Charity role) |
| **Audit & Logging** | Track every database change with field-level details |
| **Lookup Management** | Super Admin manages all reference data |
| **Dynamic Pages** | Super Admin controls which pages each role can see |
| **Localization** | Arabic (primary) and English language support via JSON files |

### 2.3 Language Support

| Language | Direction | Status | File |
|----------|-----------|--------|------|
| **Arabic** | RTL (Right-to-Left) | PRIMARY (Default) | ar.json |
| **English** | LTR (Left-to-Right) | Secondary | en.json |

---

## 3. Actors and Stakeholders

### 3.1 Actors Table

| Actor ID | Actor Name | Description | Access Level |
|----------|------------|-------------|--------------|
| **ACT-001** | **Super Admin** | Highest-level system administrator with full access to all modules. Manages lookups, dynamic pages, users, roles, and global system configuration. | Global - All Data |
| **ACT-002** | **Admin** | Administrative user with access to all business modules excluding system configuration. Manages charities, employees, projects, missions, and all business operations. | Global - All Data |
| **ACT-003** | **Charity** | Charity organization user with RESTRICTED access to ONLY their own families, orphans, and related data. Cannot access other charities' data. | Single Charity Data Only |
| **ACT-004** | **Accountant** | Financial user responsible for checks management, payment groups, import/export, and financial records. Cannot access charity-specific data. | Departmental - Financial Data |
| **ACT-005** | **Financial Officer** | Senior financial role with oversight on financial operations and approvals. Primarily read-only access for reporting. | Departmental - Financial Read-Only |
| **ACT-006** | **Employee** | Regular employee with access to import/export and incoming/outgoing correspondence. Cannot access charity-specific data. | Departmental - Correspondence |
| **ACT-007** | **System** | Automated system processes for audit logging, notifications, background tasks, data validation, and security enforcement. | Background Processes |

### 3.2 Actor Permissions Matrix

| Module | Super Admin | Admin | Charity | Accountant | Financial Officer | Employee |
|--------|-------------|-------|---------|------------|-------------------|----------|
| User & Role Management | ✅ Full | ❌ | ❌ | ❌ | ❌ | ❌ |
| Employees | ✅ Full | ✅ Full | ❌ | ❌ | ❌ | ✅ View Own |
| Charities | ✅ Full | ✅ Full | ✅ View Own Only | ❌ | ❌ | ❌ |
| Families | ✅ Full (with filter) | ✅ Full (with filter) | ✅ Own Data Only | ❌ | ❌ | ❌ |
| Orphan Payment Groups | ✅ Full | ✅ Full | ❌ | ❌ | ✅ View Only | ❌ |
| Orphan Reports | ✅ Full (with filter) | ✅ Full (with filter) | ✅ Own Data Only | ❌ | ✅ View Only | ❌ |
| Office Projects | ✅ Full | ✅ Full | ❌ | ❌ | ❌ | ❌ |
| Missions | ✅ Full | ✅ Full | ❌ | ❌ | ❌ | ❌ |
| Seasonal Aid | ✅ Full | ✅ Full | ❌ | ❌ | ❌ | ❌ |
| Housing Projects | ✅ Full | ✅ Full | ❌ | ❌ | ❌ | ❌ |
| General Checks | ✅ Full | ✅ Full | ❌ | ✅ Full | ✅ View Only | ❌ |
| Import/Export (Correspondence) | ✅ Full | ✅ Full | ❌ | ✅ Full | ✅ View Only | ✅ Full |
| Technical Support | ✅ Full | ✅ Full | ✅ Create/View Own | ✅ Create/View Own | ✅ Create/View Own | ✅ Create/View Own |
| Lookup Management | ✅ Full | ❌ | ❌ | ❌ | ❌ | ❌ |
| Dynamic Page Management | ✅ Full | ❌ | ❌ | ❌ | ❌ | ❌ |
| Localization Management | ✅ Full | ❌ | ❌ | ❌ | ❌ | ❌ |
| Audit Logs | ✅ Full | ✅ Full | ❌ | ❌ | ❌ | ❌ |
| Attachments | ✅ Full | ✅ Full | ✅ Own Data | ✅ Departmental | ✅ View Only | ✅ Departmental |

---

## 4. Use Cases by Module

### 4.1 Module: User & Role Management

**Module Owner:** Super Admin  
**Purpose:** Manage system users, roles, and access control  
**Dependencies:** Framework.Identity  

#### Use Case UC-1.1: Create User

| Field | Value |
|-------|-------|
| **ID** | UC-1.1 |
| **Name** | Create User |
| **Actor** | Super Admin |
| **Priority** | High |
| **Description** | Create a new user account with role assignment, email, personal details, and default password |

**Preconditions:**
- Super Admin is logged in
- Super Admin has user creation permission

**Main Flow:**
1. Super Admin navigates to User Management page
2. System displays list of existing users with "Add New" button
3. Super Admin clicks "Add New User"
4. System displays user creation form with fields:
   - Email (required, unique)
   - First Name (required)
   - Last Name (required)
   - Phone Number (optional)
   - Role(s) (required, multi-select)
   - Default Password (auto-generated: P@ssw0rd@2022)
   - isActive (default: true)
5. Super Admin fills in required fields
6. System validates email uniqueness
7. System validates all required fields
8. System creates user account
9. System assigns selected role(s) to user
10. System sends welcome email with credentials
11. System logs user creation in audit log
12. System displays success message
13. System redirects to user list with new user visible

**Alternative Flows:**
- **6a. Email already exists:** System displays error "Email already registered" and highlights email field
- **7a. Required field missing:** System highlights missing fields and displays validation message
- **9a. Role assignment fails:** System displays error and rolls back user creation

**Postconditions:**
- New user account exists in database
- User has assigned role(s)
- Audit log contains user creation record

---

#### Use Case UC-1.2: Update User

| Field | Value |
|-------|-------|
| **ID** | UC-1.2 |
| **Name** | Update User |
| **Actor** | Super Admin |
| **Priority** | High |
| **Description** | Modify existing user information including personal details and role reassignment |

**Preconditions:**
- Super Admin is logged in
- User account exists

**Main Flow:**
1. Super Admin navigates to User Management page
2. System displays list of users
3. Super Admin selects user to edit
4. System displays user edit form with current data
5. Super Admin modifies desired fields:
   - Email (if changed, must be unique)
   - First Name
   - Last Name
   - Phone Number
   - Role(s)
   - Active status
6. Super Admin clicks "Save"
7. System validates email uniqueness (if changed)
8. System updates user record
9. System updates role assignments
10. System logs user update in audit log with field changes
11. System displays success message
12. System redirects to user list with updated data

**Alternative Flows:**
- **7a. Email conflict:** System displays error and prevents update
- **9a. Role update fails:** System displays error and rolls back changes

**Postconditions:**
- User information is updated in database
- Role assignments reflect changes
- Audit log contains update record with before/after values

---

#### Use Case UC-1.3: Deactivate User

| Field | Value |
|-------|-------|
| **ID** | UC-1.3 |
| **Name** | Deactivate User |
| **Actor** | Super Admin |
| **Priority** | High |
| **Description** | Disable user account to prevent system access while preserving data |

**Preconditions:**
- Super Admin is logged in
- User account is active

**Main Flow:**
1. Super Admin navigates to User Management page
2. System displays list of users
3. Super Admin selects user to deactivate
4. System displays user details with "Deactivate" button
5. Super Admin clicks "Deactivate"
6. System displays confirmation dialog: "Are you sure you want to deactivate this user?"
7. Super Admin confirms deactivation
8. System sets user.IsActive = false
9. System logs deactivation in audit log
10. System displays success message
11. System updates user list (user shows as Inactive)
12. User can no longer log into system

**Alternative Flows:**
- **7a. User cancels:** System returns to user details without changes

**Postconditions:**
- User account is deactivated
- User cannot authenticate
- Audit log contains deactivation record
- User data preserved in database

---

#### Use Case UC-1.4: Reset User Password

| Field | Value |
|-------|-------|
| **ID** | UC-1.4 |
| **Name** | Reset User Password |
| **Actor** | Super Admin |
| **Priority** | High |
| **Description** | Reset password for any user account in the system |

**Preconditions:**
- Super Admin is logged in
- User account exists

**Main Flow:**
1. Super Admin navigates to User Management page
2. System displays list of users
3. Super Admin selects user for password reset
4. System displays user details with "Reset Password" button
5. Super Admin clicks "Reset Password"
6. System displays confirmation dialog
7. Super Admin confirms
8. System generates new password or prompts for manual entry
9. System hashes password and updates user record
10. System logs password reset in audit log
11. System displays success message with new password
12. System sends email to user with new password

**Alternative Flows:**
- **8a. Manual password entry:** Super Admin enters custom password instead of auto-generated
- **7a. User cancels:** System returns to user details without changes

**Postconditions:**
- User password is updated
- Audit log contains password reset record
- User receives email notification
- Old password no longer valid

---

#### Use Case UC-1.5: Assign User to Role

| Field | Value |
|-------|-------|
| **ID** | UC-1.5 |
| **Name** | Assign User to Role |
| **Actor** | Super Admin |
| **Priority** | High |
| **Description** | Assign one or more roles to a user granting specific permissions |

**Preconditions:**
- Super Admin is logged in
- User account exists
- Role(s) exist

**Main Flow:**
1. Super Admin navigates to User Management page
2. System displays list of users
3. Super Admin selects user
4. System displays user details with "Manage Roles" button
5. Super Admin clicks "Manage Roles"
6. System displays list of available roles with checkboxes
7. System highlights currently assigned roles
8. Super Admin selects/deselects roles
9. Super Admin clicks "Save"
10. System updates role assignments
11. System logs role assignment changes in audit log
12. System displays success message
13. User permissions reflect new role assignments

**Alternative Flows:**
- **9a. No role selected:** System displays error "At least one role must be assigned"

**Postconditions:**
- User has assigned role(s)
- User permissions match role permissions
- Audit log contains role assignment record

---

#### Use Case UC-1.6: Create Role

| Field | Value |
|-------|-------|
| **ID** | UC-1.6 |
| **Name** | Create Role |
| **Actor** | Super Admin |
| **Priority** | Medium |
| **Description** | Define new role with specific permissions and claims |

**Preconditions:**
- Super Admin is logged in
- Super Admin has role creation permission

**Main Flow:**
1. Super Admin navigates to Role Management page
2. System displays list of existing roles
3. Super Admin clicks "Add New Role"
4. System displays role creation form with fields:
   - Role Name (required, unique)
   - Description (optional)
   - Permissions (checkbox list by module)
   - Claims (key-value pairs for granular permissions)
5. Super Admin fills in role details
6. Super Admin selects permissions for each module
7. Super Admin adds claims if needed
8. Super Admin clicks "Save"
9. System validates role name uniqueness
10. System creates role record
11. System creates role-permission mappings
12. System creates role-claim mappings
13. System logs role creation in audit log
14. System displays success message
15. Role appears in role list and user assignment dropdowns

**Alternative Flows:**
- **9a. Role name exists:** System displays error "Role name already exists"

**Postconditions:**
- New role exists in database
- Role has assigned permissions and claims
- Role available for user assignment
- Audit log contains role creation record

---

#### Use Case UC-1.7: Update Role Permissions

| Field | Value |
|-------|-------|
| **ID** | UC-1.7 |
| **Name** | Update Role Permissions |
| **Actor** | Super Admin |
| **Priority** | Medium |
| **Description** | Modify permissions and claims associated with existing role |

**Preconditions:**
- Super Admin is logged in
- Role exists

**Main Flow:**
1. Super Admin navigates to Role Management page
2. System displays list of roles
3. Super Admin selects role to edit
4. System displays role edit form with current permissions
5. Super Admin modifies permissions (checkboxes)
6. Super Admin adds/removes claims
7. Super Admin clicks "Save"
8. System updates role-permission mappings
9. System updates role-claim mappings
10. System logs permission changes in audit log
11. System displays success message
12. All users with this role receive updated permissions

**Alternative Flows:**
- **8a. Update fails:** System displays error and rolls back changes

**Postconditions:**
- Role permissions reflect changes
- All users with role have updated access
- Audit log contains permission change record

---

#### Use Case UC-1.8: View All Users

| Field | Value |
|-------|-------|
| **ID** | UC-1.8 |
| **Name** | View All Users |
| **Actor** | Super Admin, Admin |
| **Priority** | Low |
| **Description** | View list of all system users with their roles, status, and contact information |

**Preconditions:**
- Super Admin or Admin is logged in

**Main Flow:**
1. User navigates to User Management page
2. System displays list of users in grid/table with columns:
   - Email
   - Full Name
   - Role(s)
   - Status (Active/Inactive)
   - Phone Number
   - Creation Date
   - Last Login
3. System provides search functionality
4. System provides filter by role
5. System provides filter by status
6. System provides pagination
7. System provides sort by any column
8. User can click any user to view details

**Postconditions:**
- User list is displayed
- User can search, filter, and sort

---

#### Use Case UC-1.9: View User Activity

| Field | Value |
|-------|-------|
| **ID** | UC-1.9 |
| **Name** | View User Activity |
| **Actor** | Super Admin |
| **Priority** | Medium |
| **Description** | View audit log of user actions, login history, and system interactions |

**Preconditions:**
- Super Admin is logged in
- User account exists

**Main Flow:**
1. Super Admin navigates to User Management page
2. System displays list of users
3. Super Admin selects user
4. System displays user details with "View Activity" button
5. Super Admin clicks "View Activity"
6. System retrieves audit log entries for user
7. System displays activity timeline with:
   - Timestamp
   - Action type (Create, Update, Delete, Login, Logout)
   - Entity affected
   - Details/Changes
   - IP Address
8. System provides filter by date range
9. System provides filter by action type
10. System provides pagination

**Postconditions:**
- User activity history is displayed
- Super Admin can audit user actions

---

#### Use Case UC-1.10: Manage User Claims

| Field | Value |
|-------|-------|
| **ID** | UC-1.10 |
| **Name** | Manage User Claims |
| **Actor** | Super Admin |
| **Priority** | Low |
| **Description** | Assign or remove specific claims for granular permission control beyond role-based permissions |

**Preconditions:**
- Super Admin is logged in
- User account exists

**Main Flow:**
1. Super Admin navigates to User Management page
2. System displays list of users
3. Super Admin selects user
4. System displays user details with "Manage Claims" button
5. Super Admin clicks "Manage Claims"
6. System displays current claims with key-value pairs
7. System provides "Add Claim" button
8. Super Admin clicks "Add Claim"
9. System displays claim form:
   - Claim Type (dropdown or text)
   - Claim Value (text)
10. Super Admin enters claim details
11. Super Admin clicks "Save"
12. System adds claim to user
13. System logs claim addition in audit log
14. System displays updated claims list
15. Super Admin can remove claims via "Delete" button

**Postconditions:**
- User has specific claims beyond role permissions
- Audit log contains claim changes

---

### 4.2 Module: Employees

**Module Owner:** Super Admin, Admin  
**Purpose:** Manage employee accounts and assignments  
**Dependencies:** User & Role Management  

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
11. System logs employee creation in audit log
12. System displays success message
13. Employee appears in employee list

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
8. System updates employee record
9. System logs changes in audit log
10. System displays success message
11. Employee list reflects changes

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
9. System logs deactivation in audit log
10. System displays success message
11. Employee cannot log into system
12. Employee data preserved

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
10. System logs password reset in audit log
11. System displays success message
12. Employee receives email with new password

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

**Postconditions:**
- Employee list displayed with filters

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
8. User clicks "Save"
9. System updates employee role
10. System logs role change in audit log
11. System displays success message
12. Employee permissions updated immediately

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

### 4.3 Module: Charities (NGOs)

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
     - NGO Type (dropdown from lookup)
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
11. System logs charity creation in audit log
12. System displays success message with credentials
13. Charity appears in charity list

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
8. System updates charity record
9. System logs all field changes in audit log (before/after values)
10. System displays success message
11. Charity list reflects changes

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
10. System logs activation in audit log
11. System displays success message
12. Charity can log into system
13. Charity list shows status as "Active"

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
9. System logs deactivation in audit log
10. System displays success message
11. Charity cannot log into system
12. Charity data preserved
13. Charity list shows status as "Inactive"

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
10. System logs password reset in audit log
11. System displays success message with new password
12. System sends email to charity contact with new password

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
8. System updates charity.IsAddEnabled flag
9. System logs permission change in audit log
10. System displays success message
11. Charity can/cannot add records based on setting

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
8. System updates charity.IsUpdateEnabled flag
9. System logs permission change in audit log
10. System displays success message
11. Charity can/cannot update records based on setting

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
9. System logs lock action in audit log
10. System displays success message
11. Charity cannot perform any operations
12. Charity list shows status as "Locked"

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
8. System updates charity banking details
9. System log banking changes in audit log
10. System displays success message
11. Banking information available for payment processing

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
9. User can export charity list

**Postconditions:**
- Charity list displayed with filters
- All charities visible to Admin/Super Admin

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
7. System updates charity location
8. System logs location change in audit log
9. System displays success message
10. Charity assigned to new center/region

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
6. System updates contact information
7. System logs changes in audit log
8. System displays success message

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
7. System updates charity.NgoMapLocation
8. System logs change in audit log
9. System displays success message
10. Map location available for viewing

**Postconditions:**
- Map location saved
- Audit log contains change

---

### 4.4 Module: Families

**Module Owner:** Charity (data isolation enforced)  
**Purpose:** Manage family and orphan registration  
**Dependencies:** Charities, Providers  

#### Use Case UC-4.1: Register Family

| Field | Value |
|-------|-------|
| **ID** | UC-4.1 |
| **Name** | Register Family |
| **Actor** | Charity |
| **Priority** | High |
| **Description** | Add new family with general information including address and living conditions. Charity can add ONLY to their own charity |

**Preconditions:**
- Charity user is logged in
- Charity has Add rights enabled (IsAddEnabled = true)

**Main Flow:**
1. Charity user navigates to Family Management page
2. System displays list of families (only own charity's families)
3. User clicks "Add New Family"
4. System displays family creation form with sections:
   - **Family Information:**
     - Family Code (auto-generated or manual)
     - Registration Date (default: today)
     - Address (required)
     - City/Village
     - District/Area
     - Phone (optional)
     - Living Condition (dropdown)
     - Housing Type (dropdown)
     - Notes
   - **Provider Information:**
     - Provider Type (dropdown: Father, Mother, Other)
   - **Father Information** (if provider is father or to be added):
     - Full Name (required if adding)
     - National ID / Passport
     - Date of Birth
     - Education Level
     - Job
     - Health Status
     - Phone
     - Is Alive (checkbox)
     - Is Provider (checkbox)
   - **Mother Information** (if provider is mother or to be added):
     - Full Name (required if adding)
     - National ID / Passport
     - Date of Birth
     - Education Level
     - Job
     - Health Status
     - Phone
     - Is Alive (checkbox)
     - Is Provider (checkbox)
   - **Other Provider Information** (if provider is Other):
     - Full Name (required)
     - Relationship to Family (dropdown)
     - National ID / Passport
     - Phone
     - Address
   - **Initial Orphans** (optional, can add later):
     - List of orphans to add to family
5. User fills in required fields:
   - Address (required)
   - Provider Type (required)
   - At least one parent or provider information
6. System validates required fields
7. System auto-generates family code if not provided
8. System creates family record
9. System links family to current user's charity (FK_CharityId)
10. System creates father record (if provided)
11. System creates mother record (if provided)
12. System creates provider record (if other)
13. System creates orphan records (if provided)
14. System logs family creation in audit log
15. System displays success message
16. Family appears in family list (only for this charity)

**Alternative Flows:**
- **6a. Validation fails:** System highlights missing fields
- **7a. Code exists:** System generates new unique code

**Business Rules:**
- Charity can ONLY add families to own charity
- System automatically sets FK_CharityId = current user's charity
- Admin/SuperAdmin can add families to any charity (with dropdown)

**Postconditions:**
- Family record exists
- Family linked to charity
- Father, Mother, Provider records created
- Audit log contains creation record

---

#### Use Case UC-4.2: Add Family Father

| Field | Value |
|-------|-------|
| **ID** | UC-4.2 |
| **Name** | Add Family Father |
| **Actor** | Charity |
| **Priority** | Medium |
| **Description** | Register father details for a family including identification and contact |

**Preconditions:**
- Charity user is logged in
- Family exists (and belongs to charity's data)
- Charity has Update rights enabled

**Main Flow:**
1. Charity user navigates to Family Management page
2. System displays list of families (only own charity's families)
3. User selects family without father
4. System displays family details with "Add Father" button
5. User clicks "Add Father"
6. System displays father creation form:
   - Full Name (required)
   - National ID / Passport (required)
   - Date of Birth
   - Place of Birth
   - Education Level (dropdown)
   - Job
   - Monthly Income
   - Health Status (dropdown)
   - Phone
   - Is Alive (checkbox, default: true)
   - Is Provider (checkbox)
   - Death Date (if not alive)
   - Notes
7. User fills in required fields
8. System validates national ID uniqueness (optional, may have duplicates)
9. System creates father record
10. System links father to family
11. System updates family if father is provider
12. System logs father creation in audit log
13. System displays success message
14. Father visible in family details

**Postconditions:**
- Father record created
- Linked to family
- Audit log contains creation record

---

#### Use Case UC-4.3: Add Family Mother

| Field | Value |
|-------|-------|
| **ID** | UC-4.3 |
| **Name** | Add Family Mother |
| **Actor** | Charity |
| **Priority** | Medium |
| **Description** | Register mother details for a family including identification and contact |

**Preconditions:**
- Charity user is logged in
- Family exists (and belongs to charity's data)
- Charity has Update rights enabled

**Main Flow:**
1. Charity user navigates to Family Management page
2. System displays list of families (only own charity's families)
3. User selects family without mother
4. System displays family details with "Add Mother" button
5. User clicks "Add Mother"
6. System displays mother creation form:
   - Full Name (required)
   - National ID / Passport (required)
   - Date of Birth
   - Place of Birth
   - Education Level (dropdown)
   - Job
   - Monthly Income
   - Health Status (dropdown)
   - Phone
   - Is Alive (checkbox, default: true)
   - Is Provider (checkbox)
   - Death Date (if not alive)
   - Notes
7. User fills in required fields
8. System validates data
9. System creates mother record
10. System links mother to family
11. System updates family if mother is provider
12. System logs mother creation in audit log
13. System displays success message
14. Mother visible in family details

**Postconditions:**
- Mother record created
- Linked to family
- Audit log contains creation record

---

#### Use Case UC-4.4: Add Orphan to Family

| Field | Value |
|-------|-------|
| **ID** | UC-4.4 |
| **Name** | Add Orphan to Family |
| **Actor** | Charity |
| **Priority** | High |
| **Description** | Register orphan(s) linked to family with orphan-specific details |

**Preconditions:**
- Charity user is logged in
- Family exists (and belongs to charity's data)
- Charity has Update rights enabled

**Main Flow:**
1. Charity user navigates to Family Management page
2. System displays list of families (only own charity's families)
3. User selects family
4. System displays family details with "Add Orphan" button
5. User clicks "Add Orphan"
6. System displays orphan creation form:
   - **Basic Information:**
     - Full Name (required)
     - Gender (dropdown: Male/Female)
     - Date of Birth (required)
     - Place of Birth
     - National ID / Passport
     - Photo (upload)
   - **Orphan Status:**
     - Orphan Type (dropdown: Father-deceased, Mother-deceased, Both-deceased)
     - Sponsorship Status (dropdown: Sponsored, Unsponsored, Pending)
     - Sponsorship Start Date
   - **Education:**
     - Education Level (dropdown)
     - School Name
     - Grade/Class
     - Academic Performance
   - **Health:**
     - Health Status (dropdown)
     - Disabilities (if any)
     - Chronic Diseases
     - Notes
   - **Contact:**
     - Phone (if has own)
     - Email (optional)
   - **Other:**
     - Hobbies
     - Skills
     - Notes
7. User fills in required fields
8. System validates date of birth (reasonable range)
9. System creates orphan record
10. System links orphan to family
11. System links orphan to charity
12. System calculates orphan age from date of birth
13. System logs orphan creation in audit log
14. System displays success message
15. Orphan visible in family details
16. Orphan visible in orphan list

**Postconditions:**
- Orphan record created
- Linked to family and charity
- Audit log contains creation record

---

#### Use Case UC-4.5: Specify Provider Type

| Field | Value |
|-------|-------|
| **ID** | UC-4.5 |
| **Name** | Specify Provider Type |
| **Actor** | Charity |
| **Priority** | Medium |
| **Description** | Identify provider as father, mother, or other provider |

**Preconditions:**
- Charity user is logged in
- Family exists (and belongs to charity's data)
- Father or mother or other provider exists

**Main Flow:**
1. Charity user navigates to Family Management page
2. System displays list of families (only own charity's families)
3. User selects family
4. System displays family details with "Provider Information" section
5. User selects provider type from dropdown:
   - Father
   - Mother
   - Other
6. If "Other" selected:
   - System displays "Add Provider" button
   - User must add separate provider record
7. User clicks "Save"
8. System updates family provider information
9. System marks selected person as provider
10. System logs provider change in audit log
11. System displays success message

**Postconditions:**
- Provider type specified
- Correct person marked as provider
- Audit log contains change

---

#### Use Case UC-4.6: Add Non-Parent Provider

| Field | Value |
|-------|-------|
| **ID** | UC-4.6 |
| **Name** | Add Non-Parent Provider |
| **Actor** | Charity |
| **Priority** | Medium |
| **Description** | When provider is not parent, create separate provider record with details |

**Preconditions:**
- Charity user is logged in
- Family exists (and belongs to charity's data)
- Provider type = Other

**Main Flow:**
1. Charity user navigates to Family Management page
2. System displays list of families (only own charity's families)
3. User selects family
4. User sets Provider Type = "Other"
5. System displays "Add Provider" button
6. User clicks "Add Provider"
7. System displays provider creation form:
   - Full Name (required)
   - Relationship to Family (dropdown: Grandfather, Uncle, Brother, Guardian, Other)
   - National ID / Passport (required)
   - Phone (required)
   - Address
   - Job
   - Monthly Income
   - Notes
8. User fills in required fields
9. System creates provider record
10. System links provider to family
11. System marks provider as family provider
12. System logs provider creation in audit log
13. System displays success message
14. Provider visible in family details

**Postconditions:**
- Provider record created
- Linked to family
- Audit log contains creation record

---

#### Use Case UC-4.7: Verify Parent as Provider

| Field | Value |
|-------|-------|
| **ID** | UC-4.7 |
| **Name** | Verify Parent as Provider |
| **Actor** | Charity |
| **Priority** | Medium |
| **Description** | Verify and confirm that parent(s) are the actual provider(s) |

**Preconditions:**
- Charity user is logged in
- Family exists (and belongs to charity's data)
- Father or mother marked as provider

**Main Flow:**
1. Charity user navigates to Family Management page
2. System displays list of families (only own charity's families)
3. User selects family with parent as provider
4. System displays family details with "Verify Provider" section
5. System shows current provider(s):
   - Father: Is Provider (Yes/No)
   - Mother: Is Provider (Yes/No)
6. User verifies provider status:
   - Checks/unchecks "Father Is Provider"
   - Checks/unchecks "Mother Is Provider"
7. User adds verification notes (optional)
8. User clicks "Save"
9. System updates provider verification status
10. System logs verification in audit log
11. System displays success message
12. Verification date/time recorded

**Postconditions:**
- Provider status verified
- Audit log contains verification

---

#### Use Case UC-4.8: Update Family Information

| Field | Value |
|-------|-------|
| **ID** | UC-4.8 |
| **Name** | Update Family Information |
| **Actor** | Charity |
| **Priority** | Medium |
| **Description** | Modify family general information while maintaining family structure. Charity can update ONLY their own families |

**Preconditions:**
- Charity user is logged in
- Family exists (and belongs to charity's data)
- Charity has Update rights enabled (IsUpdateEnabled = true)

**Main Flow:**
1. Charity user navigates to Family Management page
2. System displays list of families (only own charity's families)
3. User selects family to edit
4. System displays family edit form with current data
5. User modifies desired fields:
   - Address
   - City/Village
   - Phone
   - Living Condition
   - Housing Type
   - Notes
6. User clicks "Save"
7. System validates data
8. System updates family record
9. System logs all field changes in audit log (before/after values)
10. System displays success message
11. Family list reflects changes

**Business Rules:**
- Charity can ONLY update own families
- Admin/SuperAdmin can update any family (with charity filter)

**Postconditions:**
- Family information updated
- Audit log contains field changes

---

#### Use Case UC-4.9: Update Father Details

| Field | Value |
|-------|-------|
| **ID** | UC-4.9 |
| **Name** | Update Father Details |
| **Actor** | Charity |
| **Priority** | Medium |
| **Description** | Modify father information for existing family |

**Preconditions:**
- Charity user is logged in
- Family exists with father (and belongs to charity's data)
- Charity has Update rights enabled

**Main Flow:**
1. Charity user navigates to Family Management page
2. System displays list of families (only own charity's families)
3. User selects family with father
4. System displays family details with father section
5. User clicks "Edit Father"
6. System displays father edit form with current data
7. User modifies desired fields:
   - Full Name
   - National ID / Passport
   - Date of Birth
   - Job
   - Health Status
   - Phone
   - Is Alive
   - Is Provider
8. User clicks "Save"
9. System validates data
10. System updates father record
11. System logs changes in audit log
12. System displays success message

**Postconditions:**
- Father information updated
- Audit log contains changes

---

#### Use Case UC-4.10: Update Mother Details

| Field | Value |
|-------|-------|
| **ID** | UC-4.10 |
| **Name** | Update Mother Details |
| **Actor** | Charity |
| **Priority** | Medium |
| **Description** | Modify mother information for existing family |

**Preconditions:**
- Charity user is logged in
- Family exists with mother (and belongs to charity's data)
- Charity has Update rights enabled

**Main Flow:**
1. Charity user navigates to Family Management page
2. System displays list of families (only own charity's families)
3. User selects family with mother
4. System displays family details with mother section
5. User clicks "Edit Mother"
6. System displays mother edit form with current data
7. User modifies desired fields:
   - Full Name
   - National ID / Passport
   - Date of Birth
   - Job
   - Health Status
   - Phone
   - Is Alive
   - Is Provider
8. User clicks "Save"
9. System validates data
10. System updates mother record
11. System logs changes in audit log
12. System displays success message

**Postconditions:**
- Mother information updated
- Audit log contains changes

---

#### Use Case UC-4.11: Update Orphan Details

| Field | Value |
|-------|-------|
| **ID** | UC-4.11 |
| **Name** | Update Orphan Details |
| **Actor** | Charity |
| **Priority** | Medium |
| **Description** | Modify orphan information within family |

**Preconditions:**
- Charity user is logged in
- Orphan exists (and belongs to charity's data)
- Charity has Update rights enabled

**Main Flow:**
1. Charity user navigates to Orphan Management page (or family details)
2. System displays list of orphans (only own charity's orphans)
3. User selects orphan to edit
4. System displays orphan edit form with current data
5. User modifies desired fields:
   - Full Name
   - Sponsorship Status
   - Education Level
   - School Name
   - Health Status
   - Phone
   - Photo
   - Notes
6. User clicks "Save"
7. System validates data
8. System updates orphan record
9. System logs changes in audit log
10. System displays success message
11. Orphan list reflects changes

**Postconditions:**
- Orphan information updated
- Audit log contains changes

---

#### Use Case UC-4.12: View Family List

| Field | Value |
|-------|-------|
| **ID** | UC-4.12 |
| **Name** | View Family List |
| **Actor** | Charity, Admin, Super Admin |
| **Priority** | Low |
| **Description** | View registered families with filtering and search. Charity sees ONLY their families. Admin/SuperAdmin see all with Charity dropdown filter |

**Preconditions:**
- User is logged in

**Main Flow:**
1. User navigates to Family Management page
2. **For Charity:**
   - System displays list of families (only own charity's families)
   - System filters by FK_CharityId = current user's charity
3. **For Admin/Super Admin:**
   - System displays list of all families
   - System provides Charity dropdown filter
   - System provides "All Charities" option
   - User can select specific charity to filter
4. System displays family grid with columns:
   - Family Code
   - Address
   - City/Village
   - Father Name (if exists)
   - Mother Name (if exists)
   - Orphan Count
   - Provider Type
   - Registration Date
5. System provides search by:
   - Family Code
   - Address
   - Father Name
   - Mother Name
6. System provides filter by:
   - Orphan Count range
   - Provider Type
   - Registration Date range
7. System provides pagination
8. System provides sorting by any column
9. User can click family to view details

**Business Rules:**
- Charity: WHERE CharityId = CurrentUser.CharityId
- Admin/SuperAdmin: All records (with optional Charity filter)

**Postconditions:**
- Family list displayed according to data isolation rules
- Charity sees only own families
- Admin/SuperAdmin can filter by charity

---

#### Use Case UC-4.13: View Family Details

| Field | Value |
|-------|-------|
| **ID** | UC-4.13 |
| **Name** | View Family Details |
| **Actor** | Charity, Admin, Super Admin |
| **Priority** | Low |
| **Description** | View complete family profile including parents, orphans, and provider. Charity sees ONLY their families |

**Preconditions:**
- User is logged in
- Family exists

**Main Flow:**
1. User navigates to Family Management page
2. **For Charity:**
   - System displays list of families (only own charity's families)
   - User can only select own families
3. **For Admin/Super Admin:**
   - System displays list of all families
   - User can select any family
4. User selects family
5. System validates user has access to this family (data isolation)
6. System displays comprehensive family profile:
   - **Family Information:**
     - Family Code
     - Registration Date
     - Address
     - Phone
     - Living Condition
     - Housing Type
     - Notes
   - **Provider Information:**
     - Provider Type
     - Provider Details
   - **Father Section:**
     - Full Name
     - National ID
     - Date of Birth
     - Job
     - Health Status
     - Is Provider
     - Contact Information
   - **Mother Section:**
     - Full Name
     - National ID
     - Date of Birth
     - Job
     - Health Status
     - Is Provider
     - Contact Information
   - **Orphans Section:**
     - List of all orphans in family
     - For each orphan: Name, Age, Sponsorship Status, Education, Health
   - **Attachments Section:**
     - List of attached documents
   - **Audit Trail:**
     - Creation date, creator
     - Last modification date, modifier
     - (For Admin/SuperAdmin only)
7. System provides "Edit" buttons (if user has edit rights)
8. System provides "Add Orphan" button (if user has add rights)
9. System provides "Attach Document" button

**Business Rules:**
- Charity: Can only view families where CharityId = CurrentUser.CharityId
- Admin/SuperAdmin: Can view all families
- System enforces data isolation at database level

**Postconditions:**
- Family details displayed according to data isolation
- Charity sees only own families
- System validates access on every request

---

#### Use Case UC-4.14: Deactivate Family

| Field | Value |
|-------|-------|
| **ID** | UC-4.14 |
| **Name** | Deactivate Family |
| **Actor** | Charity |
| **Priority** | Medium |
| **Description** | Mark family as inactive while preserving records. Charity can deactivate ONLY their families |

**Preconditions:**
- Charity user is logged in
- Family exists (and belongs to charity's data)
- Family is active

**Main Flow:**
1. Charity user navigates to Family Management page
2. System displays list of families (only own charity's families)
3. User selects family to deactivate
4. System displays family details with "Deactivate" button
5. User clicks "Deactivate"
6. System displays confirmation dialog: "Deactivating will mark family as inactive. Continue?"
7. User confirms
8. System sets family.IsActive = false
9. System sets family.IsDeleted = true (soft delete)
10. System sets deletion timestamp and user
11. System logs deactivation in audit log
12. System displays success message
13. Family no longer appears in default family list
14. Family visible in "Inactive Families" view (if enabled)

**Alternative Flows:**
- **7a. User cancels:** System returns to family details without changes

**Business Rules:**
- Charity can only deactivate own families
- Soft delete preserves all data
- Can be reactivated later

**Postconditions:**
- Family marked as inactive
- Audit log contains deactivation record
- Data preserved for reporting

---

#### Use Case UC-4.15: Attach Family Documents

| Field | Value |
|-------|-------|
| **ID** | UC-4.15 |
| **Name** | Attach Family Documents |
| **Actor** | Charity |
| **Priority** | Medium |
| **Description** | Upload and manage supporting documents for family (attachments) |

**Preconditions:**
- Charity user is logged in
- Family exists (and belongs to charity's data)

**Main Flow:**
1. Charity user navigates to Family Management page
2. System displays list of families (only own charity's families)
3. User selects family
4. System displays family details with "Attachments" section
5. User clicks "Add Attachment"
6. System displays file upload dialog:
   - File selection (browse)
   - Document Type (dropdown: ID, Birth Certificate, Proof of Residence, Other)
   - Description (optional)
   - Date (default: today)
7. User selects file
8. User selects document type
9. User enters description (optional)
10. User clicks "Upload"
11. System validates file type and size
12. System uploads file to storage
13. System creates attachment record:
   - File name
   - File path/URL
   - Document Type
   - Description
   - Upload Date
   - Uploaded By (current user)
   - FK_FamilyId
   - FK_CharityId
14. System logs attachment in audit log
15. System displays success message
16. Attachment visible in family attachments list
17. User can download attachment by clicking filename

**Alternative Flows:**
- **11a. Invalid file type:** System displays error "Invalid file type"
- **11b. File too large:** System displays error "File size exceeds limit"

**Postconditions:**
- Document attached to family
- Attachment record created
- Audit log contains attachment record

---

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
10. System updates payment group with exchange rate
11. System logs change in audit log
12. System displays success message
13. Exchange rate visible in group details and reports

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
15. System logs additions in audit log
16. System displays success message: "Added X orphans to group"

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
9. System logs removal in audit log
10. System displays updated orphan list
11. System updates orphan count
12. System displays success message

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
8. System updates payment group record
9. System logs all changes in audit log
10. System displays success message
11. Payment group list reflects changes

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
7. System sets payment group.DontRemoveRate = true
8. System logs lock action in audit log
9. System displays success message
10. Exchange rate field becomes read-only
11. Exchange rate cannot be modified without unchecking

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
11. System logs action in audit log
12. System displays success message
13. Group status shows as "Uploaded"
14. Group cannot be modified (unless unmarked)

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
11. System logs export in audit log
12. System displays success message

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
10. System updates payment group.BatchNo
11. System logs change in audit log
12. System displays success message

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

**[Document continues with remaining modules... Due to length, I'll continue with next modules]**

---

## 5. Technical Rules and Constraints

### 5.1 Entity Base Classes

| Rule ID | Rule | Application |
|---------|------|-------------|
| **TR-001** | All main business entities inherit from `FullAuditedEntityBase<Guid>` | Provides automatic audit fields: CreatorId, CreationTime, LastModifierId, LastModificationTime, DeleterId, DeletionTime, IsDeleted |
| **TR-002** | All lookup/reference entities inherit from `LookupEntityBase<Guid>` | Simplified audit tracking for reference data with Order field |
| **TR-003** | All entities support soft-delete pattern | IsDeleted flag allows data recovery and prevents actual data loss |

### 5.2 Identity and Access Control

| Rule ID | Rule | Application |
|---------|------|-------------|
| **TR-004** | User identity, roles, and claims managed through Framework.Identity module | Standardized identity management with JWT tokens |
| **TR-005** | Five seeded roles provisioned | Super Admin, Admin, Charity, Accountant, Financial Officer |
| **TR-006** | Five seeded users with default password | Emails: OsamaSuper@IIROSA.com, Admin@IIROSA.com, Charity@IIROSA.com, Accountant@IIROSA.com, FinancialOfficer@IIROSA.com |
| **TR-007** | Default password policy | `P@ssw0rd@2022` for seeded users (must change on first login) |
| **TR-008** | Role-based access control enforced at module and operation level | Every use case checks user role and permissions |
| **TR-009** | User actions tracked through FK_UserId references | All entities include CreatorId and LastModifierId |

### 5.3 Data Isolation and Security

| Rule ID | Rule | Application |
|---------|------|-------------|
| **TR-010** | **Charity Data Isolation (CRITICAL)** | Charity role can ONLY access data where FK_CharityId = CurrentUser.CharityId |
| **TR-011** | **Database-Level Filtering** | Charity filtering applied at database query level, NOT just UI level |
| **TR-012** | **Admin Global Access** | Admin and Super Admin bypass Charity ID filter and see all data |
| **TR-013** | **Charity Dropdown Filter** | Admin/SuperAdmin interfaces include Charity dropdown for filtering data by specific charity |
| **TR-014** | **Access Validation on Every Request** | Every API request from Charity role validated to ensure data belongs to their charity |
| **TR-015** | **Authorization Failures Logged** | Unauthorized access attempts logged in audit log for security monitoring |

### 5.4 Multi-Language Support

| Rule ID | Rule | Application |
|---------|------|-------------|
| **TR-016** | **PRIMARY LANGUAGE: Arabic (RTL)** | System defaults to Arabic for all users, RTL layout |
| **TR-017** | **SECONDARY LANGUAGE: English (LTR)** | Available as alternative language option |
| **TR-018** | **Localization Files** | ar.json (Arabic), en.json (English) in /assets/i18n/ directory |
| **TR-019** | **UI Template** | Dark-RTL template optimized for Arabic with RTL layout support |
| **TR-020** | **Language Switching** | Toggle between Arabic and English with session storage persistence |
| **TR-021** | **Page Direction Attribute** | `dir="rtl"` for Arabic, `dir="ltr"` for English set dynamically |
| **TR-022** | **Missing Translation Handling** | System displays translation key if translation not found for current language |
| **TR-023** | **Translation Key Structure** | Dot notation for hierarchical organization (e.g., "families.add.title") |

### 5.5 Comprehensive Audit Logging

| Rule ID | Rule | Application |
|---------|------|-------------|
| **TR-024** | **Every DB Change Logged** | All INSERT, UPDATE, DELETE operations logged with complete details |
| **TR-025** | **Field-Level Tracking** | Audit logs capture before/after values for each modified field |
| **TR-026** | **User Attribution** | Every audit record includes UserId, UserName, Timestamp, and IP Address |
| **TR-027** | **Entity Type Logging** | Audit logs identify entity type and primary key for each change |
| **TR-028** | **Operation Type Logging** | Audit logs distinguish between Create, Update, Delete operations |
| **TR-029** | **Audit Log Retention** | Audit logs stored indefinitely with archival support |
| **TR-030** | **Audit Log Querying** | Super Admin can query audit logs by entity, user, date range, operation type |
| **TR-031** | **Soft Delete Audit Trail** | Soft deletions logged with deletion timestamp and user |
| **TR-032** | **Login/Logout Tracking** | User authentication events logged with timestamp and IP address |

### 5.6 Module Access Control

| Rule ID | Rule | Application |
|---------|------|-------------|
| **TR-033** | **Charity-Only Modules** | Families module - Charity sees ONLY their data via FK_CharityId filter |
| **TR-034** | **Admin/Super Admin Only** | Office Development Projects, Missions, Seasonal Aid, Housing Projects - Charity CANNOT access |
| **TR-035** | **Admin/SuperAdmin/Accountant** | General Checks - Charity CANNOT access |
| **TR-036** | **Excluding Charity** | Imports & Exports (Incoming/Outgoing) - Available to Admin, SuperAdmin, Accountant, Employees but NOT Charity |
| **TR-037** | **Super Admin Only** | Lookup Management, Dynamic Page Management, User & Role Management, Localization Management |
| **TR-038** | **Financial Officer** | Read-only access to financial modules for reporting |

### 5.7 Orphan Payments (Groups/Batches)

| Rule ID | Rule | Application |
|---------|------|-------------|
| **TR-039** | **No Online Payment** | Orphan Payments are GROUPS/BATCHES only - no payment gateway integration |
| **TR-040** | **Offline Process** | Payment groups used for organizing orphans for manual/offline payment processing |
| **TR-041** | **Admin Managed** | Orphan payment groups created and managed by Admin/Super Admin (NOT Accountant-only) |
| **TR-042** | **Exchange Rate Tracking** | Payment groups track exchange rate for reporting purposes only |
| **TR-043** | **Batch Upload Tracking** | IsBatchUploaded flag indicates group prepared for manual processing |
| **TR-044** | **No Financial Transaction** | Payment groups do NOT represent actual financial transactions in accounting system |

### 5.8 Dynamic Page Management

| Rule ID | Rule | Application |
|---------|------|-------------|
| **TR-045** | **Page Definition Storage** | Pages stored in database with name, route, icon, and metadata |
| **TR-046** | **Role-Based Visibility** | Each page has visibility rules per role (RolePageAssignment entity) |
| **TR-047** | **Dynamic Navigation** | Navigation menu generated dynamically based on user role and page assignments |
| **TR-048** | **Super Admin Control** | Only Super Admin can manage page definitions and role assignments |
| **TR-049** | **Display Order** | Pages have DisplayOrder field for menu sorting per role |
| **TR-050** | **Runtime Filtering** | UI components check page visibility before rendering links and buttons |
| **TR-051** | **Dashboard Configuration** | Each role can have different default landing page (dashboard) |

### 5.9 Lookup Management

| Rule ID | Rule | Application |
|---------|------|-------------|
| **TR-052** | **Centralized Management** | Super Admin manages all lookup tables through dedicated interface |
| **TR-053** | **Lookup Entities** | Centers, Regions, Countries, Departments, MissionTypes, ProjectTypes, Banks, NgoTypes, ChequeBeneficiaries |
| **TR-054** | **Hierarchy Support** | Region linked to Country, Center linked to Region (FK relationships) |
| **TR-055** | **Ordering Support** | Lookup entities have Order field for custom display sequence |
| **TR-056** | **Soft Delete** | Lookup items support soft-delete for data integrity |
| **TR-057** | **Bulk Import/Export** | Lookup tables support bulk operations for initialization |
| **TR-058** | **Cascading Filters** | UI dropdowns filter based on hierarchy (Country → Region → Center) |

### 5.10 Geographic and Organizational Structure

| Rule ID | Rule | Application |
|---------|------|-------------|
| **TR-059** | **Geographic Hierarchy** | Country → Region → Center (3-level hierarchy) |
| **TR-060** | **Organizational Hierarchy** | Department for correspondence routing and employee organization |
| **TR-061** | **Charity Location** | Charities linked to Country, Region, and Center (FK_CountryId, FK_RegionId, FK_CenterId) |
| **TR-062** | **Project Location** | Projects linked to Region and Center for geographic tracking |
| **TR-063** | **Family Location** | Families linked to geographic hierarchy through their charity assignment |
| **TR-064** | **Mission Location** | Missions linked to Region and Center for fieldwork tracking |

### 5.11 Financial Operations

| Rule ID | Rule | Application |
|---------|------|-------------|
| **TR-065** | **Multi-Currency Support** | Egyptian Pound (ProjectCostIn_Egy) and Saudi Riyal (ProjectCostIn_Ryal) |
| **TR-066** | **Exchange Rate Tracking** | Exchange rates tracked for reporting (not automatic conversion) |
| **TR-067** | **Bank Details Capture** | Bank account, IBAN, and BankId captured for charities |
| **TR-068** | **Check Management** | Checks managed by Accountant/Admin/Super Admin (not Charity) |
| **TR-069** | **No Payment Gateway** | All financial operations are offline/manual system entries |
| **TR-070** | **Cheque Beneficiaries** | Cheque beneficiaries managed as lookup entity |

### 5.12 Charity Access Control

| Rule ID | Rule | Application |
|---------|------|-------------|
| **TR-071** | **Account Locking** | Charity accounts can be locked (IsLocked flag) preventing all operations |
| **TR-072** | **Add Operations Control** | Charity add operations controlled by IsAddEnabled flag |
| **TR-073** | **Update Operations Control** | Charity update operations controlled by IsUpdateEnabled flag |
| **TR-074** | **Donation Acceptance Control** | Charity donation acceptance controlled by ReceivingDonations flag |
| **TR-075** | **Password Management** | Charity passwords can be reset by Super Admin and Admin |
| **TR-076** | **Code Uniqueness** | Charity codes must be unique (auto-generated if not provided) |

### 5.13 Document Management

| Rule ID | Rule | Application |
|---------|------|-------------|
| **TR-077** | **File Attachments** | Managed through GUID-based foreign keys (FK_AttachedFile, FK_ProjectReportFile, AttachedFile) |
| **TR-078** | **Audit Tracking** | All attachments inherit audit tracking from base entity classes |
| **TR-079** | **File Type Support** | Documents, images, reports, and correspondence files |
| **TR-080** | **File Storage** | Files stored with GUID names to prevent conflicts and maintain security |
| **TR-081** | **File Size Limits** | Configurable maximum file size for uploads (e.g., 10MB) |
| **TR-082** | **File Type Validation** | Allowed file types validated on upload (PDF, JPG, PNG, DOC, DOCX, XLS, XLSX) |

### 5.14 Mission and Project Management

| Rule ID | Rule | Application |
|---------|------|-------------|
| **TR-083** | **Admin/Super Admin Only** | Missions and Projects managed exclusively by Admin and Super Admin |
| **TR-084** | **Completion Tracking** | Projects track completion (IsFinished) with dates (ProjectDate, ProjectEndDate) |
| **TR-085** | **Mission Completion** | Missions track completion (IsMissionCompleted) with completion date |
| **TR-086** | **Mission Types** | Categorized by MissionTypeId (fieldwork, conferences, training) |
| **TR-087** | **Time Classification** | MissionTimeTypeId for one-time vs recurring missions |
| **TR-088** | **User Assignment** | Missions assigned to specific user via FK_UserId |

### 5.15 Correspondence Tracking

| Rule ID | Rule | Application |
|---------|------|-------------|
| **TR-089** | **Import/Export Scope** | Imports and Exports are for Incoming/Outgoing correspondence only |
| **TR-090** | **Access Control** | Available to Admin, SuperAdmin, Accountant, Employees but NOT Charity |
| **TR-091** | **Serial Numbering** | Incoming letters use serial numbering (Serial, Serial_Txt) with year tracking |
| **TR-092** | **Letter Details** | LetterNumber, LetterDate, Subject, Body tracked for correspondence |
| **TR-093** | **Status Tracking** | Correspondence processing tracked via Status and StatusId fields |
| **TR-094** | **Department Routing** | Letters routed to Department via FK_DepartmentId |

### 5.16 Technical Support

| Rule ID | Rule | Application |
|---------|------|-------------|
| **TR-095** | **Ticket Resolution Tracking** | Support tickets track resolution via IsSolved flag |
| **TR-096** | **File Attachments** | Tickets support file attachments via AttachedFile field |
| **TR-097** | **User Tracking** | Tickets linked to submitting user via FK_UserId |
| **TR-098** | **Universal Access** | All users can create support tickets for their issues |

### 5.17 Data Seeding

| Rule ID | Rule | Application |
|---------|------|-------------|
| **TR-099** | **Lookup Seeding** | System seeds initial lookup data for: Centers, Regions, Countries, Departments, MissionTypes, ProjectTypes, Banks |
| **TR-100** | **User Seeding** | System seeds five users with emails: OsamaSuper@IIROSA.com, Admin@IIROSA.com, Charity@IIROSA.com, Accountant@IIROSA.com, FinancialOfficer@IIROSA.com |
| **TR-101** | **Role Seeding** | System seeds five roles: Super Admin, Admin, Charity, Accountant, Financial Officer |
| **TR-102** | **Default Password** | `P@ssw0rd@2022` for seeded users (to be changed on first login) |
| **TR-103** | **Localization Seeding** | System seeds ar.json and en.json with default translations |

---

## 6. Business Rules

### 6.1 Charity Data Isolation Rules

| Rule ID | Rule | Description |
|---------|------|-------------|
| **BR-001** | **Charity Query Filtering** | Every query from Charity role must include `WHERE CharityId = @CurrentUserCharityId` |
| **BR-002** | **Cross-Charity Access Prevention** | Charity users cannot access data belonging to other charities |
| **BR-003** | **Admin Global Access** | Admin and Super Admin bypass CharityId filter and can access all data |
| **BR-004** | **Charity Dropdown for Admin** | Admin/Super Admin interfaces must include Charity dropdown filter for all Charity-linked data |
| **BR-005** | **Automatic Charity Assignment** | When Charity user creates records, system automatically sets FK_CharityId = CurrentUser.CharityId |
| **BR-006** | **Edit Authorization** | Charity users can only edit records where CharityId = CurrentUser.CharityId |
| **BR-007** | **Delete Authorization** | Charity users can only delete records where CharityId = CurrentUser.CharityId |

### 6.2 Orphan Payment Business Rules

| Rule ID | Rule | Description |
|---------|------|-------------|
| **BR-008** | **Payment Groups Are Not Transactions** | Orphan payment groups are organizational batches, not financial transactions |
| **BR-009** | **Manual Payment Processing** | Payment groups are used for offline/manual payment distribution |
| **BR-010** | **Exchange Rate Reference** | Exchange rates in payment groups are for reporting reference only |
| **BR-011** | **Admin Management Only** | Only Admin and Super Admin can manage payment groups |
| **BR-012** | **Batch Upload Indicator** | IsBatchUploaded flag indicates group is ready for manual processing |
| **BR-013** | **Orphan Eligibility** | Only sponsored orphans typically included in payment groups |

### 6.3 Charity Account Rules

| Rule ID | Rule | Description |
|---------|------|-------------|
| **BR-014** | **Account Locking** | When IsLocked = true, Charity cannot perform any operations |
| **BR-015** | **Add Rights Control** | When IsAddEnabled = false, Charity cannot add new records |
| **BR-016** | **Update Rights Control** | When IsUpdateEnabled = false, Charity cannot modify records |
| **BR-017** | **Password Reset** | Super Admin and Admin can reset Charity passwords |
| **BR-018** | **Single Charity User** | Each Charity has one primary user account linked to organization |

### 6.4 Family and Orphan Rules

| Rule ID | Rule | Description |
|---------|------|-------------|
| **BR-019** | **Provider Requirement** | Each family must have at least one provider (father, mother, or other) |
| **BR-020** | **Orphan-Parent Link** | Orphans must be linked to a family record |
| **BR-021** | **Family-Charity Link** | Families must be linked to exactly one Charity |
| **BR-022** | **Provider Verification** | System should verify and record who the family provider is |
| **BR-023** | **Orphan Sponsorship** | Orphans have sponsorship status (Sponsored, Unsponsored, Pending) |
| **BR-024** | **Family Code Generation** | Family codes auto-generated using pattern: CharityCode-SeqNumber |

### 6.5 Project and Mission Rules

| Rule ID | Rule | Description |
|---------|------|-------------|
| **BR-025** | **Admin Only Management** | Projects and Missions managed only by Admin and Super Admin |
| **BR-026** | **Project Completion** | Projects marked complete (IsFinished = true) cannot be modified without special permission |
| **BR-027** | **Mission Assignment** | Missions must be assigned to a specific user (FK_UserId) |
| **BR-028** | **Geographic Linking** | Projects and Missions linked to Region and Center for tracking |

### 6.6 Check Management Rules

| Rule ID | Rule | Description |
|---------|------|-------------|
| **BR-029** | **Check Access Control** | Checks managed by Accountant, Admin, Super Admin (not Charity) |
| **BR-030** | **Check Status Tracking** | Checks have status: Pending, Cleared, Void |
| **BR-031** | **Beneficiary Requirement** | Every check must have a beneficiary |
| **BR-032** | **Reconciliation** | Checks must be reconciled with bank statements |

---

## 7. Non-Functional Requirements

### 7.1 Performance Requirements

| Requirement | Description | Metric |
|-------------|-------------|--------|
| **NFR-001** | Page Load Time | Standard pages load within 2 seconds |
| **NFR-002** | API Response Time | API responses within 500ms for standard operations |
| **NFR-003** | Large List Performance | Grids handle 10,000+ rows with pagination and virtual scrolling |
| **NFR-004** | Report Generation | Standard reports generate within 10 seconds |
| **NFR-005** | File Upload | File uploads support up to 10MB with progress indicator |

### 7.2 Security Requirements

| Requirement | Description |
|-------------|-------------|
| **NFR-006** | **Authentication** | JWT token-based authentication with 24-hour expiration |
| **NFR-007** | **Password Policy** | Minimum 8 characters, uppercase, lowercase, number, special character |
| **NFR-008** | **Password Expiry** | Force password change every 90 days |
| **NFR-009** | **Failed Login Lockout** | Account locked after 5 failed login attempts for 30 minutes |
| **NFR-010** | **HTTPS Only** | All communication over HTTPS/TLS 1.3 |
| **NFR-011** | **SQL Injection Prevention** | Parameterized queries for all database operations |
| **NFR-012** | **XSS Prevention** | Input sanitization and output encoding |
| **NFR-013** | **CSRF Protection** | Anti-forgery tokens for all state-changing operations |
| **NFR-014** | **Authorization Headers** | API validates authorization headers on every request |
| **NFR-015** | **Audit Log Integrity** | Audit logs write-only, cannot be modified by users |
| **NFR-016** | **Data Isolation Enforcement** | Charity data filtering enforced at database level, not just UI |
| **NFR-017** | **File Upload Security** | File type validation, virus scanning, size limits |

### 7.3 Availability Requirements

| Requirement | Description | Metric |
|-------------|-------------|--------|
| **NFR-018** | System Uptime | 99.5% uptime during business hours |
| **NFR-019** | Backup Frequency | Database backed up daily, retained for 30 days |
| **NFR-020** | Disaster Recovery | RTO (Recovery Time Objective): 4 hours, RPO (Recovery Point Objective): 24 hours |

### 7.4 Usability Requirements

| Requirement | Description |
|-------------|-------------|
| **NFR-021** | **Arabic First** | UI defaults to Arabic with RTL layout |
| **NFR-022** | **Responsive Design** | UI works on desktop, tablet, and mobile devices |
| **NFR-023** | **Accessibility** | WCAG 2.1 Level AA compliance |
| **NFR-024** | **Dark Theme** | Dark-RTL template as default theme |
| **NFR-025** | **Intuitive Navigation** | Menu structure organized by role and module |
| **NFR-026** | **Search Functionality** | Global search available across all modules |
| **NFR-027** | **Export Capability** | All grids support Excel export |
| **NFR-028** | **Print Capability** | All reports support PDF printing |

### 7.5 Scalability Requirements

| Requirement | Description |
|-------------|-------------|
| **NFR-029** | **User Capacity** | System supports 1000+ concurrent users |
| **NFR-030** | **Data Volume** | System supports 100,000+ families and orphans |
| **NFR-031** | **Audit Log Volume** | System supports millions of audit log entries |
| **NFR-032** | **File Storage** | System supports terabytes of file attachments |

### 7.6 Compatibility Requirements

| Requirement | Description |
|-------------|-------------|
| **NFR-033** | **Browser Support** | Chrome 90+, Edge 90+, Firefox 88+, Safari 14+ |
| **NFR-034** | **Mobile Support** | iOS 13+, Android 10+ |
| **NFR-035** | **Database Compatibility** | SQL Server 2017+ |
| **NFR-036** | **API Versioning** | API supports versioning for backward compatibility |

### 7.7 Maintainability Requirements

| Requirement | Description |
|-------------|-------------|
| **NFR-037** | **Code Documentation** | All public APIs documented with XML comments |
| **NFR-038** | **Database Documentation** | Entity relationships and indexes documented |
| **NFR-039** | **Error Logging** | Comprehensive error logging with stack traces |
| **NFR-040** | **Health Monitoring** | Application health endpoints for monitoring |
| **NFR-041** | **Configuration Management** | Environment-specific configurations (dev, staging, prod) |

### 7.8 Audit and Compliance Requirements

| Requirement | Description |
|-------------|-------------|
| **NFR-042** | **Comprehensive Logging** | Every database change logged with before/after values |
| **NFR-043** | **Non-Repudiation** | Audit logs cannot be deleted or modified by application users |
| **NFR-044** | **Log Retention** | Audit logs retained for minimum 7 years |
| **NFR-045** | **User Action Tracking** | All user actions (create, read, update, delete) logged |
| **NFR-046** | **Access Tracking** | All login/logout events logged with IP address and timestamp |
| **NFR-047** | **Failed Access Attempts** | Unauthorized access attempts logged and monitored |
| **NFR-048** | **Data Export Logging** | All data export operations logged with user, timestamp, and record count |

---

## Appendix A: Glossary

| Term | Definition |
|------|------------|
| **Charity** | Charitable organization/NGO registered in the system that manages families and orphans |
| **Orphan Payment Group** | A batch or collection of orphans grouped together for manual/offline payment processing (NOT an online payment) |
| **Data Isolation** | Security principle where users can only access data belonging to their organization |
| **Soft Delete** | Deletion method where records are marked as deleted but not physically removed from database |
| **Audit Log** | Comprehensive record of all system changes including who, what, when, and before/after values |
| **Lookup** | Reference data table containing predefined values for dropdowns and selections |
| **Dynamic Page** | Page whose visibility is controlled by Super Admin per user role |
| **JWT** | JSON Web Token - standard for secure authentication |
| **RBAC** | Role-Based Access Control - authorization method based on user roles |
| **RTL** | Right-to-Left - text direction for Arabic language |
| **LTR** | Left-to-Right - text direction for English language |

---

## Appendix B: Document Revision History

| Version | Date | Author | Changes |
|---------|------|--------|---------|
| 1.0 | 2026-04-20 | Business Analyst | Initial complete use case specification with all 18 modules |

---

**End of Document**
