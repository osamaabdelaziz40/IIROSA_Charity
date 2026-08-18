import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup, Validators, AbstractControl, ValidationErrors } from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { RouterModule } from '@angular/router';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { Observable, Subject, takeUntil } from 'rxjs';
import { map } from 'rxjs/operators';
import {
  OrphanPaymentDto,
  CreateOrphanPaymentDto,
  UpdateOrphanPaymentDto,
  CURRENCY_OPTIONS
} from '../models/orphan-payment.model';
import { OrphanPaymentService } from '../services/orphan-payment.service';

@Component({
  selector: 'app-orphan-payment-form',
  standalone: true,
  imports: [CommonModule, FormsModule, ReactiveFormsModule, TranslateModule, RouterModule],
  templateUrl: './orphan-payment-form.component.html',
  styleUrls: ['./orphan-payment-form.component.scss']
})
export class OrphanPaymentFormComponent implements OnInit, OnDestroy {
  private destroy$ = new Subject<void>();

  // Form
  form: FormGroup;
  isEditMode = false;
  paymentGroupId: string | null = null;
  loading = false;
  saving = false;

  // Data
  paymentGroup: OrphanPaymentDto | null = null;

  // Options
  currencyOptions = CURRENCY_OPTIONS;
  availableCharities: { id: number; name: string }[] = [];
  availableRegions: { id: number; name: string }[] = [];
  availableCenters: { id: number; name: string }[] = [];

  // UI State
  showBatchNumberInput = false;
  isGeneratingBatchNumber = false;

  constructor(
    private fb: FormBuilder,
    private router: Router,
    private route: ActivatedRoute,
    private orphanPaymentService: OrphanPaymentService,
    private translate: TranslateService
  ) {
    this.form = this.buildForm();
  }

  ngOnInit(): void {
    this.loadFilterOptions();
    this.checkEditMode();

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

      // Financial Information
      exchangeRate: [null, [Validators.min(0), Validators.max(1000)]],
      currency: ['EGP'],
      dontRemoveRate: [false],

      // Filtering Options
      charityId: [null],
      regionId: [null],
      centerId: [null],

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
      exchangeRate: data.exchangeRate,
      currency: data.currency,
      dontRemoveRate: data.dontRemoveRate,
      charityId: data.charityId,
      regionId: data.regionId,
      centerId: data.centerId,
      batchNo: data.batchNo,
      showOrder: data.showOrder,
      notes: data.notes
    });

    // Disable exchange rate if locked
    if (data.dontRemoveRate) {
      this.form.get('exchangeRate')?.disable();
      this.form.get('currency')?.disable();
    }
  }

  private loadFilterOptions(): void {
    this.orphanPaymentService.getAvailableCharities().subscribe(data => {
      this.availableCharities = data;
    });

    this.orphanPaymentService.getAvailableRegions().subscribe(data => {
      this.availableRegions = data;
    });

    this.orphanPaymentService.getAvailableCenters().subscribe(data => {
      this.availableCenters = data;
    });
  }

  // ==================== FORM ACTIONS ====================

  onSubmit(): void {
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
      exchangeRate: formValue.exchangeRate,
      currency: formValue.currency,
      dontRemoveRate: formValue.dontRemoveRate,
      charityId: formValue.charityId,
      regionId: formValue.regionId,
      centerId: formValue.centerId,
      batchNo: formValue.batchNo || undefined,
      showOrder: formValue.showOrder,
      notes: formValue.notes
    };

    this.orphanPaymentService.createOrphanPayment(dto).subscribe({
      next: (result) => {
        this.saving = false;
        // Navigate to add orphans page
        this.router.navigate(['/orphan-payments', result.id, 'add-orphans']);
      },
      error: () => {
        this.saving = false;
      }
    });
  }

  private updatePaymentGroup(): void {
    const formValue = this.form.value;
    const dto: UpdateOrphanPaymentDto = {
      groupName: formValue.groupName,
      description: formValue.description,
      paymentPeriodFrom: formValue.paymentPeriodFrom,
      paymentPeriodTo: formValue.paymentPeriodTo,
      groupDate: formValue.groupDate,
      exchangeRate: formValue.exchangeRate,
      currency: formValue.currency,
      dontRemoveRate: formValue.dontRemoveRate,
      charityId: formValue.charityId,
      regionId: formValue.regionId,
      centerId: formValue.centerId,
      batchNo: formValue.batchNo,
      showOrder: formValue.showOrder,
      notes: formValue.notes
    };

    this.orphanPaymentService.updateOrphanPayment(this.paymentGroupId!, dto).subscribe({
      next: () => {
        this.saving = false;
        this.router.navigate(['/orphan-payments', this.paymentGroupId]);
      },
      error: () => {
        this.saving = false;
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

  toggleBatchNumberInput(): void {
    this.showBatchNumberInput = !this.showBatchNumberInput;
  }

  generateNextBatchNumber(): void {
    this.isGeneratingBatchNumber = true;
    this.orphanPaymentService.generateNextBatchNumber().subscribe({
      next: (result) => {
        this.form.patchValue({ batchNo: result.nextBatchNumber });
        this.showBatchNumberInput = true;
        this.isGeneratingBatchNumber = false;
      },
      error: () => {
        this.isGeneratingBatchNumber = false;
      }
    });
  }

  // ==================== HELPERS ====================

  get f(): { [key: string]: AbstractControl } {
    return this.form.controls;
  }

  hasError(controlName: string, errorName: string): boolean {
    const control = this.form.get(controlName);
    return control ? control.hasError(errorName) && (control.touched || control.dirty) : false;
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

  getTitle(): string {
    return this.isEditMode
      ? this.translate.instant('orphanPayments.editGroup')
      : this.translate.instant('orphanPayments.addNewGroup');
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
