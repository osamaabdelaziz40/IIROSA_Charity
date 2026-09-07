/**
 * Mirrors the backend Employee DTOs (camelCase JSON). Field names must stay aligned with
 * `IIROSA.Application/DTOs/EmployeeManagement/Employees.cs` — this shape replaces the old
 * firstName/lastName/employeeCode/roleName wire contract that bound nothing on the server.
 */
export interface Employee {
  id: string;
  code: string;
  fullName: string;
  email?: string | null;
  phoneNumber?: string | null;
  position?: string | null;
  departmentId?: number | null;
  departmentName?: string | null;
  nationalId?: string | null;
  dateOfBirth?: string | null;
  gender?: string | null;
  address?: string | null;
  hireDate?: string | null;
  salary?: number | null;
  notes?: string | null;
  isActive: boolean;
  /** Server-computed: does this employee have a linked login account? */
  hasAccount?: boolean;
  roles: string[];
  createdOn?: string;
  updatedOn?: string | null;
}

export interface CreateEmployeeRequest {
  /** The login name — in this stack the identity UserName IS the email. */
  email: string;
  /** Creates the linked login account; required on create (UC-EMP-03). */
  password: string;
  fullName: string;
  code?: string;
  position: string;
  departmentId?: number | null;
  phoneNumber?: string;
  dateOfBirth?: string;
  gender?: string;
  address?: string;
  hireDate: string;
  salary?: number;
  notes?: string;
  roles: string[];
}

export interface UpdateEmployeeRequest {
  email?: string;
  fullName?: string;
  code?: string;
  position?: string;
  departmentId?: number | null;
  phoneNumber?: string;
  dateOfBirth?: string;
  gender?: string;
  address?: string;
  hireDate?: string;
  salary?: number;
  notes?: string;
  roles?: string[];
  isActive?: boolean;
}

export interface EmployeeSearchRequest {
  search?: string;
  departmentId?: number | null;
  role?: string;
  isActive?: boolean;
  page: number;
  pageSize: number;
}

export interface EmployeeListResponse {
  items: Employee[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
}

/** UC-EMP-02 response: the server echoes the (trimmed) name it checked. */
export interface EmployeeUserNameAvailability {
  userName: string;
  isAvailable: boolean;
}

/** Row of `GET /api/LookupManagement/departments` (LookupDto). */
export interface DepartmentLookup {
  id: number;
  name: string;
  nameAr?: string | null;
  nameEn?: string | null;
}

export interface LookupPagedResult<T> {
  items: T[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
}

/** Row of `GET /api/RoleManagement` (RoleListDto). */
export interface RoleListItem {
  id: string;
  name: string;
  displayNameAr?: string | null;
  displayNameEn?: string | null;
}
