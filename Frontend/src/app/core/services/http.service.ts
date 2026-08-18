import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders, HttpParams, HttpErrorResponse } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError, map } from 'rxjs/operators';
import { AuthService } from './auth.service';

@Injectable({
  providedIn: 'root'
})
export class HttpService {
  private baseUrl = 'https://localhost:60960/api';

  constructor(private http: HttpClient, private authService: AuthService) { }

  private getHeaders(): HttpHeaders {
    const token = this.authService.getAccessToken();
    let headers = new HttpHeaders({
      'Content-Type': 'application/json'
    });

    if (token) {
      headers = headers.set('Authorization', `Bearer ${token}`);
    }

    return headers;
  }

  get<T>(url: string, params?: HttpParams): Observable<T> {
    return this.http.get<T>(`${this.baseUrl}${url}`, {
      headers: this.getHeaders(),
      params
    }).pipe(catchError(this.handleError));
  }

  post<T>(url: string, body: any): Observable<T> {
    return this.http.post<T>(`${this.baseUrl}${url}`, body, {
      headers: this.getHeaders()
    }).pipe(catchError(this.handleError));
  }

  put<T>(url: string, body: any): Observable<T> {
    return this.http.put<T>(`${this.baseUrl}${url}`, body, {
      headers: this.getHeaders()
    }).pipe(catchError(this.handleError));
  }

  delete<T>(url: string): Observable<T> {
    return this.http.delete<T>(`${this.baseUrl}${url}`, {
      headers: this.getHeaders()
    }).pipe(catchError(this.handleError));
  }

  private handleError(error: HttpErrorResponse) {
    if (error.status === 401) {
      // Unauthorized - redirect to login
      this.authService.logout().subscribe({
        next: () => {
          window.location.href = '/auth/login';
        },
        error: () => {
          // Navigate anyway even if logout fails
          window.location.href = '/auth/login';
        }
      });
    }

    let errorMessage = 'An error occurred';
    if (error.error instanceof ErrorEvent) {
      // Client-side error
      errorMessage = error.error.message;
    } else if (error.error && error.error.message) {
      // Server-side error with message
      errorMessage = error.error.message;
    }

    return throwError(() => errorMessage);
  }
}
