import { PagedResponse } from '../../../core/models/common.model';

// Wire contract mirrors of the backend SeasonalAid DTOs (IIROSA.Application/DTOs/SeasonalAid).
// Property names and casing must match the API exactly — this module previously shipped a
// parallel set of invented keys (campaignName, orphanCount, distributionStatus, …) that never
// matched the backend, so every screen silently rendered blanks.

export interface SeasonalAidCampaign {
  id: string;
  name: string;
  campaignType: string;
  description?: string | null;
  startDate: string;
  endDate: string;
  totalBudget: number;
  budgetCurrency: string;
  perFamilyAllocation: number;
  allocatedBudget: number;
  distributedBudget: number;
  remainingBudget: number;
  countryId?: number | null;
  countryName?: string | null;
  regionId?: number | null;
  regionName?: string | null;
  centerId?: number | null;
  centerName?: string | null;
  maximumFamilies?: number | null;
  familyType?: string | null;
  minChildrenAge?: number | null;
  maxChildrenAge?: number | null;
  registeredBeneficiariesCount: number;
  distributedBeneficiariesCount: number;
  pendingBeneficiariesCount: number;
  isActive: boolean;
  isClosed: boolean;
  closedDate?: string | null;
  closureNotes?: string | null;
  createdOn: string;
  createdBy?: string | null;
  updatedOn: string;
  updatedBy?: string | null;
}

export interface SeasonalAidCampaignListItem {
  id: string;
  name: string;
  campaignType: string;
  startDate: string;
  endDate: string;
  totalBudget: number;
  budgetCurrency: string;
  allocatedBudget: number;
  distributedBudget: number;
  registeredBeneficiariesCount: number;
  distributedBeneficiariesCount: number;
  isActive: boolean;
  isClosed: boolean;
  countryName?: string | null;
  completionPercentage: number;
}

export interface CreateSeasonalAidCampaignRequest {
  name: string;
  campaignType: string;
  description?: string | null;
  startDate: string;
  endDate: string;
  totalBudget: number;
  budgetCurrency: string;
  perFamilyAllocation: number;
  countryId?: number | null;
  regionId?: number | null;
  centerId?: number | null;
  maximumFamilies?: number | null;
  familyType?: string | null;
  minChildrenAge?: number | null;
  maxChildrenAge?: number | null;
  isActive: boolean;
}

export interface UpdateSeasonalAidCampaignRequest extends CreateSeasonalAidCampaignRequest {
  id: string;
}

export interface SeasonalAidBeneficiary {
  id: string;
  campaignId: string;
  familyId: string;
  familyCode: string;
  familyAddress?: string | null;
  orphansCount: number;
  familyMembersCount: number;
  charityName?: string | null;
  regionName?: string | null;
  centerName?: string | null;
  allocationAmount: number;
  currency: string;
  isRegistered: boolean;
  registrationDate: string;
  registrationNotes?: string | null;
  isDistributed: boolean;
  distributionDate?: string | null;
  distributedAmount: number;
  receivedBy?: string | null;
  notes?: string | null;
  createdOn: string;
}

export interface SeasonalAidDistribution {
  id: string;
  beneficiaryId: string;
  familyCode: string;
  campaignName: string;
  isDistributed: boolean;
  distributionDate: string;
  amountDistributed: number;
  currency: string;
  receivedBy?: string | null;
  recipientRelationship?: string | null;
  notes?: string | null;
  signatureImageUrl?: string | null;
  attachmentId?: string | null;
  distributionMethod?: string | null;
  distributorName?: string | null;
  distributorRole?: string | null;
  createdOn: string;
  createdBy?: string | null;
}

export interface CreateSeasonalAidDistributionRequest {
  beneficiaryId: string;
  distributionDate: string;
  amountDistributed: number;
  currency: string;
  receivedBy: string;
  recipientRelationship?: string | null;
  notes?: string | null;
  signatureImageUrl?: string | null;
  attachmentId?: string | null;
  distributionMethod?: string | null;
  distributorName?: string | null;
  distributorRole?: string | null;
}

export interface SeasonalAidCampaignReport {
  campaignId: string;
  campaignName: string;
  campaignType: string;
  startDate: string;
  endDate: string;
  description?: string | null;
  totalBudget: number;
  budgetCurrency: string;
  allocatedBudget: number;
  distributedBudget: number;
  remainingBudget: number;
  budgetUtilizationPercentage: number;
  totalBeneficiaries: number;
  distributedBeneficiaries: number;
  pendingBeneficiaries: number;
  beneficiaryDistributionPercentage: number;
  beneficiariesByRegion: Record<string, number>;
  beneficiariesByCharity: Record<string, number>;
  beneficiariesByFamilyType: Record<string, number>;
  coveredCountries: string[];
  coveredRegions: string[];
  coveredCenters: string[];
  estimatedIndividualsServed: number;
  totalOrphansServed: number;
  totalFamiliesServed: number;
  distributionDetails: BeneficiaryDistributionDetail[];
  isActive: boolean;
  isClosed: boolean;
  closedDate?: string | null;
  closureNotes?: string | null;
  reportGeneratedOn: string;
  generatedBy: string;
}

export interface BeneficiaryDistributionDetail {
  beneficiaryId: string;
  familyCode: string;
  familyAddress?: string | null;
  charityName?: string | null;
  regionName?: string | null;
  allocationAmount: number;
  distributedAmount: number;
  isDistributed: boolean;
  distributionDate?: string | null;
  receivedBy?: string | null;
  notes?: string | null;
}

// UC-PRJ-07 quick add: only familyIds are required; the rest fall back to campaign defaults.
export interface RegisterBeneficiariesRequest {
  familyIds: string[];
  allocationAmount?: number;
  currency?: string;
  registrationNotes?: string;
}

// UC-PRJ-07 full sync: familyIds is the desired final set — empty deselects everything.
export interface UpdateBeneficiariesRequest {
  familyIds: string[];
  allocationAmount?: number;
  currency?: string;
  notes?: string;
}

export interface UpdateBeneficiariesResult {
  addedCount: number;
  removedCount: number;
  totalRegistered: number;
  maximumFamilies: number;
}

// UC-PRJ-08: the acting user comes from the token, never from the request.
export interface SetFamilyReceivedFlagRequest {
  campaignId: string;
  isReceived: boolean;
}

export interface CloseCampaignRequest {
  closureNotes?: string;
}

export interface SeasonalAidCampaignFilter {
  searchTerm?: string;
  campaignType?: string;
  isActive?: boolean;
  isClosed?: boolean;
  countryId?: number;
  regionId?: number;
  centerId?: number;
  startDateFrom?: string;
  startDateTo?: string;
  endDateFrom?: string;
  endDateTo?: string;
  pageNumber: number;
  pageSize: number;
  sortBy?: string;
  sortDescending: boolean;
}

/**
 * Register statistics band — the wire shape of CampaignStatisticsDto (caller
 * country-scoped server-side; describes the whole register, not the current search)
 */
export interface SeasonalAidCampaignStatistics {
  total: number;
  /** Campaigns currently marked active */
  active: number;
  /** Campaigns closed via UC-9.9 */
  closed: number;
  /** Campaigns created since the first day of the current month */
  addedThisMonth: number;
}

export interface EligibleFamiliesFilter {
  charityId?: string;
  regionId?: number;
  centerId?: number;
  familyType?: string;
  minChildrenAge?: number;
  maxChildrenAge?: number;
  searchTerm?: string;
  pageNumber: number;
  pageSize: number;
  sortBy?: string;
  sortDescending: boolean;
}

export interface SeasonalAidBeneficiaryFilter {
  isDistributed?: boolean;
  charityId?: string;
  regionId?: number;
  centerId?: number;
  registrationDateFrom?: string;
  registrationDateTo?: string;
  distributionDateFrom?: string;
  distributionDateTo?: string;
  searchTerm?: string;
  pageNumber: number;
  pageSize: number;
  sortBy?: string;
  sortDescending: boolean;
}

export interface SeasonalAidPagedResult<T> extends PagedResponse<T> {}
