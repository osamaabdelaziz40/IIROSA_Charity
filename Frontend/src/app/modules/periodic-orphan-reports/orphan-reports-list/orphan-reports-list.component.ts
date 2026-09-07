/**
 * Orphan Reports List Component
 * Implements UC-ORR-10 (§14.S.3): report statistics by group احصائيات عامة للأيتام.
 *
 * One row per (الحاله التعليميه × المرحله الدراسه) with الاناث / الذكور / الاجمالي,
 * aggregated server-side by POST /api/OrphanReports/statistics. الجمعية filter is
 * HQ-only — a charity caller is server-pinned to its own rows.
 */

import { Component, OnInit, OnDestroy, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule } from '@angular/forms';
import { Subject, takeUntil } from 'rxjs';
import { TranslateModule, TranslateService } from '@ngx-translate/core';

import { OrphanReportService } from '../services/orphan-report.service';
import { PeriodicOrphanReportService } from '../services/periodic-orphan-report.service';
import {
  OrphanStatisticsDto,
  OrphanReportGroupCountRow,
  OrphanReportStatusCounts
} from '../models/periodic-orphan-report.model';
import { LoadingComponent } from '../../../shared/components/loading/loading.component';
import { EmptyStateComponent } from '../../../shared/components/empty-state/empty-state.component';
import { PageHeaderComponent } from '../../../shared/components/page-header/page-header.component';
import { DropDownComponent } from '../../../shared/components/drop-down/drop-down.component';
import { NotificationService } from '../../../core/services/notification.service';
import { AuthService } from '../../../core/services/auth.service';
import { CharityService } from '../../charities/services/charity.service';
import { CharityDto } from '../../charities/models/charity.model';

@Component({
  selector: 'app-orphan-reports-list',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    TranslateModule,
    LoadingComponent,
    EmptyStateComponent,
    PageHeaderComponent,
    DropDownComponent
  ],
  templateUrl: './orphan-reports-list.component.html',
  styleUrls: ['./orphan-reports-list.component.scss']
})
export class OrphanReportsListComponent implements OnInit, OnDestroy {
  loading = false;
  exporting = false;

  // §14.S.3 filter — الجمعية drop-down (HQ only), 'all' = كافة الجهات
  isHeadOffice = false;
  charities: CharityDto[] = [];

  /** Filter form — the shared select2 drop-down binds to this (charity-list pattern). */
  filterForm: FormGroup;

  /** Select2 option array ({id, name}) fed to app-drop-down. */
  charityOptions: Array<{ id: string; name: string }> = [];

  statistics?: OrphanStatisticsDto;

  // 18-14 / UC-RPT-14 — status-grouped counts (مقبول / مرفوض / قيد الانتظار)
  statusCounts?: OrphanReportStatusCounts;

  // Review P31 2026-08-24: rapid الجمعية switches race — the token discards responses
  // from any request that is no longer the latest one.
  private loadToken = 0;

  /** Review P27 2026-08-26: the three ngOnInit/load subscriptions had no teardown — a
   *  response landing after route-leave still wrote state and toasts against a dead screen. */
  private readonly destroy$ = new Subject<void>();

  constructor(
    private fb: FormBuilder,
    private orphanReportService: OrphanReportService,
    private periodicReportService: PeriodicOrphanReportService,
    private charityService: CharityService,
    public auth: AuthService,
    private notification: NotificationService,
    private translate: TranslateService,
    private cdr: ChangeDetectorRef
  ) {
    this.filterForm = this.fb.group({
      charityId: ['all']
    });
  }

  ngOnInit(): void {
    this.isHeadOffice = this.auth.hasRole('SuperAdmin') || this.auth.hasRole('Admin');
    this.buildCharityOptions();
    if (this.isHeadOffice) {
      // Review P30b 2026-08-24: OnPush — the async dropdown fill happens outside
      // Angular's zone-visible bindings; without markForCheck the list stays empty.
      this.charityService.getCharities({ pageNumber: 1, pageSize: 500 })
        .pipe(takeUntil(this.destroy$))
        .subscribe({
        next: result => {
          this.charities = result.items ?? [];
          this.buildCharityOptions();
          this.cdr.markForCheck();
        },
        error: () => {
          this.charities = [];
          this.buildCharityOptions();
          this.cdr.markForCheck();
        }
      });
    }

    // Option labels are pre-translated (app-drop-down renders raw text) — the
    // translate pipe can't refresh them, so rebuild on a language switch.
    this.translate.onLangChange
      .pipe(takeUntil(this.destroy$))
      .subscribe(() => {
        this.buildCharityOptions();
        this.cdr.markForCheck();
      });

    this.loadStatistics();
  }

  /** كافة الجهات sentinel — 'all' maps to no charity pin in the request. */
  private buildCharityOptions(): void {
    this.charityOptions = [
      { id: 'all', name: this.translate.instant('periodicReports.filters.allCharities') },
      ...this.charities.map(charity => ({ id: charity.id, name: charity.name }))
    ];
  }

  /** getCharityData() — reload on بحث / الجمعية change. No paging on this screen. */
  onSearch(): void {
    this.loadStatistics();
  }

  /** مسح التصفية — back to كافة الجهات and reload. */
  clearFilters(): void {
    this.filterForm.reset({ charityId: 'all' });
    this.loadStatistics();
  }

  /** Whether a specific charity is pinned (drives the Clear button; HQ-only filter). */
  hasActiveFilters(): boolean {
    const charityId = this.filterForm.get('charityId')?.value;
    return !!(this.isHeadOffice && charityId && charityId !== 'all');
  }

  /** Review P27 2026-08-26: teardown for the screen's subscriptions (see destroy$). */
  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  private loadStatistics(): void {
    // Review P31/P32 2026-08-24: token guards the switch race; clearing the previous
    // result means a failed re-run shows the empty state, not the previous charity's
    // numbers silently sitting under the new criteria.
    const token = ++this.loadToken;
    this.statistics = undefined;
    this.statusCounts = undefined;
    this.loading = true;
    this.cdr.markForCheck();

    const charityId = this.filterForm.get('charityId')?.value;
    const filter = {
      charityId: this.isHeadOffice && charityId && charityId !== 'all' ? charityId : undefined
    };

    this.orphanReportService.getOrphanStatistics(filter)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
      next: result => {
        if (token !== this.loadToken) {
          return;
        }
        this.statistics = result;
        this.loading = false;
        this.cdr.markForCheck();
      },
      error: error => {
        if (token !== this.loadToken) {
          return;
        }
        console.error('Error loading statistics:', error);
        this.loading = false;
        this.notification.error(
          error?.message || this.translate.instant('orphanReports.statistics.loadFailed')
        );
        this.cdr.markForCheck();
      }
    });

    // 18-14 — same charity scope, three reads; failed legs already degrade to null
    this.periodicReportService.getStatusCounts({ charityId: filter.charityId })
      .pipe(takeUntil(this.destroy$))
      .subscribe({
      next: counts => {
        if (token !== this.loadToken) {
          return;
        }
        this.statusCounts = counts;
        this.cdr.markForCheck();
      },
      error: () => {
        if (token !== this.loadToken) {
          return;
        }
        this.statusCounts = undefined;
        this.cdr.markForCheck();
      }
    });
  }

  /** Tile value — a null leg renders "—" rather than a misleading zero. */
  displayCount(value: number | null | undefined): string {
    return value === null || value === undefined ? '—' : String(value);
  }

  get groups(): OrphanReportGroupCountRow[] {
    return this.statistics?.groups ?? [];
  }

  /** Trailing totals row — الاناث + الذكور + الاجمالي sums (reconciliation helper). */
  get totals(): { female: number; male: number; total: number } {
    return {
      female: this.groups.reduce((s, g) => s + (g.femaleCount || 0), 0),
      male: this.groups.reduce((s, g) => s + (g.maleCount || 0), 0),
      total: this.groups.reduce((s, g) => s + (g.totalCount || 0), 0)
    };
  }

  /** Stable token → i18n label (closed set per the 9-3 ruling). */
  statusLabel(token: string): string {
    return this.translate.instant('orphanReports.statistics.status.' + token);
  }

  serial(index: number): number {
    return index + 1;
  }

  trackByGroup(_index: number, row: OrphanReportGroupCountRow): string {
    return row.educationalStatus + '|' + (row.educationalLevelName ?? '-');
  }

  // ========== استخراج البيانات — client-side ExcelJS (16-1 precedent) ==========

  exportData(): void {
    if (this.groups.length === 0) {
      this.notification.info(this.translate.instant('orphanReports.statistics.nothingToExport'));
      return;
    }

    // Review P43a 2026-08-24: the button already binds [disabled]="exporting" — the
    // flag was never set, so a double-click ran the workbook build twice.
    this.exporting = true;
    this.cdr.markForCheck();

    import('exceljs').then(({ default: ExcelJS }) => {
      const workbook = new ExcelJS.Workbook();
      const sheet = workbook.addWorksheet(this.translate.instant('orphanReports.statistics.title'));
      sheet.addRow([
        this.translate.instant('orphanReports.statistics.serial'),
        this.translate.instant('orphanReports.statistics.educationalStatus'),
        this.translate.instant('orphanReports.statistics.educationalLevel'),
        this.translate.instant('orphanReports.statistics.female'),
        this.translate.instant('orphanReports.statistics.male'),
        this.translate.instant('orphanReports.statistics.total')
      ]);
      this.groups.forEach((g, i) => sheet.addRow([
        i + 1,
        this.statusLabel(g.educationalStatus),
        g.educationalLevelName || '—',
        g.femaleCount || 0,
        g.maleCount || 0,
        g.totalCount || 0
      ]));
      sheet.addRow([
        '',
        this.translate.instant('orphanReports.statistics.totalRow'),
        '',
        this.totals.female,
        this.totals.male,
        this.totals.total
      ]);

      workbook.xlsx.writeBuffer().then(buffer => {
        const blob = new Blob([buffer], {
          type: 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet'
        });
        const url = window.URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = `orphan_statistics_${new Date().toISOString().slice(0, 10)}.xlsx`;
        document.body.appendChild(a);
        a.click();
        document.body.removeChild(a);
        window.URL.revokeObjectURL(url);
      }).catch(() =>
        this.notification.error(this.translate.instant('orphanReports.statistics.exportFailed'))
      ).finally(() => {
        this.exporting = false;
        this.cdr.markForCheck();
      });
    }).catch(() => {
      this.notification.error(this.translate.instant('orphanReports.statistics.exportFailed'));
      this.exporting = false;
      this.cdr.markForCheck();
    });
  }
}
