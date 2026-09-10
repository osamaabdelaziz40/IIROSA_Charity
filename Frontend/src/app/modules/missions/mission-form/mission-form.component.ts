/**
 * Mission Form Component (epic 15, UC-MSN-06/07 — §20.S.2)
 * Create/update a mission. All catalogues load from real endpoints: mission types
 * (15-3), interview types (15-4), time types (15-5), countries/regions/centers
 * (LookupManagement), all users (الموظف المسئول — /api/usermanagement).
 * Every select is the shared app-drop-down (Select2), matching the
 * office-development-projects form; catalogue labels are language-aware
 * (nameAr in Arabic mode, nameEn in English mode).
 */

import { Component, OnInit, OnDestroy, ViewChildren, QueryList } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';

import { MissionService } from '../services/mission.service';
import {
  MissionDetail,
  CreateMissionRequest,
  UpdateMissionRequest,
  MissionLookupItem
} from '../models/mission.model';
import { LookupManagementService } from '../../lookup-management/services/lookup-management.service';
import { CountryDto, RegionDto, CenterDto } from '../../lookup-management/models/lookup.model';
import { UserManagementService } from '../../user-management/services/user-management.service';
import { NotificationService } from '../../../core/services/notification.service';
import { PageHeaderComponent } from '../../../shared/components/page-header/page-header.component';
import { BreadcrumbComponent, BreadcrumbItem, CollapsibleCardComponent } from '../../../shared/components';
import { SharedModule } from '../../../shared/shared.module';
import type { PageAction } from '../../../shared/components/page-header/page-header.component';

/** Shared-dropdown option — label is the display text, value is the id (lookup int or user Guid) */
interface DropdownOption {
  id: any;
  name: string;
}

@Component({
  selector: 'app-mission-form',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, TranslateModule, SharedModule, PageHeaderComponent, BreadcrumbComponent],
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
  mission: MissionDetail | null = null;
  loading = false;
  saving = false;
  isEditMode = false;
  error: string | null = null;

  /** Collapsible §20.S.2 section cards — expanded on a failed submit to reveal
   *  the red fields hidden inside collapsed cards. */
  @ViewChildren(CollapsibleCardComponent) collapsibleCards?: QueryList<CollapsibleCardComponent>;

  // Catalogues (§20.S.2) — options for the shared dropdowns; catalogue labels
  // are resolved per current language at load time (the app hard-reloads on
  // language switch, so no re-map subscription is needed)
  missionTypeOptions: DropdownOption[] = [];
  missionTimeTypeOptions: DropdownOption[] = [];
  missionInterviewTypeOptions: DropdownOption[] = [];
  countries: CountryDto[] = [];
  regions: RegionDto[] = [];
  centers: CenterDto[] = [];
  userOptions: DropdownOption[] = [];

  // Page actions for header
  pageActions: PageAction[] = [
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
    private lookupService: LookupManagementService,
    private userManagementService: UserManagementService,
    private route: ActivatedRoute,
    private router: Router,
    private translate: TranslateService,
    private notification: NotificationService
  ) {
    this.missionForm = this.createForm();
  }

  ngOnInit(): void {
    this.loadCatalogues();
    this.checkEditMode();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  /**
   * §20.S.2 mandatory flags: كل الحقول اجباريه — every field carries Validators.required;
   * the server validator re-checks (the browser is not the control).
   */
  private createForm(): FormGroup {
    return this.fb.group({
      missionTarget: ['', Validators.required],          // الهدف
      missionDetails: ['', Validators.required],         // التفاصيل
      details: ['', Validators.required],                // المهمه
      missionTypeId: [null, Validators.required],        // نوع المأموريه
      missionTimeTypeId: [null, Validators.required],    // نوع التوقيت
      missionInterviewTypeId: [null, Validators.required], // نوع المقابلة
      missionDate: ['', Validators.required],            // التاريخ
      countryId: [null, Validators.required],            // الدولة
      regionId: [null, Validators.required],             // المنطقة
      centerId: [null, Validators.required],             // المركز
      missionLocation: ['', Validators.required],        // الموقع
      village: ['', Validators.required],                // الحي
      entityName: ['', Validators.required],             // اسم الجهة المنظمة
      conferenceName: ['', Validators.required],         // اسم المؤتمر
      assignedToUserId: [null, Validators.required]      // الموظف المسئول
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
   * Load the mission being edited
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
   * Populate the form from the detail DTO, then rehydrate the cascade selects
   * (regions for the mission's country, centers for its region).
   */
  private populateForm(mission: MissionDetail): void {
    this.missionForm.patchValue({
      missionTarget: mission.missionTarget,
      missionDetails: mission.missionDetails || '',
      details: mission.details || '',
      missionTypeId: mission.missionTypeId ?? null,
      missionTimeTypeId: mission.missionTimeTypeId ?? null,
      missionInterviewTypeId: mission.missionInterviewTypeId ?? null,
      missionDate: mission.missionDate.split('T')[0],
      missionLocation: mission.missionLocation || '',
      village: mission.village || '',
      entityName: mission.entityName || '',
      conferenceName: mission.conferenceName || '',
      assignedToUserId: mission.assignedToUserId ?? null,
      countryId: mission.countryId ?? null
    });

    // Cascade: regions need the country loaded first, then patch region → load centers
    if (mission.countryId) {
      this.lookupService.getRegionsByCountry(mission.countryId)
        .pipe(takeUntil(this.destroy$))
        .subscribe({
          next: regions => {
            this.regions = regions || [];
            this.missionForm.get('regionId')?.setValue(mission.regionId ?? null);

            if (mission.regionId) {
              this.lookupService.getCentersByRegion(mission.regionId)
                .pipe(takeUntil(this.destroy$))
                .subscribe({
                  next: centers => {
                    this.centers = centers || [];
                    this.missionForm.get('centerId')?.setValue(mission.centerId ?? null);
                  },
                  error: () => this.handleCatalogueError()
                });
            }
          },
          error: () => this.handleCatalogueError()
        });
    }
  }

  /**
   * Bilingual catalogue label — nameAr in Arabic mode, nameEn in English mode,
   * falling back to the raw name then the opposite-language name (never blank).
   */
  private localizedCatalogLabel(item: MissionLookupItem): string {
    const arabic = this.translate.currentLang === 'ar';
    const primary = arabic ? item.nameAr : item.nameEn;
    const opposite = arabic ? item.nameEn : item.nameAr;
    return primary || item.name || opposite || '';
  }

  /**
   * Load every §20.S.2 catalogue from its real endpoint
   */
  private loadCatalogues(): void {
    // نوع المأموريه (15-3) / نوع التوقيت (15-5)
    this.missionService.getMissionTypes()
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (items) => (this.missionTypeOptions = (items || []).map(item => ({ id: item.id, name: this.localizedCatalogLabel(item) }))),
        error: () => this.handleCatalogueError()
      });

    this.missionService.getMissionTimeTypes()
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (items) => (this.missionTimeTypeOptions = (items || []).map(item => ({ id: item.id, name: this.localizedCatalogLabel(item) }))),
        error: () => this.handleCatalogueError()
      });

    // نوع المقابلة (15-4)
    this.missionService.getMissionInterviewTypes()
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (items) => (this.missionInterviewTypeOptions = (items || []).map(item => ({ id: item.id, name: this.localizedCatalogLabel(item) }))),
        error: () => this.handleCatalogueError()
      });

    // الدول — regions and centers load on cascade
    this.lookupService.getCountries({ isActive: true, page: 1, pageSize: 1000 })
      .pipe(takeUntil(this.destroy$))
      .subscribe({ next: (result) => (this.countries = result.items || []), error: () => this.handleCatalogueError() });

    // الموظف المسئول — every user in the database, not just employee records
    this.userManagementService.getUsers({ page: 1, pageSize: 1000 })
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (result) => {
          this.userOptions = (result.items || []).map(user => ({
            id: user.id,
            name: user.fullName || user.userName || user.email
          }));
        },
        error: () => this.handleCatalogueError()
      });
  }

  /**
   * A failed catalogue load must not pass silently — the actor would face an empty
   * dropdown with no explanation.
   */
  private handleCatalogueError(): void {
    this.notification.error(this.translate.instant('missions.catalogueLoadFailed'));
  }

  /**
   * Country changed → reload regions, reset the region/center cascade.
   * The shared dropdown emits the selected option object (or null when cleared)
   * and has already written the id to the control.
   */
  onCountryChanged(value: DropdownOption | null): void {
    const countryId = value && value.id != null ? value.id : null;

    if (!countryId) {
      this.missionForm.get('countryId')?.setValue(null);
    }

    this.regions = [];
    this.centers = [];
    this.missionForm.get('regionId')?.setValue(null);
    this.missionForm.get('centerId')?.setValue(null);

    if (countryId) {
      this.lookupService.getRegionsByCountry(countryId)
        .pipe(takeUntil(this.destroy$))
        .subscribe({ next: (regions) => (this.regions = regions || []), error: () => this.handleCatalogueError() });
    }
  }

  /**
   * Region changed → reload centers, reset center
   */
  onRegionChanged(value: DropdownOption | null): void {
    const regionId = value && value.id != null ? value.id : null;

    if (!regionId) {
      this.missionForm.get('regionId')?.setValue(null);
    }

    this.centers = [];
    this.missionForm.get('centerId')?.setValue(null);

    if (regionId) {
      this.lookupService.getCentersByRegion(regionId)
        .pipe(takeUntil(this.destroy$))
        .subscribe({ next: (centers) => (this.centers = centers || []), error: () => this.handleCatalogueError() });
    }
  }

  /**
   * Submit — create or update with the clean wire keys
   */
  onSubmit(): void {
    if (this.missionForm.invalid) {
      this.markFormGroupTouched(this.missionForm);
      // Reveal collapsed sections — otherwise the actor sees only the error
      // toast while the red fields stay hidden.
      this.collapsibleCards?.forEach(card => card.open());
      this.notification.error(this.translate.instant('missions.fixValidationErrors'));
      return;
    }

    this.saving = true;
    const formValue = this.missionForm.value;

    const request: CreateMissionRequest = {
      missionTarget: formValue.missionTarget,
      missionDetails: formValue.missionDetails,
      details: formValue.details,
      missionTypeId: formValue.missionTypeId,
      missionTimeTypeId: formValue.missionTimeTypeId,
      missionInterviewTypeId: formValue.missionInterviewTypeId,
      missionDate: formValue.missionDate,
      countryId: formValue.countryId,
      regionId: formValue.regionId,
      centerId: formValue.centerId,
      missionLocation: formValue.missionLocation,
      village: formValue.village,
      entityName: formValue.entityName,
      conferenceName: formValue.conferenceName,
      assignedToUserId: formValue.assignedToUserId
    };

    if (this.isEditMode && this.mission) {
      this.missionService.updateMission(this.mission.id, request as UpdateMissionRequest)
        .pipe(takeUntil(this.destroy$))
        .subscribe({
          next: () => {
            this.notification.success(this.translate.instant('missions.missionUpdated'));
            this.saving = false;
            this.router.navigate(['/missions', this.mission!.id]);
          },
          error: (httpError) => this.handleSaveError(httpError, 'missions.updateFailed')
        });
    } else {
      this.missionService.createMission(request)
        .pipe(takeUntil(this.destroy$))
        .subscribe({
          next: () => {
            this.notification.success(this.translate.instant('missions.missionCreated'));
            this.saving = false;
            this.router.navigate(['/missions']);
          },
          error: (httpError) => this.handleSaveError(httpError, 'missions.createFailed')
        });
    }
  }

  /**
   * Surface the server's field→messages map (FluentValidation via the controller's
   * BadRequest(new { message, errors }) shape): flag each named control and toast
   * the summary message.
   */
  private handleSaveError(httpError: any, fallbackKey: string): void {
    this.saving = false;

    const errors = httpError?.error?.errors;
    if (errors && typeof errors === 'object') {
      for (const [field, messages] of Object.entries<any>(errors)) {
        // Server keys are PascalCase DTO names ("MissionDate") — controls are camelCase
        const controlName = field.charAt(0).toLowerCase() + field.slice(1);
        const control = this.missionForm.get(controlName);
        const message = Array.isArray(messages) ? messages.join(' · ') : String(messages);

        if (control) {
          // 'server' carries the message itself — the shared app-drop-down reads
          // errors['server'] verbatim ({server: true} renders as literal "true").
          control.setErrors({ server: message });
          control.markAsTouched();
        }
      }
      this.notification.error(httpError?.error?.message || this.translate.instant(fallbackKey));
    } else {
      this.notification.error(httpError?.error?.message || this.translate.instant(fallbackKey));
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
