import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router, Params } from '@angular/router';
import {
  Check,
  CheckDto,
  BeneficiaryType,
  Currency,
  PaymentReason,
  CheckStatus
} from '../models/check.model';
import { GeneralChecksService } from '../services/general-checks.service';
import { LookupManagementService } from '../../../modules/lookup-management/services/lookup-management.service';
import { NotificationService } from '../../../core/services/notification.service';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { Observable, forkJoin, Subject } from 'rxjs';
import { takeUntil, debounceTime } from 'rxjs/operators';
import { PageHeaderComponent } from '../../../shared/components/page-header/page-header.component';
import { BreadcrumbComponent, BreadcrumbItem } from '../../../shared/components';
import { SharedModule } from '../../../shared/shared.module';

@Component({
  selector: 'app-check-form',
  standalone: true,
  imports: [CommonModule, FormsModule, ReactiveFormsModule, TranslateModule, PageHeaderComponent, BreadcrumbComponent, SharedModule],
  templateUrl: './check-form.component.html',
  styleUrls: ['./check-form.component.scss']
})
export class CheckFormComponent implements OnInit, OnDestroy {
  // Breadcrumb items
  breadcrumbs: BreadcrumbItem[] = [
    { label: 'common.home', url: '/dashboard' },
    { label: 'generalChecks.title', url: '/general-checks' }
  ];

  get breadcrumbsWithAction(): BreadcrumbItem[] {
    return [
      ...this.breadcrumbs,
      { label: this.isEditMode ? 'generalChecks.editCheck' : 'generalChecks.addCheck' }
    ];
  }
  private destroy$ = new Subject<void>();

  checkForm!: FormGroup;
  isEditMode = false;
  checkId: number | null = null;
  loading = false;
  saving = false;

  // Lookup data
  allBanks: any[] = [];
  allBeneficiaries: any[] = [];

  // Check image
  selectedCheckImage: File | null = null;
  existingCheckImageUrl?: string;

  // Enums
  CheckStatus = CheckStatus;
  beneficiaryTypes = Object.values(BeneficiaryType);
  currencies = Object.values(Currency);
  paymentReasons = Object.values(PaymentReason);

  // UI state
  beneficiarySource: 'lookup' | 'new' = 'lookup';
  showCheckImageUpload = false;

  amountInWords$: Subject<string> = new Subject<string>();

  // Page actions for header
  pageActions = [
    {
      label: 'common.cancel',
      type: 'secondary',
      icon: 'x',
      click: () => this.onCancel()
    }
  ];

  // Dropdown options getters
  get currencyOptions() {
    return this.currencies.map(currency => ({
      id: currency,
      name: this.getCurrencyLabel(currency)
    }));
  }

  get beneficiaryTypeOptions() {
    return this.beneficiaryTypes.map(type => ({
      id: type,
      name: this.getBeneficiaryTypeLabel(type)
    }));
  }

  get paymentReasonOptions() {
    return this.paymentReasons.map(reason => ({
      id: reason,
      name: this.getPaymentReasonLabel(reason)
    }));
  }

  get bankOptions() {
    return this.allBanks.map(bank => ({
      id: bank.id,
      name: bank.bankName
    }));
  }

  get beneficiaryOptions() {
    return this.allBeneficiaries.map(beneficiary => ({
      id: beneficiary.id,
      name: beneficiary.beneficiaryName
    }));
  }

  get checkStatusOptions() {
    return [
      { id: CheckStatus.Pending, name: this.translate.instant('generalChecks.checkStatuses.Pending') },
      { id: CheckStatus.Issued, name: this.translate.instant('generalChecks.checkStatuses.Issued') }
    ];
  }

  constructor(
    private fb: FormBuilder,
    private route: ActivatedRoute,
    private router: Router,
    private checksService: GeneralChecksService,
    private lookupService: LookupManagementService,
    private notification: NotificationService,
    private translate: TranslateService
  ) {
    this.initForm();
    this.setupAmountWatcher();
  }

  ngOnInit(): void {
    this.route.params.subscribe((params: Params) => {
      if (params['id']) {
        this.isEditMode = true;
        this.checkId = +params['id'];
        this.loadCheck(this.checkId);
      }
    });

    this.loadLookupData();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  private initForm(): void {
    this.checkForm = this.fb.group({
      // Check Information
      checkNumber: ['', Validators.required],
      checkDate: [new Date(), Validators.required],
      dueDate: [null, Validators.required],
      currency: [Currency.EGP, Validators.required],

      // Beneficiary Information
      beneficiaryType: [BeneficiaryType.Individual, Validators.required],
      beneficiaryName: ['', Validators.required],
      beneficiaryId: [null],
      beneficiaryAddress: ['', Validators.required],
      beneficiaryPhone: ['', Validators.required],
      beneficiaryEmail: ['', Validators.required],
      idNumber: ['', Validators.required],

      // Financial Information
      amount: [null, [Validators.required, Validators.min(0.01)]],
      amountInWords: ['', Validators.required],
      paymentReason: [PaymentReason.Other, Validators.required],
      paymentDescription: ['', Validators.required],

      // Bank Information
      bankId: [null, Validators.required],
      branch: ['', Validators.required],
      accountNumber: ['', Validators.required],

      // Status
      checkStatus: [CheckStatus.Pending, Validators.required],
      issueDate: [new Date(), Validators.required],

      // Approval
      requiresApproval: [false], // Boolean field, not required

      // Check Image
      checkImageId: [null, Validators.required]
    });
  }

  private setupAmountWatcher(): void {
    this.checkForm.get('amount')?.valueChanges
      .pipe(
        debounceTime(500),
        takeUntil(this.destroy$)
      )
      .subscribe(amount => {
        if (amount && amount > 0) {
          this.generateAmountInWords(amount);
        } else {
          this.checkForm.patchValue({ amountInWords: '' });
        }
      });
  }

  private loadLookupData(): void {
    const observables: Observable<any>[] = [
      this.lookupService.getBanks({ page: 1, pageSize: 1000 }),
      this.checksService.getBeneficiaries()
    ];

    forkJoin(observables).pipe(takeUntil(this.destroy$)).subscribe({
      next: ([banksResponse, beneficiaries]) => {
        // Map BankDto to Bank format (name -> bankName)
        this.allBanks = (banksResponse?.items || []).map((bank: any) => ({
          id: bank.id,
          bankName: bank.name,
          bankNameAr: bank.nameAr,
          bankNameEn: bank.nameEn,
          isActive: bank.isActive
        }));
        this.allBeneficiaries = beneficiaries || [];
      },
      error: (error) => {
        console.error('Error loading lookup data:', error);
        this.notification.error('Failed to load lookup data');
      }
    });
  }

  private loadCheck(id: number): void {
    this.loading = true;
    this.checksService.getCheckById(id).subscribe({
      next: (check: Check) => {
        this.patchForm(check);
        this.existingCheckImageUrl = check.checkImageUrl;
        this.loading = false;
      },
      error: (error: any) => {
        console.error('Error loading check:', error);
        this.notification.error(`Failed to load check: ${error.message || 'Unknown error'}`);
        this.loading = false;
        this.router.navigate(['/general-checks']);
      }
    });
  }

  private patchForm(check: Check): void {
    this.checkForm.patchValue({
      checkNumber: check.checkNumber,
      checkDate: check.checkDate,
      dueDate: check.dueDate || null,
      currency: check.currency,
      beneficiaryType: check.beneficiaryType,
      beneficiaryName: check.beneficiaryName,
      beneficiaryId: check.beneficiaryId || null,
      beneficiaryAddress: check.beneficiaryAddress || '',
      beneficiaryPhone: check.beneficiaryPhone || '',
      beneficiaryEmail: check.beneficiaryEmail || '',
      idNumber: check.idNumber || '',
      amount: check.amount,
      amountInWords: check.amountInWords || '',
      paymentReason: check.paymentReason,
      paymentDescription: check.paymentDescription || '',
      bankId: check.bankId,
      branch: check.branch || '',
      accountNumber: check.accountNumber || '',
      checkStatus: check.checkStatus,
      issueDate: check.issueDate || new Date(),
      requiresApproval: check.requiresApproval,
      checkImageId: check.checkImageId || null
    });
  }

  private generateAmountInWords(amount: number): void {
    const currency = this.checkForm.get('currency')?.value;
    this.checksService.generateAmountInWords(amount, currency).subscribe({
      next: (words: string) => {
        this.checkForm.patchValue({ amountInWords: words });
      },
      error: () => {
        // If auto-generation fails, user can manually enter
        console.warn('Failed to auto-generate amount in words');
      }
    });
  }

  onBeneficiarySourceChange(): void {
    if (this.beneficiarySource === 'lookup') {
      this.checkForm.get('beneficiaryId')?.setValidators(Validators.required);
      this.checkForm.get('beneficiaryName')?.clearValidators();
    } else {
      this.checkForm.get('beneficiaryId')?.clearValidators();
      this.checkForm.get('beneficiaryName')?.setValidators(Validators.required);
    }
    this.checkForm.get('beneficiaryId')?.updateValueAndValidity();
    this.checkForm.get('beneficiaryName')?.updateValueAndValidity();
  }

  onBeneficiarySelected(): void {
    const beneficiaryId = this.checkForm.get('beneficiaryId')?.value;
    if (beneficiaryId) {
      const beneficiary = this.allBeneficiaries.find(b => b.id === beneficiaryId);
      if (beneficiary) {
        this.checkForm.patchValue({
          beneficiaryType: beneficiary.beneficiaryType,
          beneficiaryName: beneficiary.beneficiaryName,
          beneficiaryAddress: beneficiary.address || '',
          beneficiaryPhone: beneficiary.phone || '',
          beneficiaryEmail: beneficiary.email || '',
          idNumber: beneficiary.idNumber || ''
        });
      }
    }
  }

  onCheckImageSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (input.files && input.files.length > 0) {
      this.selectedCheckImage = input.files[0];
    }
  }

  removeCheckImage(): void {
    this.selectedCheckImage = null;
    this.existingCheckImageUrl = undefined;
    this.checkForm.patchValue({ checkImageId: null });
  }

  toggleCheckImageUpload(): void {
    this.showCheckImageUpload = !this.showCheckImageUpload;
  }

  onSubmit(): void {
    if (this.checkForm.invalid) {
      this.markFormGroupTouched(this.checkForm);
      this.notification.error(this.translate.instant('validation.fixErrors'));
      return;
    }

    this.saving = true;
    const formValue = this.checkForm.value;

    const checkDto: CheckDto = {
      id: this.isEditMode ? this.checkId! : undefined,
      checkNumber: formValue.checkNumber,
      checkDate: formValue.checkDate,
      dueDate: formValue.dueDate || undefined,
      currency: formValue.currency,
      beneficiaryType: formValue.beneficiaryType,
      beneficiaryName: formValue.beneficiaryName,
      beneficiaryId: formValue.beneficiaryId || undefined,
      beneficiaryAddress: formValue.beneficiaryAddress || undefined,
      beneficiaryPhone: formValue.beneficiaryPhone || undefined,
      beneficiaryEmail: formValue.beneficiaryEmail || undefined,
      idNumber: formValue.idNumber || undefined,
      amount: formValue.amount,
      amountInWords: formValue.amountInWords || undefined,
      paymentReason: formValue.paymentReason,
      paymentDescription: formValue.paymentDescription || undefined,
      bankId: formValue.bankId,
      branch: formValue.branch || undefined,
      accountNumber: formValue.accountNumber || undefined,
      checkStatus: formValue.checkStatus,
      issueDate: formValue.issueDate || new Date(),
      requiresApproval: formValue.requiresApproval,
      checkImageId: formValue.checkImageId || undefined
    };

    const operation = this.isEditMode
      ? this.checksService.updateCheck(this.checkId!, checkDto)
      : this.checksService.createCheck(checkDto);

    operation.subscribe({
      next: (check: Check) => {
        this.notification.success(
          this.isEditMode
            ? this.translate.instant('generalChecks.checkUpdated')
            : this.translate.instant('generalChecks.checkCreated')
        );

        // Upload check image if selected
        if (this.selectedCheckImage) {
          this.uploadCheckImage(check.id!);
        } else {
          this.saving = false;
          this.router.navigate(['/general-checks', check.id]);
        }
      },
      error: (error: any) => {
        console.error('Error saving check:', error);
        this.notification.error(
          `Failed to save check: ${error.message || 'Unknown error'}`
        );
        this.saving = false;
      }
    });
  }

  private uploadCheckImage(checkId: number): void {
    if (!this.selectedCheckImage) return;

    this.checksService.uploadCheckImage(checkId, this.selectedCheckImage).subscribe({
      next: () => {
        this.notification.success('Check image uploaded successfully');
        this.saving = false;
        this.router.navigate(['/general-checks', checkId]);
      },
      error: (error: any) => {
        console.error('Error uploading check image:', error);
        this.notification.error(`Failed to upload check image: ${error.message || 'Unknown error'}`);
        this.saving = false;
      }
    });
  }

  onCancel(): void {
    if (this.isEditMode && this.checkId) {
      this.router.navigate(['/general-checks', this.checkId]);
    } else {
      this.router.navigate(['/general-checks']);
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

  isFieldInvalid(fieldName: string): boolean {
    const field = this.checkForm.get(fieldName);
    return field ? field.invalid && (field.dirty || field.touched) : false;
  }

  getErrorMessage(fieldName: string): string {
    const field = this.checkForm.get(fieldName);
    if (!field || !field.errors) return '';

    if (field.errors['required']) {
      return `${fieldName} is required`;
    }
    if (field.errors['min']) {
      return `Minimum value is ${field.errors['min'].min}`;
    }
    if (field.errors['email']) {
      return 'Invalid email format';
    }

    return 'Invalid field';
  }

  getBeneficiaryTypeLabel(type: BeneficiaryType): string {
    return this.translate.instant(`generalChecks.beneficiaryTypes.${type}`);
  }

  getCurrencyLabel(currency: Currency): string {
    return this.translate.instant(`generalChecks.currencies.${currency}`);
  }

  getPaymentReasonLabel(reason: PaymentReason): string {
    return this.translate.instant(`generalChecks.paymentReasons.${reason}`);
  }
}
