import { Injectable } from '@angular/core';
import { ApiService } from '../../../core/services/api.service';
import { Observable } from 'rxjs';
import { Employee, CreateEmployeeRequest, UpdateEmployeeRequest, EmployeeListResponse, EmployeeSearchRequest } from '../../../core/models/employee.model';
import { ApiResponse, PagedResponse } from '../../../core/models/common.model';

@Injectable({
  providedIn: 'root'
})
export class EmployeeService {
  private readonly endpoint = '/api/employeemanagement';

  constructor(private api: ApiService) {}

  // Employee CRUD
  getEmployees(search: EmployeeSearchRequest): Observable<PagedResponse<Employee>> {
    return this.api.get(`${this.endpoint}`, search);
  }

  getEmployeeById(id: string): Observable<Employee> {
    return this.api.get(`${this.endpoint}/${id}`);
  }

  createEmployee(request: CreateEmployeeRequest): Observable<ApiResponse<Employee>> {
    return this.api.post(`${this.endpoint}`, request);
  }

  updateEmployee(id: string, request: UpdateEmployeeRequest): Observable<ApiResponse<Employee>> {
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

  // Department Management
  getDepartments(): Observable<string[]> {
    return this.api.get(`${this.endpoint}/departments`);
  }
}
