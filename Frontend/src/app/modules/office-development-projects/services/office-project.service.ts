import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError, map } from 'rxjs/operators';
import { environment } from '../../../../environments/environment';
import {
  OfficeProject,
  OfficeProjectDto,
  OfficeProjectFilter,
  OfficeProjectListItem,
  ProjectProgressStats,
  OfficeProjectStatusSummary,
  OfficeProjectPagedResult
} from '../models/office-project.model';

@Injectable({
  providedIn: 'root'
})
export class OfficeProjectService {
  private apiUrl = `${environment.apiUrl}/api/OfficeProjectManagement`;

  constructor(private http: HttpClient) {}

  private getHeaders(): HttpHeaders {
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

  private handleError(error: any): Observable<never> {
    console.error('Office Project service error:', error);
    return throwError(() => {
      const errorMessage = error.error?.message || error.error?.title || 'An unexpected error occurred';
      return {
        message: errorMessage,
        status: error.status || 500,
        details: error.error?.errors || null
      };
    });
  }

  /**
   * Get all office projects with filtering and pagination (UC-7.10)
   */
  getAllProjects(filter?: OfficeProjectFilter): Observable<OfficeProjectPagedResult<OfficeProjectListItem>> {
    return this.http.get<OfficeProjectPagedResult<OfficeProjectListItem>>(this.apiUrl, {
      headers: this.getHeaders(),
      params: this.buildHttpParams(filter)
    }).pipe(
      catchError(this.handleError)
    );
  }

  /**
   * Get project by ID (UC-7.11)
   */
  getProjectById(id: string): Observable<OfficeProject> {
    return this.http.get<OfficeProject>(`${this.apiUrl}/${id}`, {
      headers: this.getHeaders()
    }).pipe(
      catchError(this.handleError)
    );
  }

  /**
   * Create new project (UC-7.1)
   */
  createProject(project: OfficeProjectDto): Observable<OfficeProject> {
    return this.http.post<OfficeProject>(this.apiUrl, project, {
      headers: this.getHeaders()
    }).pipe(
      catchError(this.handleError)
    );
  }

  /**
   * Update project (UC-7.8)
   */
  updateProject(id: string, project: OfficeProjectDto): Observable<OfficeProject> {
    return this.http.put<OfficeProject>(`${this.apiUrl}/${id}`, project, {
      headers: this.getHeaders()
    }).pipe(
      catchError(this.handleError)
    );
  }

  /**
   * Delete project
   */
  deleteProject(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`, {
      headers: this.getHeaders()
    }).pipe(
      catchError(this.handleError)
    );
  }

  // ==================== Project-Specific Operations ====================

  /**
   * Set project budget (UC-7.2)
   */
  setProjectBudget(id: string, projectCostEGP?: number, projectCostSAR?: number): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/${id}/budget`, {
      projectCostEGP,
      projectCostSAR
    }, {
      headers: this.getHeaders()
    }).pipe(
      catchError(this.handleError)
    );
  }

  /**
   * Specify project donor (UC-7.3)
   */
  specifyProjectDonor(id: string, donorName: string): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/${id}/donor`, {
      donorName
    }, {
      headers: this.getHeaders()
    }).pipe(
      catchError(this.handleError)
    );
  }

  /**
   * Set beneficiaries count (UC-7.4)
   */
  setBeneficiariesCount(id: string, beneficiariesCount: number, beneficiariesType?: string): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/${id}/beneficiaries`, {
      beneficiariesCount,
      beneficiariesType
    }, {
      headers: this.getHeaders()
    }).pipe(
      catchError(this.handleError)
    );
  }

  /**
   * Assign project location (UC-7.5)
   */
  assignProjectLocation(id: string, countryId?: number, regionId?: number, centerId?: number, villageName?: string): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/${id}/location`, {
      fk_CountryId: countryId,
      fk_RegionId: regionId,
      fk_CenterId: centerId,
      villageName
    }, {
      headers: this.getHeaders()
    }).pipe(
      catchError(this.handleError)
    );
  }

  /**
   * Attach project document (UC-7.6)
   */
  attachProjectDocument(id: string, fk_AttachedFileId: string, documentType?: string, description?: string): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/${id}/documents`, {
      fk_AttachedFileId,
      documentType,
      description,
      documentDate: new Date()
    }, {
      headers: this.getHeaders()
    }).pipe(
      catchError(this.handleError)
    );
  }

  /**
   * Upload project report (UC-7.7)
   */
  uploadProjectReport(id: string, fk_ProjectReportFileId: string, reportType?: string, summary?: string): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/${id}/report`, {
      fk_ProjectReportFileId,
      reportType,
      summary,
      reportDate: new Date()
    }, {
      headers: this.getHeaders()
    }).pipe(
      catchError(this.handleError)
    );
  }

  /**
   * Mark project as completed (UC-7.9)
   */
  markAsCompleted(id: string, isFinished: boolean = true, projectEndDate?: Date): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/${id}/complete`, {
      isFinished,
      projectEndDate
    }, {
      headers: this.getHeaders()
    }).pipe(
      catchError(this.handleError)
    );
  }

  /**
   * Set project dates (UC-7.13)
   */
  setProjectDates(id: string, projectDate: Date, projectEndDate?: Date): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/${id}/dates`, {
      projectDate,
      projectEndDate
    }, {
      headers: this.getHeaders()
    }).pipe(
      catchError(this.handleError)
    );
  }

  /**
   * Assign project to charity (UC-7.14)
   */
  assignProjectToCharity(id: string, fk_CharityId: string): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/${id}/assign-charity`, {
      fk_CharityId
    }, {
      headers: this.getHeaders()
    }).pipe(
      catchError(this.handleError)
    );
  }

  // ==================== View Operations ====================

  /**
   * Get projects assigned to a specific charity (UC-7.14)
   */
  getProjectsByCharity(charityId: string, filter?: OfficeProjectFilter): Observable<OfficeProjectPagedResult<OfficeProjectListItem>> {
    return this.http.get<OfficeProjectPagedResult<OfficeProjectListItem>>(`${this.apiUrl}/charity/${charityId}`, {
      headers: this.getHeaders(),
      params: this.buildHttpParams(filter)
    }).pipe(
      catchError(this.handleError)
    );
  }

  /**
   * Get project status summary (UC-7.12)
   */
  getStatusSummary(): Observable<OfficeProjectStatusSummary> {
    return this.http.get<OfficeProjectStatusSummary>(`${this.apiUrl}/status-summary`, {
      headers: this.getHeaders()
    }).pipe(
      catchError(this.handleError)
    );
  }

  /**
   * Get ongoing projects
   */
  getOngoingProjects(): Observable<OfficeProjectListItem[]> {
    return this.http.get<OfficeProjectListItem[]>(`${this.apiUrl}/ongoing`, {
      headers: this.getHeaders()
    }).pipe(
      catchError(this.handleError)
    );
  }

  /**
   * Get completed projects
   */
  getCompletedProjects(): Observable<OfficeProjectListItem[]> {
    return this.http.get<OfficeProjectListItem[]>(`${this.apiUrl}/completed`, {
      headers: this.getHeaders()
    }).pipe(
      catchError(this.handleError)
    );
  }

  // ==================== Export ====================

  /**
   * Export projects to Excel (UC-7.10)
   */
  exportToExcel(filter?: OfficeProjectFilter): Observable<Blob> {
    return this.http.post(`${this.apiUrl}/export`, filter || {}, {
      headers: this.getHeaders(),
      responseType: 'blob'
    }).pipe(
      catchError(this.handleError)
    );
  }

  // ==================== Lookup Data ====================

  /**
   * Get lookup data for project types
   */
  getProjectTypes(): Observable<any[]> {
    return this.http.get<any[]>(`${environment.apiUrl}/api/LookupManagement/office-project-types`, {
      headers: this.getHeaders()
    }).pipe(
      catchError(this.handleError)
    );
  }

  /**
   * Get lookup data for countries
   */
  getCountries(): Observable<any[]> {
    return this.http.get<any[]>(`${environment.apiUrl}/api/Lookup/Countries`, {
      headers: this.getHeaders()
    }).pipe(
      catchError(this.handleError)
    );
  }

  /**
   * Get regions by country
   */
  getRegionsByCountry(countryId: number): Observable<any[]> {
    return this.http.get<any[]>(`${environment.apiUrl}/api/Lookup/Countries/${countryId}/Regions`, {
      headers: this.getHeaders()
    }).pipe(
      catchError(this.handleError)
    );
  }

  /**
   * Get centers by region
   */
  getCentersByRegion(regionId: number): Observable<any[]> {
    return this.http.get<any[]>(`${environment.apiUrl}/api/Lookup/Regions/${regionId}/Centers`, {
      headers: this.getHeaders()
    }).pipe(
      catchError(this.handleError)
    );
  }

  /**
   * Get charities
   */
  getCharities(): Observable<any[]> {
    return this.http.get<any[]>(`${environment.apiUrl}/api/Charities`, {
      headers: this.getHeaders()
    }).pipe(
      catchError(this.handleError)
    );
  }
}
