import { Component, OnDestroy, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule } from '@angular/forms';
import { Subject, firstValueFrom, takeUntil } from 'rxjs';
import { TranslateModule, TranslateService } from '@ngx-translate/core';

import { ReportViewerComponent } from '../report-viewer/report-viewer.component';
import { DropDownComponent } from '../../../shared/components/drop-down/drop-down.component';
import { ReportService } from '../services/report.service';
import { ReportExportService } from '../services/report-export.service';
import { CharityService } from '../../charities/services/charity.service';
import { AuthService } from '../../../core/services/auth.service';
import { AttachmentService } from '../../../core/services/attachment.service';
import { NotificationService } from '../../../core/services/notification.service';
import { OrphanFileManifestRow } from '../models/report.model';

/** The §23.S.18 screen's two grids — one pager, two result sets. */
type GridKind = 'photos' | 'certificates';

/**
 * UC-RPT-24 + UC-RPT-25 (§23.S.18 صور الأيتام وصور الشهادات) — the twin image-manifest
 * grids over accepted periodic reports dated in [من تاريخ, الى تاريخ]: the photos grid
 * (18-24) and the certificates grid (18-25) share the filter state, the pager and the
 * workbook builder; only the endpoint's kind selector differs. من تاريخ is mandatory;
 * الجمعية is an HQ-only narrow. Thumbnails and downloads stream through the live
 * attachment endpoint as authenticated blobs bound to object URLs — an <img src> cannot
 * carry the Bearer header (9-16 defect-2 ruling).
 */
@Component({
  selector: 'app-orphan-files',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    TranslateModule,
    ReportViewerComponent,
    DropDownComponent
  ],
  templateUrl: './orphan-files.component.html',
  styleUrls: ['./orphan-files.component.scss']
})
export class OrphanFilesComponent implements OnInit, OnDestroy {
  /** The active grid — the shared pager and export command act on it. */
  activeGrid: GridKind = 'photos';

  rows: OrphanFileManifestRow[] = [];
  totalCount = 0;
  hasRun = false;

  certRows: OrphanFileManifestRow[] = [];
  certTotalCount = 0;
  certHasRun = false;

  loading = false;
  exporting = false;

  currentPage = 1;
  pageSize = 20;

  charityOptions: Array<{ id: string; name: string }> = [];
  isHeadOffice = false;

  /** attachment-key → object URL, per grid (the rendered page's thumbnails only). */
  thumbUrls: Record<string, string> = {};
  certThumbUrls: Record<string, string> = {};

  /** The exports page through the whole manifest — the endpoints' validator caps PageSize at 100. */
  private readonly exportPageSize = 100;

  /**
   * Review P27 2026-08-26: one page used to fire ~20 simultaneous authenticated downloads —
   * enough parallel requests to saturate the browser's per-host connection budget and stall
   * the page's own calls. Thumbnails now stream in small concurrent batches.
   */
  private readonly thumbConcurrency = 4;

  filterForm: FormGroup;

  private readonly destroy$ = new Subject<void>();

  constructor(
    private fb: FormBuilder,
    private reportService: ReportService,
    private reportExportService: ReportExportService,
    private charityService: CharityService,
    private authService: AuthService,
    private attachmentService: AttachmentService,
    private notification: NotificationService,
    private translate: TranslateService
  ) {
    this.filterForm = this.fb.group({
      charityId: [''],
      dateFrom: [''],
      dateTo: ['']
    });
  }

  ngOnInit(): void {
    // الجمعية is an HQ-only narrow — hidden for charity callers (AC 5).
    this.isHeadOffice = this.authService.hasAnyRole(['SuperAdmin', 'Admin']);
    if (this.isHeadOffice) {
      this.loadCharities();
    }
  }

  ngOnDestroy(): void {
    this.releaseThumbs(this.thumbUrls);
    this.releaseThumbs(this.certThumbUrls);
    this.destroy$.next();
    this.destroy$.complete();
  }

  /** الجمعية — real charities endpoint only; كافة الجهات all-option. */
  private loadCharities(): void {
    this.charityService.getCharities({ pageNumber: 1, pageSize: 500 })
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: response => {
          this.charityOptions = [
            { id: '', name: this.translate.instant('reports.orphanFiles.allCharities') },
            ...(response.items || []).map(c => ({ id: c.id, name: c.name }))
          ];
          // Review P26 2026-08-26: a capped lookup says so — a silently truncated dropdown hides choices.
          if ((response.totalCount || 0) > (response.items || []).length) {
            this.notification.warning(this.translate.instant('reports.lookup.truncated'));
          }
        },
        error: () => console.error('Error loading charities')
      });
  }

  // ==================== commands (§23.S.18) ====================

  /** GetNext/GetPrev — the ONE pager pages the ACTIVE grid against its own endpoint. */
  switchGrid(grid: GridKind): void {
    if (this.activeGrid === grid || this.loading || this.exporting) {
      return;
    }
    this.activeGrid = grid;
    this.currentPage = 1;
    this.runSearch();
  }

  /** بحث — runs the ACTIVE grid's manifest query. A read; nothing stored changes. */
  runSearch(): void {
    if (this.loading || this.exporting) {
      return;
    }
    this.loading = true;
    const filter = this.buildFilter();
    const certificates = this.activeGrid === 'certificates';
    const fetch = certificates
      ? this.reportService.getCertificateFiles(filter)
      : this.reportService.getOrphanFiles(filter);

    fetch.pipe(takeUntil(this.destroy$))
      .subscribe({
        next: result => {
          const items = result.items || [];
          const total = result.totalCount || 0;
          if (certificates) {
            this.certRows = items;
            this.certTotalCount = total;
            this.certHasRun = true;
            this.loadThumbs(items, false);
          } else {
            this.rows = items;
            this.totalCount = total;
            this.hasRun = true;
            this.loadThumbs(items, true);
          }
          this.currentPage = result.page || this.currentPage;
          this.loading = false;
        },
        error: (httpError: any) => {
          this.loading = false;
          // Field-level refusals land on the offending control (missing من تاريخ — AC 3).
          // Review P27 2026-08-26: the control now carries the SERVER's reason (18-40 AC 4
          // sibling) — { server: true } rendered the generic required line for every refusal
          // — and merges instead of clobbering any coexisting flag.
          const errors = httpError?.details || {};
          if (errors.DateFrom) {
            this.filterForm.get('dateFrom')?.setErrors({
              ...this.filterForm.get('dateFrom')?.errors,
              server: this.serverMessage(errors.DateFrom)
            });
            this.filterForm.get('dateFrom')?.markAsTouched();
          }
          if (errors.DateTo) {
            this.filterForm.get('dateTo')?.setErrors({
              ...this.filterForm.get('dateTo')?.errors,
              server: this.serverMessage(errors.DateTo)
            });
            this.filterForm.get('dateTo')?.markAsTouched();
          }
          this.notification.error(
            httpError?.message || this.translate.instant('reports.orphanFiles.searchFailed')
          );
        }
      });
  }

  search(): void {
    if (this.loading || this.exporting) {
      return;
    }
    // من تاريخ is mandatory (§23.S.18 table) — refused client-side, flagged on the field.
    // Review P27 2026-08-26: merge, never clobber — a blanket setErrors also wiped a
    // coexisting server reason from a previously refused submit.
    const dateFrom = this.filterForm.get('dateFrom');
    if (!dateFrom?.value) {
      dateFrom?.setErrors({ ...dateFrom?.errors, required: true });
      dateFrom?.markAsTouched();
      this.notification.info(this.translate.instant('reports.orphanFiles.dateFromRequired'));
      return;
    }
    // Review P27 2026-08-26: clear ONLY the required flag — a server reason survives.
    const merged = { ...dateFrom?.errors };
    delete merged['required'];
    dateFrom?.setErrors(Object.keys(merged).length ? merged : null);
    this.currentPage = 1;
    this.runSearch();
  }

  onPageChange(page: number): void {
    if (page < 1) {
      return;
    }
    this.currentPage = page;
    this.runSearch();
  }

  /**
   * ExportReportData / ExportCertificatesData — the ACTIVE grid's workbook over the WHOLE
   * manifest, paging under the 100 cap. ExcelJS embeds images where feasible and hyperlinks
   * the rest (recorded decision); only the sheet title differs between the grids.
   */
  exportData(): void {
    if (this.loading || this.exporting) {
      return;
    }
    // Review P27 2026-08-26: same merge/clear rule as search() — a server reason on the
    // control survives a client-side required pass.
    const dateFrom = this.filterForm.get('dateFrom');
    if (!dateFrom?.value) {
      dateFrom?.setErrors({ ...dateFrom?.errors, required: true });
      dateFrom?.markAsTouched();
      this.notification.info(this.translate.instant('reports.orphanFiles.dateFromRequired'));
      return;
    }
    const mergedExport = { ...dateFrom?.errors };
    delete mergedExport['required'];
    dateFrom?.setErrors(Object.keys(mergedExport).length ? mergedExport : null);
    const certificates = this.activeGrid === 'certificates';
    if ((certificates ? this.certTotalCount : this.totalCount) === 0) {
      this.notification.info(this.translate.instant('reports.nothingToExport'));
      return;
    }

    this.exporting = true;
    const collected: OrphanFileManifestRow[] = [];
    const total = certificates ? this.certTotalCount : this.totalCount;
    const totalPages = Math.ceil(total / this.exportPageSize);

    const fetchNext = (page: number): void => {
      if (page > totalPages) {
        this.exporting = false;
        if (collected.length === 0) {
          this.notification.info(this.translate.instant('reports.nothingToExport'));
          return;
        }
        const v = this.filterForm.getRawValue();
        const scope = v.charityId ? String(v.charityId).slice(0, 8) : 'all';
        const from = String(v.dateFrom).slice(0, 10);
        const prefix = certificates ? 'certificate-images' : 'orphan-photographs';
        this.reportExportService.exportOrphanFiles(
          collected,
          `${prefix}_${scope}_${from}.xlsx`,
          certificates ? 'reports.orphanFiles.certificatesSection' : 'reports.orphanFiles.title'
        ).catch(() => this.notification.error(this.translate.instant(
          certificates ? 'reports.orphanFiles.certificatesExportFailed' : 'reports.orphanFiles.exportFailed'
        )));
        return;
      }

      const filter = { ...this.buildFilter(), page, pageSize: this.exportPageSize };
      const fetch = certificates
        ? this.reportService.getCertificateFiles(filter)
        : this.reportService.getOrphanFiles(filter);
      fetch.pipe(takeUntil(this.destroy$))
        .subscribe({
          next: result => {
            collected.push(...(result.items || []));
            fetchNext(page + 1);
          },
          error: (httpError: any) => {
            this.exporting = false;
            this.notification.error(httpError?.message || this.translate.instant(
              certificates ? 'reports.orphanFiles.certificatesExportFailed' : 'reports.orphanFiles.exportFailed'
            ));
          }
        });
    };

    fetchNext(1);
  }

  /**
   * downloadImageData / downloadCertificatesData — the per-row single-image download (the
   * stored bytes, real file name); identical row shape serves both grids.
   */
  downloadImage(row: OrphanFileManifestRow): void {
    this.attachmentService.download(row.attachmentId)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: blob => {
          const url = URL.createObjectURL(blob);
          const a = document.createElement('a');
          a.href = url;
          a.download = row.fileName || `orphan-image_${row.attachmentId}`;
          document.body.appendChild(a);
          a.click();
          document.body.removeChild(a);
          URL.revokeObjectURL(url);
        },
        error: () =>
          this.notification.error(this.translate.instant(
            this.activeGrid === 'certificates'
              ? 'reports.orphanFiles.certificatesDownloadFailed'
              : 'reports.orphanFiles.downloadFailed'
          ))
      });
  }

  // ==================== helpers ====================

  /**
   * Thumbnails for the rendered page only — authenticated blobs bound as object URLs.
   * Review P27 2026-08-26: fired as small concurrent batches (thumbConcurrency), not one
   * subscribe-per-row — a 20-row page used to open 20 parallel downloads.
   */
  private async loadThumbs(rows: OrphanFileManifestRow[], photos: boolean): Promise<void> {
    const current = photos ? this.thumbUrls : this.certThumbUrls;
    this.releaseThumbs(current);
    const fresh: Record<string, string> = {};
    if (photos) {
      this.thumbUrls = fresh;
    } else {
      this.certThumbUrls = fresh;
    }

    for (let i = 0; i < rows.length; i += this.thumbConcurrency) {
      if (this.destroy$.closed) {
        // Component destroyed between batches — stop scheduling; ngOnDestroy releases URLs.
        return;
      }
      const batch = rows.slice(i, i + this.thumbConcurrency);
      await Promise.all(batch.map(async row => {
        try {
          const blob = await firstValueFrom(
            this.attachmentService.getImage(row.attachmentId).pipe(takeUntil(this.destroy$))
          );
          fresh[this.rowKey(row)] = URL.createObjectURL(blob);
        } catch {
          // The row keeps its honest no-image marker — never a broken <img>.
        }
      }));
    }
  }

  /** Review P27 2026-08-26: ValidationProblemDetails detail values are string arrays — join. */
  private serverMessage(detail: unknown): string {
    return Array.isArray(detail) ? detail.join(' · ') : String(detail ?? '');
  }

  private releaseThumbs(map: Record<string, string>): void {
    for (const url of Object.values(map)) {
      URL.revokeObjectURL(url);
    }
  }

  private buildFilter(): { page: number; pageSize: number; charityId?: string; dateFrom: string; dateTo?: string } {
    const v = this.filterForm.getRawValue();
    const filter: { page: number; pageSize: number; charityId?: string; dateFrom: string; dateTo?: string } = {
      page: this.currentPage,
      pageSize: this.pageSize,
      dateFrom: String(v.dateFrom || '')
    };
    if (v.charityId) {
      filter.charityId = String(v.charityId);
    }
    if (v.dateTo) {
      filter.dateTo = String(v.dateTo);
    }
    return filter;
  }

  /** Row identity — an orphan can carry several images (one per accepted report). */
  rowKey(row: OrphanFileManifestRow): string {
    return `${row.orphanId}_${row.attachmentId}`;
  }

  /** 13-1 serial formula — continuous across pages. */
  serial(index: number): number {
    return (this.currentPage - 1) * this.pageSize + index + 1;
  }

  trackByRow(_index: number, row: OrphanFileManifestRow): string {
    return this.rowKey(row);
  }
}
