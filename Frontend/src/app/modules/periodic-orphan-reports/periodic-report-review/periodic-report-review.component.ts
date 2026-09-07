/**
 * Periodic Report Review Component
 * Implements UC-6.13: Review Periodic Report
 *
 * Allows Admin, Super Admin, Accountant, and Employee to:
 * - Review comprehensive periodic orphan report details
 * - Approve report (set Reviewed=true, IsAccepted=true)
 * - Reject report (set Reviewed=true, IsRefused=true with reason)
 *
 * Access: Super Admin, Admin, Accountant, Employee only
 */

import { Component, OnInit, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { TranslateModule, TranslateService } from '@ngx-translate/core';

import { PeriodicOrphanReportService } from '../services/periodic-orphan-report.service';
import {
  PeriodicOrphanReportDto,
  ReviewPeriodicReportDto
} from '../models/periodic-orphan-report.model';
import { LoadingComponent } from '../../../shared/components/loading/loading.component';
import { NotificationService } from '../../../core/services/notification.service';
import { AttachmentService } from '../../../core/services/attachment.service';
import { LookupManagementService } from '../../lookup-management/services/lookup-management.service';
import { LookupDto } from '../../lookup-management/models/lookup.model';

import { BreadcrumbComponent } from '../../../shared/components/breadcrumb/breadcrumb.component';
import { ReportAttachmentGalleryComponent } from '../report-attachment-gallery/report-attachment-gallery.component';

/** A §14.S.2 attachment slot resolved against the loaded report (id null ⇒ not provided). */
interface ReviewAttachmentSlot {
  control: string;
  labelKey: string;
  id: string | null;
}

@Component({
  selector: 'app-periodic-report-review',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [
    BreadcrumbComponent,

    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    TranslateModule,
    LoadingComponent,
    ReportAttachmentGalleryComponent
  ],
  templateUrl: './periodic-report-review.component.html',
  styleUrls: ['./periodic-report-review.component.scss']
})
export class PeriodicReportReviewComponent implements OnInit {
  /** Server messages that carry a translated key on the SPA side (BR-14 wording). */
  private static readonly KNOWN_MESSAGES: Record<string, string> = {
    'A refuse reason is required when refusing a report': 'periodicReports.refuseReasonRequired'
  };

  // Form - UC-6.13
  reviewForm: FormGroup;

  // Data
  loading = true;
  submitting = false;
  report?: PeriodicOrphanReportDto;
  reportId?: string;

  // Refusal reasons — live bilingual catalogue (UC-ORR-08), never a hardcoded array
  refusalReasons: LookupDto[] = [];

  constructor(
    private fb: FormBuilder,
    private route: ActivatedRoute,
    private router: Router,
    private periodicReportService: PeriodicOrphanReportService,
    private notification: NotificationService,
    private translate: TranslateService,
    private attachmentService: AttachmentService,
    private lookupService: LookupManagementService,
    private cdr: ChangeDetectorRef
  ) {
    this.reviewForm = this.buildForm();
  }

  ngOnInit(): void {
    this.reportId = this.route.snapshot.params['id'];
    if (this.reportId) {
      this.loadReport(this.reportId);
    }
    this.loadRefuseReasons();
  }

  /** سبب الرفض drop-down options — GET refuse-reasons (seeded bilingual rows). */
  private loadRefuseReasons(): void {
    this.lookupService.getRefuseReasons().subscribe({
      next: (reasons) => {
        this.refusalReasons = reasons;
        this.cdr.markForCheck();
      },
      error: () => (this.refusalReasons = [])
    });
  }

  trackByReasonId(_index: number, reason: LookupDto): number {
    return reason.id;
  }

  /** §14.S.2 documents the reviewer examines before deciding — id-keyed (BR-12). */
  get attachmentSlots(): ReviewAttachmentSlot[] {
    const r = this.report;
    if (!r) {
      return [];
    }
    return [
      { control: 'orphanImageId', labelKey: 'periodicReports.orphanPhoto', id: r.orphanImageId ?? null },
      { control: 'orphanCertificateImageId', labelKey: 'periodicReports.certificate', id: r.orphanCertificateImageId ?? null },
      { control: 'medicalReportImageId', labelKey: 'periodicReports.medicalReport', id: r.medicalReportImageId ?? null },
      { control: 'orphanDeadImageId', labelKey: 'periodicReports.form.orphanDeadImage', id: r.orphanDeadImageId ?? null },
      { control: 'orphanMarriageImageId', labelKey: 'periodicReports.form.orphanMarriageImage', id: r.orphanMarriageImageId ?? null }
    ];
  }

  /** JWT-protected endpoint — fetch as a blob and open, a plain href would 401. */
  openAttachment(id: string): void {
    this.attachmentService.download(id).subscribe({
      next: (blob) => {
        const url = URL.createObjectURL(blob);
        window.open(url, '_blank');
        setTimeout(() => URL.revokeObjectURL(url), 60_000);
      },
      error: () => this.notification.show(
        this.translate.instant('periodicReports.attachmentDownloadFailed'),
        'error'
      )
    });
  }

  trackBySlotControl(_index: number, item: ReviewAttachmentSlot): string {
    return item.control;
  }

  /**
   * Build review form - UC-6.13
   */
  private buildForm(): FormGroup {
    return this.fb.group({
      isApproved: [null, Validators.required],
      refuseReason: [''],
      refuseReasonId: [''],
      reviewComments: ['']
    });
  }

  /**
   * Load report for review - UC-6.13
   */
  private loadReport(id: string): void {
    this.loading = true;
    this.cdr.markForCheck();

    this.periodicReportService.getReport(id).subscribe({
      next: (report: PeriodicOrphanReportDto) => {
        this.report = report;
        this.loading = false;
        this.cdr.markForCheck();
      },
      error: (error) => {
        console.error('Error loading report:', error);
        // Review P37c 2026-08-24: a failed load used to leave a silent blank
        // review screen — tell the user and return to the pending queue.
        this.loading = false;
        this.notification.show(
          (typeof error === 'string' ? error : error?.message)
            || this.translate.instant('periodicReports.loadFailed'),
          'error'
        );
        this.router.navigate(['../../'], { relativeTo: this.route });
        this.cdr.markForCheck();
      }
    });
  }

  /** §25.5 — a decided report cannot be re-decided here; resubmission (9-5) reopens it. */
  get decisionBlocked(): boolean {
    return !!this.report?.reviewed || this.submitting;
  }

  /**
   * Approve report - UC-6.13 / UC-ORR-07
   */
  approveReport(): void {
    if (!this.reportId || this.decisionBlocked) return;

    this.reviewForm.patchValue({ isApproved: true });
    this.submitReview();
  }

  /**
   * Reject report - UC-6.13 / UC-ORR-08
   */
  rejectReport(): void {
    if (!this.reportId || this.decisionBlocked) return;

    // Require refusal reason - UC-6.13
    const refuseReason = this.reviewForm.value.refuseReason;
    const refuseReasonId = this.reviewForm.value.refuseReasonId;

    if (!refuseReason && !refuseReasonId) {
      this.reviewForm.get('refuseReasonId')?.markAsTouched();
      this.notification.show(
        this.translate.instant('periodicReports.refuseReasonRequired'),
        'warning'
      );
      return;
    }

    this.reviewForm.patchValue({ isApproved: false });
    this.submitReview();
  }

  /**
   * Submit review - UC-6.13. Empty optional controls are omitted from the
   * payload (an empty-string refuseReasonId would fail int? binding).
   */
  private submitReview(): void {
    if (!this.reportId) return;

    this.submitting = true;
    this.cdr.markForCheck();

    const raw = this.reviewForm.value;
    const approved = raw.isApproved === true;
    const review: ReviewPeriodicReportDto = {
      reportId: this.reportId,
      isApproved: approved,
      reviewComments: raw.reviewComments || undefined
    };
    if (!approved) {
      review.refuseReason = raw.refuseReason || undefined;
      const reasonId = Number(raw.refuseReasonId);
      // Review P46 2026-08-24: Number(null) === 0, not NaN — a cleared dropdown
      // slipped through as a 0 FK id. Only send a positive integer the select
      // actually produced.
      if (raw.refuseReasonId !== '' && raw.refuseReasonId !== null && raw.refuseReasonId !== undefined
        && Number.isInteger(reasonId) && reasonId > 0) {
        review.refuseReasonId = reasonId;
      }
    }

    this.periodicReportService.reviewReport(this.reportId, review).subscribe({
      next: () => {
        this.submitting = false;
        this.notification.show(
          this.translate.instant(approved
            ? 'periodicReports.review.approvedSuccessfully'
            : 'periodicReports.review.rejectedSuccessfully'),
          'success'
        );
        // Navigate back to the pending queue
        this.router.navigate(['../../'], { relativeTo: this.route });
      },
      error: (error) => {
        console.error('Error submitting review:', error);
        this.submitting = false;
        this.cdr.markForCheck();

        // BR-14: flag the offending control and translate the known server wording.
        // Review P35 2026-08-24: handleError already unwraps to the ApiResponse
        // body — the old error?.error ?? {} double-unwrap always produced {},
        // leaving the wording map and control flagging as dead code.
        const body = (error && typeof error === 'object' ? error : {}) as {
          message?: string;
          errors?: Record<string, string[]>;
        };
        const errors: Record<string, string[]> | undefined = body.errors;
        if (errors) {
          for (const key of Object.keys(errors)) {
            const camelKey = key.charAt(0).toLowerCase() + key.slice(1);
            if (errors[key]?.length) {
              this.reviewForm.get(camelKey)?.setErrors({ server: errors[key][0] });
              this.reviewForm.get(camelKey)?.markAsTouched();
            }
          }
        }
        const raw = typeof body.message === 'string' ? body.message : '';
        const knownKey = raw ? PeriodicReportReviewComponent.KNOWN_MESSAGES[raw] : undefined;
        this.notification.show(
          knownKey
            ? this.translate.instant(knownKey)
            : raw || this.translate.instant('periodicReports.review.submitFailed'),
          'error'
        );
      }
    });
  }

  /**
   * Cancel and go back
   */
  cancel(): void {
    this.router.navigate(['../../'], { relativeTo: this.route });
  }

  /**
   * Get status badge class
   */
  getStatusClass(status: string): string {
    switch ((status || '').toLowerCase()) {
      case 'approved':
      case 'accepted':
        return 'bg-success';
      case 'rejected':
      case 'refused':
        return 'bg-danger';
      case 'pending':
        return 'bg-warning';
      default:
        return 'bg-secondary';
    }
  }

  /**
   * Check if refuse reason is required
   */
  get refuseReasonRequired(): boolean {
    return this.reviewForm.value.isApproved === false;
  }
}
