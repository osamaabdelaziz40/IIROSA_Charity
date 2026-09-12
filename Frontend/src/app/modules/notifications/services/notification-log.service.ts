/**
 * Notification Log Service (UC-NTF web notifications epic)
 * Talks to NotificationController — the API returns raw DTOs, no envelope.
 */

import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';

import {
  NotificationsLog,
  CreateNotificationsLogRequest,
  UpdateNotificationsLogRequest,
  NotificationsLogSearchRequest,
  NotificationsLogPagedResult,
  NotificationsLogStatistics
} from '../models/notification.model';

@Injectable({
  providedIn: 'root'
})
export class NotificationLogService {
  private readonly apiBaseUrl = '/api/notification';

  constructor(private http: HttpClient) {}

  /**
   * The admin register (UC-NTF list) — every pushed notification, newest first.
   */
  getNotifications(search: NotificationsLogSearchRequest): Observable<NotificationsLogPagedResult> {
    return this.http.get<NotificationsLogPagedResult>(this.apiBaseUrl, { params: this.buildParams(search) });
  }

  /**
   * The recipient's read (UC-NTF my notifications) — rows whose audience contains
   * the caller directly or through their charity.
   */
  getMyNotifications(search: NotificationsLogSearchRequest): Observable<NotificationsLogPagedResult> {
    return this.http.get<NotificationsLogPagedResult>(`${this.apiBaseUrl}/my`, { params: this.buildParams(search) });
  }

  /**
   * Register statistics for the band above the admin notifications grid (UC-NTF list) —
   * Admin/SuperAdmin only; describes the whole register the admin list shows.
   */
  getStatistics(): Observable<NotificationsLogStatistics> {
    return this.http.get<NotificationsLogStatistics>(`${this.apiBaseUrl}/statistics`);
  }

  /**
   * Detail read for the edit screen.
   */
  getNotificationById(id: string): Observable<NotificationsLog> {
    return this.http.get<NotificationsLog>(`${this.apiBaseUrl}/${id}`);
  }

  /**
   * Create and push (UC-NTF push) — the server stores the row and delivers it
   * live over SignalR to the resolved audience.
   */
  createNotification(request: CreateNotificationsLogRequest): Observable<NotificationsLog> {
    return this.http.post<NotificationsLog>(this.apiBaseUrl, request);
  }

  /**
   * Update the stored content/audience (UC-NTF edit) — no push until resent.
   */
  updateNotification(id: string, request: UpdateNotificationsLogRequest): Observable<NotificationsLog> {
    return this.http.put<NotificationsLog>(`${this.apiBaseUrl}/${id}`, request);
  }

  /**
   * Resend (UC-NTF resend) — re-push the stored content to a freshly resolved
   * audience; the server bumps SentCount/LastSentOn.
   */
  resendNotification(id: string): Observable<NotificationsLog> {
    return this.http.post<NotificationsLog>(`${this.apiBaseUrl}/${id}/resend`, {});
  }

  private buildParams(search?: NotificationsLogSearchRequest): HttpParams {
    let params = new HttpParams();
    if (search) {
      if (search.search?.trim()) {
        params = params.set('search', search.search.trim());
      }
      params = params.set('page', String(search.page ?? 1));
      params = params.set('pageSize', String(search.pageSize ?? 20));
    }
    return params;
  }
}
