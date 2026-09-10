export interface AttachmentDto {
  id?: string;
  fileName?: string;
  extension?: string;
  filePath?: string;
  contentType?: string;
  size?: number;
  fileContent?: any;
  fileData?: any;
  thumbnail?: string;
  isNew?: boolean;
  isDeleted?: boolean;
  description?: string;
  createdOn?: string | null;
}

export interface CharityDto {
  id: string;
  code: string;
  name: string;
  // nameAr: string;
  // nameEn: string;
  ngoType: string;
  address: string;
  streetName: string;
  village: string;
  city: string;
  postalCode: string;
  mailBox: string;
  countryId: number;
  countryName?: string;
  phone: string;
  phone2: string;
  homePhone: string;
  fax: string;
  email: string;
  regionId: number;
  regionName?: string;
  centerId: number;
  centerName?: string;
  ngoMapLocation: string;
  bankId: number;
  bankName?: string;
  bankAccount: string;
  iban: string;
  bossName: string;
  bossJobName: string;
  bossPhone1: string;
  bossPhone2: string;
  responsibleJobName: string;
  responsiblePhone1: string;
  responsiblePhone2: string;
  iconId?: string;
  icon_Attach?: AttachmentDto[];
  receivingDonations: boolean;
  notes: string;
  isActive: boolean;
  isLocked: boolean;
  isAddEnabled: boolean;
  isUpdateEnabled: boolean;
  createdOn: Date;
  modifiedOn?: Date;
  createdBy?: string;
  modifiedBy?: string;
  lastLogin?: Date;
  familyCount?: number;
  orphanCount?: number;
  userId?: string;
  username?: string;
  password?: string;
}

export interface CreateCharityDto {
  code: string;
  name: string;
  // nameAr: string;
  // nameEn: string;
  ngoType: string;
  address: string;
  streetName: string;
  village: string;
  // city: string;
  postalCode: string;
  mailBox: string;
  countryId: number;
  phone: string;
  phone2: string;
  homePhone: string;
  fax: string;
  email: string;
  regionId: number;
  centerId: number;
  ngoMapLocation: string;
  bankId: number;
  bankAccount: string;
  iban: string;
  bossName: string;
  bossJobName: string;
  bossPhone1: string;
  bossPhone2: string;
  responsibleJobName: string;
  responsiblePhone1: string;
  responsiblePhone2: string;
  icon_Attach?: AttachmentDto[];
  receivingDonations: boolean;
  notes: string;
  createUserAccount: boolean;
  username?: string;
  password?: string;
}

export interface UpdateCharityDto {
  code: string;
  name: string;
  // nameAr: string;
  // nameEn: string;
  ngoType: string;
  address: string;
  streetName: string;
  village: string;
  // city: string;
  postalCode: string;
  mailBox: string;
  countryId: number;
  phone: string;
  phone2: string;
  homePhone: string;
  fax: string;
  email: string;
  regionId: number;
  centerId: number;
  ngoMapLocation: string;
  bankId: number;
  bankAccount: string;
  iban: string;
  bossName: string;
  bossJobName: string;
  bossPhone1: string;
  bossPhone2: string;
  responsibleJobName: string;
  responsiblePhone1: string;
  responsiblePhone2: string;
  icon_Attach?: AttachmentDto[];
  receivingDonations: boolean;
  notes: string;
  isAddEnabled?: boolean;
  isUpdateEnabled?: boolean;
  createUserAccount?: boolean;
  username?: string;
  password?: string;
}

export interface CharitySearchRequest {
  searchTerm?: string;
  countryId?: number;
  regionId?: number;
  centerId?: number;
  isActive?: boolean;
  isLocked?: boolean;
  isAddEnabled?: boolean;
  isUpdateEnabled?: boolean;
  pageNumber?: number;
  pageSize?: number;
}

export interface CharityPagedResult {
  items: CharityDto[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
}

export interface CharityAuditLog {
  id: string;
  charityId: string;
  charityName: string;
  action: string;
  field?: string;
  oldValue?: string;
  newValue?: string;
  performedBy: string;
  performedOn: Date;
  ipAddress?: string;
}

export interface PasswordResetDto {
  charityId: string;
  newPassword?: string;
  sendEmail: boolean;
}

export interface CharityStatusUpdateDto {
  charityId: string;
  isActive?: boolean;
  isLocked?: boolean;
  isAddEnabled?: boolean;
  isUpdateEnabled?: boolean;
}

export interface CharityCredentialsDto {
  username: string;
  password: string;
  email: string;
  temporaryPassword: boolean;
}

/**
 * Result of GET /api/Charities/check-name (UC-CHR-02).
 *
 * Carries the name back with the answer so a reply that arrives after the user has typed on can
 * be recognised as stale and discarded.
 */
export interface CharityNameAvailability {
  name: string;
  isAvailable: boolean;
}

/** One row of the by-country breakdown — pick nameAr/nameEn by current language. */
export interface CharityCountryStatistics {
  countryId: number;
  nameAr?: string | null;
  nameEn?: string | null;
  count: number;
}

/**
 * Register statistics band above the all-charities grid (UC-CHR-01).
 * Caller-scoped server-side: a charity user gets their own record's counts.
 */
export interface CharityStatistics {
  totalCharities: number;
  activeCharities: number;
  inactiveCharities: number;
  lockedCharities: number;
  receivingDonations: number;
  addedThisMonth: number;
  byCountry: CharityCountryStatistics[];
}
