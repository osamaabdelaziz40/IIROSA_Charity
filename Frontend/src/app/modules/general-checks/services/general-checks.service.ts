import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';

import {
  CheckListItem,
  CheckDetail,
  CreateCheckRequest,
  UpdateCheckRequest,
  CheckFilter,
  CheckPagedResult,
  AmountInWordsResponse,
  CheckStatement,
  ChequeBeneficiaryOption,
  CurrencyOption,
  BankChequePositions
} from '../models/check.model';

/**
 * Client for /api/CheckManagement (chapter 16, UC-CHQ-01..10) plus the three
 * cheque-support lookups the backend serves from /api/LookupManagement.
 * Filter objects map 1:1 onto the backend query DTOs — camelCase on the wire.
 */
@Injectable({
  providedIn: 'root'
})
export class GeneralChecksService {
  private apiUrl = `${environment.apiUrl}/api/CheckManagement`;
  private lookupUrl = `${environment.apiUrl}/api/LookupManagement`;

  constructor(private http: HttpClient) {}

  // Register (UC-CHQ-01) and single record (UC-CHQ-03)

  getChecks(filter?: Partial<CheckFilter>): Observable<CheckPagedResult> {
    return this.http.get<CheckPagedResult>(this.apiUrl, { params: this.buildParams(filter) });
  }

  getCheckById(id: string): Observable<CheckDetail> {
    return this.http.get<CheckDetail>(`${this.apiUrl}/${id}`);
  }

  // Register export — same filters as the list, every row, as a workbook blob

  exportToExcel(filter?: Partial<CheckFilter>): Observable<Blob> {
    return this.http.get(`${this.apiUrl}/export`, {
      params: this.buildParams(filter),
      responseType: 'blob'
    });
  }

  // Issue / update (UC-CHQ-02, UC-CHQ-04) — update carries the id in the body

  createCheck(check: CreateCheckRequest): Observable<CheckDetail> {
    return this.http.post<CheckDetail>(this.apiUrl, check);
  }

  updateCheck(check: UpdateCheckRequest): Observable<CheckDetail> {
    return this.http.put<CheckDetail>(this.apiUrl, check);
  }

  // تفقيط (UC-CHQ-07)

  getAmountInWords(amount: number, currency: string): Observable<AmountInWordsResponse> {
    const params = new HttpParams().set('amount', amount).set('currency', currency);
    return this.http.get<AmountInWordsResponse>(`${this.apiUrl}/amount-in-words`, { params });
  }

  // Statement بيان الشيكات (UC-CHQ-09)

  getStatement(filter?: Partial<CheckFilter>): Observable<CheckStatement> {
    return this.http.get<CheckStatement>(`${this.apiUrl}/report`, { params: this.buildParams(filter) });
  }

  // Cheque-support lookups (UC-CHQ-05..08)

  getChequeBeneficiaries(term?: string, take: number = 20): Observable<ChequeBeneficiaryOption[]> {
    let params = new HttpParams().set('take', take);
    if (term) {
      params = params.set('term', term);
    }
    return this.http.get<ChequeBeneficiaryOption[]>(`${this.lookupUrl}/cheque-beneficiaries`, { params });
  }

  getCurrencies(): Observable<CurrencyOption[]> {
    return this.http.get<CurrencyOption[]>(`${this.lookupUrl}/currencies`);
  }

  getBankChequePositions(bankId: number): Observable<BankChequePositions> {
    return this.http.get<BankChequePositions>(`${this.lookupUrl}/banks/${bankId}/cheque-positions`);
  }

  private buildParams(params: any): HttpParams {
    let httpParams = new HttpParams();
    if (params) {
      Object.keys(params).forEach(key => {
        if (params[key] !== null && params[key] !== undefined && params[key] !== '') {
          httpParams = httpParams.set(key, params[key]);
        }
      });
    }
    return httpParams;
  }
}
