import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { environment } from '../../../../environments/environment';
import {
  IncomingDto,
  CreateIncomingDto,
  UpdateIncomingDto,
  IncomingFilterDto,
  IncomingPagedResult,
  IncomingStatistics,
  CorrespondenceStatusOption,
  NextSerialDto,
  IncomingEmployeesDto
} from '../models/incoming.model';

@Injectable({
  providedIn: 'root'
})
export class IncomingService {
  private apiUrl = `${environment.apiUrl}/api/IncomingOutgoing/incoming`;

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

  // ========== UC-COR-01 / UC-COR-02 — the §21.S.1 register ==========

  getIncomingLetters(filter: IncomingFilterDto): Observable<IncomingPagedResult> {
    return this.http.get<IncomingPagedResult>(this.apiUrl, {
      headers: this.getHeaders(),
      params: this.buildHttpParams(filter)
    }).pipe(
      catchError(this.handleError)
    );
  }

  // ========== UC-COR-01 — the register statistics band ==========

  getStatistics(): Observable<IncomingStatistics> {
    return this.http.get<IncomingStatistics>(`${this.apiUrl}/statistics`, {
      headers: this.getHeaders()
    }).pipe(
      catchError(this.handleError)
    );
  }

  // ========== UC-COR-05 — view ==========

  getIncomingLetter(id: string): Observable<IncomingDto> {
    return this.http.get<IncomingDto>(`${this.apiUrl}/${id}`, {
      headers: this.getHeaders()
    }).pipe(
      catchError(this.handleError)
    );
  }

  // ========== UC-COR-04 / UC-COR-06 — register / update ==========

  createIncomingLetter(letter: CreateIncomingDto): Observable<IncomingDto> {
    return this.http.post<IncomingDto>(this.apiUrl, letter, {
      headers: this.getHeaders()
    }).pipe(
      catchError(this.handleError)
    );
  }

  updateIncomingLetter(id: string, letter: UpdateIncomingDto): Observable<IncomingDto> {
    return this.http.put<IncomingDto>(`${this.apiUrl}/${id}`, letter, {
      headers: this.getHeaders()
    }).pipe(
      catchError(this.handleError)
    );
  }

  // ========== UC-COR-07 — delete ==========

  deleteIncomingLetter(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`, {
      headers: this.getHeaders()
    }).pipe(
      catchError(this.handleError)
    );
  }

  // ========== UC-COR-03 — the advisory next serial ==========

  getNextSerial(year?: number, charityId?: string): Observable<NextSerialDto> {
    return this.http.get<NextSerialDto>(`${this.apiUrl}/next-serial`, {
      headers: this.getHeaders(),
      params: this.buildHttpParams({ year, charityId })
    }).pipe(
      catchError(this.handleError)
    );
  }

  // ========== The §21.S.1 tri-state status options ==========

  getAvailableStatuses(): Observable<CorrespondenceStatusOption[]> {
    return this.http.get<CorrespondenceStatusOption[]>(`${this.apiUrl}/statuses`, {
      headers: this.getHeaders()
    }).pipe(
      catchError(this.handleError)
    );
  }

  // ========== UC-COR-09 — the §21.S.3 employee attachment ==========

  getEmployees(incomingId: string): Observable<IncomingEmployeesDto> {
    return this.http.get<IncomingEmployeesDto>(`${this.apiUrl}/${incomingId}/employees`, {
      headers: this.getHeaders()
    }).pipe(
      catchError(this.handleError)
    );
  }

  attachEmployee(incomingId: string, userId: string): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/${incomingId}/employees`, { userId }, {
      headers: this.getHeaders()
    }).pipe(
      catchError(this.handleError)
    );
  }

  detachEmployee(incomingId: string, userId: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${incomingId}/employees/${userId}`, {
      headers: this.getHeaders()
    }).pipe(
      catchError(this.handleError)
    );
  }

  private handleError(error: any): Observable<never> {
    console.error('Incoming service error:', error);
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
