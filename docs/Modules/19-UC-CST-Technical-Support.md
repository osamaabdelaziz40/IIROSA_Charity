# WAR.IIROSA - Technical Support

الدعم الفني | use case prefix `UC-CST` | chapter 19 of the master document


| Field | Value |
| --- | --- |
| Document title | WAR.IIROSA - Module Specification: Technical Support |
| Module | Technical Support - الدعم الفني |
| Use case prefix | UC-CST |
| Chapter in master document | Chapter 19 |
| Documented use cases | 6 |
| Principal routes | `#/technical-support`, `#/technical-support/:id/edit` |
| Version | 1.1 |
| Status | Chapter content extracted verbatim; screen fields and scenarios derived from the source code |
| Date | 18 August 2026 |
| Parent document | WAR.IIROSA-Project-Documentation.docx v1.0 (repository WAR.IIROSA, commit 4beb736) |

## Contents of this document

1. Chapter 19 — module purpose and use-case catalogue (verbatim from the master document)
2. §19.S — screen field specifications (every field of every screen, derived from the AngularJS views)
3. §19.U — one expanded scenario per use case (trigger, pre-conditions, main flow, alternates, exceptions, post-conditions)
4. §19.A / §19.B — annexes: screens and Web API controllers of this module

## 19. Technical Support

الدعم الفني — a lightweight ticket register that lets charities raise issues and the office track their resolution.


| ID | Use case | Primary actor | Description & main flow | Realisation |
| --- | --- | --- | --- | --- |
| UC-CST-01 | List support tickets قائمة الدعم الفني | Gen. Director, Staff | Paged list of the logged support requests with their subject, requester and state. | Route `#/technical-support` → GET /api/SupportTickets/all-tickets |
| UC-CST-02 | Raise a support ticket اضافة دعم فني | Gen. Director, Staff | Records a new request — requester, charity, subject, description and date. | Route `#/technical-support/:id/edit` → POST /api/SupportTickets |
| UC-CST-03 | View a support ticket عرض الطلب | Gen. Director, Staff | Loads a single ticket by id. | GET /api/SupportTickets/{id} |
| UC-CST-04 | Update a support ticket تعديل الطلب | Gen. Director, Staff | Records progress, the response given and the closure of the request. | PUT /api/SupportTickets |
| UC-CST-05 | Delete a support ticket حذف الطلب | Gen. Director | Removes a ticket logged in error. | DELETE /api/SupportTickets |
| UC-CST-06 | Report on support tickets تقرير الدعم الفني | Gen. Director, Staff | Returns the reporting projection of the ticket register for export and printing. | GET /api/SupportTickets/report |

### 19.S  Screen field specifications

Derived from the AngularJS views of this module. For every screen the table lists each data-entry field in the order it appears, the scope property it binds to, its control type, whether the form marks it mandatory, and the lookup or rule that governs it. Labels are reproduced as they appear on screen (Arabic, right-to-left).

#### 19.S.1  Screen `#/technical-support`


| Property | Value |
| --- | --- |
| Angular route | `#/technical-support` |
| Feature module | `technical-support` (lazy-loaded) |
| Component | `TicketListComponent` |
| Route status | implemented |
| Data-entry fields | 0 |
| Grids on the screen | 1 |
| Commands | 6 |

Grids on this screen:

| Grid (row source) | Columns |
| --- | --- |
| project in CustomerSupports | الرقم · عنوان المشكله · وصف المشكله |

Commands on this screen:

| Command | Handler | Shown when |
| --- | --- | --- |
| حفظ | DeleteCustomerSupport() | always |
| (icon only) | ExtractAllData() | always |
| (icon only) | EditCustomerSupport(project.Id) | always |
| (icon only) | ViewDeleteModel(project.Id) | always |
| (icon only) | GetNext() | always |
| (icon only) | GetPrev() | always |

#### 19.S.2  Screen `#/technical-support/:id/edit`


| Property | Value |
| --- | --- |
| Angular route | `#/technical-support/:id/edit` |
| Feature module | `technical-support` (lazy-loaded) |
| Component | `TicketFormComponent` |
| Route status | implemented |
| Data-entry fields | 3 |
| Grids on the screen | 0 |
| Commands | 2 |


| Section | Field (as labelled) | Bound to | Control | Mandatory · source · rules |
| --- | --- | --- | --- | --- |
| إضافة مشكله دعم فني | وصف المشكله | CustomerSupport.Message | Text box | Mandatory |
| إضافة مشكله دعم فني | عنوان المشكله | CustomerSupport.Title | Text box | Mandatory |
| إضافة مشكله دعم فني | الملف | CustomerSupport.AttachedFile | File upload | Optional · accepts application/pdf |

Commands on this screen:

| Command | Handler | Shown when |
| --- | --- | --- |
| (icon only) | RemoveImage() | CustomerSupport.AttachedFile!=null |
| حفظ | SubmitAdd() | always |

### 19.U  Use case scenarios

One expanded scenario for every use case of this module. Pre-conditions, flows and post-conditions are derived from the screen that hosts the function, the Web API action that serves it and the business-layer method that executes it; quoted messages are the literal strings returned by the business layer.

#### 19.U.1  UC-CST-01 — List support tickets قائمة الدعم الفني


| Item | Specification |
| --- | --- |
| Use case ID | UC-CST-01 |
| Name | List support tickets قائمة الدعم الفني |
| Type | Browse a list |
| Primary actor | Gen. Director, Staff |
| Secondary actors | The system (Web API + business layer); the database. |
| Summary | Paged list of the logged support requests with their subject, requester and state. |
| Trigger | The actor opens the screen at `#/technical-support` from the module menu. |
| Pre-conditions | 1. The actor is authenticated; the JWT access token is valid and the client holds the role, charity and country claims it carries.<br>2. The actor holds one of: Gen. Director, Staff.<br>3. The SPA route `#/technical-support` has loaded and its reference-data lookups have been populated. |
| Main flow | 1. The actor navigates to the screen at `#/technical-support`.<br>2. The system loads the filter lookups the screen offers and, for HQ roles, the charity selector.<br>3. The SPA issues `GET /api/SupportTickets/all-tickets` carrying pagenum.<br>4. `SupportTicketsController` binds the typed request DTO and delegates to the application service.<br>5. `ISupportTicketService` validates the payload with its FluentValidation validator, applies the business rules and the charity scope, and persists through `IUnitOfWork`.<br>6. The result set is scoped to the caller’s charity and country, ordered and paged.<br>7. The SPA renders the page with the columns listed in the screen specification and the paging control. |
| Alternate flows | • No row matches the criteria — the grid renders empty and the paging control reports zero pages. |
| Exception flows | • The session has expired or the role is not permitted — the request is rejected and the SPA routes back to the login state. |
| Post-conditions | • No stored data is changed — the operation is a read. |
| Realisation | Route `#/technical-support` → `TicketListComponent`<br>`GET /api/SupportTickets/all-tickets` → `SupportTicketsController` → `ISupportTicketService` |

#### 19.U.2  UC-CST-02 — Raise a support ticket اضافة دعم فني


| Item | Specification |
| --- | --- |
| Use case ID | UC-CST-02 |
| Name | Raise a support ticket اضافة دعم فني |
| Type | Create a record |
| Primary actor | Gen. Director, Staff |
| Secondary actors | The system (Web API + business layer); the database. |
| Summary | Records a new request — requester, charity, subject, description and date. |
| Trigger | The actor presses «حفظ» on the screen AddCustomerSupport. |
| Pre-conditions | 1. The actor is authenticated; the JWT access token is valid and the client holds the role, charity and country claims it carries.<br>2. The actor holds one of: Gen. Director, Staff.<br>3. The SPA route `#/technical-support/:id/edit` has loaded and its reference-data lookups have been populated. |
| Main flow | 1. The actor navigates to the screen at `#/technical-support/:id/edit`. The screen opens in add mode.<br>2. The system loads the reference data the form needs (regions, centres, banks, types and the other lookup lists bound to the drop-downs).<br>3. The actor completes the input fields. Mandatory fields: وصف المشكله، عنوان المشكله.<br>4. The actor presses «حفظ» (SubmitAdd()).<br>5. The SPA issues `POST /api/SupportTickets` carrying CustomerSupportContract obj.<br>6. `SupportTicketsController` binds the typed request DTO and delegates to the application service.<br>7. `ISupportTicketService` validates the payload with its FluentValidation validator, applies the business rules and the charity scope, and persists through `IUnitOfWork`.<br>8. The business layer validates the payload, stamps the owning charity and the creating user, and persists the new record.<br>9. The system returns the outcome and the SPA confirms the save and returns to the list screen. |
| Alternate flows | • The actor abandons the form before saving — nothing is written and the record keeps its previous state. |
| Exception flows | • The session has expired or the role is not permitted — the request is rejected and the SPA routes back to the login state.<br>• A mandatory field is empty or fails its format check — the save is refused and the field is flagged on the form. |
| Post-conditions | • A new record exists, owned by the charity of the creating user, and appears in the list screen of the module. |
| Realisation | Route `#/technical-support/:id/edit` → `TicketFormComponent`<br>`POST /api/SupportTickets` → `SupportTicketsController` → `ISupportTicketService` |

#### 19.U.3  UC-CST-03 — View a support ticket عرض الطلب


| Item | Specification |
| --- | --- |
| Use case ID | UC-CST-03 |
| Name | View a support ticket عرض الطلب |
| Type | Read a record |
| Primary actor | Gen. Director, Staff |
| Secondary actors | The system (Web API + business layer); the database. |
| Summary | Loads a single ticket by id. |
| Trigger | The actor selects the function from the module menu or the screen that hosts it. |
| Pre-conditions | 1. The actor is authenticated; the JWT access token is valid and the client holds the role, charity and country claims it carries.<br>2. The actor holds one of: Gen. Director, Staff.<br>3. The target record exists and its identifier is known to the screen. |
| Main flow | 1. The actor reaches the function from the screen of this module that hosts it.<br>2. The actor selects the record to open.<br>3. The SPA issues `GET /api/SupportTickets/{id}` carrying id.<br>4. `SupportTicketsController` binds the typed request DTO and delegates to the application service.<br>5. `ISupportTicketService` validates the payload with its FluentValidation validator, applies the business rules and the charity scope, and persists through `IUnitOfWork`.<br>6. The system returns the record with its child collections and the SPA binds them to the form fields. |
| Alternate flows | None recorded. |
| Exception flows | • The session has expired or the role is not permitted — the request is rejected and the SPA routes back to the login state. |
| Post-conditions | • No stored data is changed — the operation is a read. |
| Realisation | `GET /api/SupportTickets/{id}` → `SupportTicketsController` → `ISupportTicketService` |

#### 19.U.4  UC-CST-04 — Update a support ticket تعديل الطلب


| Item | Specification |
| --- | --- |
| Use case ID | UC-CST-04 |
| Name | Update a support ticket تعديل الطلب |
| Type | Update a record |
| Primary actor | Gen. Director, Staff |
| Secondary actors | The system (Web API + business layer); the database. |
| Summary | Records progress, the response given and the closure of the request. |
| Trigger | The actor selects the function from the module menu or the screen that hosts it. |
| Pre-conditions | 1. The actor is authenticated; the JWT access token is valid and the client holds the role, charity and country claims it carries.<br>2. The actor holds one of: Gen. Director, Staff.<br>3. The target record exists and its identifier is known to the screen. |
| Main flow | 1. The actor reaches the function from the screen of this module that hosts it. The screen opens in edit mode on the selected record.<br>2. The system loads the current values into the form.<br>3. The actor changes the fields to be corrected.<br>4. The actor presses the command button of the screen.<br>5. The SPA issues `PUT /api/SupportTickets` carrying CustomerSupportContract obj.<br>6. `SupportTicketsController` binds the typed request DTO and delegates to the application service.<br>7. `ISupportTicketService` validates the payload with its FluentValidation validator, applies the business rules and the charity scope, and persists through `IUnitOfWork`.<br>8. The business layer re-validates the payload, applies the changes and persists them.<br>9. The system returns the outcome and the SPA refreshes the screen. |
| Alternate flows | • The actor abandons the form before saving — nothing is written and the record keeps its previous state. |
| Exception flows | • The session has expired or the role is not permitted — the request is rejected and the SPA routes back to the login state.<br>• A mandatory field is empty or fails its format check — the save is refused and the field is flagged on the form. |
| Post-conditions | • The stored record carries the new values; no other record is affected. |
| Realisation | `PUT /api/SupportTickets` → `SupportTicketsController` → `ISupportTicketService` |

#### 19.U.5  UC-CST-05 — Delete a support ticket حذف الطلب


| Item | Specification |
| --- | --- |
| Use case ID | UC-CST-05 |
| Name | Delete a support ticket حذف الطلب |
| Type | Delete a record |
| Primary actor | Gen. Director |
| Secondary actors | The system (Web API + business layer); the database. |
| Summary | Removes a ticket logged in error. |
| Trigger | The actor selects the function from the module menu or the screen that hosts it. |
| Pre-conditions | 1. The actor is authenticated; the JWT access token is valid and the client holds the role, charity and country claims it carries.<br>2. The actor holds one of: Gen. Director.<br>3. The target record exists and its identifier is known to the screen. |
| Main flow | 1. The actor reaches the function from the screen of this module that hosts it.<br>2. The actor selects the record and requests its deletion.<br>3. The SPA asks the actor to confirm.<br>4. The SPA issues `DELETE /api/SupportTickets` carrying id.<br>5. `SupportTicketsController` binds the typed request DTO and delegates to the application service.<br>6. `ISupportTicketService` validates the payload with its FluentValidation validator, applies the business rules and the charity scope, and persists through `IUnitOfWork`.<br>7. The business layer checks that the record may still be removed and deletes it (or marks it removed).<br>8. The system returns the outcome and the SPA drops the row from the grid. |
| Alternate flows | None recorded. |
| Exception flows | • The session has expired or the role is not permitted — the request is rejected and the SPA routes back to the login state. |
| Post-conditions | • The record is no longer returned by the list and read endpoints of the module. |
| Realisation | `DELETE /api/SupportTickets` → `SupportTicketsController` → `ISupportTicketService` |

#### 19.U.6  UC-CST-06 — Report on support tickets تقرير الدعم الفني


| Item | Specification |
| --- | --- |
| Use case ID | UC-CST-06 |
| Name | Report on support tickets تقرير الدعم الفني |
| Type | Query a report |
| Primary actor | Gen. Director, Staff |
| Secondary actors | The system (Web API + business layer); the database. |
| Summary | Returns the reporting projection of the ticket register for export and printing. |
| Trigger | The actor selects the function from the module menu or the screen that hosts it. |
| Pre-conditions | 1. The actor is authenticated; the JWT access token is valid and the client holds the role, charity and country claims it carries.<br>2. The actor holds one of: Gen. Director, Staff. |
| Main flow | 1. The actor reaches the function from the screen of this module that hosts it.<br>2. The actor sets the report criteria (period, charity, country and the other filters the screen offers).<br>3. The actor runs the report.<br>4. The SPA issues `GET /api/SupportTickets/report` carrying pagenum.<br>5. `SupportTicketsController` binds the typed request DTO and delegates to the application service.<br>6. `ISupportTicketService` validates the payload with its FluentValidation validator, applies the business rules and the charity scope, and persists through `IUnitOfWork`.<br>7. The business layer composes the projection, scoped to the caller’s charity and country.<br>8. The result is rendered in the on-screen grid, from where it can be printed or exported. |
| Alternate flows | • No row matches the criteria — the grid renders empty and the paging control reports zero pages. |
| Exception flows | • The session has expired or the role is not permitted — the request is rejected and the SPA routes back to the login state. |
| Post-conditions | • No stored data is changed — the operation is a read. |
| Realisation | `GET /api/SupportTickets/report` → `SupportTicketsController` → `ISupportTicketService` |

### 19.A  Annex - Angular routes of this module

Routes are hash-based (`useHash: true`), rendered inside `MainLayoutComponent` behind
`AuthGuard` and `PermissionGuard`. *implemented* means the route exists in
`Frontend/src/app`; *planned* means it is specified here and not yet built.

| Angular route | Feature module | Component | Status |
| --- | --- | --- | --- |
| `#/technical-support` | `technical-support` | `TicketListComponent` | implemented |
| `#/technical-support/:id/edit` | `technical-support` | `TicketFormComponent` | implemented |

### 19.B  Annex - API controllers of this module

Every controller inherits `ApiController` (`[Authorize]`, route `api/[controller]`) and
returns `Framework.Core.ApiResponse<T>`. Business rules live in the application service
behind each controller, never in the controller itself.

| Controller | Base route | Responsibility |
| --- | --- | --- |
| `SupportTicketsController` | `api/SupportTickets` | Support ticket CRUD and report projection. |

---

Shared context - system architecture, actors and roles, the sponsorship lifecycle, the full module map and all appendices - is in the companion document [00-Overview-and-Common-Context](00-Overview-and-Common-Context.md). The delivery backlog for this module is epic EP-14 in [00-EPICS-and-User-Stories](00-EPICS-and-User-Stories.md).

