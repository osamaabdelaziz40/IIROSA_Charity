/**
 * Mission Model and Related Interfaces
 * Missions Module - IIROSA Frontend Application
 */

/**
 * Main Mission entity interface
 */
export interface Mission {
  /** Primary key */
  id: string;

  /** Basic Information */
  missionTarget: string;
  missionDetails?: string;
  details?: string;

  /** Classification */
  missionTypeId: number;
  missionTypeName?: string;
  missionTimeTypeId: number;
  missionTimeTypeName?: string;

  /** Scheduling */
  missionDate: string;
  missionCompletedDate?: string;
  isMissionCompleted: boolean;
  missionCompletedTxt?: string;

  /** Location */
  countryId?: number;
  countryName?: string;
  regionId?: number;
  regionName?: string;
  centerId?: number;
  centerName?: string;
  missionLocation?: string;
  village?: string;

  /** Assignment */
  assignedTo: string; // UserId
  assignedToName?: string;

  /** Event Information */
  entityName?: string;
  conferenceName?: string;

  /** Audit Trail */
  createdOn: string;
  modifiedOn?: string;
}

/**
 * Request payload for creating a new mission
 */
export interface CreateMissionRequest {
  /** Basic Information */
  missionTarget: string;
  missionDetails?: string;
  details?: string;

  /** Classification */
  missionTypeId: number;
  missionTimeTypeId: number;

  /** Scheduling */
  missionDate: string;

  /** Location */
  countryId?: number;
  regionId?: number;
  centerId?: number;
  missionLocation?: string;
  village?: string;

  /** Assignment */
  assignedTo: string;

  /** Event Information */
  entityName?: string;
  conferenceName?: string;
}

/**
 * Request payload for updating an existing mission
 */
export interface UpdateMissionRequest {
  /** Basic Information */
  missionTarget: string;
  missionDetails?: string;
  details?: string;

  /** Classification */
  missionTypeId: number;
  missionTimeTypeId: number;

  /** Scheduling */
  missionDate: string;

  /** Location */
  countryId?: number;
  regionId?: number;
  centerId?: number;
  missionLocation?: string;
  village?: string;

  /** Assignment */
  assignedTo: string;

  /** Event Information */
  entityName?: string;
  conferenceName?: string;
}

/**
 * Search and filter request for mission list
 */
export interface MissionSearchRequest {
  /** Search by mission target */
  search?: string;

  /** Filter by mission type */
  missionTypeId?: number;

  /** Filter by mission time type */
  missionTimeTypeId?: number;

  /** Filter by completion status (undefined = all, false = pending, true = completed) */
  isCompleted?: boolean;

  /** Filter by country */
  countryId?: number;

  /** Filter by region */
  regionId?: number;

  /** Filter by center */
  centerId?: number;

  /** Filter by assigned user (empty string = current user) */
  assignedTo?: string;

  /** Filter by date range */
  dateFrom?: Date;
  dateTo?: Date;

  /** Pagination */
  page: number;
  pageSize: number;
}

/**
 * Request payload for marking a mission as completed
 */
export interface MarkMissionCompletedRequest {
  /** Optional completion notes */
  completionNotes?: string;
}

/**
 * Mission status counts for dashboard
 */
export interface MissionStatusCounts {
  /** Pending missions count */
  pending: number;

  /** In progress missions count */
  inProgress: number;

  /** Completed missions count */
  completed: number;

  /** Overdue missions count */
  overdue: number;
}

/**
 * Paginated response for mission list
 */
export interface MissionListResponse {
  /** List of missions */
  items: Mission[];

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
 * Mission type lookup (if not using generic lookup)
 */
export interface MissionType {
  id: number;
  name: string;
  nameAr?: string;
  nameEn?: string;
  description?: string;
  isActive: boolean;
}

/**
 * Mission time type lookup (if not using generic lookup)
 */
export interface MissionTimeType {
  id: number;
  name: string;
  nameAr?: string;
  nameEn?: string;
  description?: string;
  isActive: boolean;
}

/**
 * Mission status enum for type safety
 */
export enum MissionStatus {
  Pending = 'Pending',
  InProgress = 'InProgress',
  Completed = 'Completed',
  Overdue = 'Overdue'
}

/**
 * Mission time type enum for static values
 */
export enum MissionTimeTypeEnum {
  OneTime = 'One-time',
  Daily = 'Daily',
  Weekly = 'Weekly',
  Monthly = 'Monthly',
  Quarterly = 'Quarterly',
  Annually = 'Annually'
}

/**
 * Mission type enum for static values
 */
export enum MissionTypeEnum {
  Fieldwork = 'Fieldwork',
  Conference = 'Conference',
  Training = 'Training',
  Meeting = 'Meeting',
  Inspection = 'Inspection',
  Other = 'Other'
}
