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
  CreateLookupDto,
  UpdateLookupDto,
  HousingBuildingDto,
  CreateHousingBuildingDto,
  UpdateHousingBuildingDto,
  HousingFlatDto,
  CreateHousingFlatDto,
  UpdateHousingFlatDto,
  LookupFilterDto,
  LookupPagedResult,
  LookupTableSummaryDto,
  BulkExportDto,
  BulkImportDto,
  BulkImportResultDto,
  LookupDto
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

  // ==================== OFFICE PROJECT TYPES (management) ====================

  getOfficeProjectTypesItems(filter?: LookupFilterDto): Observable<LookupPagedResult<LookupDto>> {
    return this.http.get<LookupPagedResult<LookupDto>>(`${this.apiUrl}/office-project-types/items`, {
      headers: this.getHeaders(),
      params: this.buildHttpParams(filter)
    }).pipe(
      catchError(this.handleError)
    );
  }

  getOfficeProjectType(id: number): Observable<LookupDto> {
    return this.http.get<LookupDto>(`${this.apiUrl}/office-project-types/${id}`, {
      headers: this.getHeaders()
    }).pipe(
      catchError(this.handleError)
    );
  }

  createOfficeProjectType(type: CreateLookupDto): Observable<LookupDto> {
    return this.http.post<LookupDto>(`${this.apiUrl}/office-project-types`, type, {
      headers: this.getHeaders()
    }).pipe(
      catchError(this.handleError)
    );
  }

  updateOfficeProjectType(id: number, type: UpdateLookupDto): Observable<LookupDto> {
    return this.http.put<LookupDto>(`${this.apiUrl}/office-project-types/${id}`, type, {
      headers: this.getHeaders()
    }).pipe(
      catchError(this.handleError)
    );
  }

  deleteOfficeProjectType(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/office-project-types/${id}`, {
      headers: this.getHeaders()
    }).pipe(
      catchError(this.handleError)
    );
  }

  activateOfficeProjectType(id: number): Observable<void> {
    return this.http.patch<void>(`${this.apiUrl}/office-project-types/${id}/activate`, {}, {
      headers: this.getHeaders()
    }).pipe(
      catchError(this.handleError)
    );
  }

  deactivateOfficeProjectType(id: number): Observable<void> {
    return this.http.patch<void>(`${this.apiUrl}/office-project-types/${id}/deactivate`, {}, {
      headers: this.getHeaders()
    }).pipe(
      catchError(this.handleError)
    );
  }

  // ==================== OUTGOING CATEGORIES (UC-COR-17 management) ====================

  /** Active-only catalogue (GET outgoing-categories) — feeds form drop-downs. */
  getOutgoingCategories(): Observable<LookupDto[]> {
    return this.http.get<LookupDto[]>(`${this.apiUrl}/outgoing-categories`, {
      headers: this.getHeaders()
    }).pipe(
      catchError(this.handleError)
    );
  }

  /** Management list (GET outgoing-categories/items) — paged, includes inactive rows. */
  getOutgoingCategoriesItems(filter?: LookupFilterDto): Observable<LookupPagedResult<LookupDto>> {
    return this.http.get<LookupPagedResult<LookupDto>>(`${this.apiUrl}/outgoing-categories/items`, {
      headers: this.getHeaders(),
      params: this.buildHttpParams(filter)
    }).pipe(
      catchError(this.handleError)
    );
  }

  getOutgoingCategory(id: number): Observable<LookupDto> {
    return this.http.get<LookupDto>(`${this.apiUrl}/outgoing-categories/${id}`, {
      headers: this.getHeaders()
    }).pipe(
      catchError(this.handleError)
    );
  }

  createOutgoingCategory(category: CreateLookupDto): Observable<LookupDto> {
    return this.http.post<LookupDto>(`${this.apiUrl}/outgoing-categories`, category, {
      headers: this.getHeaders()
    }).pipe(
      catchError(this.handleError)
    );
  }

  updateOutgoingCategory(id: number, category: UpdateLookupDto): Observable<LookupDto> {
    return this.http.put<LookupDto>(`${this.apiUrl}/outgoing-categories/${id}`, category, {
      headers: this.getHeaders()
    }).pipe(
      catchError(this.handleError)
    );
  }

  deleteOutgoingCategory(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/outgoing-categories/${id}`, {
      headers: this.getHeaders()
    }).pipe(
      catchError(this.handleError)
    );
  }

  activateOutgoingCategory(id: number): Observable<void> {
    return this.http.patch<void>(`${this.apiUrl}/outgoing-categories/${id}/activate`, {}, {
      headers: this.getHeaders()
    }).pipe(
      catchError(this.handleError)
    );
  }

  deactivateOutgoingCategory(id: number): Observable<void> {
    return this.http.patch<void>(`${this.apiUrl}/outgoing-categories/${id}/deactivate`, {}, {
      headers: this.getHeaders()
    }).pipe(
      catchError(this.handleError)
    );
  }

  // ==================== HOUSING BUILDINGS (management, UC-HOU-05) ====================

  getHousingBuildingsItems(filter?: LookupFilterDto): Observable<LookupPagedResult<HousingBuildingDto>> {
    return this.http.get<LookupPagedResult<HousingBuildingDto>>(`${this.apiUrl}/housing-buildings/items`, {
      headers: this.getHeaders(),
      params: this.buildHttpParams(filter)
    }).pipe(
      catchError(this.handleError)
    );
  }

  getHousingBuilding(id: number): Observable<HousingBuildingDto> {
    return this.http.get<HousingBuildingDto>(`${this.apiUrl}/housing-buildings/${id}`, {
      headers: this.getHeaders()
    }).pipe(
      catchError(this.handleError)
    );
  }

  createHousingBuilding(building: CreateHousingBuildingDto): Observable<HousingBuildingDto> {
    return this.http.post<HousingBuildingDto>(`${this.apiUrl}/housing-buildings`, building, {
      headers: this.getHeaders()
    }).pipe(
      catchError(this.handleError)
    );
  }

  updateHousingBuilding(id: number, building: UpdateHousingBuildingDto): Observable<HousingBuildingDto> {
    return this.http.put<HousingBuildingDto>(`${this.apiUrl}/housing-buildings/${id}`, building, {
      headers: this.getHeaders()
    }).pipe(
      catchError(this.handleError)
    );
  }

  deleteHousingBuilding(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/housing-buildings/${id}`, {
      headers: this.getHeaders()
    }).pipe(
      catchError(this.handleError)
    );
  }

  activateHousingBuilding(id: number): Observable<void> {
    return this.http.patch<void>(`${this.apiUrl}/housing-buildings/${id}/activate`, {}, {
      headers: this.getHeaders()
    }).pipe(
      catchError(this.handleError)
    );
  }

  deactivateHousingBuilding(id: number): Observable<void> {
    return this.http.patch<void>(`${this.apiUrl}/housing-buildings/${id}/deactivate`, {}, {
      headers: this.getHeaders()
    }).pipe(
      catchError(this.handleError)
    );
  }

  // ==================== HOUSING FLATS (management, UC-HOU-05) ====================

  /**
   * All flats of one building for the management screen — includes inactive rows,
   * unlike getHousingFlats which feeds the §11.S.2 drop-down (active only).
   */
  getHousingFlatItems(buildingId: number): Observable<HousingFlatDto[]> {
    let params = new HttpParams().set('buildingId', buildingId.toString());
    return this.http.get<HousingFlatDto[]>(`${this.apiUrl}/housing-flats/items`, {
      headers: this.getHeaders(),
      params
    }).pipe(
      catchError(this.handleError)
    );
  }

  getHousingFlat(id: number): Observable<HousingFlatDto> {
    return this.http.get<HousingFlatDto>(`${this.apiUrl}/housing-flats/${id}`, {
      headers: this.getHeaders()
    }).pipe(
      catchError(this.handleError)
    );
  }

  createHousingFlat(flat: CreateHousingFlatDto): Observable<HousingFlatDto> {
    return this.http.post<HousingFlatDto>(`${this.apiUrl}/housing-flats`, flat, {
      headers: this.getHeaders()
    }).pipe(
      catchError(this.handleError)
    );
  }

  updateHousingFlat(id: number, flat: UpdateHousingFlatDto): Observable<HousingFlatDto> {
    return this.http.put<HousingFlatDto>(`${this.apiUrl}/housing-flats/${id}`, flat, {
      headers: this.getHeaders()
    }).pipe(
      catchError(this.handleError)
    );
  }

  deleteHousingFlat(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/housing-flats/${id}`, {
      headers: this.getHeaders()
    }).pipe(
      catchError(this.handleError)
    );
  }

  activateHousingFlat(id: number): Observable<void> {
    return this.http.patch<void>(`${this.apiUrl}/housing-flats/${id}/activate`, {}, {
      headers: this.getHeaders()
    }).pipe(
      catchError(this.handleError)
    );
  }

  deactivateHousingFlat(id: number): Observable<void> {
    return this.http.patch<void>(`${this.apiUrl}/housing-flats/${id}/deactivate`, {}, {
      headers: this.getHeaders()
    }).pipe(
      catchError(this.handleError)
    );
  }

  // ==================== HOUSING CATALOGUE (UC-HOU-05) ====================

  /**
   * Housing buildings (UC-HOU-05) — active-only catalogue feeding the §11.S.2
   * رقم العماره drop-down on the housing family form. Organisation-owned rows
   * (HQ catalogue, no per-charity scoping).
   */
  getHousingBuildings(): Observable<LookupDto[]> {
    return this.http.get<LookupDto[]>(`${this.apiUrl}/housing-buildings`, {
      headers: this.getHeaders()
    }).pipe(
      catchError(this.handleError)
    );
  }

  /**
   * Flats of one building (UC-HOU-05) — the §11.S.2 رقم الشقه drop-down, repopulated
   * on building change. `buildingId` is required by the endpoint (400 when absent).
   */
  getHousingFlats(buildingId: number): Observable<LookupDto[]> {
    // UC-SYS-06 hygiene guard: a non-numeric/undefined id must fail through the
    // observable (catchError sees it), not as a synchronous NaN.toString() throw.
    if (!buildingId || !Number.isFinite(buildingId)) {
      return throwError(() => ({ message: 'buildingId is required', status: 400 }));
    }
    let params = new HttpParams().set('buildingId', buildingId.toString());
    return this.http.get<LookupDto[]>(`${this.apiUrl}/housing-flats`, {
      headers: this.getHeaders(),
      params
    }).pipe(
      catchError(this.handleError)
    );
  }

  // ==================== §11.S.2 FORM CATALOGUE (UC-HOU-03) ====================
  // Read-only getters feeding the housing-family register form drop-downs. The endpoints
  // (added for the epic-7 refugee form) return the bare active-item list — not the paged
  // CRUD surface above — so these mirror the getHousingBuildings shape: Observable<LookupDto[]>.

  /** المؤهل الدراسى — GET education-levels */
  getEducationLevels(): Observable<LookupDto[]> {
    return this.http.get<LookupDto[]>(`${this.apiUrl}/education-levels`, {
      headers: this.getHeaders()
    }).pipe(catchError(this.handleError));
  }

  /** الحالة الصحية — GET health-statuses */
  getHealthStatuses(): Observable<LookupDto[]> {
    return this.http.get<LookupDto[]>(`${this.apiUrl}/health-statuses`, {
      headers: this.getHeaders()
    }).pipe(catchError(this.handleError));
  }

  /** نوع الدخل — GET income-types */
  getIncomeTypes(): Observable<LookupDto[]> {
    return this.http.get<LookupDto[]>(`${this.apiUrl}/income-types`, {
      headers: this.getHeaders()
    }).pipe(catchError(this.handleError));
  }

  /** الحالة الاجتماعية — GET social-statuses */
  getSocialStatuses(): Observable<LookupDto[]> {
    return this.http.get<LookupDto[]>(`${this.apiUrl}/social-statuses`, {
      headers: this.getHeaders()
    }).pipe(catchError(this.handleError));
  }

  /** نوع العلاقة — GET relations */
  getRelations(): Observable<LookupDto[]> {
    return this.http.get<LookupDto[]>(`${this.apiUrl}/relations`, {
      headers: this.getHeaders()
    }).pipe(catchError(this.handleError));
  }

  /** سبب العلاقة — GET reasons-of-relation */
  getReasonsOfRelation(): Observable<LookupDto[]> {
    return this.http.get<LookupDto[]>(`${this.apiUrl}/reasons-of-relation`, {
      headers: this.getHeaders()
    }).pipe(catchError(this.handleError));
  }

  // ==================== §12.S.2 REFUGEE FORM CATALOGUE (UC-REF-03) ====================
  // Same bare active-item list shape as the §11.S.2 getters above.

  /** ملكية السكن — GET house-ownerships */
  getHouseOwnerships(): Observable<LookupDto[]> {
    return this.http.get<LookupDto[]>(`${this.apiUrl}/house-ownerships`, {
      headers: this.getHeaders()
    }).pipe(catchError(this.handleError));
  }

  /** حالة محتويات السكن — GET house-statuses */
  getHouseStatuses(): Observable<LookupDto[]> {
    return this.http.get<LookupDto[]>(`${this.apiUrl}/house-statuses`, {
      headers: this.getHeaders()
    }).pipe(catchError(this.handleError));
  }

  /** نوع السكن — shared catalogue (§12.S.2 refugee form) */
  getHousingTypes(): Observable<LookupDto[]> {
    return this.http.get<LookupDto[]>(`${this.apiUrl}/housing-types`, {
      headers: this.getHeaders()
    }).pipe(catchError(this.handleError));
  }

  // ==================== PERIODIC-REPORT REVIEW (UC-ORR-08 / UC-SYS-06) ====================
  // The mandatory refusal catalogue consumed by the 9-7/9-8 review flow — a Refused
  // decision requires one of these reasons (enforced server-side by the 9-8 validator).

  /** أسباب رفض التقرير الدوري — GET refuse-reasons (UC-SYS-06 refusal catalogue) */
  getRefuseReasons(): Observable<LookupDto[]> {
    return this.http.get<LookupDto[]>(`${this.apiUrl}/refuse-reasons`, {
      headers: this.getHeaders()
    }).pipe(catchError(this.handleError));
  }

  // ==================== GUARDIAN REFERENCE DATA (UC-SYS-05) ====================
  // Feeds the guardian (provider/parent) sections of the family forms — same bare
  // active-item list shape as the catalogue getters above.

  /** الحالة الاجتماعية للعائل — GET marital-statuses */
  getMaritalStatuses(): Observable<LookupDto[]> {
    return this.http.get<LookupDto[]>(`${this.apiUrl}/marital-statuses`, {
      headers: this.getHeaders()
    }).pipe(catchError(this.handleError));
  }

  /** مهنة العائل — GET jobs (UC-SYS-09 guardian job catalogue) */
  getJobs(): Observable<LookupDto[]> {
    return this.http.get<LookupDto[]>(`${this.apiUrl}/jobs`, {
      headers: this.getHeaders()
    }).pipe(catchError(this.handleError));
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
