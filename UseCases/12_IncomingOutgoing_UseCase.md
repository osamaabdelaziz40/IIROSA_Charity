### 4.12 Module: Incoming & Outgoing Correspondence Management

**Module Owner:** Admin, Super Admin, Accountant, Employee  
**Purpose:** Manage incoming and outgoing correspondence letters (Charity CANNOT access)  
**Dependencies:** Departments, Users, UploadedFiles entities  

**Important Notes:**
- Charity users CANNOT access this module
- This module manages **Incoming and Outgoing Letters/Correspondence**
- Available to Admin, Super Admin, Accountant, and Employees
- Auto-generates serial numbers and tracking IDs
- Links to departments and file attachments

---

## Entity Structures

### Incoming Entity (Incoming Letters)
```csharp
public class Incoming
{
    public Guid Id { get; set; }                    // Primary Key
    public int? Serial { get; set; }                // Auto-generated serial number
    public string Serial_Txt { get; set; }          // Text representation of serial
    public string Subject { get; set; }             // Letter subject (Required)
    public DateTime? Date { get; set; }             // Letter received date
    public string IncomingNumber { get; set; }      // Internal tracking number
    public string IncomingId { get; set; }          // External reference ID (Required)
    public string Body { get; set; }                // Letter content/body
    public string LetterNumber { get; set; }        // Official letter number (Required)
    public DateTime? LetterDate { get; set; }       // Date on the letter (Required)
    public int? Year { get; set; }                  // Fiscal/Calendar year
    public string Status { get; set; }              // Processing status (Received, Processing, Completed, Closed, Pending)
    public string LetterDescription { get; set; }   // Brief description
    public Guid? FK_DepartmentId { get; set; }      // Foreign Key to Departments
    public Guid? FK_UserId { get; set; }            // Created by user
    public Guid? OutgoingId { get; set; }           // Linked outgoing letter (if reply)
    public Guid? UploadedFile { get; set; }         // Attached document
    public DateTime CreatedOn { get; set; }
    public DateTime? ModifiedOn { get; set; }
    public Guid? CreatedBy { get; set; }
    public Guid? ModifiedBy { get; set; }
}
```

### Outgoing Entity (Outgoing Letters)
```csharp
public class Outgoing
{
    public Guid Id { get; set; }                    // Primary Key
    public int? Serial { get; set; }                // Auto-generated serial number
    public string Subject { get; set; }             // Letter subject (Required)
    public DateTime? Date { get; set; }             // Letter date
    public string OutGoingNumber { get; set; }      // Internal tracking number
    public string OutGoingId { get; set; }          // External reference ID (Required)
    public string Body { get; set; }                // Letter content/body
    public int? Year { get; set; }                  // Fiscal/Calendar year
    public Guid? Fk_DepartmentId { get; set; }      // Foreign Key to Departments
    public Guid? UploadedFile { get; set; }         // Attached document
    public Guid? OutgoingCategoryId { get; set; }   // Category/classification (official, internal, external)
    public Guid? IncomingId { get; set; }           // Linked incoming letter (replying to)
    public ICollection<ChildOutGoing> ChildOutGoings { get; set; }  // Follow-up letters
    public DateTime CreatedOn { get; set; }
    public DateTime? ModifiedOn { get; set; }
    public Guid? CreatedBy { get; set; }
    public Guid? ModifiedBy { get; set; }
}
```

---

## Incoming Letters Use Cases

#### Use Case UC-12.1: List Incoming Letters

| Field | Value |
|-------|-------|
| **ID** | UC-12.1 |
| **Name** | List Incoming Letters |
| **Actor** | Admin, Super Admin, Accountant, Employee |
| **Priority** | High |
| **Description** | View paginated list of incoming letters with filtering capabilities |

**Preconditions:**
- Admin, Super Admin, Accountant, or Employee is logged in
- Incoming letters exist in database

**Main Flow:**
1. User navigates to "Incoming Letters" page
2. System displays list page with:
   - **Page Actions:**
     - "Add Incoming Letter" button
     - "Import Letters" button
     - "Export Letters" button
   - **Filter Section:**
     - Search text input (searches subject, IncomingId, LetterNumber)
     - Department dropdown
     - Status dropdown (All, Received, Processing, Completed, Closed, Pending)
     - Year number filter
     - Start Date / End Date date pickers
   - **Data Table:**
     - Checkbox column (for bulk actions)
     - Serial number
     - Subject (clickable to view details)
     - IncomingId
     - LetterNumber
     - LetterDate
     - Date (received date)
     - Status (badge colored by status)
     - Department
     - Actions (View, Edit, Delete)
   - **Pagination:**
     - Page size: 20 records per page
     - Page numbers display
     - Total records count
3. System loads first page of letters (sorted by Date descending, newest first)
4. User can apply filters:
   - Type in search box and press Enter or click Search
   - Select department from dropdown
   - Select status from dropdown
   - Enter year number
   - Select date range
   - Click "Clear Filters" to reset all filters
5. System updates list with filtered results
6. User can navigate pages using pagination controls
7. User can click on letter Subject to view details
8. User can click Edit to edit the letter
9. User can click Delete to delete the letter

**Alternative Flows:**
- **No results found:** System displays "No incoming letters found" message
- **Loading error:** System displays error message with retry option

**Postconditions:**
- Incoming letters list displayed
- Filters applied as selected
- Pagination functional

---

#### Use Case UC-12.2: Add Incoming Letter

| Field | Value |
|-------|-------|
| **ID** | UC-12.2 |
| **Name** | Add Incoming Letter |
| **Actor** | Admin, Super Admin, Accountant, Employee |
| **Priority** | High |
| **Description** | Create a new incoming letter record |

**Preconditions:**
- Admin, Super Admin, Accountant, or Employee is logged in
- User has "Add Incoming Letter" permission

**Main Flow:**
1. User navigates to "Incoming Letters" page
2. User clicks "Add Incoming Letter" button
3. System displays create form with fields:
   - **Subject** (Required) - Text input, max 500 characters
   - **Date** (Optional) - Date picker
   - **Incoming Number** (Optional) - Text input
   - **Incoming ID** (Required) - Text input, must be unique
   - **Body** (Optional) - Textarea for long content
   - **Letter Number** (Required) - Text input
   - **Letter Date** (Required) - Date picker
   - **Year** (Optional) - Number input
   - **Status** (Optional) - Dropdown: Received, Processing, Completed, Closed, Pending
   - **Letter Description** (Optional) - Text input
   - **Department** (Optional) - Dropdown from Departments entity
   - **Reply To Outgoing** (Optional) - Dropdown from Outgoing letters
   - **Attached File** (Optional) - File upload
4. User fills in required fields (marked with asterisk)
5. User may fill in optional fields
6. User clicks "Save" button
7. System validates:
   - Subject is required
   - IncomingId is required and unique
   - LetterNumber is required
   - LetterDate is required
   - LetterDate <= Date (if both provided)
8. If validation passes:
   - System creates new Incoming record
   - System auto-generates Serial number
   - System auto-generates Serial_Txt
   - System sets FK_UserId to current user
   - System sets CreatedOn to current timestamp
   - System links department if selected
   - System links to outgoing letter if selected
   - System saves file attachment if uploaded
9. System displays success message
10. System redirects to Incoming Letters list or details page

**Alternative Flows:**
- **Validation fails:** System displays error messages inline with fields, form remains open
- **IncomingId already exists:** System displays error "Incoming ID already exists"
- **User clicks Cancel:** System returns to list without saving

**Business Rules:**
- IncomingId must be unique across all incoming letters
- LetterDate cannot be after Date (received date)
- Serial numbers auto-generated sequentially
- Created by user automatically set to current user

**Postconditions:**
- New incoming letter created in database
- Serial number generated
- Letter visible in Incoming Letters list

---

#### Use Case UC-12.3: Edit Incoming Letter

| Field | Value |
|-------|-------|
| **ID** | UC-12.3 |
| **Name** | Edit Incoming Letter |
| **Actor** | Admin, Super Admin, Accountant, Employee |
| **Priority** | High |
| **Description** | Update an existing incoming letter |

**Preconditions:**
- Admin, Super Admin, Accountant, or Employee is logged in
- Incoming letter exists

**Main Flow:**
1. User navigates to "Incoming Letters" page
2. User clicks Edit button on a letter OR navigates to letter details and clicks Edit
3. System displays edit form pre-populated with existing data
4. Form contains same fields as create form (UC-12.2)
5. User modifies desired fields
6. User clicks "Save" button
7. System validates modified fields:
   - Same validation rules as create
   - IncomingId uniqueness check (excluding current record)
8. If validation passes:
   - System updates Incoming record
   - System sets ModifiedOn to current timestamp
   - System sets ModifiedBy to current user
   - System updates all modified fields
9. System displays success message
10. System redirects to letter details or list page

**Alternative Flows:**
- **Validation fails:** System displays error messages, form remains open
- **Record not found:** System displays "Letter not found" error
- **User clicks Cancel:** System returns without saving

**Postconditions:**
- Incoming letter updated in database
- ModifiedOn and ModifiedBy set
- Changes visible in list and details

---

#### Use Case UC-12.4: Delete Incoming Letter

| Field | Value |
|-------|-------|
| **ID** | UC-12.4 |
| **Name** | Delete Incoming Letter |
| **Actor** | Admin, Super Admin, Accountant, Employee |
| **Priority** | Medium |
| **Description** | Delete an incoming letter record |

**Preconditions:**
- Admin, Super Admin, Accountant, or Employee is logged in
- Incoming letter exists

**Main Flow:**
1. User on "Incoming Letters" page
2. User clicks Delete button on a letter
3. System displays confirmation dialog:
   - "Are you sure you want to delete letter '{Subject}'?"
   - Shows OK and Cancel buttons
4. User clicks OK to confirm
5. System performs soft delete:
   - Sets IsDeleted = true
   - Sets DeletedOn = current timestamp
   - Sets DeletedBy = current user
   - Preserves all other data
6. System displays success message: "Letter deleted successfully"
7. System refreshes the list (deleted letter no longer visible)

**Alternative Flows:**
- **User clicks Cancel:** System returns without deleting
- **Record already deleted:** System displays "Letter not found" error
- **Linked records exist:** System may warn about related outgoing letters

**Business Rules:**
- Soft delete performed (data preserved in database)
- Deleted letters not visible in default list views
- Audit trail maintained

**Postconditions:**
- Incoming letter marked as deleted
- Letter removed from active views
- Delete logged in audit trail

---

#### Use Case UC-12.5: View Incoming Letter Details

| Field | Value |
|-------|-------|
| **ID** | UC-12.5 |
| **Name** | View Incoming Letter Details |
| **Actor** | Admin, Super Admin, Accountant, Employee |
| **Priority** | Medium |
| **Description** | View full details of an incoming letter |

**Preconditions:**
- Admin, Super Admin, Accountant, or Employee is logged in
- Incoming letter exists

**Main Flow:**
1. User on "Incoming Letters" page
2. User clicks on letter Subject or View button
3. System displays letter details page with:
   - **Header Section:**
     - Page title: Subject
     - Actions: Edit, Delete, Back to List
   - **Details Section:**
     - Serial / Serial_Txt
     - Subject
     - IncomingId
     - LetterNumber
     - LetterDate
     - Date (received date)
     - IncomingNumber (internal number)
     - Status (with badge)
     - Year
     - Department (name)
     - LetterDescription
     - Body (full content)
     - Reply To (Outgoing letter if linked)
   - **Meta Information:**
     - Created By (user name)
     - Created On (date/time)
     - Modified By (if modified)
     - Modified On (if modified)
   - **Attachments Section:**
     - File name (if attached)
     - Download link
4. User can read all letter information
5. User can click Edit to modify the letter
6. User can click Delete to remove the letter
7. User can click Back to List to return

**Postconditions:**
- Letter details displayed
- All information visible to user

---

## Outgoing Letters Use Cases

#### Use Case UC-12.6: List Outgoing Letters

| Field | Value |
|-------|-------|
| **ID** | UC-12.6 |
| **Name** | List Outgoing Letters |
| **Actor** | Admin, Super Admin, Accountant, Employee |
| **Priority** | High |
| **Description** | View paginated list of outgoing letters with filtering capabilities |

**Preconditions:**
- Admin, Super Admin, Accountant, or Employee is logged in
- Outgoing letters exist in database

**Main Flow:**
1. User navigates to "Outgoing Letters" page
2. System displays list page with:
   - **Page Actions:**
     - "Add Outgoing Letter" button
     - "Import Letters" button
     - "Export Letters" button
   - **Filter Section:**
     - Search text input (searches subject, OutGoingId)
     - Department dropdown
     - Category dropdown (All, Official, Internal, External)
     - Year number filter
     - Start Date / End Date date pickers
     - Has Reply dropdown (All, Yes, No)
   - **Data Table:**
     - Checkbox column (for bulk actions)
     - Serial number
     - Subject (clickable to view details)
     - OutGoingId
     - OutGoingNumber
     - Date
     - Year
     - Department
     - Category
     - Has Reply (Yes/No indicator)
     - Actions (View, Edit, Delete)
   - **Pagination:**
     - Page size: 20 records per page
     - Page numbers display
     - Total records count
3. System loads first page of letters (sorted by Date descending, newest first)
4. User can apply filters
5. System updates list with filtered results
6. User can navigate pages using pagination
7. User can click on letter Subject to view details
8. User can click Edit to edit the letter
9. User can click Delete to delete the letter

**Postconditions:**
- Outgoing letters list displayed
- Filters functional
- Pagination working

---

#### Use Case UC-12.7: Add Outgoing Letter

| Field | Value |
|-------|-------|
| **ID** | UC-12.7 |
| **Name** | Add Outgoing Letter |
| **Actor** | Admin, Super Admin, Accountant, Employee |
| **Priority** | High |
| **Description** | Create a new outgoing letter record |

**Preconditions:**
- Admin, Super Admin, Accountant, or Employee is logged in

**Main Flow:**
1. User navigates to "Outgoing Letters" page
2. User clicks "Add Outgoing Letter" button
3. System displays create form with fields:
   - **Subject** (Required) - Text input, max 500 characters
   - **Date** (Optional) - Date picker
   - **Outgoing Number** (Optional) - Text input
   - **Outgoing ID** (Required) - Text input, must be unique
   - **Body** (Optional) - Textarea
   - **Year** (Optional) - Number input
   - **Department** (Optional) - Dropdown from Departments
   - **Category** (Optional) - Dropdown: Official, Internal, External
   - **Reply To Incoming** (Optional) - Dropdown from Incoming letters
   - **Attached File** (Optional) - File upload
4. User fills in required fields (Subject, OutgoingId)
5. User may fill in optional fields
6. User clicks "Save" button
7. System validates:
   - Subject is required
   - OutgoingId is required and unique
8. If validation passes:
   - System creates new Outgoing record
   - System auto-generates Serial number
   - System sets CreatedBy to current user
   - System sets CreatedOn to current timestamp
   - System links department if selected
   - System links to incoming letter if selected
   - System saves file attachment if uploaded
9. System displays success message
10. System redirects to list or details page

**Alternative Flows:**
- **Validation fails:** System displays error messages
- **OutgoingId already exists:** System displays error
- **User clicks Cancel:** Returns to list

**Business Rules:**
- OutgoingId must be unique
- Serial numbers auto-generated
- CreatedBy automatically set

**Postconditions:**
- New outgoing letter created
- Serial number generated
- Letter visible in list

---

#### Use Case UC-12.8: Edit Outgoing Letter

| Field | Value |
|-------|-------|
| **ID** | UC-12.8 |
| **Name** | Edit Outgoing Letter |
| **Actor** | Admin, Super Admin, Accountant, Employee |
| **Priority** | High |
| **Description** | Update an existing outgoing letter |

**Preconditions:**
- Admin, Super Admin, Accountant, or Employee is logged in
- Outgoing letter exists

**Main Flow:**
1. User navigates to "Outgoing Letters" page
2. User clicks Edit button on a letter
3. System displays edit form pre-populated with data
4. Form contains same fields as create form (UC-12.7)
5. User modifies desired fields
6. User clicks "Save" button
7. System validates modified fields
8. If validation passes:
   - System updates Outgoing record
   - System sets ModifiedOn to current timestamp
   - System sets ModifiedBy to current user
9. System displays success message
10. System redirects to details or list

**Postconditions:**
- Outgoing letter updated
- ModifiedOn and ModifiedBy set
- Changes visible

---

#### Use Case UC-12.9: Delete Outgoing Letter

| Field | Value |
|-------|-------|
| **ID** | UC-12.9 |
| **Name** | Delete Outgoing Letter |
| **Actor** | Admin, Super Admin, Accountant, Employee |
| **Priority** | Medium |
| **Description** | Delete an outgoing letter record |

**Preconditions:**
- Admin, Super Admin, Accountant, or Employee is logged in
- Outgoing letter exists

**Main Flow:**
1. User on "Outgoing Letters" page
2. User clicks Delete button on a letter
3. System displays confirmation dialog
4. User confirms deletion
5. System performs soft delete
6. System displays success message
7. System refreshes list

**Postconditions:**
- Outgoing letter marked as deleted
- Removed from active views

---

#### Use Case UC-12.10: View Outgoing Letter Details

| Field | Value |
|-------|-------|
| **ID** | UC-12.10 |
| **Name** | View Outgoing Letter Details |
| **Actor** | Admin, Super Admin, Accountant, Employee |
| **Priority** | Medium |
| **Description** | View full details of an outgoing letter |

**Preconditions:**
- Admin, Super Admin, Accountant, or Employee is logged in
- Outgoing letter exists

**Main Flow:**
1. User on "Outgoing Letters" page
2. User clicks letter Subject or View button
3. System displays details page with:
   - **Header:** Subject, Actions (Edit, Delete, Back)
   - **Details:** Serial, Subject, OutGoingId, OutGoingNumber, Date, Year
   - **Department:** Name
   - **Category:** Name
   - **Reply To:** Incoming letter (if linked)
   - **Body:** Full content
   - **Child Letters:** List of follow-up letters (if any)
   - **Attachments:** File name and download link
   - **Meta:** Created By, Created On, Modified By, Modified On
4. User can view all information
5. User can navigate to related letters

**Postconditions:**
- Letter details displayed

---

## Summary of Use Cases

| Use Case | Description | Actor | Priority |
|----------|-------------|-------|----------|
| UC-12.1 | List Incoming Letters | Admin, Super Admin, Accountant, Employee | High |
| UC-12.2 | Add Incoming Letter | Admin, Super Admin, Accountant, Employee | High |
| UC-12.3 | Edit Incoming Letter | Admin, Super Admin, Accountant, Employee | High |
| UC-12.4 | Delete Incoming Letter | Admin, Super Admin, Accountant, Employee | Medium |
| UC-12.5 | View Incoming Letter Details | Admin, Super Admin, Accountant, Employee | Medium |
| UC-12.6 | List Outgoing Letters | Admin, Super Admin, Accountant, Employee | High |
| UC-12.7 | Add Outgoing Letter | Admin, Super Admin, Accountant, Employee | High |
| UC-12.8 | Edit Outgoing Letter | Admin, Super Admin, Accountant, Employee | High |
| UC-12.9 | Delete Outgoing Letter | Admin, Super Admin, Accountant, Employee | Medium |
| UC-12.10 | View Outgoing Letter Details | Admin, Super Admin, Accountant, Employee | Medium |

---

**End of Use Cases for Module 4.12: Incoming & Outgoing Correspondence Management**
