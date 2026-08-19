# WAR.IIROSA - General Cheques

الشيكات العامة | use case prefix `UC-CHQ` | chapter 16 of the master document


| Field | Value |
| --- | --- |
| Document title | WAR.IIROSA - Module Specification: General Cheques |
| Module | General Cheques - الشيكات العامة |
| Use case prefix | UC-CHQ |
| Chapter in master document | Chapter 16 |
| Documented use cases | 10 |
| Principal routes | `#/general-checks`, `#/general-checks/create`, `#/general-checks/edit/:id`, `#/general-checks/:id`, `#/general-checks/reconcile` |
| Version | 1.1 |
| Status | Chapter content extracted verbatim; screen fields and scenarios derived from the source code |
| Date | 18 August 2026 |
| Parent document | WAR.IIROSA-Project-Documentation.docx v1.0 (repository WAR.IIROSA, commit 4beb736) |

## Contents of this document

1. Chapter 16 — module purpose and use-case catalogue (verbatim from the master document)
2. §16.S — screen field specifications (every field of every screen, derived from the AngularJS views)
3. §16.U — one expanded scenario per use case (trigger, pre-conditions, main flow, alternates, exceptions, post-conditions)
4. §16.A / §16.B — annexes: screens and Web API controllers of this module

## 16. General Cheques

الشيكات العامة — cheque issuance outside the orphan payment cycle: to suppliers, charities, project beneficiaries and staff. The module handles the register, the Arabic amount-in-words conversion and the bank-specific print alignment.


| ID | Use case | Primary actor | Description & main flow | Realisation |
| --- | --- | --- | --- | --- |
| UC-CHQ-01 | List cheques قائمة الشيكات | Fin. Director, Gen. Director | Paged register of all issued cheques with beneficiary, bank, amount, currency, date and status. | Route `#/general-checks` → GET /api/CheckManagement |
| UC-CHQ-02 | Issue a cheque اضافة شيك | Fin. Director | Captures beneficiary, bank, account, amount, currency and date, converts the amount to Arabic words for the printed cheque, and stores the record. | Route `#/general-checks/create` → POST /api/CheckManagement |
| UC-CHQ-03 | View a cheque عرض الشيك | Fin. Director, Gen. Director | Loads a single cheque for review or editing. | GET /api/CheckManagement/{id} |
| UC-CHQ-04 | Update a cheque تعديل الشيك | Fin. Director | Amends an issued cheque's data before printing or clearing. | PUT /api/CheckManagement |
| UC-CHQ-05 | Select a beneficiary اختيار المستفيد | Fin. Director | Type-ahead search over the beneficiary catalogue (charities, suppliers, individuals) to attach the payee to the cheque. | GET /api/LookupManagement/cheque-beneficiaries |
| UC-CHQ-06 | Select the currency اختيار العملة | Fin. Director | Loads the currency list that drives the amount and the words conversion. | GET /api/LookupManagement/currencies |
| UC-CHQ-07 | Convert an amount to Arabic words تفقيط المبلغ | System | Converts the numeric cheque amount into its Arabic written form, which is printed on the cheque face and stored with the record. | GET /api/CheckManagement/amount-in-words |
| UC-CHQ-08 | Load bank cheque print positions مواضع الطباعة على الشيك | Fin. Director | Retrieves the coordinate offsets configured for the selected bank's cheque stationery so the printed fields align with the pre-printed form. | GET /api/LookupManagement/banks/{id}/cheque-positions; GET /api/LookupManagement/countries/{id} for the bank list |
| UC-CHQ-09 | Produce a cheque statement بيان الشيكات | Fin. Director, Gen. Director | Filters the cheque register (bank, date range, status, beneficiary) and returns the statement used for bank reconciliation. | Route `#/general-checks/reconcile` → GET /api/CheckManagement/report |
| UC-CHQ-10 | Print a cheque and the cheque report طباعة الشيك والتقرير | Fin. Director | Renders the cheque onto the bank's stationery using the configured positions, with a country-specific variant, and produces the cheque report for a bank, date range and cheque type. | /api/Reports/general-cheque/export/pdf, /api/Reports/general-cheque-eg/export/pdf, /api/Reports/cheque-statement/export/pdf, /api/Reports/payment-cheques/export/pdf, /api/Reports/payment-cheques-eg/export/pdf |

### 16.S  Screen field specifications

Derived from the AngularJS views of this module. For every screen the table lists each data-entry field in the order it appears, the scope property it binds to, its control type, whether the form marks it mandatory, and the lookup or rule that governs it. Labels are reproduced as they appear on screen (Arabic, right-to-left).

#### 16.S.1  Screen `#/general-checks`


| Property | Value |
| --- | --- |
| Angular route | `#/general-checks` |
| Feature module | `general-checks` (lazy-loaded) |
| Component | `CheckListComponent` |
| Route status | implemented |
| Data-entry fields | 1 |
| Grids on the screen | 1 |
| Commands | 4 |


| Section | Field (as labelled) | Bound to | Control | Mandatory · source · rules |
| --- | --- | --- | --- | --- |
| — | الجمعية | CharityId | Drop-down list | Optional · options: lookup: Charities (+ كافة الجهات) · on change: getCharityData() |

Grids on this screen:

| Grid (row source) | Columns |
| --- | --- |
| Cheque in Cheques track by $index | الرقم · رقم الشيك · تاريخ الشيك · اسم المستفيد · المبلغ · البنك · تعديل |

Commands on this screen:

| Command | Handler | Shown when |
| --- | --- | --- |
| (icon only) | AddCheck() | always |
| (icon only) | EditCheck(Cheque.Id) | always |
| (icon only) | GetNext() | always |
| (icon only) | GetPrev() | always |

#### 16.S.2  Screen `#/general-checks/edit/:id`


| Property | Value |
| --- | --- |
| Angular route | `#/general-checks/edit/:id` |
| Feature module | `general-checks` (lazy-loaded) |
| Component | `CheckFormComponent` |
| Route status | implemented |
| Data-entry fields | 12 |
| Grids on the screen | 0 |
| Commands | 3 |


| Section | Field (as labelled) | Bound to | Control | Mandatory · source · rules |
| --- | --- | --- | --- | --- |
| — | الجمعية | CharityId | Drop-down list | Optional · options: lookup: Charities (+ كافة الجهات) · on change: getCharityData() |
| إضافة شيك | البنك | Bank | Drop-down list | Mandatory · options: lookup: Banks |
| إضافة شيك | اسم المستفيد | BeneficiaryName | Text box | Mandatory |
| إضافة شيك | تاريخ الشيك | checkDate | Date picker | Mandatory |
| إضافة شيك | رقم الشيك | CheckNum | Text box | Mandatory |
| إضافة شيك | العملة | Currency | Drop-down list | Mandatory · options: lookup: Currencies |
| إضافة شيك | المبلغ | CheckAmount | Text box | Mandatory |
| إضافة شيك | تعليقات | CheckComment | Text box | Optional |
| إضافة شيك | شيك تالف | FirstBeneficiaryOnly | Check box | Optional |
| إضافة شيك | تم رد الشيك | CheckHarmony | Check box | Optional |
| إضافة شيك | تم الصرف | CheckAnswered | Check box | Optional |
| إضافة شيك | (unlabelled) | CheckDone | Check box | Optional |

Commands on this screen:

| Command | Handler | Shown when |
| --- | --- | --- |
| طباعة | PrintCheck() | PrintShow |
| طباعة مصري | PrintCheckEgypt() | PrintShow |
| حفظ | AddCheck() | SaveShow |

#### 16.S.3  Screen `#/general-checks/statement`


| Property | Value |
| --- | --- |
| Angular route | `#/general-checks/statement` |
| Feature module | `general-checks` (lazy-loaded) |
| Component | `CheckStatementComponent` |
| Route status | planned |
| Data-entry fields | 6 |
| Grids on the screen | 1 |
| Commands | 2 |


| Section | Field (as labelled) | Bound to | Control | Mandatory · source · rules |
| --- | --- | --- | --- | --- |
| — | الجمعية | CharityId | Drop-down list | Optional · options: lookup: Charities (+ كافة الجهات) · on change: getCharityData() |
| — | التاريخ إلى | DateTo | Date picker | Optional |
| — | التاريخ من | DateFrom | Date picker | Optional |
| — | البنك | BankID | Drop-down list | Optional · options: lookup: Banks |
| — | شيكات إيتام | CheTypes | Radio button | Optional |
| — | شيكات أفراد | CheTypes | Radio button | Optional |

Grids on this screen:

| Grid (row source) | Columns |
| --- | --- |
| Cheque in Cheques track by $index | الرقم · رقم الشيك · تاريخ الشيك · اسم المستفيد · المبلغ |

Commands on this screen:

| Command | Handler | Shown when |
| --- | --- | --- |
| طباعة | PrintData() | always |
| بحث | Search() | always |

### 16.U  Use case scenarios

One expanded scenario for every use case of this module. Pre-conditions, flows and post-conditions are derived from the screen that hosts the function, the Web API action that serves it and the business-layer method that executes it; quoted messages are the literal strings returned by the business layer.

#### 16.U.1  UC-CHQ-01 — List cheques قائمة الشيكات


| Item | Specification |
| --- | --- |
| Use case ID | UC-CHQ-01 |
| Name | List cheques قائمة الشيكات |
| Type | Browse a list |
| Primary actor | Fin. Director, Gen. Director |
| Secondary actors | The system (Web API + business layer); the database. |
| Summary | Paged register of all issued cheques with beneficiary, bank, amount, currency, date and status. |
| Trigger | The actor opens the screen at `#/general-checks` from the module menu. |
| Pre-conditions | 1. The actor is authenticated; the JWT access token is valid and the client holds the role, charity and country claims it carries.<br>2. The actor holds one of: Fin. Director, Gen. Director.<br>3. Caller identity comes from the JWT, never from the request; the application service scopes the query to the charity (and country) that owns the user, unless the role is an HQ role permitted to pass an explicit `charityId` filter.<br>4. The SPA route `#/general-checks` has loaded and its reference-data lookups have been populated. |
| Main flow | 1. The actor navigates to the screen at `#/general-checks`.<br>2. The system loads the filter lookups the screen offers and, for HQ roles, the charity selector.<br>3. The SPA issues `GET /api/CheckManagement` carrying userId, pageNum.<br>4. `CheckManagementController` binds the typed request DTO and delegates to the application service.<br>5. `ICheckService` validates the payload with its FluentValidation validator, applies the business rules and the charity scope, and persists through `IUnitOfWork`.<br>6. The result set is scoped to the caller’s charity and country, ordered and paged.<br>7. The SPA renders the page with the columns listed in the screen specification and the paging control. |
| Alternate flows | • An HQ role (General Director, Financial Director, Staff) may pass an explicit charity id and so read across the charity boundary; a charity user may not and always sees its own data.<br>• No row matches the criteria — the grid renders empty and the paging control reports zero pages. |
| Exception flows | • The session has expired or the role is not permitted — the request is rejected and the SPA routes back to the login state. |
| Post-conditions | • No stored data is changed — the operation is a read. |
| Realisation | Route `#/general-checks` → `CheckListComponent`<br>`GET /api/CheckManagement` → `CheckManagementController` → `ICheckService` |

#### 16.U.2  UC-CHQ-02 — Issue a cheque اضافة شيك


| Item | Specification |
| --- | --- |
| Use case ID | UC-CHQ-02 |
| Name | Issue a cheque اضافة شيك |
| Type | Create a record |
| Primary actor | Fin. Director |
| Secondary actors | The system (Web API + business layer); the database. |
| Summary | Captures beneficiary, bank, account, amount, currency and date, converts the amount to Arabic words for the printed cheque, and stores the record. |
| Trigger | The actor presses «حفظ» on the screen AddCheck. |
| Pre-conditions | 1. The actor is authenticated; the JWT access token is valid and the client holds the role, charity and country claims it carries.<br>2. The actor holds one of: Fin. Director.<br>3. The SPA route `#/general-checks/edit/:id` has loaded and its reference-data lookups have been populated. |
| Main flow | 1. The actor navigates to the screen at `#/general-checks/edit/:id`. The screen opens in add mode.<br>2. The system loads the reference data the form needs (regions, centres, banks, types and the other lookup lists bound to the drop-downs).<br>3. The actor completes the input fields. Mandatory fields: البنك، اسم المستفيد، تاريخ الشيك، رقم الشيك، العملة، المبلغ.<br>4. The actor presses «حفظ» (AddCheck()).<br>5. The SPA issues `POST /api/CheckManagement` carrying typed request DTO data.<br>6. `CheckManagementController` binds the typed request DTO and delegates to the application service.<br>7. `ICheckService` validates the payload with its FluentValidation validator, applies the business rules and the charity scope, and persists through `IUnitOfWork`.<br>8. The business layer validates the payload, stamps the owning charity and the creating user, and persists the new record.<br>9. The system returns the outcome and the SPA confirms the save and returns to the list screen. |
| Alternate flows | • The actor abandons the form before saving — nothing is written and the record keeps its previous state. |
| Exception flows | • The session has expired or the role is not permitted — the request is rejected and the SPA routes back to the login state.<br>• A mandatory field is empty or fails its format check — the save is refused and the field is flagged on the form. |
| Post-conditions | • A new record exists, owned by the charity of the creating user, and appears in the list screen of the module. |
| Realisation | Route `#/general-checks/create` → `CheckFormComponent`<br>`POST /api/CheckManagement` → `CheckManagementController` → `ICheckService` |

#### 16.U.3  UC-CHQ-03 — View a cheque عرض الشيك


| Item | Specification |
| --- | --- |
| Use case ID | UC-CHQ-03 |
| Name | View a cheque عرض الشيك |
| Type | Read a record |
| Primary actor | Fin. Director, Gen. Director |
| Secondary actors | The system (Web API + business layer); the database. |
| Summary | Loads a single cheque for review or editing. |
| Trigger | The actor selects the function from the module menu or the screen that hosts it. |
| Pre-conditions | 1. The actor is authenticated; the JWT access token is valid and the client holds the role, charity and country claims it carries.<br>2. The actor holds one of: Fin. Director, Gen. Director.<br>3. Caller identity comes from the JWT, never from the request; the application service scopes the query to the charity (and country) that owns the user, unless the role is an HQ role permitted to pass an explicit `charityId` filter.<br>4. The target record exists and its identifier is known to the screen. |
| Main flow | 1. The actor reaches the function from the screen of this module that hosts it.<br>2. The actor selects the record to open.<br>3. The SPA issues `GET /api/CheckManagement/{id}` carrying id, userId.<br>4. `CheckManagementController` binds the typed request DTO and delegates to the application service.<br>5. `ICheckService` validates the payload with its FluentValidation validator, applies the business rules and the charity scope, and persists through `IUnitOfWork`.<br>6. The system returns the record with its child collections and the SPA binds them to the form fields. |
| Alternate flows | • An HQ role (General Director, Financial Director, Staff) may pass an explicit charity id and so read across the charity boundary; a charity user may not and always sees its own data. |
| Exception flows | • The session has expired or the role is not permitted — the request is rejected and the SPA routes back to the login state. |
| Post-conditions | • No stored data is changed — the operation is a read. |
| Realisation | `GET /api/CheckManagement/{id}` → `CheckManagementController` → `ICheckService` |

#### 16.U.4  UC-CHQ-04 — Update a cheque تعديل الشيك


| Item | Specification |
| --- | --- |
| Use case ID | UC-CHQ-04 |
| Name | Update a cheque تعديل الشيك |
| Type | Update a record |
| Primary actor | Fin. Director |
| Secondary actors | The system (Web API + business layer); the database. |
| Summary | Amends an issued cheque's data before printing or clearing. |
| Trigger | The actor selects the function from the module menu or the screen that hosts it. |
| Pre-conditions | 1. The actor is authenticated; the JWT access token is valid and the client holds the role, charity and country claims it carries.<br>2. The actor holds one of: Fin. Director.<br>3. The target record exists and its identifier is known to the screen. |
| Main flow | 1. The actor reaches the function from the screen of this module that hosts it. The screen opens in edit mode on the selected record.<br>2. The system loads the current values into the form.<br>3. The actor changes the fields to be corrected.<br>4. The actor presses the command button of the screen.<br>5. The SPA issues `PUT /api/CheckManagement` carrying typed request DTO data.<br>6. `CheckManagementController` binds the typed request DTO and delegates to the application service.<br>7. `ICheckService` validates the payload with its FluentValidation validator, applies the business rules and the charity scope, and persists through `IUnitOfWork`.<br>8. The business layer re-validates the payload, applies the changes and persists them.<br>9. The system returns the outcome and the SPA refreshes the screen. |
| Alternate flows | • The actor abandons the form before saving — nothing is written and the record keeps its previous state. |
| Exception flows | • The session has expired or the role is not permitted — the request is rejected and the SPA routes back to the login state.<br>• A mandatory field is empty or fails its format check — the save is refused and the field is flagged on the form. |
| Post-conditions | • The stored record carries the new values; no other record is affected. |
| Realisation | `PUT /api/CheckManagement` → `CheckManagementController` → `ICheckService` |

#### 16.U.5  UC-CHQ-05 — Select a beneficiary اختيار المستفيد


| Item | Specification |
| --- | --- |
| Use case ID | UC-CHQ-05 |
| Name | Select a beneficiary اختيار المستفيد |
| Type | Search / filter |
| Primary actor | Fin. Director |
| Secondary actors | The system (Web API + business layer); the database. |
| Summary | Type-ahead search over the beneficiary catalogue (charities, suppliers, individuals) to attach the payee to the cheque. |
| Trigger | The actor selects the function from the module menu or the screen that hosts it. |
| Pre-conditions | 1. The actor is authenticated; the JWT access token is valid and the client holds the role, charity and country claims it carries.<br>2. The actor holds one of: Fin. Director.<br>3. Caller identity comes from the JWT, never from the request; the application service scopes the query to the charity (and country) that owns the user, unless the role is an HQ role permitted to pass an explicit `charityId` filter. |
| Main flow | 1. The actor reaches the function from the screen of this module that hosts it.<br>2. The actor enters the search criteria in the filter fields.<br>3. The actor presses the command button of the screen.<br>4. The SPA issues `GET /api/LookupManagement/cheque-beneficiaries` carrying term, userId.<br>5. `LookupManagementController` binds the typed request DTO and delegates to the application service.<br>6. `ICheckService` validates the payload with its FluentValidation validator, applies the business rules and the charity scope, and persists through `IUnitOfWork`.<br>7. The matching rows are returned, scoped to the caller’s charity, and rendered in the result grid. |
| Alternate flows | • An HQ role (General Director, Financial Director, Staff) may pass an explicit charity id and so read across the charity boundary; a charity user may not and always sees its own data.<br>• No row matches the criteria — the grid renders empty and the paging control reports zero pages. |
| Exception flows | • The session has expired or the role is not permitted — the request is rejected and the SPA routes back to the login state. |
| Post-conditions | • No stored data is changed — the operation is a read. |
| Realisation | `GET /api/LookupManagement/cheque-beneficiaries` → `LookupManagementController` → `ILookupService` |

#### 16.U.6  UC-CHQ-06 — Select the currency اختيار العملة


| Item | Specification |
| --- | --- |
| Use case ID | UC-CHQ-06 |
| Name | Select the currency اختيار العملة |
| Type | Browse a list |
| Primary actor | Fin. Director |
| Secondary actors | The system (Web API + business layer); the database. |
| Summary | Loads the currency list that drives the amount and the words conversion. |
| Trigger | The actor selects the function from the module menu or the screen that hosts it. |
| Pre-conditions | 1. The actor is authenticated; the JWT access token is valid and the client holds the role, charity and country claims it carries.<br>2. The actor holds one of: Fin. Director. |
| Main flow | 1. The actor reaches the function from the screen of this module that hosts it.<br>2. The system loads the filter lookups the screen offers and, for HQ roles, the charity selector.<br>3. The SPA issues `GET /api/LookupManagement/currencies`.<br>4. `CheckManagementController` binds the typed request DTO and delegates to the application service.<br>5. `ICheckService` validates the payload with its FluentValidation validator, applies the business rules and the charity scope, and persists through `IUnitOfWork`.<br>6. The result set is scoped to the caller’s charity and country, ordered and paged.<br>7. The SPA renders the page and the paging control. |
| Alternate flows | • No row matches the criteria — the grid renders empty and the paging control reports zero pages. |
| Exception flows | • The session has expired or the role is not permitted — the request is rejected and the SPA routes back to the login state.<br>• The business layer returns «Faliure» and the operation is not applied. |
| Post-conditions | • No stored data is changed — the operation is a read. |
| Realisation | `GET /api/LookupManagement/currencies` → `LookupManagementController` → `ILookupService` |

#### 16.U.7  UC-CHQ-07 — Convert an amount to Arabic words تفقيط المبلغ


| Item | Specification |
| --- | --- |
| Use case ID | UC-CHQ-07 |
| Name | Convert an amount to Arabic words تفقيط المبلغ |
| Type | Print / produce a document |
| Primary actor | System |
| Secondary actors | The system (Web API + business layer); the database. the report and export endpoints of `api/Reports` as the rendering path — jsPDF for PDF and ExcelJS for Excel. |
| Summary | Converts the numeric cheque amount into its Arabic written form, which is printed on the cheque face and stored with the record. |
| Trigger | The actor selects the function from the module menu or the screen that hosts it. |
| Pre-conditions | 1. The actor is authenticated; the JWT access token is valid and the client holds the role, charity and country claims it carries.<br>2. The actor holds one of: System.<br>3. Caller identity comes from the JWT, never from the request; the application service scopes the query to the charity (and country) that owns the user, unless the role is an HQ role permitted to pass an explicit `charityId` filter.<br>4. The target record exists and its identifier is known to the screen. |
| Main flow | 1. The actor reaches the function from the screen of this module that hosts it.<br>2. The actor selects the record or the range to be printed.<br>3. The actor presses the command button of the screen.<br>4. The browser opens `GET /api/CheckManagement/amount-in-words` with amount, userId.<br>5. `ReportsController` resolves the report key, applies the filter, and renders the document from the dataset the application service returns.<br>6. The rendered document is streamed back as a PDF for viewing and printing. |
| Alternate flows | • An HQ role (General Director, Financial Director, Staff) may pass an explicit charity id and so read across the charity boundary; a charity user may not and always sees its own data. |
| Exception flows | • The session has expired or the role is not permitted — the request is rejected and the SPA routes back to the login state.<br>• The report definition or a required parameter is missing — the print action fails and no document is produced. |
| Post-conditions | • A printable document has been produced. Where the module records printing, the row is flagged as printed. |
| Realisation | `GET /api/CheckManagement/amount-in-words` → `CheckManagementController` → `ICheckService` |

#### 16.U.8  UC-CHQ-08 — Load bank cheque print positions مواضع الطباعة على الشيك


| Item | Specification |
| --- | --- |
| Use case ID | UC-CHQ-08 |
| Name | Load bank cheque print positions مواضع الطباعة على الشيك |
| Type | Print / produce a document |
| Primary actor | Fin. Director |
| Secondary actors | The system (Web API + business layer); the database. the report and export endpoints of `api/Reports` as the rendering path — jsPDF for PDF and ExcelJS for Excel. |
| Summary | Retrieves the coordinate offsets configured for the selected bank's cheque stationery so the printed fields align with the pre-printed form. |
| Trigger | The actor selects the function from the module menu or the screen that hosts it. |
| Pre-conditions | 1. The actor is authenticated; the JWT access token is valid and the client holds the role, charity and country claims it carries.<br>2. The actor holds one of: Fin. Director.<br>3. Caller identity comes from the JWT, never from the request; the application service scopes the query to the charity (and country) that owns the user, unless the role is an HQ role permitted to pass an explicit `charityId` filter.<br>4. The target record exists and its identifier is known to the screen. |
| Main flow | 1. The actor reaches the function from the screen of this module that hosts it.<br>2. The actor selects the record or the range to be printed.<br>3. The actor presses the command button of the screen.<br>4. The browser opens `GET /api/LookupManagement/banks/{id}/cheque-positions` with id, userId.<br>5. `ReportsController` resolves the report key, applies the filter, and renders the document from the dataset the application service returns.<br>6. The rendered document is streamed back as a PDF for viewing and printing. |
| Alternate flows | • An HQ role (General Director, Financial Director, Staff) may pass an explicit charity id and so read across the charity boundary; a charity user may not and always sees its own data. |
| Exception flows | • The session has expired or the role is not permitted — the request is rejected and the SPA routes back to the login state.<br>• The business layer returns «Operation Faild» and the operation is not applied.<br>• The report definition or a required parameter is missing — the print action fails and no document is produced. |
| Post-conditions | • A printable document has been produced. Where the module records printing, the row is flagged as printed. |
| Realisation | `GET /api/LookupManagement/banks/{id}/cheque-positions` · `GET /api/LookupManagement/countries/{id}` → `LookupManagementController` → `ILookupService` |

#### 16.U.9  UC-CHQ-09 — Produce a cheque statement بيان الشيكات


| Item | Specification |
| --- | --- |
| Use case ID | UC-CHQ-09 |
| Name | Produce a cheque statement بيان الشيكات |
| Type | Import a file |
| Primary actor | Fin. Director, Gen. Director |
| Secondary actors | The system (Web API + business layer); the database. The uploaded workbook/CSV as the data source. |
| Summary | Filters the cheque register (bank, date range, status, beneficiary) and returns the statement used for bank reconciliation. |
| Trigger | The actor presses «طباعة» on the screen ChecksStatement. |
| Pre-conditions | 1. The actor is authenticated; the JWT access token is valid and the client holds the role, charity and country claims it carries.<br>2. The actor holds one of: Fin. Director, Gen. Director.<br>3. Caller identity comes from the JWT, never from the request; the application service scopes the query to the charity (and country) that owns the user, unless the role is an HQ role permitted to pass an explicit `charityId` filter.<br>4. The SPA route `#/general-checks/statement` has loaded and its reference-data lookups have been populated. |
| Main flow | 1. The actor navigates to the screen at `#/general-checks/statement`.<br>2. The actor chooses the file to upload in the file field of the screen.<br>3. The actor presses «طباعة» (PrintData()).<br>4. The SPA posts the file as multipart content to `GET /api/CheckManagement/report`.<br>5. `CheckManagementController` binds the typed request DTO and delegates to the application service.<br>6. `ICheckService` validates the payload with its FluentValidation validator, applies the business rules and the charity scope, and persists through `IUnitOfWork`.<br>7. The business layer parses each row, matches it to the existing records by their key and applies the values it carries.<br>8. Rows that cannot be matched are reported back and the system returns the count applied. |
| Alternate flows | • An HQ role (General Director, Financial Director, Staff) may pass an explicit charity id and so read across the charity boundary; a charity user may not and always sees its own data. |
| Exception flows | • The session has expired or the role is not permitted — the request is rejected and the SPA routes back to the login state.<br>• The uploaded file is not the expected workbook/CSV layout — the import is abandoned and no row is changed. |
| Post-conditions | • The matched records carry the imported values; unmatched rows are left untouched and reported. |
| Realisation | Route `#/general-checks/reconcile` → `CheckReconcileComponent`<br>`GET /api/CheckManagement/report` → `CheckManagementController` → `ICheckService` |

#### 16.U.10  UC-CHQ-10 — Print a cheque and the cheque report طباعة الشيك والتقرير


| Item | Specification |
| --- | --- |
| Use case ID | UC-CHQ-10 |
| Name | Print a cheque and the cheque report طباعة الشيك والتقرير |
| Type | Print / produce a document |
| Primary actor | Fin. Director |
| Secondary actors | The system (Web API + business layer); the database. the report and export endpoints of `api/Reports` as the rendering path — jsPDF for PDF and ExcelJS for Excel. |
| Summary | Renders the cheque onto the bank's stationery using the configured positions, with a country-specific variant, and produces the cheque report for a bank, date range and cheque type. |
| Trigger | The actor selects the function from the module menu or the screen that hosts it. |
| Pre-conditions | 1. The actor is authenticated; the JWT access token is valid and the client holds the role, charity and country claims it carries.<br>2. The actor holds one of: Fin. Director.<br>3. Caller identity comes from the JWT, never from the request; the application service scopes the query to the charity (and country) that owns the user, unless the role is an HQ role permitted to pass an explicit `charityId` filter.<br>4. The target record exists and its identifier is known to the screen. |
| Main flow | 1. The actor reaches the function from the screen of this module that hosts it.<br>2. The actor selects the record or the range to be printed.<br>3. The actor presses the command button of the screen.<br>4. The browser opens `POST /api/Reports/general-cheque/export/pdf` with string userId, string id.<br>5. `ReportsController` resolves the report key, applies the filter, and renders the document from the dataset the application service returns.<br>6. The rendered document is streamed back as a PDF for viewing and printing. |
| Alternate flows | • An HQ role (General Director, Financial Director, Staff) may pass an explicit charity id and so read across the charity boundary; a charity user may not and always sees its own data. |
| Exception flows | • The session has expired or the role is not permitted — the request is rejected and the SPA routes back to the login state.<br>• The report definition or a required parameter is missing — the print action fails and no document is produced. |
| Post-conditions | • A printable document has been produced. Where the module records printing, the row is flagged as printed. |
| Realisation | `POST /api/Reports/general-cheque/export/pdf` · `POST /api/Reports/general-cheque-eg/export/pdf` · `POST /api/Reports/cheque-statement/export/pdf` · `POST /api/Reports/payment-cheques/export/pdf` · `POST /api/Reports/payment-cheques-eg/export/pdf` → `ReportsController` → `IReportService` |

### 16.A  Annex - Angular routes of this module

Routes are hash-based (`useHash: true`), rendered inside `MainLayoutComponent` behind
`AuthGuard` and `PermissionGuard`. *implemented* means the route exists in
`Frontend/src/app`; *planned* means it is specified here and not yet built.

| Angular route | Feature module | Component | Status |
| --- | --- | --- | --- |
| `#/general-checks` | `general-checks` | `CheckListComponent` | implemented |
| `#/general-checks/create` | `general-checks` | `CheckFormComponent` | implemented |
| `#/general-checks/edit/:id` | `general-checks` | `CheckFormComponent` | implemented |
| `#/general-checks/:id` | `general-checks` | `CheckDetailComponent` | implemented |
| `#/general-checks/reconcile` | `general-checks` | `CheckReconcileComponent` | declared but shadowed |

> Two deviations in this module, both in `general-checks-routing.module.ts`:
>
> 1. The edit route is `edit/:id`, not the `:id/edit` every other feature module uses.
> 2. `reconcile` is declared **after** `:id`, so the router matches `:id` first and
>    `#/general-checks/reconcile` opens `CheckDetailComponent` with `id = "reconcile"`.
>    Moving `reconcile` above `:id` fixes it.

### 16.B  Annex - API controllers of this module

Every controller inherits `ApiController` (`[Authorize]`, route `api/[controller]`) and
returns `Framework.Core.ApiResponse<T>`. Business rules live in the application service
behind each controller, never in the controller itself.

| Controller | Base route | Responsibility |
| --- | --- | --- |
| `CheckManagementController` | `api/CheckManagement` | Cheque create/update, currencies, Arabic amount, statements. Cheque read by id. Cheque register listing. |
| `LookupManagementController` | `api/LookupManagement` | Beneficiary autocomplete. Cheque print positions per bank. |

---

Shared context - system architecture, actors and roles, the sponsorship lifecycle, the full module map and all appendices - is in the companion document [00-Overview-and-Common-Context](00-Overview-and-Common-Context.md). The delivery backlog for this module is epic EP-11 in [00-EPICS-and-User-Stories](00-EPICS-and-User-Stories.md).

