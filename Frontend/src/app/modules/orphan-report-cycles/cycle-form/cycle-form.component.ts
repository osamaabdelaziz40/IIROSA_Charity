import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { TranslateModule, TranslateService } from '@ngx-translate/core';

import { OrphanReportCycleService } from '../services/orphan-report-cycle.service';
import {
  OrphanReportCycleDto,
  OrphanReportCycleStatus
} from '../models/orphan-report-cycle.model';
import { NotificationService } from '../../../core/services/notification.service';
import { BreadcrumbComponent, BreadcrumbItem } from '../../../shared/components';

/**
 * UC-ORC-02/03 cycle form (§26.S.2) — bilingual master data and the period. Status is
 * never an input: the server derives it from the dates (BR-ORC-02), and in edit mode
 * it renders as a read-only badge next to the live coverage numbers. A completed cycle
 * opens read-only (BR-ORC-08) — fields disabled, save hidden; the endpoint stays the
 * authority on every rule.
 */
@Component({
  selector: 'app-cycle-form',
  standalone: true,
  imports: [CommonModule, RouterModule, ReactiveFormsModule, TranslateModule, BreadcrumbComponent],
  templateUrl: './cycle-form.component.html',
  styleUrls: ['./cycle-form.component.scss']
})
export class CycleFormComponent implements OnInit {
  isEditMode = false;
  cycleId?: string;
  /** Loaded edit-mode cycle — carries the read-only status/coverage band. */
  cycle?: OrphanReportCycleDto;
  loading = false;
  saving = false;

  /** Server field errors, camelCased onto the offending controls (errors-map shape). */
  serverErrors: Record<string, string> = {};

  breadcrumbs: BreadcrumbItem[] = [
    { label: 'common.home', url: '/dashboard' },
    { label: 'orphanReportCycles.title', url: '/orphan-report-cycles' },
    { label: 'orphanReportCycles.createCycle' }
  ];

  cycleForm: FormGroup;

  constructor(
    private fb: FormBuilder,
    private route: ActivatedRoute,
    private router: Router,
    private cycleService: OrphanReportCycleService,
    private notification: NotificationService,
    private translate: TranslateService
  ) {
    this.cycleForm = this.fb.group({
      titleAr: ['', [Validators.required, Validators.maxLength(250)]],
      titleEn: ['', [Validators.maxLength(250)]],
      descriptionAr: ['', [Validators.maxLength(2000)]],
      descriptionEn: ['', [Validators.maxLength(2000)]],
      startDate: ['', Validators.required],
      endDate: ['', Validators.required]
    });
  }

  ngOnInit(): void {
    this.cycleId = this.route.snapshot.paramMap.get('id') ?? undefined;
    this.isEditMode = !!this.cycleId;

    if (this.isEditMode && this.cycleId) {
      this.breadcrumbs[2] = { label: 'orphanReportCycles.editCycle' };
      this.loadCycle(this.cycleId);
    }
  }

  loadCycle(id: string): void {
    this.loading = true;
    this.cycleService.getCycle(id).subscribe({
      next: cycle => {
        this.cycle = cycle;
        this.cycleForm.patchValue({
          titleAr: cycle.titleAr,
          titleEn: cycle.titleEn ?? '',
          descriptionAr: cycle.descriptionAr ?? '',
          descriptionEn: cycle.descriptionEn ?? '',
          // Local date parts — toISOString() would slide a date back a day west of UTC.
          startDate: cycle.startDate.split('T')[0],
          endDate: cycle.endDate.split('T')[0]
        });
        this.loading = false;
      },
      error: (error: any) => {
        this.loading = false;
        this.notification.error(
          error?.error?.message || this.translate.instant('orphanReportCycles.form.loadFailed'));
        this.router.navigate(['/orphan-report-cycles']);
      }
    });
  }

  /** BR-ORC-08 — a completed cycle is the read-only audit record of a reporting round. */
  get readOnly(): boolean {
    return this.isEditMode && this.cycle?.status === OrphanReportCycleStatus.Completed;
  }

  statusKey(): string {
    switch (this.cycle?.status) {
      case OrphanReportCycleStatus.InProgress: return 'orphanReportCycles.status.inProgress';
      case OrphanReportCycleStatus.Completed: return 'orphanReportCycles.status.completed';
      default: return 'orphanReportCycles.status.notStarted';
    }
  }

  statusClass(): string {
    switch (this.cycle?.status) {
      case OrphanReportCycleStatus.InProgress: return 'badge-success';
      case OrphanReportCycleStatus.Completed: return 'badge-secondary';
      default: return 'badge-warning';
    }
  }

  save(): void {
    if (this.cycleForm.invalid || this.saving) {
      this.cycleForm.markAllAsTouched();
      return;
    }

    const values = this.cycleForm.value;

    // Client-side mirror of the server rule — the endpoint re-validates regardless.
    if (values.endDate <= values.startDate) {
      this.notification.warning(this.translate.instant('orphanReportCycles.form.endAfterStart'));
      return;
    }

    this.saving = true;
    this.serverErrors = {};

    const done = () => {
      this.saving = false;
      this.notification.success(this.translate.instant('orphanReportCycles.form.saved'));
      this.router.navigate(['/orphan-report-cycles']);
    };
    const fail = (error: any) => {
      this.saving = false;
      this.handleSaveError(error);
    };

    if (this.isEditMode && this.cycleId) {
      this.cycleService.updateCycle(this.cycleId, { ...values, id: this.cycleId })
        .subscribe({ next: done, error: fail });
    } else {
      this.cycleService.createCycle(values).subscribe({ next: done, error: fail });
    }
  }

  cancel(): void {
    this.router.navigate(['/orphan-report-cycles']);
  }

  /**
   * { message, errors: { PascalCase key: [msgs] } } — camelCase the key, keep the first
   * message for the toast and flag the offending control.
   */
  private handleSaveError(error: any): void {
    const body = error?.error ?? error ?? {};
    const errors: Record<string, string[]> | undefined = body.errors;

    if (errors) {
      const mapped: Record<string, string> = {};
      for (const key of Object.keys(errors)) {
        const camelKey = key.charAt(0).toLowerCase() + key.slice(1);
        const messages = errors[key];
        if (messages?.length) {
          mapped[camelKey] = messages[0];
          const control = this.cycleForm.get(camelKey);
          control?.setErrors({ server: messages[0] });
          control?.markAsTouched();
        }
      }
      this.serverErrors = mapped;
    }

    this.notification.error(
      (typeof body.message === 'string' && body.message)
        || this.translate.instant('orphanReportCycles.form.saveFailed'));
  }

  fieldError(controlName: string): string | null {
    const control = this.cycleForm.get(controlName);
    if (control?.errors?.['server']) {
      return control.errors['server'] as string;
    }
    if (control?.invalid && (control.touched || control.dirty)) {
      return this.translate.instant('common.isRequired');
    }
    return null;
  }
}
