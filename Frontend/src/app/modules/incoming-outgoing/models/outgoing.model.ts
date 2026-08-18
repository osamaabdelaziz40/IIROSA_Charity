export interface OutgoingDto {
  id: string;
  serial?: number;
  subject: string;
  date?: Date;
  outGoingNumber?: string;
  outGoingId: string;
  body?: string;
  year?: number;
  fkDepartmentId?: string;
  departmentName?: string;
  uploadedFile?: string;
  fileName?: string;
  outgoingCategoryId?: string;
  outgoingCategoryName?: string;
  incomingId?: string;
  incomingNumber?: string;
  childOutgoings?: ChildOutGoingDto[];
  createdOn: Date;
  modifiedOn?: Date;
  createdBy?: string;
  modifiedBy?: string;
}

export interface CreateOutgoingDto {
  subject: string;
  date?: Date;
  outGoingNumber?: string;
  outGoingId: string;
  body?: string;
  year?: number;
  fkDepartmentId?: string;
  uploadedFile?: string;
  outgoingCategoryId?: string;
  incomingId?: string;
}

export interface UpdateOutgoingDto {
  subject?: string;
  date?: Date;
  outGoingNumber?: string;
  outGoingId?: string;
  body?: string;
  year?: number;
  fkDepartmentId?: string;
  uploadedFile?: string;
  outgoingCategoryId?: string;
  incomingId?: string;
}

export interface OutgoingSearchRequest {
  searchTerm?: string;
  departmentId?: string;
  categoryId?: string;
  year?: number;
  startDate?: Date;
  endDate?: Date;
  createdBy?: string;
  hasReply?: boolean;
  pageNumber?: number;
  pageSize?: number;
}

export interface OutgoingPagedResult {
  items: OutgoingDto[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
}

export interface ChildOutGoingDto {
  id: string;
  parentId: string;
  subject: string;
  date?: Date;
  outGoingId: string;
}
