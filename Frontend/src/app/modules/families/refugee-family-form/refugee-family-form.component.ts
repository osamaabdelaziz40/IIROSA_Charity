import { ChangeDetectionStrategy, ChangeDetectorRef, Component, OnDestroy, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, FormArray, AbstractControl, ReactiveFormsModule, ValidationErrors, ValidatorFn, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { Subject, Subscription, Observable, forkJoin, of } from 'rxjs';
import { switchMap, takeUntil } from 'rxjs/operators';
import { TranslateModule, TranslateService, LangChangeEvent } from '@ngx-translate/core';
import { FamilyService } from '../services/family.service';
import { CharityService } from '../../charities/services/charity.service';
import { AuthService } from '../../../core/services/auth.service';
import { NotificationService } from '../../../core/services/notification.service';
import { LookupManagementService } from '../../lookup-management/services/lookup-management.service';
import { CountryDto } from '../../lookup-management/models/lookup.model';
import { PageHeaderComponent } from '../../../shared/components/page-header/page-header.component';
import { BreadcrumbComponent, BreadcrumbItem } from '../../../shared/components';
import { SharedModule } from '../../../shared/shared.module';
import {
  CreateFamilyDto, UpdateFamilyDto, UpdateProviderDto, UpdateOrphanDto, UpdateRelativeDto
} from '../models/family.model';

interface DropdownOption {
  id: number | string;
  name: string;
}

/**
 * UC-REF-03 (7-3) — تسجيل أسرة لاجئة. One superset POST /api/Families with
 * familyType: 'Refugee' (§12.S.2): household fields + معيل + staged اضافة ابن / اضافة مرافق
 * rows submitted in a single payload — the server rejects duplicates and mandatory
 * flags before anything is persisted.
 *
 * UC-REF-04 (7-4) — the same screen doubles as the edit form on /families/refugees/:id/edit:
 * household PUT + provider PUT + per-member orphan/relative create/update/delete, with the
 * server re-validating the §12.S.2 mandatory flags on the effective post-copy state.
 */
@Component({
  selector: 'app-refugee-family-form',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    PageHeaderComponent,
    BreadcrumbComponent,
    TranslateModule,
    SharedModule
  ],
  templateUrl: './refugee-family-form.component.html',
  styleUrls: ['./refugee-family-form.component.scss']
})
export class RefugeeFamilyFormComponent implements OnInit, OnDestroy {
  private destroy$ = new Subject<void>();
  private langChangeSubscription?: Subscription;

  familyForm!: FormGroup;
  providerForm!: FormGroup;
  childrenForm!: FormArray;
  companionsForm!: FormArray;

  saving = false;

  /** UC-REF-04 edit mode — set when the route carries :id (refugees/:id/edit) */
  editMode = false;
  familyId?: string;

  /** Members removed in edit mode — deleted server-side on save */
  deletedOrphanIds: string[] = [];
  deletedRelativeIds: string[] = [];

  /** Title/subtitle i18n keys switch between create and edit (UC-REF-04) */
  formTitleKey = 'families.refugeeFormTitle';
  formSubtitleKey = 'families.refugeeFormSubtitle';

  /** Mirrors providerForm.isAlive so the death fields' *ngIf survives OnPush */
  providerIsAlive = true;

  /** Mirrors providerForm.mainRelation === 'علاقة أخرى' — the نوعها select only applies then (P14) */
  showRelationCatalogue = false;

  /** الجمعية selector is an HQ-only control; a Charity caller is pinned server-side */
  isHQ = false;

  /**
   * GET returns the provider's نوعها as its NAME; the form selects by catalogue id. When the
   * provider loads before the Relation catalogue does, the name waits here and resolves in the
   * catalogue's subscribe (catalogue race).
   */
  private pendingRelationName?: string;

  charityOptions: DropdownOption[] = [];
  regionOptions: DropdownOption[] = [];
  centerOptions: DropdownOption[] = [];
  countryOptions: DropdownOption[] = [];
  /** Raw country rows (UC-SYS-11) — the options list strips the NID rule columns. */
  private countries: CountryDto[] = [];
  houseOwnershipOptions: DropdownOption[] = [];
  houseStatusOptions: DropdownOption[] = [];
  housingTypeOptions: DropdownOption[] = [];
  incomeTypeOptions: DropdownOption[] = [];
  socialStatusOptions: DropdownOption[] = [];
  relationOptions: DropdownOption[] = [];
  reasonOfRelationOptions: DropdownOption[] = [];

  genderOptions: DropdownOption[] = [];
  deathReasonOptions: DropdownOption[] = [];

  /** العلاقة closed set (epic-7 review P14) — ids are the wire literals stored in MainRelation */
  mainRelationOptions: DropdownOption[] = [];
  /** الحالة الصحية catalogue shared by the ابن / مرافق rows (epic-7 review P12) */
  healthStatusOptions: DropdownOption[] = [];

  /** One catalogue-failure toast per screen life — not one per failed GET (P21) */
  private catalogueFailureNotified = false;

  breadcrumbs: BreadcrumbItem[] = [
    { label: 'common.home', url: '/dashboard' },
    { label: 'families.refugeeTitle', url: '/families/refugees' },
    { label: 'families.refugeeFormTitle' }
  ];

  constructor(
    private fb: FormBuilder,
    private router: Router,
    private route: ActivatedRoute,
    private familyService: FamilyService,
    private charityService: CharityService,
    private auth: AuthService,
    private lookupService: LookupManagementService,
    private notification: NotificationService,
    private translate: TranslateService,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    this.isHQ = this.auth.hasAnyRole(['SuperAdmin', 'Admin']);

    // UC-REF-04: refugees/:id/edit reuses this screen in edit mode
    const routeId = this.route.snapshot.paramMap.get('id');
    if (routeId) {
      this.editMode = true;
      this.familyId = routeId;
      this.formTitleKey = 'families.refugeeEditTitle';
      this.formSubtitleKey = 'families.refugeeEditSubtitle';
      this.breadcrumbs = [
        { label: 'common.home', url: '/dashboard' },
        { label: 'families.refugeeTitle', url: '/families/refugees' },
        { label: 'families.refugeeEditTitle' }
      ];
    }

    this.familyForm = this.fb.group({
      // الجمعية + الكود are create-time only — hidden and unvalidated in edit mode
      charityId: [null, this.isHQ && !this.editMode ? Validators.required : []],
      code: [''],
      regionId: [null, Validators.required],
      centerId: [null, Validators.required],
      cityVillage: ['', Validators.required],
      nearBy: [''],
      street: [''],
      address: ['', Validators.required],
      phoneNumber: [''],
      houseOwnershipId: [null, Validators.required],
      houseStatusId: [null, Validators.required],
      housingTypeId: [null, Validators.required],
      incomeTypeId: [null, Validators.required],
      // قيمة الإيجار is §12.S.2-mandatory unconditionally (server enforces it flat);
      // the legacy ASP form hid it for non-rented ownership — this form keeps it visible.
      rentAmount: [null, [Validators.required, Validators.min(0.01)]],
      notes: [''],
      // Staged اضافة ابن / اضافة مرافق rows ride the family form so the template's
      // formArrayName="orphans"/"relatives" binds and save() reads .value off one tree
      orphans: this.fb.array([]),
      relatives: this.fb.array([])
    });

    this.providerForm = this.fb.group({
      fullName: ['', [Validators.required, Validators.minLength(5)]],
      nationalId: ['', Validators.required],
      dateOfBirth: ['', Validators.required],
      nationalityCountryId: [null, Validators.required],
      // العلاقة (P14) — الاب / الام / علاقة أخرى; «علاقة أخرى» reveals the نوعها catalogue select
      mainRelation: [null, Validators.required],
      relationshipToFamily: [null, Validators.required],
      reasonOfRelationId: [null, Validators.required],
      phone: [''],
      job: [''],
      monthlyIncome: [null],
      isAlive: [true],
      deathDate: [''],
      deathReason: ['']
    });

    this.childrenForm = this.familyForm.get('orphans') as FormArray;
    this.companionsForm = this.familyForm.get('relatives') as FormArray;

    this.initializeStaticOptions();
    this.langChangeSubscription = this.translate.onLangChange.subscribe(() => {
      this.initializeStaticOptions();
      this.cdr.markForCheck();
    });

    // المنطقة → المركز cascade — changing the region empties the center choice
    this.familyForm.get('regionId')!.valueChanges.pipe(takeUntil(this.destroy$)).subscribe(regionId => {
      this.familyForm.get('centerId')!.setValue(null);
      this.loadCenters(regionId);
    });

    this.providerForm.get('isAlive')!.valueChanges.pipe(takeUntil(this.destroy$)).subscribe(alive => {
      this.providerIsAlive = alive;
      this.cdr.markForCheck();
    });

    // UC-SYS-11 — the provider NID format follows the chosen nationality country's rules
    this.providerForm.get('nationalityCountryId')!.valueChanges.pipe(takeUntil(this.destroy$)).subscribe(() => {
      this.applyNationalIdRules();
    });

    // العلاقة (P14) — the نوعها (Relation catalogue) select is only meaningful for «علاقة أخرى»
    this.providerForm.get('mainRelation')!.valueChanges.pipe(takeUntil(this.destroy$)).subscribe(relation => {
      this.showRelationCatalogue = relation === 'علاقة أخرى';
      const relationControl = this.providerForm.get('relationshipToFamily')!;
      if (this.showRelationCatalogue) {
        relationControl.setValidators(Validators.required);
      } else {
        relationControl.clearValidators();
        relationControl.setValue(null, { emitEvent: false });
      }
      relationControl.updateValueAndValidity({ emitEvent: false });
      this.cdr.markForCheck();
    });

    this.loadCatalogues();

    if (this.editMode && this.familyId) {
      this.loadExisting(this.familyId);
    }
  }

  /** Row accessors for the shared [fg]-bound inputs inside FormArray rows */
  childGroup(index: number): FormGroup {
    return this.childrenForm.at(index) as FormGroup;
  }

  companionGroup(index: number): FormGroup {
    return this.companionsForm.at(index) as FormGroup;
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
    if (this.langChangeSubscription) {
      this.langChangeSubscription.unsubscribe();
    }
  }

  /** Static closed sets rebuilt on language change — never per CD cycle */
  private initializeStaticOptions(): void {
    this.genderOptions = [
      { id: 'ذكر', name: this.translate.instant('families.refugeeGenderMale') },
      { id: 'انثى', name: this.translate.instant('families.refugeeGenderFemale') }
    ];
    this.deathReasonOptions = [
      { id: 'طبيعية', name: this.translate.instant('families.refugeeDeathNatural') },
      { id: 'مرض', name: this.translate.instant('families.refugeeDeathIllness') },
      { id: 'حادث', name: this.translate.instant('families.refugeeDeathAccident') }
    ];
    // العلاقة (P14) — ids are the wire literals persisted in Provider.MainRelation
    this.mainRelationOptions = [
      { id: 'الاب', name: this.translate.instant('families.refugeeMainRelationFather') },
      { id: 'الام', name: this.translate.instant('families.refugeeMainRelationMother') },
      { id: 'علاقة أخرى', name: this.translate.instant('families.refugeeMainRelationOther') }
    ];
  }

  /** P21 — a failed catalogue GET must not pass silently as an empty dropdown */
  private notifyCatalogueFailure(): void {
    if (this.catalogueFailureNotified) {
      return;
    }
    this.catalogueFailureNotified = true;
    this.notification.error(this.translate.instant('families.refugeeCatalogueLoadFailed'));
  }

  private toOptions(list: any[]): DropdownOption[] {
    return (list || []).map(item => ({
      id: item.id,
      name: item.nameAr || item.name || item.nameEn || String(item.id)
    }));
  }

  private loadCatalogues(): void {
    if (this.isHQ) {
      this.charityService.getCharities({ pageNumber: 1, pageSize: 1000, isActive: true })
        .pipe(takeUntil(this.destroy$))
        .subscribe({
          next: result => {
            this.charityOptions = this.toOptions(result.items || []);
            this.cdr.markForCheck();
          },
          error: () => { this.charityOptions = []; this.notifyCatalogueFailure(); }
        });
    }

    this.lookupService.getCountries({ page: 1, pageSize: 1000, isActive: true })
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: result => {
          this.countries = result.items || [];
          this.countryOptions = this.toOptions(result.items || []);
          this.applyNationalIdRules(); // edit mode may already hold a nationality
          this.cdr.markForCheck();
        },
        error: () => { this.countryOptions = []; this.notifyCatalogueFailure(); }
      });

    this.lookupService.getRegions({ page: 1, pageSize: 1000, isActive: true })
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: result => {
          this.regionOptions = this.toOptions(result.items || []);
          this.cdr.markForCheck();
        },
        error: () => { this.regionOptions = []; this.notifyCatalogueFailure(); }
      });

    this.lookupService.getHouseOwnerships()
      .pipe(takeUntil(this.destroy$))
      .subscribe({ next: r => { this.houseOwnershipOptions = this.toOptions(r); this.cdr.markForCheck(); }, error: () => { this.houseOwnershipOptions = []; this.notifyCatalogueFailure(); } });

    this.lookupService.getHouseStatuses()
      .pipe(takeUntil(this.destroy$))
      .subscribe({ next: r => { this.houseStatusOptions = this.toOptions(r); this.cdr.markForCheck(); }, error: () => { this.houseStatusOptions = []; this.notifyCatalogueFailure(); } });

    // نوع السكن — shared catalogue (GET housing-types via the paged lookup surface)
    this.lookupService.getHousingTypes()
      .pipe(takeUntil(this.destroy$))
      .subscribe({ next: r => { this.housingTypeOptions = this.toOptions(r); this.cdr.markForCheck(); }, error: () => { this.housingTypeOptions = []; this.notifyCatalogueFailure(); } });

    this.lookupService.getIncomeTypes()
      .pipe(takeUntil(this.destroy$))
      .subscribe({ next: r => { this.incomeTypeOptions = this.toOptions(r); this.cdr.markForCheck(); }, error: () => { this.incomeTypeOptions = []; this.notifyCatalogueFailure(); } });

    this.lookupService.getSocialStatuses()
      .pipe(takeUntil(this.destroy$))
      .subscribe({ next: r => { this.socialStatusOptions = this.toOptions(r); this.cdr.markForCheck(); }, error: () => { this.socialStatusOptions = []; this.notifyCatalogueFailure(); } });

    // الحالة الصحية (P12) — shared by the ابن / مرافق rows
    this.lookupService.getHealthStatuses()
      .pipe(takeUntil(this.destroy$))
      .subscribe({ next: r => { this.healthStatusOptions = this.toOptions(r); this.cdr.markForCheck(); }, error: () => { this.healthStatusOptions = []; this.notifyCatalogueFailure(); } });

    this.lookupService.getRelations()
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: r => {
          this.relationOptions = this.toOptions(r);
          // Catalogue race: a provider that loaded before this list gets its نوعها resolved now
          if (this.pendingRelationName) {
            this.resolveRelationByName(this.pendingRelationName);
          }
          this.cdr.markForCheck();
        },
        error: () => { this.relationOptions = []; this.notifyCatalogueFailure(); }
      });

    this.lookupService.getReasonsOfRelation()
      .pipe(takeUntil(this.destroy$))
      .subscribe({ next: r => { this.reasonOfRelationOptions = this.toOptions(r); this.cdr.markForCheck(); }, error: () => { this.reasonOfRelationOptions = []; this.notifyCatalogueFailure(); } });
  }

  private loadCenters(regionId: number | null): void {
    this.centerOptions = [];
    if (!regionId) {
      this.cdr.markForCheck();
      return;
    }
    this.lookupService.getCentersByRegion(regionId)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: result => {
          this.centerOptions = this.toOptions(result);
          this.cdr.markForCheck();
        },
        error: () => {
          this.centerOptions = [];
          this.cdr.markForCheck();
        }
      });
  }

  // ==================== UC-REF-04: edit mode load ====================

  /** yyyy-MM-dd for type="date" inputs — API sends full ISO timestamps */
  private toDateString(value?: string | null): string | undefined {
    return value ? value.slice(0, 10) : undefined;
  }

  /** Provider نوعها travels as its NAME; the control holds a Relation catalogue id */
  private resolveRelationByName(name: string): void {
    const match = this.relationOptions.find(o => o.name === name);
    if (match) {
      this.providerForm.get('relationshipToFamily')!.setValue(match.id, { emitEvent: false });
      this.pendingRelationName = undefined;
    } else {
      this.pendingRelationName = name;
    }
  }

  private loadExisting(familyId: string): void {
    this.familyService.getFamily(familyId)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: family => {
          // P16 — this screen is the refugee register's editor; a non-Refugee family never
          // gets its fields patched into the refugee contract
          if ((family.familyType || '') !== 'Refugee') {
            this.notification.warning(this.translate.instant('families.refugeeNotRefugeeFamily'));
            this.router.navigate(['/families/refugees']);
            return;
          }
          // regionId patched silent — the cascade subscription must not wipe centerId
          this.familyForm.patchValue({
            cityVillage: family.cityVillage,
            nearBy: family.nearBy,
            street: family.street,
            address: family.address,
            phoneNumber: family.phoneNumber,
            regionId: family.regionId,
            centerId: family.centerId,
            houseOwnershipId: family.houseOwnershipId,
            houseStatusId: family.houseStatusId,
            housingTypeId: family.housingTypeId,
            incomeTypeId: family.incomeTypeId,
            rentAmount: family.rentAmount,
            notes: family.notes
          }, { emitEvent: false });
          if (family.regionId) {
            this.loadCenters(family.regionId);
          }
          this.cdr.markForCheck();
        },
        error: () => this.notification.error(this.translate.instant('families.refugeeFormLoadFailed'))
      });

    this.familyService.getProvider(familyId)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: provider => {
          this.providerForm.patchValue({
            fullName: provider.fullName,
            nationalId: provider.nationalId,
            dateOfBirth: this.toDateString(provider.dateOfBirth),
            nationalityCountryId: provider.nationalityCountryId,
            reasonOfRelationId: provider.reasonOfRelationId,
            phone: provider.phone,
            job: provider.job,
            monthlyIncome: provider.monthlyIncome,
            isAlive: provider.isAlive ?? true,
            deathDate: this.toDateString(provider.deathDate),
            deathReason: provider.deathReason
          }, { emitEvent: false });
          if (provider.isAlive === false) {
            this.providerIsAlive = false;
          }
          // العلاقة (P14) — a stored الاب/الام restores as-is; anything else (incl. a legacy
          // empty MainRelation with a catalogue نوعها) lands on «علاقة أخرى»
          const storedMain = provider.mainRelation;
          const mainRelation = storedMain === 'الاب' || storedMain === 'الام' ? storedMain : 'علاقة أخرى';
          this.providerForm.get('mainRelation')!.setValue(mainRelation, { emitEvent: false });
          this.showRelationCatalogue = mainRelation === 'علاقة أخرى';
          const relationControl = this.providerForm.get('relationshipToFamily')!;
          if (this.showRelationCatalogue) {
            relationControl.setValidators(Validators.required);
            if (provider.relationshipToFamily) {
              this.resolveRelationByName(provider.relationshipToFamily);
            }
          } else {
            relationControl.clearValidators();
          }
          relationControl.updateValueAndValidity({ emitEvent: false });
          this.cdr.markForCheck();
        },
        error: () => this.notification.error(this.translate.instant('families.refugeeFormLoadFailed'))
      });

    this.familyService.getFamilyOrphans(familyId)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: orphans => {
          orphans.forEach(o => {
            this.childrenForm.push(this.fb.group({
              id: [o.id],
              fullName: [o.fullName, Validators.required],
              dateOfBirth: [this.toDateString(o.dateOfBirth), Validators.required],
              gender: [o.gender || 'ذكر', Validators.required],
              // No static length cap — the country rules govern NID length when present
              // (UC-SYS-11 AC 4: no rules → free-form; review P5, 2026-08-26).
              nationalId: [o.nationalId || '', [Validators.required]],
              socialStatusId: [o.socialStatusId, Validators.required],
              healthStatusId: [o.healthStatusId ?? null]
            }));
          });
          this.cdr.markForCheck();
        },
        error: () => this.notification.error(this.translate.instant('families.refugeeFormLoadFailed'))
      });

    this.familyService.getRelatives(familyId)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: relatives => {
          relatives.forEach(r => {
            this.companionsForm.push(this.fb.group({
              id: [r.id],
              fullName: [r.fullName, Validators.required],
              nationalId: [r.nationalId || '', [Validators.required]],
              dateOfBirth: [this.toDateString(r.dateOfBirth), Validators.required],
              gender: [r.gender || 'ذكر', Validators.required],
              healthStatusId: [r.healthStatusId ?? null],
              // صلة القرابة lives in the relative's notes on the wire
              relationship: [r.notes || '', Validators.required]
            }));
          });
          this.cdr.markForCheck();
        },
        error: () => this.notification.error(this.translate.instant('families.refugeeFormLoadFailed'))
      });
  }

  // ==================== اضافة ابن (children staging) ====================

  addChildRow(): void {
    this.childrenForm.push(this.fb.group({
      // id is set only in edit mode (UC-REF-04) — null means "create on save"
      id: [null],
      fullName: ['', Validators.required],
      dateOfBirth: ['', Validators.required],
      gender: ['ذكر', Validators.required],
      nationalId: ['', [Validators.required]],
      socialStatusId: [null, Validators.required],
      healthStatusId: [null]
    }));
    this.cdr.markForCheck();
  }

  removeChildRow(index: number): void {
    const rowId = this.childrenForm.at(index).get('id')?.value;
    if (this.editMode && rowId) {
      this.deletedOrphanIds.push(rowId);
    }
    this.childrenForm.removeAt(index);
    this.cdr.markForCheck();
  }

  /** P22 — track staged rows by their id control (edit rows keep identity across reorder;
   *  new rows fall back to their index until the server assigns an id) */
  trackByRowId = (index: number, row: AbstractControl): string => row.get('id')?.value ?? `new-${index}`;

  // ==================== اضافة مرافق (companions staging) ====================

  addCompanionRow(): void {
    this.companionsForm.push(this.fb.group({
      // id is set only in edit mode (UC-REF-04) — null means "create on save"
      id: [null],
      fullName: ['', Validators.required],
      nationalId: ['', [Validators.required]],
      // Server contract (CreateRelativeDto) demands both — the shared legacy DTO
      // is [Required] on Gender and DateOfBirth, so the row collects them.
      dateOfBirth: ['', Validators.required],
      gender: ['ذكر', Validators.required],
      healthStatusId: [null],
      relationship: ['', Validators.required]
    }));
    this.cdr.markForCheck();
  }

  removeCompanionRow(index: number): void {
    const rowId = this.companionsForm.at(index).get('id')?.value;
    if (this.editMode && rowId) {
      this.deletedRelativeIds.push(rowId);
    }
    this.companionsForm.removeAt(index);
    this.cdr.markForCheck();
  }

  // ==================== submit ====================

  save(): void {
    this.familyForm.markAllAsTouched();
    this.providerForm.markAllAsTouched();
    if (this.familyForm.invalid || this.providerForm.invalid || this.childrenForm.invalid || this.companionsForm.invalid) {
      this.notification.warning(this.translate.instant('families.refugeeFormIncomplete'));
      return;
    }

    // UC-REF-04: the edit flow saves through the per-member endpoints instead
    // of the single-payload create
    if (this.editMode && this.familyId) {
      this.saveEdit();
      return;
    }

    const family = this.familyForm.value;
    const provider = this.providerForm.value;

    const payload: CreateFamilyDto = {
      familyType: 'Refugee',
      charityId: this.isHQ ? family.charityId : undefined,
      code: family.code || undefined,
      headOfFamily: provider.fullName,
      address: family.address,
      cityVillage: family.cityVillage,
      nearBy: family.nearBy || undefined,
      street: family.street || undefined,
      phoneNumber: family.phoneNumber || undefined,
      regionId: family.regionId,
      centerId: family.centerId,
      rentAmount: family.rentAmount,
      houseOwnershipId: family.houseOwnershipId,
      houseStatusId: family.houseStatusId,
      housingTypeId: family.housingTypeId,
      incomeTypeId: family.incomeTypeId,
      notes: family.notes || undefined,
      provider: {
        fullName: provider.fullName,
        nationalId: provider.nationalId,
        phone: provider.phone || undefined,
        job: provider.job || undefined,
        monthlyIncome: provider.monthlyIncome ?? undefined,
        dateOfBirth: provider.dateOfBirth,
        nationalityCountryId: provider.nationalityCountryId,
        // العلاقة (P14) + نوعها — الاب/الام travel as the literal; «علاقة أخرى» sends the
        // catalogue label (falling back to the stored wire name if the catalogue never loaded)
        mainRelation: provider.mainRelation,
        relationshipToFamily: this.providerRelationOnWire(provider),
        reasonOfRelationId: provider.reasonOfRelationId,
        isAlive: provider.isAlive,
        deathDate: !provider.isAlive ? (provider.deathDate || undefined) : undefined,
        deathReason: !provider.isAlive ? (provider.deathReason || undefined) : undefined
      },
      orphans: this.childrenForm.controls.map(row => {
        const child = row.value;
        return {
          fullName: child.fullName,
          dateOfBirth: child.dateOfBirth,
          gender: child.gender,
          nationalId: child.nationalId || undefined,
          socialStatusId: child.socialStatusId,
          healthStatusId: child.healthStatusId ?? undefined
        };
      }),
      relatives: this.companionsForm.controls.map(row => {
        const companion = row.value;
        return {
          fullName: companion.fullName,
          relationshipType: 'accompany',
          gender: companion.gender,
          dateOfBirth: companion.dateOfBirth,
          nationalId: companion.nationalId || undefined,
          healthStatusId: companion.healthStatusId ?? undefined,
          notes: companion.relationship,
          isAlive: true,
          isLivingWithFamily: true
        };
      })
    };

    this.saving = true;
    this.familyService.createFamily(payload)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
      next: () => {
        this.saving = false;
        this.notification.success(this.translate.instant('families.refugeeFormSaved'));
        this.router.navigate(['/families/refugees']);
      },
      error: (error: any) => {
        this.saving = false;
        this.cdr.markForCheck();
        // Surface the server's field-level refusals (§12.S.2 flags / duplicate معيل)
        const fieldErrors = this.extractFieldErrors(error);
        if (fieldErrors.length) {
          this.notification.error(fieldErrors.join(' — '));
        } else {
          this.notification.error(error?.message || this.translate.instant('families.refugeeFormSaveFailed'));
        }
      }
    });
  }

  /**
   * UC-REF-04 edit save: family PUT → provider PUT → per-member orphan/relative
   * create/update/delete. The server re-validates the §12.S.2 mandatory flags on the
   * family's effective post-copy state and the duplicate-معيل rule on the provider's
   * new national id (excluding itself).
   */
  private saveEdit(): void {
    const family = this.familyForm.value;
    const provider = this.providerForm.value;

    const familyPayload: UpdateFamilyDto = {
      address: family.address,
      cityVillage: family.cityVillage,
      nearBy: family.nearBy || undefined,
      street: family.street || undefined,
      phoneNumber: family.phoneNumber || undefined,
      regionId: family.regionId,
      centerId: family.centerId,
      rentAmount: family.rentAmount,
      houseOwnershipId: family.houseOwnershipId,
      houseStatusId: family.houseStatusId,
      housingTypeId: family.housingTypeId,
      incomeTypeId: family.incomeTypeId,
      notes: family.notes || undefined
    };

    const providerPayload: UpdateProviderDto = {
      fullName: provider.fullName,
      nationalId: provider.nationalId,
      phone: provider.phone || undefined,
      job: provider.job || undefined,
      monthlyIncome: provider.monthlyIncome ?? undefined,
      dateOfBirth: provider.dateOfBirth,
      nationalityCountryId: provider.nationalityCountryId,
      // العلاقة (P14) + نوعها — same wire rule as the create path
      mainRelation: provider.mainRelation,
      relationshipToFamily: this.providerRelationOnWire(provider),
      reasonOfRelationId: provider.reasonOfRelationId,
      isAlive: provider.isAlive,
      deathDate: !provider.isAlive ? (provider.deathDate || undefined) : undefined,
      deathReason: !provider.isAlive ? (provider.deathReason || undefined) : undefined
    };

    this.saving = true;
    this.familyService.updateFamily(this.familyId!, familyPayload).pipe(
      switchMap(() => this.familyService.updateProvider(this.familyId!, providerPayload)),
      switchMap(() => {
        const ops: Observable<any>[] = [];

        this.childrenForm.controls.forEach(row => {
          const child = row.value;
          if (child.id) {
            const orphanPayload: UpdateOrphanDto = {
              fullName: child.fullName,
              dateOfBirth: child.dateOfBirth,
              gender: child.gender,
              nationalId: child.nationalId || undefined,
              socialStatusId: child.socialStatusId,
              healthStatusId: child.healthStatusId ?? undefined
            };
            ops.push(this.familyService.updateOrphan(child.id, orphanPayload));
          } else {
            ops.push(this.familyService.createOrphan({
              familyId: this.familyId,
              fullName: child.fullName,
              dateOfBirth: child.dateOfBirth,
              gender: child.gender,
              nationalId: child.nationalId || undefined,
              socialStatusId: child.socialStatusId,
              healthStatusId: child.healthStatusId ?? undefined
            }));
          }
        });

        this.companionsForm.controls.forEach(row => {
          const companion = row.value;
          if (companion.id) {
            const relativePayload: UpdateRelativeDto = {
              id: companion.id,
              fullName: companion.fullName,
              relationshipType: 'accompany',
              gender: companion.gender,
              dateOfBirth: companion.dateOfBirth,
              nationalId: companion.nationalId || undefined,
              healthStatusId: companion.healthStatusId ?? undefined,
              notes: companion.relationship,
              isAlive: true,
              isLivingWithFamily: true
            };
            ops.push(this.familyService.updateRelative(companion.id, relativePayload));
          } else {
            ops.push(this.familyService.createRelative(this.familyId!, {
              fullName: companion.fullName,
              relationshipType: 'accompany',
              gender: companion.gender,
              dateOfBirth: companion.dateOfBirth,
              nationalId: companion.nationalId || undefined,
              healthStatusId: companion.healthStatusId ?? undefined,
              notes: companion.relationship,
              isAlive: true,
              isLivingWithFamily: true
            }));
          }
        });

        this.deletedOrphanIds.forEach(id => ops.push(this.familyService.deleteOrphan(id)));
        this.deletedRelativeIds.forEach(id => ops.push(this.familyService.deleteRelative(this.familyId!, id)));

        return ops.length ? forkJoin(ops) : of([]);
      }),
      takeUntil(this.destroy$)
    ).subscribe({
      next: () => {
        this.saving = false;
        this.notification.success(this.translate.instant('families.refugeeFormSaved'));
        this.router.navigate(['/families/refugees']);
      },
      error: (error: any) => {
        this.saving = false;
        this.cdr.markForCheck();
        // Surface the server's field-level refusals (§12.S.2 flags / duplicate معيل)
        const fieldErrors = this.extractFieldErrors(error);
        if (fieldErrors.length) {
          this.notification.error(fieldErrors.join(' — '));
        } else {
          this.notification.error(error?.message || this.translate.instant('families.refugeeFormSaveFailed'));
        }
      }
    });
  }

  private extractFieldErrors(error: any): string[] {
    const items = error?.modelStateErrors || error?.error?.modelStateErrors;
    if (Array.isArray(items)) {
      return items.map((e: any) => e?.value || e?.message || '').filter(Boolean);
    }
    if (error?.message && typeof error.message === 'string' && /[؀-ۿ]/.test(error.message)) {
      return [error.message];
    }
    return [];
  }

  private optionName(options: DropdownOption[], id: number | string | null): string | undefined {
    if (id === null || id === undefined) {
      return undefined;
    }
    return options.find(o => o.id === id)?.name;
  }

  /**
   * P14/P21 — the provider's نوعها on the wire: الاب/الام travel literally; «علاقة أخرى»
   * resolves the Relation catalogue selection, falling back to the loaded wire name when
   * the catalogue never arrived (never send undefined and silently wipe the stored relation).
   */
  private providerRelationOnWire(provider: { mainRelation?: string | null; relationshipToFamily?: any }): string | undefined {
    if (provider.mainRelation === 'الاب' || provider.mainRelation === 'الام') {
      return provider.mainRelation;
    }
    return this.optionName(this.relationOptions, provider.relationshipToFamily) || this.pendingRelationName;
  }

  /**
   * UC-SYS-11 — re-apply the provider-NID format rules for the chosen nationality country.
   * A country with no rules keeps the field free-form (required only); a ruled country adds
   * the dedicated nationalIdRuleValidator (surfaced as validation.nationalIdInvalid).
   */
  private applyNationalIdRules(): void {
    const nidControl = this.providerForm.get('nationalId');
    if (!nidControl) {
      return;
    }
    const raw = this.providerForm.get('nationalityCountryId')?.value;
    const countryId = typeof raw === 'string' ? parseInt(raw, 10) : raw;
    const country = this.countries.find(c => c.id === countryId);
    if (country && (country.nationalIdPattern || country.nationalIdLength)) {
      nidControl.setValidators([
        Validators.required,
        nationalIdRuleValidator(country.nationalIdPattern, country.nationalIdLength)
      ]);
    } else {
      nidControl.setValidators([Validators.required]);
    }
    nidControl.updateValueAndValidity();
    this.cdr.markForCheck();
  }

  cancel(): void {
    this.router.navigate(['/families/refugees']);
  }
}

/**
 * UC-SYS-11 — national-id format validator driven by the country catalogue row's
 * optional pattern/length. Raises the dedicated 'nationalIdInvalid' error key (rendered as
 * validation.nationalIdInvalid by the shared text input). Kept local + copyable so other
 * host forms (family form, EP-05) can adopt it without a framework.
 */
export function nationalIdRuleValidator(pattern?: string | null, length?: number | null): ValidatorFn {
  // A malformed admin-entered pattern must degrade to length-only validation, never
  // throw at wiring time and crash the host form (review P4, 2026-08-26).
  let regex: RegExp | null = null;
  if (pattern) {
    try {
      regex = new RegExp(pattern);
    } catch {
      regex = null; // invalid pattern — treat as "no rule" (the AC 4 posture)
    }
  }
  return (control: AbstractControl): ValidationErrors | null => {
    const value = control.value;
    if (value === null || value === undefined || value === '') {
      return null; // emptiness is 'required's concern, not the format rule's
    }
    if (regex && !regex.test(String(value))) {
      return { nationalIdInvalid: true };
    }
    if (length && String(value).length !== length) {
      return { nationalIdInvalid: true };
    }
    return null;
  };
}
