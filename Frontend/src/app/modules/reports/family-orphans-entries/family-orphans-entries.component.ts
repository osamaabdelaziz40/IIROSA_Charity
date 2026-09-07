import { Component, OnDestroy, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule } from '@angular/forms';
import { Subject, takeUntil } from 'rxjs';
import { TranslateModule, TranslateService } from '@ngx-translate/core';

import { ReportViewerComponent } from '../report-viewer/report-viewer.component';
import { DropDownComponent } from '../../../shared/components/drop-down/drop-down.component';
import { ReportService } from '../services/report.service';
import { CharityService } from '../../charities/services/charity.service';
import { AuthService } from '../../../core/services/auth.service';
import { NotificationService } from '../../../core/services/notification.service';
import { FamilyEntryDetailRow, FamilyEntryTotals } from '../models/report.model';

/**
 * UC-RPT-13 (§23.S.10 متابعة إدخلات الأسر والأيتام) — entry tracking: families and orphans
 * ENTERED on/after a date. Two data commands share the one GET /api/Families/{id}/follow-up
 * endpoint — إجماليات (totals tiles) and تفاصيل (the grid; also the shell's search button).
 * The print command belongs to 18-37 and is not rendered; §23.S.10 has NO export command.
 *
 * Route reconciliation (recorded): 5-11 (UC-FAM-11) owns the legacy `#/reports/family-orphans`
 * route per its story decision; this screen takes the sibling `family-orphans-entries` path —
 * the same deviation class 18-39 already recorded.
 */
@Component({
  selector: 'app-family-orphans-entries',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    TranslateModule,
    ReportViewerComponent,
    DropDownComponent
  ],
  templateUrl: './family-orphans-entries.component.html',
  styleUrls: ['./family-orphans-entries.component.scss']
})
export class FamilyOrphansEntriesComponent implements OnInit, OnDestroy {
  rows: FamilyEntryDetailRow[] = [];
  totals: FamilyEntryTotals | null = null;
  loading = false;
  loadingTotals = false;
  hasRun = false;

  currentPage = 1;
  pageSize = 20;
  totalCount = 0;

  isHQ = false;
  charityOptions: Array<{ id: string; name: string }> = [];

  filterForm: FormGroup;

  private readonly destroy$ = new Subject<void>();

  constructor(
    private fb: FormBuilder,
    private reportService: ReportService,
    private charityService: CharityService,
    private authService: AuthService,
    private notification: NotificationService,
    private translate: TranslateService
  ) {
    this.filterForm = this.fb.group({
      date: [''],
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
            { id: '', name: this.translate.instant('reports.familyOrphans.allCharities') },
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

  // ==================== the two data commands ====================

  /** تفاصيل إدخالات الأسر الجديدة — also the shell's search command. A read. */
  search(): void {
    if (this.loading || this.loadingTotals) {
      return;
    }
    this.currentPage = 1;
    this.runDetails();
  }

  onPageChange(page: number): void {
    if (page < 1) {
      return;
    }
    this.currentPage = page;
    this.runDetails();
  }

  private runDetails(): void {
    this.loading = true;
    this.reportService.getFamilyEntryTracking({
      ...this.buildParams(),
      mode: 'details',
      // Review P27 2026-08-26: the pager's page was never sent — every page change silently
      // re-fetched page 1, so the grid "refused" to page while the count said otherwise.
      page: this.currentPage,
      pageSize: this.pageSize
    })
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: result => {
          const paged = result as { items?: FamilyEntryDetailRow[]; totalCount?: number };
          this.rows = paged.items || [];
          this.totalCount = paged.totalCount || 0;
          this.loading = false;
          this.hasRun = true;
        },
        error: (httpError: any) => {
          this.loading = false;
          this.notification.error(
            httpError?.message || this.translate.instant('reports.familyOrphans.searchFailed')
          );
        }
      });
  }

  /** إجماليات إدخالات الأسر والأيتام — the totals tiles. A read. */
  loadTotals(): void {
    if (this.loading || this.loadingTotals) {
      return;
    }
    this.loadingTotals = true;
    this.reportService.getFamilyEntryTracking({ ...this.buildParams(), mode: 'totals' })
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: result => {
          this.totals = result as FamilyEntryTotals;
          this.loadingTotals = false;
        },
        error: (httpError: any) => {
          this.loadingTotals = false;
          this.notification.error(
            httpError?.message || this.translate.instant('reports.familyOrphans.searchFailed')
          );
        }
      });
  }

  // ==================== helpers ====================

  private buildParams(): { charityId?: string; date?: string } {
    const v = this.filterForm.getRawValue();
    const params: { charityId?: string; date?: string } = {};
    if (v.charityId) {
      params.charityId = String(v.charityId);
    }
    if (v.date) {
      params.date = String(v.date);
    }
    return params;
  }

  /** 13-1 serial formula — continuous across pages. */
  serial(index: number): number {
    return (this.currentPage - 1) * this.pageSize + index + 1;
  }

  trackByRow(_index: number, row: FamilyEntryDetailRow): string {
    return row.familyId;
  }
}
