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
import { PageHeaderComponent } from '../../../shared/components/page-header/page-header.component';
import { BreadcrumbComponent, BreadcrumbItem } from '../../../shared/components';
import { PaginationComponent } from '../../../shared/components';
import { SharedModule } from '../../../shared/shared.module';

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
    SharedModule
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

  // Pagination
  currentPage = 1;
  pageSize = 10;

  // Row actions
  rowActions: ListAction[] = [];

  // Filter form
  filterForm: FormGroup;

  // Upload status options for dropdown
  uploadStatusOptions: Array<{ id: string | boolean | null; name: string }> = [];

  // UI State
  expandedFilters = false;

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
      icon: 'fe-file-plus',
      click: () => this.exportList()
    },
    {
      label: 'orphanPayments.generateBatchNumber',
      type: 'info',
      icon: 'fe-hash',
      click: () => this.generateBatchNumber()
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
    private router: Router,
    private route: ActivatedRoute,
    private translate: TranslateService
  ) {
    // Initialize filter form
    this.filterForm = this.fb.group({
      searchValue: [''],
      paymentPeriodFrom: [''],
      paymentPeriodTo: [''],
      isBatchUploaded: [null]
    });
  }

  ngOnInit(): void {
    this.initializeUploadStatusOptions();
    this.initializeRowActions();
    this.initializeFilters();

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
        key: 'export',
        label: 'orphanPayments.exportGroup',
        icon: 'fe fe-download',
        cssClass: 'btn-info',
        show: () => true
      },
      {
        key: 'print',
        label: 'common.print',
        icon: 'fe fe-printer',
        cssClass: 'btn-secondary',
        show: () => true
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

  loadPaymentGroups(): void {
    this.loading = true;
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

    this.orphanPaymentService.getOrphanPayments(searchRequest).subscribe({
      next: (result) => {
        this.paymentGroups = result.items;
        this.totalRecords = result.totalCount;
        this.loading = false;
      },
      error: () => {
        this.loading = false;
      }
    });
  }

  // ==================== SEARCH & FILTER ====================

  onSearch(): void {
    this.currentPage = 1;
    this.loadPaymentGroups();
  }

  clearFilters(): void {
    this.filterForm.reset({
      searchValue: '',
      paymentPeriodFrom: '',
      paymentPeriodTo: '',
      isBatchUploaded: null
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
      formValues.isBatchUploaded !== null && formValues.isBatchUploaded !== undefined
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
      case 'export':
        if (event.item) this.exportGroup(event.item);
        break;
      case 'print':
        if (event.item) this.printGroup(event.item);
        break;
      case 'mark-uploaded':
        if (event.item) this.markAsUploaded(event.item);
        break;
      case 'unmark-uploaded':
        if (event.item) this.unmarkAsUploaded(event.item);
        break;
      case 'delete':
        if (event.item) {
          // Show confirmation dialog (implement with modal service)
          if (confirm(this.translate.instant('orphanPayments.deleteGroupConfirm'))) {
            this.deleteGroup(event.item);
          }
        }
        break;
    }
  }

  // ==================== EXPORT ====================

  exportGroup(group: OrphanPaymentDto): void {
    // For now, default to Excel with no photos, no grouping
    // In a real app, show a modal to select options
    this.orphanPaymentService.exportGroup(group.id, {
      format: 'Excel',
      includePhotos: false,
      groupBy: 'None'
    }).subscribe(blob => {
      this.orphanPaymentService.downloadFile(
        blob,
        `PaymentGroup_${group.batchNo || group.id}.xlsx`
      );
    });
  }

  exportList(): void {
    const formValues = this.filterForm.value;
    const searchRequest: OrphanPaymentSearchRequest = {
      pageNumber: 1,
      pageSize: 10000,
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

    this.orphanPaymentService.exportGroupsList(searchRequest).subscribe(blob => {
      const date = new Date().toISOString().split('T')[0];
      this.orphanPaymentService.downloadFile(
        blob,
        `PaymentGroups_${date}.xlsx`
      );
    });
  }

  // ==================== PRINT ====================

  printGroup(group: OrphanPaymentDto): void {
    this.orphanPaymentService.printGroup(group.id).subscribe(html => {
      const printWindow = window.open('', '_blank');
      if (printWindow) {
        printWindow.document.write(html);
        printWindow.document.close();
        printWindow.print();
      }
    });
  }

  // ==================== STATUS MANAGEMENT ====================

  markAsUploaded(group: OrphanPaymentDto): void {
    if (confirm(this.translate.instant('orphanPayments.markAsUploadedConfirm'))) {
      this.orphanPaymentService.markAsUploaded(group.id).subscribe({
        next: () => {
          this.loadPaymentGroups();
        }
      });
    }
  }

  unmarkAsUploaded(group: OrphanPaymentDto): void {
    if (confirm(this.translate.instant('orphanPayments.unmarkAsUploadedConfirm'))) {
      this.orphanPaymentService.unmarkAsUploaded(group.id).subscribe({
        next: () => {
          this.loadPaymentGroups();
        }
      });
    }
  }

  // ==================== DELETE ====================

  deleteGroup(group: OrphanPaymentDto): void {
    this.orphanPaymentService.deleteOrphanPayment(group.id).subscribe({
      next: () => {
        this.loadPaymentGroups();
      }
    });
  }

  // ==================== BATCH NUMBER ====================

  generateBatchNumber(): void {
    this.orphanPaymentService.generateNextBatchNumber().subscribe(result => {
      // Show result or navigate to create with pre-filled batch number
      alert(`Next batch number: ${result.nextBatchNumber}`);
    });
  }

  // ==================== HELPERS ====================

  getStatusBadgeClass(isUploaded: boolean): string {
    return this.orphanPaymentService.getStatusBadgeClass(isUploaded);
  }

  getStatusText(isUploaded: boolean): string {
    return this.orphanPaymentService.getStatusText(isUploaded);
  }

  formatDate(date: string): string {
    return this.orphanPaymentService.formatDate(date);
  }
}
