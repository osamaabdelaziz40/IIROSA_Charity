import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { FormBuilder, FormGroup, ReactiveFormsModule } from '@angular/forms';
import { FamilyDto, FamilySearchRequest, FamilyStatistics } from '../models/family.model';
import { FamilyService } from '../services/family.service';
import { CharityService } from '../../charities/services/charity.service';
import { AuthService } from '../../../core/services/auth.service';
import { NotificationService } from '../../../core/services/notification.service';
import { PageHeaderComponent } from '../../../shared/components/page-header/page-header.component';
import { BreadcrumbComponent, BreadcrumbItem } from '../../../shared/components';
import { TranslateModule, TranslateService, LangChangeEvent } from '@ngx-translate/core';
import { RouterModule } from '@angular/router';
import { PaginationComponent } from '../../../shared/components';
import { SharedModule } from '../../../shared/shared.module';
import { DropDownComponent } from '../../../shared/components/drop-down/drop-down.component';
import { Subject, Subscription } from 'rxjs';
import { takeUntil } from 'rxjs/operators';

@Component({
  selector: 'app-family-list',
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
  templateUrl: './family-list.component.html',
  styleUrls: ['./family-list.component.scss']
})
export class FamilyListComponent implements OnInit, OnDestroy {
  private destroy$ = new Subject<void>();
  private langChangeSubscription?: Subscription;

  Math = Math;
  families: FamilyDto[] = [];
  loading = false;
  totalCount = 0;
  currentPage = 1;
  pageSize = 20;
  totalPages = 0;

  /** Statistics band — describes the caller's whole register, not the current filter. */
  statistics: FamilyStatistics | null = null;

  filterForm: FormGroup;

  // Dropdown options - localized
  providerTypeOptions: Array<{ id: string; name: string }> = [];
  livingConditionOptions: Array<{ id: string; name: string }> = [];
  housingTypeOptions: Array<{ id: string; name: string }> = [];
  statusOptions: Array<{ id: string; name: string }> = [];

  // Charity transfer (UC-FAM-06 نقل الأسرة لجمعية أخرى) — HQ-only action
  canTransfer = false;
  showTransferModal = false;
  transferring = false;
  /** Transfer-modal charity dropdown fetch state (first open). */
  charitiesLoading = false;
  transferTarget: FamilyDto | null = null;
  charityOptions: Array<{ id: string; name: string }> = [];
  private allCharityOptions: Array<{ id: string; name: string }> = [];
  transferForm: FormGroup;

  pageActions = [
    {
      label: 'families.addFamily',
      type: 'primary',
      icon: 'fe-plus',
      click: () => this.createFamily()
    },
    {
      label: 'common.exportToExcel',
      type: 'success',
      icon: 'fe-file-plus',
      click: () => this.exportToExcel()
    }
  ];

  breadcrumbs: BreadcrumbItem[] = [
    { label: 'common.home', url: '/dashboard' },
    { label: 'families.title' }
  ];

  constructor(
    private fb: FormBuilder,
    private familyService: FamilyService,
    private charityService: CharityService,
    private auth: AuthService,
    private notification: NotificationService,
    private router: Router,
    private translate: TranslateService
  ) {
    this.filterForm = this.fb.group({
      searchValue: [''],
      providerType: ['all'],
      livingCondition: ['all'],
      housingType: ['all'],
      status: ['all'],
      orphanCountMin: [null],
      orphanCountMax: [null]
    });

    this.transferForm = this.fb.group({
      newCharityId: [''],
      reason: ['']
    });
  }

  private getTranslation(key: string, params?: any): string {
    return this.translate.instant(key, params);
  }

  ngOnInit(): void {
    this.canTransfer = this.auth.hasPermission('Families.Transfer');

    this.initializeDropdownOptions();

    this.langChangeSubscription = this.translate.onLangChange.subscribe((event: LangChangeEvent) => {
      this.initializeDropdownOptions();
    });

    this.loadFamilies();
    this.loadStatistics();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
    if (this.langChangeSubscription) {
      this.langChangeSubscription.unsubscribe();
    }
  }

  private initializeDropdownOptions(): void {
    this.providerTypeOptions = [
      { id: 'all', name: this.translate.instant('common.all') },
      { id: 'father', name: this.translate.instant('families.providerTypeFather') },
      { id: 'mother', name: this.translate.instant('families.providerTypeMother') },
      { id: 'other', name: this.translate.instant('families.providerTypeOther') }
    ];

    this.livingConditionOptions = [
      { id: 'all', name: this.translate.instant('common.all') },
      { id: 'good', name: this.translate.instant('families.livingConditionGood') },
      { id: 'fair', name: this.translate.instant('families.livingConditionFair') },
      { id: 'poor', name: this.translate.instant('families.livingConditionPoor') }
    ];

    this.housingTypeOptions = [
      { id: 'all', name: this.translate.instant('common.all') },
      { id: 'owned', name: this.translate.instant('families.housingTypeOwned') },
      { id: 'rented', name: this.translate.instant('families.housingTypeRented') },
      { id: 'shared', name: this.translate.instant('families.housingTypeShared') },
      { id: 'other', name: this.translate.instant('families.housingTypeOther') }
    ];

    this.statusOptions = [
      { id: 'all', name: this.translate.instant('common.all') },
      { id: 'active', name: this.translate.instant('common.active') },
      { id: 'inactive', name: this.translate.instant('common.inactive') }
    ];
  }

  loadFamilies(): void {
    this.loading = true;
    const formValues = this.filterForm.value;

    const searchRequest: FamilySearchRequest = {
      pageNumber: this.currentPage,
      pageSize: this.pageSize
    };

    if (formValues.searchValue && formValues.searchValue.trim()) {
      searchRequest.searchTerm = formValues.searchValue.trim();
    }
    if (formValues.providerType && formValues.providerType !== 'all') {
      searchRequest.providerType = formValues.providerType;
    }
    if (formValues.livingCondition && formValues.livingCondition !== 'all') {
      searchRequest.livingCondition = formValues.livingCondition;
    }
    if (formValues.housingType && formValues.housingType !== 'all') {
      searchRequest.housingType = formValues.housingType;
    }
    if (formValues.orphanCountMin) {
      searchRequest.orphanCountMin = formValues.orphanCountMin;
    }
    if (formValues.orphanCountMax) {
      searchRequest.orphanCountMax = formValues.orphanCountMax;
    }

    const status = formValues.status;
    if (status === 'active') {
      searchRequest.isActive = true;
    } else if (status === 'inactive') {
      searchRequest.isActive = false;
    }

    this.familyService.getFamilies(searchRequest).subscribe({
      next: (response) => {
        this.families = response.items || [];
        this.totalCount = response.totalCount || 0;
        this.totalPages = Math.ceil(this.totalCount / this.pageSize);
        this.loading = false;
      },
      error: (error: any) => {
        console.error('Error loading families:', error);
        this.notification.error(`Failed to load families: ${error.message || 'Unknown error'}`);
        this.loading = false;
      }
    });
  }

  /** Statistics band — silent fail: the list stays fully usable without it. */
  loadStatistics(): void {
    this.familyService.getStatistics()
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (statistics) => {
          this.statistics = statistics;
        },
        error: (error: any) => {
          console.error('Error loading family statistics:', error);
        }
      });
  }

  onSearch(): void {
    this.currentPage = 1;
    this.loadFamilies();
  }

  onProviderTypeChange(): void {
    this.onSearch();
  }

  onLivingConditionChange(): void {
    this.onSearch();
  }

  onHousingTypeChange(): void {
    this.onSearch();
  }

  onStatusChange(): void {
    this.onSearch();
  }

  clearFilters(): void {
    this.filterForm.reset({
      searchValue: '',
      providerType: 'all',
      livingCondition: 'all',
      housingType: 'all',
      status: 'all',
      orphanCountMin: null,
      orphanCountMax: null
    });
    this.currentPage = 1;
    this.loadFamilies();
  }

  hasActiveFilters(): boolean {
    const formValues = this.filterForm.value;
    return !!(
      formValues.searchValue ||
      formValues.providerType !== 'all' ||
      formValues.livingCondition !== 'all' ||
      formValues.housingType !== 'all' ||
      formValues.status !== 'all' ||
      formValues.orphanCountMin ||
      formValues.orphanCountMax
    );
  }

  createFamily(): void {
    this.router.navigate(['/families/create']);
  }

  viewFamily(id: string): void {
    this.router.navigate(['/families', id]);
  }

  editFamily(id: string): void {
    this.router.navigate(['/families', id, 'edit']);
  }

  async toggleFamilyStatus(family: FamilyDto): Promise<void> {
    const action = family.isActive ? 'deactivate' : 'activate';
    const confirmed = await this.notification.confirm(
      this.getTranslation(`families.confirm${action.charAt(0).toUpperCase() + action.slice(1)}`)
    );

    if (confirmed) {
      const action$ = family.isActive
        ? this.familyService.deactivateFamily(family.id)
        : this.familyService.activateFamily(family.id);

      action$.subscribe({
        next: () => {
          this.notification.success(this.getTranslation(`families.${action}Success`));
          this.loadFamilies();
        },
        error: (error: any) => {
          console.error(`Error ${action}ing family:`, error);
          this.notification.error(this.getTranslation(`families.${action}Failed`));
        }
      });
    }
  }

  async deleteFamily(family: FamilyDto): Promise<void> {
    const confirmed = await this.notification.confirm(
      this.getTranslation('families.confirmDelete', { code: family.code })
    );

    if (confirmed) {
      this.familyService.deleteFamily(family.id).subscribe({
        next: () => {
          this.notification.success(this.getTranslation('families.deleteSuccess'));
          this.loadFamilies();
        },
        error: (error: any) => {
          console.error('Error deleting family:', error);
          this.notification.error(this.getTranslation('families.deleteFailed'));
        }
      });
    }
  }

  // ==================== CHARITY TRANSFER (UC-FAM-06) ====================

  openTransferModal(family: FamilyDto): void {
    this.transferTarget = family;
    this.transferForm.reset({ newCharityId: '', reason: '' });
    this.showTransferModal = true;

    // The receiving charity must differ from the current one — the server refuses it anyway,
    // hiding it here spares the round trip. Only ACTIVE charities: the server refuses a
    // transfer into a locked/inactive one, so offering it in the dropdown only wastes the
    // operator's confirmation.
    if (this.allCharityOptions.length === 0) {
      this.charitiesLoading = true;
      this.charityService.getCharities({ pageNumber: 1, pageSize: 500, isActive: true }).subscribe({
        next: (response) => {
          this.charitiesLoading = false;
          this.allCharityOptions = (response.items || [])
            .filter(c => c.isActive)
            .map(c => ({ id: c.id, name: c.name }));
          this.charityOptions = this.allCharityOptions.filter(c => c.id !== family.charityId);
        },
        error: (error: any) => {
          this.charitiesLoading = false;
          console.error('Error loading charities:', error);
          this.notification.error(this.getTranslation('families.transferLoadCharitiesFailed'));
        }
      });
    } else {
      this.charityOptions = this.allCharityOptions.filter(c => c.id !== family.charityId);
    }
  }

  closeTransferModal(): void {
    this.showTransferModal = false;
    this.transferTarget = null;
  }

  async submitTransfer(): Promise<void> {
    const target = this.transferTarget;
    if (!target || this.transferring) {
      return;
    }

    const newCharityId = this.transferForm.value.newCharityId;
    if (!newCharityId) {
      this.notification.error(this.getTranslation('families.transferCharityRequired'));
      return;
    }

    // Set BEFORE the awaited confirm: the guard at the top must hold across the SweetAlert2
    // promise too, or a double-click opens two confirms and fires two PUTs. `target` is
    // captured because closing the modal nulls transferTarget while the dialog is open.
    this.transferring = true;
    const charityName = this.charityOptions.find(c => c.id === newCharityId)?.name || '';
    const confirmed = await this.notification.confirm(
      this.getTranslation('families.transferConfirm', {
        code: target.code || '-',
        charity: charityName
      })
    );
    if (!confirmed) {
      this.transferring = false;
      return;
    }

    this.familyService.transferFamily(target.id, {
      newCharityId,
      reason: this.transferForm.value.reason?.trim() || undefined
    }).subscribe({
      next: () => {
        this.transferring = false;
        this.closeTransferModal();
        this.notification.success(this.getTranslation('families.transferSuccess'));
        this.loadFamilies();
      },
      error: (error: any) => {
        this.transferring = false;
        console.error('Error transferring family:', error);
        this.notification.error(error?.error?.message || error?.message || this.getTranslation('families.transferFailed'));
      }
    });
  }

  trackByCharityId(index: number, item: { id: string; name: string }): string {
    return item.id;
  }

  exportToExcel(): void {
    this.loading = true;
    const formValues = this.filterForm.value;

    const searchRequest: FamilySearchRequest = {
      pageNumber: 1,
      pageSize: 10000
    };

    if (formValues.searchValue && formValues.searchValue.trim()) {
      searchRequest.searchTerm = formValues.searchValue.trim();
    }
    if (formValues.providerType && formValues.providerType !== 'all') {
      searchRequest.providerType = formValues.providerType;
    }
    if (formValues.livingCondition && formValues.livingCondition !== 'all') {
      searchRequest.livingCondition = formValues.livingCondition;
    }
    if (formValues.housingType && formValues.housingType !== 'all') {
      searchRequest.housingType = formValues.housingType;
    }

    const status = formValues.status;
    if (status === 'active') {
      searchRequest.isActive = true;
    } else if (status === 'inactive') {
      searchRequest.isActive = false;
    }

    this.familyService.exportFamiliesToExcel(searchRequest).subscribe({
      next: (blob: Blob) => {
        const url = window.URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = `families_${new Date().toISOString().split('T')[0]}.xlsx`;
        document.body.appendChild(a);
        a.click();
        document.body.removeChild(a);
        window.URL.revokeObjectURL(url);

        this.notification.success('Families exported successfully');
        this.loading = false;
      },
      error: (error: any) => {
        console.error('Export failed:', error);
        this.notification.error('Failed to export families');
        this.loading = false;
      }
    });
  }

  onPageChange(page: number): void {
    this.currentPage = page;
    this.loadFamilies();
  }

  getStatusBadgeClass(family: FamilyDto): string {
    if (!family.isActive) return 'badge-warning';
    return 'badge-success';
  }

  getProviderTypeBadgeClass(providerType?: string): string {
    switch (providerType) {
      case 'father': return 'badge-primary';
      case 'mother': return 'badge-info';
      case 'other': return 'badge-secondary';
      default: return 'badge-light';
    }
  }

  getProviderTypeText(providerType?: string): string {
    switch (providerType) {
      case 'father': return this.getTranslation('families.providerTypeFather');
      case 'mother': return this.getTranslation('families.providerTypeMother');
      case 'other': return this.getTranslation('families.providerTypeOther');
      default: return '-';
    }
  }

  getLivingConditionText(livingCondition?: string): string {
    if (!livingCondition) return '-';
    switch (livingCondition) {
      case 'good': return this.getTranslation('families.livingConditionGood');
      case 'fair': return this.getTranslation('families.livingConditionFair');
      case 'poor': return this.getTranslation('families.livingConditionPoor');
      default: return livingCondition;
    }
  }
}
