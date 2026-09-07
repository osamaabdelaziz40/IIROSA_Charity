import { Component, OnDestroy, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { Subject } from 'rxjs';
import { debounceTime, distinctUntilChanged, takeUntil } from 'rxjs/operators';

import { GeneralChecksService } from '../services/general-checks.service';
import { LookupManagementService } from '../../lookup-management/services/lookup-management.service';
import { CharityService } from '../../charities/services/charity.service';
import { NotificationService } from '../../../core/services/notification.service';
import { AuthService } from '../../../core/services/auth.service';
import {
  ChequeBeneficiaryOption,
  BankChequePositions,
  CreateCheckRequest,
  UpdateCheckRequest
} from '../models/check.model';
import { BreadcrumbComponent, BreadcrumbItem } from '../../../shared/components';
import { SharedModule } from '../../../shared/shared.module';
import { DropDownComponent } from '../../../shared/components/drop-down/drop-down.component';

/** What the print overlay renders — the cheque face, positioned by layout mode. */
interface ChequePrintModel {
  checkNumber: string;
  checkDate: string;
  beneficiaryName: string;
  amount: number;
  amountInWords: string;
  currency: string;
  bankName: string;
  egyptian: boolean;
  positions: BankChequePositions | null;
}

/** Fallback Egyptian-stationery offsets (mm from the leaf's top-right) when the bank row has none. */
const DEFAULT_POSITIONS: BankChequePositions = {
  bankId: 0,
  bankName: '',
  configured: false,
  dateX: 150, dateY: 16,
  payeeX: 95, payeeY: 32,
  amountX: 150, amountY: 32,
  amountWordsX: 12, amountWordsY: 50
};

/**
 * §16.S.2 — the cheque form. Mandatory: البنك، اسم المستفيد، تاريخ الشيك، رقم الشيك،
 * العملة، المبلغ. Optional: تعليقات and the four flags شيك تالف / تم رد الشيك /
 * تم الصرف / CheckDone. Commands: طباعة / طباعة مصري / حفظ.
 */
@Component({
  selector: 'app-check-form',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    ReactiveFormsModule,
    TranslateModule,
    BreadcrumbComponent,
    SharedModule,
    DropDownComponent
  ],
  templateUrl: './check-form.component.html',
  styleUrls: ['./check-form.component.scss']
})
export class CheckFormComponent implements OnInit, OnDestroy {
  breadcrumbs: BreadcrumbItem[] = [
    { label: 'common.home', url: '/dashboard' },
    { label: 'generalChecks.title', url: '/general-checks' },
    { label: 'generalChecks.addCheck' }
  ];

  isEdit = false;
  checkId: string | null = null;
  saving = false;
  loading = false;

  isHeadOffice = false;

  bankOptions: Array<{ id: number; name: string }> = [];
  currencyOptions: Array<{ id: string; name: string }> = [];
  charityOptions: Array<{ id: string; name: string }> = [];

  /** UC-CHQ-05 — type-ahead suggestions under the beneficiary name input. */
  beneficiarySuggestions: ChequeBeneficiaryOption[] = [];
  showSuggestions = false;

  /** UC-CHQ-07 — words are server-generated unless the user edits the field by hand. */
  private wordsManuallyEdited = false;
  private fillingFromSuggestion = false;

  printModel: ChequePrintModel | null = null;

  checkForm: FormGroup;

  private readonly destroy$ = new Subject<void>();

  constructor(
    private fb: FormBuilder,
    private route: ActivatedRoute,
    private router: Router,
    private generalChecksService: GeneralChecksService,
    private lookupManagementService: LookupManagementService,
    private charityService: CharityService,
    private authService: AuthService,
    private notification: NotificationService,
    private translate: TranslateService
  ) {
    this.checkForm = this.fb.group({
      bankId: [null, Validators.required],
      beneficiaryName: ['', [Validators.required, Validators.maxLength(200)]],
      chequeBeneficiaryId: [null],
      beneficiaryType: [null],
      beneficiaryAddress: [null],
      beneficiaryPhone: [null],
      beneficiaryEmail: [null],
      beneficiaryIdNumber: [null],
      checkDate: [this.todayIso(), Validators.required],
      checkNumber: ['', [Validators.required, Validators.maxLength(50)]],
      currency: ['EGP', Validators.required],
      amount: [null, [Validators.required, Validators.min(0.01)]],
      amountInWords: [null],
      bankBranch: [null],
      accountNumber: [null],
      chequeType: ['Individuals'],
      isDamaged: [false],
      isReturned: [false],
      isDispensed: [false],
      isDone: [false],
      comment: [null],
      charityId: [null]
    });
  }

  ngOnInit(): void {
    this.isHeadOffice = this.authService.hasAnyRole(['SuperAdmin', 'Admin']);

    this.route.params.pipe(takeUntil(this.destroy$)).subscribe(params => {
      if (params['id']) {
        this.isEdit = true;
        this.checkId = params['id'];
        this.breadcrumbs[this.breadcrumbs.length - 1].label = 'generalChecks.editCheck';
      }
      this.loadLookups();
    });

    this.setupBeneficiaryTypeAhead();
    this.setupAmountInWords();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  private todayIso(): string {
    return new Date().toISOString().slice(0, 10);
  }

  private loadLookups(): void {
    this.lookupManagementService
      .getBanks({ pageNumber: 1, pageSize: 1000, isActive: true } as any)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: result => {
          this.bankOptions = (result.items || []).map(b => ({ id: b.id, name: b.nameAr || b.name }));
          if (this.isEdit) {
            this.loadCheck();
          }
        },
        error: () => this.notification.error(this.translate.instant('generalChecks.loadFailed'))
      });

    // UC-CHQ-06 — the distinct currencies configured on countries.
    this.generalChecksService.getCurrencies()
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: currencies => {
          this.currencyOptions = (currencies || []).map(c => ({ id: c.code, name: c.code }));
        }
      });

    if (this.isHeadOffice) {
      this.charityService.getCharities({ pageNumber: 1, pageSize: 1000, isActive: true })
        .pipe(takeUntil(this.destroy$))
        .subscribe({
          next: response => {
            this.charityOptions = (response.items || []).map(c => ({ id: c.id, name: c.name }));
          }
        });
    }
  }

  private loadCheck(): void {
    if (!this.checkId) return;
    this.loading = true;

    this.generalChecksService.getCheckById(this.checkId)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: check => {
          this.fillingFromSuggestion = true;
          this.checkForm.patchValue({
            bankId: check.bankId,
            beneficiaryName: check.beneficiaryName,
            chequeBeneficiaryId: check.chequeBeneficiaryId,
            beneficiaryType: check.beneficiaryType,
            beneficiaryAddress: check.beneficiaryAddress,
            beneficiaryPhone: check.beneficiaryPhone,
            beneficiaryEmail: check.beneficiaryEmail,
            beneficiaryIdNumber: check.beneficiaryIdNumber,
            checkDate: check.checkDate ? check.checkDate.slice(0, 10) : this.todayIso(),
            checkNumber: check.checkNumber,
            currency: check.currency,
            amount: check.amount,
            amountInWords: check.amountInWords,
            bankBranch: check.bankBranch,
            accountNumber: check.accountNumber,
            chequeType: check.chequeType,
            isDamaged: check.isDamaged,
            isReturned: check.isReturned,
            isDispensed: check.isDispensed,
            isDone: check.isDone,
            comment: check.comment,
            charityId: check.charityId
          });
          this.fillingFromSuggestion = false;
          this.loading = false;
        },
        error: () => {
          this.loading = false;
          this.router.navigate(['/general-checks']);
        }
      });
  }

  /** UC-CHQ-05 — debounced name search over the reusable cheque-beneficiary table. */
  private setupBeneficiaryTypeAhead(): void {
    this.checkForm.controls['beneficiaryName'].valueChanges
      .pipe(
        debounceTime(300),
        distinctUntilChanged(),
        takeUntil(this.destroy$)
      )
      .subscribe(term => {
        if (this.fillingFromSuggestion || !term || typeof term !== 'string' || term.trim().length < 2) {
          this.beneficiarySuggestions = [];
          this.showSuggestions = false;
          return;
        }

        this.generalChecksService.getChequeBeneficiaries(term.trim(), 10)
          .pipe(takeUntil(this.destroy$))
          .subscribe({
            next: options => {
              this.beneficiarySuggestions = options || [];
              this.showSuggestions = this.beneficiarySuggestions.length > 0;
            }
          });
      });
  }

  pickBeneficiary(option: ChequeBeneficiaryOption): void {
    this.fillingFromSuggestion = true;
    this.checkForm.patchValue({
      beneficiaryName: option.nameAr || option.nameEn || option.name,
      chequeBeneficiaryId: option.id,
      beneficiaryType: option.beneficiaryType,
      beneficiaryAddress: option.address,
      beneficiaryPhone: option.phone,
      beneficiaryEmail: option.email,
      beneficiaryIdNumber: option.idNumber,
      bankBranch: this.checkForm.value.bankBranch || null,
      accountNumber: option.accountNumber || this.checkForm.value.accountNumber
    });
    this.fillingFromSuggestion = false;
    this.showSuggestions = false;
  }

  trackBySuggestion(index: number, option: ChequeBeneficiaryOption): number {
    return option.id;
  }

  hideSuggestions(): void {
    // Delay so a click on a suggestion registers before the list disappears.
    setTimeout(() => (this.showSuggestions = false), 200);
  }

  clearBeneficiaryLink(): void {
    this.checkForm.patchValue({ chequeBeneficiaryId: null });
  }

  /** UC-CHQ-07 — keep the words in step with amount/currency unless typed by hand. */
  private setupAmountInWords(): void {
    const refresh = () => {
      if (this.wordsManuallyEdited) return;
      const amount = this.checkForm.value.amount;
      const currency = this.checkForm.value.currency;
      if (!amount || amount <= 0 || !currency) {
        this.checkForm.controls['amountInWords'].patchValue(null, { emitEvent: false });
        return;
      }

      this.generalChecksService.getAmountInWords(amount, currency)
        .pipe(takeUntil(this.destroy$))
        .subscribe({
          next: response => {
            if (!this.wordsManuallyEdited) {
              this.checkForm.controls['amountInWords'].patchValue(response.words, { emitEvent: false });
            }
          }
        });
    };

    this.checkForm.controls['amount'].valueChanges
      .pipe(debounceTime(500), distinctUntilChanged(), takeUntil(this.destroy$))
      .subscribe(refresh);
    this.checkForm.controls['currency'].valueChanges
      .pipe(debounceTime(500), distinctUntilChanged(), takeUntil(this.destroy$))
      .subscribe(refresh);

    this.checkForm.controls['amountInWords'].valueChanges
      .pipe(takeUntil(this.destroy$))
      .subscribe(() => {
        if (!this.fillingFromSuggestion && this.checkForm.controls['amountInWords'].dirty) {
          this.wordsManuallyEdited = true;
        }
      });
  }

  /**
   * UC-CHQ-08 / UC-CHQ-10 — طباعة (default layout) and طباعة مصري (stationery offsets).
   * Prints the on-screen values; saving first is not required.
   */
  printCheque(egyptian: boolean): void {
    if (this.checkForm.invalid) {
      this.notification.error(this.translate.instant('validation.fixErrors'));
      return;
    }

    const formValue = this.checkForm.value;
    const bank = this.bankOptions.find(b => b.id === formValue.bankId);

    const render = (positions: BankChequePositions | null) => {
      this.printModel = {
        checkNumber: formValue.checkNumber,
        checkDate: formValue.checkDate,
        beneficiaryName: formValue.beneficiaryName,
        amount: formValue.amount,
        amountInWords: formValue.amountInWords || '',
        currency: formValue.currency,
        bankName: bank?.name || '',
        egyptian,
        positions: egyptian ? (positions ?? DEFAULT_POSITIONS) : null
      };
      // Give change detection a tick to render the overlay before the print dialog.
      setTimeout(() => window.print(), 50);
    };

    if (egyptian && formValue.bankId) {
      this.generalChecksService.getBankChequePositions(formValue.bankId)
        .pipe(takeUntil(this.destroy$))
        .subscribe({
          next: positions => render(positions.configured ? positions : null),
          error: () => render(null)
        });
    } else {
      render(null);
    }
  }

  /** Stationery offset helper for the overlay template — always a number in the end. */
  pos(axis: number | null | undefined, fallback: number): number {
    return axis ?? fallback;
  }

  onSubmit(): void {
    if (this.checkForm.invalid) {
      this.checkForm.markAllAsTouched();
      this.notification.error(this.translate.instant('validation.fixErrors'));
      return;
    }

    this.saving = true;
    const formValue = this.checkForm.value;

    const base: CreateCheckRequest = {
      bankId: formValue.bankId,
      beneficiaryName: formValue.beneficiaryName,
      chequeBeneficiaryId: formValue.chequeBeneficiaryId ?? null,
      beneficiaryType: formValue.beneficiaryType || null,
      beneficiaryAddress: formValue.beneficiaryAddress || null,
      beneficiaryPhone: formValue.beneficiaryPhone || null,
      beneficiaryEmail: formValue.beneficiaryEmail || null,
      beneficiaryIdNumber: formValue.beneficiaryIdNumber || null,
      checkDate: formValue.checkDate,
      checkNumber: formValue.checkNumber,
      currency: formValue.currency,
      amount: formValue.amount,
      amountInWords: formValue.amountInWords || null,
      bankBranch: formValue.bankBranch || null,
      accountNumber: formValue.accountNumber || null,
      chequeType: formValue.chequeType || 'Individuals',
      isDamaged: !!formValue.isDamaged,
      isReturned: !!formValue.isReturned,
      isDispensed: !!formValue.isDispensed,
      isDone: !!formValue.isDone,
      comment: formValue.comment || null,
      charityId: formValue.charityId || null
    };

    const call = this.isEdit && this.checkId
      ? this.generalChecksService.updateCheck({ ...base, id: this.checkId } as UpdateCheckRequest)
      : this.generalChecksService.createCheck(base);

    call.subscribe({
      next: check => {
        this.notification.success(
          this.translate.instant(this.isEdit ? 'generalChecks.checkUpdated' : 'generalChecks.checkCreated'));
        this.router.navigate(['/general-checks', check.id]);
      },
      error: (err) => {
        this.saving = false;
        this.applyServerErrors(err);
      }
    });
  }

  /** Maps the 400 { message, errors } body onto the form controls it names. */
  private applyServerErrors(err: any): void {
    const errorMap = err?.error?.errors;
    if (errorMap && typeof errorMap === 'object') {
      // Wire names are camelCase DTO properties; form control names match one-to-one.
      Object.entries(errorMap).forEach(([property, messages]) => {
        const control = this.checkForm.get(this.toCamel(property));
        if (control && Array.isArray(messages) && messages.length) {
          control.setErrors({ server: messages[0] });
        }
      });
      this.notification.error(this.translate.instant('validation.fixErrors'));
    } else {
      this.notification.error(err?.error?.message || this.translate.instant('generalChecks.saveFailed'));
    }
  }

  /** "Amount" / "CheckNumber" (validator PropertyName) → "amount" / "checkNumber". */
  private toCamel(property: string): string {
    return property ? property.charAt(0).toLowerCase() + property.slice(1) : '';
  }
}
