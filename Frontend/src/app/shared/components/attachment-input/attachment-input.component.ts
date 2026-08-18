import { Component, Input, Output, EventEmitter, forwardRef, OnInit } from '@angular/core';
import { ControlValueAccessor, NG_VALUE_ACCESSOR, FormControl, NG_VALIDATORS, Validator, ValidatorFn } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { AttachmentDto } from '../../models';
import { DomSanitizer, SafeResourceUrl } from '@angular/platform-browser';

export enum AttachmentFileType {
  General = 1,
  Document = 2,
  Image = 3,
  Audio = 4,
  Video = 5,
  PDF = 6,
  Word = 8,
  PowerPoint = 9,
  Excel = 12
}

@Component({
  selector: 'app-attachment-input',
  standalone: true,
  imports: [CommonModule, TranslateModule],
  templateUrl: './attachment-input.component.html',
  styleUrls: ['./attachment-input.component.scss'],
  providers: [
    {
      provide: NG_VALUE_ACCESSOR,
      useExisting: forwardRef(() => AttachmentInputComponent),
      multi: true
    },
    {
      provide: NG_VALIDATORS,
      useExisting: forwardRef(() => AttachmentInputComponent),
      multi: true
    }
  ]
})
export class AttachmentInputComponent implements ControlValueAccessor, Validator, OnInit {
  constructor(
    private sanitizer: DomSanitizer,
    private translate: TranslateService
  ) {}

  @Input() labelKey: string = '';
  @Input() controls: string = '';
  @Input() isRequired: boolean = false;
  @Input() viewOnly: boolean = false;
  @Input() disabled: boolean = false;
  @Input() maxFilesNumber: number = 1;
  @Input() maxSizeMB: number = 5;
  @Input() acceptedFileTypes: AttachmentFileType[] = [AttachmentFileType.Image];
  @Input() descriptionEnable: boolean = false;
  @Input() displayDiscription: boolean = false;
  @Input() languageSelection: boolean = false;
  @Input() attachments: AttachmentDto[] = [];
  @Input() form: any;

  // Additional inputs for compatibility with forms
  @Input() fileType: AttachmentFileType = AttachmentFileType.Document;
  @Input() existingFileId: string | null = null;

  @Output() attachmentsChange = new EventEmitter<AttachmentDto[]>();
  @Output() fileIdChange = new EventEmitter<string | null>();

  value: AttachmentDto[] = [];
  isDragging = false;
  validationError: string = '';

  get activeFilesCount(): number {
    return this.value.filter(a => !a.isDeleted).length;
  }

  get activeAttachments(): AttachmentDto[] {
    return this.value.filter(a => !a.isDeleted);
  }

  private onChange: (value: AttachmentDto[]) => void = () => {};
  private onTouched: () => void = () => {};

  ngOnInit(): void {
    // Map fileType to acceptedFileTypes if not explicitly set
    if (this.acceptedFileTypes.length === 1 && this.acceptedFileTypes[0] === AttachmentFileType.Image) {
      this.acceptedFileTypes = [this.fileType];
    }

    if (this.attachments && this.attachments.length > 0) {
      this.value = [...this.attachments];
    }

    // Handle existingFileId - create a placeholder attachment
    if (this.existingFileId && this.value.length === 0) {
      this.value = [{
        id: this.existingFileId,
        fileName: '',
        contentType: '',
        size: 0,
        extension: '',
        isNew: false,
        isDeleted: false
      }];
    }
  }

  get acceptedFileTypesString(): string {
    const typeMap: Record<AttachmentFileType, string> = {
      [AttachmentFileType.General]: '*',
      [AttachmentFileType.Document]: '.doc,.docx,.pdf,.txt',
      [AttachmentFileType.Image]: 'image/*',
      [AttachmentFileType.Audio]: 'audio/*',
      [AttachmentFileType.Video]: 'video/*',
      [AttachmentFileType.PDF]: '.pdf',
      [AttachmentFileType.Word]: '.doc,.docx',
      [AttachmentFileType.PowerPoint]: '.ppt,.pptx',
      [AttachmentFileType.Excel]: '.xls,.xlsx'
    };

    const types = this.acceptedFileTypes.map(t => typeMap[t]).filter(Boolean);
    return types.join(',');
  }

  writeValue(value: AttachmentDto[]): void {
    this.value = value || [];
    if (this.attachments && this.attachments.length > 0 && (!value || value.length === 0)) {
      this.value = [...this.attachments];
    }
  }

  registerOnChange(fn: (value: AttachmentDto[]) => void): void {
    this.onChange = fn;
  }

  registerOnTouched(fn: () => void): void {
    this.onTouched = fn;
  }

  setDisabledState(isDisabled: boolean): void {
    this.disabled = isDisabled;
  }

  validate(): { [key: string]: any } | null {
    if (this.isRequired && this.isValueInvalid()) {
      return { requiredValidAttachment: true };
    }
    return null;
  }

  private isValueInvalid(): boolean {
    if (!Array.isArray(this.value) || this.value.length === 0) {
      return true;
    }
    return !this.value.some(a => !a.isDeleted);
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

  private async handleFiles(files: File[]): Promise<void> {
    this.validationError = '';
    const maxSizeBytes = this.maxSizeMB * 1024 * 1024;

    // Get current non-deleted count
    let currentCount = this.value.filter(a => !a.isDeleted).length;

    for (const file of files) {
      // Check max files limit (dynamic check as we add files)
      if (this.maxFilesNumber > 0 && currentCount >= this.maxFilesNumber) {
        this.validationError = this.translate.instant('common.maxFilesAllowed', { count: this.maxFilesNumber });
        return;
      }

      // Check file size
      if (file.size > maxSizeBytes) {
        this.validationError = this.translate.instant('common.fileSizeExceeded', {
          fileName: file.name,
          maxSize: this.maxSizeMB
        });
        return;
      }

      // Check file type
      const isAccepted = this.isFileTypeAccepted(file);
      if (!isAccepted) {
        this.validationError = this.translate.instant('common.fileTypeNotAccepted', {
          fileName: file.name
        });
        return;
      }

      // Convert to base64
      const base64 = await this.fileToBase64(file);

      const attachment: AttachmentDto = {
        id: this.generateTempId(),
        fileName: file.name,
        contentType: file.type,
        size: file.size,
        extension: file.name.split('.').pop(),
        fileData: base64,
        isNew: true,
        isDeleted: false
      };

      this.value = [...this.value, attachment];
      currentCount++; // Increment count after adding each file
    }

    this.onTouched();
    this.onChange(this.value);
    this.attachmentsChange.emit(this.value);

    // Emit file ID change for form compatibility
    const activeAttachments = this.value.filter(a => !a.isDeleted);
    this.fileIdChange.emit(activeAttachments.length > 0 ? activeAttachments[0].id : null);
  }

  private isFileTypeAccepted(file: File): boolean {
    if (this.acceptedFileTypes.includes(AttachmentFileType.General)) {
      return true;
    }

    const fileName = file.name.toLowerCase();
    const fileType = file.type;

    if (this.acceptedFileTypes.includes(AttachmentFileType.Image)) {
      return fileType.startsWith('image/');
    }
    if (this.acceptedFileTypes.includes(AttachmentFileType.PDF)) {
      return fileName.endsWith('.pdf') || fileType === 'application/pdf';
    }
    if (this.acceptedFileTypes.includes(AttachmentFileType.Word)) {
      return fileName.endsWith('.doc') || fileName.endsWith('.docx');
    }
    if (this.acceptedFileTypes.includes(AttachmentFileType.Excel)) {
      return fileName.endsWith('.xls') || fileName.endsWith('.xlsx');
    }

    return true;
  }

  private fileToBase64(file: File): Promise<string> {
    return new Promise((resolve, reject) => {
      const reader = new FileReader();
      reader.onload = () => {
        const result = reader.result as string;
        resolve(result);
      };
      reader.onerror = reject;
      reader.readAsDataURL(file);
    });
  }

  removeAttachment(attachment: AttachmentDto): void {
    this.value = this.value.filter(a => a.id !== attachment.id);
    this.onChange(this.value);
    this.attachmentsChange.emit(this.value);

    // Emit file ID change for form compatibility
    const activeAttachments = this.value.filter(a => !a.isDeleted);
    this.fileIdChange.emit(activeAttachments.length > 0 ? activeAttachments[0].id : null);
  }

  previewFile(attachment: AttachmentDto): void {
    if (attachment.fileData) {
      // Open base64 data in new tab
      const win = window.open();
      if (win) {
        win.document.write(
          `<iframe src="${attachment.fileData}" frameborder="0" style="border:0; top:0; left:0; bottom:0; right:0; width:100%; height:100%;" allowfullscreen></iframe>`
        );
        win.document.close();
      }
    } else if (attachment.filePath) {
      // Open file path in new tab
      window.open(attachment.filePath, '_blank');
    }
  }

  private generateTempId(): string {
    return `temp_${Date.now()}_${Math.random().toString(36).substr(2, 9)}`;
  }

  formatFileSize(bytes: number): string {
    if (!bytes) return '';
    const k = 1024;
    const sizes = ['Bytes', 'KB', 'MB', 'GB'];
    const i = Math.floor(Math.log(bytes) / Math.log(k));
    return Math.round(bytes / Math.pow(k, i) * 100) / 100 + ' ' + sizes[i];
  }
}
