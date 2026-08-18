import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';
import {
  User,
  CreateUserRequest,
  UpdateUserRequest,
  UserListResponse,
  UserSearchRequest
} from '../models/user.model';

@Injectable({
  providedIn: 'root'
})
export class UserManagementService {
  private endpoint = '/api/usermanagement';

  constructor(private apiService: ApiService) {}

  /**
   * Get all users with filtering and pagination
   * UC-1.9: View All Users
   */
  getUsers(params: UserSearchRequest): Observable<UserListResponse> {
    return this.apiService.get<UserListResponse>(this.endpoint, params);
  }

  /**
   * Get user by ID
   */
  getUser(userId: string): Observable<User> {
    return this.apiService.get<User>(`${this.endpoint}/${userId}`);
  }

  /**
   * Create new user
   * UC-1.2: Create User
   */
  createUser(request: CreateUserRequest): Observable<User> {
    return this.apiService.post<User>(this.endpoint, request);
  }

  /**
   * Update user
   * UC-1.3: Update User
   */
  updateUser(userId: string, request: UpdateUserRequest): Observable<User> {
    return this.apiService.put<User>(`${this.endpoint}/${userId}`, request);
  }

  /**
   * Deactivate user
   * UC-1.4: Deactivate User
   */
  deactivateUser(userId: string): Observable<any> {
    return this.apiService.patch<any>(`${this.endpoint}/${userId}/deactivate`, {});
  }

  /**
   * Activate user
   */
  activateUser(userId: string): Observable<any> {
    return this.apiService.patch<any>(`${this.endpoint}/${userId}/activate`, {});
  }

  /**
   * Reset user password
   * UC-1.5: Reset User Password
   */
  resetUserPassword(userId: string, newPassword?: string, sendEmail: boolean = false): Observable<any> {
    return this.apiService.post<any>(`${this.endpoint}/${userId}/reset-password`, {
      newPassword,
      sendEmail
    });
  }

  /**
   * Assign user to role
   * UC-1.6: Assign User to Role
   */
  assignUserToRole(userId: string, roleName: string): Observable<any> {
    return this.apiService.post<any>(`${this.endpoint}/${userId}/roles`, {
      roleName
    });
  }

  /**
   * Remove user from role
   */
  removeUserFromRole(userId: string, roleName: string): Observable<any> {
    return this.apiService.delete<any>(`${this.endpoint}/${userId}/roles/${roleName}`);
  }

  /**
   * Delete user
   */
  deleteUser(userId: string): Observable<any> {
    return this.apiService.delete<any>(`${this.endpoint}/${userId}`);
  }

  /**
   * Export users to CSV (UC-1.9: View All Users - Export functionality)
   */
  exportUsers(params?: UserSearchRequest): Observable<Blob> {
    return this.apiService.getBlob(`${this.endpoint}/export`, params);
  }

  /**
   * Get user activity log (UC-1.10: View User Activity)
   */
  getUserActivity(userId: string): Observable<any> {
    return this.apiService.get<any>(`${this.endpoint}/${userId}/activity`);
  }

  /**
   * Get user claims (UC-1.11: Manage User Claims)
   */
  getUserClaims(userId: string): Observable<any> {
    return this.apiService.get<any>(`${this.endpoint}/${userId}/claims`);
  }

  /**
   * Add claim to user (UC-1.11: Manage User Claims)
   */
  addUserClaim(userId: string, claimType: string, claimValue: string): Observable<any> {
    return this.apiService.post<any>(`${this.endpoint}/${userId}/claims`, {
      claimType,
      claimValue
    });
  }

  /**
   * Remove claim from user (UC-1.11: Manage User Claims)
   */
  removeUserClaim(userId: string, claimType: string): Observable<any> {
    return this.apiService.delete<any>(`${this.endpoint}/${userId}/claims/${claimType}`);
  }
}