import { Injectable } from '@angular/core';
import { ApiService } from './api.service';
import { Observable } from 'rxjs';

export interface AttachmentResponse {
  id: string;
  fileName: string;
  contentType: string;
  fileSizeBytes: number;
  moduleName?: string;
  recordId?: string;
  createdOn: Date;
}

@Injectable({
  providedIn: 'root'
})
export class AttachmentService {
  private readonly endpoint = '/api/attachments';

  constructor(private api: ApiService) {}

  upload(file: File, moduleName?: string, recordId?: string): Observable<AttachmentResponse> {
    const formData = new FormData();
    formData.append('file', file);
    if (moduleName) formData.append('moduleName', moduleName);
    if (recordId) formData.append('recordId', recordId);

    return this.api.post(`${this.endpoint}/upload`, formData);
  }

  download(id: string): Observable<Blob> {
    return this.api.download(`${this.endpoint}/${id}/download`);
  }

  getById(id: string): Observable<AttachmentResponse> {
    return this.api.get(`${this.endpoint}/${id}/info`);
  }

  delete(id: string): Observable<any> {
    return this.api.delete(`${this.endpoint}/${id}`);
  }

  getByModule(moduleName: string, recordId: string): Observable<AttachmentResponse[]> {
    return this.api.get(`${this.endpoint}/by-module/${moduleName}/${recordId}`);
  }

  getTypes(): Observable<any[]> {
    return this.api.get(`${this.endpoint}/types`);
  }
}
