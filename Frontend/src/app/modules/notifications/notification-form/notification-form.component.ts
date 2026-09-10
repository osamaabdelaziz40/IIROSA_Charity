/**
 * Notification Form Component (UC-NTF push / edit — add & resend screen)
 * Compose a web notification: Title, Message, and the audience — specific users
 * (multi-select when isUser) and/or whole charities (multi-select when
 * isCharity). Create pushes live over SignalR immediately; edit stores the
 * change and offers a Resend that re-pushes the stored content.
 */

import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';

import { NotificationLogService } from '../services/notification-log.service';
import { NotificationsLog } from '../models/notification.model';
import { UserManagementService } from '../../user-management/services/user-management.service';
import { CharityService } from '../../charities/services/charity.service';
import { NotificationService } from '../../../core/services/notification.service';
import { PageHeaderComponent, BreadcrumbComponent, BreadcrumbItem, CollapsibleCardComponent } from '../../../shared/components';
import { SharedModule } from '../../../shared/shared.module';
import type { PageAction } from '../../../shared/components/page-header/page-header.component';

/** Shared-dropdown option — label is the display text, value is the id (user/charity Guid). */
interface DropdownOption {
  id: any;
  name: string;
}

@Component({
  selector: 'app-notification-form',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, TranslateModule, SharedModule, PageHeaderComponent, BreadcrumbComponent],
  templateUrl: './notification-form.component.html',
  styleUrls: ['./notification-form.component.scss']
})
export class NotificationFormComponent implements OnInit, OnDestroy {
  private destroy$ = new Subject<void>();

  breadcrumbs: BreadcrumbItem[] = [
    { label: 'common.home', url: '/dashboard' },
    { label: 'notifications.title', url: '/notifications' }
  ];

  get breadcrumbsWithAction(): BreadcrumbItem[] {
    return [
      ...this.breadcrumbs,
      { label: this.isEditMode ? 'notifications.editNotification' : 'notifications.addNotification' }
    ];
  }

  notificationForm: FormGroup;
  /** The stored row being edited (null in create mode) — NOT the toast service. */
  storedNotification: NotificationsLog | null = null;
  loading = false;
  saving = false;
  resending = false;
  isEditMode = false;

  userOptions: DropdownOption[] = [];
  charityOptions: DropdownOption[] = [];

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
    private notificationLogService: NotificationLogService,
    private userManagementService: UserManagementService,
    private charityService: CharityService,
    private route: ActivatedRoute,
    private router: Router,
    private translate: TranslateService,
    private notification: NotificationService
  ) {
    this.notificationForm = this.createForm();
  }

  ngOnInit(): void {
    this.loadCatalogues();
    this.watchTargetToggles();
    this.checkEditMode();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  get pageTitleKey(): string {
    return this.isEditMode ? 'notifications.editNotification' : 'notifications.addNotification';
  }

  /**
   * Title/Message mandatory with the server's length caps; each recipient list
   * is mandatory only while its flag is raised (the server re-checks — the
   * browser is not the control).
   */
  private createForm(): FormGroup {
    return this.fb.group({
      title: ['', [Validators.required, Validators.maxLength(250)]],
      message: ['', [Validators.required, Validators.maxLength(4000)]],
      isUser: [false],
      isCharity: [false],
      recipientUserIds: [[] as string[]],
      recipientCharityIds: [[] as string[]]
    });
  }

  private checkEditMode(): void {
    const id = this.route.snapshot.params['id'];
    if (id) {
      this.isEditMode = true;
      this.loadNotification(id);
    }
  }

  private loadNotification(id: string): void {
    this.loading = true;

    this.notificationLogService.getNotificationById(id)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: item => {
          this.storedNotification = item;
          this.notificationForm.patchValue({
            title: item.title,
            message: item.message,
            isUser: item.isUser,
            isCharity: item.isCharity,
            recipientUserIds: (item.recipientUserIds || []).map(String),
            recipientCharityIds: (item.recipientCharityIds || []).map(String)
          });
          this.syncTargetControlStates();
          this.loading = false;
        },
        error: () => {
          this.notification.error(this.translate.instant('notifications.loadFailed'));
          this.loading = false;
          this.router.navigate(['/notifications']);
        }
      });
  }

  /**
   * Audience catalogues — users from the identity store (/api/usermanagement)
   * and the charity register (/api/Charities), the same sources the modules'
   * own screens offer.
   */
  private loadCatalogues(): void {
    this.userManagementService.getUsers({ page: 1, pageSize: 1000 })
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: result => {
          this.userOptions = (result.items || []).map(user => ({
            id: user.id,
            name: user.fullName || user.userName || user.email
          }));
        },
        error: () => this.notification.error(this.translate.instant('notifications.catalogueLoadFailed'))
      });

    this.charityService.getCharities({ pageNumber: 1, pageSize: 1000, isActive: true })
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: result => {
          this.charityOptions = (result.items || []).map(charity => ({
            id: charity.id,
            name: charity.name
          }));
        },
        error: () => this.notification.error(this.translate.instant('notifications.catalogueLoadFailed'))
      });
  }

  /**
   * Raise a flag → its recipient list becomes enabled and required; lower it →
   * the list empties and stops blocking submit. Runs on every toggle and once
   * after the edit-mode patch.
   */
  private watchTargetToggles(): void {
    this.notificationForm.get('isUser')!.valueChanges
      .pipe(takeUntil(this.destroy$))
      .subscribe(() => this.syncTargetControlStates());

    this.notificationForm.get('isCharity')!.valueChanges
      .pipe(takeUntil(this.destroy$))
      .subscribe(() => this.syncTargetControlStates());
  }

  private syncTargetControlStates(): void {
    this.syncTargetControl('isUser', 'recipientUserIds');
    this.syncTargetControl('isCharity', 'recipientCharityIds');
  }

  private syncTargetControl(flagName: string, listName: string): void {
    const flagOn = !!this.notificationForm.get(flagName)!.value;
    const list = this.notificationForm.get(listName)!;

    if (flagOn) {
      if (list.disabled) {
        list.enable({ emitEvent: false });
      }
      list.setValidators([Validators.required]);
    } else {
      if (!list.disabled) {
        list.disable({ emitEvent: false });
      }
      list.setValidators(null);
      list.setValue([], { emitEvent: false });
      list.setErrors(null);
    }
    list.updateValueAndValidity({ emitEvent: false });
  }

  /** At least one target flag must be raised — the row-level guard under the checkboxes. */
  get targetError(): string | null {
    const isUser = this.notificationForm.get('isUser')!;
    const isCharity = this.notificationForm.get('isCharity')!;
    if (!(isUser.touched && isCharity.touched)) {
      return null;
    }
    if (isUser.errors?.['server']) {
      return isUser.errors['server'];
    }
    if (!isUser.value && !isCharity.value) {
      return this.translate.instant('notifications.requireOneTarget');
    }
    return null;
  }

  /** Field-level validation message for the plain inputs (mission-form idiom). */
  fieldError(controlName: string): string | null {
    const control = this.notificationForm.get(controlName);
    if (!control || !control.errors || !(control.dirty || control.touched)) {
      return null;
    }
    if (control.errors['server']) {
      return control.errors['server'];
    }
    if (control.errors['required']) {
      return this.translate.instant('validation.required');
    }
    if (control.errors['maxlength']) {
      return this.translate.instant('validation.maxLength', { maxLength: control.errors['maxlength'].requiredLength });
    }
    return null;
  }

  onSubmit(): void {
    // Mark everything touched so the row-level target guard reports too.
    this.notificationForm.get('isUser')!.markAsTouched();
    this.notificationForm.get('isCharity')!.markAsTouched();
    this.markFormGroupTouched(this.notificationForm);

    if (this.notificationForm.invalid) {
      this.notification.error(this.translate.instant('notifications.fixValidationErrors'));
      return;
    }
    if (!this.notificationForm.value.isUser && !this.notificationForm.value.isCharity) {
      this.notification.error(this.translate.instant('notifications.fixValidationErrors'));
      return;
    }

    this.saving = true;
    const formValue = this.notificationForm.getRawValue();

    const request = {
      title: formValue.title,
      message: formValue.message,
      isUser: !!formValue.isUser,
      isCharity: !!formValue.isCharity,
      recipientUserIds: formValue.isUser ? (formValue.recipientUserIds || []) : [],
      recipientCharityIds: formValue.isCharity ? (formValue.recipientCharityIds || []) : []
    };

    if (this.isEditMode && this.storedNotification) {
      this.notificationLogService.updateNotification(this.storedNotification.id, request)
        .pipe(takeUntil(this.destroy$))
        .subscribe({
          next: () => {
            this.notification.success(this.translate.instant('notifications.notificationUpdated'));
            this.saving = false;
            this.router.navigate(['/notifications']);
          },
          error: httpError => this.handleSaveError(httpError, 'notifications.updateFailed')
        });
    } else {
      this.notificationLogService.createNotification(request)
        .pipe(takeUntil(this.destroy$))
        .subscribe({
          next: result => {
            const audience = result?.deliveredToUserIds?.length ?? 0;
            this.notification.success(this.translate.instant('notifications.notificationSent', { count: audience }));
            this.saving = false;
            this.router.navigate(['/notifications']);
          },
          error: httpError => this.handleSaveError(httpError, 'notifications.createFailed')
        });
    }
  }

  /**
   * Resend (edit mode only): re-push the stored content as-is to a freshly
   * resolved audience — separate from saving edits.
   */
  onResend(): void {
    if (!this.storedNotification) {
      return;
    }
    this.resending = true;

    this.notificationLogService.resendNotification(this.storedNotification.id)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: result => {
          this.resending = false;
          const audience = result?.deliveredToUserIds?.length ?? 0;
          this.notification.success(this.translate.instant('notifications.notificationSent', { count: audience }));
        },
        error: httpError => {
          this.resending = false;
          this.notification.error(httpError?.error?.message || this.translate.instant('notifications.resendFailed'));
        }
      });
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
        // Server keys are PascalCase DTO names ("Title", "IsUser") — controls are camelCase
        const controlName = field.charAt(0).toLowerCase() + field.slice(1);
        const control = this.notificationForm.get(controlName);
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

  onCancel(): void {
    this.router.navigate(['/notifications']);
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
}
