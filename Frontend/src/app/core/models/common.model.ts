export interface ApiResponse<T = any> {
  success: boolean;
  message?: string;
  value?: T;
  modelStateErrors?: Array<{
    name: string;
    value: string;
    errorCode?: string;
  }>;
}

export interface PagedResponse<T = any> {
  items: T[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
  hasPrevious: boolean;
  hasNext: boolean;
}

export interface Lookup {
  id: string | number;
  name: string;
  nameAr?: string;
  nameEn?: string;
}
