import { Component, OnInit, OnDestroy, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, ActivatedRoute, RouterModule } from '@angular/router';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import {
  OrphanPaymentDto,
  OrphanPaymentItemDto,
  ExportPaymentGroupOptions,
  PaymentSummary
} from '../models/orphan-payment.model';
import { OrphanPaymentService } from '../services/orphan-payment.service';
import { NotificationService } from '../../../core/services/notification.service';
import { PaginationComponent } from '../../../shared/components';
import { ReportService } from '../../reports/services/report.service';
import { ReportPdfService } from '../../reports/services/report-pdf.service';
import { PaymentsOutcomeReport, PaymentsOutcomeRow, PaymentsOutcomeVariant } from '../../reports/models/report.model';
import { ChequeNumbersReport, ChequeNumbersRow } from '../../reports/models/report.model';
import { ReportSheetConfig } from '../../reports/services/report-pdf.service';

@Component({
  selector: 'app-orphan-payment-detail',
  standalone: true,
  imports: [CommonModule, FormsModule, TranslateModule, RouterModule, PaginationComponent],
  templateUrl: './orphan-payment-detail.component.html',
  styleUrls: ['./orphan-payment-detail.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class OrphanPaymentDetailComponent implements OnInit, OnDestroy {
  private destroy$ = new Subject<void>();

  // Data — 10-3: header AND rows come from one GET {id}/details call (the old second
  // call, GET {id}/orphans, never existed server-side).
  paymentGroup: OrphanPaymentDto | null = null;
  orphans: OrphanPaymentItemDto[] = [];
  totalOrphans = 0;
  loading = false;
  notFound = false;

  // 10-8 (UC-PAY-08): orphan-level grid state — client search/sort/paging over the
  // loaded item set; no stored data changes, no second endpoint.
  searchTerm = '';
  sortColumn: 'code' | 'name' | 'amount' = 'code';
  sortAscending = true;
  pageNumber = 1;
  pageSize = 10;
  filteredOrphans: OrphanPaymentItemDto[] = [];
  pagedOrphans: OrphanPaymentItemDto[] = [];

  // Export Options
  showExportModal = false;
  exportOptions: ExportPaymentGroupOptions = {
    format: 'Excel',
    includePhotos: false,
    groupBy: 'None'
  };

  // 18-29 (UC-RPT-29): the hosted outcome-list commands — which variant's request is
  // in flight (null = idle). Guards double-clicks and drives the button spinners.
  outcomePrinting: PaymentsOutcomeVariant | null = null;

  // 18-30 (UC-RPT-30): the cheque-numbers sheet command — true while its request is in flight.
  chequePrinting = false;

  // 18-32 (UC-RPT-32): the batch's cover figures — loaded on batch open, rendered as the
  // totals band above the grid. Zeros are a valid result (an empty batch, AC 4);
  // null = still loading or the read failed (the band is supplementary — the batch view
  // stays fully usable without it).
  summary: PaymentSummary | null = null;
  summaryLoading = false;

  constructor(
    private router: Router,
    private route: ActivatedRoute,
    private orphanPaymentService: OrphanPaymentService,
    private translate: TranslateService,
    private notificationService: NotificationService,
    private reportService: ReportService,
    private reportPdfService: ReportPdfService,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.loadDetails(id);
    }
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  // ==================== LOADING DATA ====================

  loadDetails(id: string): void {
    this.loading = true;
    this.notFound = false;
    this.orphanPaymentService.getOrphanPaymentDetails(id).pipe(takeUntil(this.destroy$)).subscribe({
      next: (data) => {
        this.paymentGroup = data;
        this.orphans = data.orphans ?? [];
        this.totalOrphans = data.orphanCount ?? this.orphans.length;
        this.pageNumber = 1;
        this.applyGridState();
        this.loading = false;
        this.cdr.markForCheck();
        // 18-32 — the totals band loads with the batch (AC 1); its failure is non-fatal.
        this.loadSummary(data.id);
      },
      error: (err) => {
        this.loading = false;
        // AC 3 — unknown or soft-deleted id: 404 shows a not-found state, not a blank screen
        this.notFound = err?.status === 404;
        // Review P27 2026-08-26: every OTHER failure (500/403/timeout) now says so — it
        // previously left notFound=false and rendered a silent blank screen.
        if (!this.notFound) {
          this.notificationService.error(
            err?.error?.message || err?.message
            || this.translate.instant('orphanPayments.loadFailed'));
        }
        this.cdr.markForCheck();
      }
    });
  }

  // ==================== 18-32 PAYMENT SUMMARY (UC-RPT-32) ====================

  /**
   * 18-32 صفحة ملخص الدفعة — the batch's cover figures for the totals band. Read-only;
   * the group itself is the frame (no charityId is sent — the server scopes from the
   * token). An empty batch legitimately renders zeros (AC 4); only a transport failure
   * hides the band, and that failure never blocks the batch view.
   */
  private loadSummary(paymentId: string): void {
    this.summaryLoading = true;
    this.cdr.markForCheck();
    this.orphanPaymentService.getPaymentSummary(paymentId)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (data) => {
          this.summary = data;
          this.summaryLoading = false;
          this.cdr.markForCheck();
        },
        error: () => {
          this.summary = null;
          this.summaryLoading = false;
          this.cdr.markForCheck();
        }
      });
  }

  /** The band's currency suffix — " EGP" when the batch names one, empty otherwise. */
  get currencySuffix(): string {
    return this.summary?.currency ? ` ${this.summary.currency}` : '';
  }

  // ==================== ACTIONS ====================

  onEdit(): void {
    if (this.paymentGroup) {
      this.router.navigate(['/orphan-payments', this.paymentGroup.id, 'edit']);
    }
  }

  onAddOrphans(): void {
    if (this.paymentGroup) {
      this.router.navigate(['/orphan-payments', this.paymentGroup.id, 'add-orphans']);
    }
  }

  /** 10-2: live route is DELETE orphan-items/{itemId} — was a body-carrying group DELETE that 404'd. */
  onRemoveOrphan(orphanPaymentItemId: string): void {
    if (!this.paymentGroup) return;

    if (confirm(this.translate.instant('orphanPayments.removeOrphanConfirm'))) {
      // Review P27 2026-08-26: write actions ride destroy$ too — a response landing on a
      // destroyed component mutated dead state.
      this.orphanPaymentService.removeOrphanItem(orphanPaymentItemId)
        .pipe(takeUntil(this.destroy$))
        .subscribe({
        next: () => {
          this.loadDetails(this.paymentGroup!.id);
        }
      });
    }
  }

  onMarkAsUploaded(): void {
    if (!this.paymentGroup) return;

    if (confirm(this.translate.instant('orphanPayments.markAsUploadedConfirm'))) {
      this.orphanPaymentService.markAsUploaded(this.paymentGroup.id)
        .pipe(takeUntil(this.destroy$))
        .subscribe({
        next: () => {
          this.loadDetails(this.paymentGroup!.id);
        }
      });
    }
  }

  onUnmarkAsUploaded(): void {
    if (!this.paymentGroup) return;

    if (confirm(this.translate.instant('orphanPayments.unmarkAsUploadedConfirm'))) {
      this.orphanPaymentService.unmarkAsUploaded(this.paymentGroup.id)
        .pipe(takeUntil(this.destroy$))
        .subscribe({
        next: () => {
          this.loadDetails(this.paymentGroup!.id);
        }
      });
    }
  }

  onDelete(): void {
    if (!this.paymentGroup) return;

    if (confirm(this.translate.instant('orphanPayments.deleteGroupConfirm'))) {
      this.orphanPaymentService.deleteOrphanPayment(this.paymentGroup.id)
        .pipe(takeUntil(this.destroy$))
        .subscribe({
        next: () => {
          this.router.navigate(['/orphan-payments']);
        }
      });
    }
  }

  // ==================== EXPORT ====================

  onExport(): void {
    this.showExportModal = true;
  }

  closeExportModal(): void {
    this.showExportModal = false;
  }

  doExport(): void {
    if (!this.paymentGroup) return;

    this.orphanPaymentService.exportGroup(this.paymentGroup.id, this.exportOptions)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
      next: (blob) => {
        const extension = this.exportOptions.format === 'Excel' ? 'xlsx' : 'pdf';
        const filename = `PaymentGroup_${this.paymentGroup?.batchNo || this.paymentGroup?.id}.${extension}`;
        this.orphanPaymentService.downloadFile(blob, filename);
        this.closeExportModal();
      }
    });
  }

  // 10-3: onPrint removed — {id}/print has no server endpoint; the print flow returns with 10-24.

  // ==================== 18-29 OUTCOME LISTS (UC-RPT-29) ====================

  /**
   * 18-29 المستلمون / غير المستلمين / الموقوفون — the three printed outcome lists of
   * THIS batch (§23.U.29's hosted commands). One endpoint; the variant picks the list.
   * The server scopes charity callers to their own charity; no charityId is sent — the
   * group itself is already the frame. Empty variant ⇒ info toast, never an empty
   * document (AC 4); the sheet goes through the 18-21 browser-print pipeline.
   */
  printOutcomeList(variant: PaymentsOutcomeVariant): void {
    const batchNo = this.paymentGroup?.batchNo?.trim();
    if (!batchNo || this.outcomePrinting) return;

    this.outcomePrinting = variant;
    this.cdr.markForCheck();
    this.reportService.getPaymentsOutcome({ variant, orpCheckBatchNo: batchNo })
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (report) => {
          this.outcomePrinting = null;
          this.cdr.markForCheck();
          if (!report.rows?.length) {
            this.notificationService.info(this.translate.instant('reports.paymentsOutcome.nothingToPrint'));
            return;
          }
          this.reportPdfService.printSheet(this.buildOutcomeSheet(report));
        },
        error: (err) => {
          this.outcomePrinting = null;
          this.cdr.markForCheck();
          this.notificationService.error(
            err?.message || this.translate.instant('reports.paymentsOutcome.printFailed'));
        }
      });
  }

  /** The printed sheet — meta carries the header (batch, charity, period) and the totals
   *  (printSheet renders no totals row; meta is the contract). */
  private buildOutcomeSheet(report: PaymentsOutcomeReport): ReportSheetConfig<PaymentsOutcomeRow> {
    const t = (key: string) => this.translate.instant(key);
    // Explicit switch — the server canonicalises the variant to lowercase, and an
    // interpolated key (`title_${variant}`) would miss title_notReceived's camelCase.
    const variantTitle = report.variant === 'received'
      ? t('reports.paymentsOutcome.title_received')
      : report.variant === 'stopped'
        ? t('reports.paymentsOutcome.title_stopped')
        : t('reports.paymentsOutcome.title_notReceived');
    const currency = report.currency ? ` ${report.currency}` : '';

    return {
      documentTitle: `${variantTitle} - ${report.batchNo}`,
      title: variantTitle,
      subtitle: `${t('reports.paymentsOutcome.metaBatch')}: ${report.batchNo}`,
      meta: [
        { label: t('reports.paymentsOutcome.metaCharity'), value: report.charityName || '-' },
        {
          label: t('reports.paymentsOutcome.metaPeriod'),
          value: `${this.formatDate(report.periodFrom)} — ${this.formatDate(report.periodTo)}`
        },
        {
          label: t('reports.paymentsOutcome.metaTotal'),
          value: `${t('reports.paymentsOutcome.metaCount')}: ${report.totalCount} — `
            + `${t('reports.paymentsOutcome.metaAmount')}: ${this.formatAmount(report.totalAmount)}${currency}`
        }
      ],
      columns: [
        { header: t('reports.paymentsOutcome.colSerial'), render: (_row, index) => String(index + 1) },
        { header: t('reports.paymentsOutcome.colOrphanCode'), render: row => row.orphanCode ?? '' },
        { header: t('reports.paymentsOutcome.colOrphanName'), render: row => row.orphanName ?? '' },
        { header: t('reports.paymentsOutcome.colGuardian'), render: row => row.guardianName ?? '' },
        { header: t('reports.paymentsOutcome.colAmount'), render: row => this.formatAmount(row.amount) },
        { header: t('reports.paymentsOutcome.colChequeNo'), render: row => row.chiqueNo ?? '' },
        { header: t('reports.paymentsOutcome.colPrintDate'), render: row => this.formatDate(row.printDate) },
        { header: t('reports.paymentsOutcome.colCollector'), render: row => row.collectorName ?? '' }
      ],
      rows: report.rows
    };
  }

  // ==================== 18-30 CHEQUE NUMBERS (UC-RPT-30) ====================

  /**
   * 18-30 أرقام الشيكات — the batch's recorded cheque numbers for handover and
   * reconciliation (§23.U.30's hosted command). Only rows with a cheque number are on
   * the sheet; nothing recorded ⇒ info toast, never an empty document (AC 4).
   */
  printChequeNumbers(): void {
    const batchNo = this.paymentGroup?.batchNo?.trim();
    if (!batchNo || this.chequePrinting) return;

    this.chequePrinting = true;
    this.cdr.markForCheck();
    this.reportService.getChequeNumbers({ orpCheckBatchNo: batchNo })
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (report) => {
          this.chequePrinting = false;
          this.cdr.markForCheck();
          if (!report.rows?.length) {
            this.notificationService.info(this.translate.instant('reports.chequeNumbers.nothingToPrint'));
            return;
          }
          this.reportPdfService.printSheet(this.buildChequeSheet(report));
        },
        error: (err) => {
          this.chequePrinting = false;
          this.cdr.markForCheck();
          this.notificationService.error(
            err?.message || this.translate.instant('reports.chequeNumbers.printFailed'));
        }
      });
  }

  /** The printed sheet — same meta-band contract as the outcome lists (totals ride meta). */
  private buildChequeSheet(report: ChequeNumbersReport): ReportSheetConfig<ChequeNumbersRow> {
    const t = (key: string) => this.translate.instant(key);
    const currency = report.currency ? ` ${report.currency}` : '';

    return {
      documentTitle: `${t('reports.chequeNumbers.title')} - ${report.batchNo}`,
      title: t('reports.chequeNumbers.title'),
      subtitle: `${t('reports.chequeNumbers.metaBatch')}: ${report.batchNo}`,
      meta: [
        { label: t('reports.chequeNumbers.metaCharity'), value: report.charityName || '-' },
        {
          label: t('reports.chequeNumbers.metaPeriod'),
          value: `${this.formatDate(report.periodFrom)} — ${this.formatDate(report.periodTo)}`
        },
        {
          label: t('reports.chequeNumbers.metaTotal'),
          value: `${t('reports.chequeNumbers.metaCount')}: ${report.totalCount} — `
            + `${t('reports.chequeNumbers.metaAmount')}: ${this.formatAmount(report.totalAmount)}${currency}`
        }
      ],
      columns: [
        { header: t('reports.chequeNumbers.colSerial'), render: (_row, index) => String(index + 1) },
        { header: t('reports.chequeNumbers.colOrphanCode'), render: row => row.orphanCode ?? '' },
        { header: t('reports.chequeNumbers.colOrphanName'), render: row => row.orphanName ?? '' },
        { header: t('reports.chequeNumbers.colGuardian'), render: row => row.guardianName ?? '' },
        { header: t('reports.chequeNumbers.colAmount'), render: row => this.formatAmount(row.amount) },
        { header: t('reports.chequeNumbers.colChequeNo'), render: row => row.chiqueNo ?? '' },
        { header: t('reports.chequeNumbers.colPrintDate'), render: row => this.formatDate(row.printDate) },
        { header: t('reports.chequeNumbers.colCollector'), render: row => row.collectorName ?? '' }
      ],
      rows: report.rows
    };
  }

  // ==================== HELPERS ====================

  /** 10-9 (UC-PAY-09): stop/resume toggle — optimistic flip; the server stays the authority
   *  (out-of-scope rows 403, HQ-stop lock on resume refuses with a localised message).
   *  Review P21: in-flight guard — a double-click must not race two toggles whose
   *  out-of-order 200s leave the UI flag disagreeing with the DB. */
  private togglingRowIds = new Set<string>();

  onToggleStop(orphan: OrphanPaymentItemDto): void {
    if (this.togglingRowIds.has(orphan.id)) return;
    this.togglingRowIds.add(orphan.id);

    const target = !orphan.isStopped;
    orphan.isStopped = target;
    this.applyGridState();
    this.cdr.markForCheck();

    this.orphanPaymentService.updateOrphanItem({
      orphanPaymentItemId: orphan.id,
      action: 0,
      flag: target
    })
      .pipe(takeUntil(this.destroy$))
      .subscribe({
      next: (row) => {
        this.togglingRowIds.delete(orphan.id);
        const idx = this.orphans.findIndex(o => o.id === row.id);
        if (idx >= 0) {
          this.orphans[idx] = row;
        }
        this.applyGridState();
        this.cdr.markForCheck();
        this.notificationService.success(
          this.translate.instant(target ? 'orphanPayments.paymentStopped' : 'orphanPayments.paymentResumed'));
      },
      error: (err) => {
        this.togglingRowIds.delete(orphan.id);
        orphan.isStopped = !target; // rollback the optimistic flip
        this.applyGridState();
        this.cdr.markForCheck();
        this.notificationService.error(this.resolveRowActionError(err));
      }
    });
  }

  /** Review P24: known refusal shapes render localised — the BR-16/21 HQ-stop lock, the
   *  scope 403 (a Forbid carries no body to match on), and the validator's 400 field map —
   *  instead of raw (or misleading generic) English. */
  private resolveRowActionError(err: any): string {
    const msg: string = err?.message || '';
    if (/stopped by head office/i.test(msg)) {
      return this.translate.instant('orphanPayments.hqStopResumeRefused');
    }
    if (err?.status === 403) {
      return this.translate.instant('orphanPayments.rowActionForbidden');
    }
    if (err?.status === 400 && err?.details && Object.keys(err.details).length > 0) {
      return this.translate.instant('orphanPayments.rowActionInvalid');
    }
    return msg || this.translate.instant('common.operationFailed');
  }

  /** 10-10 (UC-PAY-10): mark a row printed (action 1) — idempotent server-side; the button
   *  hides once printed. Out-of-scope 403 / session loss handled by the shared error path. */
  onMarkPrinted(orphan: OrphanPaymentItemDto): void {
    this.orphanPaymentService.updateOrphanItem({
      orphanPaymentItemId: orphan.id,
      action: 1
    })
      .pipe(takeUntil(this.destroy$))
      .subscribe({
      next: (row) => {
        const idx = this.orphans.findIndex(o => o.id === row.id);
        if (idx >= 0) {
          this.orphans[idx] = row;
        }
        this.applyGridState();
        this.cdr.markForCheck();
        this.notificationService.success(this.translate.instant('orphanPayments.rowMarkedPrinted'));
      },
      error: (err) => {
        this.notificationService.error(this.resolveRowActionError(err));
      }
    });
  }

  // 10-8: recompute filtered → sorted → paged rows. Arabic names sort under localeCompare
  // with 'ar'; null amounts sort last regardless of direction.
  applyGridState(): void {
    const term = this.searchTerm.trim().toLowerCase();
    let rows = this.orphans;
    if (term) {
      rows = rows.filter(o =>
        (o.orphanFullName ?? '').toLowerCase().includes(term) ||
        (o.orphanCode ?? '').toLowerCase().includes(term));
    }

    const dir = this.sortAscending ? 1 : -1;
    rows = [...rows].sort((a, b) => {
      switch (this.sortColumn) {
        case 'name':
          return dir * (a.orphanFullName ?? '').localeCompare(b.orphanFullName ?? '', 'ar');
        case 'amount': {
          // Review P27 2026-08-26: nulls sort LAST regardless of direction — the previous
          // Infinity trick ordered them FIRST under a descending sort.
          const n = (a.amount == null ? 1 : 0) - (b.amount == null ? 1 : 0);
          if (n) {
            return n;
          }
          const av = a.amount!;
          const bv = b.amount!;
          return dir * (av === bv ? 0 : av < bv ? -1 : 1);
        }
        default:
          return dir * (a.orphanCode ?? '').localeCompare(b.orphanCode ?? '', 'ar');
      }
    });

    this.filteredOrphans = rows;
    const totalPages = Math.max(1, Math.ceil(rows.length / this.pageSize));
    if (this.pageNumber > totalPages) {
      this.pageNumber = totalPages;
    }
    const start = (this.pageNumber - 1) * this.pageSize;
    this.pagedOrphans = rows.slice(start, start + this.pageSize);
  }

  onSearchTermChange(): void {
    this.pageNumber = 1;
    this.applyGridState();
    this.cdr.markForCheck();
  }

  sortBy(column: 'code' | 'name' | 'amount'): void {
    if (this.sortColumn === column) {
      this.sortAscending = !this.sortAscending;
    } else {
      this.sortColumn = column;
      this.sortAscending = true;
    }
    this.applyGridState();
    this.cdr.markForCheck();
  }

  onPageChange(page: number): void {
    this.pageNumber = page;
    this.applyGridState();
    this.cdr.markForCheck();
  }

  get filteredCount(): number {
    return this.filteredOrphans.length;
  }

  canModify(): boolean {
    return this.paymentGroup ? !this.paymentGroup.isBatchUploaded : true;
  }

  getStatusBadgeClass(): string {
    return this.paymentGroup ? this.orphanPaymentService.getStatusBadgeClass(this.paymentGroup.isBatchUploaded) : '';
  }

  getStatusText(): string {
    return this.paymentGroup ? this.orphanPaymentService.getStatusText(this.paymentGroup.isBatchUploaded) : '-';
  }

  formatDate(date: string | null | undefined): string {
    return this.orphanPaymentService.formatDate(date || '');
  }

  /** Review P27 2026-08-26: money renders grouped with two decimals — raw machine numbers
   *  ("1234.5") read as data, not money (sheets and summary band alike). */
  formatAmount(value: number | null | undefined): string {
    return (value ?? 0).toLocaleString(undefined, {
      minimumFractionDigits: 2,
      maximumFractionDigits: 2
    });
  }

  getExportFormatLabel(format: string): string {
    return format === 'Excel' ? 'Excel' : 'PDF';
  }

  getGroupByLabel(groupBy: string): string {
    switch (groupBy) {
      case 'Charity': return this.translate.instant('orphanPayments.groupByCharity');
      case 'Region': return this.translate.instant('orphanPayments.groupByRegion');
      default: return this.translate.instant('orphanPayments.noGrouping');
    }
  }

  trackOrphan(index: number, orphan: OrphanPaymentItemDto): string {
    return orphan.id;
  }

  // 10-7 §15.S.3 حالة الصرف — the 10-2 column contract (per 10-17): 0=Pending, 1=Executed,
  // 2=Failed; null means the row has no imported/recorded status → render blank, never fake.
  exchangeStatusKey(status: number | null | undefined): string | null {
    switch (status) {
      case 0: return 'orphanPayments.exchangePending';
      case 1: return 'orphanPayments.exchangeExecuted';
      case 2: return 'orphanPayments.exchangeFailed';
      default: return null;
    }
  }

  exchangeStatusClass(status: number | null | undefined): string {
    switch (status) {
      case 0: return 'badge-warning';
      case 1: return 'badge-success';
      case 2: return 'badge-danger';
      default: return '';
    }
  }
}
