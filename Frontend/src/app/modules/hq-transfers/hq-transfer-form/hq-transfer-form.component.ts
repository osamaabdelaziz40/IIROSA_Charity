/**
 * HQ Transfer Form Component (epic 17, UC-TRF-02 — §22.S.2)
 * Create a transfer: 12 fields (11 mandatory), catalogues from the live lookups —
 * countries and departments from LookupManagement. Payment number is the spec's
 * 1–4 installment select.
 *
 * 17-3: the same component serves read-only view mode on `#/hq-transfers/:id/view`
 * (route data.mode = 'view') — every control disabled, no حفظ, زرار رجوع back.
 */

import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { Subject } from 'rxjs';
import { catchError, of, switchMap, takeUntil } from 'rxjs';

import { HqTransferService } from '../services/hq-transfer.service';
import { CreateHqTransferRequest, UpdateHqTransferRequest, HqTransferDetail } from '../models/hq-transfer.model';
import { LookupManagementService } from '../../lookup-management/services/lookup-management.service';
import { CountryDto, DepartmentDto } from '../../lookup-management/models/lookup.model';
import { NotificationService } from '../../../core/services/notification.service';
import { PageHeaderComponent } from '../../../shared/components/page-header/page-header.component';
import { BreadcrumbComponent, BreadcrumbItem } from '../../../shared/components';
import type { PageAction } from '../../../shared/components/page-header/page-header.component';

@Component({
  selector: 'app-hq-transfer-form',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, TranslateModule, PageHeaderComponent, BreadcrumbComponent],
  templateUrl: './hq-transfer-form.component.html',
  styleUrls: ['./hq-transfer-form.component.scss']
})
export class HqTransferFormComponent implements OnInit, OnDestroy {
  private destroy$ = new Subject<void>();

  // One component, three modes (route data.mode): 'create' (default) · 'edit' (17-4) · 'view' (17-3)
  isViewMode = false;
  isEditMode = false;
  transferId: string | null = null;
  transfer: HqTransferDetail | null = null;
  notFound = false;
  // Edit/view load failed with a non-404 error (transient 500/timeout) — the record was
  // never seen by the actor, so the form stays disabled and submit is refused (a PUT
  // from an empty form would blind-overwrite the stored record)
  loadFailed = false;

  // Breadcrumb items
  breadcrumbs: BreadcrumbItem[] = [
    { label: 'common.home', url: '/dashboard' },
    { label: 'hqTransfers.title', url: '/hq-transfers' }
  ];

  // Page actions for header — view mode swaps cancel for رجوع
  pageActions: PageAction[] = [
    {
      label: 'common.cancel',
      type: 'secondary',
      icon: 'x',
      click: () => this.onCancel()
    }
  ];

  transferForm: FormGroup;
  loading = false;
  saving = false;
  error: string | null = null;

  // §22.S.2 catalogues
  countries: CountryDto[] = [];
  departments: DepartmentDto[] = [];
  catalogueFailed = false;

  // UC-TRF-06 ceiling guard — maxAmountKnown distinguishes "no limit configured" (null) from
  // "not fetched yet"; the server re-checks on save (the client guard is UX, not the control)
  maxAmount: number | null = null;
  maxAmountKnown = false;

  // الدفعات — the spec's four installments
  paymentNumbers: number[] = [1, 2, 3, 4];

  constructor(
    private fb: FormBuilder,
    private transferService: HqTransferService,
    private lookupService: LookupManagementService,
    private route: ActivatedRoute,
    private router: Router,
    private translate: TranslateService,
    private notification: NotificationService
  ) {
    this.transferForm = this.createForm();
  }

  /** Breadcrumb tail switches with the mode */
  get breadcrumbsWithAction(): BreadcrumbItem[] {
    const label = this.isViewMode
      ? 'hqTransfers.viewTransfer'
      : this.isEditMode ? 'hqTransfers.editTransfer' : 'hqTransfers.addTransfer';
    return [...this.breadcrumbs, { label }];
  }

  /** Header title switches with the mode */
  get pageTitle(): string {
    return this.isViewMode
      ? 'hqTransfers.viewTransfer'
      : this.isEditMode ? 'hqTransfers.editTransfer' : 'hqTransfers.addTransfer';
  }

  ngOnInit(): void {
    const mode = this.route.snapshot.data['mode'];
    this.isViewMode = mode === 'view';
    this.isEditMode = mode === 'edit';
    this.transferId = this.route.snapshot.params['id'] ?? null;

    if (this.isViewMode && this.transferId) {
      this.pageActions = [
        {
          label: 'hqTransfers.backToList',
          type: 'secondary',
          icon: 'arrow-left',
          click: () => this.onCancel()
        }
      ];
    }

    this.loadCatalogues();

    // UC-TRF-06: the ceiling follows the selected country. Edit mode's patchValue fires this
    // subscription too, so the loaded country's ceiling is re-fetched automatically. A failed
    // fetch stays silent — the helper text simply doesn't show and the server still enforces.
    this.transferForm.get('countryId')!.valueChanges
      .pipe(
        takeUntil(this.destroy$),
        switchMap((countryId: number | null) => {
          this.maxAmount = null;
          this.maxAmountKnown = false;
          if (!countryId) {
            return of(null);
          }
          return this.transferService.getMaxTransferAmount(countryId)
            .pipe(catchError(() => of(null)));
        })
      )
      .subscribe(ceiling => {
        if (ceiling) {
          this.maxAmount = ceiling.maxTransferAmount;
          this.maxAmountKnown = true;
        }
      });

    if ((this.isViewMode || this.isEditMode) && this.transferId) {
      this.loadTransfer(this.transferId);
    }
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  /**
   * §22.S.2 mandatory flags: 11 of 12 fields carry Validators.required — البيان is the
   * spec's optional free text. The server validator re-checks (the browser is not the control).
   */
  private createForm(): FormGroup {
    return this.fb.group({
      countryId: [null, Validators.required],            // الدولة
      departmentId: [null, Validators.required],         // الإدارة
      operationNumber: ['', [Validators.required, Validators.maxLength(50)]], // رقم العملية
      finYear: ['', [Validators.required, Validators.maxLength(10)]],         // السنة المالية
      paymentNumber: [null, Validators.required],        // رقم الدفعة (1–4)
      dateFrom: ['', Validators.required],               // من تاريخ
      dateTo: ['', Validators.required],                 // الى تاريخ
      amountOfPayment: [null, [Validators.required, Validators.min(0.01)]],   // مبلغ الدفعة
      statement: ['', Validators.maxLength(500)],        // البيان — optional
      beneficiariesNumber: [null, [Validators.required, Validators.min(1)]],  // عدد المستفيدين
      transactionNumber: ['', [Validators.required, Validators.maxLength(50)]], // رقم المعاملة
      transactionDate: ['', Validators.required]         // تاريخ المعاملة
    });
  }

  /**
   * Load the §22.S.2 catalogue lookups from their real endpoints. A failed load is never
   * silent: the inline banner offers a retry (UC-TRF-05 — a mandatory dropdown must not
   * sit empty with no explanation).
   */
  private loadCatalogues(): void {
    this.catalogueFailed = false;

    this.lookupService.getCountries({ isActive: true, page: 1, pageSize: 1000 })
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: result => {
          this.countries = result.items || [];
          this.ensureStoredLookupsVisible();
        },
        error: () => this.handleCatalogueError()
      });

    this.lookupService.getDepartments({ isActive: true, page: 1, pageSize: 1000 })
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: result => {
          this.departments = result.items || [];
          this.ensureStoredLookupsVisible();
        },
        error: () => this.handleCatalogueError()
      });
  }

  /** User-visible retry of a failed catalogue load */
  retryCatalogues(): void {
    this.loadCatalogues();
  }

  /**
   * UC-TRF-03/04: fetch the record for view (read-only) or edit (editable) mode. 404
   * (absent, soft-deleted, or outside the caller's country scope) shows the not-found state.
   */
  private loadTransfer(id: string): void {
    this.loading = true;

    this.transferService.getTransferById(id)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: detail => {
          this.transfer = detail;
          this.transferForm.patchValue({
            countryId: detail.countryId ?? null,
            departmentId: detail.departmentId ?? null,
            operationNumber: detail.operationNumber,
            finYear: detail.finYear,
            paymentNumber: detail.paymentNumber,
            dateFrom: detail.dateFrom.split('T')[0],
            dateTo: detail.dateTo.split('T')[0],
            amountOfPayment: detail.amountOfPayment,
            statement: detail.statement || '',
            beneficiariesNumber: detail.beneficiariesNumber,
            transactionNumber: detail.transactionNumber,
            transactionDate: detail.transactionDate.split('T')[0]
          });
          // A stored lookup that has since been deactivated is absent from the
          // active-only catalogue — add it back so the select shows the resolved name
          // instead of an empty control (17-3: display names, not blank selects)
          this.ensureStoredLookupsVisible();
          // View mode: every control read-only — the operation changes no stored data
          // (UC-TRF-03 AC 1). Edit mode keeps the form editable.
          if (this.isViewMode) {
            this.transferForm.disable();
          }
          this.loading = false;
        },
        error: (httpError) => {
          this.loading = false;
          if (httpError?.status === 404) {
            this.notFound = true;
          } else {
            // The record was never shown — keep the form inert so it cannot be PUT over
            // the stored data from memory (blind-overwrite guard)
            this.loadFailed = true;
            this.transferForm.disable();
            this.notification.error(this.translate.instant('hqTransfers.loadFailed'));
          }
        }
      });
  }

  /**
   * 17-3: the stored record's country/department must render by name even when the
   * lookup row was deactivated after the transfer was filed (the catalogue asks for
   * active-only rows). If the stored id is missing from the loaded options, append a
   * synthetic option carrying the server-resolved name. Runs after BOTH loads — they
   * race, and whichever lands second must not wipe the other's option.
   */
  private ensureStoredLookupsVisible(): void {
    const detail = this.transfer;
    if (!detail) {
      return;
    }

    if (detail.countryId
        && detail.countryName
        && !this.countries.some(c => c.id === detail.countryId)) {
      this.countries = [...this.countries, { id: detail.countryId, name: detail.countryName } as CountryDto];
    }

    if (detail.departmentId
        && detail.departmentName
        && !this.departments.some(d => d.id === detail.departmentId)) {
      this.departments = [...this.departments, { id: detail.departmentId, name: detail.departmentName } as DepartmentDto];
    }
  }

  /**
   * A failed catalogue load must not pass silently — the actor would face an empty
   * dropdown with no explanation. Toast + inline retry banner.
   */
  private handleCatalogueError(): void {
    this.catalogueFailed = true;
    this.notification.error(this.translate.instant('hqTransfers.catalogueLoadFailed'));
  }

  /**
   * Submit — POST /api/HqTransfers (create) or PUT /api/HqTransfers with id in the body
   * (edit); the server pins/validates the country against the caller's claim. View mode
   * never reaches here (no حفظ button, form disabled).
   */
  onSubmit(): void {
    // Edit mode without a loaded record = the load failed — refuse rather than PUT an
    // empty form over the stored transfer (blind-overwrite guard)
    if (this.isEditMode && !this.transfer) {
      this.notification.error(this.translate.instant('hqTransfers.loadFailed'));
      return;
    }

    if (this.transferForm.invalid) {
      this.markFormGroupTouched(this.transferForm);
      this.notification.error(this.translate.instant('hqTransfers.fixValidationErrors'));
      return;
    }

    // UC-TRF-06 client pre-check over a KNOWN ceiling — refuses before the POST; the server
    // re-checks authoritatively on both create and update
    const amount = Number(this.transferForm.value.amountOfPayment);
    if (this.maxAmountKnown && this.maxAmount !== null && amount > this.maxAmount) {
      const control = this.transferForm.get('amountOfPayment');
      control?.setErrors({ server: true, serverMessage: this.translate.instant('hqTransfers.amountExceedsLimit') });
      control?.markAsTouched();
      this.notification.error(this.translate.instant('hqTransfers.amountExceedsLimit'));
      return;
    }

    this.saving = true;
    const formValue = this.transferForm.value;

    const baseRequest: CreateHqTransferRequest = {
      countryId: formValue.countryId,
      departmentId: formValue.departmentId,
      operationNumber: formValue.operationNumber,
      finYear: formValue.finYear,
      paymentNumber: formValue.paymentNumber,
      dateFrom: formValue.dateFrom,
      dateTo: formValue.dateTo,
      amountOfPayment: formValue.amountOfPayment,
      statement: formValue.statement || null,
      beneficiariesNumber: formValue.beneficiariesNumber,
      transactionNumber: formValue.transactionNumber,
      transactionDate: formValue.transactionDate
    };

    if (this.isEditMode && this.transferId) {
      // UC-TRF-04 — id rides in the body (board contract)
      const request: UpdateHqTransferRequest = { ...baseRequest, id: this.transferId };

      this.transferService.updateTransfer(request)
        .pipe(takeUntil(this.destroy$))
        .subscribe({
          next: () => {
            this.notification.success(this.translate.instant('hqTransfers.transferUpdated'));
            this.saving = false;
            this.router.navigate(['/hq-transfers']);
          },
          error: (httpError) => this.handleSaveError(httpError, 'hqTransfers.updateFailed')
        });
    } else {
      this.transferService.createTransfer(baseRequest)
        .pipe(takeUntil(this.destroy$))
        .subscribe({
          next: () => {
            this.notification.success(this.translate.instant('hqTransfers.transferCreated'));
            this.saving = false;
            this.router.navigate(['/hq-transfers']);
          },
          error: (httpError) => this.handleSaveError(httpError, 'hqTransfers.createFailed')
        });
    }
  }

  /**
   * Surface the server's field→messages map (FluentValidation via the controller's
   * BadRequest(new { message, errors }) shape): flag each named control and toast
   * the summary message. Server keys are PascalCase DTO names — controls are camelCase.
   */
  private handleSaveError(httpError: any, fallbackKey: string): void {
    this.saving = false;

    const errors = httpError?.error?.errors;
    if (errors && typeof errors === 'object') {
      for (const [field, messages] of Object.entries<any>(errors)) {
        const controlName = field.charAt(0).toLowerCase() + field.slice(1);
        const control = this.transferForm.get(controlName);
        const message = Array.isArray(messages) ? messages.join(' · ') : String(messages);

        if (control) {
          control.setErrors({ server: true, serverMessage: message });
          control.markAsTouched();
        }
      }
      this.notification.error(httpError?.error?.message || this.translate.instant(fallbackKey));
    } else {
      this.notification.error(httpError?.error?.message || this.translate.instant(fallbackKey));
    }
  }

  /**
   * Cancel / رجوع — back to the register
   */
  onCancel(): void {
    this.router.navigate(['/hq-transfers']);
  }

  /**
   * Check if field is invalid
   */
  isFieldInvalid(fieldName: string): boolean {
    const field = this.transferForm.get(fieldName);
    return field ? field.invalid && (field.dirty || field.touched) : false;
  }

  /**
   * Get field error message — client required/min/max or the server's own text
   */
  getFieldError(fieldName: string): string {
    const field = this.transferForm.get(fieldName);
    if (!field || !field.errors) return '';

    if (field.errors['server']) {
      return field.errors['serverMessage'] || this.translate.instant('validation.invalid');
    }
    if (field.errors['required']) {
      return this.translate.instant('validation.required');
    }
    if (field.errors['min']) {
      return this.translate.instant('validation.minValue') + ' ' + field.errors['min'].min;
    }
    if (field.errors['maxlength']) {
      return this.translate.instant('validation.maxLength').replace('{{maxLength}}', field.errors['maxlength'].requiredLength);
    }

    return '';
  }

  /**
   * Mark all fields as touched
   */
  private markFormGroupTouched(formGroup: FormGroup): void {
    Object.keys(formGroup.controls).forEach(key => {
      const control = formGroup.get(key);
      control?.markAsTouched();

      if (control instanceof FormGroup) {
        this.markFormGroupTouched(control);
      }
    });
  }

  // ========== TrackBys ==========

  trackCountry(index: number, country: CountryDto): number {
    return country.id;
  }

  trackDepartment(index: number, department: DepartmentDto): number {
    return department.id;
  }

  trackPaymentNumber(index: number, paymentNumber: number): number {
    return paymentNumber;
  }
}
