### 4.9 Module: Seasonal Aid

**Module Owner:** Admin, Super Admin  
**Purpose:** Manage seasonal aid campaigns (Charity CANNOT access)  
**Dependencies:** Charities, Centers, Regions, Families  

**Important Note:** Charity users CANNOT access this module. Only Admin and Super Admin can manage seasonal aid campaigns.

#### Use Case UC-9.1: Create Seasonal Aid Campaign

| Field | Value |
|-------|-------|
| **ID** | UC-9.1 |
| **Name** | Create Seasonal Aid Campaign |
| **Actor** | Admin, Super Admin |
| **Priority** | High |
| **Description** | Launch new seasonal aid program (Ramadan, Eid, Winter, etc.) (Charity CANNOT access) |

**Preconditions:**
- Admin or Super Admin is logged in
- Charities exist

**Main Flow:**
1. User navigates to Seasonal Aid page
2. System displays list of campaigns with "Add New Campaign" button
3. User clicks "Add New Campaign"
4. System displays campaign creation form with sections:
   - **Basic Information:**
     - Campaign Name (required, e.g., "Ramadan 2026 Aid")
     - Campaign Type (dropdown: Ramadan, Eid Al-Fitr, Eid Al-Adha, Winter, School Supplies, Other)
     - Description
     - Start Date (required)
     - End Date (required)
   - **Financial Information:**
     - Total Budget (decimal)
     - Budget Currency (dropdown: EGP, SAR, USD)
     - Per-Family Allocation (decimal)
   - **Geographic Scope:**
     - Country (dropdown)
     - Region (dropdown, multi-select)
     - Center (dropdown, multi-select)
   - **Charity Assignment:**
     - Assigned Charity (dropdown: All or specific charity)
   - **Beneficiary Criteria:**
     - Maximum Families (number)
     - Family Type (dropdown: All, Orphan Families, Needy Families)
     - Age Range (children)
   - **Status:**
     - Is Active (checkbox, default: true)
     - Is Closed (checkbox, default: false)
5. User fills in required fields
6. System validates date range (End Date >= Start Date)
7. System creates campaign record
8. System logs creation in audit log
9. System displays success message
10. Campaign appears in campaign list

**Postconditions:**
- Campaign record created
- Ready for beneficiary registration

---

#### Use Case UC-9.2: Set Campaign Period

| Field | Value |
|-------|-------|
| **ID** | UC-9.2 |
| **Name** | Set Campaign Period |
| **Actor** | Admin, Super Admin |
| **Priority** | High |
| **Description** | Specify start and end dates for seasonal aid campaign |

**Preconditions:**
- Admin or Super Admin is logged in
- Campaign exists

**Main Flow:**
1. User navigates to Seasonal Aid page
2. System displays list of campaigns
3. User selects campaign
4. System displays campaign details
5. User sets/updates:
   - Start Date (date picker)
   - End Date (date picker)
6. User clicks "Save"
7. System validates date range
8. System updates campaign dates
9. System logs change in audit log
10. System displays success message

**Postconditions:**
- Campaign period set
- Used for beneficiary eligibility

---

#### Use Case UC-9.3: Allocate Campaign Budget

| Field | Value |
|-------|-------|
| **ID** | UC-9.3 |
| **Name** | Allocate Campaign Budget |
| **Actor** | Admin, Super Admin |
| **Priority** | High |
| **Description** | Set budget and resources for seasonal aid |

**Preconditions:**
- Admin or Super Admin is logged in
- Campaign exists

**Main Flow:**
1. User navigates to Seasonal Aid page
2. System displays list of campaigns
3. User selects campaign
4. System displays campaign details with "Financial Information" section
5. User enters:
   - Total Budget (decimal)
   - Budget Currency (dropdown)
   - Per-Family Allocation (decimal)
6. User clicks "Save"
7. System validates budget (positive numbers)
8. System updates campaign budget
9. System logs change in audit log
10. System displays success message
11. Budget visible in campaign details

**Postconditions:**
- Campaign budget allocated
- Used for tracking expenses

---

#### Use Case UC-9.4: Register Beneficiary for Aid

| Field | Value |
|-------|-------|
| **ID** | UC-9.4 |
| **Name** | Register Beneficiary for Aid |
| **Actor** | Admin, Super Admin |
| **Priority** | High |
| **Description** | Add families/individuals to receive seasonal aid |

**Preconditions:**
- Admin or Super Admin is logged in
- Campaign exists and is active
- Families exist

**Main Flow:**
1. User navigates to Seasonal Aid page
2. System displays list of campaigns
3. User selects campaign
4. System displays campaign details with "Beneficiaries" section
5. User clicks "Add Beneficiaries"
6. System displays beneficiary selection page with:
   - **Filters:**
     - Charity (dropdown)
     - Region (dropdown)
     - Center (dropdown)
     - Family Type (dropdown)
   - **Family List:**
     - Checkbox for each eligible family
     - Family Code
     - Family Address
     - Orphan Count
     - Charity
   - **Search:** By family code or address
7. User applies filters
8. User selects families via checkboxes or "Select All"
9. User clicks "Add Selected Families"
10. System creates campaign-beneficiary mappings
11. System calculates required budget (families × allocation)
12. System logs additions in audit log
13. System displays success message with count and budget impact
14. Beneficiaries visible in campaign details

**Postconditions:**
- Families registered as beneficiaries
- Campaign budget impact calculated

---

#### Use Case UC-9.5: Record Aid Distribution

| Field | Value |
|-------|-------|
| **ID** | UC-9.5 |
| **Name** | Record Aid Distribution |
| **Actor** | Admin, Super Admin |
| **Priority** | High |
| **Description** | Document distribution of aid to beneficiaries |

**Preconditions:**
- Admin or Super Admin is logged in
- Campaign exists
- Beneficiaries registered

**Main Flow:**
1. User navigates to Seasonal Aid page
2. System displays list of campaigns
3. User selects campaign
4. System displays campaign details with beneficiaries list
5. User clicks "Record Distribution"
6. System displays distribution recording page:
   - List of registered beneficiaries
   - For each beneficiary:
     - Distributed checkbox (default: false)
     - Distribution Date (date picker, default: today)
     - Amount Distributed (decimal, default: allocation)
     - Notes
     - Received By (person name)
     - Signature (optional upload)
7. User records distribution for each beneficiary:
   - Checks "Distributed"
   - Enters date, amount, notes, recipient
   - Uploads proof/document (optional)
8. User clicks "Save Distributions"
9. System validates data
10. System creates distribution records
11. System updates beneficiary status
12. System calculates total distributed amount
13. System logs distributions in audit log
14. System displays success message with summary
15. Distribution visible in campaign details

**Postconditions:**
- Distribution recorded
- Audit trail created
- Budget tracking updated

---

#### Use Case UC-9.6: View Campaign List

| Field | Value |
|-------|-------|
| **ID** | UC-9.6 |
| **Name** | View Campaign List |
| **Actor** | Admin, Super Admin |
| **Priority** | Low |
| **Description** | View all seasonal campaigns with status |

**Preconditions:**
- Admin or Super Admin is logged in

**Main Flow:**
1. User navigates to Seasonal Aid page
2. System displays list of campaigns in grid with columns:
   - Campaign Name
   - Campaign Type
   - Start Date
   - End Date
   - Total Budget
   - Allocated Budget
   - Distributed Budget
   - Beneficiary Count
   - Status (Active/Closed)
   - Assigned Charity
3. System provides search by campaign name
4. System provides filter by:
   - Campaign Type
   - Status
   - Date Range
   - Charity
5. System provides pagination
6. System provides sorting by any column
7. User can click campaign to view details
8. System provides "Export to Excel" button to export current filtered/sorted campaign list
9. User can export list to Excel format with all displayed columns

**Postconditions:**
- Campaign list displayed
- Filtering available
- Campaign data can be exported to Excel

---

#### Use Case UC-9.7: View Campaign Beneficiaries

| Field | Value |
|-------|-------|
| **ID** | UC-9.7 |
| **Name** | View Campaign Beneficiaries |
| **Actor** | Admin, Super Admin |
| **Priority** | Low |
| **Description** | View list of beneficiaries for specific campaign |

**Preconditions:**
- Admin or Super Admin is logged in
- Campaign exists

**Main Flow:**
1. User navigates to Seasonal Aid page
2. System displays list of campaigns
3. User selects campaign
4. System displays campaign details with "Beneficiaries" section
5. System displays list of beneficiaries with:
   - Family Code
   - Family Address
   - Charity
   - Orphan Count
   - Allocation Amount
   - Distribution Status (Pending/Distributed)
   - Distribution Date
   - Amount Distributed
6. System provides filter by:
   - Distribution Status
   - Charity
   - Region
7. System provides search by family code or address
8. User can click beneficiary to view distribution details

**Postconditions:**
- Beneficiary list displayed
- Distribution status visible

---

#### Use Case UC-9.8: Update Campaign Details

| Field | Value |
|-------|-------|
| **ID** | UC-9.8 |
| **Name** | Update Campaign Details |
| **Actor** | Admin, Super Admin |
| **Priority** | Medium |
| **Description** | Modify campaign information while active |

**Preconditions:**
- Admin or Super Admin is logged in
- Campaign exists
- Campaign not closed

**Main Flow:**
1. User navigates to Seasonal Aid page
2. System displays list of campaigns
3. User selects campaign to edit
4. System displays campaign edit form with current data
5. User modifies desired fields:
   - Campaign Name
   - Description
   - Budget
   - Allocation
   - Dates
   - Notes
6. User clicks "Save"
7. System validates data
8. System updates campaign record
9. System logs all changes in audit log
10. System displays success message

**Postconditions:**
- Campaign details updated
- Audit log contains changes

---

#### Use Case UC-9.9: Close Campaign

| Field | Value |
|-------|-------|
| **ID** | UC-9.9 |
| **Name** | Close Campaign |
| **Actor** | Admin, Super Admin |
| **Priority** | High |
| **Description** | Mark campaign as completed with summary |

**Preconditions:**
- Admin or Super Admin is logged in
- Campaign exists
- Campaign not already closed

**Main Flow:**
1. User navigates to Seasonal Aid page
2. System displays list of campaigns
3. User selects campaign to close
4. System displays campaign details with "Close Campaign" button
5. User clicks "Close Campaign"
6. System displays campaign summary:
   - Total Beneficiaries
   - Total Budget
   - Allocated Amount
   - Distributed Amount
   - Remaining Budget
   - Undistributed Beneficiaries
7. System prompts for closure notes
8. User enters notes (optional)
9. User confirms closure
10. System sets campaign.IsClosed = true
11. System sets closure date
12. System logs closure in audit log
13. System displays success message
14. Campaign status shows as "Closed"
15. Campaign becomes read-only

**Postconditions:**
- Campaign closed
- Final summary generated
- Audit log contains closure record

---

#### Use Case UC-9.10: Generate Campaign Report

| Field | Value |
|-------|-------|
| **ID** | UC-9.10 |
| **Name** | Generate Campaign Report |
| **Actor** | Admin, Super Admin |
| **Priority** | Medium |
| **Description** | Produce report on aid distribution and impact |

**Preconditions:**
- Admin or Super Admin is logged in
- Campaign exists

**Main Flow:**
1. User navigates to Seasonal Aid page
2. System displays list of campaigns
3. User selects campaign
4. System displays campaign details with "Generate Report" button
5. User clicks "Generate Report"
6. System generates campaign report with:
   - **Campaign Information:**
     - Name, Type, Dates, Budget
   - **Beneficiary Summary:**
     - Total Beneficiaries
     - By Region
     - By Charity
     - By Family Type
   - **Financial Summary:**
     - Total Budget
     - Allocated
     - Distributed
     - Remaining
   - **Distribution Details:**
     - List of beneficiaries with distribution status
     - Distribution dates
     - Amounts distributed
   - **Impact Metrics:**
     - Families served
     - Individuals served (estimated)
     - Geographic coverage
7. System displays report preview
8. System provides export options (Excel, PDF)
9. User can export or print report
10. System logs report generation in audit log

**Postconditions:**
- Campaign report generated
- Available for export/print

---

#### Use Case UC-9.11: Assign Campaign to Charity

| Field | Value |
|-------|-------|
| **ID** | UC-9.11 |
| **Name** | Assign Campaign to Charity |
| **Actor** | Admin, Super Admin |
| **Priority** | Medium |
| **Description** | Link seasonal aid campaign to specific charity for beneficiary tracking |

**Preconditions:**
- Admin or Super Admin is logged in
- Campaign exists
- Charities exist

**Main Flow:**
1. User navigates to Seasonal Aid page
2. System displays list of campaigns
3. User selects campaign
4. System displays campaign details with "Charity Assignment" section
5. User selects Assigned Charity from dropdown:
   - All Charities (default, no filter)
   - [Charity 1]
   - [Charity 2]
   - ... (all active charities)
6. User clicks "Save"
7. System updates campaign with charity assignment
8. System logs assignment in audit log
9. System displays success message
10. Campaign linked to charity
11. Beneficiary selection filtered by charity (if specified)

**Postconditions:**
- Campaign assigned to charity
- Used for filtering and beneficiary selection

---


