import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { environment } from '../../../../environments/environment';
import {
  OutgoingDto,
  CreateOutgoingDto,
  UpdateOutgoingDto,
  OutgoingSearchRequest,
  OutgoingPagedResult
} from '../models/outgoing.model';

@Injectable({
  providedIn: 'root'
})
export class OutgoingService {
  private apiUrl = `${environment.apiUrl}/api/Outgoing`;

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

  getOutgoingLetters(searchRequest: OutgoingSearchRequest): Observable<OutgoingPagedResult> {
    return this.http.get<OutgoingPagedResult>(`${this.apiUrl}`, {
      headers: this.getHeaders(),
      params: this.buildHttpParams(searchRequest)
    }).pipe(
      catchError(this.handleError)
    );
  }

  getOutgoingLetter(id: string): Observable<OutgoingDto> {
    return this.http.get<OutgoingDto>(`${this.apiUrl}/${id}`, {
      headers: this.getHeaders()
    }).pipe(
      catchError(this.handleError)
    );
  }

  createOutgoingLetter(letter: CreateOutgoingDto): Observable<OutgoingDto> {
    return this.http.post<OutgoingDto>(`${this.apiUrl}`, letter, {
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

  deleteOutgoingLetter(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`, {
      headers: this.getHeaders()
    }).pipe(
      catchError(this.handleError)
    );
  }

  exportToExcel(searchRequest: OutgoingSearchRequest): Observable<Blob> {
    return this.http.post(`${this.apiUrl}/export`, searchRequest, {
      headers: this.getHeaders(),
      responseType: 'blob'
    }).pipe(
      catchError(this.handleError)
    );
  }

  exportToPDF(searchRequest: OutgoingSearchRequest): Observable<Blob> {
    return this.http.post(`${this.apiUrl}/export/pdf`, searchRequest, {
      headers: this.getHeaders(),
      responseType: 'blob'
    }).pipe(
      catchError(this.handleError)
    );
  }

  downloadTemplate(): Observable<Blob> {
    return this.http.get(`${this.apiUrl}/template`, {
      headers: this.getHeaders(),
      responseType: 'blob'
    }).pipe(
      catchError(this.handleError)
    );
  }

  importLetters(file: File, options: any): Observable<any> {
    const formData = new FormData();
    formData.append('file', file);
    formData.append('options', JSON.stringify(options));

    return this.http.post(`${this.apiUrl}/import`, formData, {
      headers: this.getHeaders().delete('Content-Type'),
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
