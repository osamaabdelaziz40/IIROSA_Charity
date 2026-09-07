/**
 * Report Attachment Gallery Component
 * Implements UC-ORR-16 (§14.U.16 صور اليتيم والشهادات) — the viewer over a report's
 * five attachment slots: orphan photo, student certificate, medical report,
 * death certificate, marriage contract.
 *
 * The ids are the single truth (BR-12); images stream as blobs through the
 * authenticated ApiService and bind as object URLs — an <img src> cannot carry the
 * Bearer header and the endpoint stays [Authorize]. Empty slots show the honest
 * "no image" state (AC 3).
 */

import { Component, ChangeDetectionStrategy, ChangeDetectorRef, Input, OnChanges, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TranslateModule, TranslateService } from '@ngx-translate/core';

import { PeriodicOrphanReportDto } from '../models/periodic-orphan-report.model';
import { AttachmentService } from '../../../core/services/attachment.service';
import { NotificationService } from '../../../core/services/notification.service';

/** One viewer slot — the report's field pair (id wins; the URL field is informational). */
interface AttachmentSlot {
  key: string;
  id?: string;
  objectUrl?: string;
  loading: boolean;
  failed: boolean;
}

@Component({
  selector: 'app-report-attachment-gallery',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [CommonModule, TranslateModule],
  templateUrl: './report-attachment-gallery.component.html',
  styleUrls: ['./report-attachment-gallery.component.scss']
})
export class ReportAttachmentGalleryComponent implements OnChanges, OnDestroy {
  @Input() report: PeriodicOrphanReportDto | null = null;

  slots: AttachmentSlot[] = [];

  /** The enlarged view (lightboxed slot), null when closed. */
  enlarged: AttachmentSlot | null = null;
  enlargedUrl = '';

  /** Review P33 2026-08-24: blob reads still in flight at destroy time must not
   *  write into the destroyed view — releaseAll() has already run, so a late
   *  objectUrl write would leak (never revoked) and markForCheck would throw. */
  private destroyed = false;

  constructor(
    private attachmentService: AttachmentService,
    private notification: NotificationService,
    private translate: TranslateService,
    private cdr: ChangeDetectorRef
  ) {}

  /** Re-reads the five slots whenever the loaded report changes. */
  ngOnChanges(): void {
    this.releaseAll();
    const r = this.report;
    if (!r) {
      this.slots = [];
      return;
    }

    this.slots = [
      { key: 'orphanPhoto', id: r.orphanImageId, loading: !!r.orphanImageId, failed: false },
      { key: 'certificate', id: r.orphanCertificateImageId, loading: !!r.orphanCertificateImageId, failed: false },
      { key: 'medicalReport', id: r.medicalReportImageId, loading: !!r.medicalReportImageId, failed: false },
      { key: 'deathCertificate', id: r.orphanDeadImageId, loading: !!r.orphanDeadImageId, failed: false },
      { key: 'marriageContract', id: r.orphanMarriageImageId, loading: !!r.orphanMarriageImageId, failed: false }
    ];

    this.slots.filter(s => s.id).forEach(s => this.loadSlot(s));
    this.cdr.markForCheck();
  }

  ngOnDestroy(): void {
    this.destroyed = true;
    this.releaseAll();
  }

  private loadSlot(slot: AttachmentSlot): void {
    this.attachmentService.getImage(slot.id!).subscribe({
      next: blob => {
        if (this.destroyed) {
          // releaseAll already revoked everything — creating the URL here would
          // leak it; skip the write entirely.
          return;
        }
        slot.objectUrl = URL.createObjectURL(blob);
        slot.loading = false;
        this.cdr.markForCheck();
      },
      error: () => {
        if (this.destroyed) {
          return;
        }
        // The slot keeps its honest state — label + no-image marker, never a broken img.
        slot.loading = false;
        slot.failed = true;
        this.cdr.markForCheck();
      }
    });
  }

  trackBySlotKey(_index: number, slot: AttachmentSlot): string {
    return slot.key;
  }

  open(slot: AttachmentSlot): void {
    if (!slot.objectUrl) return;
    this.enlarged = slot;
    this.enlargedUrl = slot.objectUrl;
  }

  close(): void {
    this.enlarged = null;
    this.enlargedUrl = '';
  }

  /** Download via the blob pattern (the stored bytes, not the object URL re-encode). */
  download(slot: AttachmentSlot): void {
    if (!slot.id) return;
    this.attachmentService.download(slot.id).subscribe({
      next: blob => {
        const url = URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        // Review P55 2026-08-24: browsers take the extension from the download
        // attribute — a bare key_id filename saved as an extensionless file.
        const ext = (blob.type.split('/')[1] || 'bin').replace(/[^a-z0-9]/gi, '');
        a.download = `${slot.key}_${slot.id}.${ext === 'jpeg' ? 'jpg' : ext}`;
        document.body.appendChild(a);
        a.click();
        document.body.removeChild(a);
        URL.revokeObjectURL(url);
      },
      error: () =>
        this.notification.error(this.translate.instant('periodicReports.attachments.downloadFailed'))
    });
  }

  private releaseAll(): void {
    this.slots?.forEach(s => {
      if (s.objectUrl) URL.revokeObjectURL(s.objectUrl);
    });
    this.close();
  }
}
