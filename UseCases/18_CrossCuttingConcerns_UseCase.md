### 4.18 Module: Cross-Cutting Concerns

**Module Owner:** All Users (varies by concern)  
**Purpose:** System-wide functionalities that apply across multiple modules  

#### Use Case UC-18.1: Attach Document to Entity

| Field | Value |
|-------|-------|
| **ID** | UC-18.1 |
| **Name** | Attach Document to Entity |
| **Actor** | All Users |
| **Priority** | Medium |
| **Description** | Upload and attach files to any entity (families, projects, etc.) |

**Preconditions:**
- User is logged in
- User has permission to modify entity
- Entity exists

**Main Flow:**
1. User views entity details (e.g., family, project, mission)
2. System displays "Attachments" section with "Add Attachment" button
3. User clicks "Add Attachment"
4. System displays file upload dialog:
   - File Selection (browse or drag-and-drop)
   - Document Type (dropdown: ID, Birth Certificate, Contract, Photo, Report, Other)
   - Description (optional)
   - Date (default: today)
5. User selects file
6. User selects document type
7. User enters description (optional)
8. User clicks "Upload"
9. System validates file:
   - File type (allowed types)
   - File size (max 10MB)
   - Virus scan (if available)
10. System uploads file to storage
11. System generates unique file ID (GUID)
12. System creates attachment record:
    - AttachmentId (GUID)
    - FileName
    - FilePath/URL
    - FileSize
    - ContentType
    - DocumentType
    - Description
    - UploadDate
    - UploadedBy (FK_UserId)
    - EntityType (e.g., "Family")
    - EntityId (GUID of entity)
13. System logs attachment creation in audit log
14. System displays success message
15. Attachment visible in entity's attachments list
16. User can download attachment by clicking filename

**Postconditions:**
- Document attached to entity
- Attachment record created
- File stored securely
- Audit log contains attachment record

---

#### Use Case UC-18.2: View Entity Attachments

| Field | Value |
|-------|-------|
| **ID** | UC-18.2 |
| **Name** | View Entity Attachments |
| **Actor** | All Users |
| **Priority** | Low |
| **Description** | View all files attached to specific entity record |

**Preconditions:**
- User is logged in
- User has permission to view entity
- Entity exists

**Main Flow:**
1. User views entity details
2. System displays "Attachments" section
3. System retrieves attachments WHERE EntityId = current entity
4. System displays attachments in list/grid:
   - FileName (clickable to download)
   - Document Type
   - Description
   - File Size
   - Upload Date
   - Uploaded By (user name)
   - Actions (Download, Delete - if user has permission)
5. System provides filter by document type
6. System provides search by filename
7. User can click attachment to download
8. User can delete attachments (if has permission)

**Postconditions:**
- All attachments visible
- Downloadable
- Manageable

---

#### Use Case UC-18.3: Delete Attachment

| Field | Value |
|-------|-------|
| **ID** | UC-18.3 |
| **Name** | Delete Attachment |
| **Actor** | All Users |
| **Priority** | Medium |
| **Description** | Remove attached file from entity |

**Preconditions:**
- User is logged in
- User has permission to modify entity
- Attachment exists

**Main Flow:**
1. User views entity attachments
2. System displays attachments list
3. User clicks "Delete" button next to attachment
4. System displays confirmation: "Delete this attachment? This cannot be undone."
5. User confirms deletion
6. System deletes attachment record (soft delete preferred)
7. System deletes file from storage OR marks for deletion
8. System logs deletion in audit log:
    - AttachmentId
    - FileName
    - EntityId
    - DeletedBy
9. System displays success message
10. Attachment no longer visible in list
11. File removed from storage

**Alternative Flows:**
- **5a. User cancels:** System returns without changes

**Postconditions:**
- Attachment deleted
- File removed from storage
- Audit log contains deletion record

---

#### Use Case UC-18.4: Download Attachment

| Field | Value |
|-------|-------|
| **ID** | UC-18.4 |
| **Name** | Download Attachment |
| **Actor** | All Users |
| **Priority** | Low |
| **Description** | Download attached file to local system |

**Preconditions:**
- User is logged in
- User has permission to view entity
- Attachment exists

**Main Flow:**
1. User views entity attachments
2. System displays attachments list
3. User clicks attachment filename
4. System validates user has access to entity
5. System retrieves file from storage
6. System streams file to user's browser
7. Browser downloads file to user's device
8. System logs download in audit log:
    - AttachmentId
    - FileName
    - DownloadedBy
    - DownloadTimestamp
9. File saved on user's device

**Postconditions:**
- File downloaded
- Access logged
- Audit trail maintained

---

#### Use Case UC-18.5: Send Notification

| Field | Value |
|-------|-------|
| **ID** | UC-18.5 |
| **Name** | Send Notification |
| **Actor** | System (Automatic) |
| **Priority** | High |
| **Description** | Send notification to users based on events |

**Preconditions:**
- System event occurs
- User notification preferences configured

**Main Flow:**
1. System event occurs (e.g., record created, status changed, ticket assigned)
2. System identifies users who should be notified
3. System checks user notification preferences:
   - Email notifications (enabled/disabled)
   - In-app notifications (enabled/disabled)
   - SMS notifications (if configured)
4. System creates notification record:
   - NotificationId (GUID)
   - UserId (recipient)
   - Title
   - Message
   - Type (dropdown: Info, Warning, Success, Error)
   - Priority (Low, Medium, High, Urgent)
   - ActionUrl (link to related entity)
   - CreatedDate
   - IsRead (false, initially)
5. System saves notification to database
6. System sends notifications via enabled channels:
   - **In-App:** Notification appears in user's notification center
   - **Email:** Email sent to user's email address
   - **SMS:** SMS sent to user's phone (if configured)
7. System logs notification delivery
8. User receives notification

**Postconditions:**
- Notification created and delivered
- User informed of event
- Notification logged

---

#### Use Case UC-18.6: View Notifications

| Field | Value |
|-------|-------|
| **ID** | UC-18.6 |
| **Name** | View Notifications |
| **Actor** | All Users |
| **Priority** | Medium |
| **Description** | View user notifications and alerts |

**Preconditions:**
- User is logged in
- Notifications exist

**Main Flow:**
1. User logged into system
2. System displays notification bell/icon in header
3. System shows badge with unread notification count
4. User clicks notification bell/icon
5. System displays notification dropdown/list:
   - **For each notification:**
     - Title
     - Message
     - Type (with icon/color coding)
     - Created Date (relative time: "2 hours ago")
     - IsRead indicator (unread highlighted)
     - Action Link (clickable)
6. System shows most recent notifications first
7. System provides "Mark All as Read" button
8. System provides "View All Notifications" link
9. User can click notification to view details
10. User can click action link to navigate to related entity

**Postconditions:**
- Notifications visible
- Unread count shown
- User can stay informed

---

#### Use Case UC-18.7: Mark Notification as Read

| Field | Value |
|-------|-------|
| **ID** | UC-18.7 |
| **Name** | Mark Notification as Read |
| **Actor** | All Users |
| **Priority** | Low |
| **Description** | Mark specific notification as read |

**Preconditions:**
- User is logged in
- Unread notification exists

**Main Flow:**
1. User views notification list
2. System displays notifications with unread indicators
3. User clicks on notification
4. System marks notification as read:
   - Sets notification.IsRead = true
   - Sets notification.ReadDate = current timestamp
5. System updates notification record
6. System logs read action in audit log
7. Notification no longer highlighted as unread
8. Unread count badge decreases
9. If user clicks "Mark All as Read":
   - System marks all user's notifications as read
   - System clears unread badge

**Postconditions:**
- Notification marked as read
- Unread count updated
- User acknowledged notification

---

#### Use Case UC-18.8: Configure Dynamic Lists

| Field | Value |
|-------|-------|
| **ID** | UC-18.8 |
| **Name** | Configure Dynamic Lists |
| **Actor** | Super Admin |
| **Priority** | Medium |
| **Description** | Set up configurable lists and lookups for dropdowns and selections |

**Preconditions:**
- Super Admin is logged in

**Main Flow:**
1. Super Admin navigates to Dynamic Lists configuration
2. System displays list of configurable lists
3. Super Admin selects list to configure
4. System displays list items with:
   - Value (stored in database)
   - Display Text (Arabic)
   - Display Text (English)
   - Order
   - IsActive
   - Color/Icon (optional)
5. Super Admin can:
   - Add new list items
   - Edit existing items
   - Reorder items
   - Activate/deactivate items
   - Set colors/icons
6. Super Admin clicks "Save Configuration"
7. System updates list configuration
8. System log changes in audit log
9. System displays success message
10. Updated list visible in dropdowns throughout system

**Postconditions:**
- Dynamic list configured
- Dropdowns reflect configuration
- No code changes needed

---

#### Use Case UC-18.9: View Dynamic List Items

| Field | Value |
|-------|-------|
| **ID** | UC-18.9 |
| **Name** | View Dynamic List Items |
| **Actor** | All Users |
| **Priority** | Low |
| **Description** | View items from configured lookup lists |

**Preconditions:**
- User is logged in
- Dynamic lists configured

**Main Flow:**
1. User interacts with system (forms, filters, dropdowns)
2. System displays dropdown/select controls
3. System populates dropdowns from dynamic lists:
   - Retrieves active items
   - Orders by Order field
   - Displays in user's language (Arabic or English)
4. User sees current list values
5. User selects value
6. System uses selected value

**Postconditions:**
- Dynamic list items visible
- User can select from list
- Lists maintained by Super Admin

---

#### Use Case UC-18.10: Search and Filter Records

| Field | Value |
|-------|-------|
| **ID** | UC-18.10 |
| **Name** | Search and Filter Records |
| **Actor** | All Users |
| **Priority** | Medium |
| **Description** | Apply search and filters across all modules with Charity filtering for Admin roles |

**Preconditions:**
- User is logged in
- Records exist in module

**Main Flow:**
1. User navigates to any module (e.g., Families, Projects, Missions)
2. System displays grid/list of records
3. System provides search box:
   - Free-text search across relevant fields
4. System provides filter panel:
   - Filters vary by module
   - Common filters: Date Range, Status, Category, etc.
5. **For Charity role:**
   - System automatically filters by CharityId = CurrentUser.CharityId
   - Charity filter not visible (data isolation)
6. **For Admin/Super Admin:**
   - System provides Charity dropdown filter:
     - All Charities (shows all records)
     - [Charity 1]
     - [Charity 2]
     - ... (all active charities)
   - User can select specific charity to filter
7. User enters search term or applies filters
8. System queries database with filters
9. System updates grid with filtered results
10. System displays result count
11. User can clear filters to see all records

**Business Rules:**
- Charity: Automatic filtering by CharityId
- Admin/SuperAdmin: Optional Charity filtering
- Database-level filtering for security

**Postconditions:**
- Records filtered
- Search results displayed
- Data isolation enforced

---

#### Use Case UC-18.11: Export Any Grid

| Field | Value |
|-------|-------|
| **ID** | UC-18.11 |
| **Name** | Export Any Grid |
| **Actor** | All Users |
| **Priority** | Medium |
| **Description** | Export data grid views to Excel/PDF |

**Preconditions:**
- User is logged in
- User has permission to view data
- Grid has records

**Main Flow:**
1. User views any data grid in system
2. System provides "Export" button above grid
3. User clicks "Export"
4. System displays export options:
   - **Format:** Excel, PDF, CSV
   - **Fields:** Select All checkbox, individual field checkboxes
   - **Filters:** Include Current Filters (checkbox)
   - **Language:** Arabic, English, Both
5. User selects export format
6. User selects fields to export (or Select All)
7. User chooses to include/exclude current filters
8. User clicks "Generate Export"
9. System retrieves data from grid:
   - Applies current filters (if selected)
   - Respects data isolation (Charity filtering)
   - Includes selected fields
10. System generates export file
11. System downloads file to user's device
12. System logs export in audit log:
    - Exported By
    - Module/Entity
    - Record Count
    - Format
13. System displays success message

**Postconditions:**
- Grid data exported
- User gets file with current view
- Audit log contains export record

---

#### Use Case UC-18.12: Apply Charity Data Filter

| Field | Value |
|-------|-------|
| **ID** | UC-18.12 |
| **Name** | Apply Charity Data Filter |
| **Actor** | System (Automatic) |
| **Priority** | Critical |
| **Description** | Automatically filter data by Charity ID for Charity role users |

**Preconditions:**
- Charity user logged in
- Data query executed

**Main Flow:**
1. Charity user accesses any data grid/list
2. System identifies user role = Charity
3. System retrieves user's CharityId
4. User's CharityId stored in JWT token
5. **For EVERY database query:**
   - System automatically adds WHERE clause:
     - `WHERE CharityId = @CurrentUserCharityId`
   - For entities without direct CharityId:
     - System filters through related entities
     - Example: Family → linked to Charity directly
     - Example: Orphan → linked to Family → linked to Charity
6. System executes filtered query
7. System returns only records belonging to user's charity
8. Charity user sees only their data
9. Charity filter NOT visible in UI (automatic, not optional)

**Business Rules:**
- AUTOMATIC and MANDATORY for Charity role
- Cannot be bypassed
- Enforced at database level, not just UI
- Admin/SuperAdmin NOT filtered (see UC-18.13)

**Postconditions:**
- Charity data isolated
- Charity users see only their data
- Security enforced at database level

---

#### Use Case UC-18.13: Validate Charity Access

| Field | Value |
|-------|-------|
| **ID** | UC-18.13 |
| **Name** | Validate Charity Access |
| **Actor** | System (Automatic) |
| **Priority** | Critical |
| **Description** | Verify Charity user can only access their own data on every request |

**Preconditions:**
- Charity user logged in
- API request received

**Main Flow:**
1. Charity user makes API request (read, update, delete)
2. System receives request with entity ID
3. System extracts user's CharityId from JWT token
4. System validates CharityId matches entity's CharityId:
   - **For READ requests:**
     - Query automatically filtered (UC-18.12)
     - User cannot request other charities' data
   - **For UPDATE/DELETE requests:**
     - System validates: entity.CharityId == user.CharityId
     - If mismatch: System returns 403 Forbidden
     - Logs unauthorized access attempt
5. System blocks cross-charity access
6. System logs validation failures in security log
7. Charity users can only access their own data

**Security Rules:**
- Validated on EVERY request
- Cannot bypass validation
- Unauthorized attempts logged
- Admin/SuperAdmin exempt from validation

**Postconditions:**
- Charity access validated
- Cross-charity access blocked
- Security maintained

---


