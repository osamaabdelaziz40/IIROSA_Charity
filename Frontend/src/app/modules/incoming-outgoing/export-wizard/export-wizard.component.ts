import { Component, OnInit, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { IncomingSearchRequest } from '../models/incoming.model';
import { OutgoingSearchRequest } from '../models/outgoing.model';
import { ExportFilters } from '../models/import-export.model';
import { IncomingService } from '../services/incoming.service';
import { OutgoingService } from '../services/outgoing.service';
import { NotificationService } from '../../../core/services/notification.service';
import { PageHeaderComponent } from '../../../shared/components/page-header/page-header.component';
import { TranslateModule } from '@ngx-translate/core';

@Component({
  selector: 'app-export-wizard',
  standalone: true,
  imports: [CommonModule, FormsModule, PageHeaderComponent, TranslateModule],
  templateUrl: './export-wizard.component.html',
  styleUrls: ['./export-wizard.component.scss']
})
export class ExportWizardComponent implements OnInit {
  @Input() fileType: 'Incoming' | 'Outgoing' = 'Incoming';

  filters: ExportFilters = {
    startDate: undefined,
    endDate: undefined,
    departmentId: undefined,
    status: undefined,
    categoryId: undefined,
    year: undefined,
    createdBy: undefined
  };

  availableFields: { key: string; label: string; selected: boolean }[] = [];
  exportFormat: 'Excel' | 'PDF' | 'CSV' = 'Excel';
  language: 'Arabic' | 'English' | 'Both' = 'Both';
  includeAttachments = false;
  sortBy = 'date';
  sortOrder: 'Ascending' | 'Descending' = 'Descending';
  exporting = false;

  incomingFields = [
    { key: 'serial', label: 'Serial' },
    { key: 'subject', label: 'Subject' },
    { key: 'date', label: 'Date' },
    { key: 'body', label: 'Body' },
    { key: 'letterNumber', label: 'Letter Number' },
    { key: 'letterDate', label: 'Letter Date' },
    { key: 'incomingId', label: 'Incoming ID' },
    { key: 'incomingNumber', label: 'Incoming Number' },
    { key: 'year', label: 'Year' },
    { key: 'status', label: 'Status' },
    { key: 'department', label: 'Department' },
    { key: 'letterDescription', label: 'Letter Description' },
    { key: 'createdBy', label: 'Created By' },
    { key: 'createdOn', label: 'Created On' },
    { key: 'outgoingId', label: 'Outgoing ID' }
  ];

  outgoingFields = [
    { key: 'serial', label: 'Serial' },
    { key: 'subject', label: 'Subject' },
    { key: 'date', label: 'Date' },
    { key: 'outGoingId', label: 'Outgoing ID' },
    { key: 'outGoingNumber', label: 'Outgoing Number' },
    { key: 'body', label: 'Body' },
    { key: 'year', label: 'Year' },
    { key: 'department', label: 'Department' },
    { key: 'category', label: 'Category' },
    { key: 'incomingId', label: 'Incoming ID' },
    { key: 'createdBy', label: 'Created By' },
    { key: 'createdOn', label: 'Created On' }
  ];

  constructor(
    private incomingService: IncomingService,
    private outgoingService: OutgoingService,
    private notification: NotificationService,
    private router: Router
  ) {}

  ngOnInit(): void {
    const fields = this.fileType === 'Incoming' ? this.incomingFields : this.outgoingFields;
    this.availableFields = fields.map(f => ({ ...f, selected: true }));
  }

  getFields() {
    return this.fileType === 'Incoming' ? this.incomingFields : this.outgoingFields;
  }

  toggleAllFields(event: Event): void {
    const checked = (event.target as HTMLInputElement).checked;
    this.availableFields.forEach(field => field.selected = checked);
  }

  getSelectedFields(): string[] {
    return this.availableFields.filter(f => f.selected).map(f => f.key);
  }

  areFieldsSelected(): boolean {
    return this.availableFields.some(f => f.selected);
  }

  export(): void {
    if (!this.areFieldsSelected()) {
      this.notification.error('Please select at least one field to export');
      return;
    }

    this.exporting = true;
    const service = this.fileType === 'Incoming' ? this.incomingService : this.outgoingService;

    // Build search request from filters
    const searchRequest = this.buildSearchRequest();

    // Call appropriate export method based on format
    const export$ = this.exportFormat === 'Excel'
      ? service.exportToExcel(searchRequest)
      : service.exportToPDF(searchRequest);

    export$.subscribe({
      next: (blob: Blob) => {
        const url = window.URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        const ext = this.exportFormat === 'Excel' ? 'xlsx' : 'pdf';
        a.download = `${this.fileType.toLowerCase()}_export_${new Date().toISOString().split('T')[0]}.${ext}`;
        document.body.appendChild(a);
        a.click();
        document.body.removeChild(a);
        window.URL.revokeObjectURL(url);

        this.notification.success('Export completed successfully');
        this.exporting = false;
      },
      error: (error: any) => {
        console.error('Export failed:', error);
        this.notification.error('Failed to export data');
        this.exporting = false;
      }
    });
  }

  private buildSearchRequest(): any {
    const request: any = {
      pageNumber: 1,
      pageSize: 10000 // Export all records
    };

    // Apply filters
    if (this.filters.startDate) {
      request.startDate = this.filters.startDate;
    }
    if (this.filters.endDate) {
      request.endDate = this.filters.endDate;
    }
    if (this.filters.departmentId) {
      request.departmentId = this.filters.departmentId;
    }
    if (this.filters.year) {
      request.year = this.filters.year;
    }
    if (this.fileType === 'Incoming' && this.filters.status) {
      request.status = this.filters.status;
    }
    if (this.fileType === 'Outgoing' && this.filters.categoryId) {
      request.categoryId = this.filters.categoryId;
    }

    return request;
  }

  cancel(): void {
    this.router.navigate(['/incoming-outgoing', this.fileType === 'Incoming' ? 'incoming' : 'outgoing']);
  }

  clearFilters(): void {
    this.filters = {
      startDate: undefined,
      endDate: undefined,
      departmentId: undefined,
      status: undefined,
      categoryId: undefined,
      year: undefined,
      createdBy: undefined
    };
  }
}
