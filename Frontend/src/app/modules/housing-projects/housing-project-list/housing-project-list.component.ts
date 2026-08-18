/**
 * Housing Project List Component
 * Displays list of housing projects with filtering, searching, and actions
 * Access: Admin and Super Admin only
 * Refactored to use shared components like charities module
 */

import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { RouterModule, Router } from '@angular/router';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { Observable, forkJoin, Subject, Subscription } from 'rxjs';
import { takeUntil } from 'rxjs/operators';

import { HousingProjectService } from '../services/housing-project.service';
import { LookupManagementService } from '../../lookup-management/services/lookup-management.service';
import { NotificationService } from '../../../core/services/notification.service';
import {
  HousingProject,
  HousingProjectSearchRequest,
  ProjectType,
  ProjectStatus,
  getProjectTypeName,
  getProjectStatusName,
  getStatusBadgeClass
} from '../models/housing-project.model';
import {
  PaginationComponent,
  BreadcrumbComponent,
  BreadcrumbItem,
  PageHeaderComponent,
  DropDownComponent
} from '../../../shared/components';
import { SharedModule } from '../../../shared/shared.module';
import { CountryDto, RegionDto, CenterDto } from '../../lookup-management/models/lookup.model';

@Component({
  selector: 'app-housing-project-list',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    RouterModule,
    TranslateModule,
    PaginationComponent,
    BreadcrumbComponent,
    PageHeaderComponent,
    SharedModule,
    DropDownComponent
  ],
  templateUrl: './housing-project-list.component.html',
  styleUrls: ['./housing-project-list.component.scss']
})
export class HousingProjectListComponent implements OnInit, OnDestroy {
  private destroy$ = new Subject<void>();
  private langChangeSubscription?: Subscription;

  // Breadcrumb items
  breadcrumbs: BreadcrumbItem[] = [
    { label: 'common.home', url: '/dashboard' },
    { label: 'housingProjects.title' }
  ];

  // Page header actions
  pageActions = [
    {
      label: 'housingProjects.addProject',
      type: 'primary',
      icon: 'fe-plus',
      click: () => this.createProject()
    },
    {
      label: 'common.exportToExcel',
      type: 'success',
      icon: 'fe-file-plus',
      click: () => this.exportToExcel()
    }
  ];

  // Filter form with shared components
  filterForm!: FormGroup;

  // Data
  housingProjects: HousingProject[] = [];
  loading: boolean = false;

  // Lookup data
  allCountries: CountryDto[] = [];
  allRegions: RegionDto[] = [];
  allCenters: CenterDto[] = [];
  filteredRegions: RegionDto[] = [];
  filteredCenters: CenterDto[] = [];

  // Pagination
  currentPage: number = 1;
  pageSize: number = 10;
  totalCount: number = 0;
  totalPages: number = 0;

  // Project type options for dropdown
  projectTypeOptions: Array<{ id: string; name: string }> = [];

  // Project status options for dropdown
  statusOptions: Array<{ id: string; name: string }> = [];

  // Helpers
  getProjectTypeName = getProjectTypeName;
  getProjectStatusName = getProjectStatusName;
  getStatusBadgeClass = getStatusBadgeClass;
  Math = Math;

  constructor(
    private fb: FormBuilder,
    private housingProjectService: HousingProjectService,
    private lookupService: LookupManagementService,
    private notification: NotificationService,
    private translate: TranslateService,
    private router: Router
  ) {
    this.initFilterForm();
  }

  ngOnInit(): void {
    this.initializeDropdownOptions();
    this.loadLookupData();

    // Subscribe to language changes to update translated options
    this.langChangeSubscription = this.translate.onLangChange.subscribe(() => {
      this.initializeDropdownOptions();
    });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
    if (this.langChangeSubscription) {
      this.langChangeSubscription.unsubscribe();
    }
  }

  /**
   * Initialize filter form with FormBuilder
   */
  private initFilterForm(): void {
    this.filterForm = this.fb.group({
      searchValue: [''],
      countryId: [null],
      regionId: [null],
      centerId: [null],
      projectType: [null],
      projectStatus: [null],
      dateFrom: [null],
      dateTo: [null]
    });
  }

  /**
   * Initialize dropdown options with translated labels
   */
  private initializeDropdownOptions(): void {
    // Project type options
    this.projectTypeOptions = Object.values(ProjectType).map(type => ({
      id: type,
      name: this.getProjectTypeName(type)
    }));

    // Project status options
    this.statusOptions = Object.values(ProjectStatus).map(status => ({
      id: status,
      name: this.getProjectStatusName(status)
    }));
  }

  /**
   * Load lookup data for dropdowns
   */
  private loadLookupData(): void {
    const observables: Observable<any>[] = [
      this.lookupService.getCountries({ isActive: true })
    ];

    forkJoin(observables).pipe(takeUntil(this.destroy$)).subscribe({
      next: ([countries]) => {
        this.allCountries = (countries as any).items || [];
      },
      error: (error) => {
        console.error('Error loading lookup data:', error);
      }
    });
  }

  /**
   * Load regions by country for cascading dropdown
   */
  private loadRegionsByCountry(countryId: any): void {
    if (!countryId) {
      this.filteredRegions = [];
      return;
    }

    const cleanCountryId = typeof countryId === 'string' ? parseInt(countryId.trim(), 10) : countryId;
    this.lookupService.getRegionsByCountry(cleanCountryId).pipe(takeUntil(this.destroy$)).subscribe({
      next: (regions) => {
        this.filteredRegions = regions || [];
      },
      error: () => {
        this.filteredRegions = [];
      }
    });
  }

  /**
   * Load centers by region for cascading dropdown
   */
  private loadCentersByRegion(regionId: any): void {
    if (!regionId) {
      this.filteredCenters = [];
      return;
    }

    const cleanRegionId = typeof regionId === 'string' ? parseInt(regionId.trim(), 10) : regionId;
    this.lookupService.getCentersByRegion(cleanRegionId).pipe(takeUntil(this.destroy$)).subscribe({
      next: (centers) => {
        this.filteredCenters = centers || [];
      },
      error: () => {
        this.filteredCenters = [];
      }
    });
  }

  /**
   * Load housing projects with current filters
   */
  loadHousingProjects(): void {
    this.loading = true;
    const formValues = this.filterForm.value;

    const searchRequest: HousingProjectSearchRequest = {
      search: formValues.searchValue || undefined,
      projectType: formValues.projectType || undefined,
      projectStatus: formValues.projectStatus || undefined,
      countryId: formValues.countryId || undefined,
      regionId: formValues.regionId || undefined,
      centerId: formValues.centerId || undefined,
      dateFrom: formValues.dateFrom || undefined,
      dateTo: formValues.dateTo || undefined,
      page: this.currentPage,
      pageSize: this.pageSize
    };

    this.housingProjectService.getHousingProjects(searchRequest).pipe(takeUntil(this.destroy$)).subscribe({
      next: (response) => {
        this.housingProjects = response.items;
        this.totalCount = response.totalCount;
        this.totalPages = response.totalPages;
        this.loading = false;
      },
      error: (error) => {
        console.error('Error loading housing projects:', error);
        this.notification.error(this.translate.instant('housingProjects.loadProjectsFailed'));
        this.loading = false;
      }
    });
  }

  /**
   * Handle search
   */
  onSearch(): void {
    this.currentPage = 1;
    this.loadHousingProjects();
  }

  /**
   * Country dropdown change handler
   */
  onCountryDropDownChanged(value: any): void {
    if (value && value.id) {
      this.loadRegionsByCountry(value.id);
    } else {
      this.filteredRegions = [];
      this.filteredCenters = [];
    }
    this.onSearch();
  }

  /**
   * Region dropdown change handler
   */
  onRegionDropDownChanged(value: any): void {
    if (value && value.id) {
      this.loadCentersByRegion(value.id);
    } else {
      this.filteredCenters = [];
    }
    this.onSearch();
  }

  /**
   * Center dropdown change handler
   */
  onCenterDropDownChanged(value: any): void {
    this.onSearch();
  }

  /**
   * Project type dropdown change handler
   */
  onProjectTypeDropDownChanged(value: any): void {
    const projectTypeControl = this.filterForm.get('projectType');
    if (value && value.id !== undefined && value.id !== null) {
      projectTypeControl?.setValue(value.id);
    } else {
      projectTypeControl?.setValue(null);
    }
    this.onSearch();
  }

  /**
   * Project status dropdown change handler
   */
  onProjectStatusDropDownChanged(value: any): void {
    const statusControl = this.filterForm.get('projectStatus');
    if (value && value.id !== undefined && value.id !== null) {
      statusControl?.setValue(value.id);
    } else {
      statusControl?.setValue(null);
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
      formValues.countryId ||
      formValues.regionId ||
      formValues.centerId ||
      formValues.projectType ||
      formValues.projectStatus ||
      formValues.dateFrom ||
      formValues.dateTo
    );
  }

  /**
   * Clear all filters
   */
  clearFilters(): void {
    this.filterForm.reset({
      searchValue: '',
      countryId: null,
      regionId: null,
      centerId: null,
      projectType: null,
      projectStatus: null,
      dateFrom: null,
      dateTo: null
    });
    this.filteredRegions = [];
    this.filteredCenters = [];
    this.currentPage = 1;
    this.loadHousingProjects();
  }

  /**
   * Handle page change
   */
  onPageChange(page: number): void {
    this.currentPage = page;
    this.loadHousingProjects();
  }

  /**
   * Delete housing project
   */
  async deleteProject(project: HousingProject): Promise<void> {
    const confirmed = await this.notification.confirm(
      this.translate.instant('housingProjects.deleteConfirmMessage', { name: project.projectName })
    );

    if (confirmed) {
      this.housingProjectService.deleteHousingProject(project.id).pipe(takeUntil(this.destroy$)).subscribe({
        next: () => {
          this.notification.success(this.translate.instant('housingProjects.deleteSuccess'));
          this.loadHousingProjects();
        },
        error: (error) => {
          console.error('Error deleting housing project:', error);
          this.notification.error(this.translate.instant('housingProjects.deleteFailed'));
        }
      });
    }
  }

  /**
   * Create new project
   */
  createProject(): void {
    this.router.navigate(['/housing-projects', 'create']);
  }

  /**
   * Export to Excel
   */
  exportToExcel(): void {
    this.loading = true;
    const formValues = this.filterForm.value;

    const searchRequest: HousingProjectSearchRequest = {
      search: formValues.searchValue || undefined,
      projectType: formValues.projectType || undefined,
      projectStatus: formValues.projectStatus || undefined,
      countryId: formValues.countryId || undefined,
      regionId: formValues.regionId || undefined,
      centerId: formValues.centerId || undefined,
      dateFrom: formValues.dateFrom || undefined,
      dateTo: formValues.dateTo || undefined,
      page: 1,
      pageSize: this.totalCount
    };

    this.housingProjectService.exportHousingProjects(searchRequest).pipe(takeUntil(this.destroy$)).subscribe({
      next: (blob) => {
        const url = window.URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = `housing-projects-${new Date().toISOString().split('T')[0]}.xlsx`;
        document.body.appendChild(a);
        a.click();
        document.body.removeChild(a);
        window.URL.revokeObjectURL(url);
        this.notification.success(this.translate.instant('housingProjects.exportSuccess'));
        this.loading = false;
      },
      error: (error) => {
        console.error('Error exporting housing projects:', error);
        this.notification.error(this.translate.instant('housingProjects.exportFailed'));
        this.loading = false;
      }
    });
  }

  /**
   * Track by function for ngFor
   */
  trackByProjectId(index: number, project: HousingProject): string {
    return project.id;
  }
}

