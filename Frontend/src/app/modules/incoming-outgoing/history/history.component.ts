import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { ImportHistoryItem, ExportHistoryItem } from '../models/import-export.model';
import { NotificationService } from '../../../core/services/notification.service';
import { PageHeaderComponent } from '../../../shared/components/page-header/page-header.component';
import { TranslateModule } from '@ngx-translate/core';

@Component({
  selector: 'app-history',
  standalone: true,
  imports: [CommonModule, FormsModule, PageHeaderComponent, TranslateModule],
  templateUrl: './history.component.html',
  styleUrls: ['./history.component.scss']
})
export class HistoryComponent implements OnInit {
  activeTab = 'import';
  importHistory: ImportHistoryItem[] = [];
  exportHistory: ExportHistoryItem[] = [];
  loading = false;

  importFilters = {
    type: 'All',
    startDate: null as Date | null,
    endDate: null as Date | null,
    status: 'All'
  };

  exportFilters = {
    type: 'All',
    startDate: null as Date | null,
    endDate: null as Date | null,
    format: 'All'
  };

  constructor(
    private notification: NotificationService,
    private router: Router
  ) {}

  ngOnInit(): void {
    // TODO: Load history from backend when endpoint is available
    // For now, show empty state
    this.loadImportHistory();
    this.loadExportHistory();
  }

  loadImportHistory(): void {
    // Placeholder - implement when backend endpoint is ready
    this.importHistory = [];
  }

  loadExportHistory(): void {
    // Placeholder - implement when backend endpoint is ready
    this.exportHistory = [];
  }

  rollbackImport(importId: string): void {
    const confirmed = confirm('Are you sure you want to rollback this import? This will soft-delete all imported records.');

    if (confirmed) {
      // TODO: Implement rollback when backend endpoint is ready
      this.notification.warning('Rollback feature will be available in the backend API');
    }
  }

  getStatusBadgeClass(status: string): string {
    const statusMap: { [key: string]: string } = {
      'Success': 'badge-success',
      'PartialSuccess': 'badge-warning',
      'Failed': 'badge-danger',
      'RolledBack': 'badge-secondary'
    };
    return statusMap[status] || 'badge-secondary';
  }

  formatDate(date: Date | string): string {
    if (!date) return '-';
    const d = new Date(date);
    return d.toLocaleDateString('en-GB') + ' ' + d.toLocaleTimeString('en-GB', { hour: '2-digit', minute: '2-digit' });
  }

  formatFileSize(bytes: number): string {
    if (bytes === 0) return '0 Bytes';
    const k = 1024;
    const sizes = ['Bytes', 'KB', 'MB', 'GB'];
    const i = Math.floor(Math.log(bytes) / Math.log(k));
    return Math.round(bytes / Math.pow(k, i) * 100) / 100 + ' ' + sizes[i];
  }

  switchTab(tab: string): void {
    this.activeTab = tab;
  }

  navigateToImport(type: 'Incoming' | 'Outgoing'): void {
    this.router.navigate(['/incoming-outgoing/import', type.toLowerCase()]);
  }

  navigateToExport(type: 'Incoming' | 'Outgoing'): void {
    this.router.navigate(['/incoming-outgoing/export', type.toLowerCase()]);
  }
}
