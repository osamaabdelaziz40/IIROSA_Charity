/**
 * Housing Projects module models (epic 6, chapter 11).
 *
 * RE-CUT (6-1/6-3): the construction-project tracker types were deleted — this module is
 * the housing-FAMILY register on the shared families store. These types mirror the backend
 * CreateFamilyDto wire (camelCase) for FamilyType=Housing; the families module's
 * family.model.ts CreateFamilyDto is a stale legacy shape, so the housing module owns its
 * own payload contract here (mirrors the §11.S.2 field order).
 */

/** Minimal projection of the created FamilyDto — the form only needs id + code. */
export interface HousingFamilyCreatedResult {
  id: string;
  code?: string;
}

/**
 * §11.S.2 بيانات الأسرة + member blocks — the POST /api/HousingProjects/projects body.
 * The Housing discriminator, the §11.S.2 mandatories and the flat⊆building rule are
 * re-validated server-side (AddNewHousingFamilyAsync); required flags here mirror that
 * contract for the client-side Validators.
 */
export interface CreateHousingFamilyRequest {
  /** الجمعية — HQ must name one; a Charity caller is pinned server-side (pin-never-widen) */
  charityId?: string;
  /** رقم القيد — optional, auto-generated when absent */
  code?: string;
  /** composed guardian name (display column of the register) */
  headOfFamily?: string;
  /** القرية / الحي */
  cityVillage?: string;
  /** البلد — drives the regions cascade (Countries lookup) */
  countryId?: number;
  /** المنطقة /المحافظة — drives the centers cascade */
  regionId?: number;
  /** المركز/ المدينة */
  centerId?: number;
  /** بجوار */
  nearBy?: string;
  /** الشارع */
  street?: string;
  /** العنوان التفصيلى */
  address?: string;
  /** بيانات الإتصال — the default row of the phones grid */
  phoneNumber?: string;
  /** قيمة الإيجار */
  rentAmount?: number;
  /** نوع الدخل */
  incomeTypeId?: number;
  /** رقم العماره — repopulates the flat drop-down on change */
  housingBuildingId?: number;
  /** رقم الشقه — must belong to the chosen building (server-checked) */
  housingFlatId?: number;
  /** ملاحظات الباحث */
  notes?: string;
  /** اضافة معيل — the guardian block (row 1; mirrors providers[0] for the legacy wire) */
  provider: CreateHousingGuardianRequest;
  /**
   * §11.S.2 اضافة الاباء (AddNewParent() always): the full guardian set — several live
   * guardians per housing family. providers[0] is mirrored onto `provider` above; the
   * server treats `providers` as the source of truth when present. On the update (PUT
   * projects/{id}) a row carrying an id updates that guardian, one without is added, and
   * an existing guardian absent from the payload is soft-removed server-side.
   */
  providers?: CreateHousingGuardianRequest[];
  /** اضافة ابن — child blocks added from the client-side collection */
  orphans?: CreateHousingChildRequest[];
}

/**
 * §11.S.2 اضافة معيل — the guardian block (the register has no father/mother
 * sections; guardians are the family's Providers). §11.S.2 اضافة الاباء allows several
 * live guardians per family.
 */
export interface CreateHousingGuardianRequest {
  /** UC-HOU-04 edit-sync key — undefined on the create path */
  id?: string;
  /** composed from أول/ثانى/ثالث/رباعي name parts */
  fullName: string;
  /** base CreateProviderDto requires it — carries the MainRelation literal (الاب/الام) */
  relationshipToFamily: string;
  /** الرقم القومي */
  nationalId: string;
  phone: string;
  /** تاريخ الميلاد */
  dateOfBirth?: string;
  /** الجنسية — Countries lookup */
  nationalityCountryId?: number;
  /** نوع العمل */
  job?: string;
  /** الدخل الشهري — feeds the computed الدخل الكلى / نصيب الفرد displays */
  monthlyIncome?: number;
  /** السبب — ReasonsOfRelation lookup */
  reasonOfRelationId?: number;
  /** نوعها — Relations lookup */
  relationId?: number;
  /** العلاقة — closed set: الاب / الام (stored as the Arabic literal) */
  mainRelation?: string;
  /** المؤهل الدراسى — EducationLevels lookup */
  educationLevelId?: number;
  /** الحالة الاجتماعية */
  socialStatusId?: number;
  /** الحالة الصحية */
  healthStatusId?: number;
  /** رعاية widow sponsorship flags (§11.S.2 checkboxes) */
  widowSponsorship?: boolean;
  anotherSponsor?: boolean;
  motherIsMar?: boolean;
  isCaring?: boolean;
}

/**
 * §11.S.2 اضافة ابن — one row per child. Wire = backend CreateOrphanDto
 * (housing extension fields included).
 */
export interface CreateHousingChildRequest {
  /**
   * UC-HOU-04 edit-sync key: on the update (PUT projects/{id}) a child carrying an id
   * updates that orphan; one without is added; an existing child absent from the payload is
   * soft-removed server-side. Always undefined on the create path.
   */
  id?: string;
  fullName: string;
  dateOfBirth: string;
  /** الرقم القومى — §11.S.2 caps at 14 digits */
  nationalId?: string;
  /** النوع — closed set: ذكر / انثى (stored as the Arabic literal) */
  gender?: string;
  /** الحالة الصحية */
  healthStatusId?: number;
  /** الحالة الاجتماعية */
  socialStatusId?: number;
  /** المرحلة الدراسية */
  educationLevelId?: number;
  /** «حاصل على مؤهل دراسى» — EducationLevel lookup (review D3 2026-08-24, §11.S.2) */
  educationalQualificationId?: number;
  /** الصف الدراسي */
  gradeClass?: string;
  /** نوعية العمل */
  profession?: string;
  /** القسم */
  departmentName?: string;
  /** الكلية */
  facultyName?: string;
  /** الموسسة التعليمية */
  schoolName?: string;
  /** الصوره الشخصيه — attachment id (shared attachment component) */
  photoAttachmentId?: string;
  /** صوره شهاده الميلاد — attachment id */
  birthCertificateAttachmentId?: string;
  /** صوره إثبات القيد — attachment id */
  enrollmentAttachmentId?: string;
  /** ملاحظات */
  notes?: string;
}

/** One row of the §11.S.2 phones grid (الرقم / يخص من / الافتراضي؟). Client-side only —
 *  the platform stores a single family phone, so the default row's number is what reaches
 *  the payload (PhoneNumber) and belongsTo/isDefault are capture conveniences. */
export interface HousingPhoneRow {
  number: string;
  belongsTo: string;
  isDefault: boolean;
}

/**
 * UC-HOU-04 read round-trip (§11.U.4): GET /api/HousingProjects/projects/{id} body — the
 * backend HousingFamilyDetailDto (FamilyDto + full child detail). The edit form re-fills
 * from this; the detail screen renders it read-only.
 */

/** ProviderDto projection — the guardian block with every §11.S.2 field + resolved names. */
export interface HousingGuardianDetail {
  id: string;
  fullName: string;
  relationshipToFamily: string;
  nationalId: string;
  phone: string;
  address?: string;
  job?: string;
  monthlyIncome?: number;
  notes?: string;
  dateOfBirth?: string;
  nationalityCountryId?: number;
  reasonOfRelationId?: number;
  reasonOfRelationName?: string;
  relationId?: number;
  relationName?: string;
  /** closed-set literal: الاب / الام */
  mainRelation?: string;
  socialStatusId?: number;
  socialStatusName?: string;
  healthStatusId?: number;
  healthStatusName?: string;
  educationLevelId?: number;
  educationLevelName?: string;
  widowSponsorship?: boolean;
  anotherSponsor?: boolean;
  motherIsMar?: boolean;
  isCaring?: boolean;
}

/** OrphanDto projection — a child row with every §11.S.2 field ( Profession/department/...). */
export interface HousingChildDetail {
  id: string;
  code?: string;
  fullName: string;
  dateOfBirth?: string;
  gender?: string;
  age?: number;
  nationalId?: string;
  educationLevelId?: number;
  educationLevelName?: string;
  /** «حاصل على مؤهل دراسى» — EducationLevel lookup (review D3 2026-08-24, §11.S.2) */
  educationalQualificationId?: number;
  schoolName?: string;
  gradeClass?: string;
  healthStatusId?: number;
  healthStatusName?: string;
  socialStatusId?: number;
  socialStatusName?: string;
  notes?: string;
  profession?: string;
  departmentName?: string;
  facultyName?: string;
  /** الصوره الشخصيه — attachment id */
  photoAttachmentId?: string;
  /** صوره شهاده الميلاد — attachment id */
  birthCertificateAttachmentId?: string;
  /** صوره إثبات القيد — attachment id */
  enrollmentAttachmentId?: string;
}

/** The housing-family aggregate — FamilyDto lifted with the full Children set. */
export interface HousingFamilyDetail {
  id: string;
  code: string;
  charityId?: string;
  charityName?: string;
  familyType: string;
  headOfFamily: string;
  cityVillage?: string;
  countryId?: number;
  countryName?: string;
  regionId?: number;
  regionName?: string;
  centerId?: number;
  centerName?: string;
  nearBy?: string;
  street?: string;
  address?: string;
  phoneNumber?: string;
  rentAmount?: number;
  incomeTypeId?: number;
  incomeTypeName?: string;
  housingBuildingId?: number;
  housingBuildingName?: string;
  housingFlatId?: number;
  housingFlatName?: string;
  notes?: string;
  perMemberShare?: number;
  orphansCount?: number;
  createdOn: string;
  updatedOn?: string;
  /** primary guardian — first live row by CreatedOn/Id (legacy single-seat pick) */
  provider?: HousingGuardianDetail;
  /** §11.S.2 اضافة الاباء — the full guardian set, primary first */
  providers?: HousingGuardianDetail[];
  children: HousingChildDetail[];
}

/** العلاقة closed set — the VALUE is the Arabic literal the server stores; labels i18n. */
export const HOUSING_MAIN_RELATIONS: ReadonlyArray<{ value: string; labelKey: string }> = [
  { value: 'الاب', labelKey: 'housingProjects.form.mainRelation.father' },
  { value: 'الام', labelKey: 'housingProjects.form.mainRelation.mother' }
];

/** النوع closed set (§11.S.2 ذكر / انثى) — Arabic literal on the wire. */
export const HOUSING_CHILD_GENDERS: ReadonlyArray<{ value: string; labelKey: string }> = [
  { value: 'ذكر', labelKey: 'housingProjects.form.gender.male' },
  { value: 'انثى', labelKey: 'housingProjects.form.gender.female' }
];

/**
 * يخص من closed set (§11.S.2 phone.BelongsTo) — client-side capture only; not persisted.
 */
export const HOUSING_PHONE_BELONGS_TO: ReadonlyArray<{ value: string; labelKey: string }> = [
  { value: 'الام', labelKey: 'housingProjects.form.phoneBelongsTo.mother' },
  { value: 'الاب', labelKey: 'housingProjects.form.phoneBelongsTo.father' },
  { value: 'خال', labelKey: 'housingProjects.form.phoneBelongsTo.maternalUncle' },
  { value: 'خالة', labelKey: 'housingProjects.form.phoneBelongsTo.maternalAunt' },
  { value: 'عم', labelKey: 'housingProjects.form.phoneBelongsTo.paternalUncle' },
  { value: 'عمة', labelKey: 'housingProjects.form.phoneBelongsTo.paternalAunt' },
  { value: 'أخ', labelKey: 'housingProjects.form.phoneBelongsTo.brother' },
  { value: 'أخت', labelKey: 'housingProjects.form.phoneBelongsTo.sister' },
  { value: 'جد', labelKey: 'housingProjects.form.phoneBelongsTo.grandfather' },
  { value: 'جدة', labelKey: 'housingProjects.form.phoneBelongsTo.grandmother' },
  { value: 'قريب', labelKey: 'housingProjects.form.phoneBelongsTo.relative' },
  { value: 'قريبة', labelKey: 'housingProjects.form.phoneBelongsTo.relativeF' },
  { value: 'غير ذلك', labelKey: 'housingProjects.form.phoneBelongsTo.other' },
  { value: 'نفسه', labelKey: 'housingProjects.form.phoneBelongsTo.self' }
];

// ==================== 6-6 — periodic reports of a housing beneficiary (§11.S.3) ====================

/**
 * §11.U.6 ChildOrParent discriminator on the wire. Server parses the query value
 * ordinally-insensitively; the canonical casing is sent.
 */
export type HousingBeneficiaryType = 'Child' | 'Parent';

/**
 * §11.S.3 grid row — PeriodicOrphanReportListDto (camelCase) projected to the five
 * legacy columns: الرقم (row serial) · رقم التقرير · التاريخ · تم الاعتماد · تاريخ الاعتماد.
 */
export interface HousingReportListItem {
  id: string;
  reportNo?: string;
  reportDate: string;
  /** تم الاعتماد */
  isAccepted: boolean;
  isRefused: boolean;
  reviewed: boolean;
  /** تاريخ الاعتماد */
  reviewedDate?: string;
  reviewStatus?: string;
  orphanId: string;
  orphanName?: string;
  orphanCode?: string;
  charityName?: string;
  createdOn: string;
}

/** PeriodicOrphanReportPagedResult<T> wire shape. */
export interface HousingReportPagedResult {
  items: HousingReportListItem[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

/**
 * UC-HOU-07 (§11.U.7 البحث بالكود) beneficiary row — GET
 * /api/HousingProjects/projects/{id}/beneficiaries[?code=]. The beneficiaryId +
 * childOrParent pair is exactly what the 6-6 reports read consumes. Codes belong to
 * children; a guardian row carries code undefined.
 */
export interface HousingBeneficiaryRow {
  beneficiaryId: string;
  childOrParent: HousingBeneficiaryType | string;
  code?: string;
  fullName: string;
  nationalId?: string;
  age?: number;
}

// ==================== 6-8 — the §11.S.4 report form ====================

/**
 * الحالة الصحية closed set (§11.S.4 سليم / معاق / مريض) — the VALUE is the Arabic
 * literal stored in MedicalStatus; labels i18n. معاق reveals the disability block,
 * مريض reveals the disease field (legacy HealthstaueFun).
 */
export const HOUSING_HEALTH_STATUSES: ReadonlyArray<{ value: string; labelKey: string }> = [
  { value: 'سليم', labelKey: 'housingProjects.reports.form.health.healthy' },
  { value: 'معاق', labelKey: 'housingProjects.reports.form.health.disabled' },
  { value: 'مريض', labelKey: 'housingProjects.reports.form.health.sick' }
];

/** نوع الاعاقة closed set (§11.S.4 حركية / بصرية / سمعية / ذهنية) — Disability column. */
export const HOUSING_DISABILITY_TYPES: ReadonlyArray<{ value: string; labelKey: string }> = [
  { value: 'حركية', labelKey: 'housingProjects.reports.form.disabilityType.motor' },
  { value: 'بصرية', labelKey: 'housingProjects.reports.form.disabilityType.visual' },
  { value: 'سمعية', labelKey: 'housingProjects.reports.form.disabilityType.hearing' },
  { value: 'ذهنية', labelKey: 'housingProjects.reports.form.disabilityType.mental' }
];

/** نوع التعليم closed set (§11.S.4 حكومي / أهلي) — SchoolType column. */
export const HOUSING_SCHOOL_TYPES: ReadonlyArray<{ value: string; labelKey: string }> = [
  { value: 'حكومي', labelKey: 'housingProjects.reports.form.schoolType.government' },
  { value: 'أهلي', labelKey: 'housingProjects.reports.form.schoolType.private' }
];

/** اخر تقدير closed set (§11.S.4) — EducationDegree column. */
export const HOUSING_GRADES: ReadonlyArray<{ value: string; labelKey: string }> = [
  { value: 'ممتاز', labelKey: 'housingProjects.reports.form.grade.excellent' },
  { value: 'جيدجدا', labelKey: 'housingProjects.reports.form.grade.veryGood' },
  { value: 'جيد', labelKey: 'housingProjects.reports.form.grade.good' },
  { value: 'مقبول', labelKey: 'housingProjects.reports.form.grade.pass' },
  { value: 'ضعيف', labelKey: 'housingProjects.reports.form.grade.weak' }
];

/**
 * sp الحالة closed set (§11.S.4 حاصل على شهادة / ترك الدراسة) — client-side branch
 * selector (legacy ChildWorkingStatuschanged): ترك الدراسة opens the drop-out year,
 * حاصل على شهادة opens the highest-qualification block. Not a stored column.
 */
export const HOUSING_WORKING_STATUSES: ReadonlyArray<{ value: string; labelKey: string }> = [
  { value: 'حاصل على شهادة', labelKey: 'housingProjects.reports.form.workingStatus.certified' },
  { value: 'ترك الدراسة', labelKey: 'housingProjects.reports.form.workingStatus.leftStudy' }
];

/** الهوايات closed set (§11.S.4) — Hobby column. */
export const HOUSING_HOBBIES: ReadonlyArray<{ value: string; labelKey: string }> = [
  { value: 'صغيرالسن', labelKey: 'housingProjects.reports.form.hobby.young' },
  { value: 'رياضة', labelKey: 'housingProjects.reports.form.hobby.sport' },
  { value: 'قراءة', labelKey: 'housingProjects.reports.form.hobby.reading' },
  { value: 'حاسب الى', labelKey: 'housingProjects.reports.form.hobby.computer' },
  { value: 'زراعة نباتات', labelKey: 'housingProjects.reports.form.hobby.planting' },
  { value: 'تدبير منزلي', labelKey: 'housingProjects.reports.form.hobby.homeManagement' },
  { value: 'زخرفة', labelKey: 'housingProjects.reports.form.hobby.decoration' },
  { value: 'خياطة', labelKey: 'housingProjects.reports.form.hobby.sewing' },
  { value: 'اخرى', labelKey: 'housingProjects.reports.form.hobby.other' }
];

/**
 * §11.S.4 payload — POST /api/PeriodicOrphanReports from the housing form. The §11.S.4
 * substance rides the epic-9 report columns (CreatePeriodicOrphanReportDto wire, camelCase);
 * the housing deltas are the beneficiary discriminator pair, the family context and the
 * create-time review flags (stored as data — the workflow endpoints are 9-7/9-8).
 * Child ⇒ orphanId (the family child); Parent ⇒ housingBeneficiaryId (the guardian
 * provider id) + housingFamilyId, orphanId stays empty (the server picks the carrier child).
 */
export interface CreateHousingReportRequest {
  childOrParent: HousingBeneficiaryType;
  housingFamilyId: string;
  housingBeneficiaryId?: string;
  orphanId?: string;
  orphanPaymentId?: string;
  reportDate: string;
  reportPeriodFrom?: string;
  reportPeriodTo?: string;
  prayerStatus?: string;
  mannersStatus?: string;
  hadeethStatus?: string;
  quranParts?: string;
  quranVerses?: string;
  medicalStatus?: string;
  disease?: string;
  disability?: string;
  disabilityDescription?: string;
  diseaseDescription?: string;
  hobby?: string;
  course?: string;
  courseName?: string;
  sportName?: string;
  professionName?: string;
  achievement?: string;
  achievementArr?: string;
  wish?: string;
  wishArr?: string;
  orphanMessage?: string;
  educationalStageId?: number;
  educationalLevelId?: number;
  grade?: string;
  school?: string;
  schoolType?: string;
  educationDegree?: string;
  highestEducationalLevel?: string;
  highestEducationalLevelYear?: number;
  isOrphanStudent?: boolean;
  educationalYear?: number;
  annualFeeForStudy?: number;
  studyingYears?: number;
  restStudyingYears?: number;
  graduationYear?: number;
  dropOut?: boolean;
  dropOutYear?: number;
  dropOutStageId?: number;
  faculty?: string;
  department?: string;
  specialization?: string;
  married?: boolean;
  marriageDate?: string;
  dead?: boolean;
  deathDate?: string;
  orphanImageId?: string;
  orphanCertificateImageId?: string;
  medicalReportImageId?: string;
  orphanDeadImageId?: string;
  orphanMarriageImageId?: string;
  missingDocuments?: boolean;
  missingDocumentsName?: string;
  isAccepted?: boolean;
  isRefused?: boolean;
  refuseReasonId?: number;
}

/**
 * Edit payload — PUT /api/PeriodicOrphanReports/{id}. The epic-9 update is a full-replace,
 * so the form PUTs every control it owns (orphanPaymentId included — a hidden control — or
 * an epic-9-created report would lose its payment link); the housing identity fields
 * (beneficiary pair, family, orphan) are immutable on update and are NOT sent.
 */
export type UpdateHousingReportRequest = Omit<
  CreateHousingReportRequest,
  'childOrParent' | 'housingFamilyId' | 'housingBeneficiaryId' | 'orphanId'
>;

/**
 * PeriodicOrphanReportDto projection for the §11.S.4 edit mode — the fields the form
 * patches. childOrParent/housingFamilyId tell the header which beneficiary block to
 * render without re-resolving the picker.
 */
export interface HousingReportDetail {
  id: string;
  reportNo?: string;
  reportDate: string;
  reportPeriodFrom?: string;
  reportPeriodTo?: string;
  orphanId: string;
  orphanPaymentId?: string;
  orphanCode?: string;
  orphanName?: string;
  childOrParent?: string;
  housingFamilyId?: string;
  charityId?: string;
  charityName?: string;
  prayerStatus?: string;
  mannersStatus?: string;
  hadeethStatus?: string;
  quranParts?: string;
  quranVerses?: string;
  medicalStatus?: string;
  disease?: string;
  disability?: string;
  disabilityDescription?: string;
  diseaseDescription?: string;
  hobby?: string;
  course?: string;
  courseName?: string;
  sportName?: string;
  professionName?: string;
  achievement?: string;
  achievementArr?: string;
  wish?: string;
  wishArr?: string;
  orphanMessage?: string;
  educationalStageId?: number;
  educationalLevelId?: number;
  grade?: string;
  school?: string;
  schoolType?: string;
  educationDegree?: string;
  highestEducationalLevel?: string;
  highestEducationalLevelYear?: number;
  isOrphanStudent?: boolean;
  educationalYear?: number;
  annualFeeForStudy?: number;
  studyingYears?: number;
  restStudyingYears?: number;
  graduationYear?: number;
  dropOut?: boolean;
  dropOutYear?: number;
  dropOutStageId?: number;
  faculty?: string;
  department?: string;
  specialization?: string;
  married?: boolean;
  marriageDate?: string;
  dead?: boolean;
  deathDate?: string;
  orphanImageId?: string;
  orphanCertificateImageId?: string;
  medicalReportImageId?: string;
  orphanDeadImageId?: string;
  orphanMarriageImageId?: string;
  missingDocuments?: boolean;
  missingDocumentsName?: string;
  isAccepted: boolean;
  isRefused: boolean;
  refuseReasonId?: number;
  refuseReasonName?: string;
  reviewed: boolean;
  reviewedDate?: string;
  locked: boolean;
}
