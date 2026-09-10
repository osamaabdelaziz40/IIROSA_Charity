import { Component, OnInit, OnDestroy, ViewChildren, QueryList } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup, FormArray, Validators, FormControl, AbstractControl } from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { FamilyService } from '../services/family.service';
import { NotificationService } from '../../../core/services/notification.service';
import { AttachmentService } from '../../../core/services/attachment.service';
import { PageHeaderComponent } from '../../../shared/components/page-header/page-header.component';
import { BreadcrumbComponent, BreadcrumbItem, AttachmentInputComponent, CollapsibleCardComponent } from '../../../shared/components';
import { TranslateModule, TranslateService, LangChangeEvent } from '@ngx-translate/core';
import {
  FamilyDto,
  CreateFamilyDto,
  UpdateFamilyDto,
  CreateFamilyPhoneDto,
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
import { LookupManagementService } from '../../lookup-management/services/lookup-management.service';
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

/** Shared dropdown option shape (missions/create convention). */
interface DropdownOption {
  id: any;
  name: string;
}

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

  // Collapsible section cards — every section starts collapsed. Cards rendered
  // through the shared app-collapsible-card; the two cards with composite
  // headers (Other Provider, Relatives) collapse manually via this map.
  sections: Record<'otherProvider' | 'relatives', boolean> = { otherProvider: false, relatives: false };

  @ViewChildren(CollapsibleCardComponent) collapsibleCards?: QueryList<CollapsibleCardComponent>;

  toggleSection(key: 'otherProvider' | 'relatives'): void {
    this.sections[key] = !this.sections[key];
  }

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
  /** الحالة الصحية — HealthStatus lookup rows (father/mother/relatives sections) */
  healthStatusOptions: Array<{ id: number; name: string }> = [];
  relationshipOptions: Array<{ id: string; name: string }> = [];
  /** الجنسية — Country lookup (father/mother sections) */
  nationalityOptions: Array<{ id: number; name: string }> = [];
  /** سبب الوفاة — DeathReason lookup (father/mother sections) */
  deathReasonOptions: Array<{ id: number; name: string }> = [];

  // Raw lookup rows — options rebuild from these on language change
  private healthStatusRows: Array<any> = [];
  private nationalityRows: Array<any> = [];
  private deathReasonRows: Array<any> = [];

  // Death certificates (father/mother) — the uploaded/attached server file shown in edit mode
  fatherDeathCertificate: { id: string; fileName: string } | null = null;
  motherDeathCertificate: { id: string; fileName: string } | null = null;

  // §4 معلومات الأسرة — catalogues from the real lookup endpoints
  countries: DropdownOption[] = [];
  private allRegions: any[] = [];
  regionOptions: DropdownOption[] = [];
  private allCenters: any[] = [];
  centerOptions: DropdownOption[] = [];
  houseOwnershipOptions: DropdownOption[] = [];
  incomeTypeOptions: DropdownOption[] = [];
  familyProjectStatusOptions: DropdownOption[] = [];

  // §4 multi-phone rows — per-row UC-ORP-10 duplicate state, index-aligned with the FormArray
  phoneRowChecks: PhoneCheckState[] = [];
  /** Edit-mode fallbacks for the per-member share preview (server computes the stored value) */
  private editMonthlyIncome: number | null = null;
  private editFamilyMembersCount: number | null = null;

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
    private lookupService: LookupManagementService,
    private attachmentService: AttachmentService,
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
    this.loadCatalogues();

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

    // healthStatusOptions / nationalityOptions / deathReasonOptions come from the lookup
    // endpoints (loadCatalogues) — rebuilt per language in refreshMemberCatalogueOptions.
    this.refreshMemberCatalogueOptions();

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
      village: [''],
      district: [''],
      countryId: [null],
      regionId: [null],
      centerId: [null],
      livingCondition: [''],
      housingType: [''],
      houseOwnershipId: [null],
      incomeTypeId: [null],
      incomeValue: [null],
      totalIncome: [null],
      childrenCount: [null],
      hasProject: [false],
      familyProjectStatusId: [null],
      phones: this.fb.array([this.createPhoneRow()]),
      providerType: ['father'],
      registrationDate: [new Date().toISOString().split('T')[0]],
      notes: ['']
    });
  }

  private createPhoneRow(isDefault = false): FormGroup {
    return this.fb.group({
      number: [''],
      isDefault: [isDefault]
    });
  }

  private createFatherForm(): FormGroup {
    const form = this.fb.group({
      fullName: ['', Validators.required],
      // §10 اضافة معيل — legacy four-part name
      firstName: [''],
      secondName: [''],
      thirdName: [''],
      familyName: [''],
      nationalId: ['', Validators.required],
      nationalityCountryId: [null],
      passportNumber: [''],
      dateOfBirth: ['', Validators.required],
      placeOfBirth: [''],
      job: [''],
      monthlyIncome: [null],
      healthStatusId: [null],
      mezaCard: [''],
      mezaCardExpirationDate: [''],
      phone: [''],
      isDead: [false],
      isProvider: [false],
      deathDate: [''],
      deathReasonId: [null],
      deathCertificateAttachmentId: [null],
      notes: ['']
    });
    this.wireDeathFields(form);
    return form;
  }

  private createMotherForm(): FormGroup {
    const form = this.fb.group({
      fullName: ['', Validators.required],
      // §10 اضافة معيل — legacy four-part name
      firstName: [''],
      secondName: [''],
      thirdName: [''],
      familyName: [''],
      nationalId: ['', Validators.required],
      nationalityCountryId: [null],
      passportNumber: [''],
      dateOfBirth: ['', Validators.required],
      placeOfBirth: [''],
      job: [''],
      monthlyIncome: [null],
      healthStatusId: [null],
      mezaCard: [''],
      mezaCardExpirationDate: [''],
      phone: [''],
      isDead: [false],
      isProvider: [false],
      deathDate: [''],
      deathReasonId: [null],
      deathCertificateAttachmentId: [null],
      notes: ['']
    });
    this.wireDeathFields(form);
    return form;
  }

  /** isDead toggles the death-details block — date + reason become required while checked. */
  private wireDeathFields(form: FormGroup): void {
    const isDead = form.get('isDead');
    if (!isDead) {
      return;
    }
    isDead.valueChanges.subscribe(dead => {
      const deathDate = form.get('deathDate');
      const deathReasonId = form.get('deathReasonId');
      if (dead) {
        deathDate?.setValidators(Validators.required);
        deathReasonId?.setValidators(Validators.required);
      } else {
        deathDate?.clearValidators();
        deathReasonId?.clearValidators();
        deathDate?.setValue('');
        deathReasonId?.setValue(null);
      }
      deathDate?.updateValueAndValidity({ emitEvent: false });
      deathReasonId?.updateValueAndValidity({ emitEvent: false });
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
    this.editMonthlyIncome = family.monthlyIncome ?? null;
    this.editFamilyMembersCount = family.familyMembersCount ?? null;

    this.familyForm.patchValue({
      code: family.code,
      address: family.address,
      village: family.village,
      district: family.district,
      countryId: family.countryId ?? null,
      regionId: family.regionId ?? null,
      centerId: family.centerId ?? null,
      livingCondition: family.livingCondition,
      housingType: family.housingType,
      providerType: family.providerType || 'father',
      houseOwnershipId: family.houseOwnershipId ?? null,
      incomeTypeId: family.incomeTypeId ?? null,
      incomeValue: family.incomeValue ?? null,
      totalIncome: family.totalIncome ?? null,
      childrenCount: family.childrenCount ?? null,
      hasProject: family.hasProject === true,
      familyProjectStatusId: family.hasProject ? (family.familyProjectStatusId ?? null) : null,
      registrationDate: family.registrationDate ? family.registrationDate.split('T')[0] : new Date().toISOString().split('T')[0],
      notes: family.notes
    });

    // Cascade lists must reflect the patched ids before the dropdowns render
    this.refreshRegionOptions();
    this.refreshCenterOptions();

    // Phones — rebuild the rows from the stored set (default-first on the wire); a family
    // created before the multi-phone table falls back to its mirrored PhoneNumber.
    this.phonesArray.clear();
    this.phoneRowChecks = [];
    const phones = family.phones || [];
    if (phones.length > 0) {
      phones.forEach(p => this.appendPhoneRow(p.number, p.isDefault === true));
    } else {
      this.appendPhoneRow(family.phoneNumber || '', true);
    }
  }

  private patchFatherForm(father: FatherDto): void {
    this.fatherForm.patchValue({
      fullName: father.fullName,
      firstName: father.firstName,
      secondName: father.secondName,
      thirdName: father.thirdName,
      familyName: father.familyName,
      nationalId: father.nationalId,
      nationalityCountryId: father.nationalityCountryId ?? null,
      passportNumber: father.passportNumber,
      dateOfBirth: father.dateOfBirth ? father.dateOfBirth.split('T')[0] : '',
      placeOfBirth: father.placeOfBirth,
      job: father.job,
      monthlyIncome: father.monthlyIncome,
      healthStatusId: father.healthStatusId ?? null,
      mezaCard: father.mezaCard,
      mezaCardExpirationDate: father.mezaCardExpirationDate ? father.mezaCardExpirationDate.split('T')[0] : '',
      phone: father.phone,
      isDead: father.isAlive === false,
      isProvider: father.isProvider,
      deathDate: father.deathDate ? father.deathDate.split('T')[0] : '',
      deathReasonId: father.deathReasonId ?? null,
      deathCertificateAttachmentId: father.deathCertificateAttachmentId ?? null,
      notes: father.notes
    });
    this.loadDeathCertificateName(father.deathCertificateAttachmentId, 'father');
  }

  private patchMotherForm(mother: MotherDto): void {
    this.motherForm.patchValue({
      fullName: mother.fullName,
      firstName: mother.firstName,
      secondName: mother.secondName,
      thirdName: mother.thirdName,
      familyName: mother.familyName,
      nationalId: mother.nationalId,
      nationalityCountryId: mother.nationalityCountryId ?? null,
      passportNumber: mother.passportNumber,
      dateOfBirth: mother.dateOfBirth ? mother.dateOfBirth.split('T')[0] : '',
      placeOfBirth: mother.placeOfBirth,
      job: mother.job,
      monthlyIncome: mother.monthlyIncome,
      healthStatusId: mother.healthStatusId ?? null,
      mezaCard: mother.mezaCard,
      mezaCardExpirationDate: mother.mezaCardExpirationDate ? mother.mezaCardExpirationDate.split('T')[0] : '',
      phone: mother.phone,
      isDead: mother.isAlive === false,
      isProvider: mother.isProvider,
      deathDate: mother.deathDate ? mother.deathDate.split('T')[0] : '',
      deathReasonId: mother.deathReasonId ?? null,
      deathCertificateAttachmentId: mother.deathCertificateAttachmentId ?? null,
      notes: mother.notes
    });
    this.loadDeathCertificateName(mother.deathCertificateAttachmentId, 'mother');
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

  // ==================== §4 catalogues + address cascade ====================

  /** Every §4 catalogue from its real endpoint — regions/centers load once and filter locally,
   *  so the cascade (and the edit-mode patch) never races a follow-up fetch. */
  private loadCatalogues(): void {
    this.lookupService.getCountries({ page: 1, pageSize: 1000, isActive: true })
      .subscribe({
        next: result => {
          this.countries = this.toOptions(result.items || []);
          this.nationalityRows = result.items || [];
          this.refreshMemberCatalogueOptions();
        },
        error: () => this.notifyCatalogueFailure()
      });

    // Father/mother section catalogues (§10 اضافة معيل)
    this.lookupService.getHealthStatuses()
      .subscribe({
        next: r => { this.healthStatusRows = r || []; this.refreshMemberCatalogueOptions(); },
        error: () => this.notifyCatalogueFailure()
      });

    this.lookupService.getDeathReasons()
      .subscribe({
        next: r => { this.deathReasonRows = r || []; this.refreshMemberCatalogueOptions(); },
        error: () => this.notifyCatalogueFailure()
      });

    this.lookupService.getRegions({ page: 1, pageSize: 1000, isActive: true })
      .subscribe({
        next: result => { this.allRegions = result.items || []; this.refreshRegionOptions(); },
        error: () => this.notifyCatalogueFailure()
      });

    this.lookupService.getCenters({ page: 1, pageSize: 1000, isActive: true })
      .subscribe({
        next: result => { this.allCenters = result.items || []; this.refreshCenterOptions(); },
        error: () => this.notifyCatalogueFailure()
      });

    this.lookupService.getHouseOwnerships()
      .subscribe({
        next: r => (this.houseOwnershipOptions = this.toOptions(r)),
        error: () => this.notifyCatalogueFailure()
      });

    this.lookupService.getIncomeTypes()
      .subscribe({
        next: r => (this.incomeTypeOptions = this.toOptions(r)),
        error: () => this.notifyCatalogueFailure()
      });

    this.lookupService.getFamilyProjectStatuses()
      .subscribe({
        next: r => (this.familyProjectStatusOptions = this.toOptions(r)),
        error: () => this.notifyCatalogueFailure()
      });
  }

  private toOptions(list: any[]): DropdownOption[] {
    return (list || []).map(item => ({
      id: item.id,
      name: item.nameAr || item.name || item.nameEn || String(item.id)
    }));
  }

  /** Lang-aware options builder for the father/mother catalogues (NameAr/NameEn rows). */
  private toLangOptions(list: any[]): Array<{ id: number; name: string }> {
    const lang = this.translate.currentLang || 'ar';
    return (list || []).map(item => ({
      id: item.id,
      name: (lang === 'en' ? item.nameEn || item.nameAr : item.nameAr || item.nameEn)
        || item.name || String(item.id)
    }));
  }

  /** Father/mother catalogue options — rebuilt from the raw rows so a language switch re-translates. */
  private refreshMemberCatalogueOptions(): void {
    this.nationalityOptions = this.toLangOptions(this.nationalityRows);
    this.healthStatusOptions = this.toLangOptions(this.healthStatusRows);
    this.deathReasonOptions = this.toLangOptions(this.deathReasonRows);
  }

  private notifyCatalogueFailure(): void {
    this.notification.error(this.translate.instant('families.catalogueLoadFailed'));
  }

  /** Cascade refresh — regions follow the chosen country, centers follow the region. */
  private refreshRegionOptions(): void {
    const countryId = this.familyForm.get('countryId')?.value;
    const list = countryId != null ? this.allRegions.filter(r => r.countryId === countryId) : [];
    this.regionOptions = this.toOptions(list);
  }

  private refreshCenterOptions(): void {
    const regionId = this.familyForm.get('regionId')?.value;
    const list = regionId != null ? this.allCenters.filter(c => c.regionId === regionId) : [];
    this.centerOptions = this.toOptions(list);
  }

  /** Country changed → narrow the region list, reset the region/center cascade. */
  onCountryChanged(value: DropdownOption | null): void {
    if (!value || value.id == null) {
      this.familyForm.get('countryId')?.setValue(null);
    }
    this.familyForm.get('regionId')?.setValue(null);
    this.familyForm.get('centerId')?.setValue(null);
    this.refreshRegionOptions();
    this.refreshCenterOptions();
  }

  /** Region changed → narrow the center list, reset center. */
  onRegionChanged(value: DropdownOption | null): void {
    if (!value || value.id == null) {
      this.familyForm.get('regionId')?.setValue(null);
    }
    this.familyForm.get('centerId')?.setValue(null);
    this.refreshCenterOptions();
  }

  // ==================== §4 multi phone rows ====================

  get phonesArray(): FormArray {
    return this.familyForm.get('phones') as FormArray;
  }

  private appendPhoneRow(number = '', isDefault = false): void {
    const row = this.createPhoneRow(isDefault);
    row.get('number')?.setValue(number);
    const state: PhoneCheckState = { status: 'idle' };
    this.phoneRowChecks.push(state);
    // UC-ORP-10 — edit mode only (the check needs this family's id to exclude it)
    if (this.familyId) {
      this.wirePhoneRowCheck(row, state);
    }
    this.phonesArray.push(row);
  }

  addPhone(): void {
    this.appendPhoneRow();
  }

  removePhone(index: number): void {
    this.phonesArray.removeAt(index);
    this.phoneRowChecks.splice(index, 1);
    if (this.phonesArray.length === 0) {
      this.appendPhoneRow();
      return;
    }
    // The set always carries exactly one flagged default
    if (!this.phonesArray.controls.some(c => c.get('isDefault')?.value === true)) {
      this.phonesArray.at(0).get('isDefault')?.setValue(true);
    }
  }

  setDefaultPhone(index: number): void {
    this.phonesArray.controls.forEach((c, i) => c.get('isDefault')?.setValue(i === index));
  }

  trackPhoneRow(index: number, item: AbstractControl): AbstractControl {
    return item;
  }

  /** Live نصيب الفرد preview — the server computes and stores the authoritative value. */
  get perMemberSharePreview(): number | null {
    const income = this.toNumber(this.familyForm.get('totalIncome')?.value) ?? this.editMonthlyIncome;
    const divisor = this.toNumber(this.familyForm.get('childrenCount')?.value) ?? this.editFamilyMembersCount;
    return income != null && divisor ? Math.round((income / divisor) * 100) / 100 : null;
  }

  private toNumber(value: any): number | null {
    if (value === null || value === undefined || value === '') { return null; }
    const n = Number(value);
    return Number.isFinite(n) ? n : null;
  }

  /** Unchecking هل الأسرة تمتلك مشروع drops the stored status. */
  onHasProjectChange(checked: boolean): void {
    if (!checked) {
      this.familyForm.get('familyProjectStatusId')?.setValue(null);
    }
  }

  // ==================== Duplicate-phone check (UC-ORP-10) ====================

  /** One debounced check per phone holder: the §4 multi-phone rows (wired as they are
   *  created), plus father, mother and provider. */
  private setupPhoneDuplicateChecks(): void {
    const holders: Array<[string, FormGroup]> = [
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

  /** UC-ORP-10 — one debounced check per §4 phone row; the state object is captured by the
   *  closure so removing a row never corrupts another row's verdict. */
  private wirePhoneRowCheck(row: FormGroup, state: PhoneCheckState): void {
    const control = row.get('number');
    if (!control) {
      return;
    }
    this.phoneCheckSubscriptions.push(
      control.valueChanges
        .pipe(debounceTime(400), distinctUntilChanged())
        .subscribe(() => {
          const number = (control.value ?? '').toString().trim();
          if (!number) {
            state.status = 'idle';
            return;
          }
          state.status = 'checking';
          state.checkedNumber = number;
          this.familyService.checkPhoneDuplicate(this.familyId!, { number }).subscribe({
            next: result => {
              if (state.checkedNumber !== number) { return; }
              if (result.isDuplicate) {
                state.status = 'duplicate';
                state.holderLabel = this.composeHolderLabel(result);
              } else {
                state.status = 'available';
              }
            },
            error: (error: any) => {
              console.error('Error checking phone number:', error);
              if (state.checkedNumber === number) {
                state.status = 'idle';
              }
            }
          });
        })
    );
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
      // Reveal collapsed sections — the red fields must not stay hidden behind
      // collapsed headers while the toast points at them.
      this.collapsibleCards?.forEach(card => card.open());
      this.sections.otherProvider = true;
      this.sections.relatives = true;
      this.notification.error(this.translate.instant('families.fixValidationErrors'));
      return;
    }

    // UC-ORP-10 — a flagged duplicate on any §4 phone row blocks the save
    if (this.phoneRowChecks.some(s => s.status === 'duplicate')) {
      this.notification.error(this.translate.instant('orphanCoding.phoneDuplicate'));
      return;
    }

    if (this.isEditMode && this.phoneDuplicateBlocked('father')) {
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
      village: formValue.village,
      district: formValue.district,
      countryId: formValue.countryId ?? undefined,
      regionId: formValue.regionId ?? undefined,
      centerId: formValue.centerId ?? undefined,
      livingCondition: formValue.livingCondition,
      housingType: formValue.housingType,
      houseOwnershipId: formValue.houseOwnershipId ?? undefined,
      incomeTypeId: formValue.incomeTypeId ?? undefined,
      incomeValue: this.toNumber(formValue.incomeValue) ?? undefined,
      totalIncome: this.toNumber(formValue.totalIncome) ?? undefined,
      childrenCount: this.toNumber(formValue.childrenCount) ?? undefined,
      hasProject: formValue.hasProject === true,
      familyProjectStatusId: formValue.hasProject === true ? (formValue.familyProjectStatusId ?? undefined) : undefined,
      phones: this.getPhonesData(),
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
      village: formValue.village,
      district: formValue.district,
      countryId: formValue.countryId ?? undefined,
      regionId: formValue.regionId ?? undefined,
      centerId: formValue.centerId ?? undefined,
      livingCondition: formValue.livingCondition,
      housingType: formValue.housingType,
      houseOwnershipId: formValue.houseOwnershipId ?? undefined,
      incomeTypeId: formValue.incomeTypeId ?? undefined,
      incomeValue: this.toNumber(formValue.incomeValue) ?? undefined,
      totalIncome: this.toNumber(formValue.totalIncome) ?? undefined,
      childrenCount: this.toNumber(formValue.childrenCount) ?? undefined,
      hasProject: formValue.hasProject === true,
      familyProjectStatusId: formValue.hasProject === true ? (formValue.familyProjectStatusId ?? undefined) : undefined,
      phones: this.getPhonesData(),
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

  /** §4 phones payload — filled rows only; the server enforces exactly one default. */
  private getPhonesData(): CreateFamilyPhoneDto[] | undefined {
    const rows = this.phonesArray.value
      .filter((p: any) => (p.number ?? '').toString().trim() !== '')
      .map((p: any) => ({ number: p.number.toString().trim(), isDefault: p.isDefault === true }));
    return rows.length > 0 ? rows : undefined;
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
    const isDead = formValue.isDead === true;
    return {
      fullName: formValue.fullName,
      firstName: formValue.firstName || undefined,
      secondName: formValue.secondName || undefined,
      thirdName: formValue.thirdName || undefined,
      familyName: formValue.familyName || undefined,
      nationalId: formValue.nationalId || '',
      nationalityCountryId: formValue.nationalityCountryId ?? undefined,
      passportNumber: formValue.passportNumber,
      dateOfBirth: formValue.dateOfBirth || '',
      placeOfBirth: formValue.placeOfBirth,
      job: formValue.job,
      monthlyIncome: formValue.monthlyIncome,
      healthStatusId: formValue.healthStatusId ?? undefined,
      mezaCard: formValue.mezaCard || undefined,
      mezaCardExpirationDate: formValue.mezaCardExpirationDate || undefined,
      phone: formValue.phone,
      isAlive: !isDead,
      isProvider: formValue.isProvider === true,
      deathDate: isDead ? (formValue.deathDate || undefined) : undefined,
      deathReasonId: isDead ? (formValue.deathReasonId ?? undefined) : undefined,
      deathCertificateAttachmentId: isDead ? (formValue.deathCertificateAttachmentId ?? undefined) : undefined,
      notes: formValue.notes
    };
  }

  private getMotherData(): CreateMotherDto {
    const formValue = this.motherForm.value;
    const isDead = formValue.isDead === true;
    return {
      fullName: formValue.fullName,
      firstName: formValue.firstName || undefined,
      secondName: formValue.secondName || undefined,
      thirdName: formValue.thirdName || undefined,
      familyName: formValue.familyName || undefined,
      nationalId: formValue.nationalId || '',
      nationalityCountryId: formValue.nationalityCountryId ?? undefined,
      passportNumber: formValue.passportNumber,
      dateOfBirth: formValue.dateOfBirth || '',
      placeOfBirth: formValue.placeOfBirth,
      job: formValue.job,
      monthlyIncome: formValue.monthlyIncome,
      healthStatusId: formValue.healthStatusId ?? undefined,
      mezaCard: formValue.mezaCard || undefined,
      mezaCardExpirationDate: formValue.mezaCardExpirationDate || undefined,
      phone: formValue.phone,
      isAlive: !isDead,
      isProvider: formValue.isProvider === true,
      deathDate: isDead ? (formValue.deathDate || undefined) : undefined,
      deathReasonId: isDead ? (formValue.deathReasonId ?? undefined) : undefined,
      deathCertificateAttachmentId: isDead ? (formValue.deathCertificateAttachmentId ?? undefined) : undefined,
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

  // ==================== Father/mother death certificate (§10 اضافة معيل) ====================

  onFatherDeathCertificateSelected(file: File): void {
    this.uploadDeathCertificate(file, 'father');
  }

  onMotherDeathCertificateSelected(file: File): void {
    this.uploadDeathCertificate(file, 'mother');
  }

  /** Multipart upload first — the form only stores the returned server attachment id. */
  private uploadDeathCertificate(file: File, holder: 'father' | 'mother'): void {
    this.attachmentService.upload(file, 'Family').subscribe({
      next: response => {
        const form = holder === 'father' ? this.fatherForm : this.motherForm;
        form.get('deathCertificateAttachmentId')?.setValue(response.id);
        this.setDeathCertificate(holder, response.id, response.fileName);
      },
      error: (error: any) => {
        console.error('Error uploading death certificate:', error);
        this.notification.error(this.translate.instant('families.deathCertificateUploadFailed'));
      }
    });
  }

  onFatherDeathCertificateRemoved(): void {
    this.fatherDeathCertificate = null;
    this.fatherForm.get('deathCertificateAttachmentId')?.setValue(null);
  }

  onMotherDeathCertificateRemoved(): void {
    this.motherDeathCertificate = null;
    this.motherForm.get('deathCertificateAttachmentId')?.setValue(null);
  }

  /** Edit mode — resolve the stored certificate's display name via the attachment service. */
  private loadDeathCertificateName(attachmentId: string | undefined, holder: 'father' | 'mother'): void {
    if (!attachmentId) {
      return;
    }
    this.attachmentService.getById(attachmentId).subscribe({
      next: attachment => this.setDeathCertificate(holder, attachmentId, attachment.fileName),
      error: () => this.setDeathCertificate(holder, attachmentId, attachmentId)
    });
  }

  private setDeathCertificate(holder: 'father' | 'mother', id: string, fileName: string | undefined): void {
    const certificate = { id, fileName: fileName || id };
    if (holder === 'father') {
      this.fatherDeathCertificate = certificate;
    } else {
      this.motherDeathCertificate = certificate;
    }
  }

  saveFather(): void {
    if (this.familyId) {
      if (this.phoneDuplicateBlocked('father')) {
        return;
      }
      const fatherData = this.getFatherData();
      if (this.father) {
        // Update existing father — PUT /api/Families/father/{fatherId}; the loaded row's id rides the route
        this.familyService.updateFather({ ...fatherData, id: this.father.id }).subscribe({
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
        // Update existing mother — PUT /api/Families/mother/{motherId}; the loaded row's id rides the route
        this.familyService.updateMother({ ...motherData, id: this.mother.id }).subscribe({
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
