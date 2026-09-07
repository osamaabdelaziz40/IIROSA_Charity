import { Injectable } from '@angular/core';
import { HttpClient, HttpParameterCodec, HttpParams } from '@angular/common/http';
import { ApiService } from '../../../core/services/api.service';
import { Observable, forkJoin, of } from 'rxjs';
import { map, switchMap } from 'rxjs/operators';
import { environment } from '../../../../environments/environment';
import {
  Employee,
  CreateEmployeeRequest,
  UpdateEmployeeRequest,
  EmployeeSearchRequest,
  EmployeeUserNameAvailability,
  DepartmentLookup,
  LookupPagedResult,
  RoleListItem
} from '../../../core/models/employee.model';
import { ApiResponse, PagedResponse } from '../../../core/models/common.model';

/**
 * Percent-encodes query parameters strictly, unlike Angular's default codec which un-escapes a
 * set of characters — including `+`, which a server reads as a space. Emails contain `+` often
 * enough (tagged addresses) that the availability check must agree with the server on the exact
 * string it is checking.
 */
class StrictHttpParameterCodec implements HttpParameterCodec {
  encodeKey(key: string): string {
    return encodeURIComponent(key);
  }

  encodeValue(value: string): string {
    return encodeURIComponent(value);
  }

  decodeKey(key: string): string {
    return decodeURIComponent(key);
  }

  decodeValue(value: string): string {
    return decodeURIComponent(value);
  }
}

@Injectable({
  providedIn: 'root'
})
export class EmployeeService {
  private readonly endpoint = '/api/employeemanagement';

  constructor(
    private api: ApiService,
    private http: HttpClient
  ) {}

  // Employee CRUD
  getEmployees(search: EmployeeSearchRequest): Observable<PagedResponse<Employee>> {
    return this.api.get(`${this.endpoint}`, search);
  }

  getEmployeeById(id: string): Observable<Employee> {
    return this.api.get(`${this.endpoint}/${id}`);
  }

  createEmployee(request: CreateEmployeeRequest): Observable<Employee> {
    return this.api.post(`${this.endpoint}`, request);
  }

  updateEmployee(id: string, request: UpdateEmployeeRequest): Observable<Employee> {
    return this.api.put(`${this.endpoint}/${id}`, request);
  }

  deleteEmployee(id: string): Observable<ApiResponse<boolean>> {
    return this.api.delete(`${this.endpoint}/${id}`);
  }

  deactivateEmployee(id: string): Observable<ApiResponse<boolean>> {
    return this.api.patch(`${this.endpoint}/${id}/deactivate`, {});
  }

  activateEmployee(id: string): Observable<ApiResponse<boolean>> {
    return this.api.patch(`${this.endpoint}/${id}/activate`, {});
  }

  resetPassword(id: string, newPassword?: string): Observable<ApiResponse<string>> {
    return this.api.post(`${this.endpoint}/${id}/reset-password`, { newPassword });
  }

  exportEmployees(search: EmployeeSearchRequest): Observable<Blob> {
    return this.api.download(`${this.endpoint}/export`);
  }

  // Role Assignment
  getEmployeeRoles(id: string): Observable<any[]> {
    return this.api.get(`${this.endpoint}/${id}/roles`);
  }

  assignEmployeeRole(id: string, roleName: string): Observable<ApiResponse<boolean>> {
    return this.api.post(`${this.endpoint}/${id}/roles`, { roleName });
  }

  // UC-EMP-02: login-name availability. The server consults both the identity user store and
  // the Employee table, and echoes the name it actually checked so stale replies can be dropped.
  checkUserNameAvailability(userName: string, excludeEmployeeId?: string): Observable<EmployeeUserNameAvailability> {
    let params = new HttpParams({ encoder: new StrictHttpParameterCodec() }).set('userName', userName);
    if (excludeEmployeeId) {
      params = params.set('excludeEmployeeId', excludeEmployeeId);
    }
    return this.http.get<EmployeeUserNameAvailability>(
      `${environment.apiUrl}${this.endpoint}/check-username`,
      { params }
    );
  }

  /**
   * Departments come from the shared lookup module (UC-14.9) — the old
   * `EmployeeManagement/departments` route never existed on the server.
   * Pages through the whole active catalogue so a register with more than one
   * page of departments is not silently truncated.
   */
  getDepartments(): Observable<DepartmentLookup[]> {
    const pageSize = 100;
    const url = `${environment.apiUrl}/api/LookupManagement/departments`;
    const params = (page: number) => ({ page: String(page), pageSize: String(pageSize), isActive: 'true' });

    return this.http
      .get<LookupPagedResult<DepartmentLookup>>(url, { params: params(1) })
      .pipe(
        switchMap(first => {
          const firstItems = first?.items ?? [];
          const pages = Math.ceil((first?.totalCount ?? firstItems.length) / pageSize);
          if (pages <= 1) {
            return of(firstItems);
          }
          const rest = Array.from({ length: pages - 1 }, (_, i) =>
            this.http
              .get<LookupPagedResult<DepartmentLookup>>(url, { params: params(i + 2) })
              .pipe(map(result => result?.items ?? []))
          );
          return forkJoin(rest).pipe(map(lists => firstItems.concat(...lists)));
        })
      );
  }

  /** Roles come from the identity store via RoleManagement (AllRoles policy). */
  getRoles(): Observable<RoleListItem[]> {
    return this.api.get<RoleListItem[]>('/api/RoleManagement');
  }
}
