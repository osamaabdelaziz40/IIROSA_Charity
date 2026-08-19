# WAR.IIROSA - Orphan Periodic Reports

التقارير الدورية للايتام | use case prefix `UC-ORR` | chapter 14 of the master document


| Field | Value |
| --- | --- |
| Document title | WAR.IIROSA - Module Specification: Orphan Periodic Reports |
| Module | Orphan Periodic Reports - التقارير الدورية للايتام |
| Use case prefix | UC-ORR |
| Chapter in master document | Chapter 14 |
| Documented use cases | 17 |
| Principal routes | `#/periodic-orphan-reports`, `/create`, `/:id`, `/:id/edit`, `/:id/review`, `/orphan-reports`, `/orphan-reports/generate`, `/orphan-reports/history`, `/orphan-reports/compare`, `/orphan-reports/schedule`, `/orphan-reports/search`, `/orphan-reports/orphan/:orphanId` |
| Version | 1.1 |
| Status | Chapter content extracted verbatim; screen fields and scenarios derived from the source code |
| Date | 18 August 2026 |
| Parent document | WAR.IIROSA-Project-Documentation.docx v1.0 (repository WAR.IIROSA, commit 4beb736) |

## Contents of this document

1. Chapter 14 — module purpose and use-case catalogue (verbatim from the master document)
2. §14.D — detailed specifications carried over from chapter 25
3. §14.S — screen field specifications (every field of every screen, derived from the AngularJS views)
4. §14.U — one expanded scenario per use case (trigger, pre-conditions, main flow, alternates, exceptions, post-conditions)
5. §14.A / §14.B — annexes: screens and Web API controllers of this module

## 14. Orphan Periodic Reports

التقارير الدورية للأيتام — the recurring status report is the evidence that sponsorship funds are reaching a living, identified beneficiary. Charities create reports; HQ accepts or refuses them; only accepted reports satisfy the payment prerequisite.

### 14.1 Report content and states


| Aspect | Detail |
| --- | --- |
| Recorded dimensions | Health status and medical condition, educational level / stage / class and educational status, behaviour and manners, prayer level, Qur'an and Hadith memorisation, hobbies, programmes offered, family circumstances, guardian data and notes. |
| Attachments | Orphan photographs and certificate/document images, uploaded through the file service and referenced by id on the report. |
| States | Submitted (created by the charity) → Accepted (IsAccepted = true) or Refused (IsRefused = true plus one or more recorded reasons). Setting accepted clears refused; re-submitting a refused report clears the refusal so it re-enters the queue. |
| Printing | The `orphan-report-form` report selects the correct printed variant according to which supporting documents exist (marriage, death, medical, certificate) and whether the orphan is studying or disabled. |

### 14.2 Use cases


| ID | Use case | Primary actor | Description & main flow | Realisation |
| --- | --- | --- | --- | --- |
| UC-ORR-01 | List an orphan's periodic reports التقارير الدورية لليتيم | Charity, HQ roles | Shows the paged report history for an orphan, identified by code, with the date and approval state of each report. | Route `#/periodic-orphan-reports` → GET /api/OrphanReports |
| UC-ORR-02 | Look up an orphan by code before reporting استدعاء اليتيم بالكود | Charity | The operator enters the sponsorship code; the system returns the orphan with their family and identifying data, pre-filling the report header. Alternate: an unknown or uncoded orphan cannot be reported on. | GET /api/PeriodicOrphanReports/by-orphan/{orphanId} |
| UC-ORR-03 | Create a periodic report إضافة تقرير دوري | Charity | Captures the full status assessment and attachments and stores it against the orphan with the reporting date and the submitting charity. Pre-condition: orphan is coded and the charity's add permission is enabled. | Route `#/periodic-orphan-reports/create` → POST /api/PeriodicOrphanReports |
| UC-ORR-04 | View a periodic report عرض التقرير | Charity, HQ roles | Loads one report in read-only mode with all recorded dimensions and its attachments. | GET /api/PeriodicOrphanReports |
| UC-ORR-05 | Update a periodic report تعديل التقرير | Charity, HQ roles | Amends a previously submitted report. If the report had been refused and is re-submitted unchanged in approval state, the refusal flag is cleared so it returns to the review queue. | PUT /api/PeriodicOrphanReports |
| UC-ORR-06 | Delete a periodic report حذف التقرير | HQ roles | Removes an erroneous report together with its attachment references. | DELETE /api/PeriodicOrphanReports |
| UC-ORR-07 | Accept a periodic report اعتماد التقرير | Gen. Director, Staff | The reviewer marks the report accepted. The system sets IsAccepted, forces IsRefused to false, and the orphan thereby satisfies the reporting prerequisite for the next payment batch. | PUT /api/PeriodicOrphanReports with IsAccepted |
| UC-ORR-08 | Refuse a periodic report with reasons رفض التقرير مع الأسباب | Gen. Director, Staff | The reviewer marks the report refused and selects one or more standard refusal reasons from the reason catalogue; the reasons are stored with the report and shown to the charity so it can correct and resubmit. | PUT /api/PeriodicOrphanReports with IsRefused; GET /api/LookupManagement/general-reasons |
| UC-ORR-09 | Filter reports by status حالة اليتيم | HQ roles, charity | The orphan-status screen filters the report population by charity, batch, date range, code list and approval state, returning the matching orphans and their report state. | Route `#/periodic-orphan-reports/orphan-reports/search` → POST /api/PeriodicOrphanReports/{id}/review (OrphanStatusRefinedContract) |
| UC-ORR-10 | View report statistics by group احصائيات عامة للأيتام | General Director | Returns aggregate counts of orphan reports grouped by status for the selected filters, giving HQ a one-screen view of where the reporting cycle stands. | Route `#/periodic-orphan-reports/orphan-reports` → POST /api/OrphanReports/statistics |
| UC-ORR-11 | Extract detailed report data تفاصيل التقارير | HQ roles | Produces the detailed report extract for a charity, report number, batch and date range — the general-purpose data pull used for analysis and export. | POST /api/OrphanReports/generate |
| UC-ORR-12 | Extract accepted reports التقارير المعتمدة | HQ roles | The same detailed extract restricted to accepted reports, used to confirm which orphans are cleared for disbursement. | GET /api/PeriodicOrphanReports/approved |
| UC-ORR-13 | Extract refused reports التقارير المرفوضة | HQ roles | The detailed extract restricted to refused reports, driving the correction worklist sent back to charities. | GET /api/PeriodicOrphanReports/rejected |
| UC-ORR-14 | List orphans with no renewed report الأيتام بدون تقرير مجدد | HQ roles | For a charity and batch, lists the coded orphans that have not submitted a current report, in full, count-only and V2 variants used by different screens. This is the primary chase list before a payment run. | POST /api/Reports/non-renewed-reports, …V2, …_Number, GetBeginingScreen |
| UC-ORR-15 | Extract report numbers added in a period أرقام التقارير المضافة | HQ roles | Lists the report numbers registered for a charity between two dates, optionally restricted to a payment batch — used to reconcile submissions against correspondence. | POST /api/OrphanReports/statistics |
| UC-ORR-16 | View report attachments صور اليتيم والشهادات | Charity, HQ roles | Returns the identifiers of the orphan photographs and of the certificate/document images attached to a report, which the client then streams from the file service. | GET /api/Attachments/{id}/image GetCertificateImages?Id; same pair on OrphanReportDetailed |
| UC-ORR-17 | Print the periodic report form طباعة التقرير الدوري | Charity, HQ roles | Renders the official orphan report form as PDF. The system selects the layout that matches the case — studying / not studying / disabled, and which of the marriage, death, medical and certificate documents are present — from the 24 available templates. | /api/Reports/orphan-report-form/export/pdf, …V2, …V3, …Students |

### 14.D  Detailed use case specifications (from chapter 25)

Reproduced verbatim from chapter 25 of the master document — the fully expanded specification of this module’s critical end-to-end scenarios. Every other use case of the module is specified in §14.U.

**25.4 UC-ORR-03 — Submit a periodic report**


| Use case ID | UC-ORR-03 |
| --- | --- |
| Name | Submit a periodic report — إضافة تقرير دوري |
| Primary actor | Charity user |
| Goal | Provide the recurring evidence of the orphan's situation that justifies continued sponsorship. |
| Pre-conditions | The orphan is coded; the charity's add permission is enabled; photographs and documents are available for upload. |
| Trigger | The reporting cycle opens, or the orphan appears on the "needs a report" chase list (UC-RPT-15). |
| Main flow | 1. The user opens the orphanReport state and enters the orphan's code. 2. The system returns the orphan and family identifying data and pre-fills the report header (UC-ORR-02). 3. The user records health and medical status, educational level, stage, class and educational status, behaviour, prayer level, Qur'an and Hadith memorisation, hobbies, programmes offered and family circumstances. 4. The user uploads the orphan photograph and any certificates or documents (UC-SYS-01), which are referenced on the report. 5. The user saves; the client posts to POST /api/PeriodicOrphanReports. 6. IPeriodicOrphanReportService creates the Orphans_Status record with all recorded dimensions, the reporting date and the submitting charity, and commits. 7. The report enters the HQ review queue (UC-RPT-17). |
| Alternate flows | A1 — Unknown or uncoded orphan. Step 2 returns nothing; the report cannot be created. A2 — Correcting a refused report. The user opens the refused report and updates it (UC-ORR-05); the refusal flag is cleared and it re-enters the queue. A3 — Housing beneficiary. The user follows UC-HOU-08, which stores a housing status record and may target the guardian rather than a child. |
| Exception flow | E1. Persistence failure aborts the save; the error is logged and a failure status returned. |
| Post-conditions | A submitted report exists against the orphan, neither accepted nor refused, awaiting HQ review. |
| Business rules | BR-10 A report belongs to exactly one orphan and one submitting charity. BR-11 Only an accepted report satisfies the payment prerequisite. BR-12 Attachments are stored by reference, not embedded in the report. |


**25.5 UC-ORR-07 / UC-ORR-08 — Review a periodic report**


| Use case ID | UC-ORR-07 (accept) / UC-ORR-08 (refuse) |
| --- | --- |
| Name | Review a periodic report — اعتماد أو رفض التقرير |
| Primary actor | General Director or Staff |
| Goal | Decide whether the submitted evidence is sufficient for the orphan to remain in the disbursement population. |
| Pre-conditions | A submitted report exists in the review queue. |
| Main flow | 1. The reviewer opens the queue of reports awaiting approval (UC-RPT-17) and selects a report. 2. The system loads the full report including the photographs and documents. 3. The reviewer verifies the identity evidence, the currency of the data and the completeness of the attachments. 4a. Accept: the reviewer marks the report accepted; the system sets IsAccepted and forces IsRefused to false, and the orphan is cleared for the next batch. 4b. Refuse: the reviewer selects one or more standard reasons from the reason catalogue (UC-SYS-06) and marks the report refused; the reasons are stored with the report. 5. The system commits and the report leaves the pending queue. 6. A refused report appears on the charity's correction list (UC-RPT-18) with its reasons. |
| Alternate flows | A1 — Resubmission. When the charity edits a refused report and neither approval flag is set, the system clears the refusal so the report returns to the pending queue. A2 — Erroneous report. The reviewer deletes it instead (UC-ORR-06). |
| Post-conditions | The report is accepted or refused; the orphan's eligibility for the next payment batch is determined accordingly. |
| Business rules | BR-13 A report cannot be both accepted and refused — accepting clears refusal. BR-14 A refusal must carry at least one recorded reason. BR-15 Only HQ roles may review. |


### 14.S  Screen field specifications

Derived from the AngularJS views of this module. For every screen the table lists each data-entry field in the order it appears, the scope property it binds to, its control type, whether the form marks it mandatory, and the lookup or rule that governs it. Labels are reproduced as they appear on screen (Arabic, right-to-left).

#### 14.S.1  Screen `#/periodic-orphan-reports`


| Property | Value |
| --- | --- |
| Angular route | `#/periodic-orphan-reports` |
| Feature module | `periodic-orphan-reports` (lazy-loaded) |
| Component | `PeriodicReportsListComponent` |
| Route status | built, not reachable (feature module unregistered in `app-routing.module.ts`) |
| Data-entry fields | 7 |
| Grids on the screen | 1 |
| Commands | 17 |


| Section | Field (as labelled) | Bound to | Control | Mandatory · source · rules |
| --- | --- | --- | --- | --- |
| — | الجمعية | CharityId | Drop-down list | Optional · options: lookup: Charities (+ كافة الجهات) · on change: getCharityData() |
| — | * من تاريخ | DateTo | Date picker | Optional |
| — | أكواد | DateFrom | Date picker | Optional |
| — | الدفعة المالية | IsCodes | Check box | Optional |
| — | (unlabelled) | BNumberFilter | Drop-down list | Optional · options: lookup: Batches (+ كل الدفعات) |
| — | كود اليتيم | ChildCodeSearch | Text box | Optional |
| — | اسم اليتيم | ChildNameSearch | Text box | Optional |

Grids on this screen:

| Grid (row source) | Columns |
| --- | --- |
| report in Orphanreports track by $index | الرقم · رقم التقرير · التاريخ · تم الاعتماد · تاريخ الاعتماد |

Commands on this screen:

| Command | Handler | Shown when |
| --- | --- | --- |
| نعم | DeleteOrpReport() | always |
| (icon only) | ExtractReportNumbersThatAdded() | always |
| (icon only) | ExtractParentMezaCard(true) | always |
| (icon only) | ExtractOrphanNonRenewedReportData_Number() | always |
| (icon only) | ExtractParentMezaCard(false) | always |
| (icon only) | ExtractOrphanNonRenewedReportData() | always |
| (icon only) | ExtractRefusedOrphansReportsData() | always |
| (icon only) | ExtractOrphansReportsData() | always |
| (icon only) | ExtractAcceptedOrphansReportsData() | always |
| (icon only) | AddOrpReport() | always |
| (icon only) | EditOrpReport(report.Id) | always |
| (icon only) | DeleteAnViewModel(report.Id) | always |
| (icon only) | GotoPrintAction(report.Id) | always |
| (icon only) | GotoPrintActionV2(report.Id) | always |
| (icon only) | GotoPrintActionStudents(report.Id) | always |
| (icon only) | GetNext() | always |
| (icon only) | GetPrev() | always |

#### 14.S.2  Screen `#/periodic-orphan-reports/create` and `#/periodic-orphan-reports/:id/edit`


| Property | Value |
| --- | --- |
| Angular route | `#/periodic-orphan-reports/create` and `#/periodic-orphan-reports/:id/edit` (one component, both routes) |
| Feature module | `periodic-orphan-reports` (lazy-loaded) |
| Component | `PeriodicReportFormComponent` |
| Route status | built, not reachable (feature module unregistered in `app-routing.module.ts`) |
| Data-entry fields | 53 |
| Grids on the screen | 0 |
| Commands | 12 |


| Section | Field (as labelled) | Bound to | Control | Mandatory · source · rules |
| --- | --- | --- | --- | --- |
| — | تاريخ التقرير | ReportDateSearch | Date picker | Optional |
| — | رقم التقرير | reportNumberSearch | Text box | Optional · read-only |
| — | الجمعية | CharityId | Drop-down list | Optional · options: lookup: Charities (+ كافة الجهات) · read-only |
| — | عمر اليتيم | ChildNameSearch | Text box | Optional · read-only |
| — | كود اليتيم | ChildAgeSearch | Text box | Optional · read-only |
| — | (unlabelled) | ChildCodeSearch | Text box | Optional · read-only |
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
| الملفات المطلوبه | صوره اليتيم (png , jpg , jpeg) | OrphanImage | File upload | Mandatory · accepts image/* |
| الملفات المطلوبه | القيد أو شهاده الطالب (png , jpg , jpeg) | OrphanCertificateImg | File upload | Optional · accepts image/* |
| الملفات المطلوبه | صوره التقرير الطبي (png , jpg , jpeg) | MedicalReportImg | File upload | Optional · accepts image/* |
| الملفات المطلوبه | شهاده وفاه اليتيم (png , jpg , jpeg) | OrphanDeadImage | File upload | Optional · accepts image/* |
| الملفات المطلوبه | عقد زواج اليتيم (png , jpg , jpeg) | OrphanMarriegeImage | File upload | Optional · accepts image/* |
| الانشطة والبرامج | * الهوايات التى يمارسها اليتيم | ChildHobies | Drop-down list | Optional · options: صغيرالسن / رياضة / قراءة / حاسب الى / زراعة نباتات / تدبير منزلي / زخرفة / خياطة / اخرى |
| الانشطة والبرامج | * اسم الدوره | CourseName | Text box | Optional |
| الانشطة والبرامج | * اسم الرياضه | SportName | Text box | Optional |
| الانشطة والبرامج | * اسم المهنه | ProfessionName | Text box | Optional |
| الانشطة والبرامج | * رساله اليتيم للكافل | MessageId | Drop-down list | Optional · options: lookup: MessageReasons |
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

#### 14.S.3  Screen `#/periodic-orphan-reports/orphan-reports`


| Property | Value |
| --- | --- |
| Angular route | `#/periodic-orphan-reports/orphan-reports` |
| Feature module | `periodic-orphan-reports` (lazy-loaded) |
| Component | `OrphanReportsListComponent` |
| Route status | built, not reachable (feature module unregistered in `app-routing.module.ts`) |
| Data-entry fields | 1 |
| Grids on the screen | 1 |
| Commands | 3 |


| Section | Field (as labelled) | Bound to | Control | Mandatory · source · rules |
| --- | --- | --- | --- | --- |
| — | الجمعية | CharityId | Drop-down list | Optional · options: lookup: Charities (+ كافة الجهات) · on change: getCharityData() |

Grids on this screen:

| Grid (row source) | Columns |
| --- | --- |
| parent in OrphanReportGroupCount track by $index | الرقم · الحاله التعليميه · المرحله الدراسيه · الاناث · الذكور · الاجمالي |

Commands on this screen:

| Command | Handler | Shown when |
| --- | --- | --- |
| استخراج البيانات | ExportData() | always |
| (icon only) | GetNext() | always |
| (icon only) | GetPrev() | always |

#### 14.S.4  Screen `#/periodic-orphan-reports/orphan-reports/search`


| Property | Value |
| --- | --- |
| Angular route | `#/periodic-orphan-reports/orphan-reports/search` |
| Feature module | `periodic-orphan-reports` (lazy-loaded) |
| Component | `OrphanReportSearchComponent` |
| Route status | built, not reachable (feature module unregistered in `app-routing.module.ts`) |
| Data-entry fields | 10 |
| Grids on the screen | 1 |
| Commands | 2 |


| Section | Field (as labelled) | Bound to | Control | Mandatory · source · rules |
| --- | --- | --- | --- | --- |
| — | الجمعية | CharityId | Drop-down list | Optional · options: lookup: Charities (+ كافة الجهات) · on change: getCharityData() |
| — | Or | OrphanStatusRefinedContract.AndOr | Radio button | Optional |
| — | And | OrphanStatusRefinedContract.AndOr | Radio button | Optional |
| — | نوع التعليم | OrphanStatusRefinedContract.SchoolType | Drop-down list | Optional · options: حكومي / أهلي |
| — | * الحالة الصحية | OrphanStatusRefinedContract.HealthStatus | Drop-down list | Optional · options: الكل / سليم / معاق / مريض |
| — | الحالة الاجتماعية | OrphanStatusRefinedContract.MaritalStatus | Drop-down list | Optional · options: الكل / تزوج / اعزب / متوفى |
| — | الحالة التعليمية | OrphanStatusRefinedContract.EducationalStatus | Drop-down list | Optional · options: الكل / يدرس / حاصل على شهادة / ترك الدراسة · on change: WorktypeofChildchanged() |
| — | الصف الدراسي | ChildEduStage | Drop-down list | Optional · options: lookup: EducationStage |
| — | المرحلة الدراسية | ChildEducationallevel | Drop-down list | Optional · options: lookup: EducationalLevelOfGraduate · on change: GetEduStages() |
| — | * اخر تقدير | EducationDegree | Drop-down list | Optional · options: ممتاز / جيدجدا / جيد / مقبول / ضعيف |

Grids on this screen:

| Grid (row source) | Columns |
| --- | --- |
| Orphan in Orphans track by $index | رقم اليتيم · أسم اليتيم · عمر اليتيم · أسم المعيل · صلة القرابة · المحافظة · المركز · القرية/الحى · العنوان التفصيلى · ت المنزل · ت الموبايل · تاريخ الميلاد · الرقم القومى · النوع · مؤهل المعيل · مهنةالمعيل · الرقم القومى للمعيل · مشروع تنموى للمعيل · الاستبعاد · سبب الاستبعاد · أسم الجمعية · تاريخ اخر تحديث · نوع التعليم · الكليه · المدرسه · تاريخ الوفاه · تاريخ الزواج · تاريخ التقرير · الحاله الصحيه · بنود الاعاقه · بنود المرض · المرحله الدراسيه · أخر صف دراسي · التخصص · التقدير · الحاله · كود العائله |

Commands on this screen:

| Command | Handler | Shown when |
| --- | --- | --- |
| بحث | GetData() | always |
| deاستخراج البيانات | ExportData() | always |

### 14.U  Use case scenarios

One expanded scenario for every use case of this module. Pre-conditions, flows and post-conditions are derived from the screen that hosts the function, the Web API action that serves it and the business-layer method that executes it; quoted messages are the literal strings returned by the business layer.

#### 14.U.1  UC-ORR-01 — List an orphan's periodic reports التقارير الدورية لليتيم


| Item | Specification |
| --- | --- |
| Use case ID | UC-ORR-01 |
| Name | List an orphan's periodic reports التقارير الدورية لليتيم |
| Type | Query a report |
| Primary actor | Charity, HQ roles |
| Secondary actors | The system (Web API + business layer); the database. |
| Summary | Shows the paged report history for an orphan, identified by code, with the date and approval state of each report. |
| Trigger | The actor opens the screen at `#/periodic-orphan-reports` from the module menu. |
| Pre-conditions | 1. The actor is authenticated; the JWT access token is valid and the client holds the role, charity and country claims it carries.<br>2. The actor holds one of: charity, HQ roles.<br>3. Caller identity comes from the JWT, never from the request; the application service scopes the query to the charity (and country) that owns the user, unless the role is an HQ role permitted to pass an explicit `charityId` filter.<br>4. The SPA route `#/periodic-orphan-reports` has loaded and its reference-data lookups have been populated. |
| Main flow | 1. The actor navigates to the screen at `#/periodic-orphan-reports`.<br>2. The actor sets the report criteria (period, charity, country and the other filters the screen offers).<br>3. The actor runs the report.<br>4. The SPA issues `GET /api/OrphanReports` carrying code, num, userId.<br>5. `PeriodicOrphanReportsController` binds the typed request DTO and delegates to the application service.<br>6. `IPeriodicOrphanReportService` validates the payload with its FluentValidation validator, applies the business rules and the charity scope, and persists through `IUnitOfWork`.<br>7. The business layer composes the projection, scoped to the caller’s charity and country.<br>8. The result is rendered in the on-screen grid, from where it can be printed or exported. |
| Alternate flows | • An HQ role (General Director, Financial Director, Staff) may pass an explicit charity id and so read across the charity boundary; a charity user may not and always sees its own data.<br>• No row matches the criteria — the grid renders empty and the paging control reports zero pages. |
| Exception flows | • The session has expired or the role is not permitted — the request is rejected and the SPA routes back to the login state. |
| Post-conditions | • No stored data is changed — the operation is a read. |
| Realisation | Route `#/periodic-orphan-reports` → `PeriodicReportsListComponent`<br>`GET /api/OrphanReports` → `OrphanReportsController` → `IOrphanReportService` |

#### 14.U.2  UC-ORR-02 — Look up an orphan by code before reporting استدعاء اليتيم بالكود


| Item | Specification |
| --- | --- |
| Use case ID | UC-ORR-02 |
| Name | Look up an orphan by code before reporting استدعاء اليتيم بالكود |
| Type | Query a report |
| Primary actor | Charity |
| Secondary actors | The system (Web API + business layer); the database. |
| Summary | The operator enters the sponsorship code; the system returns the orphan with their family and identifying data, pre-filling the report header. Alternate: an unknown or uncoded orphan cannot be reported on. |
| Trigger | The actor selects the function from the module menu or the screen that hosts it. |
| Pre-conditions | 1. The actor is authenticated; the JWT access token is valid and the client holds the role, charity and country claims it carries.<br>2. The actor holds one of: charity.<br>3. Caller identity comes from the JWT, never from the request; the application service scopes the query to the charity (and country) that owns the user, unless the role is an HQ role permitted to pass an explicit `charityId` filter. |
| Main flow | 1. The actor reaches the function from the screen of this module that hosts it.<br>2. The actor sets the report criteria (period, charity, country and the other filters the screen offers).<br>3. The actor runs the report.<br>4. The SPA issues `GET /api/PeriodicOrphanReports/by-orphan/{orphanId}` carrying code, charityId, userId.<br>5. `PeriodicOrphanReportsController` binds the typed request DTO and delegates to the application service.<br>6. `IOrphanService` validates the payload with its FluentValidation validator, applies the business rules and the charity scope, and persists through `IUnitOfWork`.<br>7. The business layer composes the projection, scoped to the caller’s charity and country.<br>8. The result is rendered in the on-screen grid, from where it can be printed or exported. |
| Alternate flows | • An HQ role (General Director, Financial Director, Staff) may pass an explicit charity id and so read across the charity boundary; a charity user may not and always sees its own data.<br>• No row matches the criteria — the grid renders empty and the paging control reports zero pages. |
| Exception flows | • The session has expired or the role is not permitted — the request is rejected and the SPA routes back to the login state. |
| Post-conditions | • No stored data is changed — the operation is a read. |
| Realisation | `GET /api/PeriodicOrphanReports/by-orphan/{orphanId}` → `PeriodicOrphanReportsController` → `IPeriodicOrphanReportService` |

#### 14.U.3  UC-ORR-03 — Create a periodic report إضافة تقرير دوري


| Item | Specification |
| --- | --- |
| Use case ID | UC-ORR-03 |
| Name | Create a periodic report إضافة تقرير دوري |
| Type | Create a record |
| Primary actor | Charity |
| Secondary actors | The system (Web API + business layer); the database. |
| Summary | Captures the full status assessment and attachments and stores it against the orphan with the reporting date and the submitting charity. Pre-condition: orphan is coded and the charity's add permission is enabled. |
| Trigger | The actor presses «حفظ» on the screen orphanReport. |
| Pre-conditions | 1. The actor is authenticated; the JWT access token is valid and the client holds the role, charity and country claims it carries.<br>2. The actor holds one of: charity.<br>3. Caller identity comes from the JWT, never from the request; the application service scopes the query to the charity (and country) that owns the user, unless the role is an HQ role permitted to pass an explicit `charityId` filter.<br>4. The SPA route `#/periodic-orphan-reports/:id/edit` has loaded and its reference-data lookups have been populated. |
| Main flow | 1. The actor navigates to the screen at `#/periodic-orphan-reports/:id/edit`. The screen opens in add mode.<br>2. The system loads the reference data the form needs (regions, centres, banks, types and the other lookup lists bound to the drop-downs).<br>3. The actor completes the input fields. Mandatory fields: صوره اليتيم (png , jpg , jpeg)، * سبب الرفض.<br>4. The actor presses «حفظ» (AddChildReport()).<br>5. The SPA issues `POST /api/PeriodicOrphanReports` carrying userId.<br>6. `PeriodicOrphanReportsController` binds the typed request DTO and delegates to the application service.<br>7. `IPeriodicOrphanReportService` validates the payload with its FluentValidation validator, applies the business rules and the charity scope, and persists through `IUnitOfWork`.<br>8. The business layer validates the payload, stamps the owning charity and the creating user, and persists the new record.<br>9. The system returns the outcome and the SPA confirms the save and returns to the list screen. |
| Alternate flows | • An HQ role (General Director, Financial Director, Staff) may pass an explicit charity id and so read across the charity boundary; a charity user may not and always sees its own data.<br>• The actor abandons the form before saving — nothing is written and the record keeps its previous state. |
| Exception flows | • The session has expired or the role is not permitted — the request is rejected and the SPA routes back to the login state.<br>• A mandatory field is empty or fails its format check — the save is refused and the field is flagged on the form. |
| Post-conditions | • A new record exists, owned by the charity of the creating user, and appears in the list screen of the module. |
| Realisation | Route `#/periodic-orphan-reports/create` → `PeriodicReportFormComponent`<br>`POST /api/PeriodicOrphanReports` → `PeriodicOrphanReportsController` → `IPeriodicOrphanReportService` |

#### 14.U.4  UC-ORR-04 — View a periodic report عرض التقرير


| Item | Specification |
| --- | --- |
| Use case ID | UC-ORR-04 |
| Name | View a periodic report عرض التقرير |
| Type | Query a report |
| Primary actor | Charity, HQ roles |
| Secondary actors | The system (Web API + business layer); the database. |
| Summary | Loads one report in read-only mode with all recorded dimensions and its attachments. |
| Trigger | The actor selects the function from the module menu or the screen that hosts it. |
| Pre-conditions | 1. The actor is authenticated; the JWT access token is valid and the client holds the role, charity and country claims it carries.<br>2. The actor holds one of: charity, HQ roles.<br>3. Caller identity comes from the JWT, never from the request; the application service scopes the query to the charity (and country) that owns the user, unless the role is an HQ role permitted to pass an explicit `charityId` filter. |
| Main flow | 1. The actor reaches the function from the screen of this module that hosts it.<br>2. The actor sets the report criteria (period, charity, country and the other filters the screen offers).<br>3. The actor runs the report.<br>4. The SPA issues `GET /api/PeriodicOrphanReports` carrying id, userId.<br>5. `PeriodicOrphanReportsController` binds the typed request DTO and delegates to the application service.<br>6. `IPeriodicOrphanReportService` validates the payload with its FluentValidation validator, applies the business rules and the charity scope, and persists through `IUnitOfWork`.<br>7. The business layer composes the projection, scoped to the caller’s charity and country.<br>8. The result is rendered in the on-screen grid, from where it can be printed or exported. |
| Alternate flows | • An HQ role (General Director, Financial Director, Staff) may pass an explicit charity id and so read across the charity boundary; a charity user may not and always sees its own data.<br>• No row matches the criteria — the grid renders empty and the paging control reports zero pages. |
| Exception flows | • The session has expired or the role is not permitted — the request is rejected and the SPA routes back to the login state. |
| Post-conditions | • No stored data is changed — the operation is a read. |
| Realisation | `GET /api/PeriodicOrphanReports` → `PeriodicOrphanReportsController` → `IPeriodicOrphanReportService` |

#### 14.U.5  UC-ORR-05 — Update a periodic report تعديل التقرير


| Item | Specification |
| --- | --- |
| Use case ID | UC-ORR-05 |
| Name | Update a periodic report تعديل التقرير |
| Type | Update a record |
| Primary actor | Charity, HQ roles |
| Secondary actors | The system (Web API + business layer); the database. |
| Summary | Amends a previously submitted report. If the report had been refused and is re-submitted unchanged in approval state, the refusal flag is cleared so it returns to the review queue. |
| Trigger | The actor selects the function from the module menu or the screen that hosts it. |
| Pre-conditions | 1. The actor is authenticated; the JWT access token is valid and the client holds the role, charity and country claims it carries.<br>2. The actor holds one of: charity, HQ roles.<br>3. Caller identity comes from the JWT, never from the request; the application service scopes the query to the charity (and country) that owns the user, unless the role is an HQ role permitted to pass an explicit `charityId` filter.<br>4. The target record exists and its identifier is known to the screen. |
| Main flow | 1. The actor reaches the function from the screen of this module that hosts it. The screen opens in edit mode on the selected record.<br>2. The system loads the current values into the form.<br>3. The actor changes the fields to be corrected.<br>4. The actor presses the command button of the screen.<br>5. The SPA issues `PUT /api/PeriodicOrphanReports` carrying userId.<br>6. `PeriodicOrphanReportsController` binds the typed request DTO and delegates to the application service.<br>7. `IPeriodicOrphanReportService` validates the payload with its FluentValidation validator, applies the business rules and the charity scope, and persists through `IUnitOfWork`.<br>8. The business layer re-validates the payload, applies the changes and persists them.<br>9. The system returns the outcome and the SPA refreshes the screen. |
| Alternate flows | • An HQ role (General Director, Financial Director, Staff) may pass an explicit charity id and so read across the charity boundary; a charity user may not and always sees its own data.<br>• The actor abandons the form before saving — nothing is written and the record keeps its previous state. |
| Exception flows | • The session has expired or the role is not permitted — the request is rejected and the SPA routes back to the login state.<br>• The business layer returns «You can not update old report» and the operation is not applied.<br>• A mandatory field is empty or fails its format check — the save is refused and the field is flagged on the form. |
| Post-conditions | • The stored record carries the new values; no other record is affected. |
| Realisation | `PUT /api/PeriodicOrphanReports` → `PeriodicOrphanReportsController` → `IPeriodicOrphanReportService` |

#### 14.U.6  UC-ORR-06 — Delete a periodic report حذف التقرير


| Item | Specification |
| --- | --- |
| Use case ID | UC-ORR-06 |
| Name | Delete a periodic report حذف التقرير |
| Type | Delete a record |
| Primary actor | HQ roles |
| Secondary actors | The system (Web API + business layer); the database. |
| Summary | Removes an erroneous report together with its attachment references. |
| Trigger | The actor selects the function from the module menu or the screen that hosts it. |
| Pre-conditions | 1. The actor is authenticated; the JWT access token is valid and the client holds the role, charity and country claims it carries.<br>2. The actor holds one of: HQ roles.<br>3. Caller identity comes from the JWT, never from the request; the application service scopes the query to the charity (and country) that owns the user, unless the role is an HQ role permitted to pass an explicit `charityId` filter.<br>4. The target record exists and its identifier is known to the screen. |
| Main flow | 1. The actor reaches the function from the screen of this module that hosts it.<br>2. The actor selects the record and requests its deletion.<br>3. The SPA asks the actor to confirm.<br>4. The SPA issues `DELETE /api/PeriodicOrphanReports` carrying id, userId.<br>5. `PeriodicOrphanReportsController` binds the typed request DTO and delegates to the application service.<br>6. `IPeriodicOrphanReportService` validates the payload with its FluentValidation validator, applies the business rules and the charity scope, and persists through `IUnitOfWork`.<br>7. The business layer checks that the record may still be removed and deletes it (or marks it removed).<br>8. The system returns the outcome and the SPA drops the row from the grid. |
| Alternate flows | • An HQ role (General Director, Financial Director, Staff) may pass an explicit charity id and so read across the charity boundary; a charity user may not and always sees its own data. |
| Exception flows | • The session has expired or the role is not permitted — the request is rejected and the SPA routes back to the login state. |
| Post-conditions | • The record is no longer returned by the list and read endpoints of the module. |
| Realisation | `DELETE /api/PeriodicOrphanReports` → `PeriodicOrphanReportsController` → `IPeriodicOrphanReportService` |

#### 14.U.7  UC-ORR-07 — Accept a periodic report اعتماد التقرير


| Item | Specification |
| --- | --- |
| Use case ID | UC-ORR-07 |
| Name | Accept a periodic report اعتماد التقرير |
| Type | Review decision |
| Primary actor | Gen. Director, Staff |
| Secondary actors | The system (Web API + business layer); the database. |
| Summary | The reviewer marks the report accepted. The system sets IsAccepted, forces IsRefused to false, and the orphan thereby satisfies the reporting prerequisite for the next payment batch. |
| Trigger | The actor selects the function from the module menu or the screen that hosts it. |
| Pre-conditions | 1. The actor is authenticated; the JWT access token is valid and the client holds the role, charity and country claims it carries.<br>2. The actor holds one of: Gen. Director, Staff.<br>3. Caller identity comes from the JWT, never from the request; the application service scopes the query to the charity (and country) that owns the user, unless the role is an HQ role permitted to pass an explicit `charityId` filter.<br>4. The target record exists and its identifier is known to the screen. |
| Main flow | 1. The actor reaches the function from the screen of this module that hosts it.<br>2. The actor opens the item awaiting a decision and examines its content and attachments.<br>3. The actor records the decision and, when refusing, the reason.<br>4. The actor presses the command button of the screen.<br>5. The SPA issues `PUT /api/PeriodicOrphanReports` carrying [FromBody]OrphanStatusContract childReport,[FromUri] string userId.<br>6. `PeriodicOrphanReportsController` binds the typed request DTO and delegates to the application service.<br>7. `IPeriodicOrphanReportService` validates the payload with its FluentValidation validator, applies the business rules and the charity scope, and persists through `IUnitOfWork`.<br>8. The business layer writes the new state, the deciding user and the decision date.<br>9. The item leaves the pending queue and becomes visible to the charity in its new state. |
| Alternate flows | • An HQ role (General Director, Financial Director, Staff) may pass an explicit charity id and so read across the charity boundary; a charity user may not and always sees its own data.<br>• The decision is a refusal — the reason is mandatory and is stored with the item so that the charity can see why it was returned. |
| Exception flows | • The session has expired or the role is not permitted — the request is rejected and the SPA routes back to the login state.<br>• The business layer returns «You can not update old report» and the operation is not applied. |
| Post-conditions | • The item carries its new state, the deciding user and the decision date, and moves out of the pending queue. |
| Realisation | `PUT /api/PeriodicOrphanReports` → `PeriodicOrphanReportsController` → `IPeriodicOrphanReportService` |

#### 14.U.8  UC-ORR-08 — Refuse a periodic report with reasons رفض التقرير مع الأسباب


| Item | Specification |
| --- | --- |
| Use case ID | UC-ORR-08 |
| Name | Refuse a periodic report with reasons رفض التقرير مع الأسباب |
| Type | Review decision |
| Primary actor | Gen. Director, Staff |
| Secondary actors | The system (Web API + business layer); the database. |
| Summary | The reviewer marks the report refused and selects one or more standard refusal reasons from the reason catalogue; the reasons are stored with the report and shown to the charity so it can correct and resubmit. |
| Trigger | The actor selects the function from the module menu or the screen that hosts it. |
| Pre-conditions | 1. The actor is authenticated; the JWT access token is valid and the client holds the role, charity and country claims it carries.<br>2. The actor holds one of: Gen. Director, Staff.<br>3. Caller identity comes from the JWT, never from the request; the application service scopes the query to the charity (and country) that owns the user, unless the role is an HQ role permitted to pass an explicit `charityId` filter.<br>4. The target record exists and its identifier is known to the screen. |
| Main flow | 1. The actor reaches the function from the screen of this module that hosts it.<br>2. The actor opens the item awaiting a decision and examines its content and attachments.<br>3. The actor records the decision and, when refusing, the reason.<br>4. The actor presses the command button of the screen.<br>5. The SPA issues `PUT /api/PeriodicOrphanReports` carrying [FromBody]OrphanStatusContract childReport,[FromUri] string userId.<br>6. `PeriodicOrphanReportsController` binds the typed request DTO and delegates to the application service.<br>7. `IPeriodicOrphanReportService` validates the payload with its FluentValidation validator, applies the business rules and the charity scope, and persists through `IUnitOfWork`.<br>8. The business layer writes the new state, the deciding user and the decision date.<br>9. The item leaves the pending queue and becomes visible to the charity in its new state. |
| Alternate flows | • An HQ role (General Director, Financial Director, Staff) may pass an explicit charity id and so read across the charity boundary; a charity user may not and always sees its own data.<br>• The decision is a refusal — the reason is mandatory and is stored with the item so that the charity can see why it was returned. |
| Exception flows | • The session has expired or the role is not permitted — the request is rejected and the SPA routes back to the login state.<br>• The business layer returns «You can not update old report» and the operation is not applied. |
| Post-conditions | • The item carries its new state, the deciding user and the decision date, and moves out of the pending queue. |
| Realisation | `PUT /api/PeriodicOrphanReports` · `GET /api/LookupManagement/general-reasons` → `PeriodicOrphanReportsController` → `IPeriodicOrphanReportService` |

#### 14.U.9  UC-ORR-09 — Filter reports by status حالة اليتيم


| Item | Specification |
| --- | --- |
| Use case ID | UC-ORR-09 |
| Name | Filter reports by status حالة اليتيم |
| Type | Search / filter |
| Primary actor | HQ roles, charity |
| Secondary actors | The system (Web API + business layer); the database. |
| Summary | The orphan-status screen filters the report population by charity, batch, date range, code list and approval state, returning the matching orphans and their report state. |
| Trigger | The actor presses «بحث» on the screen Orphanstatus. |
| Pre-conditions | 1. The actor is authenticated; the JWT access token is valid and the client holds the role, charity and country claims it carries.<br>2. The actor holds one of: HQ roles, charity.<br>3. The SPA route `#/periodic-orphan-reports/orphan-reports/search` has loaded and its reference-data lookups have been populated. |
| Main flow | 1. The actor navigates to the screen at `#/periodic-orphan-reports/orphan-reports/search`.<br>2. The actor enters the search criteria in the filter fields.<br>3. The actor presses «بحث» (GetData()).<br>4. The SPA issues `POST /api/PeriodicOrphanReports/{id}/review` carrying [FromUri]OrphanStatusRefinedContract orphanStatusRefinedObject.<br>5. `PeriodicOrphanReportsController` binds the typed request DTO and delegates to the application service.<br>6. `IPeriodicOrphanReportService` validates the payload with its FluentValidation validator, applies the business rules and the charity scope, and persists through `IUnitOfWork`.<br>7. The matching rows are returned, scoped to the caller’s charity, and rendered in the result grid. |
| Alternate flows | • No row matches the criteria — the grid renders empty and the paging control reports zero pages. |
| Exception flows | • The session has expired or the role is not permitted — the request is rejected and the SPA routes back to the login state.<br>• The business layer returns «Faliure» and the operation is not applied. |
| Post-conditions | • No stored data is changed — the operation is a read. |
| Realisation | Route `#/periodic-orphan-reports/orphan-reports/search` → `OrphanReportSearchComponent`<br>`POST /api/PeriodicOrphanReports/{id}/review` → `PeriodicOrphanReportsController` → `IPeriodicOrphanReportService` |

#### 14.U.10  UC-ORR-10 — View report statistics by group احصائيات عامة للأيتام


| Item | Specification |
| --- | --- |
| Use case ID | UC-ORR-10 |
| Name | View report statistics by group احصائيات عامة للأيتام |
| Type | Query a report |
| Primary actor | General Director |
| Secondary actors | The system (Web API + business layer); the database. |
| Summary | Returns aggregate counts of orphan reports grouped by status for the selected filters, giving HQ a one-screen view of where the reporting cycle stands. |
| Trigger | The actor opens the screen at `#/periodic-orphan-reports/orphan-reports` from the module menu. |
| Pre-conditions | 1. The actor is authenticated; the JWT access token is valid and the client holds the role, charity and country claims it carries.<br>2. The actor holds one of: General Director.<br>3. Caller identity comes from the JWT, never from the request; the application service scopes the query to the charity (and country) that owns the user, unless the role is an HQ role permitted to pass an explicit `charityId` filter.<br>4. The SPA route `#/periodic-orphan-reports/orphan-reports` has loaded and its reference-data lookups have been populated. |
| Main flow | 1. The actor navigates to the screen at `#/periodic-orphan-reports/orphan-reports`.<br>2. The actor sets the report criteria (period, charity, country and the other filters the screen offers).<br>3. The actor runs the report.<br>4. The SPA issues `POST /api/OrphanReports/statistics` carrying string userId, string familyId, string newCharityId.<br>5. `FamiliesController` binds the typed request DTO and delegates to the application service.<br>6. `IFamilyService` validates the payload with its FluentValidation validator, applies the business rules and the charity scope, and persists through `IUnitOfWork`.<br>7. The business layer composes the projection, scoped to the caller’s charity and country.<br>8. The result is rendered in the on-screen grid, from where it can be printed or exported. |
| Alternate flows | • An HQ role (General Director, Financial Director, Staff) may pass an explicit charity id and so read across the charity boundary; a charity user may not and always sees its own data.<br>• No row matches the criteria — the grid renders empty and the paging control reports zero pages. |
| Exception flows | • The session has expired or the role is not permitted — the request is rejected and the SPA routes back to the login state.<br>• The business layer returns «Faild Operation» and the operation is not applied. |
| Post-conditions | • No stored data is changed — the operation is a read. |
| Realisation | Route `#/periodic-orphan-reports/orphan-reports` → `OrphanReportsListComponent`<br>`POST /api/OrphanReports/statistics` → `OrphanReportsController` → `IOrphanReportService` |

#### 14.U.11  UC-ORR-11 — Extract detailed report data تفاصيل التقارير


| Item | Specification |
| --- | --- |
| Use case ID | UC-ORR-11 |
| Name | Extract detailed report data تفاصيل التقارير |
| Type | Query a report |
| Primary actor | HQ roles |
| Secondary actors | The system (Web API + business layer); the database. |
| Summary | Produces the detailed report extract for a charity, report number, batch and date range — the general-purpose data pull used for analysis and export. |
| Trigger | The actor selects the function from the module menu or the screen that hosts it. |
| Pre-conditions | 1. The actor is authenticated; the JWT access token is valid and the client holds the role, charity and country claims it carries.<br>2. The actor holds one of: HQ roles.<br>3. Caller identity comes from the JWT, never from the request; the application service scopes the query to the charity (and country) that owns the user, unless the role is an HQ role permitted to pass an explicit `charityId` filter. |
| Main flow | 1. The actor reaches the function from the screen of this module that hosts it.<br>2. The actor sets the report criteria (period, charity, country and the other filters the screen offers).<br>3. The actor runs the report.<br>4. The SPA issues `POST /api/OrphanReports/generate` carrying string charityId, int reportNo, bool isCodes, string batchId, DateTime? DateFrom = null, DateTime? DateTo = null.<br>5. `OrphanReportsController` binds the typed request DTO and delegates to the application service.<br>6. `IPeriodicOrphanReportService` validates the payload with its FluentValidation validator, applies the business rules and the charity scope, and persists through `IUnitOfWork`.<br>7. The business layer composes the projection, scoped to the caller’s charity and country.<br>8. The result is rendered in the on-screen grid, from where it can be printed or exported. |
| Alternate flows | • An HQ role (General Director, Financial Director, Staff) may pass an explicit charity id and so read across the charity boundary; a charity user may not and always sees its own data.<br>• No row matches the criteria — the grid renders empty and the paging control reports zero pages. |
| Exception flows | • The session has expired or the role is not permitted — the request is rejected and the SPA routes back to the login state. |
| Post-conditions | • No stored data is changed — the operation is a read. |
| Realisation | `POST /api/OrphanReports/generate` → `OrphanReportsController` → `IOrphanReportService` |

#### 14.U.12  UC-ORR-12 — Extract accepted reports التقارير المعتمدة


| Item | Specification |
| --- | --- |
| Use case ID | UC-ORR-12 |
| Name | Extract accepted reports التقارير المعتمدة |
| Type | Review decision |
| Primary actor | HQ roles |
| Secondary actors | The system (Web API + business layer); the database. |
| Summary | The same detailed extract restricted to accepted reports, used to confirm which orphans are cleared for disbursement. |
| Trigger | The actor selects the function from the module menu or the screen that hosts it. |
| Pre-conditions | 1. The actor is authenticated; the JWT access token is valid and the client holds the role, charity and country claims it carries.<br>2. The actor holds one of: HQ roles.<br>3. Caller identity comes from the JWT, never from the request; the application service scopes the query to the charity (and country) that owns the user, unless the role is an HQ role permitted to pass an explicit `charityId` filter.<br>4. The target record exists and its identifier is known to the screen. |
| Main flow | 1. The actor reaches the function from the screen of this module that hosts it.<br>2. The actor opens the item awaiting a decision and examines its content and attachments.<br>3. The actor records the decision and, when refusing, the reason.<br>4. The actor presses the command button of the screen.<br>5. The SPA issues `GET /api/PeriodicOrphanReports/approved` carrying string charityId, int reportNo, bool isCodes, string batchId, DateTime? DateFrom = null, DateTime? DateTo = null.<br>6. `OrphanReportsController` binds the typed request DTO and delegates to the application service.<br>7. `IPeriodicOrphanReportService` validates the payload with its FluentValidation validator, applies the business rules and the charity scope, and persists through `IUnitOfWork`.<br>8. The business layer writes the new state, the deciding user and the decision date.<br>9. The item leaves the pending queue and becomes visible to the charity in its new state. |
| Alternate flows | • An HQ role (General Director, Financial Director, Staff) may pass an explicit charity id and so read across the charity boundary; a charity user may not and always sees its own data.<br>• The decision is a refusal — the reason is mandatory and is stored with the item so that the charity can see why it was returned. |
| Exception flows | • The session has expired or the role is not permitted — the request is rejected and the SPA routes back to the login state. |
| Post-conditions | • The item carries its new state, the deciding user and the decision date, and moves out of the pending queue. |
| Realisation | `GET /api/PeriodicOrphanReports/approved` → `PeriodicOrphanReportsController` → `IPeriodicOrphanReportService` |

#### 14.U.13  UC-ORR-13 — Extract refused reports التقارير المرفوضة


| Item | Specification |
| --- | --- |
| Use case ID | UC-ORR-13 |
| Name | Extract refused reports التقارير المرفوضة |
| Type | Review decision |
| Primary actor | HQ roles |
| Secondary actors | The system (Web API + business layer); the database. |
| Summary | The detailed extract restricted to refused reports, driving the correction worklist sent back to charities. |
| Trigger | The actor selects the function from the module menu or the screen that hosts it. |
| Pre-conditions | 1. The actor is authenticated; the JWT access token is valid and the client holds the role, charity and country claims it carries.<br>2. The actor holds one of: HQ roles.<br>3. Caller identity comes from the JWT, never from the request; the application service scopes the query to the charity (and country) that owns the user, unless the role is an HQ role permitted to pass an explicit `charityId` filter.<br>4. The target record exists and its identifier is known to the screen. |
| Main flow | 1. The actor reaches the function from the screen of this module that hosts it.<br>2. The actor opens the item awaiting a decision and examines its content and attachments.<br>3. The actor records the decision and, when refusing, the reason.<br>4. The actor presses the command button of the screen.<br>5. The SPA issues `GET /api/PeriodicOrphanReports/rejected` carrying string charityId, int reportNo, bool isCodes, string batchId, DateTime? DateFrom = null, DateTime? DateTo = null.<br>6. `OrphanReportsController` binds the typed request DTO and delegates to the application service.<br>7. `IPeriodicOrphanReportService` validates the payload with its FluentValidation validator, applies the business rules and the charity scope, and persists through `IUnitOfWork`.<br>8. The business layer writes the new state, the deciding user and the decision date.<br>9. The item leaves the pending queue and becomes visible to the charity in its new state. |
| Alternate flows | • An HQ role (General Director, Financial Director, Staff) may pass an explicit charity id and so read across the charity boundary; a charity user may not and always sees its own data.<br>• The decision is a refusal — the reason is mandatory and is stored with the item so that the charity can see why it was returned. |
| Exception flows | • The session has expired or the role is not permitted — the request is rejected and the SPA routes back to the login state. |
| Post-conditions | • The item carries its new state, the deciding user and the decision date, and moves out of the pending queue. |
| Realisation | `GET /api/PeriodicOrphanReports/rejected` → `PeriodicOrphanReportsController` → `IPeriodicOrphanReportService` |

#### 14.U.14  UC-ORR-14 — List orphans with no renewed report الأيتام بدون تقرير مجدد


| Item | Specification |
| --- | --- |
| Use case ID | UC-ORR-14 |
| Name | List orphans with no renewed report الأيتام بدون تقرير مجدد |
| Type | Query a report |
| Primary actor | HQ roles |
| Secondary actors | The system (Web API + business layer); the database. |
| Summary | For a charity and batch, lists the coded orphans that have not submitted a current report, in full, count-only and V2 variants used by different screens. This is the primary chase list before a payment run. |
| Trigger | The actor selects the function from the module menu or the screen that hosts it. |
| Pre-conditions | 1. The actor is authenticated; the JWT access token is valid and the client holds the role, charity and country claims it carries.<br>2. The actor holds one of: HQ roles.<br>3. Caller identity comes from the JWT, never from the request; the application service scopes the query to the charity (and country) that owns the user, unless the role is an HQ role permitted to pass an explicit `charityId` filter. |
| Main flow | 1. The actor reaches the function from the screen of this module that hosts it.<br>2. The actor sets the report criteria (period, charity, country and the other filters the screen offers).<br>3. The actor runs the report.<br>4. The SPA issues `POST /api/Reports/non-renewed-reports` carrying string charityId, string batchId.<br>5. `OrphanReportsController` binds the typed request DTO and delegates to the application service.<br>6. `IPeriodicOrphanReportService` validates the payload with its FluentValidation validator, applies the business rules and the charity scope, and persists through `IUnitOfWork`.<br>7. The business layer composes the projection, scoped to the caller’s charity and country.<br>8. The result is rendered in the on-screen grid, from where it can be printed or exported. |
| Alternate flows | • An HQ role (General Director, Financial Director, Staff) may pass an explicit charity id and so read across the charity boundary; a charity user may not and always sees its own data.<br>• No row matches the criteria — the grid renders empty and the paging control reports zero pages. |
| Exception flows | • The session has expired or the role is not permitted — the request is rejected and the SPA routes back to the login state. |
| Post-conditions | • No stored data is changed — the operation is a read. |
| Realisation | `POST /api/Reports/non-renewed-reports` → `ReportsController` → `IReportService` |

#### 14.U.15  UC-ORR-15 — Extract report numbers added in a period أرقام التقارير المضافة


| Item | Specification |
| --- | --- |
| Use case ID | UC-ORR-15 |
| Name | Extract report numbers added in a period أرقام التقارير المضافة |
| Type | Create a record |
| Primary actor | HQ roles |
| Secondary actors | The system (Web API + business layer); the database. |
| Summary | Lists the report numbers registered for a charity between two dates, optionally restricted to a payment batch — used to reconcile submissions against correspondence. |
| Trigger | The actor selects the function from the module menu or the screen that hosts it. |
| Pre-conditions | 1. The actor is authenticated; the JWT access token is valid and the client holds the role, charity and country claims it carries.<br>2. The actor holds one of: HQ roles.<br>3. Caller identity comes from the JWT, never from the request; the application service scopes the query to the charity (and country) that owns the user, unless the role is an HQ role permitted to pass an explicit `charityId` filter. |
| Main flow | 1. The actor reaches the function from the screen of this module that hosts it. The screen opens in add mode.<br>2. The system loads the reference data the form needs (regions, centres, banks, types and the other lookup lists bound to the drop-downs).<br>3. The actor completes the input fields of the form.<br>4. The actor presses the command button of the screen.<br>5. The SPA issues `POST /api/OrphanReports/statistics` carrying string charityId, DateTime? DateFrom = null, DateTime? DateTo = null , string PaymentId=null.<br>6. `OrphanReportsController` binds the typed request DTO and delegates to the application service.<br>7. `IPeriodicOrphanReportService` validates the payload with its FluentValidation validator, applies the business rules and the charity scope, and persists through `IUnitOfWork`.<br>8. The business layer validates the payload, stamps the owning charity and the creating user, and persists the new record.<br>9. The system returns the outcome and the SPA confirms the save and returns to the list screen. |
| Alternate flows | • An HQ role (General Director, Financial Director, Staff) may pass an explicit charity id and so read across the charity boundary; a charity user may not and always sees its own data.<br>• The actor abandons the form before saving — nothing is written and the record keeps its previous state. |
| Exception flows | • The session has expired or the role is not permitted — the request is rejected and the SPA routes back to the login state.<br>• A mandatory field is empty or fails its format check — the save is refused and the field is flagged on the form. |
| Post-conditions | • A new record exists, owned by the charity of the creating user, and appears in the list screen of the module. |
| Realisation | `POST /api/OrphanReports/statistics` → `OrphanReportsController` → `IOrphanReportService` |

#### 14.U.16  UC-ORR-16 — View report attachments صور اليتيم والشهادات


| Item | Specification |
| --- | --- |
| Use case ID | UC-ORR-16 |
| Name | View report attachments صور اليتيم والشهادات |
| Type | Query a report |
| Primary actor | Charity, HQ roles |
| Secondary actors | The system (Web API + business layer); the database. |
| Summary | Returns the identifiers of the orphan photographs and of the certificate/document images attached to a report, which the client then streams from the file service. |
| Trigger | The actor selects the function from the module menu or the screen that hosts it. |
| Pre-conditions | 1. The actor is authenticated; the JWT access token is valid and the client holds the role, charity and country claims it carries.<br>2. The actor holds one of: charity, HQ roles. |
| Main flow | 1. The actor reaches the function from the screen of this module that hosts it.<br>2. The actor sets the report criteria (period, charity, country and the other filters the screen offers).<br>3. The actor runs the report.<br>4. The SPA issues `GET /api/Attachments/{id}/image` carrying Id.<br>5. `PeriodicOrphanReportsController` binds the typed request DTO and delegates to the application service.<br>6. `IPeriodicOrphanReportService` validates the payload with its FluentValidation validator, applies the business rules and the charity scope, and persists through `IUnitOfWork`.<br>7. The business layer composes the projection, scoped to the caller’s charity and country.<br>8. The result is rendered in the on-screen grid, from where it can be printed or exported. |
| Alternate flows | • No row matches the criteria — the grid renders empty and the paging control reports zero pages. |
| Exception flows | • The session has expired or the role is not permitted — the request is rejected and the SPA routes back to the login state. |
| Post-conditions | • No stored data is changed — the operation is a read. |
| Realisation | `GET /api/Attachments/{id}/image` → `AttachmentsController` → `IAttachmentService` |

#### 14.U.17  UC-ORR-17 — Print the periodic report form طباعة التقرير الدوري


| Item | Specification |
| --- | --- |
| Use case ID | UC-ORR-17 |
| Name | Print the periodic report form طباعة التقرير الدوري |
| Type | Print / produce a document |
| Primary actor | Charity, HQ roles |
| Secondary actors | The system (Web API + business layer); the database. the report and export endpoints of `api/Reports` as the rendering path — jsPDF for PDF and ExcelJS for Excel. |
| Summary | Renders the official orphan report form as PDF. The system selects the layout that matches the case — studying / not studying / disabled, and which of the marriage, death, medical and certificate documents are present — from the 24 available templates. |
| Trigger | The actor selects the function from the module menu or the screen that hosts it. |
| Pre-conditions | 1. The actor is authenticated; the JWT access token is valid and the client holds the role, charity and country claims it carries.<br>2. The actor holds one of: charity, HQ roles.<br>3. Caller identity comes from the JWT, never from the request; the application service scopes the query to the charity (and country) that owns the user, unless the role is an HQ role permitted to pass an explicit `charityId` filter.<br>4. The target record exists and its identifier is known to the screen. |
| Main flow | 1. The actor reaches the function from the screen of this module that hosts it.<br>2. The actor selects the record or the range to be printed.<br>3. The actor presses the command button of the screen.<br>4. The browser opens `POST /api/Reports/orphan-report-form/export/pdf` with string reportId, string userId.<br>5. `ReportsController` resolves the report key, applies the filter, and renders the document from the dataset the application service returns.<br>6. The rendered document is streamed back as a PDF for viewing and printing. |
| Alternate flows | • An HQ role (General Director, Financial Director, Staff) may pass an explicit charity id and so read across the charity boundary; a charity user may not and always sees its own data. |
| Exception flows | • The session has expired or the role is not permitted — the request is rejected and the SPA routes back to the login state.<br>• The report definition or a required parameter is missing — the print action fails and no document is produced. |
| Post-conditions | • A printable document has been produced. Where the module records printing, the row is flagged as printed. |
| Realisation | `POST /api/Reports/orphan-report-form/export/pdf` → `ReportsController` → `IReportService` |

### 14.A  Annex - Angular routes of this module

Routes are hash-based (`useHash: true`), rendered inside `MainLayoutComponent` behind
`AuthGuard` and `PermissionGuard`. *implemented* means the route exists in
`Frontend/src/app`; *planned* means it is specified here and not yet built.

| Angular route | Feature module | Component | Status |
| --- | --- | --- | --- |
| `#/periodic-orphan-reports` | `periodic-orphan-reports` | `PeriodicReportsListComponent` | built, not reachable |
| `#/periodic-orphan-reports/create` | `periodic-orphan-reports` | `PeriodicReportFormComponent` | built, not reachable |
| `#/periodic-orphan-reports/:id` | `periodic-orphan-reports` | `PeriodicReportDetailComponent` | built, not reachable |
| `#/periodic-orphan-reports/:id/edit` | `periodic-orphan-reports` | `PeriodicReportFormComponent` | built, not reachable |
| `#/periodic-orphan-reports/:id/review` | `periodic-orphan-reports` | `PeriodicReportReviewComponent` | built, not reachable |
| `#/periodic-orphan-reports/orphan-reports` | `periodic-orphan-reports` | `OrphanReportsListComponent` | built, not reachable |
| `#/periodic-orphan-reports/orphan-reports/generate` | `periodic-orphan-reports` | `OrphanReportsGenerateComponent` | built, not reachable |
| `#/periodic-orphan-reports/orphan-reports/history` | `periodic-orphan-reports` | `OrphanReportHistoryComponent` | built, not reachable |
| `#/periodic-orphan-reports/orphan-reports/compare` | `periodic-orphan-reports` | `OrphanReportComparisonComponent` | built, not reachable |
| `#/periodic-orphan-reports/orphan-reports/schedule` | `periodic-orphan-reports` | `ScheduleReportComponent` | built, not reachable |
| `#/periodic-orphan-reports/orphan-reports/search` | `periodic-orphan-reports` | `OrphanReportSearchComponent` | built, not reachable |
| `#/periodic-orphan-reports/orphan-reports/orphan/:orphanId` | `periodic-orphan-reports` | `OrphanReportSearchComponent` | built, not reachable |

> **These routes are not reachable in the running client.** `PeriodicOrphanReportsRoutingModule`
> declares them, but `app-routing.module.ts` has no `periodic-orphan-reports` entry, so nothing
> lazy-loads the feature module. Registering that entry is the first task of this chapter.

### 14.B  Annex - API controllers of this module

Every controller inherits `ApiController` (`[Authorize]`, route `api/[controller]`) and
returns `Framework.Core.ApiResponse<T>`. Business rules live in the application service
behind each controller, never in the controller itself.

| Controller | Base route | Responsibility |
| --- | --- | --- |
| `PeriodicOrphanReportsController` | `api/PeriodicOrphanReports` | Report create/read/update/delete, code lookup, attachments, payment-orphan and exclusion reports. Report history per orphan and per housing beneficiary. Refined orphan-status query. |
| `OrphanReportsController` | `api/OrphanReports` | Detailed extracts: all, accepted, refused, non-renewed, report numbers, Meza cards, widows. |

---

Shared context - system architecture, actors and roles, the sponsorship lifecycle, the full module map and all appendices - is in the companion document [00-Overview-and-Common-Context](00-Overview-and-Common-Context.md). The delivery backlog for this module is epic EP-09 in [00-EPICS-and-User-Stories](00-EPICS-and-User-Stories.md).

