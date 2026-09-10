import { Component, OnInit, ViewChildren, QueryList } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, ActivatedRoute, Router } from '@angular/router';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { TranslateModule, TranslateService } from '@ngx-translate/core';

import { TechnicalSupportService } from '../services/technical-support.service';
import {
  CreateTicketRequest,
  UpdateTicketRequest,
  TicketLookups,
  LookupOption,
  SupportTicket
} from '../../../core/models/technical-support.model';
import { PageHeaderComponent } from '../../../shared/components/page-header/page-header.component';
import { BreadcrumbComponent, BreadcrumbItem, CollapsibleCardComponent } from '../../../shared/components';
import { NotificationService } from '../../../core/services/notification.service';

@Component({
  selector: 'app-ticket-form',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    ReactiveFormsModule,
    TranslateModule,
    PageHeaderComponent,
    BreadcrumbComponent,
    CollapsibleCardComponent
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

  // Auto-detected information
  browserInfo: string = '';
  pageUrl: string = '';

  /** Collapsible section cards — expanded on a failed submit to reveal the
   *  red fields hidden inside collapsed cards. */
  @ViewChildren(CollapsibleCardComponent) collapsibleCards?: QueryList<CollapsibleCardComponent>;

  // Loaded ticket (edit mode) — carries the current statusId for the update payload
  loadedTicket: SupportTicket | null = null;

  // Lookup options from GET /api/SupportTickets/lookups
  lookups: TicketLookups | null = null;
  categories: LookupOption[] = [];
  priorities: LookupOption[] = [];

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
    private translate: TranslateService,
    private notification: NotificationService
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

    this.loadLookups();

    // Pre-fill system information
    this.ticketForm.patchValue({
      browserInfo: this.browserInfo,
      pageUrl: this.pageUrl
    });
  }

  private createForm(): FormGroup {
    return this.fb.group({
      title: ['', [Validators.required, Validators.minLength(5), Validators.maxLength(200)]],
      category: [null, [Validators.required]],
      priority: [null, [Validators.required]],
      message: ['', [Validators.required, Validators.minLength(10)]],
      userAction: [''],
      browserInfo: [''],
      pageUrl: ['']
    });
  }

  loadLookups(): void {
    this.technicalSupportService.getTicketLookups().subscribe({
      next: (lookups: TicketLookups) => {
        this.lookups = lookups;
        this.categories = lookups.categories ?? [];
        this.priorities = lookups.priorities ?? [];

        // Default the selects to the first option in create mode once options are known
        if (!this.isEditMode) {
          if (this.categories.length > 0 && !this.ticketForm.get('category')?.value) {
            this.ticketForm.patchValue({ category: this.categories[0].id });
          }
          if (this.priorities.length > 0 && !this.ticketForm.get('priority')?.value) {
            this.ticketForm.patchValue({ priority: this.priorities[0].id });
          }
        }
      }
    });
  }

  loadTicket(id: string): void {
    this.loading = true;
    this.technicalSupportService.getTicketById(id).subscribe({
      next: (ticket: SupportTicket) => {
        this.loadedTicket = ticket;
        this.ticketForm.patchValue({
          title: ticket.title,
          category: ticket.categoryId,
          priority: ticket.priorityId,
          message: ticket.message,
          userAction: ticket.userAction
        });
        this.loading = false;
      },
      error: () => {
        this.loading = false;
        this.notification.error(this.translate.instant('technicalSupport.messages.operationFailed'));
        this.router.navigate(['/technical-support']);
      }
    });
  }

  onSubmit(): void {
    if (this.ticketForm.invalid) {
      this.markFormGroupTouched(this.ticketForm);
      this.collapsibleCards?.forEach(c => c.open());
      return;
    }

    this.submitting = true;

    if (this.isEditMode && this.ticketId) {
      const request: UpdateTicketRequest = {
        id: this.ticketId,
        title: this.ticketForm.value.title,
        message: this.ticketForm.value.message,
        categoryId: Number(this.ticketForm.value.category),
        priorityId: Number(this.ticketForm.value.priority),
        // Status stays managed by the dedicated status/solve actions —
        // omitted from the payload so the backend leaves it unchanged.
        statusId: this.loadedTicket?.statusId ?? undefined
      };

      this.technicalSupportService.updateTicket(this.ticketId, request).subscribe({
        next: () => {
          this.submitting = false;
          this.technicalSupportService.notifyTicketsUpdated();
          this.router.navigate(['/technical-support', this.ticketId]);
        },
        error: () => {
          this.submitting = false;
          this.notification.error(this.translate.instant('technicalSupport.messages.operationFailed'));
        }
      });
    } else {
      const request: CreateTicketRequest = {
        title: this.ticketForm.value.title,
        message: this.ticketForm.value.message,
        categoryId: Number(this.ticketForm.value.category),
        priorityId: Number(this.ticketForm.value.priority),
        userAction: this.ticketForm.value.userAction || undefined,
        browserInfo: this.browserInfo,
        pageUrl: this.ticketForm.value.pageUrl || this.pageUrl
      };

      this.technicalSupportService.createTicket(request).subscribe({
        next: (ticket: SupportTicket) => {
          this.submitting = false;
          this.technicalSupportService.notifyTicketsUpdated();
          this.router.navigate(['/technical-support', ticket.id]);
        },
        error: () => {
          this.submitting = false;
          this.notification.error(this.translate.instant('technicalSupport.messages.operationFailed'));
        }
      });
    }
  }

  onCancel(): void {
    if (this.ticketId) {
      this.router.navigate(['/technical-support', this.ticketId]);
    } else {
      this.router.navigate(['/technical-support']);
    }
  }

  trackByLookupId(index: number, option: LookupOption): number {
    return option.id;
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
}
