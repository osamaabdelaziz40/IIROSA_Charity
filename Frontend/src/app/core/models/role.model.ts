export interface Role {
  id: string;
  name: string;
  description?: string;
  isSystemRole: boolean;
  userCount?: number;
  permissions?: string[];
}

export interface RoleDetail {
  id: string;
  name: string;
  description?: string;
  isSystemRole: boolean;
  permissions: string[];
  claims: RoleClaim[];
}

export interface RoleClaim {
  claimType: string;
  claimValue: string;
}

export interface CreateRoleRequest {
  name: string;
  description?: string;
  permissions: string[];
  claims: RoleClaim[];
}

export interface UpdateRoleRequest {
  name?: string;
  description?: string;
  permissions?: string[];
  claims?: RoleClaim[];
}

export interface RoleListResponse {
  items: Role[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
}
