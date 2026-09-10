/**
 * Notification models (UC-NTF web notifications epic)
 * The API returns raw DTOs — no {success, data} envelope.
 */

/** NotificationsLogListDto as the API serializes it (camelCase). */
export interface NotificationsLog {
  id: string;
  title: string;
  message: string;
  isUser: boolean;
  isCharity: boolean;
  /** Optional on the wire — defensive ?? at the render sites treats a missing list as empty. */
  recipientUserIds?: string[];
  recipientCharityIds?: string[];
  notificationTypeId: number;
  sentCount: number;
  lastSentOn?: string | null;
  createdOn: string;
  createdBy?: string | null;
  /** Server-resolved hub audience — populated on create/resend responses only. */
  deliveredToUserIds?: string[];
}

export interface CreateNotificationsLogRequest {
  title: string;
  message: string;
  isUser: boolean;
  isCharity: boolean;
  recipientUserIds: string[];
  recipientCharityIds: string[];
}

export type UpdateNotificationsLogRequest = CreateNotificationsLogRequest;

export interface NotificationsLogSearchRequest {
  search?: string;
  page?: number;
  pageSize?: number;
}

export interface NotificationsLogPagedResult {
  items: NotificationsLog[];
  totalCount: number;
  page: number;
  pageSize: number;
}
