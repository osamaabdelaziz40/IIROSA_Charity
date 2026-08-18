import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';
import {
  Role,
  RoleDetail,
  CreateRoleRequest,
  UpdateRoleRequest,
  RoleListResponse
} from '../models/role.model';

@Injectable({
  providedIn: 'root'
})
export class RoleManagementService {
  private endpoint = '/api/rolemanagement';

  constructor(private apiService: ApiService) {}

  /**
   * Get all roles
   * UC-1.8: Update Role Permissions - View Roles
   */
  getRoles(): Observable<Role[]> {
    return this.apiService.get<Role[]>(this.endpoint);
  }

  /**
   * Get role by ID
   */
  getRole(roleId: string): Observable<RoleDetail> {
    return this.apiService.get<RoleDetail>(`${this.endpoint}/${roleId}`);
  }

  /**
   * Create new role
   * UC-1.7: Create Role
   */
  createRole(request: CreateRoleRequest): Observable<RoleDetail> {
    return this.apiService.post<RoleDetail>(this.endpoint, request);
  }

  /**
   * Update role
   * UC-1.8: Update Role Permissions
   */
  updateRole(roleId: string, request: UpdateRoleRequest): Observable<RoleDetail> {
    return this.apiService.put<RoleDetail>(`${this.endpoint}/${roleId}`, request);
  }

  /**
   * Delete role
   */
  deleteRole(roleId: string): Observable<any> {
    return this.apiService.delete<any>(`${this.endpoint}/${roleId}`);
  }

  /**
   * Get users in role
   */
  getUsersInRole(roleId: string): Observable<any[]> {
    return this.apiService.get<any[]>(`${this.endpoint}/${roleId}/users`);
  }

  /**
   * Update role permissions
   */
  updateRolePermissions(roleId: string, permissions: Record<string, boolean>): Observable<any> {
    return this.apiService.put<any>(`${this.endpoint}/${roleId}/permissions`, {
      permissions
    });
  }
}