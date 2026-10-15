import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';

import {
  OrphanReportCycleDto,
  OrphanReportCycleFilterDto,
  OrphanReportCyclePagedResult,
  OrphanReportCycleSelectorDto,
  OrphanCycleReportStatusDto,
  CreateOrphanReportCycleDto,
  UpdateOrphanReportCycleDto,
  CycleReportCoverageFilterDto,
  CycleReportCoverageResultDto
} from '../models/orphan-report-cycle.model';

/**
 * Client for /api/OrphanReportCycles (UC-ORC, docs/Modules/26-UC-ORC). Management reads
 * and writes are HQ-only server-side; the picker reads (selector, per-orphan report
 * status) are open to every authenticated caller because they back charity screens.
 *
 * The coverage report lives on /api/Reports per the module's controller conventions —
 * its call is hosted here so the cycle types stay in one place.
 */
@Injectable({
  providedIn: 'root'
})
export class OrphanReportCycleService {
  private apiUrl = `${environment.apiUrl}/api/OrphanReportCycles`;

  constructor(private http: HttpClient) {}

  // Management (UC-ORC-01…04 — HQ register)

  getCycles(filter?: Partial<OrphanReportCycleFilterDto>): Observable<OrphanReportCyclePagedResult> {
    return this.http.get<OrphanReportCyclePagedResult>(this.apiUrl, { params: this.buildParams(filter) });
  }

  getCycle(id: string): Observable<OrphanReportCycleDto> {
    return this.http.get<OrphanReportCycleDto>(`${this.apiUrl}/${id}`);
  }

  createCycle(dto: CreateOrphanReportCycleDto): Observable<OrphanReportCycleDto> {
    return this.http.post<OrphanReportCycleDto>(this.apiUrl, dto);
  }

  updateCycle(id: string, dto: UpdateOrphanReportCycleDto): Observable<OrphanReportCycleDto> {
    return this.http.put<OrphanReportCycleDto>(`${this.apiUrl}/${id}`, dto);
  }

  deleteCycle(id: string): Observable<{ message: string }> {
    return this.http.delete<{ message: string }>(`${this.apiUrl}/${id}`);
  }

  // Picker reads (UC-ORC-06/09 — every authenticated caller)

  /** All cycles with live status — button gating + coverage dropdown. */
  getSelector(): Observable<OrphanReportCycleSelectorDto[]> {
    return this.http.get<OrphanReportCycleSelectorDto[]>(`${this.apiUrl}/selector`);
  }

  /** The In Progress set with this orphan's submission state (UC-ORC-06). */
  getOrphanReportStatus(orphanId: string): Observable<OrphanCycleReportStatusDto[]> {
    return this.http.get<OrphanCycleReportStatusDto[]>(`${this.apiUrl}/orphan/${orphanId}/report-status`);
  }

  // Coverage (UC-ORC-09 — POST /api/Reports/cycle-reports)

  getCoverage(filter: CycleReportCoverageFilterDto): Observable<CycleReportCoverageResultDto> {
    return this.http.post<CycleReportCoverageResultDto>(
      `${environment.apiUrl}/api/Reports/cycle-reports`, filter);
  }

  private buildParams(filter?: Record<string, unknown>): HttpParams {
    let params = new HttpParams();
    if (!filter) {
      return params;
    }
    for (const [key, value] of Object.entries(filter)) {
      if (value === undefined || value === null || value === '') {
        continue;
      }
      params = params.append(key, String(value));
    }
    return params;
  }
}
