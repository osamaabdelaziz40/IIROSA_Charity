# WAR.IIROSA - HQ Financial Transfers

الحوالات المالية للادارة المالية | use case prefix `UC-TRF` | chapter 22 of the master document


| Field | Value |
| --- | --- |
| Document title | WAR.IIROSA - Module Specification: HQ Financial Transfers |
| Module | HQ Financial Transfers - الحوالات المالية للادارة المالية |
| Use case prefix | UC-TRF |
| Chapter in master document | Chapter 22 |
| Documented use cases | 8 |
| Principal routes | `#/hq-transfers`, `#/hq-transfers/create`, `#/hq-transfers/:id`, `#/hq-transfers/:id/edit`, `#/hq-transfers/max-amounts` (all *planned*) |
| Version | 1.1 |
| Status | Chapter content extracted verbatim; screen fields and scenarios derived from the source code |
| Date | 18 August 2026 |
| Parent document | WAR.IIROSA-Project-Documentation.docx v1.0 (repository WAR.IIROSA, commit 4beb736) |

## Contents of this document

1. Chapter 22 — module purpose and use-case catalogue (verbatim from the master document)
2. §22.S — screen field specifications (every field of every screen, derived from the AngularJS views)
3. §22.U — one expanded scenario per use case (trigger, pre-conditions, main flow, alternates, exceptions, post-conditions)
4. §22.A / §22.B — annexes: screens and Web API controllers of this module

## 22. HQ Financial Transfers

الحوالات المالية للإدارة المالية — transfers issued by head office to charities or countries, governed by a configurable maximum amount per country. This module is the Financial Director's own area; the client menu shows it to transfer roles and hides the operational modules from them.


| ID | Use case | Primary actor | Description & main flow | Realisation |
| --- | --- | --- | --- | --- |
| UC-TRF-01 | List transfers قائمة الحوالات | Fin. Director, Gen. Director | Register of head-office transfers with beneficiary, amount, currency, department and status. | Route `#/hq-transfers` → GET /api/HqTransfers |
| UC-TRF-02 | Create a transfer اضافة حوالة | Fin. Director | Issues a transfer to a charity or country, recording purpose, department, amount and currency. Business rule: the amount is validated against the destination country's configured maximum. | Route `#/hq-transfers/create` → POST /api/HqTransfers |
| UC-TRF-03 | View a transfer عرض الحوالة | Fin. Director, Gen. Director | Loads a single transfer with its header data for review or editing. | GET /api/HqTransfers/{id} |
| UC-TRF-04 | Update a transfer تعديل الحوالة | Fin. Director | Amends transfer data and advances its processing state. | PUT /api/HqTransfers |
| UC-TRF-05 | Select the issuing department الإدارة المصدرة | Fin. Director | Loads the head-office department catalogue that owns the transfer. | GET /api/LookupManagement/departments |
| UC-TRF-06 | View the maximum transfer amount for a country الحد الأعلى للحوالة | Fin. Director | Retrieves the ceiling configured for a destination country, shown as a guard on the transfer form. | GET /api/HqTransfers/max-amount |
| UC-TRF-07 | Set the maximum transfer amount تعيين الحد الأعلى للحوالة | Fin. Director | Updates the per-country ceiling that constrains all subsequent transfers to that country. | Route `#/hq-transfers/max-amounts` → PUT /api/HqTransfers/max-amount |
| UC-TRF-08 | Manage transfer detail lines تفاصيل الحوالة | Fin. Director | Lists and updates the individual detail lines of a transfer — the allocation of the transferred sum across purposes or charities — from the financial-management screen. | Route `#/hq-transfers/:id` → GET /api/HqTransfers/{id}/details PUT /api/HqTransfers/{id}/details |

### 22.S  Screen field specifications

Derived from the AngularJS views of this module. For every screen the table lists each data-entry field in the order it appears, the scope property it binds to, its control type, whether the form marks it mandatory, and the lookup or rule that governs it. Labels are reproduced as they appear on screen (Arabic, right-to-left).

#### 22.S.1  Screen `#/hq-transfers`


| Property | Value |
| --- | --- |
| Angular route | `#/hq-transfers` |
| Feature module | `hq-transfers` (lazy-loaded) |
| Component | `HqTransferListComponent` |
| Route status | planned |
| Data-entry fields | 0 |
| Grids on the screen | 1 |
| Commands | 3 |

Grids on this screen:

| Grid (row source) | Columns |
| --- | --- |
| trans in Transfers track by $index | الرقم · رقم العملية · السنة المالية · رقم الدفعة · من تاريخ · الى تاريخ · مبلغ الدفعة · البيان · عدد المستفيدين · رقم المعاملة · تاريخ المعاملة · الاجراءات |

Commands on this screen:

| Command | Handler | Shown when |
| --- | --- | --- |
| (icon only) | AddTransFer() | always |
| (icon only) | EditTransFer(trans.Id) | always |
| (icon only) | ViewTransFer(trans.Id) | always |

#### 22.S.2  Screen `#/hq-transfers/:id/edit`


| Property | Value |
| --- | --- |
| Angular route | `#/hq-transfers/:id/edit` |
| Feature module | `hq-transfers` (lazy-loaded) |
| Component | `HqTransferFormComponent` |
| Route status | planned |
| Data-entry fields | 12 |
| Grids on the screen | 0 |
| Commands | 1 |


| Section | Field (as labelled) | Bound to | Control | Mandatory · source · rules |
| --- | --- | --- | --- | --- |
| إضافة حوالة | الدولة | Country | Drop-down list | Mandatory · options: lookup: Countries · on change: GetTransferValue() |
| إضافة حوالة | اسم الادارة الطالبة | Department | Drop-down list | Mandatory · options: lookup: Departments |
| إضافة حوالة | رقم العملية | OperationNumber | Text box | Mandatory |
| إضافة حوالة | السنة المالية | FinYear | Text box | Mandatory |
| إضافة حوالة | رقم الدفعة | PaymentNumber | Drop-down list | Mandatory · options: 1 / 2 / 3 / 4 |
| إضافة حوالة | الى تاريخ | DateTo | Text box | Mandatory |
| إضافة حوالة | من تاريخ | DateFrom | Text box | Mandatory |
| إضافة حوالة | مبلغ الدفعة | AmountOfPayment | Numeric box | Mandatory |
| إضافة حوالة | البيان | Statement | Text box | Optional |
| إضافة حوالة | عدد المستفدين | BeneficiariesNumber | Numeric box | Mandatory |
| إضافة حوالة | رقم المعاملة | TransactionNumber | Text box | Mandatory |
| إضافة حوالة | تاريخ المعاملة | TransactionDate | Date picker | Mandatory |

Commands on this screen:

| Command | Handler | Shown when |
| --- | --- | --- |
| حفظ | SubmitTransfer() | always |

#### 22.S.3  Screen `#/hq-transfers/:id`


| Property | Value |
| --- | --- |
| Angular route | `#/hq-transfers/:id` |
| Feature module | `hq-transfers` (lazy-loaded) |
| Component | `HqTransferDetailComponent` |
| Route status | planned |
| Data-entry fields | 5 |
| Grids on the screen | 1 |
| Commands | 1 |


| Section | Field (as labelled) | Bound to | Control | Mandatory · source · rules |
| --- | --- | --- | --- | --- |
| — | (unlabelled) | trans.EstimatedTransferDate | Date picker | Optional · read-only when !TransfersAdmin |
| — | (unlabelled) | trans.TransferState | Drop-down list | Optional · options: لم ينفذ / تم التنفيذ · read-only when !TransfersAdmin |
| — | (unlabelled) | trans.ExecutionDate | Date picker | Optional · read-only when !TransfersAdmin |
| — | (unlabelled) | trans.ArrivalDate | Date picker | Optional · read-only when !IsAdmin\|\|trans.TransferState!='تم التنفيذ' |
| — | (unlabelled) | trans.ArrivalAmount | Text box | Optional · read-only when !IsAdmin\|\|trans.TransferState!='تم التنفيذ' |

Grids on this screen:

| Grid (row source) | Columns |
| --- | --- |
| trans in Transfers track by $index | رقم الحوالة · مبلغ الحوالة · التاريخ المتوقع للتحويل · مصير الحوالة · تاريخ التنفيذ · تاريخ وصول الحوالة · مبلغ الوصول · حفظ |

Commands on this screen:

| Command | Handler | Shown when |
| --- | --- | --- |
| حفظ | SubmitTransfer(trans) | always |

#### 22.S.4  Screen `#/hq-transfers/max-amounts`


| Property | Value |
| --- | --- |
| Angular route | `#/hq-transfers/max-amounts` |
| Feature module | `hq-transfers` (lazy-loaded) |
| Component | `MaxTransferAmountComponent` |
| Route status | planned |
| Data-entry fields | 1 |
| Grids on the screen | 1 |
| Commands | 1 |


| Section | Field (as labelled) | Bound to | Control | Mandatory · source · rules |
| --- | --- | --- | --- | --- |
| — | (unlabelled) | country.MaxTransferAmount | Text box | Optional |

Grids on this screen:

| Grid (row source) | Columns |
| --- | --- |
| country in Countries track by $index | الرقم · البلد · قيمة الحوالة · حفظ |

Commands on this screen:

| Command | Handler | Shown when |
| --- | --- | --- |
| حفظ | UpdateCountry(country) | always |

### 22.U  Use case scenarios

One expanded scenario for every use case of this module. Pre-conditions, flows and post-conditions are derived from the screen that hosts the function, the Web API action that serves it and the business-layer method that executes it; quoted messages are the literal strings returned by the business layer.

#### 22.U.1  UC-TRF-01 — List transfers قائمة الحوالات


| Item | Specification |
| --- | --- |
| Use case ID | UC-TRF-01 |
| Name | List transfers قائمة الحوالات |
| Type | Browse a list |
| Primary actor | Fin. Director, Gen. Director |
| Secondary actors | The system (Web API + business layer); the database. |
| Summary | Register of head-office transfers with beneficiary, amount, currency, department and status. |
| Trigger | The actor opens the screen at `#/hq-transfers` from the module menu. |
| Pre-conditions | 1. The actor is authenticated; the JWT access token is valid and the client holds the role, charity and country claims it carries.<br>2. The actor holds one of: Fin. Director, Gen. Director.<br>3. Caller identity comes from the JWT, never from the request; the application service scopes the query to the charity (and country) that owns the user, unless the role is an HQ role permitted to pass an explicit `charityId` filter.<br>4. The SPA route `#/hq-transfers` has loaded and its reference-data lookups have been populated. |
| Main flow | 1. The actor navigates to the screen at `#/hq-transfers`.<br>2. The system loads the filter lookups the screen offers and, for HQ roles, the charity selector.<br>3. The SPA issues `GET /api/HqTransfers` carrying userId.<br>4. `HqTransfersController` binds the typed request DTO and delegates to the application service.<br>5. `IHqTransferService` validates the payload with its FluentValidation validator, applies the business rules and the charity scope, and persists through `IUnitOfWork`.<br>6. The result set is scoped to the caller’s charity and country, ordered and paged.<br>7. The SPA renders the page with the columns listed in the screen specification and the paging control. |
| Alternate flows | • An HQ role (General Director, Financial Director, Staff) may pass an explicit charity id and so read across the charity boundary; a charity user may not and always sees its own data.<br>• No row matches the criteria — the grid renders empty and the paging control reports zero pages. |
| Exception flows | • The session has expired or the role is not permitted — the request is rejected and the SPA routes back to the login state. |
| Post-conditions | • No stored data is changed — the operation is a read. |
| Realisation | Route `#/hq-transfers` → `HqTransferListComponent`<br>`GET /api/HqTransfers` → `HqTransfersController` → `IHqTransferService` |

#### 22.U.2  UC-TRF-02 — Create a transfer اضافة حوالة


| Item | Specification |
| --- | --- |
| Use case ID | UC-TRF-02 |
| Name | Create a transfer اضافة حوالة |
| Type | Create a record |
| Primary actor | Fin. Director |
| Secondary actors | The system (Web API + business layer); the database. |
| Summary | Issues a transfer to a charity or country, recording purpose, department, amount and currency. Business rule: the amount is validated against the destination country's configured maximum. |
| Trigger | The actor presses «حفظ» on the screen ManageTransfer. |
| Pre-conditions | 1. The actor is authenticated; the JWT access token is valid and the client holds the role, charity and country claims it carries.<br>2. The actor holds one of: Fin. Director.<br>3. The SPA route `#/hq-transfers/:id/edit` has loaded and its reference-data lookups have been populated. |
| Main flow | 1. The actor navigates to the screen at `#/hq-transfers/:id/edit`. The screen opens in add mode.<br>2. The system loads the reference data the form needs (regions, centres, banks, types and the other lookup lists bound to the drop-downs).<br>3. The actor completes the input fields. Mandatory fields: الدولة، اسم الادارة الطالبة، رقم العملية، السنة المالية، رقم الدفعة، الى تاريخ، من تاريخ، مبلغ الدفعة، عدد المستفدين، رقم المعاملة، تاريخ المعاملة.<br>4. The actor presses «حفظ» (SubmitTransfer()).<br>5. The SPA issues `POST /api/HqTransfers` carrying typed request DTO obj.<br>6. `HqTransfersController` binds the typed request DTO and delegates to the application service.<br>7. `IHqTransferService` validates the payload with its FluentValidation validator, applies the business rules and the charity scope, and persists through `IUnitOfWork`.<br>8. The business layer validates the payload, stamps the owning charity and the creating user, and persists the new record.<br>9. The system returns the outcome and the SPA confirms the save and returns to the list screen. |
| Alternate flows | • The actor abandons the form before saving — nothing is written and the record keeps its previous state. |
| Exception flows | • The session has expired or the role is not permitted — the request is rejected and the SPA routes back to the login state.<br>• A mandatory field is empty or fails its format check — the save is refused and the field is flagged on the form. |
| Post-conditions | • A new record exists, owned by the charity of the creating user, and appears in the list screen of the module. |
| Realisation | Route `#/hq-transfers/create` → `HqTransferFormComponent`<br>`POST /api/HqTransfers` → `HqTransfersController` → `IHqTransferService` |

#### 22.U.3  UC-TRF-03 — View a transfer عرض الحوالة


| Item | Specification |
| --- | --- |
| Use case ID | UC-TRF-03 |
| Name | View a transfer عرض الحوالة |
| Type | Read a record |
| Primary actor | Fin. Director, Gen. Director |
| Secondary actors | The system (Web API + business layer); the database. |
| Summary | Loads a single transfer with its header data for review or editing. |
| Trigger | The actor selects the function from the module menu or the screen that hosts it. |
| Pre-conditions | 1. The actor is authenticated; the JWT access token is valid and the client holds the role, charity and country claims it carries.<br>2. The actor holds one of: Fin. Director, Gen. Director.<br>3. Caller identity comes from the JWT, never from the request; the application service scopes the query to the charity (and country) that owns the user, unless the role is an HQ role permitted to pass an explicit `charityId` filter.<br>4. The target record exists and its identifier is known to the screen. |
| Main flow | 1. The actor reaches the function from the screen of this module that hosts it.<br>2. The actor selects the record to open.<br>3. The SPA issues `GET /api/HqTransfers/{id}` carrying Id, userId.<br>4. `HqTransfersController` binds the typed request DTO and delegates to the application service.<br>5. `IHqTransferService` validates the payload with its FluentValidation validator, applies the business rules and the charity scope, and persists through `IUnitOfWork`.<br>6. The system returns the record with its child collections and the SPA binds them to the form fields. |
| Alternate flows | • An HQ role (General Director, Financial Director, Staff) may pass an explicit charity id and so read across the charity boundary; a charity user may not and always sees its own data. |
| Exception flows | • The session has expired or the role is not permitted — the request is rejected and the SPA routes back to the login state. |
| Post-conditions | • No stored data is changed — the operation is a read. |
| Realisation | `GET /api/HqTransfers/{id}` → `HqTransfersController` → `IHqTransferService` |

#### 22.U.4  UC-TRF-04 — Update a transfer تعديل الحوالة


| Item | Specification |
| --- | --- |
| Use case ID | UC-TRF-04 |
| Name | Update a transfer تعديل الحوالة |
| Type | Update a record |
| Primary actor | Fin. Director |
| Secondary actors | The system (Web API + business layer); the database. |
| Summary | Amends transfer data and advances its processing state. |
| Trigger | The actor selects the function from the module menu or the screen that hosts it. |
| Pre-conditions | 1. The actor is authenticated; the JWT access token is valid and the client holds the role, charity and country claims it carries.<br>2. The actor holds one of: Fin. Director.<br>3. The target record exists and its identifier is known to the screen. |
| Main flow | 1. The actor reaches the function from the screen of this module that hosts it. The screen opens in edit mode on the selected record.<br>2. The system loads the current values into the form.<br>3. The actor changes the fields to be corrected.<br>4. The actor presses the command button of the screen.<br>5. The SPA issues `PUT /api/HqTransfers` carrying typed request DTO obj.<br>6. `HqTransfersController` binds the typed request DTO and delegates to the application service.<br>7. `IHqTransferService` validates the payload with its FluentValidation validator, applies the business rules and the charity scope, and persists through `IUnitOfWork`.<br>8. The business layer re-validates the payload, applies the changes and persists them.<br>9. The system returns the outcome and the SPA refreshes the screen. |
| Alternate flows | • The actor abandons the form before saving — nothing is written and the record keeps its previous state. |
| Exception flows | • The session has expired or the role is not permitted — the request is rejected and the SPA routes back to the login state.<br>• A mandatory field is empty or fails its format check — the save is refused and the field is flagged on the form. |
| Post-conditions | • The stored record carries the new values; no other record is affected. |
| Realisation | `PUT /api/HqTransfers` → `HqTransfersController` → `IHqTransferService` |

#### 22.U.5  UC-TRF-05 — Select the issuing department الإدارة المصدرة


| Item | Specification |
| --- | --- |
| Use case ID | UC-TRF-05 |
| Name | Select the issuing department الإدارة المصدرة |
| Type | Read a record |
| Primary actor | Fin. Director |
| Secondary actors | The system (Web API + business layer); the database. |
| Summary | Loads the head-office department catalogue that owns the transfer. |
| Trigger | The actor selects the function from the module menu or the screen that hosts it. |
| Pre-conditions | 1. The actor is authenticated; the JWT access token is valid and the client holds the role, charity and country claims it carries.<br>2. The actor holds one of: Fin. Director.<br>3. Caller identity comes from the JWT, never from the request; the application service scopes the query to the charity (and country) that owns the user, unless the role is an HQ role permitted to pass an explicit `charityId` filter.<br>4. The target record exists and its identifier is known to the screen. |
| Main flow | 1. The actor reaches the function from the screen of this module that hosts it.<br>2. The actor selects the record to open.<br>3. The SPA issues `GET /api/LookupManagement/departments` carrying userId.<br>4. `HqTransfersController` binds the typed request DTO and delegates to the application service.<br>5. `IHqTransferService` validates the payload with its FluentValidation validator, applies the business rules and the charity scope, and persists through `IUnitOfWork`.<br>6. The system returns the record with its child collections and the SPA binds them to the form fields. |
| Alternate flows | • An HQ role (General Director, Financial Director, Staff) may pass an explicit charity id and so read across the charity boundary; a charity user may not and always sees its own data. |
| Exception flows | • The session has expired or the role is not permitted — the request is rejected and the SPA routes back to the login state. |
| Post-conditions | • No stored data is changed — the operation is a read. |
| Realisation | `GET /api/LookupManagement/departments` → `LookupManagementController` → `ILookupService` |

#### 22.U.6  UC-TRF-06 — View the maximum transfer amount for a country الحد الأعلى للحوالة


| Item | Specification |
| --- | --- |
| Use case ID | UC-TRF-06 |
| Name | View the maximum transfer amount for a country الحد الأعلى للحوالة |
| Type | Read a record |
| Primary actor | Fin. Director |
| Secondary actors | The system (Web API + business layer); the database. |
| Summary | Retrieves the ceiling configured for a destination country, shown as a guard on the transfer form. |
| Trigger | The actor selects the function from the module menu or the screen that hosts it. |
| Pre-conditions | 1. The actor is authenticated; the JWT access token is valid and the client holds the role, charity and country claims it carries.<br>2. The actor holds one of: Fin. Director.<br>3. Caller identity comes from the JWT, never from the request; the application service scopes the query to the charity (and country) that owns the user, unless the role is an HQ role permitted to pass an explicit `charityId` filter.<br>4. The target record exists and its identifier is known to the screen. |
| Main flow | 1. The actor reaches the function from the screen of this module that hosts it.<br>2. The actor selects the record to open.<br>3. The SPA issues `GET /api/HqTransfers/max-amount` carrying countryId, userId.<br>4. `HqTransfersController` binds the typed request DTO and delegates to the application service.<br>5. `IHqTransferService` validates the payload with its FluentValidation validator, applies the business rules and the charity scope, and persists through `IUnitOfWork`.<br>6. The system returns the record with its child collections and the SPA binds them to the form fields. |
| Alternate flows | • An HQ role (General Director, Financial Director, Staff) may pass an explicit charity id and so read across the charity boundary; a charity user may not and always sees its own data. |
| Exception flows | • The session has expired or the role is not permitted — the request is rejected and the SPA routes back to the login state. |
| Post-conditions | • No stored data is changed — the operation is a read. |
| Realisation | `GET /api/HqTransfers/max-amount` → `HqTransfersController` → `IHqTransferService` |

#### 22.U.7  UC-TRF-07 — Set the maximum transfer amount تعيين الحد الأعلى للحوالة


| Item | Specification |
| --- | --- |
| Use case ID | UC-TRF-07 |
| Name | Set the maximum transfer amount تعيين الحد الأعلى للحوالة |
| Type | Update a record |
| Primary actor | Fin. Director |
| Secondary actors | The system (Web API + business layer); the database. |
| Summary | Updates the per-country ceiling that constrains all subsequent transfers to that country. |
| Trigger | The actor presses «حفظ» on the screen UpdateTransferValue. |
| Pre-conditions | 1. The actor is authenticated; the JWT access token is valid and the client holds the role, charity and country claims it carries.<br>2. The actor holds one of: Fin. Director.<br>3. The SPA route `#/hq-transfers/max-amounts` has loaded and its reference-data lookups have been populated.<br>4. The target record exists and its identifier is known to the screen. |
| Main flow | 1. The actor navigates to the screen at `#/hq-transfers/max-amounts`. The screen opens in edit mode on the selected record.<br>2. The system loads the current values into the form.<br>3. The actor changes the fields to be corrected.<br>4. The actor presses «حفظ» (UpdateCountry(country)).<br>5. The SPA issues `PUT /api/HqTransfers/max-amount` carrying typed request DTO obj.<br>6. `HqTransfersController` binds the typed request DTO and delegates to the application service.<br>7. `IHqTransferService` validates the payload with its FluentValidation validator, applies the business rules and the charity scope, and persists through `IUnitOfWork`.<br>8. The business layer re-validates the payload, applies the changes and persists them.<br>9. The system returns the outcome and the SPA refreshes the screen. |
| Alternate flows | • The actor abandons the form before saving — nothing is written and the record keeps its previous state. |
| Exception flows | • The session has expired or the role is not permitted — the request is rejected and the SPA routes back to the login state.<br>• A mandatory field is empty or fails its format check — the save is refused and the field is flagged on the form. |
| Post-conditions | • The stored record carries the new values; no other record is affected. |
| Realisation | Route `#/hq-transfers/max-amounts` → `MaxTransferAmountComponent`<br>`PUT /api/HqTransfers/max-amount` → `HqTransfersController` → `IHqTransferService` |

#### 22.U.8  UC-TRF-08 — Manage transfer detail lines تفاصيل الحوالة


| Item | Specification |
| --- | --- |
| Use case ID | UC-TRF-08 |
| Name | Manage transfer detail lines تفاصيل الحوالة |
| Type | Update a record |
| Primary actor | Fin. Director |
| Secondary actors | The system (Web API + business layer); the database. |
| Summary | Lists and updates the individual detail lines of a transfer — the allocation of the transferred sum across purposes or charities — from the financial-management screen. |
| Trigger | The actor presses «حفظ» on the screen FinManagementForTransfers. |
| Pre-conditions | 1. The actor is authenticated; the JWT access token is valid and the client holds the role, charity and country claims it carries.<br>2. The actor holds one of: Fin. Director.<br>3. Caller identity comes from the JWT, never from the request; the application service scopes the query to the charity (and country) that owns the user, unless the role is an HQ role permitted to pass an explicit `charityId` filter.<br>4. The SPA route `#/hq-transfers/:id` has loaded and its reference-data lookups have been populated.<br>5. The target record exists and its identifier is known to the screen. |
| Main flow | 1. The actor navigates to the screen at `#/hq-transfers/:id`. The screen opens in edit mode on the selected record.<br>2. The system loads the current values into the form.<br>3. The actor changes the fields to be corrected.<br>4. The actor presses «حفظ» (SubmitTransfer(trans)).<br>5. The SPA issues `GET /api/HqTransfers/{id}/details` carrying transferId, userId.<br>6. `HqTransfersController` binds the typed request DTO and delegates to the application service.<br>7. `IHqTransferService` validates the payload with its FluentValidation validator, applies the business rules and the charity scope, and persists through `IUnitOfWork`.<br>8. The business layer re-validates the payload, applies the changes and persists them.<br>9. The system returns the outcome and the SPA refreshes the screen. |
| Alternate flows | • An HQ role (General Director, Financial Director, Staff) may pass an explicit charity id and so read across the charity boundary; a charity user may not and always sees its own data.<br>• The actor abandons the form before saving — nothing is written and the record keeps its previous state. |
| Exception flows | • The session has expired or the role is not permitted — the request is rejected and the SPA routes back to the login state.<br>• The business layer returns «Failed Operation» and the operation is not applied.<br>• A mandatory field is empty or fails its format check — the save is refused and the field is flagged on the form. |
| Post-conditions | • The stored record carries the new values; no other record is affected. |
| Realisation | Route `#/hq-transfers/:id` → `HqTransferDetailComponent`<br>`GET /api/HqTransfers/{id}/details` · `PUT /api/HqTransfers/{id}/details` → `HqTransfersController` → `IHqTransferService` |

### 22.A  Annex - Angular routes of this module

Routes are hash-based (`useHash: true`), rendered inside `MainLayoutComponent` behind
`AuthGuard` and `PermissionGuard`. *implemented* means the route exists in
`Frontend/src/app`; *planned* means it is specified here and not yet built.

| Angular route | Feature module | Component | Status |
| --- | --- | --- | --- |
| `#/hq-transfers` | `hq-transfers` | `HqTransferListComponent` | planned |
| `#/hq-transfers/create` | `hq-transfers` | `HqTransferFormComponent` | planned |
| `#/hq-transfers/:id` | `hq-transfers` | `HqTransferDetailComponent` | planned |
| `#/hq-transfers/:id/edit` | `hq-transfers` | `HqTransferFormComponent` | planned |
| `#/hq-transfers/max-amounts` | `hq-transfers` | `MaxTransferAmountComponent` | planned |

### 22.B  Annex - API controllers of this module

Every controller inherits `ApiController` (`[Authorize]`, route `api/[controller]`) and
returns `Framework.Core.ApiResponse<T>`. Business rules live in the application service
behind each controller, never in the controller itself.

| Controller | Base route | Responsibility |
| --- | --- | --- |
| `HqTransfersController` | `api/HqTransfers` | HQ transfers, departments, per-country maximum. Transfer detail lines. |

---

Shared context - system architecture, actors and roles, the sponsorship lifecycle, the full module map and all appendices - is in the companion document [00-Overview-and-Common-Context](00-Overview-and-Common-Context.md). The delivery backlog for this module is epic EP-17 in [00-EPICS-and-User-Stories](00-EPICS-and-User-Stories.md).

