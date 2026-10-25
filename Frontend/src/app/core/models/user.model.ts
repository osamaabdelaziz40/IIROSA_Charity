export interface User {
  id: string;
  userName: string;
  email: string;
  fullName: string;
  phoneNumber?: string;
  isActive: boolean;
  createdOn: Date;
  updatedOn?: Date;
  roles?: string[];
  token?: string;
  charityId?: string;
  charityName?: string;
  countryId?: number;
}

export interface CreateUserRequest {
  email: string;
  fullName: string;
  phoneNumber?: string;
  roles: string[];
  password?: string;
  charityId?: string;
  countryId?: number;
}

export interface UpdateUserRequest {
  email?: string;
  fullName?: string;
  phoneNumber?: string;
  isActive?: boolean;
  roles?: string[];
  charityId?: string;
  countryId?: number;
}

export interface UserListResponse {
  items: User[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
  hasPrevious: boolean;
  hasNext: boolean;
}

export interface UserSearchRequest {
  search?: string;
  role?: string;
  isActive?: boolean;
  page?: number;
  pageSize?: number;
}
