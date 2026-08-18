import { Component, OnInit, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { IncomingService } from '../services/incoming.service';
import { OutgoingService } from '../services/outgoing.service';
import { NotificationService } from '../../../core/services/notification.service';
import { PageHeaderComponent } from '../../../shared/components/page-header/page-header.component';
import { TranslateModule } from '@ngx-translate/core';
import {
  ImportOptions,
  ValidationResult,
  ImportResult
} from '../models/import-export.model';

interface FileColumn {
  name: string;
  index: number;
  sampleValue: string;
}

interface FieldMapping {
  sourceColumn: string;
  targetField: string;
  isRequired: boolean;
}

@Component({
  selector: 'app-import-wizard',
  standalone: true,
  imports: [CommonModule, FormsModule, PageHeaderComponent, TranslateModule],
  templateUrl: './import-wizard.component.html',
  styleUrls: ['./import-wizard.component.scss']
})
export class ImportWizardComponent implements OnInit {
  @Input() fileType: 'Incoming' | 'Outgoing' = 'Incoming';

  currentStep = 1;
  totalSteps = 5;
  file: File | null = null;
  fileColumns: FileColumn[] = [];
  availableFields: FieldMapping[] = [];
  mappings: { [key: string]: string } = {};
  validationResult: ValidationResult | null = null;
  importResult: ImportResult | null = null;
  uploading = false;
  validating = false;
  importing = false;
  previewData: any[] = [];

  importOptions: ImportOptions = {
    skipDuplicates: true,
    updateExisting: false,
    validateOnly: false
  };

  incomingFields = [
    { key: 'subject', label: 'Subject', required: true },
    { key: 'date', label: 'Date', required: true },
    { key: 'letterNumber', label: 'Letter Number', required: true },
    { key: 'letterDate', label: 'Letter Date', required: true },
    { key: 'incomingId', label: 'Incoming ID', required: true },
    { key: 'body', label: 'Body', required: false },
    { key: 'year', label: 'Year', required: false },
    { key: 'incomingNumber', label: 'Incoming Number', required: false },
    { key: 'department', label: 'Department', required: false },
    { key: 'status', label: 'Status', required: false },
    { key: 'letterDescription', label: 'Letter Description', required: false },
    { key: 'outgoingId', label: 'Outgoing ID', required: false }
  ];

  outgoingFields = [
    { key: 'subject', label: 'Subject', required: true },
    { key: 'date', label: 'Date', required: true },
    { key: 'outGoingId', label: 'Outgoing ID', required: true },
    { key: 'outGoingNumber', label: 'Outgoing Number', required: false },
    { key: 'body', label: 'Body', required: false },
    { key: 'year', label: 'Year', required: false },
    { key: 'department', label: 'Department', required: false },
    { key: 'category', label: 'Category', required: false },
    { key: 'incomingId', label: 'Incoming ID', required: false }
  ];

  constructor(
    private incomingService: IncomingService,
    private outgoingService: OutgoingService,
    private notification: NotificationService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.availableFields = this.fileType === 'Incoming'
      ? this.incomingFields.map(f => ({ sourceColumn: '', targetField: f.key, isRequired: f.required }))
      : this.outgoingFields.map(f => ({ sourceColumn: '', targetField: f.key, isRequired: f.required }));
  }

  getFields() {
    return this.fileType === 'Incoming' ? this.incomingFields : this.outgoingFields;
  }

  onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (input.files && input.files.length > 0) {
      this.file = input.files[0];
      this.parseFileHeaders();
    }
  }

  parseFileHeaders(): void {
    if (!this.file) return;

    const reader = new FileReader();
    reader.onload = (e) => {
      const data = e.target?.result as string;
      const lines = data.split('\n');

      if (lines.length > 0) {
        const headers = lines[0].split(',').map(h => h.trim().replace(/"/g, ''));
        this.fileColumns = headers.map((name, index) => ({
          name,
          index,
          sampleValue: lines[1] ? lines[1].split(',')[index]?.trim().replace(/"/g, '') : ''
        }));

        this.autoMapFields();
      }
    };
    reader.readAsText(this.file);
  }

  autoMapFields(): void {
    this.mappings = {};
    this.fileColumns.forEach(column => {
      const matchedField = this.availableFields.find(field =>
        column.name.toLowerCase().includes(field.targetField.toLowerCase()) ||
        field.targetField.toLowerCase().includes(column.name.toLowerCase())
      );
      if (matchedField) {
        this.mappings[matchedField.targetField] = column.name;
      }
    });
  }

  downloadTemplate(): void {
    const service = this.fileType === 'Incoming' ? this.incomingService : this.outgoingService;
    service.downloadTemplate().subscribe({
      next: (blob: Blob) => {
        const url = window.URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = `${this.fileType.toLowerCase()}_template.xlsx`;
        document.body.appendChild(a);
        a.click();
        document.body.removeChild(a);
        window.URL.revokeObjectURL(url);
        this.notification.success('Template downloaded successfully');
      },
      error: (error: any) => {
        console.error('Download failed:', error);
        this.notification.error('Failed to download template');
      }
    });
  }

  validateFile(): void {
    if (!this.file) {
      this.notification.error('Please select a file first');
      return;
    }

    this.validating = true;
    const service = this.fileType === 'Incoming' ? this.incomingService : this.outgoingService;

    // Call validate endpoint (will be implemented in backend)
    service.importLetters(this.file, { validateOnly: true }).subscribe({
      next: (result: any) => {
        this.validationResult = result;
        this.validating = false;
        this.currentStep = 4;
      },
      error: (error: any) => {
        console.error('Validation failed:', error);
        this.notification.error(`Validation failed: ${error.message || 'Unknown error'}`);
        this.validating = false;
      }
    });
  }

  executeImport(): void {
    if (!this.file) {
      this.notification.error('Please select a file first');
      return;
    }

    this.importing = true;
    const service = this.fileType === 'Incoming' ? this.incomingService : this.outgoingService;

    service.importLetters(this.file, this.importOptions).subscribe({
      next: (result: any) => {
        this.importResult = result;
        this.importing = false;
        this.currentStep = 5;
        this.notification.success(`Import completed: ${result.successfulRows || 0} of ${result.totalRows || 0} records imported`);
      },
      error: (error: any) => {
        console.error('Import failed:', error);
        this.notification.error(`Import failed: ${error.message || 'Unknown error'}`);
        this.importing = false;
      }
    });
  }

  nextStep(): void {
    if (this.currentStep === 2) {
      this.validateFile();
    } else if (this.currentStep < this.totalSteps) {
      this.currentStep++;
    }
  }

  previousStep(): void {
    if (this.currentStep > 1) {
      this.currentStep--;
    }
  }

  cancel(): void {
    this.router.navigate(['/incoming-outgoing', this.fileType === 'Incoming' ? 'incoming' : 'outgoing']);
  }

  finish(): void {
    this.router.navigate(['/incoming-outgoing', this.fileType === 'Incoming' ? 'incoming' : 'outgoing']);
  }

  goToHistory(): void {
    this.router.navigate(['/incoming-outgoing/history']);
  }

  isStepValid(): boolean {
    switch (this.currentStep) {
      case 1:
        return this.file !== null;
      case 2:
        return this.areRequiredFieldsMapped();
      case 3:
        return true;
      case 4:
        return this.validationResult !== null && this.validationResult.validRows > 0;
      default:
        return true;
    }
  }

  areRequiredFieldsMapped(): boolean {
    const requiredFields = this.availableFields.filter(f => f.isRequired);
    return requiredFields.every(field => this.mappings[field.targetField]);
  }

  getUnmappedRequiredFields(): string[] {
    const requiredFields = this.availableFields.filter(f => f.isRequired);
    return requiredFields
      .filter(field => !this.mappings[field.targetField])
      .map(field => field.targetField);
  }

  getStepTitle(): string {
    const titles = [
      'incomingOutgoing.uploadFile',
      'incomingOutgoing.mapFields',
      'incomingOutgoing.importOptions',
      'incomingOutgoing.validateData',
      'incomingOutgoing.importComplete'
    ];
    return titles[this.currentStep - 1];
  }

  getErrorRowNumber(error: any): number {
    return error?.rowNumber || 0;
  }

  getErrorMessage(error: any): string {
    return error?.message || '';
  }
}
