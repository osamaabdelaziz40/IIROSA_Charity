/**
 * Housing Family Register Form (UC-HOU-03 + UC-HOU-04 · §11.S.2)
 * Add mode (6-3): registers a housing family — POST /api/HousingProjects/projects.
 * Edit mode (6-4, route :id/edit): loads the aggregate (GET projects/{id}), re-fills every
 * §11.S.2 section and saves through PUT projects/{id} — the server syncs family fields,
 * guardian and children (id-matched update, id-less add, absent soft-remove).
 *
 * Field order, labels and mandatory flags follow §11.S.2 verbatim — it is the contract;
 * the server re-validates everything (CreateHousingFamilyValidator + flat⊆building rule).
 * Roles: Charity + HQ (Admin/SuperAdmin). HQ names the charity on CREATE only — ownership
 * never moves, so in edit mode the charity + register-code controls are locked (the code is
 * immutable on the update path and any charity field is ignored server-side).
 */

import { Component, OnInit, OnDestroy, ChangeDetectionStrategy, ChangeDetectorRef, ViewChildren, QueryList } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, FormArray, Validators, ReactiveFormsModule } from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';

import { HousingProjectService } from '../services/housing-project.service';
import {
  CreateHousingFamilyRequest,
  CreateHousingGuardianRequest,
  CreateHousingChildRequest,
  HousingFamilyDetail,
  HousingPhoneRow,
  HOUSING_MAIN_RELATIONS,
  HOUSING_CHILD_GENDERS,
  HOUSING_PHONE_BELONGS_TO
} from '../models/housing-project.model';
import { LookupManagementService } from '../../lookup-management/services/lookup-management.service';
import { CharityService } from '../../charities/services/charity.service';
import { AuthService } from '../../../core/services/auth.service';
import { NotificationService } from '../../../core/services/notification.service';
import { AttachmentService } from '../../../core/services/attachment.service';
import {
  BreadcrumbComponent,
  BreadcrumbItem,
  PageHeaderComponent,
  CollapsibleCardComponent
} from '../../../shared/components';
import { AttachmentComponent } from '../../../shared/components/attachment/attachment.component';
import { SharedModule } from '../../../shared/shared.module';

/** { id, name } option shape the shared app-drop-down expects (LookupBase). */
interface DropdownOption {
  id: number | string;
  name: string;
}

/** One captured child row while the form is open (payload rows are cut from these). */
interface ChildRow {
  /** UC-HOU-04 edit-sync key — present on rows loaded from the family, absent on new rows. */
  id?: string;
  fullName: string;
  dateOfBirth: string;
  nationalId: string;
  gender: string;
  healthStatusId: number | null;
  socialStatusId: number | null;
  educationLevelId: number | null;
  /** «حاصل على مؤهل دراسى» — EducationLevel lookup (review D3 2026-08-24, §11.S.2) */
  educationalQualificationId: number | null;
  gradeClass: string;
  profession: string;
  departmentName: string;
  facultyName: string;
  schoolName: string;
  /** الصوره الشخصيه — attachment id (§11.S.2 اضافة ابن uploads) */
  photoAttachmentId?: string | null;
  /** صوره شهاده الميلاد — attachment id */
  birthCertificateAttachmentId?: string | null;
  /** صوره إثبات القيد — attachment id */
  enrollmentAttachmentId?: string | null;
  notes: string;
}

/** One captured guardian row (§11.S.2 اضافة الاباء — AddNewParent() always): the entry
 *  form is a staging area — تم pushes/updates a row, and the rows drive the payload. */
interface GuardianRow {
  /** UC-HOU-04 edit-sync key — present on rows loaded from the family, absent on new rows. */
  id?: string;
  value: GuardianFormValue;
}

/** The guardian entry form's raw value (name composed from أول/ثانى/ثالث/رباعي parts). */
interface GuardianFormValue {
  reasonOfRelationId: number | null;
  relationId: number | null;
  mainRelation: string | null;
  firstName: string;
  secondName: string;
  thirdName: string;
  fourthName: string;
  familyName: string;
  nationalityCountryId: number | null;
  dateOfBirth: string;
  nationalId: string;
  job: string;
  phone: string;
  /** number input — the control yields '' while empty, hence the string union */
  monthlyIncome: number | string | null;
  educationLevelId: number | null;
  socialStatusId: number | null;
  healthStatusId: number | null;
  widowSponsorship: boolean;
  anotherSponsor: boolean;
  motherIsMar: boolean;
  isCaring: boolean;
}

@Component({
  selector: 'app-housing-project-form',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    TranslateModule,
    BreadcrumbComponent,
    PageHeaderComponent,
    AttachmentComponent,
    SharedModule
  ],
  templateUrl: './housing-project-form.component.html',
  styleUrls: ['./housing-project-form.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class HousingProjectFormComponent implements OnInit, OnDestroy {
  private destroy$ = new Subject<void>();

  breadcrumbs: BreadcrumbItem[] = [
    { label: 'common.home', url: '/dashboard' },
    { label: 'housingProjects.title', url: '/housing-projects' },
    { label: 'housingProjects.addFamily' }
  ];

  /** UC-HOU-04 edit mode (route :id/edit) — the form loads the aggregate and saves via PUT. */
  editMode = false;
  familyId: string | null = null;
  loading = false;

  // Collapsible section cards — sections whose header carries extra content
  // (edit note / add-row button / saved badge) collapse manually; the plain
  // ones use the shared app-collapsible-card. A failed submit re-expands all.
  sections: Record<'family' | 'phones' | 'guardianRelation', boolean> = {
    family: false,
    phones: false,
    guardianRelation: false
  };

  @ViewChildren(CollapsibleCardComponent) collapsibleCards?: QueryList<CollapsibleCardComponent>;

  pageActions = [
    {
      label: 'housingProjects.form.actions.save',
      type: 'primary',
      icon: 'fe-check',
      click: () => this.save()
    },
    {
      label: 'common.cancel',
      type: 'secondary',
      icon: 'fe-x',
      click: () => this.router.navigate(['/housing-projects'])
    }
  ];

  // Roles — HQ picks the charity; a Charity caller is pinned server-side
  isHQ = false;
  saving = false;

  // §11.S.2 closed sets (values are the Arabic literals the server stores), rendered
  // through the shared select2 drop-down — the option ids ARE the Arabic literals.
  mainRelationSelectOptions: DropdownOption[] = [];
  genderSelectOptions: DropdownOption[] = [];
  phoneBelongsToSelectOptions: DropdownOption[] = [];

  // Drop-down catalogues
  charities: DropdownOption[] = [];
  countries: DropdownOption[] = [];
  regions: DropdownOption[] = [];
  centers: DropdownOption[] = [];
  buildings: DropdownOption[] = [];
  flats: DropdownOption[] = [];
  incomeTypes: DropdownOption[] = [];
  educationLevels: DropdownOption[] = [];
  healthStatuses: DropdownOption[] = [];
  socialStatuses: DropdownOption[] = [];
  relations: DropdownOption[] = [];
  reasonsOfRelation: DropdownOption[] = [];

  // بيانات الأسرة
  familyForm!: FormGroup;

  // Phones grid — client-side rows; the default row's number reaches the payload
  phoneArray!: FormArray;

  // اضافة معيل / اضافة الاباء — entry form (staging) + captured rows (payload source)
  guardianForm!: FormGroup;
  guardianSaved = false;
  guardians: GuardianRow[] = [];
  editingGuardianIndex = -1;

  // اضافة ابن
  childForm!: FormGroup;
  children: ChildRow[] = [];
  editingChildIndex = -1;

  constructor(
    private fb: FormBuilder,
    private housingService: HousingProjectService,
    private lookupService: LookupManagementService,
    private charityService: CharityService,
    private auth: AuthService,
    private notification: NotificationService,
    private attachmentService: AttachmentService,
    private translate: TranslateService,
    private router: Router,
    private route: ActivatedRoute,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    this.isHQ = this.auth.hasRole('SuperAdmin') || this.auth.hasRole('Admin');

    // Closed-set select2 options are translated snapshots — rebuild on language switch.
    this.rebuildTranslatedOptions();
    this.translate.onLangChange
      .pipe(takeUntil(this.destroy$))
      .subscribe(() => this.rebuildTranslatedOptions());

    // UC-HOU-04: :id/edit → edit mode (load the aggregate); create → the 6-3 add flow.
    // Resolved BEFORE buildForms — the guardian name-part validators read editMode
    // (legacy <4-token names must not hard-fail the form in edit mode; see buildForms).
    const routeId = this.route.snapshot.paramMap.get('id');
    this.editMode = !!routeId;
    this.familyId = routeId;
    if (this.editMode) {
      this.breadcrumbs[2].label = 'housingProjects.editFamily';
    }

    this.buildForms();
    this.loadCatalogues();
    if (this.editMode && this.familyId) {
      this.loadFamily(this.familyId);
    }
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  // ==================== FORM CONSTRUCTION ====================

  private buildForms(): void {
    // بيانات الأسرة — §11.S.2 order; charity control only enforced for HQ callers
    this.familyForm = this.fb.group({
      charityId: [null, this.isHQ ? Validators.required : []],
      code: [''],
      cityVillage: ['', Validators.required],
      countryId: [null, Validators.required],
      regionId: [null, Validators.required],
      centerId: [null, Validators.required],
      housingBuildingId: [null, Validators.required],
      housingFlatId: [null, Validators.required],
      nearBy: [''],
      street: [''],
      address: ['', Validators.required],
      rentAmount: [null, Validators.required],
      incomeTypeId: [null, Validators.required],
      notes: ['']
    });

    this.phoneArray = this.fb.array([this.createPhoneRow(true)]);

    // اضافة معيل — §11.S.2 order (name composed from أول/ثانى/ثالث/رباعي parts)
    this.guardianForm = this.fb.group({
      reasonOfRelationId: [null, Validators.required],
      relationId: [null, Validators.required],
      mainRelation: [null, Validators.required],
      firstName: ['', Validators.required],
      secondName: ['', Validators.required],
      // Review 2026-08-24: §11.S.2 mandates all four parts on CREATE, but stored legacy
      // guardians can carry <4-token full names — decomposeName pads the missing parts
      // with '' and hard-required controls made those families unsavable in edit mode.
      // Edit keeps أول/ثانى required and relaxes ثالث/رباعي (compose filters empties, so
      // an untouched legacy name round-trips losslessly).
      thirdName: ['', this.editMode ? [] : Validators.required],
      fourthName: ['', this.editMode ? [] : Validators.required],
      familyName: [''],
      nationalityCountryId: [null, Validators.required],
      dateOfBirth: ['', Validators.required],
      nationalId: ['', Validators.required],
      job: ['', Validators.required],
      phone: ['', Validators.required],
      monthlyIncome: [null],
      educationLevelId: [null, Validators.required],
      socialStatusId: [null, Validators.required],
      healthStatusId: [null, Validators.required],
      widowSponsorship: [false],
      anotherSponsor: [false],
      motherIsMar: [false],
      isCaring: [false]
    });

    // اضافة ابن — §11.S.2 order
    this.childForm = this.fb.group({
      dateOfBirth: ['', Validators.required],
      nationalId: ['', [Validators.required, Validators.maxLength(14)]],
      fullName: ['', Validators.required],
      gender: [null, Validators.required],
      healthStatusId: [null, Validators.required],
      socialStatusId: [null, Validators.required],
      educationLevelId: [null, Validators.required],
      // «حاصل على مؤهل دراسى» — review D3 (2026-08-24): spec-mandatory §11.S.2 child field;
      // reuses the EducationLevel catalogue, mirrored by CreateHousingFamilyValidator.
      educationalQualificationId: [null, Validators.required],
      gradeClass: ['', Validators.required],
      profession: ['', Validators.required],
      departmentName: ['', Validators.required],
      facultyName: ['', Validators.required],
      schoolName: ['', Validators.required],
      // §11.S.2 اضافة ابن attachments (shared attachment component) — the ids ride the
      // child rows; edit keeps a stored id unless the user removes/replaces the file.
      photoAttachmentId: [null],
      birthCertificateAttachmentId: [null],
      enrollmentAttachmentId: [null],
      notes: ['']
    });
  }

  private createPhoneRow(isDefault = false): FormGroup {
    // §11.S.2 phone modal: number mandatory, BelongsTo drop-down, IsDefaultPhone checkbox.
    // The seed row passes isDefault = true explicitly — the old `this.phoneArray?.length === 0`
    // guess evaluated while phoneArray was still undefined (createPhoneRow runs BEFORE the
    // field is assigned), so the first row never defaulted (review 2026-08-24).
    return this.fb.group({
      number: ['', Validators.required],
      belongsTo: [null],
      isDefault: [isDefault]
    });
  }

  get phoneRows(): FormGroup[] {
    return this.phoneArray.controls as FormGroup[];
  }

  // ==================== CATALOGUE LOADING ====================

  /** Map any lookup DTO onto the shared drop-down option shape (Arabic name first). */
  private toOptions(list: any[]): DropdownOption[] {
    return (list || []).map(item => ({
      id: item.id,
      name: item.nameAr || item.name || item.nameEn || String(item.id)
    }));
  }

  private loadCatalogues(): void {
    // OnPush: these HTTP callbacks mutate plain fields (option lists) outside Angular's
    // event stream — every arm must schedule a check or the drop-downs render empty until
    // an unrelated click (review 2026-08-24).
    const done = () => this.cdr.markForCheck();

    if (this.isHQ) {
      this.charityService.getCharities({ pageNumber: 1, pageSize: 1000, isActive: true })
        .pipe(takeUntil(this.destroy$))
        .subscribe({
          next: result => { this.charities = this.toOptions(result.items || []); done(); },
          error: () => { this.charities = []; done(); }
        });
    }

    this.lookupService.getCountries({ page: 1, pageSize: 1000, isActive: true })
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: result => { this.countries = this.toOptions(result.items || []); done(); },
        error: () => { this.countries = []; done(); }
      });

    // Regions cascade from the country (getRegionsByCountry) — no unconditional load here.

    this.lookupService.getHousingBuildings()
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: result => { this.buildings = this.toOptions(result); done(); },
        error: () => { this.buildings = []; done(); }
      });

    this.lookupService.getIncomeTypes()
      .pipe(takeUntil(this.destroy$))
      .subscribe({ next: r => { this.incomeTypes = this.toOptions(r); done(); }, error: () => { this.incomeTypes = []; done(); } });

    this.lookupService.getEducationLevels()
      .pipe(takeUntil(this.destroy$))
      .subscribe({ next: r => { this.educationLevels = this.toOptions(r); done(); }, error: () => { this.educationLevels = []; done(); } });

    this.lookupService.getHealthStatuses()
      .pipe(takeUntil(this.destroy$))
      .subscribe({ next: r => { this.healthStatuses = this.toOptions(r); done(); }, error: () => { this.healthStatuses = []; done(); } });

    this.lookupService.getSocialStatuses()
      .pipe(takeUntil(this.destroy$))
      .subscribe({ next: r => { this.socialStatuses = this.toOptions(r); done(); }, error: () => { this.socialStatuses = []; done(); } });

    this.lookupService.getRelations()
      .pipe(takeUntil(this.destroy$))
      .subscribe({ next: r => { this.relations = this.toOptions(r); done(); }, error: () => { this.relations = []; done(); } });

    this.lookupService.getReasonsOfRelation()
      .pipe(takeUntil(this.destroy$))
      .subscribe({ next: r => { this.reasonsOfRelation = this.toOptions(r); done(); }, error: () => { this.reasonsOfRelation = []; done(); } });
  }

  // ==================== EDIT MODE LOAD (UC-HOU-04) ====================

  /** Load the aggregate and re-fill every §11.S.2 section for the update round-trip. */
  private loadFamily(id: string): void {
    this.loading = true;
    this.housingService.getHousingFamily(id)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: detail => {
          this.loading = false;
          this.patchFromDetail(detail);
          this.cdr.markForCheck(); // OnPush — loading/children/forms changed off-event
        },
        error: err => {
          // 404 carries { message } — unknown/non-housing/foreign all look alike here.
          this.loading = false;
          this.cdr.markForCheck();
          this.notification.error(
            err?.error?.message ||
            this.translate.instant('housingProjects.form.messages.loadFailed'));
          this.router.navigate(['/housing-projects']);
        }
      });
  }

  private patchFromDetail(detail: HousingFamilyDetail): void {
    // بيانات الأسرة — charity + code are locked in edit mode (ownership never moves,
    // the register code is immutable on the update path). Disabled controls drop out of
    // familyForm.value at save — exactly what the PUT contract wants.
    this.familyForm.patchValue({
      charityId: detail.charityId || null,
      code: detail.code || '',
      cityVillage: detail.cityVillage || '',
      countryId: detail.countryId ?? null,
      regionId: detail.regionId ?? null,
      centerId: detail.centerId ?? null,
      housingBuildingId: detail.housingBuildingId ?? null,
      housingFlatId: detail.housingFlatId ?? null,
      nearBy: detail.nearBy || '',
      street: detail.street || '',
      address: detail.address || '',
      rentAmount: detail.rentAmount ?? null,
      incomeTypeId: detail.incomeTypeId ?? null,
      notes: detail.notes || ''
    });
    if (this.isHQ) {
      this.familyForm.get('charityId')?.disable();
    }
    this.familyForm.get('code')?.disable();

    // Option lists for the patched cascade selections (no reset — the family owns them)
    this.loadRegionsByCountry(detail.countryId ?? null);
    this.loadCenters(detail.regionId ?? null);
    this.loadFlats(detail.housingBuildingId ?? null);

    // Phones grid — the platform stores one family phone; it lands as the default row.
    this.phoneRows[0].patchValue({
      number: detail.phoneNumber || '',
      isDefault: true
    });

    // اضافة الاباء — every stored guardian becomes an editable row carrying its id
    // (legacy detail carries the single provider; new detail carries the full set).
    const guardianDetails =
      detail.providers?.length ? detail.providers : detail.provider ? [detail.provider] : [];
    this.guardians = guardianDetails.map(guardian => {
      const parts = this.decomposeName(guardian.fullName);
      return {
        id: guardian.id,
        value: {
          reasonOfRelationId: guardian.reasonOfRelationId ?? null,
          relationId: guardian.relationId ?? null,
          mainRelation: guardian.mainRelation || null,
          firstName: parts.firstName,
          secondName: parts.secondName,
          thirdName: parts.thirdName,
          fourthName: parts.fourthName,
          familyName: parts.familyName,
          nationalityCountryId: guardian.nationalityCountryId ?? null,
          dateOfBirth: guardian.dateOfBirth ? guardian.dateOfBirth.slice(0, 10) : '',
          nationalId: guardian.nationalId || '',
          job: guardian.job || '',
          phone: guardian.phone || '',
          monthlyIncome: guardian.monthlyIncome ?? null,
          educationLevelId: guardian.educationLevelId ?? null,
          socialStatusId: guardian.socialStatusId ?? null,
          healthStatusId: guardian.healthStatusId ?? null,
          widowSponsorship: !!guardian.widowSponsorship,
          anotherSponsor: !!guardian.anotherSponsor,
          motherIsMar: !!guardian.motherIsMar,
          isCaring: !!guardian.isCaring
        }
      };
    });
    this.guardianSaved = this.guardians.length > 0; // a guardian exists — the block starts confirmed

    // اضافة ابن — every stored child becomes an editable row carrying its id
    this.children = (detail.children || []).map(child => ({
      id: child.id,
      fullName: child.fullName,
      dateOfBirth: child.dateOfBirth ? child.dateOfBirth.slice(0, 10) : '',
      nationalId: child.nationalId || '',
      gender: child.gender || '',
      healthStatusId: child.healthStatusId ?? null,
      socialStatusId: child.socialStatusId ?? null,
      educationLevelId: child.educationLevelId ?? null,
      educationalQualificationId: child.educationalQualificationId ?? null,
      gradeClass: child.gradeClass || '',
      profession: child.profession || '',
      departmentName: child.departmentName || '',
      facultyName: child.facultyName || '',
      schoolName: child.schoolName || '',
      photoAttachmentId: child.photoAttachmentId ?? null,
      birthCertificateAttachmentId: child.birthCertificateAttachmentId ?? null,
      enrollmentAttachmentId: child.enrollmentAttachmentId ?? null,
      notes: child.notes || ''
    }));
  }

  /** Split a stored full name back into the 4 name parts + family name (compose is
   *  lossless; decompose is best-effort — excess tokens all land in the family name). */
  private decomposeName(fullName: string): {
    firstName: string; secondName: string; thirdName: string; fourthName: string; familyName: string;
  } {
    const tokens = (fullName || '').trim().split(/\s+/).filter(token => !!token);
    return {
      firstName: tokens[0] || '',
      secondName: tokens[1] || '',
      thirdName: tokens[2] || '',
      fourthName: tokens[3] || '',
      familyName: tokens.length > 4 ? tokens.slice(4).join(' ') : ''
    };
  }

  // ==================== CASCADES ====================

  /** البلد → المنطقة (§11.S.2: regions repopulate on country change — selection resets,
   *  and the centers below it follow, exactly like the office-development cascade). */
  onCountryChanged(): void {
    this.regions = [];
    this.familyForm.get('regionId')?.reset(null);
    this.centers = [];
    this.familyForm.get('centerId')?.reset(null);
    this.loadRegionsByCountry(this.familyForm.get('countryId')?.value);
  }

  /** المنطقة → المركز (§11.S.2: centers repopulate on region change — selection resets). */
  onRegionChanged(): void {
    this.centers = [];
    this.familyForm.get('centerId')?.reset(null);
    this.loadCenters(this.familyForm.get('regionId')?.value);
  }

  /** رقم العماره → رقم الشقه (UC-HOU-05: flats must belong to the chosen building). */
  onBuildingChanged(): void {
    this.flats = [];
    this.familyForm.get('housingFlatId')?.reset(null);
    this.loadFlats(this.familyForm.get('housingBuildingId')?.value);
  }

  /** Option-loading primitives — the edit-mode load calls these WITHOUT resetting the
   *  patched region/center/flat selections (the family already owns them). */
  private loadRegionsByCountry(countryId: number | string | null): void {
    if (!countryId) {
      return;
    }
    this.lookupService.getRegionsByCountry(Number(countryId))
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: result => { this.regions = this.toOptions(result || []); this.cdr.markForCheck(); },
        error: () => { this.regions = []; this.cdr.markForCheck(); }
      });
  }

  private loadCenters(regionId: number | string | null): void {
    if (!regionId) {
      return;
    }
    this.lookupService.getCentersByRegion(Number(regionId))
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: result => { this.centers = this.toOptions(result || []); this.cdr.markForCheck(); },
        error: () => { this.centers = []; this.cdr.markForCheck(); }
      });
  }

  private loadFlats(buildingId: number | string | null): void {
    if (!buildingId) {
      return;
    }
    this.lookupService.getHousingFlats(Number(buildingId))
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: result => { this.flats = this.toOptions(result); this.cdr.markForCheck(); },
        error: () => { this.flats = []; this.cdr.markForCheck(); }
      });
  }

  // ==================== PHONES GRID ====================

  addPhoneRow(): void {
    this.phoneArray.push(this.createPhoneRow());
  }

  removePhoneRow(index: number): void {
    if (this.phoneArray.length <= 1) {
      // The register needs at least the default contact number — clear instead of remove
      this.phoneRows[0].reset({ number: '', belongsTo: null, isDefault: true });
      return;
    }
    const wasDefault = this.phoneRows[index].get('isDefault')?.value;
    this.phoneArray.removeAt(index);
    if (wasDefault && this.phoneArray.length > 0) {
      this.phoneRows[0].patchValue({ isDefault: true }, { emitEvent: false });
    }
  }

  /** Single-default rule — checking one row clears the others. */
  onDefaultPhoneChanged(index: number, checked: boolean): void {
    if (!checked) {
      return;
    }
    this.phoneRows.forEach((row, i) => {
      if (i !== index) {
        row.patchValue({ isDefault: false }, { emitEvent: false });
      }
    });
  }

  // ==================== COMPUTED DISPLAYS (§11.S.2 read-only) ====================

  get childrenCount(): number {
    return this.children.length;
  }

  /** الدخل الكلى — the sum of every captured guardian's monthly income. */
  get totalIncome(): number {
    return this.guardians.reduce(
      (sum, row) => sum + (Number(row.value.monthlyIncome) || 0), 0);
  }

  get perMemberShare(): number {
    const members = this.children.length + this.guardians.length; // children + guardians
    return members > 0 ? this.totalIncome / members : 0;
  }

  // ==================== GUARDIANS (§11.S.2 اضافة الاباء — multi-guardian) ====================

  /** Display name for a captured guardian row (the captured-rows table). */
  guardianDisplayName(row: GuardianRow): string {
    return this.composeGuardianName(row.value);
  }

  /** Compose the full name from the أول/ثانى/ثالث/رباعي/لقب parts of a guardian value. */
  private composeGuardianName(value: GuardianFormValue | null | undefined): string {
    if (!value) {
      return '';
    }
    return [value.firstName, value.secondName, value.thirdName, value.fourthName, value.familyName]
      .filter((part: string) => !!part)
      .join(' ')
      .trim();
  }

  /** Toggle a manual-collapse section card (header click). */
  toggleSection(section: 'family' | 'phones' | 'guardianRelation'): void {
    this.sections[section] = !this.sections[section];
  }

  /** Reveal every collapsed card — a failed submit must not hide the red fields. */
  private expandAllSections(): void {
    this.sections = { family: true, phones: true, guardianRelation: true };
    this.collapsibleCards?.forEach(card => card.open());
  }

  /** تم — accept the entry form into the captured rows (add, or update the edited row). */
  confirmGuardian(): void {
    this.guardianForm.markAllAsTouched();
    if (this.guardianForm.invalid) {
      this.expandAllSections();
      this.notification.error(this.translate.instant('housingProjects.form.messages.fixErrors'));
      return;
    }
    const value = { ...this.guardianForm.value } as GuardianFormValue;
    if (this.editingGuardianIndex >= 0) {
      // Keep the loaded row's edit-sync id — the entry form has no id control.
      const rowId = this.guardians[this.editingGuardianIndex].id;
      this.guardians[this.editingGuardianIndex] = { id: rowId, value };
      this.editingGuardianIndex = -1;
    } else {
      this.guardians.push({ value });
    }
    this.guardianSaved = this.guardians.length > 0;
    this.resetGuardianForm();
    this.notification.success(this.translate.instant('housingProjects.form.messages.guardianSaved'));
  }

  editGuardian(index: number): void {
    this.guardianForm.patchValue(this.guardians[index].value);
    this.editingGuardianIndex = index;
  }

  /** Cancel the in-place guardian edit — discard the form copy, KEEP the captured row. */
  cancelGuardianEdit(): void {
    this.editingGuardianIndex = -1;
    this.resetGuardianForm();
  }

  removeGuardian(index: number): void {
    this.guardians.splice(index, 1);
    if (this.editingGuardianIndex === index) {
      this.editingGuardianIndex = -1;
      this.resetGuardianForm();
    } else if (this.editingGuardianIndex > index) {
      // The splice shifted rows below the removed one up — move the edit target with it.
      this.editingGuardianIndex--;
    }
    this.guardianSaved = this.guardians.length > 0;
  }

  /** Clear the entry form for the next guardian — closed-set selects reset to null, not ''. */
  private resetGuardianForm(): void {
    this.guardianForm.reset();
    ['reasonOfRelationId', 'relationId', 'mainRelation', 'nationalityCountryId',
      'educationLevelId', 'socialStatusId', 'healthStatusId'].forEach(
      control => this.guardianForm.get(control)?.reset(null)
    );
  }

  /** Cut a captured row into the wire guardian shape (payload per row; row 1 mirrors provider). */
  private guardianRequestFromRow(row: GuardianRow): CreateHousingGuardianRequest {
    const v = row.value;
    return {
      // Edit-sync key: rows loaded from the family carry their id (update); new rows don't
      // (add). Rows absent from the payload are soft-removed server-side.
      id: row.id,
      fullName: this.composeGuardianName(v),
      relationshipToFamily: v.mainRelation || '', // base DTO requires it — carries العلاقة
      nationalId: v.nationalId,
      phone: v.phone,
      dateOfBirth: v.dateOfBirth || undefined,
      nationalityCountryId: v.nationalityCountryId ?? undefined,
      job: v.job || undefined,
      monthlyIncome: v.monthlyIncome != null && v.monthlyIncome !== ''
        ? Number(v.monthlyIncome)
        : undefined,
      reasonOfRelationId: v.reasonOfRelationId ?? undefined,
      relationId: v.relationId ?? undefined,
      mainRelation: v.mainRelation || undefined,
      educationLevelId: v.educationLevelId ?? undefined,
      socialStatusId: v.socialStatusId ?? undefined,
      healthStatusId: v.healthStatusId ?? undefined,
      widowSponsorship: !!v.widowSponsorship,
      anotherSponsor: !!v.anotherSponsor,
      motherIsMar: !!v.motherIsMar,
      isCaring: !!v.isCaring
    };
  }

  // ==================== CHILDREN ====================

  addChild(): void {
    this.childForm.markAllAsTouched();
    if (this.childForm.invalid) {
      this.expandAllSections();
      this.notification.error(this.translate.instant('housingProjects.form.messages.fixErrors'));
      return;
    }
    const row: ChildRow = { ...this.childForm.value };
    if (this.editingChildIndex >= 0) {
      // Keep the loaded row's edit-sync id — the childForm has no id control, so the
      // spread would drop it and the update would read this row as a remove + add.
      row.id = this.children[this.editingChildIndex].id;
      this.children[this.editingChildIndex] = row;
      this.editingChildIndex = -1;
    } else {
      this.children.push(row);
    }
    this.resetChildForm();
  }

  editChild(index: number): void {
    const row = this.children[index];
    this.childForm.patchValue(row);
    this.editingChildIndex = index;
  }

  /** Cancel the in-place child edit — discard the form copy, KEEP the captured row
   *  (review 2026-08-24: the cancel button used to call removeChild(editingChildIndex),
   *  silently deleting the row being edited). */
  cancelChildEdit(): void {
    this.editingChildIndex = -1;
    this.resetChildForm();
  }

  removeChild(index: number): void {
    this.children.splice(index, 1);
    if (this.editingChildIndex === index) {
      this.editingChildIndex = -1;
      this.resetChildForm();
    } else if (this.editingChildIndex > index) {
      // Review 2026-08-24: the splice shifted every row below the removed one up — move
      // the edit target with it, or the next addChild writes the edit into the wrong row.
      this.editingChildIndex--;
    }
  }

  /** Clear the capture form — the closed-set selects must reset to null, not ''. */
  private resetChildForm(): void {
    this.childForm.reset();
    ['gender', 'healthStatusId', 'socialStatusId', 'educationLevelId', 'educationalQualificationId'].forEach(
      control => this.childForm.get(control)?.reset(null)
    );
  }

  // ==================== CHILD ATTACHMENTS (§11.S.2 اضافة ابن) ====================

  /** Shared attachment component callback — upload the picked file and hold its id on the
   *  control; addChild carries it onto the captured row. */
  onChildFileSelected(controlName: string, files: File[]): void {
    const file = files?.[0];
    if (!file) {
      return;
    }
    this.attachmentService.upload(file, 'HousingProject')
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: response => {
          this.childForm.get(controlName)?.setValue(response.id);
          this.cdr.markForCheck(); // OnPush — control change happened off the event stream
        },
        error: () => {
          this.notification.error(
            this.translate.instant('housingProjects.form.child.attachmentUploadFailed'));
          this.cdr.markForCheck();
        }
      });
  }

  onChildFileRemoved(controlName: string): void {
    this.childForm.get(controlName)?.setValue(null);
  }

  // ==================== SAVE (UC-HOU-03 create · UC-HOU-04 update) ====================

  save(): void {
    this.markFormGroupTouched(this.familyForm);
    for (const row of this.phoneRows) {
      row.markAllAsTouched();
    }

    if (this.familyForm.invalid || this.phoneArray.invalid) {
      this.expandAllSections();
      this.notification.error(this.translate.instant('housingProjects.form.messages.fixErrors'));
      return;
    }

    // §11.S.2 اضافة الاباء — at least one captured guardian row must reach the payload;
    // the entry form may sit empty between captures (the rows are the source of truth).
    if (this.guardians.length === 0) {
      this.guardianForm.markAllAsTouched();
      this.expandAllSections();
      this.notification.error(this.translate.instant('housingProjects.form.messages.guardianRequired'));
      return;
    }

    const defaultRow =
      this.phoneRows.find(row => row.get('isDefault')?.value) || this.phoneRows[0];

    const providers = this.guardians.map(row => this.guardianRequestFromRow(row));
    // providers[0] mirrors the legacy single-provider wire — the server treats the
    // providers set as the source of truth and normalises row 1 onto Provider.
    const provider = providers[0];

    const orphans: CreateHousingChildRequest[] = this.children.map(child => ({
      // Edit-sync key: rows loaded from the family carry their id (update); new rows don't
      // (add). Rows absent from this array are soft-removed server-side.
      id: child.id,
      fullName: child.fullName,
      dateOfBirth: child.dateOfBirth,
      nationalId: child.nationalId,
      gender: child.gender,
      healthStatusId: child.healthStatusId ?? undefined,
      socialStatusId: child.socialStatusId ?? undefined,
      educationLevelId: child.educationLevelId ?? undefined,
      educationalQualificationId: child.educationalQualificationId ?? undefined,
      gradeClass: child.gradeClass,
      profession: child.profession,
      departmentName: child.departmentName,
      facultyName: child.facultyName,
      schoolName: child.schoolName,
      photoAttachmentId: child.photoAttachmentId ?? undefined,
      birthCertificateAttachmentId: child.birthCertificateAttachmentId ?? undefined,
      enrollmentAttachmentId: child.enrollmentAttachmentId ?? undefined,
      notes: child.notes || undefined
    }));

    const family = this.familyForm.value;
    const request: CreateHousingFamilyRequest = {
      // Edit mode: ownership never moves and the code is immutable — both controls are
      // disabled (dropped from .value) and the server ignores them anyway.
      charityId: !this.editMode && this.isHQ ? family.charityId : undefined,
      code: this.editMode ? undefined : (family.code || undefined),
      headOfFamily: provider.fullName,
      cityVillage: family.cityVillage,
      countryId: family.countryId,
      regionId: family.regionId,
      centerId: family.centerId,
      nearBy: family.nearBy || undefined,
      street: family.street || undefined,
      address: family.address,
      phoneNumber: defaultRow.get('number')?.value,
      rentAmount: family.rentAmount != null && family.rentAmount !== '' ? Number(family.rentAmount) : undefined,
      incomeTypeId: family.incomeTypeId,
      housingBuildingId: family.housingBuildingId,
      housingFlatId: family.housingFlatId,
      notes: family.notes || undefined,
      provider,
      providers,
      orphans
    };

    this.saving = true;
    if (this.editMode && this.familyId) {
      // UC-HOU-04 update — children sync, ownership + code untouched server-side.
      this.housingService.updateHousingFamily(this.familyId, request)
        .pipe(takeUntil(this.destroy$))
        .subscribe({
          next: () => {
            this.saving = false;
            this.notification.success(
              this.translate.instant('housingProjects.form.messages.updated'));
            this.router.navigate(['/housing-projects', this.familyId]);
          },
          error: err => {
            this.saving = false;
            this.handleSaveError(err);
          }
        });
      return;
    }

    this.housingService.createHousingFamily(request)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: created => {
          this.saving = false;
          this.notification.success(
            this.translate.instant('housingProjects.form.messages.created', {
              code: created?.code || ''
            })
          );
          this.router.navigate(['/housing-projects']);
        },
        error: err => {
          this.saving = false;
          this.handleSaveError(err);
        }
      });
  }

  /** 400 shapes from HousingProjectsController: { message } (business refusal, literal
   *  Arabic §11.U.3 text) or { message, errors: { field: [messages] } } (validator).
   *  Review 2026-08-24: validator keys are also FLAGGED onto the live controls —
   *  Provider.* → the guardian entry block (reopened so the flagged fields are visible),
   *  bare family fields → familyForm. Orphans[i].* and Providers[i].* keys have no live
   *  control (rows are captured state) — they surface via the toast; the row re-opens
   *  from the grid. Mirrors the 6-8 report form's errors-map pattern. */
  private handleSaveError(err: any): void {
    const body = err?.error;
    if (body?.errors && typeof body.errors === 'object') {
      this.applyServerFieldErrors(body.errors);
      const firstMessages = Object.values(body.errors)
        .flat()
        .slice(0, 3)
        .join(' — ');
      this.notification.error(firstMessages || body.message);
      return;
    }
    this.notification.error(body?.message || this.translate.instant('housingProjects.form.messages.createFailed'));
  }

  private applyServerFieldErrors(errors: Record<string, string[]>): void {
    for (const key of Object.keys(errors)) {
      const messages = errors[key];
      if (!messages?.length) {
        continue;
      }
      if (key.startsWith('Provider.')) {
        const control = this.guardianForm.get(this.toCamelKey(key.substring('Provider.'.length)));
        control?.setErrors({ server: messages[0] });
        control?.markAsTouched();
        this.guardianSaved = false; // reopen the block so the flagged fields are visible
      } else if (!key.startsWith('Orphans[') && !key.startsWith('Providers[')) {
        const control = this.familyForm.get(this.toCamelKey(key));
        control?.setErrors({ server: messages[0] });
        control?.markAsTouched();
      }
    }
    this.cdr.markForCheck();
  }

  /** Server keys arrive PascalCase (FluentValidation PropertyName); controls are camelCase. */
  private toCamelKey(key: string): string {
    return key.charAt(0).toLowerCase() + key.slice(1);
  }

  // ==================== VALIDATION DISPLAY (family-form pattern) ====================

  private markFormGroupTouched(group: FormGroup | FormArray): void {
    Object.values(group.controls).forEach(control => {
      control.markAsTouched();
      if ((control as FormGroup | FormArray).controls) {
        this.markFormGroupTouched(control as FormGroup | FormArray);
      }
    });
  }

  isFieldInvalid(group: FormGroup, fieldName: string): boolean {
    const field = group.get(fieldName);
    return !!(field && field.invalid && (field.dirty || field.touched));
  }

  getFieldError(group: FormGroup, fieldName: string, labelKey: string): string {
    const field = group.get(fieldName);
    if (!field || !field.errors) {
      return '';
    }
    // Server-side validator refusal flagged by applyServerFieldErrors — literal message
    if (field.errors['server']) {
      return field.errors['server'] as string;
    }
    if (field.errors['required']) {
      return this.translate.instant('validation.required', {
        field: this.translate.instant(labelKey)
      });
    }
    if (field.errors['maxlength']) {
      return this.translate.instant('housingProjects.form.messages.nationalIdTooLong');
    }
    return '';
  }

  // ==================== TRACK BY + SELECT2 OPTION BUILD ====================

  trackByOption(index: number, option: DropdownOption): number | string {
    return option.id;
  }

  trackByChildIndex(index: number): number {
    return index;
  }

  trackByPhoneIndex(index: number): number {
    return index;
  }

  trackByGuardianIndex(index: number): number {
    return index;
  }

  /** Closed-set → select2 options (translated snapshot; ids are the Arabic literals the
   *  server stores — drop-down.toOptionId keeps non-numeric ids verbatim). */
  private rebuildTranslatedOptions(): void {
    this.mainRelationSelectOptions = HOUSING_MAIN_RELATIONS.map(
      option => ({ id: option.value, name: this.translate.instant(option.labelKey) }));
    this.genderSelectOptions = HOUSING_CHILD_GENDERS.map(
      option => ({ id: option.value, name: this.translate.instant(option.labelKey) }));
    this.phoneBelongsToSelectOptions = HOUSING_PHONE_BELONGS_TO.map(
      option => ({ id: option.value, name: this.translate.instant(option.labelKey) }));
  }
}
