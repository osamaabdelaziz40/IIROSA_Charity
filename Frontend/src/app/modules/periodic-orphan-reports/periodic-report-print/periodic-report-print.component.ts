/**
 * Periodic Report Print Component
 * Implements UC-ORR-17 (§14.U.17 طباعة التقرير الدوري) — renders the official periodic
 * report form and hands it to the browser's print-to-PDF.
 *
 * Per the recorded client-side print ruling (epics 9/10/18): the legacy
 * POST /api/Reports/orphan-report-form/export/pdf route returns the composed JSON payload
 * (report data + resolved variant + attachment ids), this screen renders the document, and
 * `window.print()` produces the paper/PDF — no server PDF pipeline, no jsPDF (18-21 owns
 * that decision). The collapsed legacy template matrix drives the layout via the variant
 * key: disabled > studying > not-studying base + the document sections present.
 *
 * Document images stream as authenticated blobs (§14.U.16 pattern) and bind as object
 * URLs — an <img src> cannot carry the Bearer header. The print dialog opens once every
 * image has settled (loaded or failed); a manual print button stays available regardless.
 */

import { Component, OnInit, OnDestroy, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { TranslateModule } from '@ngx-translate/core';

import { PeriodicOrphanReportService } from '../services/periodic-orphan-report.service';
import {
  OrphanReportFormPrintPayload,
  OrphanReportFormAttachmentSlot,
  PeriodicOrphanReportDto
} from '../models/periodic-orphan-report.model';
import { AttachmentService } from '../../../core/services/attachment.service';
import { BreadcrumbComponent } from '../../../shared/components/breadcrumb/breadcrumb.component';
import { LoadingComponent } from '../../../shared/components/loading/loading.component';

@Component({
  selector: 'app-periodic-report-print',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [
    CommonModule,
    TranslateModule,
    BreadcrumbComponent,
    LoadingComponent
  ],
  templateUrl: './periodic-report-print.component.html',
  styleUrls: ['./periodic-report-print.component.scss']
})
export class PeriodicReportPrintComponent implements OnInit, OnDestroy {
  payload: OrphanReportFormPrintPayload | null = null;
  loading = true;
  /** AC 4 — nothing was produced (unknown/foreign id or a failed fetch); told, not an empty file. */
  failed = false;

  /** slot key → object URL (blob fetch per §14.U.16); missing key = that image is absent. */
  readonly imageUrls = new Map<string, string>();

  private readonly destroy$ = new Set<() => void>();
  private printed = false;
  /** Review P45 2026-08-24: the auto-print timer must die with the view — an
   *  orphaned timeout fired window.print() on whatever page the user had
   *  navigated to next. */
  private printTimer: ReturnType<typeof setTimeout> | null = null;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private reportService: PeriodicOrphanReportService,
    private attachmentService: AttachmentService,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    // Review P53 2026-08-24: snapshot.paramMap reads once — a same-route id change
    // (…/print/1 → …/print/2) reuses this component and would keep showing the
    // previous report. Observe the param so every id loads its own document.
    const sub = this.route.paramMap.subscribe(params => this.load(params.get('id')));
    this.destroy$.add(() => sub.unsubscribe());
  }

  private load(id: string | null): void {
    // Fresh document state per id (P53): release the previous run's blobs and
    // re-arm the one-shot auto-print.
    this.imageUrls.forEach(url => URL.revokeObjectURL(url));
    this.imageUrls.clear();
    this.printed = false;
    this.loading = true;
    this.failed = false;
    this.payload = null;
    this.cdr.markForCheck();

    if (!id) {
      this.loading = false;
      this.failed = true;
      this.cdr.markForCheck();
      return;
    }

    this.reportService.getPrintForm(id).subscribe({
      next: payload => {
        this.payload = payload;
        this.loading = false;
        this.cdr.markForCheck();
        this.loadImages(payload.attachments || []);
      },
      error: () => {
        // 404 (unknown id or outside the caller's charity scope) and transport failures
        // land in the same honest state — there is nothing to produce (AC 4/5).
        this.loading = false;
        this.failed = true;
        this.cdr.markForCheck();
      }
    });
  }

  ngOnDestroy(): void {
    this.destroy$.forEach(release => release());
    if (this.printTimer !== null) {
      clearTimeout(this.printTimer);
      this.printTimer = null;
    }
    this.imageUrls.forEach(url => URL.revokeObjectURL(url));
    this.imageUrls.clear();
  }

  get report(): PeriodicOrphanReportDto | null {
    return this.payload?.report ?? null;
  }

  /** The collapsed matrix base — drives the education/health section emphasis (AC 3). */
  get variantBase(): string {
    return (this.payload?.variant || 'not-studying').split('+')[0];
  }

  /** Document sections to render, in the payload's order, minus the header photo. */
  get documentSlots(): OrphanReportFormAttachmentSlot[] {
    return (this.payload?.attachments || []).filter(a => a.slot !== 'orphanPhoto');
  }

  /** Manual print (also auto-invoked once after the images settle). */
  print(): void {
    window.print();
  }

  back(): void {
    this.router.navigate(['/periodic-orphan-reports']);
  }

  /** §14.S.2 slot label keys (shared with the 9-16 gallery). */
  slotLabel(slot: string): string {
    return `periodicReports.attachments.slots.${slot}`;
  }

  /** Empty values render as the dash — the official form never shows blanks. */
  v(value: string | null | undefined): string {
    const trimmed = (value ?? '').toString().trim();
    return trimmed.length > 0 ? trimmed : '—';
  }

  trackBySlot(_index: number, slot: OrphanReportFormAttachmentSlot): string {
    return slot.slot;
  }

  /** Fetch each document image as a blob, then open the print dialog once all settle. */
  private loadImages(slots: OrphanReportFormAttachmentSlot[]): void {
    if (slots.length === 0) {
      this.autoPrint();
      return;
    }

    let pending = slots.length;
    slots.forEach(slot => {
      const sub = this.attachmentService.getImage(slot.id).subscribe({
        next: blob => {
          this.imageUrls.set(slot.slot, URL.createObjectURL(blob));
          this.cdr.markForCheck();
        },
        // Review P44 2026-08-24: RxJS never calls complete after error — a failed
        // image left `pending` stuck and the dialog never opened. A failed image
        // prints its section without the picture; the form still produces.
        error: () => this.settled(--pending),
        complete: () => this.settled(--pending)
      });
      this.destroy$.add(() => sub.unsubscribe());
    });
  }

  private settled(remaining: number): void {
    if (remaining > 0) return;
    this.autoPrint();
  }

  /** One shot — after the payload and every image have settled (images render in <img> on paper). */
  private autoPrint(): void {
    if (this.printed || this.failed) return;
    this.printed = true;
    // Review P45 2026-08-24: track the handle so ngOnDestroy can cancel it.
    this.printTimer = setTimeout(() => {
      this.printTimer = null;
      window.print();
    }, 400);
  }
}
