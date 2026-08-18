/**
 * Orphan Reports List Component
 * List view for orphan reports with navigation to generate new reports
 * Implements UC-6.1 entry point
 */

import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { TranslateModule } from '@ngx-translate/core';

import { OrphanReportService } from '../../services/orphan-report.service';
import { OrphanReportHistoryDto } from '../../models/periodic-orphan-report.model';
import { LoadingComponent } from '../../../shared/components/loading/loading.component';
import { EmptyStateComponent } from '../../../shared/components/empty-state/empty-state.component';

@Component({
  selector: 'app-orphan-reports-list',
  standalone: true,
  imports: [
    CommonModule,
    TranslateModule,
    LoadingComponent,
    EmptyStateComponent
  ],
  templateUrl: './orphan-reports-list.component.html',
  styleUrls: ['./orphan-reports-list.component.scss']
})
export class OrphanReportsListComponent implements OnInit {
  loading = false;
  recentReports: OrphanReportHistoryDto[] = [];

  constructor(
    private router: Router,
    private orphanReportService: OrphanReportService
  ) {}

  ngOnInit(): void {
    this.loadRecentReports();
  }

  private loadRecentReports(): void {
    this.loading = true;
    this.orphanReportService.getReportHistory(1, 5).subscribe({
      next: (result) => {
        this.recentReports = result.items;
        this.loading = false;
      },
      error: (error) => {
        console.error('Error loading recent reports:', error);
        this.loading = false;
      }
    });
  }

  navigateToGenerate(): void {
    this.router.navigate(['/orphan-reports/generate']);
  }

  navigateToHistory(): void {
    this.router.navigate(['/orphan-reports/history']);
  }

  navigateToSchedule(): void {
    this.router.navigate(['/orphan-reports/schedule']);
  }

  navigateToCompare(): void {
    this.router.navigate(['/orphan-reports/compare']);
  }
}
