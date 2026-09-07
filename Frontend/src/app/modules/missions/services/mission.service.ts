/**
 * Mission Service
 * Handles all mission-related API calls (epic 15, UC-MSN-01…09)
 * The API returns raw DTOs — no {success, data} envelope.
 */

import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';

import {
  Mission,
  MissionDetail,
  CreateMissionRequest,
  UpdateMissionRequest,
  RegisterMissionResultRequest,
  MissionSearchRequest,
  MissionListResponse,
  MissionLookupItem
} from '../models/mission.model';

/**
 * Paged response wrapper as the API serializes it
 */
interface PagedApiResponse<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

@Injectable({
  providedIn: 'root'
})
export class MissionService {
  private readonly apiBaseUrl = '/api/missionmanagement';
  private readonly lookupBaseUrl = '/api/lookupmanagement';

  constructor(private http: HttpClient) {}

  /**
   * The §20.U.1 register read — scoped server-side to the caller's charity and country
   */
  getMyMissions(search: MissionSearchRequest): Observable<MissionListResponse> {
    const params = this.buildHttpParams(search);

    return this.http.get<PagedApiResponse<Mission>>(`${this.apiBaseUrl}/my-missions`, { params })
      .pipe(
        map(response => ({
          items: response.items || [],
          totalCount: response.totalCount || 0,
          page: response.page || search.page,
          pageSize: response.pageSize || search.pageSize,
          // The wire carries no totalPages — derive it (guarded division: the server
          // clamps pageSize ≥ 1, the fallback covers a stale client value)
          totalPages: Math.ceil((response.totalCount || 0) / Math.max(1, response.pageSize || search.pageSize || 1))
        }))
      );
  }

  /**
   * Filtered read of the register (UC-MSN-02) — same pipeline, same wire contract
   */
  getMissions(search: MissionSearchRequest): Observable<MissionListResponse> {
    const params = this.buildHttpParams(search);

    return this.http.get<PagedApiResponse<Mission>>(this.apiBaseUrl, { params })
      .pipe(
        map(response => ({
          items: response.items || [],
          totalCount: response.totalCount || 0,
          page: response.page || search.page,
          pageSize: response.pageSize || search.pageSize,
          // The wire carries no totalPages — derive it (guarded division: the server
          // clamps pageSize ≥ 1, the fallback covers a stale client value)
          totalPages: Math.ceil((response.totalCount || 0) / Math.max(1, response.pageSize || search.pageSize || 1))
        }))
      );
  }

  /**
   * Get a single mission by ID — the raw MissionDetailDto
   */
  getMissionById(id: string): Observable<MissionDetail> {
    return this.http.get<MissionDetail>(`${this.apiBaseUrl}/${id}`);
  }

  /**
   * Create a new mission (UC-MSN-06) — returns the created MissionDetailDto
   */
  createMission(request: CreateMissionRequest): Observable<MissionDetail> {
    return this.http.post<MissionDetail>(this.apiBaseUrl, request);
  }

  /**
   * Update an existing mission (UC-MSN-07) — returns the updated MissionDetailDto
   */
  updateMission(id: string, request: UpdateMissionRequest): Observable<MissionDetail> {
    return this.http.put<MissionDetail>(`${this.apiBaseUrl}/${id}`, request);
  }

  /**
   * Delete a mission (UC-MSN-08, SuperAdmin only) — raw response, no envelope mapping
   */
  deleteMission(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiBaseUrl}/${id}`);
  }

  /**
   * Register the mission result (UC-MSN-09)
   */
  registerMissionResult(id: string, request: RegisterMissionResultRequest): Observable<MissionDetail> {
    return this.http.post<MissionDetail>(`${this.apiBaseUrl}/${id}/event`, request);
  }

  /**
   * Get available mission types (UC-MSN-03)
   */
  getMissionTypes(): Observable<MissionLookupItem[]> {
    return this.http.get<MissionLookupItem[]>(`${this.apiBaseUrl}/mission-types`);
  }

  /**
   * Get available mission time types (UC-MSN-05) — served from the lookup table
   */
  getMissionTimeTypes(): Observable<MissionLookupItem[]> {
    return this.http.get<MissionLookupItem[]>(`${this.apiBaseUrl}/mission-time-types`);
  }

  /**
   * Get available mission interview types (UC-MSN-04)
   */
  getMissionInterviewTypes(): Observable<MissionLookupItem[]> {
    return this.http.get<MissionLookupItem[]>(`${this.lookupBaseUrl}/mission-interview-types`);
  }

  /**
   * Build HTTP params from search request — the keys mirror MissionFilterDto on the wire
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

    if (search.charityId) {
      params = params.set('charityId', search.charityId);
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

    if (search.assignedToUserId) {
      params = params.set('assignedToUserId', search.assignedToUserId);
    }

    if (search.dateFrom) {
      params = params.set('dateFrom', search.dateFrom);
    }

    if (search.dateTo) {
      params = params.set('dateTo', search.dateTo);
    }

    params = params.set('page', search.page.toString());
    params = params.set('pageSize', search.pageSize.toString());

    return params;
  }
}
