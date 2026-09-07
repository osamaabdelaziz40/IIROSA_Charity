/**
 * Max Transfer Amount Component (epic 17, UC-TRF-07 — §22.S.4)
 * The per-country ceiling grid: الرقم · البلد · قيمة الحوالة (editable) · حفظ per row.
 * Empty value = NULL = unlimited (legal); 0/negative is refused client and server side.
 *
 * Write is SuperAdmin-only — the PUT endpoint authorises (403); hiding the save UI for
 * Admin is UX, not the control (they still see every ceiling read-only).
 */

import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';

import { HqTransferService } from '../services/hq-transfer.service';
import { CountryMaxTransferAmount } from '../models/hq-transfer.model';
import { LookupManagementService } from '../../lookup-management/services/lookup-management.service';
import { CountryDto } from '../../lookup-management/models/lookup.model';
import { AuthService } from '../../../core/services/auth.service';
import { NotificationService } from '../../../core/services/notification.service';
import { BreadcrumbComponent, BreadcrumbItem, PageHeaderComponent } from '../../../shared/components';

/** One §22.S.4 grid row — editValue tracks the input, savedValue the persisted ceiling */
interface MaxAmountRow {
  id: number;
  name: string;
  savedValue: number | null;
  editValue: number | null;
  saving: boolean;
  error: string | null;
}

@Component({
  selector: 'app-max-transfer-amount',
  standalone: true,
  imports: [CommonModule, FormsModule, TranslateModule, BreadcrumbComponent, PageHeaderComponent],
  templateUrl: './max-transfer-amount.component.html',
  styleUrls: ['./max-transfer-amount.component.scss']
})
export class MaxTransferAmountComponent implements OnInit, OnDestroy {
  private destroy$ = new Subject<void>();

  rows: MaxAmountRow[] = [];
  loading = false;

  /** SuperAdmin only (UC-TRF-07); the endpoint re-authorises regardless */
  canManage = false;

  breadcrumbs: BreadcrumbItem[] = [
    { label: 'common.home', url: '/dashboard' },
    { label: 'hqTransfers.title', url: '/hq-transfers' },
    { label: 'hqTransfers.maxAmounts.title' }
  ];

  constructor(
    private transferService: HqTransferService,
    private lookupService: LookupManagementService,
    private authService: AuthService,
    private translate: TranslateService,
    private notification: NotificationService
  ) {}

  ngOnInit(): void {
    this.canManage = this.authService.hasPermission('HqTransfers.ManageLimits');
    this.loadCountries();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  /**
   * The §22.S.4 grid source — the active country catalogue; CountryDto carries the current
   * ceiling (maxTransferAmount, nullable) from 17-6.
   */
  private loadCountries(): void {
    this.loading = true;

    this.lookupService.getCountries({ isActive: true, page: 1, pageSize: 1000 })
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: result => {
          this.rows = (result.items || []).map(country => ({
            id: country.id,
            name: country.name || country.nameAr || country.nameEn || '',
            savedValue: country.maxTransferAmount ?? null,
            editValue: country.maxTransferAmount ?? null,
            saving: false,
            error: null
          }));
          this.loading = false;
        },
        error: () => {
          this.loading = false;
          this.notification.error(this.translate.instant('hqTransfers.maxAmounts.loadFailed'));
        }
      });
  }

  /**
   * Per-row «حفظ» — PUT /api/HqTransfers/max-amount. Empty input saves NULL (unlimited).
   * 0/negative is flagged on the row from the server's errors map; 403 toasts and changes
   * nothing.
   */
  saveRow(row: MaxAmountRow): void {
    const value = row.editValue;

    // Client pre-check (UX only — the validator re-checks server-side)
    if (value !== null && value <= 0) {
      row.error = this.translate.instant('hqTransfers.maxAmounts.invalidAmount');
      return;
    }

    row.saving = true;
    row.error = null;

    this.transferService.updateMaxTransferAmount({ countryId: row.id, maxTransferAmount: value })
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (result: CountryMaxTransferAmount) => {
          row.savedValue = result.maxTransferAmount ?? null;
          row.editValue = row.savedValue;
          row.saving = false;
          this.notification.success(this.translate.instant('hqTransfers.maxAmounts.saved'));
        },
        error: (httpError) => {
          row.saving = false;

          const errors = httpError?.error?.errors;
          if (errors && typeof errors === 'object') {
            const amountMessages = errors['MaxTransferAmount'] || errors['maxTransferAmount'];
            row.error = Array.isArray(amountMessages)
              ? amountMessages.join(' · ')
              : (amountMessages || null);
          }
          if (httpError?.status === 403) {
            this.notification.error(this.translate.instant('hqTransfers.maxAmounts.noPermission'));
          } else {
            this.notification.error(
              httpError?.error?.message || this.translate.instant('hqTransfers.maxAmounts.saveFailed'));
          }
        }
      });
  }

  /** Whether the row differs from its persisted value (drives the save button state) */
  isDirty(row: MaxAmountRow): boolean {
    return row.editValue !== row.savedValue;
  }

  // ========== TrackBys ==========

  trackRow(index: number, row: MaxAmountRow): number {
    return row.id;
  }
}
