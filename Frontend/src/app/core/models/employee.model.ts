export interface Employee {
  id: string;
  email: string;
  firstName: string;
  lastName: string;
  employeeCode?: string;
  position: string;
  department?: string;
  phoneNumber?: string;
  dateOfBirth?: string;
  gender?: string;
  address?: string;
  nationalId?: string;
  hireDate: string;
  salary?: number;
  notes?: string;
  isActive: boolean;
  roles: string[];
  departmentId?: string;
  createdOn: string;
  modifiedOn?: string;
  fullName: string; // Computed property
}

export interface CreateEmployeeRequest {
  email: string;
  firstName: string;
  lastName: string;
  employeeCode?: string;
  position: string;
  department?: string;
  phoneNumber?: string;
  dateOfBirth?: string;
  gender?: string;
  address?: string;
  hireDate: string;
  salary?: number;
  roleName: string;
}

export interface UpdateEmployeeRequest {
  email: string;
  firstName: string;
  lastName: string;
  employeeCode?: string;
  position: string;
  department?: string;
  phoneNumber?: string;
  dateOfBirth?: string;
  gender?: string;
  address?: string;
  salary?: number;
  roleName?: string;
}

export interface EmployeeSearchRequest {
  search?: string;
  department?: string;
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

export const departments = [
  'Management',
  'Human Resources',
  'Finance',
  'IT',
  'Operations',
  'Marketing',
  'Sales',
  'Customer Service'
];
