import { Injectable } from '@angular/core';
import { ApiService } from '../../../core/services/api.service';
import { Observable } from 'rxjs';
import { User, CreateUserRequest, UpdateUserRequest, UserListResponse, UserSearchRequest } from '../../../core/models/user.model';
import { Role, CreateRoleRequest, UpdateRoleRequest } from '../../../core/models/role.model';
import { ApiResponse, PagedResponse } from '../../../core/models/common.model';

@Injectable({
  providedIn: 'root'
})
export class UserManagementService {
  private readonly endpoint = '/api/usermanagement';

  constructor(private api: ApiService) {}

  // User CRUD
  getUsers(search: UserSearchRequest): Observable<PagedResponse<User>> {
    return this.api.get(`${this.endpoint}`, search);
  }

  getUserById(id: string): Observable<User> {
    return this.api.get(`${this.endpoint}/${id}`);
  }

  createUser(request: CreateUserRequest): Observable<ApiResponse<User>> {
    return this.api.post(`${this.endpoint}`, request);
  }

  updateUser(id: string, request: UpdateUserRequest): Observable<ApiResponse<User>> {
    return this.api.put(`${this.endpoint}/${id}`, request);
  }

  deleteUser(id: string): Observable<ApiResponse<boolean>> {
    return this.api.delete(`${this.endpoint}/${id}`);
  }

  deactivateUser(id: string): Observable<ApiResponse<boolean>> {
    return this.api.patch(`${this.endpoint}/${id}/deactivate`, {});
  }

  activateUser(id: string): Observable<ApiResponse<boolean>> {
    return this.api.patch(`${this.endpoint}/${id}/activate`, {});
  }

  resetPassword(id: string, newPassword?: string): Observable<ApiResponse<string>> {
    return this.api.post(`${this.endpoint}/${id}/reset-password`, { newPassword });
  }

  exportUsers(search: UserSearchRequest): Observable<Blob> {
    return this.api.download(`${this.endpoint}/export`);
  }

  // Role Assignment
  getUserRoles(id: string): Observable<any[]> {
    return this.api.get(`${this.endpoint}/${id}/roles`);
  }

  assignUserRoles(id: string, roleNames: string[]): Observable<ApiResponse<boolean>> {
    return this.api.post(`${this.endpoint}/${id}/roles`, { roleNames });
  }

  // Role Management
  getRoles(pageNumber: number = 1, pageSize: number = 20): Observable<PagedResponse<Role>> {
    return this.api.get(`${this.endpoint}/roles`, { pageNumber, pageSize });
  }

  getRoleById(id: string): Observable<Role> {
    return this.api.get(`${this.endpoint}/roles/${id}`);
  }

  getAllRoles(): Observable<Role[]> {
    return this.api.get(`${this.endpoint}/roles/all`);
  }

  createRole(request: CreateRoleRequest): Observable<ApiResponse<Role>> {
    return this.api.post(`${this.endpoint}/roles`, request);
  }

  updateRole(id: string, request: UpdateRoleRequest): Observable<ApiResponse<Role>> {
    return this.api.put(`${this.endpoint}/roles/${id}`, request);
  }

  deleteRole(id: string): Observable<ApiResponse<boolean>> {
    return this.api.delete(`${this.endpoint}/roles/${id}`);
  }

  // Activity Log
  getUserActivity(id: string): Observable<any> {
    return this.api.get(`${this.endpoint}/${id}/activity`);
  }

  // Claims
  getUserClaims(id: string): Observable<any[]> {
    return this.api.get(`${this.endpoint}/${id}/claims`);
  }

  addUserClaim(id: string, claimType: string, claimValue: string): Observable<ApiResponse<boolean>> {
    return this.api.post(`${this.endpoint}/${id}/claims`, { claimType, claimValue });
  }

  removeUserClaim(id: string, claimType: string): Observable<ApiResponse<boolean>> {
    return this.api.delete(`${this.endpoint}/${id}/claims/${claimType}`);
  }
}
