import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { TranslateModule, TranslateService } from '@ngx-translate/core';

import { TechnicalSupportService } from '../services/technical-support.service';
import {
  SupportReportRequest,
  SupportReport,
  SupportTicket
} from '../../../core/models/technical-support.model';
import { NotificationService } from '../../../core/services/notification.service';

@Component({
  selector: 'app-support-report',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    RouterModule,
    TranslateModule
  ],
  templateUrl: './support-report.component.html',
  styleUrls: ['./support-report.component.scss']
})
export class SupportReportComponent {
  // POST /api/SupportTickets/report takes just the date range
  reportRequest: SupportReportRequest = {
    startDate: this.getDefaultStartDate(),
    endDate: this.getDefaultEndDate()
  };

  report: SupportReport | null = null;
  loading: boolean = false;
  generating: boolean = false;

  constructor(
    private technicalSupportService: TechnicalSupportService,
    private translate: TranslateService,
    private notification: NotificationService
  ) {}

  generateReport(): void {
    this.generating = true;
    this.technicalSupportService.generateReport(this.reportRequest).subscribe({
      next: (report: SupportReport) => {
        this.report = report;
        this.generating = false;
      },
      error: () => {
        this.generating = false;
        this.notification.error(this.translate.instant('technicalSupport.messages.operationFailed'));
      }
    });
  }

  /**
   * Client-side CSV export of the report's ticket list —
   * the API has no server-side export endpoint.
   */
  exportReport(): void {
    if (!this.report) {
      return;
    }

    const header = [
      this.translate.instant('technicalSupport.ticketTitle'),
      this.translate.instant('technicalSupport.category'),
      this.translate.instant('technicalSupport.priority'),
      this.translate.instant('technicalSupport.status'),
      this.translate.instant('technicalSupport.createdBy'),
      this.translate.instant('technicalSupport.createdDate'),
      this.translate.instant('technicalSupport.isSolved')
    ];

    const rows = this.report.tickets.map(t => [
      t.title,
      t.categoryName ?? '',
      t.priorityName ?? '',
      t.statusName ?? '',
      t.createdByUserName ?? '',
      t.createdOn ?? '',
      t.isSolved ? this.translate.instant('common.yes') : this.translate.instant('common.no')
    ]);

    const csv = [header, ...rows]
      .map(row => row.map(cell => this.escapeCsvCell(cell)).join(','))
      .join('\n');

    const blob = new Blob(['﻿' + csv], { type: 'text/csv;charset=utf-8;' });
    const url = window.URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = `support-report-${this.reportRequest.startDate}_${this.reportRequest.endDate}.csv`;
    a.click();
    window.URL.revokeObjectURL(url);
  }

  printReport(): void {
    window.print();
  }

  // Dictionary<string, number> entries as key/value pairs for the tables
  toEntries(dict: Record<string, number> | undefined): { key: string; value: number }[] {
    return Object.entries(dict ?? {}).map(([key, value]) => ({ key, value }));
  }

  trackByEntryKey(index: number, entry: { key: string; value: number }): string {
    return entry.key;
  }

  trackByTicketId(index: number, ticket: SupportTicket): string {
    return ticket.id;
  }

  /**
   * CSV cell escaping — doubles embedded quotes and neutralizes spreadsheet
   * formula injection by prefixing an apostrophe when a cell starts with
   * = + - @ or a tab/CR (OWASP CSV injection guidance).
   */
  private escapeCsvCell(value: string | number | boolean | undefined): string {
    const text = String(value ?? '');
    const needsPrefix = /^[=+\-@\t\r]/.test(text);
    return `"${(needsPrefix ? "'" + text : text).replace(/"/g, '""')}"`;
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

  formatDuration(hours: number | undefined): string {
    const h = hours ?? 0;
    if (h < 1) {
      const minutes = Math.round(h * 60);
      return `${minutes}m`;
    }
    if (h < 24) {
      return `${Math.round(h)}h`;
    }
    const days = Math.round(h / 24);
    return `${days}d`;
  }
}
