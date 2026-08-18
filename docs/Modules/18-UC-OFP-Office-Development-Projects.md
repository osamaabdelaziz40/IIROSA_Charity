# WAR.IIROSA - Office Development Projects

المشاريع التنموية للمكتب | use case prefix `UC-OFP` | chapter 18 of the master document


| Field | Value |
| --- | --- |
| Document title | WAR.IIROSA - Module Specification: Office Development Projects |
| Module | Office Development Projects - المشاريع التنموية للمكتب |
| Use case prefix | UC-OFP |
| Chapter in master document | Chapter 18 |
| Documented use cases | 6 |
| Principal routes | `#/office-development-projects`, `#/office-development-projects/:id/edit` |
| Version | 1.1 |
| Status | Chapter content extracted verbatim; screen fields and scenarios derived from the source code |
| Date | 18 August 2026 |
| Parent document | WAR.IIROSA-Project-Documentation.docx v1.0 (repository WAR.IIROSA, commit 4beb736) |

## Contents of this document

1. Chapter 18 — module purpose and use-case catalogue (verbatim from the master document)
2. §18.S — screen field specifications (every field of every screen, derived from the AngularJS views)
3. §18.U — one expanded scenario per use case (trigger, pre-conditions, main flow, alternates, exceptions, post-conditions)
4. §18.A / §18.B — annexes: screens and Web API controllers of this module

## 18. Office Development Projects

المشاريع التنموية للمكتب — capital and development projects run by the head office itself (well drilling, mosque construction, vocational training), tracked independently of the family assistance campaigns.


| ID | Use case | Primary actor | Description & main flow | Realisation |
| --- | --- | --- | --- | --- |
| UC-OFP-01 | List development projects المشاريع التنموية | Gen. Director, Staff | Paged register of the office's development projects. | Route `#/office-development-projects` → GET /api/OfficeProjectManagement |
| UC-OFP-02 | Select a project type نوع المشروع | Gen. Director, Staff | Loads the catalogue of development project types that classifies each project. | GET /api/LookupManagement/office-project-types |
| UC-OFP-03 | Create a development project اضافة مشروع تنموي | Gen. Director, Staff | Captures the project name, type, location, beneficiaries, budget, donor and execution dates. | Route `#/office-development-projects/:id/edit` → POST /api/OfficeProjectManagement |
| UC-OFP-04 | View / update a development project تعديل المشروع التنموي | Gen. Director, Staff | Loads one project by id and saves amendments to its data and progress. | GET /api/OfficeProjectManagement/{id} PUT /api/OfficeProjectManagement |
| UC-OFP-05 | Delete a development project حذف المشروع التنموي | Gen. Director | Removes a project record. | DELETE /api/OfficeProjectManagement |
| UC-OFP-06 | Report on development projects تقرير المشاريع التنموية | Gen. Director, Staff | Returns the reporting projection of the project register for export and printing. | GET /api/OfficeProjectManagement/export |

### 18.S  Screen field specifications

Derived from the AngularJS views of this module. For every screen the table lists each data-entry field in the order it appears, the scope property it binds to, its control type, whether the form marks it mandatory, and the lookup or rule that governs it. Labels are reproduced as they appear on screen (Arabic, right-to-left).

#### 18.S.1  Screen `#/office-development-projects`


| Property | Value |
| --- | --- |
| Angular route | `#/office-development-projects` |
| Feature module | `office-development-projects` (lazy-loaded) |
| Component | `ProjectListComponent` |
| Route status | implemented |
| Data-entry fields | 0 |
| Grids on the screen | 1 |
| Commands | 6 |

Grids on this screen:

| Grid (row source) | Columns |
| --- | --- |
| project in OfficeProjects | الرقم · اسم المشروع · اشم المتبرع · التكلفه بالجنيه · التكلفه بالريال · الجمعيه |

Commands on this screen:

| Command | Handler | Shown when |
| --- | --- | --- |
| حفظ | DeleteOfficeProject() | always |
| (icon only) | ExtractAllData() | always |
| (icon only) | EditOfficeProject(project.Id) | always |
| (icon only) | ViewDeleteModel(project.Id) | always |
| (icon only) | GetNext() | always |
| (icon only) | GetPrev() | always |

#### 18.S.2  Screen `#/office-development-projects/:id/edit`


| Property | Value |
| --- | --- |
| Angular route | `#/office-development-projects/:id/edit` |
| Feature module | `office-development-projects` (lazy-loaded) |
| Component | `ProjectFormComponent` |
| Route status | implemented |
| Data-entry fields | 18 |
| Grids on the screen | 0 |
| Commands | 2 |


| Section | Field (as labelled) | Bound to | Control | Mandatory · source · rules |
| --- | --- | --- | --- | --- |
| — | الجمعية | CharityId | Drop-down list | Optional · options: lookup: Charities (+ كافة الجهات) · on change: getCharityData() |
| إضافة مشروع | تاريخ نهايه المشروع | Project.ProjectEndDate | Date picker | Mandatory |
| إضافة مشروع | تاريخ بدايه المشروع | Project.ProjectDate | Date picker | Mandatory |
| إضافة مشروع | اسم المتبرع | Project.DonorName | Text box | Mandatory |
| إضافة مشروع | اسم المشروع | Project.ProjectName | Text box | Mandatory |
| إضافة مشروع | اسم الجمعيه المتعاونه | Campaign.CharityName | Text box | Mandatory |
| إضافة مشروع | نبذه عن المشروع | Project.ProjectHint | Text box | Mandatory |
| إضافة مشروع | نوع المشروع | Project.OfficeProjectTypeId | Drop-down list | Mandatory · options: lookup: OfficeProjectTypes |
| إضافة مشروع | تكلفه المشروع بالريال | Project.ProjectCostIn_Ryal | Numeric box | Mandatory |
| إضافة مشروع | تكلفه المشروع بالجنيه | Project.ProjectCostIn_Egy | Numeric box | Mandatory |
| إضافة مشروع | القرية / الحي | Project.VillageName | Text box | Mandatory |
| إضافة مشروع | المركز/ المدينة | Project.CenterId | Drop-down list | Mandatory · options: lookup: Centers |
| إضافة مشروع | المنطقة /المحافظة | Project.RegionId | Drop-down list | Mandatory · options: lookup: Regions · on change: Getcenters() |
| إضافة مشروع | (unlabelled) | Project.BeneficiariesType | Radio button | Optional |
| إضافة مشروع | نوع الاستفاده | Project.BeneficiariesType | Radio button | Mandatory |
| إضافة مشروع | عدد المستفيدين | Project.BeneficiariesCount | Numeric box | Mandatory |
| إضافة مشروع | الملف | Project.AttachedFile | File upload | Optional · accepts application/pdf |
| إضافة مشروع | تقرير المشروع | Project.AttachedFile | File upload | Optional · accepts application/pdf |

Commands on this screen:

| Command | Handler | Shown when |
| --- | --- | --- |
| (icon only) | RemoveImage() | Project.AttachedFile!=null |
| حفظ | SubmitAdd() | always |

### 18.U  Use case scenarios

One expanded scenario for every use case of this module. Pre-conditions, flows and post-conditions are derived from the screen that hosts the function, the Web API action that serves it and the business-layer method that executes it; quoted messages are the literal strings returned by the business layer.

#### 18.U.1  UC-OFP-01 — List development projects المشاريع التنموية


| Item | Specification |
| --- | --- |
| Use case ID | UC-OFP-01 |
| Name | List development projects المشاريع التنموية |
| Type | Browse a list |
| Primary actor | Gen. Director, Staff |
| Secondary actors | The system (Web API + business layer); the database. |
| Summary | Paged register of the office's development projects. |
| Trigger | The actor opens the screen at `#/office-development-projects` from the module menu. |
| Pre-conditions | 1. The actor is authenticated; the JWT access token is valid and the client holds the role, charity and country claims it carries.<br>2. The actor holds one of: Gen. Director, Staff.<br>3. The SPA route `#/office-development-projects` has loaded and its reference-data lookups have been populated. |
| Main flow | 1. The actor navigates to the screen at `#/office-development-projects`.<br>2. The system loads the filter lookups the screen offers and, for HQ roles, the charity selector.<br>3. The SPA issues `GET /api/OfficeProjectManagement` carrying pagenum.<br>4. `OfficeProjectManagementController` binds the typed request DTO and delegates to the application service.<br>5. `IOfficeProjectService` validates the payload with its FluentValidation validator, applies the business rules and the charity scope, and persists through `IUnitOfWork`.<br>6. The result set is scoped to the caller’s charity and country, ordered and paged.<br>7. The SPA renders the page with the columns listed in the screen specification and the paging control. |
| Alternate flows | • No row matches the criteria — the grid renders empty and the paging control reports zero pages. |
| Exception flows | • The session has expired or the role is not permitted — the request is rejected and the SPA routes back to the login state. |
| Post-conditions | • No stored data is changed — the operation is a read. |
| Realisation | Route `#/office-development-projects` → `ProjectListComponent`<br>`GET /api/OfficeProjectManagement` → `OfficeProjectManagementController` → `IOfficeProjectService` |

#### 18.U.2  UC-OFP-02 — Select a project type نوع المشروع


| Item | Specification |
| --- | --- |
| Use case ID | UC-OFP-02 |
| Name | Select a project type نوع المشروع |
| Type | Read a record |
| Primary actor | Gen. Director, Staff |
| Secondary actors | The system (Web API + business layer); the database. |
| Summary | Loads the catalogue of development project types that classifies each project. |
| Trigger | The actor selects the function from the module menu or the screen that hosts it. |
| Pre-conditions | 1. The actor is authenticated; the JWT access token is valid and the client holds the role, charity and country claims it carries.<br>2. The actor holds one of: Gen. Director, Staff.<br>3. The target record exists and its identifier is known to the screen. |
| Main flow | 1. The actor reaches the function from the screen of this module that hosts it.<br>2. The actor selects the record to open.<br>3. The SPA issues `GET /api/LookupManagement/office-project-types`.<br>4. `OfficeProjectManagementController` binds the typed request DTO and delegates to the application service.<br>5. `IOfficeProjectService` validates the payload with its FluentValidation validator, applies the business rules and the charity scope, and persists through `IUnitOfWork`.<br>6. The system returns the record with its child collections and the SPA binds them to the form fields. |
| Alternate flows | None recorded. |
| Exception flows | • The session has expired or the role is not permitted — the request is rejected and the SPA routes back to the login state. |
| Post-conditions | • No stored data is changed — the operation is a read. |
| Realisation | `GET /api/LookupManagement/office-project-types` → `LookupManagementController` → `ILookupService` |

#### 18.U.3  UC-OFP-03 — Create a development project اضافة مشروع تنموي


| Item | Specification |
| --- | --- |
| Use case ID | UC-OFP-03 |
| Name | Create a development project اضافة مشروع تنموي |
| Type | Create a record |
| Primary actor | Gen. Director, Staff |
| Secondary actors | The system (Web API + business layer); the database. |
| Summary | Captures the project name, type, location, beneficiaries, budget, donor and execution dates. |
| Trigger | The actor presses «حفظ» on the screen AddOfficeProject. |
| Pre-conditions | 1. The actor is authenticated; the JWT access token is valid and the client holds the role, charity and country claims it carries.<br>2. The actor holds one of: Gen. Director, Staff.<br>3. The SPA route `#/office-development-projects/:id/edit` has loaded and its reference-data lookups have been populated. |
| Main flow | 1. The actor navigates to the screen at `#/office-development-projects/:id/edit`. The screen opens in add mode.<br>2. The system loads the reference data the form needs (regions, centres, banks, types and the other lookup lists bound to the drop-downs).<br>3. The actor completes the input fields. Mandatory fields: تاريخ نهايه المشروع، تاريخ بدايه المشروع، اسم المتبرع، اسم المشروع، اسم الجمعيه المتعاونه، نبذه عن المشروع، نوع المشروع، تكلفه المشروع بالريال، تكلفه المشروع بالجنيه، القرية / الحي، المركز/ المدينة، المنطقة /المحافظة، نوع الاستفاده، عدد المستفيدين.<br>4. The actor presses «حفظ» (SubmitAdd()).<br>5. The SPA issues `POST /api/OfficeProjectManagement` carrying OfficeProjectContract obj.<br>6. `OfficeProjectManagementController` binds the typed request DTO and delegates to the application service.<br>7. `IOfficeProjectService` validates the payload with its FluentValidation validator, applies the business rules and the charity scope, and persists through `IUnitOfWork`.<br>8. The business layer validates the payload, stamps the owning charity and the creating user, and persists the new record.<br>9. The system returns the outcome and the SPA confirms the save and returns to the list screen. |
| Alternate flows | • The actor abandons the form before saving — nothing is written and the record keeps its previous state. |
| Exception flows | • The session has expired or the role is not permitted — the request is rejected and the SPA routes back to the login state.<br>• A mandatory field is empty or fails its format check — the save is refused and the field is flagged on the form. |
| Post-conditions | • A new record exists, owned by the charity of the creating user, and appears in the list screen of the module. |
| Realisation | Route `#/office-development-projects/:id/edit` → `ProjectFormComponent`<br>`POST /api/OfficeProjectManagement` → `OfficeProjectManagementController` → `IOfficeProjectService` |

#### 18.U.4  UC-OFP-04 — View / update a development project تعديل المشروع التنموي


| Item | Specification |
| --- | --- |
| Use case ID | UC-OFP-04 |
| Name | View / update a development project تعديل المشروع التنموي |
| Type | Update a record |
| Primary actor | Gen. Director, Staff |
| Secondary actors | The system (Web API + business layer); the database. |
| Summary | Loads one project by id and saves amendments to its data and progress. |
| Trigger | The actor selects the function from the module menu or the screen that hosts it. |
| Pre-conditions | 1. The actor is authenticated; the JWT access token is valid and the client holds the role, charity and country claims it carries.<br>2. The actor holds one of: Gen. Director, Staff.<br>3. The target record exists and its identifier is known to the screen. |
| Main flow | 1. The actor reaches the function from the screen of this module that hosts it. The screen opens in edit mode on the selected record.<br>2. The system loads the current values into the form.<br>3. The actor changes the fields to be corrected.<br>4. The actor presses the command button of the screen.<br>5. The SPA issues `GET /api/OfficeProjectManagement/{id}` carrying id.<br>6. `OfficeProjectManagementController` binds the typed request DTO and delegates to the application service.<br>7. `IOfficeProjectService` validates the payload with its FluentValidation validator, applies the business rules and the charity scope, and persists through `IUnitOfWork`.<br>8. The business layer re-validates the payload, applies the changes and persists them.<br>9. The system returns the outcome and the SPA refreshes the screen. |
| Alternate flows | • The actor abandons the form before saving — nothing is written and the record keeps its previous state. |
| Exception flows | • The session has expired or the role is not permitted — the request is rejected and the SPA routes back to the login state.<br>• A mandatory field is empty or fails its format check — the save is refused and the field is flagged on the form. |
| Post-conditions | • The stored record carries the new values; no other record is affected. |
| Realisation | `GET /api/OfficeProjectManagement/{id}` · `PUT /api/OfficeProjectManagement` → `OfficeProjectManagementController` → `IOfficeProjectService` |

#### 18.U.5  UC-OFP-05 — Delete a development project حذف المشروع التنموي


| Item | Specification |
| --- | --- |
| Use case ID | UC-OFP-05 |
| Name | Delete a development project حذف المشروع التنموي |
| Type | Delete a record |
| Primary actor | Gen. Director |
| Secondary actors | The system (Web API + business layer); the database. |
| Summary | Removes a project record. |
| Trigger | The actor selects the function from the module menu or the screen that hosts it. |
| Pre-conditions | 1. The actor is authenticated; the JWT access token is valid and the client holds the role, charity and country claims it carries.<br>2. The actor holds one of: Gen. Director.<br>3. The target record exists and its identifier is known to the screen. |
| Main flow | 1. The actor reaches the function from the screen of this module that hosts it.<br>2. The actor selects the record and requests its deletion.<br>3. The SPA asks the actor to confirm.<br>4. The SPA issues `DELETE /api/OfficeProjectManagement` carrying id.<br>5. `OfficeProjectManagementController` binds the typed request DTO and delegates to the application service.<br>6. `IOfficeProjectService` validates the payload with its FluentValidation validator, applies the business rules and the charity scope, and persists through `IUnitOfWork`.<br>7. The business layer checks that the record may still be removed and deletes it (or marks it removed).<br>8. The system returns the outcome and the SPA drops the row from the grid. |
| Alternate flows | None recorded. |
| Exception flows | • The session has expired or the role is not permitted — the request is rejected and the SPA routes back to the login state. |
| Post-conditions | • The record is no longer returned by the list and read endpoints of the module. |
| Realisation | `DELETE /api/OfficeProjectManagement` → `OfficeProjectManagementController` → `IOfficeProjectService` |

#### 18.U.6  UC-OFP-06 — Report on development projects تقرير المشاريع التنموية


| Item | Specification |
| --- | --- |
| Use case ID | UC-OFP-06 |
| Name | Report on development projects تقرير المشاريع التنموية |
| Type | Query a report |
| Primary actor | Gen. Director, Staff |
| Secondary actors | The system (Web API + business layer); the database. |
| Summary | Returns the reporting projection of the project register for export and printing. |
| Trigger | The actor selects the function from the module menu or the screen that hosts it. |
| Pre-conditions | 1. The actor is authenticated; the JWT access token is valid and the client holds the role, charity and country claims it carries.<br>2. The actor holds one of: Gen. Director, Staff. |
| Main flow | 1. The actor reaches the function from the screen of this module that hosts it.<br>2. The actor sets the report criteria (period, charity, country and the other filters the screen offers).<br>3. The actor runs the report.<br>4. The SPA issues `GET /api/OfficeProjectManagement/export` carrying pagenum.<br>5. `OfficeProjectManagementController` binds the typed request DTO and delegates to the application service.<br>6. `IOfficeProjectService` validates the payload with its FluentValidation validator, applies the business rules and the charity scope, and persists through `IUnitOfWork`.<br>7. The business layer composes the projection, scoped to the caller’s charity and country.<br>8. The result is rendered in the on-screen grid, from where it can be printed or exported. |
| Alternate flows | • No row matches the criteria — the grid renders empty and the paging control reports zero pages. |
| Exception flows | • The session has expired or the role is not permitted — the request is rejected and the SPA routes back to the login state. |
| Post-conditions | • No stored data is changed — the operation is a read. |
| Realisation | `GET /api/OfficeProjectManagement/export` → `OfficeProjectManagementController` → `IOfficeProjectService` |

### 18.A  Annex - Angular routes of this module

Routes are hash-based (`useHash: true`), rendered inside `MainLayoutComponent` behind
`AuthGuard` and `PermissionGuard`. *implemented* means the route exists in
`Frontend/src/app`; *planned* means it is specified here and not yet built.

| Angular route | Feature module | Component | Status |
| --- | --- | --- | --- |
| `#/office-development-projects` | `office-development-projects` | `ProjectListComponent` | implemented |
| `#/office-development-projects/:id/edit` | `office-development-projects` | `ProjectFormComponent` | implemented |

### 18.B  Annex - API controllers of this module

Every controller inherits `ApiController` (`[Authorize]`, route `api/[controller]`) and
returns `Framework.Core.ApiResponse<T>`. Business rules live in the application service
behind each controller, never in the controller itself.

| Controller | Base route | Responsibility |
| --- | --- | --- |
| `OfficeProjectManagementController` | `api/OfficeProjectManagement` | Development project CRUD, types, report projection. |

---

Shared context - system architecture, actors and roles, the sponsorship lifecycle, the full module map and all appendices - is in the companion document [00-Overview-and-Common-Context](00-Overview-and-Common-Context.md). The delivery backlog for this module is epic EP-13 in [00-EPICS-and-User-Stories](00-EPICS-and-User-Stories.md).

