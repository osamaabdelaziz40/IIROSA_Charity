import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { Observable, throwError, forkJoin, of } from 'rxjs';
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
  PeriodicOrphanReportPagedResult,
  PeriodicOrphanReportStatistics,
  OrphanReportStatusCounts,
  OrphanLookupDto,
  OrphanReportFormPrintPayload
} from '../models/periodic-orphan-report.model';

@Injectable({
  providedIn: 'root'
})
export class PeriodicOrphanReportService {
  // Review P59 2026-08-24: environment.prod.ts's apiUrl already ends in '/api'
  // while the dev value does not — appending '/api' again would double the segment
  // in production. Normalize here (epic-9 scope); the core services that assume
  // the suffixed value are left to their owning sessions.
  private readonly baseApiUrl = environment.apiUrl.replace(/\/api\/?$/, '');
  private apiUrl = `${this.baseApiUrl}/api/PeriodicOrphanReports`;

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
    // Review P38 2026-08-24: keep the HTTP status reachable on the rethrown value
    // so components can distinguish "unknown code" (404) from transport/5xx
    // failures. The payload shape existing consumers read is unchanged.
    const rethrown: any = error?.error || error?.message || 'Server error';
    if (rethrown && typeof rethrown === 'object') {
      rethrown.status = error?.status;
    }
    return throwError(() => rethrown);
  }

  // ==================== CRUD OPERATIONS ====================

  /**
   * Look an orphan up by sponsorship code before report entry - UC-ORR-02.
   * 404 when the code is unknown or outside the caller's charity scope.
   */
  getOrphanByCode(code: string): Observable<OrphanLookupDto> {
    return this.http.get<OrphanLookupDto>(`${this.apiUrl}/by-code/${encodeURIComponent(code)}`, {
      headers: this.getHeaders()
    }).pipe(
      catchError(this.handleError)
    );
  }

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
   * Register statistics for the band above the periodic reports grid (§14.S.1, UC-ORR-01) —
   * caller-scoped server-side (charity pin or country pin), like the register read.
   */
  getStatistics(): Observable<PeriodicOrphanReportStatistics> {
    return this.http.get<PeriodicOrphanReportStatistics>(`${this.apiUrl}/statistics`, {
      headers: this.getHeaders()
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
   * Status-grouped report counts — مقبول / مرفوض / قيد الانتظار (18-14 / UC-RPT-14).
   * Rides the three paged reads above (totalCount only, pageSize 1); pending is derived
   * as all − accepted − refused, never recounted client-side. A failed leg yields null
   * so the remaining tiles still render.
   */
  getStatusCounts(filter?: Partial<PeriodicOrphanReportFilterDto>): Observable<OrphanReportStatusCounts> {
    const countsFilter: PeriodicOrphanReportFilterDto = {
      pageNumber: 1,
      pageSize: 1,
      ...filter
    };
    const totalCount = (result: PeriodicOrphanReportPagedResult): number => result.totalCount;
    return forkJoin({
      all: this.getReports(countsFilter).pipe(
        map(totalCount),
        catchError(() => of<number | null>(null))
      ),
      accepted: this.getApprovedReports(countsFilter).pipe(
        map(totalCount),
        catchError(() => of<number | null>(null))
      ),
      refused: this.getRejectedReports(countsFilter).pipe(
        map(totalCount),
        catchError(() => of<number | null>(null))
      )
    }).pipe(
      map(counts => ({
        ...counts,
        pending:
          counts.all === null || counts.accepted === null || counts.refused === null
            ? null
            : Math.max(counts.all - counts.accepted - counts.refused, 0)
      }))
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

  // ==================== PRINT OPERATIONS (UC-ORR-17) ====================

  /**
   * UC-ORR-17 (§14.U.17 طباعة التقرير الدوري) — the print payload: detail data + the
   * resolved layout variant + the attachment ids present. Served by the ReportsController
   * root under its legacy /export/pdf shape (client-side print ruling: JSON, not bytes —
   * the SPA renders the form and the browser's print-to-PDF produces the file).
   * 404 when there is nothing to produce (unknown/foreign id).
   */
  getPrintForm(id: string): Observable<OrphanReportFormPrintPayload> {
    return this.http.post<OrphanReportFormPrintPayload>(
      `${this.baseApiUrl}/api/Reports/orphan-report-form/export/pdf`,
      { reportId: id },
      { headers: this.getHeaders() }
    ).pipe(
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
