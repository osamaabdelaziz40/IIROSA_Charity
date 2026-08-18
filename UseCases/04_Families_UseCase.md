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
   - **Father Information** (mandatory - at least basic info required):
     - Full Name (required)
     - National ID / Passport (required)
     - Date of Birth (required)
     - Place of Birth
     - Education Level (dropdown)
     - Job
     - Monthly Income
     - Health Status (dropdown)
     - Phone
     - Is Alive (checkbox, default: true)
     - Is Provider (checkbox)
     - Death Date (if not alive)
   - **Mother Information** (mandatory - at least basic info required):
     - Full Name (required)
     - National ID / Passport (required)
     - Date of Birth (required)
     - Place of Birth
     - Education Level (dropdown)
     - Job
     - Monthly Income
     - Health Status (dropdown)
     - Phone
     - Is Alive (checkbox, default: true)
     - Is Provider (checkbox)
     - Death Date (if not alive)
   - **Other Provider Information** (if provider is Other):
     - Full Name (required)
     - Relationship to Family (dropdown)
     - National ID / Passport (required)
     - Phone (required)
     - Address
     - Job
     - Monthly Income
   - **Other Family Relatives** (optional):
     - Add Relative button (can add multiple)
     - For each relative:
       - Full Name (required)
       - Relationship Type (dropdown: Brother, Sister, Grandfather, Grandmother, Uncle, Aunt, Cousin, Other)
       - Gender (dropdown: Male/Female)
       - Date of Birth
       - National ID / Passport
       - Education Level
       - Job
       - Health Status
       - Phone
       - Is Alive (checkbox)
       - Is LivingWithFamily (checkbox)
       - Notes
   - **Initial Orphans** (optional, can add later):
     - List of orphans to add to family
5. User fills in required fields:
   - Address (required)
   - Father Information (mandatory - at minimum Full Name, National ID, Date of Birth)
   - Mother Information (mandatory - at minimum Full Name, National ID, Date of Birth)
   - Provider Type (required)
   - Other relatives (optional)
6. System validates required fields
7. System auto-generates family code if not provided
8. System creates family record
9. System links family to current user's charity (FK_CharityId)
10. System creates father record (mandatory)
11. System creates mother record (mandatory)
12. System creates provider record (if other)
13. System creates relative records (if any added)
14. System creates orphan records (if provided)
15. System logs family creation in audit log
16. System displays success message
17. Family appears in family list (only for this charity)

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
- Father and Mother records created (mandatory)
- Provider record created (if other)
- Relative records created (if any added)
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
12. **SIMULTANEOUSLY** System creates audit log entry (per UC-17.1):
    - AuditLogId (GUID)
    - EntityType = "Father"
    - EntityId = FatherId
    - Operation = Create
    - UserId = current user ID
    - UserName
    - Timestamp (UTC)
    - IpAddress
    - FieldChanges (JSON with all father fields):
      ```json
      {
        "FullName": {"old": null, "new": "Ahmed Mohamed Ali"},
        "NationalId": {"old": null, "new": "28501010101234"},
        "DateOfBirth": {"old": null, "new": "1975-05-15"},
        "Job": {"old": null, "new": "Driver"},
        "HealthStatus": {"old": null, "new": "Good"},
        "Phone": {"old": null, "new": "+201000000001"},
        "IsAlive": {"old": null, "new": true},
        "IsProvider": {"old": null, "new": true},
        "FamilyId": {"old": null, "new": "[Family-GUID]"}
      }
      ```
13. System saves audit log entry
14. System displays success message
15. Father visible in family details

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
13. **SIMULTANEOUSLY** System creates audit log entry (per UC-17.1):
    - AuditLogId (GUID)
    - EntityType = "Orphan"
    - EntityId = OrphanId
    - Operation = Create
    - UserId = current user ID
    - UserName
    - Timestamp (UTC)
    - IpAddress
    - FieldChanges (JSON with all orphan fields):
      ```json
      {
        "FullName": {"old": null, "new": "Omar Ahmed Ali"},
        "Gender": {"old": null, "new": "Male"},
        "DateOfBirth": {"old": null, "new": "2015-03-10"},
        "OrphanType": {"old": null, "new": "Father-deceased"},
        "SponsorshipStatus": {"old": null, "new": "Unsponsored"},
        "EducationLevel": {"old": null, "new": "Primary"},
        "HealthStatus": {"old": null, "new": "Good"},
        "FamilyId": {"old": null, "new": "[Family-GUID]"},
        "CharityId": {"old": null, "new": "[Charity-GUID]"}
      }
      ```
14. System saves audit log entry
15. System displays success message
16. Orphan visible in family details
17. Orphan visible in orphan list

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
8. System retrieves current provider information
9. System updates family provider information
10. System marks selected person as provider
11. **SIMULTANEOUSLY** System creates audit log entry (per UC-17.2):
    - AuditLogId (GUID)
    - EntityType = "Family"
    - EntityId = FamilyId
    - Operation = Update (provider change)
    - UserId = current user ID
    - UserName
    - Timestamp (UTC)
    - IpAddress
    - FieldChanges (JSON):
      ```json
      {
        "ProviderType": {"old": "Father", "new": "Mother"},
        "PreviousProvider": "Father",
        "NewProvider": "Mother"
      }
      ```
12. System saves audit log entry
13. System displays success message

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

#### Use Case UC-4.7: Add Family Relative

| Field | Value |
|-------|-------|
| **ID** | UC-4.7 |
| **Name** | Add Family Relative |
| **Actor** | Charity |
| **Priority** | Low |
| **Description** | Register additional family relatives (siblings, grandparents, uncles, aunts, etc.) linked to a family |

**Preconditions:**
- Charity user is logged in
- Family exists (and belongs to charity's data)
- Father and Mother records already exist

**Main Flow:**
1. Charity user navigates to Family Management page
2. System displays list of families (only own charity's families)
3. User selects family
4. System displays family details with "Relatives" section
5. User clicks "Add Relative"
6. System displays relative creation form:
   - **Basic Information:**
     - Full Name (required)
     - Relationship Type (dropdown: Brother, Sister, Grandfather, Grandmother, Uncle, Aunt, Cousin, Guardian, Other)
     - Gender (dropdown: Male/Female, required)
     - Date of Birth (required)
     - Place of Birth
     - National ID / Passport
   - **Contact & Status:**
     - Phone
     - Address (if different from family)
     - Is Alive (checkbox, default: true)
     - Is LivingWithFamily (checkbox, default: false)
     - Death Date (if not alive)
   - **Additional Information:**
     - Education Level (dropdown)
     - Job
     - Monthly Income
     - Health Status (dropdown)
     - Notes
7. User fills in required fields:
   - Full Name (required)
   - Relationship Type (required)
   - Gender (required)
   - Date of Birth (required)
8. User clicks "Save"
9. System validates required fields
10. System creates relative record
11. System links relative to family
12. **SIMULTANEOUSLY** System creates audit log entry (per UC-17.1):
    - AuditLogId (GUID)
    - EntityType = "Relative"
    - EntityId = RelativeId
    - Operation = Create
    - UserId = current user ID
    - UserName
    - Timestamp (UTC)
    - IpAddress
    - FieldChanges (JSON with all relative fields):
      ```json
      {
        "FullName": {"old": null, "new": "Mohamed Ahmed Ali"},
        "RelationshipType": {"old": null, "new": "Brother"},
        "Gender": {"old": null, "new": "Male"},
        "DateOfBirth": {"old": null, "new": "2010-07-20"},
        "Phone": {"old": null, "new": "+201000000003"},
        "IsLivingWithFamily": {"old": null, "new": true},
        "FamilyId": {"old": null, "new": "[Family-GUID]"}
      }
      ```
13. System saves audit log entry
14. System displays success message
15. Relative visible in family details under "Relatives" section

**Alternative Flows:**
- **9a. Validation fails:** System highlights missing fields (Full Name, Relationship Type, Gender, Date of Birth)

**Business Rules:**
- Relative records are optional
- Can add multiple relatives to same family
- Living status tracked (IsAlive, IsLivingWithFamily)
- Guardian relationship type for legal guardians

**Postconditions:**
- Relative record created
- Linked to family
- Audit log contains creation record

---

#### Use Case UC-4.8: Update Relative Details

| Field | Value |
|-------|-------|
| **ID** | UC-4.8 |
| **Name** | Update Relative Details |
| **Actor** | Charity |
| **Priority** | Low |
| **Description** | Modify relative information for existing family relative |

**Preconditions:**
- Charity user is logged in
- Family exists (and belongs to charity's data)
- Relative exists for this family
- Charity has Update rights enabled

**Main Flow:**
1. Charity user navigates to Family Management page
2. System displays list of families (only own charity's families)
3. User selects family
4. System displays family details with "Relatives" section
5. User clicks "Edit" next to relative
6. System displays relative edit form with current data
7. User modifies desired fields:
   - Full Name
   - Relationship Type
   - Phone
   - Is Living With Family
   - Job
   - Health Status
   - Notes
8. User clicks "Save"
9. System validates data
10. System retrieves current relative values (before update)
11. System updates relative record
12. **SIMULTANEOUSLY** System creates audit log entry (per UC-17.2):
    - AuditLogId (GUID)
    - EntityType = "Relative"
    - EntityId = RelativeId
    - Operation = Update
    - UserId = current user ID
    - UserName
    - Timestamp (UTC)
    - IpAddress
    - FieldChanges (JSON with changed fields):
      ```json
      {
        "FullName": {"old": "Mohamed Ahmed Ali", "new": "Mohamed Ahmed Hassan"},
        "Job": {"old": "Student", "new": "Employee"},
        "Phone": {"old": "+201000000003", "new": "+201000000333"},
        "IsLivingWithFamily": {"old": true, "new": false}
      }
      ```
13. System saves audit log entry
14. System displays success message
15. Relative details updated in family view

**Postconditions:**
- Relative information updated
- Audit log contains changes

---

#### Use Case UC-4.9: Remove Relative from Family

| Field | Value |
|-------|-------|
| **ID** | UC-4.9 |
| **Name** | Remove Relative from Family |
| **Actor** | Charity |
| **Priority** | Low |
| **Description** | Remove a relative record from family (soft delete preserving data) |

**Preconditions:**
- Charity user is logged in
- Family exists (and belongs to charity's data)
- Relative exists for this family

**Main Flow:**
1. Charity user navigates to Family Management page
2. System displays list of families (only own charity's families)
3. User selects family
4. System displays family details with "Relatives" section
5. User clicks "Remove" next to relative
6. System displays confirmation dialog: "Remove [Relative Name] from family? This will preserve record but hide from view."
7. User confirms
8. System sets relative.IsActive = false
9. System sets relative.IsDeleted = true (soft delete)
10. **SIMULTANEOUSLY** System creates audit log entry (per UC-17.3):
    - AuditLogId (GUID)
    - EntityType = "Relative"
    - EntityId = RelativeId
    - Operation = Delete (soft delete)
    - UserId = current user ID
    - UserName
    - Timestamp (UTC)
    - IpAddress
    - FieldChanges (JSON):
      ```json
      {
        "IsActive": {"old": true, "new": false},
        "IsDeleted": {"old": false, "new": true},
        "DeletionTime": {"old": null, "new": "2026-06-03T10:00:00Z"},
        "DeleterId": {"old": null, "new": "[User-GUID]"},
        "RelativeName": "Mohamed Ahmed Ali"
      }
      ```
11. System saves audit log entry
12. System displays success message
13. Relative no longer visible in family relatives list

**Alternative Flows:**
- **7a. User cancels:** System returns to family details without changes

**Postconditions:**
- Relative marked as deleted (soft delete)
- Audit log contains removal record
- Data preserved for reporting

---

#### Use Case UC-4.10: Verify Parent as Provider

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
9. System retrieves current provider status
10. System updates provider verification status
11. **SIMULTANEOUSLY** System creates audit log entry (per UC-17.2):
    - AuditLogId (GUID)
    - EntityType = "Family"
    - EntityId = FamilyId
    - Operation = Update (provider verification)
    - UserId = current user ID
    - UserName
    - Timestamp (UTC)
    - IpAddress
    - FieldChanges (JSON):
      ```json
      {
        "FatherIsProvider": {"old": false, "new": true},
        "MotherIsProvider": {"old": false, "new": false},
        "VerificationDate": {"old": null, "new": "2026-06-02T15:00:00Z"},
        "VerificationNotes": "Verified during home visit"
      }
      ```
12. System saves audit log entry
13. System displays success message
14. Verification date/time recorded

**Postconditions:**
- Provider status verified
- Audit log contains verification

---

#### Use Case UC-4.11: Update Family Information

| Field | Value |
|-------|-------|
| **ID** | UC-4.11 |
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
8. System retrieves current family values (before update)
9. System updates family record
10. **SIMULTANEOUSLY** System creates audit log entry (per UC-17.2):
    - AuditLogId (GUID)
    - EntityType = "Family"
    - EntityId = FamilyId
    - Operation = Update
    - UserId = current user ID
    - UserName
    - Timestamp (UTC)
    - IpAddress
    - FieldChanges (JSON with changed fields):
      ```json
      {
        "Address": {"old": "123 Old Street", "new": "456 New Street"},
        "Phone": {"old": "+201000000000", "new": "+201111111111"},
        "LivingCondition": {"old": "Good", "new": "Fair"},
        "HousingType": {"old": "Apartment", "new": "House"}
      }
      ```
11. System saves audit log entry
12. System displays success message
13. Family list reflects changes

**Business Rules:**
- Charity can ONLY update own families
- Admin/SuperAdmin can update any family (with charity filter)

**Postconditions:**
- Family information updated
- Audit log contains field changes

---

#### Use Case UC-4.12: Update Father Details

| Field | Value |
|-------|-------|
| **ID** | UC-4.12 |
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
10. System retrieves current father values (before update)
11. System updates father record
12. **SIMULTANEOUSLY** System creates audit log entry (per UC-17.2):
    - AuditLogId (GUID)
    - EntityType = "Father"
    - EntityId = FatherId
    - Operation = Update
    - UserId = current user ID
    - UserName
    - Timestamp (UTC)
    - IpAddress
    - FieldChanges (JSON with changed fields):
      ```json
      {
        "FullName": {"old": "Ahmed Mohamed Ali", "new": "Ahmed Mohamed Hassan"},
        "Job": {"old": "Driver", "new": "Merchant"},
        "Phone": {"old": "+201000000001", "new": "+201000000111"},
        "HealthStatus": {"old": "Good", "new": "Fair"},
        "IsAlive": {"old": true, "new": false}
      }
      ```
13. System saves audit log entry
14. System displays success message

**Postconditions:**
- Father information updated
- Audit log contains changes

---

#### Use Case UC-4.13: Update Mother Details

| Field | Value |
|-------|-------|
| **ID** | UC-4.13 |
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
10. System retrieves current mother values (before update)
11. System updates mother record
12. **SIMULTANEOUSLY** System creates audit log entry (per UC-17.2):
    - AuditLogId (GUID)
    - EntityType = "Mother"
    - EntityId = MotherId
    - Operation = Update
    - UserId = current user ID
    - UserName
    - Timestamp (UTC)
    - IpAddress
    - FieldChanges (JSON with changed fields):
      ```json
      {
        "FullName": {"old": "Fatima Mohamed Hassan", "new": "Fatima Mohamed Ali"},
        "Job": {"old": "Housewife", "new": "Teacher"},
        "Phone": {"old": "+201000000002", "new": "+201000000222"},
        "HealthStatus": {"old": "Good", "new": "Excellent"}
      }
      ```
13. System saves audit log entry
14. System displays success message

**Postconditions:**
- Mother information updated
- Audit log contains changes

---

#### Use Case UC-4.14: Update Orphan Details

| Field | Value |
|-------|-------|
| **ID** | UC-4.14 |
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
8. System retrieves current orphan values (before update)
9. System updates orphan record
10. **SIMULTANEOUSLY** System creates audit log entry (per UC-17.2):
    - AuditLogId (GUID)
    - EntityType = "Orphan"
    - EntityId = OrphanId
    - Operation = Update
    - UserId = current user ID
    - UserName
    - Timestamp (UTC)
    - IpAddress
    - FieldChanges (JSON with changed fields):
      ```json
      {
        "FullName": {"old": "Omar Ahmed Ali", "new": "Omar Ahmed Hassan"},
        "SponsorshipStatus": {"old": "Unsponsored", "new": "Sponsored"},
        "EducationLevel": {"old": "Primary", "new": "Preparatory"},
        "SchoolName": {"old": "Al-Noor School", "new": "Al-Huda School"},
        "HealthStatus": {"old": "Good", "new": "Excellent"}
      }
      ```
11. System saves audit log entry
12. System displays success message
13. Orphan list reflects changes

**Postconditions:**
- Orphan information updated
- Audit log contains changes

---

#### Use Case UC-4.15: View Family List

| Field | Value |
|-------|-------|
| **ID** | UC-4.15 |
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
   - Relative Count
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
10. System provides "Export to Excel" button to export current filtered/sorted family list
11. User can export list to Excel format with all displayed columns

**Business Rules:**
- Charity: WHERE CharityId = CurrentUser.CharityId
- Admin/SuperAdmin: All records (with optional Charity filter)

**Postconditions:**
- Family list displayed according to data isolation rules
- Charity sees only own families
- Admin/SuperAdmin can filter by charity
- Family data can be exported to Excel

---

#### Use Case UC-4.16: View Family Details

| Field | Value |
|-------|-------|
| **ID** | UC-4.16 |
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
   - **Relatives Section:**
     - List of all relatives in family
     - For each relative: Name, Relationship Type, Age, Is Living With Family, Contact
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
9. System provides "Add Relative" button (if user has add rights)
10. System provides "Attach Document" button

**Business Rules:**
- Charity: Can only view families where CharityId = CurrentUser.CharityId
- Admin/SuperAdmin: Can view all families
- System enforces data isolation at database level

**Postconditions:**
- Family details displayed according to data isolation
- Charity sees only own families
- System validates access on every request

---

#### Use Case UC-4.17: Deactivate Family

| Field | Value |
|-------|-------|
| **ID** | UC-4.17 |
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
11. **SIMULTANEOUSLY** System creates audit log entry (per UC-17.3):
    - AuditLogId (GUID)
    - EntityType = "Family"
    - EntityId = FamilyId
    - Operation = Delete (soft delete/deactivation)
    - UserId = current user ID
    - UserName
    - Timestamp (UTC)
    - IpAddress
    - FieldChanges (JSON):
      ```json
      {
        "IsActive": {"old": true, "new": false},
        "IsDeleted": {"old": false, "new": true},
        "DeletionTime": {"old": null, "new": "2026-06-02T16:00:00Z"},
        "DeleterId": {"old": null, "new": "[User-GUID]"},
        "DeletionReason": "Family moved abroad"
      }
      ```
12. System saves audit log entry
13. System displays success message
14. Family no longer appears in default family list
15. Family visible in "Inactive Families" view (if enabled)

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

#### Use Case UC-4.18: Attach Family Documents

| Field | Value |
|-------|-------|
| **ID** | UC-4.18 |
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

