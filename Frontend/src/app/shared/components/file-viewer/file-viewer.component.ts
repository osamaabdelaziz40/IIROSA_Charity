import { Component, Input, Output, EventEmitter, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { DomSanitizer, SafeUrl } from '@angular/platform-browser';
import { ApiService } from '../../../core/services/api.service';

export interface FileViewerOptions {
  width?: string;
  height?: string;
  class?: string;
  showDownload?: boolean;
  lazy?: boolean;
}

@Component({
  selector: 'app-file-viewer',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './file-viewer.component.html',
  styleUrls: ['./file-viewer.component.scss']
})
export class FileViewerComponent implements OnInit, OnDestroy {
  @Input() attachmentId!: string | null;
  @Input() fileName: string | null = null;
  @Input() contentType: string | null = null;
  @Input() alt: string = 'File preview';
  @Input() options: FileViewerOptions = {};

  @Output() loaded = new EventEmitter<void>();
  @Output() error = new EventEmitter<string>();

  loading = false;
  hasError = false;
  errorMessage = '';
  safeUrl: SafeUrl | null = null;

  private readonly imageTypes = ['image/jpeg', 'image/jpg', 'image/png', 'image/gif', 'image/svg+xml', 'image/webp', 'image/bmp', 'image/x-icon'];
  private readonly pdfType = 'application/pdf';
  private objectUrl: string | null = null;

  constructor(
    private sanitizer: DomSanitizer,
    private api: ApiService
  ) {}

  ngOnInit(): void {
    if (this.options.lazy !== false) {
      this.loadFile();
    }
  }

  loadFile(): void {
    if (!this.attachmentId) {
      this.setError('No attachment ID provided');
      return;
    }

    this.loading = true;
    this.hasError = false;

    // If it's an image or PDF, fetch the file content via authenticated API call
    if (this.isImage || this.isPdf) {
      this.api.getBlob(`/api/attachments/${this.attachmentId}/image`).subscribe({
        next: (blob) => {
          // Revoke previous object URL if exists
          if (this.objectUrl) {
            URL.revokeObjectURL(this.objectUrl);
          }
          // Create new object URL from blob
          this.objectUrl = URL.createObjectURL(blob);
          this.safeUrl = this.sanitizer.bypassSecurityTrustResourceUrl(this.objectUrl);
          this.loading = false;
          this.loaded.emit();
        },
        error: (err) => {
          console.error('Error loading file:', err);
          this.setError('Failed to load file');
          this.error.emit('Failed to load file');
        }
      });
    } else {
      this.loading = false;
    }
  }

  get isImage(): boolean {
    if (!this.contentType) return false;
    return this.imageTypes.includes(this.contentType.toLowerCase());
  }

  get isPdf(): boolean {
    if (!this.contentType) return false;
    return this.contentType.toLowerCase() === this.pdfType;
  }

  getFileIcon(): string {
    if (!this.contentType) return 'fe fe-file';

    const type = this.contentType.toLowerCase();
    if (type.includes('pdf')) return 'fe fe-file-text';
    if (type.includes('word') || type.includes('document')) return 'fe fe-file';
    if (type.includes('excel') || type.includes('spreadsheet') || type.includes('csv')) return 'fe fe-file-plus';
    if (type.includes('powerpoint') || type.includes('presentation')) return 'fe fe-film';
    if (type.includes('zip') || type.includes('rar') || type.includes('archive')) return 'fe fe-archive';
    if (type.includes('audio')) return 'fe fe-music';
    if (type.includes('video')) return 'fe fe-video';

    return 'fe fe-file';
  }

  onImageLoad(event: Event): void {
    // Image loaded successfully
  }

  /**
   * UC-SYS-02: download via the authenticated blob path — a plain href cannot
   * carry the Bearer header.
   */
  downloadFile(): void {
    if (!this.attachmentId) {
      return;
    }
    this.api.download(`/api/attachments/${this.attachmentId}/download`).subscribe({
      next: (blob) => {
        const url = URL.createObjectURL(blob);
        const link = document.createElement('a');
        link.href = url;
        link.download = this.fileName || 'attachment';
        document.body.appendChild(link);
        link.click();
        document.body.removeChild(link);
        URL.revokeObjectURL(url);
      },
      error: () => {
        // Surface the failure on the viewer's error state (review P6, 2026-08-26) —
        // same idiom as onImageError below.
        this.setError('Failed to download file');
        this.error.emit('Failed to download file');
      }
    });
  }

  onImageError(event: Event): void {
    this.setError('Failed to load image');
    this.error.emit('Failed to load image');
  }

  private setError(message: string): void {
    this.hasError = true;
    this.errorMessage = message;
    this.loading = false;
  }

  ngOnDestroy(): void {
    // Clean up object URL to prevent memory leaks
    if (this.objectUrl) {
      URL.revokeObjectURL(this.objectUrl);
    }
  }
}
