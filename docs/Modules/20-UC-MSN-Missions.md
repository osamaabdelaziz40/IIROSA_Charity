# WAR.IIROSA - Missions

المأموريات | use case prefix `UC-MSN` | chapter 20 of the master document


| Field | Value |
| --- | --- |
| Document title | WAR.IIROSA - Module Specification: Missions |
| Module | Missions - المأموريات |
| Use case prefix | UC-MSN |
| Chapter in master document | Chapter 20 |
| Documented use cases | 9 |
| Principal routes | `#/missions`, `#/missions/:id/edit`, `#/missions/:id/register` |
| Version | 1.1 |
| Status | Chapter content extracted verbatim; screen fields and scenarios derived from the source code |
| Date | 18 August 2026 |
| Parent document | WAR.IIROSA-Project-Documentation.docx v1.0 (repository WAR.IIROSA, commit 4beb736) |

## Contents of this document

1. Chapter 20 — module purpose and use-case catalogue (verbatim from the master document)
2. §20.S — screen field specifications (every field of every screen, derived from the AngularJS views)
3. §20.U — one expanded scenario per use case (trigger, pre-conditions, main flow, alternates, exceptions, post-conditions)
4. §20.A / §20.B — annexes: screens and Web API controllers of this module

## 20. Missions

المأموريات — field assignments and monitoring visits carried out by office staff to charities and beneficiaries, with the interview outcome recorded on return.


| ID | Use case | Primary actor | Description & main flow | Realisation |
| --- | --- | --- | --- | --- |
| UC-MSN-01 | List missions قائمة المأموريات | Gen. Director, Staff | Shows the mission register with destination, type, dates and assigned staff. | Route `#/missions` → GET /api/MissionManagement/my-missions |
| UC-MSN-02 | Filter missions by date البحث بالتاريخ | Gen. Director, Staff | Restricts the mission register to a date range. | GET /api/MissionManagement |
| UC-MSN-03 | Select the mission type نوع المأمورية | Gen. Director, Staff | Loads the catalogue of mission types used to classify the assignment. | GET /api/MissionManagement/mission-types |
| UC-MSN-04 | Select the interview type نوع المقابلة | Gen. Director, Staff | Loads the interview-type catalogue recorded against the mission outcome. | GET /api/LookupManagement/mission-interview-types |
| UC-MSN-05 | Select the time type التوقيت | Gen. Director, Staff | Loads the time-classification catalogue (for example morning / evening / full day) for the mission. | GET /api/MissionManagement/mission-time-types |
| UC-MSN-06 | Create a mission تسجيل المأمورية | Gen. Director, Staff | Records the assignment — purpose, type, destination charity, dates, staff and expected outcome. | Route `#/missions/:id/edit` → POST /api/MissionManagement |
| UC-MSN-07 | View / update a mission تعديل المأمورية | Gen. Director, Staff | Loads a mission by id and saves the outcome and observations after execution. | GET /api/MissionManagement/{id} PUT /api/MissionManagement |
| UC-MSN-08 | Delete a mission حذف المأمورية | Gen. Director | Removes a mission record. | DELETE /api/MissionManagement |
| UC-MSN-09 | Register a mission result تسجيل نتيجة المأمورية | Gen. Director, Staff | Dedicated entry point used by the mission-registration screen to submit the completed mission with its findings. | Route `#/missions/:id/register` → POST /api/MissionManagement/{id}/event |

### 20.S  Screen field specifications

Derived from the AngularJS views of this module. For every screen the table lists each data-entry field in the order it appears, the scope property it binds to, its control type, whether the form marks it mandatory, and the lookup or rule that governs it. Labels are reproduced as they appear on screen (Arabic, right-to-left).

#### 20.S.1  Screen `#/missions`


| Property | Value |
| --- | --- |
| Angular route | `#/missions` |
| Feature module | `missions` (lazy-loaded) |
| Component | `MissionListComponent` |
| Route status | implemented |
| Data-entry fields | 3 |
| Grids on the screen | 1 |
| Commands | 8 |


| Section | Field (as labelled) | Bound to | Control | Mandatory · source · rules |
| --- | --- | --- | --- | --- |
| — | الجمعية | CharityId | Drop-down list | Optional · options: lookup: Charities (+ كافة الجهات) · on change: getCharityData() |
| — | الي تاريخ | DateTo | Date picker | Optional |
| — | من تاريخ | DateFrom | Date picker | Optional |

Grids on this screen:

| Grid (row source) | Columns |
| --- | --- |
| incoming in Missions   track by $index | رقم المأموريه · نوع المأموريه · الجهه · اسم القائم · الهدف · التفاصيل · التاريخ · المحافظه · الحي · الموقع · حاله المأموريه · النتيجه · الاجراءات |

Commands on this screen:

| Command | Handler | Shown when |
| --- | --- | --- |
| بحث | GetData() | always |
| حفظ | DeleteIncoming() | always |
| (icon only) | ExtractIncomingData() | always |
| (icon only) | EditMission(incoming.Id) | always |
| (icon only) | ViewDeleteModel(incoming.Id) | always |
| (icon only) | RegisterMission(incoming.Id) | always |
| (icon only) | GetNext() | always |
| (icon only) | GetPrev() | always |

#### 20.S.2  Screen `#/missions/:id/edit`


| Property | Value |
| --- | --- |
| Angular route | `#/missions/:id/edit` |
| Feature module | `missions` (lazy-loaded) |
| Component | `MissionFormComponent` |
| Route status | implemented |
| Data-entry fields | 15 |
| Grids on the screen | 0 |
| Commands | 1 |


| Section | Field (as labelled) | Bound to | Control | Mandatory · source · rules |
| --- | --- | --- | --- | --- |
| إضافة مأموريه | التاريخ | Mission.MissionDate | Date picker | Mandatory |
| إضافة مأموريه | نوع المأموريه | Mission.MissionTypeId | Drop-down list | Mandatory · options: lookup: MissionTypes |
| إضافة مأموريه | اسم الجهة | Mission.EntityName | Text box | Mandatory |
| إضافة مأموريه | نوع المقابلة | Mission.MissionInterviewTypeId | Drop-down list | Mandatory · options: lookup: MissionInterviewTypes |
| إضافة مأموريه | اسم الجهة المنظمة | Mission.EntityName | Text box | Mandatory |
| إضافة مأموريه | اسم المؤتمر | Mission.ConferenceName | Text box | Mandatory |
| إضافة مأموريه | المهمه | Mission.Details | Text box | Mandatory |
| إضافة مأموريه | هدف المأموريه | Mission.MissionTarget | Text box | Mandatory |
| إضافة مأموريه | تفاصيل المأموريه | Mission.MissionDetails | Text box | Mandatory |
| إضافة مأموريه | نوع توقيت المأموريه | Mission.MissionTimeTypeId | Drop-down list | Mandatory · options: lookup: MissionTimeTypes |
| إضافة مأموريه | موقع المأموريه | Mission.MissionLocation | Text box | Mandatory |
| إضافة مأموريه | الموظف المسئول | Mission.UserId | Drop-down list | Mandatory · options: lookup: allEmployees |
| إضافة مأموريه | القرية / الحي | Mission.Village | Text box | Mandatory |
| إضافة مأموريه | المركز/ المدينة | Mission.CenterId | Drop-down list | Mandatory · options: lookup: Centers |
| إضافة مأموريه | المنطقة /المحافظة | Mission.GovernmentId | Drop-down list | Mandatory · options: lookup: Regions · on change: Getcenters() |

Commands on this screen:

| Command | Handler | Shown when |
| --- | --- | --- |
| حفظ | SubmitMission() | always |

#### 20.S.3  Screen `#/missions/:id/register`


| Property | Value |
| --- | --- |
| Angular route | `#/missions/:id/register` |
| Feature module | `missions` (lazy-loaded) |
| Component | `MissionRegisterComponent` |
| Route status | planned |
| Data-entry fields | 17 |
| Grids on the screen | 0 |
| Commands | 1 |


| Section | Field (as labelled) | Bound to | Control | Mandatory · source · rules |
| --- | --- | --- | --- | --- |
| Reg مأموريه | نوع المأموريه | Mission.MissionTypeId | Drop-down list | Mandatory · options: lookup: MissionTypes · read-only |
| Reg مأموريه | اسم الجهة | Mission.EntityName | Text box | Mandatory · read-only |
| Reg مأموريه | نوع المقابلة | Mission.MissionInterviewTypeId | Drop-down list | Mandatory · options: lookup: MissionInterviewTypes · read-only |
| Reg مأموريه | اسم الجهة المنظمة | Mission.EntityName | Text box | Mandatory |
| Reg مأموريه | اسم المؤتمر | Mission.ConferenceName | Text box | Mandatory |
| Reg مأموريه | المهمه | Mission.Details | Text box | Mandatory |
| Reg مأموريه | هدف المأموريه | Mission.MissionTarget | Text box | Mandatory |
| Reg مأموريه | تفاصيل المأموريه | Mission.MissionDetails | Text box | Mandatory |
| Reg مأموريه | نوع توقيت المأموريه | Mission.MissionTimeTypeId | Drop-down list | Mandatory · options: lookup: MissionTimeTypes · read-only |
| Reg مأموريه | موقع المأموريه | Mission.MissionLocation | Text box | Mandatory |
| Reg مأموريه | الموظف المسئول | Mission.UserId | Drop-down list | Mandatory · options: lookup: allEmployees |
| Reg مأموريه | القرية / الحي | Mission.Village | Text box | Mandatory |
| Reg مأموريه | المركز/ المدينة | Mission.CenterId | Drop-down list | Mandatory · options: lookup: Centers · read-only |
| Reg مأموريه | المنطقة /المحافظة | Mission.GovernmentId | Drop-down list | Mandatory · options: lookup: Regions · on change: Getcenters(); read-only |
| Reg مأموريه | (unlabelled) | MissionNotCompleted | Check box | Optional · on change: DisableMissionCompleted() |
| Reg مأموريه | اتمام المامورية | MissionCompleted | Check box | Optional · on change: DisableMissionNotCompleted() |
| Reg مأموريه | السبب | Mission.MissionCompletedTxt | Text box | Mandatory |

Commands on this screen:

| Command | Handler | Shown when |
| --- | --- | --- |
| حفظ | SubmitMission() | always |

### 20.U  Use case scenarios

One expanded scenario for every use case of this module. Pre-conditions, flows and post-conditions are derived from the screen that hosts the function, the Web API action that serves it and the business-layer method that executes it; quoted messages are the literal strings returned by the business layer.

#### 20.U.1  UC-MSN-01 — List missions قائمة المأموريات


| Item | Specification |
| --- | --- |
| Use case ID | UC-MSN-01 |
| Name | List missions قائمة المأموريات |
| Type | Browse a list |
| Primary actor | Gen. Director, Staff |
| Secondary actors | The system (Web API + business layer); the database. |
| Summary | Shows the mission register with destination, type, dates and assigned staff. |
| Trigger | The actor opens the screen at `#/missions` from the module menu. |
| Pre-conditions | 1. The actor is authenticated; the JWT access token is valid and the client holds the role, charity and country claims it carries.<br>2. The actor holds one of: Gen. Director, Staff.<br>3. The SPA route `#/missions` has loaded and its reference-data lookups have been populated. |
| Main flow | 1. The actor navigates to the screen at `#/missions`.<br>2. The system loads the filter lookups the screen offers and, for HQ roles, the charity selector.<br>3. The SPA issues `GET /api/MissionManagement/my-missions`.<br>4. `MissionManagementController` binds the typed request DTO and delegates to the application service.<br>5. `IMissionService` validates the payload with its FluentValidation validator, applies the business rules and the charity scope, and persists through `IUnitOfWork`.<br>6. The result set is scoped to the caller’s charity and country, ordered and paged.<br>7. The SPA renders the page with the columns listed in the screen specification and the paging control. |
| Alternate flows | • No row matches the criteria — the grid renders empty and the paging control reports zero pages. |
| Exception flows | • The session has expired or the role is not permitted — the request is rejected and the SPA routes back to the login state. |
| Post-conditions | • No stored data is changed — the operation is a read. |
| Realisation | Route `#/missions` → `MissionListComponent`<br>`GET /api/MissionManagement/my-missions` → `MissionManagementController` → `IMissionService` |

#### 20.U.2  UC-MSN-02 — Filter missions by date البحث بالتاريخ


| Item | Specification |
| --- | --- |
| Use case ID | UC-MSN-02 |
| Name | Filter missions by date البحث بالتاريخ |
| Type | Search / filter |
| Primary actor | Gen. Director, Staff |
| Secondary actors | The system (Web API + business layer); the database. |
| Summary | Restricts the mission register to a date range. |
| Trigger | The actor selects the function from the module menu or the screen that hosts it. |
| Pre-conditions | 1. The actor is authenticated; the JWT access token is valid and the client holds the role, charity and country claims it carries.<br>2. The actor holds one of: Gen. Director, Staff. |
| Main flow | 1. The actor reaches the function from the screen of this module that hosts it.<br>2. The actor enters the search criteria in the filter fields.<br>3. The actor presses the command button of the screen.<br>4. The SPA issues `GET /api/MissionManagement` carrying DateFrom, DateTo.<br>5. `MissionManagementController` binds the typed request DTO and delegates to the application service.<br>6. `IMissionService` validates the payload with its FluentValidation validator, applies the business rules and the charity scope, and persists through `IUnitOfWork`.<br>7. The matching rows are returned, scoped to the caller’s charity, and rendered in the result grid. |
| Alternate flows | • No row matches the criteria — the grid renders empty and the paging control reports zero pages. |
| Exception flows | • The session has expired or the role is not permitted — the request is rejected and the SPA routes back to the login state. |
| Post-conditions | • No stored data is changed — the operation is a read. |
| Realisation | `GET /api/MissionManagement` → `MissionManagementController` → `IMissionService` |

#### 20.U.3  UC-MSN-03 — Select the mission type نوع المأمورية


| Item | Specification |
| --- | --- |
| Use case ID | UC-MSN-03 |
| Name | Select the mission type نوع المأمورية |
| Type | Read a record |
| Primary actor | Gen. Director, Staff |
| Secondary actors | The system (Web API + business layer); the database. |
| Summary | Loads the catalogue of mission types used to classify the assignment. |
| Trigger | The actor selects the function from the module menu or the screen that hosts it. |
| Pre-conditions | 1. The actor is authenticated; the JWT access token is valid and the client holds the role, charity and country claims it carries.<br>2. The actor holds one of: Gen. Director, Staff.<br>3. The target record exists and its identifier is known to the screen. |
| Main flow | 1. The actor reaches the function from the screen of this module that hosts it.<br>2. The actor selects the record to open.<br>3. The SPA issues `GET /api/MissionManagement/mission-types`.<br>4. `MissionManagementController` binds the typed request DTO and delegates to the application service.<br>5. `IMissionService` validates the payload with its FluentValidation validator, applies the business rules and the charity scope, and persists through `IUnitOfWork`.<br>6. The system returns the record with its child collections and the SPA binds them to the form fields. |
| Alternate flows | None recorded. |
| Exception flows | • The session has expired or the role is not permitted — the request is rejected and the SPA routes back to the login state. |
| Post-conditions | • No stored data is changed — the operation is a read. |
| Realisation | `GET /api/MissionManagement/mission-types` → `MissionManagementController` → `IMissionService` |

#### 20.U.4  UC-MSN-04 — Select the interview type نوع المقابلة


| Item | Specification |
| --- | --- |
| Use case ID | UC-MSN-04 |
| Name | Select the interview type نوع المقابلة |
| Type | Read a record |
| Primary actor | Gen. Director, Staff |
| Secondary actors | The system (Web API + business layer); the database. |
| Summary | Loads the interview-type catalogue recorded against the mission outcome. |
| Trigger | The actor selects the function from the module menu or the screen that hosts it. |
| Pre-conditions | 1. The actor is authenticated; the JWT access token is valid and the client holds the role, charity and country claims it carries.<br>2. The actor holds one of: Gen. Director, Staff.<br>3. The target record exists and its identifier is known to the screen. |
| Main flow | 1. The actor reaches the function from the screen of this module that hosts it.<br>2. The actor selects the record to open.<br>3. The SPA issues `GET /api/LookupManagement/mission-interview-types`.<br>4. `MissionManagementController` binds the typed request DTO and delegates to the application service.<br>5. `IMissionService` validates the payload with its FluentValidation validator, applies the business rules and the charity scope, and persists through `IUnitOfWork`.<br>6. The system returns the record with its child collections and the SPA binds them to the form fields. |
| Alternate flows | None recorded. |
| Exception flows | • The session has expired or the role is not permitted — the request is rejected and the SPA routes back to the login state. |
| Post-conditions | • No stored data is changed — the operation is a read. |
| Realisation | `GET /api/LookupManagement/mission-interview-types` → `LookupManagementController` → `ILookupService` |

#### 20.U.5  UC-MSN-05 — Select the time type التوقيت


| Item | Specification |
| --- | --- |
| Use case ID | UC-MSN-05 |
| Name | Select the time type التوقيت |
| Type | Read a record |
| Primary actor | Gen. Director, Staff |
| Secondary actors | The system (Web API + business layer); the database. |
| Summary | Loads the time-classification catalogue (for example morning / evening / full day) for the mission. |
| Trigger | The actor selects the function from the module menu or the screen that hosts it. |
| Pre-conditions | 1. The actor is authenticated; the JWT access token is valid and the client holds the role, charity and country claims it carries.<br>2. The actor holds one of: Gen. Director, Staff.<br>3. The target record exists and its identifier is known to the screen. |
| Main flow | 1. The actor reaches the function from the screen of this module that hosts it.<br>2. The actor selects the record to open.<br>3. The SPA issues `GET /api/MissionManagement/mission-time-types`.<br>4. `MissionManagementController` binds the typed request DTO and delegates to the application service.<br>5. `IMissionService` validates the payload with its FluentValidation validator, applies the business rules and the charity scope, and persists through `IUnitOfWork`.<br>6. The system returns the record with its child collections and the SPA binds them to the form fields. |
| Alternate flows | None recorded. |
| Exception flows | • The session has expired or the role is not permitted — the request is rejected and the SPA routes back to the login state. |
| Post-conditions | • No stored data is changed — the operation is a read. |
| Realisation | `GET /api/MissionManagement/mission-time-types` → `MissionManagementController` → `IMissionService` |

#### 20.U.6  UC-MSN-06 — Create a mission تسجيل المأمورية


| Item | Specification |
| --- | --- |
| Use case ID | UC-MSN-06 |
| Name | Create a mission تسجيل المأمورية |
| Type | Create a record |
| Primary actor | Gen. Director, Staff |
| Secondary actors | The system (Web API + business layer); the database. |
| Summary | Records the assignment — purpose, type, destination charity, dates, staff and expected outcome. |
| Trigger | The actor presses «حفظ» on the screen AddMission. |
| Pre-conditions | 1. The actor is authenticated; the JWT access token is valid and the client holds the role, charity and country claims it carries.<br>2. The actor holds one of: Gen. Director, Staff.<br>3. The SPA route `#/missions/:id/edit` has loaded and its reference-data lookups have been populated. |
| Main flow | 1. The actor navigates to the screen at `#/missions/:id/edit`. The screen opens in add mode.<br>2. The system loads the reference data the form needs (regions, centres, banks, types and the other lookup lists bound to the drop-downs).<br>3. The actor completes the input fields. Mandatory fields: التاريخ، نوع المأموريه، اسم الجهة، نوع المقابلة، اسم الجهة المنظمة، اسم المؤتمر، المهمه، هدف المأموريه، تفاصيل المأموريه، نوع توقيت المأموريه، موقع المأموريه، الموظف المسئول، القرية / الحي، المركز/ المدينة ….<br>4. The actor presses «حفظ» (SubmitMission()).<br>5. The SPA issues `POST /api/MissionManagement` carrying Mission obj.<br>6. `MissionManagementController` binds the typed request DTO and delegates to the application service.<br>7. `IMissionService` validates the payload with its FluentValidation validator, applies the business rules and the charity scope, and persists through `IUnitOfWork`.<br>8. The business layer validates the payload, stamps the owning charity and the creating user, and persists the new record.<br>9. The system returns the outcome and the SPA confirms the save and returns to the list screen. |
| Alternate flows | • The actor abandons the form before saving — nothing is written and the record keeps its previous state. |
| Exception flows | • The session has expired or the role is not permitted — the request is rejected and the SPA routes back to the login state.<br>• A mandatory field is empty or fails its format check — the save is refused and the field is flagged on the form. |
| Post-conditions | • A new record exists, owned by the charity of the creating user, and appears in the list screen of the module. |
| Realisation | Route `#/missions/:id/edit` → `MissionFormComponent`<br>`POST /api/MissionManagement` → `MissionManagementController` → `IMissionService` |

#### 20.U.7  UC-MSN-07 — View / update a mission تعديل المأمورية


| Item | Specification |
| --- | --- |
| Use case ID | UC-MSN-07 |
| Name | View / update a mission تعديل المأمورية |
| Type | Update a record |
| Primary actor | Gen. Director, Staff |
| Secondary actors | The system (Web API + business layer); the database. |
| Summary | Loads a mission by id and saves the outcome and observations after execution. |
| Trigger | The actor selects the function from the module menu or the screen that hosts it. |
| Pre-conditions | 1. The actor is authenticated; the JWT access token is valid and the client holds the role, charity and country claims it carries.<br>2. The actor holds one of: Gen. Director, Staff.<br>3. The target record exists and its identifier is known to the screen. |
| Main flow | 1. The actor reaches the function from the screen of this module that hosts it. The screen opens in edit mode on the selected record.<br>2. The system loads the current values into the form.<br>3. The actor changes the fields to be corrected.<br>4. The actor presses the command button of the screen.<br>5. The SPA issues `GET /api/MissionManagement/{id}` carrying id.<br>6. `MissionManagementController` binds the typed request DTO and delegates to the application service.<br>7. `IMissionService` validates the payload with its FluentValidation validator, applies the business rules and the charity scope, and persists through `IUnitOfWork`.<br>8. The business layer re-validates the payload, applies the changes and persists them.<br>9. The system returns the outcome and the SPA refreshes the screen. |
| Alternate flows | • The actor abandons the form before saving — nothing is written and the record keeps its previous state. |
| Exception flows | • The session has expired or the role is not permitted — the request is rejected and the SPA routes back to the login state.<br>• A mandatory field is empty or fails its format check — the save is refused and the field is flagged on the form. |
| Post-conditions | • The stored record carries the new values; no other record is affected. |
| Realisation | `GET /api/MissionManagement/{id}` · `PUT /api/MissionManagement` → `MissionManagementController` → `IMissionService` |

#### 20.U.8  UC-MSN-08 — Delete a mission حذف المأمورية


| Item | Specification |
| --- | --- |
| Use case ID | UC-MSN-08 |
| Name | Delete a mission حذف المأمورية |
| Type | Delete a record |
| Primary actor | Gen. Director |
| Secondary actors | The system (Web API + business layer); the database. |
| Summary | Removes a mission record. |
| Trigger | The actor selects the function from the module menu or the screen that hosts it. |
| Pre-conditions | 1. The actor is authenticated; the JWT access token is valid and the client holds the role, charity and country claims it carries.<br>2. The actor holds one of: Gen. Director.<br>3. The target record exists and its identifier is known to the screen. |
| Main flow | 1. The actor reaches the function from the screen of this module that hosts it.<br>2. The actor selects the record and requests its deletion.<br>3. The SPA asks the actor to confirm.<br>4. The SPA issues `DELETE /api/MissionManagement` carrying id.<br>5. `MissionManagementController` binds the typed request DTO and delegates to the application service.<br>6. `IMissionService` validates the payload with its FluentValidation validator, applies the business rules and the charity scope, and persists through `IUnitOfWork`.<br>7. The business layer checks that the record may still be removed and deletes it (or marks it removed).<br>8. The system returns the outcome and the SPA drops the row from the grid. |
| Alternate flows | None recorded. |
| Exception flows | • The session has expired or the role is not permitted — the request is rejected and the SPA routes back to the login state. |
| Post-conditions | • The record is no longer returned by the list and read endpoints of the module. |
| Realisation | `DELETE /api/MissionManagement` → `MissionManagementController` → `IMissionService` |

#### 20.U.9  UC-MSN-09 — Register a mission result تسجيل نتيجة المأمورية


| Item | Specification |
| --- | --- |
| Use case ID | UC-MSN-09 |
| Name | Register a mission result تسجيل نتيجة المأمورية |
| Type | Create a record |
| Primary actor | Gen. Director, Staff |
| Secondary actors | The system (Web API + business layer); the database. |
| Summary | Dedicated entry point used by the mission-registration screen to submit the completed mission with its findings. |
| Trigger | The actor presses «حفظ» on the screen RegisterMission. |
| Pre-conditions | 1. The actor is authenticated; the JWT access token is valid and the client holds the role, charity and country claims it carries.<br>2. The actor holds one of: Gen. Director, Staff.<br>3. The SPA route `#/missions/:id/register` has loaded and its reference-data lookups have been populated. |
| Main flow | 1. The actor navigates to the screen at `#/missions/:id/register`. The screen opens in add mode.<br>2. The system loads the reference data the form needs (regions, centres, banks, types and the other lookup lists bound to the drop-downs).<br>3. The actor completes the input fields. Mandatory fields: نوع المأموريه، اسم الجهة، نوع المقابلة، اسم الجهة المنظمة، اسم المؤتمر، المهمه، هدف المأموريه، تفاصيل المأموريه، نوع توقيت المأموريه، موقع المأموريه، الموظف المسئول، القرية / الحي، المركز/ المدينة، المنطقة /المحافظة ….<br>4. The actor presses «حفظ» (SubmitMission()).<br>5. The SPA issues `POST /api/MissionManagement/{id}/event` carrying Mission obj.<br>6. `MissionManagementController` binds the typed request DTO and delegates to the application service.<br>7. `IMissionService` validates the payload with its FluentValidation validator, applies the business rules and the charity scope, and persists through `IUnitOfWork`.<br>8. The business layer validates the payload, stamps the owning charity and the creating user, and persists the new record.<br>9. The system returns the outcome and the SPA confirms the save and returns to the list screen. |
| Alternate flows | • The actor abandons the form before saving — nothing is written and the record keeps its previous state. |
| Exception flows | • The session has expired or the role is not permitted — the request is rejected and the SPA routes back to the login state.<br>• A mandatory field is empty or fails its format check — the save is refused and the field is flagged on the form. |
| Post-conditions | • A new record exists, owned by the charity of the creating user, and appears in the list screen of the module. |
| Realisation | Route `#/missions/:id/register` → `MissionRegisterComponent`<br>`POST /api/MissionManagement/{id}/event` → `MissionManagementController` → `IMissionService` |

### 20.A  Annex - Angular routes of this module

Routes are hash-based (`useHash: true`), rendered inside `MainLayoutComponent` behind
`AuthGuard` and `PermissionGuard`. *implemented* means the route exists in
`Frontend/src/app`; *planned* means it is specified here and not yet built.

| Angular route | Feature module | Component | Status |
| --- | --- | --- | --- |
| `#/missions` | `missions` | `MissionListComponent` | implemented |
| `#/missions/:id/edit` | `missions` | `MissionFormComponent` | implemented |
| `#/missions/:id/register` | `missions` | `MissionRegisterComponent` | planned |

### 20.B  Annex - API controllers of this module

Every controller inherits `ApiController` (`[Authorize]`, route `api/[controller]`) and
returns `Framework.Core.ApiResponse<T>`. Business rules live in the application service
behind each controller, never in the controller itself.

| Controller | Base route | Responsibility |
| --- | --- | --- |
| `MissionManagementController` | `api/MissionManagement` | Mission CRUD, type/interview/time catalogues, date filter. Mission result registration. |

---

Shared context - system architecture, actors and roles, the sponsorship lifecycle, the full module map and all appendices - is in the companion document [00-Overview-and-Common-Context](00-Overview-and-Common-Context.md). The delivery backlog for this module is epic EP-15 in [00-EPICS-and-User-Stories](00-EPICS-and-User-Stories.md).

