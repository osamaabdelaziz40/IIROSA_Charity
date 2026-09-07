/**
 * HQ Transfer Details Component (epic 17, UC-TRF-08 — §22.S.3)
 * One transfer's allocation lines: header summary above the §22.S.3 grid
 * (رقم الحوالة · مبلغ الحوالة · التاريخ المتوقع للتحويل · مصير الحوالة · تاريخ التنفيذ ·
 * تاريخ وصول الحوالة · مبلغ الوصول · حفظ), per-row save via PUT {id}/details.
 *
 * State gating (the legacy «Failed Operation» rule): execution/arrival fields are enterable
 * only once مصير الحوالة is تم التنفيذ — enforced client-side as UX and server-side by the
 * validator. Write controls visible only with HqTransfers.Edit; the endpoint authorises.
 */

import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';

import { HqTransferService } from '../services/hq-transfer.service';
import { HqTransferDetails, SaveHqTransferDetailLineRequest } from '../models/hq-transfer.model';
import { AuthService } from '../../../core/services/auth.service';
import { NotificationService } from '../../../core/services/notification.service';
import { BreadcrumbComponent, BreadcrumbItem, PageHeaderComponent } from '../../../shared/components';

/** One §22.S.3 grid row — id null marks a locally added, unsaved line */
interface DetailRow {
  id: string | null;
  transferNumber: string;
  amount: number | null;
  estimatedTransferDate: string | null;
  isExecuted: boolean | null;
  executionDate: string | null;
  arrivalDate: string | null;
  arrivalAmount: number | null;
  saving: boolean;
  errors: { [field: string]: string };
}

@Component({
  selector: 'app-hq-transfer-details',
  standalone: true,
  imports: [CommonModule, FormsModule, TranslateModule, BreadcrumbComponent, PageHeaderComponent],
  templateUrl: './hq-transfer-details.component.html',
  styleUrls: ['./hq-transfer-details.component.scss']
})
export class HqTransferDetailsComponent implements OnInit, OnDestroy {
  private destroy$ = new Subject<void>();

  transferId: string | null = null;
  header: HqTransferDetails | null = null;
  rows: DetailRow[] = [];
  loading = false;
  notFound = false;

  /** Write controls gate on HqTransfers.Edit; the PUT endpoint re-authorises */
  canEdit = false;

  breadcrumbs: BreadcrumbItem[] = [
    { label: 'common.home', url: '/dashboard' },
    { label: 'hqTransfers.title', url: '/hq-transfers' },
    { label: 'hqTransfers.details.title' }
  ];

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private transferService: HqTransferService,
    private authService: AuthService,
    private translate: TranslateService,
    private notification: NotificationService
  ) {}

  ngOnInit(): void {
    this.canEdit = this.authService.hasPermission('HqTransfers.Edit');

    // paramMap, not the snapshot: moving from one transfer's details straight to
    // another's reuses this component instance, and a snapshot read runs once —
    // the first transfer's rows stayed on screen under the second transfer's URL.
    this.route.paramMap
      .pipe(takeUntil(this.destroy$))
      .subscribe(params => {
        this.transferId = params.get('id');
        this.header = null;
        this.rows = [];
        this.notFound = false;
        if (this.transferId) {
          this.loadDetails(this.transferId);
        }
      });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  /**
   * UC-TRF-08 AC 1: header summary + lines without a page reload; 404 (absent, soft-deleted,
   * or outside the caller's country scope) shows the not-found state
   */
  private loadDetails(id: string): void {
    this.loading = true;

    this.transferService.getTransferDetails(id)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: details => {
          this.header = details;
          this.rows = (details.lines || []).map(line => ({
            id: line.id,
            transferNumber: line.transferNumber,
            amount: line.amount,
            estimatedTransferDate: line.estimatedTransferDate ? line.estimatedTransferDate.split('T')[0] : null,
            isExecuted: line.isExecuted ?? null,
            executionDate: line.executionDate ? line.executionDate.split('T')[0] : null,
            arrivalDate: line.arrivalDate ? line.arrivalDate.split('T')[0] : null,
            arrivalAmount: line.arrivalAmount ?? null,
            saving: false,
            errors: {}
          }));
          this.loading = false;
        },
        error: (httpError) => {
          this.loading = false;
          if (httpError?.status === 404) {
            this.notFound = true;
          } else {
            this.notification.error(this.translate.instant('hqTransfers.details.loadFailed'));
          }
        }
      });
  }

  /**
   * "+ تفاصيل" — one empty editable row locally; nothing is written until its «حفظ»
   */
  addLine(): void {
    this.rows.push({
      id: null,
      transferNumber: '',
      amount: null,
      estimatedTransferDate: null,
      isExecuted: null,
      executionDate: null,
      arrivalDate: null,
      arrivalAmount: null,
      saving: false,
      errors: {}
    });
  }

  /**
   * Per-row «حفظ» — PUT {id}/details. Client pre-check mirrors the server rules (UX only);
   * the server's errors map lands on the row's fields, the row stays editable.
   */
  saveRow(row: DetailRow): void {
    row.errors = {};

    // Client pre-checks — the validator re-runs server-side
    if (!row.transferNumber?.trim()) {
      row.errors['transferNumber'] = this.translate.instant('validation.required');
    }
    if (row.amount === null || row.amount <= 0) {
      row.errors['amount'] = this.translate.instant('hqTransfers.details.invalidAmount');
    }
    // «Failed Operation» state gating — execution/arrival only once تم التنفيذ
    if (row.isExecuted !== true && (row.executionDate || row.arrivalDate || row.arrivalAmount !== null)) {
      row.errors['isExecuted'] = this.translate.instant('hqTransfers.details.failedOperation');
    }
    if (Object.keys(row.errors).length > 0) {
      // The specific message sits on the flagged field — the toast is the generic
      // "fix the flagged fields" nudge, never the gating rule's text (which only
      // applies to one of the three checks)
      this.notification.error(this.translate.instant('hqTransfers.fixValidationErrors'));
      return;
    }

    row.saving = true;

    const request: SaveHqTransferDetailLineRequest = {
      id: row.id,
      transferNumber: row.transferNumber.trim(),
      amount: row.amount!,
      estimatedTransferDate: row.estimatedTransferDate || null,
      isExecuted: row.isExecuted,
      executionDate: row.executionDate || null,
      arrivalDate: row.arrivalDate || null,
      arrivalAmount: row.arrivalAmount ?? null
    };

    this.transferService.saveTransferDetailLine(this.transferId!, request)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: saved => {
          row.id = saved.id;
          row.saving = false;
          this.notification.success(this.translate.instant('hqTransfers.details.lineSaved'));
        },
        error: (httpError) => this.handleRowError(row, httpError)
      });
  }

  /**
   * Map the server's field→messages (PascalCase keys → the row's camelCase fields) onto the
   * row; the sum rule (a plain message, no map) toasts localized
   */
  private handleRowError(row: DetailRow, httpError: any): void {
    row.saving = false;

    const errors = httpError?.error?.errors;
    if (errors && typeof errors === 'object') {
      for (const [field, messages] of Object.entries<any>(errors)) {
        const fieldName = field.charAt(0).toLowerCase() + field.slice(1);
        row.errors[fieldName] = Array.isArray(messages) ? messages.join(' · ') : String(messages);
      }
    }

    this.notification.error(
      this.localizeServerMessage(httpError?.error?.message)
      || this.translate.instant('hqTransfers.details.lineSaveFailed'));
  }

  /**
   * Known server business-rule messages arrive as canonical English strings ({ message } on
   * the 400). Recognized ones are swapped for their translated equivalents — amounts ride
   * through verbatim (the server's own formatting); anything unrecognized passes unchanged.
   */
  private localizeServerMessage(message: string | undefined): string | undefined {
    if (!message) {
      return undefined;
    }

    // HqTransferService.SaveTransferDetailLineAsync — the Σ-lines rule
    const sumMatch = message.match(
      /^The sum of detail line amounts \(([\d.,]+)\) exceeds the transfer's payment amount \(([\d.,]+)\)$/);
    if (sumMatch) {
      return this.translate.instant('hqTransfers.details.sumExceedsPayment', {
        sum: sumMatch[1],
        payment: sumMatch[2]
      });
    }

    return message;
  }

  /** مصير الحوالة display label */
  stateLabel(isExecuted: boolean | null): string {
    if (isExecuted === true) {
      return this.translate.instant('hqTransfers.details.executed');
    }
    if (isExecuted === false) {
      return this.translate.instant('hqTransfers.details.notExecuted');
    }
    return '—';
  }

  backToList(): void {
    this.router.navigate(['/hq-transfers']);
  }

  // ========== TrackBys ==========

  trackRow(index: number, row: DetailRow): string {
    return row.id ?? `new-${index}`;
  }
}
