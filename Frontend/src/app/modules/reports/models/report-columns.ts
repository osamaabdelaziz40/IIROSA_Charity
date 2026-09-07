import {
  BeneficiaryFamilyRow,
  CharityPaymentTrackingRow,
  ExcludedOrphanRow,
  FollowUpSheetRow,
  MezaCardsRow,
  MissedPaymentRow,
  MissingOutgoingAttachmentsRow,
  NewBeneficiaryRow,
  NonRenewedOrphanRow,
  OrphanDataRow,
  OrphansMissingFilesRow,
  OrphansMissingReportsDetailRow,
  OrphansMissingReportsSummaryRow,
  OrphanStatusReportRow,
  ProviderChangeRow,
  RefusedReportsRow,
  ReportsAwaitingApprovalRow,
  WidowSponsorshipRow,
  FamilyProjectRow
} from './report.model';

/**
 * Structural shims for rows owned by other modules / the flattening component — the registry
 * declares shapes structurally instead of importing across module boundaries.
 */
export interface ChequeStatementItemShim {
  checkNumber: string;
  checkDate: string | null;
  beneficiaryName: string;
  amount: number;
  currency: string;
  chequeType: string | null;
  bankName?: string | null;
}

export interface FamilyFollowUpRowShim {
  code: string;
  headOfFamily: string;
  charityName?: string | null;
  changeKind: string;
  changedBy?: string | null;
  changedOn: string;
  orphansTouched: number;
}

/** 18-39's flattened export row — family cells repeat on every orphan row (see the set below). */
export interface FamilyOrphansFlatRow {
  familyCode: string;
  headOfFamily: string;
  regionCenter: string;
  orphansCount: number;
  orphanCode: string;
  orphanName: string;
  dateOfBirth: string | null;
  age: number | null;
}

/**
 * EP-18 export column-set registry (18-41) — the SINGLE place a report's Excel shape is
 * declared. The engine (`services/report-export.service.ts`) is the only ExcelJS consumer;
 * report components and the report-viewer shell hand over a report key + rows, never a
 * workbook.
 *
 * The serial column is engine-owned (every sheet opens with '#') — column sets start at the
 * first DATA column. Variants get their own keys (a variant is its own report shape). The
 * 18-24/25 image manifests register as special cases: images are not grid columns, so they
 * keep their dedicated sheet builder behind the same registry entry (story ruling).
 */

export type ReportColumnType = 'text' | 'number' | 'date' | 'boolean';

export interface ReportColumnDef<T = any> {
  /** Stable column id (Excel column key / autofilter anchor). */
  key: string;
  /** i18n key of the header label. */
  i18nLabel: string;
  type: ReportColumnType;
  width?: number;
  /** Reads the cell value off a row. */
  value: (row: T) => string | number | boolean | Date | null | undefined;
  /** i18n keys for the two boolean states; absent = the epic's ✓ / blank convention. */
  booleanLabels?: { true: string; false: string };
  /** i18n key written verbatim on every row (constant state columns). */
  constantLabel?: string;
  /** Date rendering — 'date' (dd/mm/yyyy, default) or 'datetime'. */
  format?: 'date' | 'datetime';
}

export interface ReportColumnSet<T = any> {
  reportKey: string;
  /** i18n key of the sheet (tab) title. */
  sheetKey: string;
  columns: ReportColumnDef<T>[];
  /** The image manifests keep their own builder — declared, not gridded (18-24/25 ruling). */
  specialCase?: 'image-manifest';
}

/** One page of an exportable read — the shape every paged report endpoint already returns. */
export interface ReportExportPage<T = any> {
  items: T[];
  totalCount: number;
}

/**
 * §23.U.41 استخراج البيانات — the shell's export request: the report's registry key plus a
 * page fetcher closing over the CURRENT filter (producers close over live component state —
 * the same binding shape as §23.U.40's preview producer). The engine walks every page under
 * the cap; a truncation is declared on a manifest sheet, never silent.
 */
export interface ReportExportRequest<T = any> {
  reportKey: string;
  fetchPage: (page: number, pageSize: number) => Promise<ReportExportPage<T>>;
  /** The endpoint validator's page-size cap (default 100). */
  pageSize?: number;
  /** No-silent-caps ceiling — the walk stops here and declares the truncation (default 5000). */
  maxRows?: number;
  /** Overrides the `<reportKey>-<yyyyMMdd>.xlsx` default (bank-file contracts). */
  fileName?: string;
  /** Overrides the registry's column set (dynamic column tails). */
  columns?: ReportColumnDef<T>[];
  /** Overrides the registry's sheet title. */
  sheetKey?: string;
}

export interface ReportExportResult {
  exportedCount: number;
  totalCount: number;
  truncated: boolean;
}

// ==================== 18-1 … 18-25 (the engine's original per-report sets) ====================

const orphanData: ReportColumnSet<OrphanDataRow> = {
  reportKey: 'orphan-data',
  sheetKey: 'reports.orphanData.title',
  columns: [
    { key: 'code', i18nLabel: 'reports.orphanData.colOrphanCode', type: 'text', width: 14, value: r => r.code },
    { key: 'fullName', i18nLabel: 'reports.orphanData.colOrphanName', type: 'text', width: 26, value: r => r.fullName },
    { key: 'nationalId', i18nLabel: 'reports.orphanData.colOrphanNationalId', type: 'text', width: 16, value: r => r.nationalId },
    { key: 'dateOfBirth', i18nLabel: 'reports.orphanData.colBirthDate', type: 'date', width: 14, value: r => r.dateOfBirth },
    { key: 'age', i18nLabel: 'reports.orphanData.colAge', type: 'number', value: r => r.age },
    { key: 'gender', i18nLabel: 'reports.orphanData.colGender', type: 'text', value: r => r.gender },
    { key: 'guardianName', i18nLabel: 'reports.orphanData.colGuardianName', type: 'text', width: 24, value: r => r.guardianName },
    { key: 'guardianRelationship', i18nLabel: 'reports.orphanData.colGuardianRelation', type: 'text', value: r => r.guardianRelationship },
    { key: 'guardianNationalId', i18nLabel: 'reports.orphanData.colGuardianNationalId', type: 'text', width: 16, value: r => r.guardianNationalId },
    { key: 'guardianEducationLevel', i18nLabel: 'reports.orphanData.colGuardianEducation', type: 'text', value: r => r.guardianEducationLevel },
    { key: 'guardianJob', i18nLabel: 'reports.orphanData.colGuardianJob', type: 'text', value: r => r.guardianJob },
    { key: 'guardianHealthStatus', i18nLabel: 'reports.orphanData.colGuardianHealth', type: 'text', value: r => r.guardianHealthStatus },
    { key: 'guardianSocialStatus', i18nLabel: 'reports.orphanData.colGuardianSocial', type: 'text', value: r => r.guardianSocialStatus },
    { key: 'guardianDevelopmentProject', i18nLabel: 'reports.orphanData.colGuardianProject', type: 'text', value: r => r.guardianDevelopmentProject },
    { key: 'governorateName', i18nLabel: 'reports.orphanData.colGovernorate', type: 'text', value: r => r.governorateName },
    { key: 'centerName', i18nLabel: 'reports.orphanData.colCenter', type: 'text', value: r => r.centerName },
    { key: 'cityVillage', i18nLabel: 'reports.orphanData.colVillage', type: 'text', value: r => r.cityVillage },
    { key: 'detailedAddress', i18nLabel: 'reports.orphanData.colAddress', type: 'text', width: 28, value: r => r.detailedAddress },
    { key: 'mobileNumber', i18nLabel: 'reports.orphanData.colMobile', type: 'text', value: r => r.mobileNumber },
    { key: 'mobileNumber2', i18nLabel: 'reports.orphanData.colMobile2', type: 'text', value: r => r.mobileNumber2 },
    { key: 'houseOwnershipName', i18nLabel: 'reports.orphanData.colHouseOwnership', type: 'text', value: r => r.houseOwnershipName },
    { key: 'rentAmount', i18nLabel: 'reports.orphanData.colRentValue', type: 'number', value: r => r.rentAmount },
    { key: 'housingTypeName', i18nLabel: 'reports.orphanData.colHousingType', type: 'text', value: r => r.housingTypeName },
    { key: 'houseStatusName', i18nLabel: 'reports.orphanData.colHouseStatus', type: 'text', value: r => r.houseStatusName },
    { key: 'monthlyIncome', i18nLabel: 'reports.orphanData.colIncomeValue', type: 'number', value: r => r.monthlyIncome },
    { key: 'socialStatusName', i18nLabel: 'reports.orphanData.colSocialStatus', type: 'text', value: r => r.socialStatusName },
    { key: 'healthStatusName', i18nLabel: 'reports.orphanData.colHealthStatus', type: 'text', value: r => r.healthStatusName },
    { key: 'profession', i18nLabel: 'reports.orphanData.colProfession', type: 'text', value: r => r.profession },
    { key: 'educationLevelName', i18nLabel: 'reports.orphanData.colEducationLevel', type: 'text', value: r => r.educationLevelName },
    { key: 'gradeClass', i18nLabel: 'reports.orphanData.colGradeClass', type: 'text', value: r => r.gradeClass },
    { key: 'schoolName', i18nLabel: 'reports.orphanData.colSchoolName', type: 'text', value: r => r.schoolName },
    { key: 'facultyName', i18nLabel: 'reports.orphanData.colFaculty', type: 'text', value: r => r.facultyName },
    { key: 'departmentName', i18nLabel: 'reports.orphanData.colDepartment', type: 'text', value: r => r.departmentName },
    { key: 'hasAcademicDegree', i18nLabel: 'reports.orphanData.colHasDegree', type: 'boolean', value: r => r.hasAcademicDegree },
    { key: 'fatherDeathDate', i18nLabel: 'reports.orphanData.colFatherDeathDate', type: 'date', width: 14, value: r => r.fatherDeathDate },
    { key: 'fatherDeathCause', i18nLabel: 'reports.orphanData.colFatherDeathCause', type: 'text', value: r => r.fatherDeathCause },
    { key: 'isExcluded', i18nLabel: 'reports.orphanData.colExcluded', type: 'boolean', value: r => r.isExcluded },
    { key: 'exclusionReason', i18nLabel: 'reports.orphanData.colExclusionReason', type: 'text', width: 24, value: r => r.exclusionReason },
    { key: 'notes', i18nLabel: 'reports.orphanData.colNotes', type: 'text', width: 32, value: r => r.notes },
    { key: 'charityName', i18nLabel: 'reports.orphanData.colCharity', type: 'text', value: r => r.charityName },
    { key: 'lastUpdatedDate', i18nLabel: 'reports.orphanData.colLastUpdated', type: 'date', width: 14, value: r => r.lastUpdatedDate }
  ]
};

const excludedOrphans: ReportColumnSet<ExcludedOrphanRow> = {
  reportKey: 'excluded-orphans',
  sheetKey: 'reports.excludedOrphans.title',
  columns: [
    { key: 'code', i18nLabel: 'reports.excludedOrphans.colOrphanCode', type: 'text', width: 14, value: r => r.code },
    { key: 'fullName', i18nLabel: 'reports.excludedOrphans.colOrphanName', type: 'text', width: 26, value: r => r.fullName },
    { key: 'isExcluded', i18nLabel: 'reports.excludedOrphans.colExcluded', type: 'boolean', value: r => r.isExcluded },
    { key: 'exclusionReason', i18nLabel: 'reports.excludedOrphans.colExclusionReason', type: 'text', width: 26, value: r => r.exclusionReason },
    { key: 'charityName', i18nLabel: 'reports.excludedOrphans.colCharity', type: 'text', value: r => r.charityName },
    { key: 'lastUpdatedDate', i18nLabel: 'reports.excludedOrphans.colLastUpdated', type: 'date', width: 14, value: r => r.lastUpdatedDate }
  ]
};

/**
 * 18-4/18-5's twin grids — one column SHAPE; the wrapper swaps the i18n block (keyPrefix)
 * and the file-name base.
 *
 * Review P27 2026-08-26: the keys are built by a factory taking the FULL i18n block. The
 * old set carried bare `colOrphanCode` fragments — they only resolved because
 * exportOrphanStatusReport prefixed them at runtime, so any other registry consumer
 * (exportRows('orphan-status')) rendered untranslated key fragments as headers.
 */
export function orphanStatusColumns(keyPrefix: string): ReportColumnDef<OrphanStatusReportRow>[] {
  return [
    { key: 'code', i18nLabel: `${keyPrefix}.colOrphanCode`, type: 'text', width: 14, value: r => r.code },
    { key: 'fullName', i18nLabel: `${keyPrefix}.colOrphanName`, type: 'text', width: 26, value: r => r.fullName },
    { key: 'charityName', i18nLabel: `${keyPrefix}.colCharity`, type: 'text', value: r => r.charityName },
    { key: 'familyCode', i18nLabel: `${keyPrefix}.colFamilyCode`, type: 'text', width: 14, value: r => r.familyCode },
    { key: 'age', i18nLabel: `${keyPrefix}.colAge`, type: 'number', value: r => r.age }
  ];
}

const orphanStatus: ReportColumnSet<OrphanStatusReportRow> = {
  reportKey: 'orphan-status',
  sheetKey: 'reports.finishedSponsorship.title',
  columns: orphanStatusColumns('reports.finishedSponsorship')
};

const mezaCards: ReportColumnSet<MezaCardsRow> = {
  reportKey: 'meza-cards',
  sheetKey: 'reports.mezaCards.title',
  columns: [
    { key: 'familyCode', i18nLabel: 'reports.mezaCards.colFamilyCode', type: 'text', width: 14, value: r => r.familyCode },
    { key: 'guardianName', i18nLabel: 'reports.mezaCards.colGuardianName', type: 'text', width: 26, value: r => r.guardianName },
    { key: 'guardianNationalId', i18nLabel: 'reports.mezaCards.colGuardianNationalId', type: 'text', width: 16, value: r => r.guardianNationalId },
    { key: 'phone', i18nLabel: 'reports.mezaCards.colPhone', type: 'text', value: r => r.phone },
    { key: 'orphanNames', i18nLabel: 'reports.mezaCards.colOrphanNames', type: 'text', width: 30, value: r => r.orphanNames },
    { key: 'orphanCodes', i18nLabel: 'reports.mezaCards.colOrphanCodes', type: 'text', width: 20, value: r => r.orphanCodes },
    { key: 'charityName', i18nLabel: 'reports.mezaCards.colCharity', type: 'text', value: r => r.charityName },
    { key: 'mezaCardNumber', i18nLabel: 'reports.mezaCards.colCardNumber', type: 'text', value: r => r.mezaCardNumber },
    { key: 'mezaCardExpiry', i18nLabel: 'reports.mezaCards.colCardExpiry', type: 'date', width: 14, value: r => r.mezaCardExpiry }
  ]
};

const beneficiaryFamilies: ReportColumnSet<BeneficiaryFamilyRow> = {
  reportKey: 'beneficiary-families',
  sheetKey: 'reports.beneficiaryFamilyDetails.title',
  columns: [
    { key: 'familyCode', i18nLabel: 'reports.beneficiaryFamilyDetails.colFamilyCode', type: 'text', width: 14, value: r => r.familyCode },
    { key: 'headOfFamilyName', i18nLabel: 'reports.beneficiaryFamilyDetails.colHeadOfFamily', type: 'text', width: 26, value: r => r.headOfFamilyName },
    { key: 'phone', i18nLabel: 'reports.beneficiaryFamilyDetails.colPhone', type: 'text', value: r => r.phone },
    { key: 'address', i18nLabel: 'reports.beneficiaryFamilyDetails.colAddress', type: 'text', width: 28, value: r => r.address },
    { key: 'charityName', i18nLabel: 'reports.beneficiaryFamilyDetails.colCharity', type: 'text', value: r => r.charityName },
    { key: 'campaignNames', i18nLabel: 'reports.beneficiaryFamilyDetails.colCampaigns', type: 'text', width: 30, value: r => r.campaignNames }
  ]
};

const familyProjects: ReportColumnSet<FamilyProjectRow> = {
  reportKey: 'family-projects',
  sheetKey: 'reports.familyProjects.title',
  columns: [
    { key: 'orphanNames', i18nLabel: 'reports.familyProjects.colOrphanNames', type: 'text', width: 30, value: r => r.orphanNames },
    { key: 'orphanNationalIds', i18nLabel: 'reports.familyProjects.colOrphanNationalIds', type: 'text', width: 20, value: r => r.orphanNationalIds },
    { key: 'guardianName', i18nLabel: 'reports.familyProjects.colGuardianName', type: 'text', width: 26, value: r => r.guardianName },
    { key: 'orphanCodes', i18nLabel: 'reports.familyProjects.colOrphanCodes', type: 'text', width: 20, value: r => r.orphanCodes },
    { key: 'familyCode', i18nLabel: 'reports.familyProjects.colFamilyCode', type: 'text', width: 14, value: r => r.familyCode },
    { key: 'phone', i18nLabel: 'reports.familyProjects.colPhone', type: 'text', value: r => r.phone },
    { key: 'charityName', i18nLabel: 'reports.familyProjects.colCharity', type: 'text', value: r => r.charityName },
    { key: 'projectStatus', i18nLabel: 'reports.familyProjects.colProjectStatus', type: 'text', value: r => r.projectStatus },
    { key: 'yearsOfExperience', i18nLabel: 'reports.familyProjects.colYearsOfExperience', type: 'number', value: r => r.yearsOfExperience },
    { key: 'totalBudget', i18nLabel: 'reports.familyProjects.colTotalBudget', type: 'number', value: r => r.totalBudget },
    { key: 'projectStartDate', i18nLabel: 'reports.familyProjects.colProjectStartDate', type: 'date', width: 14, value: r => r.projectStartDate },
    { key: 'projectAddress', i18nLabel: 'reports.familyProjects.colProjectAddress', type: 'text', width: 28, value: r => r.projectAddress },
    { key: 'hasExperience', i18nLabel: 'reports.familyProjects.colHasExperience', type: 'boolean', value: r => r.hasExperience },
    { key: 'projectDescription', i18nLabel: 'reports.familyProjects.colProjectDescription', type: 'text', width: 34, value: r => r.projectDescription }
  ]
};

const providerChanges: ReportColumnSet<ProviderChangeRow> = {
  reportKey: 'provider-changes',
  sheetKey: 'reports.providerChanges.title',
  columns: [
    { key: 'familyCode', i18nLabel: 'reports.providerChanges.colFamilyCode', type: 'text', width: 14, value: r => r.familyCode },
    { key: 'previousGuardianName', i18nLabel: 'reports.providerChanges.colPreviousGuardian', type: 'text', width: 24, value: r => r.previousGuardianName },
    { key: 'previousRelationship', i18nLabel: 'reports.providerChanges.colPreviousRelationship', type: 'text', value: r => r.previousRelationship },
    { key: 'changeReason', i18nLabel: 'reports.providerChanges.colChangeReason', type: 'text', width: 24, value: r => r.changeReason },
    { key: 'newGuardianName', i18nLabel: 'reports.providerChanges.colNewGuardian', type: 'text', width: 24, value: r => r.newGuardianName },
    { key: 'newRelationship', i18nLabel: 'reports.providerChanges.colNewRelationship', type: 'text', value: r => r.newRelationship },
    { key: 'changeDate', i18nLabel: 'reports.providerChanges.colChangeDate', type: 'date', width: 14, value: r => r.changeDate },
    { key: 'charityName', i18nLabel: 'reports.providerChanges.colCharity', type: 'text', value: r => r.charityName },
    { key: 'orphanCodes', i18nLabel: 'reports.providerChanges.colOrphanCodes', type: 'text', width: 20, value: r => r.orphanCodes }
  ]
};

const orphansMissingReportsSummary: ReportColumnSet<OrphansMissingReportsSummaryRow> = {
  reportKey: 'orphans-missing-reports-summary',
  sheetKey: 'reports.orphansMissingReports.title',
  columns: [
    { key: 'charityName', i18nLabel: 'reports.orphansMissingReports.colCharity', type: 'text', value: r => r.charityName },
    { key: 'missingCount', i18nLabel: 'reports.orphansMissingReports.colMissingCount', type: 'number', value: r => r.missingCount }
  ]
};

const orphansMissingReportsDetails: ReportColumnSet<OrphansMissingReportsDetailRow> = {
  reportKey: 'orphans-missing-reports-details',
  sheetKey: 'reports.orphansMissingReports.detailTitle',
  columns: [
    { key: 'orphanCode', i18nLabel: 'reports.orphansMissingReports.colOrphanCode', type: 'text', width: 14, value: r => r.orphanCode },
    { key: 'orphanName', i18nLabel: 'reports.orphansMissingReports.colOrphanName', type: 'text', width: 26, value: r => r.orphanName },
    { key: 'lastReportDate', i18nLabel: 'reports.orphansMissingReports.colLastReportDate', type: 'date', width: 14, value: r => r.lastReportDate }
  ]
};

const orphansMissingFiles: ReportColumnSet<OrphansMissingFilesRow> = {
  reportKey: 'orphans-missing-files',
  sheetKey: 'reports.orphansMissingFiles.title',
  columns: [
    { key: 'charityName', i18nLabel: 'reports.orphansMissingFiles.colCharity', type: 'text', value: r => r.charityName },
    { key: 'orphanCode', i18nLabel: 'reports.orphansMissingFiles.colOrphanCode', type: 'text', width: 14, value: r => r.orphanCode },
    { key: 'orphanName', i18nLabel: 'reports.orphansMissingFiles.colOrphanName', type: 'text', width: 26, value: r => r.orphanName },
    { key: 'address', i18nLabel: 'reports.orphansMissingFiles.colAddress', type: 'text', width: 28, value: r => r.address },
    { key: 'village', i18nLabel: 'reports.orphansMissingFiles.colVillage', type: 'text', value: r => r.village },
    { key: 'missingDocumentsName', i18nLabel: 'reports.orphansMissingFiles.colMissingDocuments', type: 'text', width: 28, value: r => r.missingDocumentsName }
  ]
};

const reportsAwaitingApproval: ReportColumnSet<ReportsAwaitingApprovalRow> = {
  reportKey: 'reports-awaiting-approval',
  sheetKey: 'reports.awaitingApproval.title',
  columns: [
    { key: 'charityName', i18nLabel: 'reports.awaitingApproval.colCharity', type: 'text', value: r => r.charityName },
    { key: 'orphanName', i18nLabel: 'reports.awaitingApproval.colOrphanName', type: 'text', width: 26, value: r => r.orphanName },
    { key: 'reportDate', i18nLabel: 'reports.awaitingApproval.colReportDate', type: 'date', width: 14, value: r => r.reportDate },
    { key: 'orphanCode', i18nLabel: 'reports.awaitingApproval.colOrphanCode', type: 'text', width: 14, value: r => r.orphanCode },
    { key: 'refuseReason', i18nLabel: 'reports.awaitingApproval.colRefuseReason', type: 'text', width: 24, value: r => r.refuseReason }
  ]
};

const refusedReports: ReportColumnSet<RefusedReportsRow> = {
  reportKey: 'refused-reports',
  sheetKey: 'reports.refusedReports.title',
  columns: [
    { key: 'charityName', i18nLabel: 'reports.refusedReports.colCharity', type: 'text', value: r => r.charityName },
    { key: 'orphanName', i18nLabel: 'reports.refusedReports.colOrphanName', type: 'text', width: 26, value: r => r.orphanName },
    { key: 'reportDate', i18nLabel: 'reports.refusedReports.colReportDate', type: 'date', width: 14, value: r => r.reportDate },
    { key: 'orphanCode', i18nLabel: 'reports.refusedReports.colOrphanCode', type: 'text', width: 14, value: r => r.orphanCode },
    { key: 'refuseReason', i18nLabel: 'reports.refusedReports.colRefuseReason', type: 'text', width: 24, value: r => r.refuseReason },
    { key: 'reviewedDate', i18nLabel: 'reports.refusedReports.colReviewedDate', type: 'date', width: 14, value: r => r.reviewedDate },
    { key: 'state', i18nLabel: 'reports.refusedReports.stateRefused', type: 'text', value: () => null, constantLabel: 'reports.refusedReports.stateRefused' }
  ]
};

const nonRenewedReports: ReportColumnSet<NonRenewedOrphanRow> = {
  reportKey: 'non-renewed-reports',
  sheetKey: 'reports.nonRenewed.title',
  columns: [
    { key: 'charityName', i18nLabel: 'reports.nonRenewed.colCharity', type: 'text', value: r => r.charityName },
    { key: 'code', i18nLabel: 'reports.nonRenewed.colOrphanCode', type: 'text', width: 14, value: r => r.code },
    { key: 'fullName', i18nLabel: 'reports.nonRenewed.colOrphanName', type: 'text', width: 26, value: r => r.fullName },
    { key: 'batchNo', i18nLabel: 'reports.nonRenewed.colBatch', type: 'text', value: r => r.batchNo }
  ]
};

const charityPaymentTracking: ReportColumnSet<CharityPaymentTrackingRow> = {
  reportKey: 'charity-payment-tracking',
  sheetKey: 'reports.charityTracking.title',
  columns: [
    { key: 'charityName', i18nLabel: 'reports.charityTracking.colCharity', type: 'text', value: r => r.charityName },
    { key: 'orphansInBatch', i18nLabel: 'reports.charityTracking.colOrphansInBatch', type: 'number', value: r => r.orphansInBatch },
    { key: 'reportsEntered', i18nLabel: 'reports.charityTracking.colReportsEntered', type: 'number', value: r => r.reportsEntered },
    {
      key: 'batchUploaded', i18nLabel: 'reports.charityTracking.colBatchUploaded', type: 'boolean',
      value: r => r.batchUploaded,
      booleanLabels: { true: 'reports.charityTracking.yes', false: 'reports.charityTracking.no' }
    },
    { key: 'uploadDate', i18nLabel: 'reports.charityTracking.colUploadDate', type: 'date', width: 14, value: r => r.uploadDate }
  ]
};

/** 18-22 — the base set; the dynamic one-column-per-batch tail is appended by the wrapper. */
const missedPayments: ReportColumnSet<MissedPaymentRow> = {
  reportKey: 'missed-payments',
  sheetKey: 'reports.missedPayments.title',
  columns: [
    { key: 'orphanCode', i18nLabel: 'reports.missedPayments.colOrphanCode', type: 'text', width: 14, value: r => r.orphanCode },
    { key: 'charityName', i18nLabel: 'reports.missedPayments.colCharity', type: 'text', value: r => r.charityName },
    { key: 'orphanName', i18nLabel: 'reports.missedPayments.colOrphanName', type: 'text', width: 26, value: r => r.orphanName },
    { key: 'reason', i18nLabel: 'reports.missedPayments.colReason', type: 'text', width: 24, value: r => r.reason }
  ]
};

/** 18-24/25 — images are not grid columns; the dedicated manifest builder stays, registered. */
const orphanFiles: ReportColumnSet = {
  reportKey: 'orphan-files',
  sheetKey: 'reports.orphanFiles.title',
  columns: [],
  specialCase: 'image-manifest'
};

// ==================== 18-33 / 18-35 / 18-36 / 18-38 / 18-39 — the sets this story wires ====================

/** 18-33 — the bank reconciliation statement; amount is a NUMBER, currency its own column. */
const chequeStatement: ReportColumnSet<ChequeStatementItemShim> = {
  reportKey: 'cheque-statement',
  sheetKey: 'reports.chequeStatement.title',
  columns: [
    { key: 'checkNumber', i18nLabel: 'reports.chequeStatement.colNumber', type: 'text', width: 16, value: r => r.checkNumber },
    { key: 'checkDate', i18nLabel: 'reports.chequeStatement.colDate', type: 'date', width: 14, value: r => r.checkDate },
    { key: 'beneficiaryName', i18nLabel: 'reports.chequeStatement.colBeneficiary', type: 'text', width: 26, value: r => r.beneficiaryName },
    { key: 'amount', i18nLabel: 'reports.chequeStatement.colAmount', type: 'number', value: r => r.amount },
    { key: 'currency', i18nLabel: 'reports.chequeStatement.colCurrency', type: 'text', value: r => r.currency },
    {
      // Review P27 2026-08-26: a NULL chequeType must not render as أفراد — the old
      // comparison mapped null to false (Individuals), inventing a fact the row never
      // carried; the engine blanks null cells, so unknown stays blank.
      key: 'chequeType', i18nLabel: 'reports.chequeStatement.colType', type: 'boolean',
      value: r => r.chequeType == null ? null : r.chequeType === 'Orphans',
      booleanLabels: { true: 'reports.chequeStatement.typeOrphans', false: 'reports.chequeStatement.typeIndividuals' }
    },
    { key: 'bankName', i18nLabel: 'reports.chequeStatement.colBank', type: 'text', value: r => r.bankName }
  ]
};

const newBeneficiariesOrphans: ReportColumnSet<NewBeneficiaryRow> = {
  reportKey: 'new-beneficiaries-orphans',
  // Review P27 2026-08-26: a variant IS its own report shape — its sheet title says which
  // variant it is (all four new-beneficiaries sets previously shared one generic title).
  sheetKey: 'reports.newBeneficiaries.variantOrphans',
  columns: [
    { key: 'orphanCode', i18nLabel: 'reports.newBeneficiaries.colOrphanCode', type: 'text', width: 14, value: r => r.orphanCode },
    { key: 'orphanName', i18nLabel: 'reports.newBeneficiaries.colOrphanName', type: 'text', width: 26, value: r => r.orphanName },
    { key: 'birthDate', i18nLabel: 'reports.newBeneficiaries.colBirthDate', type: 'date', width: 14, value: r => r.birthDate },
    { key: 'charityName', i18nLabel: 'reports.newBeneficiaries.colCharity', type: 'text', value: r => r.charityName },
    { key: 'registrationDate', i18nLabel: 'reports.newBeneficiaries.colRegistrationDate', type: 'date', width: 14, value: r => r.registrationDate }
  ]
};

const newBeneficiariesOrphansV2: ReportColumnSet<NewBeneficiaryRow> = {
  reportKey: 'new-beneficiaries-orphansv2',
  sheetKey: 'reports.newBeneficiaries.variantOrphansV2',
  columns: [
    { key: 'orphanCode', i18nLabel: 'reports.newBeneficiaries.colOrphanCode', type: 'text', width: 14, value: r => r.orphanCode },
    { key: 'orphanName', i18nLabel: 'reports.newBeneficiaries.colOrphanName', type: 'text', width: 26, value: r => r.orphanName },
    { key: 'birthDate', i18nLabel: 'reports.newBeneficiaries.colBirthDate', type: 'date', width: 14, value: r => r.birthDate },
    { key: 'familyCode', i18nLabel: 'reports.newBeneficiaries.colFamilyCode', type: 'text', width: 14, value: r => r.familyCode },
    { key: 'guardianName', i18nLabel: 'reports.newBeneficiaries.colGuardian', type: 'text', width: 24, value: r => r.guardianName },
    { key: 'charityName', i18nLabel: 'reports.newBeneficiaries.colCharity', type: 'text', value: r => r.charityName },
    { key: 'registrationDate', i18nLabel: 'reports.newBeneficiaries.colRegistrationDate', type: 'date', width: 14, value: r => r.registrationDate }
  ]
};

const newBeneficiariesWidows: ReportColumnSet<NewBeneficiaryRow> = {
  reportKey: 'new-beneficiaries-widows',
  sheetKey: 'reports.newBeneficiaries.variantWidows',
  columns: [
    { key: 'widowName', i18nLabel: 'reports.newBeneficiaries.colWidowName', type: 'text', width: 26, value: r => r.widowName },
    { key: 'nationalId', i18nLabel: 'reports.newBeneficiaries.colNationalId', type: 'text', width: 16, value: r => r.nationalId },
    { key: 'husbandDeathDate', i18nLabel: 'reports.newBeneficiaries.colHusbandDeathDate', type: 'date', width: 14, value: r => r.husbandDeathDate },
    { key: 'charityName', i18nLabel: 'reports.newBeneficiaries.colCharity', type: 'text', value: r => r.charityName },
    { key: 'registrationDate', i18nLabel: 'reports.newBeneficiaries.colRegistrationDate', type: 'date', width: 14, value: r => r.registrationDate }
  ]
};

const newBeneficiariesWidowsByFamily: ReportColumnSet<NewBeneficiaryRow> = {
  reportKey: 'new-beneficiaries-widowsbyfamily',
  sheetKey: 'reports.newBeneficiaries.variantWidowsByFamily',
  columns: [
    { key: 'familyCode', i18nLabel: 'reports.newBeneficiaries.colFamilyCode', type: 'text', width: 14, value: r => r.familyCode },
    { key: 'widowName', i18nLabel: 'reports.newBeneficiaries.colWidowName', type: 'text', width: 26, value: r => r.widowName },
    { key: 'childrenCount', i18nLabel: 'reports.newBeneficiaries.colChildrenCount', type: 'number', value: r => r.childrenCount },
    { key: 'charityName', i18nLabel: 'reports.newBeneficiaries.colCharity', type: 'text', value: r => r.charityName }
  ]
};

/** 18-6 (§23.S.9) — the 21-column widow grid (serial is engine-owned). */
const widowsSponsorship: ReportColumnSet<WidowSponsorshipRow> = {
  reportKey: 'widows-sponsorship',
  sheetKey: 'reports.widowsSponsorship.title',
  columns: [
    { key: 'widowName', i18nLabel: 'reports.widowsSponsorship.colWidowName', type: 'text', width: 26, value: r => r.widowName },
    { key: 'governorateName', i18nLabel: 'reports.widowsSponsorship.colGovernorate', type: 'text', value: r => r.governorateName },
    { key: 'centerName', i18nLabel: 'reports.widowsSponsorship.colCenter', type: 'text', value: r => r.centerName },
    { key: 'cityVillage', i18nLabel: 'reports.widowsSponsorship.colVillage', type: 'text', value: r => r.cityVillage },
    { key: 'detailedAddress', i18nLabel: 'reports.widowsSponsorship.colAddress', type: 'text', width: 28, value: r => r.detailedAddress },
    { key: 'mobileNumber2', i18nLabel: 'reports.widowsSponsorship.colMobile2', type: 'text', value: r => r.mobileNumber2 },
    { key: 'mobileNumber', i18nLabel: 'reports.widowsSponsorship.colMobile1', type: 'text', value: r => r.mobileNumber },
    { key: 'houseOwnershipName', i18nLabel: 'reports.widowsSponsorship.colHouseOwnership', type: 'text', value: r => r.houseOwnershipName },
    { key: 'rentAmount', i18nLabel: 'reports.widowsSponsorship.colRentValue', type: 'number', value: r => r.rentAmount },
    { key: 'housingTypeName', i18nLabel: 'reports.widowsSponsorship.colHousingType', type: 'text', value: r => r.housingTypeName },
    { key: 'houseStatusName', i18nLabel: 'reports.widowsSponsorship.colHouseStatus', type: 'text', value: r => r.houseStatusName },
    { key: 'monthlyIncome', i18nLabel: 'reports.widowsSponsorship.colIncomeValue', type: 'number', value: r => r.monthlyIncome },
    { key: 'husbandDeathDate', i18nLabel: 'reports.widowsSponsorship.colHusbandDeathDate', type: 'date', width: 14, value: r => r.husbandDeathDate },
    { key: 'developmentProject', i18nLabel: 'reports.widowsSponsorship.colDevelopmentProject', type: 'text', width: 24, value: r => r.developmentProject },
    { key: 'educationLevelName', i18nLabel: 'reports.widowsSponsorship.colEducation', type: 'text', value: r => r.educationLevelName },
    { key: 'profession', i18nLabel: 'reports.widowsSponsorship.colProfession', type: 'text', value: r => r.profession },
    { key: 'healthStatusName', i18nLabel: 'reports.widowsSponsorship.colHealthStatus', type: 'text', value: r => r.healthStatusName },
    { key: 'nationalId', i18nLabel: 'reports.widowsSponsorship.colNationalId', type: 'text', width: 16, value: r => r.nationalId },
    { key: 'notes', i18nLabel: 'reports.widowsSponsorship.colNotes', type: 'text', width: 32, value: r => r.notes },
    { key: 'charityName', i18nLabel: 'reports.widowsSponsorship.colCharity', type: 'text', value: r => r.charityName },
    { key: 'lastUpdatedDate', i18nLabel: 'reports.widowsSponsorship.colLastUpdated', type: 'date', width: 14, value: r => r.lastUpdatedDate }
  ]
};

const followUpOrphans: ReportColumnSet<FollowUpSheetRow> = {
  reportKey: 'follow-up-orphans',
  // Review P27 2026-08-26: the three follow-up variants previously shared one generic sheet
  // title — a variant's sheet says which variant it is.
  sheetKey: 'reports.followUpSheets.variantFollowUp',
  columns: [
    { key: 'orphanCode', i18nLabel: 'reports.followUpSheets.colOrphanCode', type: 'text', width: 14, value: r => r.orphanCode },
    { key: 'orphanName', i18nLabel: 'reports.followUpSheets.colOrphanName', type: 'text', width: 26, value: r => r.orphanName },
    { key: 'familyCode', i18nLabel: 'reports.followUpSheets.colFamilyCode', type: 'text', width: 14, value: r => r.familyCode },
    { key: 'charityName', i18nLabel: 'reports.followUpSheets.colCharity', type: 'text', value: r => r.charityName },
    { key: 'sponsorshipStatus', i18nLabel: 'reports.followUpSheets.colSponsorshipStatus', type: 'text', value: r => r.sponsorshipStatus },
    { key: 'lastReportDate', i18nLabel: 'reports.followUpSheets.colLastReportDate', type: 'date', width: 14, value: r => r.lastReportDate },
    { key: 'monthlyAmount', i18nLabel: 'reports.followUpSheets.colMonthlyAmount', type: 'number', value: r => r.monthlyAmount }
  ]
};

const followUpFamilies: ReportColumnSet<FollowUpSheetRow> = {
  reportKey: 'follow-up-families',
  sheetKey: 'reports.followUpSheets.variantFollowUpFamily',
  columns: [
    { key: 'familyCode', i18nLabel: 'reports.followUpSheets.colFamilyCode', type: 'text', width: 14, value: r => r.familyCode },
    { key: 'headOfFamily', i18nLabel: 'reports.followUpSheets.colHeadOfFamily', type: 'text', width: 26, value: r => r.headOfFamily },
    { key: 'charityName', i18nLabel: 'reports.followUpSheets.colCharity', type: 'text', value: r => r.charityName },
    { key: 'orphansCount', i18nLabel: 'reports.followUpSheets.colOrphansCount', type: 'number', value: r => r.orphansCount },
    { key: 'familyStatus', i18nLabel: 'reports.followUpSheets.colFamilyStatus', type: 'text', value: r => r.familyStatus },
    { key: 'registrationDate', i18nLabel: 'reports.followUpSheets.colRegistrationDate', type: 'date', width: 14, value: r => r.registrationDate },
    { key: 'lastUpdate', i18nLabel: 'reports.followUpSheets.colLastUpdate', type: 'date', width: 14, value: r => r.lastUpdate }
  ]
};

const followUpTasleem: ReportColumnSet<FollowUpSheetRow> = {
  reportKey: 'follow-up-tasleem',
  sheetKey: 'reports.followUpSheets.variantTasleem',
  columns: [
    { key: 'orphanCode', i18nLabel: 'reports.followUpSheets.colOrphanCode', type: 'text', width: 14, value: r => r.orphanCode },
    { key: 'orphanName', i18nLabel: 'reports.followUpSheets.colOrphanName', type: 'text', width: 26, value: r => r.orphanName },
    { key: 'guardianName', i18nLabel: 'reports.followUpSheets.colGuardian', type: 'text', width: 24, value: r => r.guardianName },
    { key: 'familyCode', i18nLabel: 'reports.followUpSheets.colFamilyCode', type: 'text', width: 14, value: r => r.familyCode },
    { key: 'charityName', i18nLabel: 'reports.followUpSheets.colCharity', type: 'text', value: r => r.charityName },
    { key: 'monthlyAmount', i18nLabel: 'reports.followUpSheets.colMonthlyAmount', type: 'number', value: r => r.monthlyAmount },
    { key: 'sponsorshipStatus', i18nLabel: 'reports.followUpSheets.colSponsorshipStatus', type: 'text', value: r => r.sponsorshipStatus }
  ]
};

const missingOutgoingAttachments: ReportColumnSet<MissingOutgoingAttachmentsRow> = {
  reportKey: 'missing-outgoing-attachments',
  sheetKey: 'reports.missingOutgoingAttachments.title',
  columns: [
    // Review P27 2026-08-26: the serial column is ENGINE-OWNED (every sheet opens with '#')
    // — the set's own serial column duplicated it, printing two number columns per row.
    { key: 'outGoingNumber', i18nLabel: 'reports.missingOutgoingAttachments.colOutGoingNumber', type: 'text', width: 18, value: r => r.outGoingNumber },
    { key: 'subject', i18nLabel: 'reports.missingOutgoingAttachments.colSubject', type: 'text', width: 30, value: r => r.subject },
    { key: 'date', i18nLabel: 'reports.missingOutgoingAttachments.colDate', type: 'date', width: 14, value: r => r.date },
    { key: 'year', i18nLabel: 'reports.missingOutgoingAttachments.colYear', type: 'number', value: r => r.year },
    { key: 'categoryName', i18nLabel: 'reports.missingOutgoingAttachments.colCategory', type: 'text', value: r => r.categoryName },
    { key: 'charityName', i18nLabel: 'reports.missingOutgoingAttachments.colCharity', type: 'text', value: r => r.charityName },
    { key: 'attachmentCount', i18nLabel: 'reports.missingOutgoingAttachments.colAttachmentCount', type: 'number', value: r => r.attachmentCount }
  ]
};

/**
 * 18-39 — the grouped list flattened: family cells REPEAT on every orphan row (Excel has no
 * rowspan, and the autofilter demands consistent cells — recorded deviation from the print
 * sheet's group-first banding).
 */
const familyOrphansByDate: ReportColumnSet<FamilyOrphansFlatRow> = {
  reportKey: 'family-orphans-by-date',
  sheetKey: 'reports.familyOrphansByDate.title',
  columns: [
    { key: 'familyCode', i18nLabel: 'reports.familyOrphansByDate.colFamilyCode', type: 'text', width: 14, value: r => r.familyCode },
    { key: 'headOfFamily', i18nLabel: 'reports.familyOrphansByDate.colHeadOfFamily', type: 'text', width: 26, value: r => r.headOfFamily },
    { key: 'regionCenter', i18nLabel: 'reports.familyOrphansByDate.colRegionCenter', type: 'text', width: 22, value: r => r.regionCenter },
    { key: 'orphansCount', i18nLabel: 'reports.familyOrphansByDate.colOrphansCount', type: 'number', value: r => r.orphansCount },
    { key: 'orphanCode', i18nLabel: 'reports.familyOrphansByDate.colOrphanCode', type: 'text', width: 14, value: r => r.orphanCode },
    { key: 'orphanName', i18nLabel: 'reports.familyOrphansByDate.colOrphanName', type: 'text', width: 26, value: r => r.orphanName },
    { key: 'dateOfBirth', i18nLabel: 'reports.familyOrphansByDate.colBirthDate', type: 'date', width: 14, value: r => r.dateOfBirth },
    { key: 'age', i18nLabel: 'reports.familyOrphansByDate.colAge', type: 'number', value: r => r.age }
  ]
};

/** The 18-13 fold (Task 4) — the follow-up activity grid that previously built its own workbook. */
const familyFollowUp: ReportColumnSet<FamilyFollowUpRowShim> = {
  reportKey: 'family-follow-up',
  sheetKey: 'families.followUp.title',
  columns: [
    { key: 'code', i18nLabel: 'families.followUp.colCode', type: 'text', width: 14, value: r => r.code },
    { key: 'headOfFamily', i18nLabel: 'families.followUp.colHead', type: 'text', width: 26, value: r => r.headOfFamily },
    { key: 'charityName', i18nLabel: 'families.followUp.colCharity', type: 'text', value: r => r.charityName },
    {
      // Review P27 2026-08-26: same null rule as chequeType — an absent changeKind stays
      // blank rather than asserting إنشاء.
      key: 'changeKind', i18nLabel: 'families.followUp.colKind', type: 'boolean',
      value: r => r.changeKind == null ? null : r.changeKind === 'Created',
      booleanLabels: { true: 'families.followUp.kindCreated', false: 'families.followUp.kindUpdated' }
    },
    { key: 'changedBy', i18nLabel: 'families.followUp.colChangedBy', type: 'text', value: r => r.changedBy },
    { key: 'changedOn', i18nLabel: 'families.followUp.colChangedOn', type: 'date', format: 'datetime', width: 18, value: r => r.changedOn },
    { key: 'orphansTouched', i18nLabel: 'families.followUp.colOrphansTouched', type: 'number', value: r => r.orphansTouched }
  ]
};

/**
 * The registry — report key → column set. Variants are their own keys (a variant IS its own
 * report shape). Everything the engine exports resolves through this map.
 *
 * Review P27 2026-08-26: built from an ARRAY with a duplicate scan that THROWS. The previous
 * computed-key Record collapsed duplicate reportKeys silently at object construction — the
 * last set won and the shadowed variant disappeared with no trace, so a future collision
 * (two variants mistyped to one key) could never be caught here.
 */
const REPORT_SETS: ReportColumnSet<any>[] = [
  orphanData,
  excludedOrphans,
  orphanStatus,
  mezaCards,
  beneficiaryFamilies,
  familyProjects,
  providerChanges,
  orphansMissingReportsSummary,
  orphansMissingReportsDetails,
  orphansMissingFiles,
  reportsAwaitingApproval,
  refusedReports,
  nonRenewedReports,
  charityPaymentTracking,
  missedPayments,
  orphanFiles,
  chequeStatement,
  newBeneficiariesOrphans,
  newBeneficiariesOrphansV2,
  newBeneficiariesWidows,
  newBeneficiariesWidowsByFamily,
  widowsSponsorship,
  followUpOrphans,
  followUpFamilies,
  followUpTasleem,
  missingOutgoingAttachments,
  familyOrphansByDate,
  familyFollowUp
];

export const REPORT_EXPORT_REGISTRY: Record<string, ReportColumnSet<any>> = (() => {
  const registry: Record<string, ReportColumnSet<any>> = {};
  for (const set of REPORT_SETS) {
    if (registry[set.reportKey] !== undefined) {
      throw new Error(`Duplicate report key in the export registry: '${set.reportKey}'`);
    }
    registry[set.reportKey] = set;
  }
  return registry;
})();
