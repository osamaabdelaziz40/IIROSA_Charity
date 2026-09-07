/**
 * Mission Model and Related Interfaces
 * Missions Module - IIROSA Frontend Application (epic 15)
 * Field names mirror the API wire contract (camelCase).
 */

/**
 * Mission list row — the wire shape of MissionListDto
 */
export interface Mission {
  /** Primary key */
  id: string;

  /** Basic Information */
  missionTarget: string;
  details?: string;

  /** Classification */
  missionType?: string;
  missionTimeType?: string;

  /** §20.S.1 columns */
  entityName?: string;
  village?: string;
  missionLocation?: string;
  missionCompletedTxt?: string;

  /** Scheduling */
  missionDate: string;
  isMissionCompleted: boolean;

  /** اسم القائم — the assigned employee's full name (MissionListDto.AssignedTo) */
  assignedTo?: string;

  /** Location */
  region?: string;
  center?: string;
  countryName?: string;
}

/**
 * Mission detail — the wire shape of MissionDetailDto
 */
export interface MissionDetail {
  id: string;

  /** Basic Information */
  missionTarget: string;
  missionDetails?: string;
  details?: string;

  /** Classification (ids for the form selects, names for display) */
  missionTypeId?: number;
  missionTypeName?: string;
  missionTimeTypeId?: number;
  missionTimeTypeName?: string;
  missionInterviewTypeId?: number;
  missionInterviewTypeName?: string;

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
  assignedToUserId?: string;
  assignedUserName?: string;
  assignedUserEmail?: string;

  /** Ownership */
  charityId?: string;
  charityName?: string;

  /** Event Information */
  entityName?: string;
  conferenceName?: string;

  /** Audit */
  createdOn: string;
  updatedOn?: string;
}

/**
 * Request payload for creating a new mission — the wire shape of CreateMissionDto
 */
export interface CreateMissionRequest {
  /** Basic Information */
  missionTarget: string;
  missionDetails?: string;
  details?: string;

  /** Classification */
  missionTypeId: number;
  missionTimeTypeId: number;
  missionInterviewTypeId: number;

  /** Scheduling */
  missionDate: string;

  /** Location */
  countryId?: number;
  regionId?: number;
  centerId?: number;
  missionLocation?: string;
  village?: string;

  /** Assignment */
  assignedToUserId: string;

  /** Event Information */
  entityName?: string;
  conferenceName?: string;
}

/**
 * Request payload for updating an existing mission — the wire shape of UpdateMissionDto
 */
export interface UpdateMissionRequest {
  missionTarget?: string;
  missionDetails?: string;
  details?: string;
  missionTypeId?: number;
  missionTimeTypeId?: number;
  missionInterviewTypeId?: number;
  missionDate?: string;
  countryId?: number;
  regionId?: number;
  centerId?: number;
  missionLocation?: string;
  village?: string;
  entityName?: string;
  conferenceName?: string;
  assignedToUserId?: string;
}

/**
 * Register mission result payload — the wire shape of RegisterMissionResultDto (UC-MSN-09).
 * The screen's two completion checkboxes are one tri-state: the actor checks either
 * "completed" or "not completed".
 */
export interface RegisterMissionResultRequest {
  entityName?: string;
  conferenceName?: string;
  details?: string;
  missionTarget?: string;
  missionDetails?: string;
  missionLocation?: string;
  assignedToUserId?: string;
  village?: string;
  isCompleted?: boolean;
  reason?: string;
}

/**
 * Search and filter request for the mission register — the wire shape of MissionFilterDto
 */
export interface MissionSearchRequest {
  /** Search by mission target */
  search?: string;

  /** Filter by mission type */
  missionTypeId?: number;

  /** Filter by mission time type */
  missionTimeTypeId?: number;

  /** Filter by owning charity */
  charityId?: string;

  /** Filter by completion status (undefined = all, false = pending, true = completed) */
  isCompleted?: boolean;

  /** Filter by country */
  countryId?: number;

  /** Filter by region */
  regionId?: number;

  /** Filter by center */
  centerId?: number;

  /** Filter by assigned user */
  assignedToUserId?: string;

  /** Filter by date range */
  dateFrom?: string;
  dateTo?: string;

  /** Pagination */
  page: number;
  pageSize: number;
}

/**
 * Paginated response for mission lists — the wire shape of MissionPagedResult<T>
 */
export interface MissionListResponse {
  items: Mission[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

/**
 * Lookup item for the mission catalogues (type / time type / interview type)
 */
export interface MissionLookupItem {
  id: number;
  name?: string;
  nameAr?: string;
  nameEn?: string;
  typeCode?: string;
  timeTypeCode?: string;
  isActive: boolean;
}
