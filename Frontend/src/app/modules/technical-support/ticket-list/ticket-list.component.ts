import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, ActivatedRoute, Router } from '@angular/router';
import { FormBuilder, FormGroup, FormsModule, ReactiveFormsModule } from '@angular/forms';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { Subject, Subscription, forkJoin, of, takeUntil } from 'rxjs';

import { TechnicalSupportService } from '../services/technical-support.service';
import {
  SupportTicket,
  TicketSearchRequest,
  LookupOption
} from '../../../core/models/technical-support.model';
import { AuthService, User } from '../../../core/services/auth.service';
import { NotificationService } from '../../../core/services/notification.service';
import { PagedResponse } from '../../../core/models/common.model';
import { PaginationComponent, BreadcrumbComponent, BreadcrumbItem, DropDownComponent } from '../../../shared/components';
import { SharedModule } from '../../../shared/shared.module';

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
    ReactiveFormsModule,
    TranslateModule,
    PaginationComponent,
    BreadcrumbComponent,
    DropDownComponent,
    SharedModule
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
  isSuperAdmin: boolean = false;

  // Count band above the grid — my count for everyone, all/unsolved for admins
  // (those two endpoints are role-gated server-side). Informational only.
  statistics: { myTickets: number; allTickets: number; unsolved: number } | null = null;

  // Search and filters (wire names of SupportTicketFilterDto)
  searchRequest: TicketSearchRequest = {
    pageNumber: 1,
    pageSize: 10,
    sortBy: 'CreatedOn',
    sortDirection: 'desc'
  };

  // Filter form — the shared select2 drop-downs bind to this (employee-list pattern);
  // control names mirror the old [(ngModel)] filter properties.
  filterForm: FormGroup;

  sortDirection: 'asc' | 'desc' = 'desc';

  // Lookup options (GET /api/SupportTickets/lookups)
  categories: LookupOption[] = [];
  priorities: LookupOption[] = [];
  statuses: LookupOption[] = [];
  closedStatusId: number | null = null;

  // Select2 option arrays ({id, name}) fed to app-drop-down — lookup names are data.
  categoryOptions: Array<{ id: number | string; name: string }> = [];
  priorityOptions: Array<{ id: number | string; name: string }> = [];
  statusOptions: Array<{ id: number | string; name: string }> = [];

  /** Rebuilds the translated "All" label on language switch; torn down in ngOnDestroy. */
  private langChangeSubscription?: Subscription;

  /** Sentinel id meaning "no filter" on the lookup drop-downs. */
  private static readonly ALL = 'all';

  // Pagination
  totalRecords: number = 0;
  pageNumber: number = 1;
  pageSize: number = 10;

  // Data list configuration — only columns the backend's ApplySorting understands are
  // marked sortable (title, priority, status, createdOn, updatedOn); the rest would be
  // silent no-ops.
  columns: DataColumn[] = [
    { key: 'title', title: 'technicalSupport.ticketTitle', sortable: true },
    { key: 'categoryName', title: 'technicalSupport.category' },
    { key: 'priorityName', title: 'technicalSupport.priority', sortable: true },
    { key: 'statusName', title: 'technicalSupport.status', sortable: true },
    { key: 'isSolved', title: 'technicalSupport.isSolved', type: 'boolean' },
    { key: 'createdOn', title: 'technicalSupport.createdDate', type: 'date', sortable: true },
    { key: 'updatedOn', title: 'technicalSupport.lastUpdated', type: 'datetime', sortable: true }
  ];

  adminColumns: DataColumn[] = [
    { key: 'title', title: 'technicalSupport.ticketTitle', sortable: true },
    { key: 'categoryName', title: 'technicalSupport.category' },
    { key: 'priorityName', title: 'technicalSupport.priority', sortable: true },
    { key: 'statusName', title: 'technicalSupport.status', sortable: true },
    { key: 'createdByUserName', title: 'technicalSupport.createdBy' },
    { key: 'assignedToName', title: 'technicalSupport.assignedTo' },
    { key: 'isSolved', title: 'technicalSupport.isSolved', type: 'boolean' },
    { key: 'createdOn', title: 'technicalSupport.createdDate', type: 'date', sortable: true },
    { key: 'updatedOn', title: 'technicalSupport.lastUpdated', type: 'datetime', sortable: true }
  ];

  rowActions: ListAction[] = [
    { key: 'view', label: 'common.view', icon: 'fe fe-eye' }
  ];

  adminActions: ListAction[] = [
    { key: 'view', label: 'common.view', icon: 'fe fe-eye' },
    {
      key: 'edit',
      label: 'common.edit',
      icon: 'fe fe-edit',
      show: () => this.isAdmin
    },
    {
      key: 'solve',
      label: 'technicalSupport.markAsSolved',
      icon: 'fe fe-check-circle',
      show: (item) => !item.isSolved && item.statusId !== this.closedStatusId
    },
    {
      key: 'close',
      label: 'technicalSupport.closeTicket',
      icon: 'fe fe-x-circle',
      show: (item) => this.closedStatusId !== null && item.statusId !== this.closedStatusId
    },
    {
      key: 'delete',
      label: 'common.delete',
      icon: 'fe fe-trash-2',
      show: () => this.isSuperAdmin
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
      label: 'common.exportToExcel',
      icon: 'fe fe-download',
      action: 'export',
      cssClass: 'btn-success'
    },
    {
      label: 'technicalSupport.reports.title',
      icon: 'fe fe-bar-chart-2',
      action: 'reports',
      cssClass: 'btn-info'
    }
  ];

  constructor(
    private fb: FormBuilder,
    private route: ActivatedRoute,
    private router: Router,
    private technicalSupportService: TechnicalSupportService,
    private authService: AuthService,
    private translate: TranslateService,
    private notification: NotificationService
  ) {
    // Initialize filter form
    this.filterForm = this.fb.group({
      searchTerm: [''],
      selectedCategoryId: [TicketListComponent.ALL],
      selectedPriorityId: [TicketListComponent.ALL],
      selectedStatusId: [TicketListComponent.ALL]
    });
  }

  ngOnInit(): void {
    this.currentUser = this.authService.getCurrentUser();
    this.isAdmin = this.authService.hasAnyRole(['Admin', 'SuperAdmin']);
    this.isSuperAdmin = this.authService.hasAnyRole(['SuperAdmin']);

    // View mode comes from the route data (my-tickets / all-tickets)
    const routeViewMode = this.route.snapshot.data['viewMode'];
    if (routeViewMode === 'all-tickets' && this.isAdmin) {
      this.viewMode = 'all-tickets';
    } else if (routeViewMode === 'my-tickets') {
      this.viewMode = 'my-tickets';
    }

    this.loadLookups();
    this.loadTickets();
    this.loadStatistics();

    // Subscribe to tickets updates
    this.technicalSupportService.ticketsUpdated$
      .pipe(takeUntil(this.destroy$))
      .subscribe(() => {
        this.loadTickets();
      });

    // The "All" option label is pre-translated (app-drop-down renders raw text), so a
    // language switch needs a rebuild — the translate pipe can't refresh it.
    this.langChangeSubscription = this.translate.onLangChange.subscribe(() => {
      this.buildFilterOptions();
    });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
    this.langChangeSubscription?.unsubscribe();
  }

  /**
   * Count band — loads once on init. Fails silently like the lookups load:
   * a missing band must not block the grid.
   */
  loadStatistics(): void {
    forkJoin({
      myTickets: this.technicalSupportService.getMyTicketsCount(),
      allTickets: this.isAdmin ? this.technicalSupportService.getAllTicketsCount() : of(0),
      unsolved: this.isAdmin ? this.technicalSupportService.getUnsolvedTicketsCount() : of(0)
    })
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: counts => this.statistics = counts,
        error: (error: any) => console.error('Error loading ticket statistics:', error)
      });
  }

  loadLookups(): void {
    this.technicalSupportService.getTicketLookups().subscribe({
      next: (lookups) => {
        this.categories = lookups.categories ?? [];
        this.priorities = lookups.priorities ?? [];
        this.statuses = lookups.statuses ?? [];
        this.buildFilterOptions();
        // The Closed status is matched by its English name; ColorCode also comes from the lookup
        this.closedStatusId =
          this.statuses.find(s => s.nameEn === 'Closed' || s.name === 'Closed')?.id ?? null;
      }
    });
  }

  /** Filter drop-down options: an explicit "All" first option, then the lookup rows ({id, name}). */
  buildFilterOptions(): void {
    const allOption = { id: TicketListComponent.ALL, name: this.translate.instant('common.all') };
    this.categoryOptions = [allOption, ...this.categories.map(cat => ({ id: cat.id, name: cat.name }))];
    this.priorityOptions = [allOption, ...this.priorities.map(pri => ({ id: pri.id, name: pri.name }))];
    this.statusOptions = [allOption, ...this.statuses.map(sta => ({ id: sta.id, name: sta.name }))];
  }

  loadTickets(): void {
    this.loading = true;
    const filters = this.filterForm.value;

    const searchParams: TicketSearchRequest = {
      ...this.searchRequest,
      searchTerm: filters.searchTerm || undefined,
      categoryId: this.filterIdOrUndefined(filters.selectedCategoryId),
      priorityId: this.filterIdOrUndefined(filters.selectedPriorityId),
      statusId: this.filterIdOrUndefined(filters.selectedStatusId)
    };

    const tickets$ = this.viewMode === 'my-tickets' || !this.isAdmin
      ? this.technicalSupportService.getMyTickets(searchParams)
      : this.technicalSupportService.getAllTickets(searchParams);

    tickets$.subscribe({
      next: (response: PagedResponse<SupportTicket>) => {
        this.tickets = response.items;
        this.totalRecords = response.totalCount;
        this.pageNumber = response.pageNumber || this.searchRequest.pageNumber || 1;
        this.loading = false;
      },
      error: () => {
        this.loading = false;
        this.notification.error(this.translate.instant('technicalSupport.messages.operationFailed'));
      }
    });
  }

  onSearch(): void {
    this.searchRequest.pageNumber = 1;
    this.loadTickets();
  }

  clearFilters(): void {
    this.filterForm.reset({
      searchTerm: '',
      selectedCategoryId: TicketListComponent.ALL,
      selectedPriorityId: TicketListComponent.ALL,
      selectedStatusId: TicketListComponent.ALL
    });
    this.onSearch();
  }

  // Check if any filters are active
  hasActiveFilters(): boolean {
    const filters = this.filterForm.value;
    return !!(
      filters.searchTerm ||
      this.filterIdOrUndefined(filters.selectedCategoryId) !== undefined ||
      this.filterIdOrUndefined(filters.selectedPriorityId) !== undefined ||
      this.filterIdOrUndefined(filters.selectedStatusId) !== undefined
    );
  }

  /** The 'all' sentinel (and empty values) map to undefined — no filter in the request. */
  private filterIdOrUndefined(id: number | string | null | undefined): number | undefined {
    return id !== null && id !== undefined && id !== TicketListComponent.ALL ? Number(id) : undefined;
  }

  onPageChange(page: number): void {
    this.searchRequest.pageNumber = page;
    this.loadTickets();
  }

  onPageSizeChange(size: number): void {
    this.searchRequest.pageSize = size;
    this.pageSize = size;
    this.searchRequest.pageNumber = 1;
    this.loadTickets();
  }

  // Backend ApplySorting understands: title, createdon, updatedon, priority, status
  private static readonly SORT_KEY_MAP: Record<string, string> = {
    title: 'Title',
    priorityName: 'Priority',
    statusName: 'Status',
    createdOn: 'CreatedOn',
    updatedOn: 'UpdatedOn'
  };

  private sortKeyFor(column: string): string | undefined {
    return TicketListComponent.SORT_KEY_MAP[column];
  }

  isSortActive(column: string): boolean {
    return this.searchRequest.sortBy === this.sortKeyFor(column);
  }

  onSort(column: string): void {
    const sortKey = this.sortKeyFor(column);
    if (!sortKey) {
      return;
    }
    if (this.searchRequest.sortBy === sortKey) {
      this.sortDirection = this.sortDirection === 'asc' ? 'desc' : 'asc';
    } else {
      this.sortDirection = 'desc';
    }
    this.searchRequest.sortBy = sortKey;
    this.searchRequest.sortDirection = this.sortDirection;
    this.loadTickets();
  }

  getSortIcon(column: string): string {
    // feather.css has no chevrons-up-down glyph — the icon renders only on the active column
    return this.sortDirection === 'asc'
      ? 'fe fe-chevron-up ml-1 sort-icon'
      : 'fe fe-chevron-down ml-1 sort-icon';
  }

  onAction(event: { item: SupportTicket | null; action: string }): void {
    switch (event.action) {
      case 'view':
        if (event.item) {
          this.router.navigate(['/technical-support', event.item.id]);
        }
        break;
      case 'edit':
        if (event.item) {
          this.router.navigate(['/technical-support', event.item.id, 'edit']);
        }
        break;
      case 'create':
        this.router.navigate(['/technical-support', 'create']);
        break;
      case 'solve':
        // The solve form lives on the detail screen
        if (event.item) {
          this.router.navigate(['/technical-support', event.item.id]);
        }
        break;
      case 'close':
        if (event.item) {
          this.closeTicket(event.item);
        }
        break;
      case 'delete':
        if (event.item) {
          this.deleteTicket(event.item);
        }
        break;
      case 'reports':
        this.router.navigate(['/technical-support', 'reports']);
        break;
      case 'export':
        this.exportToExcel();
        break;
    }
  }

  onViewModeChange(mode: 'my-tickets' | 'all-tickets'): void {
    this.viewMode = mode;
    this.searchRequest.pageNumber = 1;
    this.loadTickets();
  }

  /**
   * Export the current filtered register to Excel (charity-list pattern). The
   * current filters ride along; the server widens the page to every matching row
   * and decides my-tickets vs. all-tickets scope from the caller's roles.
   */
  exportToExcel(): void {
    this.loading = true;
    const filters = this.filterForm.value;

    const searchParams: TicketSearchRequest = {
      ...this.searchRequest,
      pageNumber: 1,
      pageSize: 100000,
      searchTerm: filters.searchTerm || undefined,
      categoryId: this.filterIdOrUndefined(filters.selectedCategoryId),
      priorityId: this.filterIdOrUndefined(filters.selectedPriorityId),
      statusId: this.filterIdOrUndefined(filters.selectedStatusId)
    };

    this.technicalSupportService.exportToExcel(searchParams).subscribe({
      next: (blob: Blob) => {
        const url = window.URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = `support-tickets_${new Date().toISOString().split('T')[0]}.xlsx`;
        document.body.appendChild(a);
        a.click();
        document.body.removeChild(a);
        window.URL.revokeObjectURL(url);

        this.notification.success(this.translate.instant('common.operationSuccess'));
        this.loading = false;
      },
      error: () => {
        this.loading = false;
        this.notification.error(this.translate.instant('common.operationFailed'));
      }
    });
  }

  closeTicket(ticket: SupportTicket): void {
    if (this.closedStatusId === null) {
      // Lookups not loaded — closing is impossible; say so instead of silently doing nothing
      this.notification.error(this.translate.instant('technicalSupport.messages.operationFailed'));
      return;
    }

    if (confirm(this.translate.instant('technicalSupport.messages.confirmClose'))) {
      this.technicalSupportService
        .updateTicketStatus(ticket.id, { statusId: this.closedStatusId })
        .subscribe({
          next: () => {
            this.technicalSupportService.notifyTicketsUpdated();
          },
          error: () => {
            this.notification.error(this.translate.instant('technicalSupport.messages.operationFailed'));
          }
        });
    }
  }

  deleteTicket(ticket: SupportTicket): void {
    if (confirm(
      this.translate.instant('technicalSupport.messages.confirmDelete', { title: ticket.title })
    )) {
      this.technicalSupportService.deleteTicket(ticket.id).subscribe({
        next: () => {
          this.technicalSupportService.notifyTicketsUpdated();
        },
        error: () => {
          this.notification.error(this.translate.instant('technicalSupport.messages.operationFailed'));
        }
      });
    }
  }

  // Display helpers
  shortId(id: string): string {
    return id ? id.substring(0, 8).toUpperCase() : '';
  }

  trackByTicketId(index: number, ticket: SupportTicket): string {
    return ticket.id;
  }

  trackByActionKey(index: number, action: ActionItem): string {
    return action.action;
  }

  getDisplayEnd(): number {
    return Math.min(this.pageNumber * this.pageSize, this.totalRecords);
  }

  getTotalPages(): number {
    return Math.ceil(this.totalRecords / this.pageSize);
  }
}
