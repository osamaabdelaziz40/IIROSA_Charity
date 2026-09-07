/**
 * Housing Projects Service (epic 6, chapter 11)
 *
 * RE-CUT (6-1/6-3/6-4): the construction-project CRUD/budget/progress surface was deleted
 * with the invented tracker. This module's own API is the housing-FAMILY register surface on
 * HousingProjectsController:
 *   POST /api/housingprojects/projects        — UC-HOU-03 register a housing family (§11.S.2)
 *   GET  /api/housingprojects/projects/{id}   — UC-HOU-04 view aggregate (§11.U.4)
 *   PUT  /api/housingprojects/projects/{id}   — UC-HOU-04 update (§11.U.4)
 * (beneficiaries arrives with 6-7.)
 * The §11.S.1 list itself reads the families register through FamilyService, and the
 * building/flat + form drop-down catalogues load through LookupManagementService.
 *
 * 6-6 adds the §11.S.3 reports read on the shared PeriodicOrphanReports controller:
 *   GET    /api/PeriodicOrphanReports/by-orphan/{beneficiaryId}?childOrParent=
 *   DELETE /api/PeriodicOrphanReports/{id}                          (epic 9 UC-ORR-06)
 * 6-7 adds the beneficiary resolve:
 *   GET    /api/housingprojects/projects/{id}/beneficiaries[?code=] (UC-HOU-07)
 */

import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

import {
  CreateHousingFamilyRequest,
  HousingFamilyCreatedResult,
  HousingFamilyDetail,
  HousingBeneficiaryType,
  HousingBeneficiaryRow,
  HousingReportPagedResult,
  CreateHousingReportRequest,
  UpdateHousingReportRequest,
  HousingReportDetail
} from '../models/housing-project.model';

@Injectable({
  providedIn: 'root'
})
export class HousingProjectService {
  private readonly apiBaseUrl = '/api/housingprojects';
  private readonly reportsBaseUrl = '/api/PeriodicOrphanReports';

  constructor(private http: HttpClient) {}

  /**
   * UC-HOU-03 (§11.U.3): register a housing family. The server forces the Housing
   * discriminator, enforces the §11.S.2 mandatories (ValidationException → 400 with
   * { message, errors }) and refuses a flat outside the chosen building (BusinessException →
   * 400 { message }). The response body is the created FamilyDto itself (201, raw — the
   * controller does not wrap it in the ApiResponse envelope).
   */
  createHousingFamily(request: CreateHousingFamilyRequest): Observable<HousingFamilyCreatedResult> {
    return this.http.post<HousingFamilyCreatedResult>(`${this.apiBaseUrl}/projects`, request);
  }

  /**
   * UC-HOU-04 (§11.U.4): the housing-family aggregate for the view/edit screen — every
   * §11.S.2 field incl. the guardian block and full child detail. Unknown, non-housing and
   * foreign rows all answer 404 (a foreign id must not prove the record exists).
   */
  getHousingFamily(id: string): Observable<HousingFamilyDetail> {
    return this.http.get<HousingFamilyDetail>(`${this.apiBaseUrl}/projects/${id}`);
  }

  /**
   * UC-HOU-04 (§11.U.4): update a housing family under the same §11.S.2 contract as the
   * create — family fields + allocation (flat ⊂ building re-validated) + guardian +
   * children sync (id-matched update, id-less add, absent soft-remove). Ownership never
   * moves (any client-sent charity field is ignored server-side). Returns the re-read
   * aggregate (200, raw envelope).
   */
  updateHousingFamily(id: string, request: CreateHousingFamilyRequest): Observable<HousingFamilyDetail> {
    return this.http.put<HousingFamilyDetail>(`${this.apiBaseUrl}/projects/${id}`, request);
  }

  /**
   * UC-HOU-06 (§11.S.3): one housing beneficiary's periodic report history, selected by
   * the ChildOrParent discriminator. Child ⇒ beneficiaryId is an orphan (family child) id;
   * Parent ⇒ it is the family guardian's provider id. Scoping to the caller's charity is
   * server-side; unknown / non-housing / out-of-scope beneficiaries answer 404, an invalid
   * discriminator 400.
   */
  getHousingBeneficiaryReports(
    beneficiaryId: string,
    beneficiaryType: HousingBeneficiaryType,
    pageNumber = 1,
    pageSize = 10
  ): Observable<HousingReportPagedResult> {
    return this.http.get<HousingReportPagedResult>(
      `${this.reportsBaseUrl}/by-orphan/${beneficiaryId}`,
      { params: { childOrParent: beneficiaryType, pageNumber, pageSize } }
    );
  }

  /**
   * UC-ORR-06 (epic 9 endpoint, reused by §11.S.3's delete command): soft-delete a report.
   * Server roles are SuperAdmin/Admin only, and accepted or locked reports are refused —
   * the caller gates the icon on both.
   */
  deleteHousingReport(reportId: string): Observable<void> {
    return this.http.delete<void>(`${this.reportsBaseUrl}/${reportId}`);
  }

  /**
   * UC-HOU-07 (§11.U.7 البحث بالكود): the housing family's beneficiaries — children + the
   * guardian — for the §11.S.3 picker. A non-blank code resolves a child's sponsorship code
   * exactly; unknown / other-charity codes answer an EMPTY list (explicit not-found at the
   * caller — never a silent empty grid). Foreign / non-housing family ids answer 404.
   */
  getHousingBeneficiaries(familyId: string, code?: string): Observable<HousingBeneficiaryRow[]> {
    const params: Record<string, string> = {};
    if (code && code.trim()) {
      params['code'] = code.trim();
    }
    return this.http.get<HousingBeneficiaryRow[]>(
      `${this.apiBaseUrl}/projects/${familyId}/beneficiaries`, { params });
  }

  // ==================== 6-8 — the §11.S.4 report form (UC-HOU-08) ====================

  /**
   * UC-HOU-08 (§11.S.4): create a housing periodic report on the shared epic-9 endpoint.
   * The §11.U.6 discriminator picks the branch server-side: Child ⇒ the family child's
   * orphan id; Parent ⇒ the guardian's beneficiary id (the server resolves the family and
   * the carrier child). Validation failures answer 400 { message, errors }; business rules
   * 400 { message }; out-of-scope beneficiaries 404. The 201 body is the created report.
   */
  createHousingReport(request: CreateHousingReportRequest): Observable<HousingReportDetail> {
    return this.http.post<HousingReportDetail>(this.reportsBaseUrl, request);
  }

  /**
   * §11.S.4 edit mode: the report detail (childOrParent + housingFamilyId tell the header
   * which beneficiary block to render). Unknown / out-of-scope ids answer 404.
   */
  getHousingReport(reportId: string): Observable<HousingReportDetail> {
    return this.http.get<HousingReportDetail>(`${this.reportsBaseUrl}/${reportId}`);
  }

  /**
   * §11.S.4 edit save — the epic-9 full-replace PUT; every form control is sent, the
   * housing identity fields are immutable server-side. Locked / accepted reports are
   * refused with the literal 'You can not update old report'.
   */
  updateHousingReport(reportId: string, request: UpdateHousingReportRequest): Observable<HousingReportDetail> {
    return this.http.put<HousingReportDetail>(`${this.reportsBaseUrl}/${reportId}`, request);
  }

  /** §14.U.5 gate reused for the §11.S.4 edit screen — { canEdit } (false when locked/accepted). */
  canEditHousingReport(reportId: string): Observable<{ canEdit: boolean }> {
    return this.http.get<{ canEdit: boolean }>(`${this.reportsBaseUrl}/${reportId}/can-edit`);
  }
}
