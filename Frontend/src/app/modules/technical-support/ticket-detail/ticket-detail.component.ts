import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, ActivatedRoute, Router } from '@angular/router';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule, FormsModule } from '@angular/forms';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { Observable, Subject, takeUntil } from 'rxjs';

import { TechnicalSupportService } from '../services/technical-support.service';
import {
  SupportTicket,
  AddTicketResponseRequest,
  UpdateTicketStatusRequest,
  MarkTicketSolvedRequest,
  TicketStatus
} from '../../../core/models/technical-support.model';
import { AuthService, User } from '../../../core/services/auth.service';
import { BreadcrumbComponent, BreadcrumbItem } from '../../../shared/components';

@Component({
  selector: 'app-ticket-detail',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    FormsModule,
    ReactiveFormsModule,
    TranslateModule,
    BreadcrumbComponent
  ],
  templateUrl: './ticket-detail.component.html',
  styleUrls: ['./ticket-detail.component.scss']
})
export class TicketDetailComponent implements OnInit, OnDestroy {
  private destroy$ = new Subject<void>();

  // Breadcrumb items
  breadcrumbs: BreadcrumbItem[] = [
    { label: 'common.home', url: '/dashboard' },
    { label: 'technicalSupport.title', url: '/technical-support' },
    { label: 'technicalSupport.ticketDetails' }
  ];

  ticket: SupportTicket | null = null;
  loading: boolean = false;
  submitting: boolean = false;
  currentUser: User | null = null;
  isAdmin: boolean = false;
  isOwner: boolean = false;

  // Response form
  responseForm: FormGroup;
  showResponseForm: boolean = false;
  isInternalNote: boolean = false;
  responseFile: File | null = null;

  // Status update
  updatingStatus: boolean = false;
  selectedStatus: TicketStatus | '' = '';

  // Mark as solved
  showSolveForm: boolean = false;
  solveForm: FormGroup;

  // UI helpers
  browserInfo: string = '';
  pageUrl: string = '';

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private fb: FormBuilder,
    private technicalSupportService: TechnicalSupportService,
    private authService: AuthService,
    private translate: TranslateService
  ) {
    this.currentUser = this.authService.getCurrentUser();
    this.isAdmin = this.authService.hasAnyRole(['Admin', 'SuperAdmin']);

    this.responseForm = this.fb.group({
      responseText: ['', [Validators.required, Validators.minLength(5)]]
    });

    this.solveForm = this.fb.group({
      resolutionDescription: ['', [Validators.required, Validators.minLength(10)]],
      solutionSteps: ['']
    });

    this.browserInfo = this.technicalSupportService.detectBrowserInfo();
    this.pageUrl = this.technicalSupportService.getCurrentPageUrl();
  }

  ngOnInit(): void {
    const ticketId = this.route.snapshot.paramMap.get('id');
    if (ticketId) {
      this.loadTicket(ticketId);
    } else {
      this.router.navigate(['/technical-support']);
    }

    // Subscribe to tickets updates
    this.technicalSupportService.ticketsUpdated$
      .pipe(takeUntil(this.destroy$))
      .subscribe(() => {
        const ticketId = this.route.snapshot.paramMap.get('id');
        if (ticketId) {
          this.loadTicket(ticketId);
        }
      });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  loadTicket(id: string): void {
    this.loading = true;
    this.technicalSupportService.getTicketById(id).subscribe({
      next: (ticket: SupportTicket) => {
        this.ticket = ticket;
        this.isOwner = this.currentUser?.id === ticket.userId;
        this.loading = false;
      },
      error: () => {
        this.loading = false;
        this.router.navigate(['/technical-support']);
      }
    });
  }

  // Response Management
  toggleResponseForm(): void {
    this.showResponseForm = !this.showResponseForm;
    if (!this.showResponseForm) {
      this.responseForm.reset();
      this.responseFile = null;
      this.isInternalNote = false;
    }
  }

  onResponseFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (input.files && input.files.length > 0) {
      this.responseFile = input.files[0];
    }
  }

  submitResponse(): void {
    if (this.responseForm.invalid || !this.ticket) {
      return;
    }

    this.submitting = true;

    const request: AddTicketResponseRequest = {
      responseText: this.responseForm.value.responseText,
      attachment: this.responseFile || undefined,
      isInternal: this.isInternalNote
    };

    this.technicalSupportService.addTicketResponse(this.ticket.id, request).subscribe({
      next: () => {
        this.submitting = false;
        this.showResponseForm = false;
        this.responseForm.reset();
        this.responseFile = null;
        this.isInternalNote = false;
        this.technicalSupportService.notifyTicketsUpdated();
      },
      error: () => {
        this.submitting = false;
      }
    });
  }

  // Status Management
  toggleStatusUpdate(): void {
    this.updatingStatus = !this.updatingStatus;
    if (this.updatingStatus && this.ticket) {
      this.selectedStatus = this.ticket.status;
    }
  }

  updateTicketStatus(): void {
    if (!this.ticket || !this.selectedStatus) {
      return;
    }

    const request: UpdateTicketStatusRequest = {
      status: this.selectedStatus as TicketStatus
    };

    this.technicalSupportService.updateTicketStatus(this.ticket.id, request).subscribe({
      next: () => {
        this.updatingStatus = false;
        this.technicalSupportService.notifyTicketsUpdated();
      }
    });
  }

  // Mark as Solved
  toggleSolveForm(): void {
    this.showSolveForm = !this.showSolveForm;
    if (!this.showSolveForm) {
      this.solveForm.reset();
    }
  }

  markAsSolved(): void {
    if (!this.ticket || this.solveForm.invalid) {
      return;
    }

    this.submitting = true;

    const request: MarkTicketSolvedRequest = {
      resolutionDescription: this.solveForm.value.resolutionDescription,
      solutionSteps: this.solveForm.value.solutionSteps
    };

    this.technicalSupportService.markTicketAsSolved(this.ticket.id, request).subscribe({
      next: () => {
        this.submitting = false;
        this.showSolveForm = false;
        this.solveForm.reset();
        this.technicalSupportService.notifyTicketsUpdated();
      },
      error: () => {
        this.submitting = false;
      }
    });
  }

  // Close Ticket
  closeTicket(): void {
    if (!this.ticket) {
      return;
    }

    if (confirm(this.translate.instant('technicalSupport.messages.confirmClose'))) {
      this.technicalSupportService.closeTicket(this.ticket.id).subscribe({
        next: () => {
          this.technicalSupportService.notifyTicketsUpdated();
        }
      });
    }
  }

  // Assignment
  assignToUser(userId: string): void {
    if (!this.ticket) {
      return;
    }

    this.technicalSupportService.assignTicket(this.ticket.id, userId).subscribe({
      next: () => {
        this.technicalSupportService.notifyTicketsUpdated();
      }
    });
  }

  unassignTicket(): void {
    if (!this.ticket) {
      return;
    }

    this.technicalSupportService.unassignTicket(this.ticket.id).subscribe({
      next: () => {
        this.technicalSupportService.notifyTicketsUpdated();
      }
    });
  }

  // File downloads
  downloadAttachment(fileUrl: string, fileName: string): void {
    this.technicalSupportService.downloadTicketAttachment(this.ticket!.id, this.extractAttachmentId(fileUrl))
      .subscribe((blob: Blob) => {
        const url = window.URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = fileName;
        a.click();
        window.URL.revokeObjectURL(url);
      });
  }

  private extractAttachmentId(fileUrl: string): string {
    // Extract attachment ID from URL (implementation depends on your API)
    const parts = fileUrl.split('/');
    return parts[parts.length - 1];
  }

  // Helper methods
  canViewTicket(): boolean {
    if (!this.ticket || !this.currentUser) {
      return false;
    }

    return this.isAdmin || this.ticket.userId === this.currentUser.id;
  }

  canPerformActions(): boolean {
    return this.isAdmin;
  }

  getCategoryTranslation(category: string): string {
    return this.translate.instant(`technicalSupport.categories.${category}`);
  }

  getPriorityTranslation(priority: string): string {
    return this.translate.instant(`technicalSupport.priorities.${priority}`);
  }

  getStatusTranslation(status: TicketStatus): string {
    return this.translate.instant(`technicalSupport.statuses.${status}`);
  }

  getPriorityClass(priority: string): string {
    const priorityLower = priority.toLowerCase();
    if (priorityLower === 'urgent') return 'badge-danger';
    if (priorityLower === 'high') return 'badge-warning';
    if (priorityLower === 'medium') return 'badge-info';
    return 'badge-secondary';
  }

  getStatusClass(status: TicketStatus): string {
    switch (status) {
      case TicketStatus.Open:
        return 'badge-primary';
      case TicketStatus.InProgress:
        return 'badge-info';
      case TicketStatus.Resolved:
        return 'badge-success';
      case TicketStatus.Closed:
        return 'badge-secondary';
      default:
        return 'badge-light';
    }
  }

  getResponseFileName(): string {
    return this.responseFile?.name || '';
  }

  // Form validation
  isResponseFieldInvalid(fieldName: string): boolean {
    const field = this.responseForm.get(fieldName);
    return field ? field.invalid && (field.dirty || field.touched) : false;
  }

  isSolveFieldInvalid(fieldName: string): boolean {
    const field = this.solveForm.get(fieldName);
    return field ? field.invalid && (field.dirty || field.touched) : false;
  }

  getResponseErrorMessage(fieldName: string): string {
    const field = this.responseForm.get(fieldName);
    if (!field || !field.errors) return '';

    if (field.errors['required']) {
      return this.translate.instant('technicalSupport.validation.responseRequired');
    }
    if (field.errors['minlength']) {
      return `${this.translate.instant('validation.minLength')} ${field.errors['minlength'].requiredLength}`;
    }

    return this.translate.instant('validation.invalidFormat');
  }

  getSolveErrorMessage(fieldName: string): string {
    const field = this.solveForm.get(fieldName);
    if (!field || !field.errors) return '';

    if (field.errors['required']) {
      return this.translate.instant('technicalSupport.validation.resolutionRequired');
    }
    if (field.errors['minlength']) {
      return `${this.translate.instant('validation.minLength')} ${field.errors['minlength'].requiredLength}`;
    }

    return this.translate.instant('validation.invalidFormat');
  }
}
