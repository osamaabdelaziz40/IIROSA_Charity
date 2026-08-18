export interface SeasonalCampaign {
  id: string;
  campaignName: string;
  campaignType: CampaignType;
  description?: string;
  startDate: Date;
  endDate: Date;
  totalBudget: number;
  budgetCurrency: Currency;
  perFamilyAllocation: number;
  country: string;
  regions: string[];
  centers: string[];
  assignedCharityId?: string;
  assignedCharityName?: string;
  maximumFamilies?: number;
  familyType?: FamilyType;
  ageRangeFrom?: number;
  ageRangeTo?: number;
  isActive: boolean;
  isClosed: boolean;
  closureDate?: Date;
  closureNotes?: string;
  createdDate: Date;
  createdBy: string;
  modifiedDate?: Date;
  modifiedBy?: string;
}

export enum CampaignType {
  Ramadan = 'Ramadan',
  EidAlFitr = 'EidAlFitr',
  EidAlAdha = 'EidAlAdha',
  Winter = 'Winter',
  SchoolSupplies = 'SchoolSupplies',
  Other = 'Other'
}

export enum Currency {
  EGP = 'EGP',
  SAR = 'SAR',
  USD = 'USD'
}

export enum FamilyType {
  All = 'All',
  OrphanFamilies = 'OrphanFamilies',
  NeedyFamilies = 'NeedyFamilies'
}

export interface CampaignBeneficiary {
  id: string;
  campaignId: string;
  familyId: string;
  familyCode: string;
  familyAddress: string;
  charityId: string;
  charityName: string;
  region: string;
  center: string;
  orphanCount: number;
  familyType: string;
  allocatedAmount: number;
  distributionStatus: DistributionStatus;
  registrationDate: Date;
  registeredBy: string;
}

export enum DistributionStatus {
  Pending = 'Pending',
  Distributed = 'Distributed',
  Cancelled = 'Cancelled'
}

export interface AidDistribution {
  id: string;
  campaignBeneficiaryId: string;
  campaignId: string;
  familyId: string;
  familyCode: string;
  distributionDate: Date;
  amountDistributed: number;
  receivedBy: string;
  notes?: string;
  signatureUrl?: string;
  attachmentIds?: string[];
  distributedBy: string;
  createdAt: Date;
}

export interface CampaignStatistics {
  campaignId: string;
  totalBeneficiaries: number;
  distributedBeneficiaries: number;
  pendingBeneficiaries: number;
  totalBudget: number;
  allocatedAmount: number;
  distributedAmount: number;
  remainingBudget: number;
  averageDistribution: number;
}

export interface CampaignReport {
  campaign: SeasonalCampaign;
  statistics: CampaignStatistics;
  beneficiariesByRegion: RegionStatistics[];
  beneficiariesByCharity: CharityStatistics[];
  beneficiariesByFamilyType: FamilyTypeStatistics[];
  distributions: AidDistribution[];
}

export interface RegionStatistics {
  region: string;
  totalBeneficiaries: number;
  distributedBeneficiaries: number;
  totalAmount: number;
  distributedAmount: number;
}

export interface CharityStatistics {
  charityId: string;
  charityName: string;
  totalBeneficiaries: number;
  distributedBeneficiaries: number;
  totalAmount: number;
  distributedAmount: number;
}

export interface FamilyTypeStatistics {
  familyType: string;
  totalBeneficiaries: number;
  distributedBeneficiaries: number;
  totalAmount: number;
  distributedAmount: number;
}

export interface BeneficiarySelection {
  familyId: string;
  familyCode: string;
  familyAddress: string;
  charityId: string;
  charityName: string;
  region: string;
  center: string;
  orphanCount: number;
  familyType: string;
  isSelected: boolean;
}

export interface CampaignFilter {
  campaignName?: string;
  campaignType?: CampaignType;
  status?: 'all' | 'active' | 'closed';
  charityId?: string;
  startDateFrom?: Date;
  startDateTo?: Date;
  endDateFrom?: Date;
  endDateTo?: Date;
}

export interface BeneficiaryFilter {
  charityId?: string;
  region?: string;
  center?: string;
  familyType?: FamilyType;
  distributionStatus?: DistributionStatus;
  searchTerm?: string;
}
