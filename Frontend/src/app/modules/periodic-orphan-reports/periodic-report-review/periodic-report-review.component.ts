/**
 * Periodic Report Review Component
 * Implements UC-6.13: Review Periodic Report
 *
 * Allows Admin, Super Admin, Accountant, and Employee to:
 * - Review comprehensive periodic orphan report details
 * - Approve report (set Reviewed=true, IsAccepted=true)
 * - Reject report (set Reviewed=true, IsRefused=true with reason)
 *
 * Access: Super Admin, Admin, Accountant, Employee only
 */

import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { TranslateModule } from '@ngx-translate/core';

import { PeriodicOrphanReportService } from '../../services/periodic-orphan-report.service';
import {
  PeriodicOrphanReportDto,
  ReviewPeriodicReportDto
} from '../../models/periodic-orphan-report.model';
import { LoadingComponent } from '../../../shared/components/loading/loading.component';

@Component({
  selector: 'app-periodic-report-review',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    TranslateModule,
    LoadingComponent
  ],
  templateUrl: './periodic-report-review.component.html',
  styleUrls: ['./periodic-report-review.component.scss']
})
export class PeriodicReportReviewComponent implements OnInit {
  // Form - UC-6.13
  reviewForm: FormGroup;

  // Data
  loading = true;
  submitting = false;
  report?: PeriodicOrphanReportDto;
  reportId?: string;

  // Refusal reasons (would be populated from lookup)
  refusalReasons = [
    { id: 1, name: 'Incomplete Information' },
    { id: 2, name: 'Missing Documents' },
    { id: 3, name: 'Inaccurate Data' },
    { id: 4, name: 'Needs Verification' },
    { id: 5, name: 'Other' }
  ];

  constructor(
    private fb: FormBuilder,
    private route: ActivatedRoute,
    private router: Router,
    private periodicReportService: PeriodicOrphanReportService
  ) {
    this.reviewForm = this.buildForm();
  }

  ngOnInit(): void {
    this.reportId = this.route.snapshot.params['id'];
    if (this.reportId) {
      this.loadReport(this.reportId);
    }
  }

  /**
   * Build review form - UC-6.13
   */
  private buildForm(): FormGroup {
    return this.fb.group({
      isApproved: [null, Validators.required],
      refuseReason: [''],
      refuseReasonId: [''],
      reviewComments: ['']
    });
  }

  /**
   * Load report for review - UC-6.13
   */
  private loadReport(id: string): void {
    this.loading = true;

    this.periodicReportService.getReport(id).subscribe({
      next: (report: PeriodicOrphanReportDto) => {
        this.report = report;

        // Check if already reviewed - UC-6.13
        if (report.reviewed) {
          console.log('Report already reviewed');
        }

        this.loading = false;
      },
      error: (error) => {
        console.error('Error loading report:', error);
        this.loading = false;
      }
    });
  }

  /**
   * Approve report - UC-6.13
   */
  approveReport(): void {
    if (!this.reportId) return;

    this.reviewForm.patchValue({ isApproved: true });
    this.submitReview();
  }

  /**
   * Reject report - UC-6.13
   */
  rejectReport(): void {
    if (!this.reportId) return;

    // Require refusal reason - UC-6.13
    const refuseReason = this.reviewForm.value.refuseReason;
    const refuseReasonId = this.reviewForm.value.refuseReasonId;

    if (!refuseReason && !refuseReasonId) {
      alert('Refuse reason is required');
      return;
    }

    this.reviewForm.patchValue({ isApproved: false });
    this.submitReview();
  }

  /**
   * Submit review - UC-6.13
   */
  private submitReview(): void {
    if (!this.reportId) return;

    this.submitting = true;

    const review: ReviewPeriodicReportDto = {
      reportId: this.reportId,
      isApproved: this.reviewForm.value.isApproved,
      refuseReason: this.reviewForm.value.refuseReason,
      refuseReasonId: this.reviewForm.value.refuseReasonId,
      reviewComments: this.reviewForm.value.reviewComments
    };

    this.periodicReportService.reviewReport(this.reportId, review).subscribe({
      next: () => {
        this.submitting = false;
        // Navigate back to list
        this.router.navigate(['../../'], { relativeTo: this.route });
      },
      error: (error) => {
        console.error('Error submitting review:', error);
        this.submitting = false;
      }
    });
  }

  /**
   * Cancel and go back
   */
  cancel(): void {
    this.router.navigate(['../../'], { relativeTo: this.route });
  }

  /**
   * Get status badge class
   */
  getStatusClass(status: string): string {
    switch (status.toLowerCase()) {
      case 'approved':
      case 'accepted':
        return 'bg-success';
      case 'rejected':
      case 'refused':
        return 'bg-danger';
      case 'pending':
        return 'bg-warning';
      default:
        return 'bg-secondary';
    }
  }

  /**
   * Check if refuse reason is required
   */
  get refuseReasonRequired(): boolean {
    return this.reviewForm.value.isApproved === false;
  }
}
