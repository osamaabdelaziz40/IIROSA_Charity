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
9. **SIMULTANEOUSLY** System creates audit log entry (per UC-17.1):
    - AuditLogId (GUID)
    - EntityType = "Check"
    - EntityId = CheckId
    - Operation = Create
    - UserId = current user ID
    - UserName
    - Timestamp (UTC)
    - IpAddress
    - FieldChanges (JSON with all check fields):
      ```json
      {
        "CheckNumber": {"old": null, "new": "CHQ-2026-001"},
        "CheckDate": {"old": null, "new": "2026-06-02"},
        "Currency": {"old": null, "new": "EGP"},
        "BeneficiaryName": {"old": null, "new": "ABC Supplier"},
        "Amount": {"old": null, "new": 50000},
        "PaymentReason": {"old": null, "new": "Supplier Refund"},
        "Bank": {"old": null, "new": "National Bank of Egypt"},
        "CheckStatus": {"old": null, "new": "Pending"}
      }
      ```
10. System saves audit log entry
11. System displays success message
12. Check appears in check list

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
8. System provides "Export to Excel" button to export current filtered/sorted check list
9. User can export check list to Excel format with all displayed columns

**Postconditions:**
- Check list displayed
- Filtering and search available
- Check data can be exported to Excel

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


