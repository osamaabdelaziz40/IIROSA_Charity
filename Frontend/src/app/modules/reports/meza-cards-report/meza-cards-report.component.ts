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
import { OrphanPaymentService } from '../../orphan-payments/services/orphan-payment.service';
import { AuthService } from '../../../core/services/auth.service';
import { NotificationService } from '../../../core/services/notification.service';
import { MezaCardsFilter, MezaCardsRow } from '../models/report.model';

/**
 * UC-RPT-07 (§23.S.8 تقرير الكروت المسجله) + UC-RPT-08 (§23.U.8 استخراج كروت العائل) —
 * families carrying registered guardian Meza cards, HQ-only (Gen. Director). One screen, one
 * endpoint: بحث posts the read variant; استخراج البيانات posts the SAME endpoint with the
 * extract keys (reportNo's presence is the discriminator — buildFilter never sends them).
 *
 * The domain carries no Meza/card column yet (recorded gap): the set is empty until the
 * guardian registration vertical lands the card fields — the endpoint/screen/export contract
 * ships now, and only the server predicate changes when the data exists. Card registration
 * itself belongs to that vertical, never this screen.
 */
@Component({
  selector: 'app-meza-cards-report',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    TranslateModule,
    ReportViewerComponent,
    DropDownComponent
  ],
  templateUrl: './meza-cards-report.component.html',
  styleUrls: ['./meza-cards-report.component.scss']
})
export class MezaCardsReportComponent implements OnInit, OnDestroy {
  rows: MezaCardsRow[] = [];
  loading = false;
  exporting = false;
  hasRun = false;

  currentPage = 1;
  pageSize = 20;
  totalCount = 0;

  /** The extract posts ONE request — the server returns the FULL selection (the bank file is
   * never page 1); the cap only satisfies the shared page-bounds validator. */
  private readonly exportPageSize = 200;

  isHQ = false;
  charityOptions: Array<{ id: string; name: string }> = [];
  batchOptions: Array<{ id: string; name: string }> = [];

  filterForm: FormGroup;

  private readonly destroy$ = new Subject<void>();

  constructor(
    private fb: FormBuilder,
    private reportService: ReportService,
    private reportExportService: ReportExportService,
    private charityService: CharityService,
    private orphanPaymentService: OrphanPaymentService,
    private authService: AuthService,
    private notification: NotificationService,
    private translate: TranslateService
  ) {
    this.filterForm = this.fb.group({
      charityId: [''],
      // §23.U.8 extract keys — read by exportData() only; search() never sends them.
      reportNo: [''],
      isCodes: [false],
      batchId: [''],
      dateFrom: [''],
      dateTo: [''],
      mezaCardExist: [false]
    });
  }

  ngOnInit(): void {
    this.isHQ = this.authService.hasAnyRole(['SuperAdmin', 'Admin']);
    this.loadBatches();
    if (this.isHQ) {
      this.loadCharities();
    }
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  /** الجمعية — HQ narrow; the endpoint roles are HQ-only anyway. */
  private loadCharities(): void {
    this.charityService.getCharities({ pageNumber: 1, pageSize: 500 })
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: response => {
          this.charityOptions = [
            { id: '', name: this.translate.instant('reports.mezaCards.allCharities') },
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

  /** الدفعة — §23.U.8 extract key; fed by the real batch-numbers endpoint, never hardcoded. */
  private loadBatches(): void {
    this.orphanPaymentService.getBatchNumbers()
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: options => {
          this.batchOptions = [
            { id: '', name: this.translate.instant('reports.mezaCards.allBatches') },
            ...(options || []).map(o => ({ id: o.batchNo, name: o.batchNo }))
          ];
        },
        error: () => console.error('Error loading batch numbers')
      });
  }

  // ==================== search / export ====================

  /** بحث — a read; nothing stored changes. Runs from page 1 with the current filter. */
  search(): void {
    if (this.loading || this.exporting) {
      return;
    }
    this.currentPage = 1;
    this.runSearch();
  }

  onPageChange(page: number): void {
    // Review P25 2026-08-26: the loading guard also closes the page race — overlapping page
    // clicks would land out of order and the slower response would overwrite the newer one.
    if (page < 1 || this.loading || this.exporting) {
      return;
    }
    this.currentPage = page;
    this.runSearch();
  }

  private runSearch(): void {
    this.loading = true;
    this.reportService.getMezaCards(this.buildFilter())
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
            httpError?.message || this.translate.instant('reports.mezaCards.searchFailed')
          );
        }
      });
  }

  /**
   * استخراج البيانات (§23.U.8) — the bank-file extract: ONE post to the same endpoint with
   * the extract keys (reportNo's presence flips the server to the extract path, which returns
   * the FULL selection). An empty selection refuses with the nothing-to-produce message — no
   * file is written (the set is empty until the card fields land — recorded gap).
   */
  exportData(): void {
    if (this.loading || this.exporting) {
      return;
    }

    const v = this.filterForm.getRawValue();
    const reportNo = Number(v.reportNo);
    if (!v.reportNo || !Number.isInteger(reportNo) || reportNo <= 0) {
      this.notification.error(this.translate.instant('reports.mezaCards.reportNoRequired'));
      return;
    }

    this.exporting = true;

    const payload: MezaCardsFilter = {
      page: 1,
      pageSize: this.exportPageSize,
      reportNo,
      isCodes: !!v.isCodes,
      mezaCardExist: !!v.mezaCardExist
    };
    if (v.charityId) {
      payload.charityId = String(v.charityId);
    }
    if (v.batchId) {
      payload.batchId = String(v.batchId);
    }
    if (v.dateFrom) {
      payload.dateFrom = v.dateFrom;
    }
    if (v.dateTo) {
      payload.dateTo = v.dateTo;
    }

    this.reportService.getMezaCards(payload)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: result => {
          this.exporting = false;
          const rows = result.items || [];
          if ((result.totalCount || 0) === 0 || rows.length === 0) {
            this.notification.info(this.translate.instant('reports.nothingToExport'));
            return;
          }
          // Review P25 2026-08-26: the extract posts ONE page — when the selection exceeds it,
          // the bank file is partial and says so; never silently short.
          if ((result.totalCount || 0) > rows.length) {
            this.notification.warning(this.translate.instant('reports.export.truncationNotice'));
          }
          this.reportExportService.exportMezaCards(rows, this.extractFileName(v, reportNo))
            .catch(() => this.notification.error(this.translate.instant('reports.mezaCards.exportFailed')));
        },
        error: (httpError: any) => {
          this.exporting = false;
          this.notification.error(
            httpError?.message || this.translate.instant('reports.mezaCards.exportFailed')
          );
        }
      });
  }

  /** meza-cards_<charity|all>_<batch|reportNo>_<yyyy-MM-dd>.xlsx — the §23.U.8 bank-file name. */
  private extractFileName(v: any, reportNo: number): string {
    const scope = v.charityId ? String(v.charityId) : 'all';
    const selector = v.batchId ? String(v.batchId) : String(reportNo);
    return `meza-cards_${scope}_${selector}_${new Date().toISOString().slice(0, 10)}.xlsx`;
  }

  // ==================== helpers ====================

  private buildFilter(): MezaCardsFilter {
    const v = this.filterForm.getRawValue();
    const filter: MezaCardsFilter = {
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

  trackByRow(_index: number, row: MezaCardsRow): string {
    return `${row.charityId ?? ''}-${row.familyCode}`;
  }
}
