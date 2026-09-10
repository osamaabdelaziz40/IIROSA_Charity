import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders, HttpParams, HttpParameterCodec } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError, map } from 'rxjs/operators';
import { environment } from '../../../../environments/environment';
import {
  CharityDto,
  CreateCharityDto,
  UpdateCharityDto,
  CharitySearchRequest,
  CharityPagedResult,
  CharityAuditLog,
  PasswordResetDto,
  CharityStatusUpdateDto,
  CharityCredentialsDto,
  CharityNameAvailability,
  AttachmentDto,
  CharityStatistics
} from '../models/charity.model';

/**
 * Percent-encodes query parameters strictly, unlike Angular's default codec which un-escapes a
 * set of characters — including `+`, which a server reads as a space.
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
export class CharityService {
  private apiUrl = `${environment.apiUrl}/api/Charities`;

  constructor(private http: HttpClient) {}

  private getHeaders(): HttpHeaders {
    // Let the AuthInterceptor handle the Authorization header
    return new HttpHeaders({
      'Content-Type': 'application/json'
    });
  }

  /**
   * Convert frontend attachments to backend format
   * - Converts base64 fileData to byte array (represented as array of numbers for JSON)
   * - Converts temp IDs to empty GUIDs for new attachments
   * - Preserves existing GUIDs for saved attachments
   */
  private convertAttachmentsForBackend(attachments: AttachmentDto[] | undefined): any[] | undefined {
    if (!attachments || attachments.length === 0) {
      return undefined;
    }

    return attachments.map(attachment => {
      // For new attachments (isNew = true or id starts with "temp_")
      const isNewAttachment = attachment.isNew || (attachment.id && attachment.id.startsWith('temp_'));

      // Convert base64 to byte array if present
      let fileDataBytes: number[] | undefined;
      if (attachment.fileData) {
        fileDataBytes = this.base64ToByteArray(attachment.fileData);
      }

      return {
        id: isNewAttachment ? '00000000-0000-0000-0000-000000000000' : (attachment.id || '00000000-0000-0000-0000-000000000000'),
        fileName: attachment.fileName,
        contentType: attachment.contentType,
        fileData: fileDataBytes,
        filePath: attachment.filePath,
        description: attachment.description,
        extension: attachment.extension,
        isDeleted: attachment.isDeleted || false,
        isNew: attachment.isNew || false
      };
    });
  }

  /**
   * Convert base64 string to byte array (array of numbers for JSON serialization)
   */
  private base64ToByteArray(base64: string): number[] {
    // Remove data URL prefix if present (e.g., "data:image/png;base64,")
    const base64Data = base64.includes(',') ? base64.split(',')[1] : base64;

    // Decode base64 to binary string
    const binaryString = atob(base64Data);

    // Convert to byte array
    const bytes = new Uint8Array(binaryString.length);
    for (let i = 0; i < binaryString.length; i++) {
      bytes[i] = binaryString.charCodeAt(i);
    }

    // Return as regular array for JSON serialization
    return Array.from(bytes);
  }

  private buildHttpParams(filter: any): HttpParams {
    // Same strict codec as checkNameAvailability. With Angular's default codec a value containing
    // `+` reaches the server as a space, so the list search and the availability check would
    // disagree about whether the same charity name exists.
    let params = new HttpParams({ encoder: new StrictHttpParameterCodec() });
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

  getCharities(searchRequest: CharitySearchRequest): Observable<CharityPagedResult> {
    return this.http.get<CharityPagedResult>(`${this.apiUrl}`, {
      headers: this.getHeaders(),
      params: this.buildHttpParams(searchRequest)
    }).pipe(
      catchError(this.handleError)
    );
  }

  getCharity(id: string): Observable<CharityDto> {
    return this.http.get<CharityDto>(`${this.apiUrl}/${id}`, {
      headers: this.getHeaders()
    }).pipe(
      catchError(this.handleError)
    );
  }

  /**
   * Check whether a charity name is still free (UC-CHR-02).
   *
   * Uniqueness is register-wide, matching the rule the save enforces.
   *
   * @param name the name to check
   * @param excludeId the charity being edited; omit when creating. Without it, editing a charity
   *        without renaming it would report its own name as taken.
   */
  checkNameAvailability(name: string, excludeId?: string): Observable<CharityNameAvailability> {
    // Angular's default HttpParams codec re-writes %2B back to a literal '+', which ASP.NET Core
    // then decodes as a space — so "Al Noor + Partners" would be checked as "Al Noor   Partners".
    // Encoding the value ourselves and disabling further encoding keeps the two ends agreeing.
    let params = new HttpParams({ encoder: new StrictHttpParameterCodec() }).set('name', name);
    if (excludeId) {
      params = params.set('excludeId', excludeId);
    }

    return this.http.get<CharityNameAvailability>(`${this.apiUrl}/check-name`, {
      headers: this.getHeaders(),
      params
    }).pipe(
      catchError(this.handleError)
    );
  }

  getMyCharityProfile(): Observable<CharityDto> {
    return this.http.get<CharityDto>(`${this.apiUrl}/my-profile`, {
      headers: this.getHeaders()
    }).pipe(
      catchError(this.handleError)
    );
  }

  createCharity(charity: CreateCharityDto): Observable<CharityDto> {
    // Convert attachments to backend format
    const payload = {
      ...charity,
      icon_Attach: this.convertAttachmentsForBackend(charity.icon_Attach)
    };

    return this.http.post<CharityDto>(`${this.apiUrl}`, payload, {
      headers: this.getHeaders()
    }).pipe(
      catchError(this.handleError)
    );
  }

  updateCharity(id: string, charity: UpdateCharityDto): Observable<CharityDto> {
    // Convert attachments to backend format
    const payload = {
      ...charity,
      icon_Attach: this.convertAttachmentsForBackend(charity.icon_Attach)
    };

    return this.http.put<CharityDto>(`${this.apiUrl}/${id}`, payload, {
      headers: this.getHeaders()
    }).pipe(
      catchError(this.handleError)
    );
  }

  deleteCharity(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`, {
      headers: this.getHeaders()
    }).pipe(
      catchError(this.handleError)
    );
  }

  // ==================== STATUS MANAGEMENT ====================

  activateCharity(id: string): Observable<void> {
    return this.http.patch<void>(`${this.apiUrl}/${id}/activate`, {}, {
      headers: this.getHeaders()
    }).pipe(
      catchError(this.handleError)
    );
  }

  deactivateCharity(id: string): Observable<void> {
    return this.http.patch<void>(`${this.apiUrl}/${id}/deactivate`, {}, {
      headers: this.getHeaders()
    }).pipe(
      catchError(this.handleError)
    );
  }

  lockCharity(id: string): Observable<void> {
    return this.http.patch<void>(`${this.apiUrl}/${id}/lock`, {}, {
      headers: this.getHeaders()
    }).pipe(
      catchError(this.handleError)
    );
  }

  unlockCharity(id: string): Observable<void> {
    return this.http.patch<void>(`${this.apiUrl}/${id}/unlock`, {}, {
      headers: this.getHeaders()
    }).pipe(
      catchError(this.handleError)
    );
  }

  updateStatus(id: string, statusUpdate: CharityStatusUpdateDto): Observable<void> {
    return this.http.patch<void>(`${this.apiUrl}/${id}/status`, statusUpdate, {
      headers: this.getHeaders()
    }).pipe(
      catchError(this.handleError)
    );
  }

  // ==================== PASSWORD MANAGEMENT ====================

  resetPassword(passwordReset: PasswordResetDto): Observable<CharityCredentialsDto> {
    return this.http.post<CharityCredentialsDto>(`${this.apiUrl}/${passwordReset.charityId}/reset-password`, passwordReset, {
      headers: this.getHeaders()
    }).pipe(
      catchError(this.handleError)
    );
  }

  changeMyPassword(oldPassword: string, newPassword: string): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/change-password`, {
      oldPassword,
      newPassword
    }, {
      headers: this.getHeaders()
    }).pipe(
      catchError(this.handleError)
    );
  }

  // ==================== RIGHTS MANAGEMENT ====================

  enableAddRights(id: string): Observable<void> {
    return this.http.patch<void>(`${this.apiUrl}/${id}/enable-add`, {}, {
      headers: this.getHeaders()
    }).pipe(
      catchError(this.handleError)
    );
  }

  disableAddRights(id: string): Observable<void> {
    return this.http.patch<void>(`${this.apiUrl}/${id}/disable-add`, {}, {
      headers: this.getHeaders()
    }).pipe(
      catchError(this.handleError)
    );
  }

  enableUpdateRights(id: string): Observable<void> {
    return this.http.patch<void>(`${this.apiUrl}/${id}/enable-update`, {}, {
      headers: this.getHeaders()
    }).pipe(
      catchError(this.handleError)
    );
  }

  disableUpdateRights(id: string): Observable<void> {
    return this.http.patch<void>(`${this.apiUrl}/${id}/disable-update`, {}, {
      headers: this.getHeaders()
    }).pipe(
      catchError(this.handleError)
    );
  }

  // ==================== BANKING MANAGEMENT ====================

  updateBankingDetails(id: string, bankId: number, bankAccount: string, iban: string): Observable<void> {
    return this.http.patch<void>(`${this.apiUrl}/${id}/banking`, {
      bankId,
      bankAccount,
      iban
    }, {
      headers: this.getHeaders()
    }).pipe(
      catchError(this.handleError)
    );
  }

  // ==================== LOCATION MANAGEMENT ====================

  updateLocation(id: string, countryId: number, regionId: number, centerId: number): Observable<void> {
    return this.http.patch<void>(`${this.apiUrl}/${id}/location`, {
      countryId,
      regionId,
      centerId
    }, {
      headers: this.getHeaders()
    }).pipe(
      catchError(this.handleError)
    );
  }

  updateMapLocation(id: string, mapLocation: string): Observable<void> {
    return this.http.patch<void>(`${this.apiUrl}/${id}/map-location`, {
      ngoMapLocation: mapLocation
    }, {
      headers: this.getHeaders()
    }).pipe(
      catchError(this.handleError)
    );
  }

  // ==================== CONTACTS MANAGEMENT ====================

  updateManagementContacts(id: string, contacts: Partial<UpdateCharityDto>): Observable<void> {
    return this.http.patch<void>(`${this.apiUrl}/${id}/contacts`, contacts, {
      headers: this.getHeaders()
    }).pipe(
      catchError(this.handleError)
    );
  }

  // ==================== AUDIT LOG ====================

  getAuditLogs(id: string, page: number = 1, pageSize: number = 20): Observable<CharityAuditLog[]> {
    return this.http.get<CharityAuditLog[]>(`${this.apiUrl}/${id}/audit-logs`, {
      headers: this.getHeaders(),
      params: { page, pageSize }
    }).pipe(
      catchError(this.handleError)
    );
  }

  // ==================== EXPORT ====================

  exportToExcel(searchRequest: CharitySearchRequest): Observable<Blob> {
    return this.http.post(`${this.apiUrl}/export`, searchRequest, {
      headers: this.getHeaders(),
      responseType: 'blob'
    }).pipe(
      catchError(this.handleError)
    );
  }

  exportToPDF(id: string): Observable<Blob> {
    return this.http.get(`${this.apiUrl}/${id}/export/pdf`, {
      headers: this.getHeaders(),
      responseType: 'blob'
    }).pipe(
      catchError(this.handleError)
    );
  }

  // ==================== STATISTICS ====================

  /**
   * Register statistics for the band above the all-charities grid (UC-CHR-01).
   * Caller-scoped server-side — the counts always match what the list can show.
   */
  getStatistics(): Observable<CharityStatistics> {
    return this.http.get<CharityStatistics>(`${this.apiUrl}/statistics`, {
      headers: this.getHeaders()
    }).pipe(
      catchError(this.handleError)
    );
  }

  // ==================== ERROR HANDLING ====================

  private handleError(error: any): Observable<never> {
    console.error('Charity service error:', error);
    return throwError(() => {
      const errorMessage = error.error?.message || error.error?.title || 'An unexpected error occurred';
      return {
        message: errorMessage,
        status: error.status || 500,
        details: error.error?.errors || null,
        // Preserved under the original shape as well. Callers that map per-field errors onto form
        // controls read `error.error.errors`; rethrowing only `details` silently discarded them
        // and every server-side field error degraded to a generic toast.
        error: error.error
      };
    });
  }
}
