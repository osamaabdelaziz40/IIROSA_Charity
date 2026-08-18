import { AttachmentDto } from '../../../shared/shared.module';

/**
 * Office Project Type
 */
export interface OfficeProjectType {
  id: number;
  name: string;
  nameAr: string;
  nameEn: string;
  isActive: boolean;
  sortOrder?: number;
}

/**
 * Beneficiary Type Enum
 */
export enum BeneficiaryType {
  Families = 'Families',
  Individuals = 'Individuals',
  Both = 'Both'
}

/**
 * Document Type Enum
 */
export enum DocumentType {
  Proposal = 'Proposal',
  Contract = 'Contract',
  ProgressReport = 'Progress Report',
  Other = 'Other'
}

/**
 * Report Type Enum
 */
export enum ReportType {
  Completion = 'Completion',
  Progress = 'Progress',
  Final = 'Final'
}

/**
 * Project Document Attachment
 */
export interface ProjectDocument {
  id?: string;
  officeProjectId?: string;
  documentType: DocumentType;
  fileName: string;
  fileUrl?: string;
  fileSize?: number;
  description?: string;
  documentDate: Date;
  uploadedAt?: Date;
  uploadedBy?: string;
}

/**
 * Project Report
 */
export interface ProjectReport {
  id?: string;
  officeProjectId?: string;
  reportType: ReportType;
  fileName: string;
  fileUrl?: string;
  fileSize?: number;
  reportDate: Date;
  summary?: string;
  uploadedAt?: Date;
  uploadedBy?: string;
}

/**
 * Office Development Project (matches backend OfficeProjectDetailDto)
 */
export interface OfficeProject {
  id: string;

  // Basic Information
  projectName: string;
  projectHint?: string;
  projectDate: Date;
  projectEndDate?: Date;

  // Classification
  officeProjectTypeId?: number;
  officeProjectTypeName?: string;

  // Location
  countryId?: number;
  countryName?: string;
  regionId?: number;
  regionName?: string;
  centerId?: number;
  centerName?: string;
  villageName?: string;

  // Financial Information
  projectCostEGP?: number;
  projectCostSAR?: number;
  donorName?: string;

  // Beneficiaries
  beneficiariesCount?: number;
  beneficiariesType?: string;

  // Charity Assignment
  charityId?: string;
  charityName?: string;

  // Documents
  attachedFileId?: string;
  attachedFileName?: string;
  projectReportFileId?: string;
  projectReportFileName?: string;

  // Status
  isFinished: boolean;

  // Additional Notes
  notes?: string;

  // Attachments (for form display/editing)
  document_Attach?: AttachmentDto[];
  report_Attach?: AttachmentDto[];

  // Additional fields for form
  actualEndDate?: Date;

  // Audit
  createdOn: Date;
  createdBy?: string;
  updatedOn?: Date;
  updatedBy?: string;

  // Backward compatibility aliases
  createdAt?: Date;
  modifiedAt?: Date;
  modifiedBy?: string;
  officeProjectType?: { name?: string; nameAr?: string; nameEn?: string };
  assignedCharityName?: string;
}

/**
 * Office Project Filter (matches backend OfficeProjectFilterDto)
 */
export interface OfficeProjectFilter {
  searchText?: string;
  fk_OfficeProjectTypeId?: number;
  fk_CountryId?: number;
  fk_RegionId?: number;
  fk_CenterId?: number;
  fk_CharityId?: string;
  isFinished?: boolean;
  donorName?: string;
  startDate?: Date;
  endDate?: Date;
  page?: number;
  pageSize?: number;
}

/**
 * Office Project List Item (matches backend OfficeProjectListDto)
 */
export interface OfficeProjectListItem {
  id: string;
  projectName: string;
  projectType: string;
  projectDate: Date;
  region: string;
  center: string;
  village?: string;
  projectCostEGP?: number;
  projectCostSAR?: number;
  beneficiariesCount?: number;
  donorName?: string;
  isFinished: boolean;
  projectEndDate?: Date;
  assignedCharity?: string;
  countryName?: string;

  // Backward compatibility aliases
  officeProjectType?: string;
  regionName?: string;
  centerName?: string;
  villageName?: string;
  assignedCharityName?: string;
}

/**
 * Create/Update Office Project DTO (matches backend CreateOfficeProjectDto)
 */
export interface OfficeProjectDto {
  id?: string;

  // Required fields
  projectName: string;
  projectDate: Date;
  fk_OfficeProjectTypeId: number;

  // Optional fields
  projectHint?: string;
  projectEndDate?: Date;
  fk_CountryId?: number;
  fk_RegionId?: number;
  fk_CenterId?: number;
  villageName?: string;
  projectCostEGP?: number;
  projectCostSAR?: number;
  donorName?: string;
  beneficiariesCount?: number;
  beneficiariesType?: string;
  fk_CharityId?: string;

  // Attachments - sent to backend for processing (matching Charity pattern)
  document_Attach?: AttachmentDto[];
  report_Attach?: AttachmentDto[];

  notes?: string;
  isFinished?: boolean;
}

/**
 * Office Project Status Summary (matches backend OfficeProjectStatusSummaryDto)
 */
export interface OfficeProjectStatusSummary {
  ongoingCount: number;
  completedCount: number;
  totalCount: number;
  totalCostEGP?: number;
  totalCostSAR?: number;
  totalBeneficiaries?: number;
}

/**
 * Paged result wrapper (matches backend OfficeProjectPagedResult<T>)
 */
export interface OfficeProjectPagedResult<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages?: number;
}

/**
 * Project Progress Statistics
 */
export interface ProjectProgressStats {
  totalProjects: number;
  ongoingProjects: number;
  completedProjects: number;
  totalBudgetEGP: number;
  totalBudgetSAR: number;
  totalBeneficiaries: number;
  projectsByType: { [key: string]: number };
  projectsByRegion: { [key: string]: number };
  projectsByCharity: { [key: string]: number };
}

/**
 * Project Progress Item
 */
export interface ProjectProgressItem {
  id: string;
  projectName: string;
  projectDate: Date;
  expectedEndDate?: Date;
  actualEndDate?: Date;
  progressStatus: 'Not Started' | 'In Progress' | 'Completed';
  budgetEGP?: number;
  actualCostEGP?: number;
  budgetSAR?: number;
  actualCostSAR?: number;
  plannedBeneficiaries?: number;
  servedBeneficiaries?: number;
  charityName?: string;
  regionName: string;
  centerName: string;
}

/**
 * Validation Errors
 */
export interface OfficeProjectValidationError {
  projectName?: string;
  projectDate?: string;
  fk_OfficeProjectTypeId?: string;
  fk_CountryId?: string;
  fk_RegionId?: string;
  fk_CenterId?: string;
  beneficiariesCount?: string;
  projectCostEGP?: string;
  projectCostSAR?: string;
}

/**
 * Document Upload Request
 */
export interface DocumentUploadRequest {
  officeProjectId: string;
  documentType: DocumentType;
  description?: string;
  documentDate: Date;
  file: File;
}

/**
 * Report Upload Request
 */
export interface ReportUploadRequest {
  officeProjectId: string;
  reportType: ReportType;
  reportDate: Date;
  summary?: string;
  file: File;
}
