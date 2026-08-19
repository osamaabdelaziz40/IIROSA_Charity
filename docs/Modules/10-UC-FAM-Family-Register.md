# WAR.IIROSA - Family Register

سجل الاسر | use case prefix `UC-FAM` | chapter 10 of the master document


| Field | Value |
| --- | --- |
| Document title | WAR.IIROSA - Module Specification: Family Register |
| Module | Family Register - سجل الاسر |
| Use case prefix | UC-FAM |
| Chapter in master document | Chapter 10 |
| Documented use cases | 14 |
| Principal routes | `#/families`, `#/families/create`, `#/families/:id`, `#/families/:id/edit`, `#/families/:id/members` *(planned)*, `#/families/provider-requests` *(planned)* |
| Version | 1.1 |
| Status | Chapter content extracted verbatim; screen fields and scenarios derived from the source code |
| Date | 18 August 2026 |
| Parent document | WAR.IIROSA-Project-Documentation.docx v1.0 (repository WAR.IIROSA, commit 4beb736) |

## Contents of this document

1. Chapter 10 — module purpose and use-case catalogue (verbatim from the master document)
2. §10.D — detailed specifications carried over from chapter 25
3. §10.S — screen field specifications (every field of every screen, derived from the AngularJS views)
4. §10.U — one expanded scenario per use case (trigger, pre-conditions, main flow, alternates, exceptions, post-conditions)
5. §10.A / §10.B — annexes: screens and Web API controllers of this module

## 10. Family Register

سجل الأسر — the beneficiary household file is the root record of the whole system. It owns the guardian(s) and the orphans, and everything else (reports, payments, projects) hangs off it.

### 10.1 Family file content


| Section | Captured data |
| --- | --- |
| Identification | Owning charity, country / region / centre, family code, registration date. |
| Guardian (parent) | Name, national ID, relationship to the orphans, marital and health status, job, income, phone numbers, Meza card number. |
| Housing | House type, house status, ownership type, rent, number of rooms, address. |
| Income & needs | Family income sources and amounts, other sponsoring bodies, declared needs. |
| Members | Children (orphans and non-orphans) with birth date, gender, educational level, stage, class, educational status, health status, disability. |
| Attachments | Documents and photographs uploaded through the file service. |

### 10.2 Use cases


| ID | Use case | Primary actor | Description & main flow | Realisation |
| --- | --- | --- | --- | --- |
| UC-FAM-01 | List families of a charity قائمة الأسر | Charity, HQ roles | Displays a paged list of family files owned by the charity. A charity account sees only its own families; HQ roles pass the charity id to browse any charity. | Route `#/families` → GET /api/Families?charityId= |
| UC-FAM-02 | Search families البحث عن أسرة | Charity, HQ roles | Filters the family list by a chosen criterion and value. Supported criteria are father name, mother name, orphan name, sponsor name, national ID, orphan code and phone number. Results remain paged and scoped to the charity. | GET /api/Families?charityId=&search= (SearchCriteria) |
| UC-FAM-03 | Register a new family اضافة اسرة | Charity | The charity opens a new family file: housing and income data, the guardian with their national ID, and the children. On save the system creates the family, its parent record and all child records in one unit of work and returns the new family id. Pre-condition: the charity's add permission must be enabled. Alternate: a duplicate national ID is rejected (UC-SYS-05). | Route `#/families/create` → POST /api/Families |
| UC-FAM-04 | View a family file عرض بيانات الأسرة | Charity, HQ roles | Loads a complete family file — guardian, children, housing, income and attachments — in read mode at `#/families/:id` or in edit mode at `#/families/:id/edit`; this project uses two routes rather than the legacy mode parameter. | Route `#/families/:id` (read) · `#/families/:id/edit` (edit) → GET /api/Families/{id} |
| UC-FAM-05 | Update a family file تعديل بيانات الأسرة | Charity, HQ roles | Saves amendments to the family, its guardian and its members. Pre-condition: the charity's edit permission must be enabled (UC-CHR-09); HQ roles are not subject to this lock. | PUT /api/Families |
| UC-FAM-06 | Transfer a family to another charity نقل الأسرة لجمعية أخرى | HQ roles | Reassigns the family file — and by cascade its guardian, orphans, reports and history — from its current charity to a new one, so that future reporting and payments are handled by the receiving charity. | PUT /api/Families/{id}/charity |
| UC-FAM-07 | Move an orphan between families نقل يتيم بين الأسر | Gen. Director, Staff, Fin. Director | Corrects a mis-registered member. The operator either detaches the orphan into a newly created holding family (action 0, with a justification note appended to the orphan's notes) or attaches the orphan to an existing family (action 1). Pre-condition: caller role must be 0, 3 or 4; other roles are refused. | Route `#/families/:id/members` → POST /api/Families/{familyId}/members/{memberId}/control |
| UC-FAM-08 | Move a guardian between families نقل العائل بين الأسر | Gen. Director, Staff, Fin. Director | The same correction applied to a guardian record (memberType = 2), detaching them to a new family or attaching them to an existing one. | POST /api/Families/{familyId}/members/{memberId}/control |
| UC-FAM-09 | Review guardian-change requests طلبات تعديل العائل | General Director | Lists the pending requests raised by charities to change the recorded guardian of a family — typically after the death, remarriage or departure of the current guardian — with the old and new guardian details for comparison. | Route `#/families/provider-requests` → GET /api/Families/provider-requests |
| UC-FAM-10 | Approve a guardian-change request اعتماد تعديل العائل | General Director | Confirms one pending request; the system applies the guardian change to the family file and marks the request as processed so it leaves the queue. | POST /api/Families/provider-requests/{id}/approve |
| UC-FAM-11 | Track family follow-up activity متابعة إدخالات الأسر | HQ roles | For a chosen date and charity, returns what was entered or updated on the family files, used to monitor whether charities are keeping their register current. | Route `#/reports/family-orphans` → GET /api/Families/{id}/follow-up |
| UC-FAM-12 | Verify a guardian can be added التحقق من إمكانية إضافة عائل | Charity | Before a guardian is attached to a family the system checks that the family does not already have one and that the charity is permitted to add records. | POST /api/Families/{familyId}/verify-provider |
| UC-FAM-13 | Remove a guardian's sponsorship link حذف كفالة العائل | HQ roles | Checks whether the guardian is still referenced by an active sponsorship and, when they are not, removes the sponsorship link so the family can be re-attached to a different guardian. | DELETE /api/Families/{familyId}/provider/sponsor |
| UC-FAM-14 | Print family follow-up and identification sheets طباعة كشوف المتابعة | HQ roles, charity | Produces the follow-up documents for a charity, through `POST /api/Reports/<report-key>/export/pdf`: the family update-tracking sheet for a payment date, and the identification sheets for guardians and for widows (whole charity or a single family). | /api/Reports/family-update-tracking/export/pdf, /api/Reports/guardian-identification-sheets/export/pdf, /api/Reports/widow-identification-sheets/export/pdf, …ForWidows_Family |

### 10.D  Detailed use case specifications (from chapter 25)

Reproduced verbatim from chapter 25 of the master document — the fully expanded specification of this module’s critical end-to-end scenarios. Every other use case of the module is specified in §10.U.

**25.2 UC-FAM-03 — Register a new family**


| Use case ID | UC-FAM-03 |
| --- | --- |
| Name | Register a new family — اضافة اسرة |
| Primary actor | Charity user |
| Goal | Open a beneficiary household file containing the guardian and the children. |
| Pre-conditions | The user is signed in as a charity; the charity's add permission is enabled (UC-CHR-08); the country's validation rules are loaded. |
| Trigger | The user selects Add family from the families menu. |
| Main flow | 1. The client opens the AddFamily state in mode a and loads the reference lists (UC-SYS-04, UC-SYS-05) and the country/region/centre cascade (UC-SYS-07). 2. The user completes the housing, income and needs sections. 3. The user enters the guardian's data including the national ID; on leaving the field the client calls CheckNid (UC-SYS-12). 4. The user adds each child with birth date, gender, educational level/stage/class, educational and health status and any disability; each child's national ID is likewise validated. 5. The user attaches supporting documents and photographs (UC-SYS-01). 6. On save the client posts the family contract to POST /api/Families. 7. IFamilyService creates the family, the guardian and all child records within one unit of work, stamping the owning charity, country, region and centre. 8. The unit of work is committed and the new family id is returned; the client navigates to the family file in view mode. |
| Alternate flows | A1 — Duplicate national ID. The validation call reports a clash and identifies the conflicting record; the user must correct the number or investigate the existing beneficiary before saving. A2 — Add permission disabled. HQ has frozen registration for the charity; the save is refused. A3 — Housing or refugee family. The user instead follows UC-HOU-03 or UC-REF-03, which capture the additional accommodation or displacement data. |
| Exception flow | E1. A persistence failure aborts the whole unit of work — no partial family is created — the error is logged and a failure status is returned. |
| Post-conditions | A family file exists with its guardian and children, owned by the registering charity; the children are eligible to be put forward for coding (UC-ORP-03). |
| Business rules | BR-04 A family belongs to exactly one charity at a time. BR-05 National IDs are unique within a country. BR-06 A family has one guardian of record. |


### 10.S  Screen field specifications

Derived from the AngularJS views of this module. For every screen the table lists each data-entry field in the order it appears, the scope property it binds to, its control type, whether the form marks it mandatory, and the lookup or rule that governs it. Labels are reproduced as they appear on screen (Arabic, right-to-left).

#### 10.S.1  Screen `#/families`


| Property | Value |
| --- | --- |
| Angular route | `#/families` |
| Feature module | `families` (lazy-loaded) |
| Component | `FamilyListComponent` |
| Route status | implemented |
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

#### 10.S.2  Screen `#/families/create` · `#/families/:id/edit`


| Property | Value |
| --- | --- |
| Angular route | `#/families/create` and `#/families/:id/edit` (one component, both routes) |
| Feature module | `families` (lazy-loaded) |
| Component | `FamilyFormComponent` |
| Route status | implemented |
| Data-entry fields | 87 |
| Grids on the screen | 3 |
| Commands | 41 |


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
| بيانات الأسرة | بيانات الإتصال | phone.Number | Text box | Mandatory · read-only when phone.Number && phone.Number !== '' |
| بيانات الأسرة | (unlabelled) | phone.BelongsTo | Drop-down list | Optional · options: الام / الاب / خال / خالة / عم / عمة / أخ / أخت / جد / جدة / قريب / قريبة / غير ذلك / نفسه · read-only when phone.BelongsTo && phone.BelongsTo !== '' |
| بيانات الأسرة | (unlabelled) | phone.IsDefaultPhone | Check box | Optional |
| بيانات الأسرة | قيمة الإيجار | FamilyRentNumber | Numeric box | Mandatory |
| بيانات الأسرة | ملكية السكن | FamilyHouseOwnerhip | Drop-down list | Mandatory · options: lookup: HouseOwnerShips · on change: DisplayRentValue() |
| بيانات الأسرة | حالة محتويات السكن | FamilyHouseStatus | Drop-down list | Mandatory · options: lookup: HouseStatus |
| بيانات الأسرة | نوع السكن | FamilyHouseType | Drop-down list | Mandatory · options: lookup: HouseTypes |
| بيانات الأسرة | نوع الدخل | IncomeTypesDdl | Drop-down list | Mandatory · options: lookup: IncomeTypes |
| بيانات الأسرة | الدخل الكلى | FamilyIncomeNumber | Numeric box | Optional · read-only |
| بيانات الأسرة | نصيب الفرد | FamilyInMem | Numeric box | Optional · read-only |
| بيانات الأسرة | عدد الأبناء | FamilyChildrenNumber | Numeric box | Optional · read-only |
| بيانات الأسرة | ملاحظات الباحث | FamilyNote | Text box | Optional |
| بيانات الأسرة | استبعاد الأسره ? | IsExecluded | Check box | Optional |
| مشروع الأسرة | حاله المشروع | FamilyProjectStatusEnumId | Drop-down list | Mandatory · options: يوجد مشروع قائم / مشروع جديد · on change: OnChangeFamilyProjectStatusEnum() |
| مشروع الأسرة | عنوان المشروع | familyProjectObj.ProjectTitle | Text box | Mandatory |
| مشروع الأسرة | تاريخ بدايه المشروع | familyProjectObj.ProjectStartDate | Date picker | Mandatory · on change: familyProjectObj.ProjectStartDate.date=dt.toISOString() |
| مشروع الأسرة | تفاصيل المشروع | familyProjectObj.ProjectDescription | Text box | Mandatory |
| مشروع الأسرة | هل يوجد خبره | familyProjectObj.ThereIsExperience | Drop-down list | Mandatory · options: يوجد خبره / لا يوجد خبره |
| مشروع الأسرة | راس مال المشروع | familyProjectObj.ProjectPudget | Numeric box | Mandatory |
| مشروع الأسرة | المجال | familyProjectObj.DevelopmentProjectId | Drop-down list | Mandatory · options: lookup: DevelopmentProjects |
| اضافة معيل | الصوره الشخصيه | SponserPic | File upload | Optional · accepts image/* |
| اضافة معيل | صوره الرقم القومي | SponserPic | File upload | Optional · accepts image/* |
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
| اضافة معيل | صوره شهاده الوفاه | DeathPic | File upload | Optional · read-only when DeathPic && DeathPic !== ''; accepts image/* |
| اضافة معيل | تاريخ الميلاد | ParentBDate | Date picker | Mandatory · on change: ParentBDate.date=dt.toISOString() |
| اضافة معيل | الرقم القومى | ParentNId | Text box | Mandatory |
| اضافة معيل | مشروع تنموى مقترح | ParentDevelopmentProjects | Drop-down list | Mandatory · options: lookup: DevelopmentProjects · on change: IsAnotherDevProject() |
| اضافة معيل | نوع العمل | ParentJobs | Drop-down list | Mandatory · options: lookup: Jobs |
| اضافة معيل | المؤهل الدراسى | ParentEducationalStatus | Drop-down list | Mandatory · options: lookup: EducationalStatuss |
| اضافة معيل | المشروع | OtherDevelopmentProject | Text box | Mandatory |
| اضافة معيل | البنك | ParentBank | Drop-down list | Optional · options: lookup: Banks |
| اضافة معيل | الحالة الاجتماعية | ParentsocialStatus | Drop-down list | Mandatory · options: lookup: socialStatuss · on change: MotherIsWidowed() |
| اضافة معيل | الحالة الصحية | ParentHealthStatus | Drop-down list | Mandatory · options: lookup: HealthStatuss |
| اضافة معيل | كفاله ارمله | WidowSponsorship | Check box | Optional |
| اضافة معيل | تاريخ انتهاء الكارت | MezaCardExpiredDate | Date picker | Mandatory |
| اضافة معيل | رقم كارت ميزه | MezaCard | Text box | Optional |
| اضافة معيل | الام متزوجة | AnotherSponsor | Check box | Optional |
| اضافة معيل | تحتضن اليتيم | MotherIsMar | Check box | Optional |
| اضافة معيل | تحتاج كفالة إيتام | IsCaring | Check box | Optional |
| اضافة ابن | سبب الاستبعاد * | childExcludeReson | Drop-down list | Optional · options: lookup: childExcludeResons · read-only |
| اضافة ابن | مستبعد | childExclude | Check box | Optional · on click: IschildExcluded($event); read-only |
| اضافة ابن | تاريخ الميلاد | ChildBDate | Date picker | Mandatory |
| اضافة ابن | الرقم القومى | ChildNId | Text box | Mandatory · max length 14 |
| اضافة ابن | الاسم الاول | ChildFName | Text box | Mandatory |
| اضافة ابن | الصورة | ChildPhoto | File upload | Optional · accepts image/* |
| اضافة ابن | شهاده الميلاد | ChildBirthPic | File upload | Optional · accepts image/* |
| اضافة ابن | القيد الدراسي | ChildEduPic | File upload | Optional · accepts image/* |
| اضافة ابن | الحالة الصحية | ChildHealthStatus | Drop-down list | Mandatory · options: lookup: HealthStatuss |
| اضافة ابن | الحالة الاجتماعية | ChildsocialStatus | Drop-down list | Mandatory · options: lookup: socialStatuss |
| اضافة ابن | النوع | childGender | Drop-down list | Mandatory · options: ذكر / انثى |
| اضافة ابن | الصف الدراسي | ChildEduStage | Drop-down list | Mandatory · options: lookup: EducationStage |
| اضافة ابن | المرحلة الدراسية | ChildEducationallevel | Drop-down list | Mandatory · options: lookup: EducationalLevels · on change: GetEduStages() |
| اضافة ابن | نوعية العمل | ChildProfession | Drop-down list | Mandatory · options: lookup: Professions · on change: WorktypeofChildchanged() |
| اضافة ابن | القسم | ChildDepartmentName | Text box | Mandatory |
| اضافة ابن | الكلية | ChildFacultyName | Text box | Mandatory |
| اضافة ابن | اسم الموسسة التعليمية مدرسة-معهد-جامعة | ChildSchool | Text box | Mandatory |
| اضافة ابن | ملاحظات | ChildNotes | Text box | Optional |
| اضافة ابن | حاصل على مؤهل دراسى | ChildEducationalStatus | Drop-down list | Mandatory · options: lookup: EducationalStatuss |
| اضافة ابن | طلب كفالة يتيم | childIsOr | Check box | Optional · on click: IsOrphanFun($event) |
| اضافة ابن | طلب استبعاد اليتيم | ChildCode | Numeric box | Optional · read-only when !IsAdmin |
| اضافة ابن | سبب طلب الاستبعاد * | RequsetchildExclude | Check box | Optional · on click: IsRequsetchildExclude($event); read-only when RequsetchildExclude ==true && IsAdmin ==false && InEditmode ==true && RequsetchildExcludeReason != null \|\| HideRequestToExcludeOrphan ==true |
| اضافة ابن | (unlabelled) | RequsetchildExcludeReason | Drop-down list | Mandatory · options: lookup: RequsetchildExcludeReasons · on change: RequsetchildExcludeReasonChange(); read-only when RequsetchildExclude ==true && IsAdmin ==false && InEditmode ==true && RequsetchildExcludeReason != null |
| اضافة ابن | طلب كفالة معاق | duplicatedCode | Text box | Optional |
| اضافة ابن | انتهاء الكفالة | FinishSponsorship | Check box | Optional · on click: IsFinishSponsorship($event) |
| اضافة ابن | غير مكفول | NotSponsorship | Check box | Optional · on click: IsNotSponsorship($event) |
| نقل الاسرة | اختر الجمعية | CharityId | Drop-down list | Optional · options: lookup: Charities · on change: getCharityData() |
| تعديل العلاقة | نوعها | parent.RelationId | Drop-down list | Mandatory · options: lookup: Relations |
| تعديل العلاقة | العلاقة | MainRelationInPOPUP | Drop-down list | Mandatory · options: الاب / الام / علاقة أخرى · on change: OtherRelationsOfPOPUP() |

Grids on this screen:

| Grid (row source) | Columns |
| --- | --- |
| phone in FamilyPhones track by $index | الرقم · نوع الهاتف · يخص من · الافتراضي ؟ · حذف |
| reason in RequsetchildExcludeReasons | طلب كفالة يتيم · * كود اليتيم · --------- · طلب استبعاد اليتيم · سبب طلب الاستبعاد * · طلب كفالة معاق · * كود المعاق · --------- · طلب استبعاد المعاق · سبب طلب الاستبعاد * · Savings · طلب كفالة طالب · * كود الطالب · --------- · طلب استبعاد الطالب · سبب طلب الاستبعاد * |
| childOrChec in childOrChecksList track by $index | إسم الدفعة · رقم الشيك /الحوالة · تاريخ الشيك/ الحوالة · المبلغ بالجنية · تم التسليم · إسم المعيل المستلم |

Commands on this screen:

| Command | Handler | Shown when |
| --- | --- | --- |
| تعديل اعضاء الاسرة | EditingInFamilyMembers() | always |
| حفظ | AddFamily() | always |
| اضافة معيل | AddNewParent() | always |
| اضافة ابن | AddNewchild() | CanAddChildren |
| (icon only) | AddphoneNumber() | always |
| (icon only) | AddIncomeType() | always |
| غلق | EmptyParentModel() | always |
| تم | AddParent() | always |
| حفظ كمعيل وكيتيم | AddParentAndChild() | ParentIsAChildFlag |
| غلق | EmptyChildModel() | always |
| تم | AddChild() | always |
| غلق | EmptyCheckOrModel() | always |
| غلق | CloaseMoveFamilyModel() | always |
| نقل | MoveFamily() | always |
| غلق | CloaseDeleteSposnorModel() | always |
| حذف | DeleteSposnor() | always |
| غلق | CloseEditRelPOPUP() | always |
| حفظ | EditRelationOfSponserInPOPUP() | always |
| (icon only) | PrintFamilyEstbanah() | always |
| (icon only) | ChangeCurrentSponsor(this) | parent.CurrentSponser && parent.IsDead != true |
| (icon only) | EditParent($index) | always |
| (icon only) | ViewEditRelationPOPUPFun($index) | always |
| (icon only) | EditChild($index) | always |
| (icon only) | ShowChecksOfChild(child.Id) | child.Code != null |
| (icon only) | RemovePhone($index) | IsAdmin |
| (icon only) | RemoveIncome($index,type.Number) | always |
| (icon only) | RemoveImage() | ChildPhoto!=null |
| (icon only) | RemoveNationalIdPic() | NationalIdPic!=null |
| (icon only) | WriteDeathDate() | always |
| (icon only) | RemoveDeathPic() | DeathPic!=null |
| (icon only) | IschildExcluded($event) | always |
| (icon only) | RemoveChildBirthPic() | ChildBirthPic!=null |
| (icon only) | RemoveChildEduPic() | ChildEduPic!=null |
| (icon only) | IsOrphanFun($event) | always |
| (icon only) | IsRequsetchildExclude($event) | always |
| (icon only) | IsDisabledFun($event) | always |
| (icon only) | IsRequsetDisabledExclude($event) | always |
| (icon only) | IsStudentFun($event) | always |
| (icon only) | IsRequsetstudentExclude($event) | always |
| (icon only) | IsFinishSponsorship($event) | always |
| (icon only) | IsNotSponsorship($event) | always |

#### 10.S.3  Screen `#/families/:id/members`


| Property | Value |
| --- | --- |
| Angular route | `#/families/:id/members` |
| Feature module | `families` (lazy-loaded) |
| Component | `FamilyMembersComponent` |
| Route status | planned |
| Data-entry fields | 3 |
| Grids on the screen | 1 |
| Commands | 8 |


| Section | Field (as labelled) | Bound to | Control | Mandatory · source · rules |
| --- | --- | --- | --- | --- |
| — | الجمعية | CharityId | Drop-down list | Optional · options: lookup: Charities (+ كافة الجهات) · on change: getCharityData() |
| هل انت متاكد من حذف ؟ | التعليق | Comment | Text box | Optional |
| هل انت متاكد من نقل ؟ | كود الاسرة | Familycode | Text box | Optional |

Grids on this screen:

| Grid (row source) | Columns |
| --- | --- |
| parent in ParentsList track by $index | الاسم · الرقم القومى · الصفة · الكود · عمليات |

Commands on this screen:

| Command | Handler | Shown when |
| --- | --- | --- |
| غلق | EmptyDeletingModel() | always |
| تم | SubmitDeleting() | always |
| غلق | EmptyMovingModel() | always |
| تم | SubmitMoving() | always |
| (icon only) | ShowModalOfMovingParent(parent) | always |
| (icon only) | ShowModalOfDeletingParent(parent) | always |
| (icon only) | ShowModalOfMovingChild(child) | always |
| (icon only) | ShowModalOfDeletingChild(child) | always |

#### 10.S.4  Screen `#/families/provider-requests`


| Property | Value |
| --- | --- |
| Angular route | `#/families/provider-requests` |
| Feature module | `families` (lazy-loaded) |
| Component | `ProviderRequestListComponent` |
| Route status | planned |
| Data-entry fields | 1 |
| Grids on the screen | 1 |
| Commands | 3 |


| Section | Field (as labelled) | Bound to | Control | Mandatory · source · rules |
| --- | --- | --- | --- | --- |
| — | الجمعية | CharityId | Drop-down list | Optional · options: lookup: Charities (+ كافة الجهات) · on change: getCharityData() |

Grids on this screen:

| Grid (row source) | Columns |
| --- | --- |
| parent in UpdateParentRequest track by $index | الرقم · الجمعيه · كود اليتيم · اسم اليتيم كامل · اسم الام · اسم المعيل الجديد · صله القرابه · الرقم القومي · تاريخ التعديل · سبب التعديل · موافقه |

Commands on this screen:

| Command | Handler | Shown when |
| --- | --- | --- |
| (icon only) | EnsureUpdate(parent.Id) | always |
| (icon only) | GetNext() | always |
| (icon only) | GetPrev() | always |

### 10.U  Use case scenarios

One expanded scenario for every use case of this module. Pre-conditions, flows and post-conditions are derived from the screen that hosts the function, the Web API action that serves it and the business-layer method that executes it; quoted messages are the literal strings returned by the business layer.

#### 10.U.1  UC-FAM-01 — List families of a charity قائمة الأسر


| Item | Specification |
| --- | --- |
| Use case ID | UC-FAM-01 |
| Name | List families of a charity قائمة الأسر |
| Type | Browse a list |
| Primary actor | Charity, HQ roles |
| Secondary actors | The system (Web API + business layer); the database. |
| Summary | Displays a paged list of family files owned by the charity. A charity account sees only its own families; HQ roles pass the charity id to browse any charity. |
| Trigger | The actor opens the screen at `#/families` from the module menu. |
| Pre-conditions | 1. The actor is authenticated; the JWT access token is valid and the client holds the role, charity and country claims it carries.<br>2. The actor holds one of: charity, HQ roles.<br>3. Caller identity comes from the JWT, never from the request; the application service scopes the query to the charity (and country) that owns the user, unless the role is an HQ role permitted to pass an explicit `charityId` filter.<br>4. The SPA route `#/families` has loaded and its reference-data lookups have been populated. |
| Main flow | 1. The actor navigates to the screen at `#/families`.<br>2. The system loads the filter lookups the screen offers and, for HQ roles, the charity selector.<br>3. The SPA issues `GET /api/Families?charityId=` carrying id, userId, pageNum.<br>4. `CharitiesController` binds the typed request DTO and delegates to the application service.<br>5. `IFamilyService` validates the payload with its FluentValidation validator, applies the business rules and the charity scope, and persists through `IUnitOfWork`.<br>6. The result set is scoped to the caller’s charity and country, ordered and paged.<br>7. The SPA renders the page with the columns listed in the screen specification and the paging control. |
| Alternate flows | • An HQ role (General Director, Financial Director, Staff) may pass an explicit charity id and so read across the charity boundary; a charity user may not and always sees its own data.<br>• No row matches the criteria — the grid renders empty and the paging control reports zero pages. |
| Exception flows | • The session has expired or the role is not permitted — the request is rejected and the SPA routes back to the login state. |
| Post-conditions | • No stored data is changed — the operation is a read. |
| Realisation | Route `#/families` → `FamilyListComponent`<br>`GET /api/Families?charityId=` → `FamiliesController` → `IFamilyService` |

#### 10.U.2  UC-FAM-02 — Search families البحث عن أسرة


| Item | Specification |
| --- | --- |
| Use case ID | UC-FAM-02 |
| Name | Search families البحث عن أسرة |
| Type | Search / filter |
| Primary actor | Charity, HQ roles |
| Secondary actors | The system (Web API + business layer); the database. |
| Summary | Filters the family list by a chosen criterion and value. Supported criteria are father name, mother name, orphan name, sponsor name, national ID, orphan code and phone number. Results remain paged and scoped to the charity. |
| Trigger | The actor selects the function from the module menu or the screen that hosts it. |
| Pre-conditions | 1. The actor is authenticated; the JWT access token is valid and the client holds the role, charity and country claims it carries.<br>2. The actor holds one of: charity, HQ roles.<br>3. Caller identity comes from the JWT, never from the request; the application service scopes the query to the charity (and country) that owns the user, unless the role is an HQ role permitted to pass an explicit `charityId` filter. |
| Main flow | 1. The actor reaches the function from the screen of this module that hosts it.<br>2. The actor enters the search criteria in the filter fields.<br>3. The actor presses the command button of the screen.<br>4. The SPA issues `GET /api/Families?charityId=&search=` carrying id, userId, pageNum, value, type.<br>5. `CharitiesController` binds the typed request DTO and delegates to the application service.<br>6. `IFamilyService` validates the payload with its FluentValidation validator, applies the business rules and the charity scope, and persists through `IUnitOfWork`.<br>7. The matching rows are returned, scoped to the caller’s charity, and rendered in the result grid. |
| Alternate flows | • An HQ role (General Director, Financial Director, Staff) may pass an explicit charity id and so read across the charity boundary; a charity user may not and always sees its own data.<br>• No row matches the criteria — the grid renders empty and the paging control reports zero pages. |
| Exception flows | • The session has expired or the role is not permitted — the request is rejected and the SPA routes back to the login state. |
| Post-conditions | • No stored data is changed — the operation is a read. |
| Realisation | `GET /api/Families?charityId=&search=` → `FamiliesController` → `IFamilyService` |

#### 10.U.3  UC-FAM-03 — Register a new family اضافة اسرة


| Item | Specification |
| --- | --- |
| Use case ID | UC-FAM-03 |
| Name | Register a new family اضافة اسرة |
| Type | Create a record |
| Primary actor | Charity |
| Secondary actors | The system (Web API + business layer); the database. |
| Summary | The charity opens a new family file: housing and income data, the guardian with their national ID, and the children. On save the system creates the family, its parent record and all child records in one unit of work and returns the new family id. Pre-condition: the charity's add permission must be enabled. Alternate: a duplicate national ID is rejected (UC-SYS-05). |
| Trigger | The actor presses «حفظ» on the screen AddFamily. |
| Pre-conditions | 1. The actor is authenticated; the JWT access token is valid and the client holds the role, charity and country claims it carries.<br>2. The actor holds one of: charity.<br>3. The SPA route `#/families/:id/edit` has loaded and its reference-data lookups have been populated. |
| Main flow | 1. The actor navigates to the screen at `#/families/:id/edit`. The screen opens in add mode.<br>2. The system loads the reference data the form needs (regions, centres, banks, types and the other lookup lists bound to the drop-downs).<br>3. The actor completes the input fields. Mandatory fields: القرية / الحي، المركز/ المدينة، المنطقة /المحافظة، العنوان التفصيلى، بيانات الإتصال، قيمة الإيجار، ملكية السكن، حالة محتويات السكن، نوع السكن، نوع الدخل، حاله المشروع، عنوان المشروع، تاريخ بدايه المشروع، تفاصيل المشروع ….<br>4. The actor presses «حفظ» (AddFamily()).<br>5. The SPA issues `POST /api/Families` carrying [FromBody] FamilyContract family.<br>6. `FamiliesController` binds the typed request DTO and delegates to the application service.<br>7. `IFamilyService` validates the payload with its FluentValidation validator, applies the business rules and the charity scope, and persists through `IUnitOfWork`.<br>8. The business layer validates the payload, stamps the owning charity and the creating user, and persists the new record.<br>9. The system returns the outcome and the SPA confirms the save and returns to the list screen. |
| Alternate flows | • The actor abandons the form before saving — nothing is written and the record keeps its previous state. |
| Exception flows | • The session has expired or the role is not permitted — the request is rejected and the SPA routes back to the login state.<br>• The business layer returns «أحد المعيلين مكرر من قبل أكثر من مرة» and the operation is not applied.<br>• A mandatory field is empty or fails its format check — the save is refused and the field is flagged on the form. |
| Post-conditions | • A new record exists, owned by the charity of the creating user, and appears in the list screen of the module. |
| Realisation | Route `#/families/create` → `FamilyFormComponent`<br>`POST /api/Families` → `FamiliesController` → `IFamilyService` |

#### 10.U.4  UC-FAM-04 — View a family file عرض بيانات الأسرة


| Item | Specification |
| --- | --- |
| Use case ID | UC-FAM-04 |
| Name | View a family file عرض بيانات الأسرة |
| Type | Read a record |
| Primary actor | Charity, HQ roles |
| Secondary actors | The system (Web API + business layer); the database. |
| Summary | Loads a complete family file — guardian, children, housing, income and attachments — in read or edit mode depending on the route mode parameter. |
| Trigger | The actor selects the function from the module menu or the screen that hosts it. |
| Pre-conditions | 1. The actor is authenticated; the JWT access token is valid and the client holds the role, charity and country claims it carries.<br>2. The actor holds one of: charity, HQ roles.<br>3. Caller identity comes from the JWT, never from the request; the application service scopes the query to the charity (and country) that owns the user, unless the role is an HQ role permitted to pass an explicit `charityId` filter.<br>4. The target record exists and its identifier is known to the screen. |
| Main flow | 1. The actor reaches the function from the screen of this module that hosts it.<br>2. The actor selects the record to open.<br>3. The SPA issues `GET /api/Families/{id}` carrying userId, id.<br>4. `FamiliesController` binds the typed request DTO and delegates to the application service.<br>5. `IFamilyService` validates the payload with its FluentValidation validator, applies the business rules and the charity scope, and persists through `IUnitOfWork`.<br>6. The system returns the record with its child collections and the SPA binds them to the form fields. |
| Alternate flows | • An HQ role (General Director, Financial Director, Staff) may pass an explicit charity id and so read across the charity boundary; a charity user may not and always sees its own data. |
| Exception flows | • The session has expired or the role is not permitted — the request is rejected and the SPA routes back to the login state. |
| Post-conditions | • No stored data is changed — the operation is a read. |
| Realisation | `GET /api/Families/{id}` → `FamiliesController` → `IFamilyService` |

#### 10.U.5  UC-FAM-05 — Update a family file تعديل بيانات الأسرة


| Item | Specification |
| --- | --- |
| Use case ID | UC-FAM-05 |
| Name | Update a family file تعديل بيانات الأسرة |
| Type | Update a record |
| Primary actor | Charity, HQ roles |
| Secondary actors | The system (Web API + business layer); the database. |
| Summary | Saves amendments to the family, its guardian and its members. Pre-condition: the charity's edit permission must be enabled (UC-CHR-09); HQ roles are not subject to this lock. |
| Trigger | The actor selects the function from the module menu or the screen that hosts it. |
| Pre-conditions | 1. The actor is authenticated; the JWT access token is valid and the client holds the role, charity and country claims it carries.<br>2. The actor holds one of: charity, HQ roles.<br>3. The target record exists and its identifier is known to the screen. |
| Main flow | 1. The actor reaches the function from the screen of this module that hosts it. The screen opens in edit mode on the selected record.<br>2. The system loads the current values into the form.<br>3. The actor changes the fields to be corrected.<br>4. The actor presses the command button of the screen.<br>5. The SPA issues `PUT /api/Families` carrying [FromBody]FamilyContract family.<br>6. `FamiliesController` binds the typed request DTO and delegates to the application service.<br>7. `IFamilyService` validates the payload with its FluentValidation validator, applies the business rules and the charity scope, and persists through `IUnitOfWork`.<br>8. The business layer re-validates the payload, applies the changes and persists them.<br>9. The system returns the outcome and the SPA refreshes the screen. |
| Alternate flows | • The actor abandons the form before saving — nothing is written and the record keeps its previous state. |
| Exception flows | • The session has expired or the role is not permitted — the request is rejected and the SPA routes back to the login state.<br>• The business layer returns «لا يمكن تعديل اسم لمعيل صرف شيك مسبقا» and the operation is not applied.<br>• The business layer returns «تم اضافه الطلب من قبل . انتظر موافه مسئول المكتب» and the operation is not applied.<br>• A mandatory field is empty or fails its format check — the save is refused and the field is flagged on the form. |
| Post-conditions | • The stored record carries the new values; no other record is affected. |
| Realisation | `PUT /api/Families` → `FamiliesController` → `IFamilyService` |

#### 10.U.6  UC-FAM-06 — Transfer a family to another charity نقل الأسرة لجمعية أخرى


| Item | Specification |
| --- | --- |
| Use case ID | UC-FAM-06 |
| Name | Transfer a family to another charity نقل الأسرة لجمعية أخرى |
| Type | Transfer of ownership |
| Primary actor | HQ roles |
| Secondary actors | The system (Web API + business layer); the database. |
| Summary | Reassigns the family file — and by cascade its guardian, orphans, reports and history — from its current charity to a new one, so that future reporting and payments are handled by the receiving charity. |
| Trigger | The actor selects the function from the module menu or the screen that hosts it. |
| Pre-conditions | 1. The actor is authenticated; the JWT access token is valid and the client holds the role, charity and country claims it carries.<br>2. The actor holds one of: HQ roles.<br>3. Caller identity comes from the JWT, never from the request; the application service scopes the query to the charity (and country) that owns the user, unless the role is an HQ role permitted to pass an explicit `charityId` filter. |
| Main flow | 1. The actor reaches the function from the screen of this module that hosts it.<br>2. The actor selects the file to be moved and the receiving charity.<br>3. The actor confirms the transfer.<br>4. The SPA issues `PUT /api/Families/{id}/charity` carrying userId, familyId, newCharityId.<br>5. `FamiliesController` binds the typed request DTO and delegates to the application service.<br>6. `IFamilyService` validates the payload with its FluentValidation validator, applies the business rules and the charity scope, and persists through `IUnitOfWork`.<br>7. The business layer re-points the ownership of the file and its dependent records to the receiving charity and records the movement. |
| Alternate flows | • An HQ role (General Director, Financial Director, Staff) may pass an explicit charity id and so read across the charity boundary; a charity user may not and always sees its own data. |
| Exception flows | • The session has expired or the role is not permitted — the request is rejected and the SPA routes back to the login state.<br>• The business layer returns «Faild Operation» and the operation is not applied. |
| Post-conditions | • The file and its dependent records belong to the receiving charity. |
| Realisation | `PUT /api/Families/{id}/charity` → `FamiliesController` → `IFamilyService` |

#### 10.U.7  UC-FAM-07 — Move an orphan between families نقل يتيم بين الأسر


| Item | Specification |
| --- | --- |
| Use case ID | UC-FAM-07 |
| Name | Move an orphan between families نقل يتيم بين الأسر |
| Type | Transfer of ownership |
| Primary actor | Gen. Director, Staff, Fin. Director |
| Secondary actors | The system (Web API + business layer); the database. |
| Summary | Corrects a mis-registered member. The operator either detaches the orphan into a newly created holding family (action 0, with a justification note appended to the orphan's notes) or attaches the orphan to an existing family (action 1). Pre-condition: caller role must be 0, 3 or 4; other roles are refused. |
| Trigger | The actor presses «غلق» on the screen EditingInFamilyMembers. |
| Pre-conditions | 1. The actor is authenticated; the JWT access token is valid and the client holds the role, charity and country claims it carries.<br>2. The actor holds one of: Gen. Director, Staff, Fin. Director.<br>3. Caller identity comes from the JWT, never from the request; the application service scopes the query to the charity (and country) that owns the user, unless the role is an HQ role permitted to pass an explicit `charityId` filter.<br>4. The SPA route `#/families/:id/members` has loaded and its reference-data lookups have been populated. |
| Main flow | 1. The actor navigates to the screen at `#/families/:id/members`.<br>2. The actor selects the file to be moved and the receiving charity.<br>3. The actor confirms the transfer.<br>4. The SPA issues `POST /api/Families/{familyId}/members/{memberId}/control` carrying id, userId, memberType=1, action, value.<br>5. `FamiliesController` binds the typed request DTO and delegates to the application service.<br>6. `IFamilyService` validates the payload with its FluentValidation validator, applies the business rules and the charity scope, and persists through `IUnitOfWork`.<br>7. The business layer re-points the ownership of the file and its dependent records to the receiving charity and records the movement. |
| Alternate flows | • An HQ role (General Director, Financial Director, Staff) may pass an explicit charity id and so read across the charity boundary; a charity user may not and always sees its own data. |
| Exception flows | • The session has expired or the role is not permitted — the request is rejected and the SPA routes back to the login state. |
| Post-conditions | • The file and its dependent records belong to the receiving charity. |
| Realisation | Route `#/families/:id/members` → `FamilyMembersComponent`<br>`POST /api/Families/{familyId}/members/{memberId}/control` → `FamiliesController` → `IFamilyService` |

#### 10.U.8  UC-FAM-08 — Move a guardian between families نقل العائل بين الأسر


| Item | Specification |
| --- | --- |
| Use case ID | UC-FAM-08 |
| Name | Move a guardian between families نقل العائل بين الأسر |
| Type | Create a record |
| Primary actor | Gen. Director, Staff, Fin. Director |
| Secondary actors | The system (Web API + business layer); the database. |
| Summary | The same correction applied to a guardian record (memberType = 2), detaching them to a new family or attaching them to an existing one. |
| Trigger | The actor selects the function from the module menu or the screen that hosts it. |
| Pre-conditions | 1. The actor is authenticated; the JWT access token is valid and the client holds the role, charity and country claims it carries.<br>2. The actor holds one of: Gen. Director, Staff, Fin. Director. |
| Main flow | 1. The actor reaches the function from the screen of this module that hosts it. The screen opens in add mode.<br>2. The system loads the reference data the form needs (regions, centres, banks, types and the other lookup lists bound to the drop-downs).<br>3. The actor completes the input fields of the form.<br>4. The actor presses the command button of the screen.<br>5. The SPA issues `POST /api/Families/{familyId}/members/{memberId}/control` carrying memberType=2.<br>6. `FamiliesController` binds the typed request DTO and delegates to the application service.<br>7. `IFamilyService` validates the payload with its FluentValidation validator, applies the business rules and the charity scope, and persists through `IUnitOfWork`.<br>8. The business layer validates the payload, stamps the owning charity and the creating user, and persists the new record.<br>9. The system returns the outcome and the SPA confirms the save and returns to the list screen. |
| Alternate flows | • The actor abandons the form before saving — nothing is written and the record keeps its previous state. |
| Exception flows | • The session has expired or the role is not permitted — the request is rejected and the SPA routes back to the login state.<br>• A mandatory field is empty or fails its format check — the save is refused and the field is flagged on the form. |
| Post-conditions | • A new record exists, owned by the charity of the creating user, and appears in the list screen of the module. |
| Realisation | `POST /api/Families/{familyId}/members/{memberId}/control` → `FamiliesController` → `IFamilyService` |

#### 10.U.9  UC-FAM-09 — Review guardian-change requests طلبات تعديل العائل


| Item | Specification |
| --- | --- |
| Use case ID | UC-FAM-09 |
| Name | Review guardian-change requests طلبات تعديل العائل |
| Type | Update a record |
| Primary actor | General Director |
| Secondary actors | The system (Web API + business layer); the database. |
| Summary | Lists the pending requests raised by charities to change the recorded guardian of a family — typically after the death, remarriage or departure of the current guardian — with the old and new guardian details for comparison. |
| Trigger | The actor presses «EnsureUpdate» on the screen UpdateParentRequest. |
| Pre-conditions | 1. The actor is authenticated; the JWT access token is valid and the client holds the role, charity and country claims it carries.<br>2. The actor holds one of: General Director.<br>3. Caller identity comes from the JWT, never from the request; the application service scopes the query to the charity (and country) that owns the user, unless the role is an HQ role permitted to pass an explicit `charityId` filter.<br>4. The SPA route `#/families/provider-requests` has loaded and its reference-data lookups have been populated.<br>5. The target record exists and its identifier is known to the screen. |
| Main flow | 1. The actor navigates to the screen at `#/families/provider-requests`. The screen opens in edit mode on the selected record.<br>2. The system loads the current values into the form.<br>3. The actor changes the fields to be corrected.<br>4. The actor presses «EnsureUpdate» (EnsureUpdate(parent.Id)).<br>5. The SPA issues `GET /api/Families/provider-requests` carrying string userId, string familyId, string newCharityId.<br>6. `FamiliesController` binds the typed request DTO and delegates to the application service.<br>7. `IFamilyService` validates the payload with its FluentValidation validator, applies the business rules and the charity scope, and persists through `IUnitOfWork`.<br>8. The business layer re-validates the payload, applies the changes and persists them.<br>9. The system returns the outcome and the SPA refreshes the screen. |
| Alternate flows | • An HQ role (General Director, Financial Director, Staff) may pass an explicit charity id and so read across the charity boundary; a charity user may not and always sees its own data.<br>• The actor abandons the form before saving — nothing is written and the record keeps its previous state. |
| Exception flows | • The session has expired or the role is not permitted — the request is rejected and the SPA routes back to the login state.<br>• The business layer returns «Faild Operation» and the operation is not applied.<br>• A mandatory field is empty or fails its format check — the save is refused and the field is flagged on the form. |
| Post-conditions | • The stored record carries the new values; no other record is affected. |
| Realisation | Route `#/families/provider-requests` → `ProviderRequestListComponent`<br>`GET /api/Families/provider-requests` → `FamiliesController` → `IFamilyService` |

#### 10.U.10  UC-FAM-10 — Approve a guardian-change request اعتماد تعديل العائل


| Item | Specification |
| --- | --- |
| Use case ID | UC-FAM-10 |
| Name | Approve a guardian-change request اعتماد تعديل العائل |
| Type | Review decision |
| Primary actor | General Director |
| Secondary actors | The system (Web API + business layer); the database. |
| Summary | Confirms one pending request; the system applies the guardian change to the family file and marks the request as processed so it leaves the queue. |
| Trigger | The actor selects the function from the module menu or the screen that hosts it. |
| Pre-conditions | 1. The actor is authenticated; the JWT access token is valid and the client holds the role, charity and country claims it carries.<br>2. The actor holds one of: General Director.<br>3. The target record exists and its identifier is known to the screen. |
| Main flow | 1. The actor reaches the function from the screen of this module that hosts it.<br>2. The actor opens the item awaiting a decision and examines its content and attachments.<br>3. The actor records the decision and, when refusing, the reason.<br>4. The actor presses the command button of the screen.<br>5. The SPA issues `POST /api/Families/provider-requests/{id}/approve` carrying Id.<br>6. `FamiliesController` binds the typed request DTO and delegates to the application service.<br>7. `IFamilyService` validates the payload with its FluentValidation validator, applies the business rules and the charity scope, and persists through `IUnitOfWork`.<br>8. The business layer writes the new state, the deciding user and the decision date.<br>9. The item leaves the pending queue and becomes visible to the charity in its new state. |
| Alternate flows | • The decision is a refusal — the reason is mandatory and is stored with the item so that the charity can see why it was returned. |
| Exception flows | • The session has expired or the role is not permitted — the request is rejected and the SPA routes back to the login state.<br>• The business layer returns «Faild Operation» and the operation is not applied. |
| Post-conditions | • The item carries its new state, the deciding user and the decision date, and moves out of the pending queue. |
| Realisation | `POST /api/Families/provider-requests/{id}/approve` → `FamiliesController` → `IFamilyService` |

#### 10.U.11  UC-FAM-11 — Track family follow-up activity متابعة إدخالات الأسر


| Item | Specification |
| --- | --- |
| Use case ID | UC-FAM-11 |
| Name | Track family follow-up activity متابعة إدخالات الأسر |
| Type | Query a report |
| Primary actor | HQ roles |
| Secondary actors | The system (Web API + business layer); the database. |
| Summary | For a chosen date and charity, returns what was entered or updated on the family files, used to monitor whether charities are keeping their register current. |
| Trigger | The actor opens the screen at `#/reports/family-orphans` from the module menu. |
| Pre-conditions | 1. The actor is authenticated; the JWT access token is valid and the client holds the role, charity and country claims it carries.<br>2. The actor holds one of: HQ roles.<br>3. Caller identity comes from the JWT, never from the request; the application service scopes the query to the charity (and country) that owns the user, unless the role is an HQ role permitted to pass an explicit `charityId` filter.<br>4. The SPA route `#/reports/family-orphans` has loaded and its reference-data lookups have been populated. |
| Main flow | 1. The actor navigates to the screen at `#/reports/family-orphans`.<br>2. The actor sets the report criteria (period, charity, country and the other filters the screen offers).<br>3. The actor runs the report.<br>4. The SPA issues `GET /api/Families/{id}/follow-up` carrying date, charityId.<br>5. `FamiliesController` binds the typed request DTO and delegates to the application service.<br>6. `IReportService` validates the payload with its FluentValidation validator, applies the business rules and the charity scope, and persists through `IUnitOfWork`.<br>7. The business layer composes the projection, scoped to the caller’s charity and country.<br>8. The result is rendered in the on-screen grid, from where it can be printed or exported. |
| Alternate flows | • An HQ role (General Director, Financial Director, Staff) may pass an explicit charity id and so read across the charity boundary; a charity user may not and always sees its own data.<br>• No row matches the criteria — the grid renders empty and the paging control reports zero pages. |
| Exception flows | • The session has expired or the role is not permitted — the request is rejected and the SPA routes back to the login state. |
| Post-conditions | • No stored data is changed — the operation is a read. |
| Realisation | Route `#/reports/family-orphans` → `ReportViewerComponent`<br>`GET /api/Families/{id}/follow-up` → `FamiliesController` → `IFamilyService` |

#### 10.U.12  UC-FAM-12 — Verify a guardian can be added التحقق من إمكانية إضافة عائل


| Item | Specification |
| --- | --- |
| Use case ID | UC-FAM-12 |
| Name | Verify a guardian can be added التحقق من إمكانية إضافة عائل |
| Type | Create a record |
| Primary actor | Charity |
| Secondary actors | The system (Web API + business layer); the database. |
| Summary | Before a guardian is attached to a family the system checks that the family does not already have one and that the charity is permitted to add records. |
| Trigger | The actor selects the function from the module menu or the screen that hosts it. |
| Pre-conditions | 1. The actor is authenticated; the JWT access token is valid and the client holds the role, charity and country claims it carries.<br>2. The actor holds one of: charity.<br>3. Caller identity comes from the JWT, never from the request; the application service scopes the query to the charity (and country) that owns the user, unless the role is an HQ role permitted to pass an explicit `charityId` filter. |
| Main flow | 1. The actor reaches the function from the screen of this module that hosts it. The screen opens in add mode.<br>2. The system loads the reference data the form needs (regions, centres, banks, types and the other lookup lists bound to the drop-downs).<br>3. The actor completes the input fields of the form.<br>4. The actor presses the command button of the screen.<br>5. The SPA issues `POST /api/Families/{familyId}/verify-provider` carrying id, userId.<br>6. `FamiliesController` binds the typed request DTO and delegates to the application service.<br>7. `IFamilyService` validates the payload with its FluentValidation validator, applies the business rules and the charity scope, and persists through `IUnitOfWork`.<br>8. The business layer validates the payload, stamps the owning charity and the creating user, and persists the new record.<br>9. The system returns the outcome and the SPA confirms the save and returns to the list screen. |
| Alternate flows | • An HQ role (General Director, Financial Director, Staff) may pass an explicit charity id and so read across the charity boundary; a charity user may not and always sees its own data.<br>• The actor abandons the form before saving — nothing is written and the record keeps its previous state. |
| Exception flows | • The session has expired or the role is not permitted — the request is rejected and the SPA routes back to the login state.<br>• A mandatory field is empty or fails its format check — the save is refused and the field is flagged on the form. |
| Post-conditions | • A new record exists, owned by the charity of the creating user, and appears in the list screen of the module. |
| Realisation | `POST /api/Families/{familyId}/verify-provider` → `FamiliesController` → `IFamilyService` |

#### 10.U.13  UC-FAM-13 — Remove a guardian's sponsorship link حذف كفالة العائل


| Item | Specification |
| --- | --- |
| Use case ID | UC-FAM-13 |
| Name | Remove a guardian's sponsorship link حذف كفالة العائل |
| Type | Delete a record |
| Primary actor | HQ roles |
| Secondary actors | The system (Web API + business layer); the database. |
| Summary | Checks whether the guardian is still referenced by an active sponsorship and, when they are not, removes the sponsorship link so the family can be re-attached to a different guardian. |
| Trigger | The actor selects the function from the module menu or the screen that hosts it. |
| Pre-conditions | 1. The actor is authenticated; the JWT access token is valid and the client holds the role, charity and country claims it carries.<br>2. The actor holds one of: HQ roles.<br>3. Caller identity comes from the JWT, never from the request; the application service scopes the query to the charity (and country) that owns the user, unless the role is an HQ role permitted to pass an explicit `charityId` filter.<br>4. The target record exists and its identifier is known to the screen. |
| Main flow | 1. The actor reaches the function from the screen of this module that hosts it.<br>2. The actor selects the record and requests its deletion.<br>3. The SPA asks the actor to confirm.<br>4. The SPA issues `DELETE /api/Families/{familyId}/provider/sponsor` carrying id, userId.<br>5. `FamiliesController` binds the typed request DTO and delegates to the application service.<br>6. `IFamilyService` validates the payload with its FluentValidation validator, applies the business rules and the charity scope, and persists through `IUnitOfWork`.<br>7. The business layer checks that the record may still be removed and deletes it (or marks it removed).<br>8. The system returns the outcome and the SPA drops the row from the grid. |
| Alternate flows | • An HQ role (General Director, Financial Director, Staff) may pass an explicit charity id and so read across the charity boundary; a charity user may not and always sees its own data. |
| Exception flows | • The session has expired or the role is not permitted — the request is rejected and the SPA routes back to the login state. |
| Post-conditions | • The record is no longer returned by the list and read endpoints of the module. |
| Realisation | `DELETE /api/Families/{familyId}/provider/sponsor` → `FamiliesController` → `IFamilyService` |

#### 10.U.14  UC-FAM-14 — Print family follow-up and identification sheets طباعة كشوف المتابعة


| Item | Specification |
| --- | --- |
| Use case ID | UC-FAM-14 |
| Name | Print family follow-up and identification sheets طباعة كشوف المتابعة |
| Type | Print / produce a document |
| Primary actor | HQ roles, charity |
| Secondary actors | The system (Web API + business layer); the database. the report and export endpoints of `api/Reports` as the rendering path — jsPDF for PDF and ExcelJS for Excel. |
| Summary | Produces the follow-up documents for a charity, through `POST /api/Reports/<report-key>/export/pdf`: the family update-tracking sheet for a payment date, and the identification sheets for guardians and for widows (whole charity or a single family). |
| Trigger | The actor selects the function from the module menu or the screen that hosts it. |
| Pre-conditions | 1. The actor is authenticated; the JWT access token is valid and the client holds the role, charity and country claims it carries.<br>2. The actor holds one of: HQ roles, charity.<br>3. The target record exists and its identifier is known to the screen. |
| Main flow | 1. The actor reaches the function from the screen of this module that hosts it.<br>2. The actor selects the record or the range to be printed.<br>3. The actor presses the command button of the screen.<br>4. The browser opens `POST /api/Reports/family-update-tracking/export/pdf` with string paymentId, string date.<br>5. `ReportsController` resolves the report key, applies the filter, and renders the document from the dataset the application service returns.<br>6. The rendered document is streamed back as a PDF for viewing and printing. |
| Alternate flows | None recorded. |
| Exception flows | • The session has expired or the role is not permitted — the request is rejected and the SPA routes back to the login state.<br>• The report definition or a required parameter is missing — the print action fails and no document is produced. |
| Post-conditions | • A printable document has been produced. Where the module records printing, the row is flagged as printed. |
| Realisation | `POST /api/Reports/family-update-tracking/export/pdf` · `POST /api/Reports/guardian-identification-sheets/export/pdf` · `POST /api/Reports/widow-identification-sheets/export/pdf` → `ReportsController` → `IReportService` |

### 10.A  Annex - Angular routes of this module

Routes are hash-based (`useHash: true`), rendered inside `MainLayoutComponent` behind
`AuthGuard` and `PermissionGuard`. *implemented* means the route exists in
`Frontend/src/app`; *planned* means it is specified here and not yet built.

| Angular route | Feature module | Component | Status |
| --- | --- | --- | --- |
| `#/families` | `families` | `FamilyListComponent` | implemented |
| `#/families/create` | `families` | `FamilyFormComponent` | implemented |
| `#/families/:id` | `families` | `FamilyDetailComponent` | implemented |
| `#/families/:id/edit` | `families` | `FamilyFormComponent` | implemented |
| `#/families/:id/members` | `families` | `FamilyMembersComponent` | planned |
| `#/families/provider-requests` | `families` | `ProviderRequestListComponent` | planned |

### 10.B  Annex - API controllers of this module

Every controller inherits `ApiController` (`[Authorize]`, route `api/[controller]`) and
returns `Framework.Core.ApiResponse<T>`. Business rules live in the application service
behind each controller, never in the controller itself.

| Controller | Base route | Responsibility |
| --- | --- | --- |
| `CharitiesController` | `api/Charities` | Charity creation and listing; family listing and search per charity. |
| `FamiliesController` | `api/Families` | Family / housing / refugee creation, update, transfer, follow-up, guardian-change requests, report group counts. Family read by type; project receipt flag. Guardian add check and sponsorship removal. |

---

Shared context - system architecture, actors and roles, the sponsorship lifecycle, the full module map and all appendices - is in the companion document [00-Overview-and-Common-Context](00-Overview-and-Common-Context.md). The delivery backlog for this module is epic EP-05 in [00-EPICS-and-User-Stories](00-EPICS-and-User-Stories.md).

