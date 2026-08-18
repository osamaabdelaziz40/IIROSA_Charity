import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { IncomingService } from '../services/incoming.service';
import { LookupManagementService } from '../../lookup-management/services/lookup-management.service';
import { DepartmentDto } from '../../lookup-management/models/lookup.model';
import { NotificationService } from '../../../core/services/notification.service';
import { PageHeaderComponent } from '../../../shared/components/page-header/page-header.component';
import { BreadcrumbComponent, BreadcrumbItem, AttachmentInputComponent } from '../../../shared/components';
import { TranslateModule, TranslateService, LangChangeEvent } from '@ngx-translate/core';
import { IncomingDto, CreateIncomingDto, UpdateIncomingDto } from '../models/incoming.model';
import { SharedModule, AttachmentFileType } from '../../../shared/shared.module';
import { Subscription } from 'rxjs';
import { OutgoingService } from '../services/outgoing.service';
import { OutgoingDto } from '../models/outgoing.model';

@Component({
  selector: 'app-incoming-letter-form',
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
  templateUrl: './incoming-letter-form.component.html',
  styleUrls: ['./incoming-letter-form.component.scss']
})
export class IncomingLetterFormComponent implements OnInit, OnDestroy {
  private langChangeSubscription?: Subscription;
  letterForm: FormGroup;
  isEditMode = false;
  letterId: string | null = null;
  loading = false;
  saving = false;

  // Lookup data
  allDepartments: DepartmentDto[] = [];
  allOutgoingLetters: OutgoingDto[] = [];
  loadingLookups = false;

  // Dropdown data (transformed for LookupBase compatibility)
  get outgoingLetterOptions(): Array<{ id: string; name: string }> {
    return this.allOutgoingLetters.map(letter => ({
      id: letter.id,
      name: letter.subject
    }));
  }

  // Attachments
  attachmentFileId: string | null = null;

  // Status options
  statusOptions: Array<{ id: string; name: string }> = [];

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
    { label: 'incomingOutgoing.incomingLetters', url: '/incoming-outgoing/incoming' },
    { label: 'incomingOutgoing.addIncomingLetter' }
  ];

  constructor(
    private fb: FormBuilder,
    private router: Router,
    private route: ActivatedRoute,
    private incomingService: IncomingService,
    private outgoingService: OutgoingService,
    private lookupService: LookupManagementService,
    private notification: NotificationService,
    private translate: TranslateService
  ) {
    this.letterForm = this.createForm();
  }

  ngOnInit(): void {
    this.updateBreadcrumbs();
    this.initializeStatusOptions();

    // Subscribe to language changes to update status options
    this.langChangeSubscription = this.translate.onLangChange.subscribe((event: LangChangeEvent) => {
      this.initializeStatusOptions();
    });

    this.loadLookupData();

    if (this.isEditMode && this.letterId) {
      this.loadLetter(this.letterId);
    }
  }

  ngOnDestroy(): void {
    if (this.langChangeSubscription) {
      this.langChangeSubscription.unsubscribe();
    }
  }

  private updateBreadcrumbs(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.isEditMode = true;
      this.letterId = id;
      this.breadcrumbs = [
        { label: 'common.home', url: '/dashboard' },
        { label: 'incomingOutgoing.incomingLetters', url: '/incoming-outgoing/incoming' },
        { label: 'incomingOutgoing.editIncomingLetter' }
      ];
    } else {
      this.breadcrumbs = [
        { label: 'common.home', url: '/dashboard' },
        { label: 'incomingOutgoing.incomingLetters', url: '/incoming-outgoing/incoming' },
        { label: 'incomingOutgoing.addIncomingLetter' }
      ];
    }
  }

  private initializeStatusOptions(): void {
    this.statusOptions = [
      { id: 'Received', name: this.translate.instant('incomingOutgoing.statusReceived') },
      { id: 'Processing', name: this.translate.instant('incomingOutgoing.statusProcessing') },
      { id: 'Completed', name: this.translate.instant('incomingOutgoing.statusCompleted') },
      { id: 'Closed', name: this.translate.instant('incomingOutgoing.statusClosed') },
      { id: 'Pending', name: this.translate.instant('incomingOutgoing.statusPending') }
    ];
  }

  private createForm(): FormGroup {
    return this.fb.group({
      // Required fields
      subject: ['', [Validators.required, Validators.maxLength(500)]],
      incomingId: ['', Validators.required],
      letterNumber: ['', Validators.required],
      letterDate: ['', Validators.required],

      // Optional fields
      date: [''],
      incomingNumber: [''],
      body: [''],
      year: [''],
      status: ['Received'],
      letterDescription: [''],
      fkDepartmentId: [null],
      outgoingId: [null]
    });
  }

  private loadLookupData(): void {
    this.loadingLookups = true;

    // Load Departments
    this.lookupService.getDepartments({ isActive: true }).subscribe({
      next: (response) => {
        this.allDepartments = response.items || [];
      },
      error: () => {
        this.loadingLookups = false;
      }
    });

    // Load Outgoing Letters (for Reply To dropdown)
    this.outgoingService.getOutgoingLetters({
      pageNumber: 1,
      pageSize: 1000
    }).subscribe({
      next: (response) => {
        this.allOutgoingLetters = response.items || [];
      },
      error: () => {
        console.error('Error loading outgoing letters');
      }
    });
  }

  private loadLetter(id: string): void {
    this.loading = true;
    this.incomingService.getIncomingLetter(id).subscribe({
      next: (letter: IncomingDto) => {
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

  private patchForm(letter: IncomingDto): void {
    this.letterForm.patchValue({
      subject: letter.subject,
      date: letter.date ? new Date(letter.date) : '',
      incomingNumber: letter.incomingNumber,
      incomingId: letter.incomingId,
      body: letter.body,
      letterNumber: letter.letterNumber,
      letterDate: letter.letterDate ? new Date(letter.letterDate) : '',
      year: letter.year,
      status: letter.status || 'Received',
      letterDescription: letter.letterDescription,
      fkDepartmentId: letter.fkDepartmentId,
      outgoingId: letter.outgoingId
    });

    if (letter.uploadedFile) {
      this.attachmentFileId = letter.uploadedFile;
    }
  }

  // Attachment handling
  attachmentFileType = AttachmentFileType;

  onAttachmentChange(fileId: string | null): void {
    this.attachmentFileId = fileId;
    console.log('Attachment changed:', fileId);
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

  private createLetter(): void {
    const formValue = this.letterForm.value;

    const letter: CreateIncomingDto = {
      subject: formValue.subject,
      date: formValue.date || null,
      incomingNumber: formValue.incomingNumber || null,
      incomingId: formValue.incomingId,
      body: formValue.body || null,
      letterNumber: formValue.letterNumber,
      letterDate: formValue.letterDate,
      year: formValue.year || null,
      status: formValue.status || 'Received',
      letterDescription: formValue.letterDescription || null,
      fkDepartmentId: formValue.fkDepartmentId || null,
      outgoingId: formValue.outgoingId || null,
      uploadedFile: this.attachmentFileId || undefined
    };

    this.incomingService.createIncomingLetter(letter).subscribe({
      next: (response: IncomingDto) => {
        this.notification.success(this.translate.instant('incomingOutgoing.createSuccess'));
        this.saving = false;
        this.router.navigate(['/incoming-outgoing/incoming', response.id]);
      },
      error: (error: any) => {
        console.error('Error creating letter:', error);
        this.notification.error(this.translate.instant('incomingOutgoing.createFailed'));
        this.saving = false;
      }
    });
  }

  private updateLetter(): void {
    const formValue = this.letterForm.value;

    const letter: UpdateIncomingDto = {
      subject: formValue.subject,
      date: formValue.date || null,
      incomingNumber: formValue.incomingNumber || null,
      incomingId: formValue.incomingId,
      body: formValue.body || null,
      letterNumber: formValue.letterNumber,
      letterDate: formValue.letterDate,
      year: formValue.year || null,
      status: formValue.status || 'Received',
      letterDescription: formValue.letterDescription || null,
      fkDepartmentId: formValue.fkDepartmentId || null,
      outgoingId: formValue.outgoingId || null,
      uploadedFile: this.attachmentFileId || undefined
    };

    this.incomingService.updateIncomingLetter(this.letterId!, letter).subscribe({
      next: (response: IncomingDto) => {
        this.notification.success(this.translate.instant('incomingOutgoing.updateSuccess'));
        this.saving = false;
        this.router.navigate(['/incoming-outgoing/incoming', response.id]);
      },
      error: (error: any) => {
        console.error('Error updating letter:', error);
        this.notification.error(this.translate.instant('incomingOutgoing.updateFailed'));
        this.saving = false;
      }
    });
  }

  cancel(): void {
    if (this.isEditMode && this.letterId) {
      this.router.navigate(['/incoming-outgoing/incoming', this.letterId]);
    } else {
      this.router.navigate(['/incoming-outgoing/incoming']);
    }
  }

  isFieldValid(fieldName: string): boolean {
    const field = this.letterForm.get(fieldName);
    return field ? field.valid && (field.dirty || field.touched) : false;
  }

  isFieldInvalid(fieldName: string): boolean {
    const field = this.letterForm.get(fieldName);
    return field ? field.invalid && (field.dirty || field.touched) : false;
  }

  getErrorMessage(fieldName: string): string {
    const field = this.letterForm.get(fieldName);
    if (!field || !field.errors) return '';

    const fieldLabel = this.translate.instant(`incomingOutgoing.${fieldName}`);

    if (field.errors['required']) {
      return this.translate.instant('validation.requiredField', { field: fieldLabel });
    }
    if (field.errors['maxlength']) {
      const maxLength = field.errors['maxlength'].requiredLength;
      return this.translate.instant('validation.maxLength', { maxLength });
    }

    return this.translate.instant('validation.invalid');
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

  // Dropdown change handlers
  onDepartmentDropDownChanged(value: any): void {
    if (value && value.id !== undefined && value.id !== null) {
      this.letterForm.patchValue({ fkDepartmentId: value.id });
    } else {
      this.letterForm.patchValue({ fkDepartmentId: null });
    }
  }

  onOutgoingDropDownChanged(value: any): void {
    if (value && value.id !== undefined && value.id !== null) {
      this.letterForm.patchValue({ outgoingId: value.id });
    } else {
      this.letterForm.patchValue({ outgoingId: null });
    }
  }

  onStatusDropDownChanged(value: any): void {
    if (value && value.id !== undefined && value.id !== null) {
      this.letterForm.patchValue({ status: value.id });
    } else {
      this.letterForm.patchValue({ status: 'Received' });
    }
  }
}
