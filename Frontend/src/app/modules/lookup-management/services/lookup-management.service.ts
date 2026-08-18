import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError, map, tap } from 'rxjs/operators';
import { environment } from '../../../../environments/environment';
import {
  CountryDto,
  CreateCountryDto,
  UpdateCountryDto,
  RegionDto,
  CreateRegionDto,
  UpdateRegionDto,
  CenterDto,
  CreateCenterDto,
  UpdateCenterDto,
  DepartmentDto,
  CreateDepartmentDto,
  UpdateDepartmentDto,
  BankDto,
  CreateBankDto,
  UpdateBankDto,
  LookupFilterDto,
  LookupPagedResult,
  LookupTableSummaryDto,
  BulkExportDto,
  BulkImportDto,
  BulkImportResultDto
} from '../models/lookup.model';

@Injectable({
  providedIn: 'root'
})
export class LookupManagementService {
  private apiUrl = `${environment.apiUrl}/api/LookupManagement`;

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

  // ==================== TABLES OVERVIEW (UC-14.5) ====================

  getLookupTablesSummary(): Observable<LookupTableSummaryDto[]> {
    return this.http.get<LookupTableSummaryDto[]>(`${this.apiUrl}/tables/summary`, {
      headers: this.getHeaders()
    }).pipe(
      catchError(this.handleError)
    );
  }

  exportLookupTable(tableName: string, exportDto: BulkExportDto): Observable<Blob> {
    return this.http.get(`${this.apiUrl}/tables/${tableName}/export`, {
      headers: this.getHeaders(),
      params: this.buildHttpParams(exportDto),
      responseType: 'blob'
    }).pipe(
      catchError(this.handleError)
    );
  }

  importLookupTable(tableName: string, importDto: BulkImportDto): Observable<BulkImportResultDto> {
    return this.http.post<BulkImportResultDto>(`${this.apiUrl}/tables/${tableName}/import`, importDto, {
      headers: this.getHeaders()
    }).pipe(
      catchError(this.handleError)
    );
  }

  // ==================== COUNTRIES (UC-14.8) ====================

  getCountries(filter?: LookupFilterDto): Observable<LookupPagedResult<CountryDto>> {
    return this.http.get<LookupPagedResult<CountryDto>>(`${this.apiUrl}/countries`, {
      headers: this.getHeaders(),
      params: this.buildHttpParams(filter)
    }).pipe(
      catchError(this.handleError)
    );
  }

  getCountry(id: number): Observable<CountryDto> {
    return this.http.get<CountryDto>(`${this.apiUrl}/countries/${id}`, {
      headers: this.getHeaders()
    }).pipe(
      catchError(this.handleError)
    );
  }

  createCountry(country: CreateCountryDto): Observable<CountryDto> {
    return this.http.post<CountryDto>(`${this.apiUrl}/countries`, country, {
      headers: this.getHeaders()
    }).pipe(
      catchError(this.handleError)
    );
  }

  updateCountry(id: number, country: UpdateCountryDto): Observable<CountryDto> {
    return this.http.put<CountryDto>(`${this.apiUrl}/countries/${id}`, country, {
      headers: this.getHeaders()
    }).pipe(
      catchError(this.handleError)
    );
  }

  deleteCountry(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/countries/${id}`, {
      headers: this.getHeaders()
    }).pipe(
      catchError(this.handleError)
    );
  }

  activateCountry(id: number): Observable<void> {
    return this.http.patch<void>(`${this.apiUrl}/countries/${id}/activate`, {}, {
      headers: this.getHeaders()
    }).pipe(
      catchError(this.handleError)
    );
  }

  deactivateCountry(id: number): Observable<void> {
    return this.http.patch<void>(`${this.apiUrl}/countries/${id}/deactivate`, {}, {
      headers: this.getHeaders()
    }).pipe(
      catchError(this.handleError)
    );
  }

  updateCountrySortOrder(id: number, sortOrder: number): Observable<void> {
    return this.http.patch<void>(`${this.apiUrl}/countries/${id}/sortorder`, sortOrder, {
      headers: this.getHeaders()
    }).pipe(
      catchError(this.handleError)
    );
  }

  // ==================== REGIONS (UC-14.7) ====================

  getRegions(filter?: LookupFilterDto): Observable<LookupPagedResult<RegionDto>> {
    return this.http.get<LookupPagedResult<RegionDto>>(`${this.apiUrl}/regions`, {
      headers: this.getHeaders(),
      params: this.buildHttpParams(filter)
    }).pipe(
      catchError(this.handleError)
    );
  }

  getRegionsByCountry(countryId: number): Observable<RegionDto[]> {
    return this.http.get<RegionDto[]>(`${this.apiUrl}/regions/by-country/${countryId}`, {
      headers: this.getHeaders()
    }).pipe(
      catchError(this.handleError)
    );
  }

  getRegion(id: number): Observable<RegionDto> {
    return this.http.get<RegionDto>(`${this.apiUrl}/regions/${id}`, {
      headers: this.getHeaders()
    }).pipe(
      catchError(this.handleError)
    );
  }

  createRegion(region: CreateRegionDto): Observable<RegionDto> {
    return this.http.post<RegionDto>(`${this.apiUrl}/regions`, region, {
      headers: this.getHeaders()
    }).pipe(
      catchError(this.handleError)
    );
  }

  updateRegion(id: number, region: UpdateRegionDto): Observable<RegionDto> {
    return this.http.put<RegionDto>(`${this.apiUrl}/regions/${id}`, region, {
      headers: this.getHeaders()
    }).pipe(
      catchError(this.handleError)
    );
  }

  deleteRegion(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/regions/${id}`, {
      headers: this.getHeaders()
    }).pipe(
      catchError(this.handleError)
    );
  }

  // ==================== CENTERS (UC-14.6) ====================

  getCenters(filter?: LookupFilterDto): Observable<LookupPagedResult<CenterDto>> {
    return this.http.get<LookupPagedResult<CenterDto>>(`${this.apiUrl}/centers`, {
      headers: this.getHeaders(),
      params: this.buildHttpParams(filter)
    }).pipe(
      catchError(this.handleError)
    );
  }

  getCentersByRegion(regionId: number): Observable<CenterDto[]> {
    return this.http.get<CenterDto[]>(`${this.apiUrl}/centers/by-region/${regionId}`, {
      headers: this.getHeaders()
    }).pipe(
      catchError(this.handleError)
    );
  }

  getCenter(id: number): Observable<CenterDto> {
    return this.http.get<CenterDto>(`${this.apiUrl}/centers/${id}`, {
      headers: this.getHeaders()
    }).pipe(
      catchError(this.handleError)
    );
  }

  createCenter(center: CreateCenterDto): Observable<CenterDto> {
    return this.http.post<CenterDto>(`${this.apiUrl}/centers`, center, {
      headers: this.getHeaders()
    }).pipe(
      catchError(this.handleError)
    );
  }

  updateCenter(id: number, center: UpdateCenterDto): Observable<CenterDto> {
    return this.http.put<CenterDto>(`${this.apiUrl}/centers/${id}`, center, {
      headers: this.getHeaders()
    }).pipe(
      catchError(this.handleError)
    );
  }

  deleteCenter(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/centers/${id}`, {
      headers: this.getHeaders()
    }).pipe(
      catchError(this.handleError)
    );
  }

  // ==================== DEPARTMENTS (UC-14.9) ====================

  getDepartments(filter?: LookupFilterDto): Observable<LookupPagedResult<DepartmentDto>> {
    return this.http.get<LookupPagedResult<DepartmentDto>>(`${this.apiUrl}/departments`, {
      headers: this.getHeaders(),
      params: this.buildHttpParams(filter)
    }).pipe(
      catchError(this.handleError)
    );
  }

  getDepartment(id: number): Observable<DepartmentDto> {
    return this.http.get<DepartmentDto>(`${this.apiUrl}/departments/${id}`, {
      headers: this.getHeaders()
    }).pipe(
      catchError(this.handleError)
    );
  }

  createDepartment(department: CreateDepartmentDto): Observable<DepartmentDto> {
    return this.http.post<DepartmentDto>(`${this.apiUrl}/departments`, department, {
      headers: this.getHeaders()
    }).pipe(
      catchError(this.handleError)
    );
  }

  updateDepartment(id: number, department: UpdateDepartmentDto): Observable<DepartmentDto> {
    return this.http.put<DepartmentDto>(`${this.apiUrl}/departments/${id}`, department, {
      headers: this.getHeaders()
    }).pipe(
      catchError(this.handleError)
    );
  }

  deleteDepartment(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/departments/${id}`, {
      headers: this.getHeaders()
    }).pipe(
      catchError(this.handleError)
    );
  }

  // ==================== BANKS ====================

  getBanks(filter?: LookupFilterDto): Observable<LookupPagedResult<BankDto>> {
    return this.http.get<LookupPagedResult<BankDto>>(`${this.apiUrl}/banks`, {
      headers: this.getHeaders(),
      params: this.buildHttpParams(filter)
    }).pipe(
      catchError(this.handleError)
    );
  }

  getBank(id: number): Observable<BankDto> {
    return this.http.get<BankDto>(`${this.apiUrl}/banks/${id}`, {
      headers: this.getHeaders()
    }).pipe(
      catchError(this.handleError)
    );
  }

  createBank(bank: CreateBankDto): Observable<BankDto> {
    return this.http.post<BankDto>(`${this.apiUrl}/banks`, bank, {
      headers: this.getHeaders()
    }).pipe(
      catchError(this.handleError)
    );
  }

  updateBank(id: number, bank: UpdateBankDto): Observable<BankDto> {
    return this.http.put<BankDto>(`${this.apiUrl}/banks/${id}`, bank, {
      headers: this.getHeaders()
    }).pipe(
      catchError(this.handleError)
    );
  }

  deleteBank(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/banks/${id}`, {
      headers: this.getHeaders()
    }).pipe(
      catchError(this.handleError)
    );
  }

  activateBank(id: number): Observable<void> {
    return this.http.patch<void>(`${this.apiUrl}/banks/${id}/activate`, {}, {
      headers: this.getHeaders()
    }).pipe(
      catchError(this.handleError)
    );
  }

  deactivateBank(id: number): Observable<void> {
    return this.http.patch<void>(`${this.apiUrl}/banks/${id}/deactivate`, {}, {
      headers: this.getHeaders()
    }).pipe(
      catchError(this.handleError)
    );
  }

  // ==================== ERROR HANDLING ====================

  private handleError(error: any): Observable<never> {
    console.error('An error occurred:', error);
    return throwError(() => {
      return {
        message: error.error?.message || 'An unexpected error occurred',
        status: error.status || 500
      };
    });
  }
}
