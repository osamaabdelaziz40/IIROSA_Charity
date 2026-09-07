import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { environment } from '../../../../environments/environment';
import {
  OutgoingDto,
  CreateOutgoingDto,
  UpdateOutgoingDto,
  OutgoingFilterDto,
  OutgoingPagedResult,
  OutgoingCategoryOptionDto,
  OutgoingOrphansDto,
  OutgoingOrphanReportFilterDto,
  OutgoingOrphanReportResult
} from '../models/outgoing.model';
import { NextSerialDto } from '../models/incoming.model';

@Injectable({
  providedIn: 'root'
})
export class OutgoingService {
  private apiUrl = `${environment.apiUrl}/api/IncomingOutgoing/outgoing`;

  constructor(private http: HttpClient) {}

  private getHeaders(): HttpHeaders {
    // Let the AuthInterceptor handle the Authorization header
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

  // ========== UC-COR-10 / UC-COR-11 — the §21.S.4 register ==========

  getOutgoingLetters(filter: OutgoingFilterDto): Observable<OutgoingPagedResult> {
    return this.http.get<OutgoingPagedResult>(this.apiUrl, {
      headers: this.getHeaders(),
      params: this.buildHttpParams(filter)
    }).pipe(
      catchError(this.handleError)
    );
  }

  // ========== UC-COR-14 — view ==========

  getOutgoingLetter(id: string): Observable<OutgoingDto> {
    return this.http.get<OutgoingDto>(`${this.apiUrl}/${id}`, {
      headers: this.getHeaders()
    }).pipe(
      catchError(this.handleError)
    );
  }

  // ========== UC-COR-13 / UC-COR-15 — register / update ==========

  createOutgoingLetter(letter: CreateOutgoingDto): Observable<OutgoingDto> {
    return this.http.post<OutgoingDto>(this.apiUrl, letter, {
      headers: this.getHeaders()
    }).pipe(
      catchError(this.handleError)
    );
  }

  updateOutgoingLetter(id: string, letter: UpdateOutgoingDto): Observable<OutgoingDto> {
    return this.http.put<OutgoingDto>(`${this.apiUrl}/${id}`, letter, {
      headers: this.getHeaders()
    }).pipe(
      catchError(this.handleError)
    );
  }

  // ========== UC-COR-16 — delete ==========

  deleteOutgoingLetter(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`, {
      headers: this.getHeaders()
    }).pipe(
      catchError(this.handleError)
    );
  }

  // ========== UC-COR-12 — the advisory next serial ==========

  getNextSerial(year?: number, charityId?: string): Observable<NextSerialDto> {
    return this.http.get<NextSerialDto>(`${this.apiUrl}/next-serial`, {
      headers: this.getHeaders(),
      params: this.buildHttpParams({ year, charityId })
    }).pipe(
      catchError(this.handleError)
    );
  }

  // ========== UC-COR-17 — the §21.S.5 category options ==========

  getAvailableCategories(): Observable<OutgoingCategoryOptionDto[]> {
    return this.http.get<OutgoingCategoryOptionDto[]>(`${this.apiUrl}/categories`, {
      headers: this.getHeaders()
    }).pipe(
      catchError(this.handleError)
    );
  }

  // ========== UC-COR-18 — the §21.S.6 orphan report attachment ==========

  getOrphans(outgoingId: string): Observable<OutgoingOrphansDto> {
    return this.http.get<OutgoingOrphansDto>(`${this.apiUrl}/${outgoingId}/orphans`, {
      headers: this.getHeaders()
    }).pipe(
      catchError(this.handleError)
    );
  }

  attachOrphan(outgoingId: string, orphanId: string): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/${outgoingId}/orphans`, { orphanId }, {
      headers: this.getHeaders()
    }).pipe(
      catchError(this.handleError)
    );
  }

  detachOrphan(outgoingId: string, orphanId: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${outgoingId}/orphans/${orphanId}`, {
      headers: this.getHeaders()
    }).pipe(
      catchError(this.handleError)
    );
  }

  // ========== UC-COR-19 — the §21.S.7 orphans-by-letter report ==========

  getOrphanReport(filter: OutgoingOrphanReportFilterDto): Observable<OutgoingOrphanReportResult> {
    return this.http.get<OutgoingOrphanReportResult>(`${this.apiUrl}/reports/by-orphans`, {
      headers: this.getHeaders(),
      params: this.buildHttpParams(filter)
    }).pipe(
      catchError(this.handleError)
    );
  }

  private handleError(error: any): Observable<never> {
    console.error('Outgoing service error:', error);
    return throwError(() => {
      const errorMessage = error.error?.message || error.error?.title || 'An unexpected error occurred';
      return {
        message: errorMessage,
        status: error.status || 500,
        details: error.error?.errors || null
      };
    });
  }
}
