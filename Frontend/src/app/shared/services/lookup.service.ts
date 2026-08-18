import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, of } from 'rxjs';
import { LookupBase } from '../models/lookup.base.model';
import { ApiResponse } from '../../core/models/common.model';

@Injectable({
  providedIn: 'root'
})
export class LookupService {

  private _lookups: Array<{ key: string, lookups: ApiResponse<Array<LookupBase>> }> = []

  constructor(private http: HttpClient) {
  }

  getLookup(url: string): Observable<ApiResponse<Array<LookupBase>>> {
    let value = this._lookups.find(l => l.key == url)

    if (value)
      return of(value.lookups);
    return this.http.get<ApiResponse<Array<LookupBase>>>(url);
  }

  saveLookup(key: string, lookups: ApiResponse<Array<LookupBase>>) {
    this._lookups.push({ key, lookups })
  }
}
