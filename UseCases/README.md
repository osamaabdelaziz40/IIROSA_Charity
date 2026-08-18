# Use Case Specification Files - Index

This directory contains the complete Use Case Specification for the Charity.IIROSA system, organized into individual module files.

## File Structure

All files follow the naming convention: `{SequentialNumber}_{ModuleName}_UseCase.md`

## Module Files

| # | File Name | Module | Use Cases | Description |
|---|-----------|--------|-----------|-------------|
| 01 | 01_UserRoleManagement_UseCase.md | User & Role Management | 10 | Manage system users, roles, and access control (Super Admin only) |
| 02 | 02_Employees_UseCase.md | Employees | 7 | Manage employee accounts and assignments |
| 03 | 03_Charities_UseCase.md | Charities | 14 | Register and manage charitable organizations with data isolation |
| 04 | 04_Families_UseCase.md | Families | 15 | Register families with parents, orphans, and providers (Charity sees own data only) |
| 05 | 05_OrphanPayments_UseCase.md | Orphan Payments | 13 | Create and manage orphan payment groups/batches for offline processing |
| 06 | 06_PeriodicOrphanReports_UseCase.md | Periodic Orphan Reports | 10 | Generate reports on orphans for specified periods |
| 07 | 07_OfficeDevelopmentProjects_UseCase.md | Office Development Projects | 14 | Track office development projects (Admin/Super Admin only) |
| 08 | 08_Missions_UseCase.md | Missions | 13 | Plan and track missions and fieldwork (Admin/Super Admin only) |
| 09 | 09_SeasonalAid_UseCase.md | Seasonal Aid | 11 | Manage seasonal aid campaigns (Admin/Super Admin only) |
| 10 | 10_HousingProjects_UseCase.md | Housing Projects | 10 | Track housing construction/renovation projects (Admin/Super Admin only) |
| 11 | 11_GeneralChecks_UseCase.md | General Checks | 10 | Manage checks and payment instruments (Admin/SuperAdmin/Accountant) |
| 12 | 12_ImportsExports_UseCase.md | Imports & Exports (Correspondence) | 14 | Import/export incoming and outgoing correspondence (excluding Charity) |
| 13 | 13_TechnicalSupport_UseCase.md | Technical Support | 10 | Manage support tickets for all users |
| 14 | 14_LookupManagement_UseCase.md | Lookup Management | 15 | Manage all lookup tables and reference data (Super Admin only) |
| 15 | 15_DynamicPageManagement_UseCase.md | Dynamic Page Management | 10 | Control which pages each role can see (Super Admin only) |
| 16 | 16_LocalizationManagement_UseCase.md | Localization Management | 10 | Create and manage translation files (ar.json, en.json) (Super Admin only) |
| 17 | 17_AuditLogging_UseCase.md | Audit Logging | 13 | View comprehensive audit logs of all database changes |
| 18 | 18_CrossCuttingConcerns_UseCase.md | Cross-Cutting Concerns | 13 | System-wide functionalities (attachments, notifications, search, etc.) |
| 19 | 19_UserImpersonation_UseCase.md | User Impersonation | 8 | Allow Admin & SuperAdmin to login as another user for troubleshooting and support |

## Summary Statistics

- **Total Modules:** 19
- **Total Use Cases:** 240+
- **Total Actors:** 7
- **Document Format:** Markdown
- **Language:** Arabic (Primary, RTL) and English (Secondary, LTR)

## Key Features Documented

### Data Isolation
- Charity users see ONLY their own data (enforced at database level)
- Admin/Super Admin see all data with optional Charity dropdown filter

### Audit Logging
- Every database change logged with field-level details (before/after values)
- User attribution, timestamps, and IP addresses recorded

### Access Control
- Role-based access control across all modules
- Super Admin manages lookups, dynamic pages, users, roles
- Charity cannot access: Projects, Missions, Seasonal Aid, Housing, Imports/Exports

### Localization
- Arabic as primary language (RTL UI)
- English as secondary language (LTR UI)
- Translation files: ar.json, en.json

### Payments
- Orphan Payments are GROUPS/BATCHES for offline processing
- No online payment gateway integration

## How to Use These Files

1. **For Development:** Each module file contains detailed use cases for that specific module
2. **For Testing:** Use cases provide test scenarios with preconditions, main flows, and alternative flows
3. **For Documentation:** Each use case describes complete user interactions with the system
4. **For Training:** Use cases serve as training material for system users

## File Naming Convention

`{SequentialNumber}_{ModuleName}_UseCase.md`

Where:
- **SequentialNumber:** Two-digit number (01-19) indicating module order
- **ModuleName:** Descriptive name of the module (no spaces, PascalCase)
- **UseCase.md:** Standard suffix indicating use case specification

## Document Conventions

Each use case includes:
- **ID** (e.g., UC-1.1)
- **Name**
- **Actor** (role who performs the use case)
- **Priority** (High, Medium, Low)
- **Description**
- **Preconditions** (what must be true before use case can execute)
- **Main Flow** (step-by-step description of normal execution)
- **Alternative Flows** (exception handling)
- **Business Rules** (specific rules that apply)
- **Postconditions** (state after use case completes)

## Version Information

- **Version:** 1.1
- **Date:** 2026-06-03
- **Author:** System Analyst
- **Status:** Complete

## Related Documents

- Complete_UseCaseSpecification_CharityIIROSA.md - Consolidated document with all modules
- UseCaseSpecification_CharityIIROSA.md - Original specification document
- Technical_Rules_and_Constraints.md - (if available) - Technical implementation details

---

**Last Updated:** 2026-06-03  
**Total Files:** 19 module files + 1 index file = 20 files
