/**
 * Orphan Payments Module Models
 * Handles orphan payment groups/batches for manual payment processing
 */

export interface OrphanPaymentDto {
  id: string;
  batchNo: string;
  groupName: string;
  description?: string;
  paymentPeriodFrom: string;
  paymentPeriodTo: string;
  groupDate: string;
  /** تاريخ بدء التوزيع — §15.S.2; null on legacy rows (list falls back to groupDate) */
  paymentDate?: string;
  exchangeRate: number;
  currency: string;
  dontRemoveRate: boolean;
  charityId?: string;
  charityName?: string;
  regionId?: number;
  regionName?: string;
  centerId?: number;
  centerName?: string;
  sponsorshipStatus?: string;
  ageFrom?: number;
  ageTo?: number;
  showOrder: number;
  notes?: string;
  isBatchUploaded: boolean;
  uploadDate?: string;
  orphanCount: number;
  createdOn: string;
  modifiedOn?: string;
  createdBy?: string;
  modifiedBy?: string;
  orphans?: OrphanPaymentItemDto[];
}

/**
 * 10-3 re-cut to the server wire (OrphanPaymentItemDto, camelCase). The previous shape
 * (orphanName/familyName/age/monthlyAmount/assignedOn/assignedBy/currency) matched no
 * server property — every row rendered empty.
 */
export interface OrphanPaymentItemDto {
  id: string;
  orphanPaymentId: string;
  orphanId: string;
  displayOrder?: number;
  notes?: string;
  // §15.1 row ledger (EP-10) — snapshot taken at enrolment (10-2); row actions 10-9..10-13
  /** BR-17 — amount snapshotted from the orphan's monthly amount at enrolment */
  amount?: number;
  isStopped?: boolean;
  stoppedOn?: string;
  isPrinted?: boolean;
  printedOn?: string;
  isGotIt?: boolean;
  receivedOn?: string;
  chiqueNum?: string;
  printdate?: string;
  benificiaryName?: string;
  transferNo?: string;
  exchangeStatus?: number;
  // Orphan details (server prefixes these with "orphan")
  orphanCode?: string;
  orphanFullName?: string;
  orphanFamilyName?: string | null;
  orphanAge?: number | null;
  orphanGender?: string | null;
  orphanEducationLevel?: string | null;
  orphanMonthlyAmount?: number | null;
  // Charity / location — CharityName stays null until 10-7 fixes the read join; render nothing then
  charityName?: string | null;
  regionName?: string | null;
  centerName?: string | null;
  // Sponsorship
  sponsorName?: string | null;
  sponsorshipStartDate?: string | null;
  sponsorshipStatus?: string | null;
}

export interface CreateOrphanPaymentDto {
  batchNo?: string;
  groupName: string;
  description?: string;
  paymentPeriodFrom: string;
  paymentPeriodTo: string;
  groupDate?: string;
  /** تاريخ بدء التوزيع — §15.S.2 mandatory */
  paymentDate?: string;
  exchangeRate?: number;
  currency?: string;
  dontRemoveRate?: boolean;
  showOrder?: number;
  notes?: string;
}

export interface UpdateOrphanPaymentDto {
  batchNo?: string;
  groupName?: string;
  description?: string;
  paymentPeriodFrom?: string;
  paymentPeriodTo?: string;
  groupDate?: string;
  /** تاريخ بدء التوزيع — §15.S.2 */
  paymentDate?: string;
  exchangeRate?: number;
  currency?: string;
  dontRemoveRate?: boolean;
  showOrder?: number;
  notes?: string;
}

export interface OrphanPaymentSearchRequest {
  searchTerm?: string;
  /** P14 — exact batch-number match (رقم الدفعة from the reference list; trimmed compare). */
  batchNo?: string;
  charityId?: string;
  regionId?: number;
  centerId?: number;
  isBatchUploaded?: boolean;
  paymentPeriodFrom?: string;
  paymentPeriodTo?: string;
  groupDateFrom?: string;
  groupDateTo?: string;
  /** UC-ORP-08 — restrict to the batches containing this orphan (the orphan's payment history). */
  orphanId?: string;
  pageNumber?: number;
  pageSize?: number;
  sortBy?: string;
  sortDescending?: boolean;
}

/** UC-ORP-11 — batch-number reference row (رقم الحصة), most recent first. */
export interface BatchNumberOptionDto {
  batchNo: string;
  latestGroupDate?: string | null;
}

export interface OrphanPaymentPagedResult {
  items: OrphanPaymentDto[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
}

export interface AddOrphansToPaymentDto {
  orphanIds: string[];
}

export interface RemoveOrphanFromPaymentDto {
  orphanId: string;
}

/**
 * §15.1 row-action envelope (10-9) — ONE endpoint (POST orphan-items) for the flag family.
 * Property names verbatim from the wire contract; action 0 = stop/resume (flag carries the
 * direction), actions 1..4 land with 10-10..10-13. The DTO is frozen.
 */
export interface UpdateOrphanPaymentItemDto {
  orphanPaymentItemId: string;
  action: number;
  flag?: boolean | null;
  chiqueNum?: string | null;
  chiqueDate?: string | null;
  benificiaryName?: string | null;
}

export interface OrphanSelectionFilter {
  charityId?: string;
  regionId?: number;
  centerId?: number;
  sponsorshipStatus?: string;
  ageFrom?: number;
  ageTo?: number;
  gender?: string;
  searchTerm?: string;
  /** Review P18: the read pages server-side — the 200-row hard cap hid orphans beyond it. */
  pageNumber?: number;
  pageSize?: number;
}

/**
 * 10-2 re-cut to the live wire (server OrphanForPaymentListDto, camelCase):
 * the previous shape (orphanId/orphanName/isAlreadyInGroup) matched no server property.
 */
export interface OrphanForSelectionDto {
  id: string;
  code?: string;
  fullName: string;
  familyName?: string | null;
  age?: number | null;
  gender?: string | null;
  sponsorshipStatus?: string | null;
  charityId?: string | null;
  charityName?: string | null;
  regionName?: string | null;
  centerName?: string | null;
  /** Current monthly amount — preview of the BR-17 snapshot taken at enrolment */
  monthlyAmount?: number | null;
  isInGroup: boolean;
  orphanPaymentItemId?: string | null;
}

/** Server envelope for the available-orphans read (was typed as a bare array). */
export interface OrphanSelectionPagedResult {
  items: OrphanForSelectionDto[];
  totalCount: number;
}

/** Server envelope for the enrolment add: { message, addedCount, skippedCount }. */
export interface AddOrphansResultDto {
  message: string;
  addedCount: number;
  skippedCount: number;
}

export interface ExportPaymentGroupOptions {
  format: 'Excel' | 'PDF';
  includePhotos: boolean;
  groupBy?: 'Charity' | 'Region' | 'None';
}

// 10-3: OrphanPaymentAuditLog removed — GET {id}/audit-logs never existed server-side.

export interface OrphanPaymentStatistics {
  totalGroups: number;
  activeGroups: number;
  uploadedGroups: number;
  totalOrphansInGroups: number;
  byCharity: {
    charityId: number;
    charityName: string;
    groupCount: number;
    orphanCount: number;
  }[];
  byRegion: {
    regionId: number;
    regionName: string;
    groupCount: number;
    orphanCount: number;
  }[];
}

export interface BatchNumberGeneration {
  nextBatchNumber: string;
  currentHighestBatch: string;
}

/**
 * UC-RPT-32 (§23.U.32 صفحات ملخص الدفعة) — the batch view's totals band: the batch's cover
 * figures from GET /api/Dashboard/payment-summary. Every figure arrives server-side
 * null-safe; zeros are a valid result (an empty batch), never an error.
 */
export interface PaymentSummary {
  paymentId: string;
  batchNo: string | null;
  batchDate: string | null;
  charityName: string | null;
  periodFrom: string | null;
  periodTo: string | null;
  currency: string | null;
  orphanCount: number;
  totalAmount: number;
  receivedCount: number;
  receivedAmount: number;
  notReceivedCount: number;
  notReceivedAmount: number;
  stoppedCount: number;
  stoppedAmount: number;
  chequeCount: number;
  chequeTotal: number;
  message: string | null;
}

// Currency options
export const CURRENCY_OPTIONS = [
  { value: 'EGP', label: 'EGP - Egyptian Pound' },
  { value: 'SAR', label: 'SAR - Saudi Riyal' },
  { value: 'USD', label: 'USD - US Dollar' }
];

// Sponsorship status options
export const SPONSORSHIP_STATUS_OPTIONS = [
  { value: 'All', label: 'All' },
  { value: 'Sponsored', label: 'Sponsored' },
  { value: 'Unsponsored', label: 'Unsponsored' }
];

// Gender options
export const GENDER_OPTIONS = [
  { value: 'All', label: 'All' },
  { value: 'Male', label: 'Male' },
  { value: 'Female', label: 'Female' }
];

// Export group by options
export const GROUP_BY_OPTIONS = [
  { value: 'None', label: 'No Grouping' },
  { value: 'Charity', label: 'By Charity' },
  { value: 'Region', label: 'By Region' }
];
