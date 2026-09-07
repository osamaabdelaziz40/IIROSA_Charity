import { AttachmentDto } from '../../../shared/shared.module';

/**
 * Office Project Type lookup (UC-OFP-02)
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
 * Office Development Project (matches backend OfficeProjectDetailDto — UC-OFP-04).
 * Property names mirror the camelCase wire contract exactly; no aliases.
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

  // Audit
  createdOn: Date;
  createdBy?: string;
  updatedOn?: Date;
  updatedBy?: string;
}

/**
 * Office Project Filter (matches backend OfficeProjectFilterDto — UC-OFP-01, UC-OFP-06)
 */
export interface OfficeProjectFilter {
  searchText?: string;
  officeProjectTypeId?: number;
  countryId?: number;
  regionId?: number;
  centerId?: number;
  charityId?: string;
  isFinished?: boolean;
  donorName?: string;
  startDate?: Date;
  endDate?: Date;
  page?: number;
  pageSize?: number;
}

/**
 * Office Project List Item (matches backend OfficeProjectListDto — UC-OFP-01)
 */
export interface OfficeProjectListItem {
  id: string;
  projectName: string;
  projectType?: string;
  projectDate: Date;
  region?: string;
  center?: string;
  village?: string;
  projectCostEGP?: number;
  projectCostSAR?: number;
  beneficiariesCount?: number;
  donorName?: string;
  isFinished: boolean;
  projectEndDate?: Date;
  assignedCharity?: string;
  countryName?: string;
}

/**
 * Create/Update Office Project DTO (matches backend Create/UpdateOfficeProjectDto — UC-OFP-03/04)
 */
export interface OfficeProjectDto {
  id?: string;

  // Required fields
  projectName: string;
  projectDate: Date;
  officeProjectTypeId: number;

  // Optional fields
  projectHint?: string;
  projectEndDate?: Date;
  countryId?: number;
  regionId?: number;
  centerId?: number;
  villageName?: string;
  projectCostEGP?: number;
  projectCostSAR?: number;
  donorName?: string;
  beneficiariesCount?: number;
  beneficiariesType?: string;
  charityId?: string;

  // Attachments - sent to backend for processing (matching Charity pattern)
  document_Attach?: AttachmentDto[];
  report_Attach?: AttachmentDto[];

  notes?: string;
  isFinished?: boolean;
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
