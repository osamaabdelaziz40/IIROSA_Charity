import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import {
  ReconciliationSummary,
  ReconciliationItem
} from '../models/check.model';
import { GeneralChecksService } from '../services/general-checks.service';
import { NotificationService } from '../../../core/services/notification.service';
import { PageHeaderComponent } from '../../../shared/components/page-header/page-header.component';
import { TranslateModule } from '@ngx-translate/core';
import { RouterModule } from '@angular/router';

@Component({
  selector: 'app-check-reconcile',
  standalone: true,
  imports: [CommonModule, FormsModule, PageHeaderComponent, TranslateModule, RouterModule],
  templateUrl: './check-reconcile.component.html',
  styleUrls: ['./check-reconcile.component.scss']
})
export class CheckReconcileComponent implements OnInit {
  reconciliationSummary: ReconciliationSummary | null = null;
  loading = false;
  uploading = false;

  // File upload
  selectedFile: File | null = null;

  // Manual matching
  selectedCheckId: number | null = null;
  bankReference: string = '';
  clearanceDate: Date = new Date();

  // Tabs
  activeTab: 'import' | 'manual' = 'manual';

  constructor(
    private checksService: GeneralChecksService,
    private notification: NotificationService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.loadUnreconciledChecks();
  }

  loadUnreconciledChecks(): void {
    this.loading = true;
    this.checksService.getUnreconciledChecks().subscribe({
      next: (summary) => {
        this.reconciliationSummary = summary;
        this.loading = false;
      },
      error: (error: any) => {
        console.error('Error loading unreconciled checks:', error);
        this.notification.error(
          `Failed to load unreconciled checks: ${error.message || 'Unknown error'}`
        );
        this.loading = false;
      }
    });
  }

  onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (input.files && input.files.length > 0) {
      this.selectedFile = input.files[0];
    }
  }

  onImportBankStatement(): void {
    if (!this.selectedFile) {
      this.notification.error('Please select a file to import');
      return;
    }

    this.uploading = true;
    this.checksService.importBankStatement(this.selectedFile).subscribe({
      next: (summary) => {
        this.reconciliationSummary = summary;
        this.uploading = false;
        this.notification.success('Bank statement imported successfully');
        this.selectedFile = null;
      },
      error: (error: any) => {
        console.error('Error importing bank statement:', error);
        this.notification.error(
          `Failed to import bank statement: ${error.message || 'Unknown error'}`
        );
        this.uploading = false;
      }
    });
  }

  onManualReconcile(): void {
    if (!this.selectedCheckId) {
      this.notification.error('Please select a check to reconcile');
      return;
    }

    if (!this.bankReference) {
      this.notification.error('Please enter bank reference');
      return;
    }

    this.loading = true;
    this.checksService.reconcileCheck(
      this.selectedCheckId,
      this.bankReference,
      this.clearanceDate
    ).subscribe({
      next: () => {
        this.notification.success('Check reconciled successfully');
        this.selectedCheckId = null;
        this.bankReference = '';
        this.loadUnreconciledChecks();
        this.loading = false;
      },
      error: (error: any) => {
        console.error('Error reconciling check:', error);
        this.notification.error(
          `Failed to reconcile check: ${error.message || 'Unknown error'}`
        );
        this.loading = false;
      }
    });
  }

  onBack(): void {
    this.router.navigate(['/general-checks']);
  }

  formatCurrency(amount: number): string {
    return new Intl.NumberFormat('en-US', {
      minimumFractionDigits: 2,
      maximumFractionDigits: 2
    }).format(amount);
  }

  formatDate(date: Date | string): string {
    if (!date) return '-';
    return new Date(date).toLocaleDateString();
  }

  get unreconciledChecks(): ReconciliationItem[] {
    return this.reconciliationSummary?.items?.filter(item => !item.isReconciled) || [];
  }

  get reconciledChecks(): ReconciliationItem[] {
    return this.reconciliationSummary?.items?.filter(item => item.isReconciled) || [];
  }

  get totalChecksReconciled(): number {
    return this.reconciliationSummary?.reconciledChecks || 0;
  }

  get totalAmount(): number {
    return this.reconciliationSummary?.totalAmount || 0;
  }

  get reconciledAmount(): number {
    return this.reconciliationSummary?.reconciledAmount || 0;
  }

  get unreconciledAmount(): number {
    return this.reconciliationSummary?.unreconciledAmount || 0;
  }
}
