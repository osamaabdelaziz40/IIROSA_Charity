import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { IncomingService } from '../services/incoming.service';
import { UserManagementService } from '../../user-management/services/user-management.service';
import { LookupManagementService } from '../../lookup-management/services/lookup-management.service';
import { DepartmentDto } from '../../lookup-management/models/lookup.model';
import { NotificationService } from '../../../core/services/notification.service';
import { PageHeaderComponent } from '../../../shared/components/page-header/page-header.component';
import { BreadcrumbComponent, BreadcrumbItem, AttachmentInputComponent } from '../../../shared/components';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { IncomingDto, CreateIncomingDto, UpdateIncomingDto, CorrespondenceStatusOption } from '../models/incoming.model';
import { SharedModule, AttachmentFileType } from '../../../shared/shared.module';
import { User } from '../../../core/models/user.model';

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
  letterForm: FormGroup;
  isEditMode = false;
  letterId: string | null = null;
  loading = false;
  saving = false;

  // Lookup data
  allDepartments: DepartmentDto[] = [];
  allUsers: User[] = [];

  // Dropdown data (transformed for LookupBase compatibility)
  get departmentOptions(): Array<{ id: number; name: string }> {
    return this.allDepartments.map(d => ({ id: d.id, name: d.nameAr || d.name }));
  }

  get userOptions(): Array<{ id: string; name: string }> {
    return this.allUsers.map(u => ({ id: u.id, name: u.fullName || u.userName || u.email }));
  }

  // Attachments
  attachmentFileId: string | null = null;
  attachmentFileType = AttachmentFileType;

  private destroy$ = new Subject<void>();

  // The spec's tri-state — served by the endpoint, never hard-coded
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
    private userManagementService: UserManagementService,
    private lookupService: LookupManagementService,
    private notification: NotificationService,
    private translate: TranslateService
  ) {
    this.letterForm = this.createForm();
  }

  ngOnInit(): void {
    this.updateBreadcrumbs();
    this.loadStatuses();
    this.loadLookupData();

    if (this.isEditMode && this.letterId) {
      this.loadLetter(this.letterId);
    } else {
      this.loadNextSerial();
    }

    // Review P13: serials are per charity + year — the advisory serial follows the
    // chosen registration date's year (create mode only; edit mode shows the stored serial).
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

  private loadStatuses(): void {
    this.incomingService.getAvailableStatuses().subscribe({
      next: (statuses: CorrespondenceStatusOption[]) => {
        this.statusOptions = statuses.map(s => ({ id: s.id, name: s.nameAr }));
      },
      error: (error: any) => console.error('Error loading statuses:', error)
    });
  }

  private loadNextSerial(year?: number): void {
    this.incomingService.getNextSerial(year).subscribe({
      next: result => {
        this.letterForm.get('serial')?.setValue(result.serialTxt);
      },
      error: (error: any) => console.error('Error loading next serial:', error)
    });
  }

  private createForm(): FormGroup {
    return this.fb.group({
      // §21.S.2 field contract — the serial is read-only and server-allocated;
      // create mode shows the advisory next serial (UC-COR-03), edit the stored one.
      serial: [''],
      // Review P3/P5: letter number, letter date and status are mandatory on the
      // server — require them here too so an emptiness never reaches the 400.
      date: ['', Validators.required],
      letterNumber: ['', [Validators.required, Validators.maxLength(100)]],
      letterDate: ['', Validators.required],
      departmentId: [null, Validators.required],
      subject: ['', [Validators.required, Validators.maxLength(500)]],
      status: ['معلق'],
      assignedUserId: [null, Validators.required],
      letterDescription: ['', [Validators.required, Validators.maxLength(4000)]]
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

    // Load users for assignment (الموظف المسؤول) — every user in the database,
    // matching the backend's identity-user validation of assignedUserId.
    this.userManagementService.getUsers({ page: 1, pageSize: 1000, isActive: true }).subscribe({
      next: (response) => {
        this.allUsers = response.items || [];
      },
      error: () => console.error('Error loading users')
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
    this.attachmentFileId = letter.uploadedFileId || null;

    this.letterForm.patchValue({
      serial: letter.serialTxt || '',
      date: this.toDateInput(letter.date),
      letterNumber: letter.letterNumber || '',
      letterDate: this.toDateInput(letter.letterDate),
      departmentId: letter.departmentId ?? null,
      subject: letter.subject,
      status: letter.status || 'معلق',
      assignedUserId: letter.assignedUserId ?? null,
      letterDescription: letter.letterDescription || ''
    });
  }

  /** date inputs take yyyy-MM-dd — server dates are full ISO strings */
  private toDateInput(value?: string): string {
    return value ? value.slice(0, 10) : '';
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

  private buildPayload(): CreateIncomingDto {
    const formValue = this.letterForm.value;

    return {
      date: formValue.date || null,
      letterNumber: formValue.letterNumber || null,
      letterDate: formValue.letterDate || null,
      departmentId: formValue.departmentId ?? null,
      subject: formValue.subject,
      status: formValue.status || 'معلق',
      assignedUserId: formValue.assignedUserId ?? null,
      letterDescription: formValue.letterDescription || null,
      uploadedFileId: this.attachmentFileId || undefined
    };
  }

  private createLetter(): void {
    this.incomingService.createIncomingLetter(this.buildPayload()).subscribe({
      next: () => {
        this.notification.success(this.translate.instant('incomingOutgoing.createSuccess'));
        this.saving = false;
        // §21.U.4 post-condition: back to the register, where the new row (serial
        // included) is visible.
        this.router.navigate(['/incoming-outgoing/incoming']);
      },
      error: (error: any) => {
        console.error('Error creating letter:', error);
        this.handleSaveError(error, 'incomingOutgoing.createFailed');
      }
    });
  }

  private updateLetter(): void {
    const payload: UpdateIncomingDto = { ...this.buildPayload(), id: this.letterId! };

    this.incomingService.updateIncomingLetter(this.letterId!, payload).subscribe({
      next: () => {
        this.notification.success(this.translate.instant('incomingOutgoing.updateSuccess'));
        this.saving = false;
        this.router.navigate(['/incoming-outgoing/incoming']);
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
        // Server keys are PascalCase DTO names ("LetterNumber") — controls are camelCase
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
      this.router.navigate(['/incoming-outgoing/incoming', this.letterId]);
    } else {
      this.router.navigate(['/incoming-outgoing/incoming']);
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
