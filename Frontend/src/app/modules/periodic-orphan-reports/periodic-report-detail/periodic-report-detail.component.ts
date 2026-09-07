/**
 * Periodic Report Detail Component
 * Read-only §14.S.2 view of a periodic orphan report — every section grouped,
 * missing optional fields shown as "—", attachments downloadable by server id.
 * Implements viewing for UC-6.11, UC-6.14, UC-6.15 (UC-ORR-04).
 */

import { Component, OnInit, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { TranslateModule, TranslateService } from '@ngx-translate/core';

import { PeriodicOrphanReportService } from '../services/periodic-orphan-report.service';
import { PeriodicOrphanReportDto } from '../models/periodic-orphan-report.model';
import { LoadingComponent } from '../../../shared/components/loading/loading.component';
import { NotificationService } from '../../../core/services/notification.service';
import { AttachmentService } from '../../../core/services/attachment.service';

import { BreadcrumbComponent } from '../../../shared/components/breadcrumb/breadcrumb.component';
import { ReportAttachmentGalleryComponent } from '../report-attachment-gallery/report-attachment-gallery.component';

/** A §14.S.2 attachment slot resolved against the loaded report (id null ⇒ not provided). */
interface AttachmentSlot {
  control: string;
  labelKey: string;
  id: string | null;
}

@Component({
  selector: 'app-periodic-report-detail',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [
    BreadcrumbComponent,
    RouterLink,

    CommonModule,
    TranslateModule,
    LoadingComponent,
    ReportAttachmentGalleryComponent
  ],
  templateUrl: './periodic-report-detail.component.html',
  styleUrls: ['./periodic-report-detail.component.scss']
})
export class PeriodicReportDetailComponent implements OnInit {
  loading = true;
  report?: PeriodicOrphanReportDto;
  reportId?: string;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private periodicReportService: PeriodicOrphanReportService,
    private notification: NotificationService,
    private translate: TranslateService,
    private attachmentService: AttachmentService,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    this.reportId = this.route.snapshot.params['id'];
    if (this.reportId) {
      this.loadReport(this.reportId);
    }
  }

  private loadReport(id: string): void {
    this.loading = true;
    this.cdr.markForCheck();
    this.periodicReportService.getReport(id).subscribe({
      next: (report) => {
        this.report = report;
        this.loading = false;
        this.cdr.markForCheck();
      },
      error: (error) => {
        console.error('Error loading report:', error);
        // Review P37a 2026-08-24: a failed load used to leave a silent blank
        // screen — tell the user and go back to the register.
        this.loading = false;
        this.notification.show(
          (typeof error === 'string' ? error : error?.message)
            || this.translate.instant('periodicReports.loadFailed'),
          'error'
        );
        this.router.navigate(['/periodic-orphan-reports']);
        this.cdr.markForCheck();
      }
    });
  }

  editReport(): void {
    if (this.reportId) {
      this.router.navigate([this.reportId, 'edit'], { relativeTo: this.route.parent });
    }
  }

  async deleteReport(): Promise<void> {
    if (!this.reportId) {
      return;
    }
    const confirmed = await this.notification.confirm(
      this.translate.instant('periodicReports.deleteConfirm'),
      this.translate.instant('periodicReports.deleteTitle')
    );
    if (!confirmed) {
      return;
    }
    this.periodicReportService.deleteReport(this.reportId).subscribe({
      next: () => {
        this.notification.show(
          this.translate.instant('periodicReports.deletedSuccessfully'),
          'success'
        );
        this.router.navigate(['../'], { relativeTo: this.route });
      },
      error: (error) => {
        console.error('Error deleting report:', error);
        // Review P36b 2026-08-24: handleError already unwraps to the ApiResponse
        // body — the old error?.error?.message double-unwrap always missed it, so
        // the server's refusal reason never surfaced.
        const reason = (typeof error === 'string' ? error : error?.message)
          || this.translate.instant('periodicReports.deleteFailed');
        this.notification.show(reason, 'error');
        this.cdr.markForCheck();
      }
    });
  }

  /**
   * The five §14.S.2 slots, resolved against the loaded report so the template
   * can render presence + a download action per document (BR-12: id reference).
   */
  get attachmentSlots(): AttachmentSlot[] {
    const r = this.report;
    if (!r) {
      return [];
    }
    return [
      { control: 'orphanImageId', labelKey: 'periodicReports.form.orphanImage', id: r.orphanImageId ?? null },
      { control: 'orphanCertificateImageId', labelKey: 'periodicReports.form.orphanCertificateImage', id: r.orphanCertificateImageId ?? null },
      { control: 'medicalReportImageId', labelKey: 'periodicReports.form.medicalReportImage', id: r.medicalReportImageId ?? null },
      { control: 'orphanDeadImageId', labelKey: 'periodicReports.form.orphanDeadImage', id: r.orphanDeadImageId ?? null },
      { control: 'orphanMarriageImageId', labelKey: 'periodicReports.form.orphanMarriageImage', id: r.orphanMarriageImageId ?? null }
    ];
  }

  /** Free-text list of documents the field officer marked as not supplied. */
  get missingDocumentsNames(): string | null {
    if (!this.report?.missingDocuments) {
      return null;
    }
    return this.report.missingDocumentsName || null;
  }

  /**
   * Download the attachment and open it in a new tab — the endpoint is
   * JWT-protected, so a plain href would 401; fetch as a blob instead.
   */
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

  /** Refused reports reopen for correction/resubmission; locked and accepted ones do not. */
  get canEdit(): boolean {
    return !!this.report && !this.report.locked && !this.report.isAccepted;
  }

  /** Deletion refuses only locked rows — HQ may remove reviewed records (§14.D-25.5 A2). */
  get canDelete(): boolean {
    return !!this.report && !this.report.locked;
  }

  trackBySlotControl(index: number, item: AttachmentSlot): string {
    return item.control;
  }
}
