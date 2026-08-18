import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup } from '@angular/forms';
import { Router } from '@angular/router';
import { TranslateService, TranslateModule } from '@ngx-translate/core';
import { Subject, takeUntil } from 'rxjs';
import {
  CheckListItem,
  CheckFilter,
  CheckStatus,
  Currency
} from '../models/check.model';
import { GeneralChecksService } from '../services/general-checks.service';
import { LookupManagementService } from '../../lookup-management/services/lookup-management.service';
import { NotificationService } from '../../../core/services/notification.service';
import { PageHeaderComponent } from '../../../shared/components/page-header/page-header.component';
import { RouterModule } from '@angular/router';
import { PaginationComponent, BreadcrumbComponent, BreadcrumbItem, DropDownComponent } from '../../../shared/components';
import { SharedModule } from '../../../shared/shared.module';
import { BankDto } from '../../lookup-management/models/lookup.model';

@Component({
  selector: 'app-check-list',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    PageHeaderComponent,
    TranslateModule,
    RouterModule,
    PaginationComponent,
    BreadcrumbComponent,
    SharedModule,
    DropDownComponent
  ],
  templateUrl: './check-list.component.html',
  styleUrls: ['./check-list.component.scss']
})
export class CheckListComponent implements OnInit, OnDestroy {
  private destroy$ = new Subject<void>();

  // Breadcrumb items
  breadcrumbs: BreadcrumbItem[] = [
    { label: 'common.home', url: '/dashboard' },
    { label: 'generalChecks.title' }
  ];

  // Expose Math to template for pagination calculations
  Math = Math;

  checks: CheckListItem[] = [];
  allBanks: BankDto[] = [];

  loading = false;
  loadingBanks = false;

  totalCount = 0;
  currentPage = 1;
  pageSize = 20;
  totalPages = 0;

  // Filter form with shared components
  filterForm!: FormGroup;

  // Status options for dropdown
  statusOptions: Array<{ id: string; name: string }> = [];

  // Currency options for dropdown
  currencyOptions: Array<{ id: string; name: string }> = [];

  pageActions = [
    {
      label: 'generalChecks.addCheck',
      type: 'primary',
      icon: 'fe-plus',
      click: () => this.createCheck()
    },
    {
      label: 'generalChecks.reconcileChecks',
      type: 'info',
      icon: 'fe-refresh-cw',
      click: () => this.reconcileChecks()
    },
    {
      label: 'generalChecks.generateReport',
      type: 'success',
      icon: 'fe-file-text',
      click: () => this.generateReport()
    },
    {
      label: 'common.exportToExcel',
      type: 'success',
      icon: 'fe-file-plus',
      click: () => this.exportToExcel()
    }
  ];

  constructor(
    private fb: FormBuilder,
    private checksService: GeneralChecksService,
    private lookupService: LookupManagementService,
    private notification: NotificationService,
    private translate: TranslateService,
    private router: Router
  ) {
    this.initFilterForm();
  }

  ngOnInit(): void {
    this.initializeDropdownOptions();
    this.loadBanks();
    this.loadChecks();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  /**
   * Initialize filter form with FormBuilder
   */
  private initFilterForm(): void {
    this.filterForm = this.fb.group({
      searchValue: [''],
      checkStatus: [null],
      bankId: [null],
      currency: [null],
      dateFrom: [null],
      dateTo: [null],
      amountFrom: [null],
      amountTo: [null]
    });
  }

  /**
   * Initialize dropdown options with translated labels
   */
  private initializeDropdownOptions(): void {
    // Status options
    this.statusOptions = Object.values(CheckStatus).map(status => ({
      id: status,
      name: this.getStatusLabel(status)
    }));

    // Currency options
    this.currencyOptions = Object.values(Currency).map(currency => ({
      id: currency,
      name: `generalChecks.currencies.${currency}`
    }));
  }

  loadBanks(): void {
    this.loadingBanks = true;
    this.lookupService.getBanks({ isActive: true, page: 1, pageSize: 1000 }).pipe(takeUntil(this.destroy$)).subscribe({
      next: (response) => {
        this.allBanks = (response?.items || []) as BankDto[];
        this.loadingBanks = false;
      },
      error: () => {
        this.loadingBanks = false;
      }
    });
  }

  loadChecks(): void {
    this.loading = true;
    const formValues = this.filterForm.value;

    const filter: CheckFilter = {
      searchTerm: formValues.searchValue || undefined,
      checkStatus: formValues.checkStatus || undefined,
      bankId: formValues.bankId || undefined,
      currency: formValues.currency || undefined,
      dateFrom: formValues.dateFrom || undefined,
      dateTo: formValues.dateTo || undefined,
      amountFrom: formValues.amountFrom || undefined,
      amountTo: formValues.amountTo || undefined
    };

    this.checksService.getAllChecks(filter).pipe(takeUntil(this.destroy$)).subscribe({
      next: (data) => {
        this.checks = data || [];
        this.totalCount = this.checks.length;
        this.totalPages = Math.ceil(this.totalCount / this.pageSize);
        this.loading = false;
      },
      error: (error: any) => {
        console.error('Error loading checks:', error);
        this.notification.error(
          this.translate.instant('generalChecks.loadChecksFailed')
        );
        this.loading = false;
      }
    });
  }

  onSearch(): void {
    this.currentPage = 1;
    this.loadChecks();
  }

  // Status dropdown change handler
  onStatusDropDownChanged(value: any): void {
    const statusControl = this.filterForm.get('checkStatus');
    if (value && value.id !== undefined && value.id !== null) {
      statusControl?.setValue(value.id);
    } else {
      statusControl?.setValue(null);
    }
    this.onSearch();
  }

  // Bank dropdown change handler
  onBankDropDownChanged(value: any): void {
    const bankControl = this.filterForm.get('bankId');
    if (value && value.id !== undefined && value.id !== null) {
      bankControl?.setValue(value.id);
    } else {
      bankControl?.setValue(null);
    }
    this.onSearch();
  }

  // Currency dropdown change handler
  onCurrencyDropDownChanged(value: any): void {
    const currencyControl = this.filterForm.get('currency');
    if (value && value.id !== undefined && value.id !== null) {
      currencyControl?.setValue(value.id);
    } else {
      currencyControl?.setValue(null);
    }
    this.onSearch();
  }

  /**
   * Check if any filters are active
   */
  hasActiveFilters(): boolean {
    const formValues = this.filterForm.value;
    return !!(
      formValues.searchValue ||
      formValues.checkStatus ||
      formValues.bankId ||
      formValues.currency ||
      formValues.dateFrom ||
      formValues.dateTo ||
      formValues.amountFrom ||
      formValues.amountTo
    );
  }

  clearFilters(): void {
    this.filterForm.reset({
      searchValue: '',
      checkStatus: null,
      bankId: null,
      currency: null,
      dateFrom: null,
      dateTo: null,
      amountFrom: null,
      amountTo: null
    });
    this.currentPage = 1;
    this.loadChecks();
  }

  viewCheck(checkId: number): void {
    this.router.navigate(['/general-checks', checkId]);
  }

  editCheck(checkId: number): void {
    this.router.navigate(['/general-checks', checkId, 'edit']);
  }

  async deleteCheck(checkId: number): Promise<void> {
    const confirmed = await this.notification.confirm(
      this.translate.instant('generalChecks.confirmDeleteCheck')
    );

    if (confirmed) {
      this.loading = true;
      this.checksService.deleteCheck(checkId).pipe(takeUntil(this.destroy$)).subscribe({
        next: () => {
          this.notification.success(this.translate.instant('generalChecks.deleteCheckSuccess'));
          this.loadChecks();
        },
        error: (error: any) => {
          console.error('Error deleting check:', error);
          this.notification.error(
            this.translate.instant('generalChecks.deleteCheckFailed')
          );
          this.loading = false;
        }
      });
    }
  }

  createCheck(): void {
    this.router.navigate(['/general-checks', 'create']);
  }

  reconcileChecks(): void {
    this.router.navigate(['/general-checks', 'reconcile']);
  }

  generateReport(): void {
    this.router.navigate(['/general-checks', 'report']);
  }

  exportToExcel(): void {
    this.loading = true;
    const formValues = this.filterForm.value;

    const filter: CheckFilter = {
      searchTerm: formValues.searchValue || undefined,
      checkStatus: formValues.checkStatus || undefined,
      bankId: formValues.bankId || undefined,
      currency: formValues.currency || undefined,
      dateFrom: formValues.dateFrom || undefined,
      dateTo: formValues.dateTo || undefined,
      amountFrom: formValues.amountFrom || undefined,
      amountTo: formValues.amountTo || undefined
    };

    this.checksService.exportToExcel(filter).pipe(takeUntil(this.destroy$)).subscribe({
      next: (blob: Blob) => {
        const url = window.URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = `checks-${new Date().toISOString().split('T')[0]}.xlsx`;
        document.body.appendChild(a);
        a.click();
        document.body.removeChild(a);
        window.URL.revokeObjectURL(url);
        this.notification.success(this.translate.instant('generalChecks.exportSuccess'));
        this.loading = false;
      },
      error: (error: any) => {
        console.error('Error exporting checks:', error);
        this.notification.error(
          this.translate.instant('generalChecks.exportFailed')
        );
        this.loading = false;
      }
    });
  }

  onPageChange(page: number): void {
    this.currentPage = page;
    this.loadChecks();
  }

  /**
   * Get paginated checks for display
   */
  get paginatedChecks(): CheckListItem[] {
    const start = (this.currentPage - 1) * this.pageSize;
    const end = start + this.pageSize;
    return this.checks.slice(start, end);
  }

  /**
   * Get status label with translation
   */
  getStatusLabel(status: CheckStatus): string {
    return `generalChecks.checkStatuses.${status}`;
  }

  /**
   * Get status badge class
   */
  getStatusClass(status: CheckStatus): string {
    switch (status) {
      case CheckStatus.Pending:
        return 'badge-warning';
      case CheckStatus.Issued:
        return 'badge-info';
      case CheckStatus.Cleared:
        return 'badge-success';
      case CheckStatus.Void:
        return 'badge-danger';
      default:
        return 'badge-secondary';
    }
  }

  /**
   * Format date for display
   */
  formatDate(date: Date | string | undefined): string {
    if (!date) return '-';
    return new Date(date).toLocaleDateString();
  }

  /**
   * Format currency amount
   */
  formatCurrency(amount: number): string {
    return new Intl.NumberFormat('en-US', {
      minimumFractionDigits: 2,
      maximumFractionDigits: 2
    }).format(amount);
  }

  /**
   * Check if check can be edited
   */
  canEditCheck(status: CheckStatus): boolean {
    return status === CheckStatus.Pending;
  }

  /**
   * Check if check can be voided
   */
  canVoidCheck(status: CheckStatus): boolean {
    return status !== CheckStatus.Cleared && status !== CheckStatus.Void;
  }

  getCurrencySymbol(currency: Currency): string {
    return currency;
  }
}
