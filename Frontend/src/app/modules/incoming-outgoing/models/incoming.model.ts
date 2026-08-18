export interface IncomingDto {
  id: string;
  serial?: number;
  serialTxt?: string;
  subject: string;
  date?: Date;
  incomingNumber?: string;
  incomingId: string;
  body?: string;
  letterNumber: string;
  letterDate: Date;
  year?: number;
  status?: string;
  letterDescription?: string;
  fkDepartmentId?: string;
  departmentName?: string;
  fkUserId?: string;
  userName?: string;
  outgoingId?: string;
  outgoingNumber?: string;
  uploadedFile?: string;
  fileName?: string;
  createdOn: Date;
  modifiedOn?: Date;
  createdBy?: string;
  modifiedBy?: string;
}

export interface CreateIncomingDto {
  subject: string;
  date?: Date;
  incomingNumber?: string;
  incomingId: string;
  body?: string;
  letterNumber: string;
  letterDate: Date;
  year?: number;
  status?: string;
  letterDescription?: string;
  fkDepartmentId?: string;
  outgoingId?: string;
  uploadedFile?: string;
}

export interface UpdateIncomingDto {
  subject?: string;
  date?: Date;
  incomingNumber?: string;
  incomingId?: string;
  body?: string;
  letterNumber?: string;
  letterDate?: Date;
  year?: number;
  status?: string;
  letterDescription?: string;
  fkDepartmentId?: string;
  outgoingId?: string;
  uploadedFile?: string;
}

export interface IncomingSearchRequest {
  searchTerm?: string;
  departmentId?: string;
  status?: string;
  year?: number;
  startDate?: Date;
  endDate?: Date;
  createdBy?: string;
  pageNumber?: number;
  pageSize?: number;
}

export interface IncomingPagedResult {
  items: IncomingDto[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
}
