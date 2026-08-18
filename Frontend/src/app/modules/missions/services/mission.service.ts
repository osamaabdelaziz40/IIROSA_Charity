/**
 * Mission Service
 * Handles all mission-related API calls
 * Missions Module - IIROSA Frontend Application
 */

import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';

import {
  Mission,
  CreateMissionRequest,
  UpdateMissionRequest,
  MissionSearchRequest,
  MissionListResponse,
  MarkMissionCompletedRequest,
  MissionStatusCounts,
  MissionType,
  MissionTimeType
} from '../models/mission.model';

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
export class MissionService {
  private readonly apiBaseUrl = '/api/missionmanagement';

  constructor(private http: HttpClient) {}

  /**
   * Get missions with filtering and pagination
   * @param search Search and filter criteria
   * @returns Paginated mission list
   */
  getMissions(search: MissionSearchRequest): Observable<MissionListResponse> {
    let params = this.buildHttpParams(search);

    return this.http.get<PagedApiResponse<Mission>>(this.apiBaseUrl, { params })
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
   * Get a single mission by ID
   * @param id Mission ID
   * @returns Mission details
   */
  getMissionById(id: string): Observable<Mission> {
    return this.http.get<ApiResponse<Mission>>(`${this.apiBaseUrl}/${id}`)
      .pipe(
        map(response => {
          if (!response.success || !response.data) {
            throw new Error(response.message || 'Failed to load mission');
          }
          return response.data;
        })
      );
  }

  /**
   * Create a new mission
   * @param request Mission creation request
   * @returns Created mission
   */
  createMission(request: CreateMissionRequest): Observable<Mission> {
    return this.http.post<ApiResponse<Mission>>(this.apiBaseUrl, request)
      .pipe(
        map(response => {
          if (!response.success || !response.data) {
            throw new Error(response.message || 'Failed to create mission');
          }
          return response.data;
        })
      );
  }

  /**
   * Update an existing mission
   * @param id Mission ID
   * @param request Mission update request
   * @returns Updated mission
   */
  updateMission(id: string, request: UpdateMissionRequest): Observable<Mission> {
    return this.http.put<ApiResponse<Mission>>(`${this.apiBaseUrl}/${id}`, request)
      .pipe(
        map(response => {
          if (!response.success || !response.data) {
            throw new Error(response.message || 'Failed to update mission');
          }
          return response.data;
        })
      );
  }

  /**
   * Delete a mission
   * @param id Mission ID
   * @returns Success status
   */
  deleteMission(id: string): Observable<boolean> {
    return this.http.delete<ApiResponse<boolean>>(`${this.apiBaseUrl}/${id}`)
      .pipe(
        map(response => response.success || false)
      );
  }

  /**
   * Mark a mission as completed
   * @param id Mission ID
   * @param request Completion request with optional notes
   * @returns Updated mission
   */
  markAsCompleted(id: string, request: MarkMissionCompletedRequest): Observable<Mission> {
    return this.http.patch<ApiResponse<Mission>>(`${this.apiBaseUrl}/${id}/complete`, request)
      .pipe(
        map(response => {
          if (!response.success || !response.data) {
            throw new Error(response.message || 'Failed to mark mission as completed');
          }
          return response.data;
        })
      );
  }

  /**
   * Reopen a completed mission
   * @param id Mission ID
   * @returns Updated mission
   */
  reopenMission(id: string): Observable<Mission> {
    return this.http.patch<ApiResponse<Mission>>(`${this.apiBaseUrl}/${id}/reopen`, {})
      .pipe(
        map(response => {
          if (!response.success || !response.data) {
            throw new Error(response.message || 'Failed to reopen mission');
          }
          return response.data;
        })
      );
  }

  /**
   * Get mission status counts for dashboard
   * @returns Status counts (pending, inProgress, completed, overdue)
   */
  getMissionStatusCounts(): Observable<MissionStatusCounts> {
    return this.http.get<MissionStatusCounts>(`${this.apiBaseUrl}/status-counts`);
  }

  /**
   * Get missions for the current user (My Missions)
   * @param search Search and filter criteria
   * @returns Paginated mission list
   */
  getMyMissions(search: MissionSearchRequest): Observable<MissionListResponse> {
    let params = this.buildHttpParams(search);

    return this.http.get<PagedApiResponse<Mission>>(`${this.apiBaseUrl}/my-missions`, { params })
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
   * Export missions to Excel
   * @param search Search and filter criteria (same filters will be applied to export)
   * @returns Blob for file download
   */
  exportMissions(search: MissionSearchRequest): Observable<Blob> {
    let params = this.buildHttpParams(search);

    return this.http.get(`${this.apiBaseUrl}/export`, {
      params,
      responseType: 'blob'
    });
  }

  /**
   * Get available mission types
   * @returns List of mission types
   */
  getMissionTypes(): Observable<MissionType[]> {
    return this.http.get<MissionType[]>(`${this.apiBaseUrl}/mission-types`);
  }

  /**
   * Get available mission time types
   * @returns List of mission time types
   */
  getMissionTimeTypes(): Observable<MissionTimeType[]> {
    return this.http.get<MissionTimeType[]>(`${this.apiBaseUrl}/mission-time-types`);
  }

  /**
   * Build HTTP params from search request
   * @param search Search criteria
   * @returns HttpParams object
   */
  private buildHttpParams(search: MissionSearchRequest): HttpParams {
    let params = new HttpParams();

    if (search.search) {
      params = params.set('search', search.search);
    }

    if (search.missionTypeId) {
      params = params.set('missionTypeId', search.missionTypeId.toString());
    }

    if (search.missionTimeTypeId) {
      params = params.set('missionTimeTypeId', search.missionTimeTypeId.toString());
    }

    if (search.isCompleted !== undefined) {
      params = params.set('isCompleted', search.isCompleted.toString());
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

    if (search.assignedTo) {
      params = params.set('assignedTo', search.assignedTo);
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
