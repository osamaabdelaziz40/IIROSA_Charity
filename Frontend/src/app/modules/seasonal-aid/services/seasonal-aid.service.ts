import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';

import {
  SeasonalAidCampaign,
  SeasonalAidCampaignListItem,
  SeasonalAidCampaignFilter,
  SeasonalAidBeneficiary,
  SeasonalAidBeneficiaryFilter,
  EligibleFamiliesFilter,
  SeasonalAidDistribution,
  CreateSeasonalAidDistributionRequest,
  CreateSeasonalAidCampaignRequest,
  UpdateSeasonalAidCampaignRequest,
  SeasonalAidCampaignReport,
  RegisterBeneficiariesRequest,
  UpdateBeneficiariesRequest,
  UpdateBeneficiariesResult,
  SetFamilyReceivedFlagRequest,
  CloseCampaignRequest,
  SeasonalAidPagedResult,
  SeasonalAidCampaignStatistics
} from '../models/seasonal-aid.model';

interface RegisterBeneficiariesResponse {
  message: string;
  registeredCount: number;
  totalAllocation: number;
  budgetImpact: number;
}

/**
 * Client for /api/SeasonalAid (and the UC-PRJ-08 received-flag route on /api/Families).
 * Filter objects map 1:1 onto the backend query DTOs, so they are passed straight through
 * as query params — camelCase on the wire, same names as the DTO properties.
 */
@Injectable({
  providedIn: 'root'
})
export class SeasonalAidService {
  private apiUrl = `${environment.apiUrl}/api/SeasonalAid`;

  constructor(private http: HttpClient) {}

  // Campaigns (UC-PRJ-01..05)

  getCampaigns(filter?: Partial<SeasonalAidCampaignFilter>): Observable<SeasonalAidPagedResult<SeasonalAidCampaignListItem>> {
    return this.http.get<SeasonalAidPagedResult<SeasonalAidCampaignListItem>>(
      `${this.apiUrl}/campaigns`, { params: this.buildParams(filter) });
  }

  /** Register statistics band (UC-9.6) — caller country-scoped server-side. */
  getStatistics(): Observable<SeasonalAidCampaignStatistics> {
    return this.http.get<SeasonalAidCampaignStatistics>(`${this.apiUrl}/campaigns/statistics`);
  }

  getActiveCampaigns(): Observable<SeasonalAidCampaignListItem[]> {
    return this.http.get<SeasonalAidCampaignListItem[]>(`${this.apiUrl}/campaigns/active`);
  }

  getCampaignById(id: string): Observable<SeasonalAidCampaign> {
    return this.http.get<SeasonalAidCampaign>(`${this.apiUrl}/campaigns/${id}`);
  }

  createCampaign(campaign: CreateSeasonalAidCampaignRequest): Observable<SeasonalAidCampaign> {
    return this.http.post<SeasonalAidCampaign>(`${this.apiUrl}/campaigns`, campaign);
  }

  updateCampaign(id: string, campaign: UpdateSeasonalAidCampaignRequest): Observable<SeasonalAidCampaign> {
    return this.http.put<SeasonalAidCampaign>(`${this.apiUrl}/campaigns/${id}`, campaign);
  }

  deleteCampaign(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/campaigns/${id}`);
  }

  closeCampaign(id: string, closureNotes?: string): Observable<{ message: string }> {
    const body: CloseCampaignRequest = { closureNotes };
    return this.http.post<{ message: string }>(`${this.apiUrl}/campaigns/${id}/close`, body);
  }

  reopenCampaign(id: string): Observable<{ message: string }> {
    return this.http.post<{ message: string }>(`${this.apiUrl}/campaigns/${id}/reopen`, {});
  }

  // Eligible families (UC-PRJ-10 — families not yet registered in the campaign)

  getEligibleFamilies(
    campaignId: string,
    filter?: Partial<EligibleFamiliesFilter>
  ): Observable<SeasonalAidPagedResult<SeasonalAidBeneficiary>> {
    return this.http.get<SeasonalAidPagedResult<SeasonalAidBeneficiary>>(
      `${this.apiUrl}/campaigns/${campaignId}/eligible-families`, { params: this.buildParams(filter) });
  }

  // Beneficiary registration (UC-PRJ-06..09)

  getCampaignBeneficiaries(
    campaignId: string,
    filter?: Partial<SeasonalAidBeneficiaryFilter>
  ): Observable<SeasonalAidPagedResult<SeasonalAidBeneficiary>> {
    return this.http.get<SeasonalAidPagedResult<SeasonalAidBeneficiary>>(
      `${this.apiUrl}/campaigns/${campaignId}/beneficiaries`, { params: this.buildParams(filter) });
  }

  registerBeneficiaries(
    campaignId: string,
    request: RegisterBeneficiariesRequest
  ): Observable<RegisterBeneficiariesResponse> {
    return this.http.post<RegisterBeneficiariesResponse>(
      `${this.apiUrl}/campaigns/${campaignId}/beneficiaries`, request);
  }

  /** Full sync of the campaign's family set (UC-PRJ-07) — familyIds is the desired final set. */
  updateCampaignBeneficiaries(
    campaignId: string,
    request: UpdateBeneficiariesRequest
  ): Observable<UpdateBeneficiariesResult> {
    return this.http.put<UpdateBeneficiariesResult>(
      `${this.apiUrl}/campaigns/${campaignId}/beneficiaries`, request);
  }

  removeBeneficiary(beneficiaryId: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/beneficiaries/${beneficiaryId}`);
  }

  /** UC-PRJ-08 — confirm/withdraw a family's receipt; lives on /api/Families per the module spec. */
  setFamilyReceivedFlag(familyId: string, request: SetFamilyReceivedFlagRequest): Observable<{ message: string }> {
    return this.http.put<{ message: string }>(
      `${environment.apiUrl}/api/Families/${familyId}/received-flag`, request);
  }

  // Distributions (UC-PRJ-08 record flow)

  recordDistribution(
    beneficiaryId: string,
    distribution: CreateSeasonalAidDistributionRequest
  ): Observable<SeasonalAidDistribution> {
    return this.http.post<SeasonalAidDistribution>(
      `${this.apiUrl}/beneficiaries/${beneficiaryId}/distributions`, distribution);
  }

  recordBulkDistributions(distributions: CreateSeasonalAidDistributionRequest[]): Observable<{ message: string }> {
    return this.http.post<{ message: string }>(`${this.apiUrl}/distributions/batch`, distributions);
  }

  // Reports (UC-PRJ-11, UC-PRJ-12) — the report endpoint itself; pdf/excel exports are
  // server-side NotImplemented, so rendering happens client-side (print + ExcelJS).

  getCampaignReport(campaignId: string): Observable<SeasonalAidCampaignReport> {
    return this.http.get<SeasonalAidCampaignReport>(`${this.apiUrl}/campaigns/${campaignId}/report`);
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
