import { Component, OnInit, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { TranslateModule, TranslateService } from '@ngx-translate/core';

import { PeriodicOrphanReportService } from '../services/periodic-orphan-report.service';
import {
  CreatePeriodicOrphanReportDto,
  UpdatePeriodicOrphanReportDto,
  PeriodicOrphanReportDto,
  OrphanLookupDto
} from '../models/periodic-orphan-report.model';
import { LoadingComponent } from '../../../shared/components/loading/loading.component';
import { NotificationService } from '../../../core/services/notification.service';
import { AttachmentService } from '../../../core/services/attachment.service';
import { LookupManagementService } from '../../lookup-management/services/lookup-management.service';
import { LookupDto } from '../../lookup-management/models/lookup.model';
import { AttachmentComponent } from '../../../shared/components/attachment/attachment.component';

import { BreadcrumbComponent } from '../../../shared/components/breadcrumb/breadcrumb.component';
@Component({
  selector: 'app-periodic-report-form',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [
    BreadcrumbComponent,
    AttachmentComponent,

    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    TranslateModule,
    LoadingComponent
  ],
  templateUrl: './periodic-report-form.component.html',
  styleUrls: ['./periodic-report-form.component.scss']
})
export class PeriodicReportFormComponent implements OnInit {
  /** Server messages that carry a translated key on the SPA side (9-5 legacy wording). */
  private static readonly KNOWN_MESSAGES: Record<string, string> = {
    'You can not update old report': 'periodicReports.cannotEditOldReport'
  };

  reportForm: FormGroup;
  loading = false;
  isEditMode = false;
  reportId?: string;
  saving = false;

  // §14.U.5 — locked or accepted reports are immutable; the form refuses up front.
  editBlocked = false;

  // UC-ORR-02 — orphan code lookup that prefaces report entry
  orphanCodeInput = '';
  orphanHeader?: OrphanLookupDto;
  orphanResolved = false;
  lookupLoading = false;

  // §14.S.2 lookups + server-side field errors (15-6 errors-map shape)
  educationLevels: LookupDto[] = [];
  serverErrors: Record<string, string> = {};

  /** The five §14.S.2 attachment slots — control names carry the server attachment id (BR-12). */
  readonly imageSlots: { control: string; labelKey: string; required: boolean }[] = [
    { control: 'orphanImageId', labelKey: 'periodicReports.form.orphanImage', required: true },
    { control: 'orphanCertificateImageId', labelKey: 'periodicReports.form.orphanCertificateImage', required: false },
    { control: 'medicalReportImageId', labelKey: 'periodicReports.form.medicalReportImage', required: false },
    { control: 'orphanDeadImageId', labelKey: 'periodicReports.form.orphanDeadImage', required: false },
    { control: 'orphanMarriageImageId', labelKey: 'periodicReports.form.orphanMarriageImage', required: false }
  ];

  constructor(
    private fb: FormBuilder,
    private route: ActivatedRoute,
    private router: Router,
    private periodicReportService: PeriodicOrphanReportService,
    private notification: NotificationService,
    private translate: TranslateService,
    private attachmentService: AttachmentService,
    private lookupService: LookupManagementService,
    private cdr: ChangeDetectorRef
  ) {
    this.reportForm = this.buildForm();
    this.wireInterlocks();
  }

  ngOnInit(): void {
    this.reportId = this.route.snapshot.params['id'];
    this.isEditMode = !!this.reportId;

    this.loadEducationLevels();

    if (this.isEditMode && this.reportId) {
      this.loadReport(this.reportId);
    }
  }

  /** §14.S.2 — المرحلة الدراسية from the live lookup (no hardcoded arrays). */
  private loadEducationLevels(): void {
    // Review P29 2026-08-24: OnPush — without markForCheck the async fill never
    // renders and the level select looks empty.
    this.lookupService.getEducationLevels().subscribe({
      next: (levels) => {
        this.educationLevels = levels;
        this.cdr.markForCheck();
      },
      error: () => {
        this.educationLevels = [];
        this.cdr.markForCheck();
      }
    });
  }

  /**
   * Legacy MarriedFun/DieFun/IsOrphanStudentFun interlocks: unticking a life-event
   * checkbox clears its date (and the student-funding block) so stale values are
   * never submitted; the paired field's requirement is enforced server-side.
   */
  private wireInterlocks(): void {
    this.reportForm.get('married')?.valueChanges.subscribe((married) => {
      if (!married) {
        this.reportForm.get('marriageDate')?.setValue(null);
      }
    });
    this.reportForm.get('dead')?.valueChanges.subscribe((dead) => {
      if (!dead) {
        this.reportForm.get('deathDate')?.setValue(null);
      }
    });
    this.reportForm.get('isOrphanStudent')?.valueChanges.subscribe((student) => {
      if (!student) {
        ['annualFeeForStudy', 'studyingYears', 'restStudyingYears', 'graduationYear']
          .forEach((c) => this.reportForm.get(c)?.setValue(null));
      }
    });
  }

  /**
   * Upload a §14.S.2 image and keep the returned server id in the matching
   * *ImageId control (BR-12 — reference by id, never embed the file).
   */
  onImageSelected(controlName: string, files: File[]): void {
    const file = files?.[0];
    if (!file) {
      return;
    }
    this.attachmentService.upload(file, 'PeriodicOrphanReport').subscribe({
      next: (response) => {
        this.reportForm.get(controlName)?.setValue(response.id);
        delete this.serverErrors[controlName];
      },
      error: () => this.notification.show(
        this.translate.instant('periodicReports.form.uploadFailed'),
        'error'
      )
    });
  }

  onImageRemoved(controlName: string): void {
    this.reportForm.get(controlName)?.setValue(null);
  }

  /**
   * Look the orphan up by sponsorship code (UC-ORR-02) — pre-fills the read-only
   * report header and stores the orphanId into the create payload. Unknown or
   * other-charity codes clear the header and block the save.
   */
  lookupOrphan(): void {
    const code = this.orphanCodeInput?.trim();
    if (!code || this.lookupLoading) {
      return;
    }

    this.lookupLoading = true;
    this.periodicReportService.getOrphanByCode(code).subscribe({
      next: (dto) => {
        this.lookupLoading = false;
        this.orphanHeader = dto;
        this.orphanResolved = !!dto.orphanId && dto.coded;
        this.reportForm.patchValue({ orphanId: this.orphanResolved ? dto.orphanId : '' });
        // Review P29 2026-08-24: OnPush — the spinner unstick and the resolved
        // header need an explicit markForCheck.
        this.cdr.markForCheck();
      },
      error: (error) => {
        this.lookupLoading = false;
        this.clearOrphanHeader();
        // Review P38 2026-08-24: only a 404 means "no such orphan in scope" —
        // 403/500/network used to masquerade as the unknown-code warning.
        if (error?.status === 404) {
          this.notification.show(
            this.translate.instant('periodicReports.lookup.unknownOrphan'),
            'warning'
          );
        } else {
          this.notification.show(
            this.translate.instant('periodicReports.lookup.lookupFailed'),
            'error'
          );
        }
        this.cdr.markForCheck();
      }
    });
  }

  /** Review P60a 2026-08-24: editing the code text without re-looking-up kept the
   *  previous orphan's resolution (and its orphanId) — the save would silently
   *  file against the previously resolved orphan. Drop the stale resolution. */
  onCodeInputChanged(): void {
    if (this.orphanHeader && this.orphanCodeInput?.trim() !== this.orphanHeader.code) {
      this.clearOrphanHeader();
    }
  }

  clearOrphanHeader(): void {
    this.orphanHeader = undefined;
    this.orphanResolved = false;
    this.reportForm.patchValue({ orphanId: '' });
  }

  /**
   * Build the form with all sections from UC-6.11
   */
  private buildForm(): FormGroup {
    return this.fb.group({
      // Basic Information
      orphanId: ['', Validators.required],
      orphanPaymentId: [''],
      // Review P47b 2026-08-24: toISOString() is UTC — between 00:00 and 03:00 in
      // Riyadh the "default today" silently became yesterday. Local date parts
      // instead (en-CA yields the YYYY-MM-DD shape the date input expects).
      reportDate: [new Date().toLocaleDateString('en-CA'), Validators.required],
      reportPeriodFrom: [''],
      reportPeriodTo: [''],
      reportNo: [''],

      // Religious & Behavioral (UC-6.11)
      prayerStatus: [''],
      mannersStatus: [''],
      hadeethStatus: [''],

      // Quran Education (UC-6.11)
      quranParts: [''],
      quranVerses: [''],

      // Health & Medical (UC-6.11)
      medicalStatus: [''],
      disease: [''],
      disability: [''],
      disabilityDescription: [''],
      diseaseDescription: [''],
      medicalReportImageId: [''],

      // Personal Development (UC-6.11)
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

      // Education Details (UC-6.11)
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

      // Life Events (UC-6.11)
      married: [false],
      marriageDate: [''],
      orphanMarriageImageId: [''],
      dead: [false],
      deathDate: [''],
      orphanDeadImageId: [''],

      // Attachments (UC-6.11)
      orphanCertificateImageId: [''],
      orphanImageId: [''],
      missingDocuments: [false],
      missingDocumentsName: ['']
    });
  }

  /**
   * Load existing report for editing - UC-6.11
   */
  loadReport(id: string): void {
    this.loading = true;
    this.cdr.markForCheck();
    this.periodicReportService.getReport(id).subscribe({
      next: (report: PeriodicOrphanReportDto) => {
        this.patchForm(report);
        this.loading = false;
        this.checkEditable(report);
      },
      error: (error) => {
        console.error('Error loading report:', error);
        // Review P37b 2026-08-24: a failed load used to leave a silent blank form —
        // tell the user and go back to the register (search-screen precedent).
        this.loading = false;
        this.notification.show(
          (typeof error === 'string' ? error : error?.message)
            || this.translate.instant('periodicReports.loadFailed'),
          'error'
        );
        this.router.navigate(['/periodic-orphan-reports']);
        this.cdr.markForCheck();
      }
    });
  }

  /**
   * §14.U.5 — ask the server whether this report may still be edited
   * (`!locked && !accepted`); a refused report reopens for resubmission.
   */
  private checkEditable(report: PeriodicOrphanReportDto): void {
    this.periodicReportService.canEditReport(report.id).subscribe({
      next: (result) => {
        this.editBlocked = !result.canEdit;
        this.cdr.markForCheck();
      },
      error: () => {
        // Review P60c 2026-08-24: fail CLOSED — an unreadable edit gate used to
        // fail open, offering Save on a report that may be locked or accepted.
        this.editBlocked = true;
        this.cdr.markForCheck();
      }
    });
  }

  /**
   * Patch form with existing data
   */
  private patchForm(report: PeriodicOrphanReportDto): void {
    // Header stays read-only in edit mode — pre-filled from the stored report
    this.orphanHeader = {
      orphanId: report.orphanId,
      code: report.orphanCode,
      fullName: report.orphanName,
      charityId: report.charityId,
      charityName: report.charityName,
      totalReports: 0,
      pendingReports: 0,
      coded: true
    };
    this.orphanResolved = true;
    this.orphanCodeInput = report.orphanCode ?? '';

    this.reportForm.patchValue({
      orphanId: report.orphanId,
      orphanPaymentId: report.orphanPaymentId,
      reportDate: report.reportDate.split('T')[0],
      reportPeriodFrom: report.reportPeriodFrom?.split('T')[0],
      reportPeriodTo: report.reportPeriodTo?.split('T')[0],
      reportNo: report.reportNo,
      prayerStatus: report.prayerStatus,
      mannersStatus: report.mannersStatus,
      hadeethStatus: report.hadeethStatus,
      quranParts: report.quranParts,
      quranVerses: report.quranVerses,
      medicalStatus: report.medicalStatus,
      disease: report.disease,
      disability: report.disability,
      disabilityDescription: report.disabilityDescription,
      diseaseDescription: report.diseaseDescription,
      medicalReportImageId: report.medicalReportImageId,
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
      orphanMarriageImageId: report.orphanMarriageImageId,
      dead: report.dead,
      deathDate: report.deathDate?.split('T')[0],
      orphanDeadImageId: report.orphanDeadImageId,
      orphanCertificateImageId: report.orphanCertificateImageId,
      orphanImageId: report.orphanImageId,
      missingDocuments: report.missingDocuments,
      missingDocumentsName: report.missingDocumentsName
    });
  }

  /**
   * Save report - UC-6.11
   */
  save(): void {
    if (this.reportForm.invalid) {
      this.reportForm.markAllAsTouched();
      return;
    }

    // §14.U.5 — refuse locally too; the server remains the authority
    if (this.isEditMode && this.editBlocked) {
      this.notification.show(
        this.translate.instant('periodicReports.cannotEditOldReport'),
        'warning'
      );
      return;
    }

    // UC-ORR-02 alternate: no report can be created against an unresolvable code
    if (!this.isEditMode && !this.orphanResolved) {
      this.notification.show(
        this.translate.instant('periodicReports.lookup.unknownOrphan'),
        'warning'
      );
      return;
    }

    this.saving = true;
    this.serverErrors = {};

    if (this.isEditMode && this.reportId) {
      const updateDto: UpdatePeriodicOrphanReportDto = {
        ...this.reportForm.value,
        id: this.reportId
      };

      this.periodicReportService.updateReport(this.reportId, updateDto).subscribe({
        next: () => {
          this.saving = false;
          this.notification.show(
            this.translate.instant('periodicReports.form.savedSuccessfully'),
            'success'
          );
          this.router.navigate(['../'], { relativeTo: this.route });
        },
        error: (error) => {
          this.saving = false;
          this.handleSaveError(error);
        }
      });
    } else {
      const createDto: CreatePeriodicOrphanReportDto = this.reportForm.value;

      this.periodicReportService.createReport(createDto).subscribe({
        next: () => {
          this.saving = false;
          this.notification.show(
            this.translate.instant('periodicReports.form.savedSuccessfully'),
            'success'
          );
          this.router.navigate(['../'], { relativeTo: this.route });
        },
        error: (error) => {
          this.saving = false;
          this.handleSaveError(error);
        }
      });
    }
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
    const knownKey = raw ? PeriodicReportFormComponent.KNOWN_MESSAGES[raw] : undefined;
    this.notification.show(
      knownKey
        ? this.translate.instant(knownKey)
        : raw || this.translate.instant('periodicReports.form.saveFailed'),
      'error'
    );
  }

  /**
   * Cancel and go back
   */
  cancel(): void {
    this.router.navigate(['../'], { relativeTo: this.route });
  }

  /**
   * Get form section title
   */
  getSectionTitle(section: string): string {
    return `periodicReports.form.sections.${section}`;
  }

  trackByLookupId(index: number, item: LookupDto): number {
    return item.id;
  }

  trackBySlotControl(index: number, item: { control: string }): string {
    return item.control;
  }

  get serverErrorsKeys(): string[] {
    return Object.keys(this.serverErrors);
  }
}
