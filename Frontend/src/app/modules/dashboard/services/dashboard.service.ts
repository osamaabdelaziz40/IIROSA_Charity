import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { DashboardStatistics } from '../models/dashboard-statistics.model';

@Injectable({ providedIn: 'root' })
export class DashboardService {
  constructor(private http: HttpClient) {}

  getStatistics(charityId: string | null = null): Observable<DashboardStatistics> {
    const params = charityId ? new HttpParams().set('charityId', charityId) : undefined;
    return this.http.get<DashboardStatistics>(`${environment.apiUrl}/api/Dashboard/statistics`, { params });
  }
}
