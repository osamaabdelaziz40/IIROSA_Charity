# WAR.IIROSA - Housing Project

مشروع الاسكان | use case prefix `UC-HOU` | chapter 11 of the master document


| Field | Value |
| --- | --- |
| Document title | WAR.IIROSA - Module Specification: Housing Project |
| Module | Housing Project - مشروع الاسكان |
| Use case prefix | UC-HOU |
| Chapter in master document | Chapter 11 |
| Documented use cases | 8 |
| Principal routes | `#/housing-projects`, `#/housing-projects/:id/edit`, `#/housing-projects/:id/reports`, `#/housing-projects/:id/reports/:reportId` |
| Version | 1.1 |
| Status | Chapter content extracted verbatim; screen fields and scenarios derived from the source code |
| Date | 18 August 2026 |
| Parent document | WAR.IIROSA-Project-Documentation.docx v1.0 (repository WAR.IIROSA, commit 4beb736) |

## Contents of this document

1. Chapter 11 — module purpose and use-case catalogue (verbatim from the master document)
2. §11.S — screen field specifications (every field of every screen, derived from the AngularJS views)
3. §11.U — one expanded scenario per use case (trigger, pre-conditions, main flow, alternates, exceptions, post-conditions)
4. §11.A / §11.B — annexes: screens and Web API controllers of this module

## 11. Housing Project

مشروع الإسكان — a parallel register for families accommodated in organisation-owned buildings. It mirrors the family and periodic-report structure but adds building/flat allocation, and its orphan reports may cover either a child or the guardian.


| ID | Use case | Primary actor | Description & main flow | Realisation |
| --- | --- | --- | --- | --- |
| UC-HOU-01 | List housing families قائمة الأسر الساكنة | Charity, HQ roles | Paged list of the families enrolled in the housing project for the charity. | Route `#/housing-projects` → GET /api/Families?familyType=Housing&charityId= |
| UC-HOU-02 | Search housing families البحث في الأسر الساكنة | Charity, HQ roles | Same criteria set as the main register (guardian name, orphan name, national ID, code, phone) applied to the housing population. | GET /api/Families?familyType=Housing&search=?…&type |
| UC-HOU-03 | Register a housing family اضافة أسرة ساكنة | Charity | Creates the housing family file including the building and flat to which the family is allocated, along with guardian and children data. | Route `#/housing-projects/:id/edit` → POST /api/HousingProjects/projects |
| UC-HOU-04 | View / update a housing family بيانات الأسرة الساكنة | Charity, HQ roles | Loads the housing family file for review or amendment, including its current accommodation allocation. | GET /api/HousingProjects/projects/{id} |
| UC-HOU-05 | Select building and flat اختيار المبنى والشقة | Charity | The form loads the list of housing buildings and, once a building is chosen, the flats belonging to it, so the family can be assigned an accommodation unit. | GET /api/LookupManagement/housing-buildings, GET /api/LookupManagement/housing-flats |
| UC-HOU-06 | List periodic reports of a housing beneficiary التقارير الدورية للأسر الساكنة | Charity, HQ roles | Shows the report history for a housing beneficiary, who may be either a child or the guardian — the ChildOrParent discriminator selects which. | Route `#/housing-projects/:id/reports` → GET /api/PeriodicOrphanReports/by-orphan/{orphanId} |
| UC-HOU-07 | Look up a housing beneficiary by code البحث بالكود | Charity | Before a housing report is created the operator enters the beneficiary code; the system returns the matching child or guardian with their identifying data. | GET /api/PeriodicOrphanReports/by-orphan/{orphanId}; GET /api/HousingProjects/projects/{id}/beneficiaries; GET /api/HousingProjects/projects/{id}/beneficiaries |
| UC-HOU-08 | Create a housing periodic report تقرير دوري لأسرة ساكنة | Charity | Records the recurring status report for a housing beneficiary — health, education, behaviour, prayer, memorisation and accommodation condition — with attachments, and stores it against the housing beneficiary. | Route `#/housing-projects/:id/reports/:reportId` → POST /api/PeriodicOrphanReports |

### 11.S  Screen field specifications

Derived from the AngularJS views of this module. For every screen the table lists each data-entry field in the order it appears, the scope property it binds to, its control type, whether the form marks it mandatory, and the lookup or rule that governs it. Labels are reproduced as they appear on screen (Arabic, right-to-left).

#### 11.S.1  Screen `#/housing-projects`


| Property | Value |
| --- | --- |
| Angular route | `#/housing-projects` |
| Feature module | `housing-projects` (lazy-loaded) |
| Component | `HousingProjectListComponent` |
| Route status | implemented |
| Data-entry fields | 3 |
| Grids on the screen | 1 |
| Commands | 5 |


| Section | Field (as labelled) | Bound to | Control | Mandatory · source · rules |
| --- | --- | --- | --- | --- |
| — | الجمعية | CharityId | Drop-down list | Optional · options: lookup: Charities (+ كافة الجهات) · on change: getCharityData() |
| قائمة الاسر | (unlabelled) | searchValue | Text box | Optional |
| قائمة الاسر | البحث عن طريق | searchType | Drop-down list | Optional · options: إسم الاب / إسم الام / إسم الطالب الابن / الرقم القومي / كود الطالب الابن / الهاتف |

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

#### 11.S.2  Screen `#/housing-projects/:id/edit`


| Property | Value |
| --- | --- |
| Angular route | `#/housing-projects/:id/edit` |
| Feature module | `housing-projects` (lazy-loaded) |
| Component | `HousingProjectFormComponent` |
| Route status | implemented |
| Data-entry fields | 63 |
| Grids on the screen | 2 |
| Commands | 30 |


| Section | Field (as labelled) | Bound to | Control | Mandatory · source · rules |
| --- | --- | --- | --- | --- |
| — | الجمعية | CharityId | Drop-down list | Optional · options: lookup: Charities (+ كافة الجهات) · on change: getCharityData() |
| بيانات الأسرة | إسم الأسرة | FamilyName | Text box | Optional · read-only |
| بيانات الأسرة | القرية / الحي | FamilyVillage | Text box | Mandatory |
| بيانات الأسرة | المركز/ المدينة | FamilyCenters | Drop-down list | Mandatory · options: lookup: Centers |
| بيانات الأسرة | المنطقة /المحافظة | FamilyRegions | Drop-down list | Mandatory · options: القاهرة · on change: Getcenters() |
| بيانات الأسرة | رقم الشقه | HousingFlatId | Drop-down list | Mandatory · options: lookup: HousingFlats |
| بيانات الأسرة | رقم العماره | HousingBuildingsId | Drop-down list | Mandatory · options: lookup: HousingBuildings · on change: GetHousingFlats() |
| بيانات الأسرة | بجوار | FamilyNearBy | Text box | Optional |
| بيانات الأسرة | الشارع | FamilyStreet | Text box | Optional |
| بيانات الأسرة | العنوان التفصيلى | FamilyAddress | Text box | Mandatory |
| بيانات الأسرة | بيانات الإتصال | phone.Number | Text box | Mandatory |
| بيانات الأسرة | (unlabelled) | phone.BelongsTo | Drop-down list | Optional · options: الام / الاب / خال / خالة / عم / عمة / أخ / أخت / جد / جدة / قريب / قريبة / غير ذلك / نفسه |
| بيانات الأسرة | (unlabelled) | phone.IsDefaultPhone | Check box | Optional |
| بيانات الأسرة | قيمة الإيجار | FamilyRentNumber | Numeric box | Mandatory |
| بيانات الأسرة | نوع الدخل | IncomeTypesDdl | Drop-down list | Mandatory · options: lookup: IncomeTypes |
| بيانات الأسرة | الدخل الكلى | FamilyIncomeNumber | Numeric box | Optional · read-only |
| بيانات الأسرة | نصيب الفرد | FamilyInMem | Numeric box | Optional · read-only |
| بيانات الأسرة | عدد الأبناء | FamilyChildrenNumber | Numeric box | Optional · read-only |
| بيانات الأسرة | ملاحظات الباحث | FamilyNote | Text box | Optional |
| بيانات الأسرة | استبعاد الأسره ? | IsExecluded | Check box | Optional |
| اضافة معيل | الصوره الشخصيه | SponserPic | File upload | Optional · accepts image/* |
| اضافة معيل | صوره الرقم القومي | SponserPic | File upload | Optional · accepts image/* |
| اضافة معيل | السبب | OtherParentRelationReasonID | Drop-down list | Mandatory · options: lookup: ReasonsOFRels |
| اضافة معيل | نوعها | OtherParentRelationModel | Drop-down list | Mandatory · options: lookup: Relations · on change: OnChangeRelation() |
| اضافة معيل | العلاقة | ParentRelation | Drop-down list | Mandatory · options: الاب / الام · on change: OtherRelations() |
| اضافة معيل | الاسم الثالث | ParentThirdname | Text box | Mandatory |
| اضافة معيل | الاسم الثانى | Parentsecondname | Text box | Mandatory |
| اضافة معيل | الاسم الاول | ParentFirname | Text box | Mandatory |
| اضافة معيل | الاسم الرباعي | ParentFourthname | Text box | Mandatory |
| اضافة معيل | الجنسية | ParentCountries | Drop-down list | Mandatory · options: lookup: Countries · on change: GetBanks(this) |
| اضافة معيل | إسم الأسرة | ParentFamilyname | Text box | Optional |
| اضافة معيل | تاريخ الميلاد | ParentBDate | Date picker | Mandatory · on change: ParentBDate.date=dt.toISOString() |
| اضافة معيل | رقم جواز السفر | ParentNId | Text box | Mandatory |
| اضافة معيل | نوع العمل | ParentJobs | Drop-down list | Mandatory · options: طالب |
| اضافة معيل | المؤهل الدراسى | ParentEducationalStatus | Drop-down list | Mandatory · options: lookup: EducationalStatuss |
| اضافة معيل | الحالة الاجتماعية | ParentsocialStatus | Drop-down list | Mandatory · options: lookup: socialStatuss · on change: MotherIsWidowed() |
| اضافة معيل | الحالة الصحية | ParentHealthStatus | Drop-down list | Mandatory · options: lookup: HealthStatuss |
| اضافة معيل | كفاله ارمله | WidowSponsorship | Check box | Optional · read-only when AllowWidowSponsorship==false |
| اضافة معيل | الام متزوجة | AnotherSponsor | Check box | Optional |
| اضافة معيل | تحتضن اليتيم | MotherIsMar | Check box | Optional |
| اضافة معيل | تحتاج كفالة إيتام | IsCaring | Check box | Optional |
| اضافة ابن | سبب الاستبعاد * | childExcludeReson | Drop-down list | Optional · options: lookup: childExcludeResons · read-only |
| اضافة ابن | مستبعد | childExclude | Check box | Optional · on click: IschildExcluded($event); read-only |
| اضافة ابن | تاريخ الميلاد | ChildBDate | Date picker | Mandatory |
| اضافة ابن | الرقم القومى | ChildNId | Text box | Mandatory · max length 14 |
| اضافة ابن | الاسم الاول | ChildFName | Text box | Mandatory · read-only when CanEditInChildName==false |
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
| نقل الاسرة | اختر الجمعية | CharityId | Drop-down list | Optional · options: lookup: Charities · on change: getCharityData() |
| تعديل العلاقة | نوعها | parent.RelationId | Drop-down list | Mandatory · options: lookup: Relations |
| تعديل العلاقة | العلاقة | MainRelationInPOPUP | Drop-down list | Mandatory · options: الاب / الام · on change: OtherRelationsOfPOPUP() |

Grids on this screen:

| Grid (row source) | Columns |
| --- | --- |
| phone in FamilyPhones track by $index | الرقم · نوع الهاتف · يخص من · الافتراضي ؟ · حذف |
| childOrChec in childOrChecksList track by $index | إسم الدفعة · رقم الشيك /الحوالة · تاريخ الشيك/ الحوالة · المبلغ بالجنية · تم التسليم · إسم المعيل المستلم |

Commands on this screen:

| Command | Handler | Shown when |
| --- | --- | --- |
| تعديل اعضاء الاسرة | EditingInFamilyMembers() | always |
| حفظ | AddFamily() | always |
| اضافة الاباء | AddNewParent() | always |
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
| (icon only) | EditParent($index) | always |
| (icon only) | ViewEditRelationPOPUPFun($index) | always |
| (icon only) | EditChild($index) | always |
| (icon only) | ShowChecksOfChild(child.Id) | child.Code != null |
| (icon only) | RemovePhone($index) | IsAdmin |
| (icon only) | RemoveIncome($index,type.Number) | always |
| (icon only) | RemoveImage() | ChildPhoto!=null |
| (icon only) | RemoveNationalIdPic() | NationalIdPic!=null |
| (icon only) | IschildExcluded($event) | always |
| (icon only) | RemoveChildBirthPic() | ChildBirthPic!=null |
| (icon only) | RemoveChildEduPic() | ChildEduPic!=null |

#### 11.S.3  Screen `#/housing-projects/:id/reports`


| Property | Value |
| --- | --- |
| Angular route | `#/housing-projects/:id/reports` |
| Feature module | `housing-projects` (lazy-loaded) |
| Component | `HousingReportListComponent` |
| Route status | planned |
| Data-entry fields | 2 |
| Grids on the screen | 1 |
| Commands | 9 |


| Section | Field (as labelled) | Bound to | Control | Mandatory · source · rules |
| --- | --- | --- | --- | --- |
| — | الجمعية | CharityId | Drop-down list | Optional · options: lookup: Charities (+ كافة الجهات) · on change: getCharityData() |
| — | (unlabelled) | ChildNameSearch | Text box | Optional |

Grids on this screen:

| Grid (row source) | Columns |
| --- | --- |
| report in Orphanreports track by $index | الرقم · رقم التقرير · التاريخ · تم الاعتماد · تاريخ الاعتماد |

Commands on this screen:

| Command | Handler | Shown when |
| --- | --- | --- |
| نعم | DeleteOrpReport() | always |
| (icon only) | AddOrpReport() | always |
| (icon only) | EditOrpReport(report.Id) | always |
| (icon only) | DeleteAnViewModel(report.Id) | always |
| (icon only) | GotoPrintAction(report.Id) | always |
| (icon only) | GotoPrintActionV2(report.Id) | always |
| (icon only) | GotoPrintActionStudents(report.Id) | always |
| (icon only) | GetNext() | always |
| (icon only) | GetPrev() | always |

#### 11.S.4  Screen `#/housing-projects/:id/reports/:reportId`


| Property | Value |
| --- | --- |
| Angular route | `#/housing-projects/:id/reports/:reportId` |
| Feature module | `housing-projects` (lazy-loaded) |
| Component | `HousingReportFormComponent` |
| Route status | planned |
| Data-entry fields | 52 |
| Grids on the screen | 0 |
| Commands | 12 |


| Section | Field (as labelled) | Bound to | Control | Mandatory · source · rules |
| --- | --- | --- | --- | --- |
| — | تاريخ التقرير | ReportDateSearch | Date picker | Optional |
| — | رقم التقرير | reportNumberSearch | Text box | Optional · read-only |
| — | الجمعية | CharityId | Drop-down list | Optional · options: lookup: Charities (+ كافة الجهات) · read-only |
| — | العمر | ChildNameSearch | Text box | Optional · read-only |
| — | (unlabelled) | ChildAgeSearch | Text box | Optional · read-only |
| الحالة الصحية | * الحالة الصحية | HealthStatus | Drop-down list | Optional · options: سليم / معاق / مريض · on change: HealthstaueFun() |
| الحالة الصحية | * نوع المرض | DisType | Text box | Optional |
| الحالة الصحية | * نوع الاعاقة | disabilityType | Drop-down list | Optional · options: حركية / بصرية / سمعية / ذهنية · on change: ChangedisabilityType() |
| الحالة الصحية | * تفاصيل الاعاقة | DisabilityDescription | Multi-line text | Optional |
| الحالة التعليمية | * نوعية العمل | EducType | Drop-down list | Optional · options: lookup: Professions · on change: WorktypeofChildchanged() |
| الحالة التعليمية | * المرحلة الدراسية | ChildEducationallevel | Drop-down list | Optional · options: lookup: EducationalLevels · on change: GetEduStages(true,'student') |
| الحالة التعليمية | الصف الدراسي * | ChildEduStage | Drop-down list | Optional · options: lookup: EducationStage |
| الحالة التعليمية | * اسم الموسسة التعليمية | ChildSchool | Text box | Optional |
| الحالة التعليمية | * نوع التعليم | SchoolType | Drop-down list | Optional · options: حكومي / أهلي |
| الحالة التعليمية | * الكلية | ChildFacu | Text box | Optional |
| الحالة التعليمية | * التخصص/القسم | ChildSpecial | Text box | Optional |
| الحالة التعليمية | * اخر تقدير | Grads | Drop-down list | Optional · options: ممتاز / جيدجدا / جيد / مقبول / ضعيف |
| الحالة التعليمية | * السنه الدراسيه | EducationalYear | Numeric box | Optional |
| الحالة التعليمية | * sp الحالة | ChildWorkingStatus | Drop-down list | Optional · options: حاصل على شهادة / ترك الدراسة · on change: ChildWorkingStatuschanged() |
| الحالة التعليمية | * اعلى مؤهل دراسى | HihEductionStage | Drop-down list | Optional · options: lookup: EducationalLevelOfGraduate · on change: HihEductionStagechanged() |
| الحالة التعليمية | * اسم الموسسة التعليمية | ChildSchool | Text box | Optional |
| الحالة التعليمية | * الكلية | ChildFacu | Text box | Optional |
| الحالة التعليمية | * التخصص/القسم | ChildSpecial | Text box | Optional |
| الحالة التعليمية | * تاريخ الحصول عليه | EdDate | Date picker | Optional |
| الحالة التعليمية | * تاريخ ترك الدراسة | DateLeftEdu | Date picker | Optional |
| الحالة التعليمية | * اخر مرحله دراسيه | LastLevel | Drop-down list | Optional · options: lookup: EducationalLevelsLeavingStudying · on change: GetEduStages(true,'working') |
| الحالة التعليمية | اخر صف دراسي * | ChildEduStage | Drop-down list | Optional · options: lookup: EducationStage |
| الحالة التعليمية | * التخصص/القسم | ChildSpecial | Text box | Optional |
| الملفات المطلوبه | الصوره (png , jpg , jpeg) | OrphanImage | File upload | Mandatory · accepts image/* |
| الملفات المطلوبه | القيد أو شهاده الطالب (png , jpg , jpeg) | OrphanCertificateImg | File upload | Optional · accepts image/* |
| الملفات المطلوبه | صوره التقرير الطبي (png , jpg , jpeg) | MedicalReportImg | File upload | Optional · accepts image/* |
| الملفات المطلوبه | شهاده الوفاه (png , jpg , jpeg) | OrphanDeadImage | File upload | Optional · accepts image/* |
| الملفات المطلوبه | عقد الزواج (png , jpg , jpeg) | OrphanMarriegeImage | File upload | Optional · accepts image/* |
| الانشطة والبرامج | * الهوايات التى يمارسها | ChildHobies | Drop-down list | Optional · options: صغيرالسن / رياضة / قراءة / حاسب الى / زراعة نباتات / تدبير منزلي / زخرفة / خياطة / اخرى |
| الانشطة والبرامج | * اسم الدوره | CourseName | Text box | Optional |
| الانشطة والبرامج | * اسم الرياضه | SportName | Text box | Optional |
| الانشطة والبرامج | * اسم المهنه | ProfessionName | Text box | Optional |
| الانشطة والبرامج | * الرساله للكافل | MessageId | Drop-down list | Optional · options: lookup: MessageReasons |
| الانشطة والبرامج | * تاريخ الزواج | DateMarry | Date picker | Optional |
| الانشطة والبرامج | تزوج | IsMarried | Check box | Optional · on click: MarriedFun() |
| الانشطة والبرامج | * تاريخ الوفاة | DateDie | Date picker | Optional |
| الانشطة والبرامج | توفى | IsDied | Check box | Optional · on click: DieFun() |
| الانشطة والبرامج | الموافقه علي التقرير | IsAccepted | Check box | Optional |
| الانشطة والبرامج | * سبب الرفض | RefuseReasonId | Drop-down list | Mandatory · options: lookup: RefuseReasons |
| الانشطة والبرامج | رفض التقرير | IsRefused | Check box | Optional |
| الانشطة والبرامج | طلب كفالة طالب يتيم | IsOrphanStudent | Check box | Optional · on click: IsOrphanStudentFun($event) |
| الانشطة والبرامج | * القسط السنوي للدراسة | AnnualFeeForStudy | Numeric box | Optional |
| الانشطة والبرامج | * عدد سنوات الدراسة | StudyingYears | Numeric box | Optional |
| الانشطة والبرامج | * عدد السنوات المتوقعى الباقية | RestStudyingYears | Numeric box | Optional |
| الانشطة والبرامج | * تاريخ التخرج | GraduationYear | Numeric box | Optional |
| الانشطة والبرامج | الموافقه علي التقرير | IsAccepted | Check box | Optional · read-only |
| الانشطة والبرامج | رفض التقرير | IsRefused | Check box | Optional |

Commands on this screen:

| Command | Handler | Shown when |
| --- | --- | --- |
| (icon only) | GotoPrintActionV2() | always |
| (icon only) | RemoveOrphanImage() | OrphanImage!=null |
| (icon only) | ShowOrphanImages(ChildId) | always |
| (icon only) | RemoveOrphanCertificateImg() | OrphanCertificateImg!=null |
| (icon only) | ShowCertificateImages(ChildId) | always |
| (icon only) | RemoveMedicalReportImg() | MedicalReportImg!=null |
| (icon only) | RemoveOrphanDeadImage() | OrphanDeadImage!=null |
| (icon only) | RemoveOrphanMarriegeImage() | OrphanMarriegeImage!=null |
| (icon only) | MarriedFun() | always |
| (icon only) | DieFun() | always |
| (icon only) | IsOrphanStudentFun($event) | always |
| حفظ | AddChildReport() | !IsAccepted \|\| IsAdmin \|\| IsRefused |

### 11.U  Use case scenarios

One expanded scenario for every use case of this module. Pre-conditions, flows and post-conditions are derived from the screen that hosts the function, the Web API action that serves it and the business-layer method that executes it; quoted messages are the literal strings returned by the business layer.

#### 11.U.1  UC-HOU-01 — List housing families قائمة الأسر الساكنة


| Item | Specification |
| --- | --- |
| Use case ID | UC-HOU-01 |
| Name | List housing families قائمة الأسر الساكنة |
| Type | Browse a list |
| Primary actor | Charity, HQ roles |
| Secondary actors | The system (Web API + business layer); the database. |
| Summary | Paged list of the families enrolled in the housing project for the charity. |
| Trigger | The actor opens the screen at `#/housing-projects` from the module menu. |
| Pre-conditions | 1. The actor is authenticated; the JWT access token is valid and the client holds the role, charity and country claims it carries.<br>2. The actor holds one of: charity, HQ roles.<br>3. Caller identity comes from the JWT, never from the request; the application service scopes the query to the charity (and country) that owns the user, unless the role is an HQ role permitted to pass an explicit `charityId` filter.<br>4. The SPA route `#/housing-projects` has loaded and its reference-data lookups have been populated. |
| Main flow | 1. The actor navigates to the screen at `#/housing-projects`.<br>2. The system loads the filter lookups the screen offers and, for HQ roles, the charity selector.<br>3. The SPA issues `GET /api/Families?familyType=Housing&charityId=` carrying id, userId, pageNum.<br>4. `CharitiesController` binds the typed request DTO and delegates to the application service.<br>5. `IFamilyService` validates the payload with its FluentValidation validator, applies the business rules and the charity scope, and persists through `IUnitOfWork`.<br>6. The result set is scoped to the caller’s charity and country, ordered and paged.<br>7. The SPA renders the page with the columns listed in the screen specification and the paging control. |
| Alternate flows | • An HQ role (General Director, Financial Director, Staff) may pass an explicit charity id and so read across the charity boundary; a charity user may not and always sees its own data.<br>• No row matches the criteria — the grid renders empty and the paging control reports zero pages. |
| Exception flows | • The session has expired or the role is not permitted — the request is rejected and the SPA routes back to the login state. |
| Post-conditions | • No stored data is changed — the operation is a read. |
| Realisation | Route `#/housing-projects` → `HousingProjectListComponent`<br>`GET /api/Families?familyType=Housing&charityId=` → `FamiliesController` → `IFamilyService` |

#### 11.U.2  UC-HOU-02 — Search housing families البحث في الأسر الساكنة


| Item | Specification |
| --- | --- |
| Use case ID | UC-HOU-02 |
| Name | Search housing families البحث في الأسر الساكنة |
| Type | Search / filter |
| Primary actor | Charity, HQ roles |
| Secondary actors | The system (Web API + business layer); the database. |
| Summary | Same criteria set as the main register (guardian name, orphan name, national ID, code, phone) applied to the housing population. |
| Trigger | The actor selects the function from the module menu or the screen that hosts it. |
| Pre-conditions | 1. The actor is authenticated; the JWT access token is valid and the client holds the role, charity and country claims it carries.<br>2. The actor holds one of: charity, HQ roles. |
| Main flow | 1. The actor reaches the function from the screen of this module that hosts it.<br>2. The actor enters the search criteria in the filter fields.<br>3. The actor presses the command button of the screen.<br>4. The SPA issues `GET /api/Families?familyType=Housing&search=` carrying …, type.<br>5. `CharitiesController` binds the typed request DTO and delegates to the application service.<br>6. `IFamilyService` validates the payload with its FluentValidation validator, applies the business rules and the charity scope, and persists through `IUnitOfWork`.<br>7. The matching rows are returned, scoped to the caller’s charity, and rendered in the result grid. |
| Alternate flows | • No row matches the criteria — the grid renders empty and the paging control reports zero pages. |
| Exception flows | • The session has expired or the role is not permitted — the request is rejected and the SPA routes back to the login state. |
| Post-conditions | • No stored data is changed — the operation is a read. |
| Realisation | `GET /api/Families?familyType=Housing&search=?…&type` → `FamiliesController` → `IFamilyService` |

#### 11.U.3  UC-HOU-03 — Register a housing family اضافة أسرة ساكنة


| Item | Specification |
| --- | --- |
| Use case ID | UC-HOU-03 |
| Name | Register a housing family اضافة أسرة ساكنة |
| Type | Create a record |
| Primary actor | Charity |
| Secondary actors | The system (Web API + business layer); the database. |
| Summary | Creates the housing family file including the building and flat to which the family is allocated, along with guardian and children data. |
| Trigger | The actor presses «حفظ» on the screen AddHousingFamily. |
| Pre-conditions | 1. The actor is authenticated; the JWT access token is valid and the client holds the role, charity and country claims it carries.<br>2. The actor holds one of: charity.<br>3. The SPA route `#/housing-projects/:id/edit` has loaded and its reference-data lookups have been populated. |
| Main flow | 1. The actor navigates to the screen at `#/housing-projects/:id/edit`. The screen opens in add mode.<br>2. The system loads the reference data the form needs (regions, centres, banks, types and the other lookup lists bound to the drop-downs).<br>3. The actor completes the input fields. Mandatory fields: القرية / الحي، المركز/ المدينة، المنطقة /المحافظة، رقم الشقه، رقم العماره، العنوان التفصيلى، بيانات الإتصال، قيمة الإيجار، نوع الدخل، السبب، نوعها، العلاقة، الاسم الثالث، الاسم الثانى ….<br>4. The actor presses «حفظ» (AddFamily()).<br>5. The SPA issues `POST /api/HousingProjects/projects` carrying [FromBody] FamilyContract family.<br>6. `FamiliesController` binds the typed request DTO and delegates to the application service.<br>7. `IFamilyService` validates the payload with its FluentValidation validator, applies the business rules and the charity scope, and persists through `IUnitOfWork`.<br>8. The business layer validates the payload, stamps the owning charity and the creating user, and persists the new record.<br>9. The system returns the outcome and the SPA confirms the save and returns to the list screen. |
| Alternate flows | • The actor abandons the form before saving — nothing is written and the record keeps its previous state. |
| Exception flows | • The session has expired or the role is not permitted — the request is rejected and the SPA routes back to the login state.<br>• The business layer returns «أحد المعيلين مكرر من قبل أكثر من مرة» and the operation is not applied.<br>• A mandatory field is empty or fails its format check — the save is refused and the field is flagged on the form. |
| Post-conditions | • A new record exists, owned by the charity of the creating user, and appears in the list screen of the module. |
| Realisation | Route `#/housing-projects/:id/edit` → `HousingProjectFormComponent`<br>`POST /api/HousingProjects/projects` → `HousingProjectsController` → `IHousingProjectService` |

#### 11.U.4  UC-HOU-04 — View / update a housing family بيانات الأسرة الساكنة


| Item | Specification |
| --- | --- |
| Use case ID | UC-HOU-04 |
| Name | View / update a housing family بيانات الأسرة الساكنة |
| Type | Update a record |
| Primary actor | Charity, HQ roles |
| Secondary actors | The system (Web API + business layer); the database. |
| Summary | Loads the housing family file for review or amendment, including its current accommodation allocation. |
| Trigger | The actor selects the function from the module menu or the screen that hosts it. |
| Pre-conditions | 1. The actor is authenticated; the JWT access token is valid and the client holds the role, charity and country claims it carries.<br>2. The actor holds one of: charity, HQ roles.<br>3. Caller identity comes from the JWT, never from the request; the application service scopes the query to the charity (and country) that owns the user, unless the role is an HQ role permitted to pass an explicit `charityId` filter.<br>4. The target record exists and its identifier is known to the screen. |
| Main flow | 1. The actor reaches the function from the screen of this module that hosts it. The screen opens in edit mode on the selected record.<br>2. The system loads the current values into the form.<br>3. The actor changes the fields to be corrected.<br>4. The actor presses the command button of the screen.<br>5. The SPA issues `GET /api/HousingProjects/projects/{id}` carrying userId, id.<br>6. `FamiliesController` binds the typed request DTO and delegates to the application service.<br>7. `IFamilyService` validates the payload with its FluentValidation validator, applies the business rules and the charity scope, and persists through `IUnitOfWork`.<br>8. The business layer re-validates the payload, applies the changes and persists them.<br>9. The system returns the outcome and the SPA refreshes the screen. |
| Alternate flows | • An HQ role (General Director, Financial Director, Staff) may pass an explicit charity id and so read across the charity boundary; a charity user may not and always sees its own data.<br>• The actor abandons the form before saving — nothing is written and the record keeps its previous state. |
| Exception flows | • The session has expired or the role is not permitted — the request is rejected and the SPA routes back to the login state.<br>• A mandatory field is empty or fails its format check — the save is refused and the field is flagged on the form. |
| Post-conditions | • The stored record carries the new values; no other record is affected. |
| Realisation | `GET /api/HousingProjects/projects/{id}` → `HousingProjectsController` → `IHousingProjectService` |

#### 11.U.5  UC-HOU-05 — Select building and flat اختيار المبنى والشقة


| Item | Specification |
| --- | --- |
| Use case ID | UC-HOU-05 |
| Name | Select building and flat اختيار المبنى والشقة |
| Type | Browse a list |
| Primary actor | Charity |
| Secondary actors | The system (Web API + business layer); the database. |
| Summary | The form loads the list of housing buildings and, once a building is chosen, the flats belonging to it, so the family can be assigned an accommodation unit. |
| Trigger | The actor selects the function from the module menu or the screen that hosts it. |
| Pre-conditions | 1. The actor is authenticated; the JWT access token is valid and the client holds the role, charity and country claims it carries.<br>2. The actor holds one of: charity. |
| Main flow | 1. The actor reaches the function from the screen of this module that hosts it.<br>2. The system loads the filter lookups the screen offers and, for HQ roles, the charity selector.<br>3. The SPA issues `GET /api/LookupManagement/housing-buildings`.<br>4. `LookupManagementController` binds the typed request DTO and delegates to the application service.<br>5. `ILookupService` validates the payload with its FluentValidation validator, applies the business rules and the charity scope, and persists through `IUnitOfWork`.<br>6. The result set is scoped to the caller’s charity and country, ordered and paged.<br>7. The SPA renders the page and the paging control. |
| Alternate flows | • No row matches the criteria — the grid renders empty and the paging control reports zero pages. |
| Exception flows | • The session has expired or the role is not permitted — the request is rejected and the SPA routes back to the login state. |
| Post-conditions | • No stored data is changed — the operation is a read. |
| Realisation | `GET /api/LookupManagement/housing-buildings` · `GET /api/LookupManagement/housing-flats` → `LookupManagementController` → `ILookupService` |

#### 11.U.6  UC-HOU-06 — List periodic reports of a housing beneficiary التقارير الدورية للأسر الساكنة


| Item | Specification |
| --- | --- |
| Use case ID | UC-HOU-06 |
| Name | List periodic reports of a housing beneficiary التقارير الدورية للأسر الساكنة |
| Type | Query a report |
| Primary actor | Charity, HQ roles |
| Secondary actors | The system (Web API + business layer); the database. |
| Summary | Shows the report history for a housing beneficiary, who may be either a child or the guardian — the ChildOrParent discriminator selects which. |
| Trigger | The actor opens the screen at `#/housing-projects/:id/reports` from the module menu. |
| Pre-conditions | 1. The actor is authenticated; the JWT access token is valid and the client holds the role, charity and country claims it carries.<br>2. The actor holds one of: charity, HQ roles.<br>3. Caller identity comes from the JWT, never from the request; the application service scopes the query to the charity (and country) that owns the user, unless the role is an HQ role permitted to pass an explicit `charityId` filter.<br>4. The SPA route `#/housing-projects/:id/reports` has loaded and its reference-data lookups have been populated. |
| Main flow | 1. The actor navigates to the screen at `#/housing-projects/:id/reports`.<br>2. The actor sets the report criteria (period, charity, country and the other filters the screen offers).<br>3. The actor runs the report.<br>4. The SPA issues `GET /api/PeriodicOrphanReports/by-orphan/{orphanId}` carrying id, ChildOrParent, num, userId.<br>5. `PeriodicOrphanReportsController` binds the typed request DTO and delegates to the application service.<br>6. `IPeriodicOrphanReportService` validates the payload with its FluentValidation validator, applies the business rules and the charity scope, and persists through `IUnitOfWork`.<br>7. The business layer composes the projection, scoped to the caller’s charity and country.<br>8. The result is rendered in the on-screen grid, from where it can be printed or exported. |
| Alternate flows | • An HQ role (General Director, Financial Director, Staff) may pass an explicit charity id and so read across the charity boundary; a charity user may not and always sees its own data.<br>• No row matches the criteria — the grid renders empty and the paging control reports zero pages. |
| Exception flows | • The session has expired or the role is not permitted — the request is rejected and the SPA routes back to the login state. |
| Post-conditions | • No stored data is changed — the operation is a read. |
| Realisation | Route `#/housing-projects/:id/reports` → `HousingReportListComponent`<br>`GET /api/PeriodicOrphanReports/by-orphan/{orphanId}` → `PeriodicOrphanReportsController` → `IPeriodicOrphanReportService` |

#### 11.U.7  UC-HOU-07 — Look up a housing beneficiary by code البحث بالكود


| Item | Specification |
| --- | --- |
| Use case ID | UC-HOU-07 |
| Name | Look up a housing beneficiary by code البحث بالكود |
| Type | Search / filter |
| Primary actor | Charity |
| Secondary actors | The system (Web API + business layer); the database. |
| Summary | Before a housing report is created the operator enters the beneficiary code; the system returns the matching child or guardian with their identifying data. |
| Trigger | The actor selects the function from the module menu or the screen that hosts it. |
| Pre-conditions | 1. The actor is authenticated; the JWT access token is valid and the client holds the role, charity and country claims it carries.<br>2. The actor holds one of: charity.<br>3. Caller identity comes from the JWT, never from the request; the application service scopes the query to the charity (and country) that owns the user, unless the role is an HQ role permitted to pass an explicit `charityId` filter. |
| Main flow | 1. The actor reaches the function from the screen of this module that hosts it.<br>2. The actor enters the search criteria in the filter fields.<br>3. The actor presses the command button of the screen.<br>4. The SPA issues `GET /api/PeriodicOrphanReports/by-orphan/{orphanId}` carrying code, ChildOrParent, charityId, userId.<br>5. `PeriodicOrphanReportsController` binds the typed request DTO and delegates to the application service.<br>6. `IOrphanService` validates the payload with its FluentValidation validator, applies the business rules and the charity scope, and persists through `IUnitOfWork`.<br>7. The matching rows are returned, scoped to the caller’s charity, and rendered in the result grid. |
| Alternate flows | • An HQ role (General Director, Financial Director, Staff) may pass an explicit charity id and so read across the charity boundary; a charity user may not and always sees its own data.<br>• No row matches the criteria — the grid renders empty and the paging control reports zero pages. |
| Exception flows | • The session has expired or the role is not permitted — the request is rejected and the SPA routes back to the login state. |
| Post-conditions | • No stored data is changed — the operation is a read. |
| Realisation | `GET /api/PeriodicOrphanReports/by-orphan/{orphanId}` · `GET /api/HousingProjects/projects/{id}/beneficiaries` → `PeriodicOrphanReportsController` → `IPeriodicOrphanReportService` |

#### 11.U.8  UC-HOU-08 — Create a housing periodic report تقرير دوري لأسرة ساكنة


| Item | Specification |
| --- | --- |
| Use case ID | UC-HOU-08 |
| Name | Create a housing periodic report تقرير دوري لأسرة ساكنة |
| Type | Create a record |
| Primary actor | Charity |
| Secondary actors | The system (Web API + business layer); the database. |
| Summary | Records the recurring status report for a housing beneficiary — health, education, behaviour, prayer, memorisation and accommodation condition — with attachments, and stores it against the housing beneficiary. |
| Trigger | The actor presses «حفظ» on the screen HousingorphanReport. |
| Pre-conditions | 1. The actor is authenticated; the JWT access token is valid and the client holds the role, charity and country claims it carries.<br>2. The actor holds one of: charity.<br>3. Caller identity comes from the JWT, never from the request; the application service scopes the query to the charity (and country) that owns the user, unless the role is an HQ role permitted to pass an explicit `charityId` filter.<br>4. The SPA route `#/housing-projects/:id/reports/:reportId` has loaded and its reference-data lookups have been populated. |
| Main flow | 1. The actor navigates to the screen at `#/housing-projects/:id/reports/:reportId`. The screen opens in add mode.<br>2. The system loads the reference data the form needs (regions, centres, banks, types and the other lookup lists bound to the drop-downs).<br>3. The actor completes the input fields. Mandatory fields: الصوره (png , jpg , jpeg)، * سبب الرفض.<br>4. The actor presses «حفظ» (AddChildReport()).<br>5. The SPA issues `POST /api/PeriodicOrphanReports` carrying userId.<br>6. `PeriodicOrphanReportsController` binds the typed request DTO and delegates to the application service.<br>7. `IPeriodicOrphanReportService` validates the payload with its FluentValidation validator, applies the business rules and the charity scope, and persists through `IUnitOfWork`.<br>8. The business layer validates the payload, stamps the owning charity and the creating user, and persists the new record.<br>9. The system returns the outcome and the SPA confirms the save and returns to the list screen. |
| Alternate flows | • An HQ role (General Director, Financial Director, Staff) may pass an explicit charity id and so read across the charity boundary; a charity user may not and always sees its own data.<br>• The actor abandons the form before saving — nothing is written and the record keeps its previous state. |
| Exception flows | • The session has expired or the role is not permitted — the request is rejected and the SPA routes back to the login state.<br>• A mandatory field is empty or fails its format check — the save is refused and the field is flagged on the form. |
| Post-conditions | • A new record exists, owned by the charity of the creating user, and appears in the list screen of the module. |
| Realisation | Route `#/housing-projects/:id/reports/:reportId` → `HousingReportFormComponent`<br>`POST /api/PeriodicOrphanReports` → `PeriodicOrphanReportsController` → `IPeriodicOrphanReportService` |

### 11.A  Annex - Angular routes of this module

Routes are hash-based (`useHash: true`), rendered inside `MainLayoutComponent` behind
`AuthGuard` and `PermissionGuard`. *implemented* means the route exists in
`Frontend/src/app`; *planned* means it is specified here and not yet built.

| Angular route | Feature module | Component | Status |
| --- | --- | --- | --- |
| `#/housing-projects` | `housing-projects` | `HousingProjectListComponent` | implemented |
| `#/housing-projects/:id/edit` | `housing-projects` | `HousingProjectFormComponent` | implemented |
| `#/housing-projects/:id/reports` | `housing-projects` | `HousingReportListComponent` | planned |
| `#/housing-projects/:id/reports/:reportId` | `housing-projects` | `HousingReportFormComponent` | planned |

### 11.B  Annex - API controllers of this module

Every controller inherits `ApiController` (`[Authorize]`, route `api/[controller]`) and
returns `Framework.Core.ApiResponse<T>`. Business rules live in the application service
behind each controller, never in the controller itself.

| Controller | Base route | Responsibility |
| --- | --- | --- |
| `FamiliesController` | `api/Families` | Name autocomplete for orphans and housing beneficiaries. |
| `PeriodicOrphanReportsController` | `api/PeriodicOrphanReports` | Report history per orphan and per housing beneficiary. |

---

Shared context - system architecture, actors and roles, the sponsorship lifecycle, the full module map and all appendices - is in the companion document [00-Overview-and-Common-Context](00-Overview-and-Common-Context.md). The delivery backlog for this module is epic EP-06 in [00-EPICS-and-User-Stories](00-EPICS-and-User-Stories.md).

