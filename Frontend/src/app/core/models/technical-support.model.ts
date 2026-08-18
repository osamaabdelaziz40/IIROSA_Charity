export interface SupportTicket {
  id: string;
  title: string;
  message: string;
  category: TicketCategory;
  priority: TicketPriority;
  status: TicketStatus;
  isSolved: boolean;
  userId: string;
  userName?: string;
  userEmail?: string;
  userRole?: string;
  assignedTo?: string;
  assignedToName?: string;
  attachedFile?: string;
  attachedFileName?: string;
  browserInfo?: string;
  pageUrl?: string;
  userAction?: string;
  createdDate: string;
  lastUpdated: string;
  resolutionDescription?: string;
  resolutionDate?: string;
  responses?: TicketResponse[];
}

export interface TicketResponse {
  id: string;
  ticketId: string;
  responseText: string;
  respondedBy: string;
  respondedByName?: string;
  responseDate: string;
  attachment?: string;
  attachmentName?: string;
  isInternal: boolean;
}

export interface TicketAttachment {
  id: string;
  ticketId: string;
  fileName: string;
  fileUrl: string;
  fileSize: number;
  uploadedBy: string;
  uploadedDate: string;
  description?: string;
}

export interface CreateTicketRequest {
  title: string;
  message: string;
  category: TicketCategory;
  priority: TicketPriority;
  attachedFile?: File;
  browserInfo?: string;
  pageUrl?: string;
  userAction?: string;
}

export interface UpdateTicketStatusRequest {
  status: TicketStatus;
  note?: string;
}

export interface MarkTicketSolvedRequest {
  resolutionDescription: string;
  solutionSteps?: string;
  attachment?: File;
}

export interface AddTicketResponseRequest {
  responseText: string;
  attachment?: File;
  isInternal: boolean;
}

export interface TicketSearchRequest {
  pageNumber?: number;
  pageSize?: number;
  searchTerm?: string;
  category?: TicketCategory;
  priority?: TicketPriority;
  status?: TicketStatus;
  isSolved?: boolean;
  userId?: string;
  assignedTo?: string;
  startDate?: string;
  endDate?: string;
  sortBy?: string;
  sortDescending?: boolean;
}

export interface TicketListResponse {
  tickets: SupportTicket[];
  totalRecords: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
  hasPrevious: boolean;
  hasNext: boolean;
}

export interface SupportReportRequest {
  startDate: string;
  endDate: string;
  groupBy?: ReportGroupBy;
  includeCategories?: boolean;
  includeUsers?: boolean;
}

export interface SupportReport {
  summaryStatistics: ReportSummary;
  performanceMetrics: ReportPerformance;
  userStatistics: ReportUserStats[];
  trendAnalysis: ReportTrend[];
  tickets: SupportTicket[];
}

export interface ReportSummary {
  totalTickets: number;
  ticketsByStatus: Record<TicketStatus, number>;
  ticketsByPriority: Record<TicketPriority, number>;
  ticketsByCategory: Record<TicketCategory, number>;
  solvedVsUnsolved: {
    solved: number;
    unsolved: number;
  };
}

export interface ReportPerformance {
  averageResolutionTime: number; // in hours
  ticketsResolvedWithinSLA: number;
  ticketsOpenedVsClosed: {
    opened: number;
    closed: number;
  };
}

export interface ReportUserStats {
  userId: string;
  userName: string;
  ticketCount: number;
  solvedCount: number;
}

export interface ReportTrend {
  date: string;
  ticketsOpened: number;
  ticketsClosed: number;
  resolutionTime?: number;
}

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

export enum ReportGroupBy {
  Category = 'Category',
  Status = 'Status',
  User = 'User',
  Priority = 'Priority'
}
