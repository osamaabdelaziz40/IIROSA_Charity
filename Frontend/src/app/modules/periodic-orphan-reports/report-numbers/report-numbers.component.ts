/**
 * Report Numbers Component
 * Implements UC-ORR-15 (§14.U.15 أرقام التقارير المضافة) — the ReportNo values created
 * in a window, for the register's reconcile-submissions purpose. An extract/read: the
 * epic's "Create a record" typing is a derivation artifact.
 *
 * Rides the 9-10 shared action: POST /api/OrphanReports/statistics with
 * includeReportNumbers + the date window (the grouped branch is untouched).
 */

import { Component, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { TranslateModule, TranslateService } from '@ngx-translate/core';

import { OrphanReportService } from '../services/orphan-report.service';
import { OrphanReportNumberRow } from '../models/periodic-orphan-report.model';
import { LoadingComponent } from '../../../shared/components/loading/loading.component';
import { EmptyStateComponent } from '../../../shared/components/empty-state/empty-state.component';
import { PageHeaderComponent } from '../../../shared/components/page-header/page-header.component';
import { NotificationService } from '../../../core/services/notification.service';
import { AuthService } from '../../../core/services/auth.service';
import { CharityService } from '../../charities/services/charity.service';
import { CharityDto } from '../../charities/models/charity.model';

@Component({
  selector: 'app-report-numbers',
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
  templateUrl: './report-numbers.component.html',
  styleUrls: ['./report-numbers.component.scss']
})
export class ReportNumbersComponent {
  loading = false;

  isHeadOffice = false;
  charities: CharityDto[] = [];
  charityId = '';
  dateFrom = '';
  dateTo = '';

  rows: OrphanReportNumberRow[] = [];
  totalCount = 0;
  unnumberedCount = 0;
  ranOnce = false;

  // Client-side paging over the capped numbers batch
  currentPage = 1;
  pageSize = 20;

  constructor(
    private orphanReportService: OrphanReportService,
    private charityService: CharityService,
    public auth: AuthService,
    private notification: NotificationService,
    private translate: TranslateService,
    private cdr: ChangeDetectorRef
  ) {
    this.isHeadOffice = this.auth.hasRole('SuperAdmin') || this.auth.hasRole('Admin');
    if (this.isHeadOffice) {
      // Review P30f 2026-08-24: OnPush — the async dropdown fill happens outside
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

  get pagedRows(): OrphanReportNumberRow[] {
    const start = (this.currentPage - 1) * this.pageSize;
    return this.rows.slice(start, start + this.pageSize);
  }

  /** تشغيل — the shared statistics action's numbers branch. */
  run(): void {
    if (!this.dateFrom || !this.dateTo) {
      this.notification.warning(this.translate.instant('periodicReports.reportNumbers.windowRequired'));
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

    this.orphanReportService.getOrphanStatistics({
      fromDate: this.dateFrom,
      toDate: this.dateTo,
      charityId: this.isHeadOffice && this.charityId ? this.charityId : undefined,
      includeReportNumbers: true
    }).subscribe({
      next: result => {
        this.rows = result.reportNumbers ?? [];
        this.totalCount = result.reportNumbersCount ?? this.rows.length;
        this.unnumberedCount = result.unnumberedReportsCount ?? 0;
        this.loading = false;
        this.cdr.markForCheck();
      },
      error: error => {
        console.error('Error loading report numbers:', error);
        this.loading = false;
        this.notification.error(
          error?.message || this.translate.instant('periodicReports.reportNumbers.loadFailed')
        );
        this.cdr.markForCheck();
      }
    });
  }

  onPageChange(page: number): void {
    // Review P42e 2026-08-24: clamp — a pager click past the last page would
    // otherwise blank the grid (and a negative slice start wraps the array).
    this.currentPage = Math.min(Math.max(1, page), this.totalPages);
  }

  serial(index: number): number {
    return (this.currentPage - 1) * this.pageSize + index + 1;
  }

  trackByReportId(_index: number, row: OrphanReportNumberRow): string {
    return row.reportId;
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
      // AC 4: the view says so — no zero-row workbook
      this.notification.info(this.translate.instant('periodicReports.reportNumbers.nothingToExport'));
      return;
    }

    import('exceljs').then(({ default: ExcelJS }) => {
      const workbook = new ExcelJS.Workbook();
      const sheet = workbook.addWorksheet(this.translate.instant('periodicReports.reportNumbers.title'));

      sheet.addRow([
        this.translate.instant('periodicReports.reportNumbers.columns.serial'),
        this.translate.instant('periodicReports.reportNumbers.columns.reportNo'),
        this.translate.instant('orphanReports.generate.columns.orphanCode'),
        this.translate.instant('orphanReports.generate.columns.orphanName'),
        this.translate.instant('orphanReports.generate.columns.reportDate'),
        this.translate.instant('periodicReports.reportNumbers.columns.addedOn'),
        this.translate.instant('orphanReports.generate.columns.status')
      ]);
      this.rows.forEach((r, i) => sheet.addRow([
        i + 1,
        r.reportNo || '—',
        r.orphanCode || '—',
        r.orphanName || '—',
        this.fmtDate(r.reportDate) || '—',
        this.fmtDate(r.createdOn) || '—',
        this.translate.instant('periodicReports.status.' + (r.reviewStatus || 'pending').toLowerCase())
      ]));

      workbook.xlsx.writeBuffer().then(buffer => {
        const blob = new Blob([buffer], {
          type: 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet'
        });
        const url = window.URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = `report_numbers_${new Date().toISOString().slice(0, 10)}.xlsx`;
        document.body.appendChild(a);
        a.click();
        document.body.removeChild(a);
        window.URL.revokeObjectURL(url);
      }).catch(() =>
        this.notification.error(this.translate.instant('periodicReports.reportNumbers.exportFailed'))
      );
    }).catch(() =>
      this.notification.error(this.translate.instant('periodicReports.reportNumbers.exportFailed'))
    );
  }

  private fmtDate(value?: string): string {
    if (!value) return '';
    const d = new Date(value);
    if (isNaN(d.getTime())) return '';
    // Review P47a 2026-08-24 (same defect as the 9-9 helper): toISOString shifts
    // a date back a day for UTC+3 evenings — format from local components instead.
    const pad = (n: number) => String(n).padStart(2, '0');
    return `${d.getFullYear()}-${pad(d.getMonth() + 1)}-${pad(d.getDate())}`;
  }
}
