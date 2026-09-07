// Wire model for /api/SupportTickets (SupportTicketDto / SupportTicketDetailDto / SupportTicketListDto).
// Field names match the API JSON exactly — camelCase mirrors of the C# DTOs.
export interface SupportTicket {
  id: string;
  title: string;
  message?: string;

  // Lookup ids + localized names
  categoryId: number;
  categoryName?: string;
  priorityId: number;
  priorityName?: string;
  statusId: number;
  statusName?: string;
  priorityColor?: string;
  statusColor?: string;

  // Resolution
  isSolved?: boolean;
  resolutionDescription?: string;
  resolvedOn?: string;
  resolvedBy?: string;

  // System information
  browserInfo?: string;
  pageUrl?: string;
  userAction?: string;

  // Assignment
  assignedTo?: string;
  assignedToName?: string;

  // Creator
  createdByUserId?: string;
  createdByUserName?: string;
  createdByEmail?: string;

  // Attachment
  attachmentFileName?: string;
  attachmentFilePath?: string;
  attachmentFileSize?: number;

  // Audit
  createdOn: string;
  updatedOn: string;

  responseCount?: number;
  responses?: TicketResponse[];
  publicResponses?: TicketResponse[];
  internalNotes?: TicketResponse[];
}

export interface TicketResponse {
  id: string;
  ticketId: string;
  responseText: string;
  isInternalNote: boolean;
  respondedByUserId: string;
  responderName?: string;
  responderEmail?: string;
  attachmentFileName?: string;
  attachmentFilePath?: string;
  attachmentFileSize?: number;
  createdOn: string;
}

export interface CreateTicketRequest {
  title: string;
  message: string;
  categoryId: number;
  priorityId: number;
  browserInfo?: string;
  pageUrl?: string;
  userAction?: string;
}

export interface UpdateTicketRequest {
  id: string;
  title: string;
  message: string;
  categoryId: number;
  priorityId: number;
  /** Optional — omit to leave the ticket's status unchanged on the server. */
  statusId?: number;
}

export interface UpdateTicketStatusRequest {
  statusId: number;
  statusNote?: string;
}

export interface MarkTicketSolvedRequest {
  resolutionDescription: string;
  solutionSteps?: string;
}

export interface AddTicketResponseRequest {
  responseText: string;
  isInternalNote: boolean;
}

// Query params of SupportTicketFilterDto, camelCase as the API binds them.
export interface TicketSearchRequest {
  pageNumber?: number;
  pageSize?: number;
  searchTerm?: string;
  categoryId?: number;
  priorityId?: number;
  statusId?: number;
  isSolved?: boolean;
  assignedTo?: string;
  createdByUserId?: string;
  startDate?: string;
  endDate?: string;
  sortBy?: string;
  sortDirection?: string;
}

// GET /api/SupportTickets/lookups → TicketLookupsDto
export interface LookupOption {
  id: number;
  name: string;
  nameAr?: string;
  nameEn?: string;
  isActive?: boolean;
}

export interface TicketLookups {
  categories: LookupOption[];
  priorities: LookupOption[];
  statuses: LookupOption[];
}

// POST /api/SupportTickets/report → SupportTicketReportDto
export interface SupportReportRequest {
  startDate: string;
  endDate: string;
}

export interface SupportReport {
  startDate: string;
  endDate: string;
  totalTickets: number;
  solvedTickets: number;
  unsolvedTickets: number;
  openTickets: number;
  inProgressTickets: number;
  resolvedTickets: number;
  closedTickets: number;
  ticketsByStatus: Record<string, number>;
  ticketsByPriority: Record<string, number>;
  ticketsByCategory: Record<string, number>;
  ticketsByCreator: Record<string, number>;
  averageResolutionTimeHours: number;
  // Newtonsoft camelCase keeps a trailing acronym uppercase (…WithinSLA → …WithinSLA)
  ticketsResolvedWithinSLA: number;
  ticketsBreachedSLA: number;
  tickets: SupportTicket[];
}

// Enum mirrors of the seeded lookup tables (labels come from the lookups endpoint,
// which returns the localized NameAr ?? NameEn).
export enum TicketCategory {
  Technical = 'Technical',
  Access = 'Access',
  Data = 'Data',
  FeatureRequest = 'FeatureRequest',
  Bug = 'Bug',
  Other = 'Other'
}

export enum TicketPriority {
  Low = 'Low',
  Medium = 'Medium',
  High = 'High',
  Urgent = 'Urgent'
}

export enum TicketStatus {
  Open = 'Open',
  InProgress = 'InProgress',
  Resolved = 'Resolved',
  Closed = 'Closed'
}
