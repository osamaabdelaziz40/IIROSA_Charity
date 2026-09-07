import { Injectable } from '@angular/core';
import { Observable, BehaviorSubject, map } from 'rxjs';
import { ApiService } from '../../../core/services/api.service';
import {
  SupportTicket,
  CreateTicketRequest,
  UpdateTicketRequest,
  UpdateTicketStatusRequest,
  MarkTicketSolvedRequest,
  AddTicketResponseRequest,
  TicketResponse,
  TicketSearchRequest,
  TicketLookups,
  SupportReportRequest,
  SupportReport
} from '../../../core/models/technical-support.model';
import { PagedResponse } from '../../../core/models/common.model';

/**
 * Technical Support service — wired to the real SupportTicketsController
 * (route api/SupportTickets). Responses are bare DTOs; the controller does not
 * use the ApiResponse envelope, so never read `.value` off a response here.
 */
@Injectable({
  providedIn: 'root'
})
export class TechnicalSupportService {
  private readonly endpoint = '/api/SupportTickets';
  private ticketsUpdated = new BehaviorSubject<number>(0);
  ticketsUpdated$ = this.ticketsUpdated.asObservable();

  constructor(private api: ApiService) {}

  notifyTicketsUpdated(): void {
    this.ticketsUpdated.next(Date.now());
  }

  // Ticket CRUD Operations
  getMyTickets(search: TicketSearchRequest): Observable<PagedResponse<SupportTicket>> {
    return this.api
      .get<any>(`${this.endpoint}/my-tickets`, search)
      .pipe(map(res => this.toPagedResponse(res, search)));
  }

  getAllTickets(search: TicketSearchRequest): Observable<PagedResponse<SupportTicket>> {
    return this.api
      .get<any>(`${this.endpoint}/all-tickets`, search)
      .pipe(map(res => this.toPagedResponse(res, search)));
  }

  getTicketById(id: string): Observable<SupportTicket> {
    return this.api.get<SupportTicket>(`${this.endpoint}/${id}`);
  }

  createTicket(request: CreateTicketRequest): Observable<SupportTicket> {
    return this.api.post<SupportTicket>(`${this.endpoint}`, request);
  }

  updateTicket(id: string, request: UpdateTicketRequest): Observable<SupportTicket> {
    return this.api.put<SupportTicket>(`${this.endpoint}/${id}`, request);
  }

  deleteTicket(id: string): Observable<void> {
    return this.api.delete<void>(`${this.endpoint}/${id}`);
  }

  // Ticket form lookups
  getTicketLookups(): Observable<TicketLookups> {
    return this.api.get<TicketLookups>(`${this.endpoint}/lookups`);
  }

  // Status / resolution management
  updateTicketStatus(
    id: string,
    request: UpdateTicketStatusRequest
  ): Observable<{ message: string }> {
    return this.api.put<{ message: string }>(`${this.endpoint}/${id}/status`, {
      ticketId: id,
      statusId: request.statusId,
      statusNote: request.statusNote
    });
  }

  markTicketAsSolved(
    id: string,
    request: MarkTicketSolvedRequest
  ): Observable<{ message: string }> {
    return this.api.post<{ message: string }>(`${this.endpoint}/${id}/mark-solved`, {
      ticketId: id,
      resolutionDescription: request.resolutionDescription,
      solutionSteps: request.solutionSteps
    });
  }

  // Ticket Responses
  addTicketResponse(id: string, request: AddTicketResponseRequest): Observable<TicketResponse> {
    return this.api.post<TicketResponse>(`${this.endpoint}/${id}/responses`, {
      ticketId: id,
      responseText: request.responseText,
      isInternalNote: request.isInternalNote
    });
  }

  // Reports
  generateReport(request: SupportReportRequest): Observable<SupportReport> {
    return this.api.post<SupportReport>(`${this.endpoint}/report`, request);
  }

  // Statistics
  getMyTicketsCount(): Observable<number> {
    return this.api.get<number>(`${this.endpoint}/my-tickets/count`);
  }

  getAllTicketsCount(): Observable<number> {
    return this.api.get<number>(`${this.endpoint}/all-tickets/count`);
  }

  getUnsolvedTicketsCount(): Observable<number> {
    return this.api.get<number>(`${this.endpoint}/unsolved/count`);
  }

  /**
   * The list endpoints return Ok(new { result.Items, result.TotalCount }),
   * which serializes as { items: [...], totalCount: N } (Newtonsoft camelCase).
   */
  private toPagedResponse(
    payload: { items?: SupportTicket[]; totalCount?: number } | null,
    search: TicketSearchRequest
  ): PagedResponse<SupportTicket> {
    const items = payload?.items ?? [];
    const totalCount = payload?.totalCount ?? 0;
    const pageNumber = search?.pageNumber ?? 1;
    const pageSize = search?.pageSize ?? 10;
    return {
      items,
      totalCount,
      pageNumber,
      pageSize,
      totalPages: Math.max(1, Math.ceil(totalCount / pageSize)),
      hasPrevious: pageNumber > 1,
      hasNext: pageNumber * pageSize < totalCount
    };
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
