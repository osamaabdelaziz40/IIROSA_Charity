# WAR.IIROSA - Correspondence — Incoming & Outgoing

الصادر والوارد | use case prefix `UC-COR` | chapter 21 of the master document


| Field | Value |
| --- | --- |
| Document title | WAR.IIROSA - Module Specification: Correspondence — Incoming & Outgoing |
| Module | Correspondence — Incoming & Outgoing - الصادر والوارد |
| Use case prefix | UC-COR |
| Chapter in master document | Chapter 21 |
| Documented use cases | 19 |
| Principal routes | `#/incoming-outgoing/incoming`, `/incoming/create`, `/incoming/:id`, `/incoming/:id/edit`, `/outgoing`, `/outgoing/create`, `/outgoing/:id`, `/outgoing/:id/edit`, `/import/:type`, `/export/:type`, `/history` |
| Version | 1.1 |
| Status | Chapter content extracted verbatim; screen fields and scenarios derived from the source code |
| Date | 18 August 2026 |
| Parent document | WAR.IIROSA-Project-Documentation.docx v1.0 (repository WAR.IIROSA, commit 4beb736) |

## Contents of this document

1. Chapter 21 — module purpose and use-case catalogue (verbatim from the master document)
2. §21.D — detailed specifications carried over from chapter 25
3. §21.S — screen field specifications (every field of every screen, derived from the AngularJS views)
4. §21.U — one expanded scenario per use case (trigger, pre-conditions, main flow, alternates, exceptions, post-conditions)
5. §21.A / §21.B — annexes: screens and Web API controllers of this module

## 21. Correspondence — Incoming & Outgoing

الصادر والوارد — the official letter register. Its distinguishing feature is that orphan periodic reports are attached to outgoing letters, so that every batch of reports sent to a charity or authority is traceable to a numbered dispatch.

### 21.1 Incoming correspondence


| ID | Use case | Primary actor | Description & main flow | Realisation |
| --- | --- | --- | --- | --- |
| UC-COR-01 | List incoming letters الوارد | Staff, Gen. Director | Paged register of received letters with serial, date, sender, subject and status. | Route `#/incoming-outgoing/incoming` → GET /api/IncomingOutgoing/incoming |
| UC-COR-02 | Search incoming letters البحث في الوارد | Staff, Gen. Director | Filters the register by status, date, description, letter number, serial and date range, with paging optionally disabled for export. | GET /api/IncomingOutgoing/incoming |
| UC-COR-03 | Obtain the next incoming serial رقم الوارد التالي | Staff | Reserves the next sequential registration number so the letter is filed in order. | GET /api/IncomingOutgoing/incoming/next-serial |
| UC-COR-04 | Register an incoming letter تسجيل وارد | Staff | Records the received letter — serial, date, sender, receiving department, subject, attachments and routing. | Route `#/incoming-outgoing/incoming/create` → POST /api/IncomingOutgoing/incoming |
| UC-COR-05 | View an incoming letter عرض الوارد | Staff, Gen. Director | Loads one incoming letter with its routing history and attachments. | GET /api/IncomingOutgoing/incoming/{id} |
| UC-COR-06 | Update an incoming letter تعديل الوارد | Staff | Amends the registration data or the routing of a received letter. | PUT /api/IncomingOutgoing/incoming |
| UC-COR-07 | Delete an incoming letter حذف الوارد | Staff, Gen. Director | Removes a registration made in error. | DELETE /api/IncomingOutgoing/incoming |
| UC-COR-08 | Select the routing department الإدارة المختصة | Staff | Loads the head-office department catalogue used to route both incoming and outgoing letters. | GET /api/LookupManagement/departments |
| UC-COR-09 | Attach employees to an incoming letter ربط الموظفين بخطاب وارد | Staff | Lists the employees already attached to an incoming letter and those still unattached for the chosen charity and letter, and saves the selection — used to record who a received letter concerns. | Route `#/incoming-outgoing/export/incoming` → GET /api/IncomingOutgoing/incoming, GetNonRegistered, POST /api/IncomingOutgoing/incoming, GET /api/IncomingOutgoing/incoming/statuses |

### 21.2 Outgoing correspondence


| ID | Use case | Primary actor | Description & main flow | Realisation |
| --- | --- | --- | --- | --- |
| UC-COR-10 | List outgoing letters الصادر | Staff, Gen. Director | Paged register of dispatched letters with serial, date, recipient and subject. | Route `#/incoming-outgoing/outgoing` → GET /api/IncomingOutgoing/outgoing |
| UC-COR-11 | Search outgoing letters البحث في الصادر | Staff, Gen. Director | Filters by description, letter number and date range, with paging optionally disabled; an unpaged variant supports export. | GET /api/IncomingOutgoing/outgoing, GET /api/IncomingOutgoing/export/outgoing |
| UC-COR-12 | Obtain the next outgoing serial رقم الصادر التالي | Staff | Reserves the next sequential dispatch number. | GET /api/IncomingOutgoing/outgoing/next-serial |
| UC-COR-13 | Register an outgoing letter تسجيل صادر | Staff | Records the dispatch — serial, date, recipient, originating department, category, subject and attachments. | Route `#/incoming-outgoing/outgoing/create` → POST /api/IncomingOutgoing/outgoing |
| UC-COR-14 | View an outgoing letter عرض الصادر | Staff, Gen. Director | Loads one dispatched letter with its attachments and the orphan reports linked to it. | GET /api/IncomingOutgoing/outgoing/{id} |
| UC-COR-15 | Update an outgoing letter تعديل الصادر | Staff | Amends the dispatch data before or after sending. | PUT /api/IncomingOutgoing/outgoing |
| UC-COR-16 | Delete an outgoing letter حذف الصادر | Staff, Gen. Director | Removes a dispatch registered in error. | DELETE /api/IncomingOutgoing/outgoing |
| UC-COR-17 | Select the outgoing category تصنيف الصادر | Staff | Loads the dispatch-category catalogue used to classify outgoing letters. | GET /api/IncomingOutgoing/outgoing/categories |
| UC-COR-18 | Attach orphan reports to an outgoing letter إضافة تقارير الأيتام إلى خطاب صادر | Staff | For a chosen charity and outgoing letter, the system lists the orphan reports already attached and those still unattached; the operator selects the reports to include and saves, creating the auditable link between the dispatch and the reports it carried. | Route `#/incoming-outgoing/export/outgoing` → GET /api/IncomingOutgoing/outgoing/{parentOutgoingId}/children, GetNonRegistered, POST /api/IncomingOutgoing/outgoing/{parentOutgoingId}/child, GET /api/IncomingOutgoing/outgoing/categories |
| UC-COR-19 | Report orphans by outgoing letter تقرير الأيتام حسب الخطاب الصادر | Staff, Gen. Director | Given a dispatch serial and year (optionally a charity, date range or orphan code), returns the orphans whose reports were sent under that letter — the answer to "which reports did we send, and when?". A companion report lists missing attachments. | Route `#/incoming-outgoing/export/outgoing-orphans` → GET /api/IncomingOutgoing/outgoing?orphanNumber=; rptChildOutGoingMissing.rpt |

### 21.D  Detailed use case specifications (from chapter 25)

Reproduced verbatim from chapter 25 of the master document — the fully expanded specification of this module’s critical end-to-end scenarios. Every other use case of the module is specified in §21.U.

**25.10 UC-COR-18 — Attach orphan reports to an outgoing letter**


| Use case ID | UC-COR-18 |
| --- | --- |
| Name | Attach orphan reports to an outgoing letter — إضافة تقارير الأيتام إلى خطاب صادر |
| Primary actor | Staff |
| Goal | Make every dispatch of periodic reports traceable to a numbered outgoing letter. |
| Pre-conditions | The outgoing letter is registered (UC-COR-13); the reports to be sent exist and are accepted. |
| Main flow | 1. The user opens the OutgoingsReport state and selects the charity and the outgoing letter. 2. The system lists the reports already attached to that letter and, separately, those not yet attached. 3. The user selects the reports to include in the dispatch. 4. The user saves; the client posts to POST /api/IncomingOutgoing/outgoing/{parentOutgoingId}/child and the links are created. 5. The dispatch can later be reconstructed by serial and year through the outgoing orphan report (UC-COR-19), and any expected-but-missing attachment is listed by the missing-attachments report (UC-RPT-38). |
| Alternate flows | A1 — Incoming direction. The equivalent flow attaches employees to an incoming letter (UC-COR-09). A2 — Report removed from the dispatch. The user de-selects it and saves again. |
| Post-conditions | Each attached report carries the outgoing letter under which it was sent, giving a complete correspondence audit trail. |
| Business rules | BR-26 A report is attached to at most one outgoing letter at a time. BR-27 Attachment is scoped to one charity per letter selection. |


### 21.S  Screen field specifications

Derived from the AngularJS views of this module. For every screen the table lists each data-entry field in the order it appears, the scope property it binds to, its control type, whether the form marks it mandatory, and the lookup or rule that governs it. Labels are reproduced as they appear on screen (Arabic, right-to-left).

#### 21.S.1  Screen `#/incoming-outgoing/incoming`


| Property | Value |
| --- | --- |
| Angular route | `#/incoming-outgoing/incoming` |
| Feature module | `incoming-outgoing` (lazy-loaded) |
| Component | `IncomingLettersListComponent` |
| Route status | implemented |
| Data-entry fields | 9 |
| Grids on the screen | 1 |
| Commands | 8 |


| Section | Field (as labelled) | Bound to | Control | Mandatory · source · rules |
| --- | --- | --- | --- | --- |
| — | الجمعية | CharityId | Drop-down list | Optional · options: lookup: Charities (+ كافة الجهات) · on change: getCharityData() |
| — | الحاله | Status | Drop-down list | Optional · options: معلق / تم الرد / تم عمل اللازم |
| — | * الموظف | UserId | Drop-down list | Optional · options: lookup: allEmployees |
| — | رقم الوارد | IncomSerial | Drop-down list | Optional · options: lookup: IncomingsDataForFilter |
| — | * رقم الخطاب | LetterNumber | Drop-down list | Optional · options: lookup: IncomingsDataForFilter |
| — | التاريخ | IncomDate | Drop-down list | Optional · options: lookup: IncomingsDataForFilter |
| — | * الموضوع | IncomingDescription | Text box | Optional |
| — | الي تاريخ | DateTo | Date picker | Optional |
| — | من تاريخ | DateFrom | Date picker | Optional |

Grids on this screen:

| Grid (row source) | Columns |
| --- | --- |
| incoming in Incomings   track by $index | كود الوارد · العام الهجري · الادارة · التاريخ · رقم الخطاب الوارد · البيان · الملف · الاجراءات |

Commands on this screen:

| Command | Handler | Shown when |
| --- | --- | --- |
| بحث | GetData() | always |
| حفظ | DeleteIncoming() | always |
| (icon only) | ExtractIncomingData() | always |
| (icon only) | ExtractAllIncomingData() | always |
| (icon only) | EditIncoming(incoming.Id) | always |
| (icon only) | ViewDeleteModel(incoming.Id) | always |
| (icon only) | GetNext() | always |
| (icon only) | GetPrev() | always |

#### 21.S.2  Screen `#/incoming-outgoing/incoming/:id/edit`


| Property | Value |
| --- | --- |
| Angular route | `#/incoming-outgoing/incoming/:id/edit` |
| Feature module | `incoming-outgoing` (lazy-loaded) |
| Component | `IncomingLetterFormComponent` |
| Route status | implemented |
| Data-entry fields | 11 |
| Grids on the screen | 0 |
| Commands | 2 |


| Section | Field (as labelled) | Bound to | Control | Mandatory · source · rules |
| --- | --- | --- | --- | --- |
| إضافة وارد | التاريخ | Incoming.Date | Text box | Mandatory |
| إضافة وارد | كود الوارد | Incoming.Serial_Txt | Text box | Mandatory · read-only |
| إضافة وارد | رقم الخطاب | Incoming.LetterNumber | Text box | Mandatory |
| إضافة وارد | تاريخ الخطاب | Incoming.LetterDate | Text box | Mandatory |
| إضافة وارد | الادارة او الجهة الراسله للخطاب | Incoming.Fk_DepartmentId | Drop-down list | Mandatory · options: lookup: Departsments |
| إضافة وارد | البيان | Incoming.Subject | Text box | Mandatory |
| إضافة وارد | الحاله | Incoming.Status | Drop-down list | Mandatory · options: معلق / تم الرد / تم عمل اللازم · on change: SetLetterStatus() |
| إضافة وارد | الموظف المسئول | Incoming.UserId | Drop-down list | Mandatory · options: lookup: allEmployees |
| إضافة وارد | الخطاب الصادر | Incoming.OutgoingId | Drop-down list | Mandatory · options: lookup: Outgoings |
| إضافة وارد | تفاصيل | Incoming.LetterDescription | Multi-line text | Mandatory |
| إضافة وارد | الملف | Incoming.UploadedFile | File upload | Optional · accepts application/pdf |

Commands on this screen:

| Command | Handler | Shown when |
| --- | --- | --- |
| (icon only) | RemoveImage() | Incoming.UploadedFile!=null |
| حفظ | SubmitIncoming() | always |

#### 21.S.3  Screen `#/incoming-outgoing/export/incoming`


| Property | Value |
| --- | --- |
| Angular route | `#/incoming-outgoing/export/incoming` |
| Feature module | `incoming-outgoing` (lazy-loaded) |
| Component | `ExportWizardComponent` |
| Route status | implemented |
| Data-entry fields | 2 |
| Grids on the screen | 2 |
| Commands | 3 |


| Section | Field (as labelled) | Bound to | Control | Mandatory · source · rules |
| --- | --- | --- | --- | --- |
| — | خطاب الوارد | IncomingId | Drop-down list | Mandatory · options: lookup: Incomings · on change: GetIncomingOrphan() |
| — | الجمعية | CharityId | Drop-down list | Optional · options: lookup: Charities (+ كافة الجهات) · on change: getCharityData() |

Grids on this screen:

| Grid (row source) | Columns |
| --- | --- |
| orphan in allOrphans track by $index | الرقم · إسم الموظف · اضافة الى الخطاب |
| orphan in OrphanIncoming  track by $index | الرقم · إسم الموظف · حذف |

Commands on this screen:

| Command | Handler | Shown when |
| --- | --- | --- |
| حفظ | UpdateData() | always |
| (icon only) | AppendToIncoming(orphan.Id) | always |
| (icon only) | RemoveFromIncoming(orphan.Id) | always |

#### 21.S.4  Screen `#/incoming-outgoing/outgoing`


| Property | Value |
| --- | --- |
| Angular route | `#/incoming-outgoing/outgoing` |
| Feature module | `incoming-outgoing` (lazy-loaded) |
| Component | `OutgoingLettersListComponent` |
| Route status | implemented |
| Data-entry fields | 7 |
| Grids on the screen | 1 |
| Commands | 8 |


| Section | Field (as labelled) | Bound to | Control | Mandatory · source · rules |
| --- | --- | --- | --- | --- |
| — | الجمعية | CharityId | Drop-down list | Optional · options: lookup: Charities (+ كافة الجهات) · on change: getCharityData() |
| — | الحاله | Status | Drop-down list | Optional · options: معلق / تم الرد / تم عمل اللازم |
| — | * الموظف | UserId | Drop-down list | Optional · options: lookup: allEmployees |
| — | * رقم الخطاب | LetterNumber | Numeric box | Optional |
| — | * الموضوع | OutgoingDescription | Text box | Optional |
| — | الي تاريخ | DateTo | Date picker | Optional |
| — | من تاريخ | DateFrom | Date picker | Optional |

Grids on this screen:

| Grid (row source) | Columns |
| --- | --- |
| outgoing in OutGoings   track by $index | رقم الصادر · العام الهجري · الادارة · التاريخ · البيان · الملف · الاجراءات |

Commands on this screen:

| Command | Handler | Shown when |
| --- | --- | --- |
| بحث | GetData() | always |
| حفظ | DeleteOutgoing() | always |
| (icon only) | ExtractOutgoingData() | always |
| (icon only) | ExtractAllOutgoingData() | always |
| (icon only) | EditOutgoing(outgoing.Id) | always |
| (icon only) | ViewDeleteModel(outgoing.Id) | always |
| (icon only) | GetNext() | always |
| (icon only) | GetPrev() | always |

#### 21.S.5  Screen `#/incoming-outgoing/outgoing/:id/edit`


| Property | Value |
| --- | --- |
| Angular route | `#/incoming-outgoing/outgoing/:id/edit` |
| Feature module | `incoming-outgoing` (lazy-loaded) |
| Component | `OutgoingLetterFormComponent` |
| Route status | implemented |
| Data-entry fields | 7 |
| Grids on the screen | 0 |
| Commands | 2 |


| Section | Field (as labelled) | Bound to | Control | Mandatory · source · rules |
| --- | --- | --- | --- | --- |
| إضافة صادر | رقم الصادر | OutGoing.Serial | Text box | Mandatory · read-only |
| إضافة صادر | الادارة او الجهة المرسل لها الخطاب | OutGoing.Fk_DepartmentId | Drop-down list | Mandatory · options: lookup: Departsments |
| إضافة صادر | التاريخ | OutGoing.Date | Text box | Mandatory |
| إضافة صادر | البيان | OutGoing.Subject | Text box | Mandatory |
| إضافة صادر | تصنيف موضوع الصادر | OutGoing.OutgoingCategoryId | Drop-down list | Mandatory · options: lookup: OutgoingCategories |
| إضافة صادر | ردا علي خطاب | OutGoing.IncomingId | Drop-down list | Mandatory · options: lookup: Incomings (+ -) · on change: DisplayIncominData() |
| إضافة صادر | الملف | OutGoing.UploadedFile | File upload | Optional · accepts application/pdf |

Commands on this screen:

| Command | Handler | Shown when |
| --- | --- | --- |
| (icon only) | RemoveImage() | OutGoing.UploadedFile!=null |
| حفظ | SubmitOutGoing() | always |

#### 21.S.6  Screen `#/incoming-outgoing/export/outgoing`


| Property | Value |
| --- | --- |
| Angular route | `#/incoming-outgoing/export/outgoing` |
| Feature module | `incoming-outgoing` (lazy-loaded) |
| Component | `ExportWizardComponent` |
| Route status | implemented |
| Data-entry fields | 2 |
| Grids on the screen | 2 |
| Commands | 3 |


| Section | Field (as labelled) | Bound to | Control | Mandatory · source · rules |
| --- | --- | --- | --- | --- |
| — | خطاب الصادر | OutgoingId | Drop-down list | Mandatory · options: lookup: Outgoings · on change: GetOutgoingOrphan() |
| — | الجمعية | CharityId | Drop-down list | Optional · options: lookup: Charities (+ كافة الجهات) · on change: getCharityData() |

Grids on this screen:

| Grid (row source) | Columns |
| --- | --- |
| orphan in allOrphans track by $index | الرقم · إسم اليتيم · كود اليتيم · اسم المعيل · صلة القرابة · اضافة الى الخطاب |
| orphan in OrphanInOutGoing  track by $index | الرقم · إسم اليتيم · كود اليتيم · اسم المعيل · صلة القرابة · حذف |

Commands on this screen:

| Command | Handler | Shown when |
| --- | --- | --- |
| حفظ | UpdateData() | always |
| (icon only) | AppendToOutgoing(orphan.Id) | always |
| (icon only) | RemoveFromOutgoing(orphan.Id) | always |

#### 21.S.7  Screen `#/incoming-outgoing/export/outgoing-orphans`


| Property | Value |
| --- | --- |
| Angular route | `#/incoming-outgoing/export/outgoing-orphans` |
| Feature module | `incoming-outgoing` (lazy-loaded) |
| Component | `ExportWizardComponent` |
| Route status | planned |
| Data-entry fields | 6 |
| Grids on the screen | 1 |
| Commands | 5 |


| Section | Field (as labelled) | Bound to | Control | Mandatory · source · rules |
| --- | --- | --- | --- | --- |
| — | الجمعية | CharityId | Drop-down list | Optional · options: lookup: Charities (+ كافة الجهات) · on change: getCharityData() |
| — | الي تاريخ | DateTo | Date picker | Optional |
| — | من تاريخ | DateFrom | Date picker | Optional |
| — | * رقم الخطاب | OutgoingSerial | Drop-down list | Optional · options: lookup: AllOutGoingsFor_Serial |
| — | * سنه الخطاب | OutgoingYear | Drop-down list | Optional · options: lookup: AllOutGoingsFor_Year |
| — | (unlabelled) | ChildCode | Text box | Mandatory |

Grids on this screen:

| Grid (row source) | Columns |
| --- | --- |
| outgoing in OutGoingsReport   track by $index | رقم الصادر · سنه الصادر · تاريخ الخطاب · الجمعيه · عدد الايتام · اليتيم مضاف للتقرير |

Commands on this screen:

| Command | Handler | Shown when |
| --- | --- | --- |
| بحث | GetData() | always |
| حفظ | DeleteOutgoing() | always |
| (icon only) | ExtractOutgoingData() | always |
| (icon only) | GetNext() | always |
| (icon only) | GetPrev() | always |

### 21.U  Use case scenarios

One expanded scenario for every use case of this module. Pre-conditions, flows and post-conditions are derived from the screen that hosts the function, the Web API action that serves it and the business-layer method that executes it; quoted messages are the literal strings returned by the business layer.

#### 21.U.1  UC-COR-01 — List incoming letters الوارد


| Item | Specification |
| --- | --- |
| Use case ID | UC-COR-01 |
| Name | List incoming letters الوارد |
| Type | Browse a list |
| Primary actor | Staff, Gen. Director |
| Secondary actors | The system (Web API + business layer); the database. |
| Summary | Paged register of received letters with serial, date, sender, subject and status. |
| Trigger | The actor opens the screen at `#/incoming-outgoing/incoming` from the module menu. |
| Pre-conditions | 1. The actor is authenticated; the JWT access token is valid and the client holds the role, charity and country claims it carries.<br>2. The actor holds one of: Staff, Gen. Director.<br>3. Caller identity comes from the JWT, never from the request; the application service scopes the query to the charity (and country) that owns the user, unless the role is an HQ role permitted to pass an explicit `charityId` filter.<br>4. The SPA route `#/incoming-outgoing/incoming` has loaded and its reference-data lookups have been populated. |
| Main flow | 1. The actor navigates to the screen at `#/incoming-outgoing/incoming`.<br>2. The system loads the filter lookups the screen offers and, for HQ roles, the charity selector.<br>3. The SPA issues `GET /api/IncomingOutgoing/incoming` carrying userId, pagenum.<br>4. `IncomingOutgoingController` binds the typed request DTO and delegates to the application service.<br>5. `IIncomingService` validates the payload with its FluentValidation validator, applies the business rules and the charity scope, and persists through `IUnitOfWork`.<br>6. The result set is scoped to the caller’s charity and country, ordered and paged.<br>7. The SPA renders the page with the columns listed in the screen specification and the paging control. |
| Alternate flows | • An HQ role (General Director, Financial Director, Staff) may pass an explicit charity id and so read across the charity boundary; a charity user may not and always sees its own data.<br>• No row matches the criteria — the grid renders empty and the paging control reports zero pages. |
| Exception flows | • The session has expired or the role is not permitted — the request is rejected and the SPA routes back to the login state. |
| Post-conditions | • No stored data is changed — the operation is a read. |
| Realisation | Route `#/incoming-outgoing/incoming` → `IncomingLettersListComponent`<br>`GET /api/IncomingOutgoing/incoming` → `IncomingOutgoingController` → `IIncomingService` |

#### 21.U.2  UC-COR-02 — Search incoming letters البحث في الوارد


| Item | Specification |
| --- | --- |
| Use case ID | UC-COR-02 |
| Name | Search incoming letters البحث في الوارد |
| Type | Search / filter |
| Primary actor | Staff, Gen. Director |
| Secondary actors | The system (Web API + business layer); the database. |
| Summary | Filters the register by status, date, description, letter number, serial and date range, with paging optionally disabled for export. |
| Trigger | The actor selects the function from the module menu or the screen that hosts it. |
| Pre-conditions | 1. The actor is authenticated; the JWT access token is valid and the client holds the role, charity and country claims it carries.<br>2. The actor holds one of: Staff, Gen. Director. |
| Main flow | 1. The actor reaches the function from the screen of this module that hosts it.<br>2. The actor enters the search criteria in the filter fields.<br>3. The actor presses the command button of the screen.<br>4. The SPA issues `GET /api/IncomingOutgoing/incoming` carrying Status, IncomDate, IncomingDescription, LetterNumber, IncomSerial, DateFrom, DateTo, UsePagging.<br>5. `IncomingOutgoingController` binds the typed request DTO and delegates to the application service.<br>6. `IIncomingService` validates the payload with its FluentValidation validator, applies the business rules and the charity scope, and persists through `IUnitOfWork`.<br>7. The matching rows are returned, scoped to the caller’s charity, and rendered in the result grid. |
| Alternate flows | • No row matches the criteria — the grid renders empty and the paging control reports zero pages. |
| Exception flows | • The session has expired or the role is not permitted — the request is rejected and the SPA routes back to the login state. |
| Post-conditions | • No stored data is changed — the operation is a read. |
| Realisation | `GET /api/IncomingOutgoing/incoming` → `IncomingOutgoingController` → `IIncomingService` |

#### 21.U.3  UC-COR-03 — Obtain the next incoming serial رقم الوارد التالي


| Item | Specification |
| --- | --- |
| Use case ID | UC-COR-03 |
| Name | Obtain the next incoming serial رقم الوارد التالي |
| Type | Read a record |
| Primary actor | Staff |
| Secondary actors | The system (Web API + business layer); the database. |
| Summary | Reserves the next sequential registration number so the letter is filed in order. |
| Trigger | The actor selects the function from the module menu or the screen that hosts it. |
| Pre-conditions | 1. The actor is authenticated; the JWT access token is valid and the client holds the role, charity and country claims it carries.<br>2. The actor holds one of: Staff.<br>3. Caller identity comes from the JWT, never from the request; the application service scopes the query to the charity (and country) that owns the user, unless the role is an HQ role permitted to pass an explicit `charityId` filter.<br>4. The target record exists and its identifier is known to the screen. |
| Main flow | 1. The actor reaches the function from the screen of this module that hosts it.<br>2. The actor selects the record to open.<br>3. The SPA issues `GET /api/IncomingOutgoing/incoming/next-serial` carrying UserId.<br>4. `IncomingOutgoingController` binds the typed request DTO and delegates to the application service.<br>5. `IIncomingService` validates the payload with its FluentValidation validator, applies the business rules and the charity scope, and persists through `IUnitOfWork`.<br>6. The system returns the record with its child collections and the SPA binds them to the form fields. |
| Alternate flows | • An HQ role (General Director, Financial Director, Staff) may pass an explicit charity id and so read across the charity boundary; a charity user may not and always sees its own data. |
| Exception flows | • The session has expired or the role is not permitted — the request is rejected and the SPA routes back to the login state. |
| Post-conditions | • No stored data is changed — the operation is a read. |
| Realisation | `GET /api/IncomingOutgoing/incoming/next-serial` → `IncomingOutgoingController` → `IIncomingService` |

#### 21.U.4  UC-COR-04 — Register an incoming letter تسجيل وارد


| Item | Specification |
| --- | --- |
| Use case ID | UC-COR-04 |
| Name | Register an incoming letter تسجيل وارد |
| Type | Create a record |
| Primary actor | Staff |
| Secondary actors | The system (Web API + business layer); the database. |
| Summary | Records the received letter — serial, date, sender, receiving department, subject, attachments and routing. |
| Trigger | The actor presses «حفظ» on the screen Incoming. |
| Pre-conditions | 1. The actor is authenticated; the JWT access token is valid and the client holds the role, charity and country claims it carries.<br>2. The actor holds one of: Staff.<br>3. The SPA route `#/incoming-outgoing/incoming/:id/edit` has loaded and its reference-data lookups have been populated. |
| Main flow | 1. The actor navigates to the screen at `#/incoming-outgoing/incoming/:id/edit`. The screen opens in add mode.<br>2. The system loads the reference data the form needs (regions, centres, banks, types and the other lookup lists bound to the drop-downs).<br>3. The actor completes the input fields. Mandatory fields: التاريخ، كود الوارد، رقم الخطاب، تاريخ الخطاب، الادارة او الجهة الراسله للخطاب، البيان، الحاله، الموظف المسئول، الخطاب الصادر، تفاصيل.<br>4. The actor presses «حفظ» (SubmitIncoming()).<br>5. The SPA issues `POST /api/IncomingOutgoing/incoming` carrying typed request DTO obj.<br>6. `IncomingOutgoingController` binds the typed request DTO and delegates to the application service.<br>7. `IIncomingService` validates the payload with its FluentValidation validator, applies the business rules and the charity scope, and persists through `IUnitOfWork`.<br>8. The business layer validates the payload, stamps the owning charity and the creating user, and persists the new record.<br>9. The system returns the outcome and the SPA confirms the save and returns to the list screen. |
| Alternate flows | • The actor abandons the form before saving — nothing is written and the record keeps its previous state. |
| Exception flows | • The session has expired or the role is not permitted — the request is rejected and the SPA routes back to the login state.<br>• A mandatory field is empty or fails its format check — the save is refused and the field is flagged on the form. |
| Post-conditions | • A new record exists, owned by the charity of the creating user, and appears in the list screen of the module. |
| Realisation | Route `#/incoming-outgoing/incoming/create` → `IncomingLetterFormComponent`<br>`POST /api/IncomingOutgoing/incoming` → `IncomingOutgoingController` → `IIncomingService` |

#### 21.U.5  UC-COR-05 — View an incoming letter عرض الوارد


| Item | Specification |
| --- | --- |
| Use case ID | UC-COR-05 |
| Name | View an incoming letter عرض الوارد |
| Type | Read a record |
| Primary actor | Staff, Gen. Director |
| Secondary actors | The system (Web API + business layer); the database. |
| Summary | Loads one incoming letter with its routing history and attachments. |
| Trigger | The actor selects the function from the module menu or the screen that hosts it. |
| Pre-conditions | 1. The actor is authenticated; the JWT access token is valid and the client holds the role, charity and country claims it carries.<br>2. The actor holds one of: Staff, Gen. Director.<br>3. The target record exists and its identifier is known to the screen. |
| Main flow | 1. The actor reaches the function from the screen of this module that hosts it.<br>2. The actor selects the record to open.<br>3. The SPA issues `GET /api/IncomingOutgoing/incoming/{id}` carrying id.<br>4. `IncomingOutgoingController` binds the typed request DTO and delegates to the application service.<br>5. `IIncomingService` validates the payload with its FluentValidation validator, applies the business rules and the charity scope, and persists through `IUnitOfWork`.<br>6. The system returns the record with its child collections and the SPA binds them to the form fields. |
| Alternate flows | None recorded. |
| Exception flows | • The session has expired or the role is not permitted — the request is rejected and the SPA routes back to the login state. |
| Post-conditions | • No stored data is changed — the operation is a read. |
| Realisation | `GET /api/IncomingOutgoing/incoming/{id}` → `IncomingOutgoingController` → `IIncomingService` |

#### 21.U.6  UC-COR-06 — Update an incoming letter تعديل الوارد


| Item | Specification |
| --- | --- |
| Use case ID | UC-COR-06 |
| Name | Update an incoming letter تعديل الوارد |
| Type | Update a record |
| Primary actor | Staff |
| Secondary actors | The system (Web API + business layer); the database. |
| Summary | Amends the registration data or the routing of a received letter. |
| Trigger | The actor selects the function from the module menu or the screen that hosts it. |
| Pre-conditions | 1. The actor is authenticated; the JWT access token is valid and the client holds the role, charity and country claims it carries.<br>2. The actor holds one of: Staff.<br>3. The target record exists and its identifier is known to the screen. |
| Main flow | 1. The actor reaches the function from the screen of this module that hosts it. The screen opens in edit mode on the selected record.<br>2. The system loads the current values into the form.<br>3. The actor changes the fields to be corrected.<br>4. The actor presses the command button of the screen.<br>5. The SPA issues `PUT /api/IncomingOutgoing/incoming` carrying typed request DTO obj.<br>6. `IncomingOutgoingController` binds the typed request DTO and delegates to the application service.<br>7. `IIncomingService` validates the payload with its FluentValidation validator, applies the business rules and the charity scope, and persists through `IUnitOfWork`.<br>8. The business layer re-validates the payload, applies the changes and persists them.<br>9. The system returns the outcome and the SPA refreshes the screen. |
| Alternate flows | • The actor abandons the form before saving — nothing is written and the record keeps its previous state. |
| Exception flows | • The session has expired or the role is not permitted — the request is rejected and the SPA routes back to the login state.<br>• A mandatory field is empty or fails its format check — the save is refused and the field is flagged on the form. |
| Post-conditions | • The stored record carries the new values; no other record is affected. |
| Realisation | `PUT /api/IncomingOutgoing/incoming` → `IncomingOutgoingController` → `IIncomingService` |

#### 21.U.7  UC-COR-07 — Delete an incoming letter حذف الوارد


| Item | Specification |
| --- | --- |
| Use case ID | UC-COR-07 |
| Name | Delete an incoming letter حذف الوارد |
| Type | Delete a record |
| Primary actor | Staff, Gen. Director |
| Secondary actors | The system (Web API + business layer); the database. |
| Summary | Removes a registration made in error. |
| Trigger | The actor selects the function from the module menu or the screen that hosts it. |
| Pre-conditions | 1. The actor is authenticated; the JWT access token is valid and the client holds the role, charity and country claims it carries.<br>2. The actor holds one of: Staff, Gen. Director.<br>3. The target record exists and its identifier is known to the screen. |
| Main flow | 1. The actor reaches the function from the screen of this module that hosts it.<br>2. The actor selects the record and requests its deletion.<br>3. The SPA asks the actor to confirm.<br>4. The SPA issues `DELETE /api/IncomingOutgoing/incoming` carrying id.<br>5. `IncomingOutgoingController` binds the typed request DTO and delegates to the application service.<br>6. `IIncomingService` validates the payload with its FluentValidation validator, applies the business rules and the charity scope, and persists through `IUnitOfWork`.<br>7. The business layer checks that the record may still be removed and deletes it (or marks it removed).<br>8. The system returns the outcome and the SPA drops the row from the grid. |
| Alternate flows | None recorded. |
| Exception flows | • The session has expired or the role is not permitted — the request is rejected and the SPA routes back to the login state. |
| Post-conditions | • The record is no longer returned by the list and read endpoints of the module. |
| Realisation | `DELETE /api/IncomingOutgoing/incoming` → `IncomingOutgoingController` → `IIncomingService` |

#### 21.U.8  UC-COR-08 — Select the routing department الإدارة المختصة


| Item | Specification |
| --- | --- |
| Use case ID | UC-COR-08 |
| Name | Select the routing department الإدارة المختصة |
| Type | Read a record |
| Primary actor | Staff |
| Secondary actors | The system (Web API + business layer); the database. |
| Summary | Loads the head-office department catalogue used to route both incoming and outgoing letters. |
| Trigger | The actor selects the function from the module menu or the screen that hosts it. |
| Pre-conditions | 1. The actor is authenticated; the JWT access token is valid and the client holds the role, charity and country claims it carries.<br>2. The actor holds one of: Staff.<br>3. Caller identity comes from the JWT, never from the request; the application service scopes the query to the charity (and country) that owns the user, unless the role is an HQ role permitted to pass an explicit `charityId` filter.<br>4. The target record exists and its identifier is known to the screen. |
| Main flow | 1. The actor reaches the function from the screen of this module that hosts it.<br>2. The actor selects the record to open.<br>3. The SPA issues `GET /api/LookupManagement/departments` carrying userId.<br>4. `IncomingOutgoingController` binds the typed request DTO and delegates to the application service.<br>5. `IOutgoingService` validates the payload with its FluentValidation validator, applies the business rules and the charity scope, and persists through `IUnitOfWork`.<br>6. The system returns the record with its child collections and the SPA binds them to the form fields. |
| Alternate flows | • An HQ role (General Director, Financial Director, Staff) may pass an explicit charity id and so read across the charity boundary; a charity user may not and always sees its own data. |
| Exception flows | • The session has expired or the role is not permitted — the request is rejected and the SPA routes back to the login state. |
| Post-conditions | • No stored data is changed — the operation is a read. |
| Realisation | `GET /api/LookupManagement/departments` → `LookupManagementController` → `ILookupService` |

#### 21.U.9  UC-COR-09 — Attach employees to an incoming letter ربط الموظفين بخطاب وارد


| Item | Specification |
| --- | --- |
| Use case ID | UC-COR-09 |
| Name | Attach employees to an incoming letter ربط الموظفين بخطاب وارد |
| Type | Browse a list |
| Primary actor | Staff |
| Secondary actors | The system (Web API + business layer); the database. |
| Summary | Lists the employees already attached to an incoming letter and those still unattached for the chosen charity and letter, and saves the selection — used to record who a received letter concerns. |
| Trigger | The actor opens the screen at `#/incoming-outgoing/export/incoming` from the module menu. |
| Pre-conditions | 1. The actor is authenticated; the JWT access token is valid and the client holds the role, charity and country claims it carries.<br>2. The actor holds one of: Staff.<br>3. Caller identity comes from the JWT, never from the request; the application service scopes the query to the charity (and country) that owns the user, unless the role is an HQ role permitted to pass an explicit `charityId` filter.<br>4. The SPA route `#/incoming-outgoing/export/incoming` has loaded and its reference-data lookups have been populated. |
| Main flow | 1. The actor navigates to the screen at `#/incoming-outgoing/export/incoming`.<br>2. The system loads the filter lookups the screen offers and, for HQ roles, the charity selector.<br>3. The SPA issues `GET /api/IncomingOutgoing/incoming` carrying string userId.<br>4. `IncomingOutgoingController` binds the typed request DTO and delegates to the application service.<br>5. `IOutgoingService` validates the payload with its FluentValidation validator, applies the business rules and the charity scope, and persists through `IUnitOfWork`.<br>6. The result set is scoped to the caller’s charity and country, ordered and paged.<br>7. The SPA renders the page with the columns listed in the screen specification and the paging control. |
| Alternate flows | • An HQ role (General Director, Financial Director, Staff) may pass an explicit charity id and so read across the charity boundary; a charity user may not and always sees its own data.<br>• No row matches the criteria — the grid renders empty and the paging control reports zero pages. |
| Exception flows | • The session has expired or the role is not permitted — the request is rejected and the SPA routes back to the login state.<br>• The business layer returns «Operation Faild» and the operation is not applied. |
| Post-conditions | • No stored data is changed — the operation is a read. |
| Realisation | Route `#/incoming-outgoing/export/incoming` → `ExportWizardComponent`<br>`GET /api/IncomingOutgoing/incoming` · `POST /api/IncomingOutgoing/incoming` · `GET /api/IncomingOutgoing/incoming/statuses` → `IncomingOutgoingController` → `IIncomingService` |

#### 21.U.10  UC-COR-10 — List outgoing letters الصادر


| Item | Specification |
| --- | --- |
| Use case ID | UC-COR-10 |
| Name | List outgoing letters الصادر |
| Type | Browse a list |
| Primary actor | Staff, Gen. Director |
| Secondary actors | The system (Web API + business layer); the database. |
| Summary | Paged register of dispatched letters with serial, date, recipient and subject. |
| Trigger | The actor opens the screen at `#/incoming-outgoing/outgoing` from the module menu. |
| Pre-conditions | 1. The actor is authenticated; the JWT access token is valid and the client holds the role, charity and country claims it carries.<br>2. The actor holds one of: Staff, Gen. Director.<br>3. Caller identity comes from the JWT, never from the request; the application service scopes the query to the charity (and country) that owns the user, unless the role is an HQ role permitted to pass an explicit `charityId` filter.<br>4. The SPA route `#/incoming-outgoing/outgoing` has loaded and its reference-data lookups have been populated. |
| Main flow | 1. The actor navigates to the screen at `#/incoming-outgoing/outgoing`.<br>2. The system loads the filter lookups the screen offers and, for HQ roles, the charity selector.<br>3. The SPA issues `GET /api/IncomingOutgoing/outgoing` carrying userId, pagenum.<br>4. `IncomingOutgoingController` binds the typed request DTO and delegates to the application service.<br>5. `IOutgoingService` validates the payload with its FluentValidation validator, applies the business rules and the charity scope, and persists through `IUnitOfWork`.<br>6. The result set is scoped to the caller’s charity and country, ordered and paged.<br>7. The SPA renders the page with the columns listed in the screen specification and the paging control. |
| Alternate flows | • An HQ role (General Director, Financial Director, Staff) may pass an explicit charity id and so read across the charity boundary; a charity user may not and always sees its own data.<br>• No row matches the criteria — the grid renders empty and the paging control reports zero pages. |
| Exception flows | • The session has expired or the role is not permitted — the request is rejected and the SPA routes back to the login state. |
| Post-conditions | • No stored data is changed — the operation is a read. |
| Realisation | Route `#/incoming-outgoing/outgoing` → `OutgoingLettersListComponent`<br>`GET /api/IncomingOutgoing/outgoing` → `IncomingOutgoingController` → `IOutgoingService` |

#### 21.U.11  UC-COR-11 — Search outgoing letters البحث في الصادر


| Item | Specification |
| --- | --- |
| Use case ID | UC-COR-11 |
| Name | Search outgoing letters البحث في الصادر |
| Type | Search / filter |
| Primary actor | Staff, Gen. Director |
| Secondary actors | The system (Web API + business layer); the database. |
| Summary | Filters by description, letter number and date range, with paging optionally disabled; an unpaged variant supports export. |
| Trigger | The actor selects the function from the module menu or the screen that hosts it. |
| Pre-conditions | 1. The actor is authenticated; the JWT access token is valid and the client holds the role, charity and country claims it carries.<br>2. The actor holds one of: Staff, Gen. Director. |
| Main flow | 1. The actor reaches the function from the screen of this module that hosts it.<br>2. The actor enters the search criteria in the filter fields.<br>3. The actor presses the command button of the screen.<br>4. The SPA issues `GET /api/IncomingOutgoing/outgoing` carrying string OutgoingDescription="", string LetterNumber="", DateTime? DateFrom = null, DateTime? DateTo = null, int? pagenum = 1, bool? UsePagging = true.<br>5. `IncomingOutgoingController` binds the typed request DTO and delegates to the application service.<br>6. `IOutgoingService` validates the payload with its FluentValidation validator, applies the business rules and the charity scope, and persists through `IUnitOfWork`.<br>7. The matching rows are returned, scoped to the caller’s charity, and rendered in the result grid. |
| Alternate flows | • No row matches the criteria — the grid renders empty and the paging control reports zero pages. |
| Exception flows | • The session has expired or the role is not permitted — the request is rejected and the SPA routes back to the login state. |
| Post-conditions | • No stored data is changed — the operation is a read. |
| Realisation | `GET /api/IncomingOutgoing/outgoing` · `GET /api/IncomingOutgoing/export/outgoing` → `IncomingOutgoingController` → `IOutgoingService` |

#### 21.U.12  UC-COR-12 — Obtain the next outgoing serial رقم الصادر التالي


| Item | Specification |
| --- | --- |
| Use case ID | UC-COR-12 |
| Name | Obtain the next outgoing serial رقم الصادر التالي |
| Type | Read a record |
| Primary actor | Staff |
| Secondary actors | The system (Web API + business layer); the database. |
| Summary | Reserves the next sequential dispatch number. |
| Trigger | The actor selects the function from the module menu or the screen that hosts it. |
| Pre-conditions | 1. The actor is authenticated; the JWT access token is valid and the client holds the role, charity and country claims it carries.<br>2. The actor holds one of: Staff.<br>3. Caller identity comes from the JWT, never from the request; the application service scopes the query to the charity (and country) that owns the user, unless the role is an HQ role permitted to pass an explicit `charityId` filter.<br>4. The target record exists and its identifier is known to the screen. |
| Main flow | 1. The actor reaches the function from the screen of this module that hosts it.<br>2. The actor selects the record to open.<br>3. The SPA issues `GET /api/IncomingOutgoing/outgoing/next-serial` carrying UserId.<br>4. `IncomingOutgoingController` binds the typed request DTO and delegates to the application service.<br>5. `IOutgoingService` validates the payload with its FluentValidation validator, applies the business rules and the charity scope, and persists through `IUnitOfWork`.<br>6. The system returns the record with its child collections and the SPA binds them to the form fields. |
| Alternate flows | • An HQ role (General Director, Financial Director, Staff) may pass an explicit charity id and so read across the charity boundary; a charity user may not and always sees its own data. |
| Exception flows | • The session has expired or the role is not permitted — the request is rejected and the SPA routes back to the login state. |
| Post-conditions | • No stored data is changed — the operation is a read. |
| Realisation | `GET /api/IncomingOutgoing/outgoing/next-serial` → `IncomingOutgoingController` → `IOutgoingService` |

#### 21.U.13  UC-COR-13 — Register an outgoing letter تسجيل صادر


| Item | Specification |
| --- | --- |
| Use case ID | UC-COR-13 |
| Name | Register an outgoing letter تسجيل صادر |
| Type | Create a record |
| Primary actor | Staff |
| Secondary actors | The system (Web API + business layer); the database. |
| Summary | Records the dispatch — serial, date, recipient, originating department, category, subject and attachments. |
| Trigger | The actor presses «حفظ» on the screen outgoing. |
| Pre-conditions | 1. The actor is authenticated; the JWT access token is valid and the client holds the role, charity and country claims it carries.<br>2. The actor holds one of: Staff.<br>3. The SPA route `#/incoming-outgoing/outgoing/:id/edit` has loaded and its reference-data lookups have been populated. |
| Main flow | 1. The actor navigates to the screen at `#/incoming-outgoing/outgoing/:id/edit`. The screen opens in add mode.<br>2. The system loads the reference data the form needs (regions, centres, banks, types and the other lookup lists bound to the drop-downs).<br>3. The actor completes the input fields. Mandatory fields: رقم الصادر، الادارة او الجهة المرسل لها الخطاب، التاريخ، البيان، تصنيف موضوع الصادر، ردا علي خطاب.<br>4. The actor presses «حفظ» (SubmitOutGoing()).<br>5. The SPA issues `POST /api/IncomingOutgoing/outgoing` carrying typed request DTO obj.<br>6. `IncomingOutgoingController` binds the typed request DTO and delegates to the application service.<br>7. `IOutgoingService` validates the payload with its FluentValidation validator, applies the business rules and the charity scope, and persists through `IUnitOfWork`.<br>8. The business layer validates the payload, stamps the owning charity and the creating user, and persists the new record.<br>9. The system returns the outcome and the SPA confirms the save and returns to the list screen. |
| Alternate flows | • The actor abandons the form before saving — nothing is written and the record keeps its previous state. |
| Exception flows | • The session has expired or the role is not permitted — the request is rejected and the SPA routes back to the login state.<br>• A mandatory field is empty or fails its format check — the save is refused and the field is flagged on the form. |
| Post-conditions | • A new record exists, owned by the charity of the creating user, and appears in the list screen of the module. |
| Realisation | Route `#/incoming-outgoing/outgoing/create` → `OutgoingLetterFormComponent`<br>`POST /api/IncomingOutgoing/outgoing` → `IncomingOutgoingController` → `IOutgoingService` |

#### 21.U.14  UC-COR-14 — View an outgoing letter عرض الصادر


| Item | Specification |
| --- | --- |
| Use case ID | UC-COR-14 |
| Name | View an outgoing letter عرض الصادر |
| Type | Read a record |
| Primary actor | Staff, Gen. Director |
| Secondary actors | The system (Web API + business layer); the database. |
| Summary | Loads one dispatched letter with its attachments and the orphan reports linked to it. |
| Trigger | The actor selects the function from the module menu or the screen that hosts it. |
| Pre-conditions | 1. The actor is authenticated; the JWT access token is valid and the client holds the role, charity and country claims it carries.<br>2. The actor holds one of: Staff, Gen. Director.<br>3. The target record exists and its identifier is known to the screen. |
| Main flow | 1. The actor reaches the function from the screen of this module that hosts it.<br>2. The actor selects the record to open.<br>3. The SPA issues `GET /api/IncomingOutgoing/outgoing/{id}` carrying id.<br>4. `IncomingOutgoingController` binds the typed request DTO and delegates to the application service.<br>5. `IOutgoingService` validates the payload with its FluentValidation validator, applies the business rules and the charity scope, and persists through `IUnitOfWork`.<br>6. The system returns the record with its child collections and the SPA binds them to the form fields. |
| Alternate flows | None recorded. |
| Exception flows | • The session has expired or the role is not permitted — the request is rejected and the SPA routes back to the login state. |
| Post-conditions | • No stored data is changed — the operation is a read. |
| Realisation | `GET /api/IncomingOutgoing/outgoing/{id}` → `IncomingOutgoingController` → `IOutgoingService` |

#### 21.U.15  UC-COR-15 — Update an outgoing letter تعديل الصادر


| Item | Specification |
| --- | --- |
| Use case ID | UC-COR-15 |
| Name | Update an outgoing letter تعديل الصادر |
| Type | Update a record |
| Primary actor | Staff |
| Secondary actors | The system (Web API + business layer); the database. |
| Summary | Amends the dispatch data before or after sending. |
| Trigger | The actor selects the function from the module menu or the screen that hosts it. |
| Pre-conditions | 1. The actor is authenticated; the JWT access token is valid and the client holds the role, charity and country claims it carries.<br>2. The actor holds one of: Staff.<br>3. The target record exists and its identifier is known to the screen. |
| Main flow | 1. The actor reaches the function from the screen of this module that hosts it. The screen opens in edit mode on the selected record.<br>2. The system loads the current values into the form.<br>3. The actor changes the fields to be corrected.<br>4. The actor presses the command button of the screen.<br>5. The SPA issues `PUT /api/IncomingOutgoing/outgoing` carrying typed request DTO obj.<br>6. `IncomingOutgoingController` binds the typed request DTO and delegates to the application service.<br>7. `IOutgoingService` validates the payload with its FluentValidation validator, applies the business rules and the charity scope, and persists through `IUnitOfWork`.<br>8. The business layer re-validates the payload, applies the changes and persists them.<br>9. The system returns the outcome and the SPA refreshes the screen. |
| Alternate flows | • The actor abandons the form before saving — nothing is written and the record keeps its previous state. |
| Exception flows | • The session has expired or the role is not permitted — the request is rejected and the SPA routes back to the login state.<br>• A mandatory field is empty or fails its format check — the save is refused and the field is flagged on the form. |
| Post-conditions | • The stored record carries the new values; no other record is affected. |
| Realisation | `PUT /api/IncomingOutgoing/outgoing` → `IncomingOutgoingController` → `IOutgoingService` |

#### 21.U.16  UC-COR-16 — Delete an outgoing letter حذف الصادر


| Item | Specification |
| --- | --- |
| Use case ID | UC-COR-16 |
| Name | Delete an outgoing letter حذف الصادر |
| Type | Delete a record |
| Primary actor | Staff, Gen. Director |
| Secondary actors | The system (Web API + business layer); the database. |
| Summary | Removes a dispatch registered in error. |
| Trigger | The actor selects the function from the module menu or the screen that hosts it. |
| Pre-conditions | 1. The actor is authenticated; the JWT access token is valid and the client holds the role, charity and country claims it carries.<br>2. The actor holds one of: Staff, Gen. Director.<br>3. The target record exists and its identifier is known to the screen. |
| Main flow | 1. The actor reaches the function from the screen of this module that hosts it.<br>2. The actor selects the record and requests its deletion.<br>3. The SPA asks the actor to confirm.<br>4. The SPA issues `DELETE /api/IncomingOutgoing/outgoing` carrying id.<br>5. `IncomingOutgoingController` binds the typed request DTO and delegates to the application service.<br>6. `IOutgoingService` validates the payload with its FluentValidation validator, applies the business rules and the charity scope, and persists through `IUnitOfWork`.<br>7. The business layer checks that the record may still be removed and deletes it (or marks it removed).<br>8. The system returns the outcome and the SPA drops the row from the grid. |
| Alternate flows | None recorded. |
| Exception flows | • The session has expired or the role is not permitted — the request is rejected and the SPA routes back to the login state. |
| Post-conditions | • The record is no longer returned by the list and read endpoints of the module. |
| Realisation | `DELETE /api/IncomingOutgoing/outgoing` → `IncomingOutgoingController` → `IOutgoingService` |

#### 21.U.17  UC-COR-17 — Select the outgoing category تصنيف الصادر


| Item | Specification |
| --- | --- |
| Use case ID | UC-COR-17 |
| Name | Select the outgoing category تصنيف الصادر |
| Type | Read a record |
| Primary actor | Staff |
| Secondary actors | The system (Web API + business layer); the database. |
| Summary | Loads the dispatch-category catalogue used to classify outgoing letters. |
| Trigger | The actor selects the function from the module menu or the screen that hosts it. |
| Pre-conditions | 1. The actor is authenticated; the JWT access token is valid and the client holds the role, charity and country claims it carries.<br>2. The actor holds one of: Staff.<br>3. The target record exists and its identifier is known to the screen. |
| Main flow | 1. The actor reaches the function from the screen of this module that hosts it.<br>2. The actor selects the record to open.<br>3. The SPA issues `GET /api/IncomingOutgoing/outgoing/categories`.<br>4. `IncomingOutgoingController` binds the typed request DTO and delegates to the application service.<br>5. `IOutgoingService` validates the payload with its FluentValidation validator, applies the business rules and the charity scope, and persists through `IUnitOfWork`.<br>6. The system returns the record with its child collections and the SPA binds them to the form fields. |
| Alternate flows | None recorded. |
| Exception flows | • The session has expired or the role is not permitted — the request is rejected and the SPA routes back to the login state. |
| Post-conditions | • No stored data is changed — the operation is a read. |
| Realisation | `GET /api/IncomingOutgoing/outgoing/categories` → `IncomingOutgoingController` → `IOutgoingService` |

#### 21.U.18  UC-COR-18 — Attach orphan reports to an outgoing letter إضافة تقارير الأيتام إلى خطاب صادر


| Item | Specification |
| --- | --- |
| Use case ID | UC-COR-18 |
| Name | Attach orphan reports to an outgoing letter إضافة تقارير الأيتام إلى خطاب صادر |
| Type | Create a record |
| Primary actor | Staff |
| Secondary actors | The system (Web API + business layer); the database. |
| Summary | For a chosen charity and outgoing letter, the system lists the orphan reports already attached and those still unattached; the operator selects the reports to include and saves, creating the auditable link between the dispatch and the reports it carried. |
| Trigger | The actor presses «حفظ» on the screen OutgoingsReport. |
| Pre-conditions | 1. The actor is authenticated; the JWT access token is valid and the client holds the role, charity and country claims it carries.<br>2. The actor holds one of: Staff.<br>3. Caller identity comes from the JWT, never from the request; the application service scopes the query to the charity (and country) that owns the user, unless the role is an HQ role permitted to pass an explicit `charityId` filter.<br>4. The SPA route `#/incoming-outgoing/export/outgoing` has loaded and its reference-data lookups have been populated. |
| Main flow | 1. The actor navigates to the screen at `#/incoming-outgoing/export/outgoing`. The screen opens in add mode.<br>2. The system loads the reference data the form needs (regions, centres, banks, types and the other lookup lists bound to the drop-downs).<br>3. The actor completes the input fields. Mandatory fields: خطاب الصادر.<br>4. The actor presses «حفظ» (UpdateData()).<br>5. The SPA issues `GET /api/IncomingOutgoing/outgoing/{parentOutgoingId}/children` carrying string CharityId, string OutGoingId,string userId.<br>6. `IncomingOutgoingController` binds the typed request DTO and delegates to the application service.<br>7. `IOutgoingService` validates the payload with its FluentValidation validator, applies the business rules and the charity scope, and persists through `IUnitOfWork`.<br>8. The business layer validates the payload, stamps the owning charity and the creating user, and persists the new record.<br>9. The system returns the outcome and the SPA confirms the save and returns to the list screen. |
| Alternate flows | • An HQ role (General Director, Financial Director, Staff) may pass an explicit charity id and so read across the charity boundary; a charity user may not and always sees its own data.<br>• The actor abandons the form before saving — nothing is written and the record keeps its previous state. |
| Exception flows | • The session has expired or the role is not permitted — the request is rejected and the SPA routes back to the login state.<br>• The business layer returns «Operation Faild» and the operation is not applied.<br>• A mandatory field is empty or fails its format check — the save is refused and the field is flagged on the form. |
| Post-conditions | • A new record exists, owned by the charity of the creating user, and appears in the list screen of the module. |
| Realisation | Route `#/incoming-outgoing/export/outgoing` → `ExportWizardComponent`<br>`GET /api/IncomingOutgoing/outgoing/{parentOutgoingId}/children` · `POST /api/IncomingOutgoing/outgoing/{parentOutgoingId}/child` · `GET /api/IncomingOutgoing/outgoing/categories` → `IncomingOutgoingController` → `IOutgoingService` |

#### 21.U.19  UC-COR-19 — Report orphans by outgoing letter تقرير الأيتام حسب الخطاب الصادر


| Item | Specification |
| --- | --- |
| Use case ID | UC-COR-19 |
| Name | Report orphans by outgoing letter تقرير الأيتام حسب الخطاب الصادر |
| Type | Query a report |
| Primary actor | Staff, Gen. Director |
| Secondary actors | The system (Web API + business layer); the database. |
| Summary | Given a dispatch serial and year (optionally a charity, date range or orphan code), returns the orphans whose reports were sent under that letter — the answer to "which reports did we send, and when?". A companion report lists missing attachments. |
| Trigger | The actor opens the screen at `#/incoming-outgoing/export/outgoing-orphans` from the module menu. |
| Pre-conditions | 1. The actor is authenticated; the JWT access token is valid and the client holds the role, charity and country claims it carries.<br>2. The actor holds one of: Staff, Gen. Director.<br>3. Caller identity comes from the JWT, never from the request; the application service scopes the query to the charity (and country) that owns the user, unless the role is an HQ role permitted to pass an explicit `charityId` filter.<br>4. The SPA route `#/incoming-outgoing/export/outgoing-orphans` has loaded and its reference-data lookups have been populated. |
| Main flow | 1. The actor navigates to the screen at `#/incoming-outgoing/export/outgoing-orphans`.<br>2. The actor sets the report criteria (period, charity, country and the other filters the screen offers).<br>3. The actor runs the report.<br>4. The SPA issues `GET /api/IncomingOutgoing/outgoing?orphanNumber=` carrying Serial, Year, CharityId, DateFrom, DateTo, ChildCode.<br>5. `IncomingOutgoingController` binds the typed request DTO and delegates to the application service.<br>6. `IOutgoingService` validates the payload with its FluentValidation validator, applies the business rules and the charity scope, and persists through `IUnitOfWork`.<br>7. The business layer composes the projection, scoped to the caller’s charity and country.<br>8. The result is rendered in the on-screen grid, from where it can be printed or exported. |
| Alternate flows | • An HQ role (General Director, Financial Director, Staff) may pass an explicit charity id and so read across the charity boundary; a charity user may not and always sees its own data.<br>• No row matches the criteria — the grid renders empty and the paging control reports zero pages. |
| Exception flows | • The session has expired or the role is not permitted — the request is rejected and the SPA routes back to the login state.<br>• The business layer returns «Operation Faild» and the operation is not applied. |
| Post-conditions | • No stored data is changed — the operation is a read. |
| Realisation | Route `#/incoming-outgoing/export/outgoing-orphans` → `ExportWizardComponent`<br>`GET /api/IncomingOutgoing/outgoing?orphanNumber=` → `IncomingOutgoingController` → `IOutgoingService` |

### 21.A  Annex - Angular routes of this module

Routes are hash-based (`useHash: true`), rendered inside `MainLayoutComponent` behind
`AuthGuard` and `PermissionGuard`. *implemented* means the route exists in
`Frontend/src/app`; *planned* means it is specified here and not yet built.

| Angular route | Feature module | Component | Status |
| --- | --- | --- | --- |
| `#/incoming-outgoing` | `incoming-outgoing` | redirects to `#/incoming-outgoing/incoming` | implemented |
| `#/incoming-outgoing/incoming` | `incoming-outgoing` | `IncomingLettersListComponent` | implemented |
| `#/incoming-outgoing/incoming/create` | `incoming-outgoing` | `IncomingLetterFormComponent` | implemented |
| `#/incoming-outgoing/incoming/:id` | `incoming-outgoing` | `IncomingLetterDetailComponent` | implemented |
| `#/incoming-outgoing/incoming/:id/edit` | `incoming-outgoing` | `IncomingLetterFormComponent` | implemented |
| `#/incoming-outgoing/outgoing` | `incoming-outgoing` | `OutgoingLettersListComponent` | implemented |
| `#/incoming-outgoing/outgoing/create` | `incoming-outgoing` | `OutgoingLetterFormComponent` | implemented |
| `#/incoming-outgoing/outgoing/:id` | `incoming-outgoing` | `OutgoingLetterDetailComponent` | implemented |
| `#/incoming-outgoing/outgoing/:id/edit` | `incoming-outgoing` | `OutgoingLetterFormComponent` | implemented |
| `#/incoming-outgoing/import/:type` | `incoming-outgoing` | `ImportWizardComponent` | implemented |
| `#/incoming-outgoing/export/:type` | `incoming-outgoing` | `ExportWizardComponent` | implemented |
| `#/incoming-outgoing/history` | `incoming-outgoing` | `HistoryComponent` | implemented |

> `export/:type` is one parameterised route. `#/incoming-outgoing/export/incoming`,
> `…/export/outgoing` and `…/export/outgoing-orphans` are values of `:type`, not separate routes.

### 21.B  Annex - API controllers of this module

Every controller inherits `ApiController` (`[Authorize]`, route `api/[controller]`) and
returns `Framework.Core.ApiResponse<T>`. Business rules live in the application service
behind each controller, never in the controller itself.

| Controller | Base route | Responsibility |
| --- | --- | --- |
| `IncomingOutgoingController` | `api/IncomingOutgoing` | Incoming letter CRUD, search, serial, departments. Outgoing letter CRUD, search, serial, categories, orphan dispatch report. Attach orphan reports to an outgoing letter. Attach employees to an incoming letter. |

---

Shared context - system architecture, actors and roles, the sponsorship lifecycle, the full module map and all appendices - is in the companion document [00-Overview-and-Common-Context](00-Overview-and-Common-Context.md). The delivery backlog for this module is epic EP-16 in [00-EPICS-and-User-Stories](00-EPICS-and-User-Stories.md).

