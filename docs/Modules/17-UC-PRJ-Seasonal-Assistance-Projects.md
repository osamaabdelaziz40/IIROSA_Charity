# WAR.IIROSA - Seasonal Assistance Projects

المساعدات الموسمية ومشاريع الاسر | use case prefix `UC-PRJ` | chapter 17 of the master document


| Field | Value |
| --- | --- |
| Document title | WAR.IIROSA - Module Specification: Seasonal Assistance Projects |
| Module | Seasonal Assistance Projects - المساعدات الموسمية ومشاريع الاسر |
| Use case prefix | UC-PRJ |
| Chapter in master document | Chapter 17 |
| Documented use cases | 13 |
| Principal routes | `#/seasonal-aid`, `#/seasonal-aid/create`, `#/seasonal-aid/:id`, `#/seasonal-aid/:id/edit`, `#/seasonal-aid/:id/beneficiaries`, `#/seasonal-aid/:id/distribution`, `#/seasonal-aid/:id/eligible-families` *(planned)*, `#/seasonal-aid/:id/report` *(planned)* |
| Version | 1.1 |
| Status | Chapter content extracted verbatim; screen fields and scenarios derived from the source code |
| Date | 18 August 2026 |
| Parent document | WAR.IIROSA-Project-Documentation.docx v1.0 (repository WAR.IIROSA, commit 4beb736) |

## Contents of this document

1. Chapter 17 — module purpose and use-case catalogue (verbatim from the master document)
2. §17.D — detailed specifications carried over from chapter 25
3. §17.S — screen field specifications (every field of every screen, derived from the AngularJS views)
4. §17.U — one expanded scenario per use case (trigger, pre-conditions, main flow, alternates, exceptions, post-conditions)
5. §17.A / §17.B — annexes: screens and Web API controllers of this module

## 17. Seasonal Assistance Projects

المساعدات الموسمية ومشاريع الأسر — one-off or seasonal aid campaigns (Ramadan baskets, Eid clothing, winter aid, school bags). HQ defines the project and its budget per charity; the charity selects which of its families benefit; distribution is then confirmed family by family.


| ID | Use case | Primary actor | Description & main flow | Realisation |
| --- | --- | --- | --- | --- |
| UC-PRJ-01 | List projects المشاريع | Gen. Director, Staff | Paged list of assistance projects for a charity or across all charities, with dates, budget and status. | Route `#/seasonal-aid` → GET /api/SeasonalAid/campaigns |
| UC-PRJ-02 | Create a project إضافة مشروع | Gen. Director | Defines the campaign — name, type, period, per-family amount or in-kind item, target charities and quota — making it available for family selection. | Route `#/seasonal-aid/create` → POST /api/SeasonalAid/campaigns |
| UC-PRJ-03 | View a project عرض المشروع | Gen. Director, Staff, charity | Loads one project's definition and its current registration figures. | GET /api/SeasonalAid/campaigns |
| UC-PRJ-04 | Update a project تعديل المشروع | Gen. Director | Amends project parameters and quotas while the campaign is open. | PUT /api/SeasonalAid/campaigns |
| UC-PRJ-05 | Delete a project حذف المشروع | Gen. Director | Removes a project created in error together with its family registrations. | DELETE /api/SeasonalAid/campaigns |
| UC-PRJ-06 | Open the family-selection screen مشاريع الأسر | Charity | Lists the projects open to the charity and, for the chosen project, the charity's families with their current registration state, ready to be selected. | Route `#/seasonal-aid/:id/beneficiaries` → GET /api/SeasonalAid/campaigns/{campaignId}/beneficiaries GET /api/SeasonalAid/campaigns/{campaignId}/beneficiaries |
| UC-PRJ-07 | Register families for a project اختيار الأسر للمشروع | Charity | Selects or de-selects families as beneficiaries of the project, subject to the quota defined by HQ; the selections are saved as project-family registrations. | PUT /api/SeasonalAid/campaigns/{campaignId}/beneficiaries |
| UC-PRJ-08 | Confirm a family received the assistance تأكيد استلام الأسرة | Charity | Marks the individual project-family registration as delivered, closing the distribution record for that household. | PUT /api/Families/{id}/received-flag; POST /api/SeasonalAid/campaigns/{campaignId}/beneficiaries |
| UC-PRJ-09 | List registered families الأسر المختارة للمشروع | HQ roles, charity | Reports the families selected for a project in a charity, with their delivery state. | Route `#/seasonal-aid/:id/beneficiaries` → GET /api/SeasonalAid/campaigns/{campaignId}/beneficiaries |
| UC-PRJ-10 | List non-registered families كافة الأسر للمشروع | HQ roles, charity | Reports the charity's families that were not selected for a project — the basis for rotation between campaigns so the same households are not always chosen. | Route `#/seasonal-aid/:id/eligible-families` → GET /api/SeasonalAid/campaigns/{campaignId}/eligible-families |
| UC-PRJ-11 | View the project summary ملخص المشروع | HQ roles | Aggregate figures for a project — families, individuals and amounts per charity. | GET /api/SeasonalAid/campaigns/{id}/report |
| UC-PRJ-12 | Project detail report by charity تقرير تفصيلي للمشروع حسب الجمعيات | HQ roles | Breaks a project down charity by charity with beneficiary detail; served by the campaign report query behind `POST /api/SeasonalAid/campaigns/{id}/report`. | Route `#/seasonal-aid/:id/report` → GET /api/SeasonalAid/campaigns/{id}/report |
| UC-PRJ-13 | Print project distribution documents طباعة كشوف التوزيع | Charity, HQ roles | Produces the printed distribution pack for a project in a charity: family cards, the primary distribution list and the secondary list. | /api/Reports/family-cards/export/pdf, /api/Reports/family-card-primary/export/pdf, /api/Reports/family-card-secondary/export/pdf |

### 17.D  Detailed use case specifications (from chapter 25)

Reproduced verbatim from chapter 25 of the master document — the fully expanded specification of this module’s critical end-to-end scenarios. Every other use case of the module is specified in §17.U.

**25.9 UC-PRJ-07 — Register families for an assistance project**


| Use case ID | UC-PRJ-07 |
| --- | --- |
| Name | Register families for a project — اختيار الأسر للمشروع |
| Primary actor | Charity user |
| Goal | Select which households benefit from a seasonal assistance campaign. |
| Pre-conditions | HQ has created the project and allocated a quota to the charity (UC-PRJ-02); the charity has registered families. |
| Main flow | 1. The user opens the Famproject state; the system lists the projects open to the charity. 2. The user selects a project; the system lists the charity's families with their current registration state. 3. The user selects the families to benefit, observing the quota. 4. The user saves; the client calls PUT /api/SeasonalAid/campaigns/{campaignId}/beneficiaries and the registrations are stored. 5. The distribution pack is printed — family cards and the primary and secondary lists (UC-PRJ-13). 6. As each family collects, the user marks it received (UC-PRJ-08). 7. HQ monitors the campaign through the registered, non-registered, summary and per-charity detail reports (UC-PRJ-09 to UC-PRJ-12). |
| Alternate flows | A1 — Quota exceeded. Further selections are rejected until others are removed. A2 — Rotation. The user consults the non-registered list (UC-PRJ-10) to prefer households not served in previous campaigns. |
| Post-conditions | The project's beneficiary list is fixed for the charity and each registration carries a delivery state. |
| Business rules | BR-23 A family may be registered once per project. BR-24 Only families owned by the charity may be selected. BR-25 Delivery confirmation is per family per project. |


### 17.S  Screen field specifications

Derived from the AngularJS views of this module. For every screen the table lists each data-entry field in the order it appears, the scope property it binds to, its control type, whether the form marks it mandatory, and the lookup or rule that governs it. Labels are reproduced as they appear on screen (Arabic, right-to-left).

#### 17.S.1  Screen `#/seasonal-aid`


| Property | Value |
| --- | --- |
| Angular route | `#/seasonal-aid` |
| Feature module | `seasonal-aid` (lazy-loaded) |
| Component | `CampaignListComponent` |
| Route status | implemented |
| Data-entry fields | 1 |
| Grids on the screen | 1 |
| Commands | 6 |


| Section | Field (as labelled) | Bound to | Control | Mandatory · source · rules |
| --- | --- | --- | --- | --- |
| — | الجمعية | CharityId | Drop-down list | Optional · options: lookup: Charities (+ كافة الجهات) · on change: getCharityData() |

Grids on this screen:

| Grid (row source) | Columns |
| --- | --- |
| project in Projects track by $index | الرقم · اسم المشروع · التاريخ |

Commands on this screen:

| Command | Handler | Shown when |
| --- | --- | --- |
| حفظ | DeleteProject() | always |
| (icon only) | AddProject() | always |
| (icon only) | EditProject(project.Id) | always |
| (icon only) | DeleteAnViewModel(project.Id) | always |
| (icon only) | GetNext() | always |
| (icon only) | GetPrev() | always |

#### 17.S.2  Screen `#/seasonal-aid/create` and `#/seasonal-aid/:id/edit`


| Property | Value |
| --- | --- |
| Angular route | `#/seasonal-aid/create` and `#/seasonal-aid/:id/edit` (one component, both routes) |
| Feature module | `seasonal-aid` (lazy-loaded) |
| Component | `CampaignFormComponent` |
| Route status | implemented |
| Data-entry fields | 7 |
| Grids on the screen | 0 |
| Commands | 2 |


| Section | Field (as labelled) | Bound to | Control | Mandatory · source · rules |
| --- | --- | --- | --- | --- |
| — | الجمعية | CharityId | Drop-down list | Optional · options: lookup: Charities (+ كافة الجهات) · on change: getCharityData() |
| إضافة مشروع | اسم المشروع | ProjectName | Text box | Mandatory |
| إضافة مشروع | تاريخ المشروع | ProjectDate | Date picker | Mandatory |
| إضافة مشروع | نبذة عن المشروع | ProjectComment | Text box | Mandatory |
| إضافة مشروع | معطل | ProjectHold | Check box | Optional |
| إضافة مشروع | تم الالغاء | ProjectCanceled | Check box | Optional |
| إضافة مشروع | تم الانتهاء | ProjectFinsished | Check box | Optional |

Commands on this screen:

| Command | Handler | Shown when |
| --- | --- | --- |
| اجماليات المشروع | ExportReportData() | IsEdit |
| حفظ | AddProject() | always |

#### 17.S.3  Screen `#/seasonal-aid/:id/beneficiaries`


| Property | Value |
| --- | --- |
| Angular route | `#/seasonal-aid/:id/beneficiaries` |
| Feature module | `seasonal-aid` (lazy-loaded) |
| Component | `BeneficiarySelectionComponent` |
| Route status | implemented |
| Data-entry fields | 4 |
| Grids on the screen | 3 |
| Commands | 30 |


| Section | Field (as labelled) | Bound to | Control | Mandatory · source · rules |
| --- | --- | --- | --- | --- |
| — | المشروع | ProjectId | Drop-down list | Mandatory · options: lookup: Projects · on change: GetFamilies() |
| — | الجمعية | CharityId | Drop-down list | Optional · options: lookup: Charities (+ كافة الجهات) · on change: getCharityData() |
| في حاله طباعه التقرير لن يتم التعديل | (unlabelled) | family.IsReceived | Check box | Optional · on click: Receive(this); read-only when IsDisabled |
| في حاله طباعه التقرير لن يتم التعديل | (unlabelled) | family.IsReceived | Check box | Optional · on click: Receive(this); read-only when IsDisabled |

Grids on this screen:

| Grid (row source) | Columns |
| --- | --- |
| family in AllFamilies \|orderBy:sortOfAllFamily track by $index | الرقم · أسم المستفيد · تاريخ أخر مشروع · حالة المستفيد · اسم الزوج/الزوجة · عدد الأبناء · عدد الايتام · عدد الايتام بكود · المحافظة · المركز · القرية · الإجراء |
| family in ProjectFamilies \| orderBy:sortOfPrimFamily track by $index | الرقم · أسم المستفيد · تاريخ أخر مشروع · حالة المستفيد · اسم الزوج/الزوجة · عدد الأبناء · عدد الايتام · تم الإستلام · حذف |
| family in ExtraFamilies \| orderBy:sortOfSecFamily track by $index | الرقم · أسم المستفيد · تاريخ أخر مشروع · حالة المستفيد · اسم الزوج/الزوجة · عدد الأبناء · عدد الايتام · تم الإستلام · حذف |

Commands on this screen:

| Command | Handler | Shown when |
| --- | --- | --- |
| طباعة الكشف الأساسي | PrintFamilyPrimary() | always |
| طباعة الكشف الإضافي | PrintFamilySecondary() | always |
| كروت التسليم | PrintFamilyCards() | always |
| إضافة الجميع الى الكشف الاساسى | AddAllToPrin() | always |
| (icon only) | SortAllFamily('SponserFullName',5) | always |
| (icon only) | SortAllFamily('LatestProjectDate',6) | always |
| (icon only) | SortAllFamily('Relation',1) | always |
| (icon only) | SortAllFamily('FatherFullName',2) | always |
| (icon only) | SortAllFamily('NoOfChildren',3) | always |
| (icon only) | SortAllFamily('NoOfOrphans',4) | always |
| (icon only) | SortAllFamily('NoOfOrphansWithCode',8) | always |
| (icon only) | SortAllFamily('RegionName',5) | always |
| (icon only) | SortAllFamily('CenterName',6) | always |
| (icon only) | SortAllFamily('Village',7) | always |
| (icon only) | AppendToProjectFamily(family.Id) | always |
| (icon only) | AppendToExtraFamily(family.Id) | always |
| (icon only) | SortPrimFamily('SponserFullName',5) | always |
| (icon only) | SortPrimFamily('Relation',1) | always |
| (icon only) | SortPrimFamily('FatherFullName',2) | always |
| (icon only) | SortPrimFamily('NoOfChildren',3) | always |
| (icon only) | SortPrimFamily('NoOfOrphans',4) | always |
| (icon only) | Receive(this) | always |
| (icon only) | RemoveFromProjectFamily(family.Id) | always |
| (icon only) | SortSecFamily('SponserFullName',5) | always |
| (icon only) | SortSecFamily('Relation',1) | always |
| (icon only) | SortSecFamily('FatherFullName',2) | always |
| (icon only) | SortSecFamily('NoOfChildren',3) | always |
| (icon only) | SortSecFamily('TotalIncome',4) | always |
| (icon only) | RemoveFromExtraFamily(family.Id,$index) | always |
| حفظ | AddProjectFamily() | always |

#### 17.S.4  Screen `#/seasonal-aid/:id/beneficiaries`


| Property | Value |
| --- | --- |
| Angular route | `#/seasonal-aid/:id/beneficiaries` |
| Feature module | `seasonal-aid` (lazy-loaded) |
| Component | `BeneficiarySelectionComponent` |
| Route status | implemented |
| Data-entry fields | 2 |
| Grids on the screen | 1 |
| Commands | 2 |


| Section | Field (as labelled) | Bound to | Control | Mandatory · source · rules |
| --- | --- | --- | --- | --- |
| — | المشروع | ProjectID | Drop-down list | Optional · options: lookup: Projects · on change: getCharityData() |
| — | الجمعية | CharityId | Drop-down list | Optional · options: lookup: Charities (+ كافة الجهات) · on change: getCharityData() |

Grids on this screen:

| Grid (row source) | Columns |
| --- | --- |
| result in RegisteredResultList track by $index | م · اسم المستفيد · اسم الزوج/الزوجة · الحالة الاجتماعية · الرقم القومى · عدد الابناء · عدد الايتام · الهواتف · القرية · المركز · المحافظة · الكشف · اخر تحديث · تسليم |

Commands on this screen:

| Command | Handler | Shown when |
| --- | --- | --- |
| استخراج البيانات | ExportData() | always |
| (icon only) | Update(1,result.ProjectFamilyId,this) | always |

#### 17.S.5  Screen `#/seasonal-aid/:id/eligible-families`


| Property | Value |
| --- | --- |
| Angular route | `#/seasonal-aid/:id/eligible-families` |
| Feature module | `seasonal-aid` (lazy-loaded) |
| Component | `EligibleFamiliesComponent` |
| Route status | planned |
| Data-entry fields | 2 |
| Grids on the screen | 0 |
| Commands | 1 |


| Section | Field (as labelled) | Bound to | Control | Mandatory · source · rules |
| --- | --- | --- | --- | --- |
| — | المشروع | ProjectID | Drop-down list | Optional · options: lookup: Projects · on change: getCharityData() |
| — | الجمعية | CharityId | Drop-down list | Optional · options: lookup: Charities (+ كافة الجهات) · on change: getCharityData() |

Commands on this screen:

| Command | Handler | Shown when |
| --- | --- | --- |
| استخراج البيانات | ExportData() | always |

#### 17.S.6  Screen `#/seasonal-aid/:id/report`


| Property | Value |
| --- | --- |
| Angular route | `#/seasonal-aid/:id/report` |
| Feature module | `seasonal-aid` (lazy-loaded) |
| Component | `CampaignReportComponent` |
| Route status | planned |
| Data-entry fields | 2 |
| Grids on the screen | 0 |
| Commands | 4 |


| Section | Field (as labelled) | Bound to | Control | Mandatory · source · rules |
| --- | --- | --- | --- | --- |
| — | الجمعية | CharityId | Drop-down list | Optional · options: lookup: Charities (+ كافة الجهات) · on change: getCharityData() |
| — | (unlabelled) | ProjectId | Drop-down list | Optional · options: lookup: Projects |

Commands on this screen:

| Command | Handler | Shown when |
| --- | --- | --- |
| حفظ | DeleteProject() | always |
| (icon only) | ExportReportData() | always |
| (icon only) | GetNext() | always |
| (icon only) | GetPrev() | always |

#### 17.S.7  Screen `#/seasonal-aid/:id/report`


| Property | Value |
| --- | --- |
| Angular route | `#/seasonal-aid/:id/report` |
| Feature module | `seasonal-aid` (lazy-loaded) |
| Component | `CampaignReportComponent` |
| Route status | planned |
| Data-entry fields | 2 |
| Grids on the screen | 1 |
| Commands | 2 |


| Section | Field (as labelled) | Bound to | Control | Mandatory · source · rules |
| --- | --- | --- | --- | --- |
| — | المشروع | ProjectId | Drop-down list | Mandatory · options: lookup: Projects (+ اختر المشروع) |
| — | الجمعية | CharityId | Drop-down list | Optional · options: lookup: Charities (+ كافة الجهات) · on change: getCharityData() |

Grids on this screen:

| Grid (row source) | Columns |
| --- | --- |
| row in Rows track by $index | م · اسم الجمعية · عدد الاسر المستلمة في الكشف الاساسي · عدد الاسر المستلمة في الكشف الاحتياطي · اجمالي عدد الاسر المستلمة · عدد الابناء · عدد الايتام المكودين · عدد الارامل · عدد المطلقات · عدد المتزوجين · عدد الهجر |

Commands on this screen:

| Command | Handler | Shown when |
| --- | --- | --- |
| بحث | Search() | always |
| تصدير إلى إكسل | ExportToExcel() | always |

### 17.U  Use case scenarios

One expanded scenario for every use case of this module. Pre-conditions, flows and post-conditions are derived from the screen that hosts the function, the Web API action that serves it and the business-layer method that executes it; quoted messages are the literal strings returned by the business layer.

#### 17.U.1  UC-PRJ-01 — List projects المشاريع


| Item | Specification |
| --- | --- |
| Use case ID | UC-PRJ-01 |
| Name | List projects المشاريع |
| Type | Browse a list |
| Primary actor | Gen. Director, Staff |
| Secondary actors | The system (Web API + business layer); the database. |
| Summary | Paged list of assistance projects for a charity or across all charities, with dates, budget and status. |
| Trigger | The actor opens the screen at `#/seasonal-aid` from the module menu. |
| Pre-conditions | 1. The actor is authenticated; the JWT access token is valid and the client holds the role, charity and country claims it carries.<br>2. The actor holds one of: Gen. Director, Staff.<br>3. Caller identity comes from the JWT, never from the request; the application service scopes the query to the charity (and country) that owns the user, unless the role is an HQ role permitted to pass an explicit `charityId` filter.<br>4. The SPA route `#/seasonal-aid` has loaded and its reference-data lookups have been populated.<br>5. The record is not closed by an add/edit lock (IsLocked). |
| Main flow | 1. The actor navigates to the screen at `#/seasonal-aid`.<br>2. The system loads the filter lookups the screen offers and, for HQ roles, the charity selector.<br>3. The SPA issues `GET /api/SeasonalAid/campaigns` carrying charityId, pagenum, userId.<br>4. `SeasonalAidController` binds the typed request DTO and delegates to the application service.<br>5. `ISeasonalAidService` validates the payload with its FluentValidation validator, applies the business rules and the charity scope, and persists through `IUnitOfWork`.<br>6. The result set is scoped to the caller’s charity and country, ordered and paged.<br>7. The SPA renders the page with the columns listed in the screen specification and the paging control. |
| Alternate flows | • An HQ role (General Director, Financial Director, Staff) may pass an explicit charity id and so read across the charity boundary; a charity user may not and always sees its own data.<br>• No row matches the criteria — the grid renders empty and the paging control reports zero pages. |
| Exception flows | • The session has expired or the role is not permitted — the request is rejected and the SPA routes back to the login state.<br>• The charity is closed by a lock (IsLocked) — the write is refused. |
| Post-conditions | • No stored data is changed — the operation is a read. |
| Realisation | Route `#/seasonal-aid` → `CampaignListComponent`<br>`GET /api/SeasonalAid/campaigns` → `SeasonalAidController` → `ISeasonalAidService` |

#### 17.U.2  UC-PRJ-02 — Create a project إضافة مشروع


| Item | Specification |
| --- | --- |
| Use case ID | UC-PRJ-02 |
| Name | Create a project إضافة مشروع |
| Type | Create a record |
| Primary actor | Gen. Director |
| Secondary actors | The system (Web API + business layer); the database. |
| Summary | Defines the campaign — name, type, period, per-family amount or in-kind item, target charities and quota — making it available for family selection. |
| Trigger | The actor presses «حفظ» on the screen project. |
| Pre-conditions | 1. The actor is authenticated; the JWT access token is valid and the client holds the role, charity and country claims it carries.<br>2. The actor holds one of: Gen. Director.<br>3. The SPA route `#/seasonal-aid/:id/edit` has loaded and its reference-data lookups have been populated.<br>4. The record is not closed by an add/edit lock (IsLocked). |
| Main flow | 1. The actor navigates to the screen at `#/seasonal-aid/:id/edit`. The screen opens in add mode.<br>2. The system loads the reference data the form needs (regions, centres, banks, types and the other lookup lists bound to the drop-downs).<br>3. The actor completes the input fields. Mandatory fields: اسم المشروع، تاريخ المشروع، نبذة عن المشروع.<br>4. The actor presses «حفظ» (AddProject()).<br>5. The SPA issues `POST /api/SeasonalAid/campaigns` carrying typed request DTO obj.<br>6. `SeasonalAidController` binds the typed request DTO and delegates to the application service.<br>7. `ISeasonalAidService` validates the payload with its FluentValidation validator, applies the business rules and the charity scope, and persists through `IUnitOfWork`.<br>8. The business layer validates the payload, stamps the owning charity and the creating user, and persists the new record.<br>9. The system returns the outcome and the SPA confirms the save and returns to the list screen. |
| Alternate flows | • The actor abandons the form before saving — nothing is written and the record keeps its previous state. |
| Exception flows | • The session has expired or the role is not permitted — the request is rejected and the SPA routes back to the login state.<br>• A mandatory field is empty or fails its format check — the save is refused and the field is flagged on the form.<br>• The charity is closed by a lock (IsLocked) — the write is refused. |
| Post-conditions | • A new record exists, owned by the charity of the creating user, and appears in the list screen of the module. |
| Realisation | Route `#/seasonal-aid/create` → `CampaignFormComponent`<br>`POST /api/SeasonalAid/campaigns` → `SeasonalAidController` → `ISeasonalAidService` |

#### 17.U.3  UC-PRJ-03 — View a project عرض المشروع


| Item | Specification |
| --- | --- |
| Use case ID | UC-PRJ-03 |
| Name | View a project عرض المشروع |
| Type | Read a record |
| Primary actor | Gen. Director, Staff, charity |
| Secondary actors | The system (Web API + business layer); the database. |
| Summary | Loads one project's definition and its current registration figures. |
| Trigger | The actor selects the function from the module menu or the screen that hosts it. |
| Pre-conditions | 1. The actor is authenticated; the JWT access token is valid and the client holds the role, charity and country claims it carries.<br>2. The actor holds one of: Gen. Director, Staff, charity.<br>3. Caller identity comes from the JWT, never from the request; the application service scopes the query to the charity (and country) that owns the user, unless the role is an HQ role permitted to pass an explicit `charityId` filter.<br>4. The target record exists and its identifier is known to the screen.<br>5. The record is not closed by an add/edit lock (IsLocked). |
| Main flow | 1. The actor reaches the function from the screen of this module that hosts it.<br>2. The actor selects the record to open.<br>3. The SPA issues `GET /api/SeasonalAid/campaigns` carrying id, userId.<br>4. `SeasonalAidController` binds the typed request DTO and delegates to the application service.<br>5. `ISeasonalAidService` validates the payload with its FluentValidation validator, applies the business rules and the charity scope, and persists through `IUnitOfWork`.<br>6. The system returns the record with its child collections and the SPA binds them to the form fields. |
| Alternate flows | • An HQ role (General Director, Financial Director, Staff) may pass an explicit charity id and so read across the charity boundary; a charity user may not and always sees its own data. |
| Exception flows | • The session has expired or the role is not permitted — the request is rejected and the SPA routes back to the login state.<br>• The charity is closed by a lock (IsLocked) — the write is refused. |
| Post-conditions | • No stored data is changed — the operation is a read. |
| Realisation | `GET /api/SeasonalAid/campaigns` → `SeasonalAidController` → `ISeasonalAidService` |

#### 17.U.4  UC-PRJ-04 — Update a project تعديل المشروع


| Item | Specification |
| --- | --- |
| Use case ID | UC-PRJ-04 |
| Name | Update a project تعديل المشروع |
| Type | Update a record |
| Primary actor | Gen. Director |
| Secondary actors | The system (Web API + business layer); the database. |
| Summary | Amends project parameters and quotas while the campaign is open. |
| Trigger | The actor selects the function from the module menu or the screen that hosts it. |
| Pre-conditions | 1. The actor is authenticated; the JWT access token is valid and the client holds the role, charity and country claims it carries.<br>2. The actor holds one of: Gen. Director.<br>3. The target record exists and its identifier is known to the screen.<br>4. The record is not closed by an add/edit lock (IsLocked). |
| Main flow | 1. The actor reaches the function from the screen of this module that hosts it. The screen opens in edit mode on the selected record.<br>2. The system loads the current values into the form.<br>3. The actor changes the fields to be corrected.<br>4. The actor presses the command button of the screen.<br>5. The SPA issues `PUT /api/SeasonalAid/campaigns` carrying typed request DTO obj.<br>6. `SeasonalAidController` binds the typed request DTO and delegates to the application service.<br>7. `ISeasonalAidService` validates the payload with its FluentValidation validator, applies the business rules and the charity scope, and persists through `IUnitOfWork`.<br>8. The business layer re-validates the payload, applies the changes and persists them.<br>9. The system returns the outcome and the SPA refreshes the screen. |
| Alternate flows | • The actor abandons the form before saving — nothing is written and the record keeps its previous state. |
| Exception flows | • The session has expired or the role is not permitted — the request is rejected and the SPA routes back to the login state.<br>• A mandatory field is empty or fails its format check — the save is refused and the field is flagged on the form.<br>• The charity is closed by a lock (IsLocked) — the write is refused. |
| Post-conditions | • The stored record carries the new values; no other record is affected. |
| Realisation | `PUT /api/SeasonalAid/campaigns` → `SeasonalAidController` → `ISeasonalAidService` |

#### 17.U.5  UC-PRJ-05 — Delete a project حذف المشروع


| Item | Specification |
| --- | --- |
| Use case ID | UC-PRJ-05 |
| Name | Delete a project حذف المشروع |
| Type | Delete a record |
| Primary actor | Gen. Director |
| Secondary actors | The system (Web API + business layer); the database. |
| Summary | Removes a project created in error together with its family registrations. |
| Trigger | The actor selects the function from the module menu or the screen that hosts it. |
| Pre-conditions | 1. The actor is authenticated; the JWT access token is valid and the client holds the role, charity and country claims it carries.<br>2. The actor holds one of: Gen. Director.<br>3. Caller identity comes from the JWT, never from the request; the application service scopes the query to the charity (and country) that owns the user, unless the role is an HQ role permitted to pass an explicit `charityId` filter.<br>4. The target record exists and its identifier is known to the screen. |
| Main flow | 1. The actor reaches the function from the screen of this module that hosts it.<br>2. The actor selects the record and requests its deletion.<br>3. The SPA asks the actor to confirm.<br>4. The SPA issues `DELETE /api/SeasonalAid/campaigns` carrying id, userId.<br>5. `SeasonalAidController` binds the typed request DTO and delegates to the application service.<br>6. `ISeasonalAidService` validates the payload with its FluentValidation validator, applies the business rules and the charity scope, and persists through `IUnitOfWork`.<br>7. The business layer checks that the record may still be removed and deletes it (or marks it removed).<br>8. The system returns the outcome and the SPA drops the row from the grid. |
| Alternate flows | • An HQ role (General Director, Financial Director, Staff) may pass an explicit charity id and so read across the charity boundary; a charity user may not and always sees its own data. |
| Exception flows | • The session has expired or the role is not permitted — the request is rejected and the SPA routes back to the login state. |
| Post-conditions | • The record is no longer returned by the list and read endpoints of the module. |
| Realisation | `DELETE /api/SeasonalAid/campaigns` → `SeasonalAidController` → `ISeasonalAidService` |

#### 17.U.6  UC-PRJ-06 — Open the family-selection screen مشاريع الأسر


| Item | Specification |
| --- | --- |
| Use case ID | UC-PRJ-06 |
| Name | Open the family-selection screen مشاريع الأسر |
| Type | Read a record |
| Primary actor | Charity |
| Secondary actors | The system (Web API + business layer); the database. |
| Summary | Lists the projects open to the charity and, for the chosen project, the charity's families with their current registration state, ready to be selected. |
| Trigger | The actor opens the screen at `#/seasonal-aid/:id/beneficiaries` from the module menu. |
| Pre-conditions | 1. The actor is authenticated; the JWT access token is valid and the client holds the role, charity and country claims it carries.<br>2. The actor holds one of: charity.<br>3. Caller identity comes from the JWT, never from the request; the application service scopes the query to the charity (and country) that owns the user, unless the role is an HQ role permitted to pass an explicit `charityId` filter.<br>4. The SPA route `#/seasonal-aid/:id/beneficiaries` has loaded and its reference-data lookups have been populated.<br>5. The target record exists and its identifier is known to the screen.<br>6. The record is not closed by an add/edit lock (IsLocked). |
| Main flow | 1. The actor navigates to the screen at `#/seasonal-aid/:id/beneficiaries`.<br>2. The actor selects the record to open.<br>3. The SPA issues `GET /api/SeasonalAid/campaigns/{campaignId}/beneficiaries` carrying charityId, userId.<br>4. `SeasonalAidController` binds the typed request DTO and delegates to the application service.<br>5. `ISeasonalAidService` validates the payload with its FluentValidation validator, applies the business rules and the charity scope, and persists through `IUnitOfWork`.<br>6. The system returns the record with its child collections and the SPA binds them to the form fields. |
| Alternate flows | • An HQ role (General Director, Financial Director, Staff) may pass an explicit charity id and so read across the charity boundary; a charity user may not and always sees its own data. |
| Exception flows | • The session has expired or the role is not permitted — the request is rejected and the SPA routes back to the login state.<br>• The charity is closed by a lock (IsLocked) — the write is refused. |
| Post-conditions | • No stored data is changed — the operation is a read. |
| Realisation | Route `#/seasonal-aid/:id/beneficiaries` → `BeneficiarySelectionComponent`<br>`GET /api/SeasonalAid/campaigns/{campaignId}/beneficiaries` → `SeasonalAidController` → `ISeasonalAidService` |

#### 17.U.7  UC-PRJ-07 — Register families for a project اختيار الأسر للمشروع


| Item | Specification |
| --- | --- |
| Use case ID | UC-PRJ-07 |
| Name | Register families for a project اختيار الأسر للمشروع |
| Type | Update a record |
| Primary actor | Charity |
| Secondary actors | The system (Web API + business layer); the database. |
| Summary | Selects or de-selects families as beneficiaries of the project, subject to the quota defined by HQ; the selections are saved as project-family registrations. |
| Trigger | The actor selects the function from the module menu or the screen that hosts it. |
| Pre-conditions | 1. The actor is authenticated; the JWT access token is valid and the client holds the role, charity and country claims it carries.<br>2. The actor holds one of: charity.<br>3. The target record exists and its identifier is known to the screen. |
| Main flow | 1. The actor reaches the function from the screen of this module that hosts it. The screen opens in edit mode on the selected record.<br>2. The system loads the current values into the form.<br>3. The actor changes the fields to be corrected.<br>4. The actor presses the command button of the screen.<br>5. The SPA issues `PUT /api/SeasonalAid/campaigns/{campaignId}/beneficiaries` carrying typed request DTO obj.<br>6. `SeasonalAidController` binds the typed request DTO and delegates to the application service.<br>7. `ISeasonalAidService` validates the payload with its FluentValidation validator, applies the business rules and the charity scope, and persists through `IUnitOfWork`.<br>8. The business layer re-validates the payload, applies the changes and persists them.<br>9. The system returns the outcome and the SPA refreshes the screen. |
| Alternate flows | • The actor abandons the form before saving — nothing is written and the record keeps its previous state. |
| Exception flows | • The session has expired or the role is not permitted — the request is rejected and the SPA routes back to the login state.<br>• The business layer returns «Operation Faild» and the operation is not applied.<br>• A mandatory field is empty or fails its format check — the save is refused and the field is flagged on the form. |
| Post-conditions | • The stored record carries the new values; no other record is affected. |
| Realisation | `PUT /api/SeasonalAid/campaigns/{campaignId}/beneficiaries` → `SeasonalAidController` → `ISeasonalAidService` |

#### 17.U.8  UC-PRJ-08 — Confirm a family received the assistance تأكيد استلام الأسرة


| Item | Specification |
| --- | --- |
| Use case ID | UC-PRJ-08 |
| Name | Confirm a family received the assistance تأكيد استلام الأسرة |
| Type | Read a record |
| Primary actor | Charity |
| Secondary actors | The system (Web API + business layer); the database. |
| Summary | Marks the individual project-family registration as delivered, closing the distribution record for that household. |
| Trigger | The actor selects the function from the module menu or the screen that hosts it. |
| Pre-conditions | 1. The actor is authenticated; the JWT access token is valid and the client holds the role, charity and country claims it carries.<br>2. The actor holds one of: charity.<br>3. Caller identity comes from the JWT, never from the request; the application service scopes the query to the charity (and country) that owns the user, unless the role is an HQ role permitted to pass an explicit `charityId` filter.<br>4. The target record exists and its identifier is known to the screen. |
| Main flow | 1. The actor reaches the function from the screen of this module that hosts it.<br>2. The actor selects the record to open.<br>3. The SPA issues `PUT /api/Families/{id}/received-flag` carrying familyId, projectId, userId, isReceived.<br>4. `FamiliesController` binds the typed request DTO and delegates to the application service.<br>5. `ISeasonalAidService` validates the payload with its FluentValidation validator, applies the business rules and the charity scope, and persists through `IUnitOfWork`.<br>6. The system returns the record with its child collections and the SPA binds them to the form fields. |
| Alternate flows | • An HQ role (General Director, Financial Director, Staff) may pass an explicit charity id and so read across the charity boundary; a charity user may not and always sees its own data. |
| Exception flows | • The session has expired or the role is not permitted — the request is rejected and the SPA routes back to the login state. |
| Post-conditions | • No stored data is changed — the operation is a read. |
| Realisation | `PUT /api/Families/{id}/received-flag` · `POST /api/SeasonalAid/campaigns/{campaignId}/beneficiaries` → `FamiliesController` → `IFamilyService` |

#### 17.U.9  UC-PRJ-09 — List registered families الأسر المختارة للمشروع


| Item | Specification |
| --- | --- |
| Use case ID | UC-PRJ-09 |
| Name | List registered families الأسر المختارة للمشروع |
| Type | Create a record |
| Primary actor | HQ roles, charity |
| Secondary actors | The system (Web API + business layer); the database. |
| Summary | Reports the families selected for a project in a charity, with their delivery state. |
| Trigger | The actor presses «استخراج البيانات» on the screen FamilyProjectRegistered. |
| Pre-conditions | 1. The actor is authenticated; the JWT access token is valid and the client holds the role, charity and country claims it carries.<br>2. The actor holds one of: HQ roles, charity.<br>3. Caller identity comes from the JWT, never from the request; the application service scopes the query to the charity (and country) that owns the user, unless the role is an HQ role permitted to pass an explicit `charityId` filter.<br>4. The SPA route `#/seasonal-aid/:id/beneficiaries` has loaded and its reference-data lookups have been populated. |
| Main flow | 1. The actor navigates to the screen at `#/seasonal-aid/:id/beneficiaries`. The screen opens in add mode.<br>2. The system loads the reference data the form needs (regions, centres, banks, types and the other lookup lists bound to the drop-downs).<br>3. The actor completes the input fields of the form.<br>4. The actor presses «استخراج البيانات» (ExportData()).<br>5. The SPA issues `GET /api/SeasonalAid/campaigns/{campaignId}/beneficiaries` carrying charityId, projectId, userId.<br>6. `SeasonalAidController` binds the typed request DTO and delegates to the application service.<br>7. `ISeasonalAidService` validates the payload with its FluentValidation validator, applies the business rules and the charity scope, and persists through `IUnitOfWork`.<br>8. The business layer validates the payload, stamps the owning charity and the creating user, and persists the new record.<br>9. The system returns the outcome and the SPA confirms the save and returns to the list screen. |
| Alternate flows | • An HQ role (General Director, Financial Director, Staff) may pass an explicit charity id and so read across the charity boundary; a charity user may not and always sees its own data.<br>• The actor abandons the form before saving — nothing is written and the record keeps its previous state. |
| Exception flows | • The session has expired or the role is not permitted — the request is rejected and the SPA routes back to the login state.<br>• A mandatory field is empty or fails its format check — the save is refused and the field is flagged on the form. |
| Post-conditions | • A new record exists, owned by the charity of the creating user, and appears in the list screen of the module. |
| Realisation | Route `#/seasonal-aid/:id/beneficiaries` → `BeneficiarySelectionComponent`<br>`GET /api/SeasonalAid/campaigns/{campaignId}/beneficiaries` → `SeasonalAidController` → `ISeasonalAidService` |

#### 17.U.10  UC-PRJ-10 — List non-registered families كافة الأسر للمشروع


| Item | Specification |
| --- | --- |
| Use case ID | UC-PRJ-10 |
| Name | List non-registered families كافة الأسر للمشروع |
| Type | Create a record |
| Primary actor | HQ roles, charity |
| Secondary actors | The system (Web API + business layer); the database. |
| Summary | Reports the charity's families that were not selected for a project — the basis for rotation between campaigns so the same households are not always chosen. |
| Trigger | The actor presses «استخراج البيانات» on the screen FamilyProjectNotRegistered. |
| Pre-conditions | 1. The actor is authenticated; the JWT access token is valid and the client holds the role, charity and country claims it carries.<br>2. The actor holds one of: HQ roles, charity.<br>3. Caller identity comes from the JWT, never from the request; the application service scopes the query to the charity (and country) that owns the user, unless the role is an HQ role permitted to pass an explicit `charityId` filter.<br>4. The SPA route `#/seasonal-aid/:id/eligible-families` has loaded and its reference-data lookups have been populated. |
| Main flow | 1. The actor navigates to the screen at `#/seasonal-aid/:id/eligible-families`. The screen opens in add mode.<br>2. The system loads the reference data the form needs (regions, centres, banks, types and the other lookup lists bound to the drop-downs).<br>3. The actor completes the input fields of the form.<br>4. The actor presses «استخراج البيانات» (ExportData()).<br>5. The SPA issues `GET /api/SeasonalAid/campaigns/{campaignId}/eligible-families` carrying userId, projectId, charityId.<br>6. `ReportsController` binds the typed request DTO and delegates to the application service.<br>7. `ISeasonalAidService` validates the payload with its FluentValidation validator, applies the business rules and the charity scope, and persists through `IUnitOfWork`.<br>8. The business layer validates the payload, stamps the owning charity and the creating user, and persists the new record.<br>9. The system returns the outcome and the SPA confirms the save and returns to the list screen. |
| Alternate flows | • An HQ role (General Director, Financial Director, Staff) may pass an explicit charity id and so read across the charity boundary; a charity user may not and always sees its own data.<br>• The actor abandons the form before saving — nothing is written and the record keeps its previous state. |
| Exception flows | • The session has expired or the role is not permitted — the request is rejected and the SPA routes back to the login state.<br>• A mandatory field is empty or fails its format check — the save is refused and the field is flagged on the form. |
| Post-conditions | • A new record exists, owned by the charity of the creating user, and appears in the list screen of the module. |
| Realisation | Route `#/seasonal-aid/:id/eligible-families` → `EligibleFamiliesComponent`<br>`GET /api/SeasonalAid/campaigns/{campaignId}/eligible-families` → `SeasonalAidController` → `ISeasonalAidService` |

#### 17.U.11  UC-PRJ-11 — View the project summary ملخص المشروع


| Item | Specification |
| --- | --- |
| Use case ID | UC-PRJ-11 |
| Name | View the project summary ملخص المشروع |
| Type | Read a record |
| Primary actor | HQ roles |
| Secondary actors | The system (Web API + business layer); the database. |
| Summary | Aggregate figures for a project — families, individuals and amounts per charity. |
| Trigger | The actor selects the function from the module menu or the screen that hosts it. |
| Pre-conditions | 1. The actor is authenticated; the JWT access token is valid and the client holds the role, charity and country claims it carries.<br>2. The actor holds one of: HQ roles.<br>3. Caller identity comes from the JWT, never from the request; the application service scopes the query to the charity (and country) that owns the user, unless the role is an HQ role permitted to pass an explicit `charityId` filter.<br>4. The target record exists and its identifier is known to the screen. |
| Main flow | 1. The actor reaches the function from the screen of this module that hosts it.<br>2. The actor selects the record to open.<br>3. The SPA issues `GET /api/SeasonalAid/campaigns/{id}/report` carrying projectId, userId.<br>4. `OrphanPaymentsController` binds the typed request DTO and delegates to the application service.<br>5. `IReportService` validates the payload with its FluentValidation validator, applies the business rules and the charity scope, and persists through `IUnitOfWork`.<br>6. The system returns the record with its child collections and the SPA binds them to the form fields. |
| Alternate flows | • An HQ role (General Director, Financial Director, Staff) may pass an explicit charity id and so read across the charity boundary; a charity user may not and always sees its own data. |
| Exception flows | • The session has expired or the role is not permitted — the request is rejected and the SPA routes back to the login state. |
| Post-conditions | • No stored data is changed — the operation is a read. |
| Realisation | `GET /api/SeasonalAid/campaigns/{id}/report` → `SeasonalAidController` → `ISeasonalAidService` |

#### 17.U.12  UC-PRJ-12 — Project detail report by charity تقرير تفصيلي للمشروع حسب الجمعيات


| Item | Specification |
| --- | --- |
| Use case ID | UC-PRJ-12 |
| Name | Project detail report by charity تقرير تفصيلي للمشروع حسب الجمعيات |
| Type | Query a report |
| Primary actor | HQ roles |
| Secondary actors | The system (Web API + business layer); the database. |
| Summary | Breaks a project down charity by charity with beneficiary detail; served by the campaign report query behind `POST /api/SeasonalAid/campaigns/{id}/report`. |
| Trigger | The actor opens the screen at `#/seasonal-aid/:id/report` from the module menu. |
| Pre-conditions | 1. The actor is authenticated; the JWT access token is valid and the client holds the role, charity and country claims it carries.<br>2. The actor holds one of: HQ roles.<br>3. Caller identity comes from the JWT, never from the request; the application service scopes the query to the charity (and country) that owns the user, unless the role is an HQ role permitted to pass an explicit `charityId` filter.<br>4. The SPA route `#/seasonal-aid/:id/report` has loaded and its reference-data lookups have been populated. |
| Main flow | 1. The actor navigates to the screen at `#/seasonal-aid/:id/report`.<br>2. The actor sets the report criteria (period, charity, country and the other filters the screen offers).<br>3. The actor runs the report.<br>4. The SPA issues `GET /api/SeasonalAid/campaigns/{id}/report` carrying projectId, charityId, userId.<br>5. `SeasonalAidController` binds the typed request DTO and delegates to the application service.<br>6. `ISeasonalAidService` validates the payload with its FluentValidation validator, applies the business rules and the charity scope, and persists through `IUnitOfWork`.<br>7. The business layer composes the projection, scoped to the caller’s charity and country.<br>8. The result is rendered in the on-screen grid, from where it can be printed or exported. |
| Alternate flows | • An HQ role (General Director, Financial Director, Staff) may pass an explicit charity id and so read across the charity boundary; a charity user may not and always sees its own data.<br>• No row matches the criteria — the grid renders empty and the paging control reports zero pages. |
| Exception flows | • The session has expired or the role is not permitted — the request is rejected and the SPA routes back to the login state.<br>• The business layer returns «Operation Faild» and the operation is not applied. |
| Post-conditions | • No stored data is changed — the operation is a read. |
| Realisation | Route `#/seasonal-aid/:id/report` → `CampaignReportComponent`<br>`GET /api/SeasonalAid/campaigns/{id}/report` → `SeasonalAidController` → `ISeasonalAidService` |

#### 17.U.13  UC-PRJ-13 — Print project distribution documents طباعة كشوف التوزيع


| Item | Specification |
| --- | --- |
| Use case ID | UC-PRJ-13 |
| Name | Print project distribution documents طباعة كشوف التوزيع |
| Type | Print / produce a document |
| Primary actor | Charity, HQ roles |
| Secondary actors | The system (Web API + business layer); the database. the report and export endpoints of `api/Reports` as the rendering path — jsPDF for PDF and ExcelJS for Excel. |
| Summary | Produces the printed distribution pack for a project in a charity: family cards, the primary distribution list and the secondary list. |
| Trigger | The actor selects the function from the module menu or the screen that hosts it. |
| Pre-conditions | 1. The actor is authenticated; the JWT access token is valid and the client holds the role, charity and country claims it carries.<br>2. The actor holds one of: charity, HQ roles.<br>3. Caller identity comes from the JWT, never from the request; the application service scopes the query to the charity (and country) that owns the user, unless the role is an HQ role permitted to pass an explicit `charityId` filter.<br>4. The target record exists and its identifier is known to the screen. |
| Main flow | 1. The actor reaches the function from the screen of this module that hosts it.<br>2. The actor selects the record or the range to be printed.<br>3. The actor presses the command button of the screen.<br>4. The browser opens `POST /api/Reports/family-cards/export/pdf` with string charityId, string projectId, string userId.<br>5. `ReportsController` resolves the report key, applies the filter, and renders the document from the dataset the application service returns.<br>6. The rendered document is streamed back as a PDF for viewing and printing. |
| Alternate flows | • An HQ role (General Director, Financial Director, Staff) may pass an explicit charity id and so read across the charity boundary; a charity user may not and always sees its own data. |
| Exception flows | • The session has expired or the role is not permitted — the request is rejected and the SPA routes back to the login state.<br>• The business layer returns «Operation Faild» and the operation is not applied.<br>• The report definition or a required parameter is missing — the print action fails and no document is produced. |
| Post-conditions | • A printable document has been produced. Where the module records printing, the row is flagged as printed. |
| Realisation | `POST /api/Reports/family-cards/export/pdf` · `POST /api/Reports/family-card-primary/export/pdf` · `POST /api/Reports/family-card-secondary/export/pdf` → `ReportsController` → `IReportService` |

### 17.A  Annex - Angular routes of this module

Routes are hash-based (`useHash: true`), rendered inside `MainLayoutComponent` behind
`AuthGuard` and `PermissionGuard`. *implemented* means the route exists in
`Frontend/src/app`; *planned* means it is specified here and not yet built.

| Angular route | Feature module | Component | Status |
| --- | --- | --- | --- |
| `#/seasonal-aid` | `seasonal-aid` | `CampaignListComponent` | implemented |
| `#/seasonal-aid/create` | `seasonal-aid` | `CampaignFormComponent` | implemented |
| `#/seasonal-aid/:id` | `seasonal-aid` | `CampaignDetailComponent` | implemented |
| `#/seasonal-aid/:id/edit` | `seasonal-aid` | `CampaignFormComponent` | implemented |
| `#/seasonal-aid/:id/beneficiaries` | `seasonal-aid` | `BeneficiarySelectionComponent` | implemented |
| `#/seasonal-aid/:id/distribution` | `seasonal-aid` | `DistributionRecordComponent` | implemented |
| `#/seasonal-aid/:id/eligible-families` | `seasonal-aid` | `EligibleFamiliesComponent` | planned |
| `#/seasonal-aid/:id/report` | `seasonal-aid` | `CampaignReportComponent` | planned |

### 17.B  Annex - API controllers of this module

Every controller inherits `ApiController` (`[Authorize]`, route `api/[controller]`) and
returns `Framework.Core.ApiResponse<T>`. Business rules live in the application service
behind each controller, never in the controller itself.

| Controller | Base route | Responsibility |
| --- | --- | --- |
| `FamiliesController` | `api/Families` | Family read by type; project receipt flag. |
| `OrphanPaymentsController` | `api/OrphanPayments` | Transfer-number import; project summary. |
| `SeasonalAidController` | `api/SeasonalAid` | Assistance project CRUD. Project family selection and registration. Registered families per project. Project detail by charity. |

---

Shared context - system architecture, actors and roles, the sponsorship lifecycle, the full module map and all appendices - is in the companion document [00-Overview-and-Common-Context](00-Overview-and-Common-Context.md). The delivery backlog for this module is epic EP-12 in [00-EPICS-and-User-Stories](00-EPICS-and-User-Stories.md).

