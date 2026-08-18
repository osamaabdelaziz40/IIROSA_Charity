import { Component, Input, Output, EventEmitter, forwardRef } from '@angular/core';
import { ControlValueAccessor, NG_VALUE_ACCESSOR } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { TranslateModule } from '@ngx-translate/core';

export interface AttachedFile {
  id: string;
  name: string;
  size: number;
  type: string;
  url?: string;
  file?: File;
}

@Component({
  selector: 'app-attachment',
  standalone: true,
  imports: [CommonModule, TranslateModule],
  template: `
    <div class="attachment-container" [class.dragging]="isDragging" [class.disabled]="disabled">
      <!-- Drop Zone -->
      <div class="drop-zone" [class.drag-over]="isDragging"
           *ngIf="(selectedFiles?.length ?? 0) < maxFiles"
           (dragover)="onDragOver($event)"
           (dragleave)="onDragLeave($event)"
           (drop)="onDrop($event)">
        <div class="drop-zone-content">
          <i class="fe fe-upload-cloud fe-3x mb-3"></i>
          <p class="mb-2">{{ 'common.dragDrop' | translate }}</p>
          <p class="text-muted mb-3">{{ 'common.or' | translate }}</p>
          <label class="btn btn-primary">
            <i class="fe fe-folder mr-1"></i>
            {{ 'common.browse' | translate }}
            <input type="file" class="d-none" [accept]="accept" [multiple]="multiple" [disabled]="disabled" (change)="onFileSelected($event)">
          </label>
        </div>
      </div>

      <!-- Selected Files List -->
      <div class="files-list" *ngIf="(selectedFiles?.length ?? 0) > 0">
        <div class="file-item" *ngFor="let file of selectedFiles">
          <div class="file-icon">
            <i [class]="getFileIcon(file)"></i>
          </div>
          <div class="file-info">
            <div class="file-name" [title]="file.name">{{ file.name }}</div>
            <div class="file-meta">{{ formatFileSize(file.size) }}</div>
          </div>
          <div class="file-actions">
            <button type="button" class="btn btn-sm btn-icon btn-danger" [disabled]="disabled" (click)="removeFile(file)">
              <i class="fe fe-trash"></i>
            </button>
          </div>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .attachment-container {
      border: 2px dashed var(--border-color);
      border-radius: 0.5rem;
      padding: 1rem;
      transition: all 0.3s ease;
    }
    .attachment-container.dragging {
      border-color: var(--primary);
      background-color: rgba(var(--primary-rgb), 0.05);
    }
    .drop-zone {
      text-align: center;
      padding: 2rem;
    }
    .files-list {
      margin-top: 1rem;
    }
    .file-item {
      display: flex;
      align-items: center;
      padding: 0.75rem;
      border: 1px solid var(--border-color);
      border-radius: 0.375rem;
      margin-bottom: 0.5rem;
    }
    .file-icon {
      width: 40px;
      height: 40px;
      display: flex;
      align-items: center;
      justify-content: center;
      background-color: rgba(var(--primary-rgb), 0.1);
      border-radius: 0.375rem;
      margin-right: 0.75rem;
    }
    .file-info {
      flex: 1;
    }
    .file-name {
      font-weight: 500;
    }
    .file-meta {
      font-size: 0.875rem;
      color: var(--text-muted);
    }
  `],
  providers: [
    {
      provide: NG_VALUE_ACCESSOR,
      useExisting: forwardRef(() => AttachmentComponent),
      multi: true
    }
  ]
})
export class AttachmentComponent implements ControlValueAccessor {
  @Input() maxFiles: number = 5;
  @Input() maxFileSize: number = 10 * 1024 * 1024;
  @Input() allowedTypes: string[] = [];
  @Input() accept: string = '*';
  @Input() multiple: boolean = true;
  @Input() disabled: boolean = false;

  @Output() onFileSelect = new EventEmitter<File[]>();
  @Output() onFileRemove = new EventEmitter<AttachedFile>();

  selectedFiles: AttachedFile[] = [];
  isDragging: boolean = false;

  private onChange: (value: AttachedFile[]) => void = () => {};
  private onTouched: () => void = () => {};

  writeValue(value: AttachedFile[]): void {
    this.selectedFiles = value || [];
  }

  registerOnChange(fn: (value: AttachedFile[]) => void): void {
    this.onChange = fn;
  }

  registerOnTouched(fn: () => void): void {
    this.onTouched = fn;
  }

  setDisabledState(isDisabled: boolean): void {
    this.disabled = isDisabled;
  }

  onDragOver(event: DragEvent): void {
    event.preventDefault();
    if (!this.disabled) {
      this.isDragging = true;
    }
  }

  onDragLeave(event: DragEvent): void {
    event.preventDefault();
    this.isDragging = false;
  }

  onDrop(event: DragEvent): void {
    event.preventDefault();
    this.isDragging = false;
    if (this.disabled) return;

    const files = event.dataTransfer?.files;
    if (files) {
      this.handleFiles(Array.from(files));
    }
  }

  onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (input.files) {
      this.handleFiles(Array.from(input.files));
    }
  }

  private handleFiles(files: File[]): void {
    if (this.selectedFiles.length + files.length > this.maxFiles) {
      alert(`Maximum ${this.maxFiles} files allowed`);
      return;
    }

    files.forEach(file => {
      if (file.size > this.maxFileSize) {
        alert(`File ${file.name} exceeds maximum size`);
        return;
      }

      const attachment: AttachedFile = {
        id: `${Date.now()}-${Math.random().toString(36).substr(2, 9)}`,
        name: file.name,
        size: file.size,
        type: file.type,
        file: file,
        url: URL.createObjectURL(file)
      };

      this.selectedFiles.push(attachment);
    });

    this.onTouched();
    this.onChange(this.selectedFiles);
    this.onFileSelect.emit(files);
  }

  removeFile(file: AttachedFile): void {
    const index = this.selectedFiles.findIndex(f => f.id === file.id);
    if (index > -1) {
      this.selectedFiles.splice(index, 1);
      if (file.url) {
        URL.revokeObjectURL(file.url);
      }
      this.onTouched();
      this.onChange(this.selectedFiles);
      this.onFileRemove.emit(file);
    }
  }

  getFileIcon(file: AttachedFile): string {
    const type = file.type.toLowerCase();
    if (type.includes('image')) return 'fe fe-image';
    if (type.includes('pdf')) return 'fe fe-file-text';
    if (type.includes('word') || type.includes('document')) return 'fe fe-file';
    if (type.includes('excel') || type.includes('spreadsheet')) return 'fe fe-file-plus';
    return 'fe fe-file';
  }

  formatFileSize(bytes: number): string {
    if (bytes === 0) return '0 Bytes';
    const k = 1024;
    const sizes = ['Bytes', 'KB', 'MB', 'GB'];
    const i = Math.floor(Math.log(bytes) / Math.log(k));
    return Math.round(bytes / Math.pow(k, i) * 100) / 100 + ' ' + sizes[i];
  }

  ngOnDestroy(): void {
    this.selectedFiles.forEach(file => {
      if (file.url) {
        URL.revokeObjectURL(file.url);
      }
    });
  }
}
