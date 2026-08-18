import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { environment } from '../../../../environments/environment';
import {
  ImportRequest,
  ValidationResult,
  ImportResult,
  ImportHistoryItem,
  ExportRequest,
  ExportHistoryItem
} from '../models/import-export.model';

@Injectable({
  providedIn: 'root'
})
export class ImportExportService {
  private apiUrl = `${environment.apiUrl}/api/ImportExport`;

  constructor(private http: HttpClient) {}

  private getHeaders(): HttpHeaders {
    // Let the AuthInterceptor handle the Authorization header
    return new HttpHeaders();
  }

  private getFormDataHeaders(): HttpHeaders {
    // Let the AuthInterceptor handle the Authorization header
    // Don't set Content-Type for FormData - let the browser set it with boundary
    return new HttpHeaders();
  }

  validateImport(file: File, fileType: 'Incoming' | 'Outgoing'): Observable<ValidationResult> {
    const formData = new FormData();
    formData.append('file', file);
    formData.append('fileType', fileType);

    return this.http.post<ValidationResult>(`${this.apiUrl}/validate`, formData, {
      headers: this.getFormDataHeaders()
    }).pipe(
      catchError(this.handleError)
    );
  }

  importLetters(importRequest: ImportRequest): Observable<ImportResult> {
    const formData = new FormData();
    formData.append('file', importRequest.file);
    formData.append('fileType', importRequest.fileType);
    formData.append('mappings', JSON.stringify(importRequest.mappings));
    formData.append('options', JSON.stringify(importRequest.options));

    return this.http.post<ImportResult>(`${this.apiUrl}/import`, formData, {
      headers: this.getFormDataHeaders()
    }).pipe(
      catchError(this.handleError)
    );
  }

  getImportHistory(pageNumber: number = 1, pageSize: number = 20): Observable<ImportHistoryItem[]> {
    return this.http.get<ImportHistoryItem[]>(`${this.apiUrl}/import-history`, {
      headers: this.getHeaders(),
      params: { pageNumber, pageSize }
    }).pipe(
      catchError(this.handleError)
    );
  }

  rollbackImport(importId: string): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/rollback/${importId}`, {}, {
      headers: this.getHeaders()
    }).pipe(
      catchError(this.handleError)
    );
  }

  exportLetters(exportRequest: ExportRequest): Observable<Blob> {
    return this.http.post(`${this.apiUrl}/export`, exportRequest, {
      headers: this.getHeaders(),
      responseType: 'blob'
    }).pipe(
      catchError(this.handleError)
    );
  }

  getExportHistory(pageNumber: number = 1, pageSize: number = 20): Observable<ExportHistoryItem[]> {
    return this.http.get<ExportHistoryItem[]>(`${this.apiUrl}/export-history`, {
      headers: this.getHeaders(),
      params: { pageNumber, pageSize }
    }).pipe(
      catchError(this.handleError)
    );
  }

  downloadTemplate(fileType: 'Incoming' | 'Outgoing'): Observable<Blob> {
    return this.http.get(`${this.apiUrl}/template/${fileType}`, {
      headers: this.getHeaders(),
      responseType: 'blob'
    }).pipe(
      catchError(this.handleError)
    );
  }

  private handleError(error: any): Observable<never> {
    console.error('Import/Export service error:', error);
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
