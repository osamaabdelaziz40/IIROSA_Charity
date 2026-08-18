/**
 * Housing Project Model and Related Interfaces
 * Housing Projects Module - IIROSA Frontend Application
 * Access: Admin and Super Admin only (Charity users CANNOT access)
 */

/**
 * Main Housing Project entity interface
 */
export interface HousingProject {
  /** Primary key */
  id: string;

  /** Basic Information */
  projectName: string;
  projectDescription: string;
  projectType: ProjectType;
  projectTypeName?: string;
  startDate: string;
  expectedEndDate: string;
  actualEndDate?: string;

  /** Location */
  countryId: number;
  countryName?: string;
  regionId: number;
  regionName?: string;
  centerId: number;
  centerName?: string;
  address: string;
  village: string;
  gpsCoordinates: string;

  /** Specifications */
  housingType: HousingType;
  housingTypeName?: string;
  numberOfUnits: number;
  areaPerUnit: number;
  totalArea: number;
  floorsPerUnit: number;
  roomsPerUnit: number;
  constructionMaterial: string;

  /** Financial Information */
  totalBudget: number;
  budgetCurrency: Currency;
  donorName: string;
  fundingSource: string;
  contractorName: string;
  supervisorName: string;
  finalCost?: number;

  /** Beneficiary Assignment */
  assignedCharityId: number;
  assignedCharityName?: string;
  assignedFamilyId: string;
  assignedFamilyName?: string;

  /** Status */
  projectStatus: ProjectStatus;
  projectStatusName?: string;
  completionPercentage: number;
  currentStage: ProjectStage;
  notes: string;

  /** Audit Trail */
  createdOn: string;
  modifiedOn?: string;
  createdBy?: string;
  modifiedBy?: string;
}

/**
 * Request payload for creating a new housing project
 */
export interface CreateHousingProjectRequest {
  /** Basic Information */
  projectName: string;
  projectDescription: string;
  projectType: ProjectType;
  startDate: string;
  expectedEndDate: string;
  actualEndDate?: string;

  /** Location */
  countryId: number;
  regionId: number;
  centerId: number;
  address: string;
  village: string;
  gpsCoordinates: string;

  /** Specifications */
  housingType: HousingType;
  numberOfUnits: number;
  areaPerUnit: number;
  totalArea: number;
  floorsPerUnit: number;
  roomsPerUnit: number;
  constructionMaterial: string;

  /** Financial Information */
  totalBudget: number;
  budgetCurrency: Currency;
  donorName: string;
  fundingSource: string;
  contractorName: string;
  supervisorName: string;

  /** Beneficiary Assignment */
  assignedCharityId: number;
  assignedFamilyId: string;

  /** Status */
  projectStatus: ProjectStatus;
  completionPercentage: number;
  currentStage: ProjectStage;
  notes: string;
}

/**
 * Request payload for updating an existing housing project
 */
export interface UpdateHousingProjectRequest {
  /** Basic Information */
  projectName: string;
  projectDescription: string;
  projectType: ProjectType;
  startDate: string;
  expectedEndDate: string;
  actualEndDate?: string;

  /** Location */
  countryId: number;
  regionId: number;
  centerId: number;
  address: string;
  village: string;
  gpsCoordinates: string;

  /** Specifications */
  housingType: HousingType;
  numberOfUnits: number;
  areaPerUnit: number;
  totalArea: number;
  floorsPerUnit: number;
  roomsPerUnit: number;
  constructionMaterial: string;

  /** Financial Information */
  totalBudget: number;
  budgetCurrency: Currency;
  donorName: string;
  fundingSource: string;
  contractorName: string;
  supervisorName: string;

  /** Beneficiary Assignment */
  assignedCharityId: number;
  assignedFamilyId: string;

  /** Status */
  projectStatus: ProjectStatus;
  completionPercentage: number;
  currentStage: ProjectStage;
  notes: string;
}

/**
 * Request payload for updating project budget
 */
export interface UpdateBudgetRequest {
  totalBudget: number;
  budgetCurrency: Currency;
  donorName?: string;
}

/**
 * Request payload for updating project progress
 */
export interface UpdateProgressRequest {
  projectStatus: ProjectStatus;
  completionPercentage: number;
  currentStage: ProjectStage;
  notes: string;
}

/**
 * Request payload for marking project as completed
 */
export interface MarkProjectCompletedRequest {
  actualEndDate: string;
  finalCost?: number;
  completionNotes: string;
}

/**
 * Search and filter request for housing project list
 */
export interface HousingProjectSearchRequest {
  /** Search by project name */
  search?: string;

  /** Filter by project type */
  projectType?: ProjectType;

  /** Filter by project status */
  projectStatus?: ProjectStatus;

  /** Filter by country */
  countryId?: number;

  /** Filter by region */
  regionId?: number;

  /** Filter by center */
  centerId?: number;

  /** Filter by assigned charity */
  assignedCharityId?: number;

  /** Filter by date range (start date) */
  dateFrom?: Date;
  dateTo?: Date;

  /** Pagination */
  page: number;
  pageSize: number;
}

/**
 * Request payload for attaching documents
 */
export interface AttachDocumentRequest {
  documentType: DocumentType;
  description?: string;
  documentDate?: string;
  file: File;
}

/**
 * Housing project document attachment
 */
export interface HousingProjectDocument {
  id: string;
  housingProjectId: string;
  documentType: DocumentType;
  documentTypeName?: string;
  description?: string;
  documentDate?: string;
  fileName: string;
  fileSize: number;
  fileUrl?: string;
  uploadedOn: string;
  uploadedBy?: string;
}

/**
 * Housing project progress history entry
 */
export interface HousingProjectProgress {
  id: string;
  housingProjectId: string;
  projectStatus: ProjectStatus;
  projectStatusName?: string;
  completionPercentage: number;
  currentStage?: ProjectStage;
  currentStageName?: string;
  notes?: string;
  recordedOn: string;
  recordedBy?: string;
  progressPhotos?: string[];
}

/**
 * Housing project report data
 */
export interface HousingProjectReport {
  /** Report period */
  reportPeriodStart: string;
  reportPeriodEnd: string;

  /** Project Summary */
  totalProjects: number;
  projectsByStatus: Record<ProjectStatus, number>;
  projectsByType: Record<ProjectType, number>;
  projectsByRegion: { regionName: string; count: number }[];
  projectsByCharity: { charityName: string; count: number }[];

  /** Financial Summary */
  totalBudget: number;
  budgetByStatus: Record<ProjectStatus, number>;
  averageCostPerProject: number;

  /** Progress Summary */
  averageCompletionPercentage: number;
  projectsOnTrack: number;
  delayedProjects: number;

  /** Beneficiary Summary */
  familiesHoused: number;
  individualsBenefited: number;

  /** Detailed Project List */
  projects: HousingProject[];
}

/**
 * Paginated response for housing project list
 */
export interface HousingProjectListResponse {
  /** List of housing projects */
  items: HousingProject[];

  /** Total count for pagination */
  totalCount: number;

  /** Current page number */
  pageNumber: number;

  /** Page size */
  pageSize: number;

  /** Total pages calculated */
  totalPages: number;
}

/**
 * Project Type enum
 */
export enum ProjectType {
  NewConstruction = 'NewConstruction',
  Renovation = 'Renovation',
  Repair = 'Repair',
  Expansion = 'Expansion'
}

/**
 * Housing Type enum
 */
export enum HousingType {
  Apartment = 'Apartment',
  Villa = 'Villa',
  House = 'House',
  Room = 'Room'
}

/**
 * Currency enum
 */
export enum Currency {
  EGP = 'EGP',
  SAR = 'SAR'
}

/**
 * Project Status enum
 */
export enum ProjectStatus {
  Planning = 'Planning',
  InProgress = 'InProgress',
  Completed = 'Completed',
  OnHold = 'OnHold'
}

/**
 * Project Stage enum
 */
export enum ProjectStage {
  Foundation = 'Foundation',
  Structure = 'Structure',
  Finishing = 'Finishing',
  Completed = 'Completed'
}

/**
 * Document Type enum
 */
export enum DocumentType {
  Plan = 'Plan',
  Permit = 'Permit',
  Photo = 'Photo',
  ProgressReport = 'ProgressReport',
  HandoverDocument = 'HandoverDocument',
  Other = 'Other'
}

/**
 * Helper function to get display name for ProjectType
 */
export function getProjectTypeName(type: ProjectType): string {
  const typeMap: Record<ProjectType, string> = {
    [ProjectType.NewConstruction]: 'New Construction',
    [ProjectType.Renovation]: 'Renovation',
    [ProjectType.Repair]: 'Repair',
    [ProjectType.Expansion]: 'Expansion'
  };
  return typeMap[type] || type;
}

/**
 * Helper function to get display name for HousingType
 */
export function getHousingTypeName(type: HousingType): string {
  return type; // Enum values are already display-friendly
}

/**
 * Helper function to get display name for ProjectStatus
 */
export function getProjectStatusName(status: ProjectStatus): string {
  const statusMap: Record<ProjectStatus, string> = {
    [ProjectStatus.Planning]: 'Planning',
    [ProjectStatus.InProgress]: 'In Progress',
    [ProjectStatus.Completed]: 'Completed',
    [ProjectStatus.OnHold]: 'On Hold'
  };
  return statusMap[status] || status;
}

/**
 * Helper function to get display name for ProjectStage
 */
export function getProjectStageName(stage: ProjectStage): string {
  return stage; // Enum values are already display-friendly
}

/**
 * Helper function to get display name for DocumentType
 */
export function getDocumentTypeName(type: DocumentType): string {
  const typeMap: Record<DocumentType, string> = {
    [DocumentType.Plan]: 'Plan',
    [DocumentType.Permit]: 'Permit',
    [DocumentType.Photo]: 'Photo',
    [DocumentType.ProgressReport]: 'Progress Report',
    [DocumentType.HandoverDocument]: 'Handover Document',
    [DocumentType.Other]: 'Other'
  };
  return typeMap[type] || type;
}

/**
 * Helper function to get status badge class
 */
export function getStatusBadgeClass(status: ProjectStatus): string {
  const classMap: Record<ProjectStatus, string> = {
    [ProjectStatus.Planning]: 'badge-info',
    [ProjectStatus.InProgress]: 'badge-primary',
    [ProjectStatus.Completed]: 'badge-success',
    [ProjectStatus.OnHold]: 'badge-warning'
  };
  return classMap[status] || 'badge-secondary';
}

/**
 * Helper function to check if project is completed
 */
export function isProjectCompleted(project: HousingProject): boolean {
  return project.projectStatus === ProjectStatus.Completed;
}

/**
 * Helper function to check if project is delayed
 */
export function isProjectDelayed(project: HousingProject): boolean {
  if (isProjectCompleted(project)) return false;
  if (!project.expectedEndDate) return false;

  const expectedEnd = new Date(project.expectedEndDate);
  const now = new Date();
  return now > expectedEnd && project.completionPercentage < 100;
}

/**
 * Helper function to calculate project duration in days
 */
export function calculateProjectDuration(project: HousingProject): number | null {
  if (!project.actualEndDate) return null;

  const start = new Date(project.startDate);
  const end = new Date(project.actualEndDate);
  const diffTime = Math.abs(end.getTime() - start.getTime());
  return Math.ceil(diffTime / (1000 * 60 * 60 * 24));
}
