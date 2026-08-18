import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { Observable, Subject, takeUntil } from 'rxjs';

import { TechnicalSupportService } from '../services/technical-support.service';
import {
  SupportTicket,
  TicketSearchRequest,
  TicketCategory,
  TicketPriority,
  TicketStatus
} from '../../../core/models/technical-support.model';
import { AuthService, User } from '../../../core/services/auth.service';
import { PagedResponse } from '../../../core/models/common.model';
import { PaginationComponent, BreadcrumbComponent, BreadcrumbItem } from '../../../shared/components';

// Local interfaces (data-list component not yet implemented)
export interface DataColumn {
  key: string;
  title: string;
  type?: 'text' | 'number' | 'date' | 'datetime' | 'boolean' | 'actions' | 'custom';
  sortable?: boolean;
  filterable?: boolean;
  width?: string;
}

export interface ListAction {
  key: string;
  label: string;
  icon?: string;
  cssClass?: string;
  show?: (item: SupportTicket) => boolean;
}

export interface ActionItem {
  label: string;
  icon: string;
  action: string;
  cssClass?: string;
}

@Component({
  selector: 'app-ticket-list',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    FormsModule,
    TranslateModule,
    PaginationComponent,
    BreadcrumbComponent
  ],
  templateUrl: './ticket-list.component.html',
  styleUrls: ['./ticket-list.component.scss']
})
export class TicketListComponent implements OnInit, OnDestroy {
  private destroy$ = new Subject<void>();

  // Breadcrumb items
  breadcrumbs: BreadcrumbItem[] = [
    { label: 'common.home', url: '/dashboard' },
    { label: 'technicalSupport.title' }
  ];

  tickets: SupportTicket[] = [];
  loading: boolean = false;
  viewMode: 'my-tickets' | 'all-tickets' = 'my-tickets';
  currentUser: User | null = null;
  isAdmin: boolean = false;

  // Search and filters
  searchRequest: TicketSearchRequest = {
    pageNumber: 1,
    pageSize: 10,
    sortBy: 'createdDate',
    sortDescending: true
  };

  searchTerm: string = '';
  selectedCategory: TicketCategory | '' = '';
  selectedPriority: TicketPriority | '' = '';
  selectedStatus: TicketStatus | '' = '';
  selectedIsSolved: boolean | '' = '';

  // Pagination
  totalRecords: number = 0;
  pageNumber: number = 1;
  pageSize: number = 10;

  // Data list configuration
  columns: DataColumn[] = [
    {
      key: 'ticketId',
      title: 'technicalSupport.ticketId',
      sortable: true,
      width: '100px'
    },
    {
      key: 'title',
      title: 'technicalSupport.title',
      sortable: true,
      filterable: true
    },
    {
      key: 'category',
      title: 'technicalSupport.category',
      sortable: true,
      filterable: true
    },
    {
      key: 'priority',
      title: 'technicalSupport.priority',
      sortable: true,
      filterable: true
    },
    {
      key: 'status',
      title: 'technicalSupport.status',
      sortable: true,
      filterable: true
    },
    {
      key: 'isSolved',
      title: 'technicalSupport.isSolved',
      type: 'boolean',
      sortable: true
    },
    {
      key: 'createdDate',
      title: 'technicalSupport.createdDate',
      type: 'date',
      sortable: true
    },
    {
      key: 'lastUpdated',
      title: 'technicalSupport.lastUpdated',
      type: 'datetime',
      sortable: true
    }
  ];

  adminColumns: DataColumn[] = [
    {
      key: 'ticketId',
      title: 'technicalSupport.ticketId',
      sortable: true,
      width: '100px'
    },
    {
      key: 'title',
      title: 'technicalSupport.title',
      sortable: true,
      filterable: true
    },
    {
      key: 'category',
      title: 'technicalSupport.category',
      sortable: true,
      filterable: true
    },
    {
      key: 'priority',
      title: 'technicalSupport.priority',
      sortable: true,
      filterable: true
    },
    {
      key: 'status',
      title: 'technicalSupport.status',
      sortable: true,
      filterable: true
    },
    {
      key: 'userName',
      title: 'technicalSupport.createdBy',
      sortable: true
    },
    {
      key: 'assignedToName',
      title: 'technicalSupport.assignedTo',
      sortable: true
    },
    {
      key: 'isSolved',
      title: 'technicalSupport.isSolved',
      type: 'boolean',
      sortable: true
    },
    {
      key: 'createdDate',
      title: 'technicalSupport.createdDate',
      type: 'date',
      sortable: true
    },
    {
      key: 'lastUpdated',
      title: 'technicalSupport.lastUpdated',
      type: 'datetime',
      sortable: true
    }
  ];

  rowActions: ListAction[] = [
    {
      key: 'view',
      label: 'common.view',
      icon: 'fe fe-eye'
    }
  ];

  adminActions: ListAction[] = [
    {
      key: 'view',
      label: 'common.view',
      icon: 'fe fe-eye'
    },
    {
      key: 'assign',
      label: 'technicalSupport.assignTo',
      icon: 'fe fe-user-plus',
      show: (item) => !item.assignedTo
    },
    {
      key: 'unassign',
      label: 'technicalSupport.unassign',
      icon: 'fe fe-user-minus',
      show: (item) => !!item.assignedTo
    },
    {
      key: 'solve',
      label: 'technicalSupport.markAsSolved',
      icon: 'fe fe-check-circle',
      show: (item) => !item.isSolved && item.status !== TicketStatus.Closed
    },
    {
      key: 'close',
      label: 'technicalSupport.closeTicket',
      icon: 'fe fe-x-circle',
      show: (item) => item.status !== TicketStatus.Closed
    }
  ];

  toolbarActions: ActionItem[] = [
    {
      label: 'technicalSupport.newTicket',
      icon: 'fe fe-plus',
      action: 'create',
      cssClass: 'btn-primary'
    },
    {
      label: 'technicalSupport.reports.title',
      icon: 'fe fe-bar-chart-2',
      action: 'reports',
      cssClass: 'btn-info'
    },
    {
      label: 'common.exportToExcel',
      icon: 'fe fe-file-plus',
      action: 'export',
      cssClass: 'btn-success'
    }
  ];

  // Enum values for filters
  categories = Object.values(TicketCategory);
  priorities = Object.values(TicketPriority);
  statuses = Object.values(TicketStatus);

  constructor(
    private technicalSupportService: TechnicalSupportService,
    private authService: AuthService,
    private translate: TranslateService
  ) {}

  ngOnInit(): void {
    this.currentUser = this.authService.getCurrentUser();
    this.isAdmin = this.authService.hasAnyRole(['Admin', 'SuperAdmin']);

    // Check view mode from route data
    const navigation = window.history.state;
    if (navigation?.viewMode === 'all-tickets' && this.isAdmin) {
      this.viewMode = 'all-tickets';
    }

    this.loadTickets();

    // Subscribe to tickets updates
    this.technicalSupportService.ticketsUpdated$
      .pipe(takeUntil(this.destroy$))
      .subscribe(() => {
        this.loadTickets();
      });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  loadTickets(): void {
    this.loading = true;

    const searchParams: TicketSearchRequest = {
      ...this.searchRequest,
      searchTerm: this.searchTerm || undefined,
      category: this.selectedCategory || undefined,
      priority: this.selectedPriority || undefined,
      status: this.selectedStatus || undefined,
      isSolved: this.selectedIsSolved !== '' ? this.selectedIsSolved : undefined
    };

    const tickets$ = this.viewMode === 'my-tickets' || !this.isAdmin
      ? this.technicalSupportService.getMyTickets(searchParams)
      : this.technicalSupportService.getAllTickets(searchParams);

    tickets$.subscribe({
      next: (response: PagedResponse<SupportTicket>) => {
        this.tickets = response.items;
        this.totalRecords = response.totalCount;
        this.pageNumber = response.pageNumber;
        this.loading = false;
      },
      error: () => {
        this.loading = false;
      }
    });
  }

  onSearch(): void {
    this.searchRequest.pageNumber = 1;
    this.loadTickets();
  }

  onClearFilters(): void {
    this.searchTerm = '';
    this.selectedCategory = '';
    this.selectedPriority = '';
    this.selectedStatus = '';
    this.selectedIsSolved = '';
    this.onSearch();
  }

  onPageChange(page: number): void {
    this.searchRequest.pageNumber = page;
    this.loadTickets();
  }

  onPageSizeChange(size: number): void {
    this.searchRequest.pageSize = size;
    this.searchRequest.pageNumber = 1;
    this.loadTickets();
  }

  onSort(column: string, direction: 'asc' | 'desc'): void {
    this.searchRequest.sortBy = column;
    this.searchRequest.sortDescending = direction === 'desc';
    this.loadTickets();
  }

  onAction(event: { item: SupportTicket | null; action: string }): void {
    switch (event.action) {
      case 'view':
        // Navigate to ticket details
        break;
      case 'create':
        // Navigate to create ticket
        break;
      case 'assign':
        // Open assign dialog
        break;
      case 'unassign':
        if (event.item) {
          this.unassignTicket(event.item);
        }
        break;
      case 'solve':
        // Open solve dialog
        break;
      case 'close':
        if (event.item) {
          this.closeTicket(event.item);
        }
        break;
      case 'export':
        this.exportToExcel();
        break;
      case 'reports':
        // Navigate to reports
        break;
    }
  }

  onViewModeChange(mode: 'my-tickets' | 'all-tickets'): void {
    this.viewMode = mode;
    this.searchRequest.pageNumber = 1;
    this.loadTickets();
  }

  unassignTicket(ticket: SupportTicket): void {
    this.technicalSupportService.unassignTicket(ticket.id).subscribe({
      next: () => {
        this.loadTickets();
      }
    });
  }

  closeTicket(ticket: SupportTicket): void {
    if (confirm(this.translate.instant('technicalSupport.messages.confirmClose'))) {
      this.technicalSupportService.closeTicket(ticket.id).subscribe({
        next: () => {
          this.loadTickets();
        }
      });
    }
  }

  exportToExcel(): void {
    const searchParams: TicketSearchRequest = {
      ...this.searchRequest,
      searchTerm: this.searchTerm || undefined,
      category: this.selectedCategory || undefined,
      priority: this.selectedPriority || undefined,
      status: this.selectedStatus || undefined,
      isSolved: this.selectedIsSolved !== '' ? this.selectedIsSolved : undefined
    };

    this.technicalSupportService.exportTicketsToExcel(searchParams).subscribe((blob: Blob) => {
      const url = window.URL.createObjectURL(blob);
      const a = document.createElement('a');
      a.href = url;
      a.download = `support-tickets-${new Date().toISOString().split('T')[0]}.xlsx`;
      a.click();
      window.URL.revokeObjectURL(url);
    });
  }

  // Pagination helper methods
  getTotalPages(): number {
    return Math.ceil(this.totalRecords / this.pageSize);
  }

  getPages(): number[] {
    // Now using shared pagination component
    return [];
  }

  getCategoryTranslation(category: TicketCategory): string {
    return this.translate.instant(`technicalSupport.categories.${category}`);
  }

  getPriorityTranslation(priority: TicketPriority): string {
    return this.translate.instant(`technicalSupport.priorities.${priority}`);
  }

  getStatusTranslation(status: TicketStatus): string {
    return this.translate.instant(`technicalSupport.statuses.${status}`);
  }

  getPriorityClass(priority: TicketPriority): string {
    switch (priority) {
      case TicketPriority.Urgent:
        return 'badge-danger';
      case TicketPriority.High:
        return 'badge-warning';
      case TicketPriority.Medium:
        return 'badge-info';
      case TicketPriority.Low:
        return 'badge-secondary';
      default:
        return 'badge-light';
    }
  }

  getStatusClass(status: TicketStatus): string {
    switch (status) {
      case TicketStatus.Open:
        return 'badge-primary';
      case TicketStatus.InProgress:
        return 'badge-info';
      case TicketStatus.Resolved:
        return 'badge-success';
      case TicketStatus.Closed:
        return 'badge-secondary';
      default:
        return 'badge-light';
    }
  }

  getDisplayEnd(): number {
    return Math.min(this.pageNumber * this.pageSize, this.totalRecords);
  }
}
