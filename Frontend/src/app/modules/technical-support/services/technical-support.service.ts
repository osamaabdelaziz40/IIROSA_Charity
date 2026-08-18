import { Injectable } from '@angular/core';
import { Observable, BehaviorSubject } from 'rxjs';
import { ApiService } from '../../../core/services/api.service';
import {
  SupportTicket,
  CreateTicketRequest,
  UpdateTicketStatusRequest,
  MarkTicketSolvedRequest,
  AddTicketResponseRequest,
  TicketSearchRequest,
  TicketListResponse,
  SupportReportRequest,
  SupportReport
} from '../../../core/models/technical-support.model';
import { ApiResponse, PagedResponse } from '../../../core/models/common.model';

@Injectable({
  providedIn: 'root'
})
export class TechnicalSupportService {
  private readonly endpoint = '/api/technicalsupport';
  private ticketsUpdated = new BehaviorSubject<number>(0);
  ticketsUpdated$ = this.ticketsUpdated.asObservable();

  constructor(private api: ApiService) {}

  notifyTicketsUpdated(): void {
    this.ticketsUpdated.next(Date.now());
  }

  // Ticket CRUD Operations
  getMyTickets(search: TicketSearchRequest): Observable<PagedResponse<SupportTicket>> {
    return this.api.get(`${this.endpoint}/mytickets`, search);
  }

  getAllTickets(search: TicketSearchRequest): Observable<PagedResponse<SupportTicket>> {
    return this.api.get(`${this.endpoint}`, search);
  }

  getTicketById(id: string): Observable<SupportTicket> {
    return this.api.get(`${this.endpoint}/${id}`);
  }

  createTicket(request: CreateTicketRequest): Observable<ApiResponse<SupportTicket>> {
    const formData = this.createTicketFormData(request);
    return this.api.post(`${this.endpoint}`, formData);
  }

  updateTicketStatus(
    id: string,
    request: UpdateTicketStatusRequest
  ): Observable<ApiResponse<SupportTicket>> {
    return this.api.patch(`${this.endpoint}/${id}/status`, request);
  }

  markTicketAsSolved(
    id: string,
    request: MarkTicketSolvedRequest
  ): Observable<ApiResponse<SupportTicket>> {
    const formData = this.createSolvedFormData(request);
    return this.api.patch(`${this.endpoint}/${id}/solve`, formData);
  }

  closeTicket(id: string): Observable<ApiResponse<SupportTicket>> {
    return this.api.patch(`${this.endpoint}/${id}/close`, {});
  }

  deleteTicket(id: string): Observable<ApiResponse<boolean>> {
    return this.api.delete(`${this.endpoint}/${id}`);
  }

  // Ticket Responses
  addTicketResponse(
    id: string,
    request: AddTicketResponseRequest
  ): Observable<ApiResponse<SupportTicket>> {
    const formData = this.createResponseFormData(request);
    return this.api.post(`${this.endpoint}/${id}/responses`, formData);
  }

  getTicketResponses(id: string): Observable<any> {
    return this.api.get(`${this.endpoint}/${id}/responses`);
  }

  deleteTicketResponse(ticketId: string, responseId: string): Observable<ApiResponse<boolean>> {
    return this.api.delete(`${this.endpoint}/${ticketId}/responses/${responseId}`);
  }

  // Ticket Attachments
  uploadTicketAttachment(ticketId: string, file: File, description?: string): Observable<any> {
    const formData = new FormData();
    formData.append('file', file);
    if (description) {
      formData.append('description', description);
    }
    return this.api.post(`${this.endpoint}/${ticketId}/attachments`, formData);
  }

  downloadTicketAttachment(ticketId: string, attachmentId: string): Observable<Blob> {
    return this.api.download(`${this.endpoint}/${ticketId}/attachments/${attachmentId}`);
  }

  deleteTicketAttachment(ticketId: string, attachmentId: string): Observable<ApiResponse<boolean>> {
    return this.api.delete(`${this.endpoint}/${ticketId}/attachments/${attachmentId}`);
  }

  // Ticket Assignment
  assignTicket(ticketId: string, userId: string): Observable<ApiResponse<SupportTicket>> {
    return this.api.post(`${this.endpoint}/${ticketId}/assign`, { userId });
  }

  unassignTicket(ticketId: string): Observable<ApiResponse<SupportTicket>> {
    return this.api.post(`${this.endpoint}/${ticketId}/unassign`, {});
  }

  // Search and Filter
  searchTickets(search: TicketSearchRequest): Observable<PagedResponse<SupportTicket>> {
    return this.api.get(`${this.endpoint}/search`, search);
  }

  getTicketCategories(): Observable<string[]> {
    return this.api.get(`${this.endpoint}/categories`);
  }

  getTicketPriorities(): Observable<string[]> {
    return this.api.get(`${this.endpoint}/priorities`);
  }

  getTicketStatuses(): Observable<string[]> {
    return this.api.get(`${this.endpoint}/statuses`);
  }

  // Reports
  generateReport(request: SupportReportRequest): Observable<SupportReport> {
    return this.api.post(`${this.endpoint}/reports`, request);
  }

  exportTicketsToExcel(search: TicketSearchRequest): Observable<Blob> {
    return this.api.getBlob(`${this.endpoint}/export/excel`, search);
  }

  exportTicketsToPDF(search: TicketSearchRequest): Observable<Blob> {
    return this.api.getBlob(`${this.endpoint}/export/pdf`, search);
  }

  // Statistics
  getTicketStatistics(): Observable<any> {
    return this.api.get(`${this.endpoint}/statistics`);
  }

  getMyTicketStatistics(): Observable<any> {
    return this.api.get(`${this.endpoint}/mytickets/statistics`);
  }

  // Helper Methods
  private createTicketFormData(request: CreateTicketRequest): FormData {
    const formData = new FormData();
    formData.append('title', request.title);
    formData.append('message', request.message);
    formData.append('category', request.category);
    formData.append('priority', request.priority);

    if (request.attachedFile) {
      formData.append('attachedFile', request.attachedFile);
    }
    if (request.browserInfo) {
      formData.append('browserInfo', request.browserInfo);
    }
    if (request.pageUrl) {
      formData.append('pageUrl', request.pageUrl);
    }
    if (request.userAction) {
      formData.append('userAction', request.userAction);
    }

    return formData;
  }

  private createSolvedFormData(request: MarkTicketSolvedRequest): FormData {
    const formData = new FormData();
    formData.append('resolutionDescription', request.resolutionDescription);

    if (request.solutionSteps) {
      formData.append('solutionSteps', request.solutionSteps);
    }
    if (request.attachment) {
      formData.append('attachment', request.attachment);
    }

    return formData;
  }

  private createResponseFormData(request: AddTicketResponseRequest): FormData {
    const formData = new FormData();
    formData.append('responseText', request.responseText);
    formData.append('isInternal', request.isInternal.toString());

    if (request.attachment) {
      formData.append('attachment', request.attachment);
    }

    return formData;
  }

  // Browser Information Detection
  detectBrowserInfo(): string {
    const ua = navigator.userAgent;
    let browserName = 'Unknown';

    if (ua.indexOf('Chrome') > -1) {
      browserName = 'Chrome';
    } else if (ua.indexOf('Safari') > -1) {
      browserName = 'Safari';
    } else if (ua.indexOf('Firefox') > -1) {
      browserName = 'Firefox';
    } else if (ua.indexOf('MSIE') > -1 || ua.indexOf('Trident/') > -1) {
      browserName = 'Internet Explorer';
    } else if (ua.indexOf('Edge') > -1) {
      browserName = 'Edge';
    }

    const version = ua.match(/(Chrome|Firefox|Safari|Edge)\/([\d.]+)/);
    const browserVersion = version ? version[2] : 'Unknown';

    return `${browserName} ${browserVersion}`;
  }

  getCurrentPageUrl(): string {
    return window.location.href;
  }
}
