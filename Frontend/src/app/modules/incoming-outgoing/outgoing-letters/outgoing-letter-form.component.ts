import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { OutgoingService } from '../services/outgoing.service';
import { IncomingService } from '../services/incoming.service';
import { LookupManagementService } from '../../lookup-management/services/lookup-management.service';
import { DepartmentDto } from '../../lookup-management/models/lookup.model';
import { NotificationService } from '../../../core/services/notification.service';
import { PageHeaderComponent } from '../../../shared/components/page-header/page-header.component';
import { BreadcrumbComponent, BreadcrumbItem, AttachmentInputComponent } from '../../../shared/components';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { OutgoingDto, CreateOutgoingDto, UpdateOutgoingDto, OutgoingCategoryOptionDto } from '../models/outgoing.model';
import { IncomingListDto } from '../models/incoming.model';
import { SharedModule, AttachmentFileType } from '../../../shared/shared.module';

@Component({
  selector: 'app-outgoing-letter-form',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    PageHeaderComponent,
    BreadcrumbComponent,
    TranslateModule,
    SharedModule,
    AttachmentInputComponent
  ],
  templateUrl: './outgoing-letter-form.component.html',
  styleUrls: ['./outgoing-letter-form.component.scss']
})
export class OutgoingLetterFormComponent implements OnInit, OnDestroy {
  letterForm: FormGroup;
  isEditMode = false;
  letterId: string | null = null;
  loading = false;
  saving = false;

  // The advisory next serial (UC-COR-12) — read-only; the server allocates the
  // definitive serial per charity + year inside the create transaction.
  nextSerialTxt = '';

  // Lookup data
  allDepartments: DepartmentDto[] = [];
  allIncomingLetters: IncomingListDto[] = [];

  // Categories from the live catalogue (UC-COR-17)
  categoryOptions: Array<{ id: number | null; name: string }> = [];

  // The §21.S.5 selected-letter summary (DisplayIncominData)
  selectedIncoming: IncomingListDto | null = null;

  // Attachments
  attachmentFileId: string | null = null;
  attachmentFileType = AttachmentFileType;

  private destroy$ = new Subject<void>();

  // Page actions for header
  pageActions = [
    {
      label: 'common.cancel',
      type: 'secondary',
      icon: 'x',
      click: () => this.cancel()
    }
  ];

  breadcrumbs: BreadcrumbItem[] = [
    { label: 'common.home', url: '/dashboard' },
    { label: 'incomingOutgoing.outgoingLetters', url: '/incoming-outgoing/outgoing' },
    { label: 'incomingOutgoing.addOutgoingLetter' }
  ];

  constructor(
    private fb: FormBuilder,
    private router: Router,
    private route: ActivatedRoute,
    private outgoingService: OutgoingService,
    private incomingService: IncomingService,
    private lookupService: LookupManagementService,
    private notification: NotificationService,
    private translate: TranslateService
  ) {
    this.letterForm = this.createForm();
  }

  ngOnInit(): void {
    this.updateBreadcrumbs();
    this.loadLookupData();

    if (this.isEditMode && this.letterId) {
      this.loadLetter(this.letterId);
    } else {
      this.loadNextSerial();
    }

    // Review P13: serials are per charity + year — the advisory serial follows the
    // chosen letter date's year (create mode only; edit mode shows the stored serial).
    this.letterForm.get('date')!.valueChanges
      .pipe(takeUntil(this.destroy$))
      .subscribe(value => {
        if (!this.isEditMode && value) {
          this.loadNextSerial(Number(String(value).slice(0, 4)));
        }
      });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  private updateBreadcrumbs(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.isEditMode = true;
      this.letterId = id;
      this.breadcrumbs = [
        { label: 'common.home', url: '/dashboard' },
        { label: 'incomingOutgoing.outgoingLetters', url: '/incoming-outgoing/outgoing' },
        { label: 'incomingOutgoing.editOutgoingLetter' }
      ];
    } else {
      this.breadcrumbs = [
        { label: 'common.home', url: '/dashboard' },
        { label: 'incomingOutgoing.outgoingLetters', url: '/incoming-outgoing/outgoing' },
        { label: 'incomingOutgoing.addOutgoingLetter' }
      ];
    }
  }

  private loadNextSerial(year?: number): void {
    this.outgoingService.getNextSerial(year).subscribe({
      next: result => {
        this.nextSerialTxt = result.serialTxt;
      },
      error: (error: any) => console.error('Error loading next serial:', error)
    });
  }

  private createForm(): FormGroup {
    return this.fb.group({
      // §21.S.5 field contract — the serial is read-only and server-allocated
      departmentId: [null, Validators.required],
      date: ['', Validators.required],
      subject: ['', [Validators.required, Validators.maxLength(500)]],
      outgoingCategoryId: [null, Validators.required],
      // Review P4: §21.S.5's ردا علي خطاب offers the `(+ -)` empty option — the first
      // letter of a thread has nothing to reply to, so the reply target is optional
      // here exactly as the server now treats it (the incoming side stays mandatory).
      incomingId: [null]
    });
  }

  private loadLookupData(): void {
    // Load Departments (UC-COR-08)
    this.lookupService.getDepartments({ page: 1, pageSize: 1000, isActive: true }).subscribe({
      next: (response) => {
        this.allDepartments = response.items || [];
      },
      error: () => console.error('Error loading departments')
    });

    // Load categories from the real catalogue (UC-COR-17)
    this.outgoingService.getAvailableCategories().subscribe({
      next: (categories: OutgoingCategoryOptionDto[]) => {
        this.categoryOptions = categories.map(c => ({ id: c.id, name: c.nameAr || c.nameEn }));
      },
      error: () => console.error('Error loading categories')
    });

    // Load incoming letters (ردا علي خطاب)
    this.incomingService.getIncomingLetters({ pageNumber: 1, pageSize: 1000 }).subscribe({
      next: (response) => {
        this.allIncomingLetters = response.items || [];
        // Review P12: edit mode can patch the form before this register arrives —
        // re-resolve the §21.S.5 summary against the now-complete list.
        this.updateSelectedIncoming(this.letterForm.value.incomingId ?? null);
      },
      error: () => console.error('Error loading incoming letters')
    });
  }

  get departmentOptions(): Array<{ id: number; name: string }> {
    return this.allDepartments.map(d => ({ id: d.id, name: d.nameAr || d.name }));
  }

  get incomingLetterOptions(): Array<{ id: string | null; name: string }> {
    // ردا علي خطاب — a `-` empty option first, then the caller-scoped register
    return [
      { id: null, name: '-' },
      ...this.allIncomingLetters.map(letter => ({
        id: letter.id,
        name: `${letter.serialTxt || ''} - ${letter.subject}`
      }))
    ];
  }

  private loadLetter(id: string): void {
    this.loading = true;
    this.outgoingService.getOutgoingLetter(id).subscribe({
      next: (letter: OutgoingDto) => {
        this.patchForm(letter);
        this.loading = false;
      },
      error: (error: any) => {
        console.error('Error loading letter:', error);
        this.notification.error(this.translate.instant('incomingOutgoing.loadLetterFailed'));
        this.loading = false;
      }
    });
  }

  private patchForm(letter: OutgoingDto): void {
    this.nextSerialTxt = letter.serial != null ? String(letter.serial).padStart(4, '0') : '';
    this.attachmentFileId = letter.uploadedFileId || null;

    this.letterForm.patchValue({
      departmentId: letter.departmentId ?? null,
      date: this.toDateInput(letter.date),
      subject: letter.subject,
      outgoingCategoryId: letter.outgoingCategoryId ?? null,
      incomingId: letter.incomingId ?? null
    });

    this.updateSelectedIncoming(letter.incomingId ?? null);
  }

  /** date inputs take yyyy-MM-dd — server dates are full ISO strings */
  private toDateInput(value?: string): string {
    return value ? value.slice(0, 10) : '';
  }

  // §21.S.5 DisplayIncominData — the selected incoming letter's summary
  onIncomingChange(): void {
    this.updateSelectedIncoming(this.letterForm.value.incomingId ?? null);
  }

  private updateSelectedIncoming(incomingId: string | null): void {
    this.selectedIncoming = this.allIncomingLetters.find(l => l.id === incomingId) || null;
  }

  // Attachment handling
  onAttachmentChange(fileId: string | null): void {
    this.attachmentFileId = fileId;
  }

  onSubmit(): void {
    if (this.letterForm.invalid) {
      this.markFormGroupTouched(this.letterForm);
      this.notification.error(this.translate.instant('incomingOutgoing.fixValidationErrors'));
      return;
    }

    this.saving = true;

    if (this.isEditMode && this.letterId) {
      this.updateLetter();
    } else {
      this.createLetter();
    }
  }

  private buildPayload(): CreateOutgoingDto {
    const formValue = this.letterForm.value;

    return {
      departmentId: formValue.departmentId ?? null,
      date: formValue.date || null,
      subject: formValue.subject,
      outgoingCategoryId: formValue.outgoingCategoryId ?? null,
      incomingId: formValue.incomingId ?? null,
      uploadedFileId: this.attachmentFileId || undefined
    };
  }

  private createLetter(): void {
    this.outgoingService.createOutgoingLetter(this.buildPayload()).subscribe({
      next: () => {
        this.notification.success(this.translate.instant('incomingOutgoing.createSuccess'));
        this.saving = false;
        // §21.U.13 post-condition: back to the register, where the new row (serial
        // included) is visible.
        this.router.navigate(['/incoming-outgoing/outgoing']);
      },
      error: (error: any) => {
        console.error('Error creating letter:', error);
        this.handleSaveError(error, 'incomingOutgoing.createFailed');
      }
    });
  }

  private updateLetter(): void {
    const payload: UpdateOutgoingDto = { ...this.buildPayload(), id: this.letterId! };

    this.outgoingService.updateOutgoingLetter(this.letterId!, payload).subscribe({
      next: () => {
        this.notification.success(this.translate.instant('incomingOutgoing.updateSuccess'));
        this.saving = false;
        this.router.navigate(['/incoming-outgoing/outgoing']);
      },
      error: (error: any) => {
        console.error('Error updating letter:', error);
        this.handleSaveError(error, 'incomingOutgoing.updateFailed');
      }
    });
  }

  /**
   * Review P5: surface the server's field→messages map (FluentValidation via the
   * controller's BadRequest(new { message, errors }) shape; this module's service
   * rewraps it as { message, status, details }). Each named control is flagged
   * through the shared components' `server` error, which they render verbatim.
   */
  private handleSaveError(httpError: any, fallbackKey: string): void {
    this.saving = false;

    const errors = httpError?.details;
    if (errors && typeof errors === 'object') {
      for (const [field, messages] of Object.entries<any>(errors)) {
        // Server keys are PascalCase DTO names ("OutgoingCategoryId") — controls are camelCase
        const controlName = field.charAt(0).toLowerCase() + field.slice(1);
        const control = this.letterForm.get(controlName);
        const message = Array.isArray(messages) ? messages.join(' · ') : String(messages);

        if (control) {
          control.setErrors({ server: message });
          control.markAsTouched();
        }
      }
    }

    this.notification.error(httpError?.message || this.translate.instant(fallbackKey));
  }

  cancel(): void {
    if (this.isEditMode && this.letterId) {
      this.router.navigate(['/incoming-outgoing/outgoing', this.letterId]);
    } else {
      this.router.navigate(['/incoming-outgoing/outgoing']);
    }
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
