/**
 * Non-Renewed Reports Component
 * Implements UC-ORR-14 (§14.U.14 الأيتام بدون تقرير مجدد) — the chase list before a
 * payment run: coded orphans of the (token-scoped) charity with NO accepted report
 * covering the window. Only an accepted report clears an orphan (BR-11).
 *
 * One endpoint with a countOnly flag (the legacy four-shape realisation collapsed):
 * POST /api/Reports/non-renewed-reports. Read-only — nothing here writes.
 */

import { Component, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { TranslateModule, TranslateService } from '@ngx-translate/core';

import { ReportService } from '../../reports/services/report.service';
import { NonRenewedOrphanRow, NonRenewedReportsRequest, NonRenewedReportsResult } from '../../reports/models/report.model';
import { LoadingComponent } from '../../../shared/components/loading/loading.component';
import { EmptyStateComponent } from '../../../shared/components/empty-state/empty-state.component';
import { PageHeaderComponent } from '../../../shared/components/page-header/page-header.component';
import { NotificationService } from '../../../core/services/notification.service';
import { AuthService } from '../../../core/services/auth.service';
import { CharityService } from '../../charities/services/charity.service';
import { CharityDto } from '../../charities/models/charity.model';

/** Server page of 100; the chase list drains pages up to a 2000-row cap (9-12 precedent). */
const NON_RENEWED_PAGE_SIZE = 100;
const NON_RENEWED_ROW_CAP = 2000;

@Component({
  selector: 'app-non-renewed-reports',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [
    CommonModule,
    FormsModule,
    RouterLink,
    TranslateModule,
    LoadingComponent,
    EmptyStateComponent,
    PageHeaderComponent
  ],
  templateUrl: './non-renewed-reports.component.html',
  styleUrls: ['./non-renewed-reports.component.scss']
})
export class NonRenewedReportsComponent {
  loading = false;

  // §14.U.14 criteria — the window is the semantic (batchId deferred to EP-10)
  isHeadOffice = false;
  charities: CharityDto[] = [];
  charityId = '';
  dateFrom = '';
  dateTo = '';
  countOnly = false;

  // Result
  rows: NonRenewedOrphanRow[] = [];
  count = 0;
  ranOnce = false;

  // Client-side paging over the returned page set (server page = 100 rows)
  currentPage = 1;
  pageSize = 20;

  constructor(
    private reportService: ReportService,
    private charityService: CharityService,
    public auth: AuthService,
    private notification: NotificationService,
    private translate: TranslateService,
    private cdr: ChangeDetectorRef
  ) {
    this.isHeadOffice = this.auth.hasRole('SuperAdmin') || this.auth.hasRole('Admin');
    if (this.isHeadOffice) {
      // Review P30e 2026-08-24: OnPush — the async dropdown fill happens outside
      // Angular's zone-visible bindings; without markForCheck the list stays empty.
      this.charityService.getCharities({ pageNumber: 1, pageSize: 500 }).subscribe({
        next: result => {
          this.charities = result.items ?? [];
          this.cdr.markForCheck();
        },
        error: () => {
          this.charities = [];
          this.cdr.markForCheck();
        }
      });
    }
  }

  get totalPages(): number {
    return Math.max(1, Math.ceil(this.rows.length / this.pageSize));
  }

  get pagedRows(): NonRenewedOrphanRow[] {
    const start = (this.currentPage - 1) * this.pageSize;
    return this.rows.slice(start, start + this.pageSize);
  }

  /** تشغيل the chase list — countOnly returns just the count tile (legacy _Number screens). */
  run(): void {
    if (!this.dateFrom || !this.dateTo) {
      this.notification.warning(this.translate.instant('periodicReports.nonRenewed.windowRequired'));
      return;
    }
    if (this.dateFrom > this.dateTo) {
      this.notification.warning(this.translate.instant('orphanReports.generate.dateOrderInvalid'));
      return;
    }

    this.loading = true;
    this.currentPage = 1;
    this.ranOnce = true;
    this.cdr.markForCheck();

    // countOnly asks for the number alone; otherwise pull the rows in pages of 100.
    const baseRequest: NonRenewedReportsRequest = {
      dateFrom: this.dateFrom,
      dateTo: this.dateTo,
      charityId: this.isHeadOffice && this.charityId ? this.charityId : undefined,
      countOnly: this.countOnly || undefined
    };

    // Review P40 2026-08-24: only page 1 was fetched — a matched set over 100 rows
    // was silently truncated and the export shipped the partial file as complete.
    // Drain pages of 100 up to the 2000-row cap (9-12 precedent); the count tile
    // keeps the server's matched-set size, so a capped set is visible, not silent.
    // countOnly short-circuits naturally: the server answers with an empty page.
    const fetchPage = (page: number): Promise<NonRenewedReportsResult> =>
      new Promise<NonRenewedReportsResult>((resolve, reject) =>
        this.reportService
          .getNonRenewedReports({ ...baseRequest, page, pageSize: NON_RENEWED_PAGE_SIZE })
          .subscribe({ next: resolve, error: reject })
      );

    const collected: NonRenewedOrphanRow[] = [];
    const drain = (page: number): void => {
      fetchPage(page).then(result => {
        const items = result.items ?? [];
        collected.push(...items);
        this.count = result.count ?? collected.length;
        if (items.length === NON_RENEWED_PAGE_SIZE && collected.length < NON_RENEWED_ROW_CAP) {
          drain(page + 1);
          return;
        }
        this.rows = collected.slice(0, NON_RENEWED_ROW_CAP);
        this.loading = false;
        this.cdr.markForCheck();
      }).catch(error => {
        console.error('Error loading non-renewed chase list:', error);
        this.loading = false;
        this.notification.error(
          error?.message || this.translate.instant('periodicReports.nonRenewed.loadFailed')
        );
        this.cdr.markForCheck();
      });
    };
    drain(1);
  }

  onPageChange(page: number): void {
    // Review P42d 2026-08-24: clamp — a pager click past the last page would
    // otherwise blank the grid (and a negative slice start wraps the array).
    this.currentPage = Math.min(Math.max(1, page), this.totalPages);
  }

  serial(index: number): number {
    return (this.currentPage - 1) * this.pageSize + index + 1;
  }

  trackByOrphanId(_index: number, row: NonRenewedOrphanRow): string {
    return row.orphanId;
  }

  trackByCharityId(_index: number, charity: CharityDto): string {
    return charity.id;
  }

  /** Review P51b 2026-08-24 (same as 9-11): page-number list trackBy. */
  trackByPageIndex(index: number): number {
    return index;
  }

  // ========== استخراج — client-side ExcelJS (9-11 builder pattern) ==========

  exportData(): void {
    if (this.rows.length === 0) {
      this.notification.info(this.translate.instant('periodicReports.nonRenewed.nothingToExport'));
      return;
    }

    import('exceljs').then(({ default: ExcelJS }) => {
      const workbook = new ExcelJS.Workbook();
      const sheet = workbook.addWorksheet(this.translate.instant('periodicReports.nonRenewed.title'));

      sheet.addRow([
        this.translate.instant('periodicReports.nonRenewed.columns.serial'),
        this.translate.instant('orphanReports.generate.columns.orphanCode'),
        this.translate.instant('orphanReports.generate.columns.orphanName'),
        this.translate.instant('periodicReports.nonRenewed.columns.familyCode'),
        this.translate.instant('orphanReports.generate.columns.charity'),
        this.translate.instant('periodicReports.nonRenewed.columns.lastReportDate')
      ]);
      this.rows.forEach((r, i) => sheet.addRow([
        i + 1,
        r.code || '—',
        r.fullName || '—',
        r.familyCode || '—',
        r.charityName || '—',
        r.lastReportDate ? new Date(r.lastReportDate).toISOString().slice(0, 10) : '—'
      ]));

      workbook.xlsx.writeBuffer().then(buffer => {
        const blob = new Blob([buffer], {
          type: 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet'
        });
        const url = window.URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = `non_renewed_${new Date().toISOString().slice(0, 10)}.xlsx`;
        document.body.appendChild(a);
        a.click();
        document.body.removeChild(a);
        window.URL.revokeObjectURL(url);
      }).catch(() =>
        this.notification.error(this.translate.instant('periodicReports.nonRenewed.exportFailed'))
      );
    }).catch(() =>
      this.notification.error(this.translate.instant('periodicReports.nonRenewed.exportFailed'))
    );
  }
}
