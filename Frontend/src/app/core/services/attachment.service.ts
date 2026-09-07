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

  /**
   * UC-SYS-02: authenticated blob download that saves the file locally. A plain
   * <a href> cannot carry the Bearer header — this helper is the platform's
   * mechanism wherever a file link is rendered.
   */
  downloadAndSave(id: string, fallbackName = 'attachment', onError?: () => void): void {
    this.download(id).subscribe({
      next: (blob) => {
        const url = URL.createObjectURL(blob);
        const link = document.createElement('a');
        link.href = url;
        link.download = fallbackName;
        document.body.appendChild(link);
        link.click();
        document.body.removeChild(link);
        URL.revokeObjectURL(url);
      },
      error: () => {
        // The caller owns the error UX (this shared service stays toast-free per the
        // recorded ruling) — but a failure must never be fully silent (review P6, 2026-08-26).
        if (onError) {
          onError();
        } else {
          console.error(`Failed to download attachment ${id}`);
        }
      }
    });
  }

  /**
   * The image stream for <img> rendering — fetched as a BLOB through the authenticated
   * ApiService and bound as an object URL (an <img src> cannot carry the Bearer header;
   * the endpoint stays [Authorize] — 9-16 defect-2 ruling).
   */
  getImage(id: string): Observable<Blob> {
    return this.api.download(`${this.endpoint}/${id}/image`);
  }

  getById(id: string): Observable<AttachmentResponse> {
    return this.api.get(`${this.endpoint}/${id}/info`);
  }

  /**
   * UC-SYS-03: batch metadata read for manifests/detail screens — one call instead
   * of N per-id round trips. Server caps the batch at 50 ids; unknown ids are omitted.
   */
  getInfoBatch(ids: string[]): Observable<AttachmentResponse[]> {
    if (!ids || ids.length === 0) {
      return new Observable<AttachmentResponse[]>(observer => {
        observer.next([]);
        observer.complete();
      });
    }
    const query = ids.map(id => `ids=${encodeURIComponent(id)}`).join('&');
    return this.api.get(`${this.endpoint}?${query}`);
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
