/**
 * Impersonation Models
 * Models for User Impersonation functionality (UC-19.1 through UC-19.8)
 */

/**
 * User being impersonated information
 */
export interface ImpersonatedUserInfo {
  id: string;
  username: string;
  email: string;
  fullName?: string;
  roles: string[];
}

/**
 * Response for starting impersonation
 */
export interface StartImpersonationResponse {
  impersonationToken: string;
  sessionId: string;
  impersonatedUser: ImpersonatedUserInfo;
  expiresAt: string;
}

/**
 * Response for ending impersonation
 */
export interface EndImpersonationResponse {
  originalToken: string;
  sessionDurationSeconds: number;
  actionsPerformed: number;
}

/**
 * Active impersonation session
 */
export interface ActiveImpersonationSession {
  sessionId: string;
  impersonatorUserId: string;
  impersonatorUserName: string;
  impersonatorEmail?: string;
  impersonatorRole?: string;
  impersonatedUserId: string;
  impersonatedUserName: string;
  impersonatedEmail?: string;
  impersonatedRole?: string;
  startTime: string;
  originalUserIpAddress?: string;
  actionsPerformedCount: number;
  durationMinutes: number;
}

/**
 * Impersonation session history
 */
export interface ImpersonationSessionHistory {
  sessionId: string;
  impersonatorUserId: string;
  impersonatorUserName: string;
  impersonatedUserId: string;
  impersonatedUserName: string;
  startTime: string;
  endTime?: string;
  durationSeconds?: number;
  isActive: boolean;
  actionsPerformedCount: number;
  terminationReason?: string;
  terminatedByName?: string;
}

/**
 * Filter for impersonation history
 */
export interface ImpersonationHistoryFilter {
  impersonatorId?: string;
  impersonatedId?: string;
  startDate?: string;
  endDate?: string;
  isActive?: boolean;
}

/**
 * User search result for impersonation
 */
export interface UserSearchResult {
  id: string;
  username: string;
  email: string;
  fullName?: string;
  role?: string;
  isActive: boolean;
  canImpersonate: boolean;
}

/**
 * Request for terminating a session
 */
export interface TerminateSessionRequest {
  reason?: string;
}

/**
 * Impersonation settings
 */
export interface ImpersonationSettings {
  maxSessionDurationHours: number;
  warningThresholdMinutes: number;
  allowSessionExtension: boolean;
  maxExtensionsPerSession: number;
  requireConfirmation: boolean;
  logAllActions: boolean;
  notifyImpersonatedUser: boolean;
  ipRestrictionEnabled: boolean;
  logRetentionDays: number;
}

/**
 * Current impersonation status
 */
export interface ImpersonationStatus {
  isImpersonating: boolean;
  sessionId?: string;
  originalUserId?: string;
  originalUserName?: string;
  sessionStartTime: string;
  sessionExpiresAt: string;
  currentUser?: ImpersonatedUserInfo;
}

/**
 * Request to start impersonation
 */
export interface StartImpersonationRequest {
  targetUsername: string;
}

/**
 * Request to end impersonation
 */
export interface EndImpersonationRequest {
  sessionId?: string;
}

/**
 * Request to increment actions counter
 */
export interface IncrementActionsRequest {
  sessionId: string;
}
