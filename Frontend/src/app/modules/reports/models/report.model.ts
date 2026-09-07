import { FamilyFollowUpRow } from '../../families/models/family.model';

/**
 * UC-FAM-14 — printable sheet payloads from POST /api/Reports/{reportKey}/export/pdf.
 * The endpoint returns JSON (client-side print ruling); the browser's print-to-PDF
 * produces the file.
 */
export interface ReportSheetPayload<TRow> {
  variantKey: string;
  title: string;
  charityName?: string | null;
  generatedOn: string;
  rows: TRow[];
  /** True when the selection exceeded the server's row ceiling — the document is partial and
   * the operator is warned (a sheet must never print silently incomplete). */
  truncated?: boolean;
}

/** One guardian/widow identification-sheet row. */
export interface IdentificationSheetRow {
  familyCode: string;
  fullName: string;
  nationalId?: string | null;
  relationship?: string | null;
  phone?: string | null;
  charityName?: string | null;
}

/** The tracking sheet reuses the follow-up grid's row shape (5-11). */
export type FamilyUpdateTrackingPayload = ReportSheetPayload<FamilyFollowUpRow>;
export type IdentificationSheetPayload = ReportSheetPayload<IdentificationSheetRow>;

// ==================== EP-18 — Reports & Printing (18-1 onwards) ====================

/**
 * Paged envelope shared by every EP-18 report key (ReportPagedResult&lt;T&gt; server shape).
 */
export interface ReportPagedResult<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

/** UC-RPT-01 (§23.S.3) — filter payload for POST /api/Reports/orphans. */
export interface OrphanDataFilter {
  page: number;
  pageSize: number;
  batchNumber?: string;
  charityId?: string;
  governorateId?: number;
  centerId?: number;
  ageFrom?: number;
  ageTo?: number;
  /** العمر checkbox — accepted on the wire; finished-sponsorship semantics land with 18-4. */
  isFinishedSponsorship?: boolean;
  allOrphans?: boolean;
  excluded?: boolean;
  notExcluded?: boolean;
}

/**
 * UC-RPT-01 (§23.S.3) — one orphan-data grid row; the full 44-column contract. Columns with no
 * backing field today (mobileNumber2, fatherDeathCause, exclusion…) stay null and render blank —
 * data is never invented (epic-wide no-migration ruling).
 */
export interface OrphanDataRow {
  // identity
  code: string;
  fullName: string;
  nationalId?: string | null;
  dateOfBirth?: string | null;
  age?: number | null;
  gender?: string | null;
  // guardian — Provider first, then Father, then Mother
  guardianName?: string | null;
  guardianRelationship?: string | null;
  guardianNationalId?: string | null;
  guardianEducationLevel?: string | null;
  guardianJob?: string | null;
  guardianHealthStatus?: string | null;
  guardianSocialStatus?: string | null;
  guardianDevelopmentProject?: string | null;
  // residence & income
  governorateName?: string | null;
  centerName?: string | null;
  cityVillage?: string | null;
  detailedAddress?: string | null;
  mobileNumber?: string | null;
  mobileNumber2?: string | null;
  houseOwnershipName?: string | null;
  rentAmount?: number | null;
  housingTypeName?: string | null;
  houseStatusName?: string | null;
  monthlyIncome?: number | null;
  // status
  socialStatusName?: string | null;
  healthStatusName?: string | null;
  profession?: string | null;
  // education
  educationLevelName?: string | null;
  gradeClass?: string | null;
  schoolName?: string | null;
  facultyName?: string | null;
  departmentName?: string | null;
  hasAcademicDegree?: boolean | null;
  // father
  fatherDeathDate?: string | null;
  fatherDeathCause?: string | null;
  // exclusion (no backing columns — recorded gap)
  isExcluded: boolean;
  exclusionReason?: string | null;
  // audit / org
  notes?: string | null;
  charityId?: string | null;
  charityName?: string | null;
  lastUpdatedDate?: string | null;
}

// ---- §23.S.4 excluded orphans (UC-RPT-03) ---------------------------------------

/** UC-RPT-03 — filter payload for POST /api/Reports/excluded-orphans. */
export interface ExcludedOrphansFilter {
  page: number;
  pageSize: number;
  charityId?: string;
}

/**
 * UC-RPT-03 — one excluded-orphans grid row (6-column contract). The exclusion flag/reason
 * have no backing columns today — keys stay in the contract, values render blank.
 */
export interface ExcludedOrphanRow {
  code: string;
  fullName: string;
  isExcluded: boolean;
  exclusionReason?: string | null;
  charityId?: string | null;
  charityName?: string | null;
  lastUpdatedDate?: string | null;
}

// ---- §23.S.5 / §23.S.6 orphan status reports (UC-RPT-04, UC-RPT-05) --------------

/** Shared filter payload for POST /api/Reports/finished-sponsorship-orphans and unsponsored-orphans. */
export interface OrphanStatusReportFilter {
  page: number;
  pageSize: number;
  charityId?: string;
}

/** One §23.S.5/§23.S.6 grid row — orphan code, name, charity, family code, age. */
export interface OrphanStatusReportRow {
  code: string;
  fullName: string;
  charityId?: string | null;
  charityName?: string | null;
  familyCode?: string | null;
  age?: number | null;
}

// ---- §23.S.9 widows requiring sponsorship (UC-RPT-06) ----------------------------

/** UC-RPT-06 — filter payload for POST /api/Reports/widows-allowing-sponsorship. */
export interface WidowSponsorshipFilter {
  page: number;
  pageSize: number;
  charityId?: string;
}

/**
 * UC-RPT-06 — one §23.S.9 grid row: the widow (family mother) with her family's
 * residence/income columns. Columns with no backing field (mobileNumber2,
 * developmentProject) stay null and render blank — recorded gap.
 */
export interface WidowSponsorshipRow {
  widowName: string;
  governorateName?: string | null;
  centerName?: string | null;
  cityVillage?: string | null;
  detailedAddress?: string | null;
  mobileNumber?: string | null;
  mobileNumber2?: string | null;
  houseOwnershipName?: string | null;
  rentAmount?: number | null;
  housingTypeName?: string | null;
  houseStatusName?: string | null;
  monthlyIncome?: number | null;
  husbandDeathDate?: string | null;
  developmentProject?: string | null;
  educationLevelName?: string | null;
  profession?: string | null;
  healthStatusName?: string | null;
  nationalId?: string | null;
  notes?: string | null;
  charityId?: string | null;
  charityName?: string | null;
  lastUpdatedDate?: string | null;
}

// ---- §23.S.8 registered Meza cards (UC-RPT-07) -----------------------------------

/** UC-RPT-07 — filter payload for POST /api/Reports/meza-cards. */
export interface MezaCardsFilter {
  page: number;
  pageSize: number;
  charityId?: string;
  // ---- §23.U.8 extract keys (UC-RPT-08) — reportNo's presence selects the extract path ----
  reportNo?: number;
  isCodes?: boolean;
  batchId?: string;
  dateFrom?: string;
  dateTo?: string;
  mezaCardExist?: boolean;
}

/**
 * UC-RPT-07 — one §23.S.8 grid row: a family with its guardian and aggregated orphan
 * names/codes. The Meza card number/expiry have no backing column today — keys stay in the
 * contract, values render blank (recorded gap).
 */
export interface MezaCardsRow {
  familyCode: string;
  guardianName: string;
  guardianNationalId?: string | null;
  phone?: string | null;
  orphanNames?: string | null;
  orphanCodes?: string | null;
  mezaCardNumber?: string | null;
  mezaCardExpiry?: string | null;
  charityId?: string | null;
  charityName?: string | null;
}

// ---- §23.S.13 assistance family data (UC-RPT-09) ---------------------------------

/** UC-RPT-09 — request for POST /api/Reports/beneficiary-family-details. */
export interface BeneficiaryFamilyFilter {
  page: number;
  pageSize: number;
  charityId?: string;
}

/**
 * UC-RPT-09 — one §23.S.13 grid row: a distinct family benefiting from seasonal-aid
 * assistance, with the campaign names aggregated.
 */
export interface BeneficiaryFamilyRow {
  familyCode: string;
  headOfFamilyName?: string | null;
  phone?: string | null;
  address?: string | null;
  charityId?: string | null;
  charityName?: string | null;
  campaignNames?: string | null;
}

// ---- §23.S.7 family projects (UC-RPT-11) ------------------------------------------

/** UC-RPT-11 — request for POST /api/Reports/registered-family-projects. */
export interface FamilyProjectsFilter {
  page: number;
  pageSize: number;
  charityId?: string;
}

/**
 * UC-RPT-11 — one §23.S.7 grid row: a family's registered project with the orphan
 * name/code/national-id strings joined. The two experience columns have no backing field —
 * keys stay in the contract, values render blank (recorded gap).
 */
export interface FamilyProjectRow {
  orphanNames?: string | null;
  orphanNationalIds?: string | null;
  guardianName?: string | null;
  orphanCodes?: string | null;
  familyCode: string;
  phone?: string | null;
  charityId?: string | null;
  charityName?: string | null;
  projectStatus?: string | null;
  yearsOfExperience?: number | null;
  totalBudget?: number | null;
  budgetCurrency?: string | null;
  projectStartDate?: string | null;
  projectAddress?: string | null;
  hasExperience?: boolean | null;
  projectDescription?: string | null;
}

// ---- §23.S.19 guardian change history (UC-RPT-12) --------------------------------

/** UC-RPT-12 — the §23.S.19 filter (charity narrow + paging). */
export interface ProviderChangesFilter {
  page: number;
  pageSize: number;
  charityId?: string;
}

/**
 * UC-RPT-12 — one §23.S.19 grid row: a provider assignment with the current guardian's
 * fields and the family/charity/orphan context. The three prior-guardian columns have no
 * backing field — keys stay in the contract, values render blank (recorded gap).
 */
export interface ProviderChangeRow {
  providerId: string;
  familyCode: string;
  familyId?: string | null;
  previousGuardianName?: string | null;
  previousRelationship?: string | null;
  changeReason?: string | null;
  newGuardianName?: string | null;
  newRelationship?: string | null;
  changeDate?: string | null;
  charityId?: string | null;
  charityName?: string | null;
  orphanCodes?: string | null;
}

// ---- §23.S.14 coded orphans needing a report (UC-RPT-15) -------------------------

/** UC-RPT-15 — the summary filter (charity narrow + paging). */
export interface OrphansMissingReportsFilter {
  page: number;
  pageSize: number;
  charityId?: string;
}

/** UC-RPT-15 — one summary row: a charity and its chase count. */
export interface OrphansMissingReportsSummaryRow {
  charityId?: string | null;
  charityName?: string | null;
  missingCount: number;
}

/** UC-RPT-15 — one drill-down row: an orphan in the charity's chase list. */
export interface OrphansMissingReportsDetailRow {
  orphanId: string;
  orphanCode: string;
  orphanName: string;
  lastReportDate?: string | null;
}

// ---- §23.S.15 orphans missing files (UC-RPT-16) ----------------------------------

/** UC-RPT-16 — the worklist filter (charity narrow + paging). */
export interface OrphansMissingFilesFilter {
  page: number;
  pageSize: number;
  charityId?: string;
}

/** UC-RPT-16 — one worklist row: an orphan whose latest report flags missing documents. */
export interface OrphansMissingFilesRow {
  orphanId: string;
  familyId?: string | null;
  orphanCode: string;
  orphanName: string;
  address?: string | null;
  village?: string | null;
  charityId?: string | null;
  charityName?: string | null;
  missingDocumentsName?: string | null;
}

// ---- §23.S.16 review queue — awaiting approval (UC-RPT-17) ----------------------

export interface ReportsAwaitingApprovalFilter {
  page: number;
  pageSize: number;
  charityId?: string;
}

/** UC-RPT-17 — one queued report: submitted, neither accepted nor refused. */
export interface ReportsAwaitingApprovalRow {
  reportId: string;
  orphanId: string;
  charityName?: string | null;
  orphanName: string;
  reportDate: string;
  orphanCode: string;
  /** Always null on this screen — the column exists because the legacy grid is shared with §23.S.17 (18-18). */
  refuseReason?: string | null;
}

// ---- §23.S.17 refused worklist (UC-RPT-18) --------------------------------------

export interface RefusedReportsFilter {
  page: number;
  pageSize: number;
  charityId?: string;
}

/** UC-RPT-18 — one refused report; the reason is the meaningful column here. */
export interface RefusedReportsRow {
  reportId: string;
  orphanId: string;
  charityId?: string | null;
  charityName?: string | null;
  orphanName: string;
  reportDate: string;
  orphanCode: string;
  refuseReason?: string | null;
  /** Carried for the label resolution — not rendered. */
  refuseReasonId?: number | null;
  reviewedDate?: string | null;
  reviewerId?: string | null;
}

// ---- §23.S.10 family & orphan entry tracking (UC-RPT-13) -------------------------

/** UC-RPT-13 — إجماليات: families and orphans entered since the date. */
export interface FamilyEntryTotals {
  newFamilies: number;
  newOrphans: number;
  sinceDate?: string | null;
}

/** UC-RPT-13 — تفاصيل: one row per family entered on/after the date. */
export interface FamilyEntryDetailRow {
  familyId: string;
  familyCode: string;
  headOfFamily: string;
  charityName?: string | null;
  registrationDate?: string | null;
  orphansCount: number;
}

// ---- §14.U.14 / §23.U.19 non-renewed chase list (UC-ORR-14 / UC-RPT-19) -----------

/** UC-ORR-14 / UC-RPT-19 — request for POST /api/Reports/non-renewed-reports. */
export interface NonRenewedReportsRequest {
  charityId?: string;
  /** 18-19 رقم الدفعة — batch mode; absent (no window) means the current batch. */
  batchId?: string;
  dateFrom?: string;
  dateTo?: string;
  /** The legacy _Number screens — response carries just the count. */
  countOnly?: boolean;
  page?: number;
  pageSize?: number;
}

/** One chase-list row — the orphan and, if any, its latest report date. */
export interface NonRenewedOrphanRow {
  orphanId: string;
  code: string;
  fullName: string;
  familyCode?: string | null;
  lastReportDate?: string | null;
  charityId?: string | null;
  charityName?: string | null;
  /** 18-19 الدفعة — the resolved batch number (batch mode only). */
  batchNo?: string | null;
}

/** The chase list — count is always the full matched count; items empty when countOnly. */
export interface NonRenewedReportsResult {
  count: number;
  items: NonRenewedOrphanRow[];
  page: number;
  pageSize: number;
  /** ReportPagedResult wire aliases (18-19). */
  totalCount?: number;
  totalPages?: number;
}

// ---- §23.S.12 charity payment tracking (UC-RPT-20) -------------------------------

/** UC-RPT-20 — filter for POST /api/Reports/charity-payment-tracking. */
export interface CharityPaymentTrackingFilter {
  page: number;
  pageSize: number;
  charityId?: string;
  batchId?: string;
  /** 18-21's print input — rides the filter, not consumed by the tracking query. */
  dateOfStartingUpdate?: string;
}

/** One tracking row — a charity's presence in the batch. */
export interface CharityPaymentTrackingRow {
  charityId: string;
  charityName?: string | null;
  orphansInBatch: number;
  reportsEntered: number;
  batchUploaded: boolean;
  uploadDate?: string | null;
}

/** UC-RPT-21 — filter for POST /api/Reports/family-update-tracking. */
export interface FamilyUpdateTrackingFilter {
  page: number;
  pageSize: number;
  /** The payment anchor (precise). */
  paymentId?: string;
  /** The panel's batch field — resolves the batch's latest payment. */
  batchNo?: string;
  /** من فضلك ادخل تاريخ بدا التحديث — required. */
  date: string;
  charityId?: string;
}

/** One family-update sheet row — a family file refreshed in the window. */
export interface FamilyUpdateTrackingRow {
  familyId: string;
  familyCode: string;
  headOfFamily: string;
  charityName?: string | null;
  updatedOn?: string | null;
}

/** UC-RPT-22 — filter for POST /api/Reports/missed-payments. */
export interface MissedPaymentsFilter {
  page: number;
  pageSize: number;
  charityId?: string;
  /** UC-RPT-23's HQ-only widened scope — refused server-side for non-HQ callers. */
  allOrphans?: boolean;
}

/**
 * One arrears row — an orphan with at least one entitled-but-unreceived batch.
 * batchStates keys drive the grid's dynamic batch columns; reason carries no
 * persisted source and renders as a dash (real-data-only ruling).
 */
export interface MissedPaymentRow {
  orphanId: string;
  orphanCode: string;
  orphanName: string;
  charityName?: string | null;
  reason?: string | null;
  batchStates: Record<string, boolean>;
  missedBatches: number;
}

// ---- §23.S.18 orphan photograph manifest (UC-RPT-24) ------------------------------

/** UC-RPT-24 — filter for POST /api/Reports/orphan-files/export. من تاريخ required. */
export interface OrphanFileFilter {
  page: number;
  pageSize: number;
  charityId?: string;
  dateFrom: string;
  dateTo?: string;
}

/**
 * One manifest row — a photograph attached to an accepted periodic report inside the
 * window. downloadUrl targets the live attachment endpoint; the ExcelJS workbook and the
 * grid thumbnail both stream through it (authenticated fetch — an <img src> cannot carry
 * the Bearer header).
 */
export interface OrphanFileManifestRow {
  orphanId: string;
  orphanCode: string;
  orphanName: string;
  charityName?: string | null;
  attachmentId: string;
  fileName?: string | null;
  contentType?: string | null;
  downloadUrl: string;
}

/** UC-RPT-28 — filter for POST /api/Reports/orphans-without-payment. الدفعة required. */
export interface OrphansWithoutPaymentFilter {
  paymentId: string;
  charityId?: string;
  page: number;
  pageSize: number;
}

/**
 * One zero-disbursement gap row — an orphan of the batch with no cheque, no transfer and
 * nothing received. amount is the expected entitlement (المبلغ المستحق); isStopped marks
 * the وقف الصرف subset (still a gap — nothing was disbursed).
 */
export interface OrphansWithoutPaymentRow {
  orphanId: string;
  orphanCode: string;
  orphanName: string;
  charityName?: string | null;
  amount: number;
  isStopped: boolean;
}

// ---- §23.U.29 received / not-received / stopped lists (UC-RPT-29) ----------------

/** UC-RPT-29 — filter for POST /api/Reports/payments-received. One endpoint; the
 *  variant discriminator picks the list. Batch number required. */
export type PaymentsOutcomeVariant = 'received' | 'notReceived' | 'stopped';

export interface PaymentsOutcomeFilter {
  variant: PaymentsOutcomeVariant;
  orpCheckBatchNo: string;
  charityId?: string;
}

/**
 * One row of any of the three lists — المستلمون / غير المستلمين / الموقوفون of one
 * cheque batch. collectorName is من استلم (the name recorded at receipt); chiqueNo and
 * printDate only carry values where the flow recorded them.
 */
export interface PaymentsOutcomeRow {
  orphanId: string;
  orphanCode: string;
  orphanName: string;
  guardianName?: string | null;
  amount: number;
  chiqueNo?: string | null;
  printDate?: string | null;
  collectorName?: string | null;
}

/** The report envelope — header + rows + totals. Empty variant ⇒ rows:[] + message (AC 4). */
export interface PaymentsOutcomeReport {
  variant: PaymentsOutcomeVariant;
  batchNo: string;
  charityName?: string | null;
  periodFrom?: string | null;
  periodTo?: string | null;
  currency?: string | null;
  rows: PaymentsOutcomeRow[];
  totalCount: number;
  totalAmount: number;
  message?: string | null;
}

// ---- §23.U.30 cheque numbers list (UC-RPT-30) ------------------------------------

/** UC-RPT-30 — filter for POST /api/Reports/cheque-numbers. Batch number required;
 *  bank/date/type filtering is 18-33's dimension, not this sheet's. */
export interface ChequeNumbersFilter {
  orpCheckBatchNo: string;
  charityId?: string;
}

/** One sheet row — a payment item with a cheque number recorded (10-12's settlement). */
export interface ChequeNumbersRow {
  orphanId: string;
  orphanCode: string;
  orphanName: string;
  guardianName?: string | null;
  amount: number;
  chiqueNo?: string | null;
  printDate?: string | null;
  collectorName?: string | null;
}

/** The report envelope — header + rows + totals. No cheques ⇒ rows:[] + message (AC 4). */
export interface ChequeNumbersReport {
  batchNo: string;
  charityName?: string | null;
  periodFrom?: string | null;
  periodTo?: string | null;
  currency?: string | null;
  rows: ChequeNumbersRow[];
  totalCount: number;
  totalAmount: number;
  message?: string | null;
}

/** UC-RPT-31 filter — كروت الاستلام of one cheque batch (§23.U.31). */
export interface ReceiptCardsFilter {
  orpCheckBatchNo: string;
  charityId?: string;
}

/** One card — one orphan's payment handed to that orphan's guardian (signed at collection). */
export interface ReceiptCardRow {
  orphanId: string;
  orphanCode: string;
  orphanName: string;
  guardianName?: string | null;
  amount: number;
  chiqueNo?: string | null;
}

/**
 * The cards envelope — header + one row per (guardian, orphan) payment item. Non-paged by
 * design: cards print in one pass. No rows in scope ⇒ rows:[] + message (AC 3).
 */
export interface ReceiptCardsReport {
  batchNo: string;
  charityName?: string | null;
  periodFrom?: string | null;
  periodTo?: string | null;
  currency?: string | null;
  rows: ReceiptCardRow[];
  totalCount: number;
  totalAmount: number;
  message?: string | null;
}

/** UC-RPT-35 — the four collapsed legacy variants (canonical lowercase keys). */
export type NewBeneficiariesVariant = 'orphans' | 'orphansv2' | 'widows' | 'widowsbyfamily';

/** UC-RPT-35 filter — charityId (HQ narrow), the registration window (dateFrom mandatory), variant. */
export interface NewBeneficiariesFilter {
  charityId?: string;
  dateFrom: string;
  dateTo?: string;
  variant: NewBeneficiariesVariant;
  page: number;
  pageSize: number;
}

/**
 * UC-RPT-35 row — the superset of the four variants' columns; each variant fills its own and
 * leaves the rest null (the screen picks the variant's column set).
 */
export interface NewBeneficiaryRow {
  orphanCode?: string | null;
  orphanName?: string | null;
  birthDate?: string | null;
  familyCode?: string | null;
  guardianName?: string | null;
  widowName?: string | null;
  nationalId?: string | null;
  husbandDeathDate?: string | null;
  childrenCount: number;
  charityName?: string | null;
  registrationDate?: string | null;
}

/** UC-RPT-35 envelope — the paged rows + the canonical variant echo. */
export interface NewBeneficiariesReport {
  variant: NewBeneficiariesVariant;
  items: NewBeneficiaryRow[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

/** UC-RPT-36 sheet selector — متابعة / متابعة الأسر / تسليم (wire: the enum name). */
export type FollowUpSheetVariant = 'FollowUp' | 'FollowUpFamily' | 'Tasleem';

/** UC-RPT-36 filter — GET /api/Reports/follow-up-sheets query. */
export interface FollowUpSheetFilter {
  variant: FollowUpSheetVariant;
  charityId?: string;
  dateFrom?: string;
  dateTo?: string;
  page: number;
  pageSize: number;
}

/**
 * UC-RPT-36 row — the superset of the three sheets' columns; each variant fills its own and
 * leaves the rest null (the screen picks the variant's column set). Result envelope is the
 * shared ReportPagedResult<FollowUpSheetRow>.
 */
export interface FollowUpSheetRow {
  orphanCode?: string | null;
  orphanName?: string | null;
  guardianName?: string | null;
  familyCode?: string | null;
  charityName?: string | null;
  sponsorshipStatus?: string | null;
  lastReportDate?: string | null;
  monthlyAmount?: number | null;
  headOfFamily?: string | null;
  orphansCount: number;
  familyStatus?: string | null;
  registrationDate?: string | null;
  lastUpdate?: string | null;
}

/** UC-RPT-37 identification-sheet selector — wire: the enum name (تعريف العائل / الأرامل / أسرة محددة). */
export type GuardianIdentificationVariant = 'AllGuardians' | 'WidowsOnly' | 'SingleFamily';

/** UC-RPT-37 filter — POST /api/Reports/guardian-identification-sheets. */
export interface GuardianIdentificationFilter {
  variant: GuardianIdentificationVariant;
  charityId?: string;
  date?: string;
  familyId?: string;
}

/** UC-RPT-37 row — the guardians'/widows' column superset; each variant fills its own. */
export interface GuardianIdentificationRow {
  guardianName?: string | null;
  widowName?: string | null;
  nationalId?: string | null;
  phone?: string | null;
  relationshipToFamily?: string | null;
  job?: string | null;
  birthDate?: string | null;
  familyCode?: string | null;
  charityName?: string | null;
}

/** UC-RPT-37 payload — the WHOLE selection (a print document is never page 1) + the server-stamped producer. */
export interface GuardianIdentificationSheet {
  variant: string;
  producedBy?: string | null;
  producedOn: string;
  totalCount: number;
  rows: GuardianIdentificationRow[];
  message?: string | null;
  /** Review P22 2026-08-26: the server caps the sheet at its row ceiling (P14) — true warns before print/preview. */
  truncated: boolean;
}

/** §23.U.38 filter — window + optional category; charityId is an HQ-only narrow. */
export interface MissingOutgoingAttachmentsFilter {
  charityId?: string;
  dateFrom?: string;
  dateTo?: string;
  outgoingCategoryId?: number;
  page: number;
  pageSize: number;
}

/** §23.U.38 row — an outgoing letter inside the window carrying zero attachment rows. */
export interface MissingOutgoingAttachmentsRow {
  serial: number | null;
  outGoingNumber: string | null;
  subject: string | null;
  date: string | null;
  year: number | null;
  categoryName: string | null;
  charityId?: string | null;
  charityName?: string | null;
  attachmentCount: number;
}

/** §23.U.39 filter — one charity + one as-at date (a page = families). */
export interface FamilyOrphansByDateFilter {
  charityId?: string;
  date: string;
  page: number;
  pageSize: number;
}

/** §23.U.39 orphan row — one orphan inside its family group. */
export interface FamilyOrphanRow {
  code: string;
  fullName: string;
  dateOfBirth: string | null;
  age: number | null;
}

/** §23.U.39 family group — the header band plus its orphans. */
export interface FamilyWithOrphans {
  familyCode: string;
  headOfFamily: string;
  regionName: string | null;
  centerName: string | null;
  registrationDate: string;
  orphansCount: number;
  orphans: FamilyOrphanRow[];
}
