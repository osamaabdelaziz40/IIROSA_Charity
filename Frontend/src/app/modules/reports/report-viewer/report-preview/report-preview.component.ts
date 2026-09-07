import { Component, ElementRef, HostListener, OnDestroy, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { DomSanitizer, SafeResourceUrl } from '@angular/platform-browser';
import { TranslateModule, TranslateService } from '@ngx-translate/core';

import { NotificationService } from '../../../../core/services/notification.service';
import { ReportPreviewSource } from '../../services/report-pdf.service';

/**
 * UC-RPT-40 (§23.U.40 عرض التقرير) — the ONE on-screen preview modal every report screen
 * shares (the anti-reinvention AC). Frames the report's standalone document via a blob URL:
 *
 * عرض (the shell's command) → the screen's document producer → `ReportPreviewSource` →
 * `open()` mints the blob and mounts the iframe → طباعة runs the frame's native print
 * (the browser's PDF pipeline — the epic-wide 18-21 ruling) → حفظ downloads the SAME blob
 * (no regeneration) → إغلاق (or Escape / backdrop) revokes the object URL.
 *
 * The producer result is guarded BEFORE the frame mounts: an empty/absent document refuses
 * with the nothing-to-display message (AC 3); a failed producer surfaces its toast in the
 * shell and never opens a blank modal (AC 4).
 */
@Component({
  selector: 'app-report-preview',
  standalone: true,
  imports: [CommonModule, TranslateModule],
  templateUrl: './report-preview.component.html',
  styleUrls: ['./report-preview.component.scss']
})
export class ReportPreviewComponent implements OnDestroy {
  visible = false;
  /** The blob URL as a trusted resource — iframe [src] needs the explicit bypass. */
  frameSrc: SafeResourceUrl | null = null;

  /** Kept so حفظ re-uses the SAME blob — one generation per preview (AC 2). */
  private blob: Blob | null = null;
  private blobUrl: string | null = null;
  private documentTitle = 'report';

  @ViewChild('frame') private frame?: ElementRef<HTMLIFrameElement>;

  constructor(
    private sanitizer: DomSanitizer,
    private notification: NotificationService,
    private translate: TranslateService
  ) {}

  /** عرض — mint the blob, mount the frame. A null/empty source refuses (AC 3). */
  open(source: ReportPreviewSource | null): void {
    // Hygiene first: a re-preview while open must not leak the previous object URL.
    this.release();

    if (!source || !source.html) {
      // Review P27 2026-08-26: an empty re-preview while the modal is OPEN must close it —
      // release() alone revoked the URL but left an empty, dead frame mounted behind the
      // nothing-to-display toast.
      if (this.visible) {
        this.close();
      }
      this.notification.info(this.translate.instant('reports.preview.nothingToDisplay'));
      return;
    }

    this.blob = new Blob([source.html], { type: 'text/html' });
    this.blobUrl = URL.createObjectURL(this.blob);
    this.frameSrc = this.sanitizer.bypassSecurityTrustResourceUrl(this.blobUrl);
    this.documentTitle = source.documentTitle || 'report';
    this.visible = true;
  }

  /**
   * طباعة — the framed document's own print pipeline (AC 2: one interaction, no regen).
   * Review P27 2026-08-26: refuses while the frame is still loading — printing an unfinished
   * document fired the browser dialog over a blank/partial page.
   */
  print(): void {
    const frameWindow = this.frame?.nativeElement.contentWindow;
    if (!frameWindow || frameWindow.document?.readyState !== 'complete') {
      return;
    }
    frameWindow.focus();
    frameWindow.print();
  }

  /** حفظ — downloads the SAME blob; the browser's print-to-PDF remains the PDF path. */
  save(): void {
    if (!this.blobUrl) {
      return;
    }
    const anchor = document.createElement('a');
    anchor.href = this.blobUrl;
    // Review P27 2026-08-26: document titles carry report names/dates — strip path/FS-hostile
    // characters so the browser keeps the whole title instead of truncating at a slash.
    anchor.download = `${this.documentTitle.replace(/[\\/:*?"<>|]/g, '-')}.html`;
    anchor.click();
  }

  /** إغلاق — revoke the object URL (Task 1: no leaks across previews). */
  close(): void {
    this.release();
    this.visible = false;
  }

  /** Escape closes like إغلاق (modal UX; the app is RTL-first, Escape is universal). */
  @HostListener('document:keydown.escape')
  onEscape(): void {
    if (this.visible) {
      this.close();
    }
  }

  onBackdrop(event: MouseEvent): void {
    if (event.target === event.currentTarget) {
      this.close();
    }
  }

  /**
   * Review P27 2026-08-26: navigating away with the modal open (route change, logout) must
   * still revoke the object URL — destroy-time release closes the leak path that
   * إغلاق/Escape/backdrop only cover when the user dismisses the modal first.
   */
  ngOnDestroy(): void {
    this.release();
  }

  private release(): void {
    if (this.blobUrl) {
      URL.revokeObjectURL(this.blobUrl);
    }
    this.blobUrl = null;
    this.blob = null;
    this.frameSrc = null;
  }
}
