import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup, Validators, FormControl, AbstractControl, AsyncValidatorFn, ValidationErrors } from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { HttpErrorResponse } from '@angular/common/http';
import { CharityService } from '../services/charity.service';
import { LookupManagementService } from '../../lookup-management/services/lookup-management.service';
import { CountryDto, RegionDto, CenterDto } from '../../lookup-management/models/lookup.model';
import { NotificationService } from '../../../core/services/notification.service';
import { PageHeaderComponent } from '../../../shared/components/page-header/page-header.component';
import { BreadcrumbComponent, BreadcrumbItem, AttachmentInputComponent } from '../../../shared/components';
import { TranslateModule, TranslateService, LangChangeEvent } from '@ngx-translate/core';
import { CreateCharityDto, UpdateCharityDto, CharityDto, AttachmentDto } from '../models/charity.model';
import { SharedModule, AttachmentFileType } from '../../../shared/shared.module';
import { Observable, Subscription, of, timer } from 'rxjs';
import { catchError, first, map, switchMap } from 'rxjs/operators';

declare var $: any;

@Component({
  selector: 'app-charity-form',
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
  templateUrl: './charity-form.component.html',
  styleUrls: ['./charity-form.component.scss']
})
export class CharityFormComponent implements OnInit, OnDestroy {
  private langChangeSubscription?: Subscription;
  /** Held so it can be torn down; see the PENDING branch of onSubmit(). */
  private pendingValidationSubscription?: Subscription;
  /** Guards against a second click registering a second statusChanges subscription. */
  private awaitingValidation = false;
  charityForm: FormGroup;
  isEditMode = false;
  charityId: string | null = null;
  loading = false;
  saving = false;

  // Lookup data
  allCountries: CountryDto[] = [];
  allRegions: RegionDto[] = [];
  allCenters: CenterDto[] = [];
  filteredRegions: RegionDto[] = [];
  filteredCenters: CenterDto[] = [];
  loadingLookups = false;

  // Generated credentials
  generatedCredentials: {
    username: string;
    password: string;
    email: string;
  } | null = null;

  // NGO Types - will be populated from translations (for dropdown component)
  ngoTypes: string[] = [];
  ngoTypeOptions: Array<{ id: string; name: string }> = [];

  // Bank options for dropdown - loaded from API (id can be string or number)
  bankOptions: Array<{ id: string | number; name: string }> = [];

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
    { label: 'charities.title', url: '/charities' },
    { label: 'charities.addCharity' }
  ];

  constructor(
    private fb: FormBuilder,
    private router: Router,
    private route: ActivatedRoute,
    private charityService: CharityService,
    private lookupService: LookupManagementService,
    private notification: NotificationService,
    private translate: TranslateService
  ) {
    this.charityForm = this.createForm();
  }

  ngOnInit(): void {
    this.updateBreadcrumbs();
    this.loadNgoTypes();
    this.loadLookupData();

    // Initialize user account validators based on initial createUserAccount value
    this.onCreateUserAccountChange();

    // Subscribe to language changes to update NGO types
    this.langChangeSubscription = this.translate.onLangChange.subscribe((event: LangChangeEvent) => {
      this.loadNgoTypes();
    });

    // Add value listener for ngoType to log and convert empty string to null
    this.charityForm.get('ngoType')?.valueChanges.subscribe(value => {
      console.log('[CharityForm] ngoType valueChanges:', value);
      if (value === '') {
        this.charityForm.get('ngoType')?.setValue(null, { emitEvent: false });
        console.log('[CharityForm] Converted empty ngoType string to null');
      }
    });

    if (this.isEditMode && this.charityId) {
      this.loadCharity(this.charityId);
    }
  }

  ngOnDestroy(): void {
    // Clean up language change subscription
    if (this.langChangeSubscription) {
      this.langChangeSubscription.unsubscribe();
    }

    // A name check still in flight would otherwise re-enter onSubmit() after the component is
    // gone, saving a record and navigating from a destroyed view.
    if (this.pendingValidationSubscription) {
      this.pendingValidationSubscription.unsubscribe();
    }
  }

  private updateBreadcrumbs(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.isEditMode = true;
      this.charityId = id;
      this.breadcrumbs = [
        { label: 'common.home', url: '/dashboard' },
        { label: 'charities.title', url: '/charities' },
        { label: 'charities.editCharity' }
      ];
    } else {
      this.breadcrumbs = [
        { label: 'common.home', url: '/dashboard' },
        { label: 'charities.title', url: '/charities' },
        { label: 'charities.addCharity' }
      ];
    }
  }

  private loadNgoTypes(): void {
    // Get current selected value to preserve it after reload
    const currentNgoType = this.charityForm.get('ngoType')?.value;

    const types = [
      { id: 'charity', name: this.translate.instant('charities.ngoTypeCharityOrganization') },
      { id: 'ngo', name: this.translate.instant('charities.ngoTypeNGO') },
      { id: 'nonProfit', name: this.translate.instant('charities.ngoTypeNonProfit') },
      { id: 'religious', name: this.translate.instant('charities.ngoTypeReligious') },
      { id: 'community', name: this.translate.instant('charities.ngoTypeCommunity') }
    ];

    this.ngoTypeOptions = types;
    this.ngoTypes = types.map(t => t.name);

    // Restore the previously selected value by ID (not by translated text)
    if (currentNgoType) {
      const selectedOption = types.find(t => t.id === currentNgoType);
      if (selectedOption) {
        this.charityForm.patchValue({ ngoType: selectedOption.id });
      }
    }
  }

  /**
   * Async validator for the charity name (UC-CHR-02).
   *
   * Reports `nameTaken` when another charity already holds the name, so the operator finds out
   * while typing rather than by losing a completed form to a refused save.
   *
   * Notes on the operators:
   * - `debounceTime` keeps the check off every keystroke.
   * - `switchMap` cancels a request that a newer keystroke has superseded, so a slow earlier reply
   *   cannot overwrite a newer one.
   * - `first()` completes the observable; an async validator that never completes leaves the
   *   control stuck in `pending` forever.
   * - A failed request resolves to `null` (valid). A name check is a convenience, and a network
   *   blip must not block the save — the server re-checks uniqueness on write regardless.
   */
  private charityNameAvailabilityValidator(): AsyncValidatorFn {
    return (control: AbstractControl): Observable<ValidationErrors | null> => {
      const name = (control.value ?? '').trim();

      if (name.length < 3) {
        return of(null);
      }

      return timer(400).pipe(
        switchMap(() => this.charityService.checkNameAvailability(name, this.charityId ?? undefined)),
        map(result => {
          // The server echoes the name it actually checked. If it does not match what we asked
          // about, the answer is about a different string — a stale reply, or a value mangled in
          // transit — and must not be used to mark this control invalid.
          if (result.name !== name) {
            return null;
          }
          return result.isAvailable ? null : { nameTaken: true };
        }),
        catchError((error: HttpErrorResponse) => {
          // A refused request is not a verdict on the name. 401 is handled by the interceptor;
          // for anything else keep the control valid so a transient failure cannot block the save.
          // The server re-checks uniqueness on write, so the worst case is the save is refused
          // with a message rather than the field being flagged early.
          console.warn(
            `Charity name availability check failed (${error.status}); treating the name as unverified.`
          );
          return of(null);
        }),
        first()
      );
    };
  }

  private createForm(): FormGroup {
    return this.fb.group({
      // Basic Information
      code: ['', Validators.required],
      name: [
        '',
        [Validators.required, Validators.minLength(3), Validators.maxLength(200)],
        [this.charityNameAvailabilityValidator()]
      ],
      // nameAr: ['', Validators.required],
      // nameEn: ['', Validators.required],
      ngoType: [null, Validators.required],

      // Contact Information
      address: ['', Validators.required],
      streetName: ['', Validators.required],
      village: ['', Validators.required],
      //city: ['', Validators.required],
      postalCode: ['', Validators.required],
      mailBox: ['', Validators.required],
      countryId: [null, Validators.required],

      // Phone/Contact
      phone: ['', [Validators.required, Validators.pattern(/^[+]?[\d\s\-()]+$/)]],
      phone2: ['', Validators.required],
      homePhone: ['', Validators.required],
      fax: ['', Validators.required],
      email: ['', [Validators.required, Validators.email]],

      // Location
      regionId: [null, Validators.required],
      centerId: [null, Validators.required],
      ngoMapLocation: ['', Validators.required],

      // Banking
      bankId: [null, Validators.required],
      bankAccount: ['', Validators.required],
      iban: ['', [Validators.required, Validators.pattern(/^[A-Z]{2}[0-9]{2}[A-Z0-9]{11,30}$/)]],

      // Management
      bossName: ['', Validators.required],
      bossJobName: ['', Validators.required],
      bossPhone1: ['', Validators.required],
      bossPhone2: ['', Validators.required],
      responsibleJobName: ['', Validators.required],
      responsiblePhone1: ['', Validators.required],
      responsiblePhone2: ['', Validators.required],

      // Settings
      icon_Attach: [[]],
      receivingDonations: [true],
      notes: ['', Validators.required],

      // User Account
      createUserAccount: [true],
      username: ['', [Validators.email]],
      password: ['']
    });
  }

  // Icon attachment handling
  icon_Attach_List: AttachmentDto[] = [];

  onIconAttachmentChange(attachments: AttachmentDto[]): void {
    this.icon_Attach_List = attachments;
    console.log('Icon attachments changed:', attachments);
  }

  attachmentFileType = AttachmentFileType;

  private loadLookupData(): void {
    this.loadingLookups = true;

    // Load Countries
    this.lookupService.getCountries({ isActive: true }).subscribe({
      next: (response) => {
        this.allCountries = response.items || [];
        this.loadingLookups = false;
      },
      error: () => {
        this.loadingLookups = false;
      }
    });

    // Load Banks from API. On failure the dropdown stays empty (8-11 AC 6 empty-option
    // posture) — a fake fallback list would let a required bankId bind to non-existent banks.
    this.lookupService.getBanks({ isActive: true }).subscribe({
      next: (response) => {
        this.bankOptions = response.items || [];
      },
      error: () => {
        this.bankOptions = [];
      }
    });
  }

  private loadCharity(id: string): void {
    this.loading = true;
    this.charityService.getCharity(id).subscribe({
      next: (charity: CharityDto) => {
        this.patchForm(charity);
        this.loading = false;
      },
      error: (error: any) => {
        console.error('Error loading charity:', error);
        this.notification.error(this.translate.instant('charities.loadCharityFailed'));
        this.loading = false;
      }
    });
  }

  private patchForm(charity: CharityDto): void {
    // Map ngoType to our fixed ID system if stored as translated text
    let mappedNgoType = charity.ngoType;
    if (charity.ngoType) {
      // Check if the stored value matches any of our translated texts (for backward compatibility)
      const allTranslations = [
        { en: 'Charity Organization', ar: 'منظمة خيرية', id: 'charity' },
        { en: 'NGO', ar: 'منظمة غير حكومية', id: 'ngo' },
        { en: 'Non-Profit Organization', ar: 'منظمة غير ربحية', id: 'nonProfit' },
        { en: 'Religious Organization', ar: 'منظمة دينية', id: 'religious' },
        { en: 'Community Organization', ar: 'منظمة مجتمعية', id: 'community' }
      ];
      const mapping = allTranslations.find(t => t.en === charity.ngoType || t.ar === charity.ngoType);
      if (mapping) {
        mappedNgoType = mapping.id;
      }
    }

    this.charityForm.patchValue({
      code: charity.code,
      name: charity.name,
      // nameAr: charity.nameAr,
      // nameEn: charity.nameEn,
      ngoType: mappedNgoType,
      address: charity.address,
      streetName: charity.streetName,
      village: charity.village,
      city: charity.city,
      postalCode: charity.postalCode,
      mailBox: charity.mailBox,
      countryId: charity.countryId,
      phone: charity.phone,
      phone2: charity.phone2,
      homePhone: charity.homePhone,
      fax: charity.fax,
      email: charity.email,
      regionId: charity.regionId,
      centerId: charity.centerId,
      ngoMapLocation: charity.ngoMapLocation,
      bankId: charity.bankId,
      bankAccount: charity.bankAccount,
      iban: charity.iban,
      bossName: charity.bossName,
      bossJobName: charity.bossJobName,
      bossPhone1: charity.bossPhone1,
      bossPhone2: charity.bossPhone2,
      responsibleJobName: charity.responsibleJobName,
      responsiblePhone1: charity.responsiblePhone1,
      responsiblePhone2: charity.responsiblePhone2,
      icon_Attach: charity.icon_Attach || [],
      receivingDonations: charity.receivingDonations,
      notes: charity.notes,
      // User account fields
      createUserAccount: !!charity.userId, // true if charity has a user account
      username: charity.username || '',
      password: '' // Don't display existing password for security
    });

    // Load icon attachments
    if (charity.icon_Attach && charity.icon_Attach.length > 0) {
      this.icon_Attach_List = [...charity.icon_Attach];
    }

    // Load regions and centers via API based on country and region
    if (charity.countryId) {
      this.loadRegionsByCountry(charity.countryId);
    }
    if (charity.regionId) {
      this.loadCentersByRegion(charity.regionId);
    }
  }

  onCountryChange(): void {
    debugger;
    const countryId = this.charityForm.get('countryId')?.value;
    this.loadRegionsByCountry(countryId);
    this.charityForm.patchValue({ regionId: null, centerId: null });
    this.filteredCenters = [];
  }

  onRegionChange(): void {
    debugger;
    const regionId = this.charityForm.get('regionId')?.value;
    this.loadCentersByRegion(regionId);
    this.charityForm.patchValue({ centerId: null });
  }

  // Drop-down component event handlers
  onCountryDropDownChanged(value: any): void {
    // Fix: Check for null/undefined explicitly, not falsy values (0 is valid!)
    if (value && (value.id !== undefined && value.id !== null)) {
      this.loadRegionsByCountry(value.id);
      this.charityForm.patchValue({ regionId: null, centerId: null });
      this.filteredCenters = [];
    } else {
      // Clear when country is cleared
      this.filteredRegions = [];
      this.filteredCenters = [];
    }
  }

  onRegionDropDownChanged(value: any): void {
    // Fix: Check for null/undefined explicitly, not falsy values (0 is valid!)
    if (value && (value.id !== undefined && value.id !== null)) {
      this.loadCentersByRegion(value.id);
      this.charityForm.patchValue({ centerId: null });
    } else {
      // Clear when region is cleared
      this.filteredCenters = [];
    }
  }

  onBankDropDownChanged(value: any): void {
    const bankIdControl = this.charityForm.get('bankId');
    if (value && value.id !== undefined && value.id !== null) {
      // Ensure proper type conversion (number vs string)
      const bankId = typeof value.id === 'string' ? parseInt(value.id, 10) : value.id;
      bankIdControl?.setValue(bankId, { emitEvent: true });
      bankIdControl?.markAsDirty();
      bankIdControl?.markAsTouched();
      bankIdControl?.updateValueAndValidity();
    } else {
      bankIdControl?.setValue(null, { emitEvent: true });
      bankIdControl?.updateValueAndValidity();
    }
  }

  onNgoTypeDropDownChanged(value: any): void {
    console.log('[CharityForm] onNgoTypeDropDownChanged called with:', value);
    const ngoTypeControl = this.charityForm.get('ngoType');
    if (value && value.id !== undefined && value.id !== null && value.id !== '') {
      // Set the value and ensure validation is updated
      ngoTypeControl?.setValue(value.id, { emitEvent: true });
      ngoTypeControl?.markAsDirty();
      ngoTypeControl?.markAsTouched();
      ngoTypeControl?.updateValueAndValidity({ onlySelf: false, emitEvent: true });
      console.log('[CharityForm] ngoType set to:', value.id, 'valid:', ngoTypeControl?.valid, 'errors:', ngoTypeControl?.errors);
    } else {
      ngoTypeControl?.setValue(null, { emitEvent: true });
      ngoTypeControl?.updateValueAndValidity({ onlySelf: false, emitEvent: true });
      console.log('[CharityForm] ngoType cleared');
    }
  }

  // Load Regions by Country ID from API
  private loadRegionsByCountry(countryId: any): void {
    debugger;
    console.log('[CharityForm] loadRegionsByCountry called with countryId:', countryId);
    debugger;
    if (!countryId) {
      this.filteredRegions = [];
      return;
    }

    // Trim whitespace and convert to number to ensure clean URL
    const cleanCountryId = typeof countryId === 'string' ? parseInt(countryId.trim(), 10) : countryId;
    console.log('[CharityForm] Clean countryId:', cleanCountryId);

    this.lookupService.getRegionsByCountry(cleanCountryId).subscribe({
      next: (regions) => {
        console.log('[CharityForm] Regions loaded:', regions);
        debugger;
        this.filteredRegions = regions || [];
      },
      error: (error) => {
        console.error('[CharityForm] Error loading regions:', error);
        debugger;
        this.filteredRegions = [];
      }
    });
  }

  // Load Centers by Region ID from API
  private loadCentersByRegion(regionId: any): void {
    console.log('[CharityForm] loadCentersByRegion called with regionId:', regionId);
    if (!regionId) {
      this.filteredCenters = [];
      return;
    }

    // Trim whitespace and convert to number to ensure clean URL
    const cleanRegionId = typeof regionId === 'string' ? parseInt(regionId.trim(), 10) : regionId;
    console.log('[CharityForm] Clean regionId:', cleanRegionId);

    this.lookupService.getCentersByRegion(cleanRegionId).subscribe({
      next: (centers) => {
        console.log('[CharityForm] Centers loaded:', centers);
        this.filteredCenters = centers || [];
      },
      error: () => {
        this.filteredCenters = [];
      }
    });
  }

  onCreateUserAccountChange(): void {
    const createUserAccount = this.charityForm.get('createUserAccount')?.value;
    const usernameControl = this.charityForm.get('username');
    const passwordControl = this.charityForm.get('password');

    // Check if this is an existing charity with a user account
    const hasExistingUserAccount = this.isEditMode && this.charityForm.get('username')?.value;

    if (createUserAccount) {
      // Set validators
      usernameControl?.setValidators([Validators.required, Validators.email]);
      // Only require password for new accounts or when user wants to change it
      if (!hasExistingUserAccount) {
        passwordControl?.setValidators([Validators.required, Validators.minLength(8)]);
      } else {
        passwordControl?.setValidators([Validators.minLength(8)]);
      }

      // Auto-generate username from email for new accounts
      const email = this.charityForm.get('email')?.value;
      if (email && !hasExistingUserAccount) {
        usernameControl?.setValue(email);
      }

      // Mark as touched so validation errors show immediately
      usernameControl?.markAsTouched();
      if (!hasExistingUserAccount) {
        passwordControl?.markAsTouched();
      }
    } else {
      // Clear validators and values
      usernameControl?.clearValidators();
      passwordControl?.clearValidators();
      usernameControl?.setValue('');
      passwordControl?.setValue('');

      // Mark as untouched and pristine to hide validation errors
      usernameControl?.markAsUntouched();
      usernameControl?.markAsPristine();
      passwordControl?.markAsUntouched();
      passwordControl?.markAsPristine();
    }

    // Update validity
    usernameControl?.updateValueAndValidity();
    passwordControl?.updateValueAndValidity();
  }

  onEmailChange(): void {
    const email = this.charityForm.get('email')?.value;
    const createUserAccount = this.charityForm.get('createUserAccount')?.value;

    if (createUserAccount && email) {
      this.charityForm.patchValue({ username: email });
    }
  }

  generatePassword(): string {
    const length = 12;
    const charset = 'abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789!@#$%^&*';
    let password = '';
    for (let i = 0; i < length; i++) {
      password += charset.charAt(Math.floor(Math.random() * charset.length));
    }
    return password;
  }

  onSubmit(): void {
    // Re-entrancy guard: without it a second click during the PENDING window queues a second
    // submit, producing duplicate charity rows and duplicate user accounts.
    if (this.saving) {
      return;
    }

    debugger;
    // Debug logging
    console.log('[CharityForm] onSubmit called');
    console.log('[CharityForm] Form valid:', this.charityForm.valid);
    console.log('[CharityForm] Form errors:', this.charityForm.errors);
    const ngoTypeControl = this.charityForm.get('ngoType');
    console.log('[CharityForm] ngoType value:', ngoTypeControl?.value);
    console.log('[CharityForm] ngoType valid:', ngoTypeControl?.valid);
    console.log('[CharityForm] ngoType errors:', ngoTypeControl?.errors);
    console.log('[CharityForm] ngoType dirty:', ngoTypeControl?.dirty);
    console.log('[CharityForm] ngoType touched:', ngoTypeControl?.touched);
    console.log('[CharityForm] ngoTypeOptions:', this.ngoTypeOptions);
debugger;
    // Convert empty string to null for ngoType (workaround for Select2 issue)
    if (ngoTypeControl?.value === '' || ngoTypeControl?.value === null) {
      // Try to get the value directly from Select2
      debugger;
      const selectElement = $('select[name="ngoType"]');
      const select2Value = selectElement.select2('val');
      console.log('[CharityForm] Select2 value:', select2Value);
      if (select2Value && select2Value !== '') {
        ngoTypeControl?.setValue(select2Value, { emitEvent: true });
        ngoTypeControl?.markAsDirty();
        ngoTypeControl?.markAsTouched();
        console.log('[CharityForm] Set ngoType from Select2:', select2Value);
      } else if (ngoTypeControl?.value === '') {
        ngoTypeControl.setValue(null, { emitEvent: false });
        console.log('[CharityForm] Converted empty ngoType to null');
      }
    }

    // A control whose async validator is still running is PENDING, not INVALID, so the guard
    // below would let the save through and bypass the name check entirely. Wait for the pending
    // validators to settle, then re-enter.
    if (this.charityForm.pending) {
      // `saving` is set here rather than only below, because it drives the button's disabled
      // state. Without it the button stays live while PENDING, and every extra click registers
      // another statusChanges subscription — all of which fire at once when validation settles,
      // producing one POST per click and duplicate charity rows.
      if (this.awaitingValidation) {
        return;
      }

      this.awaitingValidation = true;
      this.saving = true;

      // Stored and torn down in ngOnDestroy: otherwise navigating away mid-check still resolves
      // and re-enters onSubmit() on a destroyed component, issuing a save and a navigation.
      this.pendingValidationSubscription = this.charityForm.statusChanges
        .pipe(first(status => status !== 'PENDING'))
        .subscribe(() => {
          this.awaitingValidation = false;
          this.saving = false;
          this.onSubmit();
        });
      return;
    }

    if (this.charityForm.invalid) {
      this.markFormGroupTouched(this.charityForm);
      this.notification.error(this.translate.instant('charities.fixValidationErrors'));
      return;
    }

    this.saving = true;
debugger;
    if (this.isEditMode && this.charityId) {
      debugger;
      this.updateCharity();
    } else {
      this.createCharity();
    }
  }

  private createCharity(): void {
    const formValue = this.charityForm.value;

    const charity: CreateCharityDto = {
      code: formValue.code,
      name: formValue.name,
      // nameAr: formValue.nameAr,
      // nameEn: formValue.nameEn,
      ngoType: formValue.ngoType,
      address: formValue.address,
      streetName: formValue.streetName,
      village: formValue.village,
      // city: formValue.city,
      postalCode: formValue.postalCode,
      mailBox: formValue.mailBox,
      countryId: formValue.countryId,
      phone: formValue.phone,
      phone2: formValue.phone2,
      homePhone: formValue.homePhone,
      fax: formValue.fax,
      email: formValue.email,
      regionId: formValue.regionId,
      centerId: formValue.centerId,
      ngoMapLocation: formValue.ngoMapLocation,
      bankId: formValue.bankId,
      bankAccount: formValue.bankAccount,
      iban: formValue.iban,
      bossName: formValue.bossName,
      bossJobName: formValue.bossJobName,
      bossPhone1: formValue.bossPhone1,
      bossPhone2: formValue.bossPhone2,
      responsibleJobName: formValue.responsibleJobName,
      responsiblePhone1: formValue.responsiblePhone1,
      responsiblePhone2: formValue.responsiblePhone2,
      icon_Attach: this.icon_Attach_List,
      receivingDonations: formValue.receivingDonations,
      notes: formValue.notes,
      createUserAccount: formValue.createUserAccount,
      username: formValue.createUserAccount ? formValue.username : undefined,
      password: formValue.createUserAccount ? formValue.password : undefined
    };

    this.charityService.createCharity(charity).subscribe({
      next: (response: CharityDto) => {
        if (formValue.createUserAccount && response.password) {
          this.generatedCredentials = {
            username: formValue.username,
            password: response.password,
            email: formValue.email
          };
        }
        this.notification.success(this.translate.instant('charities.createSuccess'));
        this.saving = false;

        if (this.generatedCredentials) {
          // Show credentials dialog
          setTimeout(() => {
            this.router.navigate(['/charities', response.id]);
          }, 3000);
        } else {
          this.router.navigate(['/charities', response.id]);
        }
      },
      error: (error: any) => {
        console.error('Error creating charity:', error);

        // A 400 from the server-side validator carries which fields failed. Without this the
        // form shows only a generic toast and AC 3's "the offending field is flagged" is unmet
        // for anything the client-side rules do not catch — a duplicate name, most obviously.
        if (!this.applyServerValidationErrors(error)) {
          this.notification.error(this.translate.instant('charities.createFailed'));
        }

        this.saving = false;
      }
    });
  }

  /**
   * Maps a server validation response onto the form so the offending controls are flagged.
   *
   * @returns true when field errors were applied, so the caller can skip the generic message.
   */
  private applyServerValidationErrors(error: any): boolean {
    const fieldErrors: Record<string, string[]> | undefined = error?.error?.errors;

    if (!fieldErrors || Object.keys(fieldErrors).length === 0) {
      return false;
    }

    let applied = false;
    const unmatched: string[] = [];
    const controlNames = Object.keys(this.charityForm.controls);

    Object.keys(fieldErrors).forEach(property => {
      // Case-insensitive match against the real control names. Lowercasing only the first
      // character is not a valid inverse of .NET naming: "NGOType" becomes "nGOType" and
      // "IBAN" becomes "iBAN", neither of which is a control, so those errors were dropped.
      const controlName = controlNames.find(
        name => name.toLowerCase() === property.toLowerCase()
      );
      const control = controlName ? this.charityForm.get(controlName) : null;

      if (control) {
        // Join every message for the field. The server groups them precisely because one field
        // can break several rules at once; showing only the first makes the operator fix them
        // one submit at a time.
        control.setErrors({
          ...(control.errors ?? {}),
          server: fieldErrors[property].join(" ")
        });
        control.markAsTouched();
        applied = true;
      } else {
        unmatched.push(...fieldErrors[property]);
      }
    });

    // Errors with no matching control - model-level rules, or fields the form does not render -
    // must still be said, whether or not other errors matched. Keying this off `applied` meant
    // they vanished whenever any other field matched.
    if (unmatched.length > 0) {
      this.notification.error(unmatched.join(" "));
      return true;
    }

    return applied;
  }

  private updateCharity(): void {
    debugger;
    const formValue = this.charityForm.value;

    const charity: UpdateCharityDto = {
      code: formValue.code,
      name: formValue.name,
      // nameAr: formValue.nameAr,
      // nameEn: formValue.nameEn,
      ngoType: formValue.ngoType,
      address: formValue.address,
      streetName: formValue.streetName,
      village: formValue.village,
      // city: formValue.city,
      postalCode: formValue.postalCode,
      mailBox: formValue.mailBox,
      countryId: formValue.countryId,
      phone: formValue.phone,
      phone2: formValue.phone2,
      homePhone: formValue.homePhone,
      fax: formValue.fax,
      email: formValue.email,
      regionId: formValue.regionId,
      centerId: formValue.centerId,
      ngoMapLocation: formValue.ngoMapLocation,
      bankId: formValue.bankId,
      bankAccount: formValue.bankAccount,
      iban: formValue.iban,
      bossName: formValue.bossName,
      bossJobName: formValue.bossJobName,
      bossPhone1: formValue.bossPhone1,
      bossPhone2: formValue.bossPhone2,
      responsibleJobName: formValue.responsibleJobName,
      responsiblePhone1: formValue.responsiblePhone1,
      responsiblePhone2: formValue.responsiblePhone2,
      icon_Attach: this.icon_Attach_List,
      receivingDonations: formValue.receivingDonations,
      notes: formValue.notes,
      createUserAccount: formValue.createUserAccount,
      username: formValue.createUserAccount ? formValue.username : undefined,
      password: formValue.createUserAccount ? formValue.password : undefined
    };

    this.charityService.updateCharity(this.charityId!, charity).subscribe({
      next: (response: CharityDto) => {
        debugger;
        if (formValue.createUserAccount && response.password) {
          this.generatedCredentials = {
            username: formValue.username,
            password: response.password,
            email: formValue.email
          };
        }
        this.notification.success(this.translate.instant('charities.updateSuccess'));
        this.saving = false;

        if (this.generatedCredentials) {
          setTimeout(() => {
            this.router.navigate(['/charities', this.charityId]);
          }, 3000);
        } else {
          this.router.navigate(['/charities', this.charityId]);
        }
      },
      error: (error: any) => {
        debugger;
        console.error('Error updating charity:', error);
        this.notification.error(this.translate.instant('charities.updateFailed'));
        this.saving = false;
      }
    });
  }

  cancel(): void {
    if (this.isEditMode && this.charityId) {
      this.router.navigate(['/charities', this.charityId]);
    } else {
      this.router.navigate(['/charities']);
    }
  }

  isFieldValid(fieldName: string): boolean {
    const field = this.charityForm.get(fieldName);
    return field ? field.valid && (field.dirty || field.touched) : false;
  }

  isFieldInvalid(fieldName: string): boolean {
    const field = this.charityForm.get(fieldName);
    return field ? field.invalid && (field.dirty || field.touched) : false;
  }

  getErrorMessage(fieldName: string): string {
    const field = this.charityForm.get(fieldName);
    if (!field || !field.errors) return '';

    const fieldLabel = this.translate.instant(`charities.${fieldName}`);

    if (field.errors['required']) {
      return this.translate.instant('validation.required', { field: fieldLabel });
    }
    if (field.errors['email']) {
      return this.translate.instant('validation.email');
    }
    if (field.errors['pattern']) {
      return this.translate.instant('validation.pattern');
    }
    if (field.errors['minlength']) {
      return this.translate.instant('validation.minLength', { minLength: field.errors['minlength'].requiredLength });
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
}
