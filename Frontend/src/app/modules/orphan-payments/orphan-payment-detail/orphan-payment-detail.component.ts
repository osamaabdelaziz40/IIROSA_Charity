import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { RouterModule } from '@angular/router';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { Observable, Subject, takeUntil } from 'rxjs';
import { switchMap } from 'rxjs/operators';
import {
  OrphanPaymentDto,
  OrphanPaymentItemDto,
  ExportPaymentGroupOptions
} from '../models/orphan-payment.model';
import { OrphanPaymentService } from '../services/orphan-payment.service';

@Component({
  selector: 'app-orphan-payment-detail',
  standalone: true,
  imports: [CommonModule, FormsModule, TranslateModule, RouterModule],
  templateUrl: './orphan-payment-detail.component.html',
  styleUrls: ['./orphan-payment-detail.component.scss']
})
export class OrphanPaymentDetailComponent implements OnInit, OnDestroy {
  private destroy$ = new Subject<void>();

  // Data
  paymentGroup: OrphanPaymentDto | null = null;
  orphans: OrphanPaymentItemDto[] = [];
  totalOrphans = 0;
  loading = false;
  loadingOrphans = false;

  // Statistics
  statistics: {
    byCharity: { charityId: number; charityName: string; orphanCount: number }[];
    byRegion: { regionId: number; regionName: string; orphanCount: number }[];
    totalOrphans: number;
  } | null = null;

  // Audit Log
  auditLogs: any[] = [];
  showingAuditLog = false;
  loadingAuditLog = false;

  // Export Options
  showExportModal = false;
  exportOptions: ExportPaymentGroupOptions = {
    format: 'Excel',
    includePhotos: false,
    groupBy: 'None'
  };

  constructor(
    private router: Router,
    private route: ActivatedRoute,
    private orphanPaymentService: OrphanPaymentService,
    private translate: TranslateService
  ) {}

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.loadPaymentGroup(id);
      this.loadGroupOrphans(id);
    }
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  // ==================== LOADING DATA ====================

  loadPaymentGroup(id: string): void {
    this.loading = true;
    this.orphanPaymentService.getOrphanPayment(id).subscribe({
      next: (data) => {
        this.paymentGroup = data;
        this.loading = false;
      },
      error: () => {
        this.loading = false;
      }
    });
  }

  loadGroupOrphans(id: string, page: number = 1): void {
    this.loadingOrphans = true;
    this.orphanPaymentService.getGroupOrphans(id, page, 50).subscribe({
      next: (data) => {
        this.orphans = data.items;
        this.totalOrphans = data.totalCount;
        this.loadingOrphans = false;
      },
      error: () => {
        this.loadingOrphans = false;
      }
    });
  }

  loadStatistics(id: string): void {
    this.orphanPaymentService.getGroupStatistics(id).subscribe({
      next: (data) => {
        this.statistics = data;
      }
    });
  }

  loadAuditLog(id: string): void {
    this.loadingAuditLog = true;
    this.orphanPaymentService.getAuditLogs(id).subscribe({
      next: (data) => {
        this.auditLogs = data;
        this.loadingAuditLog = false;
        this.showingAuditLog = true;
      },
      error: () => {
        this.loadingAuditLog = false;
      }
    });
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

  onRemoveOrphan(orphanId: string): void {
    if (!this.paymentGroup) return;

    if (confirm(this.translate.instant('orphanPayments.removeOrphanConfirm'))) {
      this.orphanPaymentService.removeOrphanFromGroup(this.paymentGroup.id, { orphanId }).subscribe({
        next: () => {
          this.loadGroupOrphans(this.paymentGroup!.id);
        }
      });
    }
  }

  onMarkAsUploaded(): void {
    if (!this.paymentGroup) return;

    if (confirm(this.translate.instant('orphanPayments.markAsUploadedConfirm'))) {
      this.orphanPaymentService.markAsUploaded(this.paymentGroup.id).subscribe({
        next: () => {
          this.loadPaymentGroup(this.paymentGroup!.id);
        }
      });
    }
  }

  onUnmarkAsUploaded(): void {
    if (!this.paymentGroup) return;

    if (confirm(this.translate.instant('orphanPayments.unmarkAsUploadedConfirm'))) {
      this.orphanPaymentService.unmarkAsUploaded(this.paymentGroup.id).subscribe({
        next: () => {
          this.loadPaymentGroup(this.paymentGroup!.id);
        }
      });
    }
  }

  onDelete(): void {
    if (!this.paymentGroup) return;

    if (confirm(this.translate.instant('orphanPayments.deleteGroupConfirm'))) {
      this.orphanPaymentService.deleteOrphanPayment(this.paymentGroup.id).subscribe({
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

    this.orphanPaymentService.exportGroup(this.paymentGroup.id, this.exportOptions).subscribe({
      next: (blob) => {
        const extension = this.exportOptions.format === 'Excel' ? 'xlsx' : 'pdf';
        const filename = `PaymentGroup_${this.paymentGroup?.batchNo || this.paymentGroup?.id}.${extension}`;
        this.orphanPaymentService.downloadFile(blob, filename);
        this.closeExportModal();
      }
    });
  }

  onPrint(): void {
    if (!this.paymentGroup) return;

    this.orphanPaymentService.printGroup(this.paymentGroup.id).subscribe(html => {
      const printWindow = window.open('', '_blank');
      if (printWindow) {
        printWindow.document.write(html);
        printWindow.document.close();
        printWindow.print();
      }
    });
  }

  // ==================== HELPERS ====================

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

  formatCurrency(amount: number): string {
    return this.orphanPaymentService.formatCurrency(amount, this.paymentGroup?.currency || 'EGP');
  }

  toggleAuditLog(): void {
    if (!this.showingAuditLog && this.paymentGroup) {
      this.loadAuditLog(this.paymentGroup.id);
    } else {
      this.showingAuditLog = false;
    }
  }

  toggleStatistics(): void {
    if (!this.statistics && this.paymentGroup) {
      this.loadStatistics(this.paymentGroup.id);
    }
  }

  getAuditActionClass(action: string): string {
    switch (action.toLowerCase()) {
      case 'create': return 'badge-success';
      case 'update': return 'badge-primary';
      case 'delete': return 'badge-danger';
      case 'status': return 'badge-warning';
      case 'upload': return 'badge-info';
      default: return 'badge-secondary';
    }
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
    return orphan.orphanId;
  }
}
