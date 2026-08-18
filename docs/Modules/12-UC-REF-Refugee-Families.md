# WAR.IIROSA - Refugee Families

الاسر اللاجئة | use case prefix `UC-REF` | chapter 12 of the master document


| Field | Value |
| --- | --- |
| Document title | WAR.IIROSA - Module Specification: Refugee Families |
| Module | Refugee Families - الاسر اللاجئة |
| Use case prefix | UC-REF |
| Chapter in master document | Chapter 12 |
| Documented use cases | 4 |
| Principal routes | `#/families/refugees`, `#/families/refugees/:id/edit` |
| Version | 1.1 |
| Status | Chapter content extracted verbatim; screen fields and scenarios derived from the source code |
| Date | 18 August 2026 |
| Parent document | WAR.IIROSA-Project-Documentation.docx v1.0 (repository WAR.IIROSA, commit 4beb736) |

## Contents of this document

1. Chapter 12 — module purpose and use-case catalogue (verbatim from the master document)
2. §12.S — screen field specifications (every field of every screen, derived from the AngularJS views)
3. §12.U — one expanded scenario per use case (trigger, pre-conditions, main flow, alternates, exceptions, post-conditions)
4. §12.A / §12.B — annexes: screens and Web API controllers of this module

## 12. Refugee Families

الأسر اللاجئة — a distinct register for displaced families, using its own contract and search path. The menu entries for this module are currently commented out in the shell, so it is reachable by direct route only.


| ID | Use case | Primary actor | Description & main flow | Realisation |
| --- | --- | --- | --- | --- |
| UC-REF-01 | List refugee families قائمة الأسر اللاجئة | Charity, HQ roles | Paged list of the refugee families registered by the charity. | Route `#/families/refugees` |
| UC-REF-02 | Search refugee families البحث في الأسر اللاجئة | Charity, HQ roles | Applies the standard search criteria to the refugee population. | GET /api/Families?familyType=Refugee&search=?…&type |
| UC-REF-03 | Register a refugee family اضافة أسرة لاجئة | Charity | Creates the refugee family file using the dedicated refugee contract, which captures displacement-specific data in addition to the standard household details. | Route `#/families/refugees/:id/edit` → POST /api/Families |
| UC-REF-04 | View / update a refugee family بيانات الأسرة اللاجئة | Charity, HQ roles | Loads a refugee family file for review or amendment. | GET /api/Families/{id} |

### 12.S  Screen field specifications

Derived from the AngularJS views of this module. For every screen the table lists each data-entry field in the order it appears, the scope property it binds to, its control type, whether the form marks it mandatory, and the lookup or rule that governs it. Labels are reproduced as they appear on screen (Arabic, right-to-left).

#### 12.S.1  Screen `#/families/refugees`


| Property | Value |
| --- | --- |
| Angular route | `#/families/refugees` |
| Feature module | `families` (lazy-loaded) |
| Component | `RefugeeFamilyListComponent` |
| Route status | planned |
| Data-entry fields | 3 |
| Grids on the screen | 1 |
| Commands | 5 |


| Section | Field (as labelled) | Bound to | Control | Mandatory · source · rules |
| --- | --- | --- | --- | --- |
| — | الجمعية | CharityId | Drop-down list | Optional · options: lookup: Charities (+ كافة الجهات) · on change: getCharityData() |
| قائمة الاسر | (unlabelled) | searchValue | Text box | Optional |
| قائمة الاسر | البحث عن طريق | searchType | Drop-down list | Optional · options: إسم الاب / إسم الام / إسم اليتيم / إسم المعيل / الرقم القومي / كود اليتيم / الهاتف |

Grids on this screen:

| Grid (row source) | Columns |
| --- | --- |
| family in Families track by $index | الرقم · إسم الأب · إسم الأم · الأبناء · الهواتف · كود العائله · الجمعية |

Commands on this screen:

| Command | Handler | Shown when |
| --- | --- | --- |
| بحث | Search(1) | always |
| (icon only) | AddFamily() | always |
| (icon only) | ViewOne(family.Id) | always |
| (icon only) | GetNext() | always |
| (icon only) | GetPrev() | always |

#### 12.S.2  Screen `#/families/refugees/:id/edit`


| Property | Value |
| --- | --- |
| Angular route | `#/families/refugees/:id/edit` |
| Feature module | `families` (lazy-loaded) |
| Component | `RefugeeFamilyFormComponent` |
| Route status | planned |
| Data-entry fields | 48 |
| Grids on the screen | 1 |
| Commands | 31 |


| Section | Field (as labelled) | Bound to | Control | Mandatory · source · rules |
| --- | --- | --- | --- | --- |
| — | الجمعية | CharityId | Drop-down list | Optional · options: lookup: Charities (+ كافة الجهات) · on change: getCharityData() |
| بيانات الأسرة | إسم الأسرة | FamilyName | Text box | Optional · read-only |
| بيانات الأسرة | القرية / الحي | FamilyVillage | Text box | Mandatory |
| بيانات الأسرة | المركز/ المدينة | FamilyCenters | Drop-down list | Mandatory · options: lookup: Centers |
| بيانات الأسرة | المنطقة /المحافظة | FamilyRegions | Drop-down list | Mandatory · options: lookup: Regions · on change: Getcenters() |
| بيانات الأسرة | بجوار | FamilyNearBy | Text box | Optional |
| بيانات الأسرة | الشارع | FamilyStreet | Text box | Optional |
| بيانات الأسرة | العنوان التفصيلى | FamilyAddress | Text box | Mandatory |
| بيانات الأسرة | قيمة الإيجار | FamilyRentNumber | Numeric box | Mandatory |
| بيانات الأسرة | ملكية السكن | FamilyHouseOwnerhip | Drop-down list | Mandatory · options: lookup: HouseOwnerShips · on change: DisplayRentValue() |
| بيانات الأسرة | حالة محتويات السكن | FamilyHouseStatus | Drop-down list | Mandatory · options: lookup: HouseStatus |
| بيانات الأسرة | نوع السكن | FamilyHouseType | Drop-down list | Mandatory · options: lookup: HouseTypes |
| بيانات الأسرة | نوع الدخل | IncomeTypesDdl | Drop-down list | Mandatory · options: lookup: IncomeTypes |
| بيانات الأسرة | الدخل الكلى | FamilyIncomeNumber | Numeric box | Optional · read-only |
| بيانات الأسرة | نصيب الفرد | FamilyInMem | Numeric box | Optional · read-only |
| بيانات الأسرة | عدد الأبناء | FamilyChildrenNumber | Numeric box | Optional · read-only |
| بيانات الأسرة | ملاحظات الباحث | FamilyNote | Text box | Optional |
| اضافة معيل | السبب | OtherParentRelationReasonID | Drop-down list | Mandatory · options: lookup: ReasonsOFRels |
| اضافة معيل | نوعها | OtherParentRelationModel | Drop-down list | Mandatory · options: lookup: Relations · on change: OnChangeRelation() |
| اضافة معيل | العلاقة | ParentRelation | Drop-down list | Mandatory · options: الاب / الام / علاقة أخرى · on change: OtherRelations() |
| اضافة معيل | الاسم الثالث | ParentThirdname | Text box | Mandatory |
| اضافة معيل | الاسم الثانى | Parentsecondname | Text box | Mandatory |
| اضافة معيل | الاسم الاول | ParentFirname | Text box | Mandatory |
| اضافة معيل | الاسم الرباعي | ParentFourthname | Text box | Mandatory |
| اضافة معيل | الجنسية | ParentCountries | Drop-down list | Mandatory · options: lookup: Countries · on change: GetBanks(this) |
| اضافة معيل | إسم الأسرة | ParentFamilyname | Text box | Optional |
| اضافة معيل | تاريخ الوفاة | ParentDeathDate | Date picker | Mandatory · on change: ParentDeathDate.date=dt.toISOString() |
| اضافة معيل | متوفى | ParentIsDeath | Check box | Optional · on click: WriteDeathDate() |
| اضافة معيل | سبب الوفاة | ParentReasonDeath | Drop-down list | Mandatory · options: طبيعية / مرض / حادث |
| اضافة معيل | تاريخ الميلاد | ParentBDate | Date picker | Mandatory · on change: ParentBDate.date=dt.toISOString() |
| اضافة معيل | رقم جواز السفر | ParentNId | Text box | Mandatory |
| اضافة ابن | سبب الاستبعاد * | childExcludeReson | Drop-down list | Optional · options: lookup: childExcludeResons · read-only |
| اضافة ابن | تاريخ الميلاد | ChildBDate | Date picker | Mandatory |
| اضافة ابن | رقم جواز السفر | ChildNId | Text box | Mandatory · max length 14 |
| اضافة ابن | الاسم الاول | ChildFName | Text box | Mandatory · read-only when CanEditInChildName==false |
| اضافة ابن | الصورة | ChildPhoto | File upload | Optional · accepts image/* |
| اضافة ابن | الحالة الصحية | ChildHealthStatus | Drop-down list | Mandatory · options: lookup: HealthStatuss |
| اضافة ابن | الحالة الاجتماعية | ChildsocialStatus | Drop-down list | Mandatory · options: lookup: socialStatuss |
| اضافة ابن | النوع | childGender | Drop-down list | Mandatory · options: ذكر / انثى |
| اضافة مرافق | تاريخ الميلاد | AccompanyBDate | Date picker | Mandatory |
| اضافة مرافق | رقم جواز السفر | AccompanyNId | Text box | Mandatory · max length 14 |
| اضافة مرافق | الاسم الاول | AccompanyFName | Text box | Mandatory · read-only when CanEditInChildName==false |
| اضافة مرافق | الحالة الصحية | AccompanyHealthStatus | Drop-down list | Mandatory · options: lookup: HealthStatuss |
| اضافة مرافق | الحالة الاجتماعية | AccompanysocialStatus | Drop-down list | Mandatory · options: lookup: socialStatuss |
| اضافة مرافق | النوع | AccompanyGender | Drop-down list | Mandatory · options: ذكر / انثى |
| نقل الاسرة | اختر الجمعية | CharityId | Drop-down list | Optional · options: lookup: Charities · on change: getCharityData() |
| تعديل العلاقة | نوعها | parent.RelationId | Drop-down list | Mandatory · options: lookup: Relations |
| تعديل العلاقة | العلاقة | MainRelationInPOPUP | Drop-down list | Mandatory · options: الاب / الام / علاقة أخرى · on change: OtherRelationsOfPOPUP() |

Grids on this screen:

| Grid (row source) | Columns |
| --- | --- |
| childOrChec in childOrChecksList track by $index | إسم الدفعة · رقم الشيك /الحوالة · تاريخ الشيك/ الحوالة · المبلغ بالجنية · تم التسليم · إسم المعيل المستلم |

Commands on this screen:

| Command | Handler | Shown when |
| --- | --- | --- |
| تعديل اعضاء الاسرة | EditingInFamilyMembers() | always |
| حفظ | AddFamily() | always |
| اضافة معيل | AddNewParent() | always |
| اضافة ابن | AddNewchild() | CanAddChildren |
| اضافة مرافق | AddNewAccompany() | CanAddChildren |
| (icon only) | AddphoneNumber() | always |
| (icon only) | AddIncomeType() | always |
| غلق | EmptyParentModel() | always |
| تم | AddParent() | always |
| حفظ كمعيل وكيتيم | AddParentAndChild() | ParentIsAChildFlag |
| غلق | EmptyChildModel() | always |
| تم | AddChild() | always |
| تم | AddAccompany() | always |
| غلق | EmptyCheckOrModel() | always |
| غلق | CloaseMoveFamilyModel() | always |
| نقل | MoveFamily() | always |
| غلق | CloaseDeleteSposnorModel() | always |
| حذف | DeleteSposnor() | always |
| غلق | CloseEditRelPOPUP() | always |
| حفظ | EditRelationOfSponserInPOPUP() | always |
| (icon only) | ChangeCurrentSponsor(this) | parent.CurrentSponser && parent.IsDead != true |
| (icon only) | EditParent($index) | always |
| (icon only) | ViewEditRelationPOPUPFun($index) | always |
| (icon only) | EditChild($index) | always |
| (icon only) | ShowChecksOfChild(child.Id) | child.Code != null |
| (icon only) | EditAccompany($index) | always |
| (icon only) | ShowChecksOfChild(acc.Id) | acc.Code != null |
| (icon only) | RemovePhone($index) | always |
| (icon only) | RemoveIncome($index,type.Number) | always |
| (icon only) | WriteDeathDate() | always |
| (icon only) | RemoveImage() | ChildPhoto!=null |

### 12.U  Use case scenarios

One expanded scenario for every use case of this module. Pre-conditions, flows and post-conditions are derived from the screen that hosts the function, the Web API action that serves it and the business-layer method that executes it; quoted messages are the literal strings returned by the business layer.

#### 12.U.1  UC-REF-01 — List refugee families قائمة الأسر اللاجئة


| Item | Specification |
| --- | --- |
| Use case ID | UC-REF-01 |
| Name | List refugee families قائمة الأسر اللاجئة |
| Type | Browse a list |
| Primary actor | Charity, HQ roles |
| Secondary actors | The system (Web API + business layer); the database. |
| Summary | Paged list of the refugee families registered by the charity. |
| Trigger | The actor opens the screen at `#/families/refugees` from the module menu. |
| Pre-conditions | 1. The actor is authenticated; the JWT access token is valid and the client holds the role, charity and country claims it carries.<br>2. The actor holds one of: charity, HQ roles.<br>3. The SPA route `#/families/refugees` has loaded and its reference-data lookups have been populated. |
| Main flow | 1. The actor navigates to the screen at `#/families/refugees`.<br>2. The system loads the filter lookups the screen offers and, for HQ roles, the charity selector.<br>3. The SPA calls the service endpoint that backs the function.<br>4. The Web API controller receives the request and delegates to the business layer.<br>5. The business layer executes the rules and the data access.<br>6. The result set is scoped to the caller’s charity and country, ordered and paged.<br>7. The SPA renders the page with the columns listed in the screen specification and the paging control. |
| Alternate flows | • No row matches the criteria — the grid renders empty and the paging control reports zero pages. |
| Exception flows | • The session has expired or the role is not permitted — the request is rejected and the SPA routes back to the login state. |
| Post-conditions | • No stored data is changed — the operation is a read. |
| Realisation | Route `#/families/refugees` → `RefugeeFamilyListComponent` |

#### 12.U.2  UC-REF-02 — Search refugee families البحث في الأسر اللاجئة


| Item | Specification |
| --- | --- |
| Use case ID | UC-REF-02 |
| Name | Search refugee families البحث في الأسر اللاجئة |
| Type | Search / filter |
| Primary actor | Charity, HQ roles |
| Secondary actors | The system (Web API + business layer); the database. |
| Summary | Applies the standard search criteria to the refugee population. |
| Trigger | The actor selects the function from the module menu or the screen that hosts it. |
| Pre-conditions | 1. The actor is authenticated; the JWT access token is valid and the client holds the role, charity and country claims it carries.<br>2. The actor holds one of: charity, HQ roles. |
| Main flow | 1. The actor reaches the function from the screen of this module that hosts it.<br>2. The actor enters the search criteria in the filter fields.<br>3. The actor presses the command button of the screen.<br>4. The SPA issues `GET /api/Families?familyType=Refugee&search=` carrying …, type.<br>5. `CharitiesController` binds the typed request DTO and delegates to the application service.<br>6. `IFamilyService` validates the payload with its FluentValidation validator, applies the business rules and the charity scope, and persists through `IUnitOfWork`.<br>7. The matching rows are returned, scoped to the caller’s charity, and rendered in the result grid. |
| Alternate flows | • No row matches the criteria — the grid renders empty and the paging control reports zero pages. |
| Exception flows | • The session has expired or the role is not permitted — the request is rejected and the SPA routes back to the login state. |
| Post-conditions | • No stored data is changed — the operation is a read. |
| Realisation | `GET /api/Families?familyType=Refugee&search=?…&type` → `FamiliesController` → `IFamilyService` |

#### 12.U.3  UC-REF-03 — Register a refugee family اضافة أسرة لاجئة


| Item | Specification |
| --- | --- |
| Use case ID | UC-REF-03 |
| Name | Register a refugee family اضافة أسرة لاجئة |
| Type | Create a record |
| Primary actor | Charity |
| Secondary actors | The system (Web API + business layer); the database. |
| Summary | Creates the refugee family file using the dedicated refugee contract, which captures displacement-specific data in addition to the standard household details. |
| Trigger | The actor presses «حفظ» on the screen AddFamilyRefugees. |
| Pre-conditions | 1. The actor is authenticated; the JWT access token is valid and the client holds the role, charity and country claims it carries.<br>2. The actor holds one of: charity.<br>3. The SPA route `#/families/refugees/:id/edit` has loaded and its reference-data lookups have been populated. |
| Main flow | 1. The actor navigates to the screen at `#/families/refugees/:id/edit`. The screen opens in add mode.<br>2. The system loads the reference data the form needs (regions, centres, banks, types and the other lookup lists bound to the drop-downs).<br>3. The actor completes the input fields. Mandatory fields: القرية / الحي، المركز/ المدينة، المنطقة /المحافظة، العنوان التفصيلى، قيمة الإيجار، ملكية السكن، حالة محتويات السكن، نوع السكن، نوع الدخل، السبب، نوعها، العلاقة، الاسم الثالث، الاسم الثانى ….<br>4. The actor presses «حفظ» (AddFamily()).<br>5. The SPA issues `POST /api/Families` carrying [FromBody] RefugeesFamilyContract family.<br>6. `FamiliesController` binds the typed request DTO and delegates to the application service.<br>7. `IFamilyService` validates the payload with its FluentValidation validator, applies the business rules and the charity scope, and persists through `IUnitOfWork`.<br>8. The business layer validates the payload, stamps the owning charity and the creating user, and persists the new record.<br>9. The system returns the outcome and the SPA confirms the save and returns to the list screen. |
| Alternate flows | • The actor abandons the form before saving — nothing is written and the record keeps its previous state. |
| Exception flows | • The session has expired or the role is not permitted — the request is rejected and the SPA routes back to the login state.<br>• The business layer returns «أحد المعيلين مكرر من قبل أكثر من مرة» and the operation is not applied.<br>• A mandatory field is empty or fails its format check — the save is refused and the field is flagged on the form. |
| Post-conditions | • A new record exists, owned by the charity of the creating user, and appears in the list screen of the module. |
| Realisation | Route `#/families/refugees/:id/edit` → `RefugeeFamilyFormComponent`<br>`POST /api/Families` → `FamiliesController` → `IFamilyService` |

#### 12.U.4  UC-REF-04 — View / update a refugee family بيانات الأسرة اللاجئة


| Item | Specification |
| --- | --- |
| Use case ID | UC-REF-04 |
| Name | View / update a refugee family بيانات الأسرة اللاجئة |
| Type | Update a record |
| Primary actor | Charity, HQ roles |
| Secondary actors | The system (Web API + business layer); the database. |
| Summary | Loads a refugee family file for review or amendment. |
| Trigger | The actor selects the function from the module menu or the screen that hosts it. |
| Pre-conditions | 1. The actor is authenticated; the JWT access token is valid and the client holds the role, charity and country claims it carries.<br>2. The actor holds one of: charity, HQ roles.<br>3. Caller identity comes from the JWT, never from the request; the application service scopes the query to the charity (and country) that owns the user, unless the role is an HQ role permitted to pass an explicit `charityId` filter.<br>4. The target record exists and its identifier is known to the screen. |
| Main flow | 1. The actor reaches the function from the screen of this module that hosts it. The screen opens in edit mode on the selected record.<br>2. The system loads the current values into the form.<br>3. The actor changes the fields to be corrected.<br>4. The actor presses the command button of the screen.<br>5. The SPA issues `GET /api/Families/{id}` carrying userId, id.<br>6. `FamiliesController` binds the typed request DTO and delegates to the application service.<br>7. `IFamilyService` validates the payload with its FluentValidation validator, applies the business rules and the charity scope, and persists through `IUnitOfWork`.<br>8. The business layer re-validates the payload, applies the changes and persists them.<br>9. The system returns the outcome and the SPA refreshes the screen. |
| Alternate flows | • An HQ role (General Director, Financial Director, Staff) may pass an explicit charity id and so read across the charity boundary; a charity user may not and always sees its own data.<br>• The actor abandons the form before saving — nothing is written and the record keeps its previous state. |
| Exception flows | • The session has expired or the role is not permitted — the request is rejected and the SPA routes back to the login state.<br>• A mandatory field is empty or fails its format check — the save is refused and the field is flagged on the form. |
| Post-conditions | • The stored record carries the new values; no other record is affected. |
| Realisation | `GET /api/Families/{id}` → `FamiliesController` → `IFamilyService` |

### 12.A  Annex - Angular routes of this module

Routes are hash-based (`useHash: true`), rendered inside `MainLayoutComponent` behind
`AuthGuard` and `PermissionGuard`. *implemented* means the route exists in
`Frontend/src/app`; *planned* means it is specified here and not yet built.

| Angular route | Feature module | Component | Status |
| --- | --- | --- | --- |
| `#/families/refugees` | `families` | `RefugeeFamilyListComponent` | planned |
| `#/families/refugees/:id/edit` | `families` | `RefugeeFamilyFormComponent` | planned |

### 12.B  Annex - API controllers of this module

Every controller inherits `ApiController` (`[Authorize]`, route `api/[controller]`) and
returns `Framework.Core.ApiResponse<T>`. Business rules live in the application service
behind each controller, never in the controller itself.

| Controller | Base route | Responsibility |
| --- | --- | --- |
| `FamiliesController` | `api/Families` | Family / housing / refugee creation, update, transfer, follow-up, guardian-change requests, report group counts. Family read by type; project receipt flag. |

---

Shared context - system architecture, actors and roles, the sponsorship lifecycle, the full module map and all appendices - is in the companion document [00-Overview-and-Common-Context](00-Overview-and-Common-Context.md). The delivery backlog for this module is epic EP-07 in [00-EPICS-and-User-Stories](00-EPICS-and-User-Stories.md).

