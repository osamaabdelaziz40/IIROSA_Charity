# WAR.IIROSA - Home Dashboard

الصفحة الرئيسية | use case prefix `UC-DSH` | chapter 7 of the master document


| Field | Value |
| --- | --- |
| Document title | WAR.IIROSA - Module Specification: Home Dashboard |
| Module | Home Dashboard - الصفحة الرئيسية |
| Use case prefix | UC-DSH |
| Chapter in master document | Chapter 7 |
| Documented use cases | 3 |
| Principal routes | `#/dashboard` |
| Version | 1.1 |
| Status | Chapter content extracted verbatim; screen fields and scenarios derived from the source code |
| Date | 18 August 2026 |
| Parent document | WAR.IIROSA-Project-Documentation.docx v1.0 (repository WAR.IIROSA, commit 4beb736) |

## Contents of this document

1. Chapter 7 — module purpose and use-case catalogue (verbatim from the master document)
2. §7.S — screen field specifications (every field of every screen, derived from the AngularJS views)
3. §7.U — one expanded scenario per use case (trigger, pre-conditions, main flow, alternates, exceptions, post-conditions)
4. §7.A / §7.B — annexes: screens and Web API controllers of this module

## 7. Home Dashboard

الصفحة الرئيسية — the landing screen after login, giving each role an immediate view of the size and composition of its beneficiary population.


| ID | Use case | Primary actor | Description & main flow | Realisation |
| --- | --- | --- | --- | --- |
| UC-DSH-01 | View beneficiary family count عدد الأسر | All roles | On opening the home screen the system returns the number of family files visible to the caller — the charity's own families for a charity account, or the whole population for HQ roles. | Route `#/dashboard` → GET /api/Dashboard/summary |
| UC-DSH-02 | View statistical breakdown charts الرسوم البيانية | All roles | The dashboard renders pie charts of the beneficiary population. The caller selects a breakdown dimension (the parameter argument) and the system returns label/value pairs for that dimension within the caller's scope. | GET /api/Dashboard/charts |
| UC-DSH-03 | Navigate to a functional module التنقل بين الوحدات | All roles | From the collapsible side menu the user selects a module; the router activates the corresponding state and view. Menu entries the role may not use are not rendered. | ui-router state map in app.js |

### 7.S  Screen field specifications

Derived from the AngularJS views of this module. For every screen the table lists each data-entry field in the order it appears, the scope property it binds to, its control type, whether the form marks it mandatory, and the lookup or rule that governs it. Labels are reproduced as they appear on screen (Arabic, right-to-left).

#### 7.S.1  Screen `#/dashboard`


| Property | Value |
| --- | --- |
| Angular route | `#/dashboard` |
| Feature module | `dashboard` (lazy-loaded) |
| Component | `DashboardComponent` |
| Route status | implemented |
| Data-entry fields | 1 |
| Grids on the screen | 1 |
| Commands | 1 |


| Section | Field (as labelled) | Bound to | Control | Mandatory · source · rules |
| --- | --- | --- | --- | --- |
| — | الجمعية | CharityId | Drop-down list | Optional · options: lookup: Charities (+ كافة الجهات) · on change: getCharityData() |

Grids on this screen:

| Grid (row source) | Columns |
| --- | --- |
| report in Orphanreports track by $index | م · الكود · الاسم · السن · رقم التليفون · القريه · العنوان · الجمعية · الحاله |

Commands on this screen:

| Command | Handler | Shown when |
| --- | --- | --- |
| تحميل التقرير | ExportReport() | always |

#### 7.S.2  Screen `#/dashboard`


| Property | Value |
| --- | --- |
| Angular route | `#/dashboard` |
| Feature module | `dashboard` (lazy-loaded) |
| Component | `DashboardComponent` |
| Route status | implemented |
| Data-entry fields | 1 |
| Grids on the screen | 1 |
| Commands | 1 |


| Section | Field (as labelled) | Bound to | Control | Mandatory · source · rules |
| --- | --- | --- | --- | --- |
| — | الجمعية | CharityId | Drop-down list | Optional · options: lookup: Charities (+ كافة الجهات) · on change: getCharityData() |

Grids on this screen:

| Grid (row source) | Columns |
| --- | --- |
| report in Orphanreports track by $index | م · الكود · الاسم · السن · رقم التليفون · القريه · العنوان · الجمعية · الحاله |

Commands on this screen:

| Command | Handler | Shown when |
| --- | --- | --- |
| تحميل التقرير | ExportReport() | always |

### 7.U  Use case scenarios

One expanded scenario for every use case of this module. Pre-conditions, flows and post-conditions are derived from the screen that hosts the function, the Web API action that serves it and the business-layer method that executes it; quoted messages are the literal strings returned by the business layer.

#### 7.U.1  UC-DSH-01 — View beneficiary family count عدد الأسر


| Item | Specification |
| --- | --- |
| Use case ID | UC-DSH-01 |
| Name | View beneficiary family count عدد الأسر |
| Type | Read a record |
| Primary actor | All roles |
| Secondary actors | The system (Web API + business layer); the database. |
| Summary | On opening the home screen the system returns the number of family files visible to the caller — the charity's own families for a charity account, or the whole population for HQ roles. |
| Trigger | The actor opens the screen home (/) from the module menu. |
| Pre-conditions | 1. The actor is authenticated; the JWT access token is valid and the client holds the role, charity and country claims it carries.<br>2. The actor holds one of: All roles.<br>3. Caller identity comes from the JWT, never from the request; the application service scopes the query to the charity (and country) that owns the user, unless the role is an HQ role permitted to pass an explicit `charityId` filter.<br>4. The SPA route `#/dashboard` (route `/`) has loaded and its reference-data lookups have been populated.<br>5. The target record exists and its identifier is known to the screen. |
| Main flow | 1. The actor navigates to the screen at `#/dashboard` (route `/`).<br>2. The actor selects the record to open.<br>3. The SPA issues `GET /api/Dashboard/summary` carrying id, userId.<br>4. `DashboardController` binds the typed request DTO and delegates to the application service.<br>5. `IFamilyService` validates the payload with its FluentValidation validator, applies the business rules and the charity scope, and persists through `IUnitOfWork`.<br>6. The system returns the record with its child collections and the SPA binds them to the form fields. |
| Alternate flows | • An HQ role (General Director, Financial Director, Staff) may pass an explicit charity id and so read across the charity boundary; a charity user may not and always sees its own data. |
| Exception flows | • The session has expired or the role is not permitted — the request is rejected and the SPA routes back to the login state. |
| Post-conditions | • No stored data is changed — the operation is a read. |
| Realisation | Route `#/dashboard` → `DashboardComponent`<br>`GET /api/Dashboard/summary` → `DashboardController` → `IDashboardService` |

#### 7.U.2  UC-DSH-02 — View statistical breakdown charts الرسوم البيانية


| Item | Specification |
| --- | --- |
| Use case ID | UC-DSH-02 |
| Name | View statistical breakdown charts الرسوم البيانية |
| Type | Query a report |
| Primary actor | All roles |
| Secondary actors | The system (Web API + business layer); the database. |
| Summary | The dashboard renders pie charts of the beneficiary population. The caller selects a breakdown dimension (the parameter argument) and the system returns label/value pairs for that dimension within the caller's scope. |
| Trigger | The actor selects the function from the module menu or the screen that hosts it. |
| Pre-conditions | 1. The actor is authenticated; the JWT access token is valid and the client holds the role, charity and country claims it carries.<br>2. The actor holds one of: All roles.<br>3. Caller identity comes from the JWT, never from the request; the application service scopes the query to the charity (and country) that owns the user, unless the role is an HQ role permitted to pass an explicit `charityId` filter.<br>4. The record is not closed by an add/edit lock (IsLocked). |
| Main flow | 1. The actor reaches the function from the screen of this module that hosts it.<br>2. The actor sets the report criteria (period, charity, country and the other filters the screen offers).<br>3. The actor runs the report.<br>4. The SPA issues `GET /api/Dashboard/charts` carrying id, parameter, userId.<br>5. `DashboardController` binds the typed request DTO and delegates to the application service.<br>6. `IFamilyService` validates the payload with its FluentValidation validator, applies the business rules and the charity scope, and persists through `IUnitOfWork`.<br>7. The business layer composes the projection, scoped to the caller’s charity and country.<br>8. The result is rendered in the on-screen grid, from where it can be printed or exported. |
| Alternate flows | • An HQ role (General Director, Financial Director, Staff) may pass an explicit charity id and so read across the charity boundary; a charity user may not and always sees its own data.<br>• No row matches the criteria — the grid renders empty and the paging control reports zero pages. |
| Exception flows | • The session has expired or the role is not permitted — the request is rejected and the SPA routes back to the login state.<br>• The charity is closed by a lock (IsLocked) — the write is refused. |
| Post-conditions | • No stored data is changed — the operation is a read. |
| Realisation | `GET /api/Dashboard/charts` → `DashboardController` → `IDashboardService` |

#### 7.U.3  UC-DSH-03 — Navigate to a functional module التنقل بين الوحدات


| Item | Specification |
| --- | --- |
| Use case ID | UC-DSH-03 |
| Name | Navigate to a functional module التنقل بين الوحدات |
| Type | Read a record |
| Primary actor | All roles |
| Secondary actors | The system (Web API + business layer); the database. |
| Summary | From the collapsible side menu the user selects a module; the router activates the corresponding state and view. Menu entries the role may not use are not rendered. |
| Trigger | The actor selects the function from the module menu or the screen that hosts it. |
| Pre-conditions | 1. The actor is authenticated; the JWT access token is valid and the client holds the role, charity and country claims it carries.<br>2. The actor holds one of: All roles.<br>3. The target record exists and its identifier is known to the screen. |
| Main flow | 1. The actor reaches the function from the screen of this module that hosts it.<br>2. The actor selects the record to open.<br>3. The SPA calls the service endpoint that backs the function.<br>4. The Web API controller receives the request and delegates to the business layer.<br>5. The business layer executes the rules and the data access.<br>6. The system returns the record with its child collections and the SPA binds them to the form fields. |
| Alternate flows | None recorded. |
| Exception flows | • The session has expired or the role is not permitted — the request is rejected and the SPA routes back to the login state. |
| Post-conditions | • No stored data is changed — the operation is a read. |
| Realisation | ui-router state map in app.js |

### 7.A  Annex - Angular routes of this module

Routes are hash-based (`useHash: true`), rendered inside `MainLayoutComponent` behind
`AuthGuard` and `PermissionGuard`. *implemented* means the route exists in
`Frontend/src/app`; *planned* means it is specified here and not yet built.

| Angular route | Feature module | Component | Status |
| --- | --- | --- | --- |
| `#/` | `dashboard` | redirects to `#/dashboard` | implemented |
| `#/dashboard` | `dashboard` | `DashboardComponent` | implemented |
| `#/**` (unmatched) | `dashboard` | redirects to `#/dashboard` | implemented |

### 7.B  Annex - API controllers of this module

Every controller inherits `ApiController` (`[Authorize]`, route `api/[controller]`) and
returns `Framework.Core.ApiResponse<T>`. Business rules live in the application service
behind each controller, never in the controller itself.

| Controller | Base route | Responsibility |
| --- | --- | --- |


---

Shared context - system architecture, actors and roles, the sponsorship lifecycle, the full module map and all appendices - is in the companion document [00-Overview-and-Common-Context](00-Overview-and-Common-Context.md). The delivery backlog for this module is epic EP-02 in [00-EPICS-and-User-Stories](00-EPICS-and-User-Stories.md).

