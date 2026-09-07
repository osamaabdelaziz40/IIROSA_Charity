// Epic 16 wire models (UC-COR-10…19) — camelCase fields matching the
// api/IncomingOutgoing controller. Dates are ISO strings over the wire.

export interface OutgoingDto {
  id: string;
  serial?: number;
  subject: string;
  date?: string;
  year?: number;
  departmentId?: number;
  departmentName?: string;
  uploadedFileId?: string;
  uploadedFileName?: string;
  outgoingCategoryId?: number;
  categoryName?: string;
  incomingId?: string;
  incomingLetterNumber?: string;
  incomingLetterSubject?: string;
  charityId?: string;
  charityName?: string;
  orphans?: OutgoingOrphanRow[];
  createdOn: string;
  updatedOn: string;
}

/** An attached orphan report on the detail screen (§21.S.6 — أسم اليتيم · كود اليتيم) */
export interface OutgoingOrphanRow {
  orphanId: string;
  code: string;
  fullName: string;
}

export interface CreateOutgoingDto {
  departmentId?: number;
  date?: string;
  subject: string;
  outgoingCategoryId?: number;
  incomingId?: string;
  uploadedFileId?: string;
}

export interface UpdateOutgoingDto extends CreateOutgoingDto {
  id: string;
}

export interface OutgoingListDto {
  id: string;
  serial?: number;
  subject: string;
  date?: string;
  year?: number;
  departmentName?: string;
  outgoingCategoryId?: number;
  categoryName?: string;
  hasReply: boolean;
  incomingLetterNumber?: string;
  uploadedFileId?: string;
  uploadedFileName?: string;
  createdOn: string;
}

export interface OutgoingFilterDto {
  pageNumber?: number;
  pageSize?: number;
  searchTerm?: string;
  serial?: number;
  departmentId?: number;
  categoryId?: number;
  year?: number;
  startDate?: string;
  endDate?: string;
  hasReply?: boolean;
  charityId?: string;
  sortBy?: string;
  sortOrder?: string;
}

export interface OutgoingPagedResult {
  items: OutgoingListDto[];
  totalCount: number;
  page: number;
}

export interface OutgoingCategoryOptionDto {
  id: number;
  nameAr: string;
  nameEn: string;
}

// ========== Orphan report attachment (UC-COR-18 / §21.S.6) ==========

export interface OrphanOptionDto {
  orphanId: string;
  code: string;
  fullName: string;
  guarantorName?: string;
  kinship?: string;
}

export interface OutgoingOrphansDto {
  attached: OrphanOptionDto[];
  unattached: OrphanOptionDto[];
}

// ========== Orphans-by-outgoing-letter report (UC-COR-19 / §21.S.7) ==========

export interface OutgoingOrphanReportFilterDto {
  pageNumber?: number;
  pageSize?: number;
  serial?: number;
  year?: number;
  charityId?: string;
  dateFrom?: string;
  dateTo?: string;
  childCode?: string;
}

export interface OutgoingOrphanReportRowDto {
  outgoingId: string;
  serial?: number;
  year?: number;
  letterDate?: string;
  charityName?: string;
  orphanCount: number;
  orphanAttached: boolean;
}

export interface OutgoingOrphanReportResult {
  items: OutgoingOrphanReportRowDto[];
  totalCount: number;
  page: number;
}
