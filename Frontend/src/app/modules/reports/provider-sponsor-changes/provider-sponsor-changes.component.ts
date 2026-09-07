import { Component, OnDestroy, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule } from '@angular/forms';
import { Subject, takeUntil } from 'rxjs';
import { TranslateModule, TranslateService } from '@ngx-translate/core';

import { ReportViewerComponent } from '../report-viewer/report-viewer.component';
import { DropDownComponent } from '../../../shared/components/drop-down/drop-down.component';
import { ReportService } from '../services/report.service';
import { ReportExportService } from '../services/report-export.service';
import { CharityService } from '../../charities/services/charity.service';
import { AuthService } from '../../../core/services/auth.service';
import { NotificationService } from '../../../core/services/notification.service';
import { ProviderChangesFilter, ProviderChangeRow } from '../models/report.model';

/**
 * UC-RPT-12 (§23.S.19 تقارير تعديل المعيل) — guardian change history, all roles.
 *
 * The platform keeps no guardian-change audit trail (recorded story finding): Provider holds the
 * current assignment only, so each row projects the CURRENT guardian with تاريخ التعديل from the
 * audit stamps. The three prior-guardian columns render blank — no storage was invented.
 */
@Component({
  selector: 'app-provider-sponsor-changes',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    TranslateModule,
    ReportViewerComponent,
    DropDownComponent
  ],
  templateUrl: './provider-sponsor-changes.component.html',
  styleUrls: ['./provider-sponsor-changes.component.scss']
})
export class ProviderSponsorChangesComponent implements OnInit, OnDestroy {
  rows: ProviderChangeRow[] = [];
  loading = false;
  exporting = false;
  hasRun = false;

  currentPage = 1;
  pageSize = 20;
  totalCount = 0;

  /** The export pages through the whole selection — the validator caps PageSize at 200. */
  private readonly exportPageSize = 200;

  isHQ = false;
  charityOptions: Array<{ id: string; name: string }> = [];

  filterForm: FormGroup;

  private readonly destroy$ = new Subject<void>();

  constructor(
    private fb: FormBuilder,
    private reportService: ReportService,
    private reportExportService: ReportExportService,
    private charityService: CharityService,
    private authService: AuthService,
    private notification: NotificationService,
    private translate: TranslateService
  ) {
    this.filterForm = this.fb.group({
      charityId: ['']
    });
  }

  ngOnInit(): void {
    this.isHQ = this.authService.hasAnyRole(['SuperAdmin', 'Admin']);
    if (this.isHQ) {
      this.loadCharities();
    }
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  /** الجمعية — real charities endpoint only; كل الجهات all-option for HQ. */
  private loadCharities(): void {
    this.charityService.getCharities({ pageNumber: 1, pageSize: 500 })
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: response => {
          this.charityOptions = [
            { id: '', name: this.translate.instant('reports.providerChanges.allCharities') },
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

  // ==================== search / export ====================

  /** بحث/load — a read; nothing stored changes. Runs from page 1 with the current filter. */
  search(): void {
    if (this.loading || this.exporting) {
      return;
    }
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

  private runSearch(): void {
    this.loading = true;
    this.reportService.getProviderSponsorChanges(this.buildFilter())
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: result => {
          this.rows = result.items || [];
          this.totalCount = result.totalCount || 0;
          this.loading = false;
          this.hasRun = true;
        },
        error: (httpError: any) => {
          this.loading = false;
          this.notification.error(
            httpError?.message || this.translate.instant('reports.providerChanges.searchFailed')
          );
        }
      });
  }

  /**
   * استخراج البيانات — the §23.S.19 workbook over the WHOLE selection, paging server-side
   * under the 200 cap. An empty selection refuses with the nothing-to-produce message.
   */
  exportData(): void {
    if (this.loading || this.exporting) {
      return;
    }
    if (this.totalCount === 0) {
      this.notification.info(this.translate.instant('reports.nothingToExport'));
      return;
    }

    this.exporting = true;
    const collected: ProviderChangeRow[] = [];
    const totalPages = Math.ceil(this.totalCount / this.exportPageSize);

    const fetchNext = (page: number): void => {
      if (page > totalPages) {
        this.exporting = false;
        if (collected.length === 0) {
          this.notification.info(this.translate.instant('reports.nothingToExport'));
          return;
        }
        const v = this.filterForm.getRawValue();
        const scope = v.charityId ? String(v.charityId) : 'all';
        this.reportExportService.exportProviderChanges(collected, `provider-changes_${scope}_${new Date().toISOString().slice(0, 10)}.xlsx`)
          .catch(() => this.notification.error(this.translate.instant('reports.providerChanges.exportFailed')));
        return;
      }

      const filter = { ...this.buildFilter(), page, pageSize: this.exportPageSize };
      this.reportService.getProviderSponsorChanges(filter)
        .pipe(takeUntil(this.destroy$))
        .subscribe({
          next: result => {
            collected.push(...(result.items || []));
            fetchNext(page + 1);
          },
          error: (httpError: any) => {
            this.exporting = false;
            this.notification.error(httpError?.message || this.translate.instant('reports.providerChanges.exportFailed'));
          }
        });
    };

    fetchNext(1);
  }

  // ==================== helpers ====================

  private buildFilter(): ProviderChangesFilter {
    const v = this.filterForm.getRawValue();
    const filter: ProviderChangesFilter = {
      page: this.currentPage,
      pageSize: this.pageSize
    };
    if (v.charityId) {
      filter.charityId = String(v.charityId);
    }
    return filter;
  }

  /** 13-1 serial formula — continuous across pages. */
  serial(index: number): number {
    return (this.currentPage - 1) * this.pageSize + index + 1;
  }

  trackByRow(_index: number, row: ProviderChangeRow): string {
    return row.providerId;
  }
}
