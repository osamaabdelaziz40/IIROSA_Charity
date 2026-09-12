import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { environment } from '../../../../environments/environment';
import {
  OfficeProject,
  OfficeProjectDto,
  OfficeProjectFilter,
  OfficeProjectListItem,
  OfficeProjectPagedResult,
  OfficeProjectStatistics
} from '../models/office-project.model';

/**
 * Office Development Projects API client (UC-OFP-01…06).
 *
 * The module's lookups (types, countries, regions, centers, charities) are loaded through
 * LookupManagementService / CharityService by the components — this service deliberately does
 * not duplicate them, so there is exactly one source of truth per lookup.
 */
@Injectable({
  providedIn: 'root'
})
export class OfficeProjectService {
  private apiUrl = `${environment.apiUrl}/api/OfficeProjectManagement`;

  constructor(private http: HttpClient) {}

  private getHeaders(): HttpHeaders {
    return new HttpHeaders({
      'Content-Type': 'application/json'
    });
  }

  private buildHttpParams(filter: any): HttpParams {
    let params = new HttpParams();
    if (filter) {
      Object.keys(filter).forEach(key => {
        const value = filter[key];
        if (value !== undefined && value !== null && value !== '') {
          params = params.set(key, value.toString());
        }
      });
    }
    return params;
  }

  private handleError(error: any): Observable<never> {
    console.error('Office Project service error:', error);
    return throwError(() => {
      const errorMessage = error.error?.message || error.error?.title || 'An unexpected error occurred';
      return {
        message: errorMessage,
        status: error.status || 500,
        details: error.error?.errors || null
      };
    });
  }

  /**
   * Get all office projects with filtering and pagination (UC-OFP-01: list)
   */
  getAllProjects(filter?: OfficeProjectFilter): Observable<OfficeProjectPagedResult<OfficeProjectListItem>> {
    return this.http.get<OfficeProjectPagedResult<OfficeProjectListItem>>(this.apiUrl, {
      headers: this.getHeaders(),
      params: this.buildHttpParams(filter)
    }).pipe(
      catchError(this.handleError)
    );
  }

  /**
   * Register statistics for the band above the list (UC-OFP-01) — caller country-scoped
   * server-side; describes the whole register, not the current search
   */
  getStatistics(): Observable<OfficeProjectStatistics> {
    return this.http.get<OfficeProjectStatistics>(`${this.apiUrl}/statistics`, {
      headers: this.getHeaders()
    }).pipe(
      catchError(this.handleError)
    );
  }

  /**
   * Get project by ID (UC-OFP-04: view)
   */
  getProjectById(id: string): Observable<OfficeProject> {
    return this.http.get<OfficeProject>(`${this.apiUrl}/${id}`, {
      headers: this.getHeaders()
    }).pipe(
      catchError(this.handleError)
    );
  }

  /**
   * Create new project (UC-OFP-03)
   */
  createProject(project: OfficeProjectDto): Observable<OfficeProject> {
    return this.http.post<OfficeProject>(this.apiUrl, project, {
      headers: this.getHeaders()
    }).pipe(
      catchError(this.handleError)
    );
  }

  /**
   * Update project (UC-OFP-04)
   */
  updateProject(id: string, project: OfficeProjectDto): Observable<OfficeProject> {
    return this.http.put<OfficeProject>(`${this.apiUrl}/${id}`, project, {
      headers: this.getHeaders()
    }).pipe(
      catchError(this.handleError)
    );
  }

  /**
   * Delete project (UC-OFP-05). The endpoint authorises SuperAdmin only — the General
   * Director's role in the seeded role set.
   */
  deleteProject(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`, {
      headers: this.getHeaders()
    }).pipe(
      catchError(this.handleError)
    );
  }

  /**
   * Mark project as completed (module completion tracking, feeds the progress view)
   */
  markAsCompleted(id: string, isFinished: boolean = true, projectEndDate?: Date): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/${id}/complete`, {
      isFinished,
      projectEndDate
    }, {
      headers: this.getHeaders()
    }).pipe(
      catchError(this.handleError)
    );
  }

  /**
   * Export projects to Excel (UC-OFP-06: report) — a GET read like the list, filter bound
   * from the query string, response streamed as a blob.
   */
  exportToExcel(filter?: OfficeProjectFilter): Observable<Blob> {
    return this.http.get(`${this.apiUrl}/export`, {
      headers: this.getHeaders(),
      params: this.buildHttpParams(filter),
      responseType: 'blob'
    }).pipe(
      catchError(this.handleError)
    );
  }

  /**
   * Project types lookup (UC-OFP-02: select a project type) feeding both the list filter and
   * the form's mandatory dropdown.
   */
  getProjectTypes(): Observable<any[]> {
    return this.http.get<any[]>(`${environment.apiUrl}/api/LookupManagement/office-project-types`, {
      headers: this.getHeaders()
    }).pipe(
      catchError(this.handleError)
    );
  }
}
