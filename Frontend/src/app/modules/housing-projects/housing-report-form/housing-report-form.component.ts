/**
 * Housing Report Form (UC-HOU-08 · §11.S.4) — تقرير دوري لأسرة ساكنة.
 * POST /api/PeriodicOrphanReports (the shared epic-9 create) with the §11.U.6
 * discriminator: Child ⇒ orphanId is the 6-7-resolved family child; Parent ⇒
 * housingBeneficiaryId is the guardian's provider id and the server resolves the family
 * and the carrier child. The 6-7 lookup gates the form — arriving without a resolved
 * beneficiary redirects back to the list with a toast (§11.U.7 pre-condition).
 *
 * Edit mode (:reportId ≠ 'new') loads the report and PUTs the epic-9 full-replace update;
 * the housing identity is immutable there. The §11.S.4 review flags (الموافقه / رفض +
 * سبب الرفض) are stored as DATA on create — the review WORKFLOW endpoints are 9-7/9-8.
 * Print renders DISABLED (epic 18). Roles: Charity + HQ (Admin/SuperAdmin).
 */

import { Component, OnInit, OnDestroy, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';

import { HousingProjectService } from '../services/housing-project.service';
import {
  HousingFamilyDetail,
  HousingBeneficiaryRow,
  HousingBeneficiaryType,
  HousingReportDetail,
  CreateHousingReportRequest,
  HOUSING_HEALTH_STATUSES,
  HOUSING_DISABILITY_TYPES,
  HOUSING_SCHOOL_TYPES,
  HOUSING_GRADES,
  HOUSING_WORKING_STATUSES,
  HOUSING_HOBBIES
} from '../models/housing-project.model';
import { LookupManagementService } from '../../lookup-management/services/lookup-management.service';
import { LookupDto } from '../../lookup-management/models/lookup.model';
import { AttachmentService } from '../../../core/services/attachment.service';
import { NotificationService } from '../../../core/services/notification.service';
import { AttachmentComponent } from '../../../shared/components/attachment/attachment.component';
import {
  BreadcrumbComponent,
  BreadcrumbItem,
  PageHeaderComponent
} from '../../../shared/components';

@Component({
  selector: 'app-housing-report-form',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    TranslateModule,
    AttachmentComponent,
    BreadcrumbComponent,
    PageHeaderComponent
  ],
  templateUrl: './housing-report-form.component.html',
  styleUrls: ['./housing-report-form.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class HousingReportFormComponent implements OnInit, OnDestroy {
  private destroy$ = new Subject<void>();
  private familyId = '';

  /** Server messages mapped to translated keys — prefix matches (the service interpolates month/year). */
  private static readonly KNOWN_MESSAGE_PREFIXES: ReadonlyArray<{ prefix: string; key: string }> = [
    { prefix: "A periodic report already exists for this family's guardian", key: 'housingProjects.reports.form.errors.guardianDuplicate' },
    { prefix: 'Every child of this housing family already has a report', key: 'housingProjects.reports.form.errors.noCarrierChild' },
    { prefix: 'A periodic report already exists for this orphan', key: 'housingProjects.reports.form.errors.duplicate' }
  ];
  private static readonly KNOWN_MESSAGES: Record<string, string> = {
    'You can not update old report': 'periodicReports.cannotEditOldReport'
  };

  // Route / mode
  isEditMode = false;
  loading = true;
  saving = false;
  /** §14.U.5 gate reused for edit mode — locked/accepted reports are immutable. */
  editBlocked = false;

  // Beneficiary context (the 6-7 gate) + family header
  beneficiary: HousingBeneficiaryRow | null = null;
  beneficiaryType: HousingBeneficiaryType = 'Child';
  family: HousingFamilyDetail | null = null;

  // §11.S.4 lookups + server-side field errors (15-6 errors-map shape)
  educationLevels: LookupDto[] = [];
  refuseReasons: LookupDto[] = [];
  serverErrors: Record<string, string> = {};

  /** The five §11.S.4 attachment slots — control names carry the server attachment id (BR-12). */
  readonly imageSlots: { control: string; labelKey: string; required: boolean }[] = [
    { control: 'orphanImageId', labelKey: 'housingProjects.reports.form.files.photo', required: true },
    { control: 'orphanCertificateImageId', labelKey: 'housingProjects.reports.form.files.certificate', required: false },
    { control: 'medicalReportImageId', labelKey: 'housingProjects.reports.form.files.medicalReport', required: false },
    { control: 'orphanDeadImageId', labelKey: 'housingProjects.reports.form.files.deathCertificate', required: false },
    { control: 'orphanMarriageImageId', labelKey: 'housingProjects.reports.form.files.marriageContract', required: false }
  ];

  /** §11.S.4 closed sets — Arabic literal on the wire, labels i18n. */
  readonly healthStatuses = HOUSING_HEALTH_STATUSES;
  readonly disabilityTypes = HOUSING_DISABILITY_TYPES;
  readonly schoolTypes = HOUSING_SCHOOL_TYPES;
  readonly grades = HOUSING_GRADES;
  readonly workingStatuses = HOUSING_WORKING_STATUSES;
  readonly hobbies = HOUSING_HOBBIES;

  /** sp الحالة branch selector — standalone (not a stored column); derives dropOut / highest-level reveals. */
  workingStatus = '';

  reportForm: FormGroup;
  breadcrumbs: BreadcrumbItem[] = [
    { label: 'common.home', url: '/dashboard' },
    { label: 'housingProjects.title', url: '/housing-projects' },
    { label: 'housingProjects.reports.title' },
    { label: 'housingProjects.reports.form.title' }
  ];

  pageActions = [
    {
      label: 'common.back',
      type: 'secondary',
      icon: 'fe-arrow-left',
      click: () => this.router.navigate(['/housing-projects', this.familyId, 'reports'])
    }
  ];

  constructor(
    private fb: FormBuilder,
    private route: ActivatedRoute,
    private router: Router,
    private housingService: HousingProjectService,
    private lookupService: LookupManagementService,
    private attachmentService: AttachmentService,
    private notification: NotificationService,
    private translate: TranslateService,
    private cdr: ChangeDetectorRef
  ) {
    this.reportForm = this.buildForm();
    this.wireInterlocks();
  }

  ngOnInit(): void {
    const familyId = this.route.snapshot.paramMap.get('id');
    const reportParam = this.route.snapshot.paramMap.get('reportId');
    if (!familyId) {
      this.router.navigate(['/housing-projects']);
      return;
    }
    this.familyId = familyId;
    this.isEditMode = !!reportParam && reportParam !== 'new';

    this.loadLookups();
    this.loadFamily();

    if (this.isEditMode && reportParam) {
      this.loadReport(reportParam);
    } else {
      // §11.U.7 pre-condition: the 6-7 lookup gates the form — a beneficiary must arrive resolved.
      const beneficiaryId = this.route.snapshot.queryParamMap.get('beneficiary');
      const type = this.route.snapshot.queryParamMap.get('type');
      if (!beneficiaryId || (type !== 'Child' && type !== 'Parent')) {
        this.refuseEntry();
        return;
      }
      this.beneficiaryType = type;
      this.resolveBeneficiary(beneficiaryId);
    }
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  /** §11.S.4 drop-down sources: EducationLevels + RefuseReasons (the epic-9 refuse catalogue). */
  private loadLookups(): void {
    this.lookupService.getEducationLevels()
      .pipe(takeUntil(this.destroy$))
      .subscribe({ next: levels => (this.educationLevels = levels), error: () => (this.educationLevels = []) });

    this.lookupService.getRefuseReasons()
      .pipe(takeUntil(this.destroy$))
      .subscribe({ next: reasons => (this.refuseReasons = reasons), error: () => (this.refuseReasons = []) });
  }

  /** Family context — the §11.S.4 header الجمعية (read-only) + the beneficiaries feed. */
  private loadFamily(): void {
    this.housingService.getHousingFamily(this.familyId)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: detail => {
          this.family = detail;
          this.breadcrumbs[2].url = `/housing-projects/${this.familyId}/reports`;
          this.breadcrumbs[2].label = detail.code || 'housingProjects.reports.title';
          this.cdr.markForCheck();
        },
        error: () => {
          this.loading = false;
          this.notification.error(this.translate.instant('housingProjects.reports.messages.familyLoadFailed'));
          this.router.navigate(['/housing-projects']);
        }
      });
  }

  /** UC-HOU-07: match the resolved beneficiary against the family's rows (children + guardian). */
  private resolveBeneficiary(beneficiaryId: string): void {
    this.housingService.getHousingBeneficiaries(this.familyId)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: rows => {
          const row = (rows || []).find(r => r.beneficiaryId === beneficiaryId);
          if (!row) {
            this.refuseEntry();
            return;
          }
          this.beneficiary = row;
          // A code resolves children; the guardian row only answers the Parent branch.
          this.beneficiaryType = row.childOrParent === 'Parent' ? 'Parent' : 'Child';
          if (this.beneficiaryType === 'Child') {
            this.reportForm.patchValue({ orphanId: row.beneficiaryId });
          }
          this.loading = false;
          this.cdr.markForCheck();
        },
        error: () => this.refuseEntry()
      });
  }

  /** The 6-7 lookup gates the form — without a beneficiary there is nothing to report on. */
  private refuseEntry(): void {
    this.loading = false;
    this.notification.warning(
      this.translate.instant('housingProjects.reports.form.errors.noBeneficiary'));
    this.router.navigate(['/housing-projects', this.familyId, 'reports']);
  }

  /**
   * Legacy §11.S.4 on-change interlocks: switching الحالة الصحية clears the branch's
   * stale values; unticking a life-event clears its date; the student block clears with
   * its checkbox — stale values are never submitted (the server re-checks the pairs).
   */
  private wireInterlocks(): void {
    this.reportForm.get('medicalStatus')?.valueChanges.subscribe(status => {
      if (status !== 'مريض') {
        this.reportForm.get('disease')?.setValue(null);
      }
      if (status !== 'معاق') {
        ['disability', 'disabilityDescription'].forEach(c => this.reportForm.get(c)?.setValue(null));
      }
    });

    this.reportForm.get('married')?.valueChanges.subscribe(married => {
      if (!married) {
        this.reportForm.get('marriageDate')?.setValue(null);
      }
    });

    this.reportForm.get('dead')?.valueChanges.subscribe(dead => {
      if (!dead) {
        this.reportForm.get('deathDate')?.setValue(null);
      }
    });

    this.reportForm.get('isOrphanStudent')?.valueChanges.subscribe(student => {
      if (!student) {
        ['annualFeeForStudy', 'studyingYears', 'restStudyingYears', 'graduationYear']
          .forEach(c => this.reportForm.get(c)?.setValue(null));
      }
    });

    // الموافقه / رفض are mutually exclusive (legacy §11.S.4 checkboxes).
    this.reportForm.get('isAccepted')?.valueChanges.subscribe(accepted => {
      if (accepted) {
        this.reportForm.get('isRefused')?.setValue(false, { emitEvent: false });
        this.reportForm.get('refuseReasonId')?.setValue(null);
      }
    });
    this.reportForm.get('isRefused')?.valueChanges.subscribe(refused => {
      if (!refused) {
        this.reportForm.get('refuseReasonId')?.setValue(null);
      } else {
        this.reportForm.get('isAccepted')?.setValue(false, { emitEvent: false });
      }
    });
  }

  /** sp الحالة branch (legacy ChildWorkingStatuschanged): ترك الدراسة ⇒ drop-out year, حاصل على شهادة ⇒ highest qualification. */
  onWorkingStatusChanged(): void {
    if (this.workingStatus !== 'ترك الدراسة') {
      this.reportForm.get('dropOut')?.setValue(false);
      this.reportForm.get('dropOutYear')?.setValue(null);
    }
    if (this.workingStatus !== 'حاصل على شهادة') {
      this.reportForm.get('highestEducationalLevel')?.setValue(null);
      this.reportForm.get('highestEducationalLevelYear')?.setValue(null);
    }
  }

  /** Build the §11.S.4 form — control names are the wire names (epic-9 create DTO, camelCase). */
  private buildForm(): FormGroup {
    return this.fb.group({
      // §11.S.4 header — تاريخ التقرير mandatory (server rule); رقم التقرير server-generated
      orphanId: [''],
      orphanPaymentId: [''],
      reportDate: [new Date().toISOString().split('T')[0], Validators.required],
      reportPeriodFrom: [''],
      reportPeriodTo: [''],
      reportNo: [''],

      // الحالة الصحية — سليم / معاق / مريض on-change reveals
      medicalStatus: [''],
      disease: [''],
      disability: [''],
      disabilityDescription: [''],
      diseaseDescription: [''],

      // الانشطة والبرامج — hobbies closed set; sponsor message free text (MessageReasons = epic 19)
      hobby: [''],
      course: [''],
      courseName: [''],
      sportName: [''],
      professionName: [''],
      achievement: [''],
      achievementArr: [''],
      wish: [''],
      wishArr: [''],
      orphanMessage: [''],

      // الحالة التعليمية — level from the live lookup; the rest text/closed sets
      educationalStageId: [''],
      educationalLevelId: [''],
      grade: [''],
      school: [''],
      schoolType: [''],
      educationDegree: [''],
      highestEducationalLevel: [''],
      highestEducationalLevelYear: [''],
      isOrphanStudent: [false],
      educationalYear: [''],
      annualFeeForStudy: [''],
      studyingYears: [''],
      restStudyingYears: [''],
      graduationYear: [''],
      dropOut: [false],
      dropOutYear: [''],
      dropOutStageId: [''],
      faculty: [''],
      department: [''],
      specialization: [''],

      // Life events (§11.S.4 تزوج / توفى)
      married: [false],
      marriageDate: [''],
      dead: [false],
      deathDate: [''],

      // الملفات المطلوبه — الصوره mandatory (png/jpg/jpeg)
      orphanImageId: ['', Validators.required],
      orphanCertificateImageId: [''],
      medicalReportImageId: [''],
      orphanDeadImageId: [''],
      orphanMarriageImageId: [''],
      missingDocuments: [false],
      missingDocumentsName: [''],

      // §11.S.4 review flags — stored as data (workflow endpoints are 9-7/9-8)
      isAccepted: [false],
      isRefused: [false],
      refuseReasonId: ['']
    });
  }

  // ==================== REVEAL GETTERS (§11.S.4 on-change branches) ====================

  get isSick(): boolean {
    return this.reportForm.get('medicalStatus')?.value === 'مريض';
  }

  get isDisabled(): boolean {
    return this.reportForm.get('medicalStatus')?.value === 'معاق';
  }

  get isStudentRequest(): boolean {
    return !!this.reportForm.get('isOrphanStudent')?.value;
  }

  get isRefusedFlag(): boolean {
    return !!this.reportForm.get('isRefused')?.value;
  }

  /** The §11.S.4 header beneficiary display — the resolved row, or the loaded report's own subject. */
  get headerName(): string {
    return this.beneficiary?.fullName
      || (this.isEditMode ? this.loadedReportName : '') || '—';
  }

  get headerAge(): number | null {
    return this.beneficiary?.age ?? null;
  }

  get headerCode(): string | undefined {
    return this.beneficiary?.code;
  }

  get headerCharity(): string | undefined {
    return this.family?.charityName;
  }

  get headerBeneficiaryTypeLabel(): string {
    return this.translate.instant(this.beneficiaryType === 'Parent'
      ? 'housingProjects.reports.filters.parent'
      : 'housingProjects.reports.filters.child');
  }

  private loadedReportName = '';

  // ==================== UPLOADS (BR-12 — reference the server attachment id) ====================

  onImageSelected(controlName: string, files: File[]): void {
    const file = files?.[0];
    if (!file) {
      return;
    }
    // §11.S.4 الملفات المطلوبه accept png/jpg/jpeg only
    if (!/^image\/(png|jpe?g)$/i.test(file.type)) {
      this.notification.error(this.translate.instant('housingProjects.reports.form.errors.imageType'));
      return;
    }
    this.attachmentService.upload(file, 'PeriodicOrphanReport').subscribe({
      next: response => {
        this.reportForm.get(controlName)?.setValue(response.id);
        delete this.serverErrors[controlName];
        this.cdr.markForCheck();
      },
      error: () => this.notification.error(
        this.translate.instant('housingProjects.reports.form.errors.uploadFailed'))
    });
  }

  onImageRemoved(controlName: string): void {
    this.reportForm.get(controlName)?.setValue(null);
  }

  // ==================== EDIT MODE ====================

  private loadReport(reportId: string): void {
    this.housingService.getHousingReport(reportId)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: report => {
          this.patchForm(report);
          this.loading = false;
          this.checkEditable(report);
          this.cdr.markForCheck();
        },
        error: () => {
          this.loading = false;
          this.notification.error(this.translate.instant('housingProjects.reports.messages.loadFailed'));
          this.router.navigate(['/housing-projects', this.familyId, 'reports']);
        }
      });
  }

  /** §14.U.5 — ask the server whether this report may still be edited; refuse the save locally too. */
  private checkEditable(report: HousingReportDetail): void {
    if (report.locked || (report.isAccepted && !report.isRefused)) {
      this.editBlocked = true;
      return;
    }
    this.housingService.canEditHousingReport(report.id)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: result => {
          this.editBlocked = !result.canEdit;
          this.cdr.markForCheck();
        },
        error: () => (this.editBlocked = false)
      });
  }

  private patchForm(report: HousingReportDetail): void {
    // Header subject — the report knows its own branch; resolve the row for age/code context.
    this.beneficiaryType = report.childOrParent === 'Parent' ? 'Parent' : 'Child';
    this.loadedReportName = report.orphanName || '';

    this.reportForm.patchValue({
      orphanId: report.orphanId,
      orphanPaymentId: report.orphanPaymentId ?? null,
      reportDate: report.reportDate?.split('T')[0],
      reportPeriodFrom: report.reportPeriodFrom?.split('T')[0],
      reportPeriodTo: report.reportPeriodTo?.split('T')[0],
      reportNo: report.reportNo,
      medicalStatus: report.medicalStatus,
      disease: report.disease,
      disability: report.disability,
      disabilityDescription: report.disabilityDescription,
      diseaseDescription: report.diseaseDescription,
      hobby: report.hobby,
      course: report.course,
      courseName: report.courseName,
      sportName: report.sportName,
      professionName: report.professionName,
      achievement: report.achievement,
      achievementArr: report.achievementArr,
      wish: report.wish,
      wishArr: report.wishArr,
      orphanMessage: report.orphanMessage,
      educationalStageId: report.educationalStageId,
      educationalLevelId: report.educationalLevelId,
      grade: report.grade,
      school: report.school,
      schoolType: report.schoolType,
      educationDegree: report.educationDegree,
      highestEducationalLevel: report.highestEducationalLevel,
      highestEducationalLevelYear: report.highestEducationalLevelYear,
      isOrphanStudent: report.isOrphanStudent,
      educationalYear: report.educationalYear,
      annualFeeForStudy: report.annualFeeForStudy,
      studyingYears: report.studyingYears,
      restStudyingYears: report.restStudyingYears,
      graduationYear: report.graduationYear,
      dropOut: report.dropOut,
      dropOutYear: report.dropOutYear,
      dropOutStageId: report.dropOutStageId,
      faculty: report.faculty,
      department: report.department,
      specialization: report.specialization,
      married: report.married,
      marriageDate: report.marriageDate?.split('T')[0],
      dead: report.dead,
      deathDate: report.deathDate?.split('T')[0],
      orphanImageId: report.orphanImageId,
      orphanCertificateImageId: report.orphanCertificateImageId,
      medicalReportImageId: report.medicalReportImageId,
      orphanDeadImageId: report.orphanDeadImageId,
      orphanMarriageImageId: report.orphanMarriageImageId,
      missingDocuments: report.missingDocuments,
      missingDocumentsName: report.missingDocumentsName,
      isAccepted: report.isAccepted,
      isRefused: report.isRefused,
      refuseReasonId: report.refuseReasonId
    });

    // sp الحالة derives from the stored data (dropOut ⇒ ترك الدراسة; highest level ⇒ certified)
    this.workingStatus = report.dropOut
      ? 'ترك الدراسة'
      : (report.highestEducationalLevel || report.highestEducationalLevelYear ? 'حاصل على شهادة' : '');

    // Resolve the beneficiary row for the header (age/code) — Child rows key on the orphan id
    const lookupId = this.beneficiaryType === 'Parent' ? null : report.orphanId;
    if (lookupId) {
      this.housingService.getHousingBeneficiaries(this.familyId)
        .pipe(takeUntil(this.destroy$))
        .subscribe({
          next: rows => {
            this.beneficiary = (rows || []).find(r => r.beneficiaryId === lookupId) || null;
            this.cdr.markForCheck();
          },
          error: () => (this.beneficiary = null)
        });
    } else {
      // Guardian report — the guardian row is the family's Parent beneficiary
      this.housingService.getHousingBeneficiaries(this.familyId)
        .pipe(takeUntil(this.destroy$))
        .subscribe({
          next: rows => {
            this.beneficiary = (rows || []).find(r => r.childOrParent === 'Parent') || null;
            this.cdr.markForCheck();
          },
          error: () => (this.beneficiary = null)
        });
    }
  }

  // ==================== SAVE ====================

  save(): void {
    if (this.reportForm.invalid) {
      this.reportForm.markAllAsTouched();
      return;
    }

    if (this.isEditMode && this.editBlocked) {
      this.notification.warning(this.translate.instant('periodicReports.cannotEditOldReport'));
      return;
    }

    // Client mirror of the server rule — سبب الرفض mandatory when refused
    if (this.isRefusedFlag && !this.reportForm.get('refuseReasonId')?.value) {
      this.reportForm.get('refuseReasonId')?.setErrors({ required: true });
      this.reportForm.get('refuseReasonId')?.markAsTouched();
      return;
    }

    this.saving = true;
    this.serverErrors = {};
    this.cdr.markForCheck();

    if (this.isEditMode) {
      const { orphanId, ...payload } = this.reportForm.value;
      this.housingService.updateHousingReport(this.route.snapshot.paramMap.get('reportId')!, payload)
        .pipe(takeUntil(this.destroy$))
        .subscribe({
          next: () => {
            this.saving = false;
            this.notification.success(
              this.translate.instant('housingProjects.reports.form.messages.saved'));
            this.router.navigate(['/housing-projects', this.familyId, 'reports']);
          },
          error: error => {
            this.saving = false;
            this.handleSaveError(error);
          }
        });
    } else {
      const { orphanId, ...rest } = this.reportForm.value;
      const payload: CreateHousingReportRequest = {
        ...rest,
        childOrParent: this.beneficiaryType,
        housingFamilyId: this.familyId,
        // Parent ⇒ the guardian's beneficiary id (server resolves family + carrier child);
        // Child ⇒ the child's orphan id is the beneficiary id.
        housingBeneficiaryId: this.beneficiary?.beneficiaryId,
        ...(this.beneficiaryType === 'Child' ? { orphanId: this.beneficiary?.beneficiaryId } : {})
      };
      this.housingService.createHousingReport(payload)
        .pipe(takeUntil(this.destroy$))
        .subscribe({
          next: () => {
            this.saving = false;
            this.notification.success(
              this.translate.instant('housingProjects.reports.form.messages.saved'));
            this.router.navigate(['/housing-projects', this.familyId, 'reports']);
          },
          error: error => {
            this.saving = false;
            this.handleSaveError(error);
          }
        });
    }
  }

  cancel(): void {
    this.router.navigate(['/housing-projects', this.familyId, 'reports']);
  }

  /**
   * 15-6 errors-map shape: the server sends { message, errors: { PascalCase key: [msgs] } } —
   * camelCase the key, keep the first message for the toast and flag the offending controls.
   */
  private handleSaveError(error: any): void {
    const body = error?.error ?? error ?? {};
    const errors: Record<string, string[]> | undefined = body.errors;

    if (errors) {
      const mapped: Record<string, string> = {};
      for (const key of Object.keys(errors)) {
        const camelKey = key.charAt(0).toLowerCase() + key.slice(1);
        const messages = errors[key];
        if (messages?.length) {
          mapped[camelKey] = messages[0];
          const control = this.reportForm.get(camelKey);
          control?.setErrors({ server: messages[0] });
          control?.markAsTouched();
        }
      }
      this.serverErrors = mapped;
    }

    const raw = typeof body.message === 'string' ? body.message : '';
    const knownKey = raw ? HousingReportFormComponent.KNOWN_MESSAGES[raw] : undefined;
    const knownPrefix = raw
      ? HousingReportFormComponent.KNOWN_MESSAGE_PREFIXES.find(p => raw.startsWith(p.prefix))?.key
      : undefined;
    this.notification.error(
      knownKey || knownPrefix
        ? this.translate.instant(knownKey || knownPrefix!)
        : raw || this.translate.instant('housingProjects.reports.form.messages.saveFailed'));
    this.cdr.markForCheck();
  }

  // ==================== TRACK BY ====================

  trackByLookupId(index: number, item: LookupDto): number {
    return item.id;
  }

  trackBySlotControl(index: number, item: { control: string }): string {
    return item.control;
  }

  trackByOptionValue(index: number, item: { value: string }): string {
    return item.value;
  }

  get serverErrorsKeys(): string[] {
    return Object.keys(this.serverErrors);
  }
}
