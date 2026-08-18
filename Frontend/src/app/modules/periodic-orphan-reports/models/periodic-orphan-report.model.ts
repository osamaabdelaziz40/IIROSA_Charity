/**
 * Periodic Orphan Reports Module Models
 * Implements UC-6.11 through UC-6.17 for Periodic Orphan Reports
 */

// ==================== PERIODIC ORPHAN REPORT MODELS ====================

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
  educationalLevelName?: string;
  medicalStatus?: string;
  reviewed: boolean;
  isAccepted: boolean;
  isRefused: boolean;
  reviewerName?: string;
  reviewedDate?: string;
  reviewStatus: string;
  married?: boolean;
  dead?: boolean;
  createdOn: string;
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
  orphanId?: string;
  charityId?: string;
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
  orphanId: number;
  orphanName?: string;
  totalReportsForOrphan: number;
}

export interface PeriodicOrphanReportPagedResult {
  items: PeriodicOrphanReportListDto[];
  totalCount: number;
}

// ==================== ORPHAN SUMMARY REPORT MODELS ====================

export interface OrphanReportFilterDto {
  fromDate: string;
  toDate: string;
  charityId?: string;
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
  generatedOn: string;
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
}
