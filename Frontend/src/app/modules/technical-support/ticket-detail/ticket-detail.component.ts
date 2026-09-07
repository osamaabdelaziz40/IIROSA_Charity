import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, ActivatedRoute, Router } from '@angular/router';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule, FormsModule } from '@angular/forms';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { Subject, takeUntil } from 'rxjs';

import { TechnicalSupportService } from '../services/technical-support.service';
import {
  SupportTicket,
  AddTicketResponseRequest,
  MarkTicketSolvedRequest,
  TicketResponse,
  LookupOption
} from '../../../core/models/technical-support.model';
import { AuthService, User } from '../../../core/services/auth.service';
import { NotificationService } from '../../../core/services/notification.service';
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

  // Status update
  updatingStatus: boolean = false;
  selectedStatusId: number | null = null;

  // Status lookups (GET /api/SupportTickets/lookups)
  statuses: LookupOption[] = [];
  closedStatusId: number | null = null;

  // Mark as solved
  showSolveForm: boolean = false;
  solveForm: FormGroup;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private fb: FormBuilder,
    private technicalSupportService: TechnicalSupportService,
    private authService: AuthService,
    private translate: TranslateService,
    private notification: NotificationService
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
  }

  ngOnInit(): void {
    const ticketId = this.route.snapshot.paramMap.get('id');
    if (ticketId) {
      this.loadTicket(ticketId);
    } else {
      this.router.navigate(['/technical-support']);
    }

    this.loadLookups();

    // Subscribe to tickets updates
    this.technicalSupportService.ticketsUpdated$
      .pipe(takeUntil(this.destroy$))
      .subscribe(() => {
        const currentId = this.route.snapshot.paramMap.get('id');
        if (currentId) {
          this.loadTicket(currentId);
        }
      });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  loadLookups(): void {
    this.technicalSupportService.getTicketLookups().subscribe({
      next: (lookups) => {
        this.statuses = lookups.statuses ?? [];
        this.closedStatusId =
          this.statuses.find(s => s.nameEn === 'Closed' || s.name === 'Closed')?.id ?? null;
      }
    });
  }

  loadTicket(id: string): void {
    this.loading = true;
    this.technicalSupportService.getTicketById(id).subscribe({
      next: (ticket: SupportTicket) => {
        this.ticket = ticket;
        this.isOwner = !!this.currentUser && this.currentUser.id === ticket.createdByUserId;
        this.loading = false;
      },
      error: () => {
        this.loading = false;
        this.notification.error(this.translate.instant('technicalSupport.messages.operationFailed'));
        this.router.navigate(['/technical-support']);
      }
    });
  }

  /**
   * Admins see all responses (public + internal notes); regular users
   * only see the public responses the API returns for them.
   */
  get visibleResponses(): TicketResponse[] {
    if (!this.ticket) {
      return [];
    }
    if (this.isAdmin) {
      return this.ticket.responses ?? [];
    }
    return this.ticket.publicResponses ?? [];
  }

  // Response Management
  toggleResponseForm(): void {
    this.showResponseForm = !this.showResponseForm;
    if (!this.showResponseForm) {
      this.responseForm.reset();
      this.isInternalNote = false;
    }
  }

  submitResponse(): void {
    if (this.responseForm.invalid || !this.ticket) {
      return;
    }

    this.submitting = true;

    const request: AddTicketResponseRequest = {
      responseText: this.responseForm.value.responseText,
      isInternalNote: this.isInternalNote
    };

    this.technicalSupportService.addTicketResponse(this.ticket.id, request).subscribe({
      next: () => {
        this.submitting = false;
        this.showResponseForm = false;
        this.responseForm.reset();
        this.isInternalNote = false;
        this.technicalSupportService.notifyTicketsUpdated();
      },
      error: () => {
        this.submitting = false;
        this.notification.error(this.translate.instant('technicalSupport.messages.operationFailed'));
      }
    });
  }

  // Status Management
  toggleStatusUpdate(): void {
    this.updatingStatus = !this.updatingStatus;
    if (this.updatingStatus && this.ticket) {
      this.selectedStatusId = this.ticket.statusId;
    }
  }

  updateTicketStatus(): void {
    if (!this.ticket || this.selectedStatusId === null) {
      return;
    }

    this.technicalSupportService
      .updateTicketStatus(this.ticket.id, { statusId: this.selectedStatusId })
      .subscribe({
        next: () => {
          this.updatingStatus = false;
          this.technicalSupportService.notifyTicketsUpdated();
        },
        error: () => {
          this.notification.error(this.translate.instant('technicalSupport.messages.operationFailed'));
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
        this.notification.error(this.translate.instant('technicalSupport.messages.operationFailed'));
      }
    });
  }

  // Close Ticket — sets the Closed status via the status endpoint
  closeTicket(): void {
    if (!this.ticket || this.closedStatusId === null) {
      // Lookups not loaded — closing is impossible; say so instead of silently doing nothing
      this.notification.error(this.translate.instant('technicalSupport.messages.operationFailed'));
      return;
    }

    if (confirm(this.translate.instant('technicalSupport.messages.confirmClose'))) {
      this.technicalSupportService
        .updateTicketStatus(this.ticket.id, { statusId: this.closedStatusId })
        .subscribe({
          next: () => {
            this.technicalSupportService.notifyTicketsUpdated();
          },
          error: () => {
            this.notification.error(this.translate.instant('technicalSupport.messages.operationFailed'));
          }
        });
    }
  }

  // Helper methods
  canViewTicket(): boolean {
    if (!this.ticket || !this.currentUser) {
      return false;
    }

    return this.isAdmin || this.ticket.createdByUserId === this.currentUser.id;
  }

  canPerformActions(): boolean {
    return this.isAdmin;
  }

  shortId(id: string): string {
    return id ? id.substring(0, 8).toUpperCase() : '';
  }

  trackByResponseId(index: number, response: TicketResponse): string {
    return response.id;
  }

  trackByLookupId(index: number, option: LookupOption): number {
    return option.id;
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
