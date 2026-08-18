# Use Case Specification - Charity.IIROSA System

## 1. System Overview

**System Name:** Charity.IIROSA  
**System Type:** Charitable Organization Management System  
**Primary Language:** Arabic (RTL - Default)  
**Secondary Language:** English (LTR)  
**Architecture:** ASP.NET Core Backend + Angular Frontend (Dark-RTL Template)  

**Purpose:**  
The Charity.IIROSA system is a comprehensive charitable organization management platform designed to manage charities, beneficiaries (orphans and families), financial operations, development projects, missions, and organizational workflows. The system supports multiple user roles with strict data isolation policies where charities can only access their own data while administrators have global visibility with filtering capabilities.

**Key Business Functions:**
- Charity management with activation/deactivation and access control
- Family and orphan registration with detailed family structure (Charity-specific data)
- Provider identification and verification
- Orphan payment grouping and batch management (offline process, no online payments)
- Development project tracking (Admin/Super Admin only)
- Mission and fieldwork management (Admin/Super Admin only)
- Import/Export of incoming/outgoing correspondence (not Charity role)
- Seasonal aid distribution (Admin/Super Admin only)
- Housing project management (Admin/Super Admin only)
- General check management (Admin/SuperAdmin/Accountant)
- Technical support ticketing
- Dynamic page visibility management per role
- Comprehensive audit logging for all database changes
- Lookup and reference data management

---

## 2. Actors Table

| Actor ID | Actor Name | Description |
|----------|------------|-------------|
| ACT-001 | Super Admin | Highest-level system administrator with full access to all modules, manages lookups, dynamic pages, and global system configuration |
| ACT-002 | Admin | Administrative user with access to all modules excluding system configuration, manages charities, employees, and all business operations |
| ACT-003 | Charity | Charity user with restricted access to ONLY their own families, orphans, and related data. Cannot access other charities' data |
| ACT-004 | Accountant | Financial user responsible for checks management, payment batches, import/export, and financial records |
| ACT-005 | Financial Officer | Senior financial role with oversight on financial operations and approvals (read-only access for reporting) |
| ACT-006 | Employee | Regular employee with access to import/export and incoming/outgoing correspondence, cannot access charity-specific data |
| ACT-007 | System | Automated system processes for audit logging, notifications, background tasks, and data enforcement |

---

## 3. Use Cases by Module

### Module 1: User & Role Management

| Use Case ID | Use Case Name | Actor | Description |
|-------------|---------------|-------|-------------|
| UC-1.1 | Create User | Super Admin | Create new user accounts with role assignment, email, and default password |
| UC-1.2 | Update User | Super Admin | Modify user information including personal details and role reassignment |
| UC-1.3 | Deactivate User | Super Admin | Disable user accounts to prevent system access |
| UC-1.4 | Reset User Password | Super Admin | Reset password for any user in the system |
| UC-1.5 | Assign User to Role | Super Admin | Assign one or more roles to a user granting specific permissions |
| UC-1.6 | Create Role | Super Admin | Define new roles with specific permissions and claims |
| UC-1.7 | Update Role Permissions | Super Admin | Modify permissions and claims associated with existing roles |
| UC-1.8 | View All Users | Super Admin | View list of all system users with their roles and status |
| UC-1.9 | View User Activity | Super Admin | View audit log of user actions and login history |
| UC-1.10 | Manage User Claims | Super Admin | Assign or remove specific claims for granular permission control |

### Module 2: Employees

| Use Case ID | Use Case Name | Actor | Description |
|-------------|---------------|-------|-------------|
| UC-2.1 | Add Employee | Super Admin, Admin | Register new employee with personal details, position, and role assignment |
| UC-2.2 | Update Employee Information | Super Admin, Admin | Modify employee details including contact information and job title |
| UC-2.3 | Deactivate Employee | Super Admin, Admin | Deactivate employee account while preserving records |
| UC-2.4 | Reset Employee Password | Super Admin, Admin | Change password for employee accounts |
| UC-2.5 | View Employees List | Super Admin, Admin | View all employees with filtering and search capabilities |
| UC-2.6 | Assign Employee Role | Super Admin, Admin | Assign specific roles to employee determining module access |
| UC-2.7 | View Employee Profile | Super Admin, Admin, Employee | View detailed employee profile with assigned roles and permissions |

### Module 3: Charities (NGOs)

| Use Case ID | Use Case Name | Actor | Description |
|-------------|---------------|-------|-------------|
| UC-3.1 | Register Charity | Super Admin, Admin | Create new charity with name, address, contact, and location details |
| UC-3.2 | Update Charity Details | Super Admin, Admin | Modify charity information including address, phone, email, and bank details |
| UC-3.3 | Activate Charity | Super Admin, Admin | Enable charity account to allow system access and operations |
| UC-3.4 | Deactivate Charity | Super Admin, Admin | Disable charity account temporarily suspending operations |
| UC-3.5 | Change Charity Password | Super Admin, Admin | Reset password for charity user account |
| UC-3.6 | Enable/Disable Add Rights | Super Admin, Admin | Control whether charity can add new records (IsAddEnabled flag) |
| UC-3.7 | Enable/Disable Update Rights | Super Admin, Admin | Control whether charity can modify existing records (IsUpdateEnabled flag) |
| UC-3.8 | Lock Charity | Super Admin, Admin | Lock charity account preventing all operations (IsLocked flag) |
| UC-3.9 | Set Bank Account Details | Super Admin, Admin | Configure bank account, IBAN, and bank information for payments |
| UC-3.10 | View All Charities | Super Admin, Admin | View list of all charities with status and contact information (Charity filter available) |
| UC-3.11 | View Charity Profile | Super Admin, Admin, Charity | View detailed charity profile including all configured details and rights. Charity sees only their own profile |
| UC-3.12 | Assign Charity to Center | Super Admin, Admin | Link charity to specific center/region |
| UC-3.13 | Manage Charity Contacts | Super Admin, Admin, Charity | Update boss name, responsible person, and their contact information. Charity can update only their own |
| UC-3.14 | Set Map Location | Super Admin, Admin | Configure GPS/map location for charity office |

### Module 4: Families

| Use Case ID | Use Case Name | Actor | Description |
|-------------|---------------|-------|-------------|
| UC-4.1 | Register Family | Charity | Add new family with general information including address and living conditions. Charity can add ONLY to their own charity |
| UC-4.2 | Add Family Father | Charity | Register father details for a family including identification and contact |
| UC-4.3 | Add Family Mother | Charity | Register mother details for a family including identification and contact |
| UC-4.4 | Add Orphan to Family | Charity | Register orphan(s) linked to family with orphan-specific details |
| UC-4.5 | Specify Provider Type | Charity | Identify provider as father, mother, or other provider |
| UC-4.6 | Add Non-Parent Provider | Charity | When provider is not parent, create separate provider record with details |
| UC-4.7 | Verify Parent as Provider | Charity | Verify and confirm that parent(s) are the actual provider(s) |
| UC-4.8 | Update Family Information | Charity | Modify family general information while maintaining family structure. Charity can update ONLY their own families |
| UC-4.9 | Update Father Details | Charity | Modify father information for existing family |
| UC-4.10 | Update Mother Details | Charity | Modify mother information for existing family |
| UC-4.11 | Update Orphan Details | Charity | Modify orphan information within family |
| UC-4.12 | View Family List | Charity, Admin, Super Admin | View registered families with filtering and search. Charity sees ONLY their families. Admin/SuperAdmin see all with Charity dropdown filter |
| UC-4.13 | View Family Details | Charity, Admin, Super Admin | View complete family profile including parents, orphans, and provider. Charity sees ONLY their families |
| UC-4.14 | Deactivate Family | Charity | Mark family as inactive while preserving records. Charity can deactivate ONLY their families |
| UC-4.15 | Attach Family Documents | Charity | Upload and manage supporting documents for family (attachments) |

### Module 5: Orphan Payments (Groups/Batches)

| Use Case ID | Use Case Name | Actor | Description |
|-------------|---------------|-------|-------------|
| UC-5.1 | Create Orphan Payment Group | Admin, Super Admin | Create new orphan payment group/batch with name and date range. This is a GROUP of orphans, NOT actual payment processing |
| UC-5.2 | Set Exchange Rate | Admin, Super Admin | Configure currency exchange rate for payment group reporting |
| UC-5.3 | Add Orphans to Payment Group | Admin, Super Admin | Select orphans to include in payment group batch |
| UC-5.4 | Remove Orphan from Group | Admin, Super Admin | Remove specific orphan from payment group |
| UC-5.5 | Update Payment Group | Admin, Super Admin | Modify group details including name, date range, and exchange rate |
| UC-5.6 | Lock Exchange Rate | Admin, Super Admin | Lock exchange rate to prevent changes (DontRemoveRate flag) |
| UC-5.7 | Mark Group as Uploaded | Admin, Super Admin | Mark group as uploaded/confirmed (IsBatchUploaded flag) |
| UC-5.8 | View Payment Groups | Admin, Super Admin | View all orphan payment groups with status and orphan counts |
| UC-5.9 | View Payment Group Details | Admin, Super Admin | View complete group profile with list of included orphans |
| UC-5.10 | Export Payment Group Report | Admin, Super Admin | Generate exportable report for payment group (offline processing reference) |
| UC-5.11 | Assign Group Number | Admin, Super Admin | Assign sequential batch number to payment group |
| UC-5.12 | Filter Groups by Charity | Admin, Super Admin | View payment groups filtered by specific charity (dropdown filter) |
| UC-5.13 | Filter Groups by Date Range | Admin, Super Admin | View payment groups within specified date period |

### Module 6: Periodic Orphan Reports

| Use Case ID | Use Case Name | Actor | Description |
|-------------|---------------|-------|-------------|
| UC-6.1 | Generate Orphan Report | Charity, Admin, Super Admin | Create periodic report for registered orphans. Charity sees ONLY their orphans |
| UC-6.2 | Set Report Period | Charity, Admin, Super Admin | Specify start and end date for orphan report |
| UC-6.3 | Filter Orphans by Status | Charity, Admin, Super Admin | Generate report for active/sponsored orphans only |
| UC-6.4 | Filter Orphans by Charity | Admin, Super Admin | Generate report for specific charity (dropdown filter). Charity sees only their own |
| UC-6.5 | Filter Orphans by Region | Charity, Admin, Super Admin | Generate report for specific geographic region |
| UC-6.6 | Include Family Details | Charity, Admin, Super Admin | Generate report with orphan family information |
| UC-6.7 | Export Orphan Report | Charity, Admin, Super Admin | Export report to Excel/PDF format |
| UC-6.8 | Schedule Recurring Report | Charity, Admin, Super Admin | Configure automatic report generation on periodic basis |
| UC-6.9 | View Report History | Charity, Admin, Super Admin | View previously generated orphan reports. Charity sees only their reports |
| UC-6.10 | Compare Period Reports | Admin, Super Admin | Compare orphan statistics between different periods across all charities |

### Module 7: Office Development Projects

| Use Case ID | Use Case Name | Actor | Description |
|-------------|---------------|-------|-------------|
| UC-7.1 | Create Office Project | Admin, Super Admin | Register new development project with name, type, and location (Charity CANNOT access) |
| UC-7.2 | Set Project Budget | Admin, Super Admin | Specify project cost in Egyptian Pounds and Saudi Riyals |
| UC-7.3 | Specify Project Donor | Admin, Super Admin | Record donor name and details for project |
| UC-7.4 | Set Beneficiaries Count | Admin, Super Admin | Specify number and type of beneficiaries for project |
| UC-7.5 | Assign Project Location | Admin, Super Admin | Set village name, region, and center for project |
| UC-7.6 | Attach Project Documents | Admin, Super Admin | Upload project proposal and related files (FK_AttachedFile) |
| UC-7.7 | Upload Project Report | Admin, Super Admin | Upload completion report for project (FK_ProjectReportFile) |
| UC-7.8 | Update Project Details | Admin, Super Admin | Modify project information before completion |
| UC-7.9 | Mark Project as Completed | Admin, Super Admin | Set project as finished with completion date (IsFinished) |
| UC-7.10 | View Project List | Admin, Super Admin | View all projects with filtering by status, type, region |
| UC-7.11 | View Project Details | Admin, Super Admin | View complete project profile with all details and attachments |
| UC-7.12 | Track Project Progress | Admin, Super Admin | Monitor ongoing projects and completion status |
| UC-7.13 | Set Project Dates | Admin, Super Admin | Specify project start date and expected/actual end date |
| UC-7.14 | Assign Project to Charity | Admin, Super Admin | Link project to specific charity for tracking |

### Module 8: Missions

| Use Case ID | Use Case Name | Actor | Description |
|-------------|---------------|-------|-------------|
| UC-8.1 | Create Mission | Admin, Super Admin | Plan new mission with target, details, and location (Charity CANNOT access) |
| UC-8.2 | Set Mission Date | Admin, Super Admin | Schedule mission date and time |
| UC-8.3 | Assign Mission Type | Admin, Super Admin | Categorize mission by type (fieldwork, conference, etc.) |
| UC-8.4 | Assign Mission Time Type | Admin, Super Admin | Specify if mission is one-time or recurring |
| UC-8.5 | Set Mission Location | Admin, Super Admin | Specify region, center, village for mission |
| UC-8.6 | Assign Mission Owner | Admin, Super Admin | Assign user responsible for mission execution |
| UC-8.7 | Update Mission Details | Admin, Super Admin | Modify mission information before completion |
| UC-8.8 | Mark Mission as Completed | Admin, Super Admin | Set mission status to completed with completion date |
| UC-8.9 | Record Conference/Entity | Admin, Super Admin | Document conference name or entity name for mission |
| UC-8.10 | View Mission List | Admin, Super Admin | View all missions with filtering by status, type, date |
| UC-8.11 | View Mission Details | Admin, Super Admin | View complete mission profile with all details |
| UC-8.12 | View My Missions | Admin, Super Admin | View missions assigned to current user |
| UC-8.13 | Track Mission Status | Admin, Super Admin | Monitor ongoing and pending missions |

### Module 9: Seasonal Aid

| Use Case ID | Use Case Name | Actor | Description |
|-------------|---------------|-------|-------------|
| UC-9.1 | Create Seasonal Aid Campaign | Admin, Super Admin | Launch new seasonal aid program (Ramadan, Eid, Winter, etc.) (Charity CANNOT access) |
| UC-9.2 | Set Campaign Period | Admin, Super Admin | Specify start and end dates for seasonal aid campaign |
| UC-9.3 | Allocate Campaign Budget | Admin, Super Admin | Set budget and resources for seasonal aid |
| UC-9.4 | Register Beneficiary for Aid | Admin, Super Admin | Add families/individuals to receive seasonal aid |
| UC-9.5 | Record Aid Distribution | Admin, Super Admin | Document distribution of aid to beneficiaries |
| UC-9.6 | View Campaign List | Admin, Super Admin | View all seasonal campaigns with status |
| UC-9.7 | View Campaign Beneficiaries | Admin, Super Admin | View list of beneficiaries for specific campaign |
| UC-9.8 | Update Campaign Details | Admin, Super Admin | Modify campaign information while active |
| UC-9.9 | Close Campaign | Admin, Super Admin | Mark campaign as completed with summary |
| UC-9.10 | Generate Campaign Report | Admin, Super Admin | Produce report on aid distribution and impact |
| UC-9.11 | Assign Campaign to Charity | Admin, Super Admin | Link seasonal aid campaign to specific charity |

### Module 10: Housing Project

| Use Case ID | Use Case Name | Actor | Description |
|-------------|---------------|-------|-------------|
| UC-10.1 | Register Housing Project | Admin, Super Admin | Create new housing project with location and specifications (Charity CANNOT access) |
| UC-10.2 | Set Project Budget | Admin, Super Admin | Specify construction/renovation budget |
| UC-10.3 | Assign Beneficiary Family | Admin, Super Admin | Link eligible family to housing project |
| UC-10.4 | Track Construction Progress | Admin, Super Admin | Monitor housing project milestones |
| UC-10.5 | Record Project Completion | Admin, Super Admin | Mark housing project as completed with handover date |
| UC-10.6 | View Housing Projects | Admin, Super Admin | View all housing projects with status |
| UC-10.7 | Update Project Status | Admin, Super Admin | Modify project status and progress |
| UC-10.8 | Attach Project Documents | Admin, Super Admin | Upload plans, permits, and photos |
| UC-10.9 | Generate Housing Report | Admin, Super Admin | Report on housing projects and beneficiaries |
| UC-10.10 | Assign Project to Charity | Admin, Super Admin | Link housing project to specific charity for beneficiary tracking |

### Module 11: General Checks

| Use Case ID | Use Case Name | Actor | Description |
|-------------|---------------|-------|-------------|
| UC-11.1 | Create Check | Accountant, Admin, Super Admin | Issue check for payment or expense (Charity CANNOT access) |
| UC-11.2 | Register Check Beneficiary | Accountant, Admin, Super Admin | Add recipient information for check |
| UC-11.3 | Set Check Amount | Accountant, Admin, Super Admin | Specify check amount and currency |
| UC-11.4 | Set Check Date | Accountant, Admin, Super Admin | Record issue date and due date for check |
| UC-11.5 | Mark Check as Cleared | Accountant, Admin, Super Admin | Update check status when cleared by bank |
| UC-11.6 | Void Check | Accountant, Admin, Super Admin | Cancel check with reason recorded |
| UC-11.7 | View Checks List | Accountant, Admin, Super Admin, Financial Officer | View all checks with filtering by status and date |
| UC-11.8 | View Check Details | Accountant, Admin, Super Admin, Financial Officer | View complete check information and beneficiary |
| UC-11.9 | Reconcile Checks | Accountant, Admin, Super Admin | Match issued checks with bank statements |
| UC-11.10 | Generate Check Report | Accountant, Admin, Super Admin, Financial Officer | Produce report on issued checks |

### Module 12: Imports & Exports (Incoming/Outgoing Correspondence)

| Use Case ID | Use Case Name | Actor | Description |
|-------------|---------------|-------|-------------|
| UC-12.1 | Import Incoming Letters | Admin, Super Admin, Accountant, Employee | Bulk import incoming correspondence from file (Charity CANNOT access) |
| UC-12.2 | Import Outgoing Letters | Admin, Super Admin, Accountant, Employee | Bulk import outgoing correspondence from file |
| UC-12.3 | Validate Import Data | System | Validate imported correspondence data for errors and duplicates |
| UC-12.4 | Map Import Fields | Admin, Super Admin | Map columns from import file to system fields |
| UC-12.5 | Preview Import | Admin, Super Admin | Review correspondence data before committing import |
| UC-12.6 | Commit Import | Admin, Super Admin | Execute import and create correspondence records |
| UC-12.7 | View Import History | Admin, Super Admin | View log of previous import operations |
| UC-12.8 | Rollback Import | Admin, Super Admin | Revert failed or incorrect import operation |
| UC-12.9 | Download Import Template | Admin, Super Admin, Accountant, Employee | Get template file for bulk import |
| UC-12.10 | Export Incoming Letters | Admin, Super Admin, Accountant, Employee | Export incoming correspondence data to file |
| UC-12.11 | Export Outgoing Letters | Admin, Super Admin, Accountant, Employee | Export outgoing correspondence data to file |
| UC-12.12 | Select Export Fields | Admin, Super Admin | Choose specific fields to include in correspondence export |
| UC-12.13 | Filter Export Data | Admin, Super Admin | Apply filters to limit exported correspondence records |
| UC-12.14 | View Export History | Admin, Super Admin | View log of previous export operations |

### Module 13: Technical Support

| Use Case ID | Use Case Name | Actor | Description |
|-------------|---------------|-------|-------------|
| UC-13.1 | Create Support Ticket | All Users | Submit new support request with title and description |
| UC-13.2 | Attach File to Ticket | All Users | Upload supporting documents or screenshots |
| UC-13.3 | View My Tickets | All Users | View support tickets created by current user |
| UC-13.4 | View All Tickets | Super Admin, Admin | View all support tickets in system |
| UC-13.5 | Update Ticket Status | Super Admin, Admin | Change ticket status (open, in progress, resolved) |
| UC-13.6 | Mark Ticket as Solved | Super Admin, Admin | Set ticket to solved status (IsSolved flag) |
| UC-13.7 | Add Ticket Response | Super Admin, Admin | Add comments or solutions to ticket |
| UC-13.8 | View Ticket Details | All Users | View complete ticket with all responses |
| UC-13.9 | Search Tickets | Super Admin, Admin | Search tickets by keyword, status, user |
| UC-13.10 | Generate Support Report | Super Admin, Admin | Report on ticket volume and resolution time |

### Module 14: Lookup Management

| Use Case ID | Use Case Name | Actor | Description |
|-------------|---------------|-------|-------------|
| UC-14.1 | Create Lookup Item | Super Admin | Add new item to any lookup table (Centers, Regions, Countries, Departments, etc.) |
| UC-14.2 | Update Lookup Item | Super Admin | Modify existing lookup item details |
| UC-14.3 | Deactivate Lookup Item | Super Admin | Deactivate lookup item preventing future use |
| UC-14.4 | Set Lookup Item Order | Super Admin | Define display order for lookup items (Order field) |
| UC-14.5 | View All Lookup Tables | Super Admin | View all lookup tables with their items |
| UC-14.6 | Manage Center Lookups | Super Admin | Create, update, deactivate centers linked to regions |
| UC-14.7 | Manage Region Lookups | Super Admin | Create, update, deactivate regions linked to countries |
| UC-14.8 | Manage Country Lookups | Super Admin | Create, update, deactivate countries |
| UC-14.9 | Manage Department Lookups | Super Admin | Create, update, deactivate departments for correspondence routing |
| UC-14.10 | Manage Mission Type Lookups | Super Admin | Create, update, deactivate mission type categories |
| UC-14.11 | Manage Project Type Lookups | Super Admin | Create, update, deactivate project type categories |
| UC-14.12 | Manage Bank Lookups | Super Admin | Create, update, deactivate bank information |
| UC-14.13 | Manage NGO Type Lookups | Super Admin | Create, update, deactivate charity type categories |
| UC-14.14 | Export Lookup Table | Super Admin | Export complete lookup table to file |
| UC-14.15 | Import Lookup Table | Super Admin | Bulk import lookup items from file |

### Module 15: Dynamic Page Management

| Use Case ID | Use Case Name | Actor | Description |
|-------------|---------------|-------|-------------|
| UC-15.1 | Create Page Definition | Super Admin | Define new page with name, route, and default visibility |
| UC-15.2 | Assign Page to Role | Super Admin | Configure which roles can access specific page |
| UC-15.3 | Remove Page from Role | Super Admin | Remove page access from specific role |
| UC-15.4 | Set Page Display Order | Super Admin | Define order of pages in navigation menu per role |
| UC-15.5 | Enable/Disable Page for Role | Super Admin | Toggle page visibility for specific role without deleting assignment |
| UC-15.6 | View All Page Definitions | Super Admin | View all pages with role assignments |
| UC-15.7 | View Pages by Role | Super Admin | View all pages accessible to specific role |
| UC-15.8 | Update Page Details | Super Admin | Modify page name, route, or description |
| UC-15.9 | Set Page as Dashboard | Super Admin | Configure default landing page for each role |
| UC-15.10 | Preview Page Navigation | Super Admin | Preview navigation menu as seen by specific role |

### Module 16: Localization Management

| Use Case ID | Use Case Name | Actor | Description |
|-------------|---------------|-------|-------------|
| UC-16.1 | Create Arabic Translation | Super Admin | Add Arabic translation for UI text in ar.json |
| UC-16.2 | Create English Translation | Super Admin | Add English translation for UI text in en.json |
| UC-16.3 | Update Translation | Super Admin | Modify existing translation in either language file |
| UC-16.4 | Sync Translation Keys | Super Admin | Ensure translation keys exist in both language files |
| UC-16.5 | Export Translation File | Super Admin | Export language JSON file for external translation |
| UC-16.6 | Import Translation File | Super Admin | Import translated language JSON file |
| UC-16.7 | View Missing Translations | Super Admin | Identify translation keys missing in either language |
| UC-16.8 | Set Default Language | Super Admin | Configure system default language (Arabic) |
| UC-16.9 | Add New Language | Super Admin | Add support for additional language beyond Arabic/English |
| UC-16.10 | Validate Translation Syntax | Super Admin | Validate JSON syntax and structure of translation files |

### Module 17: Audit Logging

| Use Case ID | Use Case Name | Actor | Description |
|-------------|---------------|-------|-------------|
| UC-17.1 | Log Entity Creation | System | Automatically log record creation with user ID, timestamp, and field values |
| UC-17.2 | Log Entity Update | System | Automatically log record modification with user ID, timestamp, and field changes (before/after) |
| UC-17.3 | Log Entity Deletion | System | Automatically log record deletion with user ID, timestamp, and deleted data |
| UC-17.4 | Log User Login | System | Track user login events with timestamp and IP address |
| UC-17.5 | Log User Logout | System | Track user logout events with timestamp |
| UC-17.6 | Log Failed Login Attempts | System | Record failed login attempts for security monitoring |
| UC-17.7 | View Audit Logs | Super Admin, Admin | View comprehensive audit logs with filtering by entity, user, date range |
| UC-17.8 | View Entity Change History | Super Admin, Admin | View complete modification history for specific record |
| UC-17.9 | View User Activity Log | Super Admin | View all actions performed by specific user |
| UC-17.10 | Export Audit Logs | Super Admin | Export audit logs for compliance and reporting |
| UC-17.11 | Search Audit Logs | Super Admin, Admin | Search audit logs by keyword, entity type, or action |
| UC-17.12 | Compare Record Versions | Super Admin, Admin | Compare different versions of record to see changes |
| UC-17.13 | Restore Record Version | Super Admin | Restore record to previous version from audit log |

### Module 18: Cross-Cutting Concerns

| Use Case ID | Use Case Name | Actor | Description |
|-------------|---------------|-------|-------------|
| UC-18.1 | Attach Document to Entity | All Users | Upload and attach files to any entity (families, projects, etc.) |
| UC-18.2 | View Entity Attachments | All Users | View all files attached to specific entity record |
| UC-18.3 | Delete Attachment | All Users | Remove attached file from entity |
| UC-18.4 | Download Attachment | All Users | Download attached file to local system |
| UC-18.5 | Send Notification | System | Send notification to users based on events and triggers |
| UC-18.6 | View Notifications | All Users | View user notifications and alerts |
| UC-18.7 | Mark Notification as Read | All Users | Mark specific notification as read |
| UC-18.8 | Configure Dynamic Lists | Super Admin | Set up configurable lists and lookups |
| UC-18.9 | View Dynamic List Items | All Users | View items from configured lookup lists |
| UC-18.10 | Search and Filter Records | All Users | Apply search and filters across all modules with Charity filtering for Admin roles |
| UC-18.11 | Export Any Grid | All Users | Export data grid views to Excel/PDF |
| UC-18.12 | Apply Charity Data Filter | System | Automatically filter data by Charity ID for Charity role (enforced at database level) |
| UC-18.13 | Validate Charity Access | System | Verify Charity user can only access their own data on every request |

---

## 4. Technical Rules Applied

### 4.1 Entity Base Classes

| Rule | Application |
|------|-------------|
| **TR-001** | All main business entities inherit from `FullAuditedEntityBase<Guid>` providing automatic audit fields (CreatorId, CreationTime, LastModifierId, LastModificationTime, DeleterId, DeletionTime) |
| **TR-002** | All lookup/reference entities inherit from `LookupEntityBase<Guid>` for simplified audit tracking on reference data |
| **TR-003** | All entities support soft-delete pattern (IsDeleted flag) allowing data recovery |

### 4.2 Identity and Access Control

| Rule | Application |
|------|-------------|
| **TR-004** | User identity, roles, and claims managed through Framework.Identity module |
| **TR-005** | Five seeded roles provisioned: Super Admin, Admin, Charity, Accountant, Financial Officer |
| **TR-006** | Five seeded users with default password `P@ssw0rd@2022` corresponding to seeded roles |
| **TR-007** | Role-based access control enforced at module and operation level |
| **TR-008** | User actions tracked through FK_UserId references on all entities |

### 4.3 Multi-Language Support

| Rule | Application |
|------|-------------|
| **TR-009** | **PRIMARY LANGUAGE: Arabic (RTL)** - System defaults to Arabic for all users |
| **TR-010** | **SECONDARY LANGUAGE: English (LTR)** - Available as alternative language |
| **TR-011** | **Localization Files:** ar.json (Arabic), en.json (English) stored in /assets/i18n/ directory |
| **TR-012** | **UI Template:** Dark-RTL template optimized for Arabic with RTL layout support |
| **TR-013** | **Language Switching:** Toggle between Arabic and English with session persistence |
| **TR-014** | Data stored with language-neutral structure; localization applied at presentation layer |

### 4.4 Data Isolation and Security

| Rule | Application |
|------|-------------|
| **TR-015** | **Charity Data Isolation:** Charity role can ONLY access their own families, orphans, and related data |
| **TR-016** | **Database-Level Filtering:** Charity ID filter applied at database level for all Charity queries (not UI-only) |
| **TR-017** | **Admin Global Access:** Admin and Super Admin can access all data with optional Charity dropdown filter |
| **TR-018** | **Charity Dropdown Filter:** Admin/Super Admin interfaces include Charity dropdown for filtering data by specific charity |
| **TR-019** | **Access Validation:** Every request from Charity role validated to ensure data belongs to their charity |

### 4.5 Comprehensive Audit Logging

| Rule | Application |
|------|-------------|
| **TR-020** | **Every DB Change Logged:** All INSERT, UPDATE, DELETE operations logged with complete details |
| **TR-021** | **Field-Level Tracking:** Audit logs capture before/after values for each modified field |
| **TR-022** | **User Attribution:** Every audit record includes UserId, UserName, Timestamp, and IP Address |
| **TR-023** | **Entity Type Logging:** Audit logs identify entity type and primary key for each change |
| **TR-024** | **Operation Type Logging:** Audit logs distinguish between Create, Update, Delete, and Read operations |
| **TR-025** | **Audit Log Retention:** Audit logs stored indefinitely with archival support |
| **TR-026** | **Audit Log Querying:** Super Admin can query audit logs by entity, user, date range, and operation type |

### 4.6 Module Access Control

| Rule | Application |
|------|-------------|
| **TR-027** | **Charity-Only Access:** Families module - Charity sees ONLY their data |
| **TR-028** | **Admin/Super Admin Only:** Office Development Projects, Missions, Seasonal Aid, Housing Projects - Charity CANNOT access |
| **TR-029** | **Admin/SuperAdmin/Accountant:** General Checks - Charity CANNOT access |
| **TR-030** | **Excluding Charity:** Imports & Exports (Incoming/Outgoing) - Available to Admin, SuperAdmin, Accountant, Employees but NOT Charity |
| **TR-031** | **Super Admin Only:** Lookup Management, Dynamic Page Management, User & Role Management |

### 4.7 Orphan Payments (Groups/Batches)

| Rule | Application |
|------|-------------|
| **TR-032** | **No Online Payment:** Orphan Payments are GROUPS/BATCHES only - no payment gateway integration |
| **TR-033** | **Offline Process:** Payment groups used for organizing orphans for manual/offline payment processing |
| **TR-034** | **Admin Managed:** Orphan payment groups created and managed by Admin/Super Admin (NOT Accountant-only) |
| **TR-035** | **Exchange Rate Tracking:** Payment groups track exchange rate for reporting purposes only |
| **TR-036** | **Batch Upload Tracking:** IsBatchUploaded flag indicates group prepared for manual processing |
| **TR-037** | **No Financial Transaction:** Payment groups do NOT represent actual financial transactions |

### 4.8 Dynamic Page Management

| Rule | Application |
|------|-------------|
| **TR-038** | **Page Definition Storage:** Pages stored in database with name, route, and metadata |
| **TR-039** | **Role-Based Visibility:** Each page has visibility rules per role (RolePageAssignment entity) |
| **TR-040** | **Dynamic Navigation:** Navigation menu generated dynamically based on user role and page assignments |
| **TR-041** | **Super Admin Control:** Only Super Admin can manage page definitions and role assignments |
| **TR-042** | **Display Order:** Pages have display order field for menu sorting per role |
| **TR-043** | **Runtime Filtering:** UI components check page visibility before rendering links and buttons |

### 4.9 Lookup Management

| Rule | Application |
|------|-------------|
| **TR-044** | **Centralized Management:** Super Admin manages all lookup tables through dedicated interface |
| **TR-045** | **Lookup Entities:** Centers, Regions, Countries, Departments, MissionTypes, ProjectTypes, Banks, NgoTypes |
| **TR-046** | **Hierarchy Support:** Region linked to Country, Center linked to Region (FK relationships) |
| **TR-047** | **Ordering Support:** Lookup entities have Order field for custom display sequence |
| **TR-048** | **Soft Delete:** Lookup items support soft-delete for data integrity |
| **TR-049** | **Bulk Import/Export:** Lookup tables support bulk operations for initialization |

### 4.10 Geographic and Organizational Structure

| Rule | Application |
|------|-------------|
| **TR-050** | **Geographic Hierarchy:** Country → Region → Center |
| **TR-051** | **Organizational Hierarchy:** Department for correspondence routing |
| **TR-052** | **Charity Location:** Charities linked to Country, Region, and Center (FK_CountryId, FK_RegionId, FK_CenterId) |
| **TR-053** | **Project Location:** Projects linked to Region and Center for geographic tracking |

### 4.11 Financial Operations

| Rule | Application |
|------|-------------|
| **TR-054** | **Multi-Currency Support:** Egyptian Pound (ProjectCostIn_Egy) and Saudi Riyal (ProjectCostIn_Ryal) |
| **TR-055** | **Exchange Rate Tracking:** Exchange rates tracked for reporting (not automatic conversion) |
| **TR-056** | **Bank Details:** Bank account, IBAN, and BankId captured for charities |
| **TR-057** | **Check Management:** Checks managed by Accountant/Admin/Super Admin (not Charity) |
| **TR-058** | **No Payment Gateway:** All financial operations are offline/manual system entries |

### 4.12 Charity Access Control

| Rule | Application |
|------|-------------|
| **TR-059** | **Account Locking:** Charity accounts can be locked (IsLocked flag) preventing all operations |
| **TR-060** | **Add Operations:** Charity add operations controlled by IsAddEnabled flag |
| **TR-061** | **Update Operations:** Charity update operations controlled by IsUpdateEnabled flag |
| **TR-062** | **Donation Acceptance:** Charity donation acceptance controlled by ReceivingDonations flag |
| **TR-063** | **Password Management:** Charity passwords can be reset by Super Admin and Admin |

### 4.13 Document Management

| Rule | Application |
|------|-------------|
| **TR-064** | **File Attachments:** Managed through GUID-based foreign keys (FK_AttachedFile, FK_ProjectReportFile) |
| **TR-065** | **Audit Tracking:** All attachments inherit audit tracking from base entity classes |
| **TR-066** | **File Types:** Support for documents, images, reports, and correspondence files |
| **TR-067** | **Storage:** Files stored with GUID names to prevent conflicts and maintain security |

### 4.14 Mission and Project Management

| Rule | Application |
|------|-------------|
| **TR-068** | **Admin/Super Admin Only:** Missions and Projects managed exclusively by Admin and Super Admin |
| **TR-069** | **Completion Tracking:** Projects track completion (IsFinished) with dates (ProjectDate, ProjectEndDate) |
| **TR-070** | **Mission Completion:** Missions track completion (IsMissionCompleted) with completion date |
| **TR-071** | **Mission Types:** Categorized by MissionTypeId (fieldwork, conferences, training) |
| **TR-072** | **Time Classification:** MissionTimeTypeId for one-time vs recurring missions |
| **TR-073** | **User Assignment:** Missions assigned to specific user via FK_UserId |

### 4.15 Correspondence Tracking

| Rule | Application |
|------|-------------|
| **TR-074** | **Import/Export Scope:** Imports and Exports are for Incoming/Outgoing correspondence only |
| **TR-075** | **Access Control:** Available to Admin, SuperAdmin, Accountant, Employees but NOT Charity |
| **TR-076** | **Serial Numbering:** Incoming letters use serial numbering (Serial, Serial_Txt) with year tracking |
| **TR-077** | **Letter Details:** LetterNumber, LetterDate, Subject, Body tracked for correspondence |
| **TR-078** | **Status Tracking:** Correspondence processing tracked via Status and StatusId fields |
| **TR-079** | **Department Routing:** Letters routed to Department via FK_DepartmentId |

### 4.16 Technical Support

| Rule | Application |
|------|-------------|
| **TR-080** | **Ticket Resolution:** Support tickets track resolution via IsSolved flag |
| **TR-081** | **File Attachments:** Tickets support file attachments via AttachedFile field |
| **TR-082** | **User Tracking:** Tickets linked to submitting user via FK_UserId |
| **TR-083** | **All Users:** All users can create support tickets for their issues |

### 4.17 Data Seeding

| Rule | Application |
|------|-------------|
| **TR-084** | **Lookup Seeding:** System seeds initial lookup data for: Centers, Regions, Countries, Departments, MissionTypes, ProjectTypes, Banks |
| **TR-085** | **User Seeding:** System seeds five users with emails: OsamaSuper@IIROSA.com, Admin@IIROSA.com, Charity@IIROSA.com, Accountant@IIROSA.com, FinancialOfficer@IIROSA.com |
| **TR-086** | **Role Seeding:** System seeds five roles: Super Admin, Admin, Charity, Accountant, Financial Officer |
| **TR-087** | **Default Password:** P@ssw0rd@2022 for seeded users (to be changed on first login) |
| **TR-088** | **Localization Seeding:** System seeds ar.json and en.json with default translations |

### 4.18 Localization Structure

| Rule | Application |
|------|-------------|
| **TR-089** | **File Locations:** /assets/i18n/ar.json (Arabic), /assets/i18n/en.json (English) |
| **TR-090** | **Default Language:** Arabic (ar.json) loaded as default language on application start |
| **TR-091** | **Translation Keys:** Keys follow dot notation (e.g., "menu.families", "buttons.save") |
| **TR-092** | **Missing Translations:** System displays key name if translation not found for current language |
| **TR-093** | **Language Switcher:** UI component to toggle between Arabic and English with session storage |
| **TR-094** | **RTL/LTR Switching:** Page direction attribute (dir="rtl" or dir="ltr") changes based on selected language |

---

**Document Status:** Complete  
**Version:** 2.0  
**Last Updated:** 2026-04-20  
**Key Changes from v1.0:**
- Changed all "Association" references to "Charity"
- Added Arabic as primary language with localization JSON files
- Added comprehensive audit logging requirements
- Added Charity data isolation rules (Charity sees only their data)
- Clarified Orphan Payments as groups/batches (not actual payments)
- Added dynamic page management module
- Added lookup management module
- Updated module access control (removed Charity from admin-only modules)
- Updated Imports & Exports to be for correspondence only
- Added technical rules for data isolation and security
