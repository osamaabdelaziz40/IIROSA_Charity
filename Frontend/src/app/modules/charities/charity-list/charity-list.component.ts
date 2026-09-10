import { Component, OnInit, OnDestroy, HostListener } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { FormBuilder, FormGroup, ReactiveFormsModule } from '@angular/forms';
import { CharityDto, CharitySearchRequest, CharityStatistics, CharityCountryStatistics } from '../models/charity.model';
import { CharityService } from '../services/charity.service';
import { LookupManagementService } from '../../lookup-management/services/lookup-management.service';
import { CountryDto, RegionDto, CenterDto } from '../../lookup-management/models/lookup.model';
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
  selector: 'app-charity-list',
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
  templateUrl: './charity-list.component.html',
  styleUrls: ['./charity-list.component.scss']
})
export class CharityListComponent implements OnInit, OnDestroy {
  private destroy$ = new Subject<void>();
  private langChangeSubscription?: Subscription;

  // Expose Math to template for pagination calculations
  Math = Math;
  charities: CharityDto[] = [];

  // Register statistics band (UC-CHR-01) — caller-scoped server-side, refreshed after
  // the mutations that move its numbers. Not filter-reactive by design.
  statistics: CharityStatistics | null = null;
  /** Current language for picking nameAr/nameEn in the by-country breakdown. */
  currentLang: string = 'ar';
  allCountries: CountryDto[] = [];
  allRegions: RegionDto[] = [];
  allCenters: CenterDto[] = [];
  loading = false;
  loadingCountries = false;
  loadingRegions = false;
  loadingCenters = false;
  totalCount = 0;
  currentPage = 1;
  pageSize = 20;
  totalPages = 0;

  // Filter form with shared components
  filterForm: FormGroup;

  // Status options for dropdown - localized
  statusOptions: Array<{ id: string; name: string }> = [];

  // Row actions menu: id of the charity whose menu is open (null = all closed).
  // Driven by Angular state instead of Bootstrap's global jQuery data-api — its
  // document-level delegated handler does not fire reliably for clicks inside
  // this table, so the table manages its own menus.
  openRowMenuId: string | null = null;

  toggleRowMenu(charity: CharityDto): void {
    this.openRowMenuId = this.openRowMenuId === charity.id ? null : (charity.id ?? null);
  }

  @HostListener('document:click', ['$event'])
  onDocumentClick(event: MouseEvent): void {
    const target = event.target as HTMLElement;
    // Close when clicking outside any dropdown, or on a menu item (after its
    // action handler has fired — document listeners run last in the bubble phase).
    if (!target.closest('.dropdown') || target.closest('.dropdown-item')) {
      this.openRowMenuId = null;
    }
  }

  pageActions = [
    {
      label: 'charities.addCharity',
      type: 'primary',
      icon: 'fe-plus',
      click: () => this.createCharity()
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
    { label: 'charities.title' }
  ];

  constructor(
    private fb: FormBuilder,
    private charityService: CharityService,
    private lookupService: LookupManagementService,
    private notification: NotificationService,
    private router: Router,
    private translate: TranslateService
  ) {
    // Initialize filter form
    this.filterForm = this.fb.group({
      searchValue: [''],
      country: [null],
      region: [null],
      center: [null],
      status: ['all']
    });
  }

  private getTranslation(key: string, params?: any): string {
    return this.translate.instant(key, params);
  }

  ngOnInit(): void {
    this.currentLang = this.translate.currentLang || 'ar';
    this.initializeStatusOptions();

    // Subscribe to language changes to update translated options
    this.langChangeSubscription = this.translate.onLangChange.subscribe((event: LangChangeEvent) => {
      this.currentLang = event.lang;
      this.initializeStatusOptions();
    });

    // Page actions are now directly bound
    this.loadCountries();
    this.loadCharities();
    this.loadStatistics();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
    if (this.langChangeSubscription) {
      this.langChangeSubscription.unsubscribe();
    }
  }

  private initializeStatusOptions(): void {
    this.statusOptions = [
      { id: 'all', name: this.translate.instant('common.all') },
      { id: 'active', name: this.translate.instant('common.active') },
      { id: 'inactive', name: this.translate.instant('common.inactive') },
      { id: 'locked', name: this.translate.instant('charities.locked') }
    ];
  }

  loadCountries(): void {
    this.loadingCountries = true;
    this.lookupService.getCountries({ isActive: true }).subscribe({
      next: (response) => {
        this.allCountries = response.items || [];
        this.loadingCountries = false;
      },
      error: () => {
        this.loadingCountries = false;
      }
    });
  }

  loadRegions(): void {
    this.loadingRegions = true;
    this.lookupService.getRegions({ isActive: true }).subscribe({
      next: (response) => {
        this.allRegions = response.items || [];
        this.loadingRegions = false;
      },
      error: () => {
        this.loadingRegions = false;
      }
    });
  }

  loadCenters(): void {
    this.loadingCenters = true;
    this.lookupService.getCenters({ isActive: true }).subscribe({
      next: (response) => {
        this.allCenters = response.items || [];
        this.loadingCenters = false;
      },
      error: () => {
        this.loadingCenters = false;
      }
    });
  }

  // Load regions by country for cascading dropdown
  loadRegionsByCountry(countryId: any): void {
    if (!countryId) {
      this.allRegions = [];
      return;
    }

    const cleanCountryId = typeof countryId === 'string' ? parseInt(countryId.trim(), 10) : countryId;
    this.loadingRegions = true;
    this.lookupService.getRegionsByCountry(cleanCountryId).subscribe({
      next: (regions) => {
        this.allRegions = regions || [];
        this.loadingRegions = false;
      },
      error: () => {
        this.allRegions = [];
        this.loadingRegions = false;
      }
    });
  }

  // Load centers by region for cascading dropdown
  loadCentersByRegion(regionId: any): void {
    if (!regionId) {
      this.allCenters = [];
      return;
    }

    const cleanRegionId = typeof regionId === 'string' ? parseInt(regionId.trim(), 10) : regionId;
    this.loadingCenters = true;
    this.lookupService.getCentersByRegion(cleanRegionId).subscribe({
      next: (centers) => {
        this.allCenters = centers || [];
        this.loadingCenters = false;
      },
      error: () => {
        this.allCenters = [];
        this.loadingCenters = false;
      }
    });
  }

  loadCharities(): void {
    this.loading = true;
    const formValues = this.filterForm.value;

    const searchRequest: CharitySearchRequest = {
      pageNumber: this.currentPage,
      pageSize: this.pageSize
    };

    // Only add optional filters if they have values
    if (formValues.searchValue && formValues.searchValue.trim()) {
      searchRequest.searchTerm = formValues.searchValue.trim();
    }
    if (formValues.country) {
      searchRequest.countryId = formValues.country;
    }
    if (formValues.region) {
      searchRequest.regionId = formValues.region;
    }
    if (formValues.center) {
      searchRequest.centerId = formValues.center;
    }

    // Handle status filter
    const status = formValues.status;
    if (status === 'active') {
      searchRequest.isActive = true;
      searchRequest.isLocked = false;
    } else if (status === 'inactive') {
      searchRequest.isActive = false;
    } else if (status === 'locked') {
      searchRequest.isLocked = true;
    }

    this.charityService.getCharities(searchRequest).subscribe({
      next: (response) => {
        this.charities = response.items || [];
        this.totalCount = response.totalCount || 0;
        this.totalPages = Math.ceil(this.totalCount / this.pageSize);
        this.loading = false;
      },
      error: (error: any) => {
        console.error('Error loading charities:', error);
        this.notification.error(`Failed to load charities: ${error.message || 'Unknown error'}`);
        this.loading = false;
      }
    });
  }

  onSearch(): void {
    this.currentPage = 1;
    this.loadCharities();
  }

  /**
   * Register statistics band — describes the caller's whole scope (not the active
   * filters), so it loads once and after mutations that move its numbers, never on
   * paging or search. Fails silently like the lookup loads: a missing band must not
   * block the grid.
   */
  loadStatistics(): void {
    this.charityService.getStatistics().subscribe({
      next: statistics => this.statistics = statistics,
      error: (error: any) => console.error('Error loading charity statistics:', error)
    });
  }

  /** Bilingual label for a by-country breakdown row. */
  countryName(row: { nameAr?: string | null; nameEn?: string | null }): string {
    const english = this.currentLang === 'en';
    return (english ? row.nameEn || row.nameAr : row.nameAr || row.nameEn) || '';
  }

  trackByCountry(index: number, row: CharityCountryStatistics): number {
    return row.countryId;
  }

  // Country dropdown change handler
  onCountryChange(event?: any): void {
    const countryId = event?.id || event || this.filterForm.get('country')?.value;
    this.filterForm.patchValue({ region: null, center: null });
    if (countryId) {
      this.loadRegionsByCountry(countryId);
    } else {
      this.allRegions = [];
      this.allCenters = [];
    }
    this.onSearch();
  }

  // Region dropdown change handler
  onRegionChange(event?: any): void {
    const regionId = event?.id || event || this.filterForm.get('region')?.value;
    this.filterForm.patchValue({ center: null });
    if (regionId) {
      this.loadCentersByRegion(regionId);
    } else {
      this.allCenters = [];
    }
    this.onSearch();
  }

  // Center dropdown change handler
  onCenterChange(event?: any): void {
    this.onSearch();
  }

  // Status dropdown change handler
  onStatusChange(event?: any): void {
    this.onSearch();
  }

  clearFilters(): void {
    this.filterForm.reset({
      searchValue: '',
      country: null,
      region: null,
      center: null,
      status: 'all'
    });
    this.allRegions = [];
    this.allCenters = [];
    this.currentPage = 1;
    this.loadCharities();
  }

  // Check if any filters are active
  hasActiveFilters(): boolean {
    const formValues = this.filterForm.value;
    return !!(
      formValues.searchValue ||
      formValues.country ||
      formValues.region ||
      formValues.center ||
      formValues.status !== 'all'
    );
  }

  createCharity(): void {
    this.router.navigate(['/charities/create']);
  }

  viewCharity(id: string): void {
    this.router.navigate(['/charities', id]);
  }

  editCharity(id: string): void {
    this.router.navigate(['/charities', id, 'edit']);
  }

  async toggleCharityStatus(charity: CharityDto): Promise<void> {
    const action = charity.isActive ? 'deactivate' : 'activate';
    const confirmed = await this.notification.confirm(
      this.getTranslation(`charities.confirm${action.charAt(0).toUpperCase() + action.slice(1)}`)
    );

    if (confirmed) {
      const action$ = charity.isActive
        ? this.charityService.deactivateCharity(charity.id)
        : this.charityService.activateCharity(charity.id);

      action$.subscribe({
        next: () => {
          this.notification.success(this.getTranslation(`charities.${action}Success`));
          this.loadCharities();
          this.loadStatistics();
        },
        error: (error: any) => {
          console.error(`Error ${action}ing charity:`, error);
          this.notification.error(this.getTranslation(`charities.${action}Failed`));
        }
      });
    }
  }

  async toggleCharityLock(charity: CharityDto): Promise<void> {
    const action = charity.isLocked ? 'unlock' : 'lock';
    const confirmed = await this.notification.confirm(
      this.getTranslation(`charities.confirm${action.charAt(0).toUpperCase() + action.slice(1)}`)
    );

    if (confirmed) {
      const action$ = charity.isLocked
        ? this.charityService.unlockCharity(charity.id)
        : this.charityService.lockCharity(charity.id);

      action$.subscribe({
        next: () => {
          this.notification.success(this.getTranslation(`charities.${action}Success`));
          this.loadCharities();
          this.loadStatistics();
        },
        error: (error: any) => {
          console.error(`Error ${action}ing charity:`, error);
          this.notification.error(this.getTranslation(`charities.${action}Failed`));
        }
      });
    }
  }

  async toggleAddRights(charity: CharityDto): Promise<void> {
    const action = charity.isAddEnabled ? 'disable' : 'enable';
    const confirmed = await this.notification.confirm(
      this.getTranslation(`charities.confirm${action.charAt(0).toUpperCase() + action.slice(1)}AddRights`)
    );

    if (confirmed) {
      const action$ = charity.isAddEnabled
        ? this.charityService.disableAddRights(charity.id)
        : this.charityService.enableAddRights(charity.id);

      action$.subscribe({
        next: () => {
          this.notification.success(this.getTranslation(`charities.addRights${action.charAt(0).toUpperCase() + action.slice(1)}Success`));
          this.loadCharities();
        },
        error: (error: any) => {
          console.error(`Error ${action}ing add rights:`, error);
          this.notification.error(this.getTranslation(`charities.addRights${action.charAt(0).toUpperCase() + action.slice(1)}Failed`));
        }
      });
    }
  }

  async toggleUpdateRights(charity: CharityDto): Promise<void> {
    const action = charity.isUpdateEnabled ? 'disable' : 'enable';
    const confirmed = await this.notification.confirm(
      this.getTranslation(`charities.confirm${action.charAt(0).toUpperCase() + action.slice(1)}UpdateRights`)
    );

    if (confirmed) {
      const action$ = charity.isUpdateEnabled
        ? this.charityService.disableUpdateRights(charity.id)
        : this.charityService.enableUpdateRights(charity.id);

      action$.subscribe({
        next: () => {
          this.notification.success(this.getTranslation(`charities.updateRights${action.charAt(0).toUpperCase() + action.slice(1)}Success`));
          this.loadCharities();
        },
        error: (error: any) => {
          console.error(`Error ${action}ing update rights:`, error);
          this.notification.error(this.getTranslation(`charities.updateRights${action.charAt(0).toUpperCase() + action.slice(1)}Failed`));
        }
      });
    }
  }

  async resetPassword(charity: CharityDto): Promise<void> {
    const confirmed = await this.notification.confirm(
      this.getTranslation('charities.confirmResetPassword')
    );

    if (confirmed) {
      this.charityService.resetPassword({
        charityId: charity.id,
        sendEmail: true
      }).subscribe({
        next: () => {
          this.notification.success(this.getTranslation('charities.resetPasswordSuccess'));
        },
        error: (error: any) => {
          console.error('Error resetting password:', error);
          this.notification.error(this.getTranslation('charities.resetPasswordFailed'));
        }
      });
    }
  }

  async deleteCharity(charity: CharityDto): Promise<void> {
    const confirmed = await this.notification.confirm(
      this.getTranslation('charities.confirmDelete', { name: charity.name })
    );

    if (confirmed) {
      this.charityService.deleteCharity(charity.id).subscribe({
        next: () => {
          this.notification.success(this.getTranslation('charities.deleteSuccess'));
          this.loadCharities();
          this.loadStatistics();
        },
        error: (error: any) => {
          console.error('Error deleting charity:', error);
          this.notification.error(this.getTranslation('charities.deleteFailed'));
        }
      });
    }
  }

  exportToExcel(): void {
    this.loading = true;
    const formValues = this.filterForm.value;

    const searchRequest: CharitySearchRequest = {
      pageNumber: 1,
      pageSize: 10000
    };

    // Only add optional filters if they have values
    if (formValues.searchValue && formValues.searchValue.trim()) {
      searchRequest.searchTerm = formValues.searchValue.trim();
    }
    if (formValues.country) {
      searchRequest.countryId = formValues.country;
    }
    if (formValues.region) {
      searchRequest.regionId = formValues.region;
    }
    if (formValues.center) {
      searchRequest.centerId = formValues.center;
    }

    // Handle status filter
    const status = formValues.status;
    if (status === 'active') {
      searchRequest.isActive = true;
      searchRequest.isLocked = false;
    } else if (status === 'inactive') {
      searchRequest.isActive = false;
    } else if (status === 'locked') {
      searchRequest.isLocked = true;
    }

    this.charityService.exportToExcel(searchRequest).subscribe({
      next: (blob: Blob) => {
        const url = window.URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = `charities_${new Date().toISOString().split('T')[0]}.xlsx`;
        document.body.appendChild(a);
        a.click();
        document.body.removeChild(a);
        window.URL.revokeObjectURL(url);

        this.notification.success('Charities exported successfully');
        this.loading = false;
      },
      error: (error: any) => {
        console.error('Export failed:', error);
        this.notification.error('Failed to export charities');
        this.loading = false;
      }
    });
  }

  onPageChange(page: number): void {
    this.currentPage = page;
    this.loadCharities();
  }

  getStatusBadgeClass(charity: CharityDto): string {
    if (charity.isLocked) return 'badge-danger';
    if (!charity.isActive) return 'badge-warning';
    return 'badge-success';
  }

  getStatusText(charity: CharityDto): string {
    if (charity.isLocked) return this.getTranslation('charities.locked');
    if (!charity.isActive) return this.getTranslation('common.inactive');
    return this.getTranslation('common.active');
  }
}
