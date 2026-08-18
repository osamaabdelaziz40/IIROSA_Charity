import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { OutgoingService } from '../services/outgoing.service';
import { LookupManagementService } from '../../lookup-management/services/lookup-management.service';
import { DepartmentDto } from '../../lookup-management/models/lookup.model';
import { NotificationService } from '../../../core/services/notification.service';
import { PageHeaderComponent } from '../../../shared/components/page-header/page-header.component';
import { BreadcrumbComponent, BreadcrumbItem, AttachmentInputComponent } from '../../../shared/components';
import { TranslateModule, TranslateService, LangChangeEvent } from '@ngx-translate/core';
import { OutgoingDto, CreateOutgoingDto, UpdateOutgoingDto } from '../models/outgoing.model';
import { SharedModule, AttachmentFileType } from '../../../shared/shared.module';
import { Subscription } from 'rxjs';
import { IncomingService } from '../services/incoming.service';
import { IncomingDto } from '../models/incoming.model';

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
  private langChangeSubscription?: Subscription;
  letterForm: FormGroup;
  isEditMode = false;
  letterId: string | null = null;
  loading = false;
  saving = false;

  // Lookup data
  allDepartments: DepartmentDto[] = [];
  allIncomingLetters: IncomingDto[] = [];
  loadingLookups = false;

  // Dropdown data (transformed for LookupBase compatibility)
  get incomingLetterOptions(): Array<{ id: string; name: string }> {
    return this.allIncomingLetters.map(letter => ({
      id: letter.id,
      name: letter.subject
    }));
  }

  // Attachments
  attachmentFileId: string | null = null;

  // Category options (hardcoded for now - should come from backend)
  categoryOptions: Array<{ id: string; name: string }> = [];

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
    this.initializeCategoryOptions();

    // Subscribe to language changes to update category options
    this.langChangeSubscription = this.translate.onLangChange.subscribe((event: LangChangeEvent) => {
      this.initializeCategoryOptions();
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

  private initializeCategoryOptions(): void {
    this.categoryOptions = [
      { id: 'official', name: this.translate.instant('incomingOutgoing.categoryOfficial') },
      { id: 'internal', name: this.translate.instant('incomingOutgoing.categoryInternal') },
      { id: 'external', name: this.translate.instant('incomingOutgoing.categoryExternal') }
    ];
  }

  private createForm(): FormGroup {
    return this.fb.group({
      // Required fields
      subject: ['', [Validators.required, Validators.maxLength(500)]],
      outGoingId: ['', Validators.required],

      // Optional fields
      date: [''],
      outGoingNumber: [''],
      body: [''],
      year: [''],
      fkDepartmentId: [null],
      outgoingCategoryId: [''],
      incomingId: [null]
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

    // Load Incoming Letters (for Reply To dropdown)
    this.incomingService.getIncomingLetters({
      pageNumber: 1,
      pageSize: 1000
    }).subscribe({
      next: (response) => {
        this.allIncomingLetters = response.items || [];
      },
      error: () => {
        console.error('Error loading incoming letters');
      }
    });
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
    this.letterForm.patchValue({
      subject: letter.subject,
      date: letter.date ? new Date(letter.date) : '',
      outGoingNumber: letter.outGoingNumber,
      outGoingId: letter.outGoingId,
      body: letter.body,
      year: letter.year,
      fkDepartmentId: letter.fkDepartmentId,
      outgoingCategoryId: letter.outgoingCategoryId || '',
      incomingId: letter.incomingId
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

    const letter: CreateOutgoingDto = {
      subject: formValue.subject,
      date: formValue.date || null,
      outGoingNumber: formValue.outGoingNumber || null,
      outGoingId: formValue.outGoingId,
      body: formValue.body || null,
      year: formValue.year || null,
      fkDepartmentId: formValue.fkDepartmentId || null,
      outgoingCategoryId: formValue.outgoingCategoryId || null,
      incomingId: formValue.incomingId || null,
      uploadedFile: this.attachmentFileId || undefined
    };

    this.outgoingService.createOutgoingLetter(letter).subscribe({
      next: (response: OutgoingDto) => {
        this.notification.success(this.translate.instant('incomingOutgoing.createSuccess'));
        this.saving = false;
        this.router.navigate(['/incoming-outgoing/outgoing', response.id]);
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

    const letter: UpdateOutgoingDto = {
      subject: formValue.subject,
      date: formValue.date || null,
      outGoingNumber: formValue.outGoingNumber || null,
      outGoingId: formValue.outGoingId,
      body: formValue.body || null,
      year: formValue.year || null,
      fkDepartmentId: formValue.fkDepartmentId || null,
      outgoingCategoryId: formValue.outgoingCategoryId || null,
      incomingId: formValue.incomingId || null,
      uploadedFile: this.attachmentFileId || undefined
    };

    this.outgoingService.updateOutgoingLetter(this.letterId!, letter).subscribe({
      next: (response: OutgoingDto) => {
        this.notification.success(this.translate.instant('incomingOutgoing.updateSuccess'));
        this.saving = false;
        this.router.navigate(['/incoming-outgoing/outgoing', response.id]);
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
      this.router.navigate(['/incoming-outgoing/outgoing', this.letterId]);
    } else {
      this.router.navigate(['/incoming-outgoing/outgoing']);
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

  onCategoryDropDownChanged(value: any): void {
    if (value && value.id !== undefined && value.id !== null) {
      this.letterForm.patchValue({ outgoingCategoryId: value.id });
    } else {
      this.letterForm.patchValue({ outgoingCategoryId: '' });
    }
  }

  onIncomingDropDownChanged(value: any): void {
    if (value && value.id !== undefined && value.id !== null) {
      this.letterForm.patchValue({ incomingId: value.id });
    } else {
      this.letterForm.patchValue({ incomingId: null });
    }
  }
}
