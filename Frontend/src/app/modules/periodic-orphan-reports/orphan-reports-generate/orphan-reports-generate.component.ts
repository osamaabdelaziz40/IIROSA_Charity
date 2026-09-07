/**
 * Orphan Reports Generate Component
 * Implements UC-ORR-11 (§14.U.11): extract detailed report data تفاصيل التقارير.
 *
 * Criteria: الجمعية (HQ-gated) · رقم التقرير · أكواد toggle · من/الي تاريخ.
 * POST /api/OrphanReports/generate returns the detailed report rows (capped at
 * 1000); استخراج البيانات builds the ExcelJS workbook from the loaded rows.
 */

import { Component, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { TranslateModule, TranslateService } from '@ngx-translate/core';

import { OrphanReportService } from '../services/orphan-report.service';
import { OrphanReportDetailRow, OrphanReportFilterDto } from '../models/periodic-orphan-report.model';
import { LoadingComponent } from '../../../shared/components/loading/loading.component';
import { EmptyStateComponent } from '../../../shared/components/empty-state/empty-state.component';
import { PageHeaderComponent } from '../../../shared/components/page-header/page-header.component';
import { NotificationService } from '../../../core/services/notification.service';
import { AuthService } from '../../../core/services/auth.service';
import { CharityService } from '../../charities/services/charity.service';
import { CharityDto } from '../../charities/models/charity.model';

@Component({
  selector: 'app-orphan-reports-generate',
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
  templateUrl: './orphan-reports-generate.component.html',
  styleUrls: ['./orphan-reports-generate.component.scss']
})
export class OrphanReportsGenerateComponent {
  loading = false;
  exporting = false;

  // §14.U.11 criteria
  isHeadOffice = false;
  charities: CharityDto[] = [];
  charityId = '';
  reportNo = '';
  codesOnly = false;
  dateFrom = '';
  dateTo = '';

  rows: OrphanReportDetailRow[] = [];
  totalCount = 0;

  // Client-side paging over the capped extract batch
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
      // Review P30c 2026-08-24: OnPush — the async dropdown fill happens outside
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

  get pagedRows(): OrphanReportDetailRow[] {
    const start = (this.currentPage - 1) * this.pageSize;
    return this.rows.slice(start, start + this.pageSize);
  }

  /** تشغيل the extract — no stored data changes (AC 1). */
  generate(): void {
    if (this.dateFrom && this.dateTo && this.dateFrom > this.dateTo) {
      this.notification.warning(this.translate.instant('orphanReports.generate.dateOrderInvalid'));
      return;
    }

    this.loading = true;
    this.currentPage = 1;
    this.cdr.markForCheck();

    // Review P58 2026-08-24: restore the typed DTO — the previous `any` silently
    // accepted property-name drift against the service contract.
    const filter: OrphanReportFilterDto = {
      fromDate: this.dateFrom || '0001-01-01T00:00:00Z',
      toDate: this.dateTo || '0001-01-01T00:00:00Z',
      charityId: this.isHeadOffice && this.charityId ? this.charityId : undefined,
      reportNo: this.reportNo.trim() || undefined
    };

    this.orphanReportService.generateReport(filter).subscribe({
      next: result => {
        this.rows = result.reports ?? [];
        this.totalCount = result.reportsTotalCount ?? this.rows.length;
        this.loading = false;
        this.cdr.markForCheck();
      },
      error: error => {
        console.error('Error generating extract:', error);
        this.loading = false;
        this.notification.error(
          error?.message || this.translate.instant('orphanReports.generate.loadFailed')
        );
        this.cdr.markForCheck();
      }
    });
  }

  onPageChange(page: number): void {
    // Review P42b 2026-08-24: clamp — a pager click past the last page would
    // otherwise blank the grid (and a negative slice start wraps the array).
    this.currentPage = Math.min(Math.max(1, page), this.totalPages);
  }

  serial(index: number): number {
    return (this.currentPage - 1) * this.pageSize + index + 1;
  }

  trackByReportId(_index: number, row: OrphanReportDetailRow): string {
    return row.reportId;
  }

  trackByCharityId(_index: number, charity: CharityDto): string {
    return charity.id;
  }

  // ========== استخراج البيانات — client-side ExcelJS (16-1 precedent) ==========

  exportData(): void {
    if (this.rows.length === 0) {
      // AC 5: told there is nothing to produce — no empty file
      this.notification.info(this.translate.instant('orphanReports.generate.nothingToExport'));
      return;
    }

    // Review P43b 2026-08-24: the button binds [disabled]="exporting" — the flag
    // was never set, so a double-click ran the workbook build twice.
    this.exporting = true;
    this.cdr.markForCheck();

    import('exceljs').then(({ default: ExcelJS }) => {
      const workbook = new ExcelJS.Workbook();
      const sheet = workbook.addWorksheet(this.translate.instant('orphanReports.generate.title'));

      if (this.codesOnly) {
        sheet.addRow([
          this.translate.instant('orphanReports.generate.columns.orphanCode'),
          this.translate.instant('orphanReports.generate.columns.reportNo')
        ]);
        this.rows.forEach(r => sheet.addRow([r.orphanCode || '—', r.reportNo || '—']));
      } else {
        sheet.addRow([
          this.translate.instant('orphanReports.generate.columns.orphanCode'),
          this.translate.instant('orphanReports.generate.columns.orphanName'),
          this.translate.instant('orphanReports.generate.columns.charity'),
          this.translate.instant('orphanReports.generate.columns.reportNo'),
          this.translate.instant('orphanReports.generate.columns.reportDate'),
          this.translate.instant('orphanReports.generate.columns.period'),
          this.translate.instant('orphanReports.generate.columns.status'),
          this.translate.instant('orphanReports.generate.columns.schoolType'),
          this.translate.instant('orphanReports.generate.columns.school'),
          this.translate.instant('orphanReports.generate.columns.faculty'),
          this.translate.instant('orphanReports.generate.columns.specialization'),
          this.translate.instant('orphanReports.generate.columns.grade'),
          this.translate.instant('orphanReports.generate.columns.degree'),
          this.translate.instant('orphanReports.generate.columns.educationalLevel'),
          this.translate.instant('orphanReports.generate.columns.medicalStatus'),
          this.translate.instant('orphanReports.generate.columns.disease'),
          this.translate.instant('orphanReports.generate.columns.disability'),
          this.translate.instant('orphanReports.generate.columns.marriageDate'),
          this.translate.instant('orphanReports.generate.columns.deathDate'),
          this.translate.instant('orphanReports.generate.columns.refuseReason')
        ]);
        this.rows.forEach(r => sheet.addRow([
          r.orphanCode || '—',
          r.orphanName || '—',
          r.charityName || '—',
          r.reportNo || '—',
          this.fmtDate(r.reportDate),
          this.fmtDate(r.reportPeriodFrom) + ' - ' + this.fmtDate(r.reportPeriodTo),
          this.translate.instant('periodicReports.status.' + (r.reviewStatus || 'pending').toLowerCase()),
          r.schoolType || '—',
          r.school || '—',
          r.faculty || '—',
          r.specialization || '—',
          r.grade || '—',
          r.educationDegree || '—',
          r.educationalLevelName || '—',
          r.medicalStatus || '—',
          r.disease || '—',
          r.disability || '—',
          this.fmtDate(r.marriageDate) || '—',
          this.fmtDate(r.deathDate) || '—',
          r.isRefused ? (r.refuseReason || '—') : '—'
        ]));
      }

      // Review P43b: returned so the outer .finally() waits for the download —
      // otherwise the flag resets while the workbook is still writing.
      return workbook.xlsx.writeBuffer().then(buffer => {
        const blob = new Blob([buffer], {
          type: 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet'
        });
        const url = window.URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = `report_details_${new Date().toISOString().slice(0, 10)}.xlsx`;
        document.body.appendChild(a);
        a.click();
        document.body.removeChild(a);
        window.URL.revokeObjectURL(url);
      });
    }).catch(() =>
      this.notification.error(this.translate.instant('orphanReports.generate.exportFailed'))
    ).finally(() => {
      this.exporting = false;
      this.cdr.markForCheck();
    });
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

  /** Review P51b 2026-08-24: page-number list trackBy (CLAUDE.md: trackBy on every *ngFor). */
  trackByPageIndex(index: number): number {
    return index;
  }
}
