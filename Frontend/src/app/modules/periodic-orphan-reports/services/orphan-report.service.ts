import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { environment } from '../../../../environments/environment';
import {
  OrphanReportFilterDto,
  OrphanReportResultDto,
  OrphanReportExportDto,
  OrphanReportHistoryDto,
  OrphanReportComparisonDto,
  ScheduleRecurringReportDto,
  ScheduledReportDto,
  OrphanStatisticsDto
} from '../models/periodic-orphan-report.model';

@Injectable({
  providedIn: 'root'
})
export class OrphanReportService {
  // Review P59 2026-08-24: environment.prod.ts's apiUrl already ends in '/api'
  // while the dev value does not — appending '/api' again would double the segment
  // in production. Normalize here (epic-9 scope); the core services that assume
  // the suffixed value are left to their owning sessions.
  private readonly baseApiUrl = environment.apiUrl.replace(/\/api\/?$/, '');
  private apiUrl = `${this.baseApiUrl}/api/OrphanReports`;

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

  // ==================== REPORT GENERATION (UC-6.1) ====================

  /**
   * Generate orphan report with filters - UC-6.1
   */
  generateReport(filter: OrphanReportFilterDto): Observable<OrphanReportResultDto> {
    return this.http.post<OrphanReportResultDto>(`${this.apiUrl}/generate`, filter, {
      headers: this.getHeaders()
    }).pipe(
      catchError(this.handleError)
    );
  }

  // ==================== EXPORT OPERATIONS (UC-6.7) ====================

  /**
   * Export orphan report to Excel or PDF - UC-6.7
   */
  exportReport(filter: OrphanReportFilterDto, exportOptions?: OrphanReportExportDto): Observable<Blob> {
    const params = this.buildHttpParams(exportOptions || {});
    return this.http.post(`${this.apiUrl}/export`, filter, {
      headers: this.getHeaders(),
      params,
      responseType: 'blob'
    }).pipe(
      catchError(this.handleError)
    );
  }

  // ==================== REPORT HISTORY (UC-6.9) ====================

  /**
   * View report history - UC-6.9
   */
  getReportHistory(pageNumber = 1, pageSize = 20): Observable<{ items: OrphanReportHistoryDto[]; totalCount: number }> {
    return this.http.get<{ items: OrphanReportHistoryDto[]; totalCount: number }>(`${this.apiUrl}/history`, {
      headers: this.getHeaders(),
      params: this.buildHttpParams({ pageNumber, pageSize })
    }).pipe(
      catchError(this.handleError)
    );
  }

  /**
   * Get report history entry by ID
   */
  getReportHistoryEntry(id: string): Observable<OrphanReportHistoryDto> {
    return this.http.get<OrphanReportHistoryDto>(`${this.apiUrl}/history/${id}`, {
      headers: this.getHeaders()
    }).pipe(
      catchError(this.handleError)
    );
  }

  /**
   * Delete report from history
   */
  deleteReportHistoryEntry(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/history/${id}`, {
      headers: this.getHeaders()
    }).pipe(
      catchError(this.handleError)
    );
  }

  // ==================== PERIOD COMPARISON (UC-6.10) ====================

  /**
   * Compare two report periods - UC-6.10
   */
  comparePeriods(report1Id: string, report2Id: string): Observable<OrphanReportComparisonDto> {
    return this.http.post<OrphanReportComparisonDto>(`${this.apiUrl}/compare`, {
      report1Id,
      report2Id
    }, {
      headers: this.getHeaders()
    }).pipe(
      catchError(this.handleError)
    );
  }

  // ==================== SCHEDULING (UC-6.8) ====================

  /**
   * Schedule recurring report - UC-6.8
   */
  scheduleRecurringReport(dto: ScheduleRecurringReportDto): Observable<string> {
    return this.http.post<string>(`${this.apiUrl}/schedule`, dto, {
      headers: this.getHeaders()
    }).pipe(
      catchError(this.handleError)
    );
  }

  /**
   * Get scheduled reports
   */
  getScheduledReports(pageNumber = 1, pageSize = 20): Observable<{ items: ScheduledReportDto[]; totalCount: number }> {
    return this.http.get<{ items: ScheduledReportDto[]; totalCount: number }>(`${this.apiUrl}/scheduled`, {
      headers: this.getHeaders(),
      params: this.buildHttpParams({ pageNumber, pageSize })
    }).pipe(
      catchError(this.handleError)
    );
  }

  /**
   * Update scheduled report
   */
  updateScheduledReport(scheduleId: string, dto: ScheduleRecurringReportDto): Observable<any> {
    return this.http.put(`${this.apiUrl}/scheduled/${scheduleId}`, dto, {
      headers: this.getHeaders()
    }).pipe(
      catchError(this.handleError)
    );
  }

  /**
   * Delete scheduled report
   */
  deleteScheduledReport(scheduleId: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/scheduled/${scheduleId}`, {
      headers: this.getHeaders()
    }).pipe(
      catchError(this.handleError)
    );
  }

  // ==================== STATISTICS ====================

  /**
   * Get orphan statistics for dashboard
   */
  getOrphanStatistics(filter: OrphanReportFilterDto): Observable<OrphanStatisticsDto> {
    return this.http.post<OrphanStatisticsDto>(`${this.apiUrl}/statistics`, filter, {
      headers: this.getHeaders()
    }).pipe(
      catchError(this.handleError)
    );
  }
}
