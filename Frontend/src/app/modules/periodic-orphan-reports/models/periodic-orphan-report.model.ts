/**
 * Periodic Orphan Reports Module Models
 * Implements UC-6.11 through UC-6.17 for Periodic Orphan Reports
 */

// ==================== PERIODIC ORPHAN REPORT MODELS ====================

/** Orphan identity returned by the by-code lookup that prefaces report entry (UC-ORR-02). */
export interface OrphanLookupDto {
  orphanId: string;
  code?: string;
  fullName?: string;
  charityId?: string;
  charityName?: string;
  gender?: string;
  age?: number;
  birthDate?: string;
  familyId?: string;
  familyCode?: string;
  guardianName?: string;
  totalReports: number;
  pendingReports: number;
  coded: boolean;
}

export interface PeriodicOrphanReportDto {
  id: string;
  orphanId: string;
  orphanCode?: string;
  orphanName?: string;
  orphanPaymentId?: string;
  reportDate: string;
  reportPeriodFrom?: string;
  reportPeriodTo?: string;
  reportNo?: string;
  charityId?: string;
  charityName?: string;

  // Religious & Behavioral
  prayerStatus?: string;
  mannersStatus?: string;
  hadeethStatus?: string;

  // Quran Education
  quranParts?: string;
  quranVerses?: string;

  // Health & Medical
  medicalStatus?: string;
  disease?: string;
  disability?: string;
  disabilityDescription?: string;
  diseaseDescription?: string;
  medicalReportImageId?: string;
  medicalReportImageUrl?: string;

  // Personal Development
  hobby?: string;
  course?: string;
  courseName?: string;
  sportName?: string;
  professionName?: string;
  achievement?: string;
  achievementArr?: string;
  wish?: string;
  wishArr?: string;
  orphanMessage?: string;

  // Education Details
  educationalStageId?: number;
  educationalStageName?: string;
  educationalLevelId?: number;
  educationalLevelName?: string;
  grade?: string;
  school?: string;
  schoolType?: string;
  educationDegree?: string;
  highestEducationalLevel?: string;
  highestEducationalLevelYear?: number;
  isOrphanStudent?: boolean;
  educationalYear?: number;
  annualFeeForStudy?: number;
  studyingYears?: number;
  restStudyingYears?: number;
  graduationYear?: number;
  dropOut?: boolean;
  dropOutYear?: number;
  dropOutStageId?: number;
  dropOutStageName?: string;
  faculty?: string;
  department?: string;
  specialization?: string;

  // Life Events
  married?: boolean;
  marriageDate?: string;
  orphanMarriageImageId?: string;
  orphanMarriageImageUrl?: string;
  dead?: boolean;
  deathDate?: string;
  orphanDeadImageId?: string;
  orphanDeadImageUrl?: string;

  // Attachments
  orphanCertificateImageId?: string;
  orphanCertificateImageUrl?: string;
  orphanImageId?: string;
  orphanImageUrl?: string;
  missingDocuments?: boolean;
  missingDocumentsName?: string;

  // Status Tracking
  reviewed: boolean;
  reviewedDate?: string;
  reviewerId?: string;
  reviewerName?: string;
  locked: boolean;
  lockedDate?: string;
  active: boolean;
  activeDate?: string;
  deleted: boolean;
  deletedDate?: string;
  isAccepted: boolean;
  isRefused: boolean;
  refuseReason?: string;
  refuseReasonId?: number;
  refuseReasonName?: string;
  reviewComments?: string;
  messageId?: number;

  // Computed
  reviewStatus: string;

  // Audit
  createdOn: string;
  createdBy?: string;
  createdByName?: string;
  updatedOn: string;
  updatedBy?: string;
  updatedByName?: string;
}

export interface PeriodicOrphanReportListDto {
  id: string;
  reportNo?: string;
  reportDate: string;
  reportPeriodFrom?: string;
  reportPeriodTo?: string;
  orphanId: string;
  orphanCode?: string;
  orphanName?: string;
  charityId?: string;
  charityName?: string;
  prayerStatus?: string;
  educationalLevelId?: number;
  educationalLevelName?: string;
  medicalStatus?: string;
  reviewed: boolean;
  isAccepted: boolean;
  isRefused: boolean;
  reviewerId?: string;
  reviewerName?: string;
  reviewedDate?: string;
  refuseReasonId?: number;
  refuseReason?: string;
  reviewComments?: string;
  locked?: boolean;
  reviewStatus: string;
  married?: boolean;
  dead?: boolean;
  createdOn: string;

  // §14.S.4 orphan-status wide grid (UC-ORR-09) — orphan/family/guardian flatten
  orphanDateOfBirth?: string;
  orphanGender?: string;
  orphanNationalId?: string;
  orphanPhone?: string;
  familyCode?: string;
  guardianName?: string;
  guardianRelation?: string;
  guardianNationalId?: string;
  guardianJob?: string;
  guardianEducationLevelName?: string;
  regionName?: string;
  centerName?: string;
  cityVillage?: string;
  address?: string;
  homePhone?: string;
  schoolType?: string;
  faculty?: string;
  school?: string;
  marriageDate?: string;
  deathDate?: string;
  disease?: string;
  disability?: string;
  grade?: string;
  specialization?: string;
  educationDegree?: string;
  updatedOn?: string;
}

export interface CreatePeriodicOrphanReportDto {
  orphanId: string;
  orphanPaymentId?: string;
  reportDate: string;
  reportPeriodFrom?: string;
  reportPeriodTo?: string;
  reportNo?: string;

  // Religious & Behavioral
  prayerStatus?: string;
  mannersStatus?: string;
  hadeethStatus?: string;

  // Quran Education
  quranParts?: string;
  quranVerses?: string;

  // Health & Medical
  medicalStatus?: string;
  disease?: string;
  disability?: string;
  disabilityDescription?: string;
  diseaseDescription?: string;
  medicalReportImageId?: string;

  // Personal Development
  hobby?: string;
  course?: string;
  courseName?: string;
  sportName?: string;
  professionName?: string;
  achievement?: string;
  achievementArr?: string;
  wish?: string;
  wishArr?: string;
  orphanMessage?: string;

  // Education Details
  educationalStageId?: number;
  educationalLevelId?: number;
  grade?: string;
  school?: string;
  schoolType?: string;
  educationDegree?: string;
  highestEducationalLevel?: string;
  highestEducationalLevelYear?: number;
  isOrphanStudent?: boolean;
  educationalYear?: number;
  annualFeeForStudy?: number;
  studyingYears?: number;
  restStudyingYears?: number;
  graduationYear?: number;
  dropOut?: boolean;
  dropOutYear?: number;
  dropOutStageId?: number;
  faculty?: string;
  department?: string;
  specialization?: string;

  // Life Events
  married?: boolean;
  marriageDate?: string;
  orphanMarriageImageId?: string;
  dead?: boolean;
  deathDate?: string;
  orphanDeadImageId?: string;

  // Attachments
  orphanCertificateImageId?: string;
  orphanImageId?: string;
  missingDocuments?: boolean;
  missingDocumentsName?: string;
}

export interface UpdatePeriodicOrphanReportDto {
  id: string;
  orphanPaymentId?: string;
  reportDate?: string;
  reportPeriodFrom?: string;
  reportPeriodTo?: string;
  reportNo?: string;

  // Religious & Behavioral
  prayerStatus?: string;
  mannersStatus?: string;
  hadeethStatus?: string;

  // Quran Education
  quranParts?: string;
  quranVerses?: string;

  // Health & Medical
  medicalStatus?: string;
  disease?: string;
  disability?: string;
  disabilityDescription?: string;
  diseaseDescription?: string;
  medicalReportImageId?: string;

  // Personal Development
  hobby?: string;
  course?: string;
  courseName?: string;
  sportName?: string;
  professionName?: string;
  achievement?: string;
  achievementArr?: string;
  wish?: string;
  wishArr?: string;
  orphanMessage?: string;

  // Education Details
  educationalStageId?: number;
  educationalLevelId?: number;
  grade?: string;
  school?: string;
  schoolType?: string;
  educationDegree?: string;
  highestEducationalLevel?: string;
  highestEducationalLevelYear?: number;
  isOrphanStudent?: boolean;
  educationalYear?: number;
  annualFeeForStudy?: number;
  studyingYears?: number;
  restStudyingYears?: number;
  graduationYear?: number;
  dropOut?: boolean;
  dropOutYear?: number;
  dropOutStageId?: number;
  faculty?: string;
  department?: string;
  specialization?: string;

  // Life Events
  married?: boolean;
  marriageDate?: string;
  orphanMarriageImageId?: string;
  dead?: boolean;
  deathDate?: string;
  orphanDeadImageId?: string;

  // Attachments
  orphanCertificateImageId?: string;
  orphanImageId?: string;
  missingDocuments?: boolean;
  missingDocumentsName?: string;
}

export interface ReviewPeriodicReportDto {
  reportId: string;
  isApproved: boolean;
  refuseReason?: string;
  refuseReasonId?: number;
  reviewComments?: string;
}

export interface PeriodicOrphanReportFilterDto {
  searchTerm?: string;
  orphanCode?: string;
  orphanName?: string;
  orphanId?: string;
  charityId?: string;
  reportNo?: string;
  reviewStatus?: string;
  reviewed?: boolean;
  isAccepted?: boolean;
  isRefused?: boolean;
  reviewerId?: string;
  reportDateFrom?: string;
  reportDateTo?: string;
  educationalStageId?: number;
  educationalLevelId?: number;
  medicalStatus?: string;
  active?: boolean;
  locked?: boolean;
  andOr?: string;
  schoolType?: string;
  maritalStatus?: string;
  educationalStatus?: string;
  educationDegree?: string;
  pageNumber?: number;
  pageSize?: number;
  sortBy?: string;
  sortDirection?: string;
}

export interface PeriodicOrphanReportSummaryDto {
  totalReports: number;
  pendingReports: number;
  approvedReports: number;
  rejectedReports: number;
  lockedReports: number;
  orphanId: string;
  orphanName?: string;
  totalReportsForOrphan: number;
}

export interface PeriodicOrphanReportPagedResult {
  items: PeriodicOrphanReportListDto[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

/**
 * Register statistics band above the periodic reports grid (§14.S.1, UC-ORR-01) —
 * caller-scoped server-side (charity pin or country pin), so the counts match what
 * the grid under them can show.
 */
export interface PeriodicOrphanReportStatistics {
  total: number;
  /** Reviewed && isAccepted rows — the register's "approved" predicate. */
  accepted: number;
  /** Rows still awaiting review (!reviewed) — the reviewer's pending queue. */
  pending: number;
  addedThisMonth: number;
}

// 18-14 / UC-RPT-14 — status-grouped counts; pending is derived all − accepted − refused.
// A null leg means its request failed — the tile renders "—" instead of a wrong number.
export interface OrphanReportStatusCounts {
  all: number | null;
  accepted: number | null;
  refused: number | null;
  pending: number | null;
}

// ==================== ORPHAN SUMMARY REPORT MODELS ====================

export interface OrphanReportFilterDto {
  /** Optional on the SPA side — the grouped branch (9-10) needs no window; the
   *  numbers (9-15) and generate (9-11) branches always send both ends. */
  fromDate?: string;
  toDate?: string;
  charityId?: string;
  reportNo?: string;
  /** §14.U.15 — fill the statistics response's reportNumbers branch. */
  includeReportNumbers?: boolean;
  regionId?: number;
  centerId?: number;
  sponsorshipStatus?: string;
  ageFrom?: number;
  ageTo?: number;
  gender?: string;
  includeFamilyDetails?: boolean;
  includeContactInformation?: boolean;
  includeEducationDetails?: boolean;
  includeHealthDetails?: boolean;
  groupByCharity?: boolean;
}

export interface OrphanReportResultDto {
  metadata: OrphanReportMetadataDto;
  summary: OrphanReportSummaryDto;
  orphans: OrphanReportDto[];
  /** §14.U.11 detailed periodic-report rows (UC-ORR-11). */
  reports: OrphanReportDetailRow[];
  reportsTotalCount: number;
  generatedOn: string;
}

/** One §14.U.11 detailed extract row — report dimensions + orphan identity + charity. */
export interface OrphanReportDetailRow {
  reportId: string;
  reportNo?: string;
  reportDate: string;
  reportPeriodFrom?: string;
  reportPeriodTo?: string;
  orphanCode: string;
  orphanName: string;
  charityName?: string;
  reviewStatus: string;
  reviewed: boolean;
  isAccepted: boolean;
  isRefused: boolean;
  refuseReason?: string;
  schoolType?: string;
  school?: string;
  faculty?: string;
  specialization?: string;
  grade?: string;
  educationDegree?: string;
  educationalLevelId?: number;
  educationalLevelName?: string;
  medicalStatus?: string;
  disease?: string;
  disability?: string;
  married?: boolean;
  marriageDate?: string;
  dead?: boolean;
  deathDate?: string;
}

export interface OrphanReportMetadataDto {
  fromDate: string;
  toDate: string;
  charityId?: string;
  charityName?: string;
  regionId?: number;
  regionName?: string;
  centerId?: number;
  centerName?: string;
  sponsorshipStatus?: string;
  ageFrom?: number;
  ageTo?: number;
  gender?: string;
  includeFamilyDetails: boolean;
  includeContactInformation: boolean;
  includeEducationDetails: boolean;
  includeHealthDetails: boolean;
  groupByCharity: boolean;
  generatedByUserId?: string;
  generatedByUserName?: string;
}

export interface OrphanReportSummaryDto {
  totalOrphans: number;
  sponsoredCount: number;
  unsponsoredCount: number;
  pendingCount: number;
  maleCount: number;
  femaleCount: number;
  age0To5Count: number;
  age6To12Count: number;
  age13To18Count: number;
  age19PlusCount: number;
  charityBreakdown?: CharityBreakdownDto[];
  fromDate: string;
  toDate: string;
}

export interface CharityBreakdownDto {
  charityId: string;
  charityName: string;
  orphanCount: number;
  sponsoredCount: number;
  unsponsoredCount: number;
}

export interface OrphanReportDto {
  orphanId: string;
  orphanCode: string;
  orphanName: string;
  dateOfBirth?: string;
  age?: number;
  gender?: string;
  orphanType?: string;
  sponsorshipStatus?: string;
  charityId?: string;
  charityName?: string;
  regionId?: number;
  regionName?: string;
  centerId?: number;
  centerName?: string;
  sponsorId?: string;
  sponsorName?: string;
  monthlyAmount?: number;
  familyAddress?: string;
  fatherName?: string;
  motherName?: string;
  providerName?: string;
  familyPhone?: string;
  orphanPhone?: string;
  orphanEmail?: string;
  educationLevelId?: number;
  educationLevelName?: string;
  schoolName?: string;
  gradeClass?: string;
  academicPerformance?: string;
  healthStatusId?: number;
  healthStatusName?: string;
  disabilities?: string;
  chronicDiseases?: string;
  photoAttachmentId?: string;
  photoUrl?: string;
}

export interface OrphanReportExportDto {
  exportFormat: string;
  fileName?: string;
  includeAllFields?: boolean;
  includeSummaryOnly?: boolean;
}

export interface OrphanReportHistoryDto {
  reportId: string;
  reportName?: string;
  reportFromDate: string;
  reportToDate: string;
  generatedDate: string;
  generatedBy?: string;
  orphanCount: number;
  filtersApplied?: string;
  exportFormat: string;
}

export interface OrphanReportComparisonDto {
  report1: OrphanReportSummaryDto;
  report2: OrphanReportSummaryDto;
  comparison: OrphanReportComparisonMetricsDto;
}

export interface OrphanReportComparisonMetricsDto {
  totalCountChange: number;
  totalCountPercentChange: number;
  sponsoredCountChange: number;
  unsponsoredCountChange: number;
  age0To5CountChange: number;
  age6To12CountChange: number;
  age13To18CountChange: number;
  age19PlusCountChange: number;
  maleCountChange: number;
  femaleCountChange: number;
  charityComparison?: CharityComparisonDto[];
}

export interface CharityComparisonDto {
  charityId: string;
  charityName: string;
  report1Count: number;
  report2Count: number;
  countChange: number;
  percentChange: number;
}

export interface ScheduleRecurringReportDto {
  reportName: string;
  frequency: string;
  dayOfMonth?: number;
  emailRecipients: string;
  filter: OrphanReportFilterDto;
}

export interface ScheduledReportDto {
  scheduleId: string;
  reportName: string;
  frequency: string;
  dayOfMonth?: number;
  emailRecipients: string;
  nextRunDate?: string;
  lastRunDate?: string;
  isActive: boolean;
  createdOn: string;
}

export interface OrphanStatisticsDto {
  totalOrphans: number;
  sponsoredCount: number;
  unsponsoredCount: number;
  pendingCount: number;
  maleCount: number;
  femaleCount: number;
  /** §14.S.3 grouped rows (UC-ORR-10) — one per educational status × level. */
  groups: OrphanReportGroupCountRow[];
  /** §14.U.15 numbers-in-period rows (UC-ORR-15) — filled when includeReportNumbers + window. */
  reportNumbers: OrphanReportNumberRow[];
  reportNumbersCount: number;
  unnumberedReportsCount: number;
}

/** One §14.S.3 grouped-statistics row (UC-ORR-10). */
export interface OrphanReportGroupCountRow {
  educationalStatus: string;
  educationalLevelName?: string;
  femaleCount: number;
  maleCount: number;
  totalCount: number;
}

/** One §14.U.15 numbers-in-period row (UC-ORR-15). */
export interface OrphanReportNumberRow {
  reportId: string;
  reportNo?: string | null;
  orphanCode: string;
  orphanName: string;
  reportDate: string;
  createdOn: string;
  reviewStatus: string;
}

/** One document slot carried into the print payload (UC-ORR-17); bytes fetched by id (BR-12). */
export interface OrphanReportFormAttachmentSlot {
  /** orphanPhoto · certificate · medicalReport · deathCertificate · marriageContract */
  slot: string;
  id: string;
}

/**
 * UC-ORR-17 print payload — POST /api/Reports/orphan-report-form/export/pdf. Per the
 * client-side print ruling the endpoint returns composed data, not PDF bytes: this screen
 * renders the official form and the browser's print-to-PDF produces the file.
 */
export interface OrphanReportFormPrintPayload {
  /** Collapsed 24-template matrix: disabled|studying|not-studying + document sections. */
  variant: string;
  report: PeriodicOrphanReportDto;
  attachments: OrphanReportFormAttachmentSlot[];
}
