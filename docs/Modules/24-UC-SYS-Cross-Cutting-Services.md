# WAR.IIROSA - Cross-Cutting Services

الخدمات المشتركة | use case prefix `UC-SYS` | chapter 24 of the master document


| Field | Value |
| --- | --- |
| Document title | WAR.IIROSA - Module Specification: Cross-Cutting Services |
| Module | Cross-Cutting Services - الخدمات المشتركة |
| Use case prefix | UC-SYS |
| Chapter in master document | Chapter 24 |
| Documented use cases | 13 |
| Principal routes | none of its own — consumed by every feature module via `core/` and `shared/` |
| Version | 1.1 |
| Status | Chapter content extracted verbatim; screen fields and scenarios derived from the source code |
| Date | 18 August 2026 |
| Parent document | WAR.IIROSA-Project-Documentation.docx v1.0 (repository WAR.IIROSA, commit 4beb736) |

## Contents of this document

1. Chapter 24 — module purpose and use-case catalogue (verbatim from the master document)
2. §24.S — screen field specifications (every field of every screen, derived from the AngularJS views)
3. §24.U — one expanded scenario per use case (trigger, pre-conditions, main flow, alternates, exceptions, post-conditions)
4. §24.A / §24.B — annexes: screens and Web API controllers of this module

## 24. Cross-Cutting Services

الخدمات المشتركة — capabilities used by every module: attachment handling, reference data and validation.

### 24.1 Files and attachments


| ID | Use case | Primary actor | Description & main flow | Realisation |
| --- | --- | --- | --- | --- |
| UC-SYS-01 | Upload a file رفع ملف | Charity, HQ roles | Uploads a photograph or document as multipart content; the system stores it, classifies it by file type (project file or project report file) and returns the identifier that the calling record stores as its attachment reference. | POST /api/Attachments |
| UC-SYS-02 | Download a file تحميل ملف | All roles | Streams a stored attachment back to the browser by its identifier, with the appropriate content type. | GET /api/Attachments/{id}/download |
| UC-SYS-03 | Save a generated report file حفظ ملف التقرير | System | Persists a rendered report to the server file system so it can be retrieved or re-sent later. | /api/Attachments, /api/Attachments/{id}/download |

### 24.2 Reference data


| ID | Use case | Primary actor | Description & main flow | Realisation |
| --- | --- | --- | --- | --- |
| UC-SYS-04 | Load a generic lookup list القوائم المرجعية | All roles | Single parameterised endpoint returning any of the system's reference lists (educational level, stage, class, educational status, health status, disability, house type/status/ownership, income sources, relationships, prayer and memorisation levels, hobbies, behaviour scales, currencies, roles) by list id. | GET /api/LookupManagement |
| UC-SYS-05 | Load guardian lookup lists قوائم العائل | All roles | Returns the reference lists specific to the guardian record (marital status, relation to orphans, job categories). | GET /api/LookupManagement |
| UC-SYS-06 | Load refusal reasons أسباب الرفض | Gen. Director, Staff | Returns the catalogue of standard reasons, filtered by reason type, that a reviewer attaches when refusing a periodic report. | GET /api/LookupManagement/general-reasons; GET /api/LookupManagement/general-reasons |
| UC-SYS-07 | Select country, region and centre الدولة والمنطقة والمركز | All roles | Cascading geography selection: countries, then regions of the chosen country, then centres of the chosen region — used on the family, charity and employee forms. | GET /api/LookupManagement/countries GET /api/LookupManagement/regions GET /api/LookupManagement/centers/by-region/{regionId} |
| UC-SYS-08 | Select a bank اختيار البنك | Fin. Director, charity | Returns the banks configured for a country, used on cheque, transfer and bank-file screens. | GET /api/LookupManagement/countries |
| UC-SYS-09 | Select a job اختيار المهنة | Charity | Returns the occupation catalogue used on the guardian and family income sections. | GET /api/Reports |
| UC-SYS-10 | Select the orphan payment category فئة الصرف | HQ roles | Returns the orphan payment categories used when defining a batch's entitlement rules. | GET /api/OrphanPayments?orphanId= |

### 24.3 Validation


| ID | Use case | Primary actor | Description & main flow | Realisation |
| --- | --- | --- | --- | --- |
| UC-SYS-11 | Load country validation rules قواعد التحقق للدول | All roles | Returns per-country validation configuration — notably the national-ID format and length that the client applies to input masks and checks. | GET /api/LookupManagement/countries GET /api/LookupManagement/countries/{id} |
| UC-SYS-12 | Check a national ID is unique التحقق من الرقم القومي | Charity, HQ roles | Before a guardian or orphan is saved, the system checks the national ID against existing beneficiaries in the country. A second overload additionally excludes the family currently being edited, so re-saving an existing record does not report itself as a duplicate. Alternate: a clash returns the conflicting record's identity so the operator can investigate rather than silently creating a duplicate beneficiary. | GET /api/Families/check-national-id (+ &FamilyId overload) |
| UC-SYS-13 | Log and surface an error تسجيل الأخطاء | System | Domain failures are raised as `NotFoundException` / `ValidationException` / `BusinessException` and translated to the right status code by the API exception middleware, which logs the detail and returns `ApiResponse` with a safe message so internal detail is not exposed. The SPA routes unhandled failures to the error state. | Logger.LogError, log4net.config, route `#/error` |

### 24.S  Screen field specifications

Derived from the AngularJS views of this module. For every screen the table lists each data-entry field in the order it appears, the scope property it binds to, its control type, whether the form marks it mandatory, and the lookup or rule that governs it. Labels are reproduced as they appear on screen (Arabic, right-to-left).

### 24.U  Use case scenarios

One expanded scenario for every use case of this module. Pre-conditions, flows and post-conditions are derived from the screen that hosts the function, the Web API action that serves it and the business-layer method that executes it; quoted messages are the literal strings returned by the business layer.

#### 24.U.1  UC-SYS-01 — Upload a file رفع ملف


| Item | Specification |
| --- | --- |
| Use case ID | UC-SYS-01 |
| Name | Upload a file رفع ملف |
| Type | Import a file |
| Primary actor | Charity, HQ roles |
| Secondary actors | The system (Web API + business layer); the database. The uploaded workbook/CSV as the data source. |
| Summary | Uploads a photograph or document as multipart content; the system stores it, classifies it by file type (project file or project report file) and returns the identifier that the calling record stores as its attachment reference. |
| Trigger | The actor selects the function from the module menu or the screen that hosts it. |
| Pre-conditions | 1. The actor is authenticated; the JWT access token is valid and the client holds the role, charity and country claims it carries.<br>2. The actor holds one of: charity, HQ roles. |
| Main flow | 1. The actor reaches the function from the screen of this module that hosts it.<br>2. The actor chooses the file to upload in the file field of the screen.<br>3. The actor presses the command button of the screen.<br>4. The SPA posts the file as multipart content to `POST /api/Attachments`.<br>5. `AttachmentsController` binds the typed request DTO and delegates to the application service.<br>6. `IAttachmentService` validates the payload with its FluentValidation validator, applies the business rules and the charity scope, and persists through `IUnitOfWork`.<br>7. The business layer parses each row, matches it to the existing records by their key and applies the values it carries.<br>8. Rows that cannot be matched are reported back and the system returns the count applied. |
| Alternate flows | None recorded. |
| Exception flows | • The session has expired or the role is not permitted — the request is rejected and the SPA routes back to the login state.<br>• The uploaded file is not the expected workbook/CSV layout — the import is abandoned and no row is changed. |
| Post-conditions | • The matched records carry the imported values; unmatched rows are left untouched and reported. |
| Realisation | `POST /api/Attachments` → `AttachmentsController` → `IAttachmentService` |

#### 24.U.2  UC-SYS-02 — Download a file تحميل ملف


| Item | Specification |
| --- | --- |
| Use case ID | UC-SYS-02 |
| Name | Download a file تحميل ملف |
| Type | Browse a list |
| Primary actor | All roles |
| Secondary actors | The system (Web API + business layer); the database. |
| Summary | Streams a stored attachment back to the browser by its identifier, with the appropriate content type. |
| Trigger | The actor selects the function from the module menu or the screen that hosts it. |
| Pre-conditions | 1. The actor is authenticated; the JWT access token is valid and the client holds the role, charity and country claims it carries.<br>2. The actor holds one of: All roles. |
| Main flow | 1. The actor reaches the function from the screen of this module that hosts it.<br>2. The system loads the filter lookups the screen offers and, for HQ roles, the charity selector.<br>3. The SPA issues `GET /api/Attachments/{id}/download` carrying fileID.<br>4. `AttachmentsController` binds the typed request DTO and delegates to the application service.<br>5. `IAttachmentService` validates the payload with its FluentValidation validator, applies the business rules and the charity scope, and persists through `IUnitOfWork`.<br>6. The result set is scoped to the caller’s charity and country, ordered and paged.<br>7. The SPA renders the page and the paging control. |
| Alternate flows | • No row matches the criteria — the grid renders empty and the paging control reports zero pages. |
| Exception flows | • The session has expired or the role is not permitted — the request is rejected and the SPA routes back to the login state. |
| Post-conditions | • No stored data is changed — the operation is a read. |
| Realisation | `GET /api/Attachments/{id}/download` → `AttachmentsController` → `IAttachmentService` |

#### 24.U.3  UC-SYS-03 — Save a generated report file حفظ ملف التقرير


| Item | Specification |
| --- | --- |
| Use case ID | UC-SYS-03 |
| Name | Save a generated report file حفظ ملف التقرير |
| Type | Print / produce a document |
| Primary actor | System |
| Secondary actors | The system (Web API + business layer); the database. the report and export endpoints of `api/Reports` as the rendering path — jsPDF for PDF and ExcelJS for Excel. |
| Summary | Persists a rendered report to the server file system so it can be retrieved or re-sent later. |
| Trigger | The actor selects the function from the module menu or the screen that hosts it. |
| Pre-conditions | 1. The actor is authenticated; the JWT access token is valid and the client holds the role, charity and country claims it carries.<br>2. The actor holds one of: System.<br>3. The target record exists and its identifier is known to the screen. |
| Main flow | 1. The actor reaches the function from the screen of this module that hosts it.<br>2. The actor selects the record or the range to be printed.<br>3. The actor presses the command button of the screen.<br>4. The browser opens `GET /api/Attachments`.<br>5. `ReportsController` resolves the report key, applies the filter, and renders the document from the dataset the application service returns.<br>6. The rendered document is streamed back as a PDF for viewing and printing. |
| Alternate flows | None recorded. |
| Exception flows | • The session has expired or the role is not permitted — the request is rejected and the SPA routes back to the login state.<br>• The report definition or a required parameter is missing — the print action fails and no document is produced. |
| Post-conditions | • A printable document has been produced. Where the module records printing, the row is flagged as printed. |
| Realisation | `GET /api/Attachments` · `GET /api/Attachments/{id}/download` → `AttachmentsController` → `IAttachmentService` |

#### 24.U.4  UC-SYS-04 — Load a generic lookup list القوائم المرجعية


| Item | Specification |
| --- | --- |
| Use case ID | UC-SYS-04 |
| Name | Load a generic lookup list القوائم المرجعية |
| Type | Browse a list |
| Primary actor | All roles |
| Secondary actors | The system (Web API + business layer); the database. |
| Summary | Single parameterised endpoint returning any of the system's reference lists (educational level, stage, class, educational status, health status, disability, house type/status/ownership, income sources, relationships, prayer and memorisation levels, hobbies, behaviour scales, currencies, roles) by list id. |
| Trigger | The actor selects the function from the module menu or the screen that hosts it. |
| Pre-conditions | 1. The actor is authenticated; the JWT access token is valid and the client holds the role, charity and country claims it carries.<br>2. The actor holds one of: All roles.<br>3. Caller identity comes from the JWT, never from the request; the application service scopes the query to the charity (and country) that owns the user, unless the role is an HQ role permitted to pass an explicit `charityId` filter. |
| Main flow | 1. The actor reaches the function from the screen of this module that hosts it.<br>2. The system loads the filter lookups the screen offers and, for HQ roles, the charity selector.<br>3. The SPA issues `GET /api/LookupManagement` carrying id, userId.<br>4. `LookupManagementController` binds the typed request DTO and delegates to the application service.<br>5. `ICharityService` validates the payload with its FluentValidation validator, applies the business rules and the charity scope, and persists through `IUnitOfWork`.<br>6. The result set is scoped to the caller’s charity and country, ordered and paged.<br>7. The SPA renders the page and the paging control. |
| Alternate flows | • An HQ role (General Director, Financial Director, Staff) may pass an explicit charity id and so read across the charity boundary; a charity user may not and always sees its own data.<br>• No row matches the criteria — the grid renders empty and the paging control reports zero pages. |
| Exception flows | • The session has expired or the role is not permitted — the request is rejected and the SPA routes back to the login state. |
| Post-conditions | • No stored data is changed — the operation is a read. |
| Realisation | `GET /api/LookupManagement` → `LookupManagementController` → `ILookupService` |

#### 24.U.5  UC-SYS-05 — Load guardian lookup lists قوائم العائل


| Item | Specification |
| --- | --- |
| Use case ID | UC-SYS-05 |
| Name | Load guardian lookup lists قوائم العائل |
| Type | Browse a list |
| Primary actor | All roles |
| Secondary actors | The system (Web API + business layer); the database. |
| Summary | Returns the reference lists specific to the guardian record (marital status, relation to orphans, job categories). |
| Trigger | The actor selects the function from the module menu or the screen that hosts it. |
| Pre-conditions | 1. The actor is authenticated; the JWT access token is valid and the client holds the role, charity and country claims it carries.<br>2. The actor holds one of: All roles.<br>3. Caller identity comes from the JWT, never from the request; the application service scopes the query to the charity (and country) that owns the user, unless the role is an HQ role permitted to pass an explicit `charityId` filter. |
| Main flow | 1. The actor reaches the function from the screen of this module that hosts it.<br>2. The system loads the filter lookups the screen offers and, for HQ roles, the charity selector.<br>3. The SPA issues `GET /api/LookupManagement` carrying id, userId.<br>4. `LookupManagementController` binds the typed request DTO and delegates to the application service.<br>5. `IFamilyService` validates the payload with its FluentValidation validator, applies the business rules and the charity scope, and persists through `IUnitOfWork`.<br>6. The result set is scoped to the caller’s charity and country, ordered and paged.<br>7. The SPA renders the page and the paging control. |
| Alternate flows | • An HQ role (General Director, Financial Director, Staff) may pass an explicit charity id and so read across the charity boundary; a charity user may not and always sees its own data.<br>• No row matches the criteria — the grid renders empty and the paging control reports zero pages. |
| Exception flows | • The session has expired or the role is not permitted — the request is rejected and the SPA routes back to the login state. |
| Post-conditions | • No stored data is changed — the operation is a read. |
| Realisation | `GET /api/LookupManagement` → `LookupManagementController` → `ILookupService` |

#### 24.U.6  UC-SYS-06 — Load refusal reasons أسباب الرفض


| Item | Specification |
| --- | --- |
| Use case ID | UC-SYS-06 |
| Name | Load refusal reasons أسباب الرفض |
| Type | Review decision |
| Primary actor | Gen. Director, Staff |
| Secondary actors | The system (Web API + business layer); the database. |
| Summary | Returns the catalogue of standard reasons, filtered by reason type, that a reviewer attaches when refusing a periodic report. |
| Trigger | The actor selects the function from the module menu or the screen that hosts it. |
| Pre-conditions | 1. The actor is authenticated; the JWT access token is valid and the client holds the role, charity and country claims it carries.<br>2. The actor holds one of: Gen. Director, Staff.<br>3. The target record exists and its identifier is known to the screen. |
| Main flow | 1. The actor reaches the function from the screen of this module that hosts it.<br>2. The actor opens the item awaiting a decision and examines its content and attachments.<br>3. The actor records the decision and, when refusing, the reason.<br>4. The actor presses the command button of the screen.<br>5. The SPA issues `GET /api/LookupManagement/general-reasons` carrying Type.<br>6. `LookupManagementController` binds the typed request DTO and delegates to the application service.<br>7. `ILookupService` validates the payload with its FluentValidation validator, applies the business rules and the charity scope, and persists through `IUnitOfWork`.<br>8. The business layer writes the new state, the deciding user and the decision date.<br>9. The item leaves the pending queue and becomes visible to the charity in its new state. |
| Alternate flows | • The decision is a refusal — the reason is mandatory and is stored with the item so that the charity can see why it was returned. |
| Exception flows | • The session has expired or the role is not permitted — the request is rejected and the SPA routes back to the login state. |
| Post-conditions | • The item carries its new state, the deciding user and the decision date, and moves out of the pending queue. |
| Realisation | `GET /api/LookupManagement/general-reasons` → `LookupManagementController` → `ILookupService` |

#### 24.U.7  UC-SYS-07 — Select country, region and centre الدولة والمنطقة والمركز


| Item | Specification |
| --- | --- |
| Use case ID | UC-SYS-07 |
| Name | Select country, region and centre الدولة والمنطقة والمركز |
| Type | Read a record |
| Primary actor | All roles |
| Secondary actors | The system (Web API + business layer); the database. |
| Summary | Cascading geography selection: countries, then regions of the chosen country, then centres of the chosen region — used on the family, charity and employee forms. |
| Trigger | The actor selects the function from the module menu or the screen that hosts it. |
| Pre-conditions | 1. The actor is authenticated; the JWT access token is valid and the client holds the role, charity and country claims it carries.<br>2. The actor holds one of: All roles.<br>3. Caller identity comes from the JWT, never from the request; the application service scopes the query to the charity (and country) that owns the user, unless the role is an HQ role permitted to pass an explicit `charityId` filter.<br>4. The target record exists and its identifier is known to the screen. |
| Main flow | 1. The actor reaches the function from the screen of this module that hosts it.<br>2. The actor selects the record to open.<br>3. The SPA issues `GET /api/LookupManagement/countries` carrying userId.<br>4. `LookupManagementController` binds the typed request DTO and delegates to the application service.<br>5. `ILookupService` validates the payload with its FluentValidation validator, applies the business rules and the charity scope, and persists through `IUnitOfWork`.<br>6. The system returns the record with its child collections and the SPA binds them to the form fields. |
| Alternate flows | • An HQ role (General Director, Financial Director, Staff) may pass an explicit charity id and so read across the charity boundary; a charity user may not and always sees its own data. |
| Exception flows | • The session has expired or the role is not permitted — the request is rejected and the SPA routes back to the login state. |
| Post-conditions | • No stored data is changed — the operation is a read. |
| Realisation | `GET /api/LookupManagement/countries` · `GET /api/LookupManagement/regions` · `GET /api/LookupManagement/centers/by-region/{regionId}` → `LookupManagementController` → `ILookupService` |

#### 24.U.8  UC-SYS-08 — Select a bank اختيار البنك


| Item | Specification |
| --- | --- |
| Use case ID | UC-SYS-08 |
| Name | Select a bank اختيار البنك |
| Type | Read a record |
| Primary actor | Fin. Director, charity |
| Secondary actors | The system (Web API + business layer); the database. |
| Summary | Returns the banks configured for a country, used on cheque, transfer and bank-file screens. |
| Trigger | The actor selects the function from the module menu or the screen that hosts it. |
| Pre-conditions | 1. The actor is authenticated; the JWT access token is valid and the client holds the role, charity and country claims it carries.<br>2. The actor holds one of: Fin. Director, charity.<br>3. Caller identity comes from the JWT, never from the request; the application service scopes the query to the charity (and country) that owns the user, unless the role is an HQ role permitted to pass an explicit `charityId` filter.<br>4. The target record exists and its identifier is known to the screen. |
| Main flow | 1. The actor reaches the function from the screen of this module that hosts it.<br>2. The actor selects the record to open.<br>3. The SPA issues `GET /api/LookupManagement/countries` carrying id, userId.<br>4. `LookupManagementController` binds the typed request DTO and delegates to the application service.<br>5. `ILookupService` validates the payload with its FluentValidation validator, applies the business rules and the charity scope, and persists through `IUnitOfWork`.<br>6. The system returns the record with its child collections and the SPA binds them to the form fields. |
| Alternate flows | • An HQ role (General Director, Financial Director, Staff) may pass an explicit charity id and so read across the charity boundary; a charity user may not and always sees its own data. |
| Exception flows | • The session has expired or the role is not permitted — the request is rejected and the SPA routes back to the login state. |
| Post-conditions | • No stored data is changed — the operation is a read. |
| Realisation | `GET /api/LookupManagement/countries` → `LookupManagementController` → `ILookupService` |

#### 24.U.9  UC-SYS-09 — Select a job اختيار المهنة


| Item | Specification |
| --- | --- |
| Use case ID | UC-SYS-09 |
| Name | Select a job اختيار المهنة |
| Type | Read a record |
| Primary actor | Charity |
| Secondary actors | The system (Web API + business layer); the database. |
| Summary | Returns the occupation catalogue used on the guardian and family income sections. |
| Trigger | The actor selects the function from the module menu or the screen that hosts it. |
| Pre-conditions | 1. The actor is authenticated; the JWT access token is valid and the client holds the role, charity and country claims it carries.<br>2. The actor holds one of: charity.<br>3. Caller identity comes from the JWT, never from the request; the application service scopes the query to the charity (and country) that owns the user, unless the role is an HQ role permitted to pass an explicit `charityId` filter.<br>4. The target record exists and its identifier is known to the screen. |
| Main flow | 1. The actor reaches the function from the screen of this module that hosts it.<br>2. The actor selects the record to open.<br>3. The SPA issues `GET /api/Reports` carrying userId.<br>4. `ReportsController` binds the typed request DTO and delegates to the application service.<br>5. `ILookupService` validates the payload with its FluentValidation validator, applies the business rules and the charity scope, and persists through `IUnitOfWork`.<br>6. The system returns the record with its child collections and the SPA binds them to the form fields. |
| Alternate flows | • An HQ role (General Director, Financial Director, Staff) may pass an explicit charity id and so read across the charity boundary; a charity user may not and always sees its own data. |
| Exception flows | • The session has expired or the role is not permitted — the request is rejected and the SPA routes back to the login state. |
| Post-conditions | • No stored data is changed — the operation is a read. |
| Realisation | `GET /api/Reports` → `ReportsController` → `IReportService` |

#### 24.U.10  UC-SYS-10 — Select the orphan payment category فئة الصرف


| Item | Specification |
| --- | --- |
| Use case ID | UC-SYS-10 |
| Name | Select the orphan payment category فئة الصرف |
| Type | Read a record |
| Primary actor | HQ roles |
| Secondary actors | The system (Web API + business layer); the database. |
| Summary | Returns the orphan payment categories used when defining a batch's entitlement rules. |
| Trigger | The actor selects the function from the module menu or the screen that hosts it. |
| Pre-conditions | 1. The actor is authenticated; the JWT access token is valid and the client holds the role, charity and country claims it carries.<br>2. The actor holds one of: HQ roles.<br>3. Caller identity comes from the JWT, never from the request; the application service scopes the query to the charity (and country) that owns the user, unless the role is an HQ role permitted to pass an explicit `charityId` filter.<br>4. The target record exists and its identifier is known to the screen. |
| Main flow | 1. The actor reaches the function from the screen of this module that hosts it.<br>2. The actor selects the record to open.<br>3. The SPA issues `GET /api/OrphanPayments?orphanId=` carrying userId.<br>4. `FamiliesController` binds the typed request DTO and delegates to the application service.<br>5. `IOrphanService` validates the payload with its FluentValidation validator, applies the business rules and the charity scope, and persists through `IUnitOfWork`.<br>6. The system returns the record with its child collections and the SPA binds them to the form fields. |
| Alternate flows | • An HQ role (General Director, Financial Director, Staff) may pass an explicit charity id and so read across the charity boundary; a charity user may not and always sees its own data. |
| Exception flows | • The session has expired or the role is not permitted — the request is rejected and the SPA routes back to the login state. |
| Post-conditions | • No stored data is changed — the operation is a read. |
| Realisation | `GET /api/OrphanPayments?orphanId=` → `OrphanPaymentsController` → `IOrphanPaymentService` |

#### 24.U.11  UC-SYS-11 — Load country validation rules قواعد التحقق للدول


| Item | Specification |
| --- | --- |
| Use case ID | UC-SYS-11 |
| Name | Load country validation rules قواعد التحقق للدول |
| Type | Read a record |
| Primary actor | All roles |
| Secondary actors | The system (Web API + business layer); the database. |
| Summary | Returns per-country validation configuration — notably the national-ID format and length that the client applies to input masks and checks. |
| Trigger | The actor selects the function from the module menu or the screen that hosts it. |
| Pre-conditions | 1. The actor is authenticated; the JWT access token is valid and the client holds the role, charity and country claims it carries.<br>2. The actor holds one of: All roles.<br>3. Caller identity comes from the JWT, never from the request; the application service scopes the query to the charity (and country) that owns the user, unless the role is an HQ role permitted to pass an explicit `charityId` filter.<br>4. The target record exists and its identifier is known to the screen. |
| Main flow | 1. The actor reaches the function from the screen of this module that hosts it.<br>2. The actor selects the record to open.<br>3. The SPA issues `GET /api/LookupManagement/countries` carrying userId.<br>4. `LookupManagementController` binds the typed request DTO and delegates to the application service.<br>5. `ILookupService` validates the payload with its FluentValidation validator, applies the business rules and the charity scope, and persists through `IUnitOfWork`.<br>6. The system returns the record with its child collections and the SPA binds them to the form fields. |
| Alternate flows | • An HQ role (General Director, Financial Director, Staff) may pass an explicit charity id and so read across the charity boundary; a charity user may not and always sees its own data. |
| Exception flows | • The session has expired or the role is not permitted — the request is rejected and the SPA routes back to the login state. |
| Post-conditions | • No stored data is changed — the operation is a read. |
| Realisation | `GET /api/LookupManagement/countries` · `GET /api/LookupManagement/countries/{id}` → `LookupManagementController` → `ILookupService` |

#### 24.U.12  UC-SYS-12 — Check a national ID is unique التحقق من الرقم القومي


| Item | Specification |
| --- | --- |
| Use case ID | UC-SYS-12 |
| Name | Check a national ID is unique التحقق من الرقم القومي |
| Type | Update a record |
| Primary actor | Charity, HQ roles |
| Secondary actors | The system (Web API + business layer); the database. |
| Summary | Before a guardian or orphan is saved, the system checks the national ID against existing beneficiaries in the country. A second overload additionally excludes the family currently being edited, so re-saving an existing record does not report itself as a duplicate. Alternate: a clash returns the conflicting record's identity so the operator can investigate rather than silently creating a duplicate beneficiary. |
| Trigger | The actor selects the function from the module menu or the screen that hosts it. |
| Pre-conditions | 1. The actor is authenticated; the JWT access token is valid and the client holds the role, charity and country claims it carries.<br>2. The actor holds one of: charity, HQ roles.<br>3. Caller identity comes from the JWT, never from the request; the application service scopes the query to the charity (and country) that owns the user, unless the role is an HQ role permitted to pass an explicit `charityId` filter.<br>4. The target record exists and its identifier is known to the screen. |
| Main flow | 1. The actor reaches the function from the screen of this module that hosts it. The screen opens in edit mode on the selected record.<br>2. The system loads the current values into the form.<br>3. The actor changes the fields to be corrected.<br>4. The actor presses the command button of the screen.<br>5. The SPA issues `GET /api/Families/check-national-id` carrying nid, name, cid, uid, charityId.<br>6. `LookupManagementController` binds the typed request DTO and delegates to the application service.<br>7. `IFamilyService` validates the payload with its FluentValidation validator, applies the business rules and the charity scope, and persists through `IUnitOfWork`.<br>8. The business layer re-validates the payload, applies the changes and persists them.<br>9. The system returns the outcome and the SPA refreshes the screen. |
| Alternate flows | • An HQ role (General Director, Financial Director, Staff) may pass an explicit charity id and so read across the charity boundary; a charity user may not and always sees its own data.<br>• The actor abandons the form before saving — nothing is written and the record keeps its previous state. |
| Exception flows | • The session has expired or the role is not permitted — the request is rejected and the SPA routes back to the login state.<br>• A mandatory field is empty or fails its format check — the save is refused and the field is flagged on the form. |
| Post-conditions | • The stored record carries the new values; no other record is affected. |
| Realisation | `GET /api/Families/check-national-id` → `FamiliesController` → `IFamilyService` |

#### 24.U.13  UC-SYS-13 — Log and surface an error تسجيل الأخطاء


| Item | Specification |
| --- | --- |
| Use case ID | UC-SYS-13 |
| Name | Log and surface an error تسجيل الأخطاء |
| Type | Create a record |
| Primary actor | System |
| Secondary actors | The system (Web API + business layer); the database. |
| Summary | Domain failures are raised as `NotFoundException` / `ValidationException` / `BusinessException` and translated to the right status code by the API exception middleware, which logs the detail and returns `ApiResponse` with a safe message so internal detail is not exposed. The SPA routes unhandled failures to the error state. |
| Trigger | The actor selects the function from the module menu or the screen that hosts it. |
| Pre-conditions | 1. The actor is authenticated; the JWT access token is valid and the client holds the role, charity and country claims it carries.<br>2. The actor holds one of: System. |
| Main flow | 1. The actor reaches the function from the screen of this module that hosts it. The screen opens in add mode.<br>2. The system loads the reference data the form needs (regions, centres, banks, types and the other lookup lists bound to the drop-downs).<br>3. The actor completes the input fields of the form.<br>4. The actor presses the command button of the screen.<br>5. The SPA calls the service endpoint that backs the function.<br>6. The Web API controller receives the request and delegates to the business layer.<br>7. The business layer executes the rules and the data access.<br>8. The business layer validates the payload, stamps the owning charity and the creating user, and persists the new record.<br>9. The system returns the outcome and the SPA confirms the save and returns to the list screen. |
| Alternate flows | • The actor abandons the form before saving — nothing is written and the record keeps its previous state. |
| Exception flows | • The session has expired or the role is not permitted — the request is rejected and the SPA routes back to the login state.<br>• A mandatory field is empty or fails its format check — the save is refused and the field is flagged on the form. |
| Post-conditions | • A new record exists, owned by the charity of the creating user, and appears in the list screen of the module. |
| Realisation | Logger.LogError, log4net.config, route `#/error` |

### 24.A  Annex - Angular routes of this module

Routes are hash-based (`useHash: true`), rendered inside `MainLayoutComponent` behind
`AuthGuard` and `PermissionGuard`. *implemented* means the route exists in
`Frontend/src/app`; *planned* means it is specified here and not yet built.

| Angular route | Feature module | Component | Status |
| --- | --- | --- | --- |
| `#/error` | `core` | `ErrorPageComponent` | planned |

### 24.B  Annex - API controllers of this module

Every controller inherits `ApiController` (`[Authorize]`, route `api/[controller]`) and
returns `Framework.Core.ApiResponse<T>`. Business rules live in the application service
behind each controller, never in the controller itself.

| Controller | Base route | Responsibility |
| --- | --- | --- |
| `LookupManagementController` | `api/LookupManagement` | Guardian reference lists. Countries and banks; received-payment detail. Regions and centres. Generic reference lists, refusal reasons, housing buildings and flats. Reason catalogue. Country validation rules; national-ID uniqueness. |
| `OrphanPaymentsController` | `api/OrphanPayments` | Reference lists; phone duplicate check. |
| `ReportsController` | `api/Reports` | Job catalogue; not-received, stopped and other-sponsor reports. |
| `AttachmentsController` | `api/Attachments` | Attachment upload and download. |

---

Shared context - system architecture, actors and roles, the sponsorship lifecycle, the full module map and all appendices - is in the companion document [00-Overview-and-Common-Context](00-Overview-and-Common-Context.md). The delivery backlog for this module is epic EP-19 in [00-EPICS-and-User-Stories](00-EPICS-and-User-Stories.md).

