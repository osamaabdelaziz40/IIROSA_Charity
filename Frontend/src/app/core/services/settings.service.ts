import { Injectable } from '@angular/core';
import { ApiService } from './api.service';
import { Observable } from 'rxjs';

export interface SystemSetting {
  id: string;
  key: string;
  value: string;
  descriptionAr?: string;
  descriptionEn?: string;
  category?: string;
  dataType?: string;
  isActive: boolean;
}

export interface NotificationSettings {
  emailFromAddress?: string;
  emailFromName?: string;
  smtpServer?: string;
  smtpPort?: number;
  smtpUserName?: string;
  smtpPassword?: string;
  isSmtpAuthenticated?: boolean;
  smtpEnableSSL?: boolean;
}

@Injectable({
  providedIn: 'root'
})
export class SettingsService {
  private readonly endpoint = '/api/settings';

  constructor(private api: ApiService) {}

  getAll(category?: string): Observable<SystemSetting[]> {
    return this.api.get(`${this.endpoint}`, { category });
  }

  get(key: string): Observable<SystemSetting> {
    return this.api.get(`${this.endpoint}/${key}`);
  }

  set(setting: SystemSetting): Observable<SystemSetting> {
    return this.api.post(`${this.endpoint}`, setting);
  }

  delete(key: string): Observable<any> {
    return this.api.delete(`${this.endpoint}/${key}`);
  }

  getNotificationSettings(): Observable<NotificationSettings> {
    return this.api.get(`${this.endpoint}/notifications`);
  }

  updateNotificationSettings(settings: NotificationSettings): Observable<any> {
    return this.api.post(`${this.endpoint}/notifications`, settings);
  }
}
