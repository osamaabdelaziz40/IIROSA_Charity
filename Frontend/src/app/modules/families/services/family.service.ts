import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError, map } from 'rxjs/operators';
import { environment } from '../../../../environments/environment';
import {
  FamilyDto,
  CreateFamilyDto,
  UpdateFamilyDto,
  FamilySearchRequest,
  FamilyPagedResult,
  FatherDto,
  CreateFatherDto,
  UpdateFatherDto,
  MotherDto,
  CreateMotherDto,
  UpdateMotherDto,
  ProviderDto,
  CreateProviderDto,
  UpdateProviderDto,
  RelativeDto,
  CreateRelativeDto,
  UpdateRelativeDto,
  RelativeListDto,
  OrphanDto,
  CreateOrphanDto,
  UpdateOrphanDto,
  OrphanSearchRequest,
  OrphanPagedResult,
  OrphanCodingSearchRequest,
  OrphanLookupDto,
  OrphanLookupPagedResult,
  OrphanEligibilityCheckRequest,
  OrphanEligibilityDto,
  FamilyNationalIdCheckRequest,
  FamilyNationalIdCheckResult,
  OrphanCodeCheckRequest,
  OrphanCodeCheckDto,
  AssignOrphanCodeRequest,
  PhoneCheckRequest,
  PhoneCheckDto,
  GuardianChangeRequestPagedResult,
  CreateGuardianChangeRequest,
  DecideGuardianChangeRequest,
  FamilyFollowUpPagedResult,
  FamilyAttachmentDto,
  CreateFamilyAttachmentDto,
  FamilyAuditLog,
  AttachmentDto
} from '../models/family.model';

@Injectable({
  providedIn: 'root'
})
export class FamilyService {
  private apiUrl = `${environment.apiUrl}/api/Families`;

  constructor(private http: HttpClient) {}

  private getHeaders(): HttpHeaders {
    return new HttpHeaders({
      'Content-Type': 'application/json'
    });
  }

  private convertAttachmentsForBackend(attachments: AttachmentDto[] | undefined): any[] | undefined {
    if (!attachments || attachments.length === 0) {
      return undefined;
    }

    return attachments.map(attachment => {
      const isNewAttachment = attachment.isNew || (attachment.id && attachment.id.startsWith('temp_'));

      let fileDataBytes: number[] | undefined;
      if (attachment.fileData) {
        fileDataBytes = this.base64ToByteArray(attachment.fileData);
      }

      return {
        id: isNewAttachment ? '00000000-0000-0000-0000-000000000000' : (attachment.id || '00000000-0000-0000-0000-000000000000'),
        fileName: attachment.fileName,
        contentType: attachment.contentType,
        fileData: fileDataBytes,
        filePath: attachment.filePath,
        description: attachment.description,
        extension: attachment.extension,
        isDeleted: attachment.isDeleted || false,
        isNew: attachment.isNew || false
      };
    });
  }

  private base64ToByteArray(base64: string): number[] {
    const base64Data = base64.includes(',') ? base64.split(',')[1] : base64;
    const binaryString = atob(base64Data);
    const bytes = new Uint8Array(binaryString.length);
    for (let i = 0; i < binaryString.length; i++) {
      bytes[i] = binaryString.charCodeAt(i);
    }
    return Array.from(bytes);
  }

  private buildHttpParams(filter: any): HttpParams {
    let params = new HttpParams();
    if (filter) {
      Object.keys(filter).forEach(key => {
        const value = filter[key];
        if (value !== undefined && value !== null && value !== '') {
          params = params.set(key, value.toString());
        }
      });
    }
    return params;
  }

  // ==================== FAMILY CRUD ====================

  getFamilies(searchRequest: FamilySearchRequest): Observable<FamilyPagedResult> {
    return this.http.get<FamilyPagedResult>(`${this.apiUrl}`, {
      headers: this.getHeaders(),
      params: this.buildHttpParams(searchRequest)
    }).pipe(
      catchError(this.handleError)
    );
  }

  getFamily(id: string): Observable<FamilyDto> {
    return this.http.get<FamilyDto>(`${this.apiUrl}/${id}`, {
      headers: this.getHeaders()
    }).pipe(
      catchError(this.handleError)
    );
  }

  createFamily(family: CreateFamilyDto): Observable<FamilyDto> {
    return this.http.post<FamilyDto>(`${this.apiUrl}`, family, {
      headers: this.getHeaders()
    }).pipe(
      catchError(this.handleError)
    );
  }

  updateFamily(id: string, family: UpdateFamilyDto): Observable<FamilyDto> {
    return this.http.put<FamilyDto>(`${this.apiUrl}/${id}`, family, {
      headers: this.getHeaders()
    }).pipe(
      catchError(this.handleError)
    );
  }

  deleteFamily(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`, {
      headers: this.getHeaders()
    }).pipe(
      catchError(this.handleError)
    );
  }

  deactivateFamily(id: string): Observable<void> {
    return this.http.patch<void>(`${this.apiUrl}/${id}/deactivate`, {}, {
      headers: this.getHeaders()
    }).pipe(
      catchError(this.handleError)
    );
  }

  activateFamily(id: string): Observable<void> {
    return this.http.patch<void>(`${this.apiUrl}/${id}/activate`, {}, {
      headers: this.getHeaders()
    }).pipe(
      catchError(this.handleError)
    );
  }

  // ==================== CHARITY TRANSFER (UC-FAM-06) ====================

  transferFamily(id: string, dto: { newCharityId: string; reason?: string }): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/${id}/charity`, dto, {
      headers: this.getHeaders()
    }).pipe(
      catchError(this.handleError)
    );
  }

  // ==================== MEMBER CONTROL (UC-FAM-07/08) ====================

  /** Move an orphan (memberType 1) or guardian (memberType 2) between families. */
  controlMember(familyId: string, memberId: string, dto: {
    memberType: number;
    action: number;
    targetFamilyCode?: string;
    justification?: string;
  }): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/${familyId}/members/${memberId}/control`, dto, {
      headers: this.getHeaders()
    }).pipe(
      catchError(this.handleError)
    );
  }

  /**
   * UC-FAM-13 حذف كفالة العائل — remove the family's guardian sponsorship link.
   * HQ-only server-side; the التعليق travels as a query parameter (DELETE body).
   */
  removeProviderSponsorLink(familyId: string, comment?: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${familyId}/provider/sponsor`, {
      headers: this.getHeaders(),
      params: this.buildHttpParams({ comment })
    }).pipe(
      catchError(this.handleError)
    );
  }

  // ==================== GUARDIAN CHANGE REQUESTS (UC-FAM-09/10) ====================

  /** The review queue — pending by default; a Charity-role caller is scoped server-side. */
  getProviderRequests(filter: {
    status?: number;
    charityId?: string;
    pageNumber?: number;
    pageSize?: number;
  }): Observable<GuardianChangeRequestPagedResult> {
    return this.http.get<GuardianChangeRequestPagedResult>(`${this.apiUrl}/provider-requests`, {
      headers: this.getHeaders(),
      params: this.buildHttpParams(filter)
    }).pipe(
      catchError(this.handleError)
    );
  }

  /** Raise a guardian-change request for a family (the charity-side producer of the queue). */
  raiseGuardianChangeRequest(familyId: string, dto: CreateGuardianChangeRequest): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/${familyId}/provider-requests`, dto, {
      headers: this.getHeaders()
    }).pipe(
      catchError(this.handleError)
    );
  }

  /** Record the head-office decision on a request (UC-FAM-10 اعتماد تعديل العائل). */
  decideProviderRequest(requestId: string, dto: DecideGuardianChangeRequest): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/provider-requests/${requestId}/approve`, dto, {
      headers: this.getHeaders()
    }).pipe(
      catchError(this.handleError)
    );
  }

  /** UC-FAM-11 follow-up report — one day of family-file activity, scoped server-side. */
  getFollowUp(filter: {
    date: string;
    charityId?: string;
    pageNumber?: number;
    pageSize?: number;
  }): Observable<FamilyFollowUpPagedResult> {
    return this.http.get<FamilyFollowUpPagedResult>(`${this.apiUrl}/follow-up`, {
      headers: this.getHeaders(),
      params: this.buildHttpParams(filter)
    }).pipe(
      catchError(this.handleError)
    );
  }

  // ==================== FATHER MANAGEMENT ====================

  getFather(familyId: string): Observable<FatherDto> {
    return this.http.get<FatherDto>(`${this.apiUrl}/${familyId}/father`, {
      headers: this.getHeaders()
    }).pipe(
      catchError(this.handleError)
    );
  }

  createFather(familyId: string, father: CreateFatherDto): Observable<FatherDto> {
    return this.http.post<FatherDto>(`${this.apiUrl}/${familyId}/father`, father, {
      headers: this.getHeaders()
    }).pipe(
      catchError(this.handleError)
    );
  }

  updateFather(familyId: string, father: UpdateFatherDto): Observable<FatherDto> {
    return this.http.put<FatherDto>(`${this.apiUrl}/${familyId}/father`, father, {
      headers: this.getHeaders()
    }).pipe(
      catchError(this.handleError)
    );
  }

  deleteFather(familyId: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${familyId}/father`, {
      headers: this.getHeaders()
    }).pipe(
      catchError(this.handleError)
    );
  }

  // ==================== MOTHER MANAGEMENT ====================

  getMother(familyId: string): Observable<MotherDto> {
    return this.http.get<MotherDto>(`${this.apiUrl}/${familyId}/mother`, {
      headers: this.getHeaders()
    }).pipe(
      catchError(this.handleError)
    );
  }

  createMother(familyId: string, mother: CreateMotherDto): Observable<MotherDto> {
    return this.http.post<MotherDto>(`${this.apiUrl}/${familyId}/mother`, mother, {
      headers: this.getHeaders()
    }).pipe(
      catchError(this.handleError)
    );
  }

  updateMother(familyId: string, mother: UpdateMotherDto): Observable<MotherDto> {
    return this.http.put<MotherDto>(`${this.apiUrl}/${familyId}/mother`, mother, {
      headers: this.getHeaders()
    }).pipe(
      catchError(this.handleError)
    );
  }

  deleteMother(familyId: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${familyId}/mother`, {
      headers: this.getHeaders()
    }).pipe(
      catchError(this.handleError)
    );
  }

  // ==================== PROVIDER MANAGEMENT ====================

  getProvider(familyId: string): Observable<ProviderDto> {
    return this.http.get<ProviderDto>(`${this.apiUrl}/${familyId}/provider`, {
      headers: this.getHeaders()
    }).pipe(
      catchError(this.handleError)
    );
  }

  createProvider(familyId: string, provider: CreateProviderDto): Observable<ProviderDto> {
    return this.http.post<ProviderDto>(`${this.apiUrl}/${familyId}/provider`, provider, {
      headers: this.getHeaders()
    }).pipe(
      catchError(this.handleError)
    );
  }

  updateProvider(familyId: string, provider: UpdateProviderDto): Observable<ProviderDto> {
    return this.http.put<ProviderDto>(`${this.apiUrl}/${familyId}/provider`, provider, {
      headers: this.getHeaders()
    }).pipe(
      catchError(this.handleError)
    );
  }

  deleteProvider(familyId: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${familyId}/provider`, {
      headers: this.getHeaders()
    }).pipe(
      catchError(this.handleError)
    );
  }

  updateProviderType(familyId: string, providerType: string): Observable<void> {
    return this.http.patch<void>(`${this.apiUrl}/${familyId}/provider-type`, { providerType }, {
      headers: this.getHeaders()
    }).pipe(
      catchError(this.handleError)
    );
  }

  // ==================== RELATIVE MANAGEMENT ====================

  getRelatives(familyId: string): Observable<RelativeListDto[]> {
    return this.http.get<RelativeListDto[]>(`${this.apiUrl}/${familyId}/relatives`, {
      headers: this.getHeaders()
    }).pipe(
      catchError(this.handleError)
    );
  }

  createRelative(familyId: string, relative: CreateRelativeDto): Observable<RelativeDto> {
    return this.http.post<RelativeDto>(`${this.apiUrl}/${familyId}/relatives`, relative, {
      headers: this.getHeaders()
    }).pipe(
      catchError(this.handleError)
    );
  }

  updateRelative(relativeId: string, relative: UpdateRelativeDto): Observable<RelativeDto> {
    return this.http.put<RelativeDto>(`${this.apiUrl}/relatives/${relativeId}`, relative, {
      headers: this.getHeaders()
    }).pipe(
      catchError(this.handleError)
    );
  }

  deleteRelative(familyId: string, relativeId: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${familyId}/relatives/${relativeId}`, {
      headers: this.getHeaders()
    }).pipe(
      catchError(this.handleError)
    );
  }

  // ==================== ORPHAN MANAGEMENT ====================

  getOrphans(searchRequest: OrphanSearchRequest): Observable<OrphanPagedResult> {
    return this.http.get<OrphanPagedResult>(`${this.apiUrl}/orphans`, {
      headers: this.getHeaders(),
      params: this.buildHttpParams(searchRequest)
    }).pipe(
      catchError(this.handleError)
    );
  }

  getFamilyOrphans(familyId: string): Observable<OrphanDto[]> {
    return this.http.get<OrphanDto[]>(`${this.apiUrl}/${familyId}/orphans`, {
      headers: this.getHeaders()
    }).pipe(
      catchError(this.handleError)
    );
  }

  getOrphan(id: string): Observable<OrphanDto> {
    return this.http.get<OrphanDto>(`${this.apiUrl}/orphans/${id}`, {
      headers: this.getHeaders()
    }).pipe(
      catchError(this.handleError)
    );
  }

  createOrphan(orphan: CreateOrphanDto): Observable<OrphanDto> {
    const payload = {
      ...orphan,
      photo_Attach: this.convertAttachmentsForBackend(orphan.photo_Attach)
    };

    return this.http.post<OrphanDto>(`${this.apiUrl}/orphans`, payload, {
      headers: this.getHeaders()
    }).pipe(
      catchError(this.handleError)
    );
  }

  updateOrphan(id: string, orphan: UpdateOrphanDto): Observable<OrphanDto> {
    const payload = {
      ...orphan,
      photo_Attach: this.convertAttachmentsForBackend(orphan.photo_Attach)
    };

    return this.http.put<OrphanDto>(`${this.apiUrl}/orphans/${id}`, payload, {
      headers: this.getHeaders()
    }).pipe(
      catchError(this.handleError)
    );
  }

  deleteOrphan(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/orphans/${id}`, {
      headers: this.getHeaders()
    }).pipe(
      catchError(this.handleError)
    );
  }

  // ==================== ATTACHMENT MANAGEMENT ====================

  getFamilyAttachments(familyId: string): Observable<FamilyAttachmentDto[]> {
    return this.http.get<FamilyAttachmentDto[]>(`${this.apiUrl}/${familyId}/attachments`, {
      headers: this.getHeaders()
    }).pipe(
      catchError(this.handleError)
    );
  }

  addFamilyAttachment(familyId: string, attachment: CreateFamilyAttachmentDto): Observable<FamilyAttachmentDto> {
    return this.http.post<FamilyAttachmentDto>(`${this.apiUrl}/${familyId}/attachments`, attachment, {
      headers: this.getHeaders()
    }).pipe(
      catchError(this.handleError)
    );
  }

  deleteFamilyAttachment(familyId: string, attachmentId: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${familyId}/attachments/${attachmentId}`, {
      headers: this.getHeaders()
    }).pipe(
      catchError(this.handleError)
    );
  }

  // ==================== AUDIT LOG ====================

  getAuditLogs(familyId: string, page: number = 1, pageSize: number = 20): Observable<FamilyAuditLog[]> {
    return this.http.get<FamilyAuditLog[]>(`${this.apiUrl}/${familyId}/audit-logs`, {
      headers: this.getHeaders(),
      params: { page, pageSize }
    }).pipe(
      catchError(this.handleError)
    );
  }

  // ==================== EXPORT ====================

  exportFamiliesToExcel(searchRequest: FamilySearchRequest): Observable<Blob> {
    return this.http.post(`${this.apiUrl}/export`, searchRequest, {
      headers: this.getHeaders(),
      responseType: 'blob'
    }).pipe(
      catchError(this.handleError)
    );
  }

  exportFamilyToPDF(id: string): Observable<Blob> {
    return this.http.get(`${this.apiUrl}/${id}/export/pdf`, {
      headers: this.getHeaders(),
      responseType: 'blob'
    }).pipe(
      catchError(this.handleError)
    );
  }

  // ==================== STATISTICS ====================

  getStatistics(): Observable<any> {
    return this.http.get(`${this.apiUrl}/statistics`, {
      headers: this.getHeaders()
    }).pipe(
      catchError(this.handleError)
    );
  }

  // ==================== ORPHAN REGISTER & CODING (Epic 8, UC-ORP-*) ====================

  /** UC-ORP-02/03/07 — shared orphan read: search by name or code, or the coding worklist. */
  searchOrphansCoding(request: OrphanCodingSearchRequest): Observable<OrphanLookupPagedResult> {
    return this.http.get<OrphanLookupPagedResult>(`${this.apiUrl}/orphans`, {
      headers: this.getHeaders(),
      params: this.buildHttpParams(request)
    }).pipe(
      catchError(this.handleError)
    );
  }

  /** UC-ORP-01 — may an orphan with this national ID be added? */
  checkOrphanCanBeAdded(request: OrphanEligibilityCheckRequest): Observable<OrphanEligibilityDto> {
    return this.http.get<OrphanEligibilityDto>(`${this.apiUrl}/orphans/check-national-id`, {
      headers: this.getHeaders(),
      params: this.buildHttpParams(request)
    }).pipe(
      catchError(this.handleError)
    );
  }

  /** UC-SYS-12 — is this national id held by any person on another in-scope family? */
  checkFamilyNationalId(request: FamilyNationalIdCheckRequest): Observable<FamilyNationalIdCheckResult> {
    return this.http.get<FamilyNationalIdCheckResult>(`${this.apiUrl}/check-national-id`, {
      headers: this.getHeaders(),
      params: this.buildHttpParams(request)
    }).pipe(
      catchError(this.handleError)
    );
  }

  /** UC-ORP-05 — verify a sponsorship code is not already used (BR-07). */
  checkOrphanCode(request: OrphanCodeCheckRequest): Observable<OrphanCodeCheckDto> {
    return this.http.get<OrphanCodeCheckDto>(`${this.apiUrl}/orphans/check-code`, {
      headers: this.getHeaders(),
      params: this.buildHttpParams(request)
    }).pipe(
      catchError(this.handleError)
    );
  }

  /** UC-ORP-06 — assign a sponsorship code to an orphan (SaveCode). */
  assignOrphanCode(request: AssignOrphanCodeRequest): Observable<OrphanDto> {
    return this.http.post<OrphanDto>(`${this.apiUrl}/orphans/${request.orphanId}/code`, request, {
      headers: this.getHeaders()
    }).pipe(
      catchError(this.handleError)
    );
  }

  /** UC-ORP-10 — check a phone number is not duplicated across the family's scope. */
  checkPhoneDuplicate(familyId: string, request: PhoneCheckRequest): Observable<PhoneCheckDto> {
    return this.http.get<PhoneCheckDto>(`${this.apiUrl}/${familyId}/provider/check-phone`, {
      headers: this.getHeaders(),
      params: this.buildHttpParams(request)
    }).pipe(
      catchError(this.handleError)
    );
  }

  // ==================== ERROR HANDLING ====================

  private handleError(error: any): Observable<never> {
    console.error('Family service error:', error);
    return throwError(() => {
      const errorMessage = error.error?.message || error.error?.title || 'An unexpected error occurred';
      return {
        message: errorMessage,
        status: error.status || 500,
        details: error.error?.errors || null
      };
    });
  }
}
