import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError, map } from 'rxjs/operators';
import { environment } from '../../../../environments/environment';
import {
  PeriodicOrphanReportDto,
  PeriodicOrphanReportListDto,
  CreatePeriodicOrphanReportDto,
  UpdatePeriodicOrphanReportDto,
  ReviewPeriodicReportDto,
  PeriodicOrphanReportFilterDto,
  PeriodicOrphanReportSummaryDto,
  PeriodicOrphanReportPagedResult
} from '../models/periodic-orphan-report.model';

@Injectable({
  providedIn: 'root'
})
export class PeriodicOrphanReportService {
  private apiUrl = `${environment.apiUrl}/api/PeriodicOrphanReports`;

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
    console.error('API Error:', error);
    return throwError(() => error.error || error.message || 'Server error');
  }

  // ==================== CRUD OPERATIONS ====================

  /**
   * Create new periodic orphan report - UC-6.11
   */
  createReport(report: CreatePeriodicOrphanReportDto): Observable<PeriodicOrphanReportDto> {
    return this.http.post<PeriodicOrphanReportDto>(`${this.apiUrl}`, report, {
      headers: this.getHeaders()
    }).pipe(
      catchError(this.handleError)
    );
  }

  /**
   * Get report by ID - UC-6.11, UC-6.13, UC-6.14, UC-6.15, UC-6.16
   */
  getReport(id: string): Observable<PeriodicOrphanReportDto> {
    return this.http.get<PeriodicOrphanReportDto>(`${this.apiUrl}/${id}`, {
      headers: this.getHeaders()
    }).pipe(
      catchError(this.handleError)
    );
  }

  /**
   * Update periodic orphan report - UC-6.11
   */
  updateReport(id: string, report: UpdatePeriodicOrphanReportDto): Observable<PeriodicOrphanReportDto> {
    return this.http.put<PeriodicOrphanReportDto>(`${this.apiUrl}/${id}`, report, {
      headers: this.getHeaders()
    }).pipe(
      catchError(this.handleError)
    );
  }

  /**
   * Delete periodic orphan report - UC-6.11
   */
  deleteReport(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`, {
      headers: this.getHeaders()
    }).pipe(
      catchError(this.handleError)
    );
  }

  // ==================== REVIEW OPERATIONS (UC-6.13) ====================

  /**
   * Review periodic report (approve or reject) - UC-6.13
   */
  reviewReport(id: string, review: ReviewPeriodicReportDto): Observable<PeriodicOrphanReportDto> {
    return this.http.post<PeriodicOrphanReportDto>(`${this.apiUrl}/${id}/review`, review, {
      headers: this.getHeaders()
    }).pipe(
      catchError(this.handleError)
    );
  }

  // ==================== LIST AND FILTER OPERATIONS ====================

  /**
   * Get periodic reports with filtering and pagination - UC-6.14, UC-6.15, UC-6.16
   */
  getReports(filter: PeriodicOrphanReportFilterDto): Observable<PeriodicOrphanReportPagedResult> {
    return this.http.get<PeriodicOrphanReportPagedResult>(`${this.apiUrl}`, {
      headers: this.getHeaders(),
      params: this.buildHttpParams(filter)
    }).pipe(
      catchError(this.handleError)
    );
  }

  /**
   * Get approved reports - UC-6.14
   */
  getApprovedReports(filter: PeriodicOrphanReportFilterDto): Observable<PeriodicOrphanReportPagedResult> {
    return this.http.get<PeriodicOrphanReportPagedResult>(`${this.apiUrl}/approved`, {
      headers: this.getHeaders(),
      params: this.buildHttpParams(filter)
    }).pipe(
      catchError(this.handleError)
    );
  }

  /**
   * Get rejected reports - UC-6.15
   */
  getRejectedReports(filter: PeriodicOrphanReportFilterDto): Observable<PeriodicOrphanReportPagedResult> {
    return this.http.get<PeriodicOrphanReportPagedResult>(`${this.apiUrl}/rejected`, {
      headers: this.getHeaders(),
      params: this.buildHttpParams(filter)
    }).pipe(
      catchError(this.handleError)
    );
  }

  /**
   * Get reports by orphan - UC-6.16
   */
  getReportsByOrphan(orphanId: string, pageNumber = 1, pageSize = 20): Observable<PeriodicOrphanReportPagedResult> {
    return this.http.get<PeriodicOrphanReportPagedResult>(`${this.apiUrl}/by-orphan/${orphanId}`, {
      headers: this.getHeaders(),
      params: this.buildHttpParams({ pageNumber, pageSize })
    }).pipe(
      catchError(this.handleError)
    );
  }

  /**
   * Get orphan report summary - UC-6.16
   */
  getOrphanReportSummary(orphanId: string): Observable<PeriodicOrphanReportSummaryDto> {
    return this.http.get<PeriodicOrphanReportSummaryDto>(`${this.apiUrl}/by-orphan/${orphanId}/summary`, {
      headers: this.getHeaders()
    }).pipe(
      catchError(this.handleError)
    );
  }

  // ==================== EXPORT OPERATIONS (UC-6.17) ====================

  /**
   * Export periodic reports to Excel - UC-6.17
   */
  exportToExcel(filter: PeriodicOrphanReportFilterDto, includeAllFields = false): Observable<Blob> {
    return this.http.post(`${this.apiUrl}/export`, filter, {
      headers: this.getHeaders(),
      params: { includeAllFields },
      responseType: 'blob'
    }).pipe(
      catchError(this.handleError)
    );
  }

  /**
   * Export orphan's report history to Excel - UC-6.16, UC-6.17
   */
  exportOrphanHistoryToExcel(orphanId: string): Observable<Blob> {
    return this.http.post(`${this.apiUrl}/by-orphan/${orphanId}/export`, {}, {
      headers: this.getHeaders(),
      responseType: 'blob'
    }).pipe(
      catchError(this.handleError)
    );
  }

  // ==================== STATUS MANAGEMENT (UC-6.11) ====================

  /**
   * Lock report - UC-6.11
   */
  lockReport(id: string): Observable<any> {
    return this.http.post(`${this.apiUrl}/${id}/lock`, {}, {
      headers: this.getHeaders()
    }).pipe(
      catchError(this.handleError)
    );
  }

  /**
   * Unlock report - UC-6.11
   */
  unlockReport(id: string): Observable<any> {
    return this.http.post(`${this.apiUrl}/${id}/unlock`, {}, {
      headers: this.getHeaders()
    }).pipe(
      catchError(this.handleError)
    );
  }

  /**
   * Activate report - UC-6.11
   */
  activateReport(id: string): Observable<any> {
    return this.http.post(`${this.apiUrl}/${id}/activate`, {}, {
      headers: this.getHeaders()
    }).pipe(
      catchError(this.handleError)
    );
  }

  /**
   * Deactivate report - UC-6.11
   */
  deactivateReport(id: string): Observable<any> {
    return this.http.post(`${this.apiUrl}/${id}/deactivate`, {}, {
      headers: this.getHeaders()
    }).pipe(
      catchError(this.handleError)
    );
  }

  // ==================== HELPER METHODS ====================

  /**
   * Check if report can be edited
   */
  canEditReport(id: string): Observable<{ canEdit: boolean }> {
    return this.http.get<{ canEdit: boolean }>(`${this.apiUrl}/${id}/can-edit`, {
      headers: this.getHeaders()
    }).pipe(
      catchError(this.handleError)
    );
  }
}
