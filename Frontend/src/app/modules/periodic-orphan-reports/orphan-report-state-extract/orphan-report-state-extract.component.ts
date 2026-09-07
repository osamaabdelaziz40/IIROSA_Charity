/**
 * Orphan Report State Extract Component
 * Implements UC-ORR-12 (§14.U.12 التقارير المعتمدة) — the accepted-reports extract
 * opened from the §14.S.1 register command. Built as the shared state-filtered
 * extract surface: 9-13 (refused) rides the same component with state "refused".
 *
 * The state predicate is server-side: GET /api/PeriodicOrphanReports/approved pins
 * Reviewed && IsAccepted (a refused row can never leak in); charity/date/reportNo
 * arrive as ordinary filters under that pin.
 */

import { Component, ChangeDetectionStrategy, ChangeDetectorRef, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { TranslateModule, TranslateService } from '@ngx-translate/core';

import { PeriodicOrphanReportService } from '../services/periodic-orphan-report.service';
import { PeriodicOrphanReportListDto, PeriodicOrphanReportPagedResult } from '../models/periodic-orphan-report.model';
import { LoadingComponent } from '../../../shared/components/loading/loading.component';
import { EmptyStateComponent } from '../../../shared/components/empty-state/empty-state.component';
import { PageHeaderComponent } from '../../../shared/components/page-header/page-header.component';
import { NotificationService } from '../../../core/services/notification.service';
import { AuthService } from '../../../core/services/auth.service';
import { CharityService } from '../../charities/services/charity.service';
import { CharityDto } from '../../charities/models/charity.model';

/** Pages of 100, capped at 2000 rows per extract batch (9-9 export precedent). */
const EXTRACT_PAGE_SIZE = 100;
const EXTRACT_ROW_CAP = 2000;

@Component({
  selector: 'app-orphan-report-state-extract',
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
  templateUrl: './orphan-report-state-extract.component.html',
  styleUrls: ['./orphan-report-state-extract.component.scss']
})
export class OrphanReportStateExtractComponent implements OnInit {
  /** "accepted" (UC-ORR-12) or "refused" (UC-ORR-13) — sets title, endpoint, refuse column. */
  state: 'accepted' | 'refused' = 'accepted';

  loading = false;

  // Criteria (§14.S.1 command → extract criteria)
  isHeadOffice = false;
  charities: CharityDto[] = [];
  charityId = '';
  reportNo = '';
  dateFrom = '';
  dateTo = '';

  rows: PeriodicOrphanReportListDto[] = [];
  totalCount = 0;

  // Client-side paging over the loaded batch
  currentPage = 1;
  pageSize = 20;

  constructor(
    private route: ActivatedRoute,
    private reportService: PeriodicOrphanReportService,
    private charityService: CharityService,
    public auth: AuthService,
    private notification: NotificationService,
    private translate: TranslateService,
    private cdr: ChangeDetectorRef
  ) {
    this.isHeadOffice = this.auth.hasRole('SuperAdmin') || this.auth.hasRole('Admin');
  }

  ngOnInit(): void {
    const state = this.route.snapshot.paramMap.get('state');
    if (state === 'refused') {
      this.state = 'refused';
    } else if (state !== 'accepted') {
      // Review P25 2026-08-24: an unknown :state (e.g. /extract/pending) used to
      // silently render the accepted extract under a state-looking URL — the guard
      // keeps 'accepted'/'refused' the only two renderable states and says so.
      console.warn(`[StateExtract] Unknown extract state '${state}' — rendering the accepted extract.`);
    }

    if (this.isHeadOffice) {
      // Review P30d 2026-08-24: OnPush — the async dropdown fill happens outside
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

  get i18nPrefix(): string {
    return `periodicReports.extract.${this.state}`;
  }

  get totalPages(): number {
    return Math.max(1, Math.ceil(this.rows.length / this.pageSize));
  }

  get pagedRows(): PeriodicOrphanReportListDto[] {
    const start = (this.currentPage - 1) * this.pageSize;
    return this.rows.slice(start, start + this.pageSize);
  }

  /** تشغيل — pages of 100 until the matched set is loaded (cap 2000). */
  run(): void {
    if (this.dateFrom && this.dateTo && this.dateFrom > this.dateTo) {
      this.notification.warning(this.translate.instant('orphanReports.generate.dateOrderInvalid'));
      return;
    }

    this.loading = true;
    this.currentPage = 1;
    this.cdr.markForCheck();

    const filter: any = {
      reportNo: this.reportNo.trim() || undefined,
      reportDateFrom: this.dateFrom || undefined,
      reportDateTo: this.dateTo || undefined,
      charityId: this.isHeadOffice && this.charityId ? this.charityId : undefined
    };

    // Review P39 2026-08-24: resolve the whole paged result — the items alone threw
    // away the server's totalCount, so the "capped at N of M" indicator could only
    // ever echo the rows held client-side, never the true matched-set size.
    const fetchPage = (page: number): Promise<PeriodicOrphanReportPagedResult> =>
      new Promise<PeriodicOrphanReportPagedResult>((resolve, reject) => {
        const request = { ...filter, pageNumber: page, pageSize: EXTRACT_PAGE_SIZE };
        const call = this.state === 'refused'
          ? this.reportService.getRejectedReports(request)
          : this.reportService.getApprovedReports(request);
        call.subscribe({ next: resolve, error: reject });
      });

    const collected: PeriodicOrphanReportListDto[] = [];
    let serverTotal: number | undefined;
    const drain = (page: number): void => {
      fetchPage(page).then(result => {
        const items = result.items ?? [];
        collected.push(...items);
        // Every page carries the matched-set total; the cap only bounds what we hold.
        serverTotal = result.totalCount;
        if (items.length === EXTRACT_PAGE_SIZE && collected.length < EXTRACT_ROW_CAP) {
          drain(page + 1);
          return;
        }
        this.rows = collected.slice(0, EXTRACT_ROW_CAP);
        this.totalCount = serverTotal ?? collected.length;
        this.loading = false;
        this.cdr.markForCheck();
      }).catch(error => {
        console.error(`Error loading ${this.state} extract:`, error);
        this.loading = false;
        this.notification.error(
          error?.message || this.translate.instant(`${this.i18nPrefix}.loadFailed`)
        );
        this.cdr.markForCheck();
      });
    };
    drain(1);
  }

  onPageChange(page: number): void {
    // Review P42c 2026-08-24: clamp — a pager click past the last page would
    // otherwise blank the grid (and a negative slice start wraps the array).
    this.currentPage = Math.min(Math.max(1, page), this.totalPages);
  }

  serial(index: number): number {
    return (this.currentPage - 1) * this.pageSize + index + 1;
  }

  trackByReportId(_index: number, row: PeriodicOrphanReportListDto): string {
    return row.id;
  }

  trackByCharityId(_index: number, charity: CharityDto): string {
    return charity.id;
  }

  /** Review P51b 2026-08-24: page-number list trackBy (CLAUDE.md: trackBy on every *ngFor). */
  trackByPageIndex(index: number): number {
    return index;
  }

  // ========== استخراج — client-side ExcelJS (9-11 detailed columns) ==========

  exportData(): void {
    if (this.rows.length === 0) {
      // "nothing to produce", not an empty file
      this.notification.info(this.translate.instant(`${this.i18nPrefix}.nothingToExport`));
      return;
    }

    import('exceljs').then(({ default: ExcelJS }) => {
      const workbook = new ExcelJS.Workbook();
      const sheet = workbook.addWorksheet(this.translate.instant(`${this.i18nPrefix}.title`));

      const headers = [
        this.translate.instant('orphanReports.generate.columns.orphanCode'),
        this.translate.instant('orphanReports.generate.columns.orphanName'),
        this.translate.instant('orphanReports.generate.columns.charity'),
        this.translate.instant('orphanReports.generate.columns.reportNo'),
        this.translate.instant('orphanReports.generate.columns.reportDate'),
        this.translate.instant('orphanReports.generate.columns.period'),
        this.translate.instant('orphanReports.generate.columns.status'),
        this.translate.instant('orphanReports.generate.columns.schoolType'),
        this.translate.instant('orphanReports.generate.columns.school'),
        this.translate.instant('orphanReports.generate.columns.degree'),
        this.translate.instant('orphanReports.generate.columns.educationalLevel'),
        this.translate.instant('orphanReports.generate.columns.medicalStatus'),
        this.translate.instant('orphanReports.generate.columns.marriageDate'),
        this.translate.instant('orphanReports.generate.columns.deathDate')
      ];
      if (this.state === 'refused') {
        headers.push(this.translate.instant('orphanReports.generate.columns.refuseReason'));
      }
      sheet.addRow(headers);

      this.rows.forEach(r => {
        const cells = [
          r.orphanCode || '—',
          r.orphanName || '—',
          r.charityName || '—',
          r.reportNo || '—',
          this.fmtDate(r.reportDate),
          this.fmtDate(r.reportPeriodFrom) + ' - ' + this.fmtDate(r.reportPeriodTo),
          this.translate.instant('periodicReports.status.'
            + (r.reviewStatus || 'pending').toLowerCase()),
          r.schoolType || '—',
          r.school || '—',
          r.educationDegree || '—',
          r.educationalLevelName || '—',
          r.medicalStatus || '—',
          this.fmtDate(r.marriageDate) || '—',
          this.fmtDate(r.deathDate) || '—'
        ];
        if (this.state === 'refused') {
          cells.push(r.refuseReason || '—');
        }
        sheet.addRow(cells);
      });

      workbook.xlsx.writeBuffer().then(buffer => {
        const blob = new Blob([buffer], {
          type: 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet'
        });
        const url = window.URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = `${this.state}_reports_${new Date().toISOString().slice(0, 10)}.xlsx`;
        document.body.appendChild(a);
        a.click();
        document.body.removeChild(a);
        window.URL.revokeObjectURL(url);
      }).catch(() =>
        this.notification.error(this.translate.instant(`${this.i18nPrefix}.exportFailed`))
      );
    }).catch(() =>
      this.notification.error(this.translate.instant(`${this.i18nPrefix}.exportFailed`))
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
