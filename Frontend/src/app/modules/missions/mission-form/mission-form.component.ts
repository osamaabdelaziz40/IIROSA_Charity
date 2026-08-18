/**
 * Mission Form Component
 * Create or edit mission form with validation
 * Implements UC-8.1 (Create Mission) and UC-8.7 (Update Mission Details)
 */

import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';

import { MissionService } from '../services/mission.service';
import {
  Mission,
  CreateMissionRequest,
  UpdateMissionRequest
} from '../models/mission.model';
import { PageHeaderComponent } from '../../../shared/components/page-header/page-header.component';
import { BreadcrumbComponent, BreadcrumbItem } from '../../../shared/components';

// Lookup data interfaces (simplified)
interface LookupItem {
  id: number;
  name: string;
}

@Component({
  selector: 'app-mission-form',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, TranslateModule, PageHeaderComponent, BreadcrumbComponent],
  templateUrl: './mission-form.component.html',
  styleUrls: ['./mission-form.component.scss']
})
export class MissionFormComponent implements OnInit, OnDestroy {
  private destroy$ = new Subject<void>();

  // Breadcrumb items
  breadcrumbs: BreadcrumbItem[] = [
    { label: 'common.home', url: '/dashboard' },
    { label: 'missions.title', url: '/missions' }
  ];

  get breadcrumbsWithAction(): BreadcrumbItem[] {
    return [
      ...this.breadcrumbs,
      { label: this.isEditMode ? 'missions.editMission' : 'missions.addMission' }
    ];
  }

  missionForm: FormGroup;
  mission: Mission | null = null;
  loading = false;
  saving = false;
  isEditMode = false;
  error: string | null = null;

  // Lookup data
  missionTypes: LookupItem[] = [];
  missionTimeTypes: LookupItem[] = [];
  countries: LookupItem[] = [];
  regions: LookupItem[] = [];
  centers: LookupItem[] = [];
  users: LookupItem[] = [];

  // Filtered lookup data
  filteredRegions: LookupItem[] = [];
  filteredCenters: LookupItem[] = [];

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
    private missionService: MissionService,
    private route: ActivatedRoute,
    private router: Router,
    private translate: TranslateService
  ) {
    this.missionForm = this.createForm();
  }

  ngOnInit(): void {
    this.loadLookupData();
    this.checkEditMode();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  /**
   * Create reactive form
   */
  private createForm(): FormGroup {
    return this.fb.group({
      missionTarget: ['', Validators.required],
      missionDetails: [''],
      details: [''],
      missionTypeId: [null, Validators.required],
      missionTimeTypeId: [null, Validators.required],
      missionDate: ['', Validators.required],
      countryId: [null],
      regionId: [null],
      centerId: [null],
      missionLocation: [''],
      village: [''],
      assignedTo: [null, Validators.required],
      entityName: [''],
      conferenceName: ['']
    });
  }

  /**
   * Check if edit mode
   */
  private checkEditMode(): void {
    const id = this.route.snapshot.params['id'];
    if (id) {
      this.isEditMode = true;
      this.loadMission(id);
    }
  }

  /**
   * Load mission for editing
   */
  private loadMission(id: string): void {
    this.loading = true;

    this.missionService.getMissionById(id)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (mission) => {
          this.mission = mission;
          this.populateForm(mission);
          this.loading = false;
        },
        error: () => {
          this.error = this.translate.instant('errors.serverError');
          this.loading = false;
        }
      });
  }

  /**
   * Populate form with mission data
   */
  private populateForm(mission: Mission): void {
    this.missionForm.patchValue({
      missionTarget: mission.missionTarget,
      missionDetails: mission.missionDetails || '',
      details: mission.details || '',
      missionTypeId: mission.missionTypeId,
      missionTimeTypeId: mission.missionTimeTypeId,
      missionDate: mission.missionDate.split('T')[0], // Format for date input
      countryId: mission.countryId || null,
      regionId: mission.regionId || null,
      centerId: mission.centerId || null,
      missionLocation: mission.missionLocation || '',
      village: mission.village || '',
      assignedTo: mission.assignedTo,
      entityName: mission.entityName || '',
      conferenceName: mission.conferenceName || ''
    });

    // Filter regions and centers based on selected country/region
    if (mission.countryId) {
      this.onCountryChange();
    }
    if (mission.regionId) {
      this.onRegionChange();
    }
  }

  /**
   * Load lookup data
   */
  private loadLookupData(): void {
    // TODO: Load from actual lookup services
    // For now, using static data as placeholders

    this.missionTypes = [
      { id: 1, name: this.translate.instant('missions.missionTypeFieldwork') },
      { id: 2, name: this.translate.instant('missions.missionTypeConference') },
      { id: 3, name: this.translate.instant('missions.missionTypeTraining') },
      { id: 4, name: this.translate.instant('missions.missionTypeMeeting') },
      { id: 5, name: this.translate.instant('missions.missionTypeInspection') },
      { id: 6, name: this.translate.instant('missions.missionTypeOther') }
    ];

    this.missionTimeTypes = [
      { id: 1, name: this.translate.instant('missions.missionTimeTypeOneTime') },
      { id: 2, name: this.translate.instant('missions.missionTimeTypeDaily') },
      { id: 3, name: this.translate.instant('missions.missionTimeTypeWeekly') },
      { id: 4, name: this.translate.instant('missions.missionTimeTypeMonthly') },
      { id: 5, name: this.translate.instant('missions.missionTimeTypeQuarterly') },
      { id: 6, name: this.translate.instant('missions.missionTimeTypeAnnually') }
    ];

    // TODO: Load from LookupManagementService
    this.countries = [];
    this.regions = [];
    this.centers = [];

    // TODO: Load from UserService
    this.users = [];
  }

  /**
   * Handle country change
   */
  onCountryChange(): void {
    const countryId = this.missionForm.get('countryId')?.value;
    if (countryId) {
      this.filteredRegions = this.regions.filter(r => r.id === countryId); // Placeholder logic
      this.missionForm.get('regionId')?.setValue(null);
      this.missionForm.get('centerId')?.setValue(null);
      this.filteredCenters = [];
    } else {
      this.filteredRegions = [];
      this.filteredCenters = [];
    }
  }

  /**
   * Handle region change
   */
  onRegionChange(): void {
    const regionId = this.missionForm.get('regionId')?.value;
    if (regionId) {
      this.filteredCenters = this.centers.filter(c => c.id === regionId); // Placeholder logic
      this.missionForm.get('centerId')?.setValue(null);
    } else {
      this.filteredCenters = [];
    }
  }

  /**
   * Submit form
   */
  onSubmit(): void {
    if (this.missionForm.invalid) {
      this.markFormGroupTouched(this.missionForm);
      return;
    }

    this.saving = true;

    const formValue = this.missionForm.value;

    if (this.isEditMode && this.mission) {
      const request: UpdateMissionRequest = {
        missionTarget: formValue.missionTarget,
        missionDetails: formValue.missionDetails || undefined,
        details: formValue.details || undefined,
        missionTypeId: formValue.missionTypeId,
        missionTimeTypeId: formValue.missionTimeTypeId,
        missionDate: formValue.missionDate,
        countryId: formValue.countryId || undefined,
        regionId: formValue.regionId || undefined,
        centerId: formValue.centerId || undefined,
        missionLocation: formValue.missionLocation || undefined,
        village: formValue.village || undefined,
        assignedTo: formValue.assignedTo,
        entityName: formValue.entityName || undefined,
        conferenceName: formValue.conferenceName || undefined
      };

      this.missionService.updateMission(this.mission.id, request)
        .pipe(takeUntil(this.destroy$))
        .subscribe({
          next: () => {
            this.saving = false;
            this.router.navigate(['/missions', this.mission!.id]);
          },
          error: () => {
            this.saving = false;
          }
        });
    } else {
      const request: CreateMissionRequest = {
        missionTarget: formValue.missionTarget,
        missionDetails: formValue.missionDetails || undefined,
        details: formValue.details || undefined,
        missionTypeId: formValue.missionTypeId,
        missionTimeTypeId: formValue.missionTimeTypeId,
        missionDate: formValue.missionDate,
        countryId: formValue.countryId || undefined,
        regionId: formValue.regionId || undefined,
        centerId: formValue.centerId || undefined,
        missionLocation: formValue.missionLocation || undefined,
        village: formValue.village || undefined,
        assignedTo: formValue.assignedTo,
        entityName: formValue.entityName || undefined,
        conferenceName: formValue.conferenceName || undefined
      };

      this.missionService.createMission(request)
        .pipe(takeUntil(this.destroy$))
        .subscribe({
          next: () => {
            this.saving = false;
            this.router.navigate(['/missions']);
          },
          error: () => {
            this.saving = false;
          }
        });
    }
  }

  /**
   * Cancel and go back
   */
  onCancel(): void {
    if (this.isEditMode && this.mission) {
      this.router.navigate(['/missions', this.mission.id]);
    } else {
      this.router.navigate(['/missions']);
    }
  }

  /**
   * Check if field is invalid
   */
  isFieldInvalid(fieldName: string): boolean {
    const field = this.missionForm.get(fieldName);
    return field ? field.invalid && (field.dirty || field.touched) : false;
  }

  /**
   * Get field error message
   */
  getFieldError(fieldName: string): string {
    const field = this.missionForm.get(fieldName);
    if (!field || !field.errors) return '';

    if (field.errors['required']) {
      return this.translate.instant('validation.required');
    }

    return '';
  }

  /**
   * Mark all fields as touched
   */
  private markFormGroupTouched(formGroup: FormGroup): void {
    Object.keys(formGroup.controls).forEach(key => {
      const control = formGroup.get(key);
      control?.markAsTouched();

      if (control instanceof FormGroup) {
        this.markFormGroupTouched(control);
      }
    });
  }

  /**
   * Get page title
   */
  get pageTitle(): string {
    return this.isEditMode
      ? this.translate.instant('missions.editMission')
      : this.translate.instant('missions.addMission');
  }
}
