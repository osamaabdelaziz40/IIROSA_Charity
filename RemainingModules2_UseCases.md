# Remaining Use Cases - Modules 4.11 through 4.18

Continuation of use case specification for modules 4.11-4.18.

---

### 4.11 Module: General Checks

**Module Owner:** Admin, Super Admin, Accountant  
**Purpose:** Manage checks and payment instruments (Charity CANNOT access)  
**Dependencies:** ChequeBeneficiaries, Banks  

**Important Note:** Charity users CANNOT access this module. Only Admin, Super Admin, and Accountant can manage checks.

#### Use Case UC-11.1: Create Check

| Field | Value |
|-------|-------|
| **ID** | UC-11.1 |
| **Name** | Create Check |
| **Actor** | Accountant, Admin, Super Admin |
| **Priority** | High |
| **Description** | Issue check for payment or expense (Charity CANNOT access) |

**Preconditions:**
- Accountant, Admin, or Super Admin is logged in
- Cheque beneficiaries exist (or can create new)

**Main Flow:**
1. User navigates to General Checks page
2. System displays list of checks with "Add New Check" button
3. User clicks "Add New Check"
4. System displays check creation form with sections:
   - **Check Information:**
     - Check Number (required, auto-generated or manual)
     - Check Date (required, default: today)
     - Due Date (optional, for post-dated checks)
     - Currency (dropdown: EGP, SAR, USD)
   - **Beneficiary Information:**
     - Beneficiary Type (dropdown: From Lookup, New Entry)
     - Beneficiary Name (required)
     - Beneficiary ID (dropdown from ChequeBeneficiary lookup or manual entry)
     - Beneficiary Address
     - Beneficiary Phone
   - **Financial Information:**
     - Amount (required, decimal)
     - Amount in Words (auto-generated or manual)
     - Payment Reason (dropdown: Salary, Supplier Refund, Expense, Other)
     - Payment Description
   - **Bank Information:**
     - Bank (dropdown from lookup)
     - Branch (optional)
     - Account Number
   - **Status:**
     - Check Status (default: Pending)
     - Issue Date
   - **Approval:**
     - Requires Approval (checkbox)
     - Approved By (if applicable)
5. User fills in required fields
6. System validates data
7. System creates check record
8. System generates amount in words (if manual not provided)
9. System logs creation in audit log
10. System displays success message
11. Check appears in check list

**Postconditions:**
- Check record created
- Audit log contains creation record

---

#### Use Case UC-11.2: Register Check Beneficiary

| Field | Value |
|-------|-------|
| **ID** | UC-11.2 |
| **Name** | Register Check Beneficiary |
| **Actor** | Accountant, Admin, Super Admin |
| **Priority** | Medium |
| **Description** | Add recipient information for check |

**Preconditions:**
- Accountant, Admin, or Super Admin is logged in
- Check exists or being created

**Main Flow:**
1. User navigates to General Checks page
2. **For new check:**
   - User creating check
   - System displays beneficiary section
3. **For existing check:**
   - System displays list of checks
   - User selects check
   - System displays check details
4. User enters beneficiary information:
   - Beneficiary Name (required)
   - Beneficiary Type (dropdown: Individual, Company, Charity, Supplier, Employee)
   - ID/Passport Number
   - Address
   - Phone
   - Email
5. User selects "Save to Lookup" to add beneficiary to ChequeBeneficiary lookup
6. User clicks "Save"
7. System validates beneficiary information
8. System creates or updates beneficiary record
9. System links beneficiary to check
10. System logs beneficiary in audit log
11. System displays success message

**Postconditions:**
- Beneficiary registered
- Linked to check
- Optionally added to lookup

---

#### Use Case UC-11.3: Set Check Amount

| Field | Value |
|-------|-------|
| **ID** | UC-11.3 |
| **Name** | Set Check Amount |
| **Actor** | Accountant, Admin, Super Admin |
| **Priority** | High |
| **Description** | Specify check amount and currency |

**Preconditions:**
- Accountant, Admin, or Super Admin is logged in
- Check exists

**Main Flow:**
1. User navigates to General Checks page
2. System displays list of checks
3. User selects check
4. System displays check details with "Financial Information" section
5. User enters:
   - Amount (decimal, required)
   - Currency (dropdown: EGP, SAR, USD)
6. System auto-generates amount in words
7. User can edit amount in words if needed
8. User clicks "Save"
9. System validates amount (positive number)
10. System updates check amount
11. System logs change in audit log
12. System displays success message

**Postconditions:**
- Check amount set
- Amount in words generated

---

#### Use Case UC-11.4: Set Check Date

| Field | Value |
|-------|-------|
| **ID** | UC-11.4 |
| **Name** | Set Check Date |
| **Actor** | Accountant, Admin, Super Admin |
| **Priority** | High |
| **Description** | Record issue date and due date for check |

**Preconditions:**
- Accountant, Admin, or Super Admin is logged in
- Check exists

**Main Flow:**
1. User navigates to General Checks page
2. System displays list of checks
3. User selects check
4. System displays check details
5. User sets/updates:
   - Check Date (date picker)
   - Due Date (date picker, optional for post-dated checks)
6. User clicks "Save"
7. System validates dates (due date >= check date)
8. System updates check dates
9. System logs change in audit log
10. System displays success message

**Postconditions:**
- Check dates set
- Used for maturity tracking

---

#### Use Case UC-11.5: Mark Check as Cleared

| Field | Value |
|-------|-------|
| **ID** | UC-11.5 |
| **Name** | Mark Check as Cleared |
| **Actor** | Accountant, Admin, Super Admin |
| **Priority** | High |
| **Description** | Update check status when cleared by bank |

**Preconditions:**
- Accountant, Admin, or Super Admin is logged in
- Check exists
- Check status is Pending or Issued

**Main Flow:**
1. User navigates to General Checks page
2. System displays list of checks
3. User selects check to mark as cleared
4. System displays check details with "Mark as Cleared" button
5. User clicks "Mark as Cleared"
6. System displays clearance form:
   - Clearance Date (default: today)
   - Bank Reference (optional)
   - Clearance Notes (optional)
7. User enters clearance details
8. User confirms clearance
9. System sets check.CheckStatus = "Cleared"
10. System sets check.ClearanceDate
11. System sets bank reference
12. System logs clearance in audit log
13. System displays success message
14. Check status shows as "Cleared"

**Postconditions:**
- Check marked as cleared
- Clearance date recorded
- Audit log contains clearance record

---

#### Use Case UC-11.6: Void Check

| Field | Value |
|-------|-------|
| **ID** | UC-11.6 |
| **Name** | Void Check |
| **Actor** | Accountant, Admin, Super Admin |
| **Priority** | High |
| **Description** | Cancel check with reason recorded |

**Preconditions:**
- Accountant, Admin, or Super Admin is logged in
- Check exists
- Check not already cleared or voided

**Main Flow:**
1. User navigates to General Checks page
2. System displays list of checks
3. User selects check to void
4. System displays check details with "Void Check" button
5. User clicks "Void Check"
6. System displays void form:
   - Void Reason (dropdown: Lost, Stopped, Error, Expired, Other)
   - Void Date (default: today)
   - Void Notes (required)
7. User selects void reason
8. User enters void notes
9. User confirms void
10. System sets check.CheckStatus = "Void"
11. System sets check.VoidDate
12. System sets check.VoidReason
13. System logs void in audit log
14. System displays success message
15. Check status shows as "Void"
16. Check cannot be modified further

**Postconditions:**
- Check voided
- Void reason recorded
- Audit log contains void record

---

#### Use Case UC-11.7: View Checks List

| Field | Value |
|-------|-------|
| **ID** | UC-11.7 |
| **Name** | View Checks List |
| **Actor** | Accountant, Admin, Super Admin, Financial Officer |
| **Priority** | Low |
| **Description** | View all checks with filtering by status and date |

**Preconditions:**
- Accountant, Admin, Super Admin, or Financial Officer is logged in

**Main Flow:**
1. User navigates to General Checks page
2. System displays list of checks in grid with columns:
   - Check Number
   - Check Date
   - Due Date
   - Beneficiary Name
   - Amount
   - Currency
   - Check Status (Pending, Issued, Cleared, Void)
   - Bank
   - Created By
3. System provides search by check number or beneficiary
4. System provides filter by:
   - Check Status
   - Bank
   - Currency
   - Date Range
   - Amount Range
5. System provides pagination
6. System provides sorting by any column
7. User can click check to view details
8. User can export check list to Excel

**Postconditions:**
- Check list displayed
- Filtering and search available

---

#### Use Case UC-11.8: View Check Details

| Field | Value |
|-------|-------|
| **ID** | UC-11.8 |
| **Name** | View Check Details |
| **Actor** | Accountant, Admin, Super Admin, Financial Officer |
| **Priority** | Low |
| **Description** | View complete check information and beneficiary |

**Preconditions:**
- Accountant, Admin, Super Admin, or Financial Officer is logged in
- Check exists

**Main Flow:**
1. User navigates to General Checks page
2. System displays list of checks
3. User selects check
4. System displays comprehensive check details:
   - **Check Information:**
     - Check Number
     - Check Date
     - Due Date
     - Currency
   - **Beneficiary Information:**
     - Beneficiary Name
     - Beneficiary Type
     - Address
     - Phone
     - ID Number
   - **Financial Information:**
     - Amount
     - Amount in Words
     - Payment Reason
     - Description
   - **Bank Information:**
     - Bank Name
     - Branch
     - Account Number
   - **Status Information:**
     - Check Status
     - Issue Date
     - Clearance Date (if cleared)
     - Void Date (if voided)
     - Void Reason (if voided)
   - **Audit Trail:**
     - Created By, Created Date
     - Modified By, Modified Date
   - **Actions:**
     - "Edit" button (if pending)
     - "Mark as Cleared" button (if issued)
     - "Void Check" button (if not cleared/voided)
5. System displays check image (if uploaded)

**Postconditions:**
- Complete check details visible

---

#### Use Case UC-11.9: Reconcile Checks

| Field | Value |
|-------|-------|
| **ID** | UC-11.9 |
| **Name** | Reconcile Checks |
| **Actor** | Accountant, Admin, Super Admin |
| **Priority** | Medium |
| **Description** | Match issued checks with bank statements |

**Preconditions:**
- Accountant, Admin, or Super Admin is logged in
- Checks exist

**Main Flow:**
1. User navigates to General Checks page
2. System provides "Reconcile Checks" button
3. User clicks "Reconcile Checks"
4. System displays reconciliation page with:
   - **Unreconciled Checks:** List of issued checks not yet matched
   - **Bank Statement Import:** Upload bank statement file
   - **Manual Matching:** Select check and enter bank reference
5. User has two options:
   
   **Option A: Import Bank Statement**
   - User uploads bank statement file (Excel/CSV)
   - System parses file
   - System matches checks by:
     - Check Number
     - Amount
     - Date
   - System displays matched checks
   - User reviews matches
   - User confirms reconciliation
   - System updates matched checks to "Cleared"
   
   **Option B: Manual Matching**
   - User selects unreconciled check
   - User enters bank reference number
   - User enters clearance date
   - User confirms match
   - System updates check to "Cleared"

6. System creates reconciliation records
7. System logs reconciliation in audit log
8. System displays reconciliation summary:
   - Total Checks Reconciled
   - Total Amount
   - Unreconciled Checks
9. User can export reconciliation report

**Postconditions:**
- Checks reconciled with bank
- Audit log contains reconciliation record
- Reconciliation report generated

---

#### Use Case UC-11.10: Generate Check Report

| Field | Value |
|-------|-------|
| **ID** | UC-11.10 |
| **Name** | Generate Check Report |
| **Actor** | Accountant, Admin, Super Admin, Financial Officer |
| **Priority** | Medium |
| **Description** | Produce report on issued checks |

**Preconditions:**
- Accountant, Admin, Super Admin, or Financial Officer is logged in
- Checks exist

**Main Flow:**
1. User navigates to General Checks page
2. System provides "Generate Report" button
3. User clicks "Generate Report"
4. System displays report options:
   - Report Period (date range)
   - Check Status filter (All, Pending, Issued, Cleared, Void)
   - Bank filter
   - Currency filter
   - Group By (Status, Bank, Beneficiary)
5. User selects report options
6. User clicks "Generate"
7. System generates check report with:
   - **Summary Statistics:**
     - Total Checks
     - Total Amount
     - By Status
     - By Bank
     - By Currency
   - **Detailed Check List:**
     - All checks matching filters
   - **Cleared Checks:**
     - List of cleared checks with clearance dates
   - **Pending Checks:**
     - List of outstanding checks
   - **Void Checks:**
     - List of voided checks with reasons
8. System displays report preview
9. System provides export options (Excel, PDF)
10. User can export or print report
11. System logs report generation in audit log

**Postconditions:**
- Check report generated
- Available for export/print

---

### 4.12 Module: Imports & Exports (Correspondence)

**Module Owner:** Admin, Super Admin, Accountant, Employee  
**Purpose:** Import/export incoming and outgoing correspondence (Charity CANNOT access)  
**Dependencies:** Departments, Incoming, Outgoing entities  

**Important Notes:**
- Charity users CANNOT access this module
- Imports and Exports are for correspondence (letters) only
- Available to Admin, Super Admin, Accountant, and Employees

#### Use Case UC-12.1: Import Incoming Letters

| Field | Value |
|-------|-------|
| **ID** | UC-12.1 |
| **Name** | Import Incoming Letters |
| **Actor** | Admin, Super Admin, Accountant, Employee |
| **Priority** | High |
| **Description** | Bulk import incoming correspondence from file (Charity CANNOT access) |

**Preconditions:**
- Admin, Super Admin, Accountant, or Employee is logged in
- Import file exists (Excel/CSV format)

**Main Flow:**
1. User navigates to Imports & Exports page
2. System displays "Import" section with "Import Incoming Letters" button
3. User clicks "Import Incoming Letters"
4. System displays import page with:
   - **File Upload:**
     - Select File button (Excel/CSV)
     - Download Template button
   - **Mapping Options:**
     - Column mapping (file columns to system fields)
   - **Import Options:**
     - Skip Duplicates (checkbox)
     - Update Existing (checkbox)
     - Validate Only (checkbox, for preview)
5. User downloads template (optional)
6. User prepares file according to template
7. User selects file to import
8. System uploads and parses file
9. System displays column mapping interface:
   - File columns shown
   - System fields shown
   - User maps each file column to system field
10. User maps all required fields:
    - Subject
    - Date
    - Letter Number
    - Letter Date
    - Body
    - Year
    - Department
11. User selects import options
12. User clicks "Validate"
13. System validates data:
    - Required fields
    - Data formats
    - Business rules
14. System displays validation results:
    - Valid rows count
    - Invalid rows count
    - Error list for invalid rows
15. **If errors found:**
    - User can fix errors in file
    - User can re-upload file
    - Or user can choose to import valid rows only
16. User clicks "Import"
17. System processes import
18. System creates Incoming records
19. System generates serial numbers automatically
20. System logs import in audit log:
    - Imported by
    - Date/time
    - Record count
    - Success/failure
21. System displays success message with summary
22. Imported letters visible in Incoming Correspondence list

**Alternative Flows:**
- **13a. Validation fails:** System displays specific errors, prevents import
- **17a. Partial import:** Some records fail, system shows which succeeded/failed

**Postconditions:**
- Incoming letters imported
- Serial numbers generated
- Audit log contains import record

---

#### Use Case UC-12.2: Import Outgoing Letters

| Field | Value |
|-------|-------|
| **ID** | UC-12.2 |
| **Name** | Import Outgoing Letters |
| **Actor** | Admin, Super Admin, Accountant, Employee |
| **Priority** | High |
| **Description** | Bulk import outgoing correspondence from file |

**Preconditions:**
- Admin, Super Admin, Accountant, or Employee is logged in
- Import file exists (Excel/CSV format)

**Main Flow:**
1. User navigates to Imports & Exports page
2. System displays "Import" section with "Import Outgoing Letters" button
3. User clicks "Import Outgoing Letters"
4. System displays import page (similar to UC-12.1)
5. User downloads template (optional)
6. User prepares file according to template
7. User selects file to import
8. System uploads and parses file
9. User maps columns to system fields
10. User validates data
11. System displays validation results
12. User clicks "Import"
13. System processes import
14. System creates Outgoing records
15. System logs import in audit log
16. System displays success message
17. Imported letters visible in Outgoing Correspondence list

**Postconditions:**
- Outgoing letters imported
- Audit log contains import record

---

#### Use Case UC-12.3: Validate Import Data

| Field | Value |
|-------|-------|
| **ID** | UC-12.3 |
| **Name** | Validate Import Data |
| **Actor** | System |
| **Priority** | High |
| **Description** | Validate imported correspondence data for errors and duplicates |

**Preconditions:**
- Import file uploaded
- Column mapping complete

**Main Flow:**
1. System receives import file
2. System validates each row:
   - **Required Fields:** Check all required fields have values
   - **Data Formats:**
     - Dates in valid format
     - Numbers in valid format
     - Text lengths within limits
   - **Business Rules:**
     - Letter number uniqueness (within department/year)
     - Valid department reference
     - Date ranges (letter date <= received date)
   - **Duplicates:**
     - Check for duplicate letter numbers
     - Check for duplicate subject + date combinations
3. System categorizes rows:
   - Valid: No errors
   - Warning: Non-critical issues
   - Error: Critical issues preventing import
4. System displays validation summary:
   - Total rows
   - Valid rows
   - Warning rows
   - Error rows
5. System provides detailed error list:
   - Row number
   - Field name
   - Error message
6. System allows user to:
   - View errors in detail
   - Export error list
   - Fix file and re-upload
   - Proceed with valid rows only

**Postconditions:**
- Data validated
- Errors identified
- User informed of issues

---

#### Use Case UC-12.4: Map Import Fields

| Field | Value |
|-------|-------|
| **ID** | UC-12.4 |
| **Name** | Map Import Fields |
| **Actor** | Admin, Super Admin, Accountant, Employee |
| **Priority** | Medium |
| **Description** | Map columns from import file to system fields |

**Preconditions:**
- Admin, Super Admin, Accountant, or Employee is logged in
- Import file uploaded

**Main Flow:**
1. System parses uploaded file
2. System displays column mapping interface with:
   - **File Columns:** List of columns from import file
   - **System Fields:** Dropdowns for each required field
3. User maps each file column to system field:
   - **For Incoming Letters:**
     - Subject → Subject
     - Date → Date
     - Letter Number → LetterNumber
     - Letter Date → LetterDate
     - Body → Body
     - Year → Year
     - Department → Department
     - etc.
   - **For Outgoing Letters:**
     - Similar mapping for outgoing fields
4. System highlights required fields
5. System shows data preview for mapped columns
6. User can:
   - Skip unmapped columns
   - Map multiple columns to one system field (concatenated)
   - Use default values for missing columns
7. User completes mapping
8. System saves mapping configuration
9. Mapping can be saved as template for future imports

**Postconditions:**
- File columns mapped to system fields
- Mapping saved for reuse

---

#### Use Case UC-12.5: Preview Import

| Field | Value |
|-------|-------|
| **ID** | UC-12.5 |
| **Name** | Preview Import |
| **Actor** | Admin, Super Admin, Accountant, Employee |
| **Priority** | Medium |
| **Description** | Review correspondence data before committing import |

**Preconditions:**
- Admin, Super Admin, Accountant, or Employee is logged in
- Import file uploaded and mapped
- Data validated

**Main Flow:**
1. User completes file upload and mapping
2. User clicks "Preview Import"
3. System displays import preview with:
   - **Summary:**
     - Total rows to import
     - Valid rows
     - Invalid rows (excluded from preview)
   - **Data Preview Table:**
     - First 10-20 rows of data
     - Columns showing how data will be imported
     - Color coding for potential issues
   - **Field Mappings:**
     - Show mapped fields
   - **Warnings:**
     - Any potential issues or warnings
4. User reviews preview
5. User can:
   - Adjust mappings if needed
   - Remove specific rows
   - Filter out invalid rows
   - Proceed with import
   - Cancel import
6. If user proceeds, system shows final confirmation:
   - "Ready to import X records. Continue?"
7. User confirms to execute actual import

**Postconditions:**
- User reviews data before import
- Import previewed
- Confirmation required

---

#### Use Case UC-12.6: Commit Import

| Field | Value |
|-------|-------|
| **ID** | UC-12.6 |
| **Name** | Commit Import |
| **Actor** | Admin, Super Admin, Accountant, Employee |
| **Priority** | High |
| **Description** | Execute import and create correspondence records |

**Preconditions:**
- Admin, Super Admin, Accountant, or Employee is logged in
- Import file validated
- User confirmed import

**Main Flow:**
1. User confirms import in preview
2. System begins import process:
   - **For Incoming Letters:**
     - For each valid row:
       - Create Incoming record
       - Auto-generate Serial
       - Auto-generate Serial_Txt
       - Set status to default
       - Link to current user (FK_UserId)
       - Link to department
       - Set timestamps
   - **For Outgoing Letters:**
     - Similar process for outgoing records
3. System processes rows in batches (for performance)
4. System tracks progress:
   - Shows "Processing: X/Y records"
   - Updates progress bar
5. System handles errors:
   - If a row fails, log error
   - Continue with remaining rows
   - Report failures at end
6. System completes import
7. System displays final results:
   - Total rows processed
   - Successfully imported
   - Failed rows
   - Error list for failures
8. System logs complete import in audit log:
   - Imported by
   - Date/time
   - File name
   - Record counts
   - Success/failure details
9. System provides options:
   - View imported records
   - Export error log
   - Start another import
10. Imported records visible in correspondence lists

**Postconditions:**
- Import committed
- Records created
- Audit log complete
- Errors reported

---

#### Use Case UC-12.7: View Import History

| Field | Value |
|-------|-------|
| **ID** | UC-12.7 |
| **Name** | View Import History |
| **Actor** | Admin, Super Admin |
| **Priority** | Low |
| **Description** | View log of previous import operations |

**Preconditions:**
- Admin or Super Admin is logged in
- Imports have been performed

**Main Flow:**
1. User navigates to Imports & Exports page
2. System displays "Import History" section
3. System displays list of imports with columns:
   - Import ID
   - Import Type (Incoming/Outgoing)
   - File Name
   - Import Date/Time
   - Imported By
   - Total Rows
   - Successful Rows
   - Failed Rows
   - Status (Success, Partial Success, Failed)
   - Actions (View Details, Export Log)
4. System provides filter by:
   - Import Type
   - Date Range
   - Imported By
   - Status
5. User can click import to view details
6. System displays detailed import information:
   - File details
   - Mapping used
   - Validation results
   - Success/error details
   - List of imported records
   - Error log

**Postconditions:**
- Import history visible
- Details accessible

---

#### Use Case UC-12.8: Rollback Import

| Field | Value |
|-------|-------|
| **ID** | UC-12.8 |
| **Name** | Rollback Import |
| **Actor** | Admin, Super Admin |
| **Priority** | Medium |
| **Description** | Revert failed or incorrect import operation |

**Preconditions:**
- Admin or Super Admin is logged in
- Import exists in history
- Records were created by import

**Main Flow:**
1. User navigates to Imports & Exports page
2. System displays Import History
3. User selects import to rollback
4. System displays import details with "Rollback" button
5. User clicks "Rollback"
6. System displays confirmation dialog:
   - "This will delete X records imported from this operation. Continue?"
   - System shows list of records to be deleted
7. User confirms rollback
8. System identifies all records created by import
9. System performs soft delete on records:
   - Sets IsDeleted = true
   - Records deletion timestamp
   - Records deleted by user
10. System logs rollback in audit log:
   - Original import ID
   - Rollback date/time
   - Records deleted
   - Deleted by
11. System displays success message
12. Import marked as "Rolled Back" in history
13. Records no longer visible in default views

**Alternative Flows:**
- **7a. User cancels:** System returns without changes

**Business Rules:**
- Only Admin and Super Admin can rollback
- Rollback is soft delete (data preserved)
- Cannot rollback if records have been modified since import

**Postconditions:**
- Import rolled back
- Records deleted (soft)
- Audit log contains rollback record

---

#### Use Case UC-12.9: Download Import Template

| Field | Value |
|-------|-------|
| **ID** | UC-12.9 |
| **Name** | Download Import Template |
| **Actor** | Admin, Super Admin, Accountant, Employee |
| **Priority** | Low |
| **Description** | Get template file for bulk import |

**Preconditions:**
- Admin, Super Admin, Accountant, or Employee is logged in

**Main Flow:**
1. User navigates to Imports & Exports page
2. System displays "Import" section
3. User selects import type:
   - Incoming Letters
   - Outgoing Letters
4. System displays "Download Template" button
5. User clicks "Download Template"
6. System generates template file (Excel format)
7. Template includes:
   - **Header Row:** Column names in Arabic and English
   - **Example Row:** Sample data
   - **Format Notes:** Instructions for each column
   - **Required Fields:** Marked with asterisk
   - **Validation Rules:** Notes on acceptable values
8. System downloads file to user's device
9. System logs template download in audit log

**Postconditions:**
- Template file downloaded
- User can prepare import file

---

#### Use Case UC-12.10: Export Incoming Letters

| Field | Value |
|-------|-------|
| **ID** | UC-12.10 |
| **Name** | Export Incoming Letters |
| **Actor** | Admin, Super Admin, Accountant, Employee |
| **Priority** | Medium |
| **Description** | Export incoming correspondence data to file |

**Preconditions:**
- Admin, Super Admin, Accountant, or Employee is logged in
- Incoming letters exist

**Main Flow:**
1. User navigates to Imports & Exports page
2. System displays "Export" section with "Export Incoming Letters" button
3. User clicks "Export Incoming Letters"
4. System displays export options with:
   - **Filters:**
     - Date Range (From/To)
     - Department (dropdown)
     - Status (dropdown)
     - Year (number)
   - **Fields to Export:**
     - Select All checkbox
     - Individual field checkboxes (Serial, Subject, Date, Letter Number, Body, etc.)
   - **Format Options:**
     - File Format (Excel, PDF, CSV)
     - Language (Arabic, English, Both)
   - **Sort Options:**
     - Sort By (Date, Serial, Department)
     - Sort Order (Ascending, Descending)
5. User selects filters
6. User selects fields to export
7. User selects format
8. User clicks "Generate Export"
9. System queries incoming letters based on filters
10. System generates export file with selected fields
11. System downloads file to user's device
12. System logs export in audit log:
   - Exported by
   - Date/time
   - Filters applied
   - Record count
    - File format
13. System displays success message

**Postconditions:**
- Incoming letters exported
- File downloaded
- Audit log contains export record

---

#### Use Case UC-12.11: Export Outgoing Letters

| Field | Value |
|-------|-------|
| **ID** | UC-12.11 |
| **Name** | Export Outgoing Letters |
| **Actor** | Admin, Super Admin, Accountant, Employee |
| **Priority** | Medium |
| **Description** | Export outgoing correspondence data to file |

**Preconditions:**
- Admin, Super Admin, Accountant, or Employee is logged in
- Outgoing letters exist

**Main Flow:**
1. User navigates to Imports & Exports page
2. System displays "Export" section with "Export Outgoing Letters" button
3. User clicks "Export Outgoing Letters"
4. System displays export options (similar to UC-12.10)
5. User selects filters, fields, and format
6. User clicks "Generate Export"
7. System queries outgoing letters based on filters
8. System generates export file
9. System downloads file to user's device
10. System logs export in audit log
11. System displays success message

**Postconditions:**
- Outgoing letters exported
- File downloaded
- Audit log contains export record

---

#### Use Case UC-12.12: Select Export Fields

| Field | Value |
|-------|-------|
| **ID** | UC-12.12 |
| **Name** | Select Export Fields |
| **Actor** | Admin, Super Admin, Accountant, Employee |
| **Priority** | Low |
| **Description** | Choose specific fields to include in correspondence export |

**Preconditions:**
- Admin, Super Admin, Accountant, or Employee is logged in
- On export page

**Main Flow:**
1. User on export page (Incoming or Outgoing)
2. System displays "Fields to Export" section with:
   - **Select All** checkbox
   - List of available fields with checkboxes:
     - **For Incoming Letters:**
       - Serial
       - Serial_Txt
       - Subject
       - Date
       - Body
       - Year
       - Letter Number
       - Letter Date
       - Status
       - Department
       - Created By
       - Creation Date
     - **For Outgoing Letters:**
       - Similar fields for outgoing
3. User can:
   - Check "Select All" to include all fields
   - Check individual fields to include
   - Uncheck fields to exclude
4. At least one field must be selected
5. User selections saved for current session
6. System applies field selection to export

**Postconditions:**
- Export fields selected
- Export includes only selected fields

---

#### Use Case UC-12.13: Filter Export Data

| Field | Value |
|-------|-------|
| **ID** | UC-12.13 |
| **Name** | Filter Export Data |
| **Actor** | Admin, Super Admin, Accountant, Employee |
| **Priority** | Medium |
| **Description** | Apply filters to limit exported correspondence records |

**Preconditions:**
- Admin, Super Admin, Accountant, or Employee is logged in
- On export page

**Main Flow:**
1. User on export page (Incoming or Outgoing)
2. System displays "Filters" section with:
   - **Date Range:**
     - From Date (date picker)
     - To Date (date picker)
   - **Department Filter:**
     - Department dropdown (All or specific)
   - **Status Filter:**
     - Status dropdown (All or specific status)
   - **Year Filter:** (for incoming)
     - Year number
   - **User Filter:**
     - Created By dropdown
3. User applies desired filters:
   - All filters are optional
   - Multiple filters can be applied together
4. System displays record count preview:
   - "X records match current filters"
5. User can adjust filters to refine selection
6. System applies filters when generating export

**Postconditions:**
- Export data filtered
- Only matching records exported

---

#### Use Case UC-12.14: View Export History

| Field | Value |
|-------|-------|
| **ID** | UC-12.14 |
| **Name** | View Export History |
| **Actor** | Admin, Super Admin |
| **Priority** | Low |
| **Description** | View log of previous export operations |

**Preconditions:**
- Admin or Super Admin is logged in
- Exports have been performed

**Main Flow:**
1. User navigates to Imports & Exports page
2. System displays "Export History" section
3. System displays list of exports with columns:
   - Export ID
   - Export Type (Incoming/Outgoing)
   - Export Date/Time
   - Exported By
   - Filters Applied
   - Record Count
   - File Format
   - Actions (Download Again, View Details)
4. System provides filter by:
   - Export Type
   - Date Range
   - Exported By
5. User can click export to view details
6. System displays detailed export information:
   - Filters used
   - Fields exported
   - Record count
   - File format
   - Download link

**Postconditions:**
- Export history visible
- Details accessible

---

### 4.13 Module: Technical Support

**Module Owner:** All Users  
**Purpose:** Manage support tickets and requests  
**Dependencies:** Users  

#### Use Case UC-13.1: Create Support Ticket

| Field | Value |
|-------|-------|
| **ID** | UC-13.1 |
| **Name** | Create Support Ticket |
| **Actor** | All Users |
| **Priority** | High |
| **Description** | Submit new support request with title and description |

**Preconditions:**
- User is logged in

**Main Flow:**
1. User navigates to Technical Support page
2. System displays "My Tickets" and "New Ticket" sections
3. User clicks "Create New Ticket"
4. System displays ticket creation form:
   - **Ticket Information:**
     - Title (required, e.g., "Cannot access family records")
     - Category (dropdown: Technical, Access, Data, Feature Request, Bug, Other)
     - Priority (dropdown: Low, Medium, High, Urgent)
     - Description (required, multiline text)
   - **Attachment:**
     - Attach File (optional, upload screenshot or document)
   - **System Information:**
     - Browser (auto-detected)
     - Page URL (auto-detected)
     - User Action (what user was doing)
5. User fills in required fields:
   - Title
   - Description
6. User optionally selects category and priority
7. User optionally attaches file
8. User clicks "Submit Ticket"
9. System validates required fields
10. System creates support ticket record:
    - Title
    - Message (description)
    - Category
    - Priority
    - IsSolved = false
    - FK_UserId = current user
    - AttachedFile (if provided)
    - CreatedDate = current date/time
11. System logs ticket creation in audit log
12. System displays success message with ticket ID
13. Ticket visible in user's "My Tickets" list
14. Admin/Super Admin receive notification of new ticket

**Postconditions:**
- Support ticket created
- User can track ticket status
- Admin notified

---

#### Use Case UC-13.2: Attach File to Ticket

| Field | Value |
|-------|-------|
| **ID** | UC-13.2 |
| **Name** | Attach File to Ticket |
| **Actor** | All Users |
| **Priority** | Low |
| **Description** | Upload supporting documents or screenshots |

**Preconditions:**
- User is logged in
- Support ticket exists

**Main Flow:**
1. User navigates to Technical Support page
2. System displays "My Tickets"
3. User selects ticket
4. System displays ticket details
5. User clicks "Attach File"
6. System displays file upload dialog:
   - File selection
   - Description (optional)
7. User selects file
8. User optionally enters description
9. User clicks "Upload"
10. System validates file (type, size)
11. System uploads file
12. System links file to ticket (AttachedFile field)
13. System logs attachment in audit log
14. System displays success message
15. Attachment visible in ticket details

**Postconditions:**
- File attached to ticket
- Support team can view

---

#### Use Case UC-13.3: View My Tickets

| Field | Value |
|-------|-------|
| **ID** | UC-13.3 |
| **Name** | View My Tickets |
| **Actor** | All Users |
| **Priority** | Low |
| **Description** | View support tickets created by current user |

**Preconditions:**
- User is logged in
- Tickets exist

**Main Flow:**
1. User navigates to Technical Support page
2. System displays "My Tickets" section by default
3. System displays list of tickets WHERE FK_UserId = CurrentUser
4. System displays tickets in grid with columns:
   - Ticket ID
   - Title
   - Category
   - Priority
   - Status (Open, In Progress, Solved)
   - Created Date
   - Last Updated
   - Is Solved (Yes/No)
5. System provides filter by:
   - Status
   - Category
   - Priority
   - Date Range
6. System provides search by title or description
7. User can click ticket to view details
8. User can create new ticket
9. System shows ticket count by status

**Postconditions:**
- User sees own tickets
- Can track status

---

#### Use Case UC-13.4: View All Tickets

| Field | Value |
|-------|-------|
| **ID** | UC-13.4 |
| **Name** | View All Tickets |
| **Actor** | Super Admin, Admin |
| **Priority** | Medium |
| **Description** | View all support tickets in system |

**Preconditions:**
- Super Admin or Admin is logged in
- Tickets exist

**Main Flow:**
1. Admin/Super Admin navigates to Technical Support page
2. System displays "All Tickets" section
3. System displays all tickets in grid with columns:
   - Ticket ID
   - Title
   - Category
   - Priority
   - Status
   - Created By (User)
   - Created Date
   - Last Updated
   - Is Solved (Yes/No)
   - Assigned To (if applicable)
4. System provides filter by:
   - Status
   - Category
   - Priority
   - Created By
   - Assigned To
   - Date Range
5. System provides search by title or description
6. System provides sorting by any column
7. Admin/Super Admin can click ticket to view details
8. System shows ticket statistics:
   - Total tickets
   - Open tickets
   - In Progress tickets
   - Solved tickets

**Postconditions:**
- All tickets visible to admin
- Can manage all tickets

---

#### Use Case UC-13.5: Update Ticket Status

| Field | Value |
|-------|-------|
| **ID** | UC-13.5 |
| **Name** | Update Ticket Status |
| **Actor** | Super Admin, Admin |
| **Priority** | Medium |
| **Description** | Change ticket status (open, in progress, resolved) |

**Preconditions:**
- Super Admin or Admin is logged in
- Ticket exists

**Main Flow:**
1. Admin/Super Admin navigates to Technical Support page
2. System displays "All Tickets"
3. Admin selects ticket
4. System displays ticket details
5. Admin selects Status from dropdown:
   - Open
   - In Progress
   - Resolved
   - Closed
6. Admin optionally adds status note/comment
7. Admin clicks "Update Status"
8. System updates ticket status
9. System adds status change to ticket history
10. System logs status update in audit log
11. System displays success message
12. Ticket creator receives notification (if enabled)
13. New status visible in ticket list

**Postconditions:**
- Ticket status updated
- Ticket history maintained
- User notified

---

#### Use Case UC-13.6: Mark Ticket as Solved

| Field | Value |
|-------|-------|
| **ID** | UC-13.6 |
| **Name** | Mark Ticket as Solved |
| **Actor** | Super Admin, Admin |
| **Priority** | High |
| **Description** | Set ticket to solved status (IsSolved flag) |

**Preconditions:**
- Super Admin or Admin is logged in
- Ticket exists
- Ticket not already solved

**Main Flow:**
1. Admin/Super Admin navigates to Technical Support page
2. System displays "All Tickets"
3. Admin selects ticket to solve
4. System displays ticket details with "Mark as Solved" button
5. Admin clicks "Mark as Solved"
6. System displays resolution form:
   - Resolution Description (required)
   - Solution Steps (multiline)
   - Attachments (optional)
7. Admin enters resolution details
8. Admin optionally adds solution attachments
9. Admin confirms resolution
10. System sets ticket.IsSolved = true
11. System sets ticket.Status = "Resolved"
12. System stores resolution details
13. System adds resolution to ticket history
14. System logs resolution in audit log
15. System displays success message
16. Ticket marked as solved in list
17. Ticket creator receives notification with resolution

**Postconditions:**
- Ticket marked as solved
- Resolution recorded
- User notified of resolution

---

#### Use Case UC-13.7: Add Ticket Response

| Field | Value |
|-------|-------|
| **ID** | UC-13.7 |
| **Name** | Add Ticket Response |
| **Actor** | Super Admin, Admin |
| **Priority** | Medium |
| **Description** | Add comments or solutions to ticket |

**Preconditions:**
- Super Admin or Admin is logged in
- Ticket exists

**Main Flow:**
1. Admin/Super Admin navigates to Technical Support page
2. System displays "All Tickets"
3. Admin selects ticket
4. System displays ticket details with "Add Response" section
5. Admin enters response:
   - Response Text (required, multiline)
   - Attach File (optional)
   - Internal Note (checkbox, visible only to admins)
6. Admin clicks "Add Response"
7. System creates ticket response record:
   - Response Text
   - Response Date
   - Responded By (FK_UserId)
   - Attachment (if provided)
   - Is Internal (if internal note)
8. System links response to ticket
9. System updates ticket.LastUpdated
10. System logs response in audit log
11. System displays success message
12. Response visible in ticket history
13. If not internal, ticket creator receives notification

**Postconditions:**
- Response added to ticket
- Ticket history maintained
- User notified (if not internal)

---

#### Use Case UC-13.8: View Ticket Details

| Field | Value |
|-------|-------|
| **ID** | UC-13.8 |
| **Name** | View Ticket Details |
| **Actor** | All Users |
| **Priority** | Low |
| **Description** | View complete ticket with all responses |

**Preconditions:**
- User is logged in
- Ticket exists

**Main Flow:**
1. User navigates to Technical Support page
2. System displays ticket list
3. User selects ticket
4. System validates user has access:
   - **For regular users:** Can only view own tickets
   - **For Admin/Super Admin:** Can view all tickets
5. System displays comprehensive ticket details:
   - **Ticket Information:**
     - Ticket ID
     - Title
     - Category
     - Priority
     - Status
     - Is Solved
     - Created Date
     - Last Updated
   - **Creator Information:**
     - Created By
     - User Email
     - User Role
   - **Description:**
     - Original Message/Description
   - **Attachments:**
     - Original attachment (if any)
     - Download link
   - **Response History:**
     - Chronological list of responses
     - For each response:
       - Response Text
       - Responded By
       - Response Date
       - Attachment (if any)
       - Internal Note indicator
   - **Actions (Admin/Super Admin only):**
     - Update Status
     - Mark as Solved
     - Add Response
     - Assign To User
     - Close Ticket
6. System provides "Add Comment" button for all users
7. System displays ticket timeline visualization

**Business Rules:**
- Regular users see only own tickets
- Admin/Super Admin see all tickets
- Internal notes visible only to Admin/Super Admin

**Postconditions:**
- Complete ticket details displayed
- All responses visible

---

#### Use Case UC-13.9: Search Tickets

| Field | Value |
|-------|-------|
| **ID** | UC-13.9 |
| **Name** | Search Tickets |
| **Actor** | Super Admin, Admin |
| **Priority** | Low |
| **Description** | Search tickets by keyword, status, user |

**Preconditions:**
- Super Admin or Admin is logged in
- Tickets exist

**Main Flow:**
1. Admin/Super Admin navigates to Technical Support page
2. System displays search box above ticket list
3. User enters search term:
   - Keyword (searches title and description)
   - Ticket ID
   - User name
4. User can apply filters:
   - Status
   - Category
   - Priority
   - Created By
   - Date Range
5. User clicks "Search" or presses Enter
6. System searches tickets:
   - Searches title, description for keyword
   - Filters by selected criteria
7. System displays matching tickets
8. System highlights search term in results
9. System shows result count
10. User can clear search to see all tickets

**Postconditions:**
- Tickets matching search displayed
- Easy to find specific tickets

---

#### Use Case UC-13.10: Generate Support Report

| Field | Value |
|-------|-------|
| **ID** | UC-13.10 |
| **Name** | Generate Support Report |
| **Actor** | Super Admin, Admin |
| **Priority** | Low |
| **Description** | Report on ticket volume and resolution time |

**Preconditions:**
- Super Admin or Admin is logged in
- Tickets exist

**Main Flow:**
1. Admin/Super Admin navigates to Technical Support page
2. System provides "Generate Report" button
3. User clicks "Generate Report"
4. System displays report options:
   - Report Period (date range)
   - Include Categories
   - Include Users
   - Group By (Category, Status, User)
5. User selects report options
6. User clicks "Generate"
7. System generates support report with:
   - **Summary Statistics:**
     - Total Tickets
     - Tickets by Status
     - Tickets by Priority
     - Tickets by Category
     - Solved vs Unsolved
   - **Performance Metrics:**
     - Average Resolution Time
     - Tickets Resolved Within SLA
     - Tickets Opened vs Closed
   - **User Statistics:**
     - Tickets by Creator
     - Top Ticket Creators
   - **Trend Analysis:**
     - Tickets over time (daily/weekly/monthly)
     - Resolution time trends
   - **Detailed Ticket List:**
     - All tickets matching report period
8. System displays report with charts/graphs
9. System provides export options (Excel, PDF)
10. User can export or print report
11. System logs report generation in audit log

**Postconditions:**
- Support report generated
- Performance metrics visible
- Available for export

---

**[Document continues with final modules in next section...]**
