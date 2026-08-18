// ==================== Common Interfaces ====================

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

// ==================== Family Models ====================

export interface FamilyDto {
  id: string;
  code: string;
  charityId: string;
  charityName?: string;
  address: string;
  city?: string;
  village?: string;
  district?: string;
  phone?: string;
  livingCondition?: string;
  housingType?: string;
  providerType?: string;
  registrationDate: string;
  notes?: string;
  isActive: boolean;
  isDeleted: boolean;
  createdOn: string;
  modifiedOn?: string;
  createdBy?: string;
  modifiedBy?: string;

  // Related entities
  father?: FatherDto;
  mother?: MotherDto;
  provider?: ProviderDto;
  relatives?: RelativeDto[];
  orphans?: OrphanDto[];
  orphanCount?: number;
  relativeCount?: number;
  attachments?: AttachmentDto[];
}

export interface CreateFamilyDto {
  code?: string;
  charityId?: string;
  address: string;
  city?: string;
  village?: string;
  district?: string;
  phone?: string;
  livingCondition?: string;
  housingType?: string;
  providerType?: string;
  registrationDate?: string;
  notes?: string;
  father: CreateFatherDto; // Mandatory
  mother: CreateMotherDto; // Mandatory
  provider?: CreateProviderDto;
  relatives?: CreateRelativeDto[]; // Optional
  orphans?: CreateOrphanDto[];
}

export interface UpdateFamilyDto {
  code?: string;
  address: string;
  city?: string;
  village?: string;
  district?: string;
  phone?: string;
  livingCondition?: string;
  housingType?: string;
  providerType?: string;
  notes?: string;
}

export interface FamilySearchRequest {
  searchTerm?: string;
  charityId?: string;
  orphanCountMin?: number;
  orphanCountMax?: number;
  providerType?: string;
  livingCondition?: string;
  housingType?: string;
  registrationDateFrom?: string;
  registrationDateTo?: string;
  isActive?: boolean;
  pageNumber?: number;
  pageSize?: number;
}

export interface FamilyPagedResult {
  items: FamilyDto[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
}

// ==================== Father Models ====================

export interface FatherDto {
  id: string;
  familyId: string;
  fullName: string;
  nationalId?: string;
  passportNumber?: string;
  dateOfBirth?: string;
  placeOfBirth?: string;
  educationLevel?: string;
  job?: string;
  monthlyIncome?: number;
  healthStatus?: string;
  phone?: string;
  isAlive: boolean;
  isProvider: boolean;
  deathDate?: string;
  notes?: string;
  createdOn: string;
  modifiedOn?: string;
}

export interface CreateFatherDto {
  familyId?: string;
  fullName: string;
  nationalId: string; // Required
  passportNumber?: string;
  dateOfBirth: string; // Required
  placeOfBirth?: string;
  educationLevel?: string;
  job?: string;
  monthlyIncome?: number;
  healthStatus?: string;
  phone?: string;
  isAlive: boolean;
  isProvider: boolean;
  deathDate?: string;
  notes?: string;
}

export interface UpdateFatherDto {
  fullName: string;
  nationalId?: string;
  passportNumber?: string;
  dateOfBirth?: string;
  placeOfBirth?: string;
  educationLevel?: string;
  job?: string;
  monthlyIncome?: number;
  healthStatus?: string;
  phone?: string;
  isAlive: boolean;
  isProvider: boolean;
  deathDate?: string;
  notes?: string;
}

// ==================== Mother Models ====================

export interface MotherDto {
  id: string;
  familyId: string;
  fullName: string;
  nationalId?: string;
  passportNumber?: string;
  dateOfBirth?: string;
  placeOfBirth?: string;
  educationLevel?: string;
  job?: string;
  monthlyIncome?: number;
  healthStatus?: string;
  phone?: string;
  isAlive: boolean;
  isProvider: boolean;
  deathDate?: string;
  notes?: string;
  createdOn: string;
  modifiedOn?: string;
}

export interface CreateMotherDto {
  familyId?: string;
  fullName: string;
  nationalId: string; // Required
  passportNumber?: string;
  dateOfBirth: string; // Required
  placeOfBirth?: string;
  educationLevel?: string;
  job?: string;
  monthlyIncome?: number;
  healthStatus?: string;
  phone?: string;
  isAlive: boolean;
  isProvider: boolean;
  deathDate?: string;
  notes?: string;
}

export interface UpdateMotherDto {
  fullName: string;
  nationalId?: string;
  passportNumber?: string;
  dateOfBirth?: string;
  placeOfBirth?: string;
  educationLevel?: string;
  job?: string;
  monthlyIncome?: number;
  healthStatus?: string;
  phone?: string;
  isAlive: boolean;
  isProvider: boolean;
  deathDate?: string;
  notes?: string;
}

// ==================== Provider Models ====================

export interface ProviderDto {
  id: string;
  familyId: string;
  fullName: string;
  relationship?: string;
  nationalId?: string;
  passportNumber?: string;
  phone?: string;
  address?: string;
  job?: string;
  monthlyIncome?: number;
  notes?: string;
  createdOn: string;
  modifiedOn?: string;
}

export interface CreateProviderDto {
  familyId?: string;
  fullName: string;
  relationship?: string;
  nationalId?: string;
  passportNumber?: string;
  phone?: string;
  address?: string;
  job?: string;
  monthlyIncome?: number;
  notes?: string;
}

export interface UpdateProviderDto {
  fullName: string;
  relationship?: string;
  nationalId?: string;
  passportNumber?: string;
  phone?: string;
  address?: string;
  job?: string;
  monthlyIncome?: number;
  notes?: string;
}

// ==================== Relative Models ====================

export interface RelativeDto {
  id: string;
  familyId: string;
  fullName: string;
  relationshipType: string;
  gender: string;
  dateOfBirth: string;
  placeOfBirth?: string;
  nationalId?: string;
  educationLevel?: string;
  educationLevelId?: number;
  job?: string;
  monthlyIncome?: number;
  healthStatus?: string;
  healthStatusId?: number;
  phone?: string;
  address?: string;
  isAlive: boolean;
  isLivingWithFamily: boolean;
  deathDate?: string;
  notes?: string;
  age?: number;
  createdOn: string;
  modifiedOn?: string;
  createdBy?: string;
  modifiedBy?: string;
}

export interface CreateRelativeDto {
  familyId?: string;
  fullName: string;
  relationshipType: string;
  gender: string;
  dateOfBirth: string;
  placeOfBirth?: string;
  nationalId?: string;
  educationLevelId?: number;
  job?: string;
  monthlyIncome?: number;
  healthStatusId?: number;
  phone?: string;
  address?: string;
  isAlive: boolean;
  isLivingWithFamily: boolean;
  deathDate?: string;
  notes?: string;
}

export interface UpdateRelativeDto {
  id: string;
  fullName?: string;
  relationshipType?: string;
  gender?: string;
  dateOfBirth?: string;
  placeOfBirth?: string;
  nationalId?: string;
  educationLevelId?: number;
  job?: string;
  monthlyIncome?: number;
  healthStatusId?: number;
  phone?: string;
  address?: string;
  isAlive?: boolean;
  isLivingWithFamily?: boolean;
  deathDate?: string;
  notes?: string;
}

export interface RelativeListDto {
  id: string;
  familyId: string;
  fullName: string;
  relationshipType: string;
  gender: string;
  dateOfBirth: string;
  age?: number;
  isAlive: boolean;
  isLivingWithFamily: boolean;
  phone?: string;
  isActive: boolean;
}

// ==================== Orphan Models ====================

export interface OrphanDto {
  id: string;
  familyId: string;
  charityId: string;
  fullName: string;
  gender: string;
  dateOfBirth: string;
  placeOfBirth?: string;
  nationalId?: string;
  passportNumber?: string;
  photoId?: string;
  photo_Attach?: AttachmentDto[];
  orphanType?: string;
  sponsorshipStatus?: string;
  sponsorshipStartDate?: string;
  educationLevel?: string;
  schoolName?: string;
  grade?: string;
  academicPerformance?: string;
  healthStatus?: string;
  disabilities?: string;
  chronicDiseases?: string;
  phone?: string;
  email?: string;
  hobbies?: string;
  skills?: string;
  notes?: string;
  age?: number;
  sponsorId?: string;
  sponsorName?: string;
  isActive: boolean;
  createdOn: string;
  modifiedOn?: string;
  createdBy?: string;
  modifiedBy?: string;
}

export interface CreateOrphanDto {
  familyId?: string;
  charityId?: string;
  fullName: string;
  gender: string;
  dateOfBirth: string;
  placeOfBirth?: string;
  nationalId?: string;
  passportNumber?: string;
  photo_Attach?: AttachmentDto[];
  orphanType?: string;
  sponsorshipStatus?: string;
  sponsorshipStartDate?: string;
  educationLevel?: string;
  schoolName?: string;
  grade?: string;
  academicPerformance?: string;
  healthStatus?: string;
  disabilities?: string;
  chronicDiseases?: string;
  phone?: string;
  email?: string;
  hobbies?: string;
  skills?: string;
  notes?: string;
}

export interface UpdateOrphanDto {
  fullName: string;
  gender: string;
  dateOfBirth: string;
  placeOfBirth?: string;
  nationalId?: string;
  passportNumber?: string;
  photo_Attach?: AttachmentDto[];
  orphanType?: string;
  sponsorshipStatus?: string;
  sponsorshipStartDate?: string;
  educationLevel?: string;
  schoolName?: string;
  grade?: string;
  academicPerformance?: string;
  healthStatus?: string;
  disabilities?: string;
  chronicDiseases?: string;
  phone?: string;
  email?: string;
  hobbies?: string;
  skills?: string;
  notes?: string;
}

export interface OrphanSearchRequest {
  searchTerm?: string;
  familyId?: string;
  charityId?: string;
  gender?: string;
  orphanType?: string;
  sponsorshipStatus?: string;
  ageMin?: number;
  ageMax?: number;
  educationLevel?: string;
  healthStatus?: string;
  isActive?: boolean;
  pageNumber?: number;
  pageSize?: number;
}

export interface OrphanPagedResult {
  items: OrphanDto[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
}

// ==================== Family Attachment Models ====================

export interface FamilyAttachmentDto {
  id: string;
  familyId: string;
  fileName: string;
  filePath: string;
  documentType: string;
  description?: string;
  uploadedBy?: string;
  uploadedOn: string;
  fileSize?: number;
  contentType?: string;
}

export interface CreateFamilyAttachmentDto {
  familyId: string;
  fileName: string;
  filePath: string;
  documentType: string;
  description?: string;
}

// ==================== Audit Log ====================

export interface FamilyAuditLog {
  id: string;
  familyId: string;
  familyCode: string;
  action: string;
  field?: string;
  oldValue?: string;
  newValue?: string;
  performedBy: string;
  performedOn: Date;
  ipAddress?: string;
}
