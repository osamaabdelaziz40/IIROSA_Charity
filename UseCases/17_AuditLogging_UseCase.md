### 4.17 Module: Audit Logging

**Module Owner:** System (Automatic)  
**Purpose:** Track every database change with comprehensive details  
**Dependencies:** All entities  

**Important:** All audit logging is automatic. Users cannot directly create audit logs, but can view them.

#### Use Case UC-17.1: Log Entity Creation

| Field | Value |
|-------|-------|
| **ID** | UC-17.1 |
| **Name** | Log Entity Creation |
| **Actor** | System (Automatic) |
| **Priority** | Critical |
| **Description** | Automatically log record creation with user ID, timestamp, and field values |

**Preconditions:**
- Entity being created
- User authenticated

**Main Flow:**
1. User creates new entity record (e.g., new family)
2. System receives create request
3. System validates user has permission
4. System creates entity record
5. **SIMULTANEOUSLY** System creates audit log entry:
   - AuditLogId (GUID)
   - EntityType (e.g., "Family")
   - EntityId (GUID of created record)
   - Operation (enum: Create)
   - UserId (current user ID)
   - UserName
   - Timestamp (UTC)
   - IpAddress
   - FieldChanges (JSON with all field values after creation)
   - Changes (JSON: {"field": {"old": null, "new": "value"}})
6. System saves audit log entry to database
7. Audit log entry linked to entity via EntityId
8. Creation logged permanently

**Postconditions:**
- Entity creation logged
- All field values captured
- User and timestamp recorded
- Cannot be modified by users

---

#### Use Case UC-17.2: Log Entity Update

| Field | Value |
|-------|-------|
| **ID** | UC-17.2 |
| **Name** | Log Entity Update |
| **Actor** | System (Automatic) |
| **Priority** | Critical |
| **Description** | Automatically log record modification with user ID, timestamp, and field changes (before/after) |

**Preconditions:**
- Entity being updated
- User authenticated
- Entity exists

**Main Flow:**
1. User requests to update entity record
2. System receives update request
3. System retrieves current entity values (before update)
4. System validates user has permission
5. System validates changes
6. System applies updates to entity
7. **SIMULTANEOUSLY** System creates audit log entry:
   - AuditLogId (GUID)
   - EntityType (e.g., "Family")
   - EntityId (GUID of record)
   - Operation (enum: Update)
   - UserId (current user ID)
   - UserName
   - Timestamp (UTC)
   - IpAddress
   - FieldChanges (JSON with changed fields only):
     - For each changed field:
       - FieldName
       - OldValue
       - NewValue
8. System saves audit log entry
9. Update logged with field-level detail

**Example FieldChanges JSON:**
```json
{
  "Address": {
    "old": "123 Old Street",
    "new": "456 New Street"
  },
  "Phone": {
    "old": "555-1234",
    "new": "555-5678"
  }
}
```

**Postconditions:**
- Entity update logged
- Before/after values captured for changed fields
- User and timestamp recorded
- Cannot be modified by users

---

#### Use Case UC-17.3: Log Entity Deletion

| Field | Value |
|-------|-------|
| **ID** | UC-17.3 |
| **Name** | Log Entity Deletion |
| **Actor** | System (Automatic) |
| **Priority** | Critical |
| **Description** | Automatically log record deletion with user ID, timestamp, and deleted data |

**Preconditions:**
- Entity being deleted
- User authenticated
- Entity exists

**Main Flow:**
1. User requests to delete entity record
2. System receives delete request
3. System retrieves current entity values (before deletion)
4. System validates user has permission
5. System performs soft delete:
   - Sets entity.IsDeleted = true
   - Sets entity.DeletionTime = current UTC
   - Sets entity.DeleterId = current user ID
6. **SIMULTANEOUSLY** System creates audit log entry:
   - AuditLogId (GUID)
   - EntityType (e.g., "Family")
   - EntityId (GUID of record)
   - Operation (enum: Delete)
   - UserId (current user ID)
   - UserName
   - Timestamp (UTC)
   - IpAddress
   - FieldChanges (JSON with all field values before deletion)
7. System saves audit log entry
8. Deletion logged with complete record snapshot

**Postconditions:**
- Entity deletion logged
- All field values preserved before deletion
- User and timestamp recorded
- Data recoverable via audit log

---

#### Use Case UC-17.4: Log User Login

| Field | Value |
|-------|-------|
| **ID** | UC-17.4 |
| **Name** | Log User Login |
| **Actor** | System (Automatic) |
| **Priority** | High |
| **Description** | Track user login events with timestamp and IP address |

**Preconditions:**
- User attempting login

**Main Flow:**
1. User submits login credentials
2. System validates credentials
3. If credentials valid:
   - System generates JWT token
   - System logs login event:
     - AuditLogId (GUID)
     - EntityType = "UserLogin"
     - EntityId = UserId
     - Operation = "Login"
     - UserId
     - UserName
     - Timestamp (UTC)
     - IpAddress
     - FieldChanges (JSON with login details)
4. System saves login audit log
5. User login successful

**Postconditions:**
- Login event logged
- Timestamp and IP recorded
- Used for security monitoring

---

#### Use Case UC-17.5: Log User Logout

| Field | Value |
|-------|-------|
| **ID** | UC-17.5 |
| **Name** | Log User Logout |
| **Actor** | System (Automatic) |
| **Priority** | Medium |
| **Description** | Track user logout events with timestamp |

**Preconditions:**
- User logging out

**Main Flow:**
1. User clicks logout
2. System invalidates JWT token
3. System logs logout event:
   - AuditLogId (GUID)
   - EntityType = "UserLogout"
   - EntityId = UserId
   - Operation = "Logout"
   - UserId
   - UserName
   - Timestamp (UTC)
   - IpAddress
4. System saves logout audit log
5. User logged out

**Postconditions:**
- Logout event logged
- Session tracking complete

---

#### Use Case UC-17.6: Log Failed Login Attempts

| Field | Value |
|-------|-------|
| **ID** | UC-17.6 |
| **Name** | Log Failed Login Attempts |
| **Actor** | System (Automatic) |
| **Priority** | High |
| **Description** | Record failed login attempts for security monitoring |

**Preconditions:**
- Failed login attempt

**Main Flow:**
1. User submits login credentials
2. System validates credentials
3. If credentials INVALID:
   - System logs failed login attempt:
     - AuditLogId (GUID)
     - EntityType = "FailedLogin"
     - EntityId = null (no user context)
     - Operation = "FailedLogin"
     - UserId = null
     - UserName = attempted username
     - Timestamp (UTC)
     - IpAddress
     - FieldChanges (JSON with reason for failure)
4. System increments failed login counter for IP/username
5. If exceeded threshold:
   - System locks account or IP
   - System logs lock event
6. Failed login recorded

**Postconditions:**
- Failed login logged
- Security monitoring data
- Account lockout triggered if threshold exceeded

---

#### Use Case UC-17.7: View Audit Logs

| Field | Value |
|-------|-------|
| **ID** | UC-17.7 |
| **Name** | View Audit Logs |
| **Actor** | Super Admin, Admin |
| **Priority** | Medium |
| **Description** | View comprehensive audit logs with filtering by entity, user, date range |

**Preconditions:**
- Super Admin or Admin is logged in
- Audit logs exist

**Main Flow:**
1. Admin/Super Admin navigates to Audit Logs page
2. System displays audit log search interface with filters:
   - **Filters:**
     - Entity Type (dropdown: All, Family, Orphan, Charity, etc.)
     - Operation (dropdown: All, Create, Update, Delete, Login, Logout)
     - User (dropdown of users)
     - Date Range From (date picker)
     - Date Range To (date picker)
     - Entity Id (text input)
   - **Search:** By username, entity type, etc.
3. User applies filters
4. System queries audit log based on filters
5. System displays audit logs in grid with columns:
   - Timestamp
   - Operation
   - Entity Type
   - Entity Id
   - User Name
   - IP Address
   - Details (expandable)
6. System provides pagination
7. System provides sorting by timestamp
8. User can click log entry to view details
9. System can export filtered logs

**Postconditions:**
- Audit logs displayed
- Filtering enables specific searches
- Security monitoring possible

---

#### Use Case UC-17.8: View Entity Change History

| Field | Value |
|-------|-------|
| **ID** | UC-17.8 |
| **Name** | View Entity Change History |
| **Actor** | Super Admin, Admin |
| **Priority** | Medium |
| **Description** | View complete modification history for specific record |

**Preconditions:**
- Super Admin or Admin is logged in
- Entity exists

**Main Flow:**
1. Admin/Super Admin views entity details (e.g., family details)
2. System provides "View History" button
3. User clicks "View History"
4. System queries audit logs WHERE EntityId = current entity
5. System retrieves all audit log entries for this entity
6. System displays change history in chronological order:
   - **For each log entry:**
     - Timestamp
     - Operation (Create, Update, Delete)
     - User Name
     - Field Changes (before/after values)
     - IP Address
7. System highlights changes visually:
   - Added values in green
   - Deleted values in red
   - Modified values highlighted
8. System provides timeline view
9. User can see complete history of record
10. User can export history

**Postconditions:**
- Complete entity history visible
- All changes tracked
- Accountability maintained

---

#### Use Case UC-17.9: View User Activity Log

| Field | Value |
|-------|-------|
| **ID** | UC-17.9 |
| **Name** | View User Activity Log |
| **Actor** | Super Admin |
| **Priority** | Medium |
| **Description** | View all actions performed by specific user |

**Preconditions:**
- Super Admin is logged in
- User exists

**Main Flow:**
1. Super Admin navigates to User Management page
2. System displays list of users
3. Super Admin selects user
4. System displays user details with "View Activity" button
5. Super Admin clicks "View Activity"
6. System queries audit logs WHERE UserId = selected user
7. System retrieves all audit log entries for user
8. System displays user activity in chronological order:
   - Timestamp
   - Operation
   - Entity Type
   - Entity Details
   - IP Address
   - Field Changes
9. System provides summary statistics:
   - Total actions
   - Actions by type
   - Actions by entity
   - First activity date
   - Last activity date
10. System provides filter by date range
11. System provides filter by operation type
12. System can export user activity

**Postconditions:**
- User activity visible
- Complete audit trail for user
- Security monitoring enabled

---

#### Use Case UC-17.10: Export Audit Logs

| Field | Value |
|-------|-------|
| **ID** | UC-17.10 |
| **Name** | Export Audit Logs |
| **Actor** | Super Admin |
| **Priority** | Low |
| **Description** | Export audit logs for compliance and reporting |

**Preconditions:**
- Super Admin is logged in
- Audit logs exist

**Main Flow:**
1. Super Admin navigates to Audit Logs page
2. System provides "Export Logs" button
3. Super Admin clicks "Export Logs"
4. System displays export options:
   - Date Range (required)
   - Filters (entity type, operation, user)
   - Export Format (Excel, CSV, JSON)
   - Include Field Changes (checkbox, for detailed export)
5. Super Admin selects export options
6. Super Admin clicks "Generate Export"
7. System queries audit logs based on filters
8. System generates export file with:
   - All audit log fields
   - Field changes (if included)
   - Formatted for readability
9. System downloads file to user's device
10. System logs export in audit log (meta-logging!)
11. System displays success message

**Postconditions:**
- Audit logs exported
- Available for compliance
- Long-term archival

---

#### Use Case UC-17.11: Search Audit Logs

| Field | Value |
|-------|-------|
| **ID** | UC-17.11 |
| **Name** | Search Audit Logs |
| **Actor** | Super Admin, Admin |
| **Priority** | Medium |
| **Description** | Search audit logs by keyword, entity type, or action |

**Preconditions:**
- Super Admin or Admin is logged in
- Audit logs exist

**Main Flow:**
1. Admin/Super Admin navigates to Audit Logs page
2. System displays search box
3. User enters search term:
   - Keyword (searches field values, usernames)
   - Entity Type
   - Operation
4. User applies filters
5. System searches audit logs:
   - Searches FieldChanges JSON for keyword
   - Searches UserName
   - Searches EntityType
6. System displays matching logs
7. System highlights search term in results
8. System shows result count
9. User can refine search

**Postconditions:**
- Audit logs searched
- Specific events found
- Investigation enabled

---

#### Use Case UC-17.12: Compare Record Versions

| Field | Value |
|-------|-------|
| **ID** | UC-17.12 |
| **Name** | Compare Record Versions |
| **Actor** | Super Admin, Admin |
| **Priority** | Low |
| **Description** | Compare different versions of record to see changes |

**Preconditions:**
- Super Admin or Admin is logged in
- Entity has multiple audit log entries

**Main Flow:**
1. Admin/Super Admin views entity history
2. System displays list of audit log entries for entity
3. User selects two log entries to compare:
   - Select "Before" version
   - Select "After" version
4. System retrieves field changes for both versions
5. System displays side-by-side comparison:
   - Field Name | Before Value | After Value
   - Highlights differences
   - Shows unchanged fields
   - Shows added/removed fields
6. System provides visual diff
7. User can see exactly what changed
8. User can understand evolution of record

**Postconditions:**
- Record versions compared
- Changes clearly visible
- Understanding of modifications

---

#### Use Case UC-17.13: Restore Record Version

| Field | Value |
|-------|-------|
| **ID** | UC-17.13 |
| **Name** | Restore Record Version |
| **Actor** | Super Admin |
| **Priority** | Low |
| **Description** | Restore record to previous version from audit log |

**Preconditions:**
- Super Admin is logged in
- Entity exists
- Audit log contains previous version

**Main Flow:**
1. Super Admin views entity history
2. System displays audit log entries
3. Super Admin selects previous version to restore
4. System displays "Restore This Version" button
5. Super Admin clicks "Restore"
6. System displays confirmation dialog:
   - "This will revert the record to the state it was in on [timestamp]. Continue?"
   - System shows what will be changed
7. Super Admin confirms
8. System retrieves field values from selected audit log
9. System updates current entity with historical values
10. System creates NEW audit log entry for this restore:
    - Operation = Update
    - FieldChanges = current values → restored values
11. System logs restore in audit log (meta-logging)
12. System displays success message
13. Record restored to previous state
14. Current values preserved in new audit log

**Alternative Flows:**
- **7a. User cancels:** System returns without changes

**Business Rules:**
- Only Super Admin can restore
- Restore creates new audit log (chain of custody maintained)
- Original audit logs remain untouched

**Postconditions:**
- Record restored to previous version
- New audit log entry created
- Full history preserved

---


