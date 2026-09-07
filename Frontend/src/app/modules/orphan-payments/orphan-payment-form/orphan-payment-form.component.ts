import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators, AbstractControl, ValidationErrors } from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { Subject, takeUntil } from 'rxjs';
import {
  OrphanPaymentDto,
  CreateOrphanPaymentDto,
  UpdateOrphanPaymentDto,
  CURRENCY_OPTIONS,
  BatchNumberOptionDto
} from '../models/orphan-payment.model';
import { OrphanPaymentService } from '../services/orphan-payment.service';
import { AuthService } from '../../../core/services/auth.service';
import { CharityService } from '../../charities/services/charity.service';
// 10-2 trim: LookupManagementService import removed with the dead "Filtering Options"
// card — its values were never persisted or echoed back by the server.
// 10-6 re-introduces CharityService for the HQ batch-picker filter only (UC-PAY-06).
import { PageHeaderComponent } from '../../../shared/components/page-header/page-header.component';
import { BreadcrumbComponent, BreadcrumbItem } from '../../../shared/components';
import { SharedModule } from '../../../shared/shared.module';
import type { PageAction } from '../../../shared/components/page-header/page-header.component';

/**
 * Sentinel id of the "كل الجمعيات" entry in the HQ charity filter. Select2 single-selects
 * cannot be cleared once a choice is made, so an explicit leading option is the only way
 * back to the unfiltered batch list.
 */
const CHARITY_FILTER_ALL = 'all';

/** Shared-dropdown option — label is the display text, value is the id (charity Guid or the sentinel) */
interface DropdownOption {
  id: any;
  name: string;
}

@Component({
  selector: 'app-orphan-payment-form',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, TranslateModule, SharedModule, PageHeaderComponent, BreadcrumbComponent],
  templateUrl: './orphan-payment-form.component.html',
  styleUrls: ['./orphan-payment-form.component.scss']
})
export class OrphanPaymentFormComponent implements OnInit, OnDestroy {
  private destroy$ = new Subject<void>();

  // Breadcrumb items
  breadcrumbs: BreadcrumbItem[] = [
    { label: 'common.home', url: '/dashboard' },
    { label: 'orphanPayments.title', url: '/orphan-payments' }
  ];

  get breadcrumbsWithCurrent(): BreadcrumbItem[] {
    return [
      ...this.breadcrumbs,
      { label: this.isEditMode ? 'orphanPayments.editGroup' : 'orphanPayments.addNewGroup' }
    ];
  }

  // Page actions for header
  pageActions: PageAction[] = [
    {
      label: 'common.cancel',
      type: 'secondary',
      icon: 'x',
      click: () => this.onCancel()
    }
  ];

  // Form
  form: FormGroup;
  isEditMode = false;
  paymentGroupId: string | null = null;
  loading = false;
  saving = false;

  // Data
  paymentGroup: OrphanPaymentDto | null = null;

  // Options — Select2 wants {id, name}; CURRENCY_OPTIONS is {value, label}
  readonly currencyDropdownOptions = CURRENCY_OPTIONS.map(c => ({ id: c.value, name: c.label }));

  // 10-6 (UC-PAY-06): رقم الدفعة selector — fed by /batch-numbers in the caller's scope.
  // HQ users get the الجمعية filter that narrows the picker server-side; a charity user's
  // scope is pinned by the JWT claim and shows no selector.
  batchOptions: BatchNumberOptionDto[] = [];
  isHqUser = false;

  // The HQ-only charity filter lives in its own group: it drives the batch picker,
  // never the saved payload. The shared dropdown requires a FormGroup-bound control.
  filterForm: FormGroup;
  filterCharityOptions: DropdownOption[] = [];
  selectedCharityId: string | null = null;

  // UI State
  showBatchNumberInput = false;
  // 10-2: server-side field errors from the 400 payload (§15.S.2 field→messages map)
  serverErrors: { [controlName: string]: string } = {};
  // 10-4: general refusal message (e.g. locked exchange rate) — localised when known
  serverMessage: string | null = null;

  constructor(
    private fb: FormBuilder,
    private router: Router,
    private route: ActivatedRoute,
    private orphanPaymentService: OrphanPaymentService,
    private translate: TranslateService,
    private authService: AuthService,
    private charityService: CharityService
  ) {
    this.form = this.buildForm();
    this.filterForm = this.fb.group({ charityId: [CHARITY_FILTER_ALL] });
  }

  ngOnInit(): void {
    this.checkEditMode();

    this.isHqUser = this.authService.hasAnyRole(['SuperAdmin', 'Admin', 'Accountant', 'FinancialOfficer']);
    this.loadBatchOptions();
    if (this.isHqUser) {
      this.loadFilterCharities();
    }

    if (this.isEditMode && this.paymentGroupId) {
      this.loadPaymentGroup();
    }
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  private buildForm(): FormGroup {
    const today = new Date().toISOString().split('T')[0];

    return this.fb.group({
      // Basic Information
      groupName: ['', [Validators.required, Validators.maxLength(200)]],
      description: ['', [Validators.maxLength(1000)]],
      paymentPeriodFrom: [today, [Validators.required]],
      paymentPeriodTo: [today, [Validators.required]],
      groupDate: [today, [Validators.required]],
      // تاريخ بدء التوزيع — §15.S.2 mandatory (10-2)
      paymentDate: [today, [Validators.required]],

      // Financial Information
      exchangeRate: [null, [Validators.min(0), Validators.max(1000)]],
      currency: ['EGP'],
      dontRemoveRate: [false],

      // Settings
      batchNo: [''],
      showOrder: [0],
      notes: ['', [Validators.maxLength(2000)]]
    }, {
      validators: [this.dateRangeValidator]
    });
  }

  // ==================== CUSTOM VALIDATORS ====================

  private dateRangeValidator(control: AbstractControl): ValidationErrors | null {
    const fromDate = control.get('paymentPeriodFrom')?.value;
    const toDate = control.get('paymentPeriodTo')?.value;

    if (fromDate && toDate && new Date(fromDate) > new Date(toDate)) {
      return { dateRangeInvalid: true };
    }

    return null;
  }

  // ==================== INITIALIZATION ====================

  private checkEditMode(): void {
    this.paymentGroupId = this.route.snapshot.paramMap.get('id');
    this.isEditMode = !!this.paymentGroupId;
  }

  private loadPaymentGroup(): void {
    if (!this.paymentGroupId) return;

    this.loading = true;
    this.orphanPaymentService.getOrphanPayment(this.paymentGroupId).subscribe({
      next: (data) => {
        this.paymentGroup = data;
        this.patchForm(data);
        this.loading = false;
      },
      error: () => {
        this.loading = false;
        this.router.navigate(['/orphan-payments']);
      }
    });
  }

  private patchForm(data: OrphanPaymentDto): void {
    this.form.patchValue({
      groupName: data.groupName,
      description: data.description,
      paymentPeriodFrom: data.paymentPeriodFrom.split('T')[0],
      paymentPeriodTo: data.paymentPeriodTo.split('T')[0],
      groupDate: data.groupDate.split('T')[0],
      paymentDate: data.paymentDate ? data.paymentDate.split('T')[0] : data.groupDate.split('T')[0],
      exchangeRate: data.exchangeRate,
      currency: data.currency,
      dontRemoveRate: data.dontRemoveRate,
      batchNo: data.batchNo,
      showOrder: data.showOrder,
      notes: data.notes
    });

    // Disable exchange rate if locked
    if (data.dontRemoveRate) {
      this.form.get('exchangeRate')?.disable();
      this.form.get('currency')?.disable();
    }

    this.ensureCurrentBatchListed();
  }

  // ==================== FORM ACTIONS ====================

  onSubmit(): void {
    this.serverMessage = null;
    if (this.form.invalid) {
      this.markFormGroupTouched(this.form);
      return;
    }

    this.saving = true;

    if (this.isEditMode && this.paymentGroupId) {
      this.updatePaymentGroup();
    } else {
      this.createPaymentGroup();
    }
  }

  private createPaymentGroup(): void {
    const formValue = this.form.value;
    const dto: CreateOrphanPaymentDto = {
      groupName: formValue.groupName,
      description: formValue.description,
      paymentPeriodFrom: formValue.paymentPeriodFrom,
      paymentPeriodTo: formValue.paymentPeriodTo,
      groupDate: formValue.groupDate,
      paymentDate: formValue.paymentDate,
      exchangeRate: formValue.exchangeRate,
      currency: formValue.currency,
      dontRemoveRate: formValue.dontRemoveRate,
      batchNo: formValue.batchNo || undefined,
      showOrder: formValue.showOrder,
      notes: formValue.notes
    };

    this.orphanPaymentService.createOrphanPayment(dto)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (result) => {
          this.saving = false;
          // Navigate to add orphans page
          this.router.navigate(['/orphan-payments', result.id, 'add-orphans']);
        },
        error: (err) => {
          this.saving = false;
          this.serverMessage = this.resolveServerMessage(err);
          this.applyServerErrors(err?.details);
        }
      });
  }

  private updatePaymentGroup(): void {
    // 10-4: raw value — a locked batch disables rate/currency controls, and disabled
    // controls drop out of form.value (the server would then read the omission as a
    // rate change and refuse the whole save).
    const formValue = this.form.getRawValue();
    const dto: UpdateOrphanPaymentDto = {
      groupName: formValue.groupName,
      description: formValue.description,
      paymentPeriodFrom: formValue.paymentPeriodFrom,
      paymentPeriodTo: formValue.paymentPeriodTo,
      groupDate: formValue.groupDate,
      paymentDate: formValue.paymentDate,
      exchangeRate: formValue.exchangeRate,
      currency: formValue.currency,
      dontRemoveRate: formValue.dontRemoveRate,
      batchNo: formValue.batchNo,
      showOrder: formValue.showOrder,
      notes: formValue.notes
    };

    this.orphanPaymentService.updateOrphanPayment(this.paymentGroupId!, dto)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: () => {
          this.saving = false;
          this.router.navigate(['/orphan-payments', this.paymentGroupId]);
        },
        error: (err) => {
          this.saving = false;
          this.serverMessage = this.resolveServerMessage(err);
          this.applyServerErrors(err?.details);
        }
      });
  }

  onCancel(): void {
    if (this.isEditMode && this.paymentGroupId) {
      this.router.navigate(['/orphan-payments', this.paymentGroupId]);
    } else {
      this.router.navigate(['/orphan-payments']);
    }
  }

  // ==================== BATCH NUMBER ====================
  // 10-2: generateNextBatchNumber() removed — GET /batch-number/next never existed
  // server-side; the server auto-generates BatchNo on create when the field is blank.
  // 10-6 (UC-PAY-06): the field is a selector fed by /batch-numbers (distinct batch numbers
  // in the caller's scope); manual entry stays available via the toggle.

  toggleBatchNumberInput(): void {
    this.showBatchNumberInput = !this.showBatchNumberInput;
  }

  private loadBatchOptions(): void {
    this.orphanPaymentService.getBatchNumbers(this.selectedCharityId ?? undefined)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: options => {
          this.batchOptions = options || [];
          this.ensureCurrentBatchListed();
        }
      });
  }

  /** The HQ-only الجمعية filter — narrows the batch picker, never the saved payload. */
  private loadFilterCharities(): void {
    this.charityService.getCharities({ pageNumber: 1, pageSize: 500 })
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: response => {
          this.filterCharityOptions = [
            { id: CHARITY_FILTER_ALL, name: this.translate.instant('orphanPayments.allCharities') },
            ...(response.items || []).map(c => ({ id: c.id as string, name: c.name }))
          ];
        }
      });
  }

  /** The shared dropdown emits the selected option (or null when cleared). */
  onFilterCharityChanged(value: DropdownOption | null): void {
    this.selectedCharityId = value && value.id != null && value.id !== CHARITY_FILTER_ALL ? value.id : null;
    this.loadBatchOptions();
  }

  /** Edit-mode echo: the saved BatchNo must stay selectable even if the list lags behind it. */
  private ensureCurrentBatchListed(): void {
    const current = this.form.get('batchNo')?.value;
    if (current && !this.batchOptions.some(b => b.batchNo === current)) {
      this.batchOptions = [{ batchNo: current, latestGroupDate: null }, ...this.batchOptions];
    }
  }

  trackByBatchNo(index: number, item: BatchNumberOptionDto): string {
    return item.batchNo;
  }

  // ==================== HELPERS ====================

  get f(): { [key: string]: AbstractControl } {
    return this.form.controls;
  }

  hasError(controlName: string, errorName: string): boolean {
    const control = this.form.get(controlName);
    return control ? control.hasError(errorName) && (control.touched || control.dirty) : false;
  }

  /**
   * Maps the 400 payload's field→messages dictionary onto the form.
   * Server keys are PascalCase DTO names — form controls are camelCase.
   */
  private applyServerErrors(details: Record<string, string[]> | null | undefined): void {
    this.serverErrors = {};
    if (!details) return;
    Object.entries(details).forEach(([key, messages]) => {
      const controlName = key.charAt(0).toLowerCase() + key.slice(1);
      const first = Array.isArray(messages) ? messages[0] : null;
      if (first) {
        this.serverErrors[controlName] = first;
        this.form.get(controlName)?.markAsTouched();
      }
    });
  }

  /**
   * 10-4: business refusals arrive as a plain 400 { message } (no field map) — e.g. the
   * DontRemoveRate lock (سعر الصرف + عدم خصم النسبه). Known server messages map to an
   * i18n key; anything else shows verbatim rather than vanishing.
   */
  private resolveServerMessage(err: any): string | null {
    const message: string | null | undefined = err?.message;
    if (!message) return null;
    switch (message) {
      case 'Exchange rate is locked and cannot be modified':
        return this.translate.instant('orphanPayments.exchangeRateLockedError');
      default:
        return message;
    }
  }

  private markFormGroupTouched(formGroup: FormGroup): void {
    Object.keys(formGroup.controls).forEach(key => {
      const control = formGroup.get(key);
      if (control) {
        control.markAsTouched();
        if (control instanceof FormGroup) {
          this.markFormGroupTouched(control);
        }
      }
    });
  }

  isExchangeRateLocked(): boolean {
    return this.form.get('dontRemoveRate')?.value === true;
  }

  onDontRemoveRateChange(checked: boolean): void {
    const exchangeRateControl = this.form.get('exchangeRate');
    const currencyControl = this.form.get('currency');

    if (checked) {
      exchangeRateControl?.disable();
      currencyControl?.disable();
    } else {
      exchangeRateControl?.enable();
      currencyControl?.enable();
    }
  }
}
