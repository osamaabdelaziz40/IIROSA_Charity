# WAR.IIROSA - Orphan Payments & Disbursement

دفعات الايتام | use case prefix `UC-PAY` | chapter 15 of the master document


| Field | Value |
| --- | --- |
| Document title | WAR.IIROSA - Module Specification: Orphan Payments & Disbursement |
| Module | Orphan Payments & Disbursement - دفعات الايتام |
| Use case prefix | UC-PAY |
| Chapter in master document | Chapter 15 |
| Documented use cases | 24 |
| Principal routes | `#/orphan-payments`, `#/orphan-payments/create`, `#/orphan-payments/:id`, `#/orphan-payments/:id/edit`, `#/orphan-payments/:id/add-orphans`, `#/orphan-payments/:id/cheques` *(planned)*, `#/orphan-payments/:id/bank-file` *(planned)* |
| Version | 1.1 |
| Status | Chapter content extracted verbatim; screen fields and scenarios derived from the source code |
| Date | 18 August 2026 |
| Parent document | WAR.IIROSA-Project-Documentation.docx v1.0 (repository WAR.IIROSA, commit 4beb736) |

## Contents of this document

1. Chapter 15 — module purpose and use-case catalogue (verbatim from the master document)
2. §15.D — detailed specifications carried over from chapter 25
3. §15.S — screen field specifications (every field of every screen, derived from the AngularJS views)
4. §15.U — one expanded scenario per use case (trigger, pre-conditions, main flow, alternates, exceptions, post-conditions)
5. §15.A / §15.B — annexes: screens and Web API controllers of this module

## 15. Orphan Payments & Disbursement

دفعات الأيتام — the financial heart of the system. A payment batch enrols eligible orphans, is executed either by bank transfer or by cheque, and is closed by per-orphan receipt confirmation.

### 15.1 Payment row state model

Each orphan in a batch is represented by a Child_Payment row whose lifecycle is driven by four flags. The action codes below are the action parameter accepted by the row-update endpoints.


| Action | Flag(s) set | Meaning |
| --- | --- | --- |
| 0 | IsStopped | Suspend or resume the orphan's entitlement in this batch. Business rule: if the orphan is flagged RequestExcluded, the row is forced to stopped regardless of the value supplied by the operator. |
| 1 | IsPrinted | Mark the payment document (cheque or receipt card) as printed. |
| 2 | IsGotIt | Confirm the beneficiary received the money. |
| 3 | IsGotIt, IsPrinted, ChiqueNum, Printdate, BenificiaryName | Full settlement: receipt confirmed and the cheque number, print date and collecting beneficiary recorded together. |
| 4 | IsGotIt, IsPrinted, ChiqueNum, BenificiaryName | Settlement without a print date. |

### 15.2 Use cases


| ID | Use case | Primary actor | Description & main flow | Realisation |
| --- | --- | --- | --- | --- |
| UC-PAY-01 | List payment batches قائمة دفعات الأيتام | Gen. Director, Fin. Director, Staff | Paged list of all payment batches with their date, description, currency and totals, as the entry point to batch administration. | Route `#/orphan-payments` → GET /api/OrphanPayments |
| UC-PAY-02 | Create a payment batch اضافة دفعة مالية | Gen. Director, Fin. Director | Defines a new disbursement run — period, date, currency, amount rules and the charities in scope — and enrols the eligible orphans as payment rows. | Route `#/orphan-payments/create` → POST /api/OrphanPayments, then `#/orphan-payments/:id/add-orphans` to enrol the rows |
| UC-PAY-03 | View a payment batch عرض الدفعة | Gen. Director, Fin. Director, Staff | Loads a single batch header with its parameters for review or editing. | Route `#/orphan-payments/:id` → GET /api/OrphanPayments |
| UC-PAY-04 | Update a payment batch تعديل الدفعة | Gen. Director, Fin. Director | Amends the batch parameters before or during execution. | Route `#/orphan-payments/:id/edit` → PUT /api/OrphanPayments |
| UC-PAY-05 | Delete a payment batch حذف الدفعة | Gen. Director, Fin. Director | Removes a batch and its rows. Used for batches created in error before disbursement. | DELETE /api/OrphanPayments |
| UC-PAY-06 | List a charity's batch numbers أرقام الدفعات للجمعية | Charity, HQ roles | Returns the batch numbers in which the charity participates, used as the selector on every payment and reporting screen. | GET /api/OrphanPayments/by-batch-no/{batchNo} |
| UC-PAY-07 | View payment details for a charity تفاصيل الدفعة للجمعية | Charity, HQ roles | Lists every orphan payment row for one charity within one batch, with amount, stop/print/receipt flags and cheque data — the working screen for disbursement. | GET /api/OrphanPayments/{id}/details |
| UC-PAY-08 | List orphans in a batch الأيتام في الدفعة | Charity, HQ roles | Returns the orphan-level view of a batch for a charity, used by the receipt and printing screens. | GET /api/OrphanPayments/{id}/details |
| UC-PAY-09 | Stop or resume an orphan's payment إيقاف / استئناف صرف اليتيم | Charity, HQ roles | Suspends the orphan's entitlement in the batch (for example, unavailable, travelled, report refused) or resumes it. Orphans already requested for exclusion are forced to remain stopped. | POST /api/OrphanPayments/orphan-items |
| UC-PAY-10 | Mark a payment row printed تعليم كمطبوع | Charity | Flags that the cheque or receipt card for the row has been printed, so it is excluded from the next print run. | POST /api/OrphanPayments/orphan-items |
| UC-PAY-11 | Confirm receipt of a payment تأكيد الاستلام | Charity | Records that the guardian collected the money for that orphan, closing the row. | POST /api/OrphanPayments/orphan-items |
| UC-PAY-12 | Record cheque number, date and collector تسجيل رقم الشيك والمستلم | Charity | Settles the row in one step: confirms receipt, marks it printed, and stores the cheque number, print date and the name of the beneficiary who signed for it. A variant omits the print date. | POST /api/OrphanPayments/orphan-items |
| UC-PAY-13 | Update payment-detail flags تحديث بيانات الصرف | Charity, HQ roles | Generic row update used by the payment-detail grid to toggle the same flag set from the detail screen. | PUT /api/OrphanPayments/{id} |
| UC-PAY-14 | Generate the bank transfer file كشف التحويلات البنكية | Fin. Director, Gen. Director | Produces the file handed to the bank to execute the batch for one charity: one line per beneficiary with account/card details and amount. Three variants are available — the plain file, the merged file (rows consolidated per guardian), and the merged file carrying the orphan's full name. | Route `#/orphan-payments/:id/bank-file` → GET /api/OrphanPayments/{id}/export, getBankFileAfterMerge, getBankFileAfterMerge_FullChildName |
| UC-PAY-15 | Import transfer numbers from the bank رفع أرقام الحوالات | Fin. Director | Uploads the CSV returned by the bank containing the transfer reference generated for each beneficiary, and writes those references onto the matching payment rows of the batch. | POST /api/OrphanPayments/{id}/import/transfer-numbers |
| UC-PAY-16 | Import a batch reconciliation file رفع ملف الدفعة | Fin. Director | Uploads the batch CSV and applies its contents to the payment rows of the identified batch. | POST /api/OrphanPayments/{id}/import/bank-file |
| UC-PAY-17 | Import exchange (execution) statuses رفع حالات الصرف | Fin. Director | Uploads the bank's execution-status CSV so each payment row records whether the transfer succeeded, failed or is pending. | POST /api/OrphanPayments/{id}/import/exchange-status |
| UC-PAY-18 | Report payments received المستلمون | HQ roles, charity | Lists the orphans in a batch for a charity whose payment is confirmed received, for reconciliation and for printing the received list. | GET /api/OrphanPayments/{id}/details?received=true; GET /api/OrphanPayments/{id}/details |
| UC-PAY-19 | Report payments not received غير المستلمين | HQ roles, charity | The complement of the previous list — outstanding rows that must still be collected or returned. | POST /api/Reports/payments-not-received |
| UC-PAY-20 | Report stopped payments الموقوفون | HQ roles, charity | Lists the rows suspended in the batch together with the reason context, for HQ follow-up with the charity. | POST /api/Reports/payments-stopped |
| UC-PAY-21 | View batch summary pages ملخص الدفعة | HQ roles | Returns the paginated summary of a batch for a charity — totals, counts and per-page breakdown used as the cover sheet of the disbursement file. | GET /api/Dashboard/payment-summary |
| UC-PAY-22 | View batch transfers and cheques حوالات وشيكات الأيتام | HQ roles, charity | Lists the transfers generated for a batch and, separately, the cheques issued against orphan payments for a charity within a date or batch range. | Route `#/orphan-payments/:id/cheques` → GET /api/OrphanPayments/{id}/details, GET /api/CheckManagement?orphanPaymentId= |
| UC-PAY-23 | Identify orphans sponsored elsewhere أيتام لهم كافل آخر | HQ roles | For a charity and batch, lists orphans who also appear under another sponsoring body, so duplicate disbursement can be prevented. | POST /api/Reports/orphans-other-sponsor |
| UC-PAY-24 | Print disbursement documents طباعة مستندات الصرف | Charity, HQ roles | Produces the printed documents of a batch for a charity, through `POST /api/Reports/<report-key>/export/pdf`: the received list, the not-received list, the stopped list, the cheque-numbers list, and the signed receipt cards handed to guardians. | /api/Reports/payments-received/export/pdf, /api/Reports/payments-not-received/export/pdf, /api/Reports/payments-stopped/export/pdf, /api/Reports/cheque-numbers/export/pdf, /api/Reports/receipt-cards/export/pdf |

### 15.D  Detailed use case specifications (from chapter 25)

Reproduced verbatim from chapter 25 of the master document — the fully expanded specification of this module’s critical end-to-end scenarios. Every other use case of the module is specified in §15.U.

**25.6 UC-PAY-02 — Create and populate a payment batch**


| Use case ID | UC-PAY-02 |
| --- | --- |
| Name | Create a payment batch — اضافة دفعة مالية |
| Primary actor | General Director or Financial Director |
| Goal | Open a disbursement run and enrol the orphans entitled to it. |
| Pre-conditions | Orphans are coded; the reporting cycle for the period is closed or sufficiently advanced; the entitlement categories are configured. |
| Main flow | 1. The operator opens the orphanpayment state in add mode. 2. The operator defines the batch: period, date, description, currency, entitlement category (UC-SYS-10) and the charities in scope. 3. The operator saves; the client posts to POST /api/OrphanPayments. 4. IOrphanPaymentService creates the batch header and enrols the eligible orphans as Child_Payment rows with their amounts. 5. The batch appears in the batch list and its number becomes selectable by every charity in scope (UC-PAY-06). 6. HQ reviews the chase lists — orphans without a renewed report (UC-RPT-19) and orphans missing files (UC-RPT-16) — and stops the rows that should not be paid (UC-PAY-09). 7. Optionally HQ disables editing for the participating charities (UC-CHR-09) so the data behind the batch cannot change. |
| Alternate flows | A1 — Batch created in error. The operator deletes it before disbursement (UC-PAY-05). A2 — Parameters need adjustment. The operator updates the header (UC-PAY-04). |
| Post-conditions | A batch exists with one payment row per enrolled orphan, ready for bank-file generation or cheque issuance. |
| Business rules | BR-16 An orphan requested for exclusion is forced to stopped in every batch. BR-17 Amounts follow the batch's entitlement category. BR-18 Charities see only their own rows. |


**25.7 UC-PAY-14 — Generate the bank transfer file**


| Use case ID | UC-PAY-14 |
| --- | --- |
| Name | Generate the bank transfer file — كشف التحويلات البنكية |
| Primary actor | Financial Director |
| Goal | Produce the file the bank uses to execute the batch for one charity. |
| Pre-conditions | The batch exists with its rows; guardians have a registered payment card or account (UC-RPT-07); rows that must not be paid are already stopped. |
| Main flow | 1. The operator opens the BankFile state and selects the charity and the batch. 2. The operator chooses the file variant: plain, merged per guardian, or merged carrying the orphan's full name. 3. The client calls the corresponding endpoint on OrphanPaymentDetail. 4. The system assembles one line per beneficiary with the account or card number and the amount, excluding stopped rows. 5. The file is returned and saved locally, then transmitted to the bank. 6. When the bank responds, the operator imports the transfer numbers (UC-PAY-15) and, once executed, the exchange statuses (UC-PAY-17). 7. Charities then confirm receipt per orphan (UC-PAY-11, UC-PAY-12). |
| Alternate flows | A1 — Missing card or account. The affected guardians are identified through the card extract (UC-RPT-08) and either registered or moved to cheque disbursement. A2 — Cheque disbursement. Instead of a bank file, cheques are printed for the batch (UC-RPT-30, UC-RPT-31). |
| Post-conditions | A transfer file exists for the charity and batch; after import, each row carries its bank transfer reference and execution status. |
| Business rules | BR-19 Stopped rows are excluded. BR-20 The merged variants consolidate several orphans of one guardian into a single transfer line. |


**25.8 UC-PAY-12 — Confirm receipt and record the cheque**


| Use case ID | UC-PAY-12 |
| --- | --- |
| Name | Confirm receipt and record the cheque — تسجيل الاستلام ورقم الشيك |
| Primary actor | Charity user |
| Goal | Close the disbursement record for one orphan with evidence of who collected the money. |
| Pre-conditions | The batch is executed; the guardian has attended and signed the receipt card. |
| Main flow | 1. The user opens the payment details for the batch (UC-PAY-07). 2. The user locates the orphan's row. 3. The user enters the cheque number, the print date and the name of the person who collected. 4. The client calls the row-update endpoint with action 3. 5. The system sets IsGotIt and IsPrinted to true and stores the cheque number, print date and beneficiary name. 6. The row moves from the not-received list to the received list (UC-PAY-18, UC-PAY-19). |
| Alternate flows | A1 — No print date. Action 4 is used, storing everything except the print date. A2 — Simple confirmation. Action 2 confirms receipt without cheque details. A3 — Not collected. The row is left outstanding and appears on the not-received list, or is stopped (UC-PAY-09). |
| Post-conditions | The row is settled with an audit trail of cheque number, date and collector. |
| Business rules | BR-21 A row flagged for exclusion cannot be un-stopped. BR-22 Receipt confirmation is the charity's responsibility and is visible to HQ through the tracking report (UC-RPT-20). |


### 15.S  Screen field specifications

Derived from the AngularJS views of this module. For every screen the table lists each data-entry field in the order it appears, the scope property it binds to, its control type, whether the form marks it mandatory, and the lookup or rule that governs it. Labels are reproduced as they appear on screen (Arabic, right-to-left).

#### 15.S.1  Screen `#/orphan-payments`


| Property | Value |
| --- | --- |
| Angular route | `#/orphan-payments` |
| Feature module | `orphan-payments` (lazy-loaded) |
| Component | `OrphanPaymentListComponent` |
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
| payment in OrphanPayments track by $index | الرقم · رقم الدفعة · اسم الدفعة · الفترة · التاريخ · سعر الصرف · تعديل |

Commands on this screen:

| Command | Handler | Shown when |
| --- | --- | --- |
| حفظ | DeleteOrhPayment() | always |
| (icon only) | AddOrpPayment() | always |
| (icon only) | EditOrpPayment(payment.Id) | always |
| (icon only) | DeleteAnViewModel(payment.Id) | always |
| (icon only) | GetNext() | always |
| (icon only) | GetPrev() | always |

#### 15.S.2  Screen `#/orphan-payments/create` and `#/orphan-payments/:id/edit`


| Property | Value |
| --- | --- |
| Angular route | `#/orphan-payments/create` and `#/orphan-payments/:id/edit` (one component, both routes) |
| Feature module | `orphan-payments` (lazy-loaded) |
| Component | `OrphanPaymentFormComponent` |
| Route status | implemented |
| Data-entry fields | 11 |
| Grids on the screen | 0 |
| Commands | 1 |


| Section | Field (as labelled) | Bound to | Control | Mandatory · source · rules |
| --- | --- | --- | --- | --- |
| — | الجمعية | CharityId | Drop-down list | Optional · options: lookup: Charities (+ كافة الجهات) · on change: getCharityData() |
| إضافة إلي دفعات الآيتام | رقم الدفعة | Batch | Drop-down list | Mandatory · options: lookup: Batchs |
| إضافة إلي دفعات الآيتام | اسم الدفعة | BatchName | Text box | Mandatory |
| إضافة إلي دفعات الآيتام | الفترة الى | PeriodTo | Text box | Mandatory |
| إضافة إلي دفعات الآيتام | الفترة من | PeriodFrom | Text box | Mandatory |
| إضافة إلي دفعات الآيتام | تاريخ بدء التوزيع | PaymentDate | Date picker | Mandatory |
| إضافة إلي دفعات الآيتام | سعر الصرف | Paymentprice | Text box | Optional |
| إضافة إلي دفعات الآيتام | عدم خصم النسبه | DontRemoveRate | Check box | Optional |
| إضافة إلي دفعات الآيتام | (unlabelled) | File | File upload | Optional · accepts .xlsx |
| إضافة إلي دفعات الآيتام | (unlabelled) | FileTranfer | File upload | Optional · accepts .xlsx |
| إضافة إلي دفعات الآيتام | (unlabelled) | FileExchangestatus | File upload | Optional · accepts .xlsx |

Commands on this screen:

| Command | Handler | Shown when |
| --- | --- | --- |
| حفظ | AddOrPayment() | always |

#### 15.S.3  Screen `#/orphan-payments/:id/cheques`


| Property | Value |
| --- | --- |
| Angular route | `#/orphan-payments/:id/cheques` |
| Feature module | `orphan-payments` (lazy-loaded) |
| Component | `OrphanPaymentChequesComponent` |
| Route status | planned |
| Data-entry fields | 6 |
| Grids on the screen | 1 |
| Commands | 22 |


| Section | Field (as labelled) | Bound to | Control | Mandatory · source · rules |
| --- | --- | --- | --- | --- |
| — | إسم الجمعية | OrpCheckBatchNo | Drop-down list | Optional · options: lookup: Batches · on change: getCharityData() |
| — | إسم الجمعية | CharityId | Drop-down list | Optional · options: lookup: Charities (+ كافة الجهات) · on change: getCharityData() |
| دفعات الايتام | رقم اول شيك | OrpCheckBank | Drop-down list | Optional · options: lookup: Banks · on change: GetBankInfo() |
| دفعات الايتام | عدد الشيكات المطبوعة | OrpCheckFirstCheck | Numeric box | Optional |
| دفعات الايتام | تاريخ صرف الشيك | OrpCheckPrintedCheckNo | Numeric box | Optional |
| دفعات الايتام | من فضلك ادخل جميع البيانات | OrpCheckCheckdate | Date picker | Optional · on change: ParentBDate.date=dt.toISOString() |

Grids on this screen:

| Grid (row source) | Columns |
| --- | --- |
| orhan in OrphanChecks track by $index | وقف الصرف · رقم الشيك · كود اليتيم · اسم اليتيم · اسم المعيل · المبلغ الاجمالى · المبلغ المستلم · تمت الطباعة · إستلم · رقم الشيك · تاريخ الشيك · تم الصرف · تاريخ الصرف |

Commands on this screen:

| Command | Handler | Shown when |
| --- | --- | --- |
| متابعة الايتام - لم يتم الصرف | GetRecievedPaymentDetails() | always |
| تقرير بيانات غير المستلمين | GetGotItNotPaymentDetails() | always |
| تقرير بيانات المستلمين | GetGotItPaymentDetails() | always |
| ملخص صفحات كشف الصرف | GetPaymentSummeryPages() | always |
| طباعة الموقوفين | PrintStopped() | always |
| تقرير الموقوفين اكسل | GetGotItPaymentDetails_Stopped() | always |
| طباعة ارقام الشيكات | PrintChecksNumber() | always |
| طباعة كشف التسليم | PrintRecived() | always |
| طباعة غير المستلم | PrintNotRecived() | always |
| كشف التحويلات | DownloadChecks() | always |
| طباعة الشيكات | ViewModalOfPrintChecks() | IsPermitted |
| كروت التسليم | PrintRecieveCards() | always |
| حفظ بنك مصر | AddOrhCheckEgypt() | AddOfModel |
| حفظ | AddOrhCheck() | AddOfModel |
| طباعة | PrintCheck() | PrintOfModel |
| (icon only) | Sort('FatherFullName',1) | always |
| (icon only) | Sort('MotherFullName',2) | always |
| (icon only) | Sort('NoOfChildren',3) | always |
| (icon only) | Update(0,this) | always |
| (icon only) | Update(1,this) | always |
| (icon only) | Update(2,this) | always |
| (icon only) | Update(3,this) | always |

#### 15.S.4  Screen `#/orphan-payments/:id/bank-file`


| Property | Value |
| --- | --- |
| Angular route | `#/orphan-payments/:id/bank-file` |
| Feature module | `orphan-payments` (lazy-loaded) |
| Component | `BankFileComponent` |
| Route status | planned |
| Data-entry fields | 5 |
| Grids on the screen | 0 |
| Commands | 5 |


| Section | Field (as labelled) | Bound to | Control | Mandatory · source · rules |
| --- | --- | --- | --- | --- |
| — | البنك | OrpCheckBatchNo | Drop-down list | Optional · options: lookup: Batches · on change: getCharityData() |
| دفعات الايتام | رقم اول شيك | OrpCheckBank | Drop-down list | Optional · options: lookup: Banks · on change: GetBankInfo() |
| دفعات الايتام | عدد الشيكات المطبوعة | OrpCheckFirstCheck | Numeric box | Optional |
| دفعات الايتام | تاريخ صرف الشيك | OrpCheckPrintedCheckNo | Numeric box | Optional |
| دفعات الايتام | من فضلك ادخل جميع البيانات | OrpCheckCheckdate | Date picker | Optional · on change: ParentBDate.date=dt.toISOString() |

Commands on this screen:

| Command | Handler | Shown when |
| --- | --- | --- |
| كشف التحويلات | getCharityData() | always |
| كشف التحويلات بعد الدمج ( كرت ميزه ) | getCharityDataAfterMerge() | always |
| كشف التحويلات بعد الدمج ( تحويلات ) | getCharityDataAfterMerge() | always |
| حفظ | AddOrhCheck() | AddOfModel |
| طباعة | PrintCheck() | PrintOfModel |

### 15.U  Use case scenarios

One expanded scenario for every use case of this module. Pre-conditions, flows and post-conditions are derived from the screen that hosts the function, the Web API action that serves it and the business-layer method that executes it; quoted messages are the literal strings returned by the business layer.

#### 15.U.1  UC-PAY-01 — List payment batches قائمة دفعات الأيتام


| Item | Specification |
| --- | --- |
| Use case ID | UC-PAY-01 |
| Name | List payment batches قائمة دفعات الأيتام |
| Type | Browse a list |
| Primary actor | Gen. Director, Fin. Director, Staff |
| Secondary actors | The system (Web API + business layer); the database. |
| Summary | Paged list of all payment batches with their date, description, currency and totals, as the entry point to batch administration. |
| Trigger | The actor opens the screen at `#/orphan-payments` from the module menu. |
| Pre-conditions | 1. The actor is authenticated; the JWT access token is valid and the client holds the role, charity and country claims it carries.<br>2. The actor holds one of: Gen. Director, Fin. Director, Staff.<br>3. Caller identity comes from the JWT, never from the request; the application service scopes the query to the charity (and country) that owns the user, unless the role is an HQ role permitted to pass an explicit `charityId` filter.<br>4. The SPA route `#/orphan-payments` has loaded and its reference-data lookups have been populated. |
| Main flow | 1. The actor navigates to the screen at `#/orphan-payments`.<br>2. The system loads the filter lookups the screen offers and, for HQ roles, the charity selector.<br>3. The SPA issues `GET /api/OrphanPayments` carrying pagenum, userId.<br>4. `OrphanPaymentsController` binds the typed request DTO and delegates to the application service.<br>5. `IOrphanPaymentService` validates the payload with its FluentValidation validator, applies the business rules and the charity scope, and persists through `IUnitOfWork`.<br>6. The result set is scoped to the caller’s charity and country, ordered and paged.<br>7. The SPA renders the page with the columns listed in the screen specification and the paging control. |
| Alternate flows | • An HQ role (General Director, Financial Director, Staff) may pass an explicit charity id and so read across the charity boundary; a charity user may not and always sees its own data.<br>• No row matches the criteria — the grid renders empty and the paging control reports zero pages. |
| Exception flows | • The session has expired or the role is not permitted — the request is rejected and the SPA routes back to the login state. |
| Post-conditions | • No stored data is changed — the operation is a read. |
| Realisation | Route `#/orphan-payments` → `OrphanPaymentListComponent`<br>`GET /api/OrphanPayments` → `OrphanPaymentsController` → `IOrphanPaymentService` |

#### 15.U.2  UC-PAY-02 — Create a payment batch اضافة دفعة مالية


| Item | Specification |
| --- | --- |
| Use case ID | UC-PAY-02 |
| Name | Create a payment batch اضافة دفعة مالية |
| Type | Create a record |
| Primary actor | Gen. Director, Fin. Director |
| Secondary actors | The system (Web API + business layer); the database. |
| Summary | Defines a new disbursement run — period, date, currency, amount rules and the charities in scope — and enrols the eligible orphans as payment rows. |
| Trigger | The actor presses «حفظ» on the screen orphanpayment. |
| Pre-conditions | 1. The actor is authenticated; the JWT access token is valid and the client holds the role, charity and country claims it carries.<br>2. The actor holds one of: Gen. Director, Fin. Director.<br>3. The SPA route `#/orphan-payments/:id/edit` has loaded and its reference-data lookups have been populated. |
| Main flow | 1. The actor navigates to the screen at `#/orphan-payments/:id/edit`. The screen opens in add mode.<br>2. The system loads the reference data the form needs (regions, centres, banks, types and the other lookup lists bound to the drop-downs).<br>3. The actor completes the input fields. Mandatory fields: رقم الدفعة، اسم الدفعة، الفترة الى، الفترة من، تاريخ بدء التوزيع.<br>4. The actor presses «حفظ» (AddOrPayment()).<br>5. The SPA issues `POST /api/OrphanPayments` carrying typed request DTO obj.<br>6. `OrphanPaymentsController` binds the typed request DTO and delegates to the application service.<br>7. `IOrphanPaymentService` validates the payload with its FluentValidation validator, applies the business rules and the charity scope, and persists through `IUnitOfWork`.<br>8. The business layer validates the payload, stamps the owning charity and the creating user, and persists the new record.<br>9. The system returns the outcome and the SPA confirms the save and returns to the list screen. |
| Alternate flows | • The actor abandons the form before saving — nothing is written and the record keeps its previous state. |
| Exception flows | • The session has expired or the role is not permitted — the request is rejected and the SPA routes back to the login state.<br>• A mandatory field is empty or fails its format check — the save is refused and the field is flagged on the form. |
| Post-conditions | • A new record exists, owned by the charity of the creating user, and appears in the list screen of the module. |
| Realisation | Route `#/orphan-payments/create` → `OrphanPaymentFormComponent`<br>`POST /api/OrphanPayments` → `OrphanPaymentsController` → `IOrphanPaymentService` |

#### 15.U.3  UC-PAY-03 — View a payment batch عرض الدفعة


| Item | Specification |
| --- | --- |
| Use case ID | UC-PAY-03 |
| Name | View a payment batch عرض الدفعة |
| Type | Read a record |
| Primary actor | Gen. Director, Fin. Director, Staff |
| Secondary actors | The system (Web API + business layer); the database. |
| Summary | Loads a single batch header with its parameters for review or editing. |
| Trigger | The actor selects the function from the module menu or the screen that hosts it. |
| Pre-conditions | 1. The actor is authenticated; the JWT access token is valid and the client holds the role, charity and country claims it carries.<br>2. The actor holds one of: Gen. Director, Fin. Director, Staff.<br>3. Caller identity comes from the JWT, never from the request; the application service scopes the query to the charity (and country) that owns the user, unless the role is an HQ role permitted to pass an explicit `charityId` filter.<br>4. The target record exists and its identifier is known to the screen. |
| Main flow | 1. The actor reaches the function from the screen of this module that hosts it.<br>2. The actor selects the record to open.<br>3. The SPA issues `GET /api/OrphanPayments` carrying id, userId.<br>4. `OrphanPaymentsController` binds the typed request DTO and delegates to the application service.<br>5. `IOrphanPaymentService` validates the payload with its FluentValidation validator, applies the business rules and the charity scope, and persists through `IUnitOfWork`.<br>6. The system returns the record with its child collections and the SPA binds them to the form fields. |
| Alternate flows | • An HQ role (General Director, Financial Director, Staff) may pass an explicit charity id and so read across the charity boundary; a charity user may not and always sees its own data. |
| Exception flows | • The session has expired or the role is not permitted — the request is rejected and the SPA routes back to the login state. |
| Post-conditions | • No stored data is changed — the operation is a read. |
| Realisation | `GET /api/OrphanPayments` → `OrphanPaymentsController` → `IOrphanPaymentService` |

#### 15.U.4  UC-PAY-04 — Update a payment batch تعديل الدفعة


| Item | Specification |
| --- | --- |
| Use case ID | UC-PAY-04 |
| Name | Update a payment batch تعديل الدفعة |
| Type | Update a record |
| Primary actor | Gen. Director, Fin. Director |
| Secondary actors | The system (Web API + business layer); the database. |
| Summary | Amends the batch parameters before or during execution. |
| Trigger | The actor selects the function from the module menu or the screen that hosts it. |
| Pre-conditions | 1. The actor is authenticated; the JWT access token is valid and the client holds the role, charity and country claims it carries.<br>2. The actor holds one of: Gen. Director, Fin. Director.<br>3. The target record exists and its identifier is known to the screen. |
| Main flow | 1. The actor reaches the function from the screen of this module that hosts it. The screen opens in edit mode on the selected record.<br>2. The system loads the current values into the form.<br>3. The actor changes the fields to be corrected.<br>4. The actor presses the command button of the screen.<br>5. The SPA issues `PUT /api/OrphanPayments` carrying typed request DTO obj.<br>6. `OrphanPaymentsController` binds the typed request DTO and delegates to the application service.<br>7. `IOrphanPaymentService` validates the payload with its FluentValidation validator, applies the business rules and the charity scope, and persists through `IUnitOfWork`.<br>8. The business layer re-validates the payload, applies the changes and persists them.<br>9. The system returns the outcome and the SPA refreshes the screen. |
| Alternate flows | • The actor abandons the form before saving — nothing is written and the record keeps its previous state. |
| Exception flows | • The session has expired or the role is not permitted — the request is rejected and the SPA routes back to the login state.<br>• A mandatory field is empty or fails its format check — the save is refused and the field is flagged on the form. |
| Post-conditions | • The stored record carries the new values; no other record is affected. |
| Realisation | `PUT /api/OrphanPayments` → `OrphanPaymentsController` → `IOrphanPaymentService` |

#### 15.U.5  UC-PAY-05 — Delete a payment batch حذف الدفعة


| Item | Specification |
| --- | --- |
| Use case ID | UC-PAY-05 |
| Name | Delete a payment batch حذف الدفعة |
| Type | Delete a record |
| Primary actor | Gen. Director, Fin. Director |
| Secondary actors | The system (Web API + business layer); the database. |
| Summary | Removes a batch and its rows. Used for batches created in error before disbursement. |
| Trigger | The actor selects the function from the module menu or the screen that hosts it. |
| Pre-conditions | 1. The actor is authenticated; the JWT access token is valid and the client holds the role, charity and country claims it carries.<br>2. The actor holds one of: Gen. Director, Fin. Director.<br>3. Caller identity comes from the JWT, never from the request; the application service scopes the query to the charity (and country) that owns the user, unless the role is an HQ role permitted to pass an explicit `charityId` filter.<br>4. The target record exists and its identifier is known to the screen. |
| Main flow | 1. The actor reaches the function from the screen of this module that hosts it.<br>2. The actor selects the record and requests its deletion.<br>3. The SPA asks the actor to confirm.<br>4. The SPA issues `DELETE /api/OrphanPayments` carrying id, userId.<br>5. `OrphanPaymentsController` binds the typed request DTO and delegates to the application service.<br>6. `IOrphanPaymentService` validates the payload with its FluentValidation validator, applies the business rules and the charity scope, and persists through `IUnitOfWork`.<br>7. The business layer checks that the record may still be removed and deletes it (or marks it removed).<br>8. The system returns the outcome and the SPA drops the row from the grid. |
| Alternate flows | • An HQ role (General Director, Financial Director, Staff) may pass an explicit charity id and so read across the charity boundary; a charity user may not and always sees its own data. |
| Exception flows | • The session has expired or the role is not permitted — the request is rejected and the SPA routes back to the login state. |
| Post-conditions | • The record is no longer returned by the list and read endpoints of the module. |
| Realisation | `DELETE /api/OrphanPayments` → `OrphanPaymentsController` → `IOrphanPaymentService` |

#### 15.U.6  UC-PAY-06 — List a charity's batch numbers أرقام الدفعات للجمعية


| Item | Specification |
| --- | --- |
| Use case ID | UC-PAY-06 |
| Name | List a charity's batch numbers أرقام الدفعات للجمعية |
| Type | Browse a list |
| Primary actor | Charity, HQ roles |
| Secondary actors | The system (Web API + business layer); the database. |
| Summary | Returns the batch numbers in which the charity participates, used as the selector on every payment and reporting screen. |
| Trigger | The actor selects the function from the module menu or the screen that hosts it. |
| Pre-conditions | 1. The actor is authenticated; the JWT access token is valid and the client holds the role, charity and country claims it carries.<br>2. The actor holds one of: charity, HQ roles.<br>3. Caller identity comes from the JWT, never from the request; the application service scopes the query to the charity (and country) that owns the user, unless the role is an HQ role permitted to pass an explicit `charityId` filter. |
| Main flow | 1. The actor reaches the function from the screen of this module that hosts it.<br>2. The system loads the filter lookups the screen offers and, for HQ roles, the charity selector.<br>3. The SPA issues `GET /api/OrphanPayments/by-batch-no/{batchNo}` carrying id, userId.<br>4. `OrphanPaymentsController` binds the typed request DTO and delegates to the application service.<br>5. `IOrphanPaymentService` validates the payload with its FluentValidation validator, applies the business rules and the charity scope, and persists through `IUnitOfWork`.<br>6. The result set is scoped to the caller’s charity and country, ordered and paged.<br>7. The SPA renders the page and the paging control. |
| Alternate flows | • An HQ role (General Director, Financial Director, Staff) may pass an explicit charity id and so read across the charity boundary; a charity user may not and always sees its own data.<br>• No row matches the criteria — the grid renders empty and the paging control reports zero pages. |
| Exception flows | • The session has expired or the role is not permitted — the request is rejected and the SPA routes back to the login state.<br>• The business layer returns «Faliure» and the operation is not applied. |
| Post-conditions | • No stored data is changed — the operation is a read. |
| Realisation | `GET /api/OrphanPayments/by-batch-no/{batchNo}` → `OrphanPaymentsController` → `IOrphanPaymentService` |

#### 15.U.7  UC-PAY-07 — View payment details for a charity تفاصيل الدفعة للجمعية


| Item | Specification |
| --- | --- |
| Use case ID | UC-PAY-07 |
| Name | View payment details for a charity تفاصيل الدفعة للجمعية |
| Type | Read a record |
| Primary actor | Charity, HQ roles |
| Secondary actors | The system (Web API + business layer); the database. |
| Summary | Lists every orphan payment row for one charity within one batch, with amount, stop/print/receipt flags and cheque data — the working screen for disbursement. |
| Trigger | The actor selects the function from the module menu or the screen that hosts it. |
| Pre-conditions | 1. The actor is authenticated; the JWT access token is valid and the client holds the role, charity and country claims it carries.<br>2. The actor holds one of: charity, HQ roles.<br>3. Caller identity comes from the JWT, never from the request; the application service scopes the query to the charity (and country) that owns the user, unless the role is an HQ role permitted to pass an explicit `charityId` filter.<br>4. The target record exists and its identifier is known to the screen. |
| Main flow | 1. The actor reaches the function from the screen of this module that hosts it.<br>2. The actor selects the record to open.<br>3. The SPA issues `GET /api/OrphanPayments/{id}/details` carrying charityId, paymentId, userId.<br>4. `OrphanPaymentsController` binds the typed request DTO and delegates to the application service.<br>5. `IOrphanPaymentService` validates the payload with its FluentValidation validator, applies the business rules and the charity scope, and persists through `IUnitOfWork`.<br>6. The system returns the record with its child collections and the SPA binds them to the form fields. |
| Alternate flows | • An HQ role (General Director, Financial Director, Staff) may pass an explicit charity id and so read across the charity boundary; a charity user may not and always sees its own data. |
| Exception flows | • The session has expired or the role is not permitted — the request is rejected and the SPA routes back to the login state.<br>• The business layer returns «Faild Operation» and the operation is not applied. |
| Post-conditions | • No stored data is changed — the operation is a read. |
| Realisation | `GET /api/OrphanPayments/{id}/details` → `OrphanPaymentsController` → `IOrphanPaymentService` |

#### 15.U.8  UC-PAY-08 — List orphans in a batch الأيتام في الدفعة


| Item | Specification |
| --- | --- |
| Use case ID | UC-PAY-08 |
| Name | List orphans in a batch الأيتام في الدفعة |
| Type | Browse a list |
| Primary actor | Charity, HQ roles |
| Secondary actors | The system (Web API + business layer); the database. |
| Summary | Returns the orphan-level view of a batch for a charity, used by the receipt and printing screens. |
| Trigger | The actor selects the function from the module menu or the screen that hosts it. |
| Pre-conditions | 1. The actor is authenticated; the JWT access token is valid and the client holds the role, charity and country claims it carries.<br>2. The actor holds one of: charity, HQ roles.<br>3. Caller identity comes from the JWT, never from the request; the application service scopes the query to the charity (and country) that owns the user, unless the role is an HQ role permitted to pass an explicit `charityId` filter. |
| Main flow | 1. The actor reaches the function from the screen of this module that hosts it.<br>2. The system loads the filter lookups the screen offers and, for HQ roles, the charity selector.<br>3. The SPA issues `GET /api/OrphanPayments/{id}/details` carrying paymentId, charityId, userId.<br>4. `OrphanPaymentsController` binds the typed request DTO and delegates to the application service.<br>5. `IOrphanPaymentService` validates the payload with its FluentValidation validator, applies the business rules and the charity scope, and persists through `IUnitOfWork`.<br>6. The result set is scoped to the caller’s charity and country, ordered and paged.<br>7. The SPA renders the page and the paging control. |
| Alternate flows | • An HQ role (General Director, Financial Director, Staff) may pass an explicit charity id and so read across the charity boundary; a charity user may not and always sees its own data.<br>• No row matches the criteria — the grid renders empty and the paging control reports zero pages. |
| Exception flows | • The session has expired or the role is not permitted — the request is rejected and the SPA routes back to the login state.<br>• The business layer returns «Faliure» and the operation is not applied. |
| Post-conditions | • No stored data is changed — the operation is a read. |
| Realisation | `GET /api/OrphanPayments/{id}/details` → `OrphanPaymentsController` → `IOrphanPaymentService` |

#### 15.U.9  UC-PAY-09 — Stop or resume an orphan's payment إيقاف / استئناف صرف اليتيم


| Item | Specification |
| --- | --- |
| Use case ID | UC-PAY-09 |
| Name | Stop or resume an orphan's payment إيقاف / استئناف صرف اليتيم |
| Type | Create a record |
| Primary actor | Charity, HQ roles |
| Secondary actors | The system (Web API + business layer); the database. |
| Summary | Suspends the orphan's entitlement in the batch (for example, unavailable, travelled, report refused) or resumes it. Orphans already requested for exclusion are forced to remain stopped. |
| Trigger | The actor selects the function from the module menu or the screen that hosts it. |
| Pre-conditions | 1. The actor is authenticated; the JWT access token is valid and the client holds the role, charity and country claims it carries.<br>2. The actor holds one of: charity, HQ roles.<br>3. Caller identity comes from the JWT, never from the request; the application service scopes the query to the charity (and country) that owns the user, unless the role is an HQ role permitted to pass an explicit `charityId` filter. |
| Main flow | 1. The actor reaches the function from the screen of this module that hosts it. The screen opens in add mode.<br>2. The system loads the reference data the form needs (regions, centres, banks, types and the other lookup lists bound to the drop-downs).<br>3. The actor completes the input fields of the form.<br>4. The actor presses the command button of the screen.<br>5. The SPA issues `POST /api/OrphanPayments/orphan-items` carrying rowId, userId, action=0, flag.<br>6. `OrphanPaymentsController` binds the typed request DTO and delegates to the application service.<br>7. `IOrphanPaymentService` validates the payload with its FluentValidation validator, applies the business rules and the charity scope, and persists through `IUnitOfWork`.<br>8. The business layer validates the payload, stamps the owning charity and the creating user, and persists the new record.<br>9. The system returns the outcome and the SPA confirms the save and returns to the list screen. |
| Alternate flows | • An HQ role (General Director, Financial Director, Staff) may pass an explicit charity id and so read across the charity boundary; a charity user may not and always sees its own data.<br>• The actor abandons the form before saving — nothing is written and the record keeps its previous state. |
| Exception flows | • The session has expired or the role is not permitted — the request is rejected and the SPA routes back to the login state.<br>• The business layer returns «Faliure» and the operation is not applied.<br>• A mandatory field is empty or fails its format check — the save is refused and the field is flagged on the form. |
| Post-conditions | • A new record exists, owned by the charity of the creating user, and appears in the list screen of the module. |
| Realisation | `POST /api/OrphanPayments/orphan-items` → `OrphanPaymentsController` → `IOrphanPaymentService` |

#### 15.U.10  UC-PAY-10 — Mark a payment row printed تعليم كمطبوع


| Item | Specification |
| --- | --- |
| Use case ID | UC-PAY-10 |
| Name | Mark a payment row printed تعليم كمطبوع |
| Type | Print / produce a document |
| Primary actor | Charity |
| Secondary actors | The system (Web API + business layer); the database. the report and export endpoints of `api/Reports` as the rendering path — jsPDF for PDF and ExcelJS for Excel. |
| Summary | Flags that the cheque or receipt card for the row has been printed, so it is excluded from the next print run. |
| Trigger | The actor selects the function from the module menu or the screen that hosts it. |
| Pre-conditions | 1. The actor is authenticated; the JWT access token is valid and the client holds the role, charity and country claims it carries.<br>2. The actor holds one of: charity.<br>3. The target record exists and its identifier is known to the screen. |
| Main flow | 1. The actor reaches the function from the screen of this module that hosts it.<br>2. The actor selects the record or the range to be printed.<br>3. The actor presses the command button of the screen.<br>4. The browser opens `POST /api/OrphanPayments/orphan-items` with action=1, flag.<br>5. `ReportsController` resolves the report key, applies the filter, and renders the document from the dataset the application service returns.<br>6. The rendered document is streamed back as a PDF for viewing and printing. |
| Alternate flows | None recorded. |
| Exception flows | • The session has expired or the role is not permitted — the request is rejected and the SPA routes back to the login state.<br>• The business layer returns «Faliure» and the operation is not applied.<br>• The report definition or a required parameter is missing — the print action fails and no document is produced. |
| Post-conditions | • A printable document has been produced. Where the module records printing, the row is flagged as printed. |
| Realisation | `POST /api/OrphanPayments/orphan-items` → `OrphanPaymentsController` → `IOrphanPaymentService` |

#### 15.U.11  UC-PAY-11 — Confirm receipt of a payment تأكيد الاستلام


| Item | Specification |
| --- | --- |
| Use case ID | UC-PAY-11 |
| Name | Confirm receipt of a payment تأكيد الاستلام |
| Type | Create a record |
| Primary actor | Charity |
| Secondary actors | The system (Web API + business layer); the database. |
| Summary | Records that the guardian collected the money for that orphan, closing the row. |
| Trigger | The actor selects the function from the module menu or the screen that hosts it. |
| Pre-conditions | 1. The actor is authenticated; the JWT access token is valid and the client holds the role, charity and country claims it carries.<br>2. The actor holds one of: charity. |
| Main flow | 1. The actor reaches the function from the screen of this module that hosts it. The screen opens in add mode.<br>2. The system loads the reference data the form needs (regions, centres, banks, types and the other lookup lists bound to the drop-downs).<br>3. The actor completes the input fields of the form.<br>4. The actor presses the command button of the screen.<br>5. The SPA issues `POST /api/OrphanPayments/orphan-items` carrying action=2, flag.<br>6. `OrphanPaymentsController` binds the typed request DTO and delegates to the application service.<br>7. `IOrphanPaymentService` validates the payload with its FluentValidation validator, applies the business rules and the charity scope, and persists through `IUnitOfWork`.<br>8. The business layer validates the payload, stamps the owning charity and the creating user, and persists the new record.<br>9. The system returns the outcome and the SPA confirms the save and returns to the list screen. |
| Alternate flows | • The actor abandons the form before saving — nothing is written and the record keeps its previous state. |
| Exception flows | • The session has expired or the role is not permitted — the request is rejected and the SPA routes back to the login state.<br>• The business layer returns «Faliure» and the operation is not applied.<br>• A mandatory field is empty or fails its format check — the save is refused and the field is flagged on the form. |
| Post-conditions | • A new record exists, owned by the charity of the creating user, and appears in the list screen of the module. |
| Realisation | `POST /api/OrphanPayments/orphan-items` → `OrphanPaymentsController` → `IOrphanPaymentService` |

#### 15.U.12  UC-PAY-12 — Record cheque number, date and collector تسجيل رقم الشيك والمستلم


| Item | Specification |
| --- | --- |
| Use case ID | UC-PAY-12 |
| Name | Record cheque number, date and collector تسجيل رقم الشيك والمستلم |
| Type | Create a record |
| Primary actor | Charity |
| Secondary actors | The system (Web API + business layer); the database. |
| Summary | Settles the row in one step: confirms receipt, marks it printed, and stores the cheque number, print date and the name of the beneficiary who signed for it. A variant omits the print date. |
| Trigger | The actor selects the function from the module menu or the screen that hosts it. |
| Pre-conditions | 1. The actor is authenticated; the JWT access token is valid and the client holds the role, charity and country claims it carries.<br>2. The actor holds one of: charity. |
| Main flow | 1. The actor reaches the function from the screen of this module that hosts it. The screen opens in add mode.<br>2. The system loads the reference data the form needs (regions, centres, banks, types and the other lookup lists bound to the drop-downs).<br>3. The actor completes the input fields of the form.<br>4. The actor presses the command button of the screen.<br>5. The SPA issues `POST /api/OrphanPayments/orphan-items` carrying action=3\|4, chiqueNo, chiqueDate, sponserName.<br>6. `OrphanPaymentsController` binds the typed request DTO and delegates to the application service.<br>7. `IOrphanPaymentService` validates the payload with its FluentValidation validator, applies the business rules and the charity scope, and persists through `IUnitOfWork`.<br>8. The business layer validates the payload, stamps the owning charity and the creating user, and persists the new record.<br>9. The system returns the outcome and the SPA confirms the save and returns to the list screen. |
| Alternate flows | • The actor abandons the form before saving — nothing is written and the record keeps its previous state. |
| Exception flows | • The session has expired or the role is not permitted — the request is rejected and the SPA routes back to the login state.<br>• The business layer returns «Faliure» and the operation is not applied.<br>• A mandatory field is empty or fails its format check — the save is refused and the field is flagged on the form. |
| Post-conditions | • A new record exists, owned by the charity of the creating user, and appears in the list screen of the module. |
| Realisation | `POST /api/OrphanPayments/orphan-items\|4&chiqueNo&chiqueDate&sponserName` → `OrphanPaymentsController` → `IOrphanPaymentService` |

#### 15.U.13  UC-PAY-13 — Update payment-detail flags تحديث بيانات الصرف


| Item | Specification |
| --- | --- |
| Use case ID | UC-PAY-13 |
| Name | Update payment-detail flags تحديث بيانات الصرف |
| Type | Update a record |
| Primary actor | Charity, HQ roles |
| Secondary actors | The system (Web API + business layer); the database. |
| Summary | Generic row update used by the payment-detail grid to toggle the same flag set from the detail screen. |
| Trigger | The actor selects the function from the module menu or the screen that hosts it. |
| Pre-conditions | 1. The actor is authenticated; the JWT access token is valid and the client holds the role, charity and country claims it carries.<br>2. The actor holds one of: charity, HQ roles.<br>3. Caller identity comes from the JWT, never from the request; the application service scopes the query to the charity (and country) that owns the user, unless the role is an HQ role permitted to pass an explicit `charityId` filter.<br>4. The target record exists and its identifier is known to the screen. |
| Main flow | 1. The actor reaches the function from the screen of this module that hosts it. The screen opens in edit mode on the selected record.<br>2. The system loads the current values into the form.<br>3. The actor changes the fields to be corrected.<br>4. The actor presses the command button of the screen.<br>5. The SPA issues `PUT /api/OrphanPayments/{id}` carrying rowId, userId, action, flag.<br>6. `OrphanPaymentsController` binds the typed request DTO and delegates to the application service.<br>7. `IOrphanPaymentService` validates the payload with its FluentValidation validator, applies the business rules and the charity scope, and persists through `IUnitOfWork`.<br>8. The business layer re-validates the payload, applies the changes and persists them.<br>9. The system returns the outcome and the SPA refreshes the screen. |
| Alternate flows | • An HQ role (General Director, Financial Director, Staff) may pass an explicit charity id and so read across the charity boundary; a charity user may not and always sees its own data.<br>• The actor abandons the form before saving — nothing is written and the record keeps its previous state. |
| Exception flows | • The session has expired or the role is not permitted — the request is rejected and the SPA routes back to the login state.<br>• The business layer returns «Faliure» and the operation is not applied.<br>• A mandatory field is empty or fails its format check — the save is refused and the field is flagged on the form. |
| Post-conditions | • The stored record carries the new values; no other record is affected. |
| Realisation | `PUT /api/OrphanPayments/{id}` → `OrphanPaymentsController` → `IOrphanPaymentService` |

#### 15.U.14  UC-PAY-14 — Generate the bank transfer file كشف التحويلات البنكية


| Item | Specification |
| --- | --- |
| Use case ID | UC-PAY-14 |
| Name | Generate the bank transfer file كشف التحويلات البنكية |
| Type | Export data |
| Primary actor | Fin. Director, Gen. Director |
| Secondary actors | The system (Web API + business layer); the database. EPPlus as the worksheet writer. |
| Summary | Produces the file handed to the bank to execute the batch for one charity: one line per beneficiary with account/card details and amount. Three variants are available — the plain file, the merged file (rows consolidated per guardian), and the merged file carrying the orphan's full name. |
| Trigger | The actor presses «كشف التحويلات» on the screen BankFile. |
| Pre-conditions | 1. The actor is authenticated; the JWT access token is valid and the client holds the role, charity and country claims it carries.<br>2. The actor holds one of: Fin. Director, Gen. Director.<br>3. Caller identity comes from the JWT, never from the request; the application service scopes the query to the charity (and country) that owns the user, unless the role is an HQ role permitted to pass an explicit `charityId` filter.<br>4. The SPA route `#/orphan-payments/:id/bank-file` has loaded and its reference-data lookups have been populated. |
| Main flow | 1. The actor navigates to the screen at `#/orphan-payments/:id/bank-file`.<br>2. The actor sets the selection criteria for the extract.<br>3. The actor presses «كشف التحويلات» (getCharityData()).<br>4. The SPA issues `GET /api/OrphanPayments/{id}/export` carrying string charityId, string paymentId, string userId.<br>5. `OrphanPaymentsController` binds the typed request DTO and delegates to the application service.<br>6. `IOrphanPaymentService` validates the payload with its FluentValidation validator, applies the business rules and the charity scope, and persists through `IUnitOfWork`.<br>7. The business layer builds the worksheet (EPPlus) from the selected rows and streams it to the browser as a download. |
| Alternate flows | • An HQ role (General Director, Financial Director, Staff) may pass an explicit charity id and so read across the charity boundary; a charity user may not and always sees its own data. |
| Exception flows | • The session has expired or the role is not permitted — the request is rejected and the SPA routes back to the login state.<br>• The business layer returns «Faild Operation» and the operation is not applied. |
| Post-conditions | • A workbook has been delivered to the actor. No stored data is changed. |
| Realisation | Route `#/orphan-payments/:id/bank-file` → `BankFileComponent`<br>`GET /api/OrphanPayments/{id}/export` → `OrphanPaymentsController` → `IOrphanPaymentService` |

#### 15.U.15  UC-PAY-15 — Import transfer numbers from the bank رفع أرقام الحوالات


| Item | Specification |
| --- | --- |
| Use case ID | UC-PAY-15 |
| Name | Import transfer numbers from the bank رفع أرقام الحوالات |
| Type | Import a file |
| Primary actor | Fin. Director |
| Secondary actors | The system (Web API + business layer); the database. The uploaded workbook/CSV as the data source. |
| Summary | Uploads the CSV returned by the bank containing the transfer reference generated for each beneficiary, and writes those references onto the matching payment rows of the batch. |
| Trigger | The actor selects the function from the module menu or the screen that hosts it. |
| Pre-conditions | 1. The actor is authenticated; the JWT access token is valid and the client holds the role, charity and country claims it carries.<br>2. The actor holds one of: Fin. Director.<br>3. Caller identity comes from the JWT, never from the request; the application service scopes the query to the charity (and country) that owns the user, unless the role is an HQ role permitted to pass an explicit `charityId` filter. |
| Main flow | 1. The actor reaches the function from the screen of this module that hosts it.<br>2. The actor chooses the file to upload in the file field of the screen.<br>3. The actor presses the command button of the screen.<br>4. The SPA posts the file as multipart content to `POST /api/OrphanPayments/{id}/import/transfer-numbers`.<br>5. `OrphanPaymentsController` binds the typed request DTO and delegates to the application service.<br>6. `IOrphanPaymentService` validates the payload with its FluentValidation validator, applies the business rules and the charity scope, and persists through `IUnitOfWork`.<br>7. The business layer parses each row, matches it to the existing records by their key and applies the values it carries.<br>8. Rows that cannot be matched are reported back and the system returns the count applied. |
| Alternate flows | • An HQ role (General Director, Financial Director, Staff) may pass an explicit charity id and so read across the charity boundary; a charity user may not and always sees its own data. |
| Exception flows | • The session has expired or the role is not permitted — the request is rejected and the SPA routes back to the login state.<br>• The uploaded file is not the expected workbook/CSV layout — the import is abandoned and no row is changed. |
| Post-conditions | • The matched records carry the imported values; unmatched rows are left untouched and reported. |
| Realisation | `POST /api/OrphanPayments/{id}/import/transfer-numbers` → `OrphanPaymentsController` → `IOrphanPaymentService` |

#### 15.U.16  UC-PAY-16 — Import a batch reconciliation file رفع ملف الدفعة


| Item | Specification |
| --- | --- |
| Use case ID | UC-PAY-16 |
| Name | Import a batch reconciliation file رفع ملف الدفعة |
| Type | Import a file |
| Primary actor | Fin. Director |
| Secondary actors | The system (Web API + business layer); the database. The uploaded workbook/CSV as the data source. |
| Summary | Uploads the batch CSV and applies its contents to the payment rows of the identified batch. |
| Trigger | The actor selects the function from the module menu or the screen that hosts it. |
| Pre-conditions | 1. The actor is authenticated; the JWT access token is valid and the client holds the role, charity and country claims it carries.<br>2. The actor holds one of: Fin. Director.<br>3. Caller identity comes from the JWT, never from the request; the application service scopes the query to the charity (and country) that owns the user, unless the role is an HQ role permitted to pass an explicit `charityId` filter. |
| Main flow | 1. The actor reaches the function from the screen of this module that hosts it.<br>2. The actor chooses the file to upload in the file field of the screen.<br>3. The actor presses the command button of the screen.<br>4. The SPA posts the file as multipart content to `POST /api/OrphanPayments/{id}/import/bank-file`.<br>5. `OrphanPaymentsController` binds the typed request DTO and delegates to the application service.<br>6. `IOrphanPaymentService` validates the payload with its FluentValidation validator, applies the business rules and the charity scope, and persists through `IUnitOfWork`.<br>7. The business layer parses each row, matches it to the existing records by their key and applies the values it carries.<br>8. Rows that cannot be matched are reported back and the system returns the count applied. |
| Alternate flows | • An HQ role (General Director, Financial Director, Staff) may pass an explicit charity id and so read across the charity boundary; a charity user may not and always sees its own data. |
| Exception flows | • The session has expired or the role is not permitted — the request is rejected and the SPA routes back to the login state.<br>• The uploaded file is not the expected workbook/CSV layout — the import is abandoned and no row is changed. |
| Post-conditions | • The matched records carry the imported values; unmatched rows are left untouched and reported. |
| Realisation | `POST /api/OrphanPayments/{id}/import/bank-file` → `OrphanPaymentsController` → `IOrphanPaymentService` |

#### 15.U.17  UC-PAY-17 — Import exchange (execution) statuses رفع حالات الصرف


| Item | Specification |
| --- | --- |
| Use case ID | UC-PAY-17 |
| Name | Import exchange (execution) statuses رفع حالات الصرف |
| Type | Import a file |
| Primary actor | Fin. Director |
| Secondary actors | The system (Web API + business layer); the database. The uploaded workbook/CSV as the data source. |
| Summary | Uploads the bank's execution-status CSV so each payment row records whether the transfer succeeded, failed or is pending. |
| Trigger | The actor selects the function from the module menu or the screen that hosts it. |
| Pre-conditions | 1. The actor is authenticated; the JWT access token is valid and the client holds the role, charity and country claims it carries.<br>2. The actor holds one of: Fin. Director.<br>3. Caller identity comes from the JWT, never from the request; the application service scopes the query to the charity (and country) that owns the user, unless the role is an HQ role permitted to pass an explicit `charityId` filter. |
| Main flow | 1. The actor reaches the function from the screen of this module that hosts it.<br>2. The actor chooses the file to upload in the file field of the screen.<br>3. The actor presses the command button of the screen.<br>4. The SPA posts the file as multipart content to `POST /api/OrphanPayments/{id}/import/exchange-status`.<br>5. `OrphanPaymentsController` binds the typed request DTO and delegates to the application service.<br>6. `IOrphanPaymentService` validates the payload with its FluentValidation validator, applies the business rules and the charity scope, and persists through `IUnitOfWork`.<br>7. The business layer parses each row, matches it to the existing records by their key and applies the values it carries.<br>8. Rows that cannot be matched are reported back and the system returns the count applied. |
| Alternate flows | • An HQ role (General Director, Financial Director, Staff) may pass an explicit charity id and so read across the charity boundary; a charity user may not and always sees its own data. |
| Exception flows | • The session has expired or the role is not permitted — the request is rejected and the SPA routes back to the login state.<br>• The uploaded file is not the expected workbook/CSV layout — the import is abandoned and no row is changed. |
| Post-conditions | • The matched records carry the imported values; unmatched rows are left untouched and reported. |
| Realisation | `POST /api/OrphanPayments/{id}/import/exchange-status` → `OrphanPaymentsController` → `IOrphanPaymentService` |

#### 15.U.18  UC-PAY-18 — Report payments received المستلمون


| Item | Specification |
| --- | --- |
| Use case ID | UC-PAY-18 |
| Name | Report payments received المستلمون |
| Type | Query a report |
| Primary actor | HQ roles, charity |
| Secondary actors | The system (Web API + business layer); the database. |
| Summary | Lists the orphans in a batch for a charity whose payment is confirmed received, for reconciliation and for printing the received list. |
| Trigger | The actor selects the function from the module menu or the screen that hosts it. |
| Pre-conditions | 1. The actor is authenticated; the JWT access token is valid and the client holds the role, charity and country claims it carries.<br>2. The actor holds one of: HQ roles, charity.<br>3. Caller identity comes from the JWT, never from the request; the application service scopes the query to the charity (and country) that owns the user, unless the role is an HQ role permitted to pass an explicit `charityId` filter. |
| Main flow | 1. The actor reaches the function from the screen of this module that hosts it.<br>2. The actor sets the report criteria (period, charity, country and the other filters the screen offers).<br>3. The actor runs the report.<br>4. The SPA issues `GET /api/OrphanPayments/{id}/details?received=true` carrying paymentId, charityId, userId.<br>5. `OrphanPaymentsController` binds the typed request DTO and delegates to the application service.<br>6. `IOrphanPaymentService` validates the payload with its FluentValidation validator, applies the business rules and the charity scope, and persists through `IUnitOfWork`.<br>7. The business layer composes the projection, scoped to the caller’s charity and country.<br>8. The result is rendered in the on-screen grid, from where it can be printed or exported. |
| Alternate flows | • An HQ role (General Director, Financial Director, Staff) may pass an explicit charity id and so read across the charity boundary; a charity user may not and always sees its own data.<br>• No row matches the criteria — the grid renders empty and the paging control reports zero pages. |
| Exception flows | • The session has expired or the role is not permitted — the request is rejected and the SPA routes back to the login state. |
| Post-conditions | • No stored data is changed — the operation is a read. |
| Realisation | `GET /api/OrphanPayments/{id}/details?received=true` · `GET /api/OrphanPayments/{id}/details` → `OrphanPaymentsController` → `IOrphanPaymentService` |

#### 15.U.19  UC-PAY-19 — Report payments not received غير المستلمين


| Item | Specification |
| --- | --- |
| Use case ID | UC-PAY-19 |
| Name | Report payments not received غير المستلمين |
| Type | Query a report |
| Primary actor | HQ roles, charity |
| Secondary actors | The system (Web API + business layer); the database. |
| Summary | The complement of the previous list — outstanding rows that must still be collected or returned. |
| Trigger | The actor selects the function from the module menu or the screen that hosts it. |
| Pre-conditions | 1. The actor is authenticated; the JWT access token is valid and the client holds the role, charity and country claims it carries.<br>2. The actor holds one of: HQ roles, charity.<br>3. Caller identity comes from the JWT, never from the request; the application service scopes the query to the charity (and country) that owns the user, unless the role is an HQ role permitted to pass an explicit `charityId` filter. |
| Main flow | 1. The actor reaches the function from the screen of this module that hosts it.<br>2. The actor sets the report criteria (period, charity, country and the other filters the screen offers).<br>3. The actor runs the report.<br>4. The SPA issues `POST /api/Reports/payments-not-received` carrying paymentId, charityId, userId.<br>5. `ReportsController` binds the typed request DTO and delegates to the application service.<br>6. `IOrphanPaymentService` validates the payload with its FluentValidation validator, applies the business rules and the charity scope, and persists through `IUnitOfWork`.<br>7. The business layer composes the projection, scoped to the caller’s charity and country.<br>8. The result is rendered in the on-screen grid, from where it can be printed or exported. |
| Alternate flows | • An HQ role (General Director, Financial Director, Staff) may pass an explicit charity id and so read across the charity boundary; a charity user may not and always sees its own data.<br>• No row matches the criteria — the grid renders empty and the paging control reports zero pages. |
| Exception flows | • The session has expired or the role is not permitted — the request is rejected and the SPA routes back to the login state. |
| Post-conditions | • No stored data is changed — the operation is a read. |
| Realisation | `POST /api/Reports/payments-not-received` → `ReportsController` → `IReportService` |

#### 15.U.20  UC-PAY-20 — Report stopped payments الموقوفون


| Item | Specification |
| --- | --- |
| Use case ID | UC-PAY-20 |
| Name | Report stopped payments الموقوفون |
| Type | Query a report |
| Primary actor | HQ roles, charity |
| Secondary actors | The system (Web API + business layer); the database. |
| Summary | Lists the rows suspended in the batch together with the reason context, for HQ follow-up with the charity. |
| Trigger | The actor selects the function from the module menu or the screen that hosts it. |
| Pre-conditions | 1. The actor is authenticated; the JWT access token is valid and the client holds the role, charity and country claims it carries.<br>2. The actor holds one of: HQ roles, charity.<br>3. Caller identity comes from the JWT, never from the request; the application service scopes the query to the charity (and country) that owns the user, unless the role is an HQ role permitted to pass an explicit `charityId` filter. |
| Main flow | 1. The actor reaches the function from the screen of this module that hosts it.<br>2. The actor sets the report criteria (period, charity, country and the other filters the screen offers).<br>3. The actor runs the report.<br>4. The SPA issues `POST /api/Reports/payments-stopped` carrying paymentId, charityId, userId.<br>5. `ReportsController` binds the typed request DTO and delegates to the application service.<br>6. `IOrphanPaymentService` validates the payload with its FluentValidation validator, applies the business rules and the charity scope, and persists through `IUnitOfWork`.<br>7. The business layer composes the projection, scoped to the caller’s charity and country.<br>8. The result is rendered in the on-screen grid, from where it can be printed or exported. |
| Alternate flows | • An HQ role (General Director, Financial Director, Staff) may pass an explicit charity id and so read across the charity boundary; a charity user may not and always sees its own data.<br>• No row matches the criteria — the grid renders empty and the paging control reports zero pages. |
| Exception flows | • The session has expired or the role is not permitted — the request is rejected and the SPA routes back to the login state.<br>• The business layer returns «Faliure» and the operation is not applied. |
| Post-conditions | • No stored data is changed — the operation is a read. |
| Realisation | `POST /api/Reports/payments-stopped` → `ReportsController` → `IReportService` |

#### 15.U.21  UC-PAY-21 — View batch summary pages ملخص الدفعة


| Item | Specification |
| --- | --- |
| Use case ID | UC-PAY-21 |
| Name | View batch summary pages ملخص الدفعة |
| Type | Read a record |
| Primary actor | HQ roles |
| Secondary actors | The system (Web API + business layer); the database. |
| Summary | Returns the paginated summary of a batch for a charity — totals, counts and per-page breakdown used as the cover sheet of the disbursement file. |
| Trigger | The actor selects the function from the module menu or the screen that hosts it. |
| Pre-conditions | 1. The actor is authenticated; the JWT access token is valid and the client holds the role, charity and country claims it carries.<br>2. The actor holds one of: HQ roles.<br>3. Caller identity comes from the JWT, never from the request; the application service scopes the query to the charity (and country) that owns the user, unless the role is an HQ role permitted to pass an explicit `charityId` filter.<br>4. The target record exists and its identifier is known to the screen. |
| Main flow | 1. The actor reaches the function from the screen of this module that hosts it.<br>2. The actor selects the record to open.<br>3. The SPA issues `GET /api/Dashboard/payment-summary` carrying paymentId, charityId, userId.<br>4. `AuthController` binds the typed request DTO and delegates to the application service.<br>5. `IOrphanPaymentService` validates the payload with its FluentValidation validator, applies the business rules and the charity scope, and persists through `IUnitOfWork`.<br>6. The system returns the record with its child collections and the SPA binds them to the form fields. |
| Alternate flows | • An HQ role (General Director, Financial Director, Staff) may pass an explicit charity id and so read across the charity boundary; a charity user may not and always sees its own data. |
| Exception flows | • The session has expired or the role is not permitted — the request is rejected and the SPA routes back to the login state. |
| Post-conditions | • No stored data is changed — the operation is a read. |
| Realisation | `GET /api/Dashboard/payment-summary` → `DashboardController` → `IDashboardService` |

#### 15.U.22  UC-PAY-22 — View batch transfers and cheques حوالات وشيكات الأيتام


| Item | Specification |
| --- | --- |
| Use case ID | UC-PAY-22 |
| Name | View batch transfers and cheques حوالات وشيكات الأيتام |
| Type | Read a record |
| Primary actor | HQ roles, charity |
| Secondary actors | The system (Web API + business layer); the database. |
| Summary | Lists the transfers generated for a batch and, separately, the cheques issued against orphan payments for a charity within a date or batch range. |
| Trigger | The actor opens the screen at `#/orphan-payments/:id/cheques` from the module menu. |
| Pre-conditions | 1. The actor is authenticated; the JWT access token is valid and the client holds the role, charity and country claims it carries.<br>2. The actor holds one of: HQ roles, charity.<br>3. Caller identity comes from the JWT, never from the request; the application service scopes the query to the charity (and country) that owns the user, unless the role is an HQ role permitted to pass an explicit `charityId` filter.<br>4. The SPA route `#/orphan-payments/:id/cheques` has loaded and its reference-data lookups have been populated.<br>5. The target record exists and its identifier is known to the screen. |
| Main flow | 1. The actor navigates to the screen at `#/orphan-payments/:id/cheques`.<br>2. The actor selects the record to open.<br>3. The SPA issues `GET /api/OrphanPayments/{id}/details` carrying string paymentId, string charityId,string userId.<br>4. `OrphanPaymentsController` binds the typed request DTO and delegates to the application service.<br>5. `IOrphanPaymentService` validates the payload with its FluentValidation validator, applies the business rules and the charity scope, and persists through `IUnitOfWork`.<br>6. The system returns the record with its child collections and the SPA binds them to the form fields. |
| Alternate flows | • An HQ role (General Director, Financial Director, Staff) may pass an explicit charity id and so read across the charity boundary; a charity user may not and always sees its own data. |
| Exception flows | • The session has expired or the role is not permitted — the request is rejected and the SPA routes back to the login state. |
| Post-conditions | • No stored data is changed — the operation is a read. |
| Realisation | Route `#/orphan-payments/:id/cheques` → `OrphanPaymentChequesComponent`<br>`GET /api/OrphanPayments/{id}/details` · `GET /api/CheckManagement?orphanPaymentId=` → `OrphanPaymentsController` → `IOrphanPaymentService` |

#### 15.U.23  UC-PAY-23 — Identify orphans sponsored elsewhere أيتام لهم كافل آخر


| Item | Specification |
| --- | --- |
| Use case ID | UC-PAY-23 |
| Name | Identify orphans sponsored elsewhere أيتام لهم كافل آخر |
| Type | Browse a list |
| Primary actor | HQ roles |
| Secondary actors | The system (Web API + business layer); the database. |
| Summary | For a charity and batch, lists orphans who also appear under another sponsoring body, so duplicate disbursement can be prevented. |
| Trigger | The actor selects the function from the module menu or the screen that hosts it. |
| Pre-conditions | 1. The actor is authenticated; the JWT access token is valid and the client holds the role, charity and country claims it carries.<br>2. The actor holds one of: HQ roles.<br>3. Caller identity comes from the JWT, never from the request; the application service scopes the query to the charity (and country) that owns the user, unless the role is an HQ role permitted to pass an explicit `charityId` filter. |
| Main flow | 1. The actor reaches the function from the screen of this module that hosts it.<br>2. The system loads the filter lookups the screen offers and, for HQ roles, the charity selector.<br>3. The SPA issues `POST /api/Reports/orphans-other-sponsor` carrying charityId, batchNo, userId.<br>4. `ReportsController` binds the typed request DTO and delegates to the application service.<br>5. `IReportService` validates the payload with its FluentValidation validator, applies the business rules and the charity scope, and persists through `IUnitOfWork`.<br>6. The result set is scoped to the caller’s charity and country, ordered and paged.<br>7. The SPA renders the page and the paging control. |
| Alternate flows | • An HQ role (General Director, Financial Director, Staff) may pass an explicit charity id and so read across the charity boundary; a charity user may not and always sees its own data.<br>• No row matches the criteria — the grid renders empty and the paging control reports zero pages. |
| Exception flows | • The session has expired or the role is not permitted — the request is rejected and the SPA routes back to the login state. |
| Post-conditions | • No stored data is changed — the operation is a read. |
| Realisation | `POST /api/Reports/orphans-other-sponsor` → `ReportsController` → `IReportService` |

#### 15.U.24  UC-PAY-24 — Print disbursement documents طباعة مستندات الصرف


| Item | Specification |
| --- | --- |
| Use case ID | UC-PAY-24 |
| Name | Print disbursement documents طباعة مستندات الصرف |
| Type | Print / produce a document |
| Primary actor | Charity, HQ roles |
| Secondary actors | The system (Web API + business layer); the database. the report and export endpoints of `api/Reports` as the rendering path — jsPDF for PDF and ExcelJS for Excel. |
| Summary | Produces the printed documents of a batch for a charity, through `POST /api/Reports/<report-key>/export/pdf`: the received list, the not-received list, the stopped list, the cheque-numbers list, and the signed receipt cards handed to guardians. |
| Trigger | The actor selects the function from the module menu or the screen that hosts it. |
| Pre-conditions | 1. The actor is authenticated; the JWT access token is valid and the client holds the role, charity and country claims it carries.<br>2. The actor holds one of: charity, HQ roles.<br>3. Caller identity comes from the JWT, never from the request; the application service scopes the query to the charity (and country) that owns the user, unless the role is an HQ role permitted to pass an explicit `charityId` filter.<br>4. The target record exists and its identifier is known to the screen. |
| Main flow | 1. The actor reaches the function from the screen of this module that hosts it.<br>2. The actor selects the record or the range to be printed.<br>3. The actor presses the command button of the screen.<br>4. The browser opens `POST /api/Reports/payments-received/export/pdf` with string charityId, string orpCheckBatchNo.<br>5. `ReportsController` resolves the report key, applies the filter, and renders the document from the dataset the application service returns.<br>6. The rendered document is streamed back as a PDF for viewing and printing. |
| Alternate flows | • An HQ role (General Director, Financial Director, Staff) may pass an explicit charity id and so read across the charity boundary; a charity user may not and always sees its own data. |
| Exception flows | • The session has expired or the role is not permitted — the request is rejected and the SPA routes back to the login state.<br>• The business layer returns «Faliure» and the operation is not applied.<br>• The report definition or a required parameter is missing — the print action fails and no document is produced. |
| Post-conditions | • A printable document has been produced. Where the module records printing, the row is flagged as printed. |
| Realisation | `POST /api/Reports/payments-received/export/pdf` · `POST /api/Reports/payments-not-received/export/pdf` · `POST /api/Reports/payments-stopped/export/pdf` · `POST /api/Reports/cheque-numbers/export/pdf` · `POST /api/Reports/receipt-cards/export/pdf` → `ReportsController` → `IReportService` |

### 15.A  Annex - Angular routes of this module

Routes are hash-based (`useHash: true`), rendered inside `MainLayoutComponent` behind
`AuthGuard` and `PermissionGuard`. *implemented* means the route exists in
`Frontend/src/app`; *planned* means it is specified here and not yet built.

| Angular route | Feature module | Component | Status |
| --- | --- | --- | --- |
| `#/orphan-payments` | `orphan-payments` | `OrphanPaymentListComponent` | implemented |
| `#/orphan-payments/create` | `orphan-payments` | `OrphanPaymentFormComponent` | implemented |
| `#/orphan-payments/:id` | `orphan-payments` | `OrphanPaymentDetailComponent` | implemented |
| `#/orphan-payments/:id/edit` | `orphan-payments` | `OrphanPaymentFormComponent` | implemented |
| `#/orphan-payments/:id/add-orphans` | `orphan-payments` | `AddOrphansToGroupComponent` | implemented |
| `#/orphan-payments/:id/cheques` | `orphan-payments` | `OrphanPaymentChequesComponent` | planned |
| `#/orphan-payments/:id/bank-file` | `orphan-payments` | `BankFileComponent` | planned |

### 15.B  Annex - API controllers of this module

Every controller inherits `ApiController` (`[Authorize]`, route `api/[controller]`) and
returns `Framework.Core.ApiResponse<T>`. Business rules live in the application service
behind each controller, never in the controller itself.

| Controller | Base route | Responsibility |
| --- | --- | --- |
| `AuthController` | `api/Auth` | Signed-in user detail; payment summary pages. |
| `OrphanPaymentsController` | `api/OrphanPayments` | Batch create/read/update/delete; batch numbers. Batch read by id. Payment details, bank file variants, row flag update. Orphans in a batch; orphan payment cheques. Row settlement actions; batch transfers. Received-payment detail. Transfer-number import; project summary. Batch CSV import. Exchange-status import. |
| `LookupManagementController` | `api/LookupManagement` | Countries and banks; received-payment detail. |

---

Shared context - system architecture, actors and roles, the sponsorship lifecycle, the full module map and all appendices - is in the companion document [00-Overview-and-Common-Context](00-Overview-and-Common-Context.md). The delivery backlog for this module is epic EP-10 in [00-EPICS-and-User-Stories](00-EPICS-and-User-Stories.md).

