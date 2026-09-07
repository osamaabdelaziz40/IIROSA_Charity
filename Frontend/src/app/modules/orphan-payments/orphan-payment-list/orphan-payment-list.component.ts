import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { FormBuilder, FormGroup, ReactiveFormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { TranslateModule, TranslateService, LangChangeEvent } from '@ngx-translate/core';
import { Subject, Subscription, takeUntil } from 'rxjs';
import {
  OrphanPaymentDto,
  OrphanPaymentSearchRequest,
  CURRENCY_OPTIONS,
  SPONSORSHIP_STATUS_OPTIONS
} from '../models/orphan-payment.model';
import { OrphanPaymentService } from '../services/orphan-payment.service';
import { NotificationService } from '../../../core/services/notification.service';
import { AuthService } from '../../../core/services/auth.service';
import { CharityService } from '../../charities/services/charity.service';
import { PageHeaderComponent } from '../../../shared/components/page-header/page-header.component';
import { BreadcrumbComponent, BreadcrumbItem } from '../../../shared/components';
import { PaginationComponent } from '../../../shared/components';
import { SharedModule } from '../../../shared/shared.module';
import { DropDownComponent } from '../../../shared/components/drop-down/drop-down.component';

export interface ListAction {
  key: string;
  label: string;
  icon?: string;
  cssClass?: string;
  show?: (item: OrphanPaymentDto) => boolean;
}

@Component({
  selector: 'app-orphan-payment-list',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    PageHeaderComponent,
    BreadcrumbComponent,
    TranslateModule,
    RouterModule,
    PaginationComponent,
    SharedModule,
    DropDownComponent
  ],
  templateUrl: './orphan-payment-list.component.html',
  styleUrls: ['./orphan-payment-list.component.scss']
})
export class OrphanPaymentListComponent implements OnInit, OnDestroy {
  private destroy$ = new Subject<void>();
  private langChangeSubscription?: Subscription;

  // Expose Math to template
  Math = Math;

  // Data
  paymentGroups: OrphanPaymentDto[] = [];
  totalRecords = 0;
  loading = false;

  // Review P3b: a failed load renders an explicit error state instead of a silently
  // blank grid (a 403 used to vanish here).
  loadError = false;

  // Review D1 (2026-08-26): the §15.S.1 list-screen الجمعية filter — HQ only. Charity
  // users get no selector; their claim pins the scope server-side (pin-never-widen).
  isHqUser = false;
  charityOptions: { id: string; name: string }[] = [];

  // Pagination
  currentPage = 1;
  pageSize = 10;

  // Row actions
  rowActions: ListAction[] = [];

  // Filter form
  filterForm: FormGroup;

  // Upload status options for dropdown
  uploadStatusOptions: Array<{ id: string | boolean | null; name: string }> = [];

  // Page actions
  pageActions = [
    {
      label: 'orphanPayments.addNewGroup',
      type: 'primary',
      icon: 'fe-plus',
      click: () => this.createPaymentGroup()
    },
    {
      label: 'common.exportToExcel',
      type: 'success',
      icon: 'fe-download',
      click: () => this.exportToExcel()
    },
    {
      label: 'common.refresh',
      type: 'secondary',
      icon: 'fe-refresh-cw',
      click: () => this.loadPaymentGroups()
    }
  ];

  // Breadcrumbs
  breadcrumbs: BreadcrumbItem[] = [
    { label: 'common.home', url: '/dashboard' },
    { label: 'orphanPayments.title' }
  ];

  constructor(
    private fb: FormBuilder,
    private orphanPaymentService: OrphanPaymentService,
    private notificationService: NotificationService,
    private authService: AuthService,
    private charityService: CharityService,
    private router: Router,
    private route: ActivatedRoute,
    private translate: TranslateService
  ) {
    // Initialize filter form
    this.filterForm = this.fb.group({
      searchValue: [''],
      paymentPeriodFrom: [''],
      paymentPeriodTo: [''],
      isBatchUploaded: [null],
      charityId: [null]
    });
  }

  ngOnInit(): void {
    this.initializeUploadStatusOptions();
    this.initializeRowActions();
    this.initializeFilters();

    // Review D1: HQ users get the الجمعية dropdown (كافة الجهات = unfiltered); the
    // selected charity feeds the server-side join filter (UC-5.12).
    this.isHqUser = this.authService.hasAnyRole(['SuperAdmin', 'Admin', 'Accountant', 'FinancialOfficer']);
    if (this.isHqUser) {
      this.loadCharityOptions();
    }

    // Subscribe to language changes to update translated options
    this.langChangeSubscription = this.translate.onLangChange.subscribe((event: LangChangeEvent) => {
      this.initializeUploadStatusOptions();
    });

    this.loadPaymentGroups();

    // Check for query params for navigation from other components
    this.route.queryParams.pipe(takeUntil(this.destroy$)).subscribe(params => {
      if (params['refresh'] === 'true') {
        this.loadPaymentGroups();
      }
    });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
    if (this.langChangeSubscription) {
      this.langChangeSubscription.unsubscribe();
    }
  }

  private initializeUploadStatusOptions(): void {
    this.uploadStatusOptions = [
      { id: null, name: this.translate.instant('common.all') },
      { id: true, name: this.translate.instant('orphanPayments.uploaded') },
      { id: false, name: this.translate.instant('orphanPayments.pending') }
    ];
  }

  private initializeRowActions(): void {
    this.rowActions = [
      {
        key: 'view',
        label: 'common.view',
        icon: 'fe fe-eye',
        cssClass: 'btn-info',
        show: () => true
      },
      {
        key: 'edit',
        label: 'common.edit',
        icon: 'fe fe-edit',
        cssClass: 'btn-primary',
        show: (item: OrphanPaymentDto) => !item.isBatchUploaded
      },
      {
        key: 'add-orphans',
        label: 'orphanPayments.addOrphans',
        icon: 'fe fe-user-plus',
        cssClass: 'btn-success',
        show: (item: OrphanPaymentDto) => !item.isBatchUploaded
      },
      {
        key: 'mark-uploaded',
        label: 'orphanPayments.markAsUploaded',
        icon: 'fe fe-check-circle',
        cssClass: 'btn-warning',
        show: (item: OrphanPaymentDto) => !item.isBatchUploaded && item.orphanCount > 0
      },
      {
        key: 'unmark-uploaded',
        label: 'orphanPayments.unmarkAsUploaded',
        icon: 'fe fe-arrow-left',
        cssClass: 'btn-secondary',
        show: (item: OrphanPaymentDto) => item.isBatchUploaded
      },
      {
        key: 'delete',
        label: 'common.delete',
        icon: 'fe fe-trash',
        cssClass: 'btn-danger',
        show: (item: OrphanPaymentDto) => !item.isBatchUploaded
      }
    ];
  }

  private initializeFilters(): void {
    // Set default date range to current month
    const now = new Date();
    const firstDay = new Date(now.getFullYear(), now.getMonth(), 1);
    const lastDay = new Date(now.getFullYear(), now.getMonth() + 1, 0);

    this.filterForm.patchValue({
      paymentPeriodFrom: firstDay.toISOString().split('T')[0],
      paymentPeriodTo: lastDay.toISOString().split('T')[0]
    });
  }

  /** Review D1: charity dropdown options for HQ callers (same feed as the form's picker). */
  private loadCharityOptions(): void {
    this.charityService.getCharities({ pageNumber: 1, pageSize: 500 })
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: response => {
          this.charityOptions = (response.items || []).map(c => ({ id: c.id, name: c.name }));
        }
      });
  }

  loadPaymentGroups(): void {
    this.loading = true;
    this.loadError = false;
    const formValues = this.filterForm.value;

    const searchRequest: OrphanPaymentSearchRequest = {
      pageNumber: this.currentPage,
      pageSize: this.pageSize,
      sortBy: 'groupDate',
      sortDescending: true
    };

    // Only add optional filters if they have values
    if (formValues.searchValue && formValues.searchValue.trim()) {
      searchRequest.searchTerm = formValues.searchValue.trim();
    }
    if (formValues.paymentPeriodFrom) {
      searchRequest.paymentPeriodFrom = formValues.paymentPeriodFrom;
    }
    if (formValues.paymentPeriodTo) {
      searchRequest.paymentPeriodTo = formValues.paymentPeriodTo;
    }
    if (formValues.isBatchUploaded !== null && formValues.isBatchUploaded !== undefined) {
      searchRequest.isBatchUploaded = formValues.isBatchUploaded;
    }
    if (this.isHqUser && formValues.charityId) {
      searchRequest.charityId = formValues.charityId;
    }

    this.orphanPaymentService.getOrphanPayments(searchRequest)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (result) => {
          this.paymentGroups = result.items;
          this.totalRecords = result.totalCount;
          this.loading = false;
        },
        error: () => {
          // Review P3b: surface the failure — a 403/500 must read as an error row,
          // not a quietly empty grid.
          this.paymentGroups = [];
          this.totalRecords = 0;
          this.loading = false;
          this.loadError = true;
        }
      });
  }

  // ==================== SEARCH & FILTER ====================

  onSearch(): void {
    this.currentPage = 1;
    this.loadPaymentGroups();
  }

  /**
   * Export the §15.S.1 list to Excel (charity-list pattern) — the current filters
   * ride along; the server ignores paging and writes every matching row.
   */
  exportToExcel(): void {
    this.loading = true;
    const formValues = this.filterForm.value;

    const searchRequest: OrphanPaymentSearchRequest = {
      pageNumber: 1,
      pageSize: 100000,
      sortBy: 'groupDate',
      sortDescending: true
    };

    if (formValues.searchValue && formValues.searchValue.trim()) {
      searchRequest.searchTerm = formValues.searchValue.trim();
    }
    if (formValues.paymentPeriodFrom) {
      searchRequest.paymentPeriodFrom = formValues.paymentPeriodFrom;
    }
    if (formValues.paymentPeriodTo) {
      searchRequest.paymentPeriodTo = formValues.paymentPeriodTo;
    }
    if (formValues.isBatchUploaded !== null && formValues.isBatchUploaded !== undefined) {
      searchRequest.isBatchUploaded = formValues.isBatchUploaded;
    }
    if (this.isHqUser && formValues.charityId) {
      searchRequest.charityId = formValues.charityId;
    }

    this.orphanPaymentService.exportToExcel(searchRequest)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (blob: Blob) => {
          const url = window.URL.createObjectURL(blob);
          const a = document.createElement('a');
          a.href = url;
          a.download = `orphan-payments_${new Date().toISOString().split('T')[0]}.xlsx`;
          document.body.appendChild(a);
          a.click();
          document.body.removeChild(a);
          window.URL.revokeObjectURL(url);

          this.notificationService.success(this.translate.instant('common.operationSuccess'));
          this.loading = false;
        },
        error: () => {
          this.loading = false;
          this.notificationService.error(this.translate.instant('common.operationFailed'));
        }
      });
  }

  /**
   * app-drop-down round-trips option ids through the DOM (select2 hands back
   * strings), so the boolean options come back as 'true'/'false' and null as
   * an empty id — restore the exact null/true/false sentinel semantics that
   * loadPaymentGroups() reads from the control.
   */
  onUploadStatusChange(event: { id?: string | boolean | null } | null): void {
    const id = event?.id;
    const value = id === true || id === 'true'
      ? true
      : id === false || id === 'false'
        ? false
        : null;
    this.filterForm.get('isBatchUploaded')?.setValue(value);
  }

  clearFilters(): void {
    this.filterForm.reset({
      searchValue: '',
      paymentPeriodFrom: '',
      paymentPeriodTo: '',
      isBatchUploaded: null,
      charityId: null
    });
    this.initializeFilters();
    this.currentPage = 1;
    this.loadPaymentGroups();
  }

  hasActiveFilters(): boolean {
    const formValues = this.filterForm.value;
    return !!(
      formValues.searchValue ||
      formValues.paymentPeriodFrom ||
      formValues.paymentPeriodTo ||
      formValues.isBatchUploaded !== null && formValues.isBatchUploaded !== undefined ||
      formValues.charityId
    );
  }

  // ==================== PAGINATION ====================

  onPageChange(page: number): void {
    this.currentPage = page;
    this.loadPaymentGroups();
  }

  // ==================== ACTIONS ====================

  createPaymentGroup(): void {
    this.router.navigate(['/orphan-payments/create']);
  }

  onAction(event: { item: OrphanPaymentDto | null; action: string }): void {
    switch (event.action) {
      case 'view':
        if (event.item) this.router.navigate(['/orphan-payments', event.item.id]);
        break;
      case 'edit':
        if (event.item) this.router.navigate(['/orphan-payments', event.item.id, 'edit']);
        break;
      case 'add-orphans':
        if (event.item) this.router.navigate(['/orphan-payments', event.item.id, 'add-orphans']);
        break;
      case 'mark-uploaded':
        if (event.item) this.markAsUploaded(event.item);
        break;
      case 'unmark-uploaded':
        if (event.item) this.unmarkAsUploaded(event.item);
        break;
      case 'delete':
        if (event.item) {
          this.confirmDelete(event.item);
        }
        break;
    }
  }

  // ==================== STATUS MANAGEMENT ====================

  markAsUploaded(group: OrphanPaymentDto): void {
    if (confirm(this.translate.instant('orphanPayments.markAsUploadedConfirm'))) {
      this.orphanPaymentService.markAsUploaded(group.id)
        .pipe(takeUntil(this.destroy$))
        .subscribe({
          next: () => {
            this.loadPaymentGroups();
          }
        });
    }
  }

  unmarkAsUploaded(group: OrphanPaymentDto): void {
    if (confirm(this.translate.instant('orphanPayments.unmarkAsUploadedConfirm'))) {
      this.orphanPaymentService.unmarkAsUploaded(group.id)
        .pipe(takeUntil(this.destroy$))
        .subscribe({
          next: () => {
            this.loadPaymentGroups();
          }
        });
    }
  }

  // ==================== DELETE ====================

  /**
   * 10-5: SweetAlert2 confirm (AC 2 — a declined dialog sends no request at all),
   * then DELETE → drop row. Business refusals (disbursed rows / referenced by
   * reports) surface as a localised error toast.
   */
  async confirmDelete(group: OrphanPaymentDto): Promise<void> {
    const confirmed = await this.notificationService.confirm(
      this.translate.instant('orphanPayments.deleteGroupConfirm'),
      this.translate.instant('orphanPayments.deleteGroupTitle')
    );
    if (!confirmed) return;
    this.deleteGroup(group);
  }

  deleteGroup(group: OrphanPaymentDto): void {
    this.orphanPaymentService.deleteOrphanPayment(group.id)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
      next: () => {
        this.notificationService.success(this.translate.instant('orphanPayments.deleteGroupSuccess'));
        this.loadPaymentGroups();
      },
      error: (err) => {
        this.notificationService.error(this.resolveDeleteError(err?.message));
      }
    });
  }

  /** Known server refusal messages map to i18n; anything else shows verbatim. */
  private resolveDeleteError(message: string | null | undefined): string {
    switch (message) {
      case 'Cannot delete a payment batch with disbursed rows (received, transferred or cheque-issued)':
        return this.translate.instant('orphanPayments.deleteDisbursedError');
      case 'Cannot delete a payment batch referenced by periodic orphan reports':
        return this.translate.instant('orphanPayments.deleteReferencedError');
      default:
        return message || this.translate.instant('common.operationFailed');
    }
  }

  // ==================== HELPERS ====================

  // Widened beyond OrphanPaymentDto: the template also tracks the charity
  // filter options ({id, name}) with this same helper.
  trackById(_index: number, item: { id: string }): string {
    return item.id;
  }

  getStatusBadgeClass(isUploaded: boolean): string {
    return this.orphanPaymentService.getStatusBadgeClass(isUploaded);
  }

  formatDate(date: string): string {
    return this.orphanPaymentService.formatDate(date);
  }
}
