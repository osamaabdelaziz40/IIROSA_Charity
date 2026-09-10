# WAR.IIROSA - Charity Administration

ادارة الجمعيات | use case prefix `UC-CHR` | chapter 8 of the master document


| Field | Value |
| --- | --- |
| Document title | WAR.IIROSA - Module Specification: Charity Administration |
| Module | Charity Administration - ادارة الجمعيات |
| Use case prefix | UC-CHR |
| Chapter in master document | Chapter 8 |
| Documented use cases | 9 |
| Principal routes | `#/charities`, `#/charities/create`, `#/charities/:id`, `#/charities/:id/edit`, `#/charities/profile`, `#/charities/my-profile` |
| Version | 1.1 |
| Status | Chapter content extracted verbatim; screen fields and scenarios derived from the source code |
| Date | 18 August 2026 |
| Parent document | WAR.IIROSA-Project-Documentation.docx v1.0 (repository WAR.IIROSA, commit 4beb736) |

## Contents of this document

1. Chapter 8 — module purpose and use-case catalogue (verbatim from the master document)
2. §8.S — screen field specifications (every field of every screen, derived from the AngularJS views)
3. §8.U — one expanded scenario per use case (trigger, pre-conditions, main flow, alternates, exceptions, post-conditions)
4. §8.A / §8.B — annexes: screens and Web API controllers of this module

## 8. Charity Administration

إدارة الجمعيات — HQ onboards partner charities, maintains their profile, and controls their operational permissions. This module is restricted to the General Director.


| ID | Use case | Primary actor | Description & main flow | Realisation |
| --- | --- | --- | --- | --- |
| UC-CHR-01 | List all charities قائمة الجمعيات | General Director | Displays every registered charity with its country, contact data and current status flags, as the entry point to all charity administration actions. A statistics band above the grid summarises the caller-scoped register — totals, active/inactive/locked counts, donations-receiving count, added-this-month count and a per-country breakdown (re-platform extension, 2026-09-10). | Route `#/charities` → GET /api/Charities · GET /api/Charities/statistics |
| UC-CHR-02 | Verify charity name availability التحقق من اسم الجمعية | General Director | While the name is typed on the creation form, the system checks that no charity already uses it and enables or blocks submission accordingly. | GET /api/Charities/check-name |
| UC-CHR-03 | Create a charity اضافة جمعية | General Director | Captures name, login credentials, country, region, centre, address and contact details; creates the membership user with the charity role, then the charity record linked to that user. All data the charity later creates is owned by this record. | Route `#/charities/create` → POST /api/Charities → ICharityService |
| UC-CHR-04 | View a charity profile بيانات الجمعية | General Director | Retrieves the full profile of one charity by name for review or editing. | GET /api/Charities/{id} |
| UC-CHR-05 | Update a charity profile تعديل بيانات الجمعية | General Director | Applies changes to the charity's descriptive and contact data. The call is authorised against the acting user id. | PUT /api/Charities/{id} |
| UC-CHR-06 | Open the permissions screen الصلاحيات | General Director | Lists charities together with their three independent control flags — account locked, data entry enabled, data editing enabled — so that they can be toggled. | GET /api/Charities |
| UC-CHR-07 | Lock or unlock a charity account إيقاف / تفعيل حساب الجمعية | General Director | Sets the membership lock-out flag for the charity's login. Unlocking additionally resets the failed-attempt counter and the lock-out timestamps so the account can sign in immediately. Alternate: a non-administrator caller is rejected. | POST /api/Charities/{id}/lock · POST /api/Charities/{id}/unlock |
| UC-CHR-08 | Enable or disable data entry for a charity السماح بالإضافة | General Director | Switches the charity's IsAddEnabled flag. When disabled the charity may still sign in and view its data but cannot create new families, orphans or reports — used to freeze registration between campaigns. | PUT /api/Charities/{id}/update-rights |
| UC-CHR-09 | Enable or disable data editing for a charity السماح بالتعديل | General Director | Switches the charity's IsUpdateEnabled flag, freezing existing records against modification — typically applied once a payment batch has been built from the data. | PUT /api/Charities/{id}/update-rights |

### 8.S  Screen field specifications

Derived from the AngularJS views of this module. For every screen the table lists each data-entry field in the order it appears, the scope property it binds to, its control type, whether the form marks it mandatory, and the lookup or rule that governs it. Labels are reproduced as they appear on screen (Arabic, right-to-left).

#### 8.S.1  Screen `#/charities`


| Property | Value |
| --- | --- |
| Angular route | `#/charities` |
| Feature module | `charities` (lazy-loaded) |
| Component | `CharityListComponent` |
| Route status | implemented |
| Data-entry fields | 34 |
| Grids on the screen | 1 |
| Commands | 11 |

Statistics band (re-platform extension 2026-09-10 — not derived from the legacy view; served by `GET /api/Charities/statistics`): six read-only cards above the filters — إجمالي الجمعيات (TotalCharities) · جمعيات نشطة (ActiveCharities) · جمعيات غير نشطة (InactiveCharities) · جمعيات مقفلة (LockedCharities) · تستقبل تبرعات (ReceivingDonations) · أُضيفت هذا الشهر (AddedThisMonth, CreatedOn since the first of the current UTC month) — plus a per-country pill row (NameAr/NameEn + count, highest first; hidden when empty). The band is caller-scoped by the same ladder as the list (charity caller → own record only; country-pinned HQ → own country) and is register-scoped, not filter-reactive; it refreshes after activate/deactivate, lock/unlock and delete.


| Section | Field (as labelled) | Bound to | Control | Mandatory · source · rules |
| --- | --- | --- | --- | --- |
| تعديل الرقم السرى | * اعادة كتبة الرقم السرى | NewPassword | Password box | Optional |
| تعديل الرقم السرى | من فضلك ادخل كتابة كل البيانات | ConfirmNewPassword | Password box | Optional |
| تعديل الجمعية | كود الجمعية | Charity.CharityType | Drop-down list | Mandatory · options: جمعيه / مؤسسه / لجنه |
| تعديل الجمعية | اسم الجمعية | Charity.Code | Text box | Mandatory |
| تعديل الجمعية | تاريخ الاشهار | Charity.Name | Text box | Mandatory |
| تعديل الجمعية | رقم الاشهار | Charity.AdvertisingDate | Date picker | Mandatory |
| تعديل الجمعية | المركز/ المدينة | Charity.AdvertisingNumber | Numeric box | Mandatory |
| تعديل الجمعية | المنطقة /المحافظة | Charity.CenterId | Drop-down list | Mandatory · options: lookup: Centers |
| تعديل الجمعية | العنوان | Charity.RegionId | Drop-down list | Mandatory · options: lookup: Regions · on change: GetCenters() |
| تعديل الجمعية | اسم الشارع | Charity.Address | Text box | Mandatory |
| تعديل الجمعية | القريه | Charity.StreetName | Text box | Mandatory |
| تعديل الجمعية | الرمز البريدي | Charity.Village | Text box | Mandatory |
| تعديل الجمعية | رقم الفاكس | Charity.PostalCode | Text box | Mandatory |
| تعديل الجمعية | رقم الموبايل 2 | Charity.Fax | Text box | Mandatory |
| تعديل الجمعية | رقم الموبايل 1 | Charity.Phone2 | Text box | Mandatory |
| تعديل الجمعية | رقم الهاتف الارضي | Charity.Phone | Text box | Mandatory |
| تعديل الجمعية | رقم الحساب البنكي | Charity.HomePhone | Text box | Mandatory |
| تعديل الجمعية | البنك | Charity.BankAccount | Text box | Mandatory |
| تعديل الجمعية | البريد الالكتروني | Charity.BankId | Drop-down list | Mandatory · options: lookup: Banks |
| تعديل الجمعية | بعد الجمعية عن المكتب(الزمن/ساعة) | Charity.Email | Text box | Mandatory |
| تعديل الجمعية | بعد الجمعية عن المكتب (كم) | Charity.FarTime | Text box | Mandatory |
| تعديل الجمعية | الايبان | Charity.FarDistance | Numeric box | Mandatory |
| تعديل الجمعية | وسيله المواصلات | Charity.IBAN | Text box | Mandatory |
| تعديل الجمعية | احداثيات موقع الجمعية | Charity.Transportation | Text box | Mandatory |
| تعديل الجمعية | رقم موبايل 2 | Charity.MapLocation | Text box | Mandatory |
| تعديل الجمعية | رقم موبايل 1 | Charity.BossPhone2 | Text box | Mandatory |
| تعديل الجمعية | المسمى الوظيفي | Charity.BossPhone1 | Text box | Mandatory |
| تعديل الجمعية | اسم الرئيس | Charity.BossJobName | Text box | Mandatory |
| تعديل الجمعية | رقم موبايل 2 | Charity.BossName | Text box | Mandatory |
| تعديل الجمعية | رقم موبايل 1 | Charity.ResponsiblePhone2 | Text box | Mandatory |
| تعديل الجمعية | المسمى الوظيفي | Charity.ResponsiblePhone1 | Text box | Mandatory |
| تعديل الجمعية | اسم مسؤول الايتام | Charity.ResponsibleJobName | Text box | Mandatory |
| تعديل الجمعية | الملاحظات | Charity.UserName | Text box | Mandatory |
| تعديل الجمعية | من فضلك ادخل كتابة كل البيانات | Charity.Notes | Text box | Optional |

Grids on this screen:

| Grid (row source) | Columns |
| --- | --- |
| Charities | الجمعية · الكود · تعديل · التقرير · إيقاف الجمعية · إيقاف الادخلات · إيقاف تحديث البيانات · تغيير كلمة السر |

Commands on this screen:

| Command | Handler | Shown when |
| --- | --- | --- |
| غلق | EmptyChangepssword() | always |
| تم | ChangePasswordConfirmation() | always |
| غلق | EmptyEditCharity() | always |
| تم | EditCharityConfirmation() | always |
| (icon only) | ExtractAllCharities() | always |
| (icon only) | editCharity($index) | always |
| (icon only) | ExtractCharityReport($index) | always |
| (icon only) | stopCharity($event,charity.Id) | always |
| (icon only) | StopEnteringData($event,charity.Id) | always |
| (icon only) | StopUpdateCharity($event,charity.Id) | always |
| (icon only) | ChangePassword($index) | always |

#### 8.S.2  Screen `#/charities/create`


| Property | Value |
| --- | --- |
| Angular route | `#/charities/create` |
| Feature module | `charities` (lazy-loaded) |
| Component | `CharityFormComponent` |
| Route status | implemented |
| Data-entry fields | 35 |
| Grids on the screen | 0 |
| Commands | 1 |


| Section | Field (as labelled) | Bound to | Control | Mandatory · source · rules |
| --- | --- | --- | --- | --- |
| — | الجمعية | CharityId | Drop-down list | Optional · options: lookup: Charities (+ كافة الجهات) · on change: getCharityData() |
| إضافة جمعية | كود الجمعية | Charity.CharityType | Drop-down list | Mandatory · options: جمعيه / مؤسسه / لجنه |
| إضافة جمعية | اسم الجمعية | Charity.Code | Text box | Mandatory |
| إضافة جمعية | تاريخ الاشهار | Charity.Name | Text box | Mandatory |
| إضافة جمعية | رقم الاشهار | Charity.AdvertisingDate | Date picker | Mandatory |
| إضافة جمعية | المركز/ المدينة | Charity.AdvertisingNumber | Numeric box | Mandatory |
| إضافة جمعية | المنطقة /المحافظة | Charity.CenterId | Drop-down list | Mandatory · options: lookup: Centers |
| إضافة جمعية | العنوان | Charity.RegionId | Drop-down list | Mandatory · options: lookup: Regions · on change: GetCenters() |
| إضافة جمعية | اسم الشارع | Charity.Address | Text box | Mandatory |
| إضافة جمعية | القريه | Charity.StreetName | Text box | Mandatory |
| إضافة جمعية | الرمز البريدي | Charity.Village | Text box | Mandatory |
| إضافة جمعية | رقم الفاكس | Charity.PostalCode | Text box | Mandatory |
| إضافة جمعية | رقم الموبايل 2 | Charity.Fax | Text box | Mandatory |
| إضافة جمعية | رقم الموبايل 1 | Charity.Phone2 | Text box | Mandatory |
| إضافة جمعية | رقم الهاتف الارضي | Charity.Phone | Text box | Mandatory |
| إضافة جمعية | رقم الحساب البنكي | Charity.HomePhone | Text box | Mandatory |
| إضافة جمعية | البنك | Charity.BankAccount | Text box | Mandatory |
| إضافة جمعية | البريد الالكتروني | Charity.BankId | Drop-down list | Mandatory · options: lookup: Banks |
| إضافة جمعية | بعد الجمعية عن المكتب(الزمن/ساعة) | Charity.Email | Text box | Mandatory |
| إضافة جمعية | بعد الجمعية عن المكتب (كم) | Charity.FarTime | Text box | Mandatory |
| إضافة جمعية | الايبان | Charity.FarDistance | Numeric box | Mandatory |
| إضافة جمعية | وسيله المواصلات | Charity.IBAN | Text box | Mandatory |
| إضافة جمعية | احداثيات موقع الجمعية | Charity.Transportation | Text box | Mandatory |
| إضافة جمعية | رقم موبايل 2 | Charity.MapLocation | Text box | Mandatory |
| إضافة جمعية | رقم موبايل 1 | Charity.BossPhone2 | Text box | Mandatory |
| إضافة جمعية | المسمى الوظيفي | Charity.BossPhone1 | Text box | Mandatory |
| إضافة جمعية | اسم الرئيس | Charity.BossJobName | Text box | Mandatory |
| إضافة جمعية | رقم موبايل 2 | Charity.BossName | Text box | Mandatory |
| إضافة جمعية | رقم موبايل 1 | Charity.ResponsiblePhone2 | Text box | Mandatory |
| إضافة جمعية | المسمى الوظيفي | Charity.ResponsiblePhone1 | Text box | Mandatory |
| إضافة جمعية | اسم مسؤول الايتام | Charity.ResponsibleJobName | Text box | Mandatory |
| إضافة جمعية | الملاحظات | Charity.UserName | Text box | Mandatory |
| إضافة جمعية | اعادة كتابة الرقم السرى | Charity.Notes | Text box | Mandatory |
| إضافة جمعية | الرقم السرى | ConfirmNewPassword | Password box | Mandatory |
| إضافة جمعية | (unlabelled) | Charity.Password | Password box | Optional |

Commands on this screen:

| Command | Handler | Shown when |
| --- | --- | --- |
| حفظ | submitCharity() | always |

### 8.U  Use case scenarios

One expanded scenario for every use case of this module. Pre-conditions, flows and post-conditions are derived from the screen that hosts the function, the Web API action that serves it and the business-layer method that executes it; quoted messages are the literal strings returned by the business layer.

#### 8.U.1  UC-CHR-01 — List all charities قائمة الجمعيات


| Item | Specification |
| --- | --- |
| Use case ID | UC-CHR-01 |
| Name | List all charities قائمة الجمعيات |
| Type | Browse a list |
| Primary actor | General Director |
| Secondary actors | The system (Web API + business layer); the database. |
| Summary | Displays every registered charity with its country, contact data and current status flags, as the entry point to all charity administration actions. A statistics band above the grid summarises the caller-scoped register. |
| Trigger | The actor opens the screen at `#/charities` from the module menu. |
| Pre-conditions | 1. The actor is authenticated; the JWT access token is valid and the client holds the role, charity and country claims it carries.<br>2. The actor holds one of: General Director.<br>3. Caller identity comes from the JWT, never from the request; the application service scopes the query to the charity (and country) that owns the user, unless the role is an HQ role permitted to pass an explicit `charityId` filter.<br>4. The SPA route `#/charities` has loaded and its reference-data lookups have been populated.<br>5. The record is not closed by an add/edit lock (IsLocked, IsLockedOut). |
| Main flow | 1. The actor navigates to the screen at `#/charities`.<br>2. The system loads the filter lookups the screen offers and, for HQ roles, the charity selector.<br>3. The SPA issues `GET /api/Charities/statistics`; the service resolves the caller scope with the same ladder as the list, aggregates the live rows in grouped passes (totals, active, locked, receiving donations, added this month, count per country) and the band renders the cards.<br>4. The SPA issues `GET /api/Charities` carrying userId.<br>5. `CharitiesController` binds the typed request DTO and delegates to the application service.<br>6. `ICharityService` validates the payload with its FluentValidation validator, applies the business rules and the charity scope, and persists through `IUnitOfWork`.<br>7. The result set is scoped to the caller’s charity and country, ordered and paged.<br>8. The SPA renders the page with the columns listed in the screen specification and the paging control. |
| Alternate flows | • An HQ role (General Director, Financial Director, Staff) may pass an explicit charity id and so read across the charity boundary; a charity user may not and always sees its own data.<br>• The statistics band is register-scoped, not filter-reactive: it describes the caller's whole visible register whatever the filters say, and refreshes only after the row mutations that move its numbers (activate/deactivate, lock/unlock, delete).<br>• The statistics request fails — the band stays hidden and the grid loads normally.<br>• No row matches the criteria — the grid renders empty and the paging control reports zero pages. |
| Exception flows | • The session has expired or the role is not permitted — the request is rejected and the SPA routes back to the login state.<br>• The charity is closed by a lock (IsLocked, IsLockedOut) — the write is refused. |
| Post-conditions | • No stored data is changed — the operation is a read. |
| Realisation | Route `#/charities` → `CharityListComponent`<br>`GET /api/Charities` → `CharitiesController` → `ICharityService`<br>`GET /api/Charities/statistics` → `CharitiesController.GetStatistics` → `ICharityService.GetStatisticsAsync` → `ICharityRepository.GetRegisterStatisticsAsync` |

#### 8.U.2  UC-CHR-02 — Verify charity name availability التحقق من اسم الجمعية


| Item | Specification |
| --- | --- |
| Use case ID | UC-CHR-02 |
| Name | Verify charity name availability التحقق من اسم الجمعية |
| Type | Read a record |
| Primary actor | General Director |
| Secondary actors | The system (Web API + business layer); the database. |
| Summary | While the name is typed on the creation form, the system checks that no charity already uses it and enables or blocks submission accordingly. |
| Trigger | The actor selects the function from the module menu or the screen that hosts it. |
| Pre-conditions | 1. The actor is authenticated; the JWT access token is valid and the client holds the role, charity and country claims it carries.<br>2. The actor holds one of: General Director.<br>3. Caller identity comes from the JWT, never from the request; the application service scopes the query to the charity (and country) that owns the user, unless the role is an HQ role permitted to pass an explicit `charityId` filter.<br>4. The target record exists and its identifier is known to the screen. |
| Main flow | 1. The actor reaches the function from the screen of this module that hosts it.<br>2. The actor selects the record to open.<br>3. The SPA issues `GET /api/Charities/check-name` carrying charityName, userId.<br>4. `CharitiesController` binds the typed request DTO and delegates to the application service.<br>5. `ICharityService` validates the payload with its FluentValidation validator, applies the business rules and the charity scope, and persists through `IUnitOfWork`.<br>6. The system returns the record with its child collections and the SPA binds them to the form fields. |
| Alternate flows | • An HQ role (General Director, Financial Director, Staff) may pass an explicit charity id and so read across the charity boundary; a charity user may not and always sees its own data. |
| Exception flows | • The session has expired or the role is not permitted — the request is rejected and the SPA routes back to the login state. |
| Post-conditions | • No stored data is changed — the operation is a read. |
| Realisation | `GET /api/Charities/check-name` → `CharitiesController` → `ICharityService` |

#### 8.U.3  UC-CHR-03 — Create a charity اضافة جمعية


| Item | Specification |
| --- | --- |
| Use case ID | UC-CHR-03 |
| Name | Create a charity اضافة جمعية |
| Type | Create a record |
| Primary actor | General Director |
| Secondary actors | The system (Web API + business layer); the database. |
| Summary | Captures name, login credentials, country, region, centre, address and contact details; creates the membership user with the charity role, then the charity record linked to that user. All data the charity later creates is owned by this record. |
| Trigger | The actor presses «حفظ» on the screen Charity. |
| Pre-conditions | 1. The actor is authenticated; the JWT access token is valid and the client holds the role, charity and country claims it carries.<br>2. The actor holds one of: General Director.<br>3. The SPA route `#/charities/create` has loaded and its reference-data lookups have been populated.<br>4. The record is not closed by an add/edit lock (IsLocked). |
| Main flow | 1. The actor navigates to the screen at `#/charities/create`. The screen opens in add mode.<br>2. The system loads the reference data the form needs (regions, centres, banks, types and the other lookup lists bound to the drop-downs).<br>3. The actor completes the input fields. Mandatory fields: كود الجمعية، اسم الجمعية، تاريخ الاشهار، رقم الاشهار، المركز/ المدينة، المنطقة /المحافظة، العنوان، اسم الشارع، القريه، الرمز البريدي، رقم الفاكس، رقم الموبايل 2، رقم الموبايل 1، رقم الهاتف الارضي ….<br>4. The actor presses «حفظ» (submitCharity()).<br>5. The SPA issues `POST /api/Charities` carrying typed request DTO obj.<br>6. `CharitiesController` binds the typed request DTO and delegates to the application service.<br>7. `ICharityService` validates the payload with its FluentValidation validator, applies the business rules and the charity scope, and persists through `IUnitOfWork`.<br>8. The business layer validates the payload, stamps the owning charity and the creating user, and persists the new record.<br>9. The system returns the outcome and the SPA confirms the save and returns to the list screen. |
| Alternate flows | • The actor abandons the form before saving — nothing is written and the record keeps its previous state. |
| Exception flows | • The session has expired or the role is not permitted — the request is rejected and the SPA routes back to the login state.<br>• A mandatory field is empty or fails its format check — the save is refused and the field is flagged on the form.<br>• The charity is closed by a lock (IsLocked) — the write is refused. |
| Post-conditions | • A new record exists, owned by the charity of the creating user, and appears in the list screen of the module. |
| Realisation | Route `#/charities/create` → `CharityFormComponent`<br>`POST /api/Charities` → `CharitiesController` → `ICharityService` |

#### 8.U.4  UC-CHR-04 — View a charity profile بيانات الجمعية


| Item | Specification |
| --- | --- |
| Use case ID | UC-CHR-04 |
| Name | View a charity profile بيانات الجمعية |
| Type | Read a record |
| Primary actor | General Director |
| Secondary actors | The system (Web API + business layer); the database. |
| Summary | Retrieves the full profile of one charity by name for review or editing. |
| Trigger | The actor selects the function from the module menu or the screen that hosts it. |
| Pre-conditions | 1. The actor is authenticated; the JWT access token is valid and the client holds the role, charity and country claims it carries.<br>2. The actor holds one of: General Director.<br>3. The target record exists and its identifier is known to the screen.<br>4. The record is not closed by an add/edit lock (IsLockedOut). |
| Main flow | 1. The actor reaches the function from the screen of this module that hosts it.<br>2. The actor selects the record to open.<br>3. The SPA issues `GET /api/Charities/{id}` carrying charityName.<br>4. `CharitiesController` binds the typed request DTO and delegates to the application service.<br>5. `ICharityService` validates the payload with its FluentValidation validator, applies the business rules and the charity scope, and persists through `IUnitOfWork`.<br>6. The system returns the record with its child collections and the SPA binds them to the form fields. |
| Alternate flows | None recorded. |
| Exception flows | • The session has expired or the role is not permitted — the request is rejected and the SPA routes back to the login state.<br>• The charity is closed by a lock (IsLockedOut) — the write is refused. |
| Post-conditions | • No stored data is changed — the operation is a read. |
| Realisation | Route `#/charities/:id` → `CharityDetailComponent`<br>`GET /api/Charities/{id}` → `CharitiesController` → `ICharityService` |

#### 8.U.5  UC-CHR-05 — Update a charity profile تعديل بيانات الجمعية


| Item | Specification |
| --- | --- |
| Use case ID | UC-CHR-05 |
| Name | Update a charity profile تعديل بيانات الجمعية |
| Type | Update a record |
| Primary actor | General Director |
| Secondary actors | The system (Web API + business layer); the database. |
| Summary | Applies changes to the charity's descriptive and contact data. The call is authorised against the acting user id. |
| Trigger | The actor selects the function from the module menu or the screen that hosts it. |
| Pre-conditions | 1. The actor is authenticated; the JWT access token is valid and the client holds the role, charity and country claims it carries.<br>2. The actor holds one of: General Director.<br>3. Caller identity comes from the JWT, never from the request; the application service scopes the query to the charity (and country) that owns the user, unless the role is an HQ role permitted to pass an explicit `charityId` filter.<br>4. The target record exists and its identifier is known to the screen. |
| Main flow | 1. The actor reaches the function from the screen of this module that hosts it. The screen opens in edit mode on the selected record.<br>2. The system loads the current values into the form.<br>3. The actor changes the fields to be corrected.<br>4. The actor presses the command button of the screen.<br>5. The SPA issues `PUT /api/Charities/{id}` carrying userId.<br>6. `CharitiesController` binds the typed request DTO and delegates to the application service.<br>7. `ICharityService` validates the payload with its FluentValidation validator, applies the business rules and the charity scope, and persists through `IUnitOfWork`.<br>8. The business layer re-validates the payload, applies the changes and persists them.<br>9. The system returns the outcome and the SPA refreshes the screen. |
| Alternate flows | • An HQ role (General Director, Financial Director, Staff) may pass an explicit charity id and so read across the charity boundary; a charity user may not and always sees its own data.<br>• The actor abandons the form before saving — nothing is written and the record keeps its previous state. |
| Exception flows | • The session has expired or the role is not permitted — the request is rejected and the SPA routes back to the login state.<br>• A mandatory field is empty or fails its format check — the save is refused and the field is flagged on the form. |
| Post-conditions | • The stored record carries the new values; no other record is affected. |
| Realisation | Route `#/charities/:id/edit` → `CharityFormComponent`<br>`PUT /api/Charities/{id}` → `CharitiesController` → `ICharityService` |

#### 8.U.6  UC-CHR-06 — Open the permissions screen الصلاحيات


| Item | Specification |
| --- | --- |
| Use case ID | UC-CHR-06 |
| Name | Open the permissions screen الصلاحيات |
| Type | Read a record |
| Primary actor | General Director |
| Secondary actors | The system (Web API + business layer); the database. |
| Summary | Lists charities together with their three independent control flags — account locked, data entry enabled, data editing enabled — so that they can be toggled. |
| Trigger | The actor selects the function from the module menu or the screen that hosts it. |
| Pre-conditions | 1. The actor is authenticated; the JWT access token is valid and the client holds the role, charity and country claims it carries.<br>2. The actor holds one of: General Director.<br>3. Caller identity comes from the JWT, never from the request; the application service scopes the query to the charity (and country) that owns the user, unless the role is an HQ role permitted to pass an explicit `charityId` filter.<br>4. The target record exists and its identifier is known to the screen.<br>5. The record is not closed by an add/edit lock (IsLocked, IsLockedOut). |
| Main flow | 1. The actor reaches the function from the screen of this module that hosts it.<br>2. The actor selects the record to open.<br>3. The SPA issues `GET /api/Charities` carrying userId.<br>4. `CharitiesController` binds the typed request DTO and delegates to the application service.<br>5. `ICharityService` validates the payload with its FluentValidation validator, applies the business rules and the charity scope, and persists through `IUnitOfWork`.<br>6. The system returns the record with its child collections and the SPA binds them to the form fields. |
| Alternate flows | • An HQ role (General Director, Financial Director, Staff) may pass an explicit charity id and so read across the charity boundary; a charity user may not and always sees its own data. |
| Exception flows | • The session has expired or the role is not permitted — the request is rejected and the SPA routes back to the login state.<br>• The charity is closed by a lock (IsLocked, IsLockedOut) — the write is refused. |
| Post-conditions | • No stored data is changed — the operation is a read. |
| Realisation | `GET /api/Charities` → `CharitiesController` → `ICharityService` |

#### 8.U.7  UC-CHR-07 — Lock or unlock a charity account إيقاف / تفعيل حساب الجمعية


| Item | Specification |
| --- | --- |
| Use case ID | UC-CHR-07 |
| Name | Lock or unlock a charity account إيقاف / تفعيل حساب الجمعية |
| Type | Lock control |
| Primary actor | General Director |
| Secondary actors | The system (Web API + business layer); the database. |
| Summary | Sets the membership lock-out flag for the charity's login. Unlocking additionally resets the failed-attempt counter and the lock-out timestamps so the account can sign in immediately. Alternate: a non-administrator caller is rejected. |
| Trigger | The actor selects the function from the module menu or the screen that hosts it. |
| Pre-conditions | 1. The actor is authenticated; the JWT access token is valid and the client holds the role, charity and country claims it carries.<br>2. The actor holds one of: General Director.<br>3. Caller identity comes from the JWT, never from the request; the application service scopes the query to the charity (and country) that owns the user, unless the role is an HQ role permitted to pass an explicit `charityId` filter.<br>4. The record is not closed by an add/edit lock (LockType.Charity, IsLockedOut, LockType.CharityEdit, LockType.CharityAdd). |
| Main flow | 1. The actor reaches the function from the screen of this module that hosts it.<br>2. The actor selects the charity (or the record) whose lock is to change.<br>3. The actor sets the lock type and its value.<br>4. The SPA issues `POST /api/Charities/{id}/lock · POST /api/Charities/{id}/unlock` carrying the lock type and its value in the request body.<br>5. `CharitiesController` binds the typed request DTO and delegates to the application service.<br>6. `ICharityService` validates the payload with its FluentValidation validator, applies the business rules and the charity scope, and persists through `IUnitOfWork`.<br>7. The business layer stores the lock; subsequent add/edit attempts by the charity are refused while it is set. |
| Alternate flows | • An HQ role (General Director, Financial Director, Staff) may pass an explicit charity id and so read across the charity boundary; a charity user may not and always sees its own data. |
| Exception flows | • The session has expired or the role is not permitted — the request is rejected and the SPA routes back to the login state.<br>• The charity is closed by a lock (LockType.Charity, IsLockedOut, LockType.CharityEdit, LockType.CharityAdd) — the write is refused. |
| Post-conditions | • The lock value is stored against the charity and is enforced on every later add/edit attempt. |
| Realisation | `POST /api/Charities/{id}/lock` → `CharitiesController` → `ICharityService` |

#### 8.U.8  UC-CHR-08 — Enable or disable data entry for a charity السماح بالإضافة


| Item | Specification |
| --- | --- |
| Use case ID | UC-CHR-08 |
| Name | Enable or disable data entry for a charity السماح بالإضافة |
| Type | Create a record |
| Primary actor | General Director |
| Secondary actors | The system (Web API + business layer); the database. |
| Summary | Switches the charity's IsAddEnabled flag. When disabled the charity may still sign in and view its data but cannot create new families, orphans or reports — used to freeze registration between campaigns. |
| Trigger | The actor selects the function from the module menu or the screen that hosts it. |
| Pre-conditions | 1. The actor is authenticated; the JWT access token is valid and the client holds the role, charity and country claims it carries.<br>2. The actor holds one of: General Director.<br>3. The record is not closed by an add/edit lock (LockType.Charity, IsLockedOut, LockType.CharityEdit, LockType.CharityAdd). |
| Main flow | 1. The actor reaches the function from the screen of this module that hosts it. The screen opens in add mode.<br>2. The system loads the reference data the form needs (regions, centres, banks, types and the other lookup lists bound to the drop-downs).<br>3. The actor completes the input fields of the form.<br>4. The actor presses the command button of the screen.<br>5. The SPA issues `POST /api/Charities/{id}/lock · POST /api/Charities/{id}/unlock` carrying lockType=CharityAdd.<br>6. `CharitiesController` binds the typed request DTO and delegates to the application service.<br>7. `ICharityService` validates the payload with its FluentValidation validator, applies the business rules and the charity scope, and persists through `IUnitOfWork`.<br>8. The business layer validates the payload, stamps the owning charity and the creating user, and persists the new record.<br>9. The system returns the outcome and the SPA confirms the save and returns to the list screen. |
| Alternate flows | • The actor abandons the form before saving — nothing is written and the record keeps its previous state. |
| Exception flows | • The session has expired or the role is not permitted — the request is rejected and the SPA routes back to the login state.<br>• A mandatory field is empty or fails its format check — the save is refused and the field is flagged on the form.<br>• The charity is closed by a lock (LockType.Charity, IsLockedOut, LockType.CharityEdit, LockType.CharityAdd) — the write is refused. |
| Post-conditions | • A new record exists, owned by the charity of the creating user, and appears in the list screen of the module. |
| Realisation | `PUT /api/Charities/{id}/update-rights` → `CharitiesController` → `ICharityService` |

#### 8.U.9  UC-CHR-09 — Enable or disable data editing for a charity السماح بالتعديل


| Item | Specification |
| --- | --- |
| Use case ID | UC-CHR-09 |
| Name | Enable or disable data editing for a charity السماح بالتعديل |
| Type | Update a record |
| Primary actor | General Director |
| Secondary actors | The system (Web API + business layer); the database. |
| Summary | Switches the charity's IsUpdateEnabled flag, freezing existing records against modification — typically applied once a payment batch has been built from the data. |
| Trigger | The actor selects the function from the module menu or the screen that hosts it. |
| Pre-conditions | 1. The actor is authenticated; the JWT access token is valid and the client holds the role, charity and country claims it carries.<br>2. The actor holds one of: General Director.<br>3. The target record exists and its identifier is known to the screen.<br>4. The record is not closed by an add/edit lock (LockType.Charity, IsLockedOut, LockType.CharityEdit, LockType.CharityAdd). |
| Main flow | 1. The actor reaches the function from the screen of this module that hosts it. The screen opens in edit mode on the selected record.<br>2. The system loads the current values into the form.<br>3. The actor changes the fields to be corrected.<br>4. The actor presses the command button of the screen.<br>5. The SPA issues `POST /api/Charities/{id}/lock · POST /api/Charities/{id}/unlock` carrying lockType=CharityEdit.<br>6. `CharitiesController` binds the typed request DTO and delegates to the application service.<br>7. `ICharityService` validates the payload with its FluentValidation validator, applies the business rules and the charity scope, and persists through `IUnitOfWork`.<br>8. The business layer re-validates the payload, applies the changes and persists them.<br>9. The system returns the outcome and the SPA refreshes the screen. |
| Alternate flows | • The actor abandons the form before saving — nothing is written and the record keeps its previous state. |
| Exception flows | • The session has expired or the role is not permitted — the request is rejected and the SPA routes back to the login state.<br>• A mandatory field is empty or fails its format check — the save is refused and the field is flagged on the form.<br>• The charity is closed by a lock (LockType.Charity, IsLockedOut, LockType.CharityEdit, LockType.CharityAdd) — the write is refused. |
| Post-conditions | • The stored record carries the new values; no other record is affected. |
| Realisation | `PUT /api/Charities/{id}/update-rights` → `CharitiesController` → `ICharityService` |

### 8.A  Annex - Angular routes of this module

Routes are hash-based (`useHash: true`), rendered inside `MainLayoutComponent` behind
`AuthGuard` and `PermissionGuard`. *implemented* means the route exists in
`Frontend/src/app`; *planned* means it is specified here and not yet built.

| Angular route | Feature module | Component | Status |
| --- | --- | --- | --- |
| `#/charities` | `charities` | `CharityListComponent` | implemented |
| `#/charities/create` | `charities` | `CharityFormComponent` | implemented |
| `#/charities/:id` | `charities` | `CharityDetailComponent` | implemented |
| `#/charities/:id/edit` | `charities` | `CharityFormComponent` | implemented |
| `#/charities/profile` | `charities` | `CharityDetailComponent` | implemented |
| `#/charities/my-profile` | `charities` | `CharityDetailComponent` | implemented |

### 8.B  Annex - API controllers of this module

Every controller inherits `ApiController` (`[Authorize]`, route `api/[controller]`) and
returns `Framework.Core.ApiResponse<T>`. Business rules live in the application service
behind each controller, never in the controller itself.

| Controller | Base route | Responsibility |
| --- | --- | --- |
| `CharitiesController` | `api/Charities` | Charity creation and listing; family listing and search per charity. Charity profile read/update. Account lock, add-lock, edit-lock. |

---

Shared context - system architecture, actors and roles, the sponsorship lifecycle, the full module map and all appendices - is in the companion document [00-Overview-and-Common-Context](00-Overview-and-Common-Context.md). The delivery backlog for this module is epic EP-03 in [00-EPICS-and-User-Stories](00-EPICS-and-User-Stories.md).

