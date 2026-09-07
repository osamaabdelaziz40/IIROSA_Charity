import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError, map } from 'rxjs/operators';
import { environment } from '../../../../environments/environment';
import {
  OrphanPaymentDto,
  CreateOrphanPaymentDto,
  UpdateOrphanPaymentDto,
  OrphanPaymentSearchRequest,
  OrphanPaymentPagedResult,
  AddOrphansToPaymentDto,
  OrphanSelectionFilter,
  OrphanSelectionPagedResult,
  AddOrphansResultDto,
  ExportPaymentGroupOptions,
  BatchNumberOptionDto,
  UpdateOrphanPaymentItemDto,
  OrphanPaymentItemDto,
  PaymentSummary
} from '../models/orphan-payment.model';

@Injectable({
  providedIn: 'root'
})
export class OrphanPaymentService {
  private apiUrl = `${environment.apiUrl}/api/OrphanPayments`;

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

  // ==================== CRUD OPERATIONS ====================

  /**
   * Get orphan payment groups with pagination and filtering
   */
  getOrphanPayments(searchRequest: OrphanPaymentSearchRequest): Observable<OrphanPaymentPagedResult> {
    return this.http.get<OrphanPaymentPagedResult>(`${this.apiUrl}`, {
      headers: this.getHeaders(),
      params: this.buildHttpParams(searchRequest)
    }).pipe(
      catchError(this.handleError)
    );
  }

  /**
   * Export the payment-group list to Excel — the server ignores paging and writes
   * every row matching the same filters as the list read.
   */
  exportToExcel(searchRequest: OrphanPaymentSearchRequest): Observable<Blob> {
    return this.http.get(`${this.apiUrl}/export`, {
      headers: this.getHeaders(),
      params: this.buildHttpParams(searchRequest),
      responseType: 'blob'
    }).pipe(
      catchError(this.handleError)
    );
  }

  /**
   * Get a specific orphan payment group by ID
   */
  getOrphanPayment(id: string): Observable<OrphanPaymentDto> {
    return this.http.get<OrphanPaymentDto>(`${this.apiUrl}/${id}`, {
      headers: this.getHeaders()
    }).pipe(
      catchError(this.handleError)
    );
  }

  /**
   * UC-ORP-09 — one orphan's rows within a batch: the batch header stays complete, the rows
   * narrow to the orphan. Without orphanId this is the plain UC-5.9 details read.
   */
  getOrphanPaymentDetails(id: string, orphanId?: string): Observable<OrphanPaymentDto> {
    let params = new HttpParams();
    if (orphanId) {
      params = params.set('orphanId', orphanId);
    }
    return this.http.get<OrphanPaymentDto>(`${this.apiUrl}/${id}/details`, {
      headers: this.getHeaders(),
      params
    }).pipe(
      catchError(this.handleError)
    );
  }

  /**
   * UC-ORP-11 — the distinct batch numbers (رقم الحصة) in the caller's scope, most recent first.
   */
  getBatchNumbers(charityId?: string): Observable<BatchNumberOptionDto[]> {
    let params = new HttpParams();
    if (charityId) {
      params = params.set('charityId', charityId);
    }
    return this.http.get<BatchNumberOptionDto[]>(`${this.apiUrl}/batch-numbers`, {
      headers: this.getHeaders(),
      params
    }).pipe(
      catchError(this.handleError)
    );
  }

  /**
   * UC-RPT-32 (§23.U.32 صفحة ملخص الصرف) — the batch's cover figures for the batch view's
   * totals band. Cross-module endpoint (Dashboard, not OrphanPayments) — the call lives here
   * because the panel lives on this module's batch screen (the story's recorded pick). The
   * charity scope resolves server-side from the token; charityId is an HQ-only narrow.
   */
  getPaymentSummary(paymentId: string, charityId?: string): Observable<PaymentSummary> {
    let params = new HttpParams().set('paymentId', paymentId);
    if (charityId) {
      params = params.set('charityId', charityId);
    }
    return this.http.get<PaymentSummary>(
      `${environment.apiUrl}/api/Dashboard/payment-summary`,
      { headers: this.getHeaders(), params }
    ).pipe(
      catchError(this.handleError)
    );
  }

  /**
   * Create a new orphan payment group
   */
  createOrphanPayment(payment: CreateOrphanPaymentDto): Observable<OrphanPaymentDto> {
    return this.http.post<OrphanPaymentDto>(`${this.apiUrl}`, payment, {
      headers: this.getHeaders()
    }).pipe(
      catchError(this.handleError)
    );
  }

  /**
   * Update an existing orphan payment group
   */
  updateOrphanPayment(id: string, payment: UpdateOrphanPaymentDto): Observable<OrphanPaymentDto> {
    return this.http.put<OrphanPaymentDto>(`${this.apiUrl}/${id}`, payment, {
      headers: this.getHeaders()
    }).pipe(
      catchError(this.handleError)
    );
  }

  /**
   * Delete an orphan payment group
   */
  deleteOrphanPayment(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`, {
      headers: this.getHeaders()
    }).pipe(
      catchError(this.handleError)
    );
  }

  // ==================== ORPHAN MANAGEMENT ====================

  /**
   * Get available orphans for selection (not in the payment group yet).
   * 10-2: typed to the live paged envelope — was a bare array that never matched the wire.
   */
  getAvailableOrphans(groupId: string, filter: OrphanSelectionFilter): Observable<OrphanSelectionPagedResult> {
    // Review P18: the page comes from the caller (component pager) — was a hard-coded
    // pageSize 200 that silently made orphans 201+ unenrollable.
    return this.http.get<OrphanSelectionPagedResult>(`${this.apiUrl}/${groupId}/available-orphans`, {
      headers: this.getHeaders(),
      params: this.buildHttpParams({
        ...filter,
        pageNumber: filter.pageNumber || 1,
        pageSize: filter.pageSize || 10
      })
    }).pipe(
      catchError(this.handleError)
    );
  }

  /**
   * Add orphans to a payment group — server reports { message, addedCount, skippedCount }
   * (already-in-group orphans are skipped server-side, UC-5.3 alternative flow).
   */
  addOrphansToGroup(groupId: string, request: AddOrphansToPaymentDto): Observable<AddOrphansResultDto> {
    return this.http.post<AddOrphansResultDto>(`${this.apiUrl}/${groupId}/orphans`, request, {
      headers: this.getHeaders()
    }).pipe(
      catchError(this.handleError)
    );
  }

  /**
   * Remove an orphan row from a payment group — live route is
   * DELETE orphan-items/{orphanPaymentItemId} (was DELETE {groupId}/orphans with a body).
   */
  removeOrphanItem(orphanPaymentItemId: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/orphan-items/${orphanPaymentItemId}`, {
      headers: this.getHeaders()
    }).pipe(
      catchError(this.handleError)
    );
  }

  // 10-3: getGroupOrphans removed — GET {id}/orphans never existed server-side; the row
  // list comes from getOrphanPaymentDetails (GET {id}/details → dto.orphans).

  /**
   * §15.1 row action (10-9): ONE endpoint for the whole flag family — action 0 is the
   * stop/resume toggle (flag carries the direction); actions 1..4 land with 10-10..10-13.
   * Returns the updated row for optimistic-update replacement.
   */
  updateOrphanItem(dto: UpdateOrphanPaymentItemDto): Observable<OrphanPaymentItemDto> {
    return this.http.post<OrphanPaymentItemDto>(`${this.apiUrl}/orphan-items`, dto, {
      headers: this.getHeaders()
    }).pipe(
      catchError(this.handleError)
    );
  }

  // ==================== EXCHANGE RATE MANAGEMENT ====================

  /**
   * Update exchange rate for a payment group
   */
  // 10-4: verb fixes — the backend routes are PUT {id}/exchange-rate and POST
  // {id}/lock-exchange-rate (typed { lockRate }); the old PATCH calls 405'd.
  updateExchangeRate(groupId: string, exchangeRate: number, currency: string, dontRemoveRate?: boolean): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/${groupId}/exchange-rate`, {
      exchangeRate,
      currency,
      dontRemoveRate
    }, {
      headers: this.getHeaders()
    }).pipe(
      catchError(this.handleError)
    );
  }

  /**
   * Lock exchange rate (set DontRemoveRate flag)
   */
  lockExchangeRate(groupId: string): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/${groupId}/lock-exchange-rate`, { lockRate: true }, {
      headers: this.getHeaders()
    }).pipe(
      catchError(this.handleError)
    );
  }

  /**
   * Unlock exchange rate — same endpoint, lockRate false (10-4: no separate unlock route)
   */
  unlockExchangeRate(groupId: string): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/${groupId}/lock-exchange-rate`, { lockRate: false }, {
      headers: this.getHeaders()
    }).pipe(
      catchError(this.handleError)
    );
  }

  // ==================== BATCH UPLOAD STATUS ====================

  // 10-4 verb alignment: the backend route is POST {id}/mark-uploaded with { isUploaded };
  // there is no /unmark-uploaded route (the detail screen's 10-3 buttons were 405ing).
  markAsUploaded(groupId: string): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/${groupId}/mark-uploaded`, { isUploaded: true }, {
      headers: this.getHeaders()
    }).pipe(
      catchError(this.handleError)
    );
  }

  /**
   * Unmark group as uploaded (allow modifications again)
   */
  unmarkAsUploaded(groupId: string): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/${groupId}/mark-uploaded`, { isUploaded: false }, {
      headers: this.getHeaders()
    }).pipe(
      catchError(this.handleError)
    );
  }

  // ==================== BATCH NUMBER MANAGEMENT ====================
  // 10-2: generateNextBatchNumber() removed — GET /batch-number/next never existed server-side;
  // the server auto-generates BatchNo on create when the field is left blank.

  /**
   * Update batch number manually
   */
  updateBatchNumber(groupId: string, batchNo: string): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/${groupId}/batch-number`, { batchNo }, {
      headers: this.getHeaders()
    }).pipe(
      catchError(this.handleError)
    );
  }

  // ==================== EXPORT ====================

  /**
   * Export payment group to Excel or PDF — 10-3: aligned to the live GET {id}/export
   * endpoint (query params, not the body-carrying POST that 405'd). 10-24 re-cuts the
   * payload format.
   */
  exportGroup(groupId: string, options: ExportPaymentGroupOptions): Observable<Blob> {
    const params = new HttpParams()
      .set('format', options.format)
      .set('includePhotos', String(options.includePhotos))
      .set('groupBy', options.groupBy ?? 'None');
    return this.http.get(`${this.apiUrl}/${groupId}/export`, {
      headers: this.getHeaders(),
      params,
      responseType: 'blob'
    }).pipe(
      catchError(this.handleError)
    );
  }

  // 10-3: printGroup and getAuditLogs removed — neither {id}/print nor {id}/audit-logs
  // exists server-side (print returns with 10-24).

  // ==================== ERROR HANDLING ====================

  private handleError(error: any): Observable<never> {
    console.error('Orphan Payment service error:', error);
    return throwError(() => {
      const errorMessage = error.error?.message || error.error?.title || 'An unexpected error occurred';
      return {
        message: errorMessage,
        status: error.status || 500,
        details: error.error?.errors || null
      };
    });
  }

  // ==================== HELPER METHODS ====================

  /**
   * Download blob as file
   */
  downloadFile(blob: Blob, filename: string): void {
    const url = window.URL.createObjectURL(blob);
    const link = document.createElement('a');
    link.href = url;
    link.download = filename;
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
    window.URL.revokeObjectURL(url);
  }

  /**
   * Format date for display
   */
  formatDate(dateString: string): string {
    if (!dateString) return '-';
    return new Date(dateString).toLocaleDateString();
  }

  /**
   * Format currency for display
   */
  formatCurrency(amount: number, currency: string): string {
    return new Intl.NumberFormat('en-US', {
      style: 'currency',
      currency: currency
    }).format(amount);
  }

  /**
   * Get status badge class
   */
  getStatusBadgeClass(isUploaded: boolean): string {
    return isUploaded ? 'badge-success' : 'badge-warning';
  }

  /**
   * Get status text
   */
  getStatusText(isUploaded: boolean): string {
    return isUploaded ? 'Uploaded' : 'Pending';
  }
}
