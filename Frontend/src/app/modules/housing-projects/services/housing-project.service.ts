/**
 * Housing Project Service
 * Handles all housing project-related API calls
 * Housing Projects Module - IIROSA Frontend Application
 * Access: Admin and Super Admin only
 */

import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';

import {
  HousingProject,
  CreateHousingProjectRequest,
  UpdateHousingProjectRequest,
  UpdateBudgetRequest,
  UpdateProgressRequest,
  MarkProjectCompletedRequest,
  HousingProjectSearchRequest,
  HousingProjectListResponse,
  AttachDocumentRequest,
  HousingProjectDocument,
  HousingProjectProgress,
  HousingProjectReport,
  DocumentType
} from '../models/housing-project.model';

/**
 * API Response wrapper
 */
interface ApiResponse<T> {
  success: boolean;
  data?: T;
  message?: string;
  errors?: string[];
}

/**
 * Paged response wrapper
 */
interface PagedApiResponse<T> {
  items: T[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
}

@Injectable({
  providedIn: 'root'
})
export class HousingProjectService {
  private readonly apiBaseUrl = '/api/housingprojects';

  constructor(private http: HttpClient) {}

  /**
   * Get housing projects with filtering and pagination
   * @param search Search and filter criteria
   * @returns Paginated housing project list
   */
  getHousingProjects(search: HousingProjectSearchRequest): Observable<HousingProjectListResponse> {
    let params = this.buildHttpParams(search);

    return this.http.get<PagedApiResponse<HousingProject>>(this.apiBaseUrl, { params })
      .pipe(
        map(response => ({
          items: response.items || [],
          totalCount: response.totalCount || 0,
          pageNumber: response.pageNumber || search.page,
          pageSize: response.pageSize || search.pageSize,
          totalPages: response.totalPages || 0
        }))
      );
  }

  /**
   * Get a single housing project by ID
   * @param id Housing Project ID
   * @returns Housing Project details
   */
  getHousingProjectById(id: string): Observable<HousingProject> {
    return this.http.get<ApiResponse<HousingProject>>(`${this.apiBaseUrl}/${id}`)
      .pipe(
        map(response => {
          if (!response.success || !response.data) {
            throw new Error(response.message || 'Failed to load housing project');
          }
          return response.data;
        })
      );
  }

  /**
   * Create a new housing project
   * @param request Housing Project creation request
   * @returns Created housing project
   */
  createHousingProject(request: CreateHousingProjectRequest): Observable<HousingProject> {
    return this.http.post<ApiResponse<HousingProject>>(this.apiBaseUrl, request)
      .pipe(
        map(response => {
          if (!response.success || !response.data) {
            throw new Error(response.message || 'Failed to create housing project');
          }
          return response.data;
        })
      );
  }

  /**
   * Update an existing housing project
   * @param id Housing Project ID
   * @param request Housing Project update request
   * @returns Updated housing project
   */
  updateHousingProject(id: string, request: UpdateHousingProjectRequest): Observable<HousingProject> {
    return this.http.put<ApiResponse<HousingProject>>(`${this.apiBaseUrl}/${id}`, request)
      .pipe(
        map(response => {
          if (!response.success || !response.data) {
            throw new Error(response.message || 'Failed to update housing project');
          }
          return response.data;
        })
      );
  }

  /**
   * Delete a housing project
   * @param id Housing Project ID
   * @returns Success status
   */
  deleteHousingProject(id: string): Observable<boolean> {
    return this.http.delete<ApiResponse<boolean>>(`${this.apiBaseUrl}/${id}`)
      .pipe(
        map(response => response.success || false)
      );
  }

  /**
   * Update project budget
   * @param id Housing Project ID
   * @param request Budget update request
   * @returns Updated housing project
   */
  updateBudget(id: string, request: UpdateBudgetRequest): Observable<HousingProject> {
    return this.http.patch<ApiResponse<HousingProject>>(`${this.apiBaseUrl}/${id}/budget`, request)
      .pipe(
        map(response => {
          if (!response.success || !response.data) {
            throw new Error(response.message || 'Failed to update project budget');
          }
          return response.data;
        })
      );
  }

  /**
   * Update project progress
   * @param id Housing Project ID
   * @param request Progress update request
   * @returns Updated housing project
   */
  updateProgress(id: string, request: UpdateProgressRequest): Observable<HousingProject> {
    return this.http.patch<ApiResponse<HousingProject>>(`${this.apiBaseUrl}/${id}/progress`, request)
      .pipe(
        map(response => {
          if (!response.success || !response.data) {
            throw new Error(response.message || 'Failed to update project progress');
          }
          return response.data;
        })
      );
  }

  /**
   * Mark a housing project as completed
   * @param id Housing Project ID
   * @param request Completion request
   * @returns Updated housing project
   */
  markAsCompleted(id: string, request: MarkProjectCompletedRequest): Observable<HousingProject> {
    return this.http.patch<ApiResponse<HousingProject>>(`${this.apiBaseUrl}/${id}/complete`, request)
      .pipe(
        map(response => {
          if (!response.success || !response.data) {
            throw new Error(response.message || 'Failed to mark project as completed');
          }
          return response.data;
        })
      );
  }

  /**
   * Assign a charity to the project
   * @param id Housing Project ID
   * @param charityId Charity ID
   * @returns Updated housing project
   */
  assignCharity(id: string, charityId: number): Observable<HousingProject> {
    return this.http.patch<ApiResponse<HousingProject>>(`${this.apiBaseUrl}/${id}/charity`, { charityId })
      .pipe(
        map(response => {
          if (!response.success || !response.data) {
            throw new Error(response.message || 'Failed to assign charity');
          }
          return response.data;
        })
      );
  }

  /**
   * Assign a beneficiary family to the project
   * @param id Housing Project ID
   * @param familyId Family ID
   * @returns Updated housing project
   */
  assignFamily(id: string, familyId: string): Observable<HousingProject> {
    return this.http.patch<ApiResponse<HousingProject>>(`${this.apiBaseUrl}/${id}/family`, { familyId })
      .pipe(
        map(response => {
          if (!response.success || !response.data) {
            throw new Error(response.message || 'Failed to assign family');
          }
          return response.data;
        })
      );
  }

  /**
   * Get project documents
   * @param id Housing Project ID
   * @returns List of project documents
   */
  getProjectDocuments(id: string): Observable<HousingProjectDocument[]> {
    return this.http.get<ApiResponse<HousingProjectDocument[]>>(`${this.apiBaseUrl}/${id}/documents`)
      .pipe(
        map(response => response.data || [])
      );
  }

  /**
   * Upload project document
   * @param id Housing Project ID
   * @param documentType Document type
   * @param description Optional description
   * @param file File to upload
   * @returns Uploaded document
   */
  uploadDocument(id: string, documentType: DocumentType, description: string | undefined, file: File): Observable<HousingProjectDocument> {
    const formData = new FormData();
    formData.append('file', file);
    formData.append('documentType', documentType.toString());
    if (description) {
      formData.append('description', description);
    }

    return this.http.post<ApiResponse<HousingProjectDocument>>(`${this.apiBaseUrl}/${id}/documents`, formData)
      .pipe(
        map(response => {
          if (!response.success || !response.data) {
            throw new Error(response.message || 'Failed to upload document');
          }
          return response.data;
        })
      );
  }

  /**
   * Delete project document
   * @param id Housing Project ID
   * @param documentId Document ID
   * @returns Success status
   */
  deleteDocument(id: string, documentId: string): Observable<boolean> {
    return this.http.delete<ApiResponse<boolean>>(`${this.apiBaseUrl}/${id}/documents/${documentId}`)
      .pipe(
        map(response => response.success || false)
      );
  }

  /**
   * Get project progress history
   * @param id Housing Project ID
   * @returns List of progress entries
   */
  getProgressHistory(id: string): Observable<HousingProjectProgress[]> {
    return this.http.get<ApiResponse<HousingProjectProgress[]>>(`${this.apiBaseUrl}/${id}/progress-history`)
      .pipe(
        map(response => response.data || [])
      );
  }

  /**
   * Generate housing project report
   * @param search Search and filter criteria for report
   * @returns Housing project report data
   */
  generateReport(search: HousingProjectSearchRequest): Observable<HousingProjectReport> {
    let params = this.buildHttpParams(search);

    return this.http.get<HousingProjectReport>(`${this.apiBaseUrl}/report`, { params });
  }

  /**
   * Export housing projects to Excel
   * @param search Search and filter criteria (same filters will be applied to export)
   * @returns Blob for file download
   */
  exportHousingProjects(search: HousingProjectSearchRequest): Observable<Blob> {
    let params = this.buildHttpParams(search);

    return this.http.get(`${this.apiBaseUrl}/export`, {
      params,
      responseType: 'blob'
    });
  }

  /**
   * Export housing project report to PDF
   * @param search Search and filter criteria for report
   * @returns Blob for file download
   */
  exportReportToPDF(search: HousingProjectSearchRequest): Observable<Blob> {
    let params = this.buildHttpParams(search);

    return this.http.get(`${this.apiBaseUrl}/report/pdf`, {
      params,
      responseType: 'blob'
    });
  }

  /**
   * Get housing project statistics for dashboard
   * @returns Statistics including total projects, by status, etc.
   */
  getStatistics(): Observable<{
    totalProjects: number;
    byStatus: Record<string, number>;
    byType: Record<string, number>;
    averageCompletionPercentage: number;
    totalBudget: number;
    familiesHoused: number;
  }> {
    return this.http.get<{
      totalProjects: number;
      byStatus: Record<string, number>;
      byType: Record<string, number>;
      averageCompletionPercentage: number;
      totalBudget: number;
      familiesHoused: number;
    }>(`${this.apiBaseUrl}/statistics`);
  }

  /**
   * Build HTTP params from search request
   * @param search Search criteria
   * @returns HttpParams object
   */
  private buildHttpParams(search: HousingProjectSearchRequest): HttpParams {
    let params = new HttpParams();

    if (search.search) {
      params = params.set('search', search.search);
    }

    if (search.projectType) {
      params = params.set('projectType', search.projectType.toString());
    }

    if (search.projectStatus) {
      params = params.set('projectStatus', search.projectStatus.toString());
    }

    if (search.countryId) {
      params = params.set('countryId', search.countryId.toString());
    }

    if (search.regionId) {
      params = params.set('regionId', search.regionId.toString());
    }

    if (search.centerId) {
      params = params.set('centerId', search.centerId.toString());
    }

    if (search.assignedCharityId) {
      params = params.set('assignedCharityId', search.assignedCharityId.toString());
    }

    if (search.dateFrom) {
      params = params.set('dateFrom', search.dateFrom.toISOString());
    }

    if (search.dateTo) {
      params = params.set('dateTo', search.dateTo.toISOString());
    }

    params = params.set('page', search.page.toString());
    params = params.set('pageSize', search.pageSize.toString());

    return params;
  }
}
