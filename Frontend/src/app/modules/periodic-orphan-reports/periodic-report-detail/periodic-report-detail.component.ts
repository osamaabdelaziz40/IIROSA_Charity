/**
 * Periodic Report Detail Component
 * View comprehensive details of a periodic orphan report
 * Implements viewing for UC-6.11, UC-6.14, UC-6.15
 */

import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { TranslateModule } from '@ngx-translate/core';

import { PeriodicOrphanReportService } from '../../services/periodic-orphan-report.service';
import { PeriodicOrphanReportDto } from '../../models/periodic-orphan-report.model';
import { LoadingComponent } from '../../../shared/components/loading/loading.component';

@Component({
  selector: 'app-periodic-report-detail',
  standalone: true,
  imports: [
    CommonModule,
    TranslateModule,
    LoadingComponent
  ],
  templateUrl: './periodic-report-detail.component.html',
  styleUrls: ['./periodic-report-detail.component.scss']
})
export class PeriodicReportDetailComponent implements OnInit {
  loading = true;
  report?: PeriodicOrphanReportDto;
  reportId?: string;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private periodicReportService: PeriodicOrphanReportService
  ) {}

  ngOnInit(): void {
    this.reportId = this.route.snapshot.params['id'];
    if (this.reportId) {
      this.loadReport(this.reportId);
    }
  }

  private loadReport(id: string): void {
    this.loading = true;
    this.periodicReportService.getReport(id).subscribe({
      next: (report) => {
        this.report = report;
        this.loading = false;
      },
      error: (error) => {
        console.error('Error loading report:', error);
        this.loading = false;
      }
    });
  }

  editReport(): void {
    if (this.reportId) {
      this.router.navigate([this.reportId, 'edit'], { relativeTo: this.route.parent });
    }
  }

  deleteReport(): void {
    if (!this.reportId) return;
    if (confirm('Are you sure you want to delete this report?')) {
      this.periodicReportService.deleteReport(this.reportId).subscribe({
        next: () => {
          this.router.navigate(['../'], { relativeTo: this.route });
        },
        error: (error) => {
          console.error('Error deleting report:', error);
        }
      });
    }
  }

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

  get canEdit(): boolean {
    return this.report && !this.report.locked && !this.report.reviewed;
  }

  get canDelete(): boolean {
    return this.report && !this.report.locked;
  }
}
