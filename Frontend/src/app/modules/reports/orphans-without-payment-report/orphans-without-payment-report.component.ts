import { Component, OnDestroy, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Subject, take, takeUntil } from 'rxjs';
import { TranslateModule, TranslateService } from '@ngx-translate/core';

import { ReportViewerComponent } from '../report-viewer/report-viewer.component';
import { DropDownComponent } from '../../../shared/components/drop-down/drop-down.component';
import { ReportService } from '../services/report.service';
import { OrphanPaymentService } from '../../orphan-payments/services/orphan-payment.service';
import { CharityService } from '../../charities/services/charity.service';
import { AuthService } from '../../../core/services/auth.service';
import { NotificationService } from '../../../core/services/notification.service';
import { OrphansWithoutPaymentRow } from '../models/report.model';

/**
 * UC-RPT-28 (§23.U.28 أيتام لم يصرف لهم) — the zero-disbursement gaps of one payment
 * batch: item rows with no cheque, no transfer and nothing received. Launched from the
 * §23.S.3 orphans screen's command (carrying its charity selection) or opened directly.
 *
 * HQ report: the endpoint gates SuperAdmin/Admin; the charity filter is an HQ narrow only.
 * No export command — §23.U.28's contract is the on-screen gap list.
 */
@Component({
  selector: 'app-orphans-without-payment-report',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    TranslateModule,
    ReportViewerComponent,
    DropDownComponent
  ],
  templateUrl: './orphans-without-payment-report.component.html',
  styleUrls: ['./orphans-without-payment-report.component.scss']
})
export class OrphansWithoutPaymentReportComponent implements OnInit, OnDestroy {
  rows: OrphansWithoutPaymentRow[] = [];
  loading = false;
  hasRun = false;

  currentPage = 1;
  pageSize = 20;
  totalCount = 0;

  isHQ = false;

  /** الدفعة — payment groups (the Guid the endpoint needs), labelled by batch no. */
  batchOptions: Array<{ id: string; name: string }> = [];
  charityOptions: Array<{ id: string; name: string }> = [];

  filterForm: FormGroup;

  /**
   * Review P27 2026-08-26: the §23.S.3 jump's ?charityId= parks here until the charity
   * options land — a setValue racing the async dropdown fill left the control holding an
   * id the list could not show (and any later param emission re-applied it over the
   * user's own choice).
   */
  private pendingCharityId: string | null = null;

  private readonly destroy$ = new Subject<void>();

  constructor(
    private fb: FormBuilder,
    private route: ActivatedRoute,
    private reportService: ReportService,
    private orphanPaymentService: OrphanPaymentService,
    private charityService: CharityService,
    private authService: AuthService,
    private notification: NotificationService,
    private translate: TranslateService
  ) {
    this.filterForm = this.fb.group({
      paymentId: ['', Validators.required],
      charityId: ['']
    });
  }

  ngOnInit(): void {
    this.isHQ = this.authService.hasAnyRole(['SuperAdmin', 'Admin']);
    if (this.isHQ) {
      this.loadCharities();
    }
    this.loadBatches();

    // The §23.S.3 command lands here pre-filtered to the orphans screen's charity
    // selection (?charityId=…) — read ONCE (Review P27 2026-08-26: take(1)); it is applied
    // when the charity options are ready to display it.
    this.route.queryParamMap
      .pipe(take(1), takeUntil(this.destroy$))
      .subscribe(params => {
        const charityId = params.get('charityId');
        if (charityId && this.isHQ) {
          this.pendingCharityId = charityId;
          this.applyPendingCharity();
        }
      });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  /** الدفعة — payment groups paged read; label = batch no, falling back to group name. */
  private loadBatches(): void {
    this.orphanPaymentService.getOrphanPayments({ pageNumber: 1, pageSize: 100 })
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: result => {
          this.batchOptions = (result.items || []).map(p => ({
            id: p.id,
            name: p.batchNo || p.groupName
          }));
          // Review P26 2026-08-26: a capped lookup says so — a silently truncated dropdown hides choices.
          if ((result.totalCount || 0) > (result.items || []).length) {
            this.notification.warning(this.translate.instant('reports.lookup.truncated'));
          }
        },
        error: () => console.error('Error loading payment batches')
      });
  }

  /** الجمعية — real charities endpoint only; كل الجهات all-option for HQ. */
  private loadCharities(): void {
    this.charityService.getCharities({ pageNumber: 1, pageSize: 500 })
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: response => {
          this.charityOptions = [
            { id: '', name: this.translate.instant('reports.orphansWithoutPayment.allCharities') },
            ...(response.items || []).map(c => ({ id: c.id, name: c.name }))
          ];
          // Review P26 2026-08-26: a capped lookup says so — a silently truncated dropdown hides choices.
          if ((response.totalCount || 0) > (response.items || []).length) {
            this.notification.warning(this.translate.instant('reports.lookup.truncated'));
          }
          // Review P27 2026-08-26: the parked pre-selection can bind now that options exist.
          this.applyPendingCharity();
        },
        error: () => console.error('Error loading charities')
      });
  }

  /** Review P27 2026-08-26: apply the parked ?charityId= once the dropdown can show it. */
  private applyPendingCharity(): void {
    if (this.pendingCharityId && this.charityOptions.length > 1) {
      this.filterForm.get('charityId')!.setValue(this.pendingCharityId);
      this.pendingCharityId = null;
    }
  }

  // ==================== search ====================

  /** بحث/load — a read; nothing stored changes. Runs from page 1 with the chosen batch. */
  search(): void {
    if (this.loading || this.filterForm.invalid) {
      this.filterForm.markAllAsTouched();
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
    this.reportService.getOrphansWithoutPayment(this.buildFilter())
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: result => {
          this.rows = result.items || [];
          this.totalCount = result.totalCount ?? 0;
          this.loading = false;
          this.hasRun = true;
        },
        error: (httpError: any) => {
          this.loading = false;
          this.notification.error(
            httpError?.message || this.translate.instant('reports.orphansWithoutPayment.searchFailed')
          );
        }
      });
  }

  // ==================== helpers ====================

  private buildFilter(): { paymentId: string; charityId?: string; page: number; pageSize: number } {
    const v = this.filterForm.getRawValue();
    const filter: { paymentId: string; charityId?: string; page: number; pageSize: number } = {
      paymentId: String(v.paymentId),
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

  trackByRow(_index: number, row: OrphansWithoutPaymentRow): string {
    return row.orphanId;
  }
}
