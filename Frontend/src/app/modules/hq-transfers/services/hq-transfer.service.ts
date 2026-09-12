/**
 * HQ Financial Transfer Service
 * Handles all HQ transfer API calls (epic 17, UC-TRF-01…08)
 * The API returns raw DTOs — no {success, data} envelope.
 */

import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';

import { HqTransfer, HqTransferListResponse, HqTransferDetail, CreateHqTransferRequest, UpdateHqTransferRequest, CountryMaxTransferAmount, UpdateCountryMaxTransferRequest, HqTransferDetails, HqTransferDetailLine, SaveHqTransferDetailLineRequest, HqTransferStatistics } from '../models/hq-transfer.model';

/**
 * Paged response wrapper as the API serializes it
 */
interface PagedApiResponse<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

@Injectable({
  providedIn: 'root'
})
export class HqTransferService {
  private readonly apiBaseUrl = '/api/hqtransfers';

  constructor(private http: HttpClient) {}

  /**
   * The §22.U.1 register read (UC-TRF-01) — scoped server-side to the caller's country claim
   */
  getTransfers(page: number, pageSize: number): Observable<HqTransferListResponse> {
    let params = new HttpParams()
      .set('page', page.toString())
      .set('pageSize', pageSize.toString());

    return this.http.get<PagedApiResponse<HqTransfer>>(this.apiBaseUrl, { params })
      .pipe(
        map(response => ({
          items: response.items || [],
          totalCount: response.totalCount || 0,
          page: response.page || page,
          pageSize: response.pageSize || pageSize,
          totalPages: response.totalPages || 0
        }))
      );
  }

  /**
   * Export the §22.S.1 register to Excel — every row in the caller's country scope
   * (the list read's rules; paging is ignored server-side)
   */
  exportToExcel(): Observable<Blob> {
    return this.http.get(`${this.apiBaseUrl}/export`, { responseType: 'blob' });
  }

  /**
   * Register statistics for the band above the §22.S.1 grid — the list read's country
   * scope (the caller's country claim), one grouped round-trip server-side.
   */
  getStatistics(): Observable<HqTransferStatistics> {
    return this.http.get<HqTransferStatistics>(`${this.apiBaseUrl}/statistics`);
  }

  /**
   * UC-TRF-02: file a new HQ transfer. 201 + detail echo on success; 400 with
   * { message, errors } when FluentValidation or the lookup checks reject it.
   */
  createTransfer(request: CreateHqTransferRequest): Observable<HqTransferDetail> {
    return this.http.post<HqTransferDetail>(this.apiBaseUrl, request);
  }

  /**
   * UC-TRF-03: single-record read. 404 { message } covers absent, soft-deleted and
   * out-of-scope records alike — existence is never confirmed across the country boundary.
   */
  getTransferById(id: string): Observable<HqTransferDetail> {
    return this.http.get<HqTransferDetail>(`${this.apiBaseUrl}/${id}`);
  }

  /**
   * UC-TRF-04: update a transfer — id in the body (board contract). 200 + updated echo;
   * 400 errors map / 404 / 401 as the create path.
   */
  updateTransfer(request: UpdateHqTransferRequest): Observable<HqTransferDetail> {
    return this.http.put<HqTransferDetail>(this.apiBaseUrl, request);
  }

  /**
   * UC-TRF-06: the per-country transfer ceiling. maxTransferAmount null = no limit
   * configured (unlimited) — a legal state, not an error. Unknown country → 404.
   */
  getMaxTransferAmount(countryId: number): Observable<CountryMaxTransferAmount> {
    const params = new HttpParams().set('countryId', countryId.toString());
    return this.http.get<CountryMaxTransferAmount>(`${this.apiBaseUrl}/max-amount`, { params });
  }

  /**
   * UC-TRF-07: set a country's ceiling — SuperAdmin only (403 otherwise). null clears the
   * ceiling (unlimited); 0/negative → 400 with the errors map.
   */
  updateMaxTransferAmount(request: UpdateCountryMaxTransferRequest): Observable<CountryMaxTransferAmount> {
    return this.http.put<CountryMaxTransferAmount>(`${this.apiBaseUrl}/max-amount`, request);
  }

  /**
   * UC-TRF-08: the §22.S.3 screen read — header summary + allocation lines. 404 covers
   * absent, soft-deleted and out-of-scope transfers alike.
   */
  getTransferDetails(id: string): Observable<HqTransferDetails> {
    return this.http.get<HqTransferDetails>(`${this.apiBaseUrl}/${id}/details`);
  }

  /**
   * UC-TRF-08: save one allocation line (per-row «حفظ»). id null = add. The «Failed
   * Operation» state gating arrives as the 400 errors map; the sum rule as 400 { message }.
   */
  saveTransferDetailLine(transferId: string, request: SaveHqTransferDetailLineRequest): Observable<HqTransferDetailLine> {
    return this.http.put<HqTransferDetailLine>(`${this.apiBaseUrl}/${transferId}/details`, request);
  }
}
