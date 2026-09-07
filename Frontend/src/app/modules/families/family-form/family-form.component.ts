import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup, Validators, FormControl, AbstractControl } from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { FamilyService } from '../services/family.service';
import { NotificationService } from '../../../core/services/notification.service';
import { PageHeaderComponent } from '../../../shared/components/page-header/page-header.component';
import { BreadcrumbComponent, BreadcrumbItem, AttachmentInputComponent } from '../../../shared/components';
import { TranslateModule, TranslateService, LangChangeEvent } from '@ngx-translate/core';
import {
  FamilyDto,
  CreateFamilyDto,
  UpdateFamilyDto,
  FatherDto,
  CreateFatherDto,
  MotherDto,
  CreateMotherDto,
  ProviderDto,
  CreateProviderDto,
  RelativeDto,
  CreateRelativeDto,
  AttachmentDto,
  PhoneCheckDto,
  PhoneCheckState
} from '../models/family.model';
import { SharedModule, AttachmentFileType } from '../../../shared/shared.module';
import { Subscription } from 'rxjs';
import { debounceTime, distinctUntilChanged } from 'rxjs/operators';
import {
  FatherFormComponent,
  MotherFormComponent,
  RelativesFormComponent,
  RelativesListComponent
} from '../components';

declare var $: any;

@Component({
  selector: 'app-family-form',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    PageHeaderComponent,
    BreadcrumbComponent,
    TranslateModule,
    SharedModule,
    AttachmentInputComponent,
    FatherFormComponent,
    MotherFormComponent,
    RelativesFormComponent,
    RelativesListComponent
  ],
  templateUrl: './family-form.component.html',
  styleUrls: ['./family-form.component.scss']
})
export class FamilyFormComponent implements OnInit, OnDestroy {
  private langChangeSubscription?: Subscription;
  familyForm: FormGroup;
  fatherForm: FormGroup;
  motherForm: FormGroup;
  providerForm: FormGroup;
  relativesForm: FormGroup;

  // UC-ORP-10 — duplicate-phone flags per holder. Edit mode only: the check spans every OTHER
  // family in scope and needs this family's id to exclude it, so it cannot run before the
  // family exists (recorded story limitation for create mode).
  phoneChecks: Record<string, PhoneCheckState> = {
    family: { status: 'idle' },
    father: { status: 'idle' },
    mother: { status: 'idle' },
    provider: { status: 'idle' }
  };
  private phoneCheckSubscriptions: Subscription[] = [];
  private nidCheckSubscriptions: Subscription[] = [];

  // Relatives array for managing multiple relatives
  relatives: CreateRelativeDto[] = [];

  isEditMode = false;
  familyId: string | null = null;
  /** The loaded family's charity — sent with the UC-SYS-12 check so HQ editors scope it
   *  (a charity claim still wins server-side; review P7, 2026-08-26). */
  private familyCharityId: string | null = null;
  loading = false;
  saving = false;

  // Related entities
  father?: FatherDto;
  mother?: MotherDto;
  provider?: ProviderDto;

  // Attachments
  familyAttachments: AttachmentDto[] = [];

  // Dropdown options - localized
  providerTypeOptions: Array<{ id: string; name: string }> = [];
  livingConditionOptions: Array<{ id: string; name: string }> = [];
  housingTypeOptions: Array<{ id: string; name: string }> = [];
  educationLevelOptions: Array<{ id: string; name: string }> = [];
  healthStatusOptions: Array<{ id: string; name: string }> = [];
  relationshipOptions: Array<{ id: string; name: string }> = [];

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
    { label: 'families.title', url: '/families' },
    { label: 'families.addFamily' }
  ];

  constructor(
    private fb: FormBuilder,
    private router: Router,
    private route: ActivatedRoute,
    private familyService: FamilyService,
    private notification: NotificationService,
    private translate: TranslateService
  ) {
    this.familyForm = this.createFamilyForm();
    this.fatherForm = this.createFatherForm();
    this.motherForm = this.createMotherForm();
    this.providerForm = this.createProviderForm();
    this.relativesForm = this.createRelativeForm();
  }

  ngOnInit(): void {
    this.updateBreadcrumbs();
    this.initializeDropdownOptions();

    this.langChangeSubscription = this.translate.onLangChange.subscribe((event: LangChangeEvent) => {
      this.initializeDropdownOptions();
    });

    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.isEditMode = true;
      this.familyId = id;
      this.loadFamily(id);
    }
  }

  ngOnDestroy(): void {
    if (this.langChangeSubscription) {
      this.langChangeSubscription.unsubscribe();
    }
    this.phoneCheckSubscriptions.forEach(s => s.unsubscribe());
    this.phoneCheckSubscriptions = [];
    this.nidCheckSubscriptions.forEach(s => s.unsubscribe());
    this.nidCheckSubscriptions = [];
  }

  private updateBreadcrumbs(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.isEditMode = true;
      this.familyId = id;
      this.breadcrumbs = [
        { label: 'common.home', url: '/dashboard' },
        { label: 'families.title', url: '/families' },
        { label: 'families.editFamily' }
      ];
    } else {
      this.breadcrumbs = [
        { label: 'common.home', url: '/dashboard' },
        { label: 'families.title', url: '/families' },
        { label: 'families.addFamily' }
      ];
    }
  }

  private initializeDropdownOptions(): void {
    this.providerTypeOptions = [
      { id: 'father', name: this.translate.instant('families.providerTypeFather') },
      { id: 'mother', name: this.translate.instant('families.providerTypeMother') },
      { id: 'other', name: this.translate.instant('families.providerTypeOther') }
    ];

    this.livingConditionOptions = [
      { id: 'good', name: this.translate.instant('families.livingConditionGood') },
      { id: 'fair', name: this.translate.instant('families.livingConditionFair') },
      { id: 'poor', name: this.translate.instant('families.livingConditionPoor') }
    ];

    this.housingTypeOptions = [
      { id: 'owned', name: this.translate.instant('families.housingTypeOwned') },
      { id: 'rented', name: this.translate.instant('families.housingTypeRented') },
      { id: 'shared', name: this.translate.instant('families.housingTypeShared') },
      { id: 'other', name: this.translate.instant('families.housingTypeOther') }
    ];

    this.educationLevelOptions = [
      { id: 'illiterate', name: this.translate.instant('families.educationLevelIlliterate') },
      { id: 'primary', name: this.translate.instant('families.educationLevelPrimary') },
      { id: 'middle', name: this.translate.instant('families.educationLevelMiddle') },
      { id: 'secondary', name: this.translate.instant('families.educationLevelSecondary') },
      { id: 'university', name: this.translate.instant('families.educationLevelUniversity') }
    ];

    this.healthStatusOptions = [
      { id: 'excellent', name: this.translate.instant('families.healthStatusExcellent') },
      { id: 'good', name: this.translate.instant('families.healthStatusGood') },
      { id: 'fair', name: this.translate.instant('families.healthStatusFair') },
      { id: 'poor', name: this.translate.instant('families.healthStatusPoor') }
    ];

    this.relationshipOptions = [
      { id: 'grandfather', name: this.translate.instant('families.relationshipGrandfather') },
      { id: 'grandmother', name: this.translate.instant('families.relationshipGrandmother') },
      { id: 'uncle', name: this.translate.instant('families.relationshipUncle') },
      { id: 'aunt', name: this.translate.instant('families.relationshipAunt') },
      { id: 'brother', name: this.translate.instant('families.relationshipBrother') },
      { id: 'sister', name: this.translate.instant('families.relationshipSister') },
      { id: 'guardian', name: this.translate.instant('families.relationshipGuardian') },
      { id: 'other', name: this.translate.instant('families.relationshipOther') }
    ];
  }

  private createFamilyForm(): FormGroup {
    return this.fb.group({
      code: [''],
      address: ['', Validators.required],
      city: [''],
      village: [''],
      district: [''],
      phone: [''],
      livingCondition: [''],
      housingType: [''],
      providerType: ['father'],
      registrationDate: [new Date().toISOString().split('T')[0]],
      notes: ['']
    });
  }

  private createFatherForm(): FormGroup {
    return this.fb.group({
      fullName: ['', Validators.required],
      nationalId: ['', Validators.required],
      passportNumber: [''],
      dateOfBirth: ['', Validators.required],
      placeOfBirth: [''],
      educationLevel: [''],
      job: [''],
      monthlyIncome: [null],
      healthStatus: [''],
      phone: [''],
      isAlive: [true],
      isProvider: [false],
      deathDate: [''],
      notes: ['']
    });
  }

  private createMotherForm(): FormGroup {
    return this.fb.group({
      fullName: ['', Validators.required],
      nationalId: ['', Validators.required],
      passportNumber: [''],
      dateOfBirth: ['', Validators.required],
      placeOfBirth: [''],
      educationLevel: [''],
      job: [''],
      monthlyIncome: [null],
      healthStatus: [''],
      phone: [''],
      isAlive: [true],
      isProvider: [false],
      deathDate: [''],
      notes: ['']
    });
  }

  private createProviderForm(): FormGroup {
    return this.fb.group({
      fullName: [''],
      relationship: [''],
      nationalId: [''],
      passportNumber: [''],
      phone: [''],
      address: [''],
      job: [''],
      monthlyIncome: [null],
      notes: ['']
    });
  }

  private createRelativeForm(): FormGroup {
    return this.fb.group({
      fullName: ['', Validators.required],
      relationshipType: ['', Validators.required],
      gender: ['', Validators.required],
      dateOfBirth: ['', Validators.required],
      placeOfBirth: [''],
      nationalId: [''],
      educationLevel: [''],
      job: [''],
      monthlyIncome: [null],
      healthStatus: [''],
      phone: [''],
      address: [''],
      isAlive: [true],
      isLivingWithFamily: [false],
      deathDate: [''],
      notes: ['']
    });
  }

  private loadFamily(id: string): void {
    this.loading = true;
    this.familyService.getFamily(id).subscribe({
      next: (family: FamilyDto) => {
        this.patchFamilyForm(family);
        this.familyCharityId = family.charityId ?? null;

        // Load related entities
        if (family.father) {
          this.father = family.father;
          this.patchFatherForm(family.father);
        }
        if (family.mother) {
          this.mother = family.mother;
          this.patchMotherForm(family.mother);
        }
        if (family.provider) {
          this.provider = family.provider;
          this.patchProviderForm(family.provider);
        }

        // Load attachments
        if (family.attachments) {
          this.familyAttachments = [...family.attachments];
        }

        // UC-ORP-10 — wire the duplicate-phone checks now that the family id is known
        this.setupPhoneDuplicateChecks();

        // UC-SYS-12 — same for the national-id uniqueness checks (edit mode only)
        this.setupNationalIdChecks();

        this.loading = false;
      },
      error: (error: any) => {
        console.error('Error loading family:', error);
        this.notification.error(this.translate.instant('families.loadFamilyFailed'));
        this.loading = false;
      }
    });
  }

  private patchFamilyForm(family: FamilyDto): void {
    this.familyForm.patchValue({
      code: family.code,
      address: family.address,
      city: family.city,
      village: family.village,
      district: family.district,
      phone: family.phone,
      livingCondition: family.livingCondition,
      housingType: family.housingType,
      providerType: family.providerType || 'father',
      registrationDate: family.registrationDate ? family.registrationDate.split('T')[0] : new Date().toISOString().split('T')[0],
      notes: family.notes
    });
  }

  private patchFatherForm(father: FatherDto): void {
    this.fatherForm.patchValue({
      fullName: father.fullName,
      nationalId: father.nationalId,
      passportNumber: father.passportNumber,
      dateOfBirth: father.dateOfBirth ? father.dateOfBirth.split('T')[0] : '',
      placeOfBirth: father.placeOfBirth,
      educationLevel: father.educationLevel,
      job: father.job,
      monthlyIncome: father.monthlyIncome,
      healthStatus: father.healthStatus,
      phone: father.phone,
      isAlive: father.isAlive,
      isProvider: father.isProvider,
      deathDate: father.deathDate ? father.deathDate.split('T')[0] : '',
      notes: father.notes
    });
  }

  private patchMotherForm(mother: MotherDto): void {
    this.motherForm.patchValue({
      fullName: mother.fullName,
      nationalId: mother.nationalId,
      passportNumber: mother.passportNumber,
      dateOfBirth: mother.dateOfBirth ? mother.dateOfBirth.split('T')[0] : '',
      placeOfBirth: mother.placeOfBirth,
      educationLevel: mother.educationLevel,
      job: mother.job,
      monthlyIncome: mother.monthlyIncome,
      healthStatus: mother.healthStatus,
      phone: mother.phone,
      isAlive: mother.isAlive,
      isProvider: mother.isProvider,
      deathDate: mother.deathDate ? mother.deathDate.split('T')[0] : '',
      notes: mother.notes
    });
  }

  private patchProviderForm(provider: ProviderDto): void {
    this.providerForm.patchValue({
      fullName: provider.fullName,
      relationship: provider.relationship,
      nationalId: provider.nationalId,
      passportNumber: provider.passportNumber,
      phone: provider.phone,
      address: provider.address,
      job: provider.job,
      monthlyIncome: provider.monthlyIncome,
      notes: provider.notes
    });
  }

  onProviderTypeChange(): void {
    const providerType = this.familyForm.get('providerType')?.value;
    // Update visibility of forms based on provider type
  }

  // ==================== Duplicate-phone check (UC-ORP-10) ====================

  /** One debounced check per phone holder: family, father, mother, provider. */
  private setupPhoneDuplicateChecks(): void {
    const holders: Array<[string, FormGroup]> = [
      ['family', this.familyForm],
      ['father', this.fatherForm],
      ['mother', this.motherForm],
      ['provider', this.providerForm]
    ];

    for (const [holder, form] of holders) {
      const control = form.get('phone');
      if (!control) {
        continue;
      }
      this.phoneCheckSubscriptions.push(
        control.valueChanges
          .pipe(debounceTime(400), distinctUntilChanged())
          .subscribe(() => this.checkPhone(holder, form))
      );
    }
  }

  /** Flag a number already held by another family in scope (spans all five holders). */
  private checkPhone(holder: string, form: FormGroup): void {
    const number = (form.get('phone')?.value ?? '').toString().trim();
    if (!number || !this.familyId) {
      this.phoneChecks[holder] = { status: 'idle' };
      return;
    }

    // P6 — the state carries the number it describes: a response for any other number is
    // stale and must not pin (or clear) a flag on what the user has typed since.
    this.phoneChecks[holder] = { status: 'checking', checkedNumber: number };
    this.familyService.checkPhoneDuplicate(this.familyId, { number }).subscribe({
      next: result => {
        if (this.phoneChecks[holder]?.checkedNumber !== number) { return; }
        this.phoneChecks[holder] = result.isDuplicate
          ? { status: 'duplicate', checkedNumber: number, holderLabel: this.composeHolderLabel(result) }
          : { status: 'available', checkedNumber: number };
      },
      error: (error: any) => {
        console.error('Error checking phone number:', error);
        if (this.phoneChecks[holder]?.checkedNumber === number) {
          this.phoneChecks[holder] = { status: 'idle', checkedNumber: number };
        }
      }
    });
  }

  /** P16 — the server returns the holder as structured data; the label is translated here. */
  private composeHolderLabel(result: PhoneCheckDto): string | null {
    if (!result.holderName) { return null; }
    const key = result.holderType ? `orphanCoding.holder_${result.holderType}` : '';
    const typeWord = key ? this.translate.instant(key) : '';
    const who = typeWord && !typeWord.startsWith('orphanCoding.')
      ? `${result.holderName} (${typeWord})`
      : result.holderName;
    return result.holderFamilyCode ? `${who} — ${result.holderFamilyCode}` : who;
  }

  /** A flagged duplicate blocks the save of its holder's section (UC-ORP-10). */
  private phoneDuplicateBlocked(holder: string): boolean {
    if (this.phoneChecks[holder]?.status === 'duplicate') {
      this.notification.error(this.translate.instant('orphanCoding.phoneDuplicate'));
      return true;
    }
    return false;
  }

  // ==================== National-id uniqueness check (UC-SYS-12) ====================

  /**
   * One debounced check per id holder: father, mother, provider. Edit mode only — like the
   * phone check, the call spans every OTHER family in scope and needs this family's id to
   * exclude itself, so it cannot run before the family exists (create mode stays on the
   * server-side uniqueness judged at save).
   */
  private setupNationalIdChecks(): void {
    const holders: Array<[string, FormGroup]> = [
      ['father', this.fatherForm],
      ['mother', this.motherForm],
      ['provider', this.providerForm]
    ];

    for (const [, form] of holders) {
      const control = form.get('nationalId');
      if (!control) {
        continue;
      }
      this.nidCheckSubscriptions.push(
        control.valueChanges
          .pipe(debounceTime(400), distinctUntilChanged())
          .subscribe(() => this.checkNationalId(control))
      );
    }
  }

  /**
   * Flag an id already held by a person on another family in scope (UC-SYS-12). The clash
   * lands on the control as a `server` error — rendered as-is by app-input-text — so it
   * blocks the section's save alongside the required rule; a pass clears it. A failed check
   * stays silent on purpose: this is a read-only aid, the server re-judges on save.
   */
  private checkNationalId(control: AbstractControl): void {
    const nationalId = (control.value ?? '').toString().trim();
    this.clearNationalIdError(control);
    if (!nationalId || !this.familyId) {
      return;
    }

    this.familyService.checkFamilyNationalId({ nationalId, familyId: this.familyId, charityId: this.familyCharityId ?? undefined }).subscribe({
      next: result => {
        // Stale response guard — the user typed on after this request left.
        if ((control.value ?? '').toString().trim() !== nationalId) { return; }
        if (result.isUnique) {
          this.clearNationalIdError(control);
          return;
        }
        // P16 — the server returns the holder as structured data; the label is translated here.
        const key = result.holderType ? `orphanCoding.holder_${result.holderType.toLowerCase()}` : '';
        const typeWord = key ? this.translate.instant(key) : '';
        const who = typeWord && !typeWord.startsWith('orphanCoding.')
          ? `${result.holderName} (${typeWord})`
          : (result.holderName ?? '');
        const message = result.holderFamilyCode
          ? this.translate.instant('families.nationalIdClash', { holder: who, code: result.holderFamilyCode })
          : this.translate.instant('families.nationalIdClashNoCode', { holder: who });
        control.setErrors({ ...control.errors, server: message });
      },
      error: (error: any) => console.error('Error checking national id:', error)
    });
  }

  /** Drop a stale UC-SYS-12 verdict without touching required/other errors on the control. */
  private clearNationalIdError(control: AbstractControl): void {
    if (control.errors && control.errors['server']) {
      const { server, ...rest } = control.errors;
      control.setErrors(Object.keys(rest).length ? rest : null);
    }
  }

  onFamilyAttachmentChange(attachments: AttachmentDto[]): void {
    this.familyAttachments = attachments;
  }

  attachmentFileType = AttachmentFileType;

  onSubmit(): void {
    if (this.familyForm.invalid) {
      this.markFormGroupTouched(this.familyForm);
      this.notification.error(this.translate.instant('families.fixValidationErrors'));
      return;
    }

    // UC-ORP-10 — a duplicate family phone blocks the save
    if (this.isEditMode && this.phoneDuplicateBlocked('family')) {
      return;
    }

    this.saving = true;

    if (this.isEditMode && this.familyId) {
      this.updateFamily();
    } else {
      this.createFamily();
    }
  }

  private createFamily(): void {
    const formValue = this.familyForm.value;

    const family: CreateFamilyDto = {
      code: formValue.code,
      address: formValue.address,
      city: formValue.city,
      village: formValue.village,
      district: formValue.district,
      phone: formValue.phone,
      livingCondition: formValue.livingCondition,
      housingType: formValue.housingType,
      providerType: formValue.providerType,
      registrationDate: formValue.registrationDate,
      notes: formValue.notes,
      father: this.getFatherData(), // Mandatory
      mother: this.getMotherData(), // Mandatory
      provider: this.shouldCreateProvider() ? this.getProviderData() : undefined,
      relatives: this.relatives.length > 0 ? this.relatives : undefined
    };

    this.familyService.createFamily(family).subscribe({
      next: (response: FamilyDto) => {
        this.notification.success(this.translate.instant('families.createSuccess'));
        this.saving = false;
        this.router.navigate(['/families', response.id]);
      },
      error: (error: any) => {
        console.error('Error creating family:', error);
        this.notification.error(this.translate.instant('families.createFailed'));
        this.saving = false;
      }
    });
  }

  private updateFamily(): void {
    const formValue = this.familyForm.value;

    const family: UpdateFamilyDto = {
      code: formValue.code,
      address: formValue.address,
      city: formValue.city,
      village: formValue.village,
      district: formValue.district,
      phone: formValue.phone,
      livingCondition: formValue.livingCondition,
      housingType: formValue.housingType,
      providerType: formValue.providerType,
      notes: formValue.notes
    };

    this.familyService.updateFamily(this.familyId!, family).subscribe({
      next: (response: FamilyDto) => {
        this.notification.success(this.translate.instant('families.updateSuccess'));
        this.saving = false;
        this.router.navigate(['/families', this.familyId]);
      },
      error: (error: any) => {
        console.error('Error updating family:', error);
        this.notification.error(this.translate.instant('families.updateFailed'));
        this.saving = false;
      }
    });
  }

  private shouldCreateFather(): boolean {
    const providerType = this.familyForm.get('providerType')?.value;
    if (providerType === 'father' && !this.father) {
      return this.fatherForm.get('fullName')?.value;
    }
    return false;
  }

  private shouldCreateMother(): boolean {
    const providerType = this.familyForm.get('providerType')?.value;
    if (providerType === 'mother' && !this.mother) {
      return this.motherForm.get('fullName')?.value;
    }
    return false;
  }

  private shouldCreateProvider(): boolean {
    const providerType = this.familyForm.get('providerType')?.value;
    if (providerType === 'other' && !this.provider) {
      return this.providerForm.get('fullName')?.value;
    }
    return false;
  }

  private getFatherData(): CreateFatherDto {
    const formValue = this.fatherForm.value;
    return {
      fullName: formValue.fullName,
      nationalId: formValue.nationalId || '',
      passportNumber: formValue.passportNumber,
      dateOfBirth: formValue.dateOfBirth || '',
      placeOfBirth: formValue.placeOfBirth,
      educationLevel: formValue.educationLevel,
      job: formValue.job,
      monthlyIncome: formValue.monthlyIncome,
      healthStatus: formValue.healthStatus,
      phone: formValue.phone,
      isAlive: formValue.isAlive !== false,
      isProvider: formValue.isProvider === true,
      deathDate: formValue.deathDate || undefined,
      notes: formValue.notes
    };
  }

  private getMotherData(): CreateMotherDto {
    const formValue = this.motherForm.value;
    return {
      fullName: formValue.fullName,
      nationalId: formValue.nationalId || '',
      passportNumber: formValue.passportNumber,
      dateOfBirth: formValue.dateOfBirth || '',
      placeOfBirth: formValue.placeOfBirth,
      educationLevel: formValue.educationLevel,
      job: formValue.job,
      monthlyIncome: formValue.monthlyIncome,
      healthStatus: formValue.healthStatus,
      phone: formValue.phone,
      isAlive: formValue.isAlive !== false,
      isProvider: formValue.isProvider === true,
      deathDate: formValue.deathDate || undefined,
      notes: formValue.notes
    };
  }

  private getProviderData(): CreateProviderDto {
    const formValue = this.providerForm.value;
    return {
      fullName: formValue.fullName,
      relationship: formValue.relationship,
      nationalId: formValue.nationalId,
      passportNumber: formValue.passportNumber,
      phone: formValue.phone,
      address: formValue.address,
      job: formValue.job,
      monthlyIncome: formValue.monthlyIncome,
      notes: formValue.notes
    };
  }

  saveFather(): void {
    if (this.familyId) {
      if (this.phoneDuplicateBlocked('father')) {
        return;
      }
      const fatherData = this.getFatherData();
      if (this.father) {
        // Update existing father
        this.familyService.updateFather(this.familyId, fatherData).subscribe({
          next: (response) => {
            this.father = response;
            this.notification.success(this.translate.instant('families.fatherUpdateSuccess'));
          },
          error: (error) => {
            console.error('Error updating father:', error);
            this.notification.error(this.translate.instant('families.fatherUpdateFailed'));
          }
        });
      } else {
        // Create new father
        fatherData.familyId = this.familyId;
        this.familyService.createFather(this.familyId, fatherData).subscribe({
          next: (response) => {
            this.father = response;
            this.notification.success(this.translate.instant('families.fatherCreateSuccess'));
          },
          error: (error) => {
            console.error('Error creating father:', error);
            this.notification.error(this.translate.instant('families.fatherCreateFailed'));
          }
        });
      }
    }
  }

  saveMother(): void {
    if (this.familyId) {
      if (this.phoneDuplicateBlocked('mother')) {
        return;
      }
      const motherData = this.getMotherData();
      if (this.mother) {
        // Update existing mother
        this.familyService.updateMother(this.familyId, motherData).subscribe({
          next: (response) => {
            this.mother = response;
            this.notification.success(this.translate.instant('families.motherUpdateSuccess'));
          },
          error: (error) => {
            console.error('Error updating mother:', error);
            this.notification.error(this.translate.instant('families.motherUpdateFailed'));
          }
        });
      } else {
        // Create new mother
        motherData.familyId = this.familyId;
        this.familyService.createMother(this.familyId, motherData).subscribe({
          next: (response) => {
            this.mother = response;
            this.notification.success(this.translate.instant('families.motherCreateSuccess'));
          },
          error: (error) => {
            console.error('Error creating mother:', error);
            this.notification.error(this.translate.instant('families.motherCreateFailed'));
          }
        });
      }
    }
  }

  saveProvider(): void {
    if (this.familyId) {
      if (this.phoneDuplicateBlocked('provider')) {
        return;
      }
      const providerData = this.getProviderData();
      if (this.provider) {
        // Update existing provider
        this.familyService.updateProvider(this.familyId, providerData).subscribe({
          next: (response) => {
            this.provider = response;
            this.notification.success(this.translate.instant('families.providerUpdateSuccess'));
          },
          error: (error) => {
            console.error('Error updating provider:', error);
            this.notification.error(this.translate.instant('families.providerUpdateFailed'));
          }
        });
      } else {
        // Create new provider
        providerData.familyId = this.familyId;
        this.familyService.createProvider(this.familyId, providerData).subscribe({
          next: (response) => {
            this.provider = response;
            this.notification.success(this.translate.instant('families.providerCreateSuccess'));
          },
          error: (error) => {
            console.error('Error creating provider:', error);
            this.notification.error(this.translate.instant('families.providerCreateFailed'));
          }
        });
      }
    }
  }

  cancel(): void {
    if (this.isEditMode && this.familyId) {
      this.router.navigate(['/families', this.familyId]);
    } else {
      this.router.navigate(['/families']);
    }
  }

  isFieldValid(fieldName: string, formGroup: FormGroup = this.familyForm): boolean {
    const field = formGroup.get(fieldName);
    return field ? field.valid && (field.dirty || field.touched) : false;
  }

  isFieldInvalid(fieldName: string, formGroup: FormGroup = this.familyForm): boolean {
    const field = formGroup.get(fieldName);
    return field ? field.invalid && (field.dirty || field.touched) : false;
  }

  getErrorMessage(fieldName: string, formGroup: FormGroup = this.familyForm): string {
    const field = formGroup.get(fieldName);
    if (!field || !field.errors) return '';

    const fieldLabel = this.translate.instant(`families.${fieldName}`);

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

  hasFather(): boolean {
    return !!this.father;
  }

  hasMother(): boolean {
    return !!this.mother;
  }

  hasProvider(): boolean {
    return !!this.provider;
  }

  showProviderType(type: string): boolean {
    return this.familyForm.get('providerType')?.value === type;
  }

  // ==================== Relative Management Methods ====================

  addRelative(): void {
    if (this.relativesForm.invalid) {
      this.markFormGroupTouched(this.relativesForm);
      this.notification.error(this.translate.instant('families.fixValidationErrors'));
      return;
    }

    const relativeData = this.getRelativeData();
    this.relatives.push(relativeData);

    // Reset the form
    this.relativesForm = this.createRelativeForm();

    this.notification.success(this.translate.instant('families.relativeAdded'));
  }

  onRelativeAdded(relativeData: any): void {
    this.relatives.push(relativeData);

    // Reset the form
    this.relativesForm = this.createRelativeForm();

    this.notification.success(this.translate.instant('families.relativeAdded'));
  }

  removeRelative(index: number): void {
    this.relatives.splice(index, 1);
    this.notification.success(this.translate.instant('families.relativeRemoved'));
  }

  getRelativeData(): CreateRelativeDto {
    const formValue = this.relativesForm.value;
    return {
      fullName: formValue.fullName,
      relationshipType: formValue.relationshipType,
      gender: formValue.gender,
      dateOfBirth: formValue.dateOfBirth,
      placeOfBirth: formValue.placeOfBirth,
      nationalId: formValue.nationalId,
      educationLevelId: formValue.educationLevel,
      job: formValue.job,
      monthlyIncome: formValue.monthlyIncome,
      healthStatusId: formValue.healthStatus,
      phone: formValue.phone,
      address: formValue.address,
      isAlive: formValue.isAlive !== false,
      isLivingWithFamily: formValue.isLivingWithFamily === true,
      deathDate: formValue.deathDate || undefined,
      notes: formValue.notes
    };
  }

  getRelativesCount(): number {
    return this.relatives.length;
  }

  calculateAge(dateOfBirth: string): number {
    const today = new Date();
    const birthDate = new Date(dateOfBirth);
    let age = today.getFullYear() - birthDate.getFullYear();
    const monthDiff = today.getMonth() - birthDate.getMonth();
    if (monthDiff < 0 || (monthDiff === 0 && today.getDate() < birthDate.getDate())) {
      age--;
    }
    return age;
  }
}
