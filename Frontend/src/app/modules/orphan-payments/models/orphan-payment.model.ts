/**
 * Orphan Payments Module Models
 * Handles orphan payment groups/batches for manual payment processing
 */

export interface OrphanPaymentDto {
  id: string;
  batchNo: string;
  groupName: string;
  description?: string;
  paymentPeriodFrom: string;
  paymentPeriodTo: string;
  groupDate: string;
  exchangeRate: number;
  currency: string;
  dontRemoveRate: boolean;
  charityId?: number;
  charityName?: string;
  regionId?: number;
  regionName?: string;
  centerId?: number;
  centerName?: string;
  sponsorshipStatus?: string;
  ageFrom?: number;
  ageTo?: number;
  showOrder: number;
  notes?: string;
  isBatchUploaded: boolean;
  uploadDate?: string;
  orphanCount: number;
  createdOn: string;
  modifiedOn?: string;
  createdBy?: string;
  modifiedBy?: string;
  orphans?: OrphanPaymentItemDto[];
}

export interface OrphanPaymentItemDto {
  id: string;
  orphanPaymentId: string;
  orphanId: string;
  orphanName: string;
  familyName: string;
  charityName: string;
  regionName?: string;
  centerName?: string;
  age: number;
  gender: string;
  sponsorshipStatus: string;
  monthlyAmount?: number;
  currency?: string;
  assignedOn: string;
  assignedBy?: string;
  notes?: string;
}

export interface CreateOrphanPaymentDto {
  batchNo?: string;
  groupName: string;
  description?: string;
  paymentPeriodFrom: string;
  paymentPeriodTo: string;
  groupDate?: string;
  exchangeRate?: number;
  currency?: string;
  dontRemoveRate?: boolean;
  charityId?: number;
  regionId?: number;
  centerId?: number;
  sponsorshipStatus?: string;
  ageFrom?: number;
  ageTo?: number;
  showOrder?: number;
  notes?: string;
}

export interface UpdateOrphanPaymentDto {
  batchNo?: string;
  groupName?: string;
  description?: string;
  paymentPeriodFrom?: string;
  paymentPeriodTo?: string;
  groupDate?: string;
  exchangeRate?: number;
  currency?: string;
  dontRemoveRate?: boolean;
  charityId?: number;
  regionId?: number;
  centerId?: number;
  sponsorshipStatus?: string;
  ageFrom?: number;
  ageTo?: number;
  showOrder?: number;
  notes?: string;
}

export interface OrphanPaymentSearchRequest {
  searchTerm?: string;
  charityId?: number;
  regionId?: number;
  centerId?: number;
  isBatchUploaded?: boolean;
  paymentPeriodFrom?: string;
  paymentPeriodTo?: string;
  groupDateFrom?: string;
  groupDateTo?: string;
  pageNumber?: number;
  pageSize?: number;
  sortBy?: string;
  sortDescending?: boolean;
}

export interface OrphanPaymentPagedResult {
  items: OrphanPaymentDto[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
}

export interface AddOrphansToPaymentDto {
  orphanIds: string[];
}

export interface RemoveOrphanFromPaymentDto {
  orphanId: string;
}

export interface OrphanSelectionFilter {
  charityId?: number;
  regionId?: number;
  centerId?: number;
  sponsorshipStatus?: string;
  ageFrom?: number;
  ageTo?: number;
  gender?: string;
  searchTerm?: string;
}

export interface OrphanForSelectionDto {
  orphanId: string;
  orphanName: string;
  familyName: string;
  charityName: string;
  regionName?: string;
  centerName?: string;
  age: number;
  gender: string;
  sponsorshipStatus: string;
  monthlyAmount?: number;
  isAlreadyInGroup: boolean;
}

export interface ExportPaymentGroupOptions {
  format: 'Excel' | 'PDF';
  includePhotos: boolean;
  groupBy?: 'Charity' | 'Region' | 'None';
}

export interface OrphanPaymentAuditLog {
  id: string;
  orphanPaymentId: string;
  groupName: string;
  action: string;
  field?: string;
  oldValue?: string;
  newValue?: string;
  performedBy: string;
  performedOn: string;
  ipAddress?: string;
}

export interface OrphanPaymentStatistics {
  totalGroups: number;
  activeGroups: number;
  uploadedGroups: number;
  totalOrphansInGroups: number;
  byCharity: {
    charityId: number;
    charityName: string;
    groupCount: number;
    orphanCount: number;
  }[];
  byRegion: {
    regionId: number;
    regionName: string;
    groupCount: number;
    orphanCount: number;
  }[];
}

export interface BatchNumberGeneration {
  nextBatchNumber: string;
  currentHighestBatch: string;
}

// Currency options
export const CURRENCY_OPTIONS = [
  { value: 'EGP', label: 'EGP - Egyptian Pound' },
  { value: 'SAR', label: 'SAR - Saudi Riyal' },
  { value: 'USD', label: 'USD - US Dollar' }
];

// Sponsorship status options
export const SPONSORSHIP_STATUS_OPTIONS = [
  { value: 'All', label: 'All' },
  { value: 'Sponsored', label: 'Sponsored' },
  { value: 'Unsponsored', label: 'Unsponsored' }
];

// Gender options
export const GENDER_OPTIONS = [
  { value: 'All', label: 'All' },
  { value: 'Male', label: 'Male' },
  { value: 'Female', label: 'Female' }
];

// Export group by options
export const GROUP_BY_OPTIONS = [
  { value: 'None', label: 'No Grouping' },
  { value: 'Charity', label: 'By Charity' },
  { value: 'Region', label: 'By Region' }
];
