import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router, Params } from '@angular/router';
import {
  Check,
  CheckStatus,
  Currency,
  BeneficiaryType,
  PaymentReason,
  VoidReason
} from '../models/check.model';
import { GeneralChecksService } from '../services/general-checks.service';
import { NotificationService } from '../../../core/services/notification.service';
import { TranslateModule } from '@ngx-translate/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { BreadcrumbComponent, BreadcrumbItem } from '../../../shared/components';

@Component({
  selector: 'app-check-detail',
  standalone: true,
  imports: [CommonModule, TranslateModule, FormsModule, ReactiveFormsModule, BreadcrumbComponent],
  templateUrl: './check-detail.component.html',
  styleUrls: ['./check-detail.component.scss']
})
export class CheckDetailComponent implements OnInit {
  // Breadcrumb items
  breadcrumbs: BreadcrumbItem[] = [
    { label: 'common.home', url: '/dashboard' },
    { label: 'generalChecks.title', url: '/general-checks' },
    { label: 'generalChecks.checkDetails' }
  ];
  check: Check | null = null;
  loading = false;
  error: string | null = null;

  // Action modals
  showClearModal = false;
  showVoidModal = false;

  // Clearance form
  clearanceForm!: FormGroup;

  // Void form
  voidForm!: FormGroup;

  // Void reasons
  voidReasons = Object.values(VoidReason);

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private checksService: GeneralChecksService,
    private notification: NotificationService,
    private fb: FormBuilder
  ) {
    this.initForms();
  }

  ngOnInit(): void {
    this.route.params.subscribe((params: Params) => {
      const checkId = params['id'];
      if (checkId) {
        this.loadCheck(+checkId);
      }
    });
  }

  private initForms(): void {
    this.clearanceForm = this.fb.group({
      clearanceDate: [new Date(), Validators.required],
      bankReference: [''],
      clearanceNotes: ['']
    });

    this.voidForm = this.fb.group({
      voidReason: [null, Validators.required],
      voidDate: [new Date(), Validators.required],
      voidNotes: ['', Validators.required]
    });
  }

  private loadCheck(id: number): void {
    this.loading = true;
    this.error = null;

    this.checksService.getCheckById(id).subscribe({
      next: (check: Check) => {
        this.check = check;
        this.loading = false;
      },
      error: (error: any) => {
        console.error('Error loading check:', error);
        this.error = `Failed to load check: ${error.message || 'Unknown error'}`;
        this.notification.error(this.error);
        this.loading = false;
      }
    });
  }

  onEdit(): void {
    if (this.check?.id) {
      this.router.navigate(['/general-checks', this.check.id, 'edit']);
    }
  }

  onDelete(): void {
    if (confirm('Are you sure you want to delete this check?')) {
      if (this.check?.id) {
        this.loading = true;
        this.checksService.deleteCheck(this.check.id).subscribe({
          next: () => {
            this.notification.success('Check deleted successfully');
            this.router.navigate(['/general-checks']);
          },
          error: (error: any) => {
            console.error('Error deleting check:', error);
            this.notification.error(
              `Failed to delete check: ${error.message || 'Unknown error'}`
            );
            this.loading = false;
          }
        });
      }
    }
  }

  openClearModal(): void {
    this.showClearModal = true;
    this.clearanceForm.patchValue({
      clearanceDate: new Date(),
      bankReference: '',
      clearanceNotes: ''
    });
  }

  closeClearModal(): void {
    this.showClearModal = false;
  }

  onMarkAsCleared(): void {
    if (this.clearanceForm.invalid) {
      this.notification.error('Please fill in required fields');
      return;
    }

    if (this.check?.id) {
      this.loading = true;
      const checkId = this.check.id;
      const request = this.clearanceForm.value;
      this.checksService.markAsCleared(checkId, request).subscribe({
        next: () => {
          this.notification.success('Check marked as cleared');
          this.showClearModal = false;
          this.loadCheck(checkId);
        },
        error: (error: any) => {
          console.error('Error marking check as cleared:', error);
          this.notification.error(
            `Failed to mark check as cleared: ${error.message || 'Unknown error'}`
          );
          this.loading = false;
        }
      });
    }
  }

  openVoidModal(): void {
    this.showVoidModal = true;
    this.voidForm.patchValue({
      voidReason: null,
      voidDate: new Date(),
      voidNotes: ''
    });
  }

  closeVoidModal(): void {
    this.showVoidModal = false;
  }

  onVoidCheck(): void {
    if (this.voidForm.invalid) {
      this.notification.error('Please fill in required fields');
      return;
    }

    if (this.check?.id) {
      this.loading = true;
      const checkId = this.check.id;
      const request = this.voidForm.value;
      this.checksService.voidCheck(checkId, request).subscribe({
        next: () => {
          this.notification.success('Check voided successfully');
          this.showVoidModal = false;
          this.loadCheck(checkId);
        },
        error: (error: any) => {
          console.error('Error voiding check:', error);
          this.notification.error(
            `Failed to void check: ${error.message || 'Unknown error'}`
          );
          this.loading = false;
        }
      });
    }
  }

  onBack(): void {
    this.router.navigate(['/general-checks']);
  }

  formatDate(date: Date | string | undefined): string {
    if (!date) return '-';
    return new Date(date).toLocaleDateString();
  }

  formatCurrency(amount: number): string {
    return new Intl.NumberFormat('en-US', {
      minimumFractionDigits: 2,
      maximumFractionDigits: 2
    }).format(amount);
  }

  getBeneficiaryTypeLabel(type: BeneficiaryType): string {
    return `generalChecks.beneficiaryTypes.${type}`;
  }

  getPaymentReasonLabel(reason: PaymentReason): string {
    return `generalChecks.paymentReasons.${reason}`;
  }

  getCheckStatusLabel(status: CheckStatus): string {
    return `generalChecks.checkStatuses.${status}`;
  }

  getVoidReasonLabel(reason: VoidReason): string {
    return `generalChecks.voidReasons.${reason}`;
  }

  getStatusClass(status: CheckStatus): string {
    switch (status) {
      case CheckStatus.Pending:
        return 'badge badge-warning';
      case CheckStatus.Issued:
        return 'badge badge-info';
      case CheckStatus.Cleared:
        return 'badge badge-success';
      case CheckStatus.Void:
        return 'badge badge-danger';
      default:
        return 'badge badge-secondary';
    }
  }

  canEdit(): boolean {
    return this.check !== null && this.check.checkStatus === CheckStatus.Pending;
  }

  canVoid(): boolean {
    if (!this.check) return false;
    return this.check.checkStatus !== CheckStatus.Cleared &&
           this.check.checkStatus !== CheckStatus.Void;
  }

  canMarkAsCleared(): boolean {
    if (!this.check) return false;
    return this.check.checkStatus === CheckStatus.Issued;
  }

  downloadCheckImage(): void {
    if (this.check?.checkImageUrl) {
      window.open(this.check.checkImageUrl, '_blank');
    }
  }
}
