import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import {
  FormBuilder,
  FormGroup,
  ReactiveFormsModule
} from '@angular/forms';
import {
  OfficeProjectListItem,
  OfficeProjectFilter,
  BeneficiaryType
} from '../models/office-project.model';
import { OfficeProjectService } from '../services/office-project.service';
import { LookupManagementService } from '../../lookup-management/services/lookup-management.service';
import { CountryDto, RegionDto, CenterDto } from '../../lookup-management/models/lookup.model';
import { NotificationService } from '../../../core/services/notification.service';
import { PageHeaderComponent } from '../../../shared/components/page-header/page-header.component';
import { PaginationComponent } from '../../../shared/components/pagination/pagination.component';
import { TranslateModule, TranslateService, LangChangeEvent } from '@ngx-translate/core';
import { RouterModule } from '@angular/router';
import { BreadcrumbComponent, BreadcrumbItem } from '../../../shared/components';
import { DropDownComponent } from '../../../shared/components/drop-down/drop-down.component';
import { SharedModule } from '../../../shared/shared.module';
import { CharityService } from '../../charities/services/charity.service';
import { CharitySearchRequest } from '../../charities/models/charity.model';
import { AuthService } from '../../../core/services/auth.service';
import { Subject, Subscription } from 'rxjs';
import { takeUntil } from 'rxjs/operators';

@Component({
  selector: 'app-project-list',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    SharedModule,
    PageHeaderComponent,
    PaginationComponent,
    TranslateModule,
    RouterModule,
    BreadcrumbComponent,
    DropDownComponent
  ],
  templateUrl: './project-list.component.html',
  styleUrls: ['./project-list.component.scss']
})
export class ProjectListComponent implements OnInit, OnDestroy {
  private destroy$ = new Subject<void>();
  private langChangeSubscription?: Subscription;

  // Breadcrumb items
  breadcrumbs: BreadcrumbItem[] = [
    { label: 'common.home', url: '/dashboard' },
    { label: 'officeDevelopmentProjects.title' }
  ];

  projects: OfficeProjectListItem[] = [];
  allCountries: CountryDto[] = [];
  allRegions: RegionDto[] = [];
  allCenters: CenterDto[] = [];
  allProjectTypes: any[] = [];
  allCharities: any[] = [];

  loading = false;
  loadingCountries = false;
  loadingRegions = false;
  loadingCenters = false;
  loadingProjectTypes = false;
  loadingCharities = false;

  totalCount = 0;
  currentPage = 1;
  pageSize = 20;

  // Filter form
  filterForm: FormGroup;

  // Status options for dropdown - localized
  statusOptions: Array<{ id: string | null; name: string }> = [];

  pageActions = [
    {
      label: 'officeDevelopmentProjects.addProject',
      type: 'primary',
      icon: 'fe-plus',
      click: () => this.createProject()
    },
    {
      label: 'officeDevelopmentProjects.projectProgress',
      type: 'info',
      icon: 'fe-activity',
      click: () => this.viewProgress()
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
    private projectService: OfficeProjectService,
    private lookupService: LookupManagementService,
    private notification: NotificationService,
    private router: Router,
    private charityService: CharityService,
    private auth: AuthService,
    private translate: TranslateService
  ) {
    this.filterForm = this.fb.group({
      searchValue: [''],
      projectType: [null],
      country: [null],
      region: [null],
      center: [null],
      charity: [null],
      status: [null],
      dateFrom: [null],
      dateTo: [null]
    });
  }

  ngOnInit(): void {
    this.initializeStatusOptions();

    // Subscribe to language changes to update translated options
    this.langChangeSubscription = this.translate.onLangChange.subscribe((event: LangChangeEvent) => {
      this.initializeStatusOptions();
    });

    this.loadLookupData();
    this.loadProjects();

    // Subscribe to form value changes for real-time filtering
    this.filterForm.valueChanges.subscribe(() => {
      // Debounce could be added here if needed
    });
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
      { id: null, name: this.translate.instant('common.all') },
      { id: 'false', name: this.translate.instant('officeDevelopmentProjects.ongoing') },
      { id: 'true', name: this.translate.instant('officeDevelopmentProjects.completed') }
    ];
  }

  loadLookupData(): void {
    this.loadCountries();
    this.loadProjectTypes();
    this.loadCharities();
  }

  loadCountries(): void {
    this.loadingCountries = true;
    this.lookupService.getCountries({ isActive: true }).subscribe({
      next: (response) => {
        this.allCountries = response?.items || [];
        this.loadingCountries = false;
      },
      error: () => {
        this.allCountries = [];
        this.loadingCountries = false;
      }
    });
  }


  loadProjectTypes(): void {
    this.loadingProjectTypes = true;
    this.projectService.getProjectTypes().subscribe({
      next: (data) => {
        this.allProjectTypes = data || [];
        this.loadingProjectTypes = false;
      },
      error: () => {
        this.allProjectTypes = [];
        this.loadingProjectTypes = false;
      }
    });
  }

  loadCharities(): void {
    this.loadingCharities = true;
    // Use CharityService like charity-list does
    const searchRequest: CharitySearchRequest = {
      pageNumber: 1,
      pageSize: 1000 // Get all charities for the dropdown
    };
    this.charityService.getCharities(searchRequest).subscribe({
      next: (response) => {
        this.allCharities = response?.items || [];
        this.loadingCharities = false;
      },
      error: () => {
        this.allCharities = [];
        this.loadingCharities = false;
      }
    });
  }

  loadProjects(): void {
    this.loading = true;
    const formValues = this.filterForm.value;

    const filter: OfficeProjectFilter = {
      searchText: formValues.searchValue || undefined,
      officeProjectTypeId: formValues.projectType || undefined,
      countryId: formValues.country || undefined,
      regionId: formValues.region || undefined,
      centerId: formValues.center || undefined,
      charityId: formValues.charity || undefined,
      isFinished: formValues.status === 'true' ? true : formValues.status === 'false' ? false : undefined,
      startDate: formValues.dateFrom || undefined,
      endDate: formValues.dateTo || undefined,
      page: this.currentPage,
      pageSize: this.pageSize
    };

    this.projectService.getAllProjects(filter).subscribe({
      next: (response) => {
        // Ensure we always get an array
        this.projects = Array.isArray(response?.items) ? response.items : [];
        this.totalCount = response?.totalCount || 0;
        this.loading = false;
      },
      error: (error: any) => {
        console.error('Error loading projects:', error);
        this.projects = []; // Ensure array on error
        this.totalCount = 0;
        this.notification.error(
          `Failed to load projects: ${error.message || 'Unknown error'}`
        );
        this.loading = false;
      }
    });
  }

  onSearch(): void {
    this.currentPage = 1;
    this.loadProjects();
  }

  onCountryChange(event?: any): void {
    // Event might emit an object with id property
    const countryId = event?.id || event || this.filterForm.get('country')?.value;
    this.filterForm.patchValue({ region: null, center: null });
    // Load regions using the extracted countryId
    if (countryId) {
      this.loadingRegions = true;
      this.lookupService.getRegionsByCountry(countryId).subscribe({
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
    this.onSearch();
  }

  onRegionChange(event?: any): void {
    // Event might emit an object with id property
    const regionId = event?.id || event || this.filterForm.get('region')?.value;
    this.filterForm.patchValue({ center: null });
    // Load centers using the extracted regionId
    if (regionId) {
      this.loadingCenters = true;
      this.lookupService.getCentersByRegion(regionId).subscribe({
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
    this.onSearch();
  }

  onCenterChange(event?: any): void {
    this.onSearch();
  }

  onProjectTypeChange(event?: any): void {
    this.onSearch();
  }

  onCharityChange(event?: any): void {
    this.onSearch();
  }

  onStatusChange(event?: any): void {
    this.onSearch();
  }

  onDateChange(event?: any): void {
    this.onSearch();
  }

  clearFilters(): void {
    this.filterForm.reset({
      searchValue: '',
      projectType: null,
      country: null,
      region: null,
      center: null,
      charity: null,
      status: null,
      dateFrom: null,
      dateTo: null
    });
    this.allRegions = [];
    this.allCenters = [];
    this.onSearch();
  }

  viewProject(projectId: string): void {
    this.router.navigate(['/office-development-projects', projectId]);
  }

  editProject(projectId: string): void {
    this.router.navigate(['/office-development-projects', projectId, 'edit']);
  }

  deleteProject(projectId: string): void {
    if (confirm('Are you sure you want to delete this project?')) {
      this.loading = true;
      this.projectService.deleteProject(projectId).subscribe({
        next: () => {
          this.notification.success('Project deleted successfully');
          this.loadProjects();
        },
        error: (error: any) => {
          console.error('Error deleting project:', error);
          this.notification.error(
            `Failed to delete project: ${error.message || 'Unknown error'}`
          );
          this.loading = false;
        }
      });
    }
  }

  markAsCompleted(projectId: string): void {
    if (confirm('Are you sure you want to mark this project as completed?')) {
      this.loading = true;
      this.projectService.markAsCompleted(projectId, true).subscribe({
        next: () => {
          this.notification.success('Project marked as completed');
          this.loadProjects();
        },
        error: (error: any) => {
          console.error('Error marking project as completed:', error);
          this.notification.error(
            `Failed to mark project as completed: ${error.message || 'Unknown error'}`
          );
          this.loading = false;
        }
      });
    }
  }

  createProject(): void {
    this.router.navigate(['/office-development-projects', 'create']);
  }

  viewProgress(): void {
    this.router.navigate(['/office-development-projects', 'progress']);
  }

  exportToExcel(): void {
    const formValues = this.filterForm.value;

    const filter: OfficeProjectFilter = {
      searchText: formValues.searchValue || undefined,
      officeProjectTypeId: formValues.projectType || undefined,
      countryId: formValues.country || undefined,
      regionId: formValues.region || undefined,
      centerId: formValues.center || undefined,
      charityId: formValues.charity || undefined,
      isFinished: formValues.status === 'true' ? true : formValues.status === 'false' ? false : undefined,
      startDate: formValues.dateFrom || undefined,
      endDate: formValues.dateTo || undefined
    };

    this.projectService.exportToExcel(filter).subscribe({
      next: (blob: Blob) => {
        const url = window.URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = `office-projects-${new Date().toISOString().split('T')[0]}.xlsx`;
        document.body.appendChild(a);
        a.click();
        document.body.removeChild(a);
        window.URL.revokeObjectURL(url);
        this.notification.success('Projects exported successfully');
      },
      error: (error: any) => {
        console.error('Error exporting projects:', error);
        this.notification.error(
          `Failed to export projects: ${error.message || 'Unknown error'}`
        );
      }
    });
  }

  onPageChange(page: number): void {
    this.currentPage = page;
    this.loadProjects();
  }

  onPageSizeChange(size: number): void {
    this.pageSize = size;
    this.currentPage = 1;
    this.loadProjects();
  }

  // The server already returns the requested page (page/pageSize go up in the filter), so the
  // rows are shown as-is — slicing again here would blank every page after the first.
  get paginatedProjects(): OfficeProjectListItem[] {
    return Array.isArray(this.projects) ? this.projects : [];
  }

  // UC-OFP-05: deletion is the General Director's alone. The endpoint enforces it; this only
  // keeps the button from offering an action the server would refuse.
  get canDelete(): boolean {
    return this.auth.hasRole('SuperAdmin');
  }

  get totalPages(): number {
    return Math.ceil(this.totalCount / this.pageSize);
  }

  getBeneficiaryTypeLabel(type: string): string {
    switch (type) {
      case BeneficiaryType.Families:
        return 'officeDevelopmentProjects.beneficiaryTypeFamilies';
      case BeneficiaryType.Individuals:
        return 'officeDevelopmentProjects.beneficiaryTypeIndividuals';
      case BeneficiaryType.Both:
        return 'officeDevelopmentProjects.beneficiaryTypeBoth';
      default:
        return '-';
    }
  }

  formatDate(date: Date | string): string {
    if (!date) return '-';
    return new Date(date).toLocaleDateString();
  }

  formatCurrency(amount: number | undefined): string {
    if (amount === undefined || amount === null) return '-';
    return new Intl.NumberFormat('en-US', {
      minimumFractionDigits: 2,
      maximumFractionDigits: 2
    }).format(amount);
  }

  getPageRange(): number[] {
    const pages: number[] = [];
    const maxPagesToShow = 5;
    let startPage = Math.max(1, this.currentPage - Math.floor(maxPagesToShow / 2));
    let endPage = Math.min(this.totalPages, startPage + maxPagesToShow - 1);

    if (endPage - startPage + 1 < maxPagesToShow) {
      startPage = Math.max(1, endPage - maxPagesToShow + 1);
    }

    for (let i = startPage; i <= endPage; i++) {
      pages.push(i);
    }

    return pages;
  }

  // Check if any filters are active
  hasActiveFilters(): boolean {
    const formValues = this.filterForm.value;
    return !!(
      formValues.searchValue ||
      formValues.projectType ||
      formValues.country ||
      formValues.region ||
      formValues.center ||
      formValues.charity ||
      formValues.status ||
      formValues.dateFrom ||
      formValues.dateTo
    );
  }
}
