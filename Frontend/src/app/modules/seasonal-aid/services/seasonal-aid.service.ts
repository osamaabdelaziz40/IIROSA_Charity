import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';

import {
  SeasonalCampaign,
  CampaignBeneficiary,
  AidDistribution,
  CampaignStatistics,
  CampaignReport,
  BeneficiarySelection,
  CampaignFilter,
  BeneficiaryFilter
} from '../models/seasonal-aid.model';

@Injectable({
  providedIn: 'root'
})
export class SeasonalAidService {
  private apiUrl = `${environment.apiUrl}/api/SeasonalAid`;

  constructor(private http: HttpClient) {}

  // Campaign CRUD Operations
  getCampaigns(filter?: CampaignFilter): Observable<SeasonalCampaign[]> {
    let params = new HttpParams();

    if (filter) {
      if (filter.campaignName) params = params.append('campaignName', filter.campaignName);
      if (filter.campaignType) params = params.append('campaignType', filter.campaignType);
      if (filter.status) params = params.append('status', filter.status);
      if (filter.charityId) params = params.append('charityId', filter.charityId);
      if (filter.startDateFrom) params = params.append('startDateFrom', filter.startDateFrom.toISOString());
      if (filter.startDateTo) params = params.append('startDateTo', filter.startDateTo.toISOString());
      if (filter.endDateFrom) params = params.append('endDateFrom', filter.endDateFrom.toISOString());
      if (filter.endDateTo) params = params.append('endDateTo', filter.endDateTo.toISOString());
    }

    return this.http.get<SeasonalCampaign[]>(`${this.apiUrl}/campaigns`, { params });
  }

  getCampaignById(id: string): Observable<SeasonalCampaign> {
    return this.http.get<SeasonalCampaign>(`${this.apiUrl}/campaigns/${id}`);
  }

  createCampaign(campaign: SeasonalCampaign): Observable<SeasonalCampaign> {
    return this.http.post<SeasonalCampaign>(`${this.apiUrl}/campaigns`, campaign);
  }

  updateCampaign(id: string, campaign: SeasonalCampaign): Observable<SeasonalCampaign> {
    return this.http.put<SeasonalCampaign>(`${this.apiUrl}/campaigns/${id}`, campaign);
  }

  deleteCampaign(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/campaigns/${id}`);
  }

  closeCampaign(id: string, closureNotes: string): Observable<SeasonalCampaign> {
    return this.http.post<SeasonalCampaign>(`${this.apiUrl}/campaigns/${id}/close`, { closureNotes });
  }

  // Beneficiary Management
  getAvailableBeneficiaries(campaignId: string, filter?: BeneficiaryFilter): Observable<BeneficiarySelection[]> {
    let params = new HttpParams();

    if (filter) {
      if (filter.charityId) params = params.append('charityId', filter.charityId);
      if (filter.region) params = params.append('region', filter.region);
      if (filter.center) params = params.append('center', filter.center);
      if (filter.familyType) params = params.append('familyType', filter.familyType);
      if (filter.searchTerm) params = params.append('searchTerm', filter.searchTerm);
    }

    return this.http.get<BeneficiarySelection[]>(`${this.apiUrl}/campaigns/${campaignId}/available-beneficiaries`, { params });
  }

  getCampaignBeneficiaries(campaignId: string, filter?: BeneficiaryFilter): Observable<CampaignBeneficiary[]> {
    let params = new HttpParams();

    if (filter) {
      if (filter.distributionStatus) params = params.append('distributionStatus', filter.distributionStatus);
      if (filter.charityId) params = params.append('charityId', filter.charityId);
      if (filter.region) params = params.append('region', filter.region);
      if (filter.searchTerm) params = params.append('searchTerm', filter.searchTerm);
    }

    return this.http.get<CampaignBeneficiary[]>(`${this.apiUrl}/campaigns/${campaignId}/beneficiaries`, { params });
  }

  registerBeneficiaries(campaignId: string, beneficiaryIds: string[]): Observable<CampaignBeneficiary[]> {
    return this.http.post<CampaignBeneficiary[]>(`${this.apiUrl}/campaigns/${campaignId}/beneficiaries`, { beneficiaryIds });
  }

  removeBeneficiary(campaignId: string, beneficiaryId: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/campaigns/${campaignId}/beneficiaries/${beneficiaryId}`);
  }

  // Distribution Management
  getCampaignDistributions(campaignId: string): Observable<AidDistribution[]> {
    return this.http.get<AidDistribution[]>(`${this.apiUrl}/campaigns/${campaignId}/distributions`);
  }

  recordDistribution(distribution: AidDistribution): Observable<AidDistribution> {
    return this.http.post<AidDistribution>(`${this.apiUrl}/distributions`, distribution);
  }

  recordBulkDistributions(distributions: AidDistribution[]): Observable<AidDistribution[]> {
    return this.http.post<AidDistribution[]>(`${this.apiUrl}/distributions/bulk`, { distributions });
  }

  updateDistribution(distributionId: string, distribution: AidDistribution): Observable<AidDistribution> {
    return this.http.put<AidDistribution>(`${this.apiUrl}/distributions/${distributionId}`, distribution);
  }

  // Statistics and Reports
  getCampaignStatistics(campaignId: string): Observable<CampaignStatistics> {
    return this.http.get<CampaignStatistics>(`${this.apiUrl}/campaigns/${campaignId}/statistics`);
  }

  getCampaignReport(campaignId: string): Observable<CampaignReport> {
    return this.http.get<CampaignReport>(`${this.apiUrl}/campaigns/${campaignId}/report`);
  }

  exportCampaignReport(campaignId: string, format: 'pdf' | 'excel'): Observable<Blob> {
    return this.http.get(`${this.apiUrl}/campaigns/${campaignId}/export/${format}`, {
      responseType: 'blob'
    });
  }

  exportCampaignsList(filter?: CampaignFilter): Observable<Blob> {
    let params = new HttpParams();

    if (filter) {
      if (filter.campaignType) params = params.append('campaignType', filter.campaignType);
      if (filter.status) params = params.append('status', filter.status);
      if (filter.charityId) params = params.append('charityId', filter.charityId);
    }

    return this.http.get(`${this.apiUrl}/campaigns/export/excel`, {
      params,
      responseType: 'blob'
    });
  }
}
