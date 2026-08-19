# WAR.IIROSA - Orphan Register & Coding

سجل الايتام والتكويد | use case prefix `UC-ORP` | chapter 13 of the master document


| Field | Value |
| --- | --- |
| Document title | WAR.IIROSA - Module Specification: Orphan Register & Coding |
| Module | Orphan Register & Coding - سجل الايتام والتكويد |
| Use case prefix | UC-ORP |
| Chapter in master document | Chapter 13 |
| Documented use cases | 11 |
| Principal routes | `#/families/orphans/coding`, `#/families/orphans/coding/worklist` (both *planned*) |
| Version | 1.1 |
| Status | Chapter content extracted verbatim; screen fields and scenarios derived from the source code |
| Date | 18 August 2026 |
| Parent document | WAR.IIROSA-Project-Documentation.docx v1.0 (repository WAR.IIROSA, commit 4beb736) |

## Contents of this document

1. Chapter 13 — module purpose and use-case catalogue (verbatim from the master document)
2. §13.D — detailed specifications carried over from chapter 25
3. §13.S — screen field specifications (every field of every screen, derived from the AngularJS views)
4. §13.U — one expanded scenario per use case (trigger, pre-conditions, main flow, alternates, exceptions, post-conditions)
5. §13.A / §13.B — annexes: screens and Web API controllers of this module

## 13. Orphan Register & Coding

سجل الأيتام والتكويد — coding is the gateway to sponsorship: an orphan without a code cannot receive periodic reports or payments. HQ therefore controls code assignment centrally.


| ID | Use case | Primary actor | Description & main flow | Realisation |
| --- | --- | --- | --- | --- |
| UC-ORP-01 | Check whether an orphan may be added التحقق من إمكانية إضافة يتيم | Charity | Before a child is appended to a family, the system verifies that the charity is permitted to add records and that the family is eligible for another member. | GET /api/Families/orphans/check-national-id |
| UC-ORP-02 | Find an orphan by name البحث باسم اليتيم | All roles | Type-ahead lookup returning matching orphans as id/label pairs, used wherever an orphan must be picked (reports, corrections, correspondence). | GET /api/Families/orphans?search= |
| UC-ORP-03 | Open the coding worklist تكويد الأيتام | Gen. Director, Staff | Lists, for a chosen charity, all orphans still awaiting a sponsorship code, with the family and personal data needed to judge eligibility. | Route `#/families/orphans/coding/worklist` → GET /api/Families/orphans?codingStatus=Pending |
| UC-ORP-04 | Search orphans for coding البحث في قائمة التكويد | Gen. Director, Staff | Legacy coding screen: locates candidate orphans by name within a charity before assigning a code. | Route `#/families/orphans/coding` → GET /api/Families/orphans?search= |
| UC-ORP-05 | Verify a code is not already used التحقق من تفرد الكود | Gen. Director, Staff | Validates a proposed sponsorship code against the charity's existing codes and blocks submission on a clash. | GET /api/Families/orphans/check-code |
| UC-ORP-06 | Assign a sponsorship code to an orphan إسناد كود لليتيم | Gen. Director, Staff | Writes the approved code onto the orphan record. From this point the orphan is admitted to the sponsorship programme and becomes eligible for periodic reports and payment enrolment. | POST /api/Families/orphans/{orphanId}/code |
| UC-ORP-07 | Resolve an orphan's name from a code استعلام بالكود | All roles | Returns the orphan's name and code pair for a given identifier, used to confirm the operator is working on the right beneficiary. | GET /api/Families/orphans?search= |
| UC-ORP-08 | View an orphan's payment history دفعات اليتيم | Charity, HQ roles | Lists every payment batch in which the orphan appeared, most recent first, with the amount, batch date and whether it was stopped or received — the audit trail for beneficiary enquiries. | GET /api/OrphanPayments?orphanId= |
| UC-ORP-09 | View an orphan's payment details تفاصيل دفعة اليتيم | Charity, HQ roles | Retrieves the payment rows attached to one orphan for a chosen batch, including cheque number and receipt state. | GET /api/OrphanPayments |
| UC-ORP-10 | Check a phone number is not duplicated التحقق من رقم الهاتف | Charity | Validates that a contact number being entered for a beneficiary is not already registered elsewhere, per number type. | GET /api/Families/{familyId}/provider/check-phone |
| UC-ORP-11 | Load orphan reference data القوائم المرجعية لليتيم | Charity | Loads the lookup lists that drive the orphan forms — educational level, stage and class, educational status, health status, disability type, gender, relationship, prayer and memorisation levels, hobbies and behaviour scales. | GET /api/OrphanPayments; GET /api/LookupManagement; GET /api/LookupManagement |

### 13.D  Detailed use case specifications (from chapter 25)

Reproduced verbatim from chapter 25 of the master document — the fully expanded specification of this module’s critical end-to-end scenarios. Every other use case of the module is specified in §13.U.

**25.3 UC-ORP-06 — Assign a sponsorship code to an orphan**


| Use case ID | UC-ORP-06 |
| --- | --- |
| Name | Assign a sponsorship code — تكويد اليتيم |
| Primary actor | General Director or Staff |
| Goal | Admit an orphan to the sponsorship programme by giving them a unique code. |
| Pre-conditions | The orphan is registered in a family file and has no code; the reviewer holds an HQ role. |
| Trigger | The reviewer opens the orphan coding screen for a charity. |
| Main flow | 1. The reviewer selects a charity; the system lists its uncoded orphans with the data needed to judge eligibility (UC-ORP-03). 2. The reviewer inspects the candidate's family circumstances, age, and supporting documents. 3. The reviewer proposes a code; the client calls CanAddChildCode to confirm it is unused (UC-ORP-05). 4. On confirmation the client calls SetOrphanCode. 5. The system writes the code to the orphan record and returns success. 6. The orphan disappears from the uncoded worklist and becomes eligible for periodic reporting and payment enrolment. |
| Alternate flows | A1 — Code already used. Step 3 returns false; the reviewer must choose another code. A2 — Candidate not eligible. The reviewer leaves the orphan uncoded; the record stays on the worklist. A3 — Legacy screen. The reviewer locates the candidate by name through GetAllChildrenByName instead of browsing the worklist (UC-ORP-04). |
| Post-conditions | The orphan holds a unique sponsorship code and enters the sponsorship population. |
| Business rules | BR-07 Codes are unique. BR-08 Only HQ roles may assign codes. BR-09 An uncoded orphan cannot have periodic reports or payment rows. |


### 13.S  Screen field specifications

Derived from the AngularJS views of this module. For every screen the table lists each data-entry field in the order it appears, the scope property it binds to, its control type, whether the form marks it mandatory, and the lookup or rule that governs it. Labels are reproduced as they appear on screen (Arabic, right-to-left).

#### 13.S.1  Screen `#/families/orphans/coding`


| Property | Value |
| --- | --- |
| Angular route | `#/families/orphans/coding` |
| Feature module | `families` (lazy-loaded) |
| Component | `OrphanCodingComponent` |
| Route status | planned |
| Data-entry fields | 3 |
| Grids on the screen | 1 |
| Commands | 4 |


| Section | Field (as labelled) | Bound to | Control | Mandatory · source · rules |
| --- | --- | --- | --- | --- |
| — | الجمعية | CharityId | Drop-down list | Optional · options: lookup: Charities (+ كافة الجهات) · on change: getCharityData() |
| تكويد الايتام | اسم اليتيم | searchValue | Text box | Optional |
| اضافة كود | الكود | OrhanCode | Numeric box | Optional |

Grids on this screen:

| Grid (row source) | Columns |
| --- | --- |
| child in Children track by $index | الرقم · إسم اليتيم · إسم الأب · إسم الأم · الجمعية |

Commands on this screen:

| Command | Handler | Shown when |
| --- | --- | --- |
| بحث | Search() | always |
| غلق | EmptyOrphanModel() | always |
| تم | SaveCode() | always |
| (icon only) | ShowModalOfOrphan(child) | always |

#### 13.S.2  Screen `#/families/orphans/coding/worklist`


| Property | Value |
| --- | --- |
| Angular route | `#/families/orphans/coding/worklist` |
| Feature module | `families` (lazy-loaded) |
| Component | `OrphanCodingWorklistComponent` |
| Route status | planned |
| Data-entry fields | 1 |
| Grids on the screen | 1 |
| Commands | 6 |


| Section | Field (as labelled) | Bound to | Control | Mandatory · source · rules |
| --- | --- | --- | --- | --- |
| — | الجمعية | CharityId | Drop-down list | Optional · options: lookup: Charities (+ كافة الجهات) · on change: getCharityData() |

Grids on this screen:

| Grid (row source) | Columns |
| --- | --- |
| orphan in Orphans\|orderBy:orderByField:reverseSort | اسم الجمعية (^) (v) · إسم الام (^) (v) · إسم الابن/الابنة (^) (v) · كود · تعديل الكود |

Commands on this screen:

| Command | Handler | Shown when |
| --- | --- | --- |
| (icon only) | orderByField='CharityName'; reverseSort = !reverseSort | always |
| (icon only) | orderByField='MotherName'; reverseSort = !reverseSort | always |
| (icon only) | orderByField='OrphanName'; reverseSort = !reverseSort | always |
| (icon only) | EditingCode(orphan.OrphanId) | always |
| (icon only) | SaveCode(orphan.OrphanId) | always |
| (icon only) | CancelSavingCode(orphan.OrphanId) | always |

### 13.U  Use case scenarios

One expanded scenario for every use case of this module. Pre-conditions, flows and post-conditions are derived from the screen that hosts the function, the Web API action that serves it and the business-layer method that executes it; quoted messages are the literal strings returned by the business layer.

#### 13.U.1  UC-ORP-01 — Check whether an orphan may be added التحقق من إمكانية إضافة يتيم


| Item | Specification |
| --- | --- |
| Use case ID | UC-ORP-01 |
| Name | Check whether an orphan may be added التحقق من إمكانية إضافة يتيم |
| Type | Create a record |
| Primary actor | Charity |
| Secondary actors | The system (Web API + business layer); the database. |
| Summary | Before a child is appended to a family, the system verifies that the charity is permitted to add records and that the family is eligible for another member. |
| Trigger | The actor selects the function from the module menu or the screen that hosts it. |
| Pre-conditions | 1. The actor is authenticated; the JWT access token is valid and the client holds the role, charity and country claims it carries.<br>2. The actor holds one of: charity.<br>3. Caller identity comes from the JWT, never from the request; the application service scopes the query to the charity (and country) that owns the user, unless the role is an HQ role permitted to pass an explicit `charityId` filter. |
| Main flow | 1. The actor reaches the function from the screen of this module that hosts it. The screen opens in add mode.<br>2. The system loads the reference data the form needs (regions, centres, banks, types and the other lookup lists bound to the drop-downs).<br>3. The actor completes the input fields of the form.<br>4. The actor presses the command button of the screen.<br>5. The SPA issues `GET /api/Families/orphans/check-national-id` carrying id, userId.<br>6. `FamiliesController` binds the typed request DTO and delegates to the application service.<br>7. `IOrphanService` validates the payload with its FluentValidation validator, applies the business rules and the charity scope, and persists through `IUnitOfWork`.<br>8. The business layer validates the payload, stamps the owning charity and the creating user, and persists the new record.<br>9. The system returns the outcome and the SPA confirms the save and returns to the list screen. |
| Alternate flows | • An HQ role (General Director, Financial Director, Staff) may pass an explicit charity id and so read across the charity boundary; a charity user may not and always sees its own data.<br>• The actor abandons the form before saving — nothing is written and the record keeps its previous state. |
| Exception flows | • The session has expired or the role is not permitted — the request is rejected and the SPA routes back to the login state.<br>• A mandatory field is empty or fails its format check — the save is refused and the field is flagged on the form. |
| Post-conditions | • A new record exists, owned by the charity of the creating user, and appears in the list screen of the module. |
| Realisation | `GET /api/Families/orphans/check-national-id` → `FamiliesController` → `IFamilyService` |

#### 13.U.2  UC-ORP-02 — Find an orphan by name البحث باسم اليتيم


| Item | Specification |
| --- | --- |
| Use case ID | UC-ORP-02 |
| Name | Find an orphan by name البحث باسم اليتيم |
| Type | Search / filter |
| Primary actor | All roles |
| Secondary actors | The system (Web API + business layer); the database. |
| Summary | Type-ahead lookup returning matching orphans as id/label pairs, used wherever an orphan must be picked (reports, corrections, correspondence). |
| Trigger | The actor selects the function from the module menu or the screen that hosts it. |
| Pre-conditions | 1. The actor is authenticated; the JWT access token is valid and the client holds the role, charity and country claims it carries.<br>2. The actor holds one of: All roles.<br>3. Caller identity comes from the JWT, never from the request; the application service scopes the query to the charity (and country) that owns the user, unless the role is an HQ role permitted to pass an explicit `charityId` filter. |
| Main flow | 1. The actor reaches the function from the screen of this module that hosts it.<br>2. The actor enters the search criteria in the filter fields.<br>3. The actor presses the command button of the screen.<br>4. The SPA issues `GET /api/Families/orphans?search=` carrying term, userId.<br>5. `FamiliesController` binds the typed request DTO and delegates to the application service.<br>6. `IOrphanService` validates the payload with its FluentValidation validator, applies the business rules and the charity scope, and persists through `IUnitOfWork`.<br>7. The matching rows are returned, scoped to the caller’s charity, and rendered in the result grid. |
| Alternate flows | • An HQ role (General Director, Financial Director, Staff) may pass an explicit charity id and so read across the charity boundary; a charity user may not and always sees its own data.<br>• No row matches the criteria — the grid renders empty and the paging control reports zero pages. |
| Exception flows | • The session has expired or the role is not permitted — the request is rejected and the SPA routes back to the login state. |
| Post-conditions | • No stored data is changed — the operation is a read. |
| Realisation | `GET /api/Families/orphans?search=` → `FamiliesController` → `IFamilyService` |

#### 13.U.3  UC-ORP-03 — Open the coding worklist تكويد الأيتام


| Item | Specification |
| --- | --- |
| Use case ID | UC-ORP-03 |
| Name | Open the coding worklist تكويد الأيتام |
| Type | Read a record |
| Primary actor | Gen. Director, Staff |
| Secondary actors | The system (Web API + business layer); the database. |
| Summary | Lists, for a chosen charity, all orphans still awaiting a sponsorship code, with the family and personal data needed to judge eligibility. |
| Trigger | The actor opens the screen at `#/families/orphans/coding/worklist` from the module menu. |
| Pre-conditions | 1. The actor is authenticated; the JWT access token is valid and the client holds the role, charity and country claims it carries.<br>2. The actor holds one of: Gen. Director, Staff.<br>3. Caller identity comes from the JWT, never from the request; the application service scopes the query to the charity (and country) that owns the user, unless the role is an HQ role permitted to pass an explicit `charityId` filter.<br>4. The SPA route `#/families/orphans/coding/worklist` has loaded and its reference-data lookups have been populated.<br>5. The target record exists and its identifier is known to the screen. |
| Main flow | 1. The actor navigates to the screen at `#/families/orphans/coding/worklist`.<br>2. The actor selects the record to open.<br>3. The SPA issues `GET /api/Families/orphans?codingStatus=Pending` carrying charityId, userId.<br>4. `FamiliesController` binds the typed request DTO and delegates to the application service.<br>5. `IOrphanService` validates the payload with its FluentValidation validator, applies the business rules and the charity scope, and persists through `IUnitOfWork`.<br>6. The system returns the record with its child collections and the SPA binds them to the form fields. |
| Alternate flows | • An HQ role (General Director, Financial Director, Staff) may pass an explicit charity id and so read across the charity boundary; a charity user may not and always sees its own data. |
| Exception flows | • The session has expired or the role is not permitted — the request is rejected and the SPA routes back to the login state.<br>• The business layer returns «Unthorized User» and the operation is not applied. |
| Post-conditions | • No stored data is changed — the operation is a read. |
| Realisation | Route `#/families/orphans/coding/worklist` → `OrphanCodingWorklistComponent`<br>`GET /api/Families/orphans?codingStatus=Pending` → `FamiliesController` → `IFamilyService` |

#### 13.U.4  UC-ORP-04 — Search orphans for coding البحث في قائمة التكويد


| Item | Specification |
| --- | --- |
| Use case ID | UC-ORP-04 |
| Name | Search orphans for coding البحث في قائمة التكويد |
| Type | Search / filter |
| Primary actor | Gen. Director, Staff |
| Secondary actors | The system (Web API + business layer); the database. |
| Summary | Legacy coding screen: locates candidate orphans by name within a charity before assigning a code. |
| Trigger | The actor presses «بحث» on the screen OrphanCoding. |
| Pre-conditions | 1. The actor is authenticated; the JWT access token is valid and the client holds the role, charity and country claims it carries.<br>2. The actor holds one of: Gen. Director, Staff.<br>3. Caller identity comes from the JWT, never from the request; the application service scopes the query to the charity (and country) that owns the user, unless the role is an HQ role permitted to pass an explicit `charityId` filter.<br>4. The SPA route `#/families/orphans/coding` has loaded and its reference-data lookups have been populated. |
| Main flow | 1. The actor navigates to the screen at `#/families/orphans/coding`.<br>2. The actor enters the search criteria in the filter fields.<br>3. The actor presses «بحث» (Search()).<br>4. The SPA issues `GET /api/Families/orphans?search=` carrying name, charityId, userId.<br>5. `FamiliesController` binds the typed request DTO and delegates to the application service.<br>6. `IOrphanService` validates the payload with its FluentValidation validator, applies the business rules and the charity scope, and persists through `IUnitOfWork`.<br>7. The matching rows are returned, scoped to the caller’s charity, and rendered in the result grid. |
| Alternate flows | • An HQ role (General Director, Financial Director, Staff) may pass an explicit charity id and so read across the charity boundary; a charity user may not and always sees its own data.<br>• No row matches the criteria — the grid renders empty and the paging control reports zero pages. |
| Exception flows | • The session has expired or the role is not permitted — the request is rejected and the SPA routes back to the login state.<br>• The business layer returns «Unthorized User» and the operation is not applied. |
| Post-conditions | • No stored data is changed — the operation is a read. |
| Realisation | Route `#/families/orphans/coding` → `OrphanCodingComponent`<br>`GET /api/Families/orphans?search=` → `FamiliesController` → `IFamilyService` |

#### 13.U.5  UC-ORP-05 — Verify a code is not already used التحقق من تفرد الكود


| Item | Specification |
| --- | --- |
| Use case ID | UC-ORP-05 |
| Name | Verify a code is not already used التحقق من تفرد الكود |
| Type | Read a record |
| Primary actor | Gen. Director, Staff |
| Secondary actors | The system (Web API + business layer); the database. |
| Summary | Validates a proposed sponsorship code against the charity's existing codes and blocks submission on a clash. |
| Trigger | The actor selects the function from the module menu or the screen that hosts it. |
| Pre-conditions | 1. The actor is authenticated; the JWT access token is valid and the client holds the role, charity and country claims it carries.<br>2. The actor holds one of: Gen. Director, Staff.<br>3. Caller identity comes from the JWT, never from the request; the application service scopes the query to the charity (and country) that owns the user, unless the role is an HQ role permitted to pass an explicit `charityId` filter.<br>4. The target record exists and its identifier is known to the screen. |
| Main flow | 1. The actor reaches the function from the screen of this module that hosts it.<br>2. The actor selects the record to open.<br>3. The SPA issues `GET /api/Families/orphans/check-code` carrying id, code, userId.<br>4. `FamiliesController` binds the typed request DTO and delegates to the application service.<br>5. `IOrphanService` validates the payload with its FluentValidation validator, applies the business rules and the charity scope, and persists through `IUnitOfWork`.<br>6. The system returns the record with its child collections and the SPA binds them to the form fields. |
| Alternate flows | • An HQ role (General Director, Financial Director, Staff) may pass an explicit charity id and so read across the charity boundary; a charity user may not and always sees its own data. |
| Exception flows | • The session has expired or the role is not permitted — the request is rejected and the SPA routes back to the login state. |
| Post-conditions | • No stored data is changed — the operation is a read. |
| Realisation | `GET /api/Families/orphans/check-code` → `FamiliesController` → `IFamilyService` |

#### 13.U.6  UC-ORP-06 — Assign a sponsorship code to an orphan إسناد كود لليتيم


| Item | Specification |
| --- | --- |
| Use case ID | UC-ORP-06 |
| Name | Assign a sponsorship code to an orphan إسناد كود لليتيم |
| Type | Create a record |
| Primary actor | Gen. Director, Staff |
| Secondary actors | The system (Web API + business layer); the database. |
| Summary | Writes the approved code onto the orphan record. From this point the orphan is admitted to the sponsorship programme and becomes eligible for periodic reports and payment enrolment. |
| Trigger | The actor selects the function from the module menu or the screen that hosts it. |
| Pre-conditions | 1. The actor is authenticated; the JWT access token is valid and the client holds the role, charity and country claims it carries.<br>2. The actor holds one of: Gen. Director, Staff.<br>3. Caller identity comes from the JWT, never from the request; the application service scopes the query to the charity (and country) that owns the user, unless the role is an HQ role permitted to pass an explicit `charityId` filter. |
| Main flow | 1. The actor reaches the function from the screen of this module that hosts it. The screen opens in add mode.<br>2. The system loads the reference data the form needs (regions, centres, banks, types and the other lookup lists bound to the drop-downs).<br>3. The actor completes the input fields of the form.<br>4. The actor presses the command button of the screen.<br>5. The SPA issues `POST /api/Families/orphans/{orphanId}/code` carrying id, userId, code.<br>6. `FamiliesController` binds the typed request DTO and delegates to the application service.<br>7. `IOrphanService` validates the payload with its FluentValidation validator, applies the business rules and the charity scope, and persists through `IUnitOfWork`.<br>8. The business layer validates the payload, stamps the owning charity and the creating user, and persists the new record.<br>9. The system returns the outcome and the SPA confirms the save and returns to the list screen. |
| Alternate flows | • An HQ role (General Director, Financial Director, Staff) may pass an explicit charity id and so read across the charity boundary; a charity user may not and always sees its own data.<br>• The actor abandons the form before saving — nothing is written and the record keeps its previous state. |
| Exception flows | • The session has expired or the role is not permitted — the request is rejected and the SPA routes back to the login state.<br>• A mandatory field is empty or fails its format check — the save is refused and the field is flagged on the form. |
| Post-conditions | • A new record exists, owned by the charity of the creating user, and appears in the list screen of the module. |
| Realisation | `POST /api/Families/orphans/{orphanId}/code` → `FamiliesController` → `IFamilyService` |

#### 13.U.7  UC-ORP-07 — Resolve an orphan's name from a code استعلام بالكود


| Item | Specification |
| --- | --- |
| Use case ID | UC-ORP-07 |
| Name | Resolve an orphan's name from a code استعلام بالكود |
| Type | Read a record |
| Primary actor | All roles |
| Secondary actors | The system (Web API + business layer); the database. |
| Summary | Returns the orphan's name and code pair for a given identifier, used to confirm the operator is working on the right beneficiary. |
| Trigger | The actor selects the function from the module menu or the screen that hosts it. |
| Pre-conditions | 1. The actor is authenticated; the JWT access token is valid and the client holds the role, charity and country claims it carries.<br>2. The actor holds one of: All roles.<br>3. Caller identity comes from the JWT, never from the request; the application service scopes the query to the charity (and country) that owns the user, unless the role is an HQ role permitted to pass an explicit `charityId` filter.<br>4. The target record exists and its identifier is known to the screen. |
| Main flow | 1. The actor reaches the function from the screen of this module that hosts it.<br>2. The actor selects the record to open.<br>3. The SPA issues `GET /api/Families/orphans?search=` carrying id, userId.<br>4. `FamiliesController` binds the typed request DTO and delegates to the application service.<br>5. `IOrphanService` validates the payload with its FluentValidation validator, applies the business rules and the charity scope, and persists through `IUnitOfWork`.<br>6. The system returns the record with its child collections and the SPA binds them to the form fields. |
| Alternate flows | • An HQ role (General Director, Financial Director, Staff) may pass an explicit charity id and so read across the charity boundary; a charity user may not and always sees its own data. |
| Exception flows | • The session has expired or the role is not permitted — the request is rejected and the SPA routes back to the login state. |
| Post-conditions | • No stored data is changed — the operation is a read. |
| Realisation | `GET /api/Families/orphans?search=` → `FamiliesController` → `IFamilyService` |

#### 13.U.8  UC-ORP-08 — View an orphan's payment history دفعات اليتيم


| Item | Specification |
| --- | --- |
| Use case ID | UC-ORP-08 |
| Name | View an orphan's payment history دفعات اليتيم |
| Type | Read a record |
| Primary actor | Charity, HQ roles |
| Secondary actors | The system (Web API + business layer); the database. |
| Summary | Lists every payment batch in which the orphan appeared, most recent first, with the amount, batch date and whether it was stopped or received — the audit trail for beneficiary enquiries. |
| Trigger | The actor selects the function from the module menu or the screen that hosts it. |
| Pre-conditions | 1. The actor is authenticated; the JWT access token is valid and the client holds the role, charity and country claims it carries.<br>2. The actor holds one of: charity, HQ roles.<br>3. Caller identity comes from the JWT, never from the request; the application service scopes the query to the charity (and country) that owns the user, unless the role is an HQ role permitted to pass an explicit `charityId` filter.<br>4. The target record exists and its identifier is known to the screen. |
| Main flow | 1. The actor reaches the function from the screen of this module that hosts it.<br>2. The actor selects the record to open.<br>3. The SPA issues `GET /api/OrphanPayments?orphanId=` carrying id, userId.<br>4. `ReportsController` binds the typed request DTO and delegates to the application service.<br>5. `IOrphanPaymentService` validates the payload with its FluentValidation validator, applies the business rules and the charity scope, and persists through `IUnitOfWork`.<br>6. The system returns the record with its child collections and the SPA binds them to the form fields. |
| Alternate flows | • An HQ role (General Director, Financial Director, Staff) may pass an explicit charity id and so read across the charity boundary; a charity user may not and always sees its own data. |
| Exception flows | • The session has expired or the role is not permitted — the request is rejected and the SPA routes back to the login state. |
| Post-conditions | • No stored data is changed — the operation is a read. |
| Realisation | `GET /api/OrphanPayments?orphanId=` → `OrphanPaymentsController` → `IOrphanPaymentService` |

#### 13.U.9  UC-ORP-09 — View an orphan's payment details تفاصيل دفعة اليتيم


| Item | Specification |
| --- | --- |
| Use case ID | UC-ORP-09 |
| Name | View an orphan's payment details تفاصيل دفعة اليتيم |
| Type | Read a record |
| Primary actor | Charity, HQ roles |
| Secondary actors | The system (Web API + business layer); the database. |
| Summary | Retrieves the payment rows attached to one orphan for a chosen batch, including cheque number and receipt state. |
| Trigger | The actor selects the function from the module menu or the screen that hosts it. |
| Pre-conditions | 1. The actor is authenticated; the JWT access token is valid and the client holds the role, charity and country claims it carries.<br>2. The actor holds one of: charity, HQ roles.<br>3. Caller identity comes from the JWT, never from the request; the application service scopes the query to the charity (and country) that owns the user, unless the role is an HQ role permitted to pass an explicit `charityId` filter.<br>4. The target record exists and its identifier is known to the screen. |
| Main flow | 1. The actor reaches the function from the screen of this module that hosts it.<br>2. The actor selects the record to open.<br>3. The SPA issues `GET /api/OrphanPayments` carrying id, userid.<br>4. `OrphanPaymentsController` binds the typed request DTO and delegates to the application service.<br>5. `IOrphanPaymentService` validates the payload with its FluentValidation validator, applies the business rules and the charity scope, and persists through `IUnitOfWork`.<br>6. The system returns the record with its child collections and the SPA binds them to the form fields. |
| Alternate flows | • An HQ role (General Director, Financial Director, Staff) may pass an explicit charity id and so read across the charity boundary; a charity user may not and always sees its own data. |
| Exception flows | • The session has expired or the role is not permitted — the request is rejected and the SPA routes back to the login state.<br>• The business layer returns «Faliure» and the operation is not applied. |
| Post-conditions | • No stored data is changed — the operation is a read. |
| Realisation | `GET /api/OrphanPayments` → `OrphanPaymentsController` → `IOrphanPaymentService` |

#### 13.U.10  UC-ORP-10 — Check a phone number is not duplicated التحقق من رقم الهاتف


| Item | Specification |
| --- | --- |
| Use case ID | UC-ORP-10 |
| Name | Check a phone number is not duplicated التحقق من رقم الهاتف |
| Type | Create a record |
| Primary actor | Charity |
| Secondary actors | The system (Web API + business layer); the database. |
| Summary | Validates that a contact number being entered for a beneficiary is not already registered elsewhere, per number type. |
| Trigger | The actor selects the function from the module menu or the screen that hosts it. |
| Pre-conditions | 1. The actor is authenticated; the JWT access token is valid and the client holds the role, charity and country claims it carries.<br>2. The actor holds one of: charity.<br>3. Caller identity comes from the JWT, never from the request; the application service scopes the query to the charity (and country) that owns the user, unless the role is an HQ role permitted to pass an explicit `charityId` filter. |
| Main flow | 1. The actor reaches the function from the screen of this module that hosts it. The screen opens in add mode.<br>2. The system loads the reference data the form needs (regions, centres, banks, types and the other lookup lists bound to the drop-downs).<br>3. The actor completes the input fields of the form.<br>4. The actor presses the command button of the screen.<br>5. The SPA issues `GET /api/Families/{familyId}/provider/check-phone` carrying type, number, userId.<br>6. `OrphanPaymentsController` binds the typed request DTO and delegates to the application service.<br>7. `IFamilyService` validates the payload with its FluentValidation validator, applies the business rules and the charity scope, and persists through `IUnitOfWork`.<br>8. The business layer validates the payload, stamps the owning charity and the creating user, and persists the new record.<br>9. The system returns the outcome and the SPA confirms the save and returns to the list screen. |
| Alternate flows | • An HQ role (General Director, Financial Director, Staff) may pass an explicit charity id and so read across the charity boundary; a charity user may not and always sees its own data.<br>• The actor abandons the form before saving — nothing is written and the record keeps its previous state. |
| Exception flows | • The session has expired or the role is not permitted — the request is rejected and the SPA routes back to the login state.<br>• A mandatory field is empty or fails its format check — the save is refused and the field is flagged on the form. |
| Post-conditions | • A new record exists, owned by the charity of the creating user, and appears in the list screen of the module. |
| Realisation | `GET /api/Families/{familyId}/provider/check-phone` → `FamiliesController` → `IFamilyService` |

#### 13.U.11  UC-ORP-11 — Load orphan reference data القوائم المرجعية لليتيم


| Item | Specification |
| --- | --- |
| Use case ID | UC-ORP-11 |
| Name | Load orphan reference data القوائم المرجعية لليتيم |
| Type | Browse a list |
| Primary actor | Charity |
| Secondary actors | The system (Web API + business layer); the database. |
| Summary | Loads the lookup lists that drive the orphan forms — educational level, stage and class, educational status, health status, disability type, gender, relationship, prayer and memorisation levels, hobbies and behaviour scales. |
| Trigger | The actor selects the function from the module menu or the screen that hosts it. |
| Pre-conditions | 1. The actor is authenticated; the JWT access token is valid and the client holds the role, charity and country claims it carries.<br>2. The actor holds one of: charity.<br>3. Caller identity comes from the JWT, never from the request; the application service scopes the query to the charity (and country) that owns the user, unless the role is an HQ role permitted to pass an explicit `charityId` filter. |
| Main flow | 1. The actor reaches the function from the screen of this module that hosts it.<br>2. The system loads the filter lookups the screen offers and, for HQ roles, the charity selector.<br>3. The SPA issues `GET /api/OrphanPayments` carrying id, userId.<br>4. `OrphanPaymentsController` binds the typed request DTO and delegates to the application service.<br>5. `IOrphanPaymentService` validates the payload with its FluentValidation validator, applies the business rules and the charity scope, and persists through `IUnitOfWork`.<br>6. The result set is scoped to the caller’s charity and country, ordered and paged.<br>7. The SPA renders the page and the paging control. |
| Alternate flows | • An HQ role (General Director, Financial Director, Staff) may pass an explicit charity id and so read across the charity boundary; a charity user may not and always sees its own data.<br>• No row matches the criteria — the grid renders empty and the paging control reports zero pages. |
| Exception flows | • The session has expired or the role is not permitted — the request is rejected and the SPA routes back to the login state.<br>• The business layer returns «Faliure» and the operation is not applied. |
| Post-conditions | • No stored data is changed — the operation is a read. |
| Realisation | `GET /api/OrphanPayments` · `GET /api/LookupManagement` → `OrphanPaymentsController` → `IOrphanPaymentService` |

### 13.A  Annex - Angular routes of this module

Routes are hash-based (`useHash: true`), rendered inside `MainLayoutComponent` behind
`AuthGuard` and `PermissionGuard`. *implemented* means the route exists in
`Frontend/src/app`; *planned* means it is specified here and not yet built.

| Angular route | Feature module | Component | Status |
| --- | --- | --- | --- |
| `#/families/orphans/coding` | `families` | `OrphanCodingComponent` | planned |
| `#/families/orphans/coding/worklist` | `families` | `OrphanCodingWorklistComponent` | planned |

### 13.B  Annex - API controllers of this module

Every controller inherits `ApiController` (`[Authorize]`, route `api/[controller]`) and
returns `Framework.Core.ApiResponse<T>`. Business rules live in the application service
behind each controller, never in the controller itself.

| Controller | Base route | Responsibility |
| --- | --- | --- |
| `FamiliesController` | `api/Families` | Add check, coding search, code assignment, member relocation, payment categories. Name autocomplete for orphans and housing beneficiaries. Name/code lookup, code uniqueness. Uncoded orphan worklist. |
| `ReportsController` | `api/Reports` | Payment history per orphan; missed payments. |
| `OrphanPaymentsController` | `api/OrphanPayments` | Payment detail per orphan. Reference lists; phone duplicate check. |

---

Shared context - system architecture, actors and roles, the sponsorship lifecycle, the full module map and all appendices - is in the companion document [00-Overview-and-Common-Context](00-Overview-and-Common-Context.md). The delivery backlog for this module is epic EP-08 in [00-EPICS-and-User-Stories](00-EPICS-and-User-Stories.md).

