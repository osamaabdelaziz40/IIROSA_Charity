/**
 * Mission Register Component (epic 15, UC-MSN-09 — §20.S.3)
 * تسجيل نتيجة المأمورية: the actor reviews the mission's read-only context, edits
 * the finding fields, and records the completion outcome + السبب.
 * One POST /api/MissionManagement/{id}/event; a second registration is refused
 * server-side (a completed mission is immutable).
 */

import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';

import { MissionService } from '../services/mission.service';
import { MissionDetail, RegisterMissionResultRequest } from '../models/mission.model';
import { UserManagementService } from '../../user-management/services/user-management.service';
import { NotificationService } from '../../../core/services/notification.service';
import { BreadcrumbComponent, BreadcrumbItem } from '../../../shared/components';
import { SharedModule } from '../../../shared/shared.module';

/**
 * Shared-dropdown option for الموظف المسئول — label is the display text, value is
 * the user id. The register form must use the SAME source as the create form
 * (/api/usermanagement): assignedToUserId stores an ApplicationUser id, so employee
 * records neither populate the picker nor echo the saved selection.
 */
interface DropdownOption {
  id: string;
  name: string;
}

@Component({
  selector: 'app-mission-register',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, TranslateModule, SharedModule, BreadcrumbComponent],
  templateUrl: './mission-register.component.html',
  styleUrls: ['./mission-register.component.scss']
})
export class MissionRegisterComponent implements OnInit, OnDestroy {
  private destroy$ = new Subject<void>();

  // Breadcrumb items
  breadcrumbs: BreadcrumbItem[] = [
    { label: 'common.home', url: '/dashboard' },
    { label: 'missions.title', url: '/missions' },
    { label: 'missions.registerResult' }
  ];

  mission: MissionDetail | null = null;
  userOptions: DropdownOption[] = [];
  loading = false;
  saving = false;
  error: string | null = null;

  // The two §20.S.3 completion checkboxes — mutually exclusive (checking one
  // unchecks and disables the other); together they form the wire's tri-state.
  isCompleted = false;
  isNotCompleted = false;

  registerForm: FormGroup;

  constructor(
    private fb: FormBuilder,
    private missionService: MissionService,
    private userManagementService: UserManagementService,
    private route: ActivatedRoute,
    private router: Router,
    private translate: TranslateService,
    private notification: NotificationService
  ) {
    this.registerForm = this.fb.group({
      entityName: ['', Validators.required],        // الجهة المنظمة
      conferenceName: ['', Validators.required],    // اسم المؤتمر
      details: ['', Validators.required],           // المهمه
      missionTarget: ['', Validators.required],     // الهدف
      missionDetails: ['', Validators.required],    // التفاصيل
      missionLocation: ['', Validators.required],   // الموقع
      assignedToUserId: [null, Validators.required],// الموظف المسئول
      village: ['', Validators.required],           // القرية/الحي
      reason: ['', Validators.required]             // السبب
    });
  }

  ngOnInit(): void {
    this.loadUsers();
    this.loadMission();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  /**
   * Load the mission whose result is being registered
   */
  private loadMission(): void {
    const id = this.route.snapshot.params['id'];
    if (!id) {
      this.error = this.translate.instant('errors.notFound');
      return;
    }

    this.loading = true;

    this.missionService.getMissionById(id)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (mission) => {
          this.mission = mission;

          // A result has already been registered — either outcome makes the mission
          // immutable (a not-completed entry counts too). Send the actor to the detail
          // screen instead of an unusable form, and clear the loading flag on the way out.
          if (mission.isMissionCompleted || mission.missionCompletedTxt) {
            this.loading = false;
            this.notification.info(this.translate.instant('missions.resultAlreadyRegistered'));
            this.router.navigate(['/missions', mission.id]);
            return;
          }

          this.registerForm.patchValue({
            entityName: mission.entityName || '',
            conferenceName: mission.conferenceName || '',
            details: mission.details || '',
            missionTarget: mission.missionTarget || '',
            missionDetails: mission.missionDetails || '',
            missionLocation: mission.missionLocation || '',
            assignedToUserId: mission.assignedToUserId ?? null,
            village: mission.village || ''
          });
          this.loading = false;
        },
        error: () => {
          this.error = this.translate.instant('errors.serverError');
          this.loading = false;
        }
      });
  }

  /**
   * الموظف المسئول options — every user in the database, exactly like the create
   * form (/api/usermanagement). Employee records are the wrong source: their ids
   * never match the ApplicationUser id stored in assignedToUserId, so the saved
   * selection cannot be echoed (and an empty employee table empties the picker).
   */
  private loadUsers(): void {
    this.userManagementService.getUsers({ page: 1, pageSize: 1000 })
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (result) => {
          this.userOptions = (result.items || []).map(user => ({
            id: user.id,
            name: user.fullName || user.userName || user.email
          }));
        },
        error: () => this.notification.error(this.translate.instant('missions.catalogueLoadFailed'))
      });
  }

  // ========== §20.S.3 checkbox handlers (mutual exclusion) ==========

  /**
   * اتمام المامورية checked → uncheck and disable the not-completed box
   */
  onCompletedChange(checked: boolean): void {
    this.isCompleted = checked;
    if (checked) {
      this.isNotCompleted = false;
    }
  }

  /**
   * Not-completed checked → uncheck and disable the completed box
   */
  onNotCompletedChange(checked: boolean): void {
    this.isNotCompleted = checked;
    if (checked) {
      this.isCompleted = false;
    }
  }

  /**
   * Submit — POST {id}/event with the editable fields + the outcome tri-state + السبب
   */
  onSubmit(): void {
    if (!this.mission) return;

    if (this.registerForm.invalid) {
      this.markFormGroupTouched(this.registerForm);
      this.notification.error(this.translate.instant('missions.fixValidationErrors'));
      return;
    }

    // The outcome must be explicit — one of the two boxes has to be checked
    if (!this.isCompleted && !this.isNotCompleted) {
      this.notification.error(this.translate.instant('missions.completionRequired'));
      return;
    }

    this.saving = true;
    const formValue = this.registerForm.value;

    const request: RegisterMissionResultRequest = {
      entityName: formValue.entityName,
      conferenceName: formValue.conferenceName,
      details: formValue.details,
      missionTarget: formValue.missionTarget,
      missionDetails: formValue.missionDetails,
      missionLocation: formValue.missionLocation,
      assignedToUserId: formValue.assignedToUserId,
      village: formValue.village,
      isCompleted: this.isCompleted,
      reason: formValue.reason
    };

    this.missionService.registerMissionResult(this.mission.id, request)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: () => {
          this.notification.success(this.translate.instant('missions.resultRegistered'));
          this.saving = false;
          this.router.navigate(['/missions']);
        },
        error: (httpError) => {
          this.saving = false;
          this.handleSaveError(httpError);
        }
      });
  }

  /**
   * Surface the server's field→messages map (FluentValidation errors dictionary)
   */
  private handleSaveError(httpError: any): void {
    const errors = httpError?.error?.errors;
    if (errors && typeof errors === 'object') {
      for (const [field, messages] of Object.entries<any>(errors)) {
        const controlName = field.charAt(0).toLowerCase() + field.slice(1);
        const control = this.registerForm.get(controlName);
        const message = Array.isArray(messages) ? messages.join(' · ') : String(messages);

        if (control) {
          // 'server' carries the message itself — the shared app-drop-down reads
          // errors['server'] verbatim ({server: true} renders as literal "true").
          control.setErrors({ server: message });
          control.markAsTouched();
        }
      }
      this.notification.error(httpError?.error?.message || this.translate.instant('missions.registerFailed'));
    } else {
      this.notification.error(httpError?.error?.message || this.translate.instant('missions.registerFailed'));
    }
  }

  /**
   * Cancel — back to the mission detail
   */
  onCancel(): void {
    if (this.mission) {
      this.router.navigate(['/missions', this.mission.id]);
    } else {
      this.router.navigate(['/missions']);
    }
  }

  /**
   * Check if field is invalid
   */
  isFieldInvalid(fieldName: string): boolean {
    const field = this.registerForm.get(fieldName);
    return field ? field.invalid && (field.dirty || field.touched) : false;
  }

  /**
   * Get field error message
   */
  getFieldError(fieldName: string): string {
    const field = this.registerForm.get(fieldName);
    if (!field || !field.errors) return '';

    if (field.errors['server']) {
      return field.errors['server'] || this.translate.instant('validation.invalid');
    }
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
}
