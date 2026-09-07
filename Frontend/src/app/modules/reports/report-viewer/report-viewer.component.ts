import { Component, EventEmitter, Input, Output, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TranslateModule, TranslateService } from '@ngx-translate/core';

import { PageHeaderComponent } from '../../../shared/components/page-header/page-header.component';
import { PaginationComponent } from '../../../shared/components/pagination/pagination.component';
import { ReportPreviewComponent } from './report-preview/report-preview.component';
import { NotificationService } from '../../../core/services/notification.service';
import { ReportPreviewSource } from '../services/report-pdf.service';
import { ReportExportService } from '../services/report-export.service';
import { ReportExportRequest } from '../models/report-columns';

/**
 * EP-18 shared report shell (18-1) — every 18-x report screen feeds this instead of rebuilding
 * chrome. The shell owns the generic contract: page header, filter-panel slot ([report-filters]),
 * bespoke-grid slot ([report-grid] — the column set differs per report, so the grid itself is
 * projected, never shared), بحث / استخراج البيانات / عرض commands, empty states and the shared
 * Pagination. Deliberately NOT data-list — report grids are bespoke wide tables with horizontal
 * scroll (recorded deviation, story 18-1). Default change detection like every list screen
 * (codebase precedent).
 *
 * عرض (18-40): the screen registers a document producer — sync over the loaded grid or async
 * over a fetch — and the shell drives the ONE shared preview modal. This input IS the
 * producer registry's binding (producers close over live component state, so a static
 * key→producer map cannot serve them); a screen that passes none keeps عرض disabled — honest
 * absence, no dead button.
 */
@Component({
  selector: 'app-report-viewer',
  standalone: true,
  imports: [CommonModule, TranslateModule, PageHeaderComponent, PaginationComponent, ReportPreviewComponent],
  templateUrl: './report-viewer.component.html',
  styleUrls: ['./report-viewer.component.scss']
})
export class ReportViewerComponent {
  /** i18n key for the page title. */
  @Input() titleKey!: string;
  /** i18n key for the page subtitle. */
  @Input() subtitleKey = '';

  /** True while the report query or export is in flight — disables both commands. */
  @Input() loading = false;
  @Input() exporting = false;

  /** True once بحث has run at least once (drives empty vs initial state). */
  @Input() hasRun = false;

  /**
   * Query-only screens (§23.S.9 etc.) list no استخراج command — they hide the button.
   * Defaults true so every exporting report keeps it.
   */
  @Input() showExport = true;

  /**
   * عرض (§23.U.40) — the screen's document producer: returns the previewable document for the
   * CURRENT filter + variant, or null when there is nothing to display. May be async (reports
   * that fetch their print payload at press time). Null input = the screen has no printable
   * document and عرض stays disabled.
   */
  @Input() previewProducer: (() => ReportPreviewSource | null | Promise<ReportPreviewSource | null>) | null = null;

  /**
   * استخراج البيانات (§23.U.41) — the screen's export request: the report's registry key plus
   * a page fetcher closing over the CURRENT filter. Given one, the shell runs the ENGINE
   * (exportAll — every page under the cap, truncation declared); without one the legacy
   * (export) event fires for the screens whose exports predate the engine (special-case
   * producers). A screen with neither keeps the command hidden via [showExport]="false" —
   * honest absence.
   */
  @Input() exportRequest: (() => ReportExportRequest | null) | null = null;

  /** Paging state — owned here, fed by the report component. */
  @Input() currentPage = 1;
  @Input() pageSize = 20;
  @Input() totalCount = 0;

  /** بحث — runs the report with the current filters. */
  @Output() search = new EventEmitter<void>();
  /** استخراج البيانات — exports the whole selection (all pages), not just the loaded page. */
  @Output() export = new EventEmitter<void>();
  @Output() pageChange = new EventEmitter<number>();

  /** True while an async producer is resolving — rides the عرض button's spinner. */
  previewing = false;

  /** True while the engine walks the export pages — rides the استخراج button's spinner. */
  engineExporting = false;

  @ViewChild(ReportPreviewComponent) private previewModal?: ReportPreviewComponent;

  constructor(
    private notification: NotificationService,
    private translate: TranslateService,
    private reportExportService: ReportExportService
  ) {}

  get hasRows(): boolean {
    return this.totalCount > 0;
  }

  /**
   * عرض — resolve the producer, then hand the document to the shared modal. A null result
   * refuses inside the modal (nothing-to-display); a producer FAILURE surfaces as a toast and
   * the modal stays closed — never a blank frame (AC 4).
   */
  openPreview(): Promise<void> {
    if (!this.previewProducer || this.loading || this.exporting || this.previewing) {
      return Promise.resolve();
    }

    try {
      const result = this.previewProducer();
      if (result && typeof (result as Promise<ReportPreviewSource | null>).then === 'function') {
        this.previewing = true;
        return (result as Promise<ReportPreviewSource | null>)
          .then(source => {
            this.previewing = false;
            this.previewModal?.open(source);
          })
          .catch((error: any) => {
            // AC 4 — the data call behind the document failed: toast, modal never opens.
            // Review P27 2026-08-26: the SERVER's reason rides the toast when there is one
            // (18-40 AC 4 sibling) — a generic "preview failed" hid 400-level refusals.
            this.previewing = false;
            this.notification.error(
              error?.error?.message || error?.message || this.translate.instant('reports.preview.error')
            );
          });
      }
      this.previewModal?.open(result as ReportPreviewSource | null);
    } catch (error: any) {
      // Review P27 2026-08-26: same sibling idiom for the sync-throw path.
      this.notification.error(
        error?.error?.message || error?.message || this.translate.instant('reports.preview.error')
      );
    }
    return Promise.resolve();
  }

  /**
   * استخراج البيانات (§23.U.41) — with a registered export request the SHELL runs the engine:
   * every page under the endpoint cap, the truncation declared on a manifest sheet. An empty
   * result is told, not downloaded (AC 3); a failed fetch toasts (AC 5's 401/403 ride the
   * global interceptor + this toast). Without a request the legacy (export) event serves the
   * screens whose exports predate the engine.
   */
  async runExport(): Promise<void> {
    if (this.loading || this.exporting || this.engineExporting) {
      return;
    }
    if (!this.exportRequest) {
      this.export.emit();
      return;
    }

    // Review P27 2026-08-26: the producer is a screen-supplied closure — a THROW inside it
    // (stale filter state, aborted composition) must land in this toast, not escape as an
    // unhandled rejection with the spinner never cleared.
    let request: ReportExportRequest | null;
    try {
      request = this.exportRequest();
    } catch (error: any) {
      this.notification.error(
        error?.error?.message || error?.message || this.translate.instant('reports.export.failed')
      );
      return;
    }
    if (!request) {
      this.notification.info(this.translate.instant('reports.nothingToExport'));
      return;
    }

    this.engineExporting = true;
    try {
      const result = await this.reportExportService.exportAll(request);
      if (result.exportedCount === 0) {
        this.notification.info(this.translate.instant('reports.nothingToExport'));
      } else if (result.truncated) {
        // Review P21 2026-08-26: the cap/short-delivery truncation is declared on the
        // manifest sheet AND told here — a silent partial export reads as complete.
        this.notification.warning(this.translate.instant('reports.export.truncationNotice'));
      }
    } catch {
      this.notification.error(this.translate.instant('reports.export.failed'));
    } finally {
      this.engineExporting = false;
    }
  }
}
