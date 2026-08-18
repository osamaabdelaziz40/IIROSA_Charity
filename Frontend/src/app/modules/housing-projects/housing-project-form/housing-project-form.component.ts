/**
 * Housing Project Form Component
 * Handles creation and editing of housing projects
 * Access: Admin and Super Admin only
 */

import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { RouterModule, ActivatedRoute, Router } from '@angular/router';
import { TranslateModule, TranslateService } from '@ngx-translate/core';

import { HousingProjectService } from '../services/housing-project.service';
import {
  HousingProject,
  CreateHousingProjectRequest,
  UpdateHousingProjectRequest,
  ProjectType,
  HousingType,
  Currency,
  ProjectStatus,
  ProjectStage
} from '../models/housing-project.model';
import { PageHeaderComponent } from '../../../shared/components/page-header/page-header.component';
import { BreadcrumbComponent, BreadcrumbItem } from '../../../shared/components';
import { SharedModule } from '../../../shared/shared.module';
import { LookupBase } from '../../../shared/models/lookup.base.model';
import { LookupManagementService } from '../../../modules/lookup-management/services/lookup-management.service';
import { CountryDto, RegionDto, CenterDto } from '../../../modules/lookup-management/models/lookup.model';
import { CharityService } from '../../../modules/charities/services/charity.service';
import { CharityDto } from '../../../modules/charities/models/charity.model';
import { FamilyService } from '../../../modules/families/services/family.service';
import { FamilyDto } from '../../../modules/families/models/family.model';

@Component({
  selector: 'app-housing-project-form',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    RouterModule,
    TranslateModule,
    PageHeaderComponent,
    BreadcrumbComponent,
    SharedModule
  ],
  templateUrl: './housing-project-form.component.html',
  styleUrls: ['./housing-project-form.component.scss']
})
export class HousingProjectFormComponent implements OnInit {
  // Breadcrumb items
  breadcrumbs: BreadcrumbItem[] = [
    { label: 'common.home', url: '/dashboard' },
    { label: 'housingProjects.title', url: '/housing-projects' }
  ];

  get breadcrumbsWithAction(): BreadcrumbItem[] {
    return [
      ...this.breadcrumbs,
      { label: this.isEditMode ? 'housingProjects.editProject' : 'housingProjects.addProject' }
    ];
  }
  // Form
  housingProjectForm: FormGroup;
  isEditMode: boolean = false;
  projectId: string = '';
  loading: boolean = false;
  saving: boolean = false;

  // Enums for select options
  projectTypes = Object.values(ProjectType);
  housingTypes = Object.values(HousingType);
  currencies = Object.values(Currency);
  projectStatuses = Object.values(ProjectStatus);
  projectStages = Object.values(ProjectStage);

  // Lookup data (will be populated from API)
  countries: any[] = [];
  regions: any[] = [];
  centers: any[] = [];
  charities: any[] = [];
  families: any[] = [];

  // Filtered lists
  filteredRegions: any[] = [];
  filteredCenters: any[] = [];
  filteredFamilies: any[] = [];

  // Dropdown options (LookupBase format)
  projectTypeOptions: LookupBase[] = [];
  housingTypeOptions: LookupBase[] = [];
  currencyOptions: LookupBase[] = [];
  projectStatusOptions: LookupBase[] = [];
  projectStageOptions: LookupBase[] = [];
  allCountries: LookupBase[] = [];
  charityOptions: LookupBase[] = [];

  // Page actions for header
  pageActions = [
    {
      label: 'common.cancel',
      type: 'secondary',
      icon: 'x',
      click: () => this.onCancel()
    }
  ];

  constructor(
    private fb: FormBuilder,
    private route: ActivatedRoute,
    private router: Router,
    private housingProjectService: HousingProjectService,
    private lookupManagementService: LookupManagementService,
    private charityService: CharityService,
    private familyService: FamilyService,
    private translate: TranslateService
  ) {
    this.housingProjectForm = this.createForm();
  }

  ngOnInit(): void {
    this.projectId = this.route.snapshot.paramMap.get('id') || '';
    this.isEditMode = !!this.projectId;

    if (this.isEditMode) {
      this.loadHousingProject();
    }

    // Setup cascading dropdowns
    this.setupCascadingDropdowns();

    // Load lookup data
    this.loadLookupData();
  }

  /**
   * Create the form group
   */
  private createForm(): FormGroup {
    return this.fb.group({
      // Basic Information
      projectName: ['', Validators.required],
      projectDescription: ['', Validators.required],
      projectType: [ProjectType.NewConstruction, Validators.required],
      startDate: ['', Validators.required],
      expectedEndDate: ['', Validators.required],
      actualEndDate: [''], // Optional

      // Location
      countryId: [null, Validators.required],
      regionId: [null, Validators.required],
      centerId: [null, Validators.required],
      address: ['', Validators.required],
      village: ['', Validators.required],
      gpsCoordinates: ['', Validators.required],

      // Specifications
      housingType: [HousingType.House, Validators.required],
      numberOfUnits: [null, [Validators.required, Validators.min(1)]],
      areaPerUnit: [null, [Validators.required, Validators.min(0.01)]],
      totalArea: [null, [Validators.required, Validators.min(0.01)]],
      floorsPerUnit: [null, [Validators.required, Validators.min(1)]],
      roomsPerUnit: [null, [Validators.required, Validators.min(1)]],
      constructionMaterial: ['', Validators.required],

      // Financial Information
      totalBudget: [null, [Validators.required, Validators.min(0.01)]],
      budgetCurrency: [Currency.EGP, Validators.required],
      donorName: ['', Validators.required],
      fundingSource: ['', Validators.required],
      contractorName: ['', Validators.required],
      supervisorName: ['', Validators.required],

      // Beneficiary Assignment
      assignedCharityId: [null, Validators.required],
      assignedFamilyId: [null, Validators.required],

      // Status
      projectStatus: [ProjectStatus.Planning, Validators.required],
      completionPercentage: [0, [Validators.min(0), Validators.max(100)]],
      currentStage: [null, Validators.required],

      // Notes
      notes: ['', Validators.required]
    });
  }

  /**
   * Setup cascading dropdown behavior
   */
  private setupCascadingDropdowns(): void {
    // When country changes, load regions from backend
    this.housingProjectForm.get('countryId')?.valueChanges.subscribe((countryId) => {
      if (countryId) {
        this.loadRegionsByCountry(countryId);
      } else {
        this.filteredRegions = [];
        this.filteredCenters = [];
      }
      this.housingProjectForm.patchValue({
        regionId: null,
        centerId: null
      });
    });

    // When region changes, load centers from backend
    this.housingProjectForm.get('regionId')?.valueChanges.subscribe((regionId) => {
      if (regionId) {
        this.loadCentersByRegion(regionId);
      } else {
        this.filteredCenters = [];
      }
      this.housingProjectForm.patchValue({ centerId: null });
    });

    // When charity changes, load families from backend
    this.housingProjectForm.get('assignedCharityId')?.valueChanges.subscribe((charityId) => {
      if (charityId) {
        this.loadFamiliesByCharity(charityId);
      } else {
        this.filteredFamilies = [];
      }
      this.housingProjectForm.patchValue({ assignedFamilyId: null });
    });
  }

  /**
   * Filter regions by country
   */
  private filterRegions(countryId: number | null): void {
    if (!countryId) {
      this.filteredRegions = [];
      return;
    }

    const filtered = this.regions.filter(region => region.countryId === countryId);
    this.filteredRegions = filtered.map(r => ({
      id: r.id,
      name: r.name,
      systemValue: r.name
    }));
  }

  /**
   * Filter centers by region
   */
  private filterCenters(regionId: number | null): void {
    if (!regionId) {
      this.filteredCenters = [];
      return;
    }

    const filtered = this.centers.filter(center => center.regionId === regionId);
    this.filteredCenters = filtered.map(c => ({
      id: c.id,
      name: c.name,
      systemValue: c.name
    }));
  }

  /**
   * Filter families by charity
   */
  private filterFamilies(charityId: number | null): void {
    if (!charityId) {
      this.filteredFamilies = [];
      return;
    }

    // Convert charityId to string for comparison (FamilyDto.charityId is string)
    const charityIdStr = charityId.toString();
    const filtered = this.families.filter(family => family.charityId === charityIdStr);
    this.filteredFamilies = filtered.map(f => ({
      id: f.id,
      name: f.name,
      systemValue: f.name
    }));
  }

  /**
   * Load lookup data from API
   */
  private loadLookupData(): void {
    // Load enum-based options with localization
    this.loadEnumOptions();

    // Load Countries from backend
    this.loadCountries();

    // Load Charities from backend
    this.loadCharities();
  }

  /**
   * Load enum-based options with localization keys
   * These use ngx-translate for localization
   */
  private loadEnumOptions(): void {
    // Project Types - localized via translate pipe
    this.projectTypeOptions = this.projectTypes.map(type => ({
      id: type,
      name: `housingProjects.projectTypes.${type}`,
      systemValue: type
    }));

    // Housing Types - localized via translate pipe
    this.housingTypeOptions = this.housingTypes.map(type => ({
      id: type,
      name: `housingProjects.housingTypes.${type}`,
      systemValue: type
    }));

    // Currencies
    this.currencyOptions = this.currencies.map(currency => ({
      id: currency,
      name: currency,
      systemValue: currency
    }));

    // Project Statuses - localized via translate pipe
    this.projectStatusOptions = this.projectStatuses.map(status => ({
      id: status,
      name: `housingProjects.projectStatuses.${status}`,
      systemValue: status
    }));

    // Project Stages - localized via translate pipe
    this.projectStageOptions = this.projectStages.map(stage => ({
      id: stage,
      name: `housingProjects.projectStages.${stage}`,
      systemValue: stage
    }));
  }

  /**
   * Load Countries from backend lookup service
   */
  private loadCountries(): void {
    this.lookupManagementService.getCountries({ page: 1, pageSize: 1000, isActive: true }).subscribe({
      next: (response) => {
        this.countries = response.items.map(country => ({
          id: country.id,
          name: country.name,
          nameAr: country.nameAr || country.name,
          isoCode: country.isoCode
        }));

        // Convert to LookupBase format for dropdown
        this.allCountries = this.countries.map(c => ({
          id: c.id,
          name: c.name,
          systemValue: c.name
        }));
      },
      error: (error) => {
        console.error('Error loading countries:', error);
      }
    });
  }

  /**
   * Load Regions from backend by country
   */
  private loadRegionsByCountry(countryId: number): void {
    this.lookupManagementService.getRegionsByCountry(countryId).subscribe({
      next: (regions) => {
        this.regions = regions.map(region => ({
          id: region.id,
          name: region.name,
          nameAr: region.nameAr || region.name,
          countryId: region.countryId
        }));

        // Update filtered regions
        this.filterRegions(countryId);
      },
      error: (error) => {
        console.error('Error loading regions:', error);
        this.filteredRegions = [];
      }
    });
  }

  /**
   * Load Centers from backend by region
   */
  private loadCentersByRegion(regionId: number): void {
    this.lookupManagementService.getCentersByRegion(regionId).subscribe({
      next: (centers) => {
        this.centers = centers.map(center => ({
          id: center.id,
          name: center.name,
          nameAr: center.nameAr || center.name,
          regionId: center.regionId,
          countryId: center.countryId
        }));

        // Update filtered centers
        this.filterCenters(regionId);
      },
      error: (error) => {
        console.error('Error loading centers:', error);
        this.filteredCenters = [];
      }
    });
  }

  /**
   * Load Charities from backend
   */
  private loadCharities(): void {
    this.charityService.getCharities({
      pageNumber: 1,
      pageSize: 1000,
      isActive: true
    }).subscribe({
      next: (response) => {
        this.charities = response.items.map(charity => ({
          id: charity.id,
          name: charity.name,
          code: charity.code
        }));

        // Convert to LookupBase format for dropdown
        this.charityOptions = this.charities.map(c => ({
          id: c.id,
          name: c.name,
          systemValue: c.name
        }));
      },
      error: (error) => {
        console.error('Error loading charities:', error);
      }
    });
  }

  /**
   * Load Families from backend by charity
   */
  private loadFamiliesByCharity(charityId: number): void {
    // Convert number to string for charityId
    const charityIdStr = charityId.toString();

    // Use the family service to get families filtered by charity
    this.familyService.getFamilies({
      pageNumber: 1,
      pageSize: 1000,
      charityId: charityIdStr,
      isActive: true
    }).subscribe({
      next: (response) => {
        this.families = response.items.map(family => ({
          id: family.id,
          name: family.code || family.id,
          code: family.code,
          charityId: family.charityId
        }));

        // Update filtered families
        this.filterFamilies(charityId);
      },
      error: (error) => {
        console.error('Error loading families:', error);
        this.filteredFamilies = [];
      }
    });
  }

  /**
   * Load housing project for editing
   */
  private loadHousingProject(): void {
    this.loading = true;

    this.housingProjectService.getHousingProjectById(this.projectId).subscribe({
      next: (project) => {
        this.housingProjectForm.patchValue({
          projectName: project.projectName,
          projectDescription: project.projectDescription,
          projectType: project.projectType,
          startDate: project.startDate.split('T')[0],
          expectedEndDate: project.expectedEndDate ? project.expectedEndDate.split('T')[0] : '',
          actualEndDate: project.actualEndDate ? project.actualEndDate.split('T')[0] : '',
          countryId: project.countryId,
          regionId: project.regionId,
          centerId: project.centerId,
          address: project.address,
          village: project.village,
          gpsCoordinates: project.gpsCoordinates,
          housingType: project.housingType,
          numberOfUnits: project.numberOfUnits,
          areaPerUnit: project.areaPerUnit,
          totalArea: project.totalArea,
          floorsPerUnit: project.floorsPerUnit,
          roomsPerUnit: project.roomsPerUnit,
          constructionMaterial: project.constructionMaterial,
          totalBudget: project.totalBudget,
          budgetCurrency: project.budgetCurrency,
          donorName: project.donorName,
          fundingSource: project.fundingSource,
          contractorName: project.contractorName,
          supervisorName: project.supervisorName,
          assignedCharityId: project.assignedCharityId,
          assignedFamilyId: project.assignedFamilyId,
          projectStatus: project.projectStatus,
          completionPercentage: project.completionPercentage,
          currentStage: project.currentStage,
          notes: project.notes
        });

        // Trigger cascading filters with API calls
        if (project.countryId) {
          this.loadRegionsByCountry(project.countryId);
        }
        if (project.regionId) {
          this.loadCentersByRegion(project.regionId);
        }
        if (project.assignedCharityId) {
          this.loadFamiliesByCharity(project.assignedCharityId);
        }

        this.loading = false;
      },
      error: (error) => {
        console.error('Error loading housing project:', error);
        this.loading = false;
        this.router.navigate(['/housing-projects']);
      }
    });
  }

  /**
   * Submit the form
   */
  onSubmit(): void {
    if (this.housingProjectForm.invalid) {
      this.markFormGroupTouched(this.housingProjectForm);
      return;
    }

    this.saving = true;

    if (this.isEditMode) {
      this.updateHousingProject();
    } else {
      this.createHousingProject();
    }
  }

  /**
   * Create new housing project
   */
  private createHousingProject(): void {
    const request: CreateHousingProjectRequest = this.housingProjectForm.value;

    this.housingProjectService.createHousingProject(request).subscribe({
      next: () => {
        this.saving = false;
        this.router.navigate(['/housing-projects']);
      },
      error: (error) => {
        console.error('Error creating housing project:', error);
        this.saving = false;
      }
    });
  }

  /**
   * Update existing housing project
   */
  private updateHousingProject(): void {
    const request: UpdateHousingProjectRequest = this.housingProjectForm.value;

    this.housingProjectService.updateHousingProject(this.projectId, request).subscribe({
      next: () => {
        this.saving = false;
        this.router.navigate(['/housing-projects', this.projectId]);
      },
      error: (error) => {
        console.error('Error updating housing project:', error);
        this.saving = false;
      }
    });
  }

  /**
   * Cancel and go back
   */
  onCancel(): void {
    if (this.isEditMode) {
      this.router.navigate(['/housing-projects', this.projectId]);
    } else {
      this.router.navigate(['/housing-projects']);
    }
  }

  /**
   * Mark all controls as touched to show validation errors
   */
  private markFormGroupTouched(formGroup: FormGroup): void {
    Object.values(formGroup.controls).forEach(control => {
      control.markAsTouched();

      if (control instanceof FormGroup) {
        this.markFormGroupTouched(control);
      }
    });
  }

  /**
   * Check if field has error
   */
  hasError(fieldName: string, errorName: string): boolean {
    const field = this.housingProjectForm.get(fieldName);
    return field?.touched && field?.hasError(errorName) || false;
  }

  /**
   * Get page title
   */
  get pageTitle(): string {
    return this.isEditMode ? 'housingProjects.editProject' : 'housingProjects.addProject';
  }

  /**
   * Get submit button text
   */
  get submitButtonText(): string {
    return this.isEditMode ? 'common.update' : 'common.create';
  }

  /**
   * Handle country dropdown change
   */
  onCountryDropDownChanged(event: any): void {
    const countryId = event?.id;
    if (countryId) {
      this.loadRegionsByCountry(countryId);
    } else {
      this.filteredRegions = [];
      this.filteredCenters = [];
    }
    this.housingProjectForm.patchValue({
      regionId: null,
      centerId: null
    });
  }

  /**
   * Handle region dropdown change
   */
  onRegionDropDownChanged(event: any): void {
    const regionId = event?.id;
    if (regionId) {
      this.loadCentersByRegion(regionId);
    } else {
      this.filteredCenters = [];
    }
    this.housingProjectForm.patchValue({ centerId: null });
  }

  /**
   * Handle charity dropdown change
   */
  onCharityDropDownChanged(event: any): void {
    const charityId = event?.id;
    if (charityId) {
      this.loadFamiliesByCharity(charityId);
    } else {
      this.filteredFamilies = [];
    }
    this.housingProjectForm.patchValue({ assignedFamilyId: null });
  }
}
