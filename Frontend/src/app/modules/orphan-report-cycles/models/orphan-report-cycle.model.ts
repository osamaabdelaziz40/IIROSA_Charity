/**
 * Orphan Report Cycles Module Models (UC-ORC-01…09, docs/Modules/26-UC-ORC).
 *
 * Cycle status is derived from the dates server-side (BR-ORC-02) and travels as the
 * raw enum number — the API's Newtonsoft formatters have no string-enum converter:
 * 0 = NotStarted, 1 = InProgress, 2 = Completed.
 */

/** Mirrors Domain.Enums.OrphanReportCycleStatus — numeric on the wire. */
export enum OrphanReportCycleStatus {
  NotStarted = 0,
  InProgress = 1,
  Completed = 2
}

export interface OrphanReportCycleDto {
  id: string;
  titleAr: string;
  titleEn?: string;
  descriptionAr?: string;
  descriptionEn?: string;
  startDate: string;
  endDate: string;
  status: OrphanReportCycleStatus;
  statusChangedOn?: string;
  reportsCount: number;
  codedOrphansCount: number;
  /** Share of the coded-orphan population with a report in the cycle, 0–100 (server-computed). */
  progressPercent: number;
  createdOn: string;
  createdBy?: string;
  updatedOn?: string;
  updatedBy?: string;
}

export interface CreateOrphanReportCycleDto {
  titleAr: string;
  titleEn?: string;
  descriptionAr?: string;
  descriptionEn?: string;
  startDate: string;
  endDate: string;
}

export interface UpdateOrphanReportCycleDto extends CreateOrphanReportCycleDto {
  id: string;
}

export interface OrphanReportCycleFilterDto {
  search?: string;
  status?: OrphanReportCycleStatus;
  page?: number;
  pageSize?: number;
}

export interface OrphanReportCyclePagedResult {
  items: OrphanReportCycleDto[];
  totalCount: number;
  page: number;
  pageSize: number;
}

/** Minimal cycle read behind the report-entry button gating and the coverage dropdown. */
export interface OrphanReportCycleSelectorDto {
  id: string;
  titleAr: string;
  titleEn?: string;
  startDate: string;
  endDate: string;
  status: OrphanReportCycleStatus;
}

/** One In Progress cycle with this orphan's submission state inside it (UC-ORC-06 chooser). */
export interface OrphanCycleReportStatusDto {
  cycleId: string;
  titleAr: string;
  titleEn?: string;
  startDate: string;
  endDate: string;
  submitted: boolean;
  reportId?: string;
  reportDate?: string;
  /** 'Accepted' | 'Refused' | 'Pending' when submitted; null otherwise. */
  reviewState?: string;
}

// ==================== COVERAGE (UC-ORC-09 — POST /api/Reports/cycle-reports) ====================

export interface CycleReportCoverageFilterDto {
  cycleId: string;
  /** HQ-only narrowing; ignored for charity callers (scope pinned server-side). */
  charityId?: string;
  search?: string;
  /** undefined = both halves interleaved; 'Submitted' | 'NotSubmitted' = one tab. */
  category?: string;
  page?: number;
  pageSize?: number;
}

export interface CycleReportCoverageRowDto {
  orphanId: string;
  orphanCode: string;
  orphanName: string;
  charityName?: string;
  hasReport: boolean;
  reportDate?: string;
  reviewState?: string;
}

export interface CycleReportCoverageResultDto {
  cycleId: string;
  cycleTitle: string;
  items: CycleReportCoverageRowDto[];
  totalCount: number;
  submittedCount: number;
  notSubmittedCount: number;
  page: number;
  pageSize: number;
}
