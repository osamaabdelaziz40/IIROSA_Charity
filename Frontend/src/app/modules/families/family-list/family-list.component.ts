import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { FormBuilder, FormGroup, ReactiveFormsModule } from '@angular/forms';
import { FamilyDto, FamilySearchRequest } from '../models/family.model';
import { FamilyService } from '../services/family.service';
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

  filterForm: FormGroup;

  // Dropdown options - localized
  providerTypeOptions: Array<{ id: string; name: string }> = [];
  livingConditionOptions: Array<{ id: string; name: string }> = [];
  housingTypeOptions: Array<{ id: string; name: string }> = [];
  statusOptions: Array<{ id: string; name: string }> = [];

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
  }

  private getTranslation(key: string, params?: any): string {
    return this.translate.instant(key, params);
  }

  ngOnInit(): void {
    this.initializeDropdownOptions();

    this.langChangeSubscription = this.translate.onLangChange.subscribe((event: LangChangeEvent) => {
      this.initializeDropdownOptions();
    });

    this.loadFamilies();
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
