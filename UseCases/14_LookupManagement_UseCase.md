### 4.14 Module: Lookup Management

**Module Owner:** Super Admin  
**Purpose:** Manage all lookup tables and reference data  
**Dependencies:** All modules that use lookups  

**Important Note:** Only Super Admin can access this module. All other roles cannot manage lookups.

#### Use Case UC-14.1: Create Lookup Item

| Field | Value |
|-------|-------|
| **ID** | UC-14.1 |
| **Name** | Create Lookup Item |
| **Actor** | Super Admin |
| **Priority** | High |
| **Description** | Add new item to any lookup table (Centers, Regions, Countries, Departments, etc.) |

**Preconditions:**
- Super Admin is logged in
- Lookup table exists

**Main Flow:**
1. Super Admin navigates to Lookup Management page
2. System displays list of lookup tables:
   - Centers
   - Regions
   - Countries
   - Departments
   - Mission Types
   - Project Types
   - Banks
   - Charity Types
   - Cheque Beneficiaries
   - Etc.
3. Super Admin selects lookup table
4. System displays current items with "Add New Item" button
5. Super Admin clicks "Add New Item"
6. System displays lookup item creation form (fields vary by lookup type):
   - **Common Fields:**
     - Name (required, in Arabic and English)
     - Order (number, for display sorting)
     - IsActive (checkbox, default: true)
   - **For Countries:**
     - Country Code (ISO code)
     - Dialing Code
     - Currency
   - **For Regions:**
     - FK_CountryId (dropdown)
     - Region Code
   - **For Centers:**
     - FK_RegionId (dropdown)
     - FK_CountryId (auto-populated)
     - Center Code
   - **For Departments:**
     - Department Code
     - Description
   - **For Banks:**
     - Bank Code
     - Swift Code
     - Address
   - **For Charity Types:**
     - Type Code
     - Description
7. Super Admin fills in required fields
8. System validates data
9. System creates lookup item record
10. System logs creation in audit log
11. System displays success message
12. New item visible in lookup list

**Postconditions:**
- Lookup item created
- Available in dropdowns throughout system
- Audit log contains creation record

---

#### Use Case UC-14.2: Update Lookup Item

| Field | Value |
|-------|-------|
| **ID** | UC-14.2 |
| **Name** | Update Lookup Item |
| **Actor** | Super Admin |
| **Priority** | Medium |
| **Description** | Modify existing lookup item details |

**Preconditions:**
- Super Admin is logged in
- Lookup item exists

**Main Flow:**
1. Super Admin navigates to Lookup Management page
2. System displays list of lookup tables
3. Super Admin selects lookup table
4. System displays list of items
5. Super Admin selects item to edit
6. System displays edit form with current data
7. Super Admin modifies desired fields
8. Super Admin clicks "Save"
9. System validates data
10. System updates lookup item
11. System logs changes in audit log
12. System displays success message
13. Updated item visible in list and system dropdowns

**Business Rules:**
- Cannot change ID field
- Changes affect all records using this lookup

**Postconditions:**
- Lookup item updated
- Changes reflected system-wide

---

#### Use Case UC-14.3: Deactivate Lookup Item

| Field | Value |
|-------|-------|
| **ID** | UC-14.3 |
| **Name** | Deactivate Lookup Item |
| **Actor** | Super Admin |
| **Priority** | Medium |
| **Description** | Deactivate lookup item preventing future use |

**Preconditions:**
- Super Admin is logged in
- Lookup item exists
- Item is active

**Main Flow:**
1. Super Admin navigates to Lookup Management page
2. System displays list of lookup tables
3. Super Admin selects lookup table
4. System displays list of items
5. Super Admin selects item to deactivate
6. System displays item details with "Deactivate" button
7. Super Admin clicks "Deactivate"
8. System displays confirmation: "Deactivating will prevent this item from being selected in new records. Existing records will keep this value. Continue?"
9. Super Admin confirms
10. System sets item.IsActive = false
11. System logs deactivation in audit log
12. System displays success message
13. Item shows as "Inactive" in list
14. Item no longer appears in dropdowns for new records

**Alternative Flows:**
- **9a. User cancels:** System returns without changes

**Postconditions:**
- Lookup item deactivated
- Not available for new selections
- Existing records preserved

---

#### Use Case UC-14.4: Set Lookup Item Order

| Field | Value |
|-------|-------|
| **ID** | UC-14.4 |
| **Name** | Set Lookup Item Order |
| **Actor** | Super Admin |
| **Priority** | Low |
| **Description** | Define display order for lookup items (Order field) |

**Preconditions:**
- Super Admin is logged in
- Lookup items exist

**Main Flow:**
1. Super Admin navigates to Lookup Management page
2. System displays list of lookup tables
3. Super Admin selects lookup table
4. System displays list of items with current Order values
5. System provides drag-and-drop interface for reordering
6. **OR** System provides numbered input fields for Order
7. Super Admin adjusts order:
   - **Drag-and-drop:** Drag items to desired position
   - **Manual:** Enter order numbers
8. Super Admin clicks "Save Order"
9. System updates Order field for all items
10. System logs reorder in audit log
11. System displays success message
12. Items appear in new order in system dropdowns

**Postconditions:**
- Lookup item order updated
- Dropdowns reflect new order

---

#### Use Case UC-14.5: View All Lookup Tables

| Field | Value |
|-------|-------|
| **ID** | UC-14.5 |
| **Name** | View All Lookup Tables |
| **Actor** | Super Admin |
| **Priority** | Low |
| **Description** | View all lookup tables with their items |

**Preconditions:**
- Super Admin is logged in

**Main Flow:**
1. Super Admin navigates to Lookup Management page
2. System displays all lookup tables in grid or card view:
   - Table Name
   - Item Count
   - Active Items
   - Inactive Items
   - Last Modified
   - Actions (View Items, Export)
3. System provides search by table name
4. System provides filter by:
   - Item Count range
   - Modified Date range
5. Super Admin can click table to view items
6. System provides "Refresh" button to reload data

**Postconditions:**
- All lookup tables visible
- Easy navigation to manage items

---

#### Use Case UC-14.6: Manage Center Lookups

| Field | Value |
|-------|-------|
| **ID** | UC-14.6 |
| **Name** | Manage Center Lookups |
| **Actor** | Super Admin |
| **Priority** | High |
| **Description** | Create, update, deactivate centers linked to regions |

**Preconditions:**
- Super Admin is logged in
- Regions exist

**Main Flow:**
1. Super Admin navigates to Lookup Management page
2. System displays lookup tables
3. Super Admin selects "Centers"
4. System displays centers list with filters:
   - By Region (dropdown)
   - By Country (dropdown)
   - By Status (Active/Inactive)
5. Super Admin can:
   - Add new center
   - Edit center
   - Deactivate center
   - Set order
6. **When creating/editing center:**
   - Center Name (Arabic and English)
   - Center Code
   - FK_RegionId (required dropdown)
   - FK_CountryId (auto-filled from region)
   - Order (number)
   - IsActive (checkbox)
7. System validates region is selected
8. System saves center with region linkage
9. System logs changes in audit log

**Postconditions:**
- Centers managed with region linkage
- Used in geographic hierarchy

---

#### Use Case UC-14.7: Manage Region Lookups

| Field | Value |
|-------|-------|
| **ID** | UC-14.7 |
| **Name** | Manage Region Lookups |
| **Actor** | Super Admin |
| **Priority** | High |
| **Description** | Create, update, deactivate regions linked to countries |

**Preconditions:**
- Super Admin is logged in
- Countries exist

**Main Flow:**
1. Super Admin navigates to Lookup Management page
2. System displays lookup tables
3. Super Admin selects "Regions"
4. System displays regions list with filters:
   - By Country (dropdown)
   - By Status (Active/Inactive)
5. Super Admin can:
   - Add new region
   - Edit region
   - Deactivate region
   - Set order
6. **When creating/editing region:**
   - Region Name (Arabic and English)
   - Region Code
   - FK_CountryId (required dropdown)
   - Order (number)
   - IsActive (checkbox)
7. System validates country is selected
8. System saves region with country linkage
9. System logs changes in audit log

**Postconditions:**
- Regions managed with country linkage
- Used in geographic hierarchy

---

#### Use Case UC-14.8: Manage Country Lookups

| Field | Value |
|-------|-------|
| **ID** | UC-14.8 |
| **Name** | Manage Country Lookups |
| **Actor** | Super Admin |
| **Priority** | High |
| **Description** | Create, update, deactivate countries |

**Preconditions:**
- Super Admin is logged in

**Main Flow:**
1. Super Admin navigates to Lookup Management page
2. System displays lookup tables
3. Super Admin selects "Countries"
4. System displays countries list
5. Super Admin can:
   - Add new country
   - Edit country
   - Deactivate country
   - Set order
6. **When creating/editing country:**
   - Country Name (Arabic and English)
   - Country Code (ISO code)
   - Dialing Code
   - Currency (dropdown)
   - Order (number)
   - IsActive (checkbox)
7. System validates country code uniqueness
8. System saves country
9. System logs changes in audit log

**Postconditions:**
- Countries managed
- Used in geographic hierarchy

---

#### Use Case UC-14.9: Manage Department Lookups

| Field | Value |
|-------|-------|
| **ID** | UC-14.9 |
| **Name** | Manage Department Lookups |
| **Actor** | Super Admin |
| **Priority** | Medium |
| **Description** | Create, update, deactivate departments for correspondence routing |

**Preconditions:**
- Super Admin is logged in

**Main Flow:**
1. Super Admin navigates to Lookup Management page
2. System displays lookup tables
3. Super Admin selects "Departments"
4. System displays departments list
5. Super Admin can:
   - Add new department
   - Edit department
   - Deactivate department
   - Set order
6. **When creating/editing department:**
   - Department Name (Arabic and English)
   - Department Code
   - Description
   - Order (number)
   - IsActive (checkbox)
7. System saves department
8. System logs changes in audit log

**Postconditions:**
- Departments managed
- Used in correspondence routing

---

#### Use Case UC-14.10: Manage Mission Type Lookups

| Field | Value |
|-------|-------|
| **ID** | UC-14.10 |
| **Name** | Manage Mission Type Lookups |
| **Actor** | Super Admin |
| **Priority** | Medium |
| **Description** | Create, update, deactivate mission type categories |

**Preconditions:**
- Super Admin is logged in

**Main Flow:**
1. Super Admin navigates to Lookup Management page
2. System displays lookup tables
3. Super Admin selects "Mission Types"
4. System displays mission types list
5. Super Admin can:
   - Add new mission type
   - Edit mission type
   - Deactivate mission type
   - Set order
6. **When creating/editing mission type:**
   - Type Name (Arabic and English)
   - Type Code
   - Description
   - Order (number)
   - IsActive (checkbox)
7. System saves mission type
8. System logs changes in audit log

**Postconditions:**
- Mission types managed
- Used in mission classification

---

#### Use Case UC-14.11: Manage Project Type Lookups

| Field | Value |
|-------|-------|
| **ID** | UC-14.11 |
| **Name** | Manage Project Type Lookups |
| **Actor** | Super Admin |
| **Priority** | Medium |
| **Description** | Create, update, deactivate project type categories |

**Preconditions:**
- Super Admin is logged in

**Main Flow:**
1. Super Admin navigates to Lookup Management page
2. System displays lookup tables
3. Super Admin selects "Project Types"
4. System displays project types list
5. Super Admin can:
   - Add new project type
   - Edit project type
   - Deactivate project type
   - Set order
6. **When creating/editing project type:**
   - Type Name (Arabic and English)
   - Type Code
   - Description
   - Order (number)
   - IsActive (checkbox)
7. System saves project type
8. System logs changes in audit log

**Postconditions:**
- Project types managed
- Used in project classification

---

#### Use Case UC-14.12: Manage Bank Lookups

| Field | Value |
|-------|-------|
| **ID** | UC-14.12 |
| **Name** | Manage Bank Lookups |
| **Actor** | Super Admin |
| **Priority** | Medium |
| **Description** | Create, update, deactivate bank information |

**Preconditions:**
- Super Admin is logged in

**Main Flow:**
1. Super Admin navigates to Lookup Management page
2. System displays lookup tables
3. Super Admin selects "Banks"
4. System displays banks list
5. Super Admin can:
   - Add new bank
   - Edit bank
   - Deactivate bank
   - Set order
6. **When creating/editing bank:**
   - Bank Name (Arabic and English)
   - Bank Code
   - Swift Code
   - Address
   - Phone
   - Order (number)
   - IsActive (checkbox)
7. System saves bank
8. System logs changes in audit log

**Postconditions:**
- Banks managed
- Used in financial operations

---

#### Use Case UC-14.13: Manage Charity Type Lookups

| Field | Value |
|-------|-------|
| **ID** | UC-14.13 |
| **Name** | Manage Charity Type Lookups |
| **Actor** | Super Admin |
| **Priority** | Medium |
| **Description** | Create, update, deactivate charity type categories |

**Preconditions:**
- Super Admin is logged in

**Main Flow:**
1. Super Admin navigates to Lookup Management page
2. System displays lookup tables
3. Super Admin selects "Charity Types"
4. System displays charity types list
5. Super Admin can:
   - Add new charity type
   - Edit charity type
   - Deactivate charity type
   - Set order
6. **When creating/editing charity type:**
   - Type Name (Arabic and English)
   - Type Code
   - Description
   - Order (number)
   - IsActive (checkbox)
7. System saves charity type
8. System logs changes in audit log

**Postconditions:**
- Charity types managed
- Used in charity classification

---

#### Use Case UC-14.14: Export Lookup Table

| Field | Value |
|-------|-------|
| **ID** | UC-14.14 |
| **Name** | Export Lookup Table |
| **Actor** | Super Admin |
| **Priority** | Low |
| **Description** | Export complete lookup table to file |

**Preconditions:**
- Super Admin is logged in
- Lookup table exists

**Main Flow:**
1. Super Admin navigates to Lookup Management page
2. System displays list of lookup tables
3. Super Admin selects lookup table
4. System displays items list
5. Super Admin clicks "Export" button
6. System displays export options:
   - Format (Excel, CSV, JSON)
   - Include Inactive Items (checkbox)
   - Language (Arabic, English, Both)
7. Super Admin selects export options
8. Super Admin clicks "Generate Export"
9. System generates export file with:
   - All lookup items
   - All fields
   - Active and inactive (if selected)
10. System downloads file to user's device
11. System logs export in audit log
12. System displays success message

**Postconditions:**
- Lookup table exported
- File available for backup or migration

---

#### Use Case UC-14.15: Import Lookup Table

| Field | Value |
|-------|-------|
| **ID** | UC-14.15 |
| **Name** | Import Lookup Table |
| **Actor** | Super Admin |
| **Priority** | Low |
| **Description** | Bulk import lookup items from file |

**Preconditions:**
- Super Admin is logged in
- Import file exists (Excel/CSV)

**Main Flow:**
1. Super Admin navigates to Lookup Management page
2. System displays list of lookup tables
3. Super Admin selects lookup table
4. System displays items list with "Import" button
5. Super Admin clicks "Import"
6. System displays import interface:
   - Select File button
   - Download Template button
   - Column mapping
   - Import options (Skip Duplicates, Update Existing)
7. Super Admin downloads template (optional)
8. Super Admin prepares file
9. Super Admin selects file to import
10. System uploads and parses file
11. Super Admin maps columns to lookup fields
12. Super Admin validates data
13. System displays validation results
14. Super Admin confirms import
15. System processes import:
   - Creates new items
   - Updates existing items (if option selected)
   - Skips duplicates (if option selected)
16. System logs import in audit log
17. System displays success message with summary
18. Imported items visible in list

**Postconditions:**
- Lookup items imported
- List updated
- Audit log contains import record

---


