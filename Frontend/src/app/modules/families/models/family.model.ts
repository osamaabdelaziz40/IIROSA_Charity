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

// ==================== Family Phones (§4 multi phone) ====================

/** Read row — GET /api/Families/{id} phones[] */
export interface FamilyPhoneDto {
  id: string;
  familyId: string;
  number: string;
  isDefault: boolean;
}

/** Write row — one أرقام التواصل entry, exactly one flagged default per family */
export interface CreateFamilyPhoneDto {
  number: string;
  isDefault: boolean;
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

  // Refugee register household fields (epic 7, §12.S.2) — wire names, resolved names included
  familyType?: string;
  cityVillage?: string;
  districtArea?: string;
  phoneNumber?: string;
  countryId?: number;
  countryName?: string;
  regionId?: number;
  regionName?: string;
  centerId?: number;
  centerName?: string;
  nearBy?: string;
  street?: string;
  rentAmount?: number;
  houseOwnershipId?: number;
  houseOwnershipName?: string;
  houseStatusId?: number;
  houseStatusName?: string;
  /** نوع السكن — shared catalogue id + resolved name (GET /api/Families/{id}, UC-REF-04) */
  housingTypeId?: number;
  housingTypeName?: string;
  incomeTypeId?: number;
  incomeTypeName?: string;
  perMemberShare?: number;

  // Family data extension (§4 معلومات الأسرة) — shared across registers
  incomeValue?: number;
  totalIncome?: number;
  childrenCount?: number;
  hasProject?: boolean;
  familyProjectStatusId?: number;
  familyProjectStatusName?: string;
  familyMembersCount?: number;
  monthlyIncome?: number;
  phones?: FamilyPhoneDto[];

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
  /** Mandatory for the Regular register (service-enforced); a refugee family has neither */
  father?: CreateFatherDto;
  /** Mandatory for the Regular register (service-enforced); a refugee family has neither */
  mother?: CreateMotherDto;
  provider?: CreateProviderDto;
  relatives?: CreateRelativeDto[]; // Optional
  orphans?: CreateOrphanDto[];
  // Refugee register household fields (epic 7, §12.S.2) — string on the wire, absent ⇒ Regular
  familyType?: string;
  cityVillage?: string;
  districtArea?: string;
  phoneNumber?: string;
  countryId?: number;
  regionId?: number;
  centerId?: number;
  nearBy?: string;
  street?: string;
  rentAmount?: number;
  houseOwnershipId?: number;
  houseStatusId?: number;
  housingTypeId?: number;
  incomeTypeId?: number;
  /** إسم الأسرة — derived from the provider's name on the refugee register (§12.S.2) */
  headOfFamily?: string;
  // Family data extension (§4 معلومات الأسرة) — stamped on every register
  incomeValue?: number;
  totalIncome?: number;
  childrenCount?: number;
  hasProject?: boolean;
  familyProjectStatusId?: number;
  /** أرقام التواصل — complete live set; exactly one row flagged isDefault */
  phones?: CreateFamilyPhoneDto[];
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
  // Refugee register household fields (epic 7, §12.S.2)
  cityVillage?: string;
  districtArea?: string;
  phoneNumber?: string;
  regionId?: number;
  centerId?: number;
  nearBy?: string;
  street?: string;
  rentAmount?: number;
  houseOwnershipId?: number;
  houseStatusId?: number;
  housingTypeId?: number;
  incomeTypeId?: number;
  // Family data extension (§4 معلومات الأسرة) — patch-style, absent ⇒ unchanged
  countryId?: number;
  incomeValue?: number;
  totalIncome?: number;
  childrenCount?: number;
  hasProject?: boolean;
  familyProjectStatusId?: number;
  /** أرقام التواصل — full-replace sync; absent ⇒ phones untouched */
  phones?: CreateFamilyPhoneDto[];
}

// List-item shape actually returned by GET /api/Families (FamilyListDto on the wire)
export interface FamilyListItemDto {
  id: string;
  code: string;
  address: string;
  cityVillage?: string;
  fatherName?: string;
  motherName?: string;
  orphansCount: number;
  relativesCount: number;
  providerType?: string;
  registrationDate: string;
  isActive: boolean;
  familyType?: string;
  phoneNumber?: string;
  charityName?: string;
  /** Holding-family marker (ruling 2026-08-24): rows created only to hold a member detached
   *  by member control — not a register family. */
  isHoldingFamily?: boolean;
}

export interface FamilySearchRequest {
  searchTerm?: string;
  charityId?: string;
  familyType?: string;
  /** Typed search selector — the shared FamilyFilterDto.SearchType vocabulary: all |
   *  father | mother | student (orphan name) | provider | nationalId | code (orphan code) | phone */
  searchType?: string;
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

// ==================== Guardian Change Request Models (UC-FAM-09/10) ====================

/** Review-queue workflow status — mirrors GuardianChangeRequestStatus (Domain enum). */
export const GUARDIAN_REQUEST_STATUS = {
  Pending: 1,
  Approved: 2,
  Rejected: 3
} as const;

/** One §10.S.4 review-queue row — old/new guardian snapshots travel with the row. */
export interface GuardianChangeRequestRow {
  id: string;
  charityName?: string;
  familyId: string;
  familyCode?: string;
  orphanCode?: string;
  orphanName?: string;
  motherName?: string;
  oldGuardianName?: string;
  oldGuardianNationalId?: string;
  /** Old guardian's relationship snapshot — declared relationship, or Father/Mother for
   *  parent-designated families (DoD §10.U.09, landed 2026-08-24). */
  oldGuardianRelationship?: string;
  newGuardianName: string;
  newGuardianNationalId: string;
  relationship: string;
  reason: string;
  requestedByName: string;
  createdOn: string;
  status: number;
  decidedBy?: string | null;
  decidedOn?: string | null;
  rejectionReason?: string | null;
}

export interface GuardianChangeRequestPagedResult {
  items: GuardianChangeRequestRow[];
  totalCount: number;
}

/** Raise payload — the proposed new guardian snapshot plus the reason. */
export interface CreateGuardianChangeRequest {
  newGuardianName: string;
  newGuardianNationalId: string;
  relationship: string;
  reason: string;
}

/** UC-FAM-10 decision payload — a refusal is not accepted without a reason. */
export interface DecideGuardianChangeRequest {
  isApproved: boolean;
  rejectionReason?: string;
}

// ==================== Father Models ====================

export interface FatherDto {
  id: string;
  familyId: string;
  fullName: string;
  /** §10 اضافة معيل — legacy four-part name */
  firstName?: string;
  secondName?: string;
  thirdName?: string;
  familyName?: string;
  nationalId?: string;
  passportNumber?: string;
  /** الجنسية — Country lookup id */
  nationalityCountryId?: number;
  nationalityName?: string;
  dateOfBirth?: string;
  placeOfBirth?: string;
  educationLevel?: string;
  job?: string;
  monthlyIncome?: number;
  healthStatus?: string;
  /** الحالة الصحية — HealthStatus lookup id */
  healthStatusId?: number;
  healthStatusName?: string;
  phone?: string;
  isAlive: boolean;
  isProvider: boolean;
  deathDate?: string;
  /** سبب الوفاة — DeathReason lookup id */
  deathReasonId?: number;
  deathReasonName?: string;
  /** صوره شهاده الوفاه — server attachment id */
  deathCertificateAttachmentId?: string;
  mezaCard?: string;
  mezaCardExpirationDate?: string;
  notes?: string;
  createdOn: string;
  modifiedOn?: string;
}

export interface CreateFatherDto {
  familyId?: string;
  fullName: string;
  firstName?: string;
  secondName?: string;
  thirdName?: string;
  familyName?: string;
  nationalId: string; // Required
  nationalityCountryId?: number;
  passportNumber?: string;
  dateOfBirth: string; // Required
  placeOfBirth?: string;
  educationLevel?: string;
  job?: string;
  monthlyIncome?: number;
  healthStatus?: string;
  healthStatusId?: number;
  phone?: string;
  isAlive: boolean;
  isProvider: boolean;
  deathDate?: string;
  deathReasonId?: number;
  deathCertificateAttachmentId?: string;
  mezaCard?: string;
  mezaCardExpirationDate?: string;
  notes?: string;
}

export interface UpdateFatherDto {
  /** Route-bound — PUT /api/Families/father/{id} */
  id?: string;
  fullName: string;
  firstName?: string;
  secondName?: string;
  thirdName?: string;
  familyName?: string;
  nationalId?: string;
  nationalityCountryId?: number;
  passportNumber?: string;
  dateOfBirth?: string;
  placeOfBirth?: string;
  educationLevel?: string;
  job?: string;
  monthlyIncome?: number;
  healthStatus?: string;
  healthStatusId?: number;
  phone?: string;
  isAlive: boolean;
  isProvider: boolean;
  deathDate?: string;
  deathReasonId?: number;
  deathCertificateAttachmentId?: string;
  mezaCard?: string;
  mezaCardExpirationDate?: string;
  notes?: string;
}

// ==================== Mother Models ====================

export interface MotherDto {
  id: string;
  familyId: string;
  fullName: string;
  /** §10 اضافة معيل — legacy four-part name */
  firstName?: string;
  secondName?: string;
  thirdName?: string;
  familyName?: string;
  nationalId?: string;
  passportNumber?: string;
  /** الجنسية — Country lookup id */
  nationalityCountryId?: number;
  nationalityName?: string;
  dateOfBirth?: string;
  placeOfBirth?: string;
  educationLevel?: string;
  job?: string;
  monthlyIncome?: number;
  healthStatus?: string;
  /** الحالة الصحية — HealthStatus lookup id */
  healthStatusId?: number;
  healthStatusName?: string;
  phone?: string;
  isAlive: boolean;
  isProvider: boolean;
  deathDate?: string;
  /** سبب الوفاة — DeathReason lookup id */
  deathReasonId?: number;
  deathReasonName?: string;
  /** صوره شهاده الوفاه — server attachment id */
  deathCertificateAttachmentId?: string;
  mezaCard?: string;
  mezaCardExpirationDate?: string;
  notes?: string;
  createdOn: string;
  modifiedOn?: string;
}

export interface CreateMotherDto {
  familyId?: string;
  fullName: string;
  firstName?: string;
  secondName?: string;
  thirdName?: string;
  familyName?: string;
  nationalId: string; // Required
  nationalityCountryId?: number;
  passportNumber?: string;
  dateOfBirth: string; // Required
  placeOfBirth?: string;
  educationLevel?: string;
  job?: string;
  monthlyIncome?: number;
  healthStatus?: string;
  healthStatusId?: number;
  phone?: string;
  isAlive: boolean;
  isProvider: boolean;
  deathDate?: string;
  deathReasonId?: number;
  deathCertificateAttachmentId?: string;
  mezaCard?: string;
  mezaCardExpirationDate?: string;
  notes?: string;
}

export interface UpdateMotherDto {
  /** Route-bound — PUT /api/Families/mother/{id} */
  id?: string;
  fullName: string;
  firstName?: string;
  secondName?: string;
  thirdName?: string;
  familyName?: string;
  nationalId?: string;
  nationalityCountryId?: number;
  passportNumber?: string;
  dateOfBirth?: string;
  placeOfBirth?: string;
  educationLevel?: string;
  job?: string;
  monthlyIncome?: number;
  healthStatus?: string;
  healthStatusId?: number;
  phone?: string;
  isAlive: boolean;
  isProvider: boolean;
  deathDate?: string;
  deathReasonId?: number;
  deathCertificateAttachmentId?: string;
  mezaCard?: string;
  mezaCardExpirationDate?: string;
  notes?: string;
}

// ==================== Provider Models ====================

export interface ProviderDto {
  id: string;
  familyId: string;
  fullName: string;
  relationship?: string;
  /** نوعها — free-text relation (wire name; resolved against the Relation catalogue on edit) */
  relationshipToFamily?: string;
  /** العلاقة — closed set: الاب | الام | علاقة أخرى (epic-7 review P14) */
  mainRelation?: string;
  nationalId?: string;
  passportNumber?: string;
  phone?: string;
  address?: string;
  job?: string;
  monthlyIncome?: number;
  notes?: string;
  // Refugee register extensions (epic 7, §12.S.2 اضافة معيل)
  dateOfBirth?: string;
  nationalityCountryId?: number;
  isAlive?: boolean;
  deathDate?: string;
  /** Closed set: طبيعية / مرض / حادث (static list on the form, not a lookup) */
  deathReason?: string;
  reasonOfRelationId?: number;
  reasonOfRelationName?: string;
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
  // Refugee register extensions (epic 7, §12.S.2 اضافة معيل)
  /** نوعها — Relation catalogue label; travels as the free-text relation on the wire */
  relationshipToFamily?: string;
  /** العلاقة — closed set: الاب | الام | علاقة أخرى (epic-7 review P14) */
  mainRelation?: string;
  dateOfBirth?: string;
  nationalityCountryId?: number;
  isAlive?: boolean;
  deathDate?: string;
  deathReason?: string;
  reasonOfRelationId?: number;
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
  // Refugee register extensions (epic 7, §12.S.2 اضافة معيل — edit mode, UC-REF-04)
  relationshipToFamily?: string;
  /** العلاقة — closed set: الاب | الام | علاقة أخرى (epic-7 review P14) */
  mainRelation?: string;
  dateOfBirth?: string;
  nationalityCountryId?: number;
  isAlive?: boolean;
  deathDate?: string;
  deathReason?: string;
  reasonOfRelationId?: number;
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
  // Refugee register extensions (epic 7, UC-REF-04) — reload staged مرافق rows without
  // losing the NID / free-text صلة القرابة
  nationalId?: string;
  notes?: string;
  /** الحالة الصحية — HealthStatus lookup id + resolved name (epic-7 review P12) */
  healthStatusId?: number;
  healthStatusName?: string;
}

// ==================== Orphan Models ====================

export interface OrphanDto {
  id: string;
  familyId: string;
  charityId: string;
  code?: string;
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
  /** الحالة الصحية — HealthStatus lookup id + resolved name (epic-7 review P12) */
  healthStatusId?: number;
  healthStatusName?: string;
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
  // Refugee register extension (epic 7, §12.S.2 اضافة ابن)
  socialStatusId?: number;
  socialStatusName?: string;
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
  /** الحالة الصحية — HealthStatus lookup id (epic-7 review P12) */
  healthStatusId?: number;
  disabilities?: string;
  chronicDiseases?: string;
  phone?: string;
  email?: string;
  hobbies?: string;
  skills?: string;
  notes?: string;
  // Refugee register extension (epic 7, §12.S.2 اضافة ابن)
  socialStatusId?: number;
}

export interface UpdateOrphanDto {
  fullName: string;
  gender: string;
  dateOfBirth: string;
  // Refugee register extension (epic 7, §12.S.2 اضافة ابن)
  socialStatusId?: number;
  /** الحالة الصحية — HealthStatus lookup id (epic-7 review P12) */
  healthStatusId?: number;
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

// ==================== Orphan Register & Coding (Epic 8, UC-ORP-*) ====================

/** Filter for the shared GET /api/Families/orphans read (UC-ORP-02/03/07). */
export interface OrphanCodingSearchRequest {
  search?: string;
  charityId?: string;
  /** "Pending" = uncoded (empty code) — the coding worklist, HQ only; "Coded" = has a code. */
  codingStatus?: string;
  sortBy?: string;
  sortDescending?: boolean;
  pageNumber?: number;
  pageSize?: number;
}

/** Row of the orphan search / coding worklist (§13.S.1 / §13.S.2 judgement data). */
export interface OrphanLookupDto {
  orphanId: string;
  fullName: string;
  code?: string | null;
  fatherName?: string | null;
  motherName?: string | null;
  charityName?: string | null;
  charityId?: string | null;
  familyId?: string | null;
  familyCode?: string | null;
  dateOfBirth?: string | null;
  age?: number | null;
  gender?: string | null;
  nationalId?: string | null;
  phone?: string | null;
  sponsorshipStatus?: string | null;
  educationLevelName?: string | null;
  healthStatusName?: string | null;
}

export interface OrphanLookupPagedResult {
  items: OrphanLookupDto[];
  totalCount: number;
  page?: number;
}

/** UC-ORP-01 — check whether an orphan may be added. */
export interface OrphanEligibilityCheckRequest {
  nationalId?: string;
  /** P3 — the family being opened; checked to exist, be active and sit in scope. */
  familyId?: string;
  charityId?: string;
}

export interface OrphanEligibilityDto {
  canBeAdded: boolean;
  field?: string | null;
  /** P16 — machine key; the label resolves to orphanCoding.reasons.<reasonCode>. */
  reasonCode?: string | null;
  existingOrphanName?: string | null;
}

/** UC-SYS-12 — family/guardian-level national-id uniqueness check. */
export interface FamilyNationalIdCheckRequest {
  nationalId: string;
  /** The family being edited — its own holders never count as a clash. */
  familyId?: string;
  /** HQ names the register; a charity claim wins server-side. */
  charityId?: string;
}

export interface FamilyNationalIdCheckResult {
  isUnique: boolean;
  holderName?: string | null;
  holderFamilyCode?: string | null;
  /** Father | Mother | Provider | Relative | Orphan (null when unique). */
  holderType?: string | null;
}

/** UC-ORP-05 — verify a sponsorship code is not already used (BR-07). */
export interface OrphanCodeCheckRequest {
  code: string;
  charityId?: string;
  excludeOrphanId?: string;
}

export interface OrphanCodeCheckDto {
  isAvailable: boolean;
  existingOrphanName?: string | null;
  existingCharityName?: string | null;
}

/** UC-ORP-06 — assign a sponsorship code. */
export interface AssignOrphanCodeRequest {
  orphanId: string;
  code: string;
}

/** UC-ORP-10 — check a phone number is not duplicated. */
export interface PhoneCheckRequest {
  number: string;
  type?: string;
  charityId?: string;
}

export interface PhoneCheckDto {
  isDuplicate: boolean;
  /** P16 — the holder as structured data; the translated label is composed client-side. */
  holderFamilyCode?: string | null;
  holderName?: string | null;
  /** family | father | mother | provider | orphan — i18n key suffix. */
  holderType?: string | null;
}

/** UC-ORP-10 — per-holder duplicate-phone state on the family form (edit mode only). */
export interface PhoneCheckState {
  status: 'idle' | 'checking' | 'available' | 'duplicate';
  /** P6 — the number this state describes; a verdict for any other number is stale. */
  checkedNumber?: string;
  /** P16 — translated label naming the existing holder, composed when the verdict arrived. */
  holderLabel?: string | null;
}

/** UC-ORP-11 — batch-number reference row (رقم الحصة). */
export interface BatchNumberDto {
  batchNo: string;
  latestGroupDate?: string | null;
}

// ==================== Family Follow-Up Report (Epic 5, UC-FAM-11) ====================

/** One UC-FAM-11 row — a family file created or updated on the report date. */
export interface FamilyFollowUpRow {
  familyId: string;
  code: string;
  headOfFamily: string;
  charityName?: string | null;
  /** "Created" | "Updated" — creation wins over a same-day update */
  changeKind: string;
  changedBy?: string | null;
  changedOn: string;
  /** How many of the family's orphans were created/updated the same day */
  orphansTouched: number;
}

export interface FamilyFollowUpPagedResult {
  items: FamilyFollowUpRow[];
  totalCount: number;
}

// ==================== Register Statistics ====================

/** Statistics band above the family list pages — scoped server-side to the caller's register. */
export interface FamilyStatistics {
  total: number;
  active: number;
  inactive: number;
  addedThisMonth: number;
}
