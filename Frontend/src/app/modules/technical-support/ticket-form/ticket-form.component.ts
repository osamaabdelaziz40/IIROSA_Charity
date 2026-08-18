import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, ActivatedRoute, Router } from '@angular/router';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { TranslateModule, TranslateService } from '@ngx-translate/core';

import { TechnicalSupportService } from '../services/technical-support.service';
import {
  CreateTicketRequest,
  TicketCategory,
  TicketPriority,
  SupportTicket
} from '../../../core/models/technical-support.model';
import { ApiResponse } from '../../../core/models/common.model';
import { PageHeaderComponent } from '../../../shared/components/page-header/page-header.component';
import { BreadcrumbComponent, BreadcrumbItem } from '../../../shared/components';

@Component({
  selector: 'app-ticket-form',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    ReactiveFormsModule,
    TranslateModule,
    PageHeaderComponent,
    BreadcrumbComponent
  ],
  templateUrl: './ticket-form.component.html',
  styleUrls: ['./ticket-form.component.scss']
})
export class TicketFormComponent implements OnInit {
  ticketForm: FormGroup;
  isEditMode: boolean = false;
  ticketId: string | null = null;
  loading: boolean = false;
  submitting: boolean = false;

  // Breadcrumb items
  breadcrumbs: BreadcrumbItem[] = [
    { label: 'common.home', url: '/dashboard' },
    { label: 'technicalSupport.title', url: '/technical-support' }
  ];

  get breadcrumbsWithAction(): BreadcrumbItem[] {
    return [
      ...this.breadcrumbs,
      { label: this.isEditMode ? 'technicalSupport.editTicket' : 'technicalSupport.newTicket' }
    ];
  }

  selectedFile: File | null = null;
  filePreview: string | null = null;

  // Auto-detected information
  browserInfo: string = '';
  pageUrl: string = '';

  // Enum values
  categories = Object.values(TicketCategory);
  priorities = Object.values(TicketPriority);

  // Page actions for header
  pageActions = [
    {
      label: 'common.cancel',
      type: 'secondary',
      icon: 'x',
      click: () => this.onCancel()
    }
  ];

  constructor(
    private fb: FormBuilder,
    private route: ActivatedRoute,
    private router: Router,
    private technicalSupportService: TechnicalSupportService,
    private translate: TranslateService
  ) {
    this.ticketForm = this.createForm();
    this.browserInfo = this.technicalSupportService.detectBrowserInfo();
    this.pageUrl = this.technicalSupportService.getCurrentPageUrl();
  }

  ngOnInit(): void {
    // Check if we're in edit mode
    this.route.paramMap.subscribe(params => {
      const id = params.get('id');
      if (id) {
        this.isEditMode = true;
        this.ticketId = id;
        this.loadTicket(id);
      }
    });

    // Pre-fill system information
    this.ticketForm.patchValue({
      browserInfo: this.browserInfo,
      pageUrl: this.pageUrl
    });
  }

  private createForm(): FormGroup {
    return this.fb.group({
      title: ['', [Validators.required, Validators.minLength(5), Validators.maxLength(200)]],
      category: [TicketCategory.Other, [Validators.required]],
      priority: [TicketPriority.Medium, [Validators.required]],
      message: ['', [Validators.required, Validators.minLength(10)]],
      userAction: [''],
      browserInfo: [''],
      pageUrl: ['']
    });
  }

  loadTicket(id: string): void {
    this.loading = true;
    this.technicalSupportService.getTicketById(id).subscribe({
      next: (ticket: SupportTicket) => {
        this.ticketForm.patchValue({
          title: ticket.title,
          category: ticket.category,
          priority: ticket.priority,
          message: ticket.message,
          userAction: ticket.userAction
        });
        this.loading = false;
      },
      error: () => {
        this.loading = false;
        this.router.navigate(['/technical-support']);
      }
    });
  }

  onSubmit(): void {
    if (this.ticketForm.invalid) {
      this.markFormGroupTouched(this.ticketForm);
      return;
    }

    this.submitting = true;

    const request: CreateTicketRequest = {
      ...this.ticketForm.value,
      attachedFile: this.selectedFile || undefined
    };

    this.technicalSupportService.createTicket(request).subscribe({
      next: (response: ApiResponse<SupportTicket>) => {
        this.submitting = false;
        this.technicalSupportService.notifyTicketsUpdated();
        this.router.navigate(['/technical-support', response.value!.id]);
      },
      error: () => {
        this.submitting = false;
      }
    });
  }

  onCancel(): void {
    if (this.ticketId) {
      this.router.navigate(['/technical-support', this.ticketId]);
    } else {
      this.router.navigate(['/technical-support']);
    }
  }

  onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (input.files && input.files.length > 0) {
      this.handleFileSelection(input.files[0]);
    }
  }

  onFileDropped(event: DragEvent): void {
    event.preventDefault();
    event.stopPropagation();

    if (event.dataTransfer?.files && event.dataTransfer.files.length > 0) {
      this.handleFileSelection(event.dataTransfer.files[0]);
    }
  }

  onDragOver(event: DragEvent): void {
    event.preventDefault();
    event.stopPropagation();
  }

  private handleFileSelection(file: File): void {
    // Validate file size (max 10MB)
    const maxSize = 10 * 1024 * 1024;
    if (file.size > maxSize) {
      alert(this.translate.instant('validation.fileTooLarge'));
      return;
    }

    // Validate file type
    const allowedTypes = ['image/jpeg', 'image/png', 'image/gif', 'application/pdf',
                          'application/msword', 'application/vnd.openxmlformats-officedocument.wordprocessingml.document'];
    if (!allowedTypes.includes(file.type)) {
      alert(this.translate.instant('validation.invalidFileType'));
      return;
    }

    this.selectedFile = file;

    // Create preview for images
    if (file.type.startsWith('image/')) {
      const reader = new FileReader();
      reader.onload = (e) => {
        this.filePreview = e.target?.result as string;
      };
      reader.readAsDataURL(file);
    } else {
      this.filePreview = null;
    }
  }

  removeFile(): void {
    this.selectedFile = null;
    this.filePreview = null;
  }

  getFileName(): string {
    return this.selectedFile?.name || '';
  }

  getFileSize(): string {
    if (!this.selectedFile) return '';
    const bytes = this.selectedFile.size;
    const k = 1024;
    const sizes = ['Bytes', 'KB', 'MB', 'GB'];
    const i = Math.floor(Math.log(bytes) / Math.log(k));
    return Math.round(bytes / Math.pow(k, i) * 100) / 100 + ' ' + sizes[i];
  }

  getFileIcon(): string {
    if (!this.selectedFile) return '';
    const type = this.selectedFile.type;
    if (type.includes('image')) return 'fe fe-image';
    if (type.includes('pdf')) return 'fe fe-file-text';
    if (type.includes('word') || type.includes('document')) return 'fe fe-file';
    return 'fe fe-file';
  }

  // Form validation helpers
  isFieldInvalid(fieldName: string): boolean {
    const field = this.ticketForm.get(fieldName);
    return field ? field.invalid && (field.dirty || field.touched) : false;
  }

  getErrorMessage(fieldName: string): string {
    const field = this.ticketForm.get(fieldName);
    if (!field || !field.errors) return '';

    const errors = field.errors;
    if (errors['required']) {
      return `${this.translate.instant('technicalSupport.' + fieldName)} ${this.translate.instant('validation.required')}`;
    }
    if (errors['minlength']) {
      return `${this.translate.instant('validation.minLength')} ${errors['minlength'].requiredLength}`;
    }
    if (errors['maxlength']) {
      return `${this.translate.instant('validation.maxLength')} ${errors['maxlength'].requiredLength}`;
    }

    return this.translate.instant('validation.invalidFormat');
  }

  private markFormGroupTouched(formGroup: FormGroup): void {
    Object.keys(formGroup.controls).forEach(key => {
      const control = formGroup.get(key);
      control?.markAsTouched();

      if (control instanceof FormGroup) {
        this.markFormGroupTouched(control);
      }
    });
  }

  getCategoryTranslation(category: TicketCategory): string {
    return this.translate.instant(`technicalSupport.categories.${category}`);
  }

  getPriorityTranslation(priority: TicketPriority): string {
    return this.translate.instant(`technicalSupport.priorities.${priority}`);
  }
}
