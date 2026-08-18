import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { TranslateModule } from '@ngx-translate/core';

import { TechnicalSupportService } from '../services/technical-support.service';
import {
  SupportReportRequest,
  SupportReport,
  ReportGroupBy
} from '../../../core/models/technical-support.model';

@Component({
  selector: 'app-support-report',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    TranslateModule
  ],
  templateUrl: './support-report.component.html',
  styleUrls: ['./support-report.component.scss']
})
export class SupportReportComponent implements OnInit {
  reportRequest: SupportReportRequest = {
    startDate: this.getDefaultStartDate(),
    endDate: this.getDefaultEndDate(),
    groupBy: ReportGroupBy.Category,
    includeCategories: true,
    includeUsers: true
  };

  report: SupportReport | null = null;
  loading: boolean = false;
  generating: boolean = false;

  groupByOptions = [
    { value: ReportGroupBy.Category, label: 'technicalSupport.category' },
    { value: ReportGroupBy.Status, label: 'technicalSupport.status' },
    { value: ReportGroupBy.User, label: 'technicalSupport.createdBy' },
    { value: ReportGroupBy.Priority, label: 'technicalSupport.priority' }
  ];

  constructor(
    private technicalSupportService: TechnicalSupportService
  ) {}

  ngOnInit(): void {}

  generateReport(): void {
    this.generating = true;
    this.technicalSupportService.generateReport(this.reportRequest).subscribe({
      next: (report: SupportReport) => {
        this.report = report;
        this.generating = false;
      },
      error: () => {
        this.generating = false;
      }
    });
  }

  exportReport(): void {
    if (!this.report) {
      return;
    }

    // Implement export functionality
    console.log('Exporting report...');
  }

  printReport(): void {
    window.print();
  }

  private getDefaultStartDate(): string {
    const date = new Date();
    date.setMonth(date.getMonth() - 1);
    return date.toISOString().split('T')[0];
  }

  private getDefaultEndDate(): string {
    return new Date().toISOString().split('T')[0];
  }

  getPercentage(value: number, total: number): number {
    if (total === 0) return 0;
    return Math.round((value / total) * 100);
  }

  formatDuration(hours: number): string {
    if (hours < 1) {
      const minutes = Math.round(hours * 60);
      return `${minutes}m`;
    }
    if (hours < 24) {
      return `${Math.round(hours)}h`;
    }
    const days = Math.round(hours / 24);
    return `${days}d`;
  }

  getGroupByTranslation(groupBy: ReportGroupBy): string {
    const option = this.groupByOptions.find(opt => opt.value === groupBy);
    return option ? option.label : groupBy;
  }
}
