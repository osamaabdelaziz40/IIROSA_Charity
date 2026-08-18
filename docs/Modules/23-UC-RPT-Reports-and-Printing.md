# WAR.IIROSA - Reports & Printing

التقارير والطباعة | use case prefix `UC-RPT` | chapter 23 of the master document


| Field | Value |
| --- | --- |
| Document title | WAR.IIROSA - Module Specification: Reports & Printing |
| Module | Reports & Printing - التقارير والطباعة |
| Use case prefix | UC-RPT |
| Chapter in master document | Chapter 23 |
| Documented use cases | 41 |
| Principal routes | `#/reports/**` (see Appendix C for the full report-key index) |
| Version | 1.1 |
| Status | Chapter content extracted verbatim; screen fields and scenarios derived from the source code |
| Date | 18 August 2026 |
| Parent document | WAR.IIROSA-Project-Documentation.docx v1.0 (repository WAR.IIROSA, commit 4beb736) |

## Contents of this document

1. Chapter 23 — module purpose and use-case catalogue (verbatim from the master document)
2. §23.S — screen field specifications (every field of every screen, derived from the AngularJS views)
3. §23.U — one expanded scenario per use case (trigger, pre-conditions, main flow, alternates, exceptions, post-conditions)
4. §23.A / §23.B — annexes: screens and Web API controllers of this module

## 23. Reports & Printing

التقارير والطباعة — the reporting layer is the largest single area of the system. The report catalogue is served by `api/Reports`: one report key per report, each rendered on screen by `ReportViewerComponent` and exported through `…/export/pdf` and `…/export/excel`. Reports fall into three families: beneficiary reports, compliance / chase reports used by HQ to supervise charities, and financial reports.

### 23.1 Beneficiary reports


| ID | Report | Primary actor | Purpose & parameters | Realisation |
| --- | --- | --- | --- | --- |
| UC-RPT-01 | Orphan data بيانات الأيتام | All roles | The master orphan listing. Filtered by charity, payment batch, governorate, centre, and the flags all orphans, excluded and warranty required. Used both for review and as the source of orphan extracts. | Route `#/reports/orphans` → POST /api/Reports/orphans |
| UC-RPT-02 | Orphan status حالة اليتيم | All roles | Refined orphan-status query driven by the multi-field status contract (charity, batch, dates, code list, approval state). | Route `#/periodic-orphan-reports/orphan-reports/search` → POST /api/PeriodicOrphanReports/{id}/review |
| UC-RPT-03 | Excluded orphans المستبعدون | All roles | Orphans removed from, or requested to be removed from, the sponsorship programme for a charity. | Route `#/reports/excluded-orphans` → POST /api/Reports/excluded-orphans |
| UC-RPT-04 | Orphans with ended sponsorship أيتام انتهت كفالتهم | Gen. Director | Orphans whose sponsorship term has expired and who therefore need re-sponsoring or closure. | Route `#/reports/finished-sponsorship-orphans` → POST /api/Reports/finished-sponsorship-orphans |
| UC-RPT-05 | Unsponsored orphans أيتام غير مكفولين | Gen. Director | Registered orphans who currently have no sponsor attached — the pipeline for new sponsorships. | Route `#/reports/unsponsored-orphans` → POST /api/Reports/unsponsored-orphans |
| UC-RPT-06 | Widows requiring sponsorship أرامل مطلوب لهم كفالة | All roles | Widowed guardians who are eligible for, and have consented to, widow sponsorship for a charity, report number and batch. | Route `#/reports/widows-allowing-sponsorship` → POST /api/Reports/widows-allowing-sponsorship |
| UC-RPT-07 | Registered Meza cards تقرير الكروت المسجلة | Gen. Director | Guardians with a registered payment card, confirming that the disbursement channel exists before a bank transfer batch is built. | Route `#/reports/meza-cards` → POST /api/Reports/meza-cards |
| UC-RPT-08 | Extract guardian Meza cards استخراج كروت العائل | Gen. Director, Fin. Director | The card-number extract for a charity, report number, batch and date range, with a switch for whether a card already exists — the input to card issuance and to bank file preparation. | POST /api/Reports/meza-cards |
| UC-RPT-09 | Assistance family data بيانات أسر المساعدات | All roles | Detailed listing of the families benefiting from assistance for a charity. | Route `#/reports/beneficiary-family-details` → POST /api/Reports/beneficiary-family-details |
| UC-RPT-10 | Beneficiary statistics احصائيات المستفيدين | Gen. Director | Aggregate beneficiary counts across charities and programmes, for management reporting. | Route `#/seasonal-aid/:id/report` |
| UC-RPT-11 | Family projects مشاريع الأسر | Gen. Director | The projects each family has been registered for in a charity. | Route `#/reports/registered-family-projects` → POST /api/Reports/registered-family-projects |
| UC-RPT-12 | Guardian change history تقارير تعديل المعيل | All roles | Old and new guardian pairs for a charity, evidencing every change of the responsible adult on a family file. | Route `#/reports/provider-sponsor-changes` → POST /api/Reports/provider-sponsor-changes |
| UC-RPT-13 | Family and orphan entry tracking متابعة إدخالات الأسر والأيتام | All roles | What each charity entered on a given date, used to monitor data-entry activity. | Route `#/reports/family-orphans` → GET /api/Families/{id}/follow-up |
| UC-RPT-14 | General orphan statistics احصائيات عامة للأيتام | Gen. Director | Counts of orphan reports grouped by status across the selected scope. | Route `#/periodic-orphan-reports/orphan-reports` |

### 23.2 Compliance and chase reports


| ID | Report | Primary actor | Purpose & parameters | Realisation |
| --- | --- | --- | --- | --- |
| UC-RPT-15 | Coded orphans needing a report أيتام مكودون مطلوب لهم تقرير | Gen. Director | Coded orphans in a charity with no current periodic report — the primary chase list before a payment run. | Route `#/reports/orphans-missing-reports` → POST /api/Reports/orphans-missing-reports |
| UC-RPT-16 | Orphans missing files أيتام مطلوب لهم ملفات | Gen. Director | Orphans whose supporting document files are absent from storage, so the charity can be asked to upload them. | Route `#/reports/orphans-missing-files` → POST /api/Reports/orphans-missing-files |
| UC-RPT-17 | Reports awaiting approval تقارير في انتظار الموافقة | Gen. Director | The HQ review queue: submitted reports that are neither accepted nor refused. | Route `#/reports/reports-awaiting-approval` → POST /api/Reports/reports-awaiting-approval |
| UC-RPT-18 | Refused reports تقارير تم رفضها | All roles | Reports rejected by HQ with their refusal reasons, forming the charity's correction worklist. | Route `#/reports/refused-reports` → POST /api/Reports/refused-reports |
| UC-RPT-19 | Orphans without a renewed report أيتام بدون تقرير مجدد | Gen. Director | Per charity and batch, the orphans whose report has not been renewed for the current cycle, in detail, V2 and count-only variants. | POST /api/Reports/non-renewed-reports (+ V2, _Number, GetBeginingScreen) |
| UC-RPT-20 | Charity follow-up متابعة الجمعيات | Gen. Director | Cross-charity tracking of a payment batch: what each charity has entered, printed, disbursed and confirmed. The primary supervisory dashboard. | Route `#/reports/charity-payment-tracking` → POST /api/Reports/charity-payment-tracking; printed via /api/Reports/charity-payment-tracking/export/pdf |
| UC-RPT-21 | Family update tracking متابعة تحديث بيانات الأسر | Gen. Director | Which family files a charity refreshed around a payment date, printed as a monitoring sheet. | /api/Reports/family-update-tracking/export/pdf |
| UC-RPT-22 | Missed payments — current user scope أيتام مستحقون دفعات سابقة | Gen. Director, charity | Orphans entitled to earlier batches who did not receive them, so arrears can be settled. | Route `#/reports/missed-payments` → POST /api/Reports/missed-payments |
| UC-RPT-23 | Missed payments — all جميع الدفعات الفائتة | Gen. Director | The organisation-wide arrears view across all charities. | POST /api/Reports/missed-payments |
| UC-RPT-24 | Export orphan photographs صور الأيتام | All roles | For a date range and charity, exports the orphan photographs attached to accepted reports, for despatch to sponsors. | Route `#/reports/orphan-files` → POST /api/Reports/orphan-files/export |
| UC-RPT-25 | Export certificate images صور الشهادات | All roles | The same export restricted to certificate and supporting-document images. | POST /api/Reports/certificate-files/export |
| UC-RPT-26 | Print the survey questionnaire طباعة الاستبانة | All roles | Prints the blank field questionnaire used when surveying a household, and the widow-specific variant. | Printing() / WidowPrinting() in index.html |

### 23.3 Financial reports


| ID | Report | Primary actor | Purpose & parameters | Realisation |
| --- | --- | --- | --- | --- |
| UC-RPT-27 | Orphans in a payment batch أيتام الدفعة | HQ roles, charity | All orphans included in a batch for a charity, with amounts and payment state. | GET /api/OrphanPayments/{id}/details |
| UC-RPT-28 | Orphans receiving nothing أيتام لم يصرف لهم | HQ roles | Orphans in a batch for whom no amount was disbursed, exposing gaps in the run. | POST /api/Reports/orphans-without-payment |
| UC-RPT-29 | Received / not received / stopped lists | HQ roles, charity | The three disbursement outcome lists for a batch and charity (see UC-PAY-18 to UC-PAY-20), available on screen and as printed sheets. | /api/Reports/payments-received/export/pdf, PrintNonRecieved, PrintStopped |
| UC-RPT-30 | Cheque numbers list أرقام الشيكات | Charity, Fin. Director | The cheque numbers issued for a batch in a charity, for handover and reconciliation. | /api/Reports/cheque-numbers/export/pdf |
| UC-RPT-31 | Receipt cards كروت الاستلام | Charity | The individually printed cards each guardian signs when collecting the payment. | /api/Reports/receipt-cards/export/pdf; rptTickets.rpt, rptTicketsFam.rpt |
| UC-RPT-32 | Payment summary pages صفحات ملخص الدفعة | HQ roles | Cover and summary sheets of a batch for a charity. | GET /api/Dashboard/payment-summary; rptNFirst.rpt, rptNFirstChqNo.rpt |
| UC-RPT-33 | Cheque statement report بيان الشيكات | Fin. Director | Cheques filtered by bank, date range and type, rendered as the bank reconciliation statement. | /api/Reports/cheque-statement/export/pdf; rptChequeBayan.rpt |
| UC-RPT-34 | Project distribution sheets كشوف توزيع المشاريع | Charity, HQ roles | Family cards and the primary/secondary distribution lists for an assistance project. | rptFamilyDist.rpt, rptFamilyDistV2.rpt, /api/Reports/family-cards/export/pdf |
| UC-RPT-35 | New orphans and new widows الأيتام والأرامل الجدد | HQ roles | Printed lists of newly registered orphans and widows for a charity, used in sponsorship offers. | rptNewOrphans.rpt, rptNewOrphansV2.rpt, rptNewWidows.rpt, rptNewWidows_Family.rpt |
| UC-RPT-36 | Follow-up and handover sheets كشوف المتابعة والتسليم | HQ roles | The general follow-up sheet, the family follow-up sheet and the handover (delivery) sheet. | rptFollowUp.rpt, rptFollowUpFamily.rpt, rptFollowUpTasleem.rpt |
| UC-RPT-37 | Guardian and widow identification sheets كشوف تعريف العائل والأرامل | HQ roles, charity | Identification sheets for a charity at a date — for all guardians, for widows, or for a single family. | /api/Reports/guardian-identification-sheets/export/pdf, …ForWidows, …ForWidows_Family |
| UC-RPT-38 | Missing outgoing attachments مرفقات الصادر الناقصة | Staff | Orphan reports expected on an outgoing letter but not attached. | rptChildOutGoingMissing.rpt |
| UC-RPT-39 | Family orphan list by date أيتام الأسر بتاريخ | HQ roles | Prints the orphans of a charity's families as at a given date. | /api/Reports/family-orphans/export/pdf |
| UC-RPT-40 | Display a report in the browser عرض التقرير | All roles | Streams a generated PDF into an inline frame so the user can review before printing or saving. | /api/Reports/{reportKey}/export/pdf, /api/Reports/{reportKey}/export/pdf |
| UC-RPT-41 | Export a report to Excel تصدير إلى إكسل | All roles | Report grids are exported to spreadsheet format for offline analysis; server-side generation uses EPPlus, client-side export uses the bundled JSZip/FileSaver libraries. | EPPlus, Scripts/FileSaver.js, node_modules/jszip |

### 23.S  Screen field specifications

Derived from the AngularJS views of this module. For every screen the table lists each data-entry field in the order it appears, the scope property it binds to, its control type, whether the form marks it mandatory, and the lookup or rule that governs it. Labels are reproduced as they appear on screen (Arabic, right-to-left).

#### 23.S.1  Screen `#/periodic-orphan-reports/orphan-reports/search`


| Property | Value |
| --- | --- |
| Angular route | `#/periodic-orphan-reports/orphan-reports/search` |
| Feature module | `periodic-orphan-reports` (lazy-loaded) |
| Component | `OrphanReportSearchComponent` |
| Route status | implemented |
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

#### 23.S.2  Screen `#/seasonal-aid/:id/report`


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

#### 23.S.3  Screen `#/reports/orphans`


| Property | Value |
| --- | --- |
| Angular route | `#/reports/orphans` |
| Feature module | `reports` (lazy-loaded) |
| Component | `ReportViewerComponent` |
| Route status | planned |
| Data-entry fields | 10 |
| Grids on the screen | 1 |
| Commands | 9 |


| Section | Field (as labelled) | Bound to | Control | Mandatory · source · rules |
| --- | --- | --- | --- | --- |
| Filter | الدفعة المالية المنصرقة للايتام | BNumberFilter | Drop-down list | Optional · options: lookup: Batches (+ كل الدفعات) |
| Filter | الجمعية | CharityFilterDto | Drop-down list | Optional · options: lookup: Charities (+ كافة الجهات) |
| Filter | المركز | CenterFilter | Drop-down list | Optional · options: lookup: Centers (+ كل) |
| Filter | المحافظة | GovernorateFilter | Drop-down list | Optional · options: lookup: Regions (+ كل) · on change: GetCenters() |
| Filter | الى | AgeTo | Numeric box | Optional · min 1 |
| Filter | من | AgeFrom | Numeric box | Optional · min 1 |
| Filter | العمر | IsFinishedSponsorship | Check box | Optional |
| Filter | (unlabelled) | NotExecluded | Check box | Optional |
| Filter | المستبعدين | Excluded | Check box | Optional · on change: DisablrAllOrphan() |
| Filter | (unlabelled) | AllOrphans | Check box | Optional · on change: Disablrexcluded() |

Grids on this screen:

| Grid (row source) | Columns |
| --- | --- |
| Orphan in Orphans track by $index | رقم اليتيم · أسم اليتيم · أسم المعيل · صلة القرابة · المحافظة · المركز · القرية/الحى · العنوان التفصيلى · ت الموبايل · ت الموبايل2 · ملكية السكن · قيمة الايجار · نوع السكن · حالة مستويات السكن · قيمة الدخل · الرقم القومى · تاريخ الميلاد · العمر · النوع · الحالة الاجتماعية · الحالة الصحية · نوعية العمل · المرحلة الدراسية · الصف الدراسى · اسم الموسسة التعليمية · الكلية · القسم · حاصل على موهل دراسى · تاريخ وفاة الاب · سبب وفاة الاب · مشروع تنموى للمعيل · مؤهل المعيل · مهنة المعيل · الحالة الصحية للمعيل · الحالة الإجتماعية للمعيل · الرقم القومى للمعيل · الاستبعاد · سبب الاستبعاد · الملاحظات · أسم الجمعية · تاريخ اخر تحديث |

Commands on this screen:

| Command | Handler | Shown when |
| --- | --- | --- |
| بحث | GetData() | always |
| بيانات الأيتام وأسرهم المطلوب كفالتهم | GetOrphanThatRequiredWarranty() | always |
| كروت التسليم | PrintRecieveCards() | always |
| بيانات الأيتام - معيل آخر | GetOrphanOtherSponser() | IsAdmin |
| الأيتام المطلوب إرسال تقاريرهم للهيئة | NewActionForWaleed() | always |
| متابعة الايتام - لم يتم الصرف | GetRecievedPaymentDetails() | always |
| تقرير بيانات غير المستلمين | GetGotItNotPaymentDetails() | always |
| أيتام لم تصل لهم أي مبالغ | OrphnasDontTakeAnyAmount() | always |
| استخراج البيانات | ExportData() | always |

#### 23.S.4  Screen `#/reports/excluded-orphans`


| Property | Value |
| --- | --- |
| Angular route | `#/reports/excluded-orphans` |
| Feature module | `reports` (lazy-loaded) |
| Component | `ReportViewerComponent` |
| Route status | planned |
| Data-entry fields | 1 |
| Grids on the screen | 1 |
| Commands | 2 |


| Section | Field (as labelled) | Bound to | Control | Mandatory · source · rules |
| --- | --- | --- | --- | --- |
| الايتام المستبعدين | الجمعية | CharityFilterDto | Drop-down list | Optional · options: lookup: Charities (+ كافة الجهات) |

Grids on this screen:

| Grid (row source) | Columns |
| --- | --- |
| Orphan in Orphans track by $index | رقم اليتيم · أسم اليتيم · الاستبعاد · سبب الاستبعاد · أسم الجمعية · تاريخ اخر تحديث |

Commands on this screen:

| Command | Handler | Shown when |
| --- | --- | --- |
| بحث | GetData() | always |
| استخراج البيانات | ExportData() | always |

#### 23.S.5  Screen `#/reports/finished-sponsorship-orphans`


| Property | Value |
| --- | --- |
| Angular route | `#/reports/finished-sponsorship-orphans` |
| Feature module | `reports` (lazy-loaded) |
| Component | `ReportViewerComponent` |
| Route status | planned |
| Data-entry fields | 1 |
| Grids on the screen | 1 |
| Commands | 2 |


| Section | Field (as labelled) | Bound to | Control | Mandatory · source · rules |
| --- | --- | --- | --- | --- |
| الايتام المنتهي كفالتهم | الجمعية | CharityFilterDto | Drop-down list | Optional · options: lookup: Charities (+ كافة الجهات) |

Grids on this screen:

| Grid (row source) | Columns |
| --- | --- |
| Orphan in Orphans track by $index | رقم اليتيم · أسم اليتيم · الجمعيه · كود العائله · العمر |

Commands on this screen:

| Command | Handler | Shown when |
| --- | --- | --- |
| بحث | GetData() | always |
| استخراج البيانات | ExportData() | always |

#### 23.S.6  Screen `#/reports/unsponsored-orphans`


| Property | Value |
| --- | --- |
| Angular route | `#/reports/unsponsored-orphans` |
| Feature module | `reports` (lazy-loaded) |
| Component | `ReportViewerComponent` |
| Route status | planned |
| Data-entry fields | 1 |
| Grids on the screen | 1 |
| Commands | 2 |


| Section | Field (as labelled) | Bound to | Control | Mandatory · source · rules |
| --- | --- | --- | --- | --- |
| الايتام غير مكفولين | الجمعية | CharityFilterDto | Drop-down list | Optional · options: lookup: Charities (+ كافة الجهات) |

Grids on this screen:

| Grid (row source) | Columns |
| --- | --- |
| Orphan in Orphans track by $index | رقم اليتيم · أسم اليتيم · الجمعيه · كود العائله · العمر |

Commands on this screen:

| Command | Handler | Shown when |
| --- | --- | --- |
| بحث | GetData() | always |
| استخراج البيانات | ExportData() | always |

#### 23.S.7  Screen `#/reports/registered-family-projects`


| Property | Value |
| --- | --- |
| Angular route | `#/reports/registered-family-projects` |
| Feature module | `reports` (lazy-loaded) |
| Component | `ReportViewerComponent` |
| Route status | planned |
| Data-entry fields | 1 |
| Grids on the screen | 1 |
| Commands | 2 |


| Section | Field (as labelled) | Bound to | Control | Mandatory · source · rules |
| --- | --- | --- | --- | --- |
| تقرير مشاريع الأسر | الجمعية | CharityFilterDto | Drop-down list | Optional · options: lookup: Charities (+ كافة الجهات) |

Grids on this screen:

| Grid (row source) | Columns |
| --- | --- |
| Orphan in FamilisProject track by $index | أسماء الأيتام · الرقم القومي · اسم المعيل · أكواد الأيتام · كود العائله · التليفون · الجمعيه · حاله المشروع · عدد سنوات الخبره · الميزانيه · تاريخ بدايه المشروع · عنوان المشروع · هل يوجد خبره · وصف المشروع |

Commands on this screen:

| Command | Handler | Shown when |
| --- | --- | --- |
| بحث | GetData() | always |
| استخراج البيانات | ExportData() | always |

#### 23.S.8  Screen `#/reports/meza-cards`


| Property | Value |
| --- | --- |
| Angular route | `#/reports/meza-cards` |
| Feature module | `reports` (lazy-loaded) |
| Component | `ReportViewerComponent` |
| Route status | planned |
| Data-entry fields | 1 |
| Grids on the screen | 1 |
| Commands | 2 |


| Section | Field (as labelled) | Bound to | Control | Mandatory · source · rules |
| --- | --- | --- | --- | --- |
| تقرير الكروت المسجله | الجمعية | CharityFilterDto | Drop-down list | Optional · options: lookup: Charities (+ كافة الجهات) |

Grids on this screen:

| Grid (row source) | Columns |
| --- | --- |
| Orphan in FamilisProject track by $index | أسماء الأيتام · الرقم القومي · اسم المعيل · أكواد الأيتام · كود العائله · التليفون · الجمعيه · كارت ميزا · تاريخ انتهاء كارت ميزا |

Commands on this screen:

| Command | Handler | Shown when |
| --- | --- | --- |
| بحث | GetData() | always |
| استخراج البيانات | ExportData() | always |

#### 23.S.9  Screen `#/reports/widows-allowing-sponsorship`


| Property | Value |
| --- | --- |
| Angular route | `#/reports/widows-allowing-sponsorship` |
| Feature module | `reports` (lazy-loaded) |
| Component | `ReportViewerComponent` |
| Route status | planned |
| Data-entry fields | 1 |
| Grids on the screen | 1 |
| Commands | 1 |


| Section | Field (as labelled) | Bound to | Control | Mandatory · source · rules |
| --- | --- | --- | --- | --- |
| ارامل مطلوب لهم كفاله | الجمعية | CharityFilterDto | Drop-down list | Optional · options: lookup: Charities (+ كافة الجهات) |

Grids on this screen:

| Grid (row source) | Columns |
| --- | --- |
| Orphan in AllWidows track by $index | تاريخ اخر تحديث · اسم الجمعيه · ملاحظات · الرقم القومي · الحاله الصحيه للارمله · مهنه الارمله · مؤهل الارمله · مشروع تنموي · تاريخ وفاه الزوج · قيمه الدخل · حاله السكن · نوع السكن · قيمه الايجار · ملكيه السكن · الموبايل 1 · الموبايل 2 · العنوان تفصيلي · القريه · المركز · المحافظه · اسم الارمله |

Commands on this screen:

| Command | Handler | Shown when |
| --- | --- | --- |
| بحث | ExtractWidowsThatAllowWidowSponsorship() | always |

#### 23.S.10  Screen `#/reports/family-orphans`


| Property | Value |
| --- | --- |
| Angular route | `#/reports/family-orphans` |
| Feature module | `reports` (lazy-loaded) |
| Component | `ReportViewerComponent` |
| Route status | planned |
| Data-entry fields | 2 |
| Grids on the screen | 0 |
| Commands | 3 |


| Section | Field (as labelled) | Bound to | Control | Mandatory · source · rules |
| --- | --- | --- | --- | --- |
| Filter | تاريخ بدء التقرير | Date | Date picker | Optional · on change: ParentBDate.date=dt.toISOString() |
| Filter | الجمعية | CharityFilterDto | Drop-down list | Optional · options: lookup: Charities (+ كافة الجهات) |

Commands on this screen:

| Command | Handler | Shown when |
| --- | --- | --- |
| طباعة الاستبانة من تاريخ محدد | PrintCharityIdentifications() | always |
| تفاصيل إدخالات الأسر الجديدة | GetFamilyOrphansDetails() | always |
| إجماليات إدخالات الأسر والأيتام | GetFamilyOrphans() | always |

#### 23.S.11  Screen `#/reports/missed-payments`


| Property | Value |
| --- | --- |
| Angular route | `#/reports/missed-payments` |
| Feature module | `reports` (lazy-loaded) |
| Component | `ReportViewerComponent` |
| Route status | planned |
| Data-entry fields | 1 |
| Grids on the screen | 1 |
| Commands | 2 |


| Section | Field (as labelled) | Bound to | Control | Mandatory · source · rules |
| --- | --- | --- | --- | --- |
| — | الجمعية | CharityId | Drop-down list | Optional · options: lookup: Charities (+ كافة الجهات) · on change: getCharityData() |

Grids on this screen:

| Grid (row source) | Columns |
| --- | --- |
| missedPay in missedPaymentsList track by $index | الكود · اسم الجمعية · اسم اليتيم · سبب طلب الاستعداد · 1 · 2 · 3 · 4 · 5 · 6 · 7 · 8 · 9 · 10 · 11 · 12 · 13 · 14 · 15 · 16 · 17 |

Commands on this screen:

| Command | Handler | Shown when |
| --- | --- | --- |
| استخراج البيانات | ExportData() | always |
| (icon only) | GetDataByAllOrphanCheckBox() | always |

#### 23.S.12  Screen `#/reports/charity-payment-tracking`


| Property | Value |
| --- | --- |
| Angular route | `#/reports/charity-payment-tracking` |
| Feature module | `reports` (lazy-loaded) |
| Component | `ReportViewerComponent` |
| Route status | planned |
| Data-entry fields | 3 |
| Grids on the screen | 0 |
| Commands | 4 |


| Section | Field (as labelled) | Bound to | Control | Mandatory · source · rules |
| --- | --- | --- | --- | --- |
| — | الجمعية | CharityId | Drop-down list | Optional · options: lookup: Charities (+ كافة الجهات) · on change: getCharityData() |
| — | الدفعة المالية المنصرقة للايتام | BNumberFilter | Drop-down list | Optional · options: lookup: Batches |
| تحديثات الجمعيات | من فضلك ادخل تاريخ بدا التحديث | DateOfStartingUpdate | Date picker | Optional |

Commands on this screen:

| Command | Handler | Shown when |
| --- | --- | --- |
| متابعة تحديثات الجميعات | FamilyUpdateTracking() | always |
| متابعة تسليمات الجميعات | printCharityPaymentTracking() | always |
| غلق | CloseFamilyUpdateTrackingModal() | always |
| تم | printFamilyUpdateTracking() | always |

#### 23.S.13  Screen `#/reports/beneficiary-family-details`


| Property | Value |
| --- | --- |
| Angular route | `#/reports/beneficiary-family-details` |
| Feature module | `reports` (lazy-loaded) |
| Component | `ReportViewerComponent` |
| Route status | planned |
| Data-entry fields | 1 |
| Grids on the screen | 0 |
| Commands | 1 |


| Section | Field (as labelled) | Bound to | Control | Mandatory · source · rules |
| --- | --- | --- | --- | --- |
| — | الجمعية | CharityId | Drop-down list | Optional · options: lookup: Charities (+ كافة الجهات) · on change: getCharityData() |

Commands on this screen:

| Command | Handler | Shown when |
| --- | --- | --- |
| استخراج البيانات | ExportData() | always |

#### 23.S.14  Screen `#/reports/orphans-missing-reports`


| Property | Value |
| --- | --- |
| Angular route | `#/reports/orphans-missing-reports` |
| Feature module | `reports` (lazy-loaded) |
| Component | `ReportViewerComponent` |
| Route status | planned |
| Data-entry fields | 1 |
| Grids on the screen | 1 |
| Commands | 4 |


| Section | Field (as labelled) | Bound to | Control | Mandatory · source · rules |
| --- | --- | --- | --- | --- |
| — | الجمعية | CharityId | Drop-down list | Optional · options: lookup: Charities (+ كافة الجهات) · on change: getCharityData() |

Grids on this screen:

| Grid (row source) | Columns |
| --- | --- |
| field in AllCharities   track by $index | الجمعيه · عدد الايتام مطلوب لهم تقارير |

Commands on this screen:

| Command | Handler | Shown when |
| --- | --- | --- |
| حفظ | DeleteOutgoing() | always |
| (icon only) | ExtractDetails(field.Id) | always |
| (icon only) | GetNext() | always |
| (icon only) | GetPrev() | always |

#### 23.S.15  Screen `#/reports/orphans-missing-files`


| Property | Value |
| --- | --- |
| Angular route | `#/reports/orphans-missing-files` |
| Feature module | `reports` (lazy-loaded) |
| Component | `ReportViewerComponent` |
| Route status | planned |
| Data-entry fields | 1 |
| Grids on the screen | 1 |
| Commands | 4 |


| Section | Field (as labelled) | Bound to | Control | Mandatory · source · rules |
| --- | --- | --- | --- | --- |
| — | الجمعية | CharityId | Drop-down list | Optional · options: lookup: Charities (+ كافة الجهات) · on change: getCharityData() |

Grids on this screen:

| Grid (row source) | Columns |
| --- | --- |
| field in All_Orphans   track by $index | الجمعيه · رقم اليتيم · اسم اليتيم · العنوان · القريه |

Commands on this screen:

| Command | Handler | Shown when |
| --- | --- | --- |
| حفظ | DeleteOutgoing() | always |
| (icon only) | ExportReportData() | always |
| (icon only) | GetNext() | always |
| (icon only) | GetPrev() | always |

#### 23.S.16  Screen `#/reports/reports-awaiting-approval`


| Property | Value |
| --- | --- |
| Angular route | `#/reports/reports-awaiting-approval` |
| Feature module | `reports` (lazy-loaded) |
| Component | `ReportViewerComponent` |
| Route status | planned |
| Data-entry fields | 1 |
| Grids on the screen | 1 |
| Commands | 5 |


| Section | Field (as labelled) | Bound to | Control | Mandatory · source · rules |
| --- | --- | --- | --- | --- |
| — | الجمعية | CharityId | Drop-down list | Optional · options: lookup: Charities (+ كافة الجهات) · on change: getCharityData() |

Grids on this screen:

| Grid (row source) | Columns |
| --- | --- |
| field in All_Data   track by $index | الجمعيه · اسم اليتيم · تاريخ التقرير · كود اليتيم · سبب الرفض |

Commands on this screen:

| Command | Handler | Shown when |
| --- | --- | --- |
| حفظ | DeleteOutgoing() | always |
| (icon only) | ExportReportData() | always |
| (icon only) | EditOrpReport(field.ReportId , field.ChildId) | always |
| (icon only) | GetNext() | always |
| (icon only) | GetPrev() | always |

#### 23.S.17  Screen `#/reports/refused-reports`


| Property | Value |
| --- | --- |
| Angular route | `#/reports/refused-reports` |
| Feature module | `reports` (lazy-loaded) |
| Component | `ReportViewerComponent` |
| Route status | planned |
| Data-entry fields | 1 |
| Grids on the screen | 1 |
| Commands | 5 |


| Section | Field (as labelled) | Bound to | Control | Mandatory · source · rules |
| --- | --- | --- | --- | --- |
| — | الجمعية | CharityId | Drop-down list | Optional · options: lookup: Charities (+ كافة الجهات) · on change: getCharityData() |

Grids on this screen:

| Grid (row source) | Columns |
| --- | --- |
| field in All_Data   track by $index | الجمعيه · اسم اليتيم · تاريخ التقرير · كود اليتيم · سبب الرفض |

Commands on this screen:

| Command | Handler | Shown when |
| --- | --- | --- |
| حفظ | DeleteOutgoing() | always |
| (icon only) | ExportReportData() | always |
| (icon only) | EditOrpReport(field.ReportId , field.ChildId) | always |
| (icon only) | GetNext() | always |
| (icon only) | GetPrev() | always |

#### 23.S.18  Screen `#/reports/orphan-files`


| Property | Value |
| --- | --- |
| Angular route | `#/reports/orphan-files` |
| Feature module | `reports` (lazy-loaded) |
| Component | `ReportViewerComponent` |
| Route status | planned |
| Data-entry fields | 3 |
| Grids on the screen | 2 |
| Commands | 7 |


| Section | Field (as labelled) | Bound to | Control | Mandatory · source · rules |
| --- | --- | --- | --- | --- |
| — | الجمعية | CharityId | Drop-down list | Optional · options: lookup: Charities (+ كافة الجهات) · on change: getCharityData() |
| — | * من تاريخ | DateTo | Date picker | Optional |
| — | (unlabelled) | DateFrom | Date picker | Mandatory |

Grids on this screen:

| Grid (row source) | Columns |
| --- | --- |
| field in All_Data   track by $index | اسم اليتيم · كود اليتيم · الصوره |
| field in Certificates   track by $index | اسم اليتيم · كود اليتيم · الصوره |

Commands on this screen:

| Command | Handler | Shown when |
| --- | --- | --- |
| حفظ | DeleteOutgoing() | always |
| (icon only) | ExportReportData() | always |
| (icon only) | downloadImageData() | always |
| (icon only) | ExportCertificatesData() | always |
| (icon only) | downloadCertificatesData() | always |
| (icon only) | GetNext() | always |
| (icon only) | GetPrev() | always |

#### 23.S.19  Screen `#/reports/provider-sponsor-changes`


| Property | Value |
| --- | --- |
| Angular route | `#/reports/provider-sponsor-changes` |
| Feature module | `reports` (lazy-loaded) |
| Component | `ReportViewerComponent` |
| Route status | planned |
| Data-entry fields | 1 |
| Grids on the screen | 1 |
| Commands | 4 |


| Section | Field (as labelled) | Bound to | Control | Mandatory · source · rules |
| --- | --- | --- | --- | --- |
| — | الجمعية | CharityId | Drop-down list | Optional · options: lookup: Charities (+ كافة الجهات) · on change: getCharityData() |

Grids on this screen:

| Grid (row source) | Columns |
| --- | --- |
| field in All_Data   track by $index | كود الاسره · اسم المعيل السابق · صله القرابه · سبب التغيير · اسم المعيد الجديد · صله القرابه · تاريخ التعديل · اسم الجمعيه · كود اليتيم |

Commands on this screen:

| Command | Handler | Shown when |
| --- | --- | --- |
| حفظ | DeleteOutgoing() | always |
| (icon only) | ExportReportData() | always |
| (icon only) | GetNext() | always |
| (icon only) | GetPrev() | always |

### 23.U  Use case scenarios

One expanded scenario for every use case of this module. Pre-conditions, flows and post-conditions are derived from the screen that hosts the function, the Web API action that serves it and the business-layer method that executes it; quoted messages are the literal strings returned by the business layer.

#### 23.U.1  UC-RPT-01 — Orphan data بيانات الأيتام


| Item | Specification |
| --- | --- |
| Use case ID | UC-RPT-01 |
| Name | Orphan data بيانات الأيتام |
| Type | Read a record |
| Primary actor | All roles |
| Secondary actors | The system (Web API + business layer); the database. |
| Summary | The master orphan listing. Filtered by charity, payment batch, governorate, centre, and the flags all orphans, excluded and warranty required. Used both for review and as the source of orphan extracts. |
| Trigger | The actor opens the screen at `#/reports/orphans` from the module menu. |
| Pre-conditions | 1. The actor is authenticated; the JWT access token is valid and the client holds the role, charity and country claims it carries.<br>2. The actor holds one of: All roles.<br>3. Caller identity comes from the JWT, never from the request; the application service scopes the query to the charity (and country) that owns the user, unless the role is an HQ role permitted to pass an explicit `charityId` filter.<br>4. The SPA route `#/reports/orphans` has loaded and its reference-data lookups have been populated.<br>5. The target record exists and its identifier is known to the screen. |
| Main flow | 1. The actor navigates to the screen at `#/reports/orphans`.<br>2. The actor selects the record to open.<br>3. The SPA issues `POST /api/Reports/orphans` carrying string charityId = null, string paymentId = null, string governorateId = null, string centerId = null, bool allOrphans = false, string userId = null, bool excluded = false, bool requiredWarranty=false.<br>4. `ReportsController` binds the typed request DTO and delegates to the application service.<br>5. `IReportService` validates the payload with its FluentValidation validator, applies the business rules and the charity scope, and persists through `IUnitOfWork`.<br>6. The system returns the record with its child collections and the SPA binds them to the form fields. |
| Alternate flows | • An HQ role (General Director, Financial Director, Staff) may pass an explicit charity id and so read across the charity boundary; a charity user may not and always sees its own data. |
| Exception flows | • The session has expired or the role is not permitted — the request is rejected and the SPA routes back to the login state.<br>• The business layer returns «Faliure» and the operation is not applied. |
| Post-conditions | • No stored data is changed — the operation is a read. |
| Realisation | Route `#/reports/orphans` → `ReportViewerComponent`<br>`POST /api/Reports/orphans` → `ReportsController` → `IReportService` |

#### 23.U.2  UC-RPT-02 — Orphan status حالة اليتيم


| Item | Specification |
| --- | --- |
| Use case ID | UC-RPT-02 |
| Name | Orphan status حالة اليتيم |
| Type | Browse a list |
| Primary actor | All roles |
| Secondary actors | The system (Web API + business layer); the database. |
| Summary | Refined orphan-status query driven by the multi-field status contract (charity, batch, dates, code list, approval state). |
| Trigger | The actor opens the screen at `#/periodic-orphan-reports/orphan-reports/search` from the module menu. |
| Pre-conditions | 1. The actor is authenticated; the JWT access token is valid and the client holds the role, charity and country claims it carries.<br>2. The actor holds one of: All roles.<br>3. The SPA route `#/periodic-orphan-reports/orphan-reports/search` has loaded and its reference-data lookups have been populated. |
| Main flow | 1. The actor navigates to the screen at `#/periodic-orphan-reports/orphan-reports/search`.<br>2. The system loads the filter lookups the screen offers and, for HQ roles, the charity selector.<br>3. The SPA issues `POST /api/PeriodicOrphanReports/{id}/review` carrying [FromUri]OrphanStatusRefinedContract orphanStatusRefinedObject.<br>4. `PeriodicOrphanReportsController` binds the typed request DTO and delegates to the application service.<br>5. `IPeriodicOrphanReportService` validates the payload with its FluentValidation validator, applies the business rules and the charity scope, and persists through `IUnitOfWork`.<br>6. The result set is scoped to the caller’s charity and country, ordered and paged.<br>7. The SPA renders the page with the columns listed in the screen specification and the paging control. |
| Alternate flows | • No row matches the criteria — the grid renders empty and the paging control reports zero pages. |
| Exception flows | • The session has expired or the role is not permitted — the request is rejected and the SPA routes back to the login state.<br>• The business layer returns «Faliure» and the operation is not applied. |
| Post-conditions | • No stored data is changed — the operation is a read. |
| Realisation | Route `#/periodic-orphan-reports/orphan-reports/search` → `OrphanReportSearchComponent`<br>`POST /api/PeriodicOrphanReports/{id}/review` → `PeriodicOrphanReportsController` → `IPeriodicOrphanReportService` |

#### 23.U.3  UC-RPT-03 — Excluded orphans المستبعدون


| Item | Specification |
| --- | --- |
| Use case ID | UC-RPT-03 |
| Name | Excluded orphans المستبعدون |
| Type | Delete a record |
| Primary actor | All roles |
| Secondary actors | The system (Web API + business layer); the database. |
| Summary | Orphans removed from, or requested to be removed from, the sponsorship programme for a charity. |
| Trigger | The actor presses «بحث» on the screen ExcludedOrphans. |
| Pre-conditions | 1. The actor is authenticated; the JWT access token is valid and the client holds the role, charity and country claims it carries.<br>2. The actor holds one of: All roles.<br>3. Caller identity comes from the JWT, never from the request; the application service scopes the query to the charity (and country) that owns the user, unless the role is an HQ role permitted to pass an explicit `charityId` filter.<br>4. The SPA route `#/reports/excluded-orphans` has loaded and its reference-data lookups have been populated.<br>5. The target record exists and its identifier is known to the screen. |
| Main flow | 1. The actor navigates to the screen at `#/reports/excluded-orphans`.<br>2. The actor selects the record and requests its deletion.<br>3. The SPA asks the actor to confirm.<br>4. The SPA issues `POST /api/Reports/excluded-orphans` carrying charityId.<br>5. `PeriodicOrphanReportsController` binds the typed request DTO and delegates to the application service.<br>6. `IReportService` validates the payload with its FluentValidation validator, applies the business rules and the charity scope, and persists through `IUnitOfWork`.<br>7. The business layer checks that the record may still be removed and deletes it (or marks it removed).<br>8. The system returns the outcome and the SPA drops the row from the grid. |
| Alternate flows | • An HQ role (General Director, Financial Director, Staff) may pass an explicit charity id and so read across the charity boundary; a charity user may not and always sees its own data. |
| Exception flows | • The session has expired or the role is not permitted — the request is rejected and the SPA routes back to the login state.<br>• The business layer returns «Faliure» and the operation is not applied. |
| Post-conditions | • The record is no longer returned by the list and read endpoints of the module. |
| Realisation | Route `#/reports/excluded-orphans` → `ReportViewerComponent`<br>`POST /api/Reports/excluded-orphans` → `ReportsController` → `IReportService` |

#### 23.U.4  UC-RPT-04 — Orphans with ended sponsorship أيتام انتهت كفالتهم


| Item | Specification |
| --- | --- |
| Use case ID | UC-RPT-04 |
| Name | Orphans with ended sponsorship أيتام انتهت كفالتهم |
| Type | Read a record |
| Primary actor | Gen. Director |
| Secondary actors | The system (Web API + business layer); the database. |
| Summary | Orphans whose sponsorship term has expired and who therefore need re-sponsoring or closure. |
| Trigger | The actor opens the screen at `#/reports/finished-sponsorship-orphans` from the module menu. |
| Pre-conditions | 1. The actor is authenticated; the JWT access token is valid and the client holds the role, charity and country claims it carries.<br>2. The actor holds one of: Gen. Director.<br>3. Caller identity comes from the JWT, never from the request; the application service scopes the query to the charity (and country) that owns the user, unless the role is an HQ role permitted to pass an explicit `charityId` filter.<br>4. The SPA route `#/reports/finished-sponsorship-orphans` has loaded and its reference-data lookups have been populated.<br>5. The target record exists and its identifier is known to the screen. |
| Main flow | 1. The actor navigates to the screen at `#/reports/finished-sponsorship-orphans`.<br>2. The actor selects the record to open.<br>3. The SPA issues `POST /api/Reports/finished-sponsorship-orphans` carrying charityId.<br>4. `PeriodicOrphanReportsController` binds the typed request DTO and delegates to the application service.<br>5. `IReportService` validates the payload with its FluentValidation validator, applies the business rules and the charity scope, and persists through `IUnitOfWork`.<br>6. The system returns the record with its child collections and the SPA binds them to the form fields. |
| Alternate flows | • An HQ role (General Director, Financial Director, Staff) may pass an explicit charity id and so read across the charity boundary; a charity user may not and always sees its own data. |
| Exception flows | • The session has expired or the role is not permitted — the request is rejected and the SPA routes back to the login state. |
| Post-conditions | • No stored data is changed — the operation is a read. |
| Realisation | Route `#/reports/finished-sponsorship-orphans` → `ReportViewerComponent`<br>`POST /api/Reports/finished-sponsorship-orphans` → `ReportsController` → `IReportService` |

#### 23.U.5  UC-RPT-05 — Unsponsored orphans أيتام غير مكفولين


| Item | Specification |
| --- | --- |
| Use case ID | UC-RPT-05 |
| Name | Unsponsored orphans أيتام غير مكفولين |
| Type | Create a record |
| Primary actor | Gen. Director |
| Secondary actors | The system (Web API + business layer); the database. |
| Summary | Registered orphans who currently have no sponsor attached — the pipeline for new sponsorships. |
| Trigger | The actor presses «بحث» on the screen NotSponsorshipOrphans. |
| Pre-conditions | 1. The actor is authenticated; the JWT access token is valid and the client holds the role, charity and country claims it carries.<br>2. The actor holds one of: Gen. Director.<br>3. Caller identity comes from the JWT, never from the request; the application service scopes the query to the charity (and country) that owns the user, unless the role is an HQ role permitted to pass an explicit `charityId` filter.<br>4. The SPA route `#/reports/unsponsored-orphans` has loaded and its reference-data lookups have been populated. |
| Main flow | 1. The actor navigates to the screen at `#/reports/unsponsored-orphans`. The screen opens in add mode.<br>2. The system loads the reference data the form needs (regions, centres, banks, types and the other lookup lists bound to the drop-downs).<br>3. The actor completes the input fields of the form.<br>4. The actor presses «بحث» (GetData()).<br>5. The SPA issues `POST /api/Reports/unsponsored-orphans` carrying charityId.<br>6. `PeriodicOrphanReportsController` binds the typed request DTO and delegates to the application service.<br>7. `IReportService` validates the payload with its FluentValidation validator, applies the business rules and the charity scope, and persists through `IUnitOfWork`.<br>8. The business layer validates the payload, stamps the owning charity and the creating user, and persists the new record.<br>9. The system returns the outcome and the SPA confirms the save and returns to the list screen. |
| Alternate flows | • An HQ role (General Director, Financial Director, Staff) may pass an explicit charity id and so read across the charity boundary; a charity user may not and always sees its own data.<br>• The actor abandons the form before saving — nothing is written and the record keeps its previous state. |
| Exception flows | • The session has expired or the role is not permitted — the request is rejected and the SPA routes back to the login state.<br>• A mandatory field is empty or fails its format check — the save is refused and the field is flagged on the form. |
| Post-conditions | • A new record exists, owned by the charity of the creating user, and appears in the list screen of the module. |
| Realisation | Route `#/reports/unsponsored-orphans` → `ReportViewerComponent`<br>`POST /api/Reports/unsponsored-orphans` → `ReportsController` → `IReportService` |

#### 23.U.6  UC-RPT-06 — Widows requiring sponsorship أرامل مطلوب لهم كفالة


| Item | Specification |
| --- | --- |
| Use case ID | UC-RPT-06 |
| Name | Widows requiring sponsorship أرامل مطلوب لهم كفالة |
| Type | Query a report |
| Primary actor | All roles |
| Secondary actors | The system (Web API + business layer); the database. |
| Summary | Widowed guardians who are eligible for, and have consented to, widow sponsorship for a charity, report number and batch. |
| Trigger | The actor opens the screen at `#/reports/widows-allowing-sponsorship` from the module menu. |
| Pre-conditions | 1. The actor is authenticated; the JWT access token is valid and the client holds the role, charity and country claims it carries.<br>2. The actor holds one of: All roles.<br>3. Caller identity comes from the JWT, never from the request; the application service scopes the query to the charity (and country) that owns the user, unless the role is an HQ role permitted to pass an explicit `charityId` filter.<br>4. The SPA route `#/reports/widows-allowing-sponsorship` has loaded and its reference-data lookups have been populated. |
| Main flow | 1. The actor navigates to the screen at `#/reports/widows-allowing-sponsorship`.<br>2. The actor sets the report criteria (period, charity, country and the other filters the screen offers).<br>3. The actor runs the report.<br>4. The SPA issues `POST /api/Reports/widows-allowing-sponsorship` carrying string charityId, int reportNo, bool isCodes, string batchId.<br>5. `OrphanReportsController` binds the typed request DTO and delegates to the application service.<br>6. `IPeriodicOrphanReportService` validates the payload with its FluentValidation validator, applies the business rules and the charity scope, and persists through `IUnitOfWork`.<br>7. The business layer composes the projection, scoped to the caller’s charity and country.<br>8. The result is rendered in the on-screen grid, from where it can be printed or exported. |
| Alternate flows | • An HQ role (General Director, Financial Director, Staff) may pass an explicit charity id and so read across the charity boundary; a charity user may not and always sees its own data.<br>• No row matches the criteria — the grid renders empty and the paging control reports zero pages. |
| Exception flows | • The session has expired or the role is not permitted — the request is rejected and the SPA routes back to the login state. |
| Post-conditions | • No stored data is changed — the operation is a read. |
| Realisation | Route `#/reports/widows-allowing-sponsorship` → `ReportViewerComponent`<br>`POST /api/Reports/widows-allowing-sponsorship` → `ReportsController` → `IReportService` |

#### 23.U.7  UC-RPT-07 — Registered Meza cards تقرير الكروت المسجلة


| Item | Specification |
| --- | --- |
| Use case ID | UC-RPT-07 |
| Name | Registered Meza cards تقرير الكروت المسجلة |
| Type | Create a record |
| Primary actor | Gen. Director |
| Secondary actors | The system (Web API + business layer); the database. |
| Summary | Guardians with a registered payment card, confirming that the disbursement channel exists before a bank transfer batch is built. |
| Trigger | The actor presses «بحث» on the screen MezaCardReport. |
| Pre-conditions | 1. The actor is authenticated; the JWT access token is valid and the client holds the role, charity and country claims it carries.<br>2. The actor holds one of: Gen. Director.<br>3. Caller identity comes from the JWT, never from the request; the application service scopes the query to the charity (and country) that owns the user, unless the role is an HQ role permitted to pass an explicit `charityId` filter.<br>4. The SPA route `#/reports/meza-cards` has loaded and its reference-data lookups have been populated. |
| Main flow | 1. The actor navigates to the screen at `#/reports/meza-cards`. The screen opens in add mode.<br>2. The system loads the reference data the form needs (regions, centres, banks, types and the other lookup lists bound to the drop-downs).<br>3. The actor completes the input fields of the form.<br>4. The actor presses «بحث» (GetData()).<br>5. The SPA issues `POST /api/Reports/meza-cards` carrying charityId.<br>6. `PeriodicOrphanReportsController` binds the typed request DTO and delegates to the application service.<br>7. `IReportService` validates the payload with its FluentValidation validator, applies the business rules and the charity scope, and persists through `IUnitOfWork`.<br>8. The business layer validates the payload, stamps the owning charity and the creating user, and persists the new record.<br>9. The system returns the outcome and the SPA confirms the save and returns to the list screen. |
| Alternate flows | • An HQ role (General Director, Financial Director, Staff) may pass an explicit charity id and so read across the charity boundary; a charity user may not and always sees its own data.<br>• The actor abandons the form before saving — nothing is written and the record keeps its previous state. |
| Exception flows | • The session has expired or the role is not permitted — the request is rejected and the SPA routes back to the login state.<br>• The business layer returns «Faild Operation» and the operation is not applied.<br>• A mandatory field is empty or fails its format check — the save is refused and the field is flagged on the form. |
| Post-conditions | • A new record exists, owned by the charity of the creating user, and appears in the list screen of the module. |
| Realisation | Route `#/reports/meza-cards` → `ReportViewerComponent`<br>`POST /api/Reports/meza-cards` → `ReportsController` → `IReportService` |

#### 23.U.8  UC-RPT-08 — Extract guardian Meza cards استخراج كروت العائل


| Item | Specification |
| --- | --- |
| Use case ID | UC-RPT-08 |
| Name | Extract guardian Meza cards استخراج كروت العائل |
| Type | Export data |
| Primary actor | Gen. Director, Fin. Director |
| Secondary actors | The system (Web API + business layer); the database. EPPlus as the worksheet writer. |
| Summary | The card-number extract for a charity, report number, batch and date range, with a switch for whether a card already exists — the input to card issuance and to bank file preparation. |
| Trigger | The actor selects the function from the module menu or the screen that hosts it. |
| Pre-conditions | 1. The actor is authenticated; the JWT access token is valid and the client holds the role, charity and country claims it carries.<br>2. The actor holds one of: Gen. Director, Fin. Director.<br>3. Caller identity comes from the JWT, never from the request; the application service scopes the query to the charity (and country) that owns the user, unless the role is an HQ role permitted to pass an explicit `charityId` filter. |
| Main flow | 1. The actor reaches the function from the screen of this module that hosts it.<br>2. The actor sets the selection criteria for the extract.<br>3. The actor presses the command button of the screen.<br>4. The SPA issues `POST /api/Reports/meza-cards` carrying string charityId, int reportNo, bool isCodes, string batchId, DateTime? DateFrom = null, DateTime? DateTo = null , bool MezaCardExist=false.<br>5. `OrphanReportsController` binds the typed request DTO and delegates to the application service.<br>6. `IPeriodicOrphanReportService` validates the payload with its FluentValidation validator, applies the business rules and the charity scope, and persists through `IUnitOfWork`.<br>7. The business layer builds the worksheet (EPPlus) from the selected rows and streams it to the browser as a download. |
| Alternate flows | • An HQ role (General Director, Financial Director, Staff) may pass an explicit charity id and so read across the charity boundary; a charity user may not and always sees its own data. |
| Exception flows | • The session has expired or the role is not permitted — the request is rejected and the SPA routes back to the login state. |
| Post-conditions | • A workbook has been delivered to the actor. No stored data is changed. |
| Realisation | `POST /api/Reports/meza-cards` → `ReportsController` → `IReportService` |

#### 23.U.9  UC-RPT-09 — Assistance family data بيانات أسر المساعدات


| Item | Specification |
| --- | --- |
| Use case ID | UC-RPT-09 |
| Name | Assistance family data بيانات أسر المساعدات |
| Type | Read a record |
| Primary actor | All roles |
| Secondary actors | The system (Web API + business layer); the database. |
| Summary | Detailed listing of the families benefiting from assistance for a charity. |
| Trigger | The actor opens the screen at `#/reports/beneficiary-family-details` from the module menu. |
| Pre-conditions | 1. The actor is authenticated; the JWT access token is valid and the client holds the role, charity and country claims it carries.<br>2. The actor holds one of: All roles.<br>3. Caller identity comes from the JWT, never from the request; the application service scopes the query to the charity (and country) that owns the user, unless the role is an HQ role permitted to pass an explicit `charityId` filter.<br>4. The SPA route `#/reports/beneficiary-family-details` has loaded and its reference-data lookups have been populated.<br>5. The target record exists and its identifier is known to the screen. |
| Main flow | 1. The actor navigates to the screen at `#/reports/beneficiary-family-details`.<br>2. The actor selects the record to open.<br>3. The SPA issues `POST /api/Reports/beneficiary-family-details` carrying charityId, userId.<br>4. `ReportsController` binds the typed request DTO and delegates to the application service.<br>5. `IFamilyService` validates the payload with its FluentValidation validator, applies the business rules and the charity scope, and persists through `IUnitOfWork`.<br>6. The system returns the record with its child collections and the SPA binds them to the form fields. |
| Alternate flows | • An HQ role (General Director, Financial Director, Staff) may pass an explicit charity id and so read across the charity boundary; a charity user may not and always sees its own data. |
| Exception flows | • The session has expired or the role is not permitted — the request is rejected and the SPA routes back to the login state. |
| Post-conditions | • No stored data is changed — the operation is a read. |
| Realisation | Route `#/reports/beneficiary-family-details` → `ReportViewerComponent`<br>`POST /api/Reports/beneficiary-family-details` → `ReportsController` → `IReportService` |

#### 23.U.10  UC-RPT-10 — Beneficiary statistics احصائيات المستفيدين


| Item | Specification |
| --- | --- |
| Use case ID | UC-RPT-10 |
| Name | Beneficiary statistics احصائيات المستفيدين |
| Type | Query a report |
| Primary actor | Gen. Director |
| Secondary actors | The system (Web API + business layer); the database. |
| Summary | Aggregate beneficiary counts across charities and programmes, for management reporting. |
| Trigger | The actor opens the screen at `#/seasonal-aid/:id/report` from the module menu. |
| Pre-conditions | 1. The actor is authenticated; the JWT access token is valid and the client holds the role, charity and country claims it carries.<br>2. The actor holds one of: Gen. Director.<br>3. The SPA route `#/seasonal-aid/:id/report` has loaded and its reference-data lookups have been populated. |
| Main flow | 1. The actor navigates to the screen at `#/seasonal-aid/:id/report`.<br>2. The actor sets the report criteria (period, charity, country and the other filters the screen offers).<br>3. The actor runs the report.<br>4. The SPA calls the service endpoint that backs the function.<br>5. The Web API controller receives the request and delegates to the business layer.<br>6. The business layer executes the rules and the data access.<br>7. The business layer composes the projection, scoped to the caller’s charity and country.<br>8. The result is rendered in the on-screen grid, from where it can be printed or exported. |
| Alternate flows | • No row matches the criteria — the grid renders empty and the paging control reports zero pages. |
| Exception flows | • The session has expired or the role is not permitted — the request is rejected and the SPA routes back to the login state. |
| Post-conditions | • No stored data is changed — the operation is a read. |
| Realisation | Route `#/seasonal-aid/:id/report` → `CampaignReportComponent` |

#### 23.U.11  UC-RPT-11 — Family projects مشاريع الأسر


| Item | Specification |
| --- | --- |
| Use case ID | UC-RPT-11 |
| Name | Family projects مشاريع الأسر |
| Type | Create a record |
| Primary actor | Gen. Director |
| Secondary actors | The system (Web API + business layer); the database. |
| Summary | The projects each family has been registered for in a charity. |
| Trigger | The actor presses «بحث» on the screen RegisterdFamilyProject. |
| Pre-conditions | 1. The actor is authenticated; the JWT access token is valid and the client holds the role, charity and country claims it carries.<br>2. The actor holds one of: Gen. Director.<br>3. Caller identity comes from the JWT, never from the request; the application service scopes the query to the charity (and country) that owns the user, unless the role is an HQ role permitted to pass an explicit `charityId` filter.<br>4. The SPA route `#/reports/registered-family-projects` has loaded and its reference-data lookups have been populated. |
| Main flow | 1. The actor navigates to the screen at `#/reports/registered-family-projects`. The screen opens in add mode.<br>2. The system loads the reference data the form needs (regions, centres, banks, types and the other lookup lists bound to the drop-downs).<br>3. The actor completes the input fields of the form.<br>4. The actor presses «بحث» (GetData()).<br>5. The SPA issues `POST /api/Reports/registered-family-projects` carrying charityId.<br>6. `PeriodicOrphanReportsController` binds the typed request DTO and delegates to the application service.<br>7. `IReportService` validates the payload with its FluentValidation validator, applies the business rules and the charity scope, and persists through `IUnitOfWork`.<br>8. The business layer validates the payload, stamps the owning charity and the creating user, and persists the new record.<br>9. The system returns the outcome and the SPA confirms the save and returns to the list screen. |
| Alternate flows | • An HQ role (General Director, Financial Director, Staff) may pass an explicit charity id and so read across the charity boundary; a charity user may not and always sees its own data.<br>• The actor abandons the form before saving — nothing is written and the record keeps its previous state. |
| Exception flows | • The session has expired or the role is not permitted — the request is rejected and the SPA routes back to the login state.<br>• The business layer returns «Faild Operation» and the operation is not applied.<br>• A mandatory field is empty or fails its format check — the save is refused and the field is flagged on the form. |
| Post-conditions | • A new record exists, owned by the charity of the creating user, and appears in the list screen of the module. |
| Realisation | Route `#/reports/registered-family-projects` → `ReportViewerComponent`<br>`POST /api/Reports/registered-family-projects` → `ReportsController` → `IReportService` |

#### 23.U.12  UC-RPT-12 — Guardian change history تقارير تعديل المعيل


| Item | Specification |
| --- | --- |
| Use case ID | UC-RPT-12 |
| Name | Guardian change history تقارير تعديل المعيل |
| Type | Update a record |
| Primary actor | All roles |
| Secondary actors | The system (Web API + business layer); the database. |
| Summary | Old and new guardian pairs for a charity, evidencing every change of the responsible adult on a family file. |
| Trigger | The actor presses «حفظ» on the screen ParentSonsorNewAndOld. |
| Pre-conditions | 1. The actor is authenticated; the JWT access token is valid and the client holds the role, charity and country claims it carries.<br>2. The actor holds one of: All roles.<br>3. Caller identity comes from the JWT, never from the request; the application service scopes the query to the charity (and country) that owns the user, unless the role is an HQ role permitted to pass an explicit `charityId` filter.<br>4. The SPA route `#/reports/provider-sponsor-changes` has loaded and its reference-data lookups have been populated.<br>5. The target record exists and its identifier is known to the screen. |
| Main flow | 1. The actor navigates to the screen at `#/reports/provider-sponsor-changes`. The screen opens in edit mode on the selected record.<br>2. The system loads the current values into the form.<br>3. The actor changes the fields to be corrected.<br>4. The actor presses «حفظ» (DeleteOutgoing()).<br>5. The SPA issues `POST /api/Reports/provider-sponsor-changes` carrying charityId.<br>6. `ReportsController` binds the typed request DTO and delegates to the application service.<br>7. `ISeasonalAidService` validates the payload with its FluentValidation validator, applies the business rules and the charity scope, and persists through `IUnitOfWork`.<br>8. The business layer re-validates the payload, applies the changes and persists them.<br>9. The system returns the outcome and the SPA refreshes the screen. |
| Alternate flows | • An HQ role (General Director, Financial Director, Staff) may pass an explicit charity id and so read across the charity boundary; a charity user may not and always sees its own data.<br>• The actor abandons the form before saving — nothing is written and the record keeps its previous state. |
| Exception flows | • The session has expired or the role is not permitted — the request is rejected and the SPA routes back to the login state.<br>• A mandatory field is empty or fails its format check — the save is refused and the field is flagged on the form. |
| Post-conditions | • The stored record carries the new values; no other record is affected. |
| Realisation | Route `#/reports/provider-sponsor-changes` → `ReportViewerComponent`<br>`POST /api/Reports/provider-sponsor-changes` → `ReportsController` → `IReportService` |

#### 23.U.13  UC-RPT-13 — Family and orphan entry tracking متابعة إدخالات الأسر والأيتام


| Item | Specification |
| --- | --- |
| Use case ID | UC-RPT-13 |
| Name | Family and orphan entry tracking متابعة إدخالات الأسر والأيتام |
| Type | Query a report |
| Primary actor | All roles |
| Secondary actors | The system (Web API + business layer); the database. |
| Summary | What each charity entered on a given date, used to monitor data-entry activity. |
| Trigger | The actor opens the screen at `#/reports/family-orphans` from the module menu. |
| Pre-conditions | 1. The actor is authenticated; the JWT access token is valid and the client holds the role, charity and country claims it carries.<br>2. The actor holds one of: All roles.<br>3. Caller identity comes from the JWT, never from the request; the application service scopes the query to the charity (and country) that owns the user, unless the role is an HQ role permitted to pass an explicit `charityId` filter.<br>4. The SPA route `#/reports/family-orphans` has loaded and its reference-data lookups have been populated. |
| Main flow | 1. The actor navigates to the screen at `#/reports/family-orphans`.<br>2. The actor sets the report criteria (period, charity, country and the other filters the screen offers).<br>3. The actor runs the report.<br>4. The SPA issues `GET /api/Families/{id}/follow-up` carrying DateTime date, string charityId.<br>5. `FamiliesController` binds the typed request DTO and delegates to the application service.<br>6. `IReportService` validates the payload with its FluentValidation validator, applies the business rules and the charity scope, and persists through `IUnitOfWork`.<br>7. The business layer composes the projection, scoped to the caller’s charity and country.<br>8. The result is rendered in the on-screen grid, from where it can be printed or exported. |
| Alternate flows | • An HQ role (General Director, Financial Director, Staff) may pass an explicit charity id and so read across the charity boundary; a charity user may not and always sees its own data.<br>• No row matches the criteria — the grid renders empty and the paging control reports zero pages. |
| Exception flows | • The session has expired or the role is not permitted — the request is rejected and the SPA routes back to the login state. |
| Post-conditions | • No stored data is changed — the operation is a read. |
| Realisation | Route `#/reports/family-orphans` → `ReportViewerComponent`<br>`GET /api/Families/{id}/follow-up` → `FamiliesController` → `IFamilyService` |

#### 23.U.14  UC-RPT-14 — General orphan statistics احصائيات عامة للأيتام


| Item | Specification |
| --- | --- |
| Use case ID | UC-RPT-14 |
| Name | General orphan statistics احصائيات عامة للأيتام |
| Type | Query a report |
| Primary actor | Gen. Director |
| Secondary actors | The system (Web API + business layer); the database. |
| Summary | Counts of orphan reports grouped by status across the selected scope. |
| Trigger | The actor opens the screen at `#/periodic-orphan-reports/orphan-reports` from the module menu. |
| Pre-conditions | 1. The actor is authenticated; the JWT access token is valid and the client holds the role, charity and country claims it carries.<br>2. The actor holds one of: Gen. Director.<br>3. The SPA route `#/periodic-orphan-reports/orphan-reports` has loaded and its reference-data lookups have been populated. |
| Main flow | 1. The actor navigates to the screen at `#/periodic-orphan-reports/orphan-reports`.<br>2. The actor sets the report criteria (period, charity, country and the other filters the screen offers).<br>3. The actor runs the report.<br>4. The SPA calls the service endpoint that backs the function.<br>5. The Web API controller receives the request and delegates to the business layer.<br>6. The business layer executes the rules and the data access.<br>7. The business layer composes the projection, scoped to the caller’s charity and country.<br>8. The result is rendered in the on-screen grid, from where it can be printed or exported. |
| Alternate flows | • No row matches the criteria — the grid renders empty and the paging control reports zero pages. |
| Exception flows | • The session has expired or the role is not permitted — the request is rejected and the SPA routes back to the login state. |
| Post-conditions | • No stored data is changed — the operation is a read. |
| Realisation | Route `#/periodic-orphan-reports/orphan-reports` → `OrphanReportsListComponent` |

#### 23.U.15  UC-RPT-15 — Coded orphans needing a report أيتام مكودون مطلوب لهم تقرير


| Item | Specification |
| --- | --- |
| Use case ID | UC-RPT-15 |
| Name | Coded orphans needing a report أيتام مكودون مطلوب لهم تقرير |
| Type | Query a report |
| Primary actor | Gen. Director |
| Secondary actors | The system (Web API + business layer); the database. |
| Summary | Coded orphans in a charity with no current periodic report — the primary chase list before a payment run. |
| Trigger | The actor opens the screen at `#/reports/orphans-missing-reports` from the module menu. |
| Pre-conditions | 1. The actor is authenticated; the JWT access token is valid and the client holds the role, charity and country claims it carries.<br>2. The actor holds one of: Gen. Director.<br>3. Caller identity comes from the JWT, never from the request; the application service scopes the query to the charity (and country) that owns the user, unless the role is an HQ role permitted to pass an explicit `charityId` filter.<br>4. The SPA route `#/reports/orphans-missing-reports` has loaded and its reference-data lookups have been populated. |
| Main flow | 1. The actor navigates to the screen at `#/reports/orphans-missing-reports`.<br>2. The actor sets the report criteria (period, charity, country and the other filters the screen offers).<br>3. The actor runs the report.<br>4. The SPA issues `POST /api/Reports/orphans-missing-reports` carrying charityId.<br>5. `ReportsController` binds the typed request DTO and delegates to the application service.<br>6. `ISeasonalAidService` validates the payload with its FluentValidation validator, applies the business rules and the charity scope, and persists through `IUnitOfWork`.<br>7. The business layer composes the projection, scoped to the caller’s charity and country.<br>8. The result is rendered in the on-screen grid, from where it can be printed or exported. |
| Alternate flows | • An HQ role (General Director, Financial Director, Staff) may pass an explicit charity id and so read across the charity boundary; a charity user may not and always sees its own data.<br>• No row matches the criteria — the grid renders empty and the paging control reports zero pages. |
| Exception flows | • The session has expired or the role is not permitted — the request is rejected and the SPA routes back to the login state. |
| Post-conditions | • No stored data is changed — the operation is a read. |
| Realisation | Route `#/reports/orphans-missing-reports` → `ReportViewerComponent`<br>`POST /api/Reports/orphans-missing-reports` → `ReportsController` → `IReportService` |

#### 23.U.16  UC-RPT-16 — Orphans missing files أيتام مطلوب لهم ملفات


| Item | Specification |
| --- | --- |
| Use case ID | UC-RPT-16 |
| Name | Orphans missing files أيتام مطلوب لهم ملفات |
| Type | Import a file |
| Primary actor | Gen. Director |
| Secondary actors | The system (Web API + business layer); the database. The uploaded workbook/CSV as the data source. |
| Summary | Orphans whose supporting document files are absent from storage, so the charity can be asked to upload them. |
| Trigger | The actor presses «حفظ» on the screen NeedFileForOrphans. |
| Pre-conditions | 1. The actor is authenticated; the JWT access token is valid and the client holds the role, charity and country claims it carries.<br>2. The actor holds one of: Gen. Director.<br>3. Caller identity comes from the JWT, never from the request; the application service scopes the query to the charity (and country) that owns the user, unless the role is an HQ role permitted to pass an explicit `charityId` filter.<br>4. The SPA route `#/reports/orphans-missing-files` has loaded and its reference-data lookups have been populated. |
| Main flow | 1. The actor navigates to the screen at `#/reports/orphans-missing-files`.<br>2. The actor chooses the file to upload in the file field of the screen.<br>3. The actor presses «حفظ» (DeleteOutgoing()).<br>4. The SPA posts the file as multipart content to `POST /api/Reports/orphans-missing-files`.<br>5. `ReportsController` binds the typed request DTO and delegates to the application service.<br>6. `ISeasonalAidService` validates the payload with its FluentValidation validator, applies the business rules and the charity scope, and persists through `IUnitOfWork`.<br>7. The business layer parses each row, matches it to the existing records by their key and applies the values it carries.<br>8. Rows that cannot be matched are reported back and the system returns the count applied. |
| Alternate flows | • An HQ role (General Director, Financial Director, Staff) may pass an explicit charity id and so read across the charity boundary; a charity user may not and always sees its own data. |
| Exception flows | • The session has expired or the role is not permitted — the request is rejected and the SPA routes back to the login state.<br>• The uploaded file is not the expected workbook/CSV layout — the import is abandoned and no row is changed. |
| Post-conditions | • The matched records carry the imported values; unmatched rows are left untouched and reported. |
| Realisation | Route `#/reports/orphans-missing-files` → `ReportViewerComponent`<br>`POST /api/Reports/orphans-missing-files` → `ReportsController` → `IReportService` |

#### 23.U.17  UC-RPT-17 — Reports awaiting approval تقارير في انتظار الموافقة


| Item | Specification |
| --- | --- |
| Use case ID | UC-RPT-17 |
| Name | Reports awaiting approval تقارير في انتظار الموافقة |
| Type | Query a report |
| Primary actor | Gen. Director |
| Secondary actors | The system (Web API + business layer); the database. |
| Summary | The HQ review queue: submitted reports that are neither accepted nor refused. |
| Trigger | The actor opens the screen at `#/reports/reports-awaiting-approval` from the module menu. |
| Pre-conditions | 1. The actor is authenticated; the JWT access token is valid and the client holds the role, charity and country claims it carries.<br>2. The actor holds one of: Gen. Director.<br>3. Caller identity comes from the JWT, never from the request; the application service scopes the query to the charity (and country) that owns the user, unless the role is an HQ role permitted to pass an explicit `charityId` filter.<br>4. The SPA route `#/reports/reports-awaiting-approval` has loaded and its reference-data lookups have been populated. |
| Main flow | 1. The actor navigates to the screen at `#/reports/reports-awaiting-approval`.<br>2. The actor sets the report criteria (period, charity, country and the other filters the screen offers).<br>3. The actor runs the report.<br>4. The SPA issues `POST /api/Reports/reports-awaiting-approval` carrying charityId.<br>5. `ReportsController` binds the typed request DTO and delegates to the application service.<br>6. `ISeasonalAidService` validates the payload with its FluentValidation validator, applies the business rules and the charity scope, and persists through `IUnitOfWork`.<br>7. The business layer composes the projection, scoped to the caller’s charity and country.<br>8. The result is rendered in the on-screen grid, from where it can be printed or exported. |
| Alternate flows | • An HQ role (General Director, Financial Director, Staff) may pass an explicit charity id and so read across the charity boundary; a charity user may not and always sees its own data.<br>• No row matches the criteria — the grid renders empty and the paging control reports zero pages. |
| Exception flows | • The session has expired or the role is not permitted — the request is rejected and the SPA routes back to the login state. |
| Post-conditions | • No stored data is changed — the operation is a read. |
| Realisation | Route `#/reports/reports-awaiting-approval` → `ReportViewerComponent`<br>`POST /api/Reports/reports-awaiting-approval` → `ReportsController` → `IReportService` |

#### 23.U.18  UC-RPT-18 — Refused reports تقارير تم رفضها


| Item | Specification |
| --- | --- |
| Use case ID | UC-RPT-18 |
| Name | Refused reports تقارير تم رفضها |
| Type | Review decision |
| Primary actor | All roles |
| Secondary actors | The system (Web API + business layer); the database. |
| Summary | Reports rejected by HQ with their refusal reasons, forming the charity's correction worklist. |
| Trigger | The actor presses «حفظ» on the screen RefusedReports. |
| Pre-conditions | 1. The actor is authenticated; the JWT access token is valid and the client holds the role, charity and country claims it carries.<br>2. The actor holds one of: All roles.<br>3. Caller identity comes from the JWT, never from the request; the application service scopes the query to the charity (and country) that owns the user, unless the role is an HQ role permitted to pass an explicit `charityId` filter.<br>4. The SPA route `#/reports/refused-reports` has loaded and its reference-data lookups have been populated.<br>5. The target record exists and its identifier is known to the screen. |
| Main flow | 1. The actor navigates to the screen at `#/reports/refused-reports`.<br>2. The actor opens the item awaiting a decision and examines its content and attachments.<br>3. The actor records the decision and, when refusing, the reason.<br>4. The actor presses «حفظ» (DeleteOutgoing()).<br>5. The SPA issues `POST /api/Reports/refused-reports` carrying charityId.<br>6. `ReportsController` binds the typed request DTO and delegates to the application service.<br>7. `ISeasonalAidService` validates the payload with its FluentValidation validator, applies the business rules and the charity scope, and persists through `IUnitOfWork`.<br>8. The business layer writes the new state, the deciding user and the decision date.<br>9. The item leaves the pending queue and becomes visible to the charity in its new state. |
| Alternate flows | • An HQ role (General Director, Financial Director, Staff) may pass an explicit charity id and so read across the charity boundary; a charity user may not and always sees its own data.<br>• The decision is a refusal — the reason is mandatory and is stored with the item so that the charity can see why it was returned. |
| Exception flows | • The session has expired or the role is not permitted — the request is rejected and the SPA routes back to the login state. |
| Post-conditions | • The item carries its new state, the deciding user and the decision date, and moves out of the pending queue. |
| Realisation | Route `#/reports/refused-reports` → `ReportViewerComponent`<br>`POST /api/Reports/refused-reports` → `ReportsController` → `IReportService` |

#### 23.U.19  UC-RPT-19 — Orphans without a renewed report أيتام بدون تقرير مجدد


| Item | Specification |
| --- | --- |
| Use case ID | UC-RPT-19 |
| Name | Orphans without a renewed report أيتام بدون تقرير مجدد |
| Type | Query a report |
| Primary actor | Gen. Director |
| Secondary actors | The system (Web API + business layer); the database. |
| Summary | Per charity and batch, the orphans whose report has not been renewed for the current cycle, in detail, V2 and count-only variants. |
| Trigger | The actor selects the function from the module menu or the screen that hosts it. |
| Pre-conditions | 1. The actor is authenticated; the JWT access token is valid and the client holds the role, charity and country claims it carries.<br>2. The actor holds one of: Gen. Director.<br>3. Caller identity comes from the JWT, never from the request; the application service scopes the query to the charity (and country) that owns the user, unless the role is an HQ role permitted to pass an explicit `charityId` filter. |
| Main flow | 1. The actor reaches the function from the screen of this module that hosts it.<br>2. The actor sets the report criteria (period, charity, country and the other filters the screen offers).<br>3. The actor runs the report.<br>4. The SPA issues `POST /api/Reports/non-renewed-reports` carrying string charityId, string batchId.<br>5. `OrphanReportsController` binds the typed request DTO and delegates to the application service.<br>6. `IPeriodicOrphanReportService` validates the payload with its FluentValidation validator, applies the business rules and the charity scope, and persists through `IUnitOfWork`.<br>7. The business layer composes the projection, scoped to the caller’s charity and country.<br>8. The result is rendered in the on-screen grid, from where it can be printed or exported. |
| Alternate flows | • An HQ role (General Director, Financial Director, Staff) may pass an explicit charity id and so read across the charity boundary; a charity user may not and always sees its own data.<br>• No row matches the criteria — the grid renders empty and the paging control reports zero pages. |
| Exception flows | • The session has expired or the role is not permitted — the request is rejected and the SPA routes back to the login state. |
| Post-conditions | • No stored data is changed — the operation is a read. |
| Realisation | `POST /api/Reports/non-renewed-reports` → `ReportsController` → `IReportService` |

#### 23.U.20  UC-RPT-20 — Charity follow-up متابعة الجمعيات


| Item | Specification |
| --- | --- |
| Use case ID | UC-RPT-20 |
| Name | Charity follow-up متابعة الجمعيات |
| Type | Query a report |
| Primary actor | Gen. Director |
| Secondary actors | The system (Web API + business layer); the database. |
| Summary | Cross-charity tracking of a payment batch: what each charity has entered, printed, disbursed and confirmed. The primary supervisory dashboard. |
| Trigger | The actor opens the screen at `#/reports/charity-payment-tracking` from the module menu. |
| Pre-conditions | 1. The actor is authenticated; the JWT access token is valid and the client holds the role, charity and country claims it carries.<br>2. The actor holds one of: Gen. Director.<br>3. The SPA route `#/reports/charity-payment-tracking` has loaded and its reference-data lookups have been populated. |
| Main flow | 1. The actor navigates to the screen at `#/reports/charity-payment-tracking`.<br>2. The actor sets the report criteria (period, charity, country and the other filters the screen offers).<br>3. The actor runs the report.<br>4. The SPA issues `POST /api/Reports/charity-payment-tracking` carrying batchId.<br>5. `ReportsController` binds the typed request DTO and delegates to the application service.<br>6. `IOrphanPaymentService` validates the payload with its FluentValidation validator, applies the business rules and the charity scope, and persists through `IUnitOfWork`.<br>7. The business layer composes the projection, scoped to the caller’s charity and country.<br>8. The result is rendered in the on-screen grid, from where it can be printed or exported. |
| Alternate flows | • No row matches the criteria — the grid renders empty and the paging control reports zero pages. |
| Exception flows | • The session has expired or the role is not permitted — the request is rejected and the SPA routes back to the login state. |
| Post-conditions | • No stored data is changed — the operation is a read. |
| Realisation | Route `#/reports/charity-payment-tracking` → `ReportViewerComponent`<br>`POST /api/Reports/charity-payment-tracking` · `POST /api/Reports/charity-payment-tracking/export/pdf` → `ReportsController` → `IReportService` |

#### 23.U.21  UC-RPT-21 — Family update tracking متابعة تحديث بيانات الأسر


| Item | Specification |
| --- | --- |
| Use case ID | UC-RPT-21 |
| Name | Family update tracking متابعة تحديث بيانات الأسر |
| Type | Print / produce a document |
| Primary actor | Gen. Director |
| Secondary actors | The system (Web API + business layer); the database. the report and export endpoints of `api/Reports` as the rendering path — jsPDF for PDF and ExcelJS for Excel. |
| Summary | Which family files a charity refreshed around a payment date, printed as a monitoring sheet. |
| Trigger | The actor selects the function from the module menu or the screen that hosts it. |
| Pre-conditions | 1. The actor is authenticated; the JWT access token is valid and the client holds the role, charity and country claims it carries.<br>2. The actor holds one of: Gen. Director.<br>3. The target record exists and its identifier is known to the screen. |
| Main flow | 1. The actor reaches the function from the screen of this module that hosts it.<br>2. The actor selects the record or the range to be printed.<br>3. The actor presses the command button of the screen.<br>4. The browser opens `POST /api/Reports/family-update-tracking/export/pdf` with paymentId, date.<br>5. `ReportsController` resolves the report key, applies the filter, and renders the document from the dataset the application service returns.<br>6. The rendered document is streamed back as a PDF for viewing and printing. |
| Alternate flows | None recorded. |
| Exception flows | • The session has expired or the role is not permitted — the request is rejected and the SPA routes back to the login state.<br>• The report definition or a required parameter is missing — the print action fails and no document is produced. |
| Post-conditions | • A printable document has been produced. Where the module records printing, the row is flagged as printed. |
| Realisation | `POST /api/Reports/family-update-tracking/export/pdf` → `ReportsController` → `IReportService` |

#### 23.U.22  UC-RPT-22 — Missed payments — current user scope أيتام مستحقون دفعات سابقة


| Item | Specification |
| --- | --- |
| Use case ID | UC-RPT-22 |
| Name | Missed payments — current user scope أيتام مستحقون دفعات سابقة |
| Type | Read a record |
| Primary actor | Gen. Director, charity |
| Secondary actors | The system (Web API + business layer); the database. |
| Summary | Orphans entitled to earlier batches who did not receive them, so arrears can be settled. |
| Trigger | The actor opens the screen at `#/reports/missed-payments` from the module menu. |
| Pre-conditions | 1. The actor is authenticated; the JWT access token is valid and the client holds the role, charity and country claims it carries.<br>2. The actor holds one of: Gen. Director, charity.<br>3. Caller identity comes from the JWT, never from the request; the application service scopes the query to the charity (and country) that owns the user, unless the role is an HQ role permitted to pass an explicit `charityId` filter.<br>4. The SPA route `#/reports/missed-payments` has loaded and its reference-data lookups have been populated.<br>5. The target record exists and its identifier is known to the screen. |
| Main flow | 1. The actor navigates to the screen at `#/reports/missed-payments`.<br>2. The actor selects the record to open.<br>3. The SPA issues `POST /api/Reports/missed-payments` carrying userId.<br>4. `ReportsController` binds the typed request DTO and delegates to the application service.<br>5. `IOrphanPaymentService` validates the payload with its FluentValidation validator, applies the business rules and the charity scope, and persists through `IUnitOfWork`.<br>6. The system returns the record with its child collections and the SPA binds them to the form fields. |
| Alternate flows | • An HQ role (General Director, Financial Director, Staff) may pass an explicit charity id and so read across the charity boundary; a charity user may not and always sees its own data. |
| Exception flows | • The session has expired or the role is not permitted — the request is rejected and the SPA routes back to the login state. |
| Post-conditions | • No stored data is changed — the operation is a read. |
| Realisation | Route `#/reports/missed-payments` → `ReportViewerComponent`<br>`POST /api/Reports/missed-payments` → `ReportsController` → `IReportService` |

#### 23.U.23  UC-RPT-23 — Missed payments — all جميع الدفعات الفائتة


| Item | Specification |
| --- | --- |
| Use case ID | UC-RPT-23 |
| Name | Missed payments — all جميع الدفعات الفائتة |
| Type | Read a record |
| Primary actor | Gen. Director |
| Secondary actors | The system (Web API + business layer); the database. |
| Summary | The organisation-wide arrears view across all charities. |
| Trigger | The actor selects the function from the module menu or the screen that hosts it. |
| Pre-conditions | 1. The actor is authenticated; the JWT access token is valid and the client holds the role, charity and country claims it carries.<br>2. The actor holds one of: Gen. Director.<br>3. Caller identity comes from the JWT, never from the request; the application service scopes the query to the charity (and country) that owns the user, unless the role is an HQ role permitted to pass an explicit `charityId` filter.<br>4. The target record exists and its identifier is known to the screen. |
| Main flow | 1. The actor reaches the function from the screen of this module that hosts it.<br>2. The actor selects the record to open.<br>3. The SPA issues `POST /api/Reports/missed-payments` carrying userId.<br>4. `ReportsController` binds the typed request DTO and delegates to the application service.<br>5. `IOrphanPaymentService` validates the payload with its FluentValidation validator, applies the business rules and the charity scope, and persists through `IUnitOfWork`.<br>6. The system returns the record with its child collections and the SPA binds them to the form fields. |
| Alternate flows | • An HQ role (General Director, Financial Director, Staff) may pass an explicit charity id and so read across the charity boundary; a charity user may not and always sees its own data. |
| Exception flows | • The session has expired or the role is not permitted — the request is rejected and the SPA routes back to the login state. |
| Post-conditions | • No stored data is changed — the operation is a read. |
| Realisation | `POST /api/Reports/missed-payments` → `ReportsController` → `IReportService` |

#### 23.U.24  UC-RPT-24 — Export orphan photographs صور الأيتام


| Item | Specification |
| --- | --- |
| Use case ID | UC-RPT-24 |
| Name | Export orphan photographs صور الأيتام |
| Type | Export data |
| Primary actor | All roles |
| Secondary actors | The system (Web API + business layer); the database. EPPlus as the worksheet writer. |
| Summary | For a date range and charity, exports the orphan photographs attached to accepted reports, for despatch to sponsors. |
| Trigger | The actor presses «ExportReportData» on the screen ExportImagesAndFiles. |
| Pre-conditions | 1. The actor is authenticated; the JWT access token is valid and the client holds the role, charity and country claims it carries.<br>2. The actor holds one of: All roles.<br>3. Caller identity comes from the JWT, never from the request; the application service scopes the query to the charity (and country) that owns the user, unless the role is an HQ role permitted to pass an explicit `charityId` filter.<br>4. The SPA route `#/reports/orphan-files` has loaded and its reference-data lookups have been populated. |
| Main flow | 1. The actor navigates to the screen at `#/reports/orphan-files`.<br>2. The actor sets the selection criteria for the extract.<br>3. The actor presses «ExportReportData» (ExportReportData()).<br>4. The SPA issues `POST /api/Reports/orphan-files/export` carrying DateFrom, DateTo, charityId.<br>5. `ReportsController` binds the typed request DTO and delegates to the application service.<br>6. `ISeasonalAidService` validates the payload with its FluentValidation validator, applies the business rules and the charity scope, and persists through `IUnitOfWork`.<br>7. The business layer builds the worksheet (EPPlus) from the selected rows and streams it to the browser as a download. |
| Alternate flows | • An HQ role (General Director, Financial Director, Staff) may pass an explicit charity id and so read across the charity boundary; a charity user may not and always sees its own data. |
| Exception flows | • The session has expired or the role is not permitted — the request is rejected and the SPA routes back to the login state. |
| Post-conditions | • A workbook has been delivered to the actor. No stored data is changed. |
| Realisation | Route `#/reports/orphan-files` → `ReportViewerComponent`<br>`POST /api/Reports/orphan-files/export` → `ReportsController` → `IReportService` |

#### 23.U.25  UC-RPT-25 — Export certificate images صور الشهادات


| Item | Specification |
| --- | --- |
| Use case ID | UC-RPT-25 |
| Name | Export certificate images صور الشهادات |
| Type | Export data |
| Primary actor | All roles |
| Secondary actors | The system (Web API + business layer); the database. EPPlus as the worksheet writer. |
| Summary | The same export restricted to certificate and supporting-document images. |
| Trigger | The actor selects the function from the module menu or the screen that hosts it. |
| Pre-conditions | 1. The actor is authenticated; the JWT access token is valid and the client holds the role, charity and country claims it carries.<br>2. The actor holds one of: All roles.<br>3. Caller identity comes from the JWT, never from the request; the application service scopes the query to the charity (and country) that owns the user, unless the role is an HQ role permitted to pass an explicit `charityId` filter. |
| Main flow | 1. The actor reaches the function from the screen of this module that hosts it.<br>2. The actor sets the selection criteria for the extract.<br>3. The actor presses the command button of the screen.<br>4. The SPA issues `POST /api/Reports/certificate-files/export` carrying DateFrom, DateTo, charityId.<br>5. `ReportsController` binds the typed request DTO and delegates to the application service.<br>6. `ISeasonalAidService` validates the payload with its FluentValidation validator, applies the business rules and the charity scope, and persists through `IUnitOfWork`.<br>7. The business layer builds the worksheet (EPPlus) from the selected rows and streams it to the browser as a download. |
| Alternate flows | • An HQ role (General Director, Financial Director, Staff) may pass an explicit charity id and so read across the charity boundary; a charity user may not and always sees its own data. |
| Exception flows | • The session has expired or the role is not permitted — the request is rejected and the SPA routes back to the login state. |
| Post-conditions | • A workbook has been delivered to the actor. No stored data is changed. |
| Realisation | `POST /api/Reports/certificate-files/export` → `ReportsController` → `IReportService` |

#### 23.U.26  UC-RPT-26 — Print the survey questionnaire طباعة الاستبانة


| Item | Specification |
| --- | --- |
| Use case ID | UC-RPT-26 |
| Name | Print the survey questionnaire طباعة الاستبانة |
| Type | Print / produce a document |
| Primary actor | All roles |
| Secondary actors | The system (Web API + business layer); the database. the report and export endpoints of `api/Reports` as the rendering path — jsPDF for PDF and ExcelJS for Excel. |
| Summary | Prints the blank field questionnaire used when surveying a household, and the widow-specific variant. |
| Trigger | The actor selects the function from the module menu or the screen that hosts it. |
| Pre-conditions | 1. The actor is authenticated; the JWT access token is valid and the client holds the role, charity and country claims it carries.<br>2. The actor holds one of: All roles.<br>3. The target record exists and its identifier is known to the screen. |
| Main flow | 1. The actor reaches the function from the screen of this module that hosts it.<br>2. The actor selects the record or the range to be printed.<br>3. The actor presses the command button of the screen.<br>4. The browser opens the print action of the MVC print controller.<br>5. `ReportsController` resolves the report key, applies the filter, and renders the document from the dataset the application service returns.<br>6. The rendered document is streamed back as a PDF for viewing and printing. |
| Alternate flows | None recorded. |
| Exception flows | • The session has expired or the role is not permitted — the request is rejected and the SPA routes back to the login state.<br>• The report definition or a required parameter is missing — the print action fails and no document is produced. |
| Post-conditions | • A printable document has been produced. Where the module records printing, the row is flagged as printed. |
| Realisation | Printing() / WidowPrinting() in index.html |

#### 23.U.27  UC-RPT-27 — Orphans in a payment batch أيتام الدفعة


| Item | Specification |
| --- | --- |
| Use case ID | UC-RPT-27 |
| Name | Orphans in a payment batch أيتام الدفعة |
| Type | Read a record |
| Primary actor | HQ roles, charity |
| Secondary actors | The system (Web API + business layer); the database. |
| Summary | All orphans included in a batch for a charity, with amounts and payment state. |
| Trigger | The actor selects the function from the module menu or the screen that hosts it. |
| Pre-conditions | 1. The actor is authenticated; the JWT access token is valid and the client holds the role, charity and country claims it carries.<br>2. The actor holds one of: HQ roles, charity.<br>3. Caller identity comes from the JWT, never from the request; the application service scopes the query to the charity (and country) that owns the user, unless the role is an HQ role permitted to pass an explicit `charityId` filter.<br>4. The target record exists and its identifier is known to the screen. |
| Main flow | 1. The actor reaches the function from the screen of this module that hosts it.<br>2. The actor selects the record to open.<br>3. The SPA issues `GET /api/OrphanPayments/{id}/details` carrying charityId, paymentId.<br>4. `PeriodicOrphanReportsController` binds the typed request DTO and delegates to the application service.<br>5. `IReportService` validates the payload with its FluentValidation validator, applies the business rules and the charity scope, and persists through `IUnitOfWork`.<br>6. The system returns the record with its child collections and the SPA binds them to the form fields. |
| Alternate flows | • An HQ role (General Director, Financial Director, Staff) may pass an explicit charity id and so read across the charity boundary; a charity user may not and always sees its own data. |
| Exception flows | • The session has expired or the role is not permitted — the request is rejected and the SPA routes back to the login state.<br>• The business layer returns «Faliure» and the operation is not applied. |
| Post-conditions | • No stored data is changed — the operation is a read. |
| Realisation | `GET /api/OrphanPayments/{id}/details` → `OrphanPaymentsController` → `IOrphanPaymentService` |

#### 23.U.28  UC-RPT-28 — Orphans receiving nothing أيتام لم يصرف لهم


| Item | Specification |
| --- | --- |
| Use case ID | UC-RPT-28 |
| Name | Orphans receiving nothing أيتام لم يصرف لهم |
| Type | Read a record |
| Primary actor | HQ roles |
| Secondary actors | The system (Web API + business layer); the database. |
| Summary | Orphans in a batch for whom no amount was disbursed, exposing gaps in the run. |
| Trigger | The actor selects the function from the module menu or the screen that hosts it. |
| Pre-conditions | 1. The actor is authenticated; the JWT access token is valid and the client holds the role, charity and country claims it carries.<br>2. The actor holds one of: HQ roles.<br>3. Caller identity comes from the JWT, never from the request; the application service scopes the query to the charity (and country) that owns the user, unless the role is an HQ role permitted to pass an explicit `charityId` filter.<br>4. The target record exists and its identifier is known to the screen. |
| Main flow | 1. The actor reaches the function from the screen of this module that hosts it.<br>2. The actor selects the record to open.<br>3. The SPA issues `POST /api/Reports/orphans-without-payment` carrying charityId, paymentId.<br>4. `PeriodicOrphanReportsController` binds the typed request DTO and delegates to the application service.<br>5. `IReportService` validates the payload with its FluentValidation validator, applies the business rules and the charity scope, and persists through `IUnitOfWork`.<br>6. The system returns the record with its child collections and the SPA binds them to the form fields. |
| Alternate flows | • An HQ role (General Director, Financial Director, Staff) may pass an explicit charity id and so read across the charity boundary; a charity user may not and always sees its own data. |
| Exception flows | • The session has expired or the role is not permitted — the request is rejected and the SPA routes back to the login state.<br>• The business layer returns «Faliure» and the operation is not applied. |
| Post-conditions | • No stored data is changed — the operation is a read. |
| Realisation | `POST /api/Reports/orphans-without-payment` → `ReportsController` → `IReportService` |

#### 23.U.29  UC-RPT-29 — Received / not received / stopped lists


| Item | Specification |
| --- | --- |
| Use case ID | UC-RPT-29 |
| Name | Received / not received / stopped lists |
| Type | Print / produce a document |
| Primary actor | HQ roles, charity |
| Secondary actors | The system (Web API + business layer); the database. the report and export endpoints of `api/Reports` as the rendering path — jsPDF for PDF and ExcelJS for Excel. |
| Summary | The three disbursement outcome lists for a batch and charity (see UC-PAY-18 to UC-PAY-20), available on screen and as printed sheets. |
| Trigger | The actor selects the function from the module menu or the screen that hosts it. |
| Pre-conditions | 1. The actor is authenticated; the JWT access token is valid and the client holds the role, charity and country claims it carries.<br>2. The actor holds one of: HQ roles, charity.<br>3. Caller identity comes from the JWT, never from the request; the application service scopes the query to the charity (and country) that owns the user, unless the role is an HQ role permitted to pass an explicit `charityId` filter.<br>4. The target record exists and its identifier is known to the screen. |
| Main flow | 1. The actor reaches the function from the screen of this module that hosts it.<br>2. The actor selects the record or the range to be printed.<br>3. The actor presses the command button of the screen.<br>4. The browser opens `POST /api/Reports/payments-received/export/pdf` with string charityId, string orpCheckBatchNo.<br>5. `ReportsController` resolves the report key, applies the filter, and renders the document from the dataset the application service returns.<br>6. The rendered document is streamed back as a PDF for viewing and printing. |
| Alternate flows | • An HQ role (General Director, Financial Director, Staff) may pass an explicit charity id and so read across the charity boundary; a charity user may not and always sees its own data. |
| Exception flows | • The session has expired or the role is not permitted — the request is rejected and the SPA routes back to the login state.<br>• The business layer returns «Faliure» and the operation is not applied.<br>• The report definition or a required parameter is missing — the print action fails and no document is produced. |
| Post-conditions | • A printable document has been produced. Where the module records printing, the row is flagged as printed. |
| Realisation | `POST /api/Reports/payments-received/export/pdf` → `ReportsController` → `IReportService` |

#### 23.U.30  UC-RPT-30 — Cheque numbers list أرقام الشيكات


| Item | Specification |
| --- | --- |
| Use case ID | UC-RPT-30 |
| Name | Cheque numbers list أرقام الشيكات |
| Type | Print / produce a document |
| Primary actor | Charity, Fin. Director |
| Secondary actors | The system (Web API + business layer); the database. the report and export endpoints of `api/Reports` as the rendering path — jsPDF for PDF and ExcelJS for Excel. |
| Summary | The cheque numbers issued for a batch in a charity, for handover and reconciliation. |
| Trigger | The actor selects the function from the module menu or the screen that hosts it. |
| Pre-conditions | 1. The actor is authenticated; the JWT access token is valid and the client holds the role, charity and country claims it carries.<br>2. The actor holds one of: charity, Fin. Director.<br>3. Caller identity comes from the JWT, never from the request; the application service scopes the query to the charity (and country) that owns the user, unless the role is an HQ role permitted to pass an explicit `charityId` filter.<br>4. The target record exists and its identifier is known to the screen. |
| Main flow | 1. The actor reaches the function from the screen of this module that hosts it.<br>2. The actor selects the record or the range to be printed.<br>3. The actor presses the command button of the screen.<br>4. The browser opens `POST /api/Reports/cheque-numbers/export/pdf` with string charityId, string orpCheckBatchNo.<br>5. `ReportsController` resolves the report key, applies the filter, and renders the document from the dataset the application service returns.<br>6. The rendered document is streamed back as a PDF for viewing and printing. |
| Alternate flows | • An HQ role (General Director, Financial Director, Staff) may pass an explicit charity id and so read across the charity boundary; a charity user may not and always sees its own data. |
| Exception flows | • The session has expired or the role is not permitted — the request is rejected and the SPA routes back to the login state.<br>• The business layer returns «Faliure» and the operation is not applied.<br>• The report definition or a required parameter is missing — the print action fails and no document is produced. |
| Post-conditions | • A printable document has been produced. Where the module records printing, the row is flagged as printed. |
| Realisation | `POST /api/Reports/cheque-numbers/export/pdf` → `ReportsController` → `IReportService` |

#### 23.U.31  UC-RPT-31 — Receipt cards كروت الاستلام


| Item | Specification |
| --- | --- |
| Use case ID | UC-RPT-31 |
| Name | Receipt cards كروت الاستلام |
| Type | Print / produce a document |
| Primary actor | Charity |
| Secondary actors | The system (Web API + business layer); the database. the report and export endpoints of `api/Reports` as the rendering path — jsPDF for PDF and ExcelJS for Excel. |
| Summary | The individually printed cards each guardian signs when collecting the payment. |
| Trigger | The actor selects the function from the module menu or the screen that hosts it. |
| Pre-conditions | 1. The actor is authenticated; the JWT access token is valid and the client holds the role, charity and country claims it carries.<br>2. The actor holds one of: charity.<br>3. Caller identity comes from the JWT, never from the request; the application service scopes the query to the charity (and country) that owns the user, unless the role is an HQ role permitted to pass an explicit `charityId` filter.<br>4. The target record exists and its identifier is known to the screen. |
| Main flow | 1. The actor reaches the function from the screen of this module that hosts it.<br>2. The actor selects the record or the range to be printed.<br>3. The actor presses the command button of the screen.<br>4. The browser opens `POST /api/Reports/receipt-cards/export/pdf` with string charityId, string orpCheckBatchNo.<br>5. `ReportsController` resolves the report key, applies the filter, and renders the document from the dataset the application service returns.<br>6. The rendered document is streamed back as a PDF for viewing and printing. |
| Alternate flows | • An HQ role (General Director, Financial Director, Staff) may pass an explicit charity id and so read across the charity boundary; a charity user may not and always sees its own data. |
| Exception flows | • The session has expired or the role is not permitted — the request is rejected and the SPA routes back to the login state.<br>• The business layer returns «Faliure» and the operation is not applied.<br>• The report definition or a required parameter is missing — the print action fails and no document is produced. |
| Post-conditions | • A printable document has been produced. Where the module records printing, the row is flagged as printed. |
| Realisation | `POST /api/Reports/receipt-cards/export/pdf` → `ReportsController` → `IReportService` |

#### 23.U.32  UC-RPT-32 — Payment summary pages صفحات ملخص الدفعة


| Item | Specification |
| --- | --- |
| Use case ID | UC-RPT-32 |
| Name | Payment summary pages صفحات ملخص الدفعة |
| Type | Read a record |
| Primary actor | HQ roles |
| Secondary actors | The system (Web API + business layer); the database. |
| Summary | Cover and summary sheets of a batch for a charity. |
| Trigger | The actor selects the function from the module menu or the screen that hosts it. |
| Pre-conditions | 1. The actor is authenticated; the JWT access token is valid and the client holds the role, charity and country claims it carries.<br>2. The actor holds one of: HQ roles.<br>3. Caller identity comes from the JWT, never from the request; the application service scopes the query to the charity (and country) that owns the user, unless the role is an HQ role permitted to pass an explicit `charityId` filter.<br>4. The target record exists and its identifier is known to the screen. |
| Main flow | 1. The actor reaches the function from the screen of this module that hosts it.<br>2. The actor selects the record to open.<br>3. The SPA issues `GET /api/Dashboard/payment-summary` carrying string paymentId, string charityId,string userId.<br>4. `AuthController` binds the typed request DTO and delegates to the application service.<br>5. `IOrphanPaymentService` validates the payload with its FluentValidation validator, applies the business rules and the charity scope, and persists through `IUnitOfWork`.<br>6. The system returns the record with its child collections and the SPA binds them to the form fields. |
| Alternate flows | • An HQ role (General Director, Financial Director, Staff) may pass an explicit charity id and so read across the charity boundary; a charity user may not and always sees its own data. |
| Exception flows | • The session has expired or the role is not permitted — the request is rejected and the SPA routes back to the login state. |
| Post-conditions | • No stored data is changed — the operation is a read. |
| Realisation | `GET /api/Dashboard/payment-summary` → `DashboardController` → `IDashboardService` |

#### 23.U.33  UC-RPT-33 — Cheque statement report بيان الشيكات


| Item | Specification |
| --- | --- |
| Use case ID | UC-RPT-33 |
| Name | Cheque statement report بيان الشيكات |
| Type | Print / produce a document |
| Primary actor | Fin. Director |
| Secondary actors | The system (Web API + business layer); the database. the report and export endpoints of `api/Reports` as the rendering path — jsPDF for PDF and ExcelJS for Excel. |
| Summary | Cheques filtered by bank, date range and type, rendered as the bank reconciliation statement. |
| Trigger | The actor selects the function from the module menu or the screen that hosts it. |
| Pre-conditions | 1. The actor is authenticated; the JWT access token is valid and the client holds the role, charity and country claims it carries.<br>2. The actor holds one of: Fin. Director.<br>3. The target record exists and its identifier is known to the screen. |
| Main flow | 1. The actor reaches the function from the screen of this module that hosts it.<br>2. The actor selects the record or the range to be printed.<br>3. The actor presses the command button of the screen.<br>4. The browser opens `POST /api/Reports/cheque-statement/export/pdf` with bankId, dateFrom, dateTo, checkType.<br>5. `ReportsController` resolves the report key, applies the filter, and renders the document from the dataset the application service returns.<br>6. The rendered document is streamed back as a PDF for viewing and printing. |
| Alternate flows | None recorded. |
| Exception flows | • The session has expired or the role is not permitted — the request is rejected and the SPA routes back to the login state.<br>• The report definition or a required parameter is missing — the print action fails and no document is produced. |
| Post-conditions | • A printable document has been produced. Where the module records printing, the row is flagged as printed. |
| Realisation | `POST /api/Reports/cheque-statement/export/pdf` → `ReportsController` → `IReportService` |

#### 23.U.34  UC-RPT-34 — Project distribution sheets كشوف توزيع المشاريع


| Item | Specification |
| --- | --- |
| Use case ID | UC-RPT-34 |
| Name | Project distribution sheets كشوف توزيع المشاريع |
| Type | Print / produce a document |
| Primary actor | Charity, HQ roles |
| Secondary actors | The system (Web API + business layer); the database. the report and export endpoints of `api/Reports` as the rendering path — jsPDF for PDF and ExcelJS for Excel. |
| Summary | Family cards and the primary/secondary distribution lists for an assistance project. |
| Trigger | The actor selects the function from the module menu or the screen that hosts it. |
| Pre-conditions | 1. The actor is authenticated; the JWT access token is valid and the client holds the role, charity and country claims it carries.<br>2. The actor holds one of: charity, HQ roles.<br>3. Caller identity comes from the JWT, never from the request; the application service scopes the query to the charity (and country) that owns the user, unless the role is an HQ role permitted to pass an explicit `charityId` filter.<br>4. The target record exists and its identifier is known to the screen. |
| Main flow | 1. The actor reaches the function from the screen of this module that hosts it.<br>2. The actor selects the record or the range to be printed.<br>3. The actor presses the command button of the screen.<br>4. The browser opens `POST /api/Reports/family-cards/export/pdf` with string charityId, string projectId, string userId.<br>5. `ReportsController` resolves the report key, applies the filter, and renders the document from the dataset the application service returns.<br>6. The rendered document is streamed back as a PDF for viewing and printing. |
| Alternate flows | • An HQ role (General Director, Financial Director, Staff) may pass an explicit charity id and so read across the charity boundary; a charity user may not and always sees its own data. |
| Exception flows | • The session has expired or the role is not permitted — the request is rejected and the SPA routes back to the login state.<br>• The business layer returns «Operation Faild» and the operation is not applied.<br>• The report definition or a required parameter is missing — the print action fails and no document is produced. |
| Post-conditions | • A printable document has been produced. Where the module records printing, the row is flagged as printed. |
| Realisation | `POST /api/Reports/family-cards/export/pdf` → `ReportsController` → `IReportService` |

#### 23.U.35  UC-RPT-35 — New orphans and new widows الأيتام والأرامل الجدد


| Item | Specification |
| --- | --- |
| Use case ID | UC-RPT-35 |
| Name | New orphans and new widows الأيتام والأرامل الجدد |
| Type | Print / produce a document |
| Primary actor | HQ roles |
| Secondary actors | The system (Web API + business layer); the database. the report and export endpoints of `api/Reports` as the rendering path — jsPDF for PDF and ExcelJS for Excel. |
| Summary | Printed lists of newly registered orphans and widows for a charity, used in sponsorship offers. |
| Trigger | The actor selects the function from the module menu or the screen that hosts it. |
| Pre-conditions | 1. The actor is authenticated; the JWT access token is valid and the client holds the role, charity and country claims it carries.<br>2. The actor holds one of: HQ roles.<br>3. The target record exists and its identifier is known to the screen. |
| Main flow | 1. The actor reaches the function from the screen of this module that hosts it.<br>2. The actor selects the record or the range to be printed.<br>3. The actor presses the command button of the screen.<br>4. The browser opens the print action of the MVC print controller.<br>5. `ReportsController` resolves the report key, applies the filter, and renders the document from the dataset the application service returns.<br>6. The rendered document is streamed back as a PDF for viewing and printing. |
| Alternate flows | None recorded. |
| Exception flows | • The session has expired or the role is not permitted — the request is rejected and the SPA routes back to the login state.<br>• The report definition or a required parameter is missing — the print action fails and no document is produced. |
| Post-conditions | • A printable document has been produced. Where the module records printing, the row is flagged as printed. |
| Realisation | rptNewOrphans.rpt, rptNewOrphansV2.rpt, rptNewWidows.rpt, rptNewWidows_Family.rpt |

#### 23.U.36  UC-RPT-36 — Follow-up and handover sheets كشوف المتابعة والتسليم


| Item | Specification |
| --- | --- |
| Use case ID | UC-RPT-36 |
| Name | Follow-up and handover sheets كشوف المتابعة والتسليم |
| Type | Query a report |
| Primary actor | HQ roles |
| Secondary actors | The system (Web API + business layer); the database. |
| Summary | The general follow-up sheet, the family follow-up sheet and the handover (delivery) sheet. |
| Trigger | The actor selects the function from the module menu or the screen that hosts it. |
| Pre-conditions | 1. The actor is authenticated; the JWT access token is valid and the client holds the role, charity and country claims it carries.<br>2. The actor holds one of: HQ roles. |
| Main flow | 1. The actor reaches the function from the screen of this module that hosts it.<br>2. The actor sets the report criteria (period, charity, country and the other filters the screen offers).<br>3. The actor runs the report.<br>4. The SPA calls the service endpoint that backs the function.<br>5. The Web API controller receives the request and delegates to the business layer.<br>6. The business layer executes the rules and the data access.<br>7. The business layer composes the projection, scoped to the caller’s charity and country.<br>8. The result is rendered in the on-screen grid, from where it can be printed or exported. |
| Alternate flows | • No row matches the criteria — the grid renders empty and the paging control reports zero pages. |
| Exception flows | • The session has expired or the role is not permitted — the request is rejected and the SPA routes back to the login state. |
| Post-conditions | • No stored data is changed — the operation is a read. |
| Realisation | rptFollowUp.rpt, rptFollowUpFamily.rpt, rptFollowUpTasleem.rpt |

#### 23.U.37  UC-RPT-37 — Guardian and widow identification sheets كشوف تعريف العائل والأرامل


| Item | Specification |
| --- | --- |
| Use case ID | UC-RPT-37 |
| Name | Guardian and widow identification sheets كشوف تعريف العائل والأرامل |
| Type | Print / produce a document |
| Primary actor | HQ roles, charity |
| Secondary actors | The system (Web API + business layer); the database. the report and export endpoints of `api/Reports` as the rendering path — jsPDF for PDF and ExcelJS for Excel. |
| Summary | Identification sheets for a charity at a date — for all guardians, for widows, or for a single family. |
| Trigger | The actor selects the function from the module menu or the screen that hosts it. |
| Pre-conditions | 1. The actor is authenticated; the JWT access token is valid and the client holds the role, charity and country claims it carries.<br>2. The actor holds one of: HQ roles, charity.<br>3. Caller identity comes from the JWT, never from the request; the application service scopes the query to the charity (and country) that owns the user, unless the role is an HQ role permitted to pass an explicit `charityId` filter.<br>4. The target record exists and its identifier is known to the screen. |
| Main flow | 1. The actor reaches the function from the screen of this module that hosts it.<br>2. The actor selects the record or the range to be printed.<br>3. The actor presses the command button of the screen.<br>4. The browser opens `POST /api/Reports/guardian-identification-sheets/export/pdf` with string charityId,DateTime? date, string userId.<br>5. `ReportsController` resolves the report key, applies the filter, and renders the document from the dataset the application service returns.<br>6. The rendered document is streamed back as a PDF for viewing and printing. |
| Alternate flows | • An HQ role (General Director, Financial Director, Staff) may pass an explicit charity id and so read across the charity boundary; a charity user may not and always sees its own data. |
| Exception flows | • The session has expired or the role is not permitted — the request is rejected and the SPA routes back to the login state.<br>• The report definition or a required parameter is missing — the print action fails and no document is produced. |
| Post-conditions | • A printable document has been produced. Where the module records printing, the row is flagged as printed. |
| Realisation | `POST /api/Reports/guardian-identification-sheets/export/pdf` → `ReportsController` → `IReportService` |

#### 23.U.38  UC-RPT-38 — Missing outgoing attachments مرفقات الصادر الناقصة


| Item | Specification |
| --- | --- |
| Use case ID | UC-RPT-38 |
| Name | Missing outgoing attachments مرفقات الصادر الناقصة |
| Type | Query a report |
| Primary actor | Staff |
| Secondary actors | The system (Web API + business layer); the database. |
| Summary | Orphan reports expected on an outgoing letter but not attached. |
| Trigger | The actor selects the function from the module menu or the screen that hosts it. |
| Pre-conditions | 1. The actor is authenticated; the JWT access token is valid and the client holds the role, charity and country claims it carries.<br>2. The actor holds one of: Staff. |
| Main flow | 1. The actor reaches the function from the screen of this module that hosts it.<br>2. The actor sets the report criteria (period, charity, country and the other filters the screen offers).<br>3. The actor runs the report.<br>4. The SPA calls the service endpoint that backs the function.<br>5. The Web API controller receives the request and delegates to the business layer.<br>6. The business layer executes the rules and the data access.<br>7. The business layer composes the projection, scoped to the caller’s charity and country.<br>8. The result is rendered in the on-screen grid, from where it can be printed or exported. |
| Alternate flows | • No row matches the criteria — the grid renders empty and the paging control reports zero pages. |
| Exception flows | • The session has expired or the role is not permitted — the request is rejected and the SPA routes back to the login state. |
| Post-conditions | • No stored data is changed — the operation is a read. |
| Realisation | rptChildOutGoingMissing.rpt |

#### 23.U.39  UC-RPT-39 — Family orphan list by date أيتام الأسر بتاريخ


| Item | Specification |
| --- | --- |
| Use case ID | UC-RPT-39 |
| Name | Family orphan list by date أيتام الأسر بتاريخ |
| Type | Print / produce a document |
| Primary actor | HQ roles |
| Secondary actors | The system (Web API + business layer); the database. the report and export endpoints of `api/Reports` as the rendering path — jsPDF for PDF and ExcelJS for Excel. |
| Summary | Prints the orphans of a charity's families as at a given date. |
| Trigger | The actor selects the function from the module menu or the screen that hosts it. |
| Pre-conditions | 1. The actor is authenticated; the JWT access token is valid and the client holds the role, charity and country claims it carries.<br>2. The actor holds one of: HQ roles.<br>3. Caller identity comes from the JWT, never from the request; the application service scopes the query to the charity (and country) that owns the user, unless the role is an HQ role permitted to pass an explicit `charityId` filter.<br>4. The target record exists and its identifier is known to the screen. |
| Main flow | 1. The actor reaches the function from the screen of this module that hosts it.<br>2. The actor selects the record or the range to be printed.<br>3. The actor presses the command button of the screen.<br>4. The browser opens `POST /api/Reports/family-orphans/export/pdf` with charityId, date.<br>5. `ReportsController` resolves the report key, applies the filter, and renders the document from the dataset the application service returns.<br>6. The rendered document is streamed back as a PDF for viewing and printing. |
| Alternate flows | • An HQ role (General Director, Financial Director, Staff) may pass an explicit charity id and so read across the charity boundary; a charity user may not and always sees its own data. |
| Exception flows | • The session has expired or the role is not permitted — the request is rejected and the SPA routes back to the login state.<br>• The report definition or a required parameter is missing — the print action fails and no document is produced. |
| Post-conditions | • A printable document has been produced. Where the module records printing, the row is flagged as printed. |
| Realisation | `POST /api/Reports/family-orphans/export/pdf` → `ReportsController` → `IReportService` |

#### 23.U.40  UC-RPT-40 — Display a report in the browser عرض التقرير


| Item | Specification |
| --- | --- |
| Use case ID | UC-RPT-40 |
| Name | Display a report in the browser عرض التقرير |
| Type | Query a report |
| Primary actor | All roles |
| Secondary actors | The system (Web API + business layer); the database. |
| Summary | Streams a generated PDF into an inline frame so the user can review before printing or saving. |
| Trigger | The actor selects the function from the module menu or the screen that hosts it. |
| Pre-conditions | 1. The actor is authenticated; the JWT access token is valid and the client holds the role, charity and country claims it carries.<br>2. The actor holds one of: All roles. |
| Main flow | 1. The actor reaches the function from the screen of this module that hosts it.<br>2. The actor sets the report criteria (period, charity, country and the other filters the screen offers).<br>3. The actor runs the report.<br>4. The SPA issues `POST /api/Reports/{reportKey}/export/pdf`.<br>5. The Web API controller receives the request and delegates to the business layer.<br>6. The business layer executes the rules and the data access.<br>7. The business layer composes the projection, scoped to the caller’s charity and country.<br>8. The result is rendered in the on-screen grid, from where it can be printed or exported. |
| Alternate flows | • No row matches the criteria — the grid renders empty and the paging control reports zero pages. |
| Exception flows | • The session has expired or the role is not permitted — the request is rejected and the SPA routes back to the login state. |
| Post-conditions | • No stored data is changed — the operation is a read. |
| Realisation | `POST /api/Reports/{reportKey}/export/pdf` → `ReportsController` → `IReportService` |

#### 23.U.41  UC-RPT-41 — Export a report to Excel تصدير إلى إكسل


| Item | Specification |
| --- | --- |
| Use case ID | UC-RPT-41 |
| Name | Export a report to Excel تصدير إلى إكسل |
| Type | Export data |
| Primary actor | All roles |
| Secondary actors | The system (Web API + business layer); the database. EPPlus as the worksheet writer. |
| Summary | Report grids are exported to spreadsheet format for offline analysis; server-side generation uses EPPlus, client-side export uses the bundled JSZip/FileSaver libraries. |
| Trigger | The actor selects the function from the module menu or the screen that hosts it. |
| Pre-conditions | 1. The actor is authenticated; the JWT access token is valid and the client holds the role, charity and country claims it carries.<br>2. The actor holds one of: All roles. |
| Main flow | 1. The actor reaches the function from the screen of this module that hosts it.<br>2. The actor sets the selection criteria for the extract.<br>3. The actor presses the command button of the screen.<br>4. The SPA calls the service endpoint that backs the function.<br>5. The Web API controller receives the request and delegates to the business layer.<br>6. The business layer executes the rules and the data access.<br>7. The business layer builds the worksheet (EPPlus) from the selected rows and streams it to the browser as a download. |
| Alternate flows | None recorded. |
| Exception flows | • The session has expired or the role is not permitted — the request is rejected and the SPA routes back to the login state. |
| Post-conditions | • A workbook has been delivered to the actor. No stored data is changed. |
| Realisation | EPPlus, Scripts/FileSaver.js, node_modules/jszip |

### 23.A  Annex - Angular routes of this module

Routes are hash-based (`useHash: true`), rendered inside `MainLayoutComponent` behind
`AuthGuard` and `PermissionGuard`. *implemented* means the route exists in
`Frontend/src/app`; *planned* means it is specified here and not yet built.

| Angular route | Feature module | Component | Status |
| --- | --- | --- | --- |
| `#/periodic-orphan-reports/orphan-reports/search` | `periodic-orphan-reports` | `OrphanReportSearchComponent` | implemented |
| `#/seasonal-aid/:id/report` | `seasonal-aid` | `CampaignReportComponent` | planned |
| `#/reports/orphans` | `reports` | `ReportViewerComponent` | planned |
| `#/reports/excluded-orphans` | `reports` | `ReportViewerComponent` | planned |
| `#/reports/finished-sponsorship-orphans` | `reports` | `ReportViewerComponent` | planned |
| `#/reports/unsponsored-orphans` | `reports` | `ReportViewerComponent` | planned |
| `#/reports/registered-family-projects` | `reports` | `ReportViewerComponent` | planned |
| `#/reports/meza-cards` | `reports` | `ReportViewerComponent` | planned |
| `#/reports/widows-allowing-sponsorship` | `reports` | `ReportViewerComponent` | planned |
| `#/reports/family-orphans` | `reports` | `ReportViewerComponent` | planned |
| `#/reports/missed-payments` | `reports` | `ReportViewerComponent` | planned |
| `#/reports/charity-payment-tracking` | `reports` | `ReportViewerComponent` | planned |
| `#/reports/beneficiary-family-details` | `reports` | `ReportViewerComponent` | planned |
| `#/reports/orphans-missing-reports` | `reports` | `ReportViewerComponent` | planned |
| `#/reports/orphans-missing-files` | `reports` | `ReportViewerComponent` | planned |
| `#/reports/reports-awaiting-approval` | `reports` | `ReportViewerComponent` | planned |
| `#/reports/refused-reports` | `reports` | `ReportViewerComponent` | planned |
| `#/reports/orphan-files` | `reports` | `ReportViewerComponent` | planned |
| `#/reports/provider-sponsor-changes` | `reports` | `ReportViewerComponent` | planned |

### 23.B  Annex - API controllers of this module

Every controller inherits `ApiController` (`[Authorize]`, route `api/[controller]`) and
returns `Framework.Core.ApiResponse<T>`. Business rules live in the application service
behind each controller, never in the controller itself.

| Controller | Base route | Responsibility |
| --- | --- | --- |
| `ReportsController` | `api/Reports` | Payment history per orphan; missed payments. Orphan master report and the compliance report family. Assistance family detail. Job catalogue; not-received, stopped and other-sponsor reports. |
| `OrphanReportsController` | `api/OrphanReports` | Detailed extracts: all, accepted, refused, non-renewed, report numbers, Meza cards, widows. |
| `PeriodicOrphanReportsController` | `api/PeriodicOrphanReports` | Refined orphan-status query. |

---

Shared context - system architecture, actors and roles, the sponsorship lifecycle, the full module map and all appendices - is in the companion document [00-Overview-and-Common-Context](00-Overview-and-Common-Context.md). The delivery backlog for this module is epic EP-18 in [00-EPICS-and-User-Stories](00-EPICS-and-User-Stories.md).

