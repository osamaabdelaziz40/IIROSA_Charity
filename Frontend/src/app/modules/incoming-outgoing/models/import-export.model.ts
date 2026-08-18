export interface ImportRequest {
  fileType: 'Incoming' | 'Outgoing';
  file: File;
  mappings: ColumnMapping[];
  options: ImportOptions;
}

export interface ColumnMapping {
  sourceColumn: string;
  targetField: string;
  isRequired: boolean;
}

export interface ImportOptions {
  skipDuplicates: boolean;
  updateExisting: boolean;
  validateOnly: boolean;
}

export interface ValidationResult {
  totalRows: number;
  validRows: number;
  invalidRows: number;
  errors: ValidationError[];
  warnings: ValidationWarning[];
}

export interface ValidationError {
  rowNumber: number;
  field: string;
  message: string;
  severity: 'error' | 'warning';
}

export interface ValidationWarning {
  rowNumber: number;
  field: string;
  message: string;
}

export interface ImportResult {
  importId: string;
  totalRows: number;
  successfulRows: number;
  failedRows: number;
  errors: ImportError[];
  status: 'Success' | 'PartialSuccess' | 'Failed';
}

export interface ImportError {
  rowNumber: number;
  message: string;
}

export interface ImportHistoryItem {
  id: string;
  importType: 'Incoming' | 'Outgoing';
  fileName: string;
  importDate: Date;
  importedBy: string;
  totalRows: number;
  successfulRows: number;
  failedRows: number;
  status: 'Success' | 'PartialSuccess' | 'Failed' | 'RolledBack';
  fileSize: number;
}

export interface ExportRequest {
  exportType: 'Incoming' | 'Outgoing';
  filters: ExportFilters;
  fields: string[];
  format: 'Excel' | 'PDF' | 'CSV';
  language: 'Arabic' | 'English' | 'Both';
  includeAttachments: boolean;
  sortBy: string;
  sortOrder: 'Ascending' | 'Descending';
}

export interface ExportFilters {
  startDate?: Date;
  endDate?: Date;
  departmentId?: string;
  status?: string;
  categoryId?: string;
  year?: number;
  createdBy?: string;
}

export interface ExportHistoryItem {
  id: string;
  exportType: 'Incoming' | 'Outgoing';
  exportDate: Date;
  exportedBy: string;
  recordCount: number;
  fileFormat: 'Excel' | 'PDF' | 'CSV';
  fileSize: number;
  filtersApplied: string;
  fieldsExported: number;
}
