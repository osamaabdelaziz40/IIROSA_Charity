import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup, Validators, FormControl } from '@angular/forms';
import { ActivatedRoute, Router, Params } from '@angular/router';
import {
  OfficeProject,
  OfficeProjectDto,
  BeneficiaryType,
  DocumentType,
  ReportType
} from '../models/office-project.model';
import { OfficeProjectService } from '../services/office-project.service';
import { LookupManagementService } from '../../lookup-management/services/lookup-management.service';
import { CharityService } from '../../charities/services/charity.service';
import { CharitySearchRequest } from '../../charities/models/charity.model';
import { CountryDto, RegionDto, CenterDto } from '../../lookup-management/models/lookup.model';
import { NotificationService } from '../../../core/services/notification.service';
import { PageHeaderComponent } from '../../../shared/components/page-header/page-header.component';
import { BreadcrumbComponent, BreadcrumbItem, AttachmentInputComponent } from '../../../shared/components';
import { TranslateModule, TranslateService, LangChangeEvent } from '@ngx-translate/core';
import { SharedModule, AttachmentFileType, AttachmentDto } from '../../../shared/shared.module';
import { Observable, forkJoin, Subject, Subscription } from 'rxjs';
import { takeUntil } from 'rxjs/operators';

@Component({
  selector: 'app-project-form',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    TranslateModule,
    PageHeaderComponent,
    BreadcrumbComponent,
    SharedModule,
    AttachmentInputComponent
  ],
  templateUrl: './project-form.component.html',
  styleUrls: ['./project-form.component.scss']
})
export class ProjectFormComponent implements OnInit, OnDestroy {
  private destroy$ = new Subject<void>();
  private langChangeSubscription?: Subscription;

  // Form
  projectForm!: FormGroup;
  isEditMode = false;
  projectId: string | null = null;
  loading = false;
  saving = false;

  // Lookup data
  allProjectTypes: any[] = [];
  allCountries: CountryDto[] = [];
  allRegions: RegionDto[] = [];
  allCenters: CenterDto[] = [];
  allCharities: any[] = [];
  filteredRegions: RegionDto[] = [];
  filteredCenters: CenterDto[] = [];

  // Document types
  documentTypes = Object.values(DocumentType);
  reportTypes = Object.values(ReportType);
  beneficiaryTypes = Object.values(BeneficiaryType);

  // Cached dropdown options (to avoid infinite loop with DropDown component)
  beneficiaryTypeOptions: Array<{ id: string; name: string }> = [];
  projectTypeOptions: any[] = [];
  countryOptions: CountryDto[] = [];
  charityOptions: any[] = [];

  // Attachments
  documentAttachList: AttachmentDto[] = [];
  reportAttachList: AttachmentDto[] = [];

  attachmentFileType = AttachmentFileType;

  // Page actions for header
  pageActions = [
    {
      label: 'common.cancel',
      type: 'secondary',
      icon: 'x',
      click: () => this.onCancel()
    }
  ];

  // Breadcrumb items
  breadcrumbs: BreadcrumbItem[] = [
    { label: 'common.home', url: '/dashboard' },
    { label: 'officeDevelopmentProjects.title', url: '/office-development-projects' },
    { label: 'officeDevelopmentProjects.addProject' }
  ];

  constructor(
    private fb: FormBuilder,
    private route: ActivatedRoute,
    private router: Router,
    private projectService: OfficeProjectService,
    private lookupService: LookupManagementService,
    private charityService: CharityService,
    private notification: NotificationService,
    private translate: TranslateService
  ) {
    this.initForm();
  }

  ngOnInit(): void {
    this.updateBreadcrumbs();
    this.loadLookupData();

    // Subscribe to language changes to update translated options
    this.langChangeSubscription = this.translate.onLangChange.subscribe((event: LangChangeEvent) => {
      this.initializeDropdownOptions();
    });

    this.route.params.subscribe((params: Params) => {
      if (params['id']) {
        this.isEditMode = true;
        this.projectId = params['id'];
        if (this.projectId) {
          this.loadProject(String(this.projectId));
        }
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

  private initializeDropdownOptions(): void {
    // Initialize beneficiary type options with translated labels
    this.beneficiaryTypeOptions = this.beneficiaryTypes.map(type => ({
      id: type,
      name: this.getBeneficiaryTypeLabel(type)
    }));

    // Initialize other options (reference to existing data)
    this.projectTypeOptions = [...this.allProjectTypes];
    this.countryOptions = [...this.allCountries];
    // Note: charityOptions is initialized in initializeCharityOptions()
  }

  private initializeCharityOptions(): void {
    this.charityOptions = this.allCharities.map((charity: any) => ({
      id: charity.id,
      name: charity.name
    }));
    console.log('[ProjectForm] Charity options initialized:', this.charityOptions.length);
  }

  private updateBreadcrumbs(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.isEditMode = true;
      this.projectId = id;
      this.breadcrumbs = [
        { label: 'common.home', url: '/dashboard' },
        { label: 'officeDevelopmentProjects.title', url: '/office-development-projects' },
        { label: 'officeDevelopmentProjects.editProject' }
      ];
    } else {
      this.breadcrumbs = [
        { label: 'common.home', url: '/dashboard' },
        { label: 'officeDevelopmentProjects.title', url: '/office-development-projects' },
        { label: 'officeDevelopmentProjects.addProject' }
      ];
    }
  }

  private initForm(): void {
    this.projectForm = this.fb.group({
      // Basic Information
      projectName: ['', Validators.required],
      projectHint: ['', Validators.required],
      projectDate: [new Date(), Validators.required],
      projectEndDate: [null, Validators.required],

      // Classification
      officeProjectTypeId: [null, Validators.required],

      // Location
      countryId: [null, Validators.required],
      regionId: [null, Validators.required],
      centerId: [null, Validators.required],
      villageName: ['', Validators.required],

      // Financial Information
      projectCostEGP: [null, [Validators.required, Validators.min(0)]],
      projectCostSAR: [null, [Validators.required, Validators.min(0)]],
      donorName: ['', Validators.required],

      // Beneficiaries
      beneficiariesCount: [null, [Validators.required, Validators.min(1)]],
      beneficiariesType: [BeneficiaryType.Families, Validators.required],

      // Charity Assignment
      assignedCharityId: [null],

      // Status
      isFinished: [false],

      // Notes
      notes: [null],

      // Attachments
      document_Attach: [[]],
      report_Attach: [[]]
    });
  }

  private loadLookupData(): void {
    const observables: Observable<any>[] = [
      this.projectService.getProjectTypes(),
      this.lookupService.getCountries({ isActive: true })
    ];

    // Load charities together with other lookup data to ensure all data is ready
    const charityObservable = this.charityService.getCharities({
      pageNumber: 1,
      pageSize: 1000
    });

    observables.push(charityObservable);

    forkJoin(observables).pipe(takeUntil(this.destroy$)).subscribe({
      next: ([projectTypes, countries, charities]) => {
        console.log('[ProjectForm] Lookup data loaded:', {
          projectTypes: projectTypes?.length || 0,
          countries: (countries as any)?.items?.length || 0,
          charities: charities?.items?.length || 0
        });

        this.allProjectTypes = projectTypes || [];
        this.allCountries = (countries as any)?.items || [];
        this.allCharities = charities?.items || [];

        // Initialize dropdown options after all data is loaded
        this.initializeDropdownOptions();
        this.initializeCharityOptions();
      },
      error: (error) => {
        console.error('Error loading lookup data:', error);
        this.notification.error(this.translate.instant('officeDevelopmentProjects.loadLookupFailed'));
      }
    });
  }

  private loadProject(id: string): void {
    this.loading = true;
    this.projectService.getProjectById(id).subscribe({
      next: (project: OfficeProject) => {
        this.patchForm(project);
        this.loadRegionsByCountry(project.countryId);
        this.loadCentersByRegion(project.regionId);
        this.loading = false;
      },
      error: (error: any) => {
        console.error('Error loading project:', error);
        this.notification.error(this.translate.instant('officeDevelopmentProjects.loadProjectFailed'));
        this.loading = false;
        this.router.navigate(['/office-development-projects']);
      }
    });
  }

  private patchForm(project: OfficeProject): void {
    this.projectForm.patchValue({
      projectName: project.projectName,
      projectHint: project.projectHint || '',
      projectDate: project.projectDate,
      projectEndDate: project.projectEndDate || null,
      officeProjectTypeId: project.officeProjectTypeId,
      countryId: project.countryId,
      regionId: project.regionId,
      centerId: project.centerId,
      villageName: project.villageName || '',
      projectCostEGP: project.projectCostEGP || null,
      projectCostSAR: project.projectCostSAR || null,
      donorName: project.donorName || '',
      beneficiariesCount: project.beneficiariesCount || null,
      beneficiariesType: project.beneficiariesType || BeneficiaryType.Families,
      assignedCharityId: project.charityId || null,
      isFinished: project.isFinished,
      notes: project.notes || null,
      document_Attach: project.document_Attach || [],
      report_Attach: project.report_Attach || []
    });

    // Load attachments
    if (project.document_Attach && project.document_Attach.length > 0) {
      this.documentAttachList = [...project.document_Attach];
    }
    if (project.report_Attach && project.report_Attach.length > 0) {
      this.reportAttachList = [...project.report_Attach];
    }

    // Load regions and centers via API based on country and region
    if (project.countryId) {
      this.loadRegionsByCountry(project.countryId);
    }
    if (project.regionId) {
      this.loadCentersByRegion(project.regionId);
    }
  }

  // Drop-down component event handlers
  onCountryDropDownChanged(value: any): void {
    console.log('[ProjectForm] Country dropdown changed:', value);
    // Fix: Check for null/undefined explicitly, not falsy values (0 is valid!)
    if (value && (value.id !== undefined && value.id !== null)) {
      // Convert to number for API call
      const countryId = typeof value.id === 'string' ? parseInt(value.id, 10) : value.id;
      console.log('[ProjectForm] Loading regions for countryId:', countryId, 'Type:', typeof countryId);

      // Update form control first to enable region dropdown
      this.projectForm.get('countryId')?.setValue(countryId, { emitEvent: false });

      // Clear dependent fields
      this.projectForm.patchValue({ regionId: null, centerId: null });
      this.filteredCenters = [];

      // Now load regions
      this.loadRegionsByCountry(countryId);
    } else {
      // Clear when country is cleared
      console.log('[ProjectForm] Country cleared, clearing regions and centers');
      this.projectForm.patchValue({ countryId: null, regionId: null, centerId: null });
      this.filteredRegions = [];
      this.filteredCenters = [];
    }
  }

  onRegionDropDownChanged(value: any): void {
    console.log('[ProjectForm] Region dropdown changed:', value);
    // Fix: Check for null/undefined explicitly, not falsy values (0 is valid!)
    if (value && (value.id !== undefined && value.id !== null)) {
      // Convert to number for API call
      const regionId = typeof value.id === 'string' ? parseInt(value.id, 10) : value.id;
      console.log('[ProjectForm] Loading centers for regionId:', regionId, 'Type:', typeof regionId);

      // Update form control first to enable center dropdown
      this.projectForm.get('regionId')?.setValue(regionId, { emitEvent: false });

      // Clear dependent field
      this.projectForm.patchValue({ centerId: null });

      // Now load centers
      this.loadCentersByRegion(regionId);
    } else {
      // Clear when region is cleared
      console.log('[ProjectForm] Region cleared, clearing centers');
      this.projectForm.patchValue({ regionId: null, centerId: null });
      this.filteredCenters = [];
    }
  }

  onProjectTypeDropDownChanged(value: any): void {
    const projectTypeControl = this.projectForm.get('officeProjectTypeId');
    if (value && value.id !== undefined && value.id !== null) {
      projectTypeControl?.setValue(value.id, { emitEvent: true });
      projectTypeControl?.markAsDirty();
      projectTypeControl?.markAsTouched();
      projectTypeControl?.updateValueAndValidity();
    } else {
      projectTypeControl?.setValue(null, { emitEvent: true });
      projectTypeControl?.updateValueAndValidity();
    }
  }

  onBeneficiaryTypeDropDownChanged(value: any): void {
    const beneficiaryTypeControl = this.projectForm.get('beneficiariesType');
    if (value && value.id !== undefined && value.id !== null) {
      beneficiaryTypeControl?.setValue(value.id, { emitEvent: true });
      beneficiaryTypeControl?.markAsDirty();
      beneficiaryTypeControl?.markAsTouched();
      beneficiaryTypeControl?.updateValueAndValidity();
    } else {
      beneficiaryTypeControl?.setValue(BeneficiaryType.Families, { emitEvent: true });
      beneficiaryTypeControl?.updateValueAndValidity();
    }
  }

  onCharityDropDownChanged(value: any): void {
    const charityControl = this.projectForm.get('assignedCharityId');
    if (value && value.id !== undefined && value.id !== null) {
      charityControl?.setValue(value.id, { emitEvent: true });
      charityControl?.markAsDirty();
      charityControl?.markAsTouched();
      charityControl?.updateValueAndValidity();
    } else {
      charityControl?.setValue(null, { emitEvent: true });
      charityControl?.updateValueAndValidity();
    }
  }

  private loadRegionsByCountry(countryId: any): void {
    if (!countryId) {
      this.filteredRegions = [];
      return;
    }

    console.log('[ProjectForm] Fetching regions for countryId:', countryId, 'Type:', typeof countryId);
    this.lookupService.getRegionsByCountry(countryId).subscribe({
      next: (regions) => {
        console.log('[ProjectForm] Regions loaded:', regions);
        // Create a new array reference to trigger change detection
        this.filteredRegions = [...(regions || [])];
        console.log('[ProjectForm] filteredRegions updated:', this.filteredRegions.length);
      },
      error: (error) => {
        console.error('[ProjectForm] Error loading regions:', error);
        this.filteredRegions = [];
      }
    });
  }

  private loadCentersByRegion(regionId: any): void {
    if (!regionId) {
      this.filteredCenters = [];
      return;
    }

    console.log('[ProjectForm] Fetching centers for regionId:', regionId, 'Type:', typeof regionId);
    this.lookupService.getCentersByRegion(regionId).subscribe({
      next: (centers) => {
        console.log('[ProjectForm] Centers loaded:', centers);
        // Create a new array reference to trigger change detection
        this.filteredCenters = [...(centers || [])];
        console.log('[ProjectForm] filteredCenters updated:', this.filteredCenters.length);
      },
      error: (error) => {
        console.error('[ProjectForm] Error loading centers:', error);
        this.filteredCenters = [];
      }
    });
  }

  // Attachment handlers
  onDocumentAttachmentChange(attachments: AttachmentDto[]): void {
    this.documentAttachList = attachments;
  }

  onReportAttachmentChange(attachments: AttachmentDto[]): void {
    this.reportAttachList = attachments;
  }

  // Helper method to transform attachments for backend
  private transformAttachmentsForBackend(attachments: AttachmentDto[]): any[] {
    if (!attachments || attachments.length === 0) {
      return [];
    }

    return attachments
      .filter(a => !a.isDeleted)
      .map(attachment => {
        // For new attachments with temp IDs, send only the necessary fields
        if (attachment.isNew || (attachment.id && attachment.id.startsWith('temp_'))) {
          return {
            // No `id` key at all: the backend's AttachmentDto.Id is a non-nullable Guid and the
            // JSON binder rejects `null` for it (400 on the whole request). A missing property
            // binds to Guid.Empty; the service assigns the real id on save.
            fileName: attachment.fileName,
            contentType: attachment.contentType,
            size: attachment.size,
            extension: attachment.extension,
            // Extract base64 content from data URL if present
            fileData: attachment.fileData && attachment.fileData.startsWith('data:')
              ? attachment.fileData.split(',')[1] // Get base64 part after comma
              : attachment.fileData,
            isNew: true // Explicit boolean
          };
        }
        // For existing attachments, send as-is but ensure proper types
        const existing: any = {
          fileName: attachment.fileName,
          contentType: attachment.contentType,
          size: attachment.size,
          extension: attachment.extension,
          filePath: attachment.filePath,
          isNew: false // Explicit boolean for existing attachments
        };
        // Include the id only when it is a real server id — never `null`.
        if (attachment.id && !attachment.id.startsWith('temp_')) {
          existing.id = attachment.id;
        }
        return existing;
      });
  }

  onSubmit(): void {
    if (this.projectForm.invalid) {
      this.markFormGroupTouched(this.projectForm);
      this.notification.error(this.translate.instant('officeDevelopmentProjects.fixValidationErrors'));
      return;
    }

    this.saving = true;
    const formValue = this.projectForm.value;

    // Transform attachments before sending to backend
    const transformedDocuments = this.transformAttachmentsForBackend(this.documentAttachList);
    const transformedReports = this.transformAttachmentsForBackend(this.reportAttachList);

    console.log('[ProjectForm] Submitting with attachments:', {
      documents: transformedDocuments,
      reports: transformedReports
    });

    const projectDto: OfficeProjectDto = {
      id: this.isEditMode ? String(this.projectId!) : undefined,
      projectName: formValue.projectName,
      projectHint: formValue.projectHint || undefined,
      projectDate: formValue.projectDate,
      projectEndDate: formValue.projectEndDate || undefined,
      officeProjectTypeId: formValue.officeProjectTypeId,
      countryId: formValue.countryId,
      regionId: formValue.regionId,
      centerId: formValue.centerId,
      villageName: formValue.villageName || undefined,
      projectCostEGP: formValue.projectCostEGP || undefined,
      projectCostSAR: formValue.projectCostSAR || undefined,
      donorName: formValue.donorName || undefined,
      beneficiariesCount: formValue.beneficiariesCount || undefined,
      beneficiariesType: formValue.beneficiariesType,
      charityId: formValue.assignedCharityId || undefined,
      isFinished: formValue.isFinished || false,
      notes: formValue.notes || undefined,
      // Send transformed attachment lists to backend
      document_Attach: transformedDocuments,
      report_Attach: transformedReports
    };

    const operation = this.isEditMode
      ? this.projectService.updateProject(this.projectId!, projectDto)
      : this.projectService.createProject(projectDto);

    operation.subscribe({
      next: (project: OfficeProject) => {
        this.notification.success(
          this.isEditMode
            ? this.translate.instant('officeDevelopmentProjects.projectUpdated')
            : this.translate.instant('officeDevelopmentProjects.projectCreated')
        );
        this.saving = false;
        this.router.navigate(['/office-development-projects', project.id]);
      },
      error: (error: any) => {
        console.error('Error saving project:', error);
        this.notification.error(
          this.isEditMode
            ? this.translate.instant('officeDevelopmentProjects.updateFailed')
            : this.translate.instant('officeDevelopmentProjects.createFailed')
        );
        this.saving = false;
      }
    });
  }

  onCancel(): void {
    if (this.isEditMode && this.projectId) {
      this.router.navigate(['/office-development-projects', this.projectId]);
    } else {
      this.router.navigate(['/office-development-projects']);
    }
  }

  private markFormGroupTouched(formGroup: FormGroup): void {
    Object.keys(formGroup.controls).forEach(key => {
      const control = formGroup.get(key);
      control?.markAsTouched();

      if (control instanceof FormGroup) {
        this.markFormGroupTouched(control);
      }
    });
  }

  isFieldValid(fieldName: string): boolean {
    const field = this.projectForm.get(fieldName);
    return field ? field.valid && (field.dirty || field.touched) : false;
  }

  isFieldInvalid(fieldName: string): boolean {
    const field = this.projectForm.get(fieldName);
    return field ? field.invalid && (field.dirty || field.touched) : false;
  }

  getErrorMessage(fieldName: string): string {
    const field = this.projectForm.get(fieldName);
    if (!field || !field.errors) return '';

    const fieldLabel = this.translate.instant(`officeDevelopmentProjects.${fieldName}`);

    if (field.errors['required']) {
      return this.translate.instant('validation.required', { field: fieldLabel });
    }
    if (field.errors['email']) {
      return this.translate.instant('validation.email');
    }
    if (field.errors['pattern']) {
      return this.translate.instant('validation.pattern');
    }
    if (field.errors['minlength']) {
      return this.translate.instant('validation.minLength', { minLength: field.errors['minlength'].requiredLength });
    }
    if (field.errors['min']) {
      return this.translate.instant('validation.min', { min: field.errors['min'].min });
    }

    return this.translate.instant('validation.invalid');
  }

  getBeneficiaryTypeLabel(type: BeneficiaryType): string {
    const key = `officeDevelopmentProjects.beneficiaryType${type}`;
    return this.translate.instant(key);
  }
}
