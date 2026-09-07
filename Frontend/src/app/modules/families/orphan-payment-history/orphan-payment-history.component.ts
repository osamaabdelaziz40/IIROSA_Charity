import { ChangeDetectionStrategy, ChangeDetectorRef, Component, EventEmitter, Input, OnDestroy, OnInit, Output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Subject, Subscription } from 'rxjs';
import { TranslateModule, TranslateService, LangChangeEvent } from '@ngx-translate/core';
import { OrphanLookupDto, BatchNumberDto } from '../models/family.model';
import { OrphanPaymentService } from '../../orphan-payments/services/orphan-payment.service';
import { OrphanPaymentDto } from '../../orphan-payments/models/orphan-payment.model';
import { NotificationService } from '../../../core/services/notification.service';
import { SharedModule } from '../../../shared/shared.module';

/**
 * UC-ORP-08 / UC-ORP-09 — سجل دفعات اليتيم وتفاصيل الدفعة.
 * An inline panel opened from a coding-screen row: the orphan's payment history
 * (the batches containing them), and — on row selection — that orphan's rows
 * within the chosen batch.
 */
@Component({
  selector: 'app-orphan-payment-history',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [
    CommonModule,
    FormsModule,
    TranslateModule,
    SharedModule
  ],
  templateUrl: './orphan-payment-history.component.html',
  styleUrls: ['./orphan-payment-history.component.scss']
})
export class OrphanPaymentHistoryComponent implements OnInit, OnDestroy {
  private destroy$ = new Subject<void>();
  private langChangeSubscription?: Subscription;
  private batchNumbers: BatchNumberDto[] = [];

  @Input({ required: true }) orphan!: OrphanLookupDto;
  @Output() closed = new EventEmitter<void>();

  /** The orphan's batches (UC-ORP-08) */
  history: OrphanPaymentDto[] = [];
  loading = false;

  /** رقم الحصة optional filter options (UC-ORP-11) */
  selectedBatchNo = 'all';
  batchNumberOptions: Array<{ id: string; name: string }> = [];

  /** The selected batch's detail — this orphan's rows (UC-ORP-09) */
  detail: OrphanPaymentDto | null = null;
  detailLoading = false;

  constructor(
    private orphanPaymentService: OrphanPaymentService,
    private notification: NotificationService,
    private translate: TranslateService,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    this.langChangeSubscription = this.translate.onLangChange.subscribe((event: LangChangeEvent) => {
      this.buildBatchNumberOptions();
      this.cdr.markForCheck(); // P2 — OnPush: translated options must re-render
    });

    this.loadBatchNumbers();
    this.loadHistory();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
    if (this.langChangeSubscription) {
      this.langChangeSubscription.unsubscribe();
    }
  }

  /** UC-ORP-08 — the batches containing this orphan, newest first */
  loadHistory(): void {
    this.loading = true;
    this.detail = null;

    // P14 — the batch filter is an exact match on رقم الدفعة, not a free-text searchTerm.
    this.orphanPaymentService.getOrphanPayments({
      orphanId: this.orphan.orphanId,
      batchNo: this.selectedBatchNo !== 'all' ? this.selectedBatchNo : undefined,
      sortBy: 'GroupDate',
      sortDescending: true,
      pageNumber: 1,
      pageSize: 50
    }).subscribe({
      next: (response) => {
        this.history = (response.items as unknown as OrphanPaymentDto[]) || [];
        this.loading = false;
        this.cdr.markForCheck(); // P2 — OnPush: HTTP callbacks must mark
      },
      error: (error: any) => {
        console.error('Error loading orphan payment history:', error);
        this.notification.error(error.message || this.translate.instant('orphanCoding.historyLoadFailed'));
        this.loading = false;
        this.cdr.markForCheck();
      }
    });
  }

  /** UC-ORP-11 — the رقم الحصة picker's options */
  private loadBatchNumbers(): void {
    this.orphanPaymentService.getBatchNumbers().subscribe({
      next: (batchNumbers: BatchNumberDto[]) => {
        this.batchNumbers = batchNumbers || [];
        this.buildBatchNumberOptions();
        this.cdr.markForCheck(); // P2 — OnPush: HTTP callbacks must mark
      },
      error: (error: any) => {
        console.error('Error loading batch numbers:', error);
        this.batchNumbers = [];
        this.cdr.markForCheck();
      }
    });
  }

  private buildBatchNumberOptions(): void {
    this.batchNumberOptions = [
      { id: 'all', name: this.translate.instant('orphanCoding.allBatches') },
      ...this.batchNumbers.map(b => ({ id: b.batchNo, name: b.batchNo }))
    ];
  }

  onBatchFilterChange(): void {
    this.loadHistory();
  }

  /** UC-ORP-09 — the orphan's rows within the chosen batch */
  openDetails(paymentGroupId: string): void {
    this.detailLoading = true;

    this.orphanPaymentService.getOrphanPaymentDetails(paymentGroupId, this.orphan.orphanId).subscribe({
      next: (details) => {
        this.detail = details;
        this.detailLoading = false;
        this.cdr.markForCheck(); // P2 — OnPush: HTTP callbacks must mark
      },
      error: (error: any) => {
        console.error('Error loading payment details:', error);
        this.notification.error(error.message || this.translate.instant('orphanCoding.detailsLoadFailed'));
        this.detailLoading = false;
        this.cdr.markForCheck();
      }
    });
  }

  closeDetails(): void {
    this.detail = null;
  }

  close(): void {
    this.closed.emit();
  }

  trackByPaymentGroupId = (_: number, payment: OrphanPaymentDto): string => payment.id;
  trackByBatchOption = (_: number, option: { id: string; name: string }): string => option.id;
  trackByOrphanRow = (_: number, row: any): string => row.id;
}
