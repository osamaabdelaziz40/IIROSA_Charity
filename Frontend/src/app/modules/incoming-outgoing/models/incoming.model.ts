// Epic 16 wire models (UC-COR-01…09) — camelCase fields matching the
// api/IncomingOutgoing controller. Dates are ISO strings over the wire.

export interface IncomingDto {
  id: string;
  serial?: number;
  serialTxt?: string;
  subject: string;
  date?: string;
  letterNumber?: string;
  letterDate?: string;
  year?: number;
  status?: string;
  letterDescription?: string;
  departmentId?: number;
  departmentName?: string;
  assignedUserId?: string;
  assignedUserName?: string;
  uploadedFileId?: string;
  uploadedFileName?: string;
  charityId?: string;
  charityName?: string;
  createdOn: string;
  updatedOn: string;
}

export interface CreateIncomingDto {
  date?: string;
  letterNumber?: string;
  letterDate?: string;
  departmentId?: number;
  subject: string;
  status?: string;
  assignedUserId?: string;
  letterDescription?: string;
  uploadedFileId?: string;
}

export interface UpdateIncomingDto extends CreateIncomingDto {
  id: string;
}

export interface IncomingListDto {
  id: string;
  serial?: number;
  serialTxt?: string;
  subject: string;
  letterNumber?: string;
  letterDate?: string;
  date?: string;
  departmentId?: number;
  departmentName?: string;
  assignedUserName?: string;
  status?: string;
  year?: number;
  uploadedFileId?: string;
  uploadedFileName?: string;
  createdOn: string;
}

export interface IncomingFilterDto {
  pageNumber?: number;
  pageSize?: number;
  searchTerm?: string;
  serial?: number;
  letterNumber?: string;
  departmentId?: number;
  status?: string;
  year?: number;
  startDate?: string;
  endDate?: string;
  assignedUserId?: string;
  charityId?: string;
  sortBy?: string;
  sortOrder?: string;
}

export interface IncomingPagedResult {
  items: IncomingListDto[];
  totalCount: number;
  page: number;
}

// Register statistics band above the §21.S.1 grid — caller-scoped server-side, so the
// counts always match what the register under it can show.
export interface IncomingStatistics {
  total: number;
  thisYear: number;
  addedThisMonth: number;
}

// The spec's tri-state (معلق / تم الرد / تم عمل اللازم) — id is the Arabic stored value.
export interface CorrespondenceStatusOption {
  id: string;
  nameAr: string;
  nameEn: string;
  color: string;
}

export interface NextSerialDto {
  serial: number;
  serialTxt: string;
}

// ========== Employee attachment (UC-COR-09 / §21.S.3) ==========

export interface EmployeeOptionDto {
  userId: string;
  fullName: string;
  email?: string;
}

export interface IncomingEmployeesDto {
  attached: EmployeeOptionDto[];
  available: EmployeeOptionDto[];
}
