# WAR.IIROSA - Employee & User Administration

ادارة الموظفين | use case prefix `UC-EMP` | chapter 9 of the master document


| Field | Value |
| --- | --- |
| Document title | WAR.IIROSA - Module Specification: Employee & User Administration |
| Module | Employee & User Administration - ادارة الموظفين |
| Use case prefix | UC-EMP |
| Chapter in master document | Chapter 9 |
| Documented use cases | 6 |
| Principal routes | `#/employees`, `#/employees/:id` |
| Version | 1.1 |
| Status | Chapter content extracted verbatim; screen fields and scenarios derived from the source code |
| Date | 18 August 2026 |
| Parent document | WAR.IIROSA-Project-Documentation.docx v1.0 (repository WAR.IIROSA, commit 4beb736) |

## Contents of this document

1. Chapter 9 — module purpose and use-case catalogue (verbatim from the master document)
2. §9.S — screen field specifications (every field of every screen, derived from the AngularJS views)
3. §9.U — one expanded scenario per use case (trigger, pre-conditions, main flow, alternates, exceptions, post-conditions)
4. §9.A / §9.B — annexes: screens and Web API controllers of this module

## 9. Employee & User Administration

إدارة الموظفين — creation and lifecycle of HQ staff accounts and their role assignment.


| ID | Use case | Primary actor | Description & main flow | Realisation |
| --- | --- | --- | --- | --- |
| UC-EMP-01 | List employees قائمة الموظفين | General Director | Shows all HQ employee accounts with their role, country and active/suspended state. | Route `#/employees` → GET /api/EmployeeManagement |
| UC-EMP-02 | Verify employee username availability التحقق من اسم المستخدم | General Director | Checks a proposed login name against existing accounts before the employee record is submitted. | GET /api/EmployeeManagement/check-national-id; also GET /api/UserManagement/check-username |
| UC-EMP-03 | Create an employee account اضافة موظف | General Director | Captures personal data, country, login credentials and the role to assign. The system creates the Identity user through `Framework.Identity`, adds them to the chosen role and stores the employee record. | Route `#/employees/:id` → POST /api/EmployeeManagement → `IEmployeeService` |
| UC-EMP-04 | View an employee record بيانات الموظف | General Director | Loads one employee by id for review or editing, including the currently assigned role. | GET /api/EmployeeManagement/{id} |
| UC-EMP-05 | Update an employee and change their role تعديل بيانات الموظف | General Director | Applies profile changes. When the role has changed, the system removes the user from the previous role and adds them to the new one, so the permissions take effect on the next sign-in. | PUT /api/EmployeeManagement/{id} → `IEmployeeService` (old-role / new-role swap) |
| UC-EMP-06 | Suspend or reactivate an employee إيقاف / تفعيل الموظف | General Director | Sets a stop flag on the employee, preventing sign-in without deleting the account or its audit history. | PATCH /api/EmployeeManagement/{id}/deactivate |

### 9.S  Screen field specifications

Derived from the AngularJS views of this module. For every screen the table lists each data-entry field in the order it appears, the scope property it binds to, its control type, whether the form marks it mandatory, and the lookup or rule that governs it. Labels are reproduced as they appear on screen (Arabic, right-to-left).

#### 9.S.1  Screen `#/employees`


| Property | Value |
| --- | --- |
| Angular route | `#/employees` |
| Feature module | `employees` (lazy-loaded) |
| Component | `EmployeeListComponent` |
| Route status | implemented |
| Data-entry fields | 3 |
| Grids on the screen | 1 |
| Commands | 5 |


| Section | Field (as labelled) | Bound to | Control | Mandatory · source · rules |
| --- | --- | --- | --- | --- |
| — | الجمعية | CharityId | Drop-down list | Optional · options: lookup: Charities (+ كافة الجهات) · on change: getCharityData() |
| تعديل الرقم السرى | * اعادة كتبة الرقم السرى | NewPassword | Password box | Optional |
| تعديل الرقم السرى | من فضلك ادخل كتابة كل البيانات | ConfirmNewPassword | Password box | Optional |

Grids on this screen:

| Grid (row source) | Columns |
| --- | --- |
| emp in Employees track by $index | الموظف · الصلاحية · البلد · تاريخ اخر دخول · إيقاف الموظف · تغيير كلمة السر · تعديل |

Commands on this screen:

| Command | Handler | Shown when |
| --- | --- | --- |
| غلق | EmptyChangepssword() | always |
| تم | ChangePasswordConfirmation() | always |
| (icon only) | StopEmployee($event,emp.Id) | always |
| (icon only) | ChangePassword($index) | always |
| (icon only) | EditEmployee(emp.Id) | always |

#### 9.S.2  Screen `#/employees/:id`


| Property | Value |
| --- | --- |
| Angular route | `#/employees/:id` |
| Feature module | `employees` (lazy-loaded) |
| Component | `EmployeeDetailComponent` |
| Route status | implemented |
| Data-entry fields | 5 |
| Grids on the screen | 0 |
| Commands | 1 |


| Section | Field (as labelled) | Bound to | Control | Mandatory · source · rules |
| --- | --- | --- | --- | --- |
| — | الجمعية | CharityId | Drop-down list | Optional · options: lookup: Charities (+ كافة الجهات) · on change: getCharityData() |
| — | الاسم بالكامل | Employee.FullName | Text box | Mandatory |
| — | الوظيفة | Employee.RoleId | Drop-down list | Mandatory · options: lookup: Roles |
| — | إسم المستخدم | Employee.UserName | Text box | Mandatory |
| — | كلمة المرور | Employee.Password | Password box | Mandatory |

Commands on this screen:

| Command | Handler | Shown when |
| --- | --- | --- |
| حفظ | AddEmployee() | always |

#### 9.S.3  Legacy dead routes, not carried forward

The legacy system registered two routes whose view file never existed. They are recorded here as a specified **non-goal**: the equivalent function is served by `#/user-management/users` and its child routes, and no dead route is carried into this project.


| Legacy dead route | Replaced by | Status |
| --- | --- | --- |
| `Users` | `#/user-management/users` | Not carried forward |
| `user` | `#/user-management/users/:id` | Not carried forward |

### 9.U  Use case scenarios

One expanded scenario for every use case of this module. Pre-conditions, flows and post-conditions are derived from the screen that hosts the function, the Web API action that serves it and the business-layer method that executes it; quoted messages are the literal strings returned by the business layer.

#### 9.U.1  UC-EMP-01 — List employees قائمة الموظفين


| Item | Specification |
| --- | --- |
| Use case ID | UC-EMP-01 |
| Name | List employees قائمة الموظفين |
| Type | Browse a list |
| Primary actor | General Director |
| Secondary actors | The system (Web API + business layer); the database. |
| Summary | Shows all HQ employee accounts with their role, country and active/suspended state. |
| Trigger | The actor opens the screen at `#/employees` from the module menu. |
| Pre-conditions | 1. The actor is authenticated; the JWT access token is valid and the client holds the role, charity and country claims it carries.<br>2. The actor holds one of: General Director.<br>3. Caller identity comes from the JWT, never from the request; the application service scopes the query to the charity (and country) that owns the user, unless the role is an HQ role permitted to pass an explicit `charityId` filter.<br>4. The SPA route `#/employees` has loaded and its reference-data lookups have been populated. |
| Main flow | 1. The actor navigates to the screen at `#/employees`.<br>2. The system loads the filter lookups the screen offers and, for HQ roles, the charity selector.<br>3. The SPA issues `GET /api/EmployeeManagement` carrying userId.<br>4. `EmployeeManagementController` binds the typed request DTO and delegates to the application service.<br>5. `IEmployeeService` validates the payload with its FluentValidation validator, applies the business rules and the charity scope, and persists through `IUnitOfWork`.<br>6. The result set is scoped to the caller’s charity and country, ordered and paged.<br>7. The SPA renders the page with the columns listed in the screen specification and the paging control. |
| Alternate flows | • An HQ role (General Director, Financial Director, Staff) may pass an explicit charity id and so read across the charity boundary; a charity user may not and always sees its own data.<br>• No row matches the criteria — the grid renders empty and the paging control reports zero pages. |
| Exception flows | • The session has expired or the role is not permitted — the request is rejected and the SPA routes back to the login state. |
| Post-conditions | • No stored data is changed — the operation is a read. |
| Realisation | Route `#/employees` → `EmployeeListComponent`<br>`GET /api/EmployeeManagement` → `EmployeeManagementController` → `IEmployeeService` |

#### 9.U.2  UC-EMP-02 — Verify employee username availability التحقق من اسم المستخدم


| Item | Specification |
| --- | --- |
| Use case ID | UC-EMP-02 |
| Name | Verify employee username availability التحقق من اسم المستخدم |
| Type | Authentication / credential handling |
| Primary actor | General Director |
| Secondary actors | The system (Web API + business layer); the database. |
| Summary | Checks a proposed login name against existing accounts before the employee record is submitted. |
| Trigger | The actor selects the function from the module menu or the screen that hosts it. |
| Pre-conditions | 1. The actor is authenticated; the JWT access token is valid and the client holds the role, charity and country claims it carries.<br>2. The actor holds one of: General Director. |
| Main flow | 1. The actor reaches the function from the screen of this module that hosts it.<br>2. The actor enters the credentials the screen asks for.<br>3. The actor presses the command button of the screen.<br>4. The SPA issues `GET /api/EmployeeManagement/check-national-id` carrying userName, Id.<br>5. `EmployeeManagementController` binds the typed request DTO and delegates to the application service.<br>6. `IEmployeeService` validates the payload with its FluentValidation validator, applies the business rules and the charity scope, and persists through `IUnitOfWork`.<br>7. The credentials are validated against ASP.NET Membership; the role GUID configured in Web.config is resolved to the internal role code and the country of the user is read.<br>8. On success a short-lived JWT access token and a refresh token are issued; the access token carries the user, role, charity and country claims, and the `auth` interceptor attaches it to every later call. |
| Alternate flows | None recorded. |
| Exception flows | • The credentials do not validate, or the account is locked out — loginStatus is returned false and the actor stays on the login screen. |
| Post-conditions | • An authenticated session exists (or has been ended); the role code and country id that scope every later request are held by the SPA. |
| Realisation | `GET /api/EmployeeManagement/check-national-id` · `GET /api/UserManagement/check-username` → `EmployeeManagementController` → `IEmployeeService` |

#### 9.U.3  UC-EMP-03 — Create an employee account اضافة موظف


| Item | Specification |
| --- | --- |
| Use case ID | UC-EMP-03 |
| Name | Create an employee account اضافة موظف |
| Type | Create a record |
| Primary actor | General Director |
| Secondary actors | The system (Web API + business layer); the database. |
| Summary | Captures personal data, country, login credentials and the role to assign. The system creates the membership user, resolves the chosen RoleId to its role name, adds the user to that role and stores the employee record. |
| Trigger | The actor presses «حفظ» on the screen employee. |
| Pre-conditions | 1. The actor is authenticated; the JWT access token is valid and the client holds the role, charity and country claims it carries.<br>2. The actor holds one of: General Director.<br>3. The SPA route `#/employees/:id` has loaded and its reference-data lookups have been populated. |
| Main flow | 1. The actor navigates to the screen at `#/employees/:id`. The screen opens in add mode.<br>2. The system loads the reference data the form needs (regions, centres, banks, types and the other lookup lists bound to the drop-downs).<br>3. The actor completes the input fields. Mandatory fields: الاسم بالكامل، الوظيفة، إسم المستخدم، كلمة المرور.<br>4. The actor presses «حفظ» (AddEmployee()).<br>5. The SPA issues `POST /api/EmployeeManagement` carrying [FromBody]EmployeeModel employee.<br>6. `EmployeeManagementController` binds the typed request DTO and delegates to the application service.<br>7. `IEmployeeService` validates the payload with its FluentValidation validator, applies the business rules and the charity scope, and persists through `IUnitOfWork`.<br>8. The business layer validates the payload, stamps the owning charity and the creating user, and persists the new record.<br>9. The system returns the outcome and the SPA confirms the save and returns to the list screen. |
| Alternate flows | • The actor abandons the form before saving — nothing is written and the record keeps its previous state. |
| Exception flows | • The session has expired or the role is not permitted — the request is rejected and the SPA routes back to the login state.<br>• A mandatory field is empty or fails its format check — the save is refused and the field is flagged on the form. |
| Post-conditions | • A new record exists, owned by the charity of the creating user, and appears in the list screen of the module. |
| Realisation | Route `#/employees/:id` → `EmployeeDetailComponent`<br>`POST /api/EmployeeManagement` → `EmployeeManagementController` → `IEmployeeService` |

#### 9.U.4  UC-EMP-04 — View an employee record بيانات الموظف


| Item | Specification |
| --- | --- |
| Use case ID | UC-EMP-04 |
| Name | View an employee record بيانات الموظف |
| Type | Read a record |
| Primary actor | General Director |
| Secondary actors | The system (Web API + business layer); the database. |
| Summary | Loads one employee by id for review or editing, including the currently assigned role. |
| Trigger | The actor selects the function from the module menu or the screen that hosts it. |
| Pre-conditions | 1. The actor is authenticated; the JWT access token is valid and the client holds the role, charity and country claims it carries.<br>2. The actor holds one of: General Director.<br>3. The target record exists and its identifier is known to the screen. |
| Main flow | 1. The actor reaches the function from the screen of this module that hosts it.<br>2. The actor selects the record to open.<br>3. The SPA issues `GET /api/EmployeeManagement/{id}` carrying Id.<br>4. `EmployeeManagementController` binds the typed request DTO and delegates to the application service.<br>5. `IEmployeeService` validates the payload with its FluentValidation validator, applies the business rules and the charity scope, and persists through `IUnitOfWork`.<br>6. The system returns the record with its child collections and the SPA binds them to the form fields. |
| Alternate flows | None recorded. |
| Exception flows | • The session has expired or the role is not permitted — the request is rejected and the SPA routes back to the login state. |
| Post-conditions | • No stored data is changed — the operation is a read. |
| Realisation | `GET /api/EmployeeManagement/{id}` → `EmployeeManagementController` → `IEmployeeService` |

#### 9.U.5  UC-EMP-05 — Update an employee and change their role تعديل بيانات الموظف


| Item | Specification |
| --- | --- |
| Use case ID | UC-EMP-05 |
| Name | Update an employee and change their role تعديل بيانات الموظف |
| Type | Update a record |
| Primary actor | General Director |
| Secondary actors | The system (Web API + business layer); the database. |
| Summary | Applies profile changes. When the role has changed, the system removes the user from the previous role and adds them to the new one, so the permissions take effect on the next sign-in. |
| Trigger | The actor selects the function from the module menu or the screen that hosts it. |
| Pre-conditions | 1. The actor is authenticated; the JWT access token is valid and the client holds the role, charity and country claims it carries.<br>2. The actor holds one of: General Director.<br>3. The target record exists and its identifier is known to the screen. |
| Main flow | 1. The actor reaches the function from the screen of this module that hosts it. The screen opens in edit mode on the selected record.<br>2. The system loads the current values into the form.<br>3. The actor changes the fields to be corrected.<br>4. The actor presses the command button of the screen.<br>5. The SPA issues `PUT /api/EmployeeManagement` carrying [FromBody]EmployeeModel employee.<br>6. `EmployeeManagementController` binds the typed request DTO and delegates to the application service.<br>7. `IEmployeeService` validates the payload with its FluentValidation validator, applies the business rules and the charity scope, and persists through `IUnitOfWork`.<br>8. The business layer re-validates the payload, applies the changes and persists them.<br>9. The system returns the outcome and the SPA refreshes the screen. |
| Alternate flows | • The actor abandons the form before saving — nothing is written and the record keeps its previous state. |
| Exception flows | • The session has expired or the role is not permitted — the request is rejected and the SPA routes back to the login state.<br>• A mandatory field is empty or fails its format check — the save is refused and the field is flagged on the form. |
| Post-conditions | • The stored record carries the new values; no other record is affected. |
| Realisation | `PUT /api/EmployeeManagement` → `EmployeeManagementController` → `IEmployeeService` |

#### 9.U.6  UC-EMP-06 — Suspend or reactivate an employee إيقاف / تفعيل الموظف


| Item | Specification |
| --- | --- |
| Use case ID | UC-EMP-06 |
| Name | Suspend or reactivate an employee إيقاف / تفعيل الموظف |
| Type | Read a record |
| Primary actor | General Director |
| Secondary actors | The system (Web API + business layer); the database. |
| Summary | Sets a stop flag on the employee, preventing sign-in without deleting the account or its audit history. |
| Trigger | The actor selects the function from the module menu or the screen that hosts it. |
| Pre-conditions | 1. The actor is authenticated; the JWT access token is valid and the client holds the role, charity and country claims it carries.<br>2. The actor holds one of: General Director.<br>3. Caller identity comes from the JWT, never from the request; the application service scopes the query to the charity (and country) that owns the user, unless the role is an HQ role permitted to pass an explicit `charityId` filter.<br>4. The target record exists and its identifier is known to the screen. |
| Main flow | 1. The actor reaches the function from the screen of this module that hosts it.<br>2. The actor selects the record to open.<br>3. The SPA issues `PATCH /api/EmployeeManagement/{id}/deactivate` carrying empId, value, userId.<br>4. `EmployeeManagementController` binds the typed request DTO and delegates to the application service.<br>5. `IEmployeeService` validates the payload with its FluentValidation validator, applies the business rules and the charity scope, and persists through `IUnitOfWork`.<br>6. The system returns the record with its child collections and the SPA binds them to the form fields. |
| Alternate flows | • An HQ role (General Director, Financial Director, Staff) may pass an explicit charity id and so read across the charity boundary; a charity user may not and always sees its own data. |
| Exception flows | • The session has expired or the role is not permitted — the request is rejected and the SPA routes back to the login state. |
| Post-conditions | • No stored data is changed — the operation is a read. |
| Realisation | `PATCH /api/EmployeeManagement/{id}/deactivate` → `EmployeeManagementController` → `IEmployeeService` |

### 9.A  Annex - Angular routes of this module

Routes are hash-based (`useHash: true`), rendered inside `MainLayoutComponent` behind
`AuthGuard` and `PermissionGuard`. *implemented* means the route exists in
`Frontend/src/app`; *planned* means it is specified here and not yet built.

| Angular route | Feature module | Component | Status |
| --- | --- | --- | --- |
| `#/user-management/users` | `user-management` | `UserListComponent` | implemented |
| `#/user-management/users/:id` | `user-management` | `UserDetailComponent` | implemented |
| `#/employees` | `employees` | `EmployeeListComponent` | implemented |
| `#/employees/:id` | `employees` | `EmployeeDetailComponent` | implemented |

### 9.B  Annex - API controllers of this module

Every controller inherits `ApiController` (`[Authorize]`, route `api/[controller]`) and
returns `Framework.Core.ApiResponse<T>`. Business rules live in the application service
behind each controller, never in the controller itself.

| Controller | Base route | Responsibility |
| --- | --- | --- |
| `UserManagementController` | `api/UserManagement` | Username availability. |
| `EmployeeManagementController` | `api/EmployeeManagement` | Employee list, create, update. Username check, employee read, suspend. |

---

Shared context - system architecture, actors and roles, the sponsorship lifecycle, the full module map and all appendices - is in the companion document [00-Overview-and-Common-Context](00-Overview-and-Common-Context.md). The delivery backlog for this module is epic EP-04 in [00-EPICS-and-User-Stories](00-EPICS-and-User-Stories.md).

