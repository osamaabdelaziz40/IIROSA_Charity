/**
 * HQ Transfer List Component (epic 17, UC-TRF-01)
 * The §22.S.1 register: no filter fields, 11 data columns + الاجراءات, serial numbering.
 * Loads via GET /api/HqTransfers — scoped server-side to the caller's country claim.
 *
 * 17-2 wires the page-header add action (§22.S.2 form); the row view/edit icons stay
 * DISABLED until 17-3/17-4 wire them.
 */

import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';

import { HqTransferService } from '../services/hq-transfer.service';
import { HqTransfer, HqTransferStatistics } from '../models/hq-transfer.model';
import { AuthService } from '../../../core/services/auth.service';
import { NotificationService } from '../../../core/services/notification.service';
import { PaginationComponent, BreadcrumbComponent, PageHeaderComponent, BreadcrumbItem } from '../../../shared/components';
import { SharedModule } from '../../../shared/shared.module';
import type { PageAction } from '../../../shared/components/page-header/page-header.component';

@Component({
  selector: 'app-hq-transfer-list',
  standalone: true,
  imports: [CommonModule, FormsModule, TranslateModule, PaginationComponent, BreadcrumbComponent, PageHeaderComponent, SharedModule],
  templateUrl: './hq-transfer-list.component.html',
  styleUrls: ['./hq-transfer-list.component.scss']
})
export class HqTransferListComponent implements OnInit, OnDestroy {
  private destroy$ = new Subject<void>();

  // Data
  transfers: HqTransfer[] = [];

  // Register statistics band — caller-scoped server-side (country claim)
  statistics: HqTransferStatistics | null = null;

  // Loading state
  loading = false;

  // Pagination
  currentPage = 1;
  pageSize = 20;
  totalCount = 0;
  totalPages = 0;

  // Breadcrumb items
  breadcrumbs: BreadcrumbItem[] = [
    { label: 'common.home', url: '/dashboard' },
    { label: 'hqTransfers.title' }
  ];

  // Page actions — the create route carries its own PermissionGuard (HqTransfers.Create);
  // the endpoint re-authorises, this only offers the navigation. UC-TRF-07's ضبط الحدود
  // appears only for SuperAdmin (HqTransfers.ManageLimits) — hiding is UX, the 403 is the
  // control.
  pageActions: PageAction[] = [
    {
      label: 'hqTransfers.addTransfer',
      icon: 'fe-plus',
      type: 'primary',
      click: () => this.createTransfer()
    },
    {
      label: 'common.exportToExcel',
      icon: 'fe-download',
      type: 'success',
      click: () => this.exportToExcel()
    }
  ];

  constructor(
    protected transferService: HqTransferService,
    protected router: Router,
    protected translate: TranslateService,
    protected notification: NotificationService,
    protected authService: AuthService
  ) {}

  ngOnInit(): void {
    if (this.authService.hasPermission('HqTransfers.ManageLimits')) {
      this.pageActions = [
        {
          label: 'hqTransfers.maxAmounts.manageLimits',
          icon: 'fe-sliders',
          type: 'secondary',
          click: () => this.router.navigate(['/hq-transfers', 'max-amounts'])
        },
        ...this.pageActions
      ];
    }

    this.loadTransfers();
    this.loadStatistics();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  /**
   * Load the register (initial load + pagination) — the caller-scoped read
   */
  loadTransfers(): void {
    this.loading = true;

    this.transferService.getTransfers(this.currentPage, this.pageSize)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (response) => {
          this.transfers = response.items;
          this.totalCount = response.totalCount;
          this.totalPages = response.totalPages;
          this.loading = false;
        },
        error: () => {
          this.loading = false;
          this.notification.error(this.translate.instant('hqTransfers.loadFailed'));
        }
      });
  }

  /**
   * Register statistics band — describes the caller's whole country scope, not the
   * current page. Silent-fail so the register still renders without it.
   */
  loadStatistics(): void {
    this.transferService.getStatistics()
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: statistics => this.statistics = statistics,
        error: () => console.error('Error loading HQ transfer statistics')
      });
  }

  /**
   * الرقم — serial column, continuous across pages (13-1 formula)
   */
  serial(index: number): number {
    return (this.currentPage - 1) * this.pageSize + index + 1;
  }

  /**
   * UC-TRF-02: open the §22.S.2 create form
   */
  createTransfer(): void {
    this.router.navigate(['/hq-transfers', 'create']);
  }

  /**
   * Export the §22.S.1 register to Excel (orphan-payments §15.S.1 pattern) — the server
   * ignores paging and writes every row in the caller's country scope
   */
  exportToExcel(): void {
    this.loading = true;

    this.transferService.exportToExcel()
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (blob: Blob) => {
          const url = window.URL.createObjectURL(blob);
          const a = document.createElement('a');
          a.href = url;
          a.download = `hq-transfers_${new Date().toISOString().split('T')[0]}.xlsx`;
          document.body.appendChild(a);
          a.click();
          document.body.removeChild(a);
          window.URL.revokeObjectURL(url);

          this.notification.success(this.translate.instant('common.operationSuccess'));
          this.loading = false;
        },
        error: () => {
          this.loading = false;
          this.notification.error(this.translate.instant('common.operationFailed'));
        }
      });
  }

  /**
   * UC-TRF-03: open the read-only view of one transfer
   */
  viewTransfer(id: string): void {
    this.router.navigate(['/hq-transfers', id, 'view']);
  }

  /**
   * UC-TRF-04: open the edit form of one transfer
   */
  editTransfer(id: string): void {
    this.router.navigate(['/hq-transfers', id, 'edit']);
  }

  /**
   * UC-TRF-08: open the §22.S.3 details screen (transfer allocation lines)
   */
  viewTransferDetails(id: string): void {
    this.router.navigate(['/hq-transfers', id, 'details']);
  }

  // ========== TrackBys ==========

  trackTransfer(index: number, transfer: HqTransfer): string {
    return transfer.id;
  }

  // ========== Pagination ==========

  onPageChange(page: number): void {
    this.currentPage = page;
    this.loadTransfers();
  }
}
