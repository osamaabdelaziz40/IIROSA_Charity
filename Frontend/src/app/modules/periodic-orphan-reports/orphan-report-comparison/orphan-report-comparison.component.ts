/**
 * Orphan Report Comparison Component
 * Implements UC-6.10: Compare Period Reports
 *
 * Allows Admin/Super Admin to compare orphan statistics between
 * different periods across all charities with visual charts/graphs
 *
 * Access: Admin and Super Admin only
 */

import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { TranslateModule, TranslateService } from '@ngx-translate/core';

import { OrphanReportService } from '../services/orphan-report.service';
import {
  OrphanReportHistoryDto,
  OrphanReportComparisonDto,
  OrphanReportComparisonMetricsDto
} from '../models/periodic-orphan-report.model';
import { LoadingComponent } from '../../../shared/components/loading/loading.component';

import { BreadcrumbComponent } from '../../../shared/components/breadcrumb/breadcrumb.component';
@Component({
  selector: 'app-orphan-report-comparison',
  standalone: true,
  imports: [
    BreadcrumbComponent,

    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    TranslateModule,
    LoadingComponent
  ],
  templateUrl: './orphan-report-comparison.component.html',
  styleUrls: ['./orphan-report-comparison.component.scss']
})
export class OrphanReportComparisonComponent implements OnInit {
  // Form for selecting reports to compare
  comparisonForm: FormGroup;

  // Data
  loading = false;
  comparing = false;
  reportHistory: OrphanReportHistoryDto[] = [];
  comparisonResult?: OrphanReportComparisonDto;

  // Comparison metrics options - UC-6.10
  comparisonMetrics = [
    { key: 'totalCount', label: 'orphanReports.totalOrphanCount', checked: true },
    { key: 'sponsoredVsUnsponsored', label: 'orphanReports.sponsoredVsUnsponsored', checked: true },
    { key: 'ageDistribution', label: 'orphanReports.ageDistribution', checked: false },
    { key: 'genderDistribution', label: 'orphanReports.genderDistribution', checked: true },
    { key: 'charityDistribution', label: 'orphanReports.charityDistribution', checked: true }
  ];

  constructor(
    private fb: FormBuilder,
    private orphanReportService: OrphanReportService,
    private translate: TranslateService
  ) {
    this.comparisonForm = this.buildForm();
  }

  ngOnInit(): void {
    this.loadReportHistory();
  }

  /**
   * Build comparison form - UC-6.10
   */
  private buildForm(): FormGroup {
    return this.fb.group({
      report1Id: ['', Validators.required],
      report2Id: ['', Validators.required]
    });
  }

  /**
   * Load report history for comparison selection - UC-6.10
   */
  private loadReportHistory(): void {
    this.loading = true;

    this.orphanReportService.getReportHistory(1, 100).subscribe({
      next: (result) => {
        this.reportHistory = result.items;
        this.loading = false;
      },
      error: (error) => {
        console.error('Error loading report history:', error);
        this.loading = false;
      }
    });
  }

  /**
   * Compare selected reports - UC-6.10
   */
  compareReports(): void {
    if (this.comparisonForm.invalid) {
      this.comparisonForm.markAllAsTouched();
      return;
    }

    const report1Id = this.comparisonForm.value.report1Id;
    const report2Id = this.comparisonForm.value.report2Id;

    // Check if same report selected
    if (report1Id === report2Id) {
      alert('Please select two different reports to compare');
      return;
    }

    this.comparing = true;

    this.orphanReportService.comparePeriods(report1Id, report2Id).subscribe({
      next: (result) => {
        this.comparisonResult = result;
        this.comparing = false;
      },
      error: (error) => {
        console.error('Error comparing reports:', error);
        this.comparing = false;
      }
    });
  }

  /**
   * Get selected metrics
   */
  get selectedMetrics(): string[] {
    return this.comparisonMetrics.filter(m => m.checked).map(m => m.key);
  }

  /**
   * Check if metric is selected
   */
  isMetricVisible(metricKey: string): boolean {
    return this.comparisonMetrics.find(m => m.key === metricKey)?.checked || false;
  }

  /**
   * Get change class (positive/negative)
   */
  getChangeClass(value: number): string {
    if (value > 0) return 'text-success';
    if (value < 0) return 'text-danger';
    return 'text-secondary';
  }

  /**
   * Get change icon
   */
  getChangeIcon(value: number): string {
    if (value > 0) return 'fa-arrow-up';
    if (value < 0) return 'fa-arrow-down';
    return 'fa-minus';
  }

  /**
   * Format percentage
   */
  formatPercent(value: number): string {
    return `${value.toFixed(1)}%`;
  }

  /**
   * Get report label
   */
  getReportLabel(reportId: string, reportNum: number): string {
    const report = this.reportHistory.find(r => r.reportId === reportId);
    if (report) {
      // Pipe syntax is template-only; resolve the label and dates in TS instead.
      const name = report.reportName || this.translate.instant('orphanReports.untitledReport');
      // Review P28 2026-08-24: bare toLocaleDateString() renders in whatever
      // locale the browser carries (Arabic-Indic digits on ar browsers, en-US
      // elsewhere) — format local date parts for the module's yyyy-MM-dd shape.
      const from = this.fmtDate(report.reportFromDate);
      const to = this.fmtDate(report.reportToDate);
      return `${name} (${from} - ${to})`;
    }
    // Review P27 2026-08-24: the hard-coded English fallback leaked "Report 1"
    // into the Arabic UI — take it from the glossary like everything else.
    return this.translate.instant('orphanReports.reportN', { n: reportNum });
  }

  private fmtDate(value?: string | null): string {
    if (!value) return '';
    const d = new Date(value);
    if (isNaN(d.getTime())) return '';
    const pad = (n: number) => String(n).padStart(2, '0');
    return `${d.getFullYear()}-${pad(d.getMonth() + 1)}-${pad(d.getDate())}`;
  }

  /**
   * Reset comparison
   */
  resetComparison(): void {
    this.comparisonForm.reset();
    this.comparisonResult = undefined;
  }
}
