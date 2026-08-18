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
  RemoveOrphanFromPaymentDto,
  OrphanSelectionFilter,
  OrphanForSelectionDto,
  ExportPaymentGroupOptions,
  OrphanPaymentAuditLog,
  OrphanPaymentStatistics,
  BatchNumberGeneration,
  OrphanPaymentItemDto
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
   * Get available orphans for selection (not in the payment group yet)
   */
  getAvailableOrphans(groupId: string, filter: OrphanSelectionFilter): Observable<OrphanForSelectionDto[]> {
    return this.http.get<OrphanForSelectionDto[]>(`${this.apiUrl}/${groupId}/available-orphans`, {
      headers: this.getHeaders(),
      params: this.buildHttpParams(filter)
    }).pipe(
      catchError(this.handleError)
    );
  }

  /**
   * Add orphans to a payment group
   */
  addOrphansToGroup(groupId: string, request: AddOrphansToPaymentDto): Observable<OrphanPaymentItemDto[]> {
    return this.http.post<OrphanPaymentItemDto[]>(`${this.apiUrl}/${groupId}/orphans`, request, {
      headers: this.getHeaders()
    }).pipe(
      catchError(this.handleError)
    );
  }

  /**
   * Remove an orphan from a payment group
   */
  removeOrphanFromGroup(groupId: string, request: RemoveOrphanFromPaymentDto): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${groupId}/orphans`, {
      headers: this.getHeaders(),
      body: request
    }).pipe(
      catchError(this.handleError)
    );
  }

  /**
   * Get orphans in a payment group
   */
  getGroupOrphans(groupId: string, page: number = 1, pageSize: number = 50): Observable<{
    items: OrphanPaymentItemDto[];
    totalCount: number;
  }> {
    return this.http.get<{
      items: OrphanPaymentItemDto[];
      totalCount: number;
    }>(`${this.apiUrl}/${groupId}/orphans`, {
      headers: this.getHeaders(),
      params: { page, pageSize }
    }).pipe(
      catchError(this.handleError)
    );
  }

  // ==================== EXCHANGE RATE MANAGEMENT ====================

  /**
   * Update exchange rate for a payment group
   */
  updateExchangeRate(groupId: string, exchangeRate: number, currency: string, dontRemoveRate?: boolean): Observable<void> {
    return this.http.patch<void>(`${this.apiUrl}/${groupId}/exchange-rate`, {
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
    return this.http.patch<void>(`${this.apiUrl}/${groupId}/lock-exchange-rate`, {}, {
      headers: this.getHeaders()
    }).pipe(
      catchError(this.handleError)
    );
  }

  /**
   * Unlock exchange rate
   */
  unlockExchangeRate(groupId: string): Observable<void> {
    return this.http.patch<void>(`${this.apiUrl}/${groupId}/unlock-exchange-rate`, {}, {
      headers: this.getHeaders()
    }).pipe(
      catchError(this.handleError)
    );
  }

  // ==================== BATCH UPLOAD STATUS ====================

  /**
   * Mark group as uploaded/ready for processing
   */
  markAsUploaded(groupId: string): Observable<void> {
    return this.http.patch<void>(`${this.apiUrl}/${groupId}/mark-uploaded`, {}, {
      headers: this.getHeaders()
    }).pipe(
      catchError(this.handleError)
    );
  }

  /**
   * Unmark group as uploaded (allow modifications again)
   */
  unmarkAsUploaded(groupId: string): Observable<void> {
    return this.http.patch<void>(`${this.apiUrl}/${groupId}/unmark-uploaded`, {}, {
      headers: this.getHeaders()
    }).pipe(
      catchError(this.handleError)
    );
  }

  // ==================== BATCH NUMBER MANAGEMENT ====================

  /**
   * Generate next batch number
   */
  generateNextBatchNumber(): Observable<BatchNumberGeneration> {
    return this.http.get<BatchNumberGeneration>(`${this.apiUrl}/batch-number/next`, {
      headers: this.getHeaders()
    }).pipe(
      catchError(this.handleError)
    );
  }

  /**
   * Update batch number manually
   */
  updateBatchNumber(groupId: string, batchNo: string): Observable<void> {
    return this.http.patch<void>(`${this.apiUrl}/${groupId}/batch-number`, { batchNo }, {
      headers: this.getHeaders()
    }).pipe(
      catchError(this.handleError)
    );
  }

  // ==================== EXPORT ====================

  /**
   * Export payment group to Excel or PDF
   */
  exportGroup(groupId: string, options: ExportPaymentGroupOptions): Observable<Blob> {
    return this.http.post(`${this.apiUrl}/${groupId}/export`, options, {
      headers: this.getHeaders(),
      responseType: 'blob'
    }).pipe(
      catchError(this.handleError)
    );
  }

  /**
   * Export payment groups list to Excel
   */
  exportGroupsList(searchRequest: OrphanPaymentSearchRequest): Observable<Blob> {
    return this.http.post(`${this.apiUrl}/export-list`, searchRequest, {
      headers: this.getHeaders(),
      responseType: 'blob'
    }).pipe(
      catchError(this.handleError)
    );
  }

  /**
   * Print payment group (returns printable HTML)
   */
  printGroup(groupId: string): Observable<string> {
    return this.http.get(`${this.apiUrl}/${groupId}/print`, {
      headers: this.getHeaders(),
      responseType: 'text'
    }).pipe(
      catchError(this.handleError)
    );
  }

  // ==================== STATISTICS ====================

  /**
   * Get orphan payment statistics
   */
  getStatistics(): Observable<OrphanPaymentStatistics> {
    return this.http.get<OrphanPaymentStatistics>(`${this.apiUrl}/statistics`, {
      headers: this.getHeaders()
    }).pipe(
      catchError(this.handleError)
    );
  }

  /**
   * Get group statistics by charity/region breakdown
   */
  getGroupStatistics(groupId: string): Observable<{
    byCharity: { charityId: number; charityName: string; orphanCount: number }[];
    byRegion: { regionId: number; regionName: string; orphanCount: number }[];
    totalOrphans: number;
  }> {
    return this.http.get<{
      byCharity: { charityId: number; charityName: string; orphanCount: number }[];
      byRegion: { regionId: number; regionName: string; orphanCount: number }[];
      totalOrphans: number;
    }>(`${this.apiUrl}/${groupId}/statistics`, {
      headers: this.getHeaders()
    }).pipe(
      catchError(this.handleError)
    );
  }

  // ==================== AUDIT LOG ====================

  /**
   * Get audit log for a payment group
   */
  getAuditLogs(groupId: string, page: number = 1, pageSize: number = 20): Observable<OrphanPaymentAuditLog[]> {
    return this.http.get<OrphanPaymentAuditLog[]>(`${this.apiUrl}/${groupId}/audit-logs`, {
      headers: this.getHeaders(),
      params: { page, pageSize }
    }).pipe(
      catchError(this.handleError)
    );
  }

  // ==================== FILTER OPTIONS ====================

  /**
   * Get available charities for filter dropdown
   */
  getAvailableCharities(): Observable<{ id: number; name: string }[]> {
    return this.http.get<{ id: number; name: string }[]>(`${this.apiUrl}/filter-options/charities`, {
      headers: this.getHeaders()
    }).pipe(
      catchError(this.handleError)
    );
  }

  /**
   * Get available regions for filter dropdown
   */
  getAvailableRegions(): Observable<{ id: number; name: string }[]> {
    return this.http.get<{ id: number; name: string }[]>(`${this.apiUrl}/filter-options/regions`, {
      headers: this.getHeaders()
    }).pipe(
      catchError(this.handleError)
    );
  }

  /**
   * Get available centers for filter dropdown
   */
  getAvailableCenters(): Observable<{ id: number; name: string }[]> {
    return this.http.get<{ id: number; name: string }[]>(`${this.apiUrl}/filter-options/centers`, {
      headers: this.getHeaders()
    }).pipe(
      catchError(this.handleError)
    );
  }

  // ==================== VALIDATION ====================

  /**
   * Check if batch number is unique
   */
  validateBatchNumber(batchNo: string, excludeId?: string): Observable<boolean> {
    return this.http.get<boolean>(`${this.apiUrl}/validate-batch-number`, {
      headers: this.getHeaders(),
      params: { batchNo, excludeId: excludeId || '' }
    }).pipe(
      catchError(this.handleError)
    );
  }

  /**
   * Check if group can be modified (not uploaded/locked)
   */
  canModifyGroup(groupId: string): Observable<{ canModify: boolean; reason?: string }> {
    return this.http.get<{ canModify: boolean; reason?: string }>(`${this.apiUrl}/${groupId}/can-modify`, {
      headers: this.getHeaders()
    }).pipe(
      catchError(this.handleError)
    );
  }

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
